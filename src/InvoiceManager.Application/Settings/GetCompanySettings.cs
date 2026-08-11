namespace InvoiceManager.Application.Settings;

public sealed class GetCompanySettings(ICompanySettingsRepository repository)
{
    public async Task<CompanySettingsDetails?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var settings = await repository.GetAsync(cancellationToken);
        return settings?.ToDetails();
    }
}
