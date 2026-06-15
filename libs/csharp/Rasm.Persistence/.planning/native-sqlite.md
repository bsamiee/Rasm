# [PERSISTENCE_NATIVE_SQLITE]

The embedded engine floor of Rasm.Persistence: four policy tables and one receipted verb family over Microsoft.Data.Sqlite on the SQLitePCLRaw e_sqlite3 bundle, with zero wrapper types between policy and provider. The page owns the frozen PRAGMA axis, the verified compile-flag surface with its admission gate, the maintenance verbs that surface upward as the StoreOp.Maintain arm set, the function and collation registration rows, blob streaming, and the loadable-extension and encryption gates. AppHost vocabulary arrives settled: every fact stamps from `ClockPolicy`, cadence rides `ScheduleEntry` rows under the maintenance lease, and `DataClassification` gates the encryption row.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                              |
| :-----: | --------------- | ------------------------------------------------------------------- |
|   [1]   | PRAGMA_TABLE    | Frozen engine policy rows; before/after facts; one resolve override |
|   [2]   | COMPILE_SURFACE | Verified compile-flag admission; probe receipt; absent flags routed |
|   [3]   | MAINTENANCE_OPS | Receipted verbs, function rows, blob streams, snapshot bracket      |
|   [4]   | EXTENSION_GATES | Loadable-extension law; vector, sqlean, and encryption gates        |

## [2]-[PRAGMA_TABLE]

- Owner: `SqlitePragma` `[SmartEnum<string>]` frozen policy table; `SqliteFact` with `SqliteFactKind` is the page-wide fact stream every cluster emits onto.
- Cases: journal_mode | synchronous | foreign_keys | busy_timeout | cache_size | mmap_size | temp_store | wal_autocheckpoint | journal_size_limit | auto_vacuum
- Entry: `public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, FrozenDictionary<SqlitePragma, string> resolved)` — `IO<T>` carries the connection effect; one before/after fact per row.
- Receipt: pragma facts (Slot = pragma key, Before/After captured) fill the open receipt's pragma slots and materialize at the receipt-sink edge through the query rail's interceptor spine.
- Packages: Microsoft.Data.Sqlite, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new engine knob is one `SqlitePragma` row; per-profile variance is one override entry through `Resolve`; zero new surface.
- Boundary: inline PRAGMA text at call sites is the deleted pattern — the store open ceremony consumes `Resolve` output as its PRAGMA ladder, and connection interceptors re-apply non-persistent rows on non-pooled opens; `JournalMode` and `AutoVacuum` persist inside the database file, so their facts prove idempotent re-application rather than mutation; `BusyTimeout` is the busy-retry half of the cross-process WAL law and `WalAutocheckpoint` bounds WAL growth between drain checkpoints; a `SqliteException` carrying an extended result code routes by code rather than a flat fault — CORRUPT and NOTADB drive the lifecycle Repair restore path while BUSY rides busy-retry — once the `raw.sqlite3_extended_result_codes` enablement and the extended-code constant surface resolve on the `[RESULT_CODE_ROUTING]` gate; `ApplyRow` is the per-row provider seam and stays private to the fence.

