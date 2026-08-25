# [PERSISTENCE_STORE_OBSERVABILITY]

Engine-stat observability, the receipt-slot registry, the hook rail, and the store instrument contributor: one slot grammar names every evidence stream Persistence emits, one registry enforces uniqueness at composition, one harvest fold turns each engine's statistics surface — PostgreSQL cumulative views, DuckDB profiling output, SQLite status counters — into typed receipts, one plan-shape rail turns suspect statements into typed drift verdicts, one hook roster gives the durable lifecycle its veto/observe/replay points, one usage census turns storage truth into chargeback evidence, and one contributor projects the receipt fan into `rasm.persistence.*` instruments. Embedded engines expose no scrape surface, so the embedding process is their observer and the receipt rail is their observability.

Settled composition: `Rasm/Domain/rails#FAULT_BAND` carries `FaultBand`, `[FaultCase]`, `generated identity admission`, and `Fault`; `Rasm/Domain/frame#SOURCE` carries `TelemetrySource` and `CorrelationId`, `Rasm/Domain/frame#TENANCY` carries `TenantId`/`TenantContext`, and `Rasm/Domain/frame#RECEIPT_PORT` carries `ReceiptEnvelope` and `ReceiptSinkPort`; `Rasm/Domain/instrument#SPEC` carries `Buckets`, `InstrumentKind`, `MeasureForm`, and `InstrumentSpec`, `Rasm/Domain/instrument#WRITE` carries `InstrumentSet`, `Rasm/Domain/instrument#MOUNT` carries `LevelCells` and `TelemetryIdentity`, and `Rasm/Domain/telemetry#CONTRIBUTE` carries `InstrumentArm` and `TelemetryContributorPort`; `Rasm/Domain/hooks#HOOK_POINT` carries `HookId`, `TraceScope`, `HookModality`, `IsolatedFault`, and `IHookPoint`, `Rasm/Domain/hooks#HOOK_RAIL` carries `IHookRoster`, `IHookFact`, `IHookSpan`, `HookGate`, `HookTap`, `FaultCell`, and `HookRail`, `Rasm/Domain/hooks#HOOK_MOUNT` carries `HookBinding` and `HookMounts`, and `Rasm/Domain/hooks#HOOK_REGISTRY` carries `HookRegistry` — all reach this S2 package as kernel S0 references, so no AppHost type crosses down. `ProjectionContext` is this package's own `Element/graph#STORE_RAIL` frame, and the receipt observe tap arrives from `Rasm.AppHost/Observability/hooks#HOOK_RAIL` at composition. One `ThinktectureJsonConverterFactory` registration carries each generated owner across a receipt wire as its key scalar, so a `[ValueObject]` or `[SmartEnum]` field decodes as a bare key while a plain record decodes as its members. Provider instrumentation subscribes at the AppHost root as four settled rows: `Npgsql.OpenTelemetry` — `AddNpgsql()` tracing and the `Npgsql` meter by name under the `AddView` posture the `NpgsqlDataSourceBuilder.Name` pool dimension keys; `OpenTelemetry.Instrumentation.EntityFrameworkCore` — `AddEntityFrameworkCoreInstrumentation` beside `AddNpgsql`, the ORM-layer command span nesting over the ADO-layer driver span, complementary never redundant, trace-only beside the `Npgsql` meter roster; `OpenTelemetry.Instrumentation.StackExchangeRedis` — `AddRedisInstrumentation(connection)` binding the cache multiplexer with the handle captured through `ConfigureRedisInstrumentation` so `AddConnection` binds the egress `RedisStream` multiplexer under one subscription, tracer-only with `Filter`/`Enrich` unset on the hot cache path; `OpenTelemetry.Instrumentation.AWS` — `AddAWSInstrumentation` on the tracer AND meter builders once, the shared `AWSSDK.Core` pipeline customizer spanning both the `AWSSDK.S3` object-store and `AWSSDK.KeyManagementService` custody clients, `SuppressDownstreamInstrumentation` set where HTTP instrumentation co-admits. Metric names are dotted `rasm.<domain>.<measure>` carrying no unit suffix, units UCUM, scope id the `TelemetrySource.Persistence` row.

## [01]-[INDEX]

- [02]-[SLOT_REGISTRY]: `store.<domain>.<verb>` grammar, the registry fold, and the page-contributed mount.
- [03]-[PG_STAT_HARVEST]: `pg_stat_statements` and `pg_stat_io` typed harvest receipts.
- [04]-[DUCKDB_PROFILE_HARVEST]: Profiling-JSON harvest off the analytical lane.
- [05]-[SQLITE_STATUS_HARVEST]: Statement and connection status counters off the raw bridge.
- [06]-[PLAN_PROFILE]: Three-engine plan-shape capture, digest baselines, and the typed drift verdict.
- [07]-[HOOK_RAIL]: `PersistencePoint` closes the `rasm.persistence.<domain>.<point>` vocabulary over the kernel roster floor, `PersistenceFact` its one closed payload family, and `PersistenceHooks` seats both on the kernel rail, mount table, and evidence cell.
- [08]-[USAGE_PROJECTION]: (tenant, class, tier) usage census under `store.cost.usage`, its tenancy lift, and its wire inverse.
- [09]-[STORE_INSTRUMENTS]: `rasm.persistence.*` `InstrumentSpec` roster, contributor port, census egress, and receipt-projection arms.
- [10]-[STORE_BOARD]: `StoreDescriptors` binds the kernel board pack over that roster.

## [02]-[SLOT_REGISTRY]

- Owner: `[FaultCase]` — the harvest family's generated fault identity on the kernel floor, one band read and one declared offset per case; `StoreSlot` `[ValueObject<string>]` — the slot name under the `store.<domain>.<verb>` grammar, the verb a dotted path when one domain carries verb families; `SlotRegistry` — the composition-time catalog of every slot this package emits.
- Entry: `SlotRegistry.Mount(params ReadOnlySpan<StoreSlot> slots)` — freezes the catalog and throws on a duplicate at composition; `SlotRegistry.Mounted(params ReadOnlySpan<StoreSlot> contributed)` — the composition-root census spreading every page's contributed roster and any sibling-package family the call site supplies; `SlotRegistry.Admit(SlotRegistry registry, StoreSlot slot)` — the pre-send gate every receipt emission crosses, so an unregistered slot is a typed refusal, never a silent new stream.
- Auto: each owning page carries one `Slots` roster on its primary owner and `Mounted` spreads them, so the registry is the one census of the emitted-signal surface and discovery stops being page-by-page archaeology.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new evidence stream is one `StoreSlot` row on its owning page's roster; the grammar admits a new domain or verb with zero registry edits.
- Boundary: the slot is the `kind` argument the sink `Send` carries, so slot vocabulary and wire kind are one spelling; this page mints its own slots — `store.stat.statements`, `store.stat.io`, `store.stat.duckdb`, `store.stat.sqlite.statements`, `store.stat.sqlite.connection`, `store.stat.plan`, `store.cost.usage`, `store.cost.fact` — and every other page's slots enter as its contributed rows, so the registry owns uniqueness while each page owns its spellings; a sibling PACKAGE's family — the Fabrication `store.fabrication.<domain>.<verb>` shop-state rows (remnant inventory, fleet performance horizons, magazine slot state, capability history), each pairing a typed read and write receipt on its Fabrication owner — enters through the `Mounted` `contributed` span at composition, so a foreign family is call-site data under the same uniqueness law, never a census edit; a per-occurrence discriminant — a traversal's query case, a sink's lane — rides the receipt payload, never the slot string, so the census stays frozen while payloads vary.

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public readonly partial struct StoreSlot {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Split('.') is ["store", var domain, .. var verbs]
            && domain.Length > 0 && verbs.Length >= 1 && verbs.All(static verb => verb.Length > 0)
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-')
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { value }));

    public static Option<StoreSlot> Owned(string sql) {
        if (!sql.StartsWith("-- ", StringComparison.Ordinal)) { return None; }
        int end = sql.IndexOf('\n', StringComparison.Ordinal);
        return TryCreate((end < 0 ? sql[3..] : sql[3..end]).Trim(), out StoreSlot slot) ? Some(slot) : None;
    }
}

public sealed record SlotRegistry(FrozenSet<string> Slots) {
    public static Fin<SlotRegistry> Mount(params ReadOnlySpan<StoreSlot> slots) =>
        Framed(toSeq(slots.ToArray()).Map(static slot => slot.ToString()).Strict());

    static Fin<SlotRegistry> Framed(Seq<string> keys) =>
        keys.Collisions(static key => key) is { IsEmpty: false } forked
            ? Fin.Fail<SlotRegistry>(new StatFault.SlotCollision(toSeq(forked.Order(StringComparer.Ordinal))))
            : Fin.Succ(new SlotRegistry(keys.ToFrozenSet(StringComparer.Ordinal)));

    public static Fin<SlotRegistry> Mounted(params ReadOnlySpan<StoreSlot> contributed) => Mount([
        PgStatHarvest.StatementsSlot, PgStatHarvest.IoSlot, DuckProfileHarvest.Slot,
        SqliteStatHarvest.StatementsSlot, SqliteStatHarvest.ConnectionSlot, PlanProfile.Slot,
        StoreUsage.Slot, StoreUsage.FactSlot, .. RetentionSweep.Slots,
        .. GraphStore.Slots, .. TabularSource.Slots, .. ScheduleSource.Slots, .. GeoSource.Slots,
        .. IssueSource.Slots, .. Coordinate.Slots, .. ClusterProvision.Slots, .. ObjectIo.Slots,
        .. ModelResultIndex.Slots, .. ColumnarLane.Slots, .. ReadRouter.Slots, .. GraphSession.Slots,
        .. Federation.Slots, .. Traversals.Slots, .. SearchRoute.Slots, .. OpLog.Slots,
        .. EgressPump.Slots, .. CdcIngress.Slots, .. IdentityRail.Slots, .. StructuralMerge.Slots, .. Crdt.Slots, .. RecoveryRoutes.Slots,
        .. TimeTravel.Slots, .. contributed]);

