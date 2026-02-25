using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Validation;

namespace BusinessAnalytics.API.Services.Import.Pipeline;

public class ImportContext
{
    // Input
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    // After Parse stage
    public List<OrderImportRow> ParsedRows { get; set; } = new();
    public string[] Headers { get; set; } = Array.Empty<string>();
    
    // After Validation stage
    public List<ValidationError> Errors { get; set; } = new();
    
    // After Transform stage
    public List<Order> Orders { get; set; } = new();
    public List<Customer> CustomersCreated { get; set; } = new();
    public List<Product> ProductsCreated { get; set; } = new();
    public List<Category> CategoriesCreated { get; set; } = new();
    
    // After Persist stage
    public ImportSession? Session { get; set; }
    
    // Pipeline control
    public bool HasErrors => Errors.Count > 0;
    public bool IsAborted { get; set; }
    
    public ImportResult ToResult()
    {
        return new ImportResult
        {
            Success = !HasErrors && !IsAborted,
            OrdersCount = Session?.OrdersCount ?? 0,
            ItemsCount = Session?.ItemsCount ?? 0,
            ImportSessionId = Session?.Id,
            Errors = Errors
        };
    }
}
