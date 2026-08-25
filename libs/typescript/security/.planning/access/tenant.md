# [SECURITY_TENANT]

Tenancy contract: the ambient reference the request's active `Identity.Tenant` rides, the `SessionCoordinate` vocabulary of session GUCs the data wave pins, and the metric-tagging aspect that makes every security instrument per-tenant sound. Tenancy is one core value — `Identity.Tenant`, the `(app, tenant)` pair with its derived `scope` partition key — and this page never re-mints it; it owns the request-scoped BINDING, the coordinate vocabulary, and the telemetry tag, so a tenant never travels as a bare string past a seam, every downstream query inherits the boundary with no parameter threading, and every folder metric a request emits carries its tenant dimension through one aspect. `TenantScope` is a `Context.Reference` whose default is the unauthenticated scope; the edge binds the request's `Identity.Tenant` and subject once, the data wave reads it to pin each `SessionCoordinate` row inside its transaction transformer — a `security/access/tenant → data` [SHAPE] seam — and `TenantScope.metered` wraps any effect so its metrics land tagged under the core `Convention.rasm.tenant` key — the dimension the runtime export lane's tenant metric-view row governs under its cardinality ceiling. This contract is a value, never configuration prose: each GUC name is one coordinate row both this page's projection and the data wave's RLS policy predicate read, so a rename lands once and a new session coordinate (shard key, search-path override, region) is one row with its derivation over the same `Principal`. This page holds no SQL: the data wave owns the `within` transaction seam and the policy DDL, and hundreds of apps under mixed isolation are rows in the data wave's store map, never deployments of different code.

## [01]-[INDEX]

- [02]-[SCOPE_BINDING]: ambient tenancy reference, principal mint, request-scope provision, metric tag aspect; `TenantScope`.
- [03]-[RLS_CONTRACT]: session-coordinate GUC vocabulary and per-row projection; `SessionCoordinate`.

## [02]-[SCOPE_BINDING]

[SCOPE_BINDING]:
- Law: every bound `Principal` mints here, and `of` takes both axes as optionals because the construction sites differ on each — the data wave's explicit-tenant transformer names a context and omits the subject, `access/claim` names a subject whose tenancy is an `Option`, and a machine-key admission names a subject under no tenancy at all. One member answers all three, so a coordinate the shape acquires reaches every construction site at once instead of only the ones that remembered, and an inline `{ context, subject }` literal at any call is the drift this law forbids.
- Law: `metered` is the folder's one tenant-tag seam — a security owner emits plain effect-native `Metric` instruments, the serving edge wraps the request handler once in `TenantScope.metered`, and every instrument inside lands tagged; no owner re-reads the reference for telemetry and no exporter is named here.
- Law: per-tenant series ride governed — `metered` tags with the core `Convention.rasm.tenant` key, the one dimension the runtime export lane's tenant metric-view row admits under its cardinality ceiling, so the per-tenant fan is bounded at the exporter and a free-string tenant key that dodges the governor is unspellable at this seam.
- Packages: `effect` (`Context`, `Effect`, `Option`); `@rasm/core` (`Convention`, `Identity.Tenant`).

```typescript signature
import { Convention, Identity } from "@rasm/core"
import { Context, Effect, Option } from "effect"

type Principal = {
  readonly context: Option.Option<Identity.Tenant>
  readonly subject: Option.Option<string>
}

const _UNSCOPED = "unscoped"

class TenantScope extends Context.Reference<TenantScope>()("security/access/TenantScope", {
  defaultValue: (): Principal => ({ context: Option.none(), subject: Option.none() }),
}) {
  static readonly scopeOf = (principal: Principal): Option.Option<Identity.Tenant.Scope> =>
    Option.map(principal.context, (context) => context.scope)
  static readonly bind = <A, E, R>(principal: Principal, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    Effect.provideService(effect, TenantScope, principal)
  static readonly of = (context?: Identity.Tenant, subject?: string): Principal => ({
    context: Option.fromNullable(context),
    subject: Option.fromNullable(subject),
  })
  static readonly scoped = <A, E, R>(effect: (principal: Principal) => Effect.Effect<A, E, R>): Effect.Effect<A, E, R | TenantScope> =>
    Effect.flatMap(TenantScope, effect)
  static readonly metered = <A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    Effect.flatMap(TenantScope, (principal) =>
      Effect.tagMetrics(effect, Convention.rasm.tenant, Option.match(TenantScope.scopeOf(principal), {
        onNone: () => _UNSCOPED,
        onSome: (scope) => String(scope),
      })))
}
```

## [03]-[RLS_CONTRACT]

[RLS_CONTRACT]:
- Owner: `SessionCoordinate` — the session-GUC vocabulary the data wave pins per transaction: one row per coordinate carrying the GUC name and the projection over a bound `Principal`, so `tenant` (the RLS predicate key, projected off the core `Convention.rasm.tenant` symbol this page never respells), `scope` (`rasm.scope`, the store-map partition), and `subject` (`rasm.subject`, the audit attribution) travel one write path the data wave owns and a new coordinate is one row, never a second contract. Consumers read `SessionCoordinate.tenant.guc` directly — the tenant row's `guc` is the single anchor the RLS `CREATE POLICY` predicate reads through `current_setting`; one hop, no promotion alias. `plane` (`rasm.plane`, the maintenance-plane posture) is the one coordinate no principal projects: the data wave's maintenance transformer pins it and the RLS policy's plane arm reads it, so an estate-wide sweep is a stated admission, never a role accident.
- Law: the contract is transport-free — this page never composes `@effect/sql` and never spells `SET LOCAL`; the data wave's transaction transformer folds the coordinate table over the bound principal, pinning each `Some` projection, so search-path, tenant, and audit subject travel one write path.
- Growth: a new session coordinate the data wave pins (a shard key, a search-path override, a region) is one `SessionCoordinate` row; a GUC rename lands once in its row.
- Law: the plane row is fail-closed like every coordinate — unset folds to NULL under `current_setting(name, true)`, a principal-pinned transaction never carries it because its projection answers `None` for every principal, and its `value` is the row's own published constant, so the policy arm and the maintenance transformer spell one word from one seat.
- Boundary: the `set_config` write, the RLS `CREATE POLICY` ensure, and the per-isolation Layer construction are all the data wave's; this page declares the names and the projections the enforcement reads.
- Packages: `effect` (`Option`); `@rasm/core` (`Identity.Tenant`).

```typescript signature
const SessionCoordinate = {
  tenant: {
    guc: Convention.rasm.tenant,
    read: (principal: Principal): Option.Option<string> =>
      Option.map(principal.context, (context) => String(context.tenant)),
  },
  scope: {
    guc: "rasm.scope",
    read: (principal: Principal): Option.Option<string> =>
      Option.map(principal.context, (context) => String(context.scope)),
  },
  subject: {
    guc: "rasm.subject",
    read: (principal: Principal): Option.Option<string> => principal.subject,
  },
  plane: {
    guc: "rasm.plane",
    value: "maintenance",
    read: (): Option.Option<string> => Option.none(),
  },
} as const

declare namespace SessionCoordinate {
  type Kind = keyof typeof SessionCoordinate
  type Row = (typeof SessionCoordinate)[Kind]
  type _Rows<T extends Record<string, { readonly guc: string; readonly read: (principal: Principal) => Option.Option<string> }> = typeof SessionCoordinate> = T
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { SessionCoordinate, TenantScope }
export type { Principal }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
