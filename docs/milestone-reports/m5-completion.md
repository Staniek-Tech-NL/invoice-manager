# Milestone 5 Completion Report

**Project:** Invoice Manager
**Milestone:** M5 — Payments
**Completion date:** 2026-08-11
**Next milestone:** M6 — PDF and Dashboard

## Outcome

Milestone 5 is complete. Invoices support recorded partial and full payments, paid and outstanding amounts, overpayment protection, deterministic Paid and Overdue status, concurrent registration protection, and auditable payment voiding.

## Delivered Scope

- Immutable payment records with date, amount, method, reference, and UTC creation timestamp.
- Transactional payment registration for Sent and Overdue invoices.
- Partial and full payment support.
- `PaidAmount` and `OutstandingAmount` derived from active recorded payments.
- Overpayment rejection without persisted changes.
- Paid status when outstanding reaches zero.
- Overdue status for active past-due balances, including partially paid invoices.
- Draft and Cancelled status priority.
- Cancellation rejection when an invoice has an active payment.
- One-time payment voiding with required reason and UTC timestamp.
- Retained voided-payment history excluded from financial totals.
- Status restoration to Sent or Overdue after voiding.
- WPF payment history, registration, balance display, and voiding controls.
- ADR-0008 documenting the correction policy.
- EF Core migration adding `VoidedAt` and `VoidReason`.

## Quality Gate

- Domain tests: **42 passed / 0 failed**
- Application tests: **22 passed / 0 failed**
- Infrastructure tests: **22 passed / 0 failed**
- **Total: 86 passed / 0 failed**
- Release build: **0 errors / 0 warnings**
- Pending EF Core model changes: **0**
- NuGet vulnerability audit: **0 known vulnerable dependencies**
- Broken local documentation links: **0**
- Clean-clone restore, build, and test: **passed**

## Database State

The repository contains three migrations after M5:

1. `InitialCreate`
2. `AddInvoiceQuoteLink`
3. `AddPaymentVoiding`

## Remaining MVP Scope

- M6: quotation and invoice PDF generation, dashboard KPIs, recent invoices, and monthly revenue reporting.
- M7: final UX, accessibility, demo data, screenshots, release packaging, and portfolio polish.
