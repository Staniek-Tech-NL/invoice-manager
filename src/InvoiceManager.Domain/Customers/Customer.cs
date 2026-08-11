namespace InvoiceManager.Domain.Customers;

public sealed class Customer
{
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

    private Customer()
    {
    }
}
