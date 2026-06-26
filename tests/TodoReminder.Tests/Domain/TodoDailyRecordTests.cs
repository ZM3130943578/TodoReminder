using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.Tests.Domain;

public class TodoDailyRecordTests
{
    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var taskId = Guid.NewGuid();
        var record = new TodoDailyRecord(taskId, new DateOnly(2026, 6, 25));

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(taskId, record.TaskId);
        Assert.Equal(new DateOnly(2026, 6, 25), record.RecordDate);
        Assert.Equal(TodoStatus.Pending, record.Status);
        Assert.False(record.ReminderEnabled);
        Assert.Null(record.DueTime);
        Assert.Null(record.ReminderFiredAt);
        Assert.Null(record.InheritedFromRecordId);
        Assert.Null(record.CompletedAt);
        Assert.Null(record.AbandonedAt);
        Assert.Equal(0, record.SortOrder);
    }

    [Fact]
    public void PendingStatus_ShouldAllowReminder()
    {
        var record = new TodoDailyRecord(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        record.ReminderEnabled = true;
        record.DueTime = new TimeOnly(10, 30);

        Assert.Equal(TodoStatus.Pending, record.Status);
        Assert.True(record.ReminderEnabled);
        Assert.NotNull(record.DueTime);
    }

    [Fact]
    public void Completed_ShouldSetCompletedAt()
    {
        var record = new TodoDailyRecord(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        record.Status = TodoStatus.Completed;
        record.CompletedAt = DateTime.UtcNow;

        Assert.Equal(TodoStatus.Completed, record.Status);
        Assert.NotNull(record.CompletedAt);
    }

    [Fact]
    public void Abandoned_ShouldSetAbandonedAt()
    {
        var record = new TodoDailyRecord(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        record.Status = TodoStatus.Abandoned;
        record.AbandonedAt = DateTime.UtcNow;

        Assert.Equal(TodoStatus.Abandoned, record.Status);
        Assert.NotNull(record.AbandonedAt);
    }

    [Fact]
    public void InheritedRecord_ShouldSetSource()
    {
        var sourceId = Guid.NewGuid();
        var record = new TodoDailyRecord(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));
        record.InheritedFromRecordId = sourceId;

        Assert.Equal(sourceId, record.InheritedFromRecordId);
    }
}
