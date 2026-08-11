using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceManager.App.Navigation;
using InvoiceManager.Application.Settings;

namespace InvoiceManager.App.ViewModels;

public sealed partial class SettingsViewModel(
    GetCompanySettings getCompanySettings,
    SaveCompanySettings saveCompanySettings) : ObservableObject, IActivatableNavigationPage
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _street = string.Empty;

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _country = string.Empty;

    [ObservableProperty]
    private string _vatNumber = string.Empty;

    [ObservableProperty]
    private string _chamberOfCommerceNumber = string.Empty;

    [ObservableProperty]
    private string _iban = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _defaultVatRatePercentText = "21";

    [ObservableProperty]
    private string _defaultPaymentTermDaysText = "30";

    [ObservableProperty]
    private string _currency = "EUR";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public NavigationDestination Destination => NavigationDestination.Settings;

    public string Title => "Settings";

    public async Task ActivateAsync(CancellationToken cancellationToken = default)
    {
        await RunBusyAsync(async () =>
        {
            var settings = await getCompanySettings.ExecuteAsync(cancellationToken);
            if (settings is null)
            {
                return;
            }

            CompanyName = settings.CompanyName;
            Street = settings.Street;
            PostalCode = settings.PostalCode;
            City = settings.City;
            Country = settings.Country;
            VatNumber = settings.VatNumber ?? string.Empty;
            ChamberOfCommerceNumber = settings.ChamberOfCommerceNumber ?? string.Empty;
            Iban = settings.Iban ?? string.Empty;
            Email = settings.Email ?? string.Empty;
            Phone = settings.Phone ?? string.Empty;
            DefaultVatRatePercentText = (settings.DefaultVatRate * 100m).ToString("0.##", CultureInfo.CurrentCulture);
            DefaultPaymentTermDaysText = settings.DefaultPaymentTermDays.ToString(CultureInfo.CurrentCulture);
            Currency = settings.Currency;
        });
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;

        if (!TryParseDecimal(DefaultVatRatePercentText, out var vatRatePercent))
        {
            ErrorMessage = "Enter a valid default VAT percentage.";
            return;
        }

        if (!int.TryParse(DefaultPaymentTermDaysText, NumberStyles.None, CultureInfo.CurrentCulture, out var paymentTermDays))
        {
            ErrorMessage = "Enter a valid payment term in days.";
            return;
        }

        var input = new CompanySettingsInput(
            CompanyName,
            Street,
            PostalCode,
            City,
            Country,
            VatNumber,
            ChamberOfCommerceNumber,
            Iban,
            Email,
            Phone,
            vatRatePercent / 100m,
            paymentTermDays,
            Currency,
            null);

        await RunBusyAsync(async () =>
        {
            await saveCompanySettings.ExecuteAsync(input);
            StatusMessage = "Company settings were saved.";
        });
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
        finally
        {
            IsBusy = false;
        }
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        const NumberStyles Styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return decimal.TryParse(value, Styles, CultureInfo.CurrentCulture, out result) ||
            decimal.TryParse(value, Styles, CultureInfo.InvariantCulture, out result);
    }

    private bool CanSave() => !IsBusy;
}
