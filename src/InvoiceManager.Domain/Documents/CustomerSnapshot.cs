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
}
