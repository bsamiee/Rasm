# [RUNTIME_METER]

`Pulse` is the work-plane meter bridge — one lossy projection from durable-work evidence onto Convention-keyed Effect instruments, so queue depth, drain lag, and relay throughput read as OTel series while every dispute settles against the journal. `mark` folds a settlement fact into its counter row at the emitting call site, and `live` runs the sampled census sweep setting every gauge row from one `Probe` port the app root satisfies with the data journal's census statement — fact rows stay the billing truth, instruments stay bounded, and neither plane re-derives the other.


## [01]-[INDEX]

- [02]-[PROJECTION] — the mounted instrument tables, the one polymorphic mount, and the `mark` fold; `Pulse`.
- [03]-[CENSUS] — the `Probe` port, the sampled gauge sweep, and the hook-rail delta feeder; `Pulse`.
- [04]-[VERBOSITY] — the tier-table to `Logger.minimumLogLevel` wiring; `Pulse`.
- [05]-[VIEWS] — the metric-stream governance table contributed as one `Hooks` node; `Pulse`.
- [06]-[BOARD] — the typed deploy-feed pack folding instrument rows and vital budgets; `Pulse`.

## [02]-[PROJECTION]

- Owner: the interior `_WORK`, `_TAP`, and `_GAUGES` row tables and `_row`, the one row builder — every row carries its `Convention.named` metadata, the instrument `Convention.mount` materializes from that same row, and its own tag roster, so the settlement kinds (`drained`: relay claims settled; `parked`: deliverables diverted to the dead set) and the census levels are declared once and read by the mark fold, the sweep, and the board projection alike — the governance allow-list reads the vocabulary owner instead, because it governs every plane minting under `rasm.*`, not this module's rows. `Pulse.mark(kind, channel, count?)` is the projection fold: it increments the row's counter tagged `Convention.rasm.workChannel`, so the emitting owner adds one composed line beside its `Fact.record` call and the instrument mints nowhere else.
- Law: the projection is lossy by design — the journal fact is the truth a billing or forensic read settles against, the counter is the dashboard series, and the two emit from ONE call site so they cannot disagree on what happened, only on retention; a missing metric point is a dashboard gap, never an evidence defect.
- Law: materialization is the vocabulary owner's — `Convention.mount` reads wire form, description, bucket ladder, value width, and UCUM code off the named row, so this module composes handles and declares no constructor, no boundary vector, and no unit tag; a kind-dispatch table here is the second materialization owner the core ruling deletes.
- Law: `_WORK`, `_TAP`, and `_GAUGES` name counting and level rows alone — a word census updates on a WORD rather than a number, so no fold here consumes one and those rows mount at the capability folder producing the words.
- Law: the carrier tag rides the mount at one value per instrument, so it adds no cardinality.
- Law: three tables stand because three consumers do — `_WORK` answers the `mark` fold at the emitting call site, `_TAP` answers the rail delta feeder, and `_GAUGES` answers the level sweep, so each table's row type fixes the mounted instrument exactly and no consumer casts; the board fold reads all three through one concatenation, so a new instrument is one row in its owning table and appears on every downstream projection by construction.
- Law: `channel` values are the work plane's own closed channel vocabulary (the deliver channel rows, the queue lane names) and the PUBLISHER owns that boundedness, because `otel` and `work` seat at one stratum with the edge running work-to-otel, so importing those rosters back inverts it. Structural guarding stands regardless: the tag key is a `Convention.rasm` row, so `[05]`'s allow-list admits it under `views.tenant.limit` and a runaway channel value folds into the overflow bucket at `emit#GOVERNANCE` instead of fanning the series. Identifier-grade values ride span attributes, never this tag.
- Entry: `Pulse.mark("drained", channel, settled)` beside the relay's drain fact; `Pulse.mark("parked", channel)` beside the park fact.
- Growth: a new settlement kind is one `_WORK` row and a new census level one `_GAUGES` row, each naming its Convention metric.
- Boundary: the facts themselves are the work plane's (`work/deliver`, `work/queue`) and the journal is the data plane's; this page owns only the projection.
- Packages: `effect` (`Metric`, `Array`, `Option`, `Record`); `@rasm/core` (`Convention`, `Tap`).

