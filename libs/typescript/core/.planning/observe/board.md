# [CORE_BOARD]

Dashboards are data derived from identity, and the pack library is the same owner's dispatch: `DashboardModel` is one `Schema.Class` — deterministic uid slugged from the `AppIdentity`, a closed panel family as tagged rows, template variables, alert annotations, refresh and range defaults, the shelf-layout fold, and the pack record dispatched through one generic indexed call — and `Query` is the typed expression algebra panels are built from, a closed recursive family whose aggregation operators, windowed functions, and binary operators are vocabulary rows rather than named cases, rendered to the metric-backend dialect by one fold, so no panel ever carries a hand-assembled query string. The model is the emission contract: it encodes through its own schema (`typeof DashboardModel.Encoded` is what `iac` applies through its grafana provider), panels carry the threshold steps, legends, and units a provider row needs so `iac` invents nothing, and every pack is a total function from `AppIdentity` plus a typed payload — the later-wave signal owners hand their vocabulary rows IN as payload data, so this floor renders thresholds it never declares and a threshold, metric name, or burn-row change upstream re-renders every affected pack with zero edits here. A per-app dashboard file is unspellable: the only way to obtain a dashboard is a total function of identity. The module is `core/src/observe/board.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                  |
| :-----: | :-------- | :-------------------------------------------------------------------------- |
|  [01]   | `QUERY`   | the typed expression family, its fn/op/agg vocabularies, the one dialect-render fold |
|  [02]   | `PANEL`   | the closed panel row family and the shelf-layout grid fold                  |
|  [03]   | `MODEL`   | the `DashboardModel` owner: identity-derived uid, variables, annotations    |
|  [04]   | `PACKS`   | the pane builders, the payload map, the pack record, dispatch, and suite    |

## [2]-[QUERY]

[QUERY]:
- Owner: the `Query` closed family — `Instant` (a labeled series selector), `Windowed` (a range function over an operand: `rate`, `increase`, `delta`, `avg_over_time`, `max_over_time`, `min_over_time` as `_FNS` vocabulary rows), `Quantile` (histogram quantile over a windowed rate, fused because it carries the `le`-aggregation contract), `Aggregate` (an aggregation operator with a group-by label set: `sum`, `avg`, `min`, `max`, `count` as `_AGG` rows), `Binary` (arithmetic and bool-comparison operators as `_OPS` rows), and `Const` (a scalar literal) — recursive where composition demands it, with `Query.render` as the one total fold to the PromQL-dialect string. A function or operator is a vocabulary row, never a case: the case set is the GRAMMAR of the dialect, the row sets are its function space, so the algebra generates the expression space instead of enumerating it.
- Law: series names are `Convention.MetricName` rows and the group-by axis is `Convention.Key` rows — the algebra admits no free-string metric or grouping label, so a dashboard query cannot reference a series or axis the plane does not emit; filter label keys spell as `Convention` rows at every call site, the label-value pair set renders as the selector body, and the tenant template variable enters as an ordinary label value (`$tenant`). The histogram `le` label is the one dialect-owned key admitted beside the rows, because it belongs to the backend's bucket contract, not the emission plane.
- Law: windows are `Duration` values rendered by the interior `_span` projection — a dialect window string is never authored.
- Law: `Windowed` renders by operand shape — a selector operand takes the range form `fn(selector[w])`, any composed operand takes the subquery form `fn((expr)[w:])` — so time-share expressions (`avg_over_time` of a bool comparison) compose from the same rows and no builder hand-writes subquery syntax.
- Law: the rendered dialect is Prometheus UTF-8 — every selector emits the quoted `{"metric.name","label.key"="value"}` form and every grouping key quotes, because dotted OTel names are not legacy-PromQL identifiers; the quantile arm aggregates the windowed rate `by (le)` before `histogram_quantile`, the histogram-series contract the backend demands.
- Law: one dialect fold — a second backend dialect is a second render fold over the SAME family, never a second family; the expression data is dialect-free by construction.
- Law: the render fold IS the dialect's codegen output — PromQL is a single-line dialect whose rendered string is byte-load-bearing (quoted UTF-8 selector identity), so a document-assembly layer (`@effect/printer` `Doc`/`encloseSep`) is rejected: layout grouping and reflow forge selector spelling, and the closed family already owns every arm.
- Entry: constructors ride the family (`Query.Windowed({ fn: "rate", of, window })`), `Query.render(query)` at pack-build time.
- Law: the fn/op/agg vocabularies stay interior — `_FNS`/`_OPS` are `as const satisfies` row tables no export reaches, their unions derive as the interior `_Fn`/`_Op`/`_Agg` aliases the case fields consume, and consumers speak literals the fields already type; the `type`-plus-`const` pair is the family's whole public spelling.
- Growth: a new function or operator is one `_FNS`/`_OPS`/`_AGG` row; a new grammar shape is one case plus one render arm the compiler enforces at the fold; an arity-bearing aggregation (`topk`) lands as an optional field on the `Aggregate` case, never a new case.
- Packages: `effect` (`Data`, `Duration`, `Number`, `Record`); `convention#RASM_ROWS` (`Convention` rows).

