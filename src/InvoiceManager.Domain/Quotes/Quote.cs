using InvoiceManager.Domain.Common;
using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Domain.Quotes;

public sealed class Quote
{
    private readonly List<QuoteItem> _items = [];

    public Guid Id { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public IssuerSnapshot Issuer { get; private set; } = null!;

    public CustomerSnapshot Customer { get; private set; } = null!;

    public DateOnly IssueDate { get; private set; }

    public DateOnly ValidUntil { get; private set; }

    public QuoteStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal VatTotal { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<QuoteItem> Items => _items.AsReadOnly();

    private Quote()
    {
    }

    public static Quote Create(
        Guid customerId,
        IssuerSnapshot issuer,
        CustomerSnapshot customer,
        DateOnly issueDate,
        DateOnly validUntil,
        string? notes,
        IEnumerable<QuoteItemDraft> items,
        DateTimeOffset utcNow)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer identifier is required.", nameof(customerId));
        }

        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(customer);

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Issuer = issuer,
            Customer = customer,
            Status = QuoteStatus.Draft,
            CreatedAt = utcNow.ToUniversalTime(),
        };

        quote.UpdateDraft(issueDate, validUntil, notes, items, utcNow);
        return quote;
    }

    public void UpdateDraft(
        DateOnly issueDate,
        DateOnly validUntil,
        string? notes,
        IEnumerable<QuoteItemDraft> items,
        DateTimeOffset utcNow)
    {
        EnsureDraft();

        if (validUntil < issueDate)
        {
            throw new ArgumentException("Valid until date cannot be earlier than the issue date.", nameof(validUntil));
        }

        ArgumentNullException.ThrowIfNull(items);
        var itemList = items.ToArray();
        if (itemList.Length == 0)
        {
            throw new ArgumentException("A quote must contain at least one item.", nameof(items));
        }

        IssueDate = issueDate;
        ValidUntil = validUntil;
        Notes = TextRules.Optional(notes, nameof(notes), 2000);
        _items.Clear();
        _items.AddRange(itemList.Select(item => QuoteItem.Create(
            Id,
            item.Description,
            item.Quantity,
            item.Unit,
            item.UnitPrice,
            item.VatRate)));
        RecalculateTotals();
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void AssignNumber(string number)
    {
        if (!string.IsNullOrEmpty(Number))
        {
            throw new InvalidOperationException("The quote number has already been assigned.");
        }

        Number = TextRules.Required(number, nameof(number), 32);
    }

    public void ChangeCustomer(
        Guid customerId,
        CustomerSnapshot customer,
        DateTimeOffset utcNow)
    {
        EnsureDraft();

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer identifier is required.", nameof(customerId));
        }

        ArgumentNullException.ThrowIfNull(customer);
        CustomerId = customerId;
        Customer = customer;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void MarkSent(DateTimeOffset utcNow)
    {
        EnsureStatus(QuoteStatus.Draft, "Only a draft quote can be marked as sent.");
        Status = QuoteStatus.Sent;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void Accept(DateTimeOffset utcNow)
    {
        EnsureStatus(QuoteStatus.Sent, "Only a sent quote can be accepted.");
        Status = QuoteStatus.Accepted;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void Reject(DateTimeOffset utcNow)
    {
        EnsureStatus(QuoteStatus.Sent, "Only a sent quote can be rejected.");
        Status = QuoteStatus.Rejected;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public bool Expire(DateOnly today, DateTimeOffset utcNow)
    {
        if (Status is not (QuoteStatus.Draft or QuoteStatus.Sent) || ValidUntil >= today)
        {
            return false;
        }

        Status = QuoteStatus.Expired;
        UpdatedAt = utcNow.ToUniversalTime();
        return true;
    }

    private void RecalculateTotals()
    {
        Subtotal = FinancialRules.RoundMoney(_items.Sum(item => item.NetAmount));
        VatTotal = FinancialRules.RoundMoney(_items.Sum(item => item.VatAmount));
        Total = FinancialRules.RoundMoney(_items.Sum(item => item.GrossAmount));
    }

    private void EnsureDraft()
    {
        EnsureStatus(QuoteStatus.Draft, "Only a draft quote can be edited.");
    }

    private void EnsureStatus(QuoteStatus requiredStatus, string message)
    {
        if (Status != requiredStatus)
        {
            throw new InvalidOperationException(message);
        }
    }
}
