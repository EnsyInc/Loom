# EnsyInc.Loom

Backend for Loom, a schema-driven work tracker: work item types, statuses, and relationships are data the user defines, not hardcoded concepts. It is **single-tenant** — there is no organization/tenant concept. See [docs/high-level-design.md](docs/high-level-design.md) for the design.

**Implemented:** `Project` (CRUD at `/projects`), end to end — entity, repo, service, controller, migration, tests. **Designed but not built:** templates, work items, relationships, sprints, saved queries, Microsoft Entra ID auth, and the MCP layer.

Loom is modeled on the sibling repo `../Enclave` (EnsyInc.Enclave). When unsure how something should be done, look at how Enclave does it — but copy its conventions, not its domain.

## Solution layout

All projects live under `src/EnsyInc.Loom/` (solution file: `EnsyInc.Loom.slnx`):

- `EnsyInc.Loom.Api` — controllers, request/response models, validators, exception handling, and `Bootstrap/BootstrappingExtensions.cs` (config, logging, DI, pipeline; there is no separate Bootstrap project)
- `EnsyInc.Loom.Services` — application logic (`I*Service` interfaces, `internal` implementations, `AddApplicationServices`)
- `EnsyInc.Loom.Core` — domain models, errors, config (`DbConfig`)
- `EnsyInc.Loom.DataAccess` — **one project** (no abstractions/EF split): entities (`Models/`), entity↔domain mappers (`Mappers/`), repo interfaces (`Abstractions/`), `internal` repo implementations (`Implementations/`), EF entity configuration (`Configuration/`), `LoomDbContext`, and `AddDataAccess`
- `EnsyInc.Loom.Migrations` — standalone runner that applies EF Core migrations (the migrations themselves live in its `Migrations/` folder)
- `EnsyInc.Loom.UnitTests` / `EnsyInc.Loom.ServiceTests` — see Testing below

## Conventions

- **Result pattern, not exceptions, for expected failures.** Services return `Result<T>` / `Result` (from the `EnsyNet.Core` NuGet package) wrapping domain `Error` records (`Core/Errors/`). Controllers `switch` on `{ HasError, Error }` and map known error cases to HTTP responses. The `_` default arm throws `UnhandledResultErrorException` — this signals a missing switch arm (a real bug), and `GlobalExceptionHandler` logs it distinctly.
- **Repos are thin.** Each is a subclass of `BaseRepository<T>` (from `EnsyNet.DataAccess.EntityFramework`), which provides get/list/insert/update/soft- and hard-delete. Deletes are soft (`DeletedAt`) and idempotent, and soft-deleted rows are filtered out of reads. `LoomDbContext` has query tracking disabled because all mutations bypass the change tracker.
- **`UpdatedAt` is populated on creation** (it is not null until the first update). The `EnsyNet` package XML docs say otherwise — trust observed behavior. Don't assert it is null after create, and don't document it as null-until-updated.
- **C# 14 `extension(...)` blocks** are used for mapper/bootstrapping extension methods. Any class with 2+ extension blocks trips a Roslyn `CA1708` false positive — suppress it at the class level with `[SuppressMessage("Naming", "CA1708:...", Justification = "...")]`, matching the existing files.
- **`.editorconfig` is strict** — many Roslyn/CA/IDE rules are `error` and warnings are errors, so a local `dotnet build` fails on style violations. Common ones: unused `using`s (IDE0005), file-scoped namespaces, and namespaces must match the folder path.
- **Central package management.** Versions live in `Directory.Packages.props`; `PackageReference`s in csprojs carry no `Version`. Add the version there first.
- Request DTOs with non-nullable value-type properties (enums, etc.) should be annotated `[property: JsonRequired]` to avoid silent under-posting defaults.

## Testing

Default to **ServiceTests**. Add a **UnitTest** only for logic a black-box HTTP test genuinely can't reach.

Ask: *"Can I make this happen through the HTTP API against a real database?"* Yes → ServiceTest. No → UnitTest.

