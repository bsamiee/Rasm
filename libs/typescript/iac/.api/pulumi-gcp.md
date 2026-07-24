# [TS_IAC_API_PULUMI_GCP]

`@pulumi/gcp` is the Terraform-bridged Pulumi provider SDK for Google Cloud: one namespace per GCP service, each carrying the same generated resource quadruple (`class X extends pulumi.CustomResource` + `XArgs` + `XState` + `X.get`/`X.isInstance`) under one `Provider` binding project/region/zone/credentials — one codegen pattern across a service axis, never a bespoke API per resource.

In `iac` this is a prepared cloud row, not the first-class arm: it instantiates only when an app supplies a `gcp` `StackSpec`, and its worth is the service-equivalence map carrying each `selfhosted-k8s` capability to a named managed-GCP counterpart.

[EXPORTS]: `Provider`
[EXPORTS]: `container` `sql` `storage` `dns` `compute` `cloudrunv2` `cloudrun` `secretmanager` `serviceaccount` `projects` `organizations` `certificatemanager` `artifactregistry` `redis` `pubsub` `kms` `iam` `monitoring` `logging`
[EXPORTS]: `config` `types`

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/gcp`
- package: `@pulumi/gcp` (Apache-2.0)
- build-floor: peer `@pulumi/pulumi` — the engine owns `CustomResource`/`Output`/`Input`; the `pulumi-resource-gcp` plugin binary provisions against the Google APIs
- target: `node` (Automation-API program process; needs GCP credentials — ADC, a `credentials` JSON, or impersonation — at apply)
- entry: `@pulumi/gcp` with the service sub-paths (`@pulumi/gcp/container`, `/sql`, `/storage`, …) and `config`/`types`
- asset: generated `CustomResource` classes across every service namespace, per-service `get*`/`get*Output` data sources, the connection `Provider` (project/region/zone/credentials + per-service `*CustomEndpoint` overrides), package-wide `config` readers, and the `types.input`/`types.output` shape namespaces
- rail: iac / cloud-prepared

## [02]-[PUBLIC_TYPES]

### [02.1]-[THE_GENERATED_RESOURCE_QUADRUPLE_EVERY_SERVICE_RESOURCE]

[PUBLIC_TYPE_SCOPE]: resource pattern

Identical to `@pulumi/postgresql`'s pattern — the shared Pulumi Terraform-bridge codegen. Every resource across every service namespace is this quadruple; the service axis is the only variable, and a new service is a new namespace on the same shape.

| [INDEX] | [MEMBER]           | [SHAPE]                                                                                   |
| :-----: | :----------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `class X`          | `extends pulumi.CustomResource`; `readonly <attr>: pulumi.Output<T>` per schema attribute |
|  [02]   | `constructor`      | `(name, args?: XArgs, opts?: pulumi.CustomResourceOptions)`                               |
|  [03]   | `X.get`            | `static get(name, id: pulumi.Input<pulumi.ID>, state?: XState, opts?): X`                 |
|  [04]   | `X.isInstance`     | `static isInstance(obj): obj is X`                                                        |
|  [05]   | `interface XArgs`  | construction inputs; every field `pulumi.Input<T \| undefined>`                           |
|  [06]   | `interface XState` | `.get` lookup inputs; mirrors `XArgs`                                                     |

[CLUSTER]: `Cluster.get(string,pulumi.Input<pulumi.ID>,ClusterState?,pulumi.CustomResourceOptions?) -> Cluster` `Cluster.isInstance(any) -> obj is Cluster` `Cluster.name: pulumi.Output<string>` `Cluster.location: pulumi.Output<string>` `Cluster.endpoint: pulumi.Output<string>` `Cluster.masterAuth: pulumi.Output<outputs.container.ClusterMasterAuth>` `Cluster(string,ClusterArgs?,pulumi.CustomResourceOptions?)`
[CLUSTER_ARGS]: `ClusterArgs.name: pulumi.Input<string|undefined>` `ClusterArgs.location: pulumi.Input<string|undefined>` `ClusterArgs.initialNodeCount: pulumi.Input<number|undefined>` `ClusterArgs.network: pulumi.Input<string|undefined>`

### [02.2]-[SERVICE_EQUIVALENCE_MAP_THE_PREPARED_ROW_VALUE]

[PUBLIC_TYPE_SCOPE]: capability → gcp resource

Integration shape of the prepared row: the `provider/surface` `gcp` column maps each `store/capability` and `selfhosted-k8s` concern to a named managed-GCP resource. Finalizing the `gcp` target instantiates only this subset with `StackSpec` values; every namespace outside the profile stays dormant.

| [INDEX] | [SELFHOSTED_K8S_CONCERN]           | [GCP_SERVICE_RESOURCE]                                                          |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | typed workloads (k8s cluster)      | `container.Cluster` + `container.NodePool` (GKE)                                |
|  [02]   | CNPG PG18.4 database               | `sql.DatabaseInstance` + `sql.Database` + `sql.User` (Cloud SQL)                |
|  [03]   | object-store (conditional-put row) | `storage.Bucket` + `storage.BucketIAMMember` (GCS)                              |
|  [04]   | dns / traffic                      | `dns.ManagedZone` + `dns.RecordSet`                                             |
|  [05]   | cert / ingress                     | `certificatemanager.Certificate`; `compute.GlobalAddress` + `compute.URLMap`    |
|  [06]   | secret owner (Doppler)             | `secretmanager.Secret` + `secretmanager.SecretVersion`                          |
|  [07]   | identity / RBAC                    | `serviceaccount.Account`; `projects.IAMMember`; `organizations.getClientConfig` |
|  [08]   | image registry                     | `artifactregistry.Repository`                                                   |
|  [09]   | serverless compute (alt to k8s)    | `cloudrunv2.Service` / `cloudrunv2.Job`                                         |
|  [10]   | cache / queue                      | `redis.Instance`; `pubsub.Topic` + `pubsub.Subscription`                        |
|  [11]   | networking substrate               | `compute.Network` + `compute.Subnetwork` + `compute.Firewall`                   |

### [02.3]-[PROVIDER_PROJECT_REGION_CREDENTIALS_BOUNDARY]

[PUBLIC_TYPE_SCOPE]: provider

One explicit `Provider` per target project binds every resource in the arm. Credentials are polymorphic — a `credentials` JSON string, an `accessToken`, or `impersonateServiceAccount`, mode by field presence. Per-service `*CustomEndpoint` fields (`containerCustomEndpoint`, `sqlCustomEndpoint`, …) form one uniform `Input<string | undefined>` override family.

| [INDEX] | [MEMBER]                                 | [SIGNATURE_FIELD]                                                      |
| :-----: | :--------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `class Provider`                         | `extends pulumi.ProviderResource`, one per target project              |
|  [02]   | `Provider.isInstance`                    | `static isInstance(obj): obj is Provider`                              |
|  [03]   | `project`                                | `Input<string>` — target GCP project (`StackSpec`)                     |
|  [04]   | `region` / `zone`                        | `Input<string>` — default region/zone (`StackSpec`)                    |
|  [05]   | `credentials`                            | `Input<string>` — service-account JSON (← Doppler secret)              |
|  [06]   | `accessToken`                            | `Input<string>` — short-lived OAuth token alternative                  |
|  [07]   | `impersonateServiceAccount`              | `Input<string>` — SA impersonation chain                               |
|  [08]   | `billingProject` / `userProjectOverride` | quota/billing project attribution                                      |
|  [09]   | `<service>CustomEndpoint`                | uniform `Input<string \| undefined>` override family (one per service) |

[PROVIDER]: `Provider.isInstance(any) -> obj is Provider` `Provider(string,ProviderArgs?,pulumi.ResourceOptions?)`
[PROVIDER_ARGS]: `ProviderArgs.project: pulumi.Input<string|undefined>` `ProviderArgs.region: pulumi.Input<string|undefined>` `ProviderArgs.zone: pulumi.Input<string|undefined>` `ProviderArgs.credentials: pulumi.Input<string|undefined>` `ProviderArgs.accessToken: pulumi.Input<string|undefined>` `ProviderArgs.impersonateServiceAccount: pulumi.Input<string|undefined>` `ProviderArgs.billingProject: pulumi.Input<string|undefined>` `ProviderArgs.userProjectOverride: pulumi.Input<boolean|undefined>`

`config` re-exports the provider fields as package-wide reads (`config.project`, `config.region`, `config.credentials`, …) from `gcp:*` stack config; the arm binds an explicit `Provider` from the `StackSpec` value instead. Per-service data sources (`projects.getProject`, `organizations.getClientConfig`, `compute.getNetwork`, …) follow the `get*`/`get*Output` dual that `pulumi-postgresql.md` documents.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every resource is the quadruple: `XArgs` fields are `pulumi.Input<T>`, `X` attributes are `pulumi.Output<T>`, `X.get` adopts, `X.isInstance` brand-checks. Compose cross-resource with `Output.apply`/`pulumi.all`; never read an `Output` synchronously.
- Replace-on-change fields (region/zone/name on most resources) are `StackSpec` create-time constants, never mutable knobs.
- One `new gcp.Provider(name, { project, region, zone, credentials })` per target project; every resource passes `{ provider }` in `opts`, and credential mode discriminates by field presence, never a mode enum.
- Prepared-row worth is the equivalence map, not the resource count: each `store/capability` and `selfhosted-k8s` concern names one `service.Resource`, and a new capability is one more map row, never a structural change.

[STACKING]:
- `pulumi-pulumi.md`(`.api/pulumi-pulumi.md`): the `gcp` arm is a `Layer`-composed inline program run by `LocalWorkspace.createOrSelectStack`; realized `Output`s (Cloud SQL host, GKE endpoint, bucket URL) decode through `Schema` into the `RunReceipt` `StackOutputs` record — the `iac`→`work` `ShardingConfig` crossing.
- `pulumi-pulumi.md`(`.api/pulumi-pulumi.md`): credentials cross resource boundaries only as `pulumi.secret(...)`-marked `Output`s, redacted in state and the run receipt.
- `pulumi-policy.md`(`.api/pulumi-policy.md`): `validateResourceOfType(gcp.storage.Bucket, …)` / `validateResourceOfType(gcp.sql.DatabaseInstance, …)` narrow the CrossGuard pack against the exact classes exported here, gating every prepared-row run.
- `pulumiverse-doppler.md`(`.api/pulumiverse-doppler.md`): the SA-key `credentials` value arrives as a Doppler `getSecrets` `Output`, never a literal.
- `pulumi-kubernetes.md`(`.api/pulumi-kubernetes.md`): the equivalence map mirrors the `selfhosted-k8s` spine (GKE↔`apiextensions`-declared workloads, Cloud SQL↔CNPG `Cluster`), so an app crosses profiles without touching resource code.
- within-lib: `provider/dispatch` `Match.exhaustive` selects the `gcp` arm on the `StackSpec` `target`, builds `gcp.Provider` from the value, then the equivalence-map resources for the capability profile — adding `gcp` was one dispatch arm + one `provider/surface` column, and finalizing is app data.

[RAIL_LAW]:
- Package: `@pulumi/gcp`
- Owns: every service namespace's `CustomResource` quadruple, the `Provider` credential boundary, and the `selfhosted-k8s`→managed equivalence map — the whole managed-GCP resource surface
- Accept: `pulumi.Input<T>` for every arg; the discriminated `credentials`/`accessToken`/`impersonateServiceAccount` family; resources instantiated only under the `gcp` dispatch arm, for the capability profile the `StackSpec` names
- Reject: a bespoke per-resource client; a credential-mode enum where field presence discriminates; an in-arm typed error rail — apply faults fold through the `pulumi-pulumi.md` run-receipt event stream
