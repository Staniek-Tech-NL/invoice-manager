# Architecture

## Overview

Invoice Manager uses layered clean architecture with MVVM at the WPF boundary. Dependencies point toward the domain, while infrastructure implements interfaces defined by inner layers.

```text
InvoiceManager.App
  WPF Views, ViewModels, navigation, desktop services
                 |
                 v
InvoiceManager.Application
  Use cases, DTOs, validation orchestration, service/repository ports
                 |
                 v
InvoiceManager.Domain
  Entities, value objects, calculations, lifecycle rules

InvoiceManager.Infrastructure
  EF Core, SQLite, repositories, PDF, files, logging adapters
  implements Application and Domain ports
```

## Dependency Rules

| Project | May depend on |
|---|---|
| Domain | No other project |
| Application | Domain |
| Infrastructure | Application, Domain |
| App | Application, Infrastructure |

References in the opposite direction are prohibited. The App project is the composition root.

## Responsibilities

### Domain

- Entities and value objects
- Financial calculations and rounding policy
- Invoice and quotation lifecycle invariants
- Payment and outstanding balance rules
- Domain-specific exceptions or result types

Domain code must not reference WPF, EF Core, SQLite, file paths, PDF libraries, or dependency injection.

### Application

- Feature-oriented use cases
- Repository and external-service interfaces
- Transaction boundaries and orchestration
- Input/output models and application validation
- Cancellation-aware asynchronous I/O contracts

Use cases should be small and cohesive. A generic service containing unrelated workflows is not acceptable.

### Infrastructure

- EF Core `InvoiceManagerDbContext`, configurations, migrations, and repositories
- SQLite connection and local data directory management
- PDFsharp 6.2.4 document generator
- File storage and logging adapters
- Registration extension methods for infrastructure services

Repositories model domain needs. A generic repository abstraction is not the default.

### App

- WPF views and reusable controls
- MVVM view models and commands
- Navigation and dialogs
- UI-specific formatting and converters
- Generic Host startup and dependency composition

Views and view models must not contain financial rules, SQL, or direct `DbContext` access.

## Typical Request Flow

```text
View -> ViewModel -> Application use case -> Port
     -> Infrastructure implementation -> EF Core/SQLite
     -> Result -> ViewModel -> View
```

View models translate user intent into use-case calls and expose presentation state. Application and Domain determine business outcomes.

## Persistence

SQLite stores application data locally at:

```text
%LocalAppData%/InvoiceManager/invoice-manager.db
```

The same directory contains `logs/` and application-managed `assets/`. Generated documents are exported separately to a location selected by the user.

EF Core entity configurations define relationships, decimal storage strategy, indexes, constraints, and snapshot columns. Quote and invoice persistence includes issuer, customer, and line-item snapshots. Migrations are committed and verified through infrastructure tests.

The application uses `IDbContextFactory<InvoiceManagerDbContext>` for desktop-safe context creation. A hosted initialization service applies pending migrations before the main window is shown.

Business dates (`IssueDate`, `DueDate`, `ValidUntil`, and `PaymentDate`) use date-only values and are stored as ISO `yyyy-MM-dd` values without time-zone conversion. Audit timestamps (`CreatedAt`, `UpdatedAt`, and `Payment.CreatedAt`) are UTC instants and are persisted in a round-trippable UTC representation. The concrete EF Core conversions are centralized and covered by persistence tests.

## Consistency and Transactions

Document numbering combines a process-wide `SemaphoreSlim`, a SQLite serializable transaction, and database uniqueness constraints. The repository loads or creates the `DocumentNumberSequence` for `DocumentType + Year`, increments it, assigns the final number, persists the aggregate, and commits as one unit. A failed save rolls the sequence increment back; opening or abandoning an editor never allocates a number. Unique indexes protect both sequence keys and final quote or invoice numbers.

Accepted quote conversion uses the same allocation lock and serializable transaction. It loads the source quote with its item snapshots, rejects an existing `SourceQuoteId`, creates the invoice snapshot, allocates the invoice number, and persists the complete aggregate before commit. A unique database index on `Invoice.SourceQuoteId` provides the final one-time conversion guarantee. Failure or repetition rolls back without consuming a number.

Payment registration and voiding use a separate process-wide lock and serializable transaction. The invoice and all payments are loaded together, the aggregate validates the requested operation against the current outstanding balance, and the result is committed atomically. Competing registrations therefore cannot both spend the same outstanding balance. These numbering, conversion, rollback, uniqueness, and competing-payment scenarios are covered by automated SQLite infrastructure tests.

## Dependency Injection and Hosting

`Microsoft.Extensions.Hosting` provides application startup, configuration, logging, and dependency injection. Registration is grouped by layer. The WPF App is responsible for starting and stopping the host and resolving the main shell.

No static service locator is permitted.

Logging uses the `Microsoft.Extensions.Logging` abstraction with debug output and a local file provider. The file provider writes UTC timestamps to `%LocalAppData%/InvoiceManager/logs/invoice-manager.log` and must not be used to record sensitive document contents.

## Date and Time Boundary

Application and Domain code obtain `UtcNow` and the application-local `Today` through an injected clock abstraction. The production implementation derives `Today` from the operating system's local time zone; tests use a controlled clock. Audit timestamps are converted from UTC to local time only for display. Business dates are never shifted through a time-zone conversion. See [ADR-0007](decisions/0007-use-explicit-date-time-policy.md).

## PDF Boundary

Application code depends on `IDocumentPdfGenerator`; Infrastructure implements it with PDFsharp 6.2.4. The generator loads persisted document aggregates and renders only their issuer, customer, line-item, date, total, payment, and logo snapshots. The WPF layer supplies a user-selected output path. See [ADR-0009](decisions/0009-use-pdfsharp.md).

## Repository Structure

```text
src/
  InvoiceManager.App/
  InvoiceManager.Application/
  InvoiceManager.Domain/
  InvoiceManager.Infrastructure/
tests/
  InvoiceManager.Domain.Tests/
  InvoiceManager.Application.Tests/
  InvoiceManager.Infrastructure.Tests/
docs/
  decisions/
.github/
  workflows/
```

## Cross-Cutting Rules

- Nullable reference types and implicit usings are enabled.
- Package versions and common build settings are centralized.
- I/O APIs are asynchronous and accept `CancellationToken` where useful.
- Financial calculations are centralized and never duplicated in UI or PDF code.
- Historical document rendering reads persisted issuer, customer, and line-item snapshots.
- Statuses use domain types, not magic strings.
- Accepted architectural changes require an ADR update or a superseding ADR.
