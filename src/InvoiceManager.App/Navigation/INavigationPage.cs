namespace InvoiceManager.App.Navigation;

public interface INavigationPage
{
    NavigationDestination Destination { get; }

    string Title { get; }
}
