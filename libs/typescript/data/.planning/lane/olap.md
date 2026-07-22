# [DATA_OLAP]

Analytical throughput is distinct from transactional durability and owned by engine rows with scoped wraps. DuckDB serves node and browser acceleration, pgDuckDB keeps adjacent analytics in pg, ClickHouse enters only past its scale trigger, and Flight remains its ingress slice. Arrow is the ONE columnar wire; journal facts flow in and verdicts out, never back as authority. Boundary kernels lift promises, and tagged read cases dispatch ONE entry. Admitted evidence arms emit `Pg.Profile`; Convention instruments project gate pressure from receipt truth.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                              |
| :-----: | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ENGINE_ROWS` | the engine decision table — guarantee, storage, ceiling, trigger per engine         |
|  [02]   | `EMBEDDED`    | the scoped DuckDB wraps — node instance/session, wasm worker engine, the read entry |
|  [03]   | `LAKE_ROWS`   | the table-format and attachment rows — DuckLake, object-store Parquet, spine ATTACH |
|  [04]   | `CLICKHOUSE`  | the at-scale driver row and its ingestion surface                                   |
|  [05]   | `FLIGHT`      | the engine-blind Flight SQL wire row — scoped client, Arrow-native reads and egress |
|  [06]   | `PROFILE`     | the query-profile harvest arm and the probe-evidence escalation rows                |
|  [07]   | `ARROW_WIRE`  | the one columnar interchange — Table, IPC both directions, batch streaming, ingest  |

## [02]-[ENGINE_ROWS]

- Owner: the `_engines` anchor — one row per engine profile carrying its guarantee, storage posture, and scale ceiling; the escalation trigger is data on the row, so an admission argument reads the table instead of relitigating the lane.
- Packages: none — the rows are decision facts.
- Growth: a new engine candidate is one row measured against the same columns; a row whose trigger never fires in production is a prune candidate at the next census.
- Law: `duckdbNode` is the default — vectorized execution, out-of-core spill, zero-copy Parquet/CSV/JSON/Arrow, single-file ACID under a single writer; embedded in-process, so the ceiling is one node and one concurrent writer.
- Law: `duckdbWasm` is the client-side row — the browser queries remote Parquet by HTTP range through presigned grants, results land as Arrow, and compute leaves the service bill entirely; single-threaded by default, CORS-bound, an accelerator over server-minted data.
- Law: `pgDuckdb` is the analytics-in-OLTP row — the spine's `analytics` grant embeds the same engine inside live Postgres for workloads adjacent to transactional data; no second system, PG durability, bounded by the single PG host; the grant is `lane/postgres.md`'s row and this lane only names the boundary.
- Law: `clickhouse` is admitted ONLY past the trigger — concurrent high-throughput ingestion, multi-node scale, or high-cardinality real-time serving; below it the embedded rows own the workload, and admitting the cluster early is the named operational waste.

```typescript signature
const _engines = {
  duckdbNode: {
    guarantee: "vectorized single-node analytics, out-of-core spill",
    storage: "single-file ACID, single writer, WAL",
    ceiling: "one node, one concurrent writer",
    trigger: "default — services, CLI, maintenance folds",
  },
  duckdbWasm: {
    guarantee: "browser-resident analytics over range-read Parquet",
    storage: "OPFS-backed or in-memory",
    ceiling: "browser sandbox, single-threaded default",
    trigger: "client-side exploration over object-plane data",
  },
  pgDuckdb: {
    guarantee: "columnar execution inside live Postgres",
    storage: "PG WAL/MVCC",
    ceiling: "the single PG host",
    trigger: "analytics adjacent to transactional rows — the spine analytics grant",
  },
  clickhouse: {
    guarantee: "distributed columnar MergeTree, concurrent ingestion",
    storage: "replicated shards and replicas",
    ceiling: "multi-node",
    trigger: "concurrent high-throughput ingestion OR multi-node scale OR high-cardinality real-time serving",
  },
} as const

