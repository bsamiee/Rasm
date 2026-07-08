# [TS_IAC_API_PULUMI_GCP]

`@pulumi/gcp` is the Terraform-bridged Pulumi provider SDK for Google Cloud: 138 service namespaces (`container`, `sql`, `storage`, `dns`, `compute`, `cloudrunv2`, `secretmanager`, `serviceaccount`, `projects`, `certificatemanager`, `artifactregistry`, …), each carrying the same generated resource quadruple (`class X extends pulumi.CustomResource` + `XArgs` + `XState` + `X.get`/`X.isInstance`) plus `get*`/`get*Output` data sources, under one `Provider` that binds project/region/zone/credentials. The package is ONE codegen pattern (identical to `@pulumi/postgresql`) applied across a service axis — never a bespoke API per resource. In `iac` this is a PREPARED cloud row, not a first-class arm: it is instantiated only when an app supplies a `gcp` `StackSpec` VALUE, and the value that makes it worth carrying is the SERVICE-EQUIVALENCE MAP — each `selfhosted-k8s` capability has a named managed-GCP counterpart (GKE↔workloads, Cloud SQL↔CNPG, GCS↔object-store, Cloud DNS↔traffic, Secret Manager↔Doppler), so finalizing the `gcp` target is app data, and adding it was one `provider/dispatch` arm + one `provider/surface` column.

```ts
// @pulumi/gcp — Provider + 138 service namespaces (each: resources · get* data sources) + config/types
export { Provider }                        // pulumi.ProviderResource (project/region/zone/credentials)
export {                                   // service namespaces (equivalence-map subset shown; 138 total)
  container, sql, storage, dns, compute, cloudrunv2, cloudrun, secretmanager,
  serviceaccount, projects, organizations, certificatemanager, artifactregistry,
  redis, pubsub, kms, iam, monitoring, logging, /* …+118 more */
}
export { config, types }                   // package-wide config reads + input/output shape namespaces
```

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/gcp`
- package: `@pulumi/gcp`
- license: `Apache-2.0`
- build-floor: peer `@pulumi/pulumi ^catalog` (the engine owns `CustomResource`/`Output`/`Input`); the `pulumi-resource-gcp` plugin binary provisions against the Google APIs
- target: `node` (Automation-API program process; needs GCP credentials — ADC, a `credentials` JSON, or impersonation — reachable at apply)
- entry: `@pulumi/gcp` plus the service sub-paths (`@pulumi/gcp/container`, `/sql`, `/storage`, …) and `config`/`types`
- asset: the generated `CustomResource` classes across 138 service namespaces, per-service `get*`/`get*Output` data sources, the connection `Provider` (project/region/zone/credentials + per-service `*CustomEndpoint` overrides), package-wide `config` readers, and the `types.input`/`types.output` shape namespaces
- rail: iac / cloud-prepared

## [02]-[PUBLIC_TYPES]

### the generated resource quadruple — every service resource

[PUBLIC_TYPE_SCOPE]: resource pattern
- rail: iac / cloud-prepared
- entry: `@pulumi/gcp/<service>`

Identical to `@pulumi/postgresql`'s pattern — the shared Pulumi Terraform-bridge codegen. Every resource across all 138 namespaces is this quadruple; the service axis is the only variable.

| [INDEX] | [MEMBER] | [SHAPE] |
|:-----: |:----------------- |:---------------------------------------------------------------------------------------- |
| [01] | `class X` | `extends pulumi.CustomResource`; `readonly <attr>: pulumi.Output<T>` per schema attribute |
| [02] | `constructor` | `(name, args?: XArgs, opts?: pulumi.CustomResourceOptions)` |
| [03] | `X.get` | `static get(name, id: pulumi.Input<pulumi.ID>, state?: XState, opts?): X` |
| [04] | `X.isInstance` | `static isInstance(obj): obj is X` |
| [05] | `interface XArgs` | construction inputs; every field `pulumi.Input<T \| undefined>` |
| [06] | `interface XState` | `.get` lookup inputs; mirrors `XArgs` |

```ts contract
import * as pulumi from "@pulumi/pulumi"

