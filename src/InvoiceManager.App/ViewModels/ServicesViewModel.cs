using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.Services;
using InvoiceManager.Application.Products;

namespace InvoiceManager.App.ViewModels;

public sealed partial class ServicesViewModel(
    CreateProductService createProductService,
    UpdateProductService updateProductService,
    DeactivateProductService deactivateProductService,
    SearchProductServices searchProductServices,
    IUserDialogService dialogService) : ObservableObject, IActivatableNavigationPage
{
    private Guid? _editingProductServiceId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenCreateEditorCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeactivateSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeactivateSelectedCommand))]
    private ProductServiceDetails? _selectedProductService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _includeInactive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editorTitle = "Add service";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _unit = "hour";

    [ObservableProperty]
    private string _unitPriceText = string.Empty;

    [ObservableProperty]
    private string _vatRatePercentText = "21";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public NavigationDestination Destination => NavigationDestination.Services;

    public string Title => "Services";

    public ObservableCollection<ProductServiceDetails> ProductServices { get; } = [];

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
        _editingProductServiceId = null;
        EditorTitle = "Add service";
        Name = string.Empty;
        Description = string.Empty;
        Unit = "hour";
        UnitPriceText = string.Empty;
        VatRatePercentText = "21";
        ClearFeedback();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private void EditSelected()
    {
        var productService = SelectedProductService!;

        _editingProductServiceId = productService.Id;
        EditorTitle = "Edit service";
        Name = productService.Name;
        Description = productService.Description ?? string.Empty;
        Unit = productService.Unit;
        UnitPriceText = productService.UnitPrice.ToString("0.00", CultureInfo.CurrentCulture);
        VatRatePercentText = (productService.VatRate * 100m).ToString("0.##", CultureInfo.CurrentCulture);
        ClearFeedback();
        IsEditorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanDeactivateSelected))]
    private async Task DeactivateSelectedAsync()
    {
        var productService = SelectedProductService!;

        if (!dialogService.Confirm(
                "Deactivate service",
                $"Deactivate {productService.Name}? Historical documents will remain unchanged."))
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await deactivateProductService.ExecuteAsync(productService.Id);
            StatusMessage = $"{productService.Name} was deactivated.";
            await RefreshCoreAsync();
        });
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ClearFeedback();

        if (!TryParseDecimal(UnitPriceText, out var unitPrice))
        {
            ErrorMessage = "Enter a valid unit price.";
            return;
        }

        if (!TryParseDecimal(VatRatePercentText, out var vatRatePercent))
        {
            ErrorMessage = "Enter a valid VAT percentage.";
            return;
        }

        var input = new ProductServiceInput(Name, Description, Unit, unitPrice, vatRatePercent / 100m);

        await RunBusyAsync(async () =>
        {
            var savedProductService = _editingProductServiceId is Guid productServiceId
                ? await updateProductService.ExecuteAsync(productServiceId, input)
                : await createProductService.ExecuteAsync(input);

            IsEditorOpen = false;
            StatusMessage = $"{savedProductService.Name} was saved.";
            await RefreshCoreAsync(savedProductService.Id);
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
        Guid? selectedProductServiceId = null,
        CancellationToken cancellationToken = default)
    {
        selectedProductServiceId ??= SelectedProductService?.Id;
        var results = await searchProductServices.ExecuteAsync(SearchText, IncludeInactive, cancellationToken);

        ProductServices.Clear();
        foreach (var productService in results)
        {
            ProductServices.Add(productService);
        }

        SelectedProductService = selectedProductServiceId is Guid productServiceId
            ? ProductServices.FirstOrDefault(productService => productService.Id == productServiceId)
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

    private void ClearFeedback()
    {
        ErrorMessage = null;
        StatusMessage = null;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        const NumberStyles Styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        return decimal.TryParse(value, Styles, CultureInfo.CurrentCulture, out result) ||
            decimal.TryParse(value, Styles, CultureInfo.InvariantCulture, out result);
    }

    private bool CanRun() => !IsBusy;

    private bool CanEditSelected() => !IsBusy && SelectedProductService is not null;

    private bool CanDeactivateSelected() => !IsBusy && SelectedProductService is { IsActive: true };

    private bool CanSave() => !IsBusy && IsEditorOpen;
}
