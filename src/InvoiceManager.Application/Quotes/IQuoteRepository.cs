using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Application.Quotes;

public interface IQuoteRepository
{
    Task AddAsync(Quote quote, CancellationToken cancellationToken);

    Task<Quote?> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken);

    Task UpdateAsync(Quote quote, CancellationToken cancellationToken);

    Task<IReadOnlyList<Quote>> SearchAsync(string? searchTerm, CancellationToken cancellationToken);

    Task ExpireEligibleAsync(DateOnly today, DateTimeOffset utcNow, CancellationToken cancellationToken);
}
