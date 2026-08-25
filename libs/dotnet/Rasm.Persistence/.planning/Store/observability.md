# [PERSISTENCE_STORE_OBSERVABILITY]

Engine-stat observability reads PostgreSQL cumulative views, DuckDB profiling output, and SQLite status counters as their native measured rows. Plan capture returns the comparison verdict, usage persists one queryable row shape, and the composing root projects those canonical values through the package instrument roster.

`ProjectionContext` carries the package time, tenancy, correlation, and instrument handles. Provider instrumentation subscribes at the AppHost root through `Npgsql.OpenTelemetry`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`, `OpenTelemetry.Instrumentation.StackExchangeRedis`, and `OpenTelemetry.Instrumentation.AWS`. Metric names use dotted `rasm.<domain>.<measure>` names, UCUM units, and the `TelemetrySource.Persistence` scope.

## [01]-[INDEX]

- [02]-[PG_STAT_HARVEST]: `pg_stat_statements` and `pg_stat_io` typed rows.
- [03]-[DUCKDB_PROFILE_HARVEST]: Profiling-JSON harvest off the analytical lane.
- [04]-[SQLITE_STATUS_HARVEST]: Statement and connection status counters off the raw bridge.
- [05]-[PLAN_PROFILE]: Three-engine plan-shape capture, digest baselines, and the typed drift verdict.
- [06]-[USAGE_PROJECTION]: Queryable `(tenant, class, tier)` usage rows derived from durable catalog and settlement rows.
- [07]-[STORE_INSTRUMENTS]: `rasm.persistence.*` `InstrumentSpec` roster and contributor port.
- [08]-[STORE_BOARD]: `StoreDescriptors` binds the kernel board pack over that roster.

## [02]-[PG_STAT_HARVEST]

- Owner: `PgStatHarvest` reads the two cumulative statement and I/O views into `StatementStatRow` and `IoStatRow`.
- Entry: `PgStatHarvest.Statements(NpgsqlDataSource source, int top)` — the top-N statement rows by total execution time; `PgStatHarvest.Io(NpgsqlDataSource source)` — the per-backend-type I/O rows.
- Auto: both harvests ride the pooled `NpgsqlDataSource` the production path owns, so a stats read shares pool pressure with live traffic and never opens a side connection; `pg_stat_statements` requires the `compute_query_id` server posture the provisioning page's extension roster carries, so `queryid` joins a statement row to the driver span's query identity.
- Packages: Npgsql, LanguageExt.Core, NodaTime.
- Growth: a new harvested column is one field on the owning row and one select column; a new server view is one harvest member on this owner.
- Boundary: the `Npgsql` meter carries operation duration and pool level at the AppHost root while these rows carry per-statement and per-backend server truth. PostgreSQL statistics are server-global, and `track_io_timing` gates timing columns at provisioning.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StatFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreStat;
    private StatFault() { }

    [FaultCase(0)]
    public sealed partial record HarvestRefused(string Engine, Error Cause) : StatFault(), ICausedFault;

    public override string Message => Switch(
        harvestRefused: static c => $"<store-harvest:{c.Engine}:{c.Cause.Message}>");

    public static Error Lift(string engine, Error error, Func<Exception, bool> recognizes) =>
        error.Exception.Case is Exception raised && recognizes(raised)
            ? new HarvestRefused(engine, error)
            : error;
}

public sealed record StatementStatRow(
    long QueryId, long Calls, double TotalExecMs, double MeanExecMs, long Rows,
    long SharedBlksHit, long SharedBlksRead, long WalBytes);

public sealed record IoStatRow(
    string BackendType, string Object, string Context,
    long Reads, long ReadBytes, long Writes, long WriteBytes, long Extends, long ExtendBytes,
    long Hits, long Evictions, long Fsyncs);

public static class PgStatHarvest {
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

## [03]-[DUCKDB_PROFILE_HARVEST]

- Owner: `DuckProfileHarvest` brackets DuckDB profiling state and returns the measured `DuckProfile` from the analytical lane's connection.
- Entry: `DuckProfileHarvest.Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context)` runs one statement under JSON profiling, parses the output, deletes the scratch artifact, and returns the profile.
- Auto: `profiling_mode` stays `standard`; the per-operator tree folds to a digest with the top-cost operator rows, and the bracket deletes the decoded JSON scratch file.
- Packages: DuckDB.NET.Data.Full, LanguageExt.Core, NodaTime, System.IO.Hashing.
- Growth: one profiling metric key is one `DuckProfile` field and one parse line; plan-shape capture probes `EXPLAIN (FORMAT json)` independently.
- Boundary: the bracket sets and resets connection profiling state on every exit path. `outputPath` resolves to a full escaped path and is deleted after decode. The analytical lane is process-scoped, while tenant remains on `ProjectionContext`.

```csharp
public sealed record DuckOperatorRow(string Name, double TimingSeconds, long Cardinality);

