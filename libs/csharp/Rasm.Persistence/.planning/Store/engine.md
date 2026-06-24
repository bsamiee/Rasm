# [PERSISTENCE_NATIVE_SQLITE]

The embedded engine floor of Rasm.Persistence: one frozen PRAGMA policy axis, one receipted verb family, and one capability-probe surface over `Microsoft.Data.Sqlite` on the `SQLitePCLRaw.bundle_e_sqlite3` `e_sqlite3` provider, with zero wrapper types between policy and provider and zero re-implementation of a member the catalog already verifies. The page owns the frozen PRAGMA axis with its residency split, the verified compile-flag surface with its admission gate, the maintenance verbs that surface upward as the `StoreOp.Maintain` arm set, the function/aggregate/collation registration rows, the in-memory whole-schema image lane, blob streaming, the cross-process `data_version` change probe, and the loadable-extension and db-config hardening gates. AppHost vocabulary arrives settled: every fact stamps from `ClockPolicy`, cadence rides `ScheduleEntry` rows under the maintenance lease, and `DataClassification` routes the encryption demand to the `Store/encryption#SQLITE_KEYING` owner.

The governing law is `docs/stacks/csharp/domain/durability#EMBEDDED_STORE`: residency-carrying pragma rows, the provider-owned busy budget, `data_version` as the polling-free change probe, the paced raw backup admitted only after a copy `quick_check`, `incremental_vacuum(N)` paged against lock-hold, and the typed `sqlite3_wal_checkpoint_v2` out-parameter receipt are doctrine here, not local invention.

## [01]-[INDEX]

- [01]-[PRAGMA_TABLE]: frozen engine policy rows with residency, before/after facts, and one resolve override.
- [02]-[COMPILE_SURFACE]: verified compile-flag admission, probe receipt, and absent-flag routing.
- [03]-[MAINTENANCE_OPS]: receipted verbs with native out-channels, registration rows, blob/image streams, the change probe, and the snapshot bracket.
- [04]-[EXTENSION_GATES]: loadable-extension law; vector, sqlean, and the relocated encryption gates.

## [02]-[PRAGMA_TABLE]

- Owner: `SqlitePragma` `[SmartEnum<string>]` frozen policy table carrying value and residency columns; `SqliteFact` with `SqliteFactKind` is the page-wide fact stream every cluster emits onto, and `SqliteFactKind` carries each verb's SQL and result decode as `[UseDelegateFromConstructor]` behavior columns rather than a sibling lookup.
- Cases: `journal_mode | synchronous | foreign_keys | cache_size | mmap_size | temp_store | wal_autocheckpoint | journal_size_limit | auto_vacuum` — one row per engine knob, residency flagged per row.
- Entry: `public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, FrozenDictionary<SqlitePragma, string> resolved)` — `IO<T>` carries the connection effect; one before/after fact per row; the connection interceptor re-applies the per-connection rows on every physical open while the file-persistent rows prove idempotent re-application.
- Receipt: pragma facts (`Slot` = pragma key, `Before`/`After` captured) fill the open receipt's pragma slots and materialize at the receipt-sink edge through the query rail's interceptor spine.
- Packages: Microsoft.Data.Sqlite, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new engine knob is one `SqlitePragma` row; per-profile variance is one override entry through `Resolve`; zero new surface.
- Boundary: inline PRAGMA text at call sites is the deleted pattern — the store-open ceremony consumes `Resolve` output as its PRAGMA ladder. Residency is a row column per `durability#RITUAL_LAW`: `JournalMode`/`AutoVacuum` are file-persistent (the ritual folds them once at provisioning and re-reads them as idempotent proof, never mutation), while `synchronous`/`cache_size`/`mmap_size`/`temp_store`/`wal_autocheckpoint`/`journal_size_limit`/`foreign_keys` are connection-resident and the interceptor re-applies them on non-pooled opens. `synchronous=NORMAL` is the WAL throughput row whose loss boundary is the last commits and never corruption; `wal_autocheckpoint` bounds WAL growth between scheduled TRUNCATE drains. A `busy_timeout` row is the rejected form: the provider already retries `BUSY`/`LOCKED` at 150 ms quanta until `DefaultTimeout`, so a native sleep beneath the managed loop multiplies the busy budget — the busy budget is the `SqliteConnectionStringBuilder.DefaultTimeout` value owned at `Store/profiles#PROFILE_AXIS`, never a PRAGMA here. A `SqliteException` carrying an extended result code routes by code, not a flat fault — the open ceremony arms `raw.sqlite3_extended_result_codes(connection.Handle, 1)` and `raw.sqlite3_errstr` decodes the code into the fact: `raw.SQLITE_CORRUPT` (11) and `raw.SQLITE_NOTADB` (26) are terminal and drive the lifecycle `Repair` restore path, while `raw.SQLITE_BUSY` (5) and `raw.SQLITE_LOCKED` (6) are steady-state retry signals the provider loop owns. `ApplyRow` is the per-row provider seam and stays private to the fence.