```csharp signature
public sealed class SqliteKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class SqliteFactKind {
    public static readonly SqliteFactKind Pragma = new("pragma", maintain: false);
    public static readonly SqliteFactKind CompileOption = new("compile-option", maintain: false);
    public static readonly SqliteFactKind FunctionList = new("function-list", maintain: false);
    public static readonly SqliteFactKind Registration = new("registration", maintain: false);
    public static readonly SqliteFactKind ExtensionLoad = new("extension-load", maintain: false);
    public static readonly SqliteFactKind QuickCheck = new("quick-check", maintain: true);
    public static readonly SqliteFactKind IntegrityCheck = new("integrity-check", maintain: true);
    public static readonly SqliteFactKind ForeignKeyCheck = new("foreign-key-check", maintain: true);
    public static readonly SqliteFactKind Optimize = new("optimize", maintain: true);
    public static readonly SqliteFactKind IncrementalVacuum = new("incremental-vacuum", maintain: true);
    public static readonly SqliteFactKind VacuumInto = new("vacuum-into", maintain: true);
    public static readonly SqliteFactKind Checkpoint = new("checkpoint", maintain: true);
    public static readonly SqliteFactKind Backup = new("backup", maintain: true);
    public static readonly SqliteFactKind Snapshot = new("snapshot", maintain: true);
    public bool Maintain { get; }
}

public readonly record struct SqliteFact(SqliteFactKind Kind, string Slot, Option<string> Before, Option<string> After, long Count, long Bytes, Duration Elapsed, Instant At);

[SmartEnum<string>]
[KeyMemberEqualityComparer<SqliteKeyPolicy, string>]
[KeyMemberComparer<SqliteKeyPolicy, string>]
public sealed partial class SqlitePragma {
    public static readonly SqlitePragma JournalMode = new("journal_mode", value: "WAL", persistent: true);
    public static readonly SqlitePragma Synchronous = new("synchronous", value: "NORMAL", persistent: false);
    public static readonly SqlitePragma ForeignKeys = new("foreign_keys", value: "ON", persistent: false);
    public static readonly SqlitePragma BusyTimeout = new("busy_timeout", value: "5000", persistent: false);
    public static readonly SqlitePragma CacheSize = new("cache_size", value: "-20000", persistent: false);
    public static readonly SqlitePragma MmapSize = new("mmap_size", value: "268435456", persistent: false);
    public static readonly SqlitePragma TempStore = new("temp_store", value: "MEMORY", persistent: false);
    public static readonly SqlitePragma WalAutocheckpoint = new("wal_autocheckpoint", value: "1000", persistent: false);
    public static readonly SqlitePragma JournalSizeLimit = new("journal_size_limit", value: "67108864", persistent: false);
    public static readonly SqlitePragma AutoVacuum = new("auto_vacuum", value: "INCREMENTAL", persistent: true);

    public string Value { get; }
    public bool Persistent { get; }
}

public static class PragmaOps {
    public static FrozenDictionary<SqlitePragma, string> Resolve(HashMap<SqlitePragma, string> overrides) =>
        SqlitePragma.Items.ToFrozenDictionary(static row => row, row => overrides.Find(row).IfNone(row.Value));

    public static IO<Seq<SqliteFact>> Apply(SqliteConnection connection, ClockPolicy clocks, FrozenDictionary<SqlitePragma, string> resolved) =>
        IO.lift(() => SqlitePragma.Items.ToSeq().Map(row => ApplyRow(connection, clocks, row, resolved[row])).Strict());

    private static SqliteFact ApplyRow(SqliteConnection connection, ClockPolicy clocks, SqlitePragma row, string value) {
        var mark = clocks.Mark();
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA {row.Key};";
        var before = Optional(command.ExecuteScalar()).Map(static found => found.ToString() ?? "");
        command.CommandText = $"PRAGMA {row.Key} = {value}; PRAGMA {row.Key};";
        var after = Optional(command.ExecuteScalar()).Map(static found => found.ToString() ?? "");
        return new SqliteFact(SqliteFactKind.Pragma, row.Key, before, after, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
    }
}
```

## [3]-[COMPILE_SURFACE]

