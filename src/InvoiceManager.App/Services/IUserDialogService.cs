namespace InvoiceManager.App.Services;

public interface IUserDialogService
{
    bool Confirm(string title, string message);

    string? ChoosePdfSavePath(string suggestedFileName);

    string? ChooseLogoPath();
}
