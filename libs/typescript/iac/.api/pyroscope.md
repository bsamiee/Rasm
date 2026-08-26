# [TS_IAC_API_PYROSCOPE]

`pyroscope` is the profiles backend, the LGTM plane's fourth signal. Its whole surface NESTS under a `pyroscope` key — the override that pins the rendered name is `pyroscope.fullnameOverride` and the flat top-level spelling reaches nothing — and the chart bundles an Alloy collector that ships DEFAULT-ON beside the server.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                           | [CAPABILITY]                                                                    |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `pyroscope.*Override`           | `string` — the LIVE pins — NESTED, not top-level                                |
|  [02]   | `pyroscope.structuredConfig`    | nested map — the whole server document, tenancy and limits included             |
|  [03]   | `pyroscope.extraArgs`           | `map` — binary flags — the local-retention floor lives here                     |
|  [04]   | `pyroscope.persistence`         | `{ enabled, size, storageClassName, accessModes, … }` — the profile-store claim |
|  [05]   | `pyroscope` door rows           | the three doors — HTTP 4040, gRPC, and the gossip ring                          |
|  [06]   | `pyroscope.*` placement         | placement and privilege                                                         |
|  [07]   | `pyroscope.*` identity          | identity and scheduling                                                         |
|  [08]   | `pyroscope.*` runtime           | self-profiling, DNS suffix, and the metadata store                              |
|  [09]   | `architecture`                  | the monolithic versus microservices component split                             |
|  [10]   | `alloy.enabled`                 | `boolean` DEFAULT TRUE — a bundled Grafana Alloy collector beside the server    |
|  [11]   | `agent.enabled` `minio.enabled` | `boolean` DEFAULT FALSE — the legacy agent and a bundled object store           |

[PASSTHROUGH]: `pyroscope.extraEnvVars` `extraCustomEnvVars` `extraEnvFrom` `initContainers` `extraContainers` `extraVolumes`
[STRUCTURED_CONFIG]: the server document lives at `pyroscope.structuredConfig` — `limits.max_query_lookback` bounds the query window and `multitenancy_enabled` arms org scoping, which this cluster reads off the selected metrics store row's tenancy column rather than stating locally.
[RETENTION_FLAG]: local disk retention is a BINARY FLAG, not a config section — `extraArgs["pyroscopedb.retention-policy-min-free-disk-gb"]` is the free-space floor the store evicts against, so retention here is a disk-pressure policy where every other signal states a time window.

[FULLNAME]: NESTED. `pyroscope.fullnameOverride` renders the server StatefulSet, its Service, `<pin>-headless`, and `<pin>-memberlist` under the pin; a flat top-level `fullnameOverride` renames NOTHING — verified by render, where the flat form leaves `<release>-pyroscope` standing. The bundled Alloy workload keeps `<release>-alloy` regardless, because the pin reaches the `pyroscope` scaffold alone.
[SERVICE_NAME]: `<pin>` on HTTP 4040 is the ingest and query door, `<pin>-headless` serves the peers, and `<pin>-memberlist` carries the gossip ring.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Profiles ride the PUSH path: the runtime SDKs push directly and the collector's profiles pipeline relays, so this backend has no scrape posture to state and no receiver to declare.
- The row is present by default and its REMOVAL is the spec delta — the LGTM plane carries four signals, not three, and a profile-free stack is a deliberate choice rather than an omission.
- Profiles stay here rather than moving to a wide-event tier: a tier takes logs and traces alone, so this backend holds its signal until the tier's own swap point arms.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the server as a stack child under Pulumi diff.
- `operate/observe#CHART_ROWS`: `_charts.pyroscope` is the one row whose `fullname` projection NESTS — `{ pyroscope: { fullnameOverride: name } }` — while every sibling states it flat, and `_urls` reads the same pinned name back.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the gateway's `otlp_http/profiles` exporter dials this door while the profiles row holds, and the agent topology's profile leg relays over the gateway's own OTLP door because the Arrow receiver frames three signals and not this one.
- `grafana`(`.api/grafana.md`): the `profiles` datasource plane resolves to the 4040 door through the provisioned driver, and the actor span tracing on the runtime side is what links a profile to its correlation anchor.

[LOCAL_ADMISSION]:
- Nest the pin under `pyroscope`; the flat spelling is accepted and inert, and every address derived from it resolves to nothing.
- Arm `pyroscope.persistence`; without a claim the local profile store lives in the pod.
- State the retention floor as the disk-free flag, not as a time window — this engine has no time-retention key.
- Read `multitenancy_enabled` off the store row's tenancy column so profiles are org-isolated exactly when metrics are.
- Disarm the bundled Alloy where the cluster's own collector already owns ingest; leaving it on installs a second agent nothing declared.
- Leave `minio` off; the cluster's object plane is the one store.
