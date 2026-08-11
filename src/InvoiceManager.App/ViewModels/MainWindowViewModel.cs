using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;

namespace InvoiceManager.App.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private INavigationPage _currentPage;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _currentPage = navigationService.CurrentPage;
        _navigationService.CurrentPageChanged += OnCurrentPageChanged;
    }

    [RelayCommand]
    private void Navigate(NavigationDestination destination)
    {
        _navigationService.NavigateTo(destination);
    }

    private void OnCurrentPageChanged(object? sender, EventArgs e)
    {
        CurrentPage = _navigationService.CurrentPage;
    }
}