```typescript
import { AggregationType, createAllowListAttributesProcessor, createDenyListAttributesProcessor } from "@opentelemetry/sdk-metrics"
import { type Identity, Convention, Tap } from "@rasm/core"
import {
  Array, Context, Duration, Effect, Layer, Logger, LogLevel, Metric, Option, Record, Ref, Schedule, Schema,
} from "effect"
import { Setting } from "../proc/config.ts"
import { Hooks } from "./emit.ts"
import { Vital } from "./vital.ts"

type _Row<N extends Convention.MetricName> = {
  readonly instrument: Convention.Named[N]
  readonly metric: Convention.Mounted<N>
  readonly tags: ReadonlyArray<string>
}

const _row = <N extends Convention.MetricName, const W extends Convention.Roster>(
  metric: N,
  tags: ReadonlyArray<string>,
  ...words: Convention.Words<N, W>
): _Row<N> => ({ instrument: Convention.Metric.at(metric), metric: Convention.mount(metric, ...words), tags })

const _level = <N extends Convention.MetricName, const W extends Convention.Roster>(
  metric: N,
  read: (census: Pulse.Census) => number,
  ...words: Convention.Words<N, W>
) => ({ ..._row(metric, [], ...words), read })

const _WORK = {
  drained: _row(Convention.metric.relayDrained, [Convention.rasm.workChannel]),
  parked: _row(Convention.metric.queueParked, [Convention.rasm.workChannel]),
} as const

const _TAP = {
  admitted: _row(Convention.metric.tapAdmitted, [Convention.rasm.tapPoint]),
  dropped: _row(Convention.metric.tapDropped, [Convention.rasm.tapLoss, Convention.rasm.tapPoint]),
  seats: _row(Convention.metric.tapSeats, [Convention.rasm.tapSeating]),
  vetoed: _row(Convention.metric.tapVetoed, [Convention.rasm.tapPoint]),
} as const

declare namespace Pulse {
  type Work = keyof typeof _WORK
  type Census = {
    readonly outbox: { readonly age: Duration.Duration; readonly depth: number; readonly redelivered: number }
    readonly queue: { readonly depth: number }
  }
  type Policy = {
    readonly cadence: Duration.Duration
    readonly views: {
      readonly engine: { readonly deny: ReadonlyArray<string>; readonly limit: number }
      readonly latency: { readonly boundaries: ReadonlyArray<number>; readonly instrument: string }
      readonly tenant: { readonly keys: ReadonlyArray<string>; readonly limit: number }
    }
  }
  type Board = _Board
}

const _marked = (kind: Pulse.Work, channel: string, count = 1): Effect.Effect<void> =>
  Metric.incrementBy(Metric.tagged(_WORK[kind].metric, Convention.rasm.workChannel, channel), count)
```

## [03]-[CENSUS]