- Owner: `SqliteCompileSurface`
- Entry: `public static IO<(Seq<SqliteFact> Facts, Fin<Unit> Admission)> Probe(SqliteConnection connection, ClockPolicy clocks)` — `Fin<Unit>` aborts the open ceremony when an expected engine capability is absent.
- Receipt: one compile-option fact per probed row plus one expected-function fact each; the open receipt's compile-options slot consumes the stream.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, LanguageExt.Core, BCL inbox
- Growth: a newly required engine capability is one expected-capability row on `ExpectedOptions` or `ExpectedFunctions`; zero new surface.
- Boundary: `Probe` and its `Read` kernel are this fence's boundary capsule over ADO reader ceremony; JSON support is detected through pragma_function_list, never compile_options; the central SQLitePCLRaw bundle pin overrides the provider graph Microsoft.Data.Sqlite is tested against, so the pin settles only on this receipted probe plus the Batteries_V2 round-trip research gate; a custom native SQLite toolchain admission is the rejected escalation for absent flags — every absent concern routes to the named owner below.

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
        "DEFAULT_FOREIGN_KEYS=1",
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

    private static Seq<string> Read(SqliteConnection connection, string sql) {
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
|   [1]   | SESSION + PREUPDATE_HOOK (changesets) | the op-log HLC changefeed owns diffing                         |
|   [2]   | NORMALIZE (normalized SQL text)       | command-interceptor slow-query receipts own statement evidence |
|   [3]   | DBSTAT virtual table                  | maintenance facts own page-level diagnostics                   |
|   [4]   | SOUNDEX                               | the sqlean-fuzzy deferred gate owns phonetics                  |
|   [5]   | GEOPOLY                               | PostGIS geometry lanes own geodesy                             |

## [4]-[MAINTENANCE_OPS]

- Owner: `SqliteMaintenance`
- Cases: quick-check | integrity-check | foreign-key-check | optimize | incremental-vacuum | vacuum-into | checkpoint | backup | snapshot — the Maintain-flagged `SqliteFactKind` rows.
- Entry: `public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, Option<string> destination = default)` — one polymorphic verb entry; the destination arity serves vacuum-into.
- Auto: optimize fires at close and checkpoint TRUNCATE fires at drain through the store lifecycle's band-300 registration; incremental-vacuum arms when freelist_count exceeds `FreelistThresholdPages`; recurring cadence rides the persistence-maintenance `ScheduleEntry` row and executes only under the maintenance lease; the integrity ladder runs quick-check at open, full integrity-check on the Repair path, and foreign-key-check after migrations.
- Receipt: maintenance facts carry the engine's result rows in the After slot and result-row counts in Count; paged backup emits one progress fact per `BackupStepPages` step with remaining pages in Count and a terminal fact carrying total pages.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new maintenance verb is one Maintain-flagged `SqliteFactKind` row plus one `Sql` entry; a new engine function, aggregate (`CreateAggregate` inside the same `Bind` delegate), or collation is one `FunctionRegistration` row; zero new surface.
- Boundary: `Maintain`, `Register`, `Backup`, and `WithSnapshot` are this fence's boundary capsules over ADO and raw-interop ceremony; the verbs surface upward as the StoreOp.Maintain arm set, never as a service class; the `Maintain` `$destination` parameter pins `SqliteType.Text` on a typed `SqliteParameter` so the path binds without provider type inference and a blob- or integer-keyed maintenance bind never widens through `AddWithValue` guesswork; blob payloads stream through the constructed `SqliteBlob` write stream and the `GetStream` read path, so whole-payload byte[] materialization is the deleted pattern; paged backup steps the raw backup session over `Handle` — the provider's whole-file `BackupDatabase` copy is subsumed by the paged session, which adds progress facts; the `Checkpoint` verb rides the `wal_checkpoint(TRUNCATE)` pragma today and upgrades to the `raw.sqlite3_wal_checkpoint_v2` out-param form so the fact carries typed log-frame and checkpointed-frame counts and a BUSY checkpoint receipts a retry once the managed `_v2` signature resolves on the `[CHECKPOINT_RECEIPT]` gate; the snapshot bracket pins a consistent multi-transaction read view under WAL, attempts one `sqlite3_snapshot_recover` pass when the initial pin is refused, rejects a pin older than the optional `floor` through `sqlite3_snapshot_cmp` so a monotonic reader never regresses across successive brackets, and frees only a held snapshot handle; `uuid7` registration is the sqlite leg of the identity policy, `xxh128` is the non-cryptographic identity scalar, and `instant_iso` collates persisted ExtendedIso text chronologically by parsed comparison.

```csharp signature
public sealed record SqliteMaintenancePolicy(long FreelistThresholdPages, int BackupStepPages) {
    public static readonly SqliteMaintenancePolicy Default = new(FreelistThresholdPages: 1000, BackupStepPages: 1024);
}

public sealed record FunctionRegistration(string Name, Action<SqliteConnection> Bind) {
    public static readonly Seq<FunctionRegistration> Rows = Seq(
        new FunctionRegistration("uuid7", static connection => connection.CreateFunction("uuid7", static () => Guid.CreateVersion7().ToString("D"), isDeterministic: false)),
        new FunctionRegistration("xxh128", static connection => connection.CreateFunction("xxh128", static (byte[] payload) => XxHash128.Hash(payload), isDeterministic: true)),
        new FunctionRegistration("instant_iso", static connection => connection.CreateCollation("instant_iso", static (left, right) =>
            InstantPattern.ExtendedIso.Parse(left).Value.CompareTo(InstantPattern.ExtendedIso.Parse(right).Value))));
}

public static class SqliteMaintenance {
    public static readonly FrozenDictionary<SqliteFactKind, string> Sql = new Dictionary<SqliteFactKind, string> {
        [SqliteFactKind.QuickCheck] = "PRAGMA quick_check;",
        [SqliteFactKind.IntegrityCheck] = "PRAGMA integrity_check;",
        [SqliteFactKind.ForeignKeyCheck] = "PRAGMA foreign_key_check;",
        [SqliteFactKind.Optimize] = "PRAGMA optimize;",
        [SqliteFactKind.IncrementalVacuum] = "PRAGMA incremental_vacuum;",
        [SqliteFactKind.Checkpoint] = "PRAGMA wal_checkpoint(TRUNCATE);",
        [SqliteFactKind.VacuumInto] = "VACUUM INTO $destination;",
    }.ToFrozenDictionary();

    public static IO<Seq<SqliteFact>> Register(SqliteConnection connection, ClockPolicy clocks) =>
        IO.lift(() => FunctionRegistration.Rows.Map(row => {
            var mark = clocks.Mark();
            row.Bind(connection);
            return new SqliteFact(SqliteFactKind.Registration, row.Name, None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        }));

    public static IO<SqliteFact> Maintain(SqliteConnection connection, ClockPolicy clocks, SqliteFactKind verb, Option<string> destination = default) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            using var command = connection.CreateCommand();
            command.CommandText = Sql[verb];
            destination.Iter(path => command.Parameters.Add(new SqliteParameter { ParameterName = "$destination", SqliteType = SqliteType.Text, Value = path }));
            using var reader = command.ExecuteReader();
            var slots = Seq<string>();
            while (reader.Read())
                slots = slots.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue)));
            return new SqliteFact(verb, verb.Key, None, Optional(string.Join(";", slots)).Filter(static text => text.Length > 0), Count: slots.Count, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        });

    public static IO<Seq<SqliteFact>> Backup(SqliteConnection source, SqliteConnection destination, ClockPolicy clocks, SqliteMaintenancePolicy policy) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            var session = raw.sqlite3_backup_init(destination.Handle, "main", source.Handle, "main");
            var facts = Seq<SqliteFact>();
            var total = 0;
            try {
                while (raw.sqlite3_backup_step(session, policy.BackupStepPages) == raw.SQLITE_OK)
                    facts = facts.Add(new SqliteFact(SqliteFactKind.Backup, "main", None, None, Count: raw.sqlite3_backup_remaining(session), Bytes: 0, clocks.Elapsed(mark), clocks.Now));
                total = raw.sqlite3_backup_pagecount(session);
            } finally {
                raw.sqlite3_backup_finish(session);
            }
            return facts.Add(new SqliteFact(SqliteFactKind.Backup, "main", None, Some("complete"), Count: total, Bytes: 0, clocks.Elapsed(mark), clocks.Now));
        });

    public static IO<Fin<T>> WithSnapshot<T>(SqliteConnection connection, string schema, Func<SqliteConnection, Fin<T>> read, Option<sqlite3_snapshot> floor = default) =>
        IO.lift(() => {
            var status = raw.sqlite3_snapshot_get(connection.Handle, schema, out var snapshot);
            if (status != raw.SQLITE_OK && raw.sqlite3_snapshot_recover(connection.Handle, schema) == raw.SQLITE_OK)
                status = raw.sqlite3_snapshot_get(connection.Handle, schema, out snapshot);
            try {
                return status != raw.SQLITE_OK
                    ? Fin.Fail<T>(Error.New($"<snapshot-unavailable:{schema}>"))
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
}
```

## [5]-[EXTENSION_GATES]

- Owner: `ExtensionGate`
- Cases: vec0 gated | sqlean-regexp, sqlean-crypto, sqlean-fuzzy, sqlean-uuid, sqlean-stats, sqlean-text deferred | sqlcipher research
- Entry: `public static IO<SqliteFact> Load(SqliteConnection connection, ClockPolicy clocks, ExtensionGate gate)` — arms the db_config route and loads through the OS loader.
- Receipt: one extension-load fact per opened gate (Slot = gate name, After = entrypoint); the encryption ceremony receipts cipher_version on the same stream when its research gate opens.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new loadable concern is one `ExtensionGate` row naming artifact route and fallback lane; a new per-connection hardening posture is one `DbConfig` row; future cr-sqlite admission lands as one gate row plus merge-law rows on the sync owner, never a transport case; zero new surface.
- Boundary: `EnableExtensions` arms only the db_config form, so the SQL-level load_extension() function stays off; `DbConfig.Apply` folds per-connection config flags through `raw.sqlite3_db_config` so defensive-mode and double-quoted-string-literal rejection are connection policy values rather than connection-string knobs, applied once per physical open before any user statement; the OS loader resolves artifacts directly — absolute path or loader-path variable, with the NuGet RID convention copying payloads to output; the vector gate never deletes the brute-force fallback case; jsonb_* blob functions are an in-process raw-SQL fast path that never crosses a seam while the EF mapping stays TEXT json; the encryption key handle arrives from the app root per open and is never persisted, and the `EncryptionGate.Sqlcipher` ceremony rows declare the keying surface — `PRAGMA key`, `cipher_migrate`, `cipher_version`, and `rekey` — whose `SQLite3Provider_sqlcipher` keying mechanics resolve through the gate's research row before the row leaves `ExtensionGateState.Research`; sqlean-math stays rejected (the compile flag owns it) and Microsoft.SemanticKernel.Connectors.SqliteVec stays rejected as a thin loader dragging a foreign graph.

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
        new ExtensionGate("vec0", Some("sqlite3_vec_init"), "sqlite-vec package runtimes/<rid>/native payload; checksum-verified upstream loadable tarball when the package lags", ExtensionGateState.Gated, Some("vector brute-force fallback")),
        new ExtensionGate("sqlean-regexp", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, None),
        new ExtensionGate("sqlean-crypto", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, Some("xxh128 function row for non-cryptographic identity")),
        new ExtensionGate("sqlean-fuzzy", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, Some("pg_trgm and fuzzystrmatch on the server lanes")),
        new ExtensionGate("sqlean-uuid", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, Some("uuid7 function row")),
        new ExtensionGate("sqlean-stats", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, Some("analytical-lane percentiles")),
        new ExtensionGate("sqlean-text", None, "sqlean per-RID loadable vendored as build content", ExtensionGateState.Deferred, None),
        new ExtensionGate("sqlcipher", None, "SQLite3Provider_sqlcipher provider with an externally supplied native library; the batteries bundle route is deprecated and the two routes never mix", ExtensionGateState.Research, Some("plaintext default")));
}

