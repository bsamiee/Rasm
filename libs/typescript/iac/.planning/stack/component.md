# [IAC_COMPONENT]

The ComponentResource tier discipline: every grouped concern in the deploy plane is one `Tier` subclass — one type token under the `rasm:iac:` scope, one inherited option fold, one terminal output registration — so the resource DAG is recoverable from the tier tree alone and no child resource is ever constructed with hand-assembled options. `Tier` is the folder's one boundary adapter over Pulumi's class-based resource model: the engine demands subclass constructors with assignment bodies, so the constructor is the sanctioned statement seam and everything a tier computes beyond construction stays expression-shaped. Providers flow down the parent chain, `protect` marks the stateful tiers, `aliases` carry renames without replacement, and `registerOutputs` closes every constructor so the engine settles the component's children before dependents proceed. The module is `iac/src/stack/component.ts`; a new tier is one subclass row in its owning page, never a change here.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                          | [PUBLIC] |
| :-----: | :--------------- | :---------------------------------------------------------------- | :------- |
|  [01]   | `TIER_BASE`      | the abstract owner: type token, option fold, output registration | `Tier`   |
|  [02]   | `TIER_ROSTER`    | the folder's tier tree and its ownership rows                    | `Tier`   |

## [2]-[TIER_BASE]

[TIER_BASE]:
- Owner: `Tier`, the abstract `pulumi.ComponentResource` subclass every tier extends — the constructor stamps the type token `rasm:iac:<Kind>`, `child(overrides?)` folds `{ parent: this }` into per-resource overrides through `pulumi.mergeOptions` so ownership is inherited and never restated, and `seal(outputs)` is the mandatory terminal `registerOutputs` call.
- Law: the constructor is the platform seam — Pulumi's model is class-heritage with field assignment, so `super(...)`, child construction, and readonly field assignment are the exemption's whole extent; a tier method beyond the constructor is an expression-shaped projection over already-constructed outputs.
- Law: options are algebra, not assembly — `child()` is the only way a child receives options: `parent` rides the fold, an explicit `provider`/`providers` set at tier construction flows down the chain, `dependsOn` states only genuine extra-graph edges (an `Output` reference already is one), `protect: true` marks tiers owning irreplaceable state (the data and object tiers), `aliases` accompany a rename so state survives it, and `ignoreChanges` quarantines fields an operator mutates out-of-band.
- Law: `seal` closes every constructor — an unsealed tier reports no outputs and its dependents race construction; the sealed record is the tier's public evidence and mirrors the readonly fields the class exposes.
- Law: adoption is not composition — a `ComponentResource` has no `static get`; a pre-existing cloud object adopts through its own resource class `get` or `opts.import` inside the owning tier, and the tier remains the sole author thereafter.
- Growth: a new cross-tier option posture is one `child` override row at the tier that needs it; the base never grows knobs.
- Boundary: which tiers exist is `[3]`'s roster; the engine's option semantics (`CustomResourceOptions`, `mergeOptions` overloads) are `@pulumi/pulumi` facts consumed verbatim.
- Packages: `@pulumi/pulumi` (`ComponentResource`, `ComponentResourceOptions`, `CustomResourceOptions`, `mergeOptions`, `Inputs`).

```typescript
import * as pulumi from "@pulumi/pulumi"

abstract class Tier extends pulumi.ComponentResource {
  constructor(kind: string, name: string, opts?: pulumi.ComponentResourceOptions) {
    super(`rasm:iac:${kind}`, name, {}, opts)
  }
  protected child(overrides?: pulumi.CustomResourceOptions): pulumi.CustomResourceOptions {
    return pulumi.mergeOptions({ parent: this }, overrides)
  }
  protected seal(outputs: pulumi.Inputs): void {
    this.registerOutputs(outputs)
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Tier }
```

## [3]-[TIER_ROSTER]

[TIER_ROSTER]:
- Law: the tier tree is closed and page-owned — each row below is one `Tier` subclass whose declaration, invariants, and fences live on its owning page; the dispatch arm composes tiers by constructor, and a concern with no tier row composes inside an existing tier before a new subclass is minted.
- Law: tier names are the type-token vocabulary — `rasm:iac:<Kind>` with the kind column below — so stack state, policy packs, and drift receipts address tiers by one stable token; renaming a tier is an `aliases` row, never a silent replacement.
- Growth: a new tier is one row here plus its subclass on the owning page; the roster is the folder's whole grouping surface.

| [INDEX] | [KIND]        | [OWNING_PAGE]           | [OWNS]                                                    |
| :-----: | :------------ | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `Bootstrap`   | `provider/surface.md`   | cluster bootstrap over owned metal/VPS; kubeconfig egress  |
|  [02]   | `Secrets`     | `secret/doppler.md`     | Doppler hierarchy, generated material, runtime token       |
|  [03]   | `ObjectStore` | `kube/data.md`          | MinIO-versus-Garage object row; backup destination         |
|  [04]   | `Postgres`    | `kube/data.md`          | CNPG operator + cluster CR; per-app database finalization  |
|  [05]   | `Workload`    | `kube/workload.md`      | typed app workloads: deployment, service, account          |
|  [06]   | `Traffic`     | `kube/traffic.md`       | cert chain, TLS secret sink, ingress, dns/tunnel exposure  |
|  [07]   | `Lgtm`        | `observe/stack.md`      | LGTM stack + OTel collector chart rows; endpoint outputs   |
|  [08]   | `Boards`      | `observe/apply.md`      | grafana provider binding; dashboard/alert/SLO application  |
