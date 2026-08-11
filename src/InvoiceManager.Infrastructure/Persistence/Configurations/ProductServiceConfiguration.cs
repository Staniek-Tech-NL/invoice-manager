using InvoiceManager.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class ProductServiceConfiguration : IEntityTypeConfiguration<ProductService>
{
    public void Configure(EntityTypeBuilder<ProductService> builder)
    {
        builder.ToTable("ProductServices");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.Unit).HasMaxLength(50).IsRequired();
        builder.Property(product => product.UnitPrice).HasPrecision(18, 2);
        builder.Property(product => product.VatRate).HasPrecision(5, 4);

        builder.HasIndex(product => product.Name);
        builder.HasIndex(product => product.IsActive);
    }
}
