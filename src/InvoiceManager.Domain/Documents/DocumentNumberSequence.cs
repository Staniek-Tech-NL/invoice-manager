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

    public static DocumentNumberSequence Create(DocumentType documentType, int year)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1);

        return new DocumentNumberSequence
        {
            Id = Guid.NewGuid(),
            DocumentType = documentType,
            Year = year,
        };
    }

    public int AllocateNext()
    {
        LastNumber = checked(LastNumber + 1);
        return LastNumber;
    }
}