declare namespace Olap {
  type Engine = keyof typeof _engines
  type _Rows<T extends Record<Engine, {
    readonly guarantee: string
    readonly storage: string
    readonly ceiling: string
    readonly trigger: string
  }> = typeof _engines> = T
}
```

## [03]-[EMBEDDED]

- Owner: scoped `Olap.node` and `Olap.wasm` wraps with `Olap.read`, the ONE leased-session entry whose `Rows`, `Window`, and `Drain` cases select bounded, windowed, or streamed geometry under the owner governor.
- Packages: `@duckdb/node-api` (`DuckDBInstance.create`, `instance.closeSync`, `instance.connect`, `connection.disconnectSync`, `connection.runAndReadAll`, `connection.stream`, `connection.streamAndReadUntil`, `DuckDBResult.yieldRowObjects`, `quotedIdentifier`, `quotedString`); `@duckdb/duckdb-wasm` (`selectBundle`, `AsyncDuckDB`, `ConsoleLogger`, `db.instantiate`, `db.connect`, `conn.query`, `conn.send`, `db.registerFileURL`, `DuckDBDataProtocol`); `effect` (`Effect`, `Scope`, `Stream`, `Data`, `Schedule`, `Duration`).
- Entry: a service composes `Olap.node` once per database coordinate and leases sessions per analytical unit of work through `handle.lease`; the browser shell composes `Olap.wasm` over self-hosted bundles at boot and hands connections to the viewer's query surfaces.
- Receipt: node reads land as row-object projections; wasm queries land as Arrow Tables — the wire cluster's value, zero-copy into the viewer.
- Growth: an engine knob is an acquire-config field; a read geometry is one `Olap.Read` case with overload and dispatch arm; resilience is a `_GOVERNOR` override.
- Law: lifecycle is `acquireRelease` under `Scope` — instance, worker, and every connection release deterministically; an unscoped engine handle is unspellable because the constructors return scoped effects.
- Law: every promise lifts through the one boundary kernel `_try` into the reason-discriminated `OlapFault` — `acquire | query | extension | bundle | wire` routes recovery as a fold, never a `detail` string match; extension-load refusal is `extension`, bundle selection is `bundle`, and above the kernel the lane is rails end to end.
- Law: resilience is owner-internal — the bulkhead semaphore mints inside `Olap.node` (`Effect.makeSemaphore(_GOVERNOR.sessions)`), and `_governed` brackets every statement with the timeout budget; jittered bounded retry admits only `access: "read"` and `fault: "query"`, so an executing write, extension mutation, or profile harvest remains one-shot; the governor values are one policy row, and gate pressure projects onto the `Convention.instrument.olapWait` histogram and `Convention.instrument.olapRetried` counter tagged `Convention.rasm.olapEngine`.
- Law: the drain path never re-buffers — `DuckDBResult.yieldRowObjects()` yields one native data chunk at a time and does not retain prior chunks, while `Stream.acquireRelease` holds the session permit until downstream termination; a `DuckDBResultReader` is rejected here because its private chunk roster accumulates the full result even when `readUntil` advances incrementally. Acquisition retries before the first emission; an iteration fault fails the stream without replay because a partial-output retry duplicates rows.
- Law: extension admission is a statement — `INSTALL`/`LOAD` for `httpfs`, `ducklake`, `iceberg`, `delta`, `spatial`, `vss`, `fts` run through the `Rows` geometry; a load failure refuses the capability as a typed `extension` fault, never crashes the lane.
- Law: bundles self-host beside the app shell — `selectBundle` over owned artifact coordinates; a CDN bundle load is rejected by the deployment's content policy.
- Boundary: `_try` and `_wasm` are the marked promise kernels — typed `DuckDBValue` bind values cross without a cast, and the ambient `Worker` construction lives inside `_wasm`; its thrown missing-worker guard is caught by its own `tryPromise` and folds to the `bundle` reason.

```typescript signature
import { Data, Duration, Effect, Metric, MetricBoundaries, Schedule, type Scope, Stream } from "effect"
import { DuckDBInstance, quotedIdentifier, quotedString, type DuckDBConnection, type DuckDBValue } from "@duckdb/node-api"
import * as wasm from "@duckdb/duckdb-wasm"
import { Convention } from "@rasm/ts/core"

const _waited = Metric.tagged(
  Metric.histogram(
    Convention.instrument.olapWait.name,
    MetricBoundaries.exponential({ start: 1, factor: 2, count: 12 }),
    Convention.instrument.olapWait.description,
  ),
  Convention.rasm.olapEngine,
  "duckdbNode",
)

const _retried = Metric.tagged(
  Metric.counter(Convention.instrument.olapRetried.name, {
    description: Convention.instrument.olapRetried.description,
    incremental: true,
  }),
  Convention.rasm.olapEngine,
  "duckdbNode",
)

class OlapFault extends Data.TaggedError("OlapFault")<{
  readonly engine: Olap.Engine
  readonly reason: "acquire" | "query" | "extension" | "bundle" | "wire"
  readonly detail: string
}> {}

const _GOVERNOR = {
  budget: Duration.seconds(30),
  retry: Schedule.exponential("100 millis").pipe(
    Schedule.jittered,
    Schedule.intersect(Schedule.recurs(3)),
    Schedule.whileInput((fault: OlapFault) => fault.reason === "query"),
  ),
  sessions: 8,
} as const

type _Statement = {
  readonly sql: string
  readonly values?: ReadonlyArray<DuckDBValue>
  readonly fault: "query" | "extension"
  readonly access: "read" | "write"
}

type OlapRead = Data.TaggedEnum<{
  Rows: _Statement
  Window: _Statement & { readonly take: number }
  Drain: _Statement
}>