- Owner: the `Probe` port, the `_GAUGES` level table, the `_FEED` delta correspondence over the hook rail, and the sweep — `Probe` is one `Context.Tag` whose `census` member answers the current outbox and queue truth, each `_GAUGES` row carries its own `read` projection off that census, `_fed` diffs one `Tap.census` reading against the sample it held, and `Pulse.live(policy)` is a `Layer.scopedDiscard` forking one `Schedule.spaced(policy.cadence)` repeat that folds every table per sample; the fork dies with the graph scope, so a leaked sweep fiber is structurally impossible.
- Law: the row owns its projection, so the sweep is total by construction — a new census dimension is one row carrying its Convention gauge and its reader, and the fold reaches it with zero sweep edits; a sweep enumerating gauges by hand strands every dimension added after it.
- Law: the port keeps the strata clean — the data journal's `Journal.census` statement satisfies `Probe` at the app root, so the outbox truth crosses the seam as a value and this module imports no SQL surface; the queue depth arrives from the durable-queue owner's own read through the same binding, and that binding composes `Tenancy.sweep` around the census statement because the census answers per app across every tenant on a FORCE-RLS relation — an unpinned sample reads an empty outbox as healthy and a tenant-pinned one reports one slice as the plane's whole depth.
- Law: the probe is total by contract — the satisfying binding internalizes its store faults (the prior sample or a zero census stands in), because a broken gauge sweep must degrade a dashboard, never fail a process. Contracts type against a FAILURE and a defect escapes them, so the sweep folds defects itself: an unchecked store fault costs one interval and reads on the log rail, where an unfolded one kills the repeat fiber and freezes every gauge at its last value for the process lifetime — a dashboard reading stale levels as live ones.
- Law: `MetricPolling` is the refused substitute for this hand fold, on two independent counts the substrate itself fixes. `MetricPolling.collectAll` composes its members' polls as a sequential traversal, so each `_GAUGES` row would run the census effect once — the journal statement executes per ROW per interval and its cost grows with the table, where one census answers every level here and the table is the declared growth site. And `MetricPolling.retry` gates the poll's ERROR channel, which the `Probe` contract types as `never` by the totality law above, so it can never fire and cannot stand in for the defect fold this sweep owns — a defect under `MetricPolling.launch` kills the forked fiber and produces exactly the frozen-gauge failure the guard exists to foreclose. The substrate buys nothing else: `launch` forks scope-bound, which `Effect.forkScoped` already is.
- Law: gauges are sampled, never accumulated — depth, age, and redelivery are census facts of one instant, so the sweep sets absolute levels and rate questions (DLQ rate, redelivery rate) derive in the query plane from the counter and gauge series.
- Law: the hook rail's own tallies reach the same sweep as DELTAS, never as levels — `Tap.census` answers cumulative totals over one `Tap.Report`, so the feeder holds the prior sample and increments each counter by the difference; setting a counter to a running total re-counts every prior interval, and one gauge per tally would answer "how many since boot" where every consumer asks "how many since last".
- Law: one cadence and one fiber carry both reads — the level sweep and the rail delta are two projections of the same instant, so a second repeat would sample them apart and pay scheduling for the privilege; a defect in either costs one interval under the same fold.
- Law: the tag values ARE the tally column names — the loss halves the dropped instrument splits on and the seating columns are read off the published `Tap.Census`/`Tap.Seating` shapes, so a renamed or added column breaks at the compiler rather than reporting silently as zero forever.
- Boundary: the breach ring's own `ledger` census counts displaced BREACHES, not facts at a point, so no `tapPoint`-dimensioned row admits it; breach series ride `Convention.metric.tapBreaches` off the `Tap.breaches` stream at its own emitter.
- Entry: `Pulse.live(policy)` merged at the composition root beside `Export.live`, after the root binds `Probe` and seats `Hooks.Dispatch`.
- Growth: a new census dimension is one `Census` field and one `_GAUGES` row reading it; a new rail tally is one `_FEED` entry over its `_TAP` row.
- Packages: `effect` (`Context`, `Duration`, `Layer`, `Metric`, `Option`, `Record`, `Ref`, `Schedule`); `@rasm/core` (`Tap.census`); `./emit.ts` (`Hooks.Dispatch`).

