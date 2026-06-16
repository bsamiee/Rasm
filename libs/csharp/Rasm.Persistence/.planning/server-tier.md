# [PERSISTENCE_SERVER_TIER]

Rasm.Persistence owns the self-provisioned PostgreSQL 18.4 server tier as five raw-SQL provisioning surfaces distinct from the `schema-rail#EXTENSION_DDL` self-provisioned `CREATE EXTENSION` set and the `store-profiles#PROVISIONING_ROWS` operator-verify manifest: `TimescaleProvisioning` folds hypertable, continuous-aggregate, retention, and columnstore DDL over the `OpLogEntry`-rollup table; `SearchProvisioning` lands pgvectorscale diskann and pg_search BM25 index DDL through `MigrationBuilder.Sql`; `ClusterConfig` carries the deploy-time GUC fragments verified read-only against `pg_settings`; `TenancyModel` is the multi-tenancy and RLS axis tying the host profile to a `CREATE POLICY`; and `MigrationBundle` is the service-deploy gate over idempotent script output and the self-contained bundle artifact. The page spine is Npgsql, the Npgsql EF provider, Microsoft.EntityFrameworkCore.Design, Thinktecture vocabulary, LanguageExt rails, and NodaTime.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]               | [OWNS]                                                          |
| :-----: | :---------------------- | :------------------------------------------------------------- |
|   [1]   | TIMESCALE_PROVISIONING  | Hypertable, continuous-aggregate, retention, columnstore DDL   |
|   [2]   | SEARCH_PROVISIONING     | diskann and BM25 index DDL over the preload-gated companions   |
|   [3]   | CLUSTER_CONFIG          | Deploy-time GUC fragments verified read-only against settings  |
|   [4]   | TENANCY_RLS             | Multi-tenancy axis tying the host profile to an RLS policy     |
|   [5]   | MIGRATION_BUNDLE        | Idempotent script output and the self-contained deploy gate    |

## [2]-[TIMESCALE_PROVISIONING]

- Owner: `TimescaleProvisioning` — the TimescaleDB raw-SQL provisioning fold over the `OpLogEntry`-rollup receipt table; one static surface emitting `MigrationBuilder.Sql` steps.
- Cases: a hypertable row over the rollup table; a continuous-aggregate materialized view plus its refresh policy; a retention policy; a columnstore policy.
- Entry: `public static MigrationBuilder Provision(MigrationBuilder migration, Seq<string> steps)` — folds the non-empty provisioning DDL steps into the migration through `MigrationBuilder.Sql`; each emitter is a pure string projection.
- Auto: the provisioning rides `MigrationBuilder.Sql` since TimescaleDB carries no first-party EF translator; the time column is the HLC `Physical` instant on the rollup table; the rollup table mirrors the `OpLogEntry` columns the `DuckDBOpLogMap` projects on `data-lanes#ANALYTICAL_LANE`; the continuous-aggregate refresh, retention, and columnstore cadence rides the TimescaleDB native bgworker policy jobs (`add_continuous_aggregate_policy`/`add_retention_policy`/`add_columnstore_policy`) so the AppHost schedule port never schedules them and pg_cron stays rejected.
- Receipt: each provisioning step folds a `StoreFact`-shaped row into the open receipt's proof rows — continuous-aggregate refresh-lag, retention drop-count, and columnstore compression-ratio read from the TimescaleDB job-stats views; a provisioning failure is a typed provisioning fault.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one hypertable row, one continuous-aggregate row, one retention row, or one columnstore row; zero new surface.
- Boundary: the preload-gated TimescaleDB companion never enters the `schema-rail#EXTENSION_DDL` `Extensions` set — it provisions here through `MigrationBuilder.Sql` after the preload, never as a self-provisioned `SchemaDdl.Extension` annotation; the standalone in-process DuckDB lane owns embedded and local analytical reads while these continuous aggregates own the server analytical rollups, so there is no pg_duckdb cross-engine seam and no standalone-DuckDB server role; the hypertable rollup feeds the analytical lane and the dashboard tiles on `data-lanes#ANALYTICAL_LANE`, never a second telemetry store.

