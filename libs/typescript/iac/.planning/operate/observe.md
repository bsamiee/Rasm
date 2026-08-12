# [IAC_OBSERVE]

Observability realization in three tiers. `Lgtm` installs the k8s backend estate: one closed metrics-store row family (`prometheus | mimir | victoriametrics`) selected by `spec.profile.observe.store`, the Loki/Tempo/Pyroscope/Grafana backends beside it, one closed analytics-residence family selected by `spec.profile.observe.analytics`, and the OpenTelemetry collector as the one ingest seam — a Schema-decoded config document aiming per-signal exporters at the selected rows through `Output`-woven URLs. `Dev` is the docker arm's estate as one all-in-one container publishing the same URL plane. `Boards` applies the core observe plane's identity-derived outputs whole against either producer's URL plane.

Board content is code and the UI is drift: `storeDashboardSha256: true` diffs dashboards by content hash, and the provider carries transient-fault posture as data. Workloads learn only the collector endpoint through the env row, so a backend re-plumb, store swap included, never touches an app. Series retention answers alerting and residence retention answers audit, so evidence outlives the window a store row holds. `iac/src/operate/observe.ts` is the module; a new backend is one chart row with its pinned name and endpoint projections, a new gateway component one `_plan` row and one pipeline mention, a new pull-side surface one `_SCRAPED` row, a new store one `_stores` row, a new residence one `_RESIDENCE` row, an escalation one `_Observe` value, a new dashboard one encoded model, a new alert upstream spec data on the same fold, and a new provenance key one `_PACKS` row a landed producer earns.

## [01]-[INDEX]

- [02]-[STORE_ROWS]: the closed metrics-store family with tenancy/rules/retention/degradation; `Lgtm`.
- [03]-[CHART_ROWS]: the backend and residence chart rows, pinned names, decoded collector; `Lgtm`.
- [04]-[SCRAPE_ROWS]: the pull-side plane — the pg-server arm pair and the `_SCRAPED` job roster under the one-ingress law; `Lgtm`.
- [05]-[DEV_ROW]: the docker-arm estate as one all-in-one tier publishing the URL plane; `Dev`.
- [06]-[ENDPOINT_PROJECTION]: the ingest, query, residence, and collector rows consumers bind; `Lgtm`.
- [07]-[BOARD_APPLY]: provider, folder, sources, the builder compile leg, alerts, RBAC; `Boards`.

## [02]-[STORE_ROWS]

[STORE_ROWS]:
- Owner: the interior `_stores` family — one row per metrics store carrying `chart`/`repo`, its floor `admit` as the OTLP entry path beside the `read` query-API projection, ONE `values` projection folding retention, translation, exemplar storage, ruler, and object binding into the row's whole values block, the `plugin` column naming the Grafana datasource its query API answers, the `exemplars` and `rules` columns, and the shared `_Plane` floor (`fits`, `admit`, `signals`, `tenancy`, `lifetime`, `degrade`) — with `spec.profile.observe.store` selecting the row; the family is Mimir-SHAPED: every coordinate an escalation needs is a column on every row, so store selection edits the spec, never a tier.
- Law: one row, one values body — a store's dialect keys nest differently per chart (`server.*`, `mimir.structuredConfig.*`, `server.extraArgs.*`), so per-concern projections spread as sibling objects silently drop every key sharing a top-level parent; the row answers the whole block once and a coordinate added to the family obligates an answer on every row before it compiles.
- Law: `_Plane` seats every coordinate BOTH families answer, `admit` among them under a per-family shape parameter, and `signals` CENSUSES what a plane HOLDS rather than what any one writer routes into it — `_stores` rows hold `metrics` because a TSDB answers health and alerting, a residence filled by the gateway alone holds the two unbounded-attribute signals that leg carries, and a residence a data-branch writer also fills holds every relation that writer plants; each family extends that floor with what only its own plane decides.
- Law: crossing the two families reads different VALUES under one column set, never a second shape — `tenancy` is the one floor column each plane spells in its own closed vocabulary, since a series plane isolates by label or org while a wide-event plane isolates by partition column or sort-key lead, and one vocabulary spanning both names a mechanism neither plane runs.
- Law: an `org` row scopes its READ plane exactly as it scopes its ingest — `_scoped` is the one header projection, the collector stamping it on every exporter whose backend reads it and the board plane's own datasource stamping it on every query and every alert evaluation; a row isolating ingest while leaving the read plane unscoped provisions a door whose every request refuses for a missing scope, which reads on the board as an empty panel rather than as the tenancy posture the row declared.
- Law: `plugin` is a row column, never a tier constant — every row's query API answers a named Grafana datasource, so a row whose engine ships its own plugin and its own query dialect selects it here and the board plane's provisioning follows the selection; deriving the plugin from the reference row binds every escalation to whatever the reference row happened to answer.
- Law: translation is a family column, never a prometheus footnote — Tier-0 pins the receiver strategy unqualified by store so dotted names survive byte-identical from every runtime, the `translation` column carries each row's answer as a `Convention.Translation` value, and the row's own values body spells it in its own dialect: prometheus under `serverFiles["prometheus.yml"].otlp.translation_strategy`, mimir under `structuredConfig.limits.otel_translation_strategy` beside the `utf8` name-validation scheme and the suffix toggle its own validator panics on when they disagree, victoriametrics as the absence of `-opentelemetry.usePrometheusNaming` because the flag off IS the identity transform there.
- Law: histogram representation is a family column, never an algebra default — an OTLP exponential histogram lands as ONE native series where the row arms it and as `le`-bearing buckets where it cannot, and the quantile and fraction arms render entirely differently across that split, so each row states its answer, arms its own dialect for it (`server.otlp.convert_histograms_to_nhcb` on the reference row, `limits.native_histograms_ingestion_enabled` beside the protobuf query-response format on the fleet row, the resource-pressure row declaring the absence on `degrade`), and `_promql` carries it into the target; taking the algebra's own default renders bucket arithmetic against a store holding no buckets, which matches nothing and raises nothing.
- Law: the reference row arms representation at the RECEIVER, not on a feature flag — the pinned server accepts `enable-feature=native-histograms` and warns that it does nothing, so a row spelling it states a posture it never arms while the scrape-side knob that flag once fronted (`scrape_native_histograms`) governs a path the one-ingress law closes; the OTLP receiver's own `convert_histograms_to_nhcb` is what decides the split, left off for the native answer and turned on for the classic one.
- Law: this family's overrides nest per row and a flat key is a silent no-op — the reference row's chart declares no top-level `fullnameOverride` and invokes its own `prometheus.fullname` helper from zero templates, so the LIVE override is `server.fullnameOverride` and it becomes the rendered Service name outright, with no `-server` tail and port 80 onto the container's 9090; a flat spelling renames nothing and every address derived from the pinned name then resolves to a Service the chart never rendered, which is exactly the dead-address class the pinned-fullname law exists to close.
- Law: a store row owns its chart's whole workload set AND its whole default behaviour set — the reference chart ships FOUR subcharts default-on (alert manager, kube-state-metrics, node exporter, pushgateway), each a workload this estate never declared and the first of them injecting an `alerting` block beside the delivery path `Boards` owns, while the fleet row ships a bundled object store and a bundled Kafka; every one is disarmed by name under the same whole-page law that deletes Loki's gateway and canary tiers.
- Law: a default that is not a workload still installs a plane — the reference chart's own `scrapeConfigs` seat carries a cluster-wide kubernetes-SD job set (apiservers, nodes, cadvisor, service endpoints, pods) whose helpers keep naming the four disabled subcharts, so a row silent on that seat runs a SECOND ingest plane inside the store the one-ingress law exists to make the only one, against targets the same row just deleted; the seat empties explicitly, because the collector is the estate's one scraper and a store that discovers its own targets makes the store swap a re-plumb.
- Law: `prometheus` is the reference row — its decisive column is `exemplars: true`: native exemplar storage (`enable-feature=exemplar-storage`, spelled without leading dashes because the chart renders each flag as `- --{{ . }}`) powers the metric→trace click-through into Tempo that the whole board plane links on; tenant stays the `rasm.tenant` label (`tenancy: "label"`), the rendered server Service is the pinned `server.fullnameOverride` verbatim, and retention rides `server.retention`.
- Law: the exemplar column names TWO knobs and the row states both — the feature flag opens the store while the chart's own `server.exemplars` seat sizes it, and that seat ships empty, which renders no `storage.exemplars` block at all and leaves the decisive column riding whatever ring the server itself defaults to; the row spells the bound so a server release moving that default cannot resize the click-through plane every board links on, and it spells it at the dedicated seat, because `storage` under `serverFiles` is one of the keys the synthesized document already emits.
- Law: a store row holding its series on LOCAL disk states that claim in its own values body — both single-binary charts already ship the claim armed, so the row is not arming storage but RECORDING its size, since a chart bump moving that default silently resizes the plane every alert evaluates against; the fleet row states none, because its blocks land on the object plane whose horizon its own compactor column already ends.
- Law: the receiver block has ONE seat — `server.otlp` is the canonical knob and the chart also dumps `serverFiles["prometheus.yml"]` verbatim into the same rendered ConfigMap key, so spelling `otlp` in both emits two `otlp:` keys in one YAML document; the same trap governs `global`, `storage`, `alerting`, and the remote pair, and the row therefore writes only rule CONTENT under `serverFiles`, where setting one sub-key merges into the chart's default document and leaves its own `rule_files` list — which is what mounts that content — intact.
- Law: `mimir` is the fleet escalation — multi-component and memory-heavy, earned only past the single-store ceiling; its object-store binding reuses the object plane's endpoint and bucket coordinates through `structuredConfig.common.storage` (`backend: s3` with the endpoint and bucket rows, which every per-section store inherits — one storage truth, never a second store config) with `blocks_storage.s3.bucket_name`/`ruler_storage.s3.bucket_name` scoping the per-section prefixes, the two bundles this chart ships default-on both disarmed by name — `minio` so the escalation binds the estate's object plane instead of standing up a second one, and `kafka` because the buffer axis dials brokers the spec supplies and an in-chart broker is one nothing produces to — retention rides `structuredConfig.limits.compactor_blocks_retention_period`, the nginx gateway service is the pinned store fullname suffixed `-nginx` (ingest `location /otlp/v1/metrics`, query under `/prometheus`), `ruler.enabled` states the in-store rule component so a chart-default flip cannot silently drop store-side burn evaluation, and `tenancy: "org"` stamps the stack's org id as the `X-Scope-OrgID` header on every collector exporter whose backend reads it.
- Law: `victoriametrics` is the resource-pressure escape — `exemplars: false` is its declared degradation: the metric→trace click-through drops to trace-search-by-time, a posture the row states so selecting it is an informed trade, never a surprise; the rendered service is the pinned store fullname suffixed `-server` on `:8428`, retention rides `server.retentionPeriod`, and its translation column carries the second declared asymmetry — the store keeps dotted names and appends no type or unit suffix, so a suffix-bearing series exists on the other two rows alone.
- Law: rule evaluation rides a family column — burn numerators precompute once per group interval instead of once per alert evaluation, so `rules` rides the row and each dialect spells its own loader: prometheus reads `serverFiles["recording_rules.yml"]`, whose `groups` the chart's own `rule_files` default already mounts; mimir points `ruler_storage` at the `local` backend over a projected ConfigMap keyed `<tenant>/rules.yaml`, deliberately overriding the s3 backend `common.storage` supplies because a code-owned rule set is read-only by design and the API-backed bucket admits UI mutation; victoriametrics runs no in-store evaluator on this chart and carries that on `degrade`, so the row that cannot evaluate falls back to inline breach rendering rather than dropping the alert.
- Law: Grafana-managed recording rules are refused, not deferred — `alerting.v0alpha1.RecordingRule` writes `spec.metric` into `spec.targetDatasourceUid` through that datasource's Prometheus remote-write API, and remote-write is declined estate-wide, so arming that resource re-opens the door the one-ingress law shuts; the store's own evaluator takes the capability instead, which also keeps it a residence mechanism rather than a new executor.
- Law: degradation is a row declaration WITH A SEAT — each row's `degrade` column states what the estate loses on that row, the dashboards' exemplar links gate on the selected row's `exemplars` column, and the selected row itself seals on the tier beside the resident residence rows; a `_Plane` floor whose evidence half publishes and whose series half does not leaves half the trade unreadable while both halves read complete on the page.
- Growth: a fourth store is one row with every column answered — the family is closed until a row lands; Thanos and Cortex stay outside it because reference, fleet-scale, and resource-pressure are the three postures a metrics store is selected for and neither adds a fourth.
- Boundary: which store a stack runs is `program/spec.md`'s `observe.store` coordinate; the tenant metric label is the runtime plane's `Convention.rasm.tenant` dimension arriving on the wire, never re-minted here; remote-write is declined estate-wide, so OTLP is the one ingest door and that is what makes these rows swappable.
- Packages: `@pulumi/pulumi` (`Input`, `Output`, `interpolate`); `@rasm/ts/core` (`Board`, `Convention`); `../program/spec.ts` (`StackSpec`).

