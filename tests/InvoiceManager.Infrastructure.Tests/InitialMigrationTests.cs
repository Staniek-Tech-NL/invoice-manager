using InvoiceManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class InitialMigrationTests
{
    [Fact]
    public async Task InitialMigrationCreatesTheCompleteFoundationSchema()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);

        var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new InvoiceManagerDbContext(options);
        await context.Database.MigrateAsync(CancellationToken.None);

        var tables = await ReadTableNamesAsync(connection);

        Assert.Contains("CompanySettings", tables);
        Assert.Contains("Customers", tables);
        Assert.Contains("ProductServices", tables);
        Assert.Contains("Quotes", tables);
        Assert.Contains("QuoteItems", tables);
        Assert.Contains("Invoices", tables);
        Assert.Contains("InvoiceItems", tables);
        Assert.Contains("Payments", tables);
        Assert.Contains("DocumentNumberSequences", tables);
    }

    private static async Task<HashSet<string>> ReadTableNamesAsync(SqliteConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";

        var tables = new HashSet<string>(StringComparer.Ordinal);
        await using var reader = await command.ExecuteReaderAsync(CancellationToken.None);

        while (await reader.ReadAsync(CancellationToken.None))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
