using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Invoices;

public sealed class SearchInvoices(IInvoiceRepository repository, IApplicationClock clock)
{
    public async Task<IReadOnlyList<InvoiceDetails>> ExecuteAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        await repository.RefreshStatusesAsync(clock.Today, clock.UtcNow, cancellationToken);
        var invoices = await repository.SearchAsync(searchTerm, cancellationToken);
        return invoices.Select(invoice => invoice.ToDetails()).ToArray();
    }
}