```typescript signature
// One signal vocabulary, two readers: the census a plane answers on the floor below and the routing bound a gateway
// leg answers on its own column. Profiles enter neither family — that backend holds its own plane — so the alphabet
// is the three OTLP signals a store or a residence can hold.
type _Signal = "logs" | "metrics" | "traces"

// Both backend-row families answer this floor under the estate's own column spellings. `signals` CENSUSES what the
// plane HOLDS, so a plane gaining a relation is a value edit and never a second family, and a reader asking what it
// may join across reads this column rather than inferring contents from whichever branch owns the writer; what a
// gateway leg WRITES is the exporter column's own bound, because one plane can be filled by two owners. `tenancy`
// stays each plane's own closed vocabulary, since a series plane and a wide-event plane isolate by mechanisms neither
// can spell for the other. `lifetime` carries the ending OWNER beside the window, because a window with no owner
// reads as a promise this tier made about a store's own compactor. `admit` seats ON the floor as its own parameter
// rather than in each family's extension: every family answers it — a store row as the OTLP path a collector
// exporter dials, a residence row as the writer that fills it — so it earns the floor by the whole-family test, and
// only its SHAPE is per-family. Declared twice below the floor instead, one question read as two coordinates a
// reader crossing families could not line up.
type _Plane<Isolation extends string, Admit> = {
  readonly fits: string
  readonly admit: Admit
  readonly signals: ReadonlyArray<_Signal>
  readonly tenancy: Isolation
  readonly lifetime: string
  readonly degrade: string
}

declare namespace Lgtm {
  // one rule set, three loaders: the groups are the payload every dialect carries, the mount coordinates are
  // what a row reads when its loader is a filesystem rather than a values key.
  type Rules = {
    readonly groups: ReadonlyArray<{
      readonly name: string
      readonly interval: string
      readonly rules: ReadonlyArray<{ readonly record: string; readonly expr: string; readonly labels: Record.ReadonlyRecord<string, string> }>
    }>
    readonly configMap: pulumi.Input<string>
    readonly directory: string
    readonly tenant: string
  }
  // every column the family declares arrives in one record, so a row cannot answer a coordinate it never received
  type Store = {
    readonly fullname: string
    readonly retention: StackSpec.Window
    readonly rules: Rules
    readonly translation: Convention.Translation
    readonly histogram: Board.Query.Histogram
    readonly objects?: Lgtm.Args["objects"]
  }
}

// Local-disk coordinates the two single-binary rows share. `claim` is ONE number because both rows hold the same
// series plane at the same retention window, so a per-row size prices one escalation against the other's horizon;
// each chart already ships its claim armed, which makes stating it a record of the SIZE rather than an arming.
// `exemplars` is the reference row's alone — the ring is a single in-memory circular buffer the store trims by
// eviction, so it bounds memory rather than the retention window, and the number is the server's own default made
// explicit so the click-through plane cannot resize under a release that moves it.
const _SERIES = { claim: "50Gi", exemplars: 100_000 } as const

const _stores = {
  prometheus: {
    chart: "prometheus", repo: "https://prometheus-community.github.io/helm-charts",
    // The rendered Service IS `server.fullnameOverride` — no `-server` tail survives the override — and it
    // answers port 80 onto the container's 9090, so the address carries no port either.
    admit: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}.${ns}.svc/api/v1/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}.${ns}.svc`,
    values: (row: Lgtm.Store) => ({
      // This chart's scaffold NESTS the override: its top-level `fullnameOverride` is absent from values and its
      // `prometheus.fullname` helper is invoked by zero templates, so a flat key here renames nothing and every
      // address derived from the pinned name resolves to a Service that was never rendered under it.
      server: {
        fullnameOverride: row.fullname,
        retention: row.retention,
        // Flags carry no leading dashes here — the chart renders each as `- --{{ . }}`. `native-histograms` is
        // NOT on this list: the pinned server accepts the feature name and warns that it does nothing, so the
        // representation column arms through the OTLP receiver's own conversion knob below instead.
        extraFlags: ["enable-feature=exemplar-storage", "web.enable-otlp-receiver"],
        // `server.otlp` is the canonical seat and `serverFiles["prometheus.yml"]` is dumped verbatim into the
        // SAME ConfigMap key, so spelling `otlp` in both emits two `otlp:` keys in one YAML document. The
        // representation answer rides here because it is a receiver decision: leaving the conversion off keeps an
        // OTLP exponential histogram as one native series, and turning it on lands the `le`-bearing buckets the
        // classic arm renders against.
        otlp: { translation_strategy: row.translation, convert_histograms_to_nhcb: row.histogram === "classic" },
        // That flag above OPENS exemplar storage and this seat SIZES it. Chart defaults ship this seat empty, and
        // an empty seat renders no `storage.exemplars` block at all, which leaves the row's own decisive column
        // riding whatever ring the server itself defaults to. It seats here rather than under `serverFiles` for the
        // duplicate-key reason `otlp` does: `storage` is a key the chart already synthesizes into that document.
        exemplars: { max_exemplars: _SERIES.exemplars },
        // Chart defaults already arm this claim at their own size, so this row RECORDS the size rather than arming
        // volume: a bump moving that default resizes the plane every alert evaluates against with nothing to raise.
        persistentVolume: { enabled: true, size: _SERIES.claim },
      },
      // Four subcharts ship default-ON — an alert manager, kube-state-metrics, a node exporter, and a pushgateway
      // — none of which this estate declared: alerting is the board plane's, node and object state arrive through
      // the collector's own enrichment, and nothing pushes. The alertmanager row also injects an `alerting` block
      // into the rendered config, so leaving it on wires a delivery path beside the one `Boards` owns.
      alertmanager: { enabled: false },
      "kube-state-metrics": { enabled: false },
      "prometheus-node-exporter": { enabled: false },
      "prometheus-pushgateway": { enabled: false },
      // The dedicated scrape seat, emptied: this chart's default job set is a whole cluster-wide kubernetes-SD
      // ingest plane — apiservers, nodes, cadvisor, service endpoints, pods — beside jobs the parent's own helpers
      // still name for the four subcharts just disabled above. Leaving it stands a SECOND ingest plane against the
      // one-ingress law this page states everywhere else, consumes the discovery RBAC nothing granted it, and pins
      // four permanently-down targets. Helm replaces lists, so the empty seat is the deletion, and the collector's
      // own `prometheus` receiver stays the estate's one scraper.
      scrapeConfigs: [],
      // Only the rule content lands: setting one sub-key merges into the chart's default `prometheus.yml` map, so
      // its own `rule_files` list survives; replacing that key whole would drop the path this content mounts at.
      serverFiles: { "recording_rules.yml": { groups: row.rules.groups } },
    }),
    fits: "reference posture: one store, native exemplars, the click-through plane every board links on",
    signals: ["metrics"], plugin: "prometheus", lifetime: "`server.retention` on the single server, which the store's own retention loop ends",
    exemplars: true, tenancy: "label", rules: true, translation: Convention.wire.translation, histogram: "native",
    degrade: "single-store; tenant is a label, never an isolation boundary",
  },
  mimir: {
    chart: "mimir-distributed", repo: "https://grafana.github.io/helm-charts",
    // The reverse proxy renders as `<pinned>-gateway` on port 80 — `nginx` is the values COMMENT's scope word and
    // the retired component name, never a rendered Service, so an address spelled off it resolves to nothing.
    admit: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-gateway.${ns}.svc/otlp`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-gateway.${ns}.svc/prometheus`,
    values: (row: Lgtm.Store) => ({
      fullnameOverride: row.fullname,
      minio: { enabled: false }, // the estate's object plane is the one store; the bundled subchart would stand up a second
      // this chart's SECOND default-on bundle: a broker the estate never declared, beside the one the collector's own
      // `observe.buffer` axis dials against externally supplied addresses — leaving it on stands a broker nothing
      // produces to and nothing drains, sized for a fleet, inside the escalation already earned for its footprint
      kafka: { enabled: false },
      ruler: {
        enabled: true, // in-store rule evaluation is what lets a burn rule escalate off the board plane
        // `local` reads `<directory>/<tenant>/`, and a ConfigMap item path carries that one directory level
        extraVolumes: [{ name: "rules", configMap: { name: row.rules.configMap, items: [{ key: "rules.yaml", path: `${row.rules.tenant}/rules.yaml` }] } }],
        extraVolumeMounts: [{ name: "rules", mountPath: row.rules.directory }],
      },
      mimir: {
        structuredConfig: {
          // one storage truth: the object plane's endpoint and bucket bind common storage, and every per-section store inherits it
          ...(row.objects === undefined ? {} : {
            common: { storage: { backend: "s3", s3: { endpoint: row.objects.endpoint, bucket_name: row.objects.bucket } } },
            blocks_storage: { s3: { bucket_name: row.objects.bucket } },
          }),
          // rules diverge from common storage deliberately: code-owned groups load read-only, while the s3 backend
          // would carry an API-mutable rule set the content-is-code law forbids
          ruler_storage: { backend: "local", local: { directory: row.rules.directory } },
          // native histograms survive query sharding only under the protobuf response format, so the pair rides together
          frontend: { query_result_response_format: "protobuf" },
          limits: {
            compactor_blocks_retention_period: row.retention,
            // Mimir's distributor panics when these three disagree: an unescaped strategy demands the utf8 scheme, and its suffix half demands the toggle
            otel_translation_strategy: row.translation,
            name_validation_scheme: "utf8",
            otel_metric_suffixes_enabled: true,
            // Arming rides the row's own `histogram` column rather than a chart default: an OTLP exponential
            // histogram lands as buckets when this is off, and every quantile tile then renders the wrong arm
            native_histograms_ingestion_enabled: row.histogram === "native",
            max_native_histogram_buckets: 160, // the SDK's own exponential-histogram bucket count; a sample over it scales down instead of dropping
          },
        },
      },
    }),
    fits: "fleet escalation: horizontal ingest and query past the single-store ceiling, org-isolated end to end",
    signals: ["metrics"], plugin: "prometheus", lifetime: "`limits.compactor_blocks_retention_period` over the object plane, which the compactor ends",
    exemplars: true, tenancy: "org", rules: true, translation: Convention.wire.translation, histogram: "native",
    degrade: "multi-component memory cost; earned only past the single-store ceiling",
  },
  victoriametrics: {
    chart: "victoria-metrics-single", repo: "https://victoriametrics.github.io/helm-charts",
    admit: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-server.${ns}.svc:8428/opentelemetry`,
    read: (release: string, ns: pulumi.Input<string>) => pulumi.interpolate`http://${release}-server.${ns}.svc:8428`,
    values: (row: Lgtm.Store) => ({
      fullnameOverride: row.fullname,
      server: {
        retentionPeriod: row.retention,
        // this dialect answers translation by leaving the flag off: names land as ingested, so no escaping and no suffix rules run
        extraArgs: { "opentelemetry.usePrometheusNaming": "false" },
        // Same recording as the reference row and the same claim, because both rows hold one series plane at one
        // retention window; this chart ships the claim armed at a smaller default, and inheriting it would size the
        // escape's disk off a chart opinion rather than off the window the row already states.
        persistentVolume: { enabled: true, size: _SERIES.claim },
      },
    }),
    fits: "resource-pressure escape: one lean binary where the reference row's footprint is the constraint",
    signals: ["metrics"], plugin: "prometheus", lifetime: "`server.retentionPeriod` on the single binary, which the binary itself ends",
    exemplars: false, tenancy: "label", rules: false, translation: "NoTranslation", histogram: "classic",
    degrade: "no native histograms, no exemplar storage, no type/unit suffixes, no in-store rule evaluator: click-through degrades to trace search, series carry the bare dotted name, and burn numerators render inline per alert evaluation",
  },
} as const satisfies Record.ReadonlyRecord<string, _Plane<"label" | "org", (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>> & {
  readonly chart: string
  readonly repo: string
  readonly read: (release: string, ns: pulumi.Input<string>) => pulumi.Output<string>
  readonly values: (row: Lgtm.Store) => Record.ReadonlyRecord<string, unknown>
  // Grafana datasource this row's query API answers: an engine shipping its own plugin and its own query dialect
  // selects it here, and the board plane provisions what the row named rather than what the reference row answers
  readonly plugin: string
  readonly exemplars: boolean
  readonly rules: boolean
  // each row spells this receiver strategy in its own dialect and every store-side expression renders series names
  // through it, so a row escaping or suffixing differently reads its own names off one unchanged query value
  readonly translation: Convention.Translation
  // Whether this row's engine stores an OTLP exponential histogram as ONE series or as `le`-bearing buckets: the
  // quantile and fraction arms render entirely differently per answer, so the capability is a row column and the
  // row arms its own dialect for it — reading the algebra's default renders bucket arithmetic against a store that
  // holds no buckets, which matches nothing and raises nothing.
  readonly histogram: Board.Query.Histogram
}>

// One scope-header projection both ends of an `org` row take: the collector stamps it on every exporter whose backend
// reads it, and the board plane's own datasource stamps it on every query and every alert evaluation. Ingest-only
// scoping publishes a read door refusing every request for a missing scope, which reads as an empty panel.
const _scoped = (store: (typeof _stores)[keyof typeof _stores], app: string): Record.ReadonlyRecord<string, string> =>
  store.tenancy === "org" ? { "X-Scope-OrgID": app } : {}
```

## [03]-[CHART_ROWS]

[CHART_ROWS]:
- Owner: `Lgtm` — the `_charts` vocabulary carries the signal-backend rows (`loki`, `tempo`, `grafana`, `pyroscope` — the profiles row, present while `spec.profile.observe.profiles` holds — and `collector`) beside the posture rows (`opencost` while `spec.profile.observe.costs` holds, `ebpf` while `spec.profile.observe.ebpf` holds), the selected `_stores` row lands beside them, versions arrive as pinned args, and the tier constructs every chart under one namespace with the collector's exporters aimed at the backends by `Output`-woven URLs; every chart renders as stack children under Pulumi diff and CrossGuard validation, and `rendered` aggregates the child sets as the tier's discovery projection — real render evidence, never chart-name guesses.
- Law: every row pins its rendered name — a Helm `fullname` helper collapses to the release name only when that name CONTAINS the chart name, so a release named for its signal renders `<release>-<chart>` and any endpoint spelled from the release name alone resolves to nothing; each `_charts` row carries a `fullname` projection naming the values path its own chart reads (nested where the chart nests it), `row()` folds it into every install, and `_urls` derives every address from the same pinned name — the projection and the render agree by proof, never by chart-name arithmetic.
- Law: an operator-rendered Service is a second naming authority — a chart whose workload is a custom resource hands naming to its controller, which decorates the pinned name with its own prefix, so `fullnameOverride` pins the CR and not the address; the `service` projection is the row's own answer for that gap, identity where the chart renders its own Service and the controller's decoration where it does not, and `_urls` reads the projection rather than assuming the two names agree.
- Law: a residence is REALIZED, never merely selected — `door` answers the read address and the arming in one column, so a row addressing a plane this stack never planted answers `None` and leaves the resident set, and the published address, the render coordinate, the board driver, the gateway exporter fan, and the sealed rows all read that one realized set; the cold tail's arming is the object plane, since this tier plants nothing for it, and selecting it on a stack carrying none realizes no residence rather than publishing a coordinate over a prefix nothing holds; answering arming on a second column beside a door that already knows it is how a plane comes to be provisioned in one fold and empty in the next.
- Law: residences install as chart rows like any backend — `clickhouse` installs while `spec.profile.observe.analytics` selects it, its schema is branch-owned DDL the tier plants as an init-script ConfigMap, and the collector's `clickhouse` exporter runs `create_schema: false` against it; the exporter's default DDL leaves every attribute in a `Map` outside the sort key, so a query filtering one high-cardinality tenant reads granules holding every other tenant, which is why the tenant read leads `ORDER BY` and the attribute-key columns carry their own bloom-filter indices.
- Law: a residence answers TWO signal questions and they are different questions — `signals` censuses what the plane HOLDS and the `exporter` column's own `routes` bounds what the gateway writes into it, so a plane one owner fills answers both alike while a plane two owners fill answers them differently; collapsing the pair reports every residence as wide-event-only, because no collector leg frames a metric point anywhere and the census then inherits the routing answer.
- Law: columnar ingest at the collector carries logs and traces alone — those two carry the unbounded attribute shape the family exists for, the selected `_stores` row answers alerting because that is what a TSDB is for, and profiles stay on Pyroscope until Tier-0's own swap point arms; the bound rides `routes` rather than the census, so a residence whose plane later holds a third relation gains no gateway traffic from the widening.
- Law: the cold tail HOLDS the metric-point relations beside the wide-event pair — a `data` branch writer plants and fills them where this tier plants nothing, so the census reads three signals against an exporter column that reads `None`, and a board joining series to wide events resolves that join on ONE plane wherever the row is realized; a stack realizing the interactive plane alone joins across two, which is what its own `degrade` already states, and a reader picks the plane off the census rather than off which branch owns the bytes.
- Law: profiles stay outside both families — that backend holds its own plane, so the signal alphabet a store or a residence answers is the three OTLP signals and a profile row reaches evidence through its own door.
- Law: the residence's dialing identity is a SCOPED user row, never the chart's access-managing default — that default ships reachable from its own loopback alone, so the gateway exporter and the board driver both refuse against it, and the chart's one widening switch answers by rewriting the mask on the access-managing user itself, which trades a connection failure for an estate-wide door; the row mints the user its exporter and its driver already name, masks it to the private space a pod network is allocated inside, grants it exactly the two verbs those two legs run, and leaves the default user where it shipped, because the init script planting the DDL is the only caller that reaches it.
- Law: residences carry NO cardinality ceiling — a metrics store demands view caps because a TSDB indexes every series, while unbounded dimensionality IS the reason a wide-event residence exists, so a cap landed here deletes the capability; the residence answers evidence and history, `_stores` answers health and alerting, and one board owner reads both.
- Law: whether a residence is BOARD-readable is its own `plugin` column — a Grafana driver exists for the interactive plane and none exists for an object-plane Parquet tree, so the cold tail answers `None`, publishes its door for the `data` reader alone, and states that in `degrade`; provisioning one engine's driver against whichever row the spec armed points a ClickHouse plugin at an object prefix and reads on the board as an authentication failure rather than as the absence the row declared. That column carries the driver's DIALECT and every coordinate the driver DIALS beside its identity — port, protocol, catalog, user, release stamp — because a driver, the language it speaks, and the listener it connects to are ONE row answer; a board plane holding any of them binds every future residence to the interactive plane's grammar, catalog, and port, and the binding survives undetected while the roster's one board-readable engine happens to be the one spelled.
- Law: each residence retains through its own mechanism — table TTL and materialized views carry ClickHouse retention, the object plane's own lifecycle carries the lake tail, and no worker, scheduler, or cron surface enters this tier for telemetry, since every residence already ships the machinery a new executor duplicates.
- Law: escalations arm as spec values, never as edits — `observe.sampling: "tail"` names the already-defined `tail_sampling` processor on the traces pipeline, `observe.buffer: "broker"` mounts the paired kafka exporter and receiver legs so an accepted batch survives the gateway itself rather than only its restart, and `observe.topology: "agent"` installs the second collector row at `mode: "daemonset"` beside the gateway; each is one `_plan` mention against a component the closure filter already proves defined.
- Law: arming a tier obligates the traffic that makes it a tier — an escalation installing a workload no signal reaches is a cost with no capability, so `observe.topology: "agent"` also flips `_DOOR`, and the published collector address becomes the daemonset's own Service whose default policy holds a pod on its node's agent; leaving the gateway published installs a per-node collector every workload dials past.
- Law: a signal is a `_SIGNALS` row and a pipeline assembled outside that table is the defect the table forecloses — the exporter map, the pipeline map, the broker split, the residence fan, and the relay arm all fold ONE roster, so a row the spec disarms cannot leave a pipeline naming an exporter no arm defined, which is the dangling reference that fails the whole decode rather than degrading one signal.
- Law: agent hops ride Arrow where Arrow carries the payload — `otelarrow` is armed on the agent-to-gateway leg for the three signals it frames, where a columnar dictionary-encoded gRPC stream cuts 30-70% of the bandwidth OTLP with zstd achieves and the traffic crosses node and zone boundaries the estate pays for, and the profile row answers `arrow: false` and relays over the gateway's own OTLP door so an agent estate forwards every signal it admits; the gateway-to-backend leg stays OTLP over http+protobuf because it is one in-namespace hop against backends that speak no Arrow, and the arrow receiver's own `admission` limits bound its receive frame AHEAD of the shared pipeline's `memory_limiter` rather than instead of it — one gateway pipeline per signal serves the arrow door and the OTLP door both, and the OTLP door carries no admission bounds of its own, so dropping the guard for the columnar leg would unguard the leg beside it.
- Law: a listener is reachable only where the chart publishes its port — a receiver's bind address lives in the config document while `ports` lives in the values tree, so a door opened in one and unrowed in the other answers a Service the cluster routes nothing to; `_ports` is the one roster, closing every row the estate never opened and opening the Arrow port exactly on the gateway of an agent estate.
- Law: values are typed objects — the Grafana admin password is the Doppler-generated `GRAFANA_PASSWORD` read handed in as `auth`, so the credential is in-graph and `Boards` authenticates with the same value; persistence, replica, retention, and pipeline knobs are value rows under each pinned chart's own dialect, drifting only with the version pin.
- Law: no row installs on chart defaults — an empty values object is not neutrality, it is an unowned deployment: chart defaults run Loki single-tenant with no retention and an EMPTY schema block its own server refuses to start against, Tempo with no OTLP receiver and no retention, and the collector with a `debug` exporter beside jaeger and zipkin doors the estate never opened; a values tree also carries whole default-on SUBTIERS — Loki's nginx gateway, canary daemonset, test pod, and two memcached tiers — so a row states which of its chart's own workloads it owns and disables the rest, the gateway among them because the collector dials the log door directly; every row states retention, tenancy, its ingest surface, and — where the workload holds state no later apply re-lands — its own storage claim, because a default-off claim is the one chart default whose cost arrives on a reschedule rather than at install; and a chart default the tier does not own is deleted by an explicit `null` values row, because values merge as maps and only `null` removes — and that tombstone lands INSIDE the map the tier's own document already owns, since a sibling spread beside that document replaces the whole key and drops every null it carried, deleting nothing while reading as a complete prune.
- Law: tenancy governs every signal it can reach — the selected store row's `tenancy` column is the whole estate's isolation posture, so an `org` row arms Loki's `auth_enabled`, Tempo's `multitenancyEnabled`, and Pyroscope's tenant arm beside the metrics store, and the collector stamps `X-Scope-OrgID` on EVERY exporter whose backend reads it; a metrics-only org header leaves logs and traces pooled under one tenant on the same escalation, an asymmetry no `degrade` column ever declared.
- Law: provenance rides the pins — when a `keyring` asset accompanies the versions, every chart row verifies (`verify: true` + the keyring), so a tampered chart fails at render and the estate's content-addressed discipline extends to its chart supply.
- Law: the collector is the one ingest seam — the OTLP receiver admits, one named exporter per signal fans out (`otlp_http/logs` to Loki's OTLP ingest, `otlp_http/traces` to Tempo, `otlp_http/metrics` to the selected store row's `admit` path, `otlp_http/profiles` to Pyroscope while the profiles row holds), and the `service.pipelines` rows wire receiver through processors to exporter per signal; workloads never learn a backend address, only the collector endpoint.
- Law: the config is a decoded owner at a STATED depth — `_plan` folds the tier's coordinates into one document and `Schema.decodeSync` admits it once: every component id closes against its own map, every pipeline and extension reference resolves, and every exporter body decodes against its own egress dialect, while a receiver, processor, connector, or extension body stays the `_Bag` whose vocabulary `.api/opentelemetry-collector.md` owns, because re-modeling four upstream component vocabularies here mints a twin that drifts on the next chart pin and the tier authors none of those bodies from a closed alphabet of its own; so a typo in an exporter name fails at the tier seam where the coordinate is still loggable instead of crash-looping a gateway the operator already accepted; closure runs one way by design — a component defined and unnamed is inert, which is what makes an escalation row an arming flip rather than an edit.
- Law: the pipeline order is the tier's, never a preset's — `memory_limiter` runs first so the guard sees the request before any component allocates against it, identity enrichment (`k8s_attributes`, `resource_detection`) runs next so every downstream component sees the workload coordinates, gateway shaping (`transform`, `filter`) runs on enriched data, `cumulative_to_delta` runs after shaping so the rewrite reads the datapoint set that shaping left, and `batch` runs last so batching is the final act before egress; the chart's presets inject at their own insertion point, so this page declares the components itself and pays for the RBAC with an explicit `clusterRole.rules` roster rather than surrendering ordering to a values toggle. Every attribute key a component names — the `k8s_attributes` extract roster and its association sources, the OTTL environment migration's target, the probe filter's path — is a `core/observe/convention` row, so the keys this gateway stamps and the keys a board joins on are one set and a key spelled here as config text is the drift that set exists to delete.
- Law: span-derived series are connectors, not a second pipeline — `span_metrics` mints RED series and `service_graph` mints topology series from the SAME admitted spans, each an exporter on the traces pipeline and a receiver on a metrics pipeline, so SDK-ful workloads earn the RED and service-map series the `ebpf` row earns only for SDK-less ones; Tempo's own metrics-generator is the declined alternative, recorded on its row, because two generators over one span stream fork the series. Both connectors name their dimensions as `core/observe/convention` rows, so a derived series carries the label vocabulary every board query spells and a dimension key naming an attribute no runtime stamps cannot enter as a literal.
- Law: durability is a policy, not a flag — `file_storage` carries an explicit `directory` on a PVC (the chart provisions no default path, and a `emptyDir` erases the queue on reschedule), a bounded `max_size`, startup compaction with a bounded transaction size, and a declared `fsync` posture; every exporter states its own `sending_queue` and `retry_on_failure` bounds; that policy buys one exact guarantee — data the gateway ACCEPTED survives collector restart and reschedule while the PVC, the queue capacity, and the backend retry horizon hold, never "a backend restart drops nothing", which no queue of finite size can promise.
- Law: the collector reports itself on all three signals — `service.telemetry` carries a `periodic` metrics reader, a batched logs processor, and a batched traces processor, each dialing the collector's own OTLP door over `http/protobuf`, so gateway health, queue depth, export failure, and the gateway's own error logs land in the selected store as first-class evidence and the ingest seam is never a blind spot; the chart's `internalTelemetryViaOTLP` block is the declined alternative — it carries the chart's own instability marker and deletes the default prometheus receiver as a side effect; declaring the `readers` list at all replaces the chart's default pull reader, so the `metrics` port serves nothing and ships closed rather than advertising a dead door.
- Law: credentials reach the gateway as environment, never as values — chart values render into a ConfigMap, so a DSN or password spelled there is plaintext in cluster state and in every `pulumi stack export`; the tier mints one `k8s.core.v1.Secret` from the in-graph Doppler read, binds it through `extraEnvsFrom`, and the config document names `${env:…}`, which the collector expands at load.
- Law: the estate prices itself — the `opencost` row aims the exporter at the selected store row's `read` URL through `opencost.prometheus.external.{enabled,url}`, cost series scope by namespace and the `rasm.tenant` label the tenancy owners already stamp, and cost boards compile through the standing `_compiled` fold into the default and tenant orgs; the docker arm declares its degrade — container stats without a Kubernetes allocation feed carry no `opencost` cell, so the dev loop prices nothing and states so.
- Law: the pricing row's upstream modes are exclusive and its default is ON — the chart sums its three upstream flags and refuses to render when more than one holds, so arming `external` without also disarming `internal` is a hard render failure rather than a degraded read; the row states both halves. Its UI and MCP containers likewise ship default-on, standing up a second board plane and an agent surface beside the metrics door, and the row deletes both under the whole-page chart-default law.
- Law: SDK-less workloads earn RED metrics as a chart row — the `ebpf` row installs the OpenTelemetry eBPF instrumentation chart (`opentelemetry-ebpf-instrumentation` from the collector's own repository), and `config.data.otel_traces_export.endpoint` with `config.data.otel_metrics_export.endpoint` bind explicitly to the tier's one `collectorEndpoint`; the row demands privileged eBPF host access, so it binds only where the deploy target grants it and the toggle is spec data, never a default.
- Law: profiles ride the push path — Pyroscope ingests the runtime SDKs' push streams and the collector's profiles pipeline; the row is present-by-default and its removal is a spec delta, so the LGTM plane carries four signals, not three.
- Law: charts render, releases do not exist — `helm.v4.Chart` keeps every rendered resource under Pulumi diff and CrossGuard visibility; `helm.v3.Release` is reached only where a chart demands true release lifecycle, and no row here does.
- Growth: a new signal backend is one `_charts` row with its endpoint projections; a collector component is one `_plan` component row and one pipeline mention; a residence is one `_RESIDENCE` row answering `chart`, `door`, `exporter`, and `plugin` — the chart earning its own install with the values body and init-script DDL that install mounts, the door earning realization and the published address, the exporter earning its component id, its write seat, and its egress dialect across the whole pipeline fan, and the plugin earning the board datasource with its dialect and dialed coordinates, each `None` stating the half this plane leaves to another owner; a new egress policy axis is one field on the shared `_POLICY` block every exporter dialect then answers.
- Boundary: the app-side OTLP export composition is the runtime telemetry plane's and arrives only as the env row; board content is `#BOARD_APPLY`'s upstream data; residence table shape and query dialect are the data planes' — this tier plants the DDL and publishes the door; the chart values and config vocabularies are the external contracts `.api/opentelemetry-collector.md` and `.api/clickhouse.md` own.

```typescript signature
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Array, Duration, Option, Record, Schema } from "effect"
import { Board, Convention, Reliability } from "@rasm/ts/core"
import { Tier, type StackSpec } from "../program/spec.ts"

const _OWN = (name: string) => name // the chart renders its own Service under the pinned fullname

const _charts = {
  // `fullname` names the values path THIS chart's fullname helper reads and `service` names what actually renders:
  // pyroscope nests its whole scaffold, the residence hands naming to its operator, every other row states it flat.
  agent: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  clickhouse: { chart: "clickhouse", repo: "https://helm.altinity.com", fullname: (name: string) => ({ fullnameOverride: name }), service: (name: string) => `clickhouse-${name}` },
  collector: { chart: "opentelemetry-collector", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  ebpf: { chart: "opentelemetry-ebpf-instrumentation", repo: "https://open-telemetry.github.io/opentelemetry-helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  grafana: { chart: "grafana", repo: "https://grafana.github.io/helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  loki: { chart: "loki", repo: "https://grafana.github.io/helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  opencost: { chart: "opencost", repo: "https://opencost.github.io/opencost-helm-chart", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
  pyroscope: { chart: "pyroscope", repo: "https://grafana.github.io/helm-charts", fullname: (name: string) => ({ pyroscope: { fullnameOverride: name } }), service: _OWN },
  tempo: { chart: "tempo", repo: "https://grafana.github.io/helm-charts", fullname: (name: string) => ({ fullnameOverride: name }), service: _OWN },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly chart: string
  readonly repo: string
  readonly fullname: (name: string) => Record.ReadonlyRecord<string, unknown>
  readonly service: (name: string) => string
}>

// Residences answer the same `_Plane` floor the store rows answer, so a reader crossing the two families reads values
// under one column set. `signals` CENSUSES the plane's contents and answers what a board may JOIN across: a plane the
// gateway alone fills holds the two unbounded-attribute signals its leg carries, and a plane a data-branch writer
// also fills holds the metric relations that writer plants. What the gateway WRITES is the `exporter` column's own
// bound, because reading the census for it would fan a metrics leg at a plane no collector exporter can frame a point
// for. `cap` is stated and permanently false — unbounded dimensionality IS the
// capability, so a ceiling deletes the reason residences exist for, and stating it as a column forecloses a later
// pass helpfully adding one. Every remaining column is a row FUNCTION: `chart` the install this row plants beside the
// values body and init-script DDL that install takes, `door` the read address a stack can realize, `exporter` the
// gateway leg that fills the plane beside the address that leg dials, and `plugin` the board driver that door answers
// beside every coordinate that driver dials — because a family whose install, ingest, write seat, and connection body
// are all spelled off one engine's anchor is one row wearing a table's shape.
const _RESIDENCE = {
  lake: {
    fits: "cold tail, cheapest per byte, batch scan",
    // CENSUS, never a route: this plane holds the wide-event pair AND the OTLP metric-point relations `data`
    // `lane/olap` plants and fills, so a board joining series to wide events resolves that join HERE on one plane.
    // No gateway leg reaches this row at all, which is exactly why the two answers diverge and why the routing bound
    // rides `exporter` — widening this column fans nothing, because the column the fan reads is the other one.
    signals: ["logs", "metrics", "traces"],
    // this tier provisions the catalog prefix and publishes the door; the Parquet under it is `data` `lane/olap`'s own
    // write, which is why no collector exporter and no chart row appear on this row
    admit: "object-plane Parquet minted by `data` `lane/olap` `Olap.lake.sink`, absorbed through `Olap.absorb`",
    tenancy: "partition column",
    lifetime: "table-format retention the object plane's own lifecycle rules end, never this tier",
    cap: false,
    // No chart: the Parquet under this door is a data-plane writer's, so this row installs nothing here exactly as
    // its exporter and plugin columns already answer `None`.
    chart: () => Option.none(),
    // ARMING this row is the object plane, never the spec value: the tier plants nothing here, so a stack naming the
    // cold tail without an object plane realizes no residence and this row answers `None` — the same answer the
    // unnamed case gives. The catalog segment is the LAKE's own, because spelling it off the interactive plane's
    // database re-points every Parquet object already written the day that database is renamed.
    door: (bind) =>
      Option.map(
        Option.fromNullable(bind.objects),
        (objects) => pulumi.interpolate`${objects.endpoint}/${objects.bucket}/${_LAKE.catalog}`,
      ),
    // Objects here arrive from the `data` branch's own writer, so the gateway defines no exporter for this plane; a
    // gateway leg beside that writer double-writes one plane under two retention owners, indistinguishably.
    exporter: () => Option.none(),
    // No Grafana driver reads a Parquet tree behind an object-plane prefix, so this row publishes its door and NO
    // board datasource: the plane answers a `data`-side report reader, which is exactly what `degrade` already says.
    plugin: () => Option.none(),
    degrade: "no interactive latency, no tier-side producer, and no board datasource: a tile reading here reads as a report a data-plane reader renders, and the plane holds only what a data-plane writer already landed",
  },
  clickhouse: {
    fits: "interactive wide-event at any cardinality",
    signals: ["logs", "traces"],
    admit: "collector `clickhouse` exporter against branch-owned DDL",
    tenancy: "tenant leads the sort key",
    lifetime: "table TTL with `ttl_only_drop_parts` beside materialized views, which the store's own TTL merge ends",
    cap: false,
    // This row installs its own chart, so its values body and its init-script DDL ride the row beside the door that
    // chart's Service answers; the tier folds the roster and mounts what the column hands it.
    chart: () =>
      Option.some({
        key: "clickhouse",
        ddl: _ddl(_COLUMNAR.ttlDays),
        values: (bind) => ({
          clickhouse: {
            replicasCount: 1,
            shardsCount: 1,
            persistence: { enabled: true, size: "100Gi" }, // wide events are the volume the family exists to hold
            // Chart defaults mask this user to `127.0.0.1/32`, reaching nothing off its own loopback, and this row
            // keeps that posture: only the init script needs it, and it runs inside the server's own container.
            // This chart's one widening switch rewrites that mask to every source address on an ACCESS-MANAGING
            // user, so it is declined outright and the remote reach lands on a scoped user instead.
            defaultUser: { password_secret_name: bind.credential },
            // Gateway exporter and board driver both dial from OTHER pods, so this residence mints the user they
            // already name under the narrowest mask a pod network fits: the private ranges every CNI allocates out
            // of. Stating that key is what NARROWS — its absence renders every source address, exactly the reach
            // declined one row above. Grants carry the two verbs the write leg and the read leg run, because the
            // DDL rides the init script's own default-user session — which is why no create verb enters here and
            // why one Secret serves both custody ends.
            users: [{
              name: _COLUMNAR.user,
              hostIP: [..._COLUMNAR.reach],
              password_secret_name: bind.credential,
              grants: [`GRANT INSERT, SELECT ON ${_COLUMNAR.database}.*`],
            }],
            // the DDL is `IF NOT EXISTS` throughout, so re-running it on every start converges instead of failing
            initScripts: { enabled: true, alwaysRun: true, configMapName: bind.schema },
          },
          keeper: { enabled: false }, // replicated tables are the fleet escalation; one node holds the reference posture
          operator: { enabled: true },
        }),
      }),
    // This row installs its own chart, so naming it arms it; the address is the operator-decorated Service the
    // `_charts` row already pins rather than a second naming authority spelled here.
    door: (bind) =>
      Option.some(pulumi.interpolate`http://${_charts.clickhouse.service(`${bind.release}-clickhouse`)}.${bind.namespace}.svc:${_COLUMNAR.port.http}`),
    exporter: () => Option.some({ id: "clickhouse", routes: ["logs", "traces"], seat: _seat, egress: _column }),
    plugin: () =>
      Option.some({
        dialect: _COLUMNAR.plugin.dialect,
        type: _COLUMNAR.plugin.type,
        // Every coordinate this driver dials rides its OWN row — host off the published door, then the port, the
        // protocol, the catalog, the user, and the release stamp off the residence anchor its DDL and its exporter
        // already read — beside the one generated credential the residence chart's own user takes, so no second
        // custody owner enters for the read side.
        settings: (bind) => ({
          headers: {}, // this driver authenticates on its own connection and dials no request header at all
          json: {
            defaultDatabase: _COLUMNAR.database,
            // This driver's ad-hoc-filter key discovery ships ON and issues a DISTINCT-over-arrayJoin probe per
            // `Map` column — three of them on the logs relation, three on the traces relation — each an unbounded
            // scan of the very wide-event tables the branch-owned sort key and bloom indices exist to keep prunable;
            // it is the one driver default that reads every granule the residence was shaped to skip.
            enableMapKeysDiscovery: false,
            host: _host(bind.door),
            port: _COLUMNAR.port.http,
            protocol: "http",
            username: _COLUMNAR.user,
            version: _COLUMNAR.plugin.version,
          },
          secure: { password: bind.auth },
        }),
      }),
    degrade: "logs and traces only: metrics stay on the selected `_stores` row and profiles on Pyroscope, so an evidence query joins wide events to series across two planes rather than one",
  },
} as const satisfies Record.ReadonlyRecord<string, _Plane<string, string> & {
  readonly cap: false
  // Chart install AND its body in one answer, on the same footing as `door`, `exporter`, and `plugin`: a boolean here
  // states a capability an install below then re-decides from a key literal, so the two can disagree and a second
  // chart-bearing residence installs nothing while its column reads armed. `ddl` is the init-script payload the tier
  // mounts and `values` takes the two names that ConfigMap and the credential Secret only have once minted.
  readonly chart: () => Option.Option<{
    readonly key: keyof typeof _charts
    readonly ddl: string
    readonly values: (bind: { readonly credential: pulumi.Input<string>; readonly schema: pulumi.Input<string> }) => pulumi.Inputs
  }>
  // Read address AND arming in one answer: a row the stack cannot realize has no address to publish, so realization
  // never resolves off a value the realized and unrealized cases share. Read lazily, because the residence anchors
  // these rows name are declared with the collector coordinates below.
  readonly door: (bind: { readonly release: string; readonly namespace: pulumi.Input<string>; readonly objects: Lgtm.Args["objects"] }) => Option.Option<pulumi.Output<string>>
  // Gateway leg that FILLS this plane: its component id, the signals it writes, the write address it dials inside the
  // tier, and its own egress dialect. `None` is a first-class answer — a plane a data-plane writer fills defines no
  // gateway exporter — and the id, the pipeline fan, and the exporter map all fold this ONE column rather than three
  // matching literals.
  readonly exporter: () => Option.Option<{
    readonly id: string
    // Routing bound, held apart from the plane's own census: columnar ingest carries the wide-event pair alone
    // because a TSDB answers alerting and this exporter frames no metric point, while the census answers what the
    // plane HOLDS across every writer. One column serving both makes a plane a second owner fills unspellable — it
    // either under-reports its contents to a board or fans a leg the exporter cannot frame.
    readonly routes: ReadonlyArray<_Signal>
    // Write door, distinct from the read door in port and protocol alike, dialed inside the tier and spelled by no
    // consumer — so one row answers both rather than a reader deriving either from the other.
    readonly seat: (bind: { readonly release: string; readonly namespace: pulumi.Input<string> }) => pulumi.Output<string>
    readonly egress: (endpoint: string, ttl: string) => Record.ReadonlyRecord<string, unknown>
  }>
  // Board driver this row's door answers. `None` is a first-class answer: the row is readable, and not through a
  // panel. `dialect` and `settings` ride the same payload because a driver, the language it speaks, and the
  // coordinates it dials are ONE row answer — reading any of them off the board plane binds every future residence
  // to the interactive plane's grammar, its port, and its catalog.
  readonly plugin: () => Option.Option<{
    readonly dialect: _Dialect
    readonly type: string
    readonly settings: (bind: { readonly auth: pulumi.Input<string>; readonly door: pulumi.Output<string> }) => _Settings
  }>
}>

