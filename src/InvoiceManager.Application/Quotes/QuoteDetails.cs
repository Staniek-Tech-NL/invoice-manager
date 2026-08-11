using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Application.Quotes;

public sealed record QuoteDetails(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerCompanyName,
    DateOnly IssueDate,
    DateOnly ValidUntil,
    QuoteStatus Status,
    string? Notes,
    decimal Subtotal,
    decimal VatTotal,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<QuoteItemDetails> Items);
