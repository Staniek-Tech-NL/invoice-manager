namespace InvoiceManager.Application.Quotes;

public sealed record QuoteItemDetails(
    Guid Id,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);
