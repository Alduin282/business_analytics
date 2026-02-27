using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;

namespace BusinessAnalytics.API.Services.Events.Observers;

public class AuditObserver : IImportObserver
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditObserver> _logger;

    public AuditObserver(IServiceScopeFactory scopeFactory, ILogger<AuditObserver> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(ImportActivityEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository = unitOfWork.Repository<AuditLog, Guid>();

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            Action = @event.Action,
            Message = $"File: {@event.FileName}. Orders: {@event.OrdersCount ?? 0}. {@event.AdditionalMessage}",
            RelatedId = @event.SessionId,
            CreatedAt = @event.Timestamp
        };

        await repository.AddAsync(log);
        await unitOfWork.CompleteAsync();
        
        _logger.LogInformation("Audit log created for action {Action} by user {UserId}", @event.Action, @event.UserId);
    }
}
