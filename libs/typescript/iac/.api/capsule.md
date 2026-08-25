# [TS_IAC_API_CAPSULE]

`capsule` is the namespace-tenancy governor the deploy plane installs when a stack rules tenants soft-isolated. The chart plants an operator and its CRD estate; the TENANTS are custom resources the tier authors afterwards, so chart values decide the controller's admission posture and the `Tenant` CRD decides what a tenant may hold. That split is the contract: no values key creates a tenant, and no `Tenant` field changes how the webhook admits.

## [01]-[CHART_VALUES]

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

## [02]-[TENANT_CONTRACT]

[CRD_ESTATE]: group `capsule.clastix.io`, THIRTEEN definitions. `v1beta2` is served and storage across the estate, and `v1beta1` survives in exactly two shapes: `Tenant` serves it beside `v1beta2` without storing it, and the two proxy kinds hold `v1beta1` as their ONLY version. Cluster-scoped: `Tenant` (plural `tenants`), `CapsuleConfiguration`, `TenantOwner`, `GlobalTenantResource`, `GlobalCustomQuota`, `ResourcePool`, `GlobalProxySettings` (v1beta1). Namespaced: `TenantResource`, `ResourcePoolClaim`, `CustomQuota`, `QuantityLedger`, `RuleStatus`, `ProxySetting` (v1beta1). No chart key creates a `Tenant`: `rbac.resources.create` and `rbac.resourcepoolclaims.create` mint the two aggregation ClusterRoles and nothing else.

| [INDEX] | [FIELD]                                 | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `spec.owners[]`                         | `{ kind, name }` REQUIRED PAIR — `kind` closes on `User \| Group \| ServiceAccount` |
|  [02]   | `spec.namespaceOptions.quota`           | `int32` min 1 — the namespace ceiling; the rest of the block is metadata governance |
|  [03]   | `spec.ingressOptions.allowedClasses`    | the stable ingress-class allowlist — PRESENCE-shaped, empty denies                  |
|  [04]   | `spec.rules[]`                          | the successor enforcement construct — the CRD marks it NOT FINAL                    |
|  [05]   | `spec.{storageClasses,priorityClasses}` | the same selector-allowlist shape `ingressOptions.allowedClasses` carries           |
|  [06]   | `spec.resourceQuotas`                   | `{ items, scope }` — `scope` closes on `Tenant \| Namespace`, DEFAULT `Tenant`      |
|  [07]   | `spec.serviceOptions`                   | service governance — service-shape governance                                       |
|  [08]   | `spec.nodeSelector`                     | `map[string]string` — plain map, no allowlist algebra                               |
|  [09]   | `spec.*` remainder                      | the remaining governance surface                                                    |

[TENANT_REMAINDER]: `additionalRoleBindings` `cordoned` `data` `deviceClasses` `forceTenantPrefix` `gatewayOptions` `permissions` `podOptions` `preventDeletion` `runtimeClasses` — the governance surface beyond the rows above
[OWNER_ITEM]: beyond the required `kind`/`name` pair an owner admits `clusterRoles`, `labels`, `annotations`, and `proxySettings`.
[ALLOWLIST_SHAPE]: `ingressOptions.allowedClasses`, `storageClasses`, and `priorityClasses` share one selector-allowlist: `allowed` (`[]string`, exact match), `allowedRegex` (`string`, DEPRECATED and slated for removal), `default` (`string`, inherited by every created object), and the inline `matchLabels`/`matchExpressions` selector matching the class OBJECT by its labels. Presence shapes the block: an absent one leaves the axis unrestricted, and a present one with every matcher empty matches nothing and denies the axis outright — `allowed: []` refuses every Ingress in the tenant.
[DEPRECATED]: `spec.networkPolicies.items[]` yields to Replications, `spec.containerRegistries.{allowed,allowedRegex}` to `spec.rules[].enforce.workloads.registries`, `spec.limitRanges.items` and `spec.imagePullPolicies` outright. No `Tenant` field is required, so a spec built from deprecated spellings validates and reconciles until the operator bump that deletes them. Deprecated is NOT inert: `containerRegistries` keeps its own pod-webhook route running in parallel with the rule engine, so a spec carrying both makes an image clear two gates that read different values — the legacy one matching the registry HOST and denying any unqualified image, the successor matching the whole reference. Deprecation surfaces only as an admission warning on the `Tenant` write.
[ENFORCE_SHAPE]: `spec.rules[]` items carry `audience`, `enforce`, `namespaceSelector`, and `permissions`, none required — an absent `audience` matches every requesting subject and an absent `namespaceSelector` matches every namespace of the tenant. `enforce` carries `action`, `ingress.{hostnames,types}`, `metadata[]`, `services.{externalNames,loadBalancers,nodePorts,types}`, and `workloads.{qosClasses,registries,schedulers,targets}`; `targets` closes on `pod/containers`, `pod/initcontainers`, `pod/ephemeralcontainers`, and `pod/volumes`, an empty list meaning each webhook's own default. `permissions` is RBAC distribution rather than admission — `{ bindings[], promotions[] }` reconciled into RoleBindings across the selected namespaces.
[ENFORCE_MATCHER]: `workloads.registries` and `workloads.schedulers` are ARRAYS of one shared matcher — `{ exact: []string (minItems 1), exp: string (minLength 1), negate: bool DEFAULT false, policy: []PullPolicy }` under the CEL rule `has(self.exact) || has(self.exp)`. There is no `allowed`/`allowedRegex` pair here: `exact` is whole-value equality (for registries, the ENTIRE `<registry>/<repo>:<tag>` reference, never the host alone) and `negate` inverts the matcher result alone, never the action.
[ENFORCE_ACTION]: `enforce.action` closes on `allow | deny | audit` and DEFAULTS to `deny`, governing what happens to the values a matcher HITS. One engine decides every workload matcher: a non-match contributes nothing, the last matching `allow` or `deny` wins, `audit` only records. Arming an allow-list therefore demands a stated `action: allow`, which is what makes a value matching no allow rule denied. Two consequences follow: a matcher list written without `action` denies exactly what it names while admitting everything else, and an ARMED rule carrying no matcher denies every value on that axis.

