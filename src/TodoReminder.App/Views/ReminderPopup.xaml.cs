using System.Windows;

namespace TodoReminder.App.Views;

public enum ReminderAction
{
    Dismiss,
    Snooze10,
    Snooze30,
    Complete,
    Abandon
}

public partial class ReminderPopup : Window
{
    public ReminderAction SelectedAction { get; private set; } = ReminderAction.Dismiss;

    public ReminderPopup(string title, string? note, TimeOnly? dueTime)
    {
        InitializeComponent();
        TitleText.Text = title;
        NoteText.Text = note ?? string.Empty;
        NoteText.Visibility = string.IsNullOrEmpty(note) ? Visibility.Collapsed : Visibility.Visible;
        TimeText.Text = dueTime.HasValue ? $"⏰ {dueTime.Value:HH:mm}" : string.Empty;
    }

    public static ReminderAction Show(string title, string? note, TimeOnly? dueTime, Window? owner)
    {
        var popup = new ReminderPopup(title, note, dueTime);
        if (owner?.IsVisible == true)
            popup.Owner = owner;
        popup.ShowDialog();
        return popup.SelectedAction;
    }

    private void OnDismiss(object sender, RoutedEventArgs e)
    {
        SelectedAction = ReminderAction.Dismiss;
        DialogResult = true;
        Close();
    }

    private void OnSnooze10(object sender, RoutedEventArgs e)
    {
        SelectedAction = ReminderAction.Snooze10;
        DialogResult = true;
        Close();
    }

    private void OnSnooze30(object sender, RoutedEventArgs e)
    {
        SelectedAction = ReminderAction.Snooze30;
        DialogResult = true;
        Close();
    }

    private void OnComplete(object sender, RoutedEventArgs e)
    {
        SelectedAction = ReminderAction.Complete;
        DialogResult = true;
        Close();
    }

    private void OnAbandon(object sender, RoutedEventArgs e)
    {
        SelectedAction = ReminderAction.Abandon;
        DialogResult = true;
        Close();
    }
}
