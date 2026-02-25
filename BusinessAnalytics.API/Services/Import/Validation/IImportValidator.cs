using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Validation;

/// <summary>
/// Chain of Responsibility interface for import validation.
/// Each validator checks its own rules and passes to the next in the chain.
/// </summary>
public interface IImportValidator
{
    IImportValidator? Next { get; set; }
    
    IImportValidator SetNext(IImportValidator next);
    
    Task<List<ValidationError>> ValidateAsync(List<OrderImportRow> rows, string[] headers);
}
