# [H1][RASM_PERSISTENCE_AGENTS]
>**Dictum:** *Keep durability local, typed, and outside solve paths.*

<br>

[CRITICAL] `Rasm.Persistence` is docs-only until a future implementation explicitly creates a project and production source. Do not add package references, `.csproj`, or C# files from this folder contract alone.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *Persistence owns storage semantics, not orchestration.*

<br>

- Use SQLite-local storage as the default future plugin/app lane.
- Keep Postgres/Npgsql as companion-service-only guidance.
- Keep `DbContext` inside an `Eff<RT,T>` runtime shell.
- Use sealed query/store operation algebra rather than `IRepository<T>` method families.
- Keep System.Text.Json source generation as default serialization guidance.
- Treat MessagePack as conditional compact snapshot candidate only.

---
## [2][BOUNDARY_RULES]
>**Dictum:** *No storage concern enters the computational kernel or solve loop.*

<br>

| [INDEX] | [BOUNDARY] | [RULE] |
| :-----: | ---------- | ------ |
|   [1]   | `Rasm` | No EF, SQLite, serializer package, or store API |
|   [2]   | `Rasm.Rhino` | No default Persistence reference |
|   [3]   | `Rasm.Grasshopper` | No Persistence call during solve |
|   [4]   | `Rasm.AppHost` | Schedule/correlate durable work only |
|   [5]   | `Rasm.AppUi` | Read app-state projections only |
|   [6]   | Support bundles | AppHost collects; Persistence stores, redacts, exports, cleans |

---
## [3][EVIDENCE]
>**Dictum:** *Docs name evidence categories; source slices produce evidence.*

<br>

Documentation edits require manual consistency only. Executable proof begins when source, package references, or store scenarios land.

Future Persistence evidence categories:

- Open/migrate/query/close/dispose receipt.
- SQLite native load and file path proof.
- Downgrade rejection and partial migration failure receipt.
- Locked/corrupt database receipt.
- Snapshot codec compatibility and rejection receipt.
- Redacted support-bundle export and cleanup receipt.

---
## [4][REJECTIONS]
>**Dictum:** *Persistence is not a cache for computation.*

<br>

- No GH solve hot-path calls.
- No domain references to EF or SQLite.
- No generic repository wrapper.
- No unversioned snapshot payload.
- No MessagePack default without binary proof.
- No Postgres/Npgsql default for in-process plugin state.