// Which residence a BOARD reads: the first realized row running an engine Grafana has a driver for. Every downstream
// answer — the published door, the tier's render coordinate, the datasource's presence, its driver identity, and its
// dialect — takes THIS one search, because copies of it resolve to different rows the day a resident order changes.
const _boarded = (residences: ReadonlyArray<keyof typeof _RESIDENCE>): Option.Option<keyof typeof _RESIDENCE> =>
  Array.findFirst(residences, (key) => Option.isSome(_RESIDENCE[key].plugin()))

// Which residence a READER addresses: the board-readable row where one is realized, else the resident head — the cold
// tail, addressed off the object plane rather than through a panel. `None` here IS the refusal `analytics: "none"`
// states and equally the refusal a cold-tail selection on a stack carrying no object plane earns.
const _addressed = (residences: ReadonlyArray<keyof typeof _RESIDENCE>): Option.Option<keyof typeof _RESIDENCE> =>
  Option.orElse(_boarded(residences), () => Array.head(residences))

// Realized rows publish their board driver here, resolved once so presence, type, dialect, and every dialed
// coordinate are four answers off ONE row rather than four independent searches that can disagree.
const _driven = (residences: ReadonlyArray<keyof typeof _RESIDENCE>) =>
  Option.flatMap(_boarded(residences), (key) => _RESIDENCE[key].plugin())

// Selection is a keyed row, never a branch ladder: each spec value names its resident set whole, and the set's ORDER
// is what `_addressed` falls back to — the pair row leads with the interactive plane so a stack arming both publishes
// that door, and a stack arming the cold tail alone publishes ITS door and provisions no datasource, because the
// board-readable row wins the address wherever one is realized.
const _RESIDENT = {
  none: [],
  lake: ["lake"],
  clickhouse: ["clickhouse"],
  both: ["clickhouse", "lake"],
} as const satisfies Record<StackSpec.Residence, ReadonlyArray<keyof typeof _RESIDENCE>>

// Rosters are what a spec names; THIS is what the stack can actually reach — a row whose own `door` answers `None`
// addresses a plane nothing planted, so it leaves the resident set entirely. Every downstream answer — the published
// address, the render coordinate, the board driver, the gateway's exporter fan, and the sealed residence rows — reads
// this ONE set, so a plane the stack cannot realize is absent from all five rather than realized in one fold and
// empty in the next, which is the advertised-but-unplanted plane a reader meets as an authentication failure.
const _resident = (
  analytics: StackSpec.Residence,
  bind: { readonly release: string; readonly namespace: pulumi.Input<string>; readonly objects: Lgtm.Args["objects"] },
): ReadonlyArray<keyof typeof _RESIDENCE> =>
  Array.filter(_RESIDENT[analytics], (key) => Option.isSome(_RESIDENCE[key].door(bind)))

declare namespace Lgtm {
  type Versions = { readonly [K in keyof typeof _charts | "store"]: pulumi.Input<string> } & { readonly exporter: Lgtm.Image }
  // a chart version and a container image reference are distinct facts: one pins a rendered template, the other a digest
  // workloads run, and one field carrying both is how an image tag reached a `version:` argument
  type Image = { readonly repository: pulumi.Input<string>; readonly digest: pulumi.Input<string> }
  // Every answer the metrics plane owes a board, projected by the tier that OWNS the backend selection. `target`
  // carries the selected row's translation and representation columns, so a pack rendered off this value spells the
  // series names that receiver actually wrote; `plugin`, `scope`, `exemplars`, and `recorded` are the four answers a
  // board plane otherwise re-reads off `_stores` from the spec — which describes the K8S arm's install even on the
  // docker arm, whose bundled store ships its own escaping default, its own plugin, and no rule evaluator at all, so
  // that read provisions a scope header no backend expects and renders recorded series nothing records.
  type Metrics = {
    readonly target: Board.Query.Target
    readonly plugin: string
    readonly scope: Record.ReadonlyRecord<string, string>
    readonly exemplars: boolean
    readonly recorded: boolean
  }
  // `analytics` carries the REALIZED residence coordinate rather than a target, because the residence schema —
  // relation names, attribute accessor, engine grammar — is `data` `lane/olap`'s own row and this tier neither holds
  // nor mirrors it; a composition root hands this pair to `Olap.target`, which mints the SQL target off the row that
  // owns it. It resolves the SAME row the published door resolves, so a realized cold tail carries its coordinate
  // for whichever reader addresses it while `source` stays `None` there — readable, and not through a panel.
  // `source` carries the driver WHOLE rather than a key: identity, dialect, and every coordinate it dials come off the
  // armed row, so a board plane provisions what this tier realized instead of re-resolving realization from the spec —
  // a read that names a plane the docker arm's container never runs and aims a driver at an address it publishes empty.
  type Targets = {
    readonly metrics: Metrics
    readonly analytics: Option.Option<{
      readonly residence: keyof typeof _RESIDENCE
      readonly source: Option.Option<{
        readonly key: keyof typeof _SOURCES
        readonly dialect: _Dialect
        readonly type: string
        readonly settings: (bind: { readonly auth: pulumi.Input<string>; readonly door: pulumi.Output<string> }) => _Settings
      }>
    }>
    // Profiles answer at the producing tier too: the k8s arm installs its chart off the spec value while the
    // all-in-one image runs it regardless, so a board plane reading the spec refuses a datasource the dev container
    // is serving and provisions one the k8s stack never installed.
    readonly profiles: boolean
  }
  type Urls = {
    readonly grafana: pulumi.Output<string>
    readonly collector: pulumi.Output<string>
    readonly ingest: {
      readonly logs: pulumi.Output<string>
      readonly traces: pulumi.Output<string>
      readonly metrics: pulumi.Output<string>
      readonly profiles: pulumi.Output<string>
    }
    // Query-door rows are named for the PLANE each door answers, never for the engine behind it: `metrics` resolves to
    // whichever `_stores` row the spec selected and `residence` to whichever `_RESIDENCE` row it armed, so a stack
    // escalating either family re-points one address and every reader keeps its spelling.
    readonly query: {
      readonly loki: pulumi.Output<string>
      readonly tempo: pulumi.Output<string>
      readonly metrics: pulumi.Output<string>
      readonly pyroscope: pulumi.Output<string>
      // SELECTED residence row publishes this door, never one row's product spelled as the plane: a stack running
      // its lake row publishes that catalog prefix here and a stack running neither publishes the empty refusal
      readonly residence: pulumi.Output<string>
    }
  }
  // pg coordinates arrive discrete because `postgresqlreceiver` takes host:port beside separate credential fields and
  // `sqlqueryreceiver` takes one DSN; a single opaque DSN can serve only the second and silently mis-shapes the first.
  type Data = {
    readonly host: pulumi.Input<string>
    readonly port: pulumi.Input<number>
    readonly database: pulumi.Input<string>
    readonly user: pulumi.Input<string>
    readonly password: pulumi.Input<string>
  }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly versions: Versions
    readonly auth: pulumi.Input<string>
    readonly data: Data
    readonly objects?: { readonly endpoint: pulumi.Input<string>; readonly bucket: pulumi.Input<string> } // the object plane's coordinates the mimir ruler and the lake residence bind
    readonly alerts: ReadonlyArray<Reliability.Alert.Spec>
    readonly keyring?: pulumi.asset.Asset
  }
  // One bind, every axis: each collector arm reads the spec's own observe record rather than a boolean the caller
  // pre-folded, so adding an axis edits the schema and the arm that interprets it — never the shape between them.
  type Plan = {
    readonly observe: StackSpec.Observe
    readonly ingest: { readonly logs: string; readonly traces: string; readonly metrics: string; readonly profiles: string }
    // Realized rows carrying a gateway leg, each beside the write address its own row dialed — so the exporter map
    // and the pipeline fan fold ONE roster and a second collector-filled plane is a row rather than a third literal.
    // Rows whose writer is a data plane contribute nothing here, exactly as their `exporter` column says.
    readonly residences: ReadonlyArray<{ readonly key: keyof typeof _RESIDENCE; readonly endpoint: string }>
    // Two upstream doors serve an agent tier — an Arrow stream carries the columnar rows and an OTLP door carries every
    // other; a gateway tier's own plan carries none, which is what makes their absence the role discriminant
    readonly gateway: { readonly arrow: string; readonly otlp: string } | undefined
    readonly ttl: string
    // Pull-side infra receivers and the ids the metrics pipeline names them by: one roster, two readers, so a
    // surface entering the scrape plane cannot land a receiver no pipeline names or a pipeline name nothing defines.
    readonly infra: Record.ReadonlyRecord<string, unknown>
    readonly infraReceivers: ReadonlyArray<string>
    readonly headers: Record.ReadonlyRecord<string, string>
  }
}

// --- [COLLECTOR_CONFIG]

const _COMPONENT = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9_]*(?:\/[A-Za-z0-9_.-]+)?$/), Schema.brand("ComponentId"))
const _PIPELINE = Schema.String.pipe(Schema.pattern(/^(?:traces|metrics|logs|profiles)(?:\/[A-Za-z0-9_.-]+)?$/), Schema.brand("PipelineId"))
// The stated depth of a non-exporter component: its ID closes and its BODY does not, because the body's vocabulary is
// the chart's own external contract and a twin declared here drifts on the next pin. Exporters carry `_Egress` instead
// precisely because the tier authors those bodies off its own closed dialect roster.
const _Bag = Schema.Record({ key: Schema.String, value: Schema.Unknown })
const _Components = Schema.Record({ key: _COMPONENT, value: _Bag })

const _Queue = Schema.Struct({
  enabled: Schema.Boolean,
  storage: Schema.optional(_COMPONENT), // absent is an in-memory queue: a daemonset holds no per-node claim, so the agent leg's durability IS its retry horizon
  sizer: Schema.Literal("requests", "items", "bytes"),
  queue_size: Schema.Int.pipe(Schema.positive()),
  num_consumers: Schema.Int.pipe(Schema.between(1, 64)),
})

const _Retry = Schema.Struct({
  enabled: Schema.Boolean,
  initial_interval: Schema.String,
  max_interval: Schema.String,
  max_elapsed_time: Schema.String, // the queue survives a restart; this bounds how long ONE accepted batch keeps retrying
})

// One durability contract, four transport dialects: an accepted batch survives the same way whatever door it leaves
// by, so the policy pair rides every exporter and each dialect adds only its own wire coordinates.
const _POLICY = { sending_queue: _Queue, retry_on_failure: _Retry } as const

const _Otlp = Schema.Struct({
  ..._POLICY,
  endpoint: Schema.String,
  compression: Schema.Literal("gzip"),
  headers: Schema.optional(Schema.Record({ key: Schema.String, value: Schema.String })),
})

// Arrow rides a bidirectional gRPC stream, so its lifetime sits one anchor below the receiver's connection
// grace: expiring first lets in-flight requests drain, expiring after makes every reset a retry storm.
const _Arrow = Schema.Struct({
  ..._POLICY,
  endpoint: Schema.String,
  tls: Schema.Struct({ insecure: Schema.Boolean }),
  arrow: Schema.Struct({
    num_streams: Schema.Int.pipe(Schema.between(1, 16)),
    max_stream_lifetime: Schema.String,
    prioritizer: Schema.Literal("leastloaded"),
    payload_compression: Schema.Literal("zstd"),
  }),
})

// `create_schema` is pinned FALSE at the type, not by convention: the exporter's own DDL leaves every attribute in a
// `Map` outside the sort key, so a row selecting exporter-created schema buys a whole-granule tenant scan no reader
// sees coming, and the branch-owned DDL is what puts tenant in `ORDER BY`.
const _Column = Schema.Struct({
  ..._POLICY,
  endpoint: Schema.String,
  database: Schema.String,
  username: Schema.String,
  password: Schema.String,
  create_schema: Schema.Literal(false),
  async_insert: Schema.Literal(true),
  compress: Schema.Literal("lz4", "zstd"),
  ttl: Schema.String,
  logs_table_name: Schema.String,
  traces_table_name: Schema.String,
})

// Brokers buffer past the gateway's own life, so topics stay per-signal and acks carry the durability
// coordinate the file queue spells as `fsync`.
// One topic row, four signals: the struct answers EVERY signal the topic roster spells, because a signal the
// roster carries and this shape omits decodes away silently — the producer then falls to the client's own default
// topic while the drain leg subscribes to the rostered name, which is exactly the split-topic failure the shared
// roster exists to foreclose.
const _Topic = Schema.Struct({ topic: Schema.String, encoding: Schema.Literal("otlp_proto") })

const _Broker = Schema.Struct({
  ..._POLICY,
  brokers: Schema.NonEmptyArray(Schema.String),
  protocol_version: Schema.String,
  client_id: Schema.String,
  producer: Schema.Struct({ required_acks: Schema.Literal(-1), compression: Schema.Literal("zstd"), max_message_bytes: Schema.Int.pipe(Schema.positive()) }),
  traces: _Topic,
  metrics: _Topic,
  logs: _Topic,
  profiles: _Topic,
})

const _Egress = Schema.Union(_Otlp, _Arrow, _Column, _Broker)

const _Pipeline = Schema.Struct({
  receivers: Schema.NonEmptyArray(_COMPONENT),
  processors: Schema.Array(_COMPONENT),
  exporters: Schema.NonEmptyArray(_COMPONENT),
})

const _Service = Schema.Struct({
  extensions: Schema.NonEmptyArray(_COMPONENT),
  telemetry: Schema.Struct({
    resource: Schema.optional(Schema.Record({ key: Schema.String, value: Schema.String })), // the chart stamps pod/node coordinates from the downward API; the tier adds owned rows over them, never a free-string dimension
    metrics: Schema.Struct({ level: Schema.Literal("none", "basic", "normal", "detailed"), readers: Schema.NonEmptyArray(_Bag) }),
    logs: Schema.Struct({ level: Schema.Literal("DEBUG", "INFO", "WARN", "ERROR"), encoding: Schema.Literal("json", "console"), processors: Schema.NonEmptyArray(_Bag) }),
    traces: Schema.Struct({ processors: Schema.NonEmptyArray(_Bag) }),
  }),
  pipelines: Schema.Record({ key: _PIPELINE, value: _Pipeline }),
})

class Collector extends Schema.Class<Collector>("Collector")({
  receivers: _Components,
  processors: _Components,
  connectors: _Components,
  exporters: Schema.Record({ key: _COMPONENT, value: _Egress }),
  extensions: _Components,
  service: _Service,
}).pipe(Schema.filter((config) => {
  // Closure, not completeness: a named id MUST be defined, while a defined id may stay unnamed — an inert component is
  // exactly what makes an escalation row an arming flip. A connector is legal on both sides, so it joins both universes.
  const defined = (...maps: ReadonlyArray<Record.ReadonlyRecord<string, unknown>>): ReadonlyArray<string> =>
    Array.flatMap(maps, Record.keys)
  const held = (ids: ReadonlyArray<string>, pool: ReadonlyArray<string>): ReadonlyArray<string> =>
    Array.filter(ids, (id) => !Array.contains(pool, id))
  const ins = defined(config.receivers, config.connectors)
  const outs = defined(config.exporters, config.connectors)
  const dangling = Array.flatMap(Record.toEntries(config.service.pipelines), ([id, pipe]) => [
    ...Array.map(held(pipe.receivers, ins), (ref) => `${id}.receivers/${ref}`),
    ...Array.map(held(pipe.processors, Record.keys(config.processors)), (ref) => `${id}.processors/${ref}`),
    ...Array.map(held(pipe.exporters, outs), (ref) => `${id}.exporters/${ref}`),
    ...Array.map(held(config.service.extensions, Record.keys(config.extensions)), (ref) => `service.extensions/${ref}`),
  ])
  return dangling.length === 0 || { path: ["service"], message: `undefined component reference: ${Array.join(dangling, ", ")}` }
}, { identifier: "CollectorClosure" }))

// OTLP fixes its listener pair, and every end that opens or dials a door reads it here — collector publication, relay
// leg, gateway self-telemetry, Tempo's own receiver block, and the endpoint projection alike.
const _OTLP = { http: 4318, grpc: 4317 } as const

const _COLLECTOR = {
  // The gateway's own OTLP door, dialed at the address the receiver BINDS: the chart defaults every protocol
  // endpoint to `${env:MY_POD_IP}` and this tier merges into that block without restating one, so a listener bound
  // to the pod address answers nothing on loopback and the whole self-telemetry stack lands in a connection refusal.
  self: `http://\${env:MY_POD_IP}:${_OTLP.http}`,
  queue: { path: "/var/lib/otelcol/queue", gib: 8, consumers: 10, depth: 10_000 },
  guard: { check_interval: "5s", limit_percentage: 80, spike_limit_percentage: 25 },
  batch: { timeout: "5s", send_batch_size: 8_192, send_batch_max_size: 16_384 },
  admit: { bytes: 16 * 1024 ** 2, metadata: true },
  probe: ["/healthz", "/readyz", "/livez"],
  buckets: ["5ms", "25ms", "125ms", "625ms", "3s", "15s"],
  // arrow bounds sit AHEAD of the shared pipeline's guard on the gateway an agent estate dials: this receiver
  // admits by uncompressed request size and bounds waiters separately, and its grace outlives the exporter's
  // stream so a reset drains rather than re-sending every in-flight batch
  arrow: { port: 4319, grace: "10m", age: "1m", lifetime: "9m30s", streams: 2, request_mib: 128, waiting_mib: 32, buffer_mib: 256 },
  // one topic roster, two readers: the producing exporter and the draining receiver both fold it, because a broker
  // client naming no topic falls to its own package default and a drain leg then subscribes to a name nothing wrote
  broker: { topics: { traces: "rasm.otlp.traces", metrics: "rasm.otlp.metrics", logs: "rasm.otlp.logs", profiles: "rasm.otlp.profiles" }, bytes: 8 * 1024 ** 2 },
  // The ruler mount and ONE cadence number, two readers: the store's own recording group renders it as a duration
  // string and the board plane's rule group takes it as an integer of seconds. Two literals for one cadence let the
  // evaluator and the reader of its recorded series drift, and the alert then samples a numerator refreshed on a
  // schedule nobody stated.
  rules: { path: "/etc/rules", seconds: 60 },
} as const

// Cold-tail anchor. Its catalog segment is the prefix this tier publishes under the object plane's bucket AND the
// DuckLake catalog `data` `lane/olap` attaches over it, so the planter names it once and the reader spells that same
// name; deriving it from the interactive plane's database instead re-points every Parquet object already written on
// whatever day that database is renamed, and the two planes share no coordinate that raises on the divergence.
const _LAKE = { catalog: "lake" } as const

// One residence anchor spanning both ends: the database the DDL creates, the TTL its tables carry, the ports each end
// dials, the table names the exporter's INSERT statements address, and the plugin the board plane queries through —
// so the exporter row, the planted DDL, and the panel target all read one source.
const _COLUMNAR = {
  database: "rasm",
  port: { native: 9000, http: 8123 },
  // Read-plane identity for THIS engine, published to the board plane through its own residence row's `plugin`
  // column and read nowhere else: this type id names the driver, this editor discriminant selects the plugin's
  // raw-SQL arm over its builder arm, and `dialect` names the `_DIALECTS` row whose alphabet spells every form a
  // panel bound to this driver renders. A board plane reading this anchor directly would bind every future residence
  // to it. `version` is ONE axis filling two sites deliberately: the driver's own config field and every query
  // record's `pluginVersion` both name the PLUGIN release, never the server's — the plugin overwrites its config
  // copy with its own package version whenever the config is saved, so a server release spelled there is silently
  // rewritten and every reader gating on it answers for a version nothing pinned.
  plugin: { dialect: "clickhouse", editor: "sql", type: "grafana-clickhouse-datasource", version: "4.20.0" },
  // Evidence retention is its own decision, never a multiple of the store window: series retention answers alerting
  // and residence retention answers audit and incident reconstruction, so deriving one from the other couples two
  // unrelated horizons and silently shortens the audit trail every time an operator tunes an alert lookback.
  ttlDays: 180,
  user: "otel",
  // Source masks the residence user admits, stated because the chart's per-user key falls to every source address
  // when absent — so this narrows the reach rather than opening it. A pod CIDR is the deploy target's own allocation
  // and no coordinate this tier can spell, so the mask is the private space every CNI allocates inside and the
  // `podSelector` fence stays the estate's real edge; the default user keeps its loopback mask beside these rows.
  reach: ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16"],
  // Two tables, both branch-owned. The metric signal never enters: `_stores` answers health and alerting, the residence
  // answers evidence and history, and the exporter's own metric support is alpha — so the residence takes the two
  // signals whose wide-event shape is the reason it exists and declares the third on its degradation column.
  tables: { logs: "otel_logs", traces: "otel_traces" },
  // Reading ends need two clauses to write a pruning predicate: the partition expression bounds the parts a time
  // window opens, and the tenant expression LEADS the sort key so a single-tenant filter prunes granules instead of
  // reading every other tenant's. Both live here over inside the DDL string alone, because planter and reader are
  // different branches and a reader deriving either one is reading a schema nothing planted.
  partition: "toDate(Timestamp)",
  sort: `ResourceAttributes['${Convention.rasm.tenant}']`,
} as const

// Gateway write seat for the interactive plane: the native protocol port, distinct from the HTTP read door in both
// port and protocol, so the exporter dials what its own driver speaks and no reader derives one address from the other.
const _seat = (bind: { readonly release: string; readonly namespace: pulumi.Input<string> }): pulumi.Output<string> =>
  pulumi.interpolate`tcp://${_charts.clickhouse.service(`${bind.release}-clickhouse`)}.${bind.namespace}.svc:${_COLUMNAR.port.native}`

const _STORAGE = {
  directory: _COLLECTOR.queue.path,
  timeout: "10s",
  max_size: (_COLLECTOR.queue.gib * 1024 ** 3) / 2, // half the claim, so compaction always has room to rewrite in place
  fsync: false, // the PVC is the durability boundary; per-write fsync trades gateway throughput for a guarantee the disk already gives
  compaction: {
    on_start: true,
    // rebound compaction validates against its own cadence: `on_rebound` without a positive `check_interval` refuses at start,
    // and either rebound threshold above `max_size` refuses with it, so the pair rides the same anchor the ceiling comes from
    on_rebound: true,
    check_interval: "5m",
    rebound_needed_threshold_mib: (_COLLECTOR.queue.gib * 1024) / 8,
    rebound_trigger_threshold_mib: (_COLLECTOR.queue.gib * 1024) / 16,
    directory: _COLLECTOR.queue.path,
    max_transaction_size: 65_536,
    cleanup_on_start: true,
  },
} as const

const _SELF = (kind: "metrics" | "logs" | "traces"): Record.ReadonlyRecord<string, unknown> => ({
  [kind === "metrics" ? "periodic" : "batch"]: { exporter: { otlp: { protocol: "http/protobuf", endpoint: _COLLECTOR.self } } },
})

// One durability policy, two custody arms. `durable` is the tier's disk claim, not a per-exporter taste: a tier
// holding the PVC names its storage extension, and a tier holding none states the in-memory queue rather than naming
// a component its own plan never defines — which is what a queue pointed at an undefined `file_storage` does.
const _policy = (durable: boolean) => ({
  sending_queue: {
    enabled: true,
    ...(durable ? { storage: "file_storage" as const } : {}),
    sizer: "requests" as const,
    queue_size: _COLLECTOR.queue.depth,
    num_consumers: _COLLECTOR.queue.consumers,
  },
  retry_on_failure: { enabled: true, initial_interval: "5s", max_interval: "30s", max_elapsed_time: "300s" },
})

const _egress = (endpoint: string, headers: Record.ReadonlyRecord<string, string>, durable: boolean) => ({
  ..._policy(durable),
  endpoint,
  compression: "gzip" as const,
  ...(Record.isEmptyRecord(headers) ? {} : { headers }),
})

