using InvoiceManager.Domain.Customers;

namespace InvoiceManager.Application.Customers;

internal static class CustomerMapping
{
    public static CustomerDetails ToDetails(this Customer customer)
    {
        return new CustomerDetails(
            customer.Id,
            customer.CompanyName,
            customer.ContactPerson,
            customer.Street,
            customer.PostalCode,
            customer.City,
            customer.Country,
            customer.Email,
            customer.Phone,
            customer.VatNumber,
            customer.Notes,
            customer.IsArchived,
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}
