using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class CompanySettingsRepository(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : ICompanySettingsRepository
{
    public async Task<CompanySettings?> GetAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.CompanySettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(CompanySettings settings, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var exists = await context.CompanySettings.AnyAsync(value => value.Id == settings.Id, cancellationToken);

        if (exists)
        {
            context.CompanySettings.Update(settings);
        }
        else
        {
            context.CompanySettings.Add(settings);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
