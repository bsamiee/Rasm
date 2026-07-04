# [CORE_BOARD]

Dashboards are data derived from identity, and the pack library is the same owner's dispatch: `DashboardModel` is one `Schema.Class` — deterministic uid slugged from the `AppIdentity`, a closed panel family as tagged rows, template variables, alert annotations, the shelf-layout fold, and the pack record dispatched through one generic indexed call — and `Query` is the typed expression algebra panels are built from, a closed recursive family rendered to the metric-backend dialect by one fold, so no panel ever carries a hand-assembled query string. The model is the emission contract: it encodes through its own schema (`typeof DashboardModel.Encoded` is what `iac` applies through its grafana provider), and every pack is a total function from `AppIdentity` plus a typed payload — the later-wave signal owners hand their vocabulary rows IN as payload data, so this floor renders thresholds it never declares and a threshold, metric name, or burn-row change upstream re-renders every affected pack with zero edits here. A per-app dashboard file is unspellable: the only way to obtain a dashboard is a total function of identity. The module is `core/src/observe/board.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                  |
| :-----: | :-------- | :-------------------------------------------------------------------------- |
|  [01]   | `QUERY`   | the typed expression family and the one dialect-render fold                 |
|  [02]   | `PANEL`   | the closed panel row family and the shelf-layout grid fold                  |
|  [03]   | `MODEL`   | the `DashboardModel` owner: identity-derived uid, variables, annotations    |
|  [04]   | `PACKS`   | the pane builders, the payload map, the pack record, dispatch, and suite    |

## [2]-[QUERY]

[QUERY]:
- Owner: the `Query` closed family — `Instant` (a labeled series selector), `Rate` (per-second rate over a window), `Sum` (aggregation with a group-by label set), `Ratio` (one expression over another), `Quantile` (histogram quantile over a windowed rate) — recursive where composition demands it (`Sum`/`Ratio` hold `Query` operands), with `Query.render` as the one total fold to the PromQL-dialect string.
- Law: series names are `Convention.MetricName` rows and the group-by axis is `Convention.Key` rows — the algebra admits no free-string metric or grouping label, so a dashboard query cannot reference a series or axis the plane does not emit; filter label keys spell as `Convention` rows at every call site, the label-value pair set renders as the selector body, and the tenant template variable enters as an ordinary label value (`$tenant`).
- Law: windows are `Duration` values rendered by the interior `_span` projection — a dialect window string is never authored.
- Law: the rendered dialect is Prometheus UTF-8 — every selector emits the quoted `{"metric.name","label.key"="value"}` form and every grouping key quotes, because dotted OTel names are not legacy-PromQL identifiers; the quantile arm aggregates the windowed rate `by (le)` before `histogram_quantile`, the histogram-series contract the backend demands.
- Law: one dialect fold — a second backend dialect is a second render fold over the SAME family, never a second family; the expression data is dialect-free by construction.
- Entry: constructors ride the family (`Query.Rate({ metric, window, labels })`), `Query.render(query)` at pack-build time.
- Growth: a new expression shape is one case plus one render arm — the compiler enforces the arm at the fold.
- Packages: `effect` (`Data`, `Duration`, `Record`); `convention#RASM_ROWS` (`Convention` rows).

```typescript
import { Array, Data, Duration, Option, Record, Schema, pipe } from "effect"
import type { AppIdentity } from "../value/identity.ts"
import { Convention } from "./convention.ts"
import { Alert, Sli, type Slo } from "./slo.ts"

type Query = Data.TaggedEnum<{
  Instant: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly q: number; readonly window: Duration.Duration }
  Rate: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly window: Duration.Duration }
  Ratio: { readonly good: Query; readonly total: Query }
  Sum: { readonly by: ReadonlyArray<Convention.Key>; readonly of: Query }
}>
const _Query = Data.taggedEnum<Query>()

const _span = (window: Duration.Duration): string =>
  pipe(Duration.toMillis(window), (millis) => `${Math.round(millis / 1000)}s`)

const _selector = (metric: Convention.MetricName, labels: Convention.Attributes): string =>
  pipe(
    Record.collect(labels, (key, value) => `"${key}"="${String(value)}"`),
    (pairs) => `{"${metric}"${pairs.length === 0 ? "" : `,${pairs.join(",")}`}}`,
  )

const _render = (query: Query): string =>
  _Query.$match(query, {
    Instant: ({ labels, metric }) => _selector(metric, labels),
    Quantile: ({ labels, metric, q, window }) =>
      `histogram_quantile(${q}, sum by (le) (rate(${_selector(metric, labels)}[${_span(window)}])))`,
    Rate: ({ labels, metric, window }) => `rate(${_selector(metric, labels)}[${_span(window)}])`,
    Ratio: ({ good, total }) => `(${_render(good)}) / (${_render(total)})`,
    Sum: ({ by, of }) => `sum by (${Array.map(by, (key) => `"${key}"`).join(",")}) (${_render(of)})`,
  })

const Query: Data.TaggedEnum.Constructor<Query> & { readonly render: (query: Query) => string } = {
  ..._Query,
  render: _render,
}
```

