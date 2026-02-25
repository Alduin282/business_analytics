using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Validation;
using FluentAssertions;

namespace BusinessAnalytics.Tests.Services.Import.Validation;

public class BusinessRuleValidatorTests
{
    private readonly BusinessRuleValidator _validator;

    public BusinessRuleValidatorTests()
    {
        _validator = new BusinessRuleValidator();
    }

    private OrderImportRow CreateValidRow() => new()
    {
        RowNumber = 2,
        Quantity = "10",
        UnitPrice = "99.99",
        Status = "Delivered"
    };

    [Fact]
    public async Task ValidateAsync_WithValidRules_ReturnsNoErrors()
    {
        // Arrange
        var rows = new List<OrderImportRow> { CreateValidRow() };

        // Act
        var result = await _validator.ValidateAsync(rows, new string[0]);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task ValidateAsync_WithInvalidQuantity_ReturnsError(string qty)
    {
        // Arrange
        var row = CreateValidRow();
        row.Quantity = qty;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, new string[0]);

        // Assert
        result.Should().Contain(e => e.Column == "Quantity" && e.Message.Contains("greater than 0"));
    }

    [Fact]
    public async Task ValidateAsync_WithNegativePrice_ReturnsError()
    {
        // Arrange
        var row = CreateValidRow();
        row.UnitPrice = "-0.01";
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, new string[0]);

        // Assert
        result.Should().Contain(e => e.Column == "UnitPrice" && e.Message.Contains("cannot be negative"));
    }

    [Theory]
    [InlineData("InvalidStatus")]
    [InlineData("None")]
    public async Task ValidateAsync_WithInvalidStatus_ReturnsError(string status)
    {
        // Arrange
        var row = CreateValidRow();
        row.Status = status;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, new string[0]);

        // Assert
        result.Should().Contain(e => e.Column == "Status" && e.Message.Contains("Invalid status"));
    }

    [Theory]
    [InlineData("pending")] // Case insensitive check
    [InlineData("DELIVERED")]
    [InlineData("Shipped")]
    public async Task ValidateAsync_WithStatusDifferentCase_ReturnsNoErrors(string status)
    {
        // Arrange
        var row = CreateValidRow();
        row.Status = status;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, new string[0]);

        // Assert
        result.Should().BeEmpty();
    }
}
