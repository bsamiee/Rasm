# [API_CATALOGUE] @pulumi/kubernetes

`@pulumi/kubernetes` is the full Kubernetes API surface generated from the OpenAPI schema into a uniform `<group>.<version>.<Kind>` namespace tree — ~520 resource classes across 29 API-group namespaces, NOT a hand-picked roster. The mechanism is one parameterized triple per Kind: `<Kind>` (the managed resource), `<Kind>List` (a bulk apply of a `*List` manifest), and `<Kind>Patch` (a server-side-apply field-owner patch of an existing object). Three escape hatches sit beside the generated tree — `apiextensions.CustomResource`/`CustomResourcePatch` for CRD instances the schema does not know, the `helm.v3.Release`/`helm.v4.Chart` chart deployers, and the `yaml.ConfigFile`/`ConfigGroup` + `kustomize.Directory` manifest overlays. A `Provider` carries kubeconfig and server-side-apply policy. Every resource speaks the `@pulumi/pulumi` `Output<T>`/`Input<T>` algebra; the workload Kinds (`Pod`/`Deployment`/`StatefulSet`) block completion on readiness.

- package: `@pulumi/kubernetes`
- version: `4.32.0`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, `./provisioning` (`iac`) subpath; never on the runtime hot path, never browser-reachable.
- rail: deployment

## [01]-[PACKAGE_SURFACE]

[GENERATED_PATTERN]: a Kind is addressed as `<group>.<version>.<Kind>` and exports the triple. Do not enumerate a fixed class list — instantiate the path:

```ts
new apps.v1.Deployment(name, args?: DeploymentArgs, opts?)         // managed resource; args.metadata + args.spec
apps.v1.Deployment.get(name, id, opts?)                            // adopt an existing in-cluster object
new apps.v1.DeploymentPatch(name, args?, opts?)                    // SSA patch — this program owns only the fields it sets
new core.v1.PodList(name, args?, opts?)                            // apply every item of a *List manifest as tracked resources
```

Input/output shapes ride the parallel `types.input.<group>.<version>.<Kind>Spec` / `types.output.<group>.<version>.<Kind>` families (`ObjectMeta`, `PodSpec`, `Container`, `ResourceRequirements`, …) filled inline as `Input<…>` object literals, never hand-declared.

[NAMESPACES]: the 29 top-level members mirror the API groups plus the overlays — `core`, `apps`, `batch`, `networking`, `rbac`, `storage`, `autoscaling`, `apiextensions`, `admissionregistration`, `apiregistration`, `auditregistration`, `certificates`, `coordination`, `discovery`, `events`, `extensions`, `flowcontrol`, `node`, `policy`, `resource`, `scheduling`, `settings`, `storagemigration`, `meta`, `types`, plus `helm`, `kustomize`, `yaml`. Each group carries its versioned sub-namespaces (`v1`, `v1beta1`, `v2`, …; e.g. `autoscaling.{v1,v2,v2beta1,v2beta2}`).

## [02]-[PROVIDER]

`new Provider(name, args?: ProviderArgs, opts?)` — one per cluster, passed as `ResourceOptions.provider` for multi-cluster stacks. Server-side apply is the default authoring mode:

```ts
interface ProviderArgs {
  kubeconfig?: Input<string>            // file contents OR path; use Config.requireSecret to keep it out of state
  context?: Input<string>; cluster?: Input<string>; namespace?: Input<string>
  enableServerSideApply?: Input<boolean>        // default true
  enablePatchForce?: Input<boolean>             // force-resolve field-manager conflicts under SSA
  deleteUnreachable?: Input<boolean>; skipUpdateUnreachable?: Input<boolean>
  enableConfigMapMutable?: Input<boolean>; enableSecretMutable?: Input<boolean>
  renderYamlToDirectory?: Input<string>         // render manifests to disk instead of applying
  helmReleaseSettings?: Input<types.input.HelmReleaseSettings>; kubeClientSettings?: Input<types.input.KubeClientSettings>
  suppressDeprecationWarnings?: Input<boolean>; suppressHelmHookWarnings?: Input<boolean>; upsertExistingObjects?: Input<boolean>
}
```

