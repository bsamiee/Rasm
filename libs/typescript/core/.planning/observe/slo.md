# [CORE_SLO]

SLO is algebra, not config, and alerting is its total derivation: an `Objective` binds one `Sli` to a target ratio and a compliance window, the multi-window multi-burn-rate discipline is one closed `_BURN` table, and `Alert.of` derives one compilation-ready spec per burn row with zero re-decided thresholds. Nothing redundant is stored — budget, burn, and the human budget-share figure all derive from the admitted rows, and the strict target domain makes a positive error budget structural, so a spec that divides by zero has no construction path.

Every downstream artifact projects these values — `Slo.evaluate` verdicts, `board#PACKS` burn panels and firing annotations, the `iac` rule compile — so a threshold change is one row edit moving verdict, alerts, and dashboards in a single diff. Evaluation stays pure and source-agnostic: readings arrive as sampled breach and total counts, and delivery routing is the deploy plane's concern. Its module is `core/src/observe/slo.ts`; a hand-authored alert rule beside this derivation is the drift defect the total function exists to kill.

## [01]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                  |
| :-----: | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `OBJECTIVE`   | the `Objective` policy owner, the closed `Sli` family, the sample fold  |
|  [02]   | `BURN_ROWS`   | the multi-window multi-burn-rate table and its derivations              |
|  [03]   | `ALGEBRA`     | burn/budget/share arithmetic and the windowed verdict fold              |
|  [04]   | `ALERT_SPECS` | the severity routing rows and the `Objective -> specs` total derivation |

## [02]-[OBJECTIVE]

- Owner: the `Sli` closed family and the `Objective` class — `Sli` is a `Schema.TaggedStruct` union with five cases: `Ratio` (good-events metric over total-events metric), `Partition` (one events metric whose good half is a value set on a declared attribute key), `Latency` (a duration metric against a ceiling, with the display quantile the panels headline), `Saturation` (a level metric against a bound on either polarity), and `Freshness` (an age metric against a staleness horizon) — assembled under one export carrying the case constructors, the `rate` fold, and the level-breach classifier; `Objective` is a `Schema.Class` binding one `Sli` to a `target` ratio and a compliance `window`, with the error budget riding it as a getter.
- Law: the policy owners are Schema-declared because policy serializes — objectives and their derived specs cross into CI artifacts and iac programs — and because the refinements ARE the invariants: `target` admits exactly `0 < target < 1`, `quantile` admits `0 < quantile < 1`, the `Saturation` bound admits any finite number in the level's OWN unit (a degradation rank, a queue depth, and a utilization fraction each bound one shape), the `Partition` good set admits a non-empty distinct value list under a key `Convention.keys` censuses, and the `Latency`/`Freshness` bounds are positive `Duration` values through `Schema.DurationFromMillis`; a policy value violating its domain refuses at decode AND at `.make` construction, so a zero or negative budget is unspellable, never guarded per consumer.
- Law: an SLI names series through instrument-qualified convention rows — `Ratio` admits counter rows and rejects the same counter on both sides, `Partition` admits one counter row, and the two level cases split the gauge roster between them; `_metric(kind)` derives each membership schema from `Convention.instrument`, so a known metric in the wrong statistical role refuses at admission.
- Law: every bound-bearing case admits on the measurement domain BESIDE the role — `Latency` intersects the histogram roster with `Convention.durations`, `Freshness` intersects the gauge roster with it, and `Saturation` takes the gauge complement, all against the temporal name tuple the convention owner's two-way guard closes over its own unit column — so a size or ratio distribution named by a latency objective and a position or ratio level named by a freshness objective both refuse at decode rather than reaching a breach fold where `Convention.duration` scales the bound through a multiplier only a temporal code carries; the two level cases therefore partition rather than overlap, and a first non-temporal distribution lands as one instrument row and strands no objective, no burn rule, and no bucket-bound render.
- Law: a success share over ONE tag-partitioned counter is `Partition`, never a second counter minted to carry the numerator — an outcome, verdict, or disposition dimension already keys the good half, so a good-half twin doubles the mounted series, strands its own denominator on any emission edit, and re-mints per value the dimension keys; `Ratio` stays the shape for genuinely independent counters, and the two cases share every downstream fold because both render a good-over-total quotient.
- Law: every case defines its breach predicate as its own fields, and the sampled shape is uniform — `Slo.Sample` is `{ breaching, total }` where `breaching` counts the case's own breach events: requests over `ceiling` for `Latency`, bad events (`total - good`) for `Ratio`, events whose `by` value falls outside `good` for `Partition`, samples on the breaching side of `bound` for `Saturation`, samples older than `horizon` for `Freshness` — the sample schema proves `breaching <= total` at its filter, so every rate `Sli.rate` folds is `0..1` by construction, and `rate` stays `Option`-returning because an empty window has no rate and the absence folds at the caller, never as `NaN` downstream. Both sides bear the `Latency` `ceiling`: the sampler counts requests over it as breaching, and `board#PACKS` compiles it into the le-share breach expression — `quantile` is display vocabulary only and never enters the burn arithmetic.
- Law: level polarity is one field a sampler and a query compiler both read — `Sli.breached(sli, reading)` classifies one sample and `board#PACKS` folds the same field into its comparison operator, so an exhaustion measure whose breach falls BELOW a floor (remaining life, free capacity, budget remaining) is the same case as a utilization measure and a second saturation shape has no reason to exist.
- Law: `target` is the good-ratio and the positive error budget is its complement — `objective.budget` is the derived number `1 - target`; `window` defaults to 28 days and admits no value shorter than the longest 72-hour burn row, so every derived alert window fits inside its objective.
- Law: dispatch over the family is the held-value record form — `Match.valueTags(sli, arms)` at every consumer (`board#PACKS` `_breach` and `_indicator`, the iac rule compiler reading both) — and the constructors keep the family spelling (`Sli.Ratio({ good, total })`), so promotion off the process-local enum changed no construction site.
- Entry: `new Objective({ name, sli: Sli.Ratio({ good, total }), target: 0.999, window })` where the app declares its reliability policy; `Sli.rate(sample)` at every sampling seam; `Sli.breached(sli, reading)` at every level sampler.
- Growth: a sixth SLI shape is one `Sli` case with its breach-expression arm in `board#PACKS` `_breach` — the sample fold and the burn algebra are already kind-agnostic; a third breach polarity is one literal on `Saturation.breach` with its operator arm.
- Packages: `effect` (`Schema`, `Array`, `Duration`, `Number`, `Option`, `Order`, `Predicate`, `Record`, `Struct`); `./convention.ts` (`Convention`).

