using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class QuoteRepositoryTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddAllocatesSequentialNumbersAndPersistsSnapshotsAndItems()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("Customer BV");
        await AddCustomerAsync(database.Factory, customer);
        var repository = new QuoteRepository(database.Factory);
        var first = CreateQuote(customer);
        var second = CreateQuote(customer);

        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(first.Id, CancellationToken.None);

        Assert.Equal("Q-2026-0001", first.Number);
        Assert.Equal("Q-2026-0002", second.Number);
        Assert.NotNull(loaded);
        Assert.Equal("Issuer BV", loaded.Issuer.CompanyName);
        Assert.Equal("Customer BV", loaded.Customer.CompanyName);
        Assert.Equal(181.50m, loaded.Total);
        Assert.Single(loaded.Items);
    }

    [Fact]
    public async Task UpdateReplacesDraftItemsWithoutChangingNumberOrSnapshots()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("Customer BV");
        await AddCustomerAsync(database.Factory, customer);
        var repository = new QuoteRepository(database.Factory);
        var quote = CreateQuote(customer);
        await repository.AddAsync(quote, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(quote.Id, CancellationToken.None);
        Assert.NotNull(loaded);

        loaded.UpdateDraft(
            loaded.IssueDate,
            loaded.ValidUntil,
            "Updated",
            [new QuoteItemDraft("Consulting", 3m, "hour", 80m, 0.09m)],
            UtcNow.AddDays(1));
        await repository.UpdateAsync(loaded, CancellationToken.None);
        var updated = await repository.GetByIdAsync(quote.Id, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Q-2026-0001", updated.Number);
        Assert.Equal("Updated", updated.Notes);
        Assert.Equal("Issuer BV", updated.Issuer.CompanyName);
        Assert.Equal("Consulting", Assert.Single(updated.Items).Description);
        Assert.Equal(261.60m, updated.Total);
    }

    [Fact]
    public async Task SearchEscapesWildcardsAndExpirationUpdatesEligibleQuotes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("100% Studio");
        await AddCustomerAsync(database.Factory, customer);
        var repository = new QuoteRepository(database.Factory);
        var quote = CreateQuote(customer, validUntil: new DateOnly(2026, 8, 10));
        await repository.AddAsync(quote, CancellationToken.None);

        var literalResults = await repository.SearchAsync("%", CancellationToken.None);
        await repository.ExpireEligibleAsync(new DateOnly(2026, 8, 11), UtcNow, CancellationToken.None);
        var expired = await repository.GetByIdAsync(quote.Id, CancellationToken.None);

        Assert.Single(literalResults);
        Assert.Equal(QuoteStatus.Expired, expired?.Status);
    }

    [Fact]
    public async Task ConcurrentAddsAllocateUniqueOrderedNumbers()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"invoice-manager-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Pooling = false,
            }.ToString();
            var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;
            var factory = new TestDbContextFactory(options);
            await using (var context = factory.CreateDbContext())
            {
                await context.Database.MigrateAsync(CancellationToken.None);
            }

            var customer = CreateCustomer("Concurrent BV");
            await AddCustomerAsync(factory, customer);
            var repository = new QuoteRepository(factory);
            var quotes = Enumerable.Range(0, 5).Select(_ => CreateQuote(customer)).ToArray();

            await Task.WhenAll(quotes.Select(quote => repository.AddAsync(quote, CancellationToken.None)));

            Assert.Equal(
                ["Q-2026-0001", "Q-2026-0002", "Q-2026-0003", "Q-2026-0004", "Q-2026-0005"],
                quotes.Select(quote => quote.Number).Order(StringComparer.Ordinal));
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private static Customer CreateCustomer(string companyName)
    {
        return Customer.Create(
            companyName,
            null,
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            "customer@example.com",
            null,
            null,
            null,
            UtcNow);
    }

    private static Quote CreateQuote(Customer customer, DateOnly? validUntil = null)
    {
        return Quote.Create(
            customer.Id,
            IssuerSnapshot.Create(
                "Issuer BV",
                "Issuer Street 1",
                "2000 AB",
                "Rotterdam",
                "Netherlands",
                "NL123",
                null,
                null,
                "issuer@example.com",
                null),
            CustomerSnapshot.Create(
                customer.CompanyName,
                customer.ContactPerson,
                customer.Street,
                customer.PostalCode,
                customer.City,
                customer.Country,
                customer.Email,
                customer.Phone,
                customer.VatNumber),
            new DateOnly(2026, 8, 1),
            validUntil ?? new DateOnly(2026, 8, 31),
            null,
            [new QuoteItemDraft("Development", 2m, "hour", 75m, 0.21m)],
            UtcNow);
    }

    private static async Task AddCustomerAsync(
        IDbContextFactory<InvoiceManagerDbContext> factory,
        Customer customer)
    {
        await using var context = await factory.CreateDbContextAsync(CancellationToken.None);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, IDbContextFactory<InvoiceManagerDbContext> factory)
        {
            _connection = connection;
            Factory = factory;
        }

        public IDbContextFactory<InvoiceManagerDbContext> Factory { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync(CancellationToken.None);
            var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>().UseSqlite(connection).Options;
            var factory = new TestDbContextFactory(options);
            await using var context = factory.CreateDbContext();
            await context.Database.MigrateAsync(CancellationToken.None);
            return new TestDatabase(connection, factory);
        }

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }

    private sealed class TestDbContextFactory(
        DbContextOptions<InvoiceManagerDbContext> options) : IDbContextFactory<InvoiceManagerDbContext>
    {
        public InvoiceManagerDbContext CreateDbContext() => new(options);
    }
}
