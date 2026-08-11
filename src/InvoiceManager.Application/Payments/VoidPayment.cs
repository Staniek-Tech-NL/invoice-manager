using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Invoices;

namespace InvoiceManager.Application.Payments;

public sealed class VoidPayment(IInvoicePaymentService service, IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        Guid invoiceId,
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var invoice = await service.VoidAsync(
            invoiceId,
            paymentId,
            reason,
            clock.Today,
            clock.UtcNow,
            cancellationToken);
        return invoice.ToDetails();
    }
}
