# [PERSISTENCE_SERVER_PROVISIONING]

Rasm.Persistence provisions the self-provisioned PostgreSQL 18.4 server tier through one deploy-side raw-SQL fold, one read-only cluster-GUC verification probe, and one service-deploy migration gate. The TimescaleDB hypertable/continuous-aggregate/retention/columnstore steps and the `vectorscale` diskann / `pg_search` bm25 index builds are arms on the one `Schema/ddl#EXTENSION_DDL` `SchemaDdl` union, folded into `MigrationBuilder.Sql` by the single `SchemaDdl.Sql` traverse — there is no second `Provision` static surface and no parallel step union. `ClusterConfig` carries the deploy-time GUC fragments verified read-only against `pg_settings`, and `MigrationBundle` is the service-deploy gate over idempotent script output and the self-contained bundle artifact. The page spine is Npgsql, the Npgsql EF provider, Microsoft.EntityFrameworkCore.Design, Thinktecture vocabulary, LanguageExt rails, and NodaTime; the multi-tenancy/RLS axis is the sibling `Store/tenancy` owner.

Wire posture: this page is host-local — every owner emits raw provisioning SQL executed server-side against the deploy-image PostgreSQL, crossing no browser or peer wire, so it carries no `TS_PROJECTION` cluster. The preload-gated companions verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe` before any provisioning step runs.

## [01]-[INDEX]

- [01]-[SCHEMA_DDL_FOLD]: the one server-tier raw-SQL provisioning fold and the bm25 query-projection axis.
- [02]-[CLUSTER_CONFIG]: deploy-time GUC fragments verified read-only against settings.
- [03]-[MIGRATION_BUNDLE]: idempotent script output and the self-contained deploy gate.

## [02]-[SCHEMA_DDL_FOLD]

- Owner: `SchemaDdl.Sql` (at `Schema/ddl#EXTENSION_DDL`) is the one server-tier raw-SQL provisioning fold — it traverses the `Hypertable`/`ContinuousAggregate`/`RetentionPolicy`/`ColumnstorePolicy`/`DiskAnn`/`Bm25` provisioning cases through their `ProvisionSql` `Fin<string>` projection and folds the verified DDL into `MigrationBuilder.Sql`; `Bm25Predicate` is the column-operator and `@@@`-builder query-projection axis consumed at `Query/lanes#SEARCH_LANES`, and the `SearchProjection` static surface owns the `pdb.score`/`pdb.snippet`/`pdb.snippets`/`pdb.snippet_positions`/`pdb.agg` projection methods.
- Cases: the four TimescaleDB provisioning cases fold onto the `OpLogEntry`-rollup table; the `DiskAnn` case carries the full `DiskAnnOptions` build axis (`storage_layout`/`num_neighbors`/`search_list_size`/`max_alpha`/`num_dimensions`/`num_bits_per_dimension`) under one ops-class row; the `Bm25` case carries a `key_field`-anchored ordered text-column tuple. `Bm25Predicate` is the column-operator and `@@@`-builder projection axis (`|||`/`&&&`/`===`/`###` column operators; `pdb.parse`/`pdb.range_term`/`pdb.phrase_prefix`/`pdb.more_like_this`/`pdb.regex`/`pdb.all` builders; the `::pdb.fuzzy`/`::pdb.boost`/`::pdb.const`/`::pdb.slop` cast wrappers composing over any inner predicate).
- Entry: `public static Fin<MigrationBuilder> SchemaDdl.Sql(MigrationBuilder migration, Seq<SchemaDdl> steps)` — folds the provisioning steps into `MigrationBuilder.Sql`, surfacing a rejected `DiskAnn` (the `<#>`-on-`plain` build) as `Fin.Fail` to the deploy-gate caller before any `Sql()` lands; `Bm25Predicate.Sql()` is the `[Union]` `.Switch` over the column-operator and builder cases, each a pure string projection.
- Auto: every provisioning step rides `MigrationBuilder.Sql` since neither TimescaleDB nor `vectorscale`/`pg_search` carries a first-party EF translator; the time column is the HLC `Physical` instant on the rollup table; the rollup table mirrors the `OpLogEntry` columns the `DuckDBOpLogMap` projects on `Query/lanes#ANALYTICAL_LANE`; the continuous-aggregate refresh, retention, and columnstore cadence rides the TimescaleDB native bgworker policy jobs (`add_continuous_aggregate_policy`/`add_retention_policy`/`add_columnstore_policy`) so the AppHost schedule port never schedules them; the diskann index reuses the pgvector distance operators (`<=>`/`<->`/`<#>`) so the column stays the `EmbeddingArity`-row `vector` store type and the planner routes a distance query through diskann transparently; the `DiskAnnOptions` sentinels resolve at build time from vector dimensionality — `NumNeighbors=-1` is dimension-derived degree, `NumDimensions=0` is all dimensions, and `NumBitsPerDimension=0` is automatic bit selection (build-time picks 2 below 900 dims, 1 at or above 900), while `Default` pins `NumNeighbors=50` and `NumBitsPerDimension=0` and never a fixed bit width; the BM25 index answers through `@@@` query-builder dispatch, the bare `|||`/`&&&`/`===`/`###` column operators, `pdb.score`, and `pdb.snippet` raw SQL beside the always-present native FTS baseline — the `paradedb.*` namespace is removed in `pg_search` 0.24.0 and is never emitted, and the `Fuzzy`/`Boost`/`Const`/`Slop` cast cases wrap any inner predicate as a composable `::pdb.*` modifier so a new builder is one union case, never a sibling method; the `key_field` column is `UNIQUE`/primary-key and listed first under the one-BM25-index-per-table constraint.
- Receipt: each provisioning step folds a `StoreFact`-shaped row into the open receipt's proof rows — continuous-aggregate refresh-lag, retention drop-count, and columnstore compression-ratio read from the TimescaleDB job-stats views, and the `search.vector.route`/`search.bm25.score` facts on `Query/lanes#SEARCH_LANES` read the live planner route the index serves; a provisioning failure is a typed provisioning fault.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new TimescaleDB provisioning concern is one of the four timescale `SchemaDdl` cases; a new access-method index build is one of the two index cases; a new `DiskAnnOptions` field is one column on the build axis; a new `Bm25Predicate` builder, column operator, or cast modifier is one union case; zero new surface.
- Boundary: the preload-gated TimescaleDB, `vectorscale`, and `pg_search` companions never enter the `Schema/ddl#EXTENSION_DDL` `Extensions` set — they provision here through `SchemaDdl.Sql` after the preload, never as self-provisioned `SchemaDdl.Extension` annotations; the standalone in-process DuckDB lane owns embedded and local analytical reads while these continuous aggregates own the server analytical rollups, so there is no pg_duckdb cross-engine seam and no standalone-DuckDB server role; the hypertable rollup feeds the analytical lane and the dashboard tiles on `Query/lanes#ANALYTICAL_LANE`, never a second telemetry store; the diskann opclasses (`vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`) and the `storage_layout=memory_optimized` SBQ option ride the index DDL, never a parallel vector column, and the `<#>` inner-product operator is rejected against a `storage_layout=plain` build through the `ProvisionSql` `Fin.Fail` arm; `pg_search` runs in-process inside the PG server, never linked into managed code, so the AGPL boundary is the DB deployment and a profile without `pg_search` preloaded answers through the native FTS baseline; the column-type and runtime query-value binding are owned at `Query/lanes#SEARCH_LANES` and consumed here as settled vocabulary — `Bm25Predicate` is the index-build-adjacent column-operator and `pdb.*`-builder projector this cluster owns, with its query values arriving pre-escaped from the search-lane binder, never raw runtime input. The provisioning-step cases, the `DiskAnnOps`/`DiskAnnLayout`/`DiskAnnOptions` value types, and the `SchemaDdl.Sql` fold are owned at `Schema/ddl#EXTENSION_DDL` and consumed here; this section owns the deploy-time provisioning posture and the `Bm25Predicate`/`SearchProjection` query-projection axis.

