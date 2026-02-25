namespace BusinessAnalytics.API.Models.DTOs;

public class OrderImportRow
{
    public int RowNumber { get; set; }
    public string OrderDate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string UnitPrice { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