```typescript signature
import { Array, Duration, Number, Option, Order, Predicate, Record, Schema, Struct } from "effect"
import { Convention } from "./convention.ts"

const _metric = <K extends Convention.InstrumentKind>(kind: K): Schema.Schema<Convention.MetricName<K>> => {
  const names: ReadonlyArray<Convention.MetricName> = Array.filterMap(
    Record.values(Convention.instrument),
    (row) => row.kind === kind ? Option.some(row.name) : Option.none(),
  )
  return Schema.declare(
    (input: unknown): input is Convention.MetricName<K> => Predicate.isString(input) && Array.some(names, (name) => name === input),
    { identifier: `MetricName/${kind}` },
  )
}

const _CounterMetric = _metric("counter")
const _HistogramMetric = _metric("histogram")
// Every bound-bearing case admits on TWO axes — the instrument role and the measurement domain — because each breach
// fold scales its bound through `Convention.duration`, total over the temporal roster alone: a byte or ratio
// distribution admitted on kind alone compiles a bucket bound no multiplier reaches, and a position or ratio LEVEL
// admitted as an age compares a staleness horizon against a number carrying no clock at all. The two level cases
// therefore PARTITION the gauge roster on that same domain rather than both admitting it whole, so a temporal level is
// a `Freshness` subject and nothing else. Every roster reads as data, so no axis is re-listed.
const _isGaugeMetric = Schema.is(_metric("gauge"))
const _isHistogramMetric = Schema.is(_HistogramMetric)
const _isTemporal = (name: Convention.MetricName): name is Convention.MetricName & Convention.DurationMetric =>
  Array.some(Convention.durations, (temporal) => temporal === name)
const _LatencyMetric: Schema.Schema<Convention.MetricName<"histogram"> & Convention.DurationMetric> = Schema.declare(
  (input: unknown): input is Convention.MetricName<"histogram"> & Convention.DurationMetric => _isHistogramMetric(input) && _isTemporal(input),
  { identifier: "MetricName/temporalHistogram" },
)
const _FreshnessMetric: Schema.Schema<Convention.MetricName<"gauge"> & Convention.DurationMetric> = Schema.declare(
  (input: unknown): input is Convention.MetricName<"gauge"> & Convention.DurationMetric => _isGaugeMetric(input) && _isTemporal(input),
  { identifier: "MetricName/temporalGauge" },
)
const _SaturationMetric: Schema.Schema<Exclude<Convention.MetricName<"gauge">, Convention.DurationMetric>> = Schema.declare(
  (input: unknown): input is Exclude<Convention.MetricName<"gauge">, Convention.DurationMetric> => _isGaugeMetric(input) && !_isTemporal(input),
  { identifier: "MetricName/levelGauge" },
)

const _Key = Schema.declare(
  (input: unknown): input is Convention.Key => Predicate.isString(input) && Array.some(Convention.keys, (key) => key === input),
  { identifier: "AttributeKey" },
)

const _Span = Schema.DurationFromMillis.pipe(Schema.filter((span) => Duration.toMillis(span) > 0, { identifier: "PositiveSpan" }))

const _Ratio = Schema.TaggedStruct("Ratio", { good: _CounterMetric, total: _CounterMetric }).pipe(
  Schema.filter((sli) => sli.good !== sli.total || "<ratio-series-collision>", { identifier: "DistinctRatioSeries" }),
)
const _Partition = Schema.TaggedStruct("Partition", {
  by: _Key,
  good: Schema.NonEmptyArray(Schema.String),
  metric: _CounterMetric,
}).pipe(
  Schema.filter((sli) => Array.dedupe(sli.good).length === sli.good.length || "<partition-value-collision>", { identifier: "DistinctPartitionValues" }),
)
const _Latency = Schema.TaggedStruct("Latency", {
  ceiling: _Span,
  metric: _LatencyMetric,
  quantile: Schema.Number.pipe(Schema.greaterThan(0), Schema.lessThan(1)),
})
const _Saturation = Schema.TaggedStruct("Saturation", {
  bound: Schema.Number.pipe(Schema.finite()),   // the level's own unit: a rank, a depth, and a fraction all bound here
  breach: Schema.Literal("ceiling", "floor"),
  metric: _SaturationMetric,
})
const _Freshness = Schema.TaggedStruct("Freshness", { horizon: _Span, metric: _FreshnessMetric })

const _Sli: Schema.Union<[typeof _Ratio, typeof _Partition, typeof _Latency, typeof _Saturation, typeof _Freshness]> = Schema.Union(
  _Ratio,
  _Partition,
  _Latency,
  _Saturation,
  _Freshness,
)
type Sli = typeof _Sli.Type

const _Sample = Schema.Struct({
  breaching: Schema.Int.pipe(Schema.nonNegative()),
  total: Schema.Int.pipe(Schema.nonNegative()),
}).pipe(
  Schema.filter((sample) => sample.breaching <= sample.total, { identifier: "BreachWithinTotal" }),
  Schema.brand("SloSample"),
)
const _Rate = Schema.Number.pipe(Schema.between(0, 1), Schema.brand("SloRate"))

const _breached = (sli: Extract<Sli, { readonly _tag: "Saturation" }>, reading: number): boolean =>
  sli.breach === "ceiling" ? reading > sli.bound : reading < sli.bound

const Sli: {
  readonly Freshness: typeof _Freshness.make
  readonly Latency: typeof _Latency.make
  readonly Partition: typeof _Partition.make
  readonly Ratio: typeof _Ratio.make
  readonly Saturation: typeof _Saturation.make
  readonly Sample: typeof _Sample
  readonly breached: (sli: Extract<Sli, { readonly _tag: "Saturation" }>, reading: number) => boolean
  readonly rate: (sample: Slo.Sample) => Option.Option<Slo.Rate>
} = {
  Freshness: _Freshness.make,
  Latency: _Latency.make,
  Partition: _Partition.make,
  Ratio: _Ratio.make,
  Saturation: _Saturation.make,
  Sample: _Sample,
  breached: _breached,
  rate: ({ breaching, total }) => Option.map(Number.divide(breaching, total), _Rate.make), // the branded sample proof bounds every non-empty quotient
}

class Objective extends Schema.Class<Objective>("Objective")({
  name: Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(80)),
  sli: _Sli,
  target: Schema.Number.pipe(Schema.greaterThan(0), Schema.lessThan(1)),
  window: Schema.optionalWith(
    _Span.pipe(Schema.filter((span) => Duration.greaterThanOrEqualTo(span, Duration.hours(72)), { identifier: "ComplianceWindow" })),
    { default: () => Duration.days(28) },
  ),
}) {
  get budget(): number {
    return 1 - this.target
  }
}

declare namespace Slo {
  type Sample = typeof _Sample.Type
  type Rate = typeof _Rate.Type
  type Polarity = (typeof _Saturation.Type)["breach"]
  type Objective = InstanceType<typeof Objective>
}
```

