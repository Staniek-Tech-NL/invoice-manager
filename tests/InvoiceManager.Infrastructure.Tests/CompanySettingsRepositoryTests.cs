using InvoiceManager.Domain.Settings;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class CompanySettingsRepositoryTests
{
    [Fact]
    public async Task SaveCreatesAndUpdatesSingleCompanyProfile()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>().UseSqlite(connection).Options;
        var factory = new TestDbContextFactory(options);
        await using (var context = factory.CreateDbContext())
        {
            await context.Database.MigrateAsync(CancellationToken.None);
        }

        var repository = new CompanySettingsRepository(factory);
        var settings = CreateSettings();
        await repository.SaveAsync(settings, CancellationToken.None);
        settings.Update(
            "Updated BV",
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
        await repository.SaveAsync(settings, CancellationToken.None);

        var loaded = await repository.GetAsync(CancellationToken.None);
        Assert.Equal(settings.Id, loaded?.Id);
        Assert.Equal("Updated BV", loaded?.CompanyName);
    }

    private static CompanySettings CreateSettings()
    {
        return CompanySettings.Create(
            "Issuer BV",
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            null,
            null,
            null,
            "issuer@example.com",
            null,
            0.21m,
            30,
            "EUR",
            null);
    }

    private sealed class TestDbContextFactory(
        DbContextOptions<InvoiceManagerDbContext> options) : IDbContextFactory<InvoiceManagerDbContext>
    {
        public InvoiceManagerDbContext CreateDbContext() => new(options);
    }
}
