# [API_CATALOGUE] @pulumi/kubernetes

`@pulumi/kubernetes` supplies typed resource classes for every Kubernetes API group (`core`, `apps`, `networking`, `batch`, `rbac`, `storage`, `autoscaling`, `apiextensions`, …), a `Provider` resource for kubeconfig and server-side-apply configuration, a Helm `Release` resource backed by an embedded Helm library, and `yaml.ConfigFile` / `yaml.ConfigGroup` for manifest-driven deployment — all integrated with the `@pulumi/pulumi` `Output<T>` / `Input<T>` algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/kubernetes`
- package: `@pulumi/kubernetes`
- module: `@pulumi/kubernetes` (root), `@pulumi/kubernetes/helm/v3`, `@pulumi/kubernetes/yaml`
- asset: Kubernetes resource classes, Provider, Helm Release, yaml ConfigFile/ConfigGroup
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider family
- rail: deployment

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [RAIL]                                          |
| :-----: | :------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Provider`     | resource class | kubeconfig + server-side-apply configuration    |
|  [02]   | `ProviderArgs` | args interface | `kubeconfig`, `context`, `namespace`, SSA flags |

[PUBLIC_TYPE_SCOPE]: core/v1 resource family
- rail: deployment

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :------------------------------ | :------------- | :------------------------------ |
|  [01]   | `core.v1.Pod`                   | resource class | pod provisioning with readiness |
|  [02]   | `core.v1.Service`               | resource class | service provisioning            |
|  [03]   | `core.v1.ConfigMap`             | resource class | config map                      |
|  [04]   | `core.v1.Secret`                | resource class | secret                          |
|  [05]   | `core.v1.Namespace`             | resource class | namespace                       |
|  [06]   | `core.v1.ServiceAccount`        | resource class | service account                 |
|  [07]   | `core.v1.PersistentVolumeClaim` | resource class | PVC                             |
|  [08]   | `core.v1.PersistentVolume`      | resource class | PV                              |

[PUBLIC_TYPE_SCOPE]: apps/v1 resource family
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                            |
| :-----: | :-------------------- | :------------- | :-------------------------------- |
|  [01]   | `apps.v1.Deployment`  | resource class | deployment with rollout readiness |
|  [02]   | `apps.v1.StatefulSet` | resource class | stateful set                      |
|  [03]   | `apps.v1.DaemonSet`   | resource class | daemon set                        |
|  [04]   | `apps.v1.ReplicaSet`  | resource class | replica set                       |

[PUBLIC_TYPE_SCOPE]: networking, batch, and rbac resource families
- rail: deployment

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]               |
| :-----: | :--------------------------- | :------------- | :------------------- |
|  [01]   | `networking.v1.Ingress`      | resource class | ingress rule         |
|  [02]   | `networking.v1.IngressClass` | resource class | ingress class        |
|  [03]   | `batch.v1.Job`               | resource class | batch job            |
|  [04]   | `batch.v1.CronJob`           | resource class | cron job             |
|  [05]   | `rbac.v1.Role`               | resource class | namespaced RBAC role |
|  [06]   | `rbac.v1.ClusterRole`        | resource class | cluster-wide role    |
|  [07]   | `rbac.v1.RoleBinding`        | resource class | role binding         |
|  [08]   | `rbac.v1.ClusterRoleBinding` | resource class | cluster role binding |

