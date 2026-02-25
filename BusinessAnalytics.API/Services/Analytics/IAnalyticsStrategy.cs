using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics;

public interface IAnalyticsStrategy
{
    string Key { get; }
    decimal CalculateValue(IEnumerable<Order> orders);
}