// Residence egress pins `create_schema` false at the type, rides `${env:…}` for its credential like every
// other, and the TTL the exporter stamps agrees with the TTL the DDL wrote because both read one anchor.
const _column = (endpoint: string, ttl: string) => ({
  ..._policy(true),
  endpoint,
  database: _COLUMNAR.database,
  username: _COLUMNAR.user,
  password: "${env:CH_PASSWORD}",
  create_schema: false as const,
  async_insert: true as const,
  compress: "zstd" as const,
  ttl,
  logs_table_name: _COLUMNAR.tables.logs,
  traces_table_name: _COLUMNAR.tables.traces,
})

const _arrow = (endpoint: string) => ({
  ..._policy(false), // no claim per node, so the agent's queue lives in memory and its retry horizon IS its durability
  endpoint,
  tls: { insecure: true }, // one in-cluster hop on the pod network; the gateway terminates no public transport
  arrow: {
    num_streams: _COLLECTOR.arrow.streams, // each stream holds encoder state, so an agent per node stays deliberately narrow
    max_stream_lifetime: _COLLECTOR.arrow.lifetime,
    prioritizer: "leastloaded" as const,
    payload_compression: "zstd" as const,
  },
})

// Per-signal topic rows, spelled once for both ends of the broker leg.
const _topics = () => Record.map(_COLLECTOR.broker.topics, (topic) => ({ topic, encoding: "otlp_proto" as const }))

const _BROKER_WIRE = { protocol_version: "3.9.0" } as const // one negotiated wire version; a producer and a consumer disagreeing here fail at connect

const _broker = (brokers: ReadonlyArray<string>) => ({
  ..._policy(true),
  ..._BROKER_WIRE,
  brokers,
  client_id: "rasm-gateway",
  // full ISR acknowledgement is what makes the broker leg a durability upgrade over the disk queue rather than a second best-effort hop
  producer: { required_acks: -1 as const, compression: "zstd" as const, max_message_bytes: _COLLECTOR.broker.bytes },
  ..._topics(),
})

// Drain clients read the rows their producer wrote and commit only after the pipeline exports: an autocommit ack on
// read drops every message in flight when the gateway dies, which is the failure the broker leg exists for.
const _drain = (brokers: ReadonlyArray<string>) => ({
  ..._BROKER_WIRE,
  brokers: [...brokers],
  group_id: "rasm-drain",
  initial_offset: "earliest",
  autocommit: { enable: false, interval: "1s" },
  ..._topics(),
})

// --- [RESIDENCE_DDL]

// Branch-owned DDL, planted as the residence chart's init script. Exactly three clauses diverge from the exporter's
// shipped template: the tenant expression leads `ORDER BY` so a single-tenant filter prunes granules instead of
// reading every other tenant's, the attribute key and value indices land together so key existence prunes before any
// value comparison, and the TTL drops whole parts. Column rosters stay the exporter's own — the INSERT statements it
// generates address these names positionally, so a rename or a reorder breaks ingest at the first batch.
const _Z = "CODEC(ZSTD(1))"
const _T = {
  stamp: "DateTime64(9) CODEC(Delta(8), ZSTD(1))", // monotonic within a part, so delta before zstd is what makes the column small
  text: `String ${_Z}`,
  low: `LowCardinality(String) ${_Z}`,
  map: `Map(LowCardinality(String), String) ${_Z}`,
  keys: `Array(LowCardinality(String)) ${_Z}`,
  byte: "UInt8",
  span: `UInt64 ${_Z}`,
  stamps: `Array(DateTime64(9)) ${_Z}`,
  names: `Array(LowCardinality(String)) ${_Z}`,
  texts: `Array(String) ${_Z}`,
  maps: `Array(Map(LowCardinality(String), String)) ${_Z}`,
} as const

