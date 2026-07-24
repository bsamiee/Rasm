# [TS_IAC_API_PULUMI_KUBERNETES]

`@pulumi/kubernetes` drives the `selfhosted-k8s` arm and owns the `iac` workload, data, and traffic spine.

Two shapes cover the package: the generated typed resource (`apiVersion`/`kind`/`metadata`/`spec`/`status` as `Output`s) and the bespoke `ComponentResource` (`helm.v4.Chart`, `helm.v3.Release`, `kustomize.Directory`, `yaml.ConfigGroup`); typed charts install operators with zero authored YAML, `apiextensions.CustomResource` carries operator CRDs, and `Provider` binds the kubeconfig.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/kubernetes`
- package: `@pulumi/kubernetes` (Apache-2.0)
- module: `@pulumi/kubernetes` + group sub-paths (`/apps/v1`, `/helm/v4`, `/apiextensions`, `/kustomize`, `/yaml`)
- runtime: `node` — Automation-API program process
- asset: typed resource classes for every API group, `helm.v4.Chart`/`helm.v3.Release`, `apiextensions.CustomResource`/`CustomResourcePatch`/`v1.CustomResourceDefinition`, `kustomize.Directory`, `yaml.{ConfigFile,ConfigGroup}`, `Provider`, the `types.input`/`types.output` shape namespaces
- rail: iac / kubernetes

## [02]-[PUBLIC_TYPES]

### [02.1]-[TYPED_RESOURCE_PATTERN]

[PUBLIC_TYPE_SCOPE]: typed resource — every API-group kind
- entry: `@pulumi/kubernetes/<group>/<version>`

Every typed resource is `class Kind extends pulumi.CustomResource` with literal-typed `apiVersion`/`kind` discriminants and `metadata`/`spec`/`status` as `Output`s of the group's `types.output` shapes; `KindArgs` mirrors them as `types.input`. Server state is the source of truth, so the typed-resource `.get` is 3-arg with no client-side `State` bag, and the group roster is data threaded through this one pattern.

| [INDEX] | [SURFACE]                             | [SHAPE]   | [CAPABILITY]                                              |
| :-----: | :------------------------------------ | :-------- | :-------------------------------------------------------- |
|  [01]   | `class Kind`                          | class     | `extends pulumi.CustomResource`; literal discriminants    |
|  [02]   | `metadata`/`spec`/`status`            | property  | `Output`s of the group `types.output` shapes              |
|  [03]   | `Kind(name, args?, opts?)`            | ctor      | construct against the cluster                             |
|  [04]   | `Kind.get(name, id, opts?) -> Kind`   | static    | adopt by id; 3-arg, no client `State`                     |
|  [05]   | `Kind.isInstance(obj) -> obj is Kind` | static    | runtime type guard                                        |
|  [06]   | `interface KindArgs`                  | interface | `metadata`/`spec` `Input`s; literal `apiVersion?`/`kind?` |
|  [07]   | `<Kind>Patch`                         | class     | Server-Side-Apply patch twin                              |
|  [08]   | `<Kind>List`                          | class     | the list-kind resource                                    |

### [02.2]-[API_GROUP_ROSTER]

[PUBLIC_TYPE_SCOPE]: typed resource groups

| [INDEX] | [GROUP]                        | [KEY_KINDS]                                                              |
| :-----: | :----------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `core.v1`                      | `Namespace` `Service` `Secret` `ConfigMap` `PersistentVolumeClaim` `Pod` |
|  [02]   | `apps.v1`                      | `Deployment` `StatefulSet` `DaemonSet` `ReplicaSet`                      |
|  [03]   | `batch.v1`                     | `Job` `CronJob`                                                          |
|  [04]   | `networking.v1`                | `Ingress` `IngressClass` `NetworkPolicy`                                 |
|  [05]   | `rbac.authorization.k8s.io/v1` | `Role` `ClusterRole` `RoleBinding` `ClusterRoleBinding`                  |
|  [06]   | `storage.k8s.io/v1`            | `StorageClass` `VolumeAttachment`                                        |
|  [07]   | `apiextensions.k8s.io/v1`      | `CustomResourceDefinition`                                               |

Every other generated API group folds the same pattern; each kind exposes its `*Patch` (SSA) and `*List` twins, and `meta.v1.ObjectMeta` is the shared metadata `Input` carrying `name`/`namespace`/`labels`/`annotations`.

### [02.3]-[HELM_V4_CHART]

[PUBLIC_TYPE_SCOPE]: helm component
- entry: `@pulumi/kubernetes/helm/v4`

`helm.v4.Chart` renders server-side — `helm template --dry-run=server` then a Pulumi-managed apply — so every rendered manifest stays under Pulumi diff, transform, and CrossGuard policy with no Tiller/Release state. `values` is the typed object map (literals, nested maps, `Output`s, `pulumi.asset.Asset`s) that supplies the LGTM/CNPG/OTel configuration with zero authored YAML.

| [INDEX] | [SURFACE]                               | [SHAPE]   | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------- | :-------- | :------------------------------------------------------------ |
|  [01]   | `class Chart`                           | component | `extends pulumi.ComponentResource`; `(name, args?, opts?)`    |
|  [02]   | `Chart.isInstance(obj) -> obj is Chart` | static    | runtime type guard                                            |
|  [03]   | `chart.resources`                       | property  | `Output<any[]>` rendered child resources                      |
|  [04]   | `chart`                                 | arg       | ref/path/tgz/URL/`oci://` chart source                        |
|  [05]   | `values`                                | arg       | highest-precedence typed value map                            |
|  [06]   | `valueYamlFiles`                        | arg       | `values.yaml` override assets                                 |
|  [07]   | `repositoryOpts`                        | arg       | chart-repo auth: creds + `caFile`/`certFile`/`keyFile` assets |
|  [08]   | `version`                               | arg       | pinned chart selection                                        |
|  [09]   | `devel`                                 | arg       | pre-release chart admission                                   |
|  [10]   | `namespace`                             | arg       | release namespace                                             |
|  [11]   | `skipCrds`                              | arg       | omit chart CRD objects                                        |
|  [12]   | `skipAwait`                             | arg       | do not block on readiness                                     |
|  [13]   | `includeHooks`                          | arg       | emit `helm.sh/hook` resources                                 |
|  [14]   | `postRenderer`                          | arg       | `{ command, args }` post-render hook                          |
|  [15]   | `dependencyUpdate`                      | arg       | chart dependency rebuild                                      |
|  [16]   | `verify`                                | arg       | chart provenance verification                                 |
|  [17]   | `keyring`                               | arg       | provenance keyring asset                                      |
|  [18]   | `plainHttp`                             | arg       | insecure HTTP fetch                                           |
|  [19]   | `resourcePrefix`                        | arg       | rendered-resource name prefix                                 |
|  [20]   | `name`                                  | arg       | release name override                                         |

