namespace InvoiceManager.Application.Common.Storage;

public interface ILogoContentReader
{
    Task<byte[]?> ReadAsync(string? path, CancellationToken cancellationToken);
}