// Canonical shape, shown on container.Cluster (GKE — the workload-equivalence anchor). Every
// gcp resource (sql.DatabaseInstance, storage.Bucket, dns.ManagedZone, …) is this structure.
declare class Cluster extends pulumi.CustomResource {
  static get(name: string, id: pulumi.Input<pulumi.ID>, state?: ClusterState, opts?: pulumi.CustomResourceOptions): Cluster
  static isInstance(obj: any): obj is Cluster
  readonly name: pulumi.Output<string>
  readonly location: pulumi.Output<string>
  readonly endpoint: pulumi.Output<string>
  readonly masterAuth: pulumi.Output<outputs.container.ClusterMasterAuth>
  // …the full GKE attribute set
  constructor(name: string, args?: ClusterArgs, opts?: pulumi.CustomResourceOptions)
}
interface ClusterArgs {
  readonly name?: pulumi.Input<string | undefined>
  readonly location?: pulumi.Input<string | undefined>       // ← StackSpec region/zone
  readonly initialNodeCount?: pulumi.Input<number | undefined>
  readonly network?: pulumi.Input<string | undefined>
  // …every GKE input, each Input<T | undefined>
}
```

### service-equivalence map — the prepared-row VALUE

[PUBLIC_TYPE_SCOPE]: capability → gcp resource
- rail: iac / cloud-prepared
- entry: `@pulumi/gcp/<service>`

This is the integration shape of the prepared row: the `provider/surface` `gcp` column maps each `store/capability` and `selfhosted-k8s` concern to a named managed-GCP resource. Finalizing the `gcp` target = instantiating this subset with the `StackSpec` values; the other 130 namespaces stay dormant.

| [INDEX] | [SELFHOSTED_K8S_CONCERN] | [GCP_SERVICE_RESOURCE] |
|:-----: |:--------------------------------- |:---------------------------------------------------------------- |
| [01] | typed workloads (k8s cluster) | `container.Cluster` + `container.NodePool` (GKE) |
| [02] | CNPG PG18.4 database | `sql.DatabaseInstance` + `sql.Database` + `sql.User` (Cloud SQL) |
| [03] | object-store (conditional-put row) | `storage.Bucket` + `storage.BucketIAMMember` (GCS) |
| [04] | cert / dns / ingress (traffic) | `dns.ManagedZone` + `dns.RecordSet`; `certificatemanager.Certificate`; `compute.GlobalAddress`/`compute.URLMap` |
| [05] | secret owner (Doppler) | `secretmanager.Secret` + `secretmanager.SecretVersion` |
| [06] | identity / RBAC | `serviceaccount.Account`; `projects.IAMMember`; `organizations.getClientConfig` |
| [07] | image registry | `artifactregistry.Repository` |
| [08] | serverless compute (alt to k8s) | `cloudrunv2.Service` / `cloudrunv2.Job` |
| [09] | cache / queue | `redis.Instance`; `pubsub.Topic` + `pubsub.Subscription` |
| [10] | networking substrate | `compute.Network` + `compute.Subnetwork` + `compute.Firewall` |

### `Provider` — project/region/credentials boundary

[PUBLIC_TYPE_SCOPE]: provider
- rail: iac / cloud-prepared
- entry: `@pulumi/gcp`

One explicit `Provider` per target project binds every resource in the arm. Credentials are polymorphic: a `credentials` JSON string, an `accessToken`, or `impersonateServiceAccount` — one shape, mode by field. The huge per-service `*CustomEndpoint` roster (`containerCustomEndpoint`, `sqlCustomEndpoint`, … one `Output<string | undefined>` per service) is a uniform override family, not app config — document as a pattern, never enumerate.

| [INDEX] | [MEMBER] | [SIGNATURE_FIELD] |
|:-----: |:-------------------- |:---------------------------------------------------------------------------------- |
| [01] | `class Provider` | `extends pulumi.ProviderResource`; `constructor(name, args?: ProviderArgs, opts?: pulumi.ResourceOptions)` |
| [02] | `Provider.isInstance` | `static isInstance(obj): obj is Provider` |
| [03] | `project` | `Input<string>` — target GCP project (`StackSpec`) |
| [04] | `region` / `zone` | `Input<string>` — default region/zone (`StackSpec`) |
| [05] | `credentials` | `Input<string>` — service-account JSON (← Doppler secret) |
| [06] | `accessToken` | `Input<string>` — short-lived OAuth token alternative |
| [07] | `impersonateServiceAccount` | `Input<string>` — SA impersonation chain |
| [08] | `billingProject` / `userProjectOverride` | quota/billing project attribution |
| [09] | `<service>CustomEndpoint` | uniform `Input<string \| undefined>` override family (one per service) |

```ts contract
import * as pulumi from "@pulumi/pulumi"

