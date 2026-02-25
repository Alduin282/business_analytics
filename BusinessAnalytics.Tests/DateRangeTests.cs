using BusinessAnalytics.API.Models;
using FluentAssertions;

namespace BusinessAnalytics.Tests;

public class DateRangeTests
{
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    [Fact]
    public void Constructor_ThrowsOnInvalidRange()
    {
        Action act = () => new DateRange(new DateTime(2023, 1, 10), new DateTime(2023, 1, 1));
        act.Should().Throw<ArgumentException>().WithMessage("Start date cannot be later than end date.");
    }

    [Fact]
    public void Constructor_SetsDateProperties()
    {
        var start = new DateTime(2023, 1, 1, 10, 0, 0);
        var end = new DateTime(2023, 1, 2, 10, 0, 0);
        var range = new DateRange(start, end);

        range.Start.Should().Be(new DateTime(2023, 1, 1));
        range.End.Should().Be(new DateTime(2023, 1, 2));
    }

    [Fact]
    public void Create_EnforcesMaxYearsLimit()
    {
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2026, 1, 2);
        var maxYearsLimit = 5;
        
        Action act = () => DateRange.Create(start, end, Utc, maxYearsLimit);
        act.Should().Throw<ArgumentException>().WithMessage($"Date range cannot exceed {maxYearsLimit} years.");
    }

    [Fact]
    public void Create_AcceptsValidRange()
    {
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 2);
        
        var range = DateRange.Create(start, end, Utc, 5);

        // [start, end) range
        range.Start.Should().Be(new DateTime(2023, 1, 1));
        range.End.Should().Be(new DateTime(2023, 1, 2).AddDays(1));
    }

}
