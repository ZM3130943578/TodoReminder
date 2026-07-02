using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Serilog;
using TodoReminder.App.Services;
using TodoReminder.App.ViewModels;
using TodoReminder.App.Views;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Implementations;
using TodoReminder.Infrastructure.Repositories.Interfaces;
using TodoReminder.Infrastructure.Windows;

namespace TodoReminder.App;

public partial class App : System.Windows.Application
{
    private static readonly string MutexName = "TodoReminderTool_SingleInstance";
    private static readonly string WakeupEventName = "TodoReminderTool_WakeupEvent";

    private static Mutex? _mutex;
    private static EventWaitHandle? _wakeupHandle;
    private CancellationTokenSource? _wakeupCts;

    private AppDbContext? _dbContext;
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private MainViewModel? _viewModel;
    private HotkeyService? _hotkeyService;
    private ReminderService? _reminderService;
    private PopupScheduleService? _scheduleService;
    private DayRolloverService? _rolloverService;
    private AppSettingsService? _settingsService;
    private IPopupScheduleRepository? _scheduleRepository;
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        InitLogger();
        Log.Information("Application starting");

        if (!EnsureSingleInstance()) return;

        SetupExceptionHandlers();

        base.OnStartup(e);

        var options = DatabaseConfig.CreateOptions();
        _dbContext = new AppDbContext(options);
        DbInitializer.Initialize(_dbContext);
        Log.Information("Database initialized");

        var repository = new TodoRepository(_dbContext);
        var todoService = new TodoService(repository);
        var appSettingRepo = new AppSettingRepository(_dbContext);
        _settingsService = new AppSettingsService(appSettingRepo);
        _ = _settingsService.LoadAsync();

        _scheduleRepository = new PopupScheduleRepository(_dbContext);

        _viewModel = new MainViewModel(todoService, _settingsService, _scheduleRepository);
_viewModel.OnSettingsClosed = () =>
{
    ApplyWindowSettings();
    RegisterHotkey();
};

        CreateTrayIcon();

        _mainWindow = new MainWindow(_viewModel);
        _mainWindow.Closing += MainWindow_Closing;
        MainWindow = _mainWindow;

        ApplyWindowSettings();

        if (!_settingsService.GetBool("window.start_hidden", false))
            _mainWindow.Show();

        RegisterHotkey();

        _reminderService = new ReminderService(repository, _mainWindow, _viewModel);
        _reminderService.Start();

        _scheduleService = new PopupScheduleService(_scheduleRepository, _mainWindow, _viewModel);
        _scheduleService.Start();

        _rolloverService = new DayRolloverService(repository);
        _ = _rolloverService.EnsureRolloverAsync();
        _rolloverService.Start();

        _ = _viewModel.LoadItemsAsync();

        SetupPowerModeHandler();
        StartWakeupListener();

