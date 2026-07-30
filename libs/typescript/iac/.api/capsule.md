# [TS_IAC_API_CAPSULE]

`capsule` is the namespace-tenancy governor the deploy plane installs when a stack rules tenants soft-isolated. The chart plants an operator and its CRD estate; the TENANTS are custom resources the tier authors afterwards, so chart values decide the controller's admission posture and the `Tenant` CRD decides what a tenant may hold. That split is the contract: no values key creates a tenant, and no `Tenant` field changes how the webhook admits.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `capsule`
- chart: `capsule` from `https://projectcapsule.dev/charts` (Apache-2.0), source `charts/capsule` in `projectcapsule/capsule`, mirrored at `oci://ghcr.io/projectcapsule/charts/capsule`
- asset: one `CapsuleConfiguration`, the controller Deployment or DaemonSet, a webhook Service and a metrics Service, its RBAC cell, three kubectl hook Jobs, and the `crds/` estate — beside an optional `capsule-proxy` subchart aliased `proxy`, condition `proxy.enabled`, DEFAULT FALSE
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / namespace tenancy

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                                         | [CAPABILITY]                                                                |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `certManager.generateCertificates`            | renders a `Certificate` and an `Issuer`; demands cert-manager CRDs          |
|  [02]   | `tls.{enableController,create,name}`          | the self-managed path: the operator mints and rotates its own material      |
|  [03]   | `crds.*`                                      | CRD lifecycle, RBAC, diagnostics, and the inline-versus-`crds/` split       |
|  [04]   | `replicaCount`                                | `int` — controller replicas — DEAD once `manager.kind` is `DaemonSet`       |
|  [05]   | `manager.kind`                                | the workload shape; an unrecognized value renders NEITHER                   |
|  [06]   | `manager.webhookPort`                         | `int` 9443 — the admission listener the webhook Service targets             |
|  [07]   | `manager.options.*`                           | operator posture — prefix, promotion, administrators, protected regex       |
|  [08]   | `webhooks.hooks.*`                            | 26 booleans — per-resource admission toggles; the whole intercept surface   |
|  [09]   | `webhooks.service.*`                          | a non-empty `url` re-points EVERY hook `clientConfig` off-cluster           |
|  [10]   | `webhooks.*Timeout*`                          | `int` — per-family admission deadlines                                      |
|  [11]   | `webhooks.matchConditions`                    | CEL narrowing applied across the hook family                                |
|  [12]   | `rbac.{resources,resourcepoolclaims}.create`  | `boolean` — the two aggregation ClusterRoles tenant owners bind             |
|  [13]   | `global.jobs.{kubectl,postInstall,preDelete}` | the three lifecycle Jobs and their image                                    |
|  [14]   | `monitoring` `conversions` `extraManifests`   | ServiceMonitor and PrometheusRule, CRD conversion, passthrough objects      |
|  [15]   | `nameOverride` `fullnameOverride`             | `string` — FLAT top-level; undeclared in values yet honored by the scaffold |

[webhooks]: `exclusive` `mutatingWebhooksTimeoutSeconds` `validatingWebhooksTimeoutSeconds` `labels` `annotations` `service` `matchConditions` `hooks`
[crds]: `install` (true) `inline` (false) `exclusive` (false) `labels` `createConfig` (false) `annnotations` (UPSTREAM TYPO, three `n`s — the corrected spelling is a silent no-op) `createRBAC` (false) `createDiagnostics` (false) — `exclusive: true` collapses the release to CRDs alone, and `inline: true` templates them into the release, which the chart's own README pairs with `--skip-crds`
[manager]: `kind` `webhookPort` `image` `options` `rbac` `apiPriorityAndFairness` `hostNetwork` `hostPID` `hostUsers` `deploymentStrategy` `daemonsetStrategy` `extraArgs` `env` `probes` `resources` `volumes` `volumeMounts` `securityContext` — the replica count is `replicaCount`, NOT `manager.replicas`
[manager.options]: `createConfiguration` `capsuleConfiguration` `forceTenantPrefix` `allowServiceAccountPromotion` `administrators` `users` `userNames` `capsuleUserGroups` `ignoreUserWithGroups` `protectedNamespaceRegex` `nodeMetadata` `cacheInvalidation` `rbac` `impersonation`
[webhooks.hooks]: `customquotas` `globalcustomquotas` `calculations` `rulestatus` `metadata` `generic` `resourcepools` `customresources` `namespaces` `cordoning` `gateways` `ingresses` `devices` `pods` `persistentvolumeclaims` `tenants` `owners` `config` `managed` `replications` `services` `nodes` `serviceaccounts` `namespaceOwnerReference` `defaults` `tenantResourceObjects`

