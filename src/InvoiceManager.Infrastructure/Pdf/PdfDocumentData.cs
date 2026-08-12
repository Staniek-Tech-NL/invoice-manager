using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Infrastructure.Pdf;

internal sealed record PdfDocumentData(
    string Kind,
    string Number,
    IssuerSnapshot Issuer,
    CustomerSnapshot Customer,
    DateOnly IssueDate,
    string SecondaryDateLabel,
    DateOnly SecondaryDate,
    string? Notes,
    decimal Subtotal,
    decimal VatTotal,
    decimal Total,
    IReadOnlyList<PdfLineData> Lines,
    string? PaymentStatus);

internal sealed record PdfLineData(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal VatRate,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);

internal static class PdfDocumentDataFactory
{
    public static PdfDocumentData FromInvoice(Invoice invoice) => new(
        "INVOICE", invoice.Number, invoice.Issuer, invoice.Customer, invoice.IssueDate,
        "Due date", invoice.DueDate, invoice.Notes, invoice.Subtotal, invoice.VatTotal, invoice.Total,
        invoice.Items.Select(item => new PdfLineData(item.Description, item.Quantity, item.Unit, item.UnitPrice,
            item.VatRate, item.NetAmount, item.VatAmount, item.GrossAmount)).ToArray(),
        $"Paid: {invoice.PaidAmount:N2} EUR   Outstanding: {invoice.OutstandingAmount:N2} EUR");

    public static PdfDocumentData FromQuote(Quote quote) => new(
        "QUOTATION", quote.Number, quote.Issuer, quote.Customer, quote.IssueDate,
        "Valid until", quote.ValidUntil, quote.Notes, quote.Subtotal, quote.VatTotal, quote.Total,
        quote.Items.Select(item => new PdfLineData(item.Description, item.Quantity, item.Unit, item.UnitPrice,
            item.VatRate, item.NetAmount, item.VatAmount, item.GrossAmount)).ToArray(), null);
}
