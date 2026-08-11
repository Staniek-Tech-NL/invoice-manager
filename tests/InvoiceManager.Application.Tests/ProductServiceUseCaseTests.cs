using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Products;
using InvoiceManager.Domain.Products;

namespace InvoiceManager.Application.Tests;

public sealed class ProductServiceUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateProductServicePersistsValidatedService()
    {
        var repository = new ProductServiceRepositoryStub();
        var useCase = new CreateProductService(repository, new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(CreateInput(), CancellationToken.None);

        var service = Assert.Single(repository.ProductServices);
        Assert.Equal(service.Id, result.Id);
        Assert.Equal(85m, result.UnitPrice);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateProductServiceChangesPersistedDetails()
    {
        var repository = new ProductServiceRepositoryStub();
        var service = CreateEntity();
        repository.ProductServices.Add(service);
        var useCase = new UpdateProductService(repository, new FixedClock(UtcNow.AddDays(1)));

        var result = await useCase.ExecuteAsync(
            service.Id,
            CreateInput() with { Name = "Development", UnitPrice = 100m },
            CancellationToken.None);

        Assert.Equal("Development", result.Name);
        Assert.Equal(100m, result.UnitPrice);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task DeactivateProductServiceMarksServiceAsInactive()
    {
        var repository = new ProductServiceRepositoryStub();
        var service = CreateEntity();
        repository.ProductServices.Add(service);
        var useCase = new DeactivateProductService(repository, new FixedClock(UtcNow.AddDays(1)));

        await useCase.ExecuteAsync(service.Id, CancellationToken.None);

        Assert.False(service.IsActive);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task DeactivateProductServiceRejectsUnknownIdentifier()
    {
        var useCase = new DeactivateProductService(new ProductServiceRepositoryStub(), new FixedClock(UtcNow));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private static ProductServiceInput CreateInput()
    {
        return new ProductServiceInput("Consulting", "Advisory work", "hour", 85m, 0.21m);
    }

    private static ProductService CreateEntity()
    {
        var input = CreateInput();
        return ProductService.Create(
            input.Name,
            input.Description,
            input.Unit,
            input.UnitPrice,
            input.VatRate,
            UtcNow);
    }

    private sealed class ProductServiceRepositoryStub : IProductServiceRepository
    {
        public List<ProductService> ProductServices { get; } = [];

        public int UpdateCount { get; private set; }

        public Task AddAsync(ProductService productService, CancellationToken cancellationToken)
        {
            ProductServices.Add(productService);
            return Task.CompletedTask;
        }

        public Task<ProductService?> GetByIdAsync(Guid productServiceId, CancellationToken cancellationToken)
        {
            return Task.FromResult(ProductServices.SingleOrDefault(service => service.Id == productServiceId));
        }

        public Task UpdateAsync(ProductService productService, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ProductService>> SearchAsync(
            string? searchTerm,
            bool includeInactive,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ProductService>>(ProductServices);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IApplicationClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
