using InvoiceManager.App.Navigation;

namespace InvoiceManager.App.ViewModels;

public sealed class DashboardViewModel : INavigationPage
{
    public NavigationDestination Destination => NavigationDestination.Dashboard;

    public string Title => "Dashboard";
}

public abstract class FeaturePlaceholderViewModel(
    NavigationDestination destination,
    string title,
    string description) : INavigationPage
{
    public NavigationDestination Destination { get; } = destination;

    public string Title { get; } = title;

    public string Description { get; } = description;
}

public sealed class InvoicesViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Invoices,
        "Invoices",
        "Invoice workflows will be implemented in Milestone 4.");
