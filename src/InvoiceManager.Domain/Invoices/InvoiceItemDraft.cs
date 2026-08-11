namespace InvoiceManager.Domain.Invoices;

public sealed record InvoiceItemDraft(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate);
