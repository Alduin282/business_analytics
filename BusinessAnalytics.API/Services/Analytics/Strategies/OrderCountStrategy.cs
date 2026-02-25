using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics.Strategies;

public class OrderCountStrategy : IAnalyticsStrategy
{
    public string Key => "order_count";

    public decimal CalculateValue(IEnumerable<Order> orders)
    {
        return orders.Count();
    }
}
