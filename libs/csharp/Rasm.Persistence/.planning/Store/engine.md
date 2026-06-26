# [PERSISTENCE_NATIVE_SQLITE]

The embedded engine floor of Rasm.Persistence: one frozen PRAGMA policy axis, one receipted verb family driven by a `FactDecode` delegate algebra, one capability-probe surface, and one closed `SqliteEngineFault` rail over `Microsoft.Data.Sqlite` on the `SQLitePCLRaw.bundle_e_sqlite3` `e_sqlite3` provider, with zero wrapper types between policy and provider and zero re-implementation of a member the catalog already verifies. The page owns the frozen PRAGMA axis with its residency split, the verified compile-flag surface with its admission gate, the maintenance verbs that surface upward as the `StoreOp.Maintain` arm set, the function/aggregate/collation registration rows, the in-memory whole-schema image lane, blob streaming, the cross-process `data_version` change probe, the `DbStatusMetric` connection-residency probe set spanning both the page-cache and lookaside arenas, and the loadable-extension plus the db-config and `sqlite3_limit` hardening gates. AppHost vocabulary arrives settled: every fact stamps from `ClockPolicy`, cadence rides `ScheduleEntry` rows under the maintenance lease, and `DataClassification` routes the encryption demand to the `Store/encryption#SQLITE_KEYING` owner.

Every engine fault is one `SqliteEngineFault` case deriving from `Expected` — the raw-interop extended status `int` folds through `Classify` into a typed case carrying its extended result code and `sqlite3_errstr` text, so the `Terminal` set `Corrupt`/`NotADb`/`Io`/`Full` routes to the lifecycle `Repair` path, the `Retryable` set `Busy`/`Locked`/`CheckpointBusy` re-fires the retry schedule, `ReadOnly` rejects a write on a read-only attach, `SnapshotStale`/`SnapshotRefused`/`BackupStep`/`DeserializeRejected`/`CapabilityMissing` carry their operation-specific evidence, and an unmatched code is the closed `Engine` fallback rather than a silent `Busy` that would retry a non-transient fault forever; a bare `Error.New` for a multi-cause engine domain is the deleted form.

The governing law is `docs/stacks/csharp/domain/durability#EMBEDDED_STORE`: residency-carrying pragma rows, the provider-owned busy budget, `data_version` as the polling-free change probe, the paced raw backup admitted only after a copy `quick_check`, `incremental_vacuum(N)` paged against lock-hold, and the typed `sqlite3_wal_checkpoint_v2` out-parameter receipt are doctrine here, not local invention.

## [01]-[INDEX]

- [01]-[PRAGMA_TABLE]: frozen engine policy rows with residency, before/after facts, one resolve override, and the closed `SqliteEngineFault` classifier.
- [02]-[COMPILE_SURFACE]: verified compile-flag admission, probe receipt, and absent-flag routing.
- [03]-[MAINTENANCE_OPS]: `FactDecode`-driven statement verbs plus the raw-`Handle` capsules (checkpoint, backup, snapshot, image, data-version, the `DbStatusMetric` residency probe), registration rows, and blob streams.
- [04]-[EXTENSION_GATES]: loadable-extension law; the db-config and `sqlite3_limit` hardening axes; vector, sqlean, and the relocated encryption gates.

## [02]-[PRAGMA_TABLE]

- Owner: `SqlitePragma` `[SmartEnum<string>]` frozen policy table carrying value and residency columns; `SqliteFact` with `SqliteFactKind` is the page-wide fact stream every cluster emits onto, `SqliteFactKind` carries each verb's SQL text and its `FactDecode` row, and `FactDecode` owns the result projection as a `[UseDelegateFromConstructor]` `Project` delegate column so `Maintain` dispatches the decode through one `Switch` rather than an imperative arm ladder or a sibling lookup; `SqliteEngineFault` `[Union]` deriving from `Expected` is the closed engine-fault family every raw-interop status folds into.
- Cases: `journal_mode | synchronous | foreign_keys | cache_size | mmap_size | temp_store | wal_autocheckpoint | journal_size_limit | auto_vacuum` — one row per engine knob, residency flagged per row.
- Entry: `public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, FrozenDictionary<SqlitePragma, string> resolved)` — `IO<T>` carries the connection effect; one before/after fact per row; the connection interceptor re-applies the per-connection rows on every physical open while the file-persistent rows prove idempotent re-application.
- Receipt: pragma facts (`Slot` = pragma key, `Before`/`After` captured) fill the open receipt's pragma slots and materialize at the receipt-sink edge through the query rail's interceptor spine.
- Packages: Microsoft.Data.Sqlite, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new engine knob is one `SqlitePragma` row; per-profile variance is one override entry through `Resolve`; a new failure mode is one `SqliteEngineFault` case; zero new surface.
- Boundary: inline PRAGMA text at call sites is the deleted pattern — the store-open ceremony consumes `Resolve` output as its PRAGMA ladder. Residency is a row column per `durability#RITUAL_LAW`: `JournalMode`/`AutoVacuum` are file-persistent (the ritual folds them once at provisioning and re-reads them as idempotent proof, never mutation), while `synchronous`/`cache_size`/`mmap_size`/`temp_store`/`wal_autocheckpoint`/`journal_size_limit`/`foreign_keys` are connection-resident and the interceptor re-applies them on non-pooled opens. `synchronous=NORMAL` is the WAL throughput row whose loss boundary is the last commits and never corruption; `wal_autocheckpoint` bounds WAL growth between scheduled TRUNCATE drains. A `busy_timeout` row is the rejected form: the provider already retries `BUSY`/`LOCKED` at 150 ms quanta until `DefaultTimeout`, so a native sleep beneath the managed loop multiplies the busy budget — the busy budget is the `SqliteConnectionStringBuilder.DefaultTimeout` value owned at `Store/profiles#PROFILE_AXIS`, never a PRAGMA here. A raw-interop status routes by code, not a flat fault — the open ceremony arms `raw.sqlite3_extended_result_codes(connection.Handle, 1)`, and `SqliteEngineFault.Classify` reads `raw.sqlite3_extended_errcode(connection.Handle)` and `raw.sqlite3_errstr` to fold the code into one closed case: `raw.SQLITE_CORRUPT` (11), `raw.SQLITE_NOTADB` (26), `raw.SQLITE_IOERR` (10), and `raw.SQLITE_FULL` (13) are terminal and drive the lifecycle `Repair` restore path, `raw.SQLITE_BUSY` (5)/`raw.SQLITE_BUSY_SNAPSHOT` and `raw.SQLITE_LOCKED` (6) are steady-state retry signals the provider loop owns, and `raw.SQLITE_READONLY` rejects a write on a read-only attach — so a downstream rail folds the closed family rather than parsing a string. `ApplyRow` is the per-row provider seam and stays private to the fence.

