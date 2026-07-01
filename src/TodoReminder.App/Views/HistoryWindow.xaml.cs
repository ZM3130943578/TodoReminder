using System.Collections.ObjectModel;
using System.Windows;
using TodoReminder.Application.Services;

namespace TodoReminder.App.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow(List<HistoryItemDto> items)
    {
        InitializeComponent();
        DataContext = new ObservableCollection<HistoryItemDto>(items);
    }
}