[REPLICATION_SPEC]: `GlobalTenantResource` is CLUSTER-scoped and served under `v1beta2` alone. `spec` requires `resources`, `resyncPeriod`, and `settings`, but only `resources` has no default — the other two default at decode (`60s` and `{}`) and so admit by omission.

| [INDEX] | [FIELD]                | [CAPABILITY]                                                                                               |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `spec.resources[]`     | `{ additionalMetadata, context, generators, namespaceSelector, namespacedItems, rawItems }`, none required |
|  [02]   | `…[].rawItems[]`       | `object` under `x-kubernetes-preserve-unknown-fields` — a WHOLE embedded manifest, never a string          |
|  [03]   | `spec.tenantSelector`  | label selector over Tenants — OPTIONAL; absent selects every tenant                                        |
|  [04]   | `spec.scope`           | `Namespace \| Tenant \| None`, DEFAULT `Namespace` — one copy per owned namespace versus per tenant        |
|  [05]   | `spec.resyncPeriod`    | `string` DEFAULT `60s` — the second reconciliation beat beside manifest-change triggers                    |
|  [06]   | `spec.pruningOnDelete` | `boolean` DEFAULT TRUE — deleting the replication deletes everything it replicated                         |
|  [07]   | `spec.cordoned`        | `boolean` DEFAULT FALSE — pauses applies and deletions for maintenance                                     |
|  [08]   | `spec.settings`        | `{ adopt, force }`, both `boolean` DEFAULT FALSE — pre-existing-object adoption and SSA conflict seizure   |
|  [09]   | `spec.serviceAccount`  | `{ name, namespace }` BOTH REQUIRED when present — the identity the replication acts as                    |
|  [10]   | `spec.dependsOn[]`     | `{ name }` REQUIRED — sibling replications that must be ready first                                        |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Tenancy is a spec mode, not a default — the `namespace` arm is the soft-isolation answer where the `vcluster` arm is the hard one, and the `single` mode installs neither. The chart is therefore reached exactly once per stack that chose soft isolation.
- The operator's own namespace label is the tenancy join key: every replicated policy, peer selector, and `GlobalTenantResource` selector reads `capsule.clastix.io/tenant`, so a fence and the namespaces it admits agree by one spelling rather than by parallel rosters.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the operator and, with `skipCrds: false`, hands the thirteen CRDs to the provider as managed resources a version bump diffs — the render carrier escapes Helm's own install-once CRD rule.
- `kube/tenant#TENANT_TIER`: the `namespace` mode row installs this chart with `certManager.generateCertificates: false` and `tls.create: true`, then authors one `capsule.v1beta2.Tenant` per spec tenant and one `GlobalTenantResource` fence beside it, each parented on the chart so admission is live before the first CR lands.
- `../crds/capsule` (crd2pulumi): the typed `v1beta2.Tenant` and `v1beta2.GlobalTenantResource` classes are the branch's compile-checked spelling of this CRD estate, regenerated against the pinned chart rather than pinned as an npm dependency.
- `operate/policy#RECONCILE_LOOP`: a tenant-submitted `Stack` or `Program` CR reconciles inside the RBAC envelope this operator governs, so the tenancy boundary and the self-service provisioning boundary are one.

[LOCAL_ADMISSION]:
- Disarm `certManager.generateCertificates` and arm `tls.create`: the estate rules cert-manager an unarmed in-cluster lane, so the default renders a `Certificate` against CRDs no cluster here holds and the install fails outright rather than degrading.
- Author tenants as typed CRs, never as values: no chart key creates one, and a values-side tenant roster would be a second governance authority beside the CR the operator actually reconciles.
- Spell the successor enforcement construct and accept its instability: `spec.rules[].enforce.workloads.registries` is the live registry allowlist, and the deprecated block beside it is what an operator bump deletes without warning.
- State `action: allow` on every rule meant as an allowlist. Defaulting to `deny` fires on MATCH, so an unstated action turns the roster into a denylist of itself and admits everything it failed to name — a governance row enforcing its own inverse, and one no schema error reports.
- Spell a registry roster as an anchored `exp`, never as `exact`: `exact` compares the whole `<registry>/<repo>:<tag>` reference, so a bare host entry matches no real image. Escape the host before it becomes a pattern.
- Emit no block and no rule for an empty roster. Both governance rails read absence as unrestricted and presence-with-nothing as deny-all, so a defaulted empty roster spelled through as `[]` locks the tenant out of the axis it meant to leave open.
- Read the admission wiring off `CapsuleConfiguration.spec.overrides`, never off the rendered manifest set — the webhook configurations are runtime-reconciled objects the chart never renders.
- Set `replicaCount`, never `manager.replicas`; the latter is accepted by the schema and read by nothing.
