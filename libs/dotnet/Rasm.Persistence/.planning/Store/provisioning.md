# [PERSISTENCE_STORE_PROVISIONING]

Rasm.Persistence verifies the operator-provisioned PostgreSQL tier through one `ProvisionVerdict` and opens the embedded SQLite floor through one idempotent ritual. `ServerExtension`, `ClusterSetting`, and `FailureRank` keep installation gates, settings, and absence policy as row data; one `NpgsqlBatch` returns the held capabilities, typed faults, repair artifacts, and `VerificationEpoch`. `HandleBridge` owns every native SQLite crossing, while `EngineOps` returns exact checkpoint and backup state from provider status and out-parameters. `ProjectionContext` supplies tenancy and time.

## [01]-[INDEX]

- [02]-[SERVER_EXTENSIONS]: `StoreCapability`'s two-plane store-capability vocabulary with the legal-corner law closing the engine sweep, `ServerExtension`'s extension × admission-gate × lane roster over the kernel capability floor, the `FailureRank` absence behavior, the one-batch verification fold over the catalog reads (extension version floors included), the four provisioning rungs, the emitted repair set, the `MaintenanceJob` in-database work roster, the `RollingWindow` Marten partition roster, the `SourceWire` data-source policy row, the EF provider-binding row, the `pg_jsonschema` in-process fallback fence, and the stamped verification epoch.
- [03]-[EMBEDDED_FLOOR]: `EmbeddedRitual`'s residency-split pragma ladder, the connection-scoped capability registration, the defensive `sqlite3_db_config` hardening, the first-opener IMMEDIATE materialization gate, and the closed-engine law.
- [04]-[ENGINE_OPERATIONS]: Native SQLite handle custody, checkpoint and backup state, snapshot pinning, blob IO, and embedded KV engines.
- [05]-[STORE_AXIS_MAP]: store perimeter across eleven axes — every provider row deployment/policy DATA on one axis surface, each scale-out row carrying its proven ceiling.

## [02]-[SERVER_EXTENSIONS]

