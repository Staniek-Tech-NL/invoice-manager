using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Domain.Tests;

public sealed class IssuerSnapshotTests
{
    [Fact]
    public void LogoContentIsCopiedWhenSnapshotIsCreatedAndCopied()
    {
        var source = new byte[] { 1, 2, 3 };
        var snapshot = IssuerSnapshot.Create("Issuer", "Street", "1000 AA", "City", "Country", null, null, null, null, null, source);
        source[0] = 9;

        var copy = snapshot.Copy();
        snapshot.LogoContent![1] = 8;

        Assert.Equal(new byte[] { 1, 2, 3 }, copy.LogoContent);
        Assert.Equal(1, snapshot.LogoContent[0]);
    }
}
