using TodoReminder.Domain.Enums;

namespace TodoReminder.Domain.Entities;

public class PopupSchedule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public PopupScheduleType ScheduleType { get; set; }

    public TimeOnly? TimeOfDay { get; set; }

    public DateTime? OnceAt { get; set; }

    public int? IntervalMinutes { get; set; }

    public string? Weekdays { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime? LastTriggeredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PopupSchedule()
    {
        Id = Guid.NewGuid();
        Enabled = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