- Owner: `Lane`, `StoreCapability`, `StoreProfile`, `ServerExtension`, `ExtensionAdmission`, `RestartClass`, `FailureRank`, and `ClusterSetting` close the verification policy. `ProvisionVerdict` carries held capabilities, typed faults, repair artifacts, and `VerificationEpoch`; `ClusterProvision` executes the one verification batch and gated admissions.
- Cases: `ServerExtension` is the AUTHORITATIVE provisioning roster — it SUPERSETS the consumer-facing `README.md` `[SERVER_EXTENSIONS]` card subset with the base-type and toolkit rows a dependency chain requires (`postgis` the standalone base the raster/sfcgal/pgrouting rows gate on, `pgvector` the `vector` base `pgvectorscale` gates on, `pg_duckdb` the in-PG DuckDB bridge, `timescaledb_toolkit` over the `timescaledb` base) so the `BaseType` gate resolves against a row the same fold can admit, never against an externally-assumed prerequisite; each gate is the `.api`-verified install precondition, NOT a loose label: `timescaledb` (preload, the hypertable/continuous-aggregate/columnstore analytics, `Query/columnar`), `timescaledb_toolkit` (the hyperfunction/time-weighted-aggregate layer over the `timescaledb` base type), `pg_duckdb` (preload, the in-PG DuckDB analytical bridge distinct from the in-process `DuckDB.NET` lane, `Query/columnar`), `apache-age` (standalone — the OPTIONAL openCypher graph functions + `agtype`, no preload; Cypher connections issue per-session `LOAD 'age'`, demoted beneath QuikGraph, `Query/cypher#GRAPH_SESSION`), `pg_cron` (preload, the in-database maintenance scheduler), `postgis` (standalone — operator classes over the BUILT-IN `gist` AM, registers no custom access method, the base the raster/3D/routing rows extend), `postgis_raster`/`postgis_sfcgal` (PostGIS raster + exact 3D geometry over the `postgis` base type), `pgvector` (the `hnsw` access-method ANN tier) / `pgvectorscale` (the `diskann` AM gated on the `vector` base type), `pg_search` (PRELOAD-gated — the ParadeDB Tantivy `bm25` engine rides `shared_preload_libraries` and hard-errors on `CREATE EXTENSION` without it), `h3-pg` (standalone — the in-PG H3 cell index over built-in AMs and the `h3_postgis` bridge over the `h3` base type, matching `pocketken.H3`), `pgrouting` (the network routing over the `postgis` base type, `Query/cypher#GRAPH_QUERY`), `pg_partman` (PRELOAD-gated — its `pg_partman_bgw` background worker rides `shared_preload_libraries`), `pg_squeeze` (preload, lock-light table-bloat reclamation), `pg_jsonschema` (standalone — `CREATE EXTENSION`-registered JSON Schema CHECK functions, no preload), `pgaudit` (preload, session/object audit logging), `pg_net` (PRELOAD-gated — its `libcurl` background worker is statically `RegisterBackgroundWorker`'d in `_PG_init` and hard-errors without `shared_preload_libraries`), `pg_graphql` (standalone — pgrx SQL functions + DDL event triggers, no worker, no preload); `ExtensionAdmission` is `Preload(library)` | `BaseType(row)` | `AccessMethod(method)` (a real queryable index AM the row registers, e.g. `hnsw`) | `Standalone(reason)` (prerequisite-free function/type/operator-class extension that registers NO gating AM); `FailureRank` is `Required`/`Degradable`/`Observational`; `StoreCapability` is nine object-plane rows the `Store/blobstore#OBJECT_STORE` provider axis holds and four engine-plane rows this axis holds (`SingleProcess | TableRewrite | StrategyRedrive | BulkCopy`), the two planes disjoint and each publishing its own membership constant; `ProvisionVerdict` is `Provisioned | MissingExtension | MissingPreload | SettingDrift | Faulted`; `ServerFault` carries provisioning refusals as one compact direct union on `FaultBand.Server`.
- Law: `Manifest` folds server expectations and embedded ritual rows into one reconcile-only `ReconcileManifest`.
- Entry: `Verify` folds one catalog batch into a typed verdict; `Register` and `Admit` consume its exact snapshot beside the `StoreProfile` whose realizability they gate on, each reading the LANE off the row it is admitting rather than a call-site token, so the `geo`, `maintenance`, and `audit` lanes gate at the extension and job doors exactly as the analytical lanes gate at their own owning entries; and `BackendObservation.Of` projects a `Provisioned` verdict into the `Store/schema#PROJECTION` observation runtime admission joins against the expected generation, taking the realized artifact set beside the adapter's own observation instant and its two recovery stamps — a probed verdict is the ONE capability evidence the backend contract admits, so a desired roster or an availability read never reaches it, and the recovery halves arrive from the owners that measured them rather than from this fold.
- Auto: one batch reads preload libraries, installed and available extensions, settings, replication-slot lag, and invalid indexes. `FailureRank.Absorb` folds missing capabilities into typed faults or removes degradable lanes; `VerificationEpoch` marks each observed cluster state.
- Packages: Npgsql (`NpgsqlDataSource.CreateBatch`, `NpgsqlBatchCommand`, `NpgsqlBatch.ExecuteReaderAsync`, `NpgsqlDataReader.NextResultAsync`/`GetInt64`/`GetString`, `NpgsqlParameter<string[]>`, `ReloadTypesAsync`, `PostgresException.SqlState`/`PostgresErrorCodes.InsufficientPrivilege`, `NpgsqlException.IsTransient`, `NpgsqlDataSourceBuilder`), Npgsql.NetTopologySuite (`NpgsqlDataSourceBuilder.UseNetTopologySuite(handleOrdinates, geographyAsDefault)` — the ADO spatial codec row), Npgsql.NodaTime (`NpgsqlNodaTimeExtensions.UseNodaTime<TMapper>(TMapper) where TMapper : INpgsqlTypeMapper` — the ADO temporal codec row the same builder binds, `NpgsqlDataSourceBuilder` implementing `INpgsqlTypeMapper`), Npgsql.OpenTelemetry (`TracerProviderBuilder.AddNpgsql()` / `MeterProviderBuilder.AddNpgsqlInstrumentation()` — the observability row subscribed at the AppHost composition root), JsonSchema.Net (`Json.Schema.JsonSchema.FromText`/`Evaluate(JsonElement, EvaluationOptions?)` — the in-process validation fence), NetTopologySuite (`Ordinates`), Microsoft.EntityFrameworkCore (+ `.Sqlite` `UseSqlite` and the Npgsql EF `UseNpgsql` — the `StoreProfile.Ef` bind row over the `Element/identity` DbContext), Marten (`IDocumentSession.QueueSqlCommand`/`SaveChangesAsync`; `StoreOptions.Schema.For<T>().PartitionOn` + `ByRollingRange`/`PartitionPeriod`/`ManagedRangePartitions` — the `RollingWindow` declaration), Rasm.Persistence.Element (`FaultBand`), NodaTime, LanguageExt.Core (`Seq`/`Fin`/`@catch`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new server extension is one `ServerExtension` row carrying its SQL, install gate, lane, absence rank, and restart class, seated after every row its gate names; a new install-gate shape is one `ExtensionAdmission` case; a new absence policy is one `FailureRank` row landing every floor-miss branch with zero `Switch` edits; a new engine capability is one `StoreCapability` row plus its membership on the profile rows holding it and one term on the plane constant, with the legal corners moving at `CapabilityLaw` alone; a new verification gap is one named `Option<ProvisionVerdict>` probe and one term in the ordered alternative; a new setting comparison mode is one named policy value and the rows electing it; a new verified setting is one `ClusterSetting` row; a new analytical lane is one `Lane` row every roster and gate then composes, never a token minted at a call site; a lane a profile cannot realize is one absent `Lanes` member and one clause on that row's `Degrade`, never a caller-side engine test; a new version floor is one `floors` entry (deployment data, never a fence literal); a new in-database maintenance job is one `MaintenanceJob` row riding the gated `Register` admission; a new partition-retired document family is one `RollingWindow` row with the `Declare` call at that family's own mapping; a new deployment axis is one `ReconcileAxis` row every manifest fold then keys on; zero new surface — a runtime `ALTER SYSTEM`, a Rasm-spawned PostgreSQL, a per-extension managed package, a `Switch` re-enumerating the absence policy at the fold, a per-extension probe round trip, or a second relational engine row is the deleted form because provisioning is verification-first SQL, the absence policy IS the rank-row delegate, the verification is one batch, and the engine sweep is closed.
- Boundary: a Rasm process NEVER spawns or bundles PostgreSQL and NEVER executes runtime `ALTER SYSTEM` — provisioning is verification-only over the operator-provisioned cluster (`#SERVER_EXTENSIONS`), so a `MissingPreload`/`SettingDrift`/`MissingExtension` verdict is a typed signal carrying the EMITTED repair artifact (a `shared_preload_libraries` diff, a `CREATE EXTENSION` reconciliation, a settings diff) the operator resolves at one of the four provisioning rungs, never a self-mutation; the server extensions carry no managed assembly and admit through raw `CREATE EXTENSION IF NOT EXISTS` gated by the row's `ExtensionAdmission` (a preload library, a base type, a real queryable access method, or a prerequisite-free standalone function/type extension) — the `.api`-verified gate per row, so a preload-gated extension mislabeled no-prerequisite cannot leak a hard-erroring `CREATE EXTENSION` past the gate; the `pg_duckdb` extension is the in-PG DuckDB bridge distinct from the in-process `DuckDB.NET` analytical lane (`Query/columnar`), the two meeting at the columnar SQL surface; `apache-age` is the OPTIONAL self-hosted openCypher graph (`Query/cypher#GRAPH_SESSION`) demoted beneath the in-process QuikGraph (`H5`), so its admission is gated and the lane is disabled by default and never assumed co-resident with Marten; spatial→PG GiST (`postgis_raster`/`postgis_sfcgal`) and ANN→`pgvector`/`pgvectorscale` are the transactional index owners while DuckDB `spatial`/`vss` are the columnar aggregators (`L2`), never duplicated; a catalog read denied by privilege folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`) and a transport failure folds through `NpgsqlException.IsTransient` so a retry re-drives only the transient class; `ReloadTypesAsync` completes the deploy by re-resolving wire types, the rejected form being a process that resolves a freshly-admitted enum/composite as unknown until restart; lane absence is stated at ADMISSION on BOTH engines — the server tier folds an absent extension through its `FailureRank` and the embedded tier refuses at `StoreProfile.Admits`, so an embedded deployment discovers the columnar, geo, cypher, vector, search, maintenance, audit, and egress lanes are unrealizable at profile selection rather than at the first query, and a lane surrendered without a `Degrade` clause is the deleted form; every rostered lane reaches a gating consumer — the analytical and egress lanes at their own owning entries, and `geo`, `maintenance`, and `audit` at `Admit` and `Register`, which read the lane off the `ServerExtension`/`MaintenanceJob` row they are admitting, so a lane joining the roster gates without a new call site and a lane no row names is unreachable by construction; ONE table has ONE partition manager — `pg_partman` owns the server-partitioned relations its `MaintenanceJob.PartitionParent` row names (`public.op_log`, rolled by the `pg_partman_bgw` worker its `PartitionCycle` row schedules) and `ManagedRangePartitions` owns the Marten document tables the `RollingWindow` rows name (rolled by the `store.Advanced` verbs the single-writer boot pass runs), so a table appearing on both rosters is the deleted form and a `cron.schedule` rotation job aimed at a Marten document table is the deleted form because those verbs are the only rotation surface a document table has.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Microsoft.EntityFrameworkCore.Metadata;
using Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Lane {
    public static readonly Lane Columnar = new("columnar");
    public static readonly Lane Geo = new("geo");
    public static readonly Lane Cypher = new("cypher");
    public static readonly Lane Vector = new("vector");
    public static readonly Lane Search = new("search");
    public static readonly Lane Maintenance = new("maintenance");
    public static readonly Lane Audit = new("audit");
    public static readonly Lane Validation = new("validation");
    public static readonly Lane Egress = new("egress");
    public static readonly Lane Cache = new("cache");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReconcileAxis {
    public static readonly ReconcileAxis RelationalSor = new("relational-sor");
    public static readonly ReconcileAxis Maintenance = new("maintenance");
    public static readonly ReconcileAxis EmbeddedRelational = new("embedded-relational");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoreCapability : ICapability<StoreCapability> {
    public static readonly StoreCapability Multipart = new("multipart");
    public static readonly StoreCapability Resume = new("resume");
    public static readonly StoreCapability BatchErase = new("batch-erase");
    public static readonly StoreCapability Tiering = new("tiering");
    public static readonly StoreCapability Thaw = new("thaw");
    public static readonly StoreCapability PerObjectWorm = new("per-object-worm");
    public static readonly StoreCapability Presign = new("presign");
    public static readonly StoreCapability ReadChecksum = new("read-checksum");
    public static readonly StoreCapability ConditionalWrite = new("conditional-write");
    public static readonly StoreCapability SingleProcess = new("single-process");
    public static readonly StoreCapability TableRewrite = new("table-rewrite");
    public static readonly StoreCapability StrategyRedrive = new("strategy-redrive");
    public static readonly StoreCapability BulkCopy = new("bulk-copy");

    public static readonly CapabilitySet<StoreCapability> ObjectPlane = CapabilitySet<StoreCapability>.Of(
        Multipart, Resume, BatchErase, Tiering, Thaw, PerObjectWorm, Presign, ReadChecksum, ConditionalWrite);
    public static readonly CapabilitySet<StoreCapability> EnginePlane = CapabilitySet<StoreCapability>.Of(
        SingleProcess, TableRewrite, StrategyRedrive, BulkCopy);

    public static readonly CapabilityLaw<StoreCapability> EngineLaw = new(Seq(
        CapabilitySet<StoreCapability>.Of(StrategyRedrive, BulkCopy),
        CapabilitySet<StoreCapability>.Of(SingleProcess, TableRewrite)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoreProfile {
    public static readonly StoreProfile Server = new("server", relational: "postgresql-18", ReconcileAxis.RelationalSor,
        lanes: [Lane.Columnar, Lane.Geo, Lane.Cypher, Lane.Vector, Lane.Search, Lane.Maintenance, Lane.Audit, Lane.Validation, Lane.Egress, Lane.Cache],
        degrade: "none at the relational tier — every analytical lane the extension roster serves is reachable, and a lane whose extension is absent folds through its own `FailureRank`",
        model: static () => CompiledModels.Server,
        capabilities: CapabilitySet<StoreCapability>.Of(StoreCapability.StrategyRedrive, StoreCapability.BulkCopy),
        ef: static (builder, connection) => builder.UseNpgsql(connection).UseModel(CompiledModels.Server));
    public static readonly StoreProfile Embedded = new("embedded", relational: "sqlite", ReconcileAxis.EmbeddedRelational,
        lanes: [Lane.Validation],
        degrade: "single-writer, single-process, no server extension: the columnar, geo, cypher, vector, search, maintenance, audit, egress, and cache lanes have no embedded realization — Marten backs both cache backends, so a single-process store realizes neither — and a profile-level `Admits` refusal states each absence at ADMISSION where the server tier's `FailureRank.Degradable` would have folded the lane out, JSON Schema validation surviving only because it degrades to the in-process fence",
        model: static () => CompiledModels.Embedded,
        capabilities: CapabilitySet<StoreCapability>.Of(StoreCapability.SingleProcess, StoreCapability.TableRewrite),
        ef: static (builder, connection) => builder.UseSqlite(connection).UseModel(CompiledModels.Embedded));
    public string Relational { get; }
    public ReconcileAxis Axis { get; }
    public FrozenSet<Lane> Lanes { get; }
    public string Degrade { get; }

    public bool Admits(Lane lane) => Lanes.Contains(lane);
    public bool Admits(string lane) => Lane.TryGet(lane, out Lane row) && Lanes.Contains(row);
    private StoreProfile(string key, string relational, ReconcileAxis axis, Lane[] lanes, string degrade,
        Func<IModel> model, CapabilitySet<StoreCapability> capabilities,
        Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> ef) : this(key) =>
        (Relational, Axis, Lanes, Degrade, Model, Capabilities, Ef) =
            (relational, axis, lanes.ToFrozenSet(), degrade, model, capabilities, ef);

    public Func<IModel> Model { get; }
    public CapabilitySet<StoreCapability> Capabilities { get; }

    public static Fin<Unit> Lawful =>
        toSeq(Items).TraverseM(static row => StoreCapability.EngineLaw.Admit(row.Capabilities)).As().Map(static _ => unit);

    public Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> Ef { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TraceEmission : ICapability<TraceEmission> {
    public static readonly TraceEmission CopySpans = new("copy-spans");
    public static readonly TraceEmission FirstResponse = new("first-response");
    public static readonly TraceEmission PhysicalOpen = new("physical-open");
}

public sealed record SourceWire(
    bool GeographyAsDefault,
    Ordinates HandleOrdinates,
    Func<NpgsqlCommand, bool> CommandFilter,
    Func<NpgsqlBatch, bool> BatchFilter,
    CapabilitySet<TraceEmission> Emits) {
    public static readonly SourceWire Canonical = new(
        GeographyAsDefault: false, HandleOrdinates: Ordinates.XYZ,
        CommandFilter: static command => !command.CommandText.Contains("pg_stat_", StringComparison.Ordinal),
        BatchFilter: static _ => true,
        Emits: CapabilitySet<TraceEmission>.Of(TraceEmission.PhysicalOpen));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RestartClass {
    public static readonly RestartClass Session = new("session", rank: 0);
    public static readonly RestartClass Reload = new("reload", rank: 1);
    public static readonly RestartClass Restart = new("restart", rank: 2);
    public int Rank { get; }
    private RestartClass(string key, int rank) : this(key) => Rank = rank;

    public static RestartClass Max(Seq<RestartClass> over) =>
        over.Fold(Session, static (worst, next) => next.Rank > worst.Rank ? next : worst);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FailureRank {
    public static readonly FailureRank Required = new(
        "required",
        static (_, key) => Fin.Fail<Seq<Error>>(new ServerFault.RequiredAbsent(key)));
    public static readonly FailureRank Degradable = new(
        "degradable",
        static (faults, key) => Fin.Succ(faults.Add(new ServerFault.LaneFolded(key))));
    public static readonly FailureRank Observational = new(
        "observational",
        static (faults, key) => Fin.Succ(faults.Add(new ServerFault.Evidence("<absent>"))));

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Error>> Absorb(Seq<Error> faults, string extensionKey);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtensionAdmission {
    private ExtensionAdmission() { }
    public sealed record Preload(string Library) : ExtensionAdmission;
    public sealed record BaseType(ServerExtension Extension) : ExtensionAdmission;
    public sealed record AccessMethod(string Method) : ExtensionAdmission;
    public sealed record Standalone(string Reason) : ExtensionAdmission;

    public bool Admissible(IReadOnlySet<string> preloaded, CapabilitySet<ServerExtension> created) =>
        this.Switch(
            preload: p => preloaded.Contains(p.Library),
            baseType: b => created.Admits(b.Extension),
            accessMethod: static _ => true,
            standalone: static _ => true);
    public Option<string> PreloadLibrary => this is Preload p ? Some(p.Library) : None;
    public int Depth => this is BaseType b ? b.Extension.Rank + 1 : 0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ServerExtension : ICapability<ServerExtension> {
    public static readonly ServerExtension Timescaledb = new("timescaledb", new ExtensionAdmission.Preload("timescaledb"), Lane.Columnar, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension TimescaledbToolkit = new("timescaledb_toolkit", new ExtensionAdmission.BaseType(Timescaledb), Lane.Columnar, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgDuckdb = new("pg_duckdb", new ExtensionAdmission.Preload("pg_duckdb"), Lane.Columnar, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension Postgis = new("postgis", new ExtensionAdmission.Standalone("operator classes over the built-in gist AM; registers no custom access method"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension ApacheAge = new("age", new ExtensionAdmission.Standalone("openCypher graph functions + the agtype type over built-in storage; registers no custom AM, CREATE EXTENSION needs no preload, Cypher connections issue per-session LOAD 'age'"), Lane.Cypher, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgCron = new("pg_cron", new ExtensionAdmission.Preload("pg_cron"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgSqueeze = new("pg_squeeze", new ExtensionAdmission.Preload("pg_squeeze"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension Pgaudit = new("pgaudit", new ExtensionAdmission.Preload("pgaudit"), Lane.Audit, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PostgisRaster = new("postgis_raster", new ExtensionAdmission.BaseType(Postgis), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PostgisSfcgal = new("postgis_sfcgal", new ExtensionAdmission.BaseType(Postgis), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvector = new("vector", new ExtensionAdmission.AccessMethod("hnsw"), Lane.Vector, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvectorscale = new("vectorscale", new ExtensionAdmission.BaseType(Pgvector), Lane.Vector, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PgSearch = new("pg_search", new ExtensionAdmission.Preload("pg_search"), Lane.Search, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension H3Pg = new("h3", new ExtensionAdmission.Standalone("operator classes over the built-in btree/hash/brin/spgist AMs; registers no custom access method"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension H3Postgis = new("h3_postgis", new ExtensionAdmission.BaseType(H3Pg), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgrouting = new("pgrouting", new ExtensionAdmission.BaseType(Postgis), Lane.Cypher, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgPartman = new("pg_partman", new ExtensionAdmission.Preload("pg_partman_bgw"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgJsonschema = new("pg_jsonschema", new ExtensionAdmission.Standalone("CREATE EXTENSION-registered json_matches_schema/jsonb_matches_schema CHECK functions; no preload, no custom AM"), Lane.Validation, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgNet = new("pg_net", new ExtensionAdmission.Preload("pg_net"), Lane.Egress, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgGraphql = new("pg_graphql", new ExtensionAdmission.Standalone("pgrx SQL functions + DDL event triggers; no background worker, no preload, no custom AM"), Lane.Egress, FailureRank.Observational, RestartClass.Reload);

    public ExtensionAdmission Admission { get; }
    public Lane Lane { get; }
    public FailureRank Absence { get; }
    public RestartClass Restart { get; }
    private ServerExtension(string key, ExtensionAdmission admission, Lane lane, FailureRank absence, RestartClass restart) : this(key) =>
        (Admission, Lane, Absence, Restart) = (admission, lane, absence, restart);

    public int Rank => Admission.Depth;

    public string CreateSql => $"CREATE EXTENSION IF NOT EXISTS \"{Key}\" CASCADE;";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClusterSetting {
    public static readonly ClusterSetting WalLevel = new("wal_level", expected: "logical", RestartClass.Restart, Exact);
    public static readonly ClusterSetting MaxWorkerProcesses = new("max_worker_processes", expected: "8", RestartClass.Restart, Floor);
    public static readonly ClusterSetting MaxParallelWorkers = new("max_parallel_workers", expected: "8", RestartClass.Reload, Floor);
    public static readonly ClusterSetting MaxParallelWorkersPerGather = new("max_parallel_workers_per_gather", expected: "4", RestartClass.Reload, Floor);
    public static readonly ClusterSetting MaxReplicationSlots = new("max_replication_slots", expected: "8", RestartClass.Restart, Floor);
    public static readonly ClusterSetting MaxWalSenders = new("max_wal_senders", expected: "8", RestartClass.Restart, Floor);

    public string Expected { get; }
    public RestartClass Restart { get; }
    private ClusterSetting(string key, string expected, RestartClass restart, Func<string, string, bool> holds) : this(key) =>
        (Expected, Restart, Holds) = (expected, restart, holds);

    [UseDelegateFromConstructor]
    public partial bool Holds(string actual, string expected);
    public bool Satisfied(string actual) => Holds(actual, Expected);

    static readonly Func<string, string, bool> Floor = static (actual, expected) =>
        long.TryParse(actual, NumberStyles.Integer, CultureInfo.InvariantCulture, out long held)
        && long.TryParse(expected, NumberStyles.Integer, CultureInfo.InvariantCulture, out long want)
        && held >= want;
    static readonly Func<string, string, bool> Exact = static (actual, expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<long>]
public readonly partial struct VerificationEpoch {
    public static VerificationEpoch From(Instant at) => From(at.ToUnixTimeMilliseconds());
}

public readonly record struct RepairArtifact(string Kind, string Statement, RestartClass Restart);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaintenanceJob {
    public static readonly MaintenanceJob PartitionParent = new(
        "partition-parent", ServerExtension.PgPartman,
        "SELECT partman.create_parent(p_parent_table := 'public.op_log', p_control := 'occurred_at', p_interval := '1 month', p_type := 'range');");
    public static readonly MaintenanceJob PartitionCycle = new(
        "partition-cycle", ServerExtension.PgCron,
        "SELECT cron.schedule_in_database('rasm-partman', '17 * * * *', 'CALL partman.run_maintenance_proc()', current_database());");
    public static readonly MaintenanceJob RetentionSweep = new(
        "retention-sweep", ServerExtension.PgCron,
        "SELECT cron.schedule_in_database('rasm-retention', '*/5 * * * *', 'SELECT rasm_retention_sweep()', current_database());");
    public static readonly MaintenanceJob SqueezeHotTables = new(
        "squeeze-hot", ServerExtension.PgSqueeze,
        "INSERT INTO squeeze.tables (tabschema, tabname, schedule) VALUES ('public', 'op_log', '31 2 * * *') ON CONFLICT (tabschema, tabname) DO UPDATE SET schedule = EXCLUDED.schedule;");

    public ServerExtension Owner { get; }
    public string RegisterSql { get; }
    private MaintenanceJob(string key, ServerExtension owner, string registerSql) : this(key) =>
        (Owner, RegisterSql) = (owner, registerSql);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RollingWindow {
    public static readonly RollingWindow CacheBlob = new("cache-blob", PartitionPeriod.Day, ahead: 2, aged: 8);
    public static readonly RollingWindow DeadLetter = new("dead-letter", PartitionPeriod.Week, ahead: 2, aged: 14);

    static readonly ManagedRangePartitions Managed = new();

    public PartitionPeriod Period { get; }
    public int Ahead { get; }
    public int Aged { get; }
    private RollingWindow(string key, PartitionPeriod period, int ahead, int aged) : this(key) =>
        (Period, Ahead, Aged) = (period, ahead, aged);

    public StoreOptions Declare<T>(StoreOptions opts, Expression<Func<T, DateTimeOffset>> key) where T : notnull {
        opts.Schema.For<T>().PartitionOn(key, x => x.ByRollingRange(Managed, Period, Ahead, Aged));
        return opts;
    }
}

public sealed record ExtensionFloor(string Minimum, Func<string, string, bool> Satisfied);

public sealed record ClusterDemand(CapabilitySet<ServerExtension> Required, HashMap<string, ExtensionFloor> Floors, VerificationEpoch Epoch) {
    public Seq<ServerExtension> Ordered =>
        toSeq(Required.Held.OrderBy(static row => row.Rank).ThenBy(static row => row.Key, StringComparer.Ordinal));
}

public readonly record struct ClusterReading(
    FrozenSet<string> Preloaded,
    HashMap<string, string> Versions,
    FrozenSet<string> Available,
    HashMap<string, string> Settings,
    long SlotLag,
    long InvalidIndexes) {
    public CapabilitySet<ServerExtension> Created => CapabilitySet<ServerExtension>.Of(
        toSeq(ServerExtension.Items).Filter(row => Versions.ContainsKey(row.Key)).ToArray());
}

public sealed record ReconcileRow(string Axis, string Key, string Declared, string Restart);

public sealed record ReconcileManifest(Seq<ReconcileRow> Rows, VerificationEpoch Epoch);

public sealed record JsonValidationContract(string Text, Json.Schema.JsonSchema Parsed) {
    public static Fin<JsonValidationContract> Parse(string text) =>
        ClusterProvision.Lifted(() => new JsonValidationContract(text, Json.Schema.JsonSchema.FromText(text)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvisionVerdict {
    private ProvisionVerdict() { }
    public sealed record Provisioned(
        CapabilitySet<ServerExtension> Held,
        CapabilitySet<ServerExtension> Created,
        FrozenSet<string> Preloaded,
        Seq<Error> Faults,
        VerificationEpoch Epoch) : ProvisionVerdict {
        public FrozenSet<Lane> HeldLanes => Held.Held.Select(static row => row.Lane).ToFrozenSet();
    }
    public sealed record MissingExtension(Seq<ServerExtension> Absent, Seq<RepairArtifact> Repairs, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record MissingPreload(Seq<ServerExtension> Unloaded, RepairArtifact PreloadDiff, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record SettingDrift(
        string Setting, string Expected, Option<string> Actual, RestartClass Restart, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record Faulted(Error Fault, VerificationEpoch Epoch) : ProvisionVerdict;

    public bool Admits => this is Provisioned;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ServerFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Server;
    private ServerFault() { }
    [FaultCase(0)]
    public sealed partial record Unmapped(string SqlState, Error Cause) : ServerFault(), ICausedFault;
    [FaultCase(1)]
    public sealed partial record Unreachable(Error Cause) : ServerFault(), ICausedFault {
        public override Retriability Retriability => Retriability.Transient;
    }
    [FaultCase(2)]
    public sealed partial record CatalogDenied(string Relation, Error Cause) : ServerFault(), ICausedFault;
    [FaultCase(3)]
    public sealed partial record RequiredAbsent(string Extension) : ServerFault();
    [FaultCase(4)]
    public sealed partial record LaneFolded(string Extension) : ServerFault();
    [FaultCase(5)]
    public sealed partial record Evidence(string Extension, string Detail) : ServerFault();
    [FaultCase(6)]
    public sealed partial record SlotLag(long Bytes) : ServerFault();
    [FaultCase(7)]
    public sealed partial record InvalidIndex(long Count) : ServerFault();
    [FaultCase(8)]
    public sealed partial record Ungated(string Extension) : ServerFault();
    [FaultCase(9)]
    public sealed partial record AdmitRefused(Error Cause) : ServerFault(), ICausedFault;

    public virtual RetryShape Route => Retriability is Retriability.TerminalCase ? RetryShape.Terminal : RetryShape.Waited;

    public override string Message => Switch(
        unmapped:       static c => $"<sqlstate:{c.SqlState}>:{c.Cause.Message}",
        unreachable:    static c => $"cluster unreachable: {c.Cause.Message}",
        catalogDenied:  static c => $"catalog read denied: {c.Relation}",
        requiredAbsent: static c => $"<required-absent:{c.Extension}>",
        laneFolded:     static c => $"<lane-folded:{c.Extension}>",
        evidence:       static c => $"<evidence:{c.Extension}:{c.Detail}>",
        slotLag:        static c => $"<slot-lag:{c.Bytes}>",
        invalidIndex:   static c => $"<invalid-indexes:{c.Count}>",
        ungated:        static c => $"<provision-ungated:{c.Extension}>",
        admitRefused:   static c => $"<provision-admit:{c.Cause.Message}>");

    public static Error Lift(Error error) => error.Exception.Case switch {
        PostgresException { SqlState: PostgresErrorCodes.InsufficientPrivilege } denied =>
            new CatalogDenied(denied.TableName ?? "pg_catalog", error),
        NpgsqlException { IsTransient: true } => new Unreachable(error),
        PostgresException postgres => new Unmapped(postgres.SqlState, error),
        Json.Schema.JsonSchemaException or Json.Schema.RefResolutionException =>
            new AdmitRefused(error),
        _ => error,
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ClusterProvision {
    public static ReconcileManifest Manifest(ClusterDemand demand, EmbeddedRitual ritual) => new(
        toSeq(StoreProfile.Items).Map(static row => new ReconcileRow(row.Axis.Key, row.Key, $"{row.Relational}|{row.Capabilities.Wire}", RestartClass.Restart.Key))
        + demand.Ordered.Map(static row => new ReconcileRow(ReconcileAxis.RelationalSor.Key, row.Key, row.CreateSql, row.Restart.Key))
        + toSeq(ClusterSetting.Items).Map(static row => new ReconcileRow(ReconcileAxis.RelationalSor.Key, row.Key, row.Expected, row.Restart.Key))
        + toSeq(MaintenanceJob.Items).Map(static row => new ReconcileRow(ReconcileAxis.Maintenance.Key, row.Key, row.RegisterSql, row.Owner.Restart.Key))
        + toSeq(RollingWindow.Items).Map(static row => new ReconcileRow(ReconcileAxis.Maintenance.Key, row.Key, $"{row.Period}:+{row.Ahead}/-{row.Aged}", RestartClass.Session.Key))
        + Seq(new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, "<cipher-provider>", "SQLitePCLRaw.bundle_e_sqlite3mc", RestartClass.Restart.Key))
        + ritual.ConnectionRows.Map(static row => new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, row.Row, row.Sql, RestartClass.Session.Key))
        + ritual.DbConfig.Map(static row => new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, row.Row, row.Value.ToString(CultureInfo.InvariantCulture), RestartClass.Session.Key)),
        demand.Epoch);

    public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, ClusterDemand demand) =>
        IO.liftAsync(async () => {
            await using NpgsqlConnection connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using NpgsqlBatch batch = connection.CreateBatch();
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT current_setting('shared_preload_libraries')"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT extname, extversion FROM pg_extension"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT name FROM pg_available_extensions"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT name, setting FROM pg_settings WHERE name = ANY(@names)") {
                Parameters = { new NpgsqlParameter<string[]>("names", toSeq(ClusterSetting.Items).Map(static s => s.Key).ToArray()) },
            });
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT coalesce(max(pg_wal_lsn_diff(pg_current_wal_lsn(), restart_lsn)), 0)::bigint FROM pg_replication_slots WHERE restart_lsn IS NOT NULL"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT count(*)::bigint FROM pg_index WHERE NOT indisvalid"));
            await using NpgsqlDataReader reader = await batch.ExecuteReaderAsync().ConfigureAwait(false);
            if (!await reader.ReadAsync().ConfigureAwait(false)) { return Refused(demand, "shared_preload_libraries"); }
            FrozenSet<string> preloaded = reader.GetString(0)
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToFrozenSet(StringComparer.Ordinal);
            if (await reader.ReadAsync().ConfigureAwait(false)) { return Refused(demand, "shared_preload_libraries"); }
            HashMap<string, string> versions = toHashMap(await Next(reader, static row => (row.GetString(0), row.GetString(1))).ConfigureAwait(false));
            FrozenSet<string> available = (await Next(reader, static row => row.GetString(0)).ConfigureAwait(false)).ToFrozenSet(StringComparer.Ordinal);
            HashMap<string, string> settings = toHashMap(await Next(reader, static row => (row.GetString(0), row.GetString(1))).ConfigureAwait(false));
            Fin<long> slotLag = await NextScalar(reader, static row => row.GetInt64(0), "slot-lag").ConfigureAwait(false);
            Fin<long> invalidIndexes = await NextScalar(reader, static row => row.GetInt64(0), "invalid-indexes").ConfigureAwait(false);
            return (slotLag, invalidIndexes)
                .Apply((lag, invalid) => Fold(demand, new ClusterReading(
                    Preloaded: preloaded, Versions: versions, Available: available, Settings: settings,
                    SlotLag: lag, InvalidIndexes: invalid)))
                .As().ToFin()
                .Match(Succ: static verdict => verdict,
                       Fail: fault => (ProvisionVerdict)new ProvisionVerdict.Faulted(ServerFault.Lift(fault), demand.Epoch));
        })
        | @catch<IO, ProvisionVerdict>(static _ => true,
            error => IO.pure((ProvisionVerdict)new ProvisionVerdict.Faulted(ServerFault.Lift(error), demand.Epoch)));

    static ProvisionVerdict Refused(ClusterDemand demand, string name) =>
        new ProvisionVerdict.Faulted(
            ServerFault.Lift(new KernelFault.InvalidValue(Label: name, Requirement: "exactly one verification row")),
            demand.Epoch);

    static ProvisionVerdict Fold(ClusterDemand demand, ClusterReading read) =>
        (PreloadGap(demand, read) | ExtensionGap(demand, read) | SettingGap(demand, read))
            .IfNone(() => Survivors(demand, read));

    static Option<ProvisionVerdict> PreloadGap(ClusterDemand demand, ClusterReading read) =>
        demand.Ordered.Filter(row => row.Admission is ExtensionAdmission.Preload preload
            && !read.Preloaded.Contains(preload.Library)) is { IsEmpty: false } unloaded
            ? Some<ProvisionVerdict>(new ProvisionVerdict.MissingPreload(unloaded, new RepairArtifact(
                "shared_preload_libraries",
                $"shared_preload_libraries = '{string.Join(',', read.Preloaded.Concat(unloaded.Choose(static row => row.Admission.PreloadLibrary).Distinct()))}'",
                RestartClass.Max(unloaded.Map(static row => row.Restart))), demand.Epoch))
            : None;

    static Option<ProvisionVerdict> ExtensionGap(ClusterDemand demand, ClusterReading read) =>
        demand.Ordered.Filter(row => !read.Created.Admits(row) && read.Available.Contains(row.Key)
            && row.Admission.Admissible(read.Preloaded, read.Created)) is { IsEmpty: false } missing
            ? Some<ProvisionVerdict>(new ProvisionVerdict.MissingExtension(missing,
                missing.Map(static row => new RepairArtifact("create_extension", row.CreateSql, row.Restart)), demand.Epoch))
            : None;

    static Option<ProvisionVerdict> SettingGap(ClusterDemand demand, ClusterReading read) =>
        toSeq(ClusterSetting.Items)
            .Find(row => read.Settings.Find(row.Key).Match(
                Some: actual => !row.Satisfied(actual),
                None: static () => true))
            .Map(row => (ProvisionVerdict)new ProvisionVerdict.SettingDrift(
                row.Key, row.Expected, read.Settings.Find(row.Key), row.Restart, demand.Epoch));

    static ProvisionVerdict Survivors(ClusterDemand demand, ClusterReading read) {
        Seq<Error> readiness =
            (read.SlotLag > 0 ? Seq<Error>(new ServerFault.SlotLag(read.SlotLag)) : Seq<Error>())
            + (read.InvalidIndexes > 0 ? Seq<Error>(new ServerFault.InvalidIndex(read.InvalidIndexes)) : Seq<Error>())
            + demand.Floors.AsIterable().ToSeq().Choose(floor => read.Versions.Find(floor.Key)
                .Filter(held => !floor.Value.Satisfied(held, floor.Value.Minimum))
                .Map(held => (Error)new ServerFault.Evidence(floor.Key, $"version:{held}<{floor.Value.Minimum}")));
        (CapabilitySet<ServerExtension> Held, Seq<Error> Faults, Seq<ServerExtension> Absent) fold = demand.Ordered.Fold(
            (Held: CapabilitySet<ServerExtension>.None, Faults: readiness, Absent: Seq<ServerExtension>()),
            (acc, row) => read.Created.Admits(row)
                ? (acc.Held.With(row), acc.Faults, acc.Absent)
                : row.Absence.Absorb(acc.Faults, row.Key).Match(
                    Succ: faults => (acc.Held, faults, acc.Absent),
                    Fail: fault => (acc.Held, acc.Faults.Add(fault), acc.Absent.Add(row))));
        return fold.Absent.IsEmpty
            ? new ProvisionVerdict.Provisioned(fold.Held, read.Created, read.Preloaded, fold.Faults, demand.Epoch)
            : new ProvisionVerdict.MissingExtension(fold.Absent, Seq<RepairArtifact>(), demand.Epoch);
    }

    public static IO<Fin<Unit>> Admit(StoreProfile profile, IDocumentSession session, ServerExtension extension, ProvisionVerdict.Provisioned cluster) =>
        Queued(session, extension.CreateSql,
            profile.Admits(extension.Lane) && extension.Admission.Admissible(cluster.Preloaded, cluster.Created)
                ? None
                : Some(new ServerFault.Ungated(extension.Key)));

    public static IO<Unit> Reload(NpgsqlDataSource source) =>
        HostEdge.CapturedIO(async _ => {
            await source.ReloadTypesAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }).Map(outcome => outcome.MapFail(ServerFault.Lift))
        .Bind(IO.lift);

    public static IO<Fin<Unit>> Register(StoreProfile profile, IDocumentSession session, MaintenanceJob job, ProvisionVerdict.Provisioned cluster) =>
        Queued(session, job.RegisterSql,
            profile.Admits(job.Owner.Lane) && cluster.Created.Admits(job.Owner)
                ? None
                : Some(new ServerFault.Ungated(job.Owner.Key)));

    static IO<Fin<Unit>> Queued(IDocumentSession session, string sql, Option<ServerFault> refusal) =>
        refusal.Match(
            Some: fault => IO.pure(Fin<Unit>.Fail(fault)),
            None: () => HostEdge.CapturedIO(async token => {
                session.QueueSqlCommand(sql);
                await session.SaveChangesAsync(token).ConfigureAwait(false);
                return Fin<Unit>.Succ(unit);
            }).Map(outcome => outcome.MapFail(ServerFault.Lift)));

    public static NpgsqlDataSource Source(string dsn, string name, SourceWire wire) {
        NpgsqlDataSourceBuilder builder = new(dsn) { Name = name };
        builder.UseNetTopologySuite(handleOrdinates: wire.HandleOrdinates, geographyAsDefault: wire.GeographyAsDefault);
        builder.UseNodaTime();
        builder.ConfigureTracing(tracing => tracing
            .ConfigureCommandFilter(wire.CommandFilter)
            .ConfigureBatchFilter(wire.BatchFilter)
            .ConfigureCopyOperationFilter(_ => wire.Emits.Admits(TraceEmission.CopySpans))
            .EnableFirstResponseEvent(wire.Emits.Admits(TraceEmission.FirstResponse))
            .EnablePhysicalOpenTracing(wire.Emits.Admits(TraceEmission.PhysicalOpen)));
        return builder.Build();
    }

    public static Fin<bool> SchemaCheck(
        FrozenSet<Lane> heldLanes,
        JsonValidationContract schema,
        JsonElement instance,
        Func<string, JsonElement, bool> serverCheck) =>
        Lifted(() => heldLanes.Contains(ServerExtension.PgJsonschema.Lane)
            ? serverCheck(schema.Text, instance)
            : schema.Parsed.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Flag }).IsValid);

    internal static Fin<T> Lifted<T>(Func<T> crossing) =>
        Try.lift(() => crossing()).Run().MapFail(static error => ServerFault.Lift(error));

    static async Task<Seq<T>> Next<T>(NpgsqlDataReader reader, Func<NpgsqlDataReader, T> read) {
        await reader.NextResultAsync().ConfigureAwait(false);
        Seq<T> rows = default;
        while (await reader.ReadAsync().ConfigureAwait(false)) { rows = rows.Add(read(reader)); }
        return rows;
    }

    static async Task<Fin<T>> NextScalar<T>(NpgsqlDataReader reader, Func<NpgsqlDataReader, T> read, string name) {
        Seq<T> rows = await Next(reader, read).ConfigureAwait(false);
        return rows.Count == 1
            ? Fin.Succ(rows[0])
            : Fin.Fail<T>(new KernelFault.OutOfRange(
                Label: name, Scalar: rows.Count, Requirement: "exactly one verification row"));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                            | [BIND]                                               |
| :-----: | :------------------ | :------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | provisioning stance | verification-first                                 | never `ALTER SYSTEM`; never spawns PG                |
|  [02]   | verification cost   | one six-command `CreateBatch` round trip           | data-volume-independent; no ext probe                |
|  [03]   | absence policy      | `FailureRank.Absorb` delegate                      | required/degradable/observational tiers              |
|  [04]   | install gate        | `ExtensionAdmission` (preload/type/AM/standalone)  | `.api`-verified; CASCADE pulls dependency            |
|  [05]   | preload gap         | `MissingPreload` + emitted diff                    | resolves at cluster config; restart class            |
|  [06]   | setting drift       | `pg_settings` vs `ClusterSetting`                  | folds `SettingDrift` + `RestartClass`                |
|  [07]   | repair posture      | EMIT artifacts, never execute                      | grants + settings diffs are typed outputs            |
|  [08]   | drift visibility    | stamped `VerificationEpoch`                        | re-verify advance = health-probe event               |
|  [09]   | deploy completion   | `ReloadTypesAsync`                                 | types re-resolve before deploy is done               |
|  [10]   | h3 parity           | `h3-pg`/`h3_postgis` match `pocketken.H3`          | one cell id at ingest and in SQL                     |
|  [11]   | data-source policy  | `SourceWire` row on `Source`                       | codec + tracing compose once; literals deleted       |
|  [12]   | EF provider bind    | `StoreProfile.Ef` row data                         | one identity DbContext, two providers                |
|  [13]   | observability       | `AddNpgsql`/`AddNpgsqlInstrumentation`             | AppHost composition root, not in-fence               |
|  [14]   | schema validation   | `SchemaCheck` dual backend                         | `json_matches_schema` or `Evaluate` fallback         |
|  [15]   | fault typing        | 838x `ServerFault` whole decade                    | generated absence/readiness/admission                |
|  [16]   | version floors      | `floors` deployment data vs `extversion`           | below-floor threads `ServerFault.Evidence`           |
|  [17]   | maintenance roster  | `MaintenanceJob` rows via gated `Register`         | cron/partman/squeeze registration; no loop           |
|  [18]   | desired-state wire  | `Manifest(demand, ritual)` typed projection        | drift diffs two documents; no second expectation set |
|  [19]   | rolling windows     | `RollingWindow` rows via `Declare`                 | one shared manager; one table, one partition manager |
|  [20]   | temporal wire       | `UseNodaTime()` on `Source`                        | raw lanes read the branch instant, never a date      |
|  [21]   | lane gate           | `Admits(row.Lane)` at `Admit`/`Register`           | geo/maintenance/audit gate off row data              |
|  [22]   | lane vocabulary     | `Lane` `[SmartEnum<string>]` owns the tokens       | roster and gate compose members; bare text deleted   |
|  [23]   | engine capability   | `StoreProfile.Capabilities` over `StoreCapability` | three bool columns collapsed; corners law-gated      |
|  [24]   | verdict ladder      | ordered `Option` alternative over three gaps       | one expression; a fourth gap is one term             |
|  [25]   | server lift         | `ServerFault.Lift` on every throwing crossing      | one funnel; provider classification, not a roster    |

## [03]-[EMBEDDED_FLOOR]

- Owner: `EmbeddedRitual` the idempotent open-ritual record carrying the file-persistent provisioning rows, the per-connection pragma rows, the defensive `DbConfig` set, and the connection-scoped `Capability` registrations (each a named `Action<SqliteConnection>` grant); `EmbeddedStore` the static surface owning the dialed connection, the KMS-custodied key application, the residency-split fold, the first-opener IMMEDIATE materialization gate, the rekey rotation, and the closed-engine law — the bound provider is the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle (`Batteries_V2.Init()` binds `SQLite3Provider_e_sqlite3mc`; one provider per process), so the embedded floor is ENCRYPTED at rest wherever a data key is supplied and the plain open is the same ritual with the key slot `None`.
- Cases: the ritual's `ConnectionRows` are the per-connection pragmas (`synchronous=NORMAL`, `journal_size_limit`, `temp_store=MEMORY`, `cache_size`) the fold re-applies on every open; the `Capabilities` are the schema-resident registrations (`uuid7`/`xxh128` scalar UDFs and the `instant_iso` collation the identity policy and chronological ordering need, a domain aggregate) that register before the first statement or the file is unreadable; the `DbConfig` set is the defensive-mode + double-quoted-literal-rejection posture applied through the raw `Handle`; the file-persistent `application_id`/`user_version` are provisioning identity the materialization gate writes, never per-connection.
- Entry: `public static SqliteConnection Dialed(string path)` opens a non-pooled embedded connection with the canonical connection-string posture (`ForeignKeys`, `ReadWriteCreate`); `public static Fin<Seq<RitualStep>> Open(SqliteConnection store, EmbeddedRitual ritual, Option<ReadOnlyMemory<byte>> dek, Action<SqliteConnection, SqliteTransaction, long> materialize)` folds the declared ritual end-to-end idempotently — the supplied data key applies FIRST through `raw.sqlite3_key(handle, dek.Span)` before any statement touches a data page (the `Element/identity#KMS_CUSTODY` `EnvelopeKeyring.Unwrap` recovers it and the caller zeroizes through `CryptographicOperations.ZeroMemory` after the keyed open, so no passphrase persists past the crossing); `public static Fin<Unit> Rekey(SqliteConnection store, ReadOnlyMemory<byte> next)` rotates the store key in place through `raw.sqlite3_rekey(handle, next.Span)` after a fresh custody mint — an empty `next` strips the cipher for a plaintext export — identity check, per-connection pragma rows, defensive `sqlite3_db_config` hardening, extended-result-code arming, capability registration, the IMMEDIATE materialization gate, the epoch read — the whole ritual riding the `#ENGINE_OPERATIONS` `HandleBridge.Opened` crossing, which owns handle resolution, the one capture converting a provider fault to `EmbeddedFault`, and disposal on every failure path, so no arm here tests a handle, converts a throw, or disposes and none escapes with a leaked live handle; `materialize` is the first-opener step run under the one IMMEDIATE transaction when the held epoch trails the compiled epoch.
- Auto: every connection folds the same declared SQLite ritual: keying, identity check, connection pragmas, defensive configuration, extended result codes, capabilities, materialization, and epoch admission.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`/`CreateFunction`/`CreateAggregate`/`CreateCollation`/`BeginTransaction(IsolationLevel, deferred)`), SQLitePCLRaw.bundle_e_sqlite3mc (`Batteries_V2.Init()` binding `SQLite3Provider_e_sqlite3mc`; the keying delta `raw.sqlite3_key(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_key_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`; the carried-over raw surface `raw.sqlite3_db_config(sqlite3, int, int, out int)`/`raw.sqlite3_extended_result_codes`/`raw.SQLITE_DBCONFIG_DEFENSIVE`=1010/`raw.SQLITE_DBCONFIG_DQS_DDL`=1014/`raw.SQLITE_DBCONFIG_DQS_DML`=1013 — backup, snapshot, WAL, db_config, and serialize calls carry over the `mc` provider unchanged), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new pragma is one `ConnectionRows` row; a new capability is one `Capabilities` registration; a new defensive posture is one `DbConfig` row; zero new surface — a second embedded relational engine (libSQL, LiteDB, RavenDB.Embedded, `Realm`, hctree, embedded-pg, EF InMemory), a per-process bootstrap branch, a nonzero `busy_timeout`, a persisted capability, or a `locking_mode=EXCLUSIVE`/shared-cache posture is the deleted form because the engine sweep is closed, the ritual is the one open path, and the provider already retries `BUSY`/`LOCKED`.
- Boundary: the embedded SQLite floor is the single-process embedded store beneath the server tier — the one engine sweep is CLOSED (PostgreSQL + embedded SQLite only; libSQL, LiteDB, RavenDB.Embedded, `Realm`, hctree, embedded-pg, EF InMemory all rejected) so a new engine row is the named defect; `StoreProfile` and the `Store/schema#CONTRACT` `BackendProvider` axis are DISJOINT vocabularies and neither rejects the other's rows — a profile row names an engine THIS package opens and provisions in process, a provider row names an engine identity a schema GENERATION is minted for anywhere in the solution, so PGlite is not a rejected engine but a category the profile axis cannot spell: it publishes no .NET provider and this package never opens one, while it IS PostgreSQL at the contract grain (its wire error carries the pg `code` and `constraint` verbatim), so a generation minted for postgres serves a peer-hosted PGlite unchanged; and the embedded floor and the PostgreSQL server tier are two engines on the one `StoreProfile` axis (`#SERVER_EXTENSIONS` `StoreProfile`), the profile selecting one by deployment, never a third; pragma rows carry RESIDENCY — file-persistent rows (`journal_mode`, `application_id`, `user_version`) are provisioning identity the materialization gate writes and the ritual folds ONLY per-connection rows; capability registration is connection-instance-scoped and never persisted — schema-resident functions, aggregates, and collations register before the first statement or the file is unreadable, and `isDeterministic: true` is the capability grant admitting a function into expression indexes and generated columns; every embedded connection is non-pooled because a physical handle's cipher identity is fixed by its first key bind and path-only pooling can return a handle keyed under different material; the WAL `-wal`/`-shm` sidecar set is the unit of copy/replace/delete (a main file separated from its sidecars is silent page-level corruption); STRICT tables are the typed admission gate and `RETURNING` supersedes write-then-read identity round trips; the defensive `sqlite3_db_config` set and double-quoted-literal rejection are connection POLICY applied through the `#ENGINE_OPERATIONS` `HandleBridge` (`api-sqlite#IMPLEMENTATION_LAW`), not connection-string knobs; extension loading stays FULLY disabled — the `Canonical` ritual arms neither the SQL `load_extension()` function nor the C-API loader (`SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` is absent from the `DbConfig` set), so the bundled floor admits no runtime extension and a `DbConfig` row arming the loader is the deliberate opt-in a deployment that needs one adds, never the default; encryption-at-rest is the BOUND provider's law — the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle supersedes the plain `e_sqlite3` bundle where the encrypted floor mounts (one provider binds per process, so the selection is this provisioning row, never a per-connection knob), key material is the KMS-unwrapped DEK crossing as `ReadOnlySpan<byte>` through `raw.sqlite3_key` and zeroized after the bind, a `Password=` connection-string value exists only for the ephemeral open of an inspected foreign store and never enters durable configuration, and classification ceilings thereby extend to the offline lane — a stolen laptop or synced file leaks nothing; the ritual is the one open path so a per-process bootstrap branch is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Data;
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using static LanguageExt.Prelude;

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct RitualStep(string Row, Option<long> Applied);

public sealed record EmbeddedRitual(
    long Identity,
    long CompiledEpoch,
    Seq<(string Row, string Sql)> ConnectionRows,
    Seq<(string Row, int Op, int Value)> DbConfig,
    Seq<(string Row, Func<SqliteConnection, Fin<Unit>> Grant)> Capabilities) {

    public static readonly EmbeddedRitual Canonical = new(
        Identity: 0x5241_5731, CompiledEpoch: 1,
        ConnectionRows: [
            ("<throughput>", "PRAGMA synchronous=NORMAL"), ("<wal-bound>", "PRAGMA journal_size_limit=8388608"),
            ("<spill>", "PRAGMA temp_store=MEMORY"), ("<budget>", "PRAGMA cache_size=-32768")],
        DbConfig: [
            ("<defensive>", raw.SQLITE_DBCONFIG_DEFENSIVE, 1), ("<dqs-ddl>", raw.SQLITE_DBCONFIG_DQS_DDL, 0),
            ("<dqs-dml>", raw.SQLITE_DBCONFIG_DQS_DML, 0)],
        Capabilities: [
            ("<stmt-registry>", SqliteStatHarvest.Arm),
            ("<uuid7>", static store => { store.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("N"), isDeterministic: false); return Fin.Succ(unit); }),
            ("<xxh128>", static store => { store.CreateFunction("xxh128", static (byte[] bytes) => {
                byte[] key = new byte[16];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(key, System.IO.Hashing.XxHash128.HashToUInt128(bytes));
                return key;
            }, isDeterministic: true); return Fin.Succ(unit); }),
            ("<instant-iso>", static store => { store.CreateCollation("instant_iso", static (left, right) => string.CompareOrdinal(left, right)); return Fin.Succ(unit); }),
            ("<span-fold>", static store => { store.CreateAggregate("span_fold", 0L, static (long held, long next) => long.Max(held, next), isDeterministic: true); return Fin.Succ(unit); })]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class EmbeddedStore {
    static EmbeddedStore() => Batteries_V2.Init();

    public static SqliteConnection Dialed(string path) => new(new SqliteConnectionStringBuilder {
        DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = false, ForeignKeys = true,
    }.ConnectionString);

    public static Fin<Seq<RitualStep>> Open(SqliteConnection store, EmbeddedRitual ritual, Option<ReadOnlyMemory<byte>> dek, Action<SqliteConnection, SqliteTransaction, long> materialize) =>
        HandleBridge.Opened(store, handle => {
            Fin<Unit> keyed = dek.TraverseM(key =>
                HandleBridge.Status(raw.sqlite3_key(handle, key.Span), "<key-refused>")).As().Map(static _ => unit);
            if (keyed.IsFail) { return keyed.Map(static _ => Seq<RitualStep>()); }
            return ritual.Capabilities.TraverseM(row =>
                row.Grant(store).Map(_ => new RitualStep(row.Row, None))).As().Bind(facts => {
                long identity = Scalar(store, "PRAGMA application_id");
                if (identity != ritual.Identity && identity != 0L) {
                    return Fin.Fail<Seq<RitualStep>>(new EmbeddedFault.Refused($"<foreign-store:{identity:x8}>"));
                }
                facts += ritual.ConnectionRows.Map(row => new RitualStep(row.Row, Some(Execute(store, row.Sql))));
                _ = raw.sqlite3_extended_result_codes(handle, 1);
                facts += ritual.DbConfig.Map(row => new RitualStep(row.Row,
                    raw.sqlite3_db_config(handle, row.Value, out int applied) == raw.SQLITE_OK ? Some((long)applied) : None));
                using SqliteTransaction gate = store.BeginTransaction(IsolationLevel.Serializable, deferred: false);
                long held = Scalar(store, "PRAGMA user_version", gate);
                if (held > ritual.CompiledEpoch) {
                    return Fin.Fail<Seq<RitualStep>>(new EmbeddedFault.Refused($"<epoch-ahead:{held}>"));
                }
                if (held < ritual.CompiledEpoch) {
                    materialize(store, gate, held);
                    _ = Execute(store, $"PRAGMA application_id={ritual.Identity}", gate);
                    _ = Execute(store, $"PRAGMA user_version={ritual.CompiledEpoch}", gate);
                }
                gate.Commit();
                return Fin.Succ(facts.Add(new RitualStep("<epoch>", Some(long.Max(held, ritual.CompiledEpoch)))));
            });
        });

    public static Fin<Unit> Rekey(SqliteConnection store, ReadOnlyMemory<byte> next) =>
        HandleBridge.Crossed(store, handle =>
            HandleBridge.Status(raw.sqlite3_rekey(handle, next.Span), "<rekey-refused>"));

    static long Execute(SqliteConnection store, string sql, SqliteTransaction? gate = null) { using SqliteCommand command = store.CreateCommand(); command.Transaction = gate; command.CommandText = sql; return command.ExecuteNonQuery(); }
    static long Scalar(SqliteConnection store, string sql, SqliteTransaction? gate = null) { using SqliteCommand command = store.CreateCommand(); command.Transaction = gate; command.CommandText = sql; return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture); }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                 | [BINDING]                                             |
| :-----: | :------------------- | :-------------------------------------- | :---------------------------------------------------- |
|  [01]   | open ritual          | one idempotent fold                     | bootstrap/recovery/steady-state are one path          |
|  [02]   | pragma residency     | per-connection rows only                | file-persistent rows are the materialization gate's   |
|  [03]   | hardening            | `sqlite3_db_config` defensive + DQS off | connection policy via `Handle`, not connection-string |
|  [04]   | capability scope     | connection-instance registration        | grants apply per physical open, never persisted       |
|  [05]   | materialization gate | first-opener IMMEDIATE transaction      | losers observe the bumped epoch; no leader election   |
|  [06]   | write transaction    | IMMEDIATE begin                         | a deferred-then-write burns the busy budget           |
|  [07]   | engine sweep         | closed (PostgreSQL + SQLite only)       | a new embedded engine row is the named defect         |
|  [08]   | sidecar unit         | `-wal`/`-shm` set                       | a main file without its sidecars is silent corruption |
|  [09]   | cipher floor         | `e_sqlite3mc` + KMS-unwrapped DEK       | `sqlite3_key` first crossing; `sqlite3_rekey` rotates |
|  [10]   | key custody          | `KMS_CUSTODY` envelope algebra          | plaintext zeroized after bind; never persisted        |
|  [11]   | lane admission       | `StoreProfile.Admits` + `Degrade`       | absence states at admission on BOTH engines           |

## [04]-[ENGINE_OPERATIONS]

- Owner: `HandleBridge` owns native-handle resolution, exception admission, status discrimination, and disposal. `CheckpointState` and `BackupState` carry provider-native operation output; `EngineOps` owns checkpoint, consistent snapshot, validated backup, blob IO, and integrity. `KvSpace`, `KvMount`, `KvEngine`, and `KvFloor` own embedded key-value storage.
- Cases: `CheckpointMode` is `Passive`/`Full`/`Restart`/`Truncate` (the `raw.SQLITE_CHECKPOINT_*` modes — `Truncate` the scheduled WAL-bound reset); `RetryShape` is `Terminal`/`Waited`/`Restarted`/`Rescoped` — the ROUTE axis naming WHERE a recovery re-enters — beside the kernel `Retriability` this band overrides to answer WHETHER a bare re-offer is admitted, which the route DERIVES: `Waited` alone reads transient, so a wait-retry executor never spins against a blocker only its own caller can release; `Reoffer` is the total dispatch over those four, taking the caller's same-effect, re-read, and narrowed arrows so the two routes a bool discarded reach their own re-entry point; `EmbeddedFault` is `Busy` (`SQLITE_BUSY`/`SQLITE_LOCKED`, its shape DERIVED from the full extended status the case already keeps rather than a second column), `Corrupt` (`SQLITE_CORRUPT`/`SQLITE_NOTADB`, terminal — routes to `Version/recovery`), `Io` (`SQLITE_IOERR`/`SQLITE_FULL`), `Refused` (a foreign store / epoch-ahead / pin regression), and `Kv` (the engine-named KV verdict carrying its shape as a column, because no engine status re-derives it); the integrity ladder orders boot `quick_check`, cycle `integrity_check` and `foreign_key_check`, a deeper-tier failure routing to restore, never retry; `KvSpace` is `Spool` (the pending `OpLogEntry`/`CrdtOp` rows a disconnected peer buffers), `Cursor` (both `SyncSession` watermarks — the pull resume point and the push-ack frontier), `ChunkIndex` (chunk key → owning `ContentAddress` dup set, the one row earning `DuplicatesSort|DuplicatesFixed`), and `Meta` (engine epoch and peer identity); `KvWrite` is `Put | Append | Unlink | Drop` — the dupsorted index answers two distinct retirements (one owner leaving a content address, the address leaving whole) so a single remove case can spell only the second, and `Append` is the accrual intent both engines own natively.
- Law: key order is proved by OMISSION — every `KvSpace` row declares `KvOrder.Bytewise` and NEITHER `DatabaseConfiguration.CompareWith` NOR `FindDuplicatesWith` is ever called, because LMDB's built-in comparator and RocksDB's default comparator are both byte-lexicographic and only an uncalled override leaves them in force; calling either is the deleted form, and `Scan` gates on the row's `PrefixSound` column so a future order lands a typed refusal instead of a silently truncated walk. LMDB's sync flags are ENVIRONMENT-scoped while RocksDB's `WriteOptions.SetSync` is per-write, so a per-space LMDB posture is inexpressible: the environment opens under the STRICTEST posture across the rostered spaces and each row's own column carries its LSM realization and its contribution to that floor.
- Entry: `Checkpoint(SqliteConnection, SnapshotFloor, CheckpointMode, ProjectionContext)` resets only the owning store's promoted pin on `Truncate`; `WithSnapshot<T>(SqliteConnection, SnapshotFloor, Func<SqliteConnection,T>)` promotes a comparable snapshot into that same disposable lifetime owner. `Backup(SqliteConnection, string, BackupPolicy, ProjectionContext)` binds the policy's `Dek` to the destination BEFORE `sqlite3_backup_init`, pages until completion, returns `Busy` without spinning, then requires destination `PRAGMA quick_check` and the policy's source/destination `ContentAddress` equality. `WriteBlob(SqliteConnection, BlobBinding, long, ReadOnlyMemory<byte>)` executes the binding's parameterized `zeroblob(@length)` row preallocation before opening `SqliteBlob`; `DataVersion` reads the cross-process change register. `KvFloor.Open(KvMount, ReadOnlyMemory<byte>)` folds the WHOLE `KvSpace` roster into the opened handle set and binds the vault, so every later space lookup is total; `Put`/`Get`/`Batch`/`Scan`/`Refs` each take the `KvSpace` whose row supplies their handle and posture, while `Since` and `Snap` take none because the WAL and the on-disk clone are store-wide facts spanning every space.
- Auto: each `SnapshotFloor` scopes native comparison and disposal to one store instead of comparing process-global handles from unrelated databases. Backup policy owns page quantum and semantic identity; `SQLITE_BUSY`/`SQLITE_LOCKED` returns to the schedule rather than hot-spinning inside the native loop. Blob target identifiers arrive only through a composition-time `BlobBinding`, while row id and length remain parameters. LMDB checks every `MDBResultCode`, maps only `NotFound` to `None`, admits a write only after `Commit` succeeds, and folds a RAISED `LightningException.StatusCode` through `EmbeddedFault.OfLmdb`, the same verdict fold a returned code takes, so one engine carries one taxonomy and a `MapFull` cannot read terminal when raised and recoverable when returned; the status discriminator masks the primary byte because the ritual arms extended result codes; RocksDB keeps span-first IO and atomic `WriteBatch`, while its exception remains exact because the managed surface publishes no stable typed recovery discriminant. Accrual is row data: an `Append` on a merge-carrying row is one `WriteBatch.Merge` the engine resolves at read and compaction, and the same intent on the mmap arm is a dup put into that key's set, so a disconnected peer never pays a read-modify-write; the value seal likewise reads its row, so one `Put` path serves a sealed and a clear space.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`, `SqliteBlob(connection, table, column, rowid, readOnly)`, `BackupDatabase`, `SqliteException.SqliteErrorCode`/`SqliteExtendedErrorCode`), SQLitePCLRaw.bundle_e_sqlite3mc (`raw.sqlite3_wal_checkpoint_v2`, `raw.sqlite3_snapshot_get`/`_open`/`_cmp`/`_recover`/`_free`, `raw.sqlite3_backup_init`/`_step`/`_remaining`/`_pagecount`/`_finish`, `raw.sqlite3_extended_errcode`, `raw.sqlite3_errstr`, the `SQLITE_CHECKPOINT_*`/`SQLITE_BUSY`/`SQLITE_BUSY_RECOVERY`/`SQLITE_BUSY_SNAPSHOT`/`SQLITE_LOCKED`/`SQLITE_CORRUPT`/`SQLITE_DONE` constants), rocksdb (`RocksDb.Open(DbOptions, path, ColumnFamilies)`, `GetColumnFamily`, `ColumnFamilies.Add`/`DefaultName`, `ColumnFamilyOptions.SetCompactionStyle`/`SetMergeOperator`, `MergeOperators.Create` with `PartialMergeFunc`/`FullMergeFunc`/`OperandsEnumerator`, `WriteOptions.SetSync`, the `ColumnFamilyHandle`-taking `Get`/`Put`/`Merge`/`NewIterator` and `WriteBatch.Put`/`Merge`/`Delete`, `RocksDb.Write(WriteBatch, WriteOptions)`, `CreateSnapshot`, `ReadOptions.SetSnapshot`, `GetUpdatesSince`, `TransactionLogIterator.GetBatch`, `WriteBatch.ToBytes`, `Checkpoint.Save`, `RocksDbException`), LightningDB (`LightningEnvironment` + `EnvironmentConfiguration.MapSize`/`MaxDatabases`, `Open(EnvironmentOpenFlags, UnixAccessMode)`, `BeginTransaction`, `LightningTransaction.OpenDatabase(name, DatabaseConfiguration, closeOnDispose)`/`Get`/`Put`/`Delete(db)`/`Delete(db, value)` — the dup-value overload — `/Commit`, `LightningCursor.GetBoth`/`GetMultiple`/`NextMultiple`/`AllValuesFor`/`SetRange`, `LightningException.StatusCode`, `MDBResultCode`, `DatabaseOpenFlags`, `EnvironmentOpenFlags`), System.Security.Cryptography (`AesGcm(tagSizeInBytes)`/`Encrypt`/`Decrypt`/`NonceByteSizes`/`TagByteSizes`, `AuthenticationTagMismatchException`, `RandomNumberGenerator.Fill`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new checkpoint mode is one `CheckpointMode` row; a new boundary cause is one `EmbeddedFault` case; a new KV write intent is one `KvWrite` case; a new integrity tier is one ladder row; a new keyspace is one `KvSpace` row landing its compaction style, accrual, dup width, order, durability, and seal together, so nothing about it is decided at a call site; a new re-drive route is one `RetryShape` row every fault family reads and one arm on the `Reoffer` dispatch; a future RocksDB typed status enters the existing provider fold only after the managed API exposes it; zero new surface — the whole-file `BackupDatabase` where the paged session adds progress facts, a whole-payload `byte[]` blob materialization, a second hashing path beside the registered `xxh128` UDF, an exception flattening the engine status, a snapshot regression unguarded by `sqlite3_snapshot_cmp`, a per-engine KV service class, a composite-key prefix standing in for a keyspace the engine partitions natively, a `bool Transient` beside the re-drive shape, a separator-joined operand concatenation, a caller-side operand re-fold, a declared key comparer, or a plaintext spool value is the deleted form.
- Boundary: `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) is the one bridge joining the managed ADO surface to raw operations, and the bound `e_sqlite3mc` provider keeps raw calls and ADO statements on the same native connection; every native crossing rides inside `HandleBridge` so the cause stays a closed `EmbeddedFault` case; the WAL sidecar set is the unit of backup, snapshot pins and truncating checkpoints remain adversaries, integrity failures route to `Version/recovery`, and blob IO streams through `SqliteBlob` without whole-payload materialization; the backup destination is a SECOND physical store the paged session fills page-for-page, so it binds the same cipher key as its source and an unkeyed destination under the bound `e_sqlite3mc` floor is the plaintext egress the offline-lane classification ceiling forbids; the KV floor holds that same ceiling by a different mechanism because neither engine ships a cipher — the seal rides the VALUE bytes under the SAME KMS-unwrapped DEK custody the SQLite floor uses, and the `Degrade` it leaves in the clear is exact: every KEY byte (a key is a content digest already, and sealing it destroys the byte-lexicographic order every prefix stop and `SetRange` walk reads), the `ChunkIndex` dup values (an LMDB dup value IS a key in the dup sub-B+tree, so `GetBoth` seeks it, `Unlink` deletes by its exact bytes, and `DuplicatesSort` orders on it), the LMDB page metadata, and the RocksDB SST block boundaries and per-value LENGTHS the frame width leaks; `Get` REFUSES on an accruing row and names `Refs`, because the engine already resolved the operand chain on its own read and handing back that resolved frame pushes the framing onto every caller; the re-drive owner for every embedded fault is the CALLER's in-process effect path (`docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` row `[03]`), so `Reoffer` routes and executes here and a pipeline wrapped around embedded store work is the deleted form, replaying from the wrong boundary; `RocksDbException` carries no stable typed status, so its captured exceptional `Error` crosses unchanged and its message never drives recovery.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using LightningDB;
using Microsoft.Data.Sqlite;
using NodaTime;
using Rasm.Persistence.Element;
using RocksDbSharp;
using SQLitePCL;
using static LanguageExt.Prelude;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<int>]
public sealed partial class CheckpointMode {
    public static readonly CheckpointMode Passive = new(raw.SQLITE_CHECKPOINT_PASSIVE);
    public static readonly CheckpointMode Full = new(raw.SQLITE_CHECKPOINT_FULL);
    public static readonly CheckpointMode Restart = new(raw.SQLITE_CHECKPOINT_RESTART);
    public static readonly CheckpointMode Truncate = new(raw.SQLITE_CHECKPOINT_TRUNCATE);
}

// --- [KV_KEYSPACE]

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvOrder {
    public static readonly KvOrder Bytewise = new("bytewise", prefixSound: true);
    public bool PrefixSound { get; }
    private KvOrder(string key, bool prefixSound) : this(key) => PrefixSound = prefixSound;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvDurability {
    public static readonly KvDurability Buffered = new("buffered", EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoMetaSync, rank: 0);
    public static readonly KvDurability Synced = new("synced", EnvironmentOpenFlags.None, rank: 1);
    public EnvironmentOpenFlags Relaxed { get; }
    public int Rank { get; }
    public WriteOptions Writes { get; }
    private KvDurability(string key, EnvironmentOpenFlags relaxed, int rank) : this(key) =>
        (Relaxed, Rank, Writes) = (relaxed, rank, new WriteOptions().SetSync(rank > 0));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvSeal {
    public static readonly KvSeal Ordered = new("ordered", seals: false);
    public static readonly KvSeal Sealed = new("sealed", seals: true);
    public bool Seals { get; }
    private KvSeal(string key, bool seals) : this(key) => Seals = seals;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvLayout {
    private KvLayout() { }
    public sealed record Single : KvLayout;
    public sealed record Accrued(MergeOperator Operator) : KvLayout;
    public sealed record Fanned(int Width) : KvLayout;

    public DatabaseOpenFlags Flags => Switch(
        single:  static _ => DatabaseOpenFlags.Create,
        accrued: static _ => DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort,
        fanned:  static _ => DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.DuplicatesFixed);
    public Option<MergeOperator> Operator => this is Accrued accrued ? Some(accrued.Operator) : None;
    public Option<int> Width => this is Fanned fanned ? Some(fanned.Width) : None;
}

// --- [SPOOL_ACCRUAL]

public static class SpoolAccrual {
    const int Width = sizeof(int);
    public static readonly MergeOperator Operator = MergeOperators.Create("rasm-spool-accrual", Partial, Full);

    public static ReadOnlyMemory<byte> Frame(ReadOnlyMemory<byte> member) {
        byte[] frame = new byte[(Width * 2) + member.Length];
        BinaryPrimitives.WriteInt32BigEndian(frame, 1);
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(Width), member.Length);
        member.Span.CopyTo(frame.AsSpan(Width * 2));
        return frame;
    }

    public static Fin<Seq<ReadOnlyMemory<byte>>> Members(ReadOnlyMemory<byte> frame) {
        if (frame.Length < Width) { return Fin.Fail<Seq<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("rocksdb", "<frame-short>", "<accrual>", RetryShape.Terminal)); }
        (int At, Seq<ReadOnlyMemory<byte>> Members, bool Torn) walk = Enumerable.Range(0, BinaryPrimitives.ReadInt32BigEndian(frame.Span)).Aggregate(
            (At: Width, Members: Seq<ReadOnlyMemory<byte>>(), Torn: false),
            (acc, _) => acc.Torn || acc.At + Width > frame.Length
                ? (acc.At, acc.Members, true)
                : BinaryPrimitives.ReadInt32BigEndian(frame.Span[acc.At..]) is int held && acc.At + Width + held <= frame.Length
                    ? (acc.At + Width + held, acc.Members.Add(frame.Slice(acc.At + Width, held)), false)
                    : (acc.At, acc.Members, true));
        return walk.Torn
            ? Fin.Fail<Seq<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("rocksdb", "<frame-torn>", "<accrual>", RetryShape.Terminal))
            : Fin.Succ(walk.Members);
    }

    static byte[] Full(ReadOnlySpan<byte> key, bool hasExistingValue, ReadOnlySpan<byte> existingValue, MergeOperators.OperandsEnumerator operands, out bool success) =>
        Concat(hasExistingValue ? existingValue : [], operands, out success);

    static byte[] Partial(ReadOnlySpan<byte> key, MergeOperators.OperandsEnumerator operands, out bool success) =>
        Concat([], operands, out success);

    static byte[] Concat(ReadOnlySpan<byte> held, MergeOperators.OperandsEnumerator operands, out bool success) {
        int count = held.Length >= Width ? BinaryPrimitives.ReadInt32BigEndian(held) : 0;
        int bytes = Math.Max(held.Length - Width, 0);
        for (int index = 0; index < operands.Count; index++) {
            ReadOnlySpan<byte> operand = operands.Get(index);
            if (operand.Length < Width) { success = false; return []; }
            count += BinaryPrimitives.ReadInt32BigEndian(operand);
            bytes += operand.Length - Width;
        }
        byte[] merged = new byte[Width + bytes];
        BinaryPrimitives.WriteInt32BigEndian(merged, count);
        int at = Width;
        if (held.Length > Width) { held[Width..].CopyTo(merged.AsSpan(at)); at += held.Length - Width; }
        for (int index = 0; index < operands.Count; index++) {
            ReadOnlySpan<byte> operand = operands.Get(index);
            operand[Width..].CopyTo(merged.AsSpan(at));
            at += operand.Length - Width;
        }
        success = true;
        return merged;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvSpace {
    public static readonly KvSpace Spool = new("spool", new KvLayout.Accrued(SpoolAccrual.Operator), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Universal);
    public static readonly KvSpace Cursor = new("cursor", new KvLayout.Single(), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Level);
    public static readonly KvSpace ChunkIndex = new("chunk-index", new KvLayout.Fanned(Unsafe.SizeOf<UInt128>()), KvOrder.Bytewise, KvDurability.Buffered, KvSeal.Ordered, Compaction.Level);
    public static readonly KvSpace Meta = new("meta", new KvLayout.Single(), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Level);

    public KvLayout Layout { get; }
    public KvOrder Order { get; }
    public KvDurability Durability { get; }
    public KvSeal Seal { get; }
    public ColumnFamilyOptions Family { get; }
    public DatabaseConfiguration Database { get; }
    private KvSpace(string key, KvLayout layout, KvOrder order, KvDurability durability, KvSeal seal, Compaction compaction) : this() {
        (Layout, Order, Durability, Seal) = (layout, order, durability, seal);
        Family = layout.Operator.Match(
            Some: merge => new ColumnFamilyOptions().SetCompactionStyle(compaction).SetMergeOperator(merge),
            None: () => new ColumnFamilyOptions().SetCompactionStyle(compaction));
        Database = new DatabaseConfiguration { Flags = layout.Flags };
    }

    public static EnvironmentOpenFlags SyncFloor =>
        toSeq(Items).Fold(KvDurability.Buffered, static (strictest, row) => row.Durability.Rank > strictest.Rank ? row.Durability : strictest).Relaxed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvMount {
    private KvMount() { }
    public sealed record Lsm(string Path) : KvMount;
    public sealed record Mmap(string Path, long MapSize) : KvMount;
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct CheckpointState(int LogFrames, int Checkpointed, bool Busy, Instant At);

public readonly record struct BackupState(int Pages, int Remaining, Instant At);

public sealed class SnapshotFloor : IDisposable {
    internal Lock Gate { get; } = new();
    internal sqlite3_snapshot? Held { get; set; }

    public void Dispose() {
        lock (Gate) {
            if (Held is { } held) { raw.sqlite3_snapshot_free(held); }
            Held = null;
        }
    }
}

public sealed record BackupPolicy(
    int PageStep,
    Option<ReadOnlyMemory<byte>> Dek,
    Func<SqliteConnection, Fin<ContentAddress>> Identity);

public readonly record struct BlobBinding(string Table, string Column, string PreallocateSql);

public sealed class KvVault : IDisposable {
    static readonly int NonceWidth = AesGcm.NonceByteSizes.MaxSize;
    static readonly int TagWidth = AesGcm.TagByteSizes.MaxSize;
    readonly AesGcm cipher;

    public KvVault(ReadOnlySpan<byte> dek) => cipher = new AesGcm(dek, TagWidth);
    public void Dispose() => cipher.Dispose();

    public ReadOnlyMemory<byte> Wrap(KvSpace space, ReadOnlySpan<byte> key, ReadOnlyMemory<byte> value) {
        if (!space.Seal.Seals) { return value; }
        byte[] frame = new byte[NonceWidth + TagWidth + value.Length];
        RandomNumberGenerator.Fill(frame.AsSpan(0, NonceWidth));
        cipher.Encrypt(frame.AsSpan(0, NonceWidth), value.Span, frame.AsSpan(NonceWidth + TagWidth), frame.AsSpan(NonceWidth, TagWidth), Aad(space, key));
        return frame;
    }

    public Fin<ReadOnlyMemory<byte>> Unwrap(KvSpace space, ReadOnlySpan<byte> key, ReadOnlyMemory<byte> frame) {
        if (!space.Seal.Seals) { return Fin.Succ(frame); }
        if (frame.Length < NonceWidth + TagWidth) { return Fin.Fail<ReadOnlyMemory<byte>>(new EmbeddedFault.Kv("seal", "<frame-short>", space.Key, RetryShape.Terminal)); }
        byte[] value = new byte[frame.Length - NonceWidth - TagWidth];
        return Try.lift(() => {
            cipher.Decrypt(frame.Span[..NonceWidth], frame.Span[(NonceWidth + TagWidth)..], frame.Span.Slice(NonceWidth, TagWidth), value, Aad(space, key));
            return Fin.Succ((ReadOnlyMemory<byte>)value);
        }).Run().Bind(static inner => inner);
    }

    static byte[] Aad(KvSpace space, ReadOnlySpan<byte> key) {
        int name = Encoding.UTF8.GetByteCount(space.Key);
        byte[] aad = new byte[(sizeof(int) * 2) + name + key.Length];
        BinaryPrimitives.WriteInt32BigEndian(aad, name);
        Encoding.UTF8.GetBytes(space.Key, aad.AsSpan(sizeof(int)));
        BinaryPrimitives.WriteInt32BigEndian(aad.AsSpan(sizeof(int) + name), key.Length);
        key.CopyTo(aad.AsSpan((sizeof(int) * 2) + name));
        return aad;
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetryShape {
    public static readonly RetryShape Terminal = new("terminal");
    public static readonly RetryShape Waited = new("waited");
    public static readonly RetryShape Restarted = new("restarted");
    public static readonly RetryShape Rescoped = new("rescoped");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EmbeddedFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Embedded;
    private EmbeddedFault() { }
    [FaultCase(0)]
    public sealed partial record Busy(int Status) : EmbeddedFault() {
        public override RetryShape Route => BusyRoute(Status);
    }
    [FaultCase(1)]
    public sealed partial record Corrupt(int Status, string Detail) : EmbeddedFault();
    [FaultCase(2)]
    public sealed partial record Io(int Status, string Detail) : EmbeddedFault();
    [FaultCase(3)]
    public sealed partial record Refused(string Detail) : EmbeddedFault();
    [FaultCase(4)]
    public sealed partial record Kv(string Engine, string Status, string Detail, RetryShape Shape) : EmbeddedFault() {
        public override RetryShape Route => Shape;
    }
    [FaultCase(5)]
    public sealed partial record ProviderBusy(int Status, Error Cause) : EmbeddedFault(), ICausedFault {
        public override RetryShape Route => BusyRoute(Status);
    }
    [FaultCase(6)] public sealed partial record ProviderCorrupt(int Status, string Detail, Error Cause) : EmbeddedFault(), ICausedFault;
    [FaultCase(7)] public sealed partial record ProviderIo(int Status, string Detail, Error Cause) : EmbeddedFault(), ICausedFault;
    [FaultCase(8)] public sealed partial record ProviderRefused(string Detail, Error Cause) : EmbeddedFault(), ICausedFault;
    [FaultCase(9)]
    public sealed partial record ProviderKv(string Engine, string Status, string Detail, RetryShape Shape, Error Cause) : EmbeddedFault(), ICausedFault {
        public override RetryShape Route => Shape;
    }

    public override string Message => Switch(
        busy:            static c => $"<busy:{c.Status}>",
        corrupt:         static c => $"<corrupt:{c.Status}>:{c.Detail}",
        io:              static c => $"<io:{c.Status}>:{c.Detail}",
        refused:         static c => $"<refused:{c.Detail}>",
        kv:              static c => $"<kv:{c.Engine}:{c.Status}>:{c.Detail}",
        providerBusy:    static c => $"<busy:{c.Status}>:{c.Cause.Message}",
        providerCorrupt: static c => $"<corrupt:{c.Status}>:{c.Detail}:{c.Cause.Message}",
        providerIo:      static c => $"<io:{c.Status}>:{c.Detail}:{c.Cause.Message}",
        providerRefused: static c => $"<refused:{c.Detail}>:{c.Cause.Message}",
        providerKv:      static c => $"<kv:{c.Engine}:{c.Status}>:{c.Detail}:{c.Cause.Message}");

    public virtual RetryShape Route => RetryShape.Terminal;
    static RetryShape BusyRoute(int status) => status == raw.SQLITE_BUSY_SNAPSHOT ? RetryShape.Restarted : RetryShape.Waited;

    public override Retriability Retriability =>
        Route == RetryShape.Waited ? Retriability.Transient : Retriability.Terminal;

    public IO<T> Reoffer<T>(Func<IO<T>> same, Func<IO<T>> reread, Func<IO<T>> narrowed) => Route.Switch(
        state: (Fault: this, Same: same, Reread: reread, Narrowed: narrowed),
        terminal:  static (re, _) => IO.fail<T>(re.Fault),
        waited:    static (re, _) => re.Same(),
        restarted: static (re, _) => re.Reread(),
        rescoped:  static (re, _) => re.Narrowed());

    public static Error Lift(Error error) => error.Exception.Match(
        Some: ex => ex switch {
            SqliteException sql => WithCause(FromStatus(sql.SqliteExtendedErrorCode, sql.Message), error),
            LightningException native => WithCause(
                OfLmdb((MDBResultCode)native.StatusCode)
                    .IfNone(() => new Kv("lmdb", native.StatusCode.ToString(CultureInfo.InvariantCulture), native.Message, RetryShape.Terminal)),
                error),
            RocksDbException => error,
            _ => error,
        },
        None: () => error);

    static Error WithCause(EmbeddedFault classified, Error cause) => classified switch {
        Busy c    => new ProviderBusy(c.Status, cause),
        Corrupt c => new ProviderCorrupt(c.Status, c.Detail, cause),
        Io c      => new ProviderIo(c.Status, c.Detail, cause),
        Refused c => new ProviderRefused(c.Detail, cause),
        Kv c      => new ProviderKv(c.Engine, c.Status, c.Detail, c.Shape, cause),
        _         => cause,
    };
    public static EmbeddedFault FromStatus(int status, string detail) => (status & 0xFF) switch {
        raw.SQLITE_BUSY or raw.SQLITE_LOCKED => new Busy(status),
        raw.SQLITE_CORRUPT or raw.SQLITE_NOTADB => new Corrupt(status, detail),
        raw.SQLITE_IOERR or raw.SQLITE_FULL or raw.SQLITE_READONLY => new Io(status, detail),
        _ => new Refused(detail),
    };

    public static Option<EmbeddedFault> OfLmdb(MDBResultCode status) => status switch {
        MDBResultCode.Success => None,
        MDBResultCode.MapResized => Some<EmbeddedFault>(new Kv("lmdb", status.ToString(), "<remapped>", RetryShape.Waited)),
        MDBResultCode.ReadersFull or MDBResultCode.TLSFull => Some<EmbeddedFault>(new Kv("lmdb", status.ToString(), "<slots>", RetryShape.Waited)),
        MDBResultCode.MapFull or MDBResultCode.DbsFull or MDBResultCode.TxnFull or MDBResultCode.CursorFull or MDBResultCode.PageFull =>
            Some<EmbeddedFault>(new Kv("lmdb", status.ToString(), "<ceiling>", RetryShape.Rescoped)),
        MDBResultCode.Corrupted or MDBResultCode.Panic or MDBResultCode.PageNotFound or MDBResultCode.VersionMismatch
            or MDBResultCode.Invalid or MDBResultCode.InvalidData =>
            Some<EmbeddedFault>(new Corrupt((int)status, $"<lmdb:{status}>")),
        _ => Some<EmbeddedFault>(new Kv("lmdb", status.ToString(), "<write>", RetryShape.Terminal)),
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class HandleBridge {
    public static Fin<sqlite3> Of(SqliteConnection store) =>
        store.Handle is { } handle ? Fin.Succ(handle) : Fin.Fail<sqlite3>(new EmbeddedFault.Refused("<no-handle>"));

    public static Fin<T> Lifted<T>(Func<T> crossing) =>
        Try.lift(() => crossing()).Run().MapFail(static error => EmbeddedFault.Lift(error));

    public static Fin<T> Crossed<T>(SqliteConnection store, Func<sqlite3, Fin<T>> crossing) =>
        Of(store).Bind(handle => Lifted(() => crossing(handle)).Bind(static held => held));

    public static Fin<T> Opened<T>(SqliteConnection store, Func<sqlite3, Fin<T>> body) =>
        Lifted(fun(store.Open))
            .Bind(_ => Crossed(store, body))
            .BindFail(fault => {
                store.Dispose();
                return Fin<T>.Fail(fault);
            });

    public static Fin<T> Status<T>(int status, string detail, Func<T> value) =>
        status == raw.SQLITE_OK ? Fin.Succ(value()) : Fin.Fail<T>(EmbeddedFault.FromStatus(status, detail));
    public static Fin<Unit> Status(int status, string detail) => Status(status, detail, static () => unit);

    public static string Explain(int status) => raw.sqlite3_errstr(status).utf8_to_string();
}

public static class EngineOps {
    public static Fin<CheckpointState> Checkpoint(SqliteConnection store, SnapshotFloor floor, CheckpointMode mode, ProjectionContext frame) =>
        HandleBridge.Crossed(store, handle => {
            int status = raw.sqlite3_wal_checkpoint_v2(handle, "main", mode.Key, out int logFrames, out int checkpointed);
            if (status == raw.SQLITE_OK && mode == CheckpointMode.Truncate) { floor.Dispose(); }
            return status == raw.SQLITE_BUSY
                ? Fin.Succ(new CheckpointState(logFrames, checkpointed, Busy: true, At: frame.Now()))
                : HandleBridge.Status(status, HandleBridge.Explain(status),
                    () => new CheckpointState(logFrames, checkpointed, Busy: false, At: frame.Now()));
        });

    public static Fin<T> WithSnapshot<T>(SqliteConnection store, SnapshotFloor floor, Func<SqliteConnection, T> read) =>
        HandleBridge.Crossed(store, handle => {
            int got;
            sqlite3_snapshot snapshot;
            using (SqliteTransaction pin = store.BeginTransaction(IsolationLevel.Serializable, deferred: true)) {
                got = raw.sqlite3_snapshot_get(handle, "main", out snapshot);
                if (got != raw.SQLITE_OK) {
                    int recovered = raw.sqlite3_snapshot_recover(handle, "main");
                    got = recovered == raw.SQLITE_OK ? raw.sqlite3_snapshot_get(handle, "main", out snapshot) : recovered;
                }
                if (got != raw.SQLITE_OK) { return HandleBridge.Status(got, "<snapshot-unavailable>").Map(static _ => default(T)!); }
            }
            bool promoted = false;
            using SqliteTransaction view = store.BeginTransaction(IsolationLevel.Serializable, deferred: true);
            try {
                Fin<Unit> opened = HandleBridge.Status(raw.sqlite3_snapshot_open(handle, "main", snapshot), "<snapshot-open>");
                if (opened.IsFail) { return opened.Map(static _ => default(T)!); }
                lock (floor.Gate) {
                    if (floor.Held is { } held && raw.sqlite3_snapshot_cmp(snapshot, held) < 0) {
                        return Fin.Fail<T>(new EmbeddedFault.Refused("<snapshot-regression>"));
                    }
                    if (floor.Held is { } prior) { raw.sqlite3_snapshot_free(prior); }
                    (floor.Held, promoted) = (snapshot, true);
                }
                return Fin.Succ(read(store));
            }
            finally { if (!promoted) { raw.sqlite3_snapshot_free(snapshot); } }
        });

    public static IO<Fin<BackupState>> Backup(SqliteConnection source, string destinationPath, BackupPolicy policy, ProjectionContext frame) =>
        IO.lift<Fin<BackupState>>(() =>
            from expected in policy.Identity(source)
            from sourceHandle in HandleBridge.Of(source)
            from fact in Paged(EmbeddedStore.Dialed(destinationPath), sourceHandle, expected, policy, frame)
            select fact);

    static Fin<BackupState> Paged(SqliteConnection destination, sqlite3 source, ContentAddress expected, BackupPolicy policy, ProjectionContext frame) =>
        HandleBridge.Opened(destination, handle =>
            from _keyed in policy.Dek.TraverseM(key =>
                HandleBridge.Status(raw.sqlite3_key(handle, key.Span), "<backup-key-refused>")).As()
            from fact in Stepped(handle, source, expected, destination, policy, frame)
            select fact);

    static Fin<BackupState> Stepped(sqlite3 destination, sqlite3 source, ContentAddress expected, SqliteConnection sink, BackupPolicy policy, ProjectionContext frame) {
        sqlite3_backup backup = raw.sqlite3_backup_init(destination, "main", source, "main");
        try {
            int step;
            do { step = raw.sqlite3_backup_step(backup, policy.PageStep); }
            while (step == raw.SQLITE_OK);
            return step != raw.SQLITE_DONE
                ? HandleBridge.Status(step, HandleBridge.Explain(step)).Map(static _ => default(BackupState))
                : from _integrity in QuickCheck(sink)
                  from observed in policy.Identity(sink)
                  from proved in observed == expected
                      ? Fin.Succ(new BackupState(raw.sqlite3_backup_pagecount(backup), raw.sqlite3_backup_remaining(backup), frame.Now()))
                      : Fin.Fail<BackupState>(new EmbeddedFault.Corrupt(raw.SQLITE_CORRUPT, "<backup-identity>"))
                  select proved;
        }
        finally { _ = raw.sqlite3_backup_finish(backup); }
    }

    public static IO<Fin<long>> WriteBlob(SqliteConnection store, BlobBinding binding, long rowid, ReadOnlyMemory<byte> payload) =>
        IO.lift<Fin<long>>(() => HandleBridge.Lifted(() => {
            using SqliteCommand command = store.CreateCommand();
            command.CommandText = binding.PreallocateSql;
            command.Parameters.Add(new SqliteParameter("rowid", SqliteType.Integer) { Value = rowid });
            command.Parameters.Add(new SqliteParameter("length", SqliteType.Integer) { Value = payload.Length });
            if (command.ExecuteNonQuery() != 1) { return Fin.Fail<long>(new EmbeddedFault.Refused($"<blob-row-absent:{rowid}>")); }
            using SqliteBlob blob = new(store, binding.Table, binding.Column, rowid, readOnly: false);
            blob.Write(payload.Span);
            return Fin.Succ((long)payload.Length);
        }).Bind(static held => held));

    public static Fin<long> DataVersion(SqliteConnection store) =>
        HandleBridge.Lifted(() => Scalar(store, "PRAGMA data_version")).Map(static value => Convert.ToInt64(value, CultureInfo.InvariantCulture));

    static Fin<Unit> QuickCheck(SqliteConnection store) =>
        HandleBridge.Lifted(() => Scalar(store, "PRAGMA quick_check"))
            .Bind(static held => string.Equals(Convert.ToString(held, CultureInfo.InvariantCulture), "ok", StringComparison.Ordinal)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new EmbeddedFault.Corrupt(raw.SQLITE_CORRUPT, "<quick-check>")));

    static object? Scalar(SqliteConnection store, string sql) {
        using SqliteCommand command = store.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvEngine {
    private KvEngine() { }
    public sealed record Lsm(RocksDb Store, FrozenDictionary<KvSpace, ColumnFamilyHandle> Spaces, KvVault Keys) : KvEngine;
    public sealed record Mmap(LightningEnvironment Store, FrozenDictionary<KvSpace, LightningDatabase> Spaces, KvVault Keys) : KvEngine;
    public KvVault Vault => Switch(lsm: static l => l.Keys, mmap: static m => m.Keys);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvWrite {
    private KvWrite() { }
    public sealed record Put(ReadOnlyMemory<byte> Value) : KvWrite;
    public sealed record Append(ReadOnlyMemory<byte> Operand) : KvWrite;
    public sealed record Unlink(ReadOnlyMemory<byte> Owner) : KvWrite;
    public sealed record Drop : KvWrite;
}

public static class KvFloor {
    public static Fin<KvEngine> Open(KvMount mount, ReadOnlyMemory<byte> dek) => mount.Switch(
        state: dek,
        lsm: static (key, m) => Guarded(() => {
            ColumnFamilies families = new();
            toSeq(KvSpace.Items).Iter(row => families.Add(row.Key, row.Family));
            RocksDb store = RocksDb.Open(new DbOptions().SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true), m.Path, families);
            return (KvEngine)new KvEngine.Lsm(store, toSeq(KvSpace.Items).ToFrozenDictionary(static row => row, row => store.GetColumnFamily(row.Key)), new KvVault(key.Span));
        }),
        mmap: static (key, m) => Guarded(() => {
            LightningEnvironment store = new(m.Path, new EnvironmentConfiguration { MapSize = m.MapSize, MaxDatabases = KvSpace.Items.Count });
            store.Open(KvSpace.SyncFloor);
            using LightningTransaction opening = store.BeginTransaction();
            FrozenDictionary<KvSpace, LightningDatabase> spaces = toSeq(KvSpace.Items).ToFrozenDictionary(static row => row, row => opening.OpenDatabase(row.Key, row.Database, closeOnDispose: false));
            return Mdb(opening.Commit()).Map(_ => (KvEngine)new KvEngine.Mmap(store, spaces, new KvVault(key.Span)));
        }).Bind(static opened => opened));

    public static Fin<Unit> Put(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value) => engine.Switch(
        state: (Space: space, Value: value),
        lsm: static (s, l) => Guarded(() => {
            l.Store.Put(s.Key.Span, l.Keys.Wrap(s.Space, s.Key.Span, s.Value).Span, Cf(l, s.Space), s.Space.Durability.Writes);
            return unit;
        }),
        mmap: static (s, m) => Guarded(() => {
            using LightningTransaction transaction = m.Store.BeginTransaction();
            MDBResultCode write = transaction.Put(Db(m, s.Space), s.Key.Span, m.Keys.Wrap(s.Space, s.Key.Span, s.Value).Span);
            return write == MDBResultCode.Success ? transaction.Commit() : write;
        }).Bind(Mdb));

    public static Fin<Option<ReadOnlyMemory<byte>>> Get(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key) =>
        space.Layout is not KvLayout.Single
            ? Fin.Fail<Option<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("kv", "<accrued-point-read>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: space,
                lsm: static (s, l) => Guarded(() => Optional(l.Store.Get(s.Key.Span, Cf(l, s))))
                    .Bind(held => Opened(l.Keys, s, s.Key, held)),
                mmap: static (s, m) => Guarded(() => {
                    using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
                    (MDBResultCode code, _, MDBValue value) = transaction.Get(Db(m, s), s.Key.Span);
                    return code switch {
                        MDBResultCode.Success => m.Keys.Unwrap(s, s.Key.Span, value.CopyToNewArray()).Map(static opened => Some(opened)),
                        MDBResultCode.NotFound => Fin.Succ<Option<ReadOnlyMemory<byte>>>(None),
                        _ => Mdb(code).Map(static _ => Option<ReadOnlyMemory<byte>>.None),
                    };
                }).Bind(static result => result));

    public static Fin<Unit> Batch(KvEngine engine, KvSpace space, Seq<(ReadOnlyMemory<byte> Key, KvWrite Write)> writes) =>
        writes.Exists(row => row.Write is KvWrite.Append && space.Layout is KvLayout.Single)
            ? Fin.Fail<Unit>(new EmbeddedFault.Kv("kv", "<append-unaccrued>", space.Key, RetryShape.Terminal))
            : writes.Exists(row => row.Write is KvWrite.Unlink && space.Seal.Seals)
                ? Fin.Fail<Unit>(new EmbeddedFault.Kv("kv", "<unlink-sealed>", space.Key, RetryShape.Terminal))
                : engine.Switch(
                    state: (Space: space, Rows: writes),
                    lsm: static (s, l) => Guarded(() => {
                        using WriteBatch batch = new();
                        ColumnFamilyHandle family = Cf(l, s.Space);
                        s.Rows.Iter(row => row.Write.Switch(
                            state: (Batch: batch, Family: family, Space: s.Space, Vault: l.Keys, Key: row.Key),
                            put:    static (b, w) => b.Batch.Put(b.Key.Span, b.Vault.Wrap(b.Space, b.Key.Span, w.Value).Span, b.Family),
                            append: static (b, w) => b.Batch.Merge(b.Key.Span, SpoolAccrual.Frame(b.Vault.Wrap(b.Space, b.Key.Span, w.Operand)).Span, b.Family),
                            unlink: static (b, _) => b.Batch.Delete(b.Key.Span, b.Family),
                            drop:   static (b, _) => b.Batch.Delete(b.Key.Span, b.Family)));
                        l.Store.Write(batch, s.Space.Durability.Writes);
                        return unit;
                    }),
                    mmap: static (s, m) => Guarded(() => {
                        using LightningTransaction transaction = m.Store.BeginTransaction();
                        LightningDatabase db = Db(m, s.Space);
                        Seq<MDBResultCode> statuses = s.Rows.Map(row => row.Write.Switch(
                            state: (Txn: transaction, Db: db, Space: s.Space, Vault: m.Keys, Key: row.Key),
                            put:    static (t, w) => t.Txn.Put(t.Db, t.Key.Span, t.Vault.Wrap(t.Space, t.Key.Span, w.Value).Span),
                            append: static (t, w) => t.Txn.Put(t.Db, t.Key.Span, t.Vault.Wrap(t.Space, t.Key.Span, w.Operand).Span),
                            unlink: static (t, w) => t.Txn.Delete(t.Db, t.Key.Span, w.Owner.Span),
                            drop:   static (t, _) => t.Txn.Delete(t.Db, t.Key.Span)));
                        Option<MDBResultCode> refused = statuses.Find(static status => status != MDBResultCode.Success && status != MDBResultCode.NotFound);
                        return refused.IfNone(transaction.Commit);
                    }).Bind(Mdb));

    public static Fin<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>> Scan(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> prefix) =>
        !space.Order.PrefixSound
            ? Fin.Fail<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>>(new EmbeddedFault.Kv("kv", "<prefix-unordered>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: (Space: space, Bound: prefix),
                lsm: static (s, l) => Guarded(() => {
                    using Snapshot pinned = l.Store.CreateSnapshot();
                    using Iterator cursor = l.Store.NewIterator(Cf(l, s.Space), new ReadOptions().SetSnapshot(pinned));
                    Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)> rows = default;
                    for (cursor.Seek(s.Bound.Span); cursor.Valid() && cursor.GetKeySpan().StartsWith(s.Bound.Span); cursor.Next()) {
                        rows = rows.Add(((ReadOnlyMemory<byte>)cursor.GetKeySpan().ToArray(), (ReadOnlyMemory<byte>)cursor.GetValueSpan().ToArray()));
                    }
                    return rows;
                }).Bind(rows => Opened(l.Keys, s.Space, rows)),
                mmap: static (s, m) => Guarded(() => {
                    using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
                    using LightningCursor cursor = transaction.CreateCursor(Db(m, s.Space));
                    Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)> rows = default;
                    if (cursor.SetRange(s.Bound.Span) == MDBResultCode.Success) {
                        for ((MDBResultCode code, MDBValue key, MDBValue value) = cursor.GetCurrent();
                             code == MDBResultCode.Success && key.AsSpan().StartsWith(s.Bound.Span);
                             (code, key, value) = cursor.Next()) {
                            rows = rows.Add(((ReadOnlyMemory<byte>)key.CopyToNewArray(), (ReadOnlyMemory<byte>)value.CopyToNewArray()));
                        }
                    }
                    return rows;
                }).Bind(rows => Opened(m.Keys, s.Space, rows)));

    public static Fin<Seq<ReadOnlyMemory<byte>>> Refs(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key) =>
        space.Layout is KvLayout.Single
            ? Fin.Fail<Seq<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("kv", "<members-unaccrued>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: space,
                lsm: static (s, l) => s.Layout.Operator.IsSome
                    ? Guarded(() => Optional(l.Store.Get(s.Key.Span, Cf(l, s))))
                        .Bind(held => held.Match(
                            Some: frame => SpoolAccrual.Members(frame).Bind(members => Opened(l.Keys, s, s.Key, members)),
                            None: () => Fin.Succ(Seq<ReadOnlyMemory<byte>>())))
                    : Scan(new KvEngine.Lsm(l.Store, l.Spaces, l.Keys), s, s.Key).Map(static rows => rows.Map(static row => row.Value)),
                mmap: static (s, m) => Guarded(() => {
                    using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
                    using LightningCursor cursor = transaction.CreateCursor(Db(m, s));
                    return s.Layout.Width.Match(
                        Some: width => Paged(cursor, s.Key, width),
                        None: () => toSeq(cursor.AllValuesFor(s.Key.ToArray())).Map(static value => (ReadOnlyMemory<byte>)value.CopyToNewArray()));
                }).Bind(members => Opened(m.Keys, s, s.Key, members)));

    public static Fin<Seq<(ulong Sequence, ReadOnlyMemory<byte> Batch)>> Since(KvEngine engine, ulong sequence) => engine.Switch(
        state: sequence,
        lsm: static (cursor, l) => Guarded(() => {
            using TransactionLogIterator feed = l.Store.GetUpdatesSince(cursor);
            Seq<(ulong Sequence, ReadOnlyMemory<byte> Batch)> updates = default;
            for (; feed.Valid(); feed.Next()) {
                using WriteBatch batch = feed.GetBatch(out ulong at);
                updates = updates.Add((at, (ReadOnlyMemory<byte>)batch.ToBytes()));
            }
            return updates;
        }),
        mmap: static (_, _) => Fin.Fail<Seq<(ulong, ReadOnlyMemory<byte>)>>(new EmbeddedFault.Kv("lmdb", "<no-wal>", "<changefeed>", RetryShape.Terminal)));

    public static Fin<Unit> Snap(KvEngine engine, string directory) => engine.Switch(
        state: directory,
        lsm: static (target, l) => Guarded(() => { using Checkpoint clone = l.Store.Checkpoint(); clone.Save(target); return unit; }),
        mmap: static (target, m) => Guarded(() => m.Store.CopyTo(target, compact: true)).Bind(Mdb));

    static ColumnFamilyHandle Cf(KvEngine.Lsm engine, KvSpace space) => engine.Spaces[space];
    static LightningDatabase Db(KvEngine.Mmap engine, KvSpace space) => engine.Spaces[space];

    static Fin<Option<ReadOnlyMemory<byte>>> Opened(KvVault vault, KvSpace space, ReadOnlyMemory<byte> key, Option<byte[]> held) =>
        held.TraverseM(value => vault.Unwrap(space, key.Span, value)).As();

    static Fin<Seq<ReadOnlyMemory<byte>>> Opened(KvVault vault, KvSpace space, ReadOnlyMemory<byte> key, Seq<ReadOnlyMemory<byte>> members) =>
        members.TraverseM(member => vault.Unwrap(space, key.Span, member)).As();

    static Fin<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>> Opened(KvVault vault, KvSpace space, Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)> rows) =>
        rows.TraverseM(row => vault.Unwrap(space, row.Key.Span, row.Value)
            .Map(value => (Key: row.Key, Value: value))).As();

    static Seq<ReadOnlyMemory<byte>> Paged(LightningCursor cursor, ReadOnlyMemory<byte> key, int width) {
        Seq<ReadOnlyMemory<byte>> members = default;
        if (cursor.Set(key.Span) != MDBResultCode.Success) { return members; }
        for ((MDBResultCode code, _, MDBValue page) = cursor.GetMultiple(); code == MDBResultCode.Success; (code, _, page) = cursor.NextMultiple()) {
            ReadOnlySpan<byte> packed = page.AsSpan();
            for (int at = 0; at + width <= packed.Length; at += width) {
                members = members.Add((ReadOnlyMemory<byte>)packed.Slice(at, width).ToArray());
            }
        }
        return members;
    }

    static Fin<Unit> Mdb(MDBResultCode status) =>
        EmbeddedFault.OfLmdb(status).Match(Some: Fin.Fail<Unit>, None: static () => Fin.Succ(unit));

    static Fin<T> Guarded<T>(Func<T> call) =>
        Try.lift(() => call()).Run().MapFail(EmbeddedFault.Lift);
}
```

| [INDEX] | [POLICY]             | [VALUE]                                    | [BINDING]                                                          |
| :-----: | :------------------- | :----------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | handle bridge        | `HandleBridge` over the raw handle         | ONE resolve/capture/status/dispose path; a second reach deleted    |
|  [02]   | checkpoint state     | `sqlite3_wal_checkpoint_v2` out-params     | typed frame counts; `SQLITE_BUSY` retries the schedule             |
|  [03]   | consistent read      | `sqlite3_snapshot_*` pin bracket           | `_cmp` floor guard; `_free` only a held handle                     |
|  [04]   | backup               | paged `sqlite3_backup_*` session           | subsumes whole-file `BackupDatabase`; `quick_check` proof          |
|  [05]   | large payload        | `SqliteBlob` over `zeroblob(N)`            | streamed; whole-`byte[]` materialization deleted                   |
|  [06]   | fault discrimination | `EmbeddedFault` over the status int        | `Busy` waits or restarts; `Corrupt` routes to recovery             |
|  [07]   | embedded KV          | `KvFloor` over `KvEngine` (LSM/mmap)       | offline op spool + chunk index; one polymorphic surface            |
|  [08]   | KV drain and sweep   | `Scan`/`Refs` snapshot-pinned walks        | prefix scan + dupsorted reverse refs; point-Get-only form deleted  |
|  [09]   | KV resume and clone  | `Since` WAL cursor, `Snap` clone/copy      | reconnect replay from a sequence; hard-link or compacting copy     |
|  [10]   | KV write intent      | `KvWrite` Put/Append/Unlink/Drop           | dup-scoped retirement; a whole-key drop never empties a dup set    |
|  [11]   | KV fault taxonomy    | raised and returned codes share `Mdb`      | `LightningException.StatusCode` folds where a return code folds    |
|  [12]   | keyed backup         | `BackupPolicy.Dek` before `backup_init`    | destination binds its source's cipher; no plaintext egress         |
|  [13]   | KV keyspace axis     | `KvSpace` row = CF and named DB            | one roster, two engines; prefix never a keyspace                   |
|  [14]   | KV key order         | `KvOrder.Bytewise`, no comparer            | proof by omission; `Scan` gates on `PrefixSound`                   |
|  [15]   | KV accrual           | `KvLayout` `Single`/`Accrued`/`Fanned`     | drives flags, family options, `Append`, `Refs`                     |
|  [16]   | KV merge frame       | `SpoolAccrual` count + length prefixes     | one `Merge` per append; separator join deleted                     |
|  [17]   | KV durability        | `KvDurability` per row                     | LSM per write; LMDB env takes the strictest                        |
|  [18]   | KV at rest           | `KvVault` AEAD over values                 | keys and dup values clear; `Degrade` names both                    |
|  [19]   | retry axes           | kernel `Retriability` DERIVED from `Route` | route names where; posture names whether; `Waited` alone transient |
|  [20]   | rocksdb failure      | exact exceptional `Error`                  | no stable typed status; message never drives recovery              |
|  [21]   | embedded lift        | typed SQLite and LMDB status folds         | raised and returned codes land one verdict                         |

## [05]-[STORE_AXIS_MAP]

Store perimeter is PARAMETERIZED — eleven axes, every provider row deployment/policy DATA on one axis surface. Policy values select every provider — profile rows, grant minters, sink rows, index-residency rows — never a central-manifest edit, never a new entry point, never a parallel path. Each kept scale-out row carries the PROVEN ceiling the in-PG/in-process owner cannot reach; every provider row carries its provisioning/health/recovery posture through the `#SERVER_EXTENSIONS` verification-first fold, and the scylla/redis rows gain DEPLOYMENT-CONDITIONAL AppHost probe rows only where the axis row is composed (the Npgsql-only probe stays the default). Relational SoR spine is SINGULAR and sealed — ONE event store, ONE materializer, ONE identity, ONE changefeed — so a perimeter-axis engine row carrying unreachable capability is a legal axis admission, never a second SoR.

| [INDEX] | [AXIS]                    | [SELECTION]                                                 |
| :-----: | :------------------------ | :---------------------------------------------------------- |
|  [01]   | relational SoR spine      | SEALED                                                      |
|  [02]   | object store              | `ObjectStore` `[SmartEnum]`                                 |
|  [03]   | egress sink               | `Subscription` over `Binding`                               |
|  [04]   | read-lane/analytic engine | `ColumnarEngine` axis                                       |
|  [05]   | lakehouse interchange     | format row                                                  |
|  [06]   | vector search             | `VectorBackend` axis                                        |
|  [07]   | embedded/KV floor         | `EngineOps`-tier row                                        |
|  [08]   | embedded relational       | `StoreProfile.Ef` on ONE DbContext                          |
|  [09]   | wide-column content-index | index-residency row                                         |
|  [10]   | cache backplane           | `CacheLane.Store`-gated row                                 |
|  [11]   | spatial store plane       | profile policy rows (`geographyAsDefault`, SRID, precision) |

Per axis, the owning page(s), the provider seed rows (deployment/policy DATA), and the ceiling/charter proof each kept row proves:

- [01]-[RELATIONAL_SOR_SPINE]: `Store/provisioning` + `Element/graph`; postgres-18 (SINGULAR); the one event store · materializer · identity · changefeed, unchallengeable.
- [02]-[OBJECT_STORE]: `Store/blobstore`; s3 · azure-blob · gcs · minio · presigned-grant (`GrantMinter`); the presigned row reaches domain-cloud planes no credentialed row can.
- [03]-[EGRESS_SINK]: `Version/egress`; webhook · nats · kafka · rabbitmq · pulsar · wire-native · redis-stream · clickhouse; redis-stream persists on the awaited `StreamAdd` id under producer `StreamIdempotentId` (downstream consumer-group acks never govern the outbox cursor), clickhouse on the awaited `InsertBinaryAsync` under `insert_deduplication_token` — the zero-broker-install stream row and the warehouse leg.
- [04]-[READ_LANE_ANALYTIC_ENGINE]: `Query/columnar`; duckdb-in-process · pg_duckdb-in-PG · clickhouse-scaleout; distributed merge-tree MPP at cluster scale, never a second SoR.
- [05]-[LAKEHOUSE_INTERCHANGE]: `Query/columnar`; ducklake (extension, forward) · delta; the Delta transaction-log wire for external-warehouse interop, a format not an engine.
- [06]-[VECTOR_SEARCH]: `Query/retrieval`; pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · qdrant-scaleout; billion-scale sharded ANN over the in-PG ceiling, `CqlVector` embedding-next-to-row only.
- [07]-[EMBEDDED_KV_FLOOR]: `Store/provisioning`; sqlite (raw-ADO `EngineOps`) · rocksdb-lsm · lmdb (both operational through `#ENGINE_OPERATIONS` `KvFloor` over the one `KvSpace` keyspace roster, sealed at the value); write-optimized LSM + read-optimized memory-mapped MVCC over the single-writer WAL floor.
- [08]-[EMBEDDED_RELATIONAL]: `Element/identity` + `Store/provisioning`; npgsql-ef · sqlite-ef; one generated mapping, two providers; a hand ADO mapping beside it is deleted (ARCH).
- [09]-[WIDE_COLUMN_CONTENT_INDEX]: `Query/cache`; marten-pg (default) · scylla-widecolumn; LWT `AppliedInfo` claim-gate + shard-routed point reads at federation scale.
- [10]-[CACHE_BACKPLANE]: `Query/cache`; none (single-node default) · redis-pubsub; cross-process L1 invalidation the `IDistributedCache` contract cannot express.
- [11]-[SPATIAL_STORE_PLANE]: `Element/identity` · `Store/provisioning` · `Element/codec` · `Ingest/geospatial`; postgis-column (EF-NTS) · ado-codec (`SourceWire`) · geojson-stj · geopackage · wkb/wkt · h3-cell (pocketken); the provisioned postgis/pgrouting/h3-pg tier gains its wire, column, codec, and file-ingress counterparts, closed end-to-end.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
