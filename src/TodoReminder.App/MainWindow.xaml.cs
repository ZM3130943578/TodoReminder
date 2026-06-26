using System.Windows;
using TodoReminder.App.ViewModels;

namespace TodoReminder.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