```csharp signature
public sealed class SqliteKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class SqliteFactKind {
    public static readonly SqliteFactKind Pragma = new("pragma", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind CompileOption = new("compile-option", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind FunctionList = new("function-list", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind Registration = new("registration", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind ExtensionLoad = new("extension-load", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind DataVersion = new("data-version", maintain: false, sql: "PRAGMA data_version;", decode: FactDecode.Scalar);
    public static readonly SqliteFactKind Image = new("image", maintain: false, sql: "", decode: FactDecode.None);
    public static readonly SqliteFactKind QuickCheck = new("quick-check", maintain: true, sql: "PRAGMA quick_check;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind IntegrityCheck = new("integrity-check", maintain: true, sql: "PRAGMA integrity_check($limit);", decode: FactDecode.Rows);
    public static readonly SqliteFactKind ForeignKeyCheck = new("foreign-key-check", maintain: true, sql: "PRAGMA foreign_key_check;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind Optimize = new("optimize", maintain: true, sql: "PRAGMA optimize;", decode: FactDecode.Rows);
    public static readonly SqliteFactKind IncrementalVacuum = new("incremental-vacuum", maintain: true, sql: "PRAGMA incremental_vacuum($pages);", decode: FactDecode.FreedPages);
    public static readonly SqliteFactKind VacuumInto = new("vacuum-into", maintain: true, sql: "VACUUM INTO $destination;", decode: FactDecode.None);
    public static readonly SqliteFactKind Checkpoint = new("checkpoint", maintain: true, sql: "", decode: FactDecode.Native);
    public static readonly SqliteFactKind Backup = new("backup", maintain: true, sql: "", decode: FactDecode.Native);
    public static readonly SqliteFactKind Snapshot = new("snapshot", maintain: true, sql: "", decode: FactDecode.Native);

    public bool Maintain { get; }
    public string Sql { get; }
    public FactDecode Decode { get; }
}

[SmartEnum]
public sealed partial class FactDecode {
    public static readonly FactDecode None = new();        // no rows; before/after stay None
    public static readonly FactDecode Scalar = new();      // one scalar into After
    public static readonly FactDecode Rows = new();        // result rows joined into After, Count = row count
    public static readonly FactDecode FreedPages = new();  // freelist delta into Count
    public static readonly FactDecode Native = new();      // raw out-params own Count/Bytes; ExecuteReader is bypassed
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
- Entry: `public static IO<(Seq<SqliteFact> Facts, Fin<Unit> Admission)> Probe(SqliteConnection connection, ClockPolicy clocks)` — `Fin<Unit>` aborts the open ceremony when an expected engine capability is absent.
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
                : Fin.Fail<Unit>(Error.New($"<missing-engine-capability:{string.Join(",", missing)}>")));
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
- Cases: `quick-check | integrity-check | foreign-key-check | optimize | incremental-vacuum | vacuum-into | checkpoint | backup | snapshot` — the `Maintain`-flagged `SqliteFactKind` rows; `data-version` and `image` are the non-maintenance probe/serialize rows on the same stream.
- Entry: `public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, SqliteMaintenancePolicy policy, Option<string> destination = default)` — one polymorphic verb entry dispatching on `verb.Decode`; the destination arity serves vacuum-into.
- Auto: `optimize` fires at close and the `Checkpoint` `TRUNCATE` form fires at drain through the store lifecycle's band-300 registration; `incremental_vacuum($pages)` arms when `DataVersion`/freelist probes show `freelist_count` exceeds `FreelistThresholdPages` and pages exactly `FreelistThresholdPages` so a sweep never holds the write lock unbounded; recurring cadence rides the persistence-maintenance `ScheduleEntry` row and executes only under the maintenance lease; the integrity ladder runs `quick_check` at open, full `integrity_check($limit)` capped at `IntegrityMaxErrors` on the `Repair` path, and `foreign_key_check` after migrations (FK violations never surface from an integrity check); `PRAGMA optimize` runs its analysis at close so the planner reads fresh `sqlite_stat` rows.
- Receipt: rows-decoded verbs carry the engine's result rows in the `After` slot and the row count in `Count`; `incremental_vacuum` carries the freed-page delta in `Count`; the native-decoded `Checkpoint` carries the `sqlite3_wal_checkpoint_v2` log-frame and checkpointed-frame counts in `Count`/`Bytes`; paged `Backup` carries remaining pages in `Bytes` and total pages in `Count`, retries a `BUSY`/`LOCKED` step up to `BackupBusyRetries` times, and admits the copy only after a `quick_check` on the copy connection.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new maintenance verb is one `Maintain`-flagged `SqliteFactKind` row carrying its `Sql`/`Decode` columns; a new engine function, aggregate (`CreateAggregate` inside the same `Bind` delegate), or collation is one `FunctionRegistration` row; zero new surface.
- Boundary: `Maintain`, `Register`, `Backup`, `Image`, `DataVersion`, and `WithSnapshot` are this fence's boundary capsules over ADO and raw-interop ceremony; the verbs surface upward as the `StoreOp.Maintain` arm set, never as a service class. The `$destination` path binds a typed `SqliteType.Text` `SqliteParameter` so the vacuum-into bind never widens through `AddWithValue` inference, while the `$pages`/`$limit` page counts substitute as invariant-culture integer literals into the PRAGMA text because a PRAGMA argument is not a bindable parameter in SQLite — the page-count source is the policy row, never raw runtime input. Blob payloads stream through the constructed `SqliteBlob` write stream over a `zeroblob(N)` row preallocation (fixed-size once written) and the `GetStream` read path, so whole-payload `byte[]` materialization is the deleted pattern. The cross-process change probe is `PRAGMA data_version` — it moves only when another connection commits, so an unchanged register proves cache validity without touching tables and a notification bus or table-poll loop is the rejected form. The in-memory whole-schema image rides `raw.sqlite3_serialize(connection.Handle, "main", out size, 0)` out to a `ReadOnlyMemory<byte>` and `raw.sqlite3_deserialize(connection.Handle, "main", ptr, size, size, raw.SQLITE_DESERIALIZE_FREEONCLOSE | raw.SQLITE_DESERIALIZE_RESIZEABLE)` back in, the `sqlite-memory` profile's restore/handoff path distinct from the file-backed content-chunk frame — the serialized pointer is marshalled once and `SQLITE_DESERIALIZE_FREEONCLOSE` transfers ownership to the engine. Paged backup steps the raw backup session over `Handle`: the loop continues while `sqlite3_backup_step` returns `raw.SQLITE_OK`, retries the same step up to `BackupBusyRetries` times on `raw.SQLITE_BUSY`/`raw.SQLITE_LOCKED`, terminates on `raw.SQLITE_DONE`, and surfaces any other code as a typed fault — the `Thread.Sleep` quantum in the step loop is the raw-interop seam exemption, not a domain delay — the provider's whole-file `BackupDatabase` is subsumed because it restarts under other-connection writes, while the paced session yields bounded latency, progress facts, and a copy-side `quick_check` admission (the step succeeding is never the proof). The `Checkpoint` verb rides `raw.sqlite3_wal_checkpoint_v2(connection.Handle, "main", mode, out logFrames, out checkpointed)` so the fact carries typed frame counts and a `BUSY` return receipts a retry, with the checkpoint-mode constants `SQLITE_CHECKPOINT_PASSIVE` (0), `SQLITE_CHECKPOINT_FULL` (1), `SQLITE_CHECKPOINT_RESTART` (2), `SQLITE_CHECKPOINT_TRUNCATE` (3) — a pinned WAL read window and a TRUNCATE are adversaries, so a refused checkpoint is a receipted retry, never a silent rewind. The snapshot bracket pins a consistent multi-transaction read view under WAL, attempts one `sqlite3_snapshot_recover` pass when the initial pin is refused, rejects a pin older than the optional `floor` through `sqlite3_snapshot_cmp` so a monotonic reader never regresses across successive brackets, and frees only a held snapshot handle. `uuid7` registration is the sqlite leg of the identity policy, `xxh128` is the non-cryptographic identity scalar (`isDeterministic: true` admits it into expression indexes and generated columns), and `instant_iso` collates persisted ExtendedIso text chronologically by parsed comparison.

```csharp signature
public sealed record SqliteMaintenancePolicy(long FreelistThresholdPages, int BackupStepPages, int IntegrityMaxErrors, int BackupBusyRetries) {
    public static readonly SqliteMaintenancePolicy Default = new(FreelistThresholdPages: 1000, BackupStepPages: 1024, IntegrityMaxErrors: 100, BackupBusyRetries: 8);
}

