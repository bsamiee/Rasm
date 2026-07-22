# [DATA_SQLITE]

ONE sqlite lane runs journal, projection, tenancy, and capability contracts across node, bun, wasm-OPFS, libSQL, and D1. A TOTAL degradation table keys the spine's `Pg.Grant` union in both directions, so new or foreign grants fail its declaration. Runtime-subpath Layer rows select drivers; neutral statements use `sql.onDialectOrElse`. Server profiles use WAL and one writer, OPFS narrows to one tab, and edge profiles serialize at the primary.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                              |
| :-----: | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `DEGRADATION_TABLE` | the grant-total capability-to-fallback matrix — the lane's whole difference as data |
|  [02]   | `PROFILE_ROWS`      | Layer constructors and their runtime coordinates                                    |
|  [03]   | `SNAPSHOT_IO`       | whole-database export/backup/seed, zero-copy transfer, extension load               |
|  [04]   | `PROFILE_HARVEST`   | the per-profile query-evidence arm — availability rows, timed EXPLAIN, page stats   |

## [02]-[DEGRADATION_TABLE]

- Owner: the `_degrades` anchor — one row per `Pg.Grant` member, carrying a verdict per profile column; the derived `Sqlite.Fallback` union every consumer dispatches on, and the two-directional guard pair binding the key space to the spine's grant union.
- Packages: none — the table is pure vocabulary over `lane/postgres.md`'s grant keys, reached through the one `Pg` import.
- Growth: a new spine grant breaks `_Rows` at this declaration the day it lands in the matrix — completeness is a compile fact, never a census; a sixth profile is one more column across every row.
- Law: the table is consumed, never consulted ad hoc — the projection wake reads `channel` through the optional `PgClient` service, the append lock arm reads `advisory` through `onDialectOrElse`, tenancy reads `rls` by never constructing `Tenancy` scopes on this lane; the rows document dispatch that already exists in the statements.
- Law: `none` is a lawful verdict — analytics, geo, h3, timeseries, graphql, audit, statements, and asyncIo have no substitute, so consumer gates refuse them; guards make an absent row impossible. `statements` is `none` on every profile because the `sqlite3_stmt_status` C counters are unreachable through every admitted driver — the harvest table already prices the same refusal as `stmtStatus`, and the explicit harness-timed diagnosis arm is evidence, never a cumulative-statistics substitute.
- Law: the lane is capability-different — `bm25` degrades to FTS5, `vector` to a server extension or libSQL built-in, `virtualGenerated` and `skipScan` are engine-native, and every profile includes FTS5 and JSONB.
- Law: evidence grants degrade to composed statements — `returningOldNew` pairs RETURNING with a transactional pre-image, `conflictClaim` uses an explicit upsert marker, `merge` uses upsert arms, and `temporal` uses a single-writer overlap check.
- Law: tenancy verdicts are residency verdicts — file-per-app on the server profiles, origin scope in the browser, database-per-tenant on both edge rows where cheap databases are the platform model; the RLS policy family never runs here.
- Law: the D1 column refuses the interactive transaction — atomic publish is batch-shaped or routed to pg; the refusal is a row, not a code fork.

