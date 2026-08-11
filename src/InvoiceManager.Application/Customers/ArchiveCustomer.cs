using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Customers;

public sealed class ArchiveCustomer(
    ICustomerRepository customerRepository,
    IApplicationClock clock)
{
    public async Task ExecuteAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {customerId} was not found.");

        customer.Archive(clock.UtcNow);
        await customerRepository.UpdateAsync(customer, cancellationToken);
    }
}