```csharp signature
public sealed class SqliteKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [TYPES] ----------------------------------------------------------------------------

// The decode row IS the projection: each case folds the executed command's reader (or a freelist probe)
// into the fact's `(After, Count)` slots, so `Maintain` runs one `row.Decode.Project` over the row with no
// verb-identity `is`/equality ladder. The statement verbs are the entire `Maintain` family; a verb whose raw
// out-params or copy span the `Handle` directly with failure semantics the `(After, Count)` tuple cannot
// carry (`Checkpoint`, `CacheStatus`, `Backup`, `Snapshot`, `Image`, `DataVersion`) is its own typed capsule
// with its own signature, never a hollow `Native` decode smuggled back through an equality probe.
[SmartEnum]
public sealed partial class FactDecode {
    public static readonly FactDecode None = new(static (_, _) => (Option<string>.None, 0L));
    public static readonly FactDecode Scalar = new(static (reader, _) => (reader.Read() ? Some(reader.GetString(0)) : None, 0L));
    public static readonly FactDecode Rows = new(DecodeKernels.JoinRows);
    public static readonly FactDecode FreedPages = new(static (_, connection) => (None, DecodeKernels.FreelistCount(connection)));

    [UseDelegateFromConstructor]
    public partial (Option<string> After, long Count) Project(SqliteDataReader reader, SqliteConnection connection);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class SqliteFactKind {
    public static readonly SqliteFactKind Pragma = new("pragma", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind CompileOption = new("compile-option", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind FunctionList = new("function-list", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Registration = new("registration", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind ExtensionLoad = new("extension-load", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind DbConfig = new("db-config", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Limit = new("limit", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind DataVersion = new("data-version", maintain: false, statement: false, sql: "PRAGMA data_version;", decode: FactDecode.Scalar);
    public static readonly SqliteFactKind CacheStatus = new("cache-status", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Image = new("image", maintain: false, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind QuickCheck = new("quick-check", maintain: true, statement: true, sql: "PRAGMA quick_check;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind IntegrityCheck = new("integrity-check", maintain: true, statement: true, sql: "PRAGMA integrity_check($limit);", decode: FactDecode.Rows);
    public static readonly SqliteFactKind ForeignKeyCheck = new("foreign-key-check", maintain: true, statement: true, sql: "PRAGMA foreign_key_check;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind Optimize = new("optimize", maintain: true, statement: true, sql: "PRAGMA analysis_limit=$analysisLimit; PRAGMA optimize;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind IncrementalVacuum = new("incremental-vacuum", maintain: true, statement: true, sql: "PRAGMA incremental_vacuum($pages);", decode: FactDecode.FreedPages);
    public static readonly SqliteFactKind VacuumInto = new("vacuum-into", maintain: true, statement: true, sql: "VACUUM INTO $destination;", decode: FactDecode.None);
    public static readonly SqliteFactKind Checkpoint = new("checkpoint", maintain: true, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Backup = new("backup", maintain: true, statement: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Snapshot = new("snapshot", maintain: true, statement: false, sql: "", decode: FactDecode.None);

    // `Maintain` flags the scheduled-cadence verbs; `Statement` flags the verbs the one `Maintain` reader entry
    // executes through `Decode.Project`. A `Maintain && !Statement` row (checkpoint, backup, snapshot) is a
    // scheduled verb whose raw-`Handle` capsule and failure semantics live in a dedicated typed entry below.
    public bool Maintain { get; }
    public bool Statement { get; }
    public string Sql { get; }
    public FactDecode Decode { get; }
}

[Union]
public abstract partial record SqliteEngineFault : Expected {
    private SqliteEngineFault(string detail, int code) : base(detail, code, None) { }

    public sealed record Corrupt : SqliteEngineFault { public Corrupt(string detail) : base($"<sqlite-corrupt:{detail}>", raw.SQLITE_CORRUPT) { } }
    public sealed record NotADb : SqliteEngineFault { public NotADb(string detail) : base($"<sqlite-not-a-db:{detail}>", raw.SQLITE_NOTADB) { } }
    public sealed record Io : SqliteEngineFault { public Io(string detail) : base($"<sqlite-io:{detail}>", raw.SQLITE_IOERR) { } }
    public sealed record Full : SqliteEngineFault { public Full(string detail) : base($"<sqlite-full:{detail}>", raw.SQLITE_FULL) { } }
    public sealed record ReadOnly : SqliteEngineFault { public ReadOnly(string detail) : base($"<sqlite-readonly:{detail}>", raw.SQLITE_READONLY) { } }
    public sealed record Busy : SqliteEngineFault { public Busy(string detail) : base($"<sqlite-busy:{detail}>", raw.SQLITE_BUSY) { } }
    public sealed record Locked : SqliteEngineFault { public Locked(string detail) : base($"<sqlite-locked:{detail}>", raw.SQLITE_LOCKED) { } }
    public sealed record CheckpointBusy : SqliteEngineFault { public CheckpointBusy(long log, long done) : base($"<checkpoint-busy:{done}:{log}>", raw.SQLITE_BUSY) { } }
    public sealed record SnapshotRefused : SqliteEngineFault { public SnapshotRefused(string schema, string detail) : base($"<snapshot-open-refused:{schema}:{detail}>", raw.SQLITE_ERROR) { } }
    public sealed record SnapshotStale : SqliteEngineFault { public SnapshotStale(string schema) : base($"<snapshot-stale:{schema}>", raw.SQLITE_ERROR) { } }
    public sealed record BackupStep : SqliteEngineFault { public BackupStep(int code, string detail) : base($"<backup-step:{detail}>", code) { } }
    public sealed record DeserializeRejected : SqliteEngineFault { public DeserializeRejected(string detail) : base($"<deserialize-rejected:{detail}>", raw.SQLITE_ERROR) { } }
    public sealed record CapabilityMissing : SqliteEngineFault { public CapabilityMissing(Seq<string> missing) : base($"<missing-engine-capability:{string.Join(",", missing)}>", raw.SQLITE_ERROR) { } }
    public sealed record Engine : SqliteEngineFault { public Engine(int code, string detail) : base($"<sqlite-engine:{code}:{detail}>", code) { } }

    public bool Terminal => this is Corrupt or NotADb or Io or Full;
    public bool Retryable => this is Busy or Locked or CheckpointBusy;

    // The single boundary fold: a raw status becomes one closed case carrying the engine's own errstr text.
    // The mask reads the EXTENDED code's primary byte for routing (so BUSY_SNAPSHOT (517) routes as BUSY (5))
    // while the detail preserves the full extended code; an unmatched code is `Engine`, never a silent `Busy`
    // that would re-fire the retry schedule against a non-transient fault forever.
    public static SqliteEngineFault Classify(SqliteConnection connection, string where) {
        var extended = raw.sqlite3_extended_errcode(connection.Handle);
        var detail = $"{where}:{raw.sqlite3_errstr(extended).utf8_to_string()}";
        return (extended & 0xFF) switch {
            raw.SQLITE_CORRUPT => new Corrupt(detail),
            raw.SQLITE_NOTADB => new NotADb(detail),
            raw.SQLITE_IOERR => new Io(detail),
            raw.SQLITE_FULL => new Full(detail),
            raw.SQLITE_READONLY => new ReadOnly(detail),
            raw.SQLITE_BUSY => new Busy(detail),
            raw.SQLITE_LOCKED => new Locked(detail),
            var code => new Engine(code, detail),
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class SqlitePragma {
    public static readonly SqlitePragma JournalMode = new("journal_mode", value: "WAL", persistent: true);
    public static readonly SqlitePragma AutoVacuum = new("auto_vacuum", value: "INCREMENTAL", persistent: true);
    public static readonly SqlitePragma Synchronous = new("synchronous", value: "NORMAL", persistent: false);
    public static readonly SqlitePragma ForeignKeys = new("foreign_keys", value: "ON", persistent: false);
    public static readonly SqlitePragma TempStore = new("temp_store", value: "MEMORY", persistent: false);
    public static readonly SqlitePragma CacheSize = new("cache_size", value: "-20000", persistent: false);
    public static readonly SqlitePragma MmapSize = new("mmap_size", value: "268435456", persistent: false);
    public static readonly SqlitePragma WalAutocheckpoint = new("wal_autocheckpoint", value: "1000", persistent: false);
    public static readonly SqlitePragma JournalSizeLimit = new("journal_size_limit", value: "67108864", persistent: false);

    public string Value { get; }
    public bool Persistent { get; }
}

// The connection-residency diagnostic set `sqlite3_db_status` exposes — the page-cache effectiveness triad and the
// lookaside small-allocation arena triad (two PARALLEL pressure axes, page cache vs the per-connection malloc arena)
// plus the schema/stmt footprint. Residency PRESSURE is the hit/miss ratio against used bytes on BOTH arenas, so a
// maintenance sweep's effect is the whole set receipted, never the single used-bytes scalar that proves no pressure.
// The high-water slot carries each metric's peak — the `current`/`highest` pair `sqlite3_db_status` writes per probe.
[SmartEnum<int>]
public sealed partial class DbStatusMetric {
    public static readonly DbStatusMetric CacheUsed = new(raw.SQLITE_DBSTATUS_CACHE_USED, "cache-used");
    public static readonly DbStatusMetric CacheHit = new(raw.SQLITE_DBSTATUS_CACHE_HIT, "cache-hit");
    public static readonly DbStatusMetric CacheMiss = new(raw.SQLITE_DBSTATUS_CACHE_MISS, "cache-miss");
    public static readonly DbStatusMetric CacheWrite = new(raw.SQLITE_DBSTATUS_CACHE_WRITE, "cache-write");
    public static readonly DbStatusMetric LookasideUsed = new(raw.SQLITE_DBSTATUS_LOOKASIDE_USED, "lookaside-used");
    public static readonly DbStatusMetric LookasideHit = new(raw.SQLITE_DBSTATUS_LOOKASIDE_HIT, "lookaside-hit");
    public static readonly DbStatusMetric LookasideMissSize = new(raw.SQLITE_DBSTATUS_LOOKASIDE_MISS_SIZE, "lookaside-miss-size");
    public static readonly DbStatusMetric LookasideMissFull = new(raw.SQLITE_DBSTATUS_LOOKASIDE_MISS_FULL, "lookaside-miss-full");
    public static readonly DbStatusMetric SchemaUsed = new(raw.SQLITE_DBSTATUS_SCHEMA_USED, "schema-used");
    public static readonly DbStatusMetric StmtUsed = new(raw.SQLITE_DBSTATUS_STMT_USED, "stmt-used");
    public static readonly DbStatusMetric DeferredFks = new(raw.SQLITE_DBSTATUS_DEFERRED_FKS, "deferred-fks");

    public string Slot { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct SqliteFact(SqliteFactKind Kind, string Slot, Option<string> Before, Option<string> After, long Count, long Bytes, Duration Elapsed, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class PragmaOps {
    public static FrozenDictionary<SqlitePragma, string> Resolve(HashMap<SqlitePragma, string> overrides) =>
        SqlitePragma.Items.ToFrozenDictionary(static row => row, row => overrides.Find(row).IfNone(row.Value));

    public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, FrozenDictionary<SqlitePragma, string> resolved) =>
        IO.lift(() => SqlitePragma.Items.ToSeq().Map(row => ApplyRow(connection, clocks, row, resolved[row])).Strict());

    static SqliteFact ApplyRow(SqliteConnection connection, ClockPolicy clocks, SqlitePragma row, string value) {
        var mark = clocks.Mark();
        // BOUNDARY ADAPTER — ADO statement seam; PRAGMA read/write is the provider-forced command form.
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA {row.Key};";
        var before = Optional(command.ExecuteScalar()).Map(static found => found.ToString() ?? "");
        command.CommandText = $"PRAGMA {row.Key} = {value}; PRAGMA {row.Key};";
        var after = Optional(command.ExecuteScalar()).Map(static found => found.ToString() ?? "");
        return new SqliteFact(SqliteFactKind.Pragma, row.Key, before, after, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
    }
}
```

