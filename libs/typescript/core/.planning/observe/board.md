# [CORE_BOARD]

Dashboards are identity-derived data, and the pack library is the same owner's dispatch: `DashboardModel` is one `Schema.Class` carrying deterministic identity, the closed panel family, template variables, alert annotations, refresh/range defaults, the shelf-layout fold, and the mapped pack/suite records, while `Query` is the recursive metric-expression algebra under one render fold and `Bench` is the structural baseline-versus-candidate regression fold whose graded verdicts the claim bridge meters. A pack projects only vocabulary the `Convention`, `Slo`, and payload rows already own, so every declared instrument has a board consumer and a hand-authored dashboard has no authoring surface.

`DashboardModel.snapshot` is the in-process read twin over `Metric.snapshot`, filtering the global registry to `Convention.metric` rows for doctor consumers operating without a telemetry backend. Its module is `core/src/observe/board.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                                             |
| :-----: | :-------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `QUERY`   | the typed expression family, its fn/op/agg vocabularies, the literal grammar, the one render fold  |
|  [02]   | `PANEL`   | the closed panel row family and the shelf-layout grid fold                                         |
|  [03]   | `MODEL`   | the `DashboardModel` owner: identity-derived uid, variables, annotations                           |
|  [04]   | `BENCH`   | the structural claim shape and the baseline-versus-candidate regression fold                       |
|  [05]   | `PACKS`   | the pane builders, the payload map, the pack record, dispatch, and suite                           |

## [02]-[QUERY]

- Owner: the `Query` closed family — selectors carry equality and regex matcher rows, `Windowed` carries range functions, `Quantile` fuses histogram aggregation, `Aggregate` carries grouping posture, `Rank` carries the arity-bearing `topk`/`bottomk` pair, `Binary` carries arithmetic, comparison, and set operators, and `Const` carries scalar literals; cases describe grammar while `_FNS`/`_OPS`/`_AGG`/`_RANK`/`_MATCH` rows generate the operator space.
- Law: series names are `Convention.MetricName` rows and label keys are `Convention.Key` rows — `Query.Labels` is the closed `Convention.Attributes` stamping record widened by the `_DIALECT` pair, the histogram `le` bucket label and the frequency `key` occurrence label, both export-contract facts rather than emission-plane keys — so the algebra admits no free-string metric, no unowned label key, and no off-vocabulary bounded value, and the tenant template variable enters as an ordinary label value (`$tenant`).
- Law: the rendered string owns a literal grammar — `_literal` delegates scalar quoting and every control-character escape to `JSON.stringify`, which is compatible with PromQL string literals; metric names, label keys, equality values, and regex values all cross that one seam.
- Law: label emission is census-ordered — `_selector` walks `_LABEL_KEYS` (the `Convention.keys` census with `le`) and probes the record per key through `Option.fromNullable`, so pair order is the vocabulary's declaration order, absent keys emit nothing, and two equal `Query` values render byte-identically; an `Object.keys` walk re-imports per-record insertion order and is the deleted spelling.
- Law: windows are positive `Duration` values rendered without rounding or one closed provider interval token (`$__rate_interval`) — integral seconds use `s`, subsecond values use exact `ms`, and an arbitrary dialect window string is unspellable.
- Law: `Windowed` renders by operand shape — a selector operand takes the range form `fn(selector[w])`, any composed operand takes the subquery form `fn((expr)[w:])` — so time-share expressions (`avg_over_time` of a bool comparison) compose from the same rows and no builder hand-writes subquery syntax.
- Law: the rendered dialect is Prometheus UTF-8 — every selector emits the quoted `{"metric.name","label.key"="value"}` form and every grouping key quotes through the same `_literal` seam, because dotted OTel names are not legacy-PromQL identifiers; the quantile arm aggregates the windowed rate `by (le)` before `histogram_quantile`, the histogram-series contract the backend demands.
- Law: one dialect fold — a second backend dialect is a second render fold over the SAME family, never a second family; the expression data is dialect-free by construction.
- Law: the render fold IS the dialect's codegen output — PromQL is a single-line dialect whose rendered string is byte-load-bearing (quoted UTF-8 selector identity), so a document-assembly layer (`@effect/printer` `Doc`/`encloseSep`) is rejected: layout grouping and reflow forge selector spelling, and the closed family already owns every arm.
- Law: the fn/op/agg vocabularies stay interior — `_FNS`/`_OPS` are `as const satisfies` row tables no export reaches, their unions derive as the interior `_Fn`/`_Op`/`_Agg` aliases the case fields consume, and consumers speak literals the fields already type; the `type`-plus-`const` pair is the family's whole public spelling.
- Entry: constructors ride the family (`Query.Windowed({ fn: "rate", of, window })`), `Query.render(query)` at pack-build time.
- Growth: a new function or operator is one `_FNS`/`_OPS`/`_AGG` row; a new grammar shape is one case with its render arm the compiler enforces at the fold; an arity-bearing aggregation (`topk`) lands in the `Rank` case because parameter arity is its distinct grammar discriminant.
- Packages: `effect` (`Array`, `Data`, `Duration`, `Match`, `Number`, `Option`, `Record`, `Schema`, `pipe`); `convention#IDENTITY_PROJECTION` (`Convention` rows and the `keys` census).

