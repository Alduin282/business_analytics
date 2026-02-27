using BusinessAnalytics.API.Services.Analytics.Handlers;

namespace BusinessAnalytics.Tests;

public class DayPeriodHandlerTests
{
    private readonly DayPeriodHandler _dayPeriodHandler = new();

    [Fact]
    public void GetLabel_ReturnsCorrectFormat()
    {
        var date = new DateTime(2023, 1, 15);
        _dayPeriodHandler.GetLabel(date).Should().Be("2023-01-15");
    }

    [Fact]
    public void IsPartial_AlwaysReturnsFalse()
    {
        // Day is never considered partial in this implementation
        var date = new DateTime(2023, 1, 15);
        _dayPeriodHandler.IsPartial(date, null!).Should().BeFalse();
    }

    [Fact]
    public void AlignToStart_ReturnsSameDate()
    {
        var date = new DateTime(2023, 1, 15, 10, 30, 0);
        _dayPeriodHandler.AlignToStart(date).Should().Be(new DateTime(2023, 1, 15));
    }

    [Fact]
    public void GetNext_ReturnsNextDay()
    {
        var date = new DateTime(2023, 1, 15);
        _dayPeriodHandler.GetNext(date).Should().Be(new DateTime(2023, 1, 16));
    }
}
