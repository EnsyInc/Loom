# Loom — Data Model

Entity-relationship diagram of the core backend model. Two design choices
worth calling out before the diagram:

- **Templates and project-level instances share the same tables.**
  `WorkItemType`, `StatusWorkflow`, and `RelationshipType` each have a
  nullable `projectId`,
  set means "this is what a project actually uses." A project's copy
  points back at its template via `sourceTemplateId` (copy-on-use with
  lineage), instead of needing a separate template/instance schema.
- **Relationships between work items are just edge rows.** `Relationship`
  has a `sourceItemId` and `targetItemId`, both pointing at `WorkItem`,
  typed by `RelationshipType` — this is what makes cross-project links
  possible for free (the two items just need to live in different
  projects; nothing else changes).

```mermaid
%%{init: {'theme': 'dark'}}%%
erDiagram
    PROJECT ||--o{ WORKITEM : contains
    PROJECT ||--o{ WORKITEMTYPE : "adopts (instances)"
    PROJECT ||--o{ STATUSWORKFLOW : "adopts (instances)"
    PROJECT ||--o{ RELATIONSHIPTYPE : "adopts (instances)"
    PROJECT ||--o{ SPRINT : schedules

    WORKITEMTYPE ||--o{ WORKITEMTYPE : "copied from"
    WORKITEMTYPE ||--o{ FIELDDEFINITION : has
    WORKITEMTYPE ||--o{ WORKITEM : "instances of"

    STATUSWORKFLOW ||--o{ STATUSWORKFLOW : "copied from"
    STATUSWORKFLOW ||--o{ STATUS : has
    STATUSWORKFLOW ||--o{ STATUSTRANSITION : has
    STATUS ||--o{ WORKITEM : "current status of"
    STATUS ||--o{ STATUSTRANSITION : "from"
    STATUS ||--o{ STATUSTRANSITION : "to"

    RELATIONSHIPTYPE ||--o{ RELATIONSHIPTYPE : "copied from"
    RELATIONSHIPTYPE ||--o{ RELATIONSHIP : types

    WORKITEM ||--o{ RELATIONSHIP : "source of"
    WORKITEM ||--o{ RELATIONSHIP : "target of"
    USER ||--o{ WORKITEM : "assigned to"

    SPRINT ||--o{ SPRINTITEM : includes
    WORKITEM ||--o{ SPRINTITEM : "scheduled in"

    USER {
        uuid id PK
        string name
        string email
    }
    PROJECT {
        uuid id PK
        string name
    }
    WORKITEMTYPE {
        uuid id PK
        uuid projectId FK "null for templates"
        uuid sourceTemplateId FK "null for templates"
        string name
        string icon
    }
    FIELDDEFINITION {
        uuid id PK
        uuid workItemTypeId FK
        string key
        string label
        string dataType
        bool required
    }
    STATUSWORKFLOW {
        uuid id PK
        uuid projectId FK "null for templates"
        uuid sourceTemplateId FK "null for templates"
        string name
    }
    STATUS {
        uuid id PK
        uuid workflowId FK
        string name
        string color
        bool isTerminal
    }
    STATUSTRANSITION {
        uuid id PK
        uuid workflowId FK
        uuid fromStatusId FK
        uuid toStatusId FK
        string guard "nullable"
    }
    RELATIONSHIPTYPE {
        uuid id PK
        uuid projectId FK "null for templates"
        uuid sourceTemplateId FK "null for templates"
        string name
        string cardinality
        bool directional
    }
    WORKITEM {
        uuid id PK
        uuid projectId FK
        uuid workItemTypeId FK
        uuid statusId FK
        uuid assigneeId FK "nullable"
        string title
        json fields
    }
    RELATIONSHIP {
        uuid id PK
        uuid relationshipTypeId FK
        uuid sourceItemId FK
        uuid targetItemId FK
    }
    SPRINT {
        uuid id PK
        uuid projectId FK
        string name
        date startDate
        date endDate
        int capacityPoints
    }
    SPRINTITEM {
        uuid id PK
        uuid sprintId FK
        uuid workItemId FK
        int points
    }
```

## Notes

- `WorkItem.fields` is a JSON blob validated at write-time against its
  type's `FieldDefinition` rows — this is what lets custom fields exist
  without a schema migration every time someone adds one.
- `Status.isTerminal` marks states a workflow doesn't expect to transition
  out of (e.g. "Done"), used to drive board styling and burndown logic.
- `StatusTransition.guard` is a placeholder for the rule engine (e.g.
  "assignee must be set") — worth a proper guard/condition model once
  automation rules are designed.
