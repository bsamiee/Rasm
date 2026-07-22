# [PERSISTENCE_STORE_OBSERVABILITY]

Engine-stat observability, the receipt-slot registry, the hook rail, and the store instrument contributor: one slot grammar names every evidence stream Persistence emits, one registry enforces uniqueness at composition, one harvest fold turns each engine's statistics surface — PostgreSQL cumulative views, DuckDB profiling output, SQLite status counters — into typed receipts, one plan-shape rail turns suspect statements into typed drift verdicts, one hook roster gives the durable lifecycle its veto/observe/replay points, one usage census turns storage truth into chargeback evidence, and one contributor projects the receipt fan into `rasm.persistence.*` instruments. Embedded engines expose no scrape surface, so the embedding process is their observer and the receipt rail is their observability.

Settled composition: `ReceiptSinkPort` and `ProjectionContext` arrive from the AppHost port vocabulary; `InstrumentRow`, `InstrumentSet`, `InstrumentArm`, `LevelCells`, `TelemetryContributorPort`, and the hook vocabulary — `HookPoint<TFact>`, `HookId`, `HookModality`, `HookRegistry`, `IsolatedFault` — arrive settled from the kernel signal capsule; the receipt observe tap arrives from the AppHost hook rail. Provider instrumentation subscribes at the AppHost root as four settled rows: `Npgsql.OpenTelemetry` — `AddNpgsql()` tracing and the `Npgsql` meter by name under the `AddView` posture the `NpgsqlDataSourceBuilder.Name` pool dimension keys; `OpenTelemetry.Instrumentation.EntityFrameworkCore` — `AddEntityFrameworkCoreInstrumentation` beside `AddNpgsql`, the ORM-layer command span nesting over the ADO-layer driver span, complementary never redundant, trace-only beside the `Npgsql` meter roster; `OpenTelemetry.Instrumentation.StackExchangeRedis` — `AddRedisInstrumentation(connection)` binding the cache multiplexer with the handle captured through `ConfigureRedisInstrumentation` so `AddConnection` binds the egress `RedisStream` multiplexer under one subscription, tracer-only with `Filter`/`Enrich` unset on the hot cache path; `OpenTelemetry.Instrumentation.AWS` — `AddAWSInstrumentation` on the tracer AND meter builders once, the shared `AWSSDK.Core` pipeline customizer spanning both the `AWSSDK.S3` object-store and `AWSSDK.KeyManagementService` custody clients, `SuppressDownstreamInstrumentation` set where HTTP instrumentation co-admits. Metric names are dotted `rasm.<domain>.<measure>`, units UCUM, scope id the emitting package id.

## [01]-[INDEX]

- [01]-[SLOT_REGISTRY]: `store.<domain>.<verb>` grammar, the registry fold, and the page-contributed mount.
- [02]-[PG_STAT_HARVEST]: `pg_stat_statements` and `pg_stat_io` typed harvest receipts.
- [03]-[DUCKDB_PROFILE_HARVEST]: Profiling-JSON harvest off the analytical lane.
- [04]-[SQLITE_STATUS_HARVEST]: Statement and connection status counters off the raw bridge.
- [05]-[PLAN_PROFILE]: Three-engine plan-shape capture, digest baselines, and the typed drift verdict.
- [06]-[HOOK_RAIL]: `rasm.persistence.<domain>.<point>` roster over the settled AppHost hook vocabulary.
- [07]-[USAGE_PROJECTION]: (tenant, class, tier) usage census under `store.cost.usage`.
- [08]-[STORE_INSTRUMENTS]: `rasm.persistence.*` instrument roster, contributor port, census egress, and receipt-projection arms.

## [02]-[SLOT_REGISTRY]

- Owner: `StoreSlot` `[ValueObject<string>]` — the slot name under the `store.<domain>.<verb>` grammar, the verb a dotted path when one domain carries verb families; `SlotRegistry` — the composition-time catalog of every slot this package emits.
- Entry: `SlotRegistry.Mount(params ReadOnlySpan<StoreSlot> slots)` — freezes the catalog and throws on a duplicate at composition; `SlotRegistry.Mounted(params ReadOnlySpan<StoreSlot> contributed)` — the composition-root census spreading every page's contributed roster and any sibling-package family the call site supplies; `SlotRegistry.Admit(SlotRegistry registry, StoreSlot slot)` — the pre-send gate every receipt emission crosses, so an unregistered slot is a typed refusal, never a silent new stream.
- Auto: each owning page carries one `Slots` roster on its primary owner and `Mounted` spreads them, so the registry is the one census of the emitted-signal surface and discovery stops being page-by-page archaeology.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new evidence stream is one `StoreSlot` row on its owning page's roster; the grammar admits a new domain or verb with zero registry edits.
- Boundary: the slot is the `kind` argument the sink `Send` carries, so slot vocabulary and wire kind are one spelling; this page mints its own slots — `store.stat.statements`, `store.stat.io`, `store.stat.duckdb`, `store.stat.sqlite.statements`, `store.stat.sqlite.connection`, `store.stat.plan`, `store.cost.usage` — and every other page's slots enter as its contributed rows, so the registry owns uniqueness while each page owns its spellings; a sibling PACKAGE's family — the Fabrication `store.fabrication.<domain>.<verb>` shop-state rows (remnant inventory, fleet performance horizons, magazine slot state, capability history), each pairing a typed read and write receipt on its Fabrication owner — enters through the `Mounted` `contributed` span at composition, so a foreign family is call-site data under the same uniqueness law, never a census edit; a per-occurrence discriminant — a traversal's query case, a sink's lane — rides the receipt payload, never the slot string, so the census stays frozen while payloads vary.

