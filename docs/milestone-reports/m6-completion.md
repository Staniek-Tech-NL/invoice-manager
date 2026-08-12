# Milestone 6 Completion Report

**Project:** Invoice Manager
**Milestone:** M6 - PDF and Dashboard
**Completion date:** 2026-08-12
**Next milestone:** M7 - Portfolio Release

## Outcome

Milestone 6 is complete. Users can export snapshot-safe quotation and invoice PDFs to a selected location and review payment-based financial reporting on a live dashboard.

## Delivered Scope

- PDFsharp 6.2.4 selected and isolated behind `IDocumentPdfGenerator`.
- Quotation and invoice PDF export from persisted document aggregates.
- A4 layouts with issuer and customer details, dates, item table, VAT, totals, banking information, notes, payment balance where applicable, and page footers.
- User-selected PDF destination with overwrite confirmation.
- Historical logo bytes copied into issuer snapshots and persisted in SQLite.
- Existing documents without logo bytes remain exportable.
- Six dashboard KPIs: month revenue, year revenue, outstanding amount, overdue amount, invoice count, and active customer count.
- Twelve-month payment-revenue chart with zero-filled months.
- Five most recent non-cancelled invoices.
- Automatic invoice-status refresh before dashboard calculation.
- ADR-0009 documenting the PDF implementation and licensing choice.
- EF Core migration adding nullable issuer-logo BLOB columns to quotations and invoices.
- Visual QA of representative invoice and quotation output after PNG rendering.

## Reporting Semantics

- Revenue is the sum of active, non-voided recorded payments by `PaymentDate`.
- Outstanding and overdue values include collectible Sent and Overdue invoices, not Draft, Paid, or Cancelled documents.
- Overdue value includes only the outstanding balance of invoices currently derived as Overdue.
- Invoice count excludes cancelled invoices; customer count includes active customers only.

## Quality Gate

- Domain tests: **43 passed / 0 failed**
- Application tests: **26 passed / 0 failed**
- Infrastructure tests: **26 passed / 0 failed**
- **Total: 95 passed / 0 failed**
- Release build: **0 errors / 0 warnings**
- Pending EF Core model changes: **0**
- NuGet vulnerability audit: **0 known vulnerable dependencies**
- Broken local documentation links: **0**
- Representative PDF visual review: **passed**
- Clean-clone restore, build, and test: **passed**

## Database State

The repository contains four migrations after M6:

1. `InitialCreate`
2. `AddInvoiceQuoteLink`
3. `AddPaymentVoiding`
4. `PreserveDocumentLogo`

## Remaining MVP Scope

- M7: final UX and accessibility review, demo data, screenshots, release packaging, architecture diagrams, and portfolio polish.
