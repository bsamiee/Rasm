# [DATA_OLAP]

Engine rows own analytical throughput and no durability claim: DuckDB accelerates node and browser under one leased session family, pgDuckDB embeds it in live Postgres, ClickHouse enters past its trigger, and FLIGHT carries every remote columnar end. RESIDENCE_ROWS own the column-token alphabet and parameterize durable planes — extensions, credentials, attachments, the identity fill, and the schemas `Board.Query` renders. ARROW_WIRE lands declared columns and holds each codec pair as ONE interchange — IPC in memory, Flight on the wire, Parquet at rest — while PROFILE folds escalation evidence.

Settled composition: `core/observe/board#QUERY` owns the query algebra and its render-target axis, so this page fills `Board.Query.Residence`, mints `Board.Query.Target`, and renders none. This page PUBLISHES the wide-event column roster and `iac/operate/observe#CHART_ROWS` plants its clickhouse DDL against it; the lake plants its own through `Olap.mount`. `Convention.identity` joins signals to the journal's `app`/`tenant` columns, `lane/postgres#PROFILE_HARVEST` owns the `Pg.Profile` each profile arm fills, and `object/store#STORE` owns this lane's object coordinates.

## [01]-[INDEX]

- [02]-[ENGINE_ROWS] — `_engines` prices guarantee, storage posture, ceiling, and escalation trigger per engine.
- [03]-[EMBEDDED] — `Olap.node` and `Olap.wasm` lease one session family under `Olap.read`, and `_SOURCES` routes each lane source into a relation.
- [04]-[RESIDENCE_ROWS] — `_COLUMN` owns the column-token alphabet and `_RESIDENCES` each durable plane's extensions, credentials, attachments, schemas, and fill.
- [05]-[CLICKHOUSE] — `Olap.ingest` and `Olap.wide` bind the at-scale driver behind the lane's one fault rail.
- [06]-[FLIGHT] — `Olap.flown` carries every Flight SQL modality over one sealed remote coordinate.
- [07]-[PROFILE] — `Olap.profile` harvests engine profiles and `Olap.armed` grades the escalation evidence.
- [08]-[ARROW_WIRE] — `Olap.wire` and `Olap.lake` own the one columnar interchange in memory, on the wire, and at rest; `Olap.wire.roster` lands declared columns.

## [02]-[ENGINE_ROWS]

- Owner: the `_engines` anchor — one row per engine profile carrying its guarantee, storage posture, and scale ceiling; the escalation trigger is data on the row, so an admission argument reads the table instead of relitigating the lane.
- Packages: `effect` (`Record`) — the rows are decision facts and the contract check is their only machinery.
- Growth: a new engine candidate is one row measured against the same columns, and its meter set, fault labelling, and escalation text follow from the key; a row whose trigger never fires in production is a prune candidate at the next census.
- Law: `duckdbNode` is the default — vectorized execution, out-of-core spill, zero-copy Parquet/CSV/JSON/Arrow, single-file ACID under a single writer; embedded in-process, so the ceiling is one node and one concurrent writer.
- Law: `duckdbWasm` is the client-side row — the browser queries remote Parquet by HTTP range through presigned grants, results land as Arrow, and compute leaves the service bill entirely; single-threaded by default, CORS-bound, an accelerator over server-minted data.
- Law: `pgDuckdb` is the analytics-in-OLTP row — the spine's `analytics` grant embeds the same engine inside live Postgres for workloads adjacent to transactional data; no second system, PG durability, bounded by the single PG host; the grant is `lane/postgres.md`'s row and this lane only names the boundary.
- Law: `clickhouse` is admitted ONLY past the trigger — concurrent high-throughput ingestion, multi-node scale, or high-cardinality real-time serving; below it the embedded rows own the workload, and admitting the cluster early is the named operational waste.
- Law: `flight` is the one door to a columnar engine this estate does not operate — the server stays opaque behind the wire, so the row prices the wire and never the engine, and a second query transport beside it re-derives admission, auth, and typing at every end.
- Law: the engine key is threaded, never assumed — every session, fault, and meter carries the row it came from, so an engine's evidence is engine-labelled by construction and a hardcoded engine literal below the row table is the deleted form.
- Law: engine rows are SELECTED by a deployment's own descriptor and declare none of it — this table prices guarantee, storage, ceiling, and trigger and spells no axis literal, because a roster coined at a leaf lane re-anchors a vocabulary the branch answers once; a composition root maps its own profile onto a row rather than reading one off it.
- Law: TENANCY no embedded row decides — the engine holds one file and no session GUC, so isolation lives on `tenant`'s write path and each residence's own column, and an embedded row answering it states a guess about a plane it never scopes.

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
  flight: {
    guarantee: "Arrow Flight SQL against an opaque remote engine — the server's guarantee, never this lane's",
    storage: "the remote engine's",
    ceiling: "the remote deployment's; this lane holds none",
    trigger: "a columnar engine operated outside this estate — every columnar query end rides this one wire",
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly guarantee: string
  readonly storage: string
  readonly ceiling: string
  readonly trigger: string
}>

declare namespace Olap {
  type Engine = keyof typeof _engines
}
```

## [03]-[EMBEDDED]

- Owner: the embedded driver family — `_DRIVERS` rows carrying each driver's bind vocabulary, bounded, windowed, and drained members, its engine-side cancel, its permit ceiling, its own result grain, and the residency its registry admits; `Olap.node` and `Olap.wasm` mint one scoped `Olap.Handle` apiece whose lease carries the pool's own eviction seam beside the connection, `Olap.read` is the ONE leased-session entry whose `Rows`, `Window`, and `Drain` cases select geometry under the owner governor, and the source-admission half — `_SOURCES` routing each admitted lane source into a relation, `Olap.source` pumping a `scan` row into the handle's own registry behind a registered table function over a residency `Olap.wire.roster` derived from one column declaration, `Olap.lakeSource` registering the browser's file residencies on the leased worker.
- Packages: DuckDB, Arrow, and Effect runtime surfaces; `@duckdb/node-api` (`DuckDBTableFunction`, `DuckDBType`) and `effect` (`MutableHashMap`) carry the source registry; core `Fault.Budget`, `Convention`, and `Fault.Class`.
- Entry: a service composes `Olap.node` once per database coordinate and the browser shell composes `Olap.wasm` over self-hosted bundles at boot; both hand a `handle.lease` to each analytical unit of work, and every statement, file registration, and Arrow ingest rides that one session.
- Output: each engine answers its own grain off its driver row — the node driver materializes row-object projections, the worker hands Arrow `Table` and `RecordBatch` values straight through to the viewer with no re-encoding.
- Growth: an embedded driver is one `_DRIVERS` row answering three execution members, its cancel, its permit ceiling, and one grain pair; a read geometry is one `Olap.Read` case with its overload and per-driver member; resilience is a `_GOVERNOR` override; a registry residency is one `_registered` arm; an admitted source is one `_SOURCES` row whose route decides between minting a scan and naming the statement that already carries it, and its pumped shape is one `[name, token]` column declaration handed to `Olap.wire.roster`; a new measure is one `_meter` row naming a census metric and its axis, whose whole per-engine set follows.
- Law: lifecycle is `acquireRelease` under `Scope` — instance, worker, and every connection release deterministically; an unscoped engine handle is unspellable because the constructors return scoped effects.
- Law: every promise lifts through `_try`; `OlapFault` rows close through `Fault.Class.family` and route recovery by reason.
- Law: both embedded engines share ONE session family, so the bulkhead semaphore, the budget bracket, the replay rule, and the meter fan reach the browser lane exactly as they reach the node lane; a second ungoverned read entry beside `Olap.read` leaves a single-threaded worker taking unbounded concurrent statements and taps no engine-labelled measure at all.
- Law: the session carries its engine, so every fault the session mints and every meter it taps is engine-labelled by construction; an operation reading the engine from an enclosing literal mislabels every row the moment a second engine reaches the same operation.
- Law: driver divergence is a row, never an arm — result grain, bind vocabulary, the three execution members, the engine-side cancel, and the permit ceiling ride `_DRIVERS`, so `Olap.read` dispatches on the session's own engine key and no geometry branches on a driver name.
- Law: a budget releases the FIBER and an engine learns nothing from that, so every abandoned unit stops at its driver — `_cancelled` seats the row's own cancel inside the retry, and a drain cancels on any non-success exit through `Stream.ensuringWith` so the idle bound reaches the scan rather than leaving it running behind a consumer that is gone; a lane bounding time without cancelling leaves a stalled statement holding its native thread, its spill files, and the connection it borrowed for the process's life.
- Law: a refusal carrying a MEASURED wait publishes it as `Fault.Class.After` — the core carrier `Fault.Class.statedOf` reads back and `Fault.Budget.schedule` re-seats its row base from, so a lane-local optional keeps the number unreadable by the one policy owner that spends it.
- Law: `OlapFault.at` is the mint every refusal that measured no window takes, so absence is spelled once at the owner rather than at each raise, and the explicit construction is reserved for the rail that published a span.
- Law: the read curve compiles per REFUSAL, never at module scope — the gate is the core lattice and the base is the refusal's own measured wait, so a quota that named its window is waited exactly and every other retryable class spends the compiled `lease` curve unchanged.
- Law: a unit answering on a BOUND evicts its leased connection — a driver refusal proves the connection alive and hands it back, while a connection destroyed mid-lease settles NOTHING and the bound is the only arrival it can produce, so `Pool` (which publishes `invalidate` and no health check) otherwise serves that same corpse to every later lease for the process's life; `_ABANDONED` rosters the three bound details so the release arm reads this lane's own sentinel rather than a driver string it does not own, and one needless reconnect against a merely slow statement is the bounded price of that certainty.
- Law: the engine-side cancel LEAVES the connection leasable — an interrupted statement refuses on its own rail and the same connection answers the next one, so `_cancelled` stops work without spending a pool slot and eviction stays the bound path's alone.
- Law: permit COUNT is per-driver, never one lane scalar — the node engine runs a thread pool and the worker is single-threaded by its own ceiling row, so eight permits at the browser only queue statements behind one worker while holding eight connections; a shared count contradicts the ceiling `_engines` already publishes.
- Law: engine-side file registration is a residency ROW — a presigned grant, a picked local file, and bytes in hand each name their own member and protocol, and `registerFileBuffer` takes no protocol at all, so one shared call passing a protocol value is unspellable; registration is scoped and drops its name on release, because a per-view registration that never drops grows the worker's registry for the tab's whole life and pins every grant it named.
- Law: range-read Parquet is the browser's only REMOTE source — `Olap.lakeSource` mints the presigned grant through `ObjectStore.grant` and registers it on the leased session's own engine bindings, so the browser-analytics loop is one wired seam bounded by the grant's TTL, rides the same permit and budget every worker statement rides, and no service proxy re-streams rows; a picked file and bytes in hand are LOCAL residencies on that same entry, so the engine reads every source the at-rest codec already decodes.
- Law: a lane source becomes a RELATION the planner owns, and the route is data — a `scan` row mints a registered table function over content this lane pre-pumps, a `sql` row states the source is already expressible and mints nothing, and an admitted source carrying no `_SOURCES` row is a missing adjudication rather than a missing feature; the journal window rides `Olap.attach.pg` because a `READ_ONLY` ATTACH pushes predicate AND projection into the spine where a scan first pumps the whole window across the boundary, and the lake rides `read_parquet` because the parquet reader is statically linked and a local object loads no extension at all.
- Law: a registered scan is admissible ONLY over content resident before the query binds — a chunk of zero rows is the scan's one terminator and it is OVERLOADED, meaning both exhausted and nothing-yet, so a source pulled lazily inside the scan answers a TRUNCATED relation under a success exit; a returned promise is never awaited and the scan ends empty, a busy-spin deadlocks the loop that delivers the rows, and `Atomics.wait` against a worker-fed `SharedArrayBuffer` is REFUSED BY NAME because it blocks every fiber on the thread and leaves `_GOVERNOR`'s fiber release and `_paged`'s idle bound unenforceable for its whole hold.
- Law: the scan runs on the node MAIN THREAD, never a scanning thread — re-entry is strictly serial and `localInitFunction` fires once even at `threads=4`, so per-scan state rides the engine's own init slot and the pump's declared ceiling is what keeps a resident source from starving the runtime it shares; `interrupt()` reaches a running scan, so an abandoned unit stops at the engine exactly as every other statement does.
- Law: registration is INSTANCE-scoped and PERMANENT — the entry lands in the instance catalog, every sibling connection reads it, it survives the connection that took it, and no drop exists, so a name is minted once per SOURCE per handle and a per-lease name leaks a catalog entry for the process's life; `Olap.Handle` therefore widens by the ONE registry field and `Olap.Session` carries nothing new, because the registry's lifetime is the instance's.
- Law: re-registering a name is a SILENT no-op that keeps the FIRST registration's bind data serving, so the bind and main functions resolve their content out of the handle registry BY NAME and capture none — which makes every mint of the function interchangeable and turns a re-pump into a content swap the planner reads on its next bind; a closure carrying the content lets a stale registration answer forever with no error anywhere.
- Law: projection pushdown is MANDATORY and predicate pushdown is ABSENT — the opt-in is what narrows the output chunk, so a roster-width write without it refuses inside the engine's own converter, while a `WHERE` clause scans the whole residency and only `LIMIT` terminates early; that asymmetry is why the row's ceiling is the real bound on a filtered read, and why the exact cardinality a resident source can declare is spent rather than left to the planner's guess.
- Law: a residency is DERIVED from ONE column declaration and never assembled — `Olap.wire.roster` mints the bound roster, its engine types, the Arrow schema, the builder bank, and the scan's own vectors off the same `[name, token]` list, so a producer hands ROWS and no site pairs a column name with a vector by hand; the arity the bind seam declares exact is the count that mint MEASURED off those rows, where three readers each re-deriving a length off whichever vector they reached first state an arity nobody measured and disagree the moment one column is pumped short.
- Law: reader ordinals ARE the declaration's own indexes — `getColumnIndexes()` answers positions in the bound roster and the residency carries that same order, so a projection remaps by index and no name-to-ordinal map is minted beside the order the declaration already fixes; a plan asking for a different PHYSICAL order gets that remap, never a second declaration.
- Law: a scan's refusal is `setError` and the arm is chosen for ATTRIBUTION — a JS throw crossing the same seam folds to the same `Invalid Input Error`, so the difference is which text the caller reads back, and the lane spells its own source coordinate rather than a stack the engine reprints.
- Law: no wasm cell carries a scan — that build ships no table-function surface at all, so the browser's answer is a file or Arrow registration route or it is `None`, and the pre-pump the scan route already demands is exactly what makes one engine-side Arrow copy an equal answer rather than a degradation.
- Law: `connection.createAppender` is the load-into-a-plane counterparty and stays REFUSED — the object fill crosses no rows into this runtime at all (`Olap.absorb` scans Parquet inside the engine), and the registry fill keys every cell by COLUMN NAME off `_POINTS` where the appender only ever appends by position and only on the node driver, so admitting it re-imports the positional hazard that roster deletes, forks the fill by driver, and buys nothing on the one path that carries volume.
- Law: instrument construction belongs to `Convention.mount` alone — the census row carries kind, description, UCUM unit, and bucket ladder, so `_tapped` names a metric and an axis and mints nothing; a locally-declared boundary set is the deleted form because it silently disagrees with the ladder the row publishes and drops the unit tag the OTLP bridge reads its descriptor unit from.
- Law: every tap fans the same row across every engine key, so an operation reading a measure indexes by the engine it already carries and five engines across four measures never become twenty declarations.
- Law: `_governed` brackets every unit of work a session owns — an embedded statement, a browser file registration, an Arrow ingest — behind one permit and one budget, so `_Governed`'s two columns are the whole vocabulary a governed call answers. That budget is TOTAL over the unit: the deadline seats above the replay stack, so a governed read releases its fiber on the one bound the row declares rather than on that bound multiplied by every attempt the curve admits.
- Law: replay admits `access: "read"` alone, so an executing write, extension mutation, credential mint, or profile harvest stays one-shot; the governor values are one policy row.
- Law: transience derives from `Fault.Class.retryable`; the same projection gates retries and records replay telemetry.
- Law: each reason declares its own subject and renders its own sentence, and the raise carries ONE `case` payload beside the window — a free `detail` field and a hand-written message template both delete at the class.
- Law: connection POPULATION rides `Pool` and statement CONCURRENCY rides the semaphore — the pool bounds how many connections exist and hands a scoped lease back for reuse where the gate bounds how many statements run at once, so a lease costs a checkout rather than a fresh `connect`, a burst cannot open unbounded connections against a single-file engine, and both bounds read the driver's own `sessions` column.
- Law: connection-local state never escapes its lease — the profile bracket sets and clears its own pragma inside one `acquireUseRelease`, so a checked-in connection carries no diagnosis posture and a pooled reuse inherits none.
- Law: the drain path never re-buffers — `DuckDBResult.yieldRowObjects()` yields one native data chunk at a time and retains none while the worker's own reader pulls one `RecordBatch` at a time, and `Stream.acquireRelease` holds the session permit until downstream termination; a `DuckDBResultReader` is rejected here because its private chunk roster accumulates the full result even when `readUntil` advances incrementally. Acquisition retries before the first emission; an iteration fault fails the stream without replay because a partial-output retry duplicates rows.
- Law: a drain bounds on the gap between chunks, exactly as the remote wire bounds the gap between frames — the permit it holds is one of a fixed few and the governor budget reaches only its acquisition, so an engine that stops emitting mid-result strands that permit for the process's life; a total-elapsed bound is refused on the same ground the remote lane refuses it, because a legitimate out-of-core scan outlives any whole-stream budget.
- Law: a window is exact at both drivers — `readUntil` overshoots by chunk granularity and the worker publishes no row cutoff at all, so the node arm slices its reader and the worker arm pulls batches to the target and slices the assembled `Table`; a caller reading the driver's own overshoot receives rows it never asked for.
- Law: the worker binds through a prepared plan alone — a statement carrying values opens its plan, executes, and closes it on every exit including interruption, while admission text stays bind-free because a multi-statement `INSTALL`/`LOAD` prepares nowhere; splicing a value into browser SQL is the injection surface the node lane already refuses.
- Law: every DuckDB lambda this lane mints spells the `lambda x, i:` keyword form — the installed engine warns on the single-arrow `->` spelling and stops parsing it at its next release, and the `lambda_syntax` revert is a GLOBAL engine setting, so leaning on the arrow binds every session on the instance to a dying dialect to keep one fragment compiling; the keyword form is the only spelling a statement on this page assumes, and the residence distribution reads are its one site.
- Law: extension admission is a statement — `INSTALL`/`LOAD` for the whole `_extensions` roster runs through the `Rows` geometry; a load failure refuses the capability as a typed `extension` fault, never crashes the lane.
- Law: bundles self-host beside the app shell — `selectBundle` over owned artifact coordinates; a CDN bundle load is rejected by the deployment's content policy.
- Exemption: the scan's bind, init, and main bodies are platform-forced callbacks the engine re-enters — the chunk's row count is a setter, the cursor advances in the engine's own per-scan slot, and both are statements no rail can carry, because the engine calls them synchronously and reads their effect rather than a returned value.
- Boundary: `Olap.wasm` names the DuckDB BUILD this lane composes, never an ISOLATION coordinate — this lane's own code runs on the fiber runtime whichever driver answers, and an embedded wasm-built engine is a dependency's implementation whose worker is its internal concurrency, invisible at the consumption boundary; which driver row a root composes follows its deployment shape, and the caller's isolation stays whatever its own descriptor states.
- Boundary: `_try` and the worker acquire are the marked promise kernels — typed bind values cross without a cast, and the ambient `Worker` construction lives inside the acquire arm; its thrown missing-worker guard is caught by its own `tryPromise` and folds to the `bundle` reason.

```typescript
import { Array, Cause, Data, Duration, Effect, Exit, Match, Metric, MutableHashMap, Option, pipe, Pool, Record, Schedule, Schema, type Scope, Stream } from "effect"
import { DuckDBInstance, DuckDBTableFunction, type DuckDBConnection, type DuckDBType, type DuckDBValue } from "@duckdb/node-api"
import { GetObjectCommand } from "@aws-sdk/client-s3"
import * as wasm from "@duckdb/duckdb-wasm"
import { RecordBatch, Table } from "apache-arrow"
import { Convention, type Digest, Fault } from "@rasm/core"
import { ObjectStore } from "../object/store.ts"