## [3]-[PANEL]

[PANEL]:
- Owner: the closed panel family — `Timeseries`, `Stat`, `Gauge`, `Heatmap`, `Logs`, `Table` as `Schema.TaggedStruct` rows joined by one `Schema.Union` — every row carrying `title`, its rendered `expr` set, its `span` (grid units of a 24-column row), and its row-specific evidence (`ceiling` on gauges, `filter` on logs).
- Law: panels store RENDERED expressions — `Query` is the build-time algebra, the panel is the emission-ready datum — so the model serializes completely and the query family never needs a schema twin.
- Law: layout derives — `DashboardModel.laid(model)` is a `mapAccum` shelf fold assigning `{ x, y, w, h }` positions across the 24-column grid from each panel's `span`, wrapping when a shelf overflows and advancing by the tallest panel on the shelf; a hand-positioned panel does not exist, and a layout change is a fold change applied to every dashboard at once.
- Growth: a new visualization kind is one tagged row plus its arm in consumers' emission folds.
- Packages: `effect` (`Schema`).

```typescript
const _Span = Schema.Struct({
  h: Schema.Int.pipe(Schema.between(2, 24)),
  w: Schema.Int.pipe(Schema.between(2, 24)),
})

const Timeseries = Schema.TaggedStruct("Timeseries", {
  exprs: Schema.NonEmptyArray(Schema.String),
  span: _Span,
  title: Schema.NonEmptyString,
  unit: Schema.optionalWith(Schema.String, { as: "Option" }),
})
const Stat = Schema.TaggedStruct("Stat", { expr: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Gauge = Schema.TaggedStruct("Gauge", {
  ceiling: Schema.Number,
  expr: Schema.String,
  span: _Span,
  title: Schema.NonEmptyString,
})
const Heatmap = Schema.TaggedStruct("Heatmap", { expr: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Logs = Schema.TaggedStruct("Logs", { filter: Schema.String, span: _Span, title: Schema.NonEmptyString })
const Table = Schema.TaggedStruct("Table", {
  exprs: Schema.NonEmptyArray(Schema.String),
  span: _Span,
  title: Schema.NonEmptyString,
})

const Panel: Schema.Union<[typeof Timeseries, typeof Stat, typeof Gauge, typeof Heatmap, typeof Logs, typeof Table]> =
  Schema.Union(Timeseries, Stat, Gauge, Heatmap, Logs, Table)
type Panel = typeof Panel.Type
```

## [4]-[MODEL]

[MODEL]:
- Owner: `DashboardModel` — uid (a slug brand derived, never supplied), `title`, the identity record (the same `Convention.identity` projection every signal stamps, so a dashboard is greppable by the attributes its panels query), `tags`, the tenant template `variables` row, `annotations` derived from `slo#ALERT_SPECS` specs (slug plus tone), and the `panels` array.
- Law: `DashboardModel.of(identity, page)` is the ONLY page-level constructor consumers touch — uid derives as `${identity.app}-${page.slug}` through the slug refinement, the tenant variable row is always present (a single-tenant app simply pins it), and the identity attributes stamp automatically — so every dashboard in existence is a total function of `AppIdentity` and a per-app fork has no authoring surface.
- Law: emission is the derived twin — `typeof DashboardModel.Encoded` and the class's own `Schema.encode` are what `iac` consumes and applies through its grafana provider; a grafana-sdk admission lands as one interior emission member behind this same encode seam, changing no consumer.
- Law: statics carry the derivations — `DashboardModel.laid` (the grid fold), the panel union as `DashboardModel.Panel` with every row schema riding the same owner (`DashboardModel.Timeseries`, `.Stat`, `.Gauge`, `.Heatmap`, `.Logs`, `.Table`), and the `[05]` pack dispatch — so one import serves model, panels, rows, layout, and packs, and a consumer constructs rows by name, never by union position.
- Entry: `DashboardModel.of(identity, page)`; `DashboardModel.laid(model)` at the apply seam.
- Growth: a new dashboard-level axis (a refresh cadence, a time-range default) is one field with its default in `of`.
- Packages: `effect` (`Schema`, `Array`); `value/identity` (`AppIdentity`); `convention#IDENTITY_PROJECTION`.

