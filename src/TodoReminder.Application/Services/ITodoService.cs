using TodoReminder.Domain.Enums;

namespace TodoReminder.Application.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoItemDto>> GetTodosByDateAsync(DateOnly date);
    Task<TodoItemDto> CreateTodoAsync(string title, string? note, DateOnly date, TimeOnly? dueTime);
    Task<TodoItemDto> UpdateTodoAsync(Guid recordId, string title, string? note, TimeOnly? dueTime);
    Task DeleteTodoAsync(Guid recordId);
    Task CompleteTodoAsync(Guid recordId);
    Task AbandonTodoAsync(Guid recordId);
    Task RestoreTodoAsync(Guid recordId);
    Task<List<HistoryItemDto>> GetHistoryAsync();
}

public class TodoItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateOnly RecordDate { get; set; }
    public TodoStatus Status { get; set; }
    public TimeOnly? DueTime { get; set; }
    public bool ReminderEnabled { get; set; }
    public Guid? InheritedFromRecordId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HistoryItemDto
{
    public Guid RecordId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateOnly RecordDate { get; set; }
    public TodoStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? AbandonedAt { get; set; }
    public DateTime StatusTime => (CompletedAt ?? AbandonedAt ?? DateTime.MinValue).ToLocalTime();
    public string StatusDisplay => Status switch
    {
        TodoStatus.Completed => "已完成",
        TodoStatus.Abandoned => "已废弃",
        _ => "未知"
    };
    public string StatusTimeDisplay => StatusTime.ToString("yyyy-MM-dd HH:mm:ss");
    public string RecordDateDisplay => RecordDate.ToString("yyyy-MM-dd");
}
