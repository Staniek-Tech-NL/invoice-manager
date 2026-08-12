using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Invoices;

namespace InvoiceManager.Application.Dashboard;

public sealed class GetDashboardSummary(
    IDashboardQuery query,
    IInvoiceRepository invoiceRepository,
    IApplicationClock clock)
{
    public async Task<DashboardSummary> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await invoiceRepository.RefreshStatusesAsync(clock.Today, clock.UtcNow, cancellationToken);
        return await query.GetAsync(clock.Today, cancellationToken);
    }
}
