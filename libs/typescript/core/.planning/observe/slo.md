# [CORE_SLO]

## [01]-[INDEX]

- [03]-[BURN_ROWS]: the multi-window multi-burn-rate table and its derivations.
- [04]-[ALGEBRA]: burn/budget/share arithmetic and the windowed verdict fold.

## [02]-[OBJECTIVE]

- Law: serialized policy schemas enforce ratio, finite-bound, positive-span, distinct-partition, and compliance-window domains.
- Law: SLI schemas admit metrics by statistical role; Ratio counters are distinct, Partition uses one counter, and level cases split gauges.
- Law: exact metric tuples separate temporal histograms, temporal gauges, and non-temporal gauges at admission.
- Law: summaries back no SLI; latency requires histogram buckets, while saturation and freshness compare gauge levels.
- Law: Partition derives a good share from one tagged counter; Ratio is reserved for independent numerator and denominator counters.
- Law: budget is `1 - target`, and an objective window cannot be shorter than its longest burn window.

```typescript signature
import { Array, Duration, Number, Option, Order, Record, Schema } from "effect"
import { Shape } from "../value/schema.ts"
import { Convention } from "./convention.ts"

const _metric = <K extends Convention.InstrumentKind>(kind: K): Schema.Schema<Convention.MetricName<K>> =>
  Convention.Metric.schema.pipe(
    Schema.filter(
      (name): name is Convention.MetricName<K> => Convention.Metric.at(name).kind === kind,
      { identifier: `MetricName/${kind}` },
    ),
  )

const _CounterMetric = _metric("counter")
const _HistogramMetric = _metric("histogram")
const _latencyMetrics = [
  Convention.metric.assetTranscodeDuration, Convention.metric.batchDuration, Convention.metric.gatewayDuration,
  Convention.metric.httpServerDuration, Convention.metric.invokeDuration, Convention.metric.olapDeferred,
  Convention.metric.olapWait, Convention.metric.profileDuration, Convention.metric.securityCeremony,
  Convention.metric.securityJwksResolve, Convention.metric.securityKdf,
] as const
const _freshnessMetrics = [
  Convention.metric.benchGc, Convention.metric.benchTime, Convention.metric.outboxAge, Convention.metric.vitalDuration,
] as const
const _saturationMetrics = [
  Convention.metric.benchCounter, Convention.metric.benchHeap, Convention.metric.derivativeActive,
  Convention.metric.derivativeQueued, Convention.metric.laneCheckpoint, Convention.metric.outboxDepth,
  Convention.metric.outboxRedelivered, Convention.metric.queueDepth, Convention.metric.vitalScore, Convention.metric.vitalSize,
] as const
const _filterKeys = [
  Convention.attr.errorType, Convention.attr.httpMethod, Convention.attr.httpRoute,
  Convention.rasm.admitDisposition, Convention.rasm.admitReason, Convention.rasm.admitScheme,
  Convention.rasm.assetEngine, Convention.rasm.assetOutcome, Convention.rasm.auditAction, Convention.rasm.auditActorKind,
  Convention.rasm.benchBand, Convention.rasm.benchCounterKind, Convention.rasm.benchLabel, Convention.rasm.benchSuite,
  Convention.rasm.benchVerdict, Convention.rasm.cacheName, Convention.rasm.exportFormat, Convention.rasm.exportSource,
  Convention.rasm.factStream, Convention.rasm.formOutcome, Convention.rasm.gatewayOutcome, Convention.rasm.invokeOutcome,
  Convention.rasm.laneName, Convention.rasm.meterResource, Convention.rasm.objectOutcome, Convention.rasm.olapEngine,
  Convention.rasm.poolScheme, Convention.rasm.remoteAction, Convention.rasm.remoteEngine, Convention.rasm.remoteOp,
  Convention.rasm.remoteScheme, Convention.rasm.remoteWatch, Convention.rasm.ring, Convention.rasm.securityDialect,
  Convention.rasm.securityKind, Convention.rasm.securityReason, Convention.rasm.securitySurface, Convention.rasm.tenant,
  Convention.rasm.vitalGrade, Convention.rasm.vitalKind, Convention.rasm.workChannel, Convention.wire.occurrence,
] as const

const _LatencyMetric = Schema.Literal(..._latencyMetrics)
const _FreshnessMetric = Schema.Literal(..._freshnessMetrics)
const _SaturationMetric = Schema.Literal(..._saturationMetrics)
const _Key = Schema.Literal(..._filterKeys)

const _FilterOp = Shape.vocabulary(["equal", "unequal", "regex", "notRegex"] as const, {
  equal: {}, notRegex: {}, regex: {}, unequal: {},
})
const _Scalar = Schema.Union(Schema.String, Schema.Number.pipe(Schema.finite()), Schema.Boolean)
const _Filter = Schema.Union(
  Schema.Struct({ key: _Key, op: Schema.Literal("equal", "unequal"), value: _Scalar }),
  Schema.Struct({ key: _Key, op: Schema.Literal("regex", "notRegex"), value: Schema.String }),
)
const _FilterSet = Schema.Array(_Filter).pipe(Schema.filter((filters) => {
  const identities = Array.map(filters, ({ key, op, value }) => JSON.stringify([key, op, typeof value, value]))
  return Array.dedupe(identities).length === identities.length || "<objective-filter-collision>"
}, { identifier: "DistinctObjectiveFilters" }))
const _FilterOwner = {
  Op: _FilterOp,
  make: Schema.decodeSync(_Filter),
  schema: _Filter,
} as const

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
const _Sample = Schema.Struct({
  breaching: Schema.Int.pipe(Schema.nonNegative()),
  total: Schema.Int.pipe(Schema.nonNegative()),
}).pipe(
  Schema.filter((sample) => sample.breaching <= sample.total, { identifier: "BreachWithinTotal" }),
  Schema.brand("SloSample"),
)
const _Rate = Schema.Number.pipe(Schema.between(0, 1), Schema.brand("SloRate"))

const _breached = (sli: Extract<Reliability.Sli, { readonly _tag: "Saturation" }>, reading: number): boolean =>
  sli.breach === "ceiling" ? reading > sli.bound : reading < sli.bound

const _SliOwner: {
  readonly Freshness: typeof _Freshness.make
  readonly Latency: typeof _Latency.make
  readonly Partition: typeof _Partition.make
  readonly Ratio: typeof _Ratio.make
  readonly Saturation: typeof _Saturation.make
  readonly Sample: typeof _Sample
  readonly breached: (sli: Extract<Reliability.Sli, { readonly _tag: "Saturation" }>, reading: number) => boolean
  readonly rate: (sample: Reliability.Slo.Sample) => Option.Option<Reliability.Slo.Rate>
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

class _Objective extends Schema.Class<_Objective>("Objective")({
  name: Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(80)),
  sli: _Sli,
  filters: Schema.optionalWith(_FilterSet, { default: () => [] }),
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

```

