using System.Data;
using InvoiceManager.Application.Payments;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class InvoicePaymentService(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IInvoicePaymentService
{
    private static readonly SemaphoreSlim PaymentLock = new(1, 1);

    public Task<Invoice> RegisterAsync(
        Guid invoiceId,
        PaymentInput input,
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ExecuteAsync(
            invoiceId,
            invoice => invoice.RegisterPayment(
                input.PaymentDate,
                input.Amount,
                input.Reference,
                input.Method,
                today,
                utcNow),
            cancellationToken);
    }

    public Task<Invoice> VoidAsync(
        Guid invoiceId,
        Guid paymentId,
        string reason,
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            invoiceId,
            invoice =>
            {
                invoice.VoidPayment(paymentId, reason, today, utcNow);
                return null;
            },
            cancellationToken);
    }

    private async Task<Invoice> ExecuteAsync(
        Guid invoiceId,
        Func<Invoice, Payment?> operation,
        CancellationToken cancellationToken)
    {
        await PaymentLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var invoice = await context.Invoices
                .Include(value => value.Items)
                .Include(value => value.Payments)
                .SingleOrDefaultAsync(value => value.Id == invoiceId, cancellationToken)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");
            var newPayment = operation(invoice);
            if (newPayment is not null)
            {
                context.Payments.Add(newPayment);
            }
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return invoice;
        }
        finally
        {
            PaymentLock.Release();
        }
    }
}
