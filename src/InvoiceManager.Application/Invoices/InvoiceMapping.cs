using InvoiceManager.Application.Payments;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

internal static class InvoiceMapping
{
    public static InvoiceDetails ToDetails(this Invoice invoice)
    {
        return new InvoiceDetails(
            invoice.Id,
            invoice.Number,
            invoice.CustomerId,
            invoice.SourceQuoteId,
            invoice.Customer.CompanyName,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Status,
            invoice.Notes,
            invoice.Subtotal,
            invoice.VatTotal,
            invoice.Total,
            invoice.PaidAmount,
            invoice.OutstandingAmount,
            invoice.CreatedAt,
            invoice.UpdatedAt,
            invoice.Items.Select(item => new InvoiceItemDetails(
                item.Id,
                item.Description,
                item.Quantity,
                item.Unit,
                item.UnitPrice,
                item.VatRate,
                item.NetAmount,
                item.VatAmount,
                item.GrossAmount)).ToArray(),
            invoice.Payments
                .OrderByDescending(payment => payment.PaymentDate)
                .ThenByDescending(payment => payment.CreatedAt)
                .Select(payment => new PaymentDetails(
                    payment.Id,
                    payment.PaymentDate,
                    payment.Amount,
                    payment.Reference,
                    payment.Method,
                    payment.CreatedAt,
                    payment.IsVoided,
                    payment.VoidedAt,
                    payment.VoidReason))
                .ToArray());
    }
}