## [03]-[BURN_ROWS]

- Law: a burn row fires only when its long and short windows exceed the factor; every verdict and rendering consumes both.
- Law: burn identifiers stay TypeScript-local; deploy identities, annotations, slugs, grouping, and silencing use each row's wire key.
- Law: burn windows remain Duration values through arithmetic and dialect rendering; consumers never parse strings.
- Growth: a tuned discipline (a fifth row, a different factor) is a table edit; consumers re-derive.

```typescript signature
const _burnKinds = ["pageFast", "pageSlow", "ticketFast", "ticketSlow"] as const
const _burnRows = {
  pageFast: { factor: 14.4, key: "page-fast", long: Duration.hours(1), severity: "page", short: Duration.minutes(5) },
  pageSlow: { factor: 6, key: "page-slow", long: Duration.hours(6), severity: "page", short: Duration.minutes(30) },
  ticketFast: { factor: 3, key: "ticket-fast", long: Duration.hours(24), severity: "ticket", short: Duration.hours(2) },
  ticketSlow: { factor: 1, key: "ticket-slow", long: Duration.hours(72), severity: "ticket", short: Duration.hours(6) },
} as const
const _Burn = Shape.vocabulary(_burnKinds, _burnRows)

```

## [04]-[ALGEBRA]

- Receipt: `Verdict` carries each row's burn state and optional dominant severity as routable data.
- Growth: a new verdict axis is one field on the fold's construction — the table and arithmetic are closed.

