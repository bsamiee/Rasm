# [IAC_OBSERVE]

Observability realization in three tiers. `Lgtm` installs the k8s backend estate: one closed metrics-store row family (`prometheus | mimir | victoriametrics`) selected by `spec.profile.observe.store`, the Loki/Tempo/Pyroscope/Grafana backends beside it, and the OpenTelemetry collector as the one ingest seam aiming per-signal exporters at the selected rows through `Output`-woven URLs. `Dev` is the docker arm's estate as one all-in-one container publishing the same URL plane. `Boards` applies the core observe plane's identity-derived outputs whole against either producer's URL plane.

Board content is code and the UI is drift: `storeDashboardSha256: true` diffs dashboards by content hash, and the provider carries the transient-fault posture as data. Workloads learn only the collector endpoint through the env row, so a backend re-plumb — a store swap included — never touches an app. `iac/src/operate/observe.ts` is the module; a new backend is one chart row with its endpoint projections, a new store is one `_stores` row, a new dashboard is one more encoded model, a new alert is upstream spec data compiled by the same fold, a new severity route is one contacts row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]             | [OWNS]                                                                  | [PUBLIC] |
| :-----: | :-------------------- | :----------------------------------------------------------------------- | :------- |
|  [01]   | `STORE_ROWS`          | the closed metrics-store family with tenancy/retention/degradation rows | `Lgtm`   |
|  [02]   | `CHART_ROWS`          | the signal-backend and collector chart rows with typed values           | `Lgtm`   |
|  [03]   | `SCRAPE_ROWS`         | the pg-server metrics arm pair under the one-ingress law                | `Lgtm`   |
|  [04]   | `DEV_ROW`             | the docker-arm estate as one all-in-one tier publishing the URL plane   | `Dev`    |
|  [05]   | `ENDPOINT_PROJECTION` | the service-DNS projections every consumer binds                        | `Lgtm`   |
|  [06]   | `BOARD_APPLY`         | provider, folder, sources, the builder compile leg, alerts, RBAC        | `Boards` |

## [02]-[STORE_ROWS]

[STORE_ROWS]:
- Owner: the interior `_stores` family — one row per metrics store carrying `chart`/`repo`, the `write` (OTLP/remote-write ingest path) and `read` (query API path) projections, the `exemplars` column, the `tenancy` column (`label | org`), and the `degrade` declaration — with `spec.profile.observe.store` selecting the row and `spec.profile.observe.retention` flowing into the row's values; the family is Mimir-SHAPED: every coordinate an escalation needs is a column on every row, so promotion edits the spec, never a tier.
- Law: `prometheus` is the reference row — its decisive column is `exemplars: true`: native exemplar storage (`--enable-feature=exemplar-storage` and native histograms in the row's values) powers the metric→trace click-through into Tempo that the whole board plane links on; tenant stays the `rasm.tenant` label (`tenancy: "label"`), and retention is the row's server retention value.
- Law: `mimir` is the fleet escalation — multi-component and memory-heavy, earned only past the single-store ceiling; its object-store binding reuses the object plane's endpoint and bucket coordinates (one storage truth, never a second store config), its ruler runs in-store so the burn rules the `Boards` fold applies can escalate to store-side evaluation as one values row, and `tenancy: "org"` stamps the stack's org id as the `X-Scope-OrgID` header on the collector's metrics exporter.
- Law: `victoriametrics` is the resource-pressure escape — `exemplars: false` is its declared degradation: the metric→trace click-through drops to trace-search-by-time, a posture the row states so selecting it is an informed trade, never a surprise; every other projection column holds.
- Law: degradation is a row declaration — each row's `degrade` column states what the estate loses on that row, so the store decision reads as data at the spec seam and the dashboards' exemplar links gate on the selected row's `exemplars` column.
- Growth: a fourth store is one row with every column answered — the family is closed until a row lands.
- Boundary: which store a stack runs is `program/spec.md`'s `observe.store` coordinate; the tenant metric label is the runtime plane's `Convention.rasm.tenant` dimension arriving on the wire, never re-minted here.

```typescript
const _stores = {
  prometheus: {
    chart: "prometheus", repo: "https://prometheus-community.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-prometheus-server.${ns}.svc/api/v1/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-prometheus-server.${ns}.svc`,
    exemplars: true, tenancy: "label",
    degrade: "single-store; tenant is a label, never an isolation boundary",
  },
  mimir: {
    chart: "mimir-distributed", repo: "https://grafana.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-mimir-nginx.${ns}.svc/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-mimir-nginx.${ns}.svc/prometheus`,
    exemplars: true, tenancy: "org",
    degrade: "multi-component memory cost; earned only past the single-store ceiling",
  },
  victoriametrics: {
    chart: "victoria-metrics-single", repo: "https://victoriametrics.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-victoria-metrics-single-server.${ns}.svc:8428/opentelemetry`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-victoria-metrics-single-server.${ns}.svc:8428`,
    exemplars: false, tenancy: "label",
    degrade: "no exemplar storage: metric→trace click-through degrades to trace search",
  },
} as const satisfies Record<string, {
  readonly chart: string
  readonly repo: string
  readonly write: (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>
  readonly read: (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>
  readonly exemplars: boolean
  readonly tenancy: "label" | "org"
  readonly degrade: string
}>
```

## [03]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries the signal-backend rows (`loki`, `tempo`, `grafana`, `pyroscope` — the profiles row, present while `spec.profile.observe.profiles` holds — and `collector`), the selected `_stores` row lands beside them, versions arrive as pinned args, and the tier constructs every chart under one namespace with the collector's exporters aimed at the backends by `Output`-woven URLs; every chart renders as stack children under Pulumi diff and CrossGuard validation, and `rendered` aggregates the child sets as the tier's discovery projection — real render evidence, never chart-name guesses.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `Boards` authenticates with the same value; persistence, replica, retention, and pipeline knobs are value rows under each pinned chart's own dialect, drifting only with the version pin.
- Law: provenance rides the pins — when a `keyring` asset accompanies the versions, every chart row verifies (`verify: true` + the keyring), so a tampered chart fails at render and the estate's content-addressed discipline extends to its chart supply.
- Law: the collector is the one ingest seam — the OTLP receiver admits, one named exporter per signal fans out (`otlphttp/logs` to Loki's OTLP ingest, `otlphttp/traces` to Tempo, `otlphttp/metrics` to the selected store row's `write` path with the org header where the row's tenancy column demands it, `otlphttp/profiles` to Pyroscope while the profiles row holds), and the `service.pipelines` rows wire receiver to exporter per signal; workloads never learn a backend address, only the collector endpoint, and the exporter `sending_queue` persists through the `file_storage` extension so a backend restart drops nothing.
- Law: profiles ride the push path — Pyroscope ingests the runtime SDKs' push streams and the collector's profiles pipeline; the row is present-by-default and its removal is a spec delta, so the LGTM plane carries four signals, not three.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and no row here does.
- Entry: `new Lgtm("observe", { spec, namespace, versions, auth, dsn, keyring? }, opts)` inside the k8s arm.
- Growth: a new signal backend is one `_charts` row with its endpoint projections; a collector pipeline axis is one values row.
- Boundary: the app-side OTLP export composition is the runtime telemetry plane's and arrives only as the env row; board content is `[06]`'s upstream data.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`, `all`, `asset`); `effect` (`Array`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Tier, type StackSpec } from "../program/spec.ts"

const _charts = {
  collector: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts" },
  grafana: { chart: "grafana", repo: "https://grafana.github.io/helm-charts" },
  loki: { chart: "loki", repo: "https://grafana.github.io/helm-charts" },
  pyroscope: { chart: "pyroscope", repo: "https://grafana.github.io/helm-charts" },
  tempo: { chart: "tempo", repo: "https://grafana.github.io/helm-charts" },
} as const

declare namespace Lgtm {
  type Versions = { readonly [K in keyof typeof _charts | "store" | "exporter"]: pulumi.Input<string> }
  type Urls = {
    readonly grafana: pulumi.Output<string>
    readonly ingest: {
      readonly logs: pulumi.Output<string>
      readonly traces: pulumi.Output<string>
      readonly metrics: pulumi.Output<string>
      readonly profiles: pulumi.Output<string>
    }
    readonly query: {
      readonly loki: pulumi.Output<string>
      readonly tempo: pulumi.Output<string>
      readonly prometheus: pulumi.Output<string>
      readonly pyroscope: pulumi.Output<string>
    }
  }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly versions: Versions
    readonly auth: pulumi.Input<string>
    readonly dsn: pulumi.Input<string>
    readonly objects?: { readonly endpoint: pulumi.Input<string>; readonly bucket: pulumi.Input<string> } // the object plane's coordinates the mimir row binds; every other store row ignores them
    readonly keyring?: pulumi.asset.Asset
  }
}

