using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using InvoiceManager.App.Navigation;
using InvoiceManager.App.ViewModels;
using InvoiceManager.Application.Documents;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.App.Services;

public sealed class PortfolioCaptureService(
    MainWindow window,
    INavigationService navigation,
    DashboardViewModel dashboard,
    QuotesViewModel quotes,
    SearchInvoices searchInvoices,
    GenerateInvoicePdf generateInvoicePdf)
{
    public async Task CaptureAsync(string outputDirectory)
    {
        var directory = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(directory);
        window.Width = 1920;
        window.Height = 1080;
        window.WindowState = WindowState.Normal;

        await CapturePageAsync(NavigationDestination.Dashboard, dashboard, Path.Combine(directory, "01-dashboard.png"));

        await ActivateAsync(NavigationDestination.Quotes, quotes);
        await quotes.OpenCreateEditorCommand.ExecuteAsync(null);
        if (quotes.CatalogItems.Count > 0)
        {
            quotes.SelectedCatalogItem = quotes.CatalogItems[0];
            quotes.AddCatalogItemCommand.Execute(null);
        }
        await WarmCaptureAsync(directory);
        await CaptureWindowAsync(Path.Combine(directory, "02-quote-editor.png"));

        var invoice = (await searchInvoices.ExecuteAsync(null)).First(value => value.Status == InvoiceStatus.Paid);
        await generateInvoicePdf.ExecuteAsync(invoice.Id, Path.Combine(directory, "03-invoice-pdf.pdf"));
    }

    private async Task CapturePageAsync(
        NavigationDestination destination,
        IActivatableNavigationPage page,
        string path)
    {
        await ActivateAsync(destination, page);
        await CaptureWindowAsync(path);
    }

    private async Task ActivateAsync(NavigationDestination destination, IActivatableNavigationPage page)
    {
        navigation.NavigateTo(destination);
        await page.ActivateAsync();
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.ApplicationIdle);
    }

    private async Task CaptureWindowAsync(string path)
    {
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.ApplicationIdle);
        await Task.Delay(100);
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.ApplicationIdle);
        var root = (FrameworkElement)window.Content;
        var bitmap = new RenderTargetBitmap(
            Math.Max(1, (int)Math.Ceiling(root.ActualWidth)),
            Math.Max(1, (int)Math.Ceiling(root.ActualHeight)),
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        await using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private async Task WarmCaptureAsync(string directory)
    {
        var path = Path.Combine(directory, ".capture-warmup.png");
        await CaptureWindowAsync(path);
        File.Delete(path);
    }
}
