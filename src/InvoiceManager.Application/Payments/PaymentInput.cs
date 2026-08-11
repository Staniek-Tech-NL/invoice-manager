namespace InvoiceManager.Application.Payments;

public sealed record PaymentInput(
    DateOnly PaymentDate,
    decimal Amount,
    string? Reference,
    string? Method);
