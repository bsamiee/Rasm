# [PERSISTENCE_STORE_OBSERVABILITY]

Engine-stat observability, the receipt-slot registry, and the store instrument contributor: one slot grammar names every evidence stream Persistence emits, one registry enforces uniqueness at composition, one harvest fold turns each engine's statistics surface — PostgreSQL cumulative views, DuckDB profiling output, SQLite status counters — into typed receipts, and one contributor projects those receipts into `rasm.persistence.*` instruments. Embedded engines expose no scrape surface, so the embedding process is their observer and the receipt rail is their observability.

Settled composition: `ReceiptSinkPort` and `ProjectionContext` arrive from the AppHost port vocabulary; `TelemetrySource.Persistence`, `InstrumentRow`, `TelemetryContributorPort`, `InstrumentSet`, and the receipt observe tap arrive from the observability spine. `Npgsql.OpenTelemetry` subscribes at the AppHost root — `AddNpgsql()` tracing, the `Npgsql` meter by name under the `AddView` posture the `NpgsqlDataSourceBuilder.Name` pool dimension keys. Metric names are dotted `rasm.<domain>.<measure>`, units UCUM, scope id the emitting package id.

## [01]-[INDEX]

- [01]-[SLOT_REGISTRY]: `store.<domain>.<verb>` grammar, the registry fold, and the page-contributed mount.
- [02]-[PG_STAT_HARVEST]: `pg_stat_statements` and `pg_stat_io` typed harvest receipts.
- [03]-[DUCKDB_PROFILE_HARVEST]: Profiling-JSON harvest off the analytical lane.
- [04]-[SQLITE_STATUS_HARVEST]: Statement and connection status counters off the raw bridge.
- [05]-[STORE_INSTRUMENTS]: `rasm.persistence.*` instrument roster, contributor port, and receipt-projection arms.

## [02]-[SLOT_REGISTRY]

- Owner: `StoreSlot` `[ValueObject<string>]` — the slot name under the `store.<domain>.<verb>` grammar, the verb a dotted path when one domain carries verb families; `SlotRegistry` — the composition-time catalog of every slot this package emits.
- Entry: `SlotRegistry.Mount(params ReadOnlySpan<StoreSlot> slots)` — freezes the catalog and throws on a duplicate at composition; `SlotRegistry.Mounted()` — the composition-root census spreading every page's contributed roster; `SlotRegistry.Admit(SlotRegistry registry, StoreSlot slot)` — the pre-send gate every receipt emission crosses, so an unregistered slot is a typed refusal, never a silent new stream.
- Auto: each owning page carries one `Slots` roster on its primary owner and `Mounted` spreads them, so the registry is the one census of the emitted-signal surface and discovery stops being page-by-page archaeology.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new evidence stream is one `StoreSlot` row on its owning page's roster; the grammar admits a new domain or verb with zero registry edits.
- Boundary: the slot is the `kind` argument the sink `Send` carries, so slot vocabulary and wire kind are one spelling; this page mints its own harvest slots — `store.stat.statements`, `store.stat.io`, `store.stat.duckdb`, `store.stat.sqlite` — and every other page's slots enter as its contributed rows, so the registry owns uniqueness while each page owns its spellings; a per-occurrence discriminant — a traversal's query case, a sink's lane — rides the receipt payload, never the slot string, so the census stays frozen while payloads vary.

