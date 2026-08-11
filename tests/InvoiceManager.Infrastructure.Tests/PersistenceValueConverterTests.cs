using InvoiceManager.Infrastructure.Persistence.Converters;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class PersistenceValueConverterTests
{
    [Fact]
    public void DateOnlyConverterUsesIsoDateFormat()
    {
        var converter = new DateOnlyToStringConverter();
        var date = new DateOnly(2026, 8, 11);

        var storedValue = Assert.IsType<string>(converter.ConvertToProvider(date));
        var restoredValue = Assert.IsType<DateOnly>(converter.ConvertFromProvider(storedValue));

        Assert.Equal("2026-08-11", storedValue);
        Assert.Equal(date, restoredValue);
    }

    [Fact]
    public void AuditTimestampConverterNormalizesValuesToUtc()
    {
        var converter = new UtcDateTimeOffsetToStringConverter();
        var timestamp = new DateTimeOffset(2026, 8, 11, 14, 30, 0, TimeSpan.FromHours(2));

        var storedValue = Assert.IsType<string>(converter.ConvertToProvider(timestamp));
        var restoredValue = Assert.IsType<DateTimeOffset>(converter.ConvertFromProvider(storedValue));

        Assert.EndsWith("Z", storedValue, StringComparison.Ordinal);
        Assert.Equal(TimeSpan.Zero, restoredValue.Offset);
        Assert.Equal(timestamp.ToUniversalTime(), restoredValue);
    }
}
