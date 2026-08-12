# Domain Model

## Model Overview

```text
CompanySettings (single profile)

Customer 1 ---- * Quote 1 ---- * QuoteItem
Customer 1 ---- * Invoice 1 -- * InvoiceItem
                              1 -- * Payment

ProductService supplies editable source data for new document items.
DocumentNumberSequence allocates numbers per type and year.
Documents retain issuer, customer, and item snapshots independently of source records.
```

## Entities

### CompanySettings

Stores the single issuing business profile: company name, address, country, VAT number, chamber of commerce number, IBAN, email, phone, default VAT rate, default payment term, currency, and logo path.

### Customer

Stores company and contact information, VAT number, notes, archive state, and audit timestamps. Creation and updates enforce required address fields and valid optional email data. Customers are archived, not physically deleted; archiving is idempotent.

### ProductService

Stores name, description, unit, unit price, VAT rate, active state, and audit timestamps. Creation and updates require a name and unit, reject negative prices, and constrain VAT to a decimal fraction from zero to one. Typical units are `hour`, `day`, `item`, and `service`. Entries are deactivated, not physically deleted; deactivation is idempotent.

### Quote and QuoteItem

A quote contains a unique number, issuer snapshot, customer association and snapshot, issue and validity dates, status, notes, totals, timestamps, and one or more item snapshots.

A quotation is created as a draft with at least one valid item. Its number is allocated atomically on first persistence using the independent yearly quote sequence. Only drafts can be edited. Eligible draft and sent quotations expire after their validity date; accepted and rejected quotations remain terminal. Customer and issuer snapshots are captured on creation and do not drift when source records are edited later.

Quote statuses:

```text
Draft -> Sent -> Accepted
              -> Rejected
Draft/Sent -> Expired when validity rules apply
```

Line net, VAT, and gross amounts use the centralized two-decimal, midpoint-away-from-zero rounding policy. Quote totals are derived from the persisted rounded line amounts.

### Invoice and InvoiceItem

An invoice contains a unique number, issuer snapshot, customer association and snapshot, issue and due dates, status, notes, totals, timestamps, and one or more item snapshots.

An invoice is created as a draft with at least one valid item. Only drafts can be edited or marked as sent. Draft and sent invoices can be cancelled when they have no active payments. Paid and Overdue are derived states, never manual actions. Invoice numbers use an independent yearly sequence and are assigned atomically on first persistence.

An invoice converted from a quotation stores a unique optional `SourceQuoteId`. Conversion requires an Accepted quotation, copies issuer, customer, line, and notes snapshots, and preserves the source quotation. The database unique constraint allows each quote to be converted at most once. Conversion, invoice-number allocation, item persistence, and sequence advancement share one transaction; a failed or repeated conversion does not consume a number.

Invoice statuses are `Draft`, `Sent`, `Paid`, `Overdue`, and `Cancelled`. Paid and Overdue are derived from payment and due-date rules rather than arbitrary user toggles.

### Payment

A payment is an immutable recorded transaction associated with one invoice. It stores payment date, amount, reference, method, and creation timestamp. Its amount, date, reference, and method cannot be edited after registration.

Incorrect payments are voided rather than changed or deleted. Voiding requires a non-empty reason, records a UTC timestamp, and can occur only once. Voided payments remain visible in history but no longer contribute to `PaidAmount` or `OutstandingAmount`; the owning invoice immediately recalculates its derived status.

The invoice aggregate owns its payments and derives `PaidAmount`, `OutstandingAmount`, and payment-dependent status. Draft and Cancelled have priority, zero outstanding produces Paid, an active past-due balance produces Overdue, and other active invoices remain Sent. See [ADR-0008](decisions/0008-void-incorrect-payments.md).

### DocumentNumberSequence

Tracks the last number for one `DocumentType + Year` pair. Invoice and quotation sequences are independent.

## Domain Concepts and Value Objects

