using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Domain.Tests;

public sealed class QuoteTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateCalculatesAndRoundsEachLineBeforeTotals()
    {
        var quote = CreateQuote(
            new QuoteItemDraft("Development", 1m, "hour", 10.005m, 0.21m),
            new QuoteItemDraft("Hosting", 2m, "item", 4.444m, 0.09m));

        Assert.Equal(18.90m, quote.Subtotal);
        Assert.Equal(2.90m, quote.VatTotal);
        Assert.Equal(21.80m, quote.Total);
        Assert.Equal(2, quote.Items.Count);
    }

    [Fact]
    public void CreateRejectsQuoteWithoutItems()
    {
        Assert.Throws<ArgumentException>(() => Quote.Create(
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
    public void CreateRejectsInvalidValidityRange()
    {
        Assert.Throws<ArgumentException>(() => Quote.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 12),
            new DateOnly(2026, 8, 11),
            null,
            [new QuoteItemDraft("Service", 1m, "hour", 50m, 0.21m)],
            UtcNow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ItemRejectsNonPositiveQuantity(decimal quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QuoteItem.Create(Guid.NewGuid(), "Service", quantity, "hour", 50m, 0.21m));
    }

    [Fact]
    public void NumberCanOnlyBeAssignedOnce()
    {
        var quote = CreateQuote();

        quote.AssignNumber("Q-2026-0001");

        Assert.Equal("Q-2026-0001", quote.Number);
        Assert.Throws<InvalidOperationException>(() => quote.AssignNumber("Q-2026-0002"));
    }

    [Fact]
    public void SentQuoteCanBeAcceptedButNotEdited()
    {
        var quote = CreateQuote();

        quote.MarkSent(UtcNow.AddHours(1));
        quote.Accept(UtcNow.AddHours(2));

        Assert.Equal(QuoteStatus.Accepted, quote.Status);
        Assert.Throws<InvalidOperationException>(() => quote.UpdateDraft(
            quote.IssueDate,
            quote.ValidUntil,
            null,
            [new QuoteItemDraft("Changed", 1m, "hour", 50m, 0.21m)],
            UtcNow.AddHours(3)));
    }

    [Fact]
    public void DraftCannotBeAcceptedDirectly()
    {
        var quote = CreateQuote();

        Assert.Throws<InvalidOperationException>(() => quote.Accept(UtcNow.AddHours(1)));
    }

    [Fact]
    public void EligibleQuoteExpiresAfterValidityDate()
    {
        var quote = CreateQuote();

        var changed = quote.Expire(new DateOnly(2026, 9, 11), UtcNow.AddDays(31));

        Assert.True(changed);
        Assert.Equal(QuoteStatus.Expired, quote.Status);
    }

    [Fact]
    public void AcceptedQuoteDoesNotExpire()
    {
        var quote = CreateQuote();
        quote.MarkSent(UtcNow.AddHours(1));
        quote.Accept(UtcNow.AddHours(2));

        var changed = quote.Expire(new DateOnly(2026, 9, 11), UtcNow.AddDays(31));

        Assert.False(changed);
        Assert.Equal(QuoteStatus.Accepted, quote.Status);
    }

    private static Quote CreateQuote(params QuoteItemDraft[] items)
    {
        if (items.Length == 0)
        {
            items = [new QuoteItemDraft("Development", 2m, "hour", 75m, 0.21m)];
        }
        return Quote.Create(
            Guid.NewGuid(),
            CreateIssuer(),
            CreateCustomer(),
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 9, 10),
            "Thank you",
            items,
            UtcNow);
    }

    private static IssuerSnapshot CreateIssuer()
    {
        return IssuerSnapshot.Create(
            "Issuer BV",
            "Issuer Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            "NL123",
            null,
            null,
            "issuer@example.com",
            null);
    }

    private static CustomerSnapshot CreateCustomer()
    {
        return CustomerSnapshot.Create(
            "Customer BV",
            null,
            "Customer Street 1",
            "2000 AB",
            "Rotterdam",
            "Netherlands",
            "customer@example.com",
            null,
            null);
    }
}
