using BusinessAnalytics.API.Services.Import.Parsing;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Parsing;

public class FileParserFactoryTests
{
    private readonly Mock<IFileParser> _csvParserMock;
    private readonly Mock<IFileParser> _jsonParserMock;
    private readonly FileParserFactory _fileParserFactory;

    public FileParserFactoryTests()
    {
        _csvParserMock = new Mock<IFileParser>();
        _csvParserMock.Setup(p => p.SupportedExtension).Returns(".csv");

        _jsonParserMock = new Mock<IFileParser>();
        _jsonParserMock.Setup(p => p.SupportedExtension).Returns(".json");

        var parsers = new List<IFileParser> { _csvParserMock.Object, _jsonParserMock.Object };
        _fileParserFactory = new FileParserFactory(parsers);
    }

    [Fact]
    public void GetParser_WithSupportedExtension_ReturnsCorrectParser()
    {
        // Act
        var result = _fileParserFactory.GetParser("data.csv");

        // Assert
        result.Should().Be(_csvParserMock.Object);
        result.SupportedExtension.Should().Be(".csv");
    }

    [Fact]
    public void GetParser_WithDifferentCaseExtension_ReturnsCorrectParser()
    {
        // Act
        var result = _fileParserFactory.GetParser("DATA.JSON");

        // Assert
        result.Should().Be(_jsonParserMock.Object);
        result.SupportedExtension.Should().Be(".json");
    }

    [Fact]
    public void GetParser_WithUnsupportedExtension_ThrowsNotSupportedException()
    {
        // Act
        Action act = () => _fileParserFactory.GetParser("data.xml");

        // Assert
        act.Should().Throw<NotSupportedException>()
            .And.Message.Should().Contain(".xml");
    }

    [Fact]
    public void GetParser_WithNoExtension_ThrowsNotSupportedException()
    {
        // Act
        Action act = () => _fileParserFactory.GetParser("datafile");

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