const _Read = Data.taggedEnum<OlapRead>()

const _try = <A>(engine: Olap.Engine, reason: OlapFault["reason"], run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: (cause) => new OlapFault({ engine, reason, detail: String(cause) }) })

const _resilient = <A>(statement: _Statement, work: Effect.Effect<A, OlapFault>): Effect.Effect<A, OlapFault> => {
  const timed = work.pipe(
    Effect.timeoutFail({
      duration: _GOVERNOR.budget,
      onTimeout: () => new OlapFault({ engine: "duckdbNode", reason: statement.fault, detail: "<budget>" }),
    }),
  )
  return statement.access === "read" && statement.fault === "query"
    ? timed.pipe(
        Effect.tapError(() => Metric.increment(_retried)),
        Effect.retry(_GOVERNOR.retry),
      )
    : timed
}

const _governed = (gate: Effect.Semaphore) =>
  <A>(statement: _Statement, work: Effect.Effect<A, OlapFault>): Effect.Effect<A, OlapFault> =>
    Effect.acquireUseRelease(
      Effect.tap(Effect.timed(gate.take(1)), ([span]) => Metric.update(_waited, Duration.toMillis(span))), // Gate wait projects onto Convention; permit state stays truth.
      () => _resilient(statement, work),
      () => gate.release(1),
    )

const _values = (values: ReadonlyArray<DuckDBValue> | undefined): Array<DuckDBValue> | undefined =>
  values === undefined ? undefined : Array.from(values)

declare namespace Olap {
  type Session = { readonly connection: DuckDBConnection; readonly gate: Effect.Semaphore }
  type Handle = { readonly lease: Effect.Effect<Session, OlapFault, Scope.Scope> }
  type Row = Record<string, unknown>
}

const _node = (path: string, config?: Record<string, string>): Effect.Effect<Olap.Handle, OlapFault, Scope.Scope> =>
  Effect.gen(function* () {
    const gate = yield* Effect.makeSemaphore(_GOVERNOR.sessions)
    const instance = yield* Effect.acquireRelease(
      _try("duckdbNode", "acquire", () => DuckDBInstance.create(path, config)),
      (held) => Effect.sync(() => held.closeSync()),
    )
    return {
      lease: Effect.acquireRelease(
        _try("duckdbNode", "acquire", () => instance.connect()),
        (held) => Effect.sync(() => held.disconnectSync()),
      ).pipe(Effect.map((connection) => ({ connection, gate }))),
    }
  })

const _wasm = (bundles: wasm.DuckDBBundles): Effect.Effect<wasm.AsyncDuckDB, OlapFault, Scope.Scope> =>
  Effect.acquireRelease(
    _try("duckdbWasm", "bundle", async () => {
      const bundle = await wasm.selectBundle(bundles)
      if (bundle.mainWorker === null) throw new Error("<bundle:no-worker>")
      const worker = new Worker(bundle.mainWorker)
      const db = new wasm.AsyncDuckDB(new wasm.ConsoleLogger(), worker)
      await db.instantiate(bundle.mainModule, bundle.pthreadWorker)
      return db
    }),
    (db) => Effect.promise(() => db.terminate()),
  )

const _paged = (session: Olap.Session, op: Extract<OlapRead, { _tag: "Drain" }>): Stream.Stream<Olap.Row, OlapFault> =>
  Stream.acquireRelease(session.gate.take(1), () => session.gate.release(1)).pipe(
    Stream.flatMap(() =>
      Stream.unwrap(
        _resilient(op, _try("duckdbNode", op.fault, () => session.connection.stream(op.sql, _values(op.values)))).pipe(
          Effect.map((result) =>
            Stream.fromAsyncIterable(
              result.yieldRowObjects(),
              (cause) => new OlapFault({ engine: "duckdbNode", reason: "query", detail: String(cause) }),
            ).pipe(Stream.mapConcat((rows) => rows))),
        ),
      )),
  )

