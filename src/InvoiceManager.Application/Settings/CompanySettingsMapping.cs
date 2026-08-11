using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Settings;

internal static class CompanySettingsMapping
{
    public static CompanySettingsDetails ToDetails(this CompanySettings settings)
    {
        return new CompanySettingsDetails(
            settings.Id,
            settings.CompanyName,
            settings.Street,
            settings.PostalCode,
            settings.City,
            settings.Country,
            settings.VatNumber,
            settings.ChamberOfCommerceNumber,
            settings.Iban,
            settings.Email,
            settings.Phone,
            settings.DefaultVatRate,
            settings.DefaultPaymentTermDays,
            settings.Currency,
            settings.LogoPath);
    }
}