```csharp signature
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

public static class SearchProjection {
    public static string Score(string keyColumn) => $"pdb.score({keyColumn})";
    public static string Snippet(string column, string startTag, string endTag, int maxChars) =>
        $"pdb.snippet({column}, start_tag => '{startTag}', end_tag => '{endTag}', max_num_chars => {maxChars})";
    public static string Snippets(string column, int maxChars, int limit, int offset, string sortBy, string startTag, string endTag) =>
        $"pdb.snippets({column}, max_num_chars => {maxChars}, \"limit\" => {limit}, \"offset\" => {offset}, sort_by => '{sortBy}', start_tag => '{startTag}', end_tag => '{endTag}')";
    public static string SnippetPositions(string column) => $"pdb.snippet_positions({column})";
    public static string Agg(string esJson) => $"pdb.agg('{esJson}') OVER ()";
}
```

## [03]-[CLUSTER_CONFIG]

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

## [04]-[MIGRATION_BUNDLE]

- Owner: `MigrationBundle` — the service-deploy gate over idempotent `ScriptMigration` output, the self-contained `MigrationsBundle` executable artifact, and the `GetMigrations`/`HasPendingModelChanges` deploy gate.
- Cases: a script-migration deploy applying idempotent output; a bundle-artifact deploy running the self-contained executable; a pending-change gate aborting a deploy with un-applied model changes.
- Entry: `public static Fin<MigrationPlan> Plan(Seq<string> applied, Seq<string> known, bool pendingModelChanges)` — `Fin` aborts when the model carries pending changes the bundle does not contain or when an applied migration is unknown to the binary.
- Auto: a headless or web service applies migrations at deploy through the idempotent `ScriptMigration` output or the self-contained `MigrationsBundle` executable so no design tooling runs at deploy; the deploy gate folds the applied and known identifier sets and the pending-change flag against the placement's migrate authority; expand precedes contract so a destructive step rides the `Version/retention#DESTRUCTIVE_GATE` `DestructiveUnapproved` retention-approval gate before it applies.
- Packages: Microsoft.EntityFrameworkCore.Design, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one deploy modality row or one lock-light constraint pattern; zero new surface.
- Boundary: the migration-lock outcome reads from migration receipts, never from EF `Internal`-namespace types; the NOT-VALID/NOT-ENFORCED lock-light constraint pattern lets a constraint add without a full-table lock and validates in a later pass; a store newer than the binary (an applied migration the binary does not know) is a typed rejection, never a runtime branch; the destructive-step approval is owned at `Version/retention` and consumed here as the settled gate, never a second approval taxonomy.

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

