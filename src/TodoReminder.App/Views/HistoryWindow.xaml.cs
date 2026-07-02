using System.Collections.ObjectModel;
using System.Windows;
using TodoReminder.Application.Services;

namespace TodoReminder.App.Views;

public partial class HistoryWindow : Window
{
    private readonly ITodoService _todoService;

    public HistoryWindow(ITodoService todoService)
    {
        InitializeComponent();
        _todoService = todoService;
        Owner = System.Windows.Application.Current.MainWindow;
        _ = LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync(DateOnly? from = null, DateOnly? to = null)
    {
        try
        {
            var items = await _todoService.GetHistoryAsync(from, to);
            HistoryList.ItemsSource = new ObservableCollection<HistoryItemDto>(items);
            CountText.Text = items.Count.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载历史记录失败：{ex.Message}",
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSearch(object sender, RoutedEventArgs e)
    {
        DateOnly? from = FromDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(FromDatePicker.SelectedDate.Value) : null;
        DateOnly? to = ToDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(ToDatePicker.SelectedDate.Value) : null;
        _ = LoadHistoryAsync(from, to);
    }

    private void OnShowAll(object sender, RoutedEventArgs e)
    {
        FromDatePicker.SelectedDate = null;
        ToDatePicker.SelectedDate = null;
        _ = LoadHistoryAsync();
    }
}
