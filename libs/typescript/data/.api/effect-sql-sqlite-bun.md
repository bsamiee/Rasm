# [TS_DATA_API_EFFECT_SQL_SQLITE_BUN]

`@effect/sql-sqlite-bun` binds the abstract `@effect/sql` `SqlClient` Tag to Bun's native `bun:sqlite` as a `SqliteClient` — the `runtime:node`/bun durability lane that runs the SAME journal and projection statements as the PG spine, selected by `sql.onDialect({ pg, sqlite })`. It has zero native-addon dependencies (the driver is Bun's built-in synchronous SQLite), adds `export` (serialize the whole database to bytes for backup/ship) and `loadExtension(path)` over `SqlClient`, and marks `updateValues: never` — the one `SqlClient` member SQLite drops, and the anchor of the `lane/sqlite` capability-degradation table. Its peer lanes are `@effect/sql-sqlite-node` (the node lane over `better-sqlite catalog`) and `@effect/sql-sqlite-wasm` (the OPFS browser lane); the three share the neutral `SqlClient` contract and the same `lane/sqlite` journal/projection statements, so a lane swap is a Layer selection at the neutral tier — the driver-distinct supersets diverge (node adds online `backup`/`BackupMetadata` and a `prepareCacheSize`/`prepareCacheTTL` cache, wasm adds `import`/`OpfsWorker`/`withTransferables`, and this bun lane adds neither: `bun:sqlite`'s `db.query()` caches statements internally). `SqliteMigrator` ships and is banned — DDL is idempotent declarative ensure, and with no RLS the sqlite tenancy law is file-per-app.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-bun`
- package: `@effect/sql-sqlite-bun`
- license: `MIT`
- effect-peer: `effect catalog`, `@effect/sql catalog` (the `SqlClient` core this extends; `.api/effect-sql.md`), `@effect/experimental catalog` (`Reactivity` — `make`/`reactive` require it; `.api/effect-experimental.md`), `@effect/platform catalog` (`FileSystem`/`Path`/`CommandExecutor` — `SqliteMigrator` only; `.api/effect-platform.md`)
- backing: none — `bun:sqlite` is Bun's built-in synchronous SQLite (no npm dependency, no native addon); WAL, FTS5, and JSON1 ship in Bun
- runtime: `runtime:node`/bun only — imports `bun:sqlite`; the node peer lane is `@effect/sql-sqlite-node`, the browser peer lane `@effect/sql-sqlite-wasm` over OPFS
- modules: `SqliteClient`, `SqliteMigrator`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `SqliteClient` service and its sqlite-native additions
- rail: store/lane
- `SqliteClient extends SqlClient` — the `layer` provides both the `SqliteClient` Tag and the `SqlClient` Tag, so `lane/sqlite` rows compose the neutral `SqlClient` and only `export`/`loadExtension` rows reach the `SqliteClient` Tag (this lane has no `backup` member — that is the node lane's better-sqlite3 addition). `updateValues: never` is the degradation seam, not an omission.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:------------- |:------------------------------------------------------------------------- |
| [01] | `SqliteClient.SqliteClient` (Tag) / `interface SqliteClient` | service Tag | `lane/sqlite` journal/projection rows; `SqliteClient \| SqlClient` in one layer |
| [02] | `SqliteClient.export: Effect<Uint8Array, SqlError>` | serialize | `journal/retain`/`lane` whole-db backup/ship; content-addressable snapshot bytes |
| [03] | `SqliteClient.loadExtension(path: string): Effect<void, SqlError>` | extension | `retrieve` `sqlite-vec`/FTS load; the sqlite analogue of a `capability/row` |
| [04] | `SqliteClient.updateValues: never` | degradation | the anchor of the `lane/sqlite` capability-degradation table |
| [05] | `SqliteClient.config: SqliteClientConfig` | resolved config | `filename`/WAL introspection |

[PUBLIC_TYPE_SCOPE]: configuration
- rail: store/lane
- `SqliteClientConfig` is a single-connection file config — no pool, no TLS, no timeouts. `filename` keys the file-per-app tenancy; `disableWAL` toggles the concurrency mode; the transforms match the spine so canonical camelCase rows survive a dialect swap.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:------------- |:------------------------------------------------------------------------- |
| [01] | `SqliteClientConfig.filename` (`string`) | file | `scope` file-per-app tenancy key; `":memory:"` for specs |
| [02] | `SqliteClientConfig.readonly`/`create`/`readwrite` | open mode | read-replica lane vs. writer; `create` gates first-open ensure |
| [03] | `SqliteClientConfig.disableWAL` (`boolean`) | journal mode | WAL default for reader/writer concurrency; disable for single-writer boxes |
| [04] | `SqliteClientConfig.transformResultNames`/`transformQueryNames` | name transform | snake_case ⇄ camelCase — must match the PG spine so `onDialect` rows agree |
| [05] | `SqliteClientConfig.spanAttributes` | telemetry | per-query OTel span attributes |

[PUBLIC_TYPE_SCOPE]: the inherited `SqlClient` core the sqlite lane composes
- rail: store/lane
- The lane runs the same `@effect/sql` contracts (`.api/effect-sql.md`) as the spine; `sql.onDialect` selects the sqlite fragment where syntax diverges. `updateValues` is `never`, so `onDialect` routes bulk updates to a chunked-insert path on this lane.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:------------- |:------------------------------------------------------------------------- |
| [01] | `SqlClient.withTransaction(effect)` | transaction | `journal/append` OCC + outbox atomic commit on the lane |
| [02] | `SqlClient.reserve: Effect<Connection, SqlError, Scope>` | dedicated conn | pinned-connection work; sqlite is single-connection, so `reserve` serializes |
| [03] | `SqlClient.reactive(keys, effect): Stream` / `reactiveMailbox` | reactive read | `lane` read-your-writes via in-process `Reactivity` (no LISTEN/NOTIFY) |
| [04] | `Statement` `sql` `Constructor` (`in`/`unsafe`/`literal`/`insert`/`update`/`and`/`or`/`csv`/`join`) | statement | the shared journal/projection statements; `updateValues` excluded here |
| [05] | `sql.onDialect({ sqlite, pg, mysql, mssql, clickhouse })` (the `sqlite` arm here) / `sql.onDialectOrElse({ orElse, sqlite? })` | dialect switch | select the sqlite fragment; the cross-lane portability seam — `sqlite` is an arm-KEY, not a `sql.sqlite` method |
| [06] | `SqlSchema.findAll`/`findOne`/`single` · `SqlResolver.ordered`/`grouped`/`findById` | decode/batch | typed row decode + N+1-free batched reads, identical to the spine |
| [07] | `Model.makeRepository`/`makeDataLoaders` · `SqlStream.asyncPauseResume` · `SqlError` | model/stream/fault | typed tables, streamed cursors, the one tagged SQL error rail |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the lane Layer
- rail: store/lane
- `layer` provides both Tags from a fixed config; `layerConfig` resolves `filename`/mode from `Config`. `make` returns an `Effect` with error channel `never` (open failures surface later as `SqlError` at query time), scoped to close the database on release.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `SqliteClient.layer(config: SqliteClientConfig): Layer<SqliteClient \| SqlClient, ConfigError>` | lane layer | `lane/sqlite` app-root row (fixed `filename`) |
| [02] | `SqliteClient.layerConfig(config: Config.Wrap<SqliteClientConfig>): Layer<SqliteClient \| SqlClient, ConfigError>` | lane layer | `host/config` file/mode resolution — the standing lane row |
| [03] | `SqliteClient.make(config): Effect<SqliteClient, never, Scope \| Reactivity>` | scoped make | scoped construction inside a larger acquire graph |

[ENTRYPOINT_SCOPE]: the sqlite lane patterns — same contracts, degraded capabilities
- rail: store/lane
- Every journal/projection statement is the spine's, routed through `onDialect` where SQLite syntax differs. Snapshot `export` and `loadExtension` are the two lane-native operations.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------------- |:------------- |:------------------------------------------------------- |
| [01] | `sql.onDialect({ pg: `\``… ON CONFLICT … RETURNING (xmax = 0)`\``, sqlite: `\``… ON CONFLICT … RETURNING changes()`\`` })` | idempotency | one outbox claim across dialects |
| [02] | `sql.onDialect({ pg: bulkUpdateValues, sqlite: chunkedInsert })` | updateValues | route the `never` member to a chunked-insert path |
| [03] | `client.export` → `Uint8Array` → content-addressed snapshot | backup | `journal/retain` ship/restore; `":memory:"` spec dump |
| [04] | `client.loadExtension(vecPath)` then `sql`\``CREATE VIRTUAL TABLE … USING vec0`\` | vector index | `retrieve/index` sqlite vector lane |
| [05] | `client.reactive(keys, query): Stream` ← in-process `Reactivity.invalidate(keys)` | reactive read | `lane` read-your-writes (no NOTIFY; in-process signal) |

## [04]-[IMPLEMENTATION_LAW]

[SQLITE_LANE_TOPOLOGY]:
- `SqliteClient` IS a `SqlClient`: `layer*` provides `SqliteClient | SqlClient`, so `lane/sqlite` rows compose the neutral `SqlClient` Tag and share the journal/projection statements with the PG spine. `sql.onDialect({ pg, sqlite })` is the seam — one statement, two dialect fragments, selected at compile of the fragment.
- `updateValues: never` is the degradation anchor: the one `SqlClient` member SQLite cannot express. `onDialect` routes bulk updates to a chunked-insert path on this lane; the `lane/sqlite` table enumerates the rest as DATA — no RLS → file-per-app tenancy, no `pg_ivm` → in-process projection folds, no LISTEN/NOTIFY → in-process `Reactivity` wake, no COPY → chunked inserts, no advisory locks → the single-writer file.
- single connection, WAL for concurrency: SQLite has no pool — `bun:sqlite` is one synchronous handle. WAL (`disableWAL: false`) admits concurrent readers with one writer; `reserve` serializes a pinned-connection unit of work. Tenancy is `filename`-per-app, not a connection fork.
- `export` is the backup/ship primitive: serialize the whole database to bytes for a content-addressed snapshot or a `":memory:"` spec dump — the sqlite analogue of a `pg_dump`, as one `Effect`.
- Bun-native, no addon: the driver is `bun:sqlite`, so the lane carries no native-build step and needs no prepare-cache knob — `db.query()` caches statements internally, where the `@effect/sql-sqlite-node` peer binds `better-sqlite3` (native N-API) with an explicit `prepareCacheSize`/`prepareCacheTTL`. The two share the neutral `SqlClient` contract and the `lane/sqlite` statements, so the swap is a Layer selection — but the driver-distinct supersets differ: node adds online `backup`/`BackupMetadata`, this lane adds the `create`/`readwrite` open-mode flags and neither `backup` nor a prepare-cache config.

[INTEGRATION_LAW]:
- Stack with `@effect/sql` core (`.api/effect-sql.md`): the lane runs the same `SqlSchema` decoders, `SqlResolver` batchers, `Model` repositories, and `SqlStream` cursors as the spine; only `sql.onDialect` fragments and the `updateValues` degradation differ. The catalog documents stacking ONTO the core, never re-implementing it.
- Stack with `@effect/sql-sqlite-node` / `@effect/sql-sqlite-wasm` (`.api/effect-sql-sqlite-node.md`, `-wasm`): the three sqlite lanes share the neutral `SqlClient` contract and the `lane/sqlite` statements — node for server sidecars over `better-sqlite3`, bun for the bun runtime over `bun:sqlite`, wasm/OPFS for `browser/persist` — so a lane is a Layer row under one contract. The driver-distinct supersets diverge: node adds online `backup`/`BackupMetadata`, wasm adds `import`/`OpfsWorker`/`withTransferables`, this lane adds only the `create`/`readwrite` open-mode flags; `wasm` swaps `filename` for an OPFS worker handle.
- Stack with `@effect/sql-pg` (`.api/effect-sql-pg.md`): the PG spine is the reference contract; this lane is its degraded mirror. `sql.onDialect` keeps one journal/projection statement across both, and the capability-degradation table is the map of what routes differently.
- Stack with `@effect/experimental` (`.api/effect-experimental.md`): `make`/`reactive` require `Reactivity`; the sqlite lane's read-your-writes signal is in-process `Reactivity.invalidate` (no LISTEN/NOTIFY channel). `@effect/platform-bun` `BunContext` supplies the `FileSystem`/`Path` the banned `SqliteMigrator` does need.
- Stack with `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): every statement auto-spans with `spanAttributes`; the exporter Layer under the graph ships them with no per-query wiring.

