# [TS_IAC_API_PULUMI_KUBERNETES]

`@pulumi/kubernetes` is the first-class engine of the `selfhosted-k8s` arm: strongly-typed resource classes for every Kubernetes API group (`core/v1`, `apps/v1`, `batch/v1`, `networking/v1`, `rbac/v1`, `storage/v1`, `apiextensions/v1`, …), the `helm.v4.Chart` / `helm.v3.Release` component wrappers that render upstream charts as typed value objects, the `apiextensions.CustomResource` carrier for operator CRDs (the CNPG `Cluster`, cert-manager `Certificate`, Prometheus `ServiceMonitor`), the `yaml`/`kustomize` manifest components, and the `Provider` that binds a kubeconfig. Two shapes cover the whole package: the generated TYPED resource (`apiVersion`/`kind`/`metadata`/`spec`/`status` as `Output`s) and the bespoke COMPONENT (`Chart`/`Release`/`Directory`/`ConfigGroup` over `pulumi.ComponentResource`). In `iac` this is the workload/data/traffic spine — the cluster-bootstrap row (`@pulumi/command`) yields the kubeconfig, `helm.v4.Chart` installs the CNPG operator + LGTM stack + OTel collector as typed values (zero authored YAML), `apiextensions.CustomResource` declares the CNPG cluster whose host feeds `@pulumi/postgresql`, and `@pulumiverse/grafana` applies dashboards onto the rendered Grafana.

```ts
// @pulumi/kubernetes — Provider + API-group namespaces + component wrappers
export { Provider }                        // pulumi.ProviderResource (kubeconfig binding)
export {                                    // typed resource groups (each with v1/v2/… sub-namespaces)
  core, apps, batch, networking, rbac, storage, policy, admissionregistration,
  apiregistration, autoscaling, certificates, coordination, discovery, events,
  flowcontrol, node, scheduling, settings, meta,
  apiextensions,                            // CustomResource, CustomResourcePatch, v1.CustomResourceDefinition
  helm,                                     // helm.v4.Chart, helm.v3.Release
  kustomize, yaml,                          // kustomize.Directory, yaml.{ConfigFile,ConfigGroup}
  types,                                    // types.input / types.output nested shape namespaces
}
```

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/kubernetes`
- package: `@pulumi/kubernetes`
- license: `Apache-2.0`
- build-floor: peer `@pulumi/pulumi ^catalog`; bundles `glob`, `shell-quote` (chart/kustomize shell-out); the `pulumi-resource-kubernetes` plugin needs `helm` + `kubectl` reachable for chart render and SSA
- target: `node` (Automation-API program process; the plugin shells `helm template` and talks to the API server)
- entry: `@pulumi/kubernetes` plus the group sub-paths (`@pulumi/kubernetes/apps/v1`, `/helm/v4`, `/apiextensions`, …)
- asset: the typed resource classes for all API groups, `helm.v4.Chart` / `helm.v3.Release`, `apiextensions.CustomResource`/`CustomResourcePatch`/`v1.CustomResourceDefinition`, `kustomize.Directory`, `yaml.{ConfigFile,ConfigGroup}`, the `Provider`, and the `types.input`/`types.output` shape namespaces
- rail: iac / kubernetes

## [02]-[PUBLIC_TYPES]

### the typed resource pattern — every API-group resource

[PUBLIC_TYPE_SCOPE]: typed resource
- rail: iac / kubernetes
- entry: `@pulumi/kubernetes/<group>/<version>`

Every typed resource is `class Kind extends pulumi.CustomResource` with literal-typed `apiVersion`/`kind` discriminants and `metadata`/`spec`/`status` as `Output`s of the group's `types.output` shapes. `KindArgs` mirrors them as `types.input`. Unlike the cloud SDKs, the typed-resource `.get` is 3-arg (`name`, `id`, `opts?`) — server state is the source of truth, so there is no client-side `State` bag. Learn it once; the group roster is data.

| [INDEX] | [MEMBER] | [SHAPE] |
|:-----: |:----------------- |:-------------------------------------------------------------------------------------------- |
| [01] | `class Kind` | `extends pulumi.CustomResource`; `apiVersion: Output<"apps/v1">`, `kind: Output<"Deployment">` literal discriminants |
| [02] | attributes | `metadata: Output<meta.v1.ObjectMeta>`, `spec: Output<…Spec>`, `status: Output<…Status>` |
| [03] | `constructor` | `(name, args?: KindArgs, opts?: pulumi.CustomResourceOptions)` |
| [04] | `Kind.get` | `static get(name, id: pulumi.Input<pulumi.ID>, opts?): Kind` — 3-arg (no client State) |
| [05] | `Kind.isInstance` | `static isInstance(obj): obj is Kind` |
| [06] | `interface KindArgs`| `metadata?: Input<meta.v1.ObjectMeta>`, `spec?: Input<…Spec>`, literal `apiVersion?`/`kind?` |
| [07] | `KindPatch` twin | `<group>.<v>.<Kind>Patch` — Server-Side-Apply patch variant of every resource |
| [08] | `KindList` twin | `<group>.<v>.<Kind>List` — the list-kind resource |

```ts contract
import * as pulumi from "@pulumi/pulumi"
import * as inputs from "../../types/input"
import * as outputs from "../../types/output"

