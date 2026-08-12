using InvoiceManager.Domain.Common;
using InvoiceManager.Domain.Documents;

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

    public static CompanySettings Create(
        string companyName,
        string street,
        string postalCode,
        string city,
        string country,
        string? vatNumber,
        string? chamberOfCommerceNumber,
        string? iban,
        string? email,
        string? phone,
        decimal defaultVatRate,
        int defaultPaymentTermDays,
        string currency,
        string? logoPath)
    {
        var settings = new CompanySettings { Id = Guid.NewGuid() };
        settings.Update(
            companyName,
            street,
            postalCode,
            city,
            country,
            vatNumber,
            chamberOfCommerceNumber,
            iban,
            email,
            phone,
            defaultVatRate,
            defaultPaymentTermDays,
            currency,
            logoPath);
        return settings;
    }

    public void Update(
        string companyName,
        string street,
        string postalCode,
        string city,
        string country,
        string? vatNumber,
        string? chamberOfCommerceNumber,
        string? iban,
        string? email,
        string? phone,
        decimal defaultVatRate,
        int defaultPaymentTermDays,
        string currency,
        string? logoPath)
    {
        if (defaultVatRate is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultVatRate), "Default VAT rate must be between 0 and 1.");
        }

        if (defaultPaymentTermDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultPaymentTermDays), "Payment term cannot be negative.");
        }

        CompanyName = TextRules.Required(companyName, nameof(companyName), 200);
        Street = TextRules.Required(street, nameof(street), 200);
        PostalCode = TextRules.Required(postalCode, nameof(postalCode), 20);
        City = TextRules.Required(city, nameof(city), 100);
        Country = TextRules.Required(country, nameof(country), 100);
        VatNumber = TextRules.Optional(vatNumber, nameof(vatNumber), 50);
        ChamberOfCommerceNumber = TextRules.Optional(chamberOfCommerceNumber, nameof(chamberOfCommerceNumber), 50);
        Iban = TextRules.Optional(iban, nameof(iban), 50);
        Email = TextRules.Email(email, nameof(email));
        Phone = TextRules.Optional(phone, nameof(phone), 50);
        DefaultVatRate = defaultVatRate;
        DefaultPaymentTermDays = defaultPaymentTermDays;
        Currency = TextRules.Required(currency, nameof(currency), 3).ToUpperInvariant();
        LogoPath = TextRules.Optional(logoPath, nameof(logoPath), 500);
    }

    public IssuerSnapshot CreateSnapshot(byte[]? logoContent = null)
    {
        return IssuerSnapshot.Create(
            CompanyName,
            Street,
            PostalCode,
            City,
            Country,
            VatNumber,
            ChamberOfCommerceNumber,
            Iban,
            Email,
            Phone,
            logoContent);
    }
}
