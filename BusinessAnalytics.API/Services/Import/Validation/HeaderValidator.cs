using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

/// <summary>
/// First link in chain: validates that all required CSV headers are present.
/// </summary>
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
        var normalizedHeaders = headers.Select(h => h.Trim().ToLowerInvariant()).ToHashSet();

        foreach (var required in RequiredHeaders)
        {
            if (!normalizedHeaders.Contains(required.ToLowerInvariant()))
            {
                errors.Add(new ValidationError(1, required, $"Missing required column: '{required}'"));
            }
        }

        if (rows.Count == 0 && errors.Count == 0)
        {
            errors.Add(new ValidationError(0, "", "File contains no data rows"));
        }

        return Task.FromResult(errors);
    }
}
