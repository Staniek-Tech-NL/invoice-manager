namespace InvoiceManager.Application.Products;

public sealed record ProductServiceInput(
    string Name,
    string? Description,
    string Unit,
    decimal UnitPrice,
    decimal VatRate);
