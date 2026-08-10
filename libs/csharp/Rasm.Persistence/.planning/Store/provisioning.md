# [PERSISTENCE_STORE_PROVISIONING]

Rasm.Persistence provisions the PostgreSQL 18 server tier as ONE VERIFICATION-FIRST read fold and the embedded SQLite floor as ONE idempotent open ritual, the two engines the closed `StoreProfile` axis selects by deployment and never a third: a Rasm process NEVER executes runtime `ALTER SYSTEM`, NEVER spawns or bundles PostgreSQL, and NEVER mutates the cluster — provisioning is a typed `ProvisionVerdict` over what the operator-provisioned cluster already carries, and a gap is a typed signal the operator resolves at the four provisioning rungs (migration artifact, idempotent seed, operator runbook, environment), the fold reading all four and EMITTING repair artifacts (reconciliation grants, `shared_preload_libraries` diffs) as typed verification outputs it never executes. Each server extension is a `ServerExtension` `[SmartEnum<string>]` row carrying its `CreateSql`, its `Admission` gate (preload library, base type, or access method the install requires), the analytical `lane` it serves, and the `RestartClass` its preload gap repairs under — so a new extension is one row and a gap names its repair's disruption class; each verified cluster knob is a `ClusterSetting` row, and the absence policy is not a `Switch` arm but `FailureRank` behavior — a `[SmartEnum]` whose `Absorb` delegate threads the floor-miss receipt, `Required` refusing the profile, `Degradable` folding the lane out so absence surfaces at admission instead of first query, `Observational` recording evidence. Verification is ONE `NpgsqlBatch` round trip over `pg_available_extensions`/`pg_extension`/`pg_settings`/`pg_replication_slots`/`pg_index`, folding the required roster, the held analytical lanes, the emitted repair set, and a stamped `VerificationEpoch` into one verdict the process dispatches on and never re-probes, so admission cost is data-volume-independent and environment drift is an observable epoch event on the fact stream. Beneath the server tier the embedded SQLite floor is the same fold discipline for a single-process store — the open ritual folds pragma rows by RESIDENCY (file-persistent provisioning rows versus per-connection rows), registers connection-scoped capabilities (the `Store/observability#SQLITE_STATUS_HARVEST` statement-registry arm, `uuid7`/`xxh128` UDFs, the `instant_iso` collation, a domain aggregate) before the first statement, hardens through the `SqliteConnection.Handle` raw `sqlite3_db_config` defensive set, gates first-opener migration under one IMMEDIATE transaction, and arms extended result codes — and the `EngineOps` capsule owns the raw-handle operations the managed ADO surface omits (the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder), every throwing crossing converting to a closed `EmbeddedFault` and disposing the connection on every failure path. Every loose provisioning integer is a typed case — `ServerFault` carries the WHOLE re-banded 838x decade (`FaultBand.Server`, the absence/readiness/admission receipts included) and the foreign-store/epoch-ahead refusals are `EmbeddedFault.Refused` in-band 771x, so a bare `Error.New` is the deleted form here; the `StoreProfile` rows carry the wire and EF provider bindings — the `NpgsqlDataSourceBuilder.UseNetTopologySuite` ADO codec row (raw Npgsql lanes read/write geometry: the `cypher` pgrouting results, the verification probes over PostGIS, any `QueueSqlCommand` spatial write; `geographyAsDefault`/precision/ordinates are profile POLICY values, never call-site literals — the EF plugin does not place the codec on raw connections) and the `Ef` bind row (`Server` → `UseNpgsql`, `Embedded` → `UseSqlite` over the connection the open ritual already dialed) feeding the ONE `Element/identity#ELEMENT_IDENTITY` DbContext, so provider variance stays row data on the closed axis and a hand ADO mapping beside the generated rail is the deleted form; the `Npgsql.OpenTelemetry` observability row (`TracerProviderBuilder.AddNpgsql()` + `MeterProviderBuilder.AddNpgsqlInstrumentation()`) subscribes at the AppHost composition root; the `pg_jsonschema` validation lane degrades to the in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` fence when the server extension folds out, one schema serving both residences. This page also hosts the `[V13]` `#STORE_AXIS_MAP` — the 11-axis store perimeter whose provider rows are deployment/policy DATA. Wall clock, correlation, and tenant ride the injected `Element/graph#STORE_RAIL` `ProjectionContext` frame, the kernel `CorrelationId`/`TenantContext` pair riding it as S0 vocabulary; `ReceiptSinkPort` arrives settled from AppHost; `FaultBand` from `Element/graph#FAULT_TABLES`; `NpgsqlDataSource`/`IDocumentSession`/`IDocumentStore` from the substrate; the analytical lanes that consume the verified extensions arrive from `Query/columnar`/`Query/cypher`/`Query/topology`; the `h3-pg` cell convention the `h3_postgis` bridge serves matches the managed `pocketken.H3` (`Element/identity#ELEMENT_IDENTITY`).

## [01]-[INDEX]

- [02]-[SERVER_EXTENSIONS]: `ServerExtension`'s extension × admission-gate × lane roster, the `FailureRank` absence behavior, the one-batch verification fold over the catalog reads (extension version floors included), the four provisioning rungs, the emitted repair set, the `MaintenanceJob` in-database work roster, the `RollingWindow` Marten partition roster, the wire/EF provider-binding rows, the `pg_jsonschema` in-process fallback fence, and the stamped verification epoch.
- [03]-[EMBEDDED_FLOOR]: `EmbeddedRitual`'s residency-split pragma ladder, the connection-scoped capability registration, the defensive `sqlite3_db_config` hardening, the first-opener IMMEDIATE migration gate, and the closed-engine law.
- [04]-[ENGINE_OPERATIONS]: `HandleBridge`'s native-handle capsule, the `sqlite3_wal_checkpoint_v2` checkpoint receipt, the `sqlite3_snapshot_*` consistent read pin, the paged `sqlite3_backup_*` session, the `SqliteBlob` zeroblob lane, the integrity ladder, the `KvFloor` embedded-KV capsule over the `KvSpace` keyspace roster of axis [07], the `SpoolAccrual` merge frame, the `KvVault` value seal, the `RocksStatus` message taxonomy, and the closed `EmbeddedFault` rail over `RetryShape`.
- [05]-[STORE_AXIS_MAP]: store perimeter across eleven axes — every provider row deployment/policy DATA on one axis surface, each scale-out row carrying its proven ceiling.

## [02]-[SERVER_EXTENSIONS]

