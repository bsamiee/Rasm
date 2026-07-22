# [PERSISTENCE_STORE_PROVISIONING]

Rasm.Persistence provisions the PostgreSQL 18 server tier as ONE VERIFICATION-FIRST read fold and the embedded SQLite floor as ONE idempotent open ritual, the two engines the closed `StoreProfile` axis selects by deployment and never a third: a Rasm process NEVER executes runtime `ALTER SYSTEM`, NEVER spawns or bundles PostgreSQL, and NEVER mutates the cluster — provisioning is a typed `ProvisionVerdict` over what the operator-provisioned cluster already carries, and a gap is a typed signal the operator resolves at the four provisioning rungs (migration artifact, idempotent seed, operator runbook, environment), the fold reading all four and EMITTING repair artifacts (reconciliation grants, `shared_preload_libraries` diffs) as typed verification outputs it never executes. Each server extension is a `ServerExtension` `[SmartEnum<string>]` row carrying its `CreateSql`, its `Admission` gate (preload library, base type, or access method the install requires), the analytical `lane` it serves, and the `RestartClass` its preload gap repairs under — so a new extension is one row and a gap names its repair's disruption class; each verified cluster knob is a `ClusterSetting` row, and the absence policy is not a `Switch` arm but `FailureRank` behavior — a `[SmartEnum]` whose `Absorb` delegate threads the floor-miss receipt, `Required` refusing the profile, `Degradable` folding the lane out so absence surfaces at admission instead of first query, `Observational` recording evidence. Verification is ONE `NpgsqlBatch` round trip over `pg_available_extensions`/`pg_extension`/`pg_settings`/`pg_replication_slots`/`pg_index`, folding the required roster, the held analytical lanes, the emitted repair set, and a stamped `VerificationEpoch` into one verdict the process dispatches on and never re-probes, so admission cost is data-volume-independent and environment drift is an observable epoch event on the fact stream. Beneath the server tier the embedded SQLite floor is the same fold discipline for a single-process store — the open ritual folds pragma rows by RESIDENCY (file-persistent provisioning rows versus per-connection rows), registers connection-scoped capabilities (`uuid7`/`xxh128` UDFs, the `instant_iso` collation, a domain aggregate) before the first statement, hardens through the `SqliteConnection.Handle` raw `sqlite3_db_config` defensive set, gates first-opener migration under one IMMEDIATE transaction, and arms extended result codes — and the `EngineOps` capsule owns the raw-handle operations the managed ADO surface omits (the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder), every throwing crossing converting to a closed `EmbeddedFault` and disposing the connection on every failure path. Every loose provisioning integer is a typed case — `ServerFault` carries the WHOLE re-banded 838x decade (`FaultBand.Server`, the absence/readiness/admission receipts included) and the foreign-store/epoch-ahead refusals are `EmbeddedFault.Refused` in-band 771x, so a bare `Error.New` is the deleted form here; the `StoreProfile` rows additionally carry the wire and EF provider bindings — the `NpgsqlDataSourceBuilder.UseNetTopologySuite` ADO codec row (raw Npgsql lanes read/write geometry: the `cypher` pgrouting results, the verification probes over PostGIS, any `QueueSqlCommand` spatial write; `geographyAsDefault`/precision/ordinates are profile POLICY values, never call-site literals — the EF plugin does not place the codec on raw connections) and the `Ef` bind row (`Server` → `UseNpgsql`, `Embedded` → `UseSqlite` over the connection the open ritual already dialed) feeding the ONE `Element/identity#ELEMENT_IDENTITY` DbContext, so provider variance stays row data on the closed axis and a hand ADO mapping beside the generated rail is the deleted form; the `Npgsql.OpenTelemetry` observability row (`TracerProviderBuilder.AddNpgsql()` + `MeterProviderBuilder.AddNpgsqlInstrumentation()`) subscribes at the AppHost composition root; the `pg_jsonschema` validation lane degrades to the in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` fence when the server extension folds out, one schema serving both residences. This page also hosts the `[V13]` `#STORE_AXIS_MAP` — the 11-axis store perimeter whose provider rows are deployment/policy DATA. Wall clock, correlation, and tenant ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — a `ClockPolicy`/`CorrelationId`/`TenantContext` parameter on any signature here is the named strata inversion; `ReceiptSinkPort` arrives settled from AppHost; `FaultBand` from `Element/graph#FAULT_TABLES`; `NpgsqlDataSource`/`IDocumentSession`/`IDocumentStore` from the substrate; the analytical lanes that consume the verified extensions arrive from `Query/columnar`/`Query/cypher`/`Query/topology`; the `h3-pg` cell convention the `h3_postgis` bridge serves matches the managed `pocketken.H3` (`Element/identity#ELEMENT_IDENTITY`).

## [01]-[INDEX]

- [01]-[SERVER_EXTENSIONS]: the extension × admission-gate × lane roster, the `FailureRank` absence behavior, the one-batch verification fold over the catalog reads (extension version floors included), the four provisioning rungs, the emitted repair set, the `MaintenanceJob` in-database work roster, the wire/EF provider-binding rows, the `pg_jsonschema` in-process fallback fence, and the stamped verification epoch.
- [02]-[EMBEDDED_FLOOR]: the residency-split pragma ladder, the connection-scoped capability registration, the defensive `sqlite3_db_config` hardening, the first-opener IMMEDIATE migration gate, and the closed-engine law.
- [03]-[ENGINE_OPERATIONS]: the `Handle`-bridge capsule, the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder, the `KvFloor` embedded-KV capsule (the rocksdb-lsm op spool and lmdb membership index of axis [07]), and the closed `EmbeddedFault` rail.
- [04]-[STORE_AXIS_MAP]: the 11-axis store perimeter — every provider row deployment/policy DATA on one axis surface, each scale-out row carrying its proven ceiling.

## [02]-[SERVER_EXTENSIONS]

