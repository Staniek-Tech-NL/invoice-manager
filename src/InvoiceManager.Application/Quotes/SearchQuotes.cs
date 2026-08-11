using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Quotes;

public sealed class SearchQuotes(IQuoteRepository repository, IApplicationClock clock)
{
    public async Task<IReadOnlyList<QuoteDetails>> ExecuteAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        await repository.ExpireEligibleAsync(clock.Today, clock.UtcNow, cancellationToken);
        var quotes = await repository.SearchAsync(searchTerm, cancellationToken);
        return quotes.Select(quote => quote.ToDetails()).ToArray();
    }
}
