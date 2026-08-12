namespace InvoiceManager.Application.Documents;

public interface IDocumentPdfGenerator
{
    Task GenerateInvoiceAsync(Guid invoiceId, string outputPath, CancellationToken cancellationToken);

    Task GenerateQuoteAsync(Guid quoteId, string outputPath, CancellationToken cancellationToken);
}
