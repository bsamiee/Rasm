# [TS_IAC_API_PULUMI_KUBERNETES]

`@pulumi/kubernetes` is the first-class engine of the `selfhosted-k8s` arm: strongly-typed resource classes for every Kubernetes API group (`core/v1`, `apps/v1`, `batch/v1`, `networking/v1`, `rbac/v1`, `storage/v1`, `apiextensions/v1`, …), the `helm.v4.Chart` / `helm.v3.Release` component wrappers that render upstream charts as typed value objects, the `apiextensions.CustomResource` carrier for operator CRDs (the CNPG `Cluster`, cert-manager `Certificate`, Prometheus `ServiceMonitor`), the `yaml`/`kustomize` manifest components, and the `Provider` that binds a kubeconfig. Two shapes cover the whole package: the generated TYPED resource (`apiVersion`/`kind`/`metadata`/`spec`/`status` as `Output`s) and the bespoke COMPONENT (`Chart`/`Release`/`Directory`/`ConfigGroup` over `pulumi.ComponentResource`). In `iac` this is the workload/data/traffic spine — the cluster-bootstrap row (`@pulumi/command`) yields the kubeconfig, `helm.v4.Chart` installs the CNPG operator + LGTM stack + OTel collector as typed values (zero authored YAML), `apiextensions.CustomResource` declares the CNPG cluster whose host feeds `@pulumi/postgresql`, and `@pulumiverse/grafana` applies dashboards onto the rendered Grafana.

