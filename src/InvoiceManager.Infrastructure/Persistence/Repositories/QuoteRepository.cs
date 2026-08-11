using System.Data;
using InvoiceManager.Application.Quotes;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Quotes;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class QuoteRepository(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IQuoteRepository
{
    public async Task AddAsync(Quote quote, CancellationToken cancellationToken)
    {
        await DocumentNumberAllocationLock.Instance.WaitAsync(cancellationToken);
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var sequence = await context.DocumentNumberSequences.SingleOrDefaultAsync(
                value => value.DocumentType == DocumentType.Quote && value.Year == quote.IssueDate.Year,
                cancellationToken);

            if (sequence is null)
            {
                sequence = DocumentNumberSequence.Create(DocumentType.Quote, quote.IssueDate.Year);
                context.DocumentNumberSequences.Add(sequence);
            }

            var nextNumber = sequence.AllocateNext();
            quote.AssignNumber($"Q-{quote.IssueDate.Year}-{nextNumber:0000}");
            context.Quotes.Add(quote);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            DocumentNumberAllocationLock.Instance.Release();
        }
    }

    public async Task<Quote?> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Quotes
            .AsNoTracking()
            .Include(quote => quote.Items)
            .SingleOrDefaultAsync(quote => quote.Id == quoteId, cancellationToken);
    }

    public async Task UpdateAsync(Quote quote, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.QuoteItems
            .Where(item => item.QuoteId == quote.Id)
            .ExecuteDeleteAsync(cancellationToken);

        context.Quotes.Update(quote);
        foreach (var item in quote.Items)
        {
            context.Entry(item).State = EntityState.Added;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Quote>> SearchAsync(
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Quotes.AsNoTracking().Include(quote => quote.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";
            query = query.Where(quote =>
                EF.Functions.Like(quote.Number, pattern, "\\") ||
                EF.Functions.Like(quote.Customer.CompanyName, pattern, "\\"));
        }

        return await query
            .OrderByDescending(quote => quote.IssueDate)
            .ThenByDescending(quote => quote.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task ExpireEligibleAsync(
        DateOnly today,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var candidates = await context.Quotes
            .Where(quote =>
                quote.ValidUntil < today &&
                (quote.Status == QuoteStatus.Draft || quote.Status == QuoteStatus.Sent))
            .ToListAsync(cancellationToken);

        foreach (var quote in candidates)
        {
            quote.Expire(today, utcNow);
        }

        if (candidates.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