```csharp signature
public static class TimescaleProvisioning {
    public static string Hypertable(string table, string timeColumn, string interval) =>
        $"SELECT create_hypertable('{table}', by_range('{timeColumn}', INTERVAL '{interval}'), if_not_exists => TRUE)";

    public static string ContinuousAggregate(string view, string source, string bucket, string timeColumn) =>
        $"CREATE MATERIALIZED VIEW {view} WITH (timescaledb.continuous) AS SELECT time_bucket('{bucket}', {timeColumn}) AS bucket, count(*) AS n FROM {source} GROUP BY 1 WITH NO DATA";

    public static string ContinuousAggregatePolicy(string view, string startOffset, string endOffset, string schedule) =>
        $"SELECT add_continuous_aggregate_policy('{view}', start_offset => INTERVAL '{startOffset}', end_offset => INTERVAL '{endOffset}', schedule_interval => INTERVAL '{schedule}')";

    public static string RetentionPolicy(string table, string dropAfter) =>
        $"SELECT add_retention_policy('{table}', drop_after => INTERVAL '{dropAfter}')";

    public static string ColumnstorePolicy(string table, string segmentBy, string orderBy, string after) =>
        $"ALTER TABLE {table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = '{segmentBy}', timescaledb.orderby = '{orderBy}'); SELECT add_columnstore_policy('{table}', after => INTERVAL '{after}')";

    public static MigrationBuilder Provision(MigrationBuilder migration, Seq<string> steps) =>
        steps.Filter(static step => step.Length > 0).Fold(migration, static (builder, step) => { builder.Sql(step); return builder; });
}
```

## [3]-[SEARCH_PROVISIONING]

- Owner: `SearchProvisioning` — the pgvectorscale diskann and pg_search BM25 index-DDL fold; one static surface emitting `MigrationBuilder.Sql` index-build steps.
- Cases: a diskann index over a `vector(N)`/`halfvec(N)` column; a pg_search BM25 index over one or more text columns with its `key_field`.
- Entry: `public static MigrationBuilder Provision(MigrationBuilder migration, Seq<string> steps)` — folds the index-DDL steps into the migration through `MigrationBuilder.Sql`; `DiskAnnIndex` and `Bm25Index` are pure string projections.
- Auto: both index builds ride `MigrationBuilder.Sql` since neither pgvectorscale nor pg_search has a first-party EF translator; the diskann index reuses the pgvector distance functions so the column stays the `EmbeddingArity`-row store type and the planner routes a distance query through diskann transparently; the BM25 index answers through `@@@`/`paradedb.score`/`paradedb.snippet` raw SQL beside the always-present native FTS baseline; the preload-gated companions verify through the `store-profiles#PROVISIONING_ROWS` `PreloadProbe` before these steps run.
- Receipt: each index build folds a `StoreFact`-shaped row into the open receipt's proof rows; the `search.vector.route` and `search.bm25.score` facts on `data-lanes#SEARCH_LANES` read the live planner route the index serves.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one diskann index row or one BM25 index row; zero new surface.
- Boundary: pgvectorscale and pg_search are preload-gated companions, never `schema-rail#EXTENSION_DDL` self-provisioned `Extensions` rows — they provision here after the preload; the diskann opclasses (`vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`) and the `storage_layout=memory_optimized` SBQ option ride the index DDL, never a parallel vector column; pg_search runs in-process inside the PG server, never linked into managed code, so the AGPL boundary is the DB deployment and a profile without pg_search preloaded answers through the native FTS baseline; the column-type and query mechanics are owned at `data-lanes#SEARCH_LANES` and consumed here as settled vocabulary, this cluster owns only the index-build DDL.

```csharp signature
public static class SearchProvisioning {
    public static string DiskAnnIndex(string table, string column, string opclass) =>
        $"CREATE INDEX CONCURRENTLY ON {table} USING diskann ({column} {opclass}) WITH (storage_layout = 'memory_optimized')";

    public static string Bm25Index(string index, string table, Seq<string> columns, string keyField) =>
        $"CREATE INDEX {index} ON {table} USING bm25 ({string.Join(", ", columns)}) WITH (key_field = '{keyField}')";

    public static MigrationBuilder Provision(MigrationBuilder migration, Seq<string> steps) =>
        steps.Filter(static step => step.Length > 0).Fold(migration, static (builder, step) => { builder.Sql(step); return builder; });
}
```

## [4]-[CLUSTER_CONFIG]