- Owner: `StoreProfile` the `[SmartEnum<string>]` engine-selection axis the deployment dials (`server` the PostgreSQL 18 tier, `embedded` the SQLite floor) carrying the `Verify`/`Open` provisioning rail each engine runs — the closed two-engine sweep, never a third; `ServerExtension` the `[SmartEnum<string>]` extension axis, each row carrying its `CreateSql`, its `Admission` install gate, the analytical `Lane` it serves, and the `RestartClass` its preload gap repairs under; `ExtensionAdmission` the closed install-gate `[Union]` (a preload library, a base type the extension extends, a real queryable access method it registers, or a prerequisite-free standalone function/type extension); `RestartClass` the `[SmartEnum<string>]` repair-disruption vocabulary (`session`/`reload`/`restart`); `FailureRank` the `[SmartEnum]` whose `Absorb` delegate IS the absence policy; `ClusterSetting` the verified-knob vocabulary; `ProvisionVerdict` the verification verdict carrying the held set, the receipts, the emitted repair artifacts, and the stamped `VerificationEpoch`; `ServerFault` the closed catalog-read fault `[Union]` over `Expected`; `ClusterProvision` the static surface running the one-batch verification fold and the gated admission — never an `ALTER SYSTEM`.
- Cases: `ServerExtension` is the AUTHORITATIVE provisioning roster — it SUPERSETS the consumer-facing `README#SERVER_EXTENSIONS` subset with the base-type and toolkit rows a dependency chain requires (`postgis` the standalone base the raster/sfcgal/pgrouting rows gate on, `pgvector` the `vector` base `pgvectorscale` gates on, `pg_duckdb` the in-PG DuckDB bridge, `timescaledb_toolkit` over the `timescaledb` base) so the `BaseType` gate resolves against a row the same fold can admit, never against an externally-assumed prerequisite; each gate is the `.api`-verified install precondition, NOT a loose label: `timescaledb` (preload, the hypertable/continuous-aggregate/columnstore analytics, `Query/columnar`), `timescaledb_toolkit` (the hyperfunction/time-weighted-aggregate layer over the `timescaledb` base type), `pg_duckdb` (preload, the in-PG DuckDB analytical bridge distinct from the in-process `DuckDB.NET` lane, `Query/columnar`), `apache-age` (standalone — the OPTIONAL openCypher graph functions + `agtype`, no preload; Cypher connections issue per-session `LOAD 'age'`, demoted beneath QuikGraph, `Query/cypher#GRAPH_SESSION`), `pg_cron` (preload, the in-database maintenance scheduler), `postgis` (standalone — operator classes over the BUILT-IN `gist` AM, registers no custom access method, the base the raster/3D/routing rows extend), `postgis_raster`/`postgis_sfcgal` (PostGIS raster + exact 3D geometry over the `postgis` base type), `pgvector` (the `hnsw` access-method ANN tier) / `pgvectorscale` (the `diskann` AM gated on the `vector` base type), `pg_search` (PRELOAD-gated — the ParadeDB Tantivy `bm25` engine rides `shared_preload_libraries` and hard-errors on `CREATE EXTENSION` without it), `h3-pg` (standalone — the in-PG H3 cell index over built-in AMs and the `h3_postgis` bridge over the `h3` base type, matching `pocketken.H3`), `pgrouting` (the network routing over the `postgis` base type, `Query/cypher#GRAPH_QUERY`), `pg_partman` (PRELOAD-gated — its `pg_partman_bgw` background worker rides `shared_preload_libraries`), `pg_squeeze` (preload, lock-light table-bloat reclamation), `pg_jsonschema` (standalone — `CREATE EXTENSION`-registered JSON Schema CHECK functions, no preload), `pgaudit` (preload, session/object audit logging), `pg_net` (PRELOAD-gated — its `libcurl` background worker is statically `RegisterBackgroundWorker`'d in `_PG_init` and hard-errors without `shared_preload_libraries`), `pg_graphql` (standalone — pgrx SQL functions + DDL event triggers, no worker, no preload); `ExtensionAdmission` is `Preload(library)` | `BaseType(extension)` | `AccessMethod(method)` (a real queryable index AM the row registers, e.g. `hnsw`) | `Standalone(reason)` (prerequisite-free function/type/operator-class extension that registers NO gating AM); `FailureRank` is `Required`/`Degradable`/`Observational`; `ProvisionVerdict` is `Provisioned | MissingExtension | MissingPreload | SettingDrift | Faulted`; `ServerFault` is the WHOLE re-banded 838x decade — the catalog-read faults (`Unmapped | Unreachable | CatalogDenied`), the absence receipts the `FailureRank.Absorb` delegates thread (`RequiredAbsent | LaneFolded | Evidence`), the readiness evidence (`SlotLag | InvalidIndex`), and the admission refusals (`Ungated | AdmitRefused`) — ten typed cases deriving `FaultBand.Server + n`, so every loose provisioning integer is a registry-derived case and a bare `Error.New(83xx)` is the deleted form.
- Entry: `public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, ClusterDemand demand)` runs one `CreateBatch` over the catalog reads and folds the demand's roster, version-floor policies, epoch, held analytical lanes, absorbed receipts, and repair set into one verdict; each `ExtensionFloor` supplies its deployment-specific comparison law because extension version strings do not share one parser. `Register(IDocumentSession, MaintenanceJob, ProvisionVerdict.Provisioned)` and `Admit(IDocumentSession, ServerExtension, ProvisionVerdict.Provisioned)` consume the exact catalog snapshot carried by the admitted verdict, re-gate without loose set parameters, and queue only database-local idempotent SQL inside the migration transaction. `Reload(NpgsqlDataSource)` calls `ReloadTypesAsync` after committed type-bearing DDL. `public static ProvisionManifest Manifest(ClusterDemand demand, EmbeddedRitual ritual)` folds server expectations, the encrypted embedded provider, and ritual rows into one desired-state wire; every `ManifestRow` names its `#STORE_AXIS_MAP` coordinate, so the deploy plane converges on the same declarations the runtime consumes while in-process provisioning stays verification-only.
- Auto: verification is ONE six-command batch — `current_setting('shared_preload_libraries')`, `pg_extension` (created), `pg_available_extensions` (installed-on-disk-but-uncreated), the `pg_settings` rows for every `ClusterSetting`, the `pg_replication_slots` `pg_wal_lsn_diff` max-lag scalar, and the `pg_index WHERE NOT indisvalid` count — folded so a preload-gated extension whose library is absent from `shared_preload_libraries` is `MissingPreload` and EMITS a `shared_preload_libraries` repair diff carrying the `RestartClass.Max` worst-disruption rank across the gap set (so the operator reads ONE bounce cost, never a per-row minimum) the operator applies and restarts (never a runtime `ALTER SYSTEM`), an extension PRESENT in `pg_available_extensions` and uncreated with a satisfied gate admits through `CREATE EXTENSION IF NOT EXISTS` in the one session (one absent from the available set has no admissible repair and threads its `FailureRank.Absorb` instead), a `pg_settings` row whose live value fails its `Satisfied` check folds `SettingDrift` carrying the row's `RestartClass`, and a held analytical lane absent below its `FailureRank` threads its `Absorb` receipt — `Required` refusing the profile, `Degradable` folding the lane out so the gap surfaces at admission, `Observational` recording evidence; a lagging replication slot and any invalid index fold in as `Observational` readiness receipts on the held verdict (server-disk liability and an interrupted concurrent build, visible on the fact stream, never profile-refusing) — the slot scalar `max(pg_wal_lsn_diff(pg_current_wal_lsn(), restart_lsn))::bigint` measures the byte lag of ANY operator-configured replication slot the cluster carries (a physical streaming replica or a third-party logical consumer) read through `NpgsqlDataReader.GetInt64` since `pg_wal_lsn_diff(...)::bigint` is a PG `bigint`, the `restart_lsn` column being the WAL-retention floor every slot kind exposes so the gauge is provider-physical and never assumes a slot identity; it is NOT the durability op-log lane's lag, because the op-log changefeed is a Marten async SUBSCRIPTION reading the committed event table (`Version/ledger#CHANGEFEED`, `H11`), NOT a logical-decoding slot consumer, so the lane carries no `pg_replication_slots` row and `confirmed_flush_lsn` (a logical-slot-only column) is deliberately NOT the surface here — the `wal_level=logical` `ClusterSetting` exists for the recovery `LogicalReplicationConnection.IdentifySystem` RPO probe (`Version/recovery#RECOVERY_ROUTES`) and operator logical consumers, not for an op-log slot; the fold carries zero rank arms (a new rank is one `FailureRank` row), the `h3-pg` cell id matches the managed `pocketken.H3` so the same cell indexes at ingest and in SQL, and a periodic re-`Verify` stamps a fresh `VerificationEpoch` so cluster drift becomes an observable event the AppHost health probe reads (`ARCHITECTURE#SEAMS` `[HEALTH_PROBE]`).
- Receipt: a verification rides `store.provision.verify` carrying the verdict, the held lane set, the emitted repair count, and the stamped epoch; an admission rides `store.provision.admit` carrying the extension; a type reload rides `store.provision.reload`.
- Packages: Npgsql (`NpgsqlDataSource.CreateBatch`, `NpgsqlBatchCommand`, `NpgsqlBatch.ExecuteReaderAsync`, `NpgsqlDataReader.NextResultAsync`/`GetInt64`/`GetString`, `NpgsqlParameter<string[]>`, `ReloadTypesAsync`, `PostgresException.SqlState`/`PostgresErrorCodes.InsufficientPrivilege`, `NpgsqlException.IsTransient`, `NpgsqlDataSourceBuilder`), Npgsql.NetTopologySuite (`NpgsqlDataSourceBuilder.UseNetTopologySuite(handleOrdinates, geographyAsDefault)` — the ADO codec row), Npgsql.OpenTelemetry (`TracerProviderBuilder.AddNpgsql()` / `MeterProviderBuilder.AddNpgsqlInstrumentation()` — the observability row subscribed at the AppHost composition root), JsonSchema.Net (`Json.Schema.JsonSchema.FromText`/`Evaluate(JsonElement, EvaluationOptions?)` — the in-process validation fence), NetTopologySuite (`Ordinates`), Microsoft.EntityFrameworkCore (+ `.Sqlite` `UseSqlite` and the Npgsql EF `UseNpgsql` — the `StoreProfile.Ef` bind row over the `Element/identity` DbContext), Marten (`IDocumentSession.QueueSqlCommand`/`SaveChangesAsync`), Rasm.Persistence.Element (`FaultBand`), NodaTime, LanguageExt.Core (`Seq`/`Fin`/`@catch`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new server extension is one `ServerExtension` row carrying its SQL, install gate, lane, and restart class; a new install-gate shape is one `ExtensionAdmission` case; a new absence policy is one `FailureRank` row landing every floor-miss branch with zero `Switch` edits; a new verified setting is one `ClusterSetting` row; a new version floor is one `floors` entry (deployment data, never a fence literal); a new in-database maintenance job is one `MaintenanceJob` row riding the gated `Register` admission; zero new surface — a runtime `ALTER SYSTEM`, a Rasm-spawned PostgreSQL, a per-extension managed package, a `Switch` re-enumerating the absence policy at the fold, a per-extension probe round trip, or a second relational engine row is the deleted form because provisioning is verification-first SQL, the absence policy IS the rank-row delegate, the verification is one batch, and the engine sweep is closed.
- Boundary: a Rasm process NEVER spawns or bundles PostgreSQL and NEVER executes runtime `ALTER SYSTEM` — provisioning is verification-only over the operator-provisioned cluster (`#SERVER_EXTENSIONS`), so a `MissingPreload`/`SettingDrift`/`MissingExtension` verdict is a typed signal carrying the EMITTED repair artifact (a `shared_preload_libraries` diff, a `CREATE EXTENSION` reconciliation, a settings diff) the operator resolves at one of the four provisioning rungs, never a self-mutation; the server extensions carry no managed assembly and admit through raw `CREATE EXTENSION IF NOT EXISTS` gated by the row's `ExtensionAdmission` (a preload library, a base type, a real queryable access method, or a prerequisite-free standalone function/type extension) — the `.api`-verified gate per row, so a preload-gated extension mislabeled no-prerequisite cannot leak a hard-erroring `CREATE EXTENSION` past the gate; the `pg_duckdb` extension is the in-PG DuckDB bridge distinct from the in-process `DuckDB.NET` analytical lane (`Query/columnar`), the two meeting at the columnar SQL surface; `apache-age` is the OPTIONAL self-hosted openCypher graph (`Query/cypher#GRAPH_SESSION`) demoted beneath the in-process QuikGraph (`H5`), so its admission is gated and the lane is disabled by default and never assumed co-resident with Marten; spatial→PG GiST (`postgis_raster`/`postgis_sfcgal`) and ANN→`pgvector`/`pgvectorscale` are the transactional index owners while DuckDB `spatial`/`vss` are the columnar aggregators (`L2`), never duplicated; a catalog read denied by privilege folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`) and a transport failure folds through `NpgsqlException.IsTransient` so a retry re-drives only the transient class; `ReloadTypesAsync` completes the deploy by re-resolving wire types, the rejected form being a process that resolves a freshly-admitted enum/composite as unknown until restart.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

// --- [TYPES] ----------------------------------------------------------------------------

// closed engine-selection axis the deployment dials — the ONE place the two relational engines are named, so a
// `cls.Key == "blob"`-style string compare or a third engine is the deleted form (`#EMBEDDED_FLOOR` keeps the sweep
// closed). `Server` is the operator-provisioned PostgreSQL 18 tier whose provisioning is `ClusterProvision.Verify`
// (verification-first, never `ALTER SYSTEM`); `Embedded` is the single-process SQLite floor whose provisioning is the
// `EmbeddedStore.Open` ritual. The row carries BOTH provider bindings as data: `Ef` binds the ONE identity DbContext
// (`Element/identity#ELEMENT_IDENTITY` — Server -> UseNpgsql, Embedded -> UseSqlite over the ritual-dialed connection;
// a hand ADO mapping beside the generated rail is the deleted form) and the Server row's data source composes the
// `Npgsql.NetTopologySuite` ADO codec so RAW Npgsql lanes read/write geometry (`ClusterProvision.Source`).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoreProfile {
    public static readonly StoreProfile Server = new("server", relational: "postgresql-18", isEmbedded: false, ef: static (builder, connection) => builder.UseNpgsql((NpgsqlConnection)connection));
    public static readonly StoreProfile Embedded = new("embedded", relational: "sqlite", isEmbedded: true, ef: static (builder, connection) => builder.UseSqlite((SqliteConnection)connection));
    public string Relational { get; }
    public bool IsEmbedded { get; }
    private StoreProfile(string key, string relational, bool isEmbedded, Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> ef) : this(key) => (Relational, IsEmbedded, Ef) = (relational, isEmbedded, ef);

    // EF provider-bind row ([05] EF-Sqlite admission): provider variance as row DATA on the closed axis —
    // ONE identity DbContext maps both engines through the generated rail (`IsSqlite()`-keyed model rows
    // live at the identity owner); raw ADO keeps EmbeddedRitual/EngineOps/HandleBridge untouched.
    public Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> Ef { get; }
}

// ADO spatial-codec policy row ([05] Npgsql.NetTopologySuite admission): geographyAsDefault / SRID /
// precision / ordinates are PROFILE policy values, never call-site literals — the EF plugin does not place
// codec on raw connections, so the data source composes it once for every raw Npgsql lane (the cypher
// pgrouting results, the verification probes over PostGIS, any QueueSqlCommand spatial write).
public sealed record SpatialWire(bool GeographyAsDefault, int Srid, Ordinates HandleOrdinates) {
    public static readonly SpatialWire Canonical = new(GeographyAsDefault: false, Srid: 4326, Ordinates.XYZ);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RestartClass {
    // A preload/setting repair's disruption class — `session` is a `SET`/reconnect, `reload` an `pg_reload_conf()`,
    // `restart` a full cluster bounce; a `MissingPreload`/`SettingDrift` verdict carries it so the gap names what its
    // repair costs the operator, never a bare "fix it" signal.
    public static readonly RestartClass Session = new("session", rank: 0);
    public static readonly RestartClass Reload = new("reload", rank: 1);
    public static readonly RestartClass Restart = new("restart", rank: 2);
    public int Rank { get; }
    private RestartClass(string key, int rank) : this(key) => Rank = rank;

    // worst disruption across a gap set — an aggregated repair (a `shared_preload_libraries` diff folding several
    // preload gaps, a `MissingExtension` set with mixed restart classes) carries the MAX so the operator reads ONE
    // disruption cost for the whole reconciliation, never a per-row minimum that understates the bounce; an empty set
    // is `Session` (the no-disruption floor). The `Rank` column is load-bearing here, not decorative.
    public static RestartClass Max(Seq<RestartClass> over) =>
        over.Fold(Session, static (worst, next) => next.Rank > worst.Rank ? next : worst);
}

[SmartEnum]
public sealed partial class FailureRank {
    // absence policy IS behavior-carrying row data (`#SERVER_EXTENSIONS`): the floor-miss branch threads
    // receipt through one `Absorb` delegate, so `Required` refuses the profile and stays minimal, `Degradable`
    // folds the lane out so absence surfaces at admission not first query, `Observational` records evidence — a new
    // rank lands as one row, the fold carrying zero rank arms. Every receipt is a TYPED `ServerFault` case
    // deriving off the 8380 registry row — the loose `Error.New(8371/8372/8373)` integers are the deleted form.
    public static readonly FailureRank Required = new(static (_, key) => Fin.Fail<Seq<Error>>(new ServerFault.RequiredAbsent(key)));
    public static readonly FailureRank Degradable = new(static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.LaneFolded(key))));
    public static readonly FailureRank Observational = new(static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.Evidence(key, "<absent>"))));

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Error>> Absorb(Seq<Error> receipts, string extensionKey);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtensionAdmission {
    private ExtensionAdmission() { }
    // install precondition the cluster must already satisfy before `CREATE EXTENSION` can succeed: `Preload` a library
    // in `shared_preload_libraries` (the cluster must boot with it — its worker/hook is `RegisterBackgroundWorker`'d or
    // planner-hooked from `_PG_init` and `CREATE EXTENSION` HARD-ERRORS without it: `timescaledb`/`pg_duckdb`/
    // `pg_cron`/`pg_squeeze`/`pgaudit`/`pg_search`/`pg_net`/`pg_partman_bgw`), `BaseType` an extension the row extends and must
    // be created first (`pgvectorscale` over `vector`, `postgis_*`/`pgrouting` over `postgis`, `h3_postgis` over `h3`,
    // `timescaledb_toolkit` over `timescaledb`). `AccessMethod` names a queryable index access method the extension itself
    // REGISTERS as the gate's documentation (pgvector `hnsw`) — the row carries no CATALOG prerequisite (the AM and its
    // operator classes land WITH the `CREATE EXTENSION`), so it is unconditionally admissible once present on disk.
    // `Standalone` is the genuinely prerequisite-free function/type/event-trigger extension that registers NO gating
    // access method and rides no preload row — `postgis` (ships operator classes over the BUILT-IN `gist`, registers no
    // custom AM), `h3` (operator classes over built-in btree/hash/brin/spgist), `pg_jsonschema`/`pg_graphql` (pgrx SQL
    // functions + event triggers, no worker), `age` (openCypher functions + the `agtype` type; the per-session `LOAD 'age'`
    // is a runtime connection concern, not a preload gate) — its `Reason` documents what the row brings, never a precondition read.
    // `AccessMethod`/`Standalone` are the two no-prerequisite cases (`available` membership the `Fold` already requires is
    // their only gate); so `Admissible` reads the live `preloaded`/`created` catalog ONLY for the two gated cases (`Preload`/
    // `BaseType`) — a preload-gap or base-type-gap `CREATE EXTENSION` that the catalog says is a GUARANTEED runtime error
    // never runs, and a row mislabeling a preload-gated extension as no-prerequisite (the deleted form the `.api`-verified
    // roster forecloses) cannot leak a hard-erroring `CREATE EXTENSION pg_net` past the gate.
    public sealed record Preload(string Library) : ExtensionAdmission;
    public sealed record BaseType(string Extension) : ExtensionAdmission;
    public sealed record AccessMethod(string Method) : ExtensionAdmission;
    public sealed record Standalone(string Reason) : ExtensionAdmission;

    public bool Admissible(IReadOnlySet<string> preloaded, IReadOnlySet<string> created) => this switch {
        Preload p    => preloaded.Contains(p.Library),
        BaseType b   => created.Contains(b.Extension),
        AccessMethod => true,
        Standalone   => true,
        _            => false,
    };
    public Option<string> PreloadLibrary => this is Preload p ? Some(p.Library) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ServerExtension {
    public static readonly ServerExtension Timescaledb = new("timescaledb", new ExtensionAdmission.Preload("timescaledb"), "columnar", FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension TimescaledbToolkit = new("timescaledb_toolkit", new ExtensionAdmission.BaseType("timescaledb"), "columnar", FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgDuckdb = new("pg_duckdb", new ExtensionAdmission.Preload("pg_duckdb"), "columnar", FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension Postgis = new("postgis", new ExtensionAdmission.Standalone("operator classes over the built-in gist AM; registers no custom access method"), "geo", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension ApacheAge = new("age", new ExtensionAdmission.Standalone("openCypher graph functions + the agtype type over built-in storage; registers no custom AM, CREATE EXTENSION needs no preload, Cypher connections issue per-session LOAD 'age'"), "cypher", FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgCron = new("pg_cron", new ExtensionAdmission.Preload("pg_cron"), "maintenance", FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgSqueeze = new("pg_squeeze", new ExtensionAdmission.Preload("pg_squeeze"), "maintenance", FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension Pgaudit = new("pgaudit", new ExtensionAdmission.Preload("pgaudit"), "audit", FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PostgisRaster = new("postgis_raster", new ExtensionAdmission.BaseType("postgis"), "geo", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PostgisSfcgal = new("postgis_sfcgal", new ExtensionAdmission.BaseType("postgis"), "geo", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvector = new("vector", new ExtensionAdmission.AccessMethod("hnsw"), "vector", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvectorscale = new("vectorscale", new ExtensionAdmission.BaseType("vector"), "vector", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PgSearch = new("pg_search", new ExtensionAdmission.Preload("pg_search"), "search", FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension H3Pg = new("h3", new ExtensionAdmission.Standalone("operator classes over the built-in btree/hash/brin/spgist AMs; registers no custom access method"), "geo", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension H3Postgis = new("h3_postgis", new ExtensionAdmission.BaseType("h3"), "geo", FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgrouting = new("pgrouting", new ExtensionAdmission.BaseType("postgis"), "cypher", FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgPartman = new("pg_partman", new ExtensionAdmission.Preload("pg_partman_bgw"), "maintenance", FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgJsonschema = new("pg_jsonschema", new ExtensionAdmission.Standalone("CREATE EXTENSION-registered json_matches_schema/jsonb_matches_schema CHECK functions; no preload, no custom AM"), "validation", FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgNet = new("pg_net", new ExtensionAdmission.Preload("pg_net"), "egress", FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgGraphql = new("pg_graphql", new ExtensionAdmission.Standalone("pgrx SQL functions + DDL event triggers; no background worker, no preload, no custom AM"), "egress", FailureRank.Observational, RestartClass.Reload);

    public ExtensionAdmission Admission { get; }
    public string Lane { get; }
    public FailureRank Rank { get; }
    public RestartClass Restart { get; }
    private ServerExtension(string key, ExtensionAdmission admission, string lane, FailureRank rank, RestartClass restart) : this(key) =>
        (Admission, Lane, Rank, Restart) = (admission, lane, rank, restart);

    // CASCADE pulls the base-type/access-method dependency the row's `Admission` names; the install is idempotent so a
    // re-admit of a created extension is a no-op and the DDL commits with the schema migration in the one session.
    public string CreateSql => $"CREATE EXTENSION IF NOT EXISTS \"{Key}\" CASCADE;";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClusterSetting {
    public static readonly ClusterSetting WalLevel = new("wal_level", expected: "logical", atLeast: false, RestartClass.Restart);
    public static readonly ClusterSetting MaxWorkerProcesses = new("max_worker_processes", expected: "8", atLeast: true, RestartClass.Restart);
    public static readonly ClusterSetting MaxParallelWorkers = new("max_parallel_workers", expected: "8", atLeast: true, RestartClass.Reload);
    public static readonly ClusterSetting MaxParallelWorkersPerGather = new("max_parallel_workers_per_gather", expected: "4", atLeast: true, RestartClass.Reload);
    public static readonly ClusterSetting MaxReplicationSlots = new("max_replication_slots", expected: "8", atLeast: true, RestartClass.Restart);
    public static readonly ClusterSetting MaxWalSenders = new("max_wal_senders", expected: "8", atLeast: true, RestartClass.Restart);

    public string Expected { get; }
    public bool AtLeast { get; }
    public RestartClass Restart { get; }
    private ClusterSetting(string key, string expected, bool atLeast, RestartClass restart) : this(key) => (Expected, AtLeast, Restart) = (expected, atLeast, restart);

    // A min-threshold knob is satisfied at or above its floor; an exact-match knob (`wal_level`) by value equality.
    public bool Satisfied(string actual) => AtLeast
        ? long.TryParse(actual, NumberStyles.Integer, CultureInfo.InvariantCulture, out long held)
            && long.TryParse(Expected, NumberStyles.Integer, CultureInfo.InvariantCulture, out long expected)
            && held >= expected
        : string.Equals(actual, Expected, StringComparison.OrdinalIgnoreCase);
}

// --- [MODELS] ---------------------------------------------------------------------------

[ValueObject<long>]
public readonly partial struct VerificationEpoch {
    // A monotonic stamp the fold writes on every `Verify` so cluster drift between two verifications is an observable
    // epoch advance on the fact stream, never an unmarked re-probe; the AppHost health probe reads the epoch delta.
    public static VerificationEpoch From(Instant at) => From(at.ToUnixTimeMilliseconds());
}

// A repair artifact the verification EMITS but never executes: a `shared_preload_libraries` diff, a `CREATE EXTENSION`
// reconciliation, or a settings diff the operator applies at the named rung under the named restart class.
public readonly record struct RepairArtifact(string Kind, string Statement, RestartClass Restart);

// in-database maintenance-work roster: a durable store owns its maintenance PLAN, not just its extension
// roster — the append-only event/op-log history survives model scale only under partition lifecycle, scheduled
// bloat reclamation, and a server-local sweep cadence. Each row is idempotent registration SQL riding the
// gated `Register` admission (`cron.schedule` replaces by jobname, `partman.create_parent` no-ops on a declared
// parent, `squeeze` registration upserts), gated on the OWNING extension's presence so a folded-out lane
// registers nothing; a job the AppHost schedule port already owns is the rejected duplicate cadence. The
// canonical rows: the partitioned op-log rollup parent (`partman.create_parent` over the history table), the
// scheduled `partman.run_maintenance_proc` + retention-sweep heartbeat (`cron.schedule`), and the hot-table
// squeeze registration — each a data row, never a process loop.
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

public sealed record ExtensionFloor(string Minimum, Func<string, string, bool> Satisfied);

public sealed record ClusterDemand(Seq<ServerExtension> Required, HashMap<string, ExtensionFloor> Floors, VerificationEpoch Epoch);

// desired-state manifest — everything verification already asserts, egressed as ONE typed wire record the
// deploy plane converges on: the extension roster with its gates and restart classes, the verified server
// postures, the in-database maintenance-job roster, and the embedded-floor pragma/config set, the store-axis
// coordinate naming each row's `#STORE_AXIS_MAP` axis — so server drift is a diff between two typed documents,
// a fleet provisioning script derives from the manifest instead of restating the roster by hand, and
// in-process provisioning stays verification-only (the manifest DESCRIBES, `Verify` asserts, the operator applies).
public sealed record ManifestRow(string Axis, string Key, string Declared, string Restart);

public sealed record ProvisionManifest(Seq<ManifestRow> Rows, VerificationEpoch Epoch);

public sealed record SchemaContract(string Text, Json.Schema.JsonSchema Parsed) {
    public static Fin<SchemaContract> Parse(string text) {
        try { return Fin<SchemaContract>.Succ(new SchemaContract(text, Json.Schema.JsonSchema.FromText(text))); }
        catch (Json.Schema.JsonSchemaException failure) { return Fin<SchemaContract>.Fail(new ServerFault.AdmitRefused($"<schema:{failure.Message}>")); }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvisionVerdict {
    private ProvisionVerdict() { }
    public sealed record Provisioned(
        Seq<ServerExtension> Present,
        FrozenSet<string> Preloaded,
        FrozenSet<string> Created,
        FrozenSet<string> HeldLanes,
        Seq<Error> Receipts,
        VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record MissingExtension(Seq<ServerExtension> Absent, Seq<RepairArtifact> Repairs, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record MissingPreload(Seq<ServerExtension> Unloaded, RepairArtifact PreloadDiff, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record SettingDrift(string Setting, string Expected, string Actual, RestartClass Restart, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record Faulted(ServerFault Fault, VerificationEpoch Epoch) : ProvisionVerdict;

    // Only a fully verified profile opens; every repair verdict refuses until its cluster or database change lands.
    public bool Admits => this is Provisioned;
}

// --- [ERRORS] ---------------------------------------------------------------------------
// re-banded server-tier fault band (838x — `FaultBand.Server`, off the 835x Columnar collision): a [Union] over
// KERNEL `Rasm.Domain.Expected` (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from
// `Error`), the SAME federation base the seam `ElementFault` (2500) and `BimFault` (2600) realize — NOT
// `LanguageExt.Common.Expected`. No `[GenerateUnionOps]`. The decade absorbs EVERY formerly-loose provisioning
// integer as a typed case — the `FailureRank` receipts, the readiness evidence, the admission refusals — deriving
// `Code => FaultBand.Server + n` through the registry pointer so a duplicate decade integer fails at type init.
// `IsTransient` stays an abstract per-case bit (only `Unreachable` retries).
[Union]
public abstract partial record ServerFault : Expected, IValidationError<ServerFault> {
    private ServerFault() : base() { }
    public abstract bool IsTransient { get; }
    public sealed record Unmapped(string SqlState, string Detail) : ServerFault { public override bool IsTransient => false; }
    public sealed record Unreachable(string Detail) : ServerFault { public override bool IsTransient => true; }
    public sealed record CatalogDenied(string Relation) : ServerFault { public override bool IsTransient => false; }
    public sealed record RequiredAbsent(string Extension) : ServerFault { public override bool IsTransient => false; }
    public sealed record LaneFolded(string Extension) : ServerFault { public override bool IsTransient => false; }
    public sealed record Evidence(string Extension, string Detail) : ServerFault { public override bool IsTransient => false; }
    public sealed record SlotLag(long Bytes) : ServerFault { public override bool IsTransient => false; }
    public sealed record InvalidIndex(long Count) : ServerFault { public override bool IsTransient => false; }
    public sealed record Ungated(string Extension) : ServerFault { public override bool IsTransient => false; }
    public sealed record AdmitRefused(string Detail) : ServerFault { public override bool IsTransient => false; }

    public override int Code => FaultBand.Server + Switch(
        unmapped:       static _ => 0,
        unreachable:    static _ => 1,
        catalogDenied:  static _ => 2,
        requiredAbsent: static _ => 3,
        laneFolded:     static _ => 4,
        evidence:       static _ => 5,
        slotLag:        static _ => 6,
        invalidIndex:   static _ => 7,
        ungated:        static _ => 8,
        admitRefused:   static _ => 9);

    public override string Message => Switch(
        unmapped:       static c => $"<sqlstate:{c.SqlState}>:{c.Detail}",
        unreachable:    static c => $"cluster unreachable: {c.Detail}",
        catalogDenied:  static c => $"catalog read denied: {c.Relation}",
        requiredAbsent: static c => $"<required-absent:{c.Extension}>",
        laneFolded:     static c => $"<lane-folded:{c.Extension}>",
        evidence:       static c => $"<evidence:{c.Extension}:{c.Detail}>",
        slotLag:        static c => $"<slot-lag:{c.Bytes}>",
        invalidIndex:   static c => $"<invalid-indexes:{c.Count}>",
        ungated:        static c => $"<provision-ungated:{c.Extension}>",
        admitRefused:   static c => $"<provision-admit:{c.Detail}>");

    public override string Category => Switch(
        unmapped:       static _ => "Unmapped",
        unreachable:    static _ => "Unreachable",
        catalogDenied:  static _ => "CatalogDenied",
        requiredAbsent: static _ => "Absence",
        laneFolded:     static _ => "Absence",
        evidence:       static _ => "Readiness",
        slotLag:        static _ => "Readiness",
        invalidIndex:   static _ => "Readiness",
        ungated:        static _ => "Admission",
        admitRefused:   static _ => "Admission");

    public static ServerFault Create(string message) => new Unmapped("none", message);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class ClusterProvision {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.provision.verify"), StoreSlot.Create("store.provision.admit"), StoreSlot.Create("store.provision.reload"),
        StoreSlot.Create("store.embedded.open"), StoreSlot.Create("store.embedded.rekey"), StoreSlot.Create("store.embedded.checkpoint"),
        StoreSlot.Create("store.embedded.snapshot"), StoreSlot.Create("store.embedded.backup"), StoreSlot.Create("store.embedded.blob"));

    // desired-state projection folds verified server expectations with the encrypted embedded provider and
    // ritual rows; each deployment axis has one manifest declaration.
    public static ProvisionManifest Manifest(ClusterDemand demand, EmbeddedRitual ritual) => new(
        demand.Required.Map(static row => new ManifestRow("relational-sor", row.Key, row.CreateSql, row.Restart.Key))
        + toSeq(ClusterSetting.Items).Map(static row => new ManifestRow("relational-sor", row.Key, row.Expected, row.Restart.Key))
        + toSeq(MaintenanceJob.Items).Map(static row => new ManifestRow("maintenance", row.Key, row.RegisterSql, row.Owner.Restart.Key))
        + Seq(new ManifestRow("embedded-relational", "<cipher-provider>", "SQLitePCLRaw.bundle_e_sqlite3mc", RestartClass.Restart.Key))
        + ritual.ConnectionRows.Map(static row => new ManifestRow("embedded-relational", row.Row, row.Sql, RestartClass.Session.Key))
        + ritual.DbConfig.Map(static row => new ManifestRow("embedded-relational", row.Row, row.Value.ToString(CultureInfo.InvariantCulture), RestartClass.Session.Key)),
        demand.Epoch);

    // `floors` is deployment DATA — extension key -> minimum installed version the deployment demands (never a
    // literal in this fence); a created extension whose `pg_extension.extversion` trails its floor threads an
    // `Evidence` receipt, so a stale binary is visible at admission rather than at the first missing function.
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
            FrozenSet<string> preloaded = (await reader.ReadAsync().ConfigureAwait(false) ? reader.GetString(0) : "")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToFrozenSet(StringComparer.Ordinal);
            IReadOnlyDictionary<string, string> versions = await DrainPairs(reader).ConfigureAwait(false);
            FrozenSet<string> created = versions.Keys.ToFrozenSet(StringComparer.Ordinal);
            FrozenSet<string> available = await Drain(reader, static r => r.GetString(0)).ConfigureAwait(false);
            IReadOnlyDictionary<string, string> settings = await DrainPairs(reader).ConfigureAwait(false);
            long slotLag = await Scalar(reader).ConfigureAwait(false);
            long invalidIndexes = await Scalar(reader).ConfigureAwait(false);
            return Fold(demand, preloaded, created, versions, available, settings, slotLag, invalidIndexes);
        }) | @catch<IO, ProvisionVerdict>(static _ => true, e => IO.pure(Folded(e, demand.Epoch)));

    // fold is total over the verdict family: preload gaps EMIT a `shared_preload_libraries` diff and refuse, an
    // installable-but-uncreated set (present in `pg_available_extensions`, gate satisfied) is the operator's
    // `CREATE EXTENSION` reconciliation, a drifted setting carries its `RestartClass`, and the survivors fold their held
    // analytical lanes and the `FailureRank` receipts and the readiness evidence (slot lag, invalid indexes) — never a
    // per-extension `Switch`, the absence policy living on the rank row.
    static ProvisionVerdict Fold(ClusterDemand demand, FrozenSet<string> preloaded, FrozenSet<string> created, IReadOnlyDictionary<string, string> versions, FrozenSet<string> available, IReadOnlyDictionary<string, string> settings, long slotLag, long invalidIndexes) {
        Seq<ServerExtension> missingPreload = demand.Required.Filter(e => e.Admission is ExtensionAdmission.Preload p && !preloaded.Contains(p.Library));
        if (!missingPreload.IsEmpty) {
            Seq<string> libraries = missingPreload.Choose(e => e.Admission.PreloadLibrary).Distinct();
            RepairArtifact diff = new("shared_preload_libraries", $"shared_preload_libraries = '{string.Join(',', preloaded.Concat(libraries))}'", RestartClass.Max(missingPreload.Map(static e => e.Restart)));
            return new ProvisionVerdict.MissingPreload(missingPreload, diff, demand.Epoch);
        }
        // An installable-but-uncreated row is present in `pg_available_extensions` AND its gate is satisfied — that set is
        // operator's `CREATE EXTENSION` reconciliation; a row whose library is unavailable on disk is NOT here (it
        // routes to the survivor fold's `FailureRank.Absorb` because no `CREATE EXTENSION` repair can fix a missing binary).
        Seq<ServerExtension> missing = demand.Required.Filter(e => !created.Contains(e.Key) && available.Contains(e.Key) && e.Admission.Admissible(preloaded, created));
        if (!missing.IsEmpty) {
            return new ProvisionVerdict.MissingExtension(missing, missing.Map(e => new RepairArtifact("create_extension", e.CreateSql, e.Restart)), demand.Epoch);
        }
        Option<ClusterSetting> drift = toSeq(ClusterSetting.Items).Find(s => !s.Satisfied(settings.GetValueOrDefault(s.Key, "")));
        if (drift.IsSome) {
            ClusterSetting setting = drift.ValueUnsafe()!;
            return new ProvisionVerdict.SettingDrift(setting.Key, setting.Expected, settings.GetValueOrDefault(setting.Key, ""), setting.Restart, demand.Epoch);
        }
        // survivor fold iterates the FULL required set: a created extension is `Held`, an uncreated row whose binary is
        // absent from `pg_available_extensions` (or whose gate is unmet) threads its `FailureRank.Absorb` — a `Required`
        // rank absorbing to `Fail` aborts the verdict to a `MissingExtension` (no admissible repair exists), a
        // `Degradable`/`Observational` rank records the receipt and the held lanes still compose. Readiness evidence — a
        // lagging replication slot (server-disk liability) and any invalid index (an interrupted concurrent build) — folds
        // in as `Observational` receipts on the held verdict, never refusing the profile but visible on the fact stream.
        Seq<Error> readiness = (slotLag > 0 ? Seq<Error>(new ServerFault.SlotLag(slotLag)) : Seq<Error>())
            + (invalidIndexes > 0 ? Seq<Error>(new ServerFault.InvalidIndex(invalidIndexes)) : Seq<Error>())
            + demand.Floors.ToSeq().Choose(floor => versions.TryGetValue(floor.Key, out string? held) && !floor.Value.Satisfied(held, floor.Value.Minimum)
                ? Some((Error)new ServerFault.Evidence(floor.Key, $"version:{held}<{floor.Value.Minimum}"))
                : None);
        (Seq<ServerExtension> Held, Seq<Error> Receipts, Seq<ServerExtension> Absent) fold = demand.Required.Fold(
            (Held: Seq<ServerExtension>(), Receipts: readiness, Absent: Seq<ServerExtension>()),
            (acc, e) => created.Contains(e.Key)
                ? (acc.Held.Add(e), acc.Receipts, acc.Absent)
                : e.Rank.Absorb(acc.Receipts, e.Key).Match(
                    Succ: r => (acc.Held, r, acc.Absent),
                    Fail: r => (acc.Held, acc.Receipts.Add(r), acc.Absent.Add(e))));
        return fold.Absent.IsEmpty
            ? new ProvisionVerdict.Provisioned(fold.Held, preloaded, created, fold.Held.Map(static e => e.Lane).ToFrozenSet(StringComparer.Ordinal), fold.Receipts, demand.Epoch)
            : new ProvisionVerdict.MissingExtension(fold.Absent, Seq<RepairArtifact>(), demand.Epoch);
    }

    static ProvisionVerdict Folded(Error error, VerificationEpoch epoch) =>
        new ProvisionVerdict.Faulted(error is PostgresException { SqlState: PostgresErrorCodes.InsufficientPrivilege } denied
            ? new ServerFault.CatalogDenied(denied.TableName ?? "pg_catalog")
            : error is NpgsqlException { IsTransient: true } transient
                ? new ServerFault.Unreachable(transient.Message)
                : new ServerFault.Unmapped(error is PostgresException pg ? pg.SqlState : "none", error.Message), epoch);

    // Admission RE-GATES at the entry, never trusts the caller pre-filtered: an extension whose `ExtensionAdmission`
    // gate the live cluster does not satisfy (a preload library absent, a base type uncreated) REFUSES with no DDL queued
    // — a `CREATE EXTENSION` against an unmet gate is a guaranteed runtime error, so the gate is the precondition the
    // entry owns rather than an assumption the verdict carried. The `preloaded`/`created` sets are the ones the caller's
    // `Verify` fold already read (no second catalog probe), so the gate costs nothing beyond a set membership test.
    public static IO<Fin<Unit>> Admit(IDocumentSession session, ServerExtension extension, ProvisionVerdict.Provisioned cluster) =>
        !extension.Admission.Admissible(cluster.Preloaded, cluster.Created)
            ? IO.pure(Fin<Unit>.Fail(new ServerFault.Ungated(extension.Key)))
            : (IO.liftAsync(async () => {
                session.QueueSqlCommand(extension.CreateSql);
                await session.SaveChangesAsync().ConfigureAwait(false);
                return Fin<Unit>.Succ(unit);
            }) | @catch<IO, Fin<Unit>>(static _ => true, e => IO.pure(Fin<Unit>.Fail(new ServerFault.AdmitRefused(e.Message))))).As();

    // Deployment completes when live processes re-resolve the wire types a freshly-admitted enum/composite/extension
    // introduced — `ReloadTypesAsync` on the owning source — not when the DDL commits (`#SERVER_EXTENSIONS` deploy law).
    public static IO<Unit> Reload(NpgsqlDataSource source) =>
        IO.liftAsync(async () => { await source.ReloadTypesAsync().ConfigureAwait(false); return unit; });

    // Maintenance registration rides the SAME gated-admission discipline as `Admit`: the job's owning extension
    // must be created (a folded-out lane registers nothing — `Ungated`), the idempotent registration SQL queues
    // on the one session, and the commit rides `SaveChangesAsync` with the schema migration. The registration
    // is verification-compatible — it writes only extension-owned registration rows (`cron.job`,
    // `partman.part_config`, squeeze registration), never a cluster setting, so the never-`ALTER SYSTEM` law holds.
    public static IO<Fin<Unit>> Register(IDocumentSession session, MaintenanceJob job, ProvisionVerdict.Provisioned cluster) =>
        !cluster.Created.Contains(job.Owner.Key)
            ? IO.pure(Fin<Unit>.Fail(new ServerFault.Ungated(job.Owner.Key)))
            : (IO.liftAsync(async () => {
                session.QueueSqlCommand(job.RegisterSql);
                await session.SaveChangesAsync().ConfigureAwait(false);
                return Fin<Unit>.Succ(unit);
            }) | @catch<IO, Fin<Unit>>(static _ => true, e => IO.pure(Fin<Unit>.Fail(new ServerFault.AdmitRefused(e.Message))))).As();

    // Server row's data-source build ([05] Npgsql.NetTopologySuite): the ADO spatial codec composes ONCE on
    // owning NpgsqlDataSourceBuilder from the `SpatialWire` policy row, so every raw lane (cypher pgrouting
    // decode, verification probes, QueueSqlCommand spatial writes) reads/writes NTS geometry — the EF plugin
    // never places the codec on raw connections, so this row is the wire's one admission site. `Name`
    // (`string?`, get/set) assigns the logical-database identity here — the Persistence half of the PORT-peer
    // telemetry split: `db.client.connection.pool.name` keys stable pool dimensions on the `Npgsql` meter the
    // AppHost root subscribes, and an unnamed source collapses every pool into one anonymous series.
    public static NpgsqlDataSource Source(string dsn, string name, SpatialWire wire) {
        NpgsqlDataSourceBuilder builder = new(dsn) { Name = name };
        builder.UseNetTopologySuite(handleOrdinates: wire.HandleOrdinates, geographyAsDefault: wire.GeographyAsDefault);
        return builder.Build();
    }

    // pg_jsonschema dual-residence fence ([05] JsonSchema.Net): ONE schema text serves both residences —
    // held `validation` lane checks server-side (`json_matches_schema` in a CHECK/predicate), and a folded-out
    // lane degrades to the in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` boolean
    // parity gate, so absence of the server extension narrows residence, never capability.
    public static Fin<bool> SchemaCheck(FrozenSet<string> heldLanes, SchemaContract schema, JsonElement instance, Func<string, JsonElement, bool> serverCheck) {
        try {
            return Fin<bool>.Succ(heldLanes.Contains("validation")
                ? serverCheck(schema.Text, instance)
                : schema.Parsed.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Flag }).IsValid);
        } catch (Json.Schema.RefResolutionException failure) {
            return Fin<bool>.Fail(new ServerFault.AdmitRefused($"<schema-ref:{failure.Message}>"));
        }
    }

    static async Task<FrozenSet<string>> Drain(NpgsqlDataReader reader, Func<NpgsqlDataReader, string> read) {
        HashSet<string> rows = new(StringComparer.Ordinal);
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add(read(reader)); }   // Exemption: ADO read loop fills a seam-local set frozen once on return
        return rows.ToFrozenSet(StringComparer.Ordinal);
    }

    // one two-column pair drain — extension name/version and setting name/value share it.
    static async Task<IReadOnlyDictionary<string, string>> DrainPairs(NpgsqlDataReader reader) {
        Dictionary<string, string> pairs = new(StringComparer.Ordinal);
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false)) { pairs[reader.GetString(0)] = reader.GetString(1); }   // Exemption: ADO read loop fills a seam-local map frozen once on return
        return pairs;
    }

    // slot-lag and invalid-index readings are single-row scalar aggregates: advance to the result set and read the
    // one `bigint`/`int` cell, defaulting to 0 on an empty set so a cluster with no slots/no invalid indexes reads clean.
    static async Task<long> Scalar(NpgsqlDataReader reader) {
        await reader.NextResultAsync().ConfigureAwait(false);
        return await reader.ReadAsync().ConfigureAwait(false) ? reader.GetInt64(0) : 0L;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                           | [BIND]                                               |
| :-----: | :------------------ | :------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | provisioning stance | verification-first                                | never `ALTER SYSTEM`; never spawns PG                |
|  [02]   | verification cost   | one six-command `CreateBatch` round trip          | data-volume-independent; no ext probe                |
|  [03]   | absence policy      | `FailureRank.Absorb` delegate                     | required/degradable/observational tiers              |
|  [04]   | install gate        | `ExtensionAdmission` (preload/type/AM/standalone) | `.api`-verified; CASCADE pulls dependency            |
|  [05]   | preload gap         | `MissingPreload` + emitted diff                   | resolves at cluster config; restart class            |
|  [06]   | setting drift       | `pg_settings` vs `ClusterSetting`                 | folds `SettingDrift` + `RestartClass`                |
|  [07]   | repair posture      | EMIT artifacts, never execute                     | grants + settings diffs are typed outputs            |
|  [08]   | drift visibility    | stamped `VerificationEpoch`                       | re-verify advance = health-probe event               |
|  [09]   | deploy completion   | `ReloadTypesAsync`                                | types re-resolve before deploy is done               |
|  [10]   | h3 parity           | `h3-pg`/`h3_postgis` match `pocketken.H3`         | one cell id at ingest and in SQL                     |
|  [11]   | spatial wire        | `SpatialWire` policy row on `Source`              | ADO codec composed once; literals deleted            |
|  [12]   | EF provider bind    | `StoreProfile.Ef` row data                        | one identity DbContext, two providers                |
|  [13]   | observability       | `AddNpgsql`/`AddNpgsqlInstrumentation`            | AppHost composition root, not in-fence               |
|  [14]   | schema validation   | `SchemaCheck` dual residence                      | `json_matches_schema` or `Evaluate` fallback         |
|  [15]   | fault typing        | 838x `ServerFault` whole decade                   | registry-derived absence/readiness/admission         |
|  [16]   | version floors      | `floors` deployment data vs `extversion`          | below-floor threads an `Evidence` receipt            |
|  [17]   | maintenance roster  | `MaintenanceJob` rows via gated `Register`        | cron/partman/squeeze registration; no loop           |
|  [18]   | desired-state wire  | `Manifest(demand, ritual)` typed projection       | drift diffs two documents; no second expectation set |

## [03]-[EMBEDDED_FLOOR]

- Owner: `EmbeddedRitual` the idempotent open-ritual record carrying the file-persistent provisioning rows, the per-connection pragma rows, the defensive `DbConfig` set, and the connection-scoped `Capability` registrations (each a named `Action<SqliteConnection>` grant); `EmbeddedStore` the static surface owning the dialed connection, the KMS-custodied key application, the residency-split fold, the first-opener IMMEDIATE migration gate, the rekey rotation, and the closed-engine law — the bound provider is the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle (`Batteries_V2.Init()` binds `SQLite3Provider_e_sqlite3mc`; one provider per process), so the embedded floor is ENCRYPTED at rest wherever a data key is supplied and the plain open is the same ritual with the key slot `None`.
- Cases: the ritual's `ConnectionRows` are the per-connection pragmas (`synchronous=NORMAL`, `journal_size_limit`, `temp_store=MEMORY`, `cache_size`) the fold re-applies on every open; the `Capabilities` are the schema-resident registrations (`uuid7`/`xxh128` scalar UDFs and the `instant_iso` collation the identity policy and chronological ordering need, a domain aggregate) that register before the first statement or the file is unreadable; the `DbConfig` set is the defensive-mode + double-quoted-literal-rejection posture applied through the raw `Handle`; the file-persistent `application_id`/`user_version` are provisioning identity the migration gate writes, never per-connection.
- Entry: `public static SqliteConnection Dialed(string path)` opens a non-pooled embedded connection with the canonical connection-string posture (`ForeignKeys`, `ReadWriteCreate`); `public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual, Option<ReadOnlyMemory<byte>> dek, Action<SqliteConnection, SqliteTransaction, long> migrate)` folds the declared ritual end-to-end idempotently — the supplied data key applies FIRST through `raw.sqlite3_key(handle, dek.Span)` before any statement touches a data page (the `Element/identity#KMS_CUSTODY` `EnvelopeKeyring.Unwrap` recovers it and the caller zeroizes through `CryptographicOperations.ZeroMemory` after the keyed open, so no passphrase persists past the crossing); `public static Fin<Unit> Rekey(SqliteConnection store, ReadOnlyMemory<byte> next)` rotates the store key in place through `raw.sqlite3_rekey(handle, next.Span)` after a fresh custody mint — an empty `next` strips the cipher for a plaintext export — identity check, per-connection pragma rows, defensive `sqlite3_db_config` hardening, extended-result-code arming, capability registration, the IMMEDIATE migration gate, the epoch read — every throwing provider call staying INSIDE the `Fin` boundary so a provider fault converts to `EmbeddedFault` and disposes the connection on every failure path rather than escaping with a leaked live handle; `migrate` is the first-opener step run under the one IMMEDIATE transaction when the held epoch trails the compiled epoch.
- Auto: every connection in every process folds the SAME declared sequence so bootstrap, crash-recovery reopen, and steady-state open are one fold with no first-process special case — the identity check rejects a foreign `application_id`, the per-connection pragma rows apply (`synchronous=NORMAL` the WAL throughput row whose loss boundary is the last commits and never corruption), the defensive `sqlite3_db_config(Handle, SQLITE_DBCONFIG_DEFENSIVE, 1)` and `DQS_DDL=0`/`DQS_DML=0` harden against direct b-tree writes and double-quoted string literals (so a double-quoted literal is a prepare-time syntax error, identifiers quoting with `"` and strings with `'`), `sqlite3_extended_result_codes(Handle, 1)` upgrades the running taxonomy where receipts must discriminate (`BUSY_SNAPSHOT` from plain `BUSY`), the capabilities register connection-instance-scoped (never persisted — `isDeterministic: true` admits the UDF into expression indexes and generated columns), the first-opener migration runs the `migrate` step under one IMMEDIATE transaction (losers blocked on the lock observe the bumped `user_version` on acquisition and no-op, a register ahead of the compiled epoch a typed rejection so correctness needs no leader election), and `PRAGMA data_version` is the polling-free cross-process change probe `EngineOps` reads; any write transaction begins IMMEDIATE so a deferred read attempting its first write never burns the busy budget on a stale-snapshot retry, and the provider already retries `BUSY`/`LOCKED` at managed quanta so a nonzero `busy_timeout` is the deleted form.
- Receipt: an open rides `store.embedded.open` carrying the ritual fact count, the keyed bit, and the epoch; a rotation rides `store.embedded.rekey` carrying the wrapping-key version advance, never key material.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`/`CreateFunction`/`CreateAggregate`/`CreateCollation`/`BeginTransaction(IsolationLevel, deferred)`), SQLitePCLRaw.bundle_e_sqlite3mc (`Batteries_V2.Init()` binding `SQLite3Provider_e_sqlite3mc`; the keying delta `raw.sqlite3_key(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_key_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`; the carried-over raw surface `raw.sqlite3_db_config(sqlite3, int, int, out int)`/`raw.sqlite3_extended_result_codes`/`raw.SQLITE_DBCONFIG_DEFENSIVE`=1010/`raw.SQLITE_DBCONFIG_DQS_DDL`=1014/`raw.SQLITE_DBCONFIG_DQS_DML`=1013 — backup, snapshot, WAL, db_config, and serialize calls carry over the `mc` provider unchanged), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new pragma is one `ConnectionRows` row; a new capability is one `Capabilities` registration; a new defensive posture is one `DbConfig` row; zero new surface — a second embedded relational engine (libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory), a per-process bootstrap branch, a nonzero `busy_timeout`, a persisted capability, or a `locking_mode=EXCLUSIVE`/shared-cache posture is the deleted form because the engine sweep is closed, the ritual is the one open path, and the provider already retries `BUSY`/`LOCKED`.
- Boundary: the embedded SQLite floor is the single-process embedded store beneath the server tier — the one engine sweep is CLOSED (PostgreSQL + embedded SQLite only; libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory all rejected) so a new engine row is the named defect, and the embedded floor and the PostgreSQL server tier are two engines on the one `StoreProfile` axis (`#SERVER_EXTENSIONS` `StoreProfile`), the profile selecting one by deployment, never a third; pragma rows carry RESIDENCY — file-persistent rows (`journal_mode`, `application_id`, `user_version`) are provisioning identity the migration gate writes and the ritual folds ONLY per-connection rows; capability registration is connection-instance-scoped and never persisted — schema-resident functions, aggregates, and collations register before the first statement or the file is unreadable, and `isDeterministic: true` is the capability grant admitting a function into expression indexes and generated columns; every embedded connection is non-pooled because a physical handle's cipher identity is fixed by its first key bind and path-only pooling can return a handle keyed under different material; the WAL `-wal`/`-shm` sidecar set is the unit of copy/replace/delete (a main file separated from its sidecars is silent page-level corruption); STRICT tables are the typed admission gate and `RETURNING` supersedes write-then-read identity round trips; the defensive `sqlite3_db_config` set and double-quoted-literal rejection are connection POLICY applied through the `Handle` raw bridge (`api-sqlite#HANDLE_BRIDGE`), not connection-string knobs; extension loading stays FULLY disabled — the `Canonical` ritual arms neither the SQL `load_extension()` function nor the C-API loader (`SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` is absent from the `DbConfig` set), so the bundled floor admits no runtime extension and a `DbConfig` row arming the loader is the deliberate opt-in a deployment that needs one adds, never the default; encryption-at-rest is the BOUND provider's law — the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle supersedes the plain `e_sqlite3` bundle where the encrypted floor mounts (one provider binds per process, so the selection is this provisioning row, never a per-connection knob), key material is the KMS-unwrapped DEK crossing as `ReadOnlySpan<byte>` through `raw.sqlite3_key` and zeroized after the bind, a `Password=` connection-string value exists only for the ephemeral open of an inspected foreign store and never enters durable configuration, and classification ceilings thereby extend to the offline lane — a stolen laptop or synced file leaks nothing; the ritual is the one open path so a per-process bootstrap branch is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using System.Data;
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using static LanguageExt.Prelude;

// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct RitualFact(string Row, long Applied);

public sealed record EmbeddedRitual(
    long Identity,
    long CompiledEpoch,
    Seq<(string Row, string Sql)> ConnectionRows,
    Seq<(string Row, int Op, int Value)> DbConfig,
    Seq<(string Row, Action<SqliteConnection> Grant)> Capabilities) {

    // canonical ritual: the per-connection WAL/throughput pragmas, the defensive `sqlite3_db_config` posture
    // (DEFENSIVE on, double-quoted DDL/DML off), and the schema-resident identity/chronology capabilities — every row
    // re-applied per physical open so two processes' rituals diff as two declarations.
    public static readonly EmbeddedRitual Canonical = new(
        Identity: 0x5241_5731, CompiledEpoch: 1,
        ConnectionRows: [
            ("<throughput>", "PRAGMA synchronous=NORMAL"), ("<wal-bound>", "PRAGMA journal_size_limit=8388608"),
            ("<spill>", "PRAGMA temp_store=MEMORY"), ("<budget>", "PRAGMA cache_size=-32768")],
        DbConfig: [
            ("<defensive>", raw.SQLITE_DBCONFIG_DEFENSIVE, 1), ("<dqs-ddl>", raw.SQLITE_DBCONFIG_DQS_DDL, 0),
            ("<dqs-dml>", raw.SQLITE_DBCONFIG_DQS_DML, 0)],
        Capabilities: [
            ("<uuid7>", static store => store.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("N"), isDeterministic: false)),
            ("<xxh128>", static store => store.CreateFunction("xxh128", static (byte[] bytes) => unchecked((long)(ulong)System.IO.Hashing.XxHash128.HashToUInt128(bytes)), isDeterministic: true)),
            ("<instant-iso>", static store => store.CreateCollation("instant_iso", static (left, right) => string.CompareOrdinal(left, right))),
            ("<span-fold>", static store => store.CreateAggregate("span_fold", 0L, static (long held, long next) => long.Max(held, next), isDeterministic: true))]);
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EmbeddedStore {
    static EmbeddedStore() => Batteries_V2.Init();

    public static SqliteConnection Dialed(string path) => new(new SqliteConnectionStringBuilder {
        DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = false, ForeignKeys = true,
    }.ConnectionString);

    public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual, Option<ReadOnlyMemory<byte>> dek, Action<SqliteConnection, SqliteTransaction, long> migrate) {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(ritual);
        ArgumentNullException.ThrowIfNull(migrate);
        try {
            store.Open();
            sqlite3 handle = store.Handle ?? throw new InvalidOperationException("<no-handle>");
            // Key application is the FIRST crossing after the physical open — before any statement touches a
            // data page. The DEK arrives from EnvelopeKeyring.Unwrap; the caller zeroizes it after this returns.
            _ = dek.Map(key => raw.sqlite3_key(handle, key.Span) is raw.SQLITE_OK
                ? 1L
                : throw new InvalidOperationException("<key-refused>"));
            long identity = Scalar(store, "PRAGMA application_id");
            if (identity != ritual.Identity && identity != 0L) { return Refused(store, $"<foreign-store:{identity:x8}>"); }
            Seq<RitualFact> facts = ritual.ConnectionRows.Map(row => new RitualFact(row.Row, Execute(store, row.Sql)));
            _ = raw.sqlite3_extended_result_codes(handle, 1);
            facts += ritual.DbConfig.Map(row => new RitualFact(row.Row, raw.sqlite3_db_config(handle, row.Op, row.Value, out int applied) == raw.SQLITE_OK ? applied : -1L));
            facts += ritual.Capabilities.Map(row => (fun(() => row.Grant(store))(), new RitualFact(row.Row, 1L)).Item2);
            using SqliteTransaction gate = store.BeginTransaction(IsolationLevel.Serializable, deferred: false);
            long held = Scalar(store, "PRAGMA user_version", gate);
            if (held > ritual.CompiledEpoch) { return Refused(store, $"<epoch-ahead:{held}>"); }
            if (held < ritual.CompiledEpoch) {
                migrate(store, gate, held);
                _ = Execute(store, $"PRAGMA application_id={ritual.Identity}", gate);
                _ = Execute(store, $"PRAGMA user_version={ritual.CompiledEpoch}", gate);
            }
            gate.Commit();
            return Fin.Succ(facts.Add(new RitualFact("<epoch>", long.Max(held, ritual.CompiledEpoch))));
        }
        catch (Exception ex) {
            store.Dispose();
            return Fin.Fail<Seq<RitualFact>>(EmbeddedFault.Lift(ex));
        }
    }

    // Key rotation without an app-layer re-encrypt: one raw call on the open keyed connection after a fresh
    // KMS mint (`Custody.Wrap` -> new DEK), the wrapped envelope persisting beside the store; an EMPTY `next`
    // strips the cipher for a plaintext export. The plaintext never persists — the caller zeroizes both keys.
    // Native crossing rides the Try capture so a provider throw converts to `EmbeddedFault` inside the `Fin`
    // boundary, and a non-OK status discriminates through `FromStatus` — a BUSY rekey stays the transient retry
    // class instead of flattening into `Refused`.
    public static Fin<Unit> Rekey(SqliteConnection store, ReadOnlyMemory<byte> next) {
        ArgumentNullException.ThrowIfNull(store);
        return store.Handle is { } handle
            ? Try.lift(() => raw.sqlite3_rekey(handle, next.Span) is var status && status is raw.SQLITE_OK
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(EmbeddedFault.FromStatus(status, "<rekey-refused>")))
                .Run()
                .MapFail(static error => (Error)EmbeddedFault.Lift(error.ToException()))
                .Bind(static result => result)
            : Fin.Fail<Unit>(new EmbeddedFault.Refused("<no-handle>"));
    }

    // refusal arms are TYPED — a foreign `application_id` and an epoch-ahead register both rail
    // `EmbeddedFault.Refused` (7714, in-band 771x); the loose `Error.New(7701/7702)` integers are the deleted form.
    static Fin<Seq<RitualFact>> Refused(SqliteConnection store, string detail) =>
        (fun(store.Dispose)(), Fin.Fail<Seq<RitualFact>>(new EmbeddedFault.Refused(detail))).Item2;

    // `gate` threads the live IMMEDIATE transaction — Microsoft.Data.Sqlite REFUSES a command whose connection
    // holds an active transaction the command does not name, so an unassigned `Transaction` inside the gate throws.
    static long Execute(SqliteConnection store, string sql, SqliteTransaction? gate = null) { using SqliteCommand command = store.CreateCommand(); command.Transaction = gate; command.CommandText = sql; return command.ExecuteNonQuery(); }
    static long Scalar(SqliteConnection store, string sql, SqliteTransaction? gate = null) { using SqliteCommand command = store.CreateCommand(); command.Transaction = gate; command.CommandText = sql; return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture); }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                 | [BINDING]                                             |
| :-----: | :---------------- | :-------------------------------------- | :---------------------------------------------------- |
|  [01]   | open ritual       | one idempotent fold                     | bootstrap/recovery/steady-state are one path          |
|  [02]   | pragma residency  | per-connection rows only                | file-persistent rows are the migration gate's         |
|  [03]   | hardening         | `sqlite3_db_config` defensive + DQS off | connection policy via `Handle`, not connection-string |
|  [04]   | capability scope  | connection-instance registration        | UDFs/collation/aggregate; never persisted             |
|  [05]   | migration gate    | first-opener IMMEDIATE transaction      | losers observe the bumped epoch; no leader election   |
|  [06]   | write transaction | IMMEDIATE begin                         | a deferred-then-write burns the busy budget           |
|  [07]   | engine sweep      | closed (PostgreSQL + SQLite only)       | a new embedded engine row is the named defect         |
|  [08]   | sidecar unit      | `-wal`/`-shm` set                       | a main file without its sidecars is silent corruption |
|  [09]   | cipher floor      | `e_sqlite3mc` + KMS-unwrapped DEK       | `sqlite3_key` first crossing; `sqlite3_rekey` rotates |
|  [10]   | key custody       | `KMS_CUSTODY` envelope algebra          | plaintext zeroized after bind; never persisted        |

## [04]-[ENGINE_OPERATIONS]

- Owner: `HandleBridge` projects `SqliteConnection.Handle` into typed `Fin` results; `CheckpointMode`, `SnapshotFloor`, `BackupPolicy`, `BlobBinding`, `EmbeddedFact`, and `EmbeddedFault` carry the native operation policy, lifetime, validation, target, receipt, and closed fault family. `EngineOps` owns checkpoint, consistent snapshot, validated paged backup, preallocated blob IO, and integrity. `KvEngine` and `KvFloor` realize the RocksDB pending-op spool and LMDB local content-address index through one polymorphic `Put`/`Get`/`Batch` surface; remote object residence remains exact-object provider evidence and never consults this local index.
- Cases: `CheckpointMode` is `Passive`/`Full`/`Restart`/`Truncate` (the `raw.SQLITE_CHECKPOINT_*` modes — `Truncate` the scheduled WAL-bound reset); `EmbeddedFault` is `Busy` (a `SQLITE_BUSY`/`SQLITE_LOCKED` retry signal, the only transient case), `Corrupt` (`SQLITE_CORRUPT`/`SQLITE_NOTADB`, terminal — routes to `Version/recovery`), `Io` (`SQLITE_IOERR`/`SQLITE_FULL`), and `Refused` (a foreign store / epoch-ahead / pin regression) so a status `int` discriminates structurally rather than collapsing to one token; the integrity ladder orders boot `quick_check`, cycle `integrity_check` and `foreign_key_check`, a deeper-tier failure routing to restore, never retry.
- Entry: `Checkpoint(SqliteConnection, SnapshotFloor, CheckpointMode, ProjectionContext)` resets only the owning store's promoted pin on `Truncate`; `WithSnapshot<T>(SqliteConnection, SnapshotFloor, Func<SqliteConnection,T>)` promotes a comparable snapshot into that same disposable lifetime owner. `Backup(SqliteConnection, string, BackupPolicy, ProjectionContext)` pages until completion, returns `Busy` without spinning, then requires destination `PRAGMA quick_check` and the policy's source/destination `ContentAddress` equality. `WriteBlob(SqliteConnection, BlobBinding, long, ReadOnlyMemory<byte>)` executes the binding's parameterized `zeroblob(@length)` row preallocation before opening `SqliteBlob`; `DataVersion` reads the cross-process change register.
- Auto: each `SnapshotFloor` scopes native comparison and disposal to one store instead of comparing process-global handles from unrelated databases. Backup policy owns page quantum and semantic identity; `SQLITE_BUSY`/`SQLITE_LOCKED` returns to the schedule rather than hot-spinning inside the native loop. Blob target identifiers arrive only through a composition-time `BlobBinding`, while row id and length remain parameters. LMDB checks every `MDBResultCode`, maps only `NotFound` to `None`, and admits a write only after `Commit` succeeds; RocksDB keeps span-first IO and atomic `WriteBatch`.
- Receipt: a checkpoint rides `store.embedded.checkpoint` carrying the mode and frame counts; a snapshot read rides `store.embedded.snapshot`; a backup rides `store.embedded.backup` carrying the page progress; a blob write rides `store.embedded.blob` carrying the byte count.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`, `SqliteBlob(connection, table, column, rowid, readOnly)`, `BackupDatabase`), SQLitePCLRaw.bundle_e_sqlite3mc (`raw.sqlite3_wal_checkpoint_v2`, `raw.sqlite3_snapshot_get`/`_open`/`_cmp`/`_recover`/`_free`, `raw.sqlite3_backup_init`/`_step`/`_remaining`/`_pagecount`/`_finish`, `raw.sqlite3_extended_errcode`, `raw.sqlite3_errstr`, the `SQLITE_CHECKPOINT_*`/`SQLITE_BUSY`/`SQLITE_CORRUPT`/`SQLITE_DONE` constants), rocksdb (`RocksDb.Open`, span `Get`/`Put`, `WriteBatch`, `RocksDb.Write`), LightningDB (`LightningEnvironment`, `BeginTransaction`, `LightningTransaction.Get`/`Put`/`Delete`/`Commit`, `MDBResultCode`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new checkpoint mode is one `CheckpointMode` row; a new boundary cause is one `EmbeddedFault` case; a new integrity tier is one ladder row; zero new surface — the whole-file `BackupDatabase` where the paged session adds progress facts, a whole-payload `byte[]` blob materialization, a second hashing path beside the registered `xxh128` UDF, a bare `Error.New(ex)` flattening the status int, or a snapshot regression unguarded by `sqlite3_snapshot_cmp` is the deleted form because the paged backup subsumes the whole-file copy, the blob streams through `SqliteBlob`, and the status int discriminates structurally.
- Boundary: `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) is the one seam joining the managed ADO surface to raw operations, and the bound `e_sqlite3mc` provider keeps raw calls and ADO statements on the same native connection; every native crossing rides inside `HandleBridge` so the cause stays a closed `EmbeddedFault` case; the WAL sidecar set is the unit of backup, snapshot pins and truncating checkpoints remain adversaries, integrity failures route to `Version/recovery`, and blob IO streams through `SqliteBlob` without whole-payload materialization.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using LightningDB;
using Microsoft.Data.Sqlite;
using NodaTime;
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using RocksDbSharp;
using SQLitePCL;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using static LanguageExt.Prelude;

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<int>]
public sealed partial class CheckpointMode {
    public static readonly CheckpointMode Passive = new(raw.SQLITE_CHECKPOINT_PASSIVE);
    public static readonly CheckpointMode Full = new(raw.SQLITE_CHECKPOINT_FULL);
    public static readonly CheckpointMode Restart = new(raw.SQLITE_CHECKPOINT_RESTART);
    public static readonly CheckpointMode Truncate = new(raw.SQLITE_CHECKPOINT_TRUNCATE);
}

// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct EmbeddedFact(string Kind, long First, long Second, Instant At);

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
    Func<SqliteConnection, Fin<ContentAddress>> Identity);

public readonly record struct BlobBinding(string Table, string Column, string PreallocateSql);

// --- [ERRORS] ---------------------------------------------------------------------------
// closed embedded-boundary fault band (771x): a [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no
// `Category` to override) is the deleted form. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly
// opt-in, so the band carries no per-case `SelfOp` and the `[Union]`-generated `Switch`/`Map` is untouched; band membership
// derives `Code => FaultBand.Embedded + n` through the registry pointer (a bare 771x literal beside the registry row is
// decoupled form the sibling bands reject) and `Category` is the telemetry
// label, so the case lifts BARE onto `Fin<T>` with no `.ToError()` hop. `IsTransient` stays an abstract per-case bit (the
// retry gate — only Busy is transient) the nested records override.
[Union]
public abstract partial record EmbeddedFault : Expected, IValidationError<EmbeddedFault> {
    private EmbeddedFault() : base() { }
    public abstract bool IsTransient { get; }
    public sealed record Busy(int Status) : EmbeddedFault { public override bool IsTransient => true; }
    public sealed record Corrupt(int Status, string Detail) : EmbeddedFault { public override bool IsTransient => false; }
    public sealed record Io(int Status, string Detail) : EmbeddedFault { public override bool IsTransient => false; }
    public sealed record Refused(string Detail) : EmbeddedFault { public override bool IsTransient => false; }
    public sealed record Kv(string Engine, string Status, string Detail) : EmbeddedFault { public override bool IsTransient => false; }

    public override int Code => FaultBand.Embedded + Switch(
        busy:    static _ => 1,
        corrupt: static _ => 2,
        io:      static _ => 3,
        refused: static _ => 4,
        kv:      static _ => 5);

    public override string Message => Switch(
        busy:    static c => $"<busy:{c.Status}>",
        corrupt: static c => $"<corrupt:{c.Status}>:{c.Detail}",
        io:      static c => $"<io:{c.Status}>:{c.Detail}",
        refused: static c => $"<refused:{c.Detail}>",
        kv:      static c => $"<kv:{c.Engine}:{c.Status}>:{c.Detail}");

    public override string Category => Switch(
        busy:    static _ => "Busy",
        corrupt: static _ => "Corrupt",
        io:      static _ => "Io",
        refused: static _ => "Refused",
        kv:      static _ => "Kv");

    public static EmbeddedFault Create(string message) => new Refused(message);

    // status int discriminates structurally: BUSY/LOCKED is the only transient (retry) class, CORRUPT/NOTADB is
    // terminal and routes to restore, the rest are deterministic IO; a managed `SqliteException` carries the same code.
    public static EmbeddedFault Lift(Exception ex) => ex is SqliteException sql ? FromStatus(sql.SqliteErrorCode, sql.Message) : new Refused(ex.Message);
    public static EmbeddedFault FromStatus(int status, string detail) => status switch {
        raw.SQLITE_BUSY or raw.SQLITE_LOCKED => new Busy(status),
        raw.SQLITE_CORRUPT or raw.SQLITE_NOTADB => new Corrupt(status, detail),
        raw.SQLITE_IOERR or raw.SQLITE_FULL or raw.SQLITE_READONLY => new Io(status, detail),
        _ => new Refused(detail),
    };
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EngineOps {
    // native crossing mints a CLOSED `EmbeddedFault`, never throws into the interior (`boundaries.md`
    // `CAPSULE_OWNER`/`SEAM_CHOOSER`): an `OK` checkpoint receipts the frame counts, a `SQLITE_BUSY` receipts a retry the
    // schedule re-drives (an overlapping reader blocked the truncate — steady-state, not a fault), and every other status
    // lifts through `EmbeddedFault.FromStatus` (a `SQLITE_CORRUPT` routes to recovery) — a bare `throw` of a provider
    // `SqliteException` escaping unconverted is the deleted form the sibling ops already reject.
    // observation instant rides the injected `Element/graph#STORE_RAIL` ProjectionContext frame ([A.1]) —
    // a `ClockPolicy` parameter on any signature here is the named strata inversion.
    public static Fin<EmbeddedFact> Checkpoint(SqliteConnection store, SnapshotFloor floor, CheckpointMode mode, ProjectionContext frame) {
        try {
            int status = raw.sqlite3_wal_checkpoint_v2(Handle(store), "main", mode.Key, out int logFrames, out int checkpointed);
            if (status == raw.SQLITE_OK && mode == CheckpointMode.Truncate) { floor.Dispose(); }
            return status is raw.SQLITE_OK or raw.SQLITE_BUSY
                ? Fin.Succ(new EmbeddedFact(status == raw.SQLITE_BUSY ? "checkpoint-busy" : "checkpoint", logFrames, checkpointed, frame.Now()))
                : Fin.Fail<EmbeddedFact>(EmbeddedFault.FromStatus(status, raw.sqlite3_errstr(status).utf8_to_string()));
        }
        catch (Exception ex) { return Fin.Fail<EmbeddedFact>(EmbeddedFault.Lift(ex)); }
    }

    public static Fin<T> WithSnapshot<T>(SqliteConnection store, SnapshotFloor floor, Func<SqliteConnection, T> read) {
        if (store.Handle is not { } handle) { return Fin.Fail<T>(new EmbeddedFault.Refused("<no-handle>")); }
        int got;
        sqlite3_snapshot snapshot;
        using (SqliteTransaction pin = store.BeginTransaction(IsolationLevel.Serializable, deferred: true)) {
            got = raw.sqlite3_snapshot_get(handle, "main", out snapshot);
            if (got != raw.SQLITE_OK) {
                int recovered = raw.sqlite3_snapshot_recover(handle, "main");
                got = recovered == raw.SQLITE_OK ? raw.sqlite3_snapshot_get(handle, "main", out snapshot) : recovered;
            }
            if (got != raw.SQLITE_OK) {
                return Fin.Fail<T>(EmbeddedFault.FromStatus(got, "<snapshot-unavailable>"));
            }
        }
        bool promoted = false;
        using SqliteTransaction view = store.BeginTransaction(IsolationLevel.Serializable, deferred: true);
        try {
            if (raw.sqlite3_snapshot_open(handle, "main", snapshot) is int opened && opened != raw.SQLITE_OK) {
                return Fin.Fail<T>(EmbeddedFault.FromStatus(opened, "<snapshot-open>"));
            }
            lock (floor.Gate) {
                if (floor.Held is { } held && raw.sqlite3_snapshot_cmp(snapshot, held) < 0) {
                    return Fin.Fail<T>(new EmbeddedFault.Refused("<snapshot-regression>"));
                }
                if (floor.Held is { } prior) { raw.sqlite3_snapshot_free(prior); }
                (floor.Held, promoted) = (snapshot, true);
            }
            return Fin.Succ(read(store));
        }
        catch (Exception ex) { return Fin.Fail<T>(EmbeddedFault.Lift(ex)); }
        finally { if (!promoted) { raw.sqlite3_snapshot_free(snapshot); } }
    }

    public static IO<Fin<EmbeddedFact>> Backup(SqliteConnection source, string destinationPath, BackupPolicy policy, ProjectionContext frame) =>
        IO.lift(() => {
            Fin<ContentAddress> expected = policy.Identity(source);
            if (expected.IsFail) { return Fin.Fail<EmbeddedFact>(expected.Error); }
            using SqliteConnection destination = EmbeddedStore.Dialed(destinationPath);
            try { destination.Open(); }
            catch (Exception exception) { return Fin.Fail<EmbeddedFact>(EmbeddedFault.Lift(exception)); }
            sqlite3_backup backup = raw.sqlite3_backup_init(Handle(destination), "main", Handle(source), "main");
            try {
                int step;
                do { step = raw.sqlite3_backup_step(backup, policy.PageStep); }
                while (step == raw.SQLITE_OK);
                if (step != raw.SQLITE_DONE) { return Fin.Fail<EmbeddedFact>(EmbeddedFault.FromStatus(step, raw.sqlite3_errstr(step).utf8_to_string())); }
                Fin<Unit> integrity = QuickCheck(destination);
                Fin<ContentAddress> observed = policy.Identity(destination);
                return integrity.IsFail
                    ? Fin.Fail<EmbeddedFact>(integrity.Error)
                    : observed.IsFail
                        ? Fin.Fail<EmbeddedFact>(observed.Error)
                        : observed.ValueUnsafe() != expected.ValueUnsafe()
                            ? Fin.Fail<EmbeddedFact>(new EmbeddedFault.Corrupt(raw.SQLITE_CORRUPT, "<backup-identity>"))
                            : Fin.Succ(new EmbeddedFact("backup", raw.sqlite3_backup_pagecount(backup), raw.sqlite3_backup_remaining(backup), frame.Now()));
            }
            finally { _ = raw.sqlite3_backup_finish(backup); }
        });

    public static IO<long> WriteBlob(SqliteConnection store, BlobBinding binding, long rowid, ReadOnlyMemory<byte> payload) =>
        IO.lift(() => {
            using SqliteCommand command = store.CreateCommand();
            command.CommandText = binding.PreallocateSql;
            command.Parameters.Add(new SqliteParameter("rowid", SqliteType.Integer) { Value = rowid });
            command.Parameters.Add(new SqliteParameter("length", SqliteType.Integer) { Value = payload.Length });
            if (command.ExecuteNonQuery() != 1) { throw new InvalidOperationException("<blob-row-absent>"); }
            using SqliteBlob blob = new(store, binding.Table, binding.Column, rowid, readOnly: false);
            blob.Write(payload.Span);
            return (long)payload.Length;
        });

    public static Fin<long> DataVersion(SqliteConnection store) {
        try { using SqliteCommand command = store.CreateCommand(); command.CommandText = "PRAGMA data_version"; return Fin.Succ(Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture)); }
        catch (Exception ex) { return Fin.Fail<long>(EmbeddedFault.Lift(ex)); }
    }

    static Fin<Unit> QuickCheck(SqliteConnection store) {
        try {
            using SqliteCommand command = store.CreateCommand();
            command.CommandText = "PRAGMA quick_check";
            return string.Equals(Convert.ToString(command.ExecuteScalar(), CultureInfo.InvariantCulture), "ok", StringComparison.Ordinal)
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new EmbeddedFault.Corrupt(raw.SQLITE_CORRUPT, "<quick-check>"));
        }
        catch (Exception exception) { return Fin.Fail<Unit>(EmbeddedFault.Lift(exception)); }
    }

    static sqlite3 Handle(SqliteConnection store) => store.Handle ?? throw new InvalidOperationException("<no-handle>");
}

// operational embedded-KV floor — axis [07]'s rocksdb-lsm and lmdb rows made real: `Lsm` is the
// write-optimized local op spool a disconnected peer buffers pending `OpLogEntry`/`CrdtOp` rows in
// (`Version/ledger#SYNC_TRANSPORTS` `SyncSession` binds it as its durable row source when no server is
// reachable), `Mmap` the read-optimized local content-address index used by disconnected-peer reconstruction
// without asserting remote provider residence. ONE polymorphic surface over both engines — a per-engine service class is
// deleted form; faults lift to the closed `EmbeddedFault` band exactly like the SQLite capsule.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvEngine {
    private KvEngine() { }
    public sealed record Lsm(RocksDb Store) : KvEngine;
    public sealed record Mmap(LightningEnvironment Store, LightningDatabase Index) : KvEngine;
}

public static class KvFloor {
    public static Fin<Unit> Put(KvEngine engine, ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value) => engine.Switch(
        lsm: l => Guarded("rocksdb", () => { l.Store.Put(key.Span, value.Span); return unit; }),
        mmap: m => Guarded("lmdb", () => {
            using LightningTransaction transaction = m.Store.BeginTransaction();
            MDBResultCode write = transaction.Put(m.Index, key.Span, value.Span);
            return write == MDBResultCode.Success ? transaction.Commit() : write;
        }).Bind(Mdb));

    public static Fin<Option<ReadOnlyMemory<byte>>> Get(KvEngine engine, ReadOnlyMemory<byte> key) => engine.Switch(
        lsm: l => Guarded("rocksdb", () => Optional(l.Store.Get(key.Span)).Map(static value => (ReadOnlyMemory<byte>)value)),
        mmap: m => Guarded("lmdb", () => {
            using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
            (MDBResultCode code, _, MDBValue value) = transaction.Get(m.Index, key.Span);
            return code switch {
                MDBResultCode.Success => Fin.Succ(Some((ReadOnlyMemory<byte>)value.CopyToNewArray())),
                MDBResultCode.NotFound => Fin.Succ<Option<ReadOnlyMemory<byte>>>(None),
                _ => Fin.Fail<Option<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("lmdb", code.ToString(), "<get>")),
            };
        }).Bind(static result => result));

    // atomic multi-write both engines own natively: one RocksDB `WriteBatch`, one LMDB write transaction —
    // a `Some` value upserts, a `None` deletes, so the spool drain and the membership refresh are one verb.
    public static Fin<Unit> Batch(KvEngine engine, Seq<(ReadOnlyMemory<byte> Key, Option<ReadOnlyMemory<byte>> Value)> writes) => engine.Switch(
        state: writes,
        lsm: static (rows, l) => {
            return Guarded("rocksdb", () => {
                using WriteBatch batch = new();
                rows.Iter(row => row.Value.Match(Some: value => batch.Put(row.Key.Span, value.Span), None: () => batch.Delete(row.Key.Span)));
                l.Store.Write(batch);
                return unit;
            });
        },
        mmap: static (rows, m) => {
            return Guarded("lmdb", () => {
                using LightningTransaction transaction = m.Store.BeginTransaction();
                Seq<MDBResultCode> statuses = rows.Map(row => row.Value.Match(
                    Some: value => transaction.Put(m.Index, row.Key.Span, value.Span),
                    None: () => transaction.Delete(m.Index, row.Key.Span)));
                Option<MDBResultCode> refused = statuses.Find(static status => status != MDBResultCode.Success && status != MDBResultCode.NotFound);
                return refused.IsSome ? refused.ValueUnsafe() : transaction.Commit();
            }).Bind(Mdb);
        });

    static Fin<Unit> Mdb(MDBResultCode status) => status == MDBResultCode.Success
        ? Fin.Succ(unit)
        : Fin.Fail<Unit>(new EmbeddedFault.Kv("lmdb", status.ToString(), "<write>"));

    static Fin<T> Guarded<T>(string engine, Func<T> call) {
        try { return Fin.Succ(call()); }
        catch (Exception exception) { return Fin.Fail<T>(new EmbeddedFault.Kv(engine, exception.GetType().Name, exception.Message)); }
    }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                | [BINDING]                                                 |
| :-----: | :------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | handle bridge        | `SqliteConnection.Handle` raw seam     | the one join to `sqlite3_*` the managed API omits         |
|  [02]   | checkpoint receipt   | `sqlite3_wal_checkpoint_v2` out-params | typed frame counts; `SQLITE_BUSY` retries the schedule    |
|  [03]   | consistent read      | `sqlite3_snapshot_*` pin bracket       | `_cmp` floor guard; `_free` only a held handle            |
|  [04]   | backup               | paged `sqlite3_backup_*` session       | subsumes whole-file `BackupDatabase`; `quick_check` proof |
|  [05]   | large payload        | `SqliteBlob` over `zeroblob(N)`        | streamed; whole-`byte[]` materialization deleted          |
|  [06]   | fault discrimination | `EmbeddedFault` over the status int    | `Busy` transient; `Corrupt` terminal to recovery          |
|  [07]   | embedded KV          | `KvFloor` over `KvEngine` (LSM/mmap)   | offline op spool + chunk index; one polymorphic surface   |

## [05]-[STORE_AXIS_MAP]

Store perimeter is PARAMETERIZED — eleven axes, every provider row deployment/policy DATA on one axis surface. A future app selects providers by POLICY VALUES (profile rows, grant minters, sink rows, index-residency rows) — never a central-manifest edit, never a new entry point, never a parallel rail. Each kept scale-out row carries the PROVEN ceiling the in-PG/in-process owner cannot reach; every provider row carries its provisioning/health/recovery posture through the `#SERVER_EXTENSIONS` verification-first fold, and the scylla/redis rows gain DEPLOYMENT-CONDITIONAL AppHost probe rows only where the axis row is composed (the Npgsql-only probe stays the default). Relational SoR spine is SINGULAR and sealed — ONE event store, ONE materializer, ONE identity, ONE changefeed — so a perimeter-axis engine row carrying unreachable capability is a legal axis admission, never a second SoR.

| [INDEX] | [AXIS]                    | [SELECTION]                                                 |
| :-----: | :------------------------ | :---------------------------------------------------------- |
|  [01]   | relational SoR spine      | SEALED                                                      |
|  [02]   | object store              | `ObjectStore` `[SmartEnum]`                                 |
|  [03]   | egress sink               | `EgressSink` `[Union]`                                      |
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
- [04]-[read-lane/analytic engine]: `Query/columnar`; duckdb-in-process · pg_duckdb-in-PG · clickhouse-scaleout; distributed merge-tree MPP at cluster scale, never a second SoR.
- [05]-[LAKEHOUSE_INTERCHANGE]: `Query/columnar`; ducklake (extension, forward) · delta; the Delta transaction-log wire for external-warehouse interop, a format not an engine.
- [06]-[VECTOR_SEARCH]: `Query/retrieval`; pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · qdrant-scaleout; billion-scale sharded ANN over the in-PG ceiling, `CqlVector` embedding-next-to-row only.
- [07]-[embedded/KV floor]: `Store/provisioning`; sqlite (raw-ADO `EngineOps`) · rocksdb-lsm · lmdb (both operational through `#ENGINE_OPERATIONS` `KvFloor`); write-optimized LSM + read-optimized memory-mapped MVCC over the single-writer WAL floor.
- [08]-[EMBEDDED_RELATIONAL]: `Element/identity` + `Store/provisioning`; npgsql-ef · sqlite-ef; one generated mapping, two providers; a hand ADO mapping beside the rail is deleted (ARCH).
- [09]-[wide-column content-index]: `Query/cache`; marten-pg (default) · scylla-widecolumn; LWT `AppliedInfo` claim-gate + shard-routed point reads at federation scale.
- [10]-[CACHE_BACKPLANE]: `Query/cache`; none (single-node default) · redis-pubsub; cross-process L1 invalidation the `IDistributedCache` contract cannot express.
- [11]-[SPATIAL_STORE_PLANE]: `Element/identity` · `Store/provisioning` · `Element/codec` · `Ingest/geospatial`; postgis-column (EF-NTS) · ado-codec (`SpatialWire`) · geojson-stj · geopackage · wkb/wkt · h3-cell (pocketken); the provisioned postgis/pgrouting/h3-pg tier gains its wire, column, codec, and file-ingress counterparts, closed end-to-end.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
