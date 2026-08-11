namespace InvoiceManager.Application.Products;

public sealed record ProductServiceDetails(
    Guid Id,
    string Name,
    string? Description,
    string Unit,
    decimal UnitPrice,
    decimal VatRate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
