# [DATA_SQLITE]

ONE sqlite lane, five profile rows: the same journal, projection, tenancy, and capability contracts running against node (`better-sqlite3`), bun (`bun:sqlite`), browser wasm-OPFS, libSQL edge-replica, and Cloudflare D1 — one relational contract degraded per profile, never a second relational universe. The degradation table is TOTAL over the spine's derived `Pg.Grant` vocabulary — its keys are guarded against that union in both directions, so a grant landing on the spine breaks this table at the declaration until every profile answers, and a degradation key outside the vocabulary cannot compile; the lane and the spine structurally cannot drift. The profile constructors are Layer rows on the runtime subpaths, and no neutral statement ever names a driver — `sql.onDialectOrElse` arms inside the shared statements are the entire dialect story. WAL is the durability mode every server profile runs; single-writer is the lane-wide concurrency law, deepening to single-tab on OPFS and primary-serialized on the edge rows.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                              |
| :-----: | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `DEGRADATION_TABLE` | the grant-total capability-to-fallback matrix — the lane's whole difference as data |
|  [02]   | `PROFILE_ROWS`      | the five Layer constructors and their runtime coordinates                           |
|  [03]   | `SNAPSHOT_IO`       | whole-database export/backup/seed, zero-copy transfer, extension load               |

## [02]-[DEGRADATION_TABLE]

- Owner: the `_degrades` anchor — one row per `Pg.Grant` member, carrying a verdict per profile column; the derived `Sqlite.Fallback` union every consumer dispatches on, and the two-directional guard pair binding the key space to the spine's grant union.
- Packages: none — the table is pure vocabulary over `lane/postgres.md`'s grant keys, reached through the one `Pg` import.
- Growth: a new spine grant breaks `_Rows` at this declaration the day it lands in the matrix — completeness is a compile fact, never a census; a sixth profile is one more column across every row.
- Law: the table is consumed, never consulted ad hoc — the projection wake reads `channel` through the optional `PgClient` service, the append lock arm reads `advisory` through `onDialectOrElse`, tenancy reads `rls` by never constructing `Tenancy` scopes on this lane; the rows document dispatch that already exists in the statements.
- Law: `none` is a lawful verdict — the analytics, geo, h3, timeseries, graphql, audit, and asyncIo grants have no profile substitute, so their rows record the refusal and every consumer gate simply never admits them here; a `none` row is a decision, and the guard makes an absent row impossible.
- Law: the lane is capability-different, not capability-poor — `bm25` degrades to FTS5, `vector` to a loaded extension on the server profiles and to the edge row's built-in on libSQL, `virtualGenerated` and `skipScan` are engine-native (`builtIn`/`plannerOwned`), and FTS5 plus JSONB ship compiled into every profile.
- Law: the evidence grants degrade to composed statements — `returningOldNew` becomes RETURNING of new values plus a pre-image read inside the same transaction (`preRead`), `conflictClaim` becomes the `ON CONFLICT` upsert discriminated by the explicit insert/update marker (`conflictChanges`), `merge` becomes the upsert arm pair (`upsert`), and `temporal` becomes an application overlap check serialized by the single writer (`appCheck`).
- Law: tenancy verdicts are residency verdicts — file-per-app on the server profiles, origin scope in the browser, database-per-tenant on both edge rows where cheap databases are the platform model; the RLS policy family never runs here.
- Law: the D1 column additionally refuses the interactive transaction — the atomic-publish path is batch-shaped or routed to the pg spine; the refusal is a row, not a code fork.