// Rows transcribe the exporter's shipped `internal/sqltemplates` at the pinned version and carry its column ORDER
// verbatim, because the generated INSERT addresses this list positionally. The `*AttributesKeys` trio is one feature
// and lands whole or not at all — a key-existence index over two of three columns prunes half the predicates.
const _DDL = {
  logs: {
    table: _COLUMNAR.tables.logs,
    map: "LogAttributes",
    after: ["ServiceName", "SeverityText"],
    text: "Body",
    columns: [
      ["Timestamp", _T.stamp], ["TraceId", _T.text], ["SpanId", _T.text], ["TraceFlags", _T.byte],
      ["SeverityText", _T.low], ["SeverityNumber", _T.byte], ["ServiceName", _T.low], ["Body", _T.text],
      ["ResourceSchemaUrl", _T.low], ["ResourceAttributes", _T.map], ["ScopeSchemaUrl", _T.low],
      ["ScopeName", _T.text], ["ScopeVersion", _T.low], ["ScopeAttributes", _T.map], ["LogAttributes", _T.map],
      ["EventName", _T.text],
      ["ResourceAttributesKeys", _T.keys], ["ScopeAttributesKeys", _T.keys], ["LogAttributesKeys", _T.keys],
    ],
  },
  traces: {
    table: _COLUMNAR.tables.traces,
    map: "SpanAttributes",
    after: ["ServiceName", "SpanName"],
    text: "SpanName",
    columns: [
      ["Timestamp", _T.stamp], ["TraceId", _T.text], ["SpanId", _T.text], ["ParentSpanId", _T.text],
      ["TraceState", _T.text], ["SpanName", _T.low], ["SpanKind", _T.low], ["ServiceName", _T.low],
      ["ResourceAttributes", _T.map], ["ScopeName", _T.text], ["ScopeVersion", _T.text], ["SpanAttributes", _T.map],
      ["Duration", _T.span], ["StatusCode", _T.low], ["StatusMessage", _T.text],
      ["Events.Timestamp", _T.stamps], ["Events.Name", _T.names], ["Events.Attributes", _T.maps],
      ["Links.TraceId", _T.texts], ["Links.SpanId", _T.texts], ["Links.TraceState", _T.texts], ["Links.Attributes", _T.maps],
      ["ResourceAttributesKeys", _T.keys], ["SpanAttributesKeys", _T.keys],
    ],
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly table: string
  readonly map: string
  readonly after: ReadonlyArray<string>
  readonly text: string
  readonly columns: ReadonlyArray<readonly [name: string, type: string]>
}>

// Key-existence indices over both attribute planes plus a token index over the row's own free text: a predicate
// naming a key that no granule holds is answered from the index instead of a granule read.
const _INDEX = (row: (typeof _DDL)[keyof typeof _DDL]): ReadonlyArray<string> => [
  ...Array.flatMap(["ResourceAttributes", "ScopeAttributes", row.map], (map) => [
    `INDEX idx_${map}_key mapKeys(${map}) TYPE bloom_filter(0.01) GRANULARITY 1`,
    `INDEX idx_${map}_value mapValues(${map}) TYPE bloom_filter(0.01) GRANULARITY 1`,
  ]),
  ...Array.map(Array.filter(row.columns, ([name]) => name.endsWith("AttributesKeys")), ([name]) =>
    `INDEX idx_${name} ${name} TYPE bloom_filter(0.01) GRANULARITY 1`),
  `INDEX idx_trace_id TraceId TYPE bloom_filter(0.001) GRANULARITY 1`,
  `INDEX idx_text lower(${row.text}) TYPE tokenbf_v1(32768, 3, 0) GRANULARITY 8`,
]

const _ddl = (days: number): string =>
  Array.join([
    `CREATE DATABASE IF NOT EXISTS ${_COLUMNAR.database};`,
    ...Array.map(Record.values(_DDL), (row) =>
      `CREATE TABLE IF NOT EXISTS ${_COLUMNAR.database}.${row.table} (`
      + Array.join([...Array.map(row.columns, ([name, type]) => `\`${name}\` ${type}`), ..._INDEX(row)], ", ")
      + `) ENGINE = MergeTree PARTITION BY ${_COLUMNAR.partition} `
      // tenant reads deterministically off a stored column, so it keys the primary index directly and no
      // second column, materialized twin, or projection is minted to carry what the resource map already holds
      + `ORDER BY (${_COLUMNAR.sort}, ${Array.join([...row.after, "toUnixTimestamp(Timestamp)", "TraceId"], ", ")}) `
      + `TTL toDateTime(Timestamp) + toIntervalDay(${days}) SETTINGS ttl_only_drop_parts = 1;`),
  ], "\n")

// --- [COLLECTOR_PIPELINES]

const _ENRICH = ["memory_limiter", "k8s_attributes", "resource_detection"] as const // guard first, then identity: every later component sees the workload coordinates

// per-signal gateway shaping beyond the shared resource rule. Only the metric leg carries a datapoint pass, deleting
// that synthetic UCUM carrier the Effect metric bridge rides as its descriptor-unit tag — spelled off `Convention.wire`
// because emission and strip are one export-contract fact a free string forks at the next rename. Facade-lane producers
// strip it through their own view roster; this pass closes the NATIVE lane, where no view runs at all, so no store on
// either lane sees the carrier and no series forks into a unit-labelled family.
const _SHAPED = {
  trace: [],
  log: [],
  metric: [{ context: "datapoint", statements: [`delete_key(attributes, "${Convention.wire.unit}")`] }],
} as const satisfies Record<"trace" | "log" | "metric", readonly { readonly context: string; readonly statements: readonly string[] }[]>

// Placement rows this gateway stamps: the extract roster and the vocabulary a board joins a series to its pod
// through are ONE set, so neither side can gain a key the other cannot resolve.
const _PLACEMENT = [
  Convention.attr.k8sNamespace,
  Convention.attr.k8sPodName,
  Convention.attr.k8sPodUid,
  Convention.attr.k8sNode,
  Convention.attr.k8sDeployment,
  Convention.attr.k8sStatefulSet,
  Convention.attr.k8sDaemonSet,
  Convention.attr.k8sJob,
  Convention.attr.k8sCronJob,
  Convention.attr.k8sContainer,
  Convention.attr.containerImage,
] as const

// Container-plane rows resolve only where the SDK already stamped `container.id` or `k8s.container.name` on the
// incoming resource: this processor keys a container off one of those two and enriches from there, so the image row
// rides the detector arm every workload composition already runs rather than standing inert on this roster.
// Image TAG stays deliberately absent: this processor emits a SINGULAR `container.image.tag` of its own dialect
// while the installed semconv build declares only the plural `container.image.tags`, so requesting either spelling
// here buys one concept under two names — the roster asks for the image NAME, whose semconv row this processor
// writes byte-identical, and the tag reaches evidence through the resource detector that owns the stable spelling.

const _LEGACY_ENVIRONMENT = "deployment.environment" // the deprecated spelling a foreign SDK still emits: a migration SOURCE this estate never mints, so it earns no convention row

// Signal-shaped wiring, one rule: the ADMIT leg receives and shapes, the DRAIN leg fans to every residence the spec
// armed. `buffer: "file"` fuses them into one pipeline behind the disk queue; `buffer: "broker"` splits them at a
// kafka topic, so an accepted batch outlives the gateway POD and not merely its restart. Neither arm changes a
// component definition — the split is which pipeline names which id.
// One span-derived pair, two mentions: exporters on the traces leg, receivers on the metrics leg. Naming the pair
// once is what keeps a connector from being wired as a sink with no source, which parses and then emits nothing.
const _CONNECTORS = ["span_metrics", "service_graph"] as const

// Every signal is a row, and a row assembled outside this table is how a pipeline came to name an exporter its own
// role never defines. `spine` is the row's own processor prefix, because the enrichment set predates profile support
// and a shared constant names components that pipeline cannot load; `armed` is the spec predicate deciding whether
// this row renders at all; `arrow` states whether its payload crosses the columnar agent hop, since Arrow
// transport carries the three OTLP signals and no profile frame — a row answering false relays over the gateway's own
// OTLP door instead, so an agent estate forwards every signal it admits rather than stranding one on a dead leg.
const _SIGNALS = {
  logs: { spine: _ENRICH, shape: ["transform"], self: "otlp_http/logs", arrow: true, armed: () => true },
  traces: { spine: _ENRICH, shape: ["transform", "filter"], self: "otlp_http/traces", arrow: true, armed: () => true },
  metrics: { spine: _ENRICH, shape: ["transform", "cumulative_to_delta"], self: "otlp_http/metrics", arrow: true, armed: () => true },
  profiles: { spine: ["memory_limiter"], shape: [], self: "otlp_http/profiles", arrow: false, armed: (observe: StackSpec.Observe) => observe.profiles },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly spine: ReadonlyArray<string>
  readonly shape: ReadonlyArray<string>
  readonly self: string
  readonly arrow: boolean
  readonly armed: (observe: StackSpec.Observe) => boolean
}>

const _RELAY = "otlp_http/gateway" // the agent's OTLP leg to the gateway door, taken by every row Arrow cannot carry

// `gateway` is the whole role discriminant: a plan carrying gateway doors RELAYS — it enriches at the node and hands
// each signal to the transport its own row names — while a plan carrying none is the gateway itself and owns the
// fan-out, the span-derived connectors, the residence, and the broker legs.
const _wired = (bind: Lgtm.Plan, signal: keyof typeof _SIGNALS): Record.ReadonlyRecord<string, unknown> => {
  const row = _SIGNALS[signal]
  const relay = bind.gateway !== undefined
  const agents = bind.observe.topology === "agent"
  const sampled = signal === "traces" && bind.observe.sampling === "tail" && !relay ? ["tail_sampling"] : []
  const sources = [
    "otlp",
    ...(signal === "metrics" && !relay ? [..._CONNECTORS, ...bind.infraReceivers] : []),
    // an agent estate's gateway opens the arrow door for rows that cross it, while a profile row keeps
    // arriving on the shared OTLP receiver — same door the agent's relay leg dials
    ...(agents && !relay && row.arrow ? ["otelarrow"] : []),
  ]
  // residences take the same admitted stream the backend takes — one ingest, two durabilities, never a second door —
  // and each realized row's own `exporter` column decides both whether a leg exists and which signals it writes, so a
  // second collector-filled residence entering is a value edit rather than literals here to match. The fan reads
  // ROUTES, never the census: a plane a data-branch writer also fills censuses relations no exporter frames, and a
  // fan reading that column would name a metrics leg against an exporter whose own dialect carries no metric point.
  const backends = relay ? [row.arrow ? "otelarrow" : _RELAY] : [
    row.self,
    ...Array.filterMap(bind.residences, ({ key }) =>
      Option.flatMap(_RESIDENCE[key].exporter(), (leg) =>
        Array.some(leg.routes, (route) => route === signal) ? Option.some(leg.id) : Option.none())),
  ]
  // span-derived connectors ride the ADMIT leg alone: naming them on both legs feeds one span stream through the
  // connector twice and doubles every RED and topology series the boards read
  const derived = relay || signal !== "traces" ? [] : _CONNECTORS
  const admit = { receivers: sources, processors: [...row.spine, ...row.shape, ...sampled, "batch"] }
  return !row.armed(bind.observe)
    ? {}
    : relay || bind.observe.buffer.mode === "file"
      ? { [signal]: { ...admit, exporters: [...backends, ...derived] } }
      : {
          [signal]: { ...admit, exporters: ["kafka", ...derived] },
          // drain re-enters already-shaped data, so this leg carries the guard and the batch alone
          [`${signal}/drain`]: { receivers: ["kafka"], processors: ["memory_limiter", "batch"], exporters: backends },
        }
}

const _plan = (bind: Lgtm.Plan): Collector =>
  Schema.decodeSync(Collector)({
    receivers: {
      // each transport bounds a body in its own unit: confighttp counts bytes, configgrpc counts mebibytes on the receive frame
      otlp: {
        protocols: {
          http: { max_request_body_size: _COLLECTOR.admit.bytes, include_metadata: _COLLECTOR.admit.metadata },
          grpc: { max_recv_msg_size_mib: _COLLECTOR.admit.bytes / 1024 ** 2, include_metadata: _COLLECTOR.admit.metadata },
        },
      },
      // arrow owns its own admission at the receive frame — uncompressed request size and waiters counted
      // separately — ahead of the shared pipeline's guard rather than instead of it: one pipeline per signal
      // serves this door and the OTLP door both, and the OTLP door bounds nothing, so the spine keeps the guard
      ...(bind.observe.topology === "agent" && bind.gateway === undefined ? {
        otelarrow: {
          protocols: {
            grpc: {
              endpoint: `\${env:MY_POD_IP}:${_COLLECTOR.arrow.port}`,
              max_recv_msg_size_mib: _COLLECTOR.admit.bytes / 1024 ** 2,
              keepalive: { server_parameters: { max_connection_age: _COLLECTOR.arrow.age, max_connection_age_grace: _COLLECTOR.arrow.grace } },
            },
            arrow: { memory_limit_mib: _COLLECTOR.arrow.buffer_mib },
          },
          admission: { request_limit_mib: _COLLECTOR.arrow.request_mib, waiting_limit_mib: _COLLECTOR.arrow.waiting_mib },
        },
      } : {}),
      ...(bind.observe.buffer.mode === "broker" && bind.gateway === undefined ? { kafka: _drain(bind.observe.buffer.brokers) } : {}),
      ...(bind.gateway === undefined ? bind.infra : {}), // infra scrape is a gateway concern; an agent per node would scrape the same surface once per node
    },
    processors: {
      memory_limiter: _COLLECTOR.guard,
      k8s_attributes: {
        // pod identity by IP and by the downward-API pod name, so a workload behind a service mesh still resolves
        auth_type: "serviceAccount",
        passthrough: false,
        extract: { metadata: [..._PLACEMENT] },
        pod_association: [
          { sources: [{ from: "resource_attribute", name: Convention.attr.k8sPodIp }] },
          { sources: [{ from: "resource_attribute", name: Convention.attr.k8sPodUid }] },
          { sources: [{ from: "connection" }] },
        ],
      },
      resource_detection: { detectors: ["env", "k8s_api"], timeout: "5s", override: false }, // enrich, never replace: the SDK-minted triple wins the merge
      transform: {
        error_mode: "ignore",
        ...Object.fromEntries(Array.map(["trace", "log", "metric"], (signal) => [`${signal}_statements`, [{
          context: "resource",
          statements: [
            // one narrow shaping rule: a foreign SDK still on the deprecated key lands under the current semconv spelling
            `set(attributes["${Convention.attr.deploymentEnvironment}"], attributes["${_LEGACY_ENVIRONMENT}"]) where attributes["${Convention.attr.deploymentEnvironment}"] == nil and attributes["${_LEGACY_ENVIRONMENT}"] != nil`,
            `delete_key(attributes, "${_LEGACY_ENVIRONMENT}")`,
          ],
        }, ..._SHAPED[signal]]])),
      },
      // DELTA is the estate wire law and producer-side conversion is lossless for monotonic sums ALONE — an interval
      // min and max are unrecoverable from a cumulative histogram snapshot — so the gateway completes the law for any
      // stream a producer still ships cumulative. `include` bounds the rewrite to the estate namespace, leaving a
      // third-party series on whatever temporality its own backend contract expects.
      cumulative_to_delta: {
        include: { metrics: [`^${Convention.wire.namespace}\\..*`], match_type: "regexp" },
        initial_value: "auto",
      },
      filter: {
        error_mode: "ignore",
        // condition lists are the current spelling; the per-signal `spans`/`logs` blocks are superseded and refuse beside them
        trace_conditions: Array.map(_COLLECTOR.probe, (path) => `attributes["${Convention.attr.urlPath}"] == "${path}"`), // probe traffic is liveness, never evidence
      },
      // `observe.sampling` names it or nothing does: the definition is standing and inert, so arming the decision tier
      // is one pipeline mention the closure filter already proves resolvable, never a re-design
      tail_sampling: {
        decision_wait: "10s",
        num_traces: 100_000,
        expected_new_traces_per_sec: 1_000,
        policies: [
          { name: "errors", type: "status_code", status_code: { status_codes: ["ERROR"] } },
          { name: "slow", type: "latency", latency: { threshold_ms: 500 } },
          { name: "baseline", type: "probabilistic", probabilistic: { sampling_percentage: 10 } },
        ],
      },
      batch: _COLLECTOR.batch,
    },
    connectors: {
      span_metrics: {
        histogram: { explicit: { buckets: [..._COLLECTOR.buckets] } },
        dimensions: Array.map([Convention.rasm.tenant, Convention.attr.httpRoute, Convention.attr.httpMethod], (name) => ({ name })),
        exemplars: { enabled: true }, // the RED series link back to the spans they were derived from
        aggregation_temporality: "AGGREGATION_TEMPORALITY_DELTA",
        metrics_flush_interval: "15s",
      },
      service_graph: { latency_histogram_buckets: [..._COLLECTOR.buckets], dimensions: [Convention.rasm.tenant], store: { ttl: "10s", max_items: 100_000 } },
    },
    exporters: {
      // Backend fan-out is the gateway's alone, and each signal row's own `self`/`armed` pair mints its key — the
      // exporter map and the pipeline map fold ONE table, so a row the spec disarms cannot leave a pipeline naming an
      // exporter this map never defined, which is the dangling reference that fails the whole decode.
      ...(bind.gateway !== undefined ? {} : Record.fromEntries(
        Array.filterMap(Record.keys(_SIGNALS), (signal) =>
          _SIGNALS[signal].armed(bind.observe)
            ? Option.some([_SIGNALS[signal].self, _egress(bind.ingest[signal], bind.headers, true)] as const)
            : Option.none()))),
      // residence writes belong to the gateway fan-out too: a daemonset defining one carries that credential and a
      // disk-queue coordinate onto every node for an exporter its own relay arm can never name. Each realized row
      // names its own component id and its own egress dialect, so this map and the pipeline fan above fold ONE roster.
      ...(bind.gateway !== undefined ? {} : Record.fromEntries(
        Array.filterMap(bind.residences, ({ endpoint, key }) =>
          Option.map(_RESIDENCE[key].exporter(), (leg) => [leg.id, leg.egress(endpoint, bind.ttl)] as const)))),
      ...(bind.observe.buffer.mode === "broker" && bind.gateway === undefined ? { kafka: _broker(bind.observe.buffer.brokers) } : {}),
      // agents carry the two upstream legs their signal rows name: the Arrow stream for the columnar signals and the
      // gateway's own OTLP door for every row Arrow carries no frame for
      ...(bind.gateway === undefined ? {} : { otelarrow: _arrow(bind.gateway.arrow), [_RELAY]: _egress(bind.gateway.otlp, bind.headers, false) }),
    },
    extensions: {
      ...(bind.gateway === undefined ? { file_storage: _STORAGE } : {}),
      // probes dial this one target; pipeline-failure gating is the extension's own dead field, so export health reads off the self-telemetry series instead
      health_check: { endpoint: "${env:MY_POD_IP}:13133", path: "/", response_body: { healthy: "ok", unhealthy: "degraded" } },
      pprof: { endpoint: "localhost:1777" },
      zpages: { endpoint: "localhost:55679" },
    },
    service: {
      extensions: [...(bind.gateway === undefined ? ["file_storage"] : []), "health_check", "pprof", "zpages"],
      telemetry: {
        // gateway holds no Identity.App, so its own resource reads the estate pin off the wire triple rather than spelling a second literal
        resource: { [Convention.attr.serviceNamespace]: Convention.wire.namespace },
        metrics: { level: "detailed", readers: [_SELF("metrics")] },
        logs: { level: "INFO", encoding: "json", processors: [_SELF("logs")] },
        traces: { processors: [_SELF("traces")] },
      },
      // `cumulative_to_delta` runs AFTER shaping and before batch: the rewrite reads the datapoint set the transform left
      pipelines: Object.assign({}, ...Array.map(Record.keys(_SIGNALS), (signal) => _wired(bind, signal))),
    },
  })

// Port publication is the chart's, never the config document's. A receiver binding an address the `ports` map never
// rows renders no `servicePort`, so its listener answers a Service the cluster routes nothing to — the dead-address
// class one layer down from a dead name. Rows the estate never opened ship closed; the Arrow door opens exactly where
// an agent estate dials it, and the self-telemetry readers replace the chart's pull reader so `metrics` serves nothing.
const _ports = (arrow: boolean): Record.ReadonlyRecord<string, unknown> => ({
  "jaeger-compact": { enabled: false },
  "jaeger-thrift": { enabled: false },
  "jaeger-grpc": { enabled: false },
  zipkin: { enabled: false },
  metrics: { enabled: false },
  ...(arrow ? { otelarrow: { enabled: true, containerPort: _COLLECTOR.arrow.port, servicePort: _COLLECTOR.arrow.port, protocol: "TCP", appProtocol: "grpc" } } : {}),
})

// Chart defaults are not neutral, so this roster names every component the estate never opened. `prometheus` rides it
// like every other name BECAUSE the fold below tombstones only what the plan leaves undefined: a gateway defines that
// receiver off its own infra roster and keeps it, while an agent — whose role defines no infra scrape at all — would
// otherwise inherit the chart's default receiver untombstoned, which is the one chart default a name-conditional
// roster left standing and a reader could only find by rendering the daemonset.
const _PRUNED = { exporters: ["debug"], receivers: ["jaeger", "prometheus", "zipkin"] } as const

// Every tombstone lands INSIDE the map the decoded plan already owns. Values merge as maps one key at a time, so a sibling
// `{ ..._PRUNED, ...plan }` spread replaces `receivers` and `exporters` whole and every null in it is silently dropped.
// A name the plan DEFINES is skipped rather than tombstoned — the definition displaces the chart default on its own, and
// a null written after it would delete the tier's own component — so the roster states the estate's whole closed set
// once and each tier's own plan decides which rows it still owes a tombstone.
const _pruned = (config: Collector): Record.ReadonlyRecord<string, unknown> => ({
  ...config,
  ...Record.map(_PRUNED, (names, slot) => {
    const defined: ReadonlyArray<string> = Record.keys(config[slot]) // stated annotation widens the branded ids; no assertion re-admits them
    return {
      ...config[slot],
      ...Record.fromEntries(
        Array.filterMap(names, (name) => (Array.contains(defined, name) ? Option.none() : Option.some([name, null] as const))),
      ),
    }
  }),
})

// --- [RECORDING_RULES]

// Precomputed burn numerator. `Board.Query.breach` defines it once here; `_expr` on the board plane reads this same
// name back, so the expensive expression evaluates once per group interval instead of once per alert evaluation and
// its definition still exists at exactly one site. Colon-separated spelling is the store-derived convention and
// cannot collide with the dotted producer grammar Tier-0 fixes.
const _recorded = (spec: Reliability.Alert.Spec, window: Duration.DurationInput): string =>
  `${spec.slug}:burn:${Duration.toSeconds(Duration.decode(window))}s`

// One render target for every store-side expression on this page. The selected row's `translation` column decides how a
// mint name becomes a series name, so the recording rule, the alert numerator, and the objective observable read the
// spelling the receiver actually wrote; `source` names the query end a rendered panel binds and rule evaluation ignores.
const _promql = (store: (typeof _stores)[keyof typeof _stores]): Board.Query.Target =>
  Board.Query.promql({ histogram: store.histogram, source: "metrics", translation: store.translation })

const _groups = (alerts: ReadonlyArray<Reliability.Alert.Spec>, target: Board.Query.Target): Lgtm.Rules["groups"] =>
  Array.map(alerts, (spec) => ({
    name: spec.slug,
    interval: `${_COLLECTOR.rules.seconds}s`,
    // both windows record under one group so the short and long verdicts read one evaluation timestamp; split groups
    // let the multiwindow guard compare numerators sampled at two different instants
    rules: Array.map([spec.windows.short, spec.windows.long], (window) => ({
      record: _recorded(spec, window),
      expr: Board.Query.render(Board.Query.breach(spec.sli, Board.Query.span(Duration.decode(window)), {}, spec.filters), target),
      labels: spec.annotations,
    })),
  }))

const _RBAC = [
  { apiGroups: [""], resources: ["pods", "namespaces", "nodes"], verbs: ["get", "watch", "list"] },
  { apiGroups: ["apps"], resources: ["replicasets", "deployments", "statefulsets", "daemonsets"], verbs: ["get", "watch", "list"] },
  { apiGroups: ["batch"], resources: ["jobs", "cronjobs"], verbs: ["get", "watch", "list"] },
] as const

class Lgtm extends Tier {
  // Reading ends bind this residence RELATION CONTRACT whole: the catalog, the two planted relations, the partition
  // expression, the tenant sort-key lead every scoped read prunes on, and the evidence horizon parts drop at. Column
  // rosters stay the exporter's own — its generated INSERT addresses them positionally — so this contract fixes what
  // one foreign reader must agree with while ingest itself keeps proving the roster. Re-spelling any coordinate here
  // reads a schema this tier never planted, which is why the contract crosses as one published value.
  static readonly residence = {
    catalog: _COLUMNAR.database,
    // Cold-tail catalog crosses beside it, because the object prefix this tier publishes and the DuckLake catalog
    // a reader attaches over it are ONE name a planter owes its reader.
    lake: _LAKE.catalog,
    relations: _COLUMNAR.tables,
    partition: _COLUMNAR.partition,
    sort: _COLUMNAR.sort,
    ttlDays: _COLUMNAR.ttlDays,
  } as const
  readonly urls: Lgtm.Urls
  readonly collectorEndpoint: pulumi.Output<string>
  readonly rendered: pulumi.Output<ReadonlyArray<unknown>>
  // The SELECTED store row whole, sealed beside the resident residence rows: `fits`, `signals`, `lifetime`, and
  // `degrade` are declarations a consumer reads to know what this stack gave up on series, and they answer nowhere
  // else — `targets.metrics` publishes the four coordinates a board plane DIALS and states nothing about the trade.
  // A degradation column with no seat is a row that reads complete to a prose scan and reaches no reader at all.
  readonly store: (typeof _stores)[keyof typeof _stores]
  readonly residences: ReadonlyArray<(typeof _RESIDENCE)[keyof typeof _RESIDENCE]>
  readonly targets: Lgtm.Targets
  constructor(name: string, args: Lgtm.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Lgtm", name, opts)
    const observe = args.spec.profile.observe
    const store = _stores[observe.store]
    const isolated = store.tenancy === "org"
    const agents = observe.topology === "agent"
    // REALIZED, never the spec's roster: a cold-tail selection on a stack carrying no object plane addresses a prefix
    // nothing holds, so that row leaves the set here and every consumer below reads the absence rather than an address
    // and a coordinate for a plane this tier never planted.
    const residences = _resident(observe.analytics, { release: name, namespace: args.namespace, objects: args.objects })
    this.urls = _urls({ release: name, namespace: args.namespace, store, residences, objects: args.objects, topology: observe.topology })
    // Render coordinates resolve with the selection, ahead of every consumer: the recording-rule fold below reads the
    // metrics target, and a pack rendered against a target a composition root guessed is a board whose every selector
    // spells a strategy the selected store never wrote. Residence stays a COORDINATE the `data` branch's own row
    // completes, because relation names, attribute accessor, and engine grammar are its schema.
    this.targets = {
      metrics: {
        target: _promql(store),
        plugin: store.plugin,
        scope: _scoped(store, args.spec.app), // one header projection, two ends: the collector exporters and this door
        exemplars: store.exemplars,
        recorded: store.rules,
      },
      analytics: Option.map(_addressed(residences), (residence) => ({
        residence,
        // Datasource half answers only where a realized row runs a driver, and it carries that driver WHOLE — a cold
        // tail publishes its coordinate for whichever reader addresses it and claims no panel binding it cannot
        // provision, while an interactive row hands the board plane the identity, the language, and the coordinates
        // its own engine dials rather than a key a board plane then resolves against the spec.
        source: Option.map(_driven(residences), (plugin) => ({ key: "residence" as const, ...plugin })),
      })),
      profiles: observe.profiles, // this arm installs the chart off the spec value, so the spec value IS its answer
    }
    const provenance = args.keyring === undefined ? {} : { verify: true, keyring: args.keyring }
    const charts: Array<pulumi.Output<ReadonlyArray<unknown>>> = [] // every chart's rendered child set aggregates onto the tier's discovery projection
    const row = (key: keyof typeof _charts, values: pulumi.Inputs) => {
      const chart = new k8s.helm.v4.Chart(`${name}-${key}`, {
        chart: _charts[key].chart,
        repositoryOpts: { repo: _charts[key].repo },
        version: args.versions[key],
        namespace: args.namespace,
        ...provenance,
        values: { ..._charts[key].fullname(`${name}-${key}`), ...values }, // the pinned name IS what `_urls` projects; no consumer re-derives a helm fullname
      }, this.child())
      charts.push(chart.resources)
      return chart
    }
    row("grafana", {
      adminPassword: args.auth,
      // Persistence is this chart's one default-off row that MATTERS here: the provider re-applies folders, sources,
      // boards, and rules, and nothing else it lands — organizations, service accounts, the rotating token values
      // Doppler already holds, org preferences, and every deploy and roll annotation live in the server's own store.
      // Without a claim a reschedule returns a blank server holding none of them while Pulumi state still says it
      // does, so the tenant fleet reads empty and every issued token authenticates against an identity that no
      // longer exists. The provisioning rows (`datasources`, `dashboards`, `alerting`, `sidecar`) stay untouched
      // because content is code applied against the running server, and each ships off.
      persistence: { enabled: true, size: "10Gi" },
    })
    row("loki", {
      deploymentMode: "SingleBinary",
      singleBinary: { replicas: 1 },
      // The mode value alone does not disarm the other topology: the chart's own validator REFUSES to render when
      // the single-binary and simple-scalable targets both carry replicas, and `write`/`read`/`backend` default to
      // non-zero, so the three zeroes are what make the declared mode the installed one.
      write: { replicas: 0 },
      read: { replicas: 0 },
      backend: { replicas: 0 },
      loki: {
        auth_enabled: isolated, // the store row's tenancy column governs the log plane too; a metrics-only escalation pools every tenant's logs
        commonConfig: { replication_factor: 1 },
        storage: { type: "filesystem" },
        // this chart ships an EMPTY schema block and starts no server without one; the index period is the schema's own
        // fixed grain, so it is a chart contract rather than the estate's retention coordinate
        schemaConfig: { configs: [{ from: "2024-04-01", store: "tsdb", object_store: "filesystem", schema: "v13", index: { prefix: "loki_index_", period: "24h" } }] },
        limits_config: { retention_period: args.spec.profile.observe.retention, allow_structured_metadata: true },
        compactor: { retention_enabled: true, delete_request_store: "filesystem" }, // retention without the compactor leg is a setting that deletes nothing
      },
      // Five workloads ship default-ON beside the single binary — an nginx gateway, a canary daemonset, a test pod,
      // and two memcached tiers — none of them declared here, and one of them fronting the very door the collector
      // already dials directly, so the whole-page chart-default law deletes each by name. This chart's other two
      // subtiers, the zone-aware rollout controller and the bundled object store, ship OFF and are owed no tombstone:
      // a disarm written against a default-off row proves nothing about the census it appears to have passed, and it
      // reads on the next chart bump as a decision someone made rather than a default someone inherited.
      gateway: { enabled: false },
      lokiCanary: { enabled: false },
      test: { enabled: false },
      chunksCache: { enabled: false },
      resultsCache: { enabled: false },
    })
    row("tempo", {
      tempo: {
        retention: args.spec.profile.observe.retention,
        multitenancyEnabled: isolated,
        // Tempo installs no OTLP door by default; the collector's traces exporter dials exactly this one
        receivers: { otlp: { protocols: { http: { endpoint: `0.0.0.0:${_OTLP.http}` }, grpc: { endpoint: `0.0.0.0:${_OTLP.grpc}` } } } },
        // declined: the metrics-generator would mint a SECOND span-derived series family beside the collector's connectors
        metricsGenerator: { enabled: false },
      },
      persistence: { enabled: true, size: "20Gi" },
    })
    if (observe.profiles) {
      row("pyroscope", {
        pyroscope: {
          persistence: { enabled: true, size: "20Gi" },
          extraArgs: { "pyroscopedb.retention-policy-min-free-disk-gb": "10" },
          structuredConfig: { limits: { max_query_lookback: observe.retention }, multitenancy_enabled: isolated },
        },
        // this chart bundles a whole Grafana Alloy collector DAEMONSET default-on — a second per-node agent beside
        // the one ingest seam this tier owns, dialing its own destinations; the legacy `agent` and the bundled
        // `minio` are the row's other two subtiers and both ship off, so this is the one it owes a tombstone
        alloy: { enabled: false },
      })
    }
    if (observe.costs) {
      row("opencost", {
        opencost: {
          prometheus: {
            // This chart SUMS its three upstream modes and refuses when more than one is on, and `internal` is
            // on by default — so arming `external` alone fails the render outright rather than degrading. The
            // key names the chart's own upstream dialect; the VALUE is the selected store row, so a store swap
            // re-points cost series with zero opencost edits.
            internal: { enabled: false },
            external: { enabled: true, url: this.urls.query.metrics },
          },
          // The exporter's UI and MCP server both ship default-ON, standing up two more containers and two more
          // service ports beside the metrics door — a second board plane next to Grafana and an agent surface
          // this estate never declared. The whole-page law that no row installs on chart defaults deletes both.
          ui: { enabled: false },
          mcp: { enabled: false },
        },
      })
    }
    if (observe.ebpf) {
      row("ebpf", {
        config: {
          data: {
            otel_traces_export: { endpoint: this.urls.collector },
            otel_metrics_export: { endpoint: this.urls.collector },
          },
        },
      })
    }
    // Credential custody: chart values render into a ConfigMap, so every credential rides a Secret and the config reads ${env:…}.
    const credential = new k8s.core.v1.Secret(`${name}-pg`, {
      metadata: { namespace: args.namespace },
      stringData: {
        PG_PASSWORD: pulumi.secret(args.data.password),
        // one generated read, two key spellings, because the two consumers read by different contracts: the collector
        // expands `${env:CH_PASSWORD}` off the whole Secret while the residence chart's `password_secret_name` reads
        // key `password` by name — a second Secret here mints a second custody owner for one value
        CH_PASSWORD: pulumi.secret(args.auth),
        password: pulumi.secret(args.auth),
        DATA_SOURCE_NAME: pulumi.secret(pulumi.interpolate`postgresql://${args.data.user}:${args.data.password}@${args.data.host}:${args.data.port}/${args.data.database}`),
      },
    }, this.child())
    // Residences install before the exporter writing them: `create_schema: false` means tables must already
    // exist, and the chart's init-script hook is the one place DDL runs without minting a job the estate then owns.
    // The ROSTER drives the fold and each row's own `chart` column answers whether it installs and with what body, so
    // a second chart-bearing residence costs one column answer and no edit here, and a key literal at this seat can
    // never disagree with the row it claims to install. What is NOT the row's is the exporter's address, which its
    // `exporter` column already answered.
    Array.forEach(residences, (key) =>
      Option.match(_RESIDENCE[key].chart(), {
        onNone: () => undefined,
        onSome: (install) => {
          const schema = new k8s.core.v1.ConfigMap(`${name}-${install.key}-ddl`, {
            metadata: { namespace: args.namespace },
            data: { "schema.sql": install.ddl },
          }, this.child())
          row(install.key, install.values({ credential: credential.metadata.name, schema: schema.metadata.name }))
        },
      }))
    // Every realized row carrying a gateway leg hands the collector its OWN write address, folded once here — so the
    // exporter map, the pipeline fan, and this roster read one column and a second collector-filled residence costs
    // no edit below. A row a data plane fills contributes nothing, exactly as its `exporter` column already said.
    const seated = pulumi.all(Array.filterMap(residences, (key) =>
      Option.map(
        _RESIDENCE[key].exporter(),
        (leg) => leg.seat({ release: name, namespace: args.namespace }).apply((endpoint) => ({ key, endpoint })),
      )))
    // One compile, two readers: the ConfigMap body and the store row's own values block read the same array, so a row
    // whose loader is a filesystem and a row whose loader is a values key can never evaluate different rules.
    const compiled = store.rules ? _groups(args.alerts, this.targets.metrics.target) : []
    const groups = new k8s.core.v1.ConfigMap(`${name}-rules`, {
      metadata: { namespace: args.namespace },
      data: { "rules.yaml": pulumi.jsonStringify({ groups: compiled }) }, // YAML admits JSON, so one encoder serves both loaders
    }, this.child())
    charts.push(new k8s.helm.v4.Chart(`${name}-store`, {
      chart: store.chart,
      repositoryOpts: { repo: store.repo },
      version: args.versions.store,
      namespace: args.namespace,
      ...provenance,
      values: store.values({
        fullname: `${name}-store`,
        retention: observe.retention,
        rules: { groups: compiled, configMap: groups.metadata.name, directory: _COLLECTOR.rules.path, tenant: args.spec.app },
        translation: store.translation, // the family column reaches the row's own dialect through the one install record
        histogram: store.histogram, // same path: the representation column arms the row's own dialect and renders the matching query arm
        objects: args.objects,
      }),
    }, this.child()).resources)
    if (observe.ingest === "scrape") {
      // infra exporter is a tier child, never an app workload: the DSN reaches it by secret ref, and the collector alone scrapes it
      new k8s.apps.v1.Deployment(`${name}-pg-exporter`, {
        metadata: { namespace: args.namespace, labels: { app: "postgres-exporter" } },
        spec: {
          selector: { matchLabels: { app: "postgres-exporter" } },
          template: {
            metadata: { labels: { app: "postgres-exporter" } },
            spec: {
              containers: [{
                name: "exporter",
                image: pulumi.interpolate`${args.versions.exporter.repository}@${args.versions.exporter.digest}`, // workloads run a digest and charts pin a version; one field carrying both is how a tag reached a `version:`
                env: [{ name: "DATA_SOURCE_NAME", valueFrom: { secretKeyRef: { name: credential.metadata.name, key: "DATA_SOURCE_NAME" } } }],
              }],
            },
          },
        },
      }, this.child())
      new k8s.core.v1.Service("postgres-exporter", {
        metadata: { name: "postgres-exporter", namespace: args.namespace },
        spec: { selector: { app: "postgres-exporter" }, ports: [{ port: 9187 }] },
      }, this.child())
    }
    const queue = new k8s.core.v1.PersistentVolumeClaim(`${name}-queue`, {
      metadata: { namespace: args.namespace },
      spec: { accessModes: ["ReadWriteOnce"], resources: { requests: { storage: `${_COLLECTOR.queue.gib}Gi` } } },
    }, this.child()) // the queue directory outlives a reschedule only on a claim; an emptyDir erases every accepted batch
    // Both collector tiers install through one row: gateways carry the whole component set, agents carry the same
    // admission legs aimed at the gateway's two doors, and neither tier spells a coordinate the other invented.
    const collector = (key: "agent" | "collector", shape: pulumi.Inputs, gateway: pulumi.Output<string> | undefined) =>
      row(key, {
        mode: key === "agent" ? "daemonset" : "deployment",
        image: { repository: "otel/opentelemetry-collector-contrib" }, // standing, not an escalation: file_storage and every pg receiver ship contrib-only
        clusterRole: { create: true, rules: _RBAC }, // the price of owning processor ordering instead of surrendering it to a preset
        extraEnvsFrom: [{ secretRef: { name: credential.metadata.name } }],
        // Daemonsets render NO Service unless this row asks for one, and the chart's own default policy then keeps a
        // pod on its node's agent — which lets one published door serve both topologies with no downward-API hop.
        ...(key === "agent" ? { service: { enabled: true } } : {}),
        ports: _ports(key === "collector" && agents),
        ...shape,
        // an agent tier defines no residence leg at all, so the roster resolves once here and the plan's own role
        // discriminant drops it — a daemonset never carries a residence credential onto every node
        config: pulumi.all([this.urls.ingest.logs, this.urls.ingest.traces, this.urls.ingest.metrics, this.urls.ingest.profiles, seated, gateway ?? pulumi.output("")])
          .apply(([logs, traces, metrics, profiles, columnar, door]) =>
            _pruned(_plan({
              observe,
              ingest: { logs, traces, metrics, profiles },
              residences: columnar,
              gateway: door === "" ? undefined : { arrow: `${door}:${_COLLECTOR.arrow.port}`, otlp: `http://${door}:${_OTLP.http}` },
              ttl: `${_COLUMNAR.ttlDays * 24}h`, // the exporter's own TTL and the DDL's read one anchor, so a drift between them cannot exist
              infra: _infra(observe, args.data, { release: name, namespace: args.namespace }),
              infraReceivers: _infraReceivers(observe.ingest),
              headers: _scoped(store, args.spec.app),
            }))),
      })
    collector("collector", {
      extraVolumes: [{ name: "queue", persistentVolumeClaim: { claimName: queue.metadata.name } }],
      extraVolumeMounts: [{ name: "queue", mountPath: _COLLECTOR.queue.path }],
      resources: { limits: { memory: "2Gi", cpu: "2" }, requests: { memory: "1Gi", cpu: "500m" } }, // memory_limiter reads these percentages; without a limit the guard has no ceiling
    }, undefined)
    if (agents) {
      // one claim per node is unschedulable on a daemonset, so the agent leg's durability IS its retry horizon and its
      // queue stays in memory — the disk queue is a gateway capability, stated rather than silently absent
      collector("agent", {
        resources: { limits: { memory: "512Mi", cpu: "500m" }, requests: { memory: "256Mi", cpu: "100m" } },
      }, pulumi.interpolate`${_charts.collector.service(`${name}-collector`)}.${args.namespace}.svc`)
    }
    this.rendered = pulumi.all(charts).apply(Array.flatten)
    this.collectorEndpoint = this.urls.collector
    // both backend families seal with their declared degradation, so a consumer reads what this stack gives up on
    // evidence exactly where it reads what it gives up on series — one `_Plane` floor, one seat, two answers
    this.store = store
    this.residences = Array.map(residences, (key) => _RESIDENCE[key])
    this.seal({
      urls: this.urls,
      collectorEndpoint: this.collectorEndpoint,
      rendered: this.rendered,
      store: this.store,
      residences: this.residences,
      targets: this.targets,
    })
  }
}
```

## [04]-[SCRAPE_ROWS]

[SCRAPE_ROWS]:
- Owner: the `_infra`/`_infraReceivers` pair — the whole pull-side plane the gateway owns, keyed on the coordinates that install its surfaces. `spec.profile.observe.ingest` selects the pg-server arm: `scrape` models `postgres_exporter` as one `prometheusreceiver` job over the exporter's `/metrics` (the exporter running as one values-driven child beside the data tier with its DSN from the in-graph data read), and `native` is the OTLP-native alternative — the collector's `postgresqlreceiver` dialing the pooler directly beside the `sqlquery` view row, no exporter container at all — while `_SCRAPED` carries every scrape job either arm renders, each gated by its own coordinate, so the receiver's job list is a fold over one roster and never a per-arm literal.
- Law: one ingress holds for pull — the collector scrapes, never a store-side scrape config, so every signal enters through the one gateway and a store swap re-points exporters without touching ingestion; app telemetry never rides scrape — the plane exists exactly for infra surfaces that expose `/metrics` and cannot push.
- Law: an installed exporter and its scrape row arm on ONE coordinate — the row IS the hop between a `/metrics` door and the store every board reads, so a chart row armed without one publishes a door nothing dials and its whole plane compiles into tiles that render empty while every component reports healthy; the pricing exporter is the case that proves it, since `observe.costs` installs the chart and the same value renders the job.
- Law: the two receivers take opposite credential shapes and the arm answers both — `postgresqlreceiver` dials `host:port` with `username`, `password`, `databases`, and `tls` as discrete fields (a DSN in its `endpoint` parses as a hostname and never connects), while `sqlqueryreceiver` takes one assembled `datasource` string; the arm therefore receives the pg coordinates discrete and assembles only where the receiver demands it, and the password reaches BOTH as `${env:PG_PASSWORD}` off the tier's Secret so no credential ever renders into the chart's ConfigMap.
- Law: `postgresqlreceiver` carries block and WAL depth (`postgresql.blks_hit`/`postgresql.blks_read`, `postgresql.wal.age`/`postgresql.wal.delay`) and no `pg_stat_io` column at all, so that whole view rides the `sqlquery` receiver row beside the `native` arm — every numeric column emitted at the view's own grain — and both receivers join the metrics pipeline as one arm.
- Law: `_IO_COLUMNS` names each pg18 column against an `_IO_KINDS` row, and one fold generates the select list, the metric rows, and the `postgresql.io.<column>` names off that pairing, so no series name is hand-spelled and a server minor adding a counter is one row; pg18 deleted `op_bytes` and the block-multiplier arithmetic it existed for, so byte volumes read from `read_bytes`/`write_bytes`/`extend_bytes` and cast to `bigint` because an `int` datapoint parses as a plain integer and a `numeric` rendering carries a decimal point.
- Law: grain is the view's own `(backend_type, object, context)` triple stamped as attributes with nothing aggregated — `object` separates relation from `wal` and `temp relation`, `context` separates `normal` from `bulkread`, `bulkwrite`, `vacuum`, and `init`, so a `GROUP BY` folding them reports one number over four unrelated I/O paths; the triple product is a server-build fact, fixed per cluster and never scaled by tenants.
- Law: every triple emits every collection, active or idle — a series appearing and vanishing on activity resets its rate at each gap — and `COALESCE` carries the cells the server does not track (a `wal` row takes no `hits`, a read-only context no `extends`) as honest zeroes rather than dropped datapoints.
- Law: counters ride cumulative by server truth — `data_type: "sum"` with `monotonic: true` and `aggregation: "cumulative"` is the policy-row alternative to the estate's DELTA wire default, earned because `pg_stat_io` accumulates server-side; `start_ts_column` carries the cumulation start as the integer nanosecond epoch the receiver parses, projected off `stats_reset` falling back to `pg_postmaster_start_time()`, so `pg_stat_reset_shared('io')` reads as a new window instead of a counter rollover.
- Law: `_PG_POLL` owns the read posture for BOTH pg receivers — pg flushes cumulative statistics to shared memory no faster than once per second so a tighter cadence re-reads one snapshot, every scraper-controller receiver ships its query timeout at zero so a read blocked behind a lock stalls collection unbounded, and the connection cap holds the arm to one serial reader on the pooler; a bound stated on the DSN receiver alone leaves the sibling dialing the same server through the same locks with no deadline at all.
- Law: CNPG operator metrics enter the same door — the `cnpg` row renders on both arms (`kubernetes_sd_configs` pod role narrowed to the `cnpg.io/cluster` label, every hit relabelled onto the instances' metrics listener), so operator and instance health land in the selected store under the one-ingress law and no store-side scrape config exists.
- Law: every listener port a row targets rides `_INFRA_PORT` — the exporter workload's container port, the CNPG relabel's replacement, and each row's own target read one anchor, so a port a workload publishes and a port a job dials cannot be two numbers.
- Law: server-side depth is a database fact — the data tier's cluster config carries `pg_stat_statements` (with `compute_query_id`) and pg18 `pg_stat_io` as standing rows, so whichever arm runs, the series it harvests exist.
- Packages: `@pulumi/pulumi` (`Input`, `interpolate`); `effect` (`Array`, `Option`, `Record`); `../program/spec.ts` (`StackSpec`); `opentelemetry-collector` (`prometheus`, `postgresql`, and `sqlquery` receivers with the `queries[].metrics[]` row shape).
- Growth: a second pull-side surface is one `_SCRAPED` row answering `armed` and `job` beside one `_INFRA_PORT` entry; a counter a pg major adds is one `_IO_COLUMNS` row, and a column type it introduces is one `_IO_KINDS` row.
- Boundary: the pg coordinates and the exporter image ref arrive as args from the composing arm; the data tier owns the server config rows, `track_io_timing` and `track_wal_io_timing` among them — without them the timing columns read zero, never null.

```typescript signature
// Metrics doors the estate's own infra surfaces publish. One anchor, three readers: the exporter Deployment's
// container port, the CNPG relabel rewriting every discovered pod onto its instance listener, and the target each
// scrape row spells — so a port is stated once and no job re-types a number a workload already published.
const _INFRA_PORT = { pgExporter: 9187, opencost: 9003 } as const

// Pull-side infra surfaces ride ONE prometheus receiver under the one-ingress law, each a row gated by the very
// coordinate that installs it. A row IS the hop carrying an exporter's series into the store every board reads, so
// an installed exporter holding no row here publishes a `/metrics` door nothing dials and its whole plane renders as
// empty tiles rather than as a capability the spec declined — the failure a chart row and an ingest row split across
// two owners produces every time only one of them is armed.
const _SCRAPED = {
  // the pg exporter is the scrape arm's own child; the native arm dials the server directly and installs none
  postgres: {
    armed: (observe: StackSpec.Observe) => observe.ingest === "scrape",
    job: () => ({ job_name: "postgres", static_configs: [{ targets: [`postgres-exporter:${_INFRA_PORT.pgExporter}`] }] }),
  },
  // operator + instance health: pod discovery narrowed to the cluster label, every hit rewritten onto the instance listener
  cnpg: {
    armed: () => true,
    job: () => ({
      job_name: "cnpg",
      kubernetes_sd_configs: [{ role: "pod" }],
      relabel_configs: [
        { source_labels: ["__meta_kubernetes_pod_label_cnpg_io_cluster"], action: "keep", regex: ".+" },
        {
          source_labels: ["__address__"],
          action: "replace",
          regex: "([^:]+)(?::\\d+)?",
          replacement: `$1:${_INFRA_PORT.pgExporter}`,
          target_label: "__address__",
        },
      ],
    }),
  },
  // the pricing exporter READS the selected store and emits its OWN cost series on its own door, so installing it and
  // ingesting it are one arming decision keyed on one coordinate: an armed chart row with no scrape row prices the
  // estate onto a door nothing dials, and every cost tile the board plane compiles then renders against series no
  // producer ever landed — a plane that reads as broken exactly where the spec said it was on
  opencost: {
    armed: (observe: StackSpec.Observe) => observe.costs,
    job: (bind) => ({
      job_name: "opencost",
      static_configs: [{
        targets: [pulumi.interpolate`${_charts.opencost.service(`${bind.release}-opencost`)}.${bind.namespace}.svc:${_INFRA_PORT.opencost}`],
      }],
    }),
  },
} as const satisfies Record.ReadonlyRecord<string, {
  readonly armed: (observe: StackSpec.Observe) => boolean
  readonly job: (bind: { readonly release: string; readonly namespace: pulumi.Input<string> }) => Record.ReadonlyRecord<string, unknown>
}>

const _PG_POLL = { interval: "30s", timeout: "10s", connections: 1 } as const

const _IO_GRAIN = ["backend_type", "object", "context"] as const

const _IO_KINDS = {
  // kinds key on the pg18 source column type: `bigint` counts, `numeric` byte volumes, `double precision` millisecond timings
  count: { unit: "{operation}", value: "int", cast: "" },
  bytes: { unit: "By", value: "int", cast: "::bigint" },
  time: { unit: "ms", value: "double", cast: "" },
} as const satisfies Record<string, { readonly unit: string; readonly value: "int" | "double"; readonly cast: string }>

const _IO_COLUMNS = {
  // rows hold the pg18 view's own column order and spelling, one per tracked IOOp
  reads: "count", read_bytes: "bytes", read_time: "time",
  writes: "count", write_bytes: "bytes", write_time: "time",
  writebacks: "count", writeback_time: "time",
  extends: "count", extend_bytes: "bytes", extend_time: "time",
  hits: "count", evictions: "count", reuses: "count",
  fsyncs: "count", fsync_time: "time",
} as const satisfies Record<string, keyof typeof _IO_KINDS>

const _IO_START = "reset_ns" // start_ts_column parses an integer nanosecond epoch, never a rendered timestamp

const _IO_QUERY = {
  sql: `SELECT ${Array.join([
    ..._IO_GRAIN,
    `(EXTRACT(EPOCH FROM COALESCE(stats_reset, pg_postmaster_start_time())) * 1000000000)::bigint AS ${_IO_START}`,
    ...Array.map(Record.toEntries(_IO_COLUMNS), ([column, kind]) => `COALESCE(${column}, 0)${_IO_KINDS[kind].cast} AS ${column}`),
  ], ", ")} FROM pg_stat_io`,
  metrics: Array.map(Record.toEntries(_IO_COLUMNS), ([column, kind]) => ({
    metric_name: `postgresql.io.${column}`,
    description: `pg_stat_io ${column} by ${Array.join(_IO_GRAIN, ", ")}`,
    value_column: column,
    value_type: _IO_KINDS[kind].value,
    unit: _IO_KINDS[kind].unit,
    attribute_columns: [..._IO_GRAIN], // the receiver reads attribute columns per metric row; the query level carries no such field
    data_type: "sum",
    monotonic: true,
    aggregation: "cumulative",
    start_ts_column: _IO_START,
  })),
}

const _infra = (
  observe: StackSpec.Observe,
  data: Lgtm.Data,
  bind: { readonly release: string; readonly namespace: pulumi.Input<string> },
) => ({
  ...(observe.ingest === "scrape" ? {} : {
    postgresql: {
      // this receiver dials a host:port and takes its credential as discrete fields; a DSN here parses as a hostname and never connects
      endpoint: pulumi.interpolate`${data.host}:${data.port}`,
      transport: "tcp",
      username: data.user,
      password: "${env:PG_PASSWORD}",
      databases: [data.database],
      tls: { insecure: false, insecure_skip_verify: false },
      collection_interval: _PG_POLL.interval,
      // EVERY scraper-controller receiver ships its timeout at zero, which is no deadline — this one runs many
      // statements per collection against the same locks its sibling reads through, so the bound rides both or the
      // arm's stall posture holds on exactly one of the two receivers dialing one server
      timeout: _PG_POLL.timeout,
      connection_pool: { max_open: _PG_POLL.connections, max_idle: _PG_POLL.connections, max_idle_time: "10m" },
    },
    sqlquery: {
      driver: "postgres",
      // this receiver takes the assembled DSN instead: datasource and the individual fields are mutually exclusive at its validation
      // this env reference escapes with ONE backslash so the template emits `${env:…}` verbatim for the collector
      // to expand; two make the dollar an interpolation of `env:PG_PASSWORD`, which is not an expression at all
      datasource: pulumi.interpolate`host=${data.host} port=${data.port} user=${data.user} password=\${env:PG_PASSWORD} dbname=${data.database} sslmode=require`,
      collection_interval: _PG_POLL.interval,
      timeout: _PG_POLL.timeout,
      max_open_conn: _PG_POLL.connections,
      queries: [_IO_QUERY],
    },
  }),
  // one receiver, every armed pull-side surface: the roster decides which jobs render, so a second infra exporter is
  // one `_SCRAPED` row and no edit here, and no store-side scrape config exists on either arm
  prometheus: {
    config: {
      scrape_configs: Array.filterMap(Record.keys(_SCRAPED), (key) =>
        _SCRAPED[key].armed(observe) ? Option.some(_SCRAPED[key].job(bind)) : Option.none()),
    },
  },
})

const _infraReceivers = (ingest: StackSpec.Observe["ingest"]): ReadonlyArray<string> =>
  ingest === "scrape" ? ["prometheus"] : ["postgresql", "sqlquery", "prometheus"]
```

## [05]-[DEV_ROW]

[DEV_ROW]:
- Owner: `Dev` — the docker arm's whole observability estate as one exported tier: the `_DEV` anchor carries the all-in-one image's two port planes (`edge` rows the host publishes, `query` rows the bundled Grafana reads container-locally), and the tier publishes the same `Lgtm.Urls` plane and `collectorEndpoint` the k8s tier publishes, so `provider.md`'s docker arm returns the `otlp` and `grafana` output planes from either producer.
- Law: the dev loop is byte-identical at the SDK seam — the app's export config is the one `StackOutputs.channels["otlp.endpoint"]` row on both arms, so moving an app between loops edits zero telemetry config and a signal that renders in the dev pane renders in the estate pane.
- Law: the dev image is the dev arm's whole backend — the k8s arm never runs it; its bundled logs store differs from the estate row, a bounded asymmetry the query plane absorbs as one datasource row; the image publishes the edge rows (Grafana `3000`, OTLP `4317`/`4318`, Tempo `3200`, Pyroscope `4040`, Prometheus `9090`) while Loki's `3100` listener stays container-local, which is exactly the posture the `query` rows encode — the bundled Grafana reads them from inside the container's own network namespace.
- Law: the dev loop prices nothing — no allocation feed exists on a docker daemon, so the `costs` toggle has no dev realization and the degrade is stated here, never discovered on an empty board.
- Law: the dev loop holds no residence and its own `Targets` says so — the all-in-one image ships no columnar store and the loop's lifetime is shorter than any evidence horizon, so this arm answers `analytics: None` whatever `observe.analytics` names, and a board plane resolving realization from the spec instead provisions a ClickHouse driver against the empty address this arm publishes and reads it as an authentication failure. Profiles answer the inverse asymmetry on the same terms: this image runs that backend regardless, so this arm answers `true` where the spec value decides only which chart the k8s arm installs.
- Law: boards apply identically and RENDER differently — `Boards` consumes `Dev.urls` and `Dev.targets` exactly as it consumes the k8s tier's, so the dev pane carries the same folder, sources, dashboards, and alert rules the estate carries, authenticated by the generated credential the container's admin env row seeds; every answer the board plane needs about the metrics door — the target, the driver, the scope header, the exemplar posture, the recorded-numerator posture — rides this arm's own `Targets` value, because the `_stores` row a spec names is the K8S arm's install and describes nothing this container runs.
- Law: this arm answers the store family's own decisive columns for the store IT runs — `translation` and `histogram` both — and the two answers land on opposite sides: the bundled server takes the OTLP receiver's escaping default, so a pack rendered against the estate pin selects `rasm.invoke.calls` where the loop stores `rasm_invoke_calls_total` and matches nothing while no component faults, while representation AGREES with the estate row, because that same receiver leaves its histogram conversion unstated and unstated is the native answer. Publishing the loop's own answers is what makes the two panes differ in VALUES rather than in whether they render at all, and it is what keeps the agreeing column a reading rather than an assumption — the column that agrees today and the column that never did read identically to a pane that asserts parity.
- Law: pinning the image's own backends is the DECLINED alternative, never an unavailable one — the image publishes a per-backend extra-argument seam and mounts every backend config path, which reaches the escaping strategy through a replacement of the store's whole config file; the loop's value is running the published image unmodified, so the arm states what the image holds and renders against it, and a stack demanding estate-identical series names escalates to the k8s arm rather than forking the dev image.
- Law: the loop evaluates no rules — no loader reaches the bundled store, so `recorded` is false here and every burn numerator renders inline through the same core projection the estate's evaluator-less store row renders through; reading the estate row's rule posture instead spells recorded series names on an arm that records none.
- Growth: a dev-loop port or knob is one `_DEV` field.

```typescript signature
import * as docker from "@pulumi/docker"

const _DEV = {
  image: "grafana/otel-lgtm",
  edge: { grafana: 3000, otlpGrpc: _OTLP.grpc, otlpHttp: _OTLP.http, pyroscope: 4040 },
  query: { metrics: 9090, loki: 3100, tempo: 3200, pyroscope: 4040 },
  // This image bundles its own store and this arm hands it no receiver configuration, so it runs the OTLP receiver's
  // own escaping default rather than the estate pin an `_stores` row spells. Carrying that as a row lets the loop's
  // target render the names the loop actually holds; asserting parity renders every panel against a series spelling
  // that exists on neither pane.
  translation: "UnderscoreEscapingWithSuffixes" satisfies Convention.Translation,
  // Representation answers on the same terms and the argv is NOT where it is decided: the bundled server's flags
  // carry the OTLP receiver and exemplar storage and no native-histogram feature, which arms nothing either way,
  // because that feature name is a no-op the server warns about while ingesting natively regardless. The receiver's
  // own conversion knob is the whole split, the image's config leaves it unstated, and unstated IS the native
  // answer — so this loop holds one series per histogram and every quantile tile renders the native arm. The column
  // AGREES with the reference row here, which is precisely why it is read rather than assumed: an agreement and a
  // parity claim are indistinguishable on the pane, and only one of them survives the image moving its config.
  histogram: "native" satisfies Board.Query.Histogram,
  // Bundled Grafana provisions its own datasources, yet this tier reprovisions them by name over the same URL
  // plane; this driver names the store the image runs, and no scope header exists because the loop holds one tenant.
  plugin: "prometheus",
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
  readonly targets: Lgtm.Targets
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
      collector: otlp, // the image's own door is the tier's collector row: one `Lgtm.Urls` shape serves either producer
      ingest: { logs: otlp, traces: otlp, metrics: otlp, profiles: otlp }, // the bundled collector is the one ingest seam: every signal enters the same OTLP door
      // this image carries no residence, so the analytics door resolves to the loop's own absence rather than to a
      // port nothing serves: `observe.analytics` realizes no plane here and the degrade row says so
      query: { loki: local(_DEV.query.loki), tempo: local(_DEV.query.tempo), metrics: local(_DEV.query.metrics), pyroscope: local(_DEV.query.pyroscope), residence: pulumi.output("") },
    }
    this.collectorEndpoint = this.urls.collector
    // This loop renders against the spelling and the representation its own bundled store holds, and no residence to
    // render SQL against, so `Boards` reads one shape from either producer and the values alone differ.
    this.targets = {
      metrics: {
        target: Board.Query.promql({ histogram: _DEV.histogram, source: "metrics", translation: _DEV.translation }),
        plugin: _DEV.plugin,
        scope: {}, // one tenant in one container: no isolation header exists for this arm to stamp
        exemplars: true, // the bundled server runs exemplar storage, so the click-through into the loop's traces holds
        // no rule loader reaches the bundled store, so every burn numerator renders inline here rather than reading
        // a recorded series this arm never writes — the same column the estate's evaluator-less store row answers
        recorded: false,
      },
      analytics: Option.none(),
      // this image runs its profile backend whatever the spec says, so this arm answers for the container it
      // started rather than for the chart the k8s arm installs
      profiles: true,
    }
    this.seal({ urls: this.urls, collectorEndpoint: this.collectorEndpoint, targets: this.targets })
  }
}
```

## [06]-[ENDPOINT_PROJECTION]

[ENDPOINT_PROJECTION]:
- Law: endpoints are one projection in three role planes — `_urls` derives every address from one bind record (release, namespace, the selected store row, the resident residence set, and the object plane's coordinates) under the fullname each chart row pins, with `ingest` rows carrying each backend's write path (Loki OTLP, Tempo OTLP, the store row's `admit`, Pyroscope ingest) for the collector exporters, `query` rows carrying each backend's read API (Loki, Tempo, the store row's `read`, Pyroscope) for the Grafana data sources, and `collector` carrying the one door every workload and every eBPF exporter dials — one backend, its roles, never one URL doing two jobs; a chart bump that renames a service or moves a path edits exactly these rows and the store rows, and no consumer ever spells a service DNS.
- Law: the collector door is a projection row, never a tier-local literal — it derives from the same pinned fullname `row()` installs, so the ONE address whose failure kills every workload's telemetry egress, both eBPF exports, and `StackOutputs.otlp` is proved by the same construction that proves the four backend addresses, instead of resting on a helm fullname helper collapsing the way a reader assumed.
- Law: that door is keyed by the collector topology — `_DOOR` names which chart row a workload dials, so an agent estate publishes the node-local daemonset and a gateway estate the deployment, and one published address serves both without a workload ever learning a second spelling or a downward-API hop.
- Law: each row answers its own rendered Service name, never the projection's assumption — `svc` reads each row's own `service` projection over the pinned fullname, because a chart whose workload is a custom resource hands naming to its controller and that controller decorates the pinned name; assuming the two agree re-opens the dead-address class for exactly the rows a reader never thinks to check.
- Law: residences publish a READ door here and their WRITE seat on their own exporter column — the collector dials that seat's native port inside the tier and no consumer spells it, while `query.residence` is the one address a query end binds and `StackOutputs.analytics` carries outward; the realized row's own `door` projection answers it, so the interactive plane publishes its service and the cold tail publishes the object-plane catalog prefix under one row name, and a stack realizing neither publishes the one empty refusal at this seam rather than a sentinel each row invents or a service address the tier never installed.
- Law: consumers bind outputs, not literals — `urls.grafana` feeds the `Boards` provider and `StackOutputs.grafana`, `urls.query.*` feeds the `Boards` data sources, `urls.collector` feeds `collectorEndpoint`, the `otlp` output plane, and thence the workload env; a hand-written URL anywhere downstream is the drift this projection deletes.
- Law: a query row is named for its PLANE and the residence door resolves to its BOARD-readable row — `query.metrics` resolves to the selected store and `query.residence` to the realized residence carrying a driver, falling to the resident head where none does, so one address serves the panel driver and `StackOutputs.analytics` alike; naming a row for an engine binds every escalation to the reference row's spelling, and reading the head unconditionally aims a driver at whichever plane the resident order happened to lead with.
- Law: render coordinates are an endpoint-class projection — `targets` derives from the same selection `_urls` derives from and seals beside it, because the address a reader dials and the grammar it dials with are one decision; publishing the address alone leaves the composition root to re-mint the grammar, which is a hand-written URL wearing a different type.
- Law: one search answers both, never two spelled alike — `_boarded` finds the realized row a driver reads and `_addressed` falls from it to the resident head, so the published door and the residence coordinate name ONE row by construction; two copies of that search resolve to two planes the day the resident order changes, publishing an address for one residence beside a coordinate for another with nothing to raise on the mismatch.
- Growth: a new backend's address is one ingest row and one query row beside its chart row.
- Boundary: ports and path suffixes are the pinned charts' facts, versioned with the pins.

```typescript signature
// Which collector row every workload dials, keyed by the topology axis: a gateway estate publishes the deployment,
// and an agent estate publishes the daemonset, whose own Service policy keeps a pod on its node's agent. Publishing
// that gateway on both arms installs a per-node tier no workload ever reaches — an escalation costing a daemonset and
// buying nothing. A new topology value is one row.
const _DOOR = { gateway: "collector", agent: "agent" } as const satisfies Record<StackSpec.Observe["topology"], keyof typeof _charts>

