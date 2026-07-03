# [STORE_HANDLE]

`StoreHandle` is how hundreds of apps get isolated durable state from one surface: a `LayerMap.Service` keyed by `ScopeKey` — the `(appKey, tenancy policy)` value — whose lookup dispatches the tenancy family into a per-scope `SqlClient` subgraph, verifies capability rows and ensure relations during construction, and caches the built Layer until idle expiry. Isolation is a scope value: requesting a scope yields its Layer, revoking one is `invalidate`, and no fork, no per-app deployment, and no second wiring path exist. This page is the `./server` composition seam — the pg driver is admitted here and on the lane pages only, never on the neutral vocabulary subpaths.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                            |
| :-----: | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `SCOPE_KEY`    | the `ScopeKey` value — structural identity for the LayerMap, locus projection        |
|  [02]   | `LAYER_FAMILY` | the `Stores` LayerMap service — lookup dispatch, verify-at-construction, idle policy |

## [2]-[SCOPE_KEY]

- Owner: `ScopeKey` — one `Data.Class` carrying `app` and `tenancy`; construction implants `Equal`/`Hash`, so the LayerMap keys structurally and two requests for one scope share one subgraph.
- Packages: `effect` (`Data`); `@rasm/ts/kernel` (`AppKey` — the same value `telemetry` Resources and `browser` boot derive from `AppIdentity`).
- Growth: a new scope dimension (region, shard) is one field here plus its locus consequence in `Tenancy.locus` — every keyed cache re-keys automatically because identity is structural.
- Law: the key is the whole coordinate — anything that changes which physical subgraph serves the scope belongs in the key; anything that varies per request (the tenant of a call) stays out and rides `Tenancy.within`.

```typescript
import { Data } from "effect"
import type { AppKey } from "@rasm/ts/kernel"
import { Tenancy } from "./tenant.ts"

class ScopeKey extends Data.Class<{
  readonly app: AppKey
  readonly tenancy: Tenancy
}> {}
```

## [3]-[LAYER_FAMILY]

- Owner: `Stores` — the one `LayerMap.Service` whose `lookup` turns a `ScopeKey` into the scope's `SqlClient | Capability` Layer: base client selected by tenancy dispatch, capability probes and ensure verification composed as a discard node, core grants appended by dialect.
- Packages: `effect` (`LayerMap`, `Layer`, `Duration`, `Config`, `Redacted`); `@effect/sql-pg` (`PgClient.layer`, `PgClient.layerConfig`, `PgClientConfig`); `capability/row.md` (`Capability`), `capability/matrix.md` (`Matrix`), `scope/tenant.md` (`Tenancy`).
- Entry: `Stores.get(scope)` yields the keyed Layer to provide around any store effect; `Stores.runtime(scope)` the keyed runtime under `Scope`; `Stores.invalidate(scope)` evicts on revocation, credential rotation, or a poisoned pool — the next acquisition rebuilds while every other scope keeps its instance.
- Receipt: the capability report of the scope is readable through the provided `Capability` service — startup verification evidence per scope, never a global assumption.
- Growth: a new tenancy arm is one `$match` arm in `_lookup`; a new verified surface is one more ensure array merged into `_ensures`; the sqlite lanes publish their own layer rows (`lane/sqlite.md`, `lane/wasm.md`) and never enter this pg lookup.
- Law: the shared spine is one memoized reference — `Rls` and `SchemaPerApp` scopes compose `_shared`, so a diamond of N apps on one database costs one pool; `DatabasePerApp` builds a dedicated `PgClient.layer` whose `database` is the scope's locus, and pool budgets stay `Config` facts, never literals.
- Law: verification is construction — `Layer.effectDiscard` runs `Capability` probing and relation verification inside the lookup, so `Stores.get` returning a Layer IS the proof the scope is provisioned; `idleTimeToLive` retires an unreferenced scope's pool without touching hot scopes.
- Law: `_ensures` is the folder-standing DDL roster — `Journal.ddl`, `Outbox.ddl`, `Snapshot.ddl`, `Retain.ddl`, `AsyncLane.ddl` — applied by `iac`, proven here; surfaces that bind per app verify at their own construction (`ObjectStore` its ref table, `Index.ensure` per corpus, inline lanes their read-model tables), so the roster never grows per app.
- Boundary: which driver a runtime lane binds is the app root's Layer selection on the `./server` subpath; `security/session` and `telemetry` journal ports are satisfied by Layers built FROM these scopes at the app root — `store` never imports those folders.

```typescript
import { Config, type ConfigError, Duration, Effect, Layer, LayerMap } from "effect"
import { PgClient } from "@effect/sql-pg"
import type { SqlClient, SqlError } from "@effect/sql"
import { Capability } from "../capability/row.ts"
import { Matrix } from "../capability/matrix.ts"
import { Journal } from "../journal/append.ts"
import { Outbox } from "../journal/outbox.ts"
import { Snapshot } from "../journal/snapshot.ts"
import { Retain } from "../journal/retain.ts"
import { AsyncLane } from "../project/async.ts"

declare namespace Stores {
  type Provided = SqlClient.SqlClient | Capability
}

const _ensures: ReadonlyArray<Capability.Ensure> = [
  ...Journal.ddl,
  ...Outbox.ddl,
  ...Snapshot.ddl,
  ...Retain.ddl,
  ...AsyncLane.ddl,
]

const _pool = Config.unwrap({
  url: Config.redacted("STORE_PG_URL"),
  maxConnections: Config.integer("STORE_PG_POOL_MAX").pipe(Config.withDefault(16)),
  connectionTTL: Config.duration("STORE_PG_CONN_TTL").pipe(Config.withDefault(Duration.minutes(15))),
})

const _client = (database: string): Layer.Layer<PgClient.PgClient | SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError> =>
  Layer.unwrapEffect(
    Effect.map(_pool, (pool) =>
      PgClient.layer({
        url: pool.url,
        database,
        maxConnections: pool.maxConnections,
        connectionTTL: pool.connectionTTL,
        applicationName: "store",
      })),
  )

const _shared = _client("shared")

const _verified = (ensures: ReadonlyArray<Capability.Ensure>): Layer.Layer<Capability, SqlError.SqlError | Capability.Fault, SqlClient.SqlClient> =>
  Capability.Default(Matrix.rows, ensures, Matrix.core.pg)

const _lookup = (
  scope: ScopeKey,
  ensures: ReadonlyArray<Capability.Ensure>,
): Layer.Layer<Stores.Provided, ConfigError.ConfigError | SqlError.SqlError | Capability.Fault> =>
  Tenancy.$match(scope.tenancy, {
    Rls: () => _verified(ensures).pipe(Layer.provideMerge(_shared)),
    SchemaPerApp: () => _verified(ensures).pipe(Layer.provideMerge(_shared)),
    DatabasePerApp: () =>
      _verified(ensures).pipe(Layer.provideMerge(_client(Tenancy.locus(scope.app, scope.tenancy).database))),
  })

class Stores extends LayerMap.Service<Stores>()("store/Stores", {
  lookup: (scope: ScopeKey) => _lookup(scope, _ensures),
  idleTimeToLive: Duration.minutes(10),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ScopeKey, Stores }
```