```typescript signature
import { Array, Data, Duration, Effect, Match, Metric, MetricPair, MetricState, Number, Option, Record, Schema, Struct, pipe } from "effect"
import type { stats as MitataStats } from "mitata"
import type { AppIdentity } from "../value/identity.ts"
import { Convention } from "./convention.ts"
import { Alert, type Sli, type Slo } from "./slo.ts"

const _FNS = {
  avg: "avg_over_time",
  delta: "delta",
  increase: "increase",
  max: "max_over_time",
  min: "min_over_time",
  rate: "rate",
} as const satisfies Record<string, string>

const _OPS = {
  add: "+",
  and: "and",
  div: "/",
  eq: "== bool",
  gte: ">= bool",
  gt: "> bool",
  lte: "<= bool",
  lt: "< bool",
  mod: "%",
  mul: "*",
  neq: "!= bool",
  or: "or",
  pow: "^",
  sub: "-",
  unless: "unless",
} as const satisfies Record<string, string>

const _AGG = ["avg", "count", "group", "max", "min", "stddev", "stdvar", "sum"] as const
const _RANK = ["bottomk", "topk"] as const
const _MATCH = { equal: "=", unequal: "!=", regex: "=~", notRegex: "!~" } as const
const _INTERVAL = { rate: "$__rate_interval" } as const
const _DIALECT = ["key", "le"] as const // export-contract labels: the frequency occurrence value and the histogram bucket bound

type _Agg = (typeof _AGG)[number]
type _Fn = keyof typeof _FNS
type _Op = keyof typeof _OPS
type _Rank = (typeof _RANK)[number]
type _RankCount = typeof _RankCount.Type
type _Finite = typeof _Finite.Type
type _Quantile = typeof _Quantile.Type
type _QuerySpan = typeof _QuerySpan.Type

const _RankCount = Schema.Int.pipe(Schema.positive(), Schema.brand("RankCount"))
const _Finite = Schema.Number.pipe(Schema.finite(), Schema.brand("QueryFinite"))
const _Quantile = Schema.Number.pipe(Schema.greaterThan(0), Schema.lessThan(1), Schema.brand("QueryQuantile"))
const _QuerySpan = Schema.DurationFromSelf.pipe(
  Schema.filter((span) => Duration.toMillis(span) > 0, { identifier: "QuerySpan" }),
  Schema.brand("QuerySpan"),
)

declare namespace Query {
  type Dialect = (typeof _DIALECT)[number]
  type Labels = { readonly [K in Convention.Key]?: Convention.ValueOf<K> extends ReadonlyArray<Convention.Scalar> ? never : Convention.ValueOf<K> }
    & { readonly [K in Dialect]?: string }
  type Matcher = { readonly key: Convention.Key | Dialect; readonly op: keyof typeof _MATCH; readonly value: Convention.Scalar }
  type Finite = _Finite
  type QuantileValue = _Quantile
  type Span = _QuerySpan
  type Window = Span | (typeof _INTERVAL)[keyof typeof _INTERVAL]
}

type Query = Data.TaggedEnum<{
  Aggregate: { readonly by: ReadonlyArray<Convention.Key | Query.Dialect>; readonly of: Query; readonly op: _Agg; readonly without?: boolean }
  Binary: { readonly left: Query; readonly op: _Op; readonly right: Query }
  Const: { readonly value: Query.Finite }
  Instant: { readonly labels: Query.Labels; readonly matchers?: ReadonlyArray<Query.Matcher>; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: Query.Labels; readonly metric: Convention.MetricName<"histogram">; readonly q: Query.QuantileValue; readonly window: Query.Window }
  Rank: { readonly count: _RankCount; readonly of: Query; readonly op: _Rank }
  Windowed: { readonly fn: _Fn; readonly of: Query; readonly window: Query.Window }
}>
const _Query = Data.taggedEnum<Query>()

const _LABEL_KEYS: ReadonlyArray<Convention.Key | Query.Dialect> = [...Convention.keys, ..._DIALECT]

const _literal = (value: Convention.Scalar): string => JSON.stringify(String(value)) ?? '""'

const _span = (window: Query.Window): string =>
  typeof window === "string"
    ? window
    : pipe(Duration.toMillis(window), (millis) => millis % 1000 === 0 ? `${millis / 1000}s` : `${millis}ms`)

const _selector = (metric: Convention.MetricName, labels: Query.Labels, matchers: ReadonlyArray<Query.Matcher> = []): string =>
  pipe(
    [
      ...Array.filterMap(_LABEL_KEYS, (key) =>
        Option.map(Option.fromNullable(labels[key]), (value) => `${_literal(key)}=${_literal(value)}`)),
      ...Array.map(matchers, ({ key, op, value }) => `${_literal(key)}${_MATCH[op]}${_literal(value)}`),
    ],
    (pairs) => `{${_literal(metric)}${pairs.length === 0 ? "" : `,${Array.join(pairs, ",")}`}}`,
  )

const _render = (query: Query): string =>
  _Query.$match(query, {
    Aggregate: ({ by, of, op, without }) =>
      `${op}${by.length === 0 ? "" : ` ${without === true ? "without" : "by"} (${Array.join(Array.map(by, _literal), ",")})`} (${_render(of)})`,
    Binary: ({ left, op, right }) => `(${_render(left)}) ${_OPS[op]} (${_render(right)})`,
    Const: ({ value }) => `${value}`,
    Instant: ({ labels, matchers, metric }) => _selector(metric, labels, matchers),
    Quantile: ({ labels, metric, q, window }) =>
      `histogram_quantile(${q}, sum by (le) (rate(${_selector(metric, labels)}[${_span(window)}])))`,
    Rank: ({ count, of, op }) => `${op}(${count}, ${_render(of)})`,
    Windowed: ({ fn, of, window }) =>
      of._tag === "Instant" ? `${_FNS[fn]}(${_render(of)}[${_span(window)}])` : `${_FNS[fn]}((${_render(of)})[${_span(window)}:])`,
  })

const Query: Data.TaggedEnum.Constructor<Query> & {
  readonly breach: (sli: Sli, window: Query.Window, labels?: Query.Labels) => Query
  readonly burn: (spec: Alert.Spec, labels?: Query.Labels) => Query
  readonly finite: typeof _Finite.make
  readonly interval: typeof _INTERVAL
  readonly quantile: typeof _Quantile.make
  readonly rankCount: typeof _RankCount.make
  readonly render: (query: Query) => string
  readonly span: typeof _QuerySpan.make
} = {
  ..._Query,
  breach: (sli, window, labels = {}) => _breach(sli, window, labels),
  burn: (spec, labels = {}) => _burned(spec, labels),
  finite: _Finite.make,
  interval: _INTERVAL,
  quantile: _Quantile.make,
  rankCount: _RankCount.make,
  render: _render,
  span: _QuerySpan.make,
}
```

## [03]-[PANEL]

- Owner: the closed panel family — `_PanelFields` is the shared emission record for axes, description, interaction, links, repeat variable, grid span, transformations, transparency, and title; `Timeseries`, `Stat`, `Gauge`, `Heatmap`, `Logs`, `Table`, `Geomap`, and `Nodes` embed it and add only their genuinely distinct visualization payload.
- Law: panels store RENDERED expressions — `Query` is the build-time algebra, the panel is the emission-ready datum — so the model serializes completely and the query family never needs a schema twin.
- Law: rows are emission-complete — threshold steps, legend format, and unit are semantic panel facts declared here so `iac` maps rows to provider fields verbatim and invents nothing; the datasource binding, folder placement, and apply lifecycle stay provider facts on `iac`'s side of the seam.
- Law: every panel row maps onto one Foundation-SDK builder — the `_tag` selects its admitted builder subpath and the shared `_PanelFields` land on inherited members, so the iac compile leg is a per-tag fold over typed builders and a panel field with no cataloged builder member stays out of the settled family.
- Law: every visualization case carries the policy its name promises — interaction owns tooltip/zoom/brush, heatmaps own color and bucket scales, logs own ordering/deduplication/wrapping, tables own sort, geomaps own coordinate/label/weight mappings, and node graphs own node/edge identity mappings; these remain fields on the case, never provider-only option bags or parallel DTOs — and a field the pinned provider SDK exposes no builder member for is deleted at this owner, never carried as an inert emission fact.
- Law: `Geomap` and `Nodes` are the spatial and relational rows the BIM/geo and dependency planes fill through later-wave payloads — a geo-features pack or an element-graph pack is one pack row over these existing panel rows, never a panel family fork.
- Law: layout derives — `DashboardModel.laid(model)` is a `mapAccum` shelf fold assigning `{ x, y, w, h }` positions across the 24-column grid from each panel's `span`, wrapping when a shelf overflows and advancing by the tallest panel on the shelf; a hand-positioned panel does not exist, and a layout change is a fold change applied to every dashboard at once.
- Growth: a new visualization kind is one tagged row with its arm in consumers' emission folds.
- Packages: `effect` (`Schema`).

