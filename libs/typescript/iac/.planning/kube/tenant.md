# [IAC_TENANT]

Tenant isolation on the `selfhosted-k8s` arm as one tier over one dispatch: `Tenants` realizes the spec's tenancy mode through the `_MODES` handler record — `namespace` installs Capsule once and mints one typed `Tenant` CR per tenant slug (policy-governed soft isolation: RBAC, `NetworkPolicy`, and `ResourceQuota` propagation over vanilla namespaces at the highest density), `vcluster` realizes one virtual control plane per tenant (hard isolation for the untrusted or external tenant that needs its own API surface, CRDs, and version skew) — so escalating a tenant's isolation is a spec delta the record interprets, never a second program body. The Capsule classes are committed `crd2pulumi` output; the vcluster row is one chart per tenant. The cross-stack seam rides the same page: `Tenants.platform` wraps `StackReference` so a tenant stack reads the platform stack's published planes (`getOutput`/`requireOutput` over the same `StackOutputs` channel vocabulary), which is how a tenant-per-stack estate composes against one shared platform stack without either importing the other. The data-plane escalation this page's modes pair with is `kube/data.md`'s `_TENANCY` record (`shared-rls`/`db-per-tenant`/`cluster-per-tenant`), tenant secret access is `operate/secret.md`'s `_ACCESS` rows, tenant boards are `operate/observe.md`'s per-tenant organizations, and tenant vanity hostnames are `kube/traffic.md`'s `vanity` rows — one tenancy axis, five owners reading it. The module is `iac/src/kube/tenant.ts`; a new isolation mode is one `_MODES` row, a new tenant is one spec slug.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        | [PUBLIC]  |
| :-----: | :--------------- | :-------------------------------------------------------------- | :-------- |
|  [01]   | `ISOLATION_MODES`| the mode dispatch: Capsule tenancy rows, vcluster planes        | `Tenants` |
|  [02]   | `PLATFORM_SEAM`  | the cross-stack read: platform outputs into tenant programs     | `Tenants` |

## [2]-[ISOLATION_MODES]

[ISOLATION_MODES]:
- Owner: `Tenants` — the `_MODES` record keyed by the escalated tenancy modes (`single` never reaches this tier; the arm gates construction), exhaustive by mapped annotation so a new mode literal in the spec fails compilation here until its row lands; the `namespace` row installs the Capsule chart once (`skipCrds: false`) and folds one `Tenant` CR per slug — owner binding to the tenant's group identity, a namespace quota, and Capsule's propagated `NetworkPolicy`/`ResourceQuota` governance riding the CR's typed spec; the `vcluster` row mints one namespace per tenant and realizes one `helm.v4.Chart` inside it, each a full virtual control plane whose kubeconfig egresses through the chart's own secret convention; both rows receive the tier's option fold as a scope callback, so ownership threads without a public option surface.
- Law: tenancy is policy rows, never bespoke paths — a tenant is a slug in `spec.profile.tenancy.tenants`; everything realized for it (Tenant CR, vcluster release, database, secret access, organization, vanity hostname) derives from that one vocabulary, and per-tenant special-casing in any tier is the split this owner exists to forbid.
- Law: the isolation ladder is deliberate — `namespace` (Capsule) is the default escalation: highest density, one API server, policy-enforced boundaries; `vcluster` is the hard row for the tenant whose workloads are untrusted, need their own CRD estate, or must version-skew from the host plane; choosing per-tenant rather than per-estate isolation mixes is a spec-shape growth this record absorbs as a row parameter, not a new owner.
- Law: the tenant's blast radius is closed from both sides — Capsule governs what the tenant's namespaces may express, the `kube/traffic.md` fence governs what reaches them, `operate/policy.md`'s rows judge what they ship, and the PKO loop (`operate/policy.md` `Reconcile`) executes tenant-submitted desired state inside the same envelope, so self-service provisioning never widens the boundary.
- Entry: `new Tenants("tenants", { spec, versions }, opts)` inside the k8s arm when `tenancy.mode !== "single"`.
- Growth: a new isolation mode is one `_MODES` row; a Capsule governance axis (allowed registries, node selectors, ingress classes) is one field on the Tenant CR spec fold.
- Boundary: the Capsule and vcluster chart values drift with their pins; the generated `crds/capsule` module regenerates on operator bumps; the PG tier escalation is `kube/data.md`'s record.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `../crds/capsule` (`v1beta2.Tenant` — crd2pulumi); `effect` (`Array`); `../program/spec.ts` (`StackSpec`, `Tier`).

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

