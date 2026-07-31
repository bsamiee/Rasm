# [IAC_TENANT]

Tenant isolation on the `selfhosted-k8s` arm is one tier over one dispatch: `Tenants` realizes the spec's tenancy mode through the `_MODES` handler record — `namespace` installs Capsule once and mints one typed `Tenant` CR per tenant slug carrying the spec's governance columns beside a replicated ingress fence; `vcluster` realizes one virtual control plane per tenant for workloads needing an independent API surface, CRD estate, or version line. Capsule classes are committed `crd2pulumi` output, and each vcluster is one chart. `Tenants.platform` wraps `StackReference` so a tenant stack reads the platform stack's published planes through the same `StackOutputs` channel vocabulary. Data isolation is `kube/data.md`'s `_TENANCY` record, secret access is `operate/secret.md`'s `_ACCESS` rows, boards are `operate/observe.md`'s per-tenant organizations, and vanity hostnames are `kube/traffic.md`'s rows. Module `iac/src/kube/tenant.ts` grows by one `_MODES` row per isolation mode and one spec slug per tenant.

## [01]-[INDEX]

- [02]-[ISOLATION_MODES]: the mode dispatch: Capsule governance rows, replicated fence, vcluster planes; `Tenants`.
- [03]-[PLATFORM_SEAM]: the cross-stack read and its audit verdict; `Tenants`, `Reading`.

## [02]-[ISOLATION_MODES]