const _Engine = Schema.Literal(...Record.keys(_engines))

const _Subject = Schema.Struct({ engine: _Engine, detail: Schema.String })

const _family = Fault.Class.family(["acquire", "query", "extension", "secret", "bundle", "object", "wire"] as const, {
  acquire: Fault.Class.row({
    class: "unavailable",
    leg: "session",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} handed out no session — ${detail}`,
  }),
  query: Fault.Class.row({
    class: "unavailable",
    leg: "statement",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} refused the statement — ${detail}`,
  }),
  extension: Fault.Class.row({
    class: "absent",
    leg: "statement",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} loaded no such extension — ${detail}`,
  }),
  secret: Fault.Class.row({
    class: "denied",
    leg: "statement",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} refused the credential — ${detail}`,
  }),
  bundle: Fault.Class.row({
    class: "absent",
    leg: "session",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} selected no runnable bundle — ${detail}`,
  }),
  object: Fault.Class.row({
    class: "unavailable",
    leg: "wire",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} did not land the object — ${detail}`,
  }),
  wire: Fault.Class.row({
    class: "malformed",
    leg: "wire",
    detail: _Subject,
    render: ({ engine, detail }) => `${engine} carried an unreadable frame — ${detail}`,
  }),
})

class OlapFault extends Schema.TaggedError<OlapFault>()("OlapFault", {
  case: _family.payload,
  after: Fault.Class.After,
}) {
  static readonly at = (engine: Olap.Engine, reason: (typeof _family.kinds)[number], detail: string): OlapFault =>
    new OlapFault({ case: { reason, engine, detail }, after: Option.none() })
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

type _Tap<N extends Convention.MetricName> = Convention.Mounted<N> extends Metric.Metric<infer _Type, infer In, infer _Out> ? In
  : never

const _tapped = <N extends Convention.MetricName>(metric: N, axis: Convention.Key) =>
  Record.map(_engines, (_row, engine: Olap.Engine) =>
    pipe(
      Metric.tagged(Convention.mount(metric), axis, engine),
      (held) => (value: _Tap<N>): Effect.Effect<void> => Metric.update(held, value),
    ))

const _meter = {
  deferred: _tapped(Convention.metric.olapDeferred, Convention.rasm.olapEngine),
  profiled: _tapped(Convention.metric.profileDuration, Convention.rasm.profileEngine),
  retried: _tapped(Convention.metric.olapRetried, Convention.rasm.olapEngine),
  waited: _tapped(Convention.metric.olapWait, Convention.rasm.olapEngine),
} as const

const _GOVERNOR = { budget: Fault.Budget.at("lease").total } as const satisfies { readonly budget: Duration.Duration }

const _ABANDONED = { budget: "<budget>", idle: "<idle-budget>", profile: "<profile-budget>" } as const

const _abandoned = (fault: OlapFault): boolean => Array.contains(Record.values(_ABANDONED), fault.case.detail)

type _Governed = {
  readonly access: "read" | "write"
  readonly fault: OlapFault["case"]["reason"]
}

type _Statement = _Governed & {
  readonly sql: string
  readonly values?: ReadonlyArray<DuckDBValue>
  readonly fault: "query" | "extension" | "secret"
}

type OlapRead = Data.TaggedEnum<{
  Rows: _Statement
  Window: _Statement & { readonly take: number }
  Drain: _Statement
}>

const _Read = Data.taggedEnum<OlapRead>()

const _fault = (engine: Olap.Engine, reason: OlapFault["case"]["reason"]) => (cause: unknown): OlapFault =>
  OlapFault.at(engine, reason, String(cause))

const _try = <A>(engine: Olap.Engine, reason: OlapFault["case"]["reason"], run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: _fault(engine, reason) })

const _resilient = <A, R>(
  engine: Olap.Engine,
  governed: _Governed,
  work: Effect.Effect<A, OlapFault, R>,
): Effect.Effect<A, OlapFault, R> => {
  const metered = Effect.tapError(work, (fault: OlapFault) =>
    Fault.Class.retryable(fault) ? _meter.retried[engine](1) : Effect.void)
  return (governed.access === "read"
    ? Effect.catchIf(metered, Fault.Class.retryable, (fault) =>
      Effect.retry(metered, Fault.Budget.schedule("lease", Fault.Class.retryable, fault.after)))
    : work).pipe(
      Effect.timeoutFail({
        duration: _GOVERNOR.budget,
        onTimeout: () => OlapFault.at(engine, governed.fault, _ABANDONED.budget),
      }),
    )
}

const _governed = <E extends Olap.Embedded>(session: Olap.Session<E>) =>
  <A, R>(governed: _Governed, work: Effect.Effect<A, OlapFault, R>): Effect.Effect<A, OlapFault, R> =>
    Effect.acquireUseRelease(
      Effect.tap(Effect.timed(session.gate.take(1)), ([span]) => _meter.waited[session.engine](span)),
      () =>
        _resilient(session.engine, governed, _cancelled(session, work)).pipe(
          Effect.tapError((fault) => _abandoned(fault) ? session.evict : Effect.void),
        ),
      () => session.gate.release(1),
    )

const _values = (values: ReadonlyArray<DuckDBValue> | undefined): Array<DuckDBValue> | undefined =>
  values === undefined ? undefined : Array.fromIterable(values)

type _Driver = {
  duckdbNode: {
    readonly bind: DuckDBValue
    readonly bounded: ReadonlyArray<Olap.Row>
    readonly connection: DuckDBConnection
    readonly element: Olap.Row
    readonly resident: Olap.Resident
  }
  duckdbWasm: {
    readonly bind: Olap.Scalar
    readonly bounded: Table
    readonly connection: wasm.AsyncDuckDBConnection
    readonly element: RecordBatch
    readonly resident: never
  }
}

declare namespace Olap {
  type Embedded = keyof _Driver
  type Scalar = Extract<DuckDBValue, null | boolean | number | bigint | string>
  type Bind<E extends Embedded> = _Driver[E]["bind"]
  type Bounded<E extends Embedded> = _Driver[E]["bounded"]
  type Element<E extends Embedded> = _Driver[E]["element"]
  type Session<E extends Embedded = Embedded> = {
    readonly connection: _Driver[E]["connection"]
    readonly engine: E
    readonly evict: Effect.Effect<void>
    readonly gate: Effect.Semaphore
  }
  type Handle<E extends Embedded = Embedded> = {
    readonly engine: E
    readonly lease: Effect.Effect<Session<E>, OlapFault, Scope.Scope>
    readonly sources: Registry<E>
  }
  type Row = Record.ReadonlyRecord<string, unknown>
  type Bound<E extends Embedded, K extends OlapRead["_tag"]> = Extract<OlapRead, { readonly _tag: K }> & { readonly values?: ReadonlyArray<Bind<E>> }
  type _Drivers<K extends Engine = Embedded> = K
}

const _worker = <A>(
  session: Olap.Session<"duckdbWasm">,
  op: _Statement,
  bare: (connection: wasm.AsyncDuckDBConnection, sql: string) => Promise<A>,
  bound: (plan: wasm.AsyncPreparedStatement, values: ReadonlyArray<DuckDBValue>) => Promise<A>,
): Effect.Effect<A, OlapFault, Scope.Scope> =>
  op.values === undefined
    ? _try(session.engine, op.fault, () => bare(session.connection, op.sql))
    : Effect.flatMap(
      Effect.acquireRelease(
        _try(session.engine, op.fault, () => session.connection.prepare(op.sql)),
        (plan) => Effect.orDie(_try(session.engine, op.fault, () => plan.close())),
      ),
      (plan) => _try(session.engine, op.fault, () => bound(plan, op.values ?? [])),
    )

const _batched = (
  session: Olap.Session<"duckdbWasm">,
  op: _Statement,
): Effect.Effect<Stream.Stream<RecordBatch, OlapFault>, OlapFault, Scope.Scope> =>
  Effect.map(
    _worker(session, op, (connection, sql) => connection.send(sql), (plan, values) => plan.send(...values)),
    (reader) => Stream.fromAsyncIterable(reader, _fault(session.engine, "query")),
  )

const _DRIVERS: {
  readonly [E in Olap.Embedded]: {
    readonly bounded: (session: Olap.Session<E>, op: _Statement) => Effect.Effect<Olap.Bounded<E>, OlapFault, Scope.Scope>
    readonly cancel: (session: Olap.Session<E>) => Effect.Effect<void>
    readonly drained: (session: Olap.Session<E>, op: _Statement) => Effect.Effect<Stream.Stream<Olap.Element<E>, OlapFault>, OlapFault, Scope.Scope>
    readonly sessions: number
    readonly windowed: (
      session: Olap.Session<E>,
      op: _Statement & { readonly take: number },
    ) => Effect.Effect<Olap.Bounded<E>, OlapFault, Scope.Scope>
  }
} = {
  duckdbNode: {
    bounded: (session, op) =>
      Effect.map(_try(session.engine, op.fault, () => session.connection.runAndReadAll(op.sql, _values(op.values))), (reader) => reader.getRowObjects()),
    cancel: (session) => Effect.ignore(Effect.try(() => session.connection.interrupt())),
    drained: (session, op) =>
      Effect.map(
        _try(session.engine, op.fault, () => session.connection.stream(op.sql, _values(op.values))),
        (result) => Stream.mapConcat(Stream.fromAsyncIterable(result.yieldRowObjects(), _fault(session.engine, "query")), (rows) => rows),
      ),
    windowed: (session, op) =>
      Effect.map(
        _try(session.engine, op.fault, () => session.connection.streamAndReadUntil(op.sql, op.take, _values(op.values))),
        (reader) => Array.take(reader.getRowObjects(), op.take),
      ),
    sessions: 8,
  },
  duckdbWasm: {
    bounded: (session, op) => _worker(session, op, (connection, sql) => connection.query(sql), (plan, values) => plan.query(...values)),
    cancel: (session) => Effect.ignore(_try(session.engine, "query", () => session.connection.cancelSent())),
    drained: _batched,
    windowed: (session, op) =>
      Effect.map(
        Effect.flatMap(_batched(session, op), (batches) =>
          Stream.runFold(
            Stream.takeUntil(
              Stream.mapAccum(batches, 0, (held, batch) => [held + batch.numRows, { batch, taken: held + batch.numRows }] as const),
              ({ taken }) => taken >= op.take,
            ),
            Array.empty<RecordBatch>(),
            (held, { batch }) => Array.append(held, batch),
          )),
        (batches) => new Table(batches).slice(0, op.take),
      ),
    sessions: 1,
  },
}

const _cancelled = <E extends Olap.Embedded, A, R>(
  session: Olap.Session<E>,
  work: Effect.Effect<A, OlapFault, R>,
): Effect.Effect<A, OlapFault, R> => Effect.onInterrupt(work, () => _DRIVERS[session.engine].cancel(session))

const _node = (
  path: string,
  config?: Record.ReadonlyRecord<string, string>,
): Effect.Effect<Olap.Handle<"duckdbNode">, OlapFault, Scope.Scope> =>
  Effect.gen(function* () {
    const gate = yield* Effect.makeSemaphore(_DRIVERS.duckdbNode.sessions)
    const instance = yield* Effect.acquireRelease(
      _try("duckdbNode", "acquire", () => DuckDBInstance.create(path, config)),
      (held) => Effect.sync(() => held.closeSync()),
    )
    const pool = yield* Pool.make({
      acquire: Effect.acquireRelease(
        _try("duckdbNode", "acquire", () => instance.connect()),
        (held) => Effect.sync(() => held.disconnectSync()),
      ),
      size: _DRIVERS.duckdbNode.sessions,
    })
    return {
      engine: "duckdbNode" as const,
      sources: MutableHashMap.empty<string, Olap.Resident>(),
      lease: Effect.map(pool.get, (connection) => ({
        connection,
        engine: "duckdbNode" as const,
        evict: Effect.scoped(Pool.invalidate(pool, connection)),
        gate,
      })),
    }
  })

const _wasm = (bundles: wasm.DuckDBBundles): Effect.Effect<Olap.Handle<"duckdbWasm">, OlapFault, Scope.Scope> =>
  Effect.gen(function* () {
    const gate = yield* Effect.makeSemaphore(_DRIVERS.duckdbWasm.sessions)
    const db = yield* Effect.acquireRelease(
      _try("duckdbWasm", "bundle", async () => {
        const bundle = await wasm.selectBundle(bundles)
        if (bundle.mainWorker === null) throw new Error("<bundle:no-worker>")
        const worker = new Worker(bundle.mainWorker)
        const held = new wasm.AsyncDuckDB(new wasm.ConsoleLogger(), worker)
        await held.instantiate(bundle.mainModule, bundle.pthreadWorker)
        return held
      }),
      (held) => Effect.promise(() => held.terminate()),
    )
    const pool = yield* Pool.make({
      acquire: Effect.acquireRelease(
        _try("duckdbWasm", "acquire", () => db.connect()),
        (held) => Effect.orDie(_try("duckdbWasm", "acquire", () => held.close())),
      ),
      size: _DRIVERS.duckdbWasm.sessions,
    })
    return {
      engine: "duckdbWasm" as const,
      sources: MutableHashMap.empty<string, never>(),
      lease: Effect.map(pool.get, (connection) => ({
        connection,
        engine: "duckdbWasm" as const,
        evict: Effect.scoped(Pool.invalidate(pool, connection)),
        gate,
      })),
    }
  })

const _paged = <E extends Olap.Embedded>(session: Olap.Session<E>, op: _Statement): Stream.Stream<Olap.Element<E>, OlapFault> =>
  Stream.ensuringWith(
    Stream.timeoutFail(
      Stream.flatMap(
        Stream.acquireRelease(session.gate.take(1), () => session.gate.release(1)),
        () =>
          Stream.unwrapScoped(
            _resilient(session.engine, op, _cancelled(session, _DRIVERS[session.engine].drained(session, op))),
          ),
      ),
      () => OlapFault.at(session.engine, op.fault, _ABANDONED.idle),
      _GOVERNOR.budget,
    ),
    Exit.match({
      onFailure: (cause) =>
        Effect.zipRight(
          _DRIVERS[session.engine].cancel(session),
          Option.exists(Cause.failureOption(cause), _abandoned) ? session.evict : Effect.void,
        ),
      onSuccess: () => Effect.void,
    }),
  )

function _read<E extends Olap.Embedded>(session: Olap.Session<E>, op: Olap.Bound<E, "Rows" | "Window">): Effect.Effect<Olap.Bounded<E>, OlapFault>
function _read<E extends Olap.Embedded>(session: Olap.Session<E>, op: Olap.Bound<E, "Drain">): Stream.Stream<Olap.Element<E>, OlapFault>
function _read<E extends Olap.Embedded>(
  session: Olap.Session<E>,
  op: OlapRead,
): Effect.Effect<Olap.Bounded<E>, OlapFault> | Stream.Stream<Olap.Element<E>, OlapFault> {
  return _Read.$is("Drain")(op)
    ? _paged(session, op)
    : Effect.scoped(_governed(session)(
      op,
      _Read.$is("Window")(op) ? _DRIVERS[session.engine].windowed(session, op) : _DRIVERS[session.engine].bounded(session, op),
    ))
}

// --- [SOURCE_ADMISSION]

const _SOURCES = {
  flight: {
    ceiling: 2_097_152,
    refuses: "an unbounded fan — it rides `Olap.flown` into `Olap.wire` bytes and re-enters as a lake object",
    route: "scan",
    via: "`Olap.flown` `Fetch` drained whole before the pump, so the endpoint fan is resident at bind",
    wasm: Option.some("`Olap.wire.feed` — one engine-side copy of content the scan route already required resident"),
  },
  objects: {
    ceiling: 1_048_576,
    refuses: "a listing paged from inside the scan — a lazy page read answers a truncated relation as success",
    route: "scan",
    via: "the object plane's own listing, drained whole before the pump",
    wasm: Option.some("`Olap.wire.feed` — the listing crosses as one Arrow table the worker holds"),
  },
  journal: {
    ceiling: false,
    refuses: "a re-implementation of `postgres_scanner` on this side — it pumps the whole window across the boundary",
    route: "sql",
    via: "`Olap.attach.pg` — a `READ_ONLY` attach over a minted secret pushes predicate AND projection into the spine",
    wasm: Option.none(),
  },
  lake: {
    ceiling: false,
    refuses: "a scan over decoded batches — it pays the codec twice and strands every statistic a pruning plan reads",
    route: "sql",
    via: "`read_parquet` under `Olap.attach.object`'s scope; the reader is statically linked, so a local object loads nothing",
    wasm: Option.some("`Olap.lakeSource` — `registerFileURL` over the presigned grant under `DuckDBDataProtocol.HTTP`"),
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly ceiling: number | false
  readonly refuses: string
  readonly route: "scan" | "sql"
  readonly via: string
  readonly wasm: Option.Option<string>
}>

const _Token = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9_]*$/), Schema.brand("OlapToken"))

const _SCAN = { chunk: 2048 } as const

declare namespace Olap {
  type File = { readonly blob: Blob } | { readonly key: Digest.Key<"content"> } | { readonly octets: Uint8Array }
  type Resident = {
    readonly columns: Array.NonEmptyReadonlyArray<{
      readonly name: string
      readonly type: DuckDBType
      readonly vector: ReadonlyArray<DuckDBValue>
    }>
    readonly count: number
  }
  type Registry<E extends Embedded = Embedded> = MutableHashMap.MutableHashMap<string, _Driver[E]["resident"]>
  type Source = keyof typeof _SOURCES
  type Scanned = { readonly [K in Source]: (typeof _SOURCES)[K]["route"] extends "scan" ? K : never }[Source]
}

const _projected = (registry: Olap.Registry<"duckdbNode">, name: string, roster: ReadonlyArray<number>) =>
  Option.flatMap(MutableHashMap.get(registry, name), (resident) =>
    Option.map(
      Option.all(Array.map(roster, (bound) => Array.get(resident.columns, bound))),
      (columns) => ({ columns, count: resident.count }),
    ))