class Lgtm extends Tier {
  readonly urls: Lgtm.Urls
  readonly collectorEndpoint: pulumi.Output<string>
  readonly rendered: pulumi.Output<ReadonlyArray<unknown>>
  constructor(name: string, args: Lgtm.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Lgtm", name, opts)
    const store = _stores[args.spec.profile.observe.store]
    this.urls = _urls(name, args.namespace, store)
    const provenance = args.keyring === undefined ? {} : { verify: true, keyring: args.keyring }
    const charts: Array<pulumi.Output<ReadonlyArray<unknown>>> = [] // every chart's rendered child set aggregates onto the tier's discovery projection
    const row = (key: keyof typeof _charts, values: pulumi.Inputs) => {
      const chart = new k8s.helm.v4.Chart(`${name}-${key}`, {
        chart: _charts[key].chart,
        repositoryOpts: { repo: _charts[key].repo },
        version: args.versions[key],
        namespace: args.namespace,
        ...provenance,
        values,
      }, this.child())
      charts.push(chart.resources)
      return chart
    }
    row("grafana", { adminPassword: args.auth })
    row("loki", {})
    row("tempo", {})
    const profiled = args.spec.profile.observe.profiles
    if (profiled) row("pyroscope", {})
    charts.push(new k8s.helm.v4.Chart(`${name}-store`, {
      chart: store.chart,
      repositoryOpts: { repo: store.repo },
      version: args.versions.store,
      namespace: args.namespace,
      ...provenance,
      values: {
        retention: args.spec.profile.observe.retention, // retention rides the row's own dialect key; drifts only with the pin
        ...(args.spec.profile.observe.store === "prometheus" && {
          server: { extraFlags: ["enable-feature=exemplar-storage", "enable-feature=native-histograms", "web.enable-otlp-receiver"] },
        }),
        ...(args.spec.profile.observe.store === "mimir" && {
          // one storage truth: ruler_storage and blocks_storage bind args.objects' endpoint/bucket; the dialect keys inside ride the [STORE_CHARTS] research row
          mimir: { structuredConfig: { ruler_storage: {}, blocks_storage: {} } },
        }),
      },
    }, this.child()).resources)
    if (args.spec.profile.observe.ingest === "scrape") {
      // infra exporter is a tier child, never an app workload: DSN stays in-graph, the collector alone scrapes it
      new k8s.apps.v1.Deployment(`${name}-pg-exporter`, {
        metadata: { namespace: args.namespace, labels: { app: "postgres-exporter" } },
        spec: {
          selector: { matchLabels: { app: "postgres-exporter" } },
          template: {
            metadata: { labels: { app: "postgres-exporter" } },
            spec: { containers: [{ name: "exporter", image: args.versions.exporter, env: [{ name: "DATA_SOURCE_NAME", value: args.dsn }] }] },
          },
        },
      }, this.child())
      new k8s.core.v1.Service("postgres-exporter", {
        metadata: { name: "postgres-exporter", namespace: args.namespace },
        spec: { selector: { app: "postgres-exporter" }, ports: [{ port: 9187 }] },
      }, this.child())
    }
    const collector = `${name}-otel`
    charts.push(new k8s.helm.v4.Chart(collector, {
      chart: _charts.collector.chart,
      repositoryOpts: { repo: _charts.collector.repo },
      version: args.versions.collector,
      namespace: args.namespace,
      ...provenance,
      values: {
        mode: "deployment",
        config: pulumi.all([this.urls.ingest.logs, this.urls.ingest.traces, this.urls.ingest.metrics, this.urls.ingest.profiles])
          .apply(([logs, traces, metrics, profiles]) => ({
            receivers: {
              otlp: { protocols: { http: {}, grpc: {} } },
              ..._pg(args.spec.profile.observe.ingest, args.dsn),
            },
            extensions: { file_storage: {} },
            exporters: {
              "otlphttp/logs": { endpoint: logs, sending_queue: { storage: "file_storage" } },
              "otlphttp/traces": { endpoint: traces, sending_queue: { storage: "file_storage" } },
              "otlphttp/metrics": {
                endpoint: metrics,
                sending_queue: { storage: "file_storage" },
                ...(store.tenancy === "org" && { headers: { "X-Scope-OrgID": args.spec.app } }),
              },
              ...(profiled && { "otlphttp/profiles": { endpoint: profiles, sending_queue: { storage: "file_storage" } } }),
            },
            service: {
              extensions: ["file_storage"],
              pipelines: {
                logs: { receivers: ["otlp"], exporters: ["otlphttp/logs"] },
                traces: { receivers: ["otlp"], exporters: ["otlphttp/traces"] },
                metrics: { receivers: ["otlp", ..._pgReceivers(args.spec.profile.observe.ingest)], exporters: ["otlphttp/metrics"] },
                ...(profiled && { profiles: { receivers: ["otlp"], exporters: ["otlphttp/profiles"] } }),
              },
            },
          })),
      },
    }, this.child()).resources)
    this.rendered = pulumi.all(charts).apply(Array.flatten)
    this.collectorEndpoint = pulumi.interpolate`http://${collector}.${args.namespace}.svc:4318`
    this.seal({ urls: this.urls, collectorEndpoint: this.collectorEndpoint, rendered: this.rendered })
  }
}
```

## [04]-[SCRAPE_ROWS]

[SCRAPE_ROWS]:
- Owner: the `_pg`/`_pgReceivers` arm pair keyed by `spec.profile.observe.ingest` — the pg-server metrics arm under the one-ingress law: `scrape` models `postgres_exporter` as the collector's `prometheusreceiver` (one scrape job over the exporter's `/metrics`, the exporter running as one values-driven child beside the data tier with its DSN from the in-graph data read), and `native` is the OTLP-native alternative — the collector's `postgresqlreceiver` dialing the pooler directly, no exporter container at all.
- Law: one ingress holds for pull — the collector scrapes, never a store-side scrape config, so every signal enters through the one gateway and a store swap re-points exporters without touching ingestion; app telemetry never rides scrape — the arm exists exactly for infra surfaces that expose `/metrics` and cannot push.
- Law: the server-side depth is a database fact — the data tier's cluster config carries `pg_stat_statements` (with `compute_query_id`) and pg18 `pg_stat_io` as standing rows, so whichever arm runs, the series it harvests exist.
- Growth: a second infra exporter (node metrics) is one more scrape job row inside the same receiver.
- Boundary: the exporter image ref and DSN arrive as args from the composing arm; the data tier owns the server config rows.

```typescript
const _pg = (ingest: StackSpec.Observe["ingest"], dsn: pulumi.Input<string>) =>
  ingest === "scrape"
    ? {
        prometheus: {
          config: {
            scrape_configs: [{
              job_name: "postgres",
              static_configs: [{ targets: ["postgres-exporter:9187"] }],
            }],
          },
        },
      }
    : {
        postgresql: { endpoint: dsn }, // metrics BETA; the pg18 pg_stat_io byte/WAL depth rides the research row below
      }