`helm.v3.Release` is the stateful `helm install` peer — reach it only when a chart needs true release lifecycle (`atomic`, `waitForJobs`, `recreatePods`, `timeout`, rollback); the `Chart` path is the default for Pulumi-managed resources, keeping every object under diff and policy.

### [02.4]-[APIEXTENSIONS_CUSTOMRESOURCE]

[PUBLIC_TYPE_SCOPE]: operator CRD carrier
- entry: `@pulumi/kubernetes/apiextensions`

`CustomResource` instantiates any CRD instance — the CNPG `Cluster`, cert-manager `Certificate`, Grafana `GrafanaDashboard` — when no generated class exists; `CustomResourceArgs` requires only `apiVersion`/`kind` and carries a `[field]: Input<any>` catch-all for the operator `spec`. Pair it with `apiextensions.v1.CustomResourceDefinition` to install the CRD schema and `CustomResourcePatch` for SSA into an operator-owned object.

| [INDEX] | [SURFACE]                                   | [SHAPE]   | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------ | :-------- | :------------------------------------------------------------------- |
|  [01]   | `class CustomResource`                      | component | `extends pulumi.CustomResource`; `(name, args, opts?)`               |
|  [02]   | `CustomResource.get(name, opts)`            | static    | id via `{ apiVersion, kind, id, namespace? }`                        |
|  [03]   | `apiVersion`/`kind`/`metadata`              | property  | string-typed `Output`s, not literal discriminants                    |
|  [04]   | `getInputs() -> CustomResourceArgs`         | instance  | recover the declared spec                                            |
|  [05]   | `interface CustomResourceArgs`              | interface | required `apiVersion`/`kind`; `[field]: Input<any>` catch-all        |
|  [06]   | `class CustomResourcePatch`                 | class     | SSA patch twin over an existing CRD instance                         |
|  [07]   | `apiextensions.v1.CustomResourceDefinition` | class     | typed CRD-install resource (`spec.group`/`names`/`versions`/`scope`) |

### [02.5]-[MANIFEST_COMPONENTS_AND_PROVIDER]

[PUBLIC_TYPE_SCOPE]: manifest components + cluster binding