const _scanned = (registry: Olap.Registry<"duckdbNode">, name: string): DuckDBTableFunction =>
  DuckDBTableFunction.create({
    name,
    supportsProjectionPushdown: true,
    bindFunction: (bind) =>
      Option.match(MutableHashMap.get(registry, name), {
        onNone: () => bind.setError(`<source:${name}>`),
        onSome: (resident) => {
          Array.forEach(resident.columns, (column) => bind.addResultColumn(column.name, column.type))
          bind.setCardinality(resident.count, true)
        },
      }),
    initFunction: (init) => init.setInitData({ offset: 0, roster: init.getColumnIndexes() }),
    mainFunction: (info, chunk) => {
      const cursor = info.getInitData() as { offset: number; readonly roster: ReadonlyArray<number> }
      Option.match(_projected(registry, name, cursor.roster), {
        onNone: () => info.setError(`<source:${name}>`),
        onSome: (held) => {
          const width = Math.min(_SCAN.chunk, held.count - cursor.offset)
          chunk.rowCount = width
          chunk.setColumns(Array.map(held.columns, (column) => Array.take(Array.drop(column.vector, cursor.offset), width)))
          cursor.offset += width
        },
      })
    },
  })

const _sourced = (
  handle: Olap.Handle<"duckdbNode">,
  row: { readonly kind: Olap.Scanned; readonly resident: Olap.Resident; readonly token: string },
): Effect.Effect<string, OlapFault, Scope.Scope> =>
  Effect.gen(function* () {
    const token = yield* Effect.mapError(
      Schema.decode(_Token)(row.token),
      () => OlapFault.at(handle.engine, "wire", `<source:token ${row.token}>`),
    )
    const name = `${row.kind}_${token}`
    yield* row.resident.count <= _SOURCES[row.kind].ceiling
      ? Effect.void
      : Effect.fail(OlapFault.at(handle.engine, "wire", `<source:${row.kind} rows=${row.resident.count}>`))
    return yield* Effect.acquireRelease(
      Effect.flatMap(handle.lease, (session) =>
        _governed(session)(
          { access: "write", fault: "wire" },
          Effect.as(
            Effect.zipRight(
              Effect.sync(() => MutableHashMap.set(handle.sources, name, row.resident)),
              Effect.try({
                try: () => session.connection.registerTableFunction(_scanned(handle.sources, name)),
                catch: _fault(handle.engine, "wire"),
              }),
            ),
            name,
          ),
        )),
      () => Effect.sync(() => MutableHashMap.remove(handle.sources, name)),
    )
  })

const _registered = (session: Olap.Session<"duckdbWasm">, name: string) =>
  Match.type<Olap.File>().pipe(
    Match.when({ key: Match.string }, ({ key }) =>
      Effect.flatMap(ObjectStore, (store) =>
        Effect.flatMap(
          store.grant(key, new GetObjectCommand({ Bucket: store.bucket, Key: key })),
          (grant) =>
            _try(session.engine, "wire", () =>
              session.connection.bindings.registerFileURL(name, grant.url, wasm.DuckDBDataProtocol.HTTP, false)),
        ))),
    Match.when({ blob: Match.instanceOf(Blob) }, ({ blob }) =>
      _try(session.engine, "wire", () =>
        session.connection.bindings.registerFileHandle(name, blob, wasm.DuckDBDataProtocol.BROWSER_FILEREADER, false))),
    Match.orElse(({ octets }) =>
      _try(session.engine, "wire", () => session.connection.bindings.registerFileBuffer(name, octets))),
  )

const _lakeSource = (
  session: Olap.Session<"duckdbWasm">,
  name: string,
  source: Olap.File,
): Effect.Effect<void, OlapFault, ObjectStore | Scope.Scope> =>
  Effect.acquireRelease(
    _governed(session)({ access: "write", fault: "wire" }, _registered(session, name)(source)),
    () => Effect.orDie(_try(session.engine, "wire", () => session.connection.bindings.dropFile(name))),
  )
```

## [04]-[RESIDENCE_ROWS]

- Owner: the durable-plane family — `_extensions` and its one statement generator, the `_SECRETS` credential rail, the `_attach` statements, `_COLUMN`'s column-token alphabet with the `_METRIC_HEAD`/`_POINTS`/`_WIDE` rosters declared over it and the `_mount` attach-and-create projection rendering them, the `_RESIDENCES` rows answering the estate residence floor (`fits`, `admit`, `tenancy`, `lifetime`, `degrade`, `cap` false) beside this plane's own read extension — relations, dialect, and fill — `_residence`/`_target` projecting a row into the core query algebra, `_KIND`/`_relation`/`_absorbed`/`_recorded` filling a plane from the object plane and the live registry, and `_joined` folding signal to evidence; each is data over the `Rows` read geometry, never a new engine surface.
- Packages: AWS, DuckDB, Arrow, Effect, object storage, and core `Identity`, `Digest`, `Convention`, and `Board` supply this plane; `@duckdb/node-api`'s type constants and value carriers and `apache-arrow`'s `DataType` leaves are what `_COLUMN` holds per token.
- Entry: a maintenance fold attaches the spine read-only for offload analytics and the lake catalog for evidence reads, spending `Olap.mount` once over the deploy plane's published metadata-and-data coordinate, which attaches the catalog and creates its relations in one statement; the signal egress calls `Olap.absorb` once per written object batch and a scheduled fold calls `Olap.recorded` once per scrape to fill the derived plane; the composition root calls `Olap.target(residence, identity, source)` once per board and hands the value to `core/observe/board#QUERY`, which renders every tile against it; `Olap.joined` answers a trace-to-evidence reconstruction as one statement.
- Growth: a new residence is one `_RESIDENCES` row answering the floor beside every read column; a new credential kind is one `_SECRETS` row; a new extension is one `_extensions` seed the single `extension(name)` generator consumes; a new column shape is one `_COLUMN` token whose DDL spelling, bound engine type, Arrow type, and two cell packings all ride the row; a new metric point is one `_POINTS` row its DDL and fill both follow from; a new wide-event column is one `_WIDE` entry every dialect projects; a new fillable plane is one `Olap.Plane` shape; a new join correspondence is one `_JOIN` pair.
- Law: TRUTH is the journal alone — every residence is a DERIVED plane carrying zero authority, rebuilt from the fact stream at warm-up cost, so a residence read never settles billing, admission, or erasure; the rebuild route is the journal window replayed through the same egress that filled the plane, and the retention floor below which rebuild fails is the journal's own `Retain.Policy` window, never the residence TTL.
- Law: residences carry NO cardinality ceiling and each row STATES that — a metrics store caps series because a TSDB indexes every one, while unbounded dimensionality IS the reason a residence exists, so a cap landed here deletes the capability; the `cap` column is typed `false` rather than omitted, because a declared value is what a later pass has to overwrite instead of a gap it can helpfully fill, and `_stores` answers health and alerting while residences answer evidence and history under one board owner.
- Law: which residence a fold reaches ARRIVES, never defaults — the fill, the recorded roll-up, the evidence join, and the wide-event read each take the row key as a required field, because the deploy plane already published which residence this stack REALIZED and a member-local default answers for a plane that stack may never have planted; two members defaulting differently in one file is that defect made visible, one arm writing the cold tail while its sibling reads the interactive plane on a stack running exactly one of them.
- Law: the lake's metadata DSN and its data path are the DEPLOY plane's published coordinates — `iac/operate/observe#ENDPOINT_PROJECTION` publishes the object-plane prefix its own cold-tail anchor names and `Olap.mount` attaches the catalog that stack planted over that path, so the planter states both once and this reader spells them back; a prefix derived here, or spelled off the interactive plane's database at either end, addresses a tree nothing ever wrote and answers empty rather than raising.
- Law: `ducklake:<token>` resolves `<token>` as a SECRET NAME and refuses not-found when none exists, so the mount spells a metadata DSN and never the alias — a `<file>.ducklake` path, or a `duckdb:`/`sqlite:`/`postgres:` prefix over one — while the row's `catalog` stays the alias every relation qualifies through; collapsing the pair attaches nothing on any deployment that minted no secret.
- Law: a FILE-backed metadata catalog admits one process at a time, readers included — the second attach refuses at `ATTACH` with a conflicting-lock IO error rather than at its first insert, so the refusal reaches `Olap.mount`'s own `extension` fault and never `Olap.absorb`; a replicated deployment names a server metadata DSN, and the row's `degrade` carries that residual because no capability column expresses which backend a stack published.
- Law: `Olap.target` mints `Board.Query.Target`; the board owner alone renders its residence expression.
- Law: BOTH projection halves are `Option`-carried and the symmetry is the capability — a plane holding metric relations and no wide-event pair is as spellable as its reverse, `Olap.joined` answers `None` where the pair is absent, and `Olap.wide` refuses by name because a read is an Effect and an empty answer reads as "no rows matched" for a relation the plane never held; `Olap.Relation` keys off the wide-event roster that owns it, never off a row that may carry none.
- Law: `pgDuckdb` is a recorded NON-residence, never a missing row — it reads the spine's transactional tables and holds neither wide-event nor OTLP metric relations, so it answers no signal projection at all and `lane/postgres` owns that read path; a residence row minted for it carries two empty halves and claims a plane nothing plants.
- Law: a residence answering fewer projections says so in its row rather than growing a second path — `metrics` is `Option`-carried because the wide-event plane holds no correspondence from `Convention.MetricName` to a span or log relation, so `Olap.target` returns `None` there and the tile degrades visibly instead of rendering an expression that matches nothing.
- Law: the wide-event column vocabulary is PUBLISHED here and derived at every dialect — `_WIDE` is the one roster naming each wide-event relation, its column set, its ORDER, and the neutral token each column rides, `Olap.events` crosses it, and `iac/operate/observe#CHART_ROWS` renders its ClickHouse DDL against that value; the roster seats at this stratum because `iac` imports `data` and the reverse inverts the branch strata, and it seats as TOKENS rather than as rendered type text because a rendered dialect makes the peer a transcription of this one.
- Law: a hand copy of a readable roster carries no guarantee whatever its prose claims — a transcription references nothing, so a rename at the surface it copies breaks nothing at the copy and both ends read correct until a tile empties; the published roster replaces the claim with the fact, because `Olap.WideToken` is the exact token subset a peer's own type table answers and a token this plane starts using breaks that table at ITS declaration.
- Law: the column alphabet keys on a column's CARDINALITY POSTURE, never on its engine type — `low`/`text` and `names`/`texts` collapse to one DuckDB type and part where a dialect spells low cardinality, so the union alphabet costs this plane nothing and is the only shape that keeps the distinction the dialect holding it needs; an alphabet keyed on one engine's types erases exactly what the other engine indexes on.
- Law: the attribute plane derives from the identity projection — a key `Convention.identity` stamps reads off the resource map and every other key off the relation's own signal map, so the split follows the one projection both ends already agree on and no second roster drifts against it.
- Law: credentials never cross into engine SQL for an object plane — the `s3` and `azure` rows take `credential_chain`, so the row carries coordinates the object plane already publishes and the process's ambient grant carries material; `object/store.ts`'s sealed provider record stays sealed and this lane opens no second unwrap of it.
- Law: the `postgres` row is the one credential mint that carries material, and `_secret` is its single unwrap seam — the statement mints `fault: "secret"` and `access: "write"`, so it never rides the retry rail and the fault names the SECRET, never the statement, keeping the DSN out of every profile, log, and fault detail.
- Law: `attach.pg` is read-offload only and binds through a minted secret rather than an inline DSN — the embedded engine reads the spine's tables without a second wire format, and no write path exists from the lane back into the OLTP transaction.
- Law: the lake is ACID over object storage with a SQL catalog — multi-table transactions, time travel, and schema evolution ride the catalog database; the object plane holds immutable Parquet, exactly the content-addressed posture the folder's object rows already enforce.
- Law: a residence names its own ingest owner on the row — `absorb` carries the fill dialect where this lane writes the plane and `None` where the collector's exporter does, so exactly one writer fills each plane and a second path cannot mint rows a retention owner never sees.
- Law: what a plane HOLDS and what any one writer routes into it are different questions, and this lane answers the first from the writer's end — the deploy plane's residence row censuses the same contents at its own grain, so the two ends state one census: the interactive plane holds the wide-event pair its collector leg carries, and the lake holds that pair BESIDE the OTLP metric-point relations this lane plants and fills, which is exactly the asymmetry `plant` and `metrics` already spell as `Some` here and `None` there.
- Law: the metric relations make the lake the ONE-plane join — a board joining series to wide events resolves both sides here, where the interactive plane forces that join across two planes and says so on its own `degrade`; the deploy plane publishes that same census on its residence row, so a reader picks its plane off a column at either end rather than inferring a plane's contents from which branch owns its writer.
- Law: no collector exporter frames a metric point at any residence, so a census widening here obligates NO deploy-plane routing change — the routing bound rides that tier's own exporter column and this lane's fill is the only writer the metric relations ever see; reading a routing answer as a contents answer at either end reports both planes as wide-event-only and strands every metric tile the lake already serves.
- Law: a fill dialect obligates a DDL projection at the same owner — the `plant` column carries every relation the lake's own fills write into and `Olap.mount` attaches and creates them in ONE statement, so a plane this lane writes exists on a clean catalog and `Olap.absorb` lands its first insert; a residence the collector plants carries neither column, which is what makes the pair total.
- Law: both durable planes project ONE token roster into their own dialects rather than each spelling its own column list, so a Parquet object written for either plane absorbs into the other `BY NAME` and the wide-event vocabulary cannot fork at a name, an order, or a cardinality posture.
- Law: the metric relation roster is the OTLP data model's, never the mount roster — `_POINTS` carries one row per point type and `_KIND` projects `Convention.InstrumentKind` onto three of them, so `summary` and `exponentialHistogram` plant and read while no instrument reaches them; deleting the two unreached relations strands a foreign producer's rows in a relation nothing created, and mapping a kind onto one forges a series no mount emits.
- Law: summary series map through `Board.Query.Residence`; exponential histograms remain readable foreign-producer relations.
- Law: the kind map is TOTAL over the instrument roster, so a wire form the branch gains relates in the same edit that mints it — a kind the map omits is a producer whose series this plane declares no relation for, which reads at the tile as an empty panel rather than as the missing row it is.
- Law: the metric plane keys its own instant — OTLP metric relations carry `TimeUnix` where the wide-event relations carry `Timestamp`, so the bucket column rides the `metrics` record and the row-level `time` answers wide events alone; one shared column empties every metric tile against a name its relation never declared.
- Law: `Board.Query.Residence.value` maps each instrument kind to the scalar column its relation carries.
- Law: a bucket relation answers the DISTRIBUTION reads off its own columns, never off the scalar — the edge list and the per-bucket counts are both present, so `quantile` and `fraction` project as residence row functions beside `access` and `series` and a latency objective reads the same number here it reads against a metrics store; degrading a rank to `Sum` answers a total where the panel asked for a percentile. Interpolation is the residence engine's DIALECT and the scalar fold is the engine roster's, so the expression rides the row while the arm selecting between them reads the metric kind alone.
- Law: `fraction` takes a DECLARED edge — the mint froze the ladder, so a share below an invented bound has no bucket to sum and the unresolved position rides out as NULL rather than interpolating one; the objective owner already guarantees the precondition by landing every ceiling on a declared edge, so it is met at the surface that states it and never re-checked here, while a zero fallback over that NULL publishes an empty share for a bound nothing measured and reads identically to a genuinely empty leading bucket.
- Law: `Olap.recorded` folds `Board.DashboardModel.snapshot` into point relations and stamps the object fill's identity.
- Law: a fill cell keys by COLUMN NAME off `_POINTS`, never by position — one roster drives the arm's declaration, the VALUES projection, and the column alias together, so an arm omitting or misspelling a column fails where it is written and reordering a point moves all three in one edit; a positional tuple files every cell past a moved column under its neighbour's name, and a bucket count landing in a `Sum` column reads as a plausible number no decode, constraint, or tile ever questions.
- Law: the Parquet writer the object fill presupposes is this page's own — `Olap.lake.write` and `Olap.lake.sink` at `[08]-[ARROW_WIRE]` mint the bytes and LAND them through `object/store#CONDITIONAL` with their reference row, so "the data planes" the deploy tier credits for lake ingest names these two members and no surface owes a writer; the keys they answer are the ones `Olap.absorb` scans.
- Law: the fill stamps identity, so the derived plane is joinable on arrival — every absorbed row leaves carrying the whole `Convention.identity` projection on its resource map, which is the same key `_JOIN` reads back; a plane filled without it answers no reconstruction until a second pass rewrites every row.
- Boundary: the peer dialect derives the ROSTER and keeps its own engine facts — `iac/operate/observe#CHART_ROWS` binds its ClickHouse type table as `Record<Olap.WideToken, string>` so an unanswered token refuses at that declaration, renders each relation off `Olap.events`'s own `table`, `text`, `plane`, and column list, and keeps the sort-key roster, the bloom and token indices, the partition expression, the TTL, and the `ORDER BY` on its own side; those coordinates stay there because a column no reader on THIS side consumes is a governance row keyed past its consumer, and the seam carries exactly the facts both ends spell.
- Law: the signal-to-evidence join rides SQL `ATTACH`, never a TypeScript import — the lake relations and the spine's `fact_journal` meet inside one embedded statement, so the strata direction this page holds against `journal` survives a join that reads both.

