using InvoiceManager.Application.Dashboard;
using InvoiceManager.Domain.Common;
using InvoiceManager.Domain.Invoices;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Dashboard;

public sealed class DashboardQuery(IDbContextFactory<InvoiceManagerDbContext> contextFactory) : IDashboardQuery
{
    public async Task<DashboardSummary> GetAsync(DateOnly today, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var invoices = await context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Payments)
            .ToListAsync(cancellationToken);
        var customerCount = await context.Customers.CountAsync(customer => !customer.IsArchived, cancellationToken);
        var activeInvoices = invoices.Where(invoice => invoice.Status != InvoiceStatus.Cancelled).ToArray();
        var collectibleInvoices = activeInvoices
            .Where(invoice => invoice.Status is InvoiceStatus.Sent or InvoiceStatus.Overdue)
            .ToArray();
        var payments = activeInvoices.SelectMany(invoice => invoice.Payments).Where(payment => !payment.IsVoided).ToArray();

        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var yearStart = new DateOnly(today.Year, 1, 1);
        var nextMonth = monthStart.AddMonths(1);
        var nextYear = yearStart.AddYears(1);
        var firstChartMonth = monthStart.AddMonths(-11);

        var monthlyRevenue = Enumerable.Range(0, 12)
            .Select(offset => firstChartMonth.AddMonths(offset))
            .Select(month => new MonthlyRevenue(
                month,
                FinancialRules.RoundMoney(payments
                    .Where(payment => payment.PaymentDate >= month && payment.PaymentDate < month.AddMonths(1))
                    .Sum(payment => payment.Amount))))
            .ToArray();

        var recent = activeInvoices
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.CreatedAt)
            .Take(5)
            .Select(invoice => new RecentInvoice(
                invoice.Id,
                invoice.Number,
                invoice.Customer.CompanyName,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.Status.ToString(),
                invoice.Total,
                invoice.OutstandingAmount))
            .ToArray();

        return new DashboardSummary(
            FinancialRules.RoundMoney(payments.Where(payment => payment.PaymentDate >= monthStart && payment.PaymentDate < nextMonth).Sum(payment => payment.Amount)),
            FinancialRules.RoundMoney(payments.Where(payment => payment.PaymentDate >= yearStart && payment.PaymentDate < nextYear).Sum(payment => payment.Amount)),
            FinancialRules.RoundMoney(collectibleInvoices.Sum(invoice => invoice.OutstandingAmount)),
            FinancialRules.RoundMoney(collectibleInvoices.Where(invoice => invoice.Status == InvoiceStatus.Overdue).Sum(invoice => invoice.OutstandingAmount)),
            activeInvoices.Length,
            customerCount,
            monthlyRevenue,
            recent);
    }
}