public sealed record FunctionRegistration(string Name, Action<SqliteConnection> Bind) {
    public static readonly Seq<FunctionRegistration> Rows = Seq(
        new FunctionRegistration("uuid7", static connection => connection.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("D"), isDeterministic: false)),
        new FunctionRegistration("xxh128", static connection => connection.CreateFunction("xxh128", static (byte[] payload) => XxHash128.Hash(payload), isDeterministic: true)),
        new FunctionRegistration("instant_iso", static connection => connection.CreateCollation("instant_iso", static (left, right) =>
            InstantPattern.ExtendedIso.Parse(left).Value.CompareTo(InstantPattern.ExtendedIso.Parse(right).Value))));
}

public static class SqliteMaintenance {
    public static IO<Seq<SqliteFact>> Register(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => FunctionRegistration.Rows.Map(row => {
            var mark = clocks.Mark();
            row.Bind(connection);
            return new SqliteFact(SqliteFactKind.Registration, row.Name, None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        }));

    public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, SqliteMaintenancePolicy policy, Option<string> destination = default) =>
        verb.Decode.Equals(FactDecode.Native)
            ? verb.Equals(SqliteFactKind.Checkpoint)
                ? Checkpoint(connection, clocks, raw.SQLITE_CHECKPOINT_TRUNCATE)
                : IO.fail<SqliteFact>(Error.New($"<native-verb-needs-dedicated-entry:{verb.Key}>"))
            : IO.lift(() => {
                var mark = clocks.Mark();
                // BOUNDARY ADAPTER — ADO statement seam; the maintenance verb is a provider-forced command.
                using var command = connection.CreateCommand();
                command.CommandText = verb.Sql
                    .Replace("$pages", policy.FreelistThresholdPages.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
                    .Replace("$limit", policy.IntegrityMaxErrors.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                destination.Iter(path => command.Parameters.Add(new SqliteParameter { ParameterName = "$destination", SqliteType = SqliteType.Text, Value = path }));
                using var reader = command.ExecuteReader();
                var rows = Seq<string>();
                while (reader.Read())
                    rows = rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue)));
                return verb.Decode.Equals(FactDecode.FreedPages)
                    ? new SqliteFact(verb, verb.Key, None, None, Count: FreelistDelta(connection), Bytes: 0, clocks.Elapsed(mark), clocks.Now)
                    : new SqliteFact(verb, verb.Key, None, Optional(string.Join(";", rows)).Filter(static text => text.Length > 0), Count: rows.Count, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
            });

