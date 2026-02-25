using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics.Handlers;

public class DayPeriodHandler : IPeriodHandler
{
    public string GetLabel(DateTime localDate) => localDate.ToString("yyyy-MM-dd");
    public bool IsPartial(DateTime localDate, DateRange range) => false;
    public DateTime AlignToStart(DateTime date) => date.Date;
    public DateTime GetNext(DateTime date) => date.AddDays(1);
}
