using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using FluentAssertions;
using Moq;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline.Stages;

public class PersistStageTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PersistStage _stage;

    public PersistStageTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _stage = new PersistStage(_uowMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsAllEntitiesAndCompletes()
    {
        // Arrange
        var context = new ImportContext
        {
            UserId = "user-1",
            CategoriesCreated = new List<Category> { new() { Name = "Cat" } },
            CustomersCreated = new List<Customer> { new() { FullName = "Name" } },
            ProductsCreated = new List<Product> { new() { Name = "Prod" } },
            Orders = new List<Order> { new() { TotalAmount = 100 } }
        };

        var categoryRepo = new Mock<IRepository<Category, int>>();
        var customerRepo = new Mock<IRepository<Customer, Guid>>();
        var productRepo = new Mock<IRepository<Product, Guid>>();
        var orderRepo = new Mock<IRepository<Order, Guid>>();
        var sessionRepo = new Mock<IRepository<ImportSession, Guid>>();

        _uowMock.Setup(u => u.Repository<Category, int>()).Returns(categoryRepo.Object);
        _uowMock.Setup(u => u.Repository<Customer, Guid>()).Returns(customerRepo.Object);
        _uowMock.Setup(u => u.Repository<Product, Guid>()).Returns(productRepo.Object);
        _uowMock.Setup(u => u.Repository<Order, Guid>()).Returns(orderRepo.Object);
        _uowMock.Setup(u => u.Repository<ImportSession, Guid>()).Returns(sessionRepo.Object);

        // Act
        await _stage.ExecuteAsync(context);

        // Assert
        categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        customerRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        productRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        sessionRepo.Verify(r => r.AddAsync(It.IsAny<ImportSession>()), Times.Once);
        
        _uowMock.Verify(u => u.CompleteAsync(), Times.Once);
        context.IsAborted.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenExceptionOccurs()
    {
        // Arrange
        var context = new ImportContext();
        var sessionRepo = new Mock<IRepository<ImportSession, Guid>>();
        _uowMock.Setup(u => u.Repository<ImportSession, Guid>()).Returns(sessionRepo.Object);
        _uowMock.Setup(u => u.Repository<Category, int>()).Throws(new Exception("DB Down"));

        // Act
        var result = await _stage.ExecuteAsync(context);

        // Assert
        result.IsAborted.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("DB Down"));
    }
}