function _read(session: Olap.Session, op: Extract<OlapRead, { _tag: "Rows" }>): Effect.Effect<ReadonlyArray<Olap.Row>, OlapFault>
function _read(session: Olap.Session, op: Extract<OlapRead, { _tag: "Window" }>): Effect.Effect<ReadonlyArray<Olap.Row>, OlapFault>
function _read(session: Olap.Session, op: Extract<OlapRead, { _tag: "Drain" }>): Stream.Stream<Olap.Row, OlapFault>
function _read(session: Olap.Session, op: OlapRead): Effect.Effect<ReadonlyArray<Olap.Row>, OlapFault> | Stream.Stream<Olap.Row, OlapFault> {
  return _Read.$is("Drain")(op)
    ? _paged(session, op)
    : _governed(session.gate)(
        op,
        _Read.$is("Window")(op)
          ? _try("duckdbNode", op.fault, () => session.connection.streamAndReadUntil(op.sql, op.take, _values(op.values))).pipe(
              Effect.map((reader) => reader.getRowObjects().slice(0, op.take)), // readUntil may overshoot by chunk granularity; the window is exact
            )
          : _try("duckdbNode", op.fault, () => session.connection.runAndReadAll(op.sql, _values(op.values))).pipe(
              Effect.map((reader) => reader.getRowObjects()),
            ),
      )
}
```

## [04]-[LAKE_ROWS]

- Owner: the attachment and table-format rows — the statements that bind the embedded engine to the object plane, the pg spine, and the lake catalog; each is data (a statement mint over the `Rows` read geometry), never a new engine surface.
- Packages: none beyond `[3]`'s — the rows are SQL forms.
- Entry: a maintenance fold attaches the spine read-only for offload analytics; the fact rollup egresses Parquet to the object plane through the spine's `parquet` grant or the embedded engine's `httpfs` write; the lake row attaches a DuckLake catalog whose snapshots live in a SQL database and whose data lives as Parquet on the object plane.
- Growth: a new lake format is one attach row; a new offload source is one `ATTACH` mint; a new extension is one `_extensions` seed consumed by the single `extension(name)` statement generator.
- Law: `attach.pg` is read-offload only — the embedded engine reads the spine's tables without a second wire format, and no write path exists from the lane back into the OLTP transaction.
- Law: the lake is ACID over object storage with a SQL catalog — multi-table transactions, time travel, and schema evolution ride the catalog database; the object plane holds immutable Parquet, exactly the content-addressed posture the folder's object rows already enforce.
- Law: range-read Parquet is the browser's only remote source — `Olap.lakeSource` mints the presigned grant through `ObjectStore.grant` and registers it via `registerFileURL(name, url, DuckDBDataProtocol.HTTP, false)`, so the browser-analytics loop is one wired seam bounded by the grant's TTL; no service proxy re-streams rows.

```typescript signature
import { GetObjectCommand } from "@aws-sdk/client-s3"
import { ContentKey } from "@rasm/ts/core"
import { ObjectStore } from "../object/store.ts"

const _extensions = ["httpfs", "ducklake", "iceberg", "delta", "spatial", "vss", "fts"] as const

declare namespace Olap {
  type Extension = (typeof _extensions)[number]
}

const _attach = {
  pg: (dsn: string) => _Read.Rows({ sql: `ATTACH ${quotedString(dsn)} AS spine (TYPE postgres, READ_ONLY)`, fault: "extension", access: "write" }),
  sqlite: (path: string) => _Read.Rows({ sql: `ATTACH ${quotedString(path)} AS lane (TYPE sqlite)`, fault: "extension", access: "write" }),
  ducklake: (catalog: string, dataPath: string) =>
    _Read.Rows({
      sql: `ATTACH ${quotedString(`ducklake:${catalog}`)} AS lake (DATA_PATH ${quotedString(dataPath)})`,
      fault: "extension",
      access: "write",
    }),
  extensions: _extensions,
  extension: (name: Olap.Extension) =>
    _Read.Rows({ sql: `INSTALL ${quotedIdentifier(name)}; LOAD ${quotedIdentifier(name)};`, fault: "extension", access: "write" }),
  httpfs: _Read.Rows({ sql: "INSTALL httpfs; LOAD httpfs;", fault: "extension", access: "write" }),
} as const

const _lakeSource = (db: wasm.AsyncDuckDB, name: string, key: ContentKey) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const grant = yield* store.grant(key, new GetObjectCommand({ Bucket: store.bucket, Key: key }))
    yield* _try("duckdbWasm", "wire", () => db.registerFileURL(name, grant.url, wasm.DuckDBDataProtocol.HTTP, false))
  })