## [03]-[BURN_ROWS]

- Owner: the `_BURN` table — the standing multi-window multi-burn-rate discipline: two paging pairs (2% of budget in 1h at 14.4x burn over 5m/1h windows; 5% in 6h at 6x over 30m/6h) and two ticketing pairs (10% in 1d at 3x over 2h/1d; 10% in 3d at 1x over 6h/3d); each row carries `severity`, `long`, `short`, and `factor` — and nothing else, because the budget-share figure derives.
- Law: the two-window trip is the false-positive/reset discipline — the long window proves sustained burn, the short window proves it is still burning now, and a verdict fires only when BOTH exceed the row's factor; the short window is what lets a resolved incident reset quickly instead of paging for the tail of its own long window. Every consumer honors both halves — the runtime probe samples both windows, and `board#PACKS` renders both burn expressions per row, never the long half alone.
- Law: the row set derives — `keyof typeof _BURN` is the burn-kind union, the severity axis projects from rows, and the guard pair closes the table — so `[05]` derives one spec per row and `board#PACKS` one threshold pair per row with zero re-listing.
- Law: the record identifier and the wire key are two columns, never one — the record key is a TypeScript identifier under the branch casing law and `key` is the dotted-lowercase spelling the C# kernel's own burn table carries, so every value crossing a deploy plane (an alert slug, the `sloBurn` annotation, a rule-group identity a re-apply matches on) reads `key` and the two branch spellings of one burn row compile to one provider-side identity; a slug built from the record identifier forks that identity by branch of origin, and the two dialects then group, mute, and silence as different rows.
- Law: the budget-share figure is derived, never carried — `Slo.share(burn, objective)` computes `factor * long / objective.window`, the fraction of the objective's budget a row's sustained burn consumes over its long window, so the human meaning an alert annotation prints cannot disagree with the factor and windows that fire it; the deleted spelling is a stored `spend` column whose value a table edit can strand.
- Law: windows are `Duration` values at the anchor — consumers compose them into `Duration` arithmetic and dialect renders directly, and no reader re-parses a duration string.
- Growth: a tuned discipline (a fifth row, a different factor) is a table edit; consumers re-derive.

