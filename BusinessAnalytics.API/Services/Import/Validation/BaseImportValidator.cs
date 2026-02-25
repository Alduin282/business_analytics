using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

/// <summary>
/// Base class for import validators (Chain of Responsibility).
/// Handles chain linking and delegates to the next validator.
/// </summary>
public abstract class BaseImportValidator : IImportValidator
{
    public IImportValidator? Next { get; set; }

    public IImportValidator SetNext(IImportValidator next)
    {
        Next = next;
        return next;
    }

    public async Task<List<ValidationError>> ValidateAsync(List<OrderImportRow> rows, string[] headers)
    {
        var errors = await ValidateCoreAsync(rows, headers);
        
        // If current validator found errors, stop the chain
        if (errors.Count > 0)
            return errors;
        
        // Pass to next validator in the chain
        if (Next != null)
            return await Next.ValidateAsync(rows, headers);
        
        return errors;
    }

    protected abstract Task<List<ValidationError>> ValidateCoreAsync(List<OrderImportRow> rows, string[] headers);
}
