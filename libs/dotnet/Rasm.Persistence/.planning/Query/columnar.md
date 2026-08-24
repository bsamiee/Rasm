# [PERSISTENCE_QUERY_COLUMNAR]

Rasm.Persistence runs analytical aggregation, columnar rollup, and artifact egress through one in-process DuckDB engine. `ColumnarSession` owns one posture-configured anchor and gives every concurrent operation a `Duplicate()` lane; `Identifier` admits all SQL identifier roles, `StorePath` admits storage locations, and `SecretResidence` distinguishes session and persistent credentials. ADBC is the Arrow bridge out of that engine and the `COPY (SELECT) TO` rail is its one SQL-mediated egress — two boundaries over the shared Arrow model, never one stretched to name the other.

Four sibling owners hold everything downstream of that engine: producer declarations, the serving plane, this custodian's own dataset rosters, and the lake generations they land in. Producers reach those owners on TWO named wires over ONE admitted vocabulary: `Rasm.Element` and `Rasm.Compute` cross the `[WIRE]: AnalyticsSchema` seam, and `Rasm.Materials` crosses the `[WIRE]: MaterialsDataset` seam over the same Element table vocabulary — two wires because two producers declare on their own terms, one vocabulary because `Query/residence` admits both through a single gate.

## [01]-[INDEX]

- [02]-[COLUMNAR_LANE]: `ColumnarSession` anchors one posture-configured DuckDB engine, `ColumnarProfile` bootstraps its extension roster, and `ColumnarLane` owns the parameterized streaming query, the typed bulk appender, the read-only `ATTACH` mount, the `CREATE SECRET` rail, the ADBC Arrow bridge, and the closed fault rail.
- [03]-[ARTIFACT_EGRESS]: `ArtifactEgress` runs one engine-mediated `COPY (SELECT) TO` rail over the closed `EgressFormat`/`Codec`/`CopyMode`/`ArtifactClass` vocabularies, stamps and reads footer metadata, and scans a generation through `read_parquet`.
- [04]-[RESEARCH]: open verification debts and their routes.

Siblings this page routes to: `Query/residence` owns the producer column vocabulary, the residence family, the seam admission, and the provisioning emitter; `Query/serving` owns the read plan and the four-reach serving plane beside the one relational landing; `Query/datasets` owns the three datasets this custodian declares for itself; `Query/lakehouse` owns the BIM fact egress, the Parquet codec lane, and the `LandingArm` producer spine.

## [02]-[COLUMNAR_LANE]

