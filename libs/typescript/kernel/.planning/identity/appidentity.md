# [KERNEL_APPIDENTITY]

The app-identity value vocabulary: `AppIdentity` spans the four cross-cutting dimensions — app, tenant, build, host-fingerprint — as ONE decoded value, and `TenantContext` is the per-scope tenancy value whose derived `scope` key partitions everything tenant-bounded. The `telemetry` OTel `Resource`, the `browser` boot identity, and the `store` per-tenant `StoreHandle` scope all derive from these values — a per-folder identity re-declaration is the named defect, and hundreds of apps emit, boot, and partition through this one spine. `TenantContext` is an adopted-verbatim decode-boundary name (invariant 8): the C# wire mints it, `wire` decodes it into this owner, and no second tenancy notion exists TS-side. The module is `kernel/src/identity/appidentity.ts`; a new identity dimension is one field on the owning class, never a sibling shape.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                        | [PUBLIC]        |
| :-----: | :--------------- | :------------------------------------------------------------- | :-------------- |
|  [01]   | `PROCESS_IDENTITY` | the four-dimension app identity and its brand anchors         | `AppIdentity`   |
|  [02]   | `TENANT_SCOPE`   | the tenancy value and the derived scope-partition key          | `TenantContext` |

## [2]-[PROCESS_IDENTITY]

[PROCESS_IDENTITY]:
- Owner: `AppIdentity`, a `Schema.Class` of `app` (`AppKey` brand), `tenant` (`Option` of `TenantKey` — pinned in a single-tenant deployment, `none` in a multi-tenant process), `build` (an inline version+commit block), and `host` (`HostPrint` brand) — one decoded value carrying every identity dimension, constructed once at boot from validated config.
- Law: the brands are interior anchors reached only through the owner — `AppIdentity.fields.app` composes into sibling field records, `AppIdentity.Key` names the type — so `AppKey` has one spelling, one refinement, one edit site branch-wide.
- Law: `AppKey` is a DNS-safe lowercase slug (2..64, leading letter); `TenantKey` the same alphabet with a digit-or-letter head; `BuildVersion` is semver; `BuildCommit` is 7..40 lowercase hex; `HostPrint` is a non-empty fingerprint of at most 128 characters minted by the host at boot — each refinement flows into derived arbitraries and config contracts unchanged.
- Law: consumers derive, the kernel never encodes their vocabulary — `telemetry` maps this value to its OTel `Resource` under its own convention rows, `browser` boot stamps it, `store` scopes partition on the tenancy projection — so a convention rename lands in the consumer's vocabulary, never here.
- Law: `scoped(tenant)` projects the process identity into a `TenantContext` — the one hop from process altitude to scope altitude — and `label` is the diagnostic projection (`app@version`) log and crash rows stamp.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — the encoded side omits the key, the interior never meets `undefined`.
- Growth: a new identity dimension (region, deployment ring) is one field plus its interior brand; a new derived surface is one consumer-side projection over the value.
- Boundary: `host/config` constructs the value at boot through its provider chain; this owner declares shape and refinement only and reads no environment.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import { Option, Schema } from "effect"

const _AppKey = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,63}$/), Schema.brand("AppKey"))
const _TenantKey = Schema.String.pipe(Schema.pattern(/^[a-z0-9][a-z0-9-]{0,63}$/), Schema.brand("TenantKey"))
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

const _scope = Schema.decodeSync(_Scope)

class AppIdentity extends Schema.Class<AppIdentity>("AppIdentity")({
  app: _AppKey,
  tenant: Schema.optionalWith(_TenantKey, { as: "Option" }),
  build: Schema.Struct({ version: _Version, commit: _Commit }),
  host: _Print,
}) {
  scoped(tenant: TenantContext.Key): TenantContext {
    return new TenantContext({ app: this.app, tenant })
  }
  get label(): string {
    return `${this.app}@${this.build.version}`
  }
}

declare namespace AppIdentity {
  type Key = typeof _AppKey.Type
  type Print = typeof _Print.Type
}
```

## [3]-[TENANT_SCOPE]

[TENANT_SCOPE]:
- Owner: `TenantContext`, a `Schema.Class` of `app` plus `tenant` — the exact `(appKey, tenancy)` pair `store` keys its per-tenant `StoreHandle` Layers on and `security/authz` aligns its `app.current_tenant` claims with — carrying structural `Equal` from the declaration so contexts key `LayerMap` and `HashMap` slots with zero ceremony.
- Law: `scope` is the derived partition key — the branded `app/tenant` spelling — computed from fields already proven by their own patterns, so the interior mint is total and the one scope spelling can never disagree with its parts.
- Law: the decode boundary owns admission — `wire` decodes the C#-minted tenancy into this class at its field record; interior construction rides `AppIdentity.scoped` or `new TenantContext` over already-branded parts — one name, one owning side, both paths landing the identical value.
- Law: tenancy never travels as a bare string past a seam — a signature taking `TenantContext.Key` or `TenantContext` is the only spelling, so a tenant-confused cross-partition read fails at compile time.
- Growth: a new scope dimension (environment, shard) is one field plus one `scope` respelling here — every partition consumer inherits it in the same edit.
- Boundary: entitlement claims, RLS enforcement, and session binding are `security`/`store` concerns typed against this value; the kernel owns only the identity.
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
