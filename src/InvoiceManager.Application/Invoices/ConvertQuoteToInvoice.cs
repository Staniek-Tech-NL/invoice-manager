using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Settings;

namespace InvoiceManager.Application.Invoices;

public sealed class ConvertQuoteToInvoice(
    IQuoteToInvoiceConverter converter,
    ICompanySettingsRepository settingsRepository,
    IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        Guid quoteId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Configure company settings before converting a quote.");
        var issueDate = clock.Today;
        var dueDate = issueDate.AddDays(settings.DefaultPaymentTermDays);
        var invoice = await converter.ConvertAsync(
            quoteId,
            issueDate,
            dueDate,
            clock.UtcNow,
            cancellationToken);
        return invoice.ToDetails();
    }
}
