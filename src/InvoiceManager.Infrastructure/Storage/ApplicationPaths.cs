using InvoiceManager.Application.Common.Storage;

namespace InvoiceManager.Infrastructure.Storage;

public sealed class ApplicationPaths : IApplicationPaths
{
    public const string DataDirectoryEnvironmentVariable = "INVOICE_MANAGER_DATA_DIRECTORY";

    public ApplicationPaths(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        RootDirectory = Path.GetFullPath(rootDirectory);
        DatabasePath = Path.Combine(RootDirectory, "invoice-manager.db");
        LogsDirectory = Path.Combine(RootDirectory, "logs");
        AssetsDirectory = Path.Combine(RootDirectory, "assets");
    }

    public string RootDirectory { get; }

    public string DatabasePath { get; }

    public string LogsDirectory { get; }

    public string AssetsDirectory { get; }

    public static ApplicationPaths CreateDefault()
    {
        var overrideDirectory = Environment.GetEnvironmentVariable(DataDirectoryEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(overrideDirectory))
        {
            return new ApplicationPaths(overrideDirectory);
        }

        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.Create);

        return new ApplicationPaths(Path.Combine(localApplicationData, "InvoiceManager"));
    }

    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(AssetsDirectory);
    }
}
