# [TS_IAC_API_MIMIR_DISTRIBUTED]

`mimir-distributed` is the metrics store's fleet escalation: horizontal ingest and query past the single-store ceiling, org-isolated end to end. Its whole server configuration rides one `mimir.structuredConfig` tree, and its reverse proxy renders as `<fullname>-gateway` — `nginx` is the retired component word that survives only in the values COMMENTS, so an address spelled from it resolves to nothing.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                         | [CAPABILITY]                                                                         |
| :-----: | :-------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `mimir.structuredConfig`                      | nested map — the WHOLE server document — every section below is a key inside it      |
|  [02]   | `…common.storage`                             | one storage truth every per-section store inherits                                   |
|  [03]   | `…*_storage` sections                         | per-section stores that may diverge from common                                      |
|  [04]   | `…limits.compactor_blocks_retention_period`   | duration — the retention coordinate for this row                                     |
|  [05]   | `…limits.otel_translation_strategy`           | enum — must agree with `name_validation_scheme` and the suffix toggle                |
|  [06]   | `…limits` scheme + suffix                     | the two halves the distributor panics without                                        |
|  [07]   | `…limits.native_histograms_ingestion_enabled` | `boolean` — arms native histogram storage; off lands buckets instead                 |
|  [08]   | `…limits.max_native_histogram_buckets`        | `int` — over-bucket samples scale down rather than drop                              |
|  [09]   | `…frontend.query_result_response_format`      | `protobuf` \| `json` — native histograms survive query sharding under protobuf ALONE |
|  [10]   | `ruler.enabled`                               | `boolean` — in-store rule evaluation; burn rules escalate off the board              |
|  [11]   | `ruler.{extraVolumes,extraVolumeMounts}`      | the local rule mount; `local` reads `<directory>/<tenant>/`                          |
|  [12]   | `gateway.enabled`                             | `boolean` DEFAULT TRUE — the reverse proxy every read and write address goes through |
|  [13]   | `minio.enabled`                               | `boolean` DEFAULT TRUE — a bundled object store beside the estate's own              |
|  [14]   | `nameOverride` `fullnameOverride`             | `string` \| `null` — FLAT top-level                                                  |

[COMPONENTS]: `distributor` `ingester` `querier` `queryFrontend` `queryScheduler` `store_gateway` `compactor` `alertmanager` `overrides_exporter` `rollout_operator` `kafka`
[GATEWAY_NAME]: the reverse proxy is the `gateway` component and renders as `<fullname>-gateway`, a ClusterIP Service on port 80 (`http-metrics`) with a legacy 8080 alias. `nginx` appears in `values.yaml` only inside scope COMMENTS enumerating component names; no Service carries it. Verified by render: a pinned install yields `<pinned>-gateway` and no `<pinned>-nginx`.
[SERVICE_ROSTER]: a pinned install also renders `<fullname>-distributor` and its headless twin, `<fullname>-ingester-zone-{a,b,c}` with a headless ingester Service, `<fullname>-querier`, `<fullname>-query-frontend`, `<fullname>-query-scheduler`, `<fullname>-compactor`, `<fullname>-alertmanager` and its headless twin, `<fullname>-overrides-exporter`, and `<fullname>-gossip-ring` — beside the release-named `<release>-minio` and `<release>-rollout-operator`, which the pin does NOT reach.
[TRANSLATION_TRIPLE]: the distributor panics when the three disagree — an unescaped `otel_translation_strategy` demands `name_validation_scheme: utf8`, and its suffix half demands `otel_metric_suffixes_enabled: true`. The three are stated together or the ingest path crashes rather than degrading.
[RULE_STORAGE_SPLIT]: `ruler_storage` deliberately diverges from common storage — a `local` backend loads code-owned groups read-only, where the s3 backend would carry an API-mutable rule set the content-is-code law forbids.

[FULLNAME]: the standard collapse scaffold with flat overrides; the pin reaches every Mimir component and neither bundled subchart.
[SERVICE_NAME]: every read and write address is `<fullname>-gateway` on port 80 — the write path at `/otlp` and the read path at `/prometheus`.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- This row is EARNED, never default: the reference single-store row answers until the ingest or query ceiling is real, and this one costs a multi-component memory footprint stated on its own `degrade` column.
- Tenancy here is `org` rather than `label`, so the whole estate's isolation posture flips with the selection — the collector stamps a scope header on every exporter whose backend reads it, and Loki, Tempo, and Pyroscope arm their own tenancy alongside.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders every component as a stack child under Pulumi diff; the rule ConfigMap is a `core.v1.ConfigMap` the ruler mounts through `extraVolumes`.
- `operate/observe#STORE_ROWS`: `_stores.mimir` supplies chart and repo, projects both addresses off the gateway, and states the storage binding, the ruler mount, the translation triple, the histogram pair, and the disabled bundled object store.
- `minio`(`.api/minio.md`) / `rook-ceph-cluster`(`.api/rook-ceph-cluster.md`): the estate's object plane supplies the endpoint and bucket that bind `common.storage`, which is why the chart's own bundled store is disarmed.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/metrics` exporter dials the `/otlp` write path with the scope header this row's tenancy demands.
- `prometheus`(`.api/prometheus.md`): the reference row this one escalates from — same `plugin` column, same board queries, different ceiling and different tenancy grain.

[LOCAL_ADMISSION]:
- Address the reverse proxy as `<fullname>-gateway`; `nginx` names no rendered object on this pin.
- Bind `common.storage` to the estate's object plane and disarm the bundled MinIO — one object store, not two.
- State the translation triple together; two of three is a distributor panic.
- Pair `native_histograms_ingestion_enabled` with `frontend.query_result_response_format: protobuf`, because native histograms do not survive query sharding under JSON.
- Keep `ruler_storage` on the `local` backend with a ConfigMap-mounted group set under `<directory>/<tenant>/`; an s3 rule store is API-mutable and forks the content-is-code law.
- Read retention off `limits.compactor_blocks_retention_period`; there is no server-level retention key on this row.