// Canonical shape, shown on apps/v1.Deployment. core/v1.Namespace, core/v1.Service,
// core/v1.Secret, batch/v1.Job, networking/v1.Ingress, … are this exact structure.
declare class Deployment extends pulumi.CustomResource {
  static get(name: string, id: pulumi.Input<pulumi.ID>, opts?: pulumi.CustomResourceOptions): Deployment
  static isInstance(obj: any): obj is Deployment
  readonly apiVersion: pulumi.Output<"apps/v1">
  readonly kind: pulumi.Output<"Deployment">
  readonly metadata: pulumi.Output<outputs.meta.v1.ObjectMeta>
  readonly spec: pulumi.Output<outputs.apps.v1.DeploymentSpec>
  readonly status: pulumi.Output<outputs.apps.v1.DeploymentStatus>
  constructor(name: string, args?: DeploymentArgs, opts?: pulumi.CustomResourceOptions)
}
interface DeploymentArgs {
  readonly apiVersion?: pulumi.Input<"apps/v1">
  readonly kind?: pulumi.Input<"Deployment">
  readonly metadata?: pulumi.Input<inputs.meta.v1.ObjectMeta | undefined>
  readonly spec?: pulumi.Input<inputs.apps.v1.DeploymentSpec | undefined>
}
```

### API-group roster — the DATA fed to the typed pattern

[PUBLIC_TYPE_SCOPE]: typed resource groups
- rail: iac / kubernetes

| [INDEX] | [GROUP] | [KEY_KINDS_THE_KUBE_ROWS_COMPOSE] |
|:-----: |:---------------------------- |:----------------------------------------------------------------------------------------- |
| [01] | `core.v1` | `Namespace`, `Service`, `Secret`, `ConfigMap`, `PersistentVolumeClaim`, `ServiceAccount`, `Pod` |
| [02] | `apps.v1` | `Deployment`, `StatefulSet`, `DaemonSet`, `ReplicaSet` |
| [03] | `batch.v1` | `Job`, `CronJob` |
| [04] | `networking.v1` | `Ingress`, `IngressClass`, `NetworkPolicy` (the `kube/traffic` rows) |
| [05] | `rbac.authorization.k8s.io/v1`| `Role`, `ClusterRole`, `RoleBinding`, `ClusterRoleBinding` |
| [06] | `storage.k8s.io/v1` | `StorageClass`, `VolumeAttachment` |
| [07] | `apiextensions.k8s.io/v1` | `CustomResourceDefinition` (`apiextensions.v1.CustomResourceDefinition`) |
| [08] | `policy`, `autoscaling`, `coordination`, `scheduling`, `admissionregistration`, `certificates`, `discovery`, `events`, `flowcontrol`, `node`, `settings`, `apiregistration`, `storagemigration` | the remaining generated groups, same pattern |

Every group additionally exposes `*Patch` (SSA) and `*List` twins; `meta.v1.ObjectMeta` is the shared metadata input carrying `name`/`namespace`/`labels`/`annotations`.

### `helm.v4.Chart` — upstream charts as typed value objects

[PUBLIC_TYPE_SCOPE]: helm component
- rail: iac / observe · data
- entry: `@pulumi/kubernetes/helm/v4`

`Chart` is a `ComponentResource` equivalent to `helm template --dry-run=server` followed by Pulumi-managed apply of the rendered manifests — so Pulumi transformations and CrossGuard policies see every rendered resource, and there is no Tiller/Release state. `values` is a typed object map (literals, nested maps, `Output`s, and `pulumi.asset.Asset`s), which is HOW `iac` supplies the LGTM/CNPG/OTel configuration with zero authored YAML.

| [INDEX] | [MEMBER] | [SIGNATURE_FIELD] |
|:-----: |:---------------------- |:------------------------------------------------------------------------------------ |
| [01] | `class Chart` | `extends pulumi.ComponentResource`; `constructor(name, args?: ChartArgs, opts?: pulumi.ComponentResourceOptions)` |
| [02] | `Chart.isInstance` | `static isInstance(obj): obj is Chart` |
| [03] | `chart.resources` | `readonly resources: pulumi.Output<any[]>` — the rendered child resources |
| [04] | `chart` (arg) | `Input<string>` — ref (`repo/name`), path, tgz, URL, or `oci://…` |
| [05] | `values` | `Input<{ [k: string]: any }>` — highest-precedence typed value map |
| [06] | `valueYamlFiles` | `Input<Input<pulumi.asset.Asset \| pulumi.asset.Archive>[]>` — `values.yaml` assets |
| [07] | `repositoryOpts` | `Input<inputs.helm.v4.RepositoryOpts>` — `{ repo, username, password, caFile, certFile, keyFile }` |
| [08] | `version` / `devel` | `Input<string>` pinned chart / pre-release admission |
| [09] | `namespace` | `Input<string>` — release namespace (bind to a `core.v1.Namespace.metadata.name`) |
| [10] | `skipCrds` / `skipAwait`| `Input<boolean>` — omit chart CRDs / do not block on readiness |
| [11] | `postRenderer` | `Input<inputs.helm.v4.PostRenderer>` — `{ command, args }` kustomize/post-render hook |
| [12] | `dependencyUpdate` / `verify` / `keyring` / `plainHttp` / `resourcePrefix` / `name` | dep rebuild, provenance verify, keyring asset, insecure HTTP, name prefix, release name |

