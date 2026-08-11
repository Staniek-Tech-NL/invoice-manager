using InvoiceManager.Domain.Products;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class ProductServiceRepositoryTests
{
    [Fact]
    public async Task SearchExcludesInactiveServicesByDefaultAndSearchesByName()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken.None);
        var factory = await CreateFactoryAsync(connection);
        var repository = new ProductServiceRepository(factory);
        var timestamp = new DateTimeOffset(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);
        var activeService = ProductService.Create("Software Development", null, "hour", 90m, 0.21m, timestamp);
        var inactiveService = ProductService.Create("Legacy Consulting", null, "hour", 80m, 0.21m, timestamp);

        await repository.AddAsync(activeService, CancellationToken.None);
        await repository.AddAsync(inactiveService, CancellationToken.None);
        inactiveService.Deactivate(timestamp.AddDays(1));
        await repository.UpdateAsync(inactiveService, CancellationToken.None);

        var activeResults = await repository.SearchAsync(null, includeInactive: false, CancellationToken.None);
        var searchResults = await repository.SearchAsync("software", includeInactive: true, CancellationToken.None);
        var allResults = await repository.SearchAsync(null, includeInactive: true, CancellationToken.None);

        Assert.Equal(activeService.Id, Assert.Single(activeResults).Id);
        Assert.Equal(activeService.Id, Assert.Single(searchResults).Id);
        Assert.Equal(2, allResults.Count);
    }

    private static async Task<IDbContextFactory<InvoiceManagerDbContext>> CreateFactoryAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>()
            .UseSqlite(connection)
            .Options;
        var factory = new TestDbContextFactory(options);

        await using var context = factory.CreateDbContext();
        await context.Database.MigrateAsync(CancellationToken.None);
        return factory;
    }

    private sealed class TestDbContextFactory(
        DbContextOptions<InvoiceManagerDbContext> options) : IDbContextFactory<InvoiceManagerDbContext>
    {
        public InvoiceManagerDbContext CreateDbContext()
        {
            return new InvoiceManagerDbContext(options);
        }
    }
}
