namespace BusinessAnalytics.API.Services.Import.Pipeline;

public interface IImportPipelineStage
{
    Task<ImportContext> ExecuteAsync(ImportContext context);
}
