# Domain Model

## Model Overview

```text
CompanySettings (single profile)

Customer 1 ---- * Quote 1 ---- * QuoteItem
Customer 1 ---- * Invoice 1 -- * InvoiceItem
                              1 -- * Payment

ProductService supplies editable source data for new document items.
DocumentNumberSequence allocates numbers per type and year.
Documents retain customer and item snapshots independently of source records.
```

## Entities

### CompanySettings

Stores the single issuing business profile: company name, address, country, VAT number, chamber of commerce number, IBAN, email, phone, default VAT rate, default payment term, currency, and logo path.

### Customer

Stores company and contact information, VAT number, notes, archive state, and audit timestamps. Customers are archived, not physically deleted.

### ProductService

Stores name, description, unit, unit price, VAT rate, active state, and audit timestamps. Typical units are `hour`, `day`, `item`, and `service`. Entries are deactivated, not physically deleted.

### Quote and QuoteItem

A quote contains a unique number, customer association and snapshot, issue and validity dates, status, notes, totals, timestamps, and one or more item snapshots.

Quote statuses:

```text
Draft -> Sent -> Accepted
              -> Rejected
Draft/Sent -> Expired when validity rules apply
```

### Invoice and InvoiceItem

An invoice contains a unique number, customer association and snapshot, issue and due dates, status, notes, totals, timestamps, and one or more item snapshots.

Invoice statuses are `Draft`, `Sent`, `Paid`, `Overdue`, and `Cancelled`. Paid and Overdue are derived from payment and due-date rules rather than arbitrary user toggles.

### Payment

A payment is an immutable recorded transaction associated with one invoice. It stores payment date, amount, reference, method, and creation timestamp. Corrections must preserve a traceable history; the exact correction workflow will be designed with the payment feature.

### DocumentNumberSequence

Tracks the last number for one `DocumentType + Year` pair. Invoice and quotation sequences are independent.

## Value Concepts

Implementation should introduce value objects where they protect invariants without unnecessary complexity. Likely candidates include:

- Money or monetary amount with currency context
- VAT rate
- Document number
- Address
- Date range or payment term

The MVP has one currency, EUR, but money calculations must still be explicit and centralized.

## Relationships and Ownership

- A customer may have many quotes and invoices.
- A quote owns its quote items.
- An invoice owns its invoice items and payments.
- Deleting a document item independently of its owning aggregate is not a supported business action.
- Company settings exist as a single application profile.
- Source catalog entries are not required for a historical item snapshot to remain valid.

## Document Snapshots

Documents must preserve what was issued. Quote and invoice items therefore store description, quantity, unit, unit price, VAT rate, net amount, VAT amount, and gross amount. Documents also store the customer details needed to render the original document.

Changing a customer address, catalog price, description, or VAT rate affects new documents only. Existing documents retain their stored snapshot.

## Aggregate Invariants

- Quantity and price inputs must produce deterministic decimal totals.
- Stored totals must agree with the centralized calculation policy.
- A payment amount must be positive and cannot exceed the outstanding amount.
- Cancelled invoices cannot become overdue.
- A fully paid invoice has zero outstanding balance and Paid status.
- Document numbers are assigned once and never reused.
- Conversion does not mutate or remove the source quotation.

## Lifecycle Notes

Detailed transition permissions, edit restrictions after sending, cancellation effects, payment correction behavior, and automatic quote expiration timing must be finalized during their respective milestones. Any new behavior must remain consistent with [Business Rules](business-rules.md) and be recorded in an ADR if it changes an accepted architectural decision.