[PUBLIC_TYPE_SCOPE]: Helm v3 family
- rail: deployment
- module: `@pulumi/kubernetes/helm/v3`

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [RAIL]                                                                                                                          |
| :-----: | :------------ | :------------- | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Release`     | resource class | Helm chart deployment via embedded library                                                                                      |
|  [02]   | `ReleaseArgs` | args interface | `chart`, `version`, `namespace`, `values`, `repositoryOpts`, `atomic`, `createNamespace`, `timeout`, `skipAwait`, `waitForJobs` |

[PUBLIC_TYPE_SCOPE]: yaml manifest family
- rail: deployment
- module: `@pulumi/kubernetes/yaml`

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]      | [RAIL]                                    |
| :-----: | :--------------------------------- | :----------------- | :---------------------------------------- |
|  [01]   | `yaml.ConfigFile`                  | component resource | apply resources from a YAML manifest file |
|  [02]   | `yaml.ConfigGroup`                 | component resource | apply resources from multiple manifests   |
|  [03]   | `yaml.CollectionComponentResource` | abstract class     | base for ConfigFile/ConfigGroup           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource constructors
- rail: deployment

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `new Provider(name, args?, opts?)`              | provider init   | kubeconfig and SSA policy       |
|  [02]   | `new core.v1.Pod(name, args?, opts?)`           | resource create | pod with readiness wait         |
|  [03]   | `new apps.v1.Deployment(name, args?, opts?)`    | resource create | deployment with rollout wait    |
|  [04]   | `new apps.v1.StatefulSet(name, args?, opts?)`   | resource create | stateful set                    |
|  [05]   | `new networking.v1.Ingress(name, args?, opts?)` | resource create | ingress                         |
|  [06]   | `new batch.v1.Job(name, args?, opts?)`          | resource create | job                             |
|  [07]   | `new rbac.v1.ClusterRole(name, args?, opts?)`   | resource create | cluster role                    |
|  [08]   | `Resource.get(name, id, opts?)`                 | static lookup   | adopt existing cluster resource |

[ENTRYPOINT_SCOPE]: Helm Release and yaml entrypoints
- rail: deployment

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------------------------ | :------------- | :-------------------------------------------- |
|  [01]   | `new helm.v3.Release(name, args?, opts?)`   | chart deploy   | embedded Helm with await semantics            |
|  [02]   | `new yaml.ConfigFile(name, config?, opts?)` | manifest apply | resources from a single YAML file             |
|  [03]   | `new yaml.ConfigGroup(name, config, opts?)` | manifest apply | resources from multiple YAML files or strings |

## [04]-[IMPLEMENTATION_LAW]

[KUBERNETES_TOPOLOGY]:
- namespace: `@pulumi/kubernetes`; top-level namespaces mirror Kubernetes API groups (`core`, `apps`, `networking`, `batch`, `rbac`, `storage`, `autoscaling`, `apiextensions`, `scheduling`, `settings`, `meta`, `yaml`, `helm`, `kustomize`)
- all resource classes extend `pulumi.CustomResource`; inputs accept `Input<T>`, outputs emit `Output<T>`
- readiness: Pod, Deployment, and StatefulSet resources block Pulumi completion until the resource reaches ready state; set `customTimeouts` in `ResourceOptions` to override the 10-minute default
- server-side apply: enabled by default in `Provider`; disable with `enableServerSideApply: false`; `enablePatchForce` resolves field conflicts in SSA mode
- Helm `Release` uses the embedded Helm library; `skipAwait: false` (default) blocks until all chart resources are ready; `atomic: true` purges on failure
- `yaml.ConfigFile` and `yaml.ConfigGroup` emit a `resources` output map (`Output<{ [key: string]: CustomResource }>`) for downstream `dependsOn` wiring

[LOCAL_ADMISSION]:
- One `Provider` resource per cluster; pass as `provider` in `ResourceOptions` for multi-cluster stacks.
- `ReleaseArgs.chart` accepts a chart name (resolved from `repositoryOpts.repo`) or a local path.
- `ProviderArgs.kubeconfig` accepts the kubeconfig file contents or path; use `pulumi.Config.requireSecret` to keep credentials out of state.

[RAIL_LAW]:
- Package: `@pulumi/kubernetes`
- Owns: Kubernetes resource lifecycle, Helm chart deployment, manifest-driven deployment
- Accept: `Input<T>` for all args; `Output<T>` on all output properties
- Reject: hand-rolled kubectl calls or raw Kubernetes REST from within Pulumi programs; use resource classes instead
