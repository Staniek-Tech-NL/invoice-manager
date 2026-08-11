using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Settings;

public interface ICompanySettingsRepository
{
    Task<CompanySettings?> GetAsync(CancellationToken cancellationToken);

    Task SaveAsync(CompanySettings settings, CancellationToken cancellationToken);
}
