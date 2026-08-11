# ADR-0008: Void Incorrect Payments Without Deleting History

Status: Accepted

## Context

Recorded payments affect outstanding balances and invoice status. Users can enter a wrong amount, date, method, or reference, but editing or deleting that transaction would remove evidence of what was originally recorded. A full accounting reversal ledger would add complexity beyond the local MVP.

## Decision

Payment details are immutable after registration. An incorrect payment can be voided exactly once with a required reason. The original amount and metadata remain stored together with the UTC void timestamp and reason.

Voided payments are excluded from paid and outstanding amount calculations. Voiding recalculates invoice status using the normal priority rules and may move a Paid invoice back to Sent or Overdue. Physical deletion and in-place editing of payments are not supported.

## Consequences

- Incorrect entries can be corrected without losing audit history.
- Balances and derived statuses remain consistent with active payments.
- The UI must display voided entries and require a reason before confirmation.
- Registration and voiding require transactional persistence and concurrency protection.
- A future accounting-grade reversal ledger may supersede this decision if product scope expands.
