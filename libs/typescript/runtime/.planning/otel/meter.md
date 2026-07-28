# [RUNTIME_METER]

`Pulse` is the work-plane meter bridge — one lossy projection from durable-work evidence onto Convention-keyed Effect instruments, so queue depth, drain lag, and relay throughput read as OTel series while every dispute settles against the journal. `mark` folds a settlement fact into its counter row at the emitting call site, and `live` runs the sampled census sweep setting every gauge row from one `Probe` port the app root satisfies with the data journal's census statement — fact rows stay the billing truth, instruments stay bounded, and neither plane re-derives the other.

Two policy seams close at the same altitude: `verbosity` wires the config tier table into `Logger.minimumLogLevel` so the declared `verbose` column governs the process log floor, and `views` contributes the whole metric-stream governance table through the `Hooks` registry the export lanes drain. `board` projects the instrument and budget rows into the typed `Pulse.Board` deploy-feed value filling the core `DashboardModel` pack payloads, so a budget edit moves the emission grade and the board panel in one place. Its module is `runtime/src/otel/meter.ts`.

## [01]-[INDEX]

- [02]-[PROJECTION] — the mounted instrument tables, the one polymorphic mount, and the `mark` fold; `Pulse`.
- [03]-[CENSUS] — the `Probe` port and the sampled gauge sweep Layer; `Pulse`.
- [04]-[VERBOSITY] — the tier-table to `Logger.minimumLogLevel` wiring; `Pulse`.
- [05]-[VIEWS] — the metric-stream governance table contributed as one `Hooks` node; `Pulse`.
- [06]-[BOARD] — the typed deploy-feed pack folding instrument rows and vital budgets; `Pulse`.

## [02]-[PROJECTION]

- Owner: the interior `_WORK` and `_GAUGES` row tables and `_row`, the one row builder — every row carries its `Convention.named` metadata, the instrument `Convention.mount` materializes from that same row, and its own tag roster, so the settlement kinds (`drained`: relay claims settled; `parked`: deliverables diverted to the dead set) and the census levels are declared once and read by the mark fold, the sweep, and the board projection alike — the governance allow-list reads the vocabulary owner instead, because it governs every plane minting under `rasm.*`, not this module's rows. `Pulse.mark(kind, channel, count?)` is the projection fold: it increments the row's counter tagged `Convention.rasm.workChannel`, so the emitting owner adds one composed line beside its `Fact.record` call and the instrument mints nowhere else.
- Law: the projection is lossy by design — the journal fact is the truth a billing or forensic read settles against, the counter is the dashboard series, and the two emit from ONE call site so they cannot disagree on what happened, only on retention; a missing metric point is a dashboard gap, never an evidence defect.
- Law: materialization is the vocabulary owner's — `Convention.mount` reads wire form, description, bucket ladder, value width, and UCUM code off the named row, so this module composes handles and declares no constructor, no boundary vector, and no unit tag; a kind-dispatch table here is the second materialization owner the core ruling deletes.
- Law: `_WORK` and `_GAUGES` name counting and level rows alone — a word census updates on a WORD rather than a number, so neither the mark fold nor a census read consumes one and those rows mount at the capability folder producing the words.
- Law: the carrier tag rides the mount at one value per instrument, so it adds no cardinality.
- Law: two tables stand because two consumers do — counters answer `mark` and gauges answer the sweep, so each table's row type fixes the mounted instrument exactly and neither consumer casts; the board fold reads both through one concatenation, so a new instrument is one row in its owning table and appears on every downstream projection by construction.
- Law: `channel` values are the work plane's own closed channel vocabulary (the deliver channel rows, the queue lane names) and the PUBLISHER owns that boundedness, because `otel` and `work` seat at one stratum with the edge running work-to-otel, so importing those rosters back inverts it. Structural guarding stands regardless: the tag key is a `Convention.rasm` row, so `[05]`'s allow-list admits it under `views.tenant.limit` and a runaway channel value folds into the overflow bucket at `emit#GOVERNANCE` instead of fanning the series. Identifier-grade values ride span attributes, never this tag.
- Entry: `Pulse.mark("drained", channel, settled)` beside the relay's drain fact; `Pulse.mark("parked", channel)` beside the park fact.
- Growth: a new settlement kind is one `_WORK` row and a new census level one `_GAUGES` row, each naming its Convention metric.
- Boundary: the facts themselves are the work plane's (`work/deliver`, `work/queue`) and the journal is the data plane's; this page owns only the projection.
- Packages: `effect` (`Metric`, `Array`, `Record`); `@rasm/ts/core` (`Convention`).