## [03]-[COMPILE_SURFACE]

- Owner: `SqliteCompileSurface`
- Entry: `public static IO<(Seq<SqliteFact> Facts, Fin<Unit> Admission)> Probe(SqliteConnection connection, ClockPolicy clocks)` — `Fin<Unit>` aborts the open ceremony with `SqliteEngineFault.CapabilityMissing` carrying the absent flag/function set when an expected engine capability is absent.
- Receipt: one compile-option fact per probed row plus one expected-function fact each; the open receipt's compile-options slot consumes the stream.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, LanguageExt.Core, BCL inbox
- Growth: a newly required engine capability is one row on `ExpectedOptions` or `ExpectedFunctions`; zero new surface.
- Boundary: `Probe` and its `Read` kernel are this fence's boundary capsule over ADO reader ceremony; JSON support is detected through `pragma_function_list`, never `compile_options`; the central `SQLitePCLRaw.bundle_e_sqlite3` pin overrides the provider graph `Microsoft.Data.Sqlite` is tested against, and the bundled `e_sqlite3` emits the `DEFAULT_FOREIGN_KEYS` compile-option without a value suffix while `PRAGMA foreign_keys` defaults on, so the `ExpectedOptions` row carries the bare `DEFAULT_FOREIGN_KEYS` token and a value-suffixed spelling is the rejected form; a custom native SQLite toolchain admission is the rejected escalation for absent flags — every absent concern routes to the named owner below.

```csharp signature
public static class SqliteCompileSurface {
    public static readonly FrozenSet<string> ExpectedOptions = new[] {
        "ENABLE_COLUMN_METADATA",
        "ENABLE_FTS3_PARENTHESIS",
        "ENABLE_FTS4",
        "ENABLE_FTS5",
        "ENABLE_MATH_FUNCTIONS",
        "ENABLE_RTREE",
        "ENABLE_SNAPSHOT",
        "DEFAULT_FOREIGN_KEYS",
        "THREADSAFE=1",
    }.ToFrozenSet(StringComparer.Ordinal);

    public static readonly FrozenSet<string> ExpectedFunctions = new[] { "json_extract", "jsonb" }.ToFrozenSet(StringComparer.Ordinal);

    public static IO<(Seq<SqliteFact> Facts, Fin<Unit> Admission)> Probe(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var options = Read(connection, "SELECT compile_options FROM pragma_compile_options;");
            var functions = Read(connection, "SELECT DISTINCT name FROM pragma_function_list;");
            var missing = ExpectedOptions.Where(flag => !options.Contains(flag)).ToSeq()
                + ExpectedFunctions.Where(name => !functions.Contains(name)).ToSeq();
            var facts = options.Map(flag => new SqliteFact(SqliteFactKind.CompileOption, flag, None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now))
                + ExpectedFunctions.ToSeq().Map(name => new SqliteFact(SqliteFactKind.FunctionList, name, None, None, Count: functions.Contains(name) ? 1 : 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now));
            return (facts, missing.IsEmpty
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new SqliteEngineFault.CapabilityMissing(missing)));
        });

    static Seq<string> Read(SqliteConnection connection, string sql) {
        // BOUNDARY ADAPTER — ADO reader seam over the pragma virtual tables.
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var rows = Seq<string>();
        while (reader.Read())
            rows = rows.Add(reader.GetString(0));
        return rows;
    }
}
```

