using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Events.Observers;

public class AuditObserver : IImportObserver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditObserver> _logger;

    public AuditObserver(IServiceProvider serviceProvider, ILogger<AuditObserver> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleAsync(ImportActivityEvent @event)
    {
        // Use a new scope since this might be called from a long-running process or background
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            Action = @event.Action,
            Message = $"File: {@event.FileName}. Orders: {@event.OrdersCount ?? 0}. {@event.AdditionalMessage}",
            RelatedId = @event.SessionId,
            CreatedAt = @event.Timestamp
        };

        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Audit log created for action {Action} by user {UserId}", @event.Action, @event.UserId);
    }
}
