using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Infrastructure.Time;

public sealed class SystemApplicationClock(
    TimeProvider timeProvider,
    TimeZoneInfo localTimeZone) : IApplicationClock
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    public DateOnly Today
    {
        get
        {
            var localTime = TimeZoneInfo.ConvertTime(UtcNow, localTimeZone);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
    }
}
