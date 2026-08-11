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

public sealed class CustomersViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Customers,
        "Customers",
        "Customer management will be implemented in Milestone 2.");

public sealed class ServicesViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Services,
        "Services",
        "Products and services will be implemented in Milestone 2.");

public sealed class QuotesViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Quotes,
        "Quotes",
        "Quotation workflows will be implemented in Milestone 3.");

public sealed class InvoicesViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Invoices,
        "Invoices",
        "Invoice workflows will be implemented in Milestone 4.");

public sealed class SettingsViewModel()
    : FeaturePlaceholderViewModel(
        NavigationDestination.Settings,
        "Settings",
        "Company and invoice defaults will be added in upcoming milestones.");
