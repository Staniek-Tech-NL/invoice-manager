# ADR-0005: Preserve Document Snapshots

Status: Accepted

## Context

Company settings, customers, and catalog entries change over time. If a historical quotation or invoice dynamically reads current source data, previously issued documents can silently change, undermining auditability and customer trust.

## Decision

Persist issuer details, customer details, and all line-item commercial values as document snapshots.

At minimum, the issuer snapshot contains company name, street, postal code, city, country, VAT number, chamber of commerce number, IBAN, email, and phone. An item snapshot contains description, quantity, unit, unit price, VAT rate, net amount, VAT amount, and gross amount.

PDF generation and historical views use persisted snapshot data, not current `CompanySettings`, customer, or catalog values. Historical logo preservation is resolved with the PDF implementation in M6 without weakening the accepted textual snapshot rule.

## Consequences

- Historical documents remain stable after company, customer, or catalog edits, archival, or deactivation.
- Quote-to-invoice conversion can copy a well-defined commercial snapshot.
- Some data is intentionally duplicated.
- Snapshot mappings and migrations are broader than simple foreign-key-only models.
- Corrections to issued documents require an explicit business workflow rather than modifying source data.
