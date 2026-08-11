using CommunityToolkit.Mvvm.ComponentModel;

namespace InvoiceManager.App.ViewModels;

public sealed partial class QuoteLineEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _quantity = 1m;

    [ObservableProperty]
    private string _unit = "hour";

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _vatRatePercent = 21m;
}
