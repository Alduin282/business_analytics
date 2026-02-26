using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Models;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline;

public class ImportPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_RunsAllStages_WhenNoErrors()
    {
        // Arrange
        var context = new ImportContext { FileName = "test.csv" };
        var stage1 = new Mock<IImportPipelineStage>();
        var stage2 = new Mock<IImportPipelineStage>();

        stage1.Setup(s => s.ExecuteAsync(It.IsAny<ImportContext>()))
              .ReturnsAsync((ImportContext ctx) => ctx);
        stage2.Setup(s => s.ExecuteAsync(It.IsAny<ImportContext>()))
              .ReturnsAsync((ImportContext ctx) => ctx);

        var dispatcher = new Mock<IImportEventDispatcher>();
        var pipeline = new ImportPipeline([stage1.Object, stage2.Object], dispatcher.Object);

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        stage1.Verify(s => s.ExecuteAsync(context), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(context), Times.Once);
        result.Should().Be(context);

        dispatcher.Verify(d => d.NotifyAsync(It.Is<ImportActivityEvent>(e => 
            e.UserId == context.UserId && 
            e.Action == ImportAction.Imported && 
            e.FileName == context.FileName)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenStageSetsIsAborted()
    {
        // Arrange
        var context = new ImportContext { FileName = "test.csv" };
        var stage1 = new Mock<IImportPipelineStage>();
        var stage2 = new Mock<IImportPipelineStage>();

        stage1.Setup(s => s.ExecuteAsync(It.IsAny<ImportContext>()))
              .ReturnsAsync((ImportContext ctx) => { ctx.IsAborted = true; return ctx; });
        
        var dispatcher = new Mock<IImportEventDispatcher>();
        var pipeline = new ImportPipeline([stage1.Object, stage2.Object], dispatcher.Object);

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        stage1.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenStageAddsErrors()
    {
        // Arrange
        var context = new ImportContext { FileName = "test.csv" };
        var stage1 = new Mock<IImportPipelineStage>();
        var stage2 = new Mock<IImportPipelineStage>();

        stage1.Setup(s => s.ExecuteAsync(It.IsAny<ImportContext>()))
              .ReturnsAsync((ImportContext ctx) => { 
                  ctx.Errors.Add(new BusinessAnalytics.API.Services.Import.Validation.ValidationError(1, "Test", "Error")); 
                  return ctx; 
              });
        
        var dispatcher = new Mock<IImportEventDispatcher>();
        var pipeline = new ImportPipeline([stage1.Object, stage2.Object], dispatcher.Object);

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        stage1.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Never);
    }
}
