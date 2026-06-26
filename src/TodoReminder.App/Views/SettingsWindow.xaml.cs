using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using TodoReminder.App.Services;
using TodoReminder.Domain.Entities;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettingsService _settings;
    private readonly IPopupScheduleRepository _scheduleRepository;

    public SettingsWindow(AppSettingsService settings, IPopupScheduleRepository scheduleRepository)
    {
        InitializeComponent();
        _settings = settings;
        _scheduleRepository = scheduleRepository;
        Owner = System.Windows.Application.Current.MainWindow;
        CategoryList.SelectedIndex = 0;
        LoadSettings();
    }

    private void LoadSettings()
    {
        HotkeyEnabled.IsChecked = _settings.GetBool("hotkey.enabled", true);
        _hotkeyString = _settings.GetString("hotkey.value", "Ctrl+Alt+Space");
        HotkeyButton.Content = _hotkeyString;

        WindowTopmost.IsChecked = _settings.GetBool("window.topmost", true);
        WindowStartHidden.IsChecked = _settings.GetBool("window.start_hidden", false);
        WindowWidth.Text = _settings.GetInt("window.width", 420).ToString();
        WindowHeight.Text = _settings.GetInt("window.height", 640).ToString();

        ReminderEnabled.IsChecked = _settings.GetBool("reminder.enabled", true);
        ReminderSnooze.Text = _settings.GetInt("reminder.default_snooze_minutes", 10).ToString();

        StartupAutoStart.IsChecked = IsAutoStartEnabled();

        DbPath.Text = DatabaseConfig.GetDefaultConnectionString().Replace("Data Source=", "");
    }

    private async void LoadScheduleList()
    {
        var schedules = await _scheduleRepository.GetAllAsync();
        ScheduleList.ItemsSource = schedules.Select(s => new ScheduleListItem(s)).ToList();
    }

    private Dictionary<string, string> CollectSettings()
    {
        return new Dictionary<string, string>
        {
            ["hotkey.enabled"] = (HotkeyEnabled.IsChecked ?? true).ToString(),
            ["hotkey.value"] = _hotkeyString,
            ["window.topmost"] = (WindowTopmost.IsChecked ?? true).ToString(),
            ["window.start_hidden"] = (WindowStartHidden.IsChecked ?? false).ToString(),
            ["window.width"] = int.TryParse(WindowWidth.Text, out var w) ? w.ToString() : "420",
            ["window.height"] = int.TryParse(WindowHeight.Text, out var h) ? h.ToString() : "640",
            ["reminder.enabled"] = (ReminderEnabled.IsChecked ?? true).ToString(),
            ["reminder.default_snooze_minutes"] = int.TryParse(ReminderSnooze.Text, out var m) ? m.ToString() : "10",
            ["startup.auto_start"] = (StartupAutoStart.IsChecked ?? false).ToString(),
        };
    }

    private async void OnSave(object sender, RoutedEventArgs e)
    {
        var settings = CollectSettings();
        await _settings.SaveAllAsync(settings);

        SetAutoStart(StartupAutoStart.IsChecked ?? false);
        ApplyWindowSettings(settings);

        DialogResult = true;
        Close();
    }

    private void ApplyWindowSettings(Dictionary<string, string> settings)
    {
        if (System.Windows.Application.Current.MainWindow is not MainWindow window)
            return;

        if (settings.TryGetValue("window.topmost", out var topmost) && bool.TryParse(topmost, out var t))
            window.Topmost = t;

        if (settings.TryGetValue("window.width", out var width) && int.TryParse(width, out var w))
            window.Width = w;

        if (settings.TryGetValue("window.height", out var height) && int.TryParse(height, out var h))
            window.Height = h;
    }

    private void OnCategoryChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        PanelHotkey.Visibility = Visibility.Collapsed;
        PanelWindow.Visibility = Visibility.Collapsed;
        PanelReminder.Visibility = Visibility.Collapsed;
        PanelSchedule.Visibility = Visibility.Collapsed;
        PanelData.Visibility = Visibility.Collapsed;
        PanelStartup.Visibility = Visibility.Collapsed;

        var index = CategoryList.SelectedIndex;
        if (index == 0) PanelHotkey.Visibility = Visibility.Visible;
        else if (index == 1) PanelWindow.Visibility = Visibility.Visible;
        else if (index == 2) PanelReminder.Visibility = Visibility.Visible;
        else if (index == 3) { PanelSchedule.Visibility = Visibility.Visible; LoadScheduleList(); }
        else if (index == 4) PanelData.Visibility = Visibility.Visible;
        else if (index == 5) PanelStartup.Visibility = Visibility.Visible;
    }

    private string _hotkeyString = "Ctrl+Alt+Space";
    private bool _capturingHotkey;

    private void HotkeyButton_Click(object sender, RoutedEventArgs e)
    {
        _capturingHotkey = true;
        HotkeyButton.Content = "按下快捷键...";
        HotkeyButton.Focusable = false;
        PreviewKeyDown += Window_PreviewKeyDown;
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_capturingHotkey) return;

        e.Handled = true;

        var actualKey = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;
        if (actualKey == System.Windows.Input.Key.Escape)
        {
            ExitCaptureMode();
            return;
        }

        var mods = new List<string>();
        if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            mods.Add("Ctrl");
        if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0)
            mods.Add("Alt");
        if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0)
            mods.Add("Shift");
        if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Windows) != 0)
            mods.Add("Win");

        if (mods.Count == 0) return;

        var keyName = actualKey switch
        {
            System.Windows.Input.Key.Space => "Space",
            System.Windows.Input.Key.Enter => "Enter",
            System.Windows.Input.Key.Tab => "Tab",
            System.Windows.Input.Key.Escape => "Escape",
            System.Windows.Input.Key.Back => "Backspace",
            System.Windows.Input.Key.Delete => "Delete",
            System.Windows.Input.Key.Left => "Left",
            System.Windows.Input.Key.Right => "Right",
            System.Windows.Input.Key.Up => "Up",
            System.Windows.Input.Key.Down => "Down",
            System.Windows.Input.Key.F1 => "F1",
            System.Windows.Input.Key.F2 => "F2",
            System.Windows.Input.Key.F3 => "F3",
            System.Windows.Input.Key.F4 => "F4",
            System.Windows.Input.Key.F5 => "F5",
            System.Windows.Input.Key.F6 => "F6",
            System.Windows.Input.Key.F7 => "F7",
            System.Windows.Input.Key.F8 => "F8",
            System.Windows.Input.Key.F9 => "F9",
            System.Windows.Input.Key.F10 => "F10",
            System.Windows.Input.Key.F11 => "F11",
            System.Windows.Input.Key.F12 => "F12",
            _ when actualKey >= System.Windows.Input.Key.A && actualKey <= System.Windows.Input.Key.Z => actualKey.ToString(),
            _ when actualKey >= System.Windows.Input.Key.D0 && actualKey <= System.Windows.Input.Key.D9 => actualKey.ToString()[1..],
            _ => null
        };

        if (keyName == null) return;

        _hotkeyString = $"{string.Join("+", mods)}+{keyName}";
        ExitCaptureMode();
    }

    private void ExitCaptureMode()
    {
        _capturingHotkey = false;
        HotkeyButton.Content = _hotkeyString;
        HotkeyButton.Focusable = true;
        PreviewKeyDown -= Window_PreviewKeyDown;
    }

    private void OnOpenDbFolder(object sender, RoutedEventArgs e)
    {
        var path = DatabaseConfig.GetDefaultConnectionString().Replace("Data Source=", "");
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
            Process.Start("explorer.exe", dir);
    }

    private void OnExportData(object sender, RoutedEventArgs e)
    {
        var dbPath = DatabaseConfig.GetDefaultConnectionString().Replace("Data Source=", "");
        var dialog = new SaveFileDialog
        {
            Filter = "SQLite Database (*.db)|*.db",
            FileName = $"TodoReminder_{DateTime.Now:yyyy-MM-dd}.db"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                System.IO.File.Copy(dbPath, dialog.FileName, true);
                MessageBox.Show("数据库已备份到指定位置。", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void OnImportData(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "SQLite Database (*.db)|*.db"
        };
        if (dialog.ShowDialog() != true) return;

        var result = MessageBox.Show(
            "导入数据将替换当前数据库。\n当前数据将被备份为 .bak 文件。\n导入后请重启应用。\n\n确定继续？",
            "导入数据", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        var dbPath = DatabaseConfig.GetDefaultConnectionString().Replace("Data Source=", "");
        var backupPath = dbPath + ".bak";

        try
        {
            System.IO.File.Copy(dbPath, backupPath, true);
            System.IO.File.Copy(dialog.FileName, dbPath, true);

            MessageBox.Show(
                "数据已导入。请重启应用以加载新数据。\n原数据已备份为 " + backupPath,
                "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            try { System.IO.File.Copy(backupPath, dbPath, true); } catch { }
            MessageBox.Show($"导入失败，已恢复原数据。\n错误：{ex.Message}",
                "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        return key?.GetValue("TodoReminderTool") != null;
    }

    private static void SetAutoStart(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key == null) return;
        if (enabled)
            key.SetValue("TodoReminderTool", Environment.ProcessPath!);
        else
            key.DeleteValue("TodoReminderTool", false);
    }

    private async void OnAddSchedule(object sender, RoutedEventArgs e)
    {
        var dialog = new ScheduleRuleWindow();
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            await _scheduleRepository.AddAsync(dialog.Result);
            LoadScheduleList();
        }
    }

    private async void OnEditSchedule(object sender, RoutedEventArgs e)
    {
        if (ScheduleList.SelectedItem is not ScheduleListItem item) return;

        var dialog = new ScheduleRuleWindow();
        dialog.SetForEdit(item.Schedule);
        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            await _scheduleRepository.UpdateAsync(dialog.Result);
            LoadScheduleList();
        }
    }

    private async void OnDeleteSchedule(object sender, RoutedEventArgs e)
    {
        if (ScheduleList.SelectedItem is not ScheduleListItem item) return;

        var result = MessageBox.Show($"确定要删除「{item.Schedule.Name}」吗？",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            await _scheduleRepository.DeleteAsync(item.Schedule.Id);
            LoadScheduleList();
        }
    }

    private class ScheduleListItem
    {
        public PopupSchedule Schedule { get; }
        public string DisplayText { get; }

        public ScheduleListItem(PopupSchedule schedule)
        {
            Schedule = schedule;
            var icon = schedule.Enabled ? "✓" : "✗";
            var type = schedule.ScheduleType switch
            {
                Domain.Enums.PopupScheduleType.Once => $"Once {schedule.OnceAt:HH:mm}",
                Domain.Enums.PopupScheduleType.Daily => $"Daily {schedule.TimeOfDay:HH:mm}",
                Domain.Enums.PopupScheduleType.Weekly => $"Weekly {schedule.TimeOfDay:HH:mm}",
                Domain.Enums.PopupScheduleType.Interval => $"Every {schedule.IntervalMinutes}min",
                _ => ""
            };
            DisplayText = $"{icon} {schedule.Name} ({type})";
        }
    }
}
