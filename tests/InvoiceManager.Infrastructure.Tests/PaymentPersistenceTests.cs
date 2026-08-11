using InvoiceManager.Application.Payments;
using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class PaymentPersistenceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 11);

    [Fact]
    public async Task PartialAndFullPaymentsPersistAndSetPaidStatus()
    {
        await using var database = await TestDatabase.CreateAsync();
        var invoice = await CreateSentInvoiceAsync(database.Factory);
        var service = new InvoicePaymentService(database.Factory);

        await service.RegisterAsync(
            invoice.Id,
            new PaymentInput(Today, 40m, "PARTIAL", "Bank transfer"),
            Today,
            UtcNow,
            CancellationToken.None);
        await service.RegisterAsync(
            invoice.Id,
            new PaymentInput(Today, 81m, "FINAL", "Bank transfer"),
            Today,
            UtcNow.AddHours(1),
            CancellationToken.None);
        var loaded = await new InvoiceRepository(database.Factory).GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(121m, loaded.PaidAmount);
        Assert.Equal(0m, loaded.OutstandingAmount);
        Assert.Equal(InvoiceStatus.Paid, loaded.Status);
        Assert.Equal(2, loaded.Payments.Count);
    }

    [Fact]
    public async Task OverpaymentRollsBackWithoutPersistingPayment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var invoice = await CreateSentInvoiceAsync(database.Factory);
        var service = new InvoicePaymentService(database.Factory);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(
            invoice.Id,
            new PaymentInput(Today, 121.01m, null, null),
            Today,
            UtcNow,
            CancellationToken.None));
        var loaded = await new InvoiceRepository(database.Factory).GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Empty(loaded.Payments);
        Assert.Equal(121m, loaded.OutstandingAmount);
        Assert.Equal(InvoiceStatus.Sent, loaded.Status);
    }

    [Fact]
    public async Task VoidingPaymentPreservesAuditRecordAndRestoresOverdueStatus()
    {
        await using var database = await TestDatabase.CreateAsync();
        var invoice = await CreateSentInvoiceAsync(database.Factory, dueDate: Today.AddDays(-1));
        var service = new InvoicePaymentService(database.Factory);
        var paid = await service.RegisterAsync(
            invoice.Id,
            new PaymentInput(Today, 121m, "DUP", "Bank transfer"),
            Today,
            UtcNow,
            CancellationToken.None);
        var payment = Assert.Single(paid.Payments);

        await service.VoidAsync(
            invoice.Id,
            payment.Id,
            "Duplicate import",
            Today,
            UtcNow.AddHours(1),
            CancellationToken.None);
        var loaded = await new InvoiceRepository(database.Factory).GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(InvoiceStatus.Overdue, loaded.Status);
        Assert.Equal(121m, loaded.OutstandingAmount);
        var voided = Assert.Single(loaded.Payments);
        Assert.True(voided.IsVoided);
        Assert.Equal("Duplicate import", voided.VoidReason);
        Assert.Equal(UtcNow.AddHours(1), voided.VoidedAt);
    }

    [Fact]
    public async Task ConcurrentPaymentsCannotOverpayInvoice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var invoice = await CreateSentInvoiceAsync(database.Factory);
        var service = new InvoicePaymentService(database.Factory);
        var attempts = new[]
        {
            CaptureAsync(() => service.RegisterAsync(
                invoice.Id, new PaymentInput(Today, 80m, null, null), Today, UtcNow, CancellationToken.None)),
            CaptureAsync(() => service.RegisterAsync(
                invoice.Id, new PaymentInput(Today, 50m, null, null), Today, UtcNow, CancellationToken.None)),
        };

        var results = await Task.WhenAll(attempts);
        var loaded = await new InvoiceRepository(database.Factory).GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.Single(results, result => result is null);
        Assert.Single(results, result => result is InvalidOperationException);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Payments);
        Assert.InRange(loaded.PaidAmount, 50m, 80m);
        Assert.True(loaded.OutstandingAmount > 0m);
    }

    [Fact]
    public async Task RefreshStatusesPersistsOverdueState()
    {
        await using var database = await TestDatabase.CreateAsync();
        var invoice = await CreateSentInvoiceAsync(database.Factory, dueDate: Today.AddDays(-1));
        var repository = new InvoiceRepository(database.Factory);

        await repository.RefreshStatusesAsync(Today, UtcNow, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(invoice.Id, CancellationToken.None);

        Assert.Equal(InvoiceStatus.Overdue, loaded?.Status);
    }

    private static async Task<Exception?> CaptureAsync(Func<Task<Invoice>> operation)
    {
        try
        {
            await operation();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static async Task<Invoice> CreateSentInvoiceAsync(
        IDbContextFactory<InvoiceManagerDbContext> factory,
        DateOnly? dueDate = null)
    {
        var customer = Customer.Create(
            "Customer BV", null, "Street 1", "1000 AA", "Amsterdam", "Netherlands",
            null, null, null, null, UtcNow);
        await using (var context = await factory.CreateDbContextAsync(CancellationToken.None))
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var invoice = Invoice.Create(
            customer.Id,
            IssuerSnapshot.Create(
                "Issuer BV", "Street 2", "2000 AB", "Rotterdam", "Netherlands",
                null, null, null, null, null),
            CustomerSnapshot.Create(
                customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
                customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber),
            Today.AddDays(-30),
            dueDate ?? Today.AddDays(30),
            null,
            [new InvoiceItemDraft("Service", 1m, "item", 100m, 0.21m)],
            UtcNow);
        var repository = new InvoiceRepository(factory);
        await repository.AddAsync(invoice, CancellationToken.None);
        invoice.MarkSent(UtcNow);
        await repository.UpdateAsync(invoice, CancellationToken.None);
        return invoice;
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
