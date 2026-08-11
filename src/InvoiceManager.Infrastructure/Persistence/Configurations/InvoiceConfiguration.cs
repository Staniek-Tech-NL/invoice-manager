using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.Number).HasMaxLength(32).IsRequired();
        builder.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(invoice => invoice.Notes).HasMaxLength(2000);
        builder.Property(invoice => invoice.Subtotal).HasPrecision(18, 2);
        builder.Property(invoice => invoice.VatTotal).HasPrecision(18, 2);
        builder.Property(invoice => invoice.Total).HasPrecision(18, 2);

        builder.HasIndex(invoice => invoice.Number).IsUnique();
        builder.HasIndex(invoice => invoice.Status);
        builder.HasIndex(invoice => invoice.IssueDate);
        builder.HasIndex(invoice => invoice.DueDate);
        builder.HasIndex(invoice => invoice.SourceQuoteId).IsUnique();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(invoice => invoice.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Quote>()
            .WithOne()
            .HasForeignKey<Invoice>(invoice => invoice.SourceQuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(invoice => invoice.Issuer, issuer =>
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

        builder.OwnsOne(invoice => invoice.Customer, customer =>
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

        builder.Navigation(invoice => invoice.Issuer).IsRequired();
        builder.Navigation(invoice => invoice.Customer).IsRequired();
    }
}