```csharp signature
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<StatFault>]
public readonly partial struct StoreSlot {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
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

    // Composition-root census: every page's roster spreads here, so a new page slot is one roster row and zero registry edits.
    public static SlotRegistry Mounted() => Mount([
        PgStatHarvest.StatementsSlot, PgStatHarvest.IoSlot, DuckProfileHarvest.Slot, SqliteStatHarvest.Slot,
        .. GraphStore.Slots, .. TabularSource.Slots, .. ScheduleSource.Slots, .. GeoSource.Slots,
        .. IssueSource.Slots, .. Coordinate.Slots, .. ClusterProvision.Slots, .. ObjectIo.Slots,
        .. ModelResultIndex.Slots, .. ColumnarLane.Slots, .. ReadRouter.Slots, .. GraphSession.Slots,
        .. Federation.Slots, .. Traversals.Slots, .. SearchRoute.Slots, .. OpLog.Slots,
        .. EgressPump.Slots, .. StructuralMerge.Slots, .. Crdt.Slots, .. RecoveryRoutes.Slots,
        .. TimeTravel.Slots]);

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
- Receipt: `StatementStatRow` — queryid, calls, total and mean execution time, rows, shared-block hits and reads, WAL bytes; `IoStatRow` — backend type, object, context, reads, writes, extends, hits, evictions, fsyncs; each batch fans under `store.stat.statements` / `store.stat.io`.
- Packages: Npgsql, LanguageExt.Core, NodaTime.
- Growth: a new harvested column is one field on the owning row and one select column; a new server view is one harvest member on this owner.
- Boundary: this fold is the query-depth complement to the driver meter seam — the `Npgsql` meter carries operation duration and pool level at the AppHost root while these rows carry per-statement and per-backend server truth as receipts; pg_stat views are server-global, so these receipts carry no tenant brand by ruling and the batch envelope carries the frame correlation at the `Send` seam; the three lag gauges stay distinct owners — provisioning's slot lag, recovery's replication lag, and this page's I/O timing never share a row; `track_io_timing` is a deliberate server posture the provisioning verify batch asserts before timing columns read as truth.

```csharp signature
public sealed record StatementStatRow(
    long QueryId, long Calls, double TotalExecMs, double MeanExecMs, long Rows,
    long SharedBlksHit, long SharedBlksRead, long WalBytes);

public sealed record IoStatRow(
    string BackendType, string Object, string Context,
    long Reads, long Writes, long Extends, long Hits, long Evictions, long Fsyncs);

public static class PgStatHarvest {
    public static readonly StoreSlot StatementsSlot = StoreSlot.Create("store.stat.statements");
    public static readonly StoreSlot IoSlot = StoreSlot.Create("store.stat.io");

    const string StatementsSql = """
        SELECT queryid, calls, total_exec_time, mean_exec_time, rows,
               shared_blks_hit, shared_blks_read, wal_bytes
        FROM pg_stat_statements
        ORDER BY total_exec_time DESC
        LIMIT $1
        """;

    const string IoSql = """
        SELECT backend_type, object, context, reads, writes, extends, hits, evictions, fsyncs
        FROM pg_stat_io
        WHERE reads > 0 OR writes > 0
        """;

    public static IO<Seq<StatementStatRow>> Statements(NpgsqlDataSource source, int top) =>
        IO.liftAsync(async () => {
            await using var command = source.CreateCommand(StatementsSql);
            command.Parameters.AddWithValue(top);
            await using var reader = await command.ExecuteReaderAsync();
            var rows = Seq<StatementStatRow>();
            while (await reader.ReadAsync()) {
                rows = rows.Add(new StatementStatRow(
                    reader.GetInt64(0), reader.GetInt64(1), reader.GetDouble(2), reader.GetDouble(3),
                    reader.GetInt64(4), reader.GetInt64(5), reader.GetInt64(6), reader.GetInt64(7)));
            }
            return rows.Strict();
        });

    public static IO<Seq<IoStatRow>> Io(NpgsqlDataSource source) =>
        IO.liftAsync(async () => {
            await using var command = source.CreateCommand(IoSql);
            await using var reader = await command.ExecuteReaderAsync();
            var rows = Seq<IoStatRow>();
            while (await reader.ReadAsync()) {
                rows = rows.Add(new IoStatRow(
                    reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetInt64(3), reader.GetInt64(4), reader.GetInt64(5),
                    reader.GetInt64(6), reader.GetInt64(7), reader.GetInt64(8)));
            }
            return rows.Strict();
        });
}
```

## [04]-[DUCKDB_PROFILE_HARVEST]

- Owner: `DuckProfileHarvest` — the profiling-switch bracket and the profile receipt over the analytical lane's connection.
- Entry: `DuckProfileHarvest.Profiled(DuckDBConnection connection, string sql, ProjectionContext context)` — runs one statement under `enable_profiling='json'` with `profiling_output` redirected to a run-scoped file, parses the JSON, and folds one `DuckProfileReceipt`.
- Auto: `profiling_mode` stays `standard` for routine harvests and the detailed optimizer metrics enter as one pragma value when a plan investigation demands them; the per-operator tree folds to a digest with the top-cost operator rows so the receipt stays bounded while the full JSON lands as an artifact when retained.
- Receipt: `DuckProfileReceipt` — latency, CPU time, rows returned, result-set bytes, blocked-thread time, operator-tree digest, top operator rows, the frame's instant and correlation; fans under `store.stat.duckdb`.
- Packages: DuckDB.NET.Data.Full, LanguageExt.Core, NodaTime, System.IO.Hashing.
- Growth: one profiling metric key is one receipt field and one parse line; a per-query alternative rides `EXPLAIN ANALYZE (FORMAT JSON)` through the same parse fold.
- Boundary: the profiling switch is connection state, so the bracket sets, runs, and resets on every exit path — a lane query outside the bracket runs unprofiled at full speed; the harvest borrows the `Query/columnar` connection and mints no second DuckDB lane; the analytical lane is process-scoped, so the receipt carries the frame's correlation and instant while tenant stays a `ProjectionContext` fact the sink envelope carries by ruling.

```csharp signature
public sealed record DuckOperatorRow(string Name, double TimingSeconds, long Cardinality);

