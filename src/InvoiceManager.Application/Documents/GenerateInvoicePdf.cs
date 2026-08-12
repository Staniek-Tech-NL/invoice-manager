namespace InvoiceManager.Application.Documents;

public sealed class GenerateInvoicePdf(IDocumentPdfGenerator generator)
{
    public Task ExecuteAsync(Guid invoiceId, string outputPath, CancellationToken cancellationToken = default)
    {
        Validate(invoiceId, outputPath);
        return generator.GenerateInvoiceAsync(invoiceId, outputPath, cancellationToken);
    }

    private static void Validate(Guid id, string path)
    {
        if (id == Guid.Empty) throw new ArgumentException("Invoice identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("PDF output path is required.", nameof(path));
    }
}
