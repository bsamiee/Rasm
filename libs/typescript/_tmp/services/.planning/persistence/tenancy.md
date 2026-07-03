# [SERVICES_TENANCY]

The multi-tenant RLS scoping axis over the one `PgClient` — `TenantScope`, scoping every entity by `app_id` FK through the `app.current_tenant` GUC the RLS predicate reads, the tenant lifecycle vocabulary, and the purge-handler family, set per request at the connection boundary. The tenant key is consumed as the RLS predicate input, never re-minted branch-side. RLS scoping rides the one `PgClient` GUC, never a branch-side WHERE-clause filter. This is a node-only surface and crosses no .NET wire.

## [01]-[INDEX]

- [01]-[TENANCY]: owns the multi-tenant RLS axis, the tenant lifecycle vocabulary, and the purge-handler family.

## [02]-[TENANCY]

- Owner: `TenantScope`, the multi-tenant RLS axis carrying `withTenant` (the per-request GUC-binding decorator) and `purge` (the handler family), over the `TenantLifecycle` and `PurgeTarget` vocabularies.
- Cases: every entity is RLS-scoped by `app_id` FK against the `app.current_tenant` GUC the row-level-security predicate reads (`current_setting('app.current_tenant')`); the tenant lifecycle is `active`/`suspended`/`archived`/`purging`; the purge handler family is sessions/api-keys/assets/event-journal/job-dlq/kv-store/mfa-secrets/oauth-accounts/archive/purge-tenant, run in FK-dependency order under the one client.
- Entry: `withTenant` sets the `app.current_tenant` GUC for the wrapped effect's connection so every query inside it reads the RLS-scoped row set; the tenant context arrives from the upstream wire surface as settled vocabulary and is consumed as the RLS predicate input on the task that wires it, never re-minted here.
- Packages: `@effect/sql-pg` for the `set_config` GUC binding over the one `PgClient`.
- Growth: a new tenant lifecycle state lands as one `TenantLifecycle` literal; a new purge handler lands as one `PurgeTarget` literal, never a second tenant axis.
- Boundary: the tenant key is consumed as the RLS predicate input, never re-minted; RLS scoping rides the one `PgClient` GUC and never a branch-side WHERE-clause filter; this is a node-only surface, never browser-reachable.

```ts contract
const TenantLifecycle = Schema.Literal("active", "suspended", "archived", "purging");
const PurgeTarget = Schema.Literal(
  "sessions", "api-keys", "assets", "event-journal", "job-dlq", "kv-store", "mfa-secrets", "oauth-accounts", "archive", "purge-tenant",
);
type PurgeTarget = Schema.Schema.Type<typeof PurgeTarget>;

interface TenantScope {
  readonly withTenant: <A, E, R>(tenant: string, eff: Effect.Effect<A, E, R>) => Effect.Effect<A, E | SqlError.SqlError, R | PgClient.PgClient>;
  readonly purge: (tenant: string, target: PurgeTarget) => Effect.Effect<number, SqlError.SqlError, PgClient.PgClient>;
}
```
