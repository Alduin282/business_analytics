using System.ComponentModel.DataAnnotations;

namespace BusinessAnalytics.API.Models;

public class Customer
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