[ISOLATION_MODES]:
- Owner: `Tenants` — the `_MODES` record keyed by the escalated tenancy modes (`single` never reaches this tier; the arm gates construction), exhaustive by mapped annotation so a new mode literal in the spec fails compilation here until its row lands; the `namespace` row installs the Capsule chart once (`skipCrds: false`, self-managed webhook TLS) and folds one `Tenant` CR per slug carrying the spec's own governance columns — owner group, namespace quota, admitted ingress classes, admitted registries — beside one `GlobalTenantResource` replicating the intra-tenant ingress fence into every namespace that tenant owns; the `vcluster` row mints one namespace per tenant and realizes one `helm.v4.Chart` inside it; both rows receive the tier's option fold as a scope callback, so ownership threads without a public option surface.
- Law: tenancy is policy rows, never bespoke paths — a tenant is a slug in `spec.profile.tenancy.tenants`; this tier, the database tier, board organizations, and vanity hostnames consume that vocabulary directly, and per-tenant special-casing in any tier is the split this owner exists to forbid.
- Law: the isolation ladder is deliberate — `namespace` (Capsule) is the default escalation: highest density, one API server, policy-enforced boundaries; `vcluster` is the hard row for the tenant whose workloads are untrusted, need their own CRD estate, or must version-skew from the host plane; choosing per-tenant rather than per-estate isolation mixes is a spec-shape growth this record absorbs as a row parameter, not a new owner.
- Law: the tenant's blast radius is closed from both sides — Capsule governs what the tenant's namespaces may express, the `kube/traffic.md` fence governs what reaches them, `operate/policy.md`'s rows judge what they ship, and the PKO loop (`operate/policy.md` `Reconcile`) executes tenant-submitted desired state inside the same envelope, so self-service provisioning never widens the boundary.
- Law: governance is spec data, never a module constant — the namespace quota, the owner-group spelling, the admitted registries, and the admitted ingress classes are `StackSpec.profile.tenancy` columns this fold reads, exactly as `_scale` and `_LIFE` read spec facts elsewhere in this folder; a per-estate quota and an `-admin` suffix baked here are estate policy wearing a literal's clothes, and every one of them is the kind of value an operator changes without touching a tier.
- Law: the deprecated Tenant blocks have no spelling — the CR still serves `networkPolicies`, `containerRegistries`, `limitRanges`, and `imagePullPolicies`, and the operator has superseded each: the fence rides a `GlobalTenantResource` replication and the registry allowlist rides `rules[].enforce.workloads.registries`, while `ingressOptions.allowedClasses` stays the stable spelling it always was; composing a deprecated block on a cluster-scoped governance CR is a row the next operator bump deletes with no diff to warn on.
- Law: the governor installs against its own TLS — `certManager.generateCertificates` defaults TRUE and renders a `Certificate` beside an `Issuer`, so the chart fails to install on any cluster carrying no cert-manager CRDs, and this estate stands up none (`kube/traffic.md` rules the in-cluster ACME lane unarmed); `tls.create` is the chart's self-managed path and the operator mints and rotates its own webhook material under it. The chart renders no webhook configuration objects at all — the single `CapsuleConfiguration` it does render names them for the operator to reconcile at runtime, so a row hunting a rendered `ValidatingWebhookConfiguration` is hunting an object that does not exist.
- Law: the vcluster release name IS the vcluster name — this chart defines no fullname helper and carries neither `nameOverride` nor `fullnameOverride` in its values, so every rendered object reads `.Release.Name` verbatim and renaming a virtual plane is a new release; a row spelling an override names a key the chart's strict schema rejects, which is a render refusal rather than the silent no-op the same mistake buys elsewhere.
- Law: the virtual plane's values are a closed schema, and its removals are hard failures — the snapshot keys the chart retired (`sync.toHost.volumeSnapshots`, `sync.toHost.volumeSnapshotContents`, `sync.fromHost.volumeSnapshotClasses`, `deploy.volumeSnapshotController`, `rbac.enableVolumeSnapshotRules`) refuse at install merely by being PRESENT, so a values file carried forward from an older pin fails the whole tenant fold; `controlPlane.distro` likewise carries exactly one child, so a k3s, k0s, or eks arm is a key that no longer exists.
- Law: the virtual plane's workload kind is computed, never declared — the chart renders a StatefulSet when a persistence claim or the embedded etcd is on and a Deployment otherwise, and the stock values do the former with a 5Gi `Retain` claim that survives uninstall; a reader that hardcodes the kind reads back a resource the chart did not render the moment a values row flips.
- Entry: `new Tenants("tenants", { spec, versions }, opts)` inside the k8s arm when `tenancy.mode !== "single"`.
- Growth: a new isolation mode is one `_MODES` row; a Capsule governance axis is one `StackSpec.profile.tenancy` column with its field on this fold.
- Boundary: the Capsule and vcluster chart values drift with their pins; the generated `crds/capsule` module regenerates on operator bumps; the tenancy vocabulary and its defaults are `program/spec.md`'s; the PG tier escalation is `kube/data.md`'s record.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`, `core.v1.Namespace`, `types.input.networking.v1.NetworkPolicy`); `../crds/capsule` (`v1beta2.Tenant`, `v1beta2.GlobalTenantResource` — crd2pulumi); `effect` (`Array`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Array } from "effect"
import * as capsule from "../crds/capsule"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace Tenants {
  type Args = {
    readonly spec: StackSpec
    readonly versions: { readonly capsule: pulumi.Input<string>; readonly vcluster: pulumi.Input<string> }
  }
  type Scope = (overrides?: pulumi.CustomResourceOptions) => pulumi.CustomResourceOptions
}

// The operator's own label on every namespace it governs: the fence's peer selector and the replicated policy read
// one spelling, so a tenant's namespaces admit each other and nothing else by construction.
const _TENANT_LABEL = "capsule.clastix.io/tenant"

// The intra-tenant ingress fence, replicated into every namespace the tenant owns. It rides `GlobalTenantResource`
// because the Tenant CR's own `networkPolicies` block is deprecated in favour of Replications, and a deprecated
// spelling on a cluster-scoped governance CR is exactly the row an operator bump deletes without warning.
const _fence = (tenant: string): k8s.types.input.networking.v1.NetworkPolicy => ({
  apiVersion: "networking.k8s.io/v1",
  kind: "NetworkPolicy",
  metadata: { name: `${tenant}-fence` },
  spec: {
    policyTypes: ["Ingress"],
    podSelector: {},
    ingress: [{ from: [{ namespaceSelector: { matchLabels: { [_TENANT_LABEL]: tenant } } }] }],
  },
})

const _MODES: {
  readonly [K in Exclude<StackSpec.Tenancy["mode"], "single">]: (args: Tenants.Args, scope: Tenants.Scope) => void
} = {
  namespace: (args, scope) => {
    const governance = args.spec.profile.tenancy
    const governor = new k8s.helm.v4.Chart("capsule", {
      chart: "capsule",
      repositoryOpts: { repo: "https://projectcapsule.dev/charts" },
      version: args.versions.capsule,
      skipCrds: false,
      values: {
        // `certManager.generateCertificates` DEFAULTS TRUE and renders a `Certificate` plus an `Issuer`, so the
        // install fails outright on a cluster carrying no cert-manager CRDs — which is every cluster this estate
        // stands up, since `kube/traffic.md` rules cert-manager an unarmed in-cluster lane. The chart's own
        // self-managed path is the answer: the operator mints and rotates its own webhook material.
        certManager: { generateCertificates: false },
        tls: { create: true },
      },
    }, scope())
    Array.map(args.spec.tenants, (tenant) => {
      const owned = new capsule.v1beta2.Tenant(tenant, {
        metadata: { name: tenant },
        spec: {
          // `kind` closes on the operator's own `User | Group | ServiceAccount` enum; the group SPELLING is the
          // spec's row, so an estate binding tenants to a different directory convention edits one field.
          owners: [{ name: `${tenant}-${governance.ownerGroup}`, kind: "Group" }],
          namespaceOptions: { quota: governance.quota },
          // Not deprecated, unlike the registry and network blocks beside it: the allowlist is the CR's own
          // stable ingress governance and the traffic tier's classes are what a tenant may name.
          ingressOptions: { allowedClasses: { allowed: [...governance.ingressClasses] } },
          // Enforcement is the successor to the deprecated `containerRegistries` allowlist. The CRD marks the rule
          // construct unstable, so the column rides the operator version this tier pins and moves with it.
          rules: [{ enforce: { workloads: { registries: { allowed: [...governance.registries] } } } }],
        },
      }, scope({ dependsOn: [governor] }))
      return new capsule.v1beta2.GlobalTenantResource(`${tenant}-fence`, {
        metadata: { name: `${tenant}-fence` },
        spec: {
          tenantSelector: { matchLabels: { [_TENANT_LABEL]: tenant } },
          resources: [{ rawItems: [_fence(tenant)] }],
        },
      }, scope({ dependsOn: [owned] }))
    })
  },
  vcluster: (args, scope) =>
    void Array.map(args.spec.tenants, (tenant) => {
      const home = new k8s.core.v1.Namespace(tenant, { metadata: { name: tenant } }, scope())
      // The RELEASE name is the vcluster's name: this chart defines no fullname helper and neither
      // `nameOverride` nor `fullnameOverride` exists in its values, so every rendered object — the Service, the
      // headless Service, the `vc-`-prefixed ServiceAccount — reads `.Release.Name` verbatim. A row spelling an
      // override here names a key the schema rejects and renames nothing.
      return new k8s.helm.v4.Chart(`${tenant}-plane`, {
        chart: "vcluster",
        repositoryOpts: { repo: "https://charts.loft.sh" },
        version: args.versions.vcluster,
        namespace: home.metadata.name,
        values: {
          // `{ enabled, patches }` is this key's whole shape under a schema that refuses excess members, and the
          // legacy top-level `sync.ingresses` spelling no longer exists.
          sync: { toHost: { ingresses: { enabled: true } } },
        },
      }, scope())
    }),
}

class Tenants extends Tier {
  static platform(qualified: string): Tenants.Platform {
    return _platform(qualified)
  }
  constructor(name: string, args: Tenants.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Tenants", name, opts)
    const mode = args.spec.profile.tenancy.mode
    if (mode !== "single") {
      _MODES[mode](args, (overrides) => this.child(overrides))
    }
    this.seal({ tenants: [...args.spec.tenants] })
  }
}
```

