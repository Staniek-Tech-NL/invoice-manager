# Requirements

Requirement identifiers are stable references for issues, tests, and release verification.

## Functional Requirements

### Company Settings

- **FR-SET-001:** The user can create and update one company profile.
- **FR-SET-002:** The profile stores contact, tax, banking, default VAT, payment-term, currency, and logo information.
- **FR-SET-003:** The MVP currency is fixed to EUR.

### Customers

- **FR-CUS-001:** The user can create, view, edit, search, and archive customers.
- **FR-CUS-002:** Archived customers remain available to historical documents.
- **FR-CUS-003:** Archived customers are excluded by default when creating a new document.

### Products and Services

- **FR-SVC-001:** The user can create, edit, search, and deactivate products or services.
- **FR-SVC-002:** Deactivated entries remain available to historical documents.
- **FR-SVC-003:** Each entry can define description, unit, unit price, and VAT rate.

### Quotations

- **FR-QUO-001:** The user can create and update a quotation with a customer, dates, items, notes, and calculated totals.
- **FR-QUO-002:** A quotation supports Draft, Sent, Accepted, Rejected, and Expired statuses.
- **FR-QUO-003:** A quotation receives a unique yearly number on its first persistent save.
- **FR-QUO-004:** An accepted quotation can be converted to an invoice.
- **FR-QUO-005:** Conversion copies customer data, items, quantities, units, prices, VAT rates, and applicable notes without removing the source quotation.

### Invoices

- **FR-INV-001:** The user can create and update an invoice with a customer, dates, items, notes, and calculated totals.
- **FR-INV-002:** An invoice supports Draft, Sent, Paid, Overdue, and Cancelled statuses.
- **FR-INV-003:** An invoice receives a unique yearly number on its first persistent save.
- **FR-INV-004:** The user can mark an invoice as sent or cancel it.
- **FR-INV-005:** The user can generate a PDF from a persisted invoice.

### Payments

- **FR-PAY-001:** The user can record multiple payments against one invoice.
- **FR-PAY-002:** Each payment stores date, amount, reference, and method.
- **FR-PAY-003:** The application calculates the outstanding balance from recorded payments.
- **FR-PAY-004:** A payment greater than the outstanding balance is rejected.
- **FR-PAY-005:** A zero outstanding balance sets the invoice status to Paid.
- **FR-PAY-006:** A partially paid invoice can become Overdue.

### Dashboard

- **FR-DAS-001:** The dashboard shows revenue for the current month and year.
- **FR-DAS-002:** The dashboard shows outstanding and overdue amounts.
- **FR-DAS-003:** The dashboard shows invoice and customer counts.
- **FR-DAS-004:** The dashboard shows monthly revenue and recent invoices.

### Documents and Storage

- **FR-DOC-001:** Generated PDFs contain company, customer, date, item, VAT, total, banking, and payment-term information appropriate to the document type.
- **FR-DOC-002:** Existing documents retain their original issuer, customer, and item snapshot data after source records change.
- **FR-DOC-003:** Historical views and regenerated PDFs use persisted document snapshots rather than current company, customer, or catalog data.
- **FR-STO-001:** Application data is stored locally in SQLite under `%LocalAppData%/InvoiceManager`.
- **FR-STO-002:** PDFs are exported to a location selected by the user.

## Non-Functional Requirements

- **NFR-001 Reliability:** Financial calculations use `decimal`, centralized rounding, and automated tests.
- **NFR-002 Integrity:** Document numbering is unique per document type and year.
- **NFR-003 Maintainability:** Domain rules are isolated from WPF and infrastructure concerns.
- **NFR-004 Testability:** Core business behavior can be tested without starting the desktop UI.
- **NFR-005 Usability:** The interface is consistent, professional, and suitable for regular business use.
- **NFR-006 Portability:** A documented clean clone can restore, build, and test on a supported Windows development environment.
- **NFR-007 Local-first:** Core application workflows require no network connection.
- **NFR-008 Observability:** Operational failures are logged locally without exposing sensitive business data unnecessarily.
- **NFR-009 Compatibility:** The release targets Windows and uses .NET 10 LTS.
- **NFR-010 Quality:** Pull requests must build and pass automated tests before merge.

## MVP Acceptance Criteria

The MVP is accepted when a user can configure a company, create a customer and service, create and accept a quotation, convert it to an invoice, export the PDF, record partial and final payments, observe the Paid state and dashboard update, and verify that overdue logic and persistence work after restart.

## Traceability

Issues should reference requirement identifiers. Tests should use descriptive names and mention the relevant requirement in test documentation or issue context when useful. Requirement changes must be reflected in business rules, architecture, and ADRs where applicable.
