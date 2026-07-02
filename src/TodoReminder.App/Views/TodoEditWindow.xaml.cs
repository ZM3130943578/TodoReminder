using System.Windows;

namespace TodoReminder.App.Views;

public partial class TodoEditWindow : Window
{
    public string TodoTitle => TitleBox.Text.Trim();
    public string? TodoNote => string.IsNullOrWhiteSpace(NoteBox.Text) ? null : NoteBox.Text.Trim();
    public TimeOnly? TodoDueTime
    {
        get
        {
            if (TimeEnabledCheck.IsChecked != true)
                return null;
            if (HourCombo.SelectedItem == null || MinuteCombo.SelectedItem == null)
                return null;
            var hour = int.Parse((string)HourCombo.SelectedItem);
            var minute = int.Parse((string)MinuteCombo.SelectedItem);
            return new TimeOnly(hour, minute);
        }
    }

    public TodoEditWindow()
    {
        InitializeComponent();
        Owner = System.Windows.Application.Current.MainWindow;
        for (int i = 0; i < 24; i++) HourCombo.Items.Add(i.ToString("D2"));
        for (int i = 0; i < 60; i++) MinuteCombo.Items.Add(i.ToString("D2"));
    }

    public void SetForEdit(string title, string? note, TimeOnly? dueTime)
    {
        Title = "编辑事项";
        TitleBox.Text = title;
        NoteBox.Text = note ?? string.Empty;
        if (dueTime.HasValue)
        {
            TimeEnabledCheck.IsChecked = true;
            HourCombo.SelectedValue = dueTime.Value.Hour.ToString("D2");
            MinuteCombo.SelectedValue = dueTime.Value.Minute.ToString("D2");
        }
    }

    private void OnTimeEnabledChanged(object sender, RoutedEventArgs e)
    {
        var enabled = TimeEnabledCheck.IsChecked == true;
        HourCombo.IsEnabled = enabled;
        MinuteCombo.IsEnabled = enabled;
        if (!enabled)
        {
            HourCombo.SelectedIndex = -1;
            MinuteCombo.SelectedIndex = -1;
        }
    }

    private void OnConfirm(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show("请输入事项标题", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleBox.Focus();
            return;
        }
        DialogResult = true;
        Close();
    }
}
