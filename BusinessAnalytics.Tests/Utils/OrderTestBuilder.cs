using System.Data.Common;
using BusinessAnalytics.API.Models;

public class OrderTestBuilder
{
    string _userId = "test-user-id";
    DateTime _oderDate = DateTime.UtcNow;
    decimal _totalAmount = 100;
    OrderStatus _status = OrderStatus.Delivered;

    public OrderTestBuilder(string userId)
    {
        _userId = userId;
    }

    public OrderTestBuilder WithDate(DateTime date)
    {
        _oderDate = date;
        return this;
    }

    public OrderTestBuilder WithAmount(decimal amount)
    {
        _totalAmount = amount;
        return this;
    }

    public OrderTestBuilder WithUser(string userId)
    {
        _userId = userId;
        return this;
    }

    public OrderTestBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    public Order Build() => new()
    {
        Id = Guid.NewGuid(),
        OrderDate = _oderDate,
        TotalAmount = _totalAmount,
        UserId = _userId,
        Status = _status
    };
}