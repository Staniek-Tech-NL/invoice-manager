# ADR-0005: Preserve Document Snapshots

Status: Accepted

## Context

Customers and catalog entries change over time. If a historical quotation or invoice dynamically reads current source data, previously issued documents can silently change, undermining auditability and customer trust.

## Decision

Persist the customer details required for rendering and all line-item commercial values as document snapshots. At minimum, an item snapshot contains description, quantity, unit, unit price, VAT rate, net amount, VAT amount, and gross amount.

PDF generation and historical views use persisted snapshot data, not current customer or catalog values.

## Consequences

- Historical documents remain stable after source edits, archival, or deactivation.
- Quote-to-invoice conversion can copy a well-defined commercial snapshot.
- Some data is intentionally duplicated.
- Snapshot mappings and migrations are broader than simple foreign-key-only models.
- Corrections to issued documents require an explicit business workflow rather than modifying source data.
