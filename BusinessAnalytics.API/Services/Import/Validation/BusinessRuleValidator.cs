using System.Globalization;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

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
            Validate(row, errors)
                .Field("Quantity", r => r.Quantity).PositiveInt()
                .Field("UnitPrice", r => r.UnitPrice).NonNegativeDecimal()
                .Field("Status", r => r.Status).InSet(ValidStatuses, "Invalid status");
        }

        return Task.FromResult(errors);
    }

    private RuleBuilder Validate(OrderImportRow row, List<ValidationError> errors) 
        => new(row, errors);

    private class RuleBuilder
    {
        private readonly OrderImportRow _row;
        private readonly List<ValidationError> _errors;
        private string _currentField = "";
        private string _currentValue = "";

        public RuleBuilder(OrderImportRow row, List<ValidationError> errors)
        {
            _row = row;
            _errors = errors;
        }

        public RuleBuilder Field(string name, Func<OrderImportRow, string> selector)
        {
            _currentField = name;
            _currentValue = selector(_row) ?? string.Empty;
            return this;
        }

        public RuleBuilder PositiveInt()
        {
            if (int.TryParse(_currentValue, out var val) && val <= 0)
                AddError($"{_currentField} must be greater than 0, got: {val}");
            return this;
        }

        public RuleBuilder NonNegativeDecimal()
        {
            if (decimal.TryParse(_currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) && val < 0)
                AddError($"{_currentField} cannot be negative, got: {val}");
            return this;
        }

        public RuleBuilder InSet(HashSet<string> validSet, string messagePrefix)
        {
            if (!string.IsNullOrWhiteSpace(_currentValue) && !validSet.Contains(_currentValue))
            {
                var options = string.Join(", ", validSet);
                AddError($"{messagePrefix}: '{_currentValue}'. Valid options: {options}");
            }
            return this;
        }

        private void AddError(string message) 
            => _errors.Add(new ValidationError(_row.RowNumber, _currentField, message));
    }
}
