namespace InvoiceManager.Application.Dashboard;

public sealed record DashboardSummary(
    decimal RevenueThisMonth,
    decimal RevenueThisYear,
    decimal OutstandingAmount,
    decimal OverdueAmount,
    int InvoiceCount,
    int CustomerCount,
    IReadOnlyList<MonthlyRevenue> MonthlyRevenue,
    IReadOnlyList<RecentInvoice> RecentInvoices);

public sealed record MonthlyRevenue(DateOnly Month, decimal Amount);

public sealed record RecentInvoice(
    Guid Id,
    string Number,
    string CustomerCompanyName,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Status,
    decimal Total,
    decimal OutstandingAmount);
