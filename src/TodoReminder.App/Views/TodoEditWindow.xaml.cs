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
            var text = TimeBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (TimeOnly.TryParse(text, out var time))
                return time;
            return null;
        }
    }

    public TodoEditWindow()
    {
        InitializeComponent();
        Owner = System.Windows.Application.Current.MainWindow;
    }

    public void SetForEdit(string title, string? note, TimeOnly? dueTime)
    {
        Title = "编辑事项";
        TitleBox.Text = title;
        NoteBox.Text = note ?? string.Empty;
        TimeBox.Text = dueTime?.ToString("HH:mm") ?? string.Empty;
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
