namespace InvoiceManager.Application.Invoices;

public sealed class SearchInvoices(IInvoiceRepository repository)
{
    public async Task<IReadOnlyList<InvoiceDetails>> ExecuteAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var invoices = await repository.SearchAsync(searchTerm, cancellationToken);
        return invoices.Select(invoice => invoice.ToDetails()).ToArray();
    }
}
