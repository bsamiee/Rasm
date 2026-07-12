# [TS_DATA_API_EFFECT_SQL_SQLITE_NODE]

`@effect/sql-sqlite-node` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to a synchronous in-process `better-sqlite3` connection — the server-runtime dialect lane. `SqliteClient` extends `SqlClient`, so the entire fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, and the SQL overlay bindings compose unchanged against a sqlite file; the driver adds only its runtime-distinct surface — `export` (whole-database `Uint8Array` snapshot), `backup` (online page-progress backup), `loadExtension` (runtime `.so`/`.dylib` load — the sqlite analog to the pg capability matrix), a prepared-statement cache, and WAL control. `updateValues: never` marks the one degradation the dialect declares (no multi-row `UPDATE … FROM`). `layerConfig` sources the filename from `Config` behind `host/config`, and provides both `SqliteClient` (driver capability) and the neutral `SqlClient` the journal/projection rows depend on. The bundled `SqliteMigrator` re-export is branch-banned; DDL is `iac`↔`store` declarative ensure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-node`
- package: `@effect/sql-sqlite-node`
- license: `MIT`
- effect-peer: `effect ^catalog`, `@effect/platform ^catalog`, `@effect/experimental ^catalog`, `@effect/sql ^catalog` (`.api/effect.md`, `.api/effect-platform.md`, `.api/effect-experimental.md`, `.api/effect-sql.md`)
- dependency: `better-sqlite3 ^catalog` (bundled; synchronous in-process N-API sqlite with prebuilt binaries)
- module format: ESM + CJS dual (`dist/dts` typings); subpaths `@effect/sql-sqlite-node/SqliteClient`, `/SqliteMigrator`; `sideEffects: []`
- runtime: node/bun server only — `better-sqlite catalog` is native and synchronous; not browser/wasm (that is `@effect/sql-sqlite-wasm`, `.api/effect-sql-sqlite-wasm.md`)
- rail: the `store` `lane/sqlite` node dialect — the same journal/projection contracts as the pg spine, under the sqlite capability-degradation table
- modules: `SqliteClient`, `SqliteMigrator` (banned re-export)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the sqlite client extension and its config
- rail: boundaries
- `SqliteClient` is a superset of the neutral `SqlClient` — it satisfies the abstract Tag every `store` row yields AND carries the driver-distinct capability. The catalog documents only what the driver ADDS over `.api/effect-sql.md`; the fragment DSL, transactions, `SqlSchema`/`Model`, and overlay bindings are the neutral surface unchanged.

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]        | [CONSUMER_BOUNDARY]                                                                                                                                |
| :-----: | :----------------------------------------------------------------- | :------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SqliteClient` (interface, extends `SqlClient`)                    | `Context.Tag`        | the sqlite lane client; usable as the neutral `SqlClient` OR for the driver-distinct members below                                                 |
|  [02]   | `SqliteClient.export` (`Effect<Uint8Array, SqlError>`)             | db snapshot          | whole-database serialization — snapshot/DR export, `journal` cold-copy, content-addressed backup blob                                              |
|  [03]   | `SqliteClient.backup(dest)` (`→ Effect<BackupMetadata, SqlError>`) | online backup        | live page-progress backup to a file without blocking writers; `BackupMetadata` = `{ totalPages, remainingPages }`                                  |
|  [04]   | `SqliteClient.loadExtension(path)` (`→ Effect<void, SqlError>`)    | extension load       | runtime `.so`/`.dylib` load (sqlite-vec, spellfix) — the sqlite analog of the pg `capability/matrix` extension row                                 |
|  [05]   | `SqliteClient.config` / `SqliteClient.updateValues: never`         | config / degradation | the resolved config; `updateValues: never` is the declared lane degradation (no multi-row `UPDATE … FROM`)                                         |
|  [06]   | `SqliteClientConfig`                                               | driver config        | `filename`, `readonly?`, `prepareCacheSize?`, `prepareCacheTTL?`, `disableWAL?`, `spanAttributes?`, `transformResultNames?`/`transformQueryNames?` |
|  [07]   | `BackupMetadata`                                                   | progress record      | `{ totalPages, remainingPages }` — poll to observe online-backup completion                                                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing and providing the sqlite lane Layer
- rail: rails-and-effects
- `layer`/`layerConfig` provide `SqliteClient | SqlClient` in one Layer, so a row that yields the neutral `SqlClient` and a row that needs `backup`/`loadExtension` share one binding. `layerConfig` sources the filename and cache knobs from `Config` behind `host/config` — no hardcoded path. WAL is on by default (append throughput for the journal); `disableWAL` is the explicit opt-out.

| [INDEX] | [SURFACE]                                                                                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                                          |
| :-----: | :------------------------------------------------------------------------------------------------------------ | :------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `SqliteClient.layer(config)` → `Layer<SqliteClient \| SqlClient, ConfigError>`                                | provide lane   | the app root's `./server` composition — binds the sqlite journal/projection lane                             |
|  [02]   | `SqliteClient.layerConfig(Config.Wrap<SqliteClientConfig>)` → `Layer<SqliteClient \| SqlClient, ConfigError>` | provide lane   | filename/cache/WAL from `Config` (`PlatformConfigProvider`); the parameterized, no-hardcoded-path form       |
|  [03]   | `SqliteClient.make(config)` → `Effect<SqliteClient, never, Scope \| Reactivity>`                              | build client   | scoped client construction; requires `Reactivity` so `sql.reactive` works on the lane                        |
|  [04]   | `client.export` / `client.backup(path)` / `client.loadExtension(path)`                                        | driver ops     | snapshot/DR + extension capability — composed only where the concrete lane, not the neutral Tag, is in scope |

