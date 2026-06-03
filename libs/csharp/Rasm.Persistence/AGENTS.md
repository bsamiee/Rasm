# [H1][RASM_PERSISTENCE_AGENTS]
>**Dictum:** *Keep durability local, typed, and outside solve paths.*

<br>

[CRITICAL] Build `Rasm.Persistence` now as one local durable-state platform: store rail, query algebra, migrations, and read-only live-state projection. Add package references centrally (no version numbers in `.csproj`), create the `.csproj`, and scaffold the folder structure in Phase 0 before heavy work. Build the store core fully; integrate snapshot and companion lanes with the concern that owns them.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *Persistence owns storage semantics, not orchestration.*

<br>

- Use SQLite-local storage as the default plugin/app lane. Store file path: `RhinoApp.GetDataDirectory(persistentSettings:true)` — never `Environment.SpecialFolder`.
- Keep Postgres/Npgsql as companion-service-only guidance; Npgsql never appears in the Persistence `.csproj`.
- Keep `DbContext` inside an `Eff<RT,T>` `Bracket` shell — one context per operation, disposed after the operation; no context lives across operations.
- Use `StoreLifecycleOp` and `StoreQuery<TResult>` sealed DUs with `Fold` rather than `IRepository<T>` method families.
- Build the read-only live-state projection now as `IObservable<AppState>` (System.Reactive); `BehaviorSubject<AppState>` and DynamicData `SourceCache`/`SourceList` stay internal. AppUi calls `ObserveOn(RasmUiScheduler.RxScheduler)` and applies `Sample`/`Throttle` for back-pressure. Persistence never calls `ObserveOn`. Serialize `BehaviorSubject.OnNext` calls — use a lock or single-concurrency scheduler; `BehaviorSubject` is not thread-safe.
- Call `SQLitePCL.Batteries.Init()` before the first `SqliteConnection` (not merely before the first `DbContext`); failure → `MissingNativeAsset` receipt.
- Set PRAGMAs explicitly on each connection: `journal_mode=WAL`, `busy_timeout=3000`, `synchronous=NORMAL`, `foreign_keys=ON`. Never use `EnableRetryOnFailure` — Npgsql-only, does not compile on EF SQLite.
- Store `NodaTime.Instant` as `long` (Unix ticks) via a `ValueConverter` with provider type `long` → `INTEGER` — never the Npgsql `DateTimeOffset`/`TEXT` pattern (SQLite orders TEXT only client-side).
- Emit typed `StoreReceipt` DU from every store operation; never a generic `IReceipt` ledger. AppHost submits work as `Eff<RT, StoreReceipt>`; it never holds a `DbContext`.
- Use System.Text.Json source generation as the default serialization; treat MessagePack as the compact-snapshot lane. Use `XxHash3` (`System.IO.Hashing`) for snapshot checksums.
- Use `EFCore.NamingConventions` for snake_case columns globally.
- Apply `K4os.Compression.LZ4` for MessagePack snapshot payload compression.
- Encryption-at-rest is deferred; return `NativeEncryptionUnavailable` receipt case; document reliance on file-system (APFS) encryption.
- `RhinoCommon.PlugIn.Settings` is a complementary KV store for lightweight UI prefs — it coexists with the SQLite store; it is not a replacement.

---
## [2][BOUNDARY_RULES]
>**Dictum:** *No storage concern enters the computational kernel or solve loop.*

<br>

| [INDEX] | [BOUNDARY]         | [RULE]                                                                            |
| :-----: | ------------------ | --------------------------------------------------------------------------------- |
|   [1]   | `Rasm`             | No EF, SQLite, serializer package, or store API                                   |
|   [2]   | `Rasm.Rhino`       | No default Persistence reference                                                  |
|   [3]   | `Rasm.Grasshopper` | No Persistence call during solve                                                  |
|   [4]   | `Rasm.AppHost`     | Schedule/correlate durable work via `Eff<RT, StoreReceipt>` only; no `DbContext`  |
|   [5]   | `Rasm.AppUi`       | Consume `IObservable<AppState>` only; call `ObserveOn(RasmUiScheduler.RxScheduler)` |
|   [6]   | Support bundles    | AppHost collects; Persistence stores, redacts, exports, cleans                    |

---
## [3][INTEGRATION_CORRECTIONS]
>**Dictum:** *Fix wrong assumptions before they enter production code.*

