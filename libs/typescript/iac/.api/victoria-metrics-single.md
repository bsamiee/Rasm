# [TS_IAC_API_VICTORIA_METRICS_SINGLE]

`victoria-metrics-single` is the metrics store's resource-pressure escape: one lean binary where the reference row's footprint is the constraint. Its whole surface hangs off a `server` block, and its two override keys behave DIFFERENTLY — the top-level pin renders `<pin>-server` while the nested one renders the bare pin, so which key a fence sets decides whether the `-server` tail is in the address.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `victoria-metrics-single`
- chart: `victoria-metrics-single` from `https://victoriametrics.github.io/helm-charts` (Apache-2.0), chart and `appVersion` versioned independently
- asset: the server StatefulSet or Deployment with its Service, ServiceAccount, RBAC cell, and persistent volume, beside an optional PDB, Ingress, Route, ServiceMonitor, and the `vmbackupmanager` sidecar
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / metrics store escape
- crds: NONE

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                             | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `server.retentionPeriod`          | bare integer means MONTHS; a unit character makes it explicit                   |
|  [02]   | `server.extraArgs`                | `map` — the binary's own flags — the OTLP naming posture lives here             |
|  [03]   | `server.http`                     | `[{ name, value, primary, tls, … }]` — the listen-address list; `:8428` primary |
|  [04]   | `server.mode`                     | `statefulSet` \| `deployment` — the workload shape                              |
|  [05]   | `server.persistentVolume`         | `{ enabled, size, storageClassName, accessModes, … }` — the data claim          |
|  [06]   | `server.emptyDir`                 | the volatile alternative to a claim                                             |
|  [07]   | `server.service`                  | the door — `clusterIP: None` by default                                         |
|  [08]   | `server.fullnameOverride`         | `string` — the NESTED pin — renders the bare name, no `-server` tail            |
|  [09]   | `server.*` placement              | placement and health                                                            |
|  [10]   | `server.{ingress,route}`          | external reach                                                                  |
|  [11]   | `server.vmbackupmanager`          | the enterprise backup sidecar                                                   |
|  [12]   | `nameOverride` `fullnameOverride` | `string` — the TOP-LEVEL pair — the pin renders `<pin>-server`                  |

[PASSTHROUGH]: `server.env` `envFrom` `extraVolumes` `extraVolumeMounts` `extraContainers` `initContainers`
[OVERRIDE_DUALITY]: verified by render — a top-level `fullnameOverride: obs-metrics` yields the Service `obs-metrics-server`, while `server.fullnameOverride: obs-metrics` yields `obs-metrics`. Both keys are live and they differ by exactly the component suffix, so a row must state WHICH one it set and derive its address from that answer. Default, with neither set, is `<release>-victoria-metrics-single-server`.
[RETENTION_UNIT]: `retentionPeriod` accepts `h`, `d`, `w`, and `y` suffixes, and a bare number means MONTHS. The chart default of `1` is therefore one month, and a duration string carried from another store row's vocabulary must keep its unit character or silently change meaning.
[TRANSLATION_POSTURE]: this dialect answers the OTLP naming question by leaving the flag off — `server.extraArgs["opentelemetry.usePrometheusNaming"] = "false"` ingests names as sent, so no escaping and no type or unit suffix rules run, and the row's translation column reads `NoTranslation` to match.

[FULLNAME]: two live keys with different results; see `[OVERRIDE_DUALITY]`.
[SERVICE_NAME]: `<top-level pin>-server` or `<nested pin>`, serving the primary listen address on 8428 — the write path at `/opentelemetry` and the read path at the bare origin. The default `clusterIP: None` makes it headless, which is what the StatefulSet mode expects.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- This row is the footprint escape, and its degradation is stated rather than discovered: no native histograms, no exemplar storage, no type or unit suffixes, and no in-store rule evaluator — so click-through degrades to trace search, series carry the bare dotted name, and every burn numerator renders inline per alert evaluation rather than resolving against a recorded series.
- Tenancy on this row is a LABEL, never an isolation boundary, exactly as on the reference row — an estate needing org isolation escalates to the distributed store instead.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the binary as a stack child under Pulumi diff.
- `operate/observe#STORE_ROWS`: `_stores.victoriametrics` supplies chart and repo, pins the name, states retention and the naming flag, and projects `http://<pinned>-server.<ns>.svc:8428/opentelemetry` for write and the same origin for read.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/metrics` exporter dials the `/opentelemetry` path, which is this engine's own OTLP ingest route rather than the Prometheus one.
- `grafana`(`.api/grafana.md`): the row's `plugin` column names the stock `prometheus` datasource, so the board plane queries this engine through the PromQL-compatible surface while the exemplar and histogram columns tell the board what it cannot render.
- `prometheus`(`.api/prometheus.md`) / `mimir-distributed`(`.api/mimir-distributed.md`): the two alternatives on one axis — reference and escalation — each stating its own retention key, translation dialect, and tenancy grain.

[LOCAL_ADMISSION]:
- State which override key the row sets and derive the address from THAT answer; the two keys differ by the `-server` suffix and reading the wrong one is a dead address.
- Carry a unit character on `retentionPeriod`; a bare number is months, which no other store row means.
- Set the OTLP naming flag explicitly rather than inheriting the binary's default, so the row's `translation` column and the ingest behavior agree.
- Arm `server.persistentVolume`; the `emptyDir` alternative loses the whole TSDB on reschedule.
- Never render a quantile or fraction panel against this row assuming native histograms — the engine stores buckets and the row's `histogram` column says `classic`.

[RAIL_LAW]:
- Contract: `victoria-metrics-single` chart values
- Owns: the lean single-binary metrics store — its listen addresses, retention, data claim, workload mode, binary flags, and the backup sidecar
- Accept: one override key stated explicitly with the address derived from it; a unit-carrying `retentionPeriod`; `opentelemetry.usePrometheusNaming: "false"` matching the row's `NoTranslation` column; an armed persistent volume; the `/opentelemetry` write path on 8428
- Reject: an address derived from the other override key; a bare-integer retention read as anything but months; an implicit naming posture; `emptyDir` where the series must survive; native-histogram or exemplar assumptions the engine cannot answer; an org-tenancy assumption on a label-tenancy row
