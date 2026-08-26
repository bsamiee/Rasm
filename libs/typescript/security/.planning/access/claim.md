# [SECURITY_CLAIM]

One authorization owner: the entitlement vocabulary a verified token resolves into and the RBAC-union-ReBAC fold that evaluates it into a decision. Claims are data — the subject's granted roles, scopes, and active tenant — resolved once per request from the `crypt/sign` `AccessClaims`, enriched by the `ClaimStore` port the app root satisfies with the data wave; the tenancy key pairs with the boot `Identity.App` into the core `Identity.Tenant` the `access/tenant` reference carries, so `store` binds RLS with no parameter threading. Role-inheritance closure is compile-time-constant data computed exactly once — `RoleGrant` derives each role's transitive permission set at module load through a hop-bounded fold over the static `Role` table, so `_granted` is one `HashSet` membership read per held role and a cyclic `inherits` edit refuses the whole grant table with spent-budget evidence instead of overflowing a request or trimming itself in silence. `Policy.check` folds four inputs into one `PolicyDecision`: RBAC grants derive from `RoleGrant`, ReBAC grants from a `RelationCheck` request the `RelationStore` port's one batching resolver settles, a feature-flag gate delegates verdict evaluation to the runtime wave through the `FlagGate` consumer port — the `security → runtime` edge the ledger licenses — and the delegation ceiling `ScopeGrant` projects off the presented scope set caps whatever the grant halves answer. The two effectful sources are independent, so they compose applicatively at an explicit degree and the fold pays their max rather than their sum, while the request family's structural identity collapses an N-object render's N ReBAC round trips into one batched store call with repeated triples deduplicated. One `check` folds every input: a permission is granted when RBAC or ReBAC grants it, the flag gate is open, and the action falls inside the delegation the presented credential was issued to spend, so the next action, role, relation, or scope bundle is a table row, never a new branch. `PolicyDecision` is the tagged verdict carrying its denial reason, never a bare boolean, and every `Deny` increments the `Convention.instrument.securityPolicyDeny` counter tagged by the owned reason key — the access-denial dashboard is structural; `ClaimFault`/`PolicyFault` instantiate the folder fault shape over the core `Fault.Class.family` boundary and fire only when a store or verdict is unreachable — a denial is a decision, an empty role set is a valid unprivileged subject, and a scope set naming no `rasm:` bundle is a first-party credential rather than a stripped one.

## [01]-[INDEX]

- [02]-[CLAIM_VOCABULARY]: `Role`, `ClaimSet`, `ClaimFault`.
- [03]-[CLAIM_RESOLUTION]: `Claim`.
- [04]-[POLICY_VOCABULARY]: `PolicyDecision`, `RelationCheck`, `RelationStore`, `FlagGate`.
- [05]-[POLICY_EVALUATION]: `Policy`, `PolicyFault`.

## [02]-[CLAIM_VOCABULARY]

[CLAIM_VOCABULARY]:
- Law: the `_roles` tuple anchors the key set in both directions — `ClaimSet.roles`, `RolePermission`'s keys, and `RoleGrant`'s keys all derive from it, and the `_Keys`/`_Kinds` guard pair fails a tuple/table divergence at the declaration.
- Law: a `Role` row carries its inheritance edges and nothing else — authority derives from `RolePermission` through the closure, so a numeric rank beside the edges is a second ordering nothing reads and the first ranked comparison to arrive disagrees with the grant table it stands in for.
- Law: `ClaimSet` carries the presented scope strings verbatim beside the roles, and the shape stays OPEN here because one array carries two vocabularies — the issuing IdP's consent grants (`openid`, `email`) beside this service's own delegation bundles — where `[04]`'s `_scopes` anchor closes the delegation half alone. Scope is therefore evidence at this vocabulary and a CEILING at the fold: it subtracts from the role grant and never adds to it, so a machine key spends the intersection of its subject's authority and its own delegation rather than the whole grant behind a narrow presentation.
- Growth: a new role is one `_roles` entry with its `Role` row and `RolePermission` cells; a new claim facet is one `ClaimSet` field the store persists.
- Boundary: `crypt/sign` owns the `AccessClaims` the edge's verify hands in; `access/tenant` owns the `TenantScope` reference the tenancy binds; `ClaimStore` is a data-wave-satisfied port; the policy fold below consumes the `ClaimSet`.