```

## [05]-[CLICKHOUSE]

- Owner: the at-scale driver row — the `ClickhouseClient` Layer mint and `Olap.ingest`, the quota-governed `insertQuery` seam every replication stream rides; neutral analytical reads, command-mode effects, typed parameters, query IDs, and scoped settings remain members of the concrete client held inside the owner rather than parallel lane entries.
- Packages: `@effect/sql-clickhouse` (`ClickhouseClient.layerConfig`, `insertQuery`, `asCommand`, `param`, `withQueryId`, `withClickhouseSettings`); `@effect/experimental` (`RateLimiter.makeWithRateLimiter`, `RateLimiterStore`); `effect` (`Config`).
- Entry: admitted at the composition root only where the `_engines.clickhouse.trigger` condition is real; the fact journal's high-cardinality rollups replicate into MergeTree through `Olap.ingest`, whose intent carries the query id and settings posture applied on the concrete client fiber; dashboards read the cluster, never the OLTP spine.
- Growth: a new ingestion stream is one `ingest` call site over the same layer; a new settings posture is a `withClickhouseSettings` scope; a quota posture is an `_INGEST_QUOTA` override keyed per app, never a consumer wrap.
- Law: the driver extends the neutral `SqlClient`, so analytical reads ride the same `sql` DSL and typed decode as every lane — `clickhouse` is an `onDialect` arm-KEY; only ingestion and command routing reach the concrete Tag.
- Law: ingestion is load-shed at the owner — the token-bucket limiter keys by app so one tenant's replication burst cannot starve siblings, `onExceeded: "delay"` suspends instead of dropping (replication is re-runnable, never lossy by quota), and a durable `RateLimiterStore` composes at a multi-replica root; `withQueryId` and `withClickhouseSettings` scope correlation and server policy to the admitted insert fiber, never process state.
- Law: the quota hold is measured at the owner — the span between entering `_ingest` and the insert's admission past the limiter projects onto the `Convention.instrument.olapDeferred` histogram tagged `clickhouse`, so token-bucket deferral pressure is dashboard-visible while the limiter's suspend disposition stays the behavioral truth.
- Law: the cluster is correctness-adjacent — facts replicate IN, and a lost analytical row is a re-replication, never a billing defect; the journal remains the sole truth.

```typescript signature
import { Clock, Config, type ConfigError, type Layer } from "effect"
import { ClickhouseClient } from "@effect/sql-clickhouse"
import { RateLimiter } from "@effect/experimental"
import type { SqlClient, SqlError } from "@effect/sql"
import type { AppIdentity } from "@rasm/ts/core"

const _clickhouse: Layer.Layer<ClickhouseClient.ClickhouseClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  ClickhouseClient.layerConfig({
    url: Config.string("DATA_CLICKHOUSE_URL"),
    password: Config.redacted("DATA_CLICKHOUSE_PASSWORD"),
  })

const _INGEST_QUOTA = { algorithm: "token-bucket", onExceeded: "delay", window: "1 second", limit: 50 } as const

const _deferred = Metric.tagged(
  Metric.histogram(
    Convention.instrument.olapDeferred.name,
    MetricBoundaries.exponential({ start: 1, factor: 2, count: 12 }),
    Convention.instrument.olapDeferred.description,
  ),
  Convention.rasm.olapEngine,
  "clickhouse",
)

declare namespace Olap {
  type Ingest = Parameters<ClickhouseClient.ClickhouseClient["insertQuery"]>[0] & {
    readonly app: AppIdentity.Key
    readonly queryId: string
    readonly settings: Parameters<ClickhouseClient.ClickhouseClient["withClickhouseSettings"]>[1]
  }
}

const _ingest = (intent: Olap.Ingest) =>
  Effect.gen(function* () {
    const client = yield* ClickhouseClient.ClickhouseClient
    const limit = yield* RateLimiter.makeWithRateLimiter
    const { app, queryId, settings, ...insert } = intent
    const opened = yield* Clock.currentTimeMillis
    return yield* limit({ ..._INGEST_QUOTA, key: `olap:ingest:${app}` })(
      Effect.zipRight(
        Effect.flatMap(Clock.currentTimeMillis, (started) => Metric.update(_deferred, started - opened)), // Delta is the quota hold before insert admission.
        client.insertQuery(insert).pipe(
          client.withClickhouseSettings(settings),
          client.withQueryId(queryId),
        ),
      ),
    )
  })
```

## [06]-[PROFILE]

- Owner: the admitted embedded profile arm and escalation evidence — `_profile` leases one disposable session and holds one permit across enable, one-shot `EXPLAIN ANALYZE`, and teardown; `_probe` folds bounded serial measurements; `_armed` compares evidence against the engine trigger; `Olap` assembles the export.
- Packages: `./postgres.ts` (`Pg.Profile` — the shared receipt schema; profile parity across pg, sqlite, and this lane is one class); `effect` (`Schema`, `Array`, `Option`, `Metric`).
- Entry: an explicit diagnosis call runs `Olap.profile(handle, readStatement, label)`; the maintenance composition runs `Olap.probe` in idle windows per the budget row, holds the prior evidence beside the engine row, and folds `Olap.armed` — an armed verdict fans through the `rasm.data.lane.escalate` hook point at that composition seam, so this lane keeps its single `ObjectStore` value seam beside the profile-schema read and never imports the hook registry.
- Receipt: `Pg.Profile` with `engine: "duckdbNode"`, operator rows carrying timing and cardinality from the profile tree, and `counters` holding `cpuTime` and `resultSetSize` when the tree carries them; `Olap.Evidence` — `{ engine, statement, runs, wallP50, wallMax, rows }`; `Olap.Escalation` — `{ engine, trigger, delta, armed }`, the row's trigger text riding as data so the review argues from the table.
- Growth: a profile engine enters `_PROFILE_ENGINES` only with its landed harvest arm; a probe budget posture is a `_PROBE` field override; a new profile counter is one optional field on `_ProfileTree`.
- Law: profiling toggles are per-connection state — one permit spans enable, execution, and disable, so concurrent users cannot interleave inside the bracket; `_profileRowsOnce` bypasses the retry governor because `EXPLAIN ANALYZE` executes its statement; the `access: "read"` statement case is the only admitted diagnosis input; disable failure remains on the typed rail, and the enclosing scope disposes the leased session on every exit.
- Law: under `enable_profiling='json'` the `EXPLAIN ANALYZE` result carries the profile as its JSON cell — the harvest decodes that cell through `Schema.parseJson`; root latency and returned rows are required receipt evidence, while operator-local measures remain `Option`-carried, so absence fails the wire instead of forging zero.
- Law: probes run beside production lanes — `_PROBE.runs` bounds the repetition, the run rides the same governed session gate as every statement so a probe cannot starve live work, the whole profile bracket (enable, EXPLAIN ANALYZE, disable, release) rides one `_GOVERNOR.budget` timeout so a stalled diagnosis releases its permit, and each harvest's `wallMillis` projects onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine` — embedded engines expose no scrape surface, so this harvest is their whole observability.
- Law: escalation is evidence-driven row data — `_armed` compares evidence receipts by their p50 wall ratio against the `_PROBE.floor`, names the engine row's own trigger text, and never mutates the row; admitting ClickHouse below its trigger remains the named operational waste the table refuses.

