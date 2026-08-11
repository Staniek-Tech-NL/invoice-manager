using InvoiceManager.Domain.Common;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Payments;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Domain.Invoices;

public sealed class Invoice
{
    private readonly List<InvoiceItem> _items = [];
    private readonly List<Payment> _payments = [];

    public Guid Id { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public Guid? SourceQuoteId { get; private set; }

    public IssuerSnapshot Issuer { get; private set; } = null!;

    public CustomerSnapshot Customer { get; private set; } = null!;

    public DateOnly IssueDate { get; private set; }

    public DateOnly DueDate { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal VatTotal { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public decimal PaidAmount => FinancialRules.RoundMoney(
        _payments.Where(payment => !payment.IsVoided).Sum(payment => payment.Amount));

    public decimal OutstandingAmount => FinancialRules.RoundMoney(Total - PaidAmount);

    private Invoice()
    {
    }

    public static Invoice Create(
        Guid customerId,
        IssuerSnapshot issuer,
        CustomerSnapshot customer,
        DateOnly issueDate,
        DateOnly dueDate,
        string? notes,
        IEnumerable<InvoiceItemDraft> items,
        DateTimeOffset utcNow)
    {
        return CreateCore(
            customerId,
            null,
            issuer,
            customer,
            issueDate,
            dueDate,
            notes,
            items,
            utcNow);
    }

    public static Invoice CreateFromQuote(
        Quote quote,
        DateOnly issueDate,
        DateOnly dueDate,
        DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(quote);
        if (quote.Status != QuoteStatus.Accepted)
        {
            throw new InvalidOperationException("Only an accepted quote can be converted to an invoice.");
        }

        return CreateCore(
            quote.CustomerId,
            quote.Id,
            quote.Issuer.Copy(),
            quote.Customer.Copy(),
            issueDate,
            dueDate,
            quote.Notes,
            quote.Items.Select(item => new InvoiceItemDraft(
                item.Description,
                item.Quantity,
                item.Unit,
                item.UnitPrice,
                item.VatRate)),
            utcNow);
    }

    public void UpdateDraft(
        DateOnly issueDate,
        DateOnly dueDate,
        string? notes,
        IEnumerable<InvoiceItemDraft> items,
        DateTimeOffset utcNow)
    {
        EnsureDraft();
        ValidateDates(issueDate, dueDate);
        ArgumentNullException.ThrowIfNull(items);
        var itemList = items.ToArray();
        if (itemList.Length == 0)
        {
            throw new ArgumentException("An invoice must contain at least one item.", nameof(items));
        }

        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = TextRules.Optional(notes, nameof(notes), 2000);
        _items.Clear();
        _items.AddRange(itemList.Select(item => InvoiceItem.Create(
            Id,
            item.Description,
            item.Quantity,
            item.Unit,
            item.UnitPrice,
            item.VatRate)));
        RecalculateTotals();
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void ChangeCustomer(Guid customerId, CustomerSnapshot customer, DateTimeOffset utcNow)
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

    public void AssignNumber(string number)
    {
        if (!string.IsNullOrEmpty(Number))
        {
            throw new InvalidOperationException("The invoice number has already been assigned.");
        }

        Number = TextRules.Required(number, nameof(number), 32);
    }

    public void MarkSent(DateTimeOffset utcNow)
    {
        EnsureStatus(InvoiceStatus.Draft, "Only a draft invoice can be marked as sent.");
        Status = InvoiceStatus.Sent;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public void Cancel(DateTimeOffset utcNow)
    {
        if (Status is not (InvoiceStatus.Draft or InvoiceStatus.Sent))
        {
            throw new InvalidOperationException("Only a draft or sent invoice can be cancelled.");
        }

        if (PaidAmount > 0)
        {
            throw new InvalidOperationException("An invoice with recorded payments cannot be cancelled.");
        }

        Status = InvoiceStatus.Cancelled;
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public Payment RegisterPayment(
        DateOnly paymentDate,
        decimal amount,
        string? reference,
        string? method,
        DateOnly today,
        DateTimeOffset utcNow)
    {
        RefreshStatus(today, utcNow);
        if (Status is not (InvoiceStatus.Sent or InvoiceStatus.Overdue))
        {
            throw new InvalidOperationException("Payments can only be recorded for sent or overdue invoices.");
        }

        var payment = Payment.Create(Id, paymentDate, amount, reference, method, utcNow);
        if (payment.Amount > OutstandingAmount)
        {
            throw new InvalidOperationException("Payment amount cannot exceed the outstanding balance.");
        }

        _payments.Add(payment);
        RefreshStatus(today, utcNow);
        UpdatedAt = utcNow.ToUniversalTime();
        return payment;
    }

    public void VoidPayment(
        Guid paymentId,
        string reason,
        DateOnly today,
        DateTimeOffset utcNow)
    {
        var payment = _payments.SingleOrDefault(value => value.Id == paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} was not found for this invoice.");
        payment.Void(reason, utcNow);
        RefreshStatus(today, utcNow);
        UpdatedAt = utcNow.ToUniversalTime();
    }

    public bool RefreshStatus(DateOnly today, DateTimeOffset utcNow)
    {
        var previousStatus = Status;
        Status = Status switch
        {
            InvoiceStatus.Cancelled => InvoiceStatus.Cancelled,
            InvoiceStatus.Draft => InvoiceStatus.Draft,
            _ when OutstandingAmount == 0m => InvoiceStatus.Paid,
            _ when DueDate < today => InvoiceStatus.Overdue,
            _ => InvoiceStatus.Sent,
        };

        if (Status != previousStatus)
        {
            UpdatedAt = utcNow.ToUniversalTime();
            return true;
        }

        return false;
    }

    private static Invoice CreateCore(
        Guid customerId,
        Guid? sourceQuoteId,
        IssuerSnapshot issuer,
        CustomerSnapshot customer,
        DateOnly issueDate,
        DateOnly dueDate,
        string? notes,
        IEnumerable<InvoiceItemDraft> items,
        DateTimeOffset utcNow)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer identifier is required.", nameof(customerId));
        }

        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(customer);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            SourceQuoteId = sourceQuoteId,
            Issuer = issuer,
            Customer = customer,
            Status = InvoiceStatus.Draft,
            CreatedAt = utcNow.ToUniversalTime(),
        };
        invoice.UpdateDraft(issueDate, dueDate, notes, items, utcNow);
        return invoice;
    }

    private static void ValidateDates(DateOnly issueDate, DateOnly dueDate)
    {
        if (dueDate < issueDate)
        {
            throw new ArgumentException("Due date cannot be earlier than the issue date.", nameof(dueDate));
        }
    }

    private void RecalculateTotals()
    {
        Subtotal = FinancialRules.RoundMoney(_items.Sum(item => item.NetAmount));
        VatTotal = FinancialRules.RoundMoney(_items.Sum(item => item.VatAmount));
        Total = FinancialRules.RoundMoney(_items.Sum(item => item.GrossAmount));
    }

    private void EnsureDraft()
    {
        EnsureStatus(InvoiceStatus.Draft, "Only a draft invoice can be edited.");
    }

    private void EnsureStatus(InvoiceStatus requiredStatus, string message)
    {
        if (Status != requiredStatus)
        {
            throw new InvalidOperationException(message);
        }
    }
}
