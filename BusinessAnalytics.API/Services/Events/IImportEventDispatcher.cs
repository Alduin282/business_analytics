namespace BusinessAnalytics.API.Services.Events;

public interface IImportEventDispatcher
{
    Task NotifyAsync(ImportActivityEvent @event);
}