const _pgReceivers = (ingest: StackSpec.Observe["ingest"]): ReadonlyArray<string> =>
  ingest === "scrape" ? ["prometheus"] : ["postgresql"]
```

## [05]-[DEV_ROW]

[DEV_ROW]:
- Owner: `Dev` — the docker arm's whole observability estate as one exported tier: the `_DEV` anchor carries the all-in-one image's two port planes (`edge` rows the host publishes, `query` rows the bundled Grafana reads container-locally), and the tier publishes the same `Lgtm.Urls` plane and `collectorEndpoint` the k8s tier publishes, so `provider.md`'s docker arm returns the `otlp` and `grafana` output planes from either producer.
- Law: the dev loop is byte-identical at the SDK seam — the app's export config is the one `OTEL_EXPORTER_OTLP_ENDPOINT` env row on both arms, so moving an app between loops edits zero telemetry config and a signal that renders in the dev pane renders in the estate pane.
- Law: the dev image is the dev arm's whole backend — the k8s arm never runs it; its bundled logs store differs from the estate row, a bounded asymmetry the query plane absorbs as one datasource row.
- Law: boards apply identically — `Boards` consumes `Dev.urls` exactly as it consumes `Lgtm.urls`, so the dev pane carries the same folder, sources, dashboards, and alert rules the estate carries, authenticated by the generated credential the container's admin env row seeds.
- Growth: a dev-loop port or knob is one `_DEV` field.
- Packages: `@pulumi/docker` (`Container`); `@pulumi/pulumi` (`interpolate`, `output`); `effect` (`Array`, `Record`).

```typescript
import * as docker from "@pulumi/docker"

