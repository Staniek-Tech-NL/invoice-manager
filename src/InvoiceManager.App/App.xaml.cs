using System.Windows;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.Application;
using InvoiceManager.Application.Demo;
using InvoiceManager.Infrastructure;
using InvoiceManager.Infrastructure.Logging;
using InvoiceManager.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _host = CreateHost();
            await _host.StartAsync();

            if (e.Args.Contains("--demo", StringComparer.OrdinalIgnoreCase))
            {
                await _host.Services.GetRequiredService<IDemoDataSeeder>().SeedAsync();
            }

            var navigation = _host.Services.GetRequiredService<INavigationService>();
            if (navigation.CurrentPage is IActivatableNavigationPage activatablePage)
            {
                await activatablePage.ActivateAsync();
            }

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();

            var portfolioOutput = GetOptionValue(e.Args, "--capture-portfolio");
            if (portfolioOutput is not null)
            {
                await _host.Services.GetRequiredService<PortfolioCaptureService>().CaptureAsync(portfolioOutput);
                MainWindow.Close();
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"Invoice Manager could not start.{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                "Startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static IHost CreateHost()
    {
        var applicationPaths = ApplicationPaths.CreateDefault();
        applicationPaths.EnsureDirectoriesExist();

        var builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddDebug();
        builder.Logging.AddProvider(
            new FileLoggerProvider(applicationPaths.LogsDirectory, TimeProvider.System));

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(applicationPaths);
        builder.Services.AddPresentation();

        return builder.Build();
    }

    private static string? GetOptionValue(string[] arguments, string option)
    {
        for (var index = 0; index < arguments.Length - 1; index++)
        {
            if (string.Equals(arguments[index], option, StringComparison.OrdinalIgnoreCase))
            {
                return arguments[index + 1];
            }
        }

        return null;
    }
}
