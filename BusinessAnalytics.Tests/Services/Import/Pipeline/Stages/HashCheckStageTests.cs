using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.Tests.Services.Import.Pipeline.Stages;

public class HashCheckStageTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly HashCheckStage _hashCheckStage;
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

    public HashCheckStageTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _uowMock = new Mock<IUnitOfWork>();
        _hashCheckStage = new HashCheckStage(_uowMock.Object);
    }

    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_dbOptions);

    [Fact]
    public async Task ExecuteAsync_CalculatesHashAndContinues_WhenNoDuplicateFound()
    {
        // Arrange
        var content = "test file content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var context = new ImportContext
        {
            FileStream = stream,
            UserId = "user-1",
            FileName = "test.csv"
        };

        using var dbContext = CreateContext();
        var repo = new Repository<ImportSession, Guid>(dbContext);
        _uowMock.Setup(u => u.Repository<ImportSession, Guid>()).Returns(repo);

        // Act
        await _hashCheckStage.ExecuteAsync(context);

        // Assert
        context.FileHash.Should().NotBeEmpty();
        context.IsAborted.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_Aborts_WhenDuplicateHashFoundForSameUser()
    {
        // Arrange
        var content = "duplicate content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        // Pre-calculate hash for the content
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        var expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        var context = new ImportContext
        {
            FileStream = stream,
            UserId = "user-1",
            FileName = "test.csv"
        };

        using var dbContext = CreateContext();
        await dbContext.ImportSessions.AddAsync(new ImportSession 
        { 
            UserId = "user-1", 
            FileHash = expectedHash, 
            ImportedAt = DateTime.UtcNow, 
            Id = Guid.NewGuid(),
            FileName = "old.csv"
        });
        await dbContext.SaveChangesAsync();

        var repo = new Repository<ImportSession, Guid>(dbContext);
        _uowMock.Setup(u => u.Repository<ImportSession, Guid>()).Returns(repo);

        // Act
        await _hashCheckStage.ExecuteAsync(context);

        // Assert
        context.IsAborted.Should().BeTrue();
        context.Errors.Should().Contain(e => e.Message.Contains("already been imported"));
    }

    [Fact]
    public async Task ExecuteAsync_Continues_WhenDuplicateHashFoundForDifferentUser()
    {
        // Arrange
        var content = "shared content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        var expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        var context = new ImportContext
        {
            FileStream = stream,
            UserId = "user-2", // Different user
            FileName = "test.csv"
        };

        using var dbContext = CreateContext();
        await dbContext.ImportSessions.AddAsync(new ImportSession 
        { 
            UserId = "user-1", 
            FileHash = expectedHash, 
            ImportedAt = DateTime.UtcNow, 
            Id = Guid.NewGuid(),
            FileName = "other.csv"
        });
        await dbContext.SaveChangesAsync();

        var repo = new Repository<ImportSession, Guid>(dbContext);
        _uowMock.Setup(u => u.Repository<ImportSession, Guid>()).Returns(repo);

        // Act
        await _hashCheckStage.ExecuteAsync(context);

        // Assert
        context.IsAborted.Should().BeFalse();
        context.Errors.Should().BeEmpty();
    }
}
