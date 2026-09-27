# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Memory

At the start of each session, call `mcp__memory__read_graph` to load accumulated project context (decisions, findings, patterns).

During the session, save important information using:
- `mcp__memory__create_entities` — new concepts, components, decisions
- `mcp__memory__add_observations` — new findings about existing entities
- `mcp__memory__create_relations` — relationships between entities

At the end of significant work, save key takeaways to the graph.

## Project Overview

GorzdravBooking is a **demo project** that automates doctor appointment booking via the Gorzdrav system. It uses a **fake API client** (`services.AddFakeGorzdravClient()`) instead of the real one because the real service has no ToS. The commented-out real client is in `Infrastructure/Http/HttpClientFactorySetup.cs`.

## Commands

### Backend (.NET)

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with output
dotnet test --verbosity normal

# Run a single test project
dotnet test tests/UnitTests/Application.Tests/

# Run the CLI application
dotnet run --project src/Presentation/CLI

# Run the Web API
dotnet run --project src/Presentation/Server

# Apply EF migrations manually (auto-applied on startup)
dotnet ef database update --project src/Infrastructure --startup-project src/Presentation/Server
```

### Frontend (React)

Working directory: `src/Presentation/Web/gorzdrab-booking/`

```bash
npm install       # install dependencies
npm run dev       # dev server on http://localhost:5173
npm run build     # production build
npm run lint      # ESLint
```

### Docker

```bash
docker compose up --build
```

## Architecture

Clean Architecture with 4 layers. Dependency direction: Presentation → Application → Core ← Infrastructure.

### Layer responsibilities

- **Core** — entities, interfaces, domain events, external API models, enums. Zero external dependencies.
- **Application** — services, use cases, coordinators, background worker, DTOs. Depends only on Core.
- **Infrastructure** — EF Core + SQLite, repositories, external HTTP services, InMemoryEventBus, BCrypt, JWT auth helpers.
- **Presentation/Server** — ASP.NET Core Web API with JWT auth, FluentValidation, Swagger.
- **Presentation/CLI** — console app with StatefulMenu, runs the same Application/Infrastructure layers.

### Key patterns

**Result pattern** — all service/use case methods return `Result<T>` or `Result`. Never throw exceptions for business errors. Use `Error.Failure("Code", "message")`, `Error.NotFound(...)`, `Error.Conflict(...)`. The first argument is the error code (not a stack trace).

**Auto-registration via Scrutor** — implement `IAppService` to auto-register a service as Scoped. Implement `IAppUseCase` to auto-register a use case as Scoped. No manual DI registration needed.

**Coordinator pattern** — `AppointmentCoordinator` orchestrates multi-service booking flow (fetch slots → filter by preferences → book → retry up to 3 times). Lives in Application layer.

**InMemoryEventBus** — registered as Singleton. Events: `SearchRequestStarted`, `SearchRequestCompleted`, `NextSearchScheduled`, `SearchServiceStatusChanged`. Used for CLI UI updates.

**Background worker** — `AppointmentSchedulerWorker` runs `CheckAppointmentSearchRequestsUseCase` every 1 minute via `IServiceProvider` scope per iteration.

### Booking flow

`AppointmentSchedulerWorker` → `CheckAppointmentSearchRequestsUseCase` → `AppointmentCoordinator.CreateCompleteAppointmentAsync` → fetches slots from external API → `TryGetPreferAppointment` filters by `TimePreferencesPreset` → books via external API → saves to DB.

### Authentication (Server only)

JWT tokens. `[RequireUserClaims]` attribute on controllers extracts `UserId` into `HttpContext.Items["UserId"]`. Authorization checks ownership via `IAuthorizationProvider.CanAccessAsync<T>(userId, resourceId)`.

### Database

SQLite, file `GorzdravBooking.db` in working directory. Migrations auto-applied on startup. EF entity configurations are in `Infrastructure/Persistence/Configurations/`.

### AppointmentSearchRequest hierarchy

Abstract base `AppointmentSearchRequest` with two concrete types: `ManualSearchRequest` (by doctor/specialty) and `ReferralSearchRequest` (by referral number). TPH inheritance in EF.

### TimePreferences hierarchy

Abstract base `TimePreference` with `WeekDayPreference` and `MonthDayPreference`. Grouped into named presets per user.

## Known quirks

- `(bool IsSucces, int ErrorCode)` — `IsSucces` is intentionally spelled this way throughout the external service interface and all usages; don't "fix" it.
- `|| true` in `CheckAppointmentSearchRequestsUseCase` filter is intentional (disables time-gating during development).
- CORS is hardcoded to `http://localhost:5173` (Vite dev port) in `Server/Program.cs`.
- The fake API client returns stub data — all external service calls succeed with mock responses.

## Known Issues (code analysis, 2026-02-27)

### Critical

- **`InMemoryEventBus.cs:12`** — `_semaphore.Wait()` is a sync-over-async call; can cause deadlock. Should use `WaitAsync()`.
- **`AppSettingsService.cs:44`** — `throw new Exception(userId.Error.Description)` breaks Result pattern; should return `Result` with error.
- **`AppointmentCoordinator.cs:134-135, 166-167`** — throws `InvalidOperationException` / `NotSupportedException` instead of returning `Result<T>`.
- **`Infrastructure/Services/ExternalAppointmentService.cs:29,43,55,58,69,79`** — throws `HttpRequestException` directly instead of returning `Result<T>`. Same pattern likely in other External*Service files.

### Important

- **`AppointmentCoordinator.cs:97-99`** — result of `appointmentService.CreateAsync()` is not checked; if DB save fails, the appointment is still considered booked successfully.
- **`PatientService.cs:125`** — uses `Error.Conflict()` instead of `Error.NotFound()` when patient is not found — wrong error semantics.
- **`AppointmentSearchRequest.cs:22,25`** — `CreatedAt = DateTime.UtcNow` and `Id = Guid.NewGuid()` initialized as field defaults, not in constructor; EF Core may overwrite correctly but pattern is fragile.
- **`CheckAppointmentSearchRequestsUseCase.cs:29`** — `|| true` disables time-gating (intentional for dev, but tracked here as known issue).

### Medium

- **`AppointmentCoordinator`** — SRP violation: handles slot fetching, preference filtering, retry logic, and two search modes in one class.
- **`AppointmentSearchRequestExtensions.cs:29`** — string interpolation with `null!` fields may produce "null null null" strings.
- **`PatientService.cs:37`** — implicit `Guid` → `Result<Guid>` conversion; verify implicit operator covers this correctly.
- **`UpdatePreferencesDto.cs`** — `List<DateTime>? SpecificStartPoints` nullable but used as required in validator — inconsistency.
