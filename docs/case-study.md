# Invoice Manager — Engineering Case Study

Invoice Manager is a release-ready, local-first Windows application for freelancers and small businesses. It replaces duplicated spreadsheet and document-template work with one coherent quotation-to-payment workflow: reusable customer and service data, snapshot-safe quotes and invoices, recorded payments, reproducible PDFs, and live reporting.

The engineering challenge was not basic CRUD. The product protects historical documents, allocates business numbers transactionally, converts accepted quotations exactly once, records payment corrections without deleting history, and keeps date-dependent status behavior deterministic.

| Release | Platform | Architecture | Persistence |
|---|---|---|---|
| `v1.0.0` | Windows x64 | Clean Architecture + MVVM | EF Core + SQLite |

| Automated tests | Build | Dependency audit | EF Core model |
|---|---|---|---|
| **97 passing** | **0 warnings / 0 errors** | **0 known vulnerable packages** | **No pending migrations** |

> The source, documentation, screenshots, and verified Windows package are published in the [public GitHub repository](https://github.com/Staniek-Tech-NL/invoice-manager).

## Problem

Small businesses need a dependable quotation-to-payment workflow without the operational overhead of a full accounting suite. Spreadsheet and document-template approaches duplicate customer and service data, obscure document state, make partial payments difficult to track, and can silently change historical output when a source address, price, VAT rate, or logo is edited.

## Goals

- Build a focused, professional Windows desktop product.
- Preserve historical business documents.
- Make calculations and payment state reliable and testable.
- Demonstrate the complete engineering lifecycle from requirements through release.

## Requirements

The MVP connects customer and service management to quotations, invoices, PDF output, recorded payments, and dashboard reporting. It supports one company and EUR while deliberately excluding authentication, cloud synchronization, payment providers, and accounting integrations.

## Architecture

The design separates WPF/MVVM presentation, application use cases, domain rules, and infrastructure. Dependencies point inward, and infrastructure implements ports for SQLite persistence, files, logging, and PDF generation.

## Domain Challenges

### Stable document history

Documents store issuer, customer, and line-item snapshots so later changes to company settings, addresses, descriptions, prices, or VAT rates affect only future documents.

### Safe numbering

Quotation and invoice numbers use separate yearly sequences. A number is allocated atomically on first save, avoiding gaps caused by abandoned editors and collisions caused by concurrent saves.

### Recorded payments

Payments are transactions, not a checkbox. This supports partial payment, calculates the exact outstanding amount, prevents overpayment, and derives the Paid state.

### Time-dependent status

Overdue status depends on due date, outstanding balance, and lifecycle status. An injected clock keeps this behavior deterministic in tests.

## Key Engineering Decisions

Accepted decisions are recorded in [Architecture Decision Records](decisions/README.md): clean layered architecture, WPF with MVVM, SQLite with EF Core, first-save numbering, document snapshots, and recorded payments.

## Implementation

Milestone 1 established the .NET 10 solution, layered project structure, Generic Host, dependency injection, logging, SQLite persistence, initial EF Core migration, WPF navigation shell, CI workflow, and automated test foundation.

Milestone 2 delivered customer and product/service maintenance across all application layers. Users can create, view, edit, and search reusable records. Customers are archived and catalog entries are deactivated instead of being physically deleted, preserving their future relationship with historical documents. Domain validation protects required fields, email format, non-negative prices, and VAT ranges. Repository queries hide inactive records by default while allowing them to be included explicitly.

The desktop UI provides dedicated customer and product/service lists and editors, confirmation before archival or deactivation, validation feedback, search, and inactive-record filters. Application use cases remain independent of WPF, while EF Core repositories own SQLite access.

Milestone 3 introduced the company profile required for issuer snapshots and delivered the complete quotation workflow. The editor selects active customers and catalog entries, supports editable line snapshots, calculates totals through centralized domain rules, and allows only valid Draft → Sent → Accepted or Rejected transitions. Eligible draft and sent quotations expire automatically. Numbers such as `Q-2026-0001` are allocated during the first transactional save, with a concurrency test protecting uniqueness and ordering.

Milestone 4 added independent invoice creation and one-time conversion from accepted quotations. Converted invoices retain a unique `SourceQuoteId`, clone all historical snapshots, receive their own `INV-YYYY-NNNN` number, and leave the source quotation unchanged. Conversion and sequence allocation share one transaction, so a failed or repeated attempt neither creates partial data nor consumes a number. The WPF invoice editor supports Draft, Sent, and Cancelled lifecycle actions; payment-derived states remain reserved for Milestone 5.

Milestone 5 delivered transactional partial and full payments, outstanding balances, overpayment protection, and deterministic Paid and Overdue derivation. Concurrent registration is serialized within the desktop application so competing payments cannot exceed the balance. Incorrect entries are never edited or deleted: ADR-0008 defines one-time voiding with a reason and UTC timestamp, preserving the original record while immediately recalculating balance and status.

Milestone 6 completed the operational workflow with local quotation and invoice PDF export plus live reporting. PDFsharp renders persisted document snapshots, including a logo copied into SQLite at document creation, so historical exports are reproducible after company or customer edits. The dashboard derives month and year revenue from active recorded payments and combines it with current outstanding and overdue balances, active record counts, recent invoices, and a zero-filled 12-month chart.

Milestone 7 turned the completed workflow into a release-ready portfolio project. A transactional demo-data seeder creates a coherent fictional dataset only when the business database is empty. The desktop experience adds keyboard shortcuts, automation labels, empty states, explicit logo controls, and visible version information. An isolated capture mode produces reviewed product screenshots and a representative PDF without touching user data. A guarded release script builds a self-contained Windows x64 package, while an editable nine-slide engineering case study combines real product evidence with restrained workflow and architecture diagrams.

## Testing Strategy

The project has 97 passing automated tests: 43 domain tests, 26 application tests, and 28 infrastructure tests. Coverage includes reusable records, calculations and lifecycles, snapshots and historical logos, numbering, quote conversion, payments and voiding, concurrency, safe demo seeding, PDF generation, dashboard summaries, SQLite persistence, filtering, paths, and expiration. Representative PDFs and product screens are rendered and reviewed visually. Release verification restores, builds, tests, audits dependencies, checks the EF model, packages the app, and launches the self-contained executable against isolated data.

## Result

The result is a self-contained `v1.0.0` Windows application with a complete customer-to-payment workflow, stable local persistence, professional snapshot-safe PDFs, dashboard reporting, CI configuration, 97 automated tests, safe demo data, and a portfolio-quality visual and documentation kit. The source and verified release artifact are publicly available on GitHub.

## Lessons Learned

- Archival and deactivation are domain actions rather than UI-only flags, which keeps lifecycle rules consistent across all callers.
- Small application use cases and repository ports keep WPF and EF Core details outside the domain model.
- Search behavior needs integration tests because SQLite wildcard semantics differ from ordinary string matching.
- First-save numbering belongs in the same persistence transaction as the document; opening or abandoning an editor must never consume a number.
- Persisted line and party snapshots make historical stability an explicit aggregate boundary rather than a rendering concern.
- A unique source-quote link complements application validation and makes repeat conversion impossible even under competing callers.
- Immutable payment records with explicit void metadata offer practical auditability without introducing a full accounting reversal ledger into the MVP.
- Updating the case study at each milestone keeps implementation evidence accurate instead of reconstructing it only for the final release.
- Demo data must be treated as a guarded product capability: refusing non-empty databases makes evaluation convenient without risking real records.
- Portfolio assets are more credible when generated from the real application and fictional data, then visually inspected like any other release output.
