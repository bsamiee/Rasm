# [TS_IAC_API_PROMETHEUS]

`prometheus` is the reference metrics store — one server, native exemplars, and the click-through plane every board links on. Two facts rule every fence against it: the override key that renames the rendered Service is `server.fullnameOverride` and NOT the top-level one, and the rendered config is SYNTHESIZED from dedicated values beside a verbatim `serverFiles` dump, so a key spelled in both seats emits twice into one YAML document.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                     | [CAPABILITY]                                                                     |
| :-----: | :---------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `server.fullnameOverride`                 | `string` — the LIVE name pin; the top-level key is inert                         |
|  [02]   | `server.retention` `server.retentionSize` | `string` — the time and size bounds on the single server's TSDB                  |
|  [03]   | `server.extraFlags`                       | `string[]` WITHOUT `--` — feature flags; the chart renders each as `- --{{ . }}` |
|  [04]   | `server.extraArgs`                        | `map` — rendered as `--key=value`, the pair form `extraFlags` cannot spell       |
|  [05]   | `server.defaultFlagsOverride`             | `string[]` WITH `--` — REPLACES the default flag set outright                    |
|  [06]   | `server.otlp`                             | the canonical OTLP receiver seat, rendered from `storage`-adjacent config        |
|  [07]   | `server.tsdb` `server.exemplars`          | the same dedicated-seat pattern for `storage.tsdb` and `storage.exemplars`       |
|  [08]   | `server.persistentVolume`                 | `{ enabled, size, accessModes, mountPath, … }` — the TSDB claim, DEFAULT-ON      |
|  [09]   | `server.statefulSet.enabled`              | `boolean` — false renders a Deployment; true adds a headless Service             |
|  [10]   | `server.service.*`                        | the rendered door; port 80 onto container 9090                                   |
|  [11]   | `serverFiles."prometheus.yml"`            | `map` — dumped VERBATIM into the same ConfigMap key the dedicated seats write    |
|  [12]   | `serverFiles.*_rules.yml`                 | `{ groups }` — the two live rule files the default `rule_files` list mounts      |
|  [13]   | `scrapeConfigs` `extraScrapeConfigs`      | scrape jobs merged into the synthesized document                                 |
|  [14]   | subchart `enabled` rows                   | `boolean` — the four default-on subcharts                                        |
|  [15]   | `nameOverride`                            | `string` — absent from values yet LIVE — it feeds labels and the server fullname |

[SUBCHARTS]: `alertmanager.enabled` `kube-state-metrics.enabled` `prometheus-node-exporter.enabled` `prometheus-pushgateway.enabled` — all four DEFAULT-ON
[OTLP_STRATEGY]: `server.otlp.translation_strategy` closes on `UnderscoreEscapingWithSuffixes` (default), `NoUTF8EscapingWithSuffixes`, `UnderscoreEscapingWithoutSuffixes`, and the experimental `NoTranslation`, which warns on collision. Siblings under the same seat: `promote_resource_attributes`, `promote_all_resource_attributes`, `ignore_resource_attributes`, `keep_identifying_resource_attributes`, `convert_histograms_to_nhcb`, `promote_scope_metadata`, `label_name_underscore_sanitization`, `label_name_preserve_multiple_underscores`.
[FEATURE_FLAGS]: `enable-feature=exemplar-storage` is live and pairs with `server.exemplars`; `web.enable-otlp-receiver` is live and bare; `enable-feature=native-histograms` is REFUTED on the pinned server — it is accepted, warns, and does nothing, because the live control is the per-scrape-config `scrape_native_histograms` key.
[EXEMPLAR_SEAT]: `server.exemplars` ships as an EMPTY map and the ConfigMap template gates the whole `storage` block on it being truthy, so an unset seat renders no `storage.exemplars` at all and the feature flag alone opens the store at the server's own bound. Its one sub-key is `max_exemplars`, whose server default is `100000` — one in-memory circular buffer trimmed by eviction, so it bounds memory rather than a retention window, and a value at or below zero disables exemplar storage with no restart. That flag OPENS the store and this seat SIZES it; neither substitutes for the other.
[SERVER_CLAIM]: `server.persistentVolume` ships ARMED — `enabled: true`, `size: 8Gi`, `accessModes: [ReadWriteOnce]`, `mountPath: /data` — so the risk here is not a lost claim on reschedule but an INHERITED size: a chart bump moving that default resizes the TSDB every alert evaluates against, silently. `emptyDir` is the volatile alternative the same block gates, and `statefulSetNameOverride` renames the generated claim only under `server.statefulSet.enabled`.

