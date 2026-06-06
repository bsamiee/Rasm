# [RASM_PERSISTENCE]

`Rasm.Persistence` is the local durable-state platform for Rasm plugins and apps, built fully from the foundation: SQLite-backed state, `StoreLifecycleOp`/`StoreQuery<T>` algebra, migrations, snapshots, live-state projection, and support artifacts.

## [1][PURPOSE]

`Rasm.Persistence` owns local SQLite-backed app state, migrations, queries, presets, snapshots, caches, `IObservable<AppState>` live-state projection, support artifacts, redaction, export receipts, and cleanup. AppHost schedules durable work through Persistence's exported `StoreDispatch` capability (typed `Eff<RT, StoreReceipt>`); Persistence owns open, migrate, query, transaction, dispose, and storage receipts.

It is not a repository wrapper, EF wrapper, serializer wrapper, solve-path cache, GH tree store, or domain model replacement.

## [2][STATUS]

| [INDEX] | [SURFACE]             | [STATE]                  |
| :-----: | --------------------- | ------------------------ |
|   [1]   | Project file          | Create in Phase 0        |
|   [2]   | Production API        | In progress              |
|   [3]   | Package references    | Add centrally in Phase 0 |
|   [4]   | Local store           | SQLite-first             |
|   [5]   | Solve-path behavior   | Forbidden                |
|   [6]   | Encryption-at-rest    | Deferred; `NativeEncryptionUnavailable` receipt case |

Add packages centrally at newest viable versions during Phase 0; no version numbers in `.csproj`.

## [3][MANUAL]

| [INDEX] | [FILE]             | [READ_FOR]                                                                                                         |
| :-----: | ------------------ | ------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `_ARCHITECTURE.md` | Type shapes, provider split, operation algebra, bridge, PRAGMA init, migration policy, compaction, failure model   |
|   [2]   | `AGENTS.md`        | Build rules, integration corrections, and hot-path rejections                                                      |
|   [3]   | `ROADMAP.md`       | Build sequence and scoped lanes                                                                                     |

## [4][CONSTRAINTS]

- Store operations are `StoreLifecycleOp` and `StoreQuery<TResult>` sealed DUs with `Fold`, not an `IRepository<T>` family.
- `DbContext` lives inside an `Eff<RT,T>` `Bracket` shell — one context per operation, no context lives across operations.
- Store path resolves from `RhinoApp.GetDataDirectory(persistentSettings:true)`, never `Environment.SpecialFolder`.
- `SQLitePCL.Batteries.Init()` precedes the first `SqliteConnection`; failure → `MissingNativeAsset` receipt.
- PRAGMAs (`journal_mode`, `busy_timeout`, `synchronous`, `foreign_keys`) set explicitly per connection.
- `NodaTime.Instant` stored as `long`/`INTEGER`; never `DateTimeOffset`/`TEXT`.
- Downgrade guard: `PRAGMA user_version` fast-path check + `__EFMigrationsHistory` on `StoreOpen`; `DowngradeRejected` receipt on mismatch.
- The live-state projection is read-only `IObservable<AppState>`; `BehaviorSubject` and DynamicData stay internal.
- AppUi calls `ObserveOn(RasmUiScheduler.RxScheduler)` and applies `Sample`/`Throttle`; Persistence never calls `ObserveOn`.
- `BehaviorSubject<AppState>.OnNext` is serialized inside the fold worker (not thread-safe).
- No persistence code runs inside GH solve hot paths.
- No `EnableRetryOnFailure`, no `__EFMigrationsLock`, no `bundle_e_sqlcipher`, no EF `.Proxies`.
