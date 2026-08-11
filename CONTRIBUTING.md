# Contributing

## Language

Repository content is written in English: source code, identifiers, UI text, documentation, issues, pull requests, commits, tests, comments, screenshots, and releases. Project discussions may be conducted in other languages when appropriate.

## Workflow

1. Create or select a GitHub Issue with a clear goal and acceptance criteria.
2. Create a focused branch using the issue number.
3. Inspect related code, documentation, business rules, and tests.
4. Implement the smallest coherent change.
5. Add or update tests and documentation.
6. Run the relevant test suite and build.
7. Open a pull request and wait for CI before merge.

`main` must remain buildable and tested.

## Branch Names

```text
feat/12-create-customer
fix/31-payment-validation
docs/18-update-architecture
refactor/27-invoice-calculation
```

## Commit Messages

Use a concise conventional prefix and describe the outcome:

```text
feat: add customer creation workflow
fix: prevent invoice overpayment
test: add invoice status tests
docs: document payment business rules
refactor: extract invoice calculation service
```

Avoid messages such as `update`, `changes`, `fix`, or `final`.

## Pull Request Checklist

- Acceptance criteria are satisfied.
- The change respects documented architecture and business rules.
- Tests were added or updated and pass locally.
- The solution builds without new unexplained warnings.
- User-visible or architectural behavior is documented.
- No secrets, private customer data, generated databases, or local logs are committed.
- The pull request is focused and references its issue.

## Architecture Changes

Do not silently change accepted decisions. Update the relevant documentation and add a new ADR that supersedes the previous decision when architecture or foundational behavior changes.

## Definition of Done

An issue is done only when implementation, tests, build, review, documentation, acceptance criteria, and CI are complete.
