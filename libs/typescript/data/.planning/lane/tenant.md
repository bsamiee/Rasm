# [DATA_TENANT]

Tenancy enforcement and the per-scope store family in one owner: `Tenancy` discriminates row-scoped RLS, schema-per-app, and database-per-app as tagged cases whose locus derives from the app key; `Tenant` is the per-scope service whose `within` member is the ONLY road to a `SqlClient` under a scope — it opens the transaction, folds the security-declared `SessionCoordinate` table over the bound principal so tenant, scope, subject, and search path pin transaction-locally, and only then provides the client into the work, so a tenant relation statement outside the pinned boundary is unspellable rather than undisciplined; and `Stores` is the `LayerMap` family that turns a `ScopeKey` into a verified per-scope `Tenant | Capability` subgraph, sharing one app-owned pool across row- and schema-isolated scopes through the pool-adoption row while the raw client stays interior to the lookup. The same scopes satisfy every security state port at the app root — the port-satisfaction table is this page's contract, and hundreds of apps under mixed isolation are rows in the map, never deployments of different code.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                          |
| :-----: | :------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `POLICY_FAMILY`     | the `Tenancy` tagged family, the locus derivation, the isolation dispatch       |
|  [02]   | `SCOPED_WRITE`      | the coordinate pin fold, the `within` transformer, the RLS policy ensure        |
|  [03]   | `SCOPE_FAMILY`      | `ScopeKey`, the `Tenant` service, the `Stores` LayerMap, verify-at-construction |
|  [04]   | `PORT_SATISFACTION` | the security state ports satisfied by Layers built from these scopes            |

## [02]-[POLICY_FAMILY]

