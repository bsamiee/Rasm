# [H1][RASM_PERSISTENCE_ROADMAP]
>**Dictum:** *Scaffold the store, then build query algebra, live state, and snapshots.*

<br>

This roadmap sequences the build. The store rail, query algebra, migrations, and live-state projection are built fully from the foundation; compact-snapshot and companion lanes integrate with the concern that owns them.

---
## [1][PHASE_0]
>**Dictum:** *Housekeeping lands and compiles before heavy work.*

<br>

- Add every core package to root `Directory.Packages.props` at newest viable versions; project references stay versionless; no version numbers in `.csproj`. Core packages: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets=all`), `SQLitePCLRaw.bundle_e_sqlite3` (carries the `SQLitePCLRaw.lib.e_sqlite3` macOS-arm64 native asset), `NodaTime`, `NodaTime.Serialization.SystemTextJson`, `FluentValidation`, `MessagePack`, `MessagePack.Generator`, `EFCore.NamingConventions`, `EFCore.BulkExtensions`, `K4os.Compression.LZ4`, `Microsoft.Extensions.Compliance.Redaction`. `LanguageExt.Core` is already in central management via the Functional Core group; `System.Reactive` and `DynamicData` via the App UI group.
- `StoreOpen` calls `SQLitePCL.Batteries.Init()` (idempotent) before the first `SqliteConnection`. Failure → `MissingNativeAsset` receipt; no throw.
- Set PRAGMAs explicitly on each connection open via `ExecuteSqlRaw`: `journal_mode=WAL`, `busy_timeout=3000`, `synchronous=NORMAL`, `foreign_keys=ON`. Do NOT use `EnableRetryOnFailure` — it is Npgsql-only and does not compile on EF SQLite.
- Create `Rasm.Persistence.csproj`, wire it into `Workspace.slnx` and the central build.
- Scaffold the store folder skeleton (`Store/`, `Query/`, `Snapshot/`, `Support/`) and canonical section order.

Phase 0 is complete when restore and build pass clean.

---
## [2][STORE_CORE]
>**Dictum:** *Open, migrate, query, project, close — one algebra.*

<br>

Build the store rail with lifecycle, query, and live projection integrated:

| [INDEX] | [SURFACE]                             | [BASIS]                                                                        |
| :-----: | ------------------------------------- | ------------------------------------------------------------------------------ |
|   [1]   | `StoreProfile` + native init          | `RhinoApp.GetDataDirectory(persistentSettings:true)`, `Batteries.Init()`       |
|   [2]   | `StoreLifecycleOp` algebra            | `Eff<RT,T>` `Bracket` shell; one `DbContext` per operation                     |
|   [3]   | `StoreQuery<TResult>` algebra         | Typed reads: entity kind, key, time range, page, projection, include-deleted   |
|   [4]   | `StoreReceipt` DU                     | SUCCESS + typed failure cases; `NativeEncryptionUnavailable` for deferred encryption |
|   [5]   | Append-only migrations                | Forward-only; downgrade → `DowngradeRejected`; partial → `PartialMigration`   |
|   [6]   | `AppState` + EF→DynamicData bridge    | `ISaveChangesInterceptor` → thread pool fold → `BehaviorSubject<AppState>`     |
|   [7]   | `IObservable<AppState>` public surface | System.Reactive; DynamicData internal only; seed on `StoreOpen`; `OnCompleted` on disposal; `OnNext` serialized |
|   [8]   | PRAGMA init + `integrity_check`       | Per-connection; corruption → rename + restore from snapshot                    |
|   [9]   | `NodaTime.Instant` converter          | Provider type `long` + `INTEGER` column; `XxHash3` snapshot checksums          |
|  [10]   | `EFCore.NamingConventions`            | snake_case column names globally                                               |
|  [11]   | `PRAGMA user_version` + `__EFMigrationsHistory` | Both checked on `StoreOpen`; `user_version` is the fast-path integer gate; `__EFMigrationsHistory` is the migration-log truth |

---
## [3][SCOPED_LANES]
>**Dictum:** *Snapshot and companion lanes integrate with the concern that owns them.*

<br>

| [INDEX] | [LANE]                            | [INTEGRATES_WITH]                                                              |
| :-----: | --------------------------------- | ------------------------------------------------------------------------------ |
|   [1]   | `SnapshotEnvelope` + codecs       | Json (default) or MessagePack (`K4os.Compression.LZ4` payload compression)    |
|   [2]   | Support bundle export             | AppHost support collection; Brotli/Deflate via `System.IO.Compression`         |
|   [3]   | Redaction                         | `Microsoft.Extensions.Compliance.Redaction` — named concern, no active mechanism |
|   [4]   | `EFCore.BulkExtensions`           | Conditional cache-import lane; prove on bulk-insert benchmark before activating |
|   [5]   | FTS5 + JSON1                      | Compiled INTO SQLite native; use `FromSqlRaw`; no extra package                |
|   [6]   | Online backup                     | `SqliteConnection.BackupDatabase`; never file copy (WAL shadow race)           |
|   [7]   | Compaction                        | `VACUUM INTO` (online) + `wal_checkpoint(TRUNCATE)` or `auto_vacuum=INCREMENTAL` |
|   [8]   | Raw `Microsoft.Data.Sqlite` bypass | Native-load probe or EF bypass slice                                          |
|   [9]   | Companion-service database        | Separate out-of-process only; Npgsql never in-process                         |

---
## [4][STORE_EVIDENCE]
>**Dictum:** *Store claims require lifecycle evidence.*

<br>

Store claims are scoped to the proven local store. Receipts identify database path, schema version, migration result, query result, close/dispose path, downgrade rejection (`PRAGMA user_version` + `__EFMigrationsHistory`), partial-migration failure, native-load proof (`Batteries.Init` before first `SqliteConnection`), corruption recovery (`integrity_check` → rename → snapshot restore), WAL/PRAGMA config proof, `BehaviorSubject.OnNext` serialization proof, snapshot codec compatibility, support-bundle redaction, and backup path.
