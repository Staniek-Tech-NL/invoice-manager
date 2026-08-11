using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Domain.Quotes;

public sealed class Quote
{
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

    private Quote()
    {
    }
}
