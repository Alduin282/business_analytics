using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

public class HeaderValidator : BaseImportValidator
{
    private static readonly string[] RequiredHeaders =
    {
        "OrderDate", "CustomerName", "CustomerEmail", "ProductName",
        "CategoryName", "Quantity", "UnitPrice", "Status"
    };

    protected override Task<List<ValidationError>> ValidateCoreAsync(List<OrderImportRow> rows, string[] headers)
    {
        var errors = new List<ValidationError>();
        var presentHeaders = headers.Select(h => h.Trim().ToLowerInvariant()).ToHashSet();

        // Identify missing headers
        var missing = RequiredHeaders
            .Where(required => !presentHeaders.Contains(required.ToLowerInvariant()));

        foreach (var header in missing)
        {
            errors.Add(new ValidationError(1, header, $"Missing required column: '{header}'"));
        }

        if (rows.Count == 0 && errors.Count == 0)
        {
            errors.Add(new ValidationError(0, "", "File contains no data rows"));
        }

        return Task.FromResult(errors);
    }
}
