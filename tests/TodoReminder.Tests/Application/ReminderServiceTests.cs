using TodoReminder.App.Services;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.Tests.Application;

public class ReminderServiceTests
{
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Now);
    private readonly TimeOnly _now = TimeOnly.FromDateTime(DateTime.Now);

    [Fact]
    public void FindDueRecords_ShouldReturnPendingDueItem()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Single(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipCompleted()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Completed, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipAbandoned()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Abandoned, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipReminderDisabled()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = false,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipNoDueTime()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = null, ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipFutureDueTime()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(10), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipAlreadyFired()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = DateTime.UtcNow }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldSkipDifferentDate()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today.AddDays(-1), Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Empty(due);
    }

    [Fact]
    public void FindDueRecords_ShouldHandleMultipleDueItems()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-10), ReminderFiredAt = null },
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null },
            new() { RecordDate = _today, Status = TodoStatus.Completed, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-3), ReminderFiredAt = null }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        Assert.Equal(2, due.Count);
    }

    [Fact]
    public void FindDueRecords_ShouldReturnInReadOrder()
    {
        var records = new List<TodoDailyRecord>
        {
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-5), ReminderFiredAt = null, SortOrder = 2 },
            new() { RecordDate = _today, Status = TodoStatus.Pending, ReminderEnabled = true,
                    DueTime = _now.AddMinutes(-10), ReminderFiredAt = null, SortOrder = 1 }
        };

        var due = ReminderService.FindDueRecords(records, _today, _now);

        // Should maintain the order from the input list
        Assert.Equal(2, due.Count);
    }
}
