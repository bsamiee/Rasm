# [SECURITY_POLICY] — RBAC role grants and ReBAC relation tuples folded into one authorization decision

`authz/policy` evaluates a `ClaimSet` against a requested action into one `PolicyDecision`: RBAC grants derive from the `RolePermission` table over the inherited-role closure, ReBAC grants from a `RelationTuple` the `RelationStore` port checks, and a feature-flag gate delegates verdict evaluation to `host/flag` — `authz` keeps entitlement claims and consumes verdicts, the `security → host` edge the ledger licenses. One `Policy.check` folds all three: a permission is granted when RBAC or ReBAC allows it and the flag gate is open, so the next action, role, or relation is a table row, never a new branch. The decision is a tagged verdict carrying its denial reason, never a bare boolean, and `PolicyFault` fires only when the relation store or the flag verdict is unreachable. The flag verdict Tag is `host/flag`'s to own; until that folder lands, this page declares a minimal consumer port and the reconciliation is a deferred residual.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                          | [PACKAGES]                | [REJECTED_FORM]                          |
| :-----: | :------------------------------ | :------------------------------- | :------------------------ | :--------------------------------------- |
|  [01]   | permission + decision vocabulary| `Permission`/`RolePermission`/`PolicyDecision` | `effect` | an `if`-ladder over roles, a boolean allow |
|  [02]   | relation + flag boundaries      | `RelationStore` / `FlagGate` ports | `effect` `Context.Tag`  | a role table checked per query, an inline flag |
|  [03]   | the authorization fold          | `Policy.check` RBAC ∪ ReBAC gate | `authz/claim`             | a permission re-derived at each call site |

## [2]-[POLICY_VOCABULARY]

[GRANTS_AND_DECISION]:
- Owner: `Permission` is the action vocabulary, `RolePermission` the RBAC grant table keyed by `Role.Kind`, `Relation` the ReBAC relation vocabulary, `RelationTuple` the `(subject, relation, object)` shape; `PolicyDecision` is the tagged verdict, `PolicyFault` the folder fault shape.
- Packages: `effect` — the tables derive their unions through `keyof typeof`; `authz/claim` supplies the `Role` inheritance table and the `ClaimSet` under evaluation.
- Boundary: `RelationStore` is a `store`-satisfied port; the `FlagGate` verdict is `host/flag`'s (the deferred residual); `authz/claim` owns the claims, this page owns their evaluation.
- Growth: a new action is one `Permission` row plus its `RolePermission` cells; a new relation is one `Relation` row; the fold never changes.

```typescript
import { Array, Context, Data, Effect, HashSet, Option, Schema } from "effect"
import { Role, type ClaimSet } from "./claim.ts"

// --- [TYPES] ----------------------------------------------------------------------------

const _permissions = ["read", "write", "delete", "admin", "invite"] as const
const _relations = ["owner", "editor", "member", "viewer"] as const

type PolicyDecision = Data.TaggedEnum<{
  Allow: {}
  Deny: { readonly reason: "no-grant" | "flag-closed" }
}>

// --- [CONSTANTS] ------------------------------------------------------------------------

const RolePermission = {
  admin: ["read", "write", "delete", "admin", "invite"],
  member: ["read", "write"],
  viewer: ["read"],
} as const satisfies Record<Role.Kind, ReadonlyArray<Permission.Kind>>

const _reasons = ["store", "flag"] as const

const PolicyFaultPolicy = {
  store: { rank: 4, retry: true, status: 503 },
  flag: { rank: 3, retry: true, status: 503 },
} as const

declare namespace Permission {
  type Kind = (typeof _permissions)[number]
}

declare namespace Relation {
  type Kind = (typeof _relations)[number]
}

declare namespace PolicyFault {
  type Reason = keyof typeof PolicyFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof PolicyFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class RelationTuple extends Schema.Class<RelationTuple>("RelationTuple")({
  subject: Schema.UUID,
  relation: Schema.Literal(..._relations),
  object: Schema.NonEmptyString,
}) {}

class PolicyRequest extends Schema.Class<PolicyRequest>("PolicyRequest")({
  action: Schema.Literal(..._permissions),
  object: Schema.NonEmptyString,
  relation: Schema.optionalWith(Schema.Literal(..._relations), { as: "Option" }),
  flag: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class PolicyFault extends Schema.TaggedError<PolicyFault>()("PolicyFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): PolicyFault.Row {
    return PolicyFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<policy:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class RelationStore extends Context.Tag("security/authz/RelationStore")<RelationStore, {
  readonly check: (tuple: RelationTuple) => Effect.Effect<boolean, PolicyFault>
  readonly write: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
  readonly delete: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
}>() {}

class FlagGate extends Context.Tag("security/authz/FlagGate")<FlagGate, {
  readonly enabled: (key: string, claims: ClaimSet) => Effect.Effect<boolean, PolicyFault>
}>() {}
```

## [3]-[EVALUATION]

[POLICY]:
- Owner: `Policy.check` folds RBAC and ReBAC into a decision under a flag gate; `Policy.grant`/`Policy.revoke` write relation tuples. The RBAC arm expands each held role's inherited closure through the `Role` table before probing `RolePermission`, so inheritance is data, not a branch.
- Packages: `authz/claim` `Role`/`ClaimSet`; the `RelationStore` and `FlagGate` ports carry ReBAC and the flag verdict.
- Law: a permission is granted when RBAC or ReBAC allows it and the flag gate is open — the gate can only subtract, never grant; a missing relation defaults to no ReBAC grant; only an unreachable store or verdict is a `PolicyFault`, a denial is a decision.
- Receipt: `PolicyDecision` — `Allow` or `Deny({ reason })`, the reason distinguishing an ungranted action from a closed flag so the edge renders 403 with cause.

```typescript
// --- [OPERATIONS] -----------------------------------------------------------------------

const _PolicyDecision = Data.taggedEnum<PolicyDecision>()

const _expand = (role: Role.Kind): ReadonlyArray<Role.Kind> => [role, ...Array.flatMap(Role[role].inherits, _expand)]

const _granted = (roles: HashSet.HashSet<Role.Kind>, action: Permission.Kind): boolean =>
  HashSet.some(roles, (role) => Array.some(_expand(role), (inherited) => Array.contains(RolePermission[inherited], action)))

// --- [SERVICES] -------------------------------------------------------------------------

class Policy extends Effect.Service<Policy>()("security/authz/Policy", {
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

export { FlagGate, Policy, PolicyFault, PolicyRequest, RelationStore, RelationTuple }
export type { Permission, PolicyDecision, Relation }
```
