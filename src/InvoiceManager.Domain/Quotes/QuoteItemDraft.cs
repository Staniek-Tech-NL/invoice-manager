namespace InvoiceManager.Domain.Quotes;

public sealed record QuoteItemDraft(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate);
