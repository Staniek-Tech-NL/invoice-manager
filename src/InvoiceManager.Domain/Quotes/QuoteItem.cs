using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Quotes;

public sealed class QuoteItem
{
    public Guid Id { get; private set; }

    public Guid QuoteId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public decimal VatRate { get; private set; }

    public decimal NetAmount { get; private set; }

    public decimal VatAmount { get; private set; }

    public decimal GrossAmount { get; private set; }

    private QuoteItem()
    {
    }

    public static QuoteItem Create(
        Guid quoteId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal vatRate)
    {
        if (quoteId == Guid.Empty)
        {
            throw new ArgumentException("Quote identifier is required.", nameof(quoteId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        if (vatRate is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(vatRate), "VAT rate must be between 0 and 1.");
        }

        var netAmount = FinancialRules.RoundMoney(quantity * unitPrice);
        var vatAmount = FinancialRules.RoundMoney(netAmount * vatRate);

        return new QuoteItem
        {
            Id = Guid.NewGuid(),
            QuoteId = quoteId,
            Description = TextRules.Required(description, nameof(description), 1000),
            Quantity = quantity,
            Unit = TextRules.Required(unit, nameof(unit), 50),
            UnitPrice = unitPrice,
            VatRate = vatRate,
            NetAmount = netAmount,
            VatAmount = vatAmount,
            GrossAmount = FinancialRules.RoundMoney(netAmount + vatAmount),
        };
    }
}
