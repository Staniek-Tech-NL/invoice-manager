using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.Reference).HasMaxLength(200);
        builder.Property(payment => payment.Method).HasMaxLength(100);
        builder.Property(payment => payment.VoidReason).HasMaxLength(500);

        builder.HasIndex(payment => payment.InvoiceId);
        builder.HasIndex(payment => payment.PaymentDate);

        builder.HasOne<Invoice>()
            .WithMany(invoice => invoice.Payments)
            .HasForeignKey(payment => payment.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