[FULLNAME]: DIVERGENT. The top-level `fullnameOverride` is absent from `values.yaml`, and the `prometheus.fullname` helper it would feed is invoked by ZERO templates — setting it renames nothing. The live pin is `server.fullnameOverride`; absent it the server name resolves as `<release>-<server.name>` when the release CONTAINS the chart name and `<release>-<name>-<server.name>` otherwise, with `server.name` defaulting to `server`.
[SERVICE_NAME]: the server Service is `<server.fullname>` — which carries the `-server` tail only when the override is ABSENT — on port 80 onto container 9090, named `http`, ClusterIP, gated `server.service.enabled`. A release named `prometheus` renders `prometheus-server`; a release named `mon` renders `mon-prometheus-server`; an override renders exactly the override. The four subcharts render `<release>-alertmanager`, `<release>-kube-state-metrics`, `<release>-prometheus-node-exporter`, and `<release>-prometheus-pushgateway`, the doubled names correct under the collapse rule, and the parent's helpers REPRODUCE those names in the default scrape config even where the subchart is disabled.
[MERGE_SEMANTICS]: the ConfigMap template synthesizes `prometheus.yml` by concatenating `global`, `remote_write`, `remote_read`, `storage`, `otlp`, `scrape_config_files`, and `scrape_configs`, then dumping the REMAINDER of `serverFiles."prometheus.yml"`, then `alerting`. Helm never merges lists, so supplying `rule_files` under `serverFiles` REPLACES the default `[/etc/config/recording_rules.yml, /etc/config/alerting_rules.yml]` pair and the rule content then mounts at a path nothing reads. Setting one SUB-KEY merges into the default map and leaves that list intact.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One store per stack answers health and alerting; the residence answers evidence and history. This row is the reference posture on that axis — single server, exemplars on, tenant as a LABEL rather than an isolation boundary, and that degradation stated on the row rather than discovered on an empty query.
- The receiver decides the histogram representation, not the exporter: leaving `convert_histograms_to_nhcb` off keeps an OTLP exponential histogram as one native series, and arming it lands the `le`-bearing buckets the classic quantile arm renders against.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the server; the recording-rule content arrives as a values row rather than a mounted ConfigMap because the chart already owns the mount path.
- `operate/observe#STORE_ROWS`: `_stores.prometheus` supplies the chart and repo, `write` projects `http://<pinned>.<ns>.svc/api/v1/otlp` and `read` the bare origin — no port on either, because the Service answers 80 — and the row's `translation`, `histogram`, `exemplars`, `rules`, and `tenancy` columns are what every board query and every burn rule read.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/metrics` exporter dials the `write` path, so this server's OTLP receiver is the estate's one metrics ingest door and no workload learns the address.
- `opencost`(`.api/opencost.md`): the pricing row aims its upstream at this row's `read` URL, so a store swap re-points cost series with no opencost edit.
- `operate/observe#BOARD_APPLY`: the `plugin` column names the stock `prometheus` datasource, and the exemplar column is what makes the trace click-through plane real rather than a degraded trace search.

[LOCAL_ADMISSION]:
- Pin the name at `server.fullnameOverride`; the top-level key is accepted and inert, and every address derived from it resolves to a Service the chart never rendered.
- Disable all four subcharts explicitly: none was declared by this estate, node and object state arrive through the collector's own enrichment, nothing pushes, and the alertmanager row additionally injects an `alerting` block that wires a delivery path beside the one the board plane owns.
- Spell feature flags in `server.extraFlags` WITHOUT leading dashes, and never spell `native-histograms` there.
- Write OTLP settings at `server.otlp` alone. Spelling `otlp` under `serverFiles` too emits duplicate keys in one document; the same trap holds for `global`, `scrape_configs`, `alerting`, `remote_*`, and `storage`.
- Size the exemplar ring at `server.exemplars.max_exemplars` whenever the flag is armed — an empty seat renders no block and the decisive capability then rides a server default. Its `storage` sibling under `serverFiles` is the duplicate-key trap, never the alternative.
- State `server.persistentVolume.size` rather than inheriting it; the claim is already armed, so what a row records here is the SIZE.
- Land rule CONTENT as a `serverFiles` sub-key, never as a replacement of the whole `prometheus.yml` map.
