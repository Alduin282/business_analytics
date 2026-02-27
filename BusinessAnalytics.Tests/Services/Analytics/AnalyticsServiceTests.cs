using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Analytics;
using BusinessAnalytics.Tests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.Tests.Services.Analytics;

public class AnalyticsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AnalyticsService _service;
    private readonly OrderTestBuilder _orderBuilder;
    private const string TestUserId = "user-123";
    private const decimal DefaultTotalAmount = 100;

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _service = new AnalyticsService(_unitOfWork);
        _orderBuilder = new OrderTestBuilder(TestUserId)
            .WithAmount(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalyticsAsync_OrderCountMetric_ReturnsCorrectCount()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2023, 1, 1)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 1)).WithAmount(200).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.OrderCount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(2); // 2 orders
    }

    [Fact]
    public async Task GetAnalyticsAsync_OutOfRange_Ignored()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2023, 1, 2)).WithAmount(300).Build() // Out of range
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(0);
    }

    [Fact]
    public async Task GetAnalyticsAsync_NoData_DateRangeFilled()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(0); // Gap filling
    }

    [Fact]
    public async Task GetAnalyticsAsync_DayGrouping_CorrectGroupedByDay()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 2);

        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2023, 1, 1)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 1, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 2)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 2, 23, 59, 59, 999)).WithAmount(300).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(2);
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(150);
        result.First(d => d.Label == "2023-01-02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalyticsAsync_MonthGrouping_CorrectGroupByMonth()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 2, 28);

        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2026, 1, 1)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 1, 31, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 1)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 28, 23, 59, 59, 999)).WithAmount(300).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(2);
        result.First(d => d.Label == "2026-01").Value.Should().Be(150);
        result.First(d => d.Label == "2026-02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalyticsAsync_WeekGrouping_CorrectGroupByWeek()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 2); // 2 - 8 Feb week
        var endDate = new DateTime(2026, 2, 15); // 9 - 15 Feb week

        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2026, 2, 2)).WithAmount(100).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 8, 23, 59, 59, 999)).WithAmount(50).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 9)).WithAmount(200).Build(),
            _orderBuilder.WithDate(new DateTime(2026, 2, 15, 23, 59, 59, 999)).WithAmount(300).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Week, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(2);
        result.First(d => d.Label == "02.02 - 08.02").Value.Should().Be(150);
        result.First(d => d.Label == "09.02 - 15.02").Value.Should().Be(500);
    }

    [Fact]
    public async Task GetAnalyticsAsync_CancelledOrders_CancelledExcluded()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);
        var dateInRange = new DateTime(2023, 1, 10);
        
        var orders = new List<Order>
        {
            _orderBuilder.WithDate(dateInRange).WithStatus(OrderStatus.Delivered).Build(),
            _orderBuilder.WithDate(dateInRange).WithStatus(OrderStatus.Cancelled).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.First(d => d.Label == "2023-01").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalyticsAsync_UserTimeZone_UpperBoundaryCorrectness()
    {
        // Arrange
        // Order is at 23:30 UTC on Jan 1st. 
        // In UTC+3 (Moscow), it's 02:30 UTC on Jan 2nd.
        var timeZoneId = "Russian Standard Time";
        var orderDateUtc = new DateTime(2023, 1, 1, 23, 30, 0, DateTimeKind.Utc);
        
        var orders = new List<Order>
        {
            _orderBuilder.WithDate(orderDateUtc).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            new DateTime(2023, 1, 1), 
            new DateTime(2023, 1, 2), 
            timeZoneId);

        // Assert
        // In Moscow time, Jan 1st should be 0, Jan 2nd should be 100
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(0);
        result.First(d => d.Label == "2023-01-02").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalyticsAsync_UserTimeZone_LowerBoundaryCorrectness()
    {
        // Arrange
        // Moscow Jan 1st 00:00 is Dec 31st 21:00 UTC.
        // So an order at Dec 31st 22:00 UTC SHOULD be included in Jan 1st analytics.
        var timeZoneId = "Russian Standard Time";
        var orderDateUtc = new DateTime(2022, 12, 31, 22, 0, 0, DateTimeKind.Utc);
        
        var orders = new List<Order>
        {
            _orderBuilder.WithDate(orderDateUtc).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            new DateTime(2023, 1, 1), 
            new DateTime(2023, 1, 1), 
            timeZoneId);

        // Assert
        // The order from Dec 31st UTC should be counted for Jan 1st Local
        result.Should().HaveCount(1);
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalyticsAsync_InvalidTimeZone_UseDefaultTimeZone()
    {
        // Arrange
        var timeZoneId = "SomeInvalidTimeZone";
        var orderDateUtcLeftBoundary = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var orderDateUtcRightBoundary = new DateTime(2023, 1, 1, 23, 59, 59, DateTimeKind.Utc);
        
        var orders = new List<Order>
        {
            _orderBuilder.WithDate(orderDateUtcRightBoundary).Build(),
            _orderBuilder.WithDate(orderDateUtcLeftBoundary).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            new DateTime(2023, 1, 1), 
            new DateTime(2023, 1, 1), 
            timeZoneId);
    
        // Assert
        // Если используется UTC (дефолт), заказ останется 1-го числа.
        result.First(d => d.Label == "2023-01-01").Value.Should().Be(DefaultTotalAmount * 2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_InvalidDateRange_ThrowsArgumentException()
    {
        // Arrange
        var startDate = new DateTime(2023, 2, 1);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        var action = () => _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Day, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Start date cannot be later than end date.");
    }

    [Fact]
    public async Task GetAnalyticsAsync_ExceedsMaxYears_ThrowsArgumentException()
    {
        // Arrange
        var startDate = new DateTime(2020, 1, 1);
        var endDate = new DateTime(2026, 1, 2); // > 5 years

        // Act
        var action = () => _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Date range cannot exceed 5 years.");
    }

    [Fact]
    public async Task GetAnalyticsAsync_TwoDifferentUsers_AggregatedByUserCorrectly()
    {
        // Arrange
        var otherUser = "other-test-user";
        var orders = new List<Order>
        {
            _orderBuilder.WithUser(TestUserId).WithDate(new DateTime(2023, 1, 1)).Build(),
            _orderBuilder.WithUser(otherUser).WithDate(new DateTime(2023, 1, 1)).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            new DateTime(2023, 1, 1), 
            new DateTime(2023, 1, 31));

        // Assert
        result.First(d => d.Label == "2023-01").Value.Should().Be(DefaultTotalAmount);
    }

    [Fact]
    public async Task GetAnalyticsAsync_RolledBackSession_Excluded()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);
        
        var rolledBackSession = new ImportSession
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            FileName = "rolled-back.csv",
            IsRolledBack = true,
            FileHash = "hash-1"
        };
        
        var activeSession = new ImportSession
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            FileName = "active.csv",
            IsRolledBack = false,
            FileHash = "hash-2"
        };

        _context.ImportSessions.AddRange(rolledBackSession, activeSession);
        
        var orders = new List<Order>
        {
            _orderBuilder.WithDate(new DateTime(2023, 1, 10)).WithAmount(100).WithSession(rolledBackSession.Id).Build(),
            _orderBuilder.WithDate(new DateTime(2023, 1, 11)).WithAmount(200).WithSession(activeSession.Id).Build()
        };
        await SaveOrders(orders);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        // Only the order from the active session (200) should be counted
        result.First(d => d.Label == "2023-01").Value.Should().Be(200);
    }

    [Fact]
    public async Task GetAnalyticsAsync_IsPartial_MonthGrouping_PartialAtStart()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 15); // Mid-month
        var endDate = new DateTime(2023, 1, 31);

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Month, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsPartial.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalyticsAsync_IsPartial_WeekGrouping_PartialAtStart()
    {
        // Arrange
        var startDate = new DateTime(2026, 2, 3); // Tuesday (Feb 2 is Monday)
        var endDate = new DateTime(2026, 2, 8); // Sunday

        // Act
        var result = await _service.GetAnalyticsAsync(
            TestUserId, 
            GroupPeriod.Week, 
            MetricType.TotalAmount, 
            startDate, 
            endDate);

        // Assert
        result.Should().HaveCount(1);
        result[0].IsPartial.Should().BeTrue();
    }

    private async Task SaveOrders(IEnumerable<Order> orders)
    {
        foreach (var order in orders)
        {
            await _unitOfWork.Repository<Order, Guid>().AddAsync(order);
        }
        await _unitOfWork.CompleteAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        _unitOfWork.Dispose();
    }
}
