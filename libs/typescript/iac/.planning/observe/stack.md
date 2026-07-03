# [IAC_STACK]

The observability stack rows: one `Lgtm` tier installs the LGTM distribution (Loki, Grafana, Tempo, Mimir) and the OpenTelemetry collector as two `helm.v4.Chart` rows with typed value objects — upstream charts as typed values, zero authored YAML — and centralizes the one fact every consumer needs: the endpoint projections. The collector's values route OTLP ingest to the LGTM backends over in-cluster service DNS, the workloads receive the collector endpoint as the `OTEL_EXPORTER_OTLP_ENDPOINT` env row through the outputs seam, and `observe/apply` binds Grafana at the projected URL — so the entire telemetry pipe from app span to rendered board is wired by `Output` references, and a chart version bump edits pinned args plus at most the endpoint projection rows. Chart provenance and rendering stay under Pulumi's diff: `helm.v4.Chart` renders server-side, every rendered resource is policy-visible, and no release-lifecycle state exists. The module is `iac/src/observe/stack.ts`; a new backend is one chart row plus its endpoint projection, a new collector pipeline is one values row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                            | [PUBLIC] |
| :-----: | :------------------- | :------------------------------------------------------------------ | :------- |
|  [01]   | `CHART_ROWS`         | the LGTM and collector chart rows with typed values                | `Lgtm`   |
|  [02]   | `ENDPOINT_PROJECTION`| the service-DNS projections every consumer binds                   | `Lgtm`   |

## [2]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries both rows (`lgtm`: the `lgtm-distributed` chart from the Grafana repo; `collector`: `opentelemetry-collector` from the OpenTelemetry repo), versions arrive as pinned args, and the tier constructs both under one namespace with the collector's exporters aimed at the LGTM services by `Output`-woven URLs.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `observe/apply` authenticates with the same value; persistence, replica, and pipeline knobs are value rows under the pinned chart's own dialect, drifting only with the version pin.
- Law: the collector is the one ingest seam — receivers accept OTLP, exporters fan to Loki, Tempo, and Mimir over `[3]`'s projections; app workloads never learn a backend address, only the collector endpoint, so backend re-plumbing never touches an app.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and neither row here does.
- Entry: `new Lgtm("observe", { spec, namespace, versions, auth }, opts)` inside the k8s arm.
- Growth: a new backend chart is one `_charts` row; a collector pipeline axis is one values row.
- Boundary: dashboards and alerts are `observe/apply.md`'s; the app-side OTLP export composition is `telemetry`'s and arrives only as the env row.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"

const _charts = {
  lgtm: { chart: "lgtm-distributed", repo: "https://grafana.github.io/helm-charts" },
  collector: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts" },
} as const

declare namespace Lgtm {
  type Versions = { readonly lgtm: pulumi.Input<string>; readonly collector: pulumi.Input<string> }
  type Urls = {
    readonly grafana: pulumi.Output<string>
    readonly loki: pulumi.Output<string>
    readonly tempo: pulumi.Output<string>
    readonly prometheus: pulumi.Output<string>
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
          exporters: {
            otlphttp: pulumi.all([this.urls.loki, this.urls.tempo, this.urls.prometheus]).apply(([loki, tempo, prometheus]) => ({
              logs: { endpoint: loki },
              traces: { endpoint: tempo },
              metrics: { endpoint: prometheus },
            })),
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
- Law: endpoints are one projection — `_urls(release, namespace)` derives every backend address from the release name and namespace under the pinned chart's service-naming convention (`{release}-grafana`, `{release}-loki`, `{release}-tempo`, `{release}-mimir-nginx`), centralized so a chart bump that renames a service edits exactly these rows; no consumer ever spells a service DNS.
- Law: consumers bind outputs, not literals — `urls.grafana` feeds the `Boards` provider and `StackOutputs.grafana`, `collectorEndpoint` feeds the `otlp` output plane and thence the workload env; a hand-written URL anywhere downstream is the drift this projection deletes.
- Growth: a new backend's address is one `_urls` row beside its chart row.
- Boundary: ports and path suffixes are the pinned charts' facts, versioned with the pins.

```typescript
const _urls = (release: string, namespace: pulumi.Input<string>): Lgtm.Urls => ({
  grafana: pulumi.interpolate`http://${release}-grafana.${namespace}.svc`,
  loki: pulumi.interpolate`http://${release}-loki.${namespace}.svc:3100`,
  tempo: pulumi.interpolate`http://${release}-tempo.${namespace}.svc:4318`,
  prometheus: pulumi.interpolate`http://${release}-mimir-nginx.${namespace}.svc/api/v1/push`,
})

// --- [EXPORTS] --------------------------------------------------------------------------

export { Lgtm }
```