```csharp signature
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<StatFault>]
public readonly partial struct StoreSlot {
    static partial void ValidateFactoryArguments(ref StatFault? validationError, ref string value) =>
        validationError = value.Split('.') is ["store", var domain, .. var verbs]
            && domain.Length > 0 && verbs.Length >= 1 && verbs.All(static verb => verb.Length > 0)
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-')
            ? null
            : new StatFault.MalformedSlot(value);
}

public sealed record SlotRegistry(FrozenSet<string> Slots) {
    public static SlotRegistry Mount(params ReadOnlySpan<StoreSlot> slots) {
        var keys = slots.ToArray().Select(static slot => slot.ToString()).ToArray();
        return keys.Length == keys.Distinct(StringComparer.Ordinal).Count()
            ? new(keys.ToFrozenSet(StringComparer.Ordinal))
            : throw new InvalidOperationException($"slot-collision:{keys.Length}");
    }

    // Composition-root census: every page's roster spreads here, so a new page slot is one roster row and zero
    // registry edits; a sibling PACKAGE's family (the Fabrication `store.fabrication.<domain>.<verb>` shop-state
    // rows) enters through `contributed` at composition — call-site data under the same uniqueness law.
    public static SlotRegistry Mounted(params ReadOnlySpan<StoreSlot> contributed) => Mount([
        PgStatHarvest.StatementsSlot, PgStatHarvest.IoSlot, DuckProfileHarvest.Slot,
        SqliteStatHarvest.StatementsSlot, SqliteStatHarvest.ConnectionSlot, PlanProfile.Slot, StoreUsage.Slot,
        .. GraphStore.Slots, .. TabularSource.Slots, .. ScheduleSource.Slots, .. GeoSource.Slots,
        .. IssueSource.Slots, .. Coordinate.Slots, .. ClusterProvision.Slots, .. ObjectIo.Slots,
        .. ModelResultIndex.Slots, .. ColumnarLane.Slots, .. ReadRouter.Slots, .. GraphSession.Slots,
        .. Federation.Slots, .. Traversals.Slots, .. SearchRoute.Slots, .. OpLog.Slots,
        .. EgressPump.Slots, .. CdcIngress.Slots, .. StructuralMerge.Slots, .. Crdt.Slots, .. RecoveryRoutes.Slots,
        .. TimeTravel.Slots, .. contributed]);

    public static Fin<StoreSlot> Admit(SlotRegistry registry, StoreSlot slot) =>
        registry.Slots.Contains(slot.ToString())
            ? Fin.Succ(slot)
            : Fin.Fail<StoreSlot>(new StatFault.SlotUnregistered(slot.ToString()));
}

[Union]
public abstract partial record StatFault : Expected, IValidationError<StatFault> {
    private StatFault(string detail, int code) : base(detail, code, None) { }
    public static StatFault Create(string message) => new Text(message);
    public sealed record Text : StatFault { public Text(string detail) : base(detail, 8490) { } }
    public sealed record MalformedSlot : StatFault { public MalformedSlot(string slot) : base(slot, 8491) { } }
    public sealed record SlotUnregistered : StatFault { public SlotUnregistered(string slot) : base(slot, 8492) { } }
    public sealed record HarvestRefused : StatFault { public HarvestRefused(string engine, string detail) : base($"{engine}: {detail}", 8493) { } }
}
```

## [03]-[PG_STAT_HARVEST]

- Owner: `PgStatHarvest` — the typed read over the two cumulative statement and I/O views; `StatementStatRow` and `IoStatRow` the receipt rows.
- Entry: `PgStatHarvest.Statements(NpgsqlDataSource source, int top)` — the top-N statement rows by total execution time; `PgStatHarvest.Io(NpgsqlDataSource source)` — the per-backend-type I/O rows.
- Auto: both harvests ride the pooled `NpgsqlDataSource` the production path owns, so a stats read shares pool pressure with live traffic and never opens a side connection; `pg_stat_statements` requires the `compute_query_id` server posture the provisioning page's extension roster carries, so `queryid` joins a statement row to the driver span's query identity.
- Receipt: `StatementStatRow` — queryid, calls, total and mean execution time, rows, shared-block hits and reads, WAL bytes; `IoStatRow` — backend type, object, context, reads, writes, extends, their byte figures, hits, evictions, fsyncs; the `object` column carries `relation`, `temp relation`, AND `wal`, so WAL I/O rides the same rows with zero widening; each batch fans under `store.stat.statements` / `store.stat.io`.
- Packages: Npgsql, LanguageExt.Core, NodaTime.
- Growth: a new harvested column is one field on the owning row and one select column; a new server view is one harvest member on this owner.
- Boundary: this fold is the query-depth complement to the driver meter seam — the `Npgsql` meter carries operation duration and pool level at the AppHost root while these rows carry per-statement and per-backend server truth as receipts; pg_stat views are server-global, so these receipts carry no tenant brand by ruling and the batch envelope carries the frame correlation at the `Send` seam; the three lag gauges stay distinct owners — provisioning's slot lag, recovery's replication lag, and this page's I/O timing never share a row; `track_io_timing` is a deliberate server posture the provisioning verify batch asserts before timing columns read as truth.

```csharp signature
public sealed record StatementStatRow(
    long QueryId, long Calls, double TotalExecMs, double MeanExecMs, long Rows,
    long SharedBlksHit, long SharedBlksRead, long WalBytes);

// `ReadBytes`/`WriteBytes`/`ExtendBytes` are the pg18 `numeric` byte columns cast to `bigint` in the select
// (null before first tracking, coalesced to zero); the `wal` object rows ride the same shape.
public sealed record IoStatRow(
    string BackendType, string Object, string Context,
    long Reads, long ReadBytes, long Writes, long WriteBytes, long Extends, long ExtendBytes,
    long Hits, long Evictions, long Fsyncs);

public static class PgStatHarvest {
    public static readonly StoreSlot StatementsSlot = StoreSlot.Create("store.stat.statements");
    public static readonly StoreSlot IoSlot = StoreSlot.Create("store.stat.io");

    const string StatementsSql = """
        SELECT queryid, calls, total_exec_time, mean_exec_time, rows,
               shared_blks_hit, shared_blks_read, wal_bytes::bigint
        FROM pg_stat_statements
        ORDER BY total_exec_time DESC
        LIMIT $1
        """;

    const string IoSql = """
        SELECT backend_type, object, context,
               reads, COALESCE(read_bytes, 0)::bigint,
               writes, COALESCE(write_bytes, 0)::bigint,
               extends, COALESCE(extend_bytes, 0)::bigint,
               hits, evictions, fsyncs
        FROM pg_stat_io
        WHERE reads > 0 OR writes > 0
        """;

    public static IO<Seq<StatementStatRow>> Statements(NpgsqlDataSource source, int top) =>
        IO.liftAsync(async () => {
            await using var command = source.CreateCommand(StatementsSql);
            command.Parameters.AddWithValue(top);
            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<StatementStatRow>();
            while (await reader.ReadAsync()) {
                rows.Add(new StatementStatRow(
                    reader.GetInt64(0), reader.GetInt64(1), reader.GetDouble(2), reader.GetDouble(3),
                    reader.GetInt64(4), reader.GetInt64(5), reader.GetInt64(6), reader.GetInt64(7)));
            }
            return toSeq(rows).Strict();
        });

    public static IO<Seq<IoStatRow>> Io(NpgsqlDataSource source) =>
        IO.liftAsync(async () => {
            await using var command = source.CreateCommand(IoSql);
            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<IoStatRow>();
            while (await reader.ReadAsync()) {
                rows.Add(new IoStatRow(
                    reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetInt64(3), reader.GetInt64(4), reader.GetInt64(5), reader.GetInt64(6),
                    reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.GetInt64(10),
                    reader.GetInt64(11)));
            }
            return toSeq(rows).Strict();
        });
}
```

