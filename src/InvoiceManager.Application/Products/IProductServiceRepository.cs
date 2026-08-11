using InvoiceManager.Domain.Products;

namespace InvoiceManager.Application.Products;

public interface IProductServiceRepository
{
    Task AddAsync(ProductService productService, CancellationToken cancellationToken);

    Task<ProductService?> GetByIdAsync(Guid productServiceId, CancellationToken cancellationToken);

    Task UpdateAsync(ProductService productService, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductService>> SearchAsync(
        string? searchTerm,
        bool includeInactive,
        CancellationToken cancellationToken);
}