```typescript
import { Identity, Convention, Fault, Shape } from "@rasm/core"
import { Array, Config, Context, Data, Effect, Either, HashMap, HashSet, Metric, Option, Request, type RequestResolver, Schema } from "effect"
import type { ApiKeyRecord } from "../authn/credential.ts"
import { AccessClaims } from "../crypt/sign.ts"
import { SecurityFact, Witness } from "./audit.ts"
import { type Principal, TenantScope } from "./tenant.ts"

const _roles = ["admin", "member", "viewer"] as const

const Role = {
  admin: { inherits: ["member"] },
  member: { inherits: ["viewer"] },
  viewer: { inherits: [] },
} as const

const _claimFamily = Fault.Class.family(["store", "tenant"] as const, {
  store: Fault.Class.row({
    class: "unavailable",
    leg: "store",
    detail: Schema.Struct({ subject: Schema.String, cause: Schema.String }),
    render: ({ cause, subject }) => `claim store unreachable for ${subject}: ${cause}`,
  }),
  tenant: Fault.Class.row({
    class: "malformed",
    leg: "tenancy",
    detail: Schema.Struct({ tid: Schema.String, cause: Schema.String }),
    render: ({ cause, tid }) => `presented tid ${tid} is no tenant key: ${cause}`,
  }),
})

declare namespace Role {
  type Kind = keyof typeof Role
  type _Rows<T extends Record<(typeof _roles)[number], { readonly inherits: ReadonlyArray<(typeof _roles)[number]> }> = typeof Role> = T
  type _Keys<K extends Kind = (typeof _roles)[number]> = K
  type _Kinds<K extends (typeof _roles)[number] = Kind> = K
}

declare namespace ClaimFault {
  type Case = typeof _claimFamily.payload.Type
  type Reason = (typeof _claimFamily.kinds)[number]
}

class ClaimSet extends Schema.Class<ClaimSet>("ClaimSet")({
  subject: Schema.NonEmptyString,
  tenant: Shape.posture.of(Identity.Tenant.fields.tenant),
  roles: Schema.HashSet(Schema.Literal(..._roles)),
  scopes: Schema.HashSet(Schema.NonEmptyString),
}) {}

class ClaimFault extends Schema.TaggedError<ClaimFault>()("ClaimFault", {
  case: _claimFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _claimFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _claimFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _claimFamily.render(this.case)
  }
}

class ClaimStore extends Context.Tag("security/access/ClaimStore")<ClaimStore, {
  readonly rolesOf: (subject: string, tenant: Option.Option<Identity.Tenant.Key>) => Effect.Effect<HashSet.HashSet<Role.Kind>, ClaimFault>
  readonly grant: (subject: string, tenant: Option.Option<Identity.Tenant.Key>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
  readonly revoke: (subject: string, tenant: Option.Option<Identity.Tenant.Key>, role: Role.Kind) => Effect.Effect<void, ClaimFault>
}>() {}
```

## [03]-[CLAIM_RESOLUTION]

[CLAIM_RESOLUTION]:
- Law: roles are read once per request into the `ClaimSet`, never re-derived at each policy check, and the tenant flows through the `TenantScope` reference rather than a parameter. Absence and malformation take different answers: a token stating no `tid` accepts the deployment's default, while a token stating a `tid` this service cannot spell refuses as `ClaimFault.tenant` — folding both into one empty `Option` serves the default tenant's roles to a subject that claimed somebody else's. The two ACCEPTING answers stay apart on the value: an asserted `tid` rides `declared` and the fallback rides `defaulted` naming the deployment as its source, so RLS binding and the `Deny` fact both read whose claim the tenancy is rather than a `Some` that hides it. `DEFAULT_TENANT` itself decodes at the boot line, so a misspelled default fails the root proof instead of every untenanted request.
- Law: `resolve` is the one entitlement door and it discriminates on the CONSTRUCTOR, not on a field name — `AccessClaims` and `ApiKeyRecord` are both `Schema.Class` values, so `instanceof` reads which mint produced the value and a column either class later grows cannot invert the arm; both fold into one `ClaimSet`, the machine arm projecting subject and scopes off the record with roles read from the same store, so machine callers hit RBAC/ReBAC policy identically to token callers and no guard boundary grows a parallel claims path. Both mints present their delegation through that one `scopes` slot — `AccessClaims.scope` on the token arm, `ApiKeyRecord.scopes` on the machine arm — so the `[05]` ceiling binds every principal source from this projection alone and `authn/credential`, `authn/workload`, and the runtime admission lift carry no scope arm of their own.
- Law: `access/tenant` mints every bound `Principal` — `principal` composes `TenantScope.of` and `bind` composes `TenantScope.bind`, so the tenancy shape holds exactly one construction site and a coordinate it acquires reaches this page with no edit here.
- Output: `ClaimSet` — the immutable resolved claim the policy fold reads; `Principal` — the ambient tenancy the reference carries.
- Growth: a new claim source is one arm on the `resolve` input union, admitted by its own constructor and binding through the same `Principal`.

