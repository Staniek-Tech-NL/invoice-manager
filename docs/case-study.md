# Invoice Manager Case Study

> Status: living draft. This document will be updated with implementation evidence, screenshots, metrics, and lessons as milestones are completed.

## Problem

Small businesses need a dependable quotation-to-payment workflow without the operational overhead of a full accounting suite. Spreadsheet and document-template approaches duplicate data, make status tracking difficult, and can silently change historical output when source information is edited.

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

## Testing Strategy

The project currently has 68 passing automated tests: 32 domain tests, 19 application tests, and 17 infrastructure tests. Coverage now includes reusable-record validation, quotation and invoice calculations, lifecycle rules, snapshot stability, independent first-save and concurrent numbering, transactional quote conversion, rollback behavior, SQLite persistence, filtering, search escaping, and automatic expiration. Clean-clone verification restores, builds, and tests the committed repository independently. Future milestones will add payment, overdue-state, and PDF coverage.

## Result

Target result: a downloadable `v1.0.0` Windows application with a complete customer-to-payment workflow, stable persistence, professional PDFs, dashboard reporting, CI, and portfolio-quality documentation.

## Lessons Learned

- Archival and deactivation are domain actions rather than UI-only flags, which keeps lifecycle rules consistent across all callers.
- Small application use cases and repository ports keep WPF and EF Core details outside the domain model.
- Search behavior needs integration tests because SQLite wildcard semantics differ from ordinary string matching.
- First-save numbering belongs in the same persistence transaction as the document; opening or abandoning an editor must never consume a number.
- Persisted line and party snapshots make historical stability an explicit aggregate boundary rather than a rendering concern.
- A unique source-quote link complements application validation and makes repeat conversion impossible even under competing callers.
- Updating the case study at each milestone keeps implementation evidence accurate instead of reconstructing it only for the final release.
