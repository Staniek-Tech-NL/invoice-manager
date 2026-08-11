using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Domain.Customers;

namespace InvoiceManager.Application.Tests;

public sealed class CustomerUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateCustomerPersistsValidatedCustomer()
    {
        var repository = new CustomerRepositoryStub();
        var useCase = new CreateCustomer(repository, new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(CreateInput(), CancellationToken.None);

        var customer = Assert.Single(repository.Customers);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("Example BV", result.CompanyName);
        Assert.Equal(UtcNow, result.CreatedAt);
    }

    [Fact]
    public async Task UpdateCustomerChangesPersistedDetails()
    {
        var repository = new CustomerRepositoryStub();
        var customer = CreateCustomerEntity();
        repository.Customers.Add(customer);
        var updatedAt = UtcNow.AddDays(1);
        var useCase = new UpdateCustomer(repository, new FixedClock(updatedAt));

        var result = await useCase.ExecuteAsync(
            customer.Id,
            CreateInput() with { CompanyName = "Updated BV" },
            CancellationToken.None);

        Assert.Equal("Updated BV", result.CompanyName);
        Assert.Equal(updatedAt, result.UpdatedAt);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task ArchiveCustomerMarksCustomerAsArchived()
    {
        var repository = new CustomerRepositoryStub();
        var customer = CreateCustomerEntity();
        repository.Customers.Add(customer);
        var useCase = new ArchiveCustomer(repository, new FixedClock(UtcNow.AddDays(1)));

        await useCase.ExecuteAsync(customer.Id, CancellationToken.None);

        Assert.True(customer.IsArchived);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task UpdateCustomerRejectsUnknownIdentifier()
    {
        var useCase = new UpdateCustomer(new CustomerRepositoryStub(), new FixedClock(UtcNow));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), CreateInput(), CancellationToken.None));
    }

    private static CustomerInput CreateInput()
    {
        return new CustomerInput(
            "Example BV",
            "Alex Example",
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            "info@example.com",
            "+31 20 123 4567",
            "NL123",
            null);
    }

    private static Customer CreateCustomerEntity()
    {
        var input = CreateInput();
        return Customer.Create(
            input.CompanyName,
            input.ContactPerson,
            input.Street,
            input.PostalCode,
            input.City,
            input.Country,
            input.Email,
            input.Phone,
            input.VatNumber,
            input.Notes,
            UtcNow);
    }

    private sealed class CustomerRepositoryStub : ICustomerRepository
    {
        public List<Customer> Customers { get; } = [];

        public int UpdateCount { get; private set; }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            Customers.Add(customer);
            return Task.CompletedTask;
        }

        public Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Customers.SingleOrDefault(customer => customer.Id == customerId));
        }

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Customer>> SearchAsync(
            string? searchTerm,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Customer>>(Customers);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IApplicationClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
