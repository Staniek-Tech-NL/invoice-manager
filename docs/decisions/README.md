# Architecture Decision Records

Architecture Decision Records (ADRs) preserve the context and consequences of significant choices. Accepted decisions apply to implementation until they are explicitly superseded.

| ADR | Decision | Status |
|---|---|---|
| [0001](0001-use-clean-architecture.md) | Use layered clean architecture | Accepted |
| [0002](0002-use-wpf-and-mvvm.md) | Use WPF with MVVM | Accepted |
| [0003](0003-use-sqlite.md) | Use SQLite with EF Core | Accepted |
| [0004](0004-document-numbering.md) | Assign document numbers on first save | Accepted |
| [0005](0005-preserve-document-snapshots.md) | Preserve document snapshots | Accepted |
| [0006](0006-use-recorded-payments.md) | Use recorded payments instead of a Paid checkbox | Accepted |
| [0007](0007-use-explicit-date-time-policy.md) | Use explicit business-date and UTC timestamp semantics | Accepted |
| [0008](0008-void-incorrect-payments.md) | Void incorrect payments without deleting history | Accepted |
| [0009](0009-use-pdfsharp.md) | Use PDFsharp for local PDF generation | Accepted |

## Adding a Decision

Create the next numbered Markdown file using this structure:

```markdown
# ADR-NNNN: Decision title

Status: Proposed | Accepted | Superseded by ADR-NNNN

## Context

## Decision

## Consequences
```

An accepted ADR is not rewritten to conceal a changed decision. Add a new ADR and mark the old one as superseded.
