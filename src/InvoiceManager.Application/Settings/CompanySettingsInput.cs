namespace InvoiceManager.Application.Settings;

public sealed record CompanySettingsInput(
    string CompanyName,
    string Street,
    string PostalCode,
    string City,
    string Country,
    string? VatNumber,
    string? ChamberOfCommerceNumber,
    string? Iban,
    string? Email,
    string? Phone,
    decimal DefaultVatRate,
    int DefaultPaymentTermDays,
    string Currency,
    string? LogoPath);
