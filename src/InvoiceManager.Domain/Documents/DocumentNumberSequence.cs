namespace InvoiceManager.Domain.Documents;

public sealed class DocumentNumberSequence
{
    public Guid Id { get; private set; }

    public DocumentType DocumentType { get; private set; }

    public int Year { get; private set; }

    public int LastNumber { get; private set; }

    private DocumentNumberSequence()
    {
    }
}
