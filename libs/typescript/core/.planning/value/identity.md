# [CORE_IDENTITY]

The app-identity value vocabulary: `AppIdentity` spans the eleven cross-cutting dimensions — app, tenant, namespace, build, instance, host-fingerprint, environment, ring, region, zone, cluster — as ONE decoded value, and `TenantContext` is the per-scope tenancy value whose derived `scope` key partitions everything tenant-bounded, with the inverse decode riding the same owner. The `observe` convention projection, the runtime boot identity, and the data per-tenant store scopes all derive from these values — a per-folder identity re-declaration is the named defect, and every app emits, boots, and partitions through this one spine; the Resource identity spine reads settled owner fields, never a consumer-minted dimension. `TenantContext` is an adopted-verbatim decode-boundary name: the C# wire mints it, the interchange codec decodes it into this owner, and no second tenancy notion exists TS-side. The module is `core/src/value/identity.ts`; a new identity dimension is one field on the owning class, never a sibling shape.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                  | [PUBLIC]        |
| :-----: | :----------------- | :------------------------------------------------------ | :-------------- |
|  [01]   | `PROCESS_IDENTITY` | the eleven-dimension app identity and its brand anchors | `AppIdentity`   |
|  [02]   | `TENANT_SCOPE`     | the tenancy value and the bidirectional scope key       | `TenantContext` |

## [02]-[PROCESS_IDENTITY]

- Owner: `AppIdentity`, a `Schema.Class` of `app` (`AppKey` brand), `tenant` (`Option` of `TenantKey` — pinned in a single-tenant deployment, `none` in a multi-tenant process), `namespace` (`Option` of `NamespaceKey` — the service-group axis a fleet partitions dashboards and quotas on), `build` (an inline version+commit block), `instance` (`Refined.Guid` — the per-boot run identity, v7 so instances sort by boot time), `host` (`HostPrint` brand), `environment` (the closed deployment-tier vocabulary), `ring` (the exposure ladder, owner-defaulted to `stable`), `region` (`Option` of `RegionKey` — absent in a single-region deployment), `zone` (`Option` of `ZoneKey` — the availability zone within a region), and `cluster` (`Option` of `ClusterKey` — the orchestration cluster the instance schedules on) — one decoded value carrying every identity dimension, constructed once at boot from validated config plus the boot-minted instance guid.
- Law: the brands are interior anchors reached only through the owner — `AppIdentity.fields.app` composes into sibling field records, `AppIdentity.Key` names the type — so `AppKey` has one spelling, one refinement, one edit site branch-wide.
- Law: `AppKey` is a DNS-safe lowercase slug (2..64, leading letter); `TenantKey` the same alphabet with a digit-or-letter head; `NamespaceKey` the `AppKey` alphabet under its own brand — a namespace is never an app key; `BuildVersion` is semver; `BuildCommit` is 7..40 lowercase hex; `HostPrint` is a non-empty fingerprint of at most 128 characters minted by the host at boot; `RegionKey` and `ZoneKey` are lowercase slugs of at most 32; `ClusterKey` shares the `AppKey` alphabet — each refinement flows into derived arbitraries and config contracts unchanged.
- Law: `environment` and `ring` are closed ORDERED vocabularies, never free strings — `development`/`test`/`staging`/`production` in trust order, `canary`/`beta`/`stable` in exposure order, both tuple-anchored so wire order and non-emptiness are stated once — and the sequence carries derived surfaces, never a consumer re-walk: `byTrust`/`byExposure` derive `Order` instances from tuple position, `reaches(floor)` answers the at-least-tier gate (`production` reaches `staging`), `admits(frontier)` answers progressive exposure (a `canary` deployment admits a feature whose rollout frontier is `beta`; a `stable` one does not), and the `environments`/`rings` statics expose the tuples so a dashboard or config validator walks the owner; a new tier or rung is one tuple entry plus the literal it derives, and `ring` defaults to `stable` at the declaration because a deployment without progressive delivery is fully exposed and no consumer distinguishes absent from stable.
- Law: consumers derive, the floor never encodes their vocabulary — `convention#IDENTITY_PROJECTION` maps this value into attribute space, the runtime boot stamps it, data store scopes partition on the tenancy projection — so a convention rename lands in the consumer's vocabulary, never here, and the Resource identity spine reads settled `instance`/`namespace`/`environment` fields instead of re-minting dimensions at the projection; `alike` is the class-derived `Schema.equivalence` used for identity deduplication and keyed policy comparison.
- Law: `scoped(tenant)` projects the process identity into a `TenantContext` — the one hop from process altitude to scope altitude — and `label` is the diagnostic projection (`app@version`) log and crash rows stamp.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — the encoded side omits the key, the interior never meets `undefined`; a dimension the owner can settle (`ring`) is a declaration default, never an `Option` every consumer re-folds.
- Growth: a new identity dimension (shard, cloud provider) is one field plus its interior brand or vocabulary row, with its `convention#IDENTITY_PROJECTION` line landing in the same edit; a new derived surface is one consumer-side projection over the value.
- Boundary: the runtime config chain constructs the value at boot; this owner declares shape and refinement only and reads no environment.
- Packages: `effect` (`Schema`, `Order`); `schema#REFINED_FLOOR` (`Refined.Guid`).