```typescript
import { Array, Data, Duration, Number, Option, Record, Schema, pipe } from "effect"
import type { AppIdentity } from "../value/identity.ts"
import { Convention } from "./convention.ts"
import { Alert, Sli, type Slo } from "./slo.ts"

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
  div: "/",
  gt: "> bool",
  lt: "< bool",
  mul: "*",
  sub: "-",
} as const satisfies Record<string, string>

const _AGG = ["avg", "count", "max", "min", "sum"] as const

type _Agg = (typeof _AGG)[number]
type _Fn = keyof typeof _FNS
type _Op = keyof typeof _OPS

type Query = Data.TaggedEnum<{
  Aggregate: { readonly by: ReadonlyArray<Convention.Key>; readonly of: Query; readonly op: _Agg }
  Binary: { readonly left: Query; readonly op: _Op; readonly right: Query }
  Const: { readonly value: number }
  Instant: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly q: number; readonly window: Duration.Duration }
  Windowed: { readonly fn: _Fn; readonly of: Query; readonly window: Duration.Duration }
}>
const _Query = Data.taggedEnum<Query>()

const _span = (window: Duration.Duration): string =>
  pipe(Duration.toMillis(window), (millis) => `${Number.round(millis / 1000, 0)}s`)

const _selector = (metric: Convention.MetricName, labels: Convention.Attributes): string =>
  pipe(
    Record.collect(labels, (key, value) => `"${key}"="${String(value)}"`),
    (pairs) => `{"${metric}"${pairs.length === 0 ? "" : `,${pairs.join(",")}`}}`,
  )

const _render = (query: Query): string =>
  _Query.$match(query, {
    Aggregate: ({ by, of, op }) =>
      `${op}${by.length === 0 ? "" : ` by (${Array.map(by, (key) => `"${key}"`).join(",")})`} (${_render(of)})`,
    Binary: ({ left, op, right }) => `(${_render(left)}) ${_OPS[op]} (${_render(right)})`,
    Const: ({ value }) => `${value}`,
    Instant: ({ labels, metric }) => _selector(metric, labels),
    Quantile: ({ labels, metric, q, window }) =>
      `histogram_quantile(${q}, sum by (le) (rate(${_selector(metric, labels)}[${_span(window)}])))`,
    Windowed: ({ fn, of, window }) =>
      of._tag === "Instant" ? `${_FNS[fn]}(${_render(of)}[${_span(window)}])` : `${_FNS[fn]}((${_render(of)})[${_span(window)}:])`,
  })

const Query: Data.TaggedEnum.Constructor<Query> & { readonly render: (query: Query) => string } = {
  ..._Query,
  render: _render,
}
```

## [3]-[PANEL]

[PANEL]:
- Owner: the closed panel family — `Timeseries`, `Stat`, `Gauge`, `Heatmap`, `Logs`, `Table`, `Geomap`, `Nodes` as `Schema.TaggedStruct` rows joined by one `Schema.Union` — every row carrying `title`, its rendered `expr` set, its `span` (grid units of a 24-column row), and its row-specific evidence: `ceiling` and threshold `steps` on gauges, `steps` and `unit` on stats and timeseries, `legend` on the series-bearing rows, `filter` on logs, the paired `nodes`/`edges` queries on the node-graph row.
- Law: panels store RENDERED expressions — `Query` is the build-time algebra, the panel is the emission-ready datum — so the model serializes completely and the query family never needs a schema twin.
- Law: rows are emission-complete — threshold steps, legend format, and unit are semantic panel facts declared here so `iac` maps rows to provider fields verbatim and invents nothing; the datasource binding, folder placement, and apply lifecycle stay provider facts on `iac`'s side of the seam.
- Law: `Geomap` and `Nodes` are the spatial and relational rows the BIM/geo and dependency planes fill through later-wave payloads — a geo-features pack or an element-graph pack is one pack row over these existing panel rows, never a panel family fork.
- Law: layout derives — `DashboardModel.laid(model)` is a `mapAccum` shelf fold assigning `{ x, y, w, h }` positions across the 24-column grid from each panel's `span`, wrapping when a shelf overflows and advancing by the tallest panel on the shelf; a hand-positioned panel does not exist, and a layout change is a fold change applied to every dashboard at once.
- Growth: a new visualization kind is one tagged row plus its arm in consumers' emission folds.
- Packages: `effect` (`Schema`).

