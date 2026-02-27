using System.Security.Claims;
using BusinessAnalytics.API.Controllers;
using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using BusinessAnalytics.API.Services.Events;

namespace BusinessAnalytics.Tests;

public class ImportControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly Mock<IImportEventDispatcher> _dispatcherMock;
    private readonly ImportController _controller;
    private const string TestUserId = "user-123";

    public ImportControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_context);
        _dispatcherMock = new Mock<IImportEventDispatcher>();
        
        var pipeline = new ImportPipeline(new List<IImportPipelineStage>(), _dispatcherMock.Object);
        
        _controller = new ImportController(pipeline, _uow, _dispatcherMock.Object);
        
        SetupUser(TestUserId);
    }

    private void SetupUser(string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetupAnonymousUser()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetHistory_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _controller.GetHistory();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetHistory_NoHistory_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetHistory();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeAssignableTo<IEnumerable<ImportSessionDto>>().Subject;
        data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_UserHistoryExists_ReturnsCorrectData()
    {
        // Arrange
        var otherUser = "other-user";
        var mySession = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = TestUserId, 
            FileName = "my-file.csv", 
            ImportedAt = DateTime.UtcNow,
            FileHash = "hash1",
            OrdersCount = 10,
            ItemsCount = 20
        };
        var otherSession = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = otherUser, 
            FileName = "other-file.csv", 
            ImportedAt = DateTime.UtcNow,
            FileHash = "hash2"
        };

        _context.ImportSessions.AddRange(mySession, otherSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetHistory();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeAssignableTo<IEnumerable<ImportSessionDto>>().Subject.ToList();
        
        data.Should().HaveCount(1);
        data[0].FileName.Should().Be("my-file.csv");
        data[0].OrdersCount.Should().Be(10);
        data[0].ItemsCount.Should().Be(20);
    }

    [Fact]
    public async Task GetHistory_OrderedByDateDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var sessionOld = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = TestUserId, 
            FileName = "old.csv", 
            ImportedAt = now.AddDays(-1),
            FileHash = "hash-old"
        };
        var sessionNew = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = TestUserId, 
            FileName = "new.csv", 
            ImportedAt = now,
            FileHash = "hash-new"
        };

        _context.ImportSessions.AddRange(sessionOld, sessionNew);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetHistory();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeAssignableTo<IEnumerable<ImportSessionDto>>().Subject.ToList();
        
        data.Should().HaveCount(2);
        data[0].FileName.Should().Be("new.csv");
        data[1].FileName.Should().Be("old.csv");
    }

    [Fact]
    public async Task Rollback_ValidSession_TogglesFlag()
    {
        // Arrange
        var session = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = TestUserId, 
            FileName = "test.csv", 
            FileHash = "hash1",
            IsRolledBack = false
        };
        _context.ImportSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Rollback(session.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.ToString();
        data.Should().Contain("isRolledBack = True");
        
        var updatedSession = await _context.ImportSessions.FindAsync(session.Id);
        updatedSession.IsRolledBack.Should().BeTrue();

        _dispatcherMock.Verify(d => d.NotifyAsync(It.Is<ImportActivityEvent>(e => 
            e.UserId == TestUserId && 
            e.Action == ImportAction.RolledBack && 
            e.SessionId == session.Id)), Times.Once);
    }

    [Fact]
    public async Task Rollback_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        SetupAnonymousUser();

        // Act
        var result = await _controller.Rollback(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Rollback_OtherUserSession_ReturnsNotFound()
    {
        // Arrange
        var otherSession = new ImportSession 
        { 
            Id = Guid.NewGuid(), 
            UserId = "other-user", 
            FileName = "other.csv", 
            FileHash = "hash2"
        };
        _context.ImportSessions.Add(otherSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Rollback(otherSession.Id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
