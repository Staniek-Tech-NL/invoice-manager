using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InvoiceManager.Infrastructure.Persistence.Converters;

internal sealed class UtcDateTimeOffsetToStringConverter : ValueConverter<DateTimeOffset, string>
{
    public UtcDateTimeOffsetToStringConverter()
        : base(
            value => value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            value => ParseUtc(value))
    {
    }

    private static DateTimeOffset ParseUtc(string value)
    {
        return DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind).ToUniversalTime();
    }
}
