using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Payments;

namespace InvoiceManager.Domain.Tests;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 11);

    [Fact]
    public void PaymentCreationRoundsAmountAndNormalizesAuditData()
    {
        var payment = Payment.Create(
            Guid.NewGuid(),
            Today,
            10.005m,
            "  REF-1  ",
            "  Bank transfer  ",
            UtcNow.ToOffset(TimeSpan.FromHours(2)));

        Assert.Equal(10.01m, payment.Amount);
        Assert.Equal("REF-1", payment.Reference);
        Assert.Equal(TimeSpan.Zero, payment.CreatedAt.Offset);
        Assert.False(payment.IsVoided);
    }

    [Fact]
    public void PaymentRequiresPositiveAmount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Payment.Create(Guid.NewGuid(), Today, 0m, null, null, UtcNow));
    }

    [Fact]
    public void VoidRequiresReasonAndCannotBeRepeated()
    {
        var payment = Payment.Create(Guid.NewGuid(), Today, 10m, null, null, UtcNow);

        Assert.Throws<ArgumentException>(() => payment.Void(" ", UtcNow.AddHours(1)));
        payment.Void("Incorrect bank reference", UtcNow.AddHours(1));

        Assert.True(payment.IsVoided);
        Assert.Equal("Incorrect bank reference", payment.VoidReason);
        Assert.Throws<InvalidOperationException>(() => payment.Void("Again", UtcNow.AddHours(2)));
    }

    [Fact]
    public void PartialPaymentReducesOutstandingAndKeepsSentStatus()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m);

        invoice.RegisterPayment(Today, 40m, null, "Bank transfer", Today, UtcNow.AddHours(1));

        Assert.Equal(40m, invoice.PaidAmount);
        Assert.Equal(81m, invoice.OutstandingAmount);
        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
    }

    [Fact]
    public void FullPaymentSetsPaidStatus()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m);

        invoice.RegisterPayment(Today, 121m, null, null, Today, UtcNow.AddHours(1));

        Assert.Equal(0m, invoice.OutstandingAmount);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void OverpaymentIsRejectedWithoutChangingBalance()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m);

        Assert.Throws<InvalidOperationException>(() =>
            invoice.RegisterPayment(Today, 121.01m, null, null, Today, UtcNow.AddHours(1)));

        Assert.Empty(invoice.Payments);
        Assert.Equal(121m, invoice.OutstandingAmount);
    }

    [Fact]
    public void SentInvoiceBecomesOverdueWhenBalanceRemainsAfterDueDate()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m, dueDate: Today.AddDays(-1));
        invoice.RegisterPayment(Today, 40m, null, null, Today, UtcNow.AddHours(1));

        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        Assert.Equal(81m, invoice.OutstandingAmount);
    }

    [Fact]
    public void DraftAndCancelledInvoicesNeverBecomeOverdue()
    {
        var draft = CreateInvoice(totalUnitPrice: 100m, dueDate: Today.AddDays(-1));
        var cancelled = CreateInvoice(totalUnitPrice: 100m, dueDate: Today.AddDays(-1));
        cancelled.Cancel(UtcNow.AddHours(1));

        draft.RefreshStatus(Today, UtcNow.AddHours(2));
        cancelled.RefreshStatus(Today, UtcNow.AddHours(2));

        Assert.Equal(InvoiceStatus.Draft, draft.Status);
        Assert.Equal(InvoiceStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public void VoidingFullPaymentRestoresOverdueAndPreservesHistory()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m, dueDate: Today.AddDays(-1));
        var payment = invoice.RegisterPayment(Today, 121m, null, null, Today, UtcNow.AddHours(1));

        invoice.VoidPayment(payment.Id, "Duplicate import", Today, UtcNow.AddHours(2));

        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        Assert.Equal(0m, invoice.PaidAmount);
        Assert.Equal(121m, invoice.OutstandingAmount);
        Assert.True(Assert.Single(invoice.Payments).IsVoided);
    }

    [Fact]
    public void InvoiceWithActivePaymentCannotBeCancelled()
    {
        var invoice = CreateSentInvoice(totalUnitPrice: 100m);
        invoice.RegisterPayment(Today, 40m, null, null, Today, UtcNow.AddHours(1));

        Assert.Throws<InvalidOperationException>(() => invoice.Cancel(UtcNow.AddHours(2)));
    }

    private static Invoice CreateSentInvoice(decimal totalUnitPrice, DateOnly? dueDate = null)
    {
        var invoice = CreateInvoice(totalUnitPrice, dueDate);
        invoice.MarkSent(UtcNow.AddMinutes(1));
        return invoice;
    }

    private static Invoice CreateInvoice(decimal totalUnitPrice, DateOnly? dueDate)
    {
        return Invoice.Create(
            Guid.NewGuid(),
            IssuerSnapshot.Create(
                "Issuer BV", "Street 1", "1000 AA", "Amsterdam", "Netherlands",
                null, null, null, null, null),
            CustomerSnapshot.Create(
                "Customer BV", null, "Street 2", "2000 AB", "Rotterdam", "Netherlands",
                null, null, null),
            Today.AddDays(-30),
            dueDate ?? Today.AddDays(30),
            null,
            [new InvoiceItemDraft("Service", 1m, "item", totalUnitPrice, 0.21m)],
            UtcNow);
    }
}