- Owner: `Identifier`, `StorePath`, `ExecutionThreads`, and `AdbcSql` admit dynamic boundary values; `SecretScope` and `SecretResidence` carry credential resolution and lifetime; `ColumnarSession` owns one anchor and its duplicate lanes; `ColumnarProfile`, `ColumnarExtension`, `ExtensionRepo`, and `ColumnarFault` own posture, capability, bootstrap form, and the fault band; `WarehouseDriver`, `AdbcQuery`, `AdbcBind`, `AdbcRequest`, and `AdbcWarehouse` own the ADBC bridge; `ColumnarLane` owns the operations.
- Cases: `ColumnarProfile` is `Geometry` (`spatial`/`parquet`), `Search` (`vss`/`fts`), `Lakehouse` (`httpfs`/`iceberg`/`delta`/`postgres`, order-free remote scan), `Bim` (`parquet`/`json`/`spatial` over the BimOpenSchema `.duckdb`), and `Federation` (`parquet`/`substrait`/`postgres`, fail-closed on the community row); `ColumnarExtension` rows are `Spatial`, `Vss`, `Fts`, `Parquet`, `Json`, `Icu`, `Httpfs`, `Iceberg`, `Delta`, `Postgres`, `Sqlite`, `Excel`, `Avro`, `Aws`, `Azure`, and `Substrait`; `ExtensionRepo` is `Linked` | `Core` | `Community`; `WarehouseDriver` is `Hive` | `Impala` | `Spark` | `BigQuery`; `AdbcQuery` is `Sql` | `Plan`; `AdbcBind` is `Batch` | `Stream`; `ColumnarFault` closes native query, extension, append, mount, egress, stamp, secret, trust, Delta, and policy admission failures across `8350`–`8359`.
- Entry: `Open` admits the `StoreProfile` lane, then boots and probes the profile; `Query`, `Append`, `Mount`, and `Secret` each own a duplicate connection; `ArrowStream` admits one `AdbcRequest` and drains inside the ADBC statement lifetime; `ArrowPartitions` fans a server-side split; `AdbcWarehouse.Open` mints the connection under a driver row and `Tabular` is the whole driver-to-batches composition a federation tabular port takes.
- Auto: every concurrent operation rides a duplicate lane over the held anchor; profile settings and the extension bootstrap are composition data; `Query` streams, mapped appenders own bulk ingress, and ADBC owns Arrow extraction. Every lane slot derives from ONE verb roster under this lane's own prefix, so a new verb mounts its slot by naming itself.
- Receipt: a session open rides `store.columnar.open` carrying the loaded extension set and the posture; a query rides `store.columnar.query` carrying the `DuckDBQueryProgress` percentage; an append rides `store.columnar.append` carrying the row count; a mount rides `store.columnar.mount` carrying the alias.
- Packages: DuckDB.NET.Data.Full (`DuckDBConnection`/`DuckDBCommand`/`DuckDBConnectionStringBuilder`/`DuckDBMappedAppender<T,TMap>`/`DuckDBAppenderMap<T>`/`DuckDBDataReader`/`DuckDBQueryProgress`/`DuckDBErrorType`), Apache.Arrow, Apache.Arrow.Adbc (`AdbcDatabase`/`AdbcConnection`/`AdbcStatement`/`QueryResult`/`PartitionedResult`/`PartitionDescriptor`/`IArrowArrayStream`; the `Drivers.Apache.Hive2`/`Impala`/`Spark` and `Drivers.BigQuery` driver rows), Rasm.Domain (`TenantId`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new analytical profile is one `ColumnarProfile` row carrying its posture and roster; a new extension is one `ColumnarExtension` row carrying its `ExtensionRepo`; a new install repository is one `ExtensionRepo` row carrying its bootstrap template; a new credential kind is one `SecretScope` row carrying its `PROVIDER` token; a new warehouse is one `WarehouseDriver` row carrying its driver's own `Open`; a new dynamic-token class is one trust-gate `[ValueObject]` admission; a new fault cause is one `ColumnarFault` case; zero new surface — a per-extension NuGet package, a second analytical engine, an open-per-query connection, command interleaving on one handle, a bare `AdbcConnection` no row selected, inline credentials in a path, or a raw-string identifier crossing into engine SQL is the deleted form.
- Law: `Open` is the lane's ONE admission owner — a `StoreProfile` whose engine cannot realize the columnar lane refuses there with the axis named, so every verb below it executes on a proven lane and a per-verb realizability test is the deleted form. UDF registration binds the ANCHOR because the anchor's lifetime IS the session's: a registration on a short-lived duplicate lane gambles the function's catalog lifetime on connection close semantics.
- Boundary: `Identifier` admits aliases, tables, columns, and secret names while `StorePath` admits external paths, so all dynamic text crosses a gate and DuckDB parameters carry values alone. Foreign stores attach READ-ONLY, `substrait` fails closed when unavailable, and provider exceptions lift once into `ColumnarFault`. `ExecutePartitioned` and `ReadPartition` are both `virtual` bodies that THROW `AdbcException.NotImplemented` on a driver declining them — the base class publishes the whole vocabulary and each driver overrides what it serves — so the call lifts once into the typed fault and a consumer reads a refusal naming the driver. `PartitionDescriptor.Descriptor` is a `ReadOnlySpan<byte>` crossing no lambda and no await, so the descriptor STRUCT travels and its span stays inside the redeeming frame. Only the two genuinely portable UDFs register: ISO-8601 ordinal text ordering is native `icu` collation here and the span fold is the native max aggregate, so re-registering either shadows a stronger built-in.

```csharp signature
using System.Buffers;
using System.Buffers.Binary;                      // BinaryPrimitives — the xxh128 UDF's big-endian key pack
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO.Hashing;                          // XxHash128 — the engine-parity content-key UDF
using System.Linq;
using Apache.Arrow;
using Apache.Arrow.Adbc;
using DuckDB.NET.Data;
using DuckDB.NET.Data.Mapping;
using DuckDB.NET.Native;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using Rasm.Domain;                                // TenantId — the frame tenancy the series key packs
using Rasm.Persistence.Element;
using Rasm.Persistence.Store;                     // StoreProfile — the lane-realizability axis Open admits against
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// Trust gates admit identifiers and paths before engine SQL; DuckDB parameters own values only.
[ValueObject<string>]
[ValidationError]
public readonly partial struct Identifier {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (value is not [_, ..] || char.IsAsciiDigit(value[0]) || !value.All(static c => char.IsAsciiLetterOrDigit(c) || c == '_')) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { $"<identifier:{value}>" }));
        }
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct StorePath {
    static readonly SearchValues<char> Hostile = SearchValues.Create("'\";");
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (value is not [_, ..] || value.AsSpan().ContainsAny(Hostile) || value.Any(char.IsControl)) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { $"<store-path:{value}>" }));
        }
    }
}

[ValueObject<int>]
[ValidationError]
public readonly partial struct ExecutionThreads {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 1) validationError = new ValidationError(string.Join(" | ", new object?[] { "execution-threads", value.ToString(CultureInfo.InvariantCulture) }));
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct AdbcSql {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\0')) validationError = new ValidationError(string.Join(" | ", new object?[] { "<adbc-sql>" }));
    }
}

// `ExtensionRepo` owns bootstrap form: linked load, core install, or community install.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtensionRepo {
    public static readonly ExtensionRepo Linked = new("linked", static key => $"LOAD {key};");
    public static readonly ExtensionRepo Core = new("core", static key => $"INSTALL {key}; LOAD {key};");
    public static readonly ExtensionRepo Community = new("community", static key => $"INSTALL {key} FROM community; LOAD {key};");
    [UseDelegateFromConstructor] public partial string Bootstrap(string key);
}

// `ColumnarExtension` rows own extension identity and repository policy on one pinned runtime. `Substrait` is
// community-signed and fails closed during `Open`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarExtension {
    public static readonly ColumnarExtension Spatial   = new("spatial", ExtensionRepo.Core);
    public static readonly ColumnarExtension Vss       = new("vss", ExtensionRepo.Core);
    public static readonly ColumnarExtension Fts       = new("fts", ExtensionRepo.Core);
    public static readonly ColumnarExtension Parquet   = new("parquet", ExtensionRepo.Linked);
    public static readonly ColumnarExtension Json      = new("json", ExtensionRepo.Linked);
    public static readonly ColumnarExtension Icu       = new("icu", ExtensionRepo.Linked);
    public static readonly ColumnarExtension Httpfs    = new("httpfs", ExtensionRepo.Core);
    public static readonly ColumnarExtension Iceberg   = new("iceberg", ExtensionRepo.Core);
    public static readonly ColumnarExtension Delta     = new("delta", ExtensionRepo.Core);
    public static readonly ColumnarExtension Postgres  = new("postgres", ExtensionRepo.Core);
    public static readonly ColumnarExtension Sqlite    = new("sqlite", ExtensionRepo.Core);
    public static readonly ColumnarExtension Excel     = new("excel", ExtensionRepo.Core);
    public static readonly ColumnarExtension Avro      = new("avro", ExtensionRepo.Core);
    public static readonly ColumnarExtension Aws       = new("aws", ExtensionRepo.Core);
    public static readonly ColumnarExtension Azure     = new("azure", ExtensionRepo.Core);
    public static readonly ColumnarExtension Substrait = new("substrait", ExtensionRepo.Community);

    public ExtensionRepo Repo { get; }
    private ColumnarExtension(string key, ExtensionRepo repo) : this(key) => Repo = repo;

    public string Bootstrap => Repo.Bootstrap(Key);
}

// `ColumnarProfile` rows carry dedicated-machine posture and an ordered extension roster. Lakehouse and federation
// disable insertion-order preservation; correctness lanes retain it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarProfile {
    // Every profile shares one memory cap and spill ceiling policy.
    const string MemoryShare = "80%";
    const string SpillShare = "90%";

    public static readonly ColumnarProfile Geometry   = new("geometry", MemoryShare, "geometry.tmp", SpillShare, preserveOrder: true,  [ColumnarExtension.Spatial, ColumnarExtension.Parquet]);
    public static readonly ColumnarProfile Search     = new("search", MemoryShare, "search.tmp", SpillShare, preserveOrder: true,  [ColumnarExtension.Vss, ColumnarExtension.Fts]);
    public static readonly ColumnarProfile Lakehouse  = new("lakehouse", MemoryShare, "lakehouse.tmp", SpillShare, preserveOrder: false, [ColumnarExtension.Httpfs, ColumnarExtension.Iceberg, ColumnarExtension.Delta, ColumnarExtension.Postgres]);
    public static readonly ColumnarProfile Bim        = new("bim", MemoryShare, "bim.tmp", SpillShare, preserveOrder: true,  [ColumnarExtension.Parquet, ColumnarExtension.Json, ColumnarExtension.Spatial]);
    public static readonly ColumnarProfile Federation = new("federation", MemoryShare, "federation.tmp", SpillShare, preserveOrder: false, [ColumnarExtension.Parquet, ColumnarExtension.Substrait, ColumnarExtension.Postgres]);

    public string MemoryCap { get; }
    public string SpillRoot { get; }
    public string SpillCap { get; }
    public bool PreserveOrder { get; }
    public Seq<ColumnarExtension> Roster { get; }
    private ColumnarProfile(string key, string memoryCap, string spillRoot, string spillCap, bool preserveOrder, Seq<ColumnarExtension> roster) : this(key) =>
        (MemoryCap, SpillRoot, SpillCap, PreserveOrder, Roster) = (memoryCap, spillRoot, spillCap, preserveOrder, roster);

    // `max_temp_directory_size` converts spill exhaustion into a loud engine failure rather than an unbounded disk fill.
    public string ConnectionString(StorePath dataSource, ExecutionThreads threads) {
        DuckDBConnectionStringBuilder rows = new() { DataSource = (string)dataSource };
        (rows["threads"], rows["memory_limit"], rows["temp_directory"], rows["max_temp_directory_size"], rows["preserve_insertion_order"]) =
            ((int)threads, MemoryCap, SpillRoot, SpillCap, PreserveOrder);
        return rows.ConnectionString;
    }
}

// `SecretScope` rows own each `CREATE SECRET` type, provider, and persistence target. `httpfs` owns transport, while
// this vocabulary owns credential resolution.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SecretScope {
    public static readonly SecretScope S3       = new("s3", "credential_chain", "objstore_db");
    public static readonly SecretScope Azure    = new("azure", "credential_chain", "objstore_db");
    public static readonly SecretScope Postgres = new("postgres", "config", "marten_db");
    public string Provider { get; }
    public string PersistInto { get; }
    private SecretScope(string key, string provider, string persistInto) : this(key) => (Provider, PersistInto) = (provider, persistInto);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SecretResidence {
    private SecretResidence() { }
    public sealed record Session : SecretResidence;
    public sealed record Persistent : SecretResidence;
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Native error classes determine typed cases; generated identity supplies their numeric codes.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnarFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Columnar;
    private ColumnarFault() { }
    [FaultCase(0)]
    public sealed partial record QueryFailed(Error Cause, DuckDBErrorType Kind) : ColumnarFault(), ICausedFault;
    [FaultCase(1)]
    public sealed partial record ExtensionGap(string Extension) : ColumnarFault();
    [FaultCase(2)]
    public sealed partial record AppendRefused(string Table, DuckDBErrorType Kind, Error Cause) : ColumnarFault(), ICausedFault;
    [FaultCase(3)]
    public sealed partial record MountRefused(string Alias, DuckDBErrorType Kind, Error Cause) : ColumnarFault(), ICausedFault;
    [FaultCase(4)]
    public sealed partial record EgressRefused(string Destination, DuckDBErrorType Kind, Error Cause) : ColumnarFault(), ICausedFault;
    [FaultCase(5)]
    public sealed partial record UnstampedArtifact(string Path) : ColumnarFault();
    [FaultCase(6)]
    public sealed partial record SecretRefused(string Name, DuckDBErrorType Kind, Error Cause) : ColumnarFault(), ICausedFault;
    [FaultCase(7)]
    public sealed partial record TrustRefused(string Token) : ColumnarFault();
    [FaultCase(8)]
    public sealed partial record DeltaRefused(string Table, Error Cause) : ColumnarFault(), ICausedFault;
    [FaultCase(9)]
    public sealed partial record PolicyRefused(string Policy, string Found) : ColumnarFault();

    public override string Message => Switch(
        queryFailed:       static c => $"<columnar-query:{c.Cause.Message}>",
        extensionGap:      static c => $"<columnar-extension:{c.Extension}>",
        appendRefused:     static c => $"<columnar-append:{c.Table}>",
        mountRefused:      static c => $"<columnar-mount:{c.Alias}>",
        egressRefused:     static c => $"<columnar-egress:{c.Destination}>",
        unstampedArtifact: static c => $"<columnar-unstamped:{c.Path}>",
        secretRefused:     static c => $"<columnar-secret:{c.Name}>",
        trustRefused:      static c => $"<columnar-trust:{c.Token}>",
        deltaRefused:      static c => $"<columnar-delta:{c.Table}:{c.Cause.Message}>",
        policyRefused:     static c => $"<columnar-policy:{c.Policy}:{c.Found}>");

    public static Error Lift(Error error, Func<Error, DuckDBException, ColumnarFault> arm) =>
        error.Exception.Case is DuckDBException engine ? arm(error, engine) : error;

    // Trust-gate admissions exclusively reach generator text, so `Create` preserves the trust fault without
    // fabricating a native kind it never observed.
}

// --- [SERVICES] ---------------------------------------------------------------------------
// `ColumnarSession` holds one native anchor per source and creates `Duplicate()` lanes for concurrent drains. Private
// construction requires profile bootstrap, and negative progress remains `None`.
public sealed class ColumnarSession : IDisposable {
    readonly DuckDBConnection anchor;
    public ColumnarProfile Profile { get; }
    public Seq<string> Loaded { get; }
    internal DuckDBConnection Anchor => anchor;

    internal ColumnarSession(DuckDBConnection anchor, ColumnarProfile profile, Seq<string> loaded) =>
        (this.anchor, Profile, Loaded) = (anchor, profile, loaded);

    public DuckDBConnection Lane() => anchor.Duplicate();
    public Option<double> Progress() {
        DuckDBQueryProgress progress = anchor.GetQueryProgress();
        return progress.Percentage >= 0 ? Some(progress.Percentage) : None;
    }
    public void Dispose() => anchor.Dispose();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ColumnarLane {
    // `Lane` is the token `StoreProfile.Lanes` spells for this owner, so the literal has one home on the page and
    // every slot below derives its prefix from it.
    public const string Lane = "columnar";

    public static StoreSlot Of(string verb) => StoreSlot.Create($"store.{Lane}.{verb}");

    // Residence slots carry NAMES because `Store/observability#STORE_INSTRUMENTS` keys its projection arms on them; a
    // slot no arm folds stays a derived row.
    public static readonly StoreSlot ProvisionSlot = Of("residence.provision");
    public static readonly StoreSlot ReadSlot = Of("residence.read");
    public static readonly StoreSlot IngestSlot = Of("residence.ingest");

    // Verb roster, not a literal roster: each slot derives from the verb it meters, so a spelling drifting from its
    // own verb cannot exist and a new verb mounts by naming itself. Landing slots derive from the
    // `Query/lakehouse#FLAT_TABLE_EGRESS` arm roster on the same terms.
    static readonly Seq<string> Verbs = Seq(
        "open", "query", "append", "mount", "egress", "stamp",
        "flattable", "materialize", "frames", "parquet", "scan", "series.jobs");

    public static readonly Seq<StoreSlot> Slots =
        Verbs.Map(Of) + Seq(ProvisionSlot, ReadSlot, IngestSlot) + toSeq(LandingArm.Items).Map(static arm => arm.Slot);

    // `Open` is the lane's ONE admission owner: an engine whose profile cannot realize the lane refuses HERE naming the
    // axis, so an embedded deployment learns at profile selection and never at its first aggregation. Past that
    // gate `Open` applies ordered bootstrap policy, then verifies every roster row through `duckdb_extensions()`.
    public static IO<ColumnarSession> Open(StoreProfile store, ColumnarProfile profile, StorePath dataSource, ExecutionThreads threads) =>
        !store.Admits(Lane)
        ? IO.fail<ColumnarSession>(new ColumnarFault.PolicyRefused("store-lane", store.Key))
        : IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            DuckDBConnection anchor = new(profile.ConnectionString(dataSource, threads));
            await anchor.OpenAsync().ConfigureAwait(false);
            await using (DuckDBCommand bootstrap = anchor.CreateCommand()) {
                foreach (ColumnarExtension extension in profile.Roster) { bootstrap.CommandText = extension.Bootstrap; await bootstrap.ExecuteNonQueryAsync().ConfigureAwait(false); }
            }
            await using DuckDBCommand probe = anchor.CreateCommand();
            probe.CommandText = "SELECT extension_name FROM duckdb_extensions() WHERE loaded";
            await using DuckDBDataReader reader = (DuckDBDataReader)await probe.ExecuteReaderAsync().ConfigureAwait(false);
            Seq<string> loaded = Seq<string>();
            while (await reader.ReadAsync().ConfigureAwait(false)) loaded = loaded.Add(reader.GetString(0));
            return Fin<ColumnarSession>.Succ(new ColumnarSession(anchor, profile, loaded));
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            static (cause, engine) => new ColumnarFault.QueryFailed(cause, engine.ErrorType))))
        .Bind(IO.liftFin)
        .Bind(static session => AdmitLoaded(session));

    static IO<ColumnarSession> AdmitLoaded(ColumnarSession session) {
        Seq<string> missing = toSeq(session.Profile.Roster.Map(static extension => extension.Key)).Filter(key => !session.Loaded.Contains(key));
        if (missing.IsEmpty) return IO.pure(session);
        session.Dispose();
        return IO.fail<ColumnarSession>(new ColumnarFault.ExtensionGap(string.Join(",", missing)));
    }

    // Engine-parity UDF registration: the embedded floor's identity capabilities answer on BOTH embedded engines, so a
    // rollup joining on `xxh128(content)` runs unchanged over SQLite or DuckDB.
    public static IO<Unit> Register(ColumnarSession session) =>
        IO.lift(() => Op.Of().Catch(() => {
            session.Anchor.RegisterScalarFunction<string>("uuid7", static () => Guid.CreateVersion7().ToString("N"));
            session.Anchor.RegisterScalarFunction<byte[], byte[]>("xxh128", static bytes => {
                byte[] key = new byte[16];
                BinaryPrimitives.WriteUInt128BigEndian(key, XxHash128.HashToUInt128(bytes));
                return key;
            });
            return Fin<Unit>.Succ(unit);
        }).MapFail(error => ColumnarFault.Lift(error,
            static (cause, engine) => new ColumnarFault.QueryFailed(cause, engine.ErrorType))))
        .Bind(IO.liftFin);

    // Streaming queries run on `Duplicate()` lanes and bind interpolation holes as named `$pN` parameters. One
    // seam-local list accumulates rows once before `toSeq`, avoiding persistent-sequence forcing per row.
    public static IO<Seq<T>> Query<T>(ColumnarSession session, FormattableString sql, Func<DuckDBDataReader, T> shape) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            DuckDBConnection lane = session.Lane();
            await using (lane.ConfigureAwait(false)) {
                await using DuckDBCommand command = lane.CreateCommand();
                object[] placeholders = Enumerable.Range(0, sql.ArgumentCount).Select(static i => (object)$"$p{i}").ToArray();
                (command.CommandText, command.UseStreamingMode) = (string.Format(CultureInfo.InvariantCulture, sql.Format, placeholders), true);
                for (int i = 0; i < sql.ArgumentCount; i++) command.Parameters.Add(new DuckDBParameter($"p{i}", sql.GetArgument(i)));
                await using DuckDBDataReader reader = (DuckDBDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
                List<T> rows = [];
                while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
                return Fin<Seq<T>>.Succ(toSeq(rows));
            }
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            static (cause, engine) => new ColumnarFault.QueryFailed(cause, engine.ErrorType))))
        .Bind(IO.liftFin);

    // `DuckDBAppenderMap<T>` validates declared columns before `AppendRecords` streams and `Close` flushes the batch.
    public static IO<long> Append<T, TMap>(ColumnarSession session, Identifier table, Seq<T> rows) where TMap : DuckDBAppenderMap<T>, new() =>
        IO.lift(() => Op.Of().Catch(() => {
            using DuckDBConnection lane = session.Lane();
            DuckDBMappedAppender<T, TMap> appender = lane.CreateAppender<T, TMap>((string)table);
            appender.AppendRecords(rows);
            appender.Close();
            return Fin<long>.Succ(rows.Count);
        }).MapFail(error => ColumnarFault.Lift(error,
            (cause, engine) => new ColumnarFault.AppendRefused(table, engine.ErrorType, cause))))
        .Bind(IO.liftFin);

    // `Mount` admits aliases and paths and attaches foreign stores read-only. Object-store paths resolve credentials
    // through `Secret` before attachment.
    public static IO<Fin<Unit>> Mount(ColumnarSession session, Identifier alias, StorePath store, ColumnarExtension typed) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = $"ATTACH IF NOT EXISTS '{store}' AS {alias} (TYPE {typed.Key}, READ_ONLY)";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            (cause, engine) => new ColumnarFault.MountRefused(alias, engine.ErrorType, cause))));

    // `Secret` doubles literal quotes and forbids credentials in paths. `SecretResidence.Persistent` writes into the
    // attached credential store; `Session` remains connection-scoped.
    public static IO<Fin<Unit>> Secret(ColumnarSession session, SecretScope scope, Identifier name, Seq<(Identifier Key, string Value)> config, SecretResidence residence) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            string into = residence is SecretResidence.Persistent ? $" IN {scope.PersistInto}" : string.Empty;
            Seq<string> rows = config.Map(static pair => $"{pair.Key} '{pair.Value.Replace("'", "''", StringComparison.Ordinal)}'");
            command.CommandText = $"CREATE OR REPLACE SECRET {name}{into} (TYPE {scope.Key}, PROVIDER {scope.Provider}, {string.Join(", ", rows)})";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            (cause, engine) => new ColumnarFault.SecretRefused(name, engine.ErrorType, cause))));

    // ADBC owns SQL and Substrait execution and batch or stream binding on one statement seam. `drain` runs inside
    // statement lifetime so no `QueryResult.Stream` escapes disposal.
    public static IO<T> ArrowStream<T>(AdbcConnection adbc, AdbcRequest request, Func<QueryResult, ValueTask<T>> drain) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            using AdbcStatement statement = adbc.CreateStatement();
            request.Apply(statement);
            QueryResult result = await statement.ExecuteQueryAsync().ConfigureAwait(false);
            return Fin<T>.Succ(await drain(result).ConfigureAwait(false));
        }).ConfigureAwait(false)).Bind(IO.liftFin);

    // PARTITIONED execution over the same statement seam: `ExecutePartitioned` hands back the server-side split as
    // opaque descriptors and each redeems on its own `ReadPartition` stream, so a partition-parallel consumer fans out
    // without a second transport and without the raw ADBC surface reaching it.
    public static IO<Fin<ArrowPartitions>> ArrowPartitions(AdbcConnection adbc, AdbcRequest request) =>
        IO.lift(() => Op.Of().Catch(() => {
            using AdbcStatement statement = adbc.CreateStatement();
            request.Apply(statement);
            PartitionedResult split = statement.ExecutePartitioned();
            return Fin.Succ(new ArrowPartitions(adbc, split.Schema, split.AffectedRows, toSeq(split.PartitionDescriptors)));
        }));
}

