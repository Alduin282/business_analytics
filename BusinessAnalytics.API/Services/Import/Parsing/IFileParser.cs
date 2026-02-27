using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Parsing;

public interface IFileParser
{
    string SupportedExtension { get; }
    Task<List<OrderImportRow>> ParseAsync(Stream stream);
}
