namespace BusinessAnalytics.API.Services.Events;

public interface IImportObserver
{
    Task HandleAsync(ImportActivityEvent @event);
}
