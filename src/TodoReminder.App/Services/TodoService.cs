using TodoReminder.Application.Services;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TodoItemDto>> GetTodosByDateAsync(DateOnly date)
    {
        var records = await _repository.GetRecordsByDateAsync(date);
        var taskIds = records.Select(r => r.TaskId).Distinct().ToArray();
        var tasks = new Dictionary<Guid, TodoTask>();
        foreach (var taskId in taskIds)
        {
            var task = await _repository.GetTaskByIdAsync(taskId);
            if (task != null)
                tasks[task.Id] = task;
        }
        return records.Select(r => MapToDto(r, tasks.GetValueOrDefault(r.TaskId))).ToList();
    }

    public async Task<TodoItemDto> CreateTodoAsync(string title, string? note, DateOnly date, TimeOnly? dueTime)
    {
        var task = new TodoTask(title, note);
        await _repository.AddTaskAsync(task);

        var record = new TodoDailyRecord(task.Id, date)
        {
            DueTime = dueTime,
            ReminderEnabled = dueTime.HasValue
        };
        await _repository.AddRecordAsync(record);

        return MapToDto(record, task);
    }

    public async Task<TodoItemDto> UpdateTodoAsync(Guid recordId, string title, string? note, TimeOnly? dueTime)
    {
        var record = await _repository.GetRecordByIdAsync(recordId);
        if (record == null)
            throw new InvalidOperationException($"Record {recordId} not found");

        var task = await _repository.GetTaskByIdAsync(record.TaskId);
        if (task == null)
            throw new InvalidOperationException($"Task {record.TaskId} not found");

        task.Title = title;
        task.Note = note;
        await _repository.UpdateTaskAsync(task);

        record.DueTime = dueTime;
        record.ReminderEnabled = dueTime.HasValue;
        await _repository.UpdateRecordAsync(record);

        return MapToDto(record, task);
    }

    public async Task DeleteTodoAsync(Guid recordId)
    {
        await _repository.DeleteRecordAsync(recordId);
    }

    public async Task CompleteTodoAsync(Guid recordId)
    {
        var record = await _repository.GetRecordByIdAsync(recordId);
        if (record == null) return;

        record.Status = TodoStatus.Completed;
        record.CompletedAt = DateTime.UtcNow;
        record.ReminderFiredAt = null;
        await _repository.UpdateRecordAsync(record);
    }

    public async Task AbandonTodoAsync(Guid recordId)
    {
        var record = await _repository.GetRecordByIdAsync(recordId);
        if (record == null) return;

        record.Status = TodoStatus.Abandoned;
        record.AbandonedAt = DateTime.UtcNow;
        record.ReminderFiredAt = null;
        await _repository.UpdateRecordAsync(record);
    }

    public async Task RestoreTodoAsync(Guid recordId)
    {
        var record = await _repository.GetRecordByIdAsync(recordId);
        if (record == null) return;

        record.Status = TodoStatus.Pending;
        record.CompletedAt = null;
        record.AbandonedAt = null;
        await _repository.UpdateRecordAsync(record);
    }

    private static TodoItemDto MapToDto(TodoDailyRecord record, TodoTask? task)
    {
        return new TodoItemDto
        {
            Id = record.Id,
            Title = task?.Title ?? string.Empty,
            Note = task?.Note,
            RecordDate = record.RecordDate,
            Status = record.Status,
            DueTime = record.DueTime,
            ReminderEnabled = record.ReminderEnabled,
            InheritedFromRecordId = record.InheritedFromRecordId,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }
}