```typescript signature
const _BURN = {
  pageFast: { factor: 14.4, key: "page-fast", long: Duration.hours(1), severity: "page", short: Duration.minutes(5) },
  pageSlow: { factor: 6, key: "page-slow", long: Duration.hours(6), severity: "page", short: Duration.minutes(30) },
  ticketFast: { factor: 3, key: "ticket-fast", long: Duration.hours(24), severity: "ticket", short: Duration.hours(2) },
  ticketSlow: { factor: 1, key: "ticket-slow", long: Duration.hours(72), severity: "ticket", short: Duration.hours(6) },
} as const

declare namespace Slo {
  type Burn = keyof typeof _BURN
  type BurnKey = (typeof _BURN)[Burn]["key"]
  type BurnRow = {
    readonly factor: number
    readonly key: BurnKey // the closed wire-key union the table derives: a verdict's row and `Alert.Spec.burn` read one literal type
    readonly long: Duration.Duration
    readonly severity: "page" | "ticket"
    readonly short: Duration.Duration
  }
  type _Rows<T extends { readonly [K in Burn]: BurnRow } = typeof _BURN> = T
  type _Keys<K extends Burn = keyof typeof _BURN> = K
}
```

## [04]-[ALGEBRA]

- Owner: the assembled `Slo` export — the burn table spread in, the arithmetic members, and the verdict fold under one name with companion types on the merged hub; `Slo.Objective` rides the hub as the class's instance alias so every consumer spelling survives the class promotion.
- Law: burn rate is `errorRate / budget` — the multiple of budget-consumption speed — and `Slo.burn` is that one division over the objective's structurally positive budget, so `Infinity` has no construction path.
- Law: `Slo.evaluate(objective, readings)` accepts one long/short `Slo.Sample` pair per burn row and derives both rates internally before burn arithmetic; callers cannot bypass `breaching <= total` with a plain numeric rate, and an empty sample remains `Option.none` in the receipt rather than masquerading as quiet zero.
- Law: budget arithmetic is closed at the objective — `objective.budget`, `Slo.burn`, and `Slo.spent` consume the admitted positive budget; `Slo.share(burn, objective)` derives its denominator from the admitted objective, so no caller can inject a zero or mismatched window.
- Receipt: `Verdict` — per-row fired flags with their burn readings and the dominant severity as `Option` — data a caller routes on, never a side effect; emission belongs to `[05]` specs and runtime consumers.
- Entry: `Slo.evaluate(objective, readings)`; `Slo.burn(objective, errorRate)`; `Slo.share(burn, objective)`; `Slo.spent(objective, errorRate, elapsed)`; `Slo.rows` for derivers.
- Growth: a new verdict axis is one field on the fold's construction — the table and arithmetic are closed.