const _DEV = {
  image: "grafana/otel-lgtm",
  edge: { grafana: 3000, otlpGrpc: 4317, otlpHttp: 4318, pyroscope: 4040 },
  query: { prometheus: 9090, loki: 3100, tempo: 3200, pyroscope: 4040 },
} as const

declare namespace Dev {
  type Args = {
    readonly image: pulumi.Input<string>
    readonly host: string
    readonly network: pulumi.Input<string>
    readonly auth: pulumi.Input<string>
  }
}

class Dev extends Tier {
  readonly urls: Lgtm.Urls
  readonly collectorEndpoint: pulumi.Output<string>
  constructor(name: string, args: Dev.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Dev", name, opts)
    new docker.Container(name, {
      image: args.image,
      restart: "unless-stopped",
      envs: [pulumi.interpolate`GF_SECURITY_ADMIN_PASSWORD=${args.auth}`], // the credential discipline holds in the loop: Boards authenticates with the same generated read
      ports: Array.map(Record.toEntries(_DEV.edge), ([, port]) => ({ internal: port, external: port })),
      networksAdvanced: [{ name: args.network }],
    }, this.child())
    const otlp = pulumi.output(`http://${args.host}:${_DEV.edge.otlpHttp}`)
    const local = (port: number) => pulumi.output(`http://localhost:${port}`) // query APIs resolve from the dev Grafana's own container; every backend shares its network namespace
    this.urls = {
      grafana: pulumi.output(`http://${args.host}:${_DEV.edge.grafana}`),
      ingest: { logs: otlp, traces: otlp, metrics: otlp, profiles: otlp }, // the bundled collector is the one ingest seam: every signal enters the same OTLP door
      query: { loki: local(_DEV.query.loki), tempo: local(_DEV.query.tempo), prometheus: local(_DEV.query.prometheus), pyroscope: local(_DEV.query.pyroscope) },
    }
    this.collectorEndpoint = otlp
    this.seal({ urls: this.urls, collectorEndpoint: this.collectorEndpoint })
  }
}
```

## [06]-[ENDPOINT_PROJECTION]

[ENDPOINT_PROJECTION]:
- Law: endpoints are one projection in two role planes — `_urls(release, namespace, store)` derives every backend address from the release name, the namespace, and the selected store row under each pinned chart's service-naming convention, with `ingest` rows carrying each backend's write path (Loki OTLP, Tempo OTLP, the store row's `write`, Pyroscope ingest) for the collector exporters and `query` rows carrying each backend's read API (Loki, Tempo, the store row's `read`, Pyroscope) for the Grafana data sources — one backend, two roles, never one URL doing both jobs; a chart bump that renames a service or moves a path edits exactly these rows and the store rows, and no consumer ever spells a service DNS.
- Law: consumers bind outputs, not literals — `urls.grafana` feeds the `Boards` provider and `StackOutputs.grafana`, `urls.query.*` feeds the `Boards` data sources, `collectorEndpoint` feeds the `otlp` output plane and thence the workload env; a hand-written URL anywhere downstream is the drift this projection deletes.
- Growth: a new backend's address is one ingest row and one query row beside its chart row.
- Boundary: ports and path suffixes are the pinned charts' facts, versioned with the pins.

```typescript
const _urls = (release: string, namespace: pulumi.Input<string>, store: (typeof _stores)[keyof typeof _stores]): Lgtm.Urls => ({
  grafana: pulumi.interpolate`http://${release}-grafana.${namespace}.svc`,
  ingest: {
    logs: pulumi.interpolate`http://${release}-loki.${namespace}.svc:3100/otlp`,
    traces: pulumi.interpolate`http://${release}-tempo.${namespace}.svc:4318`,
    metrics: store.write(`${release}-store`, namespace),
    profiles: pulumi.interpolate`http://${release}-pyroscope.${namespace}.svc:4040`,
  },
  query: {
    loki: pulumi.interpolate`http://${release}-loki.${namespace}.svc:3100`,
    tempo: pulumi.interpolate`http://${release}-tempo.${namespace}.svc:3200`,
    prometheus: store.read(`${release}-store`, namespace),
    pyroscope: pulumi.interpolate`http://${release}-pyroscope.${namespace}.svc:4040`,
  },
})
```

## [07]-[BOARD_APPLY]

[BOARD_APPLY]:
- Owner: `Boards` — the tier-constructed `grafana.Provider` (`url` from the `urls.grafana` plane either producer publishes, `auth` as the `user:password` form woven from the Doppler read, the transient-fault posture as data — `retries`, `retryStatusCodes`, `retryWait` — and `storeDashboardSha256: true` so dashboard drift diffs by hash instead of full JSON) and the apply fold: one `oss.Folder` roots the app's boards (uid slugged from the spec's app key), `_SOURCES` maps backend rows onto `oss.DataSource` constructions from the query URLs with the row key pinned as the datasource `uid`, each encoded `DashboardModel` compiles once through `_compiled` into one `oss.Dashboard` under the folder, `_alerted` compiles the `Alert.Spec` rows into one `alerting.RuleGroup` with severity-routed delivery, `_slos` compiles the suite's objectives into `slo.SLO` rows, tenant organizations realize org-scoped per `spec.tenants` slug, one `oss.Annotation` stamps the deployment identity, and the machine identity mints once.
- Law: the compile leg is the Foundation-SDK builder fold — `_compiled` decodes each model once, lands `uid`/`title`/`tags`/`refresh` member-for-member with `since` on `time`, folds every `variables` row through `withVariable` and every annotation row through `annotation` (slug as the name, tone as the marker color), and `.build()` emits the Grafana JSON `pulumi.jsonStringify` posts as `configJson` — compiled once per model and applied to every org that carries it.
- Law: the panel fold is an exhaustive record dispatch — `_minted` mints each laid panel through its tag's `Match.valueTags` arm carrying the tag's own payload (gauge `ceiling` on `max`, `steps` sorted onto the thresholds builder above the wire's mandatory `-Infinity` base row, `unit` where the row carries it, the Logs display rows on `showTime`/`wrapLogMessage`/`sortOrder`/`dedupStrategy`), and the shared fields land on the members every subpath inherits (`title`, `description`, `transparent`, `repeat`, `gridPos` from the model's own shelf fold) — a new panel tag fails compilation at the record until its arm exists.
- Law: targets ride the `_POSTURE` row — the row names the query module (`loki` for Logs, prometheus for the rest), the wire form (`table` for the Table/Geomap/Nodes facet frames, `heatmap` for Heatmap), and the instant flag; every panel binds its datasource `{ type, uid }` from the `_SOURCES` row key, every panel emits at least one target (Logs its `filter`, Nodes its `nodes`/`edges` pair), `legend` lands on `legendFormat`, and `exemplar(true)` rides only range series gated on the selected store row; the model rows the fold does not yet land ride the `[PANEL_OPTIONS]` research row.
- Law: the alert compile is total over the spec — `_expr` is the one `Match.valueTags` record fold from SLI case to PromQL, every arm a breach-rate `> factor × budget` verdict joined over the short and long windows with `and` through the `_burned` seam, the multiwindow guard that keeps a burst and a slow burn both honest: the `Ratio` case compiles the good-ratio complement, the `Latency` case the `le`-share complement at the spec's own `ceiling` over the bucket/count series, and the `Saturation`/`Freshness` cases the bool-comparison time shares — each spec becomes one rule whose `datas` pair a Prometheus query node with a `__expr__` threshold node, `for` derives from the severity row's `hold`, and the spec's `annotations`/`slug` ride the rule verbatim — the suite computes policy, this fold only spells it in the provider dialect, and a consumer re-deriving burn thresholds is the forked-discipline defect.
- Law: delivery routes by severity as data — the `contacts` record carries one receiver row per severity kind (`page`, `ticket`); each present row realizes one `alerting.ContactPoint`, one `NotificationPolicy` matcher route on the `severity` label, and one `alerting.MessageTemplate` rendering the spec's annotation keys into the notification body, and a row carrying a `quiet` calendar realizes one `alerting.MuteTiming` bound onto its route — paging posture, wording, and quiet hours are all contact-row data the spec's severity row keys.
- Law: SLOs compile from objectives, never from alerts — `_slos` maps the suite's own `Slo.Objective` values (name, target, window, SLI) onto `slo.SLO` rows with the plain error-ratio query, so the alert fold and the SLO fold read one upstream vocabulary and cannot disagree.
- Law: tenancy is organizations, realized org-scoped — one `oss.Organization` per `spec.tenants` slug with the per-tenant folder, source set, and board fleet threaded `orgId` from the realized org's own output, so a tenant's boards and sources scope to its org while the default org carries the operator fleet, alerts, and machine identity.
- Law: the deployment annotates itself — one `oss.Annotation` carries the deploy plane's time-ordered identity and stack coordinates as board-visible text, so every dashboard reads deploys against its own series; richer run evidence stays receipt material on the automation plane.
- Law: the machine identity is minted least-privilege — one `oss.ServiceAccount` (`role: "Editor"`) holds exactly the folder-Admin grant one `oss.FolderPermissionItem` lands, and one `oss.ServiceAccountRotatingToken` (rotation window as `_ROTATION` policy data, `deleteOnDestroy` so a torn-down stack leaves no live credential) realizes the durable automation credential; the token key egresses as the tier's `automation` output for the composing arm to land in a Doppler `{ value }` entry, and the chart-seeded `admin:password` binding remains the one in-graph provider auth.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect; auth never rides env here — the in-graph read is the canonical binding for deploy-time application.
- Entry: `new Boards("boards", { spec, urls, auth, boards, alerts, objectives, contacts, deploy }, opts)` — the k8s arm feeds `lgtm.urls`, the docker arm `dev.urls`; `boards`/`alerts`/`objectives` produced by the app's core observe suite call.
- Growth: a new panel family is one model row upstream with its `_minted` arm and `_POSTURE` row here; a new severity route is one `contacts` row; a new SLI case is one `_expr` match arm beside its upstream `Sli` case; a new tenant is one `spec.tenants` slug realizing its whole org-scoped fleet.
- Boundary: `DashboardModel`/`Alert`/`Slo` shapes are the core observe plane's owners consumed as encoded values; folder placement conventions live here, board content never does; drift interpretation is `operate/policy.md`'s.
- Packages: `@pulumiverse/grafana` (`Provider`, `oss.Folder`, `oss.DataSource`, `oss.Dashboard`, `oss.Organization`, `oss.Annotation`, `oss.ServiceAccount`, `oss.ServiceAccountRotatingToken`, `oss.FolderPermissionItem`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `alerting.MessageTemplate`, `alerting.MuteTiming`, `slo.SLO`); `@grafana/grafana-foundation-sdk` (`dashboard` `DashboardBuilder`/`QueryVariableBuilder`/`AnnotationQueryBuilder`/`ThresholdsConfigBuilder`/`ThresholdsMode`, the per-tag panel builders, `prometheus` `DataqueryBuilder`/`PromQueryFormat`, `loki` `DataqueryBuilder`, `common` `LogsSortOrder`/`LogsDedupStrategy`); `@rasm/ts/core` (`DashboardModel`, `Alert`, `Sli`, `Slo`); `effect` (`Array`, `Duration`, `Match`, `Option`, `Order`, `Record`, `Schema`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import { LogsDedupStrategy, LogsSortOrder } from "@grafana/grafana-foundation-sdk/common"
import {
  AnnotationQueryBuilder, DashboardBuilder, QueryVariableBuilder, ThresholdsConfigBuilder, ThresholdsMode,
} from "@grafana/grafana-foundation-sdk/dashboard"
import { DataqueryBuilder as LokiQuery } from "@grafana/grafana-foundation-sdk/loki"
import { DataqueryBuilder, PromQueryFormat } from "@grafana/grafana-foundation-sdk/prometheus"
import { PanelBuilder as Gauge } from "@grafana/grafana-foundation-sdk/gauge"
import { PanelBuilder as Geomap } from "@grafana/grafana-foundation-sdk/geomap"
import { PanelBuilder as Heatmap } from "@grafana/grafana-foundation-sdk/heatmap"
import { PanelBuilder as Logs } from "@grafana/grafana-foundation-sdk/logs"
import { PanelBuilder as Nodes } from "@grafana/grafana-foundation-sdk/nodegraph"
import { PanelBuilder as Stat } from "@grafana/grafana-foundation-sdk/stat"
import { PanelBuilder as Table } from "@grafana/grafana-foundation-sdk/table"
import { PanelBuilder as Timeseries } from "@grafana/grafana-foundation-sdk/timeseries"
import * as grafana from "@pulumiverse/grafana"
import { Alert, DashboardModel, type Sli, type Slo } from "@rasm/ts/core"
import { Array, Duration, Match, Option, Order, Record, Schema } from "effect"

declare namespace Boards {
  type Model = typeof DashboardModel.Encoded
  type Contacts = Partial<Record.ReadonlyRecord<"page" | "ticket", {
    readonly webhook: pulumi.Input<string>
    readonly quiet?: ReadonlyArray<{ readonly days: ReadonlyArray<string>; readonly start: string; readonly end: string }>
  }>>
  type Args = {
    readonly spec: StackSpec
    readonly urls: Lgtm.Urls
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Alert.Spec>
    readonly objectives: ReadonlyArray<Slo.Objective>
    readonly contacts: Contacts
    readonly deploy: { readonly id: pulumi.Input<string> }
  }
}

const _ROTATION = { live: Duration.toSeconds(Duration.days(7)), early: Duration.toSeconds(Duration.days(1)) } as const

const _SOURCES = {
  prometheus: { type: "prometheus", url: (urls: Lgtm.Urls) => urls.query.prometheus },
  loki: { type: "loki", url: (urls: Lgtm.Urls) => urls.query.loki },
  tempo: { type: "tempo", url: (urls: Lgtm.Urls) => urls.query.tempo },
  pyroscope: { type: "grafana-pyroscope-datasource", url: (urls: Lgtm.Urls) => urls.query.pyroscope },
} as const

const _byAt: Order.Order<{ readonly at: number }> = Order.mapInput(Order.number, (step) => step.at)

const _DEDUP = {
  exact: LogsDedupStrategy.Exact, none: LogsDedupStrategy.None, numbers: LogsDedupStrategy.Numbers, signature: LogsDedupStrategy.Signature,
} as const

const _POSTURE = {
  // per-tag emission posture: query module, wire form, instant flag — a new tag is one row beside its mint arm
  Gauge: { source: "prometheus" },
  Geomap: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Heatmap: { source: "prometheus", form: PromQueryFormat.Heatmap },
  Logs: { source: "loki" },
  Nodes: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Stat: { source: "prometheus" },
  Table: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Timeseries: { source: "prometheus" },
} as const satisfies Record<string, { readonly source: keyof typeof _SOURCES; readonly form?: PromQueryFormat; readonly instant?: boolean }>

const _stepped = <B extends { thresholds(builder: ThresholdsConfigBuilder): B }>(
  built: B,
  steps: ReadonlyArray<{ readonly at: number; readonly tone: string }>,
): B =>
  steps.length === 0 ? built : built.thresholds(new ThresholdsConfigBuilder().mode(ThresholdsMode.Absolute).steps([
    { value: null, color: "transparent" }, // the wire demands a -Infinity base row; transparent tones nothing below the first declared step
    ...Array.map(Array.sort(steps, _byAt), (step) => ({ value: step.at, color: step.tone })),
  ]))

const _united = <B extends { unit(unit: string): B }>(built: B, unit: Option.Option<string>): B =>
  Option.match(unit, { onNone: () => built, onSome: (value) => built.unit(value) })

const _minted = (panel: typeof DashboardModel.Panel.Type) =>
  // each tag's distinct payload lands at mint: a new panel tag fails compilation at this record until its arm exists
  Match.valueTags(panel, {
    Gauge: (row) => _stepped(new Gauge().max(row.ceiling), row.steps), // the gauge ceiling is the scale fact; the trip point rides its steps row
    Geomap: () => new Geomap(),
    Heatmap: (row) => _united(new Heatmap(), row.unit),
    Logs: (row) =>
      new Logs()
        .showTime(row.showTime)
        .wrapLogMessage(row.wrap)
        .sortOrder(row.order === "ascending" ? LogsSortOrder.Ascending : LogsSortOrder.Descending)
        .dedupStrategy(_DEDUP[row.deduplicate]),
    Nodes: () => new Nodes(),
    Stat: (row) => _united(_stepped(new Stat(), row.steps), row.unit),
    Table: () => new Table(),
    Timeseries: (row) => _united(_stepped(new Timeseries(), row.steps), row.unit),
  })

const _target = (
  posture: (typeof _POSTURE)[keyof typeof _POSTURE],
  expr: string,
  refId: string,
  exemplars: boolean,
  legend: Option.Option<string>,
) => {
  const legended = <B extends { legendFormat(format: string): B }>(row: B): B =>
    Option.match(legend, { onNone: () => row, onSome: (format) => row.legendFormat(format) })
  return posture.source === "loki"
    ? legended(new LokiQuery().expr(expr).refId(refId))
    : legended(
        "form" in posture
          ? "instant" in posture
            ? new DataqueryBuilder().expr(expr).refId(refId).format(posture.form).instant()
            : new DataqueryBuilder().expr(expr).refId(refId).format(posture.form)
          : new DataqueryBuilder().expr(expr).refId(refId).exemplar(exemplars), // exemplars ride only range series; a table or heatmap form has no sample stream to link
      )
}

const _compiled = (model: Boards.Model, exemplars: boolean) => {
  const decoded = Schema.decodeSync(DashboardModel)(model) // one admission: every builder member below reads the decoded owner, never the wire bag
  const board = new DashboardBuilder(decoded.title)
    .uid(decoded.uid)
    .tags([...decoded.tags])
    .refresh(`${Duration.toSeconds(decoded.refresh)}s`)
    .time({ from: `now-${Duration.toSeconds(decoded.since)}s`, to: "now" }) // the model's since lands; the Grafana-default range never renders
  for (const variable of decoded.variables) board.withVariable(new QueryVariableBuilder(variable.name).label(variable.label))
  for (const note of decoded.annotations) board.annotation(new AnnotationQueryBuilder().name(note.slug).iconColor(note.tone).enable(true))
  for (const { panel, position } of DashboardModel.laid(decoded)) { // the shelf fold's positions land as gridPos; no hand layout exists
    const posture = _POSTURE[panel._tag]
    const built = _minted(panel)
      .title(panel.title)
      .gridPos({ h: position.h, w: position.w, x: position.x, y: position.y })
      .transparent(panel.transparent)
      .datasource({ type: _SOURCES[posture.source].type, uid: posture.source }) // the uid pinned at the source row: one compiled JSON binds identically in every org
    if (Option.isSome(panel.description)) built.description(panel.description.value)
    if (Option.isSome(panel.repeat)) built.repeat(panel.repeat.value)
    const legend = "legend" in panel ? panel.legend : Option.none<string>()
    const exprs = "exprs" in panel ? panel.exprs : "expr" in panel ? [panel.expr] : "filter" in panel ? [panel.filter] : [panel.nodes, panel.edges]
    exprs.forEach((expr, at) => built.withTarget(_target(posture, expr, `A${at}`, exemplars, legend)))
    board.withPanel(built)
  }
  return board.build()
}

const _window = (input: Duration.DurationInput): string => `${Duration.toSeconds(Duration.decode(input))}s`

const _burned = (spec: Alert.Spec, breach: (window: Duration.DurationInput) => string): string =>
  Array.join(
    Array.map([spec.windows.short, spec.windows.long], (window) => `${breach(window)} > ${spec.factor * (1 - spec.target)}`),
    " and ",
  )

const _expr = (spec: Alert.Spec): string =>
  Match.valueTags(spec.sli, {
    Freshness: ({ horizon, metric }) =>
      _burned(spec, (window) => `avg_over_time((${metric} > bool ${Duration.toSeconds(horizon)})[${_window(window)}:])`),
    Latency: ({ ceiling, metric }) =>
      _burned(spec, (window) =>
        `(1 - (sum(rate(${metric}_bucket{le="${Duration.toSeconds(ceiling)}"}[${_window(window)}])) / sum(rate(${metric}_count[${_window(window)}]))))`),
    Ratio: ({ good, total }) =>
      _burned(spec, (window) => `(1 - (sum(rate(${good}[${_window(window)}])) / sum(rate(${total}[${_window(window)}]))))`),
    Saturation: ({ ceiling, metric }) =>
      _burned(spec, (window) => `avg_over_time((${metric} > bool ${ceiling})[${_window(window)}:])`),
  })

const _query = (sli: Sli): string =>
  Match.valueTags(sli, {
    Freshness: ({ horizon, metric }) => `avg_over_time((${metric} > bool ${Duration.toSeconds(horizon)})[$__rate_interval:])`,
    Latency: ({ metric, quantile }) => `histogram_quantile(${quantile}, sum by (le) (rate(${metric}_bucket[$__rate_interval])))`,
    Ratio: ({ good, total }) => `sum(rate(${good}[$__rate_interval])) / sum(rate(${total}[$__rate_interval]))`,
    Saturation: ({ ceiling, metric }) => `avg_over_time((${metric} > bool ${ceiling})[$__rate_interval:])`,
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
    new grafana.alerting.MessageTemplate(severity, {
      name: severity,
      template: `{{ define "${severity}" }}{{ .CommonAnnotations.SortedPairs }}{{ end }}`, // the spec's annotation keys render; wording is contact-row data
    }, child)
    const quiet = row.quiet === undefined ? undefined
      : new grafana.alerting.MuteTiming(severity, {
          name: `${severity}-quiet`,
          intervals: Array.map(row.quiet, (window) => ({ weekdays: [...window.days], times: [{ start: window.start, end: window.end }] })),
        }, child)
    return { severity, point, quiet }
  })
  Array.match(routes, {
    onEmpty: () => undefined,
    onNonEmpty: ([head, ...rest]) =>
      new grafana.alerting.NotificationPolicy("routing", {
        contactPoint: head.point.name,
        groupBies: ["alertname"],
        policies: Array.map([head, ...rest], ({ severity, point, quiet }) => ({
          contactPoint: point.name,
          matchers: [{ label: "severity", match: "=", value: severity }],
          ...(quiet !== undefined && { muteTimings: [quiet.name] }),
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
): ReadonlyArray<grafana.slo.SLO> =>
  Array.map(objectives, (objective) =>
    new grafana.slo.SLO(objective.name, {
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
      url: args.urls.grafana,
      auth: pulumi.interpolate`admin:${args.auth}`,
      retries: 3,
      retryStatusCodes: ["429", "5xx"],
      retryWait: 5,
      storeDashboardSha256: true,
    }, { parent: this })
    const child = this.child({ provider })
    const exemplars = _stores[args.spec.profile.observe.store].exemplars
    const compiled = Array.map(args.boards, (model) => ({
      uid: model.uid,
      json: pulumi.jsonStringify(_compiled(model, exemplars)), // one compile serves the default org and every tenant org
    }))
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    const sources = Record.map(_SOURCES, (row, key) =>
      new grafana.oss.DataSource(key, { type: row.type, uid: key, url: row.url(args.urls) }, child)) // uid = the row key: the ref every compiled panel binds
    Array.map(compiled, (board) =>
      new grafana.oss.Dashboard(board.uid, { configJson: board.json, folder: folder.uid }, child))
    _alerted(args.alerts, { folder: folder.uid, datasource: sources.prometheus.uid, contacts: args.contacts }, child)
    _slos(args.objectives, folder.uid, child)
    Array.map(args.spec.tenants, (tenant) => {
      const org = new grafana.oss.Organization(tenant, { name: tenant }, child)
      const scope = org.orgId.apply(String) // org-scoped args take Input<string>; the realized org's number renders exactly here
      const home = new grafana.oss.Folder(`${tenant}-${name}`, { title: args.spec.app, uid: `${tenant}-${args.spec.app}`, orgId: scope }, child)
      Record.map(_SOURCES, (row, key) =>
        new grafana.oss.DataSource(`${tenant}-${key}`, { type: row.type, uid: key, url: row.url(args.urls), orgId: scope }, child)) // uid is org-scoped: the same key per org is what lets one compiled JSON bind everywhere
      return Array.map(compiled, (board) =>
        new grafana.oss.Dashboard(`${tenant}-${board.uid}`, { configJson: board.json, folder: home.uid, orgId: scope }, child))
    })
    new grafana.oss.Annotation(`${name}-deploy`, {
      text: pulumi.interpolate`deploy ${args.deploy.id} ${args.spec.name}`, // board-visible deploy marker; run evidence stays receipt material
      tags: ["deploy", args.spec.app],
    }, child)
    const account = new grafana.oss.ServiceAccount(`${name}-automation`, { name: `${name}-automation`, role: "Editor" }, child)
    new grafana.oss.FolderPermissionItem(`${name}-automation`, {
      folderUid: folder.uid,
      permission: "Admin",
      user: account.id, // the grant scopes the Editor-role identity to exactly the app folder
    }, child)
    this.automation = new grafana.oss.ServiceAccountRotatingToken(`${name}-automation`, {
      namePrefix: `${name}-automation`,
      serviceAccountId: account.id,
      secondsToLive: _ROTATION.live,
      earlyRotationWindowSeconds: _ROTATION.early,
      deleteOnDestroy: true,
    }, child).key
    this.seal({ folder: folder.uid, automation: this.automation })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Boards, Dev, Lgtm }
```

## [08]-[RESEARCH]

- [STORE_CHARTS]-[OPEN]: exact chart names, rendered service DNS, and ingest/query path suffixes for the `prometheus`/`mimir-distributed`/`victoria-metrics-single` rows at the pinned versions, and each row's retention values key; verify against each repository's helm index and rendered manifests before the store fences settle.
- [PG_RECEIVER]-[OPEN]: whether the pinned collector's `postgresqlreceiver` surfaces pg18 `pg_stat_io` byte and WAL columns; verify against the collector-contrib receiver documentation — until confirmed, that depth rides a `sqlqueryreceiver` custom-query row beside the `native` arm.
- [DEV_PORTS]-[OPEN]: query-plane listen ports of the all-in-one dev image (`prometheus`/`loki`/`tempo` rows on `_DEV.query`); verify against the pinned image's published service manifest before the dev fences settle.
- [PANEL_OPTIONS]-[OPEN]: builder member spellings for the model rows `_minted` does not yet land — `_PanelFields` axes/interaction/links/transformations, Table sort and pagination, the Geomap layer mapping, the nodegraph frame-option members; verify against the foundation-sdk module declarations, then fold each verified member beside its mint arm.
