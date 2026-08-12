using InvoiceManager.Application.Common.Storage;
using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.Application.Quotes;

public sealed class CreateQuote(
    IQuoteRepository quoteRepository,
    ICustomerRepository customerRepository,
    ICompanySettingsRepository settingsRepository,
    IApplicationClock clock,
    ILogoContentReader? logoContentReader = null)
{
    public async Task<QuoteDetails> ExecuteAsync(
        QuoteInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var customer = await customerRepository.GetByIdAsync(input.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {input.CustomerId} was not found.");

        if (customer.IsArchived)
        {
            throw new InvalidOperationException("Archived customers cannot be selected for a new quote.");
        }

        var settings = await settingsRepository.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Configure company settings before creating a quote.");

        var customerSnapshot = CustomerSnapshot.Create(
            customer.CompanyName,
            customer.ContactPerson,
            customer.Street,
            customer.PostalCode,
            customer.City,
            customer.Country,
            customer.Email,
            customer.Phone,
            customer.VatNumber);

        var logoContent = logoContentReader is null
            ? null
            : await logoContentReader.ReadAsync(settings.LogoPath, cancellationToken);
        var quote = Quote.Create(
            customer.Id,
            settings.CreateSnapshot(logoContent),
            customerSnapshot,
            input.IssueDate,
            input.ValidUntil,
            input.Notes,
            ToDrafts(input.Items),
            clock.UtcNow);

        await quoteRepository.AddAsync(quote, cancellationToken);
        return quote.ToDetails();
    }

    internal static IEnumerable<QuoteItemDraft> ToDrafts(IEnumerable<QuoteItemInput> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Select(item => new QuoteItemDraft(
            item.Description,
            item.Quantity,
            item.Unit,
            item.UnitPrice,
            item.VatRate));
    }
}
