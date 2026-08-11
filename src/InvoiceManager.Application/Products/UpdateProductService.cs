using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Products;

public sealed class UpdateProductService(
    IProductServiceRepository productServiceRepository,
    IApplicationClock clock)
{
    public async Task<ProductServiceDetails> ExecuteAsync(
        Guid productServiceId,
        ProductServiceInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var productService = await productServiceRepository.GetByIdAsync(productServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product or service {productServiceId} was not found.");

        productService.UpdateDetails(
            input.Name,
            input.Description,
            input.Unit,
            input.UnitPrice,
            input.VatRate,
            clock.UtcNow);

        await productServiceRepository.UpdateAsync(productService, cancellationToken);
        return productService.ToDetails();
    }
}