const _urls = (bind: {
  readonly release: string
  readonly namespace: pulumi.Input<string>
  readonly store: (typeof _stores)[keyof typeof _stores]
  // Resident rows drive the residence door: the FIRST armed row publishes it, so `both` publishes the interactive
  // plane while the cold tail stays readable off the object plane's own output
  readonly residences: ReadonlyArray<keyof typeof _RESIDENCE>
  readonly objects: Lgtm.Args["objects"]
  readonly topology: StackSpec.Observe["topology"]
}): Lgtm.Urls => {
  const { namespace, release, store } = bind
  // `${release}-${key}` IS the fullname row() pinned, and the row's own `service` projection is what the cluster
  // actually renders — identity where the chart names its Service, the controller's decoration where it does not
  const svc = (key: keyof typeof _charts, port: number, path = ""): pulumi.Output<string> =>
    pulumi.interpolate`http://${_charts[key].service(`${release}-${key}`)}.${namespace}.svc:${port}${path}`
  return {
    grafana: svc("grafana", 80),
    collector: svc(_DOOR[bind.topology], _OTLP.http),
    ingest: {
      logs: svc("loki", 3100, "/otlp"),
      traces: svc("tempo", _OTLP.http),
      metrics: store.admit(`${release}-store`, namespace),
      profiles: svc("pyroscope", 4040),
    },
    query: {
      loki: svc("loki", 3100),
      tempo: svc("tempo", 3200),
      metrics: store.read(`${release}-store`, namespace),
      pyroscope: svc("pyroscope", 4040),
      // Realized rows publish their own door; the write door is each row's own `exporter` seat and no consumer spells
      // it, so a stack realizing no residence publishes the one empty refusal at this ONE seam rather than a sentinel
      // each row invents. `_addressed` is the same resolution the tier's render coordinate takes, so an address a
      // reader dials and whichever row that coordinate names can never be two different planes.
      residence: Option.getOrElse(
        Option.flatMap(_addressed(bind.residences), (key) => _RESIDENCE[key].door({ release, namespace, objects: bind.objects })),
        () => pulumi.output(""),
      ),
    },
  }
}
```

## [07]-[BOARD_APPLY]

[BOARD_APPLY]:
- Law: the compile leg is the Foundation-SDK builder fold — `_compiled` decodes each model once, lands `uid`/`title`/`tags`/`refresh` member-for-member with `since` on `time`, lands the decoded variable array through `variables` and every annotation row through `annotation` (slug as the name, tone as the marker color), and `.build()` emits the Grafana JSON `pulumi.jsonStringify` posts as `configJson` — compiled once per model and applied to every org that carries it.
- Law: the verified panel fold is an exhaustive record dispatch — `_minted` composes gauge ceilings and thresholds, units, Logs display rows, the Heatmap colour mode and colour scale through the one options builder that carries both, the Geomap coordinate layer, its typed marker style bindings (colour/label/weight through the `common` dimension builders into the layer's one untyped `config` envelope), and zoom controls, the Table sort and footer, the Timeseries axis and tooltip rows, the Nodes zoom mode, and the Nodes identity/label/colour/stat mapping through the cataloged generic transformation member onto the panel's own lowercase frame-column convention (frames named `nodes`/`edges` at the target refId, because admission keys on exactly that or an `id` field); `_compiled` lands shared title, description, transparency, repetition, layout, links, targets, and transformations.
- Law: a core field with no builder member is a defect at ONE of the two owners and never a silent drop here — a value this plane's own enum cannot spell refuses at the compile seam through `_refuse`, and a field whose builder member does not exist at all is a deletion the core owner makes, so the compile leg either writes a declared field or names it; the Geomap label, weight, and colour mappings and the Nodes label, weight, and colour mappings are the open pair, each reaching a frame-column convention neither the pinned SDK's types nor the catalog enumerates, and `[08]` carries that question rather than a guessed column name.
- Law: the shared interaction row lands where its builder reads it — `tooltip` at the Timeseries arm, `zoom` at the Geomap controls pair and the Nodes zoom mode — so the two fields the core owner keeps each have a consumer here and the tags whose builders answer neither emit neither.
- Law: coordinate mapping IS the panel, never a decoration — a Geomap with no `location` layer plots the frame's first two numeric columns by accident, so the model's own latitude and longitude column names drive one `Coords`-mode marker layer over the row's basemap, and the same reasoning seats the Nodes rename in `_compiled`.
- Law: display ceilings are tier posture, never board data — `_CEILING` owns the Logs line ceiling and the Table pagination flag because each bounds what one panel renders regardless of which board mounts it, and hoisting either upstream mints a knob every author then answers.
- Law: which backend a panel reads is the MODEL's field and how it spells the query is the tier's — `panel.source` selects the `_SOURCES` row (its `type` projection and the row key as `uid`) and that row's `dialect` column selects the builder, while `_FORMED` keys the neutral wire form off the panel tag; deriving the backend from the tag instead binds every table to whatever that tag defaulted to and empties a residence tile in silence, and a key outside the roster refuses at this seam rather than compiling a panel bound to a datasource nothing provisioned.
- Law: a source key names the PLANE, never the engine — `metrics` is whichever `_stores` row the spec selected and `residence` whichever armed `_RESIDENCE` row carries a driver, so one compiled board binds under one uid across every escalation; keying the roster by engine names a `prometheus` datasource on a victoriametrics stack and a `clickhouse` datasource over an object-plane prefix, which is the same fork the store family already deleted at the ingest end.
- Law: realization gates provisioning and admission together — `_realized` reads each row's `present` column against the producer tier's own realized-backend projection, so a stack whose residence or profiles plane was never planted constructs no datasource for it and `_sourced` refuses a panel naming it at the compile seam; provisioning every declared row instead publishes doors dialing an empty address, which reads on the board as an authentication failure rather than as the absence the spec declared, and a provisioned datasource nothing exercises is the same defect wearing the opposite sign.
- Law: every backend answer arrives, none re-derives — `args.targets` is the producer tier's own projection off the rows IT installed: the metrics half carries the render target beside the driver, the scope header, the exemplar posture, and the rule posture, the analytics half carries the realized residence beside its whole board driver, and the profiles half carries whether that backend runs at all; so the rule leg, the objective leg, the datasource fold, the panel compile, and every incoming pack read one selection. Re-reading the spec here answers for the k8s install on an arm whose bundled container ships its own escaping default, its own driver, and no evaluator, names a residence that container never runs, and disarms a profile backend its image is serving — and every one of those disagreements is silent, because a mismatched selector matches nothing and a driver pointed at an empty address reads as an authentication failure rather than as the absence the tier declared.
- Law: refusals resolve at one owner — `_refuse` holds the whole reason vocabulary against a named coordinate, so an unrostered source key, a source rendering no dialect, and a form its dialect cannot spell each halt the deploy where the coordinate is still loggable; an arm minting its own message shape strands the coordinate an operator reads the failure by.
- Law: the alert datasource IS the selected store row — `_alerted` binds the metrics door, whose row dials that store's `read` projection under its own plugin and its own scope header, so an escalation re-points every rule with no alert edit; the row key stays the uid one compiled board binds in every org, which is why the key names the door and never the engine behind it.
- Law: the panel target contract is STRUCTURAL and the SDK's codegen roster bounds convenience alone — `withTarget` takes `cog.Builder<cog.Dataquery>`, `Builder<T>` declares `build(): T`, and `Dataquery` declares one marker method carrying no brand, private field, or registry probe, so `_ResidenceQuery` is a first-class target and an unbundled datasource is typed by writing its builder rather than by waiting for a bump; the marker thunk drops at serialization exactly as every generated builder's does, and the residence query record transcribes the plugin's own `CHSqlQuery` off `_COLUMNAR.plugin` rather than a spelling invented here.
- Law: the residence row answers `dialect`, `type`, and `settings` off ONE realized driver — the `_RESIDENCE` `plugin` column carries the driver, the language it speaks, and the coordinates it dials as one payload, so a residence family gaining an engine whose plugin reads another grammar against another listener renders and connects with no edit here; spelling any of the three at this row is the engine-name-as-family-key defect, and it survives undetected because the roster's one board-readable engine happens to be the one spelled.
- Law: one board reads both planes — the residence row answers the `query` column with its own dialect, so an evidence tile compiles through the identical `_minted`, `_target`, and layout fold a series tile rides and an incident review stops alternating between a dashboard and an ad-hoc console; evidence outliving the store's series window becomes readable where the operator already reads, and the model's own `source` is what mixes the two on one board.
- Law: every source row answers the whole column set — `query`, `type`, `url`, and the `settings` body one `_provisioned` fold splits three ways, its plain half into `jsonDataEncoded`, its sealed half into `secureJsonDataEncoded`, and its headers onto the resource's own typed map — so selecting a different backend changes values and never shape; a row whose driver dials coordinates rather than the provisioned address fills those bodies while a row that dials the address alone encodes empty documents, and a coordinate the family gains is a value edit at one row instead of a key the other rows silently drop.
- Law: a request header reaches a datasource through the resource's own header member and NOWHERE else — Grafana stores each as an indexed `httpHeaderName<N>`/`httpHeaderValue<N>` pair split across the two encoded documents, and the provider reserves both spellings and REFUSES either body carrying one, so a hand-fold reproducing Grafana's storage shape fails validation rather than landing the header; the scope header an `org`-tenancy store row demands is that member's one filler today, and every later row fills it the same way.
- Law: the residence read credential is the residence write credential — the tier's one generated auth reaches the chart's own user, the board provider, and this datasource row alike, so a second custody owner never appears for the read side and rotation moves one value; the door's own authority is read back for the driver's host because the plugin dials a host where the plane publishes an address, and an unrealized residence yields an empty host, which is the docker arm's declared analytics absence rather than a fault.
- Law: wire form is neutral vocabulary each dialect spells — `_FORMS` is the vocabulary a tag names once and `_DIALECTS` carries one ROW per renderable dialect answering every form in its own alphabet, so the residence's `format` ordinal pivots core's three-column long relation into the wide frame a series panel reads while LogQL selects a mode and PromQL takes its enum, and no dialect's alphabet leaks onto the shared tag row; a form a dialect declares no spelling for refuses at the target seam rather than rendering a silently empty tile, and typing the vocabulary against one backend pair aims the shared column at every plane outside that pair.
- Law: every panel emits at least one target (Logs its `filter`, Nodes its `nodes`/`edges` pair), `legend` lands on `legendFormat`, and `exemplar(true)` rides only range series gated on the metrics plane's own exemplar posture.
- Law: boards publish read-only — `_compiled` stamps `.readonly()` on every board because content is code and the UI is drift, the hash-diff provider posture and this presentation row are one law from two sides; a shared panel is one `library` row compiled through the same `_minted` fold into an `oss.LibraryPanel` `modelJson`, realized in the default org and every tenant org so a panel compiled once serves many boards.
- Law: producer packs ride one ingest arm — `_PACKS` is the closed provenance tuple, and each pack arrives as `{ wire, boards, alerts }` whose boards and alerts are already core-encoded values; the arm tags every compiled board with its wire and folds pack alerts into the same `_alerted` burn-rate leg. Producer-specific census translation belongs to its composition owner and is absent here; a tuple entry cannot substitute for that projection. One tuple entry admits a new provenance key, and an unknown wire fails at the typed boundary.
- Law: provenance keys are earned by a landed producer, and the earn-test runs BOTH ways — a tuple entry whose branch mints no pack projection types an ingest arm for a wire that can never arrive, while a branch landing one and holding no seat mints a pack the boundary refuses; the key enters with its projection, leaves with it, and a wish for one lives as a card at the producing branch.
- Law: the key rides INSIDE the projection that earns it, so this tuple originates no wire and one spelling stands at both ends by construction — every producer's pack value carries its own provenance member, which makes a pack arriving without a key unconstructable rather than merely irregular; a key first spelled here types an ingest arm against a name its own producer cannot stamp.
- Law: this tuple is the closed vocabulary and the producing branches hold no second roster — each pack carries a key as an open value and THIS boundary decides which keys exist, so a producer stamping an unadmitted wire refuses at the typed seam while no branch forks the admitted set into a local enumeration of its own.
- Law: signal owners minting measures, instruments, or a fault boundary claim no seat — a measure charter answers what a runtime emits and never what a folder renders, so the seat waits on a board-and-alert projection rather than on naming proximity.
- Law: delivery routes by severity as data — the `contacts` record carries one receiver row per severity kind (`page`, `ticket`); each present row realizes one `alerting.ContactPoint`, one `NotificationPolicy` matcher route on the spec's own `Convention.rasm.sloSeverity` row, and one `alerting.MessageTemplate` whose body carries the row's wording lead, runbook link, and the two sorted planes; a row carrying a `quiet` calendar realizes one `alerting.MuteTiming` bound onto its route.
- Law: labels and annotations split by what the provider DOES with each — labels are the routing, grouping, and silencing identity, so the spec's owned `Convention` rows (objective, burn, severity) land there and the rule's annotations carry the derived headline the spec's own `factor` and `spend` print; the inverse placement strands the whole SLO vocabulary in a plane no matcher, no `groupBies` roster, and no silence can read, and mints a free-string `severity` label beside a `Convention` row that already owns the concept.
- Law: grouping is the owned roster — `_GROUPED` names the service and tenant dimensions the breached series carries beside the objective and severity rows the rule labels, so one notification per objective per tenant replaces one per firing window, and the root policy declares it while every child route inherits it.
- Law: tenant read identity is org-scoped — each tenant org mints one viewer `oss.ServiceAccount` (`role: "Viewer"`, threaded `orgId`), one `oss.ServiceAccountPermissionItem` granting the operator team `Admin` custody over that identity, one `oss.OrganizationPreferences` pinning the tenant's first compiled board as the org home, and one `oss.ServiceAccountRotatingToken` under the same `_ROTATION` policy whose key egresses on the tier's `viewers` record for the composing arm to land in Doppler `{ value }` entries — tenant credentials ride Doppler custody exactly like the automation token, never a stack output.
- Law: fleet rolls annotate beside deploys — each `rolls` row is the AppHost roll wire consumed as data (`wave`, `channel`, `verdict`, `hosts`), realized as one `oss.Annotation` whose text carries the roll coordinates and whose tone rides the `_ROLL_TONES` verdict row so a rollback reads as loud as an advance on every board; the record shape is the AppHost mint, this fold never re-derives roll facts.
- Law: profiles compile as typed queries and links, never as a fabricated panel — the pinned SDK ships `grafanapyroscope.DataqueryBuilder` and no visualization builder at all, so `_profiled` renders the profile selector once and every board carries it as a data link into the Pyroscope explorer scoped by the reading tenant; hand-written JSON for a panel the SDK cannot type is exactly the drift this compile leg deletes, and a provisioned datasource nothing exercises is the same defect wearing the opposite sign.
- Law: tenancy is organizations, realized org-scoped — one `oss.Organization` per `spec.tenants` slug with the per-tenant folder, source set, and board fleet threaded `orgId` from the realized org's own output, so a tenant's boards and sources scope to its org while the default org carries the operator fleet, alerts, and machine identity.
- Law: the deployment annotates itself — one `oss.Annotation` carries the deploy plane's time-ordered identity and stack coordinates as board-visible text, so every dashboard reads deploys against its own series; richer run evidence stays receipt material on the automation plane.
- Law: the machine identity is minted least-privilege — one `oss.ServiceAccount` (`role: "Editor"`) holds exactly the folder-Admin grant one `oss.FolderPermissionItem` lands, and one `oss.ServiceAccountRotatingToken` (rotation window as `_ROTATION` policy data, `deleteOnDestroy` so a torn-down stack leaves no live credential) realizes the durable automation credential; the token key egresses as the tier's `automation` output for the composing arm to land in a Doppler `{ value }` entry, and the chart-seeded `admin:password` binding remains the one in-graph provider auth.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect; auth never rides env here — the in-graph read is the canonical binding for deploy-time application.
- Entry: `new Boards("boards", { spec, urls, targets, auth, boards, packs, library, alerts, objectives, contacts, deploy, rolls }, opts)` — the k8s arm feeds `lgtm.{urls,targets}`, the docker arm `dev.{urls,targets}`; `boards`/`alerts`/`objectives` produced by the app's core observe suite call against those same targets, `packs` by the producer censuses, `rolls` by the AppHost fleet ledger.

```typescript signature
import type { Builder, Dataquery } from "@grafana/grafana-foundation-sdk/cog"
import {
  AxisPlacement, ColorDimensionConfigBuilder, type DataSourceRef, FrameGeometrySourceBuilder, FrameGeometrySourceMode, LogsDedupStrategy, LogsSortOrder,
  MapLayerOptionsBuilder, ScaleDimensionConfigBuilder, ScaleDimensionMode, ScaleDistribution, ScaleDistributionConfigBuilder, TableFooterOptionsBuilder,
  TableSortByFieldStateBuilder, TextDimensionConfigBuilder, TextDimensionMode, TooltipDisplayMode, VizTooltipOptionsBuilder,
} from "@grafana/grafana-foundation-sdk/common"
import { AnnotationQueryBuilder, DashboardBuilder, DashboardLinkBuilder, DashboardLinkType, ThresholdsConfigBuilder, ThresholdsMode } from "@grafana/grafana-foundation-sdk/dashboard"
import { ControlsOptionsBuilder, MapViewConfigBuilder } from "@grafana/grafana-foundation-sdk/geomap"
import { ZoomMode } from "@grafana/grafana-foundation-sdk/nodegraph"
import { DataqueryBuilder as PyroscopeQuery } from "@grafana/grafana-foundation-sdk/grafanapyroscope"
import { DataqueryBuilder as LokiQuery } from "@grafana/grafana-foundation-sdk/loki"
import { DataqueryBuilder, PromQueryFormat } from "@grafana/grafana-foundation-sdk/prometheus"
import { PanelBuilder as Gauge } from "@grafana/grafana-foundation-sdk/gauge"
import { PanelBuilder as Geomap } from "@grafana/grafana-foundation-sdk/geomap"
import { HeatmapColorMode, HeatmapColorOptionsBuilder, HeatmapColorScale, PanelBuilder as Heatmap } from "@grafana/grafana-foundation-sdk/heatmap"
import { PanelBuilder as Logs } from "@grafana/grafana-foundation-sdk/logs"
import { PanelBuilder as Nodes } from "@grafana/grafana-foundation-sdk/nodegraph"
import { PanelBuilder as Stat } from "@grafana/grafana-foundation-sdk/stat"
import { PanelBuilder as Table } from "@grafana/grafana-foundation-sdk/table"
import { PanelBuilder as Timeseries } from "@grafana/grafana-foundation-sdk/timeseries"
import * as grafana from "@pulumiverse/grafana"
import { Board, Convention, Reliability } from "@rasm/ts/core"
import { Array, Duration, Match, Option, Order, Record, Schema, Struct } from "effect"