- Owner: `ClusterConfig` — the PG18 deploy-time GUC fragment table and its read-only verification probe; each row is a `(setting, value, fallback)` triple.
- Cases: io_method with the io_uring value and the worker portable fallback; effective_io_concurrency; maintenance_io_concurrency; data_checksums.
- Entry: `public static Fin<FrozenDictionary<string, string>> Verify(Seq<(string Setting, string Value, string Fallback)> rows, FrozenDictionary<string, string> observed)` — `Fin` aborts when an observed GUC is neither the row's value nor its declared fallback.
- Auto: the cluster GUC rows are deploy-time `postgresql.conf` fragments verified read-only against `pg_settings`, never executed at runtime; the io_method row carries `io_uring` as the Linux-guest value and `worker` as the portable fallback so a deploy image whose kernel lacks io_uring satisfies the verify with `worker`; the data-checksums-by-default and effective/maintenance io-concurrency rows feed the analytical-lane read-throughput rationale.
- Packages: Npgsql, LanguageExt.Core, BCL inbox
- Growth: one `(setting, value, fallback)` triple per new GUC fragment; zero new surface.
- Boundary: this cluster verifies, never executes — runtime `ALTER SYSTEM` is the rejected form, the GUC fragments land as physical `postgresql.conf` assets at the first headless or web app root; the verify admits a setting at either its value or its declared fallback so the portability split (io_uring on a Linux guest, worker elsewhere) is a row triple rather than a pinned literal; OAuth `pg_hba` posture and role grants are deploy-time `pg_hba`/grant assets verified through the same read-only probe, never executed; `SetPostgresVersion(18, 0)` is the provider feature-gate floor owned at `store-profiles#PROFILE_AXIS`, distinct from the PG18.4 deploy-image minimum these fragments target.

```csharp signature
public static class ClusterConfig {
    public static readonly Seq<(string Setting, string Value, string Fallback)> Rows = [
        ("io_method", "io_uring", "worker"),
        ("effective_io_concurrency", "16", "16"),
        ("maintenance_io_concurrency", "16", "16"),
        ("data_checksums", "on", "on"),
    ];

    public const string SettingsProbe = "SELECT name, setting FROM pg_settings WHERE name = ANY($names)";

    public static Fin<FrozenDictionary<string, string>> Verify(Seq<(string Setting, string Value, string Fallback)> rows, FrozenDictionary<string, string> observed) =>
        rows.Filter(row => observed.TryGetValue(row.Setting, out var live)
            ? live != row.Value && live != row.Fallback
            : true) is { IsEmpty: false } missing
            ? Fin.Fail<FrozenDictionary<string, string>>(Error.New($"<cluster-config-mismatch:{string.Join(',', missing.Map(static row => row.Setting))}>"))
            : Fin.Succ(observed);
}
```

## [5]-[TENANCY_RLS]

- Owner: `TenancyModel` — the multi-tenancy `[SmartEnum<string>]` axis under the `StoreKeyPolicy` ordinal accessor; `RlsPolicy` is the per-tenant `CREATE POLICY` projection.
- Cases: single | schema | rls | db-per-tenant.
- Entry: `public static string Policy(TenancyModel model, string table, string tenantColumn)` — projects the RLS-policy SQL; only the `rls` row emits an `ENABLE ROW LEVEL SECURITY` + `CREATE POLICY` pair, every other row projects the empty string.
- Auto: the `rls` row emits `ALTER TABLE ... ENABLE ROW LEVEL SECURITY` plus a `CREATE POLICY` keyed on the tenant-id column the op-log carries so sync converges per tenant and content-address cache keys partition by tenant; the tenant id rides the `current_setting('rasm.tenant')::uuid` session GUC; the per-tenant RLS-policy-applied fact plus the pgaudit-category binding fact fold into the provisioning verifier.
- Receipt: a per-tenant RLS-policy-applied `StoreFact` row plus the `AuditBinding.BindTenant` result fact fold into the open receipt's proof rows; a tenant-isolation mismatch is a typed provisioning fault, never silent.
- Packages: Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one `TenancyModel` row per new isolation strategy; zero new surface.
- Boundary: `TenancyModel` is one axis row tying the host profile to an RLS policy, never a forked store family — PostgreSQL with RLS is the same `store-profiles#PROFILE_AXIS` `PostgresServer` profile, never a new engine, and the charter prohibition against admitting a new engine row holds; the tenant id arrives from the AppHost `TenantContext` threading owned at `AppHost/runtime-ports#PORT_RECORDS`, never minted here; the audit-category consequence — a tenant-id-bearing classification binding to one pgaudit category — is owned at `redaction-retention#AUDIT_BINDING`, and this cluster owns only the RLS-policy mechanics; the `schema`/`db-per-tenant` rows carry no policy SQL because their isolation is a connection-routing concern resolved by the host placement, not a row-level predicate.

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

