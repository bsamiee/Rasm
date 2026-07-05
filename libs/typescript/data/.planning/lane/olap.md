# [DATA_OLAP]

The analytical lane: columnar vectorized throughput as a guarantee distinct from transactional durability, owned by one engine-row table and two scoped engine wraps. DuckDB embedded is the default row — node in services and CLI, wasm pushing compute to the browser over range-read Parquet — with analytics-in-OLTP riding the spine's `analytics` grant and ClickHouse admitted only past the crisp distributed trigger. Arrow is the ONE columnar wire: every result crossing an engine seam travels as a Table in memory or IPC on the wire, and the OLAP lane never rides the OLTP transaction — journal facts replicate in, verdicts flow out, nothing folds back as authority. The engines are boundary kernels: promise APIs lifted through typed acquire-release wraps, never a second query paradigm inside the folder.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                          |
| :-----: | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `ENGINE_ROWS`  | the four-row decision table — guarantee, storage, ceiling, trigger per engine          |
|  [02]   | `EMBEDDED`     | the scoped DuckDB wraps — node instance/session, wasm worker engine, statement lift    |
|  [03]   | `LAKE_ROWS`    | the table-format and attachment rows — DuckLake, object-store Parquet, spine ATTACH    |
|  [04]   | `CLICKHOUSE`   | the at-scale driver row and its ingestion surface                                      |
|  [05]   | `ARROW_WIRE`   | the one columnar interchange — Table, IPC both directions, batch streaming             |

## [2]-[ENGINE_ROWS]

- Owner: the `_engines` anchor — one row per engine profile carrying its guarantee, storage posture, and scale ceiling; the escalation trigger is data on the row, so an admission argument reads the table instead of relitigating the lane.
- Packages: none — the rows are decision facts.
- Growth: a new engine candidate is one row measured against the same columns; a row whose trigger never fires in production is a prune candidate at the next census.
- Law: `duckdbNode` is the default — vectorized execution, out-of-core spill, zero-copy Parquet/CSV/JSON/Arrow, single-file ACID under a single writer; embedded in-process, so the ceiling is one node and one concurrent writer.
- Law: `duckdbWasm` is the client-side row — the browser queries remote Parquet by HTTP range through presigned grants, results land as Arrow, and compute leaves the service bill entirely; single-threaded by default, CORS-bound, an accelerator over server-minted data.
- Law: `pgDuckdb` is the analytics-in-OLTP row — the spine's `analytics` grant embeds the same engine inside live Postgres for workloads adjacent to transactional data; no second system, PG durability, bounded by the single PG host; the grant is `lane/postgres.md`'s row and this lane only names the boundary.
- Law: `clickhouse` is admitted ONLY past the trigger — concurrent high-throughput ingestion, multi-node scale, or high-cardinality real-time serving; below it the embedded rows own the workload, and admitting the cluster early is the named operational waste.