```typescript
import type { Pg } from "./postgres.ts"

const _degrades = {
  rls: { server: "filePerApp", wasm: "originScope", libsql: "databasePerTenant", d1: "databasePerTenant" },
  channel: { server: "poll", wasm: "reactivityHooks", libsql: "syncPull", d1: "none" },
  advisory: { server: "singleWriter", wasm: "singleTab", libsql: "primarySerialized", d1: "primarySerialized" },
  skipLocked: { server: "singleWriter", wasm: "singleTab", libsql: "primarySerialized", d1: "primarySerialized" },
  conflictClaim: { server: "conflictChanges", wasm: "conflictChanges", libsql: "conflictChanges", d1: "conflictChanges" },
  merge: { server: "upsert", wasm: "upsert", libsql: "upsert", d1: "upsert" },
  copy: { server: "chunkedInsert", wasm: "chunkedInsert", libsql: "chunkedInsert", d1: "batchInsert" },
  uuidv7: { server: "appMint", wasm: "appMint", libsql: "appMint", d1: "appMint" },
  returningOldNew: { server: "preRead", wasm: "preRead", libsql: "preRead", d1: "preRead" },
  virtualGenerated: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "builtIn" },
  temporal: { server: "appCheck", wasm: "appCheck", libsql: "appCheck", d1: "appCheck" },
  skipScan: { server: "plannerOwned", wasm: "plannerOwned", libsql: "plannerOwned", d1: "plannerOwned" },
  asyncIo: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  ivm: { server: "asyncLane", wasm: "inTabFold", libsql: "appSide", d1: "none" },
  cron: { server: "hostSchedule", wasm: "none", libsql: "appSide", d1: "platformCron" },
  partition: { server: "snapshotTruncate", wasm: "snapshotTruncate", libsql: "appSide", d1: "none" },
  incremental: { server: "checkpointLane", wasm: "none", libsql: "none", d1: "none" },
  vector: { server: "loadExtension", wasm: "none", libsql: "builtIn", d1: "none" },
  vchord: { server: "loadExtension", wasm: "none", libsql: "builtIn", d1: "none" },
  bm25: { server: "fts5", wasm: "fts5", libsql: "fts5", d1: "fts5" },
  trigram: { server: "fts5", wasm: "fts5", libsql: "fts5", d1: "fts5" },
  phonetic: { server: "loadExtension", wasm: "none", libsql: "none", d1: "none" },
  fuzzy: { server: "loadExtension", wasm: "none", libsql: "none", d1: "none" },
  jsonschema: { server: "schemaDecode", wasm: "schemaDecode", libsql: "schemaDecode", d1: "schemaDecode" },
  parquet: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  analytics: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  graphql: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  audit: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  geo: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  h3: { server: "none", wasm: "none", libsql: "none", d1: "none" },
  timeseries: { server: "none", wasm: "none", libsql: "none", d1: "none" },
} as const

declare namespace Sqlite {
  type Degraded = keyof typeof _degrades
  type Profile = keyof (typeof _degrades)[Degraded]
  type Fallback = (typeof _degrades)[Degraded][Profile]
  type _Rows<T extends Record<Pg.Grant, Record<Profile, string>> = typeof _degrades> = T
  type _Keys<K extends Pg.Grant = Degraded> = K
}
```

## [03]-[PROFILE_ROWS]

- Owner: the five Layer constructors — `Sqlite.node(app)` and `Sqlite.bun(app)` on the `./server` subpath, `Sqlite.opfs(worker)` and `Sqlite.memory` on `./wasm`, `Sqlite.libsql` and `Sqlite.d1(db)` at their edge composition roots — plus the worker-side entry `Sqlite.worker` the OPFS profile requires.
- Packages: `@effect/sql-sqlite-node` (`SqliteClient.layerConfig`, `prepareCacheSize`, `disableWAL`, `readonly`); `@effect/sql-sqlite-bun` (`SqliteClient.layerConfig`, `create`, `readwrite`); `@effect/sql-sqlite-wasm` (`SqliteClient.layer`, `SqliteClient.layerMemory`, `OpfsWorker.run`, `installReactivityHooks`); `@effect/sql-libsql` (`LibsqlClient.layerConfig`); `@effect/sql-d1` (`D1Client.layer`); `effect` (`Config`, `Layer`, `Scope`).
- Entry: the app root selects the profile row per the host runtime — all provide `SqlClient`, and every data surface above them is unchanged; the OPFS worker entry module runs `Sqlite.worker({ port, dbName })` and nothing else.
- Growth: a profile-tuning knob is a `Config` field on its row; a sixth runtime is one more constructor under the same contract plus its degradation column.
- Law: the filename derives from the scope — file-per-app IS the server tenancy: `_filename(app)` keys the file, `":memory:"` serves specs, and the naming matches the pg spine so `onDialect` statements agree about column spellings; the OPFS `dbName` and the libSQL replica path key the same way.
- Law: WAL stays on for journal profiles (readers concurrent with the single writer); `disableWAL` plus `readonly` is the explicit read-replica posture; the prepare cache is the node profile's hot-path lever, bun caches internally, D1 exposes its own cache pair — all layer facts, never statement facts.
- Law: OPFS access is worker-only by platform contract — the durable browser constructor takes the worker effect, so a main-thread open is unspellable; `installReactivityHooks: true` restores `sql.reactive` in-tab, the same key vocabulary as every lane.
- Law: the libSQL row is contract-level compatible, never byte-level — the replica engine is not the C library; its credentials and sync cadence ride `Config.redacted` and `Config` duration facts.
- Law: the D1 row adopts the platform binding as a value — `env.DB` arrives at the Workers composition root; replication sessions and PITR are platform facts recorded as degradation semantics, never re-modeled.

