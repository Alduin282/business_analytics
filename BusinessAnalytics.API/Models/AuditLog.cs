using System.ComponentModel.DataAnnotations;

namespace BusinessAnalytics.API.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public ImportAction Action { get; set; }
    
    [MaxLength(255)]
    public string Message { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? RelatedId { get; set; } // e.g., ImportSessionId
}
