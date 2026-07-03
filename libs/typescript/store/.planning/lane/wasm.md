# [STORE_WASM]

The OPFS sqlite-wasm lane — the browser's durable SQL under the same journal and projection contracts, one degradation tier below the server file lane: the engine is WebAssembly (`@effect/wa-sqlite` behind `@effect/sql-sqlite-wasm`), durability is an OPFS sync access handle that exists only inside a Web Worker, and the lane therefore splits — the main thread holds the `SqliteClient`, the worker runs `OpfsWorker.run` and owns the file. Tenancy is database-per-origin by platform fact; snapshot `import`/`export` round-trips seed a fresh browser database from a server snapshot or an EventLog replay and persist the ephemeral memory lane; `withTransferables` moves those blobs across the worker boundary zero-copy. `browser/persist` consumes this lane as its local-first projection store beneath the EventLog overlay — the overlay journal is IndexedDB, the queryable projections live here, and the record of truth remains the server journal `[R19]`. `[R13]` carries the OPFS capability-depth verification.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                            |
| :-----: | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `OPFS_SPLIT`  | the worker/main split — layer rows, the worker entry, the spawn shape, reactivity     |
|  [02]   | `SNAPSHOT_IO` | `import`/`export` seed-restore, zero-copy transfer, the memory lane                    |

## [2]-[OPFS_SPLIT]

- Owner: the two construction paths as layer rows — `WasmLane.opfs(worker)` for the durable worker-backed lane (the `dbName` is the worker entry's coordinate), `WasmLane.memory` for the ephemeral in-thread lane — plus `WasmLane.worker`, the worker-side program that binds the OPFS handle.
- Packages: `@effect/sql-sqlite-wasm` (`SqliteClient.layer`, `SqliteClient.layerMemory`, `OpfsWorker.run`, `installReactivityHooks`); `@effect/platform-browser` supplies the spawn (`BrowserWorker.layer(spawn)`) and the worker-side runner (`BrowserWorkerRunner.layerMessagePort`) at app composition — this page names the shapes, never the binding.
- Entry: `browser/persist` composes `WasmLane.opfs` on the `./wasm` subpath; the worker entry module runs `WasmLane.worker({ port, dbName })` and nothing else — a boot module under the boot law, exporting nothing.
- Growth: a second database per origin is a `dbName` value — the OPFS namespace already scopes by origin, so multi-app browser hosts key files by app exactly like the server file lane keys paths.
- Law: OPFS access is worker-only by platform contract — a main-thread open is structurally impossible in this design because the only durable constructor takes the worker effect; the memory lane is the sanctioned no-worker variant and its durability, if any, is an explicit snapshot round-trip.
- Law: `installReactivityHooks: true` restores `sql.reactive` in the browser — the inline lanes' `changes` streams and the read-your-writes law hold locally with in-process invalidation, the same key vocabulary as every other lane.
- Law: the degradation tier extends `lane/sqlite.md`'s table — everything the file lane refuses plus no server extensions (`vector → none`, semantic retrieval stays server-side) and single-writer OPFS; the rows are read through the same grant vocabulary, so a retrieval query on this lane composes only the lanes its grants admit.

```typescript
import { type ConfigError, Effect, Layer, type Scope } from "effect"
import type { SqlClient, SqlError } from "@effect/sql"
import * as WasmSqlite from "@effect/sql-sqlite-wasm"

declare namespace WasmLane {
  type Spawn = Effect.Effect<Worker | SharedWorker | MessagePort, never, Scope.Scope>
}

const _opfs = (
  worker: WasmLane.Spawn,
): Layer.Layer<WasmSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  WasmSqlite.SqliteClient.layer({
    worker,
    installReactivityHooks: true,
  })

const _memory: Layer.Layer<WasmSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  WasmSqlite.SqliteClient.layerMemory({
    installReactivityHooks: true,
  })

const _worker = (options: { readonly port: MessagePort; readonly dbName: string }) =>
  WasmSqlite.OpfsWorker.run({ port: options.port, dbName: options.dbName })
```

## [3]-[SNAPSHOT_IO]

- Owner: the seed/restore surface — `seed` imports a snapshot blob into a fresh database, `dump` exports the whole database as bytes, both moving blobs with `withTransferables` so the worker boundary copies nothing.
- Packages: `@effect/sql-sqlite-wasm` (`client.import`, `client.export`, `SqliteClient.withTransferables`).
- Entry: `browser/persist` seeds a first-run database from a server-minted snapshot object (fetched by content key through its transport) or from an EventLog replay; the memory lane persists across sessions by `dump`-then-`seed` through its own storage row.
- Receipt: `dump` yields the raw `Uint8Array` — the browser cannot mint into the server object plane directly; shipping a browser snapshot is an app flow through `edge`, so the bytes are the receipt here.
- Growth: a new seed source (server snapshot, replay, fixture) is a caller decision — the surface takes bytes and nothing else.
- Law: seed-then-verify — after `import`, the lane's ensure rows run their relation probes exactly like server startup, so a truncated or foreign blob fails closed at seed time, never at first query.
- Law: blobs transfer, never copy — `withTransferables([bytes.buffer])` wraps every `import`/`export` crossing; a structured-clone copy of a multi-megabyte database is the named waste.
- Boundary: which snapshot to seed and when is `browser/persist`'s composition; the EventLog overlay client (IndexedDB journal, WebSocket sync) is `@effect/experimental` composed by `browser` — this lane is only the queryable SQL beneath it `[R19]`.

```typescript
const _seed = (bytes: Uint8Array) =>
  Effect.flatMap(WasmSqlite.SqliteClient.SqliteClient, (client) =>
    WasmSqlite.SqliteClient.withTransferables([bytes.buffer])(client.import(bytes)))

const _dump = Effect.flatMap(WasmSqlite.SqliteClient.SqliteClient, (client) => client.export)

const WasmLane = {
  opfs: _opfs,
  memory: _memory,
  worker: _worker,
  seed: _seed,
  dump: _dump,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { WasmLane }
```
