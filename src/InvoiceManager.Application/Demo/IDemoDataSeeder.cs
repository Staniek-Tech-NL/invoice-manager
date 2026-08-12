namespace InvoiceManager.Application.Demo;

public interface IDemoDataSeeder
{
    Task<DemoSeedResult> SeedAsync(CancellationToken cancellationToken = default);
}

public sealed record DemoSeedResult(
    int Customers,
    int Services,
    int Quotes,
    int Invoices,
    int Payments);