| [INDEX] | [SURFACE]                                     | [SHAPE]   | [CAPABILITY]                                                           |
| :-----: | :-------------------------------------------- | :-------- | :--------------------------------------------------------------------- |
|  [01]   | `yaml.ConfigFile`/`ConfigGroup`               | component | per file/glob/literal manifest set; `transformations`/`resourcePrefix` |
|  [02]   | `yaml.v2.ConfigGroup`                         | component | SSA-aware manifest component                                           |
|  [03]   | `kustomize.Directory`                         | component | `kustomize build` over dir/URL; `kustomize.v2.Directory` twin          |
|  [04]   | `class Provider`                              | component | `extends pulumi.ProviderResource`; `(name, args?, opts?)`              |
|  [05]   | `Provider.isInstance(obj) -> obj is Provider` | static    | runtime type guard                                                     |
|  [06]   | `ProviderArgs.kubeconfig`                     | arg       | cluster kubeconfig contents or path                                    |
|  [07]   | `ProviderArgs.context`/`cluster`              | arg       | kubeconfig context / cluster selection                                 |
|  [08]   | `ProviderArgs.namespace`                      | arg       | default namespace for un-namespaced resources                          |
|  [09]   | `ProviderArgs.enableServerSideApply`          | arg       | SSA field-manager mode                                                 |
|  [10]   | `ProviderArgs.renderYamlToDirectory`          | arg       | render-only manifest emit, no apply                                    |
|  [11]   | `ProviderArgs.helmReleaseSettings`            | arg       | helm apply defaults                                                    |
|  [12]   | `ProviderArgs.suppressHelmHookWarnings`       | arg       | hook-warning suppression                                               |
|  [13]   | `ProviderArgs.deleteUnreachable`              | arg       | unreachable-cluster resource GC                                        |
|  [14]   | `ProviderArgs.clusterIdentifier`              | arg       | replace-identity for cluster reassociation                             |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Typed resources thread `meta.v1.ObjectMeta` and the group `Spec` as `Input`s and expose `metadata`/`spec`/`status` as `Output`s; compose across resources with `Output.apply`/`pulumi.all`, and the literal `apiVersion`/`kind` discriminants make `isInstance` narrowing exhaustive.
- Typed classes carry the app's own workloads; reach `apiextensions.CustomResource` only for operator CRDs with no generated class, and `yaml`/`kustomize` only to adopt an unmodifiable upstream manifest bundle.
- `helm.v4.Chart` renders server-side and hands Pulumi the manifests, so `values` is the whole configuration surface — the LGTM stack, OTel collector, and CNPG operator are each one `Chart` with a typed `values` object; `repositoryOpts.repo` or an `oci://` `chart` ref selects the source, `version` pins it, `namespace` binds a `core.v1.Namespace`, and `chart.resources` feeds CrossGuard or `@pulumiverse/grafana`.
- `apiextensions.CustomResource` owns the CNPG `Cluster` (`apiVersion: "postgresql.cnpg.io/v1"`, `kind: "Cluster"`) with `spec.instances`/`spec.imageName`/`spec.storage`/`spec.backup` in the catch-all; a `helm.v4.Chart` installs the operator, `CustomResource` declares the cluster, and the `-rw` Service it creates is the Postgres host.

[STACKING]:
- `pulumi-command`(`.api/pulumi-command.md`): `remote.Command` bootstraps the cluster and its stdout `Output` is the kubeconfig fed to `Provider.kubeconfig`.
- `pulumi-postgresql`(`.api/pulumi-postgresql.md`): the CNPG `Cluster` `-rw` Service host `Output` feeds `postgresql.Provider` — the `kube/data` seam.
- `pulumi-tls`/`pulumi-random`(`.api/pulumi-tls.md`): generated cert and key land in a `core.v1.Secret` (`type: "kubernetes.io/tls"`) that `networking.v1.Ingress.spec.tls[].secretName` references — the `kube/traffic` seam.
- `pulumiverse-grafana`(`.api/pulumiverse-grafana.md`): applies `telemetry/board` dashboards onto the Grafana a `helm.v4.Chart` renders.
- `pulumi-policy`(`.api/pulumi-policy.md`): `validateResourceOfType(kubernetes.apps.v1.Deployment, …)` narrows against the resource classes exported here.
- within-lib: the arm runs under `program/automation` `LocalWorkspace.createOrSelectStack`, and realized workload `Output`s (service host, ingress hostname) project through the `StackOutputs` record — the `iac`→`work` crossing (`ShardingConfig`).

[LOCAL_ADMISSION]:
- `pulumi-resource-kubernetes` shells `helm template` and drives the API server, so the arm admits only where `helm` and `kubectl` are reachable and the kubeconfig resolves to a live cluster (bootstrap host or in-cluster job).

[RAIL_LAW]:
- Package: `@pulumi/kubernetes`
- Owns: typed resources for every API group, `helm.v4.Chart` render, the `apiextensions.CustomResource` CRD carrier, `yaml`/`kustomize` manifest components, the cluster `Provider`
- Accept: typed `values` maps for chart config; `enableServerSideApply` field-management; chart provenance (`verify`/`keyring`) as correctness controls distinct from the `values` app config
- Reject: authored chart or manifest YAML the typed `values` and resource classes replace; a hand-rolled kubeconfig-context switch the `Provider` owns