```ts contract
import * as pulumi from "@pulumi/pulumi"
import * as inputs from "../../types/input"

declare class Chart extends pulumi.ComponentResource {
  static isInstance(obj: any): obj is Chart
  readonly resources: pulumi.Output<any[]>
  constructor(name: string, args?: ChartArgs, opts?: pulumi.ComponentResourceOptions)
}
interface ChartArgs {
  readonly chart: pulumi.Input<string>
  readonly values?: pulumi.Input<{ [key: string]: any } | undefined>
  readonly valueYamlFiles?: pulumi.Input<pulumi.Input<pulumi.asset.Asset | pulumi.asset.Archive>[] | undefined>
  readonly repositoryOpts?: pulumi.Input<inputs.helm.v4.RepositoryOpts | undefined>
  readonly version?: pulumi.Input<string | undefined>
  readonly devel?: pulumi.Input<boolean | undefined>
  readonly namespace?: pulumi.Input<string | undefined>
  readonly skipCrds?: pulumi.Input<boolean | undefined>
  readonly skipAwait?: pulumi.Input<boolean | undefined>
  readonly postRenderer?: pulumi.Input<inputs.helm.v4.PostRenderer | undefined>
  readonly dependencyUpdate?: pulumi.Input<boolean | undefined>
  readonly verify?: pulumi.Input<boolean | undefined>
  readonly keyring?: pulumi.Input<pulumi.asset.Asset | pulumi.asset.Archive | undefined>
  readonly plainHttp?: pulumi.Input<boolean | undefined>
  readonly resourcePrefix?: pulumi.Input<string | undefined>
  readonly name?: pulumi.Input<string | undefined>
}
// inputs.helm.v4
interface RepositoryOpts {
  readonly repo?: pulumi.Input<string | undefined>
  readonly username?: pulumi.Input<string | undefined>; readonly password?: pulumi.Input<string | undefined>
  readonly caFile?: pulumi.Input<pulumi.asset.Asset | pulumi.asset.Archive | undefined>
  readonly certFile?: pulumi.Input<pulumi.asset.Asset | pulumi.asset.Archive | undefined>
  readonly keyFile?: pulumi.Input<pulumi.asset.Asset | pulumi.asset.Archive | undefined>
}
interface PostRenderer { readonly command: pulumi.Input<string>; readonly args?: pulumi.Input<pulumi.Input<string>[] | undefined> }
```