```typescript signature
declare namespace Slo {
  type Reading = { readonly long: Sample; readonly short: Sample }
  type Readings = { readonly [K in Burn]: Reading }
  type RowVerdict = {
    readonly burn: { readonly long: Option.Option<number>; readonly short: Option.Option<number> }
    readonly fired: boolean
    readonly row: BurnRow
    readonly state: "firing" | "no-data" | "quiet"
  }
  type Verdict = {
    readonly rows: { readonly [K in Burn]: RowVerdict }
    readonly severity: Option.Option<"page" | "ticket">
  }
}

const _burnOf = (objective: Objective, errorRate: Slo.Rate): number => errorRate / objective.budget

const _share = (burn: Slo.Burn, objective: Objective): number =>
  (_BURN[burn].factor * Duration.toMillis(_BURN[burn].long)) / Duration.toMillis(objective.window) // the admitted owner supplies the only lawful denominator

const _bySeverity: Order.Order<Slo.BurnRow["severity"]> = Order.mapInput(Order.boolean, (severity) => severity === "page")

const _evaluate = (objective: Objective, readings: Slo.Readings): Slo.Verdict => {
  const rows = Record.map(_BURN, (row, kind): Slo.RowVerdict => {
    const reading = readings[kind]
    const burn = {
      long: Option.map(Sli.rate(reading.long), (rate) => _burnOf(objective, rate)),
      short: Option.map(Sli.rate(reading.short), (rate) => _burnOf(objective, rate)),
    }
    const state = Option.match(
      Option.zipWith(burn.long, burn.short, (long, short) =>
        long >= row.factor && short >= row.factor),
      { onNone: () => "no-data" as const, onSome: (both) => both ? "firing" as const : "quiet" as const },
    )
    return { burn, fired: state === "firing", row, state }
  })
  const fired = Array.filter(Record.values(rows), (verdict) => verdict.fired)
  return {
    rows,
    severity: Array.match(fired, {                             // the ceiling is one Order policy value: the dominant fired severity, never a branch ladder
      onEmpty: Option.none,
      onNonEmpty: (verdicts) => Option.some(Array.max(Array.map(verdicts, (verdict) => verdict.row.severity), _bySeverity)),
    }),
  }
}

const _spent = (objective: Objective, errorRate: Slo.Rate, elapsed: Duration.Duration): number =>
  (_burnOf(objective, errorRate) * Duration.toMillis(elapsed)) / Duration.toMillis(objective.window)

const Slo: {
  readonly burn: (objective: Objective, errorRate: Slo.Rate) => number
  readonly evaluate: (objective: Objective, readings: Slo.Readings) => Slo.Verdict
  readonly rows: typeof _BURN
  readonly share: (burn: Slo.Burn, objective: Objective) => number
  readonly spent: (objective: Objective, errorRate: Slo.Rate, elapsed: Duration.Duration) => number
} = {
  burn: _burnOf,
  evaluate: _evaluate,
  rows: _BURN,
  share: _share,
  spent: _spent,
}
```

## [05]-[ALERT_SPECS]

