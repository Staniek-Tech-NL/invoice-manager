using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Application.Tests;

public sealed class CompanySettingsUseCaseTests
{
    [Fact]
    public async Task SaveCreatesAndThenUpdatesSingleSettingsProfile()
    {
        var repository = new SettingsRepositoryStub();
        var save = new SaveCompanySettings(repository);

        var created = await save.ExecuteAsync(CreateInput(), CancellationToken.None);
        var updated = await save.ExecuteAsync(CreateInput() with { CompanyName = "Updated BV" }, CancellationToken.None);

        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Updated BV", updated.CompanyName);
        Assert.Equal(2, repository.SaveCount);
    }

    private static CompanySettingsInput CreateInput()
    {
        return new CompanySettingsInput(
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

    private sealed class SettingsRepositoryStub : ICompanySettingsRepository
    {
        private CompanySettings? _settings;

        public int SaveCount { get; private set; }

        public Task<CompanySettings?> GetAsync(CancellationToken cancellationToken) => Task.FromResult(_settings);

        public Task SaveAsync(CompanySettings settings, CancellationToken cancellationToken)
        {
            _settings = settings;
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
