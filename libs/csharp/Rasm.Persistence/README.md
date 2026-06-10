# [RASM_PERSISTENCE]

`Rasm.Persistence` is the local durable-state package for product apps. It owns SQLite/EF Core storage, schema/migrations, app-state projection, snapshots, support artifacts, redaction, cache/index metadata, and AppHost drain integration.

## [1]-[PURPOSE]

Persistence exposes durable-state operations and receipts through typed store algebras and an AppHost dispatch adapter. AppHost supplies runtime scheduling and profile/path inputs. AppUi observes read-only app-state projections. Compute uses Persistence for deterministic model-result cache and benchmark artifact indexing.

It is not an EF wrapper, repository family, serializer wrapper, GH solve-path cache, Rhino settings wrapper, or domain model replacement.

## [2]-[STATUS]

| [INDEX] | [SURFACE]          | [STATE]                                     |
| :-----: | ------------------ | ------------------------------------------- |
|   [1]   | Project file       | Present in `Workspace.slnx`                 |
|   [2]   | Production source  | Store rail contract defined                 |
|   [3]   | Package references | Active setup references are versionless     |
|   [4]   | Storage            | SQLite/EF Core local durable-state rail     |
|   [5]   | Encryption         | SQLCipher absent; OS/profile storage policy |
|   [6]   | Redaction          | First-class support-bundle requirement      |

## [3]-[CONSTRAINTS]

- Persistence is RhinoCommon-free. AppHost/Rhino supplies resolved profile/path values.
- Persistence is built as a complete durable-state package for host profiles, companion stores, sidecar stores, app-state projection, support bundles, model-result cache, benchmark indexes, and snapshot exchange through the same store and snapshot rails.
- Store operations use typed lifecycle/query algebras and receipts, not `IRepository<T>` families.
- SQLite, file snapshots, JSON snapshots, MessagePack snapshots, companion databases, cache metadata, benchmark indexes, and support bundles enter through the same store or snapshot algebra.
- Folder architecture is rail-first: store profiles, entity kinds, query shapes, codecs, compression, redaction classes, retention policies, cache/index cases, migration states, and support artifacts add rows, typed cases, receipt fields, and projection records instead of repository families or provider-branded public services.
- Store profile, path policy, schema version, provider package, snapshot codec, compression policy, redaction class, retention policy, and projection shape are parameterized data, not hardcoded storage branches.
- `DbContext` is operation-scoped and disposed through the store rail.
- SQLite native init precedes the first `SqliteConnection`; missing native assets return receipts.
- EF Core SQLite `__EFMigrationsLock` is real in EF9+ and has abandoned-lock handling.
- `PRAGMA user_version` and `__EFMigrationsHistory` are separate schema facts.
- `Microsoft.EntityFrameworkCore.Design` is a migration-generation tool, not a core library dependency.
- `System.IO.Hashing` supplies `XxHash3` for snapshot and artifact checksums.
- `MessagePack.Generator` is not used. MessagePack snapshot source uses the analyzer/source-generator route.
- `EFCore.BulkExtensions` is not core. Bulk import uses measured raw `Microsoft.Data.Sqlite` first.
- Support-bundle artifacts are classified and redacted before export.
- No Persistence code runs inside GH solve hot paths.