[FULLNAME]: the standard collapse scaffold — `fullnameOverride` wins outright, else the name collapses to `.Release.Name` when that name CONTAINS the chart name, else `<release>-capsule`, truncated at 63. Both override keys are absent from `values.yaml` AND from `values.schema.json`, yet the schema root declares neither `additionalProperties: false` nor any `required`, so the keys pass admission and the helper honors them: undocumented and functional.
[SERVICE_NAME]: `<fullname>-webhook-service` on port 9443 named `admission` (targetPort `manager.webhookPort`) and `<fullname>-controller-manager-metrics-service` on 8080 `metrics` plus 10080 `health-api`, both ClusterIP and both gated `crds.exclusive` false; the controller workload is `<fullname>-controller-manager`. A release named `capsule` therefore renders `capsule-webhook-service`, and a release named `tenancy` renders `tenancy-capsule-webhook-service`.
[REFUTATION]: the chart renders NO `ValidatingWebhookConfiguration` and NO `MutatingWebhookConfiguration`. It renders ONE `CapsuleConfiguration` whose `spec.overrides` NAMES the objects the controller reconciles at runtime — `<fullname>-mutating-webhook-configuration`, `<fullname>-validating-webhook-configuration`, `<fullname>-webhook-service`, and the `<fullname>-dynamic-webhook` validating entry — so an estate reading the rendered set for its admission wiring finds nothing and concludes wrongly.

## [03]-[TENANT_CONTRACT]

[CRD_ESTATE]: group `capsule.clastix.io`, `v1beta2` served and storage with `v1beta1` still served — `Tenant` (CLUSTER-scoped, plural `tenants`), `CapsuleConfiguration`, `TenantOwner`, `GlobalTenantResource`, `ResourcePool`, and the namespaced `TenantResource`, `ResourcePoolClaim`, `CustomQuota`, `GlobalCustomQuota`, `QuantityLedger`, `RuleStatus`. The chart creates NO `Tenant`: `rbac.resources.create` and `rbac.resourcepoolclaims.create` mint the two aggregation ClusterRoles and nothing else.

| [INDEX] | [FIELD]                                 | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `spec.owners[]`                         | `{ kind, name }` REQUIRED PAIR — `kind` closes on `User \| Group \| ServiceAccount` |
|  [02]   | `spec.namespaceOptions.quota`           | `int32` min 1 — the namespace ceiling; the rest of the block is metadata governance |
|  [03]   | `spec.ingressOptions.allowedClasses`    | the stable ingress-class allowlist                                                  |
|  [04]   | `spec.rules[]`                          | the successor enforcement construct — the CRD marks it NOT FINAL                    |
|  [05]   | `spec.{storageClasses,priorityClasses}` | allowlist shape — the same `allowed`/`allowedRegex` pair `ingressOptions` uses      |
|  [06]   | `spec.resourceQuotas`                   | `{ items, scope }` — `scope` closes on `Tenant \| Namespace`                        |
|  [07]   | `spec.serviceOptions`                   | service governance — service-shape governance                                       |
|  [08]   | `spec.nodeSelector`                     | `map[string]string` — plain map, no allowlist algebra                               |
|  [09]   | `spec.*` remainder                      | the remaining governance surface                                                    |

