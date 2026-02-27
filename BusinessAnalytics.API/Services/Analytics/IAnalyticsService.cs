using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Analytics;

public interface IAnalyticsService
{
    Task<List<AnalyticsPoint>> GetAnalyticsAsync(
        string userId,
        GroupPeriod groupBy = GroupPeriod.Month,
        MetricType metric = MetricType.TotalAmount,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? timeZoneId = null);
}