```typescript
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

## [3]-[EMBEDDED]

- Owner: the two scoped engine wraps — `Olap.node(path, config?)` acquiring a `DuckDBInstance` and leasing sessions under `Scope`, and `Olap.wasm(bundles)` instantiating the worker-resident `AsyncDuckDB` — plus `Olap.query`, the one statement entry over a leased session whose modality is the read geometry: `rows` materializes bounded results, `drain` streams a large-but-bounded result through streaming-mode execution, `window` serves the bounded first window; every geometry rides the `_governed` resilience bracket. RESEARCH: the reader-continuation pair (`readUntil`/`done` on `DuckDBResultReader`) stays uncatalogued — the truly unbounded incremental fence settles on it; until then unbounded egress pages by keyset predicate or rides Arrow batch streaming.
- Packages: `@duckdb/node-api` (`DuckDBInstance.create`, `instance.closeSync`, `instance.connect`, `connection.disconnectSync`, `connection.runAndReadAll`, `connection.streamAndReadAll`, `connection.streamAndReadUntil`, `connection.prepare`); `@duckdb/duckdb-wasm` (`selectBundle`, `AsyncDuckDB`, `ConsoleLogger`, `db.instantiate`, `db.connect`, `conn.query`, `conn.send`, `db.registerFileURL`, `DuckDBDataProtocol`); `effect` (`Effect`, `Scope`, `Stream`, `Data`, `Schedule`, `Duration`).
- Entry: a service composes `Olap.node` once per database coordinate and leases sessions per analytical unit of work; the browser shell composes `Olap.wasm` over self-hosted bundles at boot and hands connections to the viewer's query surfaces.
- Receipt: node reads land as reader projections (`getRows`/`getColumns`); wasm queries land as Arrow Tables — the wire cluster's value, zero-copy into the viewer.
- Growth: a new engine knob (`threads`, extension roster) is a config field on the acquire; a new ingestion source is a registered file or an `ATTACH` statement, never a new API; a resilience posture is a `_GOVERNOR` field override, never a consumer wrap.
- Law: lifecycle is `acquireRelease` under `Scope` — instance, worker, and every connection release deterministically; an unscoped engine handle is unspellable because the constructors return scoped effects.
- Law: every promise lifts through the one boundary kernel `_try` into the reason-discriminated `OlapFault` — `acquire | query | extension | bundle | wire` routes recovery as a fold, never a `detail` string match; extension-load refusal is `extension`, bundle selection is `bundle`, and above the kernel the lane is rails end to end.
- Law: resilience is owner-internal — `_governed` brackets every statement with the timeout budget, the jittered bounded retry gated to `query`-reason faults, and the session bulkhead semaphore, so a consumer composes capability and never plumbing; the governor values are one policy row.
- Law: extension admission is a statement — `INSTALL`/`LOAD` for `httpfs`, `ducklake`, `iceberg`, `delta`, `spatial`, `vss`, `fts` run through `Olap.query`; a load failure refuses the capability as a typed `extension` fault, never crashes the lane.
- Law: bundles self-host beside the app shell — `selectBundle` over owned artifact coordinates; a CDN bundle load is rejected by the deployment's content policy.
- Boundary: `_try` and `_wasm` are the marked promise kernels — the `as never` bind cast and the ambient `Worker` construction live only inside them; `_wasm`'s thrown missing-worker guard is caught by its own `tryPromise` and folds to the `bundle` reason.

```typescript
import { Chunk, Data, Duration, Effect, Schedule, type Scope, Stream } from "effect"
import { DuckDBInstance } from "@duckdb/node-api"
import * as wasm from "@duckdb/duckdb-wasm"

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

const _try = <A>(engine: Olap.Engine, reason: OlapFault["reason"], run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: (cause) => new OlapFault({ engine, reason, detail: String(cause) }) })

const _governed = (gate: Effect.Semaphore) =>
  <A>(work: Effect.Effect<A, OlapFault>): Effect.Effect<A, OlapFault> =>
    gate.withPermits(1)(
      work.pipe(
        Effect.timeoutFail({
          duration: _GOVERNOR.budget,
          onTimeout: () => new OlapFault({ engine: "duckdbNode", reason: "query", detail: "<budget>" }),
        }),
        Effect.retry(_GOVERNOR.retry),
      ),
    )

const _node = (path: string, config?: Record<string, string>) =>
  Effect.acquireRelease(
    _try("duckdbNode", "acquire", () => DuckDBInstance.create(path, config)),
    (instance) => Effect.sync(() => instance.closeSync()),
  )

const _session = (instance: DuckDBInstance) =>
  Effect.acquireRelease(
    _try("duckdbNode", "acquire", () => instance.connect()),
    (held) => Effect.sync(() => held.disconnectSync()),
  )

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

