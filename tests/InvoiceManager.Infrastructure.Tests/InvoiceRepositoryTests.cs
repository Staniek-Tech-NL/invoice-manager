using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class InvoiceRepositoryTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DirectInvoicesUseIndependentSequenceAndPersistItems()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("Customer BV");
        await AddCustomerAsync(database.Factory, customer);
        var quoteRepository = new QuoteRepository(database.Factory);
        var quote = CreateQuote(customer);
        await quoteRepository.AddAsync(quote, CancellationToken.None);
        var repository = new InvoiceRepository(database.Factory);
        var first = CreateInvoice(customer);
        var second = CreateInvoice(customer);

        await repository.AddAsync(first, CancellationToken.None);
        await repository.AddAsync(second, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(first.Id, CancellationToken.None);

        Assert.Equal("Q-2026-0001", quote.Number);
        Assert.Equal("INV-2026-0001", first.Number);
        Assert.Equal("INV-2026-0002", second.Number);
        Assert.Equal(181.50m, loaded?.Total);
        Assert.Single(loaded?.Items ?? []);
    }

    [Fact]
    public async Task UpdateReplacesDraftItemsAndSearchEscapesWildcards()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("100% Studio");
        await AddCustomerAsync(database.Factory, customer);
        var repository = new InvoiceRepository(database.Factory);
        var invoice = CreateInvoice(customer);
        await repository.AddAsync(invoice, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(invoice.Id, CancellationToken.None);
        Assert.NotNull(loaded);
        loaded.UpdateDraft(
            loaded.IssueDate,
            loaded.DueDate,
            "Updated",
            [new InvoiceItemDraft("Consulting", 3m, "hour", 80m, 0.09m)],
            UtcNow.AddDays(1));
        await repository.UpdateAsync(loaded, CancellationToken.None);

        var results = await repository.SearchAsync("%", CancellationToken.None);
        var updated = Assert.Single(results);
        Assert.Equal("Consulting", Assert.Single(updated.Items).Description);
        Assert.Equal(261.60m, updated.Total);
    }

    [Fact]
    public async Task AcceptedQuoteConversionIsAtomicAndCannotBeRepeated()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("Customer BV");
        await AddCustomerAsync(database.Factory, customer);
        var quoteRepository = new QuoteRepository(database.Factory);
        var quote = CreateQuote(customer);
        await quoteRepository.AddAsync(quote, CancellationToken.None);
        quote.MarkSent(UtcNow.AddHours(1));
        quote.Accept(UtcNow.AddHours(2));
        await quoteRepository.UpdateAsync(quote, CancellationToken.None);
        var converter = new QuoteToInvoiceConverter(database.Factory);

        var invoice = await converter.ConvertAsync(
            quote.Id,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow.AddDays(1),
            CancellationToken.None);

        Assert.Equal("INV-2026-0001", invoice.Number);
        Assert.Equal(quote.Id, invoice.SourceQuoteId);
        Assert.Equal(quote.Total, invoice.Total);
        Assert.Equal(quote.Customer.CompanyName, invoice.Customer.CompanyName);
        await Assert.ThrowsAsync<InvalidOperationException>(() => converter.ConvertAsync(
            quote.Id,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow.AddDays(1),
            CancellationToken.None));
        Assert.NotNull(await quoteRepository.GetByIdAsync(quote.Id, CancellationToken.None));
    }

    [Fact]
    public async Task FailedConversionDoesNotConsumeInvoiceNumber()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = CreateCustomer("Customer BV");
        await AddCustomerAsync(database.Factory, customer);
        var quoteRepository = new QuoteRepository(database.Factory);
        var quote = CreateQuote(customer);
        await quoteRepository.AddAsync(quote, CancellationToken.None);
        var converter = new QuoteToInvoiceConverter(database.Factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => converter.ConvertAsync(
            quote.Id,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow,
            CancellationToken.None));
        quote.MarkSent(UtcNow.AddHours(1));
        quote.Accept(UtcNow.AddHours(2));
        await quoteRepository.UpdateAsync(quote, CancellationToken.None);
        var invoice = await converter.ConvertAsync(
            quote.Id,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow,
            CancellationToken.None);

        Assert.Equal("INV-2026-0001", invoice.Number);
    }

    [Fact]
    public async Task ConcurrentInvoiceAddsAllocateUniqueOrderedNumbers()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"invoice-manager-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, Pooling = false }.ToString();
            var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>().UseSqlite(connectionString).Options;
            var factory = new TestDbContextFactory(options);
            await using (var context = factory.CreateDbContext())
            {
                await context.Database.MigrateAsync(CancellationToken.None);
            }

            var customer = CreateCustomer("Concurrent BV");
            await AddCustomerAsync(factory, customer);
            var repository = new InvoiceRepository(factory);
            var invoices = Enumerable.Range(0, 5).Select(_ => CreateInvoice(customer)).ToArray();
            await Task.WhenAll(invoices.Select(invoice => repository.AddAsync(invoice, CancellationToken.None)));

            Assert.Equal(
                ["INV-2026-0001", "INV-2026-0002", "INV-2026-0003", "INV-2026-0004", "INV-2026-0005"],
                invoices.Select(invoice => invoice.Number).Order(StringComparer.Ordinal));
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private static Customer CreateCustomer(string companyName) => Customer.Create(
        companyName, null, "Main Street 1", "1000 AA", "Amsterdam", "Netherlands",
        "customer@example.com", null, null, null, UtcNow);

    private static IssuerSnapshot CreateIssuer() => IssuerSnapshot.Create(
        "Issuer BV", "Issuer Street 1", "2000 AB", "Rotterdam", "Netherlands",
        "NL123", null, null, "issuer@example.com", null);

    private static CustomerSnapshot CreateCustomerSnapshot(Customer customer) => CustomerSnapshot.Create(
        customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
        customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber);

    private static Invoice CreateInvoice(Customer customer) => Invoice.Create(
        customer.Id,
        CreateIssuer(),
        CreateCustomerSnapshot(customer),
        new DateOnly(2026, 8, 11),
        new DateOnly(2026, 9, 10),
        null,
        [new InvoiceItemDraft("Development", 2m, "hour", 75m, 0.21m)],
        UtcNow);

    private static Quote CreateQuote(Customer customer) => Quote.Create(
        customer.Id,
        CreateIssuer(),
        CreateCustomerSnapshot(customer),
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 31),
        "Quote notes",
        [new QuoteItemDraft("Development", 2m, "hour", 75m, 0.21m)],
        UtcNow);

    private static async Task AddCustomerAsync(IDbContextFactory<InvoiceManagerDbContext> factory, Customer customer)
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
