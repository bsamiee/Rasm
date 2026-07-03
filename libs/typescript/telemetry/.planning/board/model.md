# [TELEMETRY_MODEL]

Dashboards are data derived from identity: `DashboardModel` is one `Schema.Class` — deterministic uid slugged from the `AppIdentity`, a closed panel family as tagged rows, template variables, and alert annotations — and `Query` is the typed expression algebra panels are built from, a closed recursive family rendered to the metric-backend dialect by one fold, so no panel ever carries a hand-assembled query string. The model is the emission contract: it encodes through its own schema (`typeof DashboardModel.Encoded` is what `iac/observe` applies through the grafana provider), the `@grafana/grafana-foundation-sdk` stays behind this facade under the `[R14]` gate — model and packs compile without it, and its admission lands as one interior emission member, never a consumer-visible surface. A per-app dashboard file is unspellable: the only way to obtain a dashboard is a total function of identity.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                                    |
| :-----: | :-------- | :--------------------------------------------------------------------------- |
|  [01]   | [QUERY]   | the typed expression family and the one dialect-render fold                   |
|  [02]   | [PANEL]   | the closed panel row family and the shelf-layout grid fold                    |
|  [03]   | [MODEL]   | the `DashboardModel` owner: identity-derived uid, variables, annotations      |

## [2]-[QUERY]

[QUERY]:
- Owner: the `Query` closed family — `Instant` (a labeled series selector), `Rate` (per-second rate over a window), `Sum` (aggregation with a group-by label set), `Ratio` (one expression over another), `Quantile` (histogram quantile over a windowed rate) — recursive where composition demands it (`Sum`/`Ratio` hold `Query` operands), with `Query.render` as the one total fold to the PromQL-dialect string.
- Law: series names are `Convention.MetricName` rows and filter labels are `Convention` key rows — the algebra admits no free-string metric, so a dashboard query cannot reference a series the plane does not emit; the label-value pair set renders as the selector body, and the tenant template variable enters as an ordinary label value (`$tenant`).
- Law: windows are `Duration` values rendered by the interior `_span` projection (`90 seconds` -> `90s`) — a dialect window string is never authored.
- Law: one dialect fold — a second backend dialect is a second render fold over the SAME family, never a second family; the expression data is dialect-free by construction.
- Entry: constructors ride the family (`Query.Rate({ metric, window, labels })`), `Query.render(query)` at pack-build time.
- Growth: a new expression shape is one case plus one render arm — the compiler enforces the arm at the fold.

```typescript
import { Data, Duration, Record, pipe } from "effect"
import { Convention } from "../signal/convention.ts"

type Query = Data.TaggedEnum<{
  Instant: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName }
  Quantile: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly q: number; readonly window: Duration.Duration }
  Rate: { readonly labels: Convention.Attributes; readonly metric: Convention.MetricName; readonly window: Duration.Duration }
  Ratio: { readonly good: Query; readonly total: Query }
  Sum: { readonly by: ReadonlyArray<string>; readonly of: Query }
}>
const _Query = Data.taggedEnum<Query>()

const _span = (window: Duration.Duration): string =>
  pipe(Duration.toMillis(window), (millis) => `${Math.round(millis / 1000)}s`)

const _selector = (metric: Convention.MetricName, labels: Convention.Attributes): string =>
  pipe(
    Record.collect(labels, (key, value) => `${key}="${String(value)}"`),
    (pairs) => (pairs.length === 0 ? metric : `${metric}{${pairs.join(",")}}`),
  )

const _render = (query: Query): string =>
  _Query.$match(query, {
    Instant: ({ labels, metric }) => _selector(metric, labels),
    Quantile: ({ labels, metric, q, window }) =>
      `histogram_quantile(${q}, rate(${_selector(metric, labels)}[${_span(window)}]))`,
    Rate: ({ labels, metric, window }) => `rate(${_selector(metric, labels)}[${_span(window)}])`,
    Ratio: ({ good, total }) => `(${_render(good)}) / (${_render(total)})`,
    Sum: ({ by, of }) => `sum by (${by.join(",")}) (${_render(of)})`,
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

```typescript
import { Schema } from "effect"

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
- Owner: `DashboardModel` — uid (a slug brand derived, never supplied), `title`, the identity record (the same `Convention.identity` projection every signal stamps, so a dashboard is greppable by the attributes its panels query), `tags`, the tenant template `variables` row, `annotations` derived from `slo/alert` specs (slug plus tone), and the `panels` array.
- Law: `DashboardModel.of(identity, page)` is the ONLY constructor consumers touch — uid derives as `${identity.app}-${page.slug}` through the slug refinement, the tenant variable row is always present (a single-tenant app simply pins it), and the identity attributes stamp automatically — so every dashboard in existence is a total function of `AppIdentity` and a per-app fork has no authoring surface.
- Law: emission is the derived twin — `typeof DashboardModel.Encoded` and the class's own `Schema.encode` are what `iac/observe` consumes and applies through `@pulumiverse/grafana`; the `[R14]` foundation-sdk admission would land as one interior emission member behind this same encode seam, changing no consumer.
- Law: statics carry the derivations — `DashboardModel.laid` (the grid fold), the panel union as `DashboardModel.Panel` with every row schema riding the same owner (`DashboardModel.Timeseries`, `.Stat`, `.Gauge`, `.Heatmap`, `.Logs`, `.Table`) — so one import serves model, panels, rows, and layout, and a consumer constructs rows by name, never by union position.
- Entry: `DashboardModel.of(identity, page)`; `DashboardModel.laid(model)` at the apply seam.
- Growth: a new dashboard-level axis (a refresh cadence, a time-range default) is one field with its default in `of`.

```typescript
import { Array, Schema } from "effect"
import type { AppIdentity } from "@rasm/ts/kernel"
import { Convention } from "../signal/convention.ts"

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

// --- [EXPORTS] --------------------------------------------------------------------------

export { DashboardModel, Query }
```
