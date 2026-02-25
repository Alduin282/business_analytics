using BusinessAnalytics.API.Services.Import.Validation;

namespace BusinessAnalytics.API.Models.DTOs;

public class ImportResult
{
    public bool Success { get; set; }
    public int OrdersCount { get; set; }
    public int ItemsCount { get; set; }
    public Guid? ImportSessionId { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}
