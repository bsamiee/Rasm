# [DATA_SQLITE]

ONE sqlite lane runs journal, projection, tenancy, and capability contracts across node, bun, wasm-OPFS, libSQL, and D1. Degradation rows key the spine's `Pg.Grant` union in both directions, so new or foreign grants fail at that declaration. Runtime-subpath Layer rows select drivers; neutral statements use `sql.onDialectOrElse`. Server profiles use WAL and one writer, OPFS narrows to one tab, and edge profiles serialize at the primary.

## [01]-[INDEX]

- [02]-[DEGRADATION_TABLE]: the grant-total capability-to-fallback matrix — the lane's whole difference as data.
- [03]-[PROFILE_ROWS]: Layer constructors and their runtime coordinates.
- [04]-[PGLITE_PROFILE]: generation-qualified embedded PostgreSQL, neutral SQL Layer, and dump custody.
- [05]-[SNAPSHOT_IO]: whole-database export/backup/seed, zero-copy transfer, extension load.
- [06]-[PROFILE_HARVEST]: the per-profile query-evidence arm — availability rows, timed EXPLAIN, page stats.

## [02]-[DEGRADATION_TABLE]

- Owner: the `_degrades` anchor — one row per `Pg.Grant` member, carrying a verdict per embedded-profile column; the derived `Sqlite.Fallback` union every consumer dispatches on, and the two-directional guard pair binding the key space to the spine's grant union.
- Packages: none — the table is pure vocabulary over `lane/postgres.md`'s grant keys, reached through the one `Pg` import.
- Growth: a new spine grant breaks `_Rows` at this declaration the day it lands in the matrix — completeness is a compile fact, never a census; another embedded profile is one more column across every row.
- Law: the pglite column prices an embedded POSTGRESQL engine, so a spine grant it inherits reads `builtIn` and single-connection residency answers the concurrency primitives. `extensionOption` names a `PGlite.create` option row the pin's OWN contrib set can fill — presence at composition stays the capability probe's answer, never a column's claim — so a grant whose provider that set does not ship reads `none` beside every other profile refusing it, and one whose fallback is app-side reads that fallback: an `extensionOption` standing for an extension nobody can install advertises a knob a consumer gate then waits on forever.
- Law: the embedded engine answers `uuidv7`, `merge`, `returningOldNew`, `virtualGenerated`, and `temporal` natively, so each reads `builtIn`.
- Law: `skipScan` rides the embedded planner, so the column reads `plannerOwned` exactly as every server profile does.
- Law: `asyncIo` reads `none` on every column — the embedded build launches single-user under synchronous IO with no worker process.
- Law: `Sqlite.Lane` narrows the column set to the four profiles the sqlite statements serve; pglite runs neither `EXPLAIN QUERY PLAN` nor the page pragmas, so its diagnosis rides the PostgreSQL EXPLAIN arm and its harvest row prices every sqlite evidence source absent.
- Law: the table is consumed, never consulted ad hoc — the projection wake reads `channel` through the optional `PgClient` service, the append lock arm reads `advisory` through `onDialectOrElse`, tenancy reads `rls` by never constructing `Tenancy` scopes on this lane; the rows document dispatch that already exists in the statements.
- Law: `none` is a lawful verdict — analytics, geo, h3, timeseries, graphql, audit, parquet, and asyncIo have no substitute on any embedded profile, so consumer gates refuse them; guards make an absent row impossible. `statements` is `none` on every sqlite profile because the `sqlite3_stmt_status` C counters are unreachable through every admitted driver — the harvest table already prices the same refusal as `stmtStatus`, and the explicit harness-timed diagnosis arm is evidence, never a cumulative-statistics substitute.
- Law: the lane is capability-different — `bm25` degrades to FTS5, `vector` to a server extension or libSQL built-in, `virtualGenerated` and `skipScan` are engine-native, and every profile includes FTS5 and JSONB.
- Law: evidence grants degrade to composed statements — `returningOldNew` pairs RETURNING with a transactional pre-image, `conflictClaim` uses an explicit upsert marker, `merge` uses upsert arms, and `temporal` uses a single-writer overlap check.
- Law: tenancy verdicts are residency verdicts — file-per-app on the server profiles, origin scope in the browser, database-per-tenant on both edge rows where cheap databases are the platform model; the RLS policy family never runs here.
- Law: the D1 column refuses the interactive transaction — atomic publish is batch-shaped or routed to pg; the refusal is a row, not a code fork.

