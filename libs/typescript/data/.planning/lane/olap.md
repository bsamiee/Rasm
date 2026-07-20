# [DATA_OLAP]

The analytical lane: columnar vectorized throughput as a guarantee distinct from transactional durability, owned by one engine-row table and three scoped engine wraps. DuckDB embedded is the default row — node in services and CLI, wasm pushing compute to the browser over range-read Parquet — with analytics-in-OLTP riding the spine's `analytics` grant, ClickHouse admitted only past the crisp distributed trigger, and Flight SQL the engine-blind remote columnar wire. Arrow is the ONE columnar wire: every result crossing an engine seam travels as a Table in memory or IPC on the wire, and the OLAP lane never rides the OLTP transaction — journal facts replicate in, verdicts flow out, nothing folds back as authority. The engines are boundary kernels: promise APIs lifted through typed acquire-release wraps, read geometry a tagged case value dispatched through ONE polymorphic entry, never a second query paradigm inside the folder. Query evidence harvests into the one `Pg.Profile` band the spine mints, and gate pressure projects onto Convention instruments — receipts stay the truth, instruments the lossy dashboard channel.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                              |
| :-----: | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ENGINE_ROWS` | the five-row decision table — guarantee, storage, ceiling, trigger per engine       |
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

- Owner: the two scoped engine wraps — `Olap.node(path, config?)` acquiring a `DuckDBInstance` with the owner-built session bulkhead, and `Olap.wasm(bundles)` instantiating the worker-resident `AsyncDuckDB` — plus `Olap.read`, the ONE statement entry over a leased session whose modality is the `Olap.Read` case value: `Rows` materializes a bounded result, `Window` serves the bounded first window through `streamAndReadUntil`, and `Drain` acquires the native streaming result and flattens `yieldRowObjects()` batch-by-batch under one permit held for the stream lifetime; the overload set gives each case its own return, and every geometry rides the owner-internal governor.
- Packages: `@duckdb/node-api` (`DuckDBInstance.create`, `instance.closeSync`, `instance.connect`, `connection.disconnectSync`, `connection.runAndReadAll`, `connection.stream`, `connection.streamAndReadUntil`, `DuckDBResult.yieldRowObjects`, `quotedIdentifier`, `quotedString`); `@duckdb/duckdb-wasm` (`selectBundle`, `AsyncDuckDB`, `ConsoleLogger`, `db.instantiate`, `db.connect`, `conn.query`, `conn.send`, `db.registerFileURL`, `DuckDBDataProtocol`); `effect` (`Effect`, `Scope`, `Stream`, `Data`, `Schedule`, `Duration`).
- Entry: a service composes `Olap.node` once per database coordinate and leases sessions per analytical unit of work through `handle.lease`; the browser shell composes `Olap.wasm` over self-hosted bundles at boot and hands connections to the viewer's query surfaces.
- Receipt: node reads land as row-object projections; wasm queries land as Arrow Tables — the wire cluster's value, zero-copy into the viewer.
- Growth: a new engine knob (`threads`, extension roster) is a config field on the acquire; a new read geometry is one `Olap.Read` case plus its overload line and dispatch arm, never a sibling entry; a resilience posture is a `_GOVERNOR` field override, never a consumer wrap.
- Law: lifecycle is `acquireRelease` under `Scope` — instance, worker, and every connection release deterministically; an unscoped engine handle is unspellable because the constructors return scoped effects.
- Law: every promise lifts through the one boundary kernel `_try` into the reason-discriminated `OlapFault` — `acquire | query | extension | bundle | wire` routes recovery as a fold, never a `detail` string match; extension-load refusal is `extension`, bundle selection is `bundle`, and above the kernel the lane is rails end to end.
- Law: resilience is owner-internal — the bulkhead semaphore mints inside `Olap.node` (`Effect.makeSemaphore(_GOVERNOR.sessions)`), and `_governed` brackets every statement with the timeout budget and the jittered bounded retry gated to `query`-reason faults, so a consumer composes capability and never constructs, sees, or threads the gate; the governor values are one policy row, and the gate's own pressure projects onto the `Convention.instrument.olapWait` histogram and `Convention.instrument.olapRetried` counter tagged `Convention.rasm.olapEngine` — instruments ride the owner bracket, never per-effect decorators, and the permit ledger stays the truth they lossily project.
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
}

type OlapRead = Data.TaggedEnum<{
  Rows: _Statement
  Window: _Statement & { readonly take: number }
  Drain: _Statement
}>

const _Read = Data.taggedEnum<OlapRead>()

const _try = <A>(engine: Olap.Engine, reason: OlapFault["reason"], run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: (cause) => new OlapFault({ engine, reason, detail: String(cause) }) })

const _resilient = <A>(reason: _Statement["fault"], work: Effect.Effect<A, OlapFault>): Effect.Effect<A, OlapFault> =>
  work.pipe(
    Effect.timeoutFail({
      duration: _GOVERNOR.budget,
      onTimeout: () => new OlapFault({ engine: "duckdbNode", reason, detail: "<budget>" }),
    }),
    Effect.tapError((fault) => (fault.reason === "query" ? Metric.increment(_retried) : Effect.void)), // attempt-scoped: composed before the retry step, so every governed re-drive is counted
    Effect.retry(_GOVERNOR.retry),
  )

const _governed = (gate: Effect.Semaphore) =>
  <A>(reason: _Statement["fault"], work: Effect.Effect<A, OlapFault>): Effect.Effect<A, OlapFault> =>
    Effect.acquireUseRelease(
      Effect.tap(Effect.timed(gate.take(1)), ([span]) => Metric.update(_waited, Duration.toMillis(span))), // the gate wait projects onto the Convention histogram; the permit itself stays the truth
      () => _resilient(reason, work),
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
        _resilient(op.fault, _try("duckdbNode", op.fault, () => session.connection.stream(op.sql, _values(op.values)))).pipe(
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
        op.fault,
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
  pg: (dsn: string) => _Read.Rows({ sql: `ATTACH ${quotedString(dsn)} AS spine (TYPE postgres, READ_ONLY)`, fault: "extension" }),
  sqlite: (path: string) => _Read.Rows({ sql: `ATTACH ${quotedString(path)} AS lane (TYPE sqlite)`, fault: "extension" }),
  ducklake: (catalog: string, dataPath: string) =>
    _Read.Rows({
      sql: `ATTACH ${quotedString(`ducklake:${catalog}`)} AS lake (DATA_PATH ${quotedString(dataPath)})`,
      fault: "extension",
    }),
  extensions: _extensions,
  extension: (name: Olap.Extension) =>
    _Read.Rows({ sql: `INSTALL ${quotedIdentifier(name)}; LOAD ${quotedIdentifier(name)};`, fault: "extension" }),
  httpfs: _Read.Rows({ sql: "INSTALL httpfs; LOAD httpfs;", fault: "extension" }),
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
- Entry: admitted at the composition root only where the `_engines.clickhouse.trigger` condition is real; the fact journal's high-cardinality rollups replicate into MergeTree through `Olap.ingest`, and dashboards read the cluster, never the OLTP spine.
- Growth: a new ingestion stream is one `ingest` call site over the same layer; a new settings posture is a `withClickhouseSettings` scope; a quota posture is an `_INGEST_QUOTA` override keyed per app, never a consumer wrap.
- Law: the driver extends the neutral `SqlClient`, so analytical reads ride the same `sql` DSL and typed decode as every lane — `clickhouse` is an `onDialect` arm-KEY; only ingestion and command routing reach the concrete Tag.
- Law: ingestion is load-shed at the owner — the token-bucket limiter keys by app so one tenant's replication burst cannot starve siblings, `onExceeded: "delay"` suspends instead of dropping (replication is re-runnable, never lossy by quota), and a durable `RateLimiterStore` composes at a multi-replica root; `layerStoreMemory` is admitted only for a single-process topology and never described as distributed.
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
  type Ingest = Parameters<ClickhouseClient.ClickhouseClient["insertQuery"]>[0] & { readonly app: AppIdentity.Key }
}

const _ingest = (intent: Olap.Ingest) =>
  Effect.gen(function* () {
    const client = yield* ClickhouseClient.ClickhouseClient
    const limit = yield* RateLimiter.makeWithRateLimiter
    const { app, ...insert } = intent
    const opened = yield* Clock.currentTimeMillis
    return yield* limit({ ..._INGEST_QUOTA, key: `olap:ingest:${app}` })(
      Effect.zipRight(
        Effect.flatMap(Clock.currentTimeMillis, (started) => Metric.update(_deferred, started - opened)), // the delta IS the quota hold: the limiter suspended exactly this long before admitting the insert
        client.insertQuery(insert),
      ),
    )
  })
```

