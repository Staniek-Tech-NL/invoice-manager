using InvoiceManager.Domain.Common;

namespace InvoiceManager.Domain.Payments;

public sealed class Payment
{
    public Guid Id { get; private set; }

    public Guid InvoiceId { get; private set; }

    public DateOnly PaymentDate { get; private set; }

    public decimal Amount { get; private set; }

    public string? Reference { get; private set; }

    public string? Method { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? VoidedAt { get; private set; }

    public string? VoidReason { get; private set; }

    public bool IsVoided => VoidedAt.HasValue;

    private Payment()
    {
    }

    public static Payment Create(
        Guid invoiceId,
        DateOnly paymentDate,
        decimal amount,
        string? reference,
        string? method,
        DateTimeOffset utcNow)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice identifier is required.", nameof(invoiceId));
        }

        var roundedAmount = FinancialRules.RoundMoney(amount);
        if (roundedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        }

        return new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            PaymentDate = paymentDate,
            Amount = roundedAmount,
            Reference = TextRules.Optional(reference, nameof(reference), 200),
            Method = TextRules.Optional(method, nameof(method), 100),
            CreatedAt = utcNow.ToUniversalTime(),
        };
    }

    public void Void(string reason, DateTimeOffset utcNow)
    {
        if (IsVoided)
        {
            throw new InvalidOperationException("The payment has already been voided.");
        }

        VoidReason = TextRules.Required(reason, nameof(reason), 500);
        VoidedAt = utcNow.ToUniversalTime();
    }
}