| Situation | Test type |
|---|---|
| Endpoint behavior: status codes, response bodies, validation errors, not-found, idempotent delete, soft-deleted rows being hidden | **ServiceTests** |
| Behavior spanning several calls that the API can drive (create → update → get) | **ServiceTests** |
| A repo/DB failure being mapped to `UnexpectedError` (can't provoke a DB failure through the API) | **UnitTests** |
| A race between two steps inside one service call (e.g. update after a successful existence check — the row vanishes in between) | **UnitTests** |
| Internal call-sequencing only visible via mock `Verify` | **UnitTests** |
| Rule-heavy logic with many branches that is awkward to drive through the API (future: workflow transition guards, relationship cardinality, field validation) | **UnitTests** |

Never write a UnitTest that duplicates a ServiceTest scenario — the ServiceTest is the more trustworthy of the two.

**ServiceTests** (`ServiceTests/`): black-box HTTP tests against a real running Api and a real SQL Server.
- One class per endpoint under a feature folder (e.g. `Projects/CreateProjectTests.cs`), inheriting the feature's `*ApiTestBase`, which tracks created ids and deletes them on dispose.
- Mark classes `[Collection(ApiCollectionDefinition.Name)]` and give test data unique names (`$"Something-{Guid.NewGuid()}"`) so tests never collide.
- Assert only what the test is about and what the API is known to do, not incidental fields.
- Prefer creating data through the API. Seed via direct SQL only when the API can't produce the state (no SQL seeder exists yet).

**UnitTests** (`UnitTests/`): mock the repo interfaces with Moq and test `internal` services directly (Services grants `InternalsVisibleTo`). Keep the header comment on each class stating that it only covers what ServiceTests can't reach.

```sh
dotnet test src/EnsyInc.Loom/EnsyInc.Loom.UnitTests/EnsyInc.Loom.UnitTests.csproj
dotnet test src/EnsyInc.Loom/EnsyInc.Loom.ServiceTests/EnsyInc.Loom.ServiceTests.csproj
```

ServiceTests need SQL Server on `localhost:1433` with migrations applied and the Api running at `ApiBaseUrl` (default `https://localhost:7088`, overridable via an `ApiBaseUrl` environment variable). `docker compose up --build` provides all of it.

## Build

```sh
dotnet build src/EnsyInc.Loom/EnsyInc.Loom.slnx -c Release
```

## Running locally / Docker

- `Dockerfile` (in `src/EnsyInc.Loom`) is multi-stage with two final targets: `api` and `migrations`. The build context must be `src/EnsyInc.Loom`; see `docker-compose.yml` at the repo root.
- Api ports: `7088` (https) / `5029` (http); Swagger UI at `/swagger`. These differ from Enclave's so both Apis can run at once.
- **SQL Server stays on the default host port `1433`, shared with Enclave.** Run a single db instance and point several Apis at it (each project has its own database: `Loom`, `Enclave`). Don't remap the db port to dodge a clash.
- Configuration comes from the `Db:ConnectionString` setting (env var `Db__ConnectionString`). Dev defaults are in `appsettings.Development.json`.

## CI / SonarCloud

CI (`.github/workflows/ci.yml`, adapted from Enclave's) runs on push/PR to `main` and on manual dispatch: build → unit-tests + service-tests (coverage collected via `dotnet-coverage`) → SonarCloud analysis (org `ensyinc`, project key `EnsyInc_Loom`).

- Coverage: unit-test coverage comes from wrapping `dotnet test` directly (in-process). Service-test coverage comes from wrapping the **Api process itself** with `dotnet-coverage collect` — the tests hit the Api over real HTTP in a separate process, so instrumenting `dotnet test` would capture nothing. The Api is shut down through the coverage session after the tests finish so the report is flushed before upload. The "Apply database migrations" step is wrapped in `dotnet-coverage collect` too, so the Migrations `Program.cs` counts as covered instead of dragging the percentage down. Sonar merges all three reports via a comma-separated `sonar.cs.vscoveragexml.reportsPaths`.
- `service-tests` job: SQL Server as a service container, migrations, then the Api on `https://localhost:7088`, polled at `/swagger/v1/swagger.json` before the tests run. Its env (`Db__ConnectionString`, `ApiBaseUrl`) must stay in sync with `appsettings.Development.json`, the test `appsettings.json`, and `docker-compose.yml`.
- `sonar.exclusions` excludes `**/*.slnx`, `**/Migrations/*.cs` (EF-generated migration/designer/snapshot files), and `.github/**`.
- Requires a `SONAR_TOKEN` repository secret and a SonarCloud project `EnsyInc_Loom` whose "main branch" is `main`.

## Not in place yet

There is no auth. Add it deliberately rather than assuming it exists.
