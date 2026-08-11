using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Settings;

public sealed class SaveCompanySettings(ICompanySettingsRepository repository)
{
    public async Task<CompanySettingsDetails> ExecuteAsync(
        CompanySettingsInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var settings = await repository.GetAsync(cancellationToken);

        if (settings is null)
        {
            settings = CompanySettings.Create(
                input.CompanyName,
                input.Street,
                input.PostalCode,
                input.City,
                input.Country,
                input.VatNumber,
                input.ChamberOfCommerceNumber,
                input.Iban,
                input.Email,
                input.Phone,
                input.DefaultVatRate,
                input.DefaultPaymentTermDays,
                input.Currency,
                input.LogoPath);
        }
        else
        {
            settings.Update(
                input.CompanyName,
                input.Street,
                input.PostalCode,
                input.City,
                input.Country,
                input.VatNumber,
                input.ChamberOfCommerceNumber,
                input.Iban,
                input.Email,
                input.Phone,
                input.DefaultVatRate,
                input.DefaultPaymentTermDays,
                input.Currency,
                input.LogoPath);
        }

        await repository.SaveAsync(settings, cancellationToken);
        return settings.ToDetails();
    }
}
