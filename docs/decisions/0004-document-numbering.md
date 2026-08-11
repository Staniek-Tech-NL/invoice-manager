# ADR-0004: Assign Document Numbers on First Save

Status: Accepted

## Context

Allocating a number when an editor opens creates gaps when the user cancels. Generating a number only in memory risks duplicates when multiple saves occur. Quotation and invoice numbers also require independent yearly sequences.

## Decision

Allocate a document number during the first persistent save. Use `DocumentNumberSequence` keyed uniquely by document type and year. Persist sequence update and document creation atomically.

Formats are:

```text
INV-YYYY-NNNN
Q-YYYY-NNNN
```

Once assigned, a number is immutable and never reused.

## Consequences

- Abandoned, unsaved editors do not consume numbers.
- The UI must represent a not-yet-assigned number for new documents.
- Allocation requires transaction and uniqueness protection.
- Concurrency behavior must be verified against SQLite.
- Failed transactions do not leave a persisted document without a valid number.