```typescript signature
import { Order, Schema } from "effect"
import { Refined } from "./schema.ts"

const _AppKey = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("AppKey"))
const _TenantKey = Schema.String.pipe(Schema.pattern(/^[a-z0-9][a-z0-9-]{0,63}$/), Schema.brand("TenantKey"))
const _Namespace = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("NamespaceKey"))
const _Region = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,31}$/), Schema.brand("RegionKey"))
const _Zone = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,31}$/), Schema.brand("ZoneKey"))
const _Cluster = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("ClusterKey"))
const _Version = Schema.String.pipe(
  Schema.pattern(/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/),
  Schema.brand("BuildVersion"),
)
const _Commit = Schema.String.pipe(Schema.pattern(/^[0-9a-f]{7,40}$/), Schema.brand("BuildCommit"))
const _Print = Schema.NonEmptyString.pipe(Schema.maxLength(128), Schema.brand("HostPrint"))
const _Scope = Schema.String.pipe(
  Schema.pattern(/^[a-z][a-z0-9-]{1,63}\/[a-z0-9][a-z0-9-]{0,63}$/),
  Schema.brand("ScopeKey"),
)

const _environments = ["development", "test", "staging", "production"] as const // trust ascends with position: the ONE tier precedence anchor
const _rings = ["canary", "beta", "stable"] as const                            // exposure ascends with position: canary narrowest, stable widest

const _Environment = Schema.Literal(..._environments)
const _Ring = Schema.Literal(..._rings)

const _scope = Schema.decodeSync(_Scope)

class AppIdentity extends Schema.Class<AppIdentity>("AppIdentity")({
  app: _AppKey,
  tenant: Schema.optionalWith(_TenantKey, { as: "Option" }),
  namespace: Schema.optionalWith(_Namespace, { as: "Option" }),
  build: Schema.Struct({ version: _Version, commit: _Commit }),
  instance: Refined.Guid,
  host: _Print,
  environment: _Environment,
  ring: Schema.optionalWith(_Ring, { default: () => "stable" }),
  region: Schema.optionalWith(_Region, { as: "Option" }),
  zone: Schema.optionalWith(_Zone, { as: "Option" }),
  cluster: Schema.optionalWith(_Cluster, { as: "Option" }),
}) {
  static readonly alike = Schema.equivalence(AppIdentity)
  static readonly environments: typeof _environments = _environments
  static readonly rings: typeof _rings = _rings
  static readonly byTrust: Order.Order<AppIdentity.Environment> = Order.mapInput(
    Order.number,
    (environment: AppIdentity.Environment) => _environments.indexOf(environment),
  )
  static readonly byExposure: Order.Order<AppIdentity.Ring> = Order.mapInput(
    Order.number,
    (ring: AppIdentity.Ring) => _rings.indexOf(ring),
  )
  scoped(tenant: TenantContext.Key): TenantContext {
    return new TenantContext({ app: this.app, tenant })
  }
  reaches(floor: AppIdentity.Environment): boolean {
    return Order.greaterThanOrEqualTo(AppIdentity.byTrust)(this.environment, floor)
  }
  admits(frontier: AppIdentity.Ring): boolean {
    return Order.lessThanOrEqualTo(AppIdentity.byExposure)(this.ring, frontier)
  }
  get label(): string {
    return `${this.app}@${this.build.version}`
  }
}

declare namespace AppIdentity {
  type Build = AppIdentity["build"]
  type Cluster = typeof _Cluster.Type
  type Commit = typeof _Commit.Type
  type Environment = typeof _Environment.Type
  type Key = typeof _AppKey.Type
  type Namespace = typeof _Namespace.Type
  type Print = typeof _Print.Type
  type Region = typeof _Region.Type
  type Ring = typeof _Ring.Type
  type Version = typeof _Version.Type
  type Zone = typeof _Zone.Type
}
```

