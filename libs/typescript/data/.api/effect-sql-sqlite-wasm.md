# [TS_DATA_API_EFFECT_SQL_SQLITE_WASM]

`@effect/sql-sqlite-wasm` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to a WebAssembly sqlite running in a Web Worker over OPFS — the `browser/persist` dialect lane. `SqliteClient extends SqlClient`, so the fragment DSL, `SqlSchema`/`Model`, `withTransaction`, and `sql.reactive` run in the browser unchanged against a persistent database, and the driver adds only its browser-distinct construction, snapshot, and worker surface at the deepest capability-degradation tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-wasm`
- package: `@effect/sql-sqlite-wasm` (MIT)
- effect-peer: `effect`, `@effect/experimental`, `@effect/sql`, `@effect/wa-sqlite` — the WASM sqlite engine, no `better-sqlite3`, no native dependency
- module: ESM + CJS dual; subpaths `/SqliteClient`, `/SqliteMigrator`, `/OpfsWorker`, `/sqlite-wasm.d`; `sideEffects: []`
- runtime: browser only — Web Worker + OPFS + Web Crypto; the OPFS sync access handle binds a worker thread
- rail: the `store` `lane/wasm` browser dialect — server-lane journal/projection contracts under the deepest capability-degradation tier
- modules: `SqliteClient`, `OpfsWorker`, `SqliteMigrator` (banned re-export)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the wasm client extension, its two configs, and the OPFS worker
- rail: boundaries
- `SqliteClient` supersets the neutral `SqlClient`; this catalog documents only the wasm-driver additions over `.api/effect-sql.md`. `SqliteClientConfig` carries `worker: Effect<Worker \| SharedWorker \| MessagePort, never, Scope>`, `OpfsWorkerConfig` is `{ port, dbName }`, and both client configs also take `installReactivityHooks?`, `spanAttributes?`, `transformResultNames?`, `transformQueryNames?`.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                                              |
| :-----: | :--------------------------------- | :--------------- | :------------------------------------------------------------------------------- |
|  [01]   | `SqliteClient`                     | `Context.Tag`    | the wasm lane client; also the neutral `SqlClient`, plus the snapshot members    |
|  [02]   | `SqliteClient.export`              | db snapshot      | whole-DB serialize — sync-overlay seed export, `browser/persist` backup blob     |
|  [03]   | `SqliteClient.import(data)`        | db restore       | load a `Uint8Array` snapshot — seed a fresh DB, the memory-lane round-trip       |
|  [04]   | `SqliteClient.updateValues: never` | degradation      | declared degradation (no multi-row `UPDATE … FROM`) — branch via `sql.onDialect` |
|  [05]   | `SqliteClientConfig`               | worker config    | the OPFS-persistent worker lane                                                  |
|  [06]   | `SqliteClientMemoryConfig`         | in-thread config | ephemeral, no OPFS — no `worker` field                                           |
|  [07]   | `OpfsWorkerConfig`                 | worker entry cfg | the worker-side bind params                                                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the browser lane and its OPFS worker
- rail: rails-and-effects
- `layer`/`layerConfig`/`layerMemory`/`layerMemoryConfig` wire `Reactivity` internally and return `Layer<SqliteClient \| SqlClient, ConfigError \| SqlError>`; `make`/`makeMemory` return `Effect<SqliteClient, SqlError, Scope \| Reactivity>`; `OpfsWorker.run` returns `Effect<void, SqlError>`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                     |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `layer(config)`                                            | OPFS lane      | the `./wasm` durable worker-backed lane                 |
|  [02]   | `layerConfig(wrap)` / `layerMemoryConfig(wrap)`            | config lane    | config-sourced worker/memory lane, parameterized        |
|  [03]   | `layerMemory(config)` / `makeMemory(config)`               | memory lane    | ephemeral in-thread client for specs/previews — no OPFS |
|  [04]   | `make(config)`                                             | build client   | scoped worker-backed; `config.worker`-owned lifetime    |
|  [05]   | `OpfsWorker.run({ port, dbName })`                         | worker entry   | binds wa-sqlite to the OPFS sync handle for `dbName`    |
|  [06]   | `withTransferables(list)(effect)` / `currentTransferables` | zero-copy      | zero-copy `Uint8Array` across the worker boundary       |
|  [07]   | `client.export` / `client.import(data)`                    | snapshot io    | seed/restore the browser DB; sync-overlay + memory-lane |

## [04]-[IMPLEMENTATION_LAW]

[SQLITE_WASM_TOPOLOGY]:
- `SqliteClient extends SqlClient`, so every journal/projection/retrieve row runs in the browser against OPFS yielding only the abstract `SqlClient` Tag; `lane/wasm` is a `Layer` selection on the `./wasm` subpath, no row browser-specific.
- OPFS sync access handles bind a Web Worker, so the durable lane splits: the main thread holds `SqliteClient` and the `config.worker` Effect spawns the worker running `OpfsWorker.run({ port, dbName })` that owns the OPFS file. `makeMemory` is the no-worker ephemeral variant, persisting only through an explicit `export`/`import` round-trip.
- `export → Uint8Array` and `import(Uint8Array)` seed a fresh browser DB from a server snapshot or an EventLog replay and persist the memory lane across sessions; `withTransferables` moves those blobs to the worker zero-copy through the structured-clone transfer list.
- `updateValues: never`, no RLS, no server extensions, and single-writer OPFS put the wasm lane at the bottom of the `capability/matrix` table; tenancy is database-per-origin, cross-row set operations branch through `sql.onDialect`, and `installReactivityHooks` restores `sql.reactive` for local read-your-writes.

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): the neutral surface — `sql` DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, `reactive` — runs unchanged; this package supplies the sqlite `Compiler` + wa-sqlite acquirer as the `SqlClient.MakeOptions` the neutral `make` folds, and `make`/`makeMemory` carry `Reactivity` so `sql.reactive` works in the browser.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): this lane is the queryable projection store beneath the EventLog local-first overlay — `EventJournal.layerIndexedDb` owns the append-only local journal, this lane holds the SQL projections `browser/persist` reads, `export`/`import` seed the DB from an EventLog replay `[OVERLAY_BOUNDARY_RULING]`, and `installReactivityHooks` feeds the `Reactivity` invalidation the browser read lanes wake on.
- Stack on `@effect/platform-browser` (`.api/effect-platform-browser.md`): `config.worker`'s `Worker | SharedWorker | MessagePort` is exactly the spawn shape `BrowserWorker.layer(spawn)` yields, `OpfsWorker.run`'s `port` is the message-port side `BrowserWorkerRunner.layerMessagePort(port)` binds, and `EventLogEncryption.layerSubtle` rides Web Crypto for the encrypted sync overlay.
- Stack across `store`: the neutral `SqlClient` port matches the server lanes, so `journal`/`project`/`retrieve` code is runtime-blind, the wasm degradation is a `lane/wasm` + `capability/matrix` row, and snapshot `export`/`import` interlocks with the content-addressed `object` plane for browser-side blob seeding.

[LOCAL_ADMISSION]:
- Provide `SqliteClient.layer`/`layerConfig` on the `./wasm` subpath at the browser app root; a neutral journal/projection/retrieve row yields the abstract `SqlClient`.
- Run `OpfsWorker.run` in the worker thread and hold `SqliteClient` on the main thread — the OPFS sync access handle is worker-only.
- Use `export`/`import` for seed/restore and memory-lane persistence, moving blobs zero-copy with `withTransferables`.
- Express dialect variance through `sql.onDialect`: the `updateValues: never` gap and single-writer OPFS are one `capability/matrix` degradation row, never a per-runtime journal/projection fork.
- Browser schema is `iac`-owned declarative ensure verified at lane startup; the `SqliteMigrator` re-export stays banned.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-wasm`
- Owns: the browser `SqliteClient` Layer over `@effect/wa-sqlite` + OPFS behind the neutral `SqlClient` Tag, the two-path construction (`makeMemory` ephemeral vs `make` worker-backed OPFS), snapshot `export`/`import`, the `OpfsWorker.run` worker entry, zero-copy `currentTransferables`/`withTransferables`, `installReactivityHooks`, and the `updateValues: never` degradation marker
- Accept: `layer`/`layerConfig` on the `./wasm` subpath, `OpfsWorker.run` in the worker thread, the neutral `SqlClient` yielded by every runtime-agnostic row, `sql.onDialect` for the degradation gaps, `export`/`import` + `withTransferables` for snapshot seed/restore, `Reactivity` via `installReactivityHooks` for `sql.reactive`
- Reject: a driver import in a neutral row, main-thread OPFS access, a per-runtime journal/projection fork, `SqliteMigrator`/migration use, reliance on `updateValues` or server-only extensions, a native/`better-sqlite3` assumption