```typescript
class Claim extends Effect.Service<Claim>()("security/access/Claim", {
  effect: Effect.gen(function* () {
    const store = yield* ClaimStore
    const fallback = yield* Config.option(Schema.Config("DEFAULT_TENANT", Identity.Tenant.fields.tenant).pipe(
      Config.withDescription("tenant key an absent tid falls to; unset leaves the claim untenanted"),
    ))
    const _decoded = (raw: string): Effect.Effect<Identity.Tenant.Key, ClaimFault> =>
      Schema.decode(Identity.Tenant.fields.tenant)(raw).pipe(
        Effect.mapError((issue) => new ClaimFault({ case: { reason: "tenant", tid: raw, cause: issue.message } })),
      )
    const _tenantOf = (tid: Option.Option<string>): Effect.Effect<Shape.Posture<Identity.Tenant.Key>, ClaimFault> =>
      Option.match(tid, {
        onSome: (raw) => Effect.map(_decoded(raw), (value) => ({ _tag: "declared" as const, value })),
        onNone: () =>
          Effect.succeed(Option.match(fallback, {
            onNone: () => ({ _tag: "absent" as const }),
            onSome: (value) => ({ _tag: "defaulted" as const, source: "deployment" as const, value }),
          })),
      })
    const resolve = (presented: AccessClaims | ApiKeyRecord): Effect.Effect<ClaimSet, ClaimFault> =>
      Effect.gen(function* () {
        const shaped = presented instanceof AccessClaims
          ? { subject: presented.sub, tid: presented.tid, scopes: presented.scope }
          : { subject: presented.subject, tid: Option.none<string>(), scopes: presented.scopes }
        const tenant = yield* _tenantOf(shaped.tid)
        const roles = yield* store.rolesOf(shaped.subject, Shape.posture.value(tenant))
        return new ClaimSet({ subject: shaped.subject, tenant, roles, scopes: HashSet.fromIterable(shaped.scopes) })
      }).pipe(Effect.withSpan("security.claim.resolve"))
    const principal = (identity: Identity.App, claims: ClaimSet): Principal =>
      TenantScope.of(
        Option.getOrUndefined(Option.map(Shape.posture.value(claims.tenant), (tenant) => identity.scoped(tenant))),
        claims.subject,
      )
    const bind = <A, E, R>(identity: Identity.App, claims: ClaimSet, effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
      TenantScope.bind(principal(identity, claims), effect)
    return { resolve, principal, bind } as const
  }),
  accessors: true,
}) {}
```

## [04]-[POLICY_VOCABULARY]