[LOCAL_ADMISSION]:
- compose the neutral `SqlClient` Tag in lane rows; reach for the `SqliteClient` Tag only for `export`/`loadExtension`. Journal/projection statements are shared with the spine via `sql.onDialect`, never re-authored per lane.
- express the `updateValues` degradation through `sql.onDialect`, never a lane-only helper; the degradation table is the parameterized owner of every PG-vs-sqlite divergence.
- `SqliteMigrator` (`run`/`layer`) is banned branch-wide with `PgMigrator`: DDL is idempotent declarative ensure, and with no RLS the sqlite tenancy law is file-per-app.
- `filename` and open mode are `Config`/tenancy facts, never literals in a row; keep the name transforms identical to the spine so a dialect swap is transparent.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-bun`
- Owns: the `bun:sqlite` binding of `SqlClient` to a `SqliteClient`, the `export` whole-db serialize, `loadExtension`, the `updateValues: never` degradation anchor, and `layer`/`layerConfig`/`make` construction
- Accept: `SqliteClient` as a `SqlClient` provider, journal/projection statements shared with the PG spine through `sql.onDialect`, the capability-degradation table as DATA, in-process `Reactivity` for read-your-writes, `export` as the backup primitive, `filename`-per-app tenancy, `spanAttributes` auto-spanned
- Reject: a `SqliteClient`-typed row that is `SqlClient`, a lane-only re-authoring of a shared statement, `updateValues` faked outside `onDialect`, `SqliteMigrator` or any runtime schema mutation, a pool or RLS assumption on a single-connection file lane, hardcoded `filename`/mode