// Redemption face of one partitioned execution. The connection rides the value because a descriptor is meaningless
// without the connection that minted it — handing a consumer bare descriptors invites redemption against a second
// connection the server never split for.
public sealed record ArrowPartitions(AdbcConnection Connection, Schema Schema, long AffectedRows, Seq<PartitionDescriptor> Descriptors) {
    // One partition's Arrow stream; the caller owns disposal exactly as it owns a `QueryResult.Stream`.
    public IO<Fin<IArrowArrayStream>> Redeem(PartitionDescriptor descriptor) =>
        IO.lift(() => Op.Of().Catch(() => Fin.Succ(Connection.ReadPartition(descriptor))));
}

// DRIVER axis binding the admitted ADBC packages: each row names its driver, its parameter vocabulary (host/port/path
// /auth per the Apache Thrift drivers; project/dataset/credential for BigQuery), and opens the
// `AdbcDatabase` → `AdbcConnection` pair through that driver.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WarehouseDriver {
    public static readonly WarehouseDriver Hive = new("hive", static parameters => new Apache.Arrow.Adbc.Drivers.Apache.Hive2.HiveServer2Driver().Open(parameters));
    public static readonly WarehouseDriver Impala = new("impala", static parameters => new Apache.Arrow.Adbc.Drivers.Apache.Impala.ImpalaDriver().Open(parameters));
    public static readonly WarehouseDriver Spark = new("spark", static parameters => new Apache.Arrow.Adbc.Drivers.Apache.Spark.SparkDriver().Open(parameters));
    public static readonly WarehouseDriver BigQuery = new("bigquery", static parameters => new Apache.Arrow.Adbc.Drivers.BigQuery.BigQueryDriver().Open(parameters));

    [UseDelegateFromConstructor]
    public partial AdbcDatabase Open(IReadOnlyDictionary<string, string> parameters);
}