```typescript
const _Span = Schema.Struct({
  h: Schema.Int.pipe(Schema.between(2, 24)),
  w: Schema.Int.pipe(Schema.between(2, 24)),
})

const _Threshold = Schema.Struct({ at: Schema.Number, tone: Schema.NonEmptyString })

const Timeseries = Schema.TaggedStruct("Timeseries", {
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  span: _Span,
  steps: Schema.Array(_Threshold),
  title: Schema.NonEmptyString,
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Stat = Schema.TaggedStruct("Stat", {
  expr: Schema.String,
  span: _Span,
  steps: Schema.Array(_Threshold),
  title: Schema.NonEmptyString,
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Gauge = Schema.TaggedStruct("Gauge", {
  ceiling: Schema.Number,
  expr: Schema.String,
  span: _Span,
  steps: Schema.Array(_Threshold),
  title: Schema.NonEmptyString,
})
const Heatmap = Schema.TaggedStruct("Heatmap", { expr: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Logs = Schema.TaggedStruct("Logs", { filter: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Table = Schema.TaggedStruct("Table", {
  exprs: Schema.NonEmptyArray(Schema.String),
  legend: Schema.optionalWith(Schema.String, { as: "Option" }),
  span: _Span,
  title: Schema.NonEmptyString,
})
const Geomap = Schema.TaggedStruct("Geomap", { expr: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Nodes = Schema.TaggedStruct("Nodes", {
  edges: Schema.String,
  nodes: Schema.String,
  span: _Span,
  title: Schema.NonEmptyString,
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

## [4]-[MODEL]

[MODEL]:
- Owner: `DashboardModel` — uid (a slug brand derived, never supplied), `title`, the identity record (the same `Convention.identity` projection every signal stamps, so a dashboard is greppable by the attributes its panels query), `tags`, the tenant template `variables` row, `annotations` derived from `slo#ALERT_SPECS` specs (slug plus tone), the `refresh` cadence and `since` range defaults (emission facts with owner-fixed defaults, so `iac` reads them off the encoded model), and the `panels` array.
- Law: `DashboardModel.of(identity, page)` is the ONLY page-level constructor consumers touch — uid derives as `${identity.app}-${page.slug}` through the slug refinement, the tenant variable row is always present (a single-tenant app simply pins it), and the identity attributes stamp automatically — so every dashboard in existence is a total function of `AppIdentity` and a per-app fork has no authoring surface.
- Law: emission is the derived twin — `typeof DashboardModel.Encoded` and the class's own `Schema.encode` are what `iac` consumes and applies through its grafana provider; a grafana-sdk admission lands as one interior emission member behind this same encode seam, changing no consumer.
- Law: statics carry the derivations — `DashboardModel.laid` (the grid fold), the panel union as `DashboardModel.Panel` with every row schema riding the same owner (`DashboardModel.Timeseries`, `.Stat`, `.Gauge`, `.Heatmap`, `.Logs`, `.Table`, `.Geomap`, `.Nodes`), and the `[05]` pack dispatch — so one import serves model, panels, rows, layout, and packs, and a consumer constructs rows by name, never by union position.
- Entry: `DashboardModel.of(identity, page)`; `DashboardModel.laid(model)` at the apply seam.
- Growth: a new dashboard-level axis is one field with its default in the field declaration, inherited by every pack through `of`.
- Packages: `effect` (`Schema`, `Array`, `Duration`, `Number`); `value/identity` (`AppIdentity`); `convention#IDENTITY_PROJECTION`.

```typescript
const _Uid = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(40), Schema.brand("DashboardUid"))

const _Annotation = Schema.Struct({ slug: Schema.NonEmptyString, tone: Schema.NonEmptyString })
const _Variable = Schema.Struct({ label: Schema.NonEmptyString, name: Schema.NonEmptyString })

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
  static readonly pack = <K extends DashboardModel.Pack>(
    kind: K,
    identity: AppIdentity,
    payload: DashboardModel.Payload[K],
  ): DashboardModel => _PACKS[kind](identity, payload)
  static readonly suite = (identity: AppIdentity, payload: DashboardModel.Suite): ReadonlyArray<DashboardModel> => [
    _PACKS.overview(identity, { quantiles: payload.quantiles }),
    _PACKS.invoke(identity, { quantiles: payload.quantiles }),
    _PACKS.slo(identity, { objectives: payload.objectives }),
    _PACKS.vital(identity, { gauges: payload.gauges }),
    _PACKS.meter(identity, { resources: payload.resources }),
    _PACKS.audit(identity, {}),
    _PACKS.crash(identity, {}),
  ]
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
}
```

