using InvoiceManager.Application.Common.Time;
using InvoiceManager.Domain.Products;

namespace InvoiceManager.Application.Products;

public sealed class CreateProductService(
    IProductServiceRepository productServiceRepository,
    IApplicationClock clock)
{
    public async Task<ProductServiceDetails> ExecuteAsync(
        ProductServiceInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var productService = ProductService.Create(
            input.Name,
            input.Description,
            input.Unit,
            input.UnitPrice,
            input.VatRate,
            clock.UtcNow);

        await productServiceRepository.AddAsync(productService, cancellationToken);
        return productService.ToDetails();
    }
}
