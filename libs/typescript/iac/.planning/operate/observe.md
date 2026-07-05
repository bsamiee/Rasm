# [IAC_OBSERVE]

Observability realization as one page in two sequential tiers: `Lgtm` installs the LGTM distribution (Loki, Grafana, Tempo, Mimir) and the OpenTelemetry collector as two `helm.v4.Chart` rows with typed value objects — upstream charts as typed values, zero authored YAML, provenance-verified when a keyring asset rides the pins — and centralizes the one fact every consumer needs, the endpoint projections in two role planes, exposing the rendered child set for policy and discovery reads; `Boards` consumes exactly what `Lgtm` produces and applies the core observe plane's identity-derived outputs whole: one folder per app, one `oss.DataSource` row per query-plane URL, one `oss.Dashboard` per encoded `DashboardModel`, the `Alert.Spec` burn-rate compile realized as `alerting.RuleGroup` rules over the multiwindow SLI expression with severity-routed `ContactPoint`/`NotificationPolicy` delivery, `slo.Slo` rows compiled from the suite's own objectives, one `oss.Organization` per tenant, and the `oss.ServiceAccount`/`ServiceAccountToken` machine identity minted for out-of-graph automation. The board is code and the UI is drift: `storeDashboardSha256: true` diffs dashboards by content hash, and the provider carries the transient-fault posture as data (`retries`, `retryStatusCodes`, `retryWait`). The collector is the one ingest seam: workloads learn only the collector endpoint through the env row, backends re-plumb with zero app edits, and `Boards` binds query URLs that re-point every board when a backend moves. The module is `iac/src/operate/observe.ts`; a new backend is one chart row plus its endpoint projections, a new dashboard is one more encoded model, a new alert is upstream spec data compiled by the same fold, a new severity route is one contacts row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]             | [OWNS]                                                        | [PUBLIC] |
| :-----: | :-------------------- | :------------------------------------------------------------- | :------- |
|  [01]   | `CHART_ROWS`          | the LGTM and collector chart rows with typed values            | `Lgtm`   |
|  [02]   | `ENDPOINT_PROJECTION` | the service-DNS projections every consumer binds               | `Lgtm`   |
|  [03]   | `BOARD_APPLY`         | provider, folder, sources, dashboards, alert/SLO compile, RBAC | `Boards` |

