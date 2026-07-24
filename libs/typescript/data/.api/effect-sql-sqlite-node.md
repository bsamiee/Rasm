# [TS_DATA_API_EFFECT_SQL_SQLITE_NODE]

`@effect/sql-sqlite-node` binds the neutral `@effect/sql` `SqlClient` to a synchronous in-process `better-sqlite3` connection — the server-runtime sqlite dialect lane. `SqliteClient` extends `SqlClient`, adding only the driver-distinct capability the neutral surface lacks, and marks `updateValues: never`, the one dialect degradation the pg spine does not carry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-node`
- package: `@effect/sql-sqlite-node` (MIT)
- effect-peer: `effect`, `@effect/platform`, `@effect/experimental` (`Reactivity`), `@effect/sql` (the `SqlClient` core this extends)
- backing: `better-sqlite3` (bundled synchronous in-process N-API sqlite, prebuilt binaries)
- module: ESM + CJS dual (`dist/dts` typings), `sideEffects: []`; subpaths `@effect/sql-sqlite-node/SqliteClient`, `/SqliteMigrator`
- runtime: node/bun server only — native synchronous binding, never the browser/wasm plane
- rail: the `store` `lane/sqlite` node dialect — pg-spine journal/projection contracts under the sqlite degradation table
- modules: `SqliteClient`, `SqliteMigrator` (banned re-export)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the sqlite client extension over the neutral `SqlClient`

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `SqliteClient` (interface, extends `SqlClient`)                    | `Context.Tag`   | sqlite lane client; neutral or driver members    |
|  [02]   | `SqliteClient.export` (`Effect<Uint8Array, SqlError>`)             | db snapshot     | whole-db serialize; snapshot/DR, `journal` copy  |
|  [03]   | `SqliteClient.backup(dest)` (`→ Effect<BackupMetadata, SqlError>`) | online backup   | live page-progress backup; `BackupMetadata`      |
|  [04]   | `SqliteClient.loadExtension(path)` (`→ Effect<void, SqlError>`)    | extension load  | runtime `.so`/`.dylib` load; `capability/matrix` |
|  [05]   | `SqliteClient.config`                                              | resolved config | filename/WAL introspection                       |
|  [06]   | `SqliteClient.updateValues: never`                                 | degradation     | lane degradation (no multi-row `UPDATE … FROM`)  |
|  [07]   | `BackupMetadata`                                                   | progress record | `{ totalPages, remainingPages }`; poll progress  |

[CONFIG_SCOPE]: `SqliteClientConfig` — a single-connection file config, no pool/TLS/timeouts; fields match the spine so a dialect swap is transparent.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                          |
| :-----: | :--------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `filename`                                     | file            | file-per-app tenancy key                     |
|  [02]   | `readonly?`                                    | open mode       | read-replica lane vs. writer                 |
|  [03]   | `prepareCacheSize?` / `prepareCacheTTL?`       | statement cache | prepared-statement hot-path lever            |
|  [04]   | `disableWAL?`                                  | journal mode    | WAL default; opt out for single-writer boxes |
|  [05]   | `spanAttributes?`                              | telemetry       | per-query OTel span attributes               |
|  [06]   | `transformResultNames?`/`transformQueryNames?` | name transform  | snake_case ⇄ camelCase; match the PG spine   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing and providing the sqlite lane Layer
- `layer`/`layerConfig` both yield `SqliteClient | SqlClient` in one Layer (error `ConfigError`), so a neutral-`SqlClient` row and a `backup`/`loadExtension` row share one binding; `make` returns `Effect<SqliteClient, never, Scope | Reactivity>`, and `layerConfig` sources `filename` and cache knobs from `Config` behind `host/config`.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `SqliteClient.layer(config)`                                | provide lane   | app root `./server`; binds the sqlite lane          |
|  [02]   | `SqliteClient.layerConfig(Config.Wrap<SqliteClientConfig>)` | provide lane   | filename/cache/WAL from `Config`; no hardcoded path |
|  [03]   | `SqliteClient.make(config)`                                 | build client   | scoped; requires `Reactivity` for `sql.reactive`    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SqliteClient extends SqlClient`, so `layer`/`layerConfig` bind both Tags: every journal/projection/capability/retrieve row keeps yielding the abstract `SqlClient` and stays driver-agnostic, and only a snapshot or extension-load row yields the concrete `SqliteClient`. Swapping to the bun lane or the pg spine is a `Layer` selection at the app root.
- `updateValues: never` is the type-level degradation signal — sqlite lacks multi-row `UPDATE … FROM`, so a row needing it branches through `sql.onDialect` (per-row on sqlite, set-based on pg). `lane/sqlite` + `capability/matrix` own the broader degradation table (no RLS → file-per-app tenancy, no `pg_ivm` → in-process `state` folds), parameterized over the dialect; this driver contributes the sqlite row.
- `better-sqlite3` is an in-process synchronous N-API binding — no pool, no network round-trip — so `sql.withTransaction` is a real single-connection transaction and the `prepareCacheSize`/`prepareCacheTTL` cache is the hot-path lever; WAL (default on) is the concurrent-read append-throughput mode the journal relies on.
- `loadExtension` admits a native sqlite capability (vector search, phonetic) — the fail-closed analog to the pg extension matrix; FTS5 and JSON1 are compiled in, and `capability/row` probes both dialects behind one closed vocabulary.

