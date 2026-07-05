# [PERSISTENCE_STORE_PROVISIONING]

Rasm.Persistence provisions the PostgreSQL 18 server tier as ONE VERIFICATION-FIRST read fold and the embedded SQLite floor as ONE idempotent open ritual, the two engines the closed `StoreProfile` axis selects by deployment and never a third: a Rasm process NEVER executes runtime `ALTER SYSTEM`, NEVER spawns or bundles PostgreSQL, and NEVER mutates the cluster — provisioning is a typed `ProvisionVerdict` over what the operator-provisioned cluster already carries, and a gap is a typed signal the operator resolves at the four provisioning rungs (migration artifact, idempotent seed, operator runbook, environment), the fold reading all four and EMITTING repair artifacts (reconciliation grants, `shared_preload_libraries` diffs) as typed verification outputs it never executes. Each server extension is a `ServerExtension` `[SmartEnum<string>]` row carrying its `CreateSql`, its `Admission` gate (preload library, base type, or access method the install requires), the analytical `lane` it serves, and the `RestartClass` its preload gap repairs under — so a new extension is one row and a gap names its repair's disruption class; each verified cluster knob is a `ClusterSetting` row, and the absence policy is not a `Switch` arm but `FailureRank` behavior — a `[SmartEnum]` whose `Absorb` delegate threads the floor-miss receipt, `Required` refusing the profile, `Degradable` folding the lane out so absence surfaces at admission instead of first query, `Observational` recording evidence. The verification is ONE `NpgsqlBatch` round trip over `pg_available_extensions`/`pg_extension`/`pg_settings`/`pg_replication_slots`/`pg_index`, folding the required roster, the held analytical lanes, the emitted repair set, and a stamped `VerificationEpoch` into one verdict the process dispatches on and never re-probes, so admission cost is data-volume-independent and environment drift is an observable epoch event on the fact stream. Beneath the server tier the embedded SQLite floor is the same fold discipline for a single-process store — the open ritual folds pragma rows by RESIDENCY (file-persistent provisioning rows versus per-connection rows), registers connection-scoped capabilities (`uuid7`/`xxh128` UDFs, the `instant_iso` collation, a domain aggregate) before the first statement, hardens through the `SqliteConnection.Handle` raw `sqlite3_db_config` defensive set, gates first-opener migration under one IMMEDIATE transaction, and arms extended result codes — and the `EngineOps` capsule owns the raw-handle operations the managed ADO surface omits (the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder), every throwing crossing converting to a closed `EmbeddedFault` and disposing the connection on every failure path. Every loose provisioning integer is a typed case — `ServerFault` carries the WHOLE re-banded 838x decade (`FaultBand.Server`, the absence/readiness/admission receipts included) and the foreign-store/epoch-ahead refusals are `EmbeddedFault.Refused` in-band 771x, so a bare `Error.New` is the deleted form here; the `StoreProfile` rows additionally carry the wire and EF provider bindings — the `NpgsqlDataSourceBuilder.UseNetTopologySuite` ADO codec row (raw Npgsql lanes read/write geometry: the `cypher` pgrouting results, the verification probes over PostGIS, any `QueueSqlCommand` spatial write; `geographyAsDefault`/precision/ordinates are profile POLICY values, never call-site literals — the EF plugin does not place the codec on raw connections) and the `Ef` bind row (`Server` → `UseNpgsql`, `Embedded` → `UseSqlite` over the connection the open ritual already dialed) feeding the ONE `Element/identity#ELEMENT_IDENTITY` DbContext, so provider variance stays row data on the closed axis and a hand ADO mapping beside the generated rail is the deleted form; the `Npgsql.OpenTelemetry` observability row (`TracerProviderBuilder.AddNpgsql()` + `MeterProviderBuilder.AddNpgsqlInstrumentation()`) subscribes at the AppHost composition root; the `pg_jsonschema` validation lane degrades to the in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` fence when the server extension folds out, one schema serving both residences. This page also hosts the `[V13]` `#STORE_AXIS_MAP` — the 11-axis store perimeter whose provider rows are deployment/policy DATA. Wall clock, correlation, and tenant ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame — a `ClockPolicy`/`CorrelationId`/`TenantContext` parameter on any signature here is the named strata inversion; `ReceiptSinkPort` arrives settled from AppHost; `FaultBand` from `Element/graph#FAULT_TABLES`; `NpgsqlDataSource`/`IDocumentSession`/`IDocumentStore` from the substrate; the analytical lanes that consume the verified extensions arrive from `Query/columnar`/`Query/cypher`/`Query/topology`; the `h3-pg` cell convention the `h3_postgis` bridge serves matches the managed `pocketken.H3` (`Element/identity#ELEMENT_IDENTITY`).

## [01]-[INDEX]

- [01]-[SERVER_EXTENSIONS]: the extension × admission-gate × lane roster, the `FailureRank` absence behavior, the one-batch verification fold over the catalog reads, the four provisioning rungs, the emitted repair set, the wire/EF provider-binding rows, the `pg_jsonschema` in-process fallback fence, and the stamped verification epoch.
- [02]-[EMBEDDED_FLOOR]: the residency-split pragma ladder, the connection-scoped capability registration, the defensive `sqlite3_db_config` hardening, the first-opener IMMEDIATE migration gate, and the closed-engine law.
- [03]-[ENGINE_OPERATIONS]: the `Handle`-bridge capsule, the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder, and the closed `EmbeddedFault` rail.
- [04]-[STORE_AXIS_MAP]: the 11-axis store perimeter — every provider row deployment/policy DATA on one axis surface, each scale-out row carrying its proven ceiling.

## [02]-[SERVER_EXTENSIONS]

