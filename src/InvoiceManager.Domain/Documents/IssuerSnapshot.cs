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
}
