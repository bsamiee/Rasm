# [DATA_TENANT]

Tenancy enforcement and the per-scope store family in one owner: `Tenancy` discriminates row-scoped RLS, schema-per-app, and database-per-app as tagged cases whose locus derives from the app key; `Tenancy.within` is the single write path that opens the transaction and pins the tenancy contract — `set_config` over the `TENANT_GUC` anchor `security/access/tenant` declares, read ambiently from the bound `TenantScope` reference so no query threads a tenant parameter; and `Stores` is the `LayerMap` family that turns a `ScopeKey` into a verified per-scope `SqlClient | Capability` subgraph, sharing one app-owned pool across row- and schema-isolated scopes through the pool-adoption row. The same scopes satisfy every security state port at the app root — the port-satisfaction table is this page's contract, and hundreds of apps under mixed isolation are rows in the map, never deployments of different code.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                            |
| :-----: | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `POLICY_FAMILY`     | the `Tenancy` tagged family, the locus derivation, the isolation dispatch              |
|  [02]   | `SCOPED_WRITE`      | `Tenancy.within` — the ambient GUC/search-path transformer, the RLS policy ensure      |
|  [03]   | `SCOPE_FAMILY`      | `ScopeKey`, the `Stores` LayerMap, shared-pool fan-out, verify-at-construction         |
|  [04]   | `PORT_SATISFACTION` | the security state ports satisfied by Layers built from these scopes                   |

## [2]-[POLICY_FAMILY]

- Owner: `Tenancy` — one `Data.taggedEnum` family (`Rls`, `SchemaPerApp`, `DatabasePerApp`) whose constructors, `$is`/`$match` dispatch, and locus fold travel under one name.
- Packages: `effect` (`Data`); `@rasm/ts/core` (`AppIdentity` — the app-key brand the locus derives from).
- Entry: policy values are constructed by the app root and carried inside `ScopeKey`; `[4]`'s lookup dispatches Layer construction on them, and no data code branches on tenancy outside `$match`.
- Growth: a new isolation shape is one case plus its `locus` arm and one lookup arm — every consumer breaks loudly until its arm exists.
- Law: `locus` derives the physical coordinate from the app key — `Rls` shares the default schema and isolates by row, `SchemaPerApp` pins `app_<key>` as the schema, `DatabasePerApp` opens `app_<key>` as a dedicated database; the derivation is one fold, so a naming change is one arm edit.
- Law: policy is a value, never configuration prose — the app root selects a case per app, and mixed policies coexist as map rows.
- Boundary: the sqlite profiles replace this family wholesale with file-per-app and database-per-tenant (`lane/sqlite.md`'s degradation rows) — no sqlite arm exists here by design.

```typescript
import { Data } from "effect"
import type { AppIdentity } from "@rasm/ts/core"

type Tenancy = Data.TaggedEnum<{
  Rls: {}
  SchemaPerApp: {}
  DatabasePerApp: {}
}>

type _Locus = { readonly schema: "public" | `app_${string}`; readonly database: "shared" | `app_${string}` }

const _Tenancy = Data.taggedEnum<Tenancy>()

const _locus = (app: AppIdentity.Key, tenancy: Tenancy): _Locus =>
  _Tenancy.$match(tenancy, {
    Rls: (): _Locus => ({ schema: "public", database: "shared" }),
    SchemaPerApp: (): _Locus => ({ schema: `app_${app}`, database: "shared" }),
    DatabasePerApp: (): _Locus => ({ schema: "public", database: `app_${app}` }),
  })
```

## [3]-[SCOPED_WRITE]

- Owner: `Tenancy.within` — the one transformer that opens the transaction and pins the tenancy coordinate before any statement runs, modal over its input: the ambient form reads the bound `TenantScope` reference, the explicit form takes a `TenantContext` value; plus `Tenancy.rls(relation)` — the idempotent RLS policy ensure every tenant-carrying relation registers, predicated on the `TENANT_GUC` anchor.
- Packages: `@effect/sql` (`SqlClient`, `sql.withTransaction`, `sql.onDialectOrElse`); `effect` (`Effect`, `Option`); `@rasm/ts/security` (`TenantScope`, `TENANT_GUC`); `@rasm/ts/core` (`TenantContext`).
- Entry: every tenant-scoped unit of work composes `within` — the journal's atomic publish, the projection transactions, the fact drain; a statement touching tenant rows outside it reads zero rows under RLS, fail-closed by the policy predicate itself.
- Growth: a new session coordinate (a shard key, a search-path override) is one `set_config` term inside the transformer — schema pin and tenant already ride the same seam.
- Law: the ambient form is the default road — the edge binds the request principal once through `TenantScope.bind`, this transformer reads the reference and pins the projected tenant; an unauthenticated principal pins nothing and RLS answers zero rows, so fail-closed needs no branch.
- Law: transaction-local settings are the whole mechanism — `set_config(name, value, true)` because bare `SET LOCAL` cannot bind parameters; the setting dies at transaction end, nested `withTransaction` folds to savepoints beneath it, and no connection-level state survives to poison the shared pool.
- Law: the GUC name is the imported `TENANT_GUC` anchor — the policy predicate, the transformer, and the security claim projection read one spelling, so a rename lands once at the declaring owner.
- Law: `within` is dialect-honest — the pg arm pins GUC and search path; the sqlite arm degrades to the bare transaction because file-per-app already isolates, selected through `onDialectOrElse`, never a fork.
- Boundary: who mints `TenantContext` and how a request carries it is security/edge material arriving through the reference; this page owns the transaction seam and the policy rows.

```typescript
import { Effect, Option } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { TENANT_GUC, TenantScope } from "@rasm/ts/security"
import type { TenantContext } from "@rasm/ts/core"

const _pin = (sql: SqlClient.SqlClient, tenant: Option.Option<TenantContext>, locus: _Locus) =>
  Option.match(tenant, {
    onNone: () => sql`SELECT set_config('search_path', ${locus.schema}, true)`,
    onSome: (context) =>
      sql`SELECT set_config(${TENANT_GUC}, ${context.tenant}, true), set_config('search_path', ${locus.schema}, true)`,
  })

const _within = (locus: _Locus) => {
  const transform = (tenant: Option.Option<TenantContext>) =>
    <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<A, E | SqlError.SqlError, R | SqlClient.SqlClient> =>
      Effect.flatMap(SqlClient.SqlClient, (sql) =>
        sql.withTransaction(
          Effect.andThen(
            sql.onDialectOrElse({
              orElse: () => sql`SELECT 1`,
              pg: () => _pin(sql, tenant, locus),
            }),
            work,
          ),
        ))
  function within<A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<A, E | SqlError.SqlError, R | SqlClient.SqlClient>
  function within<A, E, R>(tenant: TenantContext, work: Effect.Effect<A, E, R>): Effect.Effect<A, E | SqlError.SqlError, R | SqlClient.SqlClient>
  function within<A, E, R>(
    first: TenantContext | Effect.Effect<A, E, R>,
    rest?: Effect.Effect<A, E, R>,
  ): Effect.Effect<A, E | SqlError.SqlError, R | SqlClient.SqlClient> {
    return Effect.isEffect(first)
      ? TenantScope.scoped((principal) => transform(principal.context)(first))
      : transform(Option.some(first))(rest ?? Effect.dieMessage("<within:missing-work>"))
  }
  return within
}

const _rlsEnsure = (relation: string): string =>
  `ALTER TABLE ${relation} ENABLE ROW LEVEL SECURITY;
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = '${relation}' AND policyname = 'tenant_isolation') THEN
    CREATE POLICY tenant_isolation ON ${relation}
      USING (tenant = current_setting('${TENANT_GUC}'));
  END IF;
