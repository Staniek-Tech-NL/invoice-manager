namespace InvoiceManager.Application.Common.Time;

public interface IApplicationClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}