const _MODES: {
  readonly [K in Exclude<StackSpec.Tenancy["mode"], "single">]: (args: Tenants.Args, scope: Tenants.Scope) => void
} = {
  namespace: (args, scope) => {
    const governor = new k8s.helm.v4.Chart("capsule", {
      chart: "capsule",
      repositoryOpts: { repo: "https://projectcapsule.dev/charts" },
      version: args.versions.capsule,
      skipCrds: false,
    }, scope())
    Array.map(args.spec.tenants, (tenant) =>
      new capsule.v1beta2.Tenant(tenant, {
        metadata: { name: tenant },
        spec: {
          owners: [{ name: `${tenant}-admin`, kind: "Group" }],
          namespaceOptions: { quota: 5 },
          networkPolicies: {
            items: [{
              policyTypes: ["Ingress"],
              podSelector: {},
              ingress: [{ from: [{ namespaceSelector: { matchLabels: { "capsule.clastix.io/tenant": tenant } } }] }],
            }],
          },
        },
      }, scope({ dependsOn: [governor] })))
  },
  vcluster: (args, scope) =>
    void Array.map(args.spec.tenants, (tenant) => {
      const home = new k8s.core.v1.Namespace(tenant, { metadata: { name: tenant } }, scope())
      return new k8s.helm.v4.Chart(`${tenant}-plane`, {
        chart: "vcluster",
        repositoryOpts: { repo: "https://charts.loft.sh" },
        version: args.versions.vcluster,
        namespace: home.metadata.name,
        values: {
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

## [3]-[PLATFORM_SEAM]

[PLATFORM_SEAM]:
- Owner: `Tenants.platform` — the one `StackReference` wrap for the multi-stack estate: a tenant stack's program calls `Tenants.platform(qualified)` and reads the platform stack's published planes through `output(channel)` (`Option`-shaped absence) or `require(channel)` (fail-loud), where `channel` is the same `<plane>.<field>` spelling `StackOutputs.pairsOf` mints — the cross-stack vocabulary and the env vocabulary are one vocabulary, so a platform rename breaks both consumers at one spelling.
- Law: the read is coordinates only — the platform publishes no material (its own `StackOutputs.read` gate guarantees it), so a tenant stack composes hosts, origins, and endpoints, and a tenant needing a credential routes through its own Doppler access rows, never a cross-stack secret read.
- Law: platform-stack-per-estate, tenant-stack-per-tenant is the sharded topology — the platform stack realizes the shared tiers (data, fanout, observe), each tenant stack realizes its own workload/traffic against the platform's read planes, and `getOutputDetails` serves the audit path when evidence must name the upstream version.
- Entry: `const platform = Tenants.platform("org/rasm/platform")` inside a tenant stack's `PulumiFn`; `platform.require("otlp.endpoint")` into the tenant workload env.
- Growth: a new cross-stack fact is one published plane on the platform spec — this seam inherits it with zero edits.
- Boundary: which planes exist is `spec.md`'s owner; who runs tenant stacks (deploy host, PKO, review stack) is orthogonal to this read.
- Packages: `@pulumi/pulumi` (`StackReference`); `effect` (`Option`).

```typescript
import { Option } from "effect"

const _platform = (qualified: string): {
  readonly output: (channel: string) => pulumi.Output<Option.Option<string>>
  readonly require: (channel: string) => pulumi.Output<string>
} => {
  const reference = new pulumi.StackReference(qualified)
  return {
    output: (channel) =>
      reference.getOutput(channel).apply((value: unknown) =>
        typeof value === "string" ? Option.some(value) : Option.none()),
    require: (channel) => reference.requireOutput(channel).apply(String),
  }
}

declare namespace Tenants {
  type Platform = ReturnType<typeof _platform>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Tenants }
```