    public static IO<SqliteFact> Checkpoint(SqliteConnection connection, ClockPolicy clocks, int mode) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var status = raw.sqlite3_wal_checkpoint_v2(connection.Handle, "main", mode, out var logFrames, out var checkpointed);
            return (Status: status, Log: logFrames, Done: checkpointed, Mark: mark);
        }).Bind(step =>
            // A BUSY checkpoint is a pinned-reader refusal: fail the IO so the retry schedule re-fires; OK is the receipted result.
            step.Status == raw.SQLITE_BUSY
                ? IO.fail<SqliteFact>(Error.New(raw.SQLITE_BUSY, $"<checkpoint-busy:{step.Done}:{step.Log}>"))
                : IO.pure(new SqliteFact(SqliteFactKind.Checkpoint, "main",
                    Before: Some(raw.sqlite3_errstr(step.Status).utf8_to_string()), After: Some("checkpointed"),
                    Count: step.Done, Bytes: step.Log, clocks.Elapsed(step.Mark), clocks.Now)))
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

    public static IO<SqliteFact> Backup(SqliteConnection source, SqliteConnection destination, ClockPolicy clocks, SqliteMaintenancePolicy policy) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var session = raw.sqlite3_backup_init(destination.Handle, "main", source.Handle, "main");
            var remaining = 0;
            try {
                // BOUNDARY ADAPTER — raw paged backup seam; OK continues, DONE completes, BUSY/LOCKED retries the step.
                var step = StepUntilDone(session, policy);
                remaining = raw.sqlite3_backup_remaining(session);
                return step.IsSucc
                    ? new SqliteFact(SqliteFactKind.Backup, "main", None, VerifyCopy(destination), Count: raw.sqlite3_backup_pagecount(session), Bytes: remaining, clocks.Elapsed(mark), clocks.Now)
                    : new SqliteFact(SqliteFactKind.Backup, "main", Some(step.Error.Message), None, Count: -1, Bytes: remaining, clocks.Elapsed(mark), clocks.Now);
            } finally {
                raw.sqlite3_backup_finish(session);
            }
        });

    public static IO<(SqliteFact Fact, Option<ReadOnlyMemory<byte>> Image)> Image(SqliteConnection connection, ClockPolicy clocks, Option<ReadOnlyMemory<byte>> inbound = default) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            return inbound is { IsSome: true, Case: ReadOnlyMemory<byte> bytes }
                ? (Deserialize(connection, bytes, clocks, mark), Option<ReadOnlyMemory<byte>>.None)
                : Serialize(connection, clocks, mark);
        });

    static (SqliteFact Fact, Option<ReadOnlyMemory<byte>> Image) Serialize(SqliteConnection connection, ClockPolicy clocks, long mark) {
        var pointer = raw.sqlite3_serialize(connection.Handle, "main", out var size, 0);
        var image = new byte[size];
        Marshal.Copy(pointer, image, 0, (int)size);
        return (new SqliteFact(SqliteFactKind.Image, "serialize", None, None, Count: 0, Bytes: size, clocks.Elapsed(mark), clocks.Now), Some<ReadOnlyMemory<byte>>(image));
    }

    static SqliteFact Deserialize(SqliteConnection connection, ReadOnlyMemory<byte> bytes, ClockPolicy clocks, long mark) {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes.ToArray(), 0, pointer, bytes.Length);
        var status = raw.sqlite3_deserialize(connection.Handle, "main", pointer, bytes.Length, bytes.Length,
            raw.SQLITE_DESERIALIZE_FREEONCLOSE | raw.SQLITE_DESERIALIZE_RESIZEABLE);
        return new SqliteFact(SqliteFactKind.Image, "deserialize", Some(raw.sqlite3_errstr(status).utf8_to_string()), None, Count: 0, Bytes: bytes.Length, clocks.Elapsed(mark), clocks.Now);
    }

    public static IO<Fin<T>> WithSnapshot<T>(SqliteConnection connection, string schema, Func<SqliteConnection, Fin<T>> read, Option<sqlite3_snapshot> floor = default) =>
        IO.lift(() => {
            var status = raw.sqlite3_snapshot_get(connection.Handle, schema, out var snapshot);
            if (status != raw.SQLITE_OK && raw.sqlite3_snapshot_recover(connection.Handle, schema) == raw.SQLITE_OK)
                status = raw.sqlite3_snapshot_get(connection.Handle, schema, out snapshot);
            try {
                return status != raw.SQLITE_OK
                    ? Fin.Fail<T>(Error.New($"<snapshot-unavailable:{schema}:{raw.sqlite3_errstr(status).utf8_to_string()}>"))
                    : floor is { IsSome: true, Case: sqlite3_snapshot since } && raw.sqlite3_snapshot_cmp(snapshot, since) < 0
                        ? Fin.Fail<T>(Error.New($"<snapshot-stale:{schema}>"))
                        : raw.sqlite3_snapshot_open(connection.Handle, schema, snapshot) == raw.SQLITE_OK
                            ? read(connection)
                            : Fin.Fail<T>(Error.New($"<snapshot-open-refused:{schema}>"));
            } finally {
                if (snapshot is { } held)
                    raw.sqlite3_snapshot_free(held);
            }
        });

    public static Stream BlobStream(SqliteConnection connection, string table, string column, long rowid, bool readOnly = false) =>
        new SqliteBlob(connection, table, column, rowid, readOnly);

    public static Stream BlobStream(SqliteDataReader reader, int ordinal) =>
        reader.GetStream(ordinal);

    static long FreelistDelta(SqliteConnection connection) {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA freelist_count;";
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    static Option<string> VerifyCopy(SqliteConnection destination) {
        using var command = destination.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";
        var verdict = command.ExecuteScalar()?.ToString() ?? "";
        return verdict == "ok" ? Some("complete") : None;
    }

    static Fin<Unit> StepUntilDone(sqlite3_backup session, SqliteMaintenancePolicy policy) {
        var attempt = 0;
        for (var code = raw.sqlite3_backup_step(session, policy.BackupStepPages); code != raw.SQLITE_DONE; code = raw.sqlite3_backup_step(session, policy.BackupStepPages)) {
            if (code == raw.SQLITE_OK) { attempt = 0; continue; }
            if ((code == raw.SQLITE_BUSY || code == raw.SQLITE_LOCKED) && attempt++ < policy.BackupBusyRetries) { Thread.Sleep(20); continue; }
            return Fin.Fail<Unit>(Error.New(code, $"<backup-step:{raw.sqlite3_errstr(code).utf8_to_string()}>"));
        }
        return Fin.Succ(unit);
    }

    static readonly Schedule _default_checkpoint_retry = Schedule.recurs(8) | Schedule.exponential(TimeSpan.FromMilliseconds(20)) | Schedule.maxDelay(TimeSpan.FromMilliseconds(250));
}
```

## [05]-[EXTENSION_GATES]

- Owner: `ExtensionGate`
- Cases: `vec0` gated | `sqlean-regexp`, `sqlean-crypto`, `sqlean-fuzzy`, `sqlean-uuid`, `sqlean-stats`, `sqlean-text` deferred | `sqlcipher` gated (the keying ceremony and the `EncryptionGate` owner relocated to `Store/encryption#SQLITE_KEYING`, bound to a KMS-unwrapped DEK).
- Entry: `public static IO<SqliteFact> Load(SqliteConnection connection, ClockPolicy clocks, ExtensionGate gate)` — arms the db_config route and loads through the OS loader.
- Receipt: one extension-load fact per opened gate (`Slot` = gate name, `After` = entrypoint); the encryption ceremony receipts `cipher_version` on the same stream at its owning page.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new loadable concern is one `ExtensionGate` row naming artifact route and fallback lane; a new per-connection hardening posture is one `DbConfig` row; future cr-sqlite admission lands as one gate row plus merge-law rows on the sync owner, never a transport case; zero new surface.
- Boundary: `EnableExtensions` arms only the db_config form, so the SQL-level `load_extension()` function stays off; `DbConfig.Apply` folds per-connection config flags through `raw.sqlite3_db_config` so defensive-mode and double-quoted-string-literal rejection are connection policy values rather than connection-string knobs, applied once per physical open before any user statement (`DQS=0` makes a double-quoted string a prepare-time syntax error — identifiers quote with `"`, strings with `'`). The OS loader resolves artifacts directly — absolute path or loader-path variable, with the NuGet RID convention copying the `osx-arm64` payload to output and every off-platform RID asset dropped under the single-RID target so the `e_sqlite3` bundle and the loadable-extension payloads ship as the `osx-arm64` dylib set only; the vector gate never deletes the brute-force fallback case; `jsonb_*` blob functions are an in-process raw-SQL fast path that never crosses a seam while the EF mapping stays TEXT json; the encryption key handle, the SQLCipher keying surface (`PRAGMA key`, `cipher_migrate`, `cipher_version`, `rekey` over the `SQLite3Provider_sqlcipher` provider), and the KMS-unwrapped DEK binding are owned at `Store/encryption#SQLITE_KEYING` where the promoted `EncryptionGate` binds the ceremony to a KMS-unwrapped DEK, so the `sqlcipher` gate row here is `ExtensionGateState.Gated` (no longer Research) and this page carries only the loadable-extension and db-config posture, never the keying record or a `Password` connection-string row (the admitted `e_sqlite3` bundle has no cipher and fails at open); `sqlean-math` stays rejected (the compile flag owns it) and `Microsoft.SemanticKernel.Connectors.SqliteVec` stays rejected as a thin loader dragging a foreign graph.

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
        new ExtensionGate("sqlean-crypto", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("xxh128 function row for non-cryptographic identity")),
        new ExtensionGate("sqlean-fuzzy", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("pg_trgm and fuzzystrmatch on the server lanes")),
        new ExtensionGate("sqlean-uuid", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("uuid7 function row")),
        new ExtensionGate("sqlean-stats", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, Some("analytical-lane percentiles")),
        new ExtensionGate("sqlean-text", None, "sqlean osx-arm64 loadable vendored as build content", ExtensionGateState.Deferred, None),
        new ExtensionGate("sqlcipher", None, "SQLite3Provider_sqlcipher provider with an externally supplied native library; the batteries bundle route is deprecated and the two routes never mix; keying ceremony owned at Store/encryption#SQLITE_KEYING", ExtensionGateState.Gated, Some("plaintext default")));
}

