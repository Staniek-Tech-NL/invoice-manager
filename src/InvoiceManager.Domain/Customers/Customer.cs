using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string CompanyName { get; private set; } = string.Empty;

    public string? ContactPerson { get; private set; }

    public string Street { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? VatNumber { get; private set; }

    public string? Notes { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Customer Create(
        string companyName,
        string? contactPerson,
        string street,
        string postalCode,
        string city,
        string country,
        string? email,
        string? phone,
        string? vatNumber,
        string? notes,
        DateTimeOffset utcNow)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CreatedAt = utcNow.ToUniversalTime(),
            IsArchived = false,
        };

        customer.UpdateDetails(
            companyName,
            contactPerson,
            street,
            postalCode,
            city,
            country,
            email,
            phone,
            vatNumber,
            notes,
            utcNow);

        return customer;
    }

    public void UpdateDetails(
        string companyName,
        string? contactPerson,
        string street,
        string postalCode,
        string city,
        string country,
        string? email,
        string? phone,
        string? vatNumber,
        string? notes,
        DateTimeOffset utcNow)
    {
        CompanyName = TextRules.Required(companyName, nameof(companyName), 200);
        ContactPerson = TextRules.Optional(contactPerson, nameof(contactPerson), 200);
        Street = TextRules.Required(street, nameof(street), 200);
        PostalCode = TextRules.Required(postalCode, nameof(postalCode), 20);
        City = TextRules.Required(city, nameof(city), 100);
        Country = TextRules.Required(country, nameof(country), 100);
        Email = TextRules.Email(email, nameof(email));
        Phone = TextRules.Optional(phone, nameof(phone), 50);
        VatNumber = TextRules.Optional(vatNumber, nameof(vatNumber), 50);
        Notes = TextRules.Optional(notes, nameof(notes), 2000);
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void Archive(DateTimeOffset utcNow)
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        UpdatedAt = utcNow.ToUniversalTime();
    }
}
