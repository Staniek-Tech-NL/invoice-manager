using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Invoices;

namespace InvoiceManager.Application.Payments;

public sealed class RegisterPayment(IInvoicePaymentService service, IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        Guid invoiceId,
        PaymentInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var invoice = await service.RegisterAsync(
            invoiceId,
            input,
            clock.Today,
            clock.UtcNow,
            cancellationToken);
        return invoice.ToDetails();
    }
}