## [04]-[DUCKDB_PROFILE_HARVEST]

- Owner: `DuckProfileHarvest` — the profiling-switch bracket and the profile receipt over the analytical lane's connection.
- Entry: `DuckProfileHarvest.Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context)` — runs one statement under `enable_profiling='json'` with `profiling_output` redirected to the caller's run-scoped artifact path, parses the JSON, deletes the scratch artifact, and folds one `DuckProfileReceipt`.
- Auto: `profiling_mode` stays `standard` for routine harvests and the detailed optimizer metrics enter as one pragma value when a plan investigation demands them; the per-operator tree folds to a digest with the top-cost operator rows, and the bracket deletes the decoded JSON scratch file so the receipt is the retained profile truth.
- Receipt: `DuckProfileReceipt` — latency, CPU time, rows returned, result-set bytes, blocked-thread time, operator-tree digest, top operator rows, the frame's instant and correlation; fans under `store.stat.duckdb`.
- Packages: DuckDB.NET.Data.Full, LanguageExt.Core, NodaTime, System.IO.Hashing.
- Growth: one profiling metric key is one receipt field and one parse line; plan-shape capture and drift verdicts are the `#PLAN_PROFILE` rail's, which probes `EXPLAIN (FORMAT json)` without arming this profiling bracket.
- Boundary: the profiling switch is connection state, so the bracket sets, runs, and resets on every exit path — a lane query outside the bracket runs unprofiled at full speed; `outputPath` arrives from the configured artifact owner, resolves to a full path, escapes as a DuckDB string literal, and is deleted after decode on success or failure, so ambient temp storage and orphaned profile files are forbidden; the harvest borrows the `Query/columnar` connection and mints no second DuckDB lane; the analytical lane is process-scoped, so the receipt carries the frame's correlation and instant while tenant stays a `ProjectionContext` fact the sink envelope carries by ruling.

```csharp signature
public sealed record DuckOperatorRow(string Name, double TimingSeconds, long Cardinality);

public sealed record DuckProfileReceipt(
    double LatencySeconds, double CpuSeconds, long RowsReturned, long ResultSetBytes,
    double BlockedThreadSeconds, UInt128 PlanDigest, Seq<DuckOperatorRow> TopOperators,
    Instant At, Guid Correlation);

public static class DuckProfileHarvest {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.duckdb");

    public static IO<DuckProfileReceipt> Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context) =>
        IO.liftAsync(async () => {
            var output = Path.GetFullPath(outputPath);
            var escapedOutput = output.Replace("'", "''", StringComparison.Ordinal);
            var armed = false;
            try {
                await using (var arm = connection.CreateCommand()) {
                    arm.CommandText = $"PRAGMA enable_profiling='json'; PRAGMA profiling_output='{escapedOutput}'; PRAGMA profiling_mode='standard';";
                    armed = true;
                    await arm.ExecuteNonQueryAsync();
                }
                await using var work = connection.CreateCommand();
                work.CommandText = sql;
                await work.ExecuteNonQueryAsync();
                using var profile = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
                return Decode(profile.RootElement, context);
            }
            finally {
                try {
                    if (armed) {
                        await using var disarm = connection.CreateCommand();
                        disarm.CommandText = "PRAGMA disable_profiling;";
                        await disarm.ExecuteNonQueryAsync();
                    }
                }
                finally {
                    File.Delete(output);
                }
            }
        });

    static DuckProfileReceipt Decode(JsonElement root, ProjectionContext frame) {
        var operators = Operators(root).OrderByDescending(static row => row.TimingSeconds).Take(8).ToSeq().Strict();
        return new(
            LatencySeconds: root.TryGetProperty("latency", out var latency) ? latency.GetDouble() : 0d,
            CpuSeconds: root.TryGetProperty("cpu_time", out var cpu) ? cpu.GetDouble() : 0d,
            RowsReturned: root.TryGetProperty("rows_returned", out var rows) ? rows.GetInt64() : 0L,
            ResultSetBytes: root.TryGetProperty("result_set_size", out var size) ? size.GetInt64() : 0L,
            BlockedThreadSeconds: root.TryGetProperty("blocked_thread_time", out var blocked) ? blocked.GetDouble() : 0d,
            PlanDigest: XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(root.GetRawText())),
            TopOperators: operators,
            At: frame.Now(),
            Correlation: frame.Correlation);
    }

    static Seq<DuckOperatorRow> Operators(JsonElement node) {
        var self = node.TryGetProperty("operator_type", out var kind)
            ? Seq(new DuckOperatorRow(
                kind.GetString() ?? string.Empty,
                node.TryGetProperty("operator_timing", out var timing) ? timing.GetDouble() : 0d,
                node.TryGetProperty("operator_cardinality", out var cardinality) ? cardinality.GetInt64() : 0L))
            : Seq<DuckOperatorRow>();
        return node.TryGetProperty("children", out var children)
            ? self + children.EnumerateArray().ToSeq().Bind(Operators)
            : self;
    }
}
```

## [05]-[SQLITE_STATUS_HARVEST]

- Owner: `SqliteStatHarvest` — the per-statement and per-connection counter read over the raw bridge.
- Entry: `SqliteStatHarvest.Statements(SqliteConnection connection)` — walks every prepared statement on the connection through `raw.sqlite3_next_stmt` off `SqliteConnection.Handle` and folds the read-and-reset `raw.sqlite3_stmt_status` counters into one interval receipt; `SqliteStatHarvest.Connection(SqliteConnection connection)` — samples the connection gauges off the same handle.
- Auto: statement counters read with the reset flag so each receipt carries one interval's work, while connection gauges sample without reset so cache hit ratio folds over the interval; a full-scan step count or transient-index count above zero on a hot interval is the plan-regression tell — the `#PLAN_PROFILE` sqlite leg names the offending statement.
- Receipt: `SqliteStatementStat` — VM steps, full-scan steps, sorts, transient-index rows — fans under `store.stat.sqlite.statements`; `SqliteConnectionStat` — cache hits, misses, writes, cache bytes, schema and statement bytes — fans under `store.stat.sqlite.connection`, so each receipt shape owns its slot and no consumer sniffs the payload for its kind.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, LanguageExt.Core.
- Growth: one counter is one field and one raw-call line, bounded by the constants the core interop assembly declares — the reprepare/run statement counters and the cache-spill gauge stay off the receipts because `SQLitePCLRaw.core` declares no `SQLITE_STMTSTATUS_REPREPARE`/`SQLITE_STMTSTATUS_RUN`/`SQLITE_DBSTATUS_CACHE_SPILL` constant, and they re-widen the day the core assembly grows the rows.
- Boundary: the raw calls reach through the same `SqliteConnection.Handle` the provisioning engine operations already bridge, so the harvest opens no second native path and reads the same native connection the ADO surface drives; the `sqlite3_next_stmt` walk borrows each statement handle only inside the fold and holds none past it; the per-table `dbstat` space census is foreclosed — the bundled `e_sqlite3` build ships no `SQLITE_ENABLE_DBSTAT_VTAB` (verified over `PRAGMA compile_options`), so store-level bytes ride the `SCHEMA_USED`/`STMT_USED` gauges and the SQL `PRAGMA page_count`/`page_size` product; the embedded store is process-scoped, so these receipts carry no tenant brand by ruling; provider-bundle facts stay engine-layer and never become Persistence vocabulary.