`helm.v3` exposes `Release` only (no `v3.Chart` in this release line) — the stateful `helm install` release where Pulumi drives the Helm SDK directly, with `atomic` / `skipAwait` / `waitForJobs` / `timeout` / `recreatePods` / `skipCrds` lifecycle knobs. Prefer `helm.v4.Chart` for Pulumi-managed resources (policy/transform visibility over every rendered object); reach `helm.v3.Release` only when a chart requires true release lifecycle (hooks, rollback, atomic install).

### `apiextensions.CustomResource` — the operator-CRD carrier

[PUBLIC_TYPE_SCOPE]: custom resource
- rail: iac / data
- entry: `@pulumi/kubernetes/apiextensions`

`CustomResource` instantiates any CRD instance (the CNPG `Cluster`, cert-manager `Certificate`, Grafana `GrafanaDashboard`) when no generated class exists. `CustomResourceArgs` is deliberately loose — only `apiVersion`/`kind` are required, with a `[field: string]: pulumi.Input<any>` catch-all for the operator's `spec`. Pair with `apiextensions.v1.CustomResourceDefinition` to install the CRD schema, and `CustomResourcePatch` for SSA into an operator-owned object.

| [INDEX] | [MEMBER] | [SIGNATURE_FIELD] |
|:-----: |:------------------------------------------ |:------------------------------------------------------------------------------ |
| [01] | `class CustomResource` | `extends pulumi.CustomResource`; `constructor(name, args: CustomResourceArgs, opts?: pulumi.CustomResourceOptions)` |
| [02] | `CustomResource.get` | `static get(name, opts: CustomResourceGetOptions): CustomResource` — id via `{ apiVersion, kind, id, namespace? }` |
| [03] | attributes | `apiVersion: Output<string>`, `kind: Output<string>`, `metadata: Output<meta.v1.ObjectMeta>` |
| [04] | `getInputs()` | `(): CustomResourceArgs` — recover the declared spec |
| [05] | `interface CustomResourceArgs` | `apiVersion: string; kind: string; metadata?: Input<meta.v1.ObjectMeta>; [field]: Input<any>` |
| [06] | `class CustomResourcePatch` | SSA patch twin over an existing CRD instance |
| [07] | `apiextensions.v1.CustomResourceDefinition` | the typed CRD-install resource (`spec.group`/`names`/`versions`/`scope`) |

