namespace BusinessAnalytics.API.Models.DTOs;

public class ImportSessionDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; }
    public int OrdersCount { get; set; }
    public int ItemsCount { get; set; }
}