```csharp signature
public sealed record SqliteStatementStat(int VmSteps, int FullScanSteps, int Sorts, int AutoIndexRows);

public sealed record SqliteConnectionStat(int CacheHits, int CacheMisses, int CacheWrites, int CacheBytes, int SchemaBytes, int StatementBytes);

public static class SqliteStatHarvest {
    public static readonly StoreSlot StatementsSlot = StoreSlot.Create("store.stat.sqlite.statements");
    public static readonly StoreSlot ConnectionSlot = StoreSlot.Create("store.stat.sqlite.connection");

    // Read-and-reset interval fold over every prepared statement on the connection: `sqlite3_next_stmt(db, null)`
    // seeds the walk, each statement's counters read with resetFlg 1 so the next harvest sees only new work.
    public static Fin<SqliteStatementStat> Statements(SqliteConnection connection) =>
        connection.Handle is { } db
            ? Fin.Succ(Walk(db))
            : Fin.Fail<SqliteStatementStat>(new StatFault.HarvestRefused("sqlite", "connection handle absent"));

    public static Fin<SqliteConnectionStat> Connection(SqliteConnection connection) =>
        connection.Handle is { } db
            ? Fin.Succ(new SqliteConnectionStat(
                Gauge(db, raw.SQLITE_DBSTATUS_CACHE_HIT), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_MISS),
                Gauge(db, raw.SQLITE_DBSTATUS_CACHE_WRITE), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_USED),
                Gauge(db, raw.SQLITE_DBSTATUS_SCHEMA_USED), Gauge(db, raw.SQLITE_DBSTATUS_STMT_USED)))
            : Fin.Fail<SqliteConnectionStat>(new StatFault.HarvestRefused("sqlite", "connection handle absent"));

    static SqliteStatementStat Walk(sqlite3 db) {
        var (vm, scan, sort, autoIndex) = (0, 0, 0, 0);
        // Exemption: the raw handle walk is the platform-forced statement seam — an exhausted walk yields an
        // invalid (zero) statement handle, never a throwing sentinel.
        for (sqlite3_stmt? statement = raw.sqlite3_next_stmt(db, null!); statement is { IsInvalid: false }; statement = raw.sqlite3_next_stmt(db, statement)) {
            vm += raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_VM_STEP, 1);
            scan += raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_FULLSCAN_STEP, 1);
            sort += raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_SORT, 1);
            autoIndex += raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_AUTOINDEX, 1);
        }
        return new(vm, scan, sort, autoIndex);
    }

    static int Gauge(sqlite3 db, int op) {
        ignore(raw.sqlite3_db_status(db, op, out var current, out _, 0));
        return current;
    }
}
```

## [06]-[PLAN_PROFILE]

- Owner: `PlanProfile` — the three-engine plan-shape capture; `PlanSubject` the capture request `[Union]` discriminating the engine by the value's shape, never a mode flag; `PlanEngine` the engine axis; `PlanBaselineRow` the statement-identity-keyed baseline row; `PlanVerdict` the closed compare outcome; `PlanReceipt` the probe receipt.
- Cases: `PlanSubject` is `Postgres(NpgsqlDataSource, string, Option<long>) | Duck(DuckDBConnection, string) | Sqlite(SqliteConnection, string)`; `PlanVerdict` is `Baselined | Unchanged | Drifted` — a first sighting persists its shape through the injected `baseline` arrow and reads `Baselined`, a match reads `Unchanged`, a moved digest reads `Drifted` carrying both shapes.
- Entry: `PlanProfile.Capture(PlanSubject subject, held, baseline, frame)` — one entry over the closed subject family; `held`/`baseline` are the relational identity-tier arrows filled at composition, so this owner opens no session and the baseline rows persist beside the identity tier.
- Auto: each leg folds its engine's plan artifact to a SHAPE-ONLY digest — node kinds, join types, relation and index names for PostgreSQL, the physical-operator tree for DuckDB, the `EXPLAIN QUERY PLAN` detail rows for SQLite — so the digest is run-stable: a flipped join order or a lost index moves it, a slow run does not; statement identity is the pg `queryid` when the `compute_query_id` posture supplies one, else the invariant hash of the statement text.
- Receipt: a capture rides `store.stat.plan` carrying the engine, statement key, shape digest, and verdict rule; the drift counter projects through the `#STORE_INSTRUMENTS` arm.
- Packages: Npgsql, DuckDB.NET.Data.Full, Microsoft.Data.Sqlite, System.IO.Hashing, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a fourth engine is one `PlanSubject` case and one leg; a richer shape facet is one row in the pg facet list or one decode line; zero new surface — a per-engine capture service or a timing-bearing digest is the deleted form.
- Boundary: the digest preimage carries SHAPE facets only — a timing or cardinality byte makes every run drift, the deleted form; the pg statement key joins `pg_stat_statements.queryid` so the explaining half joins the `#PG_STAT_HARVEST` evidence, and the `#SQLITE_STATUS_HARVEST` full-scan tell names the suspect statement this leg explains; the pg leg's `ANALYZE` executes the statement, so capture runs deliberately on a suspect lane, never ambient; the DuckDB leg reads `EXPLAIN (FORMAT json)`'s `physical_plan` row without arming the profiling bracket, so plan capture and profile harvest stay independent probes.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlanEngine {
    public static readonly PlanEngine Postgres = new("postgres");
    public static readonly PlanEngine Duck = new("duckdb");
    public static readonly PlanEngine Sqlite = new("sqlite");
}