```typescript signature
const _Span = Schema.Struct({
  h: Schema.Int.pipe(Schema.between(2, 24)),
  w: Schema.Int.pipe(Schema.between(2, 24)),
})

const _Threshold = Schema.Struct({ at: Schema.Number, tone: Schema.NonEmptyString })
const _Axis = Schema.Struct({
  label: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  max: Schema.optionalWith(Schema.Number, { as: "Option" }),
  min: Schema.optionalWith(Schema.Number, { as: "Option" }),
  placement: Schema.Literal("left", "right", "hidden"),
  scale: Schema.Literal("linear", "log2", "log10", "symlog"),
  unit: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})
const _Link = Schema.Struct({ title: Schema.NonEmptyString, url: Schema.NonEmptyString })
const _Interaction = Schema.Struct({
  brush: Schema.optionalWith(Schema.Boolean, { default: () => false }),
  tooltip: Schema.optionalWith(Schema.Literal("hidden", "multi", "single"), { default: () => "multi" as const }),
  zoom: Schema.optionalWith(Schema.Boolean, { default: () => true }),
})
const _Transform = Schema.Union(
  Schema.TaggedStruct("Calculate", { alias: Schema.NonEmptyString, expression: Schema.NonEmptyString }),
  Schema.TaggedStruct("Filter", { field: Schema.NonEmptyString, op: Schema.Literal("equal", "greater", "less", "match", "notEqual"), value: Schema.Union(Schema.String, Schema.Number, Schema.Boolean) }),
  Schema.TaggedStruct("Group", { by: Schema.NonEmptyArray(Schema.NonEmptyString), reducers: Schema.NonEmptyArray(Schema.Literal("count", "first", "last", "max", "mean", "min", "sum")) }),
  Schema.TaggedStruct("Join", { how: Schema.Literal("inner", "left", "outer"), on: Schema.NonEmptyArray(Schema.NonEmptyString) }),
  Schema.TaggedStruct("Organize", { order: Schema.Array(Schema.NonEmptyString), rename: Schema.Record({ key: Schema.NonEmptyString, value: Schema.NonEmptyString }) }),
  Schema.TaggedStruct("Reduce", { fields: Schema.NonEmptyArray(Schema.NonEmptyString), reducer: Schema.Literal("count", "first", "last", "max", "mean", "min", "sum") }),
)
const _PanelFields = {
  axes: Schema.optionalWith(Schema.Array(_Axis), { default: () => [] }),
  description: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  interaction: Schema.optionalWith(_Interaction, { default: () => ({ brush: false, tooltip: "multi" as const, zoom: true }) }),
  links: Schema.optionalWith(Schema.Array(_Link), { default: () => [] }),
  repeat: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  span: _Span,
  title: Schema.NonEmptyString,
  transformations: Schema.optionalWith(Schema.Array(_Transform), { default: () => [] }),
  transparent: Schema.optionalWith(Schema.Boolean, { default: () => false }),
} as const

const Timeseries = Schema.TaggedStruct("Timeseries", {
  ..._PanelFields,
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  steps: Schema.Array(_Threshold),
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Stat = Schema.TaggedStruct("Stat", {
  ..._PanelFields,
  expr: Schema.String,
  steps: Schema.Array(_Threshold),
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Gauge = Schema.TaggedStruct("Gauge", {
  ..._PanelFields,
  ceiling: Schema.Number,
  expr: Schema.String,
  steps: Schema.Array(_Threshold),
})
const Heatmap = Schema.TaggedStruct("Heatmap", {
  ..._PanelFields,
  color: Schema.optionalWith(Schema.Literal("continuous", "diverging", "opacity", "scheme"), { default: () => "scheme" as const }),
  expr: Schema.String,
  scale: Schema.optionalWith(Schema.Literal("exponential", "linear", "symlog"), { default: () => "linear" as const }),
  unit: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})
const Logs = Schema.TaggedStruct("Logs", {
  ..._PanelFields,
  deduplicate: Schema.optionalWith(Schema.Literal("exact", "none", "numbers", "signature"), { default: () => "none" as const }),
  filter: Schema.String,
  order: Schema.optionalWith(Schema.Literal("ascending", "descending"), { default: () => "descending" as const }),
  showTime: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  wrap: Schema.optionalWith(Schema.Boolean, { default: () => true }),
})
const Table = Schema.TaggedStruct("Table", {
  ..._PanelFields,
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  sort: Schema.optionalWith(Schema.Struct({ descending: Schema.Boolean, field: Schema.NonEmptyString }), { as: "Option" }),
})
const Geomap = Schema.TaggedStruct("Geomap", {
  ..._PanelFields,
  expr: Schema.String,
  mapping: Schema.Struct({
    color: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    label: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    latitude: Schema.NonEmptyString,
    longitude: Schema.NonEmptyString,
    weight: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
})
const Nodes = Schema.TaggedStruct("Nodes", {
  ..._PanelFields,
  edges: Schema.String,
  mapping: Schema.Struct({
    edgeSource: Schema.NonEmptyString,
    edgeTarget: Schema.NonEmptyString,
    nodeColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeId: Schema.NonEmptyString,
    nodeLabel: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    nodeWeight: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
  nodes: Schema.String,
})
const Panel: Schema.Union<[
  typeof Timeseries,
  typeof Stat,
  typeof Gauge,
  typeof Heatmap,
  typeof Logs,
  typeof Table,
  typeof Geomap,
  typeof Nodes,
]> = Schema.Union(Timeseries, Stat, Gauge, Heatmap, Logs, Table, Geomap, Nodes)
type Panel = typeof Panel.Type
```

## [04]-[MODEL]

- Owner: `DashboardModel` — uid (a slug brand derived, never supplied), `title`, the identity record (the same `Convention.identity` projection every signal stamps, so a dashboard is greppable by the attributes its panels query), `tags`, the tenant template `variables` row, `annotations` derived from `slo#ALERT_SPECS` specs (slug and tone), the `refresh` cadence and `since` range defaults (emission facts with owner-fixed defaults, so `iac` reads them off the encoded model), and the `panels` array.
- Law: `DashboardModel.of(identity, page)` is the ONLY page-level constructor consumers touch — uid derives as `${identity.app}-${page.slug}` through the slug refinement, the tenant variable row is always present (a single-tenant app pins it), and the identity attributes stamp automatically — so every dashboard in existence is a total function of `AppIdentity` and a per-app fork has no authoring surface.
- Law: emission is the derived twin — `typeof DashboardModel.Encoded` and the class's own `Schema.encode` are what `iac` consumes and applies through its grafana provider; a grafana-sdk admission lands as one interior emission member behind this same encode seam, changing no consumer.
- Law: model-level fields mirror the Foundation-SDK `DashboardBuilder` members one-for-one — `uid`/`title`/`tags`/`refresh` land on the builder members of the same name, `since` on `time`, `variables` on `withVariable`, `annotations` on `annotation`, `laid` positions on each panel's `gridPos` — so the iac compile leg types every knob and dashboard identity survives from `AppIdentity` into the Grafana state unchanged.
- Law: statics carry the derivations — `DashboardModel.laid`, the panel union with every row schema riding `DashboardModel`, and the pack dispatch, so one import serves model, panels, rows, layout, and packs, and a consumer constructs rows by name, never by union position.
- Entry: `DashboardModel.of(identity, page)`; `DashboardModel.laid(model)` at the apply seam.
- Growth: a new dashboard-level axis is one field with its default in the field declaration, inherited by every pack through `of`.
- Packages: `effect` (`Schema`, `Array`, `Duration`, `Number`); `value/identity` (`AppIdentity`); `convention#IDENTITY_PROJECTION`.

