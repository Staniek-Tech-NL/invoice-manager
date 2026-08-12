# ADR-0009: Use PDFsharp for local PDF generation

Status: Accepted

## Context

Milestone 6 requires professional quotation and invoice PDFs generated offline from persisted document snapshots. The implementation must remain behind an Application interface, support tables and company logos, work with .NET 10 on Windows, and avoid a dependency whose use changes with business revenue or deployment scale.

## Decision

Use PDFsharp 6.2.4 in `InvoiceManager.Infrastructure` behind `IDocumentPdfGenerator`.

The generator reads persisted quotation or invoice aggregates, never current customer, catalog, or company records. It renders A4 documents with issuer and customer details, dates, items, VAT, totals, banking information, payment state where applicable, notes, and page footers. PDF files are written only to a user-selected path.

PDFsharp's Windows font resolver is enabled because Invoice Manager is a Windows-only WPF application. Logo bytes are copied into the issuer snapshot when a document is created and stored in SQLite, so later logo changes or missing source files do not alter historical output.

## Consequences

- PDF generation remains replaceable through the Application port.
- Generation is fully local and does not transmit business data.
- PDFsharp's MIT license is compatible with unrestricted application distribution.
- The Infrastructure implementation is intentionally Windows-oriented for font resolution.
- Existing documents without stored logo bytes remain exportable without a logo.
- Invalid legacy logo content is ignored during rendering rather than blocking the complete document export.