```typescript signature
import { AggregationType, createAllowListAttributesProcessor, createDenyListAttributesProcessor } from "@opentelemetry/sdk-metrics"
import { type AppIdentity, Convention } from "@rasm/ts/core"
import {
  Array, Context, Duration, Effect, Layer, Logger, LogLevel, Metric, Option, Record, Schedule, Schema,
} from "effect"
import { Setting } from "../proc/config.ts"
import { Hooks } from "./emit.ts"
import { Vital } from "./vital.ts"

type _Row<N extends Convention.MetricName> = {
  readonly instrument: Convention.Named[N]
  readonly metric: Convention.Mounted<N>
  readonly tags: ReadonlyArray<string>
}

// Word rosters thread through both builders because the mount demands one exactly where the family counts words:
// no row here names such a family, so every call site passes none and a later frequency row cannot slip in untyped.
const _row = <N extends Convention.MetricName>(
  metric: N,
  tags: ReadonlyArray<string>,
  ...words: Convention.Words<N>
): _Row<N> => ({ instrument: Convention.named[metric], metric: Convention.mount(metric, ...words), tags })

const _level = <N extends Convention.MetricName>(
  metric: N,
  read: (census: Pulse.Census) => number,
  ...words: Convention.Words<N>
) => ({ ..._row(metric, [], ...words), read })

const _WORK = {
  // each Convention row rides beside its instrument, so the board fold reads the row and the mark fold reads the metric
  drained: _row(Convention.metric.relayDrained, [Convention.rasm.workChannel]),
  parked: _row(Convention.metric.queueParked, [Convention.rasm.workChannel]),
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
      // this row selects the raw-provider instrument space, so its name is a foreign glob: every rasm.* distribution
      // is Effect-minted and fixes its buckets from its own Convention ladder, where a view re-arm governs nothing
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

- Owner: the `Probe` port, the `_GAUGES` level table, and the sweep — `Probe` is one `Context.Tag` whose `census` member answers the current outbox and queue truth, each `_GAUGES` row carries its own `read` projection off that census, and `Pulse.live(policy)` is a `Layer.scopedDiscard` forking one `Schedule.spaced(policy.cadence)` repeat that folds the whole table per sample; the fork dies with the graph scope, so a leaked sweep fiber is structurally impossible.
- Law: the row owns its projection, so the sweep is total by construction — a new census dimension is one row carrying its Convention gauge and its reader, and the fold reaches it with zero sweep edits; a sweep enumerating gauges by hand strands every dimension added after it.
- Law: the port keeps the strata clean — the data journal's `Journal.census` statement satisfies `Probe` at the app root, so the outbox truth crosses the seam as a value and this module imports no SQL surface; the queue depth arrives from the durable-queue owner's own read through the same binding.
- Law: the probe is total by contract — the satisfying binding internalizes its store faults (the prior sample or a zero census stands in), because a broken gauge sweep must degrade a dashboard, never fail a process. Contracts type against a FAILURE and a defect escapes them, so the sweep folds defects itself: an unchecked store fault costs one interval and reads on the log rail, where an unfolded one kills the repeat fiber and freezes every gauge at its last value for the process lifetime — a dashboard reading stale levels as live ones.
- Law: gauges are sampled, never accumulated — depth, age, and redelivery are census facts of one instant, so the sweep sets absolute levels and rate questions (DLQ rate, redelivery rate) derive in the query plane from the counter and gauge series.
- Entry: `Pulse.live(policy)` merged at the composition root beside `Export.live`, after the root binds `Probe`.
- Growth: a new census dimension is one `Census` field and one `_GAUGES` row reading it.
- Packages: `effect` (`Context`, `Layer`, `Schedule`, `Metric`, `Duration`).

```typescript signature
class Probe extends Context.Tag("runtime/Pulse/Probe")<Probe, {
  readonly census: Effect.Effect<Pulse.Census>
}>() {}

