using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Services.Import.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace BusinessAnalytics.Tests;

public class PerformanceImportPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_SuccessfulImport_LogsAccuratePerformanceData()
    {
        // Arrange
        var stages = new List<IImportPipelineStage>();
        var dispatcherMock = new Mock<IImportEventDispatcher>();
        var loggerMock = new Mock<ILogger<PerformanceImportPipeline>>();
        
        var context = new ImportContext
        {
            FileName = "test.csv",
            UserId = "user-123"
        };
        
        var decorator = new PerformanceImportPipeline(stages, dispatcherMock.Object, loggerMock.Object);

        // Act
        var result = await decorator.ExecuteAsync(context);

        // Assert
        result.Should().NotBeNull();
        
        // Verify start log
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting import")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed in")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_FailedImport_LogsErrorWithPerformanceMetrics()
    {
        // Arrange
        var stages = new List<IImportPipelineStage>();
        var dispatcherMock = new Mock<IImportEventDispatcher>();
        var loggerMock = new Mock<ILogger<PerformanceImportPipeline>>();
        
        var context = new ImportContext
        {
            FileName = "test.csv",
            UserId = "user-123"
        };
        
        // Simulate failure by mocking stages to throw exception
        var stageMock = new Mock<IImportPipelineStage>();
        stageMock.Setup(x => x.ExecuteAsync(context)).ThrowsAsync(new InvalidOperationException("Import failed"));
        stages.Add(stageMock.Object);
        
        var decorator = new PerformanceImportPipeline(stages, dispatcherMock.Object, loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.ExecuteAsync(context));
        exception.Message.Should().Be("Import failed");
        
        // Verify error logging with performance metrics
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed after")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
