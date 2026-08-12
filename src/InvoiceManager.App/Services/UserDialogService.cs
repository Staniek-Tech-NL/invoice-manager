using System.Windows;
using Microsoft.Win32;

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

    public string? ChoosePdfSavePath(string suggestedFileName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export PDF",
            FileName = suggestedFileName,
            DefaultExt = ".pdf",
            Filter = "PDF document (*.pdf)|*.pdf",
            AddExtension = true,
            OverwritePrompt = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ChooseLogoPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose company logo",
            Filter = "Supported images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