```typescript signature
const _Uid = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(40), Schema.brand("DashboardUid"))

const _Annotation = Schema.Struct({ slug: Schema.NonEmptyString, tone: Schema.NonEmptyString })
const _Variable = Schema.Struct({ label: Schema.NonEmptyString, name: Schema.NonEmptyString })

type LiveMetric = Data.TaggedEnum<{
  Counter: { readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Frequency: { readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly values: ReadonlyMap<string, number> }
  Gauge: { readonly labels: Convention.Bag; readonly name: Convention.MetricName; readonly value: number | bigint }
  Histogram: { readonly buckets: ReadonlyArray<readonly [number, number]>; readonly count: number; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly sum: number }
  Summary: { readonly count: number; readonly error: number; readonly labels: Convention.Bag; readonly max: number; readonly min: number; readonly name: Convention.MetricName; readonly quantiles: ReadonlyArray<readonly [number, Option.Option<number>]>; readonly sum: number }
  Unknown: { readonly labels: Convention.Bag; readonly name: Convention.MetricName }
}>
const _LiveMetric = Data.taggedEnum<LiveMetric>()
const _metricNames: ReadonlyArray<Convention.MetricName> = Record.values(Convention.metric)
const _isMetricName = (name: string): name is Convention.MetricName => Array.some(_metricNames, (metric) => metric === name)
const _live = (pair: MetricPair.MetricPair.Untyped): Option.Option<LiveMetric> =>
  Option.map(Option.liftPredicate(pair.metricKey.name, _isMetricName), (name) => {
    const labels: Convention.Bag = Record.fromEntries(Array.map(pair.metricKey.tags, (tag) => [tag.key, tag.value] as const))
    return Match.value(pair.metricState).pipe(
      Match.when(MetricState.isCounterState, (state) => _LiveMetric.Counter({ labels, name, value: state.count })),
      Match.when(MetricState.isFrequencyState, (state) => _LiveMetric.Frequency({ labels, name, values: state.occurrences })),
      Match.when(MetricState.isGaugeState, (state) => _LiveMetric.Gauge({ labels, name, value: state.value })),
      Match.when(MetricState.isHistogramState, (state) =>
        _LiveMetric.Histogram({ buckets: state.buckets, count: state.count, labels, max: state.max, min: state.min, name, sum: state.sum })),
      Match.when(MetricState.isSummaryState, (state) =>
        _LiveMetric.Summary({ count: state.count, error: state.error, labels, max: state.max, min: state.min, name, quantiles: state.quantiles, sum: state.sum })),
      Match.orElse(() => _LiveMetric.Unknown({ labels, name })),
    )
  })

class DashboardModel extends Schema.Class<DashboardModel>("DashboardModel")({
  annotations: Schema.Array(_Annotation),
  identity: Schema.Record({ key: Schema.String, value: Schema.String }),
  panels: Schema.Array(Panel),
  refresh: Schema.optionalWith(Schema.DurationFromMillis, { default: () => Duration.seconds(30) }),
  since: Schema.optionalWith(Schema.DurationFromMillis, { default: () => Duration.hours(6) }),
  tags: Schema.Array(Schema.NonEmptyString),
  title: Schema.NonEmptyString,
  uid: _Uid,
  variables: Schema.Array(_Variable),
}) {
  static readonly Gauge = Gauge
  static readonly Geomap = Geomap
  static readonly Heatmap = Heatmap
  static readonly Logs = Logs
  static readonly Nodes = Nodes
  static readonly Panel = Panel
  static readonly Stat = Stat
  static readonly Table = Table
  static readonly Timeseries = Timeseries
  static readonly of = (identity: AppIdentity, page: DashboardModel.Page): DashboardModel =>
    new DashboardModel({
      annotations: page.annotations,
      identity: Convention.identity(identity),
      panels: page.panels,
      tags: [identity.app, ...page.tags],
      title: `${identity.app} ${page.title}`,
      uid: _Uid.make(`${identity.app}-${page.slug}`),
      variables: [{ label: "Tenant", name: "tenant" }, ...page.variables],
    })
  static readonly laid = (model: DashboardModel): ReadonlyArray<DashboardModel.Placed> =>
    Array.mapAccum(model.panels, { shelf: 0, x: 0, y: 0 }, (cursor, panel) => {
      const wraps = cursor.x + panel.span.w > 24
      const x = wraps ? 0 : cursor.x
      const y = wraps ? cursor.y + cursor.shelf : cursor.y
      return [
        { shelf: wraps ? panel.span.h : Number.max(cursor.shelf, panel.span.h), x: x + panel.span.w, y },
        { panel, position: { h: panel.span.h, w: panel.span.w, x, y } },
      ]
    })[1]
  static readonly snapshot: Effect.Effect<ReadonlyArray<LiveMetric>> = Effect.map(Metric.snapshot, (pairs) => Array.filterMap(pairs, _live))
  static readonly pack = <K extends DashboardModel.Pack>(
    kind: K,
    identity: AppIdentity,
    payload: DashboardModel.Payload[K],
  ): DashboardModel => _PACKS[kind](identity, payload)
  static readonly suite = (identity: AppIdentity, payload: DashboardModel.Suite): ReadonlyArray<DashboardModel> =>
    Array.map(Struct.keys(_SUITE), (kind) => _SUITE[kind](identity, payload))
}

declare namespace DashboardModel {
  type Page = {
    readonly annotations: ReadonlyArray<typeof _Annotation.Type>
    readonly panels: ReadonlyArray<Panel>
    readonly slug: string
    readonly tags: ReadonlyArray<string>
    readonly title: string
    readonly variables: ReadonlyArray<typeof _Variable.Type>
  }
  type Placed = { readonly panel: Panel; readonly position: { readonly h: number; readonly w: number; readonly x: number; readonly y: number } }
  type Wire = typeof DashboardModel.Encoded
  type Signal = LiveMetric
}
```

## [05]-[BENCH]

- Owner: the benchmark comparison algebra — `Bench`, the structural claim vocabulary (`Band`, the percentile ladder; `Metric`, the modality-labeled unit row; `Claim`, the suite-plus-host-print shape) and the pure baseline-versus-candidate fold `Bench.graded` yielding the `Graded`/`Refused` verdict family under one tolerance policy row.
- Law: the claim shape composes `mitata`'s state-free `stats` type, never the harness or an interchange import — `Band` selects its percentile ladder and `Metric.kind` selects its modality while this plane types the exact contextual fields it grades (`suite`, `host.print`, `label`, and `unit`), so the codec's decoded `Claim` conforms by construction and a second benchmark vocabulary beside the package shape is unspellable.
- Law: incompatible comparison is refused, never computed — `_ADMISSION` orders suite, host print, metric kind-label-unit roster, finite non-negative band values, and a strictly-positive graded band on both claims as data rows, and `Refused` carries the first failed axis and both projections, so a gate never compares unrelated suites, changes modality or units, accepts duplicate rows, divides by a zero baseline, grades a zero-measurement candidate as improvement, or mistakes a partial join for a complete grade.
- Law: the grade is a tolerance policy row — `_TOLERANCE` names the graded band (`p99`) and admits its slack through the finite `[0, 1)` `_Slack` constructor, the admitted per-kind-label-unit join is total over both rosters, and the three-grade vocabulary (`improved`/`steady`/`regressed`) is the closed union a gate reads; a caller wanting a different band or slack passes one admitted row, never a second fold.
- Law: verdicts feed the bridge, not the panels — the runtime meter bridge mints `Convention.metric.benchVerdicts` from the graded rows and the `bench` pack trends that series, so the board view and the gate view of one comparison are provably the same fold output.
- Growth: a new grade is one `_GRADES` entry every exhaustive consumer breaks on; a new comparison axis (a second banded field) is one `Tolerance` field.
- Packages: `effect` (`Array`, `Data`, `Number`, `Option`, `Schema`); `mitata` (`stats` type only — percentile ladder and modality, never the module-global harness).

