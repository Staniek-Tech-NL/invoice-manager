namespace InvoiceManager.App.Navigation;

public interface INavigationService
{
    event EventHandler? CurrentPageChanged;

    INavigationPage CurrentPage { get; }

    void NavigateTo(NavigationDestination destination);
}
