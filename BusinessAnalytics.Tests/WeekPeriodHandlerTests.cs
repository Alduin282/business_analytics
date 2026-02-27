using System;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Services.Analytics.Handlers;
using FluentAssertions;
using Xunit;

namespace BusinessAnalytics.Tests;

public class WeekPeriodHandlerTests
{
    private readonly WeekPeriodHandler _weekPeriodHandler = new();

    [Fact]
    public void GetLabel_ReturnsCorrectFormat()
    {
        var date = new DateTime(2026, 2, 4); // Wednesday
        _weekPeriodHandler.GetLabel(date).Should().Be("02.02 - 08.02");
    }

    [Fact]
    public void AlignToStart_AlignsToMonday()
    {
        var date = new DateTime(2026, 2, 4); // Wednesday
        _weekPeriodHandler.AlignToStart(date).Should().Be(new DateTime(2026, 2, 2));
    }

    [Fact]
    public void GetNext_ReturnsOneWeekLater()
    {
        var date = new DateTime(2026, 2, 2);
        _weekPeriodHandler.GetNext(date).Should().Be(new DateTime(2026, 2, 9));
    }

    [Fact]
    public void IsPartial_DetectsPartialRangeAtStart()
    {
        var range = new DateRange(new DateTime(2026, 2, 3), new DateTime(2026, 2, 8));
        var weekDate = new DateTime(2026, 2, 2); // Monday
        
        _weekPeriodHandler.IsPartial(weekDate, range).Should().BeTrue();
    }

    [Fact]
    public void IsPartial_DetectsPartialRangeAtEnd()
    {
        var range = new DateRange(new DateTime(2026, 2, 2), new DateTime(2026, 2, 7));
        var weekDate = new DateTime(2026, 2, 2); // Monday
        
        _weekPeriodHandler.IsPartial(weekDate, range).Should().BeTrue();
    }

    [Fact]
    public void IsPartial_DetectsFullWeek()
    {
        var range = new DateRange(new DateTime(2026, 2, 2), new DateTime(2026, 2, 9));
        var weekDate = new DateTime(2026, 2, 2); // Monday
        
        _weekPeriodHandler.IsPartial(weekDate, range).Should().BeFalse();
    }
}
