namespace InvoiceManager.Application.Documents;

public sealed class GenerateQuotePdf(IDocumentPdfGenerator generator)
{
    public Task ExecuteAsync(Guid quoteId, string outputPath, CancellationToken cancellationToken = default)
    {
        if (quoteId == Guid.Empty) throw new ArgumentException("Quote identifier is required.", nameof(quoteId));
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("PDF output path is required.", nameof(outputPath));
        return generator.GenerateQuoteAsync(quoteId, outputPath, cancellationToken);
    }
}
