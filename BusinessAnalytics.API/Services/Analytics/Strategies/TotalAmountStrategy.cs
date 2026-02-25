using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics.Strategies;

public class TotalAmountStrategy : IAnalyticsStrategy
{
    public string Key => "total_amount";

    public decimal CalculateValue(IEnumerable<Order> orders)
    {
        return orders.Sum(o => o.TotalAmount);
    }
}