declare namespace Boards {
  type Model = typeof Board.DashboardModel.Encoded
  type Wire = (typeof _PACKS)[number]
  type Pack = {
    readonly wire: Wire // the producer census this pack carries; the schema stays the producer's mint
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Reliability.Alert.Spec>
  }
  type Library = { readonly name: string; readonly panel: typeof Board.DashboardModel.Panel.Encoded }
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
    // Rendered against the SAME value the incoming boards were rendered against, and carrying every answer the three
    // backend planes owe a board — the metrics door's driver, scope header, exemplar posture, and rule posture, the
    // realized residence with its whole board driver, and whether the profiles backend runs — because the producer
    // tier projected each off the rows IT installed; re-deriving any of them here from the spec splits the two the
    // moment a caller renders its packs against anything else, and answers for the k8s install on the docker arm
    // outright, where the container runs no residence and runs its profile backend whatever the spec says.
    readonly targets: Lgtm.Targets
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly packs?: ReadonlyArray<Pack>
    readonly library?: ReadonlyArray<Library>
    readonly alerts: ReadonlyArray<Reliability.Alert.Spec>
    readonly objectives: ReadonlyArray<Reliability.Objective>
    readonly contacts: Contacts
    readonly deploy: { readonly id: pulumi.Input<string> }
    readonly rolls?: ReadonlyArray<Roll>
  }
}

const _ROTATION = { live: Duration.toSeconds(Duration.days(7)), early: Duration.toSeconds(Duration.days(1)) } as const

// Delivery grouping is owned vocabulary, never free strings: the service and tenant dimensions ride the breached
// series, the objective and severity rows ride the rule's own labels, and an instance carrying none of a key
// groups under its absence rather than escaping the fold.
const _GROUPED = [Convention.attr.serviceName, Convention.rasm.tenant, Convention.rasm.sloObjective, Convention.rasm.sloSeverity] as const

