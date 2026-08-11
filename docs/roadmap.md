# Roadmap

## Release Goal

The target is a polished, tested, documented Windows desktop release: `v1.0.0`.

## M1 — Foundation (`0.1.0`)

- Create solution and production/test projects
- Configure project dependencies and centralized build settings
- Add Generic Host, dependency injection, and logging
- Add EF Core, SQLite, local application directories, and initial migration
- Build the WPF shell and base navigation
- Add GitHub Actions, issue/PR templates, and documentation foundation
- Verify restore, build, and test from a clean clone

Exit criterion: the empty application launches, navigation works, persistence is initialized, tests and CI pass, and setup is documented.

## M2 — Customers and Services (`0.2.0`)

- Customer create, view, edit, archive, and search
- Product/service create, edit, deactivate, and search
- Validation and persistence tests

Exit criterion: reusable business records can be safely maintained and inactive records are filtered correctly.

## M3 — Quotations (`0.3.0`)

- Quote editor and item management
- VAT and total calculations with documented rounding
- Quote statuses and yearly numbering
- Snapshot persistence

Exit criterion: a quotation can be created, saved, updated, and moved through its supported lifecycle without calculation or history drift.

## M4 — Invoices (`0.4.0`)

- Invoice editor, calculations, statuses, and numbering
- Accepted quote-to-invoice conversion
- Transaction and snapshot tests

Exit criterion: invoices work independently and can be created reliably from accepted quotations.

## M5 — Payments (`0.5.0`)

- Recorded payments
- Partial and full payment handling
- Outstanding balance and Paid state
- Overpayment protection
- Automatic Overdue logic

Exit criterion: payment and status behavior passes all critical scenarios.

## M6 — PDF and Dashboard (`0.6.0`)

- Select and document the PDF library
- Generate quote and invoice PDFs
- Dashboard KPIs, recent invoices, and monthly revenue chart

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
