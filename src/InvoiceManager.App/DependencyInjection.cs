using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.App;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<CustomersViewModel>();
        services.AddSingleton<ServicesViewModel>();
        services.AddSingleton<QuotesViewModel>();
        services.AddSingleton<InvoicesViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<DashboardViewModel>());
        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<CustomersViewModel>());
        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<ServicesViewModel>());
        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<QuotesViewModel>());
        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<InvoicesViewModel>());
        services.AddSingleton<INavigationPage>(provider => provider.GetRequiredService<SettingsViewModel>());

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUserDialogService, UserDialogService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
