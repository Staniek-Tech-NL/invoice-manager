namespace InvoiceManager.Application.Customers;

public sealed class SearchCustomers(ICustomerRepository customerRepository)
{
    public async Task<IReadOnlyList<CustomerDetails>> ExecuteAsync(
        string? searchTerm,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.SearchAsync(
            searchTerm,
            includeArchived,
            cancellationToken);

        return customers.Select(customer => customer.ToDetails()).ToArray();
    }
}