```typescript signature
import { Pg } from "./postgres.ts"
import { Array, Exit, Option, Order, Schema } from "effect"

const _PROBE = { runs: 5, floor: 1.5 } as const // bounded repeatable measurement; the p50 ratio that arms a trigger

const _profiled = Metric.tagged(
  Metric.histogram(
    Convention.instrument.profileDuration.name,
    MetricBoundaries.exponential({ start: 1, factor: 2, count: 14 }),
    Convention.instrument.profileDuration.description,
  ),
  Convention.rasm.profileEngine,
  "duckdbNode",
)

interface _ProfileTreeEncoded {
  readonly operator_type?: string
  readonly operator_timing?: number
  readonly operator_cardinality?: number
  readonly latency?: number
  readonly rows_returned?: number
  readonly cpu_time?: number
  readonly result_set_size?: number
  readonly children?: ReadonlyArray<_ProfileTreeEncoded>
}

interface _ProfileTree {
  readonly operator_type: Option.Option<string>
  readonly operator_timing: Option.Option<number>
  readonly operator_cardinality: Option.Option<number>
  readonly latency: Option.Option<number>
  readonly rows_returned: Option.Option<number>
  readonly cpu_time: Option.Option<number>
  readonly result_set_size: Option.Option<number>
  readonly children: Option.Option<ReadonlyArray<_ProfileTree>>
}

const _Tree: Schema.Schema<_ProfileTree, _ProfileTreeEncoded> = Schema.Struct({
  operator_type: Schema.optionalWith(Schema.String, { as: "Option" }),
  operator_timing: Schema.optionalWith(Schema.Number, { as: "Option" }),
  operator_cardinality: Schema.optionalWith(Schema.Number, { as: "Option" }),
  latency: Schema.optionalWith(Schema.Number, { as: "Option" }),
  rows_returned: Schema.optionalWith(Schema.Number, { as: "Option" }),
  cpu_time: Schema.optionalWith(Schema.Number, { as: "Option" }),
  result_set_size: Schema.optionalWith(Schema.Number, { as: "Option" }),
  children: Schema.optionalWith(Schema.Array(Schema.suspend((): Schema.Schema<_ProfileTree, _ProfileTreeEncoded> => _Tree)), { as: "Option" }),
})

const _Root = Schema.Struct({
  operator_type: Schema.optionalWith(Schema.String, { as: "Option" }),
  operator_timing: Schema.optionalWith(Schema.Number, { as: "Option" }),
  operator_cardinality: Schema.optionalWith(Schema.Number, { as: "Option" }),
  latency: Schema.Number,
  rows_returned: Schema.Number,
  cpu_time: Schema.optionalWith(Schema.Number, { as: "Option" }),
  result_set_size: Schema.optionalWith(Schema.Number, { as: "Option" }),
  children: Schema.optionalWith(Schema.Array(_Tree), { as: "Option" }),
})

type _OperatorTree = Pick<_ProfileTree, "operator_type" | "operator_timing" | "operator_cardinality" | "children">

const _steps = (node: _OperatorTree): ReadonlyArray<Pg.Profile["operators"][number]> => [
  ...Option.match(node.operator_type, {
    onNone: () => [],
    onSome: (name) => [{
      name,
      millis: Option.map(node.operator_timing, (seconds) => seconds * 1000),
      rows: Option.map(node.operator_cardinality, Math.trunc),
    }],
  }),
  ...Option.match(node.children, { onNone: () => [], onSome: Array.flatMap(_steps) }),
]

type _ReadStatement = _Statement & { readonly access: "read" }

const _profileRowsOnce = (session: Olap.Session, statement: _Statement): Effect.Effect<ReadonlyArray<Olap.Row>, OlapFault> =>
  _try("duckdbNode", statement.fault, () => session.connection.runAndReadAll(statement.sql, _values(statement.values))).pipe(
    Effect.map((reader) => reader.getRowObjects()),
  )

const _profileOnce = (session: Olap.Session, statement: _ReadStatement, label: string): Effect.Effect<Pg.Profile, OlapFault> =>
  Effect.acquireUseRelease(
    Effect.tap(Effect.timed(session.gate.take(1)), ([span]) => Metric.update(_waited, Duration.toMillis(span))),
    () =>
      Effect.gen(function* () {
        yield* _profileRowsOnce(session, { sql: "PRAGMA enable_profiling='json'", fault: "query", access: "write" })
        const explained = yield* Effect.exit(_profileRowsOnce(session, {
          sql: `EXPLAIN ANALYZE ${statement.sql}`,
          values: statement.values,
          fault: statement.fault,
          access: "read",
        }))
        yield* _profileRowsOnce(session, { sql: "PRAGMA disable_profiling", fault: "query", access: "write" })
        const raw = yield* Exit.matchEffect(explained, {
          onFailure: Effect.failCause,
          onSuccess: Effect.succeed,
        })
        const cell = yield* Option.match(
          Option.flatMap(Array.head(raw), (row) => Option.fromNullable(row["explain_value"])),
          {
            onNone: () => Effect.fail(new OlapFault({ engine: "duckdbNode", reason: "wire", detail: "<no-profile-cell>" })),
            onSome: (held) => Effect.succeed(String(held)),
          },
        )
        const tree = yield* Schema.decodeUnknown(Schema.parseJson(_Root))(cell).pipe(
          Effect.mapError((fault) => new OlapFault({ engine: "duckdbNode", reason: "wire", detail: String(fault) })),
        )
        const wallMillis = tree.latency * 1000
        yield* Metric.update(_profiled, wallMillis)
        return new Pg.Profile({
          engine: "duckdbNode",
          statement: label,
          wallMillis,
          rows: Math.trunc(tree.rows_returned),
          operators: _steps(tree),
          counters: {
            ...Option.match(tree.cpu_time, { onNone: () => ({}), onSome: (held) => ({ cpuTime: held }) }),
            ...Option.match(tree.result_set_size, { onNone: () => ({}), onSome: (held) => ({ resultSetSize: held }) }),
          },
          window: Option.none(),
        })
      }),
    () => session.gate.release(1),
  ).pipe(
    // Whole-bracket budget: PRAGMA enable, EXPLAIN ANALYZE, PRAGMA disable, and the gate release ride one
    // governor timeout together, so a stalled profile can never hold a session permit past the budget.
    Effect.timeoutFail({
      duration: _GOVERNOR.budget,
      onTimeout: () => new OlapFault({ engine: "duckdbNode", reason: statement.fault, detail: "<profile-budget>" }),
    }),
  )

const _profile = (handle: Olap.Handle, statement: _ReadStatement, label: string): Effect.Effect<Pg.Profile, OlapFault> =>
  Effect.scoped(Effect.flatMap(handle.lease, (session) => _profileOnce(session, statement, label)))

declare namespace Olap {
  type Evidence = {
    readonly engine: Engine
    readonly statement: string
    readonly runs: number
    readonly wallP50: number
    readonly wallMax: number
    readonly rows: number
  }
  type Escalation = { readonly engine: Engine; readonly trigger: string; readonly delta: number; readonly armed: boolean }
}

const _probe = (handle: Olap.Handle, statement: _ReadStatement, label: string): Effect.Effect<Olap.Evidence, OlapFault> =>
  Effect.flatMap(
    Effect.forEach(Array.range(1, _PROBE.runs), () => _profile(handle, statement, label), { concurrency: 1 }),
    (receipts) =>
      Effect.gen(function* () {
        const first = yield* Effect.fromOption(
          Array.head(receipts),
          () => new OlapFault({ engine: "duckdbNode", reason: "wire", detail: "<empty-probe>" }),
        )
        const walls = Array.sort(Array.map(receipts, (receipt) => receipt.wallMillis), Order.number)
        const wallP50 = yield* Effect.fromOption(
          Array.get(walls, Math.trunc(walls.length / 2)),
          () => new OlapFault({ engine: "duckdbNode", reason: "wire", detail: "<empty-p50>" }),
        )
        const wallMax = yield* Effect.fromOption(
          Array.last(walls),
          () => new OlapFault({ engine: "duckdbNode", reason: "wire", detail: "<empty-maximum>" }),
        )
        return {
          engine: "duckdbNode" as const,
          statement: label,
          runs: receipts.length,
          wallP50,
          wallMax,
          rows: first.rows,
        }
      }),
  )

const _armed = (engine: Olap.Engine, prior: Olap.Evidence, next: Olap.Evidence): Olap.Escalation => {
  const delta = next.wallP50 / Math.max(prior.wallP50, 1)
  return { engine, trigger: _engines[engine].trigger, delta, armed: delta >= _PROBE.floor }
}
```

