using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Validation;
using FluentAssertions;

namespace BusinessAnalytics.Tests.Services.Import.Validation;

public class HeaderValidatorTests
{
    private readonly HeaderValidator _headerValidator;

    public HeaderValidatorTests()
    {
        _headerValidator = new HeaderValidator();
    }

    [Fact]
    public async Task ValidateAsync_WithAllRequiredHeaders_ReturnsNoErrors()
    {
        // Arrange
        var headers = new[] { "OrderDate", "CustomerName", "CustomerEmail", "ProductName", "CategoryName", "Quantity", "UnitPrice", "Status" };
        var rows = new List<OrderImportRow> { new OrderImportRow() };

        // Act
        var result = await _headerValidator.ValidateAsync(rows, headers);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithMissingHeader_ReturnsError()
    {
        // Arrange
        var headers = new[] { "OrderDate", "CustomerName", "CustomerEmail" }; // Missing others
        var rows = new List<OrderImportRow> { new OrderImportRow() };

        // Act
        var result = await _headerValidator.ValidateAsync(rows, headers);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(e => e.Column == "ProductName");
        result.Should().Contain(e => e.Column == "Status");
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyRows_ReturnsFileEmptyError()
    {
        // Arrange
        var headers = new[] { "OrderDate", "CustomerName", "CustomerEmail", "ProductName", "CategoryName", "Quantity", "UnitPrice", "Status" };
        var rows = new List<OrderImportRow>();

        // Act
        var result = await _headerValidator.ValidateAsync(rows, headers);

        // Assert
        result.Should().HaveCount(1);
        result[0].Message.Should().Contain("no data rows");
    }

    [Fact]
    public async Task ValidateAsync_WithExtraHeaders_ReturnsNoErrors()
    {
        // Arrange
        var headers = new[] { "OrderDate", "CustomerName", "CustomerEmail", "ProductName", "CategoryName", "Quantity", "UnitPrice", "Status", "ExtraColumn" };
        var rows = new List<OrderImportRow> { new OrderImportRow() };

        // Act
        var result = await _headerValidator.ValidateAsync(rows, headers);

        // Assert
        result.Should().BeEmpty();
    }
}
