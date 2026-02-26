using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Import.Pipeline;

public class ImportPipeline
{
    private readonly IEnumerable<IImportPipelineStage> _stages;
    private readonly IImportEventDispatcher _dispatcher;

    public ImportPipeline(IEnumerable<IImportPipelineStage> stages, IImportEventDispatcher dispatcher)
    {
        _stages = stages;
        _dispatcher = dispatcher;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        foreach (var stage in _stages)
        {
            context = await stage.ExecuteAsync(context);
            
            if (context.IsAborted || context.HasErrors)
                break;
        }

        if (!context.IsAborted && !context.HasErrors)
        {
            await _dispatcher.NotifyAsync(new ImportActivityEvent(
                context.UserId,
                ImportAction.Imported,
                context.Session?.Id ?? Guid.Empty,
                context.FileName,
                DateTime.UtcNow,
                context.Orders.Count
            ));
        }

        return context;
    }
}
