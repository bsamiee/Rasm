# [STORE_TENANT]

Tenancy is one closed policy family and one transaction transformer: `Tenancy` discriminates row-scoped RLS, schema-per-app, and database-per-app as tagged cases whose locus derives from the scope, and `Tenancy.within(tenant)` is the single write path that pins the `app.current_tenant` GUC — or the schema search path — with `SET LOCAL` inside `sql.withTransaction`, so isolation is transaction-scoped state the engine enforces, never a code fork. The GUC name honors the contract `security/authz` declares as the claim vocabulary; this page enforces it as RLS policy rows on every tenant-carrying relation.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          |
| :-----: | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `POLICY_FAMILY` | the `Tenancy` tagged family, the locus derivation, the isolation-shape dispatch   |
|  [02]   | `SCOPED_WRITE`  | `Tenancy.within` — the GUC/search-path `SET LOCAL` transformer, the RLS ensure    |

## [2]-[POLICY_FAMILY]

- Owner: `Tenancy` — one `Data.taggedEnum` family (`Rls`, `SchemaPerApp`, `DatabasePerApp`) whose constructor, `$is`/`$match` dispatch, and locus fold travel under one name.
- Packages: `effect` (`Data`, `Match`).
- Entry: policy values are constructed by the app root and carried inside `ScopeKey` — `scope/handle.md` dispatches Layer construction on them; no store code branches on tenancy outside `$match`.
- Growth: a new isolation shape is one case plus its `locus` arm and one `scope/handle.md` lookup arm — every consumer of the family breaks loudly until its arm exists.
- Law: `locus` derives the physical coordinate from the app key — `Rls` shares the default schema and isolates by row, `SchemaPerApp` derives `app_<key>` as the pinned schema, `DatabasePerApp` derives `app_<key>` as the database name a dedicated driver Layer opens; the derivation is one fold, so a naming change is one arm edit.
- Law: policy is a value, never configuration prose — the app root selects a case per app; hundreds of apps under mixed policies are rows in the `Stores` LayerMap, not deployments of different code.
- Boundary: the per-policy Layer construction (shared client versus dedicated database client) is `scope/handle.md`'s; the sqlite lanes replace this family wholesale with file-per-app (`lane/sqlite.md`'s degradation row) — no sqlite arm exists here by design.

```typescript
import { Data } from "effect"
import type { AppKey, TenantId } from "@rasm/ts/kernel"

type Tenancy = Data.TaggedEnum<{
  Rls: {}
  SchemaPerApp: {}
  DatabasePerApp: {}
}>

declare namespace Tenancy {
  type Locus = { readonly schema: "public" | `app_${string}`; readonly database: "shared" | `app_${string}` }
}

const _Tenancy = Data.taggedEnum<Tenancy>()

const _locus = (app: AppKey, tenancy: Tenancy): Tenancy.Locus =>
  _Tenancy.$match(tenancy, {
    Rls: (): Tenancy.Locus => ({ schema: "public", database: "shared" }),
    SchemaPerApp: (): Tenancy.Locus => ({ schema: `app_${app}`, database: "shared" }),
    DatabasePerApp: (): Tenancy.Locus => ({ schema: "public", database: `app_${app}` }),
  })
```

## [3]-[SCOPED_WRITE]

- Owner: `Tenancy.within` — the one transformer that opens the transaction and pins the tenant coordinate before any statement runs; plus the RLS ensure mint `Tenancy.rls(relation)` every tenant-carrying table registers.
- Packages: `@effect/sql` (`SqlClient`, `sql.withTransaction`, `SqlError`).
- Entry: every tenant-scoped unit of work is `Tenancy.within(tenant, effect)` — `journal/outbox.md`'s publish, `project/*` lane transactions, and `retrieve` reads all compose it; a statement touching tenant rows outside it reads zero rows under RLS, fail-closed by the policy's own predicate.
- Growth: a new session coordinate is one `SET LOCAL` line inside the transformer — the schema pin already rides the same seam, so search-path and tenant travel one write path.
- Law: transaction-local settings are the whole mechanism — the fence spells them `set_config(name, value, true)` because `set_config` binds parameters where bare `SET LOCAL` cannot; the setting dies at transaction end, nested `withTransaction` folds to savepoints under it, and no connection-level state survives to poison the pool — a `SET` without the local flag is the rejected spelling.
- Law: the RLS policy predicate reads `current_setting('app.current_tenant')` — the exact claim name `security/authz` declares; the ensure is a `DO` block because `CREATE POLICY` carries no `IF NOT EXISTS`, and idempotence is the ensure law.
- Law: `within` is dialect-honest — the pg arm pins GUC and search path; the sqlite arm degrades to the bare transaction because file-per-app already isolates (the `lane/sqlite.md` row), selected through `sql.onDialectOrElse`, never a fork.
- Boundary: who mints `TenantId` and how a request carries it is `security/authz`/`edge` material arriving as a value; this page owns only the transaction seam and the policy rows.

```typescript
import { Effect } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"

const _within = (tenant: TenantId, locus: Tenancy.Locus) =>
  <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<A, E | SqlError.SqlError, R | SqlClient.SqlClient> =>
    Effect.flatMap(SqlClient.SqlClient, (sql) =>
      sql.withTransaction(
        Effect.andThen(
          sql.onDialectOrElse({
            orElse: () => sql`SELECT 1`,
            pg: () => sql`SELECT set_config('app.current_tenant', ${tenant}, true), set_config('search_path', ${locus.schema}, true)`,
          }),
          work,
        ),
      ))

const _rlsEnsure = (relation: string): string =>
  `ALTER TABLE ${relation} ENABLE ROW LEVEL SECURITY;
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_policies WHERE tablename = '${relation}' AND policyname = 'tenant_isolation') THEN
    CREATE POLICY tenant_isolation ON ${relation}
      USING (tenant = current_setting('app.current_tenant'));
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Tenancy }
```
