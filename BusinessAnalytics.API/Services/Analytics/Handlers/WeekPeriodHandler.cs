using System.Globalization;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics.Handlers;

public class WeekPeriodHandler : IPeriodHandler
{
    public string GetLabel(DateTime localDate)
    {
        var startOfWeek = ISOWeek.ToDateTime(ISOWeek.GetYear(localDate), ISOWeek.GetWeekOfYear(localDate), DayOfWeek.Monday);
        var endOfWeek = startOfWeek.AddDays(6);
        return $"{startOfWeek:dd.MM} - {endOfWeek:dd.MM}";
    }

    public bool IsPartial(DateTime localDate, DateRange range)
    {
        int diff = (7 + (localDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        var periodStart = localDate.AddDays(-diff).Date;
        var periodEnd = periodStart.AddDays(7);
        return range.Start > periodStart || range.End < periodEnd;
    }

    public DateTime AlignToStart(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    public DateTime GetNext(DateTime date) => date.AddDays(7);
}
