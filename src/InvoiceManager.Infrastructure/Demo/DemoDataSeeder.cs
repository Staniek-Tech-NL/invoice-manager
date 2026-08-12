using System.Data;
using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Demo;
using InvoiceManager.Domain.Customers;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Products;
using InvoiceManager.Domain.Quotes;
using InvoiceManager.Domain.Settings;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Demo;

public sealed class DemoDataSeeder(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory,
    IApplicationClock clock) : IDemoDataSeeder
{
    public async Task<DemoSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (await HasBusinessDataAsync(context, cancellationToken))
        {
            throw new InvalidOperationException("Demo data can only be loaded into an empty database.");
        }

        var now = clock.UtcNow;
        var today = clock.Today;
        var settings = CompanySettings.Create(
            "North Sea Digital BV", "Westblaak 100", "3012 KM", "Rotterdam", "Netherlands",
            "NL123456789B01", "12345678", "NL91ABNA0417164300", "office@northsea.example",
            "+31 10 555 0100", 0.21m, 30, "EUR", null);
        var issuer = settings.CreateSnapshot();

        var customers = new[]
        {
            Customer.Create("Acme Logistics BV", "Anna de Vries", "Keizersgracht 200", "1016 DW", "Amsterdam", "Netherlands", "anna@acme.example", "+31 20 555 0200", "NL987654321B01", "Fast-growing logistics platform.", now),
            Customer.Create("Tulip Studio", "Mila Jansen", "Oudegracht 75", "3511 AD", "Utrecht", "Netherlands", "mila@tulip.example", "+31 30 555 0300", "NL111222333B01", "Brand and product design studio.", now),
            Customer.Create("Delta Advisory", "Noah Smit", "Coolsingel 50", "3011 AD", "Rotterdam", "Netherlands", "noah@delta.example", "+31 10 555 0400", "NL444555666B01", "Technology advisory practice.", now),
        };
        var services = new[]
        {
            ProductService.Create("Software development", "Application engineering and implementation.", "hour", 95m, 0.21m, now),
            ProductService.Create("Architecture workshop", "Collaborative technical design session.", "session", 650m, 0.21m, now),
            ProductService.Create("UX review", "Usability and accessibility review.", "hour", 85m, 0.21m, now),
            ProductService.Create("Support retainer", "Monthly maintenance and support.", "month", 850m, 0.21m, now),
        };

        var acceptedQuote = CreateQuote(customers[0], issuer, today.AddDays(-105), today.AddDays(-90),
            [new QuoteItemDraft("Application architecture and implementation", 24m, "hour", 95m, 0.21m), new QuoteItemDraft("Deployment workshop", 1m, "session", 650m, 0.21m)], now);
        acceptedQuote.MarkSent(now.AddMinutes(1));
        acceptedQuote.Accept(now.AddMinutes(2));
        var sentQuote = CreateQuote(customers[1], issuer, today.AddDays(-8), today.AddDays(14),
            [new QuoteItemDraft("UX review", 12m, "hour", 85m, 0.21m)], now);
        sentQuote.MarkSent(now.AddMinutes(3));
        var rejectedQuote = CreateQuote(customers[2], issuer, today.AddDays(-50), today.AddDays(-35),
            [new QuoteItemDraft("Architecture workshop", 2m, "session", 650m, 0.21m)], now);
        rejectedQuote.MarkSent(now.AddMinutes(4));
        rejectedQuote.Reject(now.AddMinutes(5));
        var quotes = new[] { acceptedQuote, sentQuote, rejectedQuote };
        AssignQuoteNumbers(quotes);

        var paidInvoice = Invoice.CreateFromQuote(acceptedQuote, today.AddDays(-100), today.AddDays(-70), now);
        var overdueInvoice = CreateInvoice(customers[1], issuer, today.AddDays(-45), today.AddDays(-15),
            [new InvoiceItemDraft("Product design sprint", 20m, "hour", 85m, 0.21m)], now);
        var sentInvoice = CreateInvoice(customers[2], issuer, today.AddDays(-5), today.AddDays(25),
            [new InvoiceItemDraft("Support retainer", 1m, "month", 850m, 0.21m)], now);
        var draftInvoice = CreateInvoice(customers[0], issuer, today, today.AddDays(30),
            [new InvoiceItemDraft("Phase two discovery", 8m, "hour", 95m, 0.21m)], now);
        var cancelledInvoice = CreateInvoice(customers[2], issuer, today.AddDays(-130), today.AddDays(-100),
            [new InvoiceItemDraft("Cancelled workshop", 1m, "session", 650m, 0.21m)], now);
        var invoices = new[] { paidInvoice, overdueInvoice, sentInvoice, draftInvoice, cancelledInvoice };
        AssignInvoiceNumbers(invoices);

        paidInvoice.MarkSent(now.AddMinutes(6));
        paidInvoice.RegisterPayment(today.AddDays(-60), paidInvoice.Total, "DEMO-FULL", "Bank transfer", today, now.AddMinutes(7));
        overdueInvoice.MarkSent(now.AddMinutes(8));
        overdueInvoice.RegisterPayment(today, 900m, "DEMO-PARTIAL", "Bank transfer", today, now.AddMinutes(9));
        sentInvoice.MarkSent(now.AddMinutes(10));
        cancelledInvoice.Cancel(now.AddMinutes(11));

        var sequences = CreateSequences(quotes, invoices);
        context.CompanySettings.Add(settings);
        context.Customers.AddRange(customers);
        context.ProductServices.AddRange(services);
        context.Quotes.AddRange(quotes);
        context.Invoices.AddRange(invoices);
        context.DocumentNumberSequences.AddRange(sequences);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DemoSeedResult(customers.Length, services.Length, quotes.Length, invoices.Length, 2);
    }

    private static async Task<bool> HasBusinessDataAsync(InvoiceManagerDbContext context, CancellationToken cancellationToken) =>
        await context.CompanySettings.AnyAsync(cancellationToken) ||
        await context.Customers.AnyAsync(cancellationToken) ||
        await context.ProductServices.AnyAsync(cancellationToken) ||
        await context.Quotes.AnyAsync(cancellationToken) ||
        await context.Invoices.AnyAsync(cancellationToken) ||
        await context.Payments.AnyAsync(cancellationToken) ||
        await context.DocumentNumberSequences.AnyAsync(cancellationToken);

    private static Quote CreateQuote(Customer customer, IssuerSnapshot issuer, DateOnly issueDate, DateOnly validUntil, IEnumerable<QuoteItemDraft> items, DateTimeOffset now) =>
        Quote.Create(customer.Id, issuer.Copy(), Snapshot(customer), issueDate, validUntil, "Fictional portfolio demo data.", items, now);

    private static Invoice CreateInvoice(Customer customer, IssuerSnapshot issuer, DateOnly issueDate, DateOnly dueDate, IEnumerable<InvoiceItemDraft> items, DateTimeOffset now) =>
        Invoice.Create(customer.Id, issuer.Copy(), Snapshot(customer), issueDate, dueDate, "Fictional portfolio demo data.", items, now);

    private static CustomerSnapshot Snapshot(Customer customer) => CustomerSnapshot.Create(
        customer.CompanyName, customer.ContactPerson, customer.Street, customer.PostalCode,
        customer.City, customer.Country, customer.Email, customer.Phone, customer.VatNumber);

    private static void AssignQuoteNumbers(IEnumerable<Quote> quotes)
    {
        foreach (var group in quotes.GroupBy(quote => quote.IssueDate.Year))
        {
            var number = 0;
            foreach (var quote in group.OrderBy(quote => quote.IssueDate)) quote.AssignNumber($"Q-{group.Key}-{++number:0000}");
        }
    }

    private static void AssignInvoiceNumbers(IEnumerable<Invoice> invoices)
    {
        foreach (var group in invoices.GroupBy(invoice => invoice.IssueDate.Year))
        {
            var number = 0;
            foreach (var invoice in group.OrderBy(invoice => invoice.IssueDate)) invoice.AssignNumber($"INV-{group.Key}-{++number:0000}");
        }
    }

    private static List<DocumentNumberSequence> CreateSequences(IEnumerable<Quote> quotes, IEnumerable<Invoice> invoices)
    {
        var sequences = new List<DocumentNumberSequence>();
        AddSequences(sequences, DocumentType.Quote, quotes.GroupBy(value => value.IssueDate.Year).Select(group => (group.Key, group.Count())));
        AddSequences(sequences, DocumentType.Invoice, invoices.GroupBy(value => value.IssueDate.Year).Select(group => (group.Key, group.Count())));
        return sequences;
    }

    private static void AddSequences(List<DocumentNumberSequence> target, DocumentType type, IEnumerable<(int Year, int Count)> values)
    {
        foreach (var (year, count) in values)
        {
            var sequence = DocumentNumberSequence.Create(type, year);
            for (var index = 0; index < count; index++) sequence.AllocateNext();
            target.Add(sequence);
        }
    }
}