// Capture request union: one entry discriminates the engine by the value's shape.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanSubject {
    private PlanSubject() { }
    public sealed record Postgres(NpgsqlDataSource Source, string Sql, Option<long> QueryId) : PlanSubject;
    public sealed record Duck(DuckDBConnection Connection, string Sql) : PlanSubject;
    public sealed record Sqlite(SqliteConnection Connection, string Sql) : PlanSubject;

    public PlanEngine Engine => this.Switch(
        postgres: static _ => PlanEngine.Postgres,
        duck: static _ => PlanEngine.Duck,
        sqlite: static _ => PlanEngine.Sqlite);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanVerdict {
    private PlanVerdict() { }
    public sealed record Baselined(UInt128 Shape) : PlanVerdict;
    public sealed record Unchanged(UInt128 Shape) : PlanVerdict;
    public sealed record Drifted(UInt128 Held, UInt128 Observed) : PlanVerdict;

    public string Rule => this.Switch(
        baselined: static _ => "baselined",
        unchanged: static _ => "unchanged",
        drifted: static _ => "drifted");
}

// --- [MODELS] ---------------------------------------------------------------------------
// Statement-identity baseline persists in the relational identity tier: pg `queryid` when the
// server computes one, else the invariant hash of the statement text — one identity axis per engine.
public sealed record PlanBaselineRow(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

public sealed record PlanReceipt(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, PlanVerdict Verdict, Instant At, Guid Correlation) {
    public string Rule => Verdict.Rule;
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class PlanProfile {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.plan");

    // One capture entry over the closed subject family: leg -> shape digest -> baseline compare -> typed
    // verdict; a first sighting persists through `baseline` and reads Baselined, never a silent implicit write.
    public static IO<PlanReceipt> Capture(PlanSubject subject, Func<PlanEngine, UInt128, IO<Option<PlanBaselineRow>>> held, Func<PlanBaselineRow, IO<Unit>> baseline, ProjectionContext frame) =>
        from captured in subject.Switch(postgres: Postgres, duck: Duck, sqlite: Sqlite)
        from prior in held(subject.Engine, captured.Key)
        from verdict in prior.Match(
            Some: row => IO.pure<PlanVerdict>(row.Shape == captured.Shape
                ? new PlanVerdict.Unchanged(captured.Shape)
                : new PlanVerdict.Drifted(row.Shape, captured.Shape)),
            None: () => baseline(new PlanBaselineRow(subject.Engine, captured.Key, captured.Shape, frame.Now()))
                .Map(_ => (PlanVerdict)new PlanVerdict.Baselined(captured.Shape)))
        select new PlanReceipt(subject.Engine, captured.Key, captured.Shape, verdict, frame.Now(), frame.Correlation);

    // Pg leg: EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) yields one json scalar; the shape fold reads node kind,
    // join type, relation, and index facets recursively over "Plans" — never a timing or row-count value.
    static IO<(UInt128 Key, UInt128 Shape)> Postgres(PlanSubject.Postgres leg) =>
        IO.liftAsync(async () => {
            await using var command = leg.Source.CreateCommand($"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {leg.Sql}");
            using var plan = JsonDocument.Parse((string?)await command.ExecuteScalarAsync() ?? "[]");
            var shape = new XxHash128();
            PgShape(plan.RootElement[0].GetProperty("Plan"), shape);
            return (leg.QueryId.Match(Some: static id => (UInt128)unchecked((ulong)id), None: () => Key(leg.Sql)), shape.GetCurrentHashAsUInt128());
        });

    // Duck leg: EXPLAIN (FORMAT json) emits (explain_key, explain_value) rows; the physical_plan row carries
    // Operator tree folds by name over children with no profiling bracket armed.
    static IO<(UInt128 Key, UInt128 Shape)> Duck(PlanSubject.Duck leg) =>
        IO.liftAsync(async () => {
            await using var command = leg.Connection.CreateCommand();
            command.CommandText = $"EXPLAIN (FORMAT json) {leg.Sql}";
            await using var reader = await command.ExecuteReaderAsync();
            var payload = "[]";
            while (await reader.ReadAsync()) {
                if (reader.GetString(0) is "physical_plan") { payload = reader.GetString(1); }
            }
            using var plan = JsonDocument.Parse(payload);
            var shape = new XxHash128();
            foreach (var root in plan.RootElement.EnumerateArray()) { DuckShape(root, shape); }
            return (Key(leg.Sql), shape.GetCurrentHashAsUInt128());
        });

    // Sqlite leg: EXPLAIN QUERY PLAN rows' detail column carries SCAN/SEARCH text and the index name.
    // Whole shape digests in row order.
    static IO<(UInt128 Key, UInt128 Shape)> Sqlite(PlanSubject.Sqlite leg) =>
        IO.liftAsync(async () => {
            await using var command = leg.Connection.CreateCommand();
            command.CommandText = $"EXPLAIN QUERY PLAN {leg.Sql}";
            await using var reader = await command.ExecuteReaderAsync();
            var shape = new XxHash128();
            while (await reader.ReadAsync()) { shape.Append(Encoding.UTF8.GetBytes(reader.GetString(3))); }
            return (Key(leg.Sql), shape.GetCurrentHashAsUInt128());
        });

    static void PgShape(JsonElement node, XxHash128 shape) {
        foreach (var facet in (string[])["Node Type", "Join Type", "Relation Name", "Index Name"]) {  // Exemption: hashing kernel over the plan tree
            if (node.TryGetProperty(facet, out var value)) { shape.Append(Encoding.UTF8.GetBytes(value.GetString() ?? string.Empty)); }
        }
        if (node.TryGetProperty("Plans", out var children)) {
            foreach (var child in children.EnumerateArray()) { PgShape(child, shape); }
        }
    }

    static void DuckShape(JsonElement node, XxHash128 shape) {
        if (node.TryGetProperty("name", out var name)) { shape.Append(Encoding.UTF8.GetBytes(name.GetString() ?? string.Empty)); }
        if (node.TryGetProperty("children", out var children)) {
            foreach (var child in children.EnumerateArray()) { DuckShape(child, shape); }  // Exemption: hashing kernel over the plan tree
        }
    }

    static UInt128 Key(string sql) => XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(sql));
}
```

## [07]-[HOOK_RAIL]

- Owner: `PersistenceHooks` — the folder's typed point roster over the kernel signal capsule, with the `Guarded` and `Swept` composition adapters that fire veto points without touching owner rail signatures.
- Cases: six points — `rasm.persistence.element.append` (`Veto` over `GraphStoreOp`), `rasm.persistence.element.committed` (`Observe` over `GraphReceipt`), `rasm.persistence.egress.delivered` (`Observe` over `EgressReceipt`), `rasm.persistence.retention.sweep` (`Veto` over `SweepVerdict`), `rasm.persistence.merge.conflict` (`Observe` over `ConflictReceipt`), `rasm.persistence.recovery.replay` (`Replay` over `StepFact`).
- Entry: `PersistenceHooks.Live()` — one fresh roster per composition so two apps never share a mount; `Points()` — the census the composition root spreads into the settled `HookRegistry.Mount` beside the AppHost rail's own points, structural id uniqueness across both rosters.
- Auto: veto fold, observe isolation, and replay depth ride the settled `HookPoint<TFact>` capsule; a throwing or failing subscriber parks as `IsolatedFault` on the roster's evidence cell — subscriber failure is hook-rail evidence, never a `StatFault` arm and never a broken emitter.
- Receipt: none — a hook fire is the evidence event itself; the emitter's own receipt already carries the fact.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new point is one typed field and one `Points` row; a subscriber is one `Observe`/`Veto` call at composition; a new lifecycle domain contributes its point through this roster, never a second registry type.
- Boundary: point ids ride the `rasm.<pkg>.<domain>.<point>` grammar the settled `HookId` factory admits, `persistence` the pkg segment; the owning pages fire through the composition adapters and injected taps — a hook parameter on an owner rail signature is the deleted form; the AppHost `Receipt` point already taps every envelope this package emits, so these points carry what that tap cannot: the TYPED facts and the two veto modalities; policy engines, audit sidecars, and UI live-update legs subscribe here without touching owner rails.

```csharp signature
// Folder hook roster mints six typed points per composition over the kernel capsule.
// Subscriber faults park as `IsolatedFault` on the roster cell; the `StatFault` band stays the harvest rail's.
public sealed record PersistenceHooks(
    HookPoint<GraphStoreOp> ElementAppend,
    HookPoint<GraphReceipt> ElementCommitted,
    HookPoint<EgressReceipt> EgressDelivered,
    HookPoint<SweepVerdict> SweepEvict,
    HookPoint<ConflictReceipt> MergeConflict,
    HookPoint<StepFact> RecoveryReplay,
    Atom<Seq<IsolatedFault>> Faults) {

    public static PersistenceHooks Live() {
        var faults = Atom(Seq<IsolatedFault>());
        return new(
            new(HookId.Create("rasm.persistence.element.append"), HookModality.Veto, faults),
            new(HookId.Create("rasm.persistence.element.committed"), HookModality.Observe, faults),
            new(HookId.Create("rasm.persistence.egress.delivered"), HookModality.Observe, faults),
            new(HookId.Create("rasm.persistence.retention.sweep"), HookModality.Veto, faults),
            new(HookId.Create("rasm.persistence.merge.conflict"), HookModality.Observe, faults),
            new(HookId.Create("rasm.persistence.recovery.replay"), HookModality.Replay, faults),
            faults);
    }

    // Census spreads into the settled `HookRegistry.Mount` beside the AppHost rail's
    // points — one frozen audit table per composition, duplicate ids structurally fatal.
    public Seq<IHookPoint> Points() => Seq<IHookPoint>(
        ElementAppend, ElementCommitted, EgressDelivered, SweepEvict, MergeConflict, RecoveryReplay);

    // Append seam crosses the veto fold BEFORE the rail runs (a refusal returns on the caller's
    // own Fin rail), and the settled receipt fires the committed observe tap — a decoration at the composition
    // root, never a hook parameter on `GraphStore.Run`.
    public IO<Fin<GraphReceipt>> Guarded(IDocumentSession session, GraphStoreOp op, ProjectionContext frame, CancellationToken cancellationToken) =>
        ElementAppend.Fire(op).Match(
            Succ: admitted => GraphStore.Run(session, admitted, frame, cancellationToken)
                .Map(outcome => outcome.Map(receipt => ElementCommitted.Fire(receipt).IfFail(receipt))),
            Fail: error => IO.pure(Fin<GraphReceipt>.Fail(error)));

    // Sweep seam crosses every evict verdict before the retention executor runs; a subscriber
    // refusal DOWNGRADES that verdict to Held (the artifact survives the pass, receipted under the veto rule),
    // never an aborted sweep; retained verdicts pass untouched.
    public Seq<SweepVerdict> Swept(Seq<SweepVerdict> verdicts) =>
        verdicts.Map(verdict => verdict.Evicts
            ? SweepEvict.Fire(verdict).IfFail(_ => new SweepVerdict.Held(verdict.Key, verdict.Bytes, "hook-veto"))
            : verdict);
}
```

## [08]-[USAGE_PROJECTION]

- Owner: `StoreUsage` — the (tenant, class, tier) usage census; `UsageReceipt` the chargeback row.
- Entry: `StoreUsage.Fold(Seq<BlobCatalogRow> catalog, Seq<(UInt128 Tenant, EgressReceipt Drain)> drains, ProjectionContext frame)` — one pure fold over the content-lineage catalog snapshot and the drain receipts; a resumed census re-folds with no journal.
- Auto: catalog rows group under `(tenant, class, tier)` summing the SEALED byte figures (never a later filesystem stat) and counting objects; drain receipts fold their delivered counts onto the drain tenant's `stream`-class row — the egress obligation is event-stream custody; the census batch fans under `store.cost.usage` carrying its `rows` array, and the `#STORE_INSTRUMENTS` arm folds it into the composition `StoreLevel` usage atom the multi-measurement gauges read at collection cadence.
- Receipt: `UsageReceipt` rows under `store.cost.usage`; identity-tier journal rows stay billing truth and the instrument projection is the lossy dashboard channel.
- Packages: LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new usage axis is one `UsageReceipt` field and one gauge row; a new source census is one `Fold` argument row.
- Boundary: tenant is the injected frame/catalog column (the RLS partition), never an ambient read; the per-tenant meter dimension rides the `rasm.tenant` spelling under the estate `*`-wildcard series cap — above the cap, attribution rides receipts and exemplar-sampled traces, never unbounded tag values.

```csharp signature
// Chargeback bytes/objects fold from the `Store/blobstore#BLOB_GC` `BlobCatalogRow` census,
// deliveries from the egress drain receipts under the drain frame's tenant.
public sealed record UsageReceipt(UInt128 Tenant, RetentionClass Class, StorageTier Tier, long Bytes, long Objects, long Deliveries, Instant At, Guid Correlation) {
    // Multi-measurement gauge projects one row per measurement with tenant/class/tier tags.
    public Measurement<long> Measured(long value) => new(value,
        new KeyValuePair<string, object?>("rasm.tenant", Tenant.ToString()),
        new KeyValuePair<string, object?>("class", Class.Key),
        new KeyValuePair<string, object?>("tier", Tier.Key));
}

public static class StoreUsage {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.cost.usage");

    // One census fold; groupings are non-empty by construction, so the head read is total.
    public static Seq<UsageReceipt> Fold(Seq<BlobCatalogRow> catalog, Seq<(UInt128 Tenant, EgressReceipt Drain)> drains, ProjectionContext frame) =>
        toSeq(catalog
            .GroupBy(static row => (row.Tenant, Class: row.Class.Key, Tier: row.Tier.Key))
            .Select(group => new UsageReceipt(
                group.Key.Tenant, group.First().Class, group.First().Tier,
                group.Sum(static row => row.Bytes), group.Count(), 0L, frame.Now(), frame.Correlation)))
        + toSeq(drains
            .GroupBy(static row => row.Tenant)
            .Select(group => new UsageReceipt(
                group.Key, RetentionClass.Stream, StorageTier.Standard,
                0L, 0L, group.Sum(static row => (long)row.Drain.Delivered), frame.Now(), frame.Correlation)));
}
```

## [09]-[STORE_INSTRUMENTS]

- Owner: `StoreInstruments` — the Persistence `InstrumentRow` roster, the `TelemetryContributorPort` mint, the census egress, and the kind-keyed projection arms; `StoreLevel` — the level atoms the observable gauges read; `StoreTelemetryCensus`/`CensusRow` — the declared-truth wire record the dashboard plane compiles from.
- Cases: statement-duration histogram off the pg statements batch; I/O hit-ratio gauge off the pg I/O batch; DuckDB latency and row histograms off the profile receipt; SQLite VM-step and full-scan counters off the interval receipt with the cache-ratio gauge off the connection sample; egress delivery and dead-letter counters with the drain-duration histogram off the drain receipts; the plan-drift counter off the plan receipt; the usage byte/object/delivery gauges off the usage census atom.
- Entry: `StoreInstruments.Telemetry(LevelCells cells, StoreLevel usage, string version, string schemaUrl)` — the contributor port peer of the AppHost host roster, carrying every row under the `Rasm.Persistence` scope with the semconv schema coordinate the mint stamps as `MeterOptions.TelemetrySchemaUrl`; `StoreInstruments.Arms(StoreLevel usage)` — the kind-keyed projection rows the AppHost receipt fan mounts beside its own arms at the composition root; `StoreInstruments.Census(LevelCells cells, StoreLevel usage, string version, SlotRegistry registry)` — the declared-truth census folding rows, bucket thresholds, mounted slots, and projected-arm keys into one wire record, so a new instrument or slot appears on the board with zero dashboard edits and a hand-listed metric name in a dashboard is the deleted form.
- Auto: the projection subscribes as one observe row on the AppHost hook rail's receipt point, so every envelope the sink emits projects with zero call-site metering; level-shaped facts fold into the `StoreLevel` atoms and ride observable gauges at collection cadence, so a polled level never aliases through a synchronous gauge; a NodaTime `Duration` crosses the wire as its JsonRoundtrip text and `Seconds` is the one arm-side decode.
- Receipt: none — the arms project the harvest, plan, usage, and egress receipts; a metric minted beside them is a second truth.
- Packages: LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one projected slot is one `Arms` row and its instrument rows here; a slot without an `Arms` row is receipt-only by default, so projection is opt-in per row and no page declares the default; the census follows rows and slots with zero edits.
- Boundary: the port `Scope` string is the package id the composing root admits by name, and instruments mount through the composing root's meter mint, never a package-local `Meter`; scalar ratio levels ride the composition `LevelCells` and the usage census the composition `StoreLevel`, so no process-static cell exists; pg_stat and engine-status sources are server- and process-global, so no harvest row carries a tenant tag — ONLY the usage gauges carry the `rasm.tenant` dimension, capped by the estate `*`-wildcard view; arm bodies are the one place receipt wire names meet instrument writes, and an arm never re-validates the payload its typed receipt already admitted.

```csharp signature
// Scalar ratios ride the composition's kernel LevelCells; the usage census snapshot is the one
// folder-shaped cell, composition-owned like every level.
public sealed record StoreLevel(Atom<Seq<UsageReceipt>> Usage) {
    public static StoreLevel Of() => new(Atom(Seq<UsageReceipt>()));
}

// Census wire pair compiles instrument rows with bucket hints, the
// mounted slot census, and the projected-arm keys — declared truth, never a hand-listed metric name.
public sealed record CensusRow(string Name, string Unit, string Description, ImmutableArray<double> Buckets);

public sealed record StoreTelemetryCensus(string Source, string Version, Seq<CensusRow> Instruments, Seq<string> Slots, Seq<string> Projected);

public static class StoreInstruments {
    static readonly ImmutableArray<double> StatementSeconds = [0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10];
    static readonly ImmutableArray<double> ProfileSeconds = [0.001, 0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60];

    // Bucket-advice table carries the boundaries histogram rows declare, keyed
    // by instrument name so the alert plane derives burn-rate windows from declared thresholds.
    static readonly FrozenDictionary<string, ImmutableArray<double>> Thresholds = new Dictionary<string, ImmutableArray<double>> {
        ["rasm.persistence.statement.duration"] = StatementSeconds,
        ["rasm.persistence.duckdb.latency"] = ProfileSeconds,
        ["rasm.persistence.egress.drain.duration"] = ProfileSeconds,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public const string Scope = "Rasm.Persistence";

    public static Seq<InstrumentRow> Rows(LevelCells cells, StoreLevel usage) => Seq(
        new InstrumentRow("rasm.persistence.statement.duration", "s", "mean execution time per harvested top-N server statement",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text, tags: null,
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = StatementSeconds })),
        new InstrumentRow("rasm.persistence.io.hit.ratio", "1", "shared-buffer hit ratio over the pg_stat_io window",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.persistence.duckdb.latency", "s", "profiled analytical statement latency",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text, tags: null,
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = ProfileSeconds })),
        new InstrumentRow("rasm.persistence.duckdb.rows", "{row}", "rows returned per profiled analytical statement",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.sqlite.vm.steps", "{step}", "virtual-machine steps per embedded statement",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.sqlite.fullscan.steps", "{step}", "full-scan steps per embedded statement — the plan-regression tell",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.sqlite.cache.ratio", "1", "embedded page-cache hit ratio over the sampled connection",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, cells.Reader(name), unit, text)),
        new InstrumentRow("rasm.persistence.egress.deliveries", "{delivery}", "egress deliveries by sink and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.egress.deadletters", "{letter}", "dead-lettered egress entries by sink",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.egress.drain.duration", "s", "wall duration per outbox drain by sink",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text, tags: null,
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = ProfileSeconds })),
        new InstrumentRow("rasm.persistence.plan.drift", "{plan}", "plan-shape drift verdicts by engine",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow("rasm.persistence.usage.bytes", "By", "durable bytes by tenant, retention class, and storage tier",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, () => usage.Usage.Value.Map(static row => row.Measured(row.Bytes)), unit, text)),
        new InstrumentRow("rasm.persistence.usage.objects", "{object}", "durable objects by tenant, retention class, and storage tier",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, () => usage.Usage.Value.Map(static row => row.Measured(row.Objects)), unit, text)),
        new InstrumentRow("rasm.persistence.usage.deliveries", "{delivery}", "egress deliveries by tenant over the usage census window",
            (meter, name, unit, text) => meter.CreateObservableGauge(name, () => usage.Usage.Value.Map(static row => row.Measured(row.Deliveries)), unit, text)));

    // Arm bodies are the one place receipt wire names meet instrument writes; the AppHost fan merges this table beside its own at the Mount seam.
    public static FrozenDictionary<string, InstrumentArm> Arms(StoreLevel usage) =>
        new Dictionary<string, InstrumentArm> {
            [PgStatHarvest.StatementsSlot.ToString()] = static (set, _, payload) => {
                foreach (var row in payload.GetProperty("rows").EnumerateArray()) {
                    ignore(set.Record("rasm.persistence.statement.duration", row.GetProperty("meanExecMs").GetDouble() / 1000d));
                }
            },
            [PgStatHarvest.IoSlot.ToString()] = static (_, cells, payload) => {
                var (hits, reads) = payload.GetProperty("rows").EnumerateArray().Aggregate((0L, 0L),
                    static (sum, row) => (sum.Item1 + row.GetProperty("hits").GetInt64(), sum.Item2 + row.GetProperty("reads").GetInt64()));
                ignore(cells.Level("rasm.persistence.io.hit.ratio", hits + reads > 0 ? (double)hits / (hits + reads) : 1d));
            },
            [DuckProfileHarvest.Slot.ToString()] = static (set, _, payload) => {
                ignore(set.Record("rasm.persistence.duckdb.latency", payload.GetProperty("latencySeconds").GetDouble()));
                ignore(set.Record("rasm.persistence.duckdb.rows", payload.GetProperty("rowsReturned").GetInt64()));
            },
            [SqliteStatHarvest.StatementsSlot.ToString()] = static (set, _, payload) => {
                ignore(set.Count("rasm.persistence.sqlite.vm.steps", payload.GetProperty("vmSteps").GetInt64()));
                ignore(set.Count("rasm.persistence.sqlite.fullscan.steps", payload.GetProperty("fullScanSteps").GetInt64()));
            },
            [SqliteStatHarvest.ConnectionSlot.ToString()] = static (_, cells, payload) => {
                var (hit, miss) = (payload.GetProperty("cacheHits").GetInt64(), payload.GetProperty("cacheMisses").GetInt64());
                ignore(cells.Level("rasm.persistence.sqlite.cache.ratio", hit + miss > 0 ? (double)hit / (hit + miss) : 1d));
            },
            [EgressPump.DrainSlot.ToString()] = static (set, _, payload) => {
                var sink = new KeyValuePair<string, object?>("sink", payload.GetProperty("sink").GetString());
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("delivered").GetInt64(), sink, new("outcome", "delivered")));
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("held").GetInt64(), sink, new("outcome", "held")));
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("deadLettered").GetInt64(), sink, new("outcome", "dead")));
                ignore(set.Record("rasm.persistence.egress.drain.duration", Seconds(payload.GetProperty("elapsed")), sink));
            },
            [EgressPump.DeadLetterSlot.ToString()] = static (set, _, payload) =>
                ignore(set.Count("rasm.persistence.egress.deadletters", 1L,
                    new KeyValuePair<string, object?>("sink", payload.GetProperty("sink").GetString()))),
            [PlanProfile.Slot.ToString()] = static (set, _, payload) => {
                if (payload.GetProperty("rule").GetString() is "drifted") {
                    ignore(set.Count("rasm.persistence.plan.drift", 1L,
                        new KeyValuePair<string, object?>("engine", payload.GetProperty("engine").GetString())));
                }
            },
            // Usage arm swaps the census snapshot; gauges read value and tags from each row.
            // Atom rows carry sentinel instant/correlation because the envelope stamps the frame.
            // Usage snapshot cell is composition-owned; this arm closes over the composing root's cell.
            [StoreUsage.Slot.ToString()] = (_, _, payload) =>
                ignore(usage.Usage.Swap(_ => toSeq(payload.GetProperty("rows").EnumerateArray().Select(static row =>
                    new UsageReceipt(
                        UInt128.Parse(row.GetProperty("tenant").GetString()!, CultureInfo.InvariantCulture),
                        RetentionClass.Get(row.GetProperty("class").GetString()!),
                        StorageTier.Get(row.GetProperty("tier").GetString()!),
                        row.GetProperty("bytes").GetInt64(), row.GetProperty("objects").GetInt64(),
                        row.GetProperty("deliveries").GetInt64(), NodaConstants.UnixEpoch, Guid.Empty))).Strict())),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Schema coordinate fills the port `SchemaUrl` slot the settled `TelemetryIdentity.Mint` stamps as
    // `MeterOptions.TelemetrySchemaUrl`, so every `rasm.persistence.*` scope reads with pinned semantics and
    // no folder OTel reference exists.
    public static TelemetryContributorPort Telemetry(LevelCells cells, StoreLevel usage, string version, string schemaUrl) =>
        new(Scope, version, schemaUrl, Rows(cells, usage));

    // Declared-truth census carries rows with bucket hints, mounted slots, and projected-arm keys.
    public static StoreTelemetryCensus Census(LevelCells cells, StoreLevel usage, string version, SlotRegistry registry) {
        FrozenDictionary<string, InstrumentArm> arms = Arms(usage);
        return new(
            Scope, version,
            Rows(cells, usage).Map(static row => new CensusRow(row.Name, row.Unit, row.Description,
                Thresholds.TryGetValue(row.Name, out var buckets) ? buckets : [])),
            toSeq(registry.Slots.Order(StringComparer.Ordinal)),
            toSeq(arms.Keys.Order(StringComparer.Ordinal)));
    }

    // NodaTime `Duration` crosses the wire as its JsonRoundtrip text (`api-nodatime-stj` `DurationConverter`);
    // One arm-side decode yields seconds beside the AppHost fan's `Seconds` peer.
    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString()!).Value.TotalSeconds;
}
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
