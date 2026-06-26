using System.Windows.Threading;
using TodoReminder.App.ViewModels;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class PopupScheduleService : IDisposable
{
    private readonly IPopupScheduleRepository _repository;
    private readonly MainWindow _mainWindow;
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _timer;

    public PopupScheduleService(IPopupScheduleRepository repository, MainWindow mainWindow, MainViewModel viewModel)
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
        try { await CheckSchedulesAsync(); }
        finally { _timer.Start(); }
    }

    public async Task CheckSchedulesAsync()
    {
        var now = DateTime.Now;
        var schedules = await _repository.GetEnabledAsync();

        foreach (var schedule in schedules)
        {
            if (!IsDue(schedule, now)) continue;

            schedule.LastTriggeredAt = now;
            await _repository.UpdateAsync(schedule);

            if (!string.IsNullOrWhiteSpace(schedule.Message))
            {
                System.Windows.MessageBox.Show(
                    schedule.Message,
                    schedule.Name,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }

            _mainWindow.Show();
            _mainWindow.Activate();
            _mainWindow.Topmost = true;
            _mainWindow.Topmost = false;
        }
    }

    internal static bool IsDue(PopupSchedule schedule, DateTime now)
    {
        if (!schedule.Enabled) return false;

        switch (schedule.ScheduleType)
        {
            case PopupScheduleType.Once:
                return schedule.OnceAt.HasValue
                    && schedule.OnceAt <= now
                    && (!schedule.LastTriggeredAt.HasValue || schedule.LastTriggeredAt < schedule.OnceAt);

            case PopupScheduleType.Daily:
                return schedule.TimeOfDay.HasValue
                    && schedule.TimeOfDay <= TimeOnly.FromDateTime(now)
                    && (!schedule.LastTriggeredAt.HasValue || schedule.LastTriggeredAt.Value.Date < now.Date);

            case PopupScheduleType.Weekly:
                if (!schedule.TimeOfDay.HasValue || string.IsNullOrWhiteSpace(schedule.Weekdays))
                    return false;
                var days = ParseWeekdays(schedule.Weekdays);
                if (!days.Contains((int)now.DayOfWeek)) return false;
                return schedule.TimeOfDay <= TimeOnly.FromDateTime(now)
                    && (!schedule.LastTriggeredAt.HasValue || schedule.LastTriggeredAt.Value.Date < now.Date);

            case PopupScheduleType.Interval:
                return schedule.IntervalMinutes.HasValue
                    && (!schedule.LastTriggeredAt.HasValue
                        || (now - schedule.LastTriggeredAt.Value).TotalMinutes >= schedule.IntervalMinutes.Value);

            default:
                return false;
        }
    }

    private static HashSet<int> ParseWeekdays(string weekdays)
    {
        return weekdays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Where(s => int.TryParse(s, out _))
                       .Select(int.Parse)
                       .ToHashSet();
    }
}
