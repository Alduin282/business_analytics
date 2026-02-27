using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Import.Pipeline;

public interface IImportPipeline
{
    Task<ImportContext> ExecuteAsync(ImportContext context);
}
