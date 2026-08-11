# ADR-0007: Use Explicit Business-Date and UTC Timestamp Semantics

Status: Accepted

## Context

Invoices and payments contain both calendar dates and audit timestamps. Treating them as interchangeable `DateTime` values creates ambiguous persistence, time-zone shifts, and nondeterministic overdue tests. The policy must be settled before the first database migration.

## Decision

Model `IssueDate`, `DueDate`, `ValidUntil`, and `PaymentDate` as `DateOnly`. Persist them as ISO `yyyy-MM-dd` values without time-zone conversion.

Model `CreatedAt`, `UpdatedAt`, and `Payment.CreatedAt` as `DateTimeOffset` audit instants normalized to UTC with offset zero. Persist them in a round-trippable UTC representation and convert them to the operating system's local time zone only for display.

Application and Domain code use an injected clock abstraction exposing the current UTC instant and application-local business date. The production clock derives the business date using the operating system's local time zone. Tests provide a controlled clock. Business rules must not call `DateTime.Now`, `DateTime.UtcNow`, or `DateOnly.FromDateTime` directly.

## Consequences

- Due dates and payment dates cannot shift when displayed in another time zone.
- Audit timestamps represent unambiguous instants and can be compared consistently.
- Overdue behavior is deterministic in automated tests.
- EF Core date and timestamp conversions must be centralized and covered by SQLite integration tests.
- The MVP follows the operating system's time zone; selectable company time zones remain outside the current scope.
