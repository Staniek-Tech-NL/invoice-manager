namespace InvoiceManager.Application.Invoices;

public sealed record InvoiceInput(
    Guid CustomerId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string? Notes,
    IReadOnlyCollection<InvoiceItemInput> Items);
