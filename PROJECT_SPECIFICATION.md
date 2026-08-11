# Invoice Manager — Project Specification

**Repository:** `invoice-manager-desktop`  
**Product:** Invoice Manager  
**Platform:** Windows Desktop  
**Default project language:** English  
**Primary audience:** Freelancers and small businesses  
**MVP currency:** EUR  
**Project status:** Milestone 2 complete / Quotations next

---

## 1. Project Overview

Invoice Manager is a desktop application for freelancers and small businesses that simplifies:

- customer management,
- product and service catalog management,
- quotation creation,
- invoice creation,
- payment tracking,
- PDF generation,
- basic financial reporting.

The application is intentionally designed as a focused desktop business product rather than a full accounting system.

The portfolio goal is to demonstrate the complete software delivery process:

> requirements → domain design → architecture → database → UI → business logic → tests → CI → documentation → release

The project is designed for GitHub from the first commit. GitHub is not only a place to publish the finished application; it is the record of how the product was designed and built.

---

## 2. Project Language Policy

All repository-facing content is written in English.

| Area | Language |
|---|---|
| Source code | English |
| Classes / methods / properties | English |
| Database schema | English |
| UI | English |
| README | English |
| Documentation | English |
| GitHub Issues | English |
| Pull Requests | English |
| Commit messages | English |
| Code comments | English |
| Test names | English |
| Screenshots | English |
| Releases / changelog | English |

Project discussions may be conducted in Polish, but repository artifacts remain English.

---

## 3. Technology Stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 LTS |
| Language | C# |
| UI | WPF |
| UI pattern | MVVM |
| MVVM toolkit | CommunityToolkit.Mvvm |
| ORM | Entity Framework Core 10 |
| Database | SQLite |
| Dependency Injection / Host | Microsoft.Extensions.Hosting |
| Testing | xUnit |
| CI | GitHub Actions |
| Version Control | Git + GitHub |
| PDF | Abstracted behind an interface; library selected before M6 |

---

## 4. Architectural Approach

The project uses a clean layered architecture combined with MVVM.

```text
┌──────────────────────────────┐
│      InvoiceManager.App      │
│          WPF / MVVM          │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ InvoiceManager.Application   │
│          Use Cases           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│    InvoiceManager.Domain     │
│  Entities / Rules / Values   │
└──────────────────────────────┘

               ▲
               │ implementations
               │
┌──────────────────────────────┐
│InvoiceManager.Infrastructure │
│   EF Core / SQLite / PDF     │
└──────────────────────────────┘
```

### Dependency Rules

```text
Domain
 └── depends on nothing

Application
 └── depends on Domain

Infrastructure
 ├── depends on Domain
 └── depends on Application

App
 ├── depends on Application
 └── depends on Infrastructure
```

### Typical Data Flow

```text
View
 ↓
ViewModel
 ↓
Application Use Case
 ↓
Repository / Service Interface
 ↓
Infrastructure Implementation
 ↓
EF Core / SQLite
```

Business logic must not be placed directly in Views or ViewModels.

---

## 5. Repository Structure

```text
invoice-manager-desktop/
│
├── .github/
│   ├── workflows/
│   │   └── ci.yml
│   │
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.yml
│   │   └── feature_request.yml
│   │
│   └── pull_request_template.md
│
├── docs/
│   ├── README.md
│   ├── project-overview.md
│   ├── requirements.md
│   ├── architecture.md
│   ├── domain-model.md
│   ├── business-rules.md
│   ├── testing-strategy.md
│   ├── roadmap.md
│   ├── case-study.md
│   │
│   ├── decisions/
│   │   ├── README.md
│   │   ├── 0001-use-clean-architecture.md
│   │   ├── 0002-use-wpf-and-mvvm.md
│   │   ├── 0003-use-sqlite.md
│   │   ├── 0004-document-numbering.md
│   │   ├── 0005-preserve-document-snapshots.md
│   │   ├── 0006-use-recorded-payments.md
│   │   └── 0007-use-explicit-date-time-policy.md
│   │
│   └── images/
│       ├── dashboard.png
│       ├── customers.png
│       ├── quote-editor.png
│       ├── invoice-editor.png
│       └── invoice-pdf.png
│
├── src/
│   ├── InvoiceManager.App/
│   ├── InvoiceManager.Application/
│   ├── InvoiceManager.Domain/
│   └── InvoiceManager.Infrastructure/
│
├── tests/
│   ├── InvoiceManager.Domain.Tests/
│   ├── InvoiceManager.Application.Tests/
│   └── InvoiceManager.Infrastructure.Tests/
│
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── CHANGELOG.md
├── CONTRIBUTING.md
├── LICENSE
├── README.md
└── InvoiceManager.sln
```