| [INDEX] | [ABSENT_FLAG]                         | [NAMED_OWNER]                                                  |
| :-----: | ------------------------------------- | -------------------------------------------------------------- |
|  [01]   | SESSION + PREUPDATE_HOOK (changesets) | the op-log HLC changefeed owns diffing                         |
|  [02]   | NORMALIZE (normalized SQL text)       | command-interceptor slow-query receipts own statement evidence |
|  [03]   | DBSTAT virtual table                  | maintenance facts own page-level diagnostics                   |
|  [04]   | SOUNDEX                               | the sqlean-fuzzy deferred gate owns phonetics                  |
|  [05]   | GEOPOLY                               | PostGIS geometry lanes own geodesy                             |

## [04]-[MAINTENANCE_OPS]

- Owner: `SqliteMaintenance`
- Cases: the `Statement` verbs `quick-check | integrity-check | foreign-key-check | optimize | incremental-vacuum | vacuum-into` run through the one `Maintain` reader entry; `checkpoint | backup | snapshot | cache-status | data-version | image` are scheduled-or-probe verbs whose raw-`Handle` out-params and failure semantics outrun the `(After, Count)` tuple, so each is a typed capsule (`Checkpoint`/`Backup`/`WithSnapshot`/`CacheStatus`/`DataVersion`/`Image`) of its own — the `Maintain && !Statement` flag pair names the scheduled-but-not-reader verbs without a hollow `Native` decode.
- Entry: `public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, SqliteMaintenancePolicy policy, Option<string> destination = default)` — the one statement-verb entry whose `ExecuteReader` rows project through `verb.Decode.Project`, so the decode is a `FactDecode`-delegate dispatch over the row; a non-`Statement` verb routed here is admission-rejected once as `CapabilityMissing` naming its dedicated capsule, never an `is`/equality ladder probing the verb identity; the destination arity serves vacuum-into.
- Auto: `optimize` fires at close and the `Checkpoint` `TRUNCATE` form fires at drain through the store lifecycle's band-300 registration; `incremental_vacuum($pages)` arms when `DataVersion`/freelist probes show `freelist_count` exceeds `FreelistThresholdPages` and pages exactly `FreelistThresholdPages` so a sweep never holds the write lock unbounded; recurring cadence rides the persistence-maintenance `ScheduleEntry` row and executes only under the maintenance lease; the integrity ladder runs `quick_check` at open, full `integrity_check($limit)` capped at `IntegrityMaxErrors` on the `Repair` path, and `foreign_key_check` after migrations (FK violations never surface from an integrity check); `PRAGMA optimize` runs its analysis at close behind `PRAGMA analysis_limit=$analysisLimit` so the scan that refreshes `sqlite_stat` is page-bounded and never holds the write lock unbounded on a large table, exactly as `incremental_vacuum($pages)` bounds the vacuum; `CacheStatus` folds the `DbStatusMetric` residency set (`cache_used` bytes and the `cache_hit`/`cache_miss`/`cache_write` page-cache triad, the `lookaside_used`/`lookaside_hit`/`lookaside_miss_size`/`lookaside_miss_full` malloc-arena triad, and the `schema_used`/`stmt_used`/`deferred_fks` footprint) into one fact per metric so a sweep's effect on residency PRESSURE — the hit/miss ratio on BOTH the page cache and the per-connection lookaside arena, not a lone used-bytes scalar — is receipted, not inferred.
- Receipt: rows-decoded verbs carry the engine's result rows in the `After` slot and the row count in `Count` through `verb.Decode.Project`; `incremental_vacuum` carries the freed-page delta in `Count`; the `Checkpoint` capsule carries the `sqlite3_wal_checkpoint_v2` log-frame and checkpointed-frame counts in `Bytes`/`Count` and fails the rail on a non-OK, non-BUSY status rather than receipting an error as success; each `CacheStatus` fact carries one `DbStatusMetric` current/high-water pair in `Bytes`/`Count` keyed by the metric `Slot`, and a non-OK `db_status` on any metric fails the whole probe; paged `Backup` carries total pages in `Count` and remaining in `Bytes`, retries a `BUSY`/`LOCKED` step up to `BackupBusyRetries` times, and surfaces a stepped-out fault or a copy whose `quick_check` is not `ok` on the `Fin` rail (a `Count: -1` sentinel-as-failure fact is the deleted form); every failure surfaces as a `SqliteEngineFault` case, never a bare `Error.New`.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new statement maintenance verb is one `Statement`-flagged `SqliteFactKind` row carrying its `Sql`/`Decode` columns; a new decode shape is one `FactDecode` row with its `Project` delegate; a new residency metric is one `DbStatusMetric` row; a new engine function, aggregate (`CreateAggregate` inside the same `Bind` delegate), or collation is one `FunctionRegistration` row; a new fault mode is one `SqliteEngineFault` case; a raw-`Handle` verb with out-param failure semantics is one typed capsule beside the existing ones; zero new public surface.
- Boundary: `Maintain`, `Register`, `Backup`, `Image`, `DataVersion`, and `WithSnapshot` are this fence's boundary capsules over ADO and raw-interop ceremony; the verbs surface upward as the `StoreOp.Maintain` arm set, never as a service class. The `$destination` path binds a typed `SqliteType.Text` `SqliteParameter` so the vacuum-into bind never widens through `AddWithValue` inference, while the `$pages`/`$limit` page counts substitute as invariant-culture integer literals into the PRAGMA text because a PRAGMA argument is not a bindable parameter in SQLite — the page-count source is the policy row, never raw runtime input. Blob payloads stream through the constructed `SqliteBlob` write stream over a `zeroblob(N)` row preallocation (fixed-size once written) and the `GetStream` read path, so whole-payload `byte[]` materialization is the deleted pattern. The cross-process change probe is `PRAGMA data_version` — it moves only when another connection commits, so an unchanged register proves cache validity without touching tables and a notification bus or table-poll loop is the rejected form. The in-memory whole-schema image rides `raw.sqlite3_serialize(connection.Handle, "main", out size, 0)` out to a `ReadOnlyMemory<byte>` and `raw.sqlite3_deserialize(connection.Handle, "main", ptr, size, size, raw.SQLITE_DESERIALIZE_FREEONCLOSE | raw.SQLITE_DESERIALIZE_RESIZEABLE)` back in, the `sqlite-memory` profile's restore/handoff path distinct from the file-backed content-chunk frame: the `flags=0` serialize mallocs a copy this owner copies once and `raw.sqlite3_free`s (a leaked engine buffer is the deleted form; a `nint.Zero` return fails the rail rather than dereferencing into a crash), while the deserialize `SQLITE_DESERIALIZE_FREEONCLOSE` transfers the marshalled inbound buffer to engine ownership. Paged backup steps the raw backup session over `Handle`: the loop continues while `sqlite3_backup_step` returns `raw.SQLITE_OK`, retries the same step up to `BackupBusyRetries` times on `raw.SQLITE_BUSY`/`raw.SQLITE_LOCKED`, terminates on `raw.SQLITE_DONE`, and surfaces any other code as a `SqliteEngineFault.BackupStep` carrying the code and `errstr` on the `Fin` rail — the `Thread.Sleep` quantum in the step loop is the raw-interop seam exemption, not a domain delay — the provider's whole-file `BackupDatabase` is subsumed because it restarts under other-connection writes, while the paced session yields bounded latency, progress facts, and a copy-side `quick_check` admission that fails the rail when the copy is not `ok` (the step succeeding is never the proof). The `Checkpoint` capsule rides `raw.sqlite3_wal_checkpoint_v2(connection.Handle, "main", mode, out logFrames, out checkpointed)` so the fact carries typed frame counts: a `BUSY` return becomes a `SqliteEngineFault.CheckpointBusy` carrying the frame counts the `_default_checkpoint_retry` schedule re-fires, an `OK` is the receipt, and any other status is a `Classify`d engine fault rather than a success fact carrying an error status — the checkpoint-mode `int` selects among `SQLITE_CHECKPOINT_PASSIVE` (0), `SQLITE_CHECKPOINT_FULL` (1), `SQLITE_CHECKPOINT_RESTART` (2), `SQLITE_CHECKPOINT_TRUNCATE` (3), and a pinned WAL read window and a TRUNCATE are adversaries, so a refused checkpoint is a receipted retry, never a silent rewind. The snapshot bracket opens the deferred read transaction `sqlite3_snapshot_get`/`_open` require before any read, pins a consistent read view under WAL, attempts one `sqlite3_snapshot_recover` pass when the initial pin is refused, rejects a pin older than the optional `floor` through `sqlite3_snapshot_cmp` as `SqliteEngineFault.SnapshotStale` so a monotonic reader never regresses across successive brackets, runs `read` inside the pinned transaction, and rolls the view back while freeing only a held snapshot handle — a pin attempted with no open read transaction is the `SQLITE_ERROR` rejected form. `uuid7` registration is the sqlite leg of the identity policy; `xxh128` projects `XxHash128.HashToUInt128` to the `UInt128` content-identity scalar the whole durability lane keys on — split into its `Low64`/`High64` `long` pair for SQLite's 64-bit integer storage rather than the allocating `byte[] Hash` form the catalog names the non-receipt path — and `isDeterministic: true` admits it into expression indexes and generated columns; `instant_iso` collates persisted ExtendedIso text chronologically through a total `ParseResult.Success` fold (an unparsable side sorts last under a fixed rule) so a malformed cell never throws a `UnparsableValueException` inside the engine's comparator.

