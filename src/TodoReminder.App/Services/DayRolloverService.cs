using System.Windows.Threading;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class DayRolloverService
{
    private readonly ITodoRepository _repository;
    private readonly DispatcherTimer _timer;

    public event EventHandler? RolloverCompleted;

    public DayRolloverService(ITodoRepository repository)
    {
        _repository = repository;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += OnTimerTick;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        try
        {
            var rolled = await EnsureRolloverAsync();
            if (rolled)
                RolloverCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _timer.Start();
        }
    }

    public async Task<bool> EnsureRolloverAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var lastDate = await _repository.GetMostRecentRecordDateAsync(today);

        if (lastDate == null || lastDate.Value >= today)
            return false;

        var pendingRecords = await _repository.GetRecordsByDateAsync(lastDate.Value);
        var pending = pendingRecords
            .Where(r => r.Status == TodoStatus.Pending)
            .ToList();

        if (pending.Count == 0)
            return false;

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

        return true;
    }
}