---

## 6. Domain Model

### 6.1 CompanySettings

Stores the single business profile used by the application.

```text
Id
CompanyName
Street
PostalCode
City
Country
VatNumber
ChamberOfCommerceNumber
Iban
Email
Phone
DefaultVatRate
DefaultPaymentTermDays
Currency
LogoPath
```

MVP supports one company profile.

---

### 6.2 Customer

```text
Id
CompanyName
ContactPerson
Street
PostalCode
City
Country
Email
Phone
VatNumber
Notes
IsArchived
CreatedAt
UpdatedAt
```

Rules:

- customers are archived instead of physically deleted,
- archived customers remain linked to historical documents,
- archived customers do not appear by default when creating new documents.

---

### 6.3 ProductService

```text
Id
Name
Description
Unit
UnitPrice
VatRate
IsActive
CreatedAt
UpdatedAt
```

Example units:

```text
hour
day
item
service
```

Rules:

- services can be deactivated,
- previously used services are not physically deleted.

---

### 6.4 Quote

```text
Id
Number
CustomerId
IssuerSnapshot
CustomerSnapshot
IssueDate
ValidUntil
Status
Notes
Subtotal
VatTotal
Total
CreatedAt
UpdatedAt
```

Statuses:

```text
Draft
Sent
Accepted
Rejected
Expired
```

---

### 6.5 QuoteItem

```text
Id
QuoteId
Description
Quantity
Unit
UnitPrice
VatRate
NetAmount
VatAmount
GrossAmount
```

---

### 6.6 Invoice

```text
Id
Number
CustomerId
IssuerSnapshot
CustomerSnapshot
IssueDate
DueDate
Status
Notes
Subtotal
VatTotal
Total
CreatedAt
UpdatedAt
```

Statuses:

```text
Draft
Sent
Paid
Overdue
Cancelled
```

---

### 6.7 InvoiceItem

```text
Id
InvoiceId
Description
Quantity
Unit
UnitPrice
VatRate
NetAmount
VatAmount
GrossAmount
```

---

### 6.8 Payment

```text
Id
InvoiceId
PaymentDate
Amount
Reference
Method
CreatedAt
```

---

### 6.9 DocumentNumberSequence

Technical entity used to generate safe yearly document numbers.

```text
Id
DocumentType
Year
LastNumber
```

Unique constraint:

```text
DocumentType + Year
```

Examples:

```text
INV-2026-0001
INV-2026-0002

Q-2026-0001
Q-2026-0002
```

---

## 7. Document Snapshot Rule

Historical documents must not change when source data changes later.

For this reason, quote and invoice items store their own snapshot values:

```text
Description
Unit
UnitPrice
VatRate
```

instead of dynamically reading current values from `ProductService`.

The same rule applies to customer and issuer data used in a document.

Each quotation and invoice preserves the issuer details required to render the original document. At minimum, the issuer snapshot contains:

```text
IssuerCompanyName
IssuerStreet
IssuerPostalCode
IssuerCity
IssuerCountry
IssuerVatNumber
IssuerChamberOfCommerceNumber
IssuerIban
IssuerEmail
IssuerPhone
```

The implementation may use an owned type or value object such as `IssuerSnapshot`. Historical views and regenerated PDFs use the persisted issuer, customer, and line-item snapshots. They must not substitute current values from `CompanySettings`, `Customer`, or `ProductService`. Historical logo preservation is finalized with PDF implementation in M6.