[EXPORTS]: `Provider`
[EXPORTS]: `core` `apps` `batch` `networking` `rbac` `storage` `policy` `admissionregistration` `apiregistration` `autoscaling` `certificates` `coordination` `discovery` `events` `flowcontrol` `node` `scheduling` `settings` `meta` `apiextensions` `helm` `kustomize` `yaml` `types`

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/kubernetes`
- package: `@pulumi/kubernetes` (Apache-2.0)
- build-floor: peer `@pulumi/pulumi ^catalog`; bundles `glob`, `shell-quote` (chart/kustomize shell-out); the `pulumi-resource-kubernetes` plugin needs `helm` + `kubectl` reachable for chart render and SSA
- target: `node` (Automation-API program process; the plugin shells `helm template` and talks to the API server)
- entry: `@pulumi/kubernetes` plus the group sub-paths (`@pulumi/kubernetes/apps/v1`, `/helm/v4`, `/apiextensions`, …)
- asset: the typed resource classes for all API groups, `helm.v4.Chart` / `helm.v3.Release`, `apiextensions.CustomResource`/`CustomResourcePatch`/`v1.CustomResourceDefinition`, `kustomize.Directory`, `yaml.{ConfigFile,ConfigGroup}`, the `Provider`, and the `types.input`/`types.output` shape namespaces
- rail: iac / kubernetes

## [02]-[PUBLIC_TYPES]

### [02.1]-[THE_TYPED_RESOURCE_PATTERN_EVERY_API_GROUP_RESOURCE]

[PUBLIC_TYPE_SCOPE]: typed resource
- rail: iac / kubernetes
- entry: `@pulumi/kubernetes/<group>/<version>`

Every typed resource is `class Kind extends pulumi.CustomResource` with literal-typed `apiVersion`/`kind` discriminants and `metadata`/`spec`/`status` as `Output`s of the group's `types.output` shapes. `KindArgs` mirrors them as `types.input`. Unlike the cloud SDKs, the typed-resource `.get` is 3-arg (`name`, `id`, `opts?`) — server state is the source of truth, so there is no client-side `State` bag. Learn it once; the group roster is data.

| [INDEX] | [MEMBER]             | [SHAPE]                                                                                      |
| :-----: | :------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `class Kind`         | `extends pulumi.CustomResource`; literal-typed `apiVersion`/`kind` discriminants             |
|  [02]   | attributes           | `metadata: Output<meta.v1.ObjectMeta>`, `spec: Output<…Spec>`, `status: Output<…Status>`     |
|  [03]   | `constructor`        | `(name, args?: KindArgs, opts?: pulumi.CustomResourceOptions)`                               |
|  [04]   | `Kind.get`           | `static get(name, id: pulumi.Input<pulumi.ID>, opts?): Kind` — 3-arg (no client State)       |
|  [05]   | `Kind.isInstance`    | `static isInstance(obj): obj is Kind`                                                        |
|  [06]   | `interface KindArgs` | `metadata?: Input<meta.v1.ObjectMeta>`, `spec?: Input<…Spec>`, literal `apiVersion?`/`kind?` |
|  [07]   | `KindPatch` twin     | `<group>.<v>.<Kind>Patch` — Server-Side-Apply patch variant of every resource                |
|  [08]   | `KindList` twin      | `<group>.<v>.<Kind>List` — the list-kind resource                                            |

[DEPLOYMENT]: `Deployment.get(string,pulumi.Input<pulumi.ID>,pulumi.CustomResourceOptions?) -> Deployment` `Deployment.isInstance(any) -> obj is Deployment` `Deployment.apiVersion: pulumi.Output<"apps/v1">` `Deployment.kind: pulumi.Output<"Deployment">` `Deployment.metadata: pulumi.Output<outputs.meta.v1.ObjectMeta>` `Deployment.spec: pulumi.Output<outputs.apps.v1.DeploymentSpec>` `Deployment.status: pulumi.Output<outputs.apps.v1.DeploymentStatus>` `Deployment(string,DeploymentArgs?,pulumi.CustomResourceOptions?)`
[DEPLOYMENT_ARGS]: `DeploymentArgs.apiVersion: pulumi.Input<"apps/v1">` `DeploymentArgs.kind: pulumi.Input<"Deployment">` `DeploymentArgs.metadata: pulumi.Input<inputs.meta.v1.ObjectMeta|undefined>` `DeploymentArgs.spec: pulumi.Input<inputs.apps.v1.DeploymentSpec|undefined>`

### [02.2]-[API_GROUP_ROSTER_THE_DATA_FED_TO_THE_TYPED_PATTERN]

[PUBLIC_TYPE_SCOPE]: typed resource groups
- rail: iac / kubernetes

| [INDEX] | [GROUP]                        | [KEY_KINDS_THE_KUBE_ROWS_COMPOSE]                                                               |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `core.v1`                      | `Namespace`, `Service`, `Secret`, `ConfigMap`, `PersistentVolumeClaim`, `ServiceAccount`, `Pod` |
|  [02]   | `apps.v1`                      | `Deployment`, `StatefulSet`, `DaemonSet`, `ReplicaSet`                                          |
|  [03]   | `batch.v1`                     | `Job`, `CronJob`                                                                                |
|  [04]   | `networking.v1`                | `Ingress`, `IngressClass`, `NetworkPolicy` (the `kube/traffic` rows)                            |
|  [05]   | `rbac.authorization.k8s.io/v1` | `Role`, `ClusterRole`, `RoleBinding`, `ClusterRoleBinding`                                      |
|  [06]   | `storage.k8s.io/v1`            | `StorageClass`, `VolumeAttachment`                                                              |
|  [07]   | `apiextensions.k8s.io/v1`      | `CustomResourceDefinition` (`apiextensions.v1.CustomResourceDefinition`)                        |

Remaining generated groups follow this exact pattern: `policy`, `autoscaling`, `coordination`, `scheduling`, `admissionregistration`, `certificates`, `discovery`, `events`, `flowcontrol`, `node`, `settings`, `apiregistration`, `storagemigration`, `auditregistration`, `extensions`. Every group additionally exposes `*Patch` (SSA) and `*List` twins; `meta.v1.ObjectMeta` is the shared metadata input carrying `name`/`namespace`/`labels`/`annotations`.

### [02.3]-[HELM_V4_CHART_UPSTREAM_CHARTS_AS_TYPED_VALUE_OBJECTS]

[PUBLIC_TYPE_SCOPE]: helm component
- rail: iac / observe · data
- entry: `@pulumi/kubernetes/helm/v4`

`Chart` is a `ComponentResource` equivalent to `helm template --dry-run=server` followed by Pulumi-managed apply of the rendered manifests — so Pulumi transformations and CrossGuard policies see every rendered resource, and there is no Tiller/Release state. `values` is a typed object map (literals, nested maps, `Output`s, and `pulumi.asset.Asset`s), which is HOW `iac` supplies the LGTM/CNPG/OTel configuration with zero authored YAML.

| [INDEX] | [MEMBER]            | [SIGNATURE_FIELD]                                                                    |
| :-----: | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `class Chart`       | `extends pulumi.ComponentResource`; `(name, args?: ChartArgs, opts?)` constructor    |
|  [02]   | `Chart.isInstance`  | `static isInstance(obj): obj is Chart`                                               |
|  [03]   | `chart.resources`   | `readonly resources: pulumi.Output<any[]>` — the rendered child resources            |
|  [04]   | `chart` (arg)       | `Input<string>` — ref (`repo/name`), path, tgz, URL, or `oci://…`                    |
|  [05]   | `values`            | `Input<{ [k: string]: any }>` — highest-precedence typed value map                   |
|  [06]   | `valueYamlFiles`    | `values.yaml` override files as typed assets                                         |
|  [07]   | `repositoryOpts`    | chart-repo auth: `repo`/`username`/`password` + `caFile`/`certFile`/`keyFile` assets |
|  [08]   | `version` / `devel` | `Input<string>` pinned chart / pre-release admission                                 |
|  [09]   | `namespace`         | `Input<string>` — release namespace (bind to `core.v1.Namespace.metadata.name`)      |
|  [10]   | `skipCrds`          | `Input<boolean>` — omit chart CRDs                                                   |
|  [11]   | `skipAwait`         | `Input<boolean>` — do not block on readiness                                         |
|  [12]   | `includeHooks`      | `Input<boolean>` — emit `helm.sh/hook` resources under `renderYamlToDirectory` mode  |
|  [13]   | `postRenderer`      | `{ command, args }` kustomize/post-render hook                                       |
|  [14]   | `dependencyUpdate`  | `Input<boolean>` — chart dependency rebuild                                          |
|  [15]   | `verify`            | `Input<boolean>` — provenance verification                                           |
|  [16]   | `keyring`           | provenance keyring asset                                                             |
|  [17]   | `plainHttp`         | `Input<boolean>` — insecure HTTP fetch                                               |
|  [18]   | `resourcePrefix`    | `Input<string>` — rendered-resource name prefix                                      |
|  [19]   | `name`              | `Input<string>` — release name override                                              |

