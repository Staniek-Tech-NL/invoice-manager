using InvoiceManager.Domain.Customers;

namespace InvoiceManager.Application.Customers;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken);

    Task UpdateAsync(Customer customer, CancellationToken cancellationToken);

    Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchTerm,
        bool includeArchived,
        CancellationToken cancellationToken);
}
