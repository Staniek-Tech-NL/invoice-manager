using InvoiceManager.Application.Common.Time;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Application.Quotes;

public sealed class ChangeQuoteStatus(IQuoteRepository repository, IApplicationClock clock)
{
    public async Task<QuoteDetails> ExecuteAsync(
        Guid quoteId,
        QuoteStatus targetStatus,
        CancellationToken cancellationToken = default)
    {
        var quote = await repository.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quote {quoteId} was not found.");

        switch (targetStatus)
        {
            case QuoteStatus.Sent:
                quote.MarkSent(clock.UtcNow);
                break;
            case QuoteStatus.Accepted:
                quote.Accept(clock.UtcNow);
                break;
            case QuoteStatus.Rejected:
                quote.Reject(clock.UtcNow);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetStatus), "Unsupported manual quote status.");
        }

        await repository.UpdateAsync(quote, cancellationToken);
        return quote.ToDetails();
    }
}
