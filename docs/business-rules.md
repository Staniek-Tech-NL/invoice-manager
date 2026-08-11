# Business Rules

## Financial Calculations

All monetary calculations use C# `decimal`. `float` and `double` are prohibited for business amounts.

For each line:

```text
Net   = Quantity * UnitPrice
VAT   = Net * VatRate
Gross = Net + VAT
```

Quotation calculations use this centralized rounding policy:

- `Net` is rounded to two decimal places after multiplying quantity by unit price.
- `VAT` is calculated from the rounded net amount and rounded to two decimal places.
- `Gross` is the rounded sum of the line net and VAT amounts.
- Document subtotal, VAT total, and total are sums of their rounded line amounts, rounded to two decimal places.
- Midpoints are rounded away from zero.

Calculations must not be distributed across view models, repositories, and PDF templates. Invoice calculations must reuse the same policy unless a later documented legal requirement supersedes it.

## Outstanding Balance

```text
OutstandingAmount = Invoice.Total - Sum(ActiveRecordedPayments)
```

- Payments must be positive.
- A payment greater than the outstanding balance is rejected.
- Partial payments reduce the outstanding amount but do not set Paid.
- When outstanding reaches exactly zero, the invoice becomes Paid.
- The MVP does not support credit balances or overpayment.
- Voided payments remain in history but are excluded from paid and outstanding amount calculations.

## Payment Corrections

- Payment amount, date, reference, and method are immutable after registration.
- An incorrect payment is voided instead of edited or deleted.
- Voiding requires a non-empty reason and records a UTC timestamp.
- A payment can be voided only once.
- Voiding recalculates the invoice balance and status immediately.
- A Paid invoice may return to Sent or Overdue after a payment is voided.
- An invoice with an active payment cannot be cancelled.

## Invoice Status

An invoice is overdue when all conditions are true:

```text
DueDate < Today
OutstandingAmount > 0
Status is not Draft
Status is not Cancelled
```

A partially paid invoice can therefore be overdue. A fully paid invoice is Paid. Date evaluation should use a single injected clock or date provider so tests are deterministic.

The transition priority is:

1. Cancelled invoices remain Cancelled.
2. Draft invoices remain Draft until explicitly sent.
3. Zero outstanding balance results in Paid.
4. An eligible past-due invoice results in Overdue.
5. Otherwise, a non-draft active invoice remains Sent.

## Document Numbering

Numbers use independent yearly sequences:

```text
Invoices: INV-YYYY-NNNN
Quotes:   Q-YYYY-NNNN
```

Examples: `INV-2026-0001` and `Q-2026-0001`.

- A number is allocated only on the first persistent save, not when the editor opens.
- Cancelling an unsaved editor does not consume a number.
- A persisted number never changes or becomes available for reuse.
- Sequence allocation and document persistence are atomic.
- Database uniqueness constraints protect sequences and document numbers.

## Historical Snapshots

Each quote and invoice persists three historical boundaries:

- issuer snapshot: company name, address, country, VAT number, chamber of commerce number, IBAN, email, and phone;
- customer snapshot: the customer details required to render the document;
- line-item snapshots: description, unit, unit price, VAT rate, quantity, and calculated values.

Later edits to `CompanySettings`, customer records, or catalog entries do not alter existing documents. The textual issuer snapshot is required from the first applicable document migration. Historical logo preservation is finalized with the PDF implementation in M6.

## Customer Archiving

- Company name, street, postal code, city, and country are required.
- Optional text is trimmed and stored as `null` when blank.
- When provided, the email address must have a valid format.
- Customers are archived instead of physically deleted.
- Archived customers remain linked to historical documents.
- Archived customers are excluded by default from new-document selection.
- Archiving does not alter document snapshots.

## Product and Service Deactivation

- Name and unit are required.
- Unit price cannot be negative.
- VAT rate is stored as a decimal fraction from `0` to `1`; the UI accepts a percentage from `0` to `100`.
- Catalog entries are deactivated instead of physically deleted.
- Deactivated entries are excluded by default from new-document selection.
- Existing document items remain unchanged and valid.

## Quote Conversion

Only an eligible quotation may be converted. The conversion:

- creates a new invoice;
- copies the customer snapshot;
- copies item descriptions, quantities, units, prices, VAT rates, and totals;
- copies applicable notes;
- preserves the source quotation;
- performs persistence as one transaction.

Each quotation can be converted at most once. The resulting invoice stores the source quotation identifier, protected by a unique database constraint. A repeat conversion is rejected without allocating a new invoice number. A failed conversion rolls back the invoice, line items, and sequence increment together.

The converted invoice receives its own `INV-YYYY-NNNN` number and starts in Draft. Its issue date is the current business date and its due date applies the configured default payment term. The source quotation remains Accepted and unchanged.

## PDF Rules

PDF generation reads persisted issuer, customer, and line-item snapshot data. It must not rehydrate current `CompanySettings`, customer, or catalog values in place of document snapshots. Invoice output includes issuer and customer details, number, dates, item table, VAT breakdown, totals, IBAN, and payment term. Quote output uses validity information instead of invoice due/payment fields where appropriate.

## Time and Audit Data

Business dates and audit timestamps are distinct concepts:

- `IssueDate`, `DueDate`, `ValidUntil`, and `PaymentDate` use `DateOnly` and are stored as ISO `yyyy-MM-dd` values. They are never converted between time zones.
- `CreatedAt`, `UpdatedAt`, and `Payment.CreatedAt` represent instants. They use `DateTimeOffset`, are normalized to UTC (offset zero), and are persisted in a round-trippable UTC representation.
- The application obtains `UtcNow` and `Today` from an injected clock abstraction.
- In production, `Today` is the calendar date obtained by converting the current UTC instant to the operating system's local time zone.
- Audit timestamps are converted to the operating system's local time zone for display; their persisted value remains UTC.
- Tests replace the clock and control both the instant and resulting business date deterministically.

The overdue rule compares `DueDate` with `clock.Today`, not directly with `DateTime.Now` or the database server.
