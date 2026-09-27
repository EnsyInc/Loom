# Loom — Work Breakdown

Ordered phases for building out everything in
[high-level-design.md](high-level-design.md) and
[loom-data-model.md](loom-data-model.md) beyond the `Project` CRUD that's
already implemented. Phases are ordered by dependency, not necessarily by
priority — re-sequence freely, but respect the "depends on" lines.

## How to read a phase

Every entity gets the same vertical slice `Project` already established:
entity (`DataAccess/Models`) + EF config (`DataAccess/Configuration`) +
mapper (`DataAccess/Mappers`) + repo interface/impl (`DataAccess/Abstractions`
/ `Implementations`) + migration, then service interface/impl
(`Services/Abstractions` / `Implementations`) returning `Result<T>`, then
request/response DTOs + validators + mapper + controller (`Api/`), then
ServiceTests per endpoint (feature folder + `*ApiTestBase`), then UnitTests
only for what ServiceTests can't reach (see the table in the root
`CLAUDE.md`). Each phase below only calls out what's *new or different* from
that template — assume the full slice unless noted.

Every table also gets the standard `DbEntity` columns (`Id`, `CreatedAt`,
`UpdatedAt`, `DeletedAt`) and soft delete, per `loom-data-model.md`.

---

## Phase 0 — Project (done)

`Project` CRUD: entity, repo, service, controller, migration, Unit + Service
tests. Reference implementation for every phase below.

---

## Phase 1 — Work item type templates (done)

**Depends on:** nothing (no FK to `User` or `WorkItem`).
**Unblocks:** Phase 3 (work items need a type, status, and fields to exist).

Build in this order — each step's FK target must exist first:

1. **`TplWorkItemStatus`** — `name` (unique), `category`
   (`ToDo | InProgress | Done` — model as a C# enum, not a free string).
   Standalone CRUD.
2. **`TplWorkItem`** — `name` (unique), `iconUrl`. Create without
   `initialStatusId` first (nullable at the DB level, or created in a
   "draft" state) since a type has no valid status until it uses at least
   one — **decide the exact creation workflow**: either (a) `initialStatusId`
   is nullable until the type is fully configured, or (b) creation is a
   single endpoint that takes the initial set of statuses plus which one is
   initial, in one call. The data model's invariant ("`initialStatusId` is
   one of the statuses the type uses") has to hold the instant the row
   becomes usable either way.
3. **`Project ↔ TplWorkItem` link table** — plain join table (no extra
   columns, per the data model's "many-to-many link tables are not drawn"
   convention). Endpoint(s) on `Project` or `TplWorkItem` to opt a project in
   / out of a type. Decide **open question 3** (removing a type from a
   project while live items use it) before building the "opt out" path —
   simplest default: block it (409) if any non-deleted `WorkItem` in the
   project still uses the type.
4. **`TplWorkItem ↔ TplWorkItemStatus` link table** — which statuses a type
   uses. Adding/removing a status from a type must keep `initialStatusId`
   valid (can't remove the initial status while it's still initial) and
   must not orphan any `StatusTransition` row for that type.
5. **`StatusTransition`** — `typeId`, `fromStatusId`, `toStatusId`, unique on
   `(typeId, fromStatusId, toStatusId)`, `fromStatusId <> toStatusId`. Both
   statuses must already be in the type's status set (step 4).
6. **`TplWorkItemField`** — `typeId`, `key` (immutable after creation),
   `label`, `dataType` (enum: `Text | Number | Date | Bool | Option |
   MultiOption | User`), `required`, `defaultValue` (JSON). Unique on
   `(typeId, key)`. Renaming is `label`-only — the update DTO should not
   accept `key`.
7. **`TplWorkItemFieldOption`** — `fieldId`, `value`, `label`, `rank`. Unique
   on `(fieldId, value)`. Service-layer guard: only creatable for a field
   whose `dataType` is `Option` or `MultiOption`.

**Not in scope here (explicitly deferred by the design doc):** transition
guards (open question 8) and comments (open question 10) — no modeling
until automation rules are designed.

---

## Phase 2 — Auth (Microsoft Entra ID) & `User` (done)

**Depends on:** nothing structurally, but do this before Phase 3 —
`WorkItem.createdById` is a required FK to `User`, so a real user must exist
before a work item can be created.

- `User` entity — `entraObjectId` (unique, the Entra `oid` claim),
  `firstName`, `lastName`, `email`. No password/credential storage; identity
  comes entirely from the OIDC token.
- Entra ID app registration (external, one-time setup — not code) +
  OAuth2/OIDC middleware in `Api/Bootstrap/BootstrappingExtensions.cs`
  (`AddAuthentication().AddJwtBearer(...)` against Entra's OIDC metadata).
- On first sign-in with an unseen `entraObjectId`, provision a `User` row
  (upsert-on-login), rather than a separate "register" endpoint.
- `[Authorize]` on all controllers except health/swagger. This is also the
  point where `ProjectsController` and every controller from Phase 1 needs
  `[Authorize]` added retroactively.
- Minimal `GET /users/me` endpoint (needed by the frontend and by later
  phases that need "who am I" for `createdById`/`assignedToId`).
- ServiceTests need a way to run authenticated — add a test-only token
  issuer or a fake JWT signed with a test key that the test `Api` trusts in
  the `Development`/`Testing` environment only (mirror however Enclave
  solved this, if it already has Entra auth).

Full permissions (**open question 9** — who can edit types vs. just work
items) is explicitly out of scope until more users are added; this phase is
authentication only, not authorization. Track the permissions model as
Phase 8, not here.

---

## Phase 3 — Work items core

**Depends on:** Phase 1 (type/status/field definitions), Phase 2 (`User`
for `createdById`/`assignedToId`).

- **`WorkItem`** — `projectId`, `typeId`, `statusId`, `parentId` (nullable),
  `assignedToId` (nullable), `createdById`, `number`, `title`,
  `description`, `points` (nullable). `sprintId` stays out until Phase 6.
  - `number` is assigned from `Project.nextItemNumber`, incremented in the
    same transaction as the insert (per the data model's concurrency
    invariant) — this is the one place a repo method needs to do more than
    the `BaseRepository<T>` default, so it's a candidate for a UnitTest
    (can't provoke the race through the API, but can verify the
    increment-and-insert happens atomically).
  - Unique on `(projectId, number)`.
  - Service-layer validation: type is one of the project's opted-in types;
    status is one of the type's statuses; new items start at the type's
    `initialStatusId`.
  - Readable key (`{Project.key}-{WorkItem.number}`, e.g. `LOOM-123`) —
    compute on read/response mapping, not stored.
- **Status transitions** — a dedicated `PATCH /work-items/{id}/status` (or
  similar) endpoint that checks the move is a `StatusTransition` row for the
  item's type, rather than allowing status to be set via a general update.
- **Parent/child** — setting `parentId` validates: not self, same project
  (unless **open question 2** decides cross-project parents are allowed —
  default to same-project only, since nothing in the design doc says
  otherwise), and no cycle in the ancestor chain (service walks up before
  saving — this cycle check is another UnitTest candidate, since triggering
  it via the API means first building a chain deep enough to matter, which
  ServiceTests *can* do but a UnitTest isolates the logic more cheaply).
- **`WorkItemField`** (custom field values) — one row per set field,
  exactly one typed value column populated (`valueText` / `valueNumber` /
  `valueDate` / `valueBool` / `valueOptionId` / `valueUserId`), matching the
  field's `dataType`. This is validated in the service layer (a DB check
  constraint can only enforce "exactly one column set", not "matches the
  field's declared type").
  - Unique on `(workItemId, fieldId)`, except `MultiOption` fields where
    `(workItemId, fieldId, valueOptionId)` is unique (one row per selected
    option).
  - Every `required` field of the item's type must have a value before the
    item is considered valid — decide whether this is enforced at create
    time only or also blocks transitioning out of the initial status.
  - Indexes on `(fieldId, valueOptionId)`, `(fieldId, valueNumber)`,
    `(fieldId, valueDate)`, `(fieldId, valueUserId)` for filtering.
- **List/filter endpoint** — `GET /work-items` with filters (project,
  status, assignee, type, sprint once Phase 6 lands) ordered by
  `category, name` by default. This is the "board and table views are
  projections over the same data" requirement from the design doc — no
  separate kanban endpoint, just this one with different client-side
  rendering.

---

## Phase 4 — Relationships

**Depends on:** Phase 3 (`WorkItem` must exist).

- **`WorkItemRelType`** — `name` (unique), `inverseName` (nullable — null
  means symmetric). Standalone CRUD, global (not scoped to a project).
- **`WorkItemRel`** — `typeId`, `sourceId`, `targetId`. Unique on
  `(typeId, sourceId, targetId)`, `sourceId <> targetId`.
  - Symmetric-type invariant: store one canonical row with the lower id as
    source, so `A relates-to B` and `B relates-to A` can't both exist — this
    normalization happens in the service before insert.
  - Cross-project links need no extra structure (validated against the
    item's *type*, not its project), so no project-scoping check here.
  - **Decide open question 5** before or during this phase: restrict which
    work item type pairs may use a given relationship type, or allow any
    pair (current design doc default: any pair, no restriction modeled).

---

## Phase 5 — Work item history

**Depends on:** Phase 3 (needs `WorkItem` to have something to log against).
Do this before or alongside Phase 6 — sprint burndown reads from it.

- **Resolve open question 1** first (it blocks the schema):
  1. Grouped per save (`WorkItemRevision` + `WorkItemRevisionChange`) vs.
     one flat change table.
  2. Store referenced ids only (rename-safe reads recompute names; renames
     rewrite old history) vs. snapshot names at write time.
- Whatever shape is chosen: creation is logged as the first revision, and
  the service writes the item and its revision(s) in the same transaction
  as the mutation — this pairing is what makes it a UnitTest concern too
  (verifying the write happens atomically isn't reachable by asserting on
  HTTP responses alone; a ServiceTest can still assert the resulting history
  is queryable and correct end-to-end).
- Read endpoint: `GET /work-items/{id}/history`.
- No update/delete on history rows — append-only, so no `UnitTests` for
  "delete history" because there is no such operation.

---

## Phase 6 — Sprints

**Depends on:** Phase 3 (`WorkItem`), Phase 5 (burndown needs history).

- **`Sprint`** — `projectId`, `name`, `startDate`, `endDate`, `capacity`
  (points). CRUD scoped under a project.
  - **Decide open question 4**: is sprint state (planned / active / closed)
    derived from dates, or stored explicitly? And can two sprints in the
    same project overlap? Pick one before building "start sprint" /
    "close sprint" endpoints — a stored state needs those endpoints; a
    derived state needs none, just date validation.
- **`WorkItem.sprintId`** — nullable FK added to the existing `WorkItem`
  entity (backlog = null). Pulling an item into a sprint is an explicit
  action (`PATCH /work-items/{id}/sprint` or similar), not implicit from
  sprint dates.
- **Sprint close / carryover** — unfinished items (status category not
  `Done`) get their `sprintId` moved to the next sprint (or back to
  backlog if there is no next sprint). This is the one multi-item mutation
  in the whole backlog — needs its own service method, and is a strong
  UnitTest candidate for the "which items moved where" logic, with a
  ServiceTest covering the end-to-end happy path.
- **Burndown endpoint** — derived from item status-category + points over
  the sprint window, using Phase 5's history to know when each item joined
  the sprint and when it reached `Done`.

---

## Phase 7 — Saved queries

**Depends on:** Phase 3 (built-in fields to filter on) and its custom-field
indexes.

- **Open question 6** says this is explicitly deferred until querying
  itself is designed — Phase 3's `GET /work-items` filter endpoint *is* that
  design in practice, so revisit this phase once that endpoint has real
  usage to model against, rather than speculatively designing a query
  language now.
- Minimal version once ready: a `SavedQuery` entity (owned by a `User`,
  scoped to a project) storing the same filter shape `GET /work-items`
  already accepts, plus a name. No new query engine — it's a named,
  persisted version of an existing filter.

---

## Phase 8 — Permissions

**Depends on:** Phase 2 (`User`). Should land before real multi-user usage,
per the design doc ("permissions model ... should exist before more people
are added") — **open question 9**.

- No project-membership or role table exists yet. Minimum viable model:
  a `ProjectMember` link (`projectId`, `userId`, `role`) with at least two
  roles (e.g. "can edit types" vs. "can only edit work items" — matching
  the design doc's own example distinction).
- Retrofit `[Authorize]` checks in Phase 1/3/4/6 controllers to also check
  role where the design doc implies a distinction (type/status/relationship
  *type* CRUD is more privileged than work item CRUD).
- This phase touches a lot of existing controllers — do it as one pass
  across all of them rather than piecemeal, so the authorization story is
  consistent.

---

## Phase 9 — MCP layer

**Depends on:** everything above being stable — the design doc is explicit
that this is "built after the API is stable so its tool schemas mirror the
API's DTOs."

- Thin wrapper tools mirroring the REST API: `create_work_item`,
  `transition_status`, `link_items`, `query_items`, etc.
- No new domain logic — each tool calls the existing service layer (or the
  HTTP API, depending on whether the MCP server runs in-process or
  out-of-process — decide that when this phase starts, informed by whatever
  Enclave does if it already has an MCP layer).

---

## Explicitly out of scope for this breakdown

Carried over from `loom-data-model.md`'s open questions, deferred by the
design doc itself and not scheduled into a phase above:

- **Transition guards** (open question 8) — e.g. "assignee must be set
  before moving to In Progress." Not modeled until automation rules are
  designed.
- **Comments** (open question 10) — implied by "requires a comment" as a
  transition guard, so blocked on the same decision.
- **Board layouts** (open question 7) — per-user column order/saved views.
  The design doc suggests this can start as a front-end-only concern; only
  becomes a backend entity if a layout needs to follow a user across
  devices or be shared.
