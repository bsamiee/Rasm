# [TS_IAC_API_LOKI]

`loki` is the log backend. Its chart is a topology SELECTOR over four deployment modes plus a values tree that stands up five auxiliary workloads by default, and its own validator refuses to render when two topologies both carry replicas. It also ships an EMPTY schema block its server will not start without, so a minimal values body is not a working install — it is a crash loop.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                            | [CAPABILITY]                                                                         |
| :-----: | :----------------------------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `deploymentMode`                                 | selects the topology; DEFAULT `SimpleScalable`                                       |
|  [02]   | `singleBinary.replicas`                          | `int` — the single-binary target's replica count                                     |
|  [03]   | `{write,read,backend}.replicas`                  | `int` — the simple-scalable targets — NON-ZERO by default                            |
|  [04]   | `loki.auth_enabled`                              | `boolean` — multi-tenancy; the store row's tenancy column governs it                 |
|  [05]   | `loki.commonConfig.replication_factor`           | `int` — quorum across ingesters                                                      |
|  [06]   | `loki.storage`                                   | `{ type, bucketNames, s3, filesystem, … }` — the chunk and index destination         |
|  [07]   | `loki.schemaConfig.configs[]`                    | SHIPS EMPTY; the server refuses to start without it                                  |
|  [08]   | `loki.limits_config`                             | retention and the OTLP attribute posture                                             |
|  [09]   | `loki.compactor`                                 | the leg that MAKES retention delete anything                                         |
|  [10]   | `gateway.enabled`                                | `boolean` DEFAULT TRUE — an nginx reverse proxy in front of the read and write doors |
|  [11]   | `lokiCanary.enabled` `test.enabled`              | `boolean` DEFAULT TRUE — a synthetic-log DaemonSet and a test pod                    |
|  [12]   | `chunksCache.enabled` `resultsCache.enabled`     | `boolean` DEFAULT TRUE — two memcached tiers                                         |
|  [13]   | `minio.enabled` `rollout_operator.enabled`       | `boolean` DEFAULT FALSE — the bundled object store and the rollout controller        |
|  [14]   | `{name,fullname,namespace,clusterLabel}Override` | nullable, FLAT top-level                                                             |

[DISTRIBUTED_COMPONENTS]: `ingester` `distributor` `querier` `queryFrontend` `queryScheduler` `indexGateway` `compactor` `ruler` `bloomGateway` `bloomPlanner` `bloomBuilder` `patternIngester` `overridesExporter`
[TOPOLOGY_VALIDATOR]: `deploymentMode` does NOT disarm the other targets. The chart's `validate.yaml` refuses to render when the single-binary and simple-scalable targets both carry replicas — and `write`, `read`, and `backend` default to non-zero — so a `SingleBinary` selection must zero all three. Verified: the render fails outright with the transitional-mode message rather than degrading.
[SCHEMA_CONFIG]: `loki.schemaConfig.configs` ships as an empty list and the server exits on an empty schema. One entry is mandatory, and its `index.period` is the schema's own fixed grain — a chart contract rather than the estate's retention coordinate, which lives at `limits_config.retention_period`.
[RETENTION_PAIR]: `limits_config.retention_period` alone deletes nothing. The compactor's `retention_enabled` plus a `delete_request_store` is what performs deletion, so retention is stated as a pair or it is a setting with no effect.

[FULLNAME]: the standard collapse scaffold with flat overrides; the pin renders the workload, its Service, and the memberlist Service under the pinned name. The chart's headless Service is named `loki-headless` from the CHART name and ignores the pin — nothing in this estate addresses it.
[SERVICE_NAME]: with the pin, the single-binary Service is `<fullname>` serving `http-metrics` on 3100 and `grpc` on 9095, beside `<fullname>-memberlist` for the gossip ring. The log door is therefore `<fullname>:3100`, dialed directly — the nginx gateway fronts exactly that door and is deleted.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- No row installs on chart defaults. FIVE auxiliary workloads ship on — a reverse proxy in front of a door the collector already dials, a synthetic-log DaemonSet, a test pod, and two caches — and each is a workload the estate never declared, so every one is disarmed explicitly.
- That census stops at five: `rollout_operator` and `minio` both ship off, so neither is owed a disarm, and a tombstone written against either proves nothing about a census it never ran. `rollout_operator` earns nothing under a single-binary topology regardless — it orders zone-aware restarts across statefulsets a single binary does not have.
- Tenancy governs every signal it can reach: the selected metrics store row's tenancy column arms `auth_enabled` here alongside the trace and profile backends, so an escalation never leaves logs pooled under one tenant while metrics are org-isolated.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the selected topology as stack children under Pulumi diff.
- `operate/observe#CHART_ROWS`: `_charts.loki` supplies chart and repo, and the row states `deploymentMode`, the three zeroed simple-scalable replica counts, the schema entry, the retention pair, filesystem storage, and the five disarmed default-on auxiliaries.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/logs` exporter dials this door directly at 3100, which is why the nginx gateway earns nothing.
- `grafana`(`.api/grafana.md`): the `logs` datasource plane resolves to this row's query address through the provisioned `loki` driver.
- `clickhouse`(`.api/clickhouse.md`): the residence alternative for wide-event logs — a residence takes logs and traces where this row takes logs alone, and both can hold the signal only because the collector fans it.

[LOCAL_ADMISSION]:
- Zero `write`, `read`, and `backend` replicas whenever `deploymentMode` is `SingleBinary`; the mode value alone leaves the chart's validator refusing to render.
- State one `schemaConfig.configs` entry; the empty default is a server that will not start.
- State retention as a pair — `limits_config.retention_period` with `compactor.retention_enabled` and a `delete_request_store`.
- Disarm `gateway`, `lokiCanary`, `test`, `chunksCache`, and `resultsCache` explicitly — that is the whole default-on set — and leave `minio` and `rollout_operator` where they ship, since both are already off and the estate's own object plane is the one store.
- Arm `allow_structured_metadata` wherever OTLP attributes must survive, and read `auth_enabled` off the store row's tenancy column rather than stating it locally.