public sealed record DuckProfile(
    double LatencySeconds, double CpuSeconds, long RowsReturned, long ResultSetBytes,
    double BlockedThreadSeconds, UInt128 PlanDigest, Seq<DuckOperatorRow> TopOperators,
    Instant At, CorrelationId Correlation);

public static class DuckProfileHarvest {
    public static IO<DuckProfile> Profiled(DuckDBConnection connection, string sql, string outputPath, ProjectionContext context) =>
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

    static DuckProfile Decode(JsonElement root, ProjectionContext frame) {
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

## [04]-[SQLITE_STATUS_HARVEST]

- Owner: `SqliteStatHarvest` — the per-statement and per-connection counter read over the raw bridge.
- Entry: `SqliteStatHarvest.Arm(SqliteConnection connection)` enables statement discovery before any statement is prepared; `Statements` folds read-and-reset statement counters, and `Connection` samples connection gauges.
- Auto: statement counters read with the reset flag for one interval while connection gauges sample without reset. A full-scan or transient-index count names a statement for the plan-profile leg.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3mc, LanguageExt.Core.
- Growth: one counter is one field and one raw-call line bounded by constants the interop assembly declares.
- Boundary: every raw call crosses `HandleBridge`, and `Arm` leads the open ritual before the first statement. The walk borrows each statement handle only inside the fold; process-scoped embedded statistics carry no tenant dimension.

```csharp
public sealed record SqliteStatementStat(int VmSteps, int FullScanSteps, int Sorts, int AutoIndexRows);

public sealed record SqliteConnectionStat(int CacheHits, int CacheMisses, int CacheWrites, int CacheBytes, int SchemaBytes, int StatementBytes);

public static class SqliteStatHarvest {
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

## [05]-[PLAN_PROFILE]

- Owner: `PlanProfile` captures plan shapes across the three engines; `PlanSubject` discriminates the engine by value shape, `PlanBaselineRow` persists the statement-keyed baseline, and `PlanVerdict` closes the comparison outcome.
- Cases: `PlanSubject` is `Postgres(NpgsqlDataSource, string, Option<long>) | Duck(DuckDBConnection, string) | Sqlite(SqliteConnection, string)`; `PlanVerdict` is `Baselined | Unchanged | Drifted` — a first sighting persists its shape through the injected `baseline` arrow and reads `Baselined`, a match reads `Unchanged`, a moved digest reads `Drifted` carrying both shapes; `PlanRule` mirrors those three outcomes as rows whose `Stable` column marks the two that hold a plan.
- Entry: `PlanProfile.Capture(PlanSubject subject, held, baseline, frame)` returns `PlanVerdict`; `held` and `baseline` are identity-tier arrows filled at composition.
- Auto: each leg folds its engine's plan artifact to a SHAPE-ONLY digest — node kinds, join types, relation and index names for PostgreSQL, the physical-operator tree for DuckDB, the `EXPLAIN QUERY PLAN` detail rows for SQLite — so the digest is run-stable: a flipped join order or a lost index moves it, a slow run does not; statement identity is the pg `queryid` when the `compute_query_id` posture supplies one, else the invariant hash of the statement text.
- Packages: Npgsql, DuckDB.NET.Data.Full, Microsoft.Data.Sqlite, System.IO.Hashing, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a fourth engine is one `PlanSubject` case and one leg; a fourth compare outcome is one `PlanVerdict` case beside one `PlanRule` row whose `Stable` column re-derives the stability share's good half with no pack edit; a richer shape facet is one row in the pg facet list or one decode line; zero new surface — a per-engine capture service or a timing-bearing digest is the deleted form.
- Boundary: the digest preimage carries shape facets only. PostgreSQL `queryid` joins the statement statistics, SQLite counters name the suspect statement, and DuckDB reads `EXPLAIN (FORMAT json)` without arming profiling.

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
public sealed record PlanBaselineRow(PlanEngine Engine, UInt128 StatementKey, UInt128 Shape, Instant At) {
    public Guid Id { get; init; } = Guid.CreateVersion7();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlanProfile {
    public static IO<PlanVerdict> Capture(PlanSubject subject, Func<PlanEngine, UInt128, IO<Option<PlanBaselineRow>>> held, Func<PlanBaselineRow, IO<Unit>> baseline, ProjectionContext frame) =>
        from captured in subject.Switch(postgres: Postgres, duck: Duck, sqlite: Sqlite)
        from prior in held(subject.Engine, captured.Key)
        from verdict in prior.Match(
            Some: row => IO.pure<PlanVerdict>(row.Shape == captured.Shape
                ? new PlanVerdict.Unchanged(captured.Shape)
                : new PlanVerdict.Drifted(row.Shape, captured.Shape)),
            None: () => baseline(new PlanBaselineRow(subject.Engine, captured.Key, captured.Shape, frame.Now()))
                .Map(_ => (PlanVerdict)new PlanVerdict.Baselined(captured.Shape)))
        select verdict;

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

## [06]-[USAGE_PROJECTION]

- Owner: `StoreUsage` derives and persists one `UsageRow` shape over tenant, artifact kind, retention class, storage tier, byte count, object count, delivery count, and observation instant.
- Entry: `StoreUsage.Fold` derives rows from the content catalog and settled egress outcomes; `Dataset`, `Cells`, `Shape`, `Batch`, `Land`, and `Resident` carry the same row through analytical storage and reads.
- Auto: catalog rows group under `(tenant, kind, class, tier)` and settlements contribute delivered counts to the tenant's stream row. The residence columns preserve the class and tier breakdown that capped meter dimensions omit.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a usage axis extends `UsageRow`, `Dataset`, `Cells`, and `Shape` together.
- Boundary: tenant enters as `TenantId`, becomes `TenantContext` once, and remains the RLS partition. `UsageRow` is derived query state rebuilt from the content catalog and durable egress settlements.

```csharp
public readonly record struct UsageRow(
    TenantContext Tenant, string Kind, string Class, string Tier,
    long Bytes, long Objects, long Deliveries, Instant At);

public static class StoreUsage {
    public static TenantContext Tenancy(TenantId partition) =>
        partition == TenantContext.Root.TenantId
            ? TenantContext.Root
            : new(partition, partition.Text);

    public static Seq<UsageRow> Fold(Seq<BlobCatalogRow> catalog, Seq<(TenantId Tenant, Settlement Drain)> drains, ProjectionContext frame) =>
        toSeq(catalog
            .GroupBy(static row => (row.Tenant, row.Kind, row.Class, row.Tier))
            .Select(group => new UsageRow(
                Tenancy(group.Key.Tenant), group.Key.Kind.Key, group.Key.Class.Key, group.Key.Tier.Key,
                group.Sum(static row => row.Bytes), group.Count(), 0L, frame.Now())))
        + toSeq(drains
            .GroupBy(static row => row.Tenant)
            .Select(group => new UsageRow(
                Tenancy(group.Key), string.Empty, RetentionClass.Stream.Key, StorageTier.Standard.Key,
                0L, 0L, group.Sum(static row => (long)row.Drain.Delivered), frame.Now())));

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

    public static Fin<UsageRow> Shape(ResidenceScope scope, ResidenceRow row) =>
        (row.Text(scope.Residence, 0), row.Text(scope.Residence, 1), row.Text(scope.Residence, 2),
            row.Whole(scope.Residence, 3), row.Whole(scope.Residence, 4), row.Whole(scope.Residence, 5),
            row.At(scope.Residence, 6))
        .Apply((kind, retention, tier, bytes, objects, deliveries, at) =>
            new UsageRow(scope.Frame.Tenant, kind, retention, tier, bytes, objects, deliveries, at)).As();

    public static IO<Fin<ResidenceResult<UsageRow>>> Resident(
        ResidenceReach reach, ResidenceScope scope, Seq<(Identifier Column, string Value)> narrow) =>
        ResidencePlan.Scan(Dataset, narrow).Match(
            Succ: plan => ResidenceRead.Read(reach, plan, scope, ResidenceProjection.Aggregate, row => Shape(scope, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<UsageRow>>.Fail(error)));

    public static Seq<ColumnCell> Cells(UsageRow row) =>
        Seq<ColumnCell>(
            new ColumnCell.Text(row.Kind), new ColumnCell.Text(row.Class), new ColumnCell.Text(row.Tier),
            new ColumnCell.Whole(row.Bytes), new ColumnCell.Whole(row.Objects),
            new ColumnCell.Whole(row.Deliveries), new ColumnCell.Moment(row.At));

    public static Fin<RecordBatch> Batch(Seq<UsageRow> rows, ProjectionContext frame) => Batched(rows, frame);

    static Fin<RecordBatch> Batched(Seq<UsageRow> rows, ProjectionContext frame) =>
        ArrowLanding.Build(Dataset, rows, Cells, Seq(
            ("dataset", (string)Dataset.Dataset),
            ("at", InstantPattern.ExtendedIso.Format(frame.Now())),
            ("correlation", frame.Correlation.ToString()),
            ("rows", rows.Count.ToString(CultureInfo.InvariantCulture))));

    public static IO<Fin<ResidenceWrite>> Land(NpgsqlDataSource store, Seq<UsageRow> rows, ProjectionContext frame) =>
        rows.Exists(row => row.Tenant.Entry != frame.Tenant.Entry)
            ? IO.pure(Fin<ResidenceWrite>.Fail(new ResidenceFault.IngestRefused("<tenant-scope>", Dataset.Dataset)))
            : ResidenceLanding.Stage(store, Dataset, rows.Map(Cells), frame);
}
```

## [07]-[STORE_INSTRUMENTS]

- Owner: `StoreInstruments` carries the Persistence `InstrumentSpec` roster and the `TelemetryContributorPort` mint.
- Cases: the kernel instrument kinds carry the roster — advised distributions for statement duration, profiled analytical time across its wall, cpu, and blocked phases, drain duration, the residence read's duration beside the rows its engine scanned, and dead-letter attempt depth whose own observation count IS the dead-letter stream; a plain distribution for profiled row counts (base2-exponential by default); counters for the embedded step tells, the pg buffer-pressure events, the egress settlement stream, the plan-capture stream, the rows a residence dataset landing stages, and object-plane facts keyed by the producing operation; scalar levels for the pg I/O and embedded cache hit ratios; keyed levels for the embedded memory regions and for the tenant usage byte, object, and delivery census, whose root group reports the same three figures untagged on the same three instruments.
- Law: a per-tenant chargeback figure is a LEVEL and a per-asset-class footprint census is a DATASET — the meter carries the cardinality a board polls and the lake carries the product a query groups. Fanning a bounded vocabulary across a keyed level multiplies two bounded axes into a series count neither declared, so the asset-class breakdown rides `#USAGE_PROJECTION`'s landed fact table where no cardinality ceiling exists by law and an operator asks "which asset class costs what" as a GROUP BY, while the meter keeps the tenant-only figure. Bounded × bounded is still a product; the fault, sweep, and object-plane rows carry their bounded dimensions because their instruments are COUNTERS whose series are the tag product alone, never a keyed level family whose cardinality is already the tenant count.
- Entry: `StoreInstruments.Telemetry(string version)` contributes the roster and `StoreDescriptors.Pack` under `TelemetrySource.Persistence`; operation owners write through `ProjectionContext.Instruments` from their canonical result values.
- Auto: rows are pure declarations, and `Rows` reads through the generated `Items` accessor after static initialization.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one measurement extends the roster with its dimensions and bucket policy; bounded variants share one instrument through their discriminant dimension.
- Boundary: instruments mount through the composing root's meter. PostgreSQL and embedded-engine measures are process-global; only usage levels carry the capped tenant dimension, while class and tier remain query columns.

```csharp
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

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Persistence, Version: version, Instruments: Rows,
            Board: StoreDescriptors.Pack);
}

```

## [08]-[STORE_BOARD]

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

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