```typescript
import {
  BOOLEAN, DOUBLE, LIST, listValue, MAP, mapValue, quotedIdentifier, quotedString, TIMESTAMP_NS, timestampNanosValue,
  UBIGINT, UINTEGER, UTINYINT, VARCHAR, type DuckDBType, type DuckDBValue,
} from "@duckdb/node-api"
import { Bool, Field, Float64, List, Map_, Struct, TimestampNanosecond, Uint8, Uint32, Uint64, Utf8, type DataType } from "apache-arrow"
import { Array, DateTime, Duration, Match, Option, pipe, Record, Redacted } from "effect"
import { Board, Convention, type Digest, type Identity } from "@rasm/core"
import { ObjectStore } from "../object/store.ts"

const _mapped = (pairs: Record.ReadonlyRecord<string, string | undefined>): string =>
  pipe(
    Array.filterMap(Record.toEntries(pairs), ([key, value]) => Option.map(Option.fromNullable(value), (held) => [key, held] as const)),
    (present) => `MAP {${Array.join(Array.map(present, ([key, value]) => `${quotedString(key)}: ${quotedString(value)}`), ", ")}}`,
  )

const _extensions = ["httpfs", "aws", "azure", "ducklake", "iceberg", "delta", "postgres", "sqlite", "spatial", "vss", "fts"] as const

const _SECRETS = {
  azure: { prefix: "azure://", provider: "credential_chain", storage: "local_file" },
  postgres: { prefix: "", provider: "config", storage: "local_file" },
  s3: { prefix: "s3://", provider: "credential_chain", storage: "local_file" },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly prefix: string
  readonly provider: string
  readonly storage: string
}>

declare namespace Olap {
  type Extension = (typeof _extensions)[number]
  type Secret = keyof typeof _SECRETS
  type SecretValue = Redacted.Redacted<string> | boolean | number | string
  type Custody = "persistent" | "session"
}

const _spliced = Match.type<Olap.SecretValue>().pipe(
  Match.when(Match.string, quotedString),
  Match.when(Match.boolean, (held) => `${held}`),
  Match.when(Match.number, (held) => `${held}`),
  Match.orElse((held) => quotedString(Redacted.value(held))),
)

const _secret = (row: {
  readonly config: ReadonlyArray<readonly [key: string, value: Olap.SecretValue]>
  readonly custody?: Olap.Custody
  readonly name: string
  readonly scope?: string
  readonly type: Olap.Secret
}): OlapRead =>
  _Read.Rows({
    sql: `CREATE OR REPLACE ${row.custody === "persistent" ? "PERSISTENT " : ""}SECRET ${quotedIdentifier(row.name)}`
      + `${row.custody === "persistent" ? ` IN ${_SECRETS[row.type].storage}` : ""} (${
        Array.join(
          [
            `TYPE ${row.type}`,
            `PROVIDER ${_SECRETS[row.type].provider}`,
            ...Option.match(Option.fromNullable(row.scope), {
              onNone: () => [],
              onSome: (held) => [`SCOPE ${quotedString(`${_SECRETS[row.type].prefix}${held}`)}`],
            }),
            ...Array.map(row.config, ([key, value]) => `${quotedIdentifier(key)} ${_spliced(value)}`),
          ],
          ", ",
        )
      })`,
    fault: "secret",
    access: "write",
  })

const _objectSecret = Effect.map(ObjectStore, (store) =>
  _secret({
    config: [
      ["CHAIN", "env;config;instance"],
      ["ENDPOINT", store.provider.endpoint],
      ["REGION", store.provider.region],
      ["URL_STYLE", store.provider.forcePathStyle ? "path" : "vhost"],
    ],
    name: "olap_object",
    scope: store.bucket,
    type: "s3",
  }))

const _attach = {
  extension: (name: Olap.Extension) =>
    _Read.Rows({ sql: `INSTALL ${quotedIdentifier(name)}; LOAD ${quotedIdentifier(name)};`, fault: "extension", access: "write" }),
  extensions: _extensions,
  object: _objectSecret,
  pg: (secret: string) =>
    _Read.Rows({ sql: `ATTACH '' AS spine (TYPE postgres, SECRET ${quotedIdentifier(secret)}, READ_ONLY)`, fault: "extension", access: "write" }),
  secret: _secret,
  secrets: _SECRETS,
  sqlite: (path: string) => _Read.Rows({ sql: `ATTACH ${quotedString(path)} AS lane (TYPE sqlite)`, fault: "extension", access: "write" }),
} as const

// --- [RESIDENCE_FAMILY]

const _entries = (): Map_<Utf8, Utf8> =>
  new Map_(new Field("entries", new Struct([new Field("key", new Utf8(), false), new Field("value", new Utf8(), true)]), false))

const _paired = (cell: ReadonlyMap<string, string>): DuckDBValue =>
  mapValue(Array.map(Array.fromIterable(cell), ([key, value]) => ({ key, value })))

const _same = <A>(cell: A): A => cell

const _shed = (cell: bigint): number => Number(cell / 1_000_000n)

const _COLUMN = {
  bool: { arrow: () => new Bool(), cast: (cell: boolean) => cell, pack: _same, type: BOOLEAN },
  byte: { arrow: () => new Uint8(), cast: (cell: number) => cell, pack: _same, type: UTINYINT },
  flag: { arrow: () => new Uint32(), cast: (cell: number) => cell, pack: _same, type: UINTEGER },
  low: { arrow: () => new Utf8(), cast: (cell: string) => cell, pack: _same, type: VARCHAR },
  map: { arrow: _entries, cast: _paired, pack: _same, type: MAP(VARCHAR, VARCHAR) },
  maps: {
    arrow: () => new List(new Field("item", _entries(), true)),
    cast: (cell: ReadonlyArray<ReadonlyMap<string, string>>) => listValue(Array.map(cell, _paired)),
    pack: _same,
    type: LIST(MAP(VARCHAR, VARCHAR)),
  },
  names: {
    arrow: () => new List(new Field("item", new Utf8(), true)),
    cast: (cell: ReadonlyArray<string>) => listValue(cell),
    pack: _same,
    type: LIST(VARCHAR),
  },
  real: { arrow: () => new Float64(), cast: (cell: number) => cell, pack: _same, type: DOUBLE },
  reals: {
    arrow: () => new List(new Field("item", new Float64(), true)),
    cast: (cell: ReadonlyArray<number>) => listValue(cell),
    pack: _same,
    type: LIST(DOUBLE),
  },
  span: { arrow: () => new Uint64(), cast: (cell: bigint) => cell, pack: _same, type: UBIGINT },
  stamp: { arrow: () => new TimestampNanosecond(), cast: timestampNanosValue, pack: _shed, type: TIMESTAMP_NS },
  stamps: {
    arrow: () => new List(new Field("item", new TimestampNanosecond(), true)),
    cast: (cell: ReadonlyArray<bigint>) => listValue(Array.map(cell, timestampNanosValue)),
    pack: (cell: ReadonlyArray<bigint>) => Array.map(cell, _shed),
    type: LIST(TIMESTAMP_NS),
  },
  text: { arrow: () => new Utf8(), cast: (cell: string) => cell, pack: _same, type: VARCHAR },
  texts: {
    arrow: () => new List(new Field("item", new Utf8(), true)),
    cast: (cell: ReadonlyArray<string>) => listValue(cell),
    pack: _same,
    type: LIST(VARCHAR),
  },
  whole: { arrow: () => new Uint64(), cast: (cell: bigint) => cell, pack: _same, type: UBIGINT },
  wholes: {
    arrow: () => new List(new Field("item", new Uint64(), true)),
    cast: (cell: ReadonlyArray<bigint>) => listValue(cell),
    pack: _same,
    type: LIST(UBIGINT),
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly arrow: () => DataType
  readonly cast: (cell: never) => DuckDBValue
  readonly pack: (cell: never) => unknown
  readonly type: DuckDBType
}>

declare namespace Olap {
  type Token = keyof typeof _COLUMN
  type Cell<T extends Token = Token> = Parameters<(typeof _COLUMN)[T]["cast"]>[0]
  type Column = readonly [name: string, token: Token]
  type Columns = Array.NonEmptyReadonlyArray<Column>
}

const _METRIC_HEAD = [
  ["ResourceAttributes", "map"], ["ResourceSchemaUrl", "low"], ["ScopeName", "text"], ["ScopeVersion", "text"],
  ["ScopeAttributes", "map"], ["ScopeSchemaUrl", "low"], ["ServiceName", "low"], ["MetricName", "low"],
  ["MetricDescription", "text"], ["MetricUnit", "low"], ["Attributes", "map"], ["StartTimeUnix", "stamp"],
  ["TimeUnix", "stamp"],
] as const satisfies Olap.Columns

const _POINTS = {
  exponentialHistogram: {
    columns: [
      ["Count", "whole"], ["Sum", "real"], ["Scale", "byte"], ["ZeroCount", "whole"],
      ["PositiveOffset", "byte"], ["PositiveBucketCounts", "wholes"], ["NegativeOffset", "byte"],
      ["NegativeBucketCounts", "wholes"], ["Min", "real"], ["Max", "real"], ["Flags", "flag"],
      ["AggregationTemporality", "byte"],
    ],
    relation: "otel_metrics_exponential_histogram",
    value: "Sum",
  },
  gauge: { columns: [["Value", "real"], ["Flags", "flag"]], relation: "otel_metrics_gauge", value: "Value" },
  histogram: {
    columns: [
      ["Count", "whole"], ["Sum", "real"], ["BucketCounts", "wholes"], ["ExplicitBounds", "reals"],
      ["Min", "real"], ["Max", "real"], ["Flags", "flag"], ["AggregationTemporality", "byte"],
    ],
    relation: "otel_metrics_histogram",
    value: "Sum",
  },
  sum: {
    columns: [["Value", "real"], ["Flags", "flag"], ["AggregationTemporality", "byte"], ["IsMonotonic", "bool"]],
    relation: "otel_metrics_sum",
    value: "Value",
  },
  summary: {
    columns: [
      ["Count", "whole"], ["Sum", "real"], ["ValueAtQuantiles.Quantile", "reals"],
      ["ValueAtQuantiles.Value", "reals"], ["Flags", "flag"],
    ],
    relation: "otel_metrics_summary",
    value: "Sum",
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly columns: Olap.Columns
  readonly relation: string
  readonly value: string
}>

const _WIDE = {
  logs: {
    columns: [
      ["Timestamp", "stamp"], ["TraceId", "text"], ["SpanId", "text"], ["TraceFlags", "byte"],
      ["SeverityText", "low"], ["SeverityNumber", "byte"], ["ServiceName", "low"], ["Body", "text"],
      ["ResourceSchemaUrl", "low"], ["ResourceAttributes", "map"], ["ScopeSchemaUrl", "low"],
      ["ScopeName", "text"], ["ScopeVersion", "low"], ["ScopeAttributes", "map"], ["LogAttributes", "map"],
      ["EventName", "text"],
      ["ResourceAttributesKeys", "names"], ["ScopeAttributesKeys", "names"], ["LogAttributesKeys", "names"],
    ],
    plane: "LogAttributes",
    table: "otel_logs",
    text: "Body",
  },
  traces: {
    columns: [
      ["Timestamp", "stamp"], ["TraceId", "text"], ["SpanId", "text"], ["ParentSpanId", "text"],
      ["TraceState", "text"], ["SpanName", "low"], ["SpanKind", "low"], ["ServiceName", "low"],
      ["ResourceAttributes", "map"], ["ScopeName", "text"], ["ScopeVersion", "text"], ["SpanAttributes", "map"],
      ["Duration", "span"], ["StatusCode", "low"], ["StatusMessage", "text"],
      ["Events.Timestamp", "stamps"], ["Events.Name", "names"], ["Events.Attributes", "maps"],
      ["Links.TraceId", "texts"], ["Links.SpanId", "texts"], ["Links.TraceState", "texts"],
      ["Links.Attributes", "maps"],
      ["ResourceAttributesKeys", "names"], ["SpanAttributesKeys", "names"],
    ],
    plane: "SpanAttributes",
    table: "otel_traces",
    text: "SpanName",
  },
} as const satisfies Record.ReadonlyRecord<"logs" | "traces", {
  readonly columns: Olap.Columns
  readonly plane: string
  readonly table: string
  readonly text: string
}>

const _RESIDENCES = {
  clickhouse: {
    absorb: Option.none(),
    access: (map: string, key: Board.Query.Key) => `${map}[${quotedString(key)}]`,
    cap: false,
    catalog: "rasm",
    degrade: "wide events only — no metric relation exists here, so a metric tile renders against the lake plane",
    engine: "clickhouse",
    fits: "interactive wide-event at any cardinality",
    admit: "the deploy plane's collector `clickhouse` exporter against branch-owned DDL",
    lifetime: { bound: "table TTL with `ttl_only_drop_parts`, expiring on the engine's own merge schedule", owner: "package" },
    tenancy: "tenant leads the sort key",
    metrics: Option.none(),
    plant: Option.none(),
    relations: Option.some({
      logs: { plane: _WIDE.logs.plane, table: _WIDE.logs.table, text: _WIDE.logs.text },
      traces: { plane: _WIDE.traces.plane, table: _WIDE.traces.table, text: _WIDE.traces.text },
    }),
    resolution: Duration.seconds(30),
    resource: "ResourceAttributes",
    series: (parts: ReadonlyArray<string>) => `concat_ws(char(31), ${Array.join(parts, ", ")})`,
    time: "Timestamp",
  },
  lake: {
    absorb: Option.some({
      merge: (plane: string, pairs: Record.ReadonlyRecord<string, string | undefined>) => `map_concat(${plane}, ${_mapped(pairs)})`,
      scan: (uris: ReadonlyArray<string>) => `read_parquet([${Array.join(Array.map(uris, quotedString), ", ")}], union_by_name = true)`,
    }),
    access: (map: string, key: Board.Query.Key) => `COALESCE(${map}[${quotedString(key)}], '')`,
    cap: false,
    catalog: "lake",
    degrade: "batch scan over object storage — a tile here reads as a report, never an interactive panel, and no Grafana driver reads the tree, so a board reaches it through a report this lane renders; a file-backed metadata catalog admits one process at a time, readers included, so a replicated stack publishes a server metadata DSN",
    engine: "duckdb",
    fits: "cold tail, cheapest per byte, batch scan",
    admit: "this lane's own `Olap.lake.sink` mints the objects and `Olap.absorb` lands them",
    lifetime: { bound: "table-format retention over the committed snapshot", owner: "object-plane" },
    tenancy: "partition column",
    metrics: Option.some({
      name: "MetricName",
      plane: "Attributes",
      tables: Record.map(_POINTS, (point) => point.relation),
      time: "TimeUnix",
      quantile: (at: number) =>
        `list_extract(ExplicitBounds, list_position(`
        + `list_transform(BucketCounts, lambda c, i: list_sum(list_slice(BucketCounts, 1, i)) >= list_sum(BucketCounts) * ${at})`
        + `, true))`,
      fraction: (below: number) =>
        `list_sum(list_slice(BucketCounts, 1, list_position(ExplicitBounds, ${below})))`
        + ` / NULLIF(list_sum(BucketCounts), 0)`,
    }),
    plant: Option.some([
      ...Array.map(Record.values(_WIDE), (row) => ({ columns: row.columns, table: row.table })),
      ...Array.map(Record.values(_POINTS), (point) => ({ columns: [..._METRIC_HEAD, ...point.columns], table: point.relation })),
    ]),
    relations: Option.some({
      logs: { plane: _WIDE.logs.plane, table: _WIDE.logs.table, text: _WIDE.logs.text },
      traces: { plane: _WIDE.traces.plane, table: _WIDE.traces.table, text: _WIDE.traces.text },
    }),
    resolution: Duration.minutes(5),
    resource: "ResourceAttributes",
    series: (parts: ReadonlyArray<string>) => `concat_ws(chr(31), ${Array.join(parts, ", ")})`,
    time: "Timestamp",
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly absorb: Option.Option<{
    readonly merge: (plane: string, pairs: Record.ReadonlyRecord<string, string | undefined>) => string
    readonly scan: (uris: ReadonlyArray<string>) => string
  }>
  readonly access: (map: string, key: Board.Query.Key) => string
  readonly cap: false
  readonly catalog: string
  readonly degrade: string
  readonly engine: Board.Query.Engine
  readonly fits: string
  readonly admit: string
  readonly lifetime: { readonly bound: string; readonly owner: "package" | "object-plane" }
  readonly tenancy: string
  readonly metrics: Option.Option<{
    readonly fraction: (below: number) => string
    readonly name: string
    readonly plane: string
    readonly quantile: (at: number) => string
    readonly tables: Record.ReadonlyRecord<Olap.Point, string>
    readonly time: string
  }>
  readonly plant: Option.Option<ReadonlyArray<{ readonly columns: Olap.Columns; readonly table: string }>>
  readonly relations: Option.Option<Record.ReadonlyRecord<"logs" | "traces", { readonly plane: string; readonly table: string; readonly text: string }>>
  readonly resolution: Duration.Duration
  readonly resource: string
  readonly series: (parts: ReadonlyArray<string>) => string
  readonly time: string
}>

declare namespace Olap {
  type Point = keyof typeof _POINTS
  type Residence = keyof typeof _RESIDENCES
  type Relation = keyof typeof _WIDE
  type WideToken = (typeof _WIDE)[Relation]["columns"][number][1]
  type Plane = { readonly signal: Relation } | { readonly kind: Convention.InstrumentKind }
  type Coordinate = { readonly data: string; readonly metadata: string }
}

const _KIND = {
  counter: "sum",
  frequency: "sum",
  gauge: "gauge",
  histogram: "histogram",
  summary: "summary",
  updown: "sum",
} as const satisfies Record.ReadonlyRecord<Convention.InstrumentKind, Olap.Point>

const _mount = (key: Olap.Residence, coordinate: Olap.Coordinate): Option.Option<OlapRead> =>
  Option.map(_RESIDENCES[key].plant, (rows) =>
    _Read.Rows({
      sql: Array.join([
        `ATTACH IF NOT EXISTS ${quotedString(`ducklake:${coordinate.metadata}`)}`
        + ` AS ${quotedIdentifier(_RESIDENCES[key].catalog)} (DATA_PATH ${quotedString(coordinate.data)});`,
        ...Array.map(rows, (row) =>
          `CREATE TABLE IF NOT EXISTS ${_RESIDENCES[key].catalog}.${quotedIdentifier(row.table)} (`
          + `${Array.join(Array.map(row.columns, ([name, token]) => `${quotedIdentifier(name)} ${_COLUMN[token].type}`), ", ")});`),
      ], " "),
      fault: "extension",
      access: "write",
    }))

const _planed = (row: (typeof _RESIDENCES)[Olap.Residence], identity: Identity.App, plane: string) =>
  (key: Board.Query.Key): string =>
    row.access(Record.has(Convention.identity(identity), key) ? row.resource : plane, key)

const _residence = (key: Olap.Residence, identity: Identity.App): Option.Option<Board.Query.Residence> =>
  Option.map(_RESIDENCES[key].metrics, (metrics) =>
    pipe(_planed(_RESIDENCES[key], identity, metrics.plane), (attribute) => ({
      attribute,
      degrade: _RESIDENCES[key].degrade,
      identity: (keys: ReadonlyArray<Board.Query.Key>) =>
        keys.length === 0 ? "''" : _RESIDENCES[key].series(Array.map(keys, attribute)),
      name: metrics.name,
      table: Record.map(_KIND, (point) => `${_RESIDENCES[key].catalog}.${metrics.tables[point]}`),
      time: metrics.time,
      quantile: metrics.quantile,
      fraction: metrics.fraction,
      value: Record.map(_KIND, (point) => _POINTS[point].value),
    })))

const _target = (key: Olap.Residence, identity: Identity.App, source: string): Option.Option<Board.Query.Target> =>
  Option.map(_residence(key, identity), (residence) =>
    Board.Query.sql({
      engine: _RESIDENCES[key].engine,
      residence,
      resolution: Board.Query.span(_RESIDENCES[key].resolution),
      source,
    }))

// --- [RESIDENCE_FILL]

const _relation = (held: (typeof _RESIDENCES)[Olap.Residence]) =>
  Match.type<Olap.Plane>().pipe(
    Match.when(
      { signal: Match.string },
      ({ signal }) => Option.map(held.relations, (rows) => ({ plane: rows[signal].plane, table: rows[signal].table })),
    ),
    Match.orElse(({ kind }) => Option.map(held.metrics, (metrics) => ({ plane: metrics.plane, table: metrics.tables[_KIND[kind]] }))),
  )

const _absorbed = (row: {
  readonly identity: Identity.App
  readonly objects: ReadonlyArray<Digest.Key<"content">>
  readonly plane: Olap.Plane
  readonly residence: Olap.Residence
}): Effect.Effect<Option.Option<OlapRead>, never, ObjectStore> =>
  Effect.map(ObjectStore, (store) =>
    pipe(_RESIDENCES[row.residence], (held) =>
      Option.map(
        Option.all({ fill: held.absorb, relation: _relation(held)(row.plane) }),
        ({ fill, relation }) =>
          _Read.Rows({
            sql: `INSERT INTO ${held.catalog}.${relation.table} BY NAME SELECT * EXCLUDE (${held.resource}),`
              + ` ${fill.merge(held.resource, Convention.identity(row.identity))} AS ${held.resource}`
              + ` FROM ${fill.scan(Array.map(row.objects, (key) => `s3://${store.bucket}/${key}`))}`,
            fault: "query",
            access: "write",
          }),
      )))

type _Cell<P extends Olap.Point> = {
  readonly attributes: Record.ReadonlyRecord<string, string | undefined>
  readonly cells: { readonly [K in (typeof _POINTS)[P]["columns"][number][0]]: string }
  readonly name: Convention.MetricName
  readonly point: P
}

const _CELLS = {
  Counter: (row: Extract<Board.DashboardModel.Signal, { readonly _tag: "Counter" }>): ReadonlyArray<_Cell<"sum">> => [{
    attributes: row.labels,
    cells: { Value: `${row.value}`, Flags: "0", AggregationTemporality: "2", IsMonotonic: `${row.declared !== "updown"}` },
    name: row.name,
    point: "sum",
  }],
  Frequency: (row: Extract<Board.DashboardModel.Signal, { readonly _tag: "Frequency" }>): ReadonlyArray<_Cell<"sum">> =>
    Array.map(Array.fromIterable(row.values), ([word, count]) => ({
      attributes: { ...row.labels, [Convention.wire.occurrence]: word },
      cells: { Value: `${count}`, Flags: "0", AggregationTemporality: "2", IsMonotonic: "true" },
      name: row.name,
      point: "sum",
    })),
  Gauge: (row: Extract<Board.DashboardModel.Signal, { readonly _tag: "Gauge" }>): ReadonlyArray<_Cell<"gauge">> => [{
    attributes: row.labels,
    cells: { Value: `${row.value}`, Flags: "0" },
    name: row.name,
    point: "gauge",
  }],
  Histogram: (row: Extract<Board.DashboardModel.Signal, { readonly _tag: "Histogram" }>): ReadonlyArray<_Cell<"histogram">> => [{
    attributes: row.labels,
    cells: {
      Count: `${row.count}`,
      Sum: `${row.sum}`,
      BucketCounts: `[${
        Array.join(
          pipe(
            Array.map(row.buckets, ([, held]) => held),
            (cumulative) => Array.zipWith(cumulative, Array.prepend(cumulative, 0), (held, prior) => `${held - prior}`),
          ),
          ", ",
        )
      }]`,
      ExplicitBounds: `[${Array.join(Array.map(Array.dropRight(row.buckets, 1), ([bound]) => `${bound}`), ", ")}]`,
      Min: `${row.min}`,
      Max: `${row.max}`,
      Flags: "0",
      AggregationTemporality: "2",
    },
    name: row.name,
    point: "histogram",
  }],
  Summary: (row: Extract<Board.DashboardModel.Signal, { readonly _tag: "Summary" }>): ReadonlyArray<_Cell<"summary">> =>
    pipe(Array.filterMap(row.quantiles, ([at, held]) => Option.map(held, (present) => [at, present] as const)), (measured) => [{
      attributes: row.labels,
      cells: {
        Count: `${row.count}`,
        Sum: `${row.sum}`,
        "ValueAtQuantiles.Quantile": `[${Array.join(Array.map(measured, ([at]) => `${at}`), ", ")}]`,
        "ValueAtQuantiles.Value": `[${Array.join(Array.map(measured, ([, held]) => `${held}`), ", ")}]`,
        Flags: "0",
      },
      name: row.name,
      point: "summary",
    }]),
  Unknown: (): ReadonlyArray<_Cell<Olap.Point>> => [],
} as const

const _headed = (
  row: { readonly at: DateTime.Utc; readonly identity: Identity.App },
  scope: ReturnType<typeof Convention.scope>,
  entry: { readonly attributes: Record.ReadonlyRecord<string, string | undefined>; readonly name: Convention.MetricName },
): { readonly [K in (typeof _METRIC_HEAD)[number][0]]: string } =>
  pipe(`${quotedString(DateTime.formatIso(row.at))}::${_COLUMN.stamp.type}`, (stamp) => ({
    ResourceAttributes: _mapped(Convention.identity(row.identity)),
    ResourceSchemaUrl: quotedString(scope.schemaUrl),
    ScopeName: quotedString(scope.name),
    ScopeVersion: quotedString(scope.version),
    ScopeAttributes: "MAP {}",
    ScopeSchemaUrl: quotedString(scope.schemaUrl),
    ServiceName: quotedString(row.identity.app),
    MetricName: quotedString(entry.name),
    MetricDescription: quotedString(Convention.Metric.at(entry.name).description),
    MetricUnit: quotedString(Convention.Metric.at(entry.name).unit),
    Attributes: _mapped(entry.attributes),
    StartTimeUnix: stamp,
    TimeUnix: stamp,
  }))

const _recorded = (row: {
  readonly identity: Identity.App
  readonly at: DateTime.Utc
  readonly residence: Olap.Residence
}): Effect.Effect<ReadonlyArray<OlapRead>, never, never> =>
  Effect.map(Board.DashboardModel.snapshot, (signals) =>
    pipe({ held: _RESIDENCES[row.residence], scope: Convention.scope("data", row.identity.build.version) }, ({ held, scope }) =>
      Option.match(held.metrics, {
        onNone: () => [],
        onSome: (metrics) =>
          pipe(
            Array.flatMap(signals, (signal) => Match.valueTags(signal, _CELLS)),
            Array.groupBy((entry) => entry.point),
            Record.toEntries,
            Array.map(([point, entries]) =>
              _Read.Rows({
                sql: `INSERT INTO ${held.catalog}.${metrics.tables[point as Olap.Point]} BY NAME SELECT * FROM (VALUES ${
                  Array.join(
                    Array.map(entries, (entry) =>
                      pipe(_headed(row, scope, entry), (head) =>
                        `(${
                          Array.join([
                            ...Array.map(_METRIC_HEAD, ([name]) => head[name]),
                            ...Array.map(_POINTS[point as Olap.Point].columns, ([name]) => entry.cells[name as keyof typeof entry.cells]),
                          ], ", ")
                        })`)),
                    ", ",
                  )
                }) AS t(${
                  Array.join(
                    Array.map([..._METRIC_HEAD, ..._POINTS[point as Olap.Point].columns], ([name]) => quotedIdentifier(name)),
                    ", ",
                  )
                })`,
                fault: "query",
                access: "write",
              })),
          ),
      })))

// --- [EVIDENCE_JOIN]

const _JOIN: ReadonlyArray<readonly [key: Board.Query.Key, column: string]> = [
  [Convention.attr.serviceName, "app"],
  [Convention.rasm.tenant, "tenant"],
]

const _joined = (row: {
  readonly identity: Identity.App
  readonly on?: ReadonlyArray<readonly [key: Board.Query.Key, column: string]>
  readonly relation?: Olap.Relation
  readonly residence: Olap.Residence
  readonly window: Duration.Duration
}): Option.Option<OlapRead> =>
  Option.map(
    Option.map(_RESIDENCES[row.residence].relations, (rows) => ({
      held: _RESIDENCES[row.residence],
      millis: Duration.toMillis(row.window),
      relation: rows[row.relation ?? "traces"],
      seconds: Duration.toSeconds(row.window),
    })),
    ({ held, millis, relation, seconds }) =>
      pipe(_planed(held, row.identity, relation.plane), (attribute) =>
        _Read.Rows({
          sql: `SELECT s.TraceId AS trace, s.SpanId AS span, s.${relation.text} AS name, s.${held.time} AS at,`
            + ` f.sequence AS evidence, f.stream AS stream, f.payload AS payload`
            + ` FROM ${held.catalog}.${relation.table} s JOIN spine.fact_journal f ON ${
              Array.join(
                [
                  ...Array.map(row.on ?? _JOIN, ([key, column]) => `${attribute(key)} = COALESCE(f.${column}, '')`),
                  `f.stamp_physical BETWEEN epoch_ms(s.${held.time}) - ${millis}`
                  + ` AND epoch_ms(s.${held.time}) + ${millis}`,
                ],
                " AND ",
              )
            } WHERE s.${held.time} >= now() - INTERVAL '${seconds} seconds'`,
          fault: "query",
          access: "read",
        })),
  )
```

