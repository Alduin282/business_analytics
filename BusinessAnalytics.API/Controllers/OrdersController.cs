using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Analytics;

namespace BusinessAnalytics.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public OrdersController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(List<AnalyticsPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] GroupPeriod groupBy = GroupPeriod.Month,
        [FromQuery] MetricType metric = MetricType.TotalAmount,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var timeZoneId = User.FindFirstValue("TimeZoneId");

        try
        {
            var result = await _analyticsService.GetAnalyticsAsync(
                userId, groupBy, metric, startDate, endDate, timeZoneId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

