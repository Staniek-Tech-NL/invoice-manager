using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InvoiceManager.Infrastructure.Persistence.Converters;

internal sealed class DateOnlyToStringConverter : ValueConverter<DateOnly, string>
{
    public DateOnlyToStringConverter()
        : base(
            value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture))
    {
    }
}