```typescript signature
const _BANDS = ["min", "avg", "p25", "p50", "p75", "p99", "p999", "max"] as const
const _GRADES = ["improved", "steady", "regressed"] as const
const _BandValue = Schema.Number.pipe(Schema.finite(), Schema.nonNegative())
const _isBandValue = Schema.is(_BandValue)
type _Slack = typeof _Slack.Type
const _Slack = Schema.Number.pipe(Schema.finite(), Schema.nonNegative(), Schema.lessThan(1), Schema.brand("BenchSlack"))

declare namespace Bench {
  type Band = Pick<MitataStats, (typeof _BANDS)[number]>
  type Metric = { readonly kind: MitataStats["kind"]; readonly label: string; readonly unit: string; readonly band: Band }
  type Claim = { readonly suite: string; readonly host: { readonly print: string }; readonly metrics: ReadonlyArray<Metric> }
  type Grade = (typeof _GRADES)[number]
  type Row = { readonly kind: Metric["kind"]; readonly label: string; readonly unit: string; readonly grade: Grade; readonly ratio: number }
  type RefusalAxis = "suite" | "host" | "metrics" | "bands" | "baseline"
  type Verdict = Data.TaggedEnum<{
    Graded: { readonly suite: string; readonly print: string; readonly rows: ReadonlyArray<Row> }
    Refused: { readonly suite: string; readonly axis: RefusalAxis; readonly baseline: string; readonly candidate: string }
  }>
  type Tolerance = { readonly band: (typeof _BANDS)[number]; readonly slack: _Slack }
}
const _Verdict = Data.taggedEnum<Bench.Verdict>()

const _TOLERANCE: Bench.Tolerance = { band: "p99", slack: _Slack.make(0.05) }

const _sameMetric = (left: Bench.Metric, right: Bench.Metric): boolean =>
  left.kind === right.kind && left.label === right.label && left.unit === right.unit

const _roster = (claim: Bench.Claim): string =>
  Array.join(Array.map(claim.metrics, ({ kind, label, unit }) => `${kind}:${label}[${unit}]`), ",")

const _bandValues = (claim: Bench.Claim): string =>
  Array.join(
    Array.flatMap(claim.metrics, (metric) =>
      Array.map(_BANDS, (band) => `${metric.kind}:${metric.label}[${metric.unit}]:${band}=${metric.band[band]}`)),
    ",",
  )

const _aligned = (baseline: Bench.Claim, candidate: Bench.Claim): boolean =>
  baseline.metrics.length === candidate.metrics.length
  && Array.every(baseline.metrics, (metric) => Array.filter(candidate.metrics, (held) => _sameMetric(metric, held)).length === 1)
  && Array.every(candidate.metrics, (metric) => Array.filter(baseline.metrics, (held) => _sameMetric(metric, held)).length === 1)

type _Admission = {
  readonly accepts: (baseline: Bench.Claim, candidate: Bench.Claim, tolerance: Bench.Tolerance) => boolean
  readonly axis: Bench.RefusalAxis
  readonly project: (claim: Bench.Claim) => string
}

const _ADMISSION: ReadonlyArray<_Admission> = [
  { axis: "suite", accepts: (baseline, candidate) => baseline.suite === candidate.suite, project: (claim) => claim.suite },
  { axis: "host", accepts: (baseline, candidate) => baseline.host.print === candidate.host.print, project: (claim) => claim.host.print },
  { axis: "metrics", accepts: _aligned, project: _roster },
  {
    axis: "bands",
    accepts: (baseline, candidate) =>
      Array.every([baseline, candidate], (claim) =>
        Array.every(claim.metrics, (metric) => Array.every(_BANDS, (band) => _isBandValue(metric.band[band])))),
    project: _bandValues,
  },
  {
    // The graded band must be strictly positive on BOTH claims: a zero baseline divides the ratio
    // and a zero candidate is a measurement artifact that would grade as a phantom improvement.
    axis: "baseline",
    accepts: (baseline, candidate, tolerance) =>
      Array.every([baseline, candidate], (claim) => Array.every(claim.metrics, (metric) => metric.band[tolerance.band] > 0)),
    project: _roster,
  },
]

const _graded = (baseline: Bench.Claim, candidate: Bench.Claim, tolerance: Bench.Tolerance = _TOLERANCE): Bench.Verdict =>
  pipe(
    Array.findFirst(_ADMISSION, (row) => !row.accepts(baseline, candidate, tolerance)),
    Option.match({
      onSome: (row) => _Verdict.Refused({
        suite: candidate.suite,
        axis: row.axis,
        baseline: row.project(baseline),
        candidate: row.project(candidate),
      }),
      onNone: () => _Verdict.Graded({
        suite: candidate.suite,
        print: candidate.host.print,
        rows: Array.filterMap(candidate.metrics, (metric) =>
          Option.map(Array.findFirst(baseline.metrics, (row) => _sameMetric(metric, row)), (held) => {
            const ratio = metric.band[tolerance.band] / held.band[tolerance.band]
            return {
              kind: metric.kind,
              label: metric.label,
              unit: metric.unit,
              ratio,
              grade: ratio > 1 + tolerance.slack
                ? ("regressed" as const)
                : ratio < 1 - tolerance.slack
                  ? ("improved" as const)
                  : ("steady" as const),
            }
          })),
      }),
    }),
  )

const Bench: Data.TaggedEnum.Constructor<Bench.Verdict> & {
  readonly bands: typeof _BANDS
  readonly grades: typeof _GRADES
  readonly graded: (baseline: Bench.Claim, candidate: Bench.Claim, tolerance?: Bench.Tolerance) => Bench.Verdict
  readonly slack: typeof _Slack.make
} = { ..._Verdict, bands: _BANDS, grades: _GRADES, graded: _graded, slack: _Slack.make }
```

## [06]-[PACKS]