- Owner: `Tenancy` — one `Data.taggedEnum` family (`Rls`, `SchemaPerApp`, `DatabasePerApp`) whose constructors, `$is`/`$match` dispatch, locus fold, and RLS ensure travel under one name.
- Packages: `effect` (`Data`); `@rasm/ts/core` (`AppIdentity` — the app-key brand the locus derives from).
- Entry: policy values are constructed by the app root and carried inside `ScopeKey`; `[4]`'s lookup dispatches Layer construction on them, and no data code branches on tenancy outside `$match`.
- Growth: a new isolation shape is one case plus its `locus` arm and one lookup arm — every consumer breaks loudly until its arm exists.
- Law: `locus` derives the physical coordinate from the app key — `Rls` shares the default schema and isolates by row, `SchemaPerApp` pins `app_<key>` as the schema, `DatabasePerApp` opens `app_<key>` as a dedicated database; the derivation is one fold, so a naming change is one arm edit.
- Law: policy is a value, never configuration prose — the app root selects a case per app, and mixed policies coexist as map rows.
- Boundary: the sqlite profiles replace this family wholesale with file-per-app and database-per-tenant (`lane/sqlite.md`'s degradation rows) — no sqlite arm exists here by design.

```typescript signature
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

## [03]-[SCOPED_WRITE]

- Owner: `_within(sql, locus)` — the transformer the `Tenant` service publishes: it opens the transaction, pins the tenancy coordinates, PROVIDES the captured client and commit registrar into the work, then drains registered effects only after commit; its input is modal — the ambient form reads the bound `TenantScope` reference, the explicit form takes a `TenantContext` value; plus `Tenancy.rls(relation)` — the idempotent RLS policy ensure every tenant-carrying relation registers, predicated on the `TENANT_GUC` anchor.
- Packages: `@effect/sql` (`SqlClient`, `sql.withTransaction`, `sql.onDialectOrElse`); `effect` (`Context.Tag`, `Effect.context`, `Effect.provide`, `Option`, `Record`, `Ref`, `Schema`); `@rasm/ts/security` (`TenantScope`, `SessionCoordinate`, `TENANT_GUC`); `@rasm/ts/core` (`TenantContext`).
- Entry: every tenant-scoped unit of work composes `Tenant.within` — the journal's atomic publish, the projection transactions, the fact drain; the requirement channel is the enforcement: `SqlClient` is satisfied only inside the transformer, so a statement cannot exist under a scope without the pin, and an unauthenticated principal pins nothing so RLS answers zero rows.
- Growth: a new session coordinate (a shard key, a region pin) is one `SessionCoordinate` row at the declaring security owner — the fold here inherits it with zero edits, because the pin iterates the imported table instead of naming coordinates.
- Law: the pin is the coordinate-table fold — `_pin` walks `SessionCoordinate` rows, projects each over the bound principal, and pins every `Some` through `set_config(row.guc, value, true)` plus the locus search path; the security page declares the vocabulary, this fold is its one write path, so tenant, scope, and audit subject travel together and a rename lands at the declaring owner.
- Law: the ambient form is the default road — the edge binds the request principal once through `TenantScope.bind`, the transformer reads the reference and pins the projections; an unset coordinate pins nothing and the policy predicate reads `current_setting(name, true)` — the `missing_ok` arm folds an unset GUC to NULL instead of raising, so fail-closed needs no branch and no error path.
- Law: transaction-local settings are the whole mechanism — `set_config(name, value, true)` because bare `SET LOCAL` cannot bind parameters; the setting dies at transaction end, nested `withTransaction` folds to savepoints beneath it, and no connection-level state survives to poison the shared pool. `Tenant.afterCommit` registers a never-failing effect in the invocation-local commit roster; `_within` drains that roster only after the outer transaction succeeds, so a savepoint release or rollback cannot publish external evidence.
- Law: the GUC names are the imported `SessionCoordinate` anchors — the policy predicate, the transformer, and the security claim projection read one spelling; `Tenancy.Relation` validates every page-authored ensure relation against the SQL identifier grammar before `rls` interpolates it, so caller text and qualified or quoted identifiers cannot reach the DDL kernel.
- Law: `within` is dialect-honest — the pg arm pins the coordinates and search path; the sqlite arm degrades to the bare transaction because file-per-app already isolates, selected through `onDialectOrElse`, never a fork.
- Boundary: who mints `TenantContext` and how a request carries it is security/edge material arriving through the reference; this page owns the transaction seam and the policy rows.

```typescript signature
import { Context, Effect, Option, Record, Ref, Schema } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import { type Principal, SessionCoordinate, TENANT_GUC, TenantScope } from "@rasm/ts/security"
import type { TenantContext } from "@rasm/ts/core"

const _pin = (sql: SqlClient.SqlClient, principal: Principal, locus: _Locus) =>
  Effect.andThen(
    sql`SELECT set_config('search_path', ${locus.schema}, true)`,
    Effect.forEach(
      Record.values(SessionCoordinate),
      (row) =>
        Option.match(row.read(principal), {
          onNone: () => Effect.void,
          onSome: (value) => sql`SELECT set_config(${row.guc}, ${value}, true)`,
        }),
      { discard: true },
    ),
  )

type _CommitEffect = Effect.Effect<void, never>

class _Commit extends Context.Tag("data/TenantCommit")<_Commit, {
  readonly register: <R>(effect: Effect.Effect<void, never, R>) => Effect.Effect<void, never, R>
}>() {}

const _afterCommit = <R>(effect: Effect.Effect<void, never, R>): Effect.Effect<void, never, R | _Commit> =>
  Effect.flatMap(_Commit, (commit) => commit.register(effect))

const _within = (sql: SqlClient.SqlClient, locus: _Locus) => {
  const transform = (principal: Principal) =>
    <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<
      A,
      E | SqlError.SqlError,
      Exclude<R, SqlClient.SqlClient | _Commit>
    > =>
      Effect.gen(function* () {
        const hooks = yield* Ref.make<ReadonlyArray<_CommitEffect>>([])
        const commit = {
          register: <R2>(effect: Effect.Effect<void, never, R2>): Effect.Effect<void, never, R2> =>
            Effect.flatMap(Effect.context<R2>(), (context) =>
              Ref.update(hooks, (held) => [...held, Effect.provide(effect, context)])),
        }
        const value = yield* sql.withTransaction(
          Effect.andThen(
            sql.onDialectOrElse({
              orElse: () => sql`SELECT 1`,
              pg: () => _pin(sql, principal, locus),
            }),
            work.pipe(
              Effect.provideService(SqlClient.SqlClient, sql),
              Effect.provideService(_Commit, commit),
            ),
          ),
        )
        yield* Effect.all(yield* Ref.get(hooks), { discard: true })
        return value
      })
  function within<A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<
    A,
    E | SqlError.SqlError,
    Exclude<R, SqlClient.SqlClient | _Commit> | TenantScope
  >
  function within<A, E, R>(tenant: TenantContext, work: Effect.Effect<A, E, R>): Effect.Effect<
    A,
    E | SqlError.SqlError,
    Exclude<R, SqlClient.SqlClient | _Commit>
  >
  function within<A, E, R>(
    ...input: readonly [work: Effect.Effect<A, E, R>] | readonly [tenant: TenantContext, work: Effect.Effect<A, E, R>]
  ): Effect.Effect<A, E | SqlError.SqlError, Exclude<R, SqlClient.SqlClient | _Commit> | TenantScope> {
    return input.length === 1
      ? TenantScope.scoped((principal) => transform(principal)(input[0]))
      : transform({ context: Option.some(input[0]), subject: Option.none() })(input[1])
  }
  return within
}

const _Relation = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z][a-z0-9_]*$/), Schema.brand("TenancyRelation"))