```typescript
class Probe extends Context.Tag("runtime/Pulse/Probe")<Probe, {
  readonly census: Effect.Effect<Pulse.Census>
}>() {}

const _GAUGES = {
  outboxAge: _level(Convention.metric.outboxAge, (census) => Convention.duration(Convention.metric.outboxAge, census.outbox.age)),
  outboxDepth: _level(Convention.metric.outboxDepth, (census) => census.outbox.depth),
  outboxRedelivered: _level(Convention.metric.outboxRedelivered, (census) => census.outbox.redelivered),
  queueDepth: _level(Convention.metric.queueDepth, (census) => census.queue.depth),
} as const

type _Sample = { readonly points: { readonly [point: string]: Tap.Census }; readonly seating: Tap.Seating }
type _Loss = Extract<keyof Tap.Census, "lost" | "shed">

const _NO_FACTS: Tap.Census = { admitted: 0, lost: 0, shed: 0, vetoed: 0 }
const _NO_SAMPLE: _Sample = { points: {}, seating: { mounted: 0, refused: 0, released: 0 } }

const _FEED = {
  admitted: { at: _TAP.admitted, half: Option.none<_Loss>() },
  lost: { at: _TAP.dropped, half: Option.some<_Loss>("lost") },
  shed: { at: _TAP.dropped, half: Option.some<_Loss>("shed") },
  vetoed: { at: _TAP.vetoed, half: Option.none<_Loss>() },
} as const satisfies { readonly [C in keyof Tap.Census]: { readonly half: Option.Option<_Loss> } }

const _stamped = <N extends Convention.MetricName>(row: _Row<N>, point: string, half: Option.Option<_Loss>) =>
  Option.match(half, {
    onNone: () => Metric.tagged(row.metric, Convention.rasm.tapPoint, point),
    onSome: (value) =>
      Metric.tagged(Metric.tagged(row.metric, Convention.rasm.tapPoint, point), Convention.rasm.tapLoss, value),
  })

const _fed = (held: Ref.Ref<_Sample>): Effect.Effect<void, never, Hooks.Dispatch> =>
  Effect.gen(function* () {
    const report = yield* Effect.flatMap(Hooks.Dispatch, Tap.census)
    const prior = yield* Ref.getAndSet(held, { points: Record.fromEntries(report.points), seating: report.seating })
    yield* Effect.forEach(report.points, ([point, taken]) => {
      const was = Option.getOrElse(Record.get(prior.points, point), () => _NO_FACTS)
      return Effect.forEach(
        Record.toEntries(_FEED),
        ([column, feed]) => Metric.incrementBy(_stamped(feed.at, point, feed.half), taken[column] - was[column]),
        { discard: true },
      )
    }, { discard: true })
    yield* Effect.forEach(
      Record.toEntries(prior.seating),
      ([column, was]) =>
        Metric.incrementBy(
          Metric.tagged(_TAP.seats.metric, Convention.rasm.tapSeating, column),
          report.seating[column] - was,
        ),
      { discard: true },
    )
  })

const _swept: Effect.Effect<void, never, Probe> = Effect.catchAllDefect(
  Effect.flatMap(Probe, (probe) =>
    Effect.flatMap(probe.census, (census) =>
      Effect.forEach(Record.values(_GAUGES), (row) => Metric.set(row.metric, row.read(census)), { discard: true }))),
  (defect) => Effect.annotateLogs(Effect.logWarning("<census-sweep>"), { detail: String(defect) }),
)
```

## [04]-[VERBOSITY]

- Owner: `Pulse.verbosity` — one Layer wiring the config tier table into the process log floor: it reads `Setting.serve.tier`, projects the tier's `verbose` column through the `Setting.tiers` anchor, and installs `Logger.minimumLogLevel` — `Debug` where the tier is verbose, `Info` otherwise — so the declared column governs every `Effect.log*` call in the process and no page carries a level literal.
- Law: the floor is one root decision — the Layer merges once at the composition root beneath the export lane, so the OTLP log leg and any file logger both inherit it; a per-module level override is a `Logger.withMinimumLogLevel` region on the owning rail, never a second root install.
- Law: this floor governs the Effect log rail alone — the SDK's own diagnostic stream rides `Export.Policy.diagnostic` into `diag.setLogger` at `emit#LANES`, and the `LoggerProvider` scope configurator stays unmounted, so exactly one owner governs each stream and a record dropped at one floor is never re-admitted at another.
- Entry: `Pulse.verbosity` at the composition root.
- Packages: `effect` (`Logger`, `LogLevel`, `Layer`); `../proc/config.ts` (`Setting`).

```typescript
const _verbosity: Layer.Layer<never, never, Setting> = Layer.unwrapEffect(
  Effect.map(Setting, (setting) =>
    Logger.minimumLogLevel(Setting.tiers[setting.serve.tier].verbose ? LogLevel.Debug : LogLevel.Info)),
)
```

## [05]-[VIEWS]