```typescript signature
const _burnOf = (objective: _Objective, errorRate: Reliability.Slo.Rate): number => errorRate / objective.budget

const _share = (burn: Reliability.Slo.Burn, objective: _Objective): number =>
  (_Burn.at(burn).factor * Duration.toMillis(_Burn.at(burn).long)) / Duration.toMillis(objective.window)

const _bySeverity: Order.Order<Reliability.Slo.BurnRow["severity"]> = Order.mapInput(Order.boolean, (severity) => severity === "page")

const _evaluate = (objective: _Objective, readings: Reliability.Slo.Readings): Reliability.Slo.Verdict => {
  const rows = Record.fromEntries(Array.map(_Burn.kinds, (kind) => {
    const row = _Burn.at(kind)
    const reading = readings[kind]
    const burn = {
      long: Option.map(_SliOwner.rate(reading.long), (rate) => _burnOf(objective, rate)),
      short: Option.map(_SliOwner.rate(reading.short), (rate) => _burnOf(objective, rate)),
    }
    const state = Option.match(
      Option.zipWith(burn.long, burn.short, (long, short) =>
        long >= row.factor && short >= row.factor),
      { onNone: () => "no-data" as const, onSome: (both) => both ? "firing" as const : "quiet" as const },
    )
    return [kind, { burn, fired: state === "firing", row, state }] as const
  })) as Reliability.Slo.Verdict["rows"]
  const fired = Array.filter(Record.values(rows), (verdict) => verdict.fired)
  return {
    rows,
    severity: Array.match(fired, {                             // the ceiling is one Order policy value: the dominant fired severity, never a branch ladder
      onEmpty: Option.none,
      onNonEmpty: (verdicts) => Option.some(Array.max(Array.map(verdicts, (verdict) => verdict.row.severity), _bySeverity)),
    }),
  }
}

const _spent = (objective: _Objective, errorRate: Reliability.Slo.Rate, elapsed: Duration.Duration): number =>
  (_burnOf(objective, errorRate) * Duration.toMillis(elapsed)) / Duration.toMillis(objective.window)

const _SloOwner: {
  readonly Burn: typeof _Burn
  readonly burn: (objective: _Objective, errorRate: Reliability.Slo.Rate) => number
  readonly evaluate: (objective: _Objective, readings: Reliability.Slo.Readings) => Reliability.Slo.Verdict
  readonly share: (burn: Reliability.Slo.Burn, objective: _Objective) => number
  readonly spent: (objective: _Objective, errorRate: Reliability.Slo.Rate, elapsed: Duration.Duration) => number
} = {
  Burn: _Burn,
  burn: _burnOf,
  evaluate: _evaluate,
  share: _share,
  spent: _spent,
}
```

## [05]-[ALERT_SPECS]

- Law: alert severity derives exactly from burn-row severity values; no independent severity vocabulary exists.
- Law: Board and IaC compile `Reliability.Alert.Spec`; neither consumer re-derives burn thresholds.
- Law: specs carry routing inputs only; deploy configuration owns receivers, schedules, and escalation chains.

```typescript signature
const _severityKinds = ["page", "ticket"] as const
const _severityRows = {
  page: { hold: Duration.zero, tone: "critical", urgency: "interrupt" },
  ticket: { hold: Duration.minutes(30), tone: "warning", urgency: "queue" },
} as const
const _Severity = Shape.vocabulary(_severityKinds, _severityRows)

const _of = (objective: _Objective): ReadonlyArray<Reliability.Alert.Spec> =>
  Array.map(_Burn.kinds, (burn): Reliability.Alert.Spec => {
    const row = _Burn.at(burn)
    return {
      annotations: {
        [Convention.rasm.sloBurn]: row.key,
        [Convention.rasm.sloObjective]: objective.name,
        [Convention.rasm.sloSeverity]: row.severity,
      },
      burn: row.key,
      factor: row.factor,
      filters: objective.filters,
      severity: { ..._Severity.at(row.severity), kind: row.severity },
      sli: objective.sli,
      slug: `${objective.name}:${row.key}`,                     // the row's own wire key, never the record identifier
      spend: _SloOwner.share(burn, objective),
      target: objective.target,
      windows: { long: row.long, short: row.short },
    }
  })

const _AlertOwner: {
  readonly Severity: typeof _Severity
  readonly of: (objective: _Objective) => ReadonlyArray<Reliability.Alert.Spec>
} = {
  Severity: _Severity,
  of: _of,
}

declare namespace Reliability {
  type Filter = typeof _Filter.Type
  type Objective = InstanceType<typeof _Objective>
  type Sli = typeof _Sli.Type
  namespace Slo {
    type Sample = typeof _Sample.Type
    type Rate = typeof _Rate.Type
    type Polarity = (typeof _Saturation.Type)["breach"]
    type Burn = (typeof _Burn.kinds)[number]
    type BurnKey = (typeof _burnRows)[Burn]["key"]
    type BurnRow = (typeof _burnRows)[Burn]
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
  namespace Alert {
    type Severity = Reliability.Slo.BurnRow["severity"]
    type SeverityRow = { readonly hold: Duration.Duration; readonly tone: string; readonly urgency: "interrupt" | "queue" }
    type Spec = {
      readonly annotations: Convention.Attributes
      readonly burn: Reliability.Slo.BurnKey
      readonly factor: number
      readonly filters: ReadonlyArray<Reliability.Filter>
      readonly severity: SeverityRow & { readonly kind: Severity }
      readonly sli: Reliability.Sli
      readonly slug: string
      readonly spend: number
      readonly target: number
      readonly windows: { readonly long: Duration.Duration; readonly short: Duration.Duration }
    }
  }
}

const Reliability = {
  Alert: _AlertOwner,
  Filter: _FilterOwner,
  Objective: _Objective,
  Sli: _SliOwner,
  Slo: _SloOwner,
} as const

export { Reliability }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