const _rlsEnsure = (input: string): string => {
  const relation = Schema.decodeSync(_Relation)(input)
  return `ALTER TABLE ${relation} ENABLE ROW LEVEL SECURITY;
ALTER TABLE ${relation} FORCE ROW LEVEL SECURITY;
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE schemaname = current_schema() AND tablename = '${relation}' AND policyname = 'tenant_isolation') THEN
    CREATE POLICY tenant_isolation ON ${relation}
      USING (tenant = current_setting('${TENANT_GUC}', true));
  END IF;
END $$;`
}

const Tenancy: Data.TaggedEnum.Constructor<Tenancy> & {
  readonly Relation: typeof _Relation
  readonly locus: typeof _locus
  readonly rls: typeof _rlsEnsure
} = {
  ..._Tenancy,
  Relation: _Relation,
  locus: _locus,
  rls: _rlsEnsure,
}
```

## [04]-[SCOPE_FAMILY]

- Owner: `ScopeKey` — the `(app, tenancy)` structural identity — `Tenant`, the per-scope service publishing the locus and the `within` write path, and `Stores`, the one `LayerMap.Service` whose lookup dispatches the tenancy family into a per-scope `Tenant | Capability` subgraph: base client selected by isolation case and CONSUMED by the subgraph (`Layer.provide`, never `provideMerge`), capability probes and ensure verification composed at construction, dialect core grants and demand pairs seeded.
- Packages: `effect` (`Data`, `LayerMap`, `Layer`, `Duration`, `Effect`, `Context`); `lane/postgres.md` (`Pg.client`, `Pg.fromPool`, `Pg.rows`, `Pg.core`, `Pg.demands`); `lane/capability.md` (`Capability`); the journal pages publish the ensure roster as data.
- Entry: `Stores.get(scope)` yields the keyed Layer to provide around any data effect; `Stores.invalidate(scope)` evicts on revocation, credential rotation, or a poisoned pool — the next acquisition rebuilds while every other scope keeps its instance.
- Receipt: the scope's `Capability.Report` is readable through the provided service — startup verification evidence per scope, never a global assumption.
- Growth: a new tenancy arm is one `$match` arm in `_lookup`; a new verified surface merges its ensure rows into the roster the app root passes; the sqlite profiles publish their own layer rows and never enter this lookup.
- Law: the raw client never leaves the lookup — the subgraph provides `Tenant | Capability` and hides `SqlClient` behind `Layer.provide`, so a port build under `Stores.get(scope)` can reach a client ONLY through `Tenant.within` and the tenancy pin is structural on the shared-pool path and the dedicated-database path alike.
- Law: the shared spine is one adopted pool — `Rls` and `SchemaPerApp` scopes share ONE `pooled` arm body by declaration (their store construction is identical; the tenancy difference is the locus fold inside `within`), so a diamond of N apps on one database costs one pool; `DatabasePerApp` builds a dedicated `Pg.client` whose database is the scope's locus.
- Law: verification is construction at the physical locus — `Capability.Default` receives `locus.schema`, so schema-per-app scopes prove their own relations rather than whichever schema the adopted pool last exposed; probing, relation verification, and demand closure complete inside the lookup before the Layer exists. `idleTimeToLive` retires an unreferenced scope's resources without touching hot scopes.
- Law: the key is the whole coordinate — anything changing which physical subgraph serves the scope is a `ScopeKey` field; anything varying per request (the tenant of a call) stays out and rides `Tenant.within`.
- Law: the roster is required construction input — `Wiring` is a `Context.Tag` carrying the shared-pool Layer and the complete ensure rows; the app root provides it once with `Layer.succeed(Wiring, { shared, ensures })`, the lookup reads it through `Layer.unwrapEffect`, and an unwired root fails the composition proof instead of silently verifying an empty schema roster.

```typescript signature
import { Context, Duration, Layer, LayerMap } from "effect"
import type { ConfigError, ParseResult } from "effect"
import { Capability } from "./capability.ts"
import { Pg } from "./postgres.ts"