Example:

```text
Customer:
ACME BV
Amsterdam

↓ customer address changes

Existing invoice:
keeps original address

New invoice:
uses new address
```

This guarantees document history remains stable.

---

## 8. MVP Modules

### 8.1 Dashboard

Displays:

```text
Revenue this month
Revenue this year
Outstanding amount
Overdue amount
Number of invoices
Number of customers
```

Additional elements:

- revenue by month chart,
- recent invoices list.

---

### 8.2 Customers

Functions:

```text
Create
View
Edit
Archive
Search
```

Example view:

```text
Customers

Search...

[ + Add customer ]

Company          Contact          VAT Number        Email
---------------------------------------------------------
Example BV       Jan de Vries     NL...             ...
AK Solutions     Anna Kowalska    PL...             ...
```

---

### 8.3 Products & Services

Functions:

```text
Create
Edit
Deactivate
Search
```

Example:

```text
Software Development
€75.00 / hour
VAT 21%

Database Consulting
€80.00 / hour
VAT 21%
```

---

### 8.4 Quotes

Workflow:

```text
Customer
   ↓
Create Quote
   ↓
Add Services
   ↓
Calculate Totals
   ↓
Draft
   ↓
Sent
   ↓
Accepted
   ↓
Convert to Invoice
```

Core action:

> Convert Quote to Invoice

The conversion copies:

- customer snapshot,
- items,
- quantities,
- prices,
- VAT rates,
- applicable notes.

The source quote remains stored after conversion.

---

### 8.5 Invoices

Core fields:

```text
Invoice number
Customer
Issue date
Due date
Items
Subtotal
VAT
Total
Notes
Status
```

Core actions:

```text
Save
Mark as Sent
Cancel
Generate PDF
Register Payment
```

---

### 8.6 Payments

Payments are recorded as separate transactions.

The application must not use a simple `Paid` checkbox.

Example:

```text
Invoice total: €1,000

Payment #1: €400
Outstanding: €600

Payment #2: €600
Outstanding: €0

Invoice status → Paid
```

Formula:

```text
OutstandingAmount =
Invoice.Total - Sum(Payments)
```

Rule:

```text
Payment > OutstandingAmount
→ validation error
```

---

### 8.7 Settings

#### Company

```text
Company name
Address
Country
VAT number
Chamber of Commerce number
Email
Phone
```

#### Banking

```text
IBAN
```

#### Invoice defaults

```text
Default VAT rate
Default payment term
Currency
```

#### Branding

```text
Company logo
```

MVP uses EUR only.

---

## 9. Business Rules

### 9.1 Invoice Overdue Rule

```text
IF
    DueDate < Today
AND OutstandingAmount > 0
AND Status != Draft
AND Status != Cancelled

THEN
    Status = Overdue
```

A partially paid invoice may still be overdue.

---

### 9.2 Paid Rule

```text
OutstandingAmount = 0
→ Paid
```

---

### 9.3 Overpayment Rule

```text
Payment > OutstandingAmount
→ reject payment
```

---

### 9.4 Document Number Assignment

A document number is not assigned when the editor is opened.

The number is assigned only during the first persistent save.

This prevents unused numbers when the user cancels document creation.

---

### 9.5 Document Number Format

```text
INV-YYYY-NNNN
Q-YYYY-NNNN
```

Examples:

```text
INV-2026-0001
Q-2026-0001
```

Sequences are independent per document type and year.

---

### 9.6 Financial Calculation

Money calculations use `decimal`.

Never use:

```text
float
double
```

Base formula:

```text
Net = Quantity × UnitPrice
VAT = Net × VatRate
Gross = Net + VAT
```

Rounding strategy must be explicit, centralized, tested and documented.

---

## 10. PDF Generation

PDF generation is abstracted from the application layer.

Example interface:

```csharp
public interface IDocumentPdfGenerator
{
    Task GenerateInvoiceAsync(...);
    Task GenerateQuoteAsync(...);
}
```