public static class AdbcWarehouse {
    // One open owner: the driver row selects, parameters admit, the database and connection open under that row, and
    // every driver exception converts ONCE to the typed columnar fault.
    public static IO<Fin<AdbcConnection>> Open(WarehouseDriver driver, HashMap<string, string> parameters) =>
        IO.lift(() => Op.Of().Catch(() => parameters.IsEmpty || parameters.Keys.Exists(string.IsNullOrWhiteSpace)
            ? Fin<AdbcConnection>.Fail(new ColumnarFault.PolicyRefused("adbc-parameters", driver.Key))
            : Fin<AdbcConnection>.Succ(driver.Open(parameters.ToDictionary(static p => p.Key, static p => p.Value)).Connect(new Dictionary<string, string>()))));

    // Driver-to-batches runs WHOLE here, in the shape `Query/federation#PLAN_EXECUTION`'s `Tabular` port
    // declares: a caller names a driver row and a request and takes batches, so the connection has an owner that
    // selected it, admitted its parameters, converted its failures, and released it — the bare `AdbcConnection` a
    // consumer would otherwise hold is the unbound form this composition deletes.
    public static IO<Fin<Seq<RecordBatch>>> Tabular(WarehouseDriver driver, HashMap<string, string> parameters, AdbcRequest request) =>
        Open(driver, parameters).Bind(opened => opened.Match(
            Succ: adbc => IO.pure(adbc).Bracket(
                connection => ColumnarLane.ArrowStream(connection, request, Batches).Map(Fin.Succ),
                static connection => IO.lift(() => Op.Of().Catch(() => { connection.Dispose(); return Fin<Unit>.Succ(unit); })).Bind(IO.liftFin)),
            Fail: error => IO.pure(Fin<Seq<RecordBatch>>.Fail(error))));