```csharp signature
public sealed record SqliteMaintenancePolicy(long FreelistThresholdPages, int BackupStepPages, int IntegrityMaxErrors, int BackupBusyRetries, int AnalysisLimit) {
    public static readonly SqliteMaintenancePolicy Default = new(FreelistThresholdPages: 1000, BackupStepPages: 1024, IntegrityMaxErrors: 100, BackupBusyRetries: 8, AnalysisLimit: 400);
}

public sealed record FunctionRegistration(string Name, Action<SqliteConnection> Bind) {
    public static readonly Seq<FunctionRegistration> Rows = Seq(
        new FunctionRegistration("uuid7", static connection => connection.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("D"), isDeterministic: false)),
        // `HashToUInt128` is the catalog content-identity primitive; the low/high split lands the 128-bit key in two
        // SQLite INTEGER cells without an allocating `byte[]` round trip, so a generated content-key column derives in-engine.
        new FunctionRegistration("xxh128_lo", static connection => connection.CreateFunction("xxh128_lo", static (byte[] payload) => unchecked((long)(ulong)XxHash128.HashToUInt128(payload)), isDeterministic: true)),
        new FunctionRegistration("xxh128_hi", static connection => connection.CreateFunction("xxh128_hi", static (byte[] payload) => unchecked((long)(ulong)(XxHash128.HashToUInt128(payload) >> 64)), isDeterministic: true)),
        new FunctionRegistration("instant_iso", static connection => connection.CreateCollation("instant_iso", static (left, right) => InstantOrder(left, right))));

    // Total collation: an unparsable instant text sorts after every parsable one and ties on parse-failure, never throwing in-engine.
    static int InstantOrder(string left, string right) {
        var (l, r) = (InstantPattern.ExtendedIso.Parse(left), InstantPattern.ExtendedIso.Parse(right));
        return (l.Success, r.Success) switch {
            (true, true) => l.Value.CompareTo(r.Value),
            (true, false) => -1,
            (false, true) => 1,
            _ => string.CompareOrdinal(left, right),
        };
    }
}

public static class SqliteMaintenance {
    public static IO<Seq<SqliteFact>> Register(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => FunctionRegistration.Rows.Map(row => {
            var mark = clocks.Mark();
            row.Bind(connection);
            return new SqliteFact(SqliteFactKind.Registration, row.Name, None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        }));

    // One reader-path dispatch over every `Statement` verb — `Decode.Project` owns the projection, with no
    // verb-identity ladder. A non-statement verb routed here is admission-rejected once (its raw-`Handle`
    // capsule is the call), never a silent fallthrough; `Checkpoint`/`Backup`/`Snapshot`/`CacheStatus` are
    // their own typed entries below because their out-params and failure semantics outrun the `(After, Count)` row.
    public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, SqliteMaintenancePolicy policy, Option<string> destination = default) =>
        verb.Statement
            ? IO.lift(() => {
                var mark = clocks.Mark();
                // BOUNDARY ADAPTER — ADO statement seam; the maintenance verb is a provider-forced command.
                using var command = connection.CreateCommand();
                command.CommandText = verb.Sql
                    .Replace("$pages", policy.FreelistThresholdPages.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                    .Replace("$limit", policy.IntegrityMaxErrors.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                    .Replace("$analysisLimit", policy.AnalysisLimit.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                destination.Iter(path => command.Parameters.Add(new SqliteParameter { ParameterName = "$destination", SqliteType = SqliteType.Text, Value = path }));
                using var reader = command.ExecuteReader();
                var (after, count) = verb.Decode.Project(reader, connection);
                return new SqliteFact(verb, verb.Key, None, after, count, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
            })
            : IO.fail<SqliteFact>(new SqliteEngineFault.CapabilityMissing(Seq($"non-statement-verb-has-dedicated-capsule:{verb.Key}")));

    // One fact per residency metric so cache pressure (hit/miss/write against used bytes) is receipted as a set,
    // never inferred from a lone used-bytes scalar; a non-OK `db_status` on any metric fails the whole probe.
    // `IO.lift(Func<Fin<A>>)` threads the `Fin` failure onto the IO rail directly — no `IO<Fin<A>>` double-wrap.
    public static IO<Seq<SqliteFact>> CacheStatus(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            return DbStatusMetric.Items.ToSeq()
                .Map(metric => {
                    var status = raw.sqlite3_db_status(connection.Handle, metric.Key, out var current, out var highest, 0);
                    return status == raw.SQLITE_OK
                        ? Fin.Succ(new SqliteFact(SqliteFactKind.CacheStatus, metric.Slot, None, Some(current.ToString(CultureInfo.InvariantCulture)), Count: highest, Bytes: current, clocks.Elapsed(mark), clocks.Now))
                        : Fin.Fail<SqliteFact>(SqliteEngineFault.Classify(connection, $"cache-status:{metric.Slot}"));
                })
                .TraverseM(identity).As().Map(static facts => facts.Strict());
        });

    public static IO<SqliteFact> Checkpoint(SqliteConnection connection, ClockPolicy clocks, int mode) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var status = raw.sqlite3_wal_checkpoint_v2(connection.Handle, "main", mode, out var logFrames, out var checkpointed);
            return (Status: status, Log: logFrames, Done: checkpointed, Mark: mark);
        }).Bind(step =>
            // BUSY is a pinned-reader refusal the schedule re-fires; OK is the receipt; any other status is a typed
            // engine fault, never a silent "checkpointed" success fact carrying an error status in its `Before` slot.
            step.Status switch {
                raw.SQLITE_BUSY => IO.fail<SqliteFact>(new SqliteEngineFault.CheckpointBusy(step.Log, step.Done)),
                raw.SQLITE_OK => IO.pure(new SqliteFact(SqliteFactKind.Checkpoint, "main",
                    Before: None, After: Some("checkpointed"),
                    Count: step.Done, Bytes: step.Log, clocks.Elapsed(step.Mark), clocks.Now)),
                var status => IO.fail<SqliteFact>(SqliteEngineFault.Classify(connection, $"checkpoint:{status}")),
            })
            .Retry(_default_checkpoint_retry);

    public static IO<SqliteFact> DataVersion(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            // BOUNDARY ADAPTER — ADO scalar seam; data_version is the polling-free change register.
            using var command = connection.CreateCommand();
            command.CommandText = SqliteFactKind.DataVersion.Sql;
            var register = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            return new SqliteFact(SqliteFactKind.DataVersion, "data_version", None, Some(register.ToString(CultureInfo.InvariantCulture)), Count: register, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        });

    // The paged copy surfaces on the rail: a stepped-out fault or a copy whose `quick_check` is not `ok` fails the
    // IO so the lifecycle retries or restores — a `Count: -1` sentinel-as-failure fact is the deleted form, because
    // the verb stepping to DONE is never the proof. The pagecount/remaining ride the success fact as progress.
    public static IO<SqliteFact> Backup(SqliteConnection source, SqliteConnection destination, ClockPolicy clocks, SqliteMaintenancePolicy policy) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var session = raw.sqlite3_backup_init(destination.Handle, "main", source.Handle, "main");
            try {
                // BOUNDARY ADAPTER — raw paged backup seam; OK continues, DONE completes, BUSY/LOCKED retries the step.
                // `_pagecount`/`_remaining` read only after the first step populates them, so they reflect the real copy.
                return StepUntilDone(session, policy)
                    .Bind(_ => VerifyCopy(destination))
                    .Map(verdict => new SqliteFact(SqliteFactKind.Backup, "main", None, Some(verdict),
                        Count: raw.sqlite3_backup_pagecount(session), Bytes: raw.sqlite3_backup_remaining(session), clocks.Elapsed(mark), clocks.Now));
            } finally {
                raw.sqlite3_backup_finish(session);
            }
        });

    // One polymorphic image entry discriminating on the inbound shape: present bytes deserialize, absent bytes
    // serialize — `Some` outbound on serialize, `None` on deserialize; the `Fin` body threads onto the IO rail
    // through `IO.lift(Func<Fin<A>>)` exactly as the other capsules, never a double-wrapped `IO<Fin<A>>`.
    public static IO<(SqliteFact Fact, Option<ReadOnlyMemory<byte>> Image)> Image(SqliteConnection connection, ClockPolicy clocks, Option<ReadOnlyMemory<byte>> inbound = default) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            return inbound is { IsSome: true, Case: ReadOnlyMemory<byte> bytes }
                ? Deserialize(connection, bytes, clocks, mark).Map(fact => (fact, Option<ReadOnlyMemory<byte>>.None))
                : Serialize(connection, clocks, mark);
        });

    // `sqlite3_serialize` with `flags=0` mallocs a copy the caller owns: `nint.Zero` is a failed image (OOM or an
    // unserializable schema), and the engine buffer is `sqlite3_free`d after the managed copy — a leaked buffer is
    // the deleted form, distinct from the `SQLITE_SERIALIZE_NOCOPY` path whose pointer the engine keeps.
    static Fin<(SqliteFact Fact, Option<ReadOnlyMemory<byte>> Image)> Serialize(SqliteConnection connection, ClockPolicy clocks, long mark) {
        var pointer = raw.sqlite3_serialize(connection.Handle, "main", out var size, 0);
        if (pointer == nint.Zero)
            return Fin.Fail<(SqliteFact, Option<ReadOnlyMemory<byte>>)>(new SqliteEngineFault.DeserializeRejected($"serialize-null:{size}"));
        try {
            var image = new byte[size];
            Marshal.Copy(pointer, image, 0, (int)size);
            return Fin.Succ((new SqliteFact(SqliteFactKind.Image, "serialize", None, None, Count: 0, Bytes: size, clocks.Elapsed(mark), clocks.Now), Some<ReadOnlyMemory<byte>>(image)));
        } finally {
            raw.sqlite3_free(pointer);
        }
    }

    static Fin<SqliteFact> Deserialize(SqliteConnection connection, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, long mark) {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes.ToArray(), 0, pointer, bytes.Length);
        // SQLITE_DESERIALIZE_FREEONCLOSE transfers the marshalled buffer to engine ownership — it frees the pointer even on a rejected load.
        var status = raw.sqlite3_deserialize(connection.Handle, "main", pointer, bytes.Length, bytes.Length,
            raw.SQLITE_DESERIALIZE_FREEONCLOSE | raw.SQLITE_DESERIALIZE_RESIZEABLE);
        return status == raw.SQLITE_OK
            ? Fin.Succ(new SqliteFact(SqliteFactKind.Image, "deserialize", None, Some("loaded"), Count: 0, Bytes: bytes.Length, clocks.Elapsed(mark), clocks.Now))
            : Fin.Fail<SqliteFact>(new SqliteEngineFault.DeserializeRejected(raw.sqlite3_errstr(status).utf8_to_string()));
    }

    // `sqlite3_snapshot_get`/`_open` are contract-bound to a DEFERRED read transaction that has not yet read, so the
    // bracket begins one, pins under it, reads inside it, and rolls it back — a snapshot get with no open read
    // transaction is a `SQLITE_ERROR`. The handle frees in `finally`; only a held pin is
    // freed. The `read` `Fin<T>` threads onto the IO rail through `IO.lift(Func<Fin<A>>)`, never a `IO<Fin<T>>` wrap.
    public static IO<T> WithSnapshot<T>(SqliteConnection connection, string schema, Func<SqliteConnection, Fin<T>> read, Option<sqlite3_snapshot> floor = default) =>
        IO.lift<T>(() => {
            // BOUNDARY ADAPTER — the deferred read-transaction seam the snapshot pin requires before any read.
            using var view = connection.BeginTransaction(deferred: true);
            var status = raw.sqlite3_snapshot_get(connection.Handle, schema, out var snapshot);
            if (status != raw.SQLITE_OK && raw.sqlite3_snapshot_recover(connection.Handle, schema) == raw.SQLITE_OK)
                status = raw.sqlite3_snapshot_get(connection.Handle, schema, out snapshot);
            try {
                return status != raw.SQLITE_OK
                    ? Fin.Fail<T>(new SqliteEngineFault.SnapshotRefused(schema, raw.sqlite3_errstr(status).utf8_to_string()))
                    : floor is { IsSome: true, Case: sqlite3_snapshot since } && raw.sqlite3_snapshot_cmp(snapshot, since) < 0
                        ? Fin.Fail<T>(new SqliteEngineFault.SnapshotStale(schema))
                        : raw.sqlite3_snapshot_open(connection.Handle, schema, snapshot) == raw.SQLITE_OK
                            ? read(connection)
                            : Fin.Fail<T>(new SqliteEngineFault.SnapshotRefused(schema, raw.sqlite3_errstr(raw.sqlite3_extended_errcode(connection.Handle)).utf8_to_string()));
            } finally {
                if (snapshot is { } held)
                    raw.sqlite3_snapshot_free(held);
                view.Rollback();
            }
        });

    public static Stream BlobStream(SqliteConnection connection, string table, string column, long rowid, bool readOnly = false) =>
        new SqliteBlob(connection, table, column, rowid, readOnly);

    public static Stream BlobStream(SqliteDataReader reader, int ordinal) =>
        reader.GetStream(ordinal);

    // The copy admission: a copy whose `quick_check` is not `ok` is refused on the rail, never receipted complete.
    static Fin<string> VerifyCopy(SqliteConnection destination) {
        using var command = destination.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";
        var verdict = command.ExecuteScalar()?.ToString() ?? "";
        return verdict == "ok"
            ? Fin.Succ("complete")
            : Fin.Fail<string>(new SqliteEngineFault.Corrupt($"backup-copy-quick-check:{verdict}"));
    }

    static Fin<Unit> StepUntilDone(sqlite3_backup session, SqliteMaintenancePolicy policy) {
        var attempt = 0;
        for (var code = raw.sqlite3_backup_step(session, policy.BackupStepPages); code != raw.SQLITE_DONE; code = raw.sqlite3_backup_step(session, policy.BackupStepPages)) {
            if (code == raw.SQLITE_OK) { attempt = 0; continue; }
            if ((code == raw.SQLITE_BUSY || code == raw.SQLITE_LOCKED) && attempt++ < policy.BackupBusyRetries) { Thread.Sleep(20); continue; }
            return Fin.Fail<Unit>(new SqliteEngineFault.BackupStep(code, raw.sqlite3_errstr(code).utf8_to_string()));
        }
        return Fin.Succ(unit);
    }

    static readonly Schedule _default_checkpoint_retry = Schedule.recurs(8) | Schedule.exponential(TimeSpan.FromMilliseconds(20)) | Schedule.maxDelay(TimeSpan.FromMilliseconds(250));
}

// --- [DECODE_KERNELS] -------------------------------------------------------------------

// The reader-projection bodies the `FactDecode` rows bind: row text join for the check verbs, freelist probe for vacuum.
static class DecodeKernels {
    public static (Option<string> After, long Count) JoinRows(SqliteDataReader reader, SqliteConnection connection) {
        var rows = Seq<string>();
        while (reader.Read())
            rows = rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue)));
        return (Optional(string.Join(";", rows)).Filter(static text => text.Length > 0), rows.Count);
    }

    public static long FreelistCount(SqliteConnection connection) {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA freelist_count;";
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }
}
```

