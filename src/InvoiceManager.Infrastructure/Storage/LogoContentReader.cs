using InvoiceManager.Application.Common.Storage;

namespace InvoiceManager.Infrastructure.Storage;

public sealed class LogoContentReader : ILogoContentReader
{
    private const long MaximumLogoSize = 5 * 1024 * 1024;

    public async Task<byte[]?> ReadAsync(string? path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new InvalidOperationException($"Company logo file was not found: {fullPath}");
        }

        var info = new FileInfo(fullPath);
        if (info.Length > MaximumLogoSize)
        {
            throw new InvalidOperationException("Company logo cannot be larger than 5 MB.");
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }
}
