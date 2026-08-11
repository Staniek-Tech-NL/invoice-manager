# Project Overview

## Problem

Freelancers and small businesses often need more structure than a document template or spreadsheet provides, but do not need the complexity of a complete accounting platform. They need a reliable local tool that keeps customer data, services, quotations, invoices, and payments connected while preserving a clear document history.

## Target Users

The primary users are freelancers and small businesses that:

- create quotations and invoices regularly;
- work primarily in EUR;
- need partial and full payment tracking;
- prefer a focused Windows desktop application;
- do not require multi-user accounting or cloud synchronization.

## Product Goal

Invoice Manager provides a professional end-to-end document workflow:

```text
Customer -> Quote -> Accepted quote -> Invoice -> PDF -> Payment -> Reporting
```

The product should reduce repetitive data entry, prevent inconsistent calculations, preserve historical documents, and make outstanding revenue visible.

## MVP

The first public release includes:

- one company profile;
- customer management;
- product and service management;
- quotation creation and lifecycle management;
- quotation-to-invoice conversion;
- invoice creation and lifecycle management;
- recorded partial and full payments;
- automatic outstanding, paid, and overdue calculations;
- invoice and quote PDF generation;
- dashboard metrics and recent invoices;
- local SQLite storage;
- EUR as the only currency.

## Product Principles

- Focused: solve invoicing and quotation workflows without becoming an ERP.
- Reliable: calculations and historical documents must remain stable.
- Understandable: common actions and statuses should be obvious to business users.
- Local-first: core workflows work without a cloud account or external service.
- Portfolio-ready: architecture, tests, decisions, CI, and releases are visible and documented.

## Non-Goals for v1.0

- Authentication, multiple users, or multiple businesses
- Cloud synchronization, web, or mobile clients
- Bank APIs or automatic reconciliation
- Stripe, PayPal, or other online payment processing
- Recurring invoices or multiple currencies
- Accounting integrations, bookkeeping, tax filing, or inventory
- Built-in email delivery

## Success Criteria

The MVP is successful when the complete workflow works from a clean installation, data survives restart, document PDFs are correct, partial and full payments update status accurately, overdue invoices are detected, automated tests and CI pass, and a downloadable release build can be launched on Windows.
