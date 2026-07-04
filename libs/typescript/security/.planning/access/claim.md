# [SECURITY_CLAIM]

The one authorization owner: the entitlement vocabulary a verified token resolves into and the RBAC-union-ReBAC fold that evaluates it into a decision. Claims are data — the subject's granted roles and scopes plus the active tenant — resolved once per request from the `crypt/sign` `AccessClaims`, enriched by the `ClaimStore` port the app root satisfies with the data wave; the tenancy key pairs with the boot `AppIdentity` into the core `TenantContext` the `access/tenant` reference carries, so `store` binds RLS with no parameter threading. `Policy.check` folds three sources into one `PolicyDecision`: RBAC grants derive from the `RolePermission` table over the inherited-role closure, ReBAC grants from a `RelationTuple` the `RelationStore` port checks, and a feature-flag gate delegates verdict evaluation to the runtime wave through the `FlagGate` consumer port — the `security → runtime` edge the ledger licenses. One `check` folds all three: a permission is granted when RBAC or ReBAC allows it and the flag gate is open, so the next action, role, or relation is a table row, never a new branch. The decision is a tagged verdict carrying its denial reason, never a bare boolean; `ClaimFault`/`PolicyFault` instantiate the folder fault shape and fire only when a store or verdict is unreachable — a denial is a decision and an empty role set is a valid unprivileged subject.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                             | [PUBLIC]                                  |
| :-----: | :------------------ | :------------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `CLAIM_VOCABULARY`  | the `Role` table, `ClaimSet`, the `ClaimStore` port, the fault      | `Role`, `ClaimSet`, `ClaimFault`          |
|  [02]   | `CLAIM_RESOLUTION`  | `Claim.resolve` token→claims, `Claim.principal` tenancy binding     | `Claim`                                   |
|  [03]   | `POLICY_VOCABULARY` | permission/relation tables, `PolicyDecision`, relation + flag ports | `PolicyDecision`, `RelationStore`, `FlagGate` |
|  [04]   | `POLICY_EVALUATION` | the RBAC ∪ ReBAC fold under the flag gate                           | `Policy`, `PolicyFault`                    |

## [2]-[CLAIM_VOCABULARY]

[CLAIM_VOCABULARY]:
- Owner: `Role` is the bounded role table — each row carries `rank` and inherited roles for the closure fold, the discriminant derives through `keyof typeof`; `ClaimSet` is the resolved claim shape (subject, tenant key, roles, scopes); `ClaimStore` is the durable-role port; `ClaimFault` is the folder fault shape at the store boundary. The tenant is the core `TenantContext.Key` brand — never a security-local re-declaration.
- Law: roles decode into a `HashSet` so the policy fold reads membership structurally; an empty role set is a valid unprivileged subject, and only a store failure is a `ClaimFault` (class `unavailable`, the branch `Budget` retries it).
- Law: the `_roles` tuple anchors the key set — `ClaimSet.roles` and `RolePermission`'s keys both derive from it, so a role without its table row (or the converse) fails at the declaration.
- Growth: a new role is one `_roles` entry plus its `Role` row and `RolePermission` cells; a new claim facet is one `ClaimSet` field the store persists.
- Boundary: `crypt/sign` owns the `AccessClaims` the edge's verify hands in; `access/tenant` owns the `TenantScope` reference the tenancy binds; `ClaimStore` is a data-wave-satisfied port; the policy fold below consumes the `ClaimSet`.
- Packages: `effect` (`Schema`, `Context`, `HashSet`); `@rasm/ts/core` (`TenantContext`, `FaultClass`); `crypt/sign` (`AccessClaims`); `access/tenant` (`TenantScope`, `Principal`).

