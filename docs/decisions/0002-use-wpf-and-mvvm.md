# ADR-0002: Use WPF with MVVM

Status: Accepted

## Context

The product targets Windows desktop users and needs forms, tables, navigation, validation feedback, reusable styles, and printable business-document workflows. The UI should remain testable and must not own business behavior.

## Decision

Use WPF on .NET 10 with the MVVM pattern. Use `CommunityToolkit.Mvvm` for observable state and commands. Views bind to view models; view models invoke Application use cases and contain presentation state only.

## Consequences

- The product has native Windows desktop capabilities and mature data binding.
- View models can be tested independently of rendered controls.
- WPF constrains the product to Windows.
- Care is required to keep view code-behind limited to presentation-specific behavior.
- Navigation, dialogs, validation display, and design resources need consistent abstractions and conventions.
