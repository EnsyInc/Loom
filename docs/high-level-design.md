# Loom — Backend Design (High-Level)

A schema-driven work tracker: work item types, statuses, and relationships are
data the user defines, not hardcoded concepts. API + a small MCP layer on top,
dockerized, Microsoft SSO for auth.

## Core concepts

Loom is single-tenant: one deployment serves one team, and there is no
organization/tenant concept. Work item types, statuses, and relationship
types are defined once at the app level, and projects are the top-level
container for work. The full model is in
[loom-data-model.md](loom-data-model.md).

- **Project** — a container for work items, with a short key (e.g. `LOOM`)
  that prefixes its item keys (`LOOM-123`). Opts into the work item types it
  uses.
- **Work item** — an instance of a type, holding a status, custom field
  values (validated against its type's field definitions), an optional
  parent, and typed relationships to other work items (including items in
  other projects).

## Work item types (app-level)

Types are **shared definitions, used by reference**: a project opts into a
type rather than getting its own copy. Editing a type changes it for every
project that uses it, and a project cannot customize a type on its own. In
exchange there is no copy lineage, no template versioning, and no question
of which project's copy a cross-project link uses.

- **Work item type** — name, icon, field definitions (key, label, data type,
  required, default). Initial data types: text / number / date / bool /
  option / multi-option / user. Option fields have an ordered list of
  allowed values, so they can be sorted and compared (Low < Medium < High).
- **Statuses and workflow** — statuses are shared across types, and each has
  a fixed category (to do / in progress / done) that the app uses for boards,
  burndown, and "open items" queries. Each type defines its own directed
  graph of allowed transitions between statuses (not just linear) and the
  status new items start in. Transition guards (e.g. "assignee must be
  set", "requires a comment") are planned but not modeled yet.
- **Relationship types** — global, with a name and an optional inverse name
  (blocks / is blocked by); no inverse name means the type is symmetric
  (relates-to). Parent/child is not a relationship type: it is built in, and
  each item has at most one parent. Restricting which pairs of work item
  types may use a relationship type is a possible later addition.

## Auth

- Microsoft Entra ID (Azure AD) app registration, OAuth2/OIDC. Users are
  identified by their Entra ID identity; there is no organization to belong to.
- Multi-user from the start; single-user today, team later — permissions
  model (who can edit types vs. just work items) should exist before
  more people are added.

## Work items & relationships

- Cross-project relationships are just edges where source/target live in
  different projects; the relationship engine validates against the item's
  *type*, not its project.
- Board and table views are both projections over the same status-graph
  data — no separate "kanban" concept to maintain.

## Sprints

- A sprint is a scoped, time-boxed subset of a project's work items
  (capacity in points, start/end dates).
- Backlog items get pulled into a sprint explicitly; an item is in at most
  one sprint at a time, and unfinished items carry over to the next one.
- Burndown is derived from item status category + points over the sprint
  window. That needs history (when items joined the sprint and when they
  reached done), which comes from the planned work item change history.

## Work item history

- Every change to a work item is recorded: who changed what, and when.
  This gives per-item history and is the source for burndown and past-sprint
  reports. Planned once the core model is in place.

## Queries

- Saved, named filters over built-in fields (assignee, status, project,
  sprint, etc.) and custom fields (e.g. priority) for now; a real query
  language is a possible later upgrade once usage patterns are clear.
- Custom field values are stored in typed, indexed columns so these filters
  don't scan every item.

## API & MCP

- REST API is the source of truth: CRUD on items, types, statuses,
  transitions, relationship types, relationships, sprints, queries.
- MCP server is a thin wrapper over the same API (`create_work_item`,
  `transition_status`, `link_items`, `query_items`, …), built after the API
  is stable so its tool schemas mirror the API's DTOs.

## Deployment

- Local Docker Compose to start; cloud hosting (Azure, matching existing
  EnsyLabs infra) later.