## [05]-[EXTENSION_GATES]

- Owner: `ExtensionGate` the loadable-extension row table; `DbConfig` and `SqliteLimit` the two per-connection hardening axes — db_config policy flags and `sqlite3_limit` resource ceilings — folded once per physical open.
- Cases: `vec0` gated | `sqlean-regexp`, `sqlean-crypto`, `sqlean-fuzzy`, `sqlean-uuid`, `sqlean-stats`, `sqlean-text` deferred | `sqlcipher` gated (the keying ceremony and the `EncryptionGate` owner relocated to `Store/encryption#SQLITE_KEYING`, bound to a KMS-unwrapped DEK).
- Entry: `public static IO<SqliteFact> Load(SqliteConnection connection, ClockPolicy clocks, ExtensionGate gate)` loads through the OS loader; `DbConfig.Apply` and `SqliteLimit.Apply` fold the hardening axes once per physical open before any user statement.
- Receipt: one extension-load fact per opened gate (`Slot` = gate name, `After` = entrypoint); a refused load receipts `Before` = the converted provider message, `After` = the gate's `FallbackLane`, and `Count` = -1 so the brute-force lane arms on evidence rather than an escaped exception; one limit fact per `SqliteLimit` row (`Slot` = limit name, `After`/`Count` = the applied floor); the encryption ceremony receipts `cipher_version` on the same stream at its owning page.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new loadable concern is one `ExtensionGate` row naming artifact route and fallback lane; a new per-connection config posture is one `DbConfig` row and a new resource ceiling is one `SqliteLimit` row; future cr-sqlite admission lands as one gate row plus merge-law rows on the sync owner, never a transport case; zero new surface.
- Boundary: `ExtensionOps.Load` calls only `LoadExtension`, which arms `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` for the C-API loader and leaves the SQL-level `load_extension()` function off — `connection.EnableExtensions(true)` is the rejected call because it routes `raw.sqlite3_enable_load_extension`, the C-API global toggle that re-enables the SQL function the defensive posture exists to deny, so the loadable-extension arming rides db_config alone; `DbConfig.Apply` folds per-connection config flags through `raw.sqlite3_db_config` (the int-flag `(sqlite3, int, int, out int)` overload) so defensive-mode, double-quoted-string-literal rejection, and the load-extension arming are connection policy values rather than connection-string knobs, applied once per physical open before any user statement, with the `ENABLE_LOAD_EXTENSION` row pre-arming the same db_config flag `LoadExtension` would set so the loader call is idempotent (`DQS=0` makes a double-quoted string a prepare-time syntax error — identifiers quote with `"`, strings with `'`); `SqliteLimit.Apply` folds the resource-bound hardening axis through `raw.sqlite3_limit` on the same physical open so the parser/expression/attach/variable ceilings bound an untrusted restore blob or sync batch beside the db_config posture, never a connection-string knob. The OS loader resolves artifacts directly — absolute path or loader-path variable, with the NuGet RID convention copying the `osx-arm64` payload to output and every off-platform RID asset dropped under the single-RID target so the `e_sqlite3` bundle and the loadable-extension payloads ship as the `osx-arm64` dylib set only; the vector gate never deletes the brute-force fallback case; `jsonb_*` blob functions are an in-process raw-SQL fast path that never crosses a seam while the EF mapping stays TEXT json; the encryption key handle, the SQLCipher keying surface (`PRAGMA key`, `cipher_migrate`, `cipher_version`, `rekey` over the `SQLite3Provider_sqlcipher` provider), and the KMS-unwrapped DEK binding are owned at `Store/encryption#SQLITE_KEYING` where the promoted `EncryptionGate` binds the ceremony to a KMS-unwrapped DEK, so the `sqlcipher` gate row here is `ExtensionGateState.Gated` (no longer Research) and this page carries only the loadable-extension and db-config posture, never the keying record or a `Password` connection-string row (the admitted `e_sqlite3` bundle has no cipher and fails at open); `sqlean-math` stays rejected (the compile flag owns it) and `Microsoft.SemanticKernel.Connectors.SqliteVec` stays rejected as a thin loader dragging a foreign graph.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class ExtensionGateState {
    public static readonly ExtensionGateState Gated = new("gated");
    public static readonly ExtensionGateState Deferred = new("deferred");
    public static readonly ExtensionGateState Research = new("research");
}