```typescript
import { AppIdentity, FaultClass, TenantContext } from "@rasm/ts/core"
import { Array, Config, Context, Data, Effect, HashSet, Option, Schema } from "effect"
import { AccessClaims } from "../crypt/sign.ts"
import { type Principal, TenantScope } from "./tenant.ts"

const _roles = ["admin", "member", "viewer"] as const

const Role = {
  admin: { rank: 3, inherits: ["member"] },
  member: { rank: 2, inherits: ["viewer"] },
  viewer: { rank: 1, inherits: [] },
} as const satisfies Record<(typeof _roles)[number], { readonly rank: number; readonly inherits: ReadonlyArray<(typeof _roles)[number]> }>

const _claimReasons = ["store"] as const
const _claimFaults = { store: { class: "unavailable" } } as const

declare namespace Role {
  type Kind = keyof typeof Role
  type _Keys<K extends Kind = (typeof _roles)[number]> = K
}

declare namespace ClaimFault {
  type Reason = keyof typeof _claimFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _claimFaults> = T
}

class ClaimSet extends Schema.Class<ClaimSet>("ClaimSet")({
  subject: Schema.NonEmptyString,
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
  roles: Schema.HashSet(Schema.Literal(..._roles)),
  scopes: Schema.HashSet(Schema.NonEmptyString),
}) {}

class ClaimFault extends Schema.TaggedError<ClaimFault>()("ClaimFault", {
  reason: Schema.Literal(..._claimReasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _claimFaults[this.reason].class
  }
  override get message(): string {
    return `<claim:${this.reason}> ${this.detail}`
  }
}

class ClaimStore extends Context.Tag("security/access/ClaimStore")<ClaimStore, {
  readonly rolesOf: (subject: string, tenant: Option.Option<TenantContext.Key>) => Effect.Effect<HashSet.HashSet<Role.Kind>, ClaimFault>
  readonly grant: (subject: string, tenant: Option.Option<TenantContext.Key>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
  readonly revoke: (subject: string, tenant: Option.Option<TenantContext.Key>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
}>() {}
```

## [3]-[CLAIM_RESOLUTION]

[CLAIM_RESOLUTION]:
- Owner: `Claim.resolve` lifts a verified `AccessClaims` into a `ClaimSet` — subject and scopes from the token, tenant from the `tid` claim decoded against `TenantContext.Key`, roles from the `ClaimStore` keyed by `(subject, tenant)`; `Claim.principal` pairs the claim's tenant key with the boot `AppIdentity.app` into the core `TenantContext` and packs it into a `Principal`; `Claim.bind` provides the `TenantScope` reference for a request scope so the data wave binds RLS.
- Law: roles are read once per request into the `ClaimSet`, never re-derived at each policy check; the tenant flows through the `TenantScope` reference, not a parameter; a `tid` that fails to decode falls to the configured default tenant, and only a store failure is a `ClaimFault`.
- Law: tenancy construction rides the core owner's method — `AppIdentity.scoped(tenantKey)` builds the `TenantContext`, so the one scope spelling can never disagree with its parts and no security-local tenancy value exists.
- Receipt: `ClaimSet` — the immutable resolved claim the policy fold reads; `Principal` — the ambient tenancy the reference carries.
- Growth: a new claim source (an API-key principal) resolves into the same `ClaimSet` and binds through the same `Principal`.
- Boundary: the edge hands the verified `AccessClaims` and the boot `AppIdentity`; the data wave satisfies `ClaimStore`; `access/tenant` owns the reference; `Policy` consumes the `ClaimSet`.
- Packages: `effect` (`Config`, `Effect`, `HashSet`, `Option`, `Schema`); `@rasm/ts/core` (`AppIdentity`, `TenantContext`); `crypt/sign` (`AccessClaims`); `access/tenant` (`TenantScope`, `Principal`).