- Owner: `StoreProfile` the `[SmartEnum<string>]` engine-selection axis the deployment dials (`server` the PostgreSQL 18 tier, `embedded` the SQLite floor) carrying the `Verify`/`Open` provisioning rail each engine runs — the closed two-engine sweep, never a third; `ServerExtension` the `[SmartEnum<string>]` extension axis, each row carrying its `CreateSql`, its `Admission` install gate, the analytical `Lane` it serves, and the `RestartClass` its preload gap repairs under; `ExtensionAdmission` the closed install-gate `[Union]` (a preload library, a base type the extension extends, a real queryable access method it registers, or a prerequisite-free standalone function/type extension); `RestartClass` the `[SmartEnum<string>]` repair-disruption vocabulary (`session`/`reload`/`restart`); `FailureRank` the `[SmartEnum]` whose `Absorb` delegate IS the absence policy; `ClusterSetting` the verified-knob vocabulary; `ProvisionVerdict` the verification verdict carrying the held set, the receipts, the emitted repair artifacts, and the stamped `VerificationEpoch`; `ServerFault` the closed catalog-read fault `[Union]` over `Expected`; `ClusterProvision` the static surface running the one-batch verification fold and the gated admission — never an `ALTER SYSTEM`.
- Cases: `ServerExtension` is the AUTHORITATIVE provisioning roster — it SUPERSETS the consumer-facing `README#SERVER_EXTENSIONS` subset with the base-type and toolkit rows a dependency chain requires (`postgis` the standalone base the raster/sfcgal/pgrouting rows gate on, `pgvector` the `vector` base `pgvectorscale` gates on, `pg_duckdb` the in-PG DuckDB bridge, `timescaledb_toolkit` over the `timescaledb` base) so the `BaseType` gate resolves against a row the same fold can admit, never against an externally-assumed prerequisite; each gate is the `.api`-verified install precondition, NOT a loose label: `timescaledb` (preload, the hypertable/continuous-aggregate/columnstore analytics, `Query/columnar`), `timescaledb_toolkit` (the hyperfunction/time-weighted-aggregate layer over the `timescaledb` base type), `pg_duckdb` (preload, the in-PG DuckDB analytical bridge distinct from the in-process `DuckDB.NET` lane, `Query/columnar`), `apache-age` (preload, the OPTIONAL openCypher graph demoted beneath QuikGraph, `Query/cypher#GRAPH_SESSION`), `pg_cron` (preload, the in-database maintenance scheduler), `postgis` (standalone — operator classes over the BUILT-IN `gist` AM, registers no custom access method, the base the raster/3D/routing rows extend), `postgis_raster`/`postgis_sfcgal` (PostGIS raster + exact 3D geometry over the `postgis` base type), `pgvector` (the `hnsw` access-method ANN tier) / `pgvectorscale` (the `diskann` AM gated on the `vector` base type), `pg_search` (PRELOAD-gated — the ParadeDB Tantivy `bm25` engine rides `shared_preload_libraries` and hard-errors on `CREATE EXTENSION` without it), `h3-pg` (standalone — the in-PG H3 cell index over built-in AMs plus the `h3_postgis` bridge over the `h3` base type, matching `pocketken.H3`), `pgrouting` (the network routing over the `postgis` base type, `Query/cypher#GRAPH_QUERY`), `pg_partman` (PRELOAD-gated — its `pg_partman_bgw` background worker rides `shared_preload_libraries`), `pg_squeeze` (preload, lock-light table-bloat reclamation), `pg_jsonschema` (standalone — `CREATE EXTENSION`-registered JSON Schema CHECK functions, no preload), `pgaudit` (preload, session/object audit logging), `pg_net` (PRELOAD-gated — its `libcurl` background worker is statically `RegisterBackgroundWorker`'d in `_PG_init` and hard-errors without `shared_preload_libraries`), `pg_graphql` (standalone — pgrx SQL functions + DDL event triggers, no worker, no preload); `ExtensionAdmission` is `Preload(library)` | `BaseType(extension)` | `AccessMethod(method)` (a real queryable index AM the row registers, e.g. `hnsw`) | `Standalone(reason)` (prerequisite-free function/type/operator-class extension that registers NO gating AM); `FailureRank` is `Required`/`Degradable`/`Observational`; `ProvisionVerdict` is `Provisioned | MissingExtension | MissingPreload | SettingDrift | Faulted`; `ServerFault` is the WHOLE re-banded 838x decade — the catalog-read faults (`Unmapped | Unreachable | CatalogDenied`), the absence receipts the `FailureRank.Absorb` delegates thread (`RequiredAbsent | LaneFolded | Evidence`), the readiness evidence (`SlotLag | InvalidIndex`), and the admission refusals (`Ungated | AdmitRefused`) — ten typed cases deriving `FaultBand.Server + n`, so every loose provisioning integer is a registry-derived case and a bare `Error.New(83xx)` is the deleted form.
- Entry: `public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, Seq<ServerExtension> required, VerificationEpoch epoch)` runs ONE `CreateBatch` over the catalog reads and folds the required roster, the held analytical lanes, the `FailureRank`-absorbed receipts, the emitted repair set, and the stamped epoch into one verdict — a polymorphic op whose arity is the `required` `Seq`, never a per-extension probe; `public static IO<Fin<Unit>> Admit(IDocumentSession session, ServerExtension extension, IReadOnlySet<string> preloaded, IReadOnlySet<string> created)` RE-GATES on the live `preloaded`/`created` sets (the ones the caller's `Verify` fold read, no second probe) and queues the `CREATE EXTENSION IF NOT EXISTS` SQL inside the one `IDocumentSession` (committing the DDL with the schema migration) ONLY when the `ExtensionAdmission` gate is satisfied, REFUSING (`8379`, no DDL queued) a preload-gated one whose library is absent or a base-type-gated one whose base is uncreated; `public static IO<Unit> Reload(NpgsqlDataSource source)` calls `ReloadTypesAsync` so live processes re-resolve the wire types a freshly-admitted enum/composite/extension introduced — deployment completes when types reload, not when the DDL lands.
- Auto: verification is ONE six-command batch — `current_setting('shared_preload_libraries')`, `pg_extension` (created), `pg_available_extensions` (installed-on-disk-but-uncreated), the `pg_settings` rows for every `ClusterSetting`, the `pg_replication_slots` `pg_wal_lsn_diff` max-lag scalar, and the `pg_index WHERE NOT indisvalid` count — folded so a preload-gated extension whose library is absent from `shared_preload_libraries` is `MissingPreload` and EMITS a `shared_preload_libraries` repair diff carrying the `RestartClass.Max` worst-disruption rank across the gap set (so the operator reads ONE bounce cost, never a per-row minimum) the operator applies and restarts (never a runtime `ALTER SYSTEM`), an extension PRESENT in `pg_available_extensions` and uncreated with a satisfied gate admits through `CREATE EXTENSION IF NOT EXISTS` in the one session (one absent from the available set has no admissible repair and threads its `FailureRank.Absorb` instead), a `pg_settings` row whose live value fails its `Satisfied` check folds `SettingDrift` carrying the row's `RestartClass`, and a held analytical lane absent below its `FailureRank` threads its `Absorb` receipt — `Required` refusing the profile, `Degradable` folding the lane out so the gap surfaces at admission, `Observational` recording evidence; a lagging replication slot and any invalid index fold in as `Observational` readiness receipts on the held verdict (server-disk liability and an interrupted concurrent build, visible on the fact stream, never profile-refusing) — the slot scalar `max(pg_wal_lsn_diff(pg_current_wal_lsn(), restart_lsn))::bigint` measures the byte lag of ANY operator-configured replication slot the cluster carries (a physical streaming replica or a third-party logical consumer) read through `NpgsqlDataReader.GetInt64` since `pg_wal_lsn_diff(...)::bigint` is a PG `bigint`, the `restart_lsn` column being the WAL-retention floor every slot kind exposes so the gauge is provider-physical and never assumes a slot identity; it is NOT the durability op-log lane's lag, because the op-log changefeed is a Marten async SUBSCRIPTION reading the committed event table (`Version/ledger#CHANGEFEED`, `H11`), NOT a logical-decoding slot consumer, so the lane carries no `pg_replication_slots` row and `confirmed_flush_lsn` (a logical-slot-only column) is deliberately NOT the surface here — the `wal_level=logical` `ClusterSetting` exists for the recovery `LogicalReplicationConnection.IdentifySystem` RPO probe (`Version/recovery#RECOVERY_ROUTES`) and operator logical consumers, not for an op-log slot; the fold carries zero rank arms (a new rank is one `FailureRank` row), the `h3-pg` cell id matches the managed `pocketken.H3` so the same cell indexes at ingest and in SQL, and a periodic re-`Verify` stamps a fresh `VerificationEpoch` so cluster drift becomes an observable event the AppHost health probe reads (`ARCHITECTURE#SEAMS` `[HEALTH_PROBE]`).
- Receipt: a verification rides `store.provision.verify` carrying the verdict, the held lane set, the emitted repair count, and the stamped epoch; an admission rides `store.provision.admit` carrying the extension; a type reload rides `store.provision.reload`.
- Packages: Npgsql (`NpgsqlDataSource.CreateBatch`, `NpgsqlBatchCommand`, `NpgsqlBatch.ExecuteReaderAsync`, `NpgsqlDataReader.NextResultAsync`/`GetInt64`/`GetString`, `NpgsqlParameter<string[]>`, `ReloadTypesAsync`, `PostgresException.SqlState`/`PostgresErrorCodes.InsufficientPrivilege`, `NpgsqlException.IsTransient`, `NpgsqlDataSourceBuilder`), Npgsql.NetTopologySuite (`NpgsqlDataSourceBuilder.UseNetTopologySuite(handleOrdinates, geographyAsDefault)` — the ADO codec row), Npgsql.OpenTelemetry (`TracerProviderBuilder.AddNpgsql()` / `MeterProviderBuilder.AddNpgsqlInstrumentation()` — the observability row subscribed at the AppHost composition root), JsonSchema.Net (`Json.Schema.JsonSchema.FromText`/`Evaluate(JsonElement, EvaluationOptions?)` — the in-process validation fence), NetTopologySuite (`Ordinates`), Microsoft.EntityFrameworkCore (+ `.Sqlite` `UseSqlite` and the Npgsql EF `UseNpgsql` — the `StoreProfile.Ef` bind row over the `Element/identity` DbContext), Marten (`IDocumentSession.QueueSqlCommand`/`SaveChangesAsync`), Rasm.Persistence.Element (`FaultBand`), NodaTime, LanguageExt.Core (`Seq`/`Fin`/`@catch`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new server extension is one `ServerExtension` row carrying its SQL, install gate, lane, and restart class; a new install-gate shape is one `ExtensionAdmission` case; a new absence policy is one `FailureRank` row landing every floor-miss branch with zero `Switch` edits; a new verified setting is one `ClusterSetting` row; zero new surface — a runtime `ALTER SYSTEM`, a Rasm-spawned PostgreSQL, a per-extension managed package, a `Switch` re-enumerating the absence policy at the fold, a per-extension probe round trip, or a second relational engine row is the deleted form because provisioning is verification-first SQL, the absence policy IS the rank-row delegate, the verification is one batch, and the engine sweep is closed.
- Boundary: a Rasm process NEVER spawns or bundles PostgreSQL and NEVER executes runtime `ALTER SYSTEM` — provisioning is verification-only over the operator-provisioned cluster (`#SERVER_EXTENSIONS`), so a `MissingPreload`/`SettingDrift`/`MissingExtension` verdict is a typed signal carrying the EMITTED repair artifact (a `shared_preload_libraries` diff, a `CREATE EXTENSION` reconciliation, a settings diff) the operator resolves at one of the four provisioning rungs, never a self-mutation; the server extensions carry no managed assembly and admit through raw `CREATE EXTENSION IF NOT EXISTS` gated by the row's `ExtensionAdmission` (a preload library, a base type, a real queryable access method, or a prerequisite-free standalone function/type extension) — the `.api`-verified gate per row, so a preload-gated extension mislabeled no-prerequisite cannot leak a hard-erroring `CREATE EXTENSION` past the gate; the `pg_duckdb` extension is the in-PG DuckDB bridge distinct from the in-process `DuckDB.NET` analytical lane (`Query/columnar`), the two meeting at the columnar SQL surface; `apache-age` is the OPTIONAL self-hosted openCypher graph (`Query/cypher#GRAPH_SESSION`) demoted beneath the in-process QuikGraph (`H5`), so its admission is gated and the lane is disabled by default and never assumed co-resident with Marten; spatial→PG GiST (`postgis_raster`/`postgis_sfcgal`) and ANN→`pgvector`/`pgvectorscale` are the transactional index owners while DuckDB `spatial`/`vss` are the columnar aggregators (`L2`), never duplicated; a catalog read denied by privilege folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`) and a transport failure folds through `NpgsqlException.IsTransient` so a retry re-drives only the transient class; `ReloadTypesAsync` completes the deploy by re-resolving wire types, the rejected form being a process that resolves a freshly-admitted enum/composite as unknown until restart.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

// --- [TYPES] ----------------------------------------------------------------------------

// The closed engine-selection axis the deployment dials — the ONE place the two relational engines are named, so a
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

    // The EF provider-bind row ([05] EF-Sqlite admission): provider variance as row DATA on the closed axis —
    // the ONE identity DbContext maps both engines through the generated rail (`IsSqlite()`-keyed model rows
    // live at the identity owner); raw ADO keeps EmbeddedRitual/EngineOps/HandleBridge untouched.
    public Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> Ef { get; }
}

// The ADO spatial-codec policy row ([05] Npgsql.NetTopologySuite admission): geographyAsDefault / SRID /
// precision / ordinates are PROFILE policy values, never call-site literals — the EF plugin does not place
// the codec on raw connections, so the data source composes it once for every raw Npgsql lane (the cypher
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

    // The worst disruption across a gap set — an aggregated repair (a `shared_preload_libraries` diff folding several
    // preload gaps, a `MissingExtension` set with mixed restart classes) carries the MAX so the operator reads ONE
    // disruption cost for the whole reconciliation, never a per-row minimum that understates the bounce; an empty set
    // is `Session` (the no-disruption floor). The `Rank` column is load-bearing here, not decorative.
    public static RestartClass Max(Seq<RestartClass> over) =>
        over.Fold(Session, static (worst, next) => next.Rank > worst.Rank ? next : worst);
}

[SmartEnum]
public sealed partial class FailureRank {
    // The absence policy IS behavior-carrying row data (`#SERVER_EXTENSIONS`): the floor-miss branch threads
    // the receipt through one `Absorb` delegate, so `Required` refuses the profile and stays minimal, `Degradable`
    // folds the lane out so absence surfaces at admission not first query, `Observational` records evidence — a new
    // rank lands as one row, the fold carrying zero rank arms. Every receipt is a TYPED `ServerFault` case
    // deriving off the 8380 registry row — the loose `Error.New(8371/8372/8373)` integers are the deleted form.
    public static readonly FailureRank Required = new(static (_, key) => Fin.Fail<Seq<Error>>(new ServerFault.RequiredAbsent(key)));
    public static readonly FailureRank Degradable = new(static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.LaneFolded(key))));
    public static readonly FailureRank Observational = new(static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.Evidence(key))));

    [UseDelegateFromConstructor]
    public partial Fin<Seq<Error>> Absorb(Seq<Error> receipts, string extensionKey);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtensionAdmission {
    private ExtensionAdmission() { }
    // The install precondition the cluster must already satisfy before `CREATE EXTENSION` can succeed: `Preload` a library
    // in `shared_preload_libraries` (the cluster must boot with it — its worker/hook is `RegisterBackgroundWorker`'d or
    // planner-hooked from `_PG_init` and `CREATE EXTENSION` HARD-ERRORS without it: `timescaledb`/`pg_duckdb`/`age`/
    // `pg_cron`/`pg_squeeze`/`pgaudit`/`pg_search`/`pg_net`/`pg_partman`), `BaseType` an extension the row extends and must
    // be created first (`pgvectorscale` over `vector`, `postgis_*`/`pgrouting` over `postgis`, `h3_postgis` over `h3`,
    // `timescaledb_toolkit` over `timescaledb`). `AccessMethod` names a queryable index access method the extension itself
    // REGISTERS as the gate's documentation (pgvector `hnsw`) — the row carries no CATALOG prerequisite (the AM and its
    // operator classes land WITH the `CREATE EXTENSION`), so it is unconditionally admissible once present on disk.
    // `Standalone` is the genuinely prerequisite-free function/type/event-trigger extension that registers NO gating
    // access method and rides no preload row — `postgis` (ships operator classes over the BUILT-IN `gist`, registers no
    // custom AM), `h3` (operator classes over built-in btree/hash/brin/spgist), `pg_jsonschema`/`pg_graphql` (pgrx SQL
    // functions + event triggers, no worker) — its `Reason` documents what the row brings, never a precondition read.
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
    public static readonly ServerExtension ApacheAge = new("age", new ExtensionAdmission.Preload("age"), "cypher", FailureRank.Observational, RestartClass.Restart);
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
    public static readonly ServerExtension PgPartman = new("pg_partman", new ExtensionAdmission.Preload("pg_partman"), "maintenance", FailureRank.Observational, RestartClass.Restart);
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
        ? long.TryParse(actual, out var a) && long.TryParse(Expected, out var e) && a >= e
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvisionVerdict {
    private ProvisionVerdict() { }
    public sealed record Provisioned(Seq<ServerExtension> Present, FrozenSet<string> HeldLanes, Seq<Error> Receipts, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record MissingExtension(Seq<ServerExtension> Absent, Seq<RepairArtifact> Repairs, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record MissingPreload(Seq<ServerExtension> Unloaded, RepairArtifact PreloadDiff, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record SettingDrift(string Setting, string Expected, string Actual, RestartClass Restart, VerificationEpoch Epoch) : ProvisionVerdict;
    public sealed record Faulted(ServerFault Fault, VerificationEpoch Epoch) : ProvisionVerdict;

    // Required-lane absence and a faulted catalog read are the only verdicts the process refuses to open against;
    // every other verdict carries a repair the operator resolves while the held lanes still compose.
    public bool Admits => this is Provisioned;
}

// --- [ERRORS] ---------------------------------------------------------------------------
// The re-banded server-tier fault band (838x — `FaultBand.Server`, off the 835x Columnar collision): a [Union] over
// the KERNEL `Rasm.Domain.Expected` (parameterless protected ctor; `Category` virtual; `Code`/`Message` inherited from
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
    public sealed record Evidence(string Extension) : ServerFault { public override bool IsTransient => false; }
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
        evidence:       static c => $"<evidence:{c.Extension}>",
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
    public static IO<ProvisionVerdict> Verify(NpgsqlDataSource source, Seq<ServerExtension> required, VerificationEpoch epoch) =>
        IO.liftAsync(async () => {
            await using var connection = await source.OpenConnectionAsync().ConfigureAwait(false);
            await using var batch = connection.CreateBatch();
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT current_setting('shared_preload_libraries')"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT extname FROM pg_extension"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT name FROM pg_available_extensions"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT name, setting FROM pg_settings WHERE name = ANY(@names)") {
                Parameters = { new NpgsqlParameter<string[]>("names", toSeq(ClusterSetting.Items).Map(static s => s.Key).ToArray()) },
            });
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT coalesce(max(pg_wal_lsn_diff(pg_current_wal_lsn(), restart_lsn)), 0)::bigint FROM pg_replication_slots WHERE restart_lsn IS NOT NULL"));
            batch.BatchCommands.Add(new NpgsqlBatchCommand("SELECT count(*)::bigint FROM pg_index WHERE NOT indisvalid"));
            await using var reader = await batch.ExecuteReaderAsync().ConfigureAwait(false);
            var preloaded = (await reader.ReadAsync().ConfigureAwait(false) ? reader.GetString(0) : "")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToFrozenSet(StringComparer.Ordinal);
            var created = await Drain(reader, static r => r.GetString(0)).ConfigureAwait(false);
            var available = await Drain(reader, static r => r.GetString(0)).ConfigureAwait(false);
            var settings = await DrainSettings(reader).ConfigureAwait(false);
            var slotLag = await Scalar(reader).ConfigureAwait(false);
            var invalidIndexes = await Scalar(reader).ConfigureAwait(false);
            return Fold(required, preloaded, created, available, settings, slotLag, invalidIndexes, epoch);
        }) | @catch<IO, ProvisionVerdict>(static _ => true, e => IO.pure(Folded(e, epoch)));

    // The fold is total over the verdict family: preload gaps EMIT a `shared_preload_libraries` diff and refuse, an
    // installable-but-uncreated set (present in `pg_available_extensions`, gate satisfied) is the operator's
    // `CREATE EXTENSION` reconciliation, a drifted setting carries its `RestartClass`, and the survivors fold their held
    // analytical lanes plus the `FailureRank` receipts and the readiness evidence (slot lag, invalid indexes) — never a
    // per-extension `Switch`, the absence policy living on the rank row.
    static ProvisionVerdict Fold(Seq<ServerExtension> required, FrozenSet<string> preloaded, FrozenSet<string> created, FrozenSet<string> available, IReadOnlyDictionary<string, string> settings, long slotLag, long invalidIndexes, VerificationEpoch epoch) {
        var missingPreload = required.Filter(e => e.Admission is ExtensionAdmission.Preload p && !preloaded.Contains(p.Library));
        if (!missingPreload.IsEmpty) {
            var libraries = missingPreload.Choose(e => e.Admission.PreloadLibrary).Distinct();
            var diff = new RepairArtifact("shared_preload_libraries", $"shared_preload_libraries = '{string.Join(',', preloaded.Concat(libraries))}'", RestartClass.Max(missingPreload.Map(static e => e.Restart)));
            return new ProvisionVerdict.MissingPreload(missingPreload, diff, epoch);
        }
        // An installable-but-uncreated row is present in `pg_available_extensions` AND its gate is satisfied — that set is
        // the operator's `CREATE EXTENSION` reconciliation; a row whose library is unavailable on disk is NOT here (it
        // routes to the survivor fold's `FailureRank.Absorb` because no `CREATE EXTENSION` repair can fix a missing binary).
        var missing = required.Filter(e => !created.Contains(e.Key) && available.Contains(e.Key) && e.Admission.Admissible(preloaded, created));
        if (!missing.IsEmpty) {
            return new ProvisionVerdict.MissingExtension(missing, missing.Map(e => new RepairArtifact("create_extension", e.CreateSql, e.Restart)), epoch);
        }
        var drift = toSeq(ClusterSetting.Items).Find(s => !s.Satisfied(settings.GetValueOrDefault(s.Key, "")));
        if (drift.IsSome) {
            var s = drift.ValueUnsafe()!;
            return new ProvisionVerdict.SettingDrift(s.Key, s.Expected, settings.GetValueOrDefault(s.Key, ""), s.Restart, epoch);
        }
        // The survivor fold iterates the FULL required set: a created extension is `Held`, an uncreated row whose binary is
        // absent from `pg_available_extensions` (or whose gate is unmet) threads its `FailureRank.Absorb` — a `Required`
        // rank absorbing to `Fail` aborts the verdict to a `MissingExtension` (no admissible repair exists), a
        // `Degradable`/`Observational` rank records the receipt and the held lanes still compose. Readiness evidence — a
        // lagging replication slot (server-disk liability) and any invalid index (an interrupted concurrent build) — folds
        // in as `Observational` receipts on the held verdict, never refusing the profile but visible on the fact stream.
        var readiness = (slotLag > 0 ? Seq<Error>(new ServerFault.SlotLag(slotLag)) : Seq<Error>())
            + (invalidIndexes > 0 ? Seq<Error>(new ServerFault.InvalidIndex(invalidIndexes)) : Seq<Error>());
        var fold = required.Fold(
            (Held: Seq<ServerExtension>(), Receipts: readiness, Absent: Seq<ServerExtension>()),
            (acc, e) => created.Contains(e.Key)
                ? (acc.Held.Add(e), acc.Receipts, acc.Absent)
                : e.Rank.Absorb(acc.Receipts, e.Key).Match(
                    Succ: r => (acc.Held, r, acc.Absent),
                    Fail: r => (acc.Held, acc.Receipts.Add(r), acc.Absent.Add(e))));
        return fold.Absent.IsEmpty
            ? new ProvisionVerdict.Provisioned(fold.Held, fold.Held.Map(static e => e.Lane).ToFrozenSet(StringComparer.Ordinal), fold.Receipts, epoch)
            : new ProvisionVerdict.MissingExtension(fold.Absent, fold.Absent.Map(e => new RepairArtifact("required_absent", e.CreateSql, e.Restart)), epoch);
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
    public static IO<Fin<Unit>> Admit(IDocumentSession session, ServerExtension extension, IReadOnlySet<string> preloaded, IReadOnlySet<string> created) =>
        !extension.Admission.Admissible(preloaded, created)
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

    // The Server row's data-source build ([05] Npgsql.NetTopologySuite): the ADO spatial codec composes ONCE on
    // the owning NpgsqlDataSourceBuilder from the `SpatialWire` policy row, so every raw lane (cypher pgrouting
    // decode, verification probes, QueueSqlCommand spatial writes) reads/writes NTS geometry — the EF plugin
    // never places the codec on raw connections, so this row is the wire's one admission site.
    public static NpgsqlDataSource Source(string dsn, SpatialWire wire) {
        var builder = new NpgsqlDataSourceBuilder(dsn);
        builder.UseNetTopologySuite(handleOrdinates: wire.HandleOrdinates, geographyAsDefault: wire.GeographyAsDefault);
        return builder.Build();
    }

    // The pg_jsonschema dual-residence fence ([05] JsonSchema.Net): ONE schema text serves both residences —
    // the held `validation` lane checks server-side (`json_matches_schema` in a CHECK/predicate), and a folded-out
    // lane degrades to the in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` boolean
    // parity gate, so absence of the server extension narrows residence, never capability.
    public static bool SchemaCheck(FrozenSet<string> heldLanes, string schema, JsonElement instance, Func<string, JsonElement, bool> serverCheck) =>
        heldLanes.Contains("validation")
            ? serverCheck(schema, instance)
            : Json.Schema.JsonSchema.FromText(schema).Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Flag }).IsValid;

    static async Task<FrozenSet<string>> Drain(NpgsqlDataReader reader, Func<NpgsqlDataReader, string> read) {
        var rows = new HashSet<string>(StringComparer.Ordinal);
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false)) { rows.Add(read(reader)); }   // Exemption: ADO read loop fills a seam-local set frozen once on return
        return rows.ToFrozenSet(StringComparer.Ordinal);
    }

    static async Task<IReadOnlyDictionary<string, string>> DrainSettings(NpgsqlDataReader reader) {
        var settings = new Dictionary<string, string>(StringComparer.Ordinal);
        await reader.NextResultAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false)) { settings[reader.GetString(0)] = reader.GetString(1); }   // Exemption: ADO read loop fills a seam-local map frozen once on return
        return settings;
    }

    // The slot-lag and invalid-index readings are single-row scalar aggregates: advance to the result set and read the
    // one `bigint`/`int` cell, defaulting to 0 on an empty set so a cluster with no slots/no invalid indexes reads clean.
    static async Task<long> Scalar(NpgsqlDataReader reader) {
        await reader.NextResultAsync().ConfigureAwait(false);
        return await reader.ReadAsync().ConfigureAwait(false) ? reader.GetInt64(0) : 0L;
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | provisioning stance | verification-first                     | never `ALTER SYSTEM`; never spawns PostgreSQL             |
|  [02]   | verification cost   | one six-command `CreateBatch` round trip | data-volume-independent; no per-extension probe         |
|  [03]   | absence policy      | `FailureRank.Absorb` delegate          | required refuses; degradable folds out; observational logs|
|  [04]   | install gate        | `ExtensionAdmission` (preload/type/AM/standalone) | `.api`-verified gate per row; CASCADE pulls the dependency |
|  [05]   | preload gap         | `MissingPreload` + emitted diff        | operator resolves at the cluster config; named restart class |
|  [06]   | setting drift       | `pg_settings` vs `ClusterSetting`      | mismatch folds `SettingDrift` carrying its `RestartClass` |
|  [07]   | repair posture      | EMIT artifacts, never execute          | reconciliation grants + settings diffs are typed outputs  |
|  [08]   | drift visibility    | stamped `VerificationEpoch`            | a re-verify advance is an observable health-probe event   |
|  [09]   | deploy completion   | `ReloadTypesAsync`                     | types re-resolve before the deploy is done               |
|  [10]   | h3 parity           | `h3-pg`/`h3_postgis` match `pocketken.H3` | one cell id at ingest and in SQL                       |
|  [11]   | spatial wire        | `SpatialWire` policy row on `Source`   | ADO codec composed once; call-site literals deleted      |
|  [12]   | EF provider bind    | `StoreProfile.Ef` row data             | one identity DbContext, two providers; hand ADO mapping deleted |
|  [13]   | observability       | `AddNpgsql`/`AddNpgsqlInstrumentation` | subscribed at the AppHost composition root, never in-fence |
|  [14]   | schema validation   | `SchemaCheck` dual residence           | server `json_matches_schema` when held; in-process `Evaluate` fallback |
|  [15]   | fault typing        | 838x `ServerFault` whole decade        | absence/readiness/admission receipts are registry-derived cases |

## [03]-[EMBEDDED_FLOOR]

- Owner: `EmbeddedRitual` the idempotent open-ritual record carrying the file-persistent provisioning rows, the per-connection pragma rows, the defensive `DbConfig` set, and the connection-scoped `Capability` registrations (each a named `Action<SqliteConnection>` grant); `EmbeddedStore` the static surface owning the dialed connection, the residency-split fold, the first-opener IMMEDIATE migration gate, and the closed-engine law.
- Cases: the ritual's `ConnectionRows` are the per-connection pragmas (`synchronous=NORMAL`, `journal_size_limit`, `temp_store=MEMORY`, `cache_size`) the fold re-applies on every open; the `Capabilities` are the schema-resident registrations (`uuid7`/`xxh128` scalar UDFs and the `instant_iso` collation the identity policy and chronological ordering need, a domain aggregate) that register before the first statement or the file is unreadable; the `DbConfig` set is the defensive-mode + double-quoted-literal-rejection posture applied through the raw `Handle`; the file-persistent `application_id`/`user_version` are provisioning identity the migration gate writes, never per-connection.
- Entry: `public static SqliteConnection Dialed(string path)` opens the embedded connection with the canonical connection-string posture (`ForeignKeys`, pooling, `ReadWriteCreate`); `public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual, Action<SqliteConnection, SqliteTransaction, long> migrate)` folds the declared ritual end-to-end idempotently — identity check, per-connection pragma rows, defensive `sqlite3_db_config` hardening, extended-result-code arming, capability registration, the IMMEDIATE migration gate, the epoch read — every throwing provider call staying INSIDE the `Fin` boundary so a provider fault converts to `EmbeddedFault` and disposes the connection on every failure path rather than escaping with a leaked live handle; `migrate` is the first-opener step run under the one IMMEDIATE transaction when the held epoch trails the compiled epoch.
- Auto: every connection in every process folds the SAME declared sequence so bootstrap, crash-recovery reopen, and steady-state open are one fold with no first-process special case — the identity check rejects a foreign `application_id`, the per-connection pragma rows apply (`synchronous=NORMAL` the WAL throughput row whose loss boundary is the last commits and never corruption), the defensive `sqlite3_db_config(Handle, SQLITE_DBCONFIG_DEFENSIVE, 1)` plus `DQS_DDL=0`/`DQS_DML=0` harden against direct b-tree writes and double-quoted string literals (so a double-quoted literal is a prepare-time syntax error, identifiers quoting with `"` and strings with `'`), `sqlite3_extended_result_codes(Handle, 1)` upgrades the running taxonomy where receipts must discriminate (`BUSY_SNAPSHOT` from plain `BUSY`), the capabilities register connection-instance-scoped (never persisted — `isDeterministic: true` admits the UDF into expression indexes and generated columns), the first-opener migration runs the `migrate` step under one IMMEDIATE transaction (losers blocked on the lock observe the bumped `user_version` on acquisition and no-op, a register ahead of the compiled epoch a typed rejection so correctness needs no leader election), and `PRAGMA data_version` is the polling-free cross-process change probe `EngineOps` reads; any write transaction begins IMMEDIATE so a deferred read attempting its first write never burns the busy budget on a stale-snapshot retry, and the provider already retries `BUSY`/`LOCKED` at managed quanta so a nonzero `busy_timeout` is the deleted form.
- Receipt: an open rides `store.embedded.open` carrying the ritual fact count and the epoch.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`/`CreateFunction`/`CreateAggregate`/`CreateCollation`/`BeginTransaction(IsolationLevel, deferred)`), SQLitePCLRaw.bundle_e_sqlite3 (`raw.sqlite3_db_config(sqlite3, int, int, out int)`/`raw.sqlite3_extended_result_codes`/`raw.SQLITE_DBCONFIG_DEFENSIVE`=1010/`raw.SQLITE_DBCONFIG_DQS_DDL`=1014/`raw.SQLITE_DBCONFIG_DQS_DML`=1013), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new pragma is one `ConnectionRows` row; a new capability is one `Capabilities` registration; a new defensive posture is one `DbConfig` row; zero new surface — a second embedded relational engine (libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory), a per-process bootstrap branch, a nonzero `busy_timeout`, a persisted capability, or a `locking_mode=EXCLUSIVE`/shared-cache posture is the deleted form because the engine sweep is closed, the ritual is the one open path, and the provider already retries `BUSY`/`LOCKED`.
- Boundary: the embedded SQLite floor is the single-process embedded store beneath the server tier — the one engine sweep is CLOSED (PostgreSQL + embedded SQLite only; libSQL, PGlite, LiteDB, RavenDB.Embedded, Realm, hctree, embedded-pg, EF InMemory all rejected) so a new engine row is the named defect, and the embedded floor and the PostgreSQL server tier are two engines on the one `StoreProfile` axis (`#SERVER_EXTENSIONS` `StoreProfile`), the profile selecting one by deployment, never a third; pragma rows carry RESIDENCY — file-persistent rows (`journal_mode`, `application_id`, `user_version`) are provisioning identity the migration gate writes and the ritual folds ONLY per-connection rows; capability registration is connection-instance-scoped and never persisted — schema-resident functions, aggregates, and collations register before the first statement or the file is unreadable, and `isDeterministic: true` is the capability grant admitting a function into expression indexes and generated columns; the WAL `-wal`/`-shm` sidecar set is the unit of copy/replace/delete (a main file separated from its sidecars is silent page-level corruption); STRICT tables are the typed admission gate and `RETURNING` supersedes write-then-read identity round trips; the defensive `sqlite3_db_config` set and double-quoted-literal rejection are connection POLICY applied through the `Handle` raw bridge (`api-sqlite#HANDLE_BRIDGE`), not connection-string knobs; extension loading stays FULLY disabled — the `Canonical` ritual arms neither the SQL `load_extension()` function nor the C-API loader (`SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` is absent from the `DbConfig` set), so the bundled `e_sqlite3` floor admits no runtime extension and a `DbConfig` row arming the loader is the deliberate opt-in a deployment that needs one adds, never the default; the `Password` row fails at open because the admitted `e_sqlite3` bundle has no cipher (encryption-at-rest for the embedded floor is a deployment concern requiring a different bundle — `e_sqlcipher` — never a connection-string knob over `e_sqlite3`, and the at-rest provider is not an admitted owner of this rebuild); the ritual is the one open path so a per-process bootstrap branch is the deleted form.

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

    // The canonical ritual: the per-connection WAL/throughput pragmas, the defensive `sqlite3_db_config` posture
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
    public static SqliteConnection Dialed(string path) => new(new SqliteConnectionStringBuilder {
        DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = true, ForeignKeys = true,
    }.ConnectionString);

    public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual, Action<SqliteConnection, SqliteTransaction, long> migrate) {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(ritual);
        ArgumentNullException.ThrowIfNull(migrate);
        try {
            store.Open();
            var handle = store.Handle ?? throw new InvalidOperationException("<no-handle>");
            var identity = Scalar(store, "PRAGMA application_id");
            if (identity != ritual.Identity && identity != 0L) { return Refused(store, $"<foreign-store:{identity:x8}>"); }
            var facts = ritual.ConnectionRows.Map(row => new RitualFact(row.Row, Execute(store, row.Sql)));
            _ = raw.sqlite3_extended_result_codes(handle, 1);
            facts += ritual.DbConfig.Map(row => new RitualFact(row.Row, raw.sqlite3_db_config(handle, row.Op, row.Value, out var applied) == raw.SQLITE_OK ? applied : -1L));
            facts += ritual.Capabilities.Map(row => (fun(() => row.Grant(store))(), new RitualFact(row.Row, 1L)).Item2);
            using var gate = store.BeginTransaction(IsolationLevel.Serializable, deferred: false);
            var held = Scalar(store, "PRAGMA user_version");
            if (held > ritual.CompiledEpoch) { return Refused(store, $"<epoch-ahead:{held}>"); }
            if (held < ritual.CompiledEpoch) {
                migrate(store, gate, held);
                _ = Execute(store, $"PRAGMA application_id={ritual.Identity}");
                _ = Execute(store, $"PRAGMA user_version={ritual.CompiledEpoch}");
            }
            gate.Commit();
            return Fin.Succ(facts.Add(new RitualFact("<epoch>", long.Max(held, ritual.CompiledEpoch))));
        }
        catch (Exception ex) {
            store.Dispose();
            return Fin.Fail<Seq<RitualFact>>(EmbeddedFault.Lift(ex));
        }
    }

    // The refusal arms are TYPED — a foreign `application_id` and an epoch-ahead register both rail
    // `EmbeddedFault.Refused` (7714, in-band 771x); the loose `Error.New(7701/7702)` integers are the deleted form.
    static Fin<Seq<RitualFact>> Refused(SqliteConnection store, string detail) =>
        (fun(store.Dispose)(), Fin.Fail<Seq<RitualFact>>(new EmbeddedFault.Refused(detail))).Item2;

    static long Execute(SqliteConnection store, string sql) { using var c = store.CreateCommand(); c.CommandText = sql; return c.ExecuteNonQuery(); }
    static long Scalar(SqliteConnection store, string sql) { using var c = store.CreateCommand(); c.CommandText = sql; return Convert.ToInt64(c.ExecuteScalar(), CultureInfo.InvariantCulture); }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | open ritual         | one idempotent fold                    | bootstrap/recovery/steady-state are one path             |
