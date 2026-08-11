using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

public interface IQuoteToInvoiceConverter
{
    Task<Invoice> ConvertAsync(
        Guid quoteId,
        DateOnly issueDate,
        DateOnly dueDate,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken);
}
