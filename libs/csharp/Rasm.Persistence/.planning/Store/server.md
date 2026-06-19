# [PERSISTENCE_SERVER_TIER]

Rasm.Persistence owns the self-provisioned PostgreSQL 18.4 server tier as five raw-SQL provisioning surfaces distinct from the `Schema/ddl#EXTENSION_DDL` self-provisioned `CREATE EXTENSION` set and the `Store/profiles#PROVISIONING_ROWS` operator-verify manifest: `TimescaleProvisioning` folds a `TimescaleStep` union over hypertable, continuous-aggregate, retention, and columnstore DDL on the `OpLogEntry`-rollup table; `SearchProvisioning` lands `vectorscale` diskann and `pg_search` BM25 index DDL through an `IndexSpec` union into `MigrationBuilder.Sql`; `ClusterConfig` carries the deploy-time GUC fragments verified read-only against `pg_settings`; `TenancyModel` is the multi-tenancy and RLS axis tying the host profile to a `CREATE POLICY`, deepened by the `TenantProvision` tenant-lifecycle fold and the `TenantQuota` per-tenant resource-bound column; and `MigrationBundle` is the service-deploy gate over idempotent script output and the self-contained bundle artifact. The page spine is Npgsql, the Npgsql EF provider, Microsoft.EntityFrameworkCore.Design, Thinktecture vocabulary, LanguageExt rails, and NodaTime.

Wire posture: this page is host-local — every owner emits raw provisioning SQL executed server-side against the deploy-image PostgreSQL, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The tenancy primitive it consumes (`TenantContext`) crosses the wire only as `TenantContextWire` owned at `AppHost/runtime-ports#TS_PROJECTION`; this cluster reads `TenantId` as the `current_setting('rasm.tenant')::uuid` RLS predicate and never mints a client-facing projection.

## [1]-[INDEX]

- [1]-[TIMESCALE_PROVISIONING]: hypertable, continuous-aggregate, retention, and columnstore DDL.
- [2]-[SEARCH_PROVISIONING]: diskann and BM25 index DDL over the preload-gated companions.
- [3]-[CLUSTER_CONFIG]: deploy-time GUC fragments verified read-only against settings.
- [4]-[TENANCY_RLS]: multi-tenancy axis, tenant lifecycle provisioning, and per-tenant quota.
- [5]-[MIGRATION_BUNDLE]: idempotent script output and the self-contained deploy gate.

## [2]-[TIMESCALE_PROVISIONING]

- Owner: `TimescaleProvisioning` — the TimescaleDB raw-SQL provisioning fold over the `OpLogEntry`-rollup receipt table; one static surface folding the `[Union]` `TimescaleStep` build-step algebra into `MigrationBuilder.Sql` steps.
- Cases: `TimescaleStep.Hypertable` over the rollup table; `TimescaleStep.ContinuousAggregate` carrying the materialized view plus its bgworker refresh policy as one DDL pair; `TimescaleStep.RetentionPolicy`; `TimescaleStep.ColumnstorePolicy`.
- Entry: `public static MigrationBuilder Provision(MigrationBuilder migration, Seq<TimescaleStep> steps)` — folds each `TimescaleStep` through its `Ddl()` projection into the migration via `MigrationBuilder.Sql`; `TimescaleStep.Ddl` is the `[Union]` `.Switch` over the four provisioning cases, each a pure string projection.
- Auto: the provisioning rides `MigrationBuilder.Sql` since TimescaleDB carries no first-party EF translator; the time column is the HLC `Physical` instant on the rollup table; the rollup table mirrors the `OpLogEntry` columns the `DuckDBOpLogMap` projects on `Query/lanes#ANALYTICAL_LANE`; the continuous-aggregate refresh, retention, and columnstore cadence rides the TimescaleDB native bgworker policy jobs (`add_continuous_aggregate_policy`/`add_retention_policy`/`add_columnstore_policy`) so the AppHost schedule port never schedules them. `pg_cron` remains an admitted server-side scheduling companion for SQL maintenance or future persistence-owned jobs that truly require database-local cadence, never a duplicate of Timescale policy scheduling.
- Receipt: each provisioning step folds a `StoreFact`-shaped row into the open receipt's proof rows — continuous-aggregate refresh-lag, retention drop-count, and columnstore compression-ratio read from the TimescaleDB job-stats views; a provisioning failure is a typed provisioning fault.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one `TimescaleStep` union case per new TimescaleDB provisioning concern (hypertable, continuous-aggregate, retention, columnstore); zero new surface.
- Boundary: the preload-gated TimescaleDB companion never enters the `Schema/ddl#EXTENSION_DDL` `Extensions` set — it provisions here through `MigrationBuilder.Sql` after the preload, never as a self-provisioned `SchemaDdl.Extension` annotation; the standalone in-process DuckDB lane owns embedded and local analytical reads while these continuous aggregates own the server analytical rollups, so there is no pg_duckdb cross-engine seam and no standalone-DuckDB server role; the hypertable rollup feeds the analytical lane and the dashboard tiles on `Query/lanes#ANALYTICAL_LANE`, never a second telemetry store.

