namespace BusinessAnalytics.API.Services.Events;

public class ImportEventDispatcher : IImportEventDispatcher
{
    private readonly IEnumerable<IImportObserver> _observers;
    private readonly ILogger<ImportEventDispatcher> _logger;

    public ImportEventDispatcher(IEnumerable<IImportObserver> observers, ILogger<ImportEventDispatcher> logger)
    {
        _observers = observers;
        _logger = logger;
    }

    public async Task NotifyAsync(ImportActivityEvent @event)
    {
        _logger.LogInformation("Dispatching import event: {Action} for session {SessionId}", @event.Action, @event.SessionId);
        
        var tasks = _observers.Select(async observer => 
        {
            try
            {
                await observer.HandleAsync(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying observer {ObserverType}", observer.GetType().Name);
            }
        });

        await Task.WhenAll(tasks);
    }
}