## [5]-[PACKS]

[PACKS]:
- Owner: the interior `_pane` builders and the `_PACKS` handler record dispatched by `DashboardModel.pack` — the payload map types each pack's input, the mapped handler contract turns a missing pack into a compile error at the record, and the one generic indexed dispatch keeps the payload following the kind.
- Law: a builder never invents a name, a threshold, or a tone — series come from `Convention.metric` rows, tenancy filters from `Convention.rasm` keys against the `$tenant` template variable, vital ceilings and meter resource axes from the caller's payload rows, burn thresholds from the spec's own `factor`, threshold tones from the spec's own severity row (`Alert.severity.page.tone` where a panel gates with no spec) — so the builders are pure plumbing between vocabulary and visualization, a severity-to-tone table re-declared here would be the hand-synced parallel the derivation law kills, and deleting any hardcoded literal from them leaves nothing to delete.
- Law: payloads carry the later-wave vocabulary IN — the runtime vital owner passes its budget rows as `gauges`, the meter owner its resource axis as `resources`, the app its objectives — so this floor renders domains it never imports, the dependency arrow stays strictly upward, and a vocabulary change upstream re-renders through the payload with zero edits here.
- Law: every pack routes through `DashboardModel.of` — identity-derived uid, stamped identity attributes, the always-present tenant variable — so the pack layer cannot mint an identity-free dashboard; the `slo` pack folds `Alert.of(objective)` specs into burn panels and annotation rows, making the alert and dashboard views of one objective provably the same data.
- Law: the burn panel renders the WHOLE discipline — `_breach(sli, window)` compiles the SLI's own breach predicate into an error-rate expression (`Latency` as the `le`-share complement at the spec's `ceiling`, `Ratio` as the good-ratio complement, `Saturation` and `Freshness` as bool-comparison time shares), `_burnPair` divides it by the objective's budget for BOTH the long and the short window as two series on one panel, and the row's `factor` lands as the panel's threshold step — so the panel shows exactly the two-window condition `slo#BURN_ROWS` legislates and the `Latency` `ceiling` has its render-side consumer.
- Law: the audit pack queries the `Convention` audit family — the action-rate series grouped by `rasm.audit.action` and the actor/action table over `rasm.audit.actor.kind`, both over the `rasm.fact.drained` fact stream — so the audit signal domain has a standing board projection beside slo/vital/meter/crash.
- Law: the invoke pack is the capability plane's RED projection — outcome rates grouped by the `Exit`-fold vocabulary rows (`rasm.invoke.outcome`, `rasm.gateway.outcome`) and duration quantiles on both directions, all over the `Convention` invoke/gateway rows with no tenant filter because the capability instruments are process-level — so the branch's hottest surface ships a standing dashboard the moment `invoke#CAPABILITY_BIND` and `invoke#COMMAND_GATEWAY` land their instruments, and the outcome-rate and quantile builders are one parameterized pair, never a builder per plane.
- Law: `DashboardModel.suite(identity, payload)` derives the standing fleet — every pack whose payload the suite input carries — the one call an app's deploy program makes; `iac` applies `DashboardModel.laid` projections of exactly this suite.
- Law: spans are the builders' only local decision — each pane declares its grid `span` so the model's shelf fold lays every pack without per-pack layout code; a reusable visualization earns a builder at two pack call sites, else it inlines.
- Boundary: provider emission — grafana JSON, folder placement, apply lifecycle — is `iac`'s seam over `typeof DashboardModel.Encoded`; delivery of alert specs is `slo#ALERT_SPECS`'s consumer law.
- Entry: `DashboardModel.pack(kind, identity, payload)`; `DashboardModel.suite(identity, payload)`.
- Growth: a new dashboard family is one payload row plus one handler row; every consumer inherits it through the derived kind union.

