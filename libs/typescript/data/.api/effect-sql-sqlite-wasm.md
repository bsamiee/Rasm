# [@effect/sql-sqlite-wasm] — the OPFS browser sqlite dialect driver: the worker-backed `SqliteClient` Layer behind `lane/wasm`, plus snapshot import/export and zero-copy worker transferables

`@effect/sql-sqlite-wasm` binds the neutral `@effect/sql` `SqlClient` (`.api/effect-sql.md`) to a WebAssembly sqlite (`@effect/wa-sqlite`) running inside a Web Worker over OPFS — the browser-runtime dialect lane `browser/persist` consumes. `SqliteClient` extends `SqlClient`, so the entire fragment DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, and `sql.reactive` run in the browser unchanged against a persistent OPFS-backed database; the driver adds its runtime-distinct surface — a two-path construction (`makeMemory` ephemeral in-thread vs `make` durable worker-backed OPFS), `export`/`import` for `Uint8Array` snapshot round-trips (the sync-overlay seed/restore and the memory-lane persistence path), and `currentTransferables`/`withTransferables` for zero-copy blob transfer across the worker boundary. `OpfsWorker.run` is the worker-side entry that owns the OPFS sync access handle (available only off the main thread). `updateValues: never` marks the dialect degradation; the wasm lane degrades deeper than the node lane (no server extensions, single-writer OPFS). No native dependency and no `better-sqlite3` — the peer is `@effect/wa-sqlite`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql-sqlite-wasm`
- package: `@effect/sql-sqlite-wasm`
- version: `0.52.0`
- license: `MIT`
- effect-peer: `effect ^3.21.0`, `@effect/experimental ^0.60.0`, `@effect/sql ^0.51.0`, `@effect/wa-sqlite ^0.1.2` (REQUIRED — the WASM sqlite build; `.api/effect.md`, `.api/effect-experimental.md`, `.api/effect-sql.md`)
- dependency: none native — the sqlite engine is WebAssembly via the `@effect/wa-sqlite` peer; no `better-sqlite3`
- module format: ESM + CJS dual (`dist/dts` typings); subpaths `@effect/sql-sqlite-wasm/SqliteClient`, `/SqliteMigrator`, `/OpfsWorker`, `/sqlite-wasm.d`; `sideEffects: []`
- runtime: browser only (Web Worker + OPFS + Web Crypto) — the OPFS sync access handle requires a worker; not node/bun server (that is `@effect/sql-sqlite-node`, `.api/effect-sql-sqlite-node.md`)
- rail: the `store` `lane/wasm` browser dialect — the same journal/projection contracts as the server lanes, under the deepest capability-degradation tier
- modules: `SqliteClient`, `OpfsWorker`, `SqliteMigrator` (banned re-export)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the wasm client extension, its two configs, and the OPFS worker
- rail: boundaries
- `SqliteClient` is a superset of the neutral `SqlClient`; the catalog documents only what the wasm driver ADDS over `.api/effect-sql.md`. Two configs split the durability model: `SqliteClientMemoryConfig` is in-thread ephemeral (specs, scratch), `SqliteClientConfig` supplies the `worker` Effect that owns the OPFS-persistent database off the main thread.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `SqliteClient` (interface, extends `SqlClient`)      | `Context.Tag`     | the wasm lane client; usable as the neutral `SqlClient` OR for the snapshot members below |
|  [02]   | `SqliteClient.export` (`Effect<Uint8Array, SqlError>`) | db snapshot     | whole-database serialization — sync-overlay seed export, `browser/persist` backup blob |
|  [03]   | `SqliteClient.import(data)` (`→ Effect<void, SqlError>`) | db restore     | load a `Uint8Array` snapshot — seed/restore a fresh browser DB, the memory-lane persistence round-trip |
|  [04]   | `SqliteClient.updateValues: never`                  | degradation       | the declared lane degradation (no multi-row `UPDATE … FROM`) — branch via `sql.onDialect` |
|  [05]   | `SqliteClientConfig`                                | worker config     | `worker: Effect<Worker \| SharedWorker \| MessagePort, never, Scope>`, `installReactivityHooks?`, `spanAttributes?`, `transformResultNames?`/`transformQueryNames?` |
|  [06]   | `SqliteClientMemoryConfig`                          | in-thread config  | `installReactivityHooks?`, `spanAttributes?`, name transforms — ephemeral, no OPFS |
|  [07]   | `OpfsWorkerConfig`                                  | worker entry cfg  | `{ port: EventTarget & Pick<MessagePort, "postMessage" \| "close">, dbName }` — the worker-side bind params |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: constructing the browser lane and its OPFS worker
- rail: rails-and-effects
- Two make paths select durability: `makeMemory`/`layerMemory` for ephemeral in-thread (specs, previews), `make`/`layer` for the durable OPFS lane whose `worker` Effect spawns/attaches the worker that runs `OpfsWorker.run`. `installReactivityHooks` wires `Reactivity` so `sql.reactive` drives `browser/persist` read-your-writes; `withTransferables` moves large `Uint8Array`s to the worker without a copy.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `SqliteClient.layer(config)` → `Layer<SqliteClient \| SqlClient, ConfigError \| SqlError>`          | provide OPFS lane | the `./wasm` composition — the durable worker-backed browser journal/projection lane |
|  [02]   | `SqliteClient.layerConfig(Config.Wrap<SqliteClientConfig>)` / `layerMemoryConfig(…)`               | provide lane   | config-sourced worker/memory lane; the parameterized form |
|  [03]   | `SqliteClient.layerMemory(config)` / `makeMemory(config)` → `Effect<…, SqlError, Scope \| Reactivity>` | provide memory lane | ephemeral in-thread client for specs/previews — no OPFS persistence |
|  [04]   | `SqliteClient.make(config)` → `Effect<SqliteClient, SqlError, Scope \| Reactivity>`                 | build client   | scoped worker-backed client; the `config.worker` Effect owns the OPFS worker lifetime |
|  [05]   | `OpfsWorker.run({ port, dbName })` → `Effect<void, SqlError>`                                       | worker entry   | the worker-thread program binding wa-sqlite to the OPFS sync access handle for `dbName` |
|  [06]   | `SqliteClient.withTransferables(list)(effect)` / `currentTransferables` (`FiberRef`)               | zero-copy      | transfer (not copy) `Uint8Array` payloads across the worker boundary — `import`/`export` blob moves |
|  [07]   | `client.export` / `client.import(data)`                                                            | snapshot io    | seed/restore the browser DB; the sync-overlay bootstrap and the memory-lane persistence path |

## [04]-[IMPLEMENTATION_LAW]

[SQLITE_WASM_TOPOLOGY]:
- neutral-Tag superset in the browser: `SqliteClient extends SqlClient`, so the same journal/projection/retrieve rows that run on the pg spine and the node lane run in the browser against OPFS — the abstract `SqlClient` is the only Tag they yield. `lane/wasm` is a `Layer` selection on the `./wasm` subpath; no row is browser-specific.
- durability is the worker: OPFS sync access handles exist only inside a Web Worker, so the durable lane splits — the main thread holds the `SqliteClient`, and the `config.worker` Effect spawns/attaches a worker running `OpfsWorker.run({ port, dbName })` that owns the OPFS file. `makeMemory` is the no-worker ephemeral variant; its persistence, if any, is an explicit `export`/`import` `Uint8Array` round-trip, not OPFS.
- snapshot round-trip is the seed/restore path: `export → Uint8Array` and `import(Uint8Array)` are how a fresh browser DB is seeded from a server snapshot or an EventLog replay, and how the memory lane persists across sessions. `withTransferables` moves those blobs to the worker zero-copy via the structured-clone transfer list.
- deepest degradation tier: `updateValues: never` plus no RLS, no server-side extensions, and single-writer OPFS — the wasm lane sits at the bottom of the `capability/matrix` degradation table. Tenancy is database-per-origin (OPFS is origin-scoped), and cross-row set operations branch through `sql.onDialect`. `installReactivityHooks` restores `sql.reactive` so read-your-writes still holds locally.

[INTEGRATION_LAW]:
- Stack on `@effect/sql` (`.api/effect-sql.md`): the neutral surface — `sql` DSL, `SqlSchema`/`SqlResolver`/`Model`, `withTransaction`, `reactive` — runs unchanged; this package supplies the sqlite `Compiler` + wa-sqlite acquirer as `SqlClient.MakeOptions` the neutral `make` folds. `make`/`makeMemory` require `Reactivity`, so `sql.reactive` works in the browser.
- Stack on `@effect/experimental` (`.api/effect-experimental.md`): the wasm SQL lane is the queryable projection store beneath the EventLog local-first overlay — `EventJournal.layerIndexedDb` is the append-only local journal, and this lane holds the SQL projections `browser/persist` reads; `export`/`import` seed the DB from an EventLog replay `[R19]`. `installReactivityHooks` feeds the `Reactivity` invalidation signal the browser read lanes wake on.
- Stack on `@effect/platform-browser` (`.api/effect-platform-browser.md`): `config.worker`'s `Worker | SharedWorker | MessagePort` is exactly the spawn shape `BrowserWorker.layer(spawn)` yields, and `OpfsWorker.run`'s `port` is the message-port side `BrowserWorkerRunner.layerMessagePort(port)` binds; Web Crypto (`EventLogEncryption.layerSubtle`) rides alongside for the encrypted sync overlay.
- Stack across `store`: the neutral `SqlClient` port is identical to the server lanes, so `journal`/`project`/`retrieve` code is runtime-blind; the wasm degradation is a `lane/wasm` + `capability/matrix` row; snapshot `export`/`import` interlocks with the content-addressed `object` plane for browser-side blob seeding.

[LOCAL_ADMISSION]:
- Provide `SqliteClient.layer`/`layerConfig` on the `./wasm` runtime subpath at the browser app root; never import the driver in a neutral journal/projection/retrieve row — those yield the abstract `SqlClient`.
- Run `OpfsWorker.run` in the worker thread and hold the `SqliteClient` on the main thread; never attempt OPFS from the main thread (the sync access handle is worker-only).
- Use `export`/`import` for seed/restore and memory-lane persistence, moving blobs with `withTransferables` (zero-copy); never serialize a DB through a string.
- Express dialect variance via `sql.onDialect` (`updateValues: never`, single-writer OPFS); never fork a journal/projection row per runtime — the wasm lane is one `capability/matrix` degradation row.
- The `SqliteMigrator` re-export is banned; browser schema is `iac`-owned declarative ensure verified at lane startup, never a migration run.

[RAIL_LAW]:
- Package: `@effect/sql-sqlite-wasm`
- Owns: the browser `SqliteClient` Layer over `@effect/wa-sqlite` + OPFS behind the neutral `SqlClient` Tag, the two-path construction (`makeMemory` ephemeral vs `make` worker-backed OPFS), snapshot `export`/`import`, the `OpfsWorker.run` worker entry, zero-copy `currentTransferables`/`withTransferables`, `installReactivityHooks`, and the `updateValues: never` degradation marker
- Accept: `layer`/`layerConfig` on the `./wasm` subpath, `OpfsWorker.run` in the worker thread, the neutral `SqlClient` yielded by every runtime-agnostic row, `sql.onDialect` for the degradation gaps, `export`/`import` + `withTransferables` for snapshot seed/restore, `Reactivity` (via `installReactivityHooks`) for `sql.reactive`
- Reject: a driver import in a neutral row, main-thread OPFS access, a per-runtime journal/projection fork, `SqliteMigrator`/migration use, reliance on `updateValues` or server-only extensions, and a native/`better-sqlite3` assumption (the engine is WASM)
