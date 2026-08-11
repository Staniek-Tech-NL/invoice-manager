using InvoiceManager.Domain.Products;

namespace InvoiceManager.Domain.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public void CreateNormalizesValuesAndActivatesTheService()
    {
        var timestamp = new DateTimeOffset(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

        var service = ProductService.Create("  Consulting  ", "  Advisory work  ", " hour ", 85m, 0.21m, timestamp);

        Assert.NotEqual(Guid.Empty, service.Id);
        Assert.Equal("Consulting", service.Name);
        Assert.Equal("Advisory work", service.Description);
        Assert.Equal("hour", service.Unit);
        Assert.Equal(85m, service.UnitPrice);
        Assert.Equal(0.21m, service.VatRate);
        Assert.True(service.IsActive);
    }

    [Fact]
    public void CreateRejectsNegativePrice()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProductService.Create("Consulting", null, "hour", -1m, 0.21m, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void CreateRejectsVatRateOutsideSupportedRange(double vatRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProductService.Create("Consulting", null, "hour", 85m, (decimal)vatRate, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void UpdateDetailsChangesEditableValues()
    {
        var service = ProductService.Create("Consulting", null, "hour", 85m, 0.21m, DateTimeOffset.UtcNow);
        var updatedAt = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);

        service.UpdateDetails("Development", "Software delivery", "day", 750m, 0.09m, updatedAt);

        Assert.Equal("Development", service.Name);
        Assert.Equal("day", service.Unit);
        Assert.Equal(750m, service.UnitPrice);
        Assert.Equal(0.09m, service.VatRate);
        Assert.Equal(updatedAt, service.UpdatedAt);
    }

    [Fact]
    public void DeactivateIsIdempotent()
    {
        var service = ProductService.Create("Consulting", null, "hour", 85m, 0.21m, DateTimeOffset.UtcNow);
        var deactivatedAt = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);

        service.Deactivate(deactivatedAt);
        service.Deactivate(deactivatedAt.AddDays(1));

        Assert.False(service.IsActive);
        Assert.Equal(deactivatedAt, service.UpdatedAt);
    }
}
