using InvoiceManager.Domain.Products;

namespace InvoiceManager.Application.Products;

internal static class ProductServiceMapping
{
    public static ProductServiceDetails ToDetails(this ProductService productService)
    {
        return new ProductServiceDetails(
            productService.Id,
            productService.Name,
            productService.Description,
            productService.Unit,
            productService.UnitPrice,
            productService.VatRate,
            productService.IsActive,
            productService.CreatedAt,
            productService.UpdatedAt);
    }
}