## [04]-[IMPLEMENTATION_LAW]

[SQLITE_NODE_TOPOLOGY]:
- neutral-Tag superset: `SqliteClient extends SqlClient`, so `layer(config)` provides both — the journal/projection/capability/retrieve rows keep yielding the abstract `SqlClient` and stay driver-agnostic, while a snapshot or extension-load row yields the concrete `SqliteClient`. Swapping to the bun lane (`@effect/sql-sqlite-bun`) or the pg spine (`@effect/sql-pg`) is a `Layer` selection at the app root; no row changes.
- capability degradation is a lane fact, not a fork: `updateValues: never` is the type-level signal that the sqlite dialect lacks multi-row `UPDATE … FROM`; a row needing it uses `sql.onDialect` to emit per-row updates on sqlite and set-based updates on pg. The broader degradation table (no RLS → file-per-app tenancy in `scope/tenant`; no `pg_ivm` → in-process `state` folds in `project`) is owned by `lane/sqlite` + `capability/matrix`, parameterized over the dialect — this driver contributes the sqlite row, never a parallel journal.
- native + synchronous: `better-sqlite3` is an in-process synchronous N-API binding with prebuilt platform binaries; there is no connection pool or network round-trip, so `sql.withTransaction` is a real single-connection transaction and the prepared-statement cache (`prepareCacheSize`/`prepareCacheTTL`) is the hot-path lever. WAL (default on) is the concurrent-read + append-throughput mode the journal relies on.
- extension load is the sqlite capability probe: `loadExtension` is how the lane admits a native sqlite capability (vector search, phonetic) — the fail-closed analog to the pg extension matrix. FTS5 and JSON1 are compiled-in; `capability/row` probes both dialects behind one closed vocabulary.

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): everything neutral — the `sql` fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`/savepoints, `reactive`, and the `SqlEventJournal`/`SqlPersistedQueue` overlay bindings — runs against this driver unchanged; this package only supplies the `SqlClient.MakeOptions` (sqlite `Compiler`, acquirer, transaction machinery) the neutral `make` folds.
- Stack on `@effect/platform` (`.api/effect-platform.md`): `layerConfig` reads `filename`/cache knobs from `Config` behind `PlatformConfigProvider`; the banned `SqliteMigrator` (`run`/`layer`) is the reason `FileSystem`/`Path`/`CommandExecutor` appear in the driver's peer graph — `store` never composes it.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): `make` requires `Reactivity`, so `sql.reactive` read-your-writes works on the sqlite lane; `SqlPersistedQueue.layerStore` (from the core) gives `work` a durable job store on the file lane. EventLog's node journal can back onto this lane via `SqlEventJournal.layer` `[R4]`.
- Stack across `store`: `SqliteClient.export`/`backup` feed the content-addressed `object` plane (a snapshot blob is content-keyed via `object/key`); `loadExtension` extends `capability/matrix` for the sqlite dialect; `work`/`security/session` see only the neutral `SqlClient` port the lane Layer satisfies.

[LOCAL_ADMISSION]:
- Provide `SqliteClient.layerConfig` on the `./server` runtime subpath at the app root only; never import the driver in a neutral journal/projection/capability row — those yield the abstract `SqlClient`.
- Yield the concrete `SqliteClient` only where `export`/`backup`/`loadExtension` are genuinely needed (snapshot, DR, extension admission); everywhere else yield `SqlClient` to stay dialect-portable.
- Express dialect variance through `sql.onDialect` (the `updateValues: never` gap is the canonical case); never fork a journal or projection row per driver.
- Keep WAL on for the journal lane unless a read-only replica explicitly sets `disableWAL`; source `filename` from `Config`, never a literal path.
- The `SqliteMigrator` re-export is banned — no migration run; schema is `iac` declarative ensure verified at `store` startup.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-node`
- Owns: the node/bun-server `SqliteClient` Layer over `better-sqlite3` behind the neutral `SqlClient` Tag, plus the driver-distinct capability — whole-db `export`, online `backup`/`BackupMetadata`, runtime `loadExtension`, the prepared-statement cache and WAL control (`SqliteClientConfig`), and the `updateValues: never` degradation marker
- Accept: `layerConfig` on the `./server` subpath sourcing `filename`/cache from `Config`, the neutral `SqlClient` yielded by every dialect-agnostic row, `sql.onDialect` for the sqlite/pg gaps, `export`/`backup`/`loadExtension` where the concrete lane is in scope, `Reactivity` for `sql.reactive`
- Reject: a driver import in a neutral row, a per-driver journal/projection fork, a hardcoded filename, `SqliteMigrator`/migration use, and reliance on `updateValues` (unsupported on sqlite — branch via `onDialect`)
