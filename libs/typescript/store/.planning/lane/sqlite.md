# [STORE_SQLITE]

The sqlite node/bun lane: the same journal, projection, scope, and retrieval contracts running against a single-writer WAL file, with every divergence from the pg spine recorded as one degradation row — no RLS means file-per-app tenancy, no `pg_ivm` means the async lanes own every fold, no LISTEN/NOTIFY means the poll arm of the wake merge, no COPY means chunked inserts, no `updateValues` means per-row updates, no advisory locks means the single writer already serializes. The lane is a Layer selection on the `./server` subpath — node binds `better-sqlite3` with its prepare cache and online backup, bun binds `bun:sqlite` with its open-mode flags — and no neutral row ever names either driver; `sql.onDialectOrElse` arms inside the shared statements are the entire dialect story.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                       |
| :-----: | :------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `DEGRADATION_TABLE` | the closed capability-to-fallback rows — the lane's whole difference as data      |
|  [02]   | `LANE_LAYERS`       | node/bun layer rows, file-per-app naming, backup-to-object snapshot, extensions   |

## [2]-[DEGRADATION_TABLE]

- Owner: the `_degrades` anchor — one row per pg grant the lane refuses, carrying the fallback verdict its consumers dispatch on; the derived `SqliteLane.Fallback` union.
- Packages: none — the table is pure vocabulary over `capability/matrix.md`'s grant keys.
- Growth: a new pg capability lands here the day it lands in the matrix — a missing row is a red gap, because every grant must answer what the file lane does.
- Law: the table is consumed, never consulted ad hoc — `project/async.md`'s wake reads `channel → poll` through the optional `PgClient` service, `journal/append.md`'s lock arm reads `advisory → singleWriter` through `onDialectOrElse`, `scope` reads `rls → filePerApp` by never constructing `Tenancy` scopes on this lane; the rows document the dispatch that already exists in the statements, so the lane and the spine cannot drift silently.
- Law: `search` degrades to FTS5 and `vector` to a loaded extension — sqlite is not capability-poor, it is capability-different; the retrieval lanes select their sqlite arms through the same grant vocabulary, and FTS5/JSON1 ship compiled into both drivers.

```typescript
const _degrades = {
  rls: { fallback: "filePerApp" },
  channel: { fallback: "poll" },
  advisory: { fallback: "singleWriter" },
  copy: { fallback: "chunkedInsert" },
  ivm: { fallback: "asyncLane" },
  cron: { fallback: "hostSchedule" },
  partition: { fallback: "snapshotTruncate" },
  queue: { fallback: "outboxTable" },
  vector: { fallback: "loadExtension" },
  search: { fallback: "fts5" },
  uuidv7: { fallback: "appMint" },
} as const

declare namespace SqliteLane {
  type Degraded = keyof typeof _degrades
  type Fallback = (typeof _degrades)[Degraded]["fallback"]
  type _Rows<T extends Record<Degraded, { readonly fallback: string }> = typeof _degrades> = T
}
```

## [3]-[LANE_LAYERS]

- Owner: the two driver layer rows and the lane operations that are genuinely lane-native — `snapshot` (whole-database `export` content-addressed into the object plane), `restore` guidance, `extend` (runtime extension load for the retrieval lanes), and the node-only online `backup`.
- Packages: `@effect/sql-sqlite-node` (`SqliteClient.layerConfig`, `client.export`, `client.backup`, `client.loadExtension`, `prepareCacheSize`); `@effect/sql-sqlite-bun` (`SqliteClient.layer`, `create`/`readwrite` flags); `object/key.md` (`ObjectStore.put` — the snapshot sink).
- Entry: the app root selects `SqliteLane.node(scope)` or `SqliteLane.bun(scope)` per `host/exec`'s runtime row — both provide `SqliteClient | SqlClient`, and every store surface above them is unchanged; `[R1]` gates the bun row load-bearing until the install proof lands.
- Receipt: `snapshot` returns the `ObjectStore.Receipt` of the exported database — a content-addressed, re-put-safe backup whose key IS its bytes.
- Growth: a lane-tuning knob (cache size, WAL toggle) is a `Config` field on the layer row; a third sqlite runtime is one more layer row under the same contracts.
- Law: the filename derives from the scope — `file-per-app` IS the tenancy: `_filename(app)` keys the file, `":memory:"` serves specs, and the name transforms match the pg spine so `onDialect` statements agree about column spellings.
- Law: WAL stays on for journal lanes (readers concurrent with the single writer); `disableWAL` is the explicit read-replica opt-out; the prepare cache is the node lane's hot-path lever and bun caches internally — both are layer facts, never statement facts.
- Law: `loadExtension` is the sqlite capability probe's write half — `retrieve/index.md`'s sqlite vector row loads its extension here and then registers through the same fail-closed vocabulary; a load failure refuses the grant, never crashes the lane.
- Boundary: the wasm/OPFS lane is `lane/wasm.md`'s — browser-only, worker-split, and one degradation tier deeper; the `SqliteMigrator` re-export stays banned branch-wide with `PgMigrator`.

```typescript
import { Config, Effect, Layer } from "effect"
import type { SqlClient } from "@effect/sql"
import * as NodeSqlite from "@effect/sql-sqlite-node"
import * as BunSqlite from "@effect/sql-sqlite-bun"
import type { AppKey } from "@rasm/ts/kernel"
import { ObjectStore } from "../object/key.ts"

const _filename = (app: AppKey): Config.Config<string> =>
  Config.string("STORE_SQLITE_DIR").pipe(
    Config.withDefault("."),
    Config.map((dir) => `${dir}/app_${app}.db`),
  )

const _node = (app: AppKey): Layer.Layer<NodeSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, unknown> =>
  NodeSqlite.SqliteClient.layerConfig({
    filename: _filename(app),
    prepareCacheSize: Config.integer("STORE_SQLITE_PREPARE_CACHE").pipe(Config.withDefault(200)),
    disableWAL: Config.boolean("STORE_SQLITE_DISABLE_WAL").pipe(Config.withDefault(false)),
  })

const _bun = (app: AppKey): Layer.Layer<BunSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, unknown> =>
  Layer.unwrapEffect(
    Effect.map(_filename(app), (filename) =>
      BunSqlite.SqliteClient.layer({
        filename,
        create: true,
        readwrite: true,
        disableWAL: false,
      })),
  )

const _snapshot = Effect.gen(function* () {
  const client = yield* NodeSqlite.SqliteClient.SqliteClient
  const store = yield* ObjectStore
  const bytes = yield* client.export
  return yield* store.put(bytes, { retention: "operational" })
})

const _extend = (path: string) =>
  Effect.flatMap(NodeSqlite.SqliteClient.SqliteClient, (client) => client.loadExtension(path))

const SqliteLane = {
  degrades: _degrades,
  filename: _filename,
  node: _node,
  bun: _bun,
  snapshot: _snapshot,
  extend: _extend,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { SqliteLane }
```
