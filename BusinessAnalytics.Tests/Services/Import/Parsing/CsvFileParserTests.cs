using System.Text;
using BusinessAnalytics.API.Services.Import.Parsing;
using FluentAssertions;

namespace BusinessAnalytics.Tests.Services.Import.Parsing;

public class CsvFileParserTests
{
    private readonly CsvFileParser _parser;

    public CsvFileParserTests()
    {
        _parser = new CsvFileParser();
    }

    [Fact]
    public void SupportedExtension_ShouldBeCsv()
    {
        _parser.SupportedExtension.Should().Be(".csv");
    }

    [Fact]
    public async Task ParseAsync_WithValidCsv_ReturnsCorrectData()
    {
        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("OrderDate,CustomerName,CustomerEmail,ProductName,CategoryName,Quantity,UnitPrice,Status");
        csvContent.AppendLine("2023-01-01 10:00,John Doe,john@example.com,Laptop,Electronics,1,1200.50,Delivered");
        csvContent.AppendLine("2023-01-02 12:00,Jane Smith,jane@example.com,Phone,Electronics,2,800.00,Shipped");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(2);
        
        result[0].OrderDate.Should().Be("2023-01-01 10:00");
        result[0].CustomerName.Should().Be("John Doe");
        result[0].UnitPrice.Should().Be("1200.50");
        result[0].RowNumber.Should().Be(2);

        result[1].CustomerEmail.Should().Be("jane@example.com");
        result[1].Quantity.Should().Be("2");
        result[1].RowNumber.Should().Be(3);
    }

    [Fact]
    public async Task ParseAsync_WithQuotedFieldsAndCommas_ParsesCorrectly()
    {
        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("OrderDate,CustomerName,CustomerEmail,ProductName,CategoryName,Quantity,UnitPrice,Status");
        csvContent.AppendLine("2023-01-01 10:00,\"Doe, John\",john@example.com,\"Laptop, Pro\",Electronics,1,1200.50,Delivered");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerName.Should().Be("Doe, John");
        result[0].ProductName.Should().Be("Laptop, Pro");
    }

    [Fact]
    public async Task ParseAsync_WithEscapedQuotes_ParsesCorrectly()
    {
        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("OrderDate,CustomerName,CustomerEmail,ProductName,CategoryName,Quantity,UnitPrice,Status");
        csvContent.AppendLine("2023-01-01 10:00,John Doe,john@example.com,\"Product with \"\"Quotes\"\"\",Electronics,1,1200.50,Delivered");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].ProductName.Should().Be("Product with \"Quotes\"");
    }

    [Fact]
    public async Task ParseAsync_WithDifferentColumnOrder_ParsesCorrectly()
    {
        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("Status,UnitPrice,Quantity,ProductName,CategoryName,OrderDate,CustomerEmail,CustomerName");
        csvContent.AppendLine("Delivered,1200.50,1,Laptop,Electronics,2023-01-01 10:00,john@example.com,John Doe");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomerName.Should().Be("John Doe");
        result[0].UnitPrice.Should().Be("1200.50");
        result[0].Status.Should().Be("Delivered");
    }

    [Fact]
    public async Task ParseAsync_WithEmptyStream_ReturnsEmptyList()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_WithMissingOptionalColumns_ReturnsEmptyStrings()
    {
        // Arrange
        var csvContent = new StringBuilder();
        csvContent.AppendLine("OrderDate,CustomerName,CustomerEmail,ProductName,CategoryName,Quantity,UnitPrice"); // Status missing in header
        csvContent.AppendLine("2023-01-01 10:00,John Doe,john@example.com,Laptop,Electronics,1,1200.50");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().BeEmpty();
    }
}
