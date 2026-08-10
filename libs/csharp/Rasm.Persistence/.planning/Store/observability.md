# [PERSISTENCE_STORE_OBSERVABILITY]

Engine-stat observability, the receipt-slot registry, the hook rail, and the store instrument contributor: one slot grammar names every evidence stream Persistence emits, one registry enforces uniqueness at composition, one harvest fold turns each engine's statistics surface — PostgreSQL cumulative views, DuckDB profiling output, SQLite status counters — into typed receipts, one plan-shape rail turns suspect statements into typed drift verdicts, one hook roster gives the durable lifecycle its veto/observe/replay points, one usage census turns storage truth into chargeback evidence, and one contributor projects the receipt fan into `rasm.persistence.*` instruments. Embedded engines expose no scrape surface, so the embedding process is their observer and the receipt rail is their observability.

Settled composition: `Rasm/Domain/telemetry#CAUSAL_FRAME` carries `TelemetrySource`, `CorrelationId`, `TenantId`/`TenantContext`, `ReceiptEnvelope`, and `ReceiptSinkPort`; `Rasm/Domain/telemetry#INSTRUMENT_MECHANISM` carries `Buckets`, `InstrumentKind`, `MeasureForm`, `InstrumentSpec`, `InstrumentSet`, `InstrumentArm`, `LevelCells`, `TelemetryContributorPort`, and `TelemetryIdentity`; `Rasm/Domain/telemetry#SIGNAL_CAPSULE` carries `HookPoint<TFact>`, `IHookPoint`, `HookId`, `HookModality`, `HookRegistry`, and `IsolatedFault` — all reach this S2 package as kernel S0 references, so no AppHost type crosses down. `ProjectionContext` is this package's own `Element/graph#STORE_RAIL` frame, and the receipt observe tap arrives from `Rasm.AppHost/Observability/hooks#HOOK_RAIL` at composition. One `ThinktectureJsonConverterFactory` registration carries each generated owner across a receipt wire as its key scalar, so a `[ValueObject]` or `[SmartEnum]` field decodes as a bare key while a plain record decodes as its members. Provider instrumentation subscribes at the AppHost root as four settled rows: `Npgsql.OpenTelemetry` — `AddNpgsql()` tracing and the `Npgsql` meter by name under the `AddView` posture the `NpgsqlDataSourceBuilder.Name` pool dimension keys; `OpenTelemetry.Instrumentation.EntityFrameworkCore` — `AddEntityFrameworkCoreInstrumentation` beside `AddNpgsql`, the ORM-layer command span nesting over the ADO-layer driver span, complementary never redundant, trace-only beside the `Npgsql` meter roster; `OpenTelemetry.Instrumentation.StackExchangeRedis` — `AddRedisInstrumentation(connection)` binding the cache multiplexer with the handle captured through `ConfigureRedisInstrumentation` so `AddConnection` binds the egress `RedisStream` multiplexer under one subscription, tracer-only with `Filter`/`Enrich` unset on the hot cache path; `OpenTelemetry.Instrumentation.AWS` — `AddAWSInstrumentation` on the tracer AND meter builders once, the shared `AWSSDK.Core` pipeline customizer spanning both the `AWSSDK.S3` object-store and `AWSSDK.KeyManagementService` custody clients, `SuppressDownstreamInstrumentation` set where HTTP instrumentation co-admits. Metric names are dotted `rasm.<domain>.<measure>` carrying no unit suffix, units UCUM, scope id the `TelemetrySource.Persistence` row.

## [01]-[INDEX]

- [02]-[SLOT_REGISTRY]: `store.<domain>.<verb>` grammar, the registry fold, and the page-contributed mount.
- [03]-[PG_STAT_HARVEST]: `pg_stat_statements` and `pg_stat_io` typed harvest receipts.
- [04]-[DUCKDB_PROFILE_HARVEST]: Profiling-JSON harvest off the analytical lane.
- [05]-[SQLITE_STATUS_HARVEST]: Statement and connection status counters off the raw bridge.
- [06]-[PLAN_PROFILE]: Three-engine plan-shape capture, digest baselines, and the typed drift verdict.
- [07]-[HOOK_RAIL]: `PersistencePoint` closes the `rasm.persistence.<domain>.<point>` vocabulary and `PersistenceHooks` seats it over the kernel signal capsule.
- [08]-[USAGE_PROJECTION]: (tenant, class, tier) usage census under `store.cost.usage`, its tenancy lift, and its wire inverse.
- [09]-[STORE_INSTRUMENTS]: `rasm.persistence.*` `InstrumentSpec` roster, contributor port, census egress, and receipt-projection arms.
- [10]-[STORE_BOARD]: `StoreDescriptors` binds the kernel board pack over that roster.

## [02]-[SLOT_REGISTRY]

- Owner: `StoreSlot` `[ValueObject<string>]` — the slot name under the `store.<domain>.<verb>` grammar, the verb a dotted path when one domain carries verb families; `SlotRegistry` — the composition-time catalog of every slot this package emits.
- Entry: `SlotRegistry.Mount(params ReadOnlySpan<StoreSlot> slots)` — freezes the catalog and throws on a duplicate at composition; `SlotRegistry.Mounted(params ReadOnlySpan<StoreSlot> contributed)` — the composition-root census spreading every page's contributed roster and any sibling-package family the call site supplies; `SlotRegistry.Admit(SlotRegistry registry, StoreSlot slot)` — the pre-send gate every receipt emission crosses, so an unregistered slot is a typed refusal, never a silent new stream.
- Auto: each owning page carries one `Slots` roster on its primary owner and `Mounted` spreads them, so the registry is the one census of the emitted-signal surface and discovery stops being page-by-page archaeology.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new evidence stream is one `StoreSlot` row on its owning page's roster; the grammar admits a new domain or verb with zero registry edits.
- Boundary: the slot is the `kind` argument the sink `Send` carries, so slot vocabulary and wire kind are one spelling; this page mints its own slots — `store.stat.statements`, `store.stat.io`, `store.stat.duckdb`, `store.stat.sqlite.statements`, `store.stat.sqlite.connection`, `store.stat.plan`, `store.cost.usage`, `store.cost.fact` — and every other page's slots enter as its contributed rows, so the registry owns uniqueness while each page owns its spellings; a sibling PACKAGE's family — the Fabrication `store.fabrication.<domain>.<verb>` shop-state rows (remnant inventory, fleet performance horizons, magazine slot state, capability history), each pairing a typed read and write receipt on its Fabrication owner — enters through the `Mounted` `contributed` span at composition, so a foreign family is call-site data under the same uniqueness law, never a census edit; a per-occurrence discriminant — a traversal's query case, a sink's lane — rides the receipt payload, never the slot string, so the census stays frozen while payloads vary.

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

    // EF renders each `TagWith` as its own leading `-- ` comment line, so line one names the owning slot and any
    // later line the predicate. The reader seats HERE because `StoreSlot` is this page's own vocabulary — both the
    // `Element/identity#SAVE_INTERCEPTOR_SPINE` wire tap and `#PLAN_PROFILE` read it.
    // Untagged text is non-rail traffic naming no owner, so the absent case is `None`, never a slot.
    public static Option<StoreSlot> Owned(string sql) {
        if (!sql.StartsWith("-- ", StringComparison.Ordinal)) { return None; }  // Exemption: leading-comment scan is the platform-forced statement seam
        int end = sql.IndexOf('\n', StringComparison.Ordinal);
        return TryCreate((end < 0 ? sql[3..] : sql[3..end]).Trim(), out StoreSlot slot) ? Some(slot) : None;
    }
}

public sealed record SlotRegistry(FrozenSet<string> Slots) {
    // Collision refusal names its duplicate spellings, so a composition merging a page roster with a foreign
    // family reads which slot forked rather than a bare count no operator can route.
    public static SlotRegistry Mount(params ReadOnlySpan<StoreSlot> slots) {
        var keys = slots.ToArray().Select(static slot => slot.ToString()).ToArray();
        var forked = toSeq(keys.GroupBy(static key => key, StringComparer.Ordinal))
            .Filter(static group => group.Count() > 1).Map(static group => group.Key)
            .Order(StringComparer.Ordinal).ToArray();
        return forked.Length > 0
            ? throw new InvalidOperationException($"slot-collision:{string.Join(',', forked)}")
            : new(keys.ToFrozenSet(StringComparer.Ordinal));
    }