```typescript signature
import { Pg } from "./postgres.ts" // Grant keys stay type-plane reads; profile receipt is the one consumed value.

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
  statements: { server: "none", wasm: "none", libsql: "none", d1: "none" },
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

- Owner: the Layer constructors — `Sqlite.node(app)` and `Sqlite.bun(app)` on `./server`, `Sqlite.opfs(worker)` and `Sqlite.memory` on `./wasm`, `Sqlite.libsql` and `Sqlite.d1(db)` at edge roots — and worker entry `Sqlite.worker`.
- Packages: `@effect/sql-sqlite-node` (`SqliteClient.layerConfig`, `prepareCacheSize`, `disableWAL`, `readonly`); `@effect/sql-sqlite-bun` (`SqliteClient.layerConfig`, `create`, `readwrite`); `@effect/sql-sqlite-wasm` (`SqliteClient.layer`, `SqliteClient.layerMemory`, `OpfsWorker.run`, `installReactivityHooks`); `@effect/sql-libsql` (`LibsqlClient.layerConfig`); `@effect/sql-d1` (`D1Client.layer`); `effect` (`Config`, `Layer`, `Scope`).
- Entry: the app root selects the profile row per the host runtime — all provide `SqlClient`, and every data surface above them is unchanged; the OPFS worker entry module runs `Sqlite.worker({ port, dbName })` and nothing else.
- Growth: a profile-tuning knob is a `Config` field on its row; another runtime adds a constructor and degradation column.
- Law: the filename derives from the scope — file-per-app IS the server tenancy: `_filename(app)` keys the file, `":memory:"` serves specs, and the naming matches the pg spine so `onDialect` statements agree about column spellings; the OPFS `dbName` and the libSQL replica path key the same way.
- Law: WAL stays on for journal profiles; `disableWAL` with `readonly` is the read-replica posture; node, bun, and D1 cache through their Layer facts, never statement facts.
- Law: OPFS access is worker-only by platform contract — the durable browser constructor takes the worker effect, so a main-thread open is unspellable; `installReactivityHooks: true` restores `sql.reactive` in-tab, the same key vocabulary as every lane.
- Law: the libSQL row is contract-level compatible, never byte-level — the replica engine is not the C library; its credentials and sync cadence ride `Config.redacted` and `Config` duration facts.
- Law: the D1 row adopts the platform binding as a value — `env.DB` arrives at the Workers composition root; replication sessions and PITR are platform facts recorded as degradation semantics, never re-modeled.

```typescript signature
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
- Growth: a byte operation is one `Sqlite.Io` case and `$match` arm; a seed source is a caller decision over bytes; libSQL excludes these cases because replica sync is its durability transport.
- Law: `export` snapshots block the writer for the copy and suit specs and small files; `backup` is the production posture on the node profile — page-incremental, non-blocking, poll-observable.
- Law: browser seed bytes transfer when their backing is an `ArrayBuffer`; `SharedArrayBuffer` cannot enter a transfer list and rides shared memory unchanged. Wasm client export transport owns its response crossing, so this page never invents an unsupported return-transfer API.
- Law: seed-then-verify — after `import`, the lane's ensure relations probe exactly like server startup, so a truncated or foreign blob fails closed at seed time, never at first query.
- Law: `loadExtension` is the degradation table's `loadExtension` verdict realized — its typed client failure aborts the admission effect, and the composition runs that effect before constructing the capability Layer whose registry probe grants the module.

```typescript signature
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

```

## [05]-[PROFILE_HARVEST]

- Owner: the lane's `Pg.Profile` arm — `_harvest` prices measures, `_profiled` folds wall span, plan structure, and page counters, `_dbstat` probes the virtual table once at arm construction (a `SELECT 1 … LIMIT 1` presence read, never a per-diagnosis aggregate scan), and `Sqlite` assembles the export.
- Packages: `@effect/sql` (`SqlClient`, `SqlSchema`, `Statement` — the profiled statement is a `Fragment` value); `effect` (`Duration`, `Effect`, `Schema`); `./postgres.ts` (`Pg.Profile` — the shared receipt schema, the lane's one value read beside the type-only grant vocabulary).
- Entry: the maintenance composition constructs the arm once per layer — `Sqlite.profile.of(sql, engine)` probes `dbstat` a single time and yields the diagnosis closure — and each explicit diagnosis call runs that closure with `(statement, label)` on any profile; the composition projects `wallMillis` onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine` exactly as the pg and DuckDB arms do — receipts stay the truth, the instrument the lossy channel.
- Receipt: `Pg.Profile` with `engine` selected by the live profile (`sqliteServer` | `sqliteWasm` | `libsql` | `d1`), operators from the `EXPLAIN QUERY PLAN` rows carrying `Option.none()` timing — the engine exposes plan structure without per-operator clocks, and an absent measure is omission, never a zero — and `counters` holding `pageCount`, `freelistCount`, and the `dbstat` aggregates where the probe granted them.
- Growth: an evidence source is one `_harvest` row and harvest line; another profile adds an availability column that the guard requires.
- Law: availability is priced as row data — `explainPlan` and the page pragmas are `builtIn` everywhere, `dbstat` is `probe` on the server and wasm profiles (the virtual table is a compile-time engine fact `_dbstat` answers per deployment, never a static claim) and `none` on the edge rows, and `stmtStatus` is `none` on every profile because the `sqlite3_stmt_status` C counters are unreachable through every admitted driver — the refusal is a recorded verdict, and no arm fabricates a zero where a counter is absent.
- Law: wall span is harness-measured — the engine exposes no per-query clock through any admitted driver, so `_profiled` times the statement's own run with `Effect.timed` and the span covers exactly the profiled execution; the diagnosis therefore EXECUTES the statement, scoping the arm to explicit calls like the pg EXPLAIN arm.
- Law: the harvest never re-parses driver rows by hand — `EXPLAIN QUERY PLAN` rows, `pragma_page_count()`/`pragma_freelist_count()` reads, and the `dbstat` aggregate all decode through `SqlSchema`, so a malformed cell is a `ParseError` on the admission rail.

```typescript signature
import { Pg } from "./postgres.ts"
import { SqlClient, SqlSchema, type Statement } from "@effect/sql"
import { Array, Duration, Schema } from "effect"

const _harvest = {
  explainPlan: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "builtIn" },
  pageStats: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "none" },
  dbstat: { server: "probe", wasm: "probe", libsql: "none", d1: "none" },
  stmtStatus: { server: "none", wasm: "none", libsql: "none", d1: "none" },
} as const