## [03]-[TENANT_SCOPE]

- Owner: `TenantContext`, a `Schema.Class` of `app` plus `tenant` — the exact `(appKey, tenancy)` pair the data folder keys its per-tenant store Layers on and `security` aligns its `rasm.tenant` claims with — carrying structural `Equal` from the declaration so contexts key `LayerMap` and `HashMap` slots with zero ceremony.
- Law: `scope` is the derived partition key — the branded `app/tenant` spelling — computed from fields already proven by their own patterns, so the interior mint is total and the one scope spelling can never disagree with its parts.
- Law: `alike` is the class-derived `Schema.equivalence` — tenancy deduplication and keyed scope comparisons consume the same structural relation as every decode-derived projection.
- Law: direction is a modality of one owner — `FromScope` is the single bidirectional transform: decode recovers `(app, tenant)` from a bare scope through `Schema.TemplateLiteralParser` (the split is unambiguous because both key alphabets forbid `/`, and each part re-proves its own brand), encode re-emits the parts as the template — so the data wave's per-scope family recovers a `ScopeKey`'s coordinates as a decode, never a hand split, and no free inverse export exists beside the mint.
- Law: the decode boundary owns admission — the interchange codec decodes the C#-minted tenancy into this class at its field record; interior construction rides `AppIdentity.scoped` or `new TenantContext` over already-branded parts — one name, one owning side, both paths landing the identical value.
- Law: tenancy never travels as a bare string past a seam — a signature taking `TenantContext.Key` or `TenantContext` is the only spelling, so a tenant-confused cross-partition read fails at compile time.
- Growth: a new scope dimension is one field plus one `scope` respelling here — every partition consumer inherits it in the same edit.
- Boundary: entitlement claims, RLS enforcement, and session binding are `security`/`data` concerns typed against this value; the floor owns only the identity. The wire owns the field set — `TenantContextWire` is C#-minted as `(app, tenant)`, so a new scope dimension admits at the C# mint first and lands here as its decode.
- Packages: `effect` (`Schema`).

```typescript signature
const _ScopeParts = Schema.TemplateLiteralParser(_AppKey, "/", _TenantKey)

class TenantContext extends Schema.Class<TenantContext>("TenantContext")({
  app: _AppKey,
  tenant: _TenantKey,
}) {
  static readonly alike = Schema.equivalence(TenantContext)
  static readonly FromScope: Schema.Schema<TenantContext, `${string}/${string}`> = Schema.transform(
    _ScopeParts,
    Schema.typeSchema(TenantContext),
    {
      strict: true,
      decode: ([app, , tenant]) => new TenantContext({ app, tenant }),
      encode: (context) => [context.app, "/", context.tenant] as const,
    },
  )
  get scope(): TenantContext.Scope {
    return _scope(`${this.app}/${this.tenant}`)
  }
}

declare namespace TenantContext {
  type Key = typeof _TenantKey.Type
  type Scope = typeof _Scope.Type
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AppIdentity, TenantContext }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