    // Composition-root census: every page's roster spreads here, so a new page slot is one roster row and zero
    // registry edits; a sibling PACKAGE's family (the Fabrication `store.fabrication.<domain>.<verb>` shop-state
    // rows) enters through `contributed` at composition — call-site data under the same uniqueness law.
    public static SlotRegistry Mounted(params ReadOnlySpan<StoreSlot> contributed) => Mount([
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

// Harvest band over the KERNEL `Rasm.Domain.Expected` federation base, exactly as every sibling Persistence
// union realizes it: the parameterless private ctor with `Code`/`Message`/`Category` overrides, and the band
// integer derived through the `Element/graph#FAULT_TABLES` registry row. A `base(detail, NNNN)` literal is the
// form that registry deletes — it claims a decade the registry never allocated and nothing detects the clash.
[Union]
public abstract partial record StatFault : Expected, IValidationError<StatFault> {
    private StatFault() : base() { }
    public sealed record Text(string Detail) : StatFault;
    public sealed record MalformedSlot(string Slot) : StatFault;
    public sealed record SlotUnregistered(string Slot) : StatFault;
    public sealed record HarvestRefused(string Engine, string Detail) : StatFault;

    public override int Code => FaultBand.Stat + Switch(
        text:             static _ => 0,
        malformedSlot:    static _ => 1,
        slotUnregistered: static _ => 2,
        harvestRefused:   static _ => 3);

    public override string Message => Switch(
        text:             static c => c.Detail,
        malformedSlot:    static c => $"<store-slot:{c.Slot}>",
        slotUnregistered: static c => $"<store-slot-unregistered:{c.Slot}>",
        harvestRefused:   static c => $"<store-harvest:{c.Engine}:{c.Detail}>");

    public override string Category => Switch(
        text:             static _ => "Text",
        malformedSlot:    static _ => "Slot",
        slotUnregistered: static _ => "Registry",
        harvestRefused:   static _ => "Harvest");

    public static StatFault Create(string message) => new Text(message);
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

```csharp signature
public sealed record StatementStatRow(
    long QueryId, long Calls, double TotalExecMs, double MeanExecMs, long Rows,
    long SharedBlksHit, long SharedBlksRead, long WalBytes);

// `ReadBytes`/`WriteBytes`/`ExtendBytes` are the pg18 `numeric` byte columns cast to `bigint` in the select,
// `wal` object rows riding the same shape. EVERY `pg_stat_io` counter reads null where its `(backend_type,
// object, context)` triple does not track it — `wal` rows carry no `hits`, read-only contexts no `extends` —
// so every column coalesces at the select and a typed reader never meets a null.
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

    // Activity predicate sums the coalesced counters rather than testing `reads`/`writes` alone: a fully
    // buffer-resident relation reports hits with zero reads and zero writes, and a null comparison would
    // drop it silently — the row the hit-ratio level exists to read.
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
        IO.liftAsync(async () => {
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
- Boundary: the profiling switch is connection state, so the bracket sets, runs, and resets on every exit path — a lane query outside the bracket runs unprofiled at full speed; `outputPath` arrives from the configured artifact owner, resolves to a full path, escapes as a DuckDB string literal, and is deleted after decode on success or failure, so ambient temp storage and orphaned profile files are forbidden; the harvest borrows the `Query/columnar` connection and mints no second DuckDB lane; the analytical lane is process-scoped, so the receipt carries the frame's correlation and instant while tenant stays a `ProjectionContext` fact the sink's message envelope carries by ruling.

```csharp signature
public sealed record DuckOperatorRow(string Name, double TimingSeconds, long Cardinality);

public sealed record DuckProfileReceipt(
    double LatencySeconds, double CpuSeconds, long RowsReturned, long ResultSetBytes,
    double BlockedThreadSeconds, UInt128 PlanDigest, Seq<DuckOperatorRow> TopOperators,
    Instant At, CorrelationId Correlation);

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
- Boundary: the raw calls reach through the same `SqliteConnection.Handle` the provisioning engine operations already bridge, so the harvest opens no second native path and reads the same native connection the ADO surface drives; `enable_sqlite3_next_stmt` is a REGISTRY arm, not a harvest step — the walk throws on an unarmed connection AND on any handle prepared before the arm, so `Arm` leads the open ritual's capability roster ahead of the first statement and a per-call arm inside the harvest faults on the statements it exists to read; the `sqlite3_next_stmt` walk borrows each statement handle only inside the fold and holds none past it; the per-table `dbstat` space census is probe-gated, never build-assumed — `raw.sqlite3_compileoption_used` over `SQLITE_ENABLE_DBSTAT_VTAB` reads false on the plain `e_sqlite3` build and the bound provider is the `Store/provisioning#EMBEDDED_FLOOR` cipher bundle, so store-level bytes ride the `SCHEMA_USED`/`STMT_USED` gauges and the SQL `PRAGMA page_count`/`page_size` product as the standing form; the embedded store is process-scoped, so these receipts carry no tenant brand by ruling; provider-bundle facts stay engine-layer and never become Persistence vocabulary.

```csharp signature
public sealed record SqliteStatementStat(int VmSteps, int FullScanSteps, int Sorts, int AutoIndexRows);

public sealed record SqliteConnectionStat(int CacheHits, int CacheMisses, int CacheWrites, int CacheBytes, int SchemaBytes, int StatementBytes);

public static class SqliteStatHarvest {
    public static readonly StoreSlot StatementsSlot = StoreSlot.Create("store.stat.sqlite.statements");
    public static readonly StoreSlot ConnectionSlot = StoreSlot.Create("store.stat.sqlite.connection");

    // Registry arm, applied once per physical open as the LEADING `EmbeddedRitual` capability grant: the managed
    // wrapper maps a prepared statement's native pointer only while armed, so an unarmed walk throws on the
    // disabled registry AND a walk reaching a handle prepared ahead of the arm throws on the unmapped pointer —
    // arming late is a fault, never a short read. An unopened connection has no handle and nothing to arm.
    public static Unit Arm(SqliteConnection connection) =>
        connection.Handle is { } db ? fun(() => db.enable_sqlite3_next_stmt(true))() : unit;

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
        // Exemption: the raw handle walk is the platform-forced statement seam — an exhausted walk returns a
        // NULL statement, never a sentinel handle, so the property pattern is the terminating test and an
        // `IsInvalid` read on the returned value dereferences null.
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

// Compare-outcome vocabulary: `Stable` is the column a plan-stability share partitions the capture stream on,
// so the good half derives from the row set and a fourth compare rule joins the objective with no edit at the
// `#STORE_INSTRUMENTS` pack — a good-value literal spelled beside the tag would fork that share on the next row.
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

    // Every arm carries its statement, so the owning-slot read is one projection rather than three call sites.
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

// --- [MODELS] ---------------------------------------------------------------------------
// Statement-identity baseline persists in the relational identity tier: pg `queryid` when the
// server computes one, else the invariant hash of the statement text — one identity axis per engine.
public sealed record PlanBaselineRow(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Option<StoreSlot> Owner, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// `Verdict` carries no polymorphic annotations, so the wire crossing is the flattened `Rule` row the
// `#STORE_INSTRUMENTS` arm tags on and the shapes the receipt already names.
public sealed record PlanReceipt(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Option<StoreSlot> Owner, PlanVerdict Verdict, Instant At, CorrelationId Correlation) {
    public PlanRule Rule => Verdict.Rule;
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class PlanProfile {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.plan");

    // One capture entry over the closed subject family: leg -> shape digest -> baseline compare -> typed
    // verdict; a first sighting persists through `baseline` and reads Baselined, never a silent implicit write.
    public static IO<PlanReceipt> Capture(PlanSubject subject, Func<PlanEngine, UInt128, IO<Option<PlanBaselineRow>>> held, Func<PlanBaselineRow, IO<Unit>> baseline, ProjectionContext frame) =>
        from captured in subject.Switch(postgres: Postgres, duck: Duck, sqlite: Sqlite)
        let owner = StoreSlot.Owned(subject.Sql)   // the digested shape traces to the op that issued it, never an ownerless plan
        from prior in held(subject.Engine, captured.Key)
        from verdict in prior.Match(
            Some: row => IO.pure<PlanVerdict>(row.Shape == captured.Shape
                ? new PlanVerdict.Unchanged(captured.Shape)
                : new PlanVerdict.Drifted(row.Shape, captured.Shape)),
            None: () => baseline(new PlanBaselineRow(subject.Engine, captured.Key, captured.Shape, owner, frame.Now()))
                .Map(_ => (PlanVerdict)new PlanVerdict.Baselined(captured.Shape)))
        select new PlanReceipt(subject.Engine, captured.Key, captured.Shape, owner, verdict, frame.Now(), frame.Correlation);

    // Pg leg: EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) yields one json scalar carrying ONE array entry PER
    // statement, so the fold digests every entry in order — a head-only read makes two statements whose first
    // plans match read Unchanged after the second flipped its join order, and it turns the empty-document
    // fallback into an index throw. The facets read node kind, join type, relation, and index recursively
    // over "Plans" — never a timing or row-count value.
    static IO<(UInt128 Key, UInt128 Shape)> Postgres(PlanSubject.Postgres leg) =>
        IO.liftAsync(async () => {
            await using var command = leg.Source.CreateCommand($"EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON) {leg.Sql}");
            using var plan = JsonDocument.Parse((string?)await command.ExecuteScalarAsync() ?? "[]");
            var shape = new XxHash128();
            foreach (var entry in plan.RootElement.EnumerateArray()) {  // Exemption: hashing kernel over the plan tree
                if (entry.TryGetProperty("Plan", out var root)) { PgShape(root, shape); }
            }
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

    // Declared facet list: a richer pg shape facet is one row here, and hoisting it off the walk keeps a deep
    // plan tree from re-allocating the vocabulary at every visited node.
    static readonly ImmutableArray<string> PgFacets = ["Node Type", "Join Type", "Relation Name", "Index Name"];

    static void PgShape(JsonElement node, XxHash128 shape) {
        foreach (var facet in PgFacets) {  // Exemption: hashing kernel over the plan tree
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

- Owner: `PersistencePoint` — the `[SmartEnum<string>]` point vocabulary carrying the kernel `HookModality` column; `PersistenceHooks` — the folder's typed point roster seating one kernel point per row, with the `Guarded` and `Swept` composition adapters that fire veto points without touching owner rail signatures.
- Cases: six points — `rasm.persistence.element.append` (`Veto` over `GraphStoreOp`), `rasm.persistence.element.committed` (`Observe` over `GraphReceipt`), `rasm.persistence.egress.delivered` (`Observe` over `EgressReceipt`), `rasm.persistence.retention.sweep` (`Veto` over `SweepVerdict`), `rasm.persistence.merge.conflict` (`Observe` over `ConflictReceipt`), `rasm.persistence.recovery.replay` (`Replay` over `StepFact`).
- Entry: `PersistenceHooks.Live()` — one fresh roster per composition, seating one kernel point per `PersistencePoint` row so two apps never share a mount; `Points` — the census the composition root folds into the one frozen `HookRegistry` beside the AppHost rail's own points, structural id uniqueness across both rosters.
- Auto: veto fold, observe isolation, and replay depth ride the settled `HookPoint<TFact>` capsule; a throwing or failing subscriber parks as `IsolatedFault` on the roster's evidence cell — subscriber failure is hook-rail evidence, never a `StatFault` arm and never a broken emitter.
- Receipt: none — a hook fire is the evidence event itself; the emitter's own receipt already carries the fact.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new point is one `PersistencePoint` row, one typed field with its `Live()` seat, and one `Points` row; a subscriber is one `Observe`/`Veto` call at composition; a new lifecycle domain contributes its point through this roster, never a second registry type.
- Boundary: ids and modalities live on the roster rows alone, so a `Live()` seat re-spelling either is the forked-vocabulary defect; point ids ride the `rasm.<pkg>.<domain>.<point>` grammar the settled `HookId` factory admits, `persistence` the pkg segment; the owning pages fire through the composition adapters and injected taps — a hook parameter on an owner rail signature is the deleted form; the AppHost `Receipt` point already taps every message envelope this package emits, so these points carry what that tap cannot: the TYPED facts and the two veto modalities; policy engines, audit sidecars, and UI live-update legs subscribe here without touching owner rails.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
// Point roster keyed rasm.persistence.<domain>.<point> — the kernel HookId four-segment grammar. Modality is
// that kernel column deciding veto admission and replay retention, so id and delivery semantics belong to the
// row and a `Live()` seat re-spelling either forks the vocabulary a construction literal would own.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PersistencePoint {
    public static readonly PersistencePoint ElementAppend = new("rasm.persistence.element.append", modality: HookModality.Veto);
    public static readonly PersistencePoint ElementCommitted = new("rasm.persistence.element.committed", modality: HookModality.Observe);
    public static readonly PersistencePoint EgressDelivered = new("rasm.persistence.egress.delivered", modality: HookModality.Observe);
    public static readonly PersistencePoint SweepEvict = new("rasm.persistence.retention.sweep", modality: HookModality.Veto);
    public static readonly PersistencePoint MergeConflict = new("rasm.persistence.merge.conflict", modality: HookModality.Observe);
    public static readonly PersistencePoint RecoveryReplay = new("rasm.persistence.recovery.replay", modality: HookModality.Replay);
    // CDC fires at BOTH ends: `EgressDelivered` over an `EgressReceipt`, this point over the ingress pump's
    // end-of-partition edge — the lane's one idle signal (`Version/ingress#INGRESS_PUMP`). Without it a stalled
    // inbound lane and a drained one look identical to every subscriber.
    public static readonly PersistencePoint IngressDrained = new("rasm.persistence.ingress.drained", modality: HookModality.Observe);

    public HookModality Modality { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------
// Folder hook roster seats one typed point per composition per `PersistencePoint` row over the kernel capsule.
// Subscriber faults park as `IsolatedFault` on the roster cell; the `StatFault` band stays the harvest rail's.
public sealed record PersistenceHooks(
    HookPoint<GraphStoreOp> ElementAppend,
    HookPoint<GraphReceipt> ElementCommitted,
    HookPoint<EgressReceipt> EgressDelivered,
    HookPoint<SweepVerdict> SweepEvict,
    HookPoint<ConflictReceipt> MergeConflict,
    HookPoint<StepFact> RecoveryReplay,
    HookPoint<IngressReceipt> IngressDrained,
    Atom<Seq<IsolatedFault>> Faults) {

    public static PersistenceHooks Live() {
        Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
        return new(
            Seat<GraphStoreOp>(PersistencePoint.ElementAppend, faults),
            Seat<GraphReceipt>(PersistencePoint.ElementCommitted, faults),
            Seat<EgressReceipt>(PersistencePoint.EgressDelivered, faults),
            Seat<SweepVerdict>(PersistencePoint.SweepEvict, faults),
            Seat<ConflictReceipt>(PersistencePoint.MergeConflict, faults),
            Seat<StepFact>(PersistencePoint.RecoveryReplay, faults),
            Seat<IngressReceipt>(PersistencePoint.IngressDrained, faults),
            faults);
    }

    private static HookPoint<TFact> Seat<TFact>(PersistencePoint row, Atom<Seq<IsolatedFault>> faults) =>
        new(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults);

    // Census folds into the one frozen `HookRegistry` beside the AppHost rail's own
    // points — one audit table per composition, duplicate ids structurally fatal.
    public Seq<IHookPoint> Points => Seq<IHookPoint>(
        ElementAppend, ElementCommitted, EgressDelivered, SweepEvict, MergeConflict, RecoveryReplay, IngressDrained);

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

    // Ingress seam fires on the EDGE, not on every pump turn: the point's name claims the lane reached lag zero,
    // so a receipt whose `AtEdge` counted no end-of-partition position states nothing and fires nothing. Firing
    // unconditionally would publish an idle claim on a lane still draining, which is the fact the counter exists
    // to separate. Same composition-root decoration as the append and sweep seams — no hook parameter on the pump.
    public IngressReceipt Drained(IngressReceipt receipt) =>
        receipt.AtEdge > 0 ? IngressDrained.Fire(receipt).IfFail(receipt) : receipt;
}
```

## [08]-[USAGE_PROJECTION]

- Owner: `StoreUsage` — the (tenant, class, tier) usage census, the tenancy lift every partition key and every census-wire slug crosses, the census wire inverse, and the CHARGEBACK FACT residence with its projection, its reader inverse, and its durable read; `UsageReceipt` the chargeback row carrying the kernel `TenantContext` the message envelope already stamps; `UsageFactRow` the flat residence row a cost question queries.
- Entry: `StoreUsage.Fold(Seq<BlobCatalogRow> catalog, Seq<(TenantId Tenant, EgressReceipt Drain)> drains, ProjectionContext frame)` — one pure fold over the content-lineage catalog snapshot and the drain receipts; a resumed census re-folds with no journal; `StoreUsage.Decode(JsonElement payload)` — the FALLIBLE wire inverse the `#STORE_INSTRUMENTS` arm binds, so the batch re-admits through the same owner that emitted it and a malformed payload lands as a typed refusal on the arm's own rail; `StoreUsage.Dataset`/`Facts`/`Cells`/`Shape` — the one `AnalyticsSchema` declaration, the flat-table projection landing under `StoreUsage.FactSlot`, the cell projection in that declaration's own order, and the reader inverse, so the chargeback breakdown this package's own tenancy ruling names becomes a queryable residence table instead of a receipt a reader must re-fold; `StoreUsage.Land(NpgsqlDataSource store, Seq<UsageReceipt> census, ProjectionContext frame)` — the write half through `Query/columnar#ANALYTICS_RESIDENCE`'s one relational landing, refusing a census row the frame's tenant does not scope; `StoreUsage.Resident(ResidenceReach reach, ResidenceScope scope, Seq<(Identifier Column, string Value)> narrow)` — the durable counterpart read over that table through the one residence entry, its residence, schema, window, and frame riding the one scope value that entry takes; `StoreUsage.Tenancy` — the one lift, discriminating the typed `TenantId` partition key at the catalog and drain ingress from the slug text at the census wire, so both wire ends resolve one tenancy through one owner.
- Auto: catalog rows group under `(tenant, class, tier)` carrying the vocabulary rows themselves in the key, summing the SEALED byte figures (never a later filesystem stat) and counting objects; drain receipts fold their delivered counts onto the drain tenant's `stream`-class row — the egress obligation is event-stream custody; the census batch fans under `store.cost.usage` carrying its `rows` array, and every tenancy on either side of that wire crosses `Tenancy` exactly once, so `Partitions`, `Entry`, and `Tags` all read the kernel row rather than a page-local zero test; the fact projection carries class and tier as COLUMNS where the meter carries neither, so the breakdown a capped metric dimension cannot express is queried rather than approximated, and its schema is one `Query/columnar#ANALYTICS_RESIDENCE` `AnalyticsSchema` value so the residence DDL, the egress column list, and every reader's ordinals derive from one declaration.
- Receipt: `UsageReceipt` rows under `store.cost.usage` and `UsageFactRow` rows under `store.cost.fact`; the receipt stream is the EVIDENCE plane and the instrument projection is the lossy health channel, so retention class and storage tier ride the receipt, the census wire, and the fact table while the meter carries the one capped dimension.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new usage axis is one `UsageReceipt` field, one `Decode` line, one `UsageFactRow` column with its `ColumnRow`, one `Cells` arm, and one gauge row; a new source census is one `Fold` argument row.
- Boundary: tenant is the injected frame/catalog column (the RLS partition), never an ambient read, and it enters as a typed `TenantId` at the ingress boundary alone — the interior carries `TenantContext`; the kernel root row IS the absent tenant, so a single-tenant store contributes no `rasm.tenant` dimension and never a zero-valued sentinel; `TenantContext` is a plain record rather than a generated owner, so the census wire carries its two members and the decode reads the slug — the `x32` prefix the key arm mints — never a raw key scalar no `JsonElement` numeric accessor spans; the per-tenant meter dimension rides the `rasm.tenant` spelling under the estate `*`-wildcard series cap — above the cap, attribution rides receipts, the fact table, and exemplar-sampled traces, never unbounded tag values; the fact table is DERIVED and carries zero authority — it accelerates a cost question and rebuilds from the receipt stream at warm-up cost, so reading it as billing truth turns a dropped accelerator into billing loss, and the metrics-plane cardinality cap governs the meter alone while the residence holding these facts carries none by law.

```csharp signature
// Chargeback bytes/objects fold from the `Store/blobstore#BLOB_GC` `BlobCatalogRow` census,
// deliveries from the egress drain receipts under the drain frame's tenant.
// `Kind` carries the ASSET-CLASS axis the retention class alone cannot answer — an asset estate asks "which asset
// class costs what" first, and the catalog row holds it one hop upstream. `Option` covers the drain half of this
// census, which counts DELIVERIES with no stored asset behind them: an empty cell states that absence, where
// minting a kind for an event stream would answer a question about an artifact nobody stored.
public sealed record UsageReceipt(TenantContext Tenant, Option<ArtifactKind> Kind, RetentionClass Class, StorageTier Tier, long Bytes, long Objects, long Deliveries, Instant At, CorrelationId Correlation);

// Flat chargeback fact: the class-by-tier breakdown the capped meter dimension cannot carry, landed as one
// residence row per (tenant, class, tier, census instant) so a cost question is a query rather than a receipt
// re-fold. `Tenant` is the routing key a multi-tenant census splits on before the write, never a stored
// column — the residence owns that column at the key type its own tenancy predicate compares against.
public readonly record struct UsageFactRow(
    string Tenant, string Kind, string Class, string Tier, long Bytes, long Objects, long Deliveries, Instant At);

public static class StoreUsage {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.cost.usage");
    public static readonly StoreSlot FactSlot = StoreSlot.Create("store.cost.fact");

    // One lift over two call shapes: the typed key at the catalog and drain ingress, the slug at the census
    // wire. Each arm tests the kernel root row's OWN sentinel — `Partitions` reads false and `Tags` empty
    // there — so a single-tenant store reads single-tenant downstream with no page-local zero test, and both
    // arms ride the kernel `Text`/`Of` inverse, the same text the blob prefix and the RLS predicate spell.
    public static TenantContext Tenancy(TenantId partition) =>
        partition == TenantContext.Root.TenantId
            ? TenantContext.Root
            : new(partition, partition.Text);

    public static TenantContext Tenancy(string slug) =>
        string.Equals(slug, TenantContext.Root.Slug, StringComparison.Ordinal)
            ? TenantContext.Root
            : new(TenantId.Of(slug), slug);

    // One census fold: the group key carries the class and tier ROWS, so each receipt reads its vocabulary
    // off the key and no group head is re-read to recover what the key already discriminated on.
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

    // Wire inverse of the batch `Fold` emits, reading each field back through the shape its own type writes:
    // `class` and `tier` are generated owners and land as bare key scalars, `tenant` is a plain record and
    // lands as its members, and the instant and correlation ride the payload — so the projection arm
    // reconstructs the census whole rather than stamping a sentinel frame over evidence the batch carried.
    // Correlation re-enters through the kernel factory: `CorrelationId` declares
    // `ConversionFromKeyMemberType = None`, so no `Guid`-to-`CorrelationId` operator exists to bind a raw scalar.
    // `Decode` ADMITS a foreign payload: a missing property, a member at the wrong JSON kind, an unminted
    // vocabulary key, and an unparsable instant each throw out of the walk, so the whole reconstruction rides one
    // capture funnel and lands as a typed refusal the arm binds. `.Strict()` forces the run INSIDE the funnel,
    // where a lazy projection defers every throw past it.
    public static Fin<Seq<UsageReceipt>> Decode(JsonElement payload) =>
        Try.lift(() => toSeq(payload.GetProperty("rows").EnumerateArray()).Map(static row => new UsageReceipt(
            Tenancy(row.GetProperty("tenant").GetProperty("slug").GetString()!),
            row.GetProperty("kind").GetString() is { } kind ? Some(ArtifactKind.Get(kind)) : None,
            RetentionClass.Get(row.GetProperty("class").GetString()!),
            StorageTier.Get(row.GetProperty("tier").GetString()!),
            row.GetProperty("bytes").GetInt64(), row.GetProperty("objects").GetInt64(),
            row.GetProperty("deliveries").GetInt64(),
            InstantPattern.ExtendedIso.Parse(row.GetProperty("at").GetString()!).Value,
            CorrelationId.Create(row.GetProperty("correlation").GetGuid()))).Strict())
        .Run()
        .MapFail(static error => new StatFault.HarvestRefused("usage-wire", error.Message));

    public static readonly Identifier KindColumn = Identifier.Create("kind");
    public static readonly Identifier ClassColumn = Identifier.Create("class");
    public static readonly Identifier TierColumn = Identifier.Create("tier");
    public static readonly Identifier BytesColumn = Identifier.Create("bytes");
    public static readonly Identifier ObjectsColumn = Identifier.Create("objects");
    public static readonly Identifier DeliveriesColumn = Identifier.Create("deliveries");
    public static readonly Identifier AtColumn = Identifier.Create("at");

    // CHARGEBACK RESIDENCE: the class-by-tier breakdown a capped meter dimension cannot carry, declared once
    // so `Query/columnar#ANALYTICS_RESIDENCE`'s DDL emitter, the egress column list, and every reader's
    // ordinals derive from one value. Key is `(class, tier)` under the census instant, so a tenant's history
    // reads as one contiguous granule range and the residence expires on the same axis it partitions. NO
    // measure: three independent magnitudes ride these rows, electing one for a rollup would answer a question
    // nobody asked, and a cost table is queried rather than averaged — so the Series arm provisions the
    // hypertable, its columnstore, and its retention and emits no continuous aggregate.
    // `kind` LEADS the sort key because the asset-class question is the one this table exists to answer and a
    // leading segment is the only one a granule prune reads; `class` and `tier` follow as the coarser axes a
    // chargeback roll-up groups on after it.
    public static readonly AnalyticsSchema Dataset = new("cost.usage",
        Seq(KindColumn, ClassColumn, TierColumn),
        Seq(new ColumnRow(KindColumn, ColumnType.Utf8, Nullable: true),
            new ColumnRow(ClassColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(TierColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(BytesColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(ObjectsColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(DeliveriesColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: None);

    // Flat projection under `FactSlot`: one residence row per census row, tenant riding as the ROUTING key a
    // landing splits on rather than as a stored column, because each residence owns its tenant column at the
    // key type its own tenancy predicate compares against and a second one at a second type breaks exactly
    // that granule pruning the leading sort key exists for.
    public static Seq<UsageFactRow> Facts(Seq<UsageReceipt> census) =>
        census.Map(static row => new UsageFactRow(
            row.Tenant.Entry, row.Kind.Match(Some: static kind => kind.Key, None: static () => string.Empty),
            row.Class.Key, row.Tier.Key, row.Bytes, row.Objects, row.Deliveries, row.At));

    // Reader inverse over the one row surface every reach yields: ordinals read off `Dataset`'s declaration
    // through the plan's projected names, and the tenant returns from the frame the read scoped with — the
    // only tenant a tenant-scoped scan can have returned. Every column declares `Nullable: false`, so the six
    // reads compose as one applicative product and an empty cell refuses rather than billing a zero.
    public static Fin<UsageFactRow> Shape(ResidenceScope scope, ResidenceRow row) =>
        (row.Text(scope.Residence, 0), row.Text(scope.Residence, 1), row.Text(scope.Residence, 2),
            row.Whole(scope.Residence, 3), row.Whole(scope.Residence, 4), row.Whole(scope.Residence, 5),
            row.At(scope.Residence, 6))
        .Apply((kind, retention, tier, bytes, objects, deliveries, at) =>
            new UsageFactRow(scope.Frame.Tenant.Entry, kind, retention, tier, bytes, objects, deliveries, at)).As();

    // DURABLE counterpart to the live census: the same rows `Facts` lands, read back through the ONE residence
    // entry, so a cost question answers after the process holding the receipts is gone and the in-process fold
    // stays the warm path rather than the only path. Shape rides the plan and scope rides one `ResidenceScope`
    // value, so a narrowing is a `(column, value)` row and no consumer writes SQL against the fact table. Plan
    // assembly is fallible — a column the roster omits and a value its declared type cannot render as a plan
    // literal each refuse here, ahead of any statement.
    public static IO<Fin<ResidenceResult<UsageFactRow>>> Resident(
        ResidenceReach reach, ResidenceScope scope, Seq<(Identifier Column, string Value)> narrow) =>
        ResidencePlan.Scan(Dataset, narrow).Match(
            Succ: plan => ResidenceRead.Read(reach, plan, scope, ResidenceProjection.Aggregate, row => Shape(scope, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<UsageFactRow>>.Fail(error)));

    // Cell projection in `Dataset`'s own declaration order, so the fact row, the DDL column list, the COPY
    // roster, and `Shape`'s read ordinals all move together on one column insert. Tenancy is absent by the same
    // law the reader inverse states: the residence owns that column at its own key type while the landing
    // scopes a whole batch with the frame's tenant.
    public static Seq<ResidenceCell> Cells(UsageFactRow row) =>
        Seq<ResidenceCell>(
            new ResidenceCell.Text(row.Kind), new ResidenceCell.Text(row.Class), new ResidenceCell.Text(row.Tier),
            new ResidenceCell.Whole(row.Bytes), new ResidenceCell.Whole(row.Objects),
            new ResidenceCell.Whole(row.Deliveries), new ResidenceCell.Moment(row.At));

    // DURABLE half of the chargeback plane: the census this owner already folds lands on the Series tier
    // through the ONE residence landing, so the fact table a cost question queries is FED by the same fold that
    // emits the census receipts rather than provisioned, readable, and empty. A row carrying a tenant the frame
    // did not scope refuses here — one COPY lands under one tenant by construction, so a multi-tenant census
    // splits BEFORE the write and never silently reattributes another tenant's bytes.
    public static IO<Fin<ResidenceIngestReceipt>> Land(NpgsqlDataSource store, Seq<UsageReceipt> census, ProjectionContext frame) =>
        census.Exists(row => row.Tenant.Entry != frame.Tenant.Entry)
            ? IO.pure(Fin<ResidenceIngestReceipt>.Fail(new ResidenceFault.IngestRefused("<tenant-scope>", Dataset.Dataset)))
            : ResidenceLanding.Stage(store, Dataset, Facts(census).Map(Cells), frame);
}
```

## [09]-[STORE_INSTRUMENTS]

- Owner: `StoreInstruments` — the Persistence `InstrumentSpec` roster, the instrument-name and dimension-slot vocabulary every row and arm reads, the wire-field-to-tag-value row tables each fanned dimension enumerates, the `TelemetryContributorPort` mint, the census egress, and the slot-keyed projection arms; `StoreTelemetryCensus`/`CensusRow` — the declared-truth wire record the dashboard plane compiles from.
- Cases: the kernel instrument kinds carry the roster — advised distributions for statement duration, profiled analytical time across its wall, cpu, and blocked phases, drain duration, the residence read's duration beside the rows its engine scanned, and dead-letter attempt depth whose own observation count IS the dead-letter stream; a plain distribution for profiled row counts (base2-exponential by default); counters for the embedded step tells, the pg buffer-pressure events, the egress settlement stream, the plan-capture stream, and the rows a residence dataset landing stages; scalar levels for the pg I/O and embedded cache hit ratios; keyed levels for the embedded memory regions and for the tenant usage byte, object, and delivery census, whose root group reports the same three figures untagged on the same three instruments.
- Law: a per-tenant chargeback figure is a LEVEL and a per-asset-class footprint census is a DATASET — the meter carries the cardinality a board polls and the lake carries the product a query groups. Fanning a bounded vocabulary across a keyed level multiplies two bounded axes into a series count neither declared, so the asset-class breakdown rides `#USAGE_PROJECTION`'s landed fact table where no cardinality ceiling exists by law and an operator asks "which asset class costs what" as a GROUP BY, while the meter keeps the tenant-only figure. Bounded × bounded is still a product; the fault, sweep, and object-plane rows carry their bounded dimensions because their instruments are COUNTERS whose series are the tag product alone, never a keyed level family whose cardinality is already the tenant count.
- Entry: `StoreInstruments.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the contributor port peer of the AppHost host roster, carrying every row and the `#STORE_BOARD` pack over those same rows under the minted `TelemetrySource.Persistence` scope with the semconv coordinate the mint stamps as `MeterOptions.TelemetrySchemaUrl`; `StoreInstruments.Arms` — the slot-keyed projection table the AppHost receipt fan mounts beside its own arms at the composition root; `StoreInstruments.Census(string version, SlotRegistry registry)` — the declared-truth census folding rows, kinds, bounds, tag vocabularies, mounted slots, and projected-arm keys into one wire record, so a new instrument or slot appears on the board with zero dashboard edits and a hand-listed metric name in a dashboard is the deleted form.
- Auto: rows are pure declarations, so the roster and the arm table are static values a composition binds rather than per-composition constructions, and the bind body derives from `Kind` x `MeasureForm` at the kernel — a folder re-spelling a counter, gauge, or histogram create re-mints the mechanism; the projection subscribes as one observe row on the AppHost hook rail's receipt point, so every message envelope the sink emits projects with zero call-site metering; level-shaped facts write through the kernel `InstrumentSet.Level` gate and ride observable gauges at collection cadence, so a polled level never aliases through a synchronous gauge and a level named by no pulled row refuses rather than accumulating in a cell no reader samples; the usage arm heads its fold with the kernel `Enabled` listener gate, so a process subscribed to none of the three usage rows pays no census decode; a NodaTime `Duration` crosses the wire as its JsonRoundtrip text and `Seconds` is the one arm-side decode.
- Receipt: none — the arms project the harvest, plan, usage, and egress receipts; a metric minted beside them is a second truth, and each arm returns the kernel `Write`/`Level` rail whose refusal names the offending row rather than dropping a measurement silently.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one projected slot is one `Arms` row and its instrument rows here, and a slot whose receipt shape an existing arm already folds binds that arm's parameterized mint under its own tag value rather than a second body; a further step tell, memory region, profile phase, or I/O event the harvest receipts grow is one `(wire field, tag value)` pair on its own row table — never a second instrument, because the fanned dimension already carries the axis; a slot without an `Arms` row is receipt-only by default, so projection is opt-in per row and no page declares the default; a new bucket policy is one `Buckets` row at the kernel, never a folder-local bound array; the census follows rows and slots with zero edits.
- Boundary: the port `Scope` string is the minted package row the composing root admits by name, board and reliability policy travel DOWN on that same port so the mounting root proves every descriptor inside the fold that binds the handles and never reaches a package-specific pack field by name, and instruments mount through the composing root's meter mint, never a package-local `Meter`; every level cell is the composition's kernel `LevelCells`, so no folder-shaped or process-static cell exists; pg_stat and engine-status sources are server- and process-global, so no harvest row carries a tenant tag — ONLY the usage levels carry the `rasm.tenant` dimension, capped by the one per-instrument governance view its declaring row projects and never multiplied by a class or tier product, and that dimension is the key those families MAY carry rather than one every entry holds — the root tenant's group reports untagged on the same instrument, the declaration's own absence arm, so a partitioned and an unpartitioned deployment publish one series shape a cross-deployment query unions — while every other fanned dimension closes over a vocabulary its row table enumerates or a `Query/columnar#ANALYTICS_RESIDENCE` closed roster names — `residence` over the residence family and `dataset` over the landed residence datasets — so the whole roster's series count is declared rather than payload-driven; every tag key an arm stamps is a declared `Dimensions` entry on its row, so the governance leg derives each view's `TagKeys` from the roster; the census `Instruments` roster is this scope's alone while its `Slots` are the composition's whole mounted surface, foreign contributed families included, because a board discovering one package's streams still resolves every slot the sink emits; arm bodies are the one place receipt wire names meet instrument writes, and an arm never re-validates the payload its typed receipt already admitted.

```csharp signature
// Census wire pair compiles each instrument row with its kind, declared bounds, and tag vocabulary beside the
// mounted slot census and the projected-arm keys — declared truth, never a hand-listed metric name.
public sealed record CensusRow(string Name, string Kind, string Unit, string Description, ImmutableArray<double> Buckets, Seq<string> Dimensions);

public sealed record StoreTelemetryCensus(string Source, string Version, Seq<CensusRow> Instruments, Seq<string> Slots, Seq<string> Projected);

public static class StoreInstruments {
    // One head for the package's whole vocabulary: every measure name and every dimension key concatenates it
    // at compile time, so the namespace is stated once and no row drifts off the dotted grammar.
    const string Head = "rasm.persistence.";

    // Dimension vocabulary: every tag key a row declares and an arm stamps reads one const, so the governance
    // view derives its TagKeys from the roster and no write site re-spells a dimension. Keys carry the package
    // head because `outcome`, `lane`, and `phase` are concepts several packages tag and a bare noun collides
    // on the second.
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
    // Object-plane and retention axes. `CategorySlot` keys the fault dimension whose VALUES are the
    // `Store/blobstore#OBJECT_STORE` `RemoteStoreFault.Category` projection — the union publishes its own bounded
    // vocabulary, so no second fault roster drifts beside it; `ClassSlot` keys the retention class, bounded by the
    // six-row `Version/retention` axis and never by the finer asset kind, which is a lake question (see the `- Law:`).
    public const string ProviderSlot = Head + "provider";
    public const string VerbSlot = Head + "verb";
    public const string CategorySlot = Head + "category";
    public const string ClassSlot = Head + "class";

    // Lane values close the egress fan: the drain and the letter replay emit the identical `EgressReceipt`, so
    // one arm body serves both and the tag — not a second instrument — separates steady state from recovery.
    const string DrainLane = "drain";
    const string ReplayLane = "replay";

    // Settlement values publish beside their slot because a partitioned counter's good half is read TWICE — the
    // arm stamps it and the `#STORE_BOARD` indicator names it as the partition's good set — so a value literal at
    // either site forks that share the moment the other moves. The four rows partition `EgressReceipt.Drained`
    // whole: a duplicate is a delivery the sink's own dedup already absorbed, so it settles on the good side and
    // dropping it from the fan would strand it out of a denominator its receipt counted.
    public const string DeliveredOutcome = "delivered";
    public const string DuplicateOutcome = "duplicate";
    public const string HeldOutcome = "held";
    public const string DeadOutcome = "dead";

    // Retriability values publish beside their slot for the same reason the settlement values do: the board's
    // contention indicator reads the transient share, so a literal at either site forks it the moment one moves.
    public const string TransientOutcome = "transient";
    public const string TerminalOutcome = "terminal";

    // `pg_stat_io` reports `hits` only against relation objects; a wal or temp row carries reads with no
    // buffer-hit concept, so folding it into the ratio reports a cache miss no buffer ever took.
    const string RelationObject = "relation";

    // Fanned-dimension vocabularies: each row pairs the receipt's wire field with the tag value it writes
    // under, so a counter or level family spans its whole axis through ONE instrument and a further
    // harvest column is one pair here. Every table IS the closed tag vocabulary its instrument row's
    // `Dimensions` declares, so declared cardinality and stamped cardinality cannot drift apart.
    static readonly Seq<(string Field, string Value)> StepTells = Seq(
        ("vmSteps", "vm"), ("fullScanSteps", "fullscan"), ("sorts", "sort"), ("autoIndexRows", "autoindex"));
    static readonly Seq<(string Field, string Value)> MemoryRegions = Seq(
        ("cacheBytes", "cache"), ("schemaBytes", "schema"), ("statementBytes", "statement"));
    static readonly Seq<(string Field, string Value)> ProfilePhases = Seq(
        ("latencySeconds", "wall"), ("cpuSeconds", "cpu"), ("blockedThreadSeconds", "blocked"));
    static readonly Seq<(string Field, string Value)> IoEventRows = Seq(
        ("evictions", "eviction"), ("fsyncs", "fsync"));

    public const string StatementDuration = Head + "statement.duration";
    public const string IoHitRatio = Head + "io.hit.ratio";
    public const string IoEvents = Head + "io.events";
    public const string DuckDuration = Head + "duckdb.duration";
    public const string DuckRows = Head + "duckdb.rows";
    public const string SqliteSteps = Head + "sqlite.steps";
    public const string SqliteCacheRatio = Head + "sqlite.cache.ratio";
    public const string SqliteMemory = Head + "sqlite.memory";
    public const string EgressDeliveries = Head + "egress.deliveries";
    public const string EgressDeadLetterAttempts = Head + "egress.deadletter.attempts";
    public const string EgressDrainDuration = Head + "egress.drain.duration";
    public const string PlanCaptures = Head + "plan.captures";
    public const string ResidenceReadDuration = Head + "residence.read.duration";
    public const string ResidenceScanned = Head + "residence.scanned";
    public const string ResidenceIngested = Head + "residence.staged";
    public const string BlobBytes = Head + "blob.bytes";
    public const string BlobParts = Head + "blob.parts";
    public const string BlobFaults = Head + "blob.faults";
    public const string CoordinationFaults = Head + "coordination.faults";
    public const string RetentionSwept = Head + "retention.swept";
    public const string UsageSize = Head + "usage.size";
    public const string UsageObjects = Head + "usage.objects";
    public const string UsageDeliveries = Head + "usage.deliveries";

    // Bounds read the kernel `Buckets` policy rows; the histogram row carrying no bounds keeps base2-exponential
    // aggregation, the estate wire default an explicit-bucket row re-arms per instrument.
    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Advised(StatementDuration, "s", "mean execution time per harvested top-N server statement", MeasureForm.Real, Buckets.FoldSeconds),
        InstrumentSpec.Level(IoHitRatio, "1", "shared-buffer hit ratio over the pg_stat_io window", MeasureForm.Real),
        // Eviction pressure is what the hit ratio cannot separate — a ratio holding under mounting evictions
        // is a buffer pool churning at its ceiling — and fsyncs are the durability cost the wal rows carry.
        InstrumentSpec.Count(IoEvents, "{event}", "buffer evictions and fsyncs over the pg_stat_io window by event", MeasureForm.Whole, EventSlot),
        // One profiled-time distribution across its phases: wall against cpu names the lane's parallel yield,
        // wall against blocked names thread contention, and neither reads off a wall-only series.
        InstrumentSpec.Advised(DuckDuration, "s", "profiled analytical statement time by wall, cpu, and blocked phase", MeasureForm.Real, Buckets.ProfileSeconds, PhaseSlot),
        InstrumentSpec.Distribution(DuckRows, "{row}", "rows returned per profiled analytical statement", MeasureForm.Whole),
        // Every read-and-reset step counter rides one stream under its tell, so the full-scan and
        // transient-index tells the #PLAN_PROFILE sqlite leg explains ride the same denominator as the
        // virtual-machine steps they are judged against.
        InstrumentSpec.Count(SqliteSteps, "{step}", "embedded statement steps per harvest interval by tell", MeasureForm.Whole, StepSlot),
        InstrumentSpec.Level(SqliteCacheRatio, "1", "embedded page-cache hit ratio over the sampled connection", MeasureForm.Real),
        InstrumentSpec.Levels(SqliteMemory, "By", "embedded store bytes held by memory region", MeasureForm.Whole, RegionSlot),
        InstrumentSpec.Count(EgressDeliveries, "{delivery}", "egress entries by sink, lane, and settlement outcome", MeasureForm.Whole, SinkSlot, LaneSlot, OutcomeSlot),
        // Attempt depth is what the per-drain outcome partition cannot carry — this distribution's own
        // observation count IS the dead-letter stream, its tail naming an entry no replay budget settles.
        InstrumentSpec.Advised(EgressDeadLetterAttempts, "{attempt}", "delivery attempts per dead-lettered egress entry by sink", MeasureForm.Whole, Buckets.IterationCounts, SinkSlot),
        InstrumentSpec.Advised(EgressDrainDuration, "s", "wall duration per egress drain by sink and lane", MeasureForm.Real, Buckets.ProfileSeconds, SinkSlot, LaneSlot),
        // Every capture writes, so the drift rate reads as a ratio over its own denominator rather than as a
        // bare numerator no objective can normalize.
        InstrumentSpec.Count(PlanCaptures, "{capture}", "plan-shape captures by engine and compare verdict", MeasureForm.Whole, EngineSlot, RuleSlot),
        // ANALYTICS RESIDENCE tier: duration answers whether a tile is affordable and scanned magnitude answers
        // WHY — a read whose granules stopped pruning holds its latency while its scan multiplies, and the
        // returned row count says nothing about either, which is the whole reason the tenant leads the sort key.
        InstrumentSpec.Advised(ResidenceReadDuration, "s", "wall duration per residence read by residence", MeasureForm.Real, Buckets.ProfileSeconds, ResidenceSlot),
        InstrumentSpec.Advised(ResidenceScanned, "{row}", "rows the engine scanned per residence read by residence", MeasureForm.Whole, Buckets.IterationCounts, ResidenceSlot),
        InstrumentSpec.Count(ResidenceIngested, "{row}", "rows staged per residence dataset landing", MeasureForm.Whole, DatasetSlot),
        // Object plane and retention. Bytes is a COUNTER in UCUM `By` rather than a bucketed distribution because
        // object magnitude spans four orders across one deployment and no bucket ladder grades that honestly; parts
        // is the depth distribution, matching the dead-letter attempt-depth argument above it. Faults key on the
        // union's own `Category` projection, and sweep verdicts on the nine-value `SweepVerdict.Rule` — both bounded
        // by their publishing union, so declared cardinality and stamped cardinality cannot drift apart.
        InstrumentSpec.Count(BlobBytes, "By", "object bytes transferred by provider and transfer verb", MeasureForm.Whole, ProviderSlot, VerbSlot),
        InstrumentSpec.Advised(BlobParts, "{part}", "multipart parts staged per object by provider", MeasureForm.Whole, Buckets.IterationCounts, ProviderSlot),
        InstrumentSpec.Count(BlobFaults, "{fault}", "object-plane refusals by provider and fault category", MeasureForm.Whole, ProviderSlot, CategorySlot),
        // Fenced-store refusals partition on the union's own `Category` beside its retriability bit, because a
        // `LeaseFenced` storm is split-brain while a `Contended` storm is lock pressure and one undifferentiated
        // count reads identically for both. Both dimensions close over the publishing union, so declared and
        // stamped cardinality cannot drift.
        InstrumentSpec.Count(CoordinationFaults, "{fault}", "fenced-store refusals by fault category and retriability", MeasureForm.Whole, CategorySlot, OutcomeSlot),
        InstrumentSpec.Count(RetentionSwept, "{verdict}", "retention verdicts by class and deciding rule", MeasureForm.Whole, ClassSlot, RuleSlot),
        InstrumentSpec.Levels(UsageSize, "By", "durable bytes by tenant", MeasureForm.Whole, TenantContext.TenantSlot),
        InstrumentSpec.Levels(UsageObjects, "{object}", "durable objects by tenant", MeasureForm.Whole, TenantContext.TenantSlot),
        InstrumentSpec.Levels(UsageDeliveries, "{delivery}", "egress deliveries by tenant over the usage census window", MeasureForm.Whole, TenantContext.TenantSlot));

    // Arm bodies are the one place receipt wire names meet instrument writes; the AppHost fan merges this table
    // beside its own at the Mount seam. Every arm returns the kernel write rail and multi-write arms traverse
    // their own row table, so the first refusal names the offending row and none is discarded at the delegate.
    public static readonly FrozenDictionary<string, InstrumentArm> Arms =
        new Dictionary<string, InstrumentArm> {
            [PgStatHarvest.StatementsSlot.ToString()] = static (set, payload) =>
                toSeq(payload.GetProperty("rows").EnumerateArray())
                    .TraverseM(row => set.Write(StatementDuration, row.GetProperty("meanExecMs").GetDouble() / 1000d)).As()
                    .Map(static _ => unit),
            [PgStatHarvest.IoSlot.ToString()] = static (set, payload) =>
                from rows in Fin.Succ(toSeq(payload.GetProperty("rows").EnumerateArray()).Strict())
                from taken in Fin.Succ(rows
                    .Filter(static row => row.GetProperty("object").GetString() == RelationObject)
                    .Fold((Hits: 0L, Reads: 0L),
                        static (sum, row) => (sum.Hits + row.GetProperty("hits").GetInt64(), sum.Reads + row.GetProperty("reads").GetInt64())))
                from _ in set.Level(IoHitRatio, taken.Hits + taken.Reads > 0L ? (double)taken.Hits / (taken.Hits + taken.Reads) : 1d)
                // Pressure counters sum across EVERY object: a wal or temp row takes no buffer hit yet
                // pays the same eviction and fsync cost, so the ratio's relation filter never bounds them.
                from done in IoEventRows.TraverseM(row => set.Write(IoEvents,
                    rows.Fold(0L, (sum, entry) => sum + entry.GetProperty(row.Field).GetInt64()),
                    InstrumentSet.Tags((EventSlot, row.Value)))).As()
                select unit,
            [DuckProfileHarvest.Slot.ToString()] = static (set, payload) =>
                ProfilePhases.TraverseM(row => set.Write(DuckDuration, payload.GetProperty(row.Field).GetDouble(),
                    InstrumentSet.Tags((PhaseSlot, row.Value)))).As()
                    .Bind(_ => set.Write(DuckRows, payload.GetProperty("rowsReturned").GetInt64())),
            [SqliteStatHarvest.StatementsSlot.ToString()] = static (set, payload) =>
                StepTells.TraverseM(row => set.Write(SqliteSteps, payload.GetProperty(row.Field).GetInt64(),
                    InstrumentSet.Tags((StepSlot, row.Value)))).As()
                    .Map(static _ => unit),
            [SqliteStatHarvest.ConnectionSlot.ToString()] = static (set, payload) =>
                from taken in Fin.Succ((Hit: payload.GetProperty("cacheHits").GetInt64(), Miss: payload.GetProperty("cacheMisses").GetInt64()))
                from _ in set.Level(SqliteCacheRatio, taken.Hit + taken.Miss > 0L ? (double)taken.Hit / (taken.Hit + taken.Miss) : 1d)
                // Region gauges sample WITHOUT reset, so each keyed entry is a level the observable family
                // reads at collection cadence rather than an interval delta a counter would accumulate.
                from done in MemoryRegions.TraverseM(row => set.Level(SqliteMemory, payload.GetProperty(row.Field).GetInt64(), Some(row.Value))).As()
                select unit,
            [EgressPump.DrainSlot.ToString()] = Fan(DrainLane),
            [EgressPump.ReplaySlot.ToString()] = Fan(ReplayLane),
            // `ObjectIo` seats four slots on ONE verb axis over one `BlobTransferFact` shape, so they fan through
            // one parameterized mint exactly as the two egress lanes do — four arm bodies for one fact shape is the
            // inline-repeated-concern defect the `Fan` precedent already deleted once.
            [ObjectIo.PartSlot.ToString()] = Verb("part"),
            [ObjectIo.ResumeSlot.ToString()] = Verb("resume"),
            [ObjectIo.AbortSlot.ToString()] = Verb("abort"),
            [ObjectIo.WriteSlot.ToString()] = Verb("write"),
            [ObjectIo.FaultSlot.ToString()] = static (set, payload) =>
                set.Write(BlobFaults, 1L, InstrumentSet.Tags(
                    (ProviderSlot, payload.GetProperty("provider").GetString()),
                    (CategorySlot, payload.GetProperty("kind").GetString()))),
            [Coordinate.FaultSlot.ToString()] = static (set, payload) =>
                set.Write(CoordinationFaults, 1L, InstrumentSet.Tags(
                    (CategorySlot, payload.GetProperty("category").GetString()),
                    (OutcomeSlot, payload.GetProperty("transient").GetBoolean() ? TransientOutcome : TerminalOutcome))),
            // Retention verdicts fold per class and deciding rule off the sweep receipt's own partition, so the
            // one instrument answers "what did retention decide, where" without a per-verdict-kind instrument
            // family: the rule vocabulary is `SweepVerdict.Rule`'s and the class vocabulary the six-row axis.
            [RetentionSweep.SweepSlot.ToString()] = static (set, payload) =>
                from carrier in Fin.Succ(InstrumentSet.Tags((ClassSlot, payload.GetProperty("class").GetString())))
                from done in SweepOutcomes.TraverseM(row => set.Write(RetentionSwept, payload.GetProperty(row.Field).GetInt64(),
                    [.. carrier, new(RuleSlot, row.Value)])).As().Map(static _ => unit)
                select done,
            [EgressPump.DeadLetterSlot.ToString()] = static (set, payload) =>
                set.Write(EgressDeadLetterAttempts, payload.GetProperty("attempts").GetInt64(),
                    InstrumentSet.Tags((SinkSlot, payload.GetProperty("sink").GetString()))),
            [PlanProfile.Slot.ToString()] = static (set, payload) =>
                set.Write(PlanCaptures, 1L, InstrumentSet.Tags(
                    (EngineSlot, payload.GetProperty("engine").GetString()),
                    (RuleSlot, payload.GetProperty("rule").GetString()))),
            // `ResidenceReceipt` stays NON-GENERIC by construction, so scanned rows and the elapsed figure
            // reach the meter while payload rows never touch the wire — read evidence lands at this custodian
            // precisely because a consumer arrow hands back bare values.
            [ColumnarLane.ReadSlot.ToString()] = static (set, payload) =>
                from carrier in Fin.Succ(InstrumentSet.Tags((ResidenceSlot, payload.GetProperty("residence").GetString())))
                from _ in set.Write(ResidenceReadDuration, Seconds(payload.GetProperty("elapsed")), carrier)
                from done in set.Write(ResidenceScanned, payload.GetProperty("scanned").GetInt64(), carrier)
                select done,
            [ColumnarLane.IngestSlot.ToString()] = static (set, payload) =>
                set.Write(ResidenceIngested, payload.GetProperty("staged").GetInt64(),
                    InstrumentSet.Tags((DatasetSlot, payload.GetProperty("dataset").GetString()))),
            // Grouping keys on `TenantContext.Key`, the kernel's ONE optional-key read, so the root group projects
            // its figures UNTAGGED and a partitioning row tags its `Entry` text: one instrument answers both
            // deployments, the root tenant's usage stops vanishing behind a filter, and the untagged entry marks the
            // absent-tenant PARTITION rather than a total, so a board folding the family counts every byte once.
            // Class and tier remain receipt and census facts, so the tenant cap never multiplies by them. This arm
            // alone gates: a whole-census decode, a grouping, and three sums precede its writes, where every other
            // arm pays one `GetProperty` read and one `TagList` mint — under the roster walk the gate itself runs,
            // so gating those arms buys a second admission read.
            [StoreUsage.Slot.ToString()] = static (set, payload) =>
                !set.Enabled(UsageSize, UsageObjects, UsageDeliveries)
                    ? Fin.Succ(unit)
                    : StoreUsage.Decode(payload).Bind(census =>
                        toSeq(census.GroupBy(static row => row.Tenant.Key))
                            .TraverseM(group => Seq(
                                (Name: UsageSize, Value: group.Sum(static row => row.Bytes)),
                                (Name: UsageObjects, Value: group.Sum(static row => row.Objects)),
                                (Name: UsageDeliveries, Value: group.Sum(static row => row.Deliveries)))
                                .TraverseM(measure => set.Level(measure.Name, measure.Value, group.Key)).As()).As()
                            .Map(static _ => unit)),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Scope reads the minted `TelemetrySource` row and `SchemaUrl` the one pinned semconv coordinate the
    // settled `TelemetryIdentity.Mint` stamps as `MeterOptions.TelemetrySchemaUrl`, so the package id and the
    // schema pin each carry exactly one spelling and no folder OTel reference exists. Rows and the `#STORE_BOARD`
    // pack over them leave as ONE downward fact, so the mounting root proves the pack in the same fold that binds
    // these handles. Forward reach stays safe by construction: the pack reads consts and one vocabulary column,
    // neither of which triggers this owner's static construction, while this factory is a method the pack never calls.
    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: TelemetrySource.Persistence.Key, Version: version, Instruments: Rows, SchemaUrl: schemaUrl,
            Board: StoreDescriptors.Pack);

    // Declared-truth census reads kind, bounds, and tag vocabulary off the rows themselves, so a bucket retune
    // or a new dimension travels to the board through the roster with no second table to drift against.
    public static StoreTelemetryCensus Census(string version, SlotRegistry registry) =>
        new(TelemetrySource.Persistence.Key, version,
            Rows.Map(static row => new CensusRow(
                row.Name, row.Kind.Key, row.Unit, row.Description, row.Bounds.IfNone([]), row.Dimensions)),
            toSeq(registry.Slots.Order(StringComparer.Ordinal)),
            toSeq(Arms.Keys.Order(StringComparer.Ordinal)));

    // ONE fan body over the closed lane vocabulary: the steady drain and the letter replay each settle an
    // `EgressReceipt` whose delivered/held/dead counts partition its drained rows, so the lane rides a tag and
    // a per-lane arm shape never forks the conservation identity a delivery objective normalizes against.
    // Settlement outcomes are one (wire field, tag value) row table like every other fanned dimension here, so a
    // fourth settlement state is one row rather than a fourth hand-spelled write on the same instrument.
    static readonly Seq<(string Field, string Value)> SettlementOutcomes = Seq(
        ("delivered", DeliveredOutcome), ("duplicates", DuplicateOutcome),
        ("held", HeldOutcome), ("deadLettered", DeadOutcome));

    // `SweepOutcomes` spells the sweep receipt's conservation partition as its tag vocabulary — closure over
    // `inventory = kept + held + cooled + evicted` makes a fifth retention-side count one row here rather than a
    // fifth instrument, and the wire fields stay the receipt's own, so an uncounted rule value never stamps.
    static readonly Seq<(string Field, string Value)> SweepOutcomes = Seq(
        ("kept", "kept"), ("held", "hold"), ("cooled", "cool"), ("evicted", "evict"));

    // ONE fan body over the object plane's verb axis: every `BlobTransferFact` carries provider, bytes, and part,
    // so bytes count under (provider, verb) and the staged depth rides the part ordinal — the `abort` and `write`
    // verbs contribute zero-byte and whole-object rows respectively through the same two writes, because the verb
    // is a tag and a per-verb arm shape forks one fact's accounting across four bodies.
    static InstrumentArm Verb(string verb) => (set, payload) =>
        from carrier in Fin.Succ(InstrumentSet.Tags(
            (ProviderSlot, payload.GetProperty("provider").GetString()), (VerbSlot, verb)))
        from _ in set.Write(BlobBytes, payload.GetProperty("bytes").GetInt64(), carrier)
        from done in set.Write(BlobParts, payload.GetProperty("part").GetInt64(),
            [.. InstrumentSet.Tags((ProviderSlot, payload.GetProperty("provider").GetString()))])
        select done;

    static InstrumentArm Fan(string lane) => (set, payload) =>
        from carrier in Fin.Succ(InstrumentSet.Tags(
            (SinkSlot, payload.GetProperty("sink").GetString()), (LaneSlot, lane)))
        from _ in SettlementOutcomes.TraverseM(row => set.Write(EgressDeliveries, payload.GetProperty(row.Field).GetInt64(),
            [.. carrier, new(OutcomeSlot, row.Value)])).As()
        from done in set.Write(EgressDrainDuration, Seconds(payload.GetProperty("elapsed")), carrier)
        select done;

    // NodaTime `Duration` crosses the wire as its JsonRoundtrip text (`api-nodatime-stj` `DurationConverter`);
    // One arm-side decode yields seconds beside the AppHost fan's `Seconds` peer.
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

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;                              // BoardPack, LevelBreach, Objective, PanelKind, PanelSpec, Sli,
                                                // TenantContext
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Store;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class StoreDescriptors {
    public static readonly BoardPack Pack = new(
        Wire: "persistence.census", // the provenance key the deploy tuple admits this projection under; pack and key are one value
        Panels: Seq(
            PanelSpec.Of("Server statement duration", StoreInstruments.StatementDuration),
            PanelSpec.Of("Buffer hit ratio", StoreInstruments.IoHitRatio),
            PanelSpec.Of("Buffer pressure", StoreInstruments.IoEvents, StoreInstruments.EventSlot),
            PanelSpec.Of("Analytical time by phase", StoreInstruments.DuckDuration, StoreInstruments.PhaseSlot),
            PanelSpec.Of("Analytical rows returned", StoreInstruments.DuckRows),
            PanelSpec.Of("Embedded step tells", StoreInstruments.SqliteSteps, StoreInstruments.StepSlot),
            PanelSpec.Of("Embedded cache ratio", StoreInstruments.SqliteCacheRatio),
            PanelSpec.Of("Embedded memory by region", StoreInstruments.SqliteMemory, StoreInstruments.RegionSlot),
            PanelSpec.Of("Egress settlement", StoreInstruments.EgressDeliveries, PanelKind.Table,
                StoreInstruments.SinkSlot, StoreInstruments.LaneSlot, StoreInstruments.OutcomeSlot),
            PanelSpec.Of("Dead-letter attempt depth", StoreInstruments.EgressDeadLetterAttempts, StoreInstruments.SinkSlot),
            PanelSpec.Of("Drain duration", StoreInstruments.EgressDrainDuration, StoreInstruments.SinkSlot, StoreInstruments.LaneSlot),
            PanelSpec.Of("Plan captures", StoreInstruments.PlanCaptures, PanelKind.Table,
                StoreInstruments.EngineSlot, StoreInstruments.RuleSlot),
            PanelSpec.Of("Residence read duration", StoreInstruments.ResidenceReadDuration, StoreInstruments.ResidenceSlot),
            PanelSpec.Of("Residence rows scanned", StoreInstruments.ResidenceScanned, StoreInstruments.ResidenceSlot),
            PanelSpec.Of("Residence rows staged", StoreInstruments.ResidenceIngested, StoreInstruments.DatasetSlot),
            PanelSpec.Of("Durable bytes by tenant", StoreInstruments.UsageSize, TenantContext.TenantSlot),
            PanelSpec.Of("Durable objects by tenant", StoreInstruments.UsageObjects, TenantContext.TenantSlot),
            PanelSpec.Of("Egress deliveries by tenant", StoreInstruments.UsageDeliveries, TenantContext.TenantSlot)),
        Objectives: Seq(
            // Duplicates count as deliveries the sink absorbed, so both settled spellings ride the good half while
            // this denominator stays the whole drained population its own arm counted.
            Objective.Create(
                name: "persistence.egress.settled",
                sli: new Sli.Partition(
                    Metric: StoreInstruments.EgressDeliveries,
                    By: StoreInstruments.OutcomeSlot,
                    Good: Seq(StoreInstruments.DeliveredOutcome, StoreInstruments.DuplicateOutcome)),
                target: 0.999d,
                window: default),
            // Good half derives from the vocabulary's own stability column, so a fourth compare rule moves this
            // target where the rule is declared rather than here.
            Objective.Create(
                name: "persistence.plan.stable",
                sli: new Sli.Partition(
                    Metric: StoreInstruments.PlanCaptures,
                    By: StoreInstruments.RuleSlot,
                    Good: PlanRule.StableKeys),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "persistence.egress.latency",
                sli: new Sli.Latency(Metric: StoreInstruments.EgressDrainDuration, Ceiling: Duration.FromSeconds(5), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            // Residence reads back interactive tiles, so this ceiling is a TILE budget rather than a batch one,
            // and the scanned distribution beside it carries no objective because a scan magnitude diagnoses a
            // breach here rather than naming a target an operator tunes against.
            Objective.Create(
                name: "persistence.residence.latency",
                sli: new Sli.Latency(Metric: StoreInstruments.ResidenceReadDuration, Ceiling: Duration.FromSeconds(2), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            // Headroom is a saturation over a pulled ratio, and the polarity column carries the direction: both
            // buffer measures breach BELOW their floor, so neither earns a shape of its own.
            Objective.Create(
                name: "persistence.io.headroom",
                sli: new Sli.Saturation(Metric: StoreInstruments.IoHitRatio, Bound: 0.9d, Breach: LevelBreach.Floor),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "persistence.embedded.headroom",
                sli: new Sli.Saturation(Metric: StoreInstruments.SqliteCacheRatio, Bound: 0.8d, Breach: LevelBreach.Floor),
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