## [05]-[RESEARCH]

- [SERVER_PROVISIONING_PROBE]: the live-PG18 round-trip from Assay `ProvisionRun` evidence — the `create_hypertable`/`add_continuous_aggregate_policy`/`add_columnstore_policy` apply contract, the diskann-over-`vector` `storage_layout=memory_optimized` plus the `num_neighbors`/`search_list_size`/`num_bits_per_dimension` build-option apply, and the `pg_search` 0.24.0 BM25 `@@@ pdb.parse`/`|||`/`pdb.score`/`pdb.snippet` query shape (the removed `paradedb.*` namespace asserted absent), proven against the installed extensions before the provisioning DDL pins; the `pg_cron` server-side scheduler row stays an admitted maintenance companion.
- [CLUSTER_CONFIG_PORTABILITY]: the `io_method=io_uring` GUC against the Forge-provisioned local PG18 runtime versus the `worker` portable fallback — whether the runtime kernel exposes io_uring, confirmed against the `pg_settings` observed value before the io-method triple's primary value is preferred over its fallback at deploy.
- [MIGRATION_BUNDLE_PROBE]: the `dotnet ef migrations bundle` self-contained executable and the idempotent `ScriptMigration` output applied against a live PG18 server — the `GetMigrations`/`HasPendingModelChanges` deploy-gate boundary and the NOT-VALID/NOT-ENFORCED lock-light constraint apply-then-validate contract, proven before the deploy-gate fence pins.
