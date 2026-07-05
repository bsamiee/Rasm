# [CORE_IDENTITY]

The app-identity value vocabulary: `AppIdentity` spans the nine cross-cutting dimensions — app, tenant, namespace, build, instance, host-fingerprint, environment, ring, region — as ONE decoded value, and `TenantContext` is the per-scope tenancy value whose derived `scope` key partitions everything tenant-bounded. The `observe` convention projection, the runtime boot identity, and the data per-tenant store scopes all derive from these values — a per-folder identity re-declaration is the named defect, and hundreds of apps emit, boot, and partition through this one spine; the Resource identity spine reads settled owner fields, never a consumer-minted dimension. `TenantContext` is an adopted-verbatim decode-boundary name: the C# wire mints it, the interchange codec decodes it into this owner, and no second tenancy notion exists TS-side. The module is `core/src/value/identity.ts`; a new identity dimension is one field on the owning class, never a sibling shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                    | [PUBLIC]        |
| :-----: | :----------------- | :--------------------------------------------------------- | :-------------- |
|  [01]   | `PROCESS_IDENTITY` | the nine-dimension app identity and its brand anchors      | `AppIdentity`   |
|  [02]   | `TENANT_SCOPE`     | the tenancy value and the derived scope-partition key      | `TenantContext` |

## [2]-[PROCESS_IDENTITY]

[PROCESS_IDENTITY]:
- Owner: `AppIdentity`, a `Schema.Class` of `app` (`AppKey` brand), `tenant` (`Option` of `TenantKey` — pinned in a single-tenant deployment, `none` in a multi-tenant process), `namespace` (`Option` of `NamespaceKey` — the service-group axis a fleet partitions dashboards and quotas on), `build` (an inline version+commit block), `instance` (`Refined.Guid` — the per-boot run identity, v7 so instances sort by boot time), `host` (`HostPrint` brand), `environment` (the closed deployment-tier vocabulary), `ring` (the exposure ladder, owner-defaulted to `stable`), and `region` (`Option` of `RegionKey` — absent in a single-region deployment) — one decoded value carrying every identity dimension, constructed once at boot from validated config plus the boot-minted instance guid.
- Law: the brands are interior anchors reached only through the owner — `AppIdentity.fields.app` composes into sibling field records, `AppIdentity.Key` names the type — so `AppKey` has one spelling, one refinement, one edit site branch-wide.
- Law: `AppKey` is a DNS-safe lowercase slug (2..64, leading letter); `TenantKey` the same alphabet with a digit-or-letter head; `NamespaceKey` the `AppKey` alphabet under its own brand — a namespace is never an app key; `BuildVersion` is semver; `BuildCommit` is 7..40 lowercase hex; `HostPrint` is a non-empty fingerprint of at most 128 characters minted by the host at boot; `RegionKey` is a lowercase slug of at most 32 — each refinement flows into derived arbitraries and config contracts unchanged.
- Law: `environment` and `ring` are closed vocabularies, never free strings — `development`/`test`/`staging`/`production` in trust order, `canary`/`beta`/`stable` in exposure order, both tuple-anchored so wire order and non-emptiness are stated once and a new tier or rung is one tuple entry plus the literal it derives; `ring` defaults to `stable` at the declaration because a deployment without progressive delivery is fully exposed and no consumer distinguishes absent from stable.
- Law: consumers derive, the floor never encodes their vocabulary — `convention#IDENTITY_PROJECTION` maps this value into attribute space, the runtime boot stamps it, data store scopes partition on the tenancy projection — so a convention rename lands in the consumer's vocabulary, never here, and the Resource identity spine reads settled `instance`/`namespace`/`environment` fields instead of re-minting dimensions at the projection.
- Law: `scoped(tenant)` projects the process identity into a `TenantContext` — the one hop from process altitude to scope altitude — and `label` is the diagnostic projection (`app@version`) log and crash rows stamp.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — the encoded side omits the key, the interior never meets `undefined`; a dimension the owner can settle (`ring`) is a declaration default, never an `Option` every consumer re-folds.
- Growth: a new identity dimension (zone, cluster) is one field plus its interior brand or vocabulary row; a new derived surface is one consumer-side projection over the value.
- Boundary: the runtime config chain constructs the value at boot; this owner declares shape and refinement only and reads no environment.
- Packages: `effect` (`Schema`, `Option`); `schema#REFINED_FLOOR` (`Refined.Guid`).

```typescript
import { Option, Schema } from "effect"
import { Refined } from "./schema.ts"

const _AppKey = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("AppKey"))
const _TenantKey = Schema.String.pipe(Schema.pattern(/^[a-z0-9][a-z0-9-]{0,63}$/), Schema.brand("TenantKey"))
const _Namespace = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("NamespaceKey"))
const _Region = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,31}$/), Schema.brand("RegionKey"))
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

const _environments = ["development", "test", "staging", "production"] as const // trust order: a load-bearing sequence policy consumers walk
const _rings = ["canary", "beta", "stable"] as const                            // exposure order: canary narrowest, stable fully exposed

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
}) {
  scoped(tenant: TenantContext.Key): TenantContext {
    return new TenantContext({ app: this.app, tenant })
  }
  get label(): string {
    return `${this.app}@${this.build.version}`
  }
}

declare namespace AppIdentity {
  type Environment = typeof _Environment.Type
  type Key = typeof _AppKey.Type
  type Namespace = typeof _Namespace.Type
  type Print = typeof _Print.Type
  type Region = typeof _Region.Type
  type Ring = typeof _Ring.Type
}
```

## [3]-[TENANT_SCOPE]

[TENANT_SCOPE]:
- Owner: `TenantContext`, a `Schema.Class` of `app` plus `tenant` — the exact `(appKey, tenancy)` pair the data folder keys its per-tenant store Layers on and `security` aligns its `app.current_tenant` claims with — carrying structural `Equal` from the declaration so contexts key `LayerMap` and `HashMap` slots with zero ceremony.
- Law: `scope` is the derived partition key — the branded `app/tenant` spelling — computed from fields already proven by their own patterns, so the interior mint is total and the one scope spelling can never disagree with its parts.
- Law: the decode boundary owns admission — the interchange codec decodes the C#-minted tenancy into this class at its field record; interior construction rides `AppIdentity.scoped` or `new TenantContext` over already-branded parts — one name, one owning side, both paths landing the identical value.
- Law: tenancy never travels as a bare string past a seam — a signature taking `TenantContext.Key` or `TenantContext` is the only spelling, so a tenant-confused cross-partition read fails at compile time.
- Growth: a new scope dimension (environment, shard) is one field plus one `scope` respelling here — every partition consumer inherits it in the same edit.
- Boundary: entitlement claims, RLS enforcement, and session binding are `security`/`data` concerns typed against this value; the floor owns only the identity.
- Packages: `effect` (`Schema`).

```typescript
class TenantContext extends Schema.Class<TenantContext>("TenantContext")({
  app: _AppKey,
  tenant: _TenantKey,
}) {
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
