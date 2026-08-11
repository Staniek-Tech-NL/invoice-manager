using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Products;
using InvoiceManager.Application.Quotes;
using InvoiceManager.Application.Settings;
using InvoiceManager.Domain.Quotes;

namespace InvoiceManager.App.ViewModels;

public sealed partial class QuotesViewModel(
    CreateQuote createQuote,
    UpdateQuote updateQuote,
    SearchQuotes searchQuotes,
    ChangeQuoteStatus changeQuoteStatus,
    SearchCustomers searchCustomers,
    SearchProductServices searchProductServices,
    GetCompanySettings getCompanySettings,
    IApplicationClock clock,
    IUserDialogService dialogService) : ObservableObject, IActivatableNavigationPage
{
    private Guid? _editingQuoteId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCreateEditorCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarkSentCommand))]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarkSentCommand))]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyCanExecuteChangedFor(nameof(RejectCommand))]
    private QuoteDetails? _selectedQuote;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editorTitle = "New quote";

    [ObservableProperty]
    private CustomerDetails? _selectedCustomer;

    [ObservableProperty]
    private ProductServiceDetails? _selectedCatalogItem;

    [ObservableProperty]
    private QuoteLineEditorViewModel? _selectedLineItem;

    [ObservableProperty]
    private DateTime? _issueDate;

    [ObservableProperty]
    private DateTime? _validUntil;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public NavigationDestination Destination => NavigationDestination.Quotes;

    public string Title => "Quotes";

    public ObservableCollection<QuoteDetails> Quotes { get; } = [];

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
        if (await getCompanySettings.ExecuteAsync() is null)
        {
            ErrorMessage = "Configure company settings before creating a quote.";
            return;
        }

        if (Customers.Count == 0)
        {
            ErrorMessage = "Create an active customer before creating a quote.";
            return;
        }

        _editingQuoteId = null;
        EditorTitle = "New quote";
        SelectedCustomer = Customers[0];
        IssueDate = clock.Today.ToDateTime(TimeOnly.MinValue);
        ValidUntil = clock.Today.AddDays(30).ToDateTime(TimeOnly.MinValue);
        Notes = string.Empty;
        LineItems.Clear();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private void EditSelected()
    {
        var quote = SelectedQuote!;
        _editingQuoteId = quote.Id;
        EditorTitle = $"Edit {quote.Number}";
        SelectedCustomer = Customers.FirstOrDefault(customer => customer.Id == quote.CustomerId);
        IssueDate = quote.IssueDate.ToDateTime(TimeOnly.MinValue);
        ValidUntil = quote.ValidUntil.ToDateTime(TimeOnly.MinValue);
        Notes = quote.Notes ?? string.Empty;
        LineItems.Clear();
        foreach (var item in quote.Items)
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
        if (SelectedCustomer is null || IssueDate is null || ValidUntil is null)
        {
            ErrorMessage = "Customer, issue date, and valid until date are required.";
            return;
        }

        var input = new QuoteInput(
            SelectedCustomer.Id,
            DateOnly.FromDateTime(IssueDate.Value),
            DateOnly.FromDateTime(ValidUntil.Value),
            Notes,
            LineItems.Select(item => new QuoteItemInput(
                item.Description,
                item.Quantity,
                item.Unit,
                item.UnitPrice,
                item.VatRatePercent / 100m)).ToArray());

        await RunBusyAsync(async () =>
        {
            var saved = _editingQuoteId is Guid quoteId
                ? await updateQuote.ExecuteAsync(quoteId, input)
                : await createQuote.ExecuteAsync(input);
            IsEditorOpen = false;
            StatusMessage = $"Quote {saved.Number} was saved.";
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
    private Task MarkSentAsync() => ChangeStatusAsync(QuoteStatus.Sent, "Mark selected quote as sent?");

    [RelayCommand(CanExecute = nameof(CanAcceptOrReject))]
    private Task AcceptAsync() => ChangeStatusAsync(QuoteStatus.Accepted, "Accept selected quote?");

    [RelayCommand(CanExecute = nameof(CanAcceptOrReject))]
    private Task RejectAsync() => ChangeStatusAsync(QuoteStatus.Rejected, "Reject selected quote?");

    private async Task ChangeStatusAsync(QuoteStatus status, string confirmation)
    {
        var quote = SelectedQuote!;
        if (!dialogService.Confirm("Change quote status", confirmation))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await changeQuoteStatus.ExecuteAsync(quote.Id, status);
            StatusMessage = $"Quote {quote.Number} is now {status}.";
            await RefreshCoreAsync(quote.Id);
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
        selectedId ??= SelectedQuote?.Id;
        var results = await searchQuotes.ExecuteAsync(SearchText, cancellationToken);
        Quotes.Clear();
        foreach (var quote in results)
        {
            Quotes.Add(quote);
        }

        SelectedQuote = selectedId is Guid id ? Quotes.FirstOrDefault(quote => quote.Id == id) : null;
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

    private bool CanEditSelected() => !IsBusy && SelectedQuote?.Status == QuoteStatus.Draft;

    private bool CanMarkSent() => !IsBusy && SelectedQuote?.Status == QuoteStatus.Draft;

    private bool CanAcceptOrReject() => !IsBusy && SelectedQuote?.Status == QuoteStatus.Sent;

    private bool CanSave() => !IsBusy && IsEditorOpen;
}