```ts contract
import * as pulumi from "@pulumi/pulumi"
import * as inputs from "../types/input"
import * as outputs from "../types/output"

declare class CustomResource extends pulumi.CustomResource {
  static get(name: string, opts: CustomResourceGetOptions): CustomResource
  readonly apiVersion: pulumi.Output<string>
  readonly kind: pulumi.Output<string>
  readonly metadata: pulumi.Output<outputs.meta.v1.ObjectMeta>
  getInputs(): CustomResourceArgs
  constructor(name: string, args: CustomResourceArgs, opts?: pulumi.CustomResourceOptions)
}
interface CustomResourceArgs {
  apiVersion: string                                  // "postgresql.cnpg.io/v1"
  kind: string                                        // "Cluster"
  metadata?: pulumi.Input<inputs.meta.v1.ObjectMeta>
  [othersFields: string]: pulumi.Input<any>           // spec.instances, spec.imageName, spec.storage, spec.backup, …
}
```

### `yaml` / `kustomize` / `Provider` — manifest components and binding

[PUBLIC_TYPE_SCOPE]: manifest components + provider
- rail: iac / kubernetes

| [INDEX] | [MEMBER] | [SIGNATURE_FIELD] |
|:-----: |:------------------------------ |:--------------------------------------------------------------------------------------- |
| [01] | `yaml.ConfigFile` / `ConfigGroup` | `ComponentResource` rendering one file / a glob or literal set of manifests; `transformations`/`resourcePrefix` |
| [02] | `yaml.v2.ConfigGroup` | the catalog-bound SSA-aware manifest component |
| [03] | `kustomize.Directory` | `ComponentResource` running `kustomize build` over a dir/URL; `kustomize.v2.Directory` twin |
| [04] | `class Provider` | `extends pulumi.ProviderResource`; `constructor(name, args?: ProviderArgs, opts?: pulumi.ResourceOptions)` |
| [05] | `ProviderArgs.kubeconfig` | `Input<string>` — the cluster-bootstrap kubeconfig (`@pulumi/command` output) |
| [06] | `ProviderArgs.context`/`cluster`| `Input<string>` — kubeconfig context / cluster selection |
| [07] | `ProviderArgs.namespace` | `Input<string>` — default namespace for un-namespaced resources |
| [08] | `ProviderArgs.enableServerSideApply` | `Input<boolean>` — SSA field-manager mode |
| [09] | `ProviderArgs.renderYamlToDirectory` | `Input<string>` — render-only (no apply) manifest emit |
| [10] | `ProviderArgs.helmReleaseSettings` / `suppressHelmHookWarnings` / `deleteUnreachable` / `clusterIdentifier` | helm defaults, hook-warning suppression, unreachable-cluster GC, replace-identity |

```ts contract
import * as pulumi from "@pulumi/pulumi"
import * as inputs from "./types/input"

declare class Provider extends pulumi.ProviderResource {
  static isInstance(obj: any): obj is Provider
  constructor(name: string, args?: ProviderArgs, opts?: pulumi.ResourceOptions)
}
interface ProviderArgs {
  readonly kubeconfig?: pulumi.Input<string | undefined>          // ← @pulumi/command remote.Command stdout
  readonly context?: pulumi.Input<string | undefined>
  readonly cluster?: pulumi.Input<string | undefined>
  readonly namespace?: pulumi.Input<string | undefined>
  readonly enableServerSideApply?: pulumi.Input<boolean | undefined>
  readonly renderYamlToDirectory?: pulumi.Input<string | undefined>
  readonly helmReleaseSettings?: pulumi.Input<inputs.HelmReleaseSettings | undefined>
  readonly deleteUnreachable?: pulumi.Input<boolean | undefined>
  readonly clusterIdentifier?: pulumi.Input<string | undefined>
}
```

## [03]-[IMPLEMENTATION_LAW]

