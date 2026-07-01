using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoReminder.App.Services;
using TodoReminder.App.Views;
using TodoReminder.Application.Services;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITodoService _todoService;
    private readonly AppSettingsService _settingsService;
    private readonly IPopupScheduleRepository _scheduleRepository;
    private readonly DispatcherTimer _dayChangeTimer;

    [ObservableProperty]
    private DateOnly _currentDate;

    [ObservableProperty]
    private ObservableCollection<TodoItemViewModel> _todoItems = [];

    [ObservableProperty]
    private bool _isLoading;

    public string DateDisplay => CurrentDate.ToString("yyyy-MM-dd ddd");
    public bool IsToday => CurrentDate == DateOnly.FromDateTime(DateTime.Now);

    public int PendingCount => TodoItems.Count(i => i.Status == TodoStatus.Pending);
    public int CompletedCount => TodoItems.Count(i => i.Status == TodoStatus.Completed);
    public int AbandonedCount => TodoItems.Count(i => i.Status == TodoStatus.Abandoned);

    public MainViewModel(ITodoService todoService, AppSettingsService settingsService, IPopupScheduleRepository scheduleRepository)
    {
        _todoService = todoService;
        _settingsService = settingsService;
        _scheduleRepository = scheduleRepository;
        CurrentDate = DateOnly.FromDateTime(DateTime.Now);

        _dayChangeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _dayChangeTimer.Tick += OnDayChangeTick;
        _dayChangeTimer.Start();
    }

    private void OnDayChangeTick(object? sender, EventArgs e)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (CurrentDate != today)
        {
            CurrentDate = today;
        }
    }

    partial void OnCurrentDateChanged(DateOnly value)
    {
        OnPropertyChanged(nameof(DateDisplay));
        OnPropertyChanged(nameof(IsToday));
        _ = LoadItemsAsync();
    }

    private void RefreshStats()
    {
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(AbandonedCount));
    }

    private void SubscribeItemEvents(TodoItemViewModel item)
    {
        item.PropertyChanged += (_, _) => RefreshStats();
        item.EditRequested += async (s, _) => await OnEditItemAsync((TodoItemViewModel)s!);
        item.DeleteRequested += async (s, _) => await OnDeleteItemAsync((TodoItemViewModel)s!);
        item.StatusChangeRequested += async (s, _) => await OnItemStatusChangedAsync((TodoItemViewModel)s!);
    }

    public async Task LoadItemsAsync()
    {
        IsLoading = true;
        try
        {
            var dtos = await _todoService.GetTodosByDateAsync(CurrentDate);
            TodoItems.Clear();
            foreach (var dto in dtos)
            {
                var vm = new TodoItemViewModel
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Note = dto.Note,
                    Status = dto.Status,
                    DueTime = dto.DueTime,
                    IsInherited = dto.InheritedFromRecordId.HasValue
                };
                SubscribeItemEvents(vm);
                TodoItems.Add(vm);
            }
        }
        finally
        {
            IsLoading = false;
        }
        RefreshStats();
    }

    [RelayCommand]
    private void GoToPreviousDay()
    {
        CurrentDate = CurrentDate.AddDays(-1);
    }

    [RelayCommand]
    private void GoToNextDay()
    {
        CurrentDate = CurrentDate.AddDays(1);
    }

    [RelayCommand]
    private void GoToToday()
    {
        CurrentDate = DateOnly.FromDateTime(DateTime.Now);
    }

    [RelayCommand]
    private async Task AddNewTodo()
    {
        var dialog = new TodoEditWindow();
        if (dialog.ShowDialog() == true)
        {
            await _todoService.CreateTodoAsync(
                dialog.TodoTitle, dialog.TodoNote, CurrentDate, dialog.TodoDueTime);
            await LoadItemsAsync();
        }
    }

    private async Task OnEditItemAsync(TodoItemViewModel item)
    {
        var dialog = new TodoEditWindow();
        dialog.SetForEdit(item.Title, item.Note, item.DueTime);
        if (dialog.ShowDialog() == true)
        {
            await _todoService.UpdateTodoAsync(
                item.Id, dialog.TodoTitle, dialog.TodoNote, dialog.TodoDueTime);
            await LoadItemsAsync();
        }
    }

    private async Task OnDeleteItemAsync(TodoItemViewModel item)
    {
        var result = System.Windows.MessageBox.Show(
            $"确定要删除「{item.Title}」吗？",
            "确认删除", System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _todoService.DeleteTodoAsync(item.Id);
            await LoadItemsAsync();
        }
    }

    private async Task OnItemStatusChangedAsync(TodoItemViewModel item)
    {
        switch (item.Status)
        {
            case TodoStatus.Completed:
                await _todoService.CompleteTodoAsync(item.Id);
                break;
            case TodoStatus.Abandoned:
                await _todoService.AbandonTodoAsync(item.Id);
                break;
            case TodoStatus.Pending:
                await _todoService.RestoreTodoAsync(item.Id);
                break;
        }
        await LoadItemsAsync();
    }

    public void HighlightItem(Guid recordId)
    {
        foreach (var item in TodoItems)
        {
            item.IsHighlighted = item.Id == recordId;
        }
    }

    public Action? OnSettingsClosed { get; set; }

    [RelayCommand]
    private void OpenSettings()
    {
        var window = new SettingsWindow(_settingsService, _scheduleRepository);
        window.ShowDialog();
        OnSettingsClosed?.Invoke();
    }

    [RelayCommand]
    private async Task OpenHistory()
    {
        var items = await _todoService.GetHistoryAsync();
        var window = new HistoryWindow(items);
        window.ShowDialog();
    }
}
