# Testing Strategy

## Goals

Tests protect business behavior, architecture boundaries, and persistence integrity. The target is 50–100 meaningful tests rather than a vanity coverage percentage.

## Test Levels

### Domain Tests

Fast, deterministic tests with no database or UI dependencies:

- line and document calculations;
- VAT and rounding behavior;
- payment totals and outstanding balance;
- paid and overdue status rules;
- number formatting rules;
- quote and invoice invariants;
- snapshot behavior represented in the domain.

### Application Tests

Use cases tested with controlled fakes or mocks at external boundaries:

- creating and archiving customers;
- creating and converting quotations;
- creating and updating invoices;
- assigning a number on first save;
- registering partial and full payments;
- rejecting invalid or excessive payments;
- generating dashboard summaries;
- invoking PDF generation with the correct snapshot data.

### Infrastructure Tests

Integration tests against temporary SQLite databases:

- entity relationships and mappings;
- unique constraints and sequence allocation;
- decimal persistence;
- snapshot persistence;
- UTC audit timestamp and date-only business-date persistence;
- cascade behavior;
- repository queries and archive filters;
- migrations from an empty database;
- transaction behavior for number allocation and quote conversion.

The M1 infrastructure suite also verifies application-directory creation, controlled local-date calculation, ISO business-date conversion, UTC audit timestamp normalization, and application of the complete initial migration to SQLite in memory.

### UI Tests

The MVP prioritizes testable view models and manual workflow verification. Automated WPF UI testing may be added when it provides clear value, especially for navigation or critical editor behavior.

## Critical Scenarios

| Scenario | Expected result |
|---|---|
| Invoice total 1,000; payment 400 | Outstanding 600; status is not Paid |
| Invoice total 1,000; payment 1,000 | Outstanding 0; status Paid |
| Outstanding 600; attempted payment 700 | Payment rejected; persisted state unchanged |
| Sent invoice due yesterday; outstanding 100 | Status Overdue |
| Draft invoice due yesterday | Status remains Draft |
| Cancelled invoice due yesterday | Status remains Cancelled |
| Company, customer, or service edited after issue | Existing issuer, customer, and item snapshots unchanged |
| Historical PDF regenerated after company edit | Persisted issuer snapshot is used |
| Same UTC instant evaluated with a controlled local zone | `Today` and overdue result are deterministic |
| New unsaved document editor cancelled | No number consumed |
| First quote and invoice in same year | Independent `Q` and `INV` sequences |
| Concurrent number allocation | Unique, ordered persisted numbers |

## Test Conventions

- Test names describe behavior and expected result.
- Arrange, Act, and Assert sections remain visually clear.
- Tests control UTC instants and local business dates through an injected clock.
- Decimal expectations are explicit.
- One test should fail for one understandable reason.
- Shared test builders may reduce noise but must not hide important inputs.
- Database tests use isolated temporary files or isolated connections and clean up their own resources.
- Tests do not depend on execution order, local user data, or network access.

## CI Quality Gate

On every pull request and push to the protected branch, CI restores packages, builds with warnings monitored, and runs all automated tests on Windows. A pull request is not ready to merge when build or tests fail.

## Manual Release Verification

Before `v1.0.0`, verify on a clean Windows environment:

1. Install or unpack and launch the release build.
2. Complete the full company-to-payment workflow.
3. Restart and verify persistence.
4. Generate and visually inspect quote and invoice PDFs.
5. Verify partial, full, excessive, and overdue payment cases.
6. Confirm logs and local data are stored in the documented directory.

## Definition of Done for a Feature

A feature is complete when acceptance criteria are met, relevant tests are added or updated, the full applicable suite passes, documentation reflects behavior, and CI succeeds.
