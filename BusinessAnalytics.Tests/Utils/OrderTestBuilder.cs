using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.Tests.Utils;
public class OrderTestBuilder
{
    string _userId = "test-user-id";
    DateTime _oderDate = DateTime.UtcNow;
    decimal _totalAmount = 100;
    OrderStatus _status = OrderStatus.Delivered;

    Guid? _importSessionId = null;

    public OrderTestBuilder(string userId)
    {
        _userId = userId;
    }

    public OrderTestBuilder WithSession(Guid? sessionId)
    {
        _importSessionId = sessionId;
        return this;
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
        Status = _status,
        ImportSessionId = _importSessionId
    };
}