## [03]-[PLATFORM_SEAM]

[PLATFORM_SEAM]:
- Owner: `Tenants.platform` — the one `StackReference` wrap for the multi-stack estate: a tenant stack's program calls `Tenants.platform(qualified)` and reads the platform stack's published planes through `output(channel)` (`Option`-shaped absence) or `require(channel)` (fail-loud), where `channel` is the same `<plane>.<field>` spelling `StackOutputs.pairsOf` mints — the cross-stack vocabulary and the env vocabulary are one vocabulary, so a platform rename breaks both consumers at one spelling.
- Law: the read is coordinates only, and `audit` is what PROVES it — the platform publishes no material (its own `StackOutputs.read` gate refuses any secret-flagged entry), and the third member is the evidence half of that claim rather than a restatement of it: `getOutputDetails` returns the `{ value, secretValue }` pair whose two halves are exclusive by contract, so a channel folds to `Published`, to `Sealed`, or to `Absent`, and a `Sealed` reading is the gate's own counterexample named at the consuming end. A tenant needing a credential still routes through its own Doppler access rows; the audit path exists so a leak is a verdict rather than a discovery.
- Law: the audit member crosses at the rail, never on the Output graph — `getOutput` and `requireOutput` return `Output` and take `Input<string>`, while `getOutputDetails` returns a `Promise` and takes a bare `string`, so it converts through `Effect.tryPromise` under the folder's one `DeployFault.triaged` seam exactly as `program/automation.md` lifts every workspace call; a raw `await` or a `.then` at a call site is the flat-code defect that conversion deletes.
- Law: enumeration outranks probing — `secretOutputNames` publishes the whole secret-flagged roster off the same reference, so the leak question is answered by reading the set rather than by guessing which channel to audit; the per-channel fold then names the one that broke.
- Law: platform-stack-per-estate, tenant-stack-per-tenant is the sharded topology — the platform stack realizes the shared tiers (data, fanout, observe), and each tenant stack realizes its own workload/traffic against the platform's read planes.
- Entry: `const platform = Tenants.platform("org/rasm/platform")` inside a tenant stack's `PulumiFn`; `platform.require("otlp.endpoint")` into the tenant workload env; `platform.audit("data.host")` where evidence must name what the upstream actually published.
- Growth: a new cross-stack fact is one published plane on the platform spec — this seam inherits it with zero edits; a new reading verdict is one `Reading` case.
- Boundary: which planes exist is `spec.md`'s owner; the publishing gate that makes `Sealed` unreachable is `spec.md`'s `StackOutputs.read`; who runs tenant stacks (deploy host, PKO, review stack) is orthogonal to this read.
- Packages: `@pulumi/pulumi` (`StackReference`, `getOutputDetails`, `secretOutputNames`); `effect` (`Data`, `Effect`, `Option`); `../program/automation.ts` (`DeployFault`).

