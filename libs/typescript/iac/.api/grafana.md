# [TS_IAC_API_GRAFANA]

`grafana` is the board plane's server. This cluster installs the chart for the RUNTIME alone — datasources, dashboards, folders, orgs, and alert rules all arrive afterwards through the Grafana provider against the running instance, so the chart's own provisioning surface stays unused and the values body is one credential row plus the placement the profile states.

## [01]-[CHART_VALUES]

| [INDEX] | [KEY]                                 | [CAPABILITY]                                                                         |
| :-----: | :------------------------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `adminUser` `adminPassword`           | `string` — the bootstrap credential; UNDECLARED in values yet read by the template   |
|  [02]   | `admin`                               | the reference alternative to a literal password                                      |
|  [03]   | `service`                             | the door — port 80 onto container 3000                                               |
|  [04]   | `headlessService`                     | `boolean` — a second Service for gossip-based HA                                     |
|  [05]   | `persistence`                         | `{ enabled, type, size, … }` — the SQLite claim; absent it, every board is ephemeral |
|  [06]   | `grafana.ini`                         | nested map — the whole `grafana.ini` document as values                              |
|  [07]   | `datasources`/`dashboards`/`alerting` | the chart's own provisioning surface — UNUSED here                                   |
|  [08]   | `plugins` `downloadDashboards*`       | plugin install and the dashboard fetch init container                                |
|  [09]   | `sidecar`                             | the ConfigMap-watching provisioner sidecar                                           |
|  [10]   | `ingress` `route` `extraExposePorts`  | external reach                                                                       |
|  [11]   | `fullnameOverride` `nameOverride`     | `string` — UNDECLARED in values, LIVE in the helper                                  |

[PLACEMENT]: `replicas` `autoscaling` `deploymentStrategy` `podDisruptionBudget` `resources` `nodeSelector` `tolerations` `affinity` `topologySpreadConstraints`
[PROVISIONING_KEYS]: `datasources` `dashboardProviders` `dashboards` `dashboardsConfigMaps` `alerting` `notifiers`, beside the ConfigMap-watching `sidecar`
[ENV_CARRIERS]: `env` `envValueFrom` `envFromSecret` `envRenderSecret` `envFromSecrets` `envFromConfigMaps`
[FULLNAME]: the standard collapse scaffold. Neither override key appears in `values.yaml`, yet both are read by `grafana.fullname` and honored — a pin renders the Deployment, the Service, the ConfigMap, and the Secret under exactly the pinned name, verified by render.
[SERVICE_NAME]: `<fullname>` UNSUFFIXED, ClusterIP, port 80 onto container 3000 with port name `service`. The address therefore carries no port, and `headlessService: true` adds `<fullname>-headless` for the gossip ring alone.
[PROVISIONING_SPLIT]: the chart provisions datasources, dashboards, folders, and alert rules from values or from watched ConfigMaps; the cluster declines all of it. Board content is code compiled through the Foundation SDK and applied by the Grafana provider against the running server, so a values-side dashboard would be a second content authority with its own drift.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The chart owns the RUNTIME and nothing above it: one instance, one credential, one door. Every datasource key names the PLANE it answers rather than the engine behind it, and every board is compiled content the provider applies — so a store swap re-points a datasource and edits no dashboard.
- The admin credential is in-graph: the same Doppler-generated read lands as `adminPassword` here and authenticates the provider that applies content, so the two halves of the board plane cannot disagree about who they are.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the server as a stack child under Pulumi diff and CrossGuard validation.
- `operate/observe#CHART_ROWS`: `_charts.grafana` supplies chart and repo, the row installs with `adminPassword` from the tier's `auth` argument and `fullnameOverride` pinned to the release, and `_urls.grafana` projects the port-free in-cluster address.
- `pulumiverse-grafana`(`.api/pulumiverse-grafana.md`): the provider that authenticates against this instance and applies every org, folder, datasource, dashboard, and alert rule — the reason the chart's provisioning surface stays empty.
- `@grafana/grafana-foundation-sdk`(`.api/grafana-grafana-foundation-sdk.md`): the compiler for that content, so a dashboard is a typed builder rather than a JSON blob in a ConfigMap.
- `prometheus`(`.api/prometheus.md`) / `loki`(`.api/loki.md`) / `tempo`(`.api/tempo.md`) / `pyroscope`(`.api/pyroscope.md`) / `clickhouse`(`.api/clickhouse.md`): the query doors the provisioned datasources dial, each named by plane and realized only where its row armed.

[LOCAL_ADMISSION]:
- Pin `fullnameOverride` to the release; the published address is derived from it and the port is 80, never 3000.
- Pass the admin password as the in-graph credential read, and let the chart render its Secret — a second hand-minted admin Secret forks the identity the provider authenticates with.
- Leave every provisioning key empty; content is code applied by the provider, and a values-side dashboard is a second authority.
- Arm `persistence` wherever the instance holds state the provider does not re-apply; without it a restart is a blank server.
- Reach secret material through the `env*` carriers alone — values render into a ConfigMap.
