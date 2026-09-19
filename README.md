# MechanicShop

A backend API for running an auto repair shop's day-to-day operations: scheduling repair bays, assigning mechanics, tracking work orders through their lifecycle, and billing customers.

Built with **.NET 9**, following **Clean Architecture** and **Domain-Driven Design**, with CQRS via **MediatR**.

## What it does

- **Scheduling** — the shop has a fixed number of bays ("spots") and operating hours. Work orders are booked into a spot and time slot, and the system prevents double-booking a spot, a mechanic ("labor"), or a vehicle for overlapping times.
- **Work orders** — track repair jobs through a state machine: `Scheduled → InProgress → Completed` (or `Cancelled`). Repair tasks and parts can be added while a work order is still editable (i.e. still `Scheduled`).
- **Customers & vehicles** — CRUD for customers and the vehicles they bring in.
- **Repair tasks & parts** — a catalog of repairable services, each with an estimated duration, labor cost, and associated parts.
- **Billing** — invoices are generated from a work order's labor and parts costs, and can be issued, settled, and exported as PDF.
- **Real-time updates** — a SignalR hub (`/hub/workorders`) pushes work order changes to connected clients.
- **Identity & auth** — JWT-based auth (with refresh tokens) and two roles, `Labor` and `Manager`.
- **Dashboard** — aggregate stats for the day's work orders.
- **Background jobs** — an `OverdueBookingCleanupService` periodically cancels work orders that were never started past a configurable threshold.

## Architecture

The solution follows Clean Architecture's dependency rule (dependencies point inward, toward `Domain`):

```
src/
├── MechanicShop.Domain          Core entities, value objects, domain events, business rules
├── MechanicShop.Application     Use cases (CQRS commands/queries + handlers), validation, behaviors
├── MechanicShop.Contracts       DTOs/request-response shapes shared with API consumers
├── MechanicShop.Infrastructure  EF Core, Identity, caching, PDF generation, SignalR, background jobs
└── MechanicShop.Api             ASP.NET Core Web API — controllers, DI wiring, middleware
```

Dependency direction: `Api → Infrastructure → Application → Domain`, with `Contracts` referenced by `Api`.

### Notable patterns

- **CQRS with MediatR** — every use case is a `Command`/`Query` + `Handler` under `MechanicShop.Application/Features/<Feature>/`, each paired with a FluentValidation validator.
- **Pipeline behaviors** — cross-cutting concerns (`ValidationBehavior`, `LoggingBehavior`, `CachingBehavior`, `PerformanceBehavior`, `UnhandledExceptionBehavior`) wrap every request through MediatR's pipeline.
- **Result pattern** — domain and application logic return a `Result<T>` type instead of throwing for expected failure cases, with a centralized `GlobalExceptionHandler` for the rest.
- **Rich domain model** — entities like `WorkOrder` enforce their own invariants (e.g. a work order can only be modified while `IsEditable`, i.e. still `Scheduled`).
- **Domain events** — e.g. `WorkOrderCompleted`, `WorkOrderCollectionModified`, handled via MediatR notification handlers.
- **Auditable entities** — created/modified metadata applied automatically via an EF Core `SaveChanges` interceptor (`AuditableEntityInterceptor`).

## Tech stack

| Concern | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core Web API |
| Data access | Entity Framework Core 9 (SQL Server) |
| Mediator / CQRS | MediatR 13 |
| Validation | FluentValidation |
| Auth | ASP.NET Core Identity + JWT Bearer (access + refresh tokens) |
| Real-time | SignalR |
| Caching | `Microsoft.Extensions.Caching.Hybrid` (in-memory + distributed) |
| PDF generation | QuestPDF |
| Logging | Serilog (console + Seq sink) |
| Observability | OpenTelemetry (ASP.NET Core, HTTP, Prometheus exporter, OTLP exporter) → Prometheus + Grafana |
| API docs | OpenAPI via Swashbuckle, with Scalar UI in development |
| API versioning | Asp.Versioning |
| Testing | xUnit — unit tests (Domain, Application) + subcutaneous tests using Testcontainers.MsSql + `WebApplicationFactory` |

## Running it

### Option 1 — Docker Compose (recommended)

`docker-compose.yml` spins up the API plus its full observability stack:

```bash
docker compose up --build
```

This starts:

| Service | URL |
|---|---|
| API | `http://localhost:5001` |
| SQL Server | `localhost:1433` |
| Seq (logs) | `http://localhost:8081` |
| Prometheus | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

> The compose file has development credentials hardcoded (SQL `sa` password, Grafana admin password). Fine for local use — change them before deploying anywhere real.

### Option 2 — Run locally with `dotnet`

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download), a SQL Server instance.

```bash
dotnet restore

# apply EF Core migrations / create the database
dotnet ef database update --project src/MechanicShop.Infrastructure --startup-project src/MechanicShop.Api

# run the API
dotnet run --project src/MechanicShop.Api
```

Update `src/MechanicShop.Api/appsettings.Development.json` (or use user secrets) first, particularly:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:Secret` — a strong, private signing key
- `Serilog:WriteTo` (Seq `serverUrl`) — or remove the sink if unused

Shop-level behavior is configurable under `AppSettings`: `OpeningTime`/`ClosingTime`, `MaxSpots`, `MinimumAppointmentDurationInMinutes`, `BookingCancellationThresholdMinutes`, `OverdueBookingCleanupFrequencyMinutes`, pagination/cache defaults, `AllowedOrigins`.

In development, the app auto-initializes/seeds the database on startup and exposes Swagger UI (launch default), Scalar, the OpenAPI spec at `/openapi/v1.json`, and the SignalR hub at `/hub/workorders`.

### `.http` files

`src/MechanicShop.Api/MechanicShop.Api.http` and `Identity.http` contain sample requests you can run directly from VS Code/Rider/Visual Studio's REST client.

## Tests

```bash
dotnet test
```

- **`MechanicShop.Domain.UnitTests`** / **`MechanicShop.Application.UnitTests`** — pure unit tests against domain rules and handlers.
- **`MechanicShop.Application.SubcutaneousTests`** — end-to-end-ish tests through `WebApplicationFactory` against a real SQL Server instance via Testcontainers (requires Docker running).
- **`MechanicShop.Tests.Common`** — shared test builders/fixtures.

## Project structure (Application layer)

Features are organized vertically by domain area rather than by technical layer:

```
Features/
├── Billings/       Invoice issuing, settlement, PDF export
├── Customers/       Customer + vehicle CRUD
├── Dashboard/        Aggregate work order stats
├── Identity/         Token generation/refresh, user lookup
├── Labor/            Mechanic/labor lookups
├── RepairTasks/       Repair task + part CRUD
├── Scheduling/        Daily schedule / availability queries
└── WorkOrders/        Create/relocate/assign/update state/delete
```

Each command/query folder typically contains three files: the request (`*Command`/`*Query`), its `Handler`, and its FluentValidation `Validator`.

## Roles

- **Manager** — administrative operations (customers, employees, repair task catalog, invoicing).
- **Labor** — mechanics assigned to work orders.

Authorization policies (see `MechanicShop.Infrastructure/Identity/Policies`) further restrict certain actions, e.g. only the labor assigned to a work order (or a manager) may act on it.