[CHART]: `Chart.isInstance(any) -> obj is Chart` `Chart.resources: pulumi.Output<any[]>` `Chart(string,ChartArgs?,pulumi.ComponentResourceOptions?)`
[CHART_ARGS]: `ChartArgs.chart: pulumi.Input<string>` `ChartArgs.values: pulumi.Input<{[key:string]:any}|undefined>` `ChartArgs.valueYamlFiles: pulumi.Input<pulumi.Input<pulumi.asset.Asset|pulumi.asset.Archive>[]|undefined>` `ChartArgs.repositoryOpts: pulumi.Input<inputs.helm.v4.RepositoryOpts|undefined>` `ChartArgs.version: pulumi.Input<string|undefined>` `ChartArgs.devel: pulumi.Input<boolean|undefined>` `ChartArgs.namespace: pulumi.Input<string|undefined>` `ChartArgs.skipCrds: pulumi.Input<boolean|undefined>` `ChartArgs.skipAwait: pulumi.Input<boolean|undefined>` `ChartArgs.includeHooks: pulumi.Input<boolean|undefined>` `ChartArgs.postRenderer: pulumi.Input<inputs.helm.v4.PostRenderer|undefined>` `ChartArgs.dependencyUpdate: pulumi.Input<boolean|undefined>` `ChartArgs.verify: pulumi.Input<boolean|undefined>` `ChartArgs.keyring: pulumi.Input<pulumi.asset.Asset|pulumi.asset.Archive|undefined>` `ChartArgs.plainHttp: pulumi.Input<boolean|undefined>` `ChartArgs.resourcePrefix: pulumi.Input<string|undefined>` `ChartArgs.name: pulumi.Input<string|undefined>`
[REPOSITORY_OPTS]: `RepositoryOpts.repo: pulumi.Input<string|undefined>` `RepositoryOpts.username: pulumi.Input<string|undefined>` `RepositoryOpts.password: pulumi.Input<string|undefined>` `RepositoryOpts.caFile: pulumi.Input<pulumi.asset.Asset|pulumi.asset.Archive|undefined>` `RepositoryOpts.certFile: pulumi.Input<pulumi.asset.Asset|pulumi.asset.Archive|undefined>` `RepositoryOpts.keyFile: pulumi.Input<pulumi.asset.Asset|pulumi.asset.Archive|undefined>`
[POST_RENDERER]: `PostRenderer.command: pulumi.Input<string>` `PostRenderer.args: pulumi.Input<pulumi.Input<string>[]|undefined>`

