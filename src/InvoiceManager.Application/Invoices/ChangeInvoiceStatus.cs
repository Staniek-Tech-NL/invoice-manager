using InvoiceManager.Application.Common.Time;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

public sealed class ChangeInvoiceStatus(IInvoiceRepository repository, IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        Guid invoiceId,
        InvoiceStatus targetStatus,
        CancellationToken cancellationToken = default)
    {
        var invoice = await repository.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");

        switch (targetStatus)
        {
            case InvoiceStatus.Sent:
                invoice.MarkSent(clock.UtcNow);
                break;
            case InvoiceStatus.Cancelled:
                invoice.Cancel(clock.UtcNow);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), "Unsupported manual invoice status.");
        }

        await repository.UpdateAsync(invoice, cancellationToken);
        return invoice.ToDetails();
    }
}
