namespace BusinessAnalytics.API.Services.Import.Pipeline;

/// <summary>
/// Pipeline pattern: interface for a single stage in the import pipeline.
/// Each stage transforms the ImportContext and passes it to the next stage.
/// </summary>
public interface IImportPipelineStage
{
    Task<ImportContext> ExecuteAsync(ImportContext context);
}
