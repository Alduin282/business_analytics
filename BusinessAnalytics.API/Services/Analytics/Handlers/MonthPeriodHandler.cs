using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics.Handlers;

public class MonthPeriodHandler : IPeriodHandler
{
    public string GetLabel(DateTime localDate) => localDate.ToString("yyyy-MM");

    public bool IsPartial(DateTime localDate, DateRange range)
    {
        var periodStart = new DateTime(localDate.Year, localDate.Month, 1);
        var periodEnd = periodStart.AddMonths(1);
        return range.Start > periodStart || range.End < periodEnd;
    }

    public DateTime AlignToStart(DateTime date) => new(date.Year, date.Month, 1);

    public DateTime GetNext(DateTime date) => date.AddMonths(1);
}