```typescript signature
import { Pg } from "./postgres.ts" // Grant keys stay type-plane reads; profile receipt is the one consumed value.

const _degrades = {
  rls: { server: "filePerApp", wasm: "originScope", libsql: "databasePerTenant", d1: "databasePerTenant", pglite: "builtIn" },
  channel: { server: "poll", wasm: "reactivityHooks", libsql: "syncPull", d1: "none", pglite: "builtIn" },
  advisory: { server: "singleWriter", wasm: "singleTab", libsql: "primarySerialized", d1: "primarySerialized", pglite: "singleWriter" },
  skipLocked: { server: "singleWriter", wasm: "singleTab", libsql: "primarySerialized", d1: "primarySerialized", pglite: "singleWriter" },
  conflictClaim: { server: "conflictChanges", wasm: "conflictChanges", libsql: "conflictChanges", d1: "conflictChanges", pglite: "builtIn" },
  merge: { server: "upsert", wasm: "upsert", libsql: "upsert", d1: "upsert", pglite: "builtIn" },
  copy: { server: "chunkedInsert", wasm: "chunkedInsert", libsql: "chunkedInsert", d1: "batchInsert", pglite: "builtIn" },
  uuidv7: { server: "appMint", wasm: "appMint", libsql: "appMint", d1: "appMint", pglite: "builtIn" },
  returningOldNew: { server: "preRead", wasm: "preRead", libsql: "preRead", d1: "preRead", pglite: "builtIn" },
  virtualGenerated: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "builtIn", pglite: "builtIn" },
  temporal: { server: "appCheck", wasm: "appCheck", libsql: "appCheck", d1: "appCheck", pglite: "builtIn" },
  skipScan: { server: "plannerOwned", wasm: "plannerOwned", libsql: "plannerOwned", d1: "plannerOwned", pglite: "plannerOwned" },
  asyncIo: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  ivm: { server: "asyncLane", wasm: "inTabFold", libsql: "appSide", d1: "none", pglite: "extensionOption" },
  cron: { server: "hostSchedule", wasm: "none", libsql: "appSide", d1: "platformCron", pglite: "none" },
  partition: { server: "snapshotTruncate", wasm: "snapshotTruncate", libsql: "appSide", d1: "none", pglite: "builtIn" },
  incremental: { server: "checkpointLane", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  vector: { server: "loadExtension", wasm: "none", libsql: "builtIn", d1: "none", pglite: "none" },
  vchord: { server: "loadExtension", wasm: "none", libsql: "builtIn", d1: "none", pglite: "none" },
  bm25: { server: "fts5", wasm: "fts5", libsql: "fts5", d1: "fts5", pglite: "none" },
  trigram: { server: "fts5", wasm: "fts5", libsql: "fts5", d1: "fts5", pglite: "extensionOption" },
  phonetic: { server: "loadExtension", wasm: "none", libsql: "none", d1: "none", pglite: "extensionOption" },
  fuzzy: { server: "loadExtension", wasm: "none", libsql: "none", d1: "none", pglite: "extensionOption" },
  jsonschema: { server: "schemaDecode", wasm: "schemaDecode", libsql: "schemaDecode", d1: "schemaDecode", pglite: "schemaDecode" },
  statements: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "extensionOption" },
  parquet: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  analytics: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  graphql: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  audit: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  geo: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  h3: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
  timeseries: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
} as const

declare namespace Sqlite {
  type Degraded = keyof typeof _degrades
  type Profile = keyof (typeof _degrades)[Degraded]
  type Lane = Exclude<Profile, "pglite">
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

const _d1 = (db: D1Client.D1ClientConfig["db"]): Layer.Layer<D1Client.D1Client | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  D1Client.layer({ db })
```

## [04]-[PGLITE_PROFILE]

- Owner: `PgliteRuntime.layer` creates, hydrates, observes, and admits one unpublished PGLite generation.
- Law: `PGlite.create` and `close` form one scoped acquire/release; one semaphore owns the single-user connection.
- Law: transaction acquisition holds the permit through begin, body, commit or rollback; statement execution cannot interleave.
- Law: `PgClient.makeCompiler` compiles neutral fragments; PGLite receives SQL and bound parameters through `query`.
- Law: source streaming fails typed because `PGliteInterface` exposes materialized results only.
- Law: `PgliteSeed` closes empty, logical SQL, and physical PGDATA inputs; only creation accepts physical state.
- Law: `snapshot` discriminates physical `dumpDataDir` and logical `pgDump`; both exclude statements for their complete run.
- Packages: PGLite, PGLite tools, Effect, `@effect/sql`, `@effect/sql-pg`, and `Reactivity`.
- Growth: storage residency is one coordinate policy row; an extension is one typed `PGlite.create` option row.
- Boundary: coordinates derive from admitted generation policy; caller options cannot set recovery, durability, or server configuration.