`helm.v3` exposes both `Chart` and `Release`. `helm.v3.Chart` extends `yaml.CollectionComponentResource` and renders client-side, exposing rendered objects only through `transformations` callbacks; `helm.v3.Release` is the stateful `helm install` where Pulumi drives the Helm SDK directly, with `atomic` / `skipAwait` / `waitForJobs` / `timeout` / `recreatePods` / `skipCrds` lifecycle knobs. `helm.v4.Chart` supersedes `v3.Chart` — server-side render keeps every rendered object under Pulumi diff, policy, and transform, and stays the default for Pulumi-managed resources; reach `helm.v3.Release` only when a chart requires true release lifecycle (hooks, rollback, atomic install).

### [02.4]-[APIEXTENSIONS_CUSTOMRESOURCE_THE_OPERATOR_CRD_CARRIER]

[PUBLIC_TYPE_SCOPE]: custom resource
- rail: iac / data
- entry: `@pulumi/kubernetes/apiextensions`

`CustomResource` instantiates any CRD instance (the CNPG `Cluster`, cert-manager `Certificate`, Grafana `GrafanaDashboard`) when no generated class exists. `CustomResourceArgs` is deliberately loose — only `apiVersion`/`kind` are required, with a `[field: string]: pulumi.Input<any>` catch-all for the operator's `spec`. Pair with `apiextensions.v1.CustomResourceDefinition` to install the CRD schema, and `CustomResourcePatch` for SSA into an operator-owned object.

| [INDEX] | [MEMBER]                                    | [SIGNATURE_FIELD]                                                                          |
| :-----: | :------------------------------------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `class CustomResource`                      | `extends pulumi.CustomResource`; `(name, args: CustomResourceArgs, opts?)` constructor     |
|  [02]   | `CustomResource.get`                        | `get(name, opts)` — id via `{ apiVersion, kind, id, namespace? }`                          |
|  [03]   | attributes                                  | `apiVersion`/`kind`/`metadata` `Output`s (string-typed, not literal discriminants)         |
|  [04]   | `getInputs()`                               | `(): CustomResourceArgs` — recover the declared spec                                       |
|  [05]   | `interface CustomResourceArgs`              | required `apiVersion`/`kind` strings, optional `metadata`, `[field]: Input<any>` catch-all |
|  [06]   | `class CustomResourcePatch`                 | SSA patch twin over an existing CRD instance                                               |
|  [07]   | `apiextensions.v1.CustomResourceDefinition` | the typed CRD-install resource (`spec.group`/`names`/`versions`/`scope`)                   |