```typescript
class Claim extends Effect.Service<Claim>()("security/access/Claim", {
  effect: Effect.gen(function* () {
    const store = yield* ClaimStore
    const fallback = yield* Config.option(Config.string("DEFAULT_TENANT"))
    const _tenantOf = (claims: AccessClaims): Effect.Effect<Option.Option<TenantContext.Key>> =>
      Option.match(claims.tid, {
        onSome: (raw) => Schema.decode(TenantContext.fields.tenant)(raw).pipe(Effect.option),
        onNone: () => Option.match(fallback, { onSome: (raw) => Schema.decode(TenantContext.fields.tenant)(raw).pipe(Effect.option), onNone: () => Effect.succeed(Option.none()) }),
      })
    const resolve = (claims: AccessClaims): Effect.Effect<ClaimSet, ClaimFault> =>
      Effect.gen(function* () {
        const tenant = yield* _tenantOf(claims)
        const roles = yield* store.rolesOf(claims.sub, tenant)
        return new ClaimSet({ subject: claims.sub, tenant, roles, scopes: HashSet.fromIterable(claims.scope) })
      })
    const principal = (identity: AppIdentity, claims: ClaimSet): Principal =>
      ({ context: Option.map(claims.tenant, (tenant) => identity.scoped(tenant)), subject: Option.some(claims.subject) })
    const bind = <A, E, R>(identity: AppIdentity, claims: ClaimSet, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      Effect.provideService(effect, TenantScope, principal(identity, claims))
    return { resolve, principal, bind } as const
  }),
  accessors: true,
}) {}
```

## [4]-[POLICY_VOCABULARY]

[POLICY_VOCABULARY]:
- Owner: `Permission` is the action vocabulary, `RolePermission` the RBAC grant table keyed by `Role.Kind`, `Relation` the ReBAC relation vocabulary, `RelationTuple` the `(subject, relation, object)` shape, `PolicyRequest` the evaluated request; `PolicyDecision` is the tagged verdict; `RelationStore` and `FlagGate` are the ReBAC and flag consumer ports; `PolicyFault` is the folder fault shape.
- Law: the tables derive their unions through the anchor tuples, so a new action is one `_permissions` entry plus its `RolePermission` cells and a new relation is one `_relations` entry; the fold never changes shape.
- Law: `FlagGate` is a consumer port — the flag verdict is the runtime wave's to own; this page declares the minimal `enabled` seam and the app root satisfies it with the runtime flag service, the `security → runtime` edge the ledger licenses.
- Growth: a new action or relation is a row; a new denial cause is one `PolicyDecision.Deny` reason.
- Boundary: `access/claim`'s `Role`/`ClaimSet` supply the RBAC input; `RelationStore` is a data-wave-satisfied port; the flag verdict is runtime-wave-owned; this cluster owns the evaluation vocabulary.
- Packages: `effect` (`Schema`, `Context`, `Data`); `@rasm/ts/core` (`FaultClass`).

```typescript
const _permissions = ["read", "write", "delete", "admin", "invite"] as const
const _relations = ["owner", "editor", "member", "viewer"] as const

type PolicyDecision = Data.TaggedEnum<{
  Allow: {}
  Deny: { readonly reason: "no-grant" | "flag-closed" }
}>

const RolePermission = {
  admin: ["read", "write", "delete", "admin", "invite"],
  member: ["read", "write"],
  viewer: ["read"],
} as const satisfies Record<Role.Kind, ReadonlyArray<Permission.Kind>>

const _policyReasons = ["store", "flag"] as const
const _policyFaults = { store: { class: "unavailable" }, flag: { class: "unavailable" } } as const

declare namespace Permission {
  type Kind = (typeof _permissions)[number]
}

declare namespace Relation {
  type Kind = (typeof _relations)[number]
}

declare namespace PolicyFault {
  type Reason = keyof typeof _policyFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _policyFaults> = T
}

class RelationTuple extends Schema.Class<RelationTuple>("RelationTuple")({
  subject: Schema.NonEmptyString,
  relation: Schema.Literal(..._relations),
  object: Schema.NonEmptyString,
}) {}

class PolicyRequest extends Schema.Class<PolicyRequest>("PolicyRequest")({
  action: Schema.Literal(..._permissions),
  object: Schema.NonEmptyString,
  relation: Schema.optionalWith(Schema.Literal(..._relations), { as: "Option" }),
  flag: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

class PolicyFault extends Schema.TaggedError<PolicyFault>()("PolicyFault", {
  reason: Schema.Literal(..._policyReasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _policyFaults[this.reason].class
  }
  override get message(): string {
    return `<policy:${this.reason}> ${this.detail}`
  }
}

class RelationStore extends Context.Tag("security/access/RelationStore")<RelationStore, {
  readonly check: (tuple: RelationTuple) => Effect.Effect<boolean, PolicyFault>
  readonly write: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
  readonly delete: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
}>() {}

class FlagGate extends Context.Tag("security/access/FlagGate")<FlagGate, {
  readonly enabled: (key: string, claims: ClaimSet) => Effect.Effect<boolean, PolicyFault>
}>() {}
```