```typescript signature
import { PGlite, type PGliteInterface } from "@electric-sql/pglite"
import { pgDump } from "@electric-sql/pglite-tools"
import { Reactivity } from "@effect/experimental"
import { SqlClient } from "@effect/sql"
import type { Connection } from "@effect/sql/SqlConnection"
import { SqlError } from "@effect/sql/SqlError"
import { PgClient } from "@effect/sql-pg"
import { Context, Effect, Layer, Stream } from "effect"
import { Backend, type BackendFault } from "./capability.ts"

declare namespace PgliteRuntime {
  type Coordinate = (contract: Backend.Contract) => string
  type Snapshot = "physical" | "logical"
  type Seed =
    | { readonly _tag: "empty" }
    | { readonly _tag: "logical"; readonly script: string }
    | { readonly _tag: "physical"; readonly archive: Blob | File }
  type Observe = (
    sql: SqlClient.SqlClient,
    expected: Backend.Contract,
  ) => Effect.Effect<Backend.Observation, BackendFault | SqlError>
  type Service = {
    readonly generation: Backend.Generation
    readonly snapshot: (kind: Snapshot) => Effect.Effect<Blob | File, SqlError>
  }
}

const _sqlFault = (message: string, cause: unknown): SqlError =>
  new SqlError({ message, cause })

const _pgliteQuery = (
  pg: PGliteInterface,
  statement: string,
  parameters: ReadonlyArray<unknown>,
): Effect.Effect<ReadonlyArray<Record<string, unknown>>, SqlError> =>
  Effect.tryPromise({
    try: () => pg.query<Record<string, unknown>>(statement, [...parameters]),
    catch: (cause) => _sqlFault("PGLite query failed", cause),
  }).pipe(Effect.map((result) => result.rows))

const _pgliteConnection = (
  pg: PGliteInterface,
  permit: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>,
): Connection => ({
  execute: (statement, parameters, transformRows) =>
    permit(_pgliteQuery(pg, statement, parameters)).pipe(
      Effect.map((rows) => transformRows?.(rows) ?? rows),
    ),
  executeRaw: (statement, parameters) =>
    permit(Effect.tryPromise({
      try: () => pg.query(statement, [...parameters]),
      catch: (cause) => _sqlFault("PGLite raw query failed", cause),
    })),
  executeStream: () =>
    Stream.fail(_sqlFault("PGLite source streaming unavailable", "materialized-only")),
  executeValues: (statement, parameters) =>
    permit(_pgliteQuery(pg, statement, parameters)).pipe(
      Effect.map((rows) => rows.map((row) => Object.values(row))),
    ),
  executeUnprepared: (statement, parameters, transformRows) =>
    permit(_pgliteQuery(pg, statement, parameters)).pipe(
      Effect.map((rows) => transformRows?.(rows) ?? rows),
    ),
})

class PgliteRuntime extends Context.Tag("data/PgliteRuntime")<
  PgliteRuntime,
  PgliteRuntime.Service
>() {
  static layer(
    projection: Backend.Projection,
    coordinate: PgliteRuntime.Coordinate,
    seed: PgliteRuntime.Seed,
    observe: PgliteRuntime.Observe,
  ): Layer.Layer<
    PgliteRuntime | SqlClient.SqlClient,
    BackendFault | SqlError,
    Reactivity.Reactivity
  > =>
    Layer.scopedContext(Effect.gen(function* () {
      const gate = yield* Effect.makeSemaphore(1)
      const pg = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: () => PGlite.create(
            coordinate(projection.contract),
            seed._tag === "physical" ? { loadDataDir: seed.archive } : {},
          ),
          catch: (cause) => _sqlFault("PGLite open failed", cause),
        }),
        (handle) => Effect.promise(() => handle.close()).pipe(Effect.orDie),
      )
      yield* seed._tag === "logical"
        ? Effect.tryPromise({
            try: () => pg.exec(seed.script),
            catch: (cause) => _sqlFault("PGLite hydrate failed", cause),
          })
        : Effect.void
      const connection = _pgliteConnection(pg, gate.withPermits(1))
      const transaction = _pgliteConnection(pg, (effect) => effect)
      const sql = yield* SqlClient.make({
        acquirer: Effect.succeed(connection),
        transactionAcquirer: Effect.acquireRelease(
          gate.take(1),
          () => gate.release(1),
        ).pipe(Effect.as(transaction)),
        compiler: PgClient.makeCompiler(),
        spanAttributes: [
          ["db.system.name", "postgresql"],
          ["db.namespace", projection.contract.wire.contract],
        ],
        beginTransaction: "BEGIN",
        commit: "COMMIT",
        rollback: "ROLLBACK",
        savepoint: (name) => `SAVEPOINT "${name}"`,
        rollbackSavepoint: (name) => `ROLLBACK TO SAVEPOINT "${name}"`,
      })
      const generation = yield* Effect.flatMap(
        observe(sql, projection.contract),
        (observed) => Backend.admit(projection.contract, observed),
      )
      const runtime = {
        generation,
        snapshot: (kind: PgliteRuntime.Snapshot): Effect.Effect<Blob | File, SqlError> =>
          gate.withPermits(1)(
            Effect.tryPromise({
              try: () => kind === "physical" ? pg.dumpDataDir("gzip") : pgDump({ pg }),
              catch: (cause) => _sqlFault("PGLite snapshot failed", cause),
            }),
          ),
      } satisfies PgliteRuntime.Service
      return Context.make(PgliteRuntime, runtime).pipe(
        Context.add(SqlClient.SqlClient, sql),
      )
    }))
}

export { PgliteRuntime }
```

