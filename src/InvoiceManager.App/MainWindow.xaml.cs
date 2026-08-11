using System.Windows;
using InvoiceManager.App.ViewModels;

namespace InvoiceManager.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
