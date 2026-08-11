using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Documents;

public sealed class CustomerSnapshot
{
    public string CompanyName { get; private set; } = string.Empty;

    public string? ContactPerson { get; private set; }

    public string Street { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? VatNumber { get; private set; }

    private CustomerSnapshot()
    {
    }

    public static CustomerSnapshot Create(
        string companyName,
        string? contactPerson,
        string street,
        string postalCode,
        string city,
        string country,
        string? email,
        string? phone,
        string? vatNumber)
    {
        return new CustomerSnapshot
        {
            CompanyName = TextRules.Required(companyName, nameof(companyName), 200),
            ContactPerson = TextRules.Optional(contactPerson, nameof(contactPerson), 200),
            Street = TextRules.Required(street, nameof(street), 200),
            PostalCode = TextRules.Required(postalCode, nameof(postalCode), 20),
            City = TextRules.Required(city, nameof(city), 100),
            Country = TextRules.Required(country, nameof(country), 100),
            Email = TextRules.Email(email, nameof(email)),
            Phone = TextRules.Optional(phone, nameof(phone), 50),
            VatNumber = TextRules.Optional(vatNumber, nameof(vatNumber), 50),
        };
    }

    public CustomerSnapshot Copy()
    {
        return Create(
            CompanyName,
            ContactPerson,
            Street,
            PostalCode,
            City,
            Country,
            Email,
            Phone,
            VatNumber);
    }
}
