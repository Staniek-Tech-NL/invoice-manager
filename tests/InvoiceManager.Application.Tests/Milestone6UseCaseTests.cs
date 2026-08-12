using InvoiceManager.Application.Common.Time;
using InvoiceManager.Application.Dashboard;
using InvoiceManager.Application.Documents;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Domain.Invoices;

namespace InvoiceManager.Application.Tests;

public sealed class Milestone6UseCaseTests
{
    [Fact]
    public async Task PdfUseCasesForwardIdentifiersAndOutputPaths()
    {
        var generator = new PdfGeneratorStub();
        var invoiceId = Guid.NewGuid();
        var quoteId = Guid.NewGuid();

        await new GenerateInvoicePdf(generator).ExecuteAsync(invoiceId, "invoice.pdf");
        await new GenerateQuotePdf(generator).ExecuteAsync(quoteId, "quote.pdf");

        Assert.Equal((invoiceId, "invoice.pdf"), generator.InvoiceRequest);
        Assert.Equal((quoteId, "quote.pdf"), generator.QuoteRequest);
    }

    [Theory]
    [InlineData(true, "")]
    [InlineData(false, "document.pdf")]
    public async Task GenerateInvoicePdfRejectsInvalidInput(bool validId, string path)
    {
        var useCase = new GenerateInvoicePdf(new PdfGeneratorStub());
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(validId ? Guid.NewGuid() : Guid.Empty, path));
    }

    [Fact]
    public async Task DashboardRefreshesStatusesBeforeQueryingSummary()
    {
        var events = new List<string>();
        var expected = new DashboardSummary(10m, 20m, 30m, 5m, 2, 3, [], []);
        var clock = new FixedClock(new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero));
        var useCase = new GetDashboardSummary(new DashboardQueryStub(expected, events), new InvoiceRepositoryStub(events), clock);

        var result = await useCase.ExecuteAsync();

        Assert.Same(expected, result);
        Assert.Equal(["refresh", "query"], events);
    }

    private sealed class PdfGeneratorStub : IDocumentPdfGenerator
    {
        public (Guid, string)? InvoiceRequest { get; private set; }
        public (Guid, string)? QuoteRequest { get; private set; }
        public Task GenerateInvoiceAsync(Guid invoiceId, string outputPath, CancellationToken cancellationToken)
        {
            InvoiceRequest = (invoiceId, outputPath);
            return Task.CompletedTask;
        }
        public Task GenerateQuoteAsync(Guid quoteId, string outputPath, CancellationToken cancellationToken)
        {
            QuoteRequest = (quoteId, outputPath);
            return Task.CompletedTask;
        }
    }

    private sealed class DashboardQueryStub(DashboardSummary summary, List<string> events) : IDashboardQuery
    {
        public Task<DashboardSummary> GetAsync(DateOnly today, CancellationToken cancellationToken)
        {
            events.Add("query");
            return Task.FromResult(summary);
        }
    }

    private sealed class InvoiceRepositoryStub(List<string> events) : IInvoiceRepository
    {
        public Task AddAsync(Invoice invoice, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<Invoice>> SearchAsync(string? searchTerm, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RefreshStatusesAsync(DateOnly today, DateTimeOffset utcNow, CancellationToken cancellationToken)
        {
            events.Add("refresh");
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IApplicationClock
    {
        public DateTimeOffset UtcNow => now;
        public DateOnly Today => DateOnly.FromDateTime(now.UtcDateTime);
    }
}
