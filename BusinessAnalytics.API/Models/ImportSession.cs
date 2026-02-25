using System.ComponentModel.DataAnnotations;

namespace BusinessAnalytics.API.Models;

public class ImportSession
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    
    public int OrdersCount { get; set; }
    
    public int ItemsCount { get; set; }
    
    public List<Order> Orders { get; set; } = new();
}