[TENANT_REMAINDER]: `additionalRoleBindings` `cordoned` `data` `deviceClasses` `gatewayOptions` `podOptions` `preventDeletion` `runtimeClasses` — the governance surface beyond the rows above
[OWNER_ITEM]: beyond the required `kind`/`name` pair an owner admits `clusterRoles`, `labels`, `annotations`, and `proxySettings`.
[DEPRECATED]: `spec.networkPolicies.items[]` yields to Replications, `spec.containerRegistries.{allowed,allowedRegex}` to `spec.rules[].enforce.workloads.registries`, `spec.limitRanges.items` and `spec.imagePullPolicies` outright. No `Tenant` field is required, so a spec built from deprecated spellings validates and reconciles until the operator bump that deletes them.
[ENFORCE_SHAPE]: `spec.rules[].enforce` carries `action`, `ingress`, `metadata`, `services`, and `workloads.{qosClasses,registries,schedulers,targets}` — the registry allowlist the deprecated block yields to lands at `workloads.registries.allowed`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Tenancy is a spec mode, not a default — the `namespace` arm is the soft-isolation answer where the `vcluster` arm is the hard one, and the `single` mode installs neither. The chart is therefore reached exactly once per stack that chose soft isolation.
- The operator's own namespace label is the tenancy join key: every replicated policy, peer selector, and `GlobalTenantResource` selector reads `capsule.clastix.io/tenant`, so a fence and the namespaces it admits agree by one spelling rather than by parallel rosters.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the operator and, with `skipCrds: false`, hands the eleven CRDs to the provider as managed resources a version bump diffs — the render carrier escapes Helm's own install-once CRD rule.
- `kube/tenant#TENANT_TIER`: the `namespace` mode row installs this chart with `certManager.generateCertificates: false` and `tls.create: true`, then authors one `capsule.v1beta2.Tenant` per spec tenant and one `GlobalTenantResource` fence beside it, each parented on the chart so admission is live before the first CR lands.
- `../crds/capsule` (crd2pulumi): the typed `v1beta2.Tenant` and `v1beta2.GlobalTenantResource` classes are the branch's compile-checked spelling of this CRD estate, regenerated against the pinned chart rather than pinned as an npm dependency.
- `operate/policy#RECONCILE_LOOP`: a tenant-submitted `Stack` or `Program` CR reconciles inside the RBAC envelope this operator governs, so the tenancy boundary and the self-service provisioning boundary are one.

[LOCAL_ADMISSION]:
- Disarm `certManager.generateCertificates` and arm `tls.create`: the estate rules cert-manager an unarmed in-cluster lane, so the default renders a `Certificate` against CRDs no cluster here holds and the install fails outright rather than degrading.
- Author tenants as typed CRs, never as values: no chart key creates one, and a values-side tenant roster would be a second governance authority beside the CR the operator actually reconciles.
- Spell the successor enforcement construct and accept its instability: `spec.rules[].enforce.workloads.registries.allowed` is the live registry allowlist, and the deprecated block beside it is what an operator bump deletes without warning.
- Read the admission wiring off `CapsuleConfiguration.spec.overrides`, never off the rendered manifest set — the webhook configurations are runtime-reconciled objects the chart never renders.
- Set `replicaCount`, never `manager.replicas`; the latter is accepted by the schema and read by nothing.

[RAIL_LAW]:
- Contract: `capsule` chart values + the `capsule.clastix.io/v1beta2` CRD estate the operator reconciles
- Owns: soft namespace tenancy — the admission operator, its webhook and metrics doors, the CRD estate, and the `Tenant` governance vocabulary over quotas, ingress classes, storage classes, registries, and owner bindings
- Accept: `certManager.generateCertificates: false` with `tls.create: true`; `crds.install` with the render carrier managing the CRDs; typed `Tenant` and `GlobalTenantResource` CRs authored beside the chart; `spec.rules[].enforce` as the registry and workload allowlist; the `capsule.clastix.io/tenant` label as the one tenancy join key
- Reject: the cert-manager default on a cluster carrying no cert-manager CRDs; a `manager.kind` value outside `Deployment`/`DaemonSet`, which renders a webhook Service with no controller behind it and raises nothing; `crds.exclusive: true` on an install expected to carry a controller; the deprecated `networkPolicies`, `containerRegistries`, `limitRanges`, and `imagePullPolicies` spellings; an endpoint derived from the release name where the release name does not contain `capsule`