```typescript
const _Uid = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]*$/), Schema.maxLength(40), Schema.brand("DashboardUid"))

const _Annotation = Schema.Struct({ slug: Schema.NonEmptyString, tone: Schema.NonEmptyString })
const _Variable = Schema.Struct({ label: Schema.NonEmptyString, name: Schema.NonEmptyString })

class DashboardModel extends Schema.Class<DashboardModel>("DashboardModel")({
  annotations: Schema.Array(_Annotation),
  identity: Schema.Record({ key: Schema.String, value: Schema.String }),
  panels: Schema.Array(Panel),
  tags: Schema.Array(Schema.NonEmptyString),
  title: Schema.NonEmptyString,
  uid: _Uid,
  variables: Schema.Array(_Variable),
}) {
  static readonly Gauge = Gauge
  static readonly Heatmap = Heatmap
  static readonly Logs = Logs
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
        { shelf: wraps ? panel.span.h : Math.max(cursor.shelf, panel.span.h), x: x + panel.span.w, y },
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
    _PACKS.slo(identity, { objectives: payload.objectives }),
    _PACKS.vital(identity, { gauges: payload.gauges }),
    _PACKS.meter(identity, { resources: payload.resources }),
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
- Law: a builder never invents a name or a threshold — series come from `Convention.metric` rows, tenancy filters from `Convention.rasm` keys against the `$tenant` template variable, vital ceilings and meter resource axes from the caller's payload rows, burn thresholds from the spec's own `factor` — so the builders are pure plumbing between vocabulary and visualization, and deleting any hardcoded literal from them leaves nothing to delete.
- Law: payloads carry the later-wave vocabulary IN — the runtime vital owner passes its budget rows as `gauges`, the meter owner its resource axis as `resources`, the app its objectives — so this floor renders domains it never imports, the dependency arrow stays strictly upward, and a vocabulary change upstream re-renders through the payload with zero edits here.
- Law: every pack routes through `DashboardModel.of` — identity-derived uid, stamped identity attributes, the always-present tenant variable — so the pack layer cannot mint an identity-free dashboard; the `slo` pack folds `Alert.of(objective)` specs into burn panels and annotation rows, making the alert and dashboard views of one objective provably the same data.
- Law: `DashboardModel.suite(identity, payload)` derives the standing fleet — every pack whose payload the suite input carries — the one call an app's deploy program makes; `iac` applies `DashboardModel.laid` projections of exactly this suite.
- Law: spans are the builders' only local decision — each pane declares its grid `span` so the model's shelf fold lays every pack without per-pack layout code; a reusable visualization earns a builder at two pack call sites, else it inlines.
- Boundary: provider emission — grafana JSON, folder placement, apply lifecycle — is `iac`'s seam over `typeof DashboardModel.Encoded`; delivery of alert specs is `slo#ALERT_SPECS`'s consumer law.
- Entry: `DashboardModel.pack(kind, identity, payload)`; `DashboardModel.suite(identity, payload)`.
- Growth: a new dashboard family is one payload row plus one handler row; every consumer inherits it through the derived kind union.

```typescript
const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = Duration.minutes(5)

const _latency = (quantile: number): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(Query.Quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, q: quantile, window: _WINDOW })),
    ],
    span: { h: 8, w: 12 },
    title: `latency p${Math.round(quantile * 100)}`,
    unit: Option.some("ms"),
  })

const _vitalGauge = (gauge: { readonly ceiling: number; readonly kind: string }): Panel =>
  Gauge.make({
    ceiling: gauge.ceiling,
    expr: Query.render(Query.Instant({ labels: { [Convention.rasm.vitalKind]: gauge.kind }, metric: Convention.metric.vitalLevel })),
    span: { h: 6, w: 4 },
    title: gauge.kind,
  })

const _usage = (resource: string): Panel =>
  Timeseries.make({
    exprs: [
      Query.render(
        Query.Sum({
          by: [Convention.rasm.tenant],
          of: Query.Rate({ labels: { [Convention.rasm.meterResource]: resource, ..._tenant }, metric: Convention.metric.meterUsage, window: _WINDOW }),
        }),
      ),
    ],
    span: { h: 8, w: 12 },
    title: `usage ${resource}`,
    unit: Option.none(),
  })

const _crashes = (): Panel =>
  Logs.make({
    filter: Convention.metric.crashCaptured,
    span: { h: 8, w: 24 },
    title: "crash captures",
  })

const _sliExpr = (sli: Sli, window: Duration.Duration): string =>
  Sli.$match(sli, {
    Latency: ({ metric, quantile }) => Query.render(Query.Quantile({ labels: _tenant, metric, q: quantile, window })),
    Ratio: ({ good, total }) =>
      Query.render(
        Query.Ratio({
          good: Query.Rate({ labels: _tenant, metric: good, window }),
          total: Query.Rate({ labels: _tenant, metric: total, window }),
        }),
      ),
  })

const _burnPair = (spec: Alert.Spec): Panel =>
  Stat.make({
    expr: _sliExpr(spec.sli, Duration.decode(spec.windows.long)),
    span: { h: 4, w: 6 },
    title: `${spec.slug} trips at ${spec.factor}x budget`,
  })

declare namespace DashboardModel {
  type Pack = keyof Payload
  type Payload = {
    readonly crash: Record.ReadonlyRecord<never, never>
    readonly meter: { readonly resources: ReadonlyArray<string> }
    readonly overview: { readonly quantiles: ReadonlyArray<number> }
    readonly slo: { readonly objectives: ReadonlyArray<Slo.Objective> }
    readonly vital: { readonly gauges: ReadonlyArray<{ readonly ceiling: number; readonly kind: string }> }
  }
  type Suite = Payload["meter"] & Payload["overview"] & Payload["slo"] & Payload["vital"]
}

const _PACKS: { readonly [K in DashboardModel.Pack]: (identity: AppIdentity, payload: DashboardModel.Payload[K]) => DashboardModel } = {
  crash: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_crashes()],
      slug: "crash",
      tags: ["crash"],
      title: "crash",
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
