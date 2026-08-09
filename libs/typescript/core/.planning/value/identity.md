# [CORE_IDENTITY]

`Identity` is the sole process and tenancy identity owner. Nested `App` carries boot-set dimensions; nested `Tenant` is exactly app plus tenant and owns the reversible scope spelling; `tenancy` is the consumption axis counting the tenants a deployment separates. Module: `core/src/value/identity.ts`.

## [01]-[IDENTITY_OWNER]

- Every DNS-safe identity brand derives through one parameterized slug constructor.
- `Identity.environment`, `Identity.ring`, and `Identity.tenancy` are ordered exact vocabularies; their schema, order, guard, and rows share one declaration.
- `Identity.tenancy` seats the tenancy axis at the stratum every consumer reaches, so a persistence sweep, a work schedule, and a consumption profile read one roster.
- Optional deployment dimensions decode to `Option`; `ring` alone has the owner-set `stable` default.

```typescript signature
import { Order, Schema } from "effect"
import { Shape } from "./schema.ts"

const _slug = <const Brand extends string>(
  brand: Brand,
  head: "letter" | "letter-or-digit",
  minimum: number,
  maximum: number,
) => Schema.String.pipe(
  Schema.pattern(new RegExp(`^[${head === "letter" ? "a-z" : "a-z0-9"}][a-z0-9-]*$`)),
  Schema.minLength(minimum),
  Schema.maxLength(maximum),
  Schema.pattern(/[a-z0-9]$/),
  Schema.brand(brand),
)

const _AppKey = _slug("AppKey", "letter", 2, 64)
const _TenantKey = _slug("TenantKey", "letter-or-digit", 1, 64)
const _Namespace = _slug("NamespaceKey", "letter", 2, 64)
const _Region = _slug("RegionKey", "letter", 2, 32)
const _Zone = _slug("ZoneKey", "letter", 2, 32)
const _Cluster = _slug("ClusterKey", "letter", 2, 64)
const _Version = Schema.String.pipe(
  Schema.pattern(/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/),
  Schema.brand("BuildVersion"),
)
const _Commit = Schema.String.pipe(Schema.pattern(/^[0-9a-f]{7,40}$/), Schema.brand("BuildCommit"))
const _Print = Schema.NonEmptyString.pipe(Schema.maxLength(128), Schema.brand("HostPrint"))
const _Scope = Schema.TemplateLiteral(_AppKey, "/", _TenantKey).pipe(Schema.brand("ScopeKey"))

const _environmentKinds = ["development", "test", "staging", "production"] as const
const _environmentRows = { development: {}, test: {}, staging: {}, production: {} } as const
const _environments = Shape.vocabulary(_environmentKinds, _environmentRows)
const _ringKinds = ["canary", "beta", "stable"] as const
const _ringRows = { canary: {}, beta: {}, stable: {} } as const
const _rings = Shape.vocabulary(_ringKinds, _ringRows)
// Ascending separation: `none` scopes nothing, `single` binds one tenant per deployment, `multi` separates inside one.
const _tenancyKinds = ["none", "single", "multi"] as const
const _tenancyRows = { none: {}, single: {}, multi: {} } as const
const _tenancies = Shape.vocabulary(_tenancyKinds, _tenancyRows)

const _scope = Schema.decodeSync(_Scope)
const _ScopeParts = Schema.TemplateLiteralParser(_AppKey, "/", _TenantKey)

class _Tenant extends Schema.Class<_Tenant>("Identity.Tenant")({ app: _AppKey, tenant: _TenantKey }) {
  static readonly alike = Schema.equivalence(_Tenant)
  static readonly FromScope: Schema.Schema<_Tenant, `${string}/${string}`> = Schema.transform(
    _ScopeParts,
    Schema.typeSchema(_Tenant),
    {
      strict: true,
      decode: ([app, , tenant]) => new _Tenant({ app, tenant }),
      encode: (context) => [context.app, "/", context.tenant] as const,
    },
  )
  get scope(): typeof _Scope.Type {
    return _scope(`${this.app}/${this.tenant}`)
  }
}

class _App extends Schema.Class<_App>("Identity.App")({
  app: _AppKey,
  tenant: Schema.optionalWith(_TenantKey, { as: "Option" }),
  namespace: Schema.optionalWith(_Namespace, { as: "Option" }),
  build: Schema.Struct({ version: _Version, commit: _Commit }),
  instance: Shape.Refined.Guid,
  host: _Print,
  environment: _environments.schema,
  ring: Schema.optionalWith(_rings.schema, { default: () => "stable" }),
  region: Schema.optionalWith(_Region, { as: "Option" }),
  zone: Schema.optionalWith(_Zone, { as: "Option" }),
  cluster: Schema.optionalWith(_Cluster, { as: "Option" }),
}) {
  static readonly alike = Schema.equivalence(_App)
  scoped(tenant: typeof _TenantKey.Type): _Tenant {
    return new _Tenant({ app: this.app, tenant })
  }
  reaches(floor: (typeof _environmentKinds)[number]): boolean {
    return Order.greaterThanOrEqualTo(_environments.order)(this.environment, floor)
  }
  admits(frontier: (typeof _ringKinds)[number]): boolean {
    return Order.lessThanOrEqualTo(_rings.order)(this.ring, frontier)
  }
  get label(): string {
    return `${this.app}@${this.build.version}`
  }
}

const Identity = {
  App: _App,
  Tenant: _Tenant,
  environment: _environments,
  ring: _rings,
  tenancy: _tenancies,
} as const

declare namespace Identity {
  export type App = _App
  export namespace App {
    export type Build = _App["build"]
    export type Cluster = typeof _Cluster.Type
    export type Commit = typeof _Commit.Type
    export type Environment = (typeof _environmentKinds)[number]
    export type Key = typeof _AppKey.Type
    export type Namespace = typeof _Namespace.Type
    export type Print = typeof _Print.Type
    export type Region = typeof _Region.Type
    export type Ring = (typeof _ringKinds)[number]
    export type Version = typeof _Version.Type
    export type Zone = typeof _Zone.Type
  }
  export type Tenancy = (typeof _tenancyKinds)[number]
  export type Tenant = _Tenant
  export namespace Tenant {
    export type Key = typeof _TenantKey.Type
    export type Scope = typeof _Scope.Type
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Identity }
```

## [02]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
