# [TS_IAC_API_OPENCOST]

`opencost` is the pricing row: one exporter reading the cluster's own metrics store and emitting cost series scoped by namespace and tenant label. Two chart behaviors decide every fence against it — the three upstream modes are SUMMED and refuse to render when more than one holds, with the in-cluster mode ON by default, and the UI and MCP containers likewise ship on, standing up a second board plane and an agent surface beside the metrics door.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                             | [CAPABILITY]                                                                     |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `opencost.prometheus.internal`                    | in-cluster upstream, DEFAULT ENABLED — composes the endpoint from its five parts |
|  [02]   | `opencost.prometheus.external`                    | `{ enabled, url }` — an explicit upstream URL, `tpl`-evaluated                   |
|  [03]   | `opencost.prometheus.amp`                         | `{ workspaceId, sigV4Proxy }` — the managed-Prometheus arm                       |
|  [04]   | `opencost.prometheus.thanos`                      | the long-range reader; internal and external together FAIL                       |
|  [05]   | `opencost.prometheus` auth rows                   | upstream auth; `kubeRBACProxy` beside `bearer_token` FAILS                       |
|  [06]   | `opencost.ui.enabled`                             | `boolean` DEFAULT TRUE — a whole second board plane on port 9090                 |
|  [07]   | `opencost.mcp.{enabled,port}`                     | `boolean` DEFAULT TRUE / `int` 8081 — an agent surface beside the metrics door   |
|  [08]   | `opencost.exporter.*` workload                    | the exporter workload; `apiPort` 9003 is the metrics door                        |
|  [09]   | `opencost.exporter.*` identity                    | cluster identity and provider pricing                                            |
|  [10]   | `opencost.exporter.*` sources                     | the durable and alternate-source surface                                         |
|  [11]   | `opencost.metrics.serviceMonitor.*`               | Prometheus Operator registration, off by default                                 |
|  [12]   | `opencost.metrics.kubeStateMetrics.*`             | KSM emission compatibility toggles                                               |
|  [13]   | `service.*`                                       | the rendered door                                                                |
|  [14]   | `{name,fullname,namespace}Override` `clusterName` | FLAT; `clusterName` is a DNS SUFFIX, never a display name                        |

[EXPORTER_ROWS]: workload `apiPort` `debugPort` `replicas` `resources` `probes` `securityContext` `command` `extraArgs`; identity `defaultClusterId` `clusterIdConfigmap` `cloudProviderApiKey`; sources `persistence` `csv_path` `prometheusDataSource` `collectorDataSource` `inferenceCostTracking` `aws` `env`/`extraEnv` `extraVolumeMounts` `adminToken` `apiIngress` `apiHttpRoute`
[UPSTREAM_AUTH]: `secret_name` `existingSecretName` `username`/`username_key` `password`/`password_key` `bearer_token`/`bearer_token_key` `kubeRBACProxy` `insecureSkipVerify`
[UPSTREAM_EXCLUSIVITY]: the chart's own validator SUMS the `internal.enabled`, `external.enabled`, and `amp` flags and fails the render above one. `internal` is ON by default, so arming `external` ALONE is a hard render failure — both halves belong in every values body that leaves the in-cluster default.
[INTERNAL_COMPOSITION]: the in-cluster endpoint composes as `<scheme>://<serviceName>.<namespaceName>.svc.<clusterName>:<port><path>`, defaulting to `prometheus-server` in `prometheus-system` on port 80. That default resolves only where a `prometheus` chart release is literally named `prometheus` in that namespace, which is why a cluster pinning its own store name states the external arm instead.

[FULLNAME]: the standard collapse scaffold with flat `nameOverride`/`fullnameOverride`, beside `namespaceOverride` driving the rendered namespace.
[SERVICE_NAME]: the Service is `<fullname>` UNSUFFIXED, ClusterIP, gated `service.enabled`, and carries THREE ports on chart defaults — `http` 9003 always, `mcp-server` 8081 while the MCP row holds, and `http-ui` 9090 while the UI row holds — beside an optional debug port and `service.extraPorts`.
[IMAGE_PINNING]: the exporter and UI image tags are DIGEST-PINNED in the chart. A plain tag supplied through `--set` silently drops the pinning, which is the one path by which a content-addressed cluster acquires a floating image.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The cluster prices itself off its OWN store row: the upstream is the selected metrics store's `read` URL, so a store swap re-points every cost series with zero edits here. Cost series scope by namespace and by the tenant label the tenancy owners already stamp, so the pricing plane inherits the isolation posture rather than declaring a second one.
- The docker arm declares its degrade — container stats with no Kubernetes allocation feed carry no cost cell, so the dev loop prices nothing and states so rather than rendering an empty board.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the exporter as a stack child under Pulumi diff and CrossGuard validation.
- `operate/observe#CHART_ROWS`: `_charts.opencost` supplies chart and repo, the row installs while `spec.profile.observe.costs` holds, and `fullnameOverride` pins the name `_urls` projects.
- `prometheus`(`.api/prometheus.md`): the pinned store's `read` URL is the `external.url` value, and the in-cluster default is disarmed in the same values body because the chart refuses two armed modes.
- `operate/observe#BOARD_APPLY`: cost boards compile through the standing fold into the default and tenant orgs, reading the same store the exporter writes through — one datasource, two producers.

[LOCAL_ADMISSION]:
- Arm the external upstream and DISARM the internal one in the same values body; arming one alone is a failed render, not a degraded read.
- Delete the UI and MCP containers explicitly — both ship on, and both stand up surfaces this cluster never declared beside the one metrics door it did.
- Point the upstream at the store row's projection rather than at the chart's composed in-cluster default, which resolves only under a release naming coincidence.
- Leave the digest-pinned image tags alone; a plain tag override drops the pin.
- Read `clusterName` as the DNS suffix it is — a display name spelled there breaks in-cluster endpoint composition.
- Never arm `kubeRBACProxy` beside a bearer token, and never arm both Thanos arms; each pair is a hard fail.