- Owner: the interior `_VIEWS` table and `Pulse.views(policy)` — one row per governed instrument space, each carrying its selection, its attribute processors, its cardinality ceiling, and any aggregation re-arm together, contributed through one `Hooks.contribute` node the export lanes drain. `tenant` admits the branch's whole declared `rasm.*` attribute vocabulary over the `rasm.*` instrument space under the policy ceiling; `engine` drops the `v8js.heap.space.name`/`v8js.gc.type` fan over `v8js.*` under its own ceiling; `latency` re-arms a named distribution onto explicit buckets.
- Law: one table, one contribution — a governed space is a row, never a Layer, so adding a guard cannot add a public member and the app root composes exactly one views node across every governed space.
- Law: the tenant allow-list derives its roster from `Convention.dimensions` whole, never from this module's mounted rows — the row selects the entire `rasm.*` instrument space, and the vital, crash, security, lake, and capability planes all mint inside it, so a roster folded from `_WORK`/`_GAUGES` alone silently strips `Convention.rasm.vitalGrade` from every vital series and breaks the `[06]` burn pair that slices on exactly that key. Reading the vocabulary's own metric census makes every declared dimension survive by construction, including the bounded semconv keys a Rasm instrument slices on, and a new Convention row survives with zero edits here; `policy.views.tenant.keys` widens the roster for foreign keys the vocabulary does not own, and the ceiling below is what bounds cardinality rather than the roster's width.
- Law: the OTLP bridge computes the exported descriptor's unit from the carrier tag before any view runs.
- Law: `Convention.wire.unit` is no `Convention.rasm` row, so the allow-list drops the carrier by construction and the descriptor keeps its unit.
- Law: a deny row for that key governs nothing the allow-list has not already decided.
- Law: the native OTLP lane registers no producer and no view engine, so the carrier rides it to the gateway as a data-point attribute and `iac/operate/observe#CHART_ROWS`'s metric-leg strip closes it there.
- Law: the governor stacks in three tiers — the row's attribute processor is the primary bound, its `aggregationCardinalityLimit` the circuit breaker above it, and `Export.Policy.cardinality` the reader ceiling above every view; the tiers are declared at two owners and compose without restatement.
- Law: the explicit-bucket row re-arms the RAW-provider plane alone, the one plane where a view re-aggregates at all — `emit#LANES`' `_aggregation` puts every raw histogram on base-2 exponential, and a foreign distribution whose store or SLO ladder demands fixed boundaries names itself on `policy.views.latency` as an instrument-name glob, since a third-party name carries no Convention row to type against. Every `rasm.*` distribution is Effect-minted and reaches the exporter as a collected point no view can re-bucket, so ITS boundaries are the `bounds` ladder on its own Convention row: one Tier-0 fallback, two seats, each on the plane that can honour it.
- Law: each row's execution seam is the metric plane it selects — `rasm.*` rows govern Effect-minted series through the producer projection at `emit#GOVERNANCE`, `v8js.*` rows govern the raw provider through the SDK's own view engine, and the aggregation re-arm only reaches instruments the raw provider owns because a collected point cannot be re-bucketed; the row shape is one vocabulary and the seam follows the selection.
- Entry: `Pulse.views(policy)` merged among the `Hooks.contribute` nodes, before `Export.live` drains.
- Growth: a new governed space is one `_VIEWS` row with its policy group.
- Packages: `@opentelemetry/sdk-metrics` (`createAllowListAttributesProcessor`, `createDenyListAttributesProcessor`, `AggregationType`); `effect` (`Record`); `@rasm/core` (`Convention`); `./emit.ts` (`Hooks`).

```typescript
const _VIEWS = {
  engine: (policy: Pulse.Policy) => ({
    aggregationCardinalityLimit: policy.views.engine.limit,
    attributesProcessors: [createDenyListAttributesProcessor([...policy.views.engine.deny])],
    instrumentName: "v8js.*",
  }),
  latency: (policy: Pulse.Policy) => ({
    aggregation: {
      type: AggregationType.EXPLICIT_BUCKET_HISTOGRAM,
      options: { boundaries: [...policy.views.latency.boundaries], recordMinMax: true },
    },
    instrumentName: policy.views.latency.instrument,
  }),
  tenant: (policy: Pulse.Policy) => ({
    aggregationCardinalityLimit: policy.views.tenant.limit,
    attributesProcessors: [createAllowListAttributesProcessor([...Convention.dimensions, ...policy.views.tenant.keys])],
    instrumentName: "rasm.*",
  }),
} as const

const _views = (policy: Pulse.Policy): Layer.Layer<never, never, Hooks> =>
  Hooks.contribute((hooks) =>
    Effect.forEach(Record.values(_VIEWS), (row) => hooks.add("views", row(policy)), { discard: true }))
```

## [06]-[BOARD]