END $$;`

const Tenancy: Data.TaggedEnum.Constructor<Tenancy> & {
  readonly locus: typeof _locus
  readonly within: typeof _within
  readonly rls: typeof _rlsEnsure
} = {
  ..._Tenancy,
  locus: _locus,
  within: _within,
  rls: _rlsEnsure,
}
```

## [4]-[SCOPE_FAMILY]

- Owner: `ScopeKey` — the `(app, tenancy)` structural identity — and `Stores`, the one `LayerMap.Service` whose lookup dispatches the tenancy family into a per-scope `SqlClient | Capability` subgraph: base client selected by isolation case, capability probes and ensure verification composed as a discard node, dialect core grants seeded.
- Packages: `effect` (`Data`, `LayerMap`, `Layer`, `Duration`, `Effect`); `lane/postgres.md` (`Pg.client`, `Pg.fromPool`, `Pg.rows`, `Pg.core`); `lane/capability.md` (`Capability`); the journal pages publish the ensure roster as data.
- Entry: `Stores.get(scope)` yields the keyed Layer to provide around any data effect; `Stores.invalidate(scope)` evicts on revocation, credential rotation, or a poisoned pool — the next acquisition rebuilds while every other scope keeps its instance.
- Receipt: the scope's `Capability.Report` is readable through the provided service — startup verification evidence per scope, never a global assumption.
- Growth: a new tenancy arm is one `$match` arm in `_lookup`; a new verified surface merges its ensure rows into the roster the app root passes; the sqlite profiles publish their own layer rows and never enter this lookup.
- Law: the shared spine is one adopted pool — `Rls` and `SchemaPerApp` scopes share ONE `pooled` arm body by declaration (their store construction is identical; the tenancy difference is the search-path pin inside `Tenancy.within`), so a diamond of N apps on one database costs one pool; `DatabasePerApp` builds a dedicated `Pg.client` whose database is the scope's locus.
- Law: verification is construction — `Layer.effectDiscard` runs probing and relation verification inside the lookup, so a Layer returned from `Stores.get` IS the proof the scope is provisioned; `idleTimeToLive` retires an unreferenced scope's resources without touching hot scopes.
- Law: the key is the whole coordinate — anything changing which physical subgraph serves the scope is a `ScopeKey` field; anything varying per request (the tenant of a call) stays out and rides `Tenancy.within`.
- Law: the roster arrives ambiently — `Wiring` is a `Context.Reference` carrying the shared-pool Layer and the collected ensure rows; the app root overrides it once with `Layer.succeed(Wiring, { shared, ensures })`, the lookup reads it through `Layer.unwrapEffect`, and an unwired root builds against the default (dedicated clients, empty roster) instead of failing on a phantom import.

