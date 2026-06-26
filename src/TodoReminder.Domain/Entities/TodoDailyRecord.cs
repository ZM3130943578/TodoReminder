using TodoReminder.Domain.Enums;

namespace TodoReminder.Domain.Entities;

public class TodoDailyRecord
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public DateOnly RecordDate { get; set; }

    public TodoStatus Status { get; set; }

    public TimeOnly? DueTime { get; set; }

    public bool ReminderEnabled { get; set; }

    public DateTime? ReminderFiredAt { get; set; }

    public Guid? InheritedFromRecordId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? AbandonedAt { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public TodoDailyRecord()
    {
        Id = Guid.NewGuid();
        Status = TodoStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public TodoDailyRecord(Guid taskId, DateOnly recordDate) : this()
    {
        TaskId = taskId;
        RecordDate = recordDate;
    }
}
