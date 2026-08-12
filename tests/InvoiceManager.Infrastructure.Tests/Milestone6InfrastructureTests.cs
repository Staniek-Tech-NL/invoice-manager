using InvoiceManager.Application.Dashboard;
using InvoiceManager.Application.Payments;
using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Infrastructure.Dashboard;
using InvoiceManager.Infrastructure.Pdf;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using InvoiceManager.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf.IO;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class Milestone6InfrastructureTests
{
    private static readonly DateOnly Today = new(2026, 8, 12);
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DashboardCalculatesPaymentRevenueBalancesCountsAndRecentInvoices()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Factory);
        var invoiceRepository = new InvoiceRepository(database.Factory);
        var first = await AddSentInvoiceAsync(invoiceRepository, customer, Today.AddDays(-10), Today.AddDays(20), 1m);
        var overdue = await AddSentInvoiceAsync(invoiceRepository, customer, Today.AddDays(-5), Today.AddDays(-1), 2m);
        var draft = Invoice.Create(customer.Id, Issuer().CreateSnapshot(), CustomerSnapshot(customer), Today.AddDays(-20), Today.AddDays(10),
            null, [new InvoiceItemDraft("Draft work", 10m, "item", 100m, 0.21m)], UtcNow);
        await invoiceRepository.AddAsync(draft, CancellationToken.None);
        await new InvoicePaymentService(database.Factory).RegisterAsync(
            first.Id, new PaymentInput(Today, 40m, "AUG", "Bank transfer"), Today, UtcNow, CancellationToken.None);
        await invoiceRepository.RefreshStatusesAsync(Today, UtcNow, CancellationToken.None);

        var result = await new DashboardQuery(database.Factory).GetAsync(Today, CancellationToken.None);

        Assert.Equal(40m, result.RevenueThisMonth);
        Assert.Equal(40m, result.RevenueThisYear);
        Assert.Equal(323m, result.OutstandingAmount);
        Assert.Equal(242m, result.OverdueAmount);
        Assert.Equal(3, result.InvoiceCount);
        Assert.Equal(1, result.CustomerCount);
        Assert.Equal(12, result.MonthlyRevenue.Count);
        Assert.Equal(40m, result.MonthlyRevenue[^1].Amount);
        Assert.Equal(overdue.Id, result.RecentInvoices[0].Id);
    }

    [Fact]
    public async Task PdfGeneratorCreatesReadableInvoiceAndQuoteDocuments()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Factory);
        var invoiceRepository = new InvoiceRepository(database.Factory);
        var quoteRepository = new QuoteRepository(database.Factory);
        var invoice = await AddSentInvoiceAsync(invoiceRepository, customer, Today, Today.AddDays(30), 1m);
        var quote = Quote.Create(
            customer.Id, Issuer().CreateSnapshot(), CustomerSnapshot(customer),
            Today, Today.AddDays(14), "Offer notes",
            [new QuoteItemDraft("Consulting", 2m, "hour", 75m, 0.21m)], UtcNow);
        await quoteRepository.AddAsync(quote, CancellationToken.None);
        var generator = new PdfSharpDocumentGenerator(invoiceRepository, quoteRepository);
        var directory = Path.Combine(Path.GetTempPath(), $"invoice-manager-m6-pdf-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var invoicePath = Path.Combine(directory, "invoice.pdf");
        var quotePath = Path.Combine(directory, "quote.pdf");

        try
        {
            await generator.GenerateInvoiceAsync(invoice.Id, invoicePath, CancellationToken.None);
            await generator.GenerateQuoteAsync(quote.Id, quotePath, CancellationToken.None);

            AssertPdf(invoicePath);
            AssertPdf(quotePath);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task PersistedDocumentKeepsLogoSnapshot()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Factory);
        var repository = new InvoiceRepository(database.Factory);
        var logo = new byte[] { 10, 20, 30, 40 };
        var invoice = Invoice.Create(customer.Id, Issuer().CreateSnapshot(logo), CustomerSnapshot(customer),
            Today, Today.AddDays(30), null, [new InvoiceItemDraft("Service", 1m, "item", 100m, 0.21m)], UtcNow);
        await repository.AddAsync(invoice, CancellationToken.None);

        var loaded = await repository.GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(logo, loaded?.Issuer.LogoContent);
    }

    [Fact]
    public async Task LogoReaderReturnsBytesAndRejectsOversizedFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"invoice-manager-m6-logo-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var validPath = Path.Combine(directory, "logo.png");
        var largePath = Path.Combine(directory, "large.png");
        try
        {
            await File.WriteAllBytesAsync(validPath, [1, 2, 3]);
            await using (var stream = new FileStream(largePath, FileMode.CreateNew, FileAccess.Write))
            {
                stream.SetLength(5 * 1024 * 1024 + 1);
            }
            var reader = new LogoContentReader();

            Assert.Equal(new byte[] { 1, 2, 3 }, await reader.ReadAsync(validPath, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(largePath, CancellationToken.None));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void AssertPdf(string path)
    {
        Assert.True(File.Exists(path));
        Assert.True(new FileInfo(path).Length > 500);
        using var document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        Assert.True(document.PageCount >= 1);
    }

    private static async Task<Customer> AddCustomerAsync(IDbContextFactory<InvoiceManagerDbContext> factory)
    {
        var customer = Customer.Create("Customer BV", null, "Street 1", "1000 AA", "Amsterdam", "Netherlands",
            "customer@example.com", null, "NL123", null, UtcNow);
        await using var context = await factory.CreateDbContextAsync();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    private static async Task<Invoice> AddSentInvoiceAsync(
        InvoiceRepository repository, Customer customer, DateOnly issueDate, DateOnly dueDate, decimal quantity)
    {
        var invoice = Invoice.Create(customer.Id, Issuer().CreateSnapshot(), CustomerSnapshot(customer), issueDate, dueDate,
            null, [new InvoiceItemDraft("Service", quantity, "item", 100m, 0.21m)], UtcNow);
        await repository.AddAsync(invoice, CancellationToken.None);
        invoice.MarkSent(UtcNow);
        await repository.UpdateAsync(invoice, CancellationToken.None);
        return invoice;
    }

    private static InvoiceManager.Domain.Settings.CompanySettings Issuer() =>
        InvoiceManager.Domain.Settings.CompanySettings.Create("Issuer BV", "Street 2", "2000 AB", "Rotterdam", "Netherlands",
            "NL999", "KVK123", "NL00BANK0123456789", "issuer@example.com", "+31 10 123", 0.21m, 30, "EUR", null);

    private static CustomerSnapshot CustomerSnapshot(Customer customer) => InvoiceManager.Domain.Documents.CustomerSnapshot.Create(
        customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
        customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber);

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
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>().UseSqlite(connection).Options;
            var factory = new TestFactory(options);
            await using var context = factory.CreateDbContext();
            await context.Database.MigrateAsync();
            return new TestDatabase(connection, factory);
        }
        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }

    private sealed class TestFactory(DbContextOptions<InvoiceManagerDbContext> options) : IDbContextFactory<InvoiceManagerDbContext>
    {
        public InvoiceManagerDbContext CreateDbContext() => new(options);
    }
}