- Owner: `Lane` the `[SmartEnum<string>]` analytical-lane vocabulary, the ONE owner of every lane token the estate spells — `StoreProfile.Lanes` declares which rows an engine realizes and `ServerExtension.Lane` declares which row an extension serves, both drawn from it, so a bare string on either side is the deleted form and one vocabulary cannot fork into two spellings; `StoreProfile` the `[SmartEnum<string>]` engine-selection axis the deployment dials (`server` the PostgreSQL 18 tier, `embedded` the SQLite floor) carrying the `Verify`/`Open` provisioning rail each engine runs, the `Lanes` set its engine can realize at all, its `Admits` gate over both a `Lane` member and an untyped token it resolves through the vocabulary first, and the `Degrade` clause naming what the row gives up — the closed two-engine sweep, never a third; `ServerExtension` the `[SmartEnum<string>]` extension axis, each row carrying its `CreateSql`, its `Admission` install gate, the `Lane` it serves, and the `RestartClass` its preload gap repairs under; `ExtensionAdmission` the closed install-gate `[Union]` (a preload library, a base type the extension extends, a real queryable access method it registers, or a prerequisite-free standalone function/type extension); `RestartClass` the `[SmartEnum<string>]` repair-disruption vocabulary (`session`/`reload`/`restart`); `FailureRank` the `[SmartEnum]` whose `Absorb` delegate IS the absence policy; `ClusterSetting` the verified-knob vocabulary; `ProvisionVerdict` the verification verdict carrying the held set, the receipts, the emitted repair artifacts, and the stamped `VerificationEpoch`; `MaintenanceJob` the in-database work roster riding the gated `Register` admission; `RollingWindow` the `[SmartEnum<string>]` Marten-document partition roster carrying each rostered family's `(Period, Ahead, Aged)` window with the one shared `ManagedRangePartitions` its `Declare` hands out; `ReconcileAxis` the closed deployment-axis vocabulary every `ReconcileRow` keys on beside its `RestartClass` sibling; `ServerFault` the closed catalog-read fault `[Union]` over `Expected`; `ClusterProvision` the static surface running the one-batch verification fold and the gated admission — never an `ALTER SYSTEM`.
- Cases: `ServerExtension` is the AUTHORITATIVE provisioning roster — it SUPERSETS the consumer-facing `README.md` `[SERVER_EXTENSIONS]` card subset with the base-type and toolkit rows a dependency chain requires (`postgis` the standalone base the raster/sfcgal/pgrouting rows gate on, `pgvector` the `vector` base `pgvectorscale` gates on, `pg_duckdb` the in-PG DuckDB bridge, `timescaledb_toolkit` over the `timescaledb` base) so the `BaseType` gate resolves against a row the same fold can admit, never against an externally-assumed prerequisite; each gate is the `.api`-verified install precondition, NOT a loose label: `timescaledb` (preload, the hypertable/continuous-aggregate/columnstore analytics, `Query/columnar`), `timescaledb_toolkit` (the hyperfunction/time-weighted-aggregate layer over the `timescaledb` base type), `pg_duckdb` (preload, the in-PG DuckDB analytical bridge distinct from the in-process `DuckDB.NET` lane, `Query/columnar`), `apache-age` (standalone — the OPTIONAL openCypher graph functions + `agtype`, no preload; Cypher connections issue per-session `LOAD 'age'`, demoted beneath QuikGraph, `Query/cypher#GRAPH_SESSION`), `pg_cron` (preload, the in-database maintenance scheduler), `postgis` (standalone — operator classes over the BUILT-IN `gist` AM, registers no custom access method, the base the raster/3D/routing rows extend), `postgis_raster`/`postgis_sfcgal` (PostGIS raster + exact 3D geometry over the `postgis` base type), `pgvector` (the `hnsw` access-method ANN tier) / `pgvectorscale` (the `diskann` AM gated on the `vector` base type), `pg_search` (PRELOAD-gated — the ParadeDB Tantivy `bm25` engine rides `shared_preload_libraries` and hard-errors on `CREATE EXTENSION` without it), `h3-pg` (standalone — the in-PG H3 cell index over built-in AMs and the `h3_postgis` bridge over the `h3` base type, matching `pocketken.H3`), `pgrouting` (the network routing over the `postgis` base type, `Query/cypher#GRAPH_QUERY`), `pg_partman` (PRELOAD-gated — its `pg_partman_bgw` background worker rides `shared_preload_libraries`), `pg_squeeze` (preload, lock-light table-bloat reclamation), `pg_jsonschema` (standalone — `CREATE EXTENSION`-registered JSON Schema CHECK functions, no preload), `pgaudit` (preload, session/object audit logging), `pg_net` (PRELOAD-gated — its `libcurl` background worker is statically `RegisterBackgroundWorker`'d in `_PG_init` and hard-errors without `shared_preload_libraries`), `pg_graphql` (standalone — pgrx SQL functions + DDL event triggers, no worker, no preload); `ExtensionAdmission` is `Preload(library)` | `BaseType(extension)` | `AccessMethod(method)` (a real queryable index AM the row registers, e.g. `hnsw`) | `Standalone(reason)` (prerequisite-free function/type/operator-class extension that registers NO gating AM); `FailureRank` is `Required`/`Degradable`/`Observational`; `ProvisionVerdict` is `Provisioned | MissingExtension | MissingPreload | SettingDrift | Faulted`; `ServerFault` is the WHOLE re-banded 838x decade — the catalog-read faults (`Unmapped | Unreachable | CatalogDenied`), the absence receipts the `FailureRank.Absorb` delegates thread (`RequiredAbsent | LaneFolded | Evidence`), the readiness evidence (`SlotLag | InvalidIndex`), and the admission refusals (`Ungated | AdmitRefused`) — ten typed cases deriving `FaultBand.Server + n`, so every loose provisioning integer is a registry-derived case and a bare `Error.New(83xx)` is the deleted form.
- Law: `Manifest` folds server expectations and embedded ritual rows into one reconcile-only `ReconcileManifest`.
- Entry: `Verify` folds one catalog batch into a typed verdict; `Register` and `Admit` consume its exact snapshot beside the `StoreProfile` whose realizability they gate on, each reading the LANE off the row it is admitting rather than a call-site token, so the `geo`, `maintenance`, and `audit` lanes gate at the extension and job doors exactly as the analytical lanes gate at their own owning entries; and `BackendObservation.Of` projects a `Provisioned` verdict into the `Store/schema#PROJECTION` observation runtime admission joins against the expected generation, taking the realized artifact set beside the adapter's own observation instant and its two recovery stamps — a probed verdict is the ONE capability evidence the backend contract admits, so a desired roster or an availability read never reaches it, and the recovery halves arrive from the owners that measured them rather than from this fold.
- Auto: verification is ONE six-command batch — `current_setting('shared_preload_libraries')`, `pg_extension` (created), `pg_available_extensions` (installed-on-disk-but-uncreated), the `pg_settings` rows for every `ClusterSetting`, the `pg_replication_slots` `pg_wal_lsn_diff` max-lag scalar, and the `pg_index WHERE NOT indisvalid` count — folded so a preload-gated extension whose library is absent from `shared_preload_libraries` is `MissingPreload` and EMITS a `shared_preload_libraries` repair diff carrying the `RestartClass.Max` worst-disruption rank across the gap set (so the operator reads ONE bounce cost, never a per-row minimum) the operator applies and restarts (never a runtime `ALTER SYSTEM`), an extension PRESENT in `pg_available_extensions` and uncreated with a satisfied gate admits through `CREATE EXTENSION IF NOT EXISTS` in the one session (one absent from the available set has no admissible repair and threads its `FailureRank.Absorb` instead), a `pg_settings` row whose live value fails its `Satisfied` check folds `SettingDrift` carrying the row's `RestartClass`, and a held analytical lane absent below its `FailureRank` threads its `Absorb` receipt — `Required` refusing the profile, `Degradable` folding the lane out so the gap surfaces at admission, `Observational` recording evidence; a lagging replication slot and any invalid index fold in as `Observational` readiness receipts on the held verdict (server-disk liability and an interrupted concurrent build, visible on the fact stream, never profile-refusing) — the slot scalar `max(pg_wal_lsn_diff(pg_current_wal_lsn(), restart_lsn))::bigint` measures the byte lag of ANY operator-configured replication slot the cluster carries (a physical streaming replica or a third-party logical consumer) read through `NpgsqlDataReader.GetInt64` since `pg_wal_lsn_diff(...)::bigint` is a PG `bigint`, the `restart_lsn` column being the WAL-retention floor every slot kind exposes so the gauge is provider-physical and never assumes a slot identity; it is NOT the durability op-log lane's lag, because the op-log changefeed is a Marten async SUBSCRIPTION reading the committed event table (`Version/ledger#CHANGEFEED`, `H11`), NOT a logical-decoding slot consumer, so the lane carries no `pg_replication_slots` row and `confirmed_flush_lsn` (a logical-slot-only column) is deliberately NOT the surface here — the `wal_level=logical` `ClusterSetting` exists for the recovery `LogicalReplicationConnection.IdentifySystem` RPO probe (`Version/recovery#RECOVERY_ROUTES`) and operator logical consumers, not for an op-log slot; the fold carries zero rank arms (a new rank is one `FailureRank` row), the `h3-pg` cell id matches the managed `pocketken.H3` so the same cell indexes at ingest and in SQL, and a periodic re-`Verify` stamps a fresh `VerificationEpoch` so cluster drift becomes an observable event the AppHost health probe reads (`ARCHITECTURE#SEAMS` `[HEALTH_PROBE]`).
- Receipt: a verification rides `store.provision.verify` carrying the verdict, the held lane set, the emitted repair count, and the stamped epoch; an admission rides `store.provision.admit` carrying the extension; a type reload rides `store.provision.reload`.
- Packages: Npgsql (`NpgsqlDataSource.CreateBatch`, `NpgsqlBatchCommand`, `NpgsqlBatch.ExecuteReaderAsync`, `NpgsqlDataReader.NextResultAsync`/`GetInt64`/`GetString`, `NpgsqlParameter<string[]>`, `ReloadTypesAsync`, `PostgresException.SqlState`/`PostgresErrorCodes.InsufficientPrivilege`, `NpgsqlException.IsTransient`, `NpgsqlDataSourceBuilder`), Npgsql.NetTopologySuite (`NpgsqlDataSourceBuilder.UseNetTopologySuite(handleOrdinates, geographyAsDefault)` — the ADO spatial codec row), Npgsql.NodaTime (`NpgsqlNodaTimeExtensions.UseNodaTime<TMapper>(TMapper) where TMapper : INpgsqlTypeMapper` — the ADO temporal codec row the same builder binds, `NpgsqlDataSourceBuilder` implementing `INpgsqlTypeMapper`), Npgsql.OpenTelemetry (`TracerProviderBuilder.AddNpgsql()` / `MeterProviderBuilder.AddNpgsqlInstrumentation()` — the observability row subscribed at the AppHost composition root), JsonSchema.Net (`Json.Schema.JsonSchema.FromText`/`Evaluate(JsonElement, EvaluationOptions?)` — the in-process validation fence), NetTopologySuite (`Ordinates`), Microsoft.EntityFrameworkCore (+ `.Sqlite` `UseSqlite` and the Npgsql EF `UseNpgsql` — the `StoreProfile.Ef` bind row over the `Element/identity` DbContext), Marten (`IDocumentSession.QueueSqlCommand`/`SaveChangesAsync`; `StoreOptions.Schema.For<T>().PartitionOn` + `ByRollingRange`/`PartitionPeriod`/`ManagedRangePartitions` — the `RollingWindow` declaration), Rasm.Persistence.Element (`FaultBand`), NodaTime, LanguageExt.Core (`Seq`/`Fin`/`@catch`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new server extension is one `ServerExtension` row carrying its SQL, install gate, lane, and restart class; a new install-gate shape is one `ExtensionAdmission` case; a new absence policy is one `FailureRank` row landing every floor-miss branch with zero `Switch` edits; a new verified setting is one `ClusterSetting` row; a new analytical lane is one `Lane` row every roster and gate then composes, never a token minted at a call site; a lane a profile cannot realize is one absent `Lanes` member and one clause on that row's `Degrade`, never a caller-side engine test; a new version floor is one `floors` entry (deployment data, never a fence literal); a new in-database maintenance job is one `MaintenanceJob` row riding the gated `Register` admission; a new partition-retired document family is one `RollingWindow` row with the `Declare` call at that family's own mapping; a new deployment axis is one `ReconcileAxis` row every manifest fold then keys on; zero new surface — a runtime `ALTER SYSTEM`, a Rasm-spawned PostgreSQL, a per-extension managed package, a `Switch` re-enumerating the absence policy at the fold, a per-extension probe round trip, or a second relational engine row is the deleted form because provisioning is verification-first SQL, the absence policy IS the rank-row delegate, the verification is one batch, and the engine sweep is closed.
- Boundary: a Rasm process NEVER spawns or bundles PostgreSQL and NEVER executes runtime `ALTER SYSTEM` — provisioning is verification-only over the operator-provisioned cluster (`#SERVER_EXTENSIONS`), so a `MissingPreload`/`SettingDrift`/`MissingExtension` verdict is a typed signal carrying the EMITTED repair artifact (a `shared_preload_libraries` diff, a `CREATE EXTENSION` reconciliation, a settings diff) the operator resolves at one of the four provisioning rungs, never a self-mutation; the server extensions carry no managed assembly and admit through raw `CREATE EXTENSION IF NOT EXISTS` gated by the row's `ExtensionAdmission` (a preload library, a base type, a real queryable access method, or a prerequisite-free standalone function/type extension) — the `.api`-verified gate per row, so a preload-gated extension mislabeled no-prerequisite cannot leak a hard-erroring `CREATE EXTENSION` past the gate; the `pg_duckdb` extension is the in-PG DuckDB bridge distinct from the in-process `DuckDB.NET` analytical lane (`Query/columnar`), the two meeting at the columnar SQL surface; `apache-age` is the OPTIONAL self-hosted openCypher graph (`Query/cypher#GRAPH_SESSION`) demoted beneath the in-process QuikGraph (`H5`), so its admission is gated and the lane is disabled by default and never assumed co-resident with Marten; spatial→PG GiST (`postgis_raster`/`postgis_sfcgal`) and ANN→`pgvector`/`pgvectorscale` are the transactional index owners while DuckDB `spatial`/`vss` are the columnar aggregators (`L2`), never duplicated; a catalog read denied by privilege folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`) and a transport failure folds through `NpgsqlException.IsTransient` so a retry re-drives only the transient class; `ReloadTypesAsync` completes the deploy by re-resolving wire types, the rejected form being a process that resolves a freshly-admitted enum/composite as unknown until restart; lane absence is stated at ADMISSION on BOTH engines — the server tier folds an absent extension through its `FailureRank` and the embedded tier refuses at `StoreProfile.Admits`, so an embedded deployment discovers the columnar, geo, cypher, vector, search, maintenance, audit, and egress lanes are unrealizable at profile selection rather than at the first query, and a lane surrendered without a `Degrade` clause is the deleted form; every rostered lane reaches a gating consumer — the analytical and egress lanes at their own owning entries, and `geo`, `maintenance`, and `audit` at `Admit` and `Register`, which read the lane off the `ServerExtension`/`MaintenanceJob` row they are admitting, so a lane joining the roster gates without a new call site and a lane no row names is unreachable by construction; ONE table has ONE partition manager — `pg_partman` owns the server-partitioned relations its `MaintenanceJob.PartitionParent` row names (`public.op_log`, rolled by the `pg_partman_bgw` worker its `PartitionCycle` row schedules) and `ManagedRangePartitions` owns the Marten document tables the `RollingWindow` rows name (rolled by the `store.Advanced` verbs the single-writer boot pass runs), so a table appearing on both rosters is the deleted form and a `cron.schedule` rotation job aimed at a Marten document table is the deleted form because those verbs are the only rotation surface a document table has.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
using Microsoft.EntityFrameworkCore.Metadata;     // IModel — the compiled model each profile row mounts
using Rasm.Persistence.Element;                   // FaultBand, IdentityShapeRow, CompiledModels (identity#ELEMENT_IDENTITY)
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected

// --- [TYPES] ----------------------------------------------------------------------------

// Closed analytical-lane vocabulary — the ONE owner of every lane token in the estate. Each row is the capability a
// deployment either realizes or refuses, and both sides of the question read this vocabulary: `StoreProfile.Lanes`
// declares which rows an engine realizes, and `ServerExtension.Lane` declares which row an extension serves, so the
// roster and the gate cannot drift. Loose text is the deleted form on both sides — a bare `"columnar"` in either
// place forks one vocabulary into two spellings with no compiler between them, and a misspelling reads as a
// permanent refusal on a lane the profile actually admits (`libs/csharp/.planning/RULINGS.md` `[02]-[SHAPE]`).
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
    public static readonly StoreProfile Server = new("server", relational: "postgresql-18", isEmbedded: false,
        lanes: [Lane.Columnar, Lane.Geo, Lane.Cypher, Lane.Vector, Lane.Search, Lane.Maintenance, Lane.Audit, Lane.Validation, Lane.Egress, Lane.Cache],
        degrade: "none at the relational tier — every analytical lane the extension roster serves is reachable, and a lane whose extension is absent folds through its own `FailureRank`",
        model: static () => CompiledModels.Server, rebuildsAlters: false, retriesInStrategy: true, nativeBulk: Some("binary-copy"),
        ef: static (builder, connection) => builder.UseNpgsql(connection, static npgsql => npgsql
            .MigrationsAssembly(IdentityShapeRow.Server.Migrations)).UseModel(CompiledModels.Server));
    public static readonly StoreProfile Embedded = new("embedded", relational: "sqlite", isEmbedded: true,
        lanes: [Lane.Validation],
        degrade: "single-writer, single-process, no server extension: the columnar, geo, cypher, vector, search, maintenance, audit, egress, and cache lanes have no embedded realization — Marten backs both cache residences, so a single-process store realizes neither — and a profile-level `Admits` refusal states each absence at ADMISSION where the server tier's `FailureRank.Degradable` would have folded the lane out, JSON Schema validation surviving only because it degrades to the in-process fence",
        model: static () => CompiledModels.Embedded, rebuildsAlters: true, retriesInStrategy: false, nativeBulk: None,
        ef: static (builder, connection) => builder.UseSqlite(connection, static sqlite => sqlite
            .MigrationsAssembly(IdentityShapeRow.Embedded.Migrations)).UseModel(CompiledModels.Embedded));
    public string Relational { get; }
    public bool IsEmbedded { get; }
    // `Lanes` names what this engine can realize AT ALL, drawn from the `Lane` vocabulary rather than loose text,
    // beside the honest clause stating what the row gives up. The absence machinery was server-only — `FailureRank`,
    // `HeldLanes`, and `LaneFolded` all key on a `ServerExtension` — so an embedded deployment admitted every
    // analytical lane the 20-row roster serves with no fold and no receipt, then met each absence at its first
    // query. `Admits` moves that discovery to admission on BOTH engines.
    public FrozenSet<Lane> Lanes { get; }
    public string Degrade { get; }

    // ONE gate over two input shapes. The `Lane` arm is the axis test — set containment over the vocabulary, no
    // string compare anywhere on the path — and every caller inside this package composes a member. The `string`
    // arm is the BOUNDARY form: it RESOLVES an untyped token against the vocabulary first, so text no row names can
    // never match, which is what a consumer holding a wire token or its own declared constant binds today.
    public bool Admits(Lane lane) => Lanes.Contains(lane);
    public bool Admits(string lane) => Lane.TryGet(lane, out Lane row) && Lanes.Contains(row);
    private StoreProfile(string key, string relational, bool isEmbedded, Lane[] lanes, string degrade,
        Func<IModel> model, bool rebuildsAlters, bool retriesInStrategy, Option<string> nativeBulk,
        Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> ef) : this(key) =>
        (Relational, IsEmbedded, Lanes, Degrade, Model, RebuildsAlters, RetriesInStrategy, NativeBulk, Ef) =
            (relational, isEmbedded, lanes.ToFrozenSet(), degrade, model, rebuildsAlters, retriesInStrategy, nativeBulk, ef);

    // MODEL identity is this row, never the process: `Ef` mounts `Model()` through `UseModel`, which bypasses the
    // framework model cache whole, so the cache keying on context type plus design-time flag can never hand one
    // engine's mapping to the other. `Element/identity#ELEMENT_IDENTITY` owns the emission and the shape columns.
    public Func<IModel> Model { get; }
    // Engine facts the `Element/identity#STORE_OPERATION_BRACKET` rail reads as data: `RebuildsAlters` names the
    // engine that rewrites a table to alter a column, `RetriesInStrategy` whether the row's execution strategy
    // re-drives (so the bracket demands `verifySucceeded` on a non-idempotent tail), `NativeBulk` the write-mass
    // lane the engine spells — an absent lane is an absent slot, never a runtime not-supported throw.
    public bool RebuildsAlters { get; }
    public bool RetriesInStrategy { get; }
    public Option<string> NativeBulk { get; }

    // EF provider-bind row ([05] EF-Sqlite admission): provider variance as row DATA on the closed axis — ONE
    // identity DbContext maps both engines through the generated rail, each arm binding its own migrations
    // assembly and its own compiled model; raw ADO keeps EmbeddedRitual/EngineOps/HandleBridge untouched.
    public Func<DbContextOptionsBuilder, DbConnection, DbContextOptionsBuilder> Ef { get; }
}

// ADO spatial-codec policy row ([05] Npgsql.NetTopologySuite admission): geographyAsDefault / SRID /
// precision / ordinates are PROFILE policy values, never call-site literals — the EF plugin does not place
// codec on raw connections, so the data source composes it once for every raw Npgsql lane (the cypher
// pgrouting results, the verification probes over PostGIS, any QueueSqlCommand spatial write).
public sealed record SpatialWire(bool GeographyAsDefault, int Srid, Ordinates HandleOrdinates) {
    public static readonly SpatialWire Canonical = new(GeographyAsDefault: false, Srid: 4326, Ordinates.XYZ);
}

// Profile-level tracing posture ([05] Npgsql admission), the SpatialWire peer on the same builder: the catalog
// rules the NpgsqlTracingOptionsBuilder filters profile policy, never per-call-site, so the predicate values ride
// this row — the binary-COPY spans the columnar importer emits per bulk load and the observability harvest's own
// polling statements drop at the source, and the verification batch names one span instead of six. Every other
// subscribed provider already carries a filter posture; the branch's highest-frequency lane no longer traces bare.
public sealed record TraceWire(
    Func<NpgsqlCommand, bool> CommandFilter,
    Func<NpgsqlBatch, bool> BatchFilter,
    bool CopySpans,
    bool FirstResponseEvent,
    bool PhysicalOpenSpans) {
    // Harvest filtering keys on the pg_stat view names the [PG_STAT_HARVEST] statements read FROM — real
    // statement text, never a minted comment marker a second page would have to remember to stamp.
    public static readonly TraceWire Canonical = new(
        CommandFilter: static command => !command.CommandText.Contains("pg_stat_", StringComparison.Ordinal),
        BatchFilter: static _ => true,
        CopySpans: false,
        FirstResponseEvent: false,
        PhysicalOpenSpans: true);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RestartClass {
    // `RestartClass` ranks a preload/setting repair's disruption — `session` is a `SET`/reconnect, `reload` an
    // `pg_reload_conf()`, `restart` a full cluster bounce; a `MissingPreload`/`SettingDrift` verdict carries it, so
    // that gap names what its repair costs the operator, never a bare "fix it" signal.
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FailureRank {
    // absence policy IS behavior-carrying row data (`#SERVER_EXTENSIONS`): the floor-miss branch threads
    // receipt through one `Absorb` delegate, so `Required` refuses the profile and stays minimal, `Degradable`
    // folds the lane out so absence surfaces at admission not first query, `Observational` records evidence — a new
    // rank lands as one row, the fold carrying zero rank arms. Every receipt is a TYPED `ServerFault` case
    // deriving off the 8380 registry row — the loose `Error.New(8371/8372/8373)` integers are the deleted form.
    public static readonly FailureRank Required = new(
        "required",
        static (_, key) => Fin.Fail<Seq<Error>>(new ServerFault.RequiredAbsent(key)));
    public static readonly FailureRank Degradable = new(
        "degradable",
        static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.LaneFolded(key))));
    public static readonly FailureRank Observational = new(
        "observational",
        static (receipts, key) => Fin.Succ(receipts.Add(new ServerFault.Evidence(key, "<absent>"))));

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
    public static readonly ServerExtension Timescaledb = new("timescaledb", new ExtensionAdmission.Preload("timescaledb"), Lane.Columnar, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension TimescaledbToolkit = new("timescaledb_toolkit", new ExtensionAdmission.BaseType("timescaledb"), Lane.Columnar, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgDuckdb = new("pg_duckdb", new ExtensionAdmission.Preload("pg_duckdb"), Lane.Columnar, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension Postgis = new("postgis", new ExtensionAdmission.Standalone("operator classes over the built-in gist AM; registers no custom access method"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension ApacheAge = new("age", new ExtensionAdmission.Standalone("openCypher graph functions + the agtype type over built-in storage; registers no custom AM, CREATE EXTENSION needs no preload, Cypher connections issue per-session LOAD 'age'"), Lane.Cypher, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgCron = new("pg_cron", new ExtensionAdmission.Preload("pg_cron"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgSqueeze = new("pg_squeeze", new ExtensionAdmission.Preload("pg_squeeze"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension Pgaudit = new("pgaudit", new ExtensionAdmission.Preload("pgaudit"), Lane.Audit, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PostgisRaster = new("postgis_raster", new ExtensionAdmission.BaseType("postgis"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PostgisSfcgal = new("postgis_sfcgal", new ExtensionAdmission.BaseType("postgis"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvector = new("vector", new ExtensionAdmission.AccessMethod("hnsw"), Lane.Vector, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgvectorscale = new("vectorscale", new ExtensionAdmission.BaseType("vector"), Lane.Vector, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension PgSearch = new("pg_search", new ExtensionAdmission.Preload("pg_search"), Lane.Search, FailureRank.Degradable, RestartClass.Restart);
    public static readonly ServerExtension H3Pg = new("h3", new ExtensionAdmission.Standalone("operator classes over the built-in btree/hash/brin/spgist AMs; registers no custom access method"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension H3Postgis = new("h3_postgis", new ExtensionAdmission.BaseType("h3"), Lane.Geo, FailureRank.Degradable, RestartClass.Reload);
    public static readonly ServerExtension Pgrouting = new("pgrouting", new ExtensionAdmission.BaseType("postgis"), Lane.Cypher, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgPartman = new("pg_partman", new ExtensionAdmission.Preload("pg_partman_bgw"), Lane.Maintenance, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgJsonschema = new("pg_jsonschema", new ExtensionAdmission.Standalone("CREATE EXTENSION-registered json_matches_schema/jsonb_matches_schema CHECK functions; no preload, no custom AM"), Lane.Validation, FailureRank.Observational, RestartClass.Reload);
    public static readonly ServerExtension PgNet = new("pg_net", new ExtensionAdmission.Preload("pg_net"), Lane.Egress, FailureRank.Observational, RestartClass.Restart);
    public static readonly ServerExtension PgGraphql = new("pg_graphql", new ExtensionAdmission.Standalone("pgrx SQL functions + DDL event triggers; no background worker, no preload, no custom AM"), Lane.Egress, FailureRank.Observational, RestartClass.Reload);

    public ExtensionAdmission Admission { get; }
    public Lane Lane { get; }
    public FailureRank Rank { get; }
    public RestartClass Restart { get; }
    private ServerExtension(string key, ExtensionAdmission admission, Lane lane, FailureRank rank, RestartClass restart) : this(key) =>
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

    // `Satisfied` admits a min-threshold knob at or above its floor and an exact-match knob (`wal_level`) by equality.
    public bool Satisfied(string actual) => AtLeast
        ? long.TryParse(actual, NumberStyles.Integer, CultureInfo.InvariantCulture, out long held)
            && long.TryParse(Expected, NumberStyles.Integer, CultureInfo.InvariantCulture, out long expected)
            && held >= expected
        : string.Equals(actual, Expected, StringComparison.OrdinalIgnoreCase);
}

// --- [MODELS] ---------------------------------------------------------------------------

[ValueObject<long>]
public readonly partial struct VerificationEpoch {
    // `Verify` stamps this monotonic epoch on every fold, so cluster drift between two verifications reads as an
    // epoch advance on the fact stream rather than an unmarked re-probe, and the AppHost health probe reads the delta.
    public static VerificationEpoch From(Instant at) => From(at.ToUnixTimeMilliseconds());
}

// `RepairArtifact` carries what verification EMITS and never executes: a `shared_preload_libraries` diff, a
// `CREATE EXTENSION` reconciliation, or a settings diff the operator applies at the named rung under the named
// restart class.
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

// Marten-document partition roster, the SERVER-side `MaintenanceJob` roster's document-side peer: a family whose
// WHOLE table ages out on one declared bound retires its trailing edge as a constant-time partition DROP and
// provisions its leading edge ahead of the clock, so the family stops paying a per-row age sweep. Each row is
// `(period, ahead, aged)` policy DATA the family's own owner declares through `Declare` and the single-writer
// boot pass rolls (`Element/identity#SCHEMA_VERDICT`).
// `Aged` IS the family's trailing bound expressed in periods, seated one period BEYOND the
// `Version/retention#RETENTION_CLASSES` class bound the family admits under, so a drop never outruns the verdict
// fold: the L2 cache blob rolls daily past the `cache` class's seven-day bound, the egress dead-letter registry
// weekly past the `evidence` class's ninety-day bound. Admission is the WHOLE-TABLE test — a family every one of
// whose rows shares one class and therefore one bound admits a row, and one carrying a never-evict,
// reachability-shielded, or mixed-class row keeps the per-row receipted sweep instead.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RollingWindow {
    public static readonly RollingWindow CacheBlob = new("cache-blob", PartitionPeriod.Day, ahead: 2, aged: 8);
    public static readonly RollingWindow DeadLetter = new("dead-letter", PartitionPeriod.Week, ahead: 2, aged: 14);

    // ONE instance across the whole roster: the roll pass matches document types by REFERENCE identity, so a
    // per-row instance rolls each table in its own pass while this one folds every rostered table into a single
    // pass. `Declare` is the only site that hands it out, which is what makes the sharing structural.
    static readonly ManagedRangePartitions Managed = new();

    public PartitionPeriod Period { get; }
    public int Ahead { get; }
    public int Aged { get; }
    private RollingWindow(string key, PartitionPeriod period, int ahead, int aged) : this(key) =>
        (Period, Ahead, Aged) = (period, ahead, aged);

    // `Declare` returns the threaded `StoreOptions` because the CALLER is the family's own owner, one stratum
    // above this roster: the `Element/graph#STREAM_GRAIN` spine seat registers spine-owned mappings alone (a
    // rolling declaration over a `Query`/`Version` document type there walks the forbidden upward edge), so each
    // family publishes a `StoreOptions -> StoreOptions` contribution the composition root folds after that seat
    // and this row supplies the policy that contribution reads. Every rostered family names its
    // key `Window` — a `DateTimeOffset` the row either stamps at admission or projects off its own canonical
    // `Instant` — so the key is one convention across the roster rather than a per-family spelling.
    // `ByRollingRange` asserts the DUPLICATED date member at CONFIGURATION time, so a family whose key is nullable
    // or absent fails composition rather than the first write; the injected `TimeProvider` moves the window, so a
    // window test drives the clock instead of the calendar. A `DEFAULT` overflow partition always exists, so an
    // out-of-window row stores rather than failing its check constraint, and only partitions this policy itself
    // named are ever dropped.
    public StoreOptions Declare<T>(StoreOptions opts, Expression<Func<T, DateTimeOffset>> key) where T : notnull {
        opts.Schema.For<T>().PartitionOn(key, x => x.ByRollingRange(Managed, Period, Ahead, Aged));
        return opts;
    }
}

// Closed deployment-axis vocabulary the reconcile manifest keys on, mirroring the `RestartClass` shape its sibling
// column already reads: one owner, every construction drawing from it, so a bare literal cannot fork one axis into
// two spellings a deploy plane then diffs as two axes. `Manifest` folds seven row families and three of these rows
// answer all of them, which is exactly why the vocabulary closes rather than growing per family.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReconcileAxis {
    public static readonly ReconcileAxis RelationalSor = new("relational-sor");
    public static readonly ReconcileAxis Maintenance = new("maintenance");
    public static readonly ReconcileAxis EmbeddedRelational = new("embedded-relational");
}

public sealed record ExtensionFloor(string Minimum, Func<string, string, bool> Satisfied);

public sealed record ClusterDemand(Seq<ServerExtension> Required, HashMap<string, ExtensionFloor> Floors, VerificationEpoch Epoch);

// desired-state manifest — everything verification already asserts, egressed as ONE typed wire record the
// deploy plane converges on: the extension roster with its gates and restart classes, the verified server
// postures, the in-database maintenance-job roster, and the embedded-floor pragma/config set, the store-axis
// coordinate naming each row's `#STORE_AXIS_MAP` axis — so server drift is a diff between two typed documents,
// a fleet provisioning script derives from the manifest instead of restating the roster by hand, and
// in-process provisioning stays verification-only (the manifest DESCRIBES, `Verify` asserts, the operator applies).
// `Axis` and `Restart` are both KEY columns off closed vocabularies — `ReconcileAxis` and `RestartClass` — so the
// wire record stays flat text a deploy plane diffs while every construction reads a member and no row can name an
// axis or a disruption class the estate does not own.
public sealed record ReconcileRow(string Axis, string Key, string Declared, string Restart);

public sealed record ReconcileManifest(Seq<ReconcileRow> Rows, VerificationEpoch Epoch);

public sealed record JsonValidationContract(string Text, Json.Schema.JsonSchema Parsed) {
    public static Fin<JsonValidationContract> Parse(string text) {
        try {
            return Fin<JsonValidationContract>.Succ(
                new JsonValidationContract(text, Json.Schema.JsonSchema.FromText(text)));
        } catch (Json.Schema.JsonSchemaException failure) {
            return Fin<JsonValidationContract>.Fail(
                new ServerFault.AdmitRefused($"<schema:{failure.Message}>"));
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProvisionVerdict {
    private ProvisionVerdict() { }
    // `Created` doubles as the backend contract's capability evidence: contract capability rows key on the same
    // `ServerExtension.Key` space this set carries, so `BackendObservation.Of` projects it with no adapter table
    // between the two — the probe IS the observation, and only a `Provisioned` verdict is admissible evidence. This
    // verdict answers the CAPABILITY half alone: the observation also carries its adapter's own read instant and the
    // two recovery stamps its `RecoveryWindow` derives from, and those enter `Of` as caller arguments because a
    // verification batch witnesses no durability frontier and no restore span.
    public sealed record Provisioned(
        Seq<ServerExtension> Present,
        FrozenSet<string> Preloaded,
        FrozenSet<string> Created,
        FrozenSet<Lane> HeldLanes,
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
    public static ReconcileManifest Manifest(ClusterDemand demand, EmbeddedRitual ritual) => new(
        demand.Required.Map(static row => new ReconcileRow(ReconcileAxis.RelationalSor.Key, row.Key, row.CreateSql, row.Restart.Key))
        + toSeq(ClusterSetting.Items).Map(static row => new ReconcileRow(ReconcileAxis.RelationalSor.Key, row.Key, row.Expected, row.Restart.Key))
        + toSeq(MaintenanceJob.Items).Map(static row => new ReconcileRow(ReconcileAxis.Maintenance.Key, row.Key, row.RegisterSql, row.Owner.Restart.Key))
        + toSeq(RollingWindow.Items).Map(static row => new ReconcileRow(ReconcileAxis.Maintenance.Key, row.Key, $"{row.Period}:+{row.Ahead}/-{row.Aged}", RestartClass.Session.Key))
        + Seq(new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, "<cipher-provider>", "SQLitePCLRaw.bundle_e_sqlite3mc", RestartClass.Restart.Key))
        + ritual.ConnectionRows.Map(static row => new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, row.Row, row.Sql, RestartClass.Session.Key))
        + ritual.DbConfig.Map(static row => new ReconcileRow(ReconcileAxis.EmbeddedRelational.Key, row.Row, row.Value.ToString(CultureInfo.InvariantCulture), RestartClass.Session.Key)),
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
        // `missing` holds the installable-but-uncreated rows — present in `pg_available_extensions` with a satisfied
        // gate — which the operator reconciles by `CREATE EXTENSION`; a row whose library is unavailable on disk
        // stays out, routing to the survivor fold's `FailureRank.Absorb` because no `CREATE EXTENSION` repair fixes
        // a missing binary.
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
            ? new ProvisionVerdict.Provisioned(fold.Held, preloaded, created, fold.Held.Map(static e => e.Lane).ToFrozenSet(), fold.Receipts, demand.Epoch)
            : new ProvisionVerdict.MissingExtension(fold.Absent, Seq<RepairArtifact>(), demand.Epoch);
    }

    static ProvisionVerdict Folded(Error error, VerificationEpoch epoch) =>
        new ProvisionVerdict.Faulted(error is PostgresException { SqlState: PostgresErrorCodes.InsufficientPrivilege } denied
            ? new ServerFault.CatalogDenied(denied.TableName ?? "pg_catalog")
            : error is NpgsqlException { IsTransient: true } transient
                ? new ServerFault.Unreachable(transient.Message)
                : new ServerFault.Unmapped(error is PostgresException pg ? pg.SqlState : "none", error.Message), epoch);

    // Admission RE-GATES at the entry, never trusts the caller pre-filtered, and it gates TWICE on row data: the
    // profile must realize the row's OWN `Lane` and the live cluster must satisfy its `ExtensionAdmission`. Lane
    // realizability leads because a profile that cannot host the lane at all makes the cluster question moot, and it
    // reads the extension's own column rather than a call-site token — so `geo`, `maintenance`, and `audit` gate
    // here through their rows exactly as `columnar` and `cypher` gate at their own analytical entries, and a lane
    // added to the roster needs no new gate. An extension whose cluster gate is unmet (a preload library absent, a
    // base type uncreated) REFUSES with no DDL queued, because a `CREATE EXTENSION` against an unmet gate is a
    // guaranteed runtime error. Both misses rail ONE `Ungated` case naming the extension: both answer the same
    // question at the same door — this extension cannot be admitted here — and splitting them would mint an
    // eleventh case in a decade the band has fully spent. The `preloaded`/`created` sets are the ones the caller's
    // `Verify` fold already read (no second catalog probe), so the gate costs nothing beyond a set membership test.
    public static IO<Fin<Unit>> Admit(StoreProfile profile, IDocumentSession session, ServerExtension extension, ProvisionVerdict.Provisioned cluster) =>
        !profile.Admits(extension.Lane) || !extension.Admission.Admissible(cluster.Preloaded, cluster.Created)
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

    // Maintenance registration rides the SAME gated-admission discipline as `Admit`, on the same two row columns —
    // profile realizability for the OWNING extension's lane, and that extension already created (a folded-out lane
    // registers nothing — `Ungated`); the idempotent registration SQL queues on the one session, and the commit
    // rides `SaveChangesAsync` with the schema migration. The registration is verification-compatible — it writes
    // only extension-owned registration rows (`cron.job`, `partman.part_config`, squeeze registration), never a
    // cluster setting, so the never-`ALTER SYSTEM` law holds.
    public static IO<Fin<Unit>> Register(StoreProfile profile, IDocumentSession session, MaintenanceJob job, ProvisionVerdict.Provisioned cluster) =>
        !profile.Admits(job.Owner.Lane) || !cluster.Created.Contains(job.Owner.Key)
            ? IO.pure(Fin<Unit>.Fail(new ServerFault.Ungated(job.Owner.Key)))
            : (IO.liftAsync(async () => {
                session.QueueSqlCommand(job.RegisterSql);
                await session.SaveChangesAsync().ConfigureAwait(false);
                return Fin<Unit>.Succ(unit);
            }) | @catch<IO, Fin<Unit>>(static _ => true, e => IO.pure(Fin<Unit>.Fail(new ServerFault.AdmitRefused(e.Message))))).As();

    // Server row's data-source build ([05] Npgsql.NetTopologySuite, Npgsql.NodaTime): the ADO codecs compose ONCE
    // on the owning NpgsqlDataSourceBuilder — the spatial one from the `SpatialWire` policy row — so every raw lane
    // (cypher pgrouting decode, verification probes, QueueSqlCommand spatial writes) reads/writes NTS geometry. The
    // TEMPORAL codec binds on the same builder for the same reason its spatial sibling does: a raw lane without it
    // degrades to the platform date the mapped lane never produces, so a `QueueSqlCommand` instant write, a binary
    // import, and a verification probe would each read a different temporal dialect than `Element/identity` maps.
    // It takes no policy row because NodaTime's mapping carries no ordinate, precision, or default-type choice to
    // make. `Name` (`string?`, get/set) assigns the logical-database identity here — the Persistence half of the PORT-peer
    // telemetry split: `db.client.connection.pool.name` keys stable pool dimensions on the `Npgsql` meter the
    // AppHost root subscribes, and an unnamed source collapses every pool into one anonymous series.
    public static NpgsqlDataSource Source(string dsn, string name, SpatialWire wire, TraceWire trace) {
        NpgsqlDataSourceBuilder builder = new(dsn) { Name = name };
        builder.UseNetTopologySuite(handleOrdinates: wire.HandleOrdinates, geographyAsDefault: wire.GeographyAsDefault);
        builder.UseNodaTime();
        builder.ConfigureTracing(tracing => tracing
            .ConfigureCommandFilter(trace.CommandFilter)
            .ConfigureBatchFilter(trace.BatchFilter)
            .ConfigureCopyOperationFilter(copy => trace.CopySpans)
            .EnableFirstResponseEvent(trace.FirstResponseEvent)
            .EnablePhysicalOpenTracing(trace.PhysicalOpenSpans));
        return builder.Build();
    }

    // pg_jsonschema dual-residence fence ([05] JsonSchema.Net): ONE schema text serves both residences — the held
    // lane checks server-side (`json_matches_schema` in a CHECK/predicate), and a folded-out lane degrades to the
    // in-process `Json.Schema.JsonSchema.Evaluate(JsonElement, EvaluationOptions?)` boolean parity gate, so absence
    // of the server extension narrows residence, never capability. Lane identity reads the owning extension's own
    // column, so the roster and this residence test can never spell one lane two ways.
    public static Fin<bool> SchemaCheck(
        FrozenSet<Lane> heldLanes,
        JsonValidationContract schema,
        JsonElement instance,
        Func<string, JsonElement, bool> serverCheck) {
        try {
            return Fin<bool>.Succ(heldLanes.Contains(ServerExtension.PgJsonschema.Lane)
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
|  [19]   | rolling windows     | `RollingWindow` rows via `Declare`                | one shared manager; one table, one partition manager |
|  [20]   | temporal wire       | `UseNodaTime()` on `Source`                       | raw lanes read the branch instant, never a date      |
|  [21]   | lane gate           | `Admits(row.Lane)` at `Admit`/`Register`          | geo/maintenance/audit gate off row data              |
|  [22]   | lane vocabulary     | `Lane` `[SmartEnum<string>]` owns the tokens      | roster and gate compose members; bare text deleted   |

## [03]-[EMBEDDED_FLOOR]

- Owner: `EmbeddedRitual` the idempotent open-ritual record carrying the file-persistent provisioning rows, the per-connection pragma rows, the defensive `DbConfig` set, and the connection-scoped `Capability` registrations (each a named `Action<SqliteConnection>` grant); `EmbeddedStore` the static surface owning the dialed connection, the KMS-custodied key application, the residency-split fold, the first-opener IMMEDIATE migration gate, the rekey rotation, and the closed-engine law — the bound provider is the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle (`Batteries_V2.Init()` binds `SQLite3Provider_e_sqlite3mc`; one provider per process), so the embedded floor is ENCRYPTED at rest wherever a data key is supplied and the plain open is the same ritual with the key slot `None`.
- Cases: the ritual's `ConnectionRows` are the per-connection pragmas (`synchronous=NORMAL`, `journal_size_limit`, `temp_store=MEMORY`, `cache_size`) the fold re-applies on every open; the `Capabilities` are the schema-resident registrations (`uuid7`/`xxh128` scalar UDFs and the `instant_iso` collation the identity policy and chronological ordering need, a domain aggregate) that register before the first statement or the file is unreadable; the `DbConfig` set is the defensive-mode + double-quoted-literal-rejection posture applied through the raw `Handle`; the file-persistent `application_id`/`user_version` are provisioning identity the migration gate writes, never per-connection.
- Entry: `public static SqliteConnection Dialed(string path)` opens a non-pooled embedded connection with the canonical connection-string posture (`ForeignKeys`, `ReadWriteCreate`); `public static Fin<Seq<RitualFact>> Open(SqliteConnection store, EmbeddedRitual ritual, Option<ReadOnlyMemory<byte>> dek, Action<SqliteConnection, SqliteTransaction, long> migrate)` folds the declared ritual end-to-end idempotently — the supplied data key applies FIRST through `raw.sqlite3_key(handle, dek.Span)` before any statement touches a data page (the `Element/identity#KMS_CUSTODY` `EnvelopeKeyring.Unwrap` recovers it and the caller zeroizes through `CryptographicOperations.ZeroMemory` after the keyed open, so no passphrase persists past the crossing); `public static Fin<Unit> Rekey(SqliteConnection store, ReadOnlyMemory<byte> next)` rotates the store key in place through `raw.sqlite3_rekey(handle, next.Span)` after a fresh custody mint — an empty `next` strips the cipher for a plaintext export — identity check, per-connection pragma rows, defensive `sqlite3_db_config` hardening, extended-result-code arming, capability registration, the IMMEDIATE migration gate, the epoch read — every throwing provider call staying INSIDE the `Fin` boundary so a provider fault converts to `EmbeddedFault` and disposes the connection on every failure path rather than escaping with a leaked live handle; `migrate` is the first-opener step run under the one IMMEDIATE transaction when the held epoch trails the compiled epoch.
- Auto: every connection in every process folds the SAME declared sequence so bootstrap, crash-recovery reopen, and steady-state open are one fold with no first-process special case — the identity check rejects a foreign `application_id`, the per-connection pragma rows apply (`synchronous=NORMAL` the WAL throughput row whose loss boundary is the last commits and never corruption), the defensive `sqlite3_db_config(Handle, SQLITE_DBCONFIG_DEFENSIVE, 1)` and `DQS_DDL=0`/`DQS_DML=0` harden against direct b-tree writes and double-quoted string literals (so a double-quoted literal is a prepare-time syntax error, identifiers quoting with `"` and strings with `'`), `sqlite3_extended_result_codes(Handle, 1)` upgrades the running taxonomy where receipts must discriminate (`BUSY_SNAPSHOT` from plain `BUSY`), the capabilities register connection-instance-scoped (never persisted — `isDeterministic: true` admits the UDF into expression indexes and generated columns), the first-opener migration runs the `migrate` step under one IMMEDIATE transaction (losers blocked on the lock observe the bumped `user_version` on acquisition and no-op, a register ahead of the compiled epoch a typed rejection so correctness needs no leader election), and `PRAGMA data_version` is the polling-free cross-process change probe `EngineOps` reads; any write transaction begins IMMEDIATE so a deferred read attempting its first write never burns the busy budget on a stale-snapshot retry, and the provider already retries `BUSY`/`LOCKED` at managed quanta so a nonzero `busy_timeout` is the deleted form.
- Receipt: an open rides `store.embedded.open` carrying the ritual fact count, the keyed bit, and the epoch; a rotation rides `store.embedded.rekey` carrying the wrapping-key version advance, never key material.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`/`CreateFunction`/`CreateAggregate`/`CreateCollation`/`BeginTransaction(IsolationLevel, deferred)`), SQLitePCLRaw.bundle_e_sqlite3mc (`Batteries_V2.Init()` binding `SQLite3Provider_e_sqlite3mc`; the keying delta `raw.sqlite3_key(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_key_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey(sqlite3, ReadOnlySpan<byte>)`/`raw.sqlite3_rekey_v2(sqlite3, utf8z, ReadOnlySpan<byte>)`; the carried-over raw surface `raw.sqlite3_db_config(sqlite3, int, int, out int)`/`raw.sqlite3_extended_result_codes`/`raw.SQLITE_DBCONFIG_DEFENSIVE`=1010/`raw.SQLITE_DBCONFIG_DQS_DDL`=1014/`raw.SQLITE_DBCONFIG_DQS_DML`=1013 — backup, snapshot, WAL, db_config, and serialize calls carry over the `mc` provider unchanged), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new pragma is one `ConnectionRows` row; a new capability is one `Capabilities` registration; a new defensive posture is one `DbConfig` row; zero new surface — a second embedded relational engine (libSQL, LiteDB, RavenDB.Embedded, `Realm`, hctree, embedded-pg, EF InMemory), a per-process bootstrap branch, a nonzero `busy_timeout`, a persisted capability, or a `locking_mode=EXCLUSIVE`/shared-cache posture is the deleted form because the engine sweep is closed, the ritual is the one open path, and the provider already retries `BUSY`/`LOCKED`.
- Boundary: the embedded SQLite floor is the single-process embedded store beneath the server tier — the one engine sweep is CLOSED (PostgreSQL + embedded SQLite only; libSQL, LiteDB, RavenDB.Embedded, `Realm`, hctree, embedded-pg, EF InMemory all rejected) so a new engine row is the named defect; `StoreProfile` and the `Store/schema#CONTRACT` `BackendProvider` axis are DISJOINT vocabularies and neither rejects the other's rows — a profile row names an engine THIS package opens and provisions in process, a provider row names an engine identity a schema GENERATION is minted for anywhere in the estate, so PGlite is not a rejected engine but a category the profile axis cannot spell: it publishes no .NET provider and this package never opens one, while it IS PostgreSQL at the contract grain (its wire error carries the pg `code` and `constraint` verbatim), so a generation minted for postgres serves a peer-hosted PGlite unchanged; and the embedded floor and the PostgreSQL server tier are two engines on the one `StoreProfile` axis (`#SERVER_EXTENSIONS` `StoreProfile`), the profile selecting one by deployment, never a third; pragma rows carry RESIDENCY — file-persistent rows (`journal_mode`, `application_id`, `user_version`) are provisioning identity the migration gate writes and the ritual folds ONLY per-connection rows; capability registration is connection-instance-scoped and never persisted — schema-resident functions, aggregates, and collations register before the first statement or the file is unreadable, and `isDeterministic: true` is the capability grant admitting a function into expression indexes and generated columns; every embedded connection is non-pooled because a physical handle's cipher identity is fixed by its first key bind and path-only pooling can return a handle keyed under different material; the WAL `-wal`/`-shm` sidecar set is the unit of copy/replace/delete (a main file separated from its sidecars is silent page-level corruption); STRICT tables are the typed admission gate and `RETURNING` supersedes write-then-read identity round trips; the defensive `sqlite3_db_config` set and double-quoted-literal rejection are connection POLICY applied through the `Handle` raw bridge (`api-sqlite#IMPLEMENTATION_LAW`), not connection-string knobs; extension loading stays FULLY disabled — the `Canonical` ritual arms neither the SQL `load_extension()` function nor the C-API loader (`SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` is absent from the `DbConfig` set), so the bundled floor admits no runtime extension and a `DbConfig` row arming the loader is the deliberate opt-in a deployment that needs one adds, never the default; encryption-at-rest is the BOUND provider's law — the `SQLitePCLRaw.bundle_e_sqlite3mc` cipher bundle supersedes the plain `e_sqlite3` bundle where the encrypted floor mounts (one provider binds per process, so the selection is this provisioning row, never a per-connection knob), key material is the KMS-unwrapped DEK crossing as `ReadOnlySpan<byte>` through `raw.sqlite3_key` and zeroized after the bind, a `Password=` connection-string value exists only for the ephemeral open of an inspected foreign store and never enters durable configuration, and classification ceilings thereby extend to the offline lane — a stolen laptop or synced file leaks nothing; the ritual is the one open path so a per-process bootstrap branch is the deleted form.

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
            // Statement-registry arm leads the roster, and the roster itself lands before this ritual's own
            // first statement: the managed wrapper maps a statement's native pointer only while armed, so
            // `Store/observability#SQLITE_STATUS_HARVEST`'s walk throws both on an unarmed connection and on
            // any handle prepared ahead of the grant, and both orderings together make each unreachable.
            ("<stmt-registry>", static store => ignore(SqliteStatHarvest.Arm(store))),
            ("<uuid7>", static store => store.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("N"), isDeterministic: false)),
            // Full-width content key as a 16-byte big-endian BLOB — the same encoding CloudRunKey.Content writes —
            // so the UDF's output joins a stored ContentAddress column byte-for-byte; the codec law rules a 64-bit
            // truncation the deleted form that collides distinct contents.
            ("<xxh128>", static store => store.CreateFunction("xxh128", static (byte[] bytes) => {
                byte[] key = new byte[16];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(key, System.IO.Hashing.XxHash128.HashToUInt128(bytes));
                return key;
            }, isDeterministic: true)),
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
            // Capability registration is the crossing that immediately follows the key bind, because every row
            // in that roster is a BEFORE-FIRST-STATEMENT grant: the statement-registry arm maps a statement's
            // native pointer only while armed, and a schema-resident function, aggregate, or collation absent
            // when a statement naming it prepares makes the file unreadable. Applying the roster after the
            // identity probe and the pragma rows leaves every one of those grants late by exactly the
            // statements this method itself runs.
            Seq<RitualFact> facts = ritual.Capabilities.Map(row => (fun(() => row.Grant(store))(), new RitualFact(row.Row, 1L)).Item2);
            long identity = Scalar(store, "PRAGMA application_id");
            if (identity != ritual.Identity && identity != 0L) { return Refused(store, $"<foreign-store:{identity:x8}>"); }
            facts += ritual.ConnectionRows.Map(row => new RitualFact(row.Row, Execute(store, row.Sql)));
            _ = raw.sqlite3_extended_result_codes(handle, 1);
            facts += ritual.DbConfig.Map(row => new RitualFact(row.Row, raw.sqlite3_db_config(handle, row.Op, row.Value, out int applied) == raw.SQLITE_OK ? applied : -1L));
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
    // KMS mint (`Custody.Wrap` -> new DEK), the wrapped DEK persisting beside the store; an EMPTY `next`
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
|  [04]   | capability scope  | connection-instance registration        | grants apply per physical open, never persisted       |
|  [05]   | migration gate    | first-opener IMMEDIATE transaction      | losers observe the bumped epoch; no leader election   |
|  [06]   | write transaction | IMMEDIATE begin                         | a deferred-then-write burns the busy budget           |
|  [07]   | engine sweep      | closed (PostgreSQL + SQLite only)       | a new embedded engine row is the named defect         |
|  [08]   | sidecar unit      | `-wal`/`-shm` set                       | a main file without its sidecars is silent corruption |
|  [09]   | cipher floor      | `e_sqlite3mc` + KMS-unwrapped DEK       | `sqlite3_key` first crossing; `sqlite3_rekey` rotates |
|  [10]   | key custody       | `KMS_CUSTODY` envelope algebra          | plaintext zeroized after bind; never persisted        |
|  [11]   | lane admission    | `StoreProfile.Admits` + `Degrade`       | absence states at admission on BOTH engines           |

## [04]-[ENGINE_OPERATIONS]

- Owner: `HandleBridge` projects `SqliteConnection.Handle` into typed `Fin` results; `CheckpointMode`, `SnapshotFloor`, `BackupPolicy`, `BlobBinding`, `EmbeddedFact`, and `EmbeddedFault` carry the native operation policy, lifetime, validation, target, receipt, and closed fault family, and `RetryShape` is the ONE re-drive vocabulary every embedded fault publishes, its four routes dispatched TOTALLY through `EmbeddedFault.Reoffer` over the caller's own re-entry arrows. `EngineOps` owns checkpoint, consistent snapshot, validated paged backup, preallocated blob IO, and integrity. `KvSpace` is the `[SmartEnum<string>]` keyspace axis BOTH KV engines realize — one row IS a RocksDB column family and an LMDB named database — carrying its `KvOrder` key order, its `Option<MergeOperator>` accrual, its fixed dup width, its `KvDurability` sync posture, its `KvSeal` at-rest posture, and its LSM compaction style; `SpoolAccrual` owns the framed merge operator, `KvVault` the AEAD value seal, `RocksStatus`/`RocksSubcode` the message taxonomy a codeless `RocksDbException` forces, `KvMount` the closed open request; `KvEngine` and `KvFloor` realize the pending-op spool and the local content-address index through one polymorphic space-keyed surface, and remote object residence remains exact-object provider evidence that never consults this local index.
- Cases: `CheckpointMode` is `Passive`/`Full`/`Restart`/`Truncate` (the `raw.SQLITE_CHECKPOINT_*` modes — `Truncate` the scheduled WAL-bound reset); `RetryShape` is `Terminal`/`Waited`/`Restarted`/`Rescoped`, so a fault names WHICH re-drive recovers it and `IsTransient` narrows to `Waited` alone; `Reoffer` is the total dispatch over those four, taking the caller's same-effect, re-read, and narrowed arrows so the two routes a bool discards reach their own re-entry point; `EmbeddedFault` is `Busy` (`SQLITE_BUSY`/`SQLITE_LOCKED`, its shape DERIVED from the full extended status the case already keeps rather than a second column), `Corrupt` (`SQLITE_CORRUPT`/`SQLITE_NOTADB`, terminal — routes to `Version/recovery`), `Io` (`SQLITE_IOERR`/`SQLITE_FULL`), `Refused` (a foreign store / epoch-ahead / pin regression), and `Kv` (the engine-named KV verdict carrying its shape as a column, because no engine status re-derives it); the integrity ladder orders boot `quick_check`, cycle `integrity_check` and `foreign_key_check`, a deeper-tier failure routing to restore, never retry; `KvSpace` is `Spool` (the pending `OpLogEntry`/`CrdtOp` rows a disconnected peer buffers), `Cursor` (both `SyncSession` watermarks — the pull resume point and the push-ack frontier), `ChunkIndex` (chunk key → owning `ContentAddress` dup set, the one row earning `DuplicatesSort|DuplicatesFixed`), and `Meta` (engine epoch and peer identity); `KvWrite` is `Put | Append | Unlink | Drop` — the dupsorted index answers two distinct retirements (one owner leaving a content address, the address leaving whole) so a single remove case can spell only the second, and `Append` is the accrual intent both engines own natively.
- Law: key order is proved by OMISSION — every `KvSpace` row declares `KvOrder.Bytewise` and NEITHER `DatabaseConfiguration.CompareWith` NOR `FindDuplicatesWith` is ever called, because LMDB's built-in comparator and RocksDB's default comparator are both byte-lexicographic and only an uncalled override leaves them in force; calling either is the deleted form, and `Scan` gates on the row's `PrefixSound` column so a future order lands a typed refusal instead of a silently truncated walk. LMDB's sync flags are ENVIRONMENT-scoped while RocksDB's `WriteOptions.SetSync` is per-write, so a per-space LMDB posture is inexpressible: the environment opens under the STRICTEST posture across the rostered spaces and each row's own column carries its LSM realization and its contribution to that floor.
- Entry: `Checkpoint(SqliteConnection, SnapshotFloor, CheckpointMode, ProjectionContext)` resets only the owning store's promoted pin on `Truncate`; `WithSnapshot<T>(SqliteConnection, SnapshotFloor, Func<SqliteConnection,T>)` promotes a comparable snapshot into that same disposable lifetime owner. `Backup(SqliteConnection, string, BackupPolicy, ProjectionContext)` binds the policy's `Dek` to the destination BEFORE `sqlite3_backup_init`, pages until completion, returns `Busy` without spinning, then requires destination `PRAGMA quick_check` and the policy's source/destination `ContentAddress` equality. `WriteBlob(SqliteConnection, BlobBinding, long, ReadOnlyMemory<byte>)` executes the binding's parameterized `zeroblob(@length)` row preallocation before opening `SqliteBlob`; `DataVersion` reads the cross-process change register. `KvFloor.Open(KvMount, ReadOnlyMemory<byte>)` folds the WHOLE `KvSpace` roster into the opened handle set and binds the vault, so every later space lookup is total; `Put`/`Get`/`Batch`/`Scan`/`Refs` each take the `KvSpace` whose row supplies their handle and posture, while `Since` and `Snap` take none because the WAL and the on-disk clone are store-wide facts spanning every space.
- Auto: each `SnapshotFloor` scopes native comparison and disposal to one store instead of comparing process-global handles from unrelated databases. Backup policy owns page quantum and semantic identity; `SQLITE_BUSY`/`SQLITE_LOCKED` returns to the schedule rather than hot-spinning inside the native loop. Blob target identifiers arrive only through a composition-time `BlobBinding`, while row id and length remain parameters. LMDB checks every `MDBResultCode`, maps only `NotFound` to `None`, admits a write only after `Commit` succeeds, and folds a RAISED `LightningException.StatusCode` through the same verdict table a returned code takes so one engine carries one taxonomy; the status discriminator masks the primary byte because the ritual arms extended result codes; RocksDB keeps span-first IO and atomic `WriteBatch`. `RocksDbException` publishes no code and no enum — its message IS RocksDB `Status::ToString()` — so the lsm arm folds that message ONCE by longest primary-prefix match, takes the subcode's shape over the status's where a subcode matches (a `max_num_locks` ceiling under a timeout is an operator act, not a wait), and mints a MEASURED `RetryShape`. Accrual is row data: an `Append` on a merge-carrying row is one `WriteBatch.Merge` the engine resolves at read and compaction, and the same intent on the mmap arm is a dup put into that key's set, so a disconnected peer never pays a read-modify-write; the value seal likewise reads its row, so one `Put` path serves a sealed and a clear space.
- Receipt: a checkpoint rides `store.embedded.checkpoint` carrying the mode and frame counts; a snapshot read rides `store.embedded.snapshot`; a backup rides `store.embedded.backup` carrying the page progress; a blob write rides `store.embedded.blob` carrying the byte count.
- Packages: Microsoft.Data.Sqlite (`SqliteConnection.Handle`, `SqliteBlob(connection, table, column, rowid, readOnly)`, `BackupDatabase`, `SqliteException.SqliteErrorCode`/`SqliteExtendedErrorCode`), SQLitePCLRaw.bundle_e_sqlite3mc (`raw.sqlite3_wal_checkpoint_v2`, `raw.sqlite3_snapshot_get`/`_open`/`_cmp`/`_recover`/`_free`, `raw.sqlite3_backup_init`/`_step`/`_remaining`/`_pagecount`/`_finish`, `raw.sqlite3_extended_errcode`, `raw.sqlite3_errstr`, the `SQLITE_CHECKPOINT_*`/`SQLITE_BUSY`/`SQLITE_BUSY_RECOVERY`/`SQLITE_BUSY_SNAPSHOT`/`SQLITE_LOCKED`/`SQLITE_CORRUPT`/`SQLITE_DONE` constants), rocksdb (`RocksDb.Open(DbOptions, path, ColumnFamilies)`, `GetColumnFamily`, `ColumnFamilies.Add`/`DefaultName`, `ColumnFamilyOptions.SetCompactionStyle`/`SetMergeOperator`, `MergeOperators.Create` with `PartialMergeFunc`/`FullMergeFunc`/`OperandsEnumerator`, `WriteOptions.SetSync`, the `ColumnFamilyHandle`-taking `Get`/`Put`/`Merge`/`NewIterator` and `WriteBatch.Put`/`Merge`/`Delete`, `RocksDb.Write(WriteBatch, WriteOptions)`, `CreateSnapshot`, `ReadOptions.SetSnapshot`, `GetUpdatesSince`, `TransactionLogIterator.GetBatch`, `WriteBatch.ToBytes`, `Checkpoint.Save`, `RocksDbException`), LightningDB (`LightningEnvironment` + `EnvironmentConfiguration.MapSize`/`MaxDatabases`, `Open(EnvironmentOpenFlags, UnixAccessMode)`, `BeginTransaction`, `LightningTransaction.OpenDatabase(name, DatabaseConfiguration, closeOnDispose)`/`Get`/`Put`/`Delete(db, key)`/`Delete(db, key, value)` — the dup-value overload — `/Commit`, `LightningCursor.GetBoth`/`GetMultiple`/`NextMultiple`/`AllValuesFor`/`SetRange`, `LightningException.StatusCode`, `MDBResultCode`, `DatabaseOpenFlags`, `EnvironmentOpenFlags`), System.Security.Cryptography (`AesGcm(key, tagSizeInBytes)`/`Encrypt`/`Decrypt`/`NonceByteSizes`/`TagByteSizes`, `AuthenticationTagMismatchException`, `RandomNumberGenerator.Fill`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new checkpoint mode is one `CheckpointMode` row; a new boundary cause is one `EmbeddedFault` case; a new KV write intent is one `KvWrite` case; a new integrity tier is one ladder row; a new keyspace is one `KvSpace` row landing its compaction style, accrual, dup width, order, durability, and seal together, so nothing about it is decided at a call site; a new re-drive route is one `RetryShape` row every fault family reads and one arm on the `Reoffer` dispatch; a new rocksdb status or subcode is one `RocksStatus`/`RocksSubcode` row; zero new surface — the whole-file `BackupDatabase` where the paged session adds progress facts, a whole-payload `byte[]` blob materialization, a second hashing path beside the registered `xxh128` UDF, a bare `Error.New(ex)` flattening the status int, a snapshot regression unguarded by `sqlite3_snapshot_cmp`, a per-engine KV service class, a composite-key prefix standing in for a keyspace the engine partitions natively, a `bool Transient` beside the re-drive shape, a separator-joined operand concatenation, a caller-side operand re-fold, a declared key comparer, or a plaintext spool value is the deleted form.
- Boundary: `SqliteConnection.Handle` (`SQLitePCL.sqlite3`) is the one seam joining the managed ADO surface to raw operations, and the bound `e_sqlite3mc` provider keeps raw calls and ADO statements on the same native connection; every native crossing rides inside `HandleBridge` so the cause stays a closed `EmbeddedFault` case; the WAL sidecar set is the unit of backup, snapshot pins and truncating checkpoints remain adversaries, integrity failures route to `Version/recovery`, and blob IO streams through `SqliteBlob` without whole-payload materialization; the backup destination is a SECOND physical store the paged session fills page-for-page, so it binds the same cipher key as its source and an unkeyed destination under the bound `e_sqlite3mc` floor is the plaintext egress the offline-lane classification ceiling forbids; the KV floor holds that same ceiling by a different mechanism because neither engine ships a cipher — the seal rides the VALUE bytes under the SAME KMS-unwrapped DEK custody the SQLite floor uses, and the `Degrade` it leaves in the clear is exact: every KEY byte (a key is a content digest already, and sealing it destroys the byte-lexicographic order every prefix stop and `SetRange` walk reads), the `ChunkIndex` dup values (an LMDB dup value IS a key in the dup sub-B+tree, so `GetBoth` seeks it, `Unlink` deletes by its exact bytes, and `DuplicatesSort` orders on it), the LMDB page metadata, and the RocksDB SST block boundaries and per-value LENGTHS the frame width leaks; `Get` REFUSES on an accruing row and names `Refs`, because the engine already resolved the operand chain on its own read and handing back that resolved frame pushes the framing onto every caller; the re-drive owner for every embedded fault is the CALLER's in-process effect rail (`docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` row `[03]`), so `Reoffer` routes and executes here and a pipeline wrapped around embedded store work is the deleted form, replaying from the wrong boundary; and a `RocksDbException` message no `RocksStatus` prefix matches is an UNCLASSIFIED verdict carrying the raw message and refusing every re-drive — the shape is a MEASURED column read off the status vocabulary, so an unmatched message took no measurement and a fabricated `Waited` spins against a fault whose recovery nobody knows.

```csharp signature
// --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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

// --- [KV_KEYSPACE]

// Key order as declared row DATA, and the one column `Scan` gates on. LMDB's built-in comparator and RocksDB's
// default comparator are both byte-lexicographic, and they hold force only while `DatabaseConfiguration.CompareWith`
// and `FindDuplicatesWith` stay UNCALLED — the order is proved by OMISSION, so no fence here declares a comparer and
// a row that later did would void every prefix stop with no compile error to catch it. `PrefixSound` converts that
// silence into a structural close: the walk refuses on a row whose order cannot bound a prefix.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvOrder {
    public static readonly KvOrder Bytewise = new("bytewise", prefixSound: true);
    public bool PrefixSound { get; }
    private KvOrder(string key, bool prefixSound) : this(key) => PrefixSound = prefixSound;
}

// Sync posture per space. RocksDB decides durability per WRITE and LMDB per ENVIRONMENT, so a row carries `Writes`
// for the LSM arm and `Relaxed` as its contribution to the one environment floor `KvSpace.SyncFloor` folds, with
// `Rank` ordering that fold so the strictest rostered row wins and a buffered row never relaxes a synced peer.
// `Writes` binds a NATIVE options handle, so each row builds exactly one for the process lifetime rather than
// minting a fresh handle on every write.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvDurability {
    public static readonly KvDurability Buffered = new("buffered", sync: false, EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.NoMetaSync, rank: 0);
    public static readonly KvDurability Synced = new("synced", sync: true, EnvironmentOpenFlags.None, rank: 1);
    public EnvironmentOpenFlags Relaxed { get; }
    public int Rank { get; }
    public WriteOptions Writes { get; }
    private KvDurability(string key, bool sync, EnvironmentOpenFlags relaxed, int rank) : this(key) =>
        (Relaxed, Rank, Writes) = (relaxed, rank, new WriteOptions().SetSync(sync));
}

// At-rest posture per space. `Sealed` rides the value through `KvVault`; `Ordered` cannot, because that value is
// itself a POSITION — an LMDB dup value is a key in the dup sub-B+tree, so `GetBoth` seeks it, `Unlink` deletes by
// its exact bytes, and `DuplicatesSort` orders on it, and a seal voids all three exactly as a sealed KEY voids `Scan`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvSeal {
    public static readonly KvSeal Ordered = new("ordered", seals: false);
    public static readonly KvSeal Sealed = new("sealed", seals: true);
    public bool Seals { get; }
    private KvSeal(string key, bool seals) : this(key) => Seals = seals;
}

// How members sit under one key — the ONE column driving the LMDB open flags, the LSM family options, `Append`
// admission, and the `Refs` walk, so none of those four is decided at a call site.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvLayout {
    private KvLayout() { }
    // One key, one value: `Append` and `Refs` both refuse, because no member set exists to append to or to walk.
    public sealed record Single : KvLayout;
    // Variable-width members accrue under one key — the LSM arm through `Operator` so an append is ONE write the
    // engine resolves at read and compaction, the mmap arm through that key's `DuplicatesSort` set.
    public sealed record Accrued(MergeOperator Operator) : KvLayout;
    // Fixed-width members under one key: the mmap arm earns `DuplicatesFixed` and the page-at-a-time `GetMultiple`
    // walk off `Width`, the LSM arm spreads the same members across composite keys under the row's own prefix.
    public sealed record Fanned(int Width) : KvLayout;

    public DatabaseOpenFlags Flags => Switch(
        single:  static _ => DatabaseOpenFlags.Create,
        accrued: static _ => DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort,
        fanned:  static _ => DatabaseOpenFlags.Create | DatabaseOpenFlags.DuplicatesSort | DatabaseOpenFlags.DuplicatesFixed);
    public Option<MergeOperator> Operator => this is Accrued accrued ? Some(accrued.Operator) : None;
    public Option<int> Width => this is Fanned fanned ? Some(fanned.Width) : None;
}

// --- [SPOOL_ACCRUAL]

// Operand accrual under ONE key ([05] rocksdb `MergeOperators`): a disconnected peer's repeated op append is one
// `Merge` write the engine resolves at read and at compaction, never a read-modify-write round trip. Every stored
// value on an `Accrued` row is ONE frame — a member count, then a length-prefixed body per member — so a partial
// merge (operands alone) and a full merge (an existing frame plus operands) share one concatenation rule and differ
// only in that slot. Framing is load-bearing, not style: members are variable-width and a separator-joined
// concatenation shifts two member splits onto one boundary (`docs/laws/patterns.md` `[PREIMAGE_FRAMING]`), so that
// form is the deleted one here. Widths write big-endian, matching the byte-lexicographic order the keyspace declares.
public static class SpoolAccrual {
    const int Width = sizeof(int);
    public static readonly MergeOperator Operator = MergeOperators.Create("rasm-spool-accrual", Partial, Full);

    // One member entering the engine, so the operator's only rule is concatenation and no arm special-cases arity.
    public static ReadOnlyMemory<byte> Frame(ReadOnlyMemory<byte> member) {
        byte[] frame = new byte[(Width * 2) + member.Length];
        BinaryPrimitives.WriteInt32BigEndian(frame, 1);
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(Width), member.Length);
        member.Span.CopyTo(frame.AsSpan(Width * 2));
        return frame;
    }

    // Engine-folded chain read back as its members — a caller never re-folds operands, which is the whole reason
    // this operator exists. A frame the walk cannot parse to its declared count is TORN, never truncated silently
    // to what parsed, because a short member set reads as a drained spool.
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

    // Exemption: the native merge contract fixes the `byte[]` + `out bool` shape and hands operands through a
    // `ref struct` enumerator, so the concatenation sizes one buffer and fills it rather than folding a Seq.
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

// ONE keyspace roster, two engine realizations: a row IS a RocksDB column family and an LMDB named database, so the
// spool, its watermarks, the chunk index, and the spool metadata each hold their own compaction posture, durability
// posture, dup layout, and iterator key space instead of sharing one under a composite-key prefix. A prefix narrows
// KEYS inside a row and can carry none of those four, which is precisely what these roles differ on — so `Scan`'s
// prefix bound stays a WITHIN-row selector and never stands in for a keyspace the engine partitions natively.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KvSpace {
    // Pending `OpLogEntry`/`CrdtOp` rows a disconnected peer buffers (`Version/ledger#SYNC_TRANSPORTS`): accrues, so
    // an append is one write; SYNCED because an acknowledged buffer a power loss erases is the one failure the spool
    // exists to prevent; write-amplification-dominated, hence universal compaction; SEALED because op payloads are
    // model content, and the drain retires whole keys so no by-value dup delete ever needs its stored bytes back.
    public static readonly KvSpace Spool = new("spool", new KvLayout.Accrued(SpoolAccrual.Operator), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Universal);
    // Both `SyncSession` watermarks — the pull resume point and the push-ack frontier. SYNCED because a lost
    // watermark re-drains or double-drains a peer's feed; a handful of point-read keys, hence level compaction.
    public static readonly KvSpace Cursor = new("cursor", new KvLayout.Single(), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Level);
    // Chunk key -> owning `ContentAddress` dup set. BUFFERED because the set rebuilds whole from the artifact
    // store's own `Element/codec#CONTENT_CHUNKING` manifests, so a lost fsync costs a rebuild and never a fact.
    // ORDERED rather than sealed (`KvSeal`), and its width is the `ContentAddress` `[ValueObject<UInt128>]` key width
    // — a derived member size, never a literal — which is what earns `DuplicatesFixed` and the paged `GetMultiple`.
    public static readonly KvSpace ChunkIndex = new("chunk-index", new KvLayout.Fanned(Unsafe.SizeOf<UInt128>()), KvOrder.Bytewise, KvDurability.Buffered, KvSeal.Ordered, Compaction.Level);
    // Engine epoch and peer identity. SYNCED because a lost epoch re-opens the store under a mismatched identity and
    // a lost peer identity re-introduces a known peer as new, orphaning the whole `Cursor` space behind it.
    public static readonly KvSpace Meta = new("meta", new KvLayout.Single(), KvOrder.Bytewise, KvDurability.Synced, KvSeal.Sealed, Compaction.Level);

    public KvLayout Layout { get; }
    public KvOrder Order { get; }
    public KvDurability Durability { get; }
    public KvSeal Seal { get; }
    public ColumnFamilyOptions Family { get; }
    public DatabaseConfiguration Database { get; }
    // `Family` and `Database` each bind once per row for the process: both wrap native handles, so building them per
    // open would leak one handle per mount. `Database` calls NEITHER `CompareWith` NOR `FindDuplicatesWith` — that
    // omission IS the key order every prefix stop reads (`KvOrder`), and supplying either is the deleted form.
    private KvSpace(string key, KvLayout layout, KvOrder order, KvDurability durability, KvSeal seal, Compaction compaction) : this(key) {
        (Layout, Order, Durability, Seal) = (layout, order, durability, seal);
        Family = layout.Operator.Match(
            Some: merge => new ColumnFamilyOptions().SetCompactionStyle(compaction).SetMergeOperator(merge),
            None: () => new ColumnFamilyOptions().SetCompactionStyle(compaction));
        Database = new DatabaseConfiguration { Flags = layout.Flags };
    }

    // LMDB takes its sync posture per ENVIRONMENT while RocksDB takes it per write, so this environment opens
    // under its STRICTEST rostered posture and a buffered row never relaxes a synced peer's durability. Each row's
    // own column carries its LSM realization and its contribution to that floor, never a per-database LMDB posture.
    public static EnvironmentOpenFlags SyncFloor =>
        toSeq(Items).Fold(KvDurability.Buffered, static (strictest, row) => row.Durability.Rank > strictest.Rank ? row.Durability : strictest).Relaxed;
}

// Open request — the engine choice and its provisioning caps as ONE closed input, so the floor opens through one
// verb. `MapSize` is LMDB's file ceiling and a provisioning act (a write past it returns `MapFull`), so it arrives
// as mount DATA and never as a write-time regrow.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvMount {
    private KvMount() { }
    public sealed record Lsm(string Path) : KvMount;
    public sealed record Mmap(string Path, long MapSize) : KvMount;
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

// `Dek` carries the destination's cipher key, `None` only where the source itself opened unkeyed. The backup
// destination is a SECOND physical store the paged session fills page-for-page, so under the bound
// `e_sqlite3mc` floor an unkeyed destination writes the source's pages back as PLAINTEXT — the offline-lane
// classification ceiling the cipher floor exists to hold, surrendered by the one verb that copies the whole
// file. A backup taking no key cannot honor a stance its source declares.
public sealed record BackupPolicy(
    int PageStep,
    Option<ReadOnlyMemory<byte>> Dek,
    Func<SqliteConnection, Fin<ContentAddress>> Identity);

public readonly record struct BlobBinding(string Table, string Column, string PreallocateSql);

// Value seal at the KV seam ([05] `libs/csharp/.api/api-bcl-cryptography.md` `AesGcm`): neither KV engine ships the
// cipher `e_sqlite3mc` gives the SQLite floor beside them, and the offline lane's classification ceiling binds every
// offline-resident plane alike, so the seal rides the VALUE bytes under the SAME KMS-unwrapped DEK that floor binds
// (`Element/identity#KMS_CUSTODY` `EnvelopeKeyring.Unwrap`; the caller zeroizes through
// `CryptographicOperations.ZeroMemory` once this binds, so no plaintext key outlives the crossing). One instance per
// DEK covers every message that key covers, and nonce and tag widths read off the algorithm's own declared bands
// rather than a call-site literal. Nonce is RANDOM per sealed value and rides the frame, and 96
// random bits hold the collision bound far past an offline spool's message count. AAD length-frames its space key
// beside the KV key (`docs/laws/patterns.md` `[PREIMAGE_FRAMING]`), so a value lifted to another key or another
// space refuses at the tag instead of opening.
public sealed class KvVault : IDisposable {
    static readonly int NonceWidth = AesGcm.NonceByteSizes.MaxSize;
    static readonly int TagWidth = AesGcm.TagByteSizes.MaxSize;
    readonly AesGcm cipher;

    public KvVault(ReadOnlySpan<byte> dek) => cipher = new AesGcm(dek, TagWidth);
    public void Dispose() => cipher.Dispose();

    // Row data decides, never the call site: an `Ordered` space passes its bytes through untouched because a dup
    // value is a B+tree key, and a `Sealed` space frames `nonce | tag | ciphertext` — every part but the last
    // fixed-width, so that layout parses with no length prefix and the frame width leaks only the value LENGTH.
    public ReadOnlyMemory<byte> Wrap(KvSpace space, ReadOnlySpan<byte> key, ReadOnlyMemory<byte> value) {
        if (!space.Seal.Seals) { return value; }
        byte[] frame = new byte[NonceWidth + TagWidth + value.Length];
        RandomNumberGenerator.Fill(frame.AsSpan(0, NonceWidth));
        cipher.Encrypt(frame.AsSpan(0, NonceWidth), value.Span, frame.AsSpan(NonceWidth + TagWidth), frame.AsSpan(NonceWidth, TagWidth), Aad(space, key));
        return frame;
    }

    // Forged or key-swapped frames raise at the tag, which is the one throw this seam converts — that fault is
    // TERMINAL because no re-drive re-authenticates bytes that never authenticated.
    public Fin<ReadOnlyMemory<byte>> Unwrap(KvSpace space, ReadOnlySpan<byte> key, ReadOnlyMemory<byte> frame) {
        if (!space.Seal.Seals) { return Fin.Succ(frame); }
        if (frame.Length < NonceWidth + TagWidth) { return Fin.Fail<ReadOnlyMemory<byte>>(new EmbeddedFault.Kv("seal", "<frame-short>", space.Key, RetryShape.Terminal)); }
        byte[] value = new byte[frame.Length - NonceWidth - TagWidth];
        try {
            cipher.Decrypt(frame.Span[..NonceWidth], frame.Span[(NonceWidth + TagWidth)..], frame.Span.Slice(NonceWidth, TagWidth), value, Aad(space, key));
            return Fin.Succ((ReadOnlyMemory<byte>)value);
        }
        catch (AuthenticationTagMismatchException) { return Fin.Fail<ReadOnlyMemory<byte>>(new EmbeddedFault.Kv("seal", "<tag-mismatch>", space.Key, RetryShape.Terminal)); }
    }

    // Length-framed pair, never a separator join: two distinct bindings would otherwise authenticate the same
    // bytes and the AAD would stop binding anything.
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

// --- [ERRORS] ---------------------------------------------------------------------------

// Re-drive vocabulary every embedded fault publishes — WHICH retry recovers a fault, not whether one might.
// `Restarted` covers `SQLITE_BUSY_SNAPSHOT`, whose blocking snapshot is the caller's OWN read transaction and must
// end and re-open first; `Rescoped` covers a `read_tier` partial, which never completes without re-reading
// unrestricted. A retry executor reads this member.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RetryShape {
    public static readonly RetryShape Terminal = new("terminal");
    public static readonly RetryShape Waited = new("waited");
    public static readonly RetryShape Restarted = new("restarted");
    public static readonly RetryShape Rescoped = new("rescoped");
}

// `RocksDbException` carries the errptr string and NOTHING else — no code, no enum, no property past `Exception` —
// so its `Message` IS RocksDB `Status::ToString()` and the primary-code PREFIX is the only discriminator the managed
// surface exposes. Each key is that prefix VERBATIM, trailing colon and space included, because the fold is a prefix
// match on the raw message. Every literal here matched the INSTALLED native library exactly; a RocksDB bump
// re-proves this roster against that library and never against memory, since a prefix the build stopped emitting
// silently demotes its whole family to unclassified.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RocksStatus {
    public static readonly RocksStatus NotFound = new("NotFound: ", RetryShape.Terminal);
    public static readonly RocksStatus Corruption = new("Corruption: ", RetryShape.Terminal);
    public static readonly RocksStatus NotImplemented = new("Not implemented: ", RetryShape.Terminal);
    public static readonly RocksStatus InvalidArgument = new("Invalid argument: ", RetryShape.Terminal);
    public static readonly RocksStatus IoError = new("IO error: ", RetryShape.Terminal);
    // Two `read_tier`-restricted verdicts: neither frees by waiting, and both complete under an UNRESTRICTED
    // re-read, so they publish that route while still reading non-transient to a wait-retry executor.
    public static readonly RocksStatus MergeInProgress = new("Merge in progress: ", RetryShape.Rescoped);
    public static readonly RocksStatus Incomplete = new("Result incomplete: ", RetryShape.Rescoped);
    public static readonly RocksStatus ShutdownInProgress = new("Shutdown in progress: ", RetryShape.Terminal);
    public static readonly RocksStatus TimedOut = new("Operation timed out: ", RetryShape.Waited);
    public static readonly RocksStatus Aborted = new("Operation aborted: ", RetryShape.Terminal);
    public static readonly RocksStatus Busy = new("Resource busy: ", RetryShape.Waited);
    public static readonly RocksStatus Expired = new("Operation expired: ", RetryShape.Terminal);
    public static readonly RocksStatus TryAgain = new("Operation failed. Try again.: ", RetryShape.Waited);
    public static readonly RocksStatus FamilyDropped = new("Column family dropped: ", RetryShape.Terminal);

    public RetryShape Retry { get; }
    private RocksStatus(string key, RetryShape retry) : this(key) => Retry = retry;

    // Longest prefix wins, so a roster growth nesting one prefix inside another cannot silently re-point a message
    // an existing row already answered. The SUBCODE wins over the status where both match, because it is the more
    // specific measurement — a `max_num_locks` ceiling under `Operation timed out: ` is an operator act, not a wait.
    public static (Option<RocksStatus> Status, Option<RocksSubcode> Subcode) Parse(string message) => (
        toSeq(Items).Fold(Option<RocksStatus>.None, (longest, next) =>
            message.StartsWith(next.Key, StringComparison.Ordinal) && next.Key.Length > longest.Map(static held => held.Key.Length).IfNone(0)
                ? Some(next)
                : longest),
        toSeq(RocksSubcode.Items).Find(row => message.Contains(row.Key, StringComparison.Ordinal)));
}

// Subcode literals `Status::ToString()` appends after the primary prefix, each proved present in the installed
// library. Two literals RocksDB's own subcode table names — `Overwritten` and `Merge operands` — matched NOTHING in
// that build as standalone strings, so the roster records what this engine can emit rather than what a header lists.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RocksSubcode {
    public static readonly RocksSubcode MutexTimeout = new("Timeout Acquiring Mutex", RetryShape.Waited);
    public static readonly RocksSubcode LockTimeout = new("Timeout waiting to lock key", RetryShape.Waited);
    public static readonly RocksSubcode Deadlock = new("Deadlock", RetryShape.Waited);
    public static readonly RocksSubcode CompactionPaused = new("Manual compaction paused", RetryShape.Waited);
    public static readonly RocksSubcode LockLimit = new("Failed to acquire lock due to max_num_locks limit", RetryShape.Terminal);
    public static readonly RocksSubcode NoSpace = new("No space left on device", RetryShape.Terminal);
    public static readonly RocksSubcode SpaceLimit = new("Space limit reached", RetryShape.Terminal);
    public static readonly RocksSubcode MemoryLimit = new("Memory limit reached", RetryShape.Terminal);
    public static readonly RocksSubcode StaleFile = new("Stale file handle", RetryShape.Terminal);
    public static readonly RocksSubcode NoSuchFile = new("No such file or directory", RetryShape.Terminal);
    public static readonly RocksSubcode TxnNotPrepared = new("Txn not prepared", RetryShape.Terminal);
    public static readonly RocksSubcode IoFenced = new("IO fenced off", RetryShape.Terminal);
    public static readonly RocksSubcode MergeFailed = new("Merge operator failed", RetryShape.Terminal);

    public RetryShape Retry { get; }
    private RocksSubcode(string key, RetryShape retry) : this(key) => Retry = retry;
}

// closed embedded-boundary fault band (771x): a [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no
// `Category` to override) is the deleted form. No `[GenerateUnionOps]` — the kernel union-ops generator is strictly
// opt-in, so the band carries no per-case `SelfOp` and the `[Union]`-generated `Switch`/`Map` is untouched; band membership
// derives `Code => FaultBand.Embedded + n` through the registry pointer (a bare 771x literal beside the registry row is
// decoupled form the sibling bands reject) and `Category` is the telemetry
// label, so the case lifts BARE onto `Fin<T>` with no `.ToError()` hop. The retry gate is a `RetryShape` table read,
// never a per-case bit — `Busy` DERIVES its shape from the full extended status it already keeps and only `Kv`
// carries one as a column, because no engine status re-derives an LMDB code or a parsed RocksDB message.
[Union]
public abstract partial record EmbeddedFault : Expected, IValidationError<EmbeddedFault> {
    private EmbeddedFault() : base() { }
    public sealed record Busy(int Status) : EmbeddedFault;
    public sealed record Corrupt(int Status, string Detail) : EmbeddedFault;
    public sealed record Io(int Status, string Detail) : EmbeddedFault;
    public sealed record Refused(string Detail) : EmbeddedFault;
    public sealed record Kv(string Engine, string Status, string Detail, RetryShape Shape) : EmbeddedFault;

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

    // Retry SHAPE by table: `SQLITE_BUSY_SNAPSHOT` names the caller's OWN read transaction as the blocker, so it
    // restarts where `SQLITE_BUSY`, `SQLITE_BUSY_RECOVERY`, `SQLITE_BUSY_TIMEOUT`, and `SQLITE_LOCKED` all wait —
    // which is why one snapshot subcode earns an arm and `SQLitePCLRaw` publishing no `BUSY_TIMEOUT` constant
    // costs nothing.
    public RetryShape Retry => Switch(
        busy:    static c => c.Status == raw.SQLITE_BUSY_SNAPSHOT ? RetryShape.Restarted : RetryShape.Waited,
        corrupt: static _ => RetryShape.Terminal,
        io:      static _ => RetryShape.Terminal,
        refused: static _ => RetryShape.Terminal,
        kv:      static c => c.Shape);

    // Transient means a WAIT re-drives the SAME call and succeeds. `Restarted` and `Rescoped` recover too, neither
    // that way, so both read false here and publish their route on `Retry` instead — a wait-retry executor that
    // read them as transient would spin against a blocker only its own caller can release.
    public bool IsTransient => Retry == RetryShape.Waited;

    // Route-dispatched caller re-offer, TOTAL over `RetryShape` through the generated `Switch`: a new route breaks the
    // build rather than falling into a default arm, which is precisely what an executor keying on `IsTransient` does
    // today, silently dropping two of four routes. Arms are the CALLER's re-entry arrows, and they differ by
    // construction: `Restarted` means its snapshot is stale, so re-offer re-enters at the READ and a same-effect retry
    // spins against a snapshot that can never advance; `Rescoped` means its request was too wide, so re-offer narrows
    // before re-entering. Backoff rides `same` as its caller's own `Schedule`, since a wait span is caller policy and
    // not a column of this vocabulary. Arrows thread as STATE, so no arm closes over anything. Embedded work is
    // `docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` row `[03]` — one typed fault on an in-process
    // effect rail — so this IS the whole executor and no pipeline appears around an embedded store op.
    public IO<T> Reoffer<T>(Func<IO<T>> same, Func<IO<T>> reread, Func<IO<T>> narrowed) => Retry.Switch(
        state: (Fault: this, Same: same, Reread: reread, Narrowed: narrowed),
        terminal:  static (re, _) => IO.fail<T>(re.Fault),
        waited:    static (re, _) => re.Same(),
        restarted: static (re, _) => re.Reread(),
        rescoped:  static (re, _) => re.Narrowed());

    // Status int discriminates structurally: BUSY/LOCKED waits, CORRUPT/NOTADB is terminal and routes to restore,
    // and every other code is deterministic IO. Primary is the LOW BYTE, because the ritual ARMS extended result
    // codes (`sqlite3_extended_result_codes`) and an extended status is `primary | (sub << 8)` — so a raw equality
    // test against `raw.SQLITE_BUSY` stops matching once that arming takes effect. Each case KEEPS the full
    // extended status, which is the whole reason for arming it: the retry gate reads the subcode, the receipt keeps
    // it, and the ADO leg passes `SqliteExtendedErrorCode`, NOT `SqliteErrorCode`, because that managed primary
    // discards the very byte `Retry` reads — reading it there let a `BUSY_SNAPSHOT` take the wait route the raw
    // legs already refused, so one exception surfaced two verdicts depending on which leg caught it.
    public static EmbeddedFault Lift(Exception ex) => ex is SqliteException sql ? FromStatus(sql.SqliteExtendedErrorCode, sql.Message) : new Refused(ex.Message);
    public static EmbeddedFault FromStatus(int status, string detail) => (status & 0xFF) switch {
        raw.SQLITE_BUSY or raw.SQLITE_LOCKED => new Busy(status),
        raw.SQLITE_CORRUPT or raw.SQLITE_NOTADB => new Corrupt(status, detail),
        raw.SQLITE_IOERR or raw.SQLITE_FULL or raw.SQLITE_READONLY => new Io(status, detail),
        _ => new Refused(detail),
    };

    // RocksDB's managed exception publishes no code, so its message folds ONCE through the status vocabulary and the
    // verdict is a MEASURED column. A message no prefix matches spells UNCLASSIFIED and refuses every re-drive: with
    // no status matched there is no measurement, and a fabricated `Waited` would spin against a fault whose recovery
    // nobody knows — the raw message rides `Detail` so the roster gap surfaces on the fact stream instead of hiding
    // inside a default.
    public static EmbeddedFault OfRocks(string message) {
        (Option<RocksStatus> Status, Option<RocksSubcode> Subcode) read = RocksStatus.Parse(message);
        return read.Status.Match(
            Some: status => (EmbeddedFault)new Kv("rocksdb", status.Key, message, read.Subcode.Map(static row => row.Retry).IfNone(status.Retry)),
            None: () => new Kv("rocksdb", "<unclassified>", message, RetryShape.Terminal));
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class EngineOps {
    // native crossing mints a CLOSED `EmbeddedFault`, never throws into the interior
    // (`docs/stacks/csharp/boundaries#CAPSULE_OWNER`): an `OK` checkpoint receipts the frame counts, a
    // `SQLITE_BUSY` receipts a retry the schedule re-drives (an overlapping reader blocked the truncate —
    // steady-state, not a fault), and every other status lifts through `EmbeddedFault.FromStatus` (a
    // `SQLITE_CORRUPT` routes to recovery) — a bare `throw` of a provider `SqliteException` escaping
    // unconverted is the deleted form the sibling ops already reject.
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
            // Key binding is the FIRST crossing on the destination, exactly as the open ritual orders it on the
            // source: `sqlite3_backup_init` begins writing data pages immediately, so a key applied after it
            // encrypts nothing already written.
            Fin<Unit> keyed = policy.Dek.Match(
                Some: key => raw.sqlite3_key(Handle(destination), key.Span) is int status && status == raw.SQLITE_OK
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(EmbeddedFault.FromStatus(status, "<backup-key-refused>")),
                None: () => Fin.Succ(unit));
            if (keyed.IsFail) { return Fin.Fail<EmbeddedFact>(keyed.Error); }
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

// operational embedded-KV floor — axis [07]'s rocksdb-lsm and lmdb rows made real: `Lsm` is the write-optimized
// local store a disconnected peer buffers pending `OpLogEntry`/`CrdtOp` rows in (`Version/ledger#SYNC_TRANSPORTS`
// `SyncSession` binds it as its durable row source when no server is reachable), `Mmap` the read-optimized local
// store disconnected-peer reconstruction reads without asserting remote provider residence. Both carry the SAME
// `KvSpace` roster — a row is a column family here and a named database there — so a keyspace question has one
// answer per row instead of one per engine. ONE polymorphic surface spans both engines and a per-engine service
// class is the deleted form; faults lift to the closed `EmbeddedFault` band exactly like the SQLite capsule.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvEngine {
    private KvEngine() { }
    public sealed record Lsm(RocksDb Store, FrozenDictionary<KvSpace, ColumnFamilyHandle> Spaces, KvVault Keys) : KvEngine;
    public sealed record Mmap(LightningEnvironment Store, FrozenDictionary<KvSpace, LightningDatabase> Spaces, KvVault Keys) : KvEngine;
    public KvVault Vault => Switch(lsm: static l => l.Keys, mmap: static m => m.Keys);
}

// `KvWrite` is the batch row's write intent: an upsert, one member accruing under a key, the removal of ONE owner
// from a dup set, or the removal of the key whole. Removal splits because a dupsorted row answers two different
// retirements and a boolean or an `Option` can spell only one of them — the case IS the scope. `Append` splits from
// `Put` for the mirror reason: an upsert REPLACES where an accrual ADDS, and one engine spells the difference as a
// merge operand while the other spells it as a dup put.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KvWrite {
    private KvWrite() { }
    public sealed record Put(ReadOnlyMemory<byte> Value) : KvWrite;
    public sealed record Append(ReadOnlyMemory<byte> Operand) : KvWrite;
    public sealed record Unlink(ReadOnlyMemory<byte> Owner) : KvWrite;
    public sealed record Drop : KvWrite;
}

public static class KvFloor {
    // ONE open for both engines: the mount case picks the engine and carries its provisioning caps, the WHOLE
    // `KvSpace` roster opens in one act, and the DEK binds the vault before any verb can write. LMDB's
    // `MaxDatabases` binds through the configuration record because the property THROWS once the environment is
    // opened — the roster size is a provisioning fact, never a widening a later space could ask for — and the
    // environment takes the roster's strictest sync posture. Each named
    // database opens with `closeOnDispose: false` inside the ONE opening transaction and outlives its commit: a
    // handle closed with that transaction leaves every later verb dialling a dead keyspace.
    public static Fin<KvEngine> Open(KvMount mount, ReadOnlyMemory<byte> dek) => mount.Switch(
        state: dek,
        lsm: static (key, m) => Guarded("rocksdb", () => {
            ColumnFamilies families = new();
            toSeq(KvSpace.Items).Iter(row => families.Add(row.Key, row.Family));
            RocksDb store = RocksDb.Open(new DbOptions().SetCreateIfMissing(true).SetCreateMissingColumnFamilies(true), m.Path, families);
            return (KvEngine)new KvEngine.Lsm(store, toSeq(KvSpace.Items).ToFrozenDictionary(static row => row, row => store.GetColumnFamily(row.Key)), new KvVault(key.Span));
        }),
        mmap: static (key, m) => Guarded("lmdb", () => {
            LightningEnvironment store = new(m.Path, new EnvironmentConfiguration { MapSize = m.MapSize, MaxDatabases = KvSpace.Items.Count });
            store.Open(KvSpace.SyncFloor);
            using LightningTransaction opening = store.BeginTransaction();
            FrozenDictionary<KvSpace, LightningDatabase> spaces = toSeq(KvSpace.Items).ToFrozenDictionary(static row => row, row => opening.OpenDatabase(row.Key, row.Database, closeOnDispose: false));
            return Mdb(opening.Commit()).Map(_ => (KvEngine)new KvEngine.Mmap(store, spaces, new KvVault(key.Span)));
        }).Bind(static opened => opened));

    public static Fin<Unit> Put(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value) => engine.Switch(
        state: (Space: space, Key: key, Value: value),
        lsm: static (s, l) => Guarded("rocksdb", () => {
            l.Store.Put(s.Key.Span, l.Keys.Wrap(s.Space, s.Key.Span, s.Value).Span, Cf(l, s.Space), s.Space.Durability.Writes);
            return unit;
        }),
        mmap: static (s, m) => Guarded("lmdb", () => {
            using LightningTransaction transaction = m.Store.BeginTransaction();
            MDBResultCode write = transaction.Put(Db(m, s.Space), s.Key.Span, m.Keys.Wrap(s.Space, s.Key.Span, s.Value).Span);
            return write == MDBResultCode.Success ? transaction.Commit() : write;
        }).Bind(Mdb));

    // Point reads on an accruing row hand back the frame the engine resolved rather than its members, so this verb
    // refuses and names `Refs` — the operand chain resolves ONCE inside the engine and no caller re-folds it.
    public static Fin<Option<ReadOnlyMemory<byte>>> Get(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key) =>
        space.Layout is not KvLayout.Single
            ? Fin.Fail<Option<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("kv", "<accrued-point-read>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: (Space: space, Key: key),
                lsm: static (s, l) => Guarded("rocksdb", () => Optional(l.Store.Get(s.Key.Span, Cf(l, s.Space))))
                    .Bind(held => Opened(l.Keys, s.Space, s.Key, held)),
                mmap: static (s, m) => Guarded("lmdb", () => {
                    using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
                    (MDBResultCode code, _, MDBValue value) = transaction.Get(Db(m, s.Space), s.Key.Span);
                    return code switch {
                        MDBResultCode.Success => m.Keys.Unwrap(s.Space, s.Key.Span, value.CopyToNewArray()).Map(static opened => Some(opened)),
                        MDBResultCode.NotFound => Fin.Succ<Option<ReadOnlyMemory<byte>>>(None),
                        // Verdict fold owns every other code, so this read carries the SAME taxonomy a write does
                        // rather than a second flattened one; a non-success code never reaches the projection.
                        _ => Mdb(code).Map(static _ => Option<ReadOnlyMemory<byte>>.None),
                    };
                }).Bind(static result => result));

    // Atomic multi-write both engines own natively: one RocksDB `WriteBatch`, one LMDB write transaction — one verb
    // serving the spool drain and the membership refresh. Removal is TWO cases because a dupsorted row is
    // `DuplicatesSort`: `Unlink` drops ONE owner from a content address's dup set, `Drop` drops the address and
    // every owner under it. A single `None`-means-remove shape can only spell the second, so retiring one artifact's
    // reference erased every OTHER artifact's claim on the same chunk — the reverse-reference set `Refs` answers and
    // retention reads as its reachability proof, silently emptied. The LSM arm's key is composite on a `Fanned` row,
    // so one owner's row is already one key and both removal cases land on the same delete there.
    // Two admissions the ROW decides, so no arm re-decides them: `Append` needs a layout that ACCRUES (an append
    // against a `Single` row silently overwrites its one value), and `Unlink` needs an UNSEALED row, because the
    // seal's per-value random nonce makes two seals of one owner differ byte for byte and LMDB's by-value dup delete
    // could never match its own stored bytes. Both read the same fact `KvSeal.Ordered` names, at two verbs.
    public static Fin<Unit> Batch(KvEngine engine, KvSpace space, Seq<(ReadOnlyMemory<byte> Key, KvWrite Write)> writes) =>
        writes.Exists(row => row.Write is KvWrite.Append && space.Layout is KvLayout.Single)
            ? Fin.Fail<Unit>(new EmbeddedFault.Kv("kv", "<append-unaccrued>", space.Key, RetryShape.Terminal))
            : writes.Exists(row => row.Write is KvWrite.Unlink && space.Seal.Seals)
                ? Fin.Fail<Unit>(new EmbeddedFault.Kv("kv", "<unlink-sealed>", space.Key, RetryShape.Terminal))
                : engine.Switch(
                    state: (Space: space, Rows: writes),
                    lsm: static (s, l) => Guarded("rocksdb", () => {
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
                    mmap: static (s, m) => Guarded("lmdb", () => {
                        using LightningTransaction transaction = m.Store.BeginTransaction();
                        LightningDatabase db = Db(m, s.Space);
                        Seq<MDBResultCode> statuses = s.Rows.Map(row => row.Write.Switch(
                            state: (Txn: transaction, Db: db, Space: s.Space, Vault: m.Keys, Key: row.Key),
                            put:    static (t, w) => t.Txn.Put(t.Db, t.Key.Span, t.Vault.Wrap(t.Space, t.Key.Span, w.Value).Span),
                            // Dup sets ARE the accumulation on this engine, so an append is a dup put at the same
                            // key and no merge frame exists to build — LSM framing substitutes for this B+tree,
                            // which is why one intent needs no second case to cross both engines.
                            append: static (t, w) => t.Txn.Put(t.Db, t.Key.Span, t.Vault.Wrap(t.Space, t.Key.Span, w.Operand).Span),
                            unlink: static (t, w) => t.Txn.Delete(t.Db, t.Key.Span, w.Owner.Span),
                            drop:   static (t, _) => t.Txn.Delete(t.Db, t.Key.Span)));
                        Option<MDBResultCode> refused = statuses.Find(static status => status != MDBResultCode.Success && status != MDBResultCode.NotFound);
                        return refused.IsSome ? refused.ValueUnsafe() : transaction.Commit();
                    }).Bind(Mdb));

    // Ordered prefix scan — the verb both declared roles require: a spool DRAIN with only point Get demands a second
    // key index outside the store the spool exists to be, and a content index with only point Get cannot enumerate a
    // namespace for a sweep. The LSM arm walks a snapshot-pinned iterator over the row's OWN family so a drain never
    // reads writes it is itself producing; the mmap arm walks SetRange inside one read transaction; both stop at the
    // first key leaving the prefix, which holds only under the row's declared order — hence the gate.
    public static Fin<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>> Scan(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> prefix) =>
        !space.Order.PrefixSound
            ? Fin.Fail<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>>(new EmbeddedFault.Kv("kv", "<prefix-unordered>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: (Space: space, Bound: prefix),
                lsm: static (s, l) => Guarded("rocksdb", () => {
                    using Snapshot pinned = l.Store.CreateSnapshot();
                    using Iterator cursor = l.Store.NewIterator(Cf(l, s.Space), new ReadOptions().SetSnapshot(pinned));
                    Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)> rows = default;
                    for (cursor.Seek(s.Bound.Span); cursor.Valid() && cursor.GetKeySpan().StartsWith(s.Bound.Span); cursor.Next()) {
                        rows = rows.Add(((ReadOnlyMemory<byte>)cursor.GetKeySpan().ToArray(), (ReadOnlyMemory<byte>)cursor.GetValueSpan().ToArray()));
                    }
                    return rows;
                }).Bind(rows => Opened(l.Keys, s.Space, rows)),
                mmap: static (s, m) => Guarded("lmdb", () => {
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

    // Member read under one key — the verb every accruing layout answers, and the ONE place `DuplicatesFixed` earns
    // its flag. The mmap arm positions with `Set` then drains PAGE-at-a-time through `GetMultiple`/`NextMultiple`,
    // slicing the packed `MDBValue` by the row's own fixed width; a row with no fixed width has no packed page and
    // walks `AllValuesFor` instead. The lsm arm reads the chain the engine already resolved and unframes it on an
    // `Accrued` row, and scans the composite-key prefix on a `Fanned` one — so retention asks one question either
    // way. A `Single` row refuses, because no member set exists to walk.
    public static Fin<Seq<ReadOnlyMemory<byte>>> Refs(KvEngine engine, KvSpace space, ReadOnlyMemory<byte> key) =>
        space.Layout is KvLayout.Single
            ? Fin.Fail<Seq<ReadOnlyMemory<byte>>>(new EmbeddedFault.Kv("kv", "<members-unaccrued>", space.Key, RetryShape.Terminal))
            : engine.Switch(
                state: (Space: space, Key: key),
                lsm: static (s, l) => s.Space.Layout.Operator.IsSome
                    ? Guarded("rocksdb", () => Optional(l.Store.Get(s.Key.Span, Cf(l, s.Space))))
                        .Bind(held => held.Match(
                            Some: frame => SpoolAccrual.Members(frame).Bind(members => Opened(l.Keys, s.Space, s.Key, members)),
                            None: () => Fin.Succ(Seq<ReadOnlyMemory<byte>>())))
                    : Scan(new KvEngine.Lsm(l.Store, l.Spaces, l.Keys), s.Space, s.Key).Map(static rows => rows.Map(static row => row.Value)),
                mmap: static (s, m) => Guarded("lmdb", () => {
                    using LightningTransaction transaction = m.Store.BeginTransaction(TransactionBeginFlags.ReadOnly);
                    using LightningCursor cursor = transaction.CreateCursor(Db(m, s.Space));
                    return s.Space.Layout.Width.Match(
                        Some: width => Paged(cursor, s.Key, width),
                        None: () => toSeq(cursor.AllValuesFor(s.Key.ToArray())).Map(static value => (ReadOnlyMemory<byte>)value.CopyToNewArray()));
                }).Bind(members => Opened(m.Keys, s.Space, s.Key, members)));

    // WAL changefeed resume — the reconnect cursor SyncSession replays from after a partial upload; each entry is
    // one atomic WriteBatch at its sequence number, the same watermark shape the server lane's StalenessWatermark
    // reads. The WAL spans every column family the store opened, so the resume point is STORE-wide and taking a
    // `KvSpace` here would promise a per-space stream the engine never cuts. LMDB holds no WAL at all, so the mmap
    // arm refuses typed rather than fabricating one.
    public static Fin<Seq<(ulong Sequence, ReadOnlyMemory<byte> Batch)>> Since(KvEngine engine, ulong sequence) => engine.Switch(
        state: sequence,
        lsm: static (cursor, l) => Guarded("rocksdb", () => {
            using TransactionLogIterator feed = l.Store.GetUpdatesSince(cursor);
            Seq<(ulong Sequence, ReadOnlyMemory<byte> Batch)> updates = default;
            for (; feed.Valid(); feed.Next()) {
                using WriteBatch batch = feed.GetBatch(out ulong at);
                updates = updates.Add((at, (ReadOnlyMemory<byte>)batch.ToBytes()));
            }
            return updates;
        }),
        mmap: static (_, _) => Fin.Fail<Seq<(ulong, ReadOnlyMemory<byte>)>>(new EmbeddedFault.Kv("lmdb", "<no-wal>", "<changefeed>", RetryShape.Terminal)));

    // Crash-consistent local durability at metadata cost — the lsm arm hard-links a checkpoint clone across every
    // family, the mmap arm runs the online compacting copy across every named database; one cheap durable act per
    // engine over the WHOLE store, the same economics the SQLite floor's paged backup session earns its fence for.
    public static Fin<Unit> Snap(KvEngine engine, string directory) => engine.Switch(
        state: directory,
        lsm: static (target, l) => Guarded("rocksdb", () => { using Checkpoint clone = l.Store.Checkpoint(); clone.Save(target); return unit; }),
        mmap: static (target, m) => Guarded("lmdb", () => m.Store.CopyTo(target, compact: true)).Bind(Mdb));

    // Space lookup is TOTAL: `Open` folded the whole roster into both maps, so an `Option` guard here would be an
    // arm no caller can reach and the indexer is the honest read.
    static ColumnFamilyHandle Cf(KvEngine.Lsm engine, KvSpace space) => engine.Spaces[space];
    static LightningDatabase Db(KvEngine.Mmap engine, KvSpace space) => engine.Spaces[space];

    // ONE unseal entry over three read shapes — the point value, the member set under one key, and the scanned pairs
    // under many — because the AAD binds each value to ITS OWN key: a value opened under a neighbour's key refuses
    // at the tag, which is exactly the property that makes a lifted value unusable.
    static Fin<Option<ReadOnlyMemory<byte>>> Opened(KvVault vault, KvSpace space, ReadOnlyMemory<byte> key, Option<byte[]> held) =>
        held.Match(
            Some: value => vault.Unwrap(space, key.Span, value).Map(static opened => Some(opened)),
            None: () => Fin.Succ<Option<ReadOnlyMemory<byte>>>(None));

    static Fin<Seq<ReadOnlyMemory<byte>>> Opened(KvVault vault, KvSpace space, ReadOnlyMemory<byte> key, Seq<ReadOnlyMemory<byte>> members) =>
        members.Fold(Fin.Succ(Seq<ReadOnlyMemory<byte>>()), (held, member) =>
            held.Bind(opened => vault.Unwrap(space, key.Span, member).Map(value => opened.Add(value))));

    static Fin<Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>> Opened(KvVault vault, KvSpace space, Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)> rows) =>
        rows.Fold(Fin.Succ(Seq<(ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte> Value)>()), (held, row) =>
            held.Bind(opened => vault.Unwrap(space, row.Key.Span, row.Value).Map(value => opened.Add((row.Key, value)))));

    // Page-at-a-time dup drain, the read `DuplicatesFixed` exists for: `Set` positions on the key, `GetMultiple`
    // returns a PACKED page of fixed-width dup values in one native call, `NextMultiple` advances to the next page,
    // and the packed value slices by the row's own width. A per-value `NextDuplicate` walk pays one native crossing
    // per member where this pays one per page, which is the whole justification for the flag.
    // Exemption: the packed page is a native span the walk slices into a seam-local seq frozen once on return.
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

    // Typed LMDB verdict fold — the ceiling family splits by RECOVERY ROUTE rather than one transient bit: a
    // `MapResized` re-drives once this process picks up the size a peer grew, a `MapFull`/`DbsFull`/`TxnFull`/
    // `CursorFull`/`PageFull` succeeds only under WIDER provisioning (a bigger map, a bigger roster, a smaller
    // transaction — a provisioning decision, never a write-time realloc), and a `ReadersFull`/`TLSFull` frees when a
    // live reader finishes or `CheckStaleReaders` reclaims its slot. The corruption family routes terminal to
    // recovery, and every other code carries its own name instead of one flattened string.
    static Fin<Unit> Mdb(MDBResultCode status) => status switch {
        MDBResultCode.Success => Fin.Succ(unit),
        MDBResultCode.MapResized => Fin.Fail<Unit>(new EmbeddedFault.Kv("lmdb", status.ToString(), "<remapped>", RetryShape.Waited)),
        MDBResultCode.ReadersFull or MDBResultCode.TLSFull => Fin.Fail<Unit>(new EmbeddedFault.Kv("lmdb", status.ToString(), "<slots>", RetryShape.Waited)),
        MDBResultCode.MapFull or MDBResultCode.DbsFull or MDBResultCode.TxnFull or MDBResultCode.CursorFull or MDBResultCode.PageFull =>
            Fin.Fail<Unit>(new EmbeddedFault.Kv("lmdb", status.ToString(), "<ceiling>", RetryShape.Rescoped)),
        MDBResultCode.Corrupted or MDBResultCode.Panic or MDBResultCode.PageNotFound or MDBResultCode.VersionMismatch
            or MDBResultCode.Invalid or MDBResultCode.InvalidData =>
            Fin.Fail<Unit>(new EmbeddedFault.Corrupt((int)status, $"<lmdb:{status}>")),
        _ => Fin.Fail<Unit>(new EmbeddedFault.Kv("lmdb", status.ToString(), "<write>", RetryShape.Terminal)),
    };

    // `LightningException` re-enters the SAME verdict fold a returned code takes, because its `StatusCode` is an
    // `MDBResultCode` integer — its ceiling family keeps that route and its corruption family stays terminal when a
    // driver raises the code instead of returning it. Stamping the CLR type name into the status slot forked one
    // engine's taxonomy in two: a `MapFull` raised read terminal while the identical code returned read recoverable.
    // `RocksDbException` cannot re-enter that fold — it publishes no code, no enum, and no property past `Exception`
    // — so its message, which IS `Status::ToString()`, folds through `EmbeddedFault.OfRocks`. The two folds stay two
    // because one reads a returned enum and the other parses a string, and a shared shape would carry a dead column
    // at each engine; what they DO share is the output, one `EmbeddedFault.Kv` under one `RetryShape`, which is the
    // collapse a retry executor actually reads.
    static Fin<T> Guarded<T>(string engine, Func<T> call) {
        try { return Fin.Succ(call()); }
        catch (LightningException native) {
            return Fin.Fail<T>(Mdb((MDBResultCode)native.StatusCode).Match(
                Succ: _ => (Error)new EmbeddedFault.Kv(engine, native.StatusCode.ToString(CultureInfo.InvariantCulture), native.Message, RetryShape.Terminal),
                Fail: banded => banded));
        }
        catch (RocksDbException native) { return Fin.Fail<T>(EmbeddedFault.OfRocks(native.Message)); }
        catch (Exception exception) { return Fin.Fail<T>(new EmbeddedFault.Kv(engine, exception.GetType().Name, exception.Message, RetryShape.Terminal)); }
    }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                | [BINDING]                                                         |
| :-----: | :------------------- | :------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | handle bridge        | `SqliteConnection.Handle` raw seam     | the one join to `sqlite3_*` the managed API omits                 |
|  [02]   | checkpoint receipt   | `sqlite3_wal_checkpoint_v2` out-params | typed frame counts; `SQLITE_BUSY` retries the schedule            |
|  [03]   | consistent read      | `sqlite3_snapshot_*` pin bracket       | `_cmp` floor guard; `_free` only a held handle                    |
|  [04]   | backup               | paged `sqlite3_backup_*` session       | subsumes whole-file `BackupDatabase`; `quick_check` proof         |
|  [05]   | large payload        | `SqliteBlob` over `zeroblob(N)`        | streamed; whole-`byte[]` materialization deleted                  |
|  [06]   | fault discrimination | `EmbeddedFault` over the status int    | `Busy` waits or restarts; `Corrupt` routes to recovery            |
|  [07]   | embedded KV          | `KvFloor` over `KvEngine` (LSM/mmap)   | offline op spool + chunk index; one polymorphic surface           |
|  [08]   | KV drain and sweep   | `Scan`/`Refs` snapshot-pinned walks    | prefix scan + dupsorted reverse refs; point-Get-only form deleted |
|  [09]   | KV resume and clone  | `Since` WAL cursor, `Snap` clone/copy  | reconnect replay from a sequence; hard-link or compacting copy    |
|  [10]   | KV write intent      | `KvWrite` Put/Append/Unlink/Drop       | dup-scoped retirement; a whole-key drop never empties a dup set   |
|  [11]   | KV fault taxonomy    | raised and returned codes share `Mdb`  | `LightningException.StatusCode` folds where a return code folds   |
|  [12]   | keyed backup         | `BackupPolicy.Dek` before `backup_init`| destination binds its source's cipher; no plaintext egress        |
|  [13]   | KV keyspace axis     | `KvSpace` row = CF and named DB        | one roster, two engines; prefix never a keyspace                  |
|  [14]   | KV key order         | `KvOrder.Bytewise`, no comparer        | proof by omission; `Scan` gates on `PrefixSound`                  |
|  [15]   | KV accrual           | `KvLayout` `Single`/`Accrued`/`Fanned` | drives flags, family options, `Append`, `Refs`                    |
|  [16]   | KV merge frame       | `SpoolAccrual` count + length prefixes | one `Merge` per append; separator join deleted                    |
|  [17]   | KV durability        | `KvDurability` per row                 | LSM per write; LMDB env takes the strictest                       |
|  [18]   | KV at rest           | `KvVault` AEAD over values             | keys and dup values clear; `Degrade` names both                   |
|  [19]   | KV retry route       | `RetryShape` on every fault            | `IsTransient` is `Waited` alone                                   |
|  [20]   | rocksdb verdict      | `RocksStatus` prefix + subcode fold    | message is the only carrier; unmatched refuses                    |

## [05]-[STORE_AXIS_MAP]

Store perimeter is PARAMETERIZED — eleven axes, every provider row deployment/policy DATA on one axis surface. Policy values select every provider — profile rows, grant minters, sink rows, index-residency rows — never a central-manifest edit, never a new entry point, never a parallel rail. Each kept scale-out row carries the PROVEN ceiling the in-PG/in-process owner cannot reach; every provider row carries its provisioning/health/recovery posture through the `#SERVER_EXTENSIONS` verification-first fold, and the scylla/redis rows gain DEPLOYMENT-CONDITIONAL AppHost probe rows only where the axis row is composed (the Npgsql-only probe stays the default). Relational SoR spine is SINGULAR and sealed — ONE event store, ONE materializer, ONE identity, ONE changefeed — so a perimeter-axis engine row carrying unreachable capability is a legal axis admission, never a second SoR.

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
- [08]-[EMBEDDED_RELATIONAL]: `Element/identity` + `Store/provisioning`; npgsql-ef · sqlite-ef; one generated mapping, two providers; a hand ADO mapping beside the rail is deleted (ARCH).
- [09]-[WIDE_COLUMN_CONTENT_INDEX]: `Query/cache`; marten-pg (default) · scylla-widecolumn; LWT `AppliedInfo` claim-gate + shard-routed point reads at federation scale.
- [10]-[CACHE_BACKPLANE]: `Query/cache`; none (single-node default) · redis-pubsub; cross-process L1 invalidation the `IDistributedCache` contract cannot express.
- [11]-[SPATIAL_STORE_PLANE]: `Element/identity` · `Store/provisioning` · `Element/codec` · `Ingest/geospatial`; postgis-column (EF-NTS) · ado-codec (`SpatialWire`) · geojson-stj · geopackage · wkb/wkt · h3-cell (pocketken); the provisioned postgis/pgrouting/h3-pg tier gains its wire, column, codec, and file-ingress counterparts, closed end-to-end.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
