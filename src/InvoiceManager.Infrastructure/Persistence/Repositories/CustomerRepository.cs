using InvoiceManager.Application.Customers;
using InvoiceManager.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(
    IDbContextFactory<InvoiceManagerDbContext> contextFactory) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Customers.Update(customer);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchTerm,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Customers.AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(customer => !customer.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

            query = query.Where(customer =>
                EF.Functions.Like(customer.CompanyName, pattern, "\\") ||
                EF.Functions.Like(customer.ContactPerson ?? string.Empty, pattern, "\\") ||
                EF.Functions.Like(customer.Email ?? string.Empty, pattern, "\\") ||
                EF.Functions.Like(customer.VatNumber ?? string.Empty, pattern, "\\"));
        }

        return await query
            .OrderBy(customer => customer.IsArchived)
            .ThenBy(customer => customer.CompanyName)
            .ToListAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
