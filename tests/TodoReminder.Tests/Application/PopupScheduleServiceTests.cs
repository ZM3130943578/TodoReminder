using TodoReminder.App.Services;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.Tests.Application;

public class PopupScheduleServiceTests
{
    private readonly DateTime _now = new(2026, 6, 25, 10, 0, 0);

    [Fact]
    public void IsDue_Once_ShouldTriggerWhenAtTime()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Once, OnceAt = _now };

        Assert.True(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Once_ShouldNotTriggerBeforeTime()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Once, OnceAt = _now.AddHours(1) };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Once_ShouldNotTriggerIfAlreadyTriggered()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Once, OnceAt = _now, LastTriggeredAt = _now };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Daily_ShouldTriggerWhenTimeReached()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Daily, TimeOfDay = new TimeOnly(10, 0) };

        Assert.True(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Daily_ShouldNotTriggerBeforeTime()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Daily, TimeOfDay = new TimeOnly(10, 30) };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Daily_ShouldNotTriggerTwiceSameDay()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Daily, TimeOfDay = new TimeOnly(10, 0), LastTriggeredAt = _now };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Weekly_ShouldTriggerOnCorrectDay()
    {
        var wednesday = new DateTime(2026, 6, 24, 10, 0, 0);
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Weekly, TimeOfDay = new TimeOnly(10, 0), Weekdays = "3" };

        Assert.True(PopupScheduleService.IsDue(s, wednesday));
    }

    [Fact]
    public void IsDue_Weekly_ShouldNotTriggerOnWrongDay()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Weekly, TimeOfDay = new TimeOnly(10, 0), Weekdays = "1" };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Interval_ShouldTriggerWhenPastInterval()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Interval, IntervalMinutes = 60, LastTriggeredAt = _now.AddMinutes(-61) };

        Assert.True(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Interval_ShouldNotTriggerBeforeInterval()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Interval, IntervalMinutes = 60, LastTriggeredAt = _now.AddMinutes(-30) };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Interval_ShouldTriggerIfNeverTriggered()
    {
        var s = new PopupSchedule { Enabled = true, ScheduleType = PopupScheduleType.Interval, IntervalMinutes = 60, LastTriggeredAt = null };

        Assert.True(PopupScheduleService.IsDue(s, _now));
    }

    [Fact]
    public void IsDue_Disabled_ShouldNotTrigger()
    {
        var s = new PopupSchedule { Enabled = false, ScheduleType = PopupScheduleType.Daily, TimeOfDay = new TimeOnly(10, 0) };

        Assert.False(PopupScheduleService.IsDue(s, _now));
    }
}