## [5]-[POLICY_EVALUATION]

[POLICY_EVALUATION]:
- Owner: `Policy.check` folds RBAC and ReBAC into a decision under the flag gate; `Policy.grant`/`Policy.revoke` write relation tuples. The RBAC arm expands each held role's inherited closure through the `Role` table before probing `RolePermission`, so inheritance is data, not a branch.
- Law: a permission is granted when RBAC or ReBAC allows it and the flag gate is open — the gate can only subtract, never grant; a missing relation defaults to no ReBAC grant; only an unreachable store or verdict is a `PolicyFault`, a denial is a `PolicyDecision`.
- Receipt: `PolicyDecision` — `Allow` or `Deny({ reason })`, the reason distinguishing an ungranted action from a closed flag so the edge renders a 403 with cause.
- Growth: a new grant source (an attribute condition) is one fold arm; the receipt never changes.
- Boundary: `RelationStore` carries ReBAC, `FlagGate` carries the runtime verdict; `access/claim`'s `Role`/`ClaimSet` supply the RBAC input; the edge maps the decision to a status.
- Packages: `effect` (`Array`, `Effect`, `HashSet`, `Option`).

```typescript
const _PolicyDecision = Data.taggedEnum<PolicyDecision>()

const _expand = (role: Role.Kind): ReadonlyArray<Role.Kind> => [role, ...Array.flatMap(Role[role].inherits, _expand)]

const _granted = (roles: HashSet.HashSet<Role.Kind>, action: Permission.Kind): boolean =>
  HashSet.some(roles, (role) => Array.some(_expand(role), (inherited) => Array.contains(RolePermission[inherited], action)))

class Policy extends Effect.Service<Policy>()("security/access/Policy", {
  effect: Effect.gen(function* () {
    const relations = yield* RelationStore
    const flags = yield* FlagGate
    const check = (claims: ClaimSet, request: PolicyRequest): Effect.Effect<PolicyDecision, PolicyFault> =>
      Effect.gen(function* () {
        const rebac = yield* Option.match(request.relation, {
          onNone: () => Effect.succeed(false),
          onSome: (relation) => relations.check(new RelationTuple({ subject: claims.subject, relation, object: request.object })),
        })
        const gate = yield* Option.match(request.flag, { onNone: () => Effect.succeed(true), onSome: (key) => flags.enabled(key, claims) })
        return !gate
          ? _PolicyDecision.Deny({ reason: "flag-closed" })
          : _granted(claims.roles, request.action) || rebac
            ? _PolicyDecision.Allow()
            : _PolicyDecision.Deny({ reason: "no-grant" })
      })
    const grant = (tuple: RelationTuple): Effect.Effect<void, PolicyFault> => relations.write(tuple)
    const revoke = (tuple: RelationTuple): Effect.Effect<void, PolicyFault> => relations.delete(tuple)
    return { check, grant, revoke } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Claim, ClaimFault, ClaimSet, ClaimStore, FlagGate, Policy, PolicyFault, PolicyRequest, RelationStore, RelationTuple, Role }
export type { Permission, PolicyDecision, Relation }
```
