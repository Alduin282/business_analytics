using Microsoft.EntityFrameworkCore;
using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using FluentAssertions;
using Xunit;

namespace OrderAnalytics.Tests;

public class DbSeederTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private const string TargetUserId = "user-1";
    private const string OtherUserId = "user-2";

    public DbSeederTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Seed_ShouldOnlyAffectTargetUser()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context);
        
        // Add data for another user
        context.Customers.Add(new Customer { Id = Guid.NewGuid(), UserId = OtherUserId, FullName = "Existing" });
        await context.SaveChangesAsync();

        // Act
        await seeder.SeedAsync(TargetUserId);

        // Assert
        context.Customers.Count(c => c.UserId == OtherUserId).Should().Be(1);
        context.Customers.Count(c => c.UserId == TargetUserId).Should().Be(DbSeeder.TotalCustomersToGenerate);
    }

    [Fact]
    public async Task Seed_ShouldCleanupPreviousRun()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context);

        // Act - Run twice
        await seeder.SeedAsync(TargetUserId);
        await seeder.SeedAsync(TargetUserId);

        // Assert
        context.Customers.Count(c => c.UserId == TargetUserId).Should().Be(DbSeeder.TotalCustomersToGenerate);
        context.Orders.Count(o => o.UserId == TargetUserId).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Seed_ShouldMaintainPriceIntegrity()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context);

        // Act
        await seeder.SeedAsync(TargetUserId);
        var orders = await context.Orders.Include(o => o.Items).ToListAsync();

        // Assert
        foreach (var order in orders)
        {
            decimal itemsTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
            order.TotalAmount.Should().Be(itemsTotal, $"Order {order.Id} total must match sum of items");
        }
    }

    [Fact]
    public async Task Seed_ShouldApplyGrowthTrend_Deterministically()
    {
        // Arrange
        var seed = 42;
        var deterministicRandom = new Random(seed);
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context, deterministicRandom);

        // Act
        await seeder.SeedAsync(TargetUserId);
        
        var orders = await context.Orders.OrderBy(o => o.OrderDate).ToListAsync();
        
        // Split into first half and second half of the year
        var middleDate = orders.First().OrderDate.AddDays(DbSeeder.DaysToHistory / 2);
        var firstHalfRevenue = orders.Where(o => o.OrderDate < middleDate).Sum(o => o.TotalAmount);
        var secondHalfRevenue = orders.Where(o => o.OrderDate >= middleDate).Sum(o => o.TotalAmount);

        // Assert
        // With GrowthTrendMultiplier = 1.0, second half MUST be higher than first half
        secondHalfRevenue.Should().BeGreaterThan(firstHalfRevenue);
    }

    [Fact]
    public async Task Seed_ShouldHaveWeekendSpikes_Deterministically()
    {
        // Arrange
        var seed = 42;
        var deterministicRandom = new Random(seed);
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context, deterministicRandom);

        // Act
        await seeder.SeedAsync(TargetUserId);
        
        var orders = await context.Orders.ToListAsync();
        
        var weekendOrdersCount = orders.Count(o => o.OrderDate.DayOfWeek == DayOfWeek.Saturday || o.OrderDate.DayOfWeek == DayOfWeek.Sunday);
        var weekdayOrdersCount = orders.Count(o => o.OrderDate.DayOfWeek != DayOfWeek.Saturday && o.OrderDate.DayOfWeek != DayOfWeek.Sunday);

        // Calculate average orders per day for comparison
        const double WeeksInYear = 52.0;
        const double WeekendDaysPerWeek = 2.0;
        const double WeekdaysPerWeek = 5.0;

        double avgWeekend = weekendOrdersCount / (WeeksInYear * WeekendDaysPerWeek);
        double avgWeekday = weekdayOrdersCount / (WeeksInYear * WeekdaysPerWeek);

        // Assert
        // Weekend factor is 1.5, so it should be significantly higher
        avgWeekend.Should().BeGreaterThan(avgWeekday);
    }

    [Fact]
    public async Task Seed_TotalCustomers_ShouldMatchConstant()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var seeder = new DbSeeder(context);

        // Act
        await seeder.SeedAsync(TargetUserId);

        // Assert
        context.Customers.Count().Should().Be(DbSeeder.TotalCustomersToGenerate);
    }
}
