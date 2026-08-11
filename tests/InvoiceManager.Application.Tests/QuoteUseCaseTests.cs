using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Quotes;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Tests;

public sealed class QuoteUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateQuoteCapturesSnapshotsAndPersistsCalculatedQuote()
    {
        var customer = CreateCustomer();
        var quoteRepository = new QuoteRepositoryStub();
        var useCase = new CreateQuote(
            quoteRepository,
            new CustomerRepositoryStub(customer),
            new SettingsRepositoryStub(CreateSettings()),
            new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(CreateInput(customer.Id), CancellationToken.None);

        var quote = Assert.Single(quoteRepository.Quotes);
        Assert.Equal("Q-2026-0001", result.Number);
        Assert.Equal("Customer BV", quote.Customer.CompanyName);
        Assert.Equal("Issuer BV", quote.Issuer.CompanyName);
        Assert.Equal(181.50m, result.Total);
    }

    [Fact]
    public async Task CreateQuoteRequiresCompanySettings()
    {
        var customer = CreateCustomer();
        var useCase = new CreateQuote(
            new QuoteRepositoryStub(),
            new CustomerRepositoryStub(customer),
            new SettingsRepositoryStub(null),
            new FixedClock(UtcNow));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(CreateInput(customer.Id), CancellationToken.None));

        Assert.Contains("company settings", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateQuoteRejectsArchivedCustomer()
    {
        var customer = CreateCustomer();
        customer.Archive(UtcNow.AddDays(1));
        var useCase = new CreateQuote(
            new QuoteRepositoryStub(),
            new CustomerRepositoryStub(customer),
            new SettingsRepositoryStub(CreateSettings()),
            new FixedClock(UtcNow));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(CreateInput(customer.Id), CancellationToken.None));
    }

    [Fact]
    public async Task ChangeQuoteStatusFollowsDomainTransition()
    {
        var quote = CreateQuoteEntity();
        quote.AssignNumber("Q-2026-0001");
        var repository = new QuoteRepositoryStub(quote);
        var useCase = new ChangeQuoteStatus(repository, new FixedClock(UtcNow.AddDays(1)));

        var result = await useCase.ExecuteAsync(quote.Id, QuoteStatus.Sent, CancellationToken.None);

        Assert.Equal(QuoteStatus.Sent, result.Status);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task SearchQuotesExpiresEligibleDocumentsBeforeReturningResults()
    {
        var quote = CreateQuoteEntity(validUntil: new DateOnly(2026, 8, 10));
        quote.AssignNumber("Q-2026-0001");
        var repository = new QuoteRepositoryStub(quote);
        var useCase = new SearchQuotes(repository, new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(null, CancellationToken.None);

        Assert.Equal(QuoteStatus.Expired, Assert.Single(result).Status);
    }

    private static QuoteInput CreateInput(Guid customerId)
    {
        return new QuoteInput(
            customerId,
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 9, 10),
            null,
            [new QuoteItemInput("Development", 2m, "hour", 75m, 0.21m)]);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Create(
            "Customer BV",
            "Alex Example",
            "Customer Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            "customer@example.com",
            null,
            null,
            null,
            UtcNow);
    }

    private static CompanySettings CreateSettings()
    {
        return CompanySettings.Create(
            "Issuer BV",
            "Issuer Street 1",
            "2000 AB",
            "Rotterdam",
            "Netherlands",
            "NL123",
            null,
            null,
            "issuer@example.com",
            null,
            0.21m,
            30,
            "EUR",
            null);
    }

    private static Quote CreateQuoteEntity(DateOnly? validUntil = null)
    {
        var customer = CreateCustomer();
        return Quote.Create(
            customer.Id,
            CreateSettings().CreateSnapshot(),
            InvoiceManager.Domain.Documents.CustomerSnapshot.Create(
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
            [new QuoteItemDraft("Development", 1m, "hour", 75m, 0.21m)],
            UtcNow);
    }

    private sealed class QuoteRepositoryStub(params Quote[] quotes) : IQuoteRepository
    {
        public List<Quote> Quotes { get; } = [.. quotes];

        public int UpdateCount { get; private set; }

        public Task AddAsync(Quote quote, CancellationToken cancellationToken)
        {
            quote.AssignNumber($"Q-{quote.IssueDate.Year}-{Quotes.Count + 1:0000}");
            Quotes.Add(quote);
            return Task.CompletedTask;
        }

        public Task<Quote?> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Quotes.SingleOrDefault(quote => quote.Id == quoteId));
        }

        public Task UpdateAsync(Quote quote, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Quote>> SearchAsync(string? searchTerm, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Quote>>(Quotes);
        }

        public Task ExpireEligibleAsync(DateOnly today, DateTimeOffset utcNow, CancellationToken cancellationToken)
        {
            foreach (var quote in Quotes)
            {
                quote.Expire(today, utcNow);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class CustomerRepositoryStub(params Customer[] customers) : ICustomerRepository
    {
        public Task AddAsync(Customer customer, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return Task.FromResult(customers.SingleOrDefault(customer => customer.Id == customerId));
        }

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<Customer>> SearchAsync(
            string? searchTerm,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Customer>>(customers);
        }
    }

    private sealed class SettingsRepositoryStub(CompanySettings? settings) : ICompanySettingsRepository
    {
        public Task<CompanySettings?> GetAsync(CancellationToken cancellationToken) => Task.FromResult(settings);

        public Task SaveAsync(CompanySettings value, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IApplicationClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
