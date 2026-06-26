using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.Tests.Domain;

public class PopupScheduleTests
{
    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        var schedule = new PopupSchedule
        {
            Name = "Morning Reminder",
            ScheduleType = PopupScheduleType.Daily,
            TimeOfDay = new TimeOnly(9, 0),
            Message = "Check your todos"
        };

        Assert.NotEqual(Guid.Empty, schedule.Id);
        Assert.Equal("Morning Reminder", schedule.Name);
        Assert.True(schedule.Enabled);
        Assert.Equal(PopupScheduleType.Daily, schedule.ScheduleType);
        Assert.Equal(new TimeOnly(9, 0), schedule.TimeOfDay);
        Assert.Equal("Check your todos", schedule.Message);
        Assert.Null(schedule.LastTriggeredAt);
    }

    [Fact]
    public void OnceType_ShouldSetOnceAt()
    {
        var at = new DateTime(2026, 6, 25, 15, 0, 0, DateTimeKind.Utc);
        var schedule = new PopupSchedule
        {
            Name = "Meeting",
            ScheduleType = PopupScheduleType.Once,
            OnceAt = at,
            Message = "Meeting time"
        };

        Assert.Equal(PopupScheduleType.Once, schedule.ScheduleType);
        Assert.Equal(at, schedule.OnceAt);
        Assert.Null(schedule.TimeOfDay);
    }

    [Fact]
    public void IntervalType_ShouldSetIntervalMinutes()
    {
        var schedule = new PopupSchedule
        {
            Name = "Periodic Check",
            ScheduleType = PopupScheduleType.Interval,
            IntervalMinutes = 60,
            Message = "Check todos"
        };

        Assert.Equal(PopupScheduleType.Interval, schedule.ScheduleType);
        Assert.Equal(60, schedule.IntervalMinutes);
    }

    [Fact]
    public void WeeklyType_ShouldSetWeekdays()
    {
        var schedule = new PopupSchedule
        {
            Name = "Weekly Standup",
            ScheduleType = PopupScheduleType.Weekly,
            TimeOfDay = new TimeOnly(10, 0),
            Weekdays = "1,2,3,4,5",
            Message = "Daily standup"
        };

        Assert.Equal(PopupScheduleType.Weekly, schedule.ScheduleType);
        Assert.Equal("1,2,3,4,5", schedule.Weekdays);
    }
}