## [05]-[CLICKHOUSE]

- Owner: the at-scale driver row — the `ClickhouseClient` Layer mint, `Olap.ingest`'s quota-governed `insertQuery` seam, and `Olap.wide`, the residence read end folding the neutral `sql` DSL onto the lane's one fault rail; typed parameters, command-mode routing, query IDs, and scoped settings stay members of the concrete client held inside the owner rather than parallel lane entries.
- Packages: `@effect/sql-clickhouse` (`ClickhouseClient.layerConfig`, `insertQuery`, `asCommand`, `param`, `withQueryId`, `withClickhouseSettings`); `@effect/sql` (`SqlClient`, `SqlError`); `@effect/experimental` (`RateLimiter.makeWithRateLimiter`, `RateLimiterStore`); `effect` (`Clock`, `Config`, `Duration`, `Layer`).
- Entry: admitted at the composition root only where the `_engines.clickhouse.trigger` condition is real; the fact journal's high-cardinality rollups replicate into MergeTree through `Olap.ingest`, whose intent carries the query id and settings posture applied on the concrete client fiber; `Olap.wide` answers the residence's logs and traces relations, and dashboards read the cluster, never the OLTP spine.
- Growth: a new ingestion stream is one `ingest` call site over the same layer; a new settings posture is a `withClickhouseSettings` scope; a quota posture is a `quota` field on the intent overriding the standing row, never a consumer wrap.
- Law: the driver extends the neutral `SqlClient`, so analytical reads ride the same `sql` DSL and typed decode as every lane and the concrete Tag is reached for ingestion, command routing, typed params, and per-query settings alone; the compiler self-reports the `sqlite` dialect, so ClickHouse divergence rides those members and never an `onDialect` arm a reader goes looking for.
- Law: the read end is a fold, not a second lane — `Olap.wide` takes a residence relation and a `Statement.Fragment`, so a wide-event read composes the same `_RESIDENCES` row the query target reads and no consumer hand-spells a table name. Its projection admits against that relation's own `_WIDE` column roster and crosses each accepted name as an escaped identifier, so an undeclared column refuses by name and no caller string reaches statement text — a comma-joined projection spliced raw is the injection surface the browser lane already refuses, and it also mis-parses every dotted attribute column the roster declares.
- Law: every foreign rail folds at this owner — `SqlError` from the driver and `RateLimiterError` from the quota each land as `OlapFault` before leaving, so `Olap` is one error channel end to end and a consumer never catches three error families to reach one recovery. That fold is what ARMS the retry rather than a tidiness: the core gate grades any shape carrying no `class` property as `defect`, whose row is non-retryable, so a foreign error reaching the schedule unfolded refuses every replay in silence while the meter beside it counts nothing — `OlapFault` publishes `class` off its own family row, which is why the gate and the replay meter cannot disagree on any of the five entries this lane governs.
- Law: both ends ride the lane governor — the read and the admitted insert each carry `_GOVERNOR.budget`, so a stalled cluster releases its fiber on the same bound an embedded statement holds; the quota wait sits OUTSIDE that bracket because `onExceeded: "delay"` is deliberate back-pressure, and timing it out makes the suspend disposition lossy exactly where it exists not to be.
- Law: ingestion is load-shed at the owner — the token-bucket limiter keys by app so one tenant's replication burst cannot starve siblings, `onExceeded: "delay"` suspends instead of dropping (replication is re-runnable, never lossy by quota), and a durable `RateLimiterStore` composes at a multi-replica root; `withQueryId` and `withClickhouseSettings` scope correlation and server policy to the admitted insert fiber, never process state.
- Law: the quota vocabulary is the limiter's OWN options row minus its key — a standing posture is the `_INGEST_QUOTA` default and a stream whose rows cost more than one draws its weight through `tokens` on the intent's `quota`, so an oversized batch prices itself instead of a second limiter growing beside the first; the bucket key stays the owner's, because a caller keying its own bucket escapes the per-app isolation this quota exists to hold.
- Law: a quota refusal folds off the limiter's OWN row, never its message — the class hard-codes that message to one constant string, so a fold reading it publishes a refusal naming neither the bucket it hit nor the wait that clears it; `detail` crosses the key with its limit and remainder while `after` crosses the measured `retryAfter`, the one field the limiter published for recovery, and the arm is live the moment an intent overrides `onExceeded: "fail"` through its own `quota`.
- Law: both limiter refusals answer ONE tag and separate on `reason` — `RateLimiterError` names the rail while `Exceeded` and `StoreError` name the cases, so a fold discriminating on a per-class tag matches nothing and files a quota refusal as a store fault whose wait no caller ever reads.
- Law: the quota hold is measured at the owner — the span between entering `_ingest` and the insert's admission past the limiter projects onto the `olapDeferred` meter tagged `clickhouse`, so token-bucket deferral pressure is dashboard-visible while the limiter's suspend disposition stays the behavioral truth.
- Law: the cluster is correctness-adjacent — facts replicate IN, and a lost analytical row is a re-replication, never a billing defect; the journal remains the sole truth.

```typescript
import { Clock, Config, type ConfigError, type Layer } from "effect"
import { ClickhouseClient } from "@effect/sql-clickhouse"
import { RateLimiter } from "@effect/experimental"
import { SqlClient, type SqlError, type Statement } from "@effect/sql"
import type { Identity } from "@rasm/core"

const _clickhouse: Layer.Layer<ClickhouseClient.ClickhouseClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  ClickhouseClient.layerConfig({
    url: Config.string("DATA_CLICKHOUSE_URL"),
    password: Config.string("DATA_CLICKHOUSE_PASSWORD"),
  })

type _Quota = Omit<Parameters<Effect.Effect.Success<typeof RateLimiter.makeWithRateLimiter>>[0], "key">

const _INGEST_QUOTA = { algorithm: "token-bucket", limit: 50, onExceeded: "delay", window: "1 second" } as const satisfies _Quota

const _lifted = <A, R>(
  reason: OlapFault["case"]["reason"],
  work: Effect.Effect<A, OlapFault | SqlError.SqlError | RateLimiter.RateLimiterError, R>,
): Effect.Effect<A, OlapFault, R> =>
  Effect.mapError(work, (cause) =>
    cause._tag === "OlapFault"
      ? cause
      : cause._tag === "RateLimiterError" && cause.reason === "Exceeded"
      ? new OlapFault({
        case: { reason, engine: "clickhouse", detail: `<quota:${cause.key} limit=${cause.limit} remaining=${cause.remaining}>` },
        after: Option.fromNullable(cause.retryAfter),
      })
      : OlapFault.at("clickhouse", reason, cause.message))

declare namespace Olap {
  type Ingest = Parameters<ClickhouseClient.ClickhouseClient["insertQuery"]>[0] & {
    readonly app: Identity.App.Key
    readonly queryId: string
    readonly quota?: Partial<_Quota>
    readonly settings: Parameters<ClickhouseClient.ClickhouseClient["withClickhouseSettings"]>[1]
  }
  type Wide = {
    readonly columns: Array.NonEmptyReadonlyArray<string>
    readonly identity: Identity.App
    readonly labels?: Board.Query.Labels
    readonly relation?: Relation
    readonly residence: Residence
    readonly where?: Statement.Fragment
  }
}

const _wide = (row: Olap.Wide) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const held = _RESIDENCES[row.residence]
    const signal = row.relation ?? "traces"
    const relation = yield* Option.match(Option.map(held.relations, (rows) => rows[signal]), {
      onNone: () => Effect.fail(OlapFault.at("clickhouse", "query", `<relation:${signal}>`)),
      onSome: Effect.succeed,
    })
    const attribute = _planed(held, row.identity, relation.plane)
    const matched = Array.filterMap(
      Convention.keys,
      (key) => Option.map(Option.fromNullable(row.labels?.[key]), (value) => sql`${sql.literal(attribute(key))} = ${String(value)}`),
    )
    const [unknown, admitted] = Array.partition(row.columns, (name) => Array.some(_WIDE[signal].columns, ([declared]) => declared === name))
    const projected = yield* Array.match(unknown, {
      onEmpty: () => Effect.succeed(sql.csv(Array.map(admitted, (name) => sql`${sql(name)}`))),
      onNonEmpty: (missing) =>
        Effect.fail(OlapFault.at("clickhouse", "query", `<column:${Array.join(missing, ",")}>`)),
    })
    return yield* _resilient(
      "clickhouse",
      { access: "read", fault: "query" },
      _lifted(
        "query",
        sql`SELECT ${projected} FROM ${sql.literal(`${held.catalog}.${relation.table}`)} WHERE ${
          sql.and([...matched, ...Option.match(Option.fromNullable(row.where), { onNone: () => [], onSome: (held) => [held] })])
        }`,
      ),
    )
  })

const _ingest = (intent: Olap.Ingest) =>
  Effect.gen(function* () {
    const client = yield* ClickhouseClient.ClickhouseClient
    const limit = yield* RateLimiter.makeWithRateLimiter
    const { app, queryId, quota, settings, ...insert } = intent
    const opened = yield* Clock.currentTimeMillis
    return yield* _lifted(
      "query",
      limit({ ..._INGEST_QUOTA, ...quota, key: `olap:ingest:${app}` })(
        Effect.zipRight(
          Effect.flatMap(Clock.currentTimeMillis, (started) => _meter.deferred.clickhouse(Duration.millis(started - opened))),
          _resilient(
            "clickhouse",
            { access: "write", fault: "query" },
            _lifted("query", client.insertQuery(insert).pipe(client.withClickhouseSettings(settings), client.withQueryId(queryId))),
          ),
        ),
      ),
    )
  })
```

## [06]-[FLIGHT]