const _GAUGES = {
  // each level row carries its own census projection, so the sweep is a total fold and growth costs one row; a temporal
  // row converts through `Convention.duration` because the multiplier belongs to its unit column, never to this call site
  outboxAge: _level(Convention.metric.outboxAge, (census) => Convention.duration(Convention.metric.outboxAge, census.outbox.age)),
  outboxDepth: _level(Convention.metric.outboxDepth, (census) => census.outbox.depth),
  outboxRedelivered: _level(Convention.metric.outboxRedelivered, (census) => census.outbox.redelivered),
  queueDepth: _level(Convention.metric.queueDepth, (census) => census.queue.depth),
} as const

const _swept: Effect.Effect<void, never, Probe> = Effect.catchAllDefect(
  Effect.flatMap(Probe, (probe) =>
    Effect.flatMap(probe.census, (census) =>
      // one census, one sequential fold: every level is a pure local write, so a fiber per row buys nothing and costs scheduling
      Effect.forEach(Record.values(_GAUGES), (row) => Metric.set(row.metric, row.read(census)), { discard: true }))),
  // one bad sample costs one interval: an unfolded defect ends the repeat and freezes every gauge at its last value
  (defect) => Effect.annotateLogs(Effect.logWarning("<census-sweep>"), { detail: String(defect) }),
)
```

## [04]-[VERBOSITY]

- Owner: `Pulse.verbosity` — one Layer wiring the config tier table into the process log floor: it reads `Setting.serve.tier`, projects the tier's `verbose` column through the `Setting.tiers` anchor, and installs `Logger.minimumLogLevel` — `Debug` where the tier is verbose, `Info` otherwise — so the declared column governs every `Effect.log*` call in the process and no page carries a level literal.
- Law: the floor is one root decision — the Layer merges once at the composition root beneath the export lane, so the OTLP log leg and any file logger both inherit it; a per-module level override is a `Logger.withMinimumLogLevel` region on the owning rail, never a second root install.
- Law: this floor governs the Effect log rail alone — the SDK's own diagnostic stream rides `Export.Policy.diagnostic` into `diag.setLogger` at `emit#LANES`, and the `LoggerProvider` scope configurator stays unmounted, so exactly one owner governs each stream and a record dropped at one floor is never re-admitted at another.
- Entry: `Pulse.verbosity` at the composition root.
- Packages: `effect` (`Logger`, `LogLevel`, `Layer`); `../proc/config.ts` (`Setting`).

```typescript signature
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
- Packages: `@opentelemetry/sdk-metrics` (`createAllowListAttributesProcessor`, `createDenyListAttributesProcessor`, `AggregationType`); `effect` (`Record`); `@rasm/ts/core` (`Convention`); `./emit.ts` (`Hooks`).

```typescript signature
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
    // vocabulary owner IS the roster: every dimension a rasm.* plane declares survives, and the unit carrier is not one
    attributesProcessors: [createAllowListAttributesProcessor([...Convention.dimensions, ...policy.views.tenant.keys])],
    instrumentName: "rasm.*",
  }),
} as const

const _views = (policy: Pulse.Policy): Layer.Layer<never, never, Hooks> =>
  Hooks.contribute((hooks) =>
    Effect.forEach(Record.values(_VIEWS), (row) => hooks.add("views", row(policy)), { discard: true }))