const _query = (connection: Awaited<ReturnType<DuckDBInstance["connect"]>>, gate: Effect.Semaphore) => {
  const governed = _governed(gate)
  return {
    rows: (sql: string, values?: ReadonlyArray<unknown>) =>
      governed(
        _try("duckdbNode", "query", () => connection.runAndReadAll(sql, values as never)).pipe(
          Effect.map((reader) => reader.getRows()),
        )),
    drain: (sql: string, values?: ReadonlyArray<unknown>) =>
      Stream.unwrap(
        governed(_try("duckdbNode", "query", () => connection.streamAndReadAll(sql, values as never))).pipe(
          Effect.map((reader) => Stream.fromIterable(reader.getRows())),
        )),
    window: (sql: string, take: number) =>
      Stream.unwrap(
        governed(_try("duckdbNode", "query", () => connection.streamAndReadUntil(sql, take))).pipe(
          Effect.map((reader) => Stream.fromChunk(Chunk.fromIterable(reader.getRows()))),
        )),
  }
}
```

## [4]-[LAKE_ROWS]

- Owner: the attachment and table-format rows — the statements that bind the embedded engine to the object plane, the pg spine, and the lake catalog; each is data (a statement mint over `Olap.query`), never a new engine surface.
- Packages: none beyond `[3]`'s — the rows are SQL forms.
- Entry: a maintenance fold attaches the spine read-only for offload analytics; the fact rollup egresses Parquet to the object plane through the spine's `parquet` grant or the embedded engine's `httpfs` write; the lake row attaches a DuckLake catalog whose snapshots live in a SQL database and whose data lives as Parquet on the object plane.
- Growth: a new lake format is one attach row; a new offload source is one `ATTACH` mint.
- Law: `attachPg` is read-offload only — the embedded engine reads the spine's tables without a second wire format, and no write path exists from the lane back into the OLTP transaction.
- Law: the lake is ACID over object storage with a SQL catalog — multi-table transactions, time travel, and schema evolution ride the catalog database; the object plane holds immutable Parquet, exactly the content-addressed posture the folder's object rows already enforce.
- Law: range-read Parquet is the browser's only remote source — `Olap.lakeSource` mints the presigned grant through `ObjectStore.grant` and registers it via `registerFileURL(name, url, DuckDBDataProtocol.HTTP, false)`, so the browser-analytics loop is one wired seam bounded by the grant's TTL; no service proxy re-streams rows.

```typescript
import { GetObjectCommand } from "@aws-sdk/client-s3"
import { ContentKey } from "@rasm/ts/core"
import { ObjectStore } from "../object/store.ts"

const _attach = {
  pg: (dsn: string) => `ATTACH '${dsn}' AS spine (TYPE postgres, READ_ONLY)`,
  sqlite: (path: string) => `ATTACH '${path}' AS lane (TYPE sqlite)`,
  ducklake: (catalog: string, dataPath: string) =>
    `ATTACH 'ducklake:${catalog}' AS lake (DATA_PATH '${dataPath}')`,
  httpfs: "INSTALL httpfs; LOAD httpfs;",
} as const

const _lakeSource = (db: wasm.AsyncDuckDB, name: string, key: ContentKey) =>
  Effect.flatMap(ObjectStore, (store) =>
    Effect.flatMap(
      store.grant(key, new GetObjectCommand({ Bucket: store.bucket, Key: key })),
      (grant) =>
        _try("duckdbWasm", "wire", () =>
          db.registerFileURL(name, grant.url, wasm.DuckDBDataProtocol.HTTP, false)),
    ))