- Owner: the engine-blind Flight SQL wire — the scoped `FlightSqlClient` over one sealed coordinate, `Olap.flown` as the ONE call entry over a closed intent family, the `_ANSWERS` and `_STREAMS` dispatch halves, the prepared-plan and transaction brackets, `_dataset`'s server-echoed descriptor, `_fanned`'s location-checked endpoint fan, and `_faulted`'s fold of the refusal classes this pin mints onto the lane's own reasons; the server engine stays opaque behind the wire and no per-engine driver enters.
- Packages: `@qualithm/arrow-flight-client` (`createFlightSqlClient`, `FlightClient`, `FlightSqlClient`, `AuthOptions`, `AuthProvider`, `TlsOptions`, `FlightAction`, `FlightClientOptions`, `FlightCriteria`, `FlightData`, `FlightDescriptor`, `FlightDescriptorInput`, `FlightEndpoint`, `FlightInfo`, `PollInfo`, `PreparedStatement`, `SchemaResult`, `Transaction`, `UpdateResult`, `ActionType`, `Result`, `FlightAuthError`/`FlightConnectionError`/`FlightServerError`); `apache-arrow`; `@rasm/core` (`Wire.Transport`); `effect`.
- Entry: a service composes `Olap.flight` once per remote coordinate and calls `Olap.flown(client, intent)` per unit of work; `Fetch` fans every endpoint of a published plan, `Frames` hands the wire's own messages through undecoded, `Put` uploads Arrow frames through `doPut`, `Bound` binds parameters to a server-side plan, `Act` reaches the server's own action vocabulary, and `Olap.transacted` brackets a sequence needing atomicity at the far end.
- Output: reads land as `apache-arrow` `Table` or a `RecordBatch` stream — the same plane `Olap.wire` and `Olap.ingest` carry, so a Flight result reaches the viewer with no re-encoding; `Frames` lands raw `FlightData`, `Bound` and `Update` land the server's own `recordCount`, `Put` streams its upload acknowledgements, and `Plan`, `Poll`, and `Schema` land `FlightInfo`, `PollInfo`, and `SchemaResult` whole so a caller reads schema, endpoints, and progress without executing.
- Growth: a new Flight capability is one `OlapFlown` case with its row in the matching dispatch half; a new zero-argument catalog read is one `_METADATA` row; a new writing case is one `_WRITES` entry; a refusal class the package starts minting is one `_faulted` arm; nothing here grows a second client or a second transport.
- Law: transport is the package's own — one `@connectrpc/connect` stack over `node:http2` carries every RPC, so this lane imports no connect module, mints no channel, and admits no second gRPC client beside it.
- Law: construction is synchronous and lifecycle is `acquireRelease` under `Scope` like every engine row — `createFlightSqlClient` returns the client outright, so the acquire arm is `Effect.try` and the release arm `Effect.sync` over a `void` `close()`; a promise lift on either end invents a suspension the package never has and swallows a disposal fault.
- Law: credential material crosses as `Redacted` and unwraps INSIDE the `authProvider` thunk `_sealed` mints — the bearer token and the handshake password never land on the options record at all, so a coordinate reaching a fault detail, a profile, or a structured log holds a closure rather than material; the client key and its passphrase unwrap at `_sealed` itself because `tls` has no thunk arm, a certificate and a CA chain are public material and cross bare, and that split is what makes the sealed set exactly the four.
- Law: the thunk is also the rotation seam — the package resolves it before the first request and again on every unauthenticated refusal, caching between, so a `Redacted` value re-read from its own source adopts a rotated credential on the next resolve while `authenticate()` adopts it EAGERLY for a lane that already knows the turnover happened; a client torn down and rebuilt under a fresh scope to take a new credential is the shape this thunk deletes.
- Law: the bound is the lane's alone — the package resolves `timeoutMs` onto its options record and applies it to nothing, so the field passes through for the pin that wires it while `_resilient` bounds every answer and `Stream.timeoutFail` bounds every emission; a design resting on the package's own timeout leaves a stalled far end holding its fiber forever.
- Law: an emitting case bounds on IDLE, never on total elapsed — a legitimately long partitioned read outlives any whole-stream budget, so the governor budget bounds the gap between frames and the resulting fault answers once; a total-elapsed bound kills the exact reads the fan exists to serve.
- Law: the refusal fold delegates numeric gRPC status classification to `Wire.Transport`; this lane owns no status roster and never treats a transport status as a remote domain fault.
- Law: the classifier keys on the refusal classes this pin mints — `FlightServerError` carries every transport and far-end verdict because the package's own auth branch compares `code` against status NAMES while ConnectRPC hands it the numeric `Code`, `FlightAuthError` reaches only a handshake answering nothing, and `FlightConnectionError` only a raw socket error carrying no `code`; `FlightTimeoutError` and `FlightCancelledError` are exported and never thrown, so an arm for either is dead the day it lands.
- Law: `FlightServerError.code` is declared `string` and populated numerically, so `Number.parse` is the sole partial admission. An absent code is connectivity; denied statuses route credential recovery; all other statuses remain transport outcomes.
- Law: arm order IS dispatch order — every subclass precedes the base, because `FlightError.isError` answers true for each of them and a base-first ladder swallows every arm whose recovery genuinely differs.
- Law: dispatch is two mapped halves over one closed family — `_ANSWERS` keys every single-value case and `_STREAMS` every emitting case, each total over its own key set, so a new case fails at the record that must hold it rather than falling into a residue arm; the answer half rides `_resilient`, the emitting half never does, because a partial-output replay duplicates rows the consumer already took.
- Law: replay admits a read alone — `_WRITES` names the two cases that land rows at a far end this lane cannot roll back, so a re-sent DML statement is unspellable while every answering read shares one budget and one backoff.
- Law: the upload descriptor is read back, never assembled — `doPut` reads it off the first frame as a protobuf message whose schema value the package keeps behind an unexported subpath, so `_dataset` takes the plain `FlightDescriptorInput` shape to `getFlightInfo` and stamps the message the server itself echoed; a caller already holding that message hands it back and the echo is skipped on its own `$typeName`, so a bulk run pays one plan read rather than one per frame set.
- Law: the whole flight is every endpoint consumed — a partitioned plan publishes one endpoint per split and `FlightInfo.ordered` is the server's own statement that the splits carry a sequence, so it decides the fan width; a reader taking the first endpoint alone silently returns a fraction of the result.
- Law: a split this lane cannot redeem FAILS the fan — an endpoint whose location roster names foreign services carries a ticket only those services honor, and an endpoint publishing no ticket at all answers no `doGet`, so each raises rather than contributing nothing; skipping either returns a fraction of the result under a success exit, which is the defect the whole-flight law already names.
- Law: the empty location roster and the `arrow-flight-reuse-connection://?` sentinel are the two spellings of "redeem where the ticket was minted", so both admit the held client while every other authority refuses by URI — this lane holds one coordinate and one credential set, and a server publishing per-node locations earns a coordinate per node at the composition root, where the credential set is already decided.
- Law: prepared statements and transactions are server-side resources their brackets own — a plan closes on every exit, a streamed plan lives exactly as long as its stream through `Stream.acquireRelease`, and the commit runs on the USE arm so a failed commit lands on the typed rail and its rollback still runs, where a release-arm commit only dies.
- Law: the wire is a read and ingest surface, never a record of truth — a Flight result is correctness-adjacent evidence and a lost row is a re-read; the journal keeps sole authority and nothing folds back through this wire as truth.
- Law: streamed reads never materialize — `queryBatches`, `queryStream`, `doGet`, and `executePreparedStream` each cross in `RecordBatch` or raw `FlightData` grain through `Olap.wire.flight`, so a result larger than memory rides the same one wire every engine seam requires.
- Boundary: a Flight result crosses column-untyped — the package's `TypeMap` parameter is an unchecked phantom it never validates, so threading it here publishes a claim the wire cannot make; a consumer needing typed columns decodes through its own `Schema` at its own boundary.
- Boundary: `@qualithm/arrow-flight-client` throws and returns promises; `_flew` is the only crossing and `_faulted` the only classifier, so above this cluster the lane is rails end to end.

```typescript
import { Exit, Match, Number, Redacted } from "effect"
import type { RecordBatch, Table } from "apache-arrow"
import { Wire } from "@rasm/core"
import {
  createFlightSqlClient,
  FlightAuthError,
  FlightConnectionError,
  FlightServerError,
  type ActionType,
  type AuthOptions,
  type AuthProvider,
  type FlightAction,
  type FlightClient,
  type FlightClientOptions,
  type FlightCriteria,
  type FlightData,
  type FlightDescriptor,
  type FlightDescriptorInput,
  type FlightEndpoint,
  type FlightInfo,
  type FlightSqlClient,
  type PollInfo,
  type PreparedStatement,
  type Result,
  type SchemaResult,
  type TlsOptions,
  type Transaction,
  type UpdateResult,
} from "@qualithm/arrow-flight-client"

const _transported = (code: number): OlapFault["case"]["reason"] =>
  Wire.Transport.denied(code)
    ? "secret"
    : Wire.Transport.kindOf(code) === "connectivity" ? "acquire" : "wire"

const _faulted = Match.type<unknown>().pipe(
  Match.when(FlightConnectionError.isError, () => "acquire" as const),
  Match.when(FlightAuthError.isError, () => "secret" as const),
  Match.when(FlightServerError.isError, ({ code }) =>
    Option.match(Number.parse(code), {
      onNone: () => "acquire" as const,
      onSome: _transported,
    })),
  Match.orElse(() => "wire" as const),
)

const _detailed = (cause: unknown): string =>
  FlightServerError.isError(cause) && cause.details !== undefined ? `${String(cause)} ${cause.details}` : String(cause)

const _flightFault = (cause: unknown): OlapFault =>
  OlapFault.at("flight", _faulted(cause), _detailed(cause))

const _flew = <A>(run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: _flightFault })

const _present = <T extends Record.ReadonlyRecord<string, unknown>>(row: T): { readonly [K in keyof T]?: NonNullable<T[K]> } =>
  Record.filter(row, (value) => value !== undefined) as { readonly [K in keyof T]?: NonNullable<T[K]> }

declare namespace Olap {
  type Auth =
    | { readonly type: "bearer"; readonly token: Redacted.Redacted<string> }
    | { readonly type: "basic"; readonly credentials: { readonly password: Redacted.Redacted<string>; readonly username: string } }
    | { readonly type: "none" }
  type Tls = Omit<TlsOptions, "key" | "passphrase"> & {
    readonly key?: Redacted.Redacted<string | Buffer>
    readonly passphrase?: Redacted.Redacted<string>
  }
  type Flight = Omit<FlightClientOptions, "auth" | "authProvider" | "tls"> & { readonly auth?: Auth; readonly tls?: Tls }
  type Dataset = FlightDescriptorInput
  type Prepared = PreparedStatement
  type Update = UpdateResult
  type Ack = ReturnType<FlightClient["doPut"]> extends AsyncIterable<infer P> ? P : never
}

const _FLIGHT = { endpoints: 4, timeoutMs: Duration.toMillis(_GOVERNOR.budget) } as const

const _authed = Match.type<Olap.Auth>().pipe(
  Match.when({ type: "bearer" }, ({ token }) => ({ type: "bearer", token: Redacted.value(token) }) satisfies AuthOptions),
  Match.when({ type: "basic" }, ({ credentials }) =>
    ({
      type: "basic",
      credentials: { password: Redacted.value(credentials.password), username: credentials.username },
    }) satisfies AuthOptions),
  Match.orElse(() => ({ type: "none" }) satisfies AuthOptions),
)

const _tls = ({ key, passphrase, ...rest }: Olap.Tls): TlsOptions => ({
  ...rest,
  ..._present({
    key: Option.getOrUndefined(Option.map(Option.fromNullable(key), Redacted.value)),
    passphrase: Option.getOrUndefined(Option.map(Option.fromNullable(passphrase), Redacted.value)),
  }),
})

const _sealed = ({ auth, tls, ...coordinate }: Olap.Flight): FlightClientOptions => ({
  timeoutMs: _FLIGHT.timeoutMs,
  ...coordinate,
  ..._present({
    authProvider: Option.getOrUndefined(Option.map(Option.fromNullable(auth), (held): AuthProvider => () => _authed(held))),
    tls: Option.getOrUndefined(Option.map(Option.fromNullable(tls), _tls)),
  }),
})

const _flight = (coordinate: Olap.Flight): Effect.Effect<FlightSqlClient, OlapFault, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.try({ try: () => createFlightSqlClient(_sealed(coordinate)), catch: _flightFault }),
    (held) => Effect.sync(() => held.close()),
  )

const _METADATA = {
  catalogs: (client: FlightSqlClient) => client.getCatalogs(),
  tableTypes: (client: FlightSqlClient) => client.getTableTypes(),
} as const satisfies Record.ReadonlyRecord<string, (client: FlightSqlClient) => Promise<Table>>

type OlapFlown = Data.TaggedEnum<{
  Act: { readonly action: FlightAction }
  Actions: Record<string, never>
  Batches: { readonly sql: string; readonly transaction?: Transaction }
  Bound: {
    readonly binds: AsyncIterable<RecordBatch> | Iterable<RecordBatch>
    readonly sql: string
    readonly transaction?: Transaction
  }
  Catalogs: { readonly kind: keyof typeof _METADATA }
  Fetch: { readonly dataset: Olap.Dataset }
  Flights: { readonly criteria?: FlightCriteria }
  Frames: { readonly sql: string; readonly transaction?: Transaction }
  Keys: { readonly catalog?: string; readonly schema?: string; readonly table: string }
  Plan: { readonly sql: string; readonly transaction?: Transaction }
  Poll: { readonly dataset: Olap.Dataset }
  Prepared: { readonly sql: string; readonly transaction?: Transaction }
  Put: { readonly dataset: Olap.Dataset | FlightDescriptor; readonly source: Olap.Frame }
  Query: { readonly sql: string; readonly transaction?: Transaction }
  Schema: { readonly dataset: Olap.Dataset }
  Schemas: { readonly catalog?: string; readonly schema?: string }
  Streamed: { readonly sql: string; readonly transaction?: Transaction }
  Tables: {
    readonly catalog?: string
    readonly columns?: boolean
    readonly kinds?: ReadonlyArray<string>
    readonly schema?: string
    readonly table?: string
  }
  Update: { readonly sql: string; readonly transaction?: Transaction }
}>

const _Flown = Data.taggedEnum<OlapFlown>()

type _Answered = {
  Bound: Olap.Update
  Catalogs: Table
  Keys: Table
  Plan: FlightInfo
  Poll: PollInfo
  Prepared: Table
  Query: Table
  Schema: SchemaResult
  Schemas: Table
  Tables: Table
  Update: Olap.Update
}

type _Emitted = {
  Act: Result
  Actions: ActionType
  Batches: RecordBatch
  Fetch: RecordBatch
  Flights: FlightInfo
  Frames: FlightData
  Put: Olap.Ack
  Streamed: RecordBatch
}

const _WRITES: ReadonlyArray<keyof _Answered> = ["Bound", "Update"]

const _planned = <A>(
  client: FlightSqlClient,
  row: { readonly sql: string; readonly transaction?: Transaction },
  use: (statement: PreparedStatement) => Effect.Effect<A, OlapFault>,
): Effect.Effect<A, OlapFault> =>
  Effect.acquireUseRelease(
    _flew(() => client.prepare(row.sql, row.transaction?.id)),
    use,
    (statement) => Effect.orDie(_flew(() => client.closePreparedStatement(statement))),
  )

const _dataset = (client: FlightSqlClient, dataset: Olap.Dataset | FlightDescriptor): Effect.Effect<FlightDescriptor, OlapFault> =>
  "$typeName" in dataset
    ? Effect.succeed(dataset)
    : Effect.flatMap(
      _flew(() => client.flight.getFlightInfo(dataset)),
      (info) =>
        Option.match(Option.fromNullable(info.flightDescriptor), {
          onNone: () => Effect.fail(OlapFault.at("flight", "wire", "<no-descriptor>")),
          onSome: Effect.succeed,
        }),
    )

const _REUSE = ["", "arrow-flight-reuse-connection://?"] as const

const _reused = (endpoint: FlightEndpoint): boolean =>
  Array.length(endpoint.location) === 0 || Array.some(endpoint.location, (held) => Array.contains(_REUSE, held.uri))

const _fanned = (client: FlightSqlClient, info: FlightInfo): Stream.Stream<RecordBatch, OlapFault> =>
  Stream.flatMap(
    Stream.fromIterable(info.endpoint),
    (endpoint) =>
      _reused(endpoint)
        ? Option.match(Option.fromNullable(endpoint.ticket), {
          onNone: () => Stream.fail(OlapFault.at("flight", "wire", "<endpoint:no-ticket>")),
          onSome: (ticket) => _wire.flight.streamed(client.flight.doGet(ticket)),
        })
        : Stream.fail(
          OlapFault.at("flight", "wire", `<endpoint:located ${Array.join(Array.map(endpoint.location, (held) => held.uri), " ")}>`),
        ),
    { concurrency: info.ordered ? 1 : _FLIGHT.endpoints },
  )

const _ANSWERS: {
  readonly [K in keyof _Answered]: (
    client: FlightSqlClient,
    intent: Extract<OlapFlown, { readonly _tag: K }>,
  ) => Effect.Effect<_Answered[K], OlapFault>
} = {
  Bound: (client, { binds, sql, transaction }) =>
    _planned(client, { sql, transaction }, (statement) => _flew(() => client.executePreparedUpdate(statement, binds))),
  Catalogs: (client, { kind }) => _flew(() => _METADATA[kind](client)),
  Keys: (client, { catalog, schema, table }) => _flew(() => client.getPrimaryKeys(table, _present({ catalog, dbSchema: schema }))),
  Plan: (client, { sql, transaction }) => _flew(() => client.getQueryInfo(sql, _present({ transactionId: transaction?.id }))),
  Poll: (client, { dataset }) => _flew(() => client.flight.pollFlightInfo(dataset)),
  Prepared: (client, { sql, transaction }) =>
    _planned(client, { sql, transaction }, (statement) => _flew(() => client.executePrepared(statement))),
  Query: (client, { sql, transaction }) => _flew(() => client.query(sql, _present({ transactionId: transaction?.id }))),
  Schema: (client, { dataset }) => _flew(() => client.flight.getSchema(dataset)),
  Schemas: (client, { catalog, schema }) => _flew(() => client.getDbSchemas(_present({ catalog, dbSchemaFilterPattern: schema }))),
  Tables: (client, { catalog, columns, kinds, schema, table }) =>
    _flew(() =>
      client.getTables(_present({
        catalog,
        dbSchemaFilterPattern: schema,
        includeSchema: columns,
        tableNameFilterPattern: table,
        tableTypes: kinds === undefined ? undefined : [...kinds],
      }))
    ),
  Update: (client, { sql, transaction }) => _flew(() => client.executeUpdate(sql, _present({ transactionId: transaction?.id }))),
}

const _STREAMS: {
  readonly [K in keyof _Emitted]: (
    client: FlightSqlClient,
    intent: Extract<OlapFlown, { readonly _tag: K }>,
  ) => Stream.Stream<_Emitted[K], OlapFault>
} = {
  Act: (client, { action }) => Stream.fromAsyncIterable(client.flight.doAction(action), _flightFault),
  Actions: (client) => Stream.fromAsyncIterable(client.flight.listActions(), _flightFault),
  Batches: (client, { sql, transaction }) =>
    Stream.fromAsyncIterable(client.queryBatches(sql, _present({ transactionId: transaction?.id })), _flightFault),
  Fetch: (client, { dataset }) =>
    Stream.unwrap(Effect.map(_flew(() => client.flight.getFlightInfo(dataset)), (info) => _fanned(client, info))),
  Flights: (client, { criteria }) => Stream.fromAsyncIterable(client.flight.listFlights(criteria), _flightFault),
  Frames: (client, { sql, transaction }) =>
    Stream.fromAsyncIterable(client.queryStream(sql, _present({ transactionId: transaction?.id })), _flightFault),
  Put: (client, { dataset, source }) =>
    Stream.unwrap(Effect.map(
      _dataset(client, dataset),
      (descriptor) => Stream.fromAsyncIterable(client.flight.doPut(_wire.flight.framed(source, descriptor)), _flightFault),
    )),
  Streamed: (client, { sql, transaction }) =>
    Stream.flatMap(
      Stream.acquireRelease(
        _flew(() => client.prepare(sql, transaction?.id)),
        (statement) => Effect.orDie(_flew(() => client.closePreparedStatement(statement))),
      ),
      (statement) => _wire.flight.streamed(client.executePreparedStream(statement)),
    ),
}

function _flown<K extends keyof _Answered>(
  client: FlightSqlClient,
  intent: Extract<OlapFlown, { readonly _tag: K }>,
): Effect.Effect<_Answered[K], OlapFault>
function _flown<K extends keyof _Emitted>(
  client: FlightSqlClient,
  intent: Extract<OlapFlown, { readonly _tag: K }>,
): Stream.Stream<_Emitted[K], OlapFault>
function _flown(
  client: FlightSqlClient,
  intent: OlapFlown,
): Effect.Effect<_Answered[keyof _Answered], OlapFault> | Stream.Stream<_Emitted[keyof _Emitted], OlapFault> {
  return Record.has(_STREAMS, intent._tag)
    ? Stream.timeoutFail(
      (_STREAMS[intent._tag as keyof _Emitted] as (
        client: FlightSqlClient,
        intent: OlapFlown,
      ) => Stream.Stream<_Emitted[keyof _Emitted], OlapFault>)(client, intent),
      () => OlapFault.at("flight", "wire", "<idle-budget>"),
      _GOVERNOR.budget,
    )
    : _resilient(
      "flight",
      { access: Array.contains(_WRITES, intent._tag as keyof _Answered) ? "write" : "read", fault: "query" },
      (_ANSWERS[intent._tag as keyof _Answered] as (
        client: FlightSqlClient,
        intent: OlapFlown,
      ) => Effect.Effect<_Answered[keyof _Answered], OlapFault>)(client, intent),
    )
}

const _transacted = <A, E, R>(
  client: FlightSqlClient,
  work: (transaction: Transaction) => Effect.Effect<A, E, R>,
): Effect.Effect<A, E | OlapFault, R> =>
  Effect.acquireUseRelease(
    _flew(() => client.beginTransaction()),
    (transaction) => Effect.tap(work(transaction), () => _flew(() => client.commit(transaction))),
    (transaction, exit) => Exit.isSuccess(exit) ? Effect.void : Effect.orDie(_flew(() => client.rollback(transaction))),
  )
```