[POLICY_VOCABULARY]:
- Owner: `Permission` is the action vocabulary, `RolePermission` the RBAC grant table keyed by `Role.Kind`, `Scope` the delegation vocabulary with `ScopePermission` its ceiling table keyed by `Scope.Kind`, `Relation` the ReBAC relation vocabulary, `RelationTuple` the write-side `(subject, relation, object)` shape, `RelationCheck` its read-side request family, `PolicyRequest` the evaluated request; `PolicyDecision` is the tagged verdict; `RelationStore` and `FlagGate` are the ReBAC and flag consumer ports; `PolicyFault` is the folder fault shape closed at the core family boundary.
- Law: the tables derive their unions through the anchor tuples, so a new action is one `_permissions` entry with its `RolePermission` cells, a new delegation bundle one `_scopes` entry with its `ScopePermission` cells, and a new relation one `_relations` entry; the fold never changes shape.
- Law: a scope row is a NAMESPACED bundle, never a permission alias — every row carries the `_namespace` prefix and the `Scope._Namespaced` guard fails a row dropping it at the declaration, so the ceiling tests a presented string against that namespace; a table keyed on bare action words carries no such discriminant. Bundles run coarser than the action axis and carry no inheritance edges: a client asks for one row and spends the cells that row names, so a wider bundle re-spells its cells and delegation stays stated by the issuer rather than derived behind it, where `Role` earns its edges by modelling an org chart no credential presents.
- Law: the ceiling caps the ACTION axis alone, because `Permission.Kind` is the whole action vocabulary and `PolicyRequest.object` carries the resource — a resource-grained scope row re-spells that object, and per-object delegation is already a `RelationTuple` the ReBAC half batches. `rasm:write` therefore bounds what a credential does and a relation bounds which objects it does it to, and neither vocabulary grows the other's axis.
- Law: the ReBAC read crosses as a request, the write as a tuple — `RelationCheck` is a `Request.TaggedClass` whose structural `Equal` over exactly the triple IS the dedup identity, so the port publishes ONE `RequestResolver` rather than a `check` member and the Zanzibar batch shape is structural: an N-object list render folds N permission checks into one store call with repeated triples collapsed, where a per-tuple member issues N queries and re-asks an identical triple twice in one request. A `checkMany` twin beside the resolver is the surface this family deletes.
- Law: `FlagGate` is a consumer port — the flag verdict is the runtime wave's to own; this page declares the minimal `enabled` port and the app root satisfies it with the runtime flag service, the `security → runtime` edge the ledger licenses.
- Growth: a new action, delegation bundle, or relation is a row; a new denial cause is one `PolicyDecision.Deny` reason; a new read shape is a field on `RelationCheck`, never a second resolver.
- Boundary: `[02]`'s `Role`/`ClaimSet` supply the RBAC input and the presented delegation the ceiling reads; `RelationStore` is a data-wave-satisfied port and the resolver behind it is that wave's `RequestResolver.makeBatched` over its own tuple store — this page owns the request family and its identity, never the batch body; the flag verdict is runtime-wave-owned; this cluster owns the evaluation vocabulary.
- Packages: `effect` (`Schema`, `Context`, `Data`, `Request`, `RequestResolver`); `@rasm/core` (`Fault.Class`, `Shape.posture`).

```typescript
const _permissions = ["read", "write", "delete", "admin", "invite"] as const
const _relations = ["owner", "editor", "member", "viewer"] as const
const _namespace = "rasm:"
const _scopes = ["rasm:read", "rasm:write", "rasm:manage", "rasm:admin"] as const

type PolicyDecision = Data.TaggedEnum<{
  Allow: {}
  Deny: { readonly reason: "no-grant" | "flag-closed" | "scope-exceeded" }
}>

const RolePermission = {
  admin: ["read", "write", "delete", "admin", "invite"],
  member: ["read", "write"],
  viewer: ["read"],
} as const satisfies Record<Role.Kind, ReadonlyArray<Permission.Kind>>

const ScopePermission = {
  "rasm:read": ["read"],
  "rasm:write": ["read", "write"],
  "rasm:manage": ["read", "write", "delete", "invite"],
  "rasm:admin": ["read", "write", "delete", "admin", "invite"],
} as const satisfies Record<Scope.Kind, ReadonlyArray<Permission.Kind>>

const _policyFamily = Fault.Class.family(["store", "flag"] as const, {
  store: Fault.Class.row({
    class: "unavailable",
    leg: "relation",
    detail: Schema.Struct({ arm: Schema.Literal("check", "write", "delete"), object: Schema.String, cause: Schema.String }),
    render: ({ arm, cause, object }) => `relation store ${arm} unreachable for ${object}: ${cause}`,
  }),
  flag: Fault.Class.row({
    class: "unavailable",
    leg: "flag",
    detail: Schema.Struct({ key: Schema.String, cause: Schema.String }),
    render: ({ cause, key }) => `flag gate unreachable for ${key}: ${cause}`,
  }),
})

declare namespace Permission {
  type Kind = (typeof _permissions)[number]
}

declare namespace Relation {
  type Kind = (typeof _relations)[number]
}

declare namespace Scope {
  type Kind = (typeof _scopes)[number]
  type _Namespaced<K extends `${typeof _namespace}${string}` = Kind> = K
}

declare namespace PolicyFault {
  type Case = typeof _policyFamily.payload.Type
  type Reason = (typeof _policyFamily.kinds)[number]
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
  case: _policyFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _policyFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _policyFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _policyFamily.render(this.case)
  }
}

class RelationCheck extends Request.TaggedClass("RelationCheck")<boolean, PolicyFault, {
  readonly subject: string
  readonly relation: Relation.Kind
  readonly object: string
}> {}

class RelationStore extends Context.Tag("security/access/RelationStore")<RelationStore, {
  readonly resolver: RequestResolver.RequestResolver<RelationCheck>
  readonly write: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
  readonly delete: (tuple: RelationTuple) => Effect.Effect<void, PolicyFault>
}>() {}

class FlagGate extends Context.Tag("security/access/FlagGate")<FlagGate, {
  readonly enabled: (key: string, claims: ClaimSet) => Effect.Effect<boolean, PolicyFault>
}>() {}
```

