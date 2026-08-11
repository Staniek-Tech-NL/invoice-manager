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

## Planned Releases

- `0.1.0` — foundation
- `0.5.0` — core workflow through payments
- `1.0.0` — portfolio release