    // Drivers answering with no stream answered an EMPTY result, which is a legitimate outcome rather than an
    // absent one, so the drain reads absence as an empty run and never as a refusal.
    static ValueTask<Seq<RecordBatch>> Batches(QueryResult result) =>
        Optional(result.Stream).Match(
            Some: static stream => Drained(stream),
            None: static () => ValueTask.FromResult(Seq<RecordBatch>()));

    static async ValueTask<Seq<RecordBatch>> Drained(IArrowArrayStream stream) {
        using (stream) {
            List<RecordBatch> drained = [];
            while (await stream.ReadNextRecordBatchAsync().ConfigureAwait(false) is { } batch) { drained.Add(batch); }
            return toSeq(drained);
        }
    }
}

// `AdbcQuery` closes the statement seam over composed SQL and portable Substrait bytes. Federation owns plan identity;
// this seam executes without rehashing.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdbcQuery {
    private AdbcQuery() { }
    public sealed record Sql(AdbcSql Composed) : AdbcQuery;
    public sealed record Plan(byte[] Substrait) : AdbcQuery;
}

// ONE application of a request onto a statement, so the streaming and the partitioned entry cannot diverge on which
// half of the request reaches the driver: a bind the partitioned arm dropped executes the parameterized plan against
// no parameters, and the driver answers a partition set for a question nobody asked.
public sealed record AdbcRequest(AdbcQuery Query, Option<AdbcBind> Bind) {
    public void Apply(AdbcStatement statement) {
        Query.Switch(
            sql:  s => statement.SqlQuery = (string)s.Composed,
            plan: p => statement.SubstraitPlan = p.Substrait);
        Bind.Iter(bind => bind.Switch(
            batch:  b => statement.Bind(b.Value, b.Value.Schema),
            stream: s => statement.BindStream(s.Value)));
    }
}

