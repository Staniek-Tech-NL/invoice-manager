namespace InvoiceManager.Application.Dashboard;

public interface IDashboardQuery
{
    Task<DashboardSummary> GetAsync(DateOnly today, CancellationToken cancellationToken);
}
