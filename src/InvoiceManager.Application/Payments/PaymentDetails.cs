namespace InvoiceManager.Application.Payments;

public sealed record PaymentDetails(
    Guid Id,
    DateOnly PaymentDate,
    decimal Amount,
    string? Reference,
    string? Method,
    DateTimeOffset CreatedAt,
    bool IsVoided,
    DateTimeOffset? VoidedAt,
    string? VoidReason);
