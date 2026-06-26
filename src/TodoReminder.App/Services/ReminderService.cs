using System.Windows.Threading;
using TodoReminder.App.ViewModels;
using TodoReminder.App.Views;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class ReminderService : IDisposable
{
    private readonly ITodoRepository _repository;
    private readonly MainWindow _mainWindow;
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _timer;

    public ReminderService(ITodoRepository repository, MainWindow mainWindow, MainViewModel viewModel)
    {
        _repository = repository;
        _mainWindow = mainWindow;
        _viewModel = viewModel;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += OnTimerTick;
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void Dispose() => Stop();

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        try
        {
            await CheckDueRemindersAsync();
        }
        finally
        {
            _timer.Start();
        }
    }

    public async Task CheckDueRemindersAsync()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        var records = await _repository.GetRecordsByDateAsync(today);
        var dueRecords = FindDueRecords(records, today, currentTime);

        foreach (var record in dueRecords)
        {
            var task = await _repository.GetTaskByIdAsync(record.TaskId);
            if (task?.DeletedAt != null) continue;

            record.ReminderFiredAt = DateTime.UtcNow;
            await _repository.UpdateRecordAsync(record);

            var action = ReminderPopup.Show(task!.Title, task.Note, record.DueTime, _mainWindow);

            await HandleReminderActionAsync(record, action);
        }

        if (dueRecords.Count > 0)
        {
            ShowMainWindow(dueRecords.Last().Id);
        }
    }

    internal static List<TodoDailyRecord> FindDueRecords(
        List<TodoDailyRecord> records, DateOnly today, TimeOnly currentTime)
    {
        return records.Where(r =>
            r.RecordDate == today &&
            r.Status == TodoStatus.Pending &&
            r.ReminderEnabled &&
            r.DueTime.HasValue &&
            r.DueTime.Value <= currentTime &&
            r.ReminderFiredAt == null
        ).ToList();
    }

    private async Task HandleReminderActionAsync(TodoDailyRecord record, ReminderAction action)
    {
        switch (action)
        {
            case ReminderAction.Snooze10:
                record.DueTime = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(10));
                record.ReminderFiredAt = null;
                await _repository.UpdateRecordAsync(record);
                break;

            case ReminderAction.Snooze30:
                record.DueTime = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(30));
                record.ReminderFiredAt = null;
                await _repository.UpdateRecordAsync(record);
                break;

            case ReminderAction.Complete:
                record.Status = TodoStatus.Completed;
                record.CompletedAt = DateTime.UtcNow;
                await _repository.UpdateRecordAsync(record);
                break;

            case ReminderAction.Abandon:
                record.Status = TodoStatus.Abandoned;
                record.AbandonedAt = DateTime.UtcNow;
                await _repository.UpdateRecordAsync(record);
                break;

            case ReminderAction.Dismiss:
                break;
        }
    }

    private void ShowMainWindow(Guid highlightId)
    {
        _mainWindow.Show();
        _mainWindow.Activate();
        _mainWindow.Topmost = true;
        _mainWindow.Topmost = false;
        _viewModel.HighlightItem(highlightId);
        _ = _viewModel.LoadItemsAsync();
    }
}
