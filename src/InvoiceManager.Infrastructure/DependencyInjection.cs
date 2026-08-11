using InvoiceManager.Application.Common.Storage;
using InvoiceManager.Application.Common.Time;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Storage;
using InvoiceManager.Infrastructure.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ApplicationPaths applicationPaths)
    {
        ArgumentNullException.ThrowIfNull(applicationPaths);

        applicationPaths.EnsureDirectoriesExist();

        services.AddSingleton(applicationPaths);
        services.AddSingleton<IApplicationPaths>(applicationPaths);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(TimeZoneInfo.Local);
        services.AddSingleton<IApplicationClock, SystemApplicationClock>();

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = applicationPaths.DatabasePath,
            ForeignKeys = true,
        }.ToString();

        services.AddDbContextFactory<InvoiceManagerDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddHostedService<DatabaseInitializationService>();

        return services;
    }
}
