using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using Microsoft.VisualBasic;

namespace BusinessAnalytics.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private const int maxYearsLimit = 5;

    private const string DefaultTimeZone = "UTC";

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(List<AnalyticsPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] GroupPeriod groupBy = GroupPeriod.Month,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Read timezone from JWT claim (set at login from user's DB record)
        var timeZoneId = User.FindFirstValue("TimeZoneId") ?? DefaultTimeZone;
        if (!TryResolveTimeZone(timeZoneId, out var tz))
            tz = TimeZoneInfo.Utc;

        if (!TryBuildDateRange(startDate, endDate, tz, out var start, out var end, out var validationError))
            return BadRequest(validationError);

        var rawOrders = await FetchOrdersFromDb(userId, start, end);
        var aggregatedDataDict = AggregateByPeriod(rawOrders, groupBy, tz);
        var result = FillGaps(aggregatedDataDict, start, end, groupBy, tz);

        return Ok(result);
    }

    private static bool TryResolveTimeZone(string timeZoneId, out TimeZoneInfo tz)
    {
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.Utc;
            return false;
        }
    }

    private static bool TryBuildDateRange(
        DateTime? startDate, DateTime? endDate,
        TimeZoneInfo tz,
        out DateTime start, out DateTime end,
        out string? error)
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var endLocal = endDate.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(endDate.Value, tz)
            : nowLocal;
        var startLocal = startDate.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(startDate.Value, tz)
            : endLocal.AddYears(-1);

        start = startLocal.Date;
        end   = endLocal.Date.AddDays(1);
        
        return ValidateDateRange(start, end, maxYearsLimit, out error);
    }

    private static bool ValidateDateRange(
        DateTime start,
        DateTime end,
        int maxYearsLimit,
        out string? error)
    {
        error = null;

        if (start > end)
        {
            error = "startDate cannot be later than endDate.";
            return false;
        }

        if ((end - start).TotalDays > 365 * maxYearsLimit)
        {
            error = $"Date range cannot exceed {maxYearsLimit} years.";
            return false;
        }

        return true;
    }

    private async Task<List<(DateTime OrderDate, decimal TotalAmount)>> FetchOrdersFromDb(
        string userId, DateTime start, DateTime end)
    {
        return await _unitOfWork.Repository<Order, Guid>()
            .Query()
            .Where(o => o.UserId == userId &&
                        o.OrderDate >= start &&
                        o.OrderDate < end &&
                        o.Status != OrderStatus.Cancelled)
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync()
            .ContinueWith(t => t.Result
                .Select(o => (o.OrderDate, o.TotalAmount))
                .ToList());
    }

    private static Dictionary<string, decimal> AggregateByPeriod(
        List<(DateTime OrderDate, decimal TotalAmount)> orders,
        GroupPeriod groupBy,
        TimeZoneInfo tz)
    {
        return orders
            .GroupBy(o => GetGroupLabel(TimeZoneInfo.ConvertTimeFromUtc(o.OrderDate, tz), groupBy))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
    }

    private static List<AnalyticsPoint> FillGaps(
        Dictionary<string, decimal> dataDict,
        DateTime start,
        DateTime end,
        GroupPeriod groupBy,
        TimeZoneInfo tz)
    {
        var result = new List<AnalyticsPoint>();
        var seenLabels = new HashSet<string>();

        var currentUtc = AlignToStartOfPeriod(start.Date, groupBy);

        while (currentUtc < end.Date)
        {
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(currentUtc, DateTimeKind.Utc), tz);
            var label = GetGroupLabel(localDate, groupBy);

            if (seenLabels.Add(label))
            {
                result.Add(new AnalyticsPoint(
                    Label: label,
                    TotalAmount: dataDict.GetValueOrDefault(label, 0m)));
            }

            currentUtc = currentUtc.AddDays(1);
        }

        return result;
    }

    private static DateTime AlignToStartOfPeriod(DateTime date, GroupPeriod groupBy)
    {
        if (groupBy != GroupPeriod.Week) return date;
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private static string GetGroupLabel(DateTime localDate, GroupPeriod groupBy) =>
    groupBy switch
    {
        GroupPeriod.Day   => localDate.ToString("yyyy-MM-dd"),
        GroupPeriod.Week  => GetWeekRangeLabel(localDate),
        GroupPeriod.Month => localDate.ToString("yyyy-MM"),
        _                 => localDate.ToString("yyyy-MM-dd")
    };

    private static string GetWeekRangeLabel(DateTime localDate)
    {
        var startOfWeek = ISOWeek.ToDateTime(ISOWeek.GetYear(localDate), ISOWeek.GetWeekOfYear(localDate), DayOfWeek.Monday);
        var endOfWeek = startOfWeek.AddDays(6);
        return $"{startOfWeek:dd.MM} - {endOfWeek:dd.MM}";
    }
}
