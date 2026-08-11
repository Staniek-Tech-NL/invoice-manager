using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Application.Invoices;

public sealed class UpdateInvoice(
    IInvoiceRepository invoiceRepository,
    ICustomerRepository customerRepository,
    IApplicationClock clock)
{
    public async Task<InvoiceDetails> ExecuteAsync(
        Guid invoiceId,
        InvoiceInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");
        var customer = await customerRepository.GetByIdAsync(input.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {input.CustomerId} was not found.");
        if (customer.IsArchived && customer.Id != invoice.CustomerId)
        {
            throw new InvalidOperationException("Archived customers cannot be selected for an invoice.");
        }

        if (customer.Id != invoice.CustomerId)
        {
            invoice.ChangeCustomer(
                customer.Id,
                CustomerSnapshot.Create(
                    customer.CompanyName,
                    customer.ContactPerson,
                    customer.Street,
                    customer.PostalCode,
                    customer.City,
                    customer.Country,
                    customer.Email,
                    customer.Phone,
                    customer.VatNumber),
                clock.UtcNow);
        }

        invoice.UpdateDraft(
            input.IssueDate,
            input.DueDate,
            input.Notes,
            CreateInvoice.ToDrafts(input.Items),
            clock.UtcNow);
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        return invoice.ToDetails();
    }
}
