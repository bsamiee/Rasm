# [PERSISTENCE_TENANCY]

Rasm.Persistence owns the multi-tenancy and row-level-security axis on the self-provisioned PostgreSQL 18.4 server tier: `TenancyModel` is the `[SmartEnum<string>]` isolation-strategy axis tying the host profile to a `CREATE POLICY`; `TenantProvision` is the single per-model lifecycle owner folding create-DDL, teardown-DDL, and RLS-policy SQL from one `model.Switch` over the `TenantPhase` discriminant; `TenantQuota` is the per-tenant resource-bound column the write path and interceptor enforce; and `TenantReceipt` is the typed lifecycle evidence. PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile — never a forked store family and never a new engine. The page spine is Npgsql, Thinktecture vocabulary, LanguageExt rails, and NodaTime; the deploy-side raw-SQL provisioning and GUC verification are the sibling `Store/provisioning` owner.

Wire posture: this page is host-local — every owner emits raw tenant-lifecycle SQL executed server-side. The tenancy primitive it consumes (`TenantContext`) crosses the wire only as `TenantContextWire` owned at `AppHost/runtime-ports#TS_PROJECTION`; this page reads `TenantId` as the `current_setting('rasm.tenant')::uuid` RLS predicate and never mints a client-facing projection.

## [01]-[INDEX]

- [01]-[TENANCY_RLS]: multi-tenancy axis, tenant lifecycle provisioning, RLS policy, and per-tenant quota.

## [02]-[TENANCY_RLS]

- Owner: `TenancyModel` — the multi-tenancy `[SmartEnum<string>]` axis under the `StoreKeyPolicy` ordinal comparer accessor; `TenantProvision` is the single per-model lifecycle owner, one `model.Switch` over the `TenantPhase` discriminant (`Apply`/`Destroy`/`Policy`) projecting create-DDL, teardown-DDL, and RLS-policy SQL from one fold; `TenantQuota` is the per-tenant resource-bound column the write path and interceptor enforce; `TenantReceipt` is the typed lifecycle evidence.
- Cases: `TenancyModel` single | schema | rls | db-per-tenant; `TenantPhase` Apply | Destroy | Policy; `TenantQuota` carries the connection-cap, statement-timeout, and write-byte-rate bounds per tenant.
- Entry: `public static Seq<string> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota)` is the one lifecycle fold: the `Apply` phase folds the per-model create DDL (schema/role/GUC steps keyed on the tenant slug and id and the `TenantQuota` bounds), the `Destroy` phase folds the idempotent teardown, and the `Policy` phase projects the RLS-policy SQL where only the `rls` row emits an `ENABLE ROW LEVEL SECURITY` + `CREATE POLICY` pair and every other row projects the empty sequence.
- Auto: the `rls` row emits `ALTER TABLE ... ENABLE ROW LEVEL SECURITY` plus a `CREATE POLICY` keyed on the tenant-id column the op-log carries so sync converges per tenant and content-address cache keys partition by tenant; the tenant id rides the `current_setting('rasm.tenant')::uuid` session GUC; `Project` derives every create step from the resolved `TenantContext` — the `schema` row creates the tenant schema and search-path role, the `rls` row seeds the policy and the per-tenant GUC defaults, the `db-per-tenant` row emits the `CREATE DATABASE` template clone, and each row folds the `TenantQuota` bounds as `ALTER ROLE ... CONNECTION LIMIT` and `ALTER ROLE ... SET statement_timeout` GUC rows so the quota is a deploy-time role constraint rather than a runtime branch; `Destroy` mirrors each create with its idempotent `DROP ... IF EXISTS` step; the per-tenant RLS-policy-applied fact, the per-tenant quota-set fact, and the pgaudit-category binding fact fold into the provisioning verifier.
- Receipt: `TenantReceipt(TenantId, Slug, TenancyModel Model, Seq<string> Applied, TenantQuota Quota, CorrelationId Correlation, Instant At)` is the lifecycle evidence; a per-tenant RLS-policy-applied `StoreFact` row, a `tenant.quota.applied` fact carrying the connection-cap and statement-timeout values, and the `AuditBinding.BindTenant` result fact fold into the open receipt's proof rows; a tenant-isolation mismatch or a quota-set failure is a typed provisioning fault, never silent.
- Packages: Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one `TenancyModel` row per new isolation strategy; one `TenantQuota` field per new resource bound; one `TenantPhase` case or one `TenantProvision` per-model fragment per new lifecycle concern; zero new surface.
- Boundary: `TenancyModel` is one axis row tying the host profile to an RLS policy, never a forked store family — PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile, never a new engine, and the package prohibition against admitting a new engine row holds; the tenant id arrives from the AppHost `TenantContext` threading owned at `AppHost/runtime-ports#PORT_RECORDS`, never minted here — `TenantProvision.Project` consumes the resolved `TenantContext.TenantId`/`Slug` as settled input across every `TenantPhase`, so the tenant-id resolution stays the AppHost concern and only the per-model DDL lifecycle is owned here; the audit-category consequence — a tenant-id-bearing classification binding to one pgaudit category — is owned at `Version/retention#AUDIT_BINDING`, and this cluster owns only the RLS-policy and lifecycle mechanics; `TenantQuota` is a column on the tenant-lifecycle fold, never a second rate-limiter — the runtime write-path enforcement reads the same quota bounds at `Query/rail#BULK_LANE` and the connection-cap enforcement rides the role `CONNECTION LIMIT` the provisioning emits, so the AppHost resource governor and this server-side bound are one declared set rather than parallel limiters; the `schema`/`db-per-tenant` rows project no `Policy`-phase SQL because their isolation is a connection-routing concern resolved by the host placement, not a row-level predicate, but they carry full `Apply`/`Destroy`-phase lifecycle DDL because schema and database creation are server-tier concerns; the `current_setting('rasm.tenant')` RLS binding and the `ClusterConfig.Preload` companion contract stay intact, owned at `Store/provisioning#CLUSTER_CONFIG`.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class TenancyModel {
    public static readonly TenancyModel Single = new("single");
    public static readonly TenancyModel Schema = new("schema");
    public static readonly TenancyModel Rls = new("rls");
    public static readonly TenancyModel DbPerTenant = new("db-per-tenant");
}