- Owner: the `_severity` routing table and the assembled `Alert` export — one severity row per posture (`urgency`: page interrupts a human now, ticket enters the queue; `hold`: how long the condition holds before the spec counts as firing — page rows fire immediately because the short window already debounces, ticket rows hold to suppress flappy toil; `tone`: the one severity-to-tone correspondence, riding annotations and threshold steps alike so no dashboard re-declares it) and `Alert.of` as the one derivation: one spec per burn row, total by construction because the burn table is closed, so every objective yields exactly the burn-table discipline.
- Law: the severity axis is exactly `[03]`'s row projection — the union derives from the burn rows' `severity` column, so a severity this table carries but no burn row produces is dead vocabulary the guard rejects, and the two clusters cannot drift.
- Law: the spec is compilation-ready data — `slug` (the deterministic `${objective.name}:${row.key}` identity both consumers apply on, built from the burn row's wire key so a re-apply updates in place and a peer branch compiling the same objective lands the same rule), the `sli` carried whole (the consumer compiles the case's breach predicate — ceiling, horizon, good/total — into its own query dialect), `target`, the row's `windows`/`factor`, the derived `spend` (`Slo.share` over the objective's own window — the budget fraction the alert's headline prints, computed at derivation so it cannot drift from the row), the severity row inline, and the annotation record typed `Convention.Attributes` under the `Convention.rasm.sloObjective`/`sloSeverity`/`sloBurn` keys — everything a rule compiler or a panel builder needs, nothing it must look up elsewhere.
- Law: consumers compile, never re-derive — `board#PACKS` folds specs into two-window burn panels and firing annotations, `iac` folds the same specs into provider rule resources; a consumer computing its own burn thresholds from the objective has forked the discipline and is the named defect.
- Law: delivery routing is not spec data — receivers, schedules, and escalation chains are deploy-plane configuration keyed by the spec's severity row; the spec's `urgency` is the routing INPUT, the route itself lives where the notifier lives.
- Receipt: `Alert.Spec` — plain policy data; no effect, no fault channel, no emission.
- Entry: `Alert.of(objective)`; `Alert.severity` for posture lookups.
- Growth: a new spec field is one construction line inherited by both consumers; a new severity is first a burn-row change, then its `_severity` row; a routing posture axis (a business-hours gate, an escalation tier) is one column every spec inherits.

```typescript signature
const _severity = {
  page: { hold: Duration.zero, tone: "critical", urgency: "interrupt" },
  ticket: { hold: Duration.minutes(30), tone: "warning", urgency: "queue" },
} as const

declare namespace Alert {
  type Severity = Slo.BurnRow["severity"]
  type SeverityRow = { readonly hold: Duration.Duration; readonly tone: string; readonly urgency: "interrupt" | "queue" }
  type Spec = {
    readonly annotations: Convention.Attributes
    readonly burn: Slo.BurnKey
    readonly factor: number
    readonly severity: SeverityRow & { readonly kind: Severity }
    readonly sli: Sli
    readonly slug: string
    readonly spend: number
    readonly target: number
    readonly windows: { readonly long: Duration.Duration; readonly short: Duration.Duration }
  }
  type _Rows<T extends { readonly [K in Severity]: SeverityRow } = typeof _severity> = T
  type _Keys<K extends Severity = keyof typeof _severity> = K
}

const _of = (objective: Objective): ReadonlyArray<Alert.Spec> =>
  Array.map(Struct.keys(Slo.rows), (burn): Alert.Spec => {
    const row = Slo.rows[burn]
    return {
      annotations: {
        [Convention.rasm.sloBurn]: row.key,
        [Convention.rasm.sloObjective]: objective.name,
        [Convention.rasm.sloSeverity]: row.severity,
      },
      burn: row.key,
      factor: row.factor,
      severity: { ..._severity[row.severity], kind: row.severity },
      sli: objective.sli,
      slug: `${objective.name}:${row.key}`,                     // the row's own wire key, never the record identifier
      spend: Slo.share(burn, objective),
      target: objective.target,
      windows: { long: row.long, short: row.short },
    }
  })

const Alert: {
  readonly of: (objective: Objective) => ReadonlyArray<Alert.Spec>
  readonly severity: typeof _severity
} = {
  of: _of,
  severity: _severity,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Alert, Objective, Sli, Slo }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