- Owner: the interior `_pane` builders and the `_PACKS` handler record dispatched by `DashboardModel.pack` — the payload map types each pack's input, the mapped handler contract turns a missing pack into a compile error at the record, and the one generic indexed dispatch keeps the payload following the kind.
- Law: a builder never invents a name, a threshold, or a tone — series come from `Convention.metric` rows, tenancy filters from `Convention.rasm` keys against the `$tenant` template variable, vital ceilings and meter resource axes from the caller's payload rows, burn thresholds from the spec's own `factor`, threshold tones from the spec's own severity row (`Alert.severity.page.tone` where a panel gates with no spec) — so the builders are pure plumbing between vocabulary and visualization, a severity-to-tone table re-declared here is the hand-synced parallel the derivation law kills, and deleting any hardcoded literal from them leaves nothing to delete.
- Law: payloads carry the later-wave vocabulary IN — the runtime vital owner passes its budget rows as `gauges`, the meter owner its resource axis as `resources`, the app its objectives — so this floor renders domains it never imports, the dependency arrow stays strictly upward, and a vocabulary change upstream re-renders through the payload with zero edits here.
- Law: every pack routes through `DashboardModel.of` — identity-derived uid, stamped identity attributes, the always-present tenant variable — so the pack layer cannot mint an identity-free dashboard; the `slo` pack folds `Alert.of(objective)` specs into burn panels and annotation rows, making the alert and dashboard views of one objective provably the same data.
- Law: the burn panel renders the WHOLE discipline — `_breach(sli, window)` compiles the SLI's own breach predicate through one `Match.valueTags` record dispatch into an error-rate expression (`Latency` as the `le`-share complement at the spec's `ceiling`, `Ratio` as the good-ratio complement, `Saturation` and `Freshness` as bool-comparison time shares), `_burnPair` divides it by the objective's budget for BOTH the long and the short window as two series on one panel, the row's `factor` lands as the panel's threshold step, and the derived `spend` prints in the panel title — so the panel shows exactly the two-window condition `slo#BURN_ROWS` legislates, the `Latency` `ceiling` has its render-side consumer, and the budget-share figure the operator reads is the spec's own derived field, never a re-computation.
- Law: the audit pack queries the `Convention` audit family — the action-rate series grouped by `rasm.audit.action` and the actor/action table over `rasm.audit.actor.kind`, both over the `rasm.fact.drained` fact stream — so the audit signal domain has a standing board projection beside slo/vital/meter/crash.
- Law: the invoke pack is the capability plane's RED projection — outcome rates grouped by the `Exit`-fold vocabulary rows (`rasm.invoke.outcome`, `rasm.gateway.outcome`), the fault-reason frequency grouped by its `key` occurrence label, and duration quantiles on both directions, all over the `Convention` invoke/gateway rows with no tenant filter because the capability instruments are process-level — so the branch's hottest surface ships a standing dashboard the moment `invoke#CAPABILITY_BIND` and `invoke#COMMAND_GATEWAY` land their instruments, and the outcome-rate and quantile builders are one parameterized pair, never a builder per plane.
- Law: the work pack is the durable-work health board `convention#RASM_ROWS` legislates — outbox/queue depth and redelivery instants, oldest-age stat, relay-drain and parked rates by `rasm.work.channel`, lane checkpoints by `rasm.lane.name`, derivative pressure, and batch-window quantiles — every series the runtime meter bridge mints from journal facts and census probes, tenant-free because work-plane instruments are process-level, while every dispute settles against the journal.
- Law: the vital pack pairs each payload gauge with the observation stream — per-kind level gauges beside the `rasm.vital.observed` rate grouped by the kind and grade axes — so both vital instruments land on one board.
- Law: the security pack projects the authenticity-reject stream — the rate series grouped by `rasm.security.kind` and the facet table over the kind/dialect/surface/reason axes — completing the reject vocabulary's receipt-to-board chain beside audit and crash.
- Law: the crash pack groups its own attribute vocabulary — the fingerprint table over `rasm.crash.kind`/`rasm.crash.fingerprint` beside the capture-rate stat and the exception log stream — so the crash axes have board consumers, never declaration-only rows.
- Law: the object pack is the content-addressed plane's health board — write outcomes grouped by `rasm.object.outcome`, the landed-bytes and resumable-upload flow pair, and the sweep-reclaim rate — every series the data object owners tap from receipts, tenant-free because the object instruments are process-level, while every dispute settles against the receipt.
- Law: the lake pack is the storage-harvest board — admission-wait and deferred-wait quantiles, harvested engine-profile quantiles, the retried rate by `rasm.olap.engine`, the cache hit-share expression grouped by `rasm.cache.name`, and the pool-lease instant by `rasm.pool.scheme` — so the lake-engine profile parity and cache/pool census the data lanes mint read on one standing board.
- Law: the bench pack trends the claim bridge — the `rasm.bench.band` timing ladder per payload suite, one generated enrichment panel per GC/heap/hardware-counter unit family, and the verdict rate grouped by `rasm.bench.verdict` — the meter-bridged projection of `[05]`'s fold, so a regression is a threshold-visible line the same fold gates on and incompatible units never share one axis.
- Law: `DashboardModel.suite(identity, payload)` folds the mapped `_SUITE` record, whose key contract is exactly `DashboardModel.Pack`; a new pack cannot compile until its suite projection lands, and the standing fleet never requires a hand-maintained array roster.
- Law: spans are the builders' only local decision — each pane declares its grid `span` so the model's shelf fold lays every pack without per-pack layout code; a reusable visualization earns a builder at two pack call sites, else it inlines.
- Boundary: provider emission — grafana JSON, folder placement, apply lifecycle — is `iac`'s seam over `typeof DashboardModel.Encoded`; delivery of alert specs is `slo#ALERT_SPECS`'s consumer law.
- Entry: `DashboardModel.pack(kind, identity, payload)`; `DashboardModel.suite(identity, payload)`.
- Growth: a new dashboard family is one payload row with its handler row; every consumer inherits it through the derived kind union.

```typescript signature
const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = Query.span(Duration.minutes(5))
const _DAY = Query.span(Duration.hours(24))

const _seconds = (span: Duration.Duration): Query.Finite => Query.finite(Duration.toMillis(span) / 1000)

const _rated = (metric: Convention.MetricName<"counter" | "histogram">, labels: Query.Labels, window: Query.Window): Query =>
  Query.Aggregate({ by: [], of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels, metric }), window }), op: "sum" })

const _breach = (sli: Sli, window: Query.Window, labels: Query.Labels): Query => {
  const span = typeof window === "string" ? window : Query.span(window)
  return Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) =>
      Query.Windowed({
        fn: "avg",
        of: Query.Binary({ left: Query.Instant({ labels, metric }), op: "gt", right: Query.Const({ value: _seconds(horizon) }) }),
        window: span,
      }),
    Latency: ({ ceiling, metric }) =>
      Query.Binary({
        left: Query.Const({ value: Query.finite(1) }),
        op: "sub",
        right: Query.Binary({
          left: _rated(metric, { ...labels, le: `${Convention.duration(metric, ceiling)}` }, span),
          op: "div",
          right: _rated(metric, { ...labels, le: "+Inf" }, span),
        }),
      }),
    Ratio: ({ good, total }) =>
      Query.Binary({
        left: Query.Const({ value: Query.finite(1) }),
        op: "sub",
        right: Query.Binary({ left: _rated(good, labels, span), op: "div", right: _rated(total, labels, span) }),
      }),
    Saturation: ({ ceiling, metric }) =>
      Query.Windowed({
        fn: "avg",
        of: Query.Binary({ left: Query.Instant({ labels, metric }), op: "gt", right: Query.Const({ value: Query.finite(ceiling) }) }),
        window: span,
      }),
  })
}

const _burned = (spec: Alert.Spec, labels: Query.Labels): Query => {
  const threshold = Query.Const({ value: Query.finite(spec.factor * (1 - spec.target)) })
  const exceeds = (window: Duration.Duration): Query =>
    Query.Binary({ left: _breach(spec.sli, Query.span(window), labels), op: "gt", right: threshold })
  return Query.Binary({ left: exceeds(spec.windows.short), op: "and", right: exceeds(spec.windows.long) })
}

const _quantile = (row: { readonly labels: Query.Labels; readonly metric: Convention.MetricName<"histogram">; readonly title: string; readonly unit: string }) =>
  (quantile: Query.QuantileValue): Panel =>
    Timeseries.make({
      exprs: [Query.render(Query.Quantile({ labels: row.labels, metric: row.metric, q: quantile, window: _WINDOW }))],
      legend: Option.none(),
      span: { h: 8, w: 12 },
      steps: [],
      title: `${row.title} p${Number.round(quantile * 100, 0)}`,
      unit: Option.some(row.unit),
    })

const _latency = _quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, title: "latency", unit: "s" }) // the semconv duration histogram is seconds; a ms label mislabels every quantile by three decades
const _invokeLatency = _quantile({ labels: {}, metric: Convention.metric.invokeDuration, title: "invoke", unit: "ms" })    // the capability instruments are process-level: no tenant tag exists on their series
const _gatewayLatency = _quantile({ labels: {}, metric: Convention.metric.gatewayDuration, title: "gateway", unit: "ms" })
const _batchLatency = _quantile({ labels: {}, metric: Convention.metric.batchDuration, title: "batch window", unit: "ms" })

const _outcomes = (metric: Convention.MetricName, axis: Convention.Key | Query.Dialect, title: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [axis],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric }), window: _WINDOW }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${axis}}}`),
    span: { h: 8, w: 12 },
    steps: [],
    title,
    unit: Option.none(),
  })

