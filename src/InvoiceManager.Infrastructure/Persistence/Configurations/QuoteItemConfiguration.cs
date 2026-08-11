using InvoiceManager.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.ToTable("QuoteItems");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Description).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.Quantity).HasPrecision(18, 4);
        builder.Property(item => item.Unit).HasMaxLength(50).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.VatRate).HasPrecision(5, 4);
        builder.Property(item => item.NetAmount).HasPrecision(18, 2);
        builder.Property(item => item.VatAmount).HasPrecision(18, 2);
        builder.Property(item => item.GrossAmount).HasPrecision(18, 2);

        builder.HasOne<Quote>()
            .WithMany(quote => quote.Items)
            .HasForeignKey(item => item.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
