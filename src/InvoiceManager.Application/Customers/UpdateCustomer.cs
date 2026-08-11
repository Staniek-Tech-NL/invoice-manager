using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Customers;

public sealed class UpdateCustomer(
    ICustomerRepository customerRepository,
    IApplicationClock clock)
{
    public async Task<CustomerDetails> ExecuteAsync(
        Guid customerId,
        CustomerInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {customerId} was not found.");

        customer.UpdateDetails(
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
            clock.UtcNow);

        await customerRepository.UpdateAsync(customer, cancellationToken);
        return customer.ToDetails();
    }
}
