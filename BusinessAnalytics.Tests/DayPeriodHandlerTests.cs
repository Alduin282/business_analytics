using System;
using BusinessAnalytics.API.Services.Analytics.Handlers;
using FluentAssertions;
using Xunit;

namespace BusinessAnalytics.Tests;

public class DayPeriodHandlerTests
{
    private readonly DayPeriodHandler _handler = new();

    [Fact]
    public void GetLabel_ReturnsCorrectFormat()
    {
        var date = new DateTime(2023, 1, 15);
        _handler.GetLabel(date).Should().Be("2023-01-15");
    }

    [Fact]
    public void IsPartial_AlwaysReturnsFalse()
    {
        // Day is never considered partial in this implementation
        var date = new DateTime(2023, 1, 15);
        _handler.IsPartial(date, null!).Should().BeFalse();
    }

    [Fact]
    public void AlignToStart_ReturnsSameDate()
    {
        var date = new DateTime(2023, 1, 15, 10, 30, 0);
        _handler.AlignToStart(date).Should().Be(new DateTime(2023, 1, 15));
    }

    [Fact]
    public void GetNext_ReturnsNextDay()
    {
        var date = new DateTime(2023, 1, 15);
        _handler.GetNext(date).Should().Be(new DateTime(2023, 1, 16));
    }
}
