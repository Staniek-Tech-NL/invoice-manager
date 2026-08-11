using InvoiceManager.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.Quantity).HasPrecision(18, 4);
        builder.Property(item => item.Unit).HasMaxLength(50).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.VatRate).HasPrecision(5, 4);
        builder.Property(item => item.NetAmount).HasPrecision(18, 2);
        builder.Property(item => item.VatAmount).HasPrecision(18, 2);
        builder.Property(item => item.GrossAmount).HasPrecision(18, 2);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