## [07]-[PROFILE]

- Owner: the profile band across every engine this lane reaches — `_profile` is the ONE entry, leasing a disposable session and holding one permit across enable, one-shot `EXPLAIN ANALYZE`, and teardown for an embedded source, and reading the cluster's own log for a source naming the id its statement already ran under; `_probe` folds bounded serial measurements; `_armed` compares evidence against the engine trigger; `Olap` assembles the export.
- Packages: `./postgres.ts` (`Pg.Profile` — the shared profile schema; parity across pg, sqlite, and this lane is one class); `@effect/sql` (`SqlClient`); `@effect/sql-clickhouse` (`ClickhouseClient`, `asCommand`); `effect` (`Array`, `Exit`, `Option`, `Order`, `Record`, `Schema`, `pipe`).
- Entry: an explicit diagnosis call runs `Olap.profile({ handle, label, statement })` against an embedded handle or `Olap.profile({ label, queryId })` against the cluster; the maintenance composition runs `Olap.probe` in idle windows per the budget row, holds the prior evidence beside the engine row, and folds `Olap.armed` — the composition seam folds `priced` once, so an unpriceable incumbent fans nothing and an armed reading hands the `rasm.data.lane.escalate` fact its delta out of the same arm that armed it, which is why that point declares `delta` required; this lane keeps its single `ObjectStore` value seam beside the profile-schema read and never imports the hook registry.
- Output: `Pg.Profile` with `engine: "duckdbNode"`, operator rows carrying timing and cardinality from the plan tree, and `counters` carrying every `_COUNTERS` measure the root reported; `Olap.Evidence` — `{ engine, statement, runs, wallP50, wallMax, rows }`; `Olap.Escalation` — `{ candidate, trigger, priced }` whose `priced` carries `{ armed, delta }` where the incumbent's p50 admits a ratio and nothing where it does not, the row's trigger text riding as data so the review argues from the table.
- Growth: a profile engine enters `_PROFILE_ENGINES` only with its landed harvest arm; a probe budget posture is a `_PROBE` field override; a new profile counter is one `_COUNTERS` row whose decode field and profile key both follow.
- Law: profiling toggles are per-connection state — one permit spans enable, execution, and disable, so concurrent users cannot interleave inside the bracket; `_profileRowsOnce` bypasses the retry governor because `EXPLAIN ANALYZE` executes its statement; the `access: "read"` statement case is the only admitted diagnosis input; disable failure remains on the typed rail, and the enclosing scope disposes the leased session on every exit.
- Law: under `enable_profiling='json'` the result is ONE row of two `VARCHAR` cells — `explain_key` reading `analyzed_plan` and `explain_value` carrying the profile — so the harvest decodes that one string through `Schema.parseJson`; root latency and the plan root's own cardinality are required profile columns and each absence fails the wire, while operator measures and the counter roster stay `Option`-carried so a missing measure is omission rather than a forged zero.
- Law: BOTH embedded drivers profile — the harvest runs through `_DRIVERS.bounded` and `_ROWED` normalizes the two answers, so the browser lane fills the same `Pg.Profile` band the node lane does and a band tile reads one comparable profile whichever driver produced it.
- Law: the worker cell reads by NAME off the Arrow schema, never by position — `enable_profiling` fixes the column names and nothing fixes their order, so a positional read forges whichever cell the engine happened to emit first.
- Law: returned rows read the plan root BENEATH the harvest's own `EXPLAIN_ANALYZE` operator — the profile root reports the OUTER statement, whose returned value is the plan text, so its `rows_returned` reads zero on every harvest and an operator walk starting above the wrapper credits the statement with the harvest's own machinery.
- Law: the counter roster is the one owner of root evidence — decode fields and profile keys both project from `_COUNTERS`, so an engine's added measure lands as a row and no profile grows a hand-spelled arm that silently disagrees with the schema beside it.
- Law: probes run beside production lanes — `_PROBE.runs` bounds the repetition, the run rides the same governed session gate as every statement so a probe cannot starve live work, the whole profile bracket (enable, EXPLAIN ANALYZE, disable, release) rides one `_GOVERNOR.budget` timeout so a stalled diagnosis releases its permit, and each harvest's wall span projects onto the census `profileDuration` histogram tagged `Convention.rasm.profileEngine` — the row codes its unit milliseconds, so the tap takes a `Duration` and the row's own scale carries it, and embedded engines expose no scrape surface, so this harvest is their whole observability.
- Law: escalation is evidence-driven row data — `_armed` compares `Olap.Evidence` values by their p50 wall ratio against the `_PROBE.floor`, names the CANDIDATE row's own trigger text, and never mutates the row; the evidence carries the incumbent that produced it, so the verdict reads as an argument for a move rather than a claim about the engine measured, and admitting ClickHouse below its trigger remains the named operational waste the table refuses.
- Law: an unpriceable incumbent is the verdict's own absent arm — a ratio needs a denominator the probe measured, so a zero-span incumbent leaves `priced` empty and the review reads "too fast to price on this axis" as evidence; clamping the denominator to a floor prices every sub-millisecond incumbent as if it took that floor, which arms a move against a number the measurement never produced and is the same forgery as a zero-filled profile field.
- Law: the cluster publishes no result-side profile at all — the driver answers rows for a read and its own insert result for an ingest — so cluster evidence is a SECOND read of `system.query_log` keyed by the statement's id, gated on the flush that makes the asynchronous log deterministic; a design expecting cost on the answer reads a surface the driver never carries.
- Law: a query id scopes exactly ONE statement — `withQueryId` binds a `FiberRef` for its whole effect, so a scope wrapping several statements files them under one log key and hands the driver's own interrupt arm a `KILL QUERY` that reaches every sibling sharing the id; `Olap.Ingest` carrying `queryId` per intent is what keeps the scope honest, and an id minted above a batch loses both the attribution and the isolation.
- Law: the harvest never re-runs the measured statement — the embedded arm profiles the one `EXPLAIN ANALYZE` execution and the cluster arm reads a log row for an execution that already happened, so a profile costs a plan decode or a log read, never a second run of the query whose cost is the subject.
- Law: counter vocabulary is shared across engines — `_COUNTERS` and `_LOGGED` key their own wire fields onto ONE profile-key set, so a band tile reads `bytesRead` or `resultSetSize` whichever engine answered and no consumer branches on the producer to find a measure.

```typescript
import { Pg } from "./postgres.ts"
import { Array, Exit, Number, Option, Order, pipe, Record, Schema } from "effect"
import { SqlClient } from "@effect/sql"
import { ClickhouseClient } from "@effect/sql-clickhouse"

const _PROBE = { runs: 5, floor: 1.5 } as const

const _WRAPPED = { cell: "explain_value", wrapper: "EXPLAIN_ANALYZE" } as const

const _COUNTERS = {
  blocked_thread_time: "blockedThreadTime",
  cpu_time: "cpuTime",
  cumulative_cardinality: "cumulativeCardinality",
  cumulative_rows_scanned: "cumulativeRowsScanned",
  result_set_size: "resultSetSize",
  system_peak_buffer_memory: "peakBufferMemory",
  system_peak_temp_dir_size: "peakTempDirSize",
  total_bytes_read: "bytesRead",
  total_bytes_written: "bytesWritten",
  total_memory_allocated: "memoryAllocated",
} as const

interface _ProfileTreeEncoded {
  readonly operator_name?: string
  readonly operator_type?: string
  readonly operator_timing?: number
  readonly operator_cardinality?: number
  readonly children?: ReadonlyArray<_ProfileTreeEncoded>
}

interface _ProfileTree {
  readonly operator_name: Option.Option<string>
  readonly operator_type: Option.Option<string>
  readonly operator_timing: Option.Option<number>
  readonly operator_cardinality: Option.Option<number>
  readonly children: Option.Option<ReadonlyArray<_ProfileTree>>
}

const _Tree: Schema.Schema<_ProfileTree, _ProfileTreeEncoded> = Schema.Struct({
  operator_name: Schema.optionalWith(Schema.String, { as: "Option" }),
  operator_type: Schema.optionalWith(Schema.String, { as: "Option" }),
  operator_timing: Schema.optionalWith(Schema.Number, { as: "Option" }),
  operator_cardinality: Schema.optionalWith(Schema.Number, { as: "Option" }),
  children: Schema.optionalWith(Schema.Array(Schema.suspend((): Schema.Schema<_ProfileTree, _ProfileTreeEncoded> => _Tree)), { as: "Option" }),
})

const _Root = Schema.Struct({
  children: Schema.optionalWith(Schema.Array(_Tree), { as: "Option" }),
  latency: Schema.Number,
  query_name: Schema.optionalWith(Schema.String, { as: "Option" }),
  ...Record.map(_COUNTERS, () => Schema.optionalWith(Schema.Number, { as: "Option" })),
})

const _planRoot = (root: typeof _Root.Type): Option.Option<_ProfileTree> =>
  Option.flatMap(Option.flatMap(Option.flatMap(root.children, Array.head), (wrapper) => wrapper.children), Array.head)

const _steps = (node: _ProfileTree): ReadonlyArray<Pg.Profile["operators"][number]> => [
  ...Option.match(Option.orElse(node.operator_name, () => node.operator_type), {
    onNone: () => [],
    onSome: (name) => [{
      name,
      millis: Option.map(node.operator_timing, (seconds) => seconds * 1000),
      rows: Option.map(node.operator_cardinality, Math.trunc),
    }],
  }),
  ...Option.match(node.children, { onNone: () => [], onSome: Array.flatMap(_steps) }),
]

const _countered = (root: typeof _Root.Type): Record.ReadonlyRecord<string, number> =>
  Record.fromEntries(
    Array.filterMap(Record.toEntries(_COUNTERS), ([wire, key]) => Option.map(root[wire], (value) => [key, value] as const)),
  )

type _ReadStatement = _Statement & { readonly access: "read" }

declare namespace Olap {
  type ProfileEngine = Extract<Embedded, Pg.ProfileEngine>
}

const _ROWED: { readonly [E in Olap.Embedded]: (bounded: Olap.Bounded<E>) => ReadonlyArray<Olap.Row> } = {
  duckdbNode: (bounded) => bounded,
  duckdbWasm: (bounded) =>
    Array.makeBy(bounded.numRows, (index) =>
      Record.fromEntries(
        Array.map(bounded.schema.fields, (field) => [field.name, bounded.getChild(field.name)?.get(index) ?? null] as const),
      )),
}

const _profileRowsOnce = <E extends Olap.ProfileEngine>(
  session: Olap.Session<E>,
  statement: _Statement,
): Effect.Effect<ReadonlyArray<Olap.Row>, OlapFault, Scope.Scope> =>
  Effect.map(_DRIVERS[session.engine].bounded(session, statement), _ROWED[session.engine])

const _required = <A>(engine: Olap.Engine, detail: string) => (held: Option.Option<A>): Effect.Effect<A, OlapFault> =>
  Option.match(held, { onNone: () => Effect.fail(OlapFault.at(engine, "wire", detail)), onSome: Effect.succeed })

const _profileOnce = <E extends Olap.ProfileEngine>(
  session: Olap.Session<E>,
  statement: _ReadStatement,
  label: string,
): Effect.Effect<Pg.Profile, OlapFault, Scope.Scope> =>
  Effect.acquireUseRelease(
    Effect.tap(Effect.timed(session.gate.take(1)), ([span]) => _meter.waited[session.engine](span)),
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
        const cell = yield* pipe(
          Option.flatMap(Array.head(raw), (row) => Option.fromNullable(row[_WRAPPED.cell])),
          Option.map(String),
          _required(session.engine, "<no-profile-cell>"),
        )
        const tree = yield* Schema.decodeUnknown(Schema.parseJson(_Root))(cell).pipe(
          Effect.mapError((fault) => OlapFault.at(session.engine, "wire", String(fault))),
        )
        const plan = yield* pipe(_planRoot(tree), _required(session.engine, `<no-plan-root:${_WRAPPED.wrapper}>`))
        const rows = yield* pipe(Option.map(plan.operator_cardinality, Math.trunc), _required(session.engine, "<no-cardinality>"))
        const wallMillis = tree.latency * 1000
        yield* _meter.profiled[session.engine](Duration.millis(wallMillis))
        return new Pg.Profile({
          engine: session.engine,
          statement: label,
          wallMillis,
          rows,
          operators: _steps(plan),
          counters: _countered(tree),
          window: Option.none(),
        })
      }),
    () => session.gate.release(1),
  ).pipe(
    Effect.timeoutFail({
      duration: _GOVERNOR.budget,
      onTimeout: () => OlapFault.at(session.engine, statement.fault, _ABANDONED.profile),
    }),
    Effect.tapError((fault) => _abandoned(fault) ? session.evict : Effect.void),
  )

// --- [CLUSTER_HARVEST]

const _LOGGED = {
  memory_usage: "memoryAllocated",
  read_bytes: "bytesRead",
  read_rows: "cumulativeRowsScanned",
  result_bytes: "resultSetSize",
  written_bytes: "bytesWritten",
} as const

const _Logged = Schema.Struct({
  query_duration_ms: Schema.Number,
  result_rows: Schema.Number,
  ...Record.map(_LOGGED, () => Schema.Number),
})

const _harvested = (row: { readonly label: string; readonly queryId: string }): Effect.Effect<
  Pg.Profile,
  OlapFault,
  ClickhouseClient.ClickhouseClient | SqlClient.SqlClient
> =>
  Effect.gen(function* () {
    const client = yield* ClickhouseClient.ClickhouseClient
    const sql = yield* SqlClient.SqlClient
    yield* _lifted("query", client.asCommand(sql`SYSTEM FLUSH LOGS query_log`))
    const answered = yield* _lifted(
      "query",
      sql`SELECT ${sql.literal(Array.join(Record.keys(_Logged.fields), ", "))} FROM system.query_log
          WHERE query_id = ${row.queryId} AND type = 'QueryFinish' ORDER BY event_time_microseconds DESC LIMIT 1`,
    )
    const logged = yield* pipe(
      Array.head(answered),
      _required("clickhouse", `<no-query-log:${row.queryId}>`),
      Effect.flatMap((held) =>
        Schema.decodeUnknown(_Logged)(held).pipe(
          Effect.mapError((fault) => OlapFault.at("clickhouse", "wire", String(fault))),
        )
      ),
    )
    yield* _meter.profiled.clickhouse(Duration.millis(logged.query_duration_ms))
    return new Pg.Profile({
      engine: "clickhouse",
      statement: row.label,
      wallMillis: logged.query_duration_ms,
      rows: Math.trunc(logged.result_rows),
      operators: [],
      counters: Record.fromEntries(Array.map(Record.toEntries(_LOGGED), ([wire, key]) => [key, logged[wire]] as const)),
      window: Option.none(),
    })
  })

function _profile<E extends Olap.ProfileEngine>(
  source: { readonly handle: Olap.Handle<E>; readonly label: string; readonly statement: _ReadStatement },
): Effect.Effect<Pg.Profile, OlapFault>
function _profile(
  source: { readonly label: string; readonly queryId: string },
): Effect.Effect<Pg.Profile, OlapFault, ClickhouseClient.ClickhouseClient | SqlClient.SqlClient>
function _profile(
  source:
    | { readonly handle: Olap.Handle<Olap.ProfileEngine>; readonly label: string; readonly statement: _ReadStatement }
    | { readonly label: string; readonly queryId: string },
): Effect.Effect<Pg.Profile, OlapFault, ClickhouseClient.ClickhouseClient | SqlClient.SqlClient> {
  return "handle" in source
    ? Effect.scoped(Effect.flatMap(source.handle.lease, (session) => _profileOnce(session, source.statement, source.label)))
    : _harvested(source)
}

declare namespace Olap {
  type Evidence = {
    readonly engine: ProfileEngine
    readonly statement: string
    readonly runs: number
    readonly wallP50: number
    readonly wallMax: number
    readonly rows: number
  }
  type Escalation = {
    readonly candidate: Engine
    readonly trigger: string
    readonly priced: Option.Option<{ readonly armed: boolean; readonly delta: number }>
  }
}

const _probe = <E extends Olap.ProfileEngine>(
  handle: Olap.Handle<E>,
  statement: _ReadStatement,
  label: string,
): Effect.Effect<Olap.Evidence, OlapFault> =>
  Effect.map(
    Effect.forEach(Array.range(1, _PROBE.runs), () => _profile({ handle, label, statement }), { concurrency: 1 }),
    (profiles) =>
      pipe(Array.sort(Array.map(profiles, (profile) => profile.wallMillis), Order.number), (walls) => ({
        engine: handle.engine,
        statement: label,
        runs: profiles.length,
        wallP50: Option.getOrElse(Array.get(walls, Math.trunc(walls.length / 2)), () => Array.lastNonEmpty(walls)),
        wallMax: Array.lastNonEmpty(walls),
        rows: Array.headNonEmpty(profiles).rows,
      })),
  )

const _armed = (candidate: Olap.Engine, prior: Olap.Evidence, next: Olap.Evidence): Olap.Escalation => ({
  candidate,
  trigger: _engines[candidate].trigger,
  priced: Option.map(Number.divide(next.wallP50, prior.wallP50), (delta) => ({ armed: delta >= _PROBE.floor, delta })),
})
```

## [08]-[ARROW_WIRE]

