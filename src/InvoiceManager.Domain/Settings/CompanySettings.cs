namespace InvoiceManager.Domain.Settings;

public sealed class CompanySettings
{
    public Guid Id { get; private set; }

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

    public decimal DefaultVatRate { get; private set; }

    public int DefaultPaymentTermDays { get; private set; }

    public string Currency { get; private set; } = "EUR";

    public string? LogoPath { get; private set; }

    private CompanySettings()
    {
    }
}
