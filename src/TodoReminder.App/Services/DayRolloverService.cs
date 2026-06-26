using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class DayRolloverService
{
    private readonly ITodoRepository _repository;

    public DayRolloverService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task EnsureRolloverAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var lastDate = await _repository.GetMostRecentRecordDateAsync(today);

        if (lastDate == null || lastDate.Value >= today)
            return;

        var pendingRecords = await _repository.GetRecordsByDateAsync(lastDate.Value);
        var pending = pendingRecords
            .Where(r => r.Status == TodoStatus.Pending)
            .ToList();

        if (pending.Count == 0)
            return;

        var todayRecords = await _repository.GetRecordsByDateAsync(today);
        var todayTaskIds = todayRecords.Select(r => r.TaskId).ToHashSet();

        foreach (var record in pending)
        {
            var task = await _repository.GetTaskByIdAsync(record.TaskId);
            if (task == null || task.DeletedAt != null)
                continue;

            if (todayTaskIds.Contains(record.TaskId))
                continue;

            var inherited = new TodoDailyRecord(record.TaskId, today)
            {
                DueTime = record.DueTime,
                ReminderEnabled = record.ReminderEnabled,
                InheritedFromRecordId = record.Id,
                SortOrder = record.SortOrder
            };

            await _repository.AddRecordAsync(inherited);
        }
    }
}