declare namespace Sqlite {
  type Evidence = keyof typeof _harvest
  type Availability = (typeof _harvest)[Evidence][keyof (typeof _harvest)[Evidence]]
  type ProfileEngine = Extract<Pg.ProfileEngine, "sqliteServer" | "sqliteWasm" | "libsql" | "d1">
  type _Harvest<T extends Record<Evidence, Record<Sqlite.Profile, string>> = typeof _harvest> = T
}

const _PlanRow = Schema.Struct({ id: Schema.Number, parent: Schema.Number, detail: Schema.String })

const _PageRow = Schema.Struct({ pages: Schema.Number, freelist: Schema.Number })

const _DbstatRow = Schema.Struct({ btrees: Schema.Number, unusedBytes: Schema.Number })

// Availability probe, not a scan: `SELECT 1 … LIMIT 1` answers whether the dbstat module compiled in — the
// prior `count(*)` aggregate walked every btree page on each diagnosis. It runs ONCE, at arm construction.
const _dbstat = (sql: SqlClient.SqlClient): Effect.Effect<boolean> =>
  Effect.match(sql`SELECT 1 AS probed FROM dbstat LIMIT 1`, { onFailure: () => false, onSuccess: () => true })

// Construction-effect: the arm probes dbstat exactly once and closes over the verdict — a compile-time engine
// fact never re-probes per diagnosis, and every diagnosis reuses the cached availability.
const _profiled = (sql: SqlClient.SqlClient, engine: Sqlite.ProfileEngine) =>
  Effect.map(_dbstat(sql), (granted) => (statement: Statement.Fragment, label: string) =>
    Effect.gen(function* () {
      const plan = yield* SqlSchema.findAll({
        Request: Schema.Void,
        Result: _PlanRow,
        execute: () => sql`EXPLAIN QUERY PLAN ${statement}`,
      })(void 0)
      const [span, rows] = yield* Effect.timed(sql`${statement}`)
      const pages = yield* SqlSchema.single({
        Request: Schema.Void,
        Result: _PageRow,
        execute: () => sql`SELECT pragma_page_count() AS pages, pragma_freelist_count() AS freelist`,
      })(void 0)
      const space = yield* (granted
        ? Effect.map(
            SqlSchema.single({
              Request: Schema.Void,
              Result: _DbstatRow,
              execute: () => sql`SELECT count(*) AS btrees, coalesce(sum(unused), 0) AS unusedBytes FROM dbstat`,
            })(void 0),
            Option.some,
          )
        : Effect.succeed(Option.none<typeof _DbstatRow.Type>()))
      return new Pg.Profile({
        engine,
        statement: label,
        wallMillis: Duration.toMillis(span),
        rows: rows.length,
        operators: Array.map(plan, (step) => ({ name: step.detail, millis: Option.none(), rows: Option.none() })),
        counters: {
          pageCount: pages.pages,
          freelistCount: pages.freelist,
          ...Option.match(space, {
            onNone: () => ({}), // dbstat refused: the counters stay absent, never zero-forged
            onSome: (held) => ({ dbstatBtrees: held.btrees, dbstatUnusedBytes: held.unusedBytes }),
          }),
        },
        window: Option.none(),
      })
    }))

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
  profile: { harvest: _harvest, dbstat: _dbstat, of: _profiled },
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Sqlite, SqliteFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
