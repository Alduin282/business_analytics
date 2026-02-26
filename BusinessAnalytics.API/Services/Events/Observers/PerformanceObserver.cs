using System.Diagnostics;

namespace BusinessAnalytics.API.Services.Events.Observers;

public class PerformanceObserver : IImportObserver
{
    private readonly ILogger<PerformanceObserver> _logger;

    public PerformanceObserver(ILogger<PerformanceObserver> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(ImportActivityEvent @event)
    {
        // In a real scenario, we might use the timestamp to calculate duration if we had a "Started" event
        // For now, we'll just log that we received it and the current system state
        _logger.LogInformation("[PERFORMANCE] Action: {Action} completed at {Time}. Session: {SessionId}", 
            @event.Action, DateTime.UtcNow, @event.SessionId);
        
        await Task.CompletedTask;
    }
}