```csharp signature
[Union]
public abstract partial record TimescaleStep {
    public sealed partial record Hypertable(string Table, string TimeColumn, string Interval) : TimescaleStep;
    public sealed partial record ContinuousAggregate(
        string View, string Source, string Bucket, string TimeColumn,
        string StartOffset, string EndOffset, string Schedule) : TimescaleStep;
    public sealed partial record RetentionPolicy(string Table, string DropAfter) : TimescaleStep;
    public sealed partial record ColumnstorePolicy(string Table, string SegmentBy, string OrderBy, string After) : TimescaleStep;

    public string Ddl() =>
        Switch(
            hypertable: static h =>
                $"SELECT create_hypertable('{h.Table}', by_range('{h.TimeColumn}', INTERVAL '{h.Interval}'), if_not_exists => TRUE)",
            continuousAggregate: static c =>
                $"CREATE MATERIALIZED VIEW {c.View} WITH (timescaledb.continuous) AS SELECT time_bucket('{c.Bucket}', {c.TimeColumn}) AS bucket, count(*) AS n FROM {c.Source} GROUP BY 1 WITH NO DATA; " +
                $"SELECT add_continuous_aggregate_policy('{c.View}', start_offset => INTERVAL '{c.StartOffset}', end_offset => INTERVAL '{c.EndOffset}', schedule_interval => INTERVAL '{c.Schedule}')",
            retentionPolicy: static r =>
                $"SELECT add_retention_policy('{r.Table}', drop_after => INTERVAL '{r.DropAfter}')",
            columnstorePolicy: static c =>
                $"ALTER TABLE {c.Table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = '{c.SegmentBy}', timescaledb.orderby = '{c.OrderBy}'); " +
                $"SELECT add_columnstore_policy('{c.Table}', after => INTERVAL '{c.After}')");
}

public static class TimescaleProvisioning {
    public static MigrationBuilder Provision(MigrationBuilder migration, Seq<TimescaleStep> steps) =>
        steps.Fold(migration, static (builder, step) => { builder.Sql(step.Ddl()); return builder; });
}
```

## [3]-[SEARCH_PROVISIONING]