public sealed record ExtensionGate(string Name, Option<string> EntryPoint, string Artifact, ExtensionGateState State, Option<string> FallbackLane) {
    public static readonly Seq<ExtensionGate> Rows = Seq(
        new ExtensionGate("vec0", Some("sqlite3_vec_init"), "sqlite-vec package runtimes/osx-arm64/native payload; checksum-verified upstream loadable tarball when the package lags", ExtensionGateState.Gated, Some("vector brute-force fallback")),
        new ExtensionGate("sqlean-regexp", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, None),
        new ExtensionGate("sqlean-crypto", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("xxh128_lo/xxh128_hi function rows for non-cryptographic identity")),
        new ExtensionGate("sqlean-fuzzy", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("pg_trgm and fuzzystrmatch on the server lanes")),
        new ExtensionGate("sqlean-uuid", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("uuid7 function row")),
        new ExtensionGate("sqlean-stats", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("analytical-lane percentiles")),
        new ExtensionGate("sqlean-text", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, None),
        new ExtensionGate("sqlcipher", None, "SQLite3Provider_sqlcipher provider with an externally supplied native library; the batteries bundle route is deprecated and the two routes never mix; keying ceremony owned at Store/encryption#SQLITE_KEYING", ExtensionGateState.Gated, Some("plaintext default")));
}

