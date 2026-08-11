using InvoiceManager.Application.Products;
using InvoiceManager.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class ProductServiceRepository(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IProductServiceRepository
{
    public async Task AddAsync(ProductService productService, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.ProductServices.Add(productService);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductService?> GetByIdAsync(Guid productServiceId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.ProductServices
            .AsNoTracking()
            .SingleOrDefaultAsync(productService => productService.Id == productServiceId, cancellationToken);
    }

    public async Task UpdateAsync(ProductService productService, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.ProductServices.Update(productService);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductService>> SearchAsync(
        string? searchTerm,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.ProductServices.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(productService => productService.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

            query = query.Where(productService =>
                EF.Functions.Like(productService.Name, pattern, "\\") ||
                EF.Functions.Like(productService.Description ?? string.Empty, pattern, "\\") ||
                EF.Functions.Like(productService.Unit, pattern, "\\"));
        }

        return await query
            .OrderByDescending(productService => productService.IsActive)
            .ThenBy(productService => productService.Name)
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
