namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal static class DocumentNumberAllocationLock
{
    public static SemaphoreSlim Instance { get; } = new(1, 1);
}
