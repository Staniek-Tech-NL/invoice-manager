using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Tests;

public sealed class InvoiceUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateInvoiceCapturesSnapshotsAndPersistsCalculatedInvoice()
    {
        var customer = CreateCustomer();
        var repository = new InvoiceRepositoryStub();
        var useCase = new CreateInvoice(
            repository,
            new CustomerRepositoryStub(customer),
            new SettingsRepositoryStub(CreateSettings()),
            new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(CreateInput(customer.Id), CancellationToken.None);

        var invoice = Assert.Single(repository.Invoices);
        Assert.Equal("INV-2026-0001", result.Number);
        Assert.Equal("Customer BV", invoice.Customer.CompanyName);
        Assert.Equal("Issuer BV", invoice.Issuer.CompanyName);
        Assert.Equal(181.50m, result.Total);
    }

    [Fact]
    public async Task CreateInvoiceRejectsArchivedCustomer()
    {
        var customer = CreateCustomer();
        customer.Archive(UtcNow.AddDays(1));
        var useCase = new CreateInvoice(
            new InvoiceRepositoryStub(),
            new CustomerRepositoryStub(customer),
            new SettingsRepositoryStub(CreateSettings()),
            new FixedClock(UtcNow));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(CreateInput(customer.Id), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateInvoiceRecalculatesDraftAndPreservesNumber()
    {
        var customer = CreateCustomer();
        var invoice = CreateInvoiceEntity(customer);
        invoice.AssignNumber("INV-2026-0001");
        var repository = new InvoiceRepositoryStub(invoice);
        var useCase = new UpdateInvoice(repository, new CustomerRepositoryStub(customer), new FixedClock(UtcNow.AddDays(1)));
        var input = CreateInput(customer.Id) with
        {
            Items = [new InvoiceItemInput("Consulting", 1m, "hour", 100m, 0.09m)],
        };

        var result = await useCase.ExecuteAsync(invoice.Id, input, CancellationToken.None);

        Assert.Equal("INV-2026-0001", result.Number);
        Assert.Equal(109m, result.Total);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task ChangeInvoiceStatusMarksDraftAsSent()
    {
        var customer = CreateCustomer();
        var invoice = CreateInvoiceEntity(customer);
        invoice.AssignNumber("INV-2026-0001");
        var repository = new InvoiceRepositoryStub(invoice);
        var useCase = new ChangeInvoiceStatus(repository, new FixedClock(UtcNow.AddDays(1)));

        var result = await useCase.ExecuteAsync(invoice.Id, InvoiceStatus.Sent, CancellationToken.None);

        Assert.Equal(InvoiceStatus.Sent, result.Status);
    }

    [Fact]
    public async Task ConvertQuoteUsesCurrentDateAndDefaultPaymentTerm()
    {
        var quote = CreateAcceptedQuote();
        var converter = new ConverterStub(quote);
        var useCase = new ConvertQuoteToInvoice(
            converter,
            new SettingsRepositoryStub(CreateSettings()),
            new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(quote.Id, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 8, 11), result.IssueDate);
        Assert.Equal(new DateOnly(2026, 9, 10), result.DueDate);
        Assert.Equal(quote.Id, result.SourceQuoteId);
    }

    private static InvoiceInput CreateInput(Guid customerId) => new(
        customerId,
        new DateOnly(2026, 8, 11),
        new DateOnly(2026, 9, 10),
        null,
        [new InvoiceItemInput("Development", 2m, "hour", 75m, 0.21m)]);

    private static Customer CreateCustomer() => Customer.Create(
        "Customer BV", null, "Customer Street 1", "1000 AA", "Amsterdam", "Netherlands",
        "customer@example.com", null, null, null, UtcNow);

    private static CompanySettings CreateSettings() => CompanySettings.Create(
        "Issuer BV", "Issuer Street 1", "2000 AB", "Rotterdam", "Netherlands",
        "NL123", null, null, "issuer@example.com", null, 0.21m, 30, "EUR", null);

    private static Invoice CreateInvoiceEntity(Customer customer) => Invoice.Create(
        customer.Id,
        CreateSettings().CreateSnapshot(),
        CustomerSnapshot.Create(
            customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
            customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber),
        new DateOnly(2026, 8, 11),
        new DateOnly(2026, 9, 10),
        null,
        [new InvoiceItemDraft("Development", 2m, "hour", 75m, 0.21m)],
        UtcNow);

    private static Quote CreateAcceptedQuote()
    {
        var customer = CreateCustomer();
        var quote = Quote.Create(
            customer.Id,
            CreateSettings().CreateSnapshot(),
            CustomerSnapshot.Create(
                customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
                customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            null,
            [new QuoteItemDraft("Development", 2m, "hour", 75m, 0.21m)],
            UtcNow);
        quote.MarkSent(UtcNow.AddHours(1));
        quote.Accept(UtcNow.AddHours(2));
        return quote;
    }

    private sealed class InvoiceRepositoryStub(params Invoice[] invoices) : IInvoiceRepository
    {
        public List<Invoice> Invoices { get; } = [.. invoices];

        public int UpdateCount { get; private set; }

        public Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            invoice.AssignNumber($"INV-{invoice.IssueDate.Year}-{Invoices.Count + 1:0000}");
            Invoices.Add(invoice);
            return Task.CompletedTask;
        }

        public Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) =>
            Task.FromResult(Invoices.SingleOrDefault(invoice => invoice.Id == invoiceId));

        public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Invoice>> SearchAsync(string? searchTerm, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Invoice>>(Invoices);
    }

    private sealed class CustomerRepositoryStub(params Customer[] customers) : ICustomerRepository
    {
        public Task AddAsync(Customer customer, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(customers.SingleOrDefault(customer => customer.Id == customerId));

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<Customer>> SearchAsync(string? searchTerm, bool includeArchived, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Customer>>(customers);
    }

    private sealed class SettingsRepositoryStub(CompanySettings settings) : ICompanySettingsRepository
    {
        public Task<CompanySettings?> GetAsync(CancellationToken cancellationToken) => Task.FromResult<CompanySettings?>(settings);

        public Task SaveAsync(CompanySettings value, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ConverterStub(Quote quote) : IQuoteToInvoiceConverter
    {
        public Task<Invoice> ConvertAsync(
            Guid quoteId,
            DateOnly issueDate,
            DateOnly dueDate,
            DateTimeOffset utcNow,
            CancellationToken cancellationToken)
        {
            var invoice = Invoice.CreateFromQuote(quote, issueDate, dueDate, utcNow);
            invoice.AssignNumber($"INV-{issueDate.Year}-0001");
            return Task.FromResult(invoice);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IApplicationClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