public sealed record DbConfig(string Slot, int Op, int Value) {
    public static readonly Seq<DbConfig> Hardened = Seq(
        new DbConfig("defensive", raw.SQLITE_DBCONFIG_DEFENSIVE, 1),
        new DbConfig("dqs-ddl", raw.SQLITE_DBCONFIG_DQS_DDL, 0),
        new DbConfig("dqs-dml", raw.SQLITE_DBCONFIG_DQS_DML, 0),
        new DbConfig("trigger", raw.SQLITE_DBCONFIG_ENABLE_TRIGGER, 1),
        new DbConfig("view", raw.SQLITE_DBCONFIG_ENABLE_VIEW, 1),
        new DbConfig("load-extension", raw.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, 1));

    // The fourth `out int` param echoes the applied flag, so the hardening fold receipts each posture rather than
    // applying it silently — a non-OK status `Classify`s the engine fault rather than swallowing a refused config.
    public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, Seq<DbConfig> rows) =>
        IO.lift(() => rows.Map(row => {
            var mark = clocks.Mark();
            var status = raw.sqlite3_db_config(connection.Handle, row.Op, row.Value, out var applied);
            return status == raw.SQLITE_OK
                ? Fin.Succ(new SqliteFact(SqliteFactKind.DbConfig, row.Slot, Some(row.Value.ToString(CultureInfo.InvariantCulture)), Some(applied.ToString(CultureInfo.InvariantCulture)), Count: applied, Bytes: 0, clocks.Elapsed(mark), clocks.Now))
                : Fin.Fail<SqliteFact>(SqliteEngineFault.Classify(connection, $"db-config:{row.Slot}"));
        }).TraverseM(identity).As().Map(static facts => facts.Strict()));
}

// The resource-bound hardening axis db_config flags cannot carry — `sqlite3_limit(id, newVal)` caps the parser,
// expression, attach, and bound-variable surfaces so an untrusted restore blob or sync batch cannot exhaust the
// engine, applied once per physical open beside `DbConfig`. `newVal < 0` is a no-op probe; the cap is the floor of
// the requested ceiling and the compiled default, so a row never RAISES a limit above the build hardening.
[SmartEnum<int>]
public sealed partial class SqliteLimit {
    public static readonly SqliteLimit SqlLength = new(raw.SQLITE_LIMIT_SQL_LENGTH, "sql-length", 1_000_000);
    public static readonly SqliteLimit Length = new(raw.SQLITE_LIMIT_LENGTH, "blob-length", 200_000_000);
    public static readonly SqliteLimit Column = new(raw.SQLITE_LIMIT_COLUMN, "column", 2_000);
    public static readonly SqliteLimit ExprDepth = new(raw.SQLITE_LIMIT_EXPR_DEPTH, "expr-depth", 1_000);
    public static readonly SqliteLimit CompoundSelect = new(raw.SQLITE_LIMIT_COMPOUND_SELECT, "compound-select", 500);
    public static readonly SqliteLimit VdbeOp = new(raw.SQLITE_LIMIT_VDBE_OP, "vdbe-op", 250_000);
    public static readonly SqliteLimit Attached = new(raw.SQLITE_LIMIT_ATTACHED, "attached", 1);
    public static readonly SqliteLimit Variable = new(raw.SQLITE_LIMIT_VARIABLE_NUMBER, "variable", 32_766);
    public static readonly SqliteLimit TriggerDepth = new(raw.SQLITE_LIMIT_TRIGGER_DEPTH, "trigger-depth", 100);

    public string Slot { get; }
    public int Ceiling { get; }

    // `sqlite3_limit(id, -1)` probes the prior limit without mutating; `sqlite3_limit(id, min(prior, ceiling))` then
    // lowers only — a host whose compiled SQLITE_MAX_* is already tighter is never widened — and returns the prior
    // value again, so the receipt carries `Before` = prior and `After` = the applied floor the engine now enforces.
    public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => SqliteLimit.Items.ToSeq().Map(limit => {
            var mark = clocks.Mark();
            var prior = raw.sqlite3_limit(connection.Handle, limit.Key, -1);
            var floor = Math.Min(prior, limit.Ceiling);
            _ = raw.sqlite3_limit(connection.Handle, limit.Key, floor);
            return new SqliteFact(SqliteFactKind.Limit, limit.Slot, Some(prior.ToString(CultureInfo.InvariantCulture)), Some(floor.ToString(CultureInfo.InvariantCulture)), Count: floor, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        }).Strict());
}

public static class ExtensionOps {
    // `LoadExtension` alone arms `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` for the C-API loader and never the SQL function:
    // `connection.EnableExtensions(true)` is the rejected call because it routes `raw.sqlite3_enable_load_extension`, which
    // re-enables the SQL-level `load_extension()` the defensive posture exists to deny — so the db_config arming is the only
    // arming, and `DbConfig.Hardened` pre-arms the same flag at hardening time so the loader call is idempotent.
    // A missing or unsigned dylib surfaces a provider exception at the loader seam; it converts to the engine-fault rail
    // exactly once here so the gate's `FallbackLane` decision rides a typed case, never an escaped `SqliteException`.
    public static IO<SqliteFact> Load(SqliteConnection connection, ClockPolicy clocks, ExtensionGate gate) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            return Try.lift(() => {
                connection.LoadExtension(gate.Name, gate.EntryPoint is { IsSome: true, Case: string entry } ? entry : null);
                return new SqliteFact(SqliteFactKind.ExtensionLoad, gate.Name, None, gate.EntryPoint, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
            }).Run().IfFail(error => new SqliteFact(SqliteFactKind.ExtensionLoad, gate.Name, Some(error.Message), gate.FallbackLane, Count: -1, Bytes: 0, clocks.Elapsed(mark), clocks.Now));
        });
}
```

## [06]-[RESEARCH]

- [EXTENSION_LOADING]: `vec0` live load with the `vec_version()` fact and the package-payload versus vendored-tarball sourcing decision; hardened-runtime `dlopen` acceptance of unsigned extension dylibs inside the signed Rhino host process; the `SQLite3Provider_sqlcipher` provider route with an externally supplied native library on `osx-arm64` and its crypto-backend notice set (the keying ceremony itself is owned and researched at `Store/encryption#SQLITE_KEYING`).
- [DB_CONFIG_OPS]: the live ordering of the `DbConfig.Hardened` set against pooled physical opens — whether defensive mode and the double-quoted-string-literal DDL/DML rejection must precede the migration ladder or apply after schema creation, and the interaction of `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` enablement with the db_config extension-arming path on the same connection.
- [MEMORY_IMAGE]: the `sqlite3_serialize`/`sqlite3_deserialize` round-trip for the `sqlite-memory` profile's restore and cross-process handoff — whether the `SQLITE_DESERIALIZE_FREEONCLOSE | SQLITE_DESERIALIZE_RESIZEABLE` flag pair transfers the marshalled image to engine ownership cleanly and the deserialized in-memory store survives a subsequent `PRAGMA optimize`, proven against `e_sqlite3` 3.50.4 before the image fence pins.
