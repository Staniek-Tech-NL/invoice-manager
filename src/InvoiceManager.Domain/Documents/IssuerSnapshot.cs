using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Documents;

public sealed class IssuerSnapshot
{
    public string CompanyName { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? VatNumber { get; private set; }

    public string? ChamberOfCommerceNumber { get; private set; }

    public string? Iban { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    private IssuerSnapshot()
    {
    }

    public static IssuerSnapshot Create(
        string companyName,
        string street,
        string postalCode,
        string city,
        string country,
        string? vatNumber,
        string? chamberOfCommerceNumber,
        string? iban,
        string? email,
        string? phone)
    {
        return new IssuerSnapshot
        {
            CompanyName = TextRules.Required(companyName, nameof(companyName), 200),
            Street = TextRules.Required(street, nameof(street), 200),
            PostalCode = TextRules.Required(postalCode, nameof(postalCode), 20),
            City = TextRules.Required(city, nameof(city), 100),
            Country = TextRules.Required(country, nameof(country), 100),
            VatNumber = TextRules.Optional(vatNumber, nameof(vatNumber), 50),
            ChamberOfCommerceNumber = TextRules.Optional(chamberOfCommerceNumber, nameof(chamberOfCommerceNumber), 50),
            Iban = TextRules.Optional(iban, nameof(iban), 50),
            Email = TextRules.Email(email, nameof(email)),
            Phone = TextRules.Optional(phone, nameof(phone), 50),
        };
    }

    public IssuerSnapshot Copy()
    {
        return Create(
            CompanyName,
            Street,
            PostalCode,
            City,
            Country,
            VatNumber,
            ChamberOfCommerceNumber,
            Iban,
            Email,
            Phone);
    }
}