declare class Provider extends pulumi.ProviderResource {
  static isInstance(obj: any): obj is Provider
  constructor(name: string, args?: ProviderArgs, opts?: pulumi.ResourceOptions)
}
interface ProviderArgs {
  readonly project?: pulumi.Input<string | undefined>
  readonly region?: pulumi.Input<string | undefined>
  readonly zone?: pulumi.Input<string | undefined>
  readonly credentials?: pulumi.Input<string | undefined>            // ← @pulumiverse/doppler SA-key secret
  readonly accessToken?: pulumi.Input<string | undefined>
  readonly impersonateServiceAccount?: pulumi.Input<string | undefined>
  readonly billingProject?: pulumi.Input<string | undefined>
  readonly userProjectOverride?: pulumi.Input<boolean | undefined>
  // + the per-service `${service}CustomEndpoint?: Input<string | undefined>` override family
}
```

`config` re-exports the provider fields as package-wide reads (`config.project`, `config.region`, `config.credentials`, …) from `gcp:*` stack config; prefer an explicit `Provider` from the `StackSpec` value. Per-service data sources (`projects.getProject`, `organizations.getClientConfig`, `compute.getNetwork`, …) follow the `get*`/`get*Output` dual documented in `pulumi-postgresql.md`.

## [03]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- The quadruple is identical to `@pulumi/postgresql` — same Pulumi Terraform-bridge codegen. `XArgs` fields are `pulumi.Input<T>`, `X` attributes are `pulumi.Output<T>`, `X.get` adopts, `X.isInstance` brand-checks. Compose cross-resource with `Output.apply`/`pulumi.all`; never read an `Output` synchronously.
- Replace-on-change fields (region/zone/name on most resources) are `StackSpec` create-time constants, not mutable knobs.

[PROVIDER_TOPOLOGY]:
- One `new gcp.Provider(name, { project, region, zone, credentials })` per target project; every resource passes `{ provider }` in `opts`. Credential mode is discriminated by field presence (`credentials` JSON / `accessToken` / `impersonateServiceAccount`), never a mode enum — the same polymorphism as the postgresql provider's auth family.

[EQUIVALENCE_TOPOLOGY]:
- The prepared row's worth is the equivalence map, not the resource count. The `provider/surface` `gcp` column names the exact `service.Resource` per `store/capability` and `selfhosted-k8s` concern; the arm instantiates only that subset. A new capability is one new row in the map (one more `service.Resource`), never a structural change — the same row-shaped growth as the dispatch itself.

[STACK_LAW]:
- PREPARED-ROW FINALIZATION: an app supplies a `StackSpec` VALUE (`{ target: "gcp", region, domain, capabilityProfile, dopplerRef }`); `provider/dispatch` `Match.exhaustive` selects the `gcp` arm; the arm builds `gcp.Provider` from the value + the Doppler-sourced SA key, then the equivalence-map resources for the capability profile. Adding `gcp` was one dispatch arm + one surface column; finalizing is app data — never a lib edit, never a fork.
- SUBSTRATE WEAVE: the arm is a `Layer`-composed program run by `program/automation` `LocalWorkspace.createOrSelectStack`; realized `Output`s (Cloud SQL host, GKE endpoint, bucket URL) project through a `Schema`-decoded `StackOutputs` record — the `iac`→`work` `ShardingConfig` crossing. Credentials arrive as secret `Output`s marked with the engine's `pulumi.secret(...)`, redacted in state and receipt.
- CROSSGUARD: `@pulumi/policy` gates the gcp resources by class — `validateResourceOfType(gcp.storage.Bucket, (b, _, report) => …)`, `validateResourceOfType(gcp.sql.DatabaseInstance, …)` — narrowing against the exact classes exported here; the pack is the compliance gate on every prepared-row run.

[RAIL_LAW]:
- iac / cloud-prepared rail; `node`-tier. Instantiated ONLY when an app targets `gcp`; dormant otherwise. The plugin provisions against live Google APIs, so the arm runs only where GCP credentials resolve. Apply failures ride the Automation-API `diagnostics` event stream folded into the typed run receipt — no in-band typed error class. Prepared (not first-class): the `selfhosted-k8s` arm is the default; `gcp` is carried for the service-equivalence path, not the primary deploy target.
