using System.Globalization;
using System.Text.RegularExpressions;
using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

public class DataTypeValidator : BaseImportValidator
{
    private static readonly string[] DateFormats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd" };
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    protected override Task<List<ValidationError>> ValidateCoreAsync(List<OrderImportRow> rows, string[] headers)
    {
        var errors = new List<ValidationError>();

        foreach (var row in rows)
        {
            Validate(row, errors)
                .Field("OrderDate", r => r.OrderDate).IsDate(DateFormats)
                .Field("CustomerName", r => r.CustomerName).NotEmpty().Max(200)
                .Field("CustomerEmail", r => r.CustomerEmail).NotEmpty().IsEmail()
                .Field("ProductName", r => r.ProductName).NotEmpty().Max(200)
                .Field("CategoryName", r => r.CategoryName).NotEmpty().Max(100)
                .Field("Quantity", r => r.Quantity).IsInt()
                .Field("UnitPrice", r => r.UnitPrice).IsDecimal()
                .Field("Status", r => r.Status).NotEmpty();
        }

        return Task.FromResult(errors);
    }

    private FieldValidator Validate(OrderImportRow row, List<ValidationError> errors) 
        => new(row, errors);

    private class FieldValidator
    {
        private readonly OrderImportRow _row;
        private readonly List<ValidationError> _errors;
        private string _currentField = "";
        private string _currentValue = "";

        public FieldValidator(OrderImportRow row, List<ValidationError> errors)
        {
            _row = row;
            _errors = errors;
        }

        public FieldValidator Field(string name, Func<OrderImportRow, string> selector)
        {
            _currentField = name;
            _currentValue = selector(_row) ?? string.Empty;
            return this;
        }

        public FieldValidator NotEmpty(string? message = null)
        {
            if (string.IsNullOrWhiteSpace(_currentValue))
                AddError(message ?? $"{_currentField} is required");
            return this;
        }

        public FieldValidator Max(int length, string? message = null)
        {
            if (_currentValue.Length > length)
                AddError(message ?? $"{_currentField} exceeds {length} characters");
            return this;
        }

        public FieldValidator IsDate(string[] formats)
        {
            if (!DateTime.TryParseExact(_currentValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                AddError($"Invalid date format: '{_currentValue}'. Expected: yyyy-MM-dd HH:mm");
            return this;
        }

        public FieldValidator IsEmail()
        {
            if (!string.IsNullOrWhiteSpace(_currentValue) && !EmailRegex.IsMatch(_currentValue))
                AddError($"Invalid email format: '{_currentValue}'");
            return this;
        }

        public FieldValidator IsInt()
        {
            if (!int.TryParse(_currentValue, out _))
                AddError($"Invalid integer: '{_currentValue}'");
            return this;
        }

        public FieldValidator IsDecimal()
        {
            if (!decimal.TryParse(_currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                AddError($"Invalid decimal: '{_currentValue}'");
            return this;
        }

        private void AddError(string message) 
            => _errors.Add(new ValidationError(_row.RowNumber, _currentField, message));
    }
}