|  [02]   | pragma residency    | per-connection rows only               | file-persistent rows are the migration gate's            |
|  [03]   | hardening           | `sqlite3_db_config` defensive + DQS off| connection policy via `Handle`, not connection-string    |
|  [04]   | capability scope    | connection-instance registration       | UDFs/collation/aggregate; never persisted                |
|  [05]   | migration gate      | first-opener IMMEDIATE transaction     | losers observe the bumped epoch; no leader election      |
|  [06]   | write transaction   | IMMEDIATE begin                        | a deferred-then-write burns the busy budget               |
|  [07]   | engine sweep        | closed (PostgreSQL + SQLite only)      | a new embedded engine row is the named defect             |
|  [08]   | sidecar unit        | `-wal`/`-shm` set                      | a main file without its sidecars is silent corruption     |

## [04]-[ENGINE_OPERATIONS]

- Owner: `HandleBridge` the capsule projecting the `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) live native crossing into typed `Fin` results so a raw call never escapes with a flattened provider token; `CheckpointMode`/`SnapshotPin` the WAL-operation vocabularies; `EmbeddedFact` the typed receipt the raw operations emit; `EmbeddedFault` the closed embedded-boundary fault `[Union]` over `Expected`; `EngineOps` the static surface owning the WAL checkpoint, the consistent-read snapshot pin, the paged backup, the large-payload blob lane, and the integrity ladder — the raw operations the managed ADO surface omits (`api-sqlitepcl#INTEGRATION_STACK`).
- Cases: `CheckpointMode` is `Passive`/`Full`/`Restart`/`Truncate` (the `raw.SQLITE_CHECKPOINT_*` modes — `Truncate` the scheduled WAL-bound reset); `EmbeddedFault` is `Busy` (a `SQLITE_BUSY`/`SQLITE_LOCKED` retry signal, the only transient case), `Corrupt` (`SQLITE_CORRUPT`/`SQLITE_NOTADB`, terminal — routes to `Version/recovery`), `Io` (`SQLITE_IOERR`/`SQLITE_FULL`), and `Refused` (a foreign store / epoch-ahead / pin regression) so a status `int` discriminates structurally rather than collapsing to one token; the integrity ladder orders boot `quick_check`, cycle `integrity_check` plus `foreign_key_check`, a deeper-tier failure routing to restore, never retry.
- Entry: `public static Fin<EmbeddedFact> Checkpoint(SqliteConnection store, CheckpointMode mode)` runs `raw.sqlite3_wal_checkpoint_v2(Handle, "main", mode, out logFrames, out checkpointed)` so the fact carries the log-frame and checkpointed-frame counts, a `SQLITE_BUSY` return receipts a retry on the rail (never throws), and every other status lifts to a closed `EmbeddedFault`; `public static Fin<T> WithSnapshot<T>(SqliteConnection store, Func<SqliteConnection, T> read)` brackets a consistent multi-statement WAL read view (`sqlite3_snapshot_get` inside a deferred pin transaction → one `sqlite3_snapshot_recover` retry with a fresh re-GET on a refused pin → `sqlite3_snapshot_open` upgrading an unread deferred read transaction → the `sqlite3_snapshot_cmp` monotonic-floor guard → the read → `sqlite3_snapshot_free` of only an unpromoted handle, the floor holding the promoted one); `public static IO<EmbeddedFact> Backup(SqliteConnection source, string destinationPath, int pageStep)` runs the paged `sqlite3_backup_*` session over `Handle` (subsuming the whole-file `BackupDatabase` by adding `_remaining`/`_pagecount` progress facts), every backup admitted only after a `quick_check` on the copy plus content identity; `public static IO<long> WriteBlob(SqliteConnection store, string table, string column, long rowid, ReadOnlyMemory<byte> payload)` streams a large payload through a constructed `SqliteBlob` over a `zeroblob(N)` preallocation (whole-payload `byte[]` materialization being the deleted pattern); `public static Fin<long> DataVersion(SqliteConnection store)` reads the polling-free cross-process change register.
- Auto: the checkpoint runs the out-param `sqlite3_wal_checkpoint_v2` form so the typed `EmbeddedFact` carries frame counts and a `SQLITE_BUSY` refusal the schedule retries rather than escalates (continuously overlapping readers starve checkpoints and the WAL grows unbounded, the countermeasure being the scheduled `Truncate` row plus short read transactions by construction); the snapshot pin brackets a consistent read view across statements, the `sqlite3_snapshot_cmp` monotonic-floor guard refusing a reader regression across brackets and the `sqlite3_snapshot_free` releasing only a held handle on every exit; the paged backup steps `pageStep` pages per `sqlite3_backup_step` so the copy yields bounded latency and progress receipts, restarting under other-connection writes (a hot store backs up on the writing connection) and admitting the copy only after `quick_check` plus content identity because the verb succeeding is never the proof; the blob lane preallocates `zeroblob(N)` then streams through the `SqliteBlob` write stream and the `GetStream` read path (the handle aborting when any writer mutates the row); every raw crossing's status `int` matches the `[RAW_CONSTANTS]` codes through `EmbeddedFault.Lift` so `SQLITE_BUSY`/`SQLITE_LOCKED` is a retry receipt and `SQLITE_CORRUPT`/`SQLITE_NOTADB` is terminal routing to restore.
- Receipt: a checkpoint rides `store.embedded.checkpoint` carrying the mode and frame counts; a snapshot read rides `store.embedded.snapshot`; a backup rides `store.embedded.backup` carrying the page progress; a blob write rides `store.embedded.blob` carrying the byte count.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`, `SqliteBlob(connection, table, column, rowid, readOnly)`, `BackupDatabase`), SQLitePCLRaw.bundle_e_sqlite3 (`raw.sqlite3_wal_checkpoint_v2`, `raw.sqlite3_snapshot_get`/`_open`/`_cmp`/`_recover`/`_free`, `raw.sqlite3_backup_init`/`_step`/`_remaining`/`_pagecount`/`_finish`, `raw.sqlite3_extended_errcode`, `raw.sqlite3_errstr`, the `SQLITE_CHECKPOINT_*`/`SQLITE_BUSY`/`SQLITE_CORRUPT`/`SQLITE_DONE` constants), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new checkpoint mode is one `CheckpointMode` row; a new boundary cause is one `EmbeddedFault` case; a new integrity tier is one ladder row; zero new surface — the whole-file `BackupDatabase` where the paged session adds progress facts, a whole-payload `byte[]` blob materialization, a second hashing path beside the registered `xxh128` UDF, a bare `Error.New(ex)` flattening the status int, or a snapshot regression unguarded by `sqlite3_snapshot_cmp` is the deleted form because the paged backup subsumes the whole-file copy, the blob streams through `SqliteBlob`, and the status int discriminates structurally.
- Boundary: `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) is the ONE seam joining the managed ADO surface to the raw `sqlite3_wal_checkpoint_v2`/`sqlite3_snapshot_*`/`sqlite3_backup_*`/`sqlite3_db_config` calls the managed API does not surface (`api-sqlite#HANDLE_BRIDGE`), the bound `e_sqlite3` provider shared so a raw call through `Handle` and an ADO statement target the same native connection; the native crossing is the `[SEAM_CHOOSER]` capsule owner — every throwing call rides inside `HandleBridge` so the cause stays a closed `EmbeddedFault` case (the syscall `Io`, the contention `Busy`, the corruption `Corrupt`, the refused pin `Refused`), never a bare `Error.New(ex)` flattening a multi-cause domain to one token; the WAL `-wal`/`-shm` sidecar set is the unit of backup/copy and a paged backup snapshots the live store without blocking writers; the snapshot pin and the `Truncate` checkpoint are adversaries (a pinned read window blocks truncation and truncation kills pins) so the schedule interleaves both rows and a lost pin is a receipted failure, never a silent rewind; the integrity ladder routes a deeper-tier failure to `Version/recovery`, never retry, because `SQLITE_CORRUPT`/`SQLITE_NOTADB` is terminal; `SQLITE_BUSY` is a steady-state retry signal the provider already retries at managed quanta, never a fault; the blob lane streams through the constructed `SqliteBlob` and the `GetStream` read path so a large payload never materializes whole.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Data.Sqlite;
using NodaTime;
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

