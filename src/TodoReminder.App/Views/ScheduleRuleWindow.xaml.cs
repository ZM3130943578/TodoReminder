using System.Windows;
using System.Windows.Controls;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.App.Views;

public partial class ScheduleRuleWindow : Window
{
    public PopupSchedule? Result { get; private set; }

    public ScheduleRuleWindow()
    {
        InitializeComponent();
        Owner = System.Windows.Application.Current.MainWindow;
        TypeCombo.SelectedIndex = 0;
    }

    public void SetForEdit(PopupSchedule schedule)
    {
        Title = "编辑定时弹出规则";
        NameBox.Text = schedule.Name;
        EnabledCheck.IsChecked = schedule.Enabled;

        TypeCombo.SelectedIndex = schedule.ScheduleType switch
        {
            PopupScheduleType.Once => 0,
            PopupScheduleType.Daily => 1,
            PopupScheduleType.Weekly => 2,
            PopupScheduleType.Interval => 3,
            _ => 0
        };

        TimeOfDayBox.Text = schedule.TimeOfDay?.ToString("HH:mm") ?? "";
        OnceAtBox.Text = schedule.OnceAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
        IntervalBox.Text = schedule.IntervalMinutes?.ToString() ?? "";
        WeekdaysBox.Text = schedule.Weekdays ?? "";
        MessageInput.Text = schedule.Message;

        Result = schedule;
    }

    private void OnTypeChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        TimeOfDayPanel.Visibility = Visibility.Collapsed;
        OnceAtPanel.Visibility = Visibility.Collapsed;
        IntervalPanel.Visibility = Visibility.Collapsed;
        WeekdaysPanel.Visibility = Visibility.Collapsed;

        switch (TypeCombo.SelectedIndex)
        {
            case 0: OnceAtPanel.Visibility = Visibility.Visible; break;
            case 1: TimeOfDayPanel.Visibility = Visibility.Visible; break;
            case 2: TimeOfDayPanel.Visibility = Visibility.Visible; WeekdaysPanel.Visibility = Visibility.Visible; break;
            case 3: IntervalPanel.Visibility = Visibility.Visible; break;
        }
    }

    private void OnConfirm(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            System.Windows.MessageBox.Show("请输入规则名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }

        var isNew = Result == null;
        if (isNew) Result = new PopupSchedule();
        var s = Result!;

        s.Name = NameBox.Text.Trim();
        s.Enabled = EnabledCheck.IsChecked ?? true;
        s.ScheduleType = TypeCombo.SelectedIndex switch
        {
            0 => PopupScheduleType.Once,
            1 => PopupScheduleType.Daily,
            2 => PopupScheduleType.Weekly,
            3 => PopupScheduleType.Interval,
            _ => PopupScheduleType.Daily
        };
        s.Message = MessageInput.Text.Trim();
        s.TimeOfDay = null;
        s.OnceAt = null;
        s.IntervalMinutes = null;
        s.Weekdays = null;

        switch (TypeCombo.SelectedIndex)
        {
            case 0:
                if (DateTime.TryParse(OnceAtBox.Text.Trim(), out var onceAt))
                    s.OnceAt = onceAt;
                break;
            case 1:
            case 2:
                if (TimeOnly.TryParse(TimeOfDayBox.Text.Trim(), out var tod))
                    s.TimeOfDay = tod;
                if (TypeCombo.SelectedIndex == 2)
                    s.Weekdays = WeekdaysBox.Text.Trim();
                break;
            case 3:
                if (int.TryParse(IntervalBox.Text.Trim(), out var interval))
                    s.IntervalMinutes = interval;
                break;
        }

        DialogResult = true;
        Close();
    }
}