## [2]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries both rows (`lgtm`: the `lgtm-distributed` chart from the Grafana repo; `collector`: `opentelemetry-collector` from the OpenTelemetry repo), versions arrive as pinned args, and the tier constructs both under one namespace with the collector's exporters aimed at the LGTM services by `Output`-woven URLs; the lgtm chart's rendered child set rides the tier as `rendered` so CrossGuard stack validation and chart-emitted-resource discovery read real render evidence, never chart-name guesses.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `Boards` authenticates with the same value; persistence, replica, and pipeline knobs are value rows under the pinned chart's own dialect, drifting only with the version pin.
- Law: provenance rides the pins — when a `keyring` asset accompanies the versions, both chart rows verify (`verify: true` + the keyring), so a tampered chart fails at render, and the estate's content-addressed discipline extends to its chart supply.
- Law: the collector is the one ingest seam — the OTLP receiver admits, one named exporter per signal fans out over the ingest projections (`otlphttp/logs` to Loki's OTLP ingest, `otlphttp/traces` to Tempo, `prometheusremotewrite` to Mimir's push path), and the `service.pipelines` rows wire receiver to exporter per signal; app workloads never learn a backend address, only the collector endpoint, so backend re-plumbing never touches an app.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and neither row here does.
- Entry: `new Lgtm("observe", { spec, namespace, versions, auth, keyring? }, opts)` inside the k8s arm.
- Growth: a new backend chart is one `_charts` row; a collector pipeline axis is one values row.
- Boundary: the app-side OTLP export composition is the runtime telemetry plane's and arrives only as the env row; board content is `[4]`'s upstream data.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`, `all`, `asset`); `../program/spec.ts` (`StackSpec`, `Tier`).

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
    readonly keyring?: pulumi.asset.Asset
  }
}

class Lgtm extends Tier {
  readonly urls: Lgtm.Urls
  readonly collectorEndpoint: pulumi.Output<string>
  readonly rendered: pulumi.Output<ReadonlyArray<unknown>>
  constructor(name: string, args: Lgtm.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Lgtm", name, opts)
    this.urls = _urls(name, args.namespace)
    const provenance = args.keyring === undefined ? {} : { verify: true, keyring: args.keyring }
    const stack = new k8s.helm.v4.Chart(name, {
      chart: _charts.lgtm.chart,
      repositoryOpts: { repo: _charts.lgtm.repo },
      version: args.versions.lgtm,
      namespace: args.namespace,
      ...provenance,
      values: {
        grafana: { adminPassword: args.auth },
      },
    }, this.child())
    this.rendered = stack.resources
    const collector = `${name}-otel`
    new k8s.helm.v4.Chart(collector, {
      chart: _charts.collector.chart,
      repositoryOpts: { repo: _charts.collector.repo },
      version: args.versions.collector,
      namespace: args.namespace,
      ...provenance,
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
- Owner: `Boards` — the tier-constructed `grafana.Provider` (`url` from `Lgtm.urls.grafana`, `auth` as the `user:password` form woven from the Doppler read, the transient-fault posture as data — `retries`, `retryStatusCodes`, `retryWait` — and `storeDashboardSha256: true` so dashboard drift diffs by hash instead of full JSON) and the apply fold: one `oss.Folder` roots the app's boards (uid slugged from the spec's app key), `_SOURCES` maps backend rows onto `oss.DataSource` constructions from the `Lgtm` query URLs, each encoded `DashboardModel` becomes one `oss.Dashboard` under the folder, `_alerted` compiles the `Alert.Spec` rows into one `alerting.RuleGroup` plus severity-routed delivery, `_slos` compiles the suite's objectives into `slo.Slo` rows, tenant organizations realize per `spec.tenants` slug, and the machine identity mints once.
- Law: the alert compile is total over the spec — `_expr` is the one `Sli.$match` fold from SLI case to PromQL (the `Ratio` case compiles the multiwindow burn-rate comparison `error-rate > factor × budget` over the spec's own `windows`; the `Latency` case compiles `histogram_quantile` over the metric's bucket series against the ceiling), each spec becomes one rule whose `datas` pair a Prometheus query node with a `__expr__` threshold node, `for` derives from the severity row's `hold`, and the spec's `annotations`/`slug` ride the rule verbatim — the suite computes policy, this fold only spells it in the provider dialect, and a consumer re-deriving burn thresholds is the forked-discipline defect.
- Law: delivery routes by severity as data — the `contacts` record carries one receiver row per severity kind (`page`, `ticket`); each present row realizes one `alerting.ContactPoint` and one `NotificationPolicy` matcher route on the `severity` label, so paging posture is data the spec's severity row keys, and `alerting.MuteTiming`/`MessageTemplate` land beside these rows when a paging calendar earns them.
- Law: SLOs compile from objectives, never from alerts — `_slos` maps the suite's own `Slo.Objective` values (name, target, window, SLI) onto `slo.Slo` rows with the plain error-ratio query, so the alert fold and the SLO fold read one upstream vocabulary and cannot disagree.
- Law: tenancy is organizations — one `oss.Organization` per `spec.tenants` slug, so a tenant's boards, sources, and permissions scope to its org; the per-tenant folder-and-source apply inside each org is the finalization the tenancy escalation drives.
- Law: the machine identity is minted, not assumed — one `oss.ServiceAccount` (`role: "Admin"`) plus `oss.ServiceAccountToken` realize the durable automation identity; the token key egresses as the tier's `automation` output for the composing arm to land in a Doppler `{ value }` entry, and the chart-seeded `admin:password` binding remains the one in-graph provider auth.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect; auth never rides env here — the in-graph read is the canonical binding for deploy-time application.
- Law: models arrive encoded — the tier consumes `typeof DashboardModel.Encoded` values and `pulumi.jsonStringify` is the only serialization; the model's uid is the resource name, so the board's identity derivation survives into the Grafana state and the drift receipt, and no dashboard JSON exists on disk anywhere.
- Entry: `new Boards("boards", { spec, lgtm, auth, boards, alerts, objectives, contacts }, opts)` inside the k8s arm, `boards`/`alerts`/`objectives` produced by the app's core observe suite call.
- Growth: a new panel family or board is upstream data; a new severity route is one `contacts` row; a new SLI case is one `_expr` match arm beside its upstream `Sli` case.
- Boundary: `DashboardModel`/`Alert`/`Slo` shapes are the core observe plane's owners consumed as encoded values; folder placement conventions live here, board content never does; drift interpretation is `operate/policy.md`'s.
- Packages: `@pulumiverse/grafana` (`Provider`, `oss.Folder`, `oss.DataSource`, `oss.Dashboard`, `oss.Organization`, `oss.ServiceAccount`, `oss.ServiceAccountToken`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `slo.Slo`); `@rasm/ts/core` (`DashboardModel`, `Alert`, `Sli`, `Slo`); `effect` (`Array`, `Duration`, `Record`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as grafana from "@pulumiverse/grafana"
import { Sli, type Alert, type DashboardModel, type Slo } from "@rasm/ts/core"
import { Array, Duration, Record } from "effect"

declare namespace Boards {
  type Model = typeof DashboardModel.Encoded
  type Contacts = Partial<Record.ReadonlyRecord<"page" | "ticket", { readonly webhook: pulumi.Input<string> }>>
  type Args = {
    readonly spec: StackSpec
    readonly lgtm: Lgtm
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Alert.Spec>
    readonly objectives: ReadonlyArray<Slo.Objective>
    readonly contacts: Contacts
  }
}

const _SOURCES = {
  prometheus: { type: "prometheus", url: (urls: Lgtm.Urls) => urls.query.prometheus },
  loki: { type: "loki", url: (urls: Lgtm.Urls) => urls.query.loki },
  tempo: { type: "tempo", url: (urls: Lgtm.Urls) => urls.query.tempo },
} as const

const _window = (input: Duration.DurationInput): string => `${Duration.toSeconds(Duration.decode(input))}s`

const _expr = (spec: Alert.Spec): string =>
  Sli.$match(spec.sli, {
    Ratio: ({ good, total }) =>
      `(1 - (sum(rate(${good}[${_window(spec.windows.short)}])) / sum(rate(${total}[${_window(spec.windows.long)}])))) > ${spec.factor} * ${1 - spec.target}`,
    Latency: ({ ceiling, metric, quantile }) =>
      `histogram_quantile(${quantile}, sum by (le) (rate(${metric}_bucket[${_window(spec.windows.long)}]))) > ${Duration.toSeconds(ceiling)}`,
  })

const _query = (sli: Sli): string =>
  Sli.$match(sli, {
    Ratio: ({ good, total }) => `sum(rate(${good}[$__rate_interval])) / sum(rate(${total}[$__rate_interval]))`,
    Latency: ({ metric, quantile }) => `histogram_quantile(${quantile}, sum by (le) (rate(${metric}_bucket[$__rate_interval])))`,
  })

const _boardArgs = (model: Boards.Model, folder: pulumi.Output<string>): grafana.oss.DashboardArgs => ({
  configJson: pulumi.jsonStringify(model),
  folder,
})

const _alerted = (
  alerts: ReadonlyArray<Alert.Spec>,
  bind: { readonly folder: pulumi.Output<string>; readonly datasource: pulumi.Output<string>; readonly contacts: Boards.Contacts },
  child: pulumi.CustomResourceOptions,
): void => {
  const routes = Array.map(Record.toEntries(bind.contacts), ([severity, row]) => {
    const point = new grafana.alerting.ContactPoint(severity, {
      name: severity,
      webhooks: [{ url: row.webhook }],
    }, child)
    return { severity, point }
  })
  Array.match(routes, {
    onEmpty: () => undefined,
    onNonEmpty: ([head, ...rest]) =>
      new grafana.alerting.NotificationPolicy("routing", {
        contactPoint: head.point.name,
        groupBies: ["alertname"],
        policies: Array.map([head, ...rest], ({ severity, point }) => ({
          contactPoint: point.name,
          matchers: [{ label: "severity", match: "=", value: severity }],
        })),
      }, child),
  })
  Array.match(alerts, {
    onEmpty: () => undefined,
    onNonEmpty: (specs) =>
      new grafana.alerting.RuleGroup("burn", {
        folderUid: bind.folder,
        intervalSeconds: 60,
        rules: Array.map(specs, (spec) => ({
          name: spec.slug,
          condition: "B",
          for: _window(spec.severity.hold),
          annotations: spec.annotations,
          labels: { severity: spec.severity.kind },
          datas: [
            {
              refId: "A",
              datasourceUid: bind.datasource,
              relativeTimeRange: { from: 3600, to: 0 },
              model: JSON.stringify({ refId: "A", expr: _expr(spec) }),
            },
            {
              refId: "B",
              datasourceUid: "__expr__",
              relativeTimeRange: { from: 0, to: 0 },
              model: JSON.stringify({
                refId: "B",
                type: "threshold",
                expression: "A",
                conditions: [{ evaluator: { type: "gt", params: [0] } }],
              }),
            },
          ],
        })),
      }, child),
  })
}

const _slos = (
  objectives: ReadonlyArray<Slo.Objective>,
  folder: pulumi.Output<string>,
  child: pulumi.CustomResourceOptions,
): ReadonlyArray<grafana.slo.Slo> =>
  Array.map(objectives, (objective) =>
    new grafana.slo.Slo(objective.name, {
      name: objective.name,
      description: `<slo:${objective.name}>`,
      folderUid: folder,
      objectives: [{ value: objective.target, window: _window(objective.window) }],
      queries: [{ type: "freeform", freeform: { query: _query(objective.sli) } }],
    }, child))

class Boards extends Tier {
  readonly automation: pulumi.Output<string>
  constructor(name: string, args: Boards.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Boards", name, opts)
    const provider = new grafana.Provider(name, {
      url: args.lgtm.urls.grafana,
      auth: pulumi.interpolate`admin:${args.auth}`,
      retries: 3,
      retryStatusCodes: ["429", "5xx"],
      retryWait: 5,
      storeDashboardSha256: true,
    }, { parent: this })
    const child = this.child({ provider })
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    const sources = Record.map(_SOURCES, (row, key) =>
      new grafana.oss.DataSource(key, { type: row.type, url: row.url(args.lgtm.urls) }, child))
    Array.map(args.boards, (model) => new grafana.oss.Dashboard(model.uid, _boardArgs(model, folder.uid), child))
    _alerted(args.alerts, { folder: folder.uid, datasource: sources.prometheus.uid, contacts: args.contacts }, child)
    _slos(args.objectives, folder.uid, child)
    Array.map(args.spec.tenants, (tenant) => new grafana.oss.Organization(tenant, { name: tenant }, child))
    const account = new grafana.oss.ServiceAccount(`${name}-automation`, { name: `${name}-automation`, role: "Admin" }, child)
    this.automation = new grafana.oss.ServiceAccountToken(`${name}-automation`, {
      name: `${name}-automation`,
      serviceAccountId: account.id,
    }, child).key
    this.seal({ folder: folder.uid })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Boards, Lgtm }
```
