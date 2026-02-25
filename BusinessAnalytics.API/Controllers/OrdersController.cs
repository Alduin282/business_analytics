using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Analytics;
using BusinessAnalytics.API.Services.Analytics.Handlers;
using BusinessAnalytics.API.Services.Analytics.Strategies;

namespace BusinessAnalytics.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private const int maxYearsLimit = 5;
    private const string DefaultTimeZone = "UTC";

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(List<AnalyticsPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] GroupPeriod groupBy = GroupPeriod.Month,
        [FromQuery] MetricType metric = MetricType.TotalAmount,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var timeZoneId = User.FindFirstValue("TimeZoneId") ?? DefaultTimeZone;
        if (!TryResolveTimeZone(timeZoneId, out var tz))
            tz = TimeZoneInfo.Utc;

        try
        {
            var range = DateRange.Create(startDate, endDate, tz, maxYearsLimit);
            var periodHandler = GetPeriodHandler(groupBy);
            var analyticsStrategy = GetAnalyticsStrategy(metric);

            var rawOrders = await FetchOrdersFromDb(userId, range, tz);
            var result = BuildAnalytics(rawOrders, range, periodHandler, analyticsStrategy, tz);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static bool TryResolveTimeZone(string timeZoneId, out TimeZoneInfo tz)
    {
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.Utc;
            return false;
        }
    }
    
    private IPeriodHandler GetPeriodHandler(GroupPeriod groupBy) => groupBy switch
    {
        GroupPeriod.Day => new DayPeriodHandler(),
        GroupPeriod.Week => new WeekPeriodHandler(),
        GroupPeriod.Month => new MonthPeriodHandler(),
        _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
    };

    private IAnalyticsStrategy GetAnalyticsStrategy(MetricType metric) => metric switch
    {
        MetricType.TotalAmount => new TotalAmountStrategy(),
        MetricType.OrderCount => new OrderCountStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(metric))
    };

    private async Task<List<Order>> FetchOrdersFromDb(string userId, DateRange range, TimeZoneInfo tz)
    {
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(range.Start, DateTimeKind.Unspecified), tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(range.End, DateTimeKind.Unspecified), tz);

        return await _unitOfWork.Repository<Order, Guid>()
            .Query()
            .Where(o => o.UserId == userId &&
                        o.OrderDate >= startUtc &&
                        o.OrderDate < endUtc &&
                        o.Status != OrderStatus.Cancelled)
            .ToListAsync();
    }

    private static List<AnalyticsPoint> BuildAnalytics(
        List<Order> orders,
        DateRange range,
        IPeriodHandler periodHandler,
        IAnalyticsStrategy strategy,
        TimeZoneInfo tz)
    {
        var result = new List<AnalyticsPoint>();
        var currentLocal = periodHandler.AlignToStart(range.Start);

        while (currentLocal < range.End)
        {
            var periodEnd = periodHandler.GetNext(currentLocal);
            var label = periodHandler.GetLabel(currentLocal);
            
            var ordersInPeriod = orders.Where(o => 
            {
                var localOrderDate = TimeZoneInfo.ConvertTimeFromUtc(o.OrderDate, tz);
                return localOrderDate >= currentLocal && localOrderDate < periodEnd;
            });

            bool isPartial = periodHandler.IsPartial(currentLocal, range);
            result.Add(new AnalyticsPoint(
                Label: label,
                Value: strategy.CalculateValue(ordersInPeriod),
                IsPartial: isPartial));

            currentLocal = periodEnd;
        }

        return result;
    }
}