[CUSTOM_RESOURCE]: `CustomResource.get(string,CustomResourceGetOptions) -> CustomResource` `CustomResource.apiVersion: pulumi.Output<string>` `CustomResource.kind: pulumi.Output<string>` `CustomResource.metadata: pulumi.Output<outputs.meta.v1.ObjectMeta>` `CustomResource.getInputs() -> CustomResourceArgs` `CustomResource(string,CustomResourceArgs,pulumi.CustomResourceOptions?)`
[CUSTOM_RESOURCE_ARGS]: `CustomResourceArgs.apiVersion: string` `CustomResourceArgs.kind: string` `CustomResourceArgs.metadata: pulumi.Input<inputs.meta.v1.ObjectMeta>` `CustomResourceArgs[string]: pulumi.Input<any>`

### [02.5]-[YAML_KUSTOMIZE_PROVIDER_MANIFEST_COMPONENTS_AND_BINDING]

[PUBLIC_TYPE_SCOPE]: manifest components + provider
- rail: iac / kubernetes

| [INDEX] | [MEMBER]                                | [SIGNATURE_FIELD]                                                                              |
| :-----: | :-------------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `yaml.ConfigFile` / `ConfigGroup`       | `ComponentResource` per file / glob / literal manifest set; `transformations`/`resourcePrefix` |
|  [02]   | `yaml.v2.ConfigGroup`                   | the catalog-bound SSA-aware manifest component                                                 |
|  [03]   | `kustomize.Directory`                   | `ComponentResource` running `kustomize build` over a dir/URL; `kustomize.v2.Directory` twin    |
|  [04]   | `class Provider`                        | `extends pulumi.ProviderResource`; `(name, args?: ProviderArgs, opts?)` constructor            |
|  [05]   | `ProviderArgs.kubeconfig`               | `Input<string>` — the cluster-bootstrap kubeconfig (`@pulumi/command` output)                  |
|  [06]   | `ProviderArgs.context`/`cluster`        | `Input<string>` — kubeconfig context / cluster selection                                       |
|  [07]   | `ProviderArgs.namespace`                | `Input<string>` — default namespace for un-namespaced resources                                |
|  [08]   | `ProviderArgs.enableServerSideApply`    | `Input<boolean>` — SSA field-manager mode                                                      |
|  [09]   | `ProviderArgs.renderYamlToDirectory`    | `Input<string>` — render-only (no apply) manifest emit                                         |
|  [10]   | `ProviderArgs.helmReleaseSettings`      | `Input<inputs.HelmReleaseSettings>` — helm apply defaults                                      |
|  [11]   | `ProviderArgs.suppressHelmHookWarnings` | hook-warning suppression                                                                       |
|  [12]   | `ProviderArgs.deleteUnreachable`        | unreachable-cluster resource GC                                                                |
|  [13]   | `ProviderArgs.clusterIdentifier`        | replace-identity for cluster reassociation                                                     |

[PROVIDER]: `Provider.isInstance(any) -> obj is Provider` `Provider(string,ProviderArgs?,pulumi.ResourceOptions?)`
[PROVIDER_ARGS]: `ProviderArgs.kubeconfig: pulumi.Input<string|undefined>` `ProviderArgs.context: pulumi.Input<string|undefined>` `ProviderArgs.cluster: pulumi.Input<string|undefined>` `ProviderArgs.namespace: pulumi.Input<string|undefined>` `ProviderArgs.enableServerSideApply: pulumi.Input<boolean|undefined>` `ProviderArgs.renderYamlToDirectory: pulumi.Input<string|undefined>` `ProviderArgs.helmReleaseSettings: pulumi.Input<inputs.HelmReleaseSettings|undefined>` `ProviderArgs.deleteUnreachable: pulumi.Input<boolean|undefined>` `ProviderArgs.clusterIdentifier: pulumi.Input<string|undefined>`

## [03]-[IMPLEMENTATION_LAW]

