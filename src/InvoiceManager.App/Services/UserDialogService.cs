using System.Windows;

namespace InvoiceManager.App.Services;

public sealed class UserDialogService : IUserDialogService
{
    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No) == MessageBoxResult.Yes;
    }
}