- Owner: `SearchProvisioning` — the `vectorscale` diskann and `pg_search` BM25 index-DDL fold; one static surface folding a `[Union]` `IndexSpec` (`DiskAnn | Bm25`) build-spec algebra into `MigrationBuilder.Sql` index-build steps. The query-projection vocabulary (`pdb.*`/`@@@`/`pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg`) lives on the `[Union]` `Bm25Predicate` axis and the `SearchProvisioning` projection methods consumed at `Query/lanes#SEARCH_LANES`.
- Cases: `IndexSpec.DiskAnn` over a `vector(N)` column carrying the full `vectorscale` build-options axis (`storage_layout`/`num_neighbors`/`search_list_size`/`max_alpha`/`num_dimensions`/`num_bits_per_dimension`) under one ops-class row; `IndexSpec.Bm25` over a `key_field`-anchored ordered text-column tuple. `Bm25Predicate` is the column-operator and `@@@`-builder projection axis (`|||`/`&&&`/`===`/`###` column operators; `pdb.parse`/`pdb.range_term`/`pdb.phrase_prefix`/`pdb.more_like_this`/`pdb.regex`/`pdb.all` builders; the `::pdb.fuzzy`/`::pdb.boost`/`::pdb.const`/`::pdb.slop` cast wrappers composing over any inner predicate).
- Entry: `public static Fin<MigrationBuilder> Provision(MigrationBuilder migration, Seq<IndexSpec> specs)` — traverses each `IndexSpec.Ddl()` projection and, on the `Succ` arm, folds the verified DDL into `MigrationBuilder.Sql`; a rejected `IndexSpec` (the `<#>`-on-`plain` build) surfaces as `Fin.Fail` to the deploy-gate caller before any `Sql()` lands. `IndexSpec.Ddl` is the `[Union]` `.Switch` over the build-spec cases, each a pure `Fin<string>` projection.
- Auto: both index builds ride `MigrationBuilder.Sql` since neither `vectorscale` nor `pg_search` carries a first-party EF translator; the diskann index reuses the pgvector distance operators (`<=>`/`<->`/`<#>`) so the column stays the `EmbeddingArity`-row `vector` store type and the planner routes a distance query through diskann transparently; the `DiskAnnOptions` sentinels resolve at build time from vector dimensionality — `NumNeighbors=-1` is dimension-derived degree, `NumDimensions=0` is all dimensions, and `NumBitsPerDimension=0` is automatic bit selection (build-time picks 2 below 900 dims, 1 at or above 900), while `Default` pins `NumNeighbors=50` and `NumBitsPerDimension=0` and never a fixed bit width; the BM25 index answers through `@@@` query-builder dispatch, the bare `|||`/`&&&`/`===`/`###` column operators, `pdb.score`, and `pdb.snippet` raw SQL beside the always-present native FTS baseline — the `paradedb.*` namespace is removed in `pg_search` 0.24.0 and is never emitted, the column-operator cases execute without `@@@`, the builder cases take a `pdb.*` function on the right of `@@@`, and the `Fuzzy`/`Boost`/`Const`/`Slop` cast cases wrap any inner predicate as a composable `::pdb.*` modifier so a new builder is one union case, never a sibling method; the `key_field` column is `UNIQUE`/primary-key and listed first under the one-BM25-index-per-table constraint; the preload-gated companions verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe` before these steps run.
- Receipt: each index build folds a `StoreFact`-shaped row into the open receipt's proof rows; the `search.vector.route` and `search.bm25.score` facts on `Query/lanes#SEARCH_LANES` read the live planner route the index serves.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one `IndexSpec` union case per new access method; one `DiskAnnOptions` field per new `vectorscale` build option; one `Bm25Predicate` union case per new `pdb.*` builder, column operator, or cast modifier; zero new surface.
- Boundary: `vectorscale` and `pg_search` are preload-gated companions, never `Schema/ddl#EXTENSION_DDL` self-provisioned `Extensions` rows — they provision here after the preload; the diskann opclasses (`vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`) and the `storage_layout=memory_optimized` SBQ option ride the index DDL, never a parallel vector column, and the `<#>` inner-product operator is rejected against a `storage_layout=plain` build; `pg_search` runs in-process inside the PG server, never linked into managed code, so the AGPL boundary is the DB deployment and a profile without `pg_search` preloaded answers through the native FTS baseline; the column-type and runtime query-value binding are owned at `Query/lanes#SEARCH_LANES` and consumed here as settled vocabulary — `Bm25Predicate` is the index-build-adjacent column-operator and `pdb.*`-builder projector this cluster owns, with its query values arriving pre-escaped from the search-lane binder, never raw runtime input.

