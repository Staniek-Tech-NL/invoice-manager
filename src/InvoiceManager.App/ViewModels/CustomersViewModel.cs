using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.Application.Customers;

namespace InvoiceManager.App.ViewModels;

public sealed partial class CustomersViewModel(
    CreateCustomer createCustomer,
    UpdateCustomer updateCustomer,
    ArchiveCustomer archiveCustomer,
    SearchCustomers searchCustomers,
    IUserDialogService dialogService) : ObservableObject, IActivatableNavigationPage
{
    private Guid? _editingCustomerId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCreateEditorCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(ArchiveSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(ArchiveSelectedCommand))]
    private CustomerDetails? _selectedCustomer;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _includeArchived;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editorTitle = "Add customer";

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _contactPerson = string.Empty;

    [ObservableProperty]
    private string _street = string.Empty;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _country = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _vatNumber = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public NavigationDestination Destination => NavigationDestination.Customers;

    public string Title => "Customers";

    public ObservableCollection<CustomerDetails> Customers { get; } = [];

    public Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        return RefreshAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task SearchAsync()
    {
        return RefreshAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private void OpenCreateEditor()
    {
        _editingCustomerId = null;
        EditorTitle = "Add customer";
        ClearEditor();
        ClearFeedback();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private void EditSelected()
    {
        var customer = SelectedCustomer!;

        _editingCustomerId = customer.Id;
        EditorTitle = "Edit customer";
        CompanyName = customer.CompanyName;
        ContactPerson = customer.ContactPerson ?? string.Empty;
        Street = customer.Street;
        PostalCode = customer.PostalCode;
        City = customer.City;
        Country = customer.Country;
        Email = customer.Email ?? string.Empty;
        Phone = customer.Phone ?? string.Empty;
        VatNumber = customer.VatNumber ?? string.Empty;
        Notes = customer.Notes ?? string.Empty;
        ClearFeedback();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanArchiveSelected))]
    private async Task ArchiveSelectedAsync()
    {
        var customer = SelectedCustomer!;

        if (!dialogService.Confirm(
                "Archive customer",
                $"Archive {customer.CompanyName}? Historical documents will remain unchanged."))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await archiveCustomer.ExecuteAsync(customer.Id);
            StatusMessage = $"{customer.CompanyName} was archived.";
            await RefreshCoreAsync();
        });
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ClearFeedback();

        var input = new CustomerInput(
            CompanyName,
            ContactPerson,
            Street,
            PostalCode,
            City,
            Country,
            Email,
            Phone,
            VatNumber,
            Notes);

        await RunBusyAsync(async () =>
        {
            var savedCustomer = _editingCustomerId is Guid customerId
                ? await updateCustomer.ExecuteAsync(customerId, input)
                : await createCustomer.ExecuteAsync(input);

            IsEditorOpen = false;
            StatusMessage = $"{savedCustomer.CompanyName} was saved.";
            await RefreshCoreAsync(savedCustomer.Id);
        });
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditorOpen = false;
        ErrorMessage = null;
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await RunBusyAsync(() => RefreshCoreAsync(cancellationToken: cancellationToken));
    }

    private async Task RefreshCoreAsync(
        Guid? selectedCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        selectedCustomerId ??= SelectedCustomer?.Id;
        var results = await searchCustomers.ExecuteAsync(SearchText, IncludeArchived, cancellationToken);

        Customers.Clear();
        foreach (var customer in results)
        {
            Customers.Add(customer);
        }

        SelectedCustomer = selectedCustomerId is Guid customerId
            ? Customers.FirstOrDefault(customer => customer.Id == customerId)
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
        catch (ArgumentException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (KeyNotFoundException exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearEditor()
    {
        CompanyName = string.Empty;
        ContactPerson = string.Empty;
        Street = string.Empty;
        PostalCode = string.Empty;
        City = string.Empty;
        Country = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        VatNumber = string.Empty;
        Notes = string.Empty;
    }

    private void ClearFeedback()
    {
        ErrorMessage = null;
        StatusMessage = null;
    }

    private bool CanRun() => !IsBusy;

    private bool CanEditSelected() => !IsBusy && SelectedCustomer is not null;

    private bool CanArchiveSelected() => !IsBusy && SelectedCustomer is { IsArchived: false };

    private bool CanSave() => !IsBusy && IsEditorOpen;
}