[WORKLOAD_TOPOLOGY]:
- Typed resources thread `meta.v1.ObjectMeta` (name/namespace/labels/annotations) and the group's `Spec` shape as `Input`s; the realized object exposes `metadata`/`spec`/`status` as `Output`s. Compose across resources with `Output.apply` and `pulumi.all` — e.g. bind a `Service.spec.clusterIP` into an `Ingress` rule. The literal-typed `apiVersion`/`kind` discriminants make `isInstance`/type-narrowing exhaustive.
- Prefer typed classes for the app's own workloads (`Deployment`/`Service`/`Ingress`/`Secret`); reach `apiextensions.CustomResource` only for operator CRDs with no generated class, and `yaml`/`kustomize` only when adopting an unmodifiable upstream manifest bundle.

[HELM_TOPOLOGY]:
- `helm.v4.Chart` renders server-side and hands Pulumi the manifests, so `values` is the typed configuration surface — the LGTM stack, the OTel collector, and the CNPG operator are each ONE `Chart` with a typed `values` object (no `Pulumi.yaml`, no authored chart YAML). `repositoryOpts.repo` or an `oci://` `chart` ref selects the source; `version` pins it; `namespace` binds to a `core.v1.Namespace`.
- `chart.resources` is the rendered child set — feed it to CrossGuard (`validateStack`) or to `@pulumiverse/grafana` when a chart emits a Grafana whose dashboards this run must populate. Use `helm.v3.Release` ONLY for true release lifecycle; the `Chart` path keeps every resource under Pulumi diff/policy.

[CRD_TOPOLOGY]:
- `apiextensions.CustomResource` is the CNPG `Cluster` owner: `apiVersion: "postgresql.cnpg.io/v1"`, `kind: "Cluster"`, with `spec.instances`/`spec.imageName` (the PG18.4-extension image)/`spec.storage`/`spec.backup` (scheduled-backup + PITR to the object-store row) in the `[field]` catch-all. Install the operator itself via a `helm.v4.Chart`; declare cluster instances via `CustomResource`. The `-rw` Service that the CNPG operator creates is the host handed to `@pulumi/postgresql`.

[STACK_LAW]:
- SELFHOSTED-K8S ARM: the `provider/dispatch` `Match.exhaustive` `selfhosted-k8s` arm is a `Layer`-composed program — (1) `@pulumi/command` `remote.Command` bootstraps the cluster and emits the kubeconfig; (2) `new kubernetes.Provider({ kubeconfig, enableServerSideApply: true })`; (3) `helm.v4.Chart` installs CNPG operator + LGTM + OTel collector with typed `values`; (4) `apiextensions.CustomResource` declares the CNPG `Cluster`; (5) its host `Output` feeds `@pulumi/postgresql.Provider` (`kube/data` seam); (6) `@pulumi/tls`/`@pulumi/random` cert+key material lands in a `core.v1.Secret` (`type: "kubernetes.io/tls"`, `stringData: { "tls.crt", "tls.key" }` — the TLS-secret sink) that `networking.v1.Ingress.spec.tls[].secretName` references to realize `kube/traffic`; (7) `@pulumiverse/grafana` applies `telemetry/board` dashboards onto the rendered Grafana.
- SUBSTRATE WEAVE: authored inside the arm the `dispatch` selects; run by `program/automation` `LocalWorkspace.createOrSelectStack`. Realized workload `Output`s (service host/port, ingress hostname) project through a `Schema`-decoded `StackOutputs` record — the sole `iac`→`work` value crossing (`ShardingConfig`). `@pulumi/policy` `validateResourceOfType(kubernetes.apps.v1.Deployment, …)` narrows against the very classes exported here.

[RAIL_LAW]:
- iac / kubernetes rail; `node`-tier. The plugin shells `helm template` and drives the API server, so this arm runs only where the kubeconfig resolves to a reachable cluster (bootstrap host or in-cluster job). Apply failures ride the Automation-API `diagnostics` event stream folded into the typed run receipt; there is no in-band typed error class — the failure channel is the engine event stream. Chart provenance (`verify`/`keyring`) and SSA field-management (`enableServerSideApply`) are the correctness knobs, not app config.