```typescript
import { Data, Effect, Option } from "effect"
import { DeployFault } from "../program/automation.ts"

// What ONE cross-stack channel reads back. The three cases are the platform's own contract read as a family
// rather than asserted as prose: a coordinate published, a value the publishing gate should have refused, and a
// channel that does not exist. `Sealed` is the case the page's no-material law exists to make unreachable, so
// carrying it is what turns that law into a verdict a caller can route on.
type Reading = Data.TaggedEnum<{
  Published: { readonly channel: string; readonly value: string }
  Sealed: { readonly channel: string }
  Absent: { readonly channel: string }
}>
const Reading: Data.TaggedEnum.Constructor<Reading> = Data.taggedEnum<Reading>()

const _platform = (qualified: string): {
  readonly output: (channel: string) => pulumi.Output<Option.Option<string>>
  readonly require: (channel: string) => pulumi.Output<string>
  readonly audit: (channel: string) => Effect.Effect<Reading, DeployFault>
  readonly sealed: pulumi.Output<ReadonlyArray<string>>
} => {
  const reference = new pulumi.StackReference(qualified)
  return {
    output: (channel) =>
      reference.getOutput(channel).apply((value: unknown) =>
        typeof value === "string" ? Option.some(value) : Option.none()),
    require: (channel) => reference.requireOutput(channel).apply(String),
    // `getOutputDetails` is the one member returning a Promise rather than an Output, and it takes a bare
    // `string` where its two siblings take `Input<string>` — so the audit path converts at the folder's own
    // rail seam instead of riding the Output graph, exactly as `automation.md` lifts every workspace call.
    audit: (channel) =>
      Effect.map(
        Effect.tryPromise({
          try: () => reference.getOutputDetails(channel),
          catch: DeployFault.triaged(qualified),
        }),
        // The two halves are exclusive by the type's own contract, so the fold is total: a coordinate answers
        // `Published`, a secret-flagged output answers `Sealed`, and a channel the platform never emitted
        // answers `Absent` — which is what turns the platform's no-material law into evidence rather than an
        // assertion, because a `secretValue` reaching this arm IS the law's counterexample, named and reported.
        (details) =>
          details.value !== undefined && details.value !== null
            ? Reading.Published({ channel, value: String(details.value) })
            : details.secretValue !== undefined && details.secretValue !== null
              ? Reading.Sealed({ channel })
              : Reading.Absent({ channel }),
      ),
    // The roster read the per-channel audit fans over: the reference publishes every secret-flagged name, so a
    // platform that leaked one is caught by enumeration rather than by guessing which channel to probe.
    sealed: reference.secretOutputNames,
  }
}

declare namespace Tenants {
  type Platform = ReturnType<typeof _platform>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Reading, Tenants }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

[REPLICATION_SPEC]-[OPEN]: does the committed `crds/capsule` module spell `GlobalTenantResource` under `v1beta2` with `spec.tenantSelector` and `spec.resources[].rawItems`, and does `rawItems` admit a whole typed object rather than a raw-extension string; verification route: the generated `../crds/capsule` module's `GlobalTenantResourceSpecArgs` and `GlobalTenantResourceSpecResourcesArgs` declarations, regenerated from the operator's own CRD at the pinned version.
[ENFORCE_REGISTRIES]-[OPEN]: does `Tenant.spec.rules[].enforce.workloads.registries` carry an `allowed` list beside its regex twin under the pinned operator, and does a rule entry admit an absent `audience`/`namespaceSelector`; verification route: the generated `../crds/capsule` `TenantSpecRulesArgs` chain, since the CRD's own description marks the rule construct not final.
