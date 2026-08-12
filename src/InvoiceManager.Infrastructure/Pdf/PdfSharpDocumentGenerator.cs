using InvoiceManager.Application.Documents;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Application.Quotes;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace InvoiceManager.Infrastructure.Pdf;

public sealed class PdfSharpDocumentGenerator(
    IInvoiceRepository invoiceRepository,
    IQuoteRepository quoteRepository) : IDocumentPdfGenerator
{
    private static readonly object FontLock = new();
    private static bool _fontsConfigured;

    public async Task GenerateInvoiceAsync(Guid invoiceId, string outputPath, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} was not found.");
        Generate(PdfDocumentDataFactory.FromInvoice(invoice), outputPath);
    }

    public async Task GenerateQuoteAsync(Guid quoteId, string outputPath, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quote {quoteId} was not found.");
        Generate(PdfDocumentDataFactory.FromQuote(quote), outputPath);
    }

    private static void Generate(PdfDocumentData data, string outputPath)
    {
        ConfigureFonts();
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("PDF output directory is required.", nameof(outputPath));
        }

        Directory.CreateDirectory(directory);
        using var document = new PdfDocument();
        document.Info.Title = $"{data.Kind} {data.Number}";
        document.Info.Author = data.Issuer.CompanyName;
        var renderer = new Renderer(document, data);
        renderer.Render();
        document.Save(fullPath);
    }

    private static void ConfigureFonts()
    {
        lock (FontLock)
        {
            if (_fontsConfigured) return;
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            _fontsConfigured = true;
        }
    }

    private sealed class Renderer(PdfDocument document, PdfDocumentData data)
    {
        private const double Margin = 42;
        private const double PageBottom = 800;
        private readonly XFont _body = new("Arial", 9);
        private readonly XFont _small = new("Arial", 8);
        private readonly XFont _heading = new("Arial", 22, XFontStyleEx.Bold);
        private readonly XFont _subheading = new("Arial", 11, XFontStyleEx.Bold);
        private XGraphics _graphics = null!;
        private double _y;

        public void Render()
        {
            AddPage();
            DrawHeader();
            DrawAddresses();
            DrawDates();
            DrawLineHeader();
            foreach (var line in data.Lines)
            {
                EnsureSpace(28);
                DrawLine(line);
            }

            EnsureSpace(145);
            DrawTotals();
            DrawFooter();
            _graphics.Dispose();
        }

        private void AddPage()
        {
            _graphics?.Dispose();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            _graphics = XGraphics.FromPdfPage(page);
            _y = Margin;
        }

        private void DrawHeader()
        {
            _graphics.DrawString(data.Kind, _heading, new XSolidBrush(XColor.FromArgb(45, 108, 223)), new XPoint(Margin, _y + 22));
            _graphics.DrawString(data.Number, _subheading, XBrushes.DarkSlateGray, new XPoint(Margin, _y + 43));
            if (data.Issuer.LogoContent is { Length: > 0 } logo)
            {
                try
                {
                    using var stream = new MemoryStream(logo, writable: false);
                    using var image = XImage.FromStream(stream);
                    var ratio = Math.Min(120d / image.PixelWidth, 55d / image.PixelHeight);
                    _graphics.DrawImage(image, 432, _y, image.PixelWidth * ratio, image.PixelHeight * ratio);
                }
                catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or NotSupportedException)
                {
                    // Invalid legacy logo data must not prevent document export.
                }
            }
            _y += 72;
            _graphics.DrawLine(new XPen(XColor.FromArgb(225, 230, 239)), Margin, _y, 553, _y);
            _y += 18;
        }

        private void DrawAddresses()
        {
            DrawAddress(Margin, "FROM", data.Issuer.CompanyName, data.Issuer.Street,
                $"{data.Issuer.PostalCode} {data.Issuer.City}", data.Issuer.Country,
                Optional("VAT", data.Issuer.VatNumber), Optional("CoC", data.Issuer.ChamberOfCommerceNumber), data.Issuer.Email);
            DrawAddress(310, "BILL TO", data.Customer.CompanyName, data.Customer.Street,
                $"{data.Customer.PostalCode} {data.Customer.City}", data.Customer.Country,
                data.Customer.ContactPerson, Optional("VAT", data.Customer.VatNumber), data.Customer.Email);
            _y += 120;
        }

        private void DrawAddress(double x, params string?[] lines)
        {
            var y = _y;
            foreach (var line in lines.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                var font = y == _y ? _small : _body;
                _graphics.DrawString(line!, font, XBrushes.DarkSlateGray, new XPoint(x, y + 10));
                y += 14;
            }
        }

        private void DrawDates()
        {
            _graphics.DrawString($"Issue date: {data.IssueDate:yyyy-MM-dd}", _body, XBrushes.DarkSlateGray, new XPoint(Margin, _y + 10));
            _graphics.DrawString($"{data.SecondaryDateLabel}: {data.SecondaryDate:yyyy-MM-dd}", _body, XBrushes.DarkSlateGray, new XPoint(310, _y + 10));
            _y += 30;
        }

        private void DrawLineHeader()
        {
            _graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(239, 243, 250)), Margin, _y, 511, 24);
            DrawCell("Description", Margin + 5, 250, _subheading);
            DrawCell("Qty", 302, 42, _subheading, true);
            DrawCell("Unit price", 350, 66, _subheading, true);
            DrawCell("VAT", 422, 44, _subheading, true);
            DrawCell("Gross", 472, 76, _subheading, true);
            _y += 28;
        }

        private void DrawLine(PdfLineData line)
        {
            DrawCell(Trim(line.Description, 48), Margin + 5, 250, _body);
            DrawCell($"{line.Quantity:0.##} {line.Unit}", 302, 42, _body, true);
            DrawCell($"{line.UnitPrice:N2}", 350, 66, _body, true);
            DrawCell($"{line.VatRate:P0}", 422, 44, _body, true);
            DrawCell($"{line.GrossAmount:N2}", 472, 76, _body, true);
            _graphics.DrawLine(new XPen(XColor.FromArgb(235, 238, 244)), Margin, _y + 22, 553, _y + 22);
            _y += 26;
        }

        private void DrawTotals()
        {
            _y += 8;
            DrawTotal("Subtotal", data.Subtotal, false);
            DrawTotal("VAT", data.VatTotal, false);
            DrawTotal("TOTAL EUR", data.Total, true);
            if (!string.IsNullOrWhiteSpace(data.PaymentStatus))
            {
                _graphics.DrawString(data.PaymentStatus, _body, XBrushes.DarkSlateGray, new XPoint(Margin, _y + 10));
                _y += 22;
            }
            if (!string.IsNullOrWhiteSpace(data.Issuer.Iban))
            {
                _graphics.DrawString($"Payment by bank transfer to IBAN: {data.Issuer.Iban}", _body, XBrushes.DarkSlateGray, new XPoint(Margin, _y + 10));
                _y += 18;
            }
            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                _graphics.DrawString($"Notes: {Trim(data.Notes, 100)}", _small, XBrushes.Gray, new XPoint(Margin, _y + 10));
                _y += 18;
            }
        }

        private void DrawTotal(string label, decimal amount, bool emphasized)
        {
            var font = emphasized ? _subheading : _body;
            _graphics.DrawString(label, font, XBrushes.DarkSlateGray, new XPoint(390, _y + 10));
            DrawCell($"{amount:N2}", 472, 76, font, true);
            _y += emphasized ? 22 : 18;
        }

        private void DrawFooter()
        {
            var text = string.Join("  |  ", new[] { data.Issuer.CompanyName, data.Issuer.Email, data.Issuer.Phone }.Where(value => !string.IsNullOrWhiteSpace(value)));
            _graphics.DrawString(text, _small, XBrushes.Gray, new XRect(Margin, 807, 511, 14), XStringFormats.Center);
            _graphics.DrawString($"Page {document.PageCount}", _small, XBrushes.Gray, new XRect(500, 807, 53, 14), XStringFormats.TopRight);
        }

        private void EnsureSpace(double required)
        {
            if (_y + required <= PageBottom) return;
            DrawFooter();
            AddPage();
            DrawLineHeader();
        }

        private void DrawCell(string text, double x, double width, XFont font, bool right = false)
        {
            var format = right ? XStringFormats.TopRight : XStringFormats.TopLeft;
            _graphics.DrawString(text, font, XBrushes.DarkSlateGray, new XRect(x, _y + 6, width, 18), format);
        }

        private static string? Optional(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label}: {value}";
        private static string Trim(string value, int length) => value.Length <= length ? value : value[..(length - 3)] + "...";
    }
}
