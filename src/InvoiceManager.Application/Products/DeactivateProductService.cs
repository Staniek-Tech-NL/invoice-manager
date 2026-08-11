using InvoiceManager.Application.Common.Time;

namespace InvoiceManager.Application.Products;

public sealed class DeactivateProductService(
    IProductServiceRepository productServiceRepository,
    IApplicationClock clock)
{
    public async Task ExecuteAsync(Guid productServiceId, CancellationToken cancellationToken = default)
    {
        var productService = await productServiceRepository.GetByIdAsync(productServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product or service {productServiceId} was not found.");

        productService.Deactivate(clock.UtcNow);
        await productServiceRepository.UpdateAsync(productService, cancellationToken);
    }
}
