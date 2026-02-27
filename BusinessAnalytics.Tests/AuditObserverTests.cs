using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Services.Events.Observers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BusinessAnalytics.Tests;

public class AuditObserverTests
{
    [Fact]
    public async Task HandleAsync_ValidEvent_CreatesAuditLogAndSaves()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApplicationDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork))).Returns(unitOfWork);
        
        var loggerMock = new Mock<ILogger<AuditObserver>>();
        var observer = new AuditObserver(scopeFactoryMock.Object, loggerMock.Object);
        
        var @event = new ImportActivityEvent(
            UserId: "user-123",
            Action: ImportAction.Imported,
            SessionId: Guid.NewGuid(),
            FileName: "test_data.csv",
            Timestamp: DateTime.UtcNow,
            OrdersCount: 10,
            AdditionalMessage: "Success"
        );

        // Act
        await observer.HandleAsync(@event);

        // Assert
        var log = await context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal(@event.UserId, log!.UserId);
        Assert.Equal(@event.Action, log.Action);
        Assert.Equal(@event.SessionId, log.RelatedId);
        Assert.Contains("test_data.csv", log.Message);
        Assert.Contains("10", log.Message);
        Assert.Contains("Success", log.Message);
    }
}
