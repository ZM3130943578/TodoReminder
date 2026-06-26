using System.Windows;
using TodoReminder.App.Services;
using TodoReminder.App.ViewModels;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Views;

public partial class DebugWindow : Window
{
    private readonly ReminderService _reminderService;
    private readonly PopupScheduleService _scheduleService;
    private readonly DayRolloverService _rolloverService;
    private readonly ITodoRepository _todoRepository;
    private readonly MainViewModel _viewModel;

    public DebugWindow(ReminderService reminderService, PopupScheduleService scheduleService,
        DayRolloverService rolloverService, ITodoRepository todoRepository, MainViewModel viewModel)
    {
        InitializeComponent();
        Owner = System.Windows.Application.Current.MainWindow;
        _reminderService = reminderService;
        _scheduleService = scheduleService;
        _rolloverService = rolloverService;
        _todoRepository = todoRepository;
        _viewModel = viewModel;
    }

    private async void OnCheckReminders(object sender, RoutedEventArgs e)
    {
        await _reminderService.CheckDueRemindersAsync();
        await _viewModel.LoadItemsAsync();
        MessageBox.Show("提醒检查完成", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnCheckSchedules(object sender, RoutedEventArgs e)
    {
        await _scheduleService.CheckSchedulesAsync();
        MessageBox.Show("定时弹出检查完成", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnRollover(object sender, RoutedEventArgs e)
    {
        await _rolloverService.EnsureRolloverAsync();
        await _viewModel.LoadItemsAsync();
        MessageBox.Show("今日继承完成", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnOpenDbFolder(object sender, RoutedEventArgs e)
    {
        var path = DatabaseConfig.GetDefaultConnectionString().Replace("Data Source=", "");
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
            System.Diagnostics.Process.Start("explorer.exe", dir);
    }

    private async void OnClearData(object sender, RoutedEventArgs e)
    {
        var first = MessageBox.Show("确定要清空所有待办数据吗？\n此操作不可恢复！",
            "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (first != MessageBoxResult.Yes) return;

        var second = MessageBox.Show("再次确认：清空后将删除所有事项和每日记录。",
            "二次确认", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
        if (second != MessageBoxResult.Yes) return;

        var today = DateOnly.FromDateTime(DateTime.Now);
        var allRecords = await _todoRepository.GetRecordsByDateRangeAsync(today.AddYears(-10), today.AddYears(10));
        foreach (var record in allRecords)
            await _todoRepository.DeleteRecordAsync(record.Id);

        await _viewModel.LoadItemsAsync();
        MessageBox.Show("所有待办数据已清空", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
