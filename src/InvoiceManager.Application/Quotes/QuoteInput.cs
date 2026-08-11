namespace InvoiceManager.Application.Quotes;

public sealed record QuoteInput(
    Guid CustomerId,
    DateOnly IssueDate,
    DateOnly ValidUntil,
    string? Notes,
    IReadOnlyCollection<QuoteItemInput> Items);
