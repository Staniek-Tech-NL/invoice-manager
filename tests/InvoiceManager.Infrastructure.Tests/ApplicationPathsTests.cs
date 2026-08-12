using InvoiceManager.Infrastructure.Storage;

namespace InvoiceManager.Infrastructure.Tests;

public sealed class ApplicationPathsTests
{
    [Fact]
    public void CreateDefaultUsesExplicitDataDirectoryOverride()
    {
        var expected = Path.Combine(Path.GetTempPath(), "InvoiceManager.Override", Guid.NewGuid().ToString("N"));
        var previous = Environment.GetEnvironmentVariable(ApplicationPaths.DataDirectoryEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(ApplicationPaths.DataDirectoryEnvironmentVariable, expected);
            Assert.Equal(Path.GetFullPath(expected), ApplicationPaths.CreateDefault().RootDirectory);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ApplicationPaths.DataDirectoryEnvironmentVariable, previous);
        }
    }

    [Fact]
    public void EnsureDirectoriesExistCreatesTheExpectedStructure()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "InvoiceManager.Tests", Guid.NewGuid().ToString("N"));

        try
        {
            var paths = new ApplicationPaths(testRoot);

            paths.EnsureDirectoriesExist();

            Assert.True(Directory.Exists(paths.RootDirectory));
            Assert.True(Directory.Exists(paths.LogsDirectory));
            Assert.True(Directory.Exists(paths.AssetsDirectory));
            Assert.Equal(Path.Combine(paths.RootDirectory, "invoice-manager.db"), paths.DatabasePath);
        }
        finally
        {
            if (Directory.Exists(testRoot))
            {
                Directory.Delete(testRoot, recursive: true);
            }
        }
    }
}
