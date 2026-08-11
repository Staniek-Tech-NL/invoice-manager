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

    private Payment()
    {
    }
}
