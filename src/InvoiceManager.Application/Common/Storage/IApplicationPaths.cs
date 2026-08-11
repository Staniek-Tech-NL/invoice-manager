namespace InvoiceManager.Application.Common.Storage;

public interface IApplicationPaths
{
    string RootDirectory { get; }

    string DatabasePath { get; }

    string LogsDirectory { get; }

    string AssetsDirectory { get; }
}