public sealed record TenantQuota(int ConnectionLimit, string StatementTimeout, long WriteBytesPerSecond);

public sealed record TenantReceipt(
    UInt128 TenantId, string Slug, TenancyModel Model, Seq<string> Applied,
    TenantQuota Quota, CorrelationId Correlation, Instant At);

[Union]
public abstract partial record TenantPhase {
    public sealed partial record Apply : TenantPhase;
    public sealed partial record Destroy : TenantPhase;
    public sealed partial record Policy(string Table, string TenantColumn) : TenantPhase;
}

public static class TenantProvision {
    public static Seq<string> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota) =>
        model.Switch(
            state: (Phase: phase, Tenant: tenant, Quota: quota),
            single: static s => s.Phase.Switch(
                apply: static () => Seq<string>(), destroy: static () => Seq<string>(), policy: static _ => Seq<string>()),
            schema: static s => s.Phase.Switch(
                apply: () => Seq(
                    $"CREATE SCHEMA IF NOT EXISTS tenant_{s.Tenant.Slug}",
                    $"CREATE ROLE tenant_{s.Tenant.Slug} WITH LOGIN CONNECTION LIMIT {s.Quota.ConnectionLimit}",
                    $"ALTER ROLE tenant_{s.Tenant.Slug} SET search_path = tenant_{s.Tenant.Slug}",
                    $"ALTER ROLE tenant_{s.Tenant.Slug} SET statement_timeout = '{s.Quota.StatementTimeout}'"),
                destroy: () => Seq(
                    $"DROP SCHEMA IF EXISTS tenant_{s.Tenant.Slug} CASCADE",
                    $"DROP ROLE IF EXISTS tenant_{s.Tenant.Slug}"),
                policy: static _ => Seq<string>()),
            rls: static s => s.Phase.Switch(
                apply: () => Seq(
                    $"CREATE ROLE tenant_{s.Tenant.Slug} WITH LOGIN CONNECTION LIMIT {s.Quota.ConnectionLimit}",
                    $"ALTER ROLE tenant_{s.Tenant.Slug} SET rasm.tenant = '{s.Tenant.TenantId}'",
                    $"ALTER ROLE tenant_{s.Tenant.Slug} SET statement_timeout = '{s.Quota.StatementTimeout}'"),
                destroy: () => Seq($"DROP ROLE IF EXISTS tenant_{s.Tenant.Slug}"),
                policy: static p => Seq(
                    $"ALTER TABLE {p.Table} ENABLE ROW LEVEL SECURITY",
                    $"CREATE POLICY {p.Table}_tenant ON {p.Table} USING ({p.TenantColumn} = current_setting('rasm.tenant')::uuid)")),
            dbPerTenant: static s => s.Phase.Switch(
                apply: () => Seq($"CREATE DATABASE tenant_{s.Tenant.Slug} TEMPLATE rasm_template CONNECTION LIMIT {s.Quota.ConnectionLimit}"),
                destroy: () => Seq($"DROP DATABASE IF EXISTS tenant_{s.Tenant.Slug} WITH (FORCE)"),
                policy: static _ => Seq<string>()));
}
```

## [03]-[RESEARCH]

- [RLS_POLICY_PROBE]: the live-PG18 `CREATE POLICY ... USING (tenant_id = current_setting('rasm.tenant')::uuid)` round-trip against a per-tenant `ALTER ROLE ... SET rasm.tenant` GUC default — whether the row-level-security predicate filters a foreign tenant's rows under the role-scoped session GUC, the `ENABLE ROW LEVEL SECURITY` + policy pair forces the predicate on every read, and a `FORCE ROW LEVEL SECURITY` is required for the table owner, proven against a live PG18 server before the `rls` lifecycle fence pins.
- [TENANT_QUOTA_APPLY]: the per-tenant `ALTER ROLE ... CONNECTION LIMIT` and `ALTER ROLE ... SET statement_timeout` deploy-time role-constraint application and the `db-per-tenant` `CREATE DATABASE ... TEMPLATE rasm_template` clone — whether the role connection cap and statement-timeout GUC bind as a server-side limit the runtime write path reads rather than a runtime branch, and whether the template clone carries the seeded schema, confirmed against the Forge-provisioned PG18 before the quota fence pins.