## [03]-[WORKLOAD_EXEMPLARS]

The Kinds the `provisioning/contract#PROVISIONING` cloud compute tier composes — each an INSTANCE of the generated pattern (§01), not a closed set:

| [INDEX] | [KIND]                                    | [ROLE_IN_COMPUTE_TIER]                                 |
| :-----: | :---------------------------------------- | :----------------------------------------------------- |
|  [01]   | `apps.v1.Deployment`                      | the API workload; blocks on rollout readiness          |
|  [02]   | `core.v1.Service`                         | in-cluster/LB service front                            |
|  [03]   | `autoscaling.v2.HorizontalPodAutoscaler`  | the HPA autoscaling row                                |
|  [04]   | `networking.v1.Ingress` / `IngressClass`  | the ingress rule + class                               |
|  [05]   | `core.v1.ConfigMap` / `Secret`            | config + secret projection (`Secret.stringData`)       |
|  [06]   | `core.v1.Namespace` / `ServiceAccount`    | tenancy scope + workload identity                      |
|  [07]   | `apps.v1.StatefulSet` / `DaemonSet`       | stateful tier + node-local collector (observe tier)    |
|  [08]   | `batch.v1.Job` / `CronJob`                | one-shot + scheduled workloads                          |
|  [09]   | `rbac.v1.Role`/`ClusterRole`/`*Binding`   | authorization rows                                     |
|  [10]   | `core.v1.PersistentVolumeClaim` / `…Volume` | data-tier storage claims                             |

`Pod`, `Deployment`, and `StatefulSet` block Pulumi completion until ready; override the 10-minute default via `ResourceOptions.customTimeouts`.

## [04]-[HELM_YAML_KUSTOMIZE]

[HELM]: two generations. `helm.v3.Release` is the CRD-backed release (full Helm lifecycle + await); `helm.v4.Chart` and the deprecated `helm.v3.Chart` are `ComponentResource` renderers (chart → tracked resources, no Helm-release object).

```ts
new helm.v3.Release(name, args: ReleaseArgs, opts?)   // ~36 fields; load-bearing:
interface ReleaseArgs {
  chart: Input<string>                                 // name (resolved via repositoryOpts.repo) or local path
  version?: Input<string>; namespace?: Input<string>; createNamespace?: Input<boolean>
  repositoryOpts?: Input<types.input.helm.v3.RepositoryOpts>       // { repo, username, password, key/cert material }
  values?: Input<{[k:string]: any}>; valueYamlFiles?: Input<(asset.Asset|asset.Archive)[]>
  atomic?: Input<boolean>            // purge on failure
  skipAwait?: Input<boolean>         // default false → block until all chart resources ready
  waitForJobs?: Input<boolean>; timeout?: Input<number>; maxHistory?: Input<number>
  cleanupOnFail?: Input<boolean>; forceUpdate?: Input<boolean>; recreatePods?: Input<boolean>
  disableWebhooks?: Input<boolean>; disableCRDHooks?: Input<boolean>; skipCrds?: Input<boolean>; postrender?: Input<string>
  // + allowNullValues/dependencyUpdate/devel/lint/replace/resetValues/reuseValues/verify/keyring/description/…
}
new helm.v4.Chart(name, args: ChartArgs, opts?)        // chart, namespace?, values?, valueYamlFiles?, repositoryOpts?, version?, skipAwait?, skipCrds?, postRenderer?, plainHttp?, resourcePrefix?, dependencyUpdate?, devel?, keyring?, verify?
```

