using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Events;

public record ImportActivityEvent(
    string UserId,
    ImportAction Action,
    Guid SessionId,
    string FileName,
    DateTime Timestamp,
    int? OrdersCount = null,
    string? AdditionalMessage = null
);