## [07]-[PROFILE]

- Owner: the embedded arm of the one engine-profile receipt family and the evidence plane that arms escalation — `_profile`, the per-query harvest bracketing `PRAGMA enable_profiling='json'` around an `EXPLAIN ANALYZE` run and folding the JSON profile tree into `Pg.Profile`; `_probe`, the bounded repeatable measurement run folding N harvests into one claim-shaped `Olap.Evidence`; `_armed`, the pure delta fold that turns two evidence receipts into an `Olap.Escalation` verdict against the row's own trigger — plus the assembled `Olap` export.
- Packages: `./postgres.ts` (`Pg.Profile` — the shared receipt schema; profile parity across pg, sqlite, and this lane is one class); `effect` (`Schema`, `Array`, `Option`, `Metric`).
- Entry: an explicit diagnosis call runs `Olap.profile(session, statement, label)`; the maintenance composition runs `Olap.probe` in idle windows per the budget row, holds the prior evidence beside the engine row, and folds `Olap.armed` — an armed verdict fans through the `rasm.data.lane.escalate` hook point at that composition seam, so this lane keeps its single `ObjectStore` value seam beside the profile-schema read and never imports the hook registry.
- Receipt: `Pg.Profile` with `engine: "duckdbNode"`, operator rows carrying timing and cardinality from the profile tree, and `counters` holding `cpuTime` and `resultSetSize` when the tree carries them; `Olap.Evidence` — `{ engine, statement, runs, wallP50, wallMax, rows }`; `Olap.Escalation` — `{ engine, trigger, delta, armed }`, the row's trigger text riding as data so the review argues from the table.
- Growth: the wasm arm is the same fold over `conn.send` output when browser diagnosis earns it — one `engine` literal, zero schema edits; a probe budget posture is a `_PROBE` field override; a new profile counter is one optional field on `_ProfileTree`.
- Law: profiling toggles are per-connection state — the bracket enables `json` profiling, runs exactly the profiled statement, and `Effect.ensuring` disables it on every exit, so a leaked toggle cannot tax the session's next statement; the harvest EXECUTES the statement and scopes to explicit diagnosis, never ambient reads.
- Law: under `enable_profiling='json'` the `EXPLAIN ANALYZE` result carries the profile as its JSON cell — the harvest decodes that one cell through `Schema.parseJson` into the tolerant tree (every measure `Option`-carried, an absent clock is omission, never zero), and a malformed profile is the `wire` reason on the lane fault, never a hand-parsed guess.
- Law: probes run beside production lanes — `_PROBE.runs` bounds the repetition, the run rides the same governed session gate as every statement so a probe cannot starve live work, and each harvest's `wallMillis` projects onto the `Convention.instrument.profileDuration` histogram tagged `Convention.rasm.profileEngine` — embedded engines expose no scrape surface, so this harvest is their whole observability.
- Law: escalation is evidence-driven row data — `_armed` compares evidence receipts by their p50 wall ratio against the `_PROBE.floor`, names the engine row's own trigger text, and never mutates the row; admitting ClickHouse below its trigger remains the named operational waste the table refuses.

