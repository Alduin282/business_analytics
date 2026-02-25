using System.Security.Claims;
using BusinessAnalytics.API.Controllers;
using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.Tests;

public class OrdersControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly OrdersController _controller;
    private const string TestUserId = "user-123";

    private const decimal DefaultTotalAmount = 100;

    private readonly OrderTestBuilder _orderBuilder;

    private readonly DateTime _defaultStartDate = new DateTime(2023, 1, 1);

    public OrdersControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_context);
        _controller = new OrdersController(_uow);
        _orderBuilder = new OrderTestBuilder(TestUserId)
            .WithAmount(DefaultTotalAmount)
            .WithDate(_defaultStartDate);
        
        SetupUser(TestUserId, "UTC");
    }

    private void SetupUser(string userId, string timeZoneId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("TimeZoneId", timeZoneId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAnalytics_OutOfRange_Ignored()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        _context.Orders.AddRange(
            _orderBuilder.WithDate(new DateTime(2023, 1, 2)).WithAmount(300).Build() // Out of range
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        data.Should().HaveCount(1);
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(0);
    }

    [Fact]
    public async Task GetAnalytics_NoData_DateRangeFilled()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        data.Should().HaveCount(1);
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(0); // Gap filling
    }

    [Fact]
    public async Task GetAnalytics_DayGrouping_CorrectGroupedByDay()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);

        _context.Orders.AddRange(
            _orderBuilder.WithDate(new DateTime(2023, 1, 1)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 1, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 2)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 2, 23, 59, 59, 999)).WithAmount(300).Build()
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        data.Should().HaveCount(2);
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(150);
        data.First(d => d.Label == "2023-01-02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalytics_MonthGrouping_CorrectGroupByMonth()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 2, 28);

        _context.Orders.AddRange(
            _orderBuilder.WithDate(new DateTime(2026, 1, 1)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 1, 31, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 1)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 28, 23, 59, 59, 999)).WithAmount(300).Build()
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        data.Should().HaveCount(2);
        data.First(d => d.Label == "2026-01").Value.Should().Be(150);
        data.First(d => d.Label == "2026-02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalytics_WeekGrouping_CorrectGroupByWeek()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 2); // 2 - 8 Feb week
        var endDate = new DateTime(2026, 2, 15); // 9 - 15 Feb week

        _context.Orders.AddRange(
            _orderBuilder.WithDate(new DateTime(2026, 2, 2)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 8, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 9)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 15, 23, 59, 59, 999)).WithAmount(300).Build()
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Week, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        data.Should().HaveCount(2);
        data.First(d => d.Label == "02.02 - 08.02").Value.Should().Be(150);
        data.First(d => d.Label == "09.02 - 15.02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalytics_CancelledOrders_CancelledExcluded()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);
        var dateInRange = new DateTime(2023, 1, 10);
        
        _context.Orders.AddRange(
            _orderBuilder.WithDate(dateInRange).WithStatus(OrderStatus.Delivered).Build(),
            _orderBuilder.WithDate(dateInRange).WithStatus(OrderStatus.Cancelled).Build()
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;
        data.First(d => d.Label == "2023-01").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalytics_UserTimeZone_UpperBoundaryCorrectness()
    {
        // Arrange
        // Order is at 23:30 UTC on Jan 1st. 
        // In UTC+3 (Moscow), it's 02:30 UTC on Jan 2nd.
        SetupUser(TestUserId, "Russian Standard Time"); 
        
        var orderDateUtc = new DateTime(2023, 1, 1, 23, 30, 0, DateTimeKind.Utc);
        _context.Orders.Add(_orderBuilder.WithDate(orderDateUtc).Build());
        await _context.SaveChangesAsync();

        // Act
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: new DateTime(2023,1,1), endDate: new DateTime(2023,1,2));

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;
        
        // In Moscow time, Jan 1st should be 0, Jan 2nd should be 100
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(0);
        data.First(d => d.Label == "2023-01-02").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalytics_UserTimeZone_LowerBoundaryCorrectness()
    {
        // Arrange
        // Moscow Jan 1st 00:00 is Dec 31st 21:00 UTC.
        // So an order at Dec 31st 22:00 UTC SHOULD be included in Jan 1st analytics.
        SetupUser(TestUserId, "Russian Standard Time");

        var orderDateUtc = new DateTime(2022, 12, 31, 22, 0, 0, DateTimeKind.Utc);
        _context.Orders.Add(_orderBuilder.WithDate(orderDateUtc).Build());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: new DateTime(2023, 1, 1), endDate: new DateTime(2023, 1, 1));

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;

        // The order from Dec 31st UTC should be counted for Jan 1st Local
        data.Should().HaveCount(1);
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalytics_InvalidTimeZone_UseDefaultTimeZone()
    {
        // Arrange
        SetupUser(TestUserId, "SomeInvalidTimeZone"); 

        var orderDateUtcLeftBoundary = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var orderDateUtcRightBoundary = new DateTime(2023, 1, 1, 23, 59, 59, DateTimeKind.Utc);
        _context.Orders.AddRange(
            _orderBuilder.WithDate(orderDateUtcRightBoundary).Build(),
            _orderBuilder.WithDate(orderDateUtcLeftBoundary).Build()
            );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: _defaultStartDate, endDate: _defaultStartDate);
    
        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;
        
        // Если используется UTC (дефолт), заказ останется 1-го числа.
        data.First(d => d.Label == "2023-01-01").Value.Should().Be(DefaultTotalAmount * 2);
    }

    [Fact]
    public async Task GetAnalytics_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2023, 2, 1);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: startDate, endDate: endDate);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Start date cannot be later than end date.");
    }

    [Fact]
    public async Task GetAnalytics_ExceedsMaxYears_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2020, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // > 5 years

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Date range cannot exceed 5 years.");
    }

    [Fact]
    public async Task GetAnalytics_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: _defaultStartDate, endDate: _defaultStartDate);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetAnalytics_TwoDifferentUsers_AggregatedByUserCorrectly()
    {
        // Arrange
        var otherUser = "other-test-user";
        _context.Orders.AddRange(
            _orderBuilder.WithUser(TestUserId).Build(),
            _orderBuilder.WithUser(otherUser).Build()
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: _defaultStartDate, endDate: _defaultStartDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;
        data.First(d => d.Label == "2023-01").Value.Should().Be(DefaultTotalAmount);
    }

    private void SetupAnonymousUser()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_DayGrouping_AlwaysFalse()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1, 0, 0, 0);
        var endDate = new DateTime(2023, 1, 1, 0, 0, 1);

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Day, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().AllSatisfy(p => p.IsPartial.Should().BeFalse());
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_MonthGrouping_PartialAtStart()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 15); // Mid-month
        var endDate = new DateTime(2023, 1, 31);

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_MonthGrouping_PartialAtEnd()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 15); // End mid-month

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_MonthGrouping_FullMonth()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Month, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeFalse();
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_WeekGrouping_PartialAtStart()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 3); // Tuesday (Feb 2 is Monday)
        var endDate = new DateTime(2026, 2, 8); // Sunday

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Week, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_WeekGrouping_PartialAtEnd()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 2); // Monday
        var endDate = new DateTime(2026, 2, 6); // Friday

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Week, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalytics_IsPartial_WeekGrouping_FullWeek()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 2); // Monday
        var endDate = new DateTime(2026, 2, 8); // Sunday

        // Act
        var result = await _controller.GetAnalytics(groupBy: GroupPeriod.Week, startDate: startDate, endDate: endDate);

        // Assert
        var data = ((OkObjectResult)result).Value as List<AnalyticsPoint>;
        data.Should().HaveCount(1);
        data[0].IsPartial.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