## [07]-[ARROW_WIRE]

- Owner: the one columnar interchange — typed decode and encode, bounded batch streaming, wasm lazy pull, source-shaped ingest, and the assembled `Olap` export.
- Packages: `apache-arrow` (`tableFromIPC`, `tableToIPC`, `RecordBatchReader`, `Table`); `@duckdb/duckdb-wasm` (`conn.send`, `conn.insertArrowTable`, `conn.insertArrowFromIPCStream`).
- Entry: wasm query results are already Tables; node results and ClickHouse Arrow output decode through `tableFromIPC`; the viewer's geoarrow plane consumes the same Tables downstream; the wasm ingest entry closes the loop for locally staged frames.
- Growth: a new engine row joins the wire by emitting or accepting IPC — no per-engine result shape is ever admitted.
- Law: one wire — an analytical result crossing any engine seam travels as Arrow; a JSON or row-object re-encoding between engines is the named defect, and the only row-shaped egress is the final consumer projection.
- Law: streams stay bounded — large interchange rides `RecordBatchReader` batch iteration lifted to `Stream`, and the wasm side pulls lazily through `conn.send` so a browser result larger than memory never materializes as one Table.
- Law: ingest is ONE entry discriminating on the source value — a live `Table` rides `insertArrowTable`, IPC bytes ride `insertArrowFromIPCStream`; a per-format sibling pair is the deleted spelling.

