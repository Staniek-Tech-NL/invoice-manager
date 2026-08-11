namespace InvoiceManager.Application.Products;

public sealed class SearchProductServices(IProductServiceRepository productServiceRepository)
{
    public async Task<IReadOnlyList<ProductServiceDetails>> ExecuteAsync(
        string? searchTerm,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var productServices = await productServiceRepository.SearchAsync(
            searchTerm,
            includeInactive,
            cancellationToken);

        return productServices.Select(productService => productService.ToDetails()).ToArray();
    }
}
