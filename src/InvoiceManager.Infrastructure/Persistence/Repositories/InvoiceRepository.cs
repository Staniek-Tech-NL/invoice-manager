using System.Data;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class InvoiceRepository(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IInvoiceRepository
{
    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        await DocumentNumberAllocationLock.Instance.WaitAsync(cancellationToken);
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            await AssignNextNumberAsync(context, invoice, cancellationToken);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            DocumentNumberAllocationLock.Instance.Release();
        }
    }

    public async Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .Include(invoice => invoice.Payments)
            .SingleOrDefaultAsync(invoice => invoice.Id == invoiceId, cancellationToken);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.InvoiceItems
            .Where(item => item.InvoiceId == invoice.Id)
            .ExecuteDeleteAsync(cancellationToken);
        context.Invoices.Update(invoice);
        foreach (var item in invoice.Items)
        {
            context.Entry(item).State = EntityState.Added;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> SearchAsync(
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .Include(invoice => invoice.Payments)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";
            query = query.Where(invoice =>
                EF.Functions.Like(invoice.Number, pattern, "\\") ||
                EF.Functions.Like(invoice.Customer.CompanyName, pattern, "\\"));
        }

        return await query
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task RefreshStatusesAsync(
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var invoices = await context.Invoices
            .Include(invoice => invoice.Payments)
            .Where(invoice => invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);
        var changed = false;
        foreach (var invoice in invoices)
        {
            changed |= invoice.RefreshStatus(today, utcNow);
        }

        if (changed)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    internal static async Task AssignNextNumberAsync(
        InvoiceManagerDbContext context,
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        var sequence = await context.DocumentNumberSequences.SingleOrDefaultAsync(
            value => value.DocumentType == DocumentType.Invoice && value.Year == invoice.IssueDate.Year,
            cancellationToken);
        if (sequence is null)
        {
            sequence = DocumentNumberSequence.Create(DocumentType.Invoice, invoice.IssueDate.Year);
            context.DocumentNumberSequences.Add(sequence);
        }

        var nextNumber = sequence.AllocateNext();
        invoice.AssignNumber($"INV-{invoice.IssueDate.Year}-{nextNumber:0000}");
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
