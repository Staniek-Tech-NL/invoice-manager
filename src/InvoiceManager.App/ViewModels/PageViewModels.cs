using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using InvoiceManager.App.Navigation;
using InvoiceManager.Application.Dashboard;

namespace InvoiceManager.App.ViewModels;

public sealed partial class DashboardViewModel(GetDashboardSummary getDashboardSummary)
    : ObservableObject, IActivatableNavigationPage
{
    [ObservableProperty] private decimal _revenueThisMonth;
    [ObservableProperty] private decimal _revenueThisYear;
    [ObservableProperty] private decimal _outstandingAmount;
    [ObservableProperty] private decimal _overdueAmount;
    [ObservableProperty] private int _invoiceCount;
    [ObservableProperty] private int _customerCount;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public NavigationDestination Destination => NavigationDestination.Dashboard;
    public string Title => "Dashboard";
    public ObservableCollection<MonthlyRevenueBar> RevenueChart { get; } = [];
    public ObservableCollection<RecentInvoice> RecentInvoices { get; } = [];

    public async Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var summary = await getDashboardSummary.ExecuteAsync(cancellationToken);
            RevenueThisMonth = summary.RevenueThisMonth;
            RevenueThisYear = summary.RevenueThisYear;
            OutstandingAmount = summary.OutstandingAmount;
            OverdueAmount = summary.OverdueAmount;
            InvoiceCount = summary.InvoiceCount;
            CustomerCount = summary.CustomerCount;

            var maximum = summary.MonthlyRevenue.Select(value => value.Amount).DefaultIfEmpty(0m).Max();
            RevenueChart.Clear();
            foreach (var month in summary.MonthlyRevenue)
            {
                RevenueChart.Add(new MonthlyRevenueBar(
                    month.Month.ToString("MMM", CultureInfo.InvariantCulture),
                    month.Amount,
                    maximum == 0m ? 4d : Math.Max(4d, (double)(month.Amount / maximum) * 120d)));
            }

            RecentInvoices.Clear();
            foreach (var invoice in summary.RecentInvoices) RecentInvoices.Add(invoice);
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed record MonthlyRevenueBar(string Label, decimal Amount, double Height);