[YAML]: `yaml.ConfigFile` (single manifest) and `yaml.ConfigGroup` (globs/strings/URLs) are `CollectionComponentResource`s exposing a `resources` `Output<{[key: string]: CustomResource}>` map for downstream `dependsOn` wiring; `yaml.v2.ConfigFile`/`ConfigGroup` are the SSA-native re-implementations.

[KUSTOMIZE]: `kustomize.Directory` (v1, `CollectionComponentResource`) and `kustomize.v2.Directory` build a kustomization overlay into tracked resources.

[CRD]: `apiextensions.CustomResource(name, { apiVersion, kind, metadata, spec }, opts?)` and `apiextensions.CustomResourcePatch` are the generic escape hatch for any CRD instance; `apiextensions.v1.CustomResourceDefinition` registers the CRD itself.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- All Kinds extend `pulumi.CustomResource`; args accept `Input<T>`, outputs emit `Output<T>`. The `<Kind>Patch` variant is SSA-only: the program owns solely the fields it sets, so two programs can co-own one object by field manager.
- Server-side apply is on by default in `Provider`; `enablePatchForce` resolves field-manager conflicts. `renderYamlToDirectory` turns any provider into a manifest renderer (GitOps handoff).
- `yaml`/`kustomize`/`helm.v3.Chart` collection resources expose their child map through `resources` / `getResource(...)` for cross-resource `dependsOn`.

[DEPLOY_STACK]: how the `provisioning/contract#PROVISIONING` cloud arm stacks this onto `@pulumi/pulumi` (`pulumi-pulumi.md`) core and the Effect rails.
- The `DeployMode` `Match.exhaustive` `cloud` arm builds the compute tier from `Deployment` + `Service` + `HorizontalPodAutoscaler` + `Ingress`, and the observe tier from a `helm.v3.Release` (Grafana Alloy OTLP collector as a `DaemonSet`) — each a child of the `TierStack` `ComponentResource` (`{ parent: this, provider }`).
- One `Provider` per cluster is threaded through `ResourceOptions.provider`; `kubeconfig` arrives as `Config.requireSecret` so credentials never enter state. `Secret.stringData` values are `pulumi.secret()` `Output`s.
- Effect boundary: the whole graph applies through the `@pulumi/pulumi/automation` `Stack` under `Effect.tryPromise`/`Effect.async` (the deploy/drift fold in `pulumi-pulumi.md`); the `StackOutputs` `StackReference` publishes the cluster's DSN/OTLP endpoint the consuming tiers read.

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Output`/`Input` algebra, `ComponentResource`/`registerOutputs`, and the `CustomResourceOptions` (`parent`/`dependsOn`/`provider`/`customTimeouts`) every Kind takes.
- `@pulumi/docker` (`pulumi-docker.md`) is the self-hosted peer this cloud tier mirrors resource-for-resource under the one `DeployMode` dispatch; `@pulumi/aws` (`pulumi-aws.md`) provisions the EKS cluster whose kubeconfig feeds this `Provider`.
- `@pulumiverse/grafana` and the Alloy Helm chart ride `helm.v3.Release` for the observe tier; `@pulumi/command` (`pulumi-command.md`) covers imperative post-apply steps; `effect` owns the `Match.exhaustive` `DeployMode` fold and the `Effect.tryPromise` automation boundary.

[RAIL_LAW]:
- Package: `@pulumi/kubernetes`
- Owns: the full Kubernetes resource lifecycle (generated `group.version.Kind` triples), Helm chart deployment, and manifest/kustomize-driven apply, under server-side apply.
- Accept: the generated `<group>.<version>.<Kind>` path (never a hand-maintained class roster); `<Kind>Patch` for co-owned SSA fields; `apiextensions.CustomResource` for unknown CRDs; `Input<T>` on every arg and `Output<T>` on every output; `Config.requireSecret` for kubeconfig.
- Reject: hand-rolled `kubectl`/raw REST inside a Pulumi program; treating the exemplar Kinds as the whole surface; a parallel cloud/self-hosted codebase instead of the one `DeployMode` dispatch.