// `AdbcBind` closes binding arity over one batch or an `IArrowArrayStream`. `BindStream` preserves chunking without
// materializing a batch.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdbcBind {
    private AdbcBind() { }
    public sealed record Batch(Apache.Arrow.RecordBatch Value) : AdbcBind;
    public sealed record Stream(IArrowArrayStream Value) : AdbcBind;
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                          |
| :-----: | :------------------ | :---------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | engine session      | one anchor + `Duplicate()`                | never command interleaving                                         |
|  [02]   | extension bootstrap | ordered repo-derived `INSTALL`/`LOAD` SQL | tri-state `ExtensionRepo`; fail-closed `duckdb_extensions()` probe |
|  [03]   | consistency stance  | async, `StalenessWatermark`               | never read by interactive correctness without the wait             |
|  [04]   | index ownership     | DuckDB spatial/vss are aggregators        | GiST/pgvector own the transactional index                          |
|  [05]   | credential rail     | `CREATE SECRET` over `SecretScope`        | quote-doubled config literals; never an inline path key            |
|  [06]   | Arrow bridge        | ADBC driver manager → `IArrowArrayStream` | no managed Arrow member; params via `AdbcStatement.Bind`           |
|  [07]   | driver custody      | `WarehouseDriver` row opens every lane    | `Tabular` owns the whole span; never a bare unbound connection     |
|  [08]   | fault rail          | `DuckDBException` → `ColumnarFault`       | discriminated on `DuckDBErrorType`, never a raw ADO throw          |
|  [09]   | trust gate          | `Identifier`/`StorePath`                  | one grammar per identity regime                                    |
|  [10]   | plan execution      | `AdbcQuery.Plan` → `SubstraitPlan`        | the federation intra-leg edge on the one ADBC statement seam       |
|  [11]   | lane admission      | `StoreProfile.Admits(Lane)` inside `Open` | refused once with the axis named; never a per-verb lane test       |
|  [12]   | slot derivation     | one verb roster under the lane prefix     | a literal drifting from the verb it meters cannot exist            |

## [03]-[ARTIFACT_EGRESS]