Architecture:

```text
Application
      ↓
IDocumentPdfGenerator
      ↑
Infrastructure
      ↓
PDF library
```

The PDF library is selected before Milestone 6.

Historical PDF generation uses persisted issuer, customer, and line-item snapshots. It must not read current company, customer, or catalog values in place of document snapshots.

PDF output should include:

- company logo,
- company details,
- customer details,
- document number,
- issue date,
- due date or validity date,
- item table,
- VAT,
- totals,
- IBAN,
- payment term.

---

## 11. Local Storage

Application data is stored locally.

Default application directory:

```text
%LocalAppData%
└── InvoiceManager/
    ├── invoice-manager.db
    ├── logs/
    └── assets/
```

Generated PDFs are exported to a user-selected location.

### Date and Time Storage Policy

Business dates and audit timestamps are separate concepts:

```text
Business dates (DateOnly, stored as yyyy-MM-dd):
IssueDate
DueDate
ValidUntil
PaymentDate

Audit timestamps (DateTimeOffset, normalized to UTC):
CreatedAt
UpdatedAt
Payment.CreatedAt
```

Business dates are never converted between time zones. Audit timestamps are persisted as round-trippable UTC instants and converted to the operating system's local time zone only for display.

Application and Domain code obtain `UtcNow` and the current local business date from an injected clock abstraction. The production clock derives `Today` using the operating system's local time zone; tests use a controlled clock. Business rules must not read system time directly. The overdue rule compares `DueDate` with `clock.Today`.

---

## 12. UI Structure

Main navigation:

```text
Dashboard
Customers
Services
Quotes
Invoices
Settings
```

Main shell concept:

```text
┌─────────────────────────────────────────────────┐
│ Invoice Manager                                 │
├─────────────┬───────────────────────────────────┤
│ Dashboard   │                                   │
│ Customers   │                                   │
│ Services    │        Current View               │
│ Quotes      │                                   │
│ Invoices    │                                   │
│ Settings    │                                   │
└─────────────┴───────────────────────────────────┘
```

Design principles:

- clean,
- professional,
- light,
- business-oriented,
- portfolio-ready,
- consistent across screenshots.

---

## 13. WPF Project Structure

```text
InvoiceManager.App/
│
├── Views/
│   ├── Dashboard/
│   ├── Customers/
│   ├── Products/
│   ├── Quotes/
│   ├── Invoices/
│   └── Settings/
│
├── ViewModels/
│   ├── Dashboard/
│   ├── Customers/
│   ├── Products/
│   ├── Quotes/
│   ├── Invoices/
│   └── Settings/
│
├── Controls/
├── Converters/
├── Navigation/
├── Services/
├── Resources/
├── Styles/
│
├── App.xaml
└── App.xaml.cs
```

---

## 14. Application Layer

Use cases are organized by feature.

```text
Customers/
    CreateCustomer
    UpdateCustomer
    ArchiveCustomer
    GetCustomer
    SearchCustomers

Products/
    CreateProductService
    UpdateProductService
    DeactivateProductService

Quotes/
    CreateQuote
    UpdateQuote
    SendQuote
    AcceptQuote
    RejectQuote
    ConvertQuoteToInvoice

Invoices/
    CreateInvoice
    UpdateInvoice
    MarkInvoiceAsSent
    CancelInvoice
    GetInvoice

Payments/
    RegisterPayment
    GetInvoicePayments

Dashboard/
    GetDashboardSummary

Documents/
    GenerateInvoicePdf
    GenerateQuotePdf

Settings/
    GetCompanySettings
    UpdateCompanySettings
```

Avoid giant service classes containing unrelated responsibilities.

---

## 15. Infrastructure Layer

```text
InvoiceManager.Infrastructure/
│
├── Persistence/
│   ├── InvoiceManagerDbContext.cs
│   ├── DbContextFactory.cs
│   │
│   ├── Configurations/
│   ├── Repositories/
│   └── Migrations/
│
├── Pdf/
├── Storage/
├── Logging/
└── DependencyInjection.cs
```