public static class RlsPolicy {
    public static string Policy(TenancyModel model, string table, string tenantColumn) =>
        model.Switch(
            state: (Table: table, Tenant: tenantColumn),
            single: static _ => string.Empty,
            schema: static _ => string.Empty,
            rls: static s => $"ALTER TABLE {s.Table} ENABLE ROW LEVEL SECURITY; CREATE POLICY {s.Table}_tenant ON {s.Table} USING ({s.Tenant} = current_setting('rasm.tenant')::uuid)",
            dbPerTenant: static _ => string.Empty);
}
```

## [6]-[MIGRATION_BUNDLE]

- Owner: `MigrationBundle` — the service-deploy gate over idempotent `ScriptMigration` output, the self-contained `MigrationsBundle` executable artifact, and the `GetMigrations`/`HasPendingModelChanges` deploy gate.
- Cases: a script-migration deploy applying idempotent output; a bundle-artifact deploy running the self-contained executable; a pending-change gate aborting a deploy with un-applied model changes.
- Entry: `public static Fin<MigrationPlan> Plan(Seq<string> applied, Seq<string> known, bool pendingModelChanges)` — `Fin` aborts when the model carries pending changes the bundle does not contain or when an applied migration is unknown to the binary.
- Auto: a headless or web service applies migrations at deploy through the idempotent `ScriptMigration` output or the self-contained `MigrationsBundle` executable so no design tooling runs at deploy; the deploy gate folds the applied and known identifier sets and the pending-change flag against the placement's migrate authority; expand precedes contract so a destructive step rides the `redaction-retention` `DestructiveUnapproved` retention-approval gate before it applies.
- Packages: Microsoft.EntityFrameworkCore.Design, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one deploy modality row or one lock-light constraint pattern; zero new surface.
- Boundary: the migration-lock outcome reads from migration receipts, never from EF `Internal`-namespace types; the NOT-VALID/NOT-ENFORCED lock-light constraint pattern lets a constraint add without a full-table lock and validates in a later pass; a store newer than the binary (an applied migration the binary does not know) is a typed rejection, never a runtime branch; the destructive-step approval is owned at `redaction-retention` and consumed here as the settled gate, never a second approval taxonomy.

```csharp signature
public sealed record MigrationPlan(Seq<string> Apply, bool ExpandBeforeContract, bool DestructiveApprovalRequired);

public static class MigrationBundle {
    public static Fin<MigrationPlan> Plan(Seq<string> applied, Seq<string> known, bool pendingModelChanges) =>
        applied.Filter(id => !known.Contains(id)) is { IsEmpty: false } unknown
            ? Fin.Fail<MigrationPlan>(Error.New($"<store-newer-than-binary:{string.Join(',', unknown)}>"))
            : pendingModelChanges
                ? Fin.Fail<MigrationPlan>(Error.New("<pending-model-changes-absent-from-bundle>"))
                : Fin.Succ(new MigrationPlan(known.Filter(id => !applied.Contains(id)), ExpandBeforeContract: true, DestructiveApprovalRequired: true));
}
```

## [7]-[RESEARCH]

- [SERVER_PROVISIONING_PROBE]: the live-PG18 round-trip against the colima timescaledb/paradedb image — the `create_hypertable`/`add_continuous_aggregate_policy`/`add_columnstore_policy` apply contract, the diskann-over-`halfvec` `storage_layout=memory_optimized` storage-layout support, the pg_search BM25 `@@@`/`paradedb.score` query shape, and the RLS `current_setting('rasm.tenant')` per-tenant isolation, proven against the installed extensions before the provisioning DDL pins.
- [CLUSTER_CONFIG_PORTABILITY]: the `io_method=io_uring` GUC against the colima Linux guest versus the `worker` portable fallback — whether the deploy image's kernel exposes io_uring, confirmed against the `pg_settings` observed value before the io-method triple's primary value is preferred over its fallback at deploy.
- [MIGRATION_BUNDLE_PROBE]: the `dotnet ef migrations bundle` self-contained executable and the idempotent `ScriptMigration` output applied against a live PG18 server — the `GetMigrations`/`HasPendingModelChanges` deploy-gate boundary and the NOT-VALID/NOT-ENFORCED lock-light constraint apply-then-validate contract, proven before the deploy-gate fence pins.
