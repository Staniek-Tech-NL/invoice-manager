using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Payments;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Tests;

public sealed class PaymentUseCaseTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RegisterPaymentReturnsUpdatedBalanceAndPaidStatus()
    {
        var invoice = CreateSentInvoice();
        var useCase = new RegisterPayment(new PaymentServiceStub(invoice), new FixedClock(UtcNow));

        var result = await useCase.ExecuteAsync(
            invoice.Id,
            new PaymentInput(new DateOnly(2026, 8, 11), 121m, "REF-1", "Bank transfer"),
            CancellationToken.None);

        Assert.Equal(InvoiceStatus.Paid, result.Status);
        Assert.Equal(0m, result.OutstandingAmount);
        Assert.Equal("REF-1", Assert.Single(result.Payments).Reference);
    }

    [Fact]
    public async Task RegisterPaymentPropagatesOverpaymentRejection()
    {
        var invoice = CreateSentInvoice();
        var useCase = new RegisterPayment(new PaymentServiceStub(invoice), new FixedClock(UtcNow));

        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(
            invoice.Id,
            new PaymentInput(new DateOnly(2026, 8, 11), 122m, null, null),
            CancellationToken.None));
    }

    [Fact]
    public async Task VoidPaymentReturnsAuditableEntryAndRestoredBalance()
    {
        var invoice = CreateSentInvoice();
        var payment = invoice.RegisterPayment(
            new DateOnly(2026, 8, 11),
            121m,
            null,
            null,
            new DateOnly(2026, 8, 11),
            UtcNow);
        var useCase = new VoidPayment(new PaymentServiceStub(invoice), new FixedClock(UtcNow.AddHours(1)));

        var result = await useCase.ExecuteAsync(
            invoice.Id,
            payment.Id,
            "Duplicate payment",
            CancellationToken.None);

        Assert.Equal(121m, result.OutstandingAmount);
        var voided = Assert.Single(result.Payments);
        Assert.True(voided.IsVoided);
        Assert.Equal("Duplicate payment", voided.VoidReason);
    }

    private static Invoice CreateSentInvoice()
    {
        var invoice = Invoice.Create(
            Guid.NewGuid(),
            IssuerSnapshot.Create(
                "Issuer BV", "Street 1", "1000 AA", "Amsterdam", "Netherlands",
                null, null, null, null, null),
            CustomerSnapshot.Create(
                "Customer BV", null, "Street 2", "2000 AB", "Rotterdam", "Netherlands",
                null, null, null),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            null,
            [new InvoiceItemDraft("Service", 1m, "item", 100m, 0.21m)],
            UtcNow);
        invoice.MarkSent(UtcNow);
        return invoice;
    }

    private sealed class PaymentServiceStub(Invoice invoice) : IInvoicePaymentService
    {
        public Task<Invoice> RegisterAsync(
            Guid invoiceId,
            PaymentInput input,
            DateOnly today,
            DateTimeOffset utcNow,
            CancellationToken cancellationToken)
        {
            invoice.RegisterPayment(input.PaymentDate, input.Amount, input.Reference, input.Method, today, utcNow);
            return Task.FromResult(invoice);
        }

        public Task<Invoice> VoidAsync(
            Guid invoiceId,
            Guid paymentId,
            string reason,
            DateOnly today,
            DateTimeOffset utcNow,
            CancellationToken cancellationToken)
        {
            invoice.VoidPayment(paymentId, reason, today, utcNow);
            return Task.FromResult(invoice);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IApplicationClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
    }
}