```typescript
import { Config, type ConfigError, Effect, Layer, type Scope } from "effect"
import type { SqlClient, SqlError } from "@effect/sql"
import * as NodeSqlite from "@effect/sql-sqlite-node"
import * as BunSqlite from "@effect/sql-sqlite-bun"
import * as WasmSqlite from "@effect/sql-sqlite-wasm"
import { LibsqlClient } from "@effect/sql-libsql"
import { D1Client } from "@effect/sql-d1"
import type { AppIdentity } from "@rasm/ts/core"

declare namespace Sqlite {
  type Spawn = Effect.Effect<Worker | SharedWorker | MessagePort, never, Scope.Scope>
}

const _filename = (app: AppIdentity.Key): Config.Config<string> =>
  Config.string("DATA_SQLITE_DIR").pipe(
    Config.withDefault("."),
    Config.map((dir) => `${dir}/app_${app}.db`),
  )

const _node = (app: AppIdentity.Key): Layer.Layer<NodeSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError> =>
  NodeSqlite.SqliteClient.layerConfig({
    filename: _filename(app),
    prepareCacheSize: Config.integer("DATA_SQLITE_PREPARE_CACHE").pipe(Config.withDefault(200)),
    disableWAL: Config.boolean("DATA_SQLITE_DISABLE_WAL").pipe(Config.withDefault(false)),
    readonly: Config.boolean("DATA_SQLITE_READONLY").pipe(Config.withDefault(false)),
  })

const _bun = (app: AppIdentity.Key): Layer.Layer<BunSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError> =>
  BunSqlite.SqliteClient.layerConfig({
    filename: _filename(app),
    create: Config.succeed(true),
    readwrite: Config.succeed(true),
    disableWAL: Config.boolean("DATA_SQLITE_DISABLE_WAL").pipe(Config.withDefault(false)),
  })

const _opfs = (
  worker: Sqlite.Spawn,
): Layer.Layer<WasmSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  WasmSqlite.SqliteClient.layer({ worker, installReactivityHooks: true })

const _memory: Layer.Layer<WasmSqlite.SqliteClient.SqliteClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  WasmSqlite.SqliteClient.layerMemory({ installReactivityHooks: true })

const _worker = (options: { readonly port: MessagePort; readonly dbName: string }): Effect.Effect<void, SqlError.SqlError> =>
  WasmSqlite.OpfsWorker.run({ port: options.port, dbName: options.dbName })

const _libsql: Layer.Layer<LibsqlClient.LibsqlClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  LibsqlClient.layerConfig({
    url: Config.string("DATA_LIBSQL_URL"),
    authToken: Config.redacted("DATA_LIBSQL_TOKEN"),
    syncUrl: Config.string("DATA_LIBSQL_SYNC_URL"),
    syncInterval: Config.integer("DATA_LIBSQL_SYNC_SECONDS").pipe(Config.withDefault(30)),
  })

const _d1 = (db: D1Database): Layer.Layer<D1Client.D1Client | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  D1Client.layer({ db })
```

## [04]-[SNAPSHOT_IO]

