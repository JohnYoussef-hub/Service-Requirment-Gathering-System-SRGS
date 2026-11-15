# SRGS — Service Requirement Gathering System

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-Language-239120?style=for-the-badge&logo=csharp)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-blue?style=for-the-badge)
![Status: In Progress](https://img.shields.io/badge/Status-In%20Progress-yellow?style=for-the-badge)

An internal **Requirement Gathering System** built for Orange Egypt's Service Management Assurance department, using **.NET 10**, **Clean Architecture**, **CQRS**, and **Domain-Driven Design**. It centralizes how internal service/module requirements — issues, changes, new requests, integrations — get logged, approved, developed, and tracked through to closure.

This project is under active development. See [Project Status](#project-status) before assuming any feature below is finished.

---

## Table of Contents

- [SRGS — Service Requirement Gathering System](#srgs--service-requirement-gathering-system)
  - [Table of Contents](#table-of-contents)
  - [Project Overview](#project-overview)
  - [Project Status](#project-status)
  - [Architecture \& Patterns](#architecture--patterns)
    - [Design patterns in use](#design-patterns-in-use)
  - [Tech Stack](#tech-stack)
  - [Domain Model](#domain-model)
  - [Request Workflow](#request-workflow)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Clone the repository](#clone-the-repository)
    - [Configure the connection string](#configure-the-connection-string)
    - [Apply database migrations](#apply-database-migrations)
    - [Run the API](#run-the-api)
    - [LDAP implementation handoff](#ldap-implementation-handoff)
    - [Seed lookup data](#seed-lookup-data)
  - [Known Gaps \& Open Decisions](#known-gaps--open-decisions)

---

## Project Overview

SRGS replaces ad-hoc requirement tracking with a single system that:

- Logs requests against a **request type** (Issue, Change, New Req., Integration) and an **impacted module** (Dashboard, Incident, Change, Problem)
- Routes each request through **approval** (Stakeholder / Managerial) before development starts
- Tracks a request's full lifecycle — priority, status, phase, assigned developer/BA, effort, UAT result — from logging to closure
- Keeps an **audit trail** of every change and a **notification** log per request
- Enforces role-based access via a real, admin-managed **Role** table (not a hardcoded enum)

---

## Project Status

**What's built and working:**
- Full domain model (`SRGS.Domain`) — all 12 aggregates/entities, enums backed by real `CHECK` constraints, domain events
- `CreateRequest` command end-to-end: command → validator → handler → EF Core → SQL Server → API response
- `GetRequestById` query with a name-resolving projection (request type / requester / module names, not just IDs)
- Request management commands for updating and deleting a request, restricted to its requester
- `GetAllRequests` query with name-resolving projections and cache invalidation on request changes
- Attachment commands and queries for uploading, updating, listing, downloading, and deleting request files
- BRD request workflow with role-based transitions, approval decisions, developer assignment, clarification responses, status history, and optimistic concurrency
- Identity/authentication slice: LDAP abstraction, first-login user provisioning, JWT issuance, `[Authorize]` protection, claims-based `RequestedById`, and refresh-token rotation with family-based reuse detection
- EF Core configurations mapping every entity to its exact DDL column, including the `request_code` computed column and all `CHECK` constraints
- Domain event dispatch through MediatR on `SaveChangesAsync`
- Integration tests covering the authentication flow against a LocalDB-backed test host

**Not built yet:**
- Filtered request views
- Frontend
- API versioning (deliberately deferred — see reasoning below)

Treat this README as describing the architecture and how to run what exists today, not a finished product.

---

## Architecture & Patterns

```
┌─────────────────────────────────────────────┐
│                  SRGS.Api                   │  ← Presentation Layer
│              (Controllers)                  │
├─────────────────────────────────────────────┤
│              SRGS.Application                │  ← Application Layer
│      (CQRS Commands/Queries via MediatR)     │
├─────────────────────────────────────────────┤
│             SRGS.Infrastructure               │  ← Infrastructure Layer
│         (EF Core, SQL Server, Caching)       │
├─────────────────────────────────────────────┤
│                SRGS.Domain                    │  ← Domain Layer
│      (Entities, Enums, Domain Events)        │
└─────────────────────────────────────────────┘
```

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** | `SRGS.Domain` | Aggregates, entities, enums, domain events, business rules. No dependencies on any other layer. |
| **Application** | `SRGS.Application` | CQRS commands/queries via MediatR, FluentValidation validators, DTOs, mappers, `IAppDbContext` abstraction. Depends only on Domain. |
| **Infrastructure** | `SRGS.Infrastructure` | EF Core `AppDbContext`, entity configurations, SQL Server provider, HybridCache. Implements Application's interfaces. |
| **Api** | `SRGS.Api` | ASP.NET Core Web API — controllers, DI composition root, Swagger. |

### Design patterns in use

- **CQRS** — commands and queries are separate MediatR requests, never a shared "do everything" handler
- **Result pattern** — domain and application operations return `Result<T>` instead of throwing; the API maps `Error.Type` to the right HTTP status via a shared `ApiController.Problem()` helper
- **Domain events** — e.g. `RequestCreated`, `RequestStatusChanged`, `ApprovalDecided` — dispatched through a MediatR pipeline on `SaveChangesAsync`, before the underlying insert/update actually commits
- **Pipeline behaviors** — `ValidationBehavior` runs every registered FluentValidation validator automatically before a handler executes
- **Explicit join mapping** — `USER_ROLE` is modeled as a plain `UserRoleRow` persistence shape, synced manually against `User.RoleIds` rather than exposed as an EF Core many-to-many, since `User` deliberately holds only role IDs, not `Role` references

---

## Tech Stack

| Technology | Purpose |
|---|---|
| **.NET 10 / C#** | Runtime and language |
| **ASP.NET Core** | Web API |
| **Entity Framework Core** | ORM, SQL Server provider |
| **MediatR** | CQRS mediator + domain event dispatch |
| **FluentValidation** | Command/query input validation |
| **HybridCache** | Response caching, invalidated by tag on writes |
| **SQL Server** | Relational database |

---

## Domain Model

Full detail lives in `SRGS.Domain/README.md`. Summary:

| Table | Entity | Notes |
|---|---|---|
| USER | `User` | Surrogate INT key; AD `username` stays a unique column, never the PK |
| ROLE | `Role` | Admin-managed lookup, not an enum |
| USER_ROLE | *(no entity)* | `User.RoleIds`, synced via `UserRoleRow` |
| REQUEST | `Request` | Aggregate root; `RequestCode` is a DB-computed, read-only column |
| REQUEST_TYPE / MODULE_TYPE | `RequestType` / `ModuleType` | Admin-managed lookups |
| ATTACHMENT | `Attachment` | |
| DESCRIPTION | `RequestNote` | Renamed in code — a type called `Description` next to `Request.Description` was confusing |
| APPROVAL | `Approval` | |
| CHANGE_HISTORY | `ChangeHistory` | Append-only audit log; workflow rows include old/new status, actor, note, and UTC timestamp |
| NOTIFICATION | `Notification` | |
| REFRESH_TOKEN | `RefreshToken` | Only entity using a GUID key and full audit columns |

**Enums backed by a real `CHECK` constraint:** `RequestStatus`, `RequestPriority`, `UatResult`, `ApprovalType`, `ApprovalDecisionStatus`, `ChangeOperation`, `ChangeType`, `NotificationStatus`.

`RequestPhase` remains a separate process-stage value and is intentionally not merged with `RequestStatus`.

## Request Workflow

The request lifecycle is defined in `SRGS.Domain/Requests/RequestWorkflow.cs`. The allowed
transitions are centralized in one table and checked against the actor's JWT role claims:

```text
Submitted -> UnderReview              BA
Submitted -> Cancelled                Requester
UnderReview -> ClarificationRequired  BA
UnderReview -> Analysis               BA
UnderReview -> Rejected               BA
ClarificationRequired -> UnderReview  Requester
ClarificationRequired -> Cancelled    Requester
Analysis -> ManagementApproval        BA
Analysis -> ClarificationRequired     BA
Analysis -> Rejected                  BA
ManagementApproval -> StakeholdersApproval  Business Approver
ManagementApproval -> ClarificationRequired  Business Approver
ManagementApproval -> Rejected               Business Approver
StakeholdersApproval -> Assigned             Business Approver
StakeholdersApproval -> ClarificationRequired Business Approver
StakeholdersApproval -> Rejected              Business Approver
Assigned -> InDevelopment              Developer
InDevelopment -> InternalTesting        Developer
InternalTesting -> UAT                  Developer
InternalTesting -> InDevelopment        Developer
UAT -> Deployed                         Developer
UAT -> InDevelopment                    Requester
Deployed -> Closed                      BA or Requester
```

`Rejected`, `Cancelled`, and `Closed` are terminal statuses. Rejection, cancellation, and
clarification-required transitions require a non-empty note.

### Workflow endpoints

All workflow endpoints require `[Authorize]`. The actor ID and roles come from
`ICurrentUser`; they are never accepted as trusted request-body fields.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/requests/{id}/transition` | Transition without additional workflow data |
| `POST` | `/api/requests/{id}/assign-developer` | Assign a developer and transition to `Assigned` |
| `POST` | `/api/requests/{id}/approval` | Create and decide a managerial or stakeholder approval |
| `POST` | `/api/requests/{id}/clarification-response` | Respond and transition `ClarificationRequired` to `UnderReview` |

Successful transitions invalidate the `request` cache tag. Each transition publishes
`RequestStatusChanged`; the MediatR handler adds one `CHANGE_HISTORY` row in the same
`SaveChangesAsync` operation.

`REQUEST.row_version` is a SQL Server `rowversion` concurrency token. Concurrent updates
return a clear conflict instead of silently overwriting a request.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running SQL Server instance (LocalDB, Express, or full SQL Server)

### Clone the repository

```bash
git clone https://github.com/JohnYoussef-hub/Service-Requirment-Gathering-System-SRGS.git
cd Service-Requirment-Gathering-System-SRGS
```

### Configure the connection string

Set it in `SRGS.Api/appsettings.Development.json` (not `appsettings.json` — see note below):

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=RequirementGatheringSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

`appsettings.Development.json` is gitignored on purpose. Create it locally when needed, or use `dotnet user-secrets`. Never put SQL-auth passwords, JWT secrets, LDAP bind passwords, or other real credentials in any committed `appsettings*.json` file.

### Apply database migrations

```bash
dotnet ef database update --project src/SRGS.infrastructure/SRGS.infrastructure.csproj --startup-project src/SRGS.Api/SRGS.Api.csproj
```

The workflow migration is `20261115201929_AddRequestWorkflowPersistence`. It safely maps
legacy status values, adds the `REQUEST.status` CHECK constraint, adds `row_version`, and
adds status columns to `CHANGE_HISTORY`. Review the SQL before applying it with:

```bash
dotnet ef migrations script 20261004184019_AddRefreshTokenFamilies 20261115201929_AddRequestWorkflowPersistence ^
  --project src/SRGS.infrastructure/SRGS.infrastructure.csproj ^
  --startup-project src/SRGS.Api/SRGS.Api.csproj
```

### Run the API

```bash
dotnet run --project SRGS.Api
```

Swagger UI: `https://localhost:<port>/swagger`

### LDAP implementation handoff

The only production integration that remains to be implemented is `ILdapService`:

- Interface: `src/SRGS.Application/Common/Interfaces/ILdapService.cs`
- Contract: `AuthenticateAsync(string username, string password)` returns `Task<Result<LdapUserDto>>`. Return a successful `LdapUserDto` for an authenticated directory user, or an appropriate failed `Result` for invalid credentials or an unavailable directory.
- DTO: `src/SRGS.Application/Features/Identity/Dtos/LdapUserDto.cs`. Keep its fields and semantics stable; `IdentityService` uses it for first-login provisioning and profile refresh.
- Placeholder: `src/SRGS.infrastructure/Identity/PlaceholderLdapService.cs`. Replace its registration/implementation with the real LDAP service; it currently accepts any non-empty credentials for local development only.
- Options: `LdapSettings` in `src/SRGS.infrastructure/Identity/LdapOptions/LdapSettings.cs`, bound from the `LdapSettings` configuration section. It provides `Server`, `Port`, `BaseDn`, `UseSsl`, `BindUsername`, and `BindPassword` placeholders. Configure real values in local `appsettings.Development.json` (which is gitignored) or user-secrets/environment variables, not in committed `appsettings.json`.

After implementing LDAP, run the full test suite:

```bash
dotnet test SRGS.slnx
```

The workflow-specific checks can be run with:

```bash
dotnet test tests/SRGS.Domain.Tests/SRGS.Domain.Tests.csproj
dotnet test tests/SRGS.Infrastructure.Tests/SRGS.Infrastructure.Tests.csproj --filter FullyQualifiedName~RequestWorkflowIntegrationTests
```

The integration coverage is in `tests/SRGS.Infrastructure.Tests/Integration/AuthenticationFlowTests.cs`; it currently replaces the production service with a fake implementation in `SrgsApiFactory`. Keep those tests passing while adding real LDAP-specific tests separately.

### Seed lookup data

`REQUEST_TYPE` and `MODULE_TYPE` need at least one row each before `CreateRequest` will succeed:

```sql
INSERT INTO REQUEST_TYPE (type_name) VALUES ('Integration'), ('New Req.'), ('Change'), ('Issue');
INSERT INTO MODULE_TYPE (module_name) VALUES ('Dashboard'), ('Incident'), ('Change'), ('Problem');
```

---

## Known Gaps & Open Decisions

Tracked here instead of scattered across commit messages:

- **Workflow rules by approval type and impact** — intentionally out of scope for now.
- **Notifications, dashboards, reports, effort, and test-result fields** — intentionally out of scope for now.
- **`STATUS_HISTORY`** was removed from the schema; status timestamps are now stored in `CHANGE_HISTORY` for workflow transitions.
- **FK columns had no index** in the original DDL (SQL Server doesn't auto-index them) — added in the EF configurations, but worth double-checking the generated migration.
- **API versioning deferred on purpose** — no external consumers yet, and the domain model is still actively changing. Add `Asp.Versioning` once a first contract is genuinely stable or a second consumer exists.
