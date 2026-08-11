using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);

    Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken);

    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken);

    Task<IReadOnlyList<Invoice>> SearchAsync(string? searchTerm, CancellationToken cancellationToken);

    Task RefreshStatusesAsync(DateOnly today, DateTimeOffset utcNow, CancellationToken cancellationToken);
}
