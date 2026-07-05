# [IAC_OBSERVE]

Observability realization as one page in two sequential tiers: `Lgtm` installs the LGTM distribution (Loki, Grafana, Tempo, Mimir) and the OpenTelemetry collector as two `helm.v4.Chart` rows with typed value objects — upstream charts as typed values, zero authored YAML — and centralizes the one fact every consumer needs, the endpoint projections in two role planes; `Boards` consumes exactly what `Lgtm` produces (the Grafana URL and the same Doppler-generated password the chart seeded) and applies the core observe plane's identity-derived outputs: one folder per app, one `oss.DataSource` row per query-plane URL, one `oss.Dashboard` per encoded `DashboardModel`, alert rows per `Alert.Spec`. The board is code and the UI is drift: `storeDashboardSha256: true` diffs dashboards by content hash — the deploy-plane echo of the core content-key discipline — so a hand-edited board surfaces in the drift sweep as an `update` step. The collector is the one ingest seam: workloads learn only the collector endpoint through the env row, backends re-plumb with zero app edits, and `Boards` binds query URLs that re-point every board when a backend moves. The module is `iac/src/operate/observe.ts`; a new backend is one chart row plus its endpoint projections, a new dashboard is one more encoded model in the args array, a new collector pipeline is one values row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]             | [OWNS]                                                      | [PUBLIC] |
| :-----: | :-------------------- | :----------------------------------------------------------- | :------- |
|  [01]   | `CHART_ROWS`          | the LGTM and collector chart rows with typed values          | `Lgtm`   |
|  [02]   | `ENDPOINT_PROJECTION` | the service-DNS projections every consumer binds             | `Lgtm`   |
|  [03]   | `BOARD_APPLY`         | the Grafana provider, folder, sources, dashboards, alerts    | `Boards` |

