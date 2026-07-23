# SRGS.Domain

Domain layer for the Requirement Gathering System, built to match the `MechanicShop.Domain`
patterns 1:1: `Entity<TId>` / `AuditableEntity<TId>` base types, private constructors with
`CS8618` suppression, `static Result<T> Create(...)` factories, a static `XErrors` class per
entity, `Enums/` and `Events/` subfolders per feature.

Two differences from MechanicShop, both deliberate:
- `Entity<TId>` is generic. MechanicShop hardcodes `Guid` because every Id is client-generated.
  SRGS mostly uses `INT IDENTITY` (DB-generated), so the base type needs to work for both —
  `RefreshToken` is the one entity that stays on `Guid`, matching the comment already in your DDL.
- Several lookup tables (`ROLE`, `REQUEST_TYPE`, `MODULE_TYPE`) are real entities, not enums,
  because they're admin-managed tables in your schema, not a fixed set baked into code.

## Table → entity map

| Table | Entity | Folder |
|---|---|---|
| USER | `User` | `Users/` |
| ROLE | `Role` | `Roles/` |
| USER_ROLE | *(no separate entity — `User.RoleIds`)* | `Users/` |
| REQUEST | `Request` (aggregate root) | `Requests/` |
| REQUEST_TYPE | `RequestType` | `Requests/Lookups/` |
| MODULE_TYPE | `ModuleType` | `Requests/Lookups/` |
| ATTACHMENT | `Attachment` | `Requests/Attachments/` |
| DESCRIPTION | `Description` | `Requests/Notes/` |
| APPROVAL | `Approval` | `Requests/Approvals/` |
| CHANGE_HISTORY | `ChangeHistory` | `Requests/History/` |
| NOTIFICATION | `Notification` | `Requests/Notifications/` |
| REFRESH_TOKEN | `RefreshToken` | `Identity/` |

## Enums that are genuinely fixed (backed by an existing `CHECK` constraint)

- `RequestPriority` — `CK_REQUEST_priority`
- `UatResult` — `CK_REQUEST_uat`
- `ApprovalType` — `CK_APPROVAL_TYPE_decision`
- `ApprovalDecisionStatus` — `CK_APPROVAL_decision`
- `ChangeOperation` — `CK_CHANGE_operation`
- `ChangeType` — `CK_CHANGE_type`
- `NotificationStatus` — `CK_NOTIFICATION_status`

## Enums that are my best guess — please double-check against the BRD

`REQUEST.status` and `REQUEST.phase` have no `CHECK` constraint yet (you already flagged this
as a pending schema improvement). I inferred `RequestStatus` and `RequestPhase` from the
default values (`'Under Analysis'`, `'Logging'`) and the tables that already exist
(Approval, UAT). `Request.CanTransitionTo(...)` encodes the state machine I assumed — treat
both the enum values and the transitions as a draft, not ground truth, until you confirm them.
`Notification.EventType` stays a plain `string` for the same reason (no `CHECK` constraint).

## Domain events

Following the `WorkOrderCompleted` / `WorkOrderCollectionModified` pattern:
- `Request`: `RequestCreated`, `RequestStatusChanged`, `RequestPhaseChanged`,
  `RequestAssignedToDeveloper`, `RequestAssignedToBusinessAnalyst`, `RequestCompleted`,
  `RequestCollectionModified` (generic "refresh the list" signal for SignalR/dashboard).
- `Approval`: `ApprovalDecided` — a handler for this is the natural place to auto-advance
  `Request.Status` once all required approvals are in, and to spin up a `Notification`.

## Result pattern

`Common/Results/*` is an exact copy of what's already in `SRGS.Domain.Common.Results` per your
notes — included here for completeness so the folder is self-contained. If your existing copy
already has the "four fixes" you mentioned, keep that one and just drop these four files.

## Not yet modeled (left for the Application layer)

- Validation of FK existence (e.g. does `RequestTypeId` actually exist) — that's a DB/repository
  concern, not something the aggregate can check on its own.
- The junction row behavior of `USER_ROLE` beyond membership — if you need e.g. "who granted
  this role and when," that data belongs on the join row and `User.RoleIds` won't be enough;
  say so and I'll add a proper `UserRole` entity instead.
