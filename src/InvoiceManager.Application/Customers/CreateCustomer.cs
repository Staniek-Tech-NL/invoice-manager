using InvoiceManager.Application.Common.Time;
using InvoiceManager.Domain.Customers;

namespace InvoiceManager.Application.Customers;

public sealed class CreateCustomer(
    ICustomerRepository customerRepository,
    IApplicationClock clock)
{
    public async Task<CustomerDetails> ExecuteAsync(
        CustomerInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var customer = Customer.Create(
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

        await customerRepository.AddAsync(customer, cancellationToken);
        return customer.ToDetails();
    }
}
