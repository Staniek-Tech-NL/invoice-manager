# Roadmap

## Release Goal

The target is a polished, tested, documented Windows desktop release: `v1.0.0`.

## M1 — Foundation (`0.1.0`) — Complete

- Create solution and production/test projects
- Configure project dependencies and centralized build settings
- Add Generic Host, dependency injection, and logging
- Add EF Core, SQLite, local application directories, and initial migration
- Build the WPF shell and base navigation
- Add GitHub Actions, issue/PR templates, and documentation foundation
- Verify restore, build, and test from a clean clone

Exit criterion: achieved. The application launches, navigation works, persistence is initialized through the first migration, build and tests pass, CI is configured, and setup is documented.

## M2 — Customers and Services (`0.2.0`) — Complete

- Customer create, view, edit, archive, and search
- Product/service create, edit, deactivate, and search
- Validation and persistence tests

Exit criterion: achieved. Reusable business records can be safely maintained, validation is enforced, and inactive records are filtered correctly unless explicitly requested.

## M3 — Quotations (`0.3.0`) — Complete

- Quote editor and item management
- VAT and total calculations with documented rounding
- Quote statuses and yearly numbering
- Snapshot persistence

Exit criterion: achieved. A quotation can be created, numbered on first save, updated while in draft, searched, expired automatically, and moved through its supported lifecycle without calculation or snapshot drift.

## M4 — Invoices (`0.4.0`) — Complete

- Invoice editor, calculations, statuses, and numbering
- Accepted quote-to-invoice conversion
- Transaction and snapshot tests

Exit criterion: achieved. Invoices work independently, use safe yearly numbering and snapshots, and can be created exactly once from an accepted quotation in one transaction.

## M5 — Payments (`0.5.0`) — Complete

- Recorded payments
- Partial and full payment handling
- Outstanding balance and Paid state
- Overpayment protection
- Automatic Overdue logic
- Auditable payment voiding without deleting history

Exit criterion: achieved. Partial, full, excessive, concurrent, overdue, and voided-payment scenarios preserve correct balances, statuses, transactions, and audit history.

## M6 — PDF and Dashboard (`0.6.0`) — Complete

- Select and document the PDF library
- Generate quote and invoice PDFs
- Dashboard KPIs, recent invoices, and monthly revenue chart
- Persist historical logo bytes with each document snapshot
- Verify PDF layout visually and cover PDF/dashboard behavior with automated tests

Exit criterion: achieved. Users can export professional snapshot-safe documents and see accurate payment-based business summaries.

Exit criterion: users can export professional documents and see accurate business summaries.

## M7 — Portfolio Release (`1.0.0`)

- Complete testing and manual release verification
- Polish UI, accessibility, validation, and empty/error states
- Add demo data and final screenshots
- Complete README, diagrams, case study, and changelog
- Produce and verify the release build
- Publish GitHub release

Exit criterion: the Definition of Done for the MVP is satisfied.

## Suggested Public Releases

- `v0.1.0` — foundation
- `v0.5.0` — complete core business workflow through payments
- `v1.0.0` — polished portfolio release

## Future Ideas

Items outside the MVP may be reconsidered only after `v1.0.0`: recurring invoices, multiple currencies, cloud synchronization, accounting integrations, email delivery, payment providers, multi-business support, or multiple users. Each requires separate product and architecture evaluation.

## MVP Definition of Done

- The complete configure-to-payment workflow works.
- Overdue and partial payment rules are correct.
- Historical document snapshots remain stable.
- Data survives restart and migrations are valid.
- PDF output is complete and visually verified.
- Automated tests and CI pass.
- A clean release build launches on supported Windows.
- README, screenshots, engineering decisions, and case study are complete.
