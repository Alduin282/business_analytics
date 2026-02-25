using System.ComponentModel.DataAnnotations;

namespace BusinessAnalytics.API.Models;

public class Order
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public decimal TotalAmount { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public List<OrderItem> Items { get; set; } = new();
    
    public Guid? ImportSessionId { get; set; }
    public ImportSession? ImportSession { get; set; }
}
