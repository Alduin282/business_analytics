using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Validation;
using FluentAssertions;

namespace BusinessAnalytics.Tests.Services.Import.Validation;

public class DataTypeValidatorTests
{
    private readonly DataTypeValidator _validator;

    public DataTypeValidatorTests()
    {
        _validator = new DataTypeValidator();
    }

    private OrderImportRow CreateValidRow() => new()
    {
        RowNumber = 2,
        OrderDate = "2023-01-01 10:00",
        CustomerName = "John Doe",
        CustomerEmail = "john@example.com",
        ProductName = "Laptop",
        CategoryName = "Electronics",
        Quantity = "1",
        UnitPrice = "100.50",
        Status = "Pending"
    };

    [Fact]
    public async Task ValidateAsync_WithValidData_ReturnsNoErrors()
    {
        // Arrange
        var rows = new List<OrderImportRow> { CreateValidRow() };

        // Act
        var result = await _validator.ValidateAsync(rows, []);

        // Assert
        result.Should().BeEmpty();
    }

    #region OrderDate Tests

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("2023/01/01")]
    [InlineData("2023-13-01")]
    [InlineData("")]
    public async Task ValidateAsync_WithInvalidDate_ReturnsError(string date)
    {
        // Arrange
        var row = CreateValidRow();
        row.OrderDate = date;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, []);

        // Assert
        result.Should().Contain(e => e.Column == "OrderDate");
    }

    #endregion

    #region CustomerName Tests

    [Fact]
    public async Task ValidateAsync_WithEmptyCustomerName_ReturnsError()
    {
        var row = CreateValidRow();
        row.CustomerName = "";
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "CustomerName" && e.Message.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_WithLongCustomerName_ReturnsError()
    {
        var row = CreateValidRow();
        row.CustomerName = new string('A', 201);
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "CustomerName" && e.Message.Contains("exceeds 200"));
    }

    #endregion

    #region CustomerEmail Tests

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("")]
    public async Task ValidateAsync_WithInvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var row = CreateValidRow();
        row.CustomerEmail = email;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, []);

        // Assert
        result.Should().Contain(e => e.Column == "CustomerEmail");
    }

    #endregion

    #region ProductName Tests

    [Fact]
    public async Task ValidateAsync_WithEmptyProductName_ReturnsError()
    {
        var row = CreateValidRow();
        row.ProductName = "";
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "ProductName" && e.Message.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_WithLongProductName_ReturnsError()
    {
        var row = CreateValidRow();
        row.ProductName = new string('A', 201);
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "ProductName" && e.Message.Contains("exceeds 200"));
    }

    #endregion

    #region CategoryName Tests

    [Fact]
    public async Task ValidateAsync_WithEmptyCategoryName_ReturnsError()
    {
        var row = CreateValidRow();
        row.CategoryName = "";
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "CategoryName" && e.Message.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_WithLongCategoryName_ReturnsError()
    {
        var row = CreateValidRow();
        row.CategoryName = new string('A', 101);
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "CategoryName" && e.Message.Contains("exceeds 100"));
    }

    #endregion

    #region Quantity Tests

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    public async Task ValidateAsync_WithInvalidQuantity_ReturnsError(string qty)
    {
        // Arrange
        var row = CreateValidRow();
        row.Quantity = qty;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, []);

        // Assert
        result.Should().Contain(e => e.Column == "Quantity");
    }

    #endregion

    #region UnitPrice Tests

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34.56")]
    [InlineData("100$")]
    [InlineData("")]
    public async Task ValidateAsync_WithInvalidPrice_ReturnsError(string price)
    {
        // Arrange
        var row = CreateValidRow();
        row.UnitPrice = price;
        var rows = new List<OrderImportRow> { row };

        // Act
        var result = await _validator.ValidateAsync(rows, []);

        // Assert
        result.Should().Contain(e => e.Column == "UnitPrice");
    }

    #endregion

    #region Status Tests

    [Fact]
    public async Task ValidateAsync_WithEmptyStatus_ReturnsError()
    {
        var row = CreateValidRow();
        row.Status = "";
        var result = await _validator.ValidateAsync(new List<OrderImportRow> { row }, []);
        result.Should().Contain(e => e.Column == "Status" && e.Message.Contains("required"));
    }

    #endregion
}
