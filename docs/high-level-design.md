# Loom — Backend Design (High-Level)

A schema-driven work tracker: work item types, statuses, and relationships are
data the user defines, not hardcoded concepts. API + a small MCP layer on top,
dockerized, Microsoft SSO for auth.

## Core concepts

Loom is single-tenant: one deployment serves one team, and there is no
organization/tenant concept. Templates live at the app level, and projects are
the top-level container for work.

- **Project** — a container for work items. Adopts templates from the
  app-level template library.
- **Work item** — an instance of a type, holding a `status`, a `fields` blob
  (validated against its type's field definitions), and typed relationships
  to other work items (including items in other projects).

## Templates (app-level)

Three kinds of template, each **copied into a project on adoption** rather than
referenced live (copy-on-use with lineage back to the source template/version).
This lets a project diverge (extra field, tweaked transition) without forking
the whole template, and protects live projects from a later template edit.

- **Work item type template** — name, icon, field definitions (key, label,
  type: text / number / date / enum / user / link / richtext, required,
  default, validation).
- **Status workflow template** — a directed graph of statuses and allowed
  transitions (not just linear), with optional guards (e.g. "assignee must
  be set", "requires a comment").
- **Relationship type template** — name (blocks, parent-of, relates-to),
  cardinality (1:1 / 1:N / N:N), directional or symmetric, and which type
  pairs are allowed to use it.

## Auth

- Microsoft Entra ID (Azure AD) app registration, OAuth2/OIDC. Users are
  identified by their Entra ID identity; there is no organization to belong to.
- Multi-user from the start; single-user today, team later — permissions
  model (who can edit templates vs. just work items) should exist before
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
- Backlog items get pulled into a sprint explicitly; burndown is derived
  from item status + points over the sprint window.

## Queries

- Saved, named filters over predefined fields (assignee, status, project,
  priority, etc.) for now; a real query language is a possible later
  upgrade once usage patterns are clear.

## API & MCP

- REST API is the source of truth: CRUD on items, types, workflows,
  relationships, transitions, sprints, queries.
- MCP server is a thin wrapper over the same API (`create_work_item`,
  `transition_status`, `link_items`, `query_items`, …), built after the API
  is stable so its tool schemas mirror the API's DTOs.

## Deployment

- Local Docker Compose to start; cloud hosting (Azure, matching existing
  EnsyLabs infra) later.
