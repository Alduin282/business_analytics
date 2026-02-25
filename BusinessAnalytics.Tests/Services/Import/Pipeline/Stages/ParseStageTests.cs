using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Parsing;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using BusinessAnalytics.API.Services.Import.Validation;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline.Stages;

public class ParseStageTests
{
    private readonly Mock<FileParserFactory> _factoryMock;
    private readonly ParseStage _stage;

    public ParseStageTests()
    {
        _factoryMock = new Mock<FileParserFactory>(new List<IFileParser>());
        _stage = new ParseStage(_factoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfullyParsesFile()
    {
        // Arrange
        var stream = new MemoryStream();
        var context = new ImportContext { FileName = "test.csv", FileStream = stream };
        
        var parserMock = new Mock<IFileParser>();
        var rows = new List<OrderImportRow> { new() { ProductName = "Test" } };
        parserMock.Setup(p => p.ParseAsync(stream)).ReturnsAsync(rows);
        
        _factoryMock.Setup(f => f.GetParser("test.csv")).Returns(parserMock.Object);

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.ParsedRows.Should().HaveCount(1);
        result.ParsedRows[0].ProductName.Should().Be("Test");
        result.IsAborted.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenNoParserFound()
    {
        // Arrange
        var context = new ImportContext { FileName = "unknown.ext" };
        _factoryMock.Setup(f => f.GetParser("unknown.ext")).Throws(new NotSupportedException("Not supported"));

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.IsAborted.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Not supported"));
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenParserThrows()
    {
        // Arrange
        var context = new ImportContext { FileName = "test.csv", FileStream = new MemoryStream() };
        var parserMock = new Mock<IFileParser>();
        parserMock.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ThrowsAsync(new Exception("Parse error"));
        
        _factoryMock.Setup(f => f.GetParser("test.csv")).Returns(parserMock.Object);

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.IsAborted.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Parse error"));
    }
}
