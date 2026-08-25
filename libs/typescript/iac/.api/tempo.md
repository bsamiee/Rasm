# [TS_IAC_API_TEMPO]

`tempo` is the trace backend in its single-binary chart. Two facts rule a fence against it: the chart installs NO receiver by default, so a trace door exists only where the values body declares one — and its Service publishes a FIXED port roster regardless, advertising jaeger, zipkin, and opencensus doors no receiver is listening on.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                          | [CAPABILITY]                                                                       |
| :-----: | :--------------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `tempo.receivers`                              | nested map — the ingest doors — EMPTY by default, so no receiver listens           |
|  [02]   | `tempo.retention`                              | duration, DEFAULT `24h` — the block retention window                               |
|  [03]   | `tempo.multitenancyEnabled`                    | `boolean` — org-scoped ingest and query; the store row's tenancy column governs it |
|  [04]   | `tempo.metricsGenerator`                       | `{ enabled, remoteWriteUrl, … }` — a SECOND span-derived series producer; declined |
|  [05]   | `tempo.storage`                                | `{ trace: { backend, … } }` — the block destination                                |
|  [06]   | `tempo.overrides` `tempo.per_tenant_overrides` | per-tenant limits                                                                  |
|  [07]   | `tempo.{ingester,querier,queryFrontend}`       | component-level configuration inside the single binary                             |
|  [08]   | `tempo.*` passthrough                          | server block and container passthrough                                             |
|  [09]   | `tempo.*` runtime posture                      | runtime posture                                                                    |
|  [10]   | `config`                                       | the WHOLE Tempo config document as a Go template over `.Values.tempo`              |
|  [11]   | `persistence`                                  | the block claim; DEFAULT DISABLED                                                  |
|  [12]   | `tempoQuery`                                   | the standalone jaeger-UI query sidecar                                             |
|  [13]   | `service`                                      | the door's type and annotations — NOT its port roster                              |
|  [14]   | `nameOverride` `fullnameOverride`              | `string` — FLAT top-level                                                          |

[PLACEMENT]: `replicas` `serviceAccount` `serviceMonitor` `podAnnotations` `extraVolumes` `nodeSelector` `tolerations` `affinity` `priorityClassName` `networkPolicy`
[RECEIVER_LAW]: `tempo.receivers` is empty on chart defaults, so an install taking the defaults accepts no traces at all and reports nothing. The OTLP door is `tempo.receivers.otlp.protocols.{http,grpc}.endpoint`, each a bind address the server listens on — declaring it is what makes the exporter's target real.
[SERVICE_PORTS]: the Service's port list is HARDCODED in the template and independent of `tempo.receivers` — it publishes jaeger thrift compact 6831/UDP, thrift binary 6832/UDP, thrift http 14268, jaeger grpc 14250, zipkin 9411, opencensus 55678, the legacy OTLP pair 55680/55681, OTLP grpc 4317, OTLP http 4318, and the 3200 metrics-and-query door. A door in that roster with no matching receiver routes to a closed port, so the roster is not evidence of an armed receiver.
[CONFIG_TEMPLATE]: `config` is the whole server document as a Go template rendered over `.Values.tempo`, which is why every key under `tempo.*` is a config coordinate rather than a chart abstraction — and why a raw directive lands by editing `config` rather than by inventing a values key.

[FULLNAME]: the standard collapse scaffold with flat overrides; the pin renders the StatefulSet, the Service, the ConfigMap, and the ServiceAccount under exactly the pinned name, verified by render.
[SERVICE_NAME]: `<fullname>` UNSUFFIXED. The query door is 3200 and the OTLP doors are 4317 and 4318 — every address this estate publishes reads the pinned name with the explicit port.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Span-derived series are the collector's connectors, not this backend's generator. `span_metrics` and `service_graph` mint RED and topology series from the same admitted spans one hop earlier, so the chart's own metrics generator is declined on its row — two generators over one span stream fork the series and no board can tell which it read.
- Tenancy is inherited: `multitenancyEnabled` reads the selected metrics store row's tenancy column, so traces are org-isolated exactly when metrics are, and the collector stamps the scope header on every exporter whose backend reads it.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the server as a stack child under Pulumi diff.
- `operate/observe#CHART_ROWS`: `_charts.tempo` supplies chart and repo, and the row states retention, the tenancy flag, the explicit OTLP receiver pair bound on all interfaces, the declined metrics generator, and an armed 20Gi persistence claim.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/traces` exporter dials the receiver this row declared, and its `span_metrics`/`service_graph` connectors are the declined generator's replacement.
- `grafana`(`.api/grafana.md`): the `traces` datasource plane resolves to the 3200 query door through the provisioned `tempo` driver, and exemplar click-through from the metrics store lands here.
- `clickhouse`(`.api/clickhouse.md`): the residence alternative for wide-event traces; both can hold the signal because the collector fans it, and the residence takes no cardinality ceiling where a TSDB must.

[LOCAL_ADMISSION]:
- Declare the OTLP receiver explicitly; chart defaults accept no traces and the failure is silence, not an error.
- Arm `persistence`; without a claim the blocks live in the pod and a reschedule is data loss.
- State retention through `tempo.retention` — the 24h default is a window no profile chose.
- Read `multitenancyEnabled` off the store row's tenancy column rather than stating it locally.
- Leave `metricsGenerator` disabled; the collector's connectors own span-derived series.
- Never read the Service's port roster as evidence of an armed door — the list is fixed and most of it is closed.