```

## [06]-[BOARD]

- Owner: `Pulse.Board` and `Pulse.board(identity)` — the census projection folding the `_WORK`/`_GAUGES` instrument rows and the `Vital.rows` budget table into one Schema-classed deploy-feed value: `panels` carry name, description, UCUM unit, instrument kind, and tag keys off the row's own Convention metadata; `budgets` carry each vital kind's good/poor thresholds, unit, and the level series `Vital.level` selects for it; `burn` carries the SLO burn-rate input pairs — a bad and total series with an optional tag slice — so the boards derive from the same rows the emitters write, and a new instrument or vital appears on the board by construction because the fold reads the tables, never a hand roster.
- Law: every panel column reads the row, never the fold — kind, unit, description, and tag keys all live on `Convention.instrument` and the row's tag column, so one concatenated map over both tables emits every panel and a fold re-stating a kind is the duplication this projection deletes.
- Law: the pack is runtime's mint and the app projects, never redefines — `budgets` rows land as the core `vital` pack's `gauges` payload (`kind`, the `metric` its unit selects, and the `poor` threshold as the gauge ceiling), `burn` rows feed the app's `Slo.Objective` inputs, and the encoded `DashboardModel` values those packs mint reach the iac `Boards` compile leg as the `runtime.pulse` pack rows; board truth cannot drift from emission truth because the fold reads the instrument tables, and no iac decode of this class exists — the deploy plane ingests core-encoded models alone.
- Law: burn inputs are series names, never queries — the vital pair (total `Convention.metric.vitalObserved`, bad the same series sliced `vitalGrade=poor`) and the work pair (total `relayDrained`, bad `queueParked`) are data rows; the burn-rate algebra, objectives, and window ladders stay the core slo plane's, compiled by the iac observe fold.
- Law: `Pulse.wire` is the provenance key this producer mints — the deploy tuple admits a pack only under a key its producing branch spells, so the constant lives beside the projection earning it and the app's deploy feed stamps it rather than re-typing a literal; a key spelled at the consuming tier alone forks the moment either end edits it.
- Entry: `Pulse.board(identity)` at the app's deploy-feed seam — a pure value mint, no Layer; the app maps `budgets` onto `DashboardModel.pack("vital", board, { gauges })` under the root's own `DashboardModel.Board` context, folds `burn` rows into its objective set, and stamps `Pulse.wire` on the encoded pack. `Pulse.Board` is this page's census value and `DashboardModel.Board` the core pack's emitter-and-plane context; the two never substitute.
- Growth: a new burn family is one `_BURN` row; a new panel axis is one field on the panel struct every producer inherits.
- Packages: `effect` (`Schema`, `Array`, `Record`, `Option`); `./vital.ts` (`Vital.rows`); `@rasm/ts/core` (`Convention`, `AppIdentity`).

```typescript signature
class _Board extends Schema.Class<_Board>("Pulse/Board")({
  app: Schema.NonEmptyString,
  panels: Schema.Array(Schema.Struct({
    description: Schema.String,
    instrument: Schema.Literal(...Convention.kinds), // the vocabulary owner's roster: a wire form it gains widens the panel union with it
    name: Schema.NonEmptyString,
    tags: Schema.Array(Schema.String),
    unit: Schema.Literal(...Convention.units), // UCUM codes close the same way the kind column does: a free-string unit passes the rescale divergence a name-only proof misses
  })),
  budgets: Schema.Array(Schema.Struct({
    good: Schema.Number,
    kind: Schema.NonEmptyString,
    metric: Schema.Literal(...Vital.levels), // level series the kind's unit selects; the tuple keeps the decoded union closed
    poor: Schema.Number,
    unit: Schema.Literal(...Convention.units),
  })),
  burn: Schema.Array(Schema.Struct({
    bad: Schema.NonEmptyString,
    slice: Schema.optionalWith(Schema.Struct({ tag: Schema.NonEmptyString, value: Schema.NonEmptyString }), { as: "Option" }),
    total: Schema.NonEmptyString,
  })),
}) {}

const _BURN = [
  // burn inputs as data: the algebra, objectives, and window ladders are the iac observe plane's
  {
    bad: Convention.metric.vitalObserved,
    slice: Option.some({ tag: Convention.rasm.vitalGrade, value: "poor" }),
    total: Convention.metric.vitalObserved,
  },
  { bad: _WORK.parked.instrument.name, slice: Option.none(), total: _WORK.drained.instrument.name },
] as const

const _board = (identity: AppIdentity): _Board =>
  new _Board({
    app: identity.app,
    panels: Array.map(
      [...Record.values(_WORK), ...Record.values(_GAUGES)],
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
  readonly board: (identity: AppIdentity) => _Board
  readonly live: (policy: Pulse.Policy) => Layer.Layer<never, never, Probe>
  readonly mark: typeof _marked
  readonly verbosity: Layer.Layer<never, never, Setting>
  readonly views: (policy: Pulse.Policy) => Layer.Layer<never, never, Hooks>
  readonly wire: "runtime.pulse"
} = {
  Board: _Board,
  Probe,
  board: _board,
  live: (policy) => Layer.scopedDiscard(Effect.forkScoped(Effect.repeat(_swept, Schedule.spaced(policy.cadence)))),
  mark: _marked,
  verbosity: _verbosity,
  views: _views,
  // Provenance key minted where the projection earning it lives, so the deploy tuple and this producer hold one
  // spelling and the app's deploy feed stamps a constant rather than re-typing the literal that admits its pack.
  wire: "runtime.pulse",
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Pulse }
```

## [07]-[RESEARCH]

(none)
