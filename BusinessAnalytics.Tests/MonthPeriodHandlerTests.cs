using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Services.Analytics.Handlers;

namespace BusinessAnalytics.Tests;

public class MonthPeriodHandlerTests
{
    private readonly MonthPeriodHandler _mouthPeriodHandler = new();

    [Fact]
    public void GetLabel_ReturnsCorrectFormat()
    {
        var date = new DateTime(2023, 1, 15);
        _mouthPeriodHandler.GetLabel(date).Should().Be("2023-01");
    }

    [Fact]
    public void AlignToStart_AlignsToFirstDayOfMonth()
    {
        var date = new DateTime(2023, 1, 15);
        _mouthPeriodHandler.AlignToStart(date).Should().Be(new DateTime(2023, 1, 1));
    }

    [Fact]
    public void GetNext_ReturnsFirstDayOfNextMonth()
    {
        var date = new DateTime(2023, 1, 1);
        _mouthPeriodHandler.GetNext(date).Should().Be(new DateTime(2023, 2, 1));
    }

    [Fact]
    public void IsPartial_DetectsPartialRange_StartMidMonth()
    {
        var range = new DateRange(new DateTime(2023, 1, 15), new DateTime(2023, 1, 31));
        var monthDate = new DateTime(2023, 1, 1);
        
        _mouthPeriodHandler.IsPartial(monthDate, range).Should().BeTrue();
    }

    [Fact]
    public void IsPartial_DetectsPartialRange_EndMidMonth()
    {
        var range = new DateRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 15));
        var monthDate = new DateTime(2023, 1, 1);
        
        _mouthPeriodHandler.IsPartial(monthDate, range).Should().BeTrue();
    }

    [Fact]
    public void IsPartial_DetectsFullMonth()
    {
        var range = new DateRange(new DateTime(2023, 1, 1), new DateTime(2023, 2, 1));
        var monthDate = new DateTime(2023, 1, 1);
        
        _mouthPeriodHandler.IsPartial(monthDate, range).Should().BeFalse();
    }
}
