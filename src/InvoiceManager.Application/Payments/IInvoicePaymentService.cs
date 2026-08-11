using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Payments;

public interface IInvoicePaymentService
{
    Task<Invoice> RegisterAsync(
        Guid invoiceId,
        PaymentInput input,
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken);

    Task<Invoice> VoidAsync(
        Guid invoiceId,
        Guid paymentId,
        string reason,
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken);
}
