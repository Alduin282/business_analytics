using System;

namespace BusinessAnalytics.API.Models;

public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public DateRange(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("Start date cannot be later than end date.");

        Start = start.Date;
        End = end.Date;
    }

    public static DateRange Create(DateTime? startDate, DateTime? endDate, TimeZoneInfo timeZone, int maxYearsLimit)
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var endLocal = endDate ?? nowLocal.Date;
        var startLocal = startDate ?? endLocal.AddYears(-1).Date;

        var start = startLocal.Date;
        var end = endLocal.Date.AddDays(1);

        if (start.AddYears(maxYearsLimit) < end)
            throw new ArgumentException($"Date range cannot exceed {maxYearsLimit} years.");

        return new DateRange(start, end);
    }
}