public sealed record DuckProfileReceipt(
    double LatencySeconds, double CpuSeconds, long RowsReturned, long ResultSetBytes,
    double BlockedThreadSeconds, UInt128 PlanDigest, Seq<DuckOperatorRow> TopOperators,
    Instant At, Guid Correlation);

public static class DuckProfileHarvest {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.duckdb");

    public static IO<DuckProfileReceipt> Profiled(DuckDBConnection connection, string sql, ProjectionContext context) =>
        IO.liftAsync(async () => {
            var output = Path.Join(Path.GetTempPath(), $"duck-profile-{context.Correlation}.json");
            await using (var arm = connection.CreateCommand()) {
                arm.CommandText = $"PRAGMA enable_profiling='json'; PRAGMA profiling_output='{output}'; PRAGMA profiling_mode='standard';";
                await arm.ExecuteNonQueryAsync();
            }
            try {
                await using var work = connection.CreateCommand();
                work.CommandText = sql;
                await work.ExecuteNonQueryAsync();
                using var profile = JsonDocument.Parse(await File.ReadAllBytesAsync(output));
                return Decode(profile.RootElement, context);
            }
            finally {
                await using var disarm = connection.CreateCommand();
                disarm.CommandText = "PRAGMA disable_profiling;";
                await disarm.ExecuteNonQueryAsync();
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
- Entry: `SqliteStatHarvest.Statement(SqliteDataReader reader)` — reads and resets the executed statement's counters off `SqliteDataReader.Handle`; `SqliteStatHarvest.Connection(SqliteConnection connection)` — samples the connection cache gauges off `SqliteConnection.Handle`.
- Auto: statement counters reset per read so each receipt carries one run's work, while connection gauges sample without reset so cache hit ratio folds over the interval; a full-scan step count or transient-index count above zero on a hot statement is the plan-regression tell the receipt makes visible.
- Receipt: `SqliteStatementStat` — VM steps, full-scan steps, sorts, transient-index rows, re-prepares, runs; `SqliteConnectionStat` — cache hits, misses, writes, spills, schema and statement bytes; both fan under `store.stat.sqlite`.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, LanguageExt.Core.
- Growth: one counter is one field and one raw-call line; the DBSTAT space census joins as one aggregate query member once its research row settles.
- Boundary: the raw calls reach through the same handles the provisioning engine operations already bridge, so the harvest opens no second native path and reads the same native connection the ADO surface drives; the embedded store is process-scoped, so these receipts carry no tenant brand by ruling; provider-bundle facts stay engine-layer and never become Persistence vocabulary.

```csharp signature
public sealed record SqliteStatementStat(int VmSteps, int FullScanSteps, int Sorts, int AutoIndexRows, int Reprepares, int Runs);

public sealed record SqliteConnectionStat(int CacheHits, int CacheMisses, int CacheWrites, int CacheSpills, int SchemaBytes, int StatementBytes);

public static class SqliteStatHarvest {
    public static readonly StoreSlot Slot = StoreSlot.Create("store.stat.sqlite");

    public static Fin<SqliteStatementStat> Statement(SqliteDataReader reader) =>
        reader.Handle is { } statement
            ? Fin.Succ(new SqliteStatementStat(
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_VM_STEP, 1),
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_FULLSCAN_STEP, 1),
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_SORT, 1),
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_AUTOINDEX, 1),
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_REPREPARE, 1),
                raw.sqlite3_stmt_status(statement, raw.SQLITE_STMTSTATUS_RUN, 1)))
            : Fin.Fail<SqliteStatementStat>(new StatFault.HarvestRefused("sqlite", "statement handle absent"));

    public static Fin<SqliteConnectionStat> Connection(SqliteConnection connection) =>
        connection.Handle is { } db
            ? Fin.Succ(new SqliteConnectionStat(
                Gauge(db, raw.SQLITE_DBSTATUS_CACHE_HIT), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_MISS),
                Gauge(db, raw.SQLITE_DBSTATUS_CACHE_WRITE), Gauge(db, raw.SQLITE_DBSTATUS_CACHE_SPILL),
                Gauge(db, raw.SQLITE_DBSTATUS_SCHEMA_USED), Gauge(db, raw.SQLITE_DBSTATUS_STMT_USED)))
            : Fin.Fail<SqliteConnectionStat>(new StatFault.HarvestRefused("sqlite", "connection handle absent"));

    static int Gauge(sqlite3 db, int op) {
        ignore(raw.sqlite3_db_status(db, op, out var current, out _, 0));
        return current;
    }
}
```

## [06]-[STORE_INSTRUMENTS]

- Owner: `StoreInstruments` — the Persistence `InstrumentRow` roster, the `TelemetryContributorPort` mint, and the kind-keyed projection arms; `StoreLevel` — the level atoms the two observable gauges read.
- Cases: statement-duration histogram off the pg statements batch; I/O hit-ratio gauge off the pg I/O batch; DuckDB latency and row histograms off the profile receipt; SQLite VM-step and full-scan counters off the per-statement receipt with the cache-ratio gauge off the connection sample; egress delivery and dead-letter counters off the drain receipts.
- Entry: `StoreInstruments.Telemetry(string version)` — the contributor port peer of the AppHost host roster, carrying every row under `TelemetrySource.Persistence`; `StoreInstruments.Arms` — the kind-keyed projection rows the AppHost receipt fan mounts beside its own arms at the composition root.
- Auto: the projection subscribes as one observe row on the AppHost hook rail's receipt point, so every envelope the sink emits projects with zero call-site metering; level-shaped facts fold into the `StoreLevel` atoms and ride observable gauges at collection cadence, so a polled level never aliases through a synchronous gauge.
- Receipt: none — the arms project the harvest and egress receipts; a metric minted beside them is a second truth.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: one projected slot is one `Arms` row and its instrument rows here; a slot without an `Arms` row is receipt-only by default, so projection is opt-in per row and no page declares the default.
- Boundary: every row binds `TelemetrySource.Persistence`, so scope id equals the package id and instruments mount through the AppHost meter mint, never a package-local `Meter`; pg_stat and engine-status sources are server- and process-global, so no row carries a tenant tag and tenant-scoped stores read the estate wildcard view cap; arm bodies are the one place receipt wire names meet instrument writes, and an arm never re-validates the payload its typed receipt already admitted.

```csharp signature
public sealed record StoreLevel(Atom<double> IoHitRatio, Atom<double> SqliteCacheRatio) {
    public static readonly StoreLevel Live = new(Atom(1d), Atom(1d));
}

public static class StoreInstruments {
    static readonly ImmutableArray<double> StatementSeconds = [0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10];
    static readonly ImmutableArray<double> ProfileSeconds = [0.001, 0.01, 0.05, 0.1, 0.5, 1, 5, 15, 60];

    public static readonly Seq<InstrumentRow> Rows = Seq(
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.statement.duration", "s", "mean execution time per harvested top-N server statement",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text, tags: null,
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = StatementSeconds })),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.io.hit.ratio", "1", "shared-buffer hit ratio over the pg_stat_io window",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => StoreLevel.Live.IoHitRatio.Value, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.duckdb.latency", "s", "profiled analytical statement latency",
            static (meter, name, unit, text) => meter.CreateHistogram<double>(name, unit, text, tags: null,
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = ProfileSeconds })),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.duckdb.rows", "{row}", "rows returned per profiled analytical statement",
            static (meter, name, unit, text) => meter.CreateHistogram<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.sqlite.vm.steps", "{step}", "virtual-machine steps per embedded statement",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.sqlite.fullscan.steps", "{step}", "full-scan steps per embedded statement — the plan-regression tell",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.sqlite.cache.ratio", "1", "embedded page-cache hit ratio over the sampled connection",
            static (meter, name, unit, text) => meter.CreateObservableGauge(name, static () => StoreLevel.Live.SqliteCacheRatio.Value, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.egress.deliveries", "{delivery}", "egress deliveries by sink and outcome",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)),
        new InstrumentRow(TelemetrySource.Persistence, "rasm.persistence.egress.deadletters", "{letter}", "dead-lettered egress entries by sink",
            static (meter, name, unit, text) => meter.CreateCounter<long>(name, unit, text)));

    // Arm bodies are the one place receipt wire names meet instrument writes; the AppHost fan merges this table beside its own at the Mount seam.
    public static readonly FrozenDictionary<string, Action<InstrumentSet, JsonElement>> Arms =
        new Dictionary<string, Action<InstrumentSet, JsonElement>> {
            [PgStatHarvest.StatementsSlot.ToString()] = static (set, payload) => {
                foreach (var row in payload.GetProperty("rows").EnumerateArray()) {
                    ignore(set.Record("rasm.persistence.statement.duration", row.GetProperty("meanExecMs").GetDouble() / 1000d));
                }
            },
            [PgStatHarvest.IoSlot.ToString()] = static (_, payload) => {
                var (hits, reads) = payload.GetProperty("rows").EnumerateArray().Aggregate((0L, 0L),
                    static (sum, row) => (sum.Item1 + row.GetProperty("hits").GetInt64(), sum.Item2 + row.GetProperty("reads").GetInt64()));
                ignore(StoreLevel.Live.IoHitRatio.Swap(_ => hits + reads > 0 ? (double)hits / (hits + reads) : 1d));
            },
            [DuckProfileHarvest.Slot.ToString()] = static (set, payload) => {
                ignore(set.Record("rasm.persistence.duckdb.latency", payload.GetProperty("latencySeconds").GetDouble()));
                ignore(set.Record("rasm.persistence.duckdb.rows", payload.GetProperty("rowsReturned").GetInt64()));
            },
            [SqliteStatHarvest.Slot.ToString()] = static (set, payload) => {
                if (payload.TryGetProperty("vmSteps", out var steps)) {
                    ignore(set.Count("rasm.persistence.sqlite.vm.steps", steps.GetInt64()));
                    ignore(set.Count("rasm.persistence.sqlite.fullscan.steps", payload.GetProperty("fullScanSteps").GetInt64()));
                }
                else {
                    var (hit, miss) = (payload.GetProperty("cacheHits").GetInt64(), payload.GetProperty("cacheMisses").GetInt64());
                    ignore(StoreLevel.Live.SqliteCacheRatio.Swap(_ => hit + miss > 0 ? (double)hit / (hit + miss) : 1d));
                }
            },
            ["store.egress.drain"] = static (set, payload) => {
                var sink = new KeyValuePair<string, object?>("sink", payload.GetProperty("sink").GetString());
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("delivered").GetInt64(), sink, new("outcome", "delivered")));
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("held").GetInt64(), sink, new("outcome", "held")));
                ignore(set.Count("rasm.persistence.egress.deliveries", payload.GetProperty("deadLettered").GetInt64(), sink, new("outcome", "dead")));
            },
            ["store.egress.deadletter"] = static (set, payload) =>
                ignore(set.Count("rasm.persistence.egress.deadletters", 1L,
                    new KeyValuePair<string, object?>("sink", payload.GetProperty("sink").GetString()))),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static TelemetryContributorPort Telemetry(string version) =>
        new(TelemetrySource.Persistence, version, Rows);
}
```

## [07]-[RESEARCH]

- [PG_STAT_IO_BYTES]-[OPEN]: pg18 byte and WAL columns on `pg_stat_io` — exact column spellings for read/write/extend bytes and the WAL object rows the `IoStatRow` widens onto; verify with `\d pg_stat_io` against the provisioned pg18 server before the byte fields land.
- [DBSTAT_ENABLEMENT]-[OPEN]: whether the bundled `e_sqlite3` build ships `SQLITE_ENABLE_DBSTAT_VTAB`, gating the per-table space census over the `dbstat` aggregate mode; verify with `PRAGMA compile_options` through the bundled provider.
- [DRAIN_ELAPSED_WIRE]-[OPEN]: exact wire spelling of `EgressReceipt.Elapsed` under the NodaTime STJ round-trip — a drain-duration histogram row and arm land once the seconds projection settles; verify against the `api-nodatime-stj` catalog's `Duration` pattern.
