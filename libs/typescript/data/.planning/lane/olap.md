# [DATA_OLAP]

Engine rows own analytical throughput, never transactional durability: DuckDB accelerates node and browser under one leased session family, pgDuckDB embeds it in live Postgres, ClickHouse enters past its trigger, and FLIGHT carries every remote columnar end. RESIDENCE_ROWS parameterize durable planes — extensions, credentials, attachments, the identity-stamping fill, and the schemas `Board.Query` renders against. ARROW_WIRE holds each codec pair as the ONE interchange — IPC in memory, Flight on the wire, Parquet at rest — while PROFILE folds escalation evidence and residences stay derived.

Settled composition: `core/observe/board#QUERY` owns the query algebra and its render-target axis, so this page fills `Board.Query.Residence`, mints `Board.Query.Target`, and renders none. `iac/operate/observe#CHART_ROWS` plants the clickhouse wide-event DDL this page transcribes; the lake plants its own through `Olap.mount`. `Convention.identity` joins signals to the journal's `app`/`tenant` columns, `lane/postgres#PROFILE_HARVEST` owns the `Pg.Profile` each profile arm fills, and `object/store#STORE` owns this lane's object coordinates.

## [01]-[INDEX]

- [02]-[ENGINE_ROWS] — `_engines` prices guarantee, storage posture, ceiling, and escalation trigger per engine.
- [03]-[EMBEDDED] — `Olap.node` and `Olap.wasm` lease one session family under `Olap.read`, and `_SOURCES` routes each lane source into a relation.
- [04]-[RESIDENCE_ROWS] — `_RESIDENCES` carries each durable plane's extensions, credentials, attachments, schemas, and fill.
- [05]-[CLICKHOUSE] — `Olap.ingest` and `Olap.wide` bind the at-scale driver behind the lane's one fault rail.
- [06]-[FLIGHT] — `Olap.flown` carries every Flight SQL modality over one sealed remote coordinate.
- [07]-[PROFILE] — `Olap.profile` harvests engine profiles and `Olap.armed` grades the escalation evidence.
- [08]-[ARROW_WIRE] — `Olap.wire` and `Olap.lake` own the one columnar interchange in memory, on the wire, and at rest.

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

```typescript signature
// Escalation order, not alphabetical: the embedded default first, the browser and in-OLTP rows beside it, then the
// two rows a workload reaches only past a stated trigger. `satisfies` binds every row to the same four columns, so a
// fifth column fails at this declaration rather than reading as absent on whichever row forgot it.
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

- Owner: the embedded driver family — `_DRIVERS` rows carrying each driver's bind vocabulary, bounded, windowed, and drained members, its engine-side cancel, its permit ceiling, its own result grain, and the residency its registry admits; `Olap.node` and `Olap.wasm` mint one scoped `Olap.Handle` apiece whose lease carries the pool's own eviction seam beside the connection, `Olap.read` is the ONE leased-session entry whose `Rows`, `Window`, and `Drain` cases select geometry under the owner governor, and the source-admission half — `_SOURCES` routing each admitted lane source into a relation, `Olap.source` pumping a `scan` row into the handle's own registry behind a registered table function, `Olap.lakeSource` registering the browser's file residencies on the leased worker.
- Packages: DuckDB, Arrow, and Effect runtime surfaces; `@duckdb/node-api` (`DuckDBTableFunction`, `DuckDBType`) and `effect` (`MutableHashMap`) carry the source registry; core `Fault.Budget`, `Convention`, and `Fault.Class`.
- Entry: a service composes `Olap.node` once per database coordinate and the browser shell composes `Olap.wasm` over self-hosted bundles at boot; both hand a `handle.lease` to each analytical unit of work, and every statement, file registration, and Arrow ingest rides that one session.
- Receipt: each engine answers its own grain off its driver row — the node driver materializes row-object projections, the worker hands Arrow `Table` and `RecordBatch` values straight through to the viewer with no re-encoding.
- Growth: an embedded driver is one `_DRIVERS` row answering three execution members, its cancel, its permit ceiling, and one grain pair; a read geometry is one `Olap.Read` case with its overload and per-driver member; resilience is a `_GOVERNOR` override; a registry residency is one `_registered` arm; an admitted source is one `_SOURCES` row whose route decides between minting a scan and naming the statement that already carries it; a new measure is one `_meter` row naming a census metric and its axis, whose whole per-engine set follows.
- Law: lifecycle is `acquireRelease` under `Scope` — instance, worker, and every connection release deterministically; an unscoped engine handle is unspellable because the constructors return scoped effects.
- Law: every promise lifts through `_try`; `OlapFault` rows close through `Fault.Class.family` and route recovery by reason.
- Law: both embedded engines share ONE session family, so the bulkhead semaphore, the budget bracket, the replay rule, and the meter fan reach the browser lane exactly as they reach the node lane; a second ungoverned read entry beside `Olap.read` leaves a single-threaded worker taking unbounded concurrent statements and taps no engine-labelled measure at all.
- Law: the session carries its engine, so every fault the session mints and every meter it taps is engine-labelled by construction; an operation reading the engine from an enclosing literal mislabels every row the moment a second engine reaches the same operation.
- Law: driver divergence is a row, never an arm — result grain, bind vocabulary, the three execution members, the engine-side cancel, and the permit ceiling ride `_DRIVERS`, so `Olap.read` dispatches on the session's own engine key and no geometry branches on a driver name.
- Law: a budget releases the FIBER and an engine learns nothing from that, so every abandoned unit stops at its driver — `_cancelled` seats the row's own cancel inside the retry, and a drain cancels on any non-success exit through `Stream.ensuringWith` so the idle bound reaches the scan rather than leaving it running behind a consumer that is gone; a lane bounding time without cancelling leaves a stalled statement holding its native thread, its spill files, and the connection it borrowed for the process's life.
- Law: a refusal carrying a MEASURED wait publishes it as `after` — the field names the earliest instant a replay succeeds and only the rail that measured one fills it, so a caller re-offering work reads a number its refusing owner published instead of guessing off a curve that priced a different arrival; the field stays optional at construction because a sentinel mint learns no wait and an `Option` every such mint restates is emptiness spelled fifteen times.
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
- Law: a scan's refusal is `setError` and the arm is chosen for ATTRIBUTION — a JS throw crossing the same seam folds to the same `Invalid Input Error`, so the difference is which text the caller reads back, and the lane spells its own source coordinate rather than a stack the engine reprints.
- Law: no wasm cell carries a scan — that build ships no table-function surface at all, so the browser's answer is a file or Arrow registration route or it is `None`, and the pre-pump the scan route already demands is exactly what makes one engine-side Arrow copy an equal answer rather than a degradation.
- Law: `connection.createAppender` is the load-into-a-plane counterparty and stays REFUSED — the object fill crosses no rows into this runtime at all (`Olap.absorb` scans Parquet inside the engine), and the registry fill keys every cell by COLUMN NAME off `_POINTS` where the appender only ever appends by position and only on the node driver, so admitting it re-imports the positional hazard that roster deletes, forks the fill by driver, and buys nothing on the one path that carries volume.
- Law: instrument construction belongs to `Convention.mount` alone — the census row carries kind, description, UCUM unit, and bucket ladder, so `_tapped` names a metric and an axis and mints nothing; a locally-declared boundary set is the deleted form because it silently disagrees with the ladder the row publishes and drops the unit tag the OTLP bridge reads its descriptor unit from.
- Law: every tap fans the same row across every engine key, so an operation reading a measure indexes by the engine it already carries and five engines across four measures never become twenty declarations.
- Law: `_governed` brackets every unit of work a session owns — an embedded statement, a browser file registration, an Arrow ingest — behind one permit and one budget, so `_Governed`'s two columns are the whole vocabulary a governed call answers. That budget is TOTAL over the unit: the deadline seats above the replay stack, so a governed read releases its fiber on the one bound the row declares rather than on that bound multiplied by every attempt the curve admits.
- Law: replay admits `access: "read"` alone, so an executing write, extension mutation, credential mint, or profile harvest stays one-shot; the governor values are one policy row.
- Law: transience derives from `Fault.Class.retryable`; the same projection gates retries and records replay telemetry.
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

```typescript signature
import { Array, Cause, Data, Duration, Effect, Exit, Match, Metric, MutableHashMap, Option, pipe, Pool, Record, Schedule, Schema, type Scope, Stream } from "effect"
import { DuckDBInstance, DuckDBTableFunction, type DuckDBConnection, type DuckDBType, type DuckDBValue } from "@duckdb/node-api"
import { GetObjectCommand } from "@aws-sdk/client-s3"
import * as wasm from "@duckdb/duckdb-wasm"
import { RecordBatch, Table } from "apache-arrow"
import { Convention, type Digest, Fault } from "@rasm/ts/core"
import { ObjectStore } from "../object/store.ts"

const _Engine = Schema.Literal(...Record.keys(_engines)) // the one engine anchor spread: no second roster to drift

// One row per arrival class carrying its core kind alone: an unreachable coordinate, an engine arrival an idempotent
// read may re-issue, and a refused object landing are the three retryable classes — content addressing admits that
// third one, since a replayed window lands the same key or answers a `412` no-op — while a credential refusal, an
// absent extension, an unselectable bundle, and a broken codec crossing each answer once. Whether a REPLAY can
// resolve an arrival is the core Fault.Class row table's `retryable` column, so the gate and the retry meter read
// that lattice, and a local replay column beside `class` forks the taxonomy into this folder. Reasons derive the
// fault's own union, so an eighth arrival class is ONE row the gate, the meter, and the union all follow from.
const _family = Fault.Class.family(["acquire", "query", "extension", "secret", "bundle", "object", "wire"] as const, {
  acquire: { class: "unavailable" },
  query: { class: "unavailable" },
  extension: { class: "absent" },
  secret: { class: "denied" },
  bundle: { class: "absent" },
  object: { class: "unavailable" },
  wire: { class: "malformed" },
})