```csharp signature
[SmartEnum<string>]
public sealed partial class DiskAnnOps {
    public static readonly DiskAnnOps Cosine = new("vector_cosine_ops", "<=>");
    public static readonly DiskAnnOps L2 = new("vector_l2_ops", "<->");
    public static readonly DiskAnnOps InnerProduct = new("vector_ip_ops", "<#>");
    public string Operator { get; }
    private DiskAnnOps(string key, string @operator) : this(key) => Operator = @operator;
}

[SmartEnum<string>]
public sealed partial class DiskAnnLayout {
    public static readonly DiskAnnLayout MemoryOptimized = new("memory_optimized");
    public static readonly DiskAnnLayout Plain = new("plain");
}

public sealed record DiskAnnOptions(
    DiskAnnLayout StorageLayout, int NumNeighbors, int SearchListSize,
    double MaxAlpha, int NumDimensions, int NumBitsPerDimension) {
    public static readonly DiskAnnOptions Default =
        new(DiskAnnLayout.MemoryOptimized, NumNeighbors: 50, SearchListSize: 100,
            MaxAlpha: 1.2, NumDimensions: 0, NumBitsPerDimension: 0);
}

[Union]
public abstract partial record IndexSpec {
    public sealed partial record DiskAnn(string Index, string Table, string Column, DiskAnnOps Ops, DiskAnnOptions Options) : IndexSpec;
    public sealed partial record Bm25(string Index, string Table, string KeyField, Seq<string> TextColumns) : IndexSpec;

    public Fin<string> Ddl() =>
        Switch<Fin<string>>(
            diskAnn: static d =>
                d is { Ops.Key: "vector_ip_ops", Options.StorageLayout.Key: "plain" }
                    ? Fin<string>.Fail(Error.New($"<diskann-ip-on-plain-layout:{d.Index}>"))
                    : Fin<string>.Succ(
                        $"CREATE INDEX CONCURRENTLY {d.Index} ON {d.Table} USING diskann ({d.Column} {d.Ops.Key}) " +
                        $"WITH (storage_layout = '{d.Options.StorageLayout.Key}', num_neighbors = {d.Options.NumNeighbors}, " +
                        $"search_list_size = {d.Options.SearchListSize}, max_alpha = {d.Options.MaxAlpha.ToString(CultureInfo.InvariantCulture)}, " +
                        $"num_dimensions = {d.Options.NumDimensions}, num_bits_per_dimension = {d.Options.NumBitsPerDimension})"),
            bm25: static b =>
                Fin<string>.Succ(
                    $"CREATE INDEX {b.Index} ON {b.Table} USING bm25 ({string.Join(", ", b.TextColumns.Prepend(b.KeyField))}) " +
                    $"WITH (key_field = '{b.KeyField}')"));
}

[Union]
public abstract partial record Bm25Predicate {
    public sealed partial record AnyToken(string Column, string Query) : Bm25Predicate;
    public sealed partial record AllToken(string Column, string Query) : Bm25Predicate;
    public sealed partial record ExactTerm(string Column, string Term) : Bm25Predicate;
    public sealed partial record Phrase(string Column, string PhraseText) : Bm25Predicate;
    public sealed partial record Parse(string Column, string QueryString, bool Lenient, bool ConjunctionMode) : Bm25Predicate;
    public sealed partial record RangeTerm(string Column, string Value, string Relation, Option<string> RangeType) : Bm25Predicate;
    public sealed partial record PhrasePrefix(string Column, Seq<string> Tokens, int MaxExpansions) : Bm25Predicate;
    public sealed partial record MoreLikeThis(string Column, string DocumentId, Seq<string> Fields, int MaxQueryTerms) : Bm25Predicate;
    public sealed partial record Regex(string Column, string Pattern) : Bm25Predicate;
    public sealed partial record All(string Column) : Bm25Predicate;
    public sealed partial record Fuzzy(Bm25Predicate Inner, int Distance, bool Prefix, bool TranspositionCostOne) : Bm25Predicate;
    public sealed partial record Boost(Bm25Predicate Inner, double Factor) : Bm25Predicate;
    public sealed partial record Const(Bm25Predicate Inner, double Score) : Bm25Predicate;
    public sealed partial record Slop(Bm25Predicate Inner, int Distance) : Bm25Predicate;

    public string Sql() =>
        Switch(
            anyToken: static a => $"{a.Column} ||| '{a.Query}'",
            allToken: static a => $"{a.Column} &&& '{a.Query}'",
            exactTerm: static e => $"{e.Column} === '{e.Term}'",
            phrase: static p => $"{p.Column} ### '{p.PhraseText}'",
            parse: static p =>
                $"{p.Column} @@@ pdb.parse('{p.QueryString}', lenient => {p.Lenient.ToString().ToLowerInvariant()}, conjunction_mode => {p.ConjunctionMode.ToString().ToLowerInvariant()})",
            rangeTerm: static r =>
                $"{r.Column} @@@ pdb.range_term('{r.Value}', relation => '{r.Relation}'{r.RangeType.Match(Some: static t => $", range_type => '{t}'", None: static () => string.Empty)})",
            phrasePrefix: static p =>
                $"{p.Column} @@@ pdb.phrase_prefix(ARRAY[{string.Join(", ", p.Tokens.Map(static t => $"'{t}'"))}], max_expansions => {p.MaxExpansions})",
            moreLikeThis: static m =>
                $"{m.Column} @@@ pdb.more_like_this('{m.DocumentId}', fields => ARRAY[{string.Join(", ", m.Fields.Map(static f => $"'{f}'"))}], max_query_terms => {m.MaxQueryTerms})",
            regex: static r => $"{r.Column} @@@ pdb.regex('{r.Pattern}')",
            all: static a => $"{a.Column} @@@ pdb.all()",
            fuzzy: static f => $"{f.Inner.Sql()}::pdb.fuzzy({f.Distance}, {f.Prefix.ToString().ToLowerInvariant()}, {f.TranspositionCostOne.ToString().ToLowerInvariant()})",
            boost: static b => $"{b.Inner.Sql()}::pdb.boost({b.Factor.ToString(CultureInfo.InvariantCulture)})",
            @const: static c => $"{c.Inner.Sql()}::pdb.const({c.Score.ToString(CultureInfo.InvariantCulture)})",
            slop: static s => $"{s.Inner.Sql()}::pdb.slop({s.Distance})");
}

public static class SearchProvisioning {
    public static Fin<MigrationBuilder> Provision(MigrationBuilder migration, Seq<IndexSpec> specs) =>
        specs.Map(static spec => spec.Ddl())
            .Traverse(identity).As()
            .Map(steps => steps.Fold(migration, static (builder, step) => { builder.Sql(step); return builder; }));

    public static string Score(string keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(string column, string startTag, string endTag, int maxChars) =>
        $"pdb.snippet({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars})";
    public static string Snippets(string column, int maxChars, int limit, int offset, string sortBy, string startTag, string endTag) =>
        $"pdb.snippets({column}, max_num_chars => {maxChars}, \"limit\" => {limit}, \"offset\" => {offset}, sort_by => '{sortBy}', start_tag => '{startTag}', end_tag => '{endTag}')";
    public static string SnippetPositions(string column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{esJson}') OVER ()";
}
```

