using InvoiceManager.Infrastructure.Time;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class SystemApplicationClockTests
{
    [Fact]
    public void TodayUsesTheConfiguredLocalTimeZone()
    {
        var utcNow = new DateTimeOffset(2026, 8, 11, 22, 30, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(utcNow);
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("TestZone", TimeSpan.FromHours(2), "Test Zone", "Test Zone");
        var clock = new SystemApplicationClock(timeProvider, timeZone);

        Assert.Equal(utcNow, clock.UtcNow);
        Assert.Equal(new DateOnly(2026, 8, 12), clock.Today);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
