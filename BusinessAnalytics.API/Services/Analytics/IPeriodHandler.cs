using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Services.Analytics;

public interface IPeriodHandler
{
    string GetLabel(DateTime localDate);
    bool IsPartial(DateTime localDate, DateRange range);
    DateTime AlignToStart(DateTime date);
    DateTime GetNext(DateTime date);
}