- Owner: `Pulse.Board` and `Pulse.board(identity)` — the census projection folding the `_WORK`/`_TAP`/`_GAUGES` instrument rows and the `Vital.rows` budget table into one Schema-classed deploy-feed value: `panels` carry name, description, UCUM unit, instrument kind, and tag keys off the row's own Convention metadata; `budgets` carry each vital kind's good/poor thresholds, unit, and the level series `Vital.level` selects for it; `burn` carries the SLO burn-rate input pairs — a bad and total series with an optional tag slice — so the boards derive from the same rows the emitters write, and a new instrument or vital appears on the board by construction because the fold reads the tables, never a hand roster.
- Law: every panel column reads the row, never the fold — kind, unit, description, and tag keys all live on `Convention.instrument` and the row's tag column, so one concatenated map over the instrument tables emits every panel and a fold re-stating a kind is the duplication this projection deletes.
- Law: burn inputs are series names, never queries — the vital pair (total `Convention.metric.vitalObserved`, bad the same series sliced `vitalGrade=poor`) and the work pair (total `relayDrained`, bad `queueParked`) are data rows; the burn-rate algebra, objectives, and window ladders stay the core slo plane's, compiled by the iac observe fold.
- Law: `Pulse.wire` is the provenance key this producer mints — the deploy tuple admits a pack only under a key its producing branch spells, so the constant lives beside the projection earning it and the app's deploy feed stamps it rather than re-typing a literal; a key spelled at the consuming tier alone forks the moment either end edits it.
- Growth: a new burn family is one `_BURN` row; a new panel axis is one field on the panel struct every producer inherits.
- Packages: `effect` (`Schema`, `Array`, `Record`, `Option`); `./vital.ts` (`Vital.rows`); `@rasm/core` (`Convention`, `Identity.App`).

```typescript
class _Board extends Schema.Class<_Board>("Pulse/Board")({
  app: Schema.NonEmptyString,
  panels: Schema.Array(Schema.Struct({
    description: Schema.String,
    instrument: Convention.Kind.schema,
    name: Schema.NonEmptyString,
    tags: Schema.Array(Schema.String),
    unit: Convention.Unit.schema,
  })),
  budgets: Schema.Array(Schema.Struct({
    good: Schema.Number,
    kind: Schema.NonEmptyString,
    metric: Schema.Literal(...Vital.levels),
    poor: Schema.Number,
    unit: Convention.Unit.schema,
  })),
  burn: Schema.Array(Schema.Struct({
    bad: Schema.NonEmptyString,
    slice: Schema.optionalWith(Schema.Struct({ tag: Schema.NonEmptyString, value: Schema.NonEmptyString }), { as: "Option" }),
    total: Schema.NonEmptyString,
  })),
}) {}

const _BURN = [
  {
    bad: Convention.metric.vitalObserved,
    slice: Option.some({ tag: Convention.rasm.vitalGrade, value: "poor" }),
    total: Convention.metric.vitalObserved,
  },
  { bad: _WORK.parked.instrument.name, slice: Option.none(), total: _WORK.drained.instrument.name },
] as const

const _board = (identity: Identity.App): _Board =>
  new _Board({
    app: identity.app,
    panels: Array.map(
      [...Record.values(_WORK), ...Record.values(_TAP), ...Record.values(_GAUGES)],
      ({ instrument, tags }) => ({
        description: instrument.description,
        instrument: instrument.kind,
        name: instrument.name,
        tags: [...tags],
        unit: instrument.unit,
      }),
    ),
    budgets: Array.map(
      Record.toEntries(Vital.rows),
      ([kind, row]) => ({ good: row.good, kind, metric: Vital.level(kind), poor: row.poor, unit: row.unit }),
    ),
    burn: [..._BURN],
  })

const Pulse: {
  readonly Board: typeof _Board
  readonly Probe: typeof Probe
  readonly board: (identity: Identity.App) => _Board
  readonly live: (policy: Pulse.Policy) => Layer.Layer<never, never, Probe | Hooks.Dispatch>
  readonly mark: typeof _marked
  readonly verbosity: Layer.Layer<never, never, Setting>
  readonly views: (policy: Pulse.Policy) => Layer.Layer<never, never, Hooks>
  readonly wire: "runtime.pulse"
} = {
  Board: _Board,
  Probe,
  board: _board,
  live: (policy) =>
    Layer.scopedDiscard(Effect.flatMap(
      Ref.make(_NO_SAMPLE),
      (held) => Effect.forkScoped(Effect.repeat(Effect.zipRight(_swept, _fed(held)), Schedule.spaced(policy.cadence))),
    )),
  mark: _marked,
  verbosity: _verbosity,
  views: _views,
  wire: "runtime.pulse",
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pulse }
```

## [07]-[RESEARCH]

(none)
