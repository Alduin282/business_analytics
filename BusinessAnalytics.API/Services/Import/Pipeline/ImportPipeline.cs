namespace BusinessAnalytics.API.Services.Import.Pipeline;

/// <summary>
/// Pipeline orchestrator. Executes stages sequentially, aborting on errors.
/// </summary>
public class ImportPipeline
{
    private readonly IEnumerable<IImportPipelineStage> _stages;

    public ImportPipeline(IEnumerable<IImportPipelineStage> stages)
    {
        _stages = stages;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        foreach (var stage in _stages)
        {
            context = await stage.ExecuteAsync(context);
            
            if (context.IsAborted || context.HasErrors)
                break;
        }

        return context;
    }
}