Repositories should represent domain needs rather than using one generic repository for everything.

---

## 16. Testing Strategy

Target:

> 50–100 meaningful tests

The goal is useful coverage of business logic, not artificial coverage numbers.

### 16.1 Domain Tests

```text
invoice calculations
VAT calculations
money rounding
payment totals
outstanding balance
invoice status
overdue status
document numbering
quote totals
issuer, customer and item snapshots
date-only business dates
UTC audit timestamps
```

### 16.2 Application Tests

```text
create customer
archive customer
create quote
convert quote to invoice
create invoice
register payment
reject overpayment
generate dashboard data
```

### 16.3 Infrastructure Tests

```text
SQLite persistence
EF relationships
database constraints
migrations
repository queries
```

---

## 17. Critical Test Scenarios

### Partial Payment

```text
Given:
Invoice total = €1,000

When:
Payment €400 is registered

Then:
Outstanding = €600
Invoice != Paid
```

### Full Payment

```text
Given:
Invoice total = €1,000

When:
Payment €1,000 is registered

Then:
Outstanding = €0
Status = Paid
```

### Overpayment

```text
Given:
Outstanding = €600

When:
User tries to register €700

Then:
Payment is rejected
```

### Overdue Invoice

```text
Given:
Due date = yesterday
Outstanding = €100

Then:
Status = Overdue
```

---

## 18. GitHub Workflow

The `main` branch must always represent a buildable and tested state.

```text
GitHub Issue
      ↓
feature branch
      ↓
implementation
      ↓
tests
      ↓
Pull Request
      ↓
CI
      ↓
merge → main
```

Even when the project is developed by one person, the workflow remains visible and structured for portfolio value.

---

## 19. Branch Naming

```text
feat/12-create-customer
feat/23-invoice-numbering

fix/31-payment-validation

docs/18-update-architecture

refactor/27-invoice-calculation
```

Branch numbers should match the related GitHub Issue.

---

## 20. Commit Convention

Examples:

```text
feat: add customer creation workflow

feat: implement invoice number generator

fix: prevent invoice overpayment

test: add invoice status tests

docs: document payment business rules

refactor: extract invoice calculation service
```

Avoid vague commit messages such as:

```text
update
changes
fix
final
```

---

## 21. GitHub Issue Template

```markdown
## Goal

## Requirements

## Acceptance Criteria

## Technical Notes

## Dependencies

## Documentation
```

Example:

```text
# Create customer management

Goal:
Allow users to create and manage customers.

Acceptance Criteria:
- Customer can be created
- Required fields are validated
- Customer is persisted in SQLite
- Customer appears in customer list
- Tests pass
```

---

## 22. Milestones

### M1 — Foundation

```text
Solution
Projects
Git repository
Dependency Injection
Logging
SQLite
EF Core
Initial migration
GitHub Actions
Base navigation
Documentation skeleton
```

### M2 — Customers & Services — COMPLETE

```text
Customer CRUD
Customer archive
Customer search

Service CRUD
Service deactivate
Service search
```

### M3 — Quotes

```text
Quote editor
Quote items
VAT calculations
Quote statuses
Quote numbering
```

### M4 — Invoices

```text
Invoice editor
Invoice calculations
Invoice numbering
Invoice statuses
Quote → Invoice
```

### M5 — Payments

```text
Register payments
Partial payments
Full payments
Outstanding amount
Paid state
Overpayment protection
Overdue logic
```

### M6 — PDF & Dashboard

```text
Invoice PDF
Quote PDF

Dashboard KPIs
Recent invoices
Revenue chart
```

### M7 — Portfolio Polish

```text
Final testing
UI polishing
Demo data
Screenshots
README
Architecture diagrams
Case study
Release build
GitHub Release
```

---

## 23. GitHub Actions CI

CI exists from Milestone 1.

Workflow:

```text
Pull Request / Push
        ↓
Restore
        ↓
Build
        ↓
Tests
        ↓
Result
```

The WPF build uses a Windows runner.

Pull Requests should not be merged when:

