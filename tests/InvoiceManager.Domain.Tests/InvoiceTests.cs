using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Domain.Tests;

public sealed class InvoiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateCalculatesRoundedLineAndDocumentTotals()
    {
        var invoice = CreateInvoice(
            new InvoiceItemDraft("Development", 1m, "hour", 10.005m, 0.21m),
            new InvoiceItemDraft("Hosting", 2m, "item", 4.444m, 0.09m));

        Assert.Equal(18.90m, invoice.Subtotal);
        Assert.Equal(2.90m, invoice.VatTotal);
        Assert.Equal(21.80m, invoice.Total);
    }

    [Fact]
    public void CreateRejectsInvoiceWithoutItems()
    {
        Assert.Throws<ArgumentException>(() => Invoice.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 9, 10),
            null,
            [],
            UtcNow));
    }

    [Fact]
    public void CreateRejectsDueDateBeforeIssueDate()
    {
        Assert.Throws<ArgumentException>(() => Invoice.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 8, 11),
            null,
            [new InvoiceItemDraft("Service", 1m, "hour", 75m, 0.21m)],
            UtcNow));
    }

    [Fact]
    public void SentInvoiceCannotBeEdited()
    {
        var invoice = CreateInvoice();
        invoice.MarkSent(UtcNow.AddHours(1));

        Assert.Throws<InvalidOperationException>(() => invoice.UpdateDraft(
            invoice.IssueDate,
            invoice.DueDate,
            null,
            [new InvoiceItemDraft("Changed", 1m, "hour", 80m, 0.21m)],
            UtcNow.AddHours(2)));
    }

    [Fact]
    public void DraftAndSentInvoicesCanBeCancelled()
    {
        var draft = CreateInvoice();
        var sent = CreateInvoice();
        sent.MarkSent(UtcNow.AddHours(1));

        draft.Cancel(UtcNow.AddHours(2));
        sent.Cancel(UtcNow.AddHours(2));

        Assert.Equal(InvoiceStatus.Cancelled, draft.Status);
        Assert.Equal(InvoiceStatus.Cancelled, sent.Status);
    }

    [Fact]
    public void NumberCanOnlyBeAssignedOnce()
    {
        var invoice = CreateInvoice();
        invoice.AssignNumber("INV-2026-0001");

        Assert.Throws<InvalidOperationException>(() => invoice.AssignNumber("INV-2026-0002"));
    }

    [Fact]
    public void AcceptedQuoteConversionCopiesSnapshotsItemsAndNotes()
    {
        var quote = CreateQuote(QuoteStatus.Accepted);

        var invoice = Invoice.CreateFromQuote(
            quote,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow.AddDays(1));

        Assert.Equal(quote.Id, invoice.SourceQuoteId);
        Assert.Equal(quote.Customer.CompanyName, invoice.Customer.CompanyName);
        Assert.Equal(quote.Issuer.CompanyName, invoice.Issuer.CompanyName);
        Assert.Equal(quote.Notes, invoice.Notes);
        Assert.Equal(quote.Total, invoice.Total);
        Assert.Equal(quote.Items.Single().Description, invoice.Items.Single().Description);
        Assert.NotSame(quote.Customer, invoice.Customer);
        Assert.NotSame(quote.Issuer, invoice.Issuer);
    }

    [Fact]
    public void NonAcceptedQuoteCannotBeConverted()
    {
        var quote = CreateQuote(QuoteStatus.Sent);

        Assert.Throws<InvalidOperationException>(() => Invoice.CreateFromQuote(
            quote,
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11),
            UtcNow));
    }

    private static Invoice CreateInvoice(params InvoiceItemDraft[] items)
    {
        if (items.Length == 0)
        {
            items = [new InvoiceItemDraft("Development", 2m, "hour", 75m, 0.21m)];
        }

        return Invoice.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 9, 10),
            "Invoice notes",
            items,
            UtcNow);
    }

    private static Quote CreateQuote(QuoteStatus targetStatus)
    {
        var quote = Quote.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            "Quote notes",
            [new QuoteItemDraft("Development", 2m, "hour", 75m, 0.21m)],
            UtcNow);
        quote.MarkSent(UtcNow.AddHours(1));
        if (targetStatus == QuoteStatus.Accepted)
        {
            quote.Accept(UtcNow.AddHours(2));
        }

        return quote;
    }

    private static IssuerSnapshot CreateIssuer() => IssuerSnapshot.Create(
        "Issuer BV", "Issuer Street 1", "1000 AA", "Amsterdam", "Netherlands",
        "NL123", null, null, "issuer@example.com", null);

    private static CustomerSnapshot CreateCustomer() => CustomerSnapshot.Create(
        "Customer BV", null, "Customer Street 1", "2000 AB", "Rotterdam", "Netherlands",
        "customer@example.com", null, null);
}
