using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoReminder.Domain.Enums;

namespace TodoReminder.App.ViewModels;

public partial class TodoItemViewModel : ObservableObject
{
    public event EventHandler? EditRequested;
    public event EventHandler? DeleteRequested;
    public event EventHandler? StatusChangeRequested;

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _note;

    [ObservableProperty]
    private TodoStatus _status = TodoStatus.Pending;

    [ObservableProperty]
    private TimeOnly? _dueTime;

    [ObservableProperty]
    private bool _isInherited;

    [ObservableProperty]
    private bool _isHighlighted;

    public string DueTimeDisplay => DueTime?.ToString("HH:mm") ?? string.Empty;

    public string StatusIcon => Status switch
    {
        TodoStatus.Pending => "☐",
        TodoStatus.Completed => "✓",
        TodoStatus.Abandoned => "⊘",
        _ => "☐"
    };

    public string StatusDisplay => Status switch
    {
        TodoStatus.Pending => "待处理",
        TodoStatus.Completed => "已完成",
        TodoStatus.Abandoned => "已废弃",
        _ => string.Empty
    };

    partial void OnStatusChanged(TodoStatus value)
    {
        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusDisplay));
    }

    partial void OnDueTimeChanged(TimeOnly? value)
    {
        OnPropertyChanged(nameof(DueTimeDisplay));
    }

    [RelayCommand]
    private void ToggleComplete()
    {
        Status = Status == TodoStatus.Completed ? TodoStatus.Pending : TodoStatus.Completed;
        StatusChangeRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ToggleAbandon()
    {
        Status = Status == TodoStatus.Abandoned ? TodoStatus.Pending : TodoStatus.Abandoned;
        StatusChangeRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Edit()
    {
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Delete()
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }
}