```typescript signature
import { Pg } from "./postgres.ts"
import { Array, Option, Order, Schema } from "effect"

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

const _steps = (node: _ProfileTree): ReadonlyArray<Pg.Profile["operators"][number]> => [
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

const _profile = (session: Olap.Session, statement: _Statement, label: string): Effect.Effect<Pg.Profile, OlapFault> =>
  Effect.gen(function* () {
    yield* _read(session, _Read.Rows({ sql: "PRAGMA enable_profiling='json'", fault: "query" }))
    const raw = yield* _read(session, _Read.Rows({ sql: `EXPLAIN ANALYZE ${statement.sql}`, values: statement.values, fault: statement.fault })).pipe(
      Effect.ensuring(Effect.ignore(_read(session, _Read.Rows({ sql: "PRAGMA disable_profiling", fault: "query" })))), // the toggle dies with the diagnosis on every exit
    )
    const cell = yield* Option.match(
      Option.flatMap(Array.head(raw), (row) => Option.fromNullable(row["explain_value"])),
      {
        onNone: () => Effect.fail(new OlapFault({ engine: "duckdbNode", reason: "wire", detail: "<no-profile-cell>" })),
        onSome: (held) => Effect.succeed(String(held)),
      },
    )
    const tree = yield* Schema.decodeUnknown(Schema.parseJson(_Tree))(cell).pipe(
      Effect.mapError((fault) => new OlapFault({ engine: "duckdbNode", reason: "wire", detail: String(fault) })),
    )
    const wallMillis = Option.getOrElse(Option.map(tree.latency, (seconds) => seconds * 1000), () => 0)
    yield* Metric.update(_profiled, wallMillis)
    return new Pg.Profile({
      engine: "duckdbNode",
      statement: label,
      wallMillis,
      rows: Option.getOrElse(Option.map(tree.rows_returned, Math.trunc), () => 0),
      operators: _steps(tree),
      counters: {
        ...Option.match(tree.cpu_time, { onNone: () => ({}), onSome: (held) => ({ cpuTime: held }) }),
        ...Option.match(tree.result_set_size, { onNone: () => ({}), onSome: (held) => ({ resultSetSize: held }) }),
      },
      window: Option.none(),
    })
  })

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

const _probe = (session: Olap.Session, statement: _Statement, label: string): Effect.Effect<Olap.Evidence, OlapFault> =>
  Effect.map(
    Effect.forEach(Array.range(1, _PROBE.runs), () => _profile(session, statement, label)),
    (receipts) => {
      const walls = Array.sort(Array.map(receipts, (receipt) => receipt.wallMillis), Order.number)
      return {
        engine: "duckdbNode" as const,
        statement: label,
        runs: receipts.length,
        wallP50: Option.getOrElse(Array.get(walls, Math.trunc(walls.length / 2)), () => 0),
        wallMax: Option.getOrElse(Array.last(walls), () => 0),
        rows: Option.getOrElse(Option.map(Array.head(receipts), (receipt) => receipt.rows), () => 0),
      }
    },
  )

const _armed = (engine: Olap.Engine, prior: Olap.Evidence, next: Olap.Evidence): Olap.Escalation => {
  const delta = next.wallP50 / Math.max(prior.wallP50, 1)
  return { engine, trigger: _engines[engine].trigger, delta, armed: delta >= _PROBE.floor }
}
```

## [08]-[ARROW_WIRE]

- Owner: the one columnar interchange — typed-effect decode and encode at the lane seams, batch streaming for bounded memory, the wasm lazy record-batch pull, and the one ingest entry whose modality is the source shape — plus the assembled `Olap` export.
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
