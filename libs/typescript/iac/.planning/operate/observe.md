# [IAC_OBSERVE]

Observability realization in three tiers. `Lgtm` installs the k8s backend estate: one closed metrics-store row family (`prometheus | mimir | victoriametrics`) selected by `spec.profile.observe.store`, the Loki/Tempo/Pyroscope/Grafana backends beside it, and the OpenTelemetry collector as the one ingest seam aiming per-signal exporters at the selected rows through `Output`-woven URLs. `Dev` is the docker arm's estate as one all-in-one container publishing the same URL plane. `Boards` applies the core observe plane's identity-derived outputs whole against either producer's URL plane.

Board content is code and the UI is drift: `storeDashboardSha256: true` diffs dashboards by content hash, and the provider carries the transient-fault posture as data. Workloads learn only the collector endpoint through the env row, so a backend re-plumb — a store swap included — never touches an app. `iac/src/operate/observe.ts` is the module; a new backend is one chart row with its endpoint projections, a new store is one `_stores` row, a new dashboard is one encoded model, a new alert is upstream spec data compiled by the same fold, and a new producer provenance key is one `_PACKS` row over already core-encoded boards and alerts.

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
- Owner: the interior `_stores` family — one row per metrics store carrying `chart`/`repo`, the `write` (OTLP/remote-write ingest path) and `read` (query API path) projections, the `retain` values projection (the row's own retention dialect key), the `exemplars` column, the `tenancy` column (`label | org`), and the `degrade` declaration — with `spec.profile.observe.store` selecting the row and `spec.profile.observe.retention` flowing through the row's `retain` projection; the family is Mimir-SHAPED: every coordinate an escalation needs is a column on every row, so store selection edits the spec, never a tier.
- Law: `prometheus` is the reference row — its decisive column is `exemplars: true`: native exemplar storage (`--enable-feature=exemplar-storage` and native histograms in the row's values) powers the metric→trace click-through into Tempo that the whole board plane links on; tenant stays the `rasm.tenant` label (`tenancy: "label"`), the rendered server service is `<release>-prometheus-server`, retention rides `server.retention`, and its OTLP receiver stamps `Convention.wire.translation` under the chart's `serverFiles["prometheus.yml"].otlp` key so ingested dotted names survive with type and unit suffixes.
- Law: `mimir` is the fleet escalation — multi-component and memory-heavy, earned only past the single-store ceiling; its object-store binding reuses the object plane's endpoint and bucket coordinates through `structuredConfig.common.storage` (`backend: s3` with the endpoint, credential, and bucket rows — one storage truth, never a second store config) with `blocks_storage.s3.bucket_name`/`ruler_storage.s3.bucket_name` scoping the per-section prefixes, retention rides `structuredConfig.limits.compactor_blocks_retention_period`, the nginx gateway service is `<release>-mimir-nginx` (ingest `location /otlp/v1/metrics`, query under `/prometheus`), its ruler runs in-store so the burn rules the `Boards` fold applies can escalate to store-side evaluation as one values row, and `tenancy: "org"` stamps the stack's org id as the `X-Scope-OrgID` header on the collector's metrics exporter.
- Law: `victoriametrics` is the resource-pressure escape — `exemplars: false` is its declared degradation: the metric→trace click-through drops to trace-search-by-time, a posture the row states so selecting it is an informed trade, never a surprise; the rendered service is `<release>-victoria-metrics-single-server` on `:8428`, retention rides `server.retentionPeriod`, and every other projection column holds.
- Law: degradation is a row declaration — each row's `degrade` column states what the estate loses on that row, so the store decision reads as data at the spec seam and the dashboards' exemplar links gate on the selected row's `exemplars` column.
- Growth: a fourth store is one row with every column answered — the family is closed until a row lands.
- Boundary: which store a stack runs is `program/spec.md`'s `observe.store` coordinate; the tenant metric label is the runtime plane's `Convention.rasm.tenant` dimension arriving on the wire, never re-minted here.

```typescript
const _stores = {
  prometheus: {
    chart: "prometheus", repo: "https://prometheus-community.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-prometheus-server.${ns}.svc/api/v1/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-prometheus-server.${ns}.svc`,
    retain: (window: string) => ({ server: { retention: window } }),
    exemplars: true, tenancy: "label",
    degrade: "single-store; tenant is a label, never an isolation boundary",
  },
  mimir: {
    chart: "mimir-distributed", repo: "https://grafana.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-mimir-nginx.${ns}.svc/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-mimir-nginx.${ns}.svc/prometheus`,
    retain: (window: string) => ({ mimir: { structuredConfig: { limits: { compactor_blocks_retention_period: window } } } }),
    exemplars: true, tenancy: "org",
    degrade: "multi-component memory cost; earned only past the single-store ceiling",
  },
  victoriametrics: {
    chart: "victoria-metrics-single", repo: "https://victoriametrics.github.io/helm-charts",
    write: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-victoria-metrics-single-server.${ns}.svc:8428/opentelemetry`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-victoria-metrics-single-server.${ns}.svc:8428`,
    retain: (window: string) => ({ server: { retentionPeriod: window } }),
    exemplars: false, tenancy: "label",
    degrade: "no exemplar storage: metric→trace click-through degrades to trace search",
  },
} as const satisfies Record<string, {
  readonly chart: string
  readonly repo: string
  readonly write: (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>
  readonly read: (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>
  readonly retain: (window: string) => Record<string, unknown>
  readonly exemplars: boolean
  readonly tenancy: "label" | "org"
  readonly degrade: string
}>
```

## [03]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries the signal-backend rows (`loki`, `tempo`, `grafana`, `pyroscope` — the profiles row, present while `spec.profile.observe.profiles` holds — and `collector`) beside the posture rows (`opencost` while `spec.profile.observe.costs` holds, `ebpf` while `spec.profile.observe.ebpf` holds), the selected `_stores` row lands beside them, versions arrive as pinned args, and the tier constructs every chart under one namespace with the collector's exporters aimed at the backends by `Output`-woven URLs; every chart renders as stack children under Pulumi diff and CrossGuard validation, and `rendered` aggregates the child sets as the tier's discovery projection — real render evidence, never chart-name guesses.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `Boards` authenticates with the same value; persistence, replica, retention, and pipeline knobs are value rows under each pinned chart's own dialect, drifting only with the version pin.
- Law: provenance rides the pins — when a `keyring` asset accompanies the versions, every chart row verifies (`verify: true` + the keyring), so a tampered chart fails at render and the estate's content-addressed discipline extends to its chart supply.
- Law: the collector is the one ingest seam — the OTLP receiver admits, one named exporter per signal fans out (`otlphttp/logs` to Loki's OTLP ingest, `otlphttp/traces` to Tempo, `otlphttp/metrics` to the selected store row's `write` path with the org header where the row's tenancy column demands it, `otlphttp/profiles` to Pyroscope while the profiles row holds), and the `service.pipelines` rows wire receiver to exporter per signal; workloads never learn a backend address, only the collector endpoint, and the exporter `sending_queue` persists through the `file_storage` extension so a backend restart drops nothing.
- Law: the collector reports itself — `service.telemetry.metrics.readers` carries one `periodic` reader whose `otlp` exporter dials the collector's own OTLP door over `http/protobuf`, so gateway health, queue depth, and export failure land in the selected store as first-class series and the ingest seam is never a blind spot.
- Law: the estate prices itself — the `opencost` row aims the exporter at the selected store row's `read` URL through `opencost.prometheus.external.{enabled,url}`, cost series scope by namespace and the `rasm.tenant` label the tenancy owners already stamp, and cost boards compile through the standing `_compiled` fold into the default and tenant orgs; the docker arm declares its degrade — container stats without a Kubernetes allocation feed carry no `opencost` cell, so the dev loop prices nothing and states so.
- Law: SDK-less workloads earn RED metrics as a chart row — the `ebpf` row installs the OpenTelemetry eBPF instrumentation chart (`opentelemetry-ebpf-instrumentation` from the collector's own repository), and `config.data.otel_traces_export.endpoint` with `config.data.otel_metrics_export.endpoint` bind explicitly to the tier's one `collectorEndpoint`; the row demands privileged eBPF host access, so it binds only where the deploy target grants it and the toggle is spec data, never a default.
- Law: profiles ride the push path — Pyroscope ingests the runtime SDKs' push streams and the collector's profiles pipeline; the row is present-by-default and its removal is a spec delta, so the LGTM plane carries four signals, not three.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and no row here does.
- Entry: `new Lgtm("observe", { spec, namespace, versions, auth, dsn, objects, keyring? }, opts)` inside the k8s arm, `objects` the object plane's coordinates the mimir row binds.
- Growth: a new signal backend is one `_charts` row with its endpoint projections; a collector pipeline axis is one values row.
- Boundary: the app-side OTLP export composition is the runtime telemetry plane's and arrives only as the env row; board content is `[06]`'s upstream data.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `@pulumi/pulumi` (`Input`, `Output`, `interpolate`, `all`, `asset`); `effect` (`Array`); `@rasm/ts/core` (`Convention`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Tier, type StackSpec } from "../program/spec.ts"

const _charts = {
  collector: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts" },
  ebpf: { chart: "opentelemetry-ebpf-instrumentation", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts" },
  grafana: { chart: "grafana", repo: "https://grafana.github.io/helm-charts" },
  loki: { chart: "loki", repo: "https://grafana.github.io/helm-charts" },
  opencost: { chart: "opencost", repo: "https://opencost.github.io/opencost-helm-chart" },
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
    const collector = `${name}-otel`
    const collectorEndpoint = pulumi.interpolate`http://${collector}.${args.namespace}.svc:4318`
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
    if (args.spec.profile.observe.costs) {
      row("opencost", {
        opencost: { prometheus: { external: { enabled: true, url: this.urls.query.prometheus } } }, // the exporter reads the selected store row; a store swap re-points cost series with zero opencost edits
      })
    }
    if (args.spec.profile.observe.ebpf) {
      row("ebpf", {
        config: {
          data: {
            otel_traces_export: { endpoint: collectorEndpoint },
            otel_metrics_export: { endpoint: collectorEndpoint },
          },
        },
      })
    }
    charts.push(new k8s.helm.v4.Chart(`${name}-store`, {
      chart: store.chart,
      repositoryOpts: { repo: store.repo },
      version: args.versions.store,
      namespace: args.namespace,
      ...provenance,
      values: {
        ...store.retain(args.spec.profile.observe.retention), // retention rides the row's own dialect key; drifts only with the pin
        ...(args.spec.profile.observe.store === "prometheus" && {
          server: {
            retention: args.spec.profile.observe.retention,
            extraFlags: ["enable-feature=exemplar-storage", "enable-feature=native-histograms", "web.enable-otlp-receiver"],
          },
          serverFiles: { "prometheus.yml": { otlp: { translation_strategy: Convention.wire.translation } } }, // the one owned OTLP-receiver strategy: dotted names survive with type/unit suffixes
        }),
        ...(args.spec.profile.observe.store === "mimir" && args.objects !== undefined && {
          mimir: {
            structuredConfig: {
              // one storage truth: the object plane's endpoint and bucket bind every mimir storage section
              common: {
                storage: {
                  backend: "s3",
                  s3: { endpoint: args.objects.endpoint, bucket_name: args.objects.bucket },
                },
              },
              blocks_storage: { s3: { bucket_name: args.objects.bucket } },
              ruler_storage: { s3: { bucket_name: args.objects.bucket } },
              limits: { compactor_blocks_retention_period: args.spec.profile.observe.retention },
            },
          },
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
              telemetry: {
                metrics: {
                  readers: [{
                    // Gateway self-telemetry uses its OTLP door: queue depth and export failure are store series, never a blind spot.
                    periodic: { exporter: { otlp: { protocol: "http/protobuf", endpoint: "http://localhost:4318" } } },
                  }],
                },
              },
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
    this.collectorEndpoint = collectorEndpoint
    this.seal({ urls: this.urls, collectorEndpoint: this.collectorEndpoint, rendered: this.rendered })
  }
}
```

## [04]-[SCRAPE_ROWS]

[SCRAPE_ROWS]:
- Owner: the `_pg`/`_pgReceivers` arm pair keyed by `spec.profile.observe.ingest` — the pg-server metrics arm under the one-ingress law: `scrape` models `postgres_exporter` as the collector's `prometheusreceiver` (one scrape job over the exporter's `/metrics`, the exporter running as one values-driven child beside the data tier with its DSN from the in-graph data read), and `native` is the OTLP-native alternative — the collector's `postgresqlreceiver` dialing the pooler directly, no exporter container at all.
- Law: one ingress holds for pull — the collector scrapes, never a store-side scrape config, so every signal enters through the one gateway and a store swap re-points exporters without touching ingestion; app telemetry never rides scrape — the arm exists exactly for infra surfaces that expose `/metrics` and cannot push.
- Law: the `postgresqlreceiver` carries block and WAL depth (`postgresql.blks_hit`/`postgresql.blks_read`, `postgresql.wal.age`/`postgresql.wal.delay`) and no pg18 `pg_stat_io` byte or WAL columns, so that depth rides the `sqlquery` receiver row beside the `native` arm — one custom query over `pg_stat_io` emitting read/write/extend byte series per backend type — and both receivers join the metrics pipeline as one arm.
- Law: CNPG operator metrics enter the same door — a `cnpg` scrape job rides the `prometheusreceiver` on both arms (`kubernetes_sd_configs` pod role narrowed to the `cnpg.io/cluster` label over the instances' `:9187` metrics listener), so operator and instance health land in the selected store under the one-ingress law and no store-side scrape config exists.
- Law: the server-side depth is a database fact — the data tier's cluster config carries `pg_stat_statements` (with `compute_query_id`) and pg18 `pg_stat_io` as standing rows, so whichever arm runs, the series it harvests exist.
- Growth: a second infra exporter (node metrics) is one more scrape job row inside the same receiver.
- Boundary: the exporter image ref and DSN arrive as args from the composing arm; the data tier owns the server config rows.

```typescript
const _CNPG_JOB = {
  // operator + instance health under the one-ingress law: pod discovery narrowed to the cluster label, the instances' metrics listener
  job_name: "cnpg",
  kubernetes_sd_configs: [{ role: "pod" }],
  relabel_configs: [
    { source_labels: ["__meta_kubernetes_pod_label_cnpg_io_cluster"], action: "keep", regex: ".+" },
    { source_labels: ["__address__"], action: "replace", regex: "([^:]+)(?::\\d+)?", replacement: "$1:9187", target_label: "__address__" },
  ],
} as const

const _pg = (ingest: StackSpec.Observe["ingest"], dsn: pulumi.Input<string>) =>
  ingest === "scrape"
    ? {
        prometheus: {
          config: {
            scrape_configs: [
              { job_name: "postgres", static_configs: [{ targets: ["postgres-exporter:9187"] }] },
              _CNPG_JOB,
            ],
          },
        },
      }
    : {
        postgresql: { endpoint: dsn },
        sqlquery: {
          // PG18 pg_stat_io depth absent from the native receiver uses one custom query with byte series per backend type.
          driver: "postgres",
          datasource: dsn,
          queries: [{
            sql: "SELECT backend_type, sum(reads * op_bytes) AS read_bytes, sum(writes * op_bytes) AS write_bytes, sum(extends * op_bytes) AS extend_bytes FROM pg_stat_io GROUP BY backend_type",
            metrics: [
              { metric_name: "postgresql.io.read_bytes", value_column: "read_bytes", attribute_columns: ["backend_type"], value_type: "int" },
              { metric_name: "postgresql.io.write_bytes", value_column: "write_bytes", attribute_columns: ["backend_type"], value_type: "int" },
              { metric_name: "postgresql.io.extend_bytes", value_column: "extend_bytes", attribute_columns: ["backend_type"], value_type: "int" },
            ],
          }],
        },
        prometheus: { config: { scrape_configs: [_CNPG_JOB] } },
      }

const _pgReceivers = (ingest: StackSpec.Observe["ingest"]): ReadonlyArray<string> =>
  ingest === "scrape" ? ["prometheus"] : ["postgresql", "sqlquery", "prometheus"]
```

## [05]-[DEV_ROW]

[DEV_ROW]:
- Owner: `Dev` — the docker arm's whole observability estate as one exported tier: the `_DEV` anchor carries the all-in-one image's two port planes (`edge` rows the host publishes, `query` rows the bundled Grafana reads container-locally), and the tier publishes the same `Lgtm.Urls` plane and `collectorEndpoint` the k8s tier publishes, so `provider.md`'s docker arm returns the `otlp` and `grafana` output planes from either producer.
- Law: the dev loop is byte-identical at the SDK seam — the app's export config is the one `OTEL_EXPORTER_OTLP_ENDPOINT` env row on both arms, so moving an app between loops edits zero telemetry config and a signal that renders in the dev pane renders in the estate pane.
- Law: the dev image is the dev arm's whole backend — the k8s arm never runs it; its bundled logs store differs from the estate row, a bounded asymmetry the query plane absorbs as one datasource row; the image publishes the edge rows (Grafana `3000`, OTLP `4317`/`4318`, Tempo `3200`, Pyroscope `4040`, Prometheus `9090`) while Loki's `3100` listener stays container-local, which is exactly the posture the `query` rows encode — the bundled Grafana reads them from inside the container's own network namespace.
- Law: the dev loop prices nothing — no allocation feed exists on a docker daemon, so the `costs` toggle has no dev realization and the degrade is stated here, never discovered on an empty board.
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
- Owner: `Boards` — the tier-constructed `grafana.Provider` (`url` from the `urls.grafana` plane either producer publishes, `auth` as the `user:password` form woven from the Doppler read, the transient-fault posture as data — `retries`, `retryStatusCodes`, `retryWait` — and `storeDashboardSha256: true` so dashboard drift diffs by hash instead of full JSON) and the apply fold: one `oss.Folder` roots the app's boards (uid slugged from the spec's app key), `_SOURCES` maps backend rows onto `oss.DataSource` constructions from the query URLs with the row key pinned as the datasource `uid`, each encoded `DashboardModel` compiles once through `_compiled` into one `oss.Dashboard` under the folder, producer packs join their already core-encoded boards and alerts into the same folds under a `_PACKS` provenance key, `library` rows realize `oss.LibraryPanel` shared panels through the same `_minted` fold, `_alerted` compiles the `Alert.Spec` rows into one `alerting.RuleGroup` with severity-routed delivery, `_slos` compiles the suite's objectives into `slo.SLO` rows, tenant organizations realize org-scoped per `spec.tenants` slug with a viewer identity and pinned home board each, one `oss.Annotation` stamps the deployment identity beside the fleet-roll annotation rows, and the machine identity mints once.
- Law: the compile leg is the Foundation-SDK builder fold — `_compiled` decodes each model once, lands `uid`/`title`/`tags`/`refresh` member-for-member with `since` on `time`, lands the decoded variable array through `variables` and every annotation row through `annotation` (slug as the name, tone as the marker color), and `.build()` emits the Grafana JSON `pulumi.jsonStringify` posts as `configJson` — compiled once per model and applied to every org that carries it.
- Law: the verified panel fold is an exhaustive record dispatch — `_minted` composes gauge ceilings and thresholds, units, Logs display rows, and the Nodes mapping through the cataloged generic transformation member; `_compiled` lands shared title, description, transparency, repetition, layout, targets, and transformations. Panel links, Geomap mapping, Table sort/pagination, and Timeseries axes/tooltip stay outside settled code until their exact builder rows enter the catalog.
- Law: targets ride the `_POSTURE` row — the row names the query module (`loki` for Logs, prometheus for the rest), the wire form (`table` for the Table/Geomap/Nodes facet frames, `heatmap` for Heatmap), the instant flag, and the Logs `maxLines` ceiling the loki query carries as policy; every panel binds its datasource `{ type, uid }` from the `_SOURCES` row key, every panel emits at least one target (Logs its `filter`, Nodes its `nodes`/`edges` pair), `legend` lands on `legendFormat`, and `exemplar(true)` rides only range series gated on the selected store row.
- Law: boards publish read-only — `_compiled` stamps `.readonly()` on every board because content is code and the UI is drift, the hash-diff provider posture and this presentation row are one law from two sides; a shared panel is one `library` row compiled through the same `_minted` fold into an `oss.LibraryPanel` `modelJson`, realized in the default org and every tenant org so a panel compiled once serves many boards.
- Law: producer packs ride one ingest arm — `_PACKS` is the closed provenance tuple, and each pack arrives as `{ wire, boards, alerts }` whose boards and alerts are already core-encoded values; the arm tags every compiled board with its wire and folds pack alerts into the same `_alerted` burn-rate leg. Producer-specific census translation belongs to its composition owner and is absent here; a tuple entry cannot substitute for that projection. A new provenance key is one tuple entry, and an unknown wire fails at the typed boundary.
- Law: the alert compile is total over the spec — `_expr` folds each spec through `_burned`, joining the short and long breach-rate `> factor × budget` verdicts with `and` (the multiwindow guard that keeps a burst and a slow burn both honest), while the breach spelling itself is core-owned: `Query.breach(spec.sli, window)` renders the quoted dotted series and folds the `Latency` `le` bound through `Convention.duration`, so this fold never reconstructs `${metric}_bucket`/`${metric}_count` nor re-derives units and the board burn panels and these alert rules read one breach projection — each spec becomes one rule whose `datas` pair a Prometheus query node with a `__expr__` threshold node, `for` derives from the severity row's `hold`, and the spec's `annotations`/`slug` ride the rule verbatim — the suite computes policy, this fold only spells it in the provider dialect, and a consumer re-deriving burn thresholds is the forked-discipline defect.
- Law: delivery routes by severity as data — the `contacts` record carries one receiver row per severity kind (`page`, `ticket`); each present row realizes one `alerting.ContactPoint`, one `NotificationPolicy` matcher route on the `severity` label, and one `alerting.MessageTemplate` whose body carries the row's wording lead, runbook link, and sorted annotation pairs; a row carrying a `quiet` calendar realizes one `alerting.MuteTiming` bound onto its route.
- Law: tenant read identity is org-scoped — each tenant org mints one viewer `oss.ServiceAccount` (`role: "Viewer"`, threaded `orgId`), one `oss.ServiceAccountPermissionItem` granting the operator team `Admin` custody over that identity, one `oss.OrganizationPreferences` pinning the tenant's first compiled board as the org home, and one `oss.ServiceAccountRotatingToken` under the same `_ROTATION` policy whose key egresses on the tier's `viewers` record for the composing arm to land in Doppler `{ value }` entries — tenant credentials ride Doppler custody exactly like the automation token, never a stack output.
- Law: fleet rolls annotate beside deploys — each `rolls` row is the AppHost roll wire consumed as data (`wave`, `channel`, `verdict`, `hosts`), realized as one `oss.Annotation` whose text carries the roll coordinates and whose tone rides the `_ROLL_TONES` verdict row so a rollback reads as loud as an advance on every board; the record shape is the AppHost mint, this fold never re-derives roll facts.
- Law: SLOs compile from objectives, never from alerts — `_slos` maps the suite's own `Slo.Objective` values (name, target, window, SLI) onto `slo.SLO` rows whose SLI observable `_query` renders through the same core `Query` family the panels and alerts use, so every dotted series quotes and reads its native histogram and no store-series string is hand-spelled; the alert fold and the SLO fold read one upstream vocabulary and cannot disagree.
- Law: tenancy is organizations, realized org-scoped — one `oss.Organization` per `spec.tenants` slug with the per-tenant folder, source set, and board fleet threaded `orgId` from the realized org's own output, so a tenant's boards and sources scope to its org while the default org carries the operator fleet, alerts, and machine identity.
- Law: the deployment annotates itself — one `oss.Annotation` carries the deploy plane's time-ordered identity and stack coordinates as board-visible text, so every dashboard reads deploys against its own series; richer run evidence stays receipt material on the automation plane.
- Law: the machine identity is minted least-privilege — one `oss.ServiceAccount` (`role: "Editor"`) holds exactly the folder-Admin grant one `oss.FolderPermissionItem` lands, and one `oss.ServiceAccountRotatingToken` (rotation window as `_ROTATION` policy data, `deleteOnDestroy` so a torn-down stack leaves no live credential) realizes the durable automation credential; the token key egresses as the tier's `automation` output for the composing arm to land in a Doppler `{ value }` entry, and the chart-seeded `admin:password` binding remains the one in-graph provider auth.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect; auth never rides env here — the in-graph read is the canonical binding for deploy-time application.
- Entry: `new Boards("boards", { spec, urls, auth, boards, packs, library, alerts, objectives, contacts, deploy, rolls }, opts)` — the k8s arm feeds `lgtm.urls`, the docker arm `dev.urls`; `boards`/`alerts`/`objectives` produced by the app's core observe suite call, `packs` by the producer censuses, `rolls` by the AppHost fleet ledger.
- Growth: a new panel family is one model row upstream with its `_minted` arm and `_POSTURE` row here; a new severity route is one `contacts` row; a new SLI case is one `_expr` match arm beside its upstream `Sli` case; a new tenant is one `spec.tenants` slug realizing its whole org-scoped fleet, viewer identity included; a new producer census is one `_PACKS` row; a new shared panel is one `library` row.
- Boundary: `DashboardModel`/`Alert`/`Slo` shapes are the core observe plane's owners consumed as encoded values; pack censuses and the roll wire are their producers' mints consumed as data; folder placement conventions live here, board content never does; drift interpretation is `operate/policy.md`'s.
- Packages: `@pulumiverse/grafana` (`Provider`, `oss.Folder`, `oss.DataSource`, `oss.Dashboard`, `oss.LibraryPanel`, `oss.Organization`, `oss.OrganizationPreferences`, `oss.Annotation`, `oss.ServiceAccount`, `oss.ServiceAccountRotatingToken`, `oss.ServiceAccountPermissionItem`, `oss.FolderPermissionItem`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `alerting.MessageTemplate`, `alerting.MuteTiming`, `slo.SLO`); `@grafana/grafana-foundation-sdk` (`dashboard` `DashboardBuilder`/`AnnotationQueryBuilder`/`ThresholdsConfigBuilder`/`ThresholdsMode`, the per-tag panel builders, `prometheus` `DataqueryBuilder`/`PromQueryFormat`, `loki` `DataqueryBuilder`, `common` `LogsSortOrder`/`LogsDedupStrategy`); `@rasm/ts/core` (`Alert`, `Convention`, `DashboardModel`, `Query`, `Sli`, `Slo`); `effect` (`Array`, `Duration`, `Match`, `Option`, `Order`, `Record`, `Schema`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import { LogsDedupStrategy, LogsSortOrder } from "@grafana/grafana-foundation-sdk/common"
import { AnnotationQueryBuilder, DashboardBuilder, ThresholdsConfigBuilder, ThresholdsMode } from "@grafana/grafana-foundation-sdk/dashboard"
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
import { Alert, Convention, DashboardModel, Query, type Sli, type Slo } from "@rasm/ts/core"
import { Array, Duration, Match, Option, Order, Record, Schema } from "effect"

declare namespace Boards {
  type Model = typeof DashboardModel.Encoded
  type Wire = (typeof _PACKS)[number]
  type Pack = {
    readonly wire: Wire // the producer census this pack carries; the schema stays the producer's mint
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Alert.Spec>
  }
  type Library = { readonly name: string; readonly panel: typeof DashboardModel.Panel.Encoded }
  type Roll = {
    readonly wave: number
    readonly channel: string
    readonly verdict: keyof typeof _ROLL_TONES
    readonly hosts: number
  }
  type Contacts = Partial<Record.ReadonlyRecord<"page" | "ticket", {
    readonly webhook: pulumi.Input<string>
    readonly wording?: string
    readonly runbook?: string
    readonly quiet?: ReadonlyArray<{ readonly days: ReadonlyArray<string>; readonly start: string; readonly end: string }>
  }>>
  type Args = {
    readonly spec: StackSpec
    readonly urls: Lgtm.Urls
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly packs?: ReadonlyArray<Pack>
    readonly library?: ReadonlyArray<Library>
    readonly alerts: ReadonlyArray<Alert.Spec>
    readonly objectives: ReadonlyArray<Slo.Objective>
    readonly contacts: Contacts
    readonly deploy: { readonly id: pulumi.Input<string> }
    readonly rolls?: ReadonlyArray<Roll>
  }
}

const _ROTATION = { live: Duration.toSeconds(Duration.days(7)), early: Duration.toSeconds(Duration.days(1)) } as const

const _PACKS = [
  "security.audit",
  "runtime.pulse",
  "grasshopper.fan",
  "compute.descriptor",
  "fabrication.slo",
  "persistence.census",
  "geometry.charter",
] as const

const _ROLL_TONES = { advanced: "green", rolledBack: "red" } as const // a rollback reads as loud as an advance

const _TRANSFORMS = {
  // model transform tags onto Grafana transformer ids; options stay the row's own payload
  Calculate: "calculateField",
  Filter: "filterByValue",
  Group: "groupBy",
  Join: "joinByField",
  Organize: "organize",
  Reduce: "reduce",
} as const

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
  // per-tag emission posture: query module, wire form, instant flag, log ceiling — a new tag is one row beside its mint arm
  Gauge: { source: "prometheus" },
  Geomap: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Heatmap: { source: "prometheus", form: PromQueryFormat.Heatmap },
  Logs: { source: "loki", maxLines: 1000 },
  Nodes: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Stat: { source: "prometheus" },
  Table: { source: "prometheus", form: PromQueryFormat.Table, instant: true },
  Timeseries: { source: "prometheus" },
} as const satisfies Record<typeof DashboardModel.Panel.Type["_tag"], {
  readonly source: keyof typeof _SOURCES
  readonly form?: PromQueryFormat
  readonly instant?: boolean
  readonly maxLines?: number
}>

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
  Match.value(panel).pipe(Match.tagsExhaustive({
    Gauge: (row) => _stepped(new Gauge().max(row.ceiling), row.steps), // the gauge ceiling is the scale fact; the trip point rides its steps row
    Geomap: () => new Geomap(),
    Heatmap: (row) => _united(new Heatmap(), row.unit),
    Logs: (row) =>
      new Logs()
        .showTime(row.showTime)
        .wrapLogMessage(row.wrap)
        .sortOrder(row.order === "ascending" ? LogsSortOrder.Ascending : LogsSortOrder.Descending)
        .dedupStrategy(_DEDUP[row.deduplicate]),
    Nodes: () => new Nodes(), // the mapping row lands as the Organize rename in _fielded: the node graph reads conventional frame columns
    Stat: (row) => _united(_stepped(new Stat(), row.steps), row.unit),
    Table: () => new Table(),
    Timeseries: (row) => _united(_stepped(new Timeseries(), row.steps), row.unit),
  }))

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
    ? legended(new LokiQuery().expr(expr).refId(refId).maxLines("maxLines" in posture ? posture.maxLines : 1000)) // the log ceiling is posture policy, never a per-board knob
    : legended(
        "form" in posture
          ? "instant" in posture
            ? new DataqueryBuilder().expr(expr).refId(refId).format(posture.form).instant()
            : new DataqueryBuilder().expr(expr).refId(refId).format(posture.form)
          : new DataqueryBuilder().expr(expr).refId(refId).exemplar(exemplars), // exemplars ride only range series; a table or heatmap form has no sample stream to link
      )
}

const _compiled = (model: Boards.Model, exemplars: boolean) => {
  // BOUNDARY ADAPTER: Foundation SDK builders mutate their own drafts; this function contains that imperative contract.
  const decoded = Schema.decodeSync(DashboardModel)(model) // one admission: every builder member below reads the decoded owner, never the wire bag
  const board = new DashboardBuilder(decoded.title)
    .uid(decoded.uid)
    .tags([...decoded.tags])
    .readonly() // content is code and the UI is drift: the hash-diff provider posture and this presentation row are one law
    .refresh(`${Duration.toSeconds(decoded.refresh)}s`)
    .time({ from: `now-${Duration.toSeconds(decoded.since)}s`, to: "now" }) // the model's since lands; the Grafana-default range never renders
  board.variables([...decoded.variables])
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
    for (const transform of panel.transformations) built.withTransformation({ id: _TRANSFORMS[transform._tag], options: transform }) // the tag selects the transformer id; the row is its own options payload
    if (panel._tag === "Nodes") {
      built.withTransformation({
        // Mapping renames onto the conventional frame columns the node graph reads; identity stays model data.
        id: _TRANSFORMS.Organize,
        options: { order: [], rename: { [panel.mapping.nodeId]: "id", [panel.mapping.edgeSource]: "source", [panel.mapping.edgeTarget]: "target" } },
      })
    }
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
  // Core Query.breach quotes dotted series and folds the Latency le bound through Convention.duration.
  _burned(spec, (window) => Query.render(Query.breach(spec.sli, Query.span(Duration.decode(window)))))

const _rateSum = (metric: Convention.MetricName): Query =>
  Query.Aggregate({ by: [], of: Query.Windowed({ fn: "rate", of: Query.Instant({ labels: {}, metric }), window: Query.interval.rate }), op: "sum" })

const _query = (sli: Sli): string =>
  // SLI observables render through core Query; dotted series quote and Latency reads native buckets.
  Query.render(
    Match.value(sli).pipe(Match.tagsExhaustive({
      Freshness: ({ horizon, metric }) =>
        Query.Windowed({ fn: "avg", of: Query.Binary({ left: Query.Instant({ labels: {}, metric }), op: "gt", right: Query.Const({ value: Query.finite(Duration.toMillis(horizon) / 1000) }) }), window: Query.interval.rate }),
      Latency: ({ metric, quantile }) => Query.Quantile({ labels: {}, metric, q: Query.quantile(quantile), window: Query.interval.rate }),
      Ratio: ({ good, total }) => Query.Binary({ left: _rateSum(good), op: "div", right: _rateSum(total) }),
      Saturation: ({ ceiling, metric }) =>
        Query.Windowed({ fn: "avg", of: Query.Binary({ left: Query.Instant({ labels: {}, metric }), op: "gt", right: Query.Const({ value: Query.finite(ceiling) }) }), window: Query.interval.rate }),
    })),
  )

const _alerted = (
  alerts: ReadonlyArray<Alert.Spec>,
  bind: {
    readonly folder: pulumi.Output<string>
    readonly datasource: pulumi.Output<string>
    readonly contacts: Boards.Contacts
  },
  child: pulumi.CustomResourceOptions,
): void => {
  const routes = Array.map(Record.toEntries(bind.contacts), ([severity, row]) => {
    const point = new grafana.alerting.ContactPoint(severity, {
      name: severity,
      webhooks: [{ url: row.webhook }],
    }, child)
    new grafana.alerting.MessageTemplate(severity, {
      name: severity,
      // per-severity wording leads, the runbook link follows, the spec's sorted annotation pairs close: the whole body is contact-row data
      template: `{{ define "${severity}" }}${row.wording ?? ""}${row.runbook === undefined ? "" : ` runbook: ${row.runbook}`} {{ .CommonAnnotations.SortedPairs }}{{ end }}`,
    }, child)
    const quiet = row.quiet === undefined ? undefined
      : new grafana.alerting.MuteTiming(severity, {
          name: `${severity}-quiet`,
          intervals: Array.map(row.quiet, (window) => ({ weekdays: [...window.days], times: [{ start: window.start, end: window.end }] })),
        }, child)
    return { severity, row, point, quiet }
  })
  Array.match(routes, {
    onEmpty: () => undefined,
    onNonEmpty: ([head, ...rest]) =>
      new grafana.alerting.NotificationPolicy("routing", {
        contactPoint: head.point.name,
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
  readonly viewers: Record.ReadonlyRecord<string, pulumi.Output<string>>
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
    const ingested = Array.flatMap(args.packs ?? [], (pack) =>
      Array.map(pack.boards, (model) => ({ ...model, tags: [...model.tags, pack.wire] })))
    const compiled = Array.map([...args.boards, ...ingested], (model) => ({
      uid: model.uid,
      json: pulumi.jsonStringify(_compiled(model, exemplars)), // one compile serves the default org and every tenant org
    }))
    const alerts = [...args.alerts, ...Array.flatMap(args.packs ?? [], (pack) => pack.alerts)] // pack burn rows join the one rule fold
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    const sources = Record.map(_SOURCES, (row, key) =>
      new grafana.oss.DataSource(key, { type: row.type, uid: key, url: row.url(args.urls) }, child)) // uid = the row key: the ref every compiled panel binds
    Array.map(compiled, (board) =>
      new grafana.oss.Dashboard(board.uid, { configJson: board.json, folder: folder.uid }, child))
    const shelf = Array.map(args.library ?? [], (row) => ({
      row,
      json: pulumi.jsonStringify(_minted(Schema.decodeSync(DashboardModel.Panel)(row.panel)).title(row.name).build()),
    }))
    Array.map(shelf, ({ row, json }) =>
      new grafana.oss.LibraryPanel(row.name, { name: row.name, folderUid: folder.uid, modelJson: json }, child)) // compiled once, served to every board that references it
    _alerted(alerts, { folder: folder.uid, datasource: sources.prometheus.uid, contacts: args.contacts }, child)
    _slos(args.objectives, folder.uid, child)
    const operator = new grafana.oss.ServiceAccount(`${name}-automation`, { name: `${name}-automation`, role: "Editor" }, child)
    this.viewers = Record.fromEntries(Array.map(args.spec.tenants, (tenant) => {
      const org = new grafana.oss.Organization(tenant, { name: tenant }, child)
      const scope = org.orgId.apply(String) // org-scoped args take Input<string>; the realized org's number renders exactly here
      const home = new grafana.oss.Folder(`${tenant}-${name}`, { title: args.spec.app, uid: `${tenant}-${args.spec.app}`, orgId: scope }, child)
      Record.map(_SOURCES, (row, key) =>
        new grafana.oss.DataSource(`${tenant}-${key}`, { type: row.type, uid: key, url: row.url(args.urls), orgId: scope }, child)) // uid is org-scoped: the same key per org is what lets one compiled JSON bind everywhere
      const fleet = Array.map(compiled, (board) =>
        new grafana.oss.Dashboard(`${tenant}-${board.uid}`, { configJson: board.json, folder: home.uid, orgId: scope }, child))
      Array.map(shelf, ({ row, json }) =>
        new grafana.oss.LibraryPanel(`${tenant}-${row.name}`, { name: row.name, folderUid: home.uid, modelJson: json, orgId: scope }, child))
      const viewer = new grafana.oss.ServiceAccount(`${tenant}-viewer`, { name: `${tenant}-viewer`, role: "Viewer", orgId: scope }, child)
      new grafana.oss.ServiceAccountPermissionItem(`${tenant}-viewer`, {
        serviceAccountId: viewer.id,
        permission: "Admin",
        user: operator.id, // custody of the tenant identity stays with the operator identity, never the tenant
      }, child)
      Array.match(fleet, {
        onEmpty: () => undefined,
        onNonEmpty: ([overview]) =>
          new grafana.oss.OrganizationPreferences(`${tenant}-home`, { orgId: scope, homeDashboardUid: overview.uid }, child), // the org opens on its own overview board
      })
      const key = new grafana.oss.ServiceAccountRotatingToken(`${tenant}-viewer`, {
        namePrefix: `${tenant}-viewer`,
        serviceAccountId: viewer.id,
        secondsToLive: _ROTATION.live,
        earlyRotationWindowSeconds: _ROTATION.early,
        deleteOnDestroy: true,
      }, child).key
      return [tenant, key] as const // Doppler custody rides the composing arm: the key never becomes a stack output
    }))
    new grafana.oss.Annotation(`${name}-deploy`, {
      text: pulumi.interpolate`deploy ${args.deploy.id} ${args.spec.name}`, // board-visible deploy marker; run evidence stays receipt material
      tags: ["deploy", args.spec.app],
    }, child)
    Array.map(args.rolls ?? [], (roll) =>
      new grafana.oss.Annotation(`${name}-roll-${roll.wave}`, {
        text: `roll ${roll.wave} ${roll.channel} ${roll.verdict} hosts=${roll.hosts}`, // the AppHost wire consumed as data; tone rides the verdict row
        tags: ["fleet-roll", _ROLL_TONES[roll.verdict], args.spec.app],
      }, child))
    new grafana.oss.FolderPermissionItem(`${name}-automation`, {
      folderUid: folder.uid,
      permission: "Admin",
      user: operator.id, // the grant scopes the Editor-role identity to exactly the app folder
    }, child)
    this.automation = new grafana.oss.ServiceAccountRotatingToken(`${name}-automation`, {
      namePrefix: `${name}-automation`,
      serviceAccountId: operator.id,
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

- [PYROSCOPE_PANEL]-[BLOCKED]: which exported Foundation SDK panel builder and profile-query target compile a Pyroscope flamegraph without a raw JSON panel; route through `libs/typescript/iac/.api/grafana-grafana-foundation-sdk.md`, and arm only when exact module, constructor, builder, and target declarations are cataloged, then restore the core `DashboardModel` profile row and this compile arm together.
- [FOUNDATION_PANEL_MEMBERS]-[BLOCKED]: which Foundation SDK declarations compose panel links, Geomap frame mapping, Table sort and pagination, and Timeseries axes, scale, and tooltip; route through `libs/typescript/iac/.api/grafana-grafana-foundation-sdk.md`, and arm a field only when its module, constructor, member, argument builder, and terminal type are cataloged, then restore its `_compiled` or `_minted` arm and close IDEAS `[BOARD_MEMBER_CATALOGS]` with TASKLOG `[FOUNDATION_PANEL_ROWS]`.
- [NOTIFICATION_GROUPING]-[BLOCKED]: which exact `NotificationPolicyArgs` field carries Grafana grouping keys, including tenant identity, at the pinned provider row; route through `libs/typescript/iac/.api/pulumiverse-grafana.md`, and restore contact `groupBy` data only when the catalog names the field and its input shape, then close TASKLOG `[GRAFANA_POLICY_FIELD]` with IDEAS `[BOARD_MEMBER_CATALOGS]`.
