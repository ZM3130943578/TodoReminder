using TodoReminder.Domain.Entities;

namespace TodoReminder.Infrastructure.Repositories.Interfaces;

public interface ITodoRepository
{
    Task<TodoTask?> GetTaskByIdAsync(Guid id);
    Task<TodoTask> AddTaskAsync(TodoTask task);
    Task UpdateTaskAsync(TodoTask task);

    Task<TodoDailyRecord?> GetRecordByIdAsync(Guid id);
    Task<List<TodoDailyRecord>> GetRecordsByDateAsync(DateOnly date);
    Task<List<TodoDailyRecord>> GetRecordsByDateRangeAsync(DateOnly from, DateOnly to);
    Task<TodoDailyRecord> AddRecordAsync(TodoDailyRecord record);
    Task UpdateRecordAsync(TodoDailyRecord record);
    Task DeleteRecordAsync(Guid id);
    Task<List<TodoDailyRecord>> GetPendingRecordsBeforeDateAsync(DateOnly date);
    Task<DateOnly?> GetMostRecentRecordDateAsync(DateOnly before);
    Task<List<TodoDailyRecord>> GetCompletedRecordsAsync();
}