## [05]-[POLICY_EVALUATION]

[POLICY_EVALUATION]:
- Owner: `Policy.check` folds RBAC and ReBAC into a decision under the flag gate and the delegation ceiling; `Policy.grant`/`Policy.revoke` write relation tuples. `RoleGrant` is the derived closure — one module-load fold expands each role's transitive inheritance through the hop-bounded `_closure` and flattens it into a permission `HashSet` per role — so inheritance is derived data, `_granted` is O(held roles) membership, and the recursion, its re-expansion cost, and its cycle risk exist nowhere at request time. `ScopeGrant` is its delegation twin: one module-load flatten of each `_scopes` row into a permission `HashSet`, so `_delegated` pays the same O(presented scopes) membership read and the ceiling costs no effect, no round trip, and no second check site.
- Law: a permission is granted when RBAC or ReBAC grants it, the flag gate is open, AND the presented delegation admits the action — everything past the grant halves SUBTRACTS and nothing past them adds, so the verdict is the minimum of held authority and exercised delegation; a missing relation defaults to no ReBAC grant; only an unreachable store or verdict is a `PolicyFault`, a denial is a `PolicyDecision`; every `Deny` increments `Convention.instrument.securityPolicyDeny` tagged under `Convention.rasm.securityReason` and publishes the `Deny` fact through `Witness` so the denial reaches the audit journal whole — a denial is a decision, never an authenticity reject, so it mints its own Convention row rather than a `Reject` kind.
- Law: delegation is measured on what the credential PRESENTED, never on what this service can spell — a presented set naming no `_namespace` string bound nothing and exercises the subject's whole role grant (a first-party session or a key issued without delegation), and a set naming one or more caps authority at the union `ScopeGrant` projects, so a namespaced scope no row spells narrows to nothing. Reading the bound off table membership instead inverts the guarantee: one misspelled or retired scope falls out of the ceiling and hands its token the subject's entire authority, which is the confused-deputy hole the ceiling exists to close.
- Law: the ceiling binds every principal source at this one arm — `Claim.resolve` folds `AccessClaims.scope` and `ApiKeyRecord.scopes` into the same `ClaimSet.scopes` slot, so a bearer token, a machine key, and a cookie-borne session meet one delegation test with zero edits at `authn/credential`, `authn/workload`, or the runtime admission lift; a per-source ceiling is the drift this arm forecloses, and the day a fourth mint lands it inherits the bound by resolving through the same door.
- Law: the two effectful sources are independent, so they compose applicatively — `Effect.all` at `{ concurrency: 2 }` pays their max where a bind chain paid their sum, and every check runs both because the gate can only subtract; a sequential fold here also serializes the batch window, so the ReBAC half never accumulates a batch worth collapsing; the ceiling joins that same expression as pure data, so a third effect never appears beside them.
- Entry: the serving edge wraps one request scope in `Effect.withRequestCaching(true)`, so a triple checked twice inside one request resolves once; batching itself needs no knob because `Effect.request` funnels into the resolver's own window.
- Output: `PolicyDecision` — `Allow` or `Deny({ reason })`, the reason separating an ungranted action, a closed flag, and an action the subject holds but the presented credential was never issued to spend, so the edge renders a 403 whose cause tells a caller to widen the token rather than the role.
- Growth: a new grant source (an attribute condition) is one `Effect.all` slot; a new role's grants land in `RoleGrant` and a new bundle's cells in `ScopeGrant` at module load with zero fold edits; `PolicyDecision` never changes.
- Boundary: `RelationStore` carries ReBAC, `FlagGate` carries the runtime verdict; `access/claim`'s `Role`/`ClaimSet` supply the RBAC input; the edge maps the decision to a status and owns the caching scope.
- Packages: `effect` (`Array`, `Effect`, `Either`, `HashMap`, `HashSet`, `Metric`, `Option`, `Effect.request`); `@rasm/core` (`Convention`, `Fault.Class.spent`, `Shape.Bound`); `access/audit` (`Witness`, `SecurityFact`).