class OlapFault extends Schema.TaggedError<OlapFault>()("OlapFault", {
  engine: _Engine,
  reason: _family.schema,
  detail: Schema.String,
  // Wait published BY the refusing owner, never derived here: `Schema.optional` keeps the field absent at
  // construction, so the one rail that measures a span fills it and no sentinel mint spells an emptiness it never
  // learned. Detail carries text a reader parses; this carries the number a caller re-offers work against.
  after: Schema.optional(Schema.DurationFromSelf),
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<olap:${this.reason}> ${this.engine}: ${this.detail}`
  }
}

// Census rows decide each tap's input through the carrier they materialize into: a millisecond-coded histogram row
// takes the `Duration` its own scale multiplies, a dimensionless counter row takes the number the site holds.
// `Mounted` keeps that correspondence with the row rather than restating it per measure.
type _Tap<N extends Convention.MetricName> = Convention.Mounted<N> extends Metric.Metric<infer _Type, infer In, infer _Out> ? In
  : never

// `Convention.mount` owns kind, description, ladder, and the UCUM tag the OTLP bridge computes its descriptor unit
// from, so a tap names a census metric and the axis it fans on. `profiled` fans every engine key including those with
// no landed harvest arm — the tap exists the day an arm lands rather than the day someone remembers its constant.
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

// One budget and one curve, both engine-blind — replayability is NOT a third field here, because it is a property
// of the arrival that the core Fault.Class row table already prices through each family row's class; a roster
// restated beside that lattice admits a reason the union never declared and drops one it does. Both curve and
// budget are the core `lease` row's, so this lane spells no cadence: its compiled schedule carries jitter, reset,
// attempt bound, and elapsed ceiling, and its own gate is the lattice the replay meter already reads. Permit
// COUNT is not engine-blind and therefore not seated here — it rides `_DRIVERS`, where each engine's ceiling is.
const _GOVERNOR = {
  budget: Fault.Budget.at("lease").total,
  retry: Fault.Budget.schedule("lease"),
} as const satisfies {
  readonly budget: Duration.Duration
  readonly retry: Schedule.Schedule<unknown>
}

// Every detail a BOUND mints, rostered rather than spelled at each site: a destroyed connection settles nothing at
// all, so a bound is the only arrival it can produce and this roster is what lets the release arm tell "the engine
// refused" from "the engine never answered" without reading a driver string it does not own.
const _ABANDONED = { budget: "<budget>", idle: "<idle-budget>", profile: "<profile-budget>" } as const

const _abandoned = (fault: OlapFault): boolean => Array.contains(Record.values(_ABANDONED), fault.detail)

// Two columns the governor reads off every governed unit of work, whatever surface raised it: an embedded
// statement, a browser file registration, a ClickHouse fragment, and a Flight intent each answer them, so one budget
// and the core lattice's own retryability govern this whole lane and no engine row grows a private resilience posture.
type _Governed = {
  readonly access: "read" | "write"
  readonly fault: OlapFault["reason"]
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

// One fault mint for every promise, iterator, and codec crossing this lane holds: engine and reason arrive from the
// site already carrying them, so no crossing spells its own constructor and the cause prints exactly once.
const _fault = (engine: Olap.Engine, reason: OlapFault["reason"]) => (cause: unknown): OlapFault =>
  new OlapFault({ engine, reason, detail: String(cause) })

const _try = <A>(engine: Olap.Engine, reason: OlapFault["reason"], run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: _fault(engine, reason) })

const _resilient = <A, R>(
  engine: Olap.Engine,
  governed: _Governed,
  work: Effect.Effect<A, OlapFault, R>,
): Effect.Effect<A, OlapFault, R> =>
  (governed.access === "read"
    // `_meter.retried` counts REPLAYS, so the tap reads the same core lattice the gate does: a terminal refusal
    // answers once and records nothing, where an unconditional tap reports every one-shot failure as a retry that
    // never ran. `Fault.Class.retryable` projects the fault's class row, so gate and meter cannot disagree.
    ? work.pipe(
      Effect.tapError((fault: OlapFault) => Fault.Class.retryable(fault) ? _meter.retried[engine](1) : Effect.void),
      // No `while` argument: the compiled `lease` schedule already gates on `Fault.Class.retryable` as its owner
      // default, so a call-site predicate would restate the exact lattice the policy value carries.
      Effect.retry(_GOVERNOR.retry),
    )
    : work).pipe(
      // ONE budget, and it bounds the WHOLE governed unit. Seated UNDER the retry it bounded each attempt alone, so a
      // replayed read against a stalled engine spent four budgets plus its backoff before releasing the fiber the
      // bound exists to release. The surface declares one budget, so the transformer sits where that budget is total
      // rather than where pipe order happened to put it.
      Effect.timeoutFail({
        duration: _GOVERNOR.budget,
        onTimeout: () => new OlapFault({ engine, reason: governed.fault, detail: _ABANDONED.budget }),
      }),
    )

const _governed = <E extends Olap.Embedded>(session: Olap.Session<E>) =>
  <A, R>(governed: _Governed, work: Effect.Effect<A, OlapFault, R>): Effect.Effect<A, OlapFault, R> =>
    Effect.acquireUseRelease(
      Effect.tap(Effect.timed(session.gate.take(1)), ([span]) => _meter.waited[session.engine](span)), // Gate wait projects onto Convention; permit state stays truth.
      // Cancellation seats INSIDE the retry, so each abandoned attempt stops at the engine before the next binds that
      // connection; seated outside it, the curve stacks four live statements on one leased session. Eviction rides
      // ONLY the bound: a driver refusal proves the connection alive and hands it back, where a connection destroyed
      // mid-lease settles nothing and `Pool` publishes no health check to notice, so that one slot serves
      // `connection disconnected` to every later lease until the process ends.
      () =>
        _resilient(session.engine, governed, _cancelled(session, work)).pipe(
          Effect.tapError((fault) => _abandoned(fault) ? session.evict : Effect.void),
        ),
      () => session.gate.release(1),
    )

// `runAndReadAll` takes a mutable bind list, so this copy IS the boundary; `fromIterable` is effect's own spelling,
// and the global `from` is unreachable under the module import that shadows it.
const _values = (values: ReadonlyArray<DuckDBValue> | undefined): Array<DuckDBValue> | undefined =>
  values === undefined ? undefined : Array.fromIterable(values)

// Each embedded driver answers one geometry family through its own members, its own bind vocabulary, and its own
// result grain — the node driver types every cell and materializes row objects, the worker clones binds over
// `postMessage` and hands Arrow through. Keying that correspondence to the engine is what lets one lease, one gate,
// one budget, and one meter fan serve both without a single geometry branching on a driver name.
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
    // This build publishes no table-function surface at all, so this handle's registry is uninhabitable by
    // construction rather than empty by convention, and the proof rides the same column the scan route reads.
    readonly resident: never
  }
}

declare namespace Olap {
  // Engine rides as a type parameter, never a field read at runtime: a member demanding a profiling engine says so
  // in its signature, so the harvest arm's admitted set is a compile fact rather than a literal it re-asserts.
  type Embedded = keyof _Driver
  type Scalar = Extract<DuckDBValue, null | boolean | number | bigint | string>
  type Bind<E extends Embedded> = _Driver[E]["bind"]
  type Bounded<E extends Embedded> = _Driver[E]["bounded"]
  type Element<E extends Embedded> = _Driver[E]["element"]
  // `evict` closes over the pool that leased this very connection, so the session that discovers a corpse retires it
  // without a pool handle crossing into every fold that holds a session.
  type Session<E extends Embedded = Embedded> = {
    readonly connection: _Driver[E]["connection"]
    readonly engine: E
    readonly evict: Effect.Effect<void>
    readonly gate: Effect.Semaphore
  }
  // `Handle` carries its engine beside the lease, so a fold labelling evidence, a fault, or a meter before any
  // session exists reads the key rather than paying a connection to learn what the handle's own type already fixes.
  // `sources` seats at the handle because a registration lands in the INSTANCE catalog and outlives every connection
  // that reads it, so the residency a scan resolves has to live exactly as long as the instance does.
  type Handle<E extends Embedded = Embedded> = {
    readonly engine: E
    readonly lease: Effect.Effect<Session<E>, OlapFault, Scope.Scope>
    readonly sources: Registry<E>
  }
  type Row = Record.ReadonlyRecord<string, unknown>
  // `Bound` narrows binds by the session's own engine, so a driver value class no structured clone survives is
  // unspellable on a statement a worker session runs — the divergence closes at the type, never at a runtime probe.
  type Bound<E extends Embedded, K extends OlapRead["_tag"]> = Extract<OlapRead, { readonly _tag: K }> & { readonly values?: ReadonlyArray<Bind<E>> }
  type _Drivers<K extends Engine = Embedded> = K // a driver keyed off the engine table refuses right here, so no fault, meter, or trigger reads an unrostered key
}

// `AsyncPreparedStatement` is the worker's ONE bind seam: a statement carrying values acquires its plan on the
// enclosing scope so a streamed read outlives the call that opened it, while bind-free admission text runs on the
// connection outright because a multi-statement `INSTALL`/`LOAD` prepares nowhere.
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

// Worker readers pull one `RecordBatch` at a time, so the drain and the window share one acquisition and neither
// materializes more than the caller asked for.
const _batched = (
  session: Olap.Session<"duckdbWasm">,
  op: _Statement,
): Effect.Effect<Stream.Stream<RecordBatch, OlapFault>, OlapFault, Scope.Scope> =>
  Effect.map(
    _worker(session, op, (connection, sql) => connection.send(sql), (plan, values) => plan.send(...values)),
    (reader) => Stream.fromAsyncIterable(reader, _fault(session.engine, "query")),
  )

// Dispatch stays cast-free because the session's engine is a BOUND parameter, not a runtime tag over a union: the
// mapped record's indexed access correlates each member to the session that selected it, so nothing rejoins by hand.
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
    // `interrupt()` is synchronous and returns void, so the cancel arm lifts a throw and drops it: a cancel racing a
    // statement that already finished has nothing to stop, and turning that race into a fault would fail a fiber
    // that is leaving anyway. Ignoring is the disposition, never an untyped `sync` whose throw becomes a defect.
    cancel: (session) => Effect.ignore(Effect.try(() => session.connection.interrupt())),
    drained: (session, op) =>
      Effect.map(
        _try(session.engine, op.fault, () => session.connection.stream(op.sql, _values(op.values))),
        (result) => Stream.mapConcat(Stream.fromAsyncIterable(result.yieldRowObjects(), _fault(session.engine, "query")), (rows) => rows),
      ),
    // `readUntil` advances by chunk grain and overshoots the target, so the take is what makes the window exact
    windowed: (session, op) =>
      Effect.map(
        _try(session.engine, op.fault, () => session.connection.streamAndReadUntil(op.sql, op.take, _values(op.values))),
        (reader) => Array.take(reader.getRowObjects(), op.take),
      ),
    // Out-of-core spill and vectorized execution run on the engine's own thread pool, so eight statements in flight
    // is the node ceiling; the row states it because the browser row cannot hold the same number.
    sessions: 8,
  },
  duckdbWasm: {
    bounded: (session, op) => _worker(session, op, (connection, sql) => connection.query(sql), (plan, values) => plan.query(...values)),
    // `cancelSent()` answers whether a sent query stopped, and `false` means nothing was in flight —
    // a benign race, never a fault — so the boolean drops here exactly as the node arm drops its own throw.
    cancel: (session) => Effect.ignore(_try(session.engine, "query", () => session.connection.cancelSent())),
    drained: _batched,
    // Worker readers publish no row cutoff at all, so the window pulls batches until the target is met and slices the
    // assembled Table — the same exactness the node arm buys after its own reader overshoots.
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
    // One worker, single-threaded by default, so a second permit buys no parallelism and only lets a second statement
    // queue behind the first while holding a connection; eight permits here would hand the browser lane the node
    // lane's number against an engine whose own ceiling row already refuses it.
    sessions: 1,
  },
}

// Budgets release the FIBER and nothing else: an abandoned `tryPromise` leaves the engine executing, so a read that
// answered `<budget>` keeps its native thread, its spill files, and the pooled connection it borrowed until the
// statement finishes on its own. Each driver publishes its own stop — the node engine an in-process `interrupt`, the
// worker a `cancelSent` over `postMessage` — so cancellation is one more `_DRIVERS` column and the interrupted unit
// stops at the engine rather than running to completion for a caller that already left.
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
        // `Pool.invalidate` reallocates lazily, so retiring a corpse mid-lease costs nothing until the release the
        // lease already owns; its own `Scope` closes here because the pool's lifetime is this handle's, not the call's.
        evict: Effect.scoped(Pool.invalidate(pool, connection)),
        gate,
      })),
    }
  })

// `Olap.wasm` mints the same handle shape `Olap.node` does, so the browser lane leases, gates, budgets, and
// meters exactly like the service lane and the viewer reaches analytics through one entry rather than a private path.
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
      // Typed uninhabitable, never merely empty: this build registers FILES and never FUNCTIONS, so a resident this
      // registry could hold has no shape at all and the pump refuses at the signature rather than at a runtime arm.
      sources: MutableHashMap.empty<string, never>(),
      lease: Effect.map(pool.get, (connection) => ({
        connection,
        engine: "duckdbWasm" as const,
        evict: Effect.scoped(Pool.invalidate(pool, connection)),
        gate,
      })),
    }
  })

// Acquisition retries before the first emission and the permit holds until downstream termination; an iteration fault
// fails the stream without replay because a partial-output retry duplicates rows the consumer already took, and the
// scope the acquisition registers on is the stream's own, so a worker plan lives exactly as long as its batches.
// Under the IDLE bound a governed answer releases its fiber on the budget, and that bound alone makes the held permit
// releasable: a drain whose engine stops emitting mid-result otherwise holds one of the `sessions` permits forever
// while the budget the surface declares governs only the acquisition. Bounding total elapsed instead kills the exact
// out-of-core scans this geometry exists to serve, so the gap between chunks is the bound, exactly as the remote wire
// bounds the gap between frames.
// Exit-keyed cancellation is what makes the idle bound REACH the engine: a drain that stops emitting fails its
// stream, releases its permit, and would otherwise leave the scan running behind a consumer that is already gone.
// Success exits cancel nothing, so a completed drain spends no round trip, and the finalizer runs after the permit
// release because the stop targets the connection the caller's lease still holds, never the gate.
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
      () => new OlapFault({ engine: session.engine, reason: op.fault, detail: _ABANDONED.idle }),
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

// One row per lane source admitted as a RELATION, and `route` is the whole adjudication: a `scan` row mints a
// registered table function over content this lane pre-pumps, and a `sql` row states the source is ALREADY a relation
// through a statement this page owns, so it mints nothing beside it. The two scan rows lead because they are the only
// ones that spend a permanent catalog entry. `wasm` carries the browser's own answer and never a scan.
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
  // Scan rows state the residency bound their pump refuses past; a `sql` row states `false`, because the engine
  // bounds that read and a number here prices a ceiling this lane never enforces.
  readonly ceiling: number | false
  readonly refuses: string
  readonly route: "scan" | "sql"
  readonly via: string
  readonly wasm: Option.Option<string>
}>

// Registration takes a BARE name into a catalog entry nothing can drop, so an unvetted token would pin an unquotable
// entry for the process's life; this pattern is the one admission, and the kind prefixes it so two sources sharing a
// token stay two entries rather than colliding into whichever registered first.
const _Token = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9_]*$/), Schema.brand("OlapToken"))

// Chunks cap at 2048 rows and a wider write refuses outright, so one re-entry fills one chunk and the cursor carries
// whatever remains; that write is exactly how long the scan holds the node main thread.
const _SCAN = { chunk: 2048 } as const

declare namespace Olap {
  // File residencies mirror the codec's own `Stored` union one for one — an object-plane key this lane presigns, a
  // picked local file, and bytes already in hand — because engine-side registration and the at-rest read answer one
  // source set. A lane registering the remote residency alone leaves the browser able to DECODE a picked file through
  // `Olap.lake` and unable to QUERY it, which reads as a missing feature rather than the missing row it is.
  type File = { readonly blob: Blob } | { readonly key: Digest.Key<"content"> } | { readonly octets: Uint8Array }
  // Pre-pumped columnar content under ONE roster: the name, its engine type, and its vector ride together, so a bound
  // column and the vector the scan writes for it cannot slide apart and the arity is the head vector's own length.
  type Resident = {
    readonly columns: Array.NonEmptyReadonlyArray<{
      readonly name: string
      readonly type: DuckDBType
      readonly vector: ReadonlyArray<DuckDBValue>
    }>
  }
  type Registry<E extends Embedded = Embedded> = MutableHashMap.MutableHashMap<string, _Driver[E]["resident"]>
  type Source = keyof typeof _SOURCES
  // Scan routes alone reach the pump: a `sql` row is unspellable at that entry, so the route column refuses at the
  // signature rather than at a runtime arm every caller has to read about.
  type Scanned = { readonly [K in Source]: (typeof _SOURCES)[K]["route"] extends "scan" ? K : never }[Source]
}

// Rosters carry the PLAN's projected order rather than the SELECT list's, and their members index the BOUND columns,
// so residency and projection resolve in ONE read: an unresolvable index answers `None` and refuses by name instead
// of writing a chunk whose width the engine then rejects from inside its own value converter.
const _projected = (registry: Olap.Registry<"duckdbNode">, name: string, roster: ReadonlyArray<number>) =>
  Option.flatMap(MutableHashMap.get(registry, name), (resident) =>
    Option.map(
      Option.all(Array.map(roster, (bound) => Array.get(resident.columns, bound))),
      (columns) => ({ columns, count: Array.headNonEmpty(resident.columns).vector.length }),
    ))

// Bind and main resolve their content out of the handle registry BY NAME and capture none: re-registering a name is a
// SILENT no-op that keeps the FIRST mint serving, so every mint has to be interchangeable, and reading by name is what
// makes them so — a re-pump then swaps what the planner reads on its next bind. Bind fires TWICE per statement, which
// is why it only declares and never advances.
const _scanned = (registry: Olap.Registry<"duckdbNode">, name: string): DuckDBTableFunction =>
  DuckDBTableFunction.create({
    name,
    // MANDATORY, never an optimization: `getColumnIndexes()` answers the projected set either way, but only the opt-in
    // NARROWS the output chunk, and a roster-width write into an unnarrowed chunk refuses inside the engine's own
    // converter rather than at any seam this lane holds.
    supportsProjectionPushdown: true,
    bindFunction: (bind) =>
      Option.match(MutableHashMap.get(registry, name), {
        // Catalog entries outlive every scope that filled them and nothing drops one, so a scan past release reads an
        // empty residency and REFUSES by name — the one disposition that beats serving a stale pump as success.
        onNone: () => bind.setError(`<source:${name}>`),
        onSome: (resident) => {
          Array.forEach(resident.columns, (column) => bind.addResultColumn(column.name, column.type))
          // Exact and free for a resident source, so it is spent: the setter refuses a `bigint`, and the arity crosses
          // as the number the row's own ceiling already bounded.
          bind.setCardinality(Array.headNonEmpty(resident.columns).vector.length, true)
        },
      }),
    initFunction: (init) => init.setInitData({ offset: 0, roster: init.getColumnIndexes() }),
    mainFunction: (info, chunk) => {
      // BOUNDARY ADAPTER: the engine types its per-scan slot `object | undefined`, so this read states the shape its
      // own setter wrote; re-entry is strictly serial on the node main thread, so the cursor is a plain field.
      const cursor = info.getInitData() as { offset: number; readonly roster: ReadonlyArray<number> }
      Option.match(_projected(registry, name, cursor.roster), {
        onNone: () => info.setError(`<source:${name}>`),
        onSome: (held) => {
          const width = Math.min(_SCAN.chunk, held.count - cursor.offset)
          // `rowCount` is a SETTER and zero is the scan's ONLY terminator, which is also why the source is resident: a
          // zero written while content is still arriving ends the scan and hands back a truncated relation as success.
          chunk.rowCount = width
          chunk.setColumns(Array.map(held.columns, (column) => Array.take(Array.drop(column.vector, cursor.offset), width)))
          cursor.offset += width
        },
      })
    },
  })

// Registration is INSTANCE-scoped and permanent, so the pump seats at the handle and the release arm retires the
// RESIDENCY alone — the name stays in the catalog refusing rather than serving what the last scope pumped. The whole
// unit rides one permit and the lane's own budget, exactly as a statement does.
const _sourced = (
  handle: Olap.Handle<"duckdbNode">,
  row: { readonly kind: Olap.Scanned; readonly resident: Olap.Resident; readonly token: string },
): Effect.Effect<string, OlapFault, Scope.Scope> =>
  Effect.gen(function* () {
    const token = yield* Effect.mapError(
      Schema.decode(_Token)(row.token),
      () => new OlapFault({ engine: handle.engine, reason: "wire", detail: `<source:token ${row.token}>` }),
    )
    const name = `${row.kind}_${token}`
    const count = Array.headNonEmpty(row.resident.columns).vector.length
    // Ceilings are the pump's whole bound, because no predicate reaches this scan: a `WHERE` clause reads every
    // resident row, so a source admitted past its row's number costs that memory on every statement that names it.
    yield* count <= _SOURCES[row.kind].ceiling
      ? Effect.void
      : Effect.fail(new OlapFault({ engine: handle.engine, reason: "wire", detail: `<source:${row.kind} rows=${count}>` }))
    return yield* Effect.acquireRelease(
      Effect.flatMap(handle.lease, (session) =>
        _governed(session)(
          { access: "write", fault: "wire" },
          Effect.as(
            Effect.zipRight(
              Effect.sync(() => MutableHashMap.set(handle.sources, name, row.resident)),
              // Both registration members are synchronous and `void`, so the lift is `Effect.try`: a `tryPromise` here
              // invents a suspension the surface never has and swallows the one throw registration can raise.
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

// One registration per residency, each naming its own member and its own protocol: `registerFileURL` carries HTTP for
// a presigned grant, `registerFileHandle` carries BROWSER_FILEREADER for a picked file, and `registerFileBuffer`
// carries no protocol argument at all — which is precisely why the member rides the row rather than one shared call
// taking a protocol value. The remaining four `DuckDBDataProtocol` arms stay unrostered: presigned HTTP is this lane's
// declared remote source, and no browser session holds a node filesystem, an S3 credential, or a file-system handle.
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

// Worker sessions reach their own engine off the leased connection, so the registration takes a session like every
// other unit of work the browser lane owns and no second handle crosses into this fold. Registration is SCOPED here
// because this build DOES publish a drop: a browser session that registers per view and never drops grows the worker's
// file registry for the tab's whole life, and each stale entry pins its own grant or blob.
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

- Owner: the durable-plane family — `_extensions` and its one statement generator, the `_SECRETS` credential rail, the `_attach` statements, the `_D`/`_METRIC_HEAD`/`_POINTS`/`_WIDE` column rosters and the `_mount` attach-and-create projection over them, the `_RESIDENCES` rows answering the estate residence floor (`fits`, `admit`, `tenancy`, `lifetime`, `degrade`, `cap` false) beside this plane's own read extension — relations, dialect, and fill — `_residence`/`_target` projecting a row into the core query algebra, `_KIND`/`_relation`/`_absorbed`/`_recorded` filling a plane from the object plane and the live registry, and `_joined` folding signal to evidence; each is data over the `Rows` read geometry, never a new engine surface.
- Packages: AWS, DuckDB, Effect, object storage, and core `Identity`, `Digest`, `Convention`, and `Board` supply this plane.
- Entry: a maintenance fold attaches the spine read-only for offload analytics and the lake catalog for evidence reads, spending `Olap.mount` once over the deploy plane's published metadata-and-data coordinate, which attaches the catalog and creates its relations in one statement; the signal egress calls `Olap.absorb` once per written object batch and a scheduled fold calls `Olap.recorded` once per scrape to fill the derived plane; the composition root calls `Olap.target(residence, identity, source)` once per board and hands the value to `core/observe/board#QUERY`, which renders every tile against it; `Olap.joined` answers a trace-to-receipt reconstruction as one statement.
- Growth: a new residence is one `_RESIDENCES` row answering the floor beside every read column; a new credential kind is one `_SECRETS` row; a new extension is one `_extensions` seed the single `extension(name)` generator consumes; a new metric point is one `_POINTS` row its DDL and fill both follow from; a new fillable plane is one `Olap.Plane` shape; a new join correspondence is one `_JOIN` pair.
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
- Law: the clickhouse plane's names are transcribed, never coined — `iac/operate/observe#CHART_ROWS` plants its wide-event relations and columns, so a rename there breaks this row at its declaration rather than emptying a tile at runtime.
- Law: the attribute plane derives from the identity projection — a key `Convention.identity` stamps reads off the resource map and every other key off the relation's own signal map, so the split follows the one projection both ends already agree on and no second roster drifts against it.
- Law: credentials never cross into engine SQL for an object plane — the `s3` and `azure` rows take `credential_chain`, so the row carries coordinates the object plane already publishes and the process's ambient grant carries material; `object/store.ts`'s sealed provider record stays sealed and this lane opens no second unwrap of it.
- Law: the `postgres` row is the one credential mint that carries material, and `_secret` is its single unwrap seam — the statement mints `fault: "secret"` and `access: "write"`, so it never rides the retry rail and the fault names the SECRET, never the statement, keeping the DSN out of every receipt, log, and fault detail.
- Law: `attach.pg` is read-offload only and binds through a minted secret rather than an inline DSN — the embedded engine reads the spine's tables without a second wire format, and no write path exists from the lane back into the OLTP transaction.
- Law: the lake is ACID over object storage with a SQL catalog — multi-table transactions, time travel, and schema evolution ride the catalog database; the object plane holds immutable Parquet, exactly the content-addressed posture the folder's object rows already enforce.
- Law: a residence names its own ingest owner on the row — `absorb` carries the fill dialect where this lane writes the plane and `None` where the collector's exporter does, so exactly one writer fills each plane and a second path cannot mint rows a retention owner never sees.
- Law: what a plane HOLDS and what any one writer routes into it are different questions, and this lane answers the first from the writer's end — the deploy plane's residence row censuses the same contents at its own grain, so the two ends state one census: the interactive plane holds the wide-event pair its collector leg carries, and the lake holds that pair BESIDE the OTLP metric-point relations this lane plants and fills, which is exactly the asymmetry `plant` and `metrics` already spell as `Some` here and `None` there.
- Law: the metric relations make the lake the ONE-plane join — a board joining series to wide events resolves both sides here, where the interactive plane forces that join across two planes and says so on its own `degrade`; the deploy plane publishes that same census on its residence row, so a reader picks its plane off a column at either end rather than inferring a plane's contents from which branch owns its writer.
- Law: no collector exporter frames a metric point at any residence, so a census widening here obligates NO deploy-plane routing change — the routing bound rides that tier's own exporter column and this lane's fill is the only writer the metric relations ever see; reading a routing answer as a contents answer at either end reports both planes as wide-event-only and strands every metric tile the lake already serves.
- Law: a fill dialect obligates a DDL projection at the same owner — the `plant` column carries every relation the lake's own fills write into and `Olap.mount` attaches and creates them in ONE statement, so a plane this lane writes exists on a clean catalog and `Olap.absorb` lands its first insert; a residence the collector plants carries neither column, which is what makes the pair total.
- Law: the lake spells the exporter's own column names and ORDER in DuckDB types, so a Parquet object written for either plane absorbs into the other `BY NAME` and neither plane forks the wide-event vocabulary.
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
- Law: the signal-to-evidence join rides SQL `ATTACH`, never a TypeScript import — the lake relations and the spine's `fact_journal` meet inside one embedded statement, so the strata direction this page holds against `journal` survives a join that reads both.

```typescript signature
import { quotedIdentifier, quotedString } from "@duckdb/node-api"
import { Array, DateTime, Duration, Match, Option, pipe, Record, Redacted } from "effect"
import { Board, Convention, type Digest, type Identity } from "@rasm/ts/core"
import { ObjectStore } from "../object/store.ts"

// One DuckDB `MAP` literal spelling for every attribute plane this lane writes — the identity overlay, the recorded
// fill's resource and signal maps — so a key or value never reaches a statement unescaped and no fill re-spells it.
const _mapped = (pairs: Record.ReadonlyRecord<string, string | undefined>): string =>
  pipe(
    Array.filterMap(Record.toEntries(pairs), ([key, value]) => Option.map(Option.fromNullable(value), (held) => [key, held] as const)),
    (present) => `MAP {${Array.join(Array.map(present, ([key, value]) => `${quotedString(key)}: ${quotedString(value)}`), ", ")}}`,
  )

// Roster order is admission concern — transport and credential, lake formats, foreign attach, index families.
// `postgres` and `sqlite` are the engine's own aliases for `postgres_scanner`/`sqlite_scanner` and the spelling its
// `ATTACH ... (TYPE …)` clause takes, so roster and clause name one token and the generator serves both.
const _extensions = ["httpfs", "aws", "azure", "ducklake", "iceberg", "delta", "postgres", "sqlite", "spatial", "vss", "fts"] as const

// One row per `CREATE SECRET` type: the provider that resolves credentials, the URI prefix its scope clause binds,
// and the storage the persistent form writes into. The object-plane rows take `credential_chain`, so their config
// carries only coordinates and the ambient grant carries material.
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

// `CREATE SECRET` takes scalars bare and strings single-quoted, and the redacted arm is the ONE unwrap on this lane.
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
    // Persistent custody writes the secret into the engine's own credential store, so a reconnect resolves it
    // without a second mint; session custody dies with the connection, which is what an ephemeral lane wants.
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

// Object-plane rows read the store's published coordinates and never its sealed keys: `credential_chain` walks
// this process's own grant, which is why no `Redacted` value appears on the path at all.
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
  // Empty paths are the engine's own spelling for "the named secret carries every coordinate", which keeps the DSN
  // out of the statement text a fault, receipt, or query log otherwise carries.
  pg: (secret: string) =>
    _Read.Rows({ sql: `ATTACH '' AS spine (TYPE postgres, SECRET ${quotedIdentifier(secret)}, READ_ONLY)`, fault: "extension", access: "write" }),
  secret: _secret,
  secrets: _SECRETS,
  sqlite: (path: string) => _Read.Rows({ sql: `ATTACH ${quotedString(path)} AS lane (TYPE sqlite)`, fault: "extension", access: "write" }),
} as const

// --- [RESIDENCE_FAMILY]

// DuckDB spellings for the exporter's own column vocabulary. The lake plants the same NAMES in its own types, so an
// object written against either plane absorbs `BY NAME` and no fill re-maps a column on the way in.
const _D = {
  bool: "BOOLEAN",
  flag: "UINTEGER",
  low: "VARCHAR",
  map: "MAP(VARCHAR, VARCHAR)",
  maps: "MAP(VARCHAR, VARCHAR)[]",
  real: "DOUBLE",
  reals: "DOUBLE[]",
  small: "UTINYINT",
  span: "UBIGINT",
  stamp: "TIMESTAMP_NS",
  stamps: "TIMESTAMP_NS[]",
  text: "VARCHAR",
  texts: "VARCHAR[]",
  whole: "UBIGINT",
  wholes: "UBIGINT[]",
} as const

// Every metric relation opens on one head — identity, scope, and descriptor columns the exporter writes ahead of the
// point payload — so a point row carries only what genuinely differs and no relation drifts its own head.
// Names stay LITERAL here rather than widening under an annotation, because the fill keys its head cells off this
// very roster and a widened name buys the projection back its positional hazard.
const _METRIC_HEAD = [
  ["ResourceAttributes", _D.map], ["ResourceSchemaUrl", _D.low], ["ScopeName", _D.text], ["ScopeVersion", _D.text],
  ["ScopeAttributes", _D.map], ["ScopeSchemaUrl", _D.low], ["ServiceName", _D.low], ["MetricName", _D.low],
  ["MetricDescription", _D.text], ["MetricUnit", _D.low], ["Attributes", _D.map], ["StartTimeUnix", _D.stamp],
  ["TimeUnix", _D.stamp],
] as const satisfies ReadonlyArray<readonly [name: string, type: string]>

// One row per OTLP metric POINT type: the relation holding it, the payload columns past the shared head, and the
// scalar a query fold reads off it. `value` differs by point because a bucket relation carries no per-point value.
const _POINTS = {
  exponentialHistogram: {
    columns: [
      ["Count", _D.whole], ["Sum", _D.real], ["Scale", _D.small], ["ZeroCount", _D.whole],
      ["PositiveOffset", _D.small], ["PositiveBucketCounts", _D.wholes], ["NegativeOffset", _D.small],
      ["NegativeBucketCounts", _D.wholes], ["Min", _D.real], ["Max", _D.real], ["Flags", _D.flag],
      ["AggregationTemporality", _D.small],
    ],
    relation: "otel_metrics_exponential_histogram",
    value: "Sum",
  },
  gauge: { columns: [["Value", _D.real], ["Flags", _D.flag]], relation: "otel_metrics_gauge", value: "Value" },
  histogram: {
    columns: [
      ["Count", _D.whole], ["Sum", _D.real], ["BucketCounts", _D.wholes], ["ExplicitBounds", _D.reals],
      ["Min", _D.real], ["Max", _D.real], ["Flags", _D.flag], ["AggregationTemporality", _D.small],
    ],
    relation: "otel_metrics_histogram",
    value: "Sum",
  },
  sum: {
    columns: [["Value", _D.real], ["Flags", _D.flag], ["AggregationTemporality", _D.small], ["IsMonotonic", _D.bool]],
    relation: "otel_metrics_sum",
    value: "Value",
  },
  summary: {
    columns: [
      ["Count", _D.whole], ["Sum", _D.real], ["ValueAtQuantiles.Quantile", _D.reals],
      ["ValueAtQuantiles.Value", _D.reals], ["Flags", _D.flag],
    ],
    relation: "otel_metrics_summary",
    value: "Sum",
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly columns: ReadonlyArray<readonly [name: string, type: string]>
  readonly relation: string
  readonly value: string
}>

// Wide-event relations transcribe `iac/operate/observe#CHART_ROWS`'s column list and ORDER verbatim into DuckDB
// types; a rename or reorder there breaks this roster at its declaration rather than emptying a tile at read time.
const _WIDE = {
  logs: {
    columns: [
      ["Timestamp", _D.stamp], ["TraceId", _D.text], ["SpanId", _D.text], ["TraceFlags", _D.small],
      ["SeverityText", _D.low], ["SeverityNumber", _D.small], ["ServiceName", _D.low], ["Body", _D.text],
      ["ResourceSchemaUrl", _D.low], ["ResourceAttributes", _D.map], ["ScopeSchemaUrl", _D.low],
      ["ScopeName", _D.text], ["ScopeVersion", _D.low], ["ScopeAttributes", _D.map], ["LogAttributes", _D.map],
      ["EventName", _D.text],
      ["ResourceAttributesKeys", _D.texts], ["ScopeAttributesKeys", _D.texts], ["LogAttributesKeys", _D.texts],
    ],
    plane: "LogAttributes",
    table: "otel_logs",
    text: "Body",
  },
  traces: {
    columns: [
      ["Timestamp", _D.stamp], ["TraceId", _D.text], ["SpanId", _D.text], ["ParentSpanId", _D.text],
      ["TraceState", _D.text], ["SpanName", _D.low], ["SpanKind", _D.low], ["ServiceName", _D.low],
      ["ResourceAttributes", _D.map], ["ScopeName", _D.text], ["ScopeVersion", _D.text], ["SpanAttributes", _D.map],
      ["Duration", _D.span], ["StatusCode", _D.low], ["StatusMessage", _D.text],
      ["Events.Timestamp", _D.stamps], ["Events.Name", _D.texts], ["Events.Attributes", _D.maps],
      ["Links.TraceId", _D.texts], ["Links.SpanId", _D.texts], ["Links.TraceState", _D.texts],
      ["Links.Attributes", _D.maps],
      ["ResourceAttributesKeys", _D.texts], ["SpanAttributesKeys", _D.texts],
    ],
    plane: "SpanAttributes",
    table: "otel_traces",
    text: "SpanName",
  },
} as const satisfies Record.ReadonlyRecord<"logs" | "traces", {
  readonly columns: ReadonlyArray<readonly [name: string, type: string]>
  readonly plane: string
  readonly table: string
  readonly text: string
}>

// Two durable planes, one row shape. `relations` names the wide-event tables and the attribute map each carries;
// `metrics` is Option-carried because only a plane holding OTLP metric relations answers the metric-series algebra,
// and a row that cannot answer says so here instead of exposing a second query path. `plant` is Some exactly where
// `absorb` is, because a plane this lane fills is a plane this lane must first create. Every column that differs by
// dialect — the map accessor, the identity concatenation — is a row function, so no render arm branches on a key.
// Read together, the two Option halves ARE this end's signal census — the pair alone on the collector-filled plane
// and the pair beside the metric relations on the one this lane fills — and the deploy plane's own residence row
// states that same census at its grain, so neither end infers a plane's contents from which branch wrote the bytes.
const _RESIDENCES = {
  clickhouse: {
    // Collector exporters fill this plane, so nothing here writes it: an `absorb` arm beside the exporter
    // double-writes the same wide events under two retention owners with no way to tell which row is which.
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
    // This lane fills the plane itself, so the row carries the two dialect spellings the fill needs and nothing else:
    // `scan` reads object-plane objects by URI and `merge` overlays the identity pairs onto the resource map.
    absorb: Option.some({
      merge: (plane: string, pairs: Record.ReadonlyRecord<string, string | undefined>) => `map_concat(${plane}, ${_mapped(pairs)})`,
      scan: (uris: ReadonlyArray<string>) => `read_parquet([${Array.join(Array.map(uris, quotedString), ", ")}], union_by_name = true)`,
    }),
    // Missing map keys read NULL on this engine, and a NULL operand poisons every comparison the predicate fold
    // builds, so the accessor coalesces to the empty string exactly as an absent attribute means on the wire.
    access: (map: string, key: Board.Query.Key) => `COALESCE(${map}[${quotedString(key)}], '')`,
    cap: false,
    catalog: "lake",
    degrade: "batch scan over object storage — a tile here reads as a report, never an interactive panel, and no Grafana driver reads the tree, so a board reaches it through a report this lane renders; a file-backed metadata catalog admits one process at a time, readers included, so a replicated stack publishes a server metadata DSN",
    engine: "duckdb",
    fits: "cold tail, cheapest per byte, batch scan",
    admit: "this lane's own `Olap.lake.sink` mints the objects and `Olap.absorb` lands them",
    lifetime: { bound: "table-format retention over the committed snapshot", owner: "object-plane" },
    tenancy: "partition column",
    // Keyed by OTLP point, never by instrument kind: the roster is the data model's, so the two points this estate
    // mounts nothing onto still relate and a foreign producer's rows land where the plane already declares them.
    metrics: Option.some({
      name: "MetricName",
      plane: "Attributes",
      tables: Record.map(_POINTS, (point) => point.relation),
      time: "TimeUnix",
      // Distribution rides WHOLE on the bucket relation — the edge list and the per-bucket counts — so the two
      // distribution reads answer off the relation's own columns instead of degrading to the scalar a `Sum` column
      // only approximates. Both are DuckDB dialect, which is why they ride the residence row beside `access` and
      // `series`: interpolation is the engine's, the arm that picks between them is the metric kind's alone.
      // `quantile` answers the rank's own bucket as that bucket's upper edge: the lambda's second parameter binds the
      // element's 1-based position and `list_slice` counts from that same base inclusively, so
      // `list_slice(BucketCounts, 1, i)` is the cumulative prefix THROUGH the current bucket and the walk lands on the
      // first bucket whose running total meets the rank. A rank in the overflow bucket resolves one position past the
      // edge list, so the extract answers NULL — the honest "above the top rung" verdict rather than a forged bound.
      quantile: (at: number) =>
        `list_extract(ExplicitBounds, list_position(`
        + `list_transform(BucketCounts, lambda c, i: list_sum(list_slice(BucketCounts, 1, i)) >= list_sum(BucketCounts) * ${at})`
        + `, true))`,
      // `fraction` answers the share at or under a DECLARED edge: cumulative count at that edge over the total. An
      // invented bound resolves no position, a NULL bound slices NULL, and that NULL rides out as the absent verdict
      // — a zero fallback here answers "nothing below this bound" for a bound the mint never froze, a share no
      // measurement produced, while a genuinely empty leading bucket still sums to a measured 0.
      fraction: (below: number) =>
        `list_sum(list_slice(BucketCounts, 1, list_position(ExplicitBounds, ${below})))`
        + ` / NULLIF(list_sum(BucketCounts), 0)`,
    }),
    // Every relation the lake's own fills write into, planted on the attach fold that mounts the catalog.
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
  // Floor column stated and permanently false: unbounded dimensionality IS why a residence exists, so a declared
  // `false` is what a later pass has to overwrite instead of a gap it can helpfully fill.
  readonly cap: false
  readonly catalog: string
  readonly degrade: string
  readonly engine: Board.Query.Engine
  readonly fits: string
  readonly admit: string
  // Both halves or none: a bound with no owner reads as a promise this lane keeps, and the lake row's bound is kept
  // by a plane this lane only writes into.
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
  readonly plant: Option.Option<ReadonlyArray<{ readonly columns: ReadonlyArray<readonly [name: string, type: string]>; readonly table: string }>>
  // Option-carried in BOTH halves, so the family expresses a plane holding metric relations and no wide-event pair
  // exactly as it already expresses the reverse; a required `relations` made the metric-only residence unspellable
  // and left `Board.Query`'s own dialect roster carrying a row no residence here could ever answer.
  readonly relations: Option.Option<Record.ReadonlyRecord<"logs" | "traces", { readonly plane: string; readonly table: string; readonly text: string }>>
  readonly resolution: Duration.Duration
  readonly resource: string
  readonly series: (parts: ReadonlyArray<string>) => string
  readonly time: string
}>

declare namespace Olap {
  type Point = keyof typeof _POINTS
  type Residence = keyof typeof _RESIDENCES
  // Relation keys read off the wide-event roster that OWNS them, never off a residence row: the row carries its
  // relations optionally now, so keying through it would narrow to whichever residence happened to hold a pair.
  type Relation = keyof typeof _WIDE
  // Two plane shapes a residence relation takes: a wide-event signal names its own relation key, a metric series
  // names the instrument kind whose table the row maps it onto, so one fill entry reaches both without a second member.
  type Plane = { readonly signal: Relation } | { readonly kind: Convention.InstrumentKind }
  // Both halves the deploy plane publishes: `metadata` locates the catalog the mount attaches — a `<file>.ducklake`
  // path or a `duckdb:`/`sqlite:`/`postgres:` prefix over one — and `data` the object-plane prefix its files land
  // under. The catalog BACKEND is the whole writer story, which is why it crosses as a coordinate and not a name.
  type Coordinate = { readonly data: string; readonly metadata: string }
}

// OTLP carries a monotonic sum, a non-monotonic sum, and a word census into the ONE sum relation, so three kinds share
// a point rather than the roster losing the two that would otherwise read as unrepresentable and drop their series.
// `_KIND` maps TOTAL over the instrument roster by its own contract, so a wire form the branch gains lands here in the
// same edit that mints it — a kind absent from this record is a producer whose series the plane silently never relates.
const _KIND = {
  counter: "sum",
  frequency: "sum",
  gauge: "gauge",
  histogram: "histogram",
  summary: "summary",
  updown: "sum",
} as const satisfies Record.ReadonlyRecord<Convention.InstrumentKind, Olap.Point>

// Mount and create are ONE statement, so the first `Olap.absorb` after a clean attach lands instead of failing at a
// relation nobody created; `IF NOT EXISTS` makes the whole fold idempotent, and a residence carrying no `plant` row
// answers `None` here exactly as it answers `None` for the fill, so no caller can mount a plane this lane cannot own.
// METADATA and ALIAS are two coordinates the row refuses to collapse: the engine resolves a bare `ducklake:<token>`
// as a SECRET name and refuses when none exists, so the metadata DSN arrives from the deploy plane beside the data
// path while the row's own `catalog` stays the alias every relation qualifies through.
const _mount = (key: Olap.Residence, coordinate: Olap.Coordinate): Option.Option<OlapRead> =>
  Option.map(_RESIDENCES[key].plant, (rows) =>
    _Read.Rows({
      sql: Array.join([
        `ATTACH IF NOT EXISTS ${quotedString(`ducklake:${coordinate.metadata}`)}`
        + ` AS ${quotedIdentifier(_RESIDENCES[key].catalog)} (DATA_PATH ${quotedString(coordinate.data)});`,
        ...Array.map(rows, (row) =>
          `CREATE TABLE IF NOT EXISTS ${_RESIDENCES[key].catalog}.${quotedIdentifier(row.table)} (`
          + `${Array.join(Array.map(row.columns, ([name, type]) => `${quotedIdentifier(name)} ${type}`), ", ")});`),
      ], " "),
      fault: "extension",
      access: "write",
    }))

// Resource and signal split off the ONE identity projection both ends already agree on: a key the resource stamps
// reads off the resource map, every other key off the relation's own map. Hand-listing that roster here drifts the
// first time an identity dimension lands or leaves.
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
      // `quantile` and `fraction` project the distribution reads onto the query algebra beside the scalar, so the
      // objective query and the burn panel render the same number against a residence they render against a metrics
      // store — the last case where a target swap changed what an operator read. The arm selects on the metric KIND,
      // never on the residence.
      quantile: metrics.quantile,
      fraction: metrics.fraction,
      // Bucket relations carry no per-point value column, so the scalar rides the KIND rather than the residence and
      // a fold over a histogram reads `Sum` where a fold over a counter reads `Value`.
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

// One relation resolution over both plane shapes: a wide-event key reads the row's own relation entry and a metric
// kind reads the metric table map, so a residence holding no metric relation answers `None` here exactly as it does
// for the query target and no caller learns the difference through a second member.
const _relation = (held: (typeof _RESIDENCES)[Olap.Residence]) =>
  Match.type<Olap.Plane>().pipe(
    Match.when(
      { signal: Match.string },
      ({ signal }) => Option.map(held.relations, (rows) => ({ plane: rows[signal].plane, table: rows[signal].table })),
    ),
    Match.orElse(({ kind }) => Option.map(held.metrics, (metrics) => ({ plane: metrics.plane, table: metrics.tables[_KIND[kind]] }))),
  )

// Filling IS what keeps the derived plane joinable: every absorbed row leaves carrying the whole
// `Convention.identity` projection on its resource map, which is the same key the evidence join reads back, so a
// plane rebuilt from object storage answers the reconstruction on its first read rather than after a second pass
// stamps identity onto it. `BY NAME` binds the Parquet footer's columns to the relation by name, so a producer
// adding a column ahead of the residence widens nothing here; an unresolved identity dimension contributes no pair.
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

// One INSERT per point relation the snapshot reached: `Board.DashboardModel.snapshot` filters the live registry to
// `Convention.metric` rows and its `Signal` family discriminates exactly the point types the plane relates, so the
// projection is a total match and a state shape with no relation is unrepresentable rather than silently dropped.
// Frequency states fan one row per occurrence under the owned word axis, which is why every arm yields an array.
// Cells key by COLUMN NAME off the point's own roster, never by position: `_POINTS` owns membership and order alike,
// so an arm omitting a column or misspelling one fails at its own declaration, and reordering a point moves the
// SELECT list and the fill in one edit. Positional tuples file every cell past a reordered column under its
// neighbour's name — a bucket count landing in `Sum` reads as a plausible number with no error anywhere.
type _Cell<P extends Olap.Point> = {
  readonly attributes: Record.ReadonlyRecord<string, string | undefined>
  readonly cells: { readonly [K in (typeof _POINTS)[P]["columns"][number][0]]: string }
  readonly name: Convention.MetricName
  readonly point: P
}

const _CELLS = {
  // Effect stores a signed level as a counter state, so monotonicity reads the signal's own DECLARED wire form —
  // a `updown` row planting `is_monotonic` true files a level that falls as a total that only rises.
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
  // Effect states buckets as CUMULATIVE counts against their upper bound where OTLP states per-bucket counts against
  // that bound roster minus its overflow edge, so this arm deltas the counts and drops the trailing edge exactly once.
  // Adjacency pairing states the delta, never an index step back into the source: an index-guarded read carries no
  // type-level evidence under unchecked-index semantics, so it needs an assertion to compile at all — zipping the
  // counts against themselves prepended by the zero origin states the same recurrence totally and index-free.
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
  // Unmeasured quantiles contribute to neither list, so both stay index-aligned by construction.
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
  // Registry states carrying no OTLP point shape relate to no relation, so they contribute no row at all.
  Unknown: (): ReadonlyArray<_Cell<Olap.Point>> => [],
} as const

// Head cells key by name for the reason the point cells do — `_METRIC_HEAD` owns the order, this record owns the
// values, and the projection reads one against the other, so the resource and scope halves never slide a column apart.
// Descriptor text is `Convention.Metric`'s alone, so the census row a mount already published is the only description
// and unit that ever reach the plane.
const _headed = (
  row: { readonly at: DateTime.Utc; readonly identity: Identity.App },
  scope: ReturnType<typeof Convention.scope>,
  entry: { readonly attributes: Record.ReadonlyRecord<string, string | undefined>; readonly name: Convention.MetricName },
): { readonly [K in (typeof _METRIC_HEAD)[number][0]]: string } =>
  pipe(`${quotedString(DateTime.formatIso(row.at))}::${_D.stamp}`, (stamp) => ({
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

// Lake METRICS rise from this process's own registry, folded into the point relations the plane plants. Identity
// merges here exactly as the object fill merges it onto a scanned object, so both fills leave the plane joinable on
// arrival and `_JOIN` reads one key set whichever fill wrote the row. `_recorded` is the cold tail a metrics store's
// retention cannot hold, minted with no second exporter, no second wire, and no object round trip.
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
            // `Match.valueTags` is the one-shot record dispatch over an already-held union: it correlates each arm to the
            // case its own discriminant selected, where an indexed read off the tag hands the call a union of signatures
            // whose only common parameter is `never` — an assertion standing in for the correlation the dispatch form
            // already carries, and one that swallows a genuinely mistyped arm alongside it.
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
                            // BOUNDARY ADAPTER: `Array.groupBy` keys by a string and erases the point-to-cells
                            // correlation each arm declared, so one index rejoins them. Head and payload each
                            // project through the roster that also spells the `AS t(…)` alias below, which is what
                            // keeps every value under the column its own owner named.
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

// Identity is the join key, spelled once per side: the signal plane carries the whole `Convention.identity`
// projection on its resource map and the journal declares two columns, so the correspondence is a row table rather
// than a predicate re-derived per call site. A caller extending the correspondence — a content-key attribute the
// span plane stamps against the receipt key the journal payload carries — passes one more pair and nothing else moves.
const _JOIN: ReadonlyArray<readonly [key: Board.Query.Key, column: string]> = [
  [Convention.attr.serviceName, "app"],
  [Convention.rasm.tenant, "tenant"],
]

// One statement, two planes: the DuckLake-attached relation and the ATTACH-mounted `fact_journal`. The window bounds
// both sides so neither scans past the reconstruction the caller asked for, and the evidence side stays the truth —
// this fold reads it, never writes it.
const _joined = (row: {
  readonly identity: Identity.App
  readonly on?: ReadonlyArray<readonly [key: Board.Query.Key, column: string]>
  readonly relation?: Olap.Relation
  readonly residence: Olap.Residence
  readonly window: Duration.Duration
}): Option.Option<OlapRead> =>
  Option.map(
    // Absent relations answer `None` here exactly as an absent metric plane answers `None` at the query target, so a
    // residence holding no wide-event pair reconstructs nothing rather than joining against a table nobody planted.
    Option.map(_RESIDENCES[row.residence].relations, (rows) => ({
      held: _RESIDENCES[row.residence],
      // Evidence compares in millis and the span side in seconds, so both bounds derive from the ONE window
      // through its own duration projections rather than a hand-scaled literal drifting from its twin.
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
                  // Journals write NULL for an absent tenant while the wire writes no attribute at all, so both
                  // sides normalize to the empty string; comparing NULL to '' drops every single-tenant row silently.
                  ...Array.map(row.on ?? _JOIN, ([key, column]) => `${attribute(key)} = COALESCE(f.${column}, '')`),
                  // Window bounds close on the fact's OWN stamp, never on `recorded_at`: a row landing late under
                  // a dead database carries a write time hours past its span and drops out of this join silently.
                  // Both sides reduce to epoch millis, so the comparison is integer and the correlation exact.
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

```typescript signature
import { Clock, Config, type ConfigError, type Layer } from "effect"
import { ClickhouseClient } from "@effect/sql-clickhouse"
import { RateLimiter } from "@effect/experimental"
import { SqlClient, type SqlError, type Statement } from "@effect/sql"
import type { Identity } from "@rasm/ts/core"

const _clickhouse: Layer.Layer<ClickhouseClient.ClickhouseClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =
  ClickhouseClient.layerConfig({
    url: Config.string("DATA_CLICKHOUSE_URL"),
    // this driver declares `password?: string`, so a `Redacted` config value refuses at the wrap and custody rides the env read
    password: Config.string("DATA_CLICKHOUSE_PASSWORD"),
  })

// `RateLimiter` publishes the quota vocabulary as its own options row — `Effect.Effect.Success` reaches the factory and
// `Parameters` its options — so a posture override names fields the package already declares and no lane-local mirror
// drifts on the next release; the bucket key subtracts because the owner alone decides it.
type _Quota = Omit<Parameters<Effect.Effect.Success<typeof RateLimiter.makeWithRateLimiter>>[0], "key">

const _INGEST_QUOTA = { algorithm: "token-bucket", limit: 50, onExceeded: "delay", window: "1 second" } as const satisfies _Quota

// Three rails converge here, and a fault the governor already minted on the inner statement passes through untouched
// so a budget timeout keeps the reason its own bracket named. BOTH limiter refusals answer `_tag: "RateLimiterError"`
// — the tag names the RAIL and `reason` names the case — so the quota arm keys on `"Exceeded"` and a fold reaching for
// a `RateLimitExceeded` tag matches nothing and files every quota refusal as a store fault. That arm cannot read
// `message` either: the class hard-codes it to `Rate limit exceeded`, carrying no key, no bound, and no wait, so the
// detail crosses the published row and `after` crosses the measured span. `SqlError` and the store refusal own real
// message text, which is the whole reason they keep the residue arm.
const _lifted = <A, R>(
  reason: OlapFault["reason"],
  work: Effect.Effect<A, OlapFault | SqlError.SqlError | RateLimiter.RateLimiterError, R>,
): Effect.Effect<A, OlapFault, R> =>
  Effect.mapError(work, (cause) =>
    cause._tag === "OlapFault"
      ? cause
      : cause._tag === "RateLimiterError" && cause.reason === "Exceeded"
      ? new OlapFault({
        engine: "clickhouse",
        reason,
        detail: `<quota:${cause.key} limit=${cause.limit} remaining=${cause.remaining}>`,
        after: cause.retryAfter,
      })
      : new OlapFault({ engine: "clickhouse", reason, detail: cause.message }))

declare namespace Olap {
  // `Parameters` resolves an overloaded member against its LAST signature, which is the data-first arm here — so the
  // settings type derives from the driver instead of re-importing `@clickhouse/client`, a package no manifest admits.
  type Ingest = Parameters<ClickhouseClient.ClickhouseClient["insertQuery"]>[0] & {
    readonly app: Identity.App.Key
    readonly queryId: string
    readonly quota?: Partial<_Quota>
    readonly settings: Parameters<ClickhouseClient.ClickhouseClient["withClickhouseSettings"]>[1]
  }
  type Wide = {
    // a projection names at least one column, so an empty request is unspellable rather than a `SELECT` with nothing
    // between it and its `FROM`; which names are legal is the relation roster's answer at the fold below.
    readonly columns: Array.NonEmptyReadonlyArray<string>
    readonly identity: Identity.App
    readonly labels?: Board.Query.Labels
    readonly relation?: Relation
    readonly residence: Residence
    readonly where?: Statement.Fragment
  }
}

// Residence rows spell the table, the attribute plane, and the accessor, so a wide-event read names a relation
// and a label set and never a schema; the caller's own `Fragment` carries every remaining predicate parameterized,
// and the projection admits against the relation's declared column roster before a single name reaches the statement.
const _wide = (row: Olap.Wide) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const held = _RESIDENCES[row.residence]
    const signal = row.relation ?? "traces"
    // Residences carrying no wide-event pair refuse BY NAME here, because a read is an Effect and a silent empty
    // answer reads as "no rows matched" for a relation the plane never held at all.
    const relation = yield* Option.match(Option.map(held.relations, (rows) => rows[signal]), {
      onNone: () => Effect.fail(new OlapFault({ engine: "clickhouse", reason: "query", detail: `<relation:${signal}>` })),
      onSome: Effect.succeed,
    })
    const attribute = _planed(held, row.identity, relation.plane)
    const matched = Array.filterMap(
      Convention.keys,
      (key) => Option.map(Option.fromNullable(row.labels?.[key]), (value) => sql`${sql.literal(attribute(key))} = ${String(value)}`),
    )
    // `_WIDE[signal].columns` bounds the whole projection vocabulary: a caller-supplied name reaches statement text
    // nowhere, an undeclared one refuses BY NAME rather than dropping out of a result the caller still reads as
    // complete, and every admitted one crosses as an `Identifier` the compiler escapes — which is also what makes a
    // dotted attribute column read as one name instead of a table reference the parser then cannot resolve.
    const [unknown, admitted] = Array.partition(row.columns, (name) => Array.some(_WIDE[signal].columns, ([declared]) => declared === name))
    const projected = yield* Array.match(unknown, {
      onEmpty: () => Effect.succeed(sql.csv(Array.map(admitted, (name) => sql`${sql(name)}`))),
      onNonEmpty: (missing) =>
        Effect.fail(new OlapFault({ engine: "clickhouse", reason: "query", detail: `<column:${Array.join(missing, ",")}>` })),
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
      // Per-stream posture narrows the standing row and never the key: an oversized batch draws its own `tokens`
      // while the bucket stays the app's, so weighting a stream cannot leak it into a sibling tenant's window.
      limit({ ..._INGEST_QUOTA, ...quota, key: `olap:ingest:${app}` })(
        Effect.zipRight(
          Effect.flatMap(Clock.currentTimeMillis, (started) => _meter.deferred.clickhouse(Duration.millis(started - opened))), // Delta is the quota hold before insert admission.
          // Budgets bracket the ADMITTED insert alone: bracketing the limiter too converts a deliberate quota
          // suspension into a fault and makes the delay disposition lossy exactly where it exists not to be.
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
- Packages: `@qualithm/arrow-flight-client` (`createFlightSqlClient`, `FlightClient`, `FlightSqlClient`, `AuthOptions`, `AuthProvider`, `TlsOptions`, `FlightAction`, `FlightClientOptions`, `FlightCriteria`, `FlightData`, `FlightDescriptor`, `FlightDescriptorInput`, `FlightEndpoint`, `FlightInfo`, `PollInfo`, `PreparedStatement`, `SchemaResult`, `Transaction`, `UpdateResult`, `ActionType`, `Result`, `FlightAuthError`/`FlightConnectionError`/`FlightServerError`); `apache-arrow` (`RecordBatch`, `Table`); `@rasm/ts/core` (`Wire.Hops`); `effect` (`Array`, `Data`, `Effect`, `Exit`, `Match`, `Option`, `Record`, `Redacted`, `Stream`).
- Entry: a service composes `Olap.flight` once per remote coordinate and calls `Olap.flown(client, intent)` per unit of work; `Fetch` fans every endpoint of a published plan, `Frames` hands the wire's own messages through undecoded, `Put` uploads Arrow frames through `doPut`, `Bound` binds parameters to a server-side plan, `Act` reaches the server's own action vocabulary, and `Olap.transacted` brackets a sequence needing atomicity at the far end.
- Receipt: reads land as `apache-arrow` `Table` or a `RecordBatch` stream — the same plane `Olap.wire` and `Olap.ingest` carry, so a Flight result reaches the viewer with no re-encoding; `Frames` lands raw `FlightData`, `Bound` and `Update` land the server's own `recordCount`, `Put` streams its upload acknowledgements, and `Plan`, `Poll`, and `Schema` land `FlightInfo`, `PollInfo`, and `SchemaResult` whole so a caller reads schema, endpoints, and progress without executing.
- Growth: a new Flight capability is one `OlapFlown` case with its row in the matching dispatch half; a new zero-argument catalog read is one `_METADATA` row; a new writing case is one `_WRITES` entry; a refusal class the package starts minting is one `_faulted` arm; nothing here grows a second client or a second transport.
- Law: transport is the package's own — one `@connectrpc/connect` stack over `node:http2` carries every RPC, so this lane imports no connect module, mints no channel, and admits no second gRPC client beside it.
- Law: construction is synchronous and lifecycle is `acquireRelease` under `Scope` like every engine row — `createFlightSqlClient` returns the client outright, so the acquire arm is `Effect.try` and the release arm `Effect.sync` over a `void` `close()`; a promise lift on either end invents a suspension the package never has and swallows a disposal fault.
- Law: credential material crosses as `Redacted` and unwraps INSIDE the `authProvider` thunk `_sealed` mints — the bearer token and the handshake password never land on the options record at all, so a coordinate reaching a fault detail, a receipt, or a structured log holds a closure rather than material; the client key and its passphrase unwrap at `_sealed` itself because `tls` has no thunk arm, a certificate and a CA chain are public material and cross bare, and that split is what makes the sealed set exactly the four.
- Law: the thunk is also the rotation seam — the package resolves it before the first request and again on every unauthenticated refusal, caching between, so a `Redacted` value re-read from its own source adopts a rotated credential on the next resolve while `authenticate()` adopts it EAGERLY for a lane that already knows the turnover happened; a client torn down and rebuilt under a fresh scope to take a new credential is the shape this thunk deletes.
- Law: the bound is the lane's alone — the package resolves `timeoutMs` onto its options record and applies it to nothing, so the field passes through for the pin that wires it while `_resilient` bounds every answer and `Stream.timeoutFail` bounds every emission; a design resting on the package's own timeout leaves a stalled far end holding its fiber forever.
- Law: an emitting case bounds on IDLE, never on total elapsed — a legitimately long partitioned read outlives any whole-stream budget, so the governor budget bounds the gap between frames and the resulting fault answers once; a total-elapsed bound kills the exact reads the fan exists to serve.
- Law: the refusal fold reads the ONE gRPC status algebra the estate already owns — `core/interchange/codec#LANDING_WIRE`'s `Wire.Hops` rows carry the numeric code, its retryability, and its fault class, so `_faulted` projects those columns onto this lane's reasons and a retryability edit lands at that owner with no edit here; a second code roster spelled beside the client forks the classification the first hop already settled.
- Law: the classifier keys on the refusal classes this pin mints — `FlightServerError` carries every transport and far-end verdict because the package's own auth branch compares `code` against status NAMES while ConnectRPC hands it the numeric `Code`, `FlightAuthError` reaches only a handshake answering nothing, and `FlightConnectionError` only a raw socket error carrying no `code`; `FlightTimeoutError` and `FlightCancelledError` are exported and never thrown, so an arm for either is dead the day it lands.
- Law: `FlightServerError.code` is DECLARED `string` and POPULATED with a number, so it reads through the ONE partial numeric admission — `Number.parse` answers `Some` for a code a `Wire.Hops` row takes the verdict from and `None` for everything else, a node syscall string off a raw socket error, transport by construction and replayable like any unreachable coordinate; the bare `Number()` coercion behind an `isNaN` guard is the deleted spelling because it resolves the empty and whitespace codes to `0`, the OK status, and classifies a refusal carrying no code as a broken codec crossing that never replays.
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

```typescript signature
import { Exit, Match, Number, Redacted } from "effect"
import type { RecordBatch, Table } from "apache-arrow"
import { Wire } from "@rasm/ts/core"
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

// Recovery reads the hop row's own columns rather than a second status roster: an unreachable coordinate is the
// acquire the governor already replays, a denied verdict routes the credential rail, every other retryable arrival
// replays as a query, and each terminal verdict answers once — so a retryability edit at `interchange/codec` lands
// here unedited. Field patterns subtract, and the residue is genuinely open because `class` is `Fault.Class`-wide.
const _hopped = Match.type<Wire.Hops.Row & { readonly reason: Wire.Hops.Reason }>().pipe(
  Match.when({ reason: "unavailable" }, () => "acquire" as const),
  Match.when({ class: "denied" }, () => "secret" as const),
  Match.when({ retryable: true }, () => "query" as const),
  Match.orElse(() => "wire" as const),
)

// Subclasses first: each also answers `FlightError.isError`, so a base-first ladder would swallow every arm whose
// recovery differs. Only these three mint at this pin, and the server arm carries every transport and far-end verdict
// — its `code` is declared a `string` and populated with ConnectRPC's numeric `Code`, so `Number.parse` is the one
// partial admission that resolves it, and its absent arm is the node syscall string the raw-socket branch admits.
// Behind an `isNaN` guard a bare coercion resolves an empty code to `0`, the OK status, filing a refusal carrying no
// code as a terminal codec crossing; the `Option` names that arm instead and spends one coercion rather than two.
const _faulted = Match.type<unknown>().pipe(
  Match.when(FlightConnectionError.isError, () => "acquire" as const),
  Match.when(FlightAuthError.isError, () => "secret" as const),
  Match.when(FlightServerError.isError, ({ code }) =>
    Option.match(Number.parse(code), {
      onNone: () => "acquire" as const,
      onSome: (held) => pipe(Wire.Hops.fromCode(held), (reason) => _hopped({ ...Wire.Hops[reason], reason })),
    })),
  Match.orElse(() => "wire" as const),
)

// `FlightServerError` alone carries a far-end diagnostic — `details` holds `ConnectError.rawMessage`, server text
// stripped of the status prefix the thrown message already repeats — so both survive on one detail; a fold reading
// `String(cause)` alone discards the only field the far end wrote for a reader.
const _detailed = (cause: unknown): string =>
  FlightServerError.isError(cause) && cause.details !== undefined ? `${String(cause)} ${cause.details}` : String(cause)

const _flightFault = (cause: unknown): OlapFault =>
  new OlapFault({ engine: "flight", reason: _faulted(cause), detail: _detailed(cause) })

const _flew = <A>(run: () => Promise<A>): Effect.Effect<A, OlapFault> =>
  Effect.tryPromise({ try: run, catch: _flightFault })

// `exactOptionalPropertyTypes` makes a present-but-undefined key a different value from an absent one, and every
// filter the package declares is absent-or-present — an explicit `undefined` reaches the server as a supplied empty
// pattern. BOUNDARY ADAPTER: `Record.filter` erases the per-key correlation the mapped return type re-states.
const _present = <T extends Record.ReadonlyRecord<string, unknown>>(row: T): { readonly [K in keyof T]?: NonNullable<T[K]> } =>
  Record.filter(row, (value) => value !== undefined) as { readonly [K in keyof T]?: NonNullable<T[K]> }

declare namespace Olap {
  // Custody widens the package's two material-bearing records and nothing else: the token, the password, the client
  // key, and its passphrase arrive `Redacted` so material has exactly one unwrap on this lane, and every remaining
  // coordinate IS the package's own options record, because a lane-local mirror drifts on the first upstream field.
  type Auth =
    | { readonly type: "bearer"; readonly token: Redacted.Redacted<string> }
    | { readonly type: "basic"; readonly credentials: { readonly password: Redacted.Redacted<string>; readonly username: string } }
    | { readonly type: "none" }
  // Certificates and CA chains are published material and stay bare; a private key and its passphrase are the two
  // fields a coordinate printed into a log must never hold, so the seal covers exactly those.
  type Tls = Omit<TlsOptions, "key" | "passphrase"> & {
    readonly key?: Redacted.Redacted<string | Buffer>
    readonly passphrase?: Redacted.Redacted<string>
  }
  type Flight = Omit<FlightClientOptions, "auth" | "authProvider" | "tls"> & { readonly auth?: Auth; readonly tls?: Tls }
  type Dataset = FlightDescriptorInput
  type Prepared = PreparedStatement
  type Update = UpdateResult
  // Upload acknowledgements are the one Flight message the package's root leaves unexported, so this type derives
  // from the member that yields it rather than from an import nothing resolves.
  type Ack = ReturnType<FlightClient["doPut"]> extends AsyncIterable<infer P> ? P : never
}

// Remote coordinates answer on the lane's own budget, so one governor row bounds an embedded statement, a Flight
// answer, and the gap between two Flight frames alike; `endpoints` is the fan width an unordered plan reads at.
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

// Auth crosses as a thunk, so the token and the password unwrap per resolve and never sit on the record a log could
// print; that same laziness is the rotation seam, since the package re-resolves on every unauthenticated refusal.
// `timeoutMs` leads so a coordinate naming its own budget still wins the spread; the package applies neither today.
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
    (held) => Effect.sync(() => held.close()), // close() is void on both clients — a promise lift here would hide a disposal fault
  )

// Two zero-argument catalog reads keyed by their own name: a ternary would restate a correspondence the table already
// carries, and a third catalog the server publishes is one row.
const _METADATA = {
  catalogs: (client: FlightSqlClient) => client.getCatalogs(),
  tableTypes: (client: FlightSqlClient) => client.getTableTypes(),
} as const satisfies Record.ReadonlyRecord<string, (client: FlightSqlClient) => Promise<Table>>

// One closed intent family over every Flight modality — reads, lazy batch streams, the endpoint fan a partitioned
// plan demands, DML, bulk upload, plan and progress inspection, catalog discovery, and the server's own action
// vocabulary — so a capability is a case rather than a suffix family, and the transaction handle rides the case so an
// atomic sequence needs no second entry.
type OlapFlown = Data.TaggedEnum<{
  Act: { readonly action: FlightAction }
  Actions: Record<string, never>
  Batches: { readonly sql: string; readonly transaction?: Transaction }
  // Binds ride the package's own union rather than a materialized array, so a generator-produced parameter set larger
  // than memory is spellable on exactly the leg that takes one.
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
  // Descriptors already echoed ride straight through, so a bulk run stamps every frame set off one plan read
  Put: { readonly dataset: Olap.Dataset | FlightDescriptor; readonly source: Olap.Frame }
  Query: { readonly sql: string; readonly transaction?: Transaction }
  Schema: { readonly dataset: Olap.Dataset }
  Schemas: { readonly catalog?: string; readonly schema?: string }
  Streamed: { readonly sql: string; readonly transaction?: Transaction }
  // Both filter patterns and the schema fetch are the caller's: a discovery walk over a warehouse narrows by table
  // name, and pulling every table's Arrow schema is a cost the roster read alone never wants.
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

// Two halves of the family, each keyed by its own tags: one value per case on the left, one element grain per case
// on the right. `Olap.flown` derives its return type from these maps, so an overload never restates a case's shape.
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

// Replay lands rows twice at a far end this lane cannot roll back, so the writing cases name themselves and every
// other answer replays under the shared budget.
const _WRITES: ReadonlyArray<keyof _Answered> = ["Bound", "Update"]

// Prepared plans are server-side resources: the release arm closes one on every exit, interruption included, so an
// abandoned fiber never strands a plan at the far end.
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

// Uploads carry the descriptor on their first frame, and that descriptor is a protobuf message whose schema value
// this package keeps behind an unexported subpath — so the server's own echo supplies it. A message the caller already
// holds passes through on its own `$typeName`, which is what lets a bulk run pay one plan read rather than one per
// frame set; a plain input still buys its echo, because an assembled literal is unspellable at this pin.
const _dataset = (client: FlightSqlClient, dataset: Olap.Dataset | FlightDescriptor): Effect.Effect<FlightDescriptor, OlapFault> =>
  "$typeName" in dataset
    ? Effect.succeed(dataset)
    : Effect.flatMap(
      _flew(() => client.flight.getFlightInfo(dataset)),
      (info) =>
        Option.match(Option.fromNullable(info.flightDescriptor), {
          onNone: () => Effect.fail(new OlapFault({ engine: "flight", reason: "wire", detail: "<no-descriptor>" })),
          onSome: Effect.succeed,
        }),
    )

// Flight spells "redeem where the ticket was minted" two ways — an empty roster and this sentinel — and every other
// authority names a service holding splits this coordinate cannot reach.
const _REUSE = ["", "arrow-flight-reuse-connection://?"] as const

const _reused = (endpoint: FlightEndpoint): boolean =>
  Array.length(endpoint.location) === 0 || Array.some(endpoint.location, (held) => Array.contains(_REUSE, held.uri))

// `ordered` is the server's own statement that the splits carry a sequence, so it decides the fan width rather than a
// knob. An endpoint IS a portion of the result, so one this coordinate cannot redeem raises: skipping a foreign
// location or a ticketless publication returns a fraction under a success exit, which is the whole-flight defect.
const _fanned = (client: FlightSqlClient, info: FlightInfo): Stream.Stream<RecordBatch, OlapFault> =>
  Stream.flatMap(
    Stream.fromIterable(info.endpoint),
    (endpoint) =>
      _reused(endpoint)
        ? Option.match(Option.fromNullable(endpoint.ticket), {
          onNone: () => Stream.fail(new OlapFault({ engine: "flight", reason: "wire", detail: "<endpoint:no-ticket>" })),
          onSome: (ticket) => _wire.flight.streamed(client.flight.doGet(ticket)), // the `Ticket` message satisfies `FlightTicket` structurally
        })
        : Stream.fail(
          new OlapFault({
            engine: "flight",
            reason: "wire",
            detail: `<endpoint:located ${Array.join(Array.map(endpoint.location, (held) => held.uri), " ")}>`,
          }),
        ),
    { concurrency: info.ordered ? 1 : _FLIGHT.endpoints },
  )

const _ANSWERS: {
  readonly [K in keyof _Answered]: (
    client: FlightSqlClient,
    intent: Extract<OlapFlown, { readonly _tag: K }>,
  ) => Effect.Effect<_Answered[K], OlapFault>
} = {
  // `executePrepared` takes no parameters at this pin, so a bind sequence IS the update leg: the plan carries the
  // statement, the batches carry the values, and the server answers its own affected count.
  Bound: (client, { binds, sql, transaction }) =>
    _planned(client, { sql, transaction }, (statement) => _flew(() => client.executePreparedUpdate(statement, binds))),
  Catalogs: (client, { kind }) => _flew(() => _METADATA[kind](client)),
  Keys: (client, { catalog, schema, table }) => _flew(() => client.getPrimaryKeys(table, _present({ catalog, dbSchema: schema }))),
  // `getQueryInfo` answers the plan, schema, and endpoints WITHOUT executing, so a cost check never pays for rows
  Plan: (client, { sql, transaction }) => _flew(() => client.getQueryInfo(sql, _present({ transactionId: transaction?.id }))),
  Poll: (client, { dataset }) => _flew(() => client.flight.pollFlightInfo(dataset)), // progress on a long-running plan
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
  // Frames cross undecoded: a schema read that never materializes rows takes `Olap.wire.flight.header` off this
  // grain, and a relay onward re-frames nothing because the messages never left their encoding.
  Frames: (client, { sql, transaction }) =>
    Stream.fromAsyncIterable(client.queryStream(sql, _present({ transactionId: transaction?.id })), _flightFault),
  Put: (client, { dataset, source }) =>
    Stream.unwrap(Effect.map(
      _dataset(client, dataset),
      (descriptor) => Stream.fromAsyncIterable(client.flight.doPut(_wire.flight.framed(source, descriptor)), _flightFault),
    )),
  // Plans live exactly as long as their stream, so an abandoned consumer closes the plan instead of stranding it
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
  // BOUNDARY ADAPTER: the keyed dispatch erases the case-to-arm correlation each mapped record declares, so one cast
  // per half rejoins the arm to the case its own discriminant selected; the halves are disjoint by construction and
  // each is total over its own keys, so no arm can go missing and no residue arm exists to go stale.
  // Emission bounds on the gap between frames, never on total elapsed: a partitioned read outlives any whole-stream
  // budget, and the fault answers once because a partial-output replay duplicates rows the consumer already took.
  return Record.has(_STREAMS, intent._tag)
    ? Stream.timeoutFail(
      (_STREAMS[intent._tag as keyof _Emitted] as (
        client: FlightSqlClient,
        intent: OlapFlown,
      ) => Stream.Stream<_Emitted[keyof _Emitted], OlapFault>)(client, intent),
      () => new OlapFault({ engine: "flight", reason: "wire", detail: "<idle-budget>" }),
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

// Atomicity at the far end: the commit rides the USE arm so its own failure lands on the typed rail and the release
// arm still rolls back, where a release-arm commit only dies and leaves the caller reading success.
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
- Packages: `./postgres.ts` (`Pg.Profile` — the shared receipt schema; profile parity across pg, sqlite, and this lane is one class); `@effect/sql` (`SqlClient`); `@effect/sql-clickhouse` (`ClickhouseClient`, `asCommand`); `effect` (`Array`, `Exit`, `Option`, `Order`, `Record`, `Schema`, `pipe`).
- Entry: an explicit diagnosis call runs `Olap.profile({ handle, label, statement })` against an embedded handle or `Olap.profile({ label, queryId })` against the cluster; the maintenance composition runs `Olap.probe` in idle windows per the budget row, holds the prior evidence beside the engine row, and folds `Olap.armed` — the composition seam folds `priced` once, so an unpriceable incumbent fans nothing and an armed reading hands the `rasm.data.lane.escalate` fact its delta out of the same arm that armed it, which is why that point declares `delta` required; this lane keeps its single `ObjectStore` value seam beside the profile-schema read and never imports the hook registry.
- Receipt: `Pg.Profile` with `engine: "duckdbNode"`, operator rows carrying timing and cardinality from the plan tree, and `counters` carrying every `_COUNTERS` measure the root reported; `Olap.Evidence` — `{ engine, statement, runs, wallP50, wallMax, rows }`; `Olap.Escalation` — `{ candidate, trigger, priced }` whose `priced` carries `{ armed, delta }` where the incumbent's p50 admits a ratio and nothing where it does not, the row's trigger text riding as data so the review argues from the table.
- Growth: a profile engine enters `_PROFILE_ENGINES` only with its landed harvest arm; a probe budget posture is a `_PROBE` field override; a new profile counter is one `_COUNTERS` row whose decode field and receipt key both follow.
- Law: profiling toggles are per-connection state — one permit spans enable, execution, and disable, so concurrent users cannot interleave inside the bracket; `_profileRowsOnce` bypasses the retry governor because `EXPLAIN ANALYZE` executes its statement; the `access: "read"` statement case is the only admitted diagnosis input; disable failure remains on the typed rail, and the enclosing scope disposes the leased session on every exit.
- Law: under `enable_profiling='json'` the result is ONE row of two `VARCHAR` cells — `explain_key` reading `analyzed_plan` and `explain_value` carrying the profile — so the harvest decodes that one string through `Schema.parseJson`; root latency and the plan root's own cardinality are required receipt evidence and each absence fails the wire, while operator measures and the counter roster stay `Option`-carried so a missing measure is omission rather than a forged zero.
- Law: BOTH embedded drivers profile — the harvest runs through `_DRIVERS.bounded` and `_ROWED` normalizes the two answers, so the browser lane fills the same `Pg.Profile` band the node lane does and a band tile reads one comparable receipt whichever driver produced it.
- Law: the worker cell reads by NAME off the Arrow schema, never by position — `enable_profiling` fixes the column names and nothing fixes their order, so a positional read forges whichever cell the engine happened to emit first.
- Law: returned rows read the plan root BENEATH the harvest's own `EXPLAIN_ANALYZE` operator — the profile root reports the OUTER statement, whose returned value is the plan text, so its `rows_returned` reads zero on every harvest and an operator walk starting above the wrapper credits the statement with the harvest's own machinery.
- Law: the counter roster is the one owner of root evidence — decode fields and receipt keys both project from `_COUNTERS`, so an engine's added measure lands as a row and no receipt grows a hand-spelled arm that silently disagrees with the schema beside it.
- Law: probes run beside production lanes — `_PROBE.runs` bounds the repetition, the run rides the same governed session gate as every statement so a probe cannot starve live work, the whole profile bracket (enable, EXPLAIN ANALYZE, disable, release) rides one `_GOVERNOR.budget` timeout so a stalled diagnosis releases its permit, and each harvest's wall span projects onto the census `profileDuration` histogram tagged `Convention.rasm.profileEngine` — the row codes its unit milliseconds, so the tap takes a `Duration` and the row's own scale carries it, and embedded engines expose no scrape surface, so this harvest is their whole observability.
- Law: escalation is evidence-driven row data — `_armed` compares evidence receipts by their p50 wall ratio against the `_PROBE.floor`, names the CANDIDATE row's own trigger text, and never mutates the row; the receipts carry the incumbent that produced them, so the verdict reads as an argument for a move rather than a claim about the engine measured, and admitting ClickHouse below its trigger remains the named operational waste the table refuses.
- Law: an unpriceable incumbent is the verdict's own absent arm — a ratio needs a denominator the probe measured, so a zero-span incumbent leaves `priced` empty and the review reads "too fast to price on this axis" as evidence; clamping the denominator to a floor prices every sub-millisecond incumbent as if it took that floor, which arms a move against a number the measurement never produced and is the same forgery as a zero-filled receipt field.
- Law: the cluster publishes no result-side profile at all — the driver answers rows for a read and its own insert result for an ingest — so cluster evidence is a SECOND read of `system.query_log` keyed by the statement's id, gated on the flush that makes the asynchronous log deterministic; a design expecting cost on the answer reads a surface the driver never carries.
- Law: a query id scopes exactly ONE statement — `withQueryId` binds a `FiberRef` for its whole effect, so a scope wrapping several statements files them under one log key and hands the driver's own interrupt arm a `KILL QUERY` that reaches every sibling sharing the id; `Olap.Ingest` carrying `queryId` per intent is what keeps the scope honest, and an id minted above a batch loses both the attribution and the isolation.
- Law: the harvest never re-runs the measured statement — the embedded arm profiles the one `EXPLAIN ANALYZE` execution and the cluster arm reads a log row for an execution that already happened, so a profile costs a plan decode or a log read, never a second run of the query whose cost is the subject.
- Law: counter vocabulary is shared across engines — `_COUNTERS` and `_LOGGED` key their own wire fields onto ONE receipt-key set, so a band tile reads `bytesRead` or `resultSetSize` whichever engine answered and no consumer branches on the producer to find a measure.

```typescript signature
import { Pg } from "./postgres.ts"
import { Array, Exit, Number, Option, Order, pipe, Record, Schema } from "effect"
import { SqlClient } from "@effect/sql"
import { ClickhouseClient } from "@effect/sql-clickhouse"

const _PROBE = { runs: 5, floor: 1.5 } as const // bounded repeatable measurement; the p50 ratio that arms a trigger

// Harvest statements wrap the profiled plan, so its tree carries an `EXPLAIN_ANALYZE` operator above it, and this row
// names the second of the two `VARCHAR` columns the result publishes.
const _WRAPPED = { cell: "explain_value", wrapper: "EXPLAIN_ANALYZE" } as const

// Root evidence keyed wire-field to receipt-key: ONE roster drives the decode fields and the receipt projection alike,
// so an engine's added measure is a row rather than a schema field beside a hand-spelled arm that drifts from it.
// Every entry is `Option`-carried because an absent counter is omission where a zero is a measurement.
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

// Roots hold no operator identity — each names its query, reports wall latency, and answers the counter roster.
// Its own `rows_returned` measures the outer statement, whose returned value is the plan text, so no field reads it.
const _Root = Schema.Struct({
  children: Schema.optionalWith(Schema.Array(_Tree), { as: "Option" }),
  latency: Schema.Number,
  query_name: Schema.optionalWith(Schema.String, { as: "Option" }),
  ...Record.map(_COUNTERS, () => Schema.optionalWith(Schema.Number, { as: "Option" })),
})

// One descent past the wrapper the harvest itself introduced: the profiled statement's plan root is that operator's
// own first child, and it is where returned rows and the operator walk both begin.
const _planRoot = (root: typeof _Root.Type): Option.Option<_ProfileTree> =>
  Option.flatMap(Option.flatMap(Option.flatMap(root.children, Array.head), (wrapper) => wrapper.children), Array.head)

// `operator_name` is the concrete implementation the engine chose and `operator_type` its plan category, so the name
// takes the sharper of the two; a node answering neither is structural and contributes no row.
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

// Every counter field carries one type, so the roster's key union indexes the decoded root without a cast and the
// filter is what keeps a measure the engine never reported out of the receipt.
const _countered = (root: typeof _Root.Type): Record.ReadonlyRecord<string, number> =>
  Record.fromEntries(
    Array.filterMap(Record.toEntries(_COUNTERS), ([wire, key]) => Option.map(root[wire], (value) => [key, value] as const)),
  )

type _ReadStatement = _Statement & { readonly access: "read" }

declare namespace Olap {
  // Intersection IS the roster: an embedded engine profiles exactly when `lane/postgres#PROFILE_HARVEST` admits its
  // key, so a landed harvest arm widens this set at its owner and no second roster drifts against it.
  type ProfileEngine = Extract<Embedded, Pg.ProfileEngine>
}

// Drivers answer different bounded grains, so this projection is the ONE place the harvest crosses them: node hands row
// objects already and the worker hands an Arrow `Table` whose schema names every column, so a cell reads by NAME on
// both sides and no arm indexes a position the engine never promised to hold.
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

// Required receipt evidence fails the wire on absence — one lift serves the profile cell, the plan root, and the
// returned-row count alike, so no arm quietly substitutes a zero for a measure the engine never reported.
const _required = <A>(engine: Olap.Engine, detail: string) => (held: Option.Option<A>): Effect.Effect<A, OlapFault> =>
  Option.match(held, { onNone: () => Effect.fail(new OlapFault({ engine, reason: "wire", detail })), onSome: Effect.succeed })

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
        // `explain_value` is the second column of the one answered row and the engine declares it `VARCHAR`, so this
        // coercion narrows the driver's own cell union rather than guessing a shape.
        const cell = yield* pipe(
          Option.flatMap(Array.head(raw), (row) => Option.fromNullable(row[_WRAPPED.cell])),
          Option.map(String),
          _required(session.engine, "<no-profile-cell>"),
        )
        const tree = yield* Schema.decodeUnknown(Schema.parseJson(_Root))(cell).pipe(
          Effect.mapError((fault) => new OlapFault({ engine: session.engine, reason: "wire", detail: String(fault) })),
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
    // Whole-bracket budget: PRAGMA enable, EXPLAIN ANALYZE, PRAGMA disable, and the gate release ride one
    // governor timeout together, so a stalled profile can never hold a session permit past the budget.
    Effect.timeoutFail({
      duration: _GOVERNOR.budget,
      onTimeout: () => new OlapFault({ engine: session.engine, reason: statement.fault, detail: _ABANDONED.profile }),
    }),
    Effect.tapError((fault) => _abandoned(fault) ? session.evict : Effect.void),
  )

// --- [CLUSTER_HARVEST]

// Clusters publish NO profile on their own answer — a read lands rows and an insert lands its own result — so
// evidence is a SECOND read of the server's log keyed by the id the statement already carried. Log columns key to the
// same receipt names the embedded roster publishes, so one band reads one counter vocabulary whichever engine answered.
const _LOGGED = {
  memory_usage: "memoryAllocated",
  read_bytes: "bytesRead",
  read_rows: "cumulativeRowsScanned",
  result_bytes: "resultSetSize",
  written_bytes: "bytesWritten",
} as const

// Decode owners ARE the projection list: each SELECT reads its own field keys, so a column added to the roster
// travels to the statement and the receipt together and neither can name a column the other does not.
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
    // Log rows land asynchronously, so the flush is what makes the read deterministic; it returns no rows of its own
    // and therefore runs in command mode, where a query-mode send would decode an empty result as a failure to answer.
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
          Effect.mapError((fault) => new OlapFault({ engine: "clickhouse", reason: "wire", detail: String(fault) })),
        )
      ),
    )
    yield* _meter.profiled.clickhouse(Duration.millis(logged.query_duration_ms))
    return new Pg.Profile({
      engine: "clickhouse",
      statement: row.label,
      wallMillis: logged.query_duration_ms,
      rows: Math.trunc(logged.result_rows),
      // Log rows answer cost, never a plan tree — `EXPLAIN` is a second execution whose cost is the measured thing —
      // so the operator roster is empty by construction and the band reads depth off the engine that publishes one.
      operators: [],
      counters: Record.fromEntries(Array.map(Record.toEntries(_LOGGED), ([wire, key]) => [key, logged[wire]] as const)),
      window: Option.none(),
    })
  })

// One entry over both harvest modalities, discriminating on the source value: an embedded source carries the handle
// whose session the diagnosis leases, and a cluster source carries the id its statement already ran under.
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
  // Evidence names the engine that PRODUCED it, so its key is the profiled set alone; the escalation below names the
  // candidate the evidence argues for, which is any engine row the table carries.
  type Evidence = {
    readonly engine: ProfileEngine
    readonly statement: string
    readonly runs: number
    readonly wallP50: number
    readonly wallMax: number
    readonly rows: number
  }
  // `Escalation` grades a RATIO, so a verdict exists only where the incumbent's own p50 carries a span to divide by:
  // `priced` answers `None` for an incumbent too fast to price on this axis, and the delta rides inside the Some
  // beside the verdict it produced, so an armed reading cannot exist without the measurement that armed it. A flat
  // boolean beside an optional delta admits that pair — armed against an absent measure — as a state the mint never builds.
  type Escalation = {
    readonly candidate: Engine
    readonly trigger: string
    readonly priced: Option.Option<{ readonly armed: boolean; readonly delta: number }>
  }
}

// `Array.range` mints a non-empty tuple and `Effect.forEach` carries that arity through, so head, crest, and midpoint
// read a roster whose inhabitance is a type fact: three fabricated faults for an impossible empty fold leave with it,
// and the engine rides the handle's own key rather than a literal the row table already owns.
const _probe = <E extends Olap.ProfileEngine>(
  handle: Olap.Handle<E>,
  statement: _ReadStatement,
  label: string,
): Effect.Effect<Olap.Evidence, OlapFault> =>
  Effect.map(
    Effect.forEach(Array.range(1, _PROBE.runs), () => _profile({ handle, label, statement }), { concurrency: 1 }),
    (receipts) =>
      pipe(Array.sort(Array.map(receipts, (receipt) => receipt.wallMillis), Order.number), (walls) => ({
        engine: handle.engine,
        statement: label,
        runs: receipts.length,
        // Midpoint indexing stays in range for every non-empty roster, so the crest fallback is unreachable by
        // construction and keeps the fold total without minting a fault for a state the arity already forecloses.
        wallP50: Option.getOrElse(Array.get(walls, Math.trunc(walls.length / 2)), () => Array.lastNonEmpty(walls)),
        wallMax: Array.lastNonEmpty(walls),
        rows: Array.headNonEmpty(receipts).rows,
      })),
  )

// `candidate` names the engine the evidence ARGUES FOR while the receipts carry the incumbent producing them, so a
// duckdb probe arms the cluster's own trigger text and the verdict never reads as a claim about the measured engine.
// `Number.divide` is the ratio's own partial owner and its `None` is the one arm the division cannot take, so an
// incumbent whose p50 carries no span leaves `priced` absent instead of dividing by a clamped floor — a clamp reads
// every sub-millisecond incumbent as slower than it measured and under-states the delta by the width of the clamp,
// which is the escalation argument reading a number no probe produced.
const _armed = (candidate: Olap.Engine, prior: Olap.Evidence, next: Olap.Evidence): Olap.Escalation => ({
  candidate,
  trigger: _engines[candidate].trigger,
  priced: Option.map(Number.divide(next.wallP50, prior.wallP50), (delta) => ({ armed: delta >= _PROBE.floor, delta })),
})
```

## [08]-[ARROW_WIRE]

- Owner: the one columnar interchange — the IPC codec pair, the Flight frame codec pair over one grain adapter, `Olap.lake`'s engine-free Parquet codec at rest, bounded batch streaming, the worker ingest entry, and the assembled `Olap` export.
- Packages: `apache-arrow` (`RecordBatch`, `RecordBatchReader`, `Schema`, `Table`, `tableFromIPC`, `tableToIPC`); `@qualithm/arrow-flight-client` (`createFlightDataFromIpc`, `decodeFlightDataStream`, `decodeFlightDataToTable`, `encodeRecordBatchesToFlightData`, `encodeTableToFlightData`, `getSchemaFromFlightData`); `parquet-wasm` (`readParquet`, `readSchema`, `writeParquet`, `ParquetFile.fromUrl`/`.fromFile`/`.stream`/`.free`, `Table.fromIPCStream`, `RecordBatch.intoIPCStream`, `Schema.intoIPCStream`, `WriterPropertiesBuilder`, `WriterProperties`, `Compression`, `EnabledStatistics`, `ReaderOptions`); `@duckdb/duckdb-wasm` (`conn.insertArrowTable`, `conn.insertArrowFromIPCStream`); `effect` (`Sink`); `object/store.ts` (`ObjectStore.Receipt`, `put`, `refer`, `ObjectStore.owner`) and `journal/retain.ts` (`Retain.Class`) close the landing.
- Entry: node reads land as row objects and worker reads as Arrow, so only ClickHouse output and foreign IPC cross `tableFromIPC`; every Flight read and upload crosses `_wire.flight`; a lake object decodes through `Olap.lake.read` or streams through `Olap.lake.batches`, and `Olap.lake.write`/`.sink` land each object through `object/store#CONDITIONAL` and answer its receipt; the viewer's geoarrow plane consumes the same Tables downstream.
- Growth: a new engine row joins the wire by emitting or accepting IPC — no per-engine result shape is ever admitted; a new frame source is one `_frames` arm; a writer economics posture is a `_PARQUET` override, never a per-call flag list.
- Law: one wire — an analytical result crossing any engine seam travels as Arrow; a JSON or row-object re-encoding between engines is the named defect, and the only row-shaped egress is the final consumer projection.
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

```typescript signature
import { Sink } from "effect"
import { zstdCompressSync, zstdDecompressSync } from "node:zlib"
import {
  compressionRegistry, CompressionType, isArrowTable, RecordBatch, RecordBatchReader, type Schema as ArrowSchema, Table,
  tableFromIPC, tableToIPC,
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
  // Three source shapes one wire admits: a materialized Table, a batch stream carrying its own schema because
  // encoders write the schema message first, and raw IPC bytes an Arrow writer already framed.
  type Frame =
    | Table
    | Uint8Array
    | { readonly batches: AsyncIterable<RecordBatch> | Iterable<RecordBatch>; readonly schema: ArrowSchema }
  // Both grains one Flight codec admits: the package's own iterator and this lane's stream.
  type Frames = AsyncIterable<FlightData> | Stream.Stream<FlightData, OlapFault>
  // Three lake residencies: bytes already in hand, a remote object read by range request, a picked local file.
  type Stored = Uint8Array | { readonly url: string } | { readonly blob: Blob }
  // Read cost is fixed at the write, so the row carries the axes a later scan pays for and the patience a partial
  // window waits before it lands — five decisions one policy value states, never a builder chain at a call site.
  type Writing = {
    readonly compression: keyof typeof parquet.Compression
    readonly dictionary: boolean
    readonly patience: Duration.Duration
    readonly rowGroup: number
    readonly statistics: keyof typeof parquet.EnabledStatistics
  }
  // Custody the landing cannot derive: the catalog the `lake:` owner names, and the retention class whose window the
  // sweep honours. Both arrive because the object plane re-derives an object's tag from its live reference set alone,
  // so a landing guessing either files the cold tail under a life no policy chose.
  type Landing = { readonly catalog: string; readonly retention: Retain.Class }
  type Lake = {
    readonly batches: (source: Stored, read?: parquet.ReaderOptions) => Stream.Stream<RecordBatch, OlapFault>
    readonly read: (bytes: Uint8Array, read?: parquet.ReaderOptions) => Effect.Effect<Table, OlapFault>
    readonly schema: (bytes: Uint8Array) => Effect.Effect<ArrowSchema, OlapFault>
    readonly sink: (
      batches: Stream.Stream<RecordBatch, OlapFault>,
      landing: Landing,
      policy?: Writing,
    ) => Stream.Stream<ObjectStore.Receipt, OlapFault, ObjectStore>
    readonly write: (table: Table, landing: Landing, policy?: Writing) => Effect.Effect<ObjectStore.Receipt, OlapFault, ObjectStore>
  }
}

// ZSTD over page statistics at a 128k row group is the cold-tail default: the compression a scan decompresses once
// against the statistics that let it skip whole groups, sized so a predicate prunes before it reads.
const _PARQUET = {
  compression: "ZSTD",
  dictionary: true,
  patience: Duration.seconds(5),
  rowGroup: 128_000,
  statistics: "Page",
} as const satisfies Olap.Writing

// Reads take the writer row's counterpart: `batchSize` fixes the grain a scan emits at and `concurrency` bounds the
// range requests a row-group fan holds in flight, so a cold-tail read states both instead of inheriting the codec's
// silent defaults. The fan matches the Flight endpoint fan, because both spend the same remote-request budget.
const _READING = { batchSize: 65_536, concurrency: _FLIGHT.endpoints } as const satisfies parquet.ReaderOptions

const _frames = Match.type<Olap.Frame>().pipe(
  Match.when(Match.instanceOf(Table), (table) => Stream.fromAsyncIterable(encodeTableToFlightData(table), _flightFault)),
  Match.when(
    Match.instanceOf(Uint8Array),
    (bytes) => Stream.fromEffect(Effect.try({ try: () => createFlightDataFromIpc(bytes), catch: _flightFault })),
  ),
  Match.orElse(({ batches, schema }) => Stream.fromAsyncIterable(encodeRecordBatchesToFlightData(batches, schema), _flightFault)),
)

// `Stream` carries no async-iterator key and every package-minted frame source does, so the value answers which grain
// it is and the codec below never publishes a second entry per grain.
const _framed = (source: Olap.Frames): AsyncIterable<FlightData> =>
  Symbol.asyncIterator in source ? source : Stream.toAsyncIterable(source)

// arrow-js ships the compression PROTOCOL and no codec at all — the registry is empty at import, so naming a
// compression type without a registered codec fails at encode and a compressed foreign frame is undecodable at read.
// Registration and naming land as ONE admission: the codec registers once at module scope from `node:zlib`, which
// admits no package, and the wire then names its type. Registration is what makes DECODE of a peer's compressed
// frame possible at all, so the read half arrives with the write half rather than a release later.
compressionRegistry.set(CompressionType.ZSTD, { decode: zstdDecompressSync, encode: zstdCompressSync })

const _wire = {
  decode: (engine: Olap.Engine, bytes: Uint8Array): Effect.Effect<Table, OlapFault> =>
    Effect.try({ try: () => tableFromIPC(bytes), catch: _fault(engine, "wire") }),
  // Every seam consumes the stream form, and the third argument carries the whole compression decision, so a frame
  // crossing to a Flight peer, a ClickHouse ingest, or a lake object leaves compressed by construction.
  encode: (engine: Olap.Engine, table: Table): Effect.Effect<Uint8Array, OlapFault> =>
    Effect.try({ try: () => tableToIPC(table, "stream", CompressionType.ZSTD), catch: _fault(engine, "wire") }),
  batches: (engine: Olap.Engine, source: AsyncIterable<Uint8Array>) =>
    Stream.unwrap(
      Effect.map(
        _try(engine, "wire", () => RecordBatchReader.from(source)),
        (reader) => Stream.fromAsyncIterable(reader, _fault(engine, "wire")),
      )),
  // Flight half of the one wire: `framed` stamps the descriptor onto the first message and leaves every later one
  // untouched, `streamed` and `landed` are the grain pair a consumer picks between, and `header` answers the schema
  // without taking the batches — same IPC bytes in every direction, never a second encoding.
  flight: {
    framed: (source: Olap.Frame, descriptor: FlightDescriptor): AsyncIterable<FlightData> =>
      Stream.toAsyncIterable(
        Stream.mapAccum(_frames(source), true, (first, data) => [false, first ? { ...data, flightDescriptor: descriptor } : data]),
      ),
    // `getSchemaFromFlightData` CONSUMES its stream, so a caller wanting both takes the batches and reads the schema
    // off the first one; this member exists for the schema-only round trip the `Frames` intent serves.
    header: (source: Olap.Frames): Effect.Effect<Option.Option<ArrowSchema>, OlapFault> =>
      Effect.map(_flew(() => getSchemaFromFlightData(_framed(source))), Option.fromNullable),
    landed: (source: Olap.Frames): Effect.Effect<Table, OlapFault> => _flew(() => decodeFlightDataToTable(_framed(source))),
    streamed: (source: Olap.Frames): Stream.Stream<RecordBatch, OlapFault> =>
      Stream.fromAsyncIterable(decodeFlightDataStream(_framed(source)), _flightFault),
  },
  // Locally staged frames enter the worker through its own leased session, so the ingest holds a permit and answers the
  // one budget every worker statement answers.
  // `isArrowTable` guards POSITIVELY, because this fence holds two classes named `Table` sharing no identity and the
  // workspace resolves three Arrow copies: it reads a `Symbol.for` marker, so it answers true across duplicate
  // package instances where `instanceof` answers false, and a `parquet.Table` is refused at the branch rather than
  // falling through into a driver call that fails inside the worker. A negative test also silently reclassifies
  // every future member of a widened source union as an Arrow table.
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

// Every codec member this lane composes CONSUMES its handle — `intoIPCStream`, `writeParquet`, and `build` each take
// its pointer and destroy it — so a container lives inside one expression and a release bracket around one would free
// a pointer the call already took. `ParquetFile` alone outlives its mint, so it alone brackets.
// Codec work boots no engine, but its faults still name a LANE: the cold-tail sink runs in a node service and the
// range reader in the browser shell, so a hardcoded key files every service-side write fault under the browser engine
// and a fault board reads codec pressure against a worker that was never instantiated. Threading the composing lane's
// own key is what the engine-threading law already demands of every session, meter, and fault on this page.
const _lake = (engine: Olap.Embedded, ready?: () => Promise<unknown>): Effect.Effect<Olap.Lake, OlapFault> =>
  Effect.gen(function* () {
    // Async builds resolve their module once here and the node build resolves nothing, so no read or write below
    // can reach an uninstantiated codec and no call site branches on which build the exports map selected.
    yield* ready === undefined ? Effect.void : _try(engine, "bundle", ready)
    const propertied = (policy: Olap.Writing): parquet.WriterProperties =>
      new parquet.WriterPropertiesBuilder()
        .setCompression(parquet.Compression[policy.compression])
        .setDictionaryEnabled(policy.dictionary)
        .setMaxRowGroupSize(policy.rowGroup)
        .setStatisticsEnabled(parquet.EnabledStatistics[policy.statistics])
        .build()
    // One IPC hop per direction is the whole crossing: the two `Table` classes share no identity, so the stream buffer
    // is the seam and a shared-instance shortcut does not exist to reach for.
    const written = (table: Table, policy: Olap.Writing = _PARQUET): Effect.Effect<Uint8Array, OlapFault> =>
      Effect.try({
        try: () => parquet.writeParquet(parquet.Table.fromIPCStream(tableToIPC(table, "stream")), propertied(policy)),
        catch: _fault(engine, "wire"),
      })
    // Producer law at `object/store#REFERENCE_GC` closes here rather than at a caller who can land the object and
    // forget the row: the conditional put and the reference verb are ONE unit of work, the owner crosses through the
    // store's own mint so the `lake:` prefix has a single speller, and the retention rides the landing the caller
    // declared. `put` is the leg that mints identity from the bytes, and its conditional makes a replayed window a
    // no-op against the key it already wrote — which is what keeps the `object` reason on the retryable side.
    const landed = (landing: Olap.Landing) => (bytes: Uint8Array): Effect.Effect<ObjectStore.Receipt, OlapFault, ObjectStore> =>
      Effect.flatMap(ObjectStore, (store) =>
        Effect.mapError(
          Effect.tap(store.put(bytes), (receipt) => store.refer(receipt.key, ObjectStore.owner("lake", landing.catalog), landing.retention)),
          _fault(engine, "object"),
        ))
    // Bytes in hand still open as a file, because the range reader is the only member that yields row groups lazily.
    const opened = (source: Olap.Stored): Effect.Effect<parquet.ParquetFile, OlapFault, Scope.Scope> =>
      Effect.acquireRelease(
        _try(engine, "wire", () =>
          "url" in source
            ? parquet.ParquetFile.fromUrl(source.url)
            : parquet.ParquetFile.fromFile("blob" in source ? source.blob : new Blob([source]))),
        (file) => Effect.sync(() => file.free()),
      )
    return {
      // Whole-buffer decode admits only where the object is provably bounded; everything else takes `batches`.
      read: (bytes, read) =>
        Effect.try({ try: () => tableFromIPC(parquet.readParquet(bytes, read).intoIPCStream()), catch: _fault(engine, "wire") }),
      schema: (bytes) =>
        Effect.try({ try: () => tableFromIPC(parquet.readSchema(bytes).intoIPCStream()).schema, catch: _fault(engine, "wire") }),
      // BOUNDARY ADAPTER: the package types its readers element-free, so the element the stream carries is stated here
      // rather than inferred; range requests pull one row group at a time, so an object past the bounded floor never
      // lands whole and the file handle releases with the stream's own scope.
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
      // Streamed egress lands ONE object per row-group window, weighted by the rows a batch actually carries and
      // flushed on the policy's own patience, so a cold tail larger than memory writes as a content-addressed object
      // set and no fold ever holds more than a window — each one referenced as it lands, never in a trailing pass a
      // failed stream never reaches.
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Olap, OlapFault }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
