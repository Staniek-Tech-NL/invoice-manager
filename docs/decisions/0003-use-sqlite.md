# ADR-0003: Use SQLite with Entity Framework Core

Status: Accepted

## Context

The MVP is a local-first, single-business desktop application. It does not require a database server, shared cloud data, or multi-user access. Installation and backup should remain simple while still supporting relational integrity, migrations, queries, and transactions.

## Decision

Use SQLite as the local database and Entity Framework Core 10 as the ORM. Store the database under `%LocalAppData%/InvoiceManager`. Commit and test schema migrations. Define entity mappings and constraints explicitly.

## Consequences

- Deployment requires no separate database service.
- Local development and isolated integration testing are straightforward.
- EF Core provides migrations and expressive queries.
- SQLite concurrency and decimal storage characteristics must be handled and tested deliberately.
- Future cloud or multi-user requirements would require architectural reassessment and likely a different persistence deployment.
