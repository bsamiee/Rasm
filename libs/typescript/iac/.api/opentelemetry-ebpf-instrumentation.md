# [TS_IAC_API_OPENTELEMETRY_EBPF_INSTRUMENTATION]

`opentelemetry-ebpf-instrumentation` is the RED-metric and trace source for workloads carrying no SDK: a privileged DaemonSet whose eBPF probes read HTTP, gRPC, and SQL traffic off the host kernel and push OTLP at the collector door. One chart row carries two contracts: CHART values decide privilege, scheduling, and the ConfigMap, and the AGENT document the chart plants under `config.data` configures the probes. Both resolve as wire data a fence spells rather than imports.

## [01]-[CHART_VALUES]

`privileged` carries the admission decision — host privilege a workload cannot grant itself — and every other key places the DaemonSet.

| [INDEX] | [KEY]                                 | [CAPABILITY]                                                                              |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `config.data`                         | `[03]-[AGENT_CONFIG]` — carries the agent document the chart writes to its ConfigMap      |
|  [02]   | `config.*`                            | `boolean` / `string` — chart-authored config, or an operator ConfigMap                    |
|  [03]   | `preset`                              | `application` / `network` — selects the shipped property bundle                           |
|  [04]   | `privileged`                          | `boolean` — full privilege; `false` selects the capability path                           |
|  [05]   | `securityContext` `extraCapabilities` | `SecurityContext` / `string[]` — grants the capability path names                         |
|  [06]   | `contextPropagation.enabled`          | `boolean` — HTTP and TCP trace-context injection; adds `NET_ADMIN`                        |
|  [07]   | `dnsPolicy` `hostNetwork`             | `string` / `boolean` — `ClusterFirstWithHostNet` keeps DNS under host networking          |
|  [08]   | `image.*`                             | distribution selection; `digest` outranks `tag`                                           |
|  [09]   | `rbac.*` `serviceAccount.*`           | grant and identity the metadata reader earns                                              |
|  [10]   | `service.*`                           | serves the Prometheus and internal-metrics listeners, both off                            |
|  [11]   | `serviceMonitor.*`                    | operator scrape objects over those listeners                                              |
|  [12]   | `k8sCache.*`                          | shared metadata cache; `replicas: 0` disables it                                          |
|  [13]   | `env` `envValueFrom`                  | `Record<string,string>` / rows — `OTEL_EBPF_*` overrides outranking every ConfigMap value |
|  [14]   | `<scheduling>`                        | placement, roll posture, and extra mounts                                                 |

[config]: `create` `name` `skipConfigMapCheck` — an operator ConfigMap keys the document `ebpf-instrument-config.yaml`
[preset]: `application` is the shipped value; `network` arms the network-flow signal and demands host networking with it
[image]: `registry` `repository` `tag` `digest` `pullPolicy` `pullSecrets`
[rbac]: `create` `extraClusterRoleRules`
[serviceAccount]: `create` `automount` `name` `labels` `annotations`
[service]: `enabled` `type` `port` `portName` `targetPort` `appProtocol` `nodePort` `clusterIP` `loadBalancerIP` `loadBalancerClass` `loadBalancerSourceRanges` `externalIPs` `externalTrafficPolicy` `annotations` `labels` `internalMetrics.{port,targetPort,portName,appProtocol,nodePort}` — `targetPort` defaults off the agent's own `prometheus_export.port`
[serviceMonitor]: `enabled` `additionalLabels` `annotations` `jobLabel` `metrics.endpoint.interval` `internalMetrics.endpoint.interval`
[k8sCache]: `replicas` `profilePort` `image.*` `service.{name,port,annotations,labels}` `internalMetrics.{port,path,portName}` `env` `envValueFrom` `resources` `annotations` `podAnnotations` `podLabels`
[scheduling]: `updateStrategy.type` `priorityClassName` `nodeSelector` `tolerations` `affinity` `resources` `podSecurityContext` `podAnnotations` `podLabels` `annotations` `initContainers` `volumes` `volumeMounts` `nameOverride` `fullnameOverride` `namespaceOverride`

[PRIVILEGE]: `privileged: true` is the shipped posture and `securityContext.privileged` mirrors it; dropping to the capability path means naming each capability the probes need — `SYS_ADMIN` for Go trace-context propagation and for Debian kernels at `perf_event_paranoid >= 3`, `NET_ADMIN` whenever `contextPropagation` holds, `SYS_RESOURCE` only below kernel 5.11 where locked-memory limits still bind. Spec data carries the toggle, never a chart default.

## [02]-[AGENT_CONFIG]

One document under `config.data`, written verbatim into the ConfigMap and overridable key-by-key through `env`.

[EXPORT]:
- `otel_traces_export.endpoint`, `otel_metrics_export.endpoint`: agent pushes OTLP at both doors; the chart defaults each to a host-IP address, so binding them to the tier's one collector endpoint is what makes the DaemonSet reach the gateway
- `prometheus_export.{port,path}`: agent exposes its own scrape surface here, which `service.targetPort` and `serviceMonitor` both default off
- `internal_metrics.prometheus.{port,path}`: agent self-telemetry on a second listener

[DISCOVERY]:
- `attributes.kubernetes.enable`: pod, namespace, and workload decoration on every emitted signal
- `filter.network.{k8s_src_owner_name,k8s_dst_owner_name}.{match,not_match}`: glob filters keeping the platform's own components out of the network series
- `network.enable`: arms the network-flow signal, as does the `network` preset
- `routes.unmatched`: route-name policy for paths the heuristics do not resolve
- `open_port`, `log_level`, `profile_port`: process selection, verbosity, and the agent's own pprof door

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Agent produces at the collector's door holding no queue, no backend address, and no store opinion, so every posture question past the export endpoint answers at the collector.

[STACKING]:
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): both export endpoints bind the collector's `otlp` receiver, so the agent's traces and metrics enter the identical pipelines an SDK-ful workload enters and pick up the same processors, connectors, and exporter queue — the SDK-less path differs at the source and nowhere downstream.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the DaemonSet and its RBAC as parented children, so the privilege grant reads as a CrossGuard-visible object rather than a chart-internal fact.
- `operate/observe#CHART_ROWS`: `_charts.ebpf` gates on `spec.profile.observe.ebpf` and binds `config.data.otel_traces_export.endpoint` and `config.data.otel_metrics_export.endpoint` to the tier's one `collectorEndpoint`.

[LOCAL_ADMISSION]:
- Deploy targets granting privileged host access admit this row; absent that grant the toggle stays off and SDK-ful workloads carry the RED series alone.
- `preset: network` and `config.data.network.enable` both demand host networking, which breaks cluster DNS for the agent — a collector reached by service DNS needs the `application` preset.
