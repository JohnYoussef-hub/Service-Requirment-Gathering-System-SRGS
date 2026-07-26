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

- [Project Overview](#project-overview)
- [Project Status](#project-status)
- [Architecture & Patterns](#architecture--patterns)
- [Tech Stack](#tech-stack)
- [Domain Model](#domain-model)
- [Getting Started](#getting-started)
- [Known Gaps & Open Decisions](#known-gaps--open-decisions)

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
- EF Core configurations mapping every entity to its exact DDL column, including the `request_code` computed column and all `CHECK` constraints
- Domain event dispatch through MediatR on `SaveChangesAsync`

**Not built yet:**
- Authentication/authorization (JWT is modeled in the domain via `RefreshToken`, not wired up yet)
- Every other command/query beyond Create/GetById (Update, Assign, Approve, list/filter views, attachments, notes, notifications)
- Frontend
- Automated tests
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
| CHANGE_HISTORY | `ChangeHistoryEntry` | Append-only audit log, no mutation methods |
| NOTIFICATION | `Notification` | |
| REFRESH_TOKEN | `RefreshToken` | Only entity using a GUID key and full audit columns |

**Enums backed by a real `CHECK` constraint:** `RequestPriority`, `UatResult`, `ApprovalType`, `ApprovalDecisionStatus`, `ChangeOperation`, `ChangeType`, `NotificationStatus`.

**Enums that are a best-guess draft, not yet confirmed against the BRD:** `RequestStatus`, `RequestPhase` — the DB has no `CHECK` constraint for either yet.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running SQL Server instance (LocalDB, Express, or full SQL Server)

### Clone the repository

```bash
git clone https://github.com/JohnYoussef-hub/srgs-learn.git
cd srgs-learn
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

`appsettings.Development.json` is gitignored on purpose. If the connection string ever needs a SQL-auth password instead of Windows auth, use `dotnet user-secrets` instead of putting it in any `appsettings*.json` file.

### Apply database migrations

```bash
dotnet ef database update --project SRGS.Infrastructure --startup-project SRGS.Api
```

### Run the API

```bash
dotnet run --project SRGS.Api
```

Swagger UI: `https://localhost:<port>/swagger`

### Seed lookup data

`REQUEST_TYPE` and `MODULE_TYPE` need at least one row each before `CreateRequest` will succeed:

```sql
INSERT INTO REQUEST_TYPE (type_name) VALUES ('Integration'), ('New Req.'), ('Change'), ('Issue');
INSERT INTO MODULE_TYPE (module_name) VALUES ('Dashboard'), ('Incident'), ('Change'), ('Problem');
```

---

## Known Gaps & Open Decisions

Tracked here instead of scattered across commit messages:

- **`RequestStatus`/`RequestPhase`** — inferred from defaults, not confirmed against the BRD. No `CHECK` constraint yet.
- **`STATUS_HISTORY`** was removed from the schema; SLA/aging reporting has no data source yet.
- **FK columns had no index** in the original DDL (SQL Server doesn't auto-index them) — added in the EF configurations, but worth double-checking the generated migration.
- **API versioning deferred on purpose** — no external consumers yet, and the domain model is still actively changing. Add `Asp.Versioning` once a first contract is genuinely stable or a second consumer exists.
- **Authentication not wired up** — `RefreshToken`/JWT are modeled in the domain but there's no login endpoint yet. Until then, `RequestedById` on `CreateRequestCommand` is client-supplied, which is a real integrity gap — it should come from the authenticated user's claims once auth exists.