// Closed tuples admit exactly the wires a branch mints: an entry whose producer exists nowhere types an arm promising
// an ingest arm for a pack that can never arrive, so a key enters here only once its producing branch lands the pack
// projection, and a wish for one lives as a card at that branch instead.
const _PACKS = [
  "apphost.instrument",
  "compute.receipt",
  "fabrication.slo",
  "grasshopper.fan",
  "materials.catalogue",
  "persistence.census",
  "runtime.pulse",
  "security.audit",
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

// Panel tags name a wire form once in this neutral vocabulary. Every RENDERABLE dialect answers every row here in
// its own alphabet or declares the absence, so a form is named at one altitude and spelled at another, and a tag
// naming a form its bound dialect cannot spell refuses at the target seam over rendering a silently empty tile.
const _FORMS = ["heatmap", "logs", "table", "timeseries"] as const

// Display ceilings are tier posture: each bounds what ONE panel renders regardless of which board mounts it, so both
// live at this anchor rather than as knobs an upstream author answers per board or as optional columns one tag row
// each carries.
const _CEILING = { lines: 1000, paginate: true } as const

// Every compile-seam refusal is one reason row against one coordinate. A deploy program signals fatal configuration
// where the coordinate is still loggable, so this is the tier's one refusal owner and no arm mints a second message
// shape; a panel bound to an unprovisionable target never reaches the provider.
const _REFUSED = {
  dialect: "untyped-panel-datasource",
  form: "unspellable-panel-form",
  residence: "unrealized-residence-driver",
  source: "unrealized-datasource",
} as const

const _refuse = (reason: keyof typeof _REFUSED, coordinate: string): never => {
  throw new Error(`${_REFUSED[reason]}:${coordinate}`)
}

// Each renderable dialect's own form alphabet, declared once so the row table, the build seam, and the correlated
// dispatch between them read one correspondence: the residence takes the sqlds `FormatQueryOption` ordinal, LogQL
// carries no wire-form field and selects its mode instead, and PromQL takes the SDK's own enum.
type _Alphabet = { readonly clickhouse: number; readonly loki: "instant" | "range"; readonly prometheus: PromQueryFormat }

type _Dialect = keyof _Alphabet

// What one target build reads before its dialect spells the form. A new emission coordinate is a field here rather
// than a parameter every dialect row then re-lists.
type _Rendered = {
  readonly exemplars: boolean
  readonly expr: string
  readonly legend: Option.Option<string>
  readonly refId: string
}

// Dialect builders receive this record: the rendered coordinates beside the form already spelled in that dialect's
// own alphabet, so a builder never re-reads the neutral vocabulary that selected it.
type _Aim<Spelling> = _Rendered & { readonly form: Spelling }

// What a datasource row dials: the URL plane either producer publishes, the one generated credential the residence
// and the board plane share, the metrics plane's OWN projection off whichever tier installed it, and the app key an
// org-scoped read stamps — so a row needing more than an address reads it here rather than from a second args field
// every arm then threads. `metrics` arrives rather than re-deriving off `_stores` from the spec, because that read
// answers for the k8s install on an arm running a container whose store this tier never configured.
type _Dial = {
  readonly app: string
  readonly auth: pulumi.Input<string>
  readonly metrics: Lgtm.Metrics
  // Realized backend rows a source row reads to answer `present`, projected by the tier that INSTALLED them. A source
  // row cannot decide its own realization from a URL, because every unrealized plane publishes an EMPTY address and an
  // empty address provisions a datasource that authenticates against nothing; and it cannot decide it from the spec
  // either, because `analytics: "clickhouse"` on the docker arm names a plane that container never runs and
  // `profiles: false` on that same arm names one its image is serving regardless. Both arrive answered.
  readonly analytics: Lgtm.Targets["analytics"]
  readonly profiles: boolean
  readonly urls: Lgtm.Urls
}

// ONE resolution of the realized board driver: identity, language, and every coordinate it dials come off the same
// answer, so the four residence columns below cannot resolve against different rows. `_drives` is the arm `present`
// already gated — the refusal exists because the type demands a value, not as a second admission gate.
const _driver = (bind: _Dial) => Option.flatMap(bind.analytics, (row) => row.source)

const _drives = (bind: _Dial) => Option.getOrElse(_driver(bind), () => _refuse("residence", "unrealized"))

// BOUNDARY ADAPTER: residence drivers take a host where this door publishes an address, so its own authority is
// read back once. An unrealized residence publishes an empty door and yields an empty host, which is exactly the
// docker arm's declared analytics absence rather than a parse fault.
const _host = (door: pulumi.Output<string>): pulumi.Output<string> =>
  door.apply((address) => address === "" ? "" : new URL(address).hostname)

// Every source row answers this one settings body in THREE columns: its plain half encodes into `jsonDataEncoded`,
// its sealed half into `secureJsonDataEncoded`, and headers ride the resource's own typed map. That third column is
// no convenience — Grafana stores a custom header as an indexed `httpHeaderName<N>`/`httpHeaderValue<N>` pair
// split across those two documents, and the provider RESERVES both spellings and refuses either encoded body that
// contains one, so hand-folding Grafana's storage shape fails validation before a request is ever made and the typed
// map is the one seat. Naming the shape once keeps a row dialing its own coordinates and a row dialing the
// provisioned address alone differing in values and never in shape.
type _Settings = {
  readonly headers: Record.ReadonlyRecord<string, pulumi.Input<string>>
  readonly json: Record.ReadonlyRecord<string, unknown>
  readonly secure: Record.ReadonlyRecord<string, unknown>
}

// Rows whose driver dials the provisioned url alone still ANSWER every settings column: an empty body encodes as an
// empty document and an empty map lands no header, so a coordinate the family gains is a value edit at one row and
// never a silently dropped key.
const _PLAIN = (): _Settings => ({ headers: {}, json: {}, secure: {} })

// Every key names the PLANE its door answers, never the engine behind it: `metrics` is whichever store row the tier
// that installed it projected and `residence` whichever armed `_RESIDENCE` row carries a driver, so an escalation
// re-points one uid and every compiled board keeps its binding. `dialect` names the dataquery language a panel bound
// to the row emits, and its presence is what the core algebra RENDERS: PromQL and SQL have render targets, while
// TraceQL and the profile selector do not, so those rows answer `Option.none()` and stay Explore doors even where the
// pinned SDK types their builders — and it is a row FUNCTION like every sibling column, because whichever language a
// residence speaks is that armed row's own answer, and a literal here binds every future residence to one grammar.
// `present` is the row's own realization answer off the armed rows — a plane the spec never armed publishes an empty
// address, and provisioning a driver against one leaves a board tile failing to authenticate where the spec declared
// an absence. `settings` and `type` are columns every row answers, so a row dialing its own coordinates and a row
// dialing the provisioned address alone differ in values and never in shape.
const _SOURCES = {
  loki: { dialect: () => Option.some("loki" as const), present: () => true, type: () => "loki", url: (bind) => bind.urls.query.loki, settings: _PLAIN },
  metrics: {
    // Store rows mint their target through `Board.Query.promql`, so this language follows that projection rather than
    // whichever engine sits behind the door — a row shipping its own plugin renders the dialect its family renders
    dialect: () => Option.some("prometheus" as const),
    present: () => true, // every stack runs a metrics store: the family has no `none` arm and alerting depends on it
    // PROJECTED row supplies both plugin and scope header: this door IS the metrics plane, so an escalation isolating
    // by org reads through the same header its ingest already stamps and a row shipping its own plugin provisions
    // that plugin rather than the reference row's
    type: (bind) => bind.metrics.plugin,
    url: (bind) => bind.urls.query.metrics,
    // Scope IS a header, so it rides the resource's own header column rather than a fold into those two encoded
    // documents, which refuse the indexed spelling outright; a `label`-tenancy row hands an empty record here and
    // lands no pair at all, which is the same answer every row dialing the provisioned address alone gives.
    settings: (bind) => ({ ..._PLAIN(), headers: bind.metrics.scope }),
  },
  pyroscope: {
    dialect: () => Option.none(),
    present: (bind) => bind.profiles, // the profiles row is spec data, and its chart is absent where the value is
    type: () => "grafana-pyroscope-datasource",
    url: (bind) => bind.urls.query.pyroscope,
    settings: _PLAIN,
  },
  residence: {
    // Realized rows name their own language, their own driver, and every coordinate that driver dials, so a
    // residence family gaining an engine whose Grafana plugin speaks another dialect against another port renders and
    // connects with no edit at this row; reading any of it off `_COLUMNAR` here binds every future residence to the
    // interactive plane's grammar, catalog, and listener — the engine-name-as-family-key defect `type` already deleted.
    dialect: (bind) => Option.map(_driver(bind), (driver) => driver.dialect),
    present: (bind) => Option.isSome(_driver(bind)),
    type: (bind) => _drives(bind).type,
    url: (bind) => bind.urls.query.residence,
    settings: (bind) => _drives(bind).settings({ auth: bind.auth, door: bind.urls.query.residence }),
  },
  tempo: { dialect: () => Option.none(), present: () => true, type: () => "tempo", url: (bind) => bind.urls.query.tempo, settings: _PLAIN },
} as const satisfies Record<string, {
  readonly dialect: (bind: _Dial) => Option.Option<_Dialect>
  readonly present: (bind: _Dial) => boolean
  readonly settings: (bind: _Dial) => _Settings
  readonly type: (bind: _Dial) => pulumi.Input<string>
  readonly url: (bind: _Dial) => pulumi.Output<string>
}>

// Realized rows answer in declaration order. Every downstream fold — provisioning, panel admission, the tier's own
// target projection — reads this ONE resolution, so a plane the spec disarmed is absent from the board estate rather
// than provisioned against an empty address, and a tile bound to it refuses at the compile seam.
const _realized = (bind: _Dial): ReadonlyArray<keyof typeof _SOURCES> =>
  Array.filter(Struct.keys(_SOURCES), (key) => _SOURCES[key].present(bind))

// One `oss.DataSource` construction both the default org and every tenant org compose: the row answers its whole
// settings body, the key pins the `uid` one compiled board binds in every org, and an empty body encodes as an empty
// document rather than as an absent argument, so a row gaining a coordinate never changes this fold.
const _provisioned = (
  key: keyof typeof _SOURCES,
  bind: _Dial,
  org: { readonly id: pulumi.Input<string>; readonly slug: string } | undefined,
  child: pulumi.CustomResourceOptions,
): grafana.oss.DataSource => {
  const row = _SOURCES[key]
  const dialed = row.settings(bind)
  return new grafana.oss.DataSource(org === undefined ? key : `${org.slug}-${key}`, {
    type: row.type(bind),
    uid: key, // the row key IS the uid in every org, which is what lets one compiled board JSON bind everywhere
    url: row.url(bind),
    httpHeaders: dialed.headers, // the resource's own header seat; both encoded bodies below refuse the indexed pair by name
    jsonDataEncoded: pulumi.jsonStringify(dialed.json),
    secureJsonDataEncoded: pulumi.jsonStringify(dialed.secure),
    ...(org !== undefined && { orgId: org.id }),
  }, child)
}

// Residence queries transcribe the plugin's own `CHSqlQuery` field-for-field — SQL editor discriminant, release
// stamp, rendered relation, sqlds format ordinal — over the base every Grafana dataquery carries. `queryType`, the
// builder-mode payload, and the macro-expansion flag stay off this record because each decides something only the
// plugin's own editor decides, and a compiled board opens no editor. `format` carries the plugin's own optionality:
// one dialect row spells the ordinal at every build, so a draft that never reached it stays unset, and the plugin
// reads that absence as the timeseries frame rather than as a coordinate the seed had to invent.
type _ChSql = {
  datasource?: DataSourceRef
  editorType: typeof _COLUMNAR.plugin.editor
  format?: number
  hide?: boolean
  pluginVersion: string
  rawSql: string
  refId: string
  _implementsDataqueryVariant(): void
}

const _defaultChSql = (): _ChSql => ({
  editorType: _COLUMNAR.plugin.editor,
  pluginVersion: _COLUMNAR.plugin.version,
  rawSql: "",
  refId: "",
  _implementsDataqueryVariant: () => {}, // the marker the target contract reads; serialization drops it, as it does for every generated builder
})

// Transcribed from the generated dataquery builders — a `protected` draft seeded by a default factory, fluent setters
// returning `this`, one `build()` emission — because `cog` publishes `Builder<T>` and `isBuilder` and no base class,
// so that shape IS the SDK's builder machinery. Extending `datasource.DataqueryBuilder` was the alternative and is
// refused on evidence: its `defaultDataquery()` seeds `panelId` and `withTransforms`, two fields the plugin's config
// never declares, so every residence query would ship envelope keys its own datasource discards.
class _ResidenceQuery implements Builder<Dataquery> {
  protected readonly internal: _ChSql = _defaultChSql()
  build(): _ChSql {
    return this.internal
  }
  datasource(datasource: DataSourceRef): this {
    this.internal.datasource = datasource
    return this
  }
  format(format: number): this {
    this.internal.format = format
    return this
  }
  hide(hide: boolean): this {
    this.internal.hide = hide
    return this
  }
  // core renders the whole relation, absolute bucket boundaries included, so the plugin's `$__` macro vocabulary
  // never fires and the residence receives the identical text `Board.Query.render` produced for every other reader
  rawSql(rawSql: string): this {
    this.internal.rawSql = rawSql
    return this
  }
  refId(refId: string): this {
    this.internal.refId = refId
    return this
  }
}

const _legended = <B extends { legendFormat(format: string): B }>(built: B, legend: Option.Option<string>): B =>
  Option.match(legend, { onNone: () => built, onSome: (format) => built.legendFormat(format) })

// Per-form emission posture as a mapped row over the SDK's own format union, so the dispatch grows a row and never an
// arm: exemplars ride range series alone because a table or heatmap frame carries no sample stream to link, and the
// instant flag rides the table form because that is the frame a point-in-time facet reads.
const _PROM_MODE: { readonly [F in PromQueryFormat]: (built: DataqueryBuilder, exemplars: boolean) => DataqueryBuilder } = {
  [PromQueryFormat.Heatmap]: (built) => built,
  [PromQueryFormat.Table]: (built) => built.instant(),
  [PromQueryFormat.TimeSeries]: (built, exemplars) => built.exemplar(exemplars),
}

// One row per RENDERABLE dialect: its own form alphabet beside the builder consuming it, mapped so a row missing
// either column fails at this record and the indexed dispatch below stays correlated and cast-free. A new query
// language is one row here plus one `_SOURCES` row naming it — the dispatch that reads them never grows an arm, and
// switch arms restating this correspondence are what the table deletes.
const _DIALECTS: {
  readonly [K in _Dialect]: {
    readonly forms: Record.ReadonlyRecord<(typeof _FORMS)[number], Option.Option<_Alphabet[K]>>
    readonly target: (aim: _Aim<_Alphabet[K]>) => Builder<Dataquery>
  }
} = {
  // sqlds `FormatQueryOption` ordinals: the timeseries row pivots core's three-column long relation into the wide
  // frame a series panel reads and the table row leaves it long, so the heatmap facet takes the SAME pivot — a bucket
  // frame is time beside one column per bound, which the long relation is not.
  clickhouse: {
    forms: { heatmap: Option.some(0), logs: Option.some(2), table: Option.some(1), timeseries: Option.some(0) },
    // core renders the whole relation, absolute bucket boundaries included, so the plugin's `$__` macro vocabulary
    // never fires and the residence receives the identical text every other reader took; `legendFormat` has no
    // spelling here because series identity rides the relation's own `by` column rather than a selector template
    target: (aim) => new _ResidenceQuery().rawSql(aim.expr).refId(aim.refId).format(aim.form),
  },
  // LogQL carries no wire-form field at all — the builder exposes the range/instant mode pair instead — so a log
  // frame and a range series ride `range` while a point-in-time facet frame rides `instant`, and the heatmap form
  // declares its absence because this dialect spells no bucket frame a heatmap panel reads.
  loki: {
    forms: { heatmap: Option.none(), logs: Option.some("range"), table: Option.some("instant"), timeseries: Option.some("range") },
    target: (aim) => {
      const built = new LokiQuery().expr(aim.expr).refId(aim.refId).maxLines(_CEILING.lines) // the log ceiling is tier posture, never a per-board knob
      return _legended(aim.form === "instant" ? built.instant(true) : built.range(true), aim.legend)
    },
  },
  prometheus: {
    forms: {
      heatmap: Option.some(PromQueryFormat.Heatmap),
      logs: Option.none(), // no log frame exists on a series plane: a Logs tile bound here refuses instead of emitting a range query
      table: Option.some(PromQueryFormat.Table),
      timeseries: Option.some(PromQueryFormat.TimeSeries),
    },
    target: (aim) => _legended(_PROM_MODE[aim.form](new DataqueryBuilder().expr(aim.expr).refId(aim.refId).format(aim.form), aim.exemplars), aim.legend),
  },
}

// One profile selector carries the tenant dimension every board filters on, aims at the CPU profile type, and renders
// through the typed query builder rather than a hand-spelled selector string a datasource then reinterprets.
const _PROFILE_TYPE = "process_cpu:cpu:nanoseconds:cpu:nanoseconds"

const _profiled = (app: string) =>
  new PyroscopeQuery()
    .profileTypeId(_PROFILE_TYPE)
    .labelSelector(`{${Convention.attr.serviceName}="${app}", ${Convention.rasm.tenant}="$tenant"}`)
    .groupBy([Convention.rasm.tenant])
    .queryType("both") // series answer the board's time axis, the flamegraph answers the explorer this links into
    .refId("P")

const _byAt: Order.Order<{ readonly at: number }> = Order.mapInput(Order.number, (step) => step.at)

const _DEDUP = {
  exact: LogsDedupStrategy.Exact, none: LogsDedupStrategy.None, numbers: LogsDedupStrategy.Numbers, signature: LogsDedupStrategy.Signature,
} as const

// Which wire form each panel tag reads, in the neutral vocabulary every dialect answers — so one row serves a store
// tile and an evidence tile alike, and the point-in-time frame a facet panel needs is the form itself rather than a
// second flag every dialect then re-interprets. Which BACKEND a panel reads is the MODEL's own `source` and never
// this row: a tag-derived key binds every table to whatever backend that tag defaulted to, so an evidence tile and a
// series tile on one board would both land on the metrics store.
const _FORMED = {
  Gauge: "timeseries",
  Geomap: "table",
  Heatmap: "heatmap",
  Logs: "logs",
  Nodes: "table",
  Stat: "timeseries",
  Table: "table",
  Timeseries: "timeseries",
} as const satisfies Record<typeof Board.DashboardModel.Panel.Type["_tag"], (typeof _FORMS)[number]>

// Admission of the model's own datasource key against the rows this stack REALIZED, never the declared roster: a key
// outside the roster and a key naming a plane the spec disarmed are one defect, and both refuse at the compile seam
// where the coordinate is still loggable rather than as a panel bound to a datasource nothing provisioned. Matching
// against the realized array narrows the literal by search, so no assertion re-admits a key the roster never held.
const _sourced = (source: string, bind: _Dial): keyof typeof _SOURCES =>
  Option.getOrElse(Array.findFirst(_realized(bind), (key) => key === source), () => _refuse("source", source))

// Model axis vocabulary maps onto the builder's own enums, and both logarithmic rows differ only in the base
// they carry — so each case builds its own scale rather than a switch re-deciding the family per panel.
const _PLACED = { left: AxisPlacement.Left, right: AxisPlacement.Right, hidden: AxisPlacement.Hidden } as const
const _SCALED = {
  linear: () => new ScaleDistributionConfigBuilder().type(ScaleDistribution.Linear),
  log2: () => new ScaleDistributionConfigBuilder().type(ScaleDistribution.Log).log(2),
  log10: () => new ScaleDistributionConfigBuilder().type(ScaleDistribution.Log).log(10),
  symlog: () => new ScaleDistributionConfigBuilder().type(ScaleDistribution.Symlog),
} as const
const _TIP = { hidden: TooltipDisplayMode.None, multi: TooltipDisplayMode.Multi, single: TooltipDisplayMode.Single } as const

// The heatmap's two colour axes against the builder's OWN enums, TOTAL because the core owner now carries exactly
// the pinned SDK's value sets — the retired scheme-family and axis-scale literals were deleted at the model under
// core's SDK-floor law, so an unspellable colour coordinate is unconstructible and no refusal arm survives here.
const _HEAT_MODE = {
  opacity: HeatmapColorMode.Opacity,
  scheme: HeatmapColorMode.Scheme,
} as const satisfies Record<(typeof Board.DashboardModel.Heatmap.Type)["color"], HeatmapColorMode>

const _HEAT_SCALE = {
  exponential: HeatmapColorScale.Exponential,
  linear: HeatmapColorScale.Linear,
} as const satisfies Record<(typeof Board.DashboardModel.Heatmap.Type)["scale"], HeatmapColorScale>

// Axes and interaction are shared model fields whose builder members live on the Timeseries tag alone, so they land at
// that one arm; carrying them on every other tag would emit a field no builder there reads.
const _axed = (
  built: Timeseries,
  axes: ReadonlyArray<{
    readonly label: Option.Option<string>
    readonly max: Option.Option<number>
    readonly min: Option.Option<number>
    readonly placement: keyof typeof _PLACED
    readonly scale: keyof typeof _SCALED
    readonly unit: Option.Option<string>
  }>,
  tooltip: keyof typeof _TIP,
): Timeseries =>
  Array.reduce(axes, built, (row, axis) => {
    const placed = row.axisPlacement(_PLACED[axis.placement]).scaleDistribution(_SCALED[axis.scale]())
    const labelled = Option.match(axis.label, { onNone: () => placed, onSome: (label) => placed.axisLabel(label) })
    const floored = Option.match(axis.min, { onNone: () => labelled, onSome: (min) => labelled.min(min) })
    const ceiled = Option.match(axis.max, { onNone: () => floored, onSome: (max) => floored.max(max) })
    return _united(ceiled, axis.unit)
  }).tooltip(new VizTooltipOptionsBuilder().mode(_TIP[tooltip]).hideZeros(false))

// Geomap coordinate mapping IS the panel: without it every point lands at the frame's first two numeric columns
// by accident, so the model's own lat/lon column names drive one Coords-mode layer over the row's own basemap.
const _mapped = (
  mapping: {
    readonly color: Option.Option<string>
    readonly label: Option.Option<string>
    readonly latitude: string
    readonly longitude: string
    readonly weight: Option.Option<string>
  },
  zoom: boolean,
): Geomap =>
  new Geomap()
    .view(new MapViewConfigBuilder().allLayers(true)) // the view frames every layer rather than pinning a hardcoded centre no dataset shares
    // one model field drives both halves of this row's controls: a visible control the reader never scrolls onto
    // denies as much as a wheel zooming a map the reader meant to scroll past
    .controls(new ControlsOptionsBuilder().showZoom(zoom).mouseWheelZoom(zoom))
    .basemap(new MapLayerOptionsBuilder().type("default").name("basemap"))
    .layers([
      new MapLayerOptionsBuilder()
        .type("markers")
        .name("points")
        .tooltip(true)
        .location(new FrameGeometrySourceBuilder().mode(FrameGeometrySourceMode.Coords).latitude(mapping.latitude).longitude(mapping.longitude))
        // `MapLayerOptions.config` is the SDK's one untyped envelope (`config?: any`), yet every binding inside
        // it compiles through the typed `common` dimension builders — the `.field` spellings are checked and
        // only the `{ style }` wrapper is the markers layer's own persisted shape. `text.mode` is REQUIRED by
        // the schema for a field-driven label; `size.min`/`size.max` are required scale bounds (the panel's own
        // 2/15 defaults, spelled because the schema demands them, never a styling opinion).
        .config({
          showLegend: true,
          style: {
            ...Option.match(mapping.color, {
              onNone: () => ({}),
              onSome: (field) => ({ color: new ColorDimensionConfigBuilder().field(field).build() }),
            }),
            ...Option.match(mapping.label, {
              onNone: () => ({}),
              onSome: (field) => ({ text: new TextDimensionConfigBuilder().mode(TextDimensionMode.Field).field(field).build() }),
            }),
            ...Option.match(mapping.weight, {
              onNone: () => ({}),
              onSome: (field) => ({ size: new ScaleDimensionConfigBuilder().field(field).min(2).max(15).mode(ScaleDimensionMode.Linear).build() }),
            }),
          },
        }),
    ])

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

const _minted = (panel: typeof Board.DashboardModel.Panel.Type) =>
  // each tag's distinct payload lands at mint: a new panel tag fails compilation at this record until its arm exists
  Match.value(panel).pipe(Match.tagsExhaustive({
    Gauge: (row) => _stepped(new Gauge().max(row.ceiling), row.steps), // the gauge ceiling is the scale fact; the trip point rides its steps row
    Geomap: (row) => _mapped(row.mapping, row.zoom),
    // both colour axes land on the one options builder this panel reads them through: the model declared them and
    // the builder carries them, so neither survives as an emission fact the compile leg dropped on the floor
    Heatmap: (row) =>
      _united(
        new Heatmap().color(new HeatmapColorOptionsBuilder().mode(_HEAT_MODE[row.color]).scale(_HEAT_SCALE[row.scale])),
        row.unit,
      ),
    Logs: (row) =>
      new Logs()
        .showTime(row.showTime)
        .wrapLogMessage(row.wrap)
        .sortOrder(row.order === "ascending" ? LogsSortOrder.Ascending : LogsSortOrder.Descending)
        .dedupStrategy(_DEDUP[row.deduplicate]),
    // Organize below renames the mapping row onto the conventional frame columns this graph reads. Greedy zoom claims
    // every wheel event where cooperative demands a modifier, so the model's zoom field IS this row's mode.
    Nodes: (row) => new Nodes().zoomMode(row.zoom ? ZoomMode.Greedy : ZoomMode.Cooperative),
    Stat: (row) => _united(_stepped(new Stat(), row.steps), row.unit),
    // pagination is tier posture like the log ceiling; the sort row is the model's, so the footer and the sort land together
    Table: (row) =>
      Option.match(row.sort, {
        onNone: () => new Table(),
        onSome: (sort) => new Table().sortBy([new TableSortByFieldStateBuilder().displayName(sort.field).desc(sort.descending)]),
      }).footer(new TableFooterOptionsBuilder().enablePagination(_CEILING.paginate).show(false)),
    Timeseries: (row) => _axed(_united(_stepped(new Timeseries(), row.steps), row.unit), row.axes, row.tooltip),
  }))

// Three owners, one correlated indexed dispatch: the SOURCE row names its dialect, `_FORMED` names the tag's neutral
// wire form, and the dialect's own row spells that form in its own alphabet before building. The generic parameter is
// what keeps the alphabet following the dialect, so the arm-per-dialect switch this replaces cannot re-enter. Forms
// no dialect spells refuse here over emitting a range query for a tile whose tag asked for wide events, which is the
// silent empty panel the neutral vocabulary exists to surface.
const _target = <K extends _Dialect>(dialect: K, form: (typeof _FORMS)[number], aim: _Rendered): Builder<Dataquery> =>
  _DIALECTS[dialect].target({
    ...aim,
    form: Option.getOrElse(_DIALECTS[dialect].forms[form], () => _refuse("form", `${dialect}:${form}`)),
  })

// One dial answers every coordinate this compile reads — the app key, the metrics plane's own projection and thence
// its exemplar posture, the realized residence driver, the profiles answer, and the URL plane — so a panel's
// datasource type, its target, and the board's own profile link all resolve off the same value the provisioning fold
// took, and no second field re-states what the dial already carries.
const _compiled = (model: Boards.Model, bind: _Dial) => {
  // BOUNDARY ADAPTER: Foundation SDK builders mutate their own drafts; this function contains that imperative contract.
  const decoded = Schema.decodeSync(Board.DashboardModel)(model)
  const board = new DashboardBuilder(decoded.title)
    .uid(decoded.uid)
    .tags([...decoded.tags])
    .readonly() // content is code and the UI is drift: the hash-diff provider posture and this presentation row are one law
    .refresh(`${Duration.toSeconds(decoded.refresh)}s`)
    .time({ from: `now-${Duration.toSeconds(decoded.since)}s`, to: "now" }) // the model's since lands; the Grafana-default range never renders
  if (bind.profiles) {
    // this SDK types the query and Explore renders it: the board hands the typed selector across rather than a panel
    // that pin cannot build, and the link keeps the reader's window so the profile matches what they were reading
    board.link(
      new DashboardLinkBuilder("profiles")
        .type(DashboardLinkType.Link)
        .icon("bolt")
        .url(`/explore?left=${encodeURIComponent(JSON.stringify({ datasource: "pyroscope", queries: [_profiled(bind.app).build()] }))}`)
        .includeVars(true)
        .keepTime(true),
    )
  }
  board.variables([...decoded.variables])
  for (const note of decoded.annotations) board.annotation(new AnnotationQueryBuilder().name(note.slug).iconColor(note.tone).enable(true))
  for (const { panel, position } of Board.DashboardModel.laid(decoded)) {
    const key = _sourced(panel.source, bind) // the model names its own plane, so one board mixes a store tile and a residence tile
    const source = _SOURCES[key]
    const built = _minted(panel)
      .title(panel.title)
      .gridPos({ h: position.h, w: position.w, x: position.x, y: position.y })
      .transparent(panel.transparent)
      .datasource({ type: source.type(bind), uid: key }) // the uid pinned at the source row: one compiled JSON binds identically in every org
    if (Option.isSome(panel.description)) built.description(panel.description.value)
    if (Option.isSome(panel.repeat)) built.repeat(panel.repeat.value)
    // panel links open in place, carrying the board's variables and time range, so a drill-down lands on the same
    // tenant and window the operator was reading rather than the target's own defaults
    if (panel.links.length > 0) {
      built.links(Array.map(panel.links, (link) =>
        new DashboardLinkBuilder(link.title).url(link.url).type(DashboardLinkType.Link).includeVars(true).keepTime(true).targetBlank(false)))
    }
    for (const transform of panel.transformations) built.withTransformation({ id: _TRANSFORMS[transform._tag], options: transform }) // the tag selects the transformer id; the row is its own options payload
    if (panel._tag === "Nodes") {
      built.withTransformation({
        // Mapping renames onto the conventional frame columns the node graph reads; identity stays model data.
        // The discrimination stage is CASE-SENSITIVE (`FieldCache.getFieldByName`): lowercase `id`/`source` are
        // what admit and split the two frames, so every landing spelling below is the panel's own lowercase
        // convention. `nodeWeight` lands on `mainstat` — the panel's numeric stat column, unit-carried through
        // `NodeOptionsBuilder.mainStatUnit` — never `noderadius`, a pixel radius, not a magnitude.
        id: _TRANSFORMS.Organize,
        options: {
          order: [],
          rename: {
            [panel.mapping.nodeId]: "id",
            [panel.mapping.edgeId]: "id",
            [panel.mapping.edgeSource]: "source",
            [panel.mapping.edgeTarget]: "target",
            ...Option.match(panel.mapping.nodeLabel, { onNone: () => ({}), onSome: (field) => ({ [field]: "title" }) }),
            ...Option.match(panel.mapping.nodeColor, { onNone: () => ({}), onSome: (field) => ({ [field]: "color" }) }),
            ...Option.match(panel.mapping.nodeWeight, { onNone: () => ({}), onSome: (field) => ({ [field]: "mainstat" }) }),
          },
        },
      })
    }
    const legend = "legend" in panel ? panel.legend : Option.none<string>()
    const exprs = "exprs" in panel ? panel.exprs : "expr" in panel ? [panel.expr] : "filter" in panel ? [panel.filter] : [panel.nodes, panel.edges]
    // a source rendering no dialect refuses here: TraceQL and the profile selector reach an operator through the
    // Explore link this board already carries, never through a panel target the core algebra cannot render
    const dialect = Option.getOrElse(source.dialect(bind), () => _refuse("dialect", key))
    // Node-graph frame ADMISSION keys on `refId`/`name` `nodes`|`edges` (or an `id` field) — an `A0`/`A1` refId
    // pair fails all three tests for the edges frame, so the Nodes tag names its frames outright.
    exprs.forEach((expr, at) =>
      built.withTarget(_target(dialect, _FORMED[panel._tag], {
        exemplars: bind.metrics.exemplars,
        expr,
        legend,
        refId: panel._tag === "Nodes" ? (at === 0 ? "nodes" : "edges") : `A${at}`,
      })))
    board.withPanel(built)
  }
  return board.build()
}

const _window = (input: Duration.DurationInput): string => `${Duration.toSeconds(Duration.decode(input))}s`

const _burned = (spec: Reliability.Alert.Spec, breach: (window: Duration.DurationInput) => string): string =>
  Array.join(
    Array.map([spec.windows.short, spec.windows.long], (window) => `${breach(window)} > ${spec.factor * (1 - spec.target)}`),
    " and ",
  )

// Numerators resolve at exactly one altitude: a store that evaluates rules already holds `Board.Query.breach`'s result
// under `_recorded`, so the rule reads the recorded series and the store computes it once per group interval; a store
// with no evaluator renders the same core projection inline. Neither arm re-derives a threshold or a series spelling,
// and `Board.Query.breach` remains the one definition both arms trace back to.
const _expr = (spec: Reliability.Alert.Spec, recorded: boolean, target: Board.Query.Target): string =>
  _burned(spec, (window) =>
    recorded
      ? _recorded(spec, window)
      : Board.Query.render(Board.Query.breach(spec.sli, Board.Query.span(Duration.decode(window)), {}, spec.filters), target))

// Core owns the operator-read observable beside the breach expression, so this leg renders and
// re-derives nothing — a local dispatch here forks the level polarity, the partition selector, and the
// quantile spelling away from the burn panels reading the identical objective.
const _query = (objective: Reliability.Objective, target: Board.Query.Target): string =>
  Board.Query.render(Board.Query.indicator(objective.sli, undefined, {}, objective.filters), target)

const _alerted = (
  alerts: ReadonlyArray<Reliability.Alert.Spec>,
  bind: {
    readonly folder: pulumi.Output<string>
    readonly datasource: pulumi.Output<string>
    readonly contacts: Boards.Contacts
    readonly recorded: boolean // the metrics plane's own rule posture: the numerator is precomputed or it is not
    readonly target: Board.Query.Target
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
      // wording leads, the runbook follows, then the two sorted planes: labels carry the spec's owned identity, annotations the derived headline
      template: `{{ define "${severity}" }}${row.wording ?? ""}${row.runbook === undefined ? "" : ` runbook: ${row.runbook}`} {{ .CommonLabels.SortedPairs }} {{ .CommonAnnotations.SortedPairs }}{{ end }}`,
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
        groupBies: [..._GROUPED], // the root grouping identity every child route inherits unless it declares its own
        policies: Array.map([head, ...rest], ({ severity, point, quiet }) => ({
          contactPoint: point.name,
          matchers: [{ label: Convention.rasm.sloSeverity, match: "=", value: severity }], // the spec's own severity row, never a free-string label
          ...(quiet !== undefined && { muteTimings: [quiet.name] }),
        })),
      }, child),
  })
  Array.match(alerts, {
    onEmpty: () => undefined,
    onNonEmpty: (specs) =>
      new grafana.alerting.RuleGroup("burn", {
        folderUid: bind.folder,
        // the store's recording group and this evaluator read ONE cadence anchor, so the rule never samples a
        // recorded numerator on a schedule the group that writes it does not keep
        intervalSeconds: _COLLECTOR.rules.seconds,
        rules: Array.map(specs, (spec) => ({
          name: spec.slug,
          condition: "B",
          for: _window(spec.severity.hold),
          // Grafana splits the two planes by use: labels carry routing, grouping, and silencing identity while
          // annotations carry the read headline — so the spec's owned Convention rows land as labels
          annotations: { summary: `${spec.slug} burning ${spec.factor}x, spending ${spec.spend} of the objective window budget` },
          labels: spec.annotations,
          datas: [
            {
              refId: "A",
              datasourceUid: bind.datasource,
              // the query window IS the spec's own long window: a fixed hour is a literal beside a spec that already
              // names both horizons, and it reads short the moment an objective declares a longer burn window
              relativeTimeRange: { from: Duration.toSeconds(Duration.decode(spec.windows.long)), to: 0 },
              model: JSON.stringify({ refId: "A", expr: _expr(spec, bind.recorded, bind.target) }),
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
  objectives: ReadonlyArray<Reliability.Objective>,
  folder: pulumi.Output<string>,
  target: Board.Query.Target,
  child: pulumi.CustomResourceOptions,
): ReadonlyArray<grafana.slo.SLO> =>
  Array.map(objectives, (objective) =>
    new grafana.slo.SLO(objective.name, {
      name: objective.name,
      description: `<slo:${objective.name}>`,
      folderUid: folder,
      objectives: [{ value: objective.target, window: _window(objective.window) }],
      queries: [{ type: "freeform", freeform: { query: _query(objective, target) } }],
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
    // Producer tiers project every backend plane off the rows THEY installed — the metrics target, driver, scope
    // header, exemplar posture, and rule posture; the realized residence and its whole board driver; the profiles
    // answer — so nothing here re-reads the spec and a docker arm's bundled container answers for itself instead of
    // borrowing the k8s install's columns.
    const plane = args.targets.metrics
    // One read of what a row may dial and which rows this stack REALIZED: every org and every compile compose the
    // same value, so a plane the stack never planted is absent from provisioning and from panel admission alike.
    const dial: _Dial = {
      app: args.spec.app,
      auth: args.auth,
      metrics: plane,
      analytics: args.targets.analytics,
      profiles: args.targets.profiles,
      urls: args.urls,
    }
    const ingested = Array.flatMap(args.packs ?? [], (pack) =>
      Array.map(pack.boards, (model) => ({ ...model, tags: [...model.tags, pack.wire] })))
    const compiled = Array.map([...args.boards, ...ingested], (model) => ({
      uid: model.uid,
      json: pulumi.jsonStringify(_compiled(model, dial)), // one compile serves the default org and every tenant org
    }))
    const alerts = [...args.alerts, ...Array.flatMap(args.packs ?? [], (pack) => pack.alerts)] // pack burn rows join the one rule fold
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    // Metrics binds by name because the rule fold needs its uid and the store family has no `none` arm; every other
    // realized row folds beside it, and a row the spec disarmed constructs nothing at all.
    const metrics = _provisioned("metrics", dial, undefined, child)
    Array.map(Array.filter(_realized(dial), (key) => key !== "metrics"), (key) => _provisioned(key, dial, undefined, child))
    Array.map(compiled, (board) =>
      new grafana.oss.Dashboard(board.uid, { configJson: board.json, folder: folder.uid }, child))
    const shelf = Array.map(args.library ?? [], (row) => ({
      row,
      json: pulumi.jsonStringify(_minted(Schema.decodeSync(Board.DashboardModel.Panel)(row.panel)).title(row.name).build()),
    }))
    Array.map(shelf, ({ row, json }) =>
      new grafana.oss.LibraryPanel(row.name, { name: row.name, folderUid: folder.uid, modelJson: json }, child)) // compiled once, served to every board that references it
    _alerted(alerts, { folder: folder.uid, datasource: metrics.uid, contacts: args.contacts, recorded: plane.recorded, target: plane.target }, child)
    _slos(args.objectives, folder.uid, plane.target, child)
    const operator = new grafana.oss.ServiceAccount(`${name}-automation`, { name: `${name}-automation`, role: "Editor" }, child)
    this.viewers = Record.fromEntries(Array.map(args.spec.tenants, (tenant) => {
      const org = new grafana.oss.Organization(tenant, { name: tenant }, child)
      const scope = org.orgId.apply(String) // org-scoped args take Input<string>; the realized org's number renders exactly here
      const home = new grafana.oss.Folder(`${tenant}-${name}`, { title: args.spec.app, uid: `${tenant}-${args.spec.app}`, orgId: scope }, child)
      Array.map(_realized(dial), (key) => _provisioned(key, dial, { id: scope, slug: tenant }, child)) // every org realizes the same rows, so one board JSON binds in all of them
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

(none)

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->