```typescript
const _PolicyDecision = Data.taggedEnum<PolicyDecision>()

const _deny = Convention.mount(Convention.metric.securityPolicyDeny)

const _inheritance = Shape.Bound.bounded("hops", _roles.length)

const _closure = (
  role: Role.Kind,
  seen: HashSet.HashSet<Role.Kind>,
  hops: number,
): Either.Either<HashSet.HashSet<Role.Kind>, Shape.BoundSpent> =>
  HashSet.has(seen, role)
    ? Either.right(seen)
    : Option.match(Shape.Bound.spent(_inheritance, hops), {
      onSome: Either.left,
      onNone: () =>
        Either.map(
          Array.reduce(
            Role[role].inherits,
            Either.right<HashSet.HashSet<Role.Kind>, Shape.BoundSpent>(seen),
            (held, next) => Either.flatMap(held, (acc) => _closure(next, acc, hops + 1)),
          ),
          (acc) => HashSet.add(acc, role),
        ),
    })

const RoleGrant: HashMap.HashMap<Role.Kind, HashSet.HashSet<Permission.Kind>> = HashMap.fromIterable(
  Array.map(_roles, (role) =>
    [
      role,
      HashSet.flatMap(
        Either.getOrThrowWith(
          _closure(role, HashSet.empty(), 0),
          (spent) =>
            new TypeError(`<role-inheritance:${role}> ${Fault.Class.spent.render({ reason: spent.unit, ceiling: spent.ceiling, reached: spent.reached })}`),
        ),
        (held) => HashSet.fromIterable(RolePermission[held]),
      ),
    ] as const))

const _granted = (roles: HashSet.HashSet<Role.Kind>, action: Permission.Kind): boolean =>
  HashSet.some(roles, (role) => Option.exists(HashMap.get(RoleGrant, role), (grants) => HashSet.has(grants, action)))

const ScopeGrant: HashMap.HashMap<string, HashSet.HashSet<Permission.Kind>> = HashMap.fromIterable(
  Array.map(_scopes, (scope) => [scope, HashSet.fromIterable(ScopePermission[scope])] as const))

const _bounded = (scopes: HashSet.HashSet<string>): boolean =>
  HashSet.some(scopes, (scope) => scope.startsWith(_namespace))

const _delegated = (scopes: HashSet.HashSet<string>, action: Permission.Kind): boolean =>
  !_bounded(scopes)
  || HashSet.some(scopes, (scope) => Option.exists(HashMap.get(ScopeGrant, scope), (grants) => HashSet.has(grants, action)))

class Policy extends Effect.Service<Policy>()("security/access/Policy", {
  effect: Effect.gen(function* () {
    const relations = yield* RelationStore
    const flags = yield* FlagGate
    const check = (claims: ClaimSet, request: PolicyRequest): Effect.Effect<PolicyDecision, PolicyFault> =>
      Effect.gen(function* () {
        const { gate, rebac } = yield* Effect.all({
          gate: Option.match(request.flag, { onNone: () => Effect.succeed(true), onSome: (key) => flags.enabled(key, claims) }),
          rebac: Option.match(request.relation, {
            onNone: () => Effect.succeed(false),
            onSome: (relation) =>
              Effect.request(new RelationCheck({ subject: claims.subject, relation, object: request.object }), relations.resolver),
          }),
        }, { concurrency: 2 })
        const entitled = _granted(claims.roles, request.action) || rebac
        return !gate
          ? _PolicyDecision.Deny({ reason: "flag-closed" })
          : !entitled
            ? _PolicyDecision.Deny({ reason: "no-grant" })
            : _delegated(claims.scopes, request.action)
              ? _PolicyDecision.Allow()
              : _PolicyDecision.Deny({ reason: "scope-exceeded" })
      }).pipe(
        Effect.tap((decision) =>
          decision._tag === "Deny"
            ? Effect.zipRight(
                Metric.increment(_deny.pipe(Metric.tagged(Convention.rasm.securityReason, decision.reason))),
                Witness.publish(SecurityFact.Deny({ subject: claims.subject, action: request.action, reason: decision.reason, tenant: claims.tenant })),
              )
            : Effect.void),
        Effect.withSpan("security.policy.check"),
      )
    const grant = (tuple: RelationTuple): Effect.Effect<void, PolicyFault> => relations.write(tuple)
    const revoke = (tuple: RelationTuple): Effect.Effect<void, PolicyFault> => relations.delete(tuple)
    return { check, grant, revoke } as const
  }),
  accessors: true,
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Claim, ClaimFault, ClaimSet, ClaimStore, FlagGate, Policy, PolicyFault, PolicyRequest, RelationCheck, RelationStore, RelationTuple, Role, RoleGrant, ScopeGrant }
export type { Permission, PolicyDecision, Relation, Scope }
```

## [06]-[RESEARCH]

(none)