## [05]-[SNAPSHOT_IO]

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

## [06]-[PROFILE_HARVEST]

- Owner: the lane's `Pg.Profile` arm — `_harvest` prices measures and GATES every statement the arm issues, `_profiled` folds wall span, plan structure, and page counters, `_dbstat` probes the virtual table once at arm construction where the row prices it (a `SELECT 1 … LIMIT 1` presence read, never a per-diagnosis aggregate scan), `_ENGINE` binds each profile to the engine its receipt stamps, and `Sqlite` assembles the export.
- Packages: `@effect/sql` (`SqlClient`, `SqlSchema`, `Statement` — the profiled statement is a `Fragment` value); `effect` (`Duration`, `Effect`, `Schema`); `./postgres.ts` (`Pg.Profile` — the shared receipt schema, the lane's one value read beside the type-only grant vocabulary).
- Entry: the maintenance composition constructs the arm once per layer — `Sqlite.profile.of(sql, profile)` reads that profile's harvest row, probes `dbstat` only where the row prices it, and yields the diagnosis closure — and each explicit diagnosis call runs that closure with `(statement, label)` on any sqlite profile; the composition projects `wallMillis` onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine` exactly as the pg and DuckDB arms do — receipts stay the truth, the instrument the lossy channel.
- Receipt: `Pg.Profile` with `engine` selected by the live profile (`sqliteServer` | `sqliteWasm` | `libsql` | `d1`), operators from the `EXPLAIN QUERY PLAN` rows carrying `Option.none()` timing — the engine exposes plan structure without per-operator clocks, and an absent measure is omission, never a zero — and `counters` holding `pageCount`, `freelistCount`, and the `dbstat` aggregates where the probe granted them.
- Growth: an evidence source is one `_harvest` row and harvest line; another profile adds an availability column that the guard requires.
- Law: availability is priced as row data and the arm READS that price — every evidence statement gates on the active profile's `_harvest` column, so a profile the table marks without page pragmas issues none and omits the counter; a probe running past its own row erases the table's reason to exist and fires SQL at a surface the deployment does not carry.
- Law: `dbstat` is `probe` on the server and wasm profiles — the virtual table is a compile-time engine fact `_dbstat` answers per deployment, never a static claim — and `stmtStatus` is `none` on every profile because the `sqlite3_stmt_status` C counters are unreachable through every admitted driver; a recorded refusal omits its counter and no arm fabricates a zero.
- Law: wall span is harness-measured — the engine exposes no per-query clock through any admitted driver, so `_profiled` times the statement's own run with `Effect.timed` and the span covers exactly the profiled execution; the diagnosis therefore EXECUTES the statement, scoping the arm to explicit calls like the pg EXPLAIN arm.
- Law: the harvest never re-parses driver rows by hand — `EXPLAIN QUERY PLAN` rows, `pragma_page_count()`/`pragma_freelist_count()` reads, and the `dbstat` aggregate all decode through `SqlSchema`, so a malformed cell is a `ParseError` on the admission rail.

```typescript signature
import { Pg } from "./postgres.ts"
import { SqlClient, SqlSchema, type Statement } from "@effect/sql"
import { Array, Duration, Schema } from "effect"

const _harvest = {
  explainPlan: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "builtIn", pglite: "none" },
  pageStats: { server: "builtIn", wasm: "builtIn", libsql: "builtIn", d1: "none", pglite: "none" },
  dbstat: { server: "probe", wasm: "probe", libsql: "none", d1: "none", pglite: "none" },
  stmtStatus: { server: "none", wasm: "none", libsql: "none", d1: "none", pglite: "none" },
} as const

// `_ENGINE` binds each profile column to the receipt engine it stamps, so the harvest gate and the receipt
// read ONE key and no call site carries both spellings.
const _ENGINE = {
  server: "sqliteServer",
  wasm: "sqliteWasm",
  libsql: "libsql",
  d1: "d1",
  pglite: "pglite",
} as const satisfies Record<Sqlite.Profile, Pg.ProfileEngine>

declare namespace Sqlite {
  type Evidence = keyof typeof _harvest
  type Availability = (typeof _harvest)[Evidence][keyof (typeof _harvest)[Evidence]]
  type ProfileEngine = (typeof _ENGINE)[Sqlite.Lane]
  type _Harvest<T extends Record<Evidence, Record<Sqlite.Profile, string>> = typeof _harvest> = T
}

const _PlanRow = Schema.Struct({ id: Schema.Number, parent: Schema.Number, detail: Schema.String })

const _PageRow = Schema.Struct({ pages: Schema.Number, freelist: Schema.Number })

const _DbstatRow = Schema.Struct({ btrees: Schema.Number, unusedBytes: Schema.Number })

// Availability probe, not a scan: `SELECT 1 … LIMIT 1` answers whether the dbstat module compiled in — the
// prior `count(*)` aggregate walked every btree page on each diagnosis. It runs ONCE, at arm construction.
const _dbstat = (sql: SqlClient.SqlClient): Effect.Effect<boolean> =>
  Effect.match(sql`SELECT 1 AS probed FROM dbstat LIMIT 1`, { onFailure: () => false, onSuccess: () => true })

// Construction-effect: the arm reads its profile's `_harvest` row ONCE, probes dbstat only where that row
// prices it `probe`, and closes over the verdict — so an edge profile pays no construction round trip, a
// compile-time engine fact never re-probes per diagnosis, and every diagnosis reuses the cached availability.
// Each evidence source below gates on the same row, so a profile whose column reads `none` issues no statement
// against a surface it does not carry and omits the counter rather than erroring or forging a zero.
const _profiled = (sql: SqlClient.SqlClient, profile: Sqlite.Lane) =>
  Effect.map(
    _harvest.dbstat[profile] === "probe" ? _dbstat(sql) : Effect.succeed(false),
    (granted) => (statement: Statement.Fragment, label: string) =>
      Effect.gen(function* () {
        const plan = yield* (_harvest.explainPlan[profile] === "builtIn"
          ? SqlSchema.findAll({
              Request: Schema.Void,
              Result: _PlanRow,
              execute: () => sql`EXPLAIN QUERY PLAN ${statement}`,
            })(void 0)
          : Effect.succeed<ReadonlyArray<typeof _PlanRow.Type>>([]))
        const [span, rows] = yield* Effect.timed(sql`${statement}`)
        const pages = yield* (_harvest.pageStats[profile] === "builtIn"
          ? Effect.map(
              SqlSchema.single({
                Request: Schema.Void,
                Result: _PageRow,
                execute: () => sql`SELECT pragma_page_count() AS pages, pragma_freelist_count() AS freelist`,
              })(void 0),
              Option.some,
            )
          : Effect.succeed(Option.none<typeof _PageRow.Type>()))
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
          engine: _ENGINE[profile],
          statement: label,
          wallMillis: Duration.toMillis(span),
          rows: rows.length,
          operators: Array.map(plan, (step) => ({ name: step.detail, millis: Option.none(), rows: Option.none() })),
          counters: {
            ...Option.match(pages, {
              onNone: () => ({}),
              onSome: (held) => ({ pageCount: held.pages, freelistCount: held.freelist }),
            }),
            ...Option.match(space, {
              onNone: () => ({}),
              onSome: (held) => ({ dbstatBtrees: held.btrees, dbstatUnusedBytes: held.unusedBytes }),
            }),
          },
          window: Option.none(),
        })
      }),
  )

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

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
