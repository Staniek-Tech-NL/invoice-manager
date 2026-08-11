using InvoiceManager.Domain.Settings;

namespace InvoiceManager.Domain.Tests;

public sealed class CompanySettingsTests
{
    [Fact]
    public void CreateValidatesAndNormalizesSettings()
    {
        var settings = CreateSettings(companyName: "  Example BV  ");

        Assert.Equal("Example BV", settings.CompanyName);
        Assert.Equal("EUR", settings.Currency);
        Assert.Equal("Example BV", settings.CreateSnapshot().CompanyName);
    }

    [Fact]
    public void CreateRejectsVatOutsideSupportedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateSettings(defaultVatRate: 1.01m));
    }

    [Fact]
    public void CreateRejectsNegativePaymentTerm()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateSettings(defaultPaymentTermDays: -1));
    }

    private static CompanySettings CreateSettings(
        string companyName = "Example BV",
        decimal defaultVatRate = 0.21m,
        int defaultPaymentTermDays = 30)
    {
        return CompanySettings.Create(
            companyName,
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            "NL123",
            null,
            null,
            "info@example.com",
            null,
            defaultVatRate,
            defaultPaymentTermDays,
            "eur",
            null);
    }
}