```text
Build ❌
Tests ❌
```

---

## 24. Definition of Done — Issue

A task is complete only when:

```text
implementation complete
+
tests added or updated
+
existing tests pass
+
build passes
+
code reviewed
+
documentation updated when required
+
acceptance criteria satisfied
```

---

## 25. Definition of Done — MVP

The complete workflow must work:

```text
Configure company
      ↓
Create customer
      ↓
Create service
      ↓
Create quote
      ↓
Accept quote
      ↓
Convert quote to invoice
      ↓
Generate PDF
      ↓
Mark invoice as sent
      ↓
Register partial/full payment
      ↓
Invoice becomes Paid
      ↓
Dashboard updates
```

Additionally:

- overdue invoice logic works correctly,
- persistence survives application restart,
- CI passes,
- release build can be downloaded and launched,
- README explains the project,
- screenshots are included.

Completion target:

```text
v1.0.0
```

---

## 26. Explicitly Out of Scope for MVP

The following features are not part of v1.0:

```text
User accounts
Authentication
Cloud synchronization
Web application
Mobile application

Bank API
Stripe
PayPal
Automatic bank reconciliation

Recurring invoices
Multiple currencies
Accounting integrations

Inventory

Multiple businesses
Multiple users

Email server integration
```

The project is a desktop invoice and quotation manager, not a full accounting or ERP system.

---

## 27. Documentation Structure

### `docs/project-overview.md`

Contains:

```text
problem
target user
product goal
MVP
non-goals
```

### `docs/requirements.md`

Contains:

```text
functional requirements
non-functional requirements
acceptance criteria
```

### `docs/architecture.md`

Contains:

```text
layers
dependencies
data flow
dependency injection
persistence
```

### `docs/domain-model.md`

Contains:

```text
entities
relationships
value objects
document lifecycle
```

### `docs/business-rules.md`

Contains:

```text
document numbering
payments
statuses
VAT
archiving
quote conversion
```

### `docs/testing-strategy.md`

Contains:

```text
test levels
test conventions
critical scenarios
```

### `docs/roadmap.md`

Contains:

```text
M1–M7
future ideas
```

---

## 28. Architecture Decision Records

Architecture decisions are documented in:

```text
docs/decisions/
```

ADR format:

```markdown
# ADR-0001: Decision Title

Status: Accepted

## Context

...

## Decision

...

## Consequences

...
```

Initial ADR list:

```text
0001 Use layered clean architecture
0002 Use WPF with MVVM
0003 Use SQLite with EF Core
0004 Assign document numbers on first save
0005 Preserve document snapshots
0006 Use recorded payments instead of Paid checkbox
0007 Use explicit business-date and UTC timestamp semantics
```

---

## 29. README Structure

```text
# Invoice Manager

Desktop invoice and quotation management application
for freelancers and small businesses.

[Screenshot]

## Features

## Demo Workflow

## Technology

## Architecture

## Screenshots

## Getting Started

## Testing

## Project Status

## Roadmap

## Engineering Decisions

## License
```

The README should present the application as a real software product, not as a coding exercise.

---

## 30. Portfolio Case Study

File:

```text
docs/case-study.md
```

Suggested structure:

```text
Problem

Goals

Requirements

Architecture

Domain challenges

Key engineering decisions

Implementation

Testing strategy

Result

Lessons learned
```

Strong portfolio topics:

- Quote → Invoice workflow,
- document numbering,
- partial payments,
- automatic overdue calculation,
- document snapshots,
- PDF generation,
- clean architecture,
- automated CI.

---

## 31. Screenshot Plan

Minimum screenshot set:

```text
01-dashboard.png
02-customers.png
03-quote-editor.png
04-invoice-editor.png
05-invoice-pdf.png
```

Optional portfolio demo:

```text
Customer
→ Quote
→ Invoice
→ Payment
```

---

## 32. Release Strategy

Planned internal version progression:

```text
0.1.0 Foundation
0.2.0 Customers & Services
0.3.0 Quotes
0.4.0 Invoices
0.5.0 Payments
0.6.0 PDF & Dashboard
1.0.0 Portfolio Release
```