const _vitalGauge = (gauge: { readonly ceiling: number; readonly kind: string }): Panel =>
  Gauge.make({
    ceiling: gauge.ceiling,
    expr: Query.render(
      Query.Windowed({
        fn: "avg",
        of: Query.Instant({ labels: { [Convention.rasm.vitalKind]: gauge.kind }, metric: Convention.metric.vitalLevel }),
        window: _WINDOW,
      }),
    ),
    span: { h: 6, w: 4 },
    steps: [{ at: gauge.ceiling, tone: Alert.severity.page.tone }], // the paging tone reads slo's own severity table: no tone correspondence is re-declared here
    title: gauge.kind,
  })

const _vitalGrades: Panel =
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.vitalKind, Convention.rasm.vitalGrade],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.vitalObserved }), window: _WINDOW }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.vitalKind}}} {{${Convention.rasm.vitalGrade}}}`),
    span: { h: 8, w: 24 },
    steps: [],
    title: "observations by grade",
    unit: Option.none(),
  })

const _usage = (resource: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.tenant],
          of: Query.Windowed({
            fn: "increase",
            of: Query.Instant({ labels: { [Convention.rasm.meterResource]: resource, ..._tenant }, metric: Convention.metric.meterUsage }),
            window: _WINDOW,
          }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.tenant}}}`),
    span: { h: 8, w: 12 },
    steps: [],
    title: `usage ${resource}`,
    unit: Option.none(),
  })

const _auditRate: Panel =
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.auditAction],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: _tenant, metric: Convention.metric.factDrained }), window: _WINDOW }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.auditAction}}}`),
    span: { h: 8, w: 16 },
    steps: [],
    title: "audit actions",
    unit: Option.none(),
  })

const _auditActors: Panel =
  Table.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.auditActorKind, Convention.rasm.auditAction],
          of: Query.Windowed({ fn: "increase", of: Query.Instant({ labels: _tenant, metric: Convention.metric.factDrained }), window: _DAY }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.none(),
    span: { h: 8, w: 8 },
    title: "actors by action",
  })

const _crashes: Panel =
  Logs.make({
    filter: Convention.event.exception,
    span: { h: 8, w: 24 },
    title: "exception records",
  })

const _crashRate: Panel =
  Stat.make({
    expr: Query.render(Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.crashCaptured }), window: _WINDOW })),
    span: { h: 6, w: 6 },
    steps: [],
    title: "crash capture rate",
    unit: Option.none(),
  })

const _crashFingerprints: Panel =
  Table.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.crashKind, Convention.rasm.crashFingerprint],
          of: Query.Windowed({ fn: "increase", of: Query.Instant({ labels: {}, metric: Convention.metric.crashCaptured }), window: _DAY }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.none(),
    span: { h: 8, w: 18 },
    title: "fingerprints by class",
  })

const _workDepth: Panel =
  Timeseries.make({
    exprs: [
      Query.render(Query.Instant({ labels: {}, metric: Convention.metric.outboxDepth })),
      Query.render(Query.Instant({ labels: {}, metric: Convention.metric.queueDepth })),
      Query.render(Query.Instant({ labels: {}, metric: Convention.metric.outboxRedelivered })), // redelivery rides the depth panel: a rising claimed-twice line against depth is the stall signature
    ],
    legend: Option.none(),
    span: { h: 8, w: 12 },
    steps: [],
    title: "outbox and queue depth",
    unit: Option.none(),
  })

const _workFlow: Panel =
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.workChannel],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.relayDrained }), window: _WINDOW }),
          op: "sum",
        }),
      ),
      Query.render(
        Query.Aggregate({
          by: [],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.queueParked }), window: _WINDOW }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.workChannel}}}`),
    span: { h: 8, w: 12 },
    steps: [],
    title: "relay drain and parked",
    unit: Option.none(),
  })

const _workAge: Panel =
  Stat.make({
    expr: Query.render(Query.Aggregate({ by: [], of: Query.Instant({ labels: {}, metric: Convention.metric.outboxAge }), op: "max" })),
    span: { h: 4, w: 6 },
    steps: [],
    title: "oldest undelivered age",
    unit: Option.some("s"),
  })

const _laneProgress: Panel =
  Timeseries.make({
    exprs: [Query.render(Query.Instant({ labels: {}, metric: Convention.metric.laneCheckpoint }))],
    legend: Option.some(`{{${Convention.rasm.laneName}}}`),
    span: { h: 6, w: 9 },
    steps: [],
    title: "lane checkpoints",
    unit: Option.none(),
  })

const _derivativePressure: Panel =
  Timeseries.make({
    exprs: [
      Query.render(Query.Instant({ labels: {}, metric: Convention.metric.derivativeActive })),
      Query.render(Query.Instant({ labels: {}, metric: Convention.metric.derivativeQueued })),
    ],
    legend: Option.none(),
    span: { h: 6, w: 9 },
    steps: [],
    title: "derivative pressure",
    unit: Option.none(),
  })

const _securityRejects: Panel =
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.securityKind],
          of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.securityRejects }), window: _WINDOW }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.securityKind}}}`),
    span: { h: 8, w: 14 },
    steps: [],
    title: "authenticity rejects",
    unit: Option.none(),
  })

const _securityFacets: Panel =
  Table.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.securityKind, Convention.rasm.securityDialect, Convention.rasm.securitySurface, Convention.rasm.securityReason],
          of: Query.Windowed({ fn: "increase", of: Query.Instant({ labels: {}, metric: Convention.metric.securityRejects }), window: _DAY }),
          op: "sum",
        }),
      ),
    ],
    legend: Option.none(),
    span: { h: 8, w: 10 },
    title: "rejects by facet",
  })

const _objectOutcomes: Panel = _outcomes(Convention.metric.objectWritten, Convention.rasm.objectOutcome, "writes by outcome")

const _objectFlow: Panel =
  Timeseries.make({
    exprs: [
      Query.render(Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.objectBytes }), window: _WINDOW })),
      Query.render(Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.streamBytes }), window: _WINDOW })),
      Query.render(Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric: Convention.metric.objectReclaimed }), window: _WINDOW })),
    ],
    legend: Option.none(),
    span: { h: 8, w: 12 },
    steps: [],
    title: "landed, uploaded, reclaimed",
    unit: Option.some("By"),
  })

const _lakeWait = _quantile({ labels: {}, metric: Convention.metric.olapWait, title: "lake wait", unit: "ms" })
const _lakeDeferred = _quantile({ labels: {}, metric: Convention.metric.olapDeferred, title: "deferred wait", unit: "ms" })
const _lakeProfile = _quantile({ labels: {}, metric: Convention.metric.profileDuration, title: "engine profile", unit: "ms" })

const _cacheShare: Panel =
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Binary({
          left: Query.Instant({ labels: {}, metric: Convention.metric.cacheHits }),
          op: "div",
          right: Query.Binary({
            left: Query.Instant({ labels: {}, metric: Convention.metric.cacheHits }),
            op: "add",
            right: Query.Instant({ labels: {}, metric: Convention.metric.cacheMisses }),
          }),
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.cacheName}}}`),
    span: { h: 6, w: 12 },
    steps: [],
    title: "cache hit share",
    unit: Option.none(),
  })

const _poolLeases: Panel =
  Timeseries.make({
    exprs: [Query.render(Query.Instant({ labels: {}, metric: Convention.metric.poolHeld }))],
    legend: Option.some(`{{${Convention.rasm.poolScheme}}}`),
    span: { h: 6, w: 12 },
    steps: [],
    title: "pool leases held",
    unit: Option.none(),
  })

const _benchLadder = (suite: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Aggregate({
          by: [Convention.rasm.benchBand, Convention.rasm.benchLabel],
          of: Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: Convention.metric.benchTime }),
          op: "max",
        }),
      ),
    ],
    legend: Option.some(`{{${Convention.rasm.benchLabel}}} {{${Convention.rasm.benchBand}}}`),
    span: { h: 8, w: 12 },
    steps: [],
    title: `${suite} timing ladder`,
    unit: Option.some("ns"),
  })

