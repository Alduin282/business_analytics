using System.Globalization;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

/// <summary>
/// Third link in chain: validates business rules — quantity > 0, price >= 0, valid status.
/// </summary>
public class BusinessRuleValidator : BaseImportValidator
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(OrderStatus.Pending),
        nameof(OrderStatus.Processing),
        nameof(OrderStatus.Shipped),
        nameof(OrderStatus.Delivered),
        nameof(OrderStatus.Cancelled)
    };

    protected override Task<List<ValidationError>> ValidateCoreAsync(List<OrderImportRow> rows, string[] headers)
    {
        var errors = new List<ValidationError>();

        foreach (var row in rows)
        {
            // Quantity > 0
            if (int.TryParse(row.Quantity, out var quantity) && quantity <= 0)
            {
                errors.Add(new ValidationError(row.RowNumber, "Quantity",
                    $"Quantity must be greater than 0, got: {quantity}"));
            }

            // UnitPrice >= 0
            if (decimal.TryParse(row.UnitPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) && price < 0)
            {
                errors.Add(new ValidationError(row.RowNumber, "UnitPrice",
                    $"UnitPrice cannot be negative, got: {price}"));
            }

            // Valid Status
            if (!string.IsNullOrWhiteSpace(row.Status) && !ValidStatuses.Contains(row.Status))
            {
                var validOptions = string.Join(", ", ValidStatuses);
                errors.Add(new ValidationError(row.RowNumber, "Status",
                    $"Invalid status: '{row.Status}'. Valid options: {validOptions}"));
            }
        }

        return Task.FromResult(errors);
    }
}
