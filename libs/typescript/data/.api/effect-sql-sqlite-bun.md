# [TS_DATA_API_EFFECT_SQL_SQLITE_BUN]

`@effect/sql-sqlite-bun` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to Bun's synchronous `bun:sqlite` as a `SqliteClient` — the server durability lane running the PG spine's journal and projection statements, dialect-selected by `sql.onDialect`. It carries no native addon, adds `export` (whole-database `Uint8Array` snapshot) and `loadExtension` over the neutral contract, and marks `updateValues: never` — the one member SQLite drops and the anchor of the `lane/sqlite` degradation table. `SqliteMigrator` ships branch-banned; with no RLS, tenancy is `filename`-per-app.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-bun`
- package: `@effect/sql-sqlite-bun` (MIT)
- effect-peer: `effect`, `@effect/sql` (the `SqlClient` core; `.api/effect-sql.md`), `@effect/experimental` (`Reactivity` for `make`/`reactive`; `.api/effect-experimental.md`), `@effect/platform` (`FileSystem`/`Path` for the banned `SqliteMigrator`; `.api/effect-platform.md`)
- module: ESM + CJS dual (`dist/dts` typings); subpaths `@effect/sql-sqlite-bun/SqliteClient`, `/SqliteMigrator`; `sideEffects: []`
- runtime: `runtime:node`/bun only — imports `bun:sqlite`, Bun's built-in synchronous SQLite with no npm dependency and no native addon (WAL, FTS5, JSON1 compiled in); the node peer lane is `@effect/sql-sqlite-node`, the browser lane `@effect/sql-sqlite-wasm`
- rail: the `store` `lane/sqlite` bun dialect — the PG spine's journal/projection contracts under the sqlite capability-degradation table
- modules: `SqliteClient`, `SqliteMigrator` (banned re-export)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `SqliteClient` service and its sqlite-native additions
- `SqliteClient extends SqlClient`; `layer` binds both Tags, so a `lane/sqlite` row yields the neutral `SqlClient` and only `export`/`loadExtension` reach the concrete `SqliteClient`. This lane carries no `backup` (the node lane's `better-sqlite3` addition); `updateValues: never` is the degradation seam, not an omission.

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                              |
| :-----: | :----------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `SqliteClient.SqliteClient` (Tag) / `interface SqliteClient`       | service Tag     | `lane/sqlite` journal/projection rows            |
|  [02]   | `SqliteClient.export: Effect<Uint8Array, SqlError>`                | serialize       | `journal/retain` whole-db backup; snapshot bytes |
|  [03]   | `SqliteClient.loadExtension(path: string): Effect<void, SqlError>` | extension       | `retrieve` `sqlite-vec`/FTS; `capability/row`    |
|  [04]   | `SqliteClient.updateValues: never`                                 | degradation     | anchor of the `lane/sqlite` degradation table    |
|  [05]   | `SqliteClient.config: SqliteClientConfig`                          | resolved config | `filename`/WAL introspection                     |

[PUBLIC_TYPE_SCOPE]: configuration
- `SqliteClientConfig` is a single-connection file config — no pool, TLS, or timeouts. `filename` keys file-per-app tenancy, `disableWAL` toggles the concurrency mode, and the name transforms match the spine so a dialect swap preserves canonical camelCase rows.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `filename` (`string`)                        | file           | `scope` file-per-app tenancy key; `":memory:"` for specs |
|  [02]   | `readonly`/`create`/`readwrite`              | open mode      | read-replica lane vs. writer; `create` gates first-open  |
|  [03]   | `disableWAL` (`boolean`)                     | journal mode   | WAL default for concurrency; disable for single-writer   |
|  [04]   | `transformResultNames`/`transformQueryNames` | name transform | snake_case ⇄ camelCase; match PG spine for `onDialect`   |
|  [05]   | `spanAttributes`                             | telemetry      | per-query OTel span attributes                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the lane and its two native operations
- `layer` binds both Tags from a fixed config and `layerConfig` resolves `filename`/mode from `Config`, each yielding `SqliteClient | SqlClient` in one `Layer` (`ConfigError`); `make` returns `Effect<SqliteClient, never, Scope | Reactivity>`, scoped to close the database on release. `export` and `loadExtension` are the two lane-native operations; every journal/projection statement is the spine's, routed through `sql.onDialect` where SQLite syntax differs.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                               |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------------------ |
|  [01]   | `SqliteClient.layer(config: SqliteClientConfig)`                    | lane layer     | `lane/sqlite` app-root row (fixed `filename`)     |
|  [02]   | `SqliteClient.layerConfig(config: Config.Wrap<SqliteClientConfig>)` | lane layer     | `host/config` file/mode resolution; standing row  |
|  [03]   | `SqliteClient.make(config)`                                         | scoped make    | scoped construction inside a larger acquire graph |
|  [04]   | `client.export` → `Uint8Array`                                      | snapshot       | `journal/retain` ship/restore; `":memory:"` dump  |
|  [05]   | `client.loadExtension(vecPath)`                                     | extension      | `retrieve/index` `vec0` virtual table             |

## [04]-[IMPLEMENTATION_LAW]

[SQLITE_LANE_TOPOLOGY]:
- `SqliteClient` IS a `SqlClient`: `layer*` binds `SqliteClient | SqlClient`, so `lane/sqlite` rows compose the neutral Tag and share the PG spine's journal/projection statements. `sql.onDialect({ pg, sqlite })` is the seam — one statement, two dialect fragments, selected at compile of the fragment.
- `updateValues: never` is the degradation anchor, the one `SqlClient` member SQLite cannot express; `onDialect` routes bulk updates to a chunked-insert path, and the `lane/sqlite` degradation table owns every remaining PG-vs-sqlite divergence as data.
- Single synchronous handle, no addon: `bun:sqlite` is one connection with no pool and no native build; WAL admits concurrent readers under one writer, `reserve` serializes a pinned unit of work, and `db.query()` caches statements internally — no prepare-cache knob, where the `@effect/sql-sqlite-node` peer binds `better-sqlite3` with an explicit cache. Tenancy is `filename`-per-app, not a connection fork.

[INTEGRATION_LAW]:
- Stack with `@effect/sql` (`.api/effect-sql.md`): the lane runs the spine's `SqlSchema` decoders, `SqlResolver` batchers, `Model` repositories, and `SqlStream` cursors unchanged — only the `sql.onDialect` fragments and the `updateValues` degradation differ.
- Stack with `@effect/sql-sqlite-node`/`-wasm` (`.api/effect-sql-sqlite-node.md`): the three sqlite lanes share the neutral `SqlClient` and the `lane/sqlite` statements, so a lane is a `Layer` row under one contract; the driver-distinct supersets diverge — node adds `backup`/`BackupMetadata`, wasm adds `import`/`OpfsWorker`/`withTransferables`, this lane adds only the `create`/`readwrite` open-mode flags.
- Stack with `@effect/sql-pg` (`.api/effect-sql-pg.md`): the PG spine is the reference contract and this lane its degraded mirror; `sql.onDialect` holds one journal/projection statement across both, and the degradation table maps what routes differently.
- Stack with `@effect/experimental` (`.api/effect-experimental.md`): `make`/`reactive` require `Reactivity`, so read-your-writes rides in-process `Reactivity.invalidate`; `@effect/platform-bun` `BunContext` supplies the `FileSystem`/`Path` the banned `SqliteMigrator` requires.

[LOCAL_ADMISSION]:
- Yield the neutral `SqlClient` in every row and reach for the concrete `SqliteClient` only for `export`/`loadExtension`; express the `updateValues` degradation and every PG-vs-sqlite divergence through `sql.onDialect`, never a lane-only helper or a re-authored statement.
- `SqliteMigrator` (`run`/`layer`) is banned branch-wide with `PgMigrator`: DDL is idempotent declarative ensure, and with no RLS the tenancy law is `filename`-per-app.
- Source `filename` and open mode from `Config`, never a row literal, and keep the name transforms identical to the spine so a dialect swap stays transparent.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-bun`
- Owns: the `bun:sqlite` binding of `SqlClient` to a `SqliteClient`, the whole-database `export`, `loadExtension`, the `updateValues: never` degradation anchor, and `layer`/`layerConfig`/`make` construction
- Accept: `SqliteClient` as a `SqlClient` provider, journal/projection statements shared with the PG spine through `sql.onDialect`, the degradation table as data, in-process `Reactivity` for read-your-writes, `export` as the backup primitive, `filename`-per-app tenancy, `spanAttributes` auto-spanned
- Reject: a `SqliteClient`-typed row that should be `SqlClient`, a lane-only re-authoring of a shared statement, `updateValues` faked outside `onDialect`, `SqliteMigrator` or any runtime schema mutation, a pool or RLS assumption on a single-connection file lane, a hardcoded `filename`/mode
