using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using BusinessAnalytics.API.Services.Import.Validation;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline.Stages;

public class ValidationStageTests
{
    private readonly ValidationStage _stage;
    private readonly HeaderValidator _headerValidator;
    private readonly DataTypeValidator _dataTypeValidator;
    private readonly BusinessRuleValidator _businessRuleValidator;

    public ValidationStageTests()
    {
        _headerValidator = new HeaderValidator();
        _dataTypeValidator = new DataTypeValidator();
        _businessRuleValidator = new BusinessRuleValidator();
        _stage = new ValidationStage(_headerValidator, _dataTypeValidator, _businessRuleValidator);
    }

    [Fact]
    public async Task ExecuteAsync_RunsValidatorChain_NoErrors()
    {
        // Arrange
        var context = new ImportContext 
        { 
            ParsedRows = new List<OrderImportRow> 
            { 
                new() { OrderDate = "2023-01-01 10:00", CustomerName = "John", CustomerEmail = "j@t.com", ProductName = "P", CategoryName = "C", Quantity = "1", UnitPrice = "1", Status = "Pending" } 
            },
            Headers = new[] { "OrderDate", "CustomerName", "CustomerEmail", "ProductName", "CategoryName", "Quantity", "UnitPrice", "Status" }
        };
        
        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_CollectsErrors_ValidationFails()
    {
        // Arrange
        var context = new ImportContext 
        { 
            ParsedRows = new List<OrderImportRow> { new() { CustomerEmail = "invalid" } },
            Headers = new[] { "OrderDate", "CustomerName", "CustomerEmail", "ProductName", "CategoryName", "Quantity", "UnitPrice", "Status" }
        };
        
        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Errors.Should().NotBeEmpty();
        result.IsAborted.Should().BeTrue();
    }
}