```typescript
import { Context, Duration, Layer, LayerMap } from "effect"
import type { ConfigError } from "effect"
import { Capability } from "./capability.ts"
import { Pg } from "./postgres.ts"

class ScopeKey extends Data.Class<{
  readonly app: AppIdentity.Key
  readonly tenancy: Tenancy
}> {}

class Wiring extends Context.Reference<Wiring>()("data/Wiring", {
  defaultValue: (): {
    readonly shared: Layer.Layer<SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError>
    readonly ensures: ReadonlyArray<Capability.Ensure>
  } => ({ shared: Pg.client("shared"), ensures: [] }),
}) {}

declare namespace Stores {
  type Provided = SqlClient.SqlClient | Capability
}

const _verified = (ensures: ReadonlyArray<Capability.Ensure>): Layer.Layer<Capability, SqlError.SqlError | Capability.Fault, SqlClient.SqlClient> =>
  Capability.Default(Pg.rows, ensures, Pg.core.pg)

const _lookup = (
  scope: ScopeKey,
): Layer.Layer<Stores.Provided, ConfigError.ConfigError | SqlError.SqlError | Capability.Fault> =>
  Layer.unwrapEffect(
    Effect.map(Wiring, (wiring) => {
      const pooled = () => _verified(wiring.ensures).pipe(Layer.provideMerge(wiring.shared))
      return Tenancy.$match(scope.tenancy, {
        Rls: pooled,
        SchemaPerApp: pooled,
        DatabasePerApp: () =>
          _verified(wiring.ensures).pipe(Layer.provideMerge(Pg.client(Tenancy.locus(scope.app, scope.tenancy).database))),
      })
    }),
  )

class Stores extends LayerMap.Service<Stores>()("data/Stores", {
  lookup: _lookup,
  idleTimeToLive: Duration.minutes(10),
}) {
  static readonly port = <Self, Shape>(
    scope: ScopeKey,
    tag: Context.Tag<Self, Shape>,
    build: Effect.Effect<Shape, SqlError.SqlError | Capability.Fault, Stores.Provided>,
  ): Layer.Layer<Self, ConfigError.ConfigError | SqlError.SqlError | Capability.Fault, Stores> =>
    Layer.effect(tag, build).pipe(Layer.provide(Stores.get(scope)))
}
```

## [5]-[PORT_SATISFACTION]

- Owner: the port-satisfaction contract — every security state port is satisfied at the app root by a Layer built FROM a `Stores` scope, and `Stores.port` is the one combinator that binds a port Tag to its statement implementation over a scope's verified subgraph.
- Packages: `effect` (`Layer`); the port Tags arrive from `@rasm/ts/security` at the composition root, never imported by the neutral rows.
- Entry: the app root composes `Stores.port(scope, Tag, build)` per port; the builds are ordinary statement folds over the scope's `SqlClient`, their tables published as ensure rows in the roster.
- Growth: a new security port is one table ensure plus one `port` composition at the root — the map, the verification, and the isolation dispatch are already settled.
- Law: the satisfaction rows are the folder's standing obligations — `SessionStore`/`IdentityJournal` (session and identity state), `ClaimStore`/`RelationStore` (entitlement and relation tuples), `ApiKeyStore` (machine credentials), `WebAuthnStore`/`ChallengeStore` (passkey material and ceremonies), `OAuthStateStore` (redirect state), `PublicKeyStore`/`JwksLedger` (verification keys); each is a Tag the security folder declares and a Layer this folder's scopes back.
- Law: the per-subject `WrappedKey` persistence that drives crypto-shredding rides `journal/retain.md`'s subject ledger — the security `Shredder` mints and wraps, this folder stores and destroys; destruction is the erasure verb.
- Law: security never imports data and data never imports the port implementations' callers — the Tags meet the Layers only at the app root, keeping the folder edge exactly one direction: `data → security` for `TenantScope`/`TENANT_GUC`/`Shredder` values only.

```typescript
// --- [EXPORTS] --------------------------------------------------------------------------

export { ScopeKey, Stores, Tenancy, Wiring }
```