[WORKLOAD_TOPOLOGY]:
- Typed resources thread `meta.v1.ObjectMeta` (name/namespace/labels/annotations) and the group's `Spec` shape as `Input`s; the realized object exposes `metadata`/`spec`/`status` as `Output`s. Compose across resources with `Output.apply` and `pulumi.all`, binding a `Service.spec.clusterIP` into an `Ingress` rule. Literal-typed `apiVersion`/`kind` discriminants make `isInstance`/type-narrowing exhaustive.
- Typed classes carry the app's own workloads (`Deployment`/`Service`/`Ingress`/`Secret`); reach `apiextensions.CustomResource` only for operator CRDs with no generated class, and `yaml`/`kustomize` only when adopting an unmodifiable upstream manifest bundle.

[HELM_TOPOLOGY]:
- `helm.v4.Chart` renders server-side and hands Pulumi the manifests, so `values` is the typed configuration surface — the LGTM stack, the OTel collector, and the CNPG operator are each ONE `Chart` with a typed `values` object (no `Pulumi.yaml`, no authored chart YAML). `repositoryOpts.repo` or an `oci://` `chart` ref selects the source; `version` pins it; `namespace` binds to a `core.v1.Namespace`.
- `chart.resources` is the rendered child set — feed it to CrossGuard (`validateStack`) or to `@pulumiverse/grafana` when a chart emits a Grafana whose dashboards this run must populate. Use `helm.v3.Release` ONLY for true release lifecycle; the `Chart` path keeps every resource under Pulumi diff/policy.

[CRD_TOPOLOGY]:
- `apiextensions.CustomResource` is the CNPG `Cluster` owner: `apiVersion: "postgresql.cnpg.io/v1"`, `kind: "Cluster"`, with `spec.instances`/`spec.imageName` (the PG18.4-extension image)/`spec.storage`/`spec.backup` (scheduled-backup + PITR to the object-store row) in the `[field]` catch-all. Install the operator itself via a `helm.v4.Chart`; declare cluster instances via `CustomResource`. `-rw` Service the CNPG operator creates is the host handed to `@pulumi/postgresql`.

[STACK_LAW]:
- SELFHOSTED-K8S ARM: the `provider/dispatch` `Match.exhaustive` `selfhosted-k8s` arm is a `Layer`-composed program — (1) `@pulumi/command` `remote.Command` bootstraps the cluster and emits the kubeconfig; (2) `new kubernetes.Provider({ kubeconfig, enableServerSideApply: true })`; (3) `helm.v4.Chart` installs CNPG operator + LGTM + OTel collector with typed `values`; (4) `apiextensions.CustomResource` declares the CNPG `Cluster`; (5) its host `Output` feeds `@pulumi/postgresql.Provider` (`kube/data` seam); (6) `@pulumi/tls`/`@pulumi/random` cert+key material lands in a `core.v1.Secret` (`type: "kubernetes.io/tls"`, `stringData: { "tls.crt", "tls.key" }` — the TLS-secret sink) that `networking.v1.Ingress.spec.tls[].secretName` references to realize `kube/traffic`; (7) `@pulumiverse/grafana` applies `telemetry/board` dashboards onto the rendered Grafana.
- SUBSTRATE WEAVE: authored inside the arm the `dispatch` selects; run by `program/automation` `LocalWorkspace.createOrSelectStack`. Realized workload `Output`s (service host/port, ingress hostname) project through a `Schema`-decoded `StackOutputs` record — the sole `iac`→`work` value crossing (`ShardingConfig`). `@pulumi/policy` `validateResourceOfType(kubernetes.apps.v1.Deployment, …)` narrows against the classes exported here.

[RAIL_LAW]:
- iac / kubernetes rail; `node`-tier. Plugin shells `helm template` and drives the API server, so this arm runs only where the kubeconfig resolves to a reachable cluster (bootstrap host or in-cluster job). Apply failures ride the Automation-API `diagnostics` event stream folded into the typed run receipt; there is no in-band typed error class — the failure channel is the engine event stream. Chart provenance (`verify`/`keyring`) and SSA field-management (`enableServerSideApply`) are the correctness knobs, not app config.