// --- [ERRORS] ---------------------------------------------------------------------------
// The closed embedded-boundary fault band (771x): a [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no
// `Category` to override) is the deleted form. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly
// opt-in, so the band carries no per-case `SelfOp` and the `[Union]`-generated `Switch`/`Map` is untouched; band membership is a per-case `Code => 771x` override and `Category` the telemetry
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

    public override int Code => Switch(
        busy:    static _ => 7711,
        corrupt: static _ => 7712,
        io:      static _ => 7713,
        refused: static _ => 7714);

    public override string Message => Switch(
        busy:    static c => $"<busy:{c.Status}>",
        corrupt: static c => $"<corrupt:{c.Status}>:{c.Detail}",
        io:      static c => $"<io:{c.Status}>:{c.Detail}",
        refused: static c => $"<refused:{c.Detail}>");

    public override string Category => Switch(
        busy:    static _ => "Busy",
        corrupt: static _ => "Corrupt",
        io:      static _ => "Io",
        refused: static _ => "Refused");

    public static EmbeddedFault Create(string message) => new Refused(message);

    // The status int discriminates structurally: BUSY/LOCKED is the only transient (retry) class, CORRUPT/NOTADB is
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
    // The native crossing mints a CLOSED `EmbeddedFault`, never throws into the interior (`boundaries.md`
    // `CAPSULE_OWNER`/`SEAM_CHOOSER`): an `OK` checkpoint receipts the frame counts, a `SQLITE_BUSY` receipts a retry the
    // schedule re-drives (an overlapping reader blocked the truncate — steady-state, not a fault), and every other status
    // lifts through `EmbeddedFault.FromStatus` (a `SQLITE_CORRUPT` routes to recovery) — a bare `throw` of a provider
    // `SqliteException` escaping unconverted is the deleted form the sibling ops already reject.
    // The observation instant rides the injected `Element/graph#STORE_RAIL` ProjectionContext frame ([A.1]) —
    // a `ClockPolicy` parameter on any signature here is the named strata inversion.
    public static Fin<EmbeddedFact> Checkpoint(SqliteConnection store, CheckpointMode mode, ProjectionContext frame) {
        try {
            var status = raw.sqlite3_wal_checkpoint_v2(Handle(store), "main", mode.Key, out var logFrames, out var checkpointed);
            if (status == raw.SQLITE_OK && mode == CheckpointMode.Truncate) { FloorReset(); }            // a truncated WAL ends the `_cmp` comparability epoch — the pin/truncate adversary law
            return status is raw.SQLITE_OK or raw.SQLITE_BUSY
                ? Fin.Succ(new EmbeddedFact(status == raw.SQLITE_BUSY ? "checkpoint-busy" : "checkpoint", logFrames, checkpointed, frame.Now()))
                : Fin.Fail<EmbeddedFact>(EmbeddedFault.FromStatus(status, raw.sqlite3_errstr(status).utf8_to_string()));
        }
        catch (Exception ex) { return Fin.Fail<EmbeddedFact>(EmbeddedFault.Lift(ex)); }
    }

    // The reader monotonic floor: the newest snapshot any bracket on this process has served. `_cmp` is defined
    // only within one WAL comparability epoch, so the `Truncate` checkpoint clears the floor; the floor HOLDS
    // its promoted native handle (freed only on supersession or reset), the bracket frees only unpromoted pins.
    static sqlite3_snapshot? floor;
    static readonly Lock FloorGate = new();

    static void FloorReset() { lock (FloorGate) { if (floor is { } held) { raw.sqlite3_snapshot_free(held); } floor = null; } }

    public static Fin<T> WithSnapshot<T>(SqliteConnection store, Func<SqliteConnection, T> read) {       // Exemption: the native pin/open/free bracket is the platform-forced statement seam
        if (store.Handle is not { } handle) { return Fin.Fail<T>(new EmbeddedFault.Refused("<no-handle>")); }
        int got;
        sqlite3_snapshot snapshot;
        using (var pin = store.BeginTransaction(IsolationLevel.Serializable, deferred: true)) {          // `snapshot_get` needs an open transaction; this bracket only records the pin
            got = raw.sqlite3_snapshot_get(handle, "main", out snapshot);
            if (got != raw.SQLITE_OK
                && (raw.sqlite3_snapshot_recover(handle, "main") != raw.SQLITE_OK
                    || raw.sqlite3_snapshot_get(handle, "main", out snapshot) != raw.SQLITE_OK)) {       // re-GET after recover — the failed get's null handle is never opened
                return Fin.Fail<T>(EmbeddedFault.FromStatus(got, "<snapshot-unavailable>"));
            }
        }
        bool promoted = false;
        using var view = store.BeginTransaction(IsolationLevel.Serializable, deferred: true);            // `snapshot_open` upgrades an UNREAD deferred read transaction — opened before any read
        try {
            if (raw.sqlite3_snapshot_open(handle, "main", snapshot) is var opened && opened != raw.SQLITE_OK) {
                return Fin.Fail<T>(EmbeddedFault.FromStatus(opened, "<snapshot-open>"));
            }
            lock (FloorGate) {
                if (floor is { } held && raw.sqlite3_snapshot_cmp(snapshot, held) < 0) {                 // a pin older than the floor is a reader regression — refused, never served
                    return Fin.Fail<T>(new EmbeddedFault.Refused("<snapshot-regression>"));
                }
                if (floor is { } prior) { raw.sqlite3_snapshot_free(prior); }
                (floor, promoted) = (snapshot, true);
            }
            return Fin.Succ(read(store));
        }
        catch (Exception ex) { return Fin.Fail<T>(EmbeddedFault.Lift(ex)); }
        finally { if (!promoted) { raw.sqlite3_snapshot_free(snapshot); } }
    }

    public static IO<EmbeddedFact> Backup(SqliteConnection source, string destinationPath, int pageStep, ProjectionContext frame) =>
        IO.lift(() => {
            using var destination = EmbeddedStore.Dialed(destinationPath);
            destination.Open();
            var backup = raw.sqlite3_backup_init(Handle(destination), "main", Handle(source), "main");
            try {
                int step;
                do { step = raw.sqlite3_backup_step(backup, pageStep); }                                 // Exemption: the paged native backup step loop is the platform-forced statement seam
                while (step is raw.SQLITE_OK or raw.SQLITE_BUSY or raw.SQLITE_LOCKED);
                return step is raw.SQLITE_DONE
                    ? new EmbeddedFact("backup", raw.sqlite3_backup_pagecount(backup), raw.sqlite3_backup_remaining(backup), frame.Now())
                    : throw EmbeddedFault.FromStatus(step, raw.sqlite3_errstr(step).utf8_to_string()).ToException();
            }
            finally { _ = raw.sqlite3_backup_finish(backup); }
        });

    public static IO<long> WriteBlob(SqliteConnection store, string table, string column, long rowid, ReadOnlyMemory<byte> payload) =>
        IO.lift(() => {
            using var blob = new SqliteBlob(store, table, column, rowid, readOnly: false);
            blob.Write(payload.Span);                                                                   // Exemption: span blob stream write — whole-payload byte[] materialization is the deleted form
            return (long)payload.Length;
        });

    public static Fin<long> DataVersion(SqliteConnection store) {
        try { using var c = store.CreateCommand(); c.CommandText = "PRAGMA data_version"; return Fin.Succ(Convert.ToInt64(c.ExecuteScalar())); }
        catch (Exception ex) { return Fin.Fail<long>(EmbeddedFault.Lift(ex)); }
    }

    static sqlite3 Handle(SqliteConnection store) => store.Handle ?? throw new InvalidOperationException("<no-handle>");
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | handle bridge       | `SqliteConnection.Handle` raw seam     | the one join to `sqlite3_*` the managed API omits         |
|  [02]   | checkpoint receipt  | `sqlite3_wal_checkpoint_v2` out-params | typed frame counts; `SQLITE_BUSY` retries the schedule    |
|  [03]   | consistent read     | `sqlite3_snapshot_*` pin bracket       | `_cmp` floor guard; `_free` only a held handle            |
|  [04]   | backup              | paged `sqlite3_backup_*` session       | subsumes whole-file `BackupDatabase`; `quick_check` proof |
|  [05]   | large payload       | `SqliteBlob` over `zeroblob(N)`        | streamed; whole-`byte[]` materialization deleted          |
|  [06]   | fault discrimination| `EmbeddedFault` over the status int    | `Busy` transient; `Corrupt` terminal to recovery          |