Recommended public GitHub releases:

```text
v0.1.0
v0.5.0
v1.0.0
```

---

## 33. Code Quality Rules

Project-wide standards:

```text
Nullable = enable
ImplicitUsings = enable
.editorconfig
centralized package versions
centralized build configuration
async APIs for I/O
CancellationToken where useful
warnings monitored
```

Architecture rules:

```text
no business logic in Views
no SQL in ViewModels
no DbContext in Views
no giant MainViewModel
no static service locator
no duplicated financial calculations
no magic document status strings
```

---

## 34. AI Development Rules

Any AI working on the repository must follow these rules:

1. Inspect existing code before proposing changes.
2. Inspect related documentation.
3. Check existing business rules.
4. Check existing tests covering the affected feature.
5. Implement the smallest coherent change.
6. Add or update tests.
7. Run the relevant test suite.
8. Update documentation when architecture or behaviour changes.
9. Never silently change accepted project decisions.
10. Do not redesign working architecture during feature implementation unless the redesign is explicitly justified and documented.

---

## 35. Current Project Status

| Area | Status |
|---|---|
| Product concept | DONE |
| MVP scope | DONE |
| Out-of-scope definition | DONE |
| Technology stack | DECIDED |
| Architecture | DECIDED |
| Repository strategy | DECIDED |
| Language strategy | DECIDED |
| Domain model | DESIGNED |
| Business rules | DESIGNED |
| Testing strategy | DESIGNED |
| GitHub workflow | DESIGNED |
| Documentation foundation | DONE |
| ADR foundation | DONE |
| PDF implementation | TO DECIDE — M6 |
| Milestone 1 foundation | DONE |
| Milestone 2 customers and services | DONE |
| Source code | M2 COMPLETE |
| Local Git repository | DONE |
| GitHub remote repository | NOT STARTED |
| CI | CONFIGURED |
| UI | SHELL, CUSTOMER, AND SERVICE WORKFLOWS DONE |

---

## 36. Milestone 1 — Initial Backlog

### M1 — Foundation

```text
#1  Initialize Git repository and solution — DONE
#2  Add Domain project — DONE
#3  Add Application project — DONE
#4  Add Infrastructure project — DONE
#5  Add WPF App project — DONE
#6  Add test projects — DONE
#7  Configure project dependencies — DONE
#8  Configure Generic Host and DI — DONE
#9  Configure application logging — DONE
#10 Add EF Core and SQLite — DONE
#11 Create initial DbContext — DONE
#12 Configure application data directory — DONE
#13 Create first database migration — DONE
#14 Add basic WPF shell — DONE
#15 Implement application navigation — DONE
#16 Configure GitHub Actions CI — DONE
#17 Add repository templates — DONE
#18 Verify documentation consistency before implementation — DONE
#19 Verify README and documentation navigation — DONE
#20 Verify clean clone build — DONE
```

---

## 37. Recommended First Step

The first implementation task is:

> **M1.1 — Repository & Solution Foundation**

Expected structure:

```text
InvoiceManager.sln

src/
    InvoiceManager.App
    InvoiceManager.Application
    InvoiceManager.Domain
    InvoiceManager.Infrastructure

tests/
    InvoiceManager.Domain.Tests
    InvoiceManager.Application.Tests
    InvoiceManager.Infrastructure.Tests

docs/

.github/

README.md
```

Recommended first commit:

```text
chore: initialize Invoice Manager solution
```

From this point forward, every feature should be implemented as part of the GitHub workflow rather than being added to GitHub only after completion.

---

## 38. Final Product Definition

Invoice Manager v1.0 is:

> A professional Windows desktop invoice and quotation management application for freelancers and small businesses, built with .NET, WPF, MVVM, EF Core and SQLite, designed from the beginning as a portfolio-grade GitHub project with documented architecture, automated testing, CI and a complete release workflow.

It is intentionally not a full accounting system.

The priority is to deliver a finished, testable, documented and presentable product.
