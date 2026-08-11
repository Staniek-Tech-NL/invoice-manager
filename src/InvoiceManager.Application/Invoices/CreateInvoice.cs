using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Documents;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Invoices;

public sealed class CreateInvoice(
    IInvoiceRepository invoiceRepository,
    ICustomerRepository customerRepository,
    ICompanySettingsRepository settingsRepository,
    IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        InvoiceInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var customer = await customerRepository.GetByIdAsync(input.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {input.CustomerId} was not found.");
        if (customer.IsArchived)
        {
            throw new InvalidOperationException("Archived customers cannot be selected for a new invoice.");
        }

        var settings = await settingsRepository.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Configure company settings before creating an invoice.");
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
        var invoice = Invoice.Create(
            customer.Id,
            settings.CreateSnapshot(),
            customerSnapshot,
            input.IssueDate,
            input.DueDate,
            input.Notes,
            ToDrafts(input.Items),
            clock.UtcNow);

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        return invoice.ToDetails();
    }

    internal static IEnumerable<InvoiceItemDraft> ToDrafts(IEnumerable<InvoiceItemInput> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Select(item => new InvoiceItemDraft(
            item.Description,
            item.Quantity,
            item.Unit,
            item.UnitPrice,
            item.VatRate));
    }
}
