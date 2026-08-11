using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Application.Products;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.App.ViewModels;

public sealed partial class InvoicesViewModel(
    CreateInvoice createInvoice,
    UpdateInvoice updateInvoice,
    SearchInvoices searchInvoices,
    ChangeInvoiceStatus changeInvoiceStatus,
    SearchCustomers searchCustomers,
    SearchProductServices searchProductServices,
    GetCompanySettings getCompanySettings,
    IApplicationClock clock,
    IUserDialogService dialogService) : ObservableObject, IActivatableNavigationPage
{
    private Guid? _editingInvoiceId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCreateEditorCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarkSentCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelInvoiceCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarkSentCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelInvoiceCommand))]
    private InvoiceDetails? _selectedInvoice;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editorTitle = "New invoice";

    [ObservableProperty]
    private CustomerDetails? _selectedCustomer;

    [ObservableProperty]
    private ProductServiceDetails? _selectedCatalogItem;

    [ObservableProperty]
    private QuoteLineEditorViewModel? _selectedLineItem;

    [ObservableProperty]
    private DateTime? _issueDate;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public NavigationDestination Destination => NavigationDestination.Invoices;

    public string Title => "Invoices";

    public ObservableCollection<InvoiceDetails> Invoices { get; } = [];

    public ObservableCollection<CustomerDetails> Customers { get; } = [];

    public ObservableCollection<ProductServiceDetails> CatalogItems { get; } = [];

    public ObservableCollection<QuoteLineEditorViewModel> LineItems { get; } = [];

    public async Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        await RefreshReferenceDataAsync(cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task OpenCreateEditorAsync()
    {
        ClearFeedback();
        var settings = await getCompanySettings.ExecuteAsync();
        if (settings is null)
        {
            ErrorMessage = "Configure company settings before creating an invoice.";
            return;
        }

        if (Customers.Count == 0)
        {
            ErrorMessage = "Create an active customer before creating an invoice.";
            return;
        }

        _editingInvoiceId = null;
        EditorTitle = "New invoice";
        SelectedCustomer = Customers[0];
        IssueDate = clock.Today.ToDateTime(TimeOnly.MinValue);
        DueDate = clock.Today.AddDays(settings.DefaultPaymentTermDays).ToDateTime(TimeOnly.MinValue);
        Notes = string.Empty;
        LineItems.Clear();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private void EditSelected()
    {
        var invoice = SelectedInvoice!;
        _editingInvoiceId = invoice.Id;
        EditorTitle = $"Edit {invoice.Number}";
        SelectedCustomer = Customers.FirstOrDefault(customer => customer.Id == invoice.CustomerId);
        IssueDate = invoice.IssueDate.ToDateTime(TimeOnly.MinValue);
        DueDate = invoice.DueDate.ToDateTime(TimeOnly.MinValue);
        Notes = invoice.Notes ?? string.Empty;
        LineItems.Clear();
        foreach (var item in invoice.Items)
        {
            LineItems.Add(new QuoteLineEditorViewModel
            {
                Description = item.Description,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                VatRatePercent = item.VatRate * 100m,
            });
        }

        ClearFeedback();
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void AddCatalogItem()
    {
        if (SelectedCatalogItem is null)
        {
            ErrorMessage = "Select a service to add.";
            return;
        }

        LineItems.Add(new QuoteLineEditorViewModel
        {
            Description = SelectedCatalogItem.Name,
            Quantity = 1m,
            Unit = SelectedCatalogItem.Unit,
            UnitPrice = SelectedCatalogItem.UnitPrice,
            VatRatePercent = SelectedCatalogItem.VatRate * 100m,
        });
        ErrorMessage = null;
    }

    [RelayCommand]
    private void RemoveSelectedLine()
    {
        if (SelectedLineItem is not null)
        {
            LineItems.Remove(SelectedLineItem);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ClearFeedback();
        if (SelectedCustomer is null || IssueDate is null || DueDate is null)
        {
            ErrorMessage = "Customer, issue date, and due date are required.";
            return;
        }

        var input = new InvoiceInput(
            SelectedCustomer.Id,
            DateOnly.FromDateTime(IssueDate.Value),
            DateOnly.FromDateTime(DueDate.Value),
            Notes,
            LineItems.Select(item => new InvoiceItemInput(
                item.Description,
                item.Quantity,
                item.Unit,
                item.UnitPrice,
                item.VatRatePercent / 100m)).ToArray());
        await RunBusyAsync(async () =>
        {
            var saved = _editingInvoiceId is Guid invoiceId
                ? await updateInvoice.ExecuteAsync(invoiceId, input)
                : await createInvoice.ExecuteAsync(input);
            IsEditorOpen = false;
            StatusMessage = $"Invoice {saved.Number} was saved.";
            await RefreshCoreAsync(saved.Id);
        });
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditorOpen = false;
        ErrorMessage = null;
    }

    [RelayCommand(CanExecute = nameof(CanMarkSent))]
    private Task MarkSentAsync() => ChangeStatusAsync(InvoiceStatus.Sent, "Mark selected invoice as sent?");

    [RelayCommand(CanExecute = nameof(CanCancelInvoice))]
    private Task CancelInvoiceAsync() => ChangeStatusAsync(InvoiceStatus.Cancelled, "Cancel selected invoice?");

    private async Task ChangeStatusAsync(InvoiceStatus status, string confirmation)
    {
        var invoice = SelectedInvoice!;
        if (!dialogService.Confirm("Change invoice status", confirmation))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await changeInvoiceStatus.ExecuteAsync(invoice.Id, status);
            StatusMessage = $"Invoice {invoice.Number} is now {status}.";
            await RefreshCoreAsync(invoice.Id);
        });
    }

    private async Task RefreshReferenceDataAsync(CancellationToken cancellationToken)
    {
        var customerResults = await searchCustomers.ExecuteAsync(null, false, cancellationToken);
        Customers.Clear();
        foreach (var customer in customerResults)
        {
            Customers.Add(customer);
        }

        var catalogResults = await searchProductServices.ExecuteAsync(null, false, cancellationToken);
        CatalogItems.Clear();
        foreach (var item in catalogResults)
        {
            CatalogItems.Add(item);
        }
    }

    private Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return RunBusyAsync(() => RefreshCoreAsync(cancellationToken: cancellationToken));
    }

    private async Task RefreshCoreAsync(Guid? selectedId = null, CancellationToken cancellationToken = default)
    {
        selectedId ??= SelectedInvoice?.Id;
        var results = await searchInvoices.ExecuteAsync(SearchText, cancellationToken);
        Invoices.Clear();
        foreach (var invoice in results)
        {
            Invoices.Add(invoice);
        }

        SelectedInvoice = selectedId is Guid id
            ? Invoices.FirstOrDefault(invoice => invoice.Id == id)
            : null;
    }

    private async Task RunBusyAsync(Func<Task> operation)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await operation();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFeedback()
    {
        ErrorMessage = null;
        StatusMessage = null;
    }

    private bool CanRun() => !IsBusy;

    private bool CanEditSelected() => !IsBusy && SelectedInvoice?.Status == InvoiceStatus.Draft;

    private bool CanMarkSent() => !IsBusy && SelectedInvoice?.Status == InvoiceStatus.Draft;

    private bool CanCancelInvoice() => !IsBusy && SelectedInvoice?.Status is InvoiceStatus.Draft or InvoiceStatus.Sent;

    private bool CanSave() => !IsBusy && IsEditorOpen;
}
