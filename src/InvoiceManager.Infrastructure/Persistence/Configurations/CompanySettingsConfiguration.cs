using InvoiceManager.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("CompanySettings");
        builder.HasKey(settings => settings.Id);

        builder.Property(settings => settings.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(settings => settings.Street).HasMaxLength(200).IsRequired();
        builder.Property(settings => settings.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(settings => settings.City).HasMaxLength(100).IsRequired();
        builder.Property(settings => settings.Country).HasMaxLength(100).IsRequired();
        builder.Property(settings => settings.VatNumber).HasMaxLength(50);
        builder.Property(settings => settings.ChamberOfCommerceNumber).HasMaxLength(50);
        builder.Property(settings => settings.Iban).HasMaxLength(50);
        builder.Property(settings => settings.Email).HasMaxLength(254);
        builder.Property(settings => settings.Phone).HasMaxLength(50);
        builder.Property(settings => settings.DefaultVatRate).HasPrecision(5, 4);
        builder.Property(settings => settings.Currency).HasMaxLength(3).IsRequired();
        builder.Property(settings => settings.LogoPath).HasMaxLength(1024);
    }
}
