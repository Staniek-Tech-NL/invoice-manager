# ADR-0001: Use Layered Clean Architecture

Status: Accepted

## Context

Invoice Manager combines desktop presentation, business workflows, financial rules, local persistence, and document generation. These concerns change for different reasons and require different forms of testing. Directly coupling WPF, EF Core, and business rules would make calculations difficult to test and future technical substitutions unnecessarily expensive.

## Decision

Use four production projects:

- `InvoiceManager.Domain` for entities, value concepts, and business rules;
- `InvoiceManager.Application` for use cases and external-service ports;
- `InvoiceManager.Infrastructure` for EF Core, SQLite, PDF, files, and other adapters;
- `InvoiceManager.App` for WPF/MVVM presentation and dependency composition.

Dependencies point inward: Domain depends on nothing; Application depends on Domain; Infrastructure depends on Application and Domain; App depends on Application and Infrastructure.

## Consequences

- Core rules can be tested without WPF or SQLite.
- Infrastructure libraries can be replaced behind interfaces.
- More projects and mapping boundaries add initial structure.
- Team members must actively prevent business logic from drifting into view models or persistence code.
- Cross-layer changes may require coordinated DTO, mapping, and registration updates.