public sealed record DbConfig(int Op, int Value) {
    public static readonly Seq<DbConfig> Hardened = Seq(
        new DbConfig(raw.SQLITE_DBCONFIG_DEFENSIVE, 1),
        new DbConfig(raw.SQLITE_DBCONFIG_DQS_DDL, 0),
        new DbConfig(raw.SQLITE_DBCONFIG_DQS_DML, 0),
        new DbConfig(raw.SQLITE_DBCONFIG_ENABLE_TRIGGER, 1),
        new DbConfig(raw.SQLITE_DBCONFIG_ENABLE_VIEW, 1),
        new DbConfig(raw.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, 1));

    public static IO<Unit> Apply(SqliteConnection connection, Seq<DbConfig> rows) =>
        IO.lift(() => rows.Iter(row => raw.sqlite3_db_config(connection.Handle, row.Op, row.Value, out _)));
}

public static class ExtensionOps {
    public static IO<SqliteFact> Load(SqliteConnection connection, ClockPolicy clocks, ExtensionGate gate) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            connection.EnableExtensions(true);
            connection.LoadExtension(gate.Name, gate.EntryPoint is { IsSome: true, Case: string entry } ? entry : null);
            return new SqliteFact(SqliteFactKind.ExtensionLoad, gate.Name, None, gate.EntryPoint, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        });
}
```

## [06]-[RESEARCH]

- [EXTENSION_LOADING]: `vec0` live load with the `vec_version()` fact and the package-payload versus vendored-tarball sourcing decision; hardened-runtime `dlopen` acceptance of unsigned extension dylibs inside the signed Rhino host process; the `SQLite3Provider_sqlcipher` provider route with an externally supplied native library on `osx-arm64` and its crypto-backend notice set (the keying ceremony itself is owned and researched at `Store/encryption#SQLITE_KEYING`).
- [DB_CONFIG_OPS]: the live ordering of the `DbConfig.Hardened` set against pooled physical opens — whether defensive mode and the double-quoted-string-literal DDL/DML rejection must precede the migration ladder or apply after schema creation, and the interaction of `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` enablement with the db_config extension-arming path on the same connection.
- [MEMORY_IMAGE]: the `sqlite3_serialize`/`sqlite3_deserialize` round-trip for the `sqlite-memory` profile's restore and cross-process handoff — whether the `SQLITE_DESERIALIZE_FREEONCLOSE | SQLITE_DESERIALIZE_RESIZEABLE` flag pair transfers the marshalled image to engine ownership cleanly and the deserialized in-memory store survives a subsequent `PRAGMA optimize`, proven against `e_sqlite3` 3.50.4 before the image fence pins.
