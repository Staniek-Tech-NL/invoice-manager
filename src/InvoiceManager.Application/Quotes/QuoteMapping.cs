using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Application.Quotes;

internal static class QuoteMapping
{
    public static QuoteDetails ToDetails(this Quote quote)
    {
        return new QuoteDetails(
            quote.Id,
            quote.Number,
            quote.CustomerId,
            quote.Customer.CompanyName,
            quote.IssueDate,
            quote.ValidUntil,
            quote.Status,
            quote.Notes,
            quote.Subtotal,
            quote.VatTotal,
            quote.Total,
            quote.CreatedAt,
            quote.UpdatedAt,
            quote.Items.Select(item => new QuoteItemDetails(
                item.Id,
                item.Description,
                item.Quantity,
                item.Unit,
                item.UnitPrice,
                item.VatRate,
                item.NetAmount,
                item.VatAmount,
                item.GrossAmount)).ToArray());
    }
}
