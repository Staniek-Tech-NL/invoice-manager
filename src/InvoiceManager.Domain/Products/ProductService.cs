using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Products;

public sealed class ProductService
{
    private ProductService()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public decimal VatRate { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static ProductService Create(
        string name,
        string? description,
        string unit,
        decimal unitPrice,
        decimal vatRate,
        DateTimeOffset utcNow)
    {
        var productService = new ProductService
        {
            Id = Guid.NewGuid(),
            CreatedAt = utcNow.ToUniversalTime(),
            IsActive = true,
        };

        productService.UpdateDetails(name, description, unit, unitPrice, vatRate, utcNow);
        return productService;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string unit,
        decimal unitPrice,
        decimal vatRate,
        DateTimeOffset utcNow)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        if (vatRate is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(vatRate), "VAT rate must be between 0 and 1.");
        }

        Name = TextRules.Required(name, nameof(name), 200);
        Description = TextRules.Optional(description, nameof(description), 2000);
        Unit = TextRules.Required(unit, nameof(unit), 50);
        UnitPrice = unitPrice;
        VatRate = vatRate;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void Deactivate(DateTimeOffset utcNow)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = utcNow.ToUniversalTime();
    }
}
