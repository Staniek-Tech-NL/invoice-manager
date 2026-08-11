using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(quote => quote.Id);

        builder.Property(quote => quote.Number).HasMaxLength(32).IsRequired();
        builder.Property(quote => quote.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(quote => quote.Notes).HasMaxLength(2000);
        builder.Property(quote => quote.Subtotal).HasPrecision(18, 2);
        builder.Property(quote => quote.VatTotal).HasPrecision(18, 2);
        builder.Property(quote => quote.Total).HasPrecision(18, 2);

        builder.HasIndex(quote => quote.Number).IsUnique();
        builder.HasIndex(quote => quote.Status);
        builder.HasIndex(quote => quote.IssueDate);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(quote => quote.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(quote => quote.Issuer, issuer =>
        {
            issuer.Property(value => value.CompanyName).HasColumnName("IssuerCompanyName").HasMaxLength(200).IsRequired();
            issuer.Property(value => value.Street).HasColumnName("IssuerStreet").HasMaxLength(200).IsRequired();
            issuer.Property(value => value.PostalCode).HasColumnName("IssuerPostalCode").HasMaxLength(20).IsRequired();
            issuer.Property(value => value.City).HasColumnName("IssuerCity").HasMaxLength(100).IsRequired();
            issuer.Property(value => value.Country).HasColumnName("IssuerCountry").HasMaxLength(100).IsRequired();
            issuer.Property(value => value.VatNumber).HasColumnName("IssuerVatNumber").HasMaxLength(50);
            issuer.Property(value => value.ChamberOfCommerceNumber).HasColumnName("IssuerChamberOfCommerceNumber").HasMaxLength(50);
            issuer.Property(value => value.Iban).HasColumnName("IssuerIban").HasMaxLength(50);
            issuer.Property(value => value.Email).HasColumnName("IssuerEmail").HasMaxLength(254);
            issuer.Property(value => value.Phone).HasColumnName("IssuerPhone").HasMaxLength(50);
        });

        builder.OwnsOne(quote => quote.Customer, customer =>
        {
            customer.Property(value => value.CompanyName).HasColumnName("CustomerCompanyName").HasMaxLength(200).IsRequired();
            customer.Property(value => value.ContactPerson).HasColumnName("CustomerContactPerson").HasMaxLength(200);
            customer.Property(value => value.Street).HasColumnName("CustomerStreet").HasMaxLength(200).IsRequired();
            customer.Property(value => value.PostalCode).HasColumnName("CustomerPostalCode").HasMaxLength(20).IsRequired();
            customer.Property(value => value.City).HasColumnName("CustomerCity").HasMaxLength(100).IsRequired();
            customer.Property(value => value.Country).HasColumnName("CustomerCountry").HasMaxLength(100).IsRequired();
            customer.Property(value => value.Email).HasColumnName("CustomerEmail").HasMaxLength(254);
            customer.Property(value => value.Phone).HasColumnName("CustomerPhone").HasMaxLength(50);
            customer.Property(value => value.VatNumber).HasColumnName("CustomerVatNumber").HasMaxLength(50);
        });

        builder.Navigation(quote => quote.Issuer).IsRequired();
        builder.Navigation(quote => quote.Customer).IsRequired();
    }
}