        Log.Information("Application started successfully");
    }

    private static void InitLogger()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TodoReminderTool", "logs");
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: Path.Combine(logDir, "todo-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();
    }

    private bool EnsureSingleInstance()
    {
        _mutex = new Mutex(true, MutexName, out var createdNew);
        if (createdNew) return true;

        Log.Information("Another instance detected, waking existing instance");
        try
        {
            using var evt = EventWaitHandle.OpenExisting(WakeupEventName);
            evt.Set();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to wake existing instance");
        }

        Shutdown();
        return false;
    }

    private void SetupExceptionHandlers()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            Log.Fatal(e.Exception, "Dispatcher unhandled exception");
            System.Windows.MessageBox.Show(
                $"发生未处理的异常：{e.Exception.Message}\n\n详细信息已记录到日志。",
                "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "AppDomain unhandled exception");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Fatal(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }

    private void SetupPowerModeHandler()
    {
        SystemEvents.PowerModeChanged += async (_, e) =>
        {
            if (e.Mode != PowerModes.Resume) return;

            Log.Information("System resumed from sleep, re-checking reminders and rollover");
            try
            {
                if (_rolloverService != null)
                {
                    await _rolloverService.EnsureRolloverAsync();
                    if (_viewModel != null)
                        await _viewModel.LoadItemsAsync();
                }

                if (_reminderService != null)
                    await _reminderService.CheckDueRemindersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during post-sleep recovery");
            }
        };
    }

    private void StartWakeupListener()
    {
        _wakeupCts = new CancellationTokenSource();

        Task.Run(() =>
        {
            try
            {
                _wakeupHandle = new EventWaitHandle(false, EventResetMode.AutoReset, WakeupEventName);
                while (!_wakeupCts.Token.IsCancellationRequested)
                {
                    _wakeupHandle.WaitOne();
                    if (_wakeupCts.Token.IsCancellationRequested) break;
                    Current.Dispatcher.Invoke(() =>
                    {
                        if (!_isExiting) ShowWindow();
                    });
                }
            }
            catch (Exception ex)
            {
                if (!_wakeupCts.Token.IsCancellationRequested)
                    Log.Error(ex, "Wakeup listener error");
            }
        }, _wakeupCts.Token);
    }

    private void ApplyWindowSettings()
    {
        if (_settingsService == null || _mainWindow == null) return;
        _mainWindow.Topmost = _settingsService.GetBool("window.topmost", true);
        _mainWindow.Width = _settingsService.GetInt("window.width", 420);
        _mainWindow.Height = _settingsService.GetInt("window.height", 640);
    }

    private void CreateTrayIcon()
    {
        var iconStream = System.Windows.Application.GetResourceStream(
            new Uri("pack://application:,,,/TodoReminder.ico")).Stream;
        _trayIcon = new TaskbarIcon
        {
            Icon = new System.Drawing.Icon(iconStream),
            ToolTipText = "TodoReminder"
        };

        var menu = new ContextMenu();

        var toggleItem = new MenuItem { Header = "打开/隐藏" };
        toggleItem.Click += (_, _) => ToggleWindow();
        menu.Items.Add(toggleItem);

        var todayItem = new MenuItem { Header = "今日待办" };
        todayItem.Click += (_, _) => ShowWindow();
        menu.Items.Add(todayItem);

        var addItem = new MenuItem { Header = "新增事项" };
        addItem.Click += (_, _) => ShowWindowAndAdd();
        menu.Items.Add(addItem);

        var historyItem = new MenuItem { Header = "历史记录" };
        historyItem.Click += (_, _) => OpenHistoryWindow();
        menu.Items.Add(historyItem);

        menu.Items.Add(new Separator());

        var settingsItem = new MenuItem { Header = "设置" };
        settingsItem.Click += (_, _) => OpenSettingsWindow();
        menu.Items.Add(settingsItem);

        menu.Items.Add(new Separator());

#if DEBUG
        var debugItem = new MenuItem { Header = "Debug" };
        debugItem.Click += (_, _) => OpenDebugWindow();
        menu.Items.Add(debugItem);
        menu.Items.Add(new Separator());
#endif

        var exitItem = new MenuItem { Header = "退出" };
        exitItem.Click += (_, _) => ExitApp();
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => ToggleWindow();
    }

    private void OpenDebugWindow()
    {
        if (_dbContext == null || _viewModel == null) return;
        var repository = new TodoRepository(_dbContext);
        var window = new DebugWindow(_reminderService!, _scheduleService!, _rolloverService!, repository, _viewModel);
        window.ShowDialog();
    }

    private void OpenHistoryWindow()
    {
        if (_dbContext == null) return;
        var repository = new TodoRepository(_dbContext);
        var todoService = new TodoService(repository);
        var window = new HistoryWindow(todoService);
        window.Owner = _mainWindow;
        window.ShowDialog();
    }

    private void OpenSettingsWindow()
    {
        if (_settingsService == null || _scheduleRepository == null) return;
        var window = new SettingsWindow(_settingsService, _scheduleRepository);
        window.ShowDialog();
        ApplyWindowSettings();
        RegisterHotkey();
    }

    private void ToggleWindow()
    {
        if (_mainWindow == null) return;
        if (_mainWindow.Visibility == System.Windows.Visibility.Visible)
            _mainWindow.Hide();
        else
            ShowWindow();
    }

    private void ShowWindow()
    {
        if (_mainWindow == null) return;
        _mainWindow.Show();
        _mainWindow.Activate();
        _mainWindow.Topmost = true;
        _mainWindow.Topmost = false;
    }

    private void ShowWindowAndAdd()
    {
        ShowWindow();
        _viewModel?.AddNewTodoCommand.Execute(null);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExiting) return;
        e.Cancel = true;
        _mainWindow?.Hide();
    }
    private void RegisterHotkey()
    {
        if (_mainWindow == null) return;

        if (_hotkeyService == null)
        {
            _hotkeyService = new HotkeyService(_mainWindow);
            _hotkeyService.HotkeyPressed += (_, _) => ToggleWindow();
        }

        var hotkeyStr = _settingsService?.GetString("hotkey.value", "Ctrl+Alt+Space") ?? "Ctrl+Alt+Space";
        var (modifiers, key) = ParseHotkey(hotkeyStr);

        if (!_hotkeyService.Reregister(modifiers, key))
        {
            System.Windows.MessageBox.Show(
                $"全局快捷键 {hotkeyStr} 注册失败，可能被其他应用占用。",
                "快捷键注册失败",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }

    private static (uint modifiers, uint key) ParseHotkey(string hotkey)
    {
        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return (Win32Api.MOD_CONTROL | Win32Api.MOD_ALT, Win32Api.VK_SPACE);

        uint modifiers = 0;
        uint key = Win32Api.VK_SPACE;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            modifiers |= parts[i].ToLowerInvariant() switch
            {
                "ctrl" => Win32Api.MOD_CONTROL,
                "alt" => Win32Api.MOD_ALT,
                "shift" => Win32Api.MOD_SHIFT,
                "win" => Win32Api.MOD_WIN,
                _ => 0
            };
        }

        var keyName = parts[^1].ToLowerInvariant();
        key = keyName switch
        {
            "space" => Win32Api.VK_SPACE,
            "enter" => 0x0D,
            "tab" => 0x09,
            "esc" or "escape" => 0x1B,
            "back" or "backspace" => 0x08,
            "delete" => 0x2E,
            "left" => 0x25,
            "right" => 0x27,
            "up" => 0x26,
            "down" => 0x28,
            _ when keyName.Length == 1 && char.IsAsciiLetter(keyName[0]) => (uint)char.ToUpperInvariant(keyName[0]),
            _ when int.TryParse(keyName, out var n) && n >= 0 && n <= 9 => (uint)('0' + n),
            _ when keyName.StartsWith("f") && int.TryParse(keyName[1..], out var fn) && fn >= 1 && fn <= 24
                => (uint)(0x6F + fn - 1),
            _ => Win32Api.VK_SPACE
        };

        return (modifiers, key);
    }

    private void ExitApp()
    {
        _isExiting = true;
        Log.Information("Application exiting");
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        _trayIcon = null;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scheduleService?.Dispose();
        _reminderService?.Dispose();
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        _dbContext?.Dispose();
        _mutex?.Dispose();
        _wakeupHandle?.Dispose();
        _wakeupCts?.Cancel();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
