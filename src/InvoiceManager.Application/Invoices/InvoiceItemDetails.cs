namespace InvoiceManager.Application.Invoices;

public sealed record InvoiceItemDetails(
    Guid Id,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);
