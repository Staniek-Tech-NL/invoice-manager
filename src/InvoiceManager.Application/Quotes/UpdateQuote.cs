using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Domain.Documents;

namespace InvoiceManager.Application.Quotes;

public sealed class UpdateQuote(
    IQuoteRepository quoteRepository,
    ICustomerRepository customerRepository,
    IApplicationClock clock)
{
    public async Task<QuoteDetails> ExecuteAsync(
        Guid quoteId,
        QuoteInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var quote = await quoteRepository.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quote {quoteId} was not found.");
        var customer = await customerRepository.GetByIdAsync(input.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {input.CustomerId} was not found.");

        if (customer.IsArchived && customer.Id != quote.CustomerId)
        {
            throw new InvalidOperationException("Archived customers cannot be selected for a quote.");
        }

        if (customer.Id != quote.CustomerId)
        {
            quote.ChangeCustomer(
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

        quote.UpdateDraft(
            input.IssueDate,
            input.ValidUntil,
            input.Notes,
            CreateQuote.ToDrafts(input.Items),
            clock.UtcNow);
        await quoteRepository.UpdateAsync(quote, cancellationToken);
        return quote.ToDetails();
    }
}
