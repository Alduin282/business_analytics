using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Analytics.Handlers;
using BusinessAnalytics.API.Services.Analytics.Strategies;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.API.Services.Analytics;

public class AnalyticsService(IUnitOfWork unitOfWork) : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private const int MaxYearsLimit = 5;
    private const string DefaultTimeZone = "UTC";

    public async Task<List<AnalyticsPoint>> GetAnalyticsAsync(
        string userId,
        GroupPeriod groupBy = GroupPeriod.Month,
        MetricType metric = MetricType.TotalAmount,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? timeZoneId = null)
    {
        var tz = TryResolveTimeZone(timeZoneId ?? DefaultTimeZone);

        var range = DateRange.Create(startDate, endDate, tz, MaxYearsLimit);

        var periodHandler = GetPeriodHandler(groupBy);
        var analyticsStrategy = GetAnalyticsStrategy(metric);

        var rawOrders = await FetchOrdersFromDb(userId, range, tz);
        var result = BuildAnalytics(rawOrders, range, periodHandler, analyticsStrategy, tz);

        return result;
    }

    private static TimeZoneInfo TryResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private async Task<List<Order>> FetchOrdersFromDb(string userId, DateRange range, TimeZoneInfo tz)
    {
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(range.Start, DateTimeKind.Unspecified), tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(range.End, DateTimeKind.Unspecified), tz);

        return await _unitOfWork.Repository<Order, Guid>()
            .Query()
            .Include(o => o.ImportSession)
            .Where(o => o.UserId == userId &&
                        o.OrderDate >= startUtc &&
                        o.OrderDate < endUtc &&
                        o.Status != OrderStatus.Cancelled &&
                        (o.ImportSession == null || !o.ImportSession.IsRolledBack))
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
}
