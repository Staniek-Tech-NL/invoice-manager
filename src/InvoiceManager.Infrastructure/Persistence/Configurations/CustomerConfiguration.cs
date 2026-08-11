using InvoiceManager.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.ContactPerson).HasMaxLength(200);
        builder.Property(customer => customer.Street).HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.City).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Country).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(254);
        builder.Property(customer => customer.Phone).HasMaxLength(50);
        builder.Property(customer => customer.VatNumber).HasMaxLength(50);
        builder.Property(customer => customer.Notes).HasMaxLength(2000);

        builder.HasIndex(customer => customer.CompanyName);
        builder.HasIndex(customer => customer.IsArchived);
    }
}