class ScopeKey extends Data.Class<{
  readonly app: AppIdentity.Key
  readonly tenancy: Tenancy
}> {}

class Tenant extends Effect.Service<Tenant>()("data/Tenant", {
  effect: (locus: _Locus) =>
    Effect.map(SqlClient.SqlClient, (sql) => ({
      locus,
      within: _within(sql, locus),
      afterCommit: _afterCommit,
    })),
}) {}

class Wiring extends Context.Tag("data/Wiring")<Wiring, {
  readonly shared: Layer.Layer<SqlClient.SqlClient, ConfigError.ConfigError | SqlError.SqlError>
  readonly ensures: ReadonlyArray<Capability.Ensure>
}>() {}

declare namespace Stores {
  type Provided = Tenant | Capability
}

const _verified = (
  locus: _Locus,
  ensures: ReadonlyArray<Capability.Ensure>,
): Layer.Layer<Capability, SqlError.SqlError | ParseResult.ParseError | Capability.Fault, SqlClient.SqlClient> =>
  Capability.Default({ rows: Pg.rows, ensures, core: Pg.core.pg, demands: Pg.demands, schema: locus.schema })

const _lookup = (
  scope: ScopeKey,
): Layer.Layer<Stores.Provided, ConfigError.ConfigError | SqlError.SqlError | ParseResult.ParseError | Capability.Fault> =>
  Layer.unwrapEffect(
    Effect.map(Wiring, (wiring) => {
      const locus = Tenancy.locus(scope.app, scope.tenancy)
      const subgraph = Layer.mergeAll(Tenant.Default(locus), _verified(locus, wiring.ensures))
      const client = Tenancy.$match(scope.tenancy, {
        Rls: () => wiring.shared,
        SchemaPerApp: () => wiring.shared,
        DatabasePerApp: () => Pg.client(locus.database),
      })
      return subgraph.pipe(Layer.provide(client)) // provide, never provideMerge: the raw client dies inside the lookup
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
  ): Layer.Layer<Self, ConfigError.ConfigError | SqlError.SqlError | ParseResult.ParseError | Capability.Fault, Stores> =>
    Layer.effect(tag, build).pipe(Layer.provide(Stores.get(scope)))
}
```

## [05]-[PORT_SATISFACTION]

- Owner: the port-satisfaction contract — every security state port is satisfied at the app root by a Layer built FROM a `Stores` scope, and `Stores.port` is the one combinator that binds a port Tag to its statement implementation over a scope's verified subgraph.
- Packages: `effect` (`Layer`); the port Tags arrive from `@rasm/ts/security` at the composition root, never imported by the neutral rows.
- Entry: the app root composes `Stores.port(scope, Tag, build)` per port; the builds are ordinary statement folds run through the scope's `Tenant.within`, their tables published as ensure rows in the roster.
- Growth: a new security port is one table ensure plus one `port` composition at the root — the map, the verification, and the isolation dispatch are already settled.
- Law: the satisfaction rows are the folder's standing obligations — `SessionStore`/`IdentityJournal` (session and identity state), `ClaimStore`/`RelationStore` (entitlement and relation tuples), `ApiKeyStore` (machine credentials), `WebAuthnStore`/`ChallengeStore` (passkey material and the `SingleUse` ceremony phase), `OAuthStateStore` (the `SingleUse` redirect snapshot), `PublicKeyStore`/`JwksLedger` (verification keys), and the `@effect/experimental` `RateLimiter.RateLimiterStore` (the credential-verify throttle budgets); each is a Tag the security folder declares — the `SingleUse` ports as TTL single-consume contracts a `Cache`/`PersistedCache` row satisfies — and a Layer this folder's scopes back.
- Law: the per-subject `WrappedKey` persistence that drives crypto-shredding rides `journal/retain.md`'s subject ledger — the security `Shredder` mints and wraps, this folder stores and destroys; destruction is the erasure verb.
- Law: security never imports data and data never imports the port implementations' callers — the Tags meet the Layers only at the app root, keeping the folder edge exactly one direction: `data → security` for `TenantScope`/`SessionCoordinate`/`TENANT_GUC`/`Shredder` values only.

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { ScopeKey, Stores, Tenancy, Tenant, Wiring }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
