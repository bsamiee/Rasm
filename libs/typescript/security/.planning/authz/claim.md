# [SECURITY_CLAIM] — tenant and entitlement claims, and the app.current_tenant tenancy contract

`authz/claim` owns the entitlement and tenancy claim vocabulary: the `Role` table, the resolved `ClaimSet` a verified `AccessClaims` lifts into, and the `TenantContext` reference that is the `app.current_tenant` contract `store` enforces as row-level security. Claims are data — the subject's granted roles and scopes plus the active tenant — resolved from the JWT claims enriched by the `ClaimStore` port the app root satisfies with `store`. The tenancy contract is a `Context.Reference`: the edge binds the request's `TenantContext` once, `store/scope` reads it to `SET LOCAL app.current_tenant`, and every downstream query inherits the tenant boundary with no parameter threading — a `security/authz/claim → store/scope [SHAPE]` seam. This page holds the claims and the tenant; `authz/policy` evaluates them into a decision. `ClaimFault` is the folder fault shape, and a subject with no roles is an empty set, never a fault.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                          | [PACKAGES]                | [REJECTED_FORM]                          |
| :-----: | :------------------------------ | :------------------------------- | :------------------------ | :--------------------------------------- |
|  [01]   | role + claim vocabulary         | `Role` table / `ClaimSet`        | `effect` `Schema`         | a role string per site, a claims DTO     |
|  [02]   | tenancy contract for RLS        | `TenantContext` `Context.Reference` | `effect`               | a tenant parameter threaded per query    |
|  [03]   | resolve claims from a token     | `Claim.resolve` / `Claim.bind`   | `session/jwt`, `store` port | re-deriving roles at each policy check   |

## [2]-[CLAIM_VOCABULARY]

[ROLES_AND_CONTEXT]:
- Owner: `Role` is the bounded role table (rank plus inherited roles for the policy fold); `ClaimSet` is the resolved claim shape (subject, tenant, roles, scopes); `TenantContext` is the ambient tenancy reference. `ClaimStore` declares the role storage.
- Packages: `effect` — `Role` derives its `Kind` through `keyof typeof`, `ClaimSet` decodes roles into a `HashSet`, `TenantContext` is a `Context.Reference` whose default is the unauthenticated context.
- Boundary: `store/scope` reads `TenantContext` to bind RLS (the tenancy seam); `session/token` supplies the `AccessClaims`; `authz/policy` consumes the `ClaimSet`; `ClaimStore` is a `store`-satisfied port.
- Growth: a new role is one `_roles` entry; a new claim facet is one `ClaimSet` field; the tenant boundary never changes shape.

```typescript
import { Config, Context, Effect, HashSet, Option, Schema } from "effect"
import type { AccessClaims } from "../sign/jwt.ts"

// --- [TYPES] ----------------------------------------------------------------------------

const _TenantId = Schema.NonEmptyString.pipe(Schema.brand("TenantId"))
type TenantId = typeof _TenantId.Type

const _roles = ["admin", "member", "viewer"] as const

// --- [CONSTANTS] ------------------------------------------------------------------------

const Role = {
  admin: { rank: 3, inherits: ["member"] },
  member: { rank: 2, inherits: ["viewer"] },
  viewer: { rank: 1, inherits: [] },
} as const

const _reasons = ["store"] as const

const ClaimFaultPolicy = {
  store: { rank: 4, retry: true, status: 503 },
} as const

declare namespace Role {
  type Kind = keyof typeof Role
}

declare namespace ClaimFault {
  type Reason = keyof typeof ClaimFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof ClaimFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class ClaimSet extends Schema.Class<ClaimSet>("ClaimSet")({
  subject: Schema.UUID,
  tenant: Schema.optionalWith(_TenantId, { as: "Option" }),
  roles: Schema.HashSet(Schema.Literal(..._roles)),
  scopes: Schema.HashSet(Schema.NonEmptyString),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class ClaimFault extends Schema.TaggedError<ClaimFault>()("ClaimFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): ClaimFault.Row {
    return ClaimFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<claim:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class TenantContext extends Context.Reference<TenantContext>()("security/authz/TenantContext", {
  defaultValue: (): { readonly tenant: Option.Option<TenantId>; readonly subject: Option.Option<string> } =>
    ({ tenant: Option.none(), subject: Option.none() }),
}) {}

class ClaimStore extends Context.Tag("security/authz/ClaimStore")<ClaimStore, {
  readonly rolesOf: (subject: string, tenant: Option.Option<TenantId>) => Effect.Effect<HashSet.HashSet<Role.Kind>, ClaimFault>
  readonly grant: (subject: string, tenant: Option.Option<TenantId>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
  readonly revoke: (subject: string, tenant: Option.Option<TenantId>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
}>() {}
```

## [3]-[RESOLUTION]

[CLAIM]:
- Owner: `Claim.resolve` lifts a verified `AccessClaims` into a `ClaimSet` — subject and scopes from the token, roles from the `ClaimStore` keyed by `(subject, tenant)`; `Claim.bind` provides the `TenantContext` for a request scope so `store` binds RLS; `Claim.context` derives the reference value from a `ClaimSet`.
- Packages: `session/jwt` `AccessClaims` as the token source, `store`-satisfied `ClaimStore` for the durable roles, `effect` `Config` for the default tenant policy.
- Law: roles are read once per request into the `ClaimSet`, never re-derived at each policy check; the tenant flows through the `Context.Reference`, not a parameter; an empty role set is a valid unprivileged subject, and only a store failure is a `ClaimFault`.
- Receipt: `ClaimSet` — the immutable resolved claim the policy fold reads; `TenantContext` — the ambient tenancy the store binds.

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Claim extends Effect.Service<Claim>()("security/authz/Claim", {
  effect: Effect.gen(function* () {
    const store = yield* ClaimStore
    const fallbackTenant = yield* Config.option(Config.string("DEFAULT_TENANT"))
    const _tenantOf = (claims: AccessClaims): Effect.Effect<Option.Option<TenantId>> =>
      Option.match(claims.tid, {
        onSome: (raw) => Schema.decode(_TenantId)(raw).pipe(Effect.option),
        onNone: () => Option.match(fallbackTenant, { onSome: (raw) => Schema.decode(_TenantId)(raw).pipe(Effect.option), onNone: () => Effect.succeed(Option.none()) }),
      })
    const resolve = (claims: AccessClaims): Effect.Effect<ClaimSet, ClaimFault> =>
      Effect.gen(function* () {
        const tenant = yield* _tenantOf(claims)
        const roles = yield* store.rolesOf(claims.sub, tenant)
        return new ClaimSet({ subject: claims.sub, tenant, roles, scopes: HashSet.fromIterable(claims.scope) })
      })
    const context = (claims: ClaimSet): Context.Tag.Service<TenantContext> => ({ tenant: claims.tenant, subject: Option.some(claims.subject) })
    const bind = <A, E, R>(claims: ClaimSet, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      Effect.provideService(effect, TenantContext, context(claims))
    return { resolve, context, bind } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Claim, ClaimFault, ClaimSet, ClaimStore, Role, TenantContext }
```