- Owner: the one columnar interchange — `Olap.wire.roster`'s schema-declared landing, the IPC codec pair, the Flight frame codec pair over one grain adapter, `Olap.lake`'s engine-free Parquet codec at rest, bounded batch streaming, the worker ingest entry, and the assembled `Olap` export.
- Packages: `apache-arrow` (`Builder`, `Field`, `makeBuilder`, `RecordBatch`, `RecordBatchReader`, `Schema`, `Table`, `tableFromIPC`, `tableToIPC`); `@qualithm/arrow-flight-client` (`createFlightDataFromIpc`, `decodeFlightDataStream`, `decodeFlightDataToTable`, `encodeRecordBatchesToFlightData`, `encodeTableToFlightData`, `getSchemaFromFlightData`); `parquet-wasm` (`readParquet`, `readSchema`, `writeParquet`, `ParquetFile.fromUrl`/`.fromFile`/`.stream`/`.free`, `Table.fromIPCStream`, `RecordBatch.intoIPCStream`, `Schema.intoIPCStream`, `WriterPropertiesBuilder`, `WriterProperties`, `Compression`, `EnabledStatistics`, `ReaderOptions`); `@duckdb/duckdb-wasm` (`conn.insertArrowTable`, `conn.insertArrowFromIPCStream`); `effect` (`Sink`); `object/store.ts` (`ObjectStore.Landed`, `put`, `refer`, `ObjectStore.owner`) and `journal/retain.ts` (`Retain.Class`) close the landing.
- Entry: node reads land as row objects and worker reads as Arrow, so only ClickHouse output and foreign IPC cross `tableFromIPC`; every Flight read and upload crosses `_wire.flight`; a lake object decodes through `Olap.lake.read` or streams through `Olap.lake.batches`, and `Olap.lake.write`/`.sink` land each object through `object/store#CONDITIONAL` and answer its `ObjectStore.Landed`; the viewer's geoarrow plane consumes the same Tables downstream.
- Growth: a new engine row joins the wire by emitting or accepting IPC — no per-engine result shape is ever admitted; a landed column set is one `[name, token]` declaration and nothing else; a new frame source is one `_frames` arm; a writer economics posture is a `_PARQUET` override, never a per-call flag list.
- Law: one wire — an analytical result crossing any engine seam travels as Arrow; a JSON or row-object re-encoding between engines is the named defect, and the only row-shaped egress is the final consumer projection.
- Law: a row set reaches this wire through its own DECLARATION and never a hand-assembled container — `Olap.wire.roster(columns)` is the ONE landing, and because its `table` answers an `apache-arrow` `Table` every member already taking one admits a row set unchanged: `wire.encode`, `wire.feed`, `lake.write`, and the cluster insert each gain the row-shaped path with zero new entry, where an overload per member mints exactly the arity twins the closed-family law refuses.
- Law: the roster's `schema` is the frame source's own — `Olap.Frame`'s batch arm carries a schema because an encoder writes the schema message first, so a declared column set feeds `wire.flight.framed` with no `Table` ever materializing between the rows and the wire.
- Law: `pack` and `cast` are two packings of ONE cell because the dialects disagree on exactly the temporal shape — arrow-js normalizes every timestamp unit to a millisecond number and THROWS on a `bigint`, so a nanosecond column sheds sub-millisecond grain crossing an Arrow builder while the engine arm keeps the whole nanos its residence column declares; that loss rides the token row rather than each producer rediscovering it at its own conversion.
- Law: a builder bank holds `Builder<DataType>` at the family type and re-proves nothing — per-column precision is a declaration fact `Olap.Landed` already fixed at the producer, so the bank restates nothing and the ONE correlation break the token union opens is stated at the bank rather than at each fold reading it.
- Law: the wire is COMPRESSED and the codec is an admission, not an option — arrow-js ships the compression protocol with an empty registry, so naming a type without registering a codec fails at encode and a peer's compressed frame is undecodable at read; one module-scope registration off `node:zlib` admits zero packages and arms both directions at once, which is why the register and the encode argument are one landing rather than a write half and a read half a release apart.
- Law: compression prices a CROSSING, so the parquet hop declines it — the IPC buffer between this lane's `Table` and the codec's own never leaves the process and the writer row re-compresses every byte of it at rest, so compressing that hop pays twice for one saving; the registration still arms the read half there, because a foreign object's frames arrive however their producer wrote them.
- Law: the guard is POSITIVE — this fence holds two classes named `Table` sharing no identity and the workspace resolves three Arrow copies, so the ingest discriminates through `isArrowTable`'s `Symbol.for` marker (true across duplicate instances where `instanceof` is false) and refuses a foreign `Table` at the branch; a negative `instanceof Uint8Array` test also reclassifies every future member of a widened source union as an Arrow table by default, which is the failure mode a positive guard cannot have.
- Law: Parquet is the format AT REST and Arrow IPC the format in flight, so a lake object decodes and encodes with no engine booted — booting DuckDB or dialling a Flight server to transcode a stored object pays a query engine for codec work this row already owns.
- Law: the Flight frame is the same interchange inside a protobuf envelope, so BOTH directions live here and the Flight cluster owns no codec — a decode pair beside the client is a second wire the one-wire law refuses.
- Law: the frame codec admits either grain — the package hands `AsyncIterable`, this lane hands `Stream` — so `_framed` normalizes once on the value's own async-iterator key and a `Frames` intent composes straight into `header`, `landed`, and `streamed` with no consumer bridging the two.
- Law: streams stay bounded — large interchange rides `RecordBatchReader` batch iteration lifted to `Stream`, `decodeFlightDataStream` rides the same lift, a lake object past the bounded floor rides `ParquetFile.stream` range reads, and streamed egress rides the weighted row-group window, so an object larger than memory never materializes as one Table in either direction.
- Law: the range reader is the WHOLE remote read on both builds — opening a URL costs three bounded requests (the footer length suffix, the footer metadata suffix, and the page-index span), and the stream then draws ONE request per row group, so a cold-tail scan pays the object's metadata and the groups it reads, never the object; a whole-object GET means the source reached `read` instead of `batches`.
- Law: read economics ride ONE policy row beside the writer's — `_READING` states the batch grain and the in-flight range fan a scan opens, and the fan bounds concurrent requests exactly as declared, so a caller narrowing either passes a row and no call site spells a bare option object.
- Law: a parquet container never escapes as a value — bytes and `apache-arrow` values are the only egress, so no linear-memory view crosses this seam and the codec's own `Table`, `Schema`, and `RecordBatch` stay inside the expression that minted them.
- Law: custody follows the member's own ownership, not a blanket bracket — `intoIPCStream`, `writeParquet`, and `build` each CONSUME their handle, so a release arm around one frees a pointer the call already took; `ParquetFile` is the one container outliving its mint, and it alone acquires under a bracket that frees.
- Law: writer economics ride ONE policy row — compression, dictionary posture, statistics depth, row-group width, and the patience a partial window waits decide every later read's cost, so a stream wanting different economics passes a row and no call site spells a builder chain.
- Law: an object leaving this lane carries its REFERENCE ROW in the same unit of work — `object/store#REFERENCE_GC` closes the `<producer>:<coordinate>` namespace and owns the cold tail's `lake:<catalog>` row, so the landing composes the store's own `refer` beside every conditional put under the store's own owner mint and never a string this lane interpolates; a window landed without its row is an orphan the CAS sweep reclaims on its next pass while every residence read still names it, and the retention class ARRIVES on the landing because the class a cold tail keeps is the caller's policy answer, never a codec's.
- Law: the landing leg is the one that MINTS identity — the window is bytes in hand, so it takes `put` and the key derives from the content, where a caller-asserted key on this side addresses a tree the digest never proved.
- Law: streamed egress weights its window by ROWS, never by batch count — a feed of uneven batches otherwise lands objects whose row-group width swings with arrival shape, and the row-group column is the one number a later scan prunes against.
- Law: the codec build arrives by exports-map condition — the node entry inlines its wasm and the async entries resolve theirs once — so the composition root hands `Olap.lake` only the initializer its own build publishes and no call site branches on runtime.
- Law: codec work boots no engine and still names its LANE — `Olap.lake` takes the embedded key of the runtime composing it, so a service-side cold-tail write files its faults under the node lane and a browser range read under the worker; a hardcoded key here attributes every service-side codec failure to an engine that stack never instantiated, which is the engine-threading law refused at the one owner that boots nothing.
- Law: ingest is ONE entry discriminating on the source value — a live `Table` rides `insertArrowTable`, IPC bytes ride `insertArrowFromIPCStream` — and it rides the leased worker session like every other unit of work the browser lane owns, so a staged frame set cannot outrun the engine's own permit.
- Law: framing is ONE entry over three source shapes — a `Table`, a batch stream carrying its own schema because the encoder writes the schema message first, and raw IPC bytes an Arrow writer already framed; the descriptor stamps the FIRST message alone, which is exactly where `DoPut` reads it.
- Boundary: `parquet-wasm`'s own `Table` meets `apache-arrow`'s across the IPC stream buffer alone — the two containers share no class identity, so every crossing spells `intoIPCStream`/`fromIPCStream` and a shared-instance assumption is unspellable.
- Boundary: `transformParquetStream` is declined — its input grain is the codec's OWN `RecordBatch`, reachable from an Arrow batch only through a per-batch round trip whose intermediate container no fold on this side can free, so the weighted window above buys the same bounded egress with no orphaned handle.

```typescript
import { Sink } from "effect"
import { zstdCompressSync, zstdDecompressSync } from "node:zlib"
import {
  compressionRegistry, CompressionType, Field, isArrowTable, makeBuilder, RecordBatch, RecordBatchReader,
  Schema as ArrowSchema, Table, tableFromIPC, tableToIPC, type Builder, type DataType,
} from "apache-arrow"
import {
  createFlightDataFromIpc,
  decodeFlightDataStream,
  decodeFlightDataToTable,
  encodeRecordBatchesToFlightData,
  encodeTableToFlightData,
  getSchemaFromFlightData,
} from "@qualithm/arrow-flight-client"
import * as parquet from "parquet-wasm"
import { ObjectStore } from "../object/store.ts"
import { Retain } from "../journal/retain.ts"

declare namespace Olap {
  type Landed<C extends Columns> = { readonly [K in C[number] as K[0]]: Cell<K[1]> }
  type Roster<C extends Columns = Columns> = {
    readonly columns: C
    readonly resident: (rows: ReadonlyArray<Landed<C>>) => Resident
    readonly schema: ArrowSchema
    readonly table: (rows: ReadonlyArray<Landed<C>>) => Table
  }
  type Frame =
    | Table
    | Uint8Array
    | { readonly batches: AsyncIterable<RecordBatch> | Iterable<RecordBatch>; readonly schema: ArrowSchema }
  type Frames = AsyncIterable<FlightData> | Stream.Stream<FlightData, OlapFault>
  type Stored = Uint8Array | { readonly url: string } | { readonly blob: Blob }
  type Writing = {
    readonly compression: keyof typeof parquet.Compression
    readonly dictionary: boolean
    readonly patience: Duration.Duration
    readonly rowGroup: number
    readonly statistics: keyof typeof parquet.EnabledStatistics
  }
  type Landing = { readonly catalog: string; readonly retention: Retain.Class }
  type Lake = {
    readonly batches: (source: Stored, read?: parquet.ReaderOptions) => Stream.Stream<RecordBatch, OlapFault>
    readonly read: (bytes: Uint8Array, read?: parquet.ReaderOptions) => Effect.Effect<Table, OlapFault>
    readonly schema: (bytes: Uint8Array) => Effect.Effect<ArrowSchema, OlapFault>
    readonly sink: (
      batches: Stream.Stream<RecordBatch, OlapFault>,
      landing: Landing,
      policy?: Writing,
    ) => Stream.Stream<ObjectStore.Landed, OlapFault, ObjectStore>
    readonly write: (table: Table, landing: Landing, policy?: Writing) => Effect.Effect<ObjectStore.Landed, OlapFault, ObjectStore>
  }
}

const _PARQUET = {
  compression: "ZSTD",
  dictionary: true,
  patience: Duration.seconds(5),
  rowGroup: 128_000,
  statistics: "Page",
} as const satisfies Olap.Writing

const _READING = { batchSize: 65_536, concurrency: _FLIGHT.endpoints } as const satisfies parquet.ReaderOptions

const _frames = Match.type<Olap.Frame>().pipe(
  Match.when(Match.instanceOf(Table), (table) => Stream.fromAsyncIterable(encodeTableToFlightData(table), _flightFault)),
  Match.when(
    Match.instanceOf(Uint8Array),
    (bytes) => Stream.fromEffect(Effect.try({ try: () => createFlightDataFromIpc(bytes), catch: _flightFault })),
  ),
  Match.orElse(({ batches, schema }) => Stream.fromAsyncIterable(encodeRecordBatchesToFlightData(batches, schema), _flightFault)),
)

const _framed = (source: Olap.Frames): AsyncIterable<FlightData> =>
  Symbol.asyncIterator in source ? source : Stream.toAsyncIterable(source)

compressionRegistry.set(CompressionType.ZSTD, { decode: zstdDecompressSync, encode: zstdCompressSync })

const _roster = <const C extends Olap.Columns>(columns: C): Olap.Roster<C> =>
  pipe(
    new ArrowSchema(Array.map(columns, ([name, token]) => new Field(name, _COLUMN[token].arrow(), true))),
    (schema) => ({
      columns,
      resident: (rows) => ({
        columns: Array.map(columns, ([name, token]) => ({
          name,
          type: _COLUMN[token].type,
          vector: Array.map(rows, (row) => _COLUMN[token].cast(row[name])),
        })),
        count: rows.length,
      }),
      schema,
      table: (rows) =>
        new Table(
          schema,
          Record.fromEntries(Array.map(columns, ([name, token]) =>
            [
              name,
              pipe(makeBuilder({ type: _COLUMN[token].arrow() }), (bank: Builder<DataType>) => {
                Array.forEach(rows, (row) => bank.append(_COLUMN[token].pack(row[name])))
                return bank.finish().toVector()
              }),
            ] as const)),
        ),
    }),
  )

const _wire = {
  roster: _roster,
  decode: (engine: Olap.Engine, bytes: Uint8Array): Effect.Effect<Table, OlapFault> =>
    Effect.try({ try: () => tableFromIPC(bytes), catch: _fault(engine, "wire") }),
  encode: (engine: Olap.Engine, table: Table): Effect.Effect<Uint8Array, OlapFault> =>
    Effect.try({ try: () => tableToIPC(table, "stream", CompressionType.ZSTD), catch: _fault(engine, "wire") }),
  batches: (engine: Olap.Engine, source: AsyncIterable<Uint8Array>) =>
    Stream.unwrap(
      Effect.map(
        _try(engine, "wire", () => RecordBatchReader.from(source)),
        (reader) => Stream.fromAsyncIterable(reader, _fault(engine, "wire")),
      )),
  flight: {
    framed: (source: Olap.Frame, descriptor: FlightDescriptor): AsyncIterable<FlightData> =>
      Stream.toAsyncIterable(
        Stream.mapAccum(_frames(source), true, (first, data) => [false, first ? { ...data, flightDescriptor: descriptor } : data]),
      ),
    header: (source: Olap.Frames): Effect.Effect<Option.Option<ArrowSchema>, OlapFault> =>
      Effect.map(_flew(() => getSchemaFromFlightData(_framed(source))), Option.fromNullable),
    landed: (source: Olap.Frames): Effect.Effect<Table, OlapFault> => _flew(() => decodeFlightDataToTable(_framed(source))),
    streamed: (source: Olap.Frames): Stream.Stream<RecordBatch, OlapFault> =>
      Stream.fromAsyncIterable(decodeFlightDataStream(_framed(source)), _flightFault),
  },
  feed: (session: Olap.Session<"duckdbWasm">, name: string, source: Table | Uint8Array) =>
    _governed(session)(
      { access: "write", fault: "wire" },
      _try(session.engine, "wire", () =>
        isArrowTable(source)
          ? session.connection.insertArrowTable(source, { name })
          : session.connection.insertArrowFromIPCStream(source, { name })),
    ),
} as const

// --- [PARQUET_AT_REST]

const _lake = (engine: Olap.Embedded, ready?: () => Promise<unknown>): Effect.Effect<Olap.Lake, OlapFault> =>
  Effect.gen(function* () {
    yield* ready === undefined ? Effect.void : _try(engine, "bundle", ready)
    const propertied = (policy: Olap.Writing): parquet.WriterProperties =>
      new parquet.WriterPropertiesBuilder()
        .setCompression(parquet.Compression[policy.compression])
        .setDictionaryEnabled(policy.dictionary)
        .setMaxRowGroupSize(policy.rowGroup)
        .setStatisticsEnabled(parquet.EnabledStatistics[policy.statistics])
        .build()
    const written = (table: Table, policy: Olap.Writing = _PARQUET): Effect.Effect<Uint8Array, OlapFault> =>
      Effect.try({
        try: () => parquet.writeParquet(parquet.Table.fromIPCStream(tableToIPC(table, "stream")), propertied(policy)),
        catch: _fault(engine, "wire"),
      })
    const landed = (landing: Olap.Landing) => (bytes: Uint8Array): Effect.Effect<ObjectStore.Landed, OlapFault, ObjectStore> =>
      Effect.flatMap(ObjectStore, (store) =>
        Effect.mapError(
          Effect.tap(store.put(bytes), (put) => store.refer(put.key, ObjectStore.owner("lake", landing.catalog), landing.retention)),
          _fault(engine, "object"),
        ))
    const opened = (source: Olap.Stored): Effect.Effect<parquet.ParquetFile, OlapFault, Scope.Scope> =>
      Effect.acquireRelease(
        _try(engine, "wire", () =>
          "url" in source
            ? parquet.ParquetFile.fromUrl(source.url)
            : parquet.ParquetFile.fromFile("blob" in source ? source.blob : new Blob([source]))),
        (file) => Effect.sync(() => file.free()),
      )
    return {
      read: (bytes, read) =>
        Effect.try({ try: () => tableFromIPC(parquet.readParquet(bytes, read).intoIPCStream()), catch: _fault(engine, "wire") }),
      schema: (bytes) =>
        Effect.try({ try: () => tableFromIPC(parquet.readSchema(bytes).intoIPCStream()).schema, catch: _fault(engine, "wire") }),
      batches: (source, read) =>
        Stream.unwrapScoped(Effect.map(
          Effect.flatMap(opened(source), (file) => _try(engine, "wire", () => file.stream({ ..._READING, ...read }))),
          (readable) =>
            Stream.mapConcat(
              Stream.mapEffect(
                Stream.fromReadableStream<parquet.RecordBatch, OlapFault>({
                  evaluate: () => readable,
                  onError: _fault(engine, "wire"),
                }),
                (batch) => Effect.try({ try: () => tableFromIPC(batch.intoIPCStream()), catch: _fault(engine, "wire") }),
              ),
              (held) => held.batches,
            ),
        )),
      sink: (batches, landing, policy = _PARQUET) =>
        Stream.mapEffect(
          Stream.filter(
            Stream.aggregateWithin(
              batches,
              Sink.foldWeighted({
                initial: Array.empty<RecordBatch>(),
                maxCost: policy.rowGroup,
                cost: (_held, batch: RecordBatch) => batch.numRows,
                body: (held, batch: RecordBatch) => Array.append(held, batch),
              }),
              Schedule.spaced(policy.patience),
            ),
            Array.isNonEmptyReadonlyArray,
          ),
          (window) => Effect.flatMap(written(new Table(window), policy), landed(landing)),
        ),
      write: (table, landing, policy) => Effect.flatMap(written(table, policy), landed(landing)),
    }
  })

const Olap = {
  engines: _engines,
  node: _node,
  wasm: _wasm,
  Read: _Read,
  read: _read,
  sources: _SOURCES,
  source: _sourced,
  attach: _attach,
  mount: _mount,
  events: _WIDE,
  points: _POINTS,
  lake: _lake,
  lakeSource: _lakeSource,
  absorb: _absorbed,
  recorded: _recorded,
  joined: _joined,
  residences: _RESIDENCES,
  target: _target,
  clickhouse: _clickhouse,
  ingest: _ingest,
  wide: _wide,
  flight: _flight,
  Flown: _Flown,
  flown: _flown,
  transacted: _transacted,
  wire: _wire,
  profile: _profile,
  probe: _probe,
  armed: _armed,
  Fault: OlapFault,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Olap, OlapFault }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
