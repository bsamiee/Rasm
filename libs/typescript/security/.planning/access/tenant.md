# [SECURITY_TENANT]

The tenancy contract: the ambient reference the request's active `TenantContext` rides, the `SessionCoordinate` vocabulary of session GUCs the data wave pins, and the metric-tagging aspect that makes every security instrument per-tenant sound. Tenancy is one core value — `TenantContext`, the `(app, tenant)` pair with its derived `scope` partition key — and this page never re-mints it; it owns the request-scoped BINDING, the coordinate vocabulary, and the telemetry tag, so a tenant never travels as a bare string past a seam, every downstream query inherits the boundary with no parameter threading, and every folder metric a request emits carries its tenant dimension through one aspect. `TenantScope` is a `Context.Reference` whose default is the unauthenticated scope; the edge binds the request's `TenantContext` and subject once, the data wave reads it to pin each `SessionCoordinate` row inside its transaction transformer — a `security/access/tenant → data` [SHAPE] seam — and `TenantScope.metered` wraps any effect so its metrics land `tenant`-tagged, the hook the runtime OTLP lane exports unchanged. The contract is a value, never configuration prose: each GUC name is one coordinate row both this page's projection and the data wave's RLS policy predicate read, so a rename lands once and a new session coordinate (shard key, search-path override, region) is one row plus its derivation over the same `Principal`. This page holds no SQL: the data wave owns the `within` transaction seam and the policy DDL, and hundreds of apps under mixed isolation are rows in the data wave's store map, never deployments of different code.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            | [PUBLIC]       |
| :-----: | :---------------- | :----------------------------------------------------------------- | :------------- |
|  [01]   | `SCOPE_BINDING`   | the ambient tenancy reference, its request-scope provision, the metric tag aspect | `TenantScope`  |
|  [02]   | `RLS_CONTRACT`    | the session-coordinate GUC vocabulary and the per-row projection   | `SessionCoordinate`, `TENANT_GUC` |

## [2]-[SCOPE_BINDING]

[SCOPE_BINDING]:
- Owner: `TenantScope` — a `Context.Reference` carrying the request's active `TenantContext` (as `Option`, `none` for an unauthenticated or single-tenant-pinned request) and the acting subject; the statics ride the class: `bind` provides it for a request scope, the reference itself is the read (`yield* TenantScope`), `scoped` runs an effect under the resolved principal, `scopeOf` projects the partition key, and `metered` is the telemetry aspect — it reads the bound principal and tags every metric the wrapped effect emits with the `tenant` dimension, falling back to the unscoped label, so the folder's counters and timers stay per-tenant sound when thousands of apps share the library. The reference is the one hop from request altitude to every tenant-bounded read: the edge sets it once from the resolved claim, and no query threads a tenant parameter.
- Law: the value is the core `TenantContext`, never a security-local re-declaration — its `scope` key partitions the data wave's per-tenant store Layers and keys `LayerMap`/`HashMap` slots with the structural `Equal` the class carries; a second tenancy notion TS-side is the split-brain the core owner already forbids.
- Law: binding is Layer provision — `bind` is `Effect.provideService` around a request scope, so the ambient tenancy rides the same substitution mechanism as every capability and never becomes a signature tail; a bare string tenant crossing a seam is unspellable because signatures take `TenantContext` or its branded `Key`.
- Law: `metered` is the folder's one tenant-tag seam — a security owner emits plain effect-native `Metric` instruments, the serving edge wraps the request handler once in `TenantScope.metered`, and every instrument inside lands tagged; no owner re-reads the reference for telemetry and no exporter is named here.
- Growth: a new scope dimension (region, deployment ring) is one field on the core `TenantContext` inherited here; a new binding source (an API-key principal, a machine identity) is a caller-composed `TenantContext` provided through the same `bind`.
- Boundary: who mints the `TenantContext` is `access/claim`'s resolution from a verified token or the edge's principal; how a request carries it is edge material; the data wave reads the reference to bind RLS; the runtime wave exports the tagged instruments; this page owns only the ambient seam.
- Packages: `effect` (`Context`, `Effect`, `Option`); `@rasm/ts/core` (`TenantContext`).

