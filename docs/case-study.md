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

Documents store customer and line-item snapshots so later changes to addresses, descriptions, prices, or VAT rates affect only future documents.

### Safe numbering

Quotation and invoice numbers use separate yearly sequences. A number is allocated atomically on first save, avoiding gaps caused by abandoned editors and collisions caused by concurrent saves.

### Recorded payments

Payments are transactions, not a checkbox. This supports partial payment, calculates the exact outstanding amount, prevents overpayment, and derives the Paid state.

### Time-dependent status

Overdue status depends on due date, outstanding balance, and lifecycle status. An injected clock keeps this behavior deterministic in tests.

## Key Engineering Decisions

Accepted decisions are recorded in [Architecture Decision Records](decisions/README.md): clean layered architecture, WPF with MVVM, SQLite with EF Core, first-save numbering, document snapshots, and recorded payments.

## Implementation

Implementation has not started. This section will record milestone outcomes, significant trade-offs, and links to representative code and pull requests.

## Testing Strategy

The project emphasizes domain tests for calculations and state, application tests for workflows, and SQLite integration tests for mappings, constraints, transactions, and migrations. Release verification adds an end-to-end manual workflow and visual PDF review.

## Result

Target result: a downloadable `v1.0.0` Windows application with a complete customer-to-payment workflow, stable persistence, professional PDFs, dashboard reporting, CI, and portfolio-quality documentation.

## Lessons Learned

To be completed throughout development rather than reconstructed after release.
