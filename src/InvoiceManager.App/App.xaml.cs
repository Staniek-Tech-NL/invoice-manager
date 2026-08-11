using System.Windows;
using InvoiceManager.Application;
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

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
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
}