```

## [5]-[CLICKHOUSE]

- Owner: the at-scale driver row — the `ClickhouseClient` Layer mints, the three members the ingestion path composes (streamed `insertQuery`, command-mode `asCommand`, typed `param` fragments), and `Olap.ingest` — the quota-governed ingestion seam every replication stream rides; per-query settings scope through the fiber.
- Packages: `@effect/sql-clickhouse` (`ClickhouseClient.layer`, `ClickhouseClient.layerConfig`, `insertQuery`, `asCommand`, `param`, `withClickhouseSettings`); `@effect/experimental` (`RateLimiter.makeWithRateLimiter`, `RateLimiter.layerStoreMemory` — the store-backed distributed limiter surviving multi-replica ingestion); `effect` (`Config`, `Redacted`).
- Entry: admitted at the composition root only where the `_engines.clickhouse.trigger` condition is real; the fact journal's high-cardinality rollups replicate into MergeTree through `Olap.ingest`, and dashboards read the cluster, never the OLTP spine.
- Growth: a new ingestion stream is one `ingest` call site over the same layer; a new settings posture is a `withClickhouseSettings` scope; a quota posture is an `_INGEST_QUOTA` override keyed per app, never a consumer wrap.
- Law: the driver extends the neutral `SqlClient`, so analytical reads ride the same `sql` DSL and typed decode as every lane — `clickhouse` is an `onDialect` arm-KEY; only ingestion and command routing reach the concrete Tag.
- Law: ingestion is load-shed at the owner — the token-bucket limiter keys by app so one tenant's replication burst cannot starve siblings, `onExceeded: "delay"` suspends instead of dropping (replication is re-runnable, never lossy by quota), and the store-backed limiter form holds across replicas; `makeWithRateLimiter` is an accessor over the `RateLimiter` service, so the quota transformer resolves the limiter from the requirement channel and the store backing composes at the root.
- Law: the cluster is correctness-adjacent — facts replicate IN, and a lost analytical row is a re-replication, never a billing defect; the journal remains the sole truth.

```typescript
import { Config, type ConfigError, type Layer } from "effect"
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

const _ingest = (app: AppIdentity.Key) =>
  <A, E, R>(work: Effect.Effect<A, E, R>) =>
    Effect.flatMap(RateLimiter.makeWithRateLimiter, (limit) =>
      limit({ ..._INGEST_QUOTA, key: `olap:ingest:${app}` })(work))
```

## [6]-[ARROW_WIRE]

- Owner: the one columnar interchange — decode and encode at the lane seams, batch streaming for bounded memory, and the two wasm ingest members that close the loop.
- Packages: `apache-arrow` (`tableFromIPC`, `tableToIPC`, `RecordBatchReader`, `Table`); `@duckdb/duckdb-wasm` (`conn.insertArrowTable`, `conn.insertArrowFromIPCStream`).
- Entry: wasm query results are already Tables; node results and ClickHouse Arrow output decode through `tableFromIPC`; the viewer's geoarrow plane consumes the same Tables downstream.
- Growth: a new engine row joins the wire by emitting or accepting IPC — no per-engine result shape is ever admitted.
- Law: one wire — an analytical result crossing any engine seam travels as Arrow; a JSON or row-object re-encoding between engines is the named defect, and the only row-shaped egress is the final consumer projection.
- Law: streams stay bounded — large interchange rides `RecordBatchReader` batch iteration lifted to `Stream`, never a whole-table buffer of unbounded cardinality.

```typescript
import { RecordBatchReader, type Table, tableFromIPC, tableToIPC } from "apache-arrow"

const _wire = {
  decode: (bytes: Uint8Array): Table => tableFromIPC(bytes),
  encode: (table: Table): Uint8Array => tableToIPC(table),
  batches: (engine: Olap.Engine, source: AsyncIterable<Uint8Array>) =>
    Stream.unwrap(
      _try(engine, "wire", () => RecordBatchReader.from(source)).pipe(
        Effect.map((reader) => Stream.fromAsyncIterable(reader, (cause) =>
          new OlapFault({ engine, reason: "wire", detail: String(cause) }))),
      )),
} as const

const Olap = {
  engines: _engines,
  node: _node,
  session: _session,
  wasm: _wasm,
  query: _query,
  attach: _attach,
  lakeSource: _lakeSource,
  clickhouse: _clickhouse,
  ingest: _ingest,
  wire: _wire,
  Fault: OlapFault,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Olap, OlapFault }
```