const _BENCH_ENRICHMENT = [
  { metric: Convention.metric.benchGc, title: "gc timing", unit: "ns" },
  { metric: Convention.metric.benchHeap, title: "heap delta", unit: "By" },
  { metric: Convention.metric.benchCounter, title: "hardware counters", unit: "1" },
] as const

const _benchEnrichment = (suite: string, row: (typeof _BENCH_ENRICHMENT)[number]): Panel =>
  Timeseries.make({
    exprs: [Query.render(Query.Instant({ labels: { [Convention.rasm.benchSuite]: suite }, metric: row.metric }))],
    legend: Option.some(`{{${Convention.rasm.benchLabel}}} {{${Convention.rasm.benchBand}}}`),
    span: { h: 8, w: 12 },
    steps: [],
    title: `${suite} ${row.title}`,
    unit: Option.some(row.unit),
  })

const _benchVerdicts: Panel = _outcomes(Convention.metric.benchVerdicts, Convention.rasm.benchVerdict, "regression verdicts")

const _burnPair = (spec: Alert.Spec): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(Query.Binary({ left: Query.breach(spec.sli, Query.span(spec.windows.long), _tenant), op: "div", right: Query.Const({ value: Query.finite(1 - spec.target) }) })),
      Query.render(Query.Binary({ left: Query.breach(spec.sli, Query.span(spec.windows.short), _tenant), op: "div", right: Query.Const({ value: Query.finite(1 - spec.target) }) })),
    ],
    legend: Option.none(),
    span: { h: 6, w: 12 },
    steps: [{ at: spec.factor, tone: spec.severity.tone }],
    title: `${spec.slug} trips at ${spec.factor}x — ${Number.round(spec.spend * 100, 1)}% budget`, // the derived spend prints here: the human figure cannot drift from the row that fires it
    unit: Option.none(),
  })

declare namespace DashboardModel {
  type Pack = keyof Payload
  type Payload = {
    readonly audit: Record.ReadonlyRecord<never, never>
    readonly bench: { readonly suites: ReadonlyArray<string> }
    readonly crash: Record.ReadonlyRecord<never, never>
    readonly invoke: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly lake: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly meter: { readonly resources: ReadonlyArray<string> }
    readonly object: Record.ReadonlyRecord<never, never>
    readonly overview: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
    readonly security: Record.ReadonlyRecord<never, never>
    readonly slo: { readonly objectives: ReadonlyArray<Slo.Objective> }
    readonly vital: { readonly gauges: ReadonlyArray<{ readonly ceiling: number; readonly kind: string }> }
    readonly work: { readonly quantiles: ReadonlyArray<Query.QuantileValue> }
  }
  type Suite = Payload["bench"] & Payload["meter"] & Payload["overview"] & Payload["slo"] & Payload["vital"]
}

const _PACKS: { readonly [K in DashboardModel.Pack]: (identity: AppIdentity, payload: DashboardModel.Payload[K]) => DashboardModel } = {
  audit: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_auditRate, _auditActors],
      slug: "audit",
      tags: ["audit"],
      title: "audit",
      variables: [],
    }),
  bench: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [
        ...Array.flatMap(payload.suites, (suite) => [
          _benchLadder(suite),
          ...Array.map(_BENCH_ENRICHMENT, (row) => _benchEnrichment(suite, row)),
        ]),
        _benchVerdicts,
      ],
      slug: "bench",
      tags: ["bench"],
      title: "benchmarks",
      variables: [],
    }),
  crash: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_crashRate, _crashFingerprints, _crashes],
      slug: "crash",
      tags: ["crash"],
      title: "crash",
      variables: [],
    }),
  invoke: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [
        _outcomes(Convention.metric.invokeCalls, Convention.rasm.invokeOutcome, "invoke outcomes"),
        _outcomes(Convention.metric.gatewayCommands, Convention.rasm.gatewayOutcome, "gateway outcomes"),
        _outcomes(Convention.metric.invokeFault, "key", "fault reasons"), // the frequency export mints the reason as its `key` occurrence label
        ...Array.map(payload.quantiles, _invokeLatency),
        ...Array.map(payload.quantiles, _gatewayLatency),
      ],
      slug: "invoke",
      tags: ["invoke", "capability"],
      title: "capability plane",
      variables: [],
    }),
  lake: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [
        ...Array.map(payload.quantiles, _lakeWait),
        ...Array.map(payload.quantiles, _lakeDeferred),
        ...Array.map(payload.quantiles, _lakeProfile),
        _outcomes(Convention.metric.olapRetried, Convention.rasm.olapEngine, "queries retried"),
        _cacheShare,
        _poolLeases,
      ],
      slug: "lake",
      tags: ["lake", "storage"],
      title: "storage harvest",
      variables: [],
    }),
  meter: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: Array.map(payload.resources, _usage),
      slug: "meter",
      tags: ["meter", "billing"],
      title: "usage",
      variables: [],
    }),
  object: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_objectOutcomes, _objectFlow],
      slug: "object",
      tags: ["object", "storage"],
      title: "object plane",
      variables: [],
    }),
  overview: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: Array.map(payload.quantiles, _latency),
      slug: "overview",
      tags: ["overview"],
      title: "service overview",
      variables: [],
    }),
  security: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_securityRejects, _securityFacets],
      slug: "security",
      tags: ["security"],
      title: "authenticity",
      variables: [],
    }),
  slo: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: Array.flatMap(payload.objectives, (objective) =>
        Array.map(Alert.of(objective), (spec) => ({ slug: spec.slug, tone: spec.severity.tone }))),
      panels: Array.flatMap(payload.objectives, (objective) => Array.map(Alert.of(objective), _burnPair)),
      slug: "slo",
      tags: ["slo"],
      title: "objectives",
      variables: [],
    }),
  vital: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [...Array.map(payload.gauges, _vitalGauge), _vitalGrades],
      slug: "vital",
      tags: ["vital", "rum"],
      title: "web vitals",
      variables: [],
    }),
  work: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_workDepth, _workFlow, _workAge, _laneProgress, _derivativePressure, ...Array.map(payload.quantiles, _batchLatency)],
      slug: "work",
      tags: ["work", "durable"],
      title: "durable work",
      variables: [],
    }),
}

const _SUITE: { readonly [K in DashboardModel.Pack]: (identity: AppIdentity, payload: DashboardModel.Suite) => DashboardModel } = {
  audit: (identity) => _PACKS.audit(identity, {}),
  bench: (identity, payload) => _PACKS.bench(identity, { suites: payload.suites }),
  crash: (identity) => _PACKS.crash(identity, {}),
  invoke: (identity, payload) => _PACKS.invoke(identity, { quantiles: payload.quantiles }),
  lake: (identity, payload) => _PACKS.lake(identity, { quantiles: payload.quantiles }),
  meter: (identity, payload) => _PACKS.meter(identity, { resources: payload.resources }),
  object: (identity) => _PACKS.object(identity, {}),
  overview: (identity, payload) => _PACKS.overview(identity, { quantiles: payload.quantiles }),
  security: (identity) => _PACKS.security(identity, {}),
  slo: (identity, payload) => _PACKS.slo(identity, { objectives: payload.objectives }),
  vital: (identity, payload) => _PACKS.vital(identity, { gauges: payload.gauges }),
  work: (identity, payload) => _PACKS.work(identity, { quantiles: payload.quantiles }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bench, DashboardModel, Query }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
