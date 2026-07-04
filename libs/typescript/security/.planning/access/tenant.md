# [SECURITY_TENANT]

The tenancy contract: the ambient reference the request's active `TenantContext` rides and the `app.current_tenant` shape the data wave enforces as row-level security. Tenancy is one core value — `TenantContext`, the `(app, tenant)` pair with its derived `scope` partition key — and this page never re-mints it; it owns the request-scoped BINDING and the RLS contract name, so a tenant never travels as a bare string past a seam and every downstream query inherits the boundary with no parameter threading. `TenantScope` is a `Context.Reference` whose default is the unauthenticated scope; the edge binds the request's `TenantContext` and subject once, and the data wave reads it to pin `SET LOCAL app.current_tenant` inside its transaction transformer — a `security/access/tenant → data` [SHAPE] seam. The contract is a value, never configuration prose: the GUC name is one anchor both this page's claim projection and the data wave's RLS policy predicate read, so a rename lands once. This page holds no SQL: it declares the reference, the scope derivation, and the claim name; the data wave owns the `within` transaction seam and the policy DDL, and hundreds of apps under mixed isolation are rows in the data wave's store map, never deployments of different code.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            | [PUBLIC]       |
| :-----: | :---------------- | :----------------------------------------------------------------- | :------------- |
|  [01]   | `SCOPE_BINDING`   | the ambient tenancy reference and its request-scope provision      | `TenantScope`  |
|  [02]   | `RLS_CONTRACT`    | the `app.current_tenant` claim name and the scope-key projection   | `TENANT_GUC`   |

## [2]-[SCOPE_BINDING]

[SCOPE_BINDING]:
- Owner: `TenantScope` — a `Context.Reference` carrying the request's active `TenantContext` (as `Option`, `none` for an unauthenticated or single-tenant-pinned request) and the acting subject; the statics ride the class: `bind` provides it for a request scope, the reference itself is the read (`yield* TenantScope`), `scoped` runs an effect under the resolved principal, and `scopeOf` projects the partition key. The reference is the one hop from request altitude to every tenant-bounded read: the edge sets it once from the resolved claim, and no query threads a tenant parameter.
- Law: the value is the core `TenantContext`, never a security-local re-declaration — its `scope` key partitions the data wave's per-tenant store Layers and keys `LayerMap`/`HashMap` slots with the structural `Equal` the class carries; a second tenancy notion TS-side is the split-brain the core owner already forbids.
- Law: binding is Layer provision — `bind` is `Effect.provideService` around a request scope, so the ambient tenancy rides the same substitution mechanism as every capability and never becomes a signature tail; a bare string tenant crossing a seam is unspellable because signatures take `TenantContext` or its branded `Key`.
- Growth: a new scope dimension (region, deployment ring) is one field on the core `TenantContext` inherited here; a new binding source (an API-key principal, a machine identity) is a caller-composed `TenantContext` provided through the same `bind`.
- Boundary: who mints the `TenantContext` is `access/claim`'s resolution from a verified token or the edge's principal; how a request carries it is edge material; the data wave reads the reference to bind RLS; this page owns only the ambient seam.
- Packages: `effect` (`Context`, `Effect`, `Option`); `@rasm/ts/core` (`TenantContext`).

```typescript
import { TenantContext } from "@rasm/ts/core"
import { Context, Effect, Option } from "effect"

type Principal = {
  readonly context: Option.Option<TenantContext>
  readonly subject: Option.Option<string>
}

class TenantScope extends Context.Reference<TenantScope>()("security/access/TenantScope", {
  defaultValue: (): Principal => ({ context: Option.none(), subject: Option.none() }),
}) {
  static readonly scopeOf = (principal: Principal): Option.Option<TenantContext.Scope> =>
    Option.map(principal.context, (context) => context.scope)
  static readonly bind = <A, E, R>(principal: Principal, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    Effect.provideService(effect, TenantScope, principal)
  static readonly scoped = <A, E, R>(effect: (principal: Principal) => Effect.Effect<A, E, R>): Effect.Effect<A, E, R | TenantScope> =>
    Effect.flatMap(TenantScope, effect)
}
```

## [3]-[RLS_CONTRACT]

[RLS_CONTRACT]:
- Owner: the tenancy-contract vocabulary — `TENANT_GUC` is the single `app.current_tenant` claim-name anchor the data wave's RLS policy predicate reads through `current_setting`, and `TenantScope.scopeOf` projects a bound `Principal` into the branded `TenantContext.Scope` the data wave keys its store map on. The GUC name lives here because the tenancy shape is a security contract; the data wave enforces it and never re-declares it.
- Law: `TenantScope.scopeOf` reads the core `TenantContext.scope` getter — the branded `app/tenant` spelling computed from fields already proven by their own patterns — so the one scope spelling can never disagree with its parts, and an unauthenticated principal projects `none`, the fail-closed default RLS reads zero rows under.
- Law: the contract is transport-free — this page never composes `@effect/sql` and never spells `SET LOCAL`; the data wave's transaction transformer pins the GUC from `active`, so search-path and tenant travel one write path the data wave owns.
- Growth: a new session coordinate the data wave pins (a shard key, a search-path override) is one derivation over the same `Principal`; the GUC name never changes shape.
- Boundary: the `set_config('app.current_tenant', ...)` write, the RLS `CREATE POLICY` ensure, and the per-isolation Layer construction are all the data wave's; this page declares the name and the projection the enforcement reads.
- Packages: `effect` (`Effect`, `Option`); `@rasm/ts/core` (`TenantContext`).

```typescript
const TENANT_GUC = "app.current_tenant" as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { TenantScope, TENANT_GUC }
export type { Principal }
```
