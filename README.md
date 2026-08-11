# Invoice Manager

Invoice Manager is a Windows desktop application for freelancers and small businesses that need a focused way to manage customers, services, quotations, invoices, payments, and basic financial reporting.

> Project status: Milestone 1 complete. The host, dependency injection, logging, SQLite persistence, initial migration, WPF shell, navigation, tests, and CI foundation are in place. Business features begin in Milestone 2.

## Product Scope

The MVP supports one company profile and one currency (EUR). Its core workflow is:

```text
Configure company -> Create customer -> Create service -> Create quote
-> Accept quote -> Convert to invoice -> Generate PDF -> Register payment
-> Update dashboard
```

Invoice Manager is not intended to replace accounting software or an ERP system.

## Planned Features

- Customer management with archiving and search
- Product and service catalog with deactivation
- Quote creation, status tracking, and conversion to an invoice
- Invoice creation, lifecycle management, and PDF export
- Partial and full payment recording with overpayment protection
- Automatic paid and overdue state calculation
- Dashboard KPIs, recent invoices, and monthly revenue chart
- Local SQLite persistence and company branding settings

## Technology

| Area | Technology |
|---|---|
| Runtime | .NET 10 LTS |
| Language | C# |
| Desktop UI | WPF |
| UI pattern | MVVM with CommunityToolkit.Mvvm |
| Persistence | Entity Framework Core 10 and SQLite |
| Hosting and DI | Microsoft.Extensions.Hosting |
| Testing | xUnit |
| CI | GitHub Actions on Windows |

## Architecture

The application follows a layered architecture:

```text
InvoiceManager.App            WPF views, view models, navigation
        |
InvoiceManager.Application    Use cases and ports
        |
InvoiceManager.Domain         Entities, value objects, business rules

InvoiceManager.Infrastructure implements persistence, PDF, and storage ports
```

The Domain project has no dependencies. Business logic belongs in Domain or Application, never in WPF views, view models, or persistence code. See [Architecture](docs/architecture.md) and the [decision records](docs/decisions/README.md).

## Repository Layout

```text
src/        production projects
tests/      automated test projects
docs/       product and engineering documentation
.github/    CI and collaboration templates
```

The planned detailed layout is documented in [Architecture](docs/architecture.md).

## Getting Started

### Prerequisites

- Windows 10 or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), version `10.0.302` or a compatible later patch

### Restore, Build, and Test

```powershell
dotnet tool restore
dotnet restore InvoiceManager.sln
dotnet build InvoiceManager.sln --no-restore --configuration Release
dotnet test InvoiceManager.sln --no-build --configuration Release
```

### Run the Desktop App

```powershell
dotnet run --project src/InvoiceManager.App/InvoiceManager.App.csproj
```

On startup, the application creates its directories and applies pending EF Core migrations automatically. The current UI provides the navigable product shell; customer and service workflows are the next implementation milestone.

## Local Data

Runtime data is stored under `%LocalAppData%/InvoiceManager`:

```text
invoice-manager.db
logs/invoice-manager.log
assets/
```

Generated PDFs will be exported separately to a location selected by the user.

## Testing

The project targets 50–100 meaningful tests covering financial calculations, document numbering, snapshots, payments, status transitions, use cases, database constraints, and migrations. See the [Testing Strategy](docs/testing-strategy.md).

## Documentation

- [Project overview](docs/project-overview.md)
- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Domain model](docs/domain-model.md)
- [Business rules](docs/business-rules.md)
- [Testing strategy](docs/testing-strategy.md)
- [Roadmap](docs/roadmap.md)
- [Portfolio case study](docs/case-study.md)
- [Architecture decisions](docs/decisions/README.md)
- [Contributing](CONTRIBUTING.md)
- [Changelog](CHANGELOG.md)

## Roadmap

Development is split into seven milestones, from foundation through customers, quotes, invoices, payments, PDF generation, dashboard reporting, and portfolio polish. The target release is `v1.0.0`. See the complete [Roadmap](docs/roadmap.md).

## License

A project license has not been selected yet. Until a license file is added, all rights are reserved.
