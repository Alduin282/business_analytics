using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Services.Events.Observers;
using BusinessAnalytics.API.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessAnalytics.Tests;

public class ImportEventDispatcherTests
{
    private readonly Mock<IImportObserver> _observerMock1;
    private readonly Mock<IImportObserver> _observerMock2;
    private readonly Mock<ILogger<ImportEventDispatcher>> _loggerMock;
    private readonly ImportEventDispatcher _importEventDispatcher;

    public ImportEventDispatcherTests()
    {
        _observerMock1 = new Mock<IImportObserver>();
        _observerMock2 = new Mock<IImportObserver>();
        _loggerMock = new Mock<ILogger<ImportEventDispatcher>>();
        
        var observers = new List<IImportObserver> { _observerMock1.Object, _observerMock2.Object };
        _importEventDispatcher = new ImportEventDispatcher(observers, _loggerMock.Object);
    }

    [Fact]
    public async Task NotifyAsync_CallsAllObservers_AllObservesCalledCorrectly()
    {
        // Arrange
        var @event = new ImportActivityEvent(
            "user-1",
            ImportAction.Imported,
            Guid.NewGuid(),
            "test.csv",
            DateTime.UtcNow
        );

        // Act
        await _importEventDispatcher.NotifyAsync(@event);

        // Assert
        _observerMock1.Verify(x => x.HandleAsync(@event), Times.Once);
        _observerMock2.Verify(x => x.HandleAsync(@event), Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_HandlesObserverExceptions_SecondOneHandledAnyway()
    {
        // Arrange
        var @event = new ImportActivityEvent("u1", ImportAction.RolledBack, Guid.NewGuid(), "f.csv", DateTime.UtcNow);
        
        _observerMock1.Setup(x => x.HandleAsync(It.IsAny<ImportActivityEvent>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        // Should not throw
        await _importEventDispatcher.NotifyAsync(@event);

        // Assert
        _observerMock1.Verify(x => x.HandleAsync(@event), Times.Once);
        _observerMock2.Verify(x => x.HandleAsync(@event), Times.Once); // Second observer still called
    }
}
