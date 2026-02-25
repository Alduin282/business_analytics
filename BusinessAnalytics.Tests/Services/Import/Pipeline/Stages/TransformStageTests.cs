using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline.Stages;

public class TransformStageTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly TransformStage _stage;
    private readonly string _userId = Guid.NewGuid().ToString();

    public TransformStageTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _stage = new TransformStage(_uowMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_TransformsRowsToOrders()
    {
        // Arrange
        var context = new ImportContext
        {
            UserId = _userId,
            ParsedRows = new List<OrderImportRow>
            {
                new() 
                { 
                    OrderDate = "2023-01-01 10:00", 
                    CustomerName = "John", 
                    CustomerEmail = "john@test.com",
                    ProductName = "Phone",
                    CategoryName = "Tech",
                    Quantity = "2",
                    UnitPrice = "500",
                    Status = "Pending"
                }
            }
        };

        var customerRepo = new Mock<IRepository<Customer, Guid>>();
        customerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer>());
        
        var categoryRepo = new Mock<IRepository<Category, int>>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());
        
        var productRepo = new Mock<IRepository<Product, Guid>>();
        productRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        _uowMock.Setup(u => u.Repository<Customer, Guid>()).Returns(customerRepo.Object);
        _uowMock.Setup(u => u.Repository<Category, int>()).Returns(categoryRepo.Object);
        _uowMock.Setup(u => u.Repository<Product, Guid>()).Returns(productRepo.Object);

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Orders.Should().HaveCount(1);
        var order = result.Orders[0];
        order.TotalAmount.Should().Be(1000);
        order.Items.Should().HaveCount(1);
        
        result.CustomersCreated.Should().HaveCount(1);
        result.CategoriesCreated.Should().HaveCount(1);
        result.ProductsCreated.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_GroupsSameOrder_ByDateAndEmail()
    {
        // Arrange
        var context = new ImportContext
        {
            UserId = _userId,
            ParsedRows = new List<OrderImportRow>
            {
                new() { OrderDate = "2023-01-01 10:00", CustomerEmail = "john@test.com", ProductName = "Item1", Quantity = "1", UnitPrice = "100", Status = "Pending", CategoryName = "Cat" },
                new() { OrderDate = "2023-01-01 10:00", CustomerEmail = "john@test.com", ProductName = "Item2", Quantity = "1", UnitPrice = "200", Status = "Pending", CategoryName = "Cat" }
            }
        };

        SetupEmptyMocks();

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.Orders.Should().HaveCount(1);
        result.Orders[0].Items.Should().HaveCount(2);
        result.Orders[0].TotalAmount.Should().Be(300);
    }

    private void SetupEmptyMocks()
    {
        var customerRepo = new Mock<IRepository<Customer, Guid>>();
        customerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer>());
        var categoryRepo = new Mock<IRepository<Category, int>>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());
        var productRepo = new Mock<IRepository<Product, Guid>>();
        productRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        _uowMock.Setup(u => u.Repository<Customer, Guid>()).Returns(customerRepo.Object);
        _uowMock.Setup(u => u.Repository<Category, int>()).Returns(categoryRepo.Object);
        _uowMock.Setup(u => u.Repository<Product, Guid>()).Returns(productRepo.Object);
    }
}