## [4]-[CLUSTER_CONFIG]

- Owner: `ClusterConfig` — the PG18 deploy-time GUC fragment table and its read-only verification probe; each row is a `(setting, value, fallback)` triple.
- Cases: io_method with the io_uring value and the worker portable fallback; effective_io_concurrency; maintenance_io_concurrency; data_checksums; the `Preload` shared_preload_libraries row carrying the bgworker-preload companion set.
- Entry: `public static Fin<FrozenDictionary<string, string>> Verify(Seq<(string Setting, string Value, string Fallback)> rows, FrozenDictionary<string, string> observed)` — `Fin` aborts when an observed GUC is neither the row's value nor its declared fallback.
- Auto: the cluster GUC rows are deploy-time `postgresql.conf` fragments verified read-only against `pg_settings`, never executed at runtime; the io_method row carries `io_uring` as the Linux-guest value and `worker` as the portable fallback so a deploy image whose kernel lacks io_uring satisfies the verify with `worker`; the data-checksums-by-default and effective/maintenance io-concurrency rows feed the analytical-lane read-throughput rationale; the `Preload` row's `shared_preload_libraries` value (`timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron`) is the unified deploy-image's bgworker-preload contract verified read-only after boot — `timescaledb`/`pg_search`/`pg_cron`/`pg_partman_bgw`/`pg_squeeze`/`pgaudit` each require preload, while `vectorscale`/`pg_jsonschema`/`pgvector` load through their index AM or type registration and are correctly absent from the preload row.
- Packages: Npgsql, LanguageExt.Core, BCL inbox
- Growth: one `(setting, value, fallback)` triple per new GUC fragment; zero new surface.
- Boundary: this cluster verifies, never executes — runtime `ALTER SYSTEM` is the rejected form, the GUC fragments land as physical `postgresql.conf` assets at the first headless or web app root; the verify admits a setting at either its value or its declared fallback so the portability split (io_uring on a Linux guest, worker elsewhere) is a row triple rather than a pinned literal; OAuth `pg_hba` posture and role grants are deploy-time `pg_hba`/grant assets verified through the same read-only probe, never executed; `SetPostgresVersion(18, 0)` is the provider feature-gate floor owned at `Store/profiles#PROFILE_AXIS`, distinct from the PG18.4 deployment minimum these fragments target. Image composition, Docker/Compose mechanics, and native build/export stay Forge-owned; this page names the required GUC semantics and consumes Assay `ProvisionRun` observations before pinning a row, never a Dockerfile or image-build recipe. The `io_method` observed value is read from `pg_settings` as a deploy-runtime observation, not a capability gate.