- Owner: `Codec` is the compression vocabulary whose `.Key` IS the `COPY` `COMPRESSION` token; `CopyMode` the destination-collision vocabulary; `EgressFormat` the format vocabulary carrying its JSON `ARRAY` row and the row-group default its grouping admits; `CopyBody` the composed TYPED projection the COPY body embeds — never raw caller SQL; `ArtifactClass` the closed analytical-artifact declaration deriving emission, partition, and the footer stamp from one row; `ArtifactEgress` the static surface owning the rail, the footer-metadata stamp read, and the `read_parquet` generation scan.
- Cases: `EgressFormat` is `Parquet` (grouping, carrying the default row count), `Csv`, and `Json` (carrying `ARRAY true`); `Codec` is `Zstd`/`Snappy`; `CopyMode` is `Overwrite`/`OverwriteOrIgnore`/`Append`; `ArtifactClass` is `BimRollup` (the QTO Parquet generation, Zstd, overwrite), `CoverageFeed` (the partitioned geospatial-coverage JSON feed, Snappy, append), and `TelemetryEvidence` (the receipt-stream Parquet generation, Zstd, domain-partitioned, append — the receipt plane is append-only truth, so `Overwrite` is unrepresentable for it by row).
- Entry: `Publish(ColumnarSession, ArtifactClass, CopyBody, StorePath, UInt128)` runs the one `COPY (body) TO destination (…)` statement assembled from the artifact-class rows over the trust-gated body and destination, the stamp hex-formatted at the seam so caller raw text is unrepresentable; `StampOf(ColumnarSession, StorePath)` reads the content stamp from the Parquet footer through `parquet_kv_metadata` without decoding data; `Generation(StorePath)` derives the `read_parquet` glob scan over a generation directory with `union_by_name`/`hive_partitioning`/`filename` provenance.
- Auto: one `COPY (SELECT) TO` statement owns engine-mediated egress — `FORMAT`, `COMPRESSION`, `ROW_GROUP_SIZE`, and `PARTITION_BY` interpolate beside the shared destination from the artifact-class rows, so a mistyped token is unrepresentable rather than a runtime SQL parse error and a second export path per format is the deleted form. One `KV_METADATA` stamp binds the artifact's content identity into the footer, and the generation read is `read_parquet` over a path or glob so a growing generation directory changes only the path argument.
- Receipt: an egress rides `store.columnar.egress` carrying the artifact class and the destination; a footer stamp read rides `store.columnar.stamp` carrying the content identity.
- Packages: DuckDB.NET.Data.Full (`DuckDBCommand.ExecuteNonQuery`/`ExecuteScalar`/`DuckDBParameter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact class is one `ArtifactClass` row deriving emission, partition, and stamp; a new egress format, codec, or collision mode is one vocabulary row whose `.Key` IS the `COPY` token; a format that groups declares its own default row count; zero new surface — a per-format export path, a `FORMAT` value stretched to name a transport the engine never performs, a filename-convention identity trust, an in-place generation rewrite, or a row-group literal restated per class is the deleted form.
- Law: row-group geometry is the unit of scan parallelism and zonemap pruning, so the emitted size is the class's own override or the FORMAT's declared default and a non-grouping format has no size to emit — groups near the default row count prune well, and tiny groups are the signature of an append-per-batch exporter, which stages through a view and exports once. Partitioning is a pruning instrument at cardinality in the tens to low thousands, never a uniqueness scheme.
- Boundary: the `COPY (SELECT) TO` rail is the SQL-mediated egress LANE, not the egress monopoly — the zero-copy `ArrowStream` ADBC bridge and the direct managed `Query/lakehouse#FLAT_TABLE_EGRESS` `ParquetSharp.Arrow` codec are distinct lanes a `FORMAT` token cannot express, so a non-SQL egress lands as a sibling lane and never as a `FORMAT` row. Artifact identity is the footer content stamp and the declared `ArtifactClass` shape, never a filename convention: a renamed artifact keeps its identity and a stamp that no longer matches its content is corruption, not drift. Generations are IMMUTABLE — compaction is a new artifact written beside the old with a new stamp — and `FIELD_IDS` at export with an id-keyed scan map make renames non-breaking across generations. `COPY` is a filesystem effect outside transaction rollback, so publication composes the atomic-write protocol `Element/codec#SNAPSHOT_SPINE` owns. Lakehouse `delta`/`iceberg` scans read the same tables the managed `PublishDelta` commit produces: DuckDB the read projection, the managed Delta log the versioned publication, meeting at the table path and never re-authoring each other's metadata.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// COPY-token smart enums own emitted format, codec, and collision literals, so a mistyped `OVERWRITE_OR_INGORE` is
// unrepresentable rather than a runtime SQL parse error.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Codec {
    public static readonly Codec Zstd = new("zstd");
    public static readonly Codec Snappy = new("snappy");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CopyMode {
    public static readonly CopyMode Overwrite = new("OVERWRITE");
    public static readonly CopyMode OverwriteOrIgnore = new("OVERWRITE_OR_IGNORE");
    public static readonly CopyMode Append = new("APPEND");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EgressFormat {
    // Engine default row count, declared once on the row that groups: a class restating it is a literal
    // drifting from the geometry it names.
    public const int GroupRows = 122_880;

    public static readonly EgressFormat Parquet = new("parquet", None, Some(GroupRows));
    public static readonly EgressFormat Csv = new("csv", None, None);
    public static readonly EgressFormat Json = new("json", Some("ARRAY true"), None);

    public Option<string> ArrayRow { get; }
    // Grouping is a CAPABILITY carried as the default it admits, not a flag beside a size every class restates: a
    // format that groups carries its own row count and a format that does not has no size to carry, so the two facts
    // cannot disagree and a `ROW_GROUP_SIZE` under a row-less format is unrepresentable.
    public Option<int> Grouping { get; }
    private EgressFormat(string key, Option<string> arrayRow, Option<int> grouping) : this(key) => (ArrayRow, Grouping) = (arrayRow, grouping);
}

// --- [MODELS] -----------------------------------------------------------------------------
// `CopyBody` composes an admitted source and non-empty admitted columns. Filtered or joined egress stages through a
// view the parameterized query rail creates.
public sealed record CopyBody(Identifier Source, Seq<Identifier> Columns) {
    public string Sql => Columns.IsEmpty
        ? $"SELECT * FROM {Source}"
        : $"SELECT {string.Join(", ", Columns)} FROM {Source}";
}

// Artifact-class rows derive complete `COPY` policy and immutable generation paths, keyed exactly as every sibling
// egress vocabulary is so `Items`, `Get`, and `Validate` serve the census the analytics registry reads. `KV_METADATA`
// carries the content stamp in the footer rather than the filename.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactClass {
    public static readonly ArtifactClass BimRollup = new("bim-rollup", EgressFormat.Parquet, Codec.Zstd, None, None, CopyMode.Overwrite);
    public static readonly ArtifactClass CoverageFeed = new("coverage-feed", EgressFormat.Json, Codec.Snappy, None, Some(Identifier.Create("crs")), CopyMode.Append);
    // Evidence extract of the kernel receipt stream: one immutable Parquet generation per emission window, partitioned
    // by the capability domain a query joins on, appended so a window never overwrites a sibling.
    public static readonly ArtifactClass TelemetryEvidence = new("telemetry-evidence", EgressFormat.Parquet, Codec.Zstd, None, Some(Identifier.Create("domain")), CopyMode.Append);

    public EgressFormat Format { get; }
    public Codec Codec { get; }
    // Classes state a row count only where it DIFFERS from the format's default, so the roster carries an override
    // and never a transcription.
    public Option<int> RowGroup { get; }
    public Option<Identifier> PartitionKey { get; }
    public CopyMode Mode { get; }

    private ArtifactClass(string key, EgressFormat format, Codec codec, Option<int> rowGroup, Option<Identifier> partitionKey, CopyMode mode) : this(key) =>
        (Format, Codec, RowGroup, PartitionKey, Mode) = (format, codec, rowGroup, partitionKey, mode);

    // Emitted geometry is the class's override or the format's own default; a non-grouping format supplies
    // neither, so the clause is absent rather than zero.
    public Option<int> Rows => RowGroup | Format.Grouping;

    public string Egress(CopyBody body, StorePath destination, UInt128 stamp) =>
        $"COPY ({body.Sql}) TO '{destination}' ({string.Join(", ",
            Seq(Some($"FORMAT {Format.Key}"), Some($"COMPRESSION {Codec.Key}"),
                Rows.Map(static rows => $"ROW_GROUP_SIZE {rows.ToString(CultureInfo.InvariantCulture)}"), Format.ArrayRow,
                PartitionKey.Map(static key => $"PARTITION_BY ({key})"), Some(Mode.Key),
                Some($"KV_METADATA {{ stamp: '{stamp.ToString("x32", CultureInfo.InvariantCulture)}' }}")).Somes())})";
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ArtifactEgress {
    public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, CopyBody body, StorePath destination, UInt128 stamp) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = artifact.Egress(body, destination, stamp);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            (cause, engine) => new ColumnarFault.EgressRefused(destination, engine.ErrorType, cause))));

    // Artifact admission reads the content stamp from footer metadata without decoding rows; a missing or malformed
    // stamp rails `UnstampedArtifact`.
    public static IO<Fin<UInt128>> StampOf(ColumnarSession session, StorePath artifact) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = "SELECT decode(value) FROM parquet_kv_metadata($path) WHERE decode(key) = 'stamp'";
            command.Parameters.Add(new DuckDBParameter("path", (string)artifact));
            return Fin<Option<string>>.Succ(Optional(await command.ExecuteScalarAsync().ConfigureAwait(false)).Map(static held => (string)held));
        }).ConfigureAwait(false)).MapFail(error => ColumnarFault.Lift(error,
            static (cause, engine) => new ColumnarFault.QueryFailed(cause, engine.ErrorType))))
        .Map(captured => captured.Bind(stamp => stamp
            .Bind(static held => ParseStamp(held))
            .Match(Some: Fin<UInt128>.Succ, None: () => Fin<UInt128>.Fail(new ColumnarFault.UnstampedArtifact(artifact)))));

    static Option<UInt128> ParseStamp(string held) {
        bool parsed = UInt128.TryParse(held, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 key);
        return parsed ? Some(key) : None;
    }

    // Generation reads use `read_parquet` with schema union, hive keys, and row provenance: `union_by_name` makes
    // additive columns compatible by construction (absent reads NULL) and `filename`/`file_row_number` pin per-row
    // provenance. One unquoted parameter hole carries the whole glob so DuckDB binds the path.
    public static FormattableString Generation(StorePath root) =>
        $"SELECT *, filename, file_row_number FROM read_parquet({$"{(string)root}/**/*.parquet"}, union_by_name = true, hive_partitioning = true, filename = true, file_row_number = true)";
}
```

| [INDEX] | [POLICY]          | [VALUE]                                      | [BINDING]                                                         |
| :-----: | :---------------- | :------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | engine egress     | one `COPY (SELECT) TO` rail                  | `.Key` IS the COPY token; a second export path is deleted         |
|  [02]   | artifact identity | footer `KV_METADATA` stamp                   | rides the file, never a filename convention                       |
|  [03]   | row-group size    | the class override or the format's default   | a non-grouping format emits no clause; never a restated literal   |
|  [04]   | partitioning      | `PARTITION_BY` hive directories              | a pruning instrument, never a uniqueness scheme                   |
|  [05]   | generation read   | `read_parquet` glob + `union_by_name`        | generations immutable; additive columns compatible                |
|  [06]   | non-SQL egress    | sibling ADBC / `ParquetSharp.Arrow` lane     | never a `FORMAT` token stretched to name a transport              |
|  [07]   | copy body         | composed `CopyBody`, trust-gated identifiers | never raw caller SQL; filtered egress stages via the `Query` rail |

## [04]-[RESEARCH]

(none)
