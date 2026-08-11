# ADR-0006: Use Recorded Payments Instead of a Paid Checkbox

Status: Accepted

## Context

A boolean Paid flag cannot explain partial payment, payment dates, methods, references, or outstanding amounts. It also allows state to disagree with the actual money received.

## Decision

Model each payment as a separate transaction linked to an invoice. Calculate outstanding amount as invoice total minus recorded payments. Reject payments greater than the outstanding balance and derive Paid status when the balance reaches zero.

## Consequences

- Partial and multiple payments are supported naturally.
- Payment history is visible and auditable.
- Paid state remains consistent with recorded amounts.
- Overdue logic can account for partial payments.
- Payment corrections and reversals require an explicit traceable policy, to be designed with the payment milestone.
