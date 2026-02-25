using System.Globalization;
using System.Text.RegularExpressions;
using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

/// <summary>
/// Second link in chain: validates data types — date format, numbers, email, string lengths.
/// </summary>
public class DataTypeValidator : BaseImportValidator
{
    private static readonly string[] DateFormats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd" };
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    protected override Task<List<ValidationError>> ValidateCoreAsync(List<OrderImportRow> rows, string[] headers)
    {
        var errors = new List<ValidationError>();

        foreach (var row in rows)
        {
            // OrderDate
            if (!DateTime.TryParseExact(row.OrderDate, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                errors.Add(new ValidationError(row.RowNumber, "OrderDate",
                    $"Invalid date format: '{row.OrderDate}'. Expected: yyyy-MM-dd HH:mm"));
            }

            // CustomerName
            if (string.IsNullOrWhiteSpace(row.CustomerName))
            {
                errors.Add(new ValidationError(row.RowNumber, "CustomerName", "CustomerName is required"));
            }
            else if (row.CustomerName.Length > 200)
            {
                errors.Add(new ValidationError(row.RowNumber, "CustomerName", "CustomerName exceeds 200 characters"));
            }

            // CustomerEmail
            if (string.IsNullOrWhiteSpace(row.CustomerEmail))
            {
                errors.Add(new ValidationError(row.RowNumber, "CustomerEmail", "CustomerEmail is required"));
            }
            else if (!EmailRegex.IsMatch(row.CustomerEmail))
            {
                errors.Add(new ValidationError(row.RowNumber, "CustomerEmail",
                    $"Invalid email format: '{row.CustomerEmail}'"));
            }

            // ProductName
            if (string.IsNullOrWhiteSpace(row.ProductName))
            {
                errors.Add(new ValidationError(row.RowNumber, "ProductName", "ProductName is required"));
            }
            else if (row.ProductName.Length > 200)
            {
                errors.Add(new ValidationError(row.RowNumber, "ProductName", "ProductName exceeds 200 characters"));
            }

            // CategoryName
            if (string.IsNullOrWhiteSpace(row.CategoryName))
            {
                errors.Add(new ValidationError(row.RowNumber, "CategoryName", "CategoryName is required"));
            }
            else if (row.CategoryName.Length > 100)
            {
                errors.Add(new ValidationError(row.RowNumber, "CategoryName", "CategoryName exceeds 100 characters"));
            }

            // Quantity
            if (!int.TryParse(row.Quantity, out _))
            {
                errors.Add(new ValidationError(row.RowNumber, "Quantity",
                    $"Invalid integer: '{row.Quantity}'"));
            }

            // UnitPrice
            if (!decimal.TryParse(row.UnitPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                errors.Add(new ValidationError(row.RowNumber, "UnitPrice",
                    $"Invalid decimal: '{row.UnitPrice}'"));
            }

            // Status
            if (string.IsNullOrWhiteSpace(row.Status))
            {
                errors.Add(new ValidationError(row.RowNumber, "Status", "Status is required"));
            }
        }

        return Task.FromResult(errors);
    }
}
