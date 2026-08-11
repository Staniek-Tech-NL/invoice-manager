namespace InvoiceManager.Domain.Invoices;

public sealed class InvoiceItem
{
    public Guid Id { get; private set; }

    public Guid InvoiceId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public decimal VatRate { get; private set; }

    public decimal NetAmount { get; private set; }

    public decimal VatAmount { get; private set; }

    public decimal GrossAmount { get; private set; }

    private InvoiceItem()
    {
    }
}
