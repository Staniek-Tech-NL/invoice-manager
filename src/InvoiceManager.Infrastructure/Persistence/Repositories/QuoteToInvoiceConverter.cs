using System.Data;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class QuoteToInvoiceConverter(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IQuoteToInvoiceConverter
{
    public async Task<Invoice> ConvertAsync(
        Guid quoteId,
        DateOnly issueDate,
        DateOnly dueDate,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        await DocumentNumberAllocationLock.Instance.WaitAsync(cancellationToken);
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var quote = await context.Quotes
                .Include(value => value.Items)
                .SingleOrDefaultAsync(value => value.Id == quoteId, cancellationToken)
                ?? throw new KeyNotFoundException($"Quote {quoteId} was not found.");
            var alreadyConverted = await context.Invoices.AnyAsync(
                invoice => invoice.SourceQuoteId == quoteId,
                cancellationToken);
            if (alreadyConverted)
            {
                throw new InvalidOperationException("This quote has already been converted to an invoice.");
            }

            var invoice = Invoice.CreateFromQuote(quote, issueDate, dueDate, utcNow);
            await InvoiceRepository.AssignNextNumberAsync(context, invoice, cancellationToken);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return invoice;
        }
        finally
        {
            DocumentNumberAllocationLock.Instance.Release();
        }
    }
}
