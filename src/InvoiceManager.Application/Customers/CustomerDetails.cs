namespace InvoiceManager.Application.Customers;

public sealed record CustomerDetails(
    Guid Id,
    string CompanyName,
    string? ContactPerson,
    string Street,
    string PostalCode,
    string City,
    string Country,
    string? Email,
    string? Phone,
    string? VatNumber,
    string? Notes,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