```typescript
const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = Duration.minutes(5)
const _DAY = Duration.hours(24)

const _seconds = (span: Duration.Duration): number => Duration.toMillis(span) / 1000

const _rated = (metric: Convention.MetricName, labels: Convention.Attributes, window: Duration.Duration): Query =>
  Query.Aggregate({ by: [], of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels, metric }), window }), op: "sum" })

const _breach = (sli: Sli, window: Duration.Duration): Query =>
  Sli.$match(sli, {
    Freshness: ({ horizon, metric }) =>
      Query.Windowed({
        fn: "avg",
        of: Query.Binary({ left: Query.Instant({ labels: _tenant, metric }), op: "gt", right: Query.Const({ value: _seconds(horizon) }) }),
        window,
      }),
    Latency: ({ ceiling, metric }) =>
      Query.Binary({
        left: Query.Const({ value: 1 }),
        op: "sub",
        right: Query.Binary({
          left: _rated(metric, { ..._tenant, le: `${_seconds(ceiling)}` }, window),
          op: "div",
          right: _rated(metric, { ..._tenant, le: "+Inf" }, window),
        }),
      }),
    Ratio: ({ good, total }) =>
      Query.Binary({
        left: Query.Const({ value: 1 }),
        op: "sub",
        right: Query.Binary({ left: _rated(good, _tenant, window), op: "div", right: _rated(total, _tenant, window) }),
      }),
    Saturation: ({ ceiling, metric }) =>
      Query.Windowed({
        fn: "avg",
        of: Query.Binary({ left: Query.Instant({ labels: _tenant, metric }), op: "gt", right: Query.Const({ value: ceiling }) }),
        window,
      }),
  })

const _quantile = (row: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly title: string; readonly unit: string }) =>
  (quantile: number): Panel =>
    Timeseries.make({
      exprs: [Query.render(Query.Quantile({ labels: row.labels, metric: row.metric, q: quantile, window: _WINDOW }))],
      legend: Option.none(),
      span: { h: 8, w: 12 },
      steps: [],
      title: `${row.title} p${Number.round(quantile * 100, 0)}`,
      unit: Option.some(row.unit),
    })

const _latency = _quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, title: "latency", unit: "s" }) // the semconv duration histogram is seconds; a ms label would mislabel every quantile by three decades
const _invokeLatency = _quantile({ labels: {}, metric: Convention.metric.invokeDuration, title: "invoke", unit: "ms" })    // the capability instruments are process-level: no tenant tag exists on their series
const _gatewayLatency = _quantile({ labels: {}, metric: Convention.metric.gatewayDuration, title: "gateway", unit: "ms" })

const _outcomes = (metric: Convention.MetricName, axis: Convention.Key, title: string): Panel =>
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
    filter: Convention.metric.crashCaptured,
    span: { h: 8, w: 24 },
    title: "crash captures",
  })

const _burnPair = (spec: Alert.Spec): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(Query.Binary({ left: _breach(spec.sli, Duration.decode(spec.windows.long)), op: "div", right: Query.Const({ value: 1 - spec.target }) })),
      Query.render(Query.Binary({ left: _breach(spec.sli, Duration.decode(spec.windows.short)), op: "div", right: Query.Const({ value: 1 - spec.target }) })),
    ],
    legend: Option.none(),
    span: { h: 6, w: 12 },
    steps: [{ at: spec.factor, tone: spec.severity.tone }],
    title: `${spec.slug} trips at ${spec.factor}x budget`,
    unit: Option.none(),
  })

declare namespace DashboardModel {
  type Pack = keyof Payload
  type Payload = {
    readonly audit: Record.ReadonlyRecord<never, never>
    readonly crash: Record.ReadonlyRecord<never, never>
    readonly invoke: { readonly quantiles: ReadonlyArray<number> }
    readonly meter: { readonly resources: ReadonlyArray<string> }
    readonly overview: { readonly quantiles: ReadonlyArray<number> }
    readonly slo: { readonly objectives: ReadonlyArray<Slo.Objective> }
    readonly vital: { readonly gauges: ReadonlyArray<{ readonly ceiling: number; readonly kind: string }> }
  }
  type Suite = Payload["meter"] & Payload["overview"] & Payload["slo"] & Payload["vital"]
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
  crash: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_crashes],
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
        ...Array.map(payload.quantiles, _invokeLatency),
        ...Array.map(payload.quantiles, _gatewayLatency),
      ],
      slug: "invoke",
      tags: ["invoke", "capability"],
      title: "capability plane",
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
  overview: (identity, payload) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: Array.map(payload.quantiles, _latency),
      slug: "overview",
      tags: ["overview"],
      title: "service overview",
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
      panels: Array.map(payload.gauges, _vitalGauge),
      slug: "vital",
      tags: ["vital", "rum"],
      title: "web vitals",
      variables: [],
    }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { DashboardModel, Query }
```
