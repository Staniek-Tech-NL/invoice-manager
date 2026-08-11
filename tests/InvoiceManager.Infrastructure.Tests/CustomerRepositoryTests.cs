using InvoiceManager.Domain.Customers;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class CustomerRepositoryTests
{
    [Fact]
    public async Task SearchExcludesArchivedCustomersByDefaultAndMatchesLiteralWildcards()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var factory = await CreateFactoryAsync(connection);
        var repository = new CustomerRepository(factory);
        var timestamp = new DateTimeOffset(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);
        var activeCustomer = CreateCustomer("100% Studio", "studio@example.com", timestamp);
        var archivedCustomer = CreateCustomer("Archived BV", "archive@example.com", timestamp);

        await repository.AddAsync(activeCustomer, CancellationToken.None);
        await repository.AddAsync(archivedCustomer, CancellationToken.None);
        archivedCustomer.Archive(timestamp.AddDays(1));
        await repository.UpdateAsync(archivedCustomer, CancellationToken.None);

        var activeResults = await repository.SearchAsync(null, includeArchived: false, CancellationToken.None);
        var wildcardResults = await repository.SearchAsync("%", includeArchived: true, CancellationToken.None);
        var allResults = await repository.SearchAsync(null, includeArchived: true, CancellationToken.None);

        Assert.Equal(activeCustomer.Id, Assert.Single(activeResults).Id);
        Assert.Equal(activeCustomer.Id, Assert.Single(wildcardResults).Id);
        Assert.Equal(2, allResults.Count);
    }

    private static Customer CreateCustomer(string companyName, string email, DateTimeOffset utcNow)
    {
        return Customer.Create(
            companyName,
            null,
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            email,
            null,
            null,
            null,
            utcNow);
    }

    private static async Task<IDbContextFactory<InvoiceManagerDbContext>> CreateFactoryAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>()
            .UseSqlite(connection)
            .Options;
        var factory = new TestDbContextFactory(options);

        await using var context = factory.CreateDbContext();
        await context.Database.MigrateAsync(CancellationToken.None);
        return factory;
    }

    private sealed class TestDbContextFactory(
        DbContextOptions<InvoiceManagerDbContext> options) : IDbContextFactory<InvoiceManagerDbContext>
    {
        public InvoiceManagerDbContext CreateDbContext()
        {
            return new InvoiceManagerDbContext(options);
        }
    }
}