```csharp signature
public static class ClusterConfig {
    public static readonly Seq<(string Setting, string Value, string Fallback)> Rows = [
        ("io_method", "io_uring", "worker"),
        ("effective_io_concurrency", "16", "16"),
        ("maintenance_io_concurrency", "16", "16"),
        ("data_checksums", "on", "on"),
        ("shared_preload_libraries", "timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron", "timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron"),
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

- Owner: `TenancyModel` — the multi-tenancy `[SmartEnum<string>]` axis under the `StoreKeyPolicy` ordinal comparer accessor; `TenantProvision` is the single per-model lifecycle owner, one `model.Switch` over the `TenantPhase` discriminant (`Apply`/`Destroy`/`Policy`) projecting create-DDL, teardown-DDL, and RLS-policy SQL from one fold; `TenantQuota` is the per-tenant resource-bound column the write path and interceptor enforce; `TenantReceipt` is the typed lifecycle evidence.
- Cases: `TenancyModel` single | schema | rls | db-per-tenant; `TenantPhase` Apply | Destroy | Policy; `TenantQuota` carries the connection-cap, statement-timeout, and write-byte-rate bounds per tenant.
- Entry: `public static Seq<string> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota)` is the one lifecycle fold: the `Apply` phase folds the per-model create DDL (schema/role/GUC steps keyed on the tenant slug and id and the `TenantQuota` bounds), the `Destroy` phase folds the idempotent teardown, and the `Policy` phase projects the RLS-policy SQL where only the `rls` row emits an `ENABLE ROW LEVEL SECURITY` + `CREATE POLICY` pair and every other row projects the empty sequence.
- Auto: the `rls` row emits `ALTER TABLE ... ENABLE ROW LEVEL SECURITY` plus a `CREATE POLICY` keyed on the tenant-id column the op-log carries so sync converges per tenant and content-address cache keys partition by tenant; the tenant id rides the `current_setting('rasm.tenant')::uuid` session GUC; `Provision` derives every create step from the resolved `TenantContext` — the `schema` row creates the tenant schema and search-path role, the `rls` row seeds the policy and the per-tenant GUC defaults, the `db-per-tenant` row emits the `CREATE DATABASE` template clone, and each row folds the `TenantQuota` bounds as `ALTER ROLE ... CONNECTION LIMIT` and `ALTER ROLE ... SET statement_timeout` GUC rows so the quota is a deploy-time role constraint rather than a runtime branch; `Destroy` mirrors each create with its idempotent `DROP ... IF EXISTS` step; the per-tenant RLS-policy-applied fact, the per-tenant quota-set fact, and the pgaudit-category binding fact fold into the provisioning verifier.
- Receipt: `TenantReceipt(TenantId, Slug, TenancyModel Model, Seq<string> Applied, TenantQuota Quota, CorrelationId Correlation, Instant At)` is the lifecycle evidence; a per-tenant RLS-policy-applied `StoreFact` row, a `tenant.quota.applied` fact carrying the connection-cap and statement-timeout values, and the `AuditBinding.BindTenant` result fact fold into the open receipt's proof rows; a tenant-isolation mismatch or a quota-set failure is a typed provisioning fault, never silent.
- Packages: Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one `TenancyModel` row per new isolation strategy; one `TenantQuota` field per new resource bound; one `TenantPhase` case or one `TenantProvision` per-model fragment per new lifecycle concern; zero new surface.
- Boundary: `TenancyModel` is one axis row tying the host profile to an RLS policy, never a forked store family — PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile, never a new engine, and the package prohibition against admitting a new engine row holds; the tenant id arrives from the AppHost `TenantContext` threading owned at `AppHost/runtime-ports#PORT_RECORDS`, never minted here — `TenantProvision.Project` consumes the resolved `TenantContext.TenantId`/`Slug` as settled input across every `TenantPhase`, so the tenant-id resolution stays the AppHost concern and only the per-model DDL lifecycle is owned here; the audit-category consequence — a tenant-id-bearing classification binding to one pgaudit category — is owned at `Version/retention#AUDIT_BINDING`, and this cluster owns only the RLS-policy and lifecycle mechanics; `TenantQuota` is a column on the tenant-lifecycle fold, never a second rate-limiter — the runtime write-path enforcement reads the same quota bounds at `Query/rail#BULK_LANE` and the connection-cap enforcement rides the role `CONNECTION LIMIT` the provisioning emits, so the AppHost resource governor and this server-side bound are one declared set rather than parallel limiters; the `schema`/`db-per-tenant` rows project no `Policy`-phase SQL because their isolation is a connection-routing concern resolved by the host placement, not a row-level predicate, but they carry full `Apply`/`Destroy`-phase lifecycle DDL because schema and database creation are server-tier concerns.

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