    public static Fin<StoreSlot> Admit(SlotRegistry registry, StoreSlot slot) =>
        registry.Slots.Contains(slot.ToString())
            ? Fin.Succ(slot)
            : Fin.Fail<StoreSlot>(new StatFault.SlotUnregistered(slot.ToString()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StatFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreStat;
    private StatFault() { }

    [FaultCase(0)]
    public sealed partial record MalformedSlot(string Slot) : StatFault();
    [FaultCase(1)]
    public sealed partial record SlotUnregistered(string Slot) : StatFault();
    [FaultCase(2)]
    public sealed partial record HarvestRefused(string Engine, Error Cause) : StatFault(), ICausedFault;
    [FaultCase(3)]
    public sealed partial record SlotCollision(Seq<string> Slots) : StatFault();

    public override string Message => Switch(
        malformedSlot:    static c => $"<store-slot:{c.Slot}>",
        slotUnregistered: static c => $"<store-slot-unregistered:{c.Slot}>",
        harvestRefused:   static c => $"<store-harvest:{c.Engine}:{c.Cause.Message}>",
        slotCollision:    static c => $"<store-slot-collision:{string.Join(',', c.Slots)}>");

    public static Error Lift(string engine, Error error, Func<Exception, bool> recognizes) =>
        error.Exception.Case is Exception raised && recognizes(raised)
            ? new HarvestRefused(engine, error)
            : error;
}
```

## [03]-[PG_STAT_HARVEST]

- Owner: `PgStatHarvest` — the typed read over the two cumulative statement and I/O views; `StatementStatRow` and `IoStatRow` the receipt rows.
- Entry: `PgStatHarvest.Statements(NpgsqlDataSource source, int top)` — the top-N statement rows by total execution time; `PgStatHarvest.Io(NpgsqlDataSource source)` — the per-backend-type I/O rows.
- Auto: both harvests ride the pooled `NpgsqlDataSource` the production path owns, so a stats read shares pool pressure with live traffic and never opens a side connection; `pg_stat_statements` requires the `compute_query_id` server posture the provisioning page's extension roster carries, so `queryid` joins a statement row to the driver span's query identity.
- Receipt: `StatementStatRow` — queryid, calls, total and mean execution time, rows, shared-block hits and reads, WAL bytes; `IoStatRow` — backend type, object, context, reads, writes, extends, their byte figures, hits, evictions, fsyncs; the `object` column carries `relation`, `temp relation`, AND `wal`, so WAL I/O rides the same rows with zero widening; each batch fans under `store.stat.statements` / `store.stat.io`.
- Packages: Npgsql, LanguageExt.Core, NodaTime.
- Growth: a new harvested column is one field on the owning row and one select column; a new server view is one harvest member on this owner.
- Boundary: this fold is the query-depth complement to the driver meter seam — the `Npgsql` meter carries operation duration and pool level at the AppHost root while these rows carry per-statement and per-backend server truth as receipts; pg_stat views are server-global, so these receipts carry no tenant brand by ruling and the batch's message envelope carries the frame correlation at the `Send` seam; the three lag gauges stay distinct owners — provisioning's slot lag, recovery's replication lag, and this page's I/O timing never share a row; `track_io_timing` is a deliberate server posture the provisioning verify batch asserts before timing columns read as truth.

```csharp
public sealed record StatementStatRow(
    long QueryId, long Calls, double TotalExecMs, double MeanExecMs, long Rows,
    long SharedBlksHit, long SharedBlksRead, long WalBytes);

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
        LIMIT @top
        """;

    const string IoSql = """
        SELECT backend_type, object, context,
               COALESCE(reads, 0), COALESCE(read_bytes, 0)::bigint,
               COALESCE(writes, 0), COALESCE(write_bytes, 0)::bigint,
               COALESCE(extends, 0), COALESCE(extend_bytes, 0)::bigint,
               COALESCE(hits, 0), COALESCE(evictions, 0), COALESCE(fsyncs, 0)
        FROM pg_stat_io
        WHERE COALESCE(reads, 0) + COALESCE(writes, 0) + COALESCE(extends, 0) + COALESCE(hits, 0) > 0
        """;

    public static IO<Seq<StatementStatRow>> Statements(NpgsqlDataSource source, int top) =>
        Captured("postgres-statements", async () => {
            await using var command = source.CreateCommand(StatementsSql);
            command.Parameters.Add(new NpgsqlParameter<int>("top", top));
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
        Captured("postgres-io", async () => {
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

    static IO<T> Captured<T>(string engine, Func<Task<T>> crossing) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => Fin<T>.Succ(await crossing().ConfigureAwait(false))).ConfigureAwait(false))
            .MapFail(error => StatFault.Lift(engine, error, static raised => raised is NpgsqlException)))
        .Bind(IO.liftFin);
}
```

## [04]-[DUCKDB_PROFILE_HARVEST]

- Owner: `DuckProfileHarvest` — the profiling-switch bracket and the profile receipt over the analytical lane's connection.
- Entry: `DuckProfileHarvest.Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context)` — runs one statement under `enable_profiling='json'` with `profiling_output` redirected to the caller's run-scoped artifact path, parses the JSON, deletes the scratch artifact, and folds one `DuckProfileReceipt`.
- Auto: `profiling_mode` stays `standard` for routine harvests and the detailed optimizer metrics enter as one pragma value when a plan investigation demands them; the per-operator tree folds to a digest with the top-cost operator rows, and the bracket deletes the decoded JSON scratch file so the receipt is the retained profile truth.
- Receipt: `DuckProfileReceipt` — latency, CPU time, rows returned, result-set bytes, blocked-thread time, operator-tree digest, top operator rows, the frame's instant and correlation; fans under `store.stat.duckdb`.
- Packages: DuckDB.NET.Data.Full, LanguageExt.Core, NodaTime, System.IO.Hashing.
- Growth: one profiling metric key is one receipt field and one parse line; plan-shape capture and drift verdicts are the `#PLAN_PROFILE` rail's, which probes `EXPLAIN (FORMAT json)` without arming this profiling bracket.
- Boundary: the profiling switch is connection state, so the bracket sets, runs, and resets on every exit path — a lane query outside the bracket runs unprofiled at full speed; `outputPath` arrives from the configured artifact owner, resolves to a full path, escapes as a DuckDB string literal, and is deleted after decode on success or failure, so ambient temp storage and orphaned profile files are forbidden; the harvest borrows the `Query/columnar` connection and mints no second DuckDB lane; the analytical lane is process-scoped, so the receipt carries the frame's correlation and instant while tenant stays a `ProjectionContext` fact the sink's message envelope carries by ruling.

```csharp
public sealed record DuckOperatorRow(string Name, double TimingSeconds, long Cardinality);

public sealed record DuckProfileReceipt(
    double LatencySeconds, double CpuSeconds, long RowsReturned, long ResultSetBytes,
    double BlockedThreadSeconds, UInt128 PlanDigest, Seq<DuckOperatorRow> TopOperators,
    Instant At, CorrelationId Correlation);

public static class DuckProfileHarvest {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.duckdb");

    public static IO<DuckProfileReceipt> Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
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
        }).ConfigureAwait(false)).MapFail(error => StatFault.Lift("duckdb-profile", error,
            static raised => raised is DuckDBException or IOException or JsonException)))
        .Bind(IO.liftFin);

    static DuckProfileReceipt Decode(JsonElement root, ProjectionContext frame) {
        var operators = toSeq(Operators(root).OrderByDescending(static row => row.TimingSeconds).Take(8)).Strict();
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
            ? self + toSeq(children.EnumerateArray()).Bind(Operators)
            : self;
    }
}
```

## [05]-[SQLITE_STATUS_HARVEST]

- Owner: `SqliteStatHarvest` — the per-statement and per-connection counter read over the raw bridge.
- Entry: `SqliteStatHarvest.Arm(SqliteConnection connection)` — the walk's precondition, mounted as the leading `Store/provisioning#EMBEDDED_FLOOR` `EmbeddedRitual.Capabilities` grant; `SqliteStatHarvest.Statements(SqliteConnection connection)` — walks every prepared statement on the connection through `raw.sqlite3_next_stmt` off `SqliteConnection.Handle` and folds the read-and-reset `raw.sqlite3_stmt_status` counters into one interval receipt; `SqliteStatHarvest.Connection(SqliteConnection connection)` — samples the connection gauges off the same handle.
- Auto: statement counters read with the reset flag so each receipt carries one interval's work, while connection gauges sample without reset so cache hit ratio folds over the interval; a full-scan step count or transient-index count above zero on a hot interval is the plan-regression tell — the `#PLAN_PROFILE` sqlite leg names the offending statement.
- Receipt: `SqliteStatementStat` — VM steps, full-scan steps, sorts, transient-index rows — fans under `store.stat.sqlite.statements`; `SqliteConnectionStat` — cache hits, misses, writes, cache bytes, schema and statement bytes — fans under `store.stat.sqlite.connection`, so each receipt shape owns its slot and no consumer sniffs the payload for its kind.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3mc, LanguageExt.Core.
- Growth: one counter is one field and one raw-call line, bounded by the constants the core interop assembly declares — the reprepare/run statement counters and the cache-spill gauge stay off the receipts because `SQLitePCLRaw.core` declares no `SQLITE_STMTSTATUS_REPREPARE`/`SQLITE_STMTSTATUS_RUN`/`SQLITE_DBSTATUS_CACHE_SPILL` constant, and they re-widen the day the core assembly grows the rows.
- Boundary: every raw call crosses the ONE `Store/provisioning#ENGINE_OPERATIONS` `HandleBridge`, so the harvest opens no second native path, reads the same native connection the ADO surface drives, and inherits that capsule's handle refusal and fault capture rather than restating either; `enable_sqlite3_next_stmt` is a REGISTRY arm, not a harvest step — the walk throws on an unarmed connection AND on any handle prepared before the arm, so `Arm` leads the open ritual's capability roster ahead of the first statement and a per-call arm inside the harvest faults on the statements it exists to read; the `sqlite3_next_stmt` walk borrows each statement handle only inside the fold and holds none past it; the per-table `dbstat` space census is probe-gated, never build-assumed — `raw.sqlite3_compileoption_used` over `SQLITE_ENABLE_DBSTAT_VTAB` reads false on the plain `e_sqlite3` build and the bound provider is the `Store/provisioning#EMBEDDED_FLOOR` cipher bundle, so store-level bytes ride the `SCHEMA_USED`/`STMT_USED` gauges and the SQL `PRAGMA page_count`/`page_size` product as the standing form; the embedded store is process-scoped, so these receipts carry no tenant brand by ruling; provider-bundle facts stay engine-layer and never become Persistence vocabulary.

```csharp
public sealed record SqliteStatementStat(int VmSteps, int FullScanSteps, int Sorts, int AutoIndexRows);

public sealed record SqliteConnectionStat(int CacheHits, int CacheMisses, int CacheWrites, int CacheBytes, int SchemaBytes, int StatementBytes);

public static class SqliteStatHarvest {
    public static readonly StoreSlot StatementsSlot = StoreSlot.Create("store.stat.sqlite.statements");
    public static readonly StoreSlot ConnectionSlot = StoreSlot.Create("store.stat.sqlite.connection");

    public static Fin<Unit> Arm(SqliteConnection connection) =>
        HandleBridge.Of(connection).Map(static db => fun(() => db.enable_sqlite3_next_stmt(true))());

    public static Fin<SqliteStatementStat> Statements(SqliteConnection connection) =>
        HandleBridge.Crossed(connection, static db => Fin.Succ(Walk(db)));

    public static Fin<SqliteConnectionStat> Connection(SqliteConnection connection) =>
        HandleBridge.Crossed(connection, static db => Fin.Succ(new SqliteConnectionStat(
            Gauge(db, raw.SQLITE_DBSTATUS_CACHE_HIT), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_MISS),
            Gauge(db, raw.SQLITE_DBSTATUS_CACHE_WRITE), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_USED),
            Gauge(db, raw.SQLITE_DBSTATUS_SCHEMA_USED), Gauge(db, raw.SQLITE_DBSTATUS_STMT_USED))));

    static SqliteStatementStat Walk(sqlite3 db) {
        var (vm, scan, sort, autoIndex) = (0, 0, 0, 0);
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

- Owner: `PlanProfile` — the three-engine plan-shape capture; `PlanSubject` the capture request `[Union]` discriminating the engine by the value's shape, never a mode flag; `PlanEngine` the engine axis; `PlanRule` the compare-outcome vocabulary carrying the stability column a plan-stability share partitions on; `PlanBaselineRow` the statement-identity-keyed baseline row; `PlanVerdict` the closed compare outcome; `PlanReceipt` the probe receipt.
- Cases: `PlanSubject` is `Postgres(NpgsqlDataSource, string, Option<long>) | Duck(DuckDBConnection, string) | Sqlite(SqliteConnection, string)`; `PlanVerdict` is `Baselined | Unchanged | Drifted` — a first sighting persists its shape through the injected `baseline` arrow and reads `Baselined`, a match reads `Unchanged`, a moved digest reads `Drifted` carrying both shapes; `PlanRule` mirrors those three outcomes as rows whose `Stable` column marks the two that hold a plan.
- Entry: `PlanProfile.Capture(PlanSubject subject, held, baseline, frame)` — one entry over the closed subject family; `held`/`baseline` are the relational identity-tier arrows filled at composition, so this owner opens no session and the baseline rows persist beside the identity tier.
- Auto: each leg folds its engine's plan artifact to a SHAPE-ONLY digest — node kinds, join types, relation and index names for PostgreSQL, the physical-operator tree for DuckDB, the `EXPLAIN QUERY PLAN` detail rows for SQLite — so the digest is run-stable: a flipped join order or a lost index moves it, a slow run does not; statement identity is the pg `queryid` when the `compute_query_id` posture supplies one, else the invariant hash of the statement text.
- Receipt: a capture rides `store.stat.plan` carrying the engine, statement key, shape digest, and verdict rule; the `#STORE_INSTRUMENTS` arm counts EVERY capture under its engine and rule tags, so drift reads as a rate over the capture stream rather than as a numerator with no denominator.
- Packages: Npgsql, DuckDB.NET.Data.Full, Microsoft.Data.Sqlite, System.IO.Hashing, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a fourth engine is one `PlanSubject` case and one leg; a fourth compare outcome is one `PlanVerdict` case beside one `PlanRule` row whose `Stable` column re-derives the stability share's good half with no pack edit; a richer shape facet is one row in the pg facet list or one decode line; zero new surface — a per-engine capture service or a timing-bearing digest is the deleted form.
- Boundary: the digest preimage carries SHAPE facets only — a timing or cardinality byte makes every run drift, the deleted form; the pg statement key joins `pg_stat_statements.queryid` so the explaining half joins the `#PG_STAT_HARVEST` evidence, and the `#SQLITE_STATUS_HARVEST` full-scan tell names the suspect statement this leg explains; the pg leg's `ANALYZE` executes the statement, so capture runs deliberately on a suspect lane, never ambient; the DuckDB leg reads `EXPLAIN (FORMAT json)`'s `physical_plan` row without arming the profiling bracket, so plan capture and profile harvest stay independent probes.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlanEngine {
    public static readonly PlanEngine Postgres = new("postgres");
    public static readonly PlanEngine Duck = new("duckdb");
    public static readonly PlanEngine Sqlite = new("sqlite");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlanRule {
    public static readonly PlanRule Baselined = new("baselined", stable: true);
    public static readonly PlanRule Unchanged = new("unchanged", stable: true);
    public static readonly PlanRule Drifted = new("drifted", stable: false);

    public bool Stable { get; }

    public static Seq<string> StableKeys =>
        toSeq(Items).Filter(static row => row.Stable).Map(static row => row.Key).Strict();
}

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

    public string Sql => this.Switch(
        postgres: static leg => leg.Sql,
        duck: static leg => leg.Sql,
        sqlite: static leg => leg.Sql);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlanVerdict {
    private PlanVerdict() { }
    public sealed record Baselined(UInt128 Shape) : PlanVerdict;
    public sealed record Unchanged(UInt128 Shape) : PlanVerdict;
    public sealed record Drifted(UInt128 Held, UInt128 Observed) : PlanVerdict;

    public PlanRule Rule => this.Switch(
        baselined: static _ => PlanRule.Baselined,
        unchanged: static _ => PlanRule.Unchanged,
        drifted: static _ => PlanRule.Drifted);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlanBaselineRow(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Option<StoreSlot> Owner, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

public sealed record PlanReceipt(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Option<StoreSlot> Owner, PlanVerdict Verdict, Instant At, CorrelationId Correlation) {
    public PlanRule Rule => Verdict.Rule;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlanProfile {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.plan");

    public static IO<PlanReceipt> Capture(PlanSubject subject, Func<PlanEngine, UInt128, IO<Option<PlanBaselineRow>>> held, Func<PlanBaselineRow, IO<Unit>> baseline, ProjectionContext frame) =>
        from captured in subject.Switch(postgres: Postgres, duck: Duck, sqlite: Sqlite)
        let owner = StoreSlot.Owned(subject.Sql)
        from prior in held(subject.Engine, captured.Key)
        from verdict in prior.Match(
            Some: row => IO.pure<PlanVerdict>(row.Shape == captured.Shape
                ? new PlanVerdict.Unchanged(captured.Shape)
                : new PlanVerdict.Drifted(row.Shape, captured.Shape)),
            None: () => baseline(new PlanBaselineRow(subject.Engine, captured.Key, captured.Shape, owner, frame.Now()))
                .Map(_ => (PlanVerdict)new PlanVerdict.Baselined(captured.Shape)))
        select new PlanReceipt(subject.Engine, captured.Key, captured.Shape, owner, verdict, frame.Now(), frame.Correlation);

    static IO<(UInt128 Key, UInt128 Shape)> Postgres(PlanSubject.Postgres leg) =>
        Captured("postgres-plan", static raised => raised is NpgsqlException or JsonException, async () => {
            await using var command = leg.Source.CreateCommand($"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {leg.Sql}");
            using var plan = JsonDocument.Parse((string?)await command.ExecuteScalarAsync() ?? "[]");
            var shape = new XxHash128();
            foreach (var entry in plan.RootElement.EnumerateArray()) {
                if (entry.TryGetProperty("Plan", out var root)) { Shape(root, shape, PgFacets, "Plans"); }
            }
            return (leg.QueryId.Match(Some: static id => (UInt128)unchecked((ulong)id), None: () => Key(leg.Sql)), shape.GetCurrentHashAsUInt128());
        });

    static IO<(UInt128 Key, UInt128 Shape)> Duck(PlanSubject.Duck leg) =>
        Captured("duckdb-plan", static raised => raised is DuckDBException or JsonException, async () => {
            await using var command = leg.Connection.CreateCommand();
            command.CommandText = $"EXPLAIN (FORMAT json) {leg.Sql}";
            await using var reader = await command.ExecuteReaderAsync();
            var payload = "[]";
            while (await reader.ReadAsync()) {
                if (reader.GetString(0) is "physical_plan") { payload = reader.GetString(1); }
            }
            using var plan = JsonDocument.Parse(payload);
            var shape = new XxHash128();
            foreach (var root in plan.RootElement.EnumerateArray()) { Shape(root, shape, DuckFacets, "children"); }
            return (Key(leg.Sql), shape.GetCurrentHashAsUInt128());
        });

    static IO<(UInt128 Key, UInt128 Shape)> Sqlite(PlanSubject.Sqlite leg) =>
        Captured("sqlite-plan", static raised => raised is SqliteException, async () => {
            await using var command = leg.Connection.CreateCommand();
            command.CommandText = $"EXPLAIN QUERY PLAN {leg.Sql}";
            await using var reader = await command.ExecuteReaderAsync();
            var shape = new XxHash128();
            while (await reader.ReadAsync()) { shape.Append(Encoding.UTF8.GetBytes(reader.GetString(3))); }
            return (Key(leg.Sql), shape.GetCurrentHashAsUInt128());
        });

    static IO<T> Captured<T>(string engine, Func<Exception, bool> recognizes, Func<Task<T>> crossing) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => Fin<T>.Succ(await crossing().ConfigureAwait(false))).ConfigureAwait(false))
            .MapFail(error => StatFault.Lift(engine, error, recognizes)))
        .Bind(IO.liftFin);

    static readonly ImmutableArray<string> PgFacets = ["Node Type", "Join Type", "Relation Name", "Index Name"];
    static readonly ImmutableArray<string> DuckFacets = ["name"];

    static void Shape(JsonElement node, XxHash128 shape, ImmutableArray<string> facets, string children) {
        foreach (string facet in facets) {
            if (node.TryGetProperty(facet, out JsonElement value)) { shape.Append(Encoding.UTF8.GetBytes(value.GetString() ?? string.Empty)); }
        }
        if (node.TryGetProperty(children, out JsonElement nested)) {
            foreach (JsonElement child in nested.EnumerateArray()) { Shape(child, shape, facets, children); }
        }
    }

    static UInt128 Key(string sql) => XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(sql));
}
```

## [07]-[HOOK_RAIL]

- Owner: `PersistencePoint` — the `[SmartEnum<string>]` point vocabulary realizing the kernel `IHookRoster` floor, each row carrying its `HookId`, its `CapabilitySet<HookModality>`, and the one trace plane this package reports under; `PersistenceFact` — the closed fact family the roster's one rail carries, realizing the kernel `IHookFact` floor so each case names the point it seats at; `PersistenceHooks` — the composition surface over the kernel `HookRail` and `HookMounts`, with the `Guarded`, `Swept`, and `Drained` adapters that fire veto and observe points without touching owner rail signatures.
- Cases: seven points — `rasm.persistence.element.append` (`Veto`), `.element.committed` (`Observe`), `.egress.delivered` (`Observe`), `.retention.sweep` (`Veto`), `.merge.conflict` (`Observe`), `.recovery.replay` (`Replay`), `.ingress.drained` (`Observe`); `PersistenceFact` closes the seven payloads those points carry.
- Entry: `PersistenceHooks.Live(key, gates, taps, span, cell)` — one fresh rail per composition whose seats mint from `PersistencePoint.Items` ALONE, so a point outside the roster is unrepresentable; `Points` — the census the composition root folds into the one frozen `HookRegistry` beside the AppHost rail's own points, structural id uniqueness across both rosters; `Mounts` — the rider seat table a plugin claims its typed ask-and-grant through.
- Auto: veto fold, observe isolation, replay depth, span bracketing, subscription rollback, detach ordering, and the bounded evidence ring all ride the kernel mechanism — a throwing or failing subscriber parks as `IsolatedFault` on the composition's one `FaultCell`, which sheds oldest-first and counts the shed rather than growing for process lifetime, so subscriber failure is hook-rail evidence and never a `StatFault` arm or a broken emitter.
- Receipt: none — a hook fire is the evidence event itself; the emitter's own receipt already carries the fact.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new point is ONE `PersistencePoint` row and one `PersistenceFact` case — the seat, the census, and the registry entry all derive from `Items`, so no `Live` body, no census literal, and no typed column moves; a subscriber is one gate or tap value at composition; a rider is one `HookBinding` through `Mounts`; a new lifecycle domain contributes its point through this roster, never a second registry type.
- Boundary: ids, modalities, and the trace plane live on the roster rows alone, so a construction literal re-spelling any of the three has no place to live; point ids ride the `rasm.<pkg>.<domain>.<point>` grammar the settled `HookId` factory admits, `persistence` the pkg segment; the owning pages fire through the composition adapters and injected taps — a hook parameter on an owner rail signature is the deleted form; the AppHost `Receipt` point already taps every message envelope this package emits, so these points carry what that tap cannot: the typed fact cases and the two veto modalities; policy engines, audit sidecars, and UI live-update legs subscribe or seat riders here without touching owner rails.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PersistencePoint : IHookRoster<PersistencePoint> {
    public static readonly PersistencePoint ElementAppend = new("rasm.persistence.element.append", HookModality.Veto);
    public static readonly PersistencePoint ElementCommitted = new("rasm.persistence.element.committed", HookModality.Observe);
    public static readonly PersistencePoint EgressDelivered = new("rasm.persistence.egress.delivered", HookModality.Observe);
    public static readonly PersistencePoint SweepEvict = new("rasm.persistence.retention.sweep", HookModality.Veto);
    public static readonly PersistencePoint MergeConflict = new("rasm.persistence.merge.conflict", HookModality.Observe);
    public static readonly PersistencePoint RecoveryReplay = new("rasm.persistence.recovery.replay", HookModality.Replay);
    public static readonly PersistencePoint IngressDrained = new("rasm.persistence.ingress.drained", HookModality.Observe);

    public HookId Id { get; }
    public CapabilitySet<HookModality> Modalities { get; }
    public Option<TraceScope> Plane => Some(StorePlane);
    static readonly TraceScope StorePlane = TraceScope.Create("rasm.persistence.store");
    private PersistencePoint(string key, params ReadOnlySpan<HookModality> modalities) : this(key) =>
        (Id, Modalities) = (HookId.Create(key), CapabilitySet<HookModality>.Of(modalities));
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PersistenceFact : IHookFact<PersistencePoint> {
    private PersistenceFact() { }
    public sealed record Append(GraphStoreOp Op) : PersistenceFact;
    public sealed record Committed(GraphReceipt Receipt) : PersistenceFact;
    public sealed record Delivered(EgressReceipt Receipt) : PersistenceFact;
    public sealed record Swept(SweepVerdict Verdict) : PersistenceFact;
    public sealed record Conflicted(ConflictReceipt Receipt) : PersistenceFact;
    public sealed record Replayed(StepFact Step) : PersistenceFact;
    public sealed record Drained(IngressReceipt Receipt) : PersistenceFact;

    public bool Seats(PersistencePoint at) => Switch(
        append:     _ => at.Equals(PersistencePoint.ElementAppend),
        committed:  _ => at.Equals(PersistencePoint.ElementCommitted),
        delivered:  _ => at.Equals(PersistencePoint.EgressDelivered),
        swept:      _ => at.Equals(PersistencePoint.SweepEvict),
        conflicted: _ => at.Equals(PersistencePoint.MergeConflict),
        replayed:   _ => at.Equals(PersistencePoint.RecoveryReplay),
        drained:    _ => at.Equals(PersistencePoint.IngressDrained));
}

public sealed record PersistenceHooks(
    HookRail<PersistencePoint, PersistenceFact, TelemetrySource> Rail,
    HookMounts<PersistencePoint, TelemetrySource> Mounts) {

    public static Fin<PersistenceHooks> Live(
        Op key,
        Seq<HookGate<PersistencePoint, PersistenceFact, TelemetrySource>> gates = default,
        Seq<HookTap<PersistencePoint, PersistenceFact, TelemetrySource>> taps = default,
        Option<IHookSpan> span = default,
        Option<FaultCell> cell = default) =>
        HookRail<PersistencePoint, PersistenceFact, TelemetrySource>
            .Of(key: key, gates: gates, taps: taps, span: span, cell: cell)
            .Map(static rail => new PersistenceHooks(rail, new HookMounts<PersistencePoint, TelemetrySource>()));

    public Seq<IHookPoint> Points => Rail.Points;
    public FaultCell Faults => Rail.Faults;

    public IO<Fin<GraphReceipt>> Guarded(IDocumentSession session, GraphStoreOp op, ProjectionContext frame, Op key, CancellationToken cancellationToken) =>
        Rail.Fire(PersistencePoint.ElementAppend, new PersistenceFact.Append(op), key, fact => Admitted<GraphStoreOp>(fact, key))
            .Match(
                Succ: admitted => GraphStore.Run(session, admitted, frame, cancellationToken)
                    .Map(outcome => outcome.Map(receipt =>
                        Rail.Fire(PersistencePoint.ElementCommitted, new PersistenceFact.Committed(receipt), key,
                            fact => Admitted<GraphReceipt>(fact, key)).IfFail(receipt))),
                Fail: error => IO.pure(Fin<GraphReceipt>.Fail(error)));

    public Seq<SweepVerdict> Swept(Seq<SweepVerdict> verdicts, Op key) =>
        verdicts.Map(verdict => verdict.Evicts
            ? Rail.Fire(PersistencePoint.SweepEvict, new PersistenceFact.Swept(verdict), key, fact => Admitted<SweepVerdict>(fact, key))
                .IfFail(_ => new SweepVerdict.Held(verdict.Key, verdict.Bytes, "hook-veto"))
            : verdict);

    public IngressReceipt Drained(IngressReceipt receipt, Op key) =>
        receipt.AtEdge > 0
            ? Rail.Fire(PersistencePoint.IngressDrained, new PersistenceFact.Drained(receipt), key, fact => Admitted<IngressReceipt>(fact, key))
                .IfFail(receipt)
            : receipt;

    static Fin<T> Admitted<T>(PersistenceFact fact, Op key) =>
        fact switch {
            PersistenceFact.Append row when row.Op is T held => Fin.Succ(held),
            PersistenceFact.Committed row when row.Receipt is T held => Fin.Succ(held),
            PersistenceFact.Swept row when row.Verdict is T held => Fin.Succ(held),
            PersistenceFact.Drained row when row.Receipt is T held => Fin.Succ(held),
            PersistenceFact.Delivered row when row.Receipt is T held => Fin.Succ(held),
            PersistenceFact.Conflicted row when row.Receipt is T held => Fin.Succ(held),
            PersistenceFact.Replayed row when row.Step is T held => Fin.Succ(held),
            _ => Fin.Fail<T>(key.InvalidInput()),
        };
}
```

## [08]-[USAGE_PROJECTION]

- Owner: `StoreUsage` — the (tenant, class, tier) usage census, the tenancy lift every partition key and every census-wire slug crosses, the census wire inverse, and the CHARGEBACK FACT residence with its projection, its reader inverse, and its durable read; `UsageReceipt` the chargeback row carrying the kernel `TenantContext` the message envelope already stamps; `UsageFactRow` the flat residence row a cost question queries.
- Entry: `StoreUsage.Fold(Seq<BlobCatalogRow> catalog, Seq<(TenantId Tenant, EgressReceipt Drain)> drains, ProjectionContext frame)` — one pure fold over the content-lineage catalog snapshot and the drain receipts; a resumed census re-folds with no journal; `StoreUsage.Decode(JsonElement payload)` — the FALLIBLE wire inverse the `#STORE_INSTRUMENTS` arm binds, so the batch re-admits through the same owner that emitted it and a malformed payload lands as a typed refusal on the arm's own rail; `StoreUsage.Dataset`/`Facts`/`Cells`/`Shape`/`Batch` — the one `AnalyticsSchema` declaration, the flat-table projection landing under `StoreUsage.FactSlot`, the cell projection in that declaration's own order, the reader inverse, and the metadata-bearing `RecordBatch` fold off the SAME declaration, so the chargeback breakdown this package's own tenancy ruling names becomes a queryable residence table and an Arrow egress instead of a receipt a reader must re-fold; `StoreUsage.Land(NpgsqlDataSource store, Seq<UsageReceipt> census, ProjectionContext frame)` — the write half through `Query/serving#SERVING_PLANE`'s one relational landing, refusing a census row the frame's tenant does not scope; `StoreUsage.Resident(ResidenceReach reach, ResidenceScope scope, Seq<(Identifier Column, string Value)> narrow)` — the durable counterpart read over that table through the one residence entry, its residence, schema, window, and frame riding the one scope value that entry takes; `StoreUsage.Tenancy` — the one lift, discriminating the typed `TenantId` partition key at the catalog and drain ingress from the slug text at the census wire, so both wire ends resolve one tenancy through one owner.
- Auto: catalog rows group under `(tenant, class, tier)` carrying the vocabulary rows themselves in the key, summing the SEALED byte figures (never a later filesystem stat) and counting objects; drain receipts fold their delivered counts onto the drain tenant's `stream`-class row — the egress obligation is event-stream custody; the census batch fans under `store.cost.usage` carrying its `rows` array, and every tenancy on either side of that wire crosses `Tenancy` exactly once, so `Partitions`, `Entry`, and `Tags` all read the kernel row rather than a page-local zero test; the fact projection carries class and tier as COLUMNS where the meter carries neither, so the breakdown a capped metric dimension cannot express is queried rather than approximated, and its schema is one `Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema` value so the residence DDL, the egress column list, and every reader's ordinals derive from one declaration.
- Receipt: `UsageReceipt` rows under `store.cost.usage` and `UsageFactRow` rows under `store.cost.fact`; the receipt stream is the EVIDENCE plane and the instrument projection is the lossy health channel, so retention class and storage tier ride the receipt, the census wire, and the fact table while the meter carries the one capped dimension.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new usage axis is one `UsageReceipt` field, one `Decode` line, one `UsageFactRow` column with its `ColumnRow`, one `Cells` arm, and one gauge row; a new source census is one `Fold` argument row.
- Boundary: tenant is the injected frame/catalog column (the RLS partition), never an ambient read, and it enters as a typed `TenantId` at the ingress boundary alone — the interior carries `TenantContext`; the kernel root row IS the absent tenant, so a single-tenant store contributes no `rasm.tenant` dimension and never a zero-valued sentinel; `TenantContext` is a plain record rather than a generated owner, so the census wire carries its two members and the decode reads the slug — the `x32` prefix the key arm mints — never a raw key scalar no `JsonElement` numeric accessor spans; the per-tenant meter dimension rides the `rasm.tenant` spelling under the estate `*`-wildcard series cap — above the cap, attribution rides receipts, the fact table, and exemplar-sampled traces, never unbounded tag values; the fact table is DERIVED and carries zero authority — it accelerates a cost question and rebuilds from the receipt stream at warm-up cost, so reading it as billing truth turns a dropped accelerator into billing loss, and the metrics-plane cardinality cap governs the meter alone while the residence holding these facts carries none by law.

```csharp
public sealed record UsageReceipt(TenantContext Tenant, Option<ArtifactKind> Kind, RetentionClass Class, StorageTier Tier, long Bytes, long Objects, long Deliveries, Instant At, CorrelationId Correlation);

public readonly record struct UsageFactRow(
    string Tenant, string Kind, string Class, string Tier, long Bytes, long Objects, long Deliveries, Instant At);

public static class StoreUsage {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.cost.usage");
    public static readonly StoreSlot FactSlot = StoreSlot.Create("store.cost.fact");

    public static TenantContext Tenancy(TenantId partition) =>
        partition == TenantContext.Root.TenantId
            ? TenantContext.Root
            : new(partition, partition.Text);

    public static TenantContext Tenancy(string slug) =>
        string.Equals(slug, TenantContext.Root.Slug, StringComparison.Ordinal)
            ? TenantContext.Root
            : new(TenantId.Of(slug), slug);

    public static Seq<UsageReceipt> Fold(Seq<BlobCatalogRow> catalog, Seq<(TenantId Tenant, EgressReceipt Drain)> drains, ProjectionContext frame) =>
        toSeq(catalog
            .GroupBy(static row => (row.Tenant, row.Kind, row.Class, row.Tier))
            .Select(group => new UsageReceipt(
                Tenancy(group.Key.Tenant), Some(group.Key.Kind), group.Key.Class, group.Key.Tier,
                group.Sum(static row => row.Bytes), group.Count(), 0L, frame.Now(), frame.Correlation)))
        + toSeq(drains
            .GroupBy(static row => row.Tenant)
            .Select(group => new UsageReceipt(
                Tenancy(group.Key), None, RetentionClass.Stream, StorageTier.Standard,
                0L, 0L, group.Sum(static row => (long)row.Drain.Delivered), frame.Now(), frame.Correlation)));

    public static Fin<Seq<UsageReceipt>> Decode(JsonElement payload) =>
        Op.Of().Catch(() => Fin.Succ(toSeq(payload.GetProperty("rows").EnumerateArray()).Strict()))
            .MapFail(static error => StatFault.Lift("usage-wire", error, WireFault))
            .Bind(rows => rows.Traverse(row => Decoded(row).ToValidation<Error>()).As().ToFin());

    static Fin<UsageReceipt> Decoded(JsonElement row) =>
        Op.Of().Catch(() => Fin.Succ(new UsageReceipt(
            Tenancy(row.GetProperty("tenant").GetProperty("slug").GetString()!),
            row.GetProperty("kind").GetString() is { } kind ? Some(ArtifactKind.Get(kind)) : None,
            RetentionClass.Get(row.GetProperty("class").GetString()!),
            StorageTier.Get(row.GetProperty("tier").GetString()!),
            row.GetProperty("bytes").GetInt64(), row.GetProperty("objects").GetInt64(),
            row.GetProperty("deliveries").GetInt64(),
            InstantPattern.ExtendedIso.Parse(row.GetProperty("at").GetString()!).Value,
            CorrelationId.Create(row.GetProperty("correlation").GetGuid()))))
        .MapFail(static error => StatFault.Lift("usage-wire", error, WireFault));

    static bool WireFault(Exception raised) =>
        raised is JsonException or InvalidOperationException or ArgumentException or KeyNotFoundException
            or NodaTime.Text.UnparsableValueException;

    public static readonly Identifier KindColumn = Identifier.Create("kind");
    public static readonly Identifier ClassColumn = Identifier.Create("class");
    public static readonly Identifier TierColumn = Identifier.Create("tier");
    public static readonly Identifier BytesColumn = Identifier.Create("bytes");
    public static readonly Identifier ObjectsColumn = Identifier.Create("objects");
    public static readonly Identifier DeliveriesColumn = Identifier.Create("deliveries");
    public static readonly Identifier AtColumn = Identifier.Create("at");

    public static readonly AnalyticsSchema Dataset = new("cost.usage",
        Seq(KindColumn, ClassColumn, TierColumn),
        Seq(new ColumnRow(KindColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(ClassColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(TierColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(BytesColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(ObjectsColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(DeliveriesColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: None);

    public static Seq<UsageFactRow> Facts(Seq<UsageReceipt> census) =>
        census.Map(static row => new UsageFactRow(
            row.Tenant.Entry, row.Kind.Match(Some: static kind => kind.Key, None: static () => string.Empty),
            row.Class.Key, row.Tier.Key, row.Bytes, row.Objects, row.Deliveries, row.At));

    public static Fin<UsageFactRow> Shape(ResidenceScope scope, ResidenceRow row) =>
        (row.Text(scope.Residence, 0), row.Text(scope.Residence, 1), row.Text(scope.Residence, 2),
            row.Whole(scope.Residence, 3), row.Whole(scope.Residence, 4), row.Whole(scope.Residence, 5),
            row.At(scope.Residence, 6))
        .Apply((kind, retention, tier, bytes, objects, deliveries, at) =>
            new UsageFactRow(scope.Frame.Tenant.Entry, kind, retention, tier, bytes, objects, deliveries, at)).As();

    public static IO<Fin<ResidenceResult<UsageFactRow>>> Resident(
        ResidenceReach reach, ResidenceScope scope, Seq<(Identifier Column, string Value)> narrow) =>
        ResidencePlan.Scan(Dataset, narrow).Match(
            Succ: plan => ResidenceRead.Read(reach, plan, scope, ResidenceProjection.Aggregate, row => Shape(scope, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<UsageFactRow>>.Fail(error)));

    public static Seq<ColumnCell> Cells(UsageFactRow row) =>
        Seq<ColumnCell>(
            new ColumnCell.Text(row.Kind), new ColumnCell.Text(row.Class), new ColumnCell.Text(row.Tier),
            new ColumnCell.Whole(row.Bytes), new ColumnCell.Whole(row.Objects),
            new ColumnCell.Whole(row.Deliveries), new ColumnCell.Moment(row.At));

    public static Fin<RecordBatch> Batch(Seq<UsageReceipt> census, ProjectionContext frame) => Batched(Facts(census), frame);

    static Fin<RecordBatch> Batched(Seq<UsageFactRow> rows, ProjectionContext frame) =>
        ArrowLanding.Build(Dataset, rows, Cells, Seq(
            ("dataset", (string)Dataset.Dataset),
            ("at", InstantPattern.ExtendedIso.Format(frame.Now())),
            ("correlation", frame.Correlation.ToString()),
            ("rows", rows.Count.ToString(CultureInfo.InvariantCulture))));

    public static IO<Fin<ResidenceIngestReceipt>> Land(NpgsqlDataSource store, Seq<UsageReceipt> census, ProjectionContext frame) =>
        census.Exists(row => row.Tenant.Entry != frame.Tenant.Entry)
            ? IO.pure(Fin<ResidenceIngestReceipt>.Fail(new ResidenceFault.IngestRefused("<tenant-scope>", Dataset.Dataset)))
            : ResidenceLanding.Stage(store, Dataset, Facts(census).Map(Cells), frame);
}
```

## [09]-[STORE_INSTRUMENTS]

- Owner: `StoreInstruments` — the Persistence instrument roster, each row CARRYING its `InstrumentSpec` so the mounted sequence derives from `Items` rather than mirroring the named fields, the instrument-name and dimension-slot vocabulary every row and arm reads, the wire-field-to-tag-value row tables each fanned dimension enumerates, the `TelemetryContributorPort` mint, the census egress, and the slot-keyed projection arms; `StoreTelemetryCensus`/`CensusRow` — the declared-truth wire record the dashboard plane compiles from.
- Cases: the kernel instrument kinds carry the roster — advised distributions for statement duration, profiled analytical time across its wall, cpu, and blocked phases, drain duration, the residence read's duration beside the rows its engine scanned, and dead-letter attempt depth whose own observation count IS the dead-letter stream; a plain distribution for profiled row counts (base2-exponential by default); counters for the embedded step tells, the pg buffer-pressure events, the egress settlement stream, the plan-capture stream, the rows a residence dataset landing stages, and the object-plane fact stream whose kind dimension spans the whole `BlobFactKind` vocabulary, refusals included; scalar levels for the pg I/O and embedded cache hit ratios; keyed levels for the embedded memory regions and for the tenant usage byte, object, and delivery census, whose root group reports the same three figures untagged on the same three instruments.
- Law: a per-tenant chargeback figure is a LEVEL and a per-asset-class footprint census is a DATASET — the meter carries the cardinality a board polls and the lake carries the product a query groups. Fanning a bounded vocabulary across a keyed level multiplies two bounded axes into a series count neither declared, so the asset-class breakdown rides `#USAGE_PROJECTION`'s landed fact table where no cardinality ceiling exists by law and an operator asks "which asset class costs what" as a GROUP BY, while the meter keeps the tenant-only figure. Bounded × bounded is still a product; the fault, sweep, and object-plane rows carry their bounded dimensions because their instruments are COUNTERS whose series are the tag product alone, never a keyed level family whose cardinality is already the tenant count.
- Entry: `StoreInstruments.Telemetry(string version)` — the contributor port peer of the AppHost host roster, carrying every row and the `#STORE_BOARD` pack over those same rows under the minted `TelemetrySource.Persistence` scope with the semconv coordinate the mint stamps as `MeterOptions.TelemetrySchemaUrl`; `StoreInstruments.Arms` — the kernel-keyed projection table the AppHost receipt fan merges beside its own through `ReceiptFan.Of`, which refuses a duplicate projection key on the rail and names every collided one; `StoreInstruments.Census(string version, SlotRegistry registry)` — the declared-truth census folding rows, kinds, bounds, tag vocabularies, mounted slots, and projected-arm keys into one wire record, so a new instrument or slot appears on the board with zero dashboard edits and a hand-listed metric name in a dashboard is the deleted form.
- Auto: rows are pure declarations, so the roster and the arm table are values a composition binds rather than per-composition constructions — both read through accessors, because a field initializer reading the generated `Items` freezes an empty roster and one reading a table declared beneath it captures null, and the bind body derives from `Kind` x `MeasureForm` at the kernel — a folder re-spelling a counter, gauge, or histogram create re-mints the mechanism; the projection subscribes as one observe row on the AppHost hook rail's receipt point, so every message envelope the sink emits projects with zero call-site metering; level-shaped facts write through the kernel `InstrumentSet.Level` gate and ride observable gauges at collection cadence, so a polled level never aliases through a synchronous gauge and a level named by no pulled row refuses rather than accumulating in a cell no reader samples; the usage arm heads its fold with the kernel `Enabled` listener gate, so a process subscribed to none of the three usage rows pays no census decode; a NodaTime `Duration` crosses the wire as its JsonRoundtrip text and `Seconds` is the one arm-side decode.
- Receipt: none — the arms project the harvest, plan, usage, and egress receipts; a metric minted beside them is a second truth, and each arm returns the kernel `Write`/`Level` rail whose refusal names the offending row rather than dropping a measurement silently.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one projected slot is one `ReceiptFan.Arm` registration and its instrument rows here, a slot whose receipt shape an existing arm already folds binds that arm's parameterized mint under its own tag value rather than a second body, and a slot family drawn from a CLOSED vocabulary lands with no edit at all — the object plane's arms derive from `BlobFactKind.Items`, so a tenth fact kind reaches the meter the moment that vocabulary grows; a further step tell, memory region, profile phase, or I/O event the harvest receipts grow is one `(wire field, tag value)` pair on its own row table — never a second instrument, because the fanned dimension already carries the axis; a slot without an `Arms` row is receipt-only by default, so projection is opt-in per row and no page declares the default; a new bucket policy is one `Buckets` row at the kernel, never a folder-local bound array; the census follows rows and slots with zero edits.
- Boundary: the port `Scope` string is the minted package row the composing root admits by name, board and reliability policy travel DOWN on that same port so the mounting root proves every descriptor inside the fold that binds the handles and never reaches a package-specific pack field by name, and instruments mount through the composing root's meter mint, never a package-local `Meter`; every level cell is the composition's kernel `LevelCells`, so no folder-shaped or process-static cell exists; pg_stat and engine-status sources are server- and process-global, so no harvest row carries a tenant tag — ONLY the usage levels carry the `rasm.tenant` dimension, capped by the one per-instrument governance view its declaring row projects and never multiplied by a class or tier product, and that dimension is the key those families MAY carry rather than one every entry holds — the root tenant's group reports untagged on the same instrument, the declaration's own absence arm, so a partitioned and an unpartitioned deployment publish one series shape a cross-deployment query unions — while every other fanned dimension closes over a vocabulary its row table enumerates or a `Query/residence#RESIDENCE_FAMILY` closed roster names — `residence` over the residence family and `dataset` over the landed residence datasets — so the whole roster's series count is declared rather than payload-driven; every tag key an arm stamps is a declared `Dimensions` entry on its row, so the governance leg derives each view's `TagKeys` from the roster; the census `Instruments` roster is this scope's alone while its `Slots` are the composition's whole mounted surface, foreign contributed families included, because a board discovering one package's streams still resolves every slot the sink emits; arm bodies are the one place receipt wire names meet instrument writes, and an arm never re-validates the payload its typed receipt already admitted.

```csharp
public sealed record CensusRow(string Name, string Kind, string Unit, string Description, ImmutableArray<double> Buckets, Seq<string> Dimensions);

public sealed record StoreTelemetryCensus(string Source, string Version, Seq<CensusRow> Instruments, Seq<string> Slots, Seq<string> Projected);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StoreInstruments {
    const string Head = "rasm.persistence.";

    public const string SinkSlot = Head + "sink";
    public const string OutcomeSlot = Head + "outcome";
    public const string EngineSlot = Head + "engine";
    public const string RuleSlot = Head + "rule";
    public const string LaneSlot = Head + "lane";
    public const string StepSlot = Head + "step";
    public const string RegionSlot = Head + "region";
    public const string PhaseSlot = Head + "phase";
    public const string EventSlot = Head + "event";
    public const string ResidenceSlot = Head + "residence";
    public const string DatasetSlot = Head + "dataset";
    public const string ProviderSlot = Head + "provider";
    public const string KindSlot = Head + "kind";
    public const string ClassSlot = Head + "class";
    public const string RouteSlot = Head + "route";

    const string DrainLane = "drain";
    const string ReplayLane = "replay";

    public const string DeliveredOutcome = "delivered";
    public const string DuplicateOutcome = "duplicate";
    public const string HeldOutcome = "held";
    public const string DeadOutcome = "dead";

    const string RelationObject = "relation";

    static StoreInstruments Row(
        string name, InstrumentKind kind, MeasureForm form, string unit, string description,
        Seq<string> dimensions, Option<Buckets> bounds, Option<string> tag, Option<int> ceiling) =>
        new(name, InstrumentSpec.Create(name, kind, form, unit, description, dimensions, bounds, tag, ceiling));

    public InstrumentSpec Spec { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Spec).Strict();

    static readonly Seq<(string Field, string Value)> StepTells = Seq(
        ("vmSteps", "vm"), ("fullScanSteps", "fullscan"), ("sorts", "sort"), ("autoIndexRows", "autoindex"));
    static readonly Seq<(string Field, string Value)> MemoryRegions = Seq(
        ("cacheBytes", "cache"), ("schemaBytes", "schema"), ("statementBytes", "statement"));
    static readonly Seq<(string Field, string Value)> ProfilePhases = Seq(
        ("latencySeconds", "wall"), ("cpuSeconds", "cpu"), ("blockedThreadSeconds", "blocked"));
    static readonly Seq<(string Field, string Value)> IoEventRows = Seq(
        ("evictions", "eviction"), ("fsyncs", "fsync"));

    public static readonly StoreInstruments StatementDuration = Row(Head + "statement.duration",
        InstrumentKind.Distribution, MeasureForm.Real, "s",
        "mean execution time per harvested top-N server statement", Seq<string>(), Some(Buckets.FoldSeconds), None, None);
    public static readonly StoreInstruments IoHitRatio = Row(Head + "io.hit.ratio",
        InstrumentKind.Level, MeasureForm.Real, "1",
        "shared-buffer hit ratio over the pg_stat_io window", Seq<string>(), None, None, None);
    public static readonly StoreInstruments IoEvents = Row(Head + "io.events",
        InstrumentKind.Count, MeasureForm.Whole, "{event}",
        "buffer evictions and fsyncs over the pg_stat_io window by event", Seq(EventSlot), None, None, None);
    public static readonly StoreInstruments DuckDuration = Row(Head + "duckdb.duration",
        InstrumentKind.Distribution, MeasureForm.Real, "s",
        "profiled analytical statement time by wall, cpu, and blocked phase", Seq(PhaseSlot), Some(Buckets.ProfileSeconds), None, None);
    public static readonly StoreInstruments DuckRows = Row(Head + "duckdb.rows",
        InstrumentKind.Distribution, MeasureForm.Whole, "{row}",
        "rows returned per profiled analytical statement", Seq<string>(), None, None, None);
    public static readonly StoreInstruments SqliteSteps = Row(Head + "sqlite.steps",
        InstrumentKind.Count, MeasureForm.Whole, "{step}",
        "embedded statement steps per harvest interval by tell", Seq(StepSlot), None, None, None);
    public static readonly StoreInstruments SqliteCacheRatio = Row(Head + "sqlite.cache.ratio",
        InstrumentKind.Level, MeasureForm.Real, "1",
        "embedded page-cache hit ratio over the sampled connection", Seq<string>(), None, None, None);
    public static readonly StoreInstruments SqliteMemory = Row(Head + "sqlite.memory",
        InstrumentKind.Levels, MeasureForm.Whole, "By",
        "embedded store bytes held by memory region", Seq<string>(), None, Some(RegionSlot), None);
    public static readonly StoreInstruments EgressDeliveries = Row(Head + "egress.deliveries",
        InstrumentKind.Count, MeasureForm.Whole, "{delivery}",
        "egress entries by sink, lane, and settlement outcome", Seq(SinkSlot, LaneSlot, OutcomeSlot), None, None, None);
    public static readonly StoreInstruments EgressDeadLetterAttempts = Row(Head + "egress.deadletter.attempts",
        InstrumentKind.Distribution, MeasureForm.Whole, "{attempt}",
        "delivery attempts per dead-lettered egress entry by sink", Seq(SinkSlot), Some(Buckets.IterationCounts), None, None);
    public static readonly StoreInstruments EgressDrainDuration = Row(Head + "egress.drain.duration",
        InstrumentKind.Distribution, MeasureForm.Real, "s",
        "wall duration per egress drain by sink and lane", Seq(SinkSlot, LaneSlot), Some(Buckets.ProfileSeconds), None, None);
    public static readonly StoreInstruments PlanCaptures = Row(Head + "plan.captures",
        InstrumentKind.Count, MeasureForm.Whole, "{capture}",
        "plan-shape captures by engine and compare verdict", Seq(EngineSlot, RuleSlot), None, None, None);

    public static readonly StoreInstruments ResidenceReadDuration = Row(Head + "residence.read.duration",
        InstrumentKind.Distribution, MeasureForm.Real, "s",
        "wall duration per residence read by residence", Seq(ResidenceSlot), Some(Buckets.ProfileSeconds), None, None);
    public static readonly StoreInstruments ResidenceScanned = Row(Head + "residence.scanned",
        InstrumentKind.Distribution, MeasureForm.Whole, "{row}",
        "rows the engine scanned per residence read by residence", Seq(ResidenceSlot), Some(Buckets.IterationCounts), None, None);
    public static readonly StoreInstruments ResidenceIngested = Row(Head + "residence.staged",
        InstrumentKind.Count, MeasureForm.Whole, "{row}",
        "rows staged per residence dataset landing", Seq(DatasetSlot), None, None, None);
    public static readonly StoreInstruments BlobFacts = Row(Head + "blob.facts",
        InstrumentKind.Count, MeasureForm.Whole, "{fact}",
        "object-plane facts by provider and fact kind", Seq(ProviderSlot, KindSlot), None, None, None);
    public static readonly StoreInstruments BlobBytes = Row(Head + "blob.bytes",
        InstrumentKind.Count, MeasureForm.Whole, "By",
        "object bytes transferred by provider and fact kind", Seq(ProviderSlot, KindSlot), None, None, None);
    public static readonly StoreInstruments BlobParts = Row(Head + "blob.parts",
        InstrumentKind.Distribution, MeasureForm.Whole, "{part}",
        "multipart parts staged per object by provider", Seq(ProviderSlot), Some(Buckets.IterationCounts), None, None);
    public static readonly StoreInstruments CoordinationFaults = Row(Head + "coordination.faults",
        InstrumentKind.Count, MeasureForm.Whole, "{fault}",
        "fenced-store refusals by numeric fault code and re-offer route", Seq(KernelInstrument.CodeSlot, RouteSlot), None, None, None);
    public static readonly StoreInstruments RetentionSwept = Row(Head + "retention.swept",
        InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
        "retention verdicts by class and deciding rule", Seq(ClassSlot, RuleSlot), None, None, None);
    public static readonly StoreInstruments UsageSize = Row(Head + "usage.size",
        InstrumentKind.Levels, MeasureForm.Whole, "By",
        "durable bytes by tenant", Seq<string>(), None, Some(TenantContext.TenantSlot), None);
    public static readonly StoreInstruments UsageObjects = Row(Head + "usage.objects",
        InstrumentKind.Levels, MeasureForm.Whole, "{object}",
        "durable objects by tenant", Seq<string>(), None, Some(TenantContext.TenantSlot), None);
    public static readonly StoreInstruments UsageDeliveries = Row(Head + "usage.deliveries",
        InstrumentKind.Levels, MeasureForm.Whole, "{delivery}",
        "egress deliveries by tenant over the usage census window", Seq<string>(), None, Some(TenantContext.TenantSlot), None);

    public static HashMap<ArmKey, InstrumentArm> Arms =>
        ArmRows.ToHashMap(static row => row.Key, static row => row.Arm);

    static readonly Seq<(ArmKey Key, InstrumentArm Arm)> ArmRows =
        toSeq(BlobFactKind.Items).Map(static row => ReceiptFan.Arm(row.Slot.ToString(), Blob(row)))
        + Seq(
            ReceiptFan.Arm(PgStatHarvest.StatementsSlot.ToString(), static (set, payload) =>
                toSeq(payload.GetProperty("rows").EnumerateArray())
                    .TraverseM(row => set.Write(StatementDuration.Spec, row.GetProperty("meanExecMs").GetDouble() / 1000d)).As()
                    .Map(static _ => unit)),
            ReceiptFan.Arm(PgStatHarvest.IoSlot.ToString(), static (set, payload) =>
                from rows in Fin.Succ(toSeq(payload.GetProperty("rows").EnumerateArray()).Strict())
                from taken in Fin.Succ(rows
                    .Filter(static row => row.GetProperty("object").GetString() == RelationObject)
                    .Fold((Hits: 0L, Reads: 0L),
                        static (sum, row) => (sum.Hits + row.GetProperty("hits").GetInt64(), sum.Reads + row.GetProperty("reads").GetInt64())))
                from _ in set.Level(IoHitRatio.Spec, taken.Hits + taken.Reads > 0L ? (double)taken.Hits / (taken.Hits + taken.Reads) : 1d)
                from done in IoEventRows.TraverseM(row => set.Write(IoEvents.Spec,
                    rows.Fold(0L, (sum, entry) => sum + entry.GetProperty(row.Field).GetInt64()),
                    InstrumentSet.Tags((EventSlot, row.Value)))).As()
                select unit),
            ReceiptFan.Arm(DuckProfileHarvest.Slot.ToString(), static (set, payload) =>
                ProfilePhases.TraverseM(row => set.Write(DuckDuration.Spec, payload.GetProperty(row.Field).GetDouble(),
                    InstrumentSet.Tags((PhaseSlot, row.Value)))).As()
                    .Bind(_ => set.Write(DuckRows.Spec, payload.GetProperty("rowsReturned").GetInt64()))),
            ReceiptFan.Arm(SqliteStatHarvest.StatementsSlot.ToString(), static (set, payload) =>
                StepTells.TraverseM(row => set.Write(SqliteSteps.Spec, payload.GetProperty(row.Field).GetInt64(),
                    InstrumentSet.Tags((StepSlot, row.Value)))).As()
                    .Map(static _ => unit)),
            ReceiptFan.Arm(SqliteStatHarvest.ConnectionSlot.ToString(), static (set, payload) =>
                from taken in Fin.Succ((Hit: payload.GetProperty("cacheHits").GetInt64(), Miss: payload.GetProperty("cacheMisses").GetInt64()))
                from _ in set.Level(SqliteCacheRatio.Spec, taken.Hit + taken.Miss > 0L ? (double)taken.Hit / (taken.Hit + taken.Miss) : 1d)
                from done in MemoryRegions.TraverseM(row => set.Level(SqliteMemory.Spec, payload.GetProperty(row.Field).GetInt64(), Some(row.Value))).As()
                select unit),
            ReceiptFan.Arm(EgressPump.DrainSlot.ToString(), Fan(DrainLane)),
            ReceiptFan.Arm(EgressPump.ReplaySlot.ToString(), Fan(ReplayLane)),
            ReceiptFan.Arm(Coordinate.FaultSlot.ToString(), static (set, payload) =>
                set.Write(CoordinationFaults.Spec, 1L, InstrumentSet.Tags(
                    (KernelInstrument.CodeSlot, payload.GetProperty("identity").GetProperty("code").GetInt32()),
                    (RouteSlot, payload.GetProperty("route").GetString())))),
            ReceiptFan.Arm(RetentionSweep.SweepSlot.ToString(), static (set, payload) =>
                from carrier in Fin.Succ(InstrumentSet.Tags((ClassSlot, payload.GetProperty("class").GetString())))
                from done in SweepOutcomes.TraverseM(row => set.Write(RetentionSwept.Spec, payload.GetProperty(row.Field).GetInt64(),
                    [.. carrier, new(RuleSlot, row.Value)])).As().Map(static _ => unit)
                select done),
            ReceiptFan.Arm(EgressPump.DeadLetterSlot.ToString(), static (set, payload) =>
                set.Write(EgressDeadLetterAttempts.Spec, payload.GetProperty("attempts").GetInt64(),
                    InstrumentSet.Tags((SinkSlot, payload.GetProperty("sink").GetString())))),
            ReceiptFan.Arm(PlanProfile.Slot.ToString(), static (set, payload) =>
                set.Write(PlanCaptures.Spec, 1L, InstrumentSet.Tags(
                    (EngineSlot, payload.GetProperty("engine").GetString()),
                    (RuleSlot, payload.GetProperty("rule").GetString())))),
            ReceiptFan.Arm(ColumnarLane.ReadSlot.ToString(), static (set, payload) =>
                from carrier in Fin.Succ(InstrumentSet.Tags((ResidenceSlot, payload.GetProperty("residence").GetString())))
                from _ in set.Write(ResidenceReadDuration.Spec, Seconds(payload.GetProperty("elapsed")), carrier)
                from done in set.Write(ResidenceScanned.Spec, payload.GetProperty("scanned").GetInt64(), carrier)
                select done),
            ReceiptFan.Arm(ColumnarLane.IngestSlot.ToString(), static (set, payload) =>
                set.Write(ResidenceIngested.Spec, payload.GetProperty("staged").GetInt64(),
                    InstrumentSet.Tags((DatasetSlot, payload.GetProperty("dataset").GetString())))),
            ReceiptFan.Arm(StoreUsage.Slot.ToString(), static (set, payload) =>
                !set.Enabled(Seq(UsageSize.Spec, UsageObjects.Spec, UsageDeliveries.Spec))
                    ? Fin.Succ(unit)
                    : StoreUsage.Decode(payload).Bind(census =>
                        toSeq(census.GroupBy(static row => row.Tenant.Key))
                            .TraverseM(group => Seq(
                                (Row: UsageSize, Value: group.Sum(static row => row.Bytes)),
                                (Row: UsageObjects, Value: group.Sum(static row => row.Objects)),
                                (Row: UsageDeliveries, Value: group.Sum(static row => row.Deliveries)))
                                .TraverseM(measure => set.Level(measure.Row, measure.Value, group.Key)).As()).As()
                            .Map(static _ => unit))));

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Persistence, Version: version, Instruments: Rows,
            Board: StoreDescriptors.Pack);

    public static StoreTelemetryCensus Census(string version, SlotRegistry registry) =>
        new(TelemetrySource.Persistence.Key, version,
            Rows.Map(static row => new CensusRow(
                row.Name, row.Kind.Key, row.Unit, row.Description, row.Bounds.IfNone([]), row.Dimensions)),
            toSeq(registry.Slots.Order(StringComparer.Ordinal)),
            toSeq(ArmRows.Choose(static row => row.Key is ArmKey.Kind kind ? Some(kind.Value) : None).Order(StringComparer.Ordinal)));

    static readonly Seq<(string Field, string Value)> SettlementOutcomes = Seq(
        ("delivered", DeliveredOutcome), ("duplicates", DuplicateOutcome),
        ("held", HeldOutcome), ("deadLettered", DeadOutcome));

    static readonly Seq<(string Field, string Value)> SweepOutcomes = Seq(
        ("kept", "kept"), ("held", "hold"), ("cooled", "cool"), ("evicted", "evict"));

    static Func<InstrumentSet, JsonElement, Fin<Unit>> Blob(BlobFactKind kind) => (set, payload) =>
        from carrier in Fin.Succ(InstrumentSet.Tags(
            (ProviderSlot, payload.GetProperty("provider").GetString()), (KindSlot, kind.Key)))
        from _facts in set.Write(BlobFacts.Spec, 1L, carrier)
        from _bytes in set.Write(BlobBytes.Spec, payload.GetProperty("bytes").GetInt64(), carrier)
        from done in payload.GetProperty("part").GetInt64() is long part && part > 0L
            ? set.Write(BlobParts.Spec, part, InstrumentSet.Tags((ProviderSlot, payload.GetProperty("provider").GetString())))
            : Fin.Succ(unit)
        select done;

    static Func<InstrumentSet, JsonElement, Fin<Unit>> Fan(string lane) => (set, payload) =>
        from carrier in Fin.Succ(InstrumentSet.Tags(
            (SinkSlot, payload.GetProperty("sink").GetString()), (LaneSlot, lane)))
        from _ in SettlementOutcomes.TraverseM(row => set.Write(EgressDeliveries.Spec, payload.GetProperty(row.Field).GetInt64(),
            [.. carrier, new(OutcomeSlot, row.Value)])).As()
        from done in set.Write(EgressDrainDuration.Spec, Seconds(payload.GetProperty("elapsed")), carrier)
        select done;

    static double Seconds(JsonElement element) =>
        DurationPattern.JsonRoundtrip.Parse(element.GetString()!).Value.TotalSeconds;
}
```

## [10]-[STORE_BOARD]

- Owner: `StoreDescriptors` — the package's one kernel `BoardPack` value binding the panel rows and reliability objectives over the `#STORE_INSTRUMENTS` roster.
- Cases: indicators over four shapes — a settlement share partitioning the egress stream on its own outcome dimension, a plan-stability share partitioning the capture stream on the `PlanRule` column, buffer-headroom saturations reading a hit ratio against a floor, and latency ceilings over the egress drain and the residence read.
- Entry: `StoreDescriptors.Pack` is the whole descriptor surface the AppHost alert rail and the deploy-plane board compile decode — `Panels` and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, every indicator series, and objective-name distinctness against the declaring port's own roster; the pack rides `#STORE_INSTRUMENTS`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; every descriptor names an instrument on the roster and every break key one of that row's declared dimensions, so a renamed instrument or a dropped dimension refuses at composition rather than rendering an empty panel; every objective omits its window, so kernel admission canonicalizes the one estate compliance default and no calendar literal lands here; burn windows, factors, severities, hold, tone, and the budget share derive from the kernel table, so no threshold is spelled here.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new board panel is one `PanelSpec` on the pack; a new reliability policy is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit at all; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: dashboards, alert provisioning, query dialects, the panel descriptor row, and the burn algebra are the kernel's and the deploy plane's — this page carries pack DATA behind the same `rasm.persistence.*` names the instruments carry and never a descriptor type, query string, board JSON, or provider type; a success share is a partition over the ONE counter its outcome dimension already fans, so the settlement share reads all four settlement rows the arm writes and `Ratio` stays reserved for genuinely independent counters; both headroom indicators are `Saturation` over a scalar level with `LevelBreach.Floor`, because a cache hit ratio breaches BELOW its bound and a counter pair no level reading can form is the one alternative shape; the top-N statement duration carries a PANEL and no objective — the harvest selects the slowest statements by total execution time, so an objective over that sample targets a population chosen for breaching and reports a fixed rate no tuning moves; the tenant usage families carry panels alone, because a chargeback census is a figure against no ceiling and a storage population has no reliability target, and their three `TenantContext.TenantSlot` break keys render on EVERY deployment now that the root group publishes untagged — an unpartitioned host draws one unbroken series under that key rather than the empty panel a partition-only write left it; the plan-stability good set derives from the `PlanRule` stability column rather than a value literal, so a fourth compare rule joins the share where the vocabulary owns it; the pack's own `Wire` column spells `persistence.census`, the provenance key that plane's closed tuple admits this projection under.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Store;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StoreDescriptors {
    public static readonly BoardPack Pack = new(
        Wire: "persistence.census",
        Panels: Seq(
            PanelSpec.Of("Server statement duration", StoreInstruments.StatementDuration.Key),
            PanelSpec.Of("Buffer hit ratio", StoreInstruments.IoHitRatio.Key),
            PanelSpec.Of("Buffer pressure", StoreInstruments.IoEvents.Key, StoreInstruments.EventSlot),
            PanelSpec.Of("Analytical time by phase", StoreInstruments.DuckDuration.Key, StoreInstruments.PhaseSlot),
            PanelSpec.Of("Analytical rows returned", StoreInstruments.DuckRows.Key),
            PanelSpec.Of("Embedded step tells", StoreInstruments.SqliteSteps.Key, StoreInstruments.StepSlot),
            PanelSpec.Of("Embedded cache ratio", StoreInstruments.SqliteCacheRatio.Key),
            PanelSpec.Of("Embedded memory by region", StoreInstruments.SqliteMemory.Key, StoreInstruments.RegionSlot),
            PanelSpec.Of("Egress settlement", StoreInstruments.EgressDeliveries.Key, PanelKind.Table,
                StoreInstruments.SinkSlot, StoreInstruments.LaneSlot, StoreInstruments.OutcomeSlot),
            PanelSpec.Of("Dead-letter attempt depth", StoreInstruments.EgressDeadLetterAttempts.Key, StoreInstruments.SinkSlot),
            PanelSpec.Of("Drain duration", StoreInstruments.EgressDrainDuration.Key, StoreInstruments.SinkSlot, StoreInstruments.LaneSlot),
            PanelSpec.Of("Plan captures", StoreInstruments.PlanCaptures.Key, PanelKind.Table,
                StoreInstruments.EngineSlot, StoreInstruments.RuleSlot),
            PanelSpec.Of("Object-plane facts", StoreInstruments.BlobFacts.Key, PanelKind.Table,
                StoreInstruments.ProviderSlot, StoreInstruments.KindSlot),
            PanelSpec.Of("Residence read duration", StoreInstruments.ResidenceReadDuration.Key, StoreInstruments.ResidenceSlot),
            PanelSpec.Of("Residence rows scanned", StoreInstruments.ResidenceScanned.Key, StoreInstruments.ResidenceSlot),
            PanelSpec.Of("Residence rows staged", StoreInstruments.ResidenceIngested.Key, StoreInstruments.DatasetSlot),
            PanelSpec.Of("Durable bytes by tenant", StoreInstruments.UsageSize.Key, TenantContext.TenantSlot),
            PanelSpec.Of("Durable objects by tenant", StoreInstruments.UsageObjects.Key, TenantContext.TenantSlot),
            PanelSpec.Of("Egress deliveries by tenant", StoreInstruments.UsageDeliveries.Key, TenantContext.TenantSlot)),
        Objectives: Seq(
            Objective.Create(
                name: "persistence.egress.settled",
                sli: new Sli.Partition(
                    Metric: StoreInstruments.EgressDeliveries.Key,
                    By: StoreInstruments.OutcomeSlot,
                    Good: Seq(StoreInstruments.DeliveredOutcome, StoreInstruments.DuplicateOutcome)),
                target: 0.999d,
                window: default),
            Objective.Create(
                name: "persistence.plan.stable",
                sli: new Sli.Partition(
                    Metric: StoreInstruments.PlanCaptures.Key,
                    By: StoreInstruments.RuleSlot,
                    Good: PlanRule.StableKeys),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "persistence.egress.latency",
                sli: new Sli.Latency(Metric: StoreInstruments.EgressDrainDuration.Key, Ceiling: Duration.FromSeconds(5), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "persistence.residence.latency",
                sli: new Sli.Latency(Metric: StoreInstruments.ResidenceReadDuration.Key, Ceiling: Duration.FromSeconds(2), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "persistence.io.headroom",
                sli: new Sli.Saturation(Metric: StoreInstruments.IoHitRatio.Key, Bound: 0.9d, Breach: LevelBreach.Floor),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "persistence.embedded.headroom",
                sli: new Sli.Saturation(Metric: StoreInstruments.SqliteCacheRatio.Key, Bound: 0.8d, Breach: LevelBreach.Floor),
                target: 0.95d,
                window: default)));
}
```

## [11]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
