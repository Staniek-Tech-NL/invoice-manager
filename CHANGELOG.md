# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project intends to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial product, requirements, architecture, domain, business rule, testing, roadmap, and decision documentation.
- Historical issuer snapshot requirements and an explicit date/time policy for persistence.
- .NET 10 solution foundation with WPF, layered projects, test projects, centralized build settings, and package versions.
- Milestone 1 host, dependency injection, local logging, SQLite persistence, initial migration, WPF navigation shell, CI workflow, and repository templates.
- Integration tests for application paths, date/time persistence, the application clock, and the initial SQLite schema.
- A direct safe native SQLite dependency replacing the vulnerable transitive version reported by NuGet audit.
- Customer creation, editing, searching, and safe archiving across the domain, application, persistence, and WPF layers.
- Product and service creation, editing, searching, and safe deactivation across the domain, application, persistence, and WPF layers.
- Milestone 2 validation, use-case, repository, filtering, and wildcard-search tests.
- Company profile settings used to create stable issuer snapshots.
- Quotation creation, editing, line-item management, search, lifecycle transitions, and automatic expiration.
- Centralized line rounding, persisted customer/issuer/item snapshots, transactional yearly numbering, and concurrent allocation coverage.
- Milestone 3 domain, application, and SQLite integration tests, bringing the suite to 50 tests.
- Direct invoice creation, editing, search, item management, status controls, and independent yearly numbering.
- Atomic one-time accepted quote-to-invoice conversion with copied snapshots and a unique persisted source link.
- A schema migration adding the nullable, unique invoice-to-source-quote relationship.
- Milestone 4 transaction, rollback, snapshot, numbering, domain, application, and SQLite tests, bringing the suite to 68 tests.
- Partial and full payment registration with paid and outstanding amounts on invoices.
- Derived Paid and Overdue status, overpayment rollback, and concurrency-safe payment registration.
- Auditable one-time payment voiding with a required reason and retained history.
- A payment-voiding migration, ADR-0008, and Milestone 5 coverage bringing the suite to 86 tests.
- PDFsharp-based quotation and invoice export with user-selected destinations and visually verified A4 layouts.
- Snapshot-persisted company logos with an EF Core migration and ADR-0009.
- Dashboard KPIs, active record counts, recent invoices, and a 12-month payment-revenue chart.
- Milestone 6 application and infrastructure coverage bringing the suite to 95 tests.

### Changed

- Refreshed the README, project specification, and case study to reflect the completed Milestone 2 implementation.
- Updated product and engineering documentation with the completed Milestone 3 behavior and rounding policy.
- Updated documentation with the completed Milestone 4 invoice and conversion behavior.
- Updated documentation and milestone reporting with the completed Milestone 5 payment behavior.
- Updated architecture and product documentation with the completed Milestone 6 PDF and dashboard behavior.

## Planned Releases

- `0.1.0` — foundation
- `0.5.0` — core workflow through payments
- `1.0.0` — portfolio release