- `IssuerSnapshot` owns the issuing company data and optional logo bytes copied into a document. It prevents historical output from reading mutable company settings.
- `CustomerSnapshot` owns the customer identity and address copied into a document. It prevents later customer edits from changing issued content.
- `QuoteItem` and `InvoiceItem` are owned line snapshots. They retain description, quantity, unit, unit price, VAT rate, and rounded monetary results.
- `QuoteItemDraft` and `InvoiceItemDraft` carry validated editor inputs into aggregate creation and replacement operations.
- `DocumentNumberSequence` protects monotonic allocation for one document type and year. An allocated aggregate number is immutable.
- `FinancialRules` centralizes decimal line and document calculations, two-decimal rounding, and `MidpointRounding.AwayFromZero`.
- `DateOnly` represents issue, validity, due, and payment dates; UTC `DateTimeOffset` represents audit instants.

The MVP uses EUR as its single currency. Monetary values remain explicit `decimal` amounts and never use binary floating-point types.

## Relationships and Ownership

- A customer may have many quotes and invoices.
- A quote owns its quote items.
- An invoice owns its invoice items and payments.
- Deleting a document item independently of its owning aggregate is not a supported business action.
- Company settings exist as a single application profile.
- Source catalog entries are not required for a historical item snapshot to remain valid.

## Document Snapshots

Documents preserve what was issued. Each quote and invoice stores:

- an issuer snapshot containing company name, address, country, VAT number, chamber of commerce number, IBAN, email, and phone;
- the customer details needed to render the original document;
- item snapshots containing description, quantity, unit, unit price, VAT rate, net amount, VAT amount, and gross amount.

EF Core maps `IssuerSnapshot` and `CustomerSnapshot` as owned document data, while line snapshots are owned aggregate collections. Historical rendering reads these persisted snapshots and never substitutes current `CompanySettings`, customer, or catalog records.

Changing company settings, a customer address, catalog price, description, VAT rate, or logo affects new documents only. Existing documents retain their stored snapshots. Since M6, `IssuerSnapshot` also stores optional logo bytes; pre-M6 documents keep a null logo and remain exportable.

## Date and Time Model

- `IssueDate`, `DueDate`, `ValidUntil`, and `PaymentDate` are business dates modeled as `DateOnly` values. They have no time or time zone.
- `CreatedAt`, `UpdatedAt`, and `Payment.CreatedAt` are audit instants modeled as UTC `DateTimeOffset` values with offset zero.
- The application obtains UTC time and the current local business date through an injected clock.
- Audit timestamps are converted to the operating system's local time zone for display only; business dates are displayed without conversion.

## Aggregate Invariants

- Quantity and price inputs must produce deterministic decimal totals.
- Stored totals must agree with the centralized calculation policy.
- A payment amount must be positive and cannot exceed the outstanding amount.
- Cancelled invoices cannot become overdue.
- A fully paid invoice has zero outstanding balance and Paid status.
- Document numbers are assigned once and never reused.
- Accepted quotes can be converted at most once; conversion does not mutate or remove the source quotation.
- A failed or repeated conversion does not consume an invoice number.
- Active payment totals cannot exceed the invoice total.
- Payment corrections preserve the original record and complete void metadata.

## Lifecycle Enforcement

- Quotes are editable only in Draft. Draft moves to Sent; Sent moves to Accepted or Rejected. Draft and Sent expire when `ValidUntil < clock.Today`; Accepted and Rejected are terminal.
- Invoices are editable only in Draft. Draft moves to Sent. Draft or Sent may be cancelled only without active payments.
- Paid and Overdue are recalculated from active payments, due date, and `clock.Today`; Cancelled and Draft take priority.
- Payment registration rejects non-positive and excessive amounts. Voiding is the only supported correction.
- Repository transactions enforce atomic numbering, one-time conversion, and competing-payment protection at persistence boundaries.

These rules match [Business Rules](business-rules.md); architectural changes require an updated or superseding ADR.