public sealed record EncryptionGate(string GateRow, FrozenSet<DataClassification> Mandating, Seq<string> Ceremony) {
    public static readonly EncryptionGate Sqlcipher = new(
        GateRow: "sqlcipher",
        Mandating: new[] { DataClassification.Personal, DataClassification.Credential, DataClassification.Secret }.ToFrozenSet(),
        Ceremony: Seq("PRAGMA key", "PRAGMA cipher_migrate", "PRAGMA cipher_version", "PRAGMA rekey"));
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

## [6]-[RESEARCH]

- [ENGINE_FLOOR]: compile-options receipt spellings under the central bundle-line override plus the Batteries_V2 round-trip; snapshot-bracket preconditions — read-transaction entry and checkpoint interaction under the bundled engine.
- [CHECKPOINT_RECEIPT]: the `raw.sqlite3_wal_checkpoint_v2` managed signature and out-parameter shape (checkpoint mode, log-frame count, checkpointed-frame count, busy status) so a `Checkpoint` maintenance fact carries the typed frame counts and a BUSY outcome receipts a retry rather than a swallowed pragma result; the `SQLITE_CHECKPOINT_TRUNCATE`/`PASSIVE`/`FULL`/`RESTART` mode constant spellings resolve against the installed `SQLitePCLRaw.core` assembly before the `Checkpoint` row consumes the `_v2` form beside its current `wal_checkpoint(TRUNCATE)` pragma.
- [RESULT_CODE_ROUTING]: the `raw.sqlite3_extended_result_codes` enablement call and the extended result-code constant spellings (`SQLITE_BUSY`, `SQLITE_CORRUPT`, `SQLITE_NOTADB`, and their extended variants) so a `SqliteException` result-code arm routes CORRUPT and NOTADB to the Repair restore path and BUSY to busy-retry rather than a flat fault; the managed constant surface resolves against the installed `SQLitePCLRaw.core` assembly before the routing arm lands on the maintenance and open ceremonies.
- [EXTENSION_LOADING]: vec0 live load with the `vec_version()` fact and the package-payload versus vendored-tarball sourcing decision; hardened-runtime dlopen acceptance of unsigned extension dylibs inside the signed Rhino host process; SQLCipher provider route with an externally supplied native library on osx-arm64, including the crypto-backend notice set; the `SQLite3Provider_sqlcipher` keying surface for the `EncryptionGate.Sqlcipher` ceremony rows — the connection-string keying keyword versus the inline `PRAGMA key` form, key-literal escaping, and the pooled-physical-open application point.
- [DB_CONFIG_OPS]: the live ordering of the `DbConfig.Hardened` set against pooled physical opens — whether defensive mode and the double-quoted-string-literal DDL/DML rejection must precede the migration ladder or apply after schema creation, and the interaction of `SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION` enablement with the db_config extension-arming path on the same connection.