- Owner: `Sqlite.bytes(io)` — ONE byte-operation entry whose modality is the `Sqlite.Io` case value: `Snapshot` (whole-database export content-addressed into the object plane across either server profile), `Backup` (node-only non-blocking online backup with page-progress metadata), `Seed`/`Dump` (wasm import/export), and `Extend` (runtime extension load across either server profile); `_server` resolves the structurally common byte-capable client from the environment without making the caller select the profile.
- Packages: `@effect/sql-sqlite-node` (`client.export`, `client.backup`, `client.loadExtension`, `BackupMetadata`); `@effect/sql-sqlite-bun` (`client.export`, `client.loadExtension`); `@effect/sql-sqlite-wasm` (`client.import`, `client.export`, `SqliteClient.withTransferables`); the object plane's put entry consumes the exported bytes at the composition seam.
- Entry: server snapshots feed the content-addressed object plane — the key IS the bytes, so a re-put is idempotent; the browser seeds a first-run database from a server-minted snapshot fetched by content key, and the memory profile persists by dump-then-seed through its own storage row.
- Receipt: `backup` yields `BackupMetadata` — total and remaining pages — so a live backup is observable progress, not a blocking export; `dump` yields the raw bytes because the browser cannot mint into the object plane directly.
- Growth: a new byte operation is one `Sqlite.Io` case plus its `$match` arm — every consumer breaks loudly until the arm exists; a new seed source (server snapshot, replay, fixture) is a caller decision — the surface takes bytes and nothing else; the libSQL row never composes these cases because replica sync IS its durability transport.
- Law: `export` snapshots block the writer for the copy and suit specs and small files; `backup` is the production posture on the node profile — page-incremental, non-blocking, poll-observable.
- Law: browser seed bytes transfer when their backing is an `ArrayBuffer`; `SharedArrayBuffer` cannot enter a transfer list and rides shared memory unchanged. The wasm client's export transport owns its response crossing, so this page never invents an unsupported return-transfer API.
- Law: seed-then-verify — after `import`, the lane's ensure relations probe exactly like server startup, so a truncated or foreign blob fails closed at seed time, never at first query.
- Law: `loadExtension` is the degradation table's `loadExtension` verdict realized — its typed client failure aborts the admission effect, and the composition runs that effect before constructing the capability Layer whose registry probe grants the module.

```typescript
import { Data, Option } from "effect"

class SqliteFault extends Data.TaggedError("SqliteFault")<{
  readonly reason: "profile"
  readonly operation: "snapshot" | "extend"
}> {}

type _ServerClient = Pick<NodeSqlite.SqliteClient.SqliteClient, "export" | "loadExtension">

const _server = (operation: SqliteFault["operation"]): Effect.Effect<_ServerClient, SqliteFault> =>
  Effect.all([
    Effect.serviceOption(NodeSqlite.SqliteClient.SqliteClient).pipe(
      Effect.map(Option.map((client): _ServerClient => client)),
    ),
    Effect.serviceOption(BunSqlite.SqliteClient.SqliteClient).pipe(
      Effect.map(Option.map((client): _ServerClient => client)),
    ),
  ]).pipe(
    Effect.flatMap(([node, bun]) =>
      Option.match(Option.orElse(node, () => bun), {
        onNone: () => Effect.fail(new SqliteFault({ reason: "profile", operation })),
        onSome: Effect.succeed,
      })),
  )

type SqliteIo = Data.TaggedEnum<{
  Snapshot: {}
  Backup: { readonly destination: string }
  Extend: { readonly path: string }
  Seed: { readonly bytes: Uint8Array }
  Dump: {}
}>

const _Io = Data.taggedEnum<SqliteIo>()

const _bytes = (io: SqliteIo) =>
  _Io.$match(io, {
    Snapshot: () => Effect.flatMap(_server("snapshot"), (client) => client.export),
    Backup: ({ destination }) => Effect.flatMap(NodeSqlite.SqliteClient.SqliteClient, (client) => client.backup(destination)),
    Extend: ({ path }) => Effect.flatMap(_server("extend"), (client) => client.loadExtension(path)),
    Seed: ({ bytes }) =>
      Effect.flatMap(WasmSqlite.SqliteClient.SqliteClient, (client) =>
        WasmSqlite.SqliteClient.withTransferables(bytes.buffer instanceof ArrayBuffer ? [bytes.buffer] : [])(client.import(bytes))),
    Dump: () => Effect.flatMap(WasmSqlite.SqliteClient.SqliteClient, (client) => client.export),
  })

const Sqlite = {
  degrades: _degrades,
  filename: _filename,
  node: _node,
  bun: _bun,
  opfs: _opfs,
  memory: _memory,
  worker: _worker,
  libsql: _libsql,
  d1: _d1,
  Io: _Io,
  bytes: _bytes,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Sqlite, SqliteFault }
```