```typescript
import { TenantContext } from "@rasm/ts/core"
import { Context, Effect, Option } from "effect"

type Principal = {
  readonly context: Option.Option<TenantContext>
  readonly subject: Option.Option<string>
}

const _UNSCOPED = "unscoped"

class TenantScope extends Context.Reference<TenantScope>()("security/access/TenantScope", {
  defaultValue: (): Principal => ({ context: Option.none(), subject: Option.none() }),
}) {
  static readonly scopeOf = (principal: Principal): Option.Option<TenantContext.Scope> =>
    Option.map(principal.context, (context) => context.scope)
  static readonly bind = <A, E, R>(principal: Principal, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    Effect.provideService(effect, TenantScope, principal)
  static readonly scoped = <A, E, R>(effect: (principal: Principal) => Effect.Effect<A, E, R>): Effect.Effect<A, E, R | TenantScope> =>
    Effect.flatMap(TenantScope, effect)
  static readonly metered = <A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    Effect.flatMap(TenantScope, (principal) =>
      Effect.tagMetrics(effect, "tenant", Option.match(TenantScope.scopeOf(principal), {
        onNone: () => _UNSCOPED,
        onSome: (scope) => String(scope),
      })))
}
```

## [3]-[RLS_CONTRACT]

[RLS_CONTRACT]:
- Owner: `SessionCoordinate` — the session-GUC vocabulary the data wave pins per transaction: one row per coordinate carrying the GUC name and the projection over a bound `Principal`, so `tenant` (`app.current_tenant`, the RLS predicate key), `scope` (`app.current_scope`, the store-map partition), and `subject` (`app.current_subject`, the audit attribution) travel one write path the data wave owns and a new coordinate is one row, never a second contract. `TENANT_GUC` derives from the tenant row — the single anchor the RLS `CREATE POLICY` predicate reads through `current_setting`.
- Law: every projection reads the core `TenantContext` getters — the branded spelling computed from fields already proven by their own patterns — so the one scope spelling can never disagree with its parts, and an unauthenticated principal projects `none` on every row, the fail-closed default RLS reads zero rows under.
- Law: the contract is transport-free — this page never composes `@effect/sql` and never spells `SET LOCAL`; the data wave's transaction transformer folds the coordinate table over the bound principal, pinning each `Some` projection, so search-path, tenant, and audit subject travel one write path.
- Growth: a new session coordinate the data wave pins (a shard key, a search-path override, a region) is one `SessionCoordinate` row; a GUC rename lands once in its row.
- Boundary: the `set_config` write, the RLS `CREATE POLICY` ensure, and the per-isolation Layer construction are all the data wave's; this page declares the names and the projections the enforcement reads.
- Packages: `effect` (`Option`); `@rasm/ts/core` (`TenantContext`).

```typescript
const SessionCoordinate = {
  tenant: {
    guc: "app.current_tenant",
    read: (principal: Principal): Option.Option<string> =>
      Option.map(principal.context, (context) => String(context.tenant)),
  },
  scope: {
    guc: "app.current_scope",
    read: (principal: Principal): Option.Option<string> =>
      Option.map(principal.context, (context) => String(context.scope)),
  },
  subject: {
    guc: "app.current_subject",
    read: (principal: Principal): Option.Option<string> => principal.subject,
  },
} as const satisfies Record<string, { readonly guc: string; readonly read: (principal: Principal) => Option.Option<string> }>

declare namespace SessionCoordinate {
  type Kind = keyof typeof SessionCoordinate
  type Row = (typeof SessionCoordinate)[Kind]
}

const TENANT_GUC: typeof SessionCoordinate.tenant.guc = SessionCoordinate.tenant.guc

// --- [EXPORTS] --------------------------------------------------------------------------

export { SessionCoordinate, TenantScope, TENANT_GUC }
export type { Principal }
```
