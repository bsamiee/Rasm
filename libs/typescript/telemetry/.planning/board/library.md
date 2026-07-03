# [TELEMETRY_LIBRARY]

The dashboard library is one handler record over a closed pack vocabulary: five pack rows — `overview`, `vital`, `crash`, `meter`, `slo` — each a total function from `AppIdentity` plus its own typed payload to a `DashboardModel`, dispatched through one generic indexed call so a consumer names a pack kind and receives a dashboard, never assembles panels. Every panel inside every pack is built from the settled layers — `Query` expressions over `Convention` metric rows, `Vital.rows` budget thresholds, `Alert.of` specs — so a threshold, metric name, or burn-row change upstream re-renders every affected pack with zero edits here. A new dashboard family is one payload row plus one pack row; a dashboard authored outside a pack function is the fork this page makes unspellable.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                             |
| :-----: | :-------- | :------------------------------------------------------------------ |
|  [01]   | [PANES]   | the reusable panel builders over the convention and policy layers    |
|  [02]   | [PACKS]   | the pack payload map, the handler record, the one dispatch and suite |

## [2]-[PANES]

[PANES]:
- Owner: the interior `_pane` builders — small total functions each returning one panel row from settled inputs: the request-duration quantile timeseries, the vital gauge against its budget row, the meter usage timeseries split by resource, the crash logs pane, and the per-spec SLO burn stat over the objective's own SLI expression.
- Law: a builder never invents a name or a threshold — series come from `Convention.metric` rows, tenancy filters from `Convention.rasm` keys against the `$tenant` template variable, vital ceilings from `Vital.rows`, burn thresholds from the spec's own `factor` — so the builders are pure plumbing between vocabulary and visualization, and deleting any hardcoded literal from them leaves nothing to delete.
- Law: spans are the builders' only local decision — each pane declares its grid `span` so the model's shelf fold lays every pack without per-pack layout code.
- Growth: a new reusable visualization is one builder consumed by pack rows — it earns existence at two pack call sites, else it inlines.

```typescript
import { Duration, Option } from "effect"
import { Convention } from "../signal/convention.ts"
import type { MeterFact } from "../signal/meter.ts"
import { Vital } from "../signal/vital.ts"
import type { Alert } from "../slo/alert.ts"
import { Sli } from "../slo/burnrate.ts"
import { DashboardModel, Query } from "./model.ts"

const _tenant = { [Convention.rasm.tenant]: "$tenant" } as const

const _WINDOW = Duration.minutes(5)

const _latency = (quantile: number): typeof DashboardModel.Panel.Type =>
  DashboardModel.Timeseries.make({
    exprs: [
      Query.render(Query.Quantile({ labels: _tenant, metric: Convention.metric.httpServerDuration, q: quantile, window: _WINDOW })),
    ],
    span: { h: 8, w: 12 },
    title: `latency p${Math.round(quantile * 100)}`,
    unit: Option.some("ms"),
  })

const _vitalGauge = (kind: Vital.Kind): typeof DashboardModel.Panel.Type =>
  DashboardModel.Gauge.make({
    ceiling: Vital.rows[kind].poor,
    expr: Query.render(Query.Instant({ labels: { [Convention.rasm.vitalKind]: kind }, metric: Convention.metric.vitalLevel })),
    span: { h: 6, w: 4 },
    title: kind,
  })

const _usage = (resource: MeterFact.Resource): typeof DashboardModel.Panel.Type =>
  DashboardModel.Timeseries.make({
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

const _crashes = (): typeof DashboardModel.Panel.Type =>
  DashboardModel.Logs.make({
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

const _burnPair = (spec: Alert.Spec): typeof DashboardModel.Panel.Type =>
  DashboardModel.Stat.make({
    expr: _sliExpr(spec.sli, Duration.decode(spec.windows.long)),
    span: { h: 4, w: 6 },
    title: `${spec.slug} trips at ${spec.factor}x budget`,
  })
```

## [3]-[PACKS]

[PACKS]:
- Owner: the `Library` handler record — the payload map types each pack's input (`slo` demands objectives, `overview` its quantiles, the rest are empty rows), the mapped handler contract turns a missing pack into a compile error at the record, and `Library.pack(kind, identity, payload)` is the one generic indexed dispatch whose payload follows the kind.
- Law: every pack routes through `DashboardModel.of` — identity-derived uid, stamped identity attributes, the always-present tenant variable — so the pack layer cannot mint an identity-free dashboard; the `slo` pack folds `Alert.of(objective)` specs into burn panels and annotation rows, making the alert and dashboard views of one objective provably the same data.
- Law: `Library.suite(identity, payload)` derives the standing fleet — every pack whose payload the suite input carries — the one call an app's deploy program makes; `iac/observe` applies `DashboardModel.laid` projections of exactly this suite.
- Boundary: provider emission — grafana JSON, folder placement, apply lifecycle — is `iac/observe`'s seam over `typeof DashboardModel.Encoded`; the `[R14]` foundation-sdk gate lives behind the model's encode, invisible here.
- Entry: `Library.pack(kind, identity, payload)`; `Library.suite(identity, payload)`.
- Growth: a new dashboard family is one payload row plus one handler row; every consumer inherits it through the derived kind union.

```typescript
import { Array, Struct } from "effect"
import type { AppIdentity } from "@rasm/ts/kernel"
import { Meter } from "../signal/meter.ts"
import { Alert } from "../slo/alert.ts"
import type { Slo } from "../slo/burnrate.ts"

declare namespace Library {
  type Payload = {
    readonly crash: Record<never, never>
    readonly meter: Record<never, never>
    readonly overview: { readonly quantiles: ReadonlyArray<number> }
    readonly slo: { readonly objectives: ReadonlyArray<Slo.Objective> }
    readonly vital: Record<never, never>
  }
  type Kind = keyof Payload
  type Suite = Payload["overview"] & Payload["slo"]
}

const _PACKS: { readonly [K in Library.Kind]: (identity: AppIdentity, payload: Library.Payload[K]) => DashboardModel } = {
  crash: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: [_crashes()],
      slug: "crash",
      tags: ["crash"],
      title: "crash",
      variables: [],
    }),
  meter: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: Array.map(Struct.keys(Meter.rows), _usage),
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
  vital: (identity) =>
    DashboardModel.of(identity, {
      annotations: [],
      panels: Array.map(Struct.keys(Vital.rows), _vitalGauge),
      slug: "vital",
      tags: ["vital", "rum"],
      title: "web vitals",
      variables: [],
    }),
}

const Library: {
  readonly pack: <K extends Library.Kind>(kind: K, identity: AppIdentity, payload: Library.Payload[K]) => DashboardModel
  readonly suite: (identity: AppIdentity, payload: Library.Suite) => ReadonlyArray<DashboardModel>
} = {
  pack: (kind, identity, payload) => _PACKS[kind](identity, payload),
  suite: (identity, payload) => [
    _PACKS.overview(identity, { quantiles: payload.quantiles }),
    _PACKS.slo(identity, { objectives: payload.objectives }),
    _PACKS.vital(identity, {}),
    _PACKS.meter(identity, {}),
    _PACKS.crash(identity, {}),
  ],
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Library }
```
