namespace InvoiceManager.Application.Quotes;

public sealed record QuoteItemInput(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate);