<br>

[CRITICAL] These are definitive: treat violations as bugs.

| [INDEX] | [WRONG]                                                   | [CORRECT]                                                                               |
| :-----: | --------------------------------------------------------- | --------------------------------------------------------------------------------------- |
|   [1]   | `__EFMigrationsLock` failure case                         | SQL Server-specific; EF Core SQLite has no such table. Remove. Model `PartialMigration` instead. |
|   [2]   | Store path via `Environment.SpecialFolder`                | Use `RhinoApp.GetDataDirectory(persistentSettings:true)`.                               |
|   [3]   | `EnableRetryOnFailure` on SQLite                          | Npgsql-only; does not compile on EF SQLite. Use `busy_timeout` + LanguageExt `Schedule`. |
|   [4]   | `NodaTime.Instant` via `DateTimeOffset`/`TEXT`            | Use `long` provider type → `INTEGER` column for ordering. TEXT sorts lexicographically client-side only. |
|   [5]   | `Batteries.Init()` before first `DbContext`               | Must precede the first `SqliteConnection`; a bare `SqliteConnection` before `DbContext` also needs it. |
|   [6]   | `e_sqlite3` symbol conflict with RhinoWIP                 | Verify at integration time; `bundle_e_sqlite3` symbol prefix is the isolation guard.    |
|   [7]   | `SQLitePCLRaw.bundle_e_sqlcipher` for encryption          | Deprecated; conflicts with `e_sqlite3`. Do not add. Defer encryption; return `NativeEncryptionUnavailable`. |
|   [8]   | Downgrade guard via `__EFMigrationsHistory` alone         | Also check `PRAGMA user_version` (the integer schema-version mirror) on `StoreOpen` as the fast-path gate before EF initialization. |
|   [9]   | `BehaviorSubject<AppState>.OnNext` called from multiple threads | `BehaviorSubject` is not thread-safe. Serialize all `OnNext` calls through a lock or single-concurrency scheduler inside the fold worker. |

---
## [4][EVIDENCE]
>**Dictum:** *Source slices produce store evidence.*

<br>

Executable proof comes from source and store scenarios. Evidence categories:

- Open/migrate/query/close/dispose receipt.
- SQLite native load (`Batteries.Init` before first `SqliteConnection`), `e_sqlite3` symbol isolation, file path proof.
- PRAGMA config proof (`journal_mode`, `busy_timeout`, `synchronous`, `foreign_keys`) applied per-connection.
- Downgrade rejection (`__EFMigrationsHistory` + `PRAGMA user_version` check) and partial-migration failure receipt.
- Corrupt database receipt (`integrity_check`), rename, and snapshot-restore path.
- `BehaviorSubject<AppState>.OnNext` serialization proof (lock or single-concurrency scheduler).
- Snapshot `XxHash3` checksum, codec compat, and rejection receipt.
- `NodaTime.Instant` → `long`/`INTEGER` round-trip and ordering proof.
- `K4os.Compression.LZ4` payload compression round-trip.
- Redacted support-bundle export and cleanup receipt.
- Online backup (`SqliteConnection.BackupDatabase`) receipt.
- `NativeEncryptionUnavailable` receipt returned, not thrown.

---
## [5][REJECTIONS]
>**Dictum:** *Persistence is not a cache for computation.*

<br>

- No GH solve hot-path calls.
- No domain references to EF or SQLite.
- No generic repository wrapper.
- No unversioned snapshot payload.
- No MessagePack default without binary proof.
- No Postgres/Npgsql default for in-process plugin state.
- No `EnableRetryOnFailure` on SQLite.
- No `__EFMigrationsLock` failure case.
- No `Environment.SpecialFolder` for the store path.
- No `DateTimeOffset`/`TEXT` for `Instant` columns.
- No `bundle_e_sqlcipher` package.
- No MemoryPack, FlexLabs.Upsert, ZstdSharp, or `Microsoft.AspNetCore.DataProtection`.
- No encryption mechanism without a non-deprecated free NuGet path; return `NativeEncryptionUnavailable` receipt.
- No EF `.Proxies` (breaks per-operation `Bracket` model).
- No unserialized concurrent `BehaviorSubject.OnNext` calls from multiple thread-pool fold workers.
- No file-copy backup (races WAL shadow pages); use `SqliteConnection.BackupDatabase` only.
