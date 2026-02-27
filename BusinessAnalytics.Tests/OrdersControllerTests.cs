using System.Security.Claims;
using BusinessAnalytics.API.Controllers;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Analytics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BusinessAnalytics.Tests;

public class OrdersControllerTests : IDisposable
{
    private readonly Mock<IAnalyticsService> _analyticsServiceMock;
    private readonly OrdersController _controller;
    private const string TestUserId = "user-123";

    public OrdersControllerTests()
    {
        _analyticsServiceMock = new Mock<IAnalyticsService>();
        _controller = new OrdersController(_analyticsServiceMock.Object);
        
        SetupUser(TestUserId, "UTC");
    }

    private void SetupUser(string userId, string timeZoneId)
    {
        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(userId))
            claims.Add(new(ClaimTypes.NameIdentifier, userId));
            
        if (!string.IsNullOrEmpty(timeZoneId))
            claims.Add(new("TimeZoneId", timeZoneId));
            
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAnalytics_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var expectedAnalytics = new List<AnalyticsPoint>
        {
            new("2023-01", 100, false),
            new("2023-02", 200, false)
        };
        
        _analyticsServiceMock
            .Setup(x => x.GetAnalyticsAsync(
                TestUserId, 
                GroupPeriod.Month, 
                MetricType.TotalAmount,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                "UTC"))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(
            groupBy: GroupPeriod.Month, 
            metric: MetricType.TotalAmount,
            startDate: new DateTime(2023, 1, 1),
            endDate: new DateTime(2023, 2, 28));

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<List<AnalyticsPoint>>().Subject;
        
        data.Should().HaveCount(2);
        data.Should().BeEquivalentTo(expectedAnalytics);
        
        _analyticsServiceMock.Verify(
            x => x.GetAnalyticsAsync(
                TestUserId, 
                GroupPeriod.Month, 
                MetricType.TotalAmount,
                new DateTime(2023, 1, 1),
                new DateTime(2023, 2, 28),
                "UTC"),
            Times.Once);
    }

    [Fact]
    public async Task GetAnalytics_WithInvalidUser_ReturnsUnauthorized()
    {
        // Arrange
        SetupUser(null, "UTC"); // No user ID

        // Act
        var result = await _controller.GetAnalytics();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        
        _analyticsServiceMock.Verify(
            x => x.GetAnalyticsAsync(It.IsAny<string>(), It.IsAny<GroupPeriod>(), It.IsAny<MetricType>(), 
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAnalytics_WithServiceException_ReturnsBadRequest()
    {
        // Arrange
        _analyticsServiceMock
            .Setup(x => x.GetAnalyticsAsync(
                TestUserId, 
                It.IsAny<GroupPeriod>(), 
                It.IsAny<MetricType>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Invalid date range"));

        // Act
        var result = await _controller.GetAnalytics();

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Invalid date range");
    }

    public void Dispose()
    {
        // No cleanup needed with mocked dependencies
    }
}
