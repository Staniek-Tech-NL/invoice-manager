using InvoiceManager.App.Navigation;

namespace InvoiceManager.App.ViewModels;

public sealed class DashboardViewModel : INavigationPage
{
    public NavigationDestination Destination => NavigationDestination.Dashboard;

    public string Title => "Dashboard";
}
