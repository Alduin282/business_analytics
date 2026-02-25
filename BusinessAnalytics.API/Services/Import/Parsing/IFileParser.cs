using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Parsing;

/// <summary>
/// Strategy interface for file parsing.
/// Implement this for each supported file format (CSV, Excel, JSON, etc.).
/// </summary>
public interface IFileParser
{
    string SupportedExtension { get; }
    Task<List<OrderImportRow>> ParseAsync(Stream stream);
}
