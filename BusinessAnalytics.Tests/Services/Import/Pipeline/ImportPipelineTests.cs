using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Services.Import.Pipeline;
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

        var pipeline = new ImportPipeline(new[] { stage1.Object, stage2.Object });

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        stage1.Verify(s => s.ExecuteAsync(context), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(context), Times.Once);
        result.Should().Be(context);
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
        
        var pipeline = new ImportPipeline(new[] { stage1.Object, stage2.Object });

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
        
        var pipeline = new ImportPipeline(new[] { stage1.Object, stage2.Object });

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        stage1.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(It.IsAny<ImportContext>()), Times.Never);
    }
}
