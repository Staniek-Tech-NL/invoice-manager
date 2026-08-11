using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

public sealed record InvoiceDetails(
    Guid Id,
    string Number,
    Guid CustomerId,
    Guid? SourceQuoteId,
    string CustomerCompanyName,
    DateOnly IssueDate,
    DateOnly DueDate,
    InvoiceStatus Status,
    string? Notes,
    decimal Subtotal,
    decimal VatTotal,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<InvoiceItemDetails> Items);