## [2]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries both rows (`lgtm`: the `lgtm-distributed` chart from the Grafana repo; `collector`: `opentelemetry-collector` from the OpenTelemetry repo), versions arrive as pinned args, and the tier constructs both under one namespace with the collector's exporters aimed at the LGTM services by `Output`-woven URLs.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `Boards` authenticates with the same value; persistence, replica, and pipeline knobs are value rows under the pinned chart's own dialect, drifting only with the version pin.
- Law: the collector is the one ingest seam — the OTLP receiver admits, one named exporter per signal fans out over the ingest projections (`otlphttp/logs` to Loki's OTLP ingest, `otlphttp/traces` to Tempo, `prometheusremotewrite` to Mimir's push path), and the `service.pipelines` rows wire receiver to exporter per signal; app workloads never learn a backend address, only the collector endpoint, so backend re-plumbing never touches an app.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and neither row here does.
- Entry: `new Lgtm("observe", { spec, namespace, versions, auth }, opts)` inside the k8s arm.
- Growth: a new backend chart is one `_charts` row; a collector pipeline axis is one values row.
- Boundary: the app-side OTLP export composition is the runtime telemetry plane's and arrives only as the env row; board content is `[4]`'s upstream data.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`, `all`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Tier, type StackSpec } from "../program/spec.ts"

const _charts = {
  lgtm: { chart: "lgtm-distributed", repo: "https://grafana.github.io/helm-charts" },
  collector: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts" },
} as const

declare namespace Lgtm {
  type Versions = { readonly lgtm: pulumi.Input<string>; readonly collector: pulumi.Input<string> }
  type Urls = {
    readonly grafana: pulumi.Output<string>
    readonly ingest: {
      readonly logs: pulumi.Output<string>
      readonly traces: pulumi.Output<string>
      readonly metrics: pulumi.Output<string>
    }
    readonly query: {
      readonly loki: pulumi.Output<string>
      readonly tempo: pulumi.Output<string>
      readonly prometheus: pulumi.Output<string>
    }
  }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly versions: Versions
    readonly auth: pulumi.Input<string>
  }
}

class Lgtm extends Tier {
  readonly urls: Lgtm.Urls
  readonly collectorEndpoint: pulumi.Output<string>
  constructor(name: string, args: Lgtm.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Lgtm", name, opts)
    this.urls = _urls(name, args.namespace)
    new k8s.helm.v4.Chart(name, {
      chart: _charts.lgtm.chart,
      repositoryOpts: { repo: _charts.lgtm.repo },
      version: args.versions.lgtm,
      namespace: args.namespace,
      values: {
        grafana: { adminPassword: args.auth },
      },
    }, this.child())
    const collector = `${name}-otel`
    new k8s.helm.v4.Chart(collector, {
      chart: _charts.collector.chart,
      repositoryOpts: { repo: _charts.collector.repo },
      version: args.versions.collector,
      namespace: args.namespace,
      values: {
        mode: "deployment",
        config: {
          receivers: { otlp: { protocols: { http: {}, grpc: {} } } },
          exporters: pulumi.all([this.urls.ingest.logs, this.urls.ingest.traces, this.urls.ingest.metrics]).apply(([logs, traces, metrics]) => ({
            "otlphttp/logs": { endpoint: logs },
            "otlphttp/traces": { endpoint: traces },
            prometheusremotewrite: { endpoint: metrics },
          })),
          service: {
            pipelines: {
              logs: { receivers: ["otlp"], exporters: ["otlphttp/logs"] },
              traces: { receivers: ["otlp"], exporters: ["otlphttp/traces"] },
              metrics: { receivers: ["otlp"], exporters: ["prometheusremotewrite"] },
            },
          },
        },
      },
    }, this.child())
    this.collectorEndpoint = pulumi.interpolate`http://${collector}.${args.namespace}.svc:4318`
    this.seal({ urls: this.urls, collectorEndpoint: this.collectorEndpoint })
  }
}
```

## [3]-[ENDPOINT_PROJECTION]

[ENDPOINT_PROJECTION]:
- Law: endpoints are one projection in two role planes — `_urls(release, namespace)` derives every backend address from the release name and namespace under the pinned chart's service-naming convention, with `ingest` rows carrying each backend's write path (Loki OTLP, Tempo OTLP, Mimir remote-write) for the collector exporters and `query` rows carrying each backend's read API for the Grafana data sources — one backend, two roles, never one URL doing both jobs; a chart bump that renames a service or moves a path edits exactly these rows, and no consumer ever spells a service DNS.
- Law: consumers bind outputs, not literals — `urls.grafana` feeds the `Boards` provider and `StackOutputs.grafana`, `urls.query.*` feeds the `Boards` data sources, `collectorEndpoint` feeds the `otlp` output plane and thence the workload env; a hand-written URL anywhere downstream is the drift this projection deletes.
- Growth: a new backend's address is one ingest row plus one query row beside its chart row.
- Boundary: ports and path suffixes are the pinned charts' facts, versioned with the pins.

```typescript
const _urls = (release: string, namespace: pulumi.Input<string>): Lgtm.Urls => ({
  grafana: pulumi.interpolate`http://${release}-grafana.${namespace}.svc`,
  ingest: {
    logs: pulumi.interpolate`http://${release}-loki.${namespace}.svc:3100/otlp`,
    traces: pulumi.interpolate`http://${release}-tempo.${namespace}.svc:4318`,
    metrics: pulumi.interpolate`http://${release}-mimir-nginx.${namespace}.svc/api/v1/push`,
  },
  query: {
    loki: pulumi.interpolate`http://${release}-loki.${namespace}.svc:3100`,
    tempo: pulumi.interpolate`http://${release}-tempo.${namespace}.svc:3200`,
    prometheus: pulumi.interpolate`http://${release}-mimir-nginx.${namespace}.svc/prometheus`,
  },
})
```

## [4]-[BOARD_APPLY]

[BOARD_APPLY]:
- Owner: `Boards` — the tier-constructed `grafana.Provider` (`url` from `Lgtm.urls.grafana`, `auth` as the `user:password` form woven from the Doppler read, `retries` as the transient-fault posture, `storeDashboardSha256: true` so dashboard drift diffs by hash instead of full JSON) and the apply fold: one `oss.Folder` roots the app's boards (uid slugged from the spec's app key), `_SOURCES` maps backend rows (`prometheus`, `loki`, `tempo`) onto `oss.DataSource` constructions from the `Lgtm` query URLs, each encoded `DashboardModel` becomes one `oss.Dashboard` under the folder, and each `Alert.Spec` row realizes through the alerting arm — rule group, contact point, notification policy — with `slo.Slo` rows when the spec carries an objective.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect; auth never rides env here — the in-graph read is the canonical binding for deploy-time application.
- Law: models arrive encoded — the tier consumes `typeof DashboardModel.Encoded` values and `pulumi.jsonStringify` is the only serialization; the model's uid is the resource name, so the board's identity derivation survives into the Grafana state and the drift receipt, and no dashboard JSON exists on disk anywhere.
- Law: sources bind outputs — every `DataSource` url is an `Lgtm` query-plane projection `Output` (the read API, never an ingest path), so re-plumbing a backend re-points every board with zero board edits; a literal URL in a source row is the named defect.
- Law: the unverified argument surfaces stay declared — `_boardArgs`, `_alertRows`, and `_sloRows` are declared signatures over the catalogued class names (`oss.Dashboard`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `slo.Slo`) whose exact argument-record field spellings are the standing RESEARCH row; the tier's shape, ordering, and identity law are settled now, and the machine-identity upgrade (`oss.ServiceAccount` + `ServiceAccountToken` over the `admin:password` binding) lands the same way when its argument records reach catalogue depth.
- Entry: `new Boards("boards", { spec, lgtm, auth, boards, alerts }, opts)` inside the k8s arm, `boards`/`alerts` produced by the app's core observe suite call.
- Growth: a new panel family or board is upstream data — this tier changes only when Grafana grows a resource kind worth a row; alert silencing and templated notification rows (`alerting.MuteTiming`, `alerting.MessageTemplate`) land beside the alert rows when a paging policy earns them.
- Boundary: `DashboardModel`/`Alert` shapes are the core observe plane's owners consumed as encoded values; folder placement conventions live here, board content never does; drift interpretation is `operate/policy.md`'s.
- Packages: `@pulumiverse/grafana` (`Provider`, `oss.Folder`, `oss.DataSource`, `oss.Dashboard`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `slo.Slo`); `@rasm/ts/core` (`DashboardModel`, `Alert`); `effect` (`Array`, `Record`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as grafana from "@pulumiverse/grafana"
import type { Alert, DashboardModel } from "@rasm/ts/core"
import { Array, Record } from "effect"

declare namespace Boards {
  type Model = typeof DashboardModel.Encoded
  type Args = {
    readonly spec: StackSpec
    readonly lgtm: Lgtm
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Alert.Spec>
  }
}

declare const _boardArgs: (model: Boards.Model, folder: pulumi.Output<string>) => grafana.oss.DashboardArgs
declare const _alertRows: (
  alerts: ReadonlyArray<Alert.Spec>,
  folder: pulumi.Output<string>,
  child: pulumi.CustomResourceOptions,
) => ReadonlyArray<pulumi.CustomResource>
declare const _sloRows: (
  alerts: ReadonlyArray<Alert.Spec>,
  child: pulumi.CustomResourceOptions,
) => ReadonlyArray<grafana.slo.Slo>

const _SOURCES = {
  prometheus: { type: "prometheus", url: (urls: Lgtm.Urls) => urls.query.prometheus },
  loki: { type: "loki", url: (urls: Lgtm.Urls) => urls.query.loki },
  tempo: { type: "tempo", url: (urls: Lgtm.Urls) => urls.query.tempo },
} as const

class Boards extends Tier {
  constructor(name: string, args: Boards.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Boards", name, opts)
    const provider = new grafana.Provider(name, {
      url: args.lgtm.urls.grafana,
      auth: pulumi.interpolate`admin:${args.auth}`,
      retries: 3,
      storeDashboardSha256: true,
    }, { parent: this })
    const child = this.child({ provider })
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    Array.map(Record.toEntries(_SOURCES), ([key, row]) =>
      new grafana.oss.DataSource(key, { type: row.type, url: row.url(args.lgtm.urls) }, child))
    Array.map(args.boards, (model) => new grafana.oss.Dashboard(model.uid, _boardArgs(model, folder.uid), child))
    _alertRows(args.alerts, folder.uid, child)
    _sloRows(args.alerts, child)
    this.seal({ folder: folder.uid })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Boards, Lgtm }
```