[STACKING]:
- `@effect/sql`(`.api/effect-sql.md`): everything neutral — the `sql` fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`/savepoints, `reactive`, and the `SqlEventJournal`/`SqlPersistedQueue` overlay bindings — runs against this driver unchanged; this package supplies only the `SqlClient.MakeOptions` (sqlite `Compiler`, acquirer, transaction machinery) the neutral `make` folds.
- `@effect/platform`(`.api/effect-platform.md`): `layerConfig` reads `filename`/cache knobs from `Config` behind `PlatformConfigProvider`; the banned `SqliteMigrator` (`run`/`layer`) is why `FileSystem`/`Path`/`CommandExecutor` ride the peer graph, never composed.
- `@effect/experimental`(`.api/effect-experimental.md`): `make` requires `Reactivity`, so `sql.reactive` read-your-writes works on the lane, and `SqlPersistedQueue.layerStore` gives `work` a durable job store on the file lane; EventLog's node journal backs onto this lane via `SqlEventJournal.layer` `[SQL_OVERLAY_BACKING]`.
- within-`store`: `SqliteClient.export`/`backup` feed the content-addressed `object` plane (a snapshot blob is content-keyed via `object/key`), `loadExtension` extends `capability/matrix` for the sqlite dialect, and `work`/`security/session` see only the neutral `SqlClient` the lane Layer satisfies.

[LOCAL_ADMISSION]:
- Provide `SqliteClient.layerConfig` on the `./server` subpath at the app root; yield the concrete `SqliteClient` only where `export`/`backup`/`loadExtension` are genuinely needed, `SqlClient` everywhere else to stay dialect-portable.
- Source `filename` from `Config` and keep WAL on unless a read-only replica sets `disableWAL`; express every dialect gap through `sql.onDialect`, never a per-driver journal or projection fork.
- `SqliteMigrator` re-export is banned — no migration run; schema is `iac` declarative ensure verified at `store` startup.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-node`
- Owns: the node/bun-server `SqliteClient` Layer over `better-sqlite3` behind the neutral `SqlClient` Tag, and the driver-distinct capability — whole-db `export`, online `backup`/`BackupMetadata`, runtime `loadExtension`, the `SqliteClientConfig` prepared-statement cache and WAL control, and the `updateValues: never` degradation marker
- Accept: `layerConfig` on `./server` sourcing `filename`/cache from `Config`, the neutral `SqlClient` yielded by every dialect-agnostic row, `sql.onDialect` for the sqlite/pg gaps, `export`/`backup`/`loadExtension` where the concrete lane is in scope, `Reactivity` for `sql.reactive`
- Reject: a driver import in a neutral row, a per-driver journal/projection fork, a hardcoded filename, `SqliteMigrator`/migration use, reliance on `updateValues`