## [05]-[STORE_AXIS_MAP]

The store perimeter is PARAMETERIZED — eleven axes, every provider row deployment/policy DATA on one axis surface. A future app selects providers by POLICY VALUES (profile rows, grant minters, sink rows, index-residency rows) — never a central-manifest edit, never a new entry point, never a parallel rail. Each kept scale-out row carries the PROVEN ceiling the in-PG/in-process owner cannot reach; every provider row carries its provisioning/health/recovery posture through the `#SERVER_EXTENSIONS` verification-first fold, and the scylla/redis rows gain DEPLOYMENT-CONDITIONAL AppHost probe rows only where the axis row is composed (the Npgsql-only probe stays the default). The relational SoR spine is SINGULAR and sealed — ONE event store, ONE materializer, ONE identity, ONE changefeed — so a perimeter-axis engine row carrying unreachable capability is a legal axis admission, never a second SoR.

| [INDEX] | [AXIS]                     | [OWNING_PAGE]                                      | [PROVIDER_ROWS] (seed DATA)                                              | [SELECTION]                        | [CEILING / CHARTER PROOF]                                                                 |
| :-----: | :------------------------- | :------------------------------------------------- | :----------------------------------------------------------------------- | :--------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | relational SoR spine       | `Store/provisioning` + `Element/graph`              | postgres-18 (SINGULAR)                                                    | SEALED                             | the one event store · materializer · identity · changefeed — unchallengeable               |
|  [02]   | object store               | `Store/blobstore`                                   | s3 · azure-blob · gcs · minio · presigned-grant (`GrantMinter`)           | `ObjectStore` `[SmartEnum]`        | the presigned row reaches domain-cloud planes no credentialed row can                       |
|  [03]   | egress sink                | `Version/egress`                                    | webhook · nats · kafka · rabbitmq · pulsar · wire-native · redis-stream   | `EgressSink` `[Union]`             | consumer-group ack + `Acknowledged` trim — the zero-broker-install stream row               |
|  [04]   | read-lane/analytic engine  | `Query/columnar`                                    | duckdb-in-process · pg_duckdb-in-PG · clickhouse-scaleout                 | `ColumnarEngine` axis              | distributed merge-tree MPP at cluster scale; never a second SoR                             |
|  [05]   | lakehouse interchange      | `Query/columnar`                                    | ducklake (extension, forward) · delta                                     | format row                         | the Delta transaction-log wire for external-warehouse interop; a format, not an engine     |
|  [06]   | vector search              | `Query/retrieval`                                   | pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · qdrant-scaleout | `VectorBackend` axis            | billion-scale sharded ANN over the in-PG ceiling; `CqlVector` embedding-next-to-row only    |
|  [07]   | embedded/KV floor          | `Store/provisioning`                                | sqlite (raw-ADO `EngineOps`) · rocksdb-lsm · lmdb                         | `EngineOps`-tier row               | write-optimized LSM + read-optimized memory-mapped MVCC over the single-writer WAL floor    |
|  [08]   | embedded relational        | `Element/identity` + `Store/provisioning`           | npgsql-ef · sqlite-ef                                                     | `StoreProfile.Ef` on ONE DbContext | one generated mapping, two providers; a hand ADO mapping beside the rail is deleted (ARCH)  |
|  [09]   | wide-column content-index  | `Query/cache`                                       | marten-pg (default) · scylla-widecolumn                                   | index-residency row                | LWT `AppliedInfo` claim-gate + shard-routed point reads at federation scale                 |
|  [10]   | cache backplane            | `Query/cache`                                       | none (single-node default) · redis-pubsub                                 | `CacheLane.Store`-gated row        | cross-process L1 invalidation the `IDistributedCache` contract cannot express              |
|  [11]   | spatial store plane        | `Element/identity` · `Store/provisioning` · `Element/codec` · `Ingest/geospatial` | postgis-column (EF-NTS) · ado-codec (`SpatialWire`) · geojson-stj · geopackage · wkb/wkt · h3-cell (pocketken) | profile policy rows (`geographyAsDefault`, SRID, precision) | the provisioned postgis/pgrouting/h3-pg tier gains its wire, column, codec, and file-ingress counterparts — closed end-to-end |