- [SERVER_PROVISIONING_PROBE]: the live-PG18 round-trip from Assay `ProvisionRun` evidence — the `create_hypertable`/`add_continuous_aggregate_policy`/`add_columnstore_policy` apply contract, the diskann-over-`vector` `storage_layout=memory_optimized` plus the `num_neighbors`/`search_list_size`/`num_bits_per_dimension` build-option apply, the `pg_search` 0.24.0 BM25 `@@@ pdb.parse`/`|||`/`pdb.score`/`pdb.snippet` query shape (the removed `paradedb.*` namespace asserted absent), the `pg_cron` server-side scheduler row, and the RLS `current_setting('rasm.tenant')` per-tenant isolation, proven against the installed extensions before the provisioning DDL pins.
- [CLUSTER_CONFIG_PORTABILITY]: the `io_method=io_uring` GUC against the Forge-provisioned local PG18 runtime versus the `worker` portable fallback — whether the runtime kernel exposes io_uring, confirmed against the `pg_settings` observed value before the io-method triple's primary value is preferred over its fallback at deploy.
- [MIGRATION_BUNDLE_PROBE]: the `dotnet ef migrations bundle` self-contained executable and the idempotent `ScriptMigration` output applied against a live PG18 server — the `GetMigrations`/`HasPendingModelChanges` deploy-gate boundary and the NOT-VALID/NOT-ENFORCED lock-light constraint apply-then-validate contract, proven before the deploy-gate fence pins.
