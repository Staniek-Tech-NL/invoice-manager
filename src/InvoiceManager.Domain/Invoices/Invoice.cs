using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Domain.Invoices;

public sealed class Invoice
{
    public Guid Id { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

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

    private Invoice()
    {
    }
}
