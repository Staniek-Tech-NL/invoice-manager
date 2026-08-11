namespace InvoiceManager.Application.Invoices;

public sealed record InvoiceItemInput(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate);
