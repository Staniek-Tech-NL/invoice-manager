using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Infrastructure.Persistence;

internal sealed partial class DatabaseInitializationService(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory,
    ILogger<DatabaseInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogApplyingMigrations(logger);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);

        LogDatabaseReady(logger);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Applying database migrations.")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Database is ready.")]
    private static partial void LogDatabaseReady(ILogger logger);
}
