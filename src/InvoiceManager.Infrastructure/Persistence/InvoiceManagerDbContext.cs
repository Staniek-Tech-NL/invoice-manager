using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Payments;
using InvoiceManager.Domain.Products;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Domain.Settings;
using InvoiceManager.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence;

public sealed class InvoiceManagerDbContext(
    DbContextOptions<InvoiceManagerDbContext> options) : DbContext(options)
{
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<ProductService> ProductServices => Set<ProductService>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<DocumentNumberSequence> DocumentNumberSequences => Set<DocumentNumberSequence>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyToStringConverter>();
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetToStringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceManagerDbContext).Assembly);
    }
}