```typescript signature
import { RecordBatchReader, Table, tableFromIPC, tableToIPC } from "apache-arrow"

const _wire = {
  decode: (engine: Olap.Engine, bytes: Uint8Array): Effect.Effect<Table, OlapFault> =>
    Effect.try({
      try: () => tableFromIPC(bytes),
      catch: (cause) => new OlapFault({ engine, reason: "wire", detail: String(cause) }),
    }),
  encode: (engine: Olap.Engine, table: Table): Effect.Effect<Uint8Array, OlapFault> =>
    Effect.try({
      try: () => tableToIPC(table),
      catch: (cause) => new OlapFault({ engine, reason: "wire", detail: String(cause) }),
    }),
  batches: (engine: Olap.Engine, source: AsyncIterable<Uint8Array>) =>
    Stream.unwrap(
      _try(engine, "wire", () => RecordBatchReader.from(source)).pipe(
        Effect.map((reader) => Stream.fromAsyncIterable(reader, (cause) =>
          new OlapFault({ engine, reason: "wire", detail: String(cause) }))),
      )),
  pull: (conn: wasm.AsyncDuckDBConnection, sql: string) =>
    Stream.unwrap(
      _try("duckdbWasm", "query", () => conn.send(sql)).pipe(
        Effect.map((reader) => Stream.fromAsyncIterable(reader, (cause) =>
          new OlapFault({ engine: "duckdbWasm", reason: "query", detail: String(cause) }))),
      )),
  feed: (conn: wasm.AsyncDuckDBConnection, name: string, source: Table | Uint8Array) =>
    _try("duckdbWasm", "wire", () =>
      source instanceof Uint8Array ? conn.insertArrowFromIPCStream(source, { name }) : conn.insertArrowTable(source, { name })),
} as const

const Olap = {
  engines: _engines,
  node: _node,
  wasm: _wasm,
  Read: _Read,
  read: _read,
  attach: _attach,
  lakeSource: _lakeSource,
  clickhouse: _clickhouse,
  ingest: _ingest,
  wire: _wire,
  profile: _profile,
  probe: _probe,
  armed: _armed,
  Fault: OlapFault,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Olap, OlapFault }
```

## [08]-[RESEARCH]

- [PROFILE_ROW_SHAPE]-[OPEN]: which typed cell shape does `runAndReadAll(...).getRowObjects()` yield for the `explain_value` column of `EXPLAIN ANALYZE` under `PRAGMA enable_profiling='json'`; `libs/typescript/data/.api/duckdb-node-api.md` `[02]` (`DuckDBResultReader.getRowObjects`) and `[03]` (`runAndReadAll`), armed on the exact declarations and one decoded profile-row fixture pinning the `explain_value` cell the `_Root` decode consumes.
- [CLICKHOUSE_PROFILE_SCOPE]-[OPEN]: which query-id scope yields ClickHouse profile evidence; `libs/typescript/data/.api/effect-sql-clickhouse.md` `[02]`, armed on exact scope and result declarations.
