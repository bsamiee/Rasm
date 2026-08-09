# [PERSISTENCE_QUERY_COLUMNAR]

Rasm.Persistence runs analytical aggregation, columnar rollup, lakehouse scan, and artifact egress through one in-process DuckDB engine. `ColumnarSession` owns one posture-configured anchor and gives every concurrent operation a `Duplicate()` lane. `Identifier` admits all SQL identifier roles, `StorePath` admits storage locations, and `SecretResidence` distinguishes session and persistent credentials. Parquet and ADBC remain distinct boundaries over the shared Arrow model. Beyond the in-process engine this page owns the ANALYTICS RESIDENCE FAMILY — one parameterized row set spanning the temporal projection tier, the interactive wide-event tier, and the cold tail, each answering the same capability columns, provisioned by one branch-owned DDL emitter, and read through one Substrait plan lowered per dialect.

This package is the branch's single columnar custodian: producers hand typed record-batch schemas across the `[WIRE]: AnalyticsSchema` seam and every writer, residence, slot, and serving plane homes here. Residences carry ZERO authority — the receipt stream and the identity-tier journal own truth, a residence drops at warm-up cost and rebuilds from evidence, and no residence carries a cardinality ceiling because unbounded dimensionality is the reason it exists.

## [01]-[INDEX]

- [02]-[COLUMNAR_LANE]: `ColumnarSession` anchors one posture-configured DuckDB engine, `ColumnarProfile` bootstraps its extension roster, and `ColumnarLane` owns the parameterized streaming query, the typed bulk appender, the read-only `ATTACH` mount, the `CREATE SECRET` rail, the ADBC Arrow bridge, and the closed fault rail.
- [03]-[ARTIFACT_EGRESS]: `ArtifactEgress` runs one engine-mediated `COPY (SELECT) TO` rail over the closed `EgressFormat`/`Codec`/`Collision`/`ArtifactClass` vocabularies, stamps and reads footer metadata, and scans a generation through `read_parquet`.
- [04]-[FLAT_TABLE_EGRESS]: `BimOpenSchemaProjection` writes the columnar BIM facts co-transactionally, and `FlatTableEgress` owns the in-corpus eleven-table `.duckdb` write, the daemon materialization, the `ParquetSharp.Arrow` read and PME-encrypted write, the metadata-only Delta publication versioning each generation to its AS-OF cut, the hive-partitioned lake scan, the Arrow IPC decode arm, and the `LandingArm` producer spine.
- [05]-[ANALYTICS_RESIDENCE]: `AnalyticsSchema` admits a producer seam in SQL and Arrow, `Residence` rows carry dialect and honest degradation, `ResidenceDdl` provisions, `ResidencePlan` lowers one plan under one `ResidenceScope`, `ResidenceRead` serves every `ResidenceReach`, `SeriesKind` roots the hypertable roster under one `SeriesSelector`, and the op-log and receipt-evidence planes close both seams.
- [06]-[RESEARCH]: open verification debts and their routes.

## [02]-[COLUMNAR_LANE]

- Owner: `Identifier`, `StorePath`, `ExecutionThreads`, and `AdbcSql` admit dynamic boundary values; `SecretResidence` carries credential lifetime; `AdbcRequest` carries one statement door and its optional bind; `ColumnarSession` owns one anchor and duplicate lanes; `ColumnarProfile`, `ColumnarExtension`, `ColumnarFault`, `AdbcQuery`, and `ColumnarLane` own posture, capabilities, faults, execution shape, and operations.
- Cases: `ColumnarProfile` is `Geometry` (`spatial`/`parquet`, memory-heavy spatial aggregation posture), `Search` (`vss`/`fts`, ANN/BM25 columnar posture), `Lakehouse` (`httpfs`/`iceberg`/`delta`/`postgres`, order-free remote-scan posture with `preserve_insertion_order` false), `Bim` (`parquet`/`json` over the BimOpenSchema `.duckdb`, read-your-writes BIM analytics posture), `Federation` (`parquet`/`substrait`/`postgres`, fail-closed on the community row); `ColumnarExtension` rows are `Spatial`, `Vss`, `Fts`, `Parquet`, `Json`, `Httpfs`, `Iceberg`, `Delta`, `Postgres`, `Sqlite`, `Excel`, `Avro`, `Icu`, `Aws`, `Azure`, and `Substrait`; `ColumnarFault` closes native query, extension, append, mount, egress, stamp, secret, trust, Delta, and policy admission failures across `8350`–`8359`.
- Entry: `Open` admits the `StoreProfile` lane, then boots and probes the profile; `Query`, `Append`, `Mount`, `Secret`, `Publish`, and `StampOf` each own a duplicate connection; `ArrowStream` admits one `AdbcRequest` and drains inside the ADBC statement lifetime.
- Auto: every concurrent operation rides a duplicate lane over the held anchor; profile settings and extension bootstrap are composition data; `Query` streams, mapped appenders own bulk ingress, and ADBC owns Arrow extraction.
- Receipt: a session open rides `store.columnar.open` carrying the loaded extension set and the posture; a query rides `store.columnar.query` carrying the `DuckDBQueryProgress` percentage; an append rides `store.columnar.append` carrying the row count; a mount rides `store.columnar.mount` carrying the alias.
- Packages: DuckDB.NET.Data.Full (`DuckDBConnection`/`DuckDBCommand`/`DuckDBConnectionStringBuilder`/`DuckDBMappedAppender<T,TMap>`/`DuckDBAppenderMap<T>`/`DuckDBDataReader`/`DuckDBQueryProgress`/`DuckDBErrorType`), Apache.Arrow, Apache.Arrow.Adbc (`AdbcDatabase`/`AdbcConnection`/`AdbcStatement`/`QueryResult`/`IArrowArrayStream`), Rasm.Persistence (`Store/provisioning#SERVER_EXTENSIONS` `StoreProfile.Admits` — the lane-realizability axis), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new analytical profile is one `ColumnarProfile` row carrying its posture and roster; a new extension is one `ColumnarExtension` row carrying its `ExtensionRepo`; a new install repository is one `ExtensionRepo` row carrying its bootstrap template; a new credential kind is one `SecretScope` row carrying its `PROVIDER` token; a new dynamic-token class is one trust-gate `[ValueObject]` admission, never a raw interpolation site; a new fault cause is one `ColumnarFault` case; zero new surface — a per-extension NuGet package, a second analytical engine, an open-per-query connection, command interleaving on one handle, inline credentials in a path, a raw-string identifier crossing into engine SQL, or a provider-branded service family is the deleted form because DuckDB is fully-featured through the one centrally pinned runtime, the engine is a posture-configured anchor with `Duplicate()` concurrency, and the extension roster is profile policy expressed as SQL.
- Boundary: `Open` is the lane's ONE admission owner — a `StoreProfile` whose engine cannot realize the columnar lane refuses there with the axis named, so every verb below it executes on a proven lane and a per-verb or per-query realizability test is the deleted form. DuckDB extensions load under profile policy, and each source owns one anchor whose duplicate connections isolate commands and streams. `Identifier` admits aliases, tables, columns, and secret names; `StorePath` admits external paths. `SecretResidence` distinguishes session and persistent secrets without a bool payload. Foreign stores attach read-only, `substrait` fails closed when unavailable, and provider exceptions lift once into `ColumnarFault`.

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
using JasperFx.Events.Daemon;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using Rasm.Domain;                                // TenantId — the frame tenancy the series key packs
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
using Rasm.Persistence.Store;                     // StoreProfile — the lane-realizability axis Open admits against
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// Trust gates admit identifiers and paths before engine SQL; DuckDB parameters own values only.
// `Identifier` rejects non-token text, while `StorePath` rejects quotes, separators, and control characters.
[ValueObject<string>]
[ValidationError<ColumnarFault>]
public readonly partial struct Identifier {
    static partial void ValidateFactoryArguments(ref ColumnarFault? validationError, ref string value) {
        if (value is not [_, ..] || char.IsAsciiDigit(value[0]) || !value.All(static c => char.IsAsciiLetterOrDigit(c) || c == '_')) {
            validationError = new ColumnarFault.TrustRefused($"<identifier:{value}>");
        }
    }
}

[ValueObject<string>]
[ValidationError<ColumnarFault>]
public readonly partial struct StorePath {
    static readonly SearchValues<char> Hostile = SearchValues.Create("'\";");
    static partial void ValidateFactoryArguments(ref ColumnarFault? validationError, ref string value) {
        if (value is not [_, ..] || value.AsSpan().ContainsAny(Hostile) || value.Any(char.IsControl)) {
            validationError = new ColumnarFault.TrustRefused($"<store-path:{value}>");
        }
    }
}

[ValueObject<int>]
[ValidationError<ColumnarFault>]
public readonly partial struct ExecutionThreads {
    static partial void ValidateFactoryArguments(ref ColumnarFault? validationError, ref int value) {
        if (value < 1) validationError = new ColumnarFault.PolicyRefused("execution-threads", value.ToString(CultureInfo.InvariantCulture));
    }
}

[ValueObject<string>]
[ValidationError<ColumnarFault>]
public readonly partial struct AdbcSql {
    static partial void ValidateFactoryArguments(ref ColumnarFault? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\0')) validationError = new ColumnarFault.TrustRefused("<adbc-sql>");
    }
}

// `ExtensionRepo` owns bootstrap form: linked load, core install, or community install.
// `Open` probes every admitted extension and converts channel incompatibility into a typed fault.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtensionRepo {
    public static readonly ExtensionRepo Linked = new("linked", static key => $"LOAD {key};");
    public static readonly ExtensionRepo Core = new("core", static key => $"INSTALL {key}; LOAD {key};");
    public static readonly ExtensionRepo Community = new("community", static key => $"INSTALL {key} FROM community; LOAD {key};");
    [UseDelegateFromConstructor] public partial string Bootstrap(string key);
}

// `ColumnarExtension` rows own extension identity and repository policy on one pinned runtime.
// `Substrait` is community-signed and fails closed during `Open`.
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

// `ColumnarProfile` rows carry dedicated-machine posture and an ordered extension roster.
// Lakehouse and federation disable insertion-order preservation; correctness lanes retain it.
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
    // Federation tabular subtrees use community Substrait under fail-closed admission.
    public static readonly ColumnarProfile Federation = new("federation", MemoryShare, "federation.tmp", SpillShare, preserveOrder: false, [ColumnarExtension.Parquet, ColumnarExtension.Substrait, ColumnarExtension.Postgres]);

    public string MemoryCap { get; }
    public string SpillRoot { get; }
    public string SpillCap { get; }
    public bool PreserveOrder { get; }
    public Seq<ColumnarExtension> Roster { get; }
    private ColumnarProfile(string key, string memoryCap, string spillRoot, string spillCap, bool preserveOrder, Seq<ColumnarExtension> roster) : this(key) =>
        (MemoryCap, SpillRoot, SpillCap, PreserveOrder, Roster) = (memoryCap, spillRoot, spillCap, preserveOrder, roster);

    // `ConnectionString` composes host parallelism, memory, spill, and insertion-order policy once.
    // `max_temp_directory_size` converts spill exhaustion into a loud engine failure.
    public string ConnectionString(StorePath dataSource, ExecutionThreads threads) {
        DuckDBConnectionStringBuilder rows = new() { DataSource = (string)dataSource };
        (rows["threads"], rows["memory_limit"], rows["temp_directory"], rows["max_temp_directory_size"], rows["preserve_insertion_order"]) =
            ((int)threads, MemoryCap, SpillRoot, SpillCap, PreserveOrder);
        return rows.ConnectionString;
    }
}

// `SecretScope` rows own each `CREATE SECRET` type, provider, and persistence target.
// `httpfs` owns transport, while this vocabulary owns credential resolution.
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

// --- [ERRORS] -----------------------------------------------------------------------------
// `ColumnarFault` closes `FaultBand.Columnar` over `Rasm.Domain.Expected` without generated union operations.
// Native error classes determine typed cases, and `Category` projects telemetry labels.
[Union]
public abstract partial record ColumnarFault : Expected, IValidationError<ColumnarFault> {
    private ColumnarFault() : base() { }
    public sealed record QueryFailed(string Detail, DuckDBErrorType Kind) : ColumnarFault;
    public sealed record ExtensionGap(string Extension) : ColumnarFault;
    public sealed record AppendRefused(string Table, DuckDBErrorType Kind) : ColumnarFault;
    public sealed record MountRefused(string Alias, DuckDBErrorType Kind) : ColumnarFault;
    public sealed record EgressRefused(string Destination, DuckDBErrorType Kind) : ColumnarFault;
    public sealed record UnstampedArtifact(string Path) : ColumnarFault;
    public sealed record SecretRefused(string Name, DuckDBErrorType Kind) : ColumnarFault;
    public sealed record TrustRefused(string Token) : ColumnarFault;
    public sealed record DeltaRefused(string Table, string Detail) : ColumnarFault;
    public sealed record PolicyRefused(string Policy, string Found) : ColumnarFault;

    public override int Code => FaultBand.Columnar + Switch(
        queryFailed:       static _ => 0,
        extensionGap:      static _ => 1,
        appendRefused:     static _ => 2,
        mountRefused:      static _ => 3,
        egressRefused:     static _ => 4,
        unstampedArtifact: static _ => 5,
        secretRefused:     static _ => 6,
        trustRefused:      static _ => 7,
        deltaRefused:      static _ => 8,
        policyRefused:     static _ => 9);

    public override string Message => Switch(
        queryFailed:       static c => $"<columnar-query:{c.Detail}>",
        extensionGap:      static c => $"<columnar-extension:{c.Extension}>",
        appendRefused:     static c => $"<columnar-append:{c.Table}>",
        mountRefused:      static c => $"<columnar-mount:{c.Alias}>",
        egressRefused:     static c => $"<columnar-egress:{c.Destination}>",
        unstampedArtifact: static c => $"<columnar-unstamped:{c.Path}>",
        secretRefused:     static c => $"<columnar-secret:{c.Name}>",
        trustRefused:      static c => $"<columnar-trust:{c.Token}>",
        deltaRefused:      static c => $"<columnar-delta:{c.Table}:{c.Detail}>",
        policyRefused:     static c => $"<columnar-policy:{c.Policy}:{c.Found}>");

    public override string Category => Switch(
        queryFailed:       static _ => "Query",
        extensionGap:      static _ => "Extension",
        appendRefused:     static _ => "Append",
        mountRefused:      static _ => "Mount",
        egressRefused:     static _ => "Egress",
        unstampedArtifact: static _ => "Unstamped",
        secretRefused:     static _ => "Secret",
        trustRefused:      static _ => "Trust",
        deltaRefused:      static _ => "Delta",
        policyRefused:     static _ => "Policy");

    // Trust-gate admissions exclusively reach generator text; `Create` preserves trust faults without fabricating native kinds.
    public static ColumnarFault Create(string message) => new TrustRefused(message);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// `ColumnarSession` holds one native anchor per source and creates `Duplicate()` lanes for concurrent drains.
// Private construction requires profile bootstrap, and negative progress remains `None`.
public sealed class ColumnarSession : IDisposable {
    readonly DuckDBConnection anchor;
    public ColumnarProfile Profile { get; }
    public Seq<string> Loaded { get; }
    // UDF registration binds the anchor because the anchor's lifetime IS the session's — a registration on a
    // short-lived duplicate lane would gamble the function's catalog lifetime on connection close semantics.
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
    // Residence slots carry NAMES because `Store/observability#STORE_INSTRUMENTS` keys its projection arms on
    // them; a slot no arm folds stays an inline row, so a name here tells a reader this stream reaches meter
    // as well as receipt plane.
    public static readonly StoreSlot ProvisionSlot = StoreSlot.Create("store.columnar.residence.provision");
    public static readonly StoreSlot ReadSlot = StoreSlot.Create("store.columnar.residence.read");
    public static readonly StoreSlot IngestSlot = StoreSlot.Create("store.columnar.residence.ingest");

    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.columnar.open"), StoreSlot.Create("store.columnar.query"), StoreSlot.Create("store.columnar.append"),
        StoreSlot.Create("store.columnar.mount"), StoreSlot.Create("store.columnar.egress"), StoreSlot.Create("store.columnar.stamp"),
        StoreSlot.Create("store.columnar.flattable"), StoreSlot.Create("store.columnar.materialize"), StoreSlot.Create("store.columnar.frames"),
        StoreSlot.Create("store.columnar.parquet"), StoreSlot.Create("store.columnar.scan"),
        // Landing slots derive from the `LandingArm` roster itself, so a new producer arm mounts its slot
        // with no edit here and a literal drifting from its own row cannot exist.
        ProvisionSlot, ReadSlot, IngestSlot) + toSeq(LandingArm.Items).Map(static arm => arm.Slot);

    // `Lane` is the token `StoreProfile.Lanes` spells for this owner, so the literal has one home on the page.
    public const string Lane = "columnar";

    // `Open` is the lane's ONE admission owner: an engine whose profile cannot realize the lane refuses HERE
    // naming the axis, so an embedded deployment learns at profile selection, never at its first aggregation.
    // Past that gate `Open` applies ordered bootstrap policy, then verifies every roster row through
    // `duckdb_extensions()`, and missing linked, core, or community extensions rail `ExtensionGap` before any query.
    public static IO<ColumnarSession> Open(StoreProfile store, ColumnarProfile profile, StorePath dataSource, ExecutionThreads threads) =>
        !store.Admits(Lane)
        ? IO.fail<ColumnarSession>(new ColumnarFault.PolicyRefused("store-lane", store.Key))
        : IO.liftAsync(async () => {
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
            return new ColumnarSession(anchor, profile, loaded);
        }).Bind(static session => AdmitLoaded(session));

    static IO<ColumnarSession> AdmitLoaded(ColumnarSession session) {
        Seq<string> missing = toSeq(session.Profile.Roster.Map(static extension => extension.Key)).Filter(key => !session.Loaded.Contains(key));
        if (missing.IsEmpty) return IO.pure(session);
        session.Dispose();
        return IO.fail<ColumnarSession>(new ColumnarFault.ExtensionGap(string.Join(",", missing)));
    }

    // Engine-parity UDF registration: the embedded floor's identity capabilities answer on BOTH embedded engines,
    // so a rollup joining on xxh128(content) runs unchanged over SQLite or DuckDB. Only the two genuinely portable
    // functions register — ISO-8601 ordinal text ordering is native icu collation here and span_fold is the native
    // max aggregate, so re-registering either would shadow a stronger built-in.
    public static IO<Unit> Register(ColumnarSession session) =>
        IO.lift(() => {
            session.Anchor.RegisterScalarFunction<string>("uuid7", static () => Guid.CreateVersion7().ToString("N"));
            session.Anchor.RegisterScalarFunction<byte[], byte[]>("xxh128", static bytes => {
                byte[] key = new byte[16];
                BinaryPrimitives.WriteUInt128BigEndian(key, XxHash128.HashToUInt128(bytes));
                return key;
            });
            return unit;
        });

    // Streaming queries run on `Duplicate()` lanes and bind interpolation holes as named `$pN` parameters.
    // One seam-local list accumulates rows once before `toSeq`, avoiding persistent-sequence forcing per row.
    public static IO<Seq<T>> Query<T>(ColumnarSession session, FormattableString sql, Func<DuckDBDataReader, T> shape) =>
        IO.liftAsync(async () => {
            DuckDBConnection lane = session.Lane();
            await using (lane.ConfigureAwait(false)) {
                await using DuckDBCommand command = lane.CreateCommand();
                object[] placeholders = Enumerable.Range(0, sql.ArgumentCount).Select(static i => (object)$"$p{i}").ToArray();
                (command.CommandText, command.UseStreamingMode) = (string.Format(CultureInfo.InvariantCulture, sql.Format, placeholders), true);
                for (int i = 0; i < sql.ArgumentCount; i++) command.Parameters.Add(new DuckDBParameter($"p{i}", sql.GetArgument(i)));
                await using DuckDBDataReader reader = (DuckDBDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
                List<T> rows = [];
                while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
                return toSeq(rows);
            }
        }) | @catch<IO, Seq<T>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<Seq<T>>(new ColumnarFault.QueryFailed(e.Message, ((DuckDBException)e.ToException()).ErrorType)));

    // `DuckDBAppenderMap<T>` validates declared columns before `AppendRecords` streams and `Close` flushes the batch.
    public static IO<long> Append<T, TMap>(ColumnarSession session, Identifier table, Seq<T> rows) where TMap : DuckDBAppenderMap<T>, new() =>
        IO.lift(() => {
            using DuckDBConnection lane = session.Lane();
            DuckDBMappedAppender<T, TMap> appender = lane.CreateAppender<T, TMap>((string)table);
            appender.AppendRecords(rows);
            appender.Close();
            return (long)rows.Count;
        }) | @catch<IO, long>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<long>(new ColumnarFault.AppendRefused(table, ((DuckDBException)e.ToException()).ErrorType)));

    // `Mount` admits aliases and paths, attaches foreign stores read-only, and pre-flights metadata.
    // Object-store paths resolve credentials through `Secret` before attachment.
    public static IO<Fin<Unit>> Mount(ColumnarSession session, Identifier alias, StorePath store, ColumnarExtension typed) =>
        IO.liftAsync(async () => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = $"ATTACH IF NOT EXISTS '{store}' AS {alias} (TYPE {typed.Key}, READ_ONLY)";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.MountRefused(alias, ((DuckDBException)e.ToException()).ErrorType))));

    // `Secret` admits names and configuration keys, doubles literal quotes, and forbids credentials in paths.
    // `SecretResidence.Persistent` writes into the attached credential store; `Session` remains connection-scoped.
    public static IO<Fin<Unit>> Secret(ColumnarSession session, SecretScope scope, Identifier name, Seq<(Identifier Key, string Value)> config, SecretResidence residence) =>
        IO.liftAsync(async () => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            string into = residence is SecretResidence.Persistent ? $" IN {scope.PersistInto}" : string.Empty;
            Seq<string> rows = config.Map(static pair => $"{pair.Key} '{pair.Value.Replace("'", "''", StringComparison.Ordinal)}'");
            command.CommandText = $"CREATE OR REPLACE SECRET {name}{into} (TYPE {scope.Key}, PROVIDER {scope.Provider}, {string.Join(", ", rows)})";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.SecretRefused(name, ((DuckDBException)e.ToException()).ErrorType))));

    // ADBC owns SQL and Substrait execution and batch or stream binding on one statement seam.
    // `drain` runs inside statement lifetime so no `QueryResult.Stream` escapes disposal.
    public static IO<T> ArrowStream<T>(AdbcConnection adbc, AdbcRequest request, Func<QueryResult, ValueTask<T>> drain) =>
        IO.liftAsync(async () => {
            using AdbcStatement statement = adbc.CreateStatement();
            request.Apply(statement);
            QueryResult result = await statement.ExecuteQueryAsync().ConfigureAwait(false);
            return await drain(result).ConfigureAwait(false);
        });

    // PARTITIONED execution over the same statement seam: `ExecutePartitioned` hands back the server-side
    // split as opaque descriptors and each redeems on its own `ReadPartition` stream, so a partition-parallel
    // consumer fans out without a second transport and without the raw ADBC surface reaching it.
    // `ExecutePartitioned` and `ReadPartition` are both `virtual` bodies that THROW
    // `AdbcException.NotImplemented` on a driver that declines them — the base class publishes the whole
    // vocabulary and each driver overrides what it serves — so the call lifts once into the typed fault and a
    // consumer reads a refusal naming the driver rather than an exception crossing the rail.
    // `PartitionDescriptor.Descriptor` is a `ReadOnlySpan<byte>` that crosses no lambda and no await, so the
    // descriptor STRUCT travels and its span stays inside the redeeming frame.
    public static IO<Fin<ArrowPartitions>> ArrowPartitions(AdbcConnection adbc, AdbcRequest request) =>
        IO.lift(() => {
            using AdbcStatement statement = adbc.CreateStatement();
            request.Apply(statement);
            PartitionedResult split = statement.ExecutePartitioned();
            return Fin.Succ(new ArrowPartitions(adbc, split.Schema, split.AffectedRows, toSeq(split.PartitionDescriptors)));
        }) | @catch<IO, Fin<ArrowPartitions>>(static error => error.IsExceptional,
            error => IO.pure(Fin<ArrowPartitions>.Fail(new ColumnarFault.PolicyRefused("adbc-partitioned", error.Message))));
}

// Redemption face of one partitioned execution: the schema every partition shares, the affected-row count the
// driver reported, and the descriptor run a consumer redeems in any order and any degree of parallelism. The
// connection rides the value because a descriptor is meaningless without the connection that minted it —
// handing a consumer bare descriptors invites redemption against a second connection the server never split for.
public sealed record ArrowPartitions(AdbcConnection Connection, Schema Schema, long AffectedRows, Seq<PartitionDescriptor> Descriptors) {
    // One partition's Arrow stream; the caller owns disposal exactly as it owns a `QueryResult.Stream`.
    public IO<Fin<IArrowArrayStream>> Redeem(PartitionDescriptor descriptor) =>
        IO.lift(() => Fin.Succ(Connection.ReadPartition(descriptor)))
        | @catch<IO, Fin<IArrowArrayStream>>(static error => error.IsExceptional,
            error => IO.pure(Fin<IArrowArrayStream>.Fail(new ColumnarFault.PolicyRefused("adbc-partition-read", error.Message))));
}

// DRIVER axis binding the admitted ADBC packages: each row names its driver, its parameter vocabulary
// (host/port/path/auth per the Apache Thrift drivers; project/dataset/credential for BigQuery), and opens the
// AdbcDatabase → AdbcConnection pair through that driver — a caller-supplied bare AdbcConnection with no owner
// selecting the driver, admitting its parameters, or converting its failures is the deleted unbound form.
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
    // One open owner: driver row selects, parameters admit (non-empty, no blank keys), the database and
    // connection open under the row, and every driver exception converts ONCE to the typed columnar fault.
    public static IO<Fin<AdbcConnection>> Open(WarehouseDriver driver, HashMap<string, string> parameters) =>
        IO.lift(() => parameters.IsEmpty || parameters.Keys.Exists(string.IsNullOrWhiteSpace)
            ? Fin<AdbcConnection>.Fail(new ColumnarFault.PolicyRefused("adbc-parameters", driver.Key))
            : Fin<AdbcConnection>.Succ(driver.Open(parameters.ToDictionary(static p => p.Key, static p => p.Value)).Connect(new Dictionary<string, string>())))
        | @catch<IO, Fin<AdbcConnection>>(static error => error.IsExceptional,
            error => IO.pure(Fin<AdbcConnection>.Fail(new ColumnarFault.PolicyRefused("adbc-open", error.Message))));
}

// `AdbcRequest` closes the statement seam over composed SQL and portable Substrait bytes.
// Federation owns plan identity; this seam executes without rehashing.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdbcQuery {
    private AdbcQuery() { }
    public sealed record Sql(AdbcSql Composed) : AdbcQuery;
    public sealed record Plan(byte[] Substrait) : AdbcQuery;
}

// ONE application of a request onto a statement, so the streaming and the partitioned entry cannot diverge on
// which half of the request reaches the driver: a bind the partitioned arm dropped executes the parameterized
// plan against no parameters, and the driver answers a partition set for a question nobody asked.
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

// `AdbcBind` closes binding arity over one batch or an `IArrowArrayStream`.
// `BindStream` preserves chunking without materializing a batch.
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
|  [03]   | consistency stance  | async, `StalenessWatermark`               | never read by interactive correctness without the wait (`C2`)      |
|  [04]   | index ownership     | DuckDB spatial/vss are aggregators        | GiST/pgvector own the transactional index (`L2`)                   |
|  [05]   | credential rail     | `CREATE SECRET` over `SecretScope`        | quote-doubled config literals; never an inline path key            |
|  [06]   | Arrow bridge        | ADBC driver manager → `IArrowArrayStream` | no managed Arrow member; params via `AdbcStatement.Bind`           |
|  [07]   | fault rail          | `DuckDBException` → `ColumnarFault`       | discriminated on `DuckDBErrorType`, never a raw ADO throw          |
|  [08]   | trust gate          | `Identifier`/`StorePath`                  | one grammar per identity regime                                    |
|  [09]   | plan execution      | `AdbcQuery.Plan` → `SubstraitPlan`        | the federation intra-leg edge on the one ADBC statement seam       |
|  [10]   | lane admission      | `StoreProfile.Admits(Lane)` inside `Open` | refused once with the axis named; never a per-verb lane test       |

## [03]-[ARTIFACT_EGRESS]

- Owner: `Codec` the `[SmartEnum<string>]` compression vocabulary whose `.Key` IS the `COPY` `COMPRESSION` token; `Collision` the destination-collision vocabulary; `EgressFormat` the format vocabulary carrying its grouped flag and the JSON `ARRAY` row; `Projection` the composed TYPED projection (trust-gated source + column identifiers) the COPY body embeds — never raw caller SQL; `ArtifactClass` the closed analytical-artifact declaration deriving emission, partition, and the footer stamp from one row; `ArtifactEgress` the static surface owning the `COPY (SELECT) TO` rail, the footer-metadata stamp read, and the `read_parquet` generation scan.
- Cases: `EgressFormat` is `Parquet` (grouped), `Csv`, `Json` (carrying `ARRAY true`); `Codec` is `Zstd`/`Snappy`; `Collision` is `Overwrite`/`OverwriteOrIgnore`/`Append`; `ArtifactClass` is `BimRollup` (the QTO/quantity Parquet generation, Zstd, overwrite), `CoverageFeed` (the partitioned geospatial-coverage JSON feed, Snappy, append), and `TelemetryEvidence` (the receipt-stream Parquet generation, Zstd, domain-partitioned, append — the receipt plane is append-only truth, so overwrite is unrepresentable for it by row) — a new artifact class is one row deriving its whole emission.
- Entry: `public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, Projection projection, StorePath destination, UInt128 stamp)` runs the one `COPY (projection) TO destination (…)` statement assembled from the artifact-class policy rows over the trust-gated projection and destination, the stamp the `UInt128` content-address currency hex-formatted at the seam so caller raw text is unrepresentable; `public static IO<Fin<UInt128>> StampOf(ColumnarSession session, StorePath artifact)` reads the content stamp from the Parquet footer through `parquet_kv_metadata` without decoding data and parses it back to the content-address currency (missing or malformed rails `UnstampedArtifact`); `public static FormattableString Generation(StorePath root)` derives the `read_parquet` glob scan over an artifact-generation directory with `union_by_name`/`hive_partitioning`/`filename` provenance.
- Auto: one `COPY (SELECT) TO` statement owns engine-mediated egress (data-interchange `ARTIFACT_PROJECTION`) — `FORMAT`/`COMPRESSION`/`ROW_GROUP_SIZE`/`PARTITION_BY` interpolate beside the shared destination from the artifact-class rows so a mistyped token is unrepresentable rather than a runtime SQL parse error, a second export path per format is the deleted form, and a `KV_METADATA` stamp binds the artifact's content identity into the footer; row-group geometry is the unit of scan parallelism and zonemap pruning so the `ROW_GROUP_SIZE` near the default-row count prunes well and a tiny-group append-per-batch exporter batches through a staging projection and exports once; partitioning is a pruning instrument (`PARTITION_BY` into hive directories at cardinality in the tens to low thousands), never a uniqueness scheme; the footer answers declared shape, per-row-group statistics, and the caller stamp without decoding data (`parquet_kv_metadata`/`parquet_metadata`/`parquet_schema`) so artifact admission is a metadata-cost gate run on every delivery; the generation read is `read_parquet` over a path/glob/list so a generation directory growing changes only the path argument, `union_by_name` makes additive columns compatible by construction (absent reads NULL), and `filename`/`file_row_number` pin per-row provenance; the `COPY` is a filesystem effect outside transaction rollback so publication is the atomic-write protocol, never transactional cleanup.
- Receipt: an egress rides `store.columnar.egress` carrying the artifact class and the destination; a footer stamp read rides `store.columnar.stamp` carrying the content identity.
- Packages: DuckDB.NET.Data.Full (`DuckDBCommand.ExecuteNonQuery`/`ExecuteScalar`/`DuckDBParameter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact class is one `ArtifactClass` row deriving emission, partition, and stamp; a new egress format/codec/collision is one vocabulary row whose `.Key` IS the `COPY` token; zero new surface — a per-format export path, a `FORMAT` value stretched to name a transport the engine never performs, a filename-convention identity trust, or an in-place generation rewrite is the deleted form because the COPY rail is the one SQL-mediated egress, identity rides the footer stamp, and generations are immutable.
- Boundary: the `COPY (SELECT) TO` rail is the SQL-mediated egress lane, not the egress monopoly — a zero-copy in-process columnar handoff (the `ColumnarLane.ArrowStream` ADBC bridge) and a direct managed file codec (`#FLAT_TABLE_EGRESS` `ParquetSharp.Arrow`) are distinct lanes a `COPY` `FORMAT` token cannot express, so a non-SQL egress lands as a sibling lane beside the COPY family, never as a `FORMAT` row (the deleted form is a `FORMAT` value stretched to name a transport the engine never performs); artifact identity is the footer content stamp and the declared `ArtifactClass` shape, never a filename convention — a renamed artifact keeps its identity and a stamp that no longer matches its content is corruption, not drift; generations are immutable (compaction is a new artifact written beside the old with a new stamp, never an in-place merge) and `FIELD_IDS` at export and an id-keyed scan map make renames non-breaking across generations; the `COPY` is a filesystem effect outside transaction rollback so publication composes the atomic-write protocol the `Element/codec#SNAPSHOT_SPINE` owns, never transactional cleanup; the lakehouse `delta`/`iceberg` scans read the same tables the managed `#FLAT_TABLE_EGRESS` `PublishDelta` commit produces — DuckDB the read/aggregate projection, the managed Delta log the versioned publication, meeting at the table path and never re-authoring each other's metadata.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// COPY-token smart enums own emitted format, codec, and compression literals.
// Mistyped tokens (`OVERWRITE_OR_INGORE`) are unrepresentable rather than runtime SQL parse errors; a new format/codec/collision is ONE row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Codec {
    public static readonly Codec Zstd = new("zstd");
    public static readonly Codec Snappy = new("snappy");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Collision {
    public static readonly Collision Overwrite = new("OVERWRITE");
    public static readonly Collision OverwriteOrIgnore = new("OVERWRITE_OR_IGNORE");
    public static readonly Collision Append = new("APPEND");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EgressFormat {
    public static readonly EgressFormat Parquet = new("parquet", None, grouped: true);
    public static readonly EgressFormat Csv = new("csv", None, grouped: false);
    public static readonly EgressFormat Json = new("json", Some("ARRAY true"), grouped: false);

    public Option<string> ArrayRow { get; }
    public bool Grouped { get; }
    private EgressFormat(string key, Option<string> arrayRow, bool grouped) : this(key) => (ArrayRow, Grouped) = (arrayRow, grouped);
}

// --- [MODELS] -----------------------------------------------------------------------------
// `CopyProjection` composes an admitted source and non-empty admitted columns.
// Filtered or joined egress stages through a view created by the parameterized query rail.
public sealed record Projection(Identifier Source, Seq<Identifier> Columns) {
    public string Sql => Columns.IsEmpty
        ? $"SELECT * FROM {Source}"
        : $"SELECT {string.Join(", ", Columns)} FROM {Source}";
}

// Artifact-class rows derive complete `COPY` policy and immutable generation paths, keyed exactly as every
// sibling egress vocabulary is so `Items`, `Get`, and `Validate` serve the census the analytics registry reads
// — a hand-rolled roster beside three generated ones is the shape this owner deletes.
// `KV_METADATA` carries the `ContentAddress` stamp in the footer rather than the filename.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactClass {
    public static readonly ArtifactClass BimRollup = new("bim-rollup", EgressFormat.Parquet, Codec.Zstd, 122_880, None, Collision.Overwrite);
    public static readonly ArtifactClass CoverageFeed = new("coverage-feed", EgressFormat.Json, Codec.Snappy, 122_880, Some(Identifier.Create("crs")), Collision.Append);
    // Evidence extract of the kernel receipt stream: one immutable Parquet generation per emission window,
    // partitioned by the capability domain a query joins on, appended so a window never overwrites a sibling.
    // Receipt-plane truth is append-only, so `Overwrite` is unrepresentable for this class by row.
    public static readonly ArtifactClass TelemetryEvidence = new("telemetry-evidence", EgressFormat.Parquet, Codec.Zstd, 122_880, Some(Identifier.Create("domain")), Collision.Append);

    public EgressFormat Format { get; }
    public Codec Codec { get; }
    public int RowGroup { get; }
    public Option<Identifier> PartitionKey { get; }
    public Collision Collision { get; }

    private ArtifactClass(string key, EgressFormat format, Codec codec, int rowGroup, Option<Identifier> partitionKey, Collision collision) : this(key) =>
        (Format, Codec, RowGroup, PartitionKey, Collision) = (format, codec, rowGroup, partitionKey, collision);

    public string Egress(Projection projection, StorePath destination, UInt128 stamp) =>
        $"COPY ({projection.Sql}) TO '{destination}' ({string.Join(", ",
            Seq(Some($"FORMAT {Format.Key}"), Some($"COMPRESSION {Codec.Key}"),
                Format.Grouped ? Some($"ROW_GROUP_SIZE {RowGroup}") : Option<string>.None, Format.ArrayRow,
                PartitionKey.Map(static key => $"PARTITION_BY ({key})"), Some(Collision.Key),
                Some($"KV_METADATA {{ stamp: '{stamp.ToString("x32", CultureInfo.InvariantCulture)}' }}")).Somes())})";
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ArtifactEgress {
    // One engine-mediated `COPY` statement projects each artifact policy row through admitted tokens.
    public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, Projection projection, StorePath destination, UInt128 stamp) =>
        IO.liftAsync(async () => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = artifact.Egress(projection, destination, stamp);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.EgressRefused(destination, ((DuckDBException)e.ToException()).ErrorType))));

    // Artifact admission reads the `UInt128` content stamp from footer metadata without decoding rows.
    // Missing or malformed stamps rail `UnstampedArtifact`.
    public static IO<Fin<UInt128>> StampOf(ColumnarSession session, StorePath artifact) =>
        IO.liftAsync(async () => {
            await using DuckDBConnection lane = session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            command.CommandText = "SELECT decode(value) FROM parquet_kv_metadata($path) WHERE decode(key) = 'stamp'";
            command.Parameters.Add(new DuckDBParameter("path", (string)artifact));
            return Optional(await command.ExecuteScalarAsync().ConfigureAwait(false)).Map(static held => (string)held);
        }).Map(stamp => stamp
            .Bind(static held => ParseStamp(held))
            .Match(Some: Fin<UInt128>.Succ, None: () => Fin<UInt128>.Fail(new ColumnarFault.UnstampedArtifact(artifact))));

    static Option<UInt128> ParseStamp(string held) {
        bool parsed = UInt128.TryParse(held, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 key);
        return parsed ? Some(key) : None;
    }

    // Generation reads use `read_parquet` with schema union, hive keys, and row provenance.
    // One unquoted parameter hole carries the whole glob so DuckDB binds the path.
    public static FormattableString Generation(StorePath root) =>
        $"SELECT *, filename, file_row_number FROM read_parquet({$"{(string)root}/**/*.parquet"}, union_by_name = true, hive_partitioning = true, filename = true, file_row_number = true)";
}
```

| [INDEX] | [POLICY]          | [VALUE]                                        | [BINDING]                                                         |
| :-----: | :---------------- | :--------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | engine egress     | one `COPY (SELECT) TO` rail                    | `.Key` IS the COPY token; a second export path is deleted         |
|  [02]   | artifact identity | footer `KV_METADATA` stamp                     | rides the file, never a filename convention                       |
|  [03]   | partitioning      | `PARTITION_BY` hive directories                | a pruning instrument, never a uniqueness scheme                   |
|  [04]   | generation read   | `read_parquet` glob + `union_by_name`          | generations immutable; additive columns compatible                |
|  [05]   | non-SQL egress    | sibling ADBC / `ParquetSharp.Arrow` lane       | never a `FORMAT` token stretched to name a transport              |
|  [06]   | projection        | composed `Projection`, trust-gated identifiers | never raw caller SQL; filtered egress stages via the `Query` rail |

## [04]-[FLAT_TABLE_EGRESS]

- Owner: `BimOpenSchemaProjection` the co-transactional Marten `FlatTableProjection` writing the columnar BIM fact table in the append transaction; `FlatTableEgress` the static surface owning the async daemon materialization, the IN-CORPUS eleven-table `.duckdb` write over the one pinned runtime (the DECISION `[10]`.3 absorption — the DEBUG-IL `Ara3D` writer never sits on the hot path), and the native `ParquetSharp.Arrow` codec lane (read AND PME-encrypted write) the columnar query rides; `ScanTuning` the ONE read policy both Parquet legs derive their reader and Arrow-decode properties from; `LandingArm` the producer-family row set and `LakeGeneration` the one owner of a cold-tail directory spelling.
- Entry: `public sealed class BimOpenSchemaProjection : FlatTableProjection` registers inline columnar facts; `Materialize` runs the async daemon view; `WriteFrames` streams the `ToDataSet()` tables through the raw appender; `ReadParquetFrames`, `ScanDataset`, and `ReadIpcFrames` return owned `IAsyncEnumerable<RecordBatch>` drains, both Parquet legs taking the same `Option<PmeCustody>` the write does and the same `ScanTuning` policy so an encrypted generation reads back through one member and a remote residence is tuned on both legs or neither; `WriteParquetFrames` stages then atomically publishes one Parquet generation; `PublishDelta` registers published files metadata-only; `Land` takes one `LakeGeneration` carrying tenancy, the arm's partition value, and the distinct schema and generation content keys, and a custody failure unpublishes the generation and rails `UnstampedArtifact` so an unregistered generation is never scan-visible.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` (`Project<T>(StatementMap)`) written in the same transaction as the events, NOT daemon-lagged (`M4`), because a flat analytical view a live QTO reads must be read-your-writes consistent — the structural map maps the `GraphEvent.GraphCreated`'s `Header.Schema.Key`/`Header.View.Key` (the `ReleaseVersion`/`ModelView` smart-enum keys, since `StatementMap.Map` writes a primitive column, never a smart-enum object) and the `GraphRevised`'s `GraphDelta.NodeCount`/`EdgeCount` change magnitude through the single-column primary key `FlatTableProjection` requires; the eleven suffixed BIM tables (`Points_0`/`Strings_1`/`Descriptors_2`/`Documents_3`/`Entities_4`/`Relations_5`/`DoubleParameters_6`/`IntegerParameters_7`/`StringParameters_8`/`EntityParameters_9`/`PointParameters_10`) are written IN-CORPUS: `frames.ToDataSet()` projects the fixed-ordinal `IDataSet` (`Tables` in the order that IS the DuckDB ordinal suffix), and `WriteFrames` folds each `IDataTable` (`Name`/`Columns`/`Rows`, `IDataDescriptor.Name`/`Type` typing the DDL, the `[column, row]` indexer supplying cells) through a `CREATE OR REPLACE TABLE` + raw `DuckDBAppender` `CreateRow`/`AppendValue`/`EndRow` stream on THIS lane's session — the DEBUG-IL `DuckDbUtils.WriteToDuckDB` writer is data-model-only, never the hot write loop — and a Persistence analytical query opens that `.duckdb` over the same pinned runtime and SQL-joins the suffixed entity/parameter/relation tables by their exact suffixed names; the async daemon `Materialize` blocks on `WaitForNonStaleData` so the generation is current before the heavy aggregation lanes read it carrying the `StalenessWatermark`; the native `ParquetSharp.Arrow.FileReader.GetRecordBatchReader` reads the same standard-format `.parquet` files the managed `Parquet.Net` writer produced into `IArrowArrayStream` `RecordBatch`es for the columnar query rail (managed writer / native libparquet-cpp reader interoperate at the file format, never the assembly).
- Receipt: a flat-table projection rides `store.columnar.flattable` carrying the change magnitude; a daemon materialization rides `store.columnar.materialize` carrying the watermark; a frame write rides `store.columnar.frames` carrying the table count; a Parquet read rides `store.columnar.parquet` carrying the record-batch count.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`SchemaNameSource`/`ProjectionLifecycle`/`IDocumentStore`/`BuildProjectionDaemonAsync`/`WaitForNonStaleData`), Ara3D.BimOpenSchema (`BimData`/`BimDataBuilder`/`ToDataSet` — DATA MODEL only post-absorption), Ara3D.SDK (`IDataSet.Tables`/`IDataTable.Name`/`Rows`/`Columns`/`this[column,row]`/`IDataColumn.ColumnIndex`/`Descriptor`/`IDataDescriptor.Name`/`Type` — decompile-verified), DuckDB.NET.Data.Full (`DuckDBAppender.CreateRow`/`IDuckDBAppenderRow.AppendValue`/`AppendNullValue`/`EndRow`/`Close` — the in-corpus write loop), ParquetSharp (`Arrow.FileReader`/`Arrow.FileWriter`/`WriterPropertiesBuilder`; the read-tuning pair `ReaderProperties.GetDefaultReaderProperties`/`SetFooterReadSize`/`SetThriftStringSizeLimit`/`SetThriftContainerSizeLimit`/`FileDecryptionProperties` beside `Arrow.ArrowReaderProperties.GetDefault`/`BatchSize`/`UseThreads`/`PreBuffer`/`CacheOptions` and the `CacheOptions(hole_size_limit, range_size_limit, lazy, prefetch_limit)` struct — both property types mutate in place and dispose; and the in-package `ParquetSharp.Encryption` namespace `CryptoFactory`/`KmsConnectionConfig`/`EncryptionConfiguration`/`DecryptionConfiguration` — PME over the admitted KMS trio ships inside this one distribution, so a manifest row named for it is a phantom package), DeltaLake.Net (`DeltaEngine`/`EngineOptions`/`TableOptions`/`AddAction`/`CommitOptions`/`CreateWriteTransactionAsync`/`GetLatestTransactionVersionAsync`/`DeltaLakeException` — the metadata-only Delta commit rail; assembly `DeltaLake`), ParquetSharp.Dataset (`DatasetReader(string, IPartitioningFactory?, Schema?, ReaderProperties?, ArrowReaderProperties?, DatasetOptions?)`/`ToBatches(IFilter?, IReadOnlyCollection<string>?, IReadOnlyCollection<string>?)`/`HivePartitioning.Factory`/`Col`/`FilterExtensions` — the partitioned lake scan), Apache.Arrow (`RecordBatch`/`IArrowArrayStream`/`ArrowStreamReader(Stream, ICompressionCodecFactory)`), Apache.Arrow.Compression (`CompressionCodecFactory` — the `Lz4Frame`/`Zstd` ingest decode factory), Rasm.Element (`GraphDelta`/`Header`), Rasm.Persistence (`Element/graph#STREAM_GRAIN` `GraphEvent`/`GraphCreated`/`GraphRevised` the Marten event body), LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `map.Map` statement on the `StatementMap`; a new analytical generation is one async daemon view; a new frame codec is the existing `ParquetSharp.Arrow` lane reading a new format; an encryption stance is one `PmeCustody` value the write and the read both take, never a sibling encrypted writer or a read that cannot open what the write sealed; a new lakehouse publication is one `PublishDelta` commit over `AddAction` rows the codec write already computed, never a second write of the bytes; a new producer landing is one `LandingArm` row carrying its slot, hive key, and write order — schema handoff only, zero new storage code, the `Receipt` row landing the evidence plane's cold tail under its capability domain and the `MaterialsTexture` row landing per-channel texture-plane generations beside the catalogue arm because the arm is the DATASET SHAPE and one producer package may hold several; a new scan predicate is one `Col`-rooted `IFilter` composition at the call, never a reader fork; a read-side retune for a slower store is one `ScanTuning` value both Parquet legs take, never a knob on one leg; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, a second Parquet runtime beside `ParquetSharp`, or a hollow writer that opens a row group and writes no column is the deleted form because the BimOpenSchema egress is co-transactional, the managed `Parquet.Net` writer and the native `ParquetSharp` reader meet at the file format, and the Arrow record-batch model is `api-arrow`'s.
- Boundary: the `ElementGraph → Ara3D.BimOpenSchema` egress is a co-transactional `FlatTableProjection` (`M4`) so a live-QTO analytical read is read-your-writes consistent rather than daemon-lagged — `FlatTableProjection` requires a single-column primary key and writes a primitive per `StatementMap.Map`, so a `ReleaseVersion`/`ModelView` smart-enum maps as its `.Key` and a `GraphDelta` maps as its `NodeCount`/`EdgeCount`, never as the smart-enum or delta object itself; if BimOpenSchema is EAV-generic Persistence owns the structural map, if BIM-typed it is a Bim-implemented seam projection (the wire seam, never a sibling reference); a Bim-lowered `StorePlan` (the `Rasm.Bim` `Model/query#PREDICATE_PUSHDOWN` predicate push-down — one parameterized statement over the suffixed fact tables and an in-process residue) executes on this lane's `ColumnarSession` as DATA crossing the same seam, so the estate-scale element query runs where the data rests with no Persistence-side predicate vocabulary; the eleven suffixed BIM tables are read with the built-in `parquet`/`json` surface and `spatial`/`vss`/`fts` extend them for geometry/ANN/text analytics over the same `.duckdb`, all on the one pinned runtime, and a direct SQL consumer references the `<Name>_<n>` projection-ordinal suffix that IS the real table identity (`api-ara3d-bimopenschema#IMPLEMENTATION_LAW`), never a bare table name; the Parquet file codec is `ParquetSharp.Arrow` (the native libparquet-cpp read/write the managed Arrow stack lacks, exposing the `Apache.Arrow` `RecordBatch` directly so Parquet↔Arrow is a first-class managed call), distinct from the DuckDB SQL `read_parquet`/`COPY` path, the three meeting at the Parquet file format and the `Apache.Arrow` model owned by `api-arrow` not re-declared here; the `Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built at the HELD `1.0.1` pin (JIT optimizations disabled in the shipped IL; the feed-newest `.IO` `1.6.1` regressed to `net8.0-windows7.0`, `NU1202` on net10.0 osx-arm64, so the bump is restore-inadmissible) — the ruled escalation is EXECUTED here: the consumed write surface is absorbed in-corpus (`WriteFrames` streams the eleven tables through this lane's appender; `ReadParquetFrames`/`WriteParquetFrames` ride the native `ParquetSharp` codec), so the DEBUG-IL assemblies serve only the in-memory schema model and `ToDataSet()` projection, never a hot IO loop, and the pin bump is never the fix.

```csharp signature
using Apache.Arrow;
using Ara3D.BimOpenSchema;
using Ara3D.DataTable;
using DeltaLake.Errors;
using DeltaLake.Table;
using DuckDB.NET.Data;
using JasperFx.Events.Daemon;
using LanguageExt;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Events.Projections.Flattened;
using NodaTime;
using ParquetSharp;
using ParquetSharp.Arrow;
using ParquetSharp.Encryption;
using Rasm.Domain;                                // TenantContext — the tenancy prefix a lake generation rests under
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using System.Globalization;                       // CultureInfo — the invariant generation-directory spelling
using System.Runtime.CompilerServices;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] -----------------------------------------------------------------------------
// `BimOpenSchemaProjection` maps primitive header keys and delta magnitudes under a single primary key.
// Inline lifecycle preserves read-your-writes correctness for live QTO.
public sealed class BimOpenSchemaProjection : FlatTableProjection {
    public BimOpenSchemaProjection() : base("bim_model_facts", SchemaNameSource.DocumentSchema) {
        Project<GraphEvent.GraphCreated>(map => {
            map.Map(static e => e.Header.Schema.Key).NotNull();
            map.Map(static e => e.Header.View.Key);
            map.Map(static e => e.Delta.NodeCount);
            map.Map(static e => e.Delta.EdgeCount);
        });
        Project<GraphEvent.GraphRevised>(map => {
            map.Map(static e => e.Delta.NodeCount);
            map.Map(static e => e.Delta.EdgeCount);
        });
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FlatTableEgress {
    // Daemon materialization waits for non-stale state before heavy analytical scans and returns the MEASURED
    // wait for the store.query.wait seal. Inline projection remains the same-commit correctness owner.
    public static IO<Duration> Materialize(IDocumentStore store) =>
        IO.liftAsync(async () => {
            await using IProjectionDaemon daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            return await ReadRouter.AwaitNonStale(daemon, QueryLane.Columnar).RunAsync().ConfigureAwait(false);
        });

    // `WriteFrames` projects fixed-ordinal tables and streams cells through the raw DuckDB appender.
    // `<Name>_<n>` remains the admitted table identity consumed by direct SQL.
    public static IO<long> WriteFrames(ColumnarSession session, BimData frames) =>
        IO.lift(() => {
            IDataSet set = frames.ToDataSet();
            long written = 0;
            using DuckDBConnection lane = session.Lane();
            foreach ((IDataTable table, int ordinal) in set.Tables.Select(static (held, index) => (held, index))) {
                Identifier name = Identifier.Create($"{table.Name}_{ordinal}");
                using (DuckDBCommand shape = lane.CreateCommand()) {
                    shape.CommandText = $"CREATE OR REPLACE TABLE {name} ({string.Join(", ",
                        table.Columns.Select(static column => $"{Identifier.Create(column.Descriptor.Name)} {DuckType(column.Descriptor.Type)}"))})";
                    shape.ExecuteNonQuery();
                }
                using DuckDBAppender appender = lane.CreateAppender((string)name);
                for (int row = 0; row < table.Rows.Count; row++) {
                    IDuckDBAppenderRow target = appender.CreateRow();
                    foreach (IDataColumn column in table.Columns) { Cell(target, table[column.ColumnIndex, row]); }
                    target.EndRow();
                }
                appender.Close();
                written += table.Rows.Count;
            }
            return written;
        }) | @catch<IO, long>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<long>(new ColumnarFault.AppendRefused("<bim-frames>", ((DuckDBException)e.ToException()).ErrorType)));

    // Typed cell dispatch maps admitted EAV values to `AppendValue<T>` and absence to `AppendNullValue`.
    static IDuckDBAppenderRow Cell(IDuckDBAppenderRow row, object? value) => value switch {
        null => row.AppendNullValue(),
        double d => row.AppendValue(d),
        float f => row.AppendValue(f),
        long l => row.AppendValue(l),
        int i => row.AppendValue(i),
        string s => row.AppendValue(s),
        object other => row.AppendValue(other.ToString()),
    };

    static string DuckType(Type type) =>
        type == typeof(double) ? "DOUBLE"
        : type == typeof(float) ? "REAL"
        : type == typeof(long) ? "BIGINT"
        : type == typeof(int) ? "INTEGER"
        : "VARCHAR";

    // ONE PME custody value both directions consume: the write derives file encryption properties from it and the
    // read derives file decryption properties from the SAME factory, so a generation written encrypted is
    // readable by construction rather than by a second owner nobody wired. The factory OUTLIVES every reader
    // it arms — ParquetSharp holds native references inside the decryption properties, so disposing it while a
    // generation is still streaming faults in native memory where no managed catch can see it, which is why
    // custody is a composition-held value and never a per-call construction.
    public sealed record PmeCustody(CryptoFactory Crypto, KmsConnectionConfig Kms, EncryptionConfiguration Encrypt, DecryptionConfiguration Decrypt);

    // ONE read policy both Parquet read legs take: the single-file `Arrow.FileReader` read and its multi-file
    // `DatasetReader` counterpart rest on the SAME object-store residence, so a lane tuned on one and left at
    // native defaults on the other coalesces ranges under two regimes over identical bytes. The two thrift
    // ceilings bound footer deserialization against a length the FILE declares, so an untrusted lake generation
    // refuses at a stated ceiling rather than allocating against its own header; `PreBuffer` with a sized
    // `CacheOptions` IS the native range coalescing a hand-rolled request layer would re-implement, and
    // `BatchSize` sets the record-batch grain every downstream fold already reads. Both property types MUTATE
    // IN PLACE and every reader ctor captures by reference, so each derivation mints its own pair — one shared
    // instance propagates a later tune into a scan already streaming.
    public readonly record struct ScanTuning(
        long FooterReadSize, int ThriftStringLimit, int ThriftContainerLimit,
        long BatchSize, bool PreBuffer, bool Threaded, CacheOptions Cache) {
        public static readonly ScanTuning Residence = new(
            FooterReadSize: 256L * 1024, ThriftStringLimit: 100 * 1024 * 1024, ThriftContainerLimit: 100 * 1024 * 1024,
            BatchSize: 65_536L, PreBuffer: true, Threaded: true,
            Cache: new CacheOptions(hole_size_limit: 8L * 1024 * 1024, range_size_limit: 32L * 1024 * 1024, lazy: true, prefetch_limit: 8L));

        // PME decryption mounts on the SAME properties object the tuning does, so an encrypted generation and a
        // plain one read through one derivation. `path` is `Some` on the single-file leg and `None` on a
        // multi-file scan, and that is a capability boundary rather than a convenience: external key material
        // keys off the path each generation carries, which one properties value cannot supply across a tree, so
        // a scan binds INTERNAL key material alone and an external-material generation reads single-file.
        public ReaderProperties Reader(Option<PmeCustody> custody, Option<StorePath> path) {
            ReaderProperties properties = ReaderProperties.GetDefaultReaderProperties();
            properties.SetFooterReadSize(FooterReadSize);
            properties.SetThriftStringSizeLimit(ThriftStringLimit);
            properties.SetThriftContainerSizeLimit(ThriftContainerLimit);
            custody.Iter(pme => properties.FileDecryptionProperties = pme.Crypto.GetFileDecryptionProperties(
                pme.Kms, pme.Decrypt, path.Match<string?>(Some: static held => (string)held, None: static () => null)));
            return properties;
        }

        // `CacheOptions` is a mutable struct behind a property, so it assigns WHOLE — a field write through the
        // getter mutates a copy the reader never sees.
        public ArrowReaderProperties Arrow() {
            ArrowReaderProperties properties = ArrowReaderProperties.GetDefault();
            (properties.BatchSize, properties.UseThreads, properties.PreBuffer, properties.CacheOptions) =
                (BatchSize, Threaded, PreBuffer, Cache);
            return properties;
        }
    }

    // Reader and stream ownership spans enumeration; early cancellation disposes both. Decryption is the
    // symmetric `Option` of the write's own custody value, so an encrypted generation and a plain one read
    // through one member and a direction-split sibling reader is the deleted form.
    public static async IAsyncEnumerable<RecordBatch> ReadParquetFrames(StorePath parquetPath, Option<PmeCustody> custody,
        ScanTuning tuning, [EnumeratorCancellation] CancellationToken token = default) {
        using ReaderProperties properties = tuning.Reader(custody, Some(parquetPath));
        using FileReader reader = new(File.OpenRead((string)parquetPath), properties);
        using IArrowArrayStream stream = reader.GetRecordBatchReader();
        await foreach (RecordBatch batch in Drain(stream, token).ConfigureAwait(false)) yield return batch;
    }

    // ParquetSharp.Arrow `FileWriter` owns plain and PME-encrypted record-batch writes.
    // Read, write, and encryption metadata share one admitted `StorePath`.
    // The pushdown the scan side already CONTRACTS for: `ScanDataset` states its own grain as "partition,
    // row-group-statistics, and row", and only the page index plus declared sorting columns make the row-grain skip
    // real — without them the finest reachable grain is column-chunk min/max over unsorted row groups, so a
    // content-key predicate touches every row group in every generation it scans. The size-statistics level and the
    // page index ARM TOGETHER by the codec's own coupling — a `PageAndColumnChunk` level with the index disabled
    // writes no page-level statistics at all and degrades silently to column-chunk grain — so the two ride one fold
    // and neither appears without the other. `Sorted` is the arm's own column, not a call-site literal: the arm IS
    // the dataset shape, so a generation whose arm declares no order passes an empty set and writes the index alone.
    public static IO<long> WriteParquetFrames(Seq<RecordBatch> batches, StorePath path, Schema schema, Seq<Identifier> sorted, Option<PmeCustody> custody) =>
        IO.lift(() => {
            string published = (string)path;
            string directory = Path.GetDirectoryName(published) ?? throw new InvalidOperationException("<parquet-generation-directory>");
            Directory.CreateDirectory(directory);
            string staging = Path.Combine(directory, $".{Path.GetFileName(published)}.{Guid.CreateVersion7():N}.tmp");
            WriterProperties.SortingColumn[] order = [.. sorted.Map(column =>
                new WriterProperties.SortingColumn { ColumnIndex = schema.GetFieldIndex((string)column), IsDescending = false, NullsFirst = false })];
            using WriterProperties properties = custody.Match(
                Some: pme => Tuned(new WriterPropertiesBuilder().Encryption(pme.Crypto.GetFileEncryptionProperties(pme.Kms, pme.Encrypt, published)), order).Build(),
                None: () => Tuned(new WriterPropertiesBuilder(), order).Build());
            try {
                using (FileWriter writer = new(File.Open(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None), schema, properties, null, leaveOpen: false))
                    foreach (RecordBatch batch in batches) writer.WriteRecordBatch(batch);
                File.Move(staging, published, overwrite: false);
                return (long)batches.Count;
            } finally {
                if (File.Exists(staging)) File.Delete(staging);
            }
        });

    // ONE tuning fold both custody arms take, so an encrypted generation and a plain one carry identical read
    // geometry — the arm difference is the encryption row alone. `EnableStatistics` is the column-chunk floor the
    // page index refines; `DefaultWriterProperties` is process-global ambient policy no per-file builder can scope,
    // so every knob this generation needs is stated on its own builder rather than inherited from a static field
    // another composition may have set.
    static WriterPropertiesBuilder Tuned(WriterPropertiesBuilder builder, WriterProperties.SortingColumn[] order) =>
        builder.EnableStatistics()
            .EnableWritePageIndex()
            .SetSizeStatisticsLevel(SizeStatisticsLevel.PageAndColumnChunk)
            .SortingColumns(order);

    // `PublishDelta` registers existing Parquet files through a metadata-only Delta transaction.
    // App and transaction versions enforce exactly-once publication after the latest-version pre-check.
    public static IO<Fin<long>> PublishDelta(TableOptions table, Seq<AddAction> files, Identifier appId, long asOfVersion) =>
        IO.liftAsync(async () => {
            using DeltaEngine engine = new(EngineOptions.Default);
            using DeltaTable delta = await engine.LoadTableAsync(table, CancellationToken.None).ConfigureAwait(false);
            long? held = await delta.GetLatestTransactionVersionAsync((string)appId, CancellationToken.None).ConfigureAwait(false);
            if (held is { } committed && committed >= asOfVersion) { return Fin.Succ(committed); }
            await delta.CreateWriteTransactionAsync([.. files], new CommitOptions { AppId = (string)appId, TransactionVersion = asOfVersion }, CancellationToken.None).ConfigureAwait(false);
            return Fin.Succ(asOfVersion);
        }) | @catch<IO, Fin<long>>(static e => e.Exception.Map(static x => x is DeltaLakeException).IfNone(false),
            e => IO.pure(Fin<long>.Fail(new ColumnarFault.DeltaRefused("<flat-table-generation>", e.Message))));

    // Partitioned lake scan — the multi-file counterpart to the single-file `Arrow.FileReader` read: the hive
    // scheme infers from the `key=value` directory tree, `Col`-rooted predicates and column projection push down
    // to partition, row-group-statistics, and row grain, and the survivors stream back as one Arrow lane —
    // lake-resident history queryable with no DuckDB mount in the loop. It takes the SAME `ScanTuning` its
    // single-file sibling does, because a scan spanning many generations on a high-latency store is exactly
    // where footer bounds, range coalescing, and batch grain pay, and the untuned form ran native defaults
    // over the one leg whose residence is remote.
    public static async IAsyncEnumerable<RecordBatch> ScanDataset(StorePath root, Option<ParquetSharp.Dataset.Filter.IFilter> filter,
        Seq<Identifier> columns, Option<PmeCustody> custody, ScanTuning tuning,
        [EnumeratorCancellation] CancellationToken token = default) {
        using ReaderProperties properties = tuning.Reader(custody, Option<StorePath>.None);
        using ArrowReaderProperties arrow = tuning.Arrow();
        ParquetSharp.Dataset.DatasetReader dataset = new((string)root,
            new ParquetSharp.Dataset.Partitioning.HivePartitioning.Factory(),
            schema: null, readerProperties: properties, arrowReaderProperties: arrow);
        using IArrowArrayStream stream = dataset.ToBatches(
            filter.Match<ParquetSharp.Dataset.Filter.IFilter?>(Some: static held => held, None: static () => null),
            columns.IsEmpty ? null : [.. columns.Map(static column => (string)column)]);
        await foreach (RecordBatch batch in Drain(stream, token).ConfigureAwait(false)) yield return batch;
    }

    // compressed-carrier decode arm: sibling-minted Arrow IPC wires may arrive with transport-band
    // `Lz4Frame`/`Zstd` block compression, so every ingest reader passes the ONE codec factory — and every
    // `ContentAddress` derivation reads the DECOMPRESSED canonical bytes, so transport framing never enters
    // identity (`Element/codec` pairs Arrow-compressed bodies with `CompressionPolicy.None`).
    static readonly Apache.Arrow.Compression.CompressionCodecFactory IpcCodecs = new();

    public static async IAsyncEnumerable<RecordBatch> ReadIpcFrames(Stream carrier,
        [EnumeratorCancellation] CancellationToken token = default) {
        using ArrowStreamReader reader = new(carrier, IpcCodecs);
        await foreach (RecordBatch batch in Drain(reader, token).ConfigureAwait(false)) yield return batch;
    }

    static async IAsyncEnumerable<RecordBatch> Drain(IArrowArrayStream stream,
        [EnumeratorCancellation] CancellationToken token = default) {
        while (await stream.ReadNextRecordBatchAsync(token).ConfigureAwait(false) is { } batch) yield return batch;
    }

    // ONE landing discipline for every producer arm: the write rides the standing Parquet codec into the arm's
    // hive generation keyed by the producer's schema-identity content key, and custody registers the
    // content-keyed residence on the `Query/cache#ARTIFACT_BLOB_INDEX` — so a producer hands a typed batch
    // schema and never touches storage code, and the landed generation serves back through `ScanDataset` and
    // `Query/federation#FLIGHT_RESULT_PLANE` arms. Custody is the visibility gate: a custody failure unpublishes
    // its generation before the typed `UnstampedArtifact` fault returns, so `ScanDataset` never serves an
    // unregistered generation and a retry of the same generationKey re-lands clean through the CreateNew stage.
    public static IO<Fin<long>> Land(LakeGeneration generation, Seq<RecordBatch> batches, Schema schema, StorePath root,
        Option<PmeCustody> pme, Func<UInt128, StorePath, IO<Unit>> custody) {
        StorePath published = generation.Path(root);
        return WriteParquetFrames(batches, published, schema, generation.Arm.Sorted, pme)
            .Bind(written => (custody(generation.GenerationKey, published).Map(_ => Fin.Succ(written))
                | @catch<IO, Fin<long>>(static _ => true,
                    error => Unpublish(published).Map(_ => Fin.Fail<long>(
                        new ColumnarFault.UnstampedArtifact($"{(string)published}:{error.Message}"))))).As());
    }

    // Custody-failure compensation: delete the published body and prune the emptied generation directory so the
    // hive tree never carries an index-less generation and the same generationKey re-publishes without collision.
    static IO<Unit> Unpublish(StorePath published) =>
        IO.lift(() => {
            string path = (string)published;
            if (File.Exists(path)) File.Delete(path);
            string? generation = Path.GetDirectoryName(path);
            if (generation is not null && Directory.Exists(generation) && !Directory.EnumerateFileSystemEntries(generation).Any()) Directory.Delete(generation);
            return unit;
        });
}

// landing spine rows: each producer family hands a typed record-batch schema and Persistence owns writers,
// residence, slots, index custody, and batch-metadata preservation — a producer owns only its batch shape, and
// a NEW producer is one row, zero new storage code. Geometry wires key by the kernel `ContentHash`
// schema-identity law; the Compute DOE/receipt, Element `Tabulate`, and Materials catalogue/texture arms key by
// their suite content addresses. `Partition` names the hive KEY the arm's generation directories carry, and the
// landed value fills it — the key is the arm's, the value the generation's. One producer PACKAGE may hold more
// than one arm: the arm is the DATASET SHAPE, so two datasets whose generations prune on different segments are
// two arms.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LandingArm {
    public static readonly LandingArm Geometry  = new("geometry", "store.geometry.land", "model", ["node"]);
    public static readonly LandingArm Doe       = new("doe", "store.doe.land", "study", ["run"]);
    public static readonly LandingArm Tabulate  = new("tabulate", "store.tabulate.land", "model", ["entity"]);
    public static readonly LandingArm Materials = new("materials", "store.materials.land", "catalogue", ["material"]);
    // Texture-plane analytics land on their OWN arm partitioned by `channel`, never folded under the materials
    // catalogue arm: a catalogue generation is one row per material row while a texture generation is one row
    // per CHANNEL of a set, so sharing the arm splits the catalogue tree at a segment a catalogue scan cannot
    // prune on and strands every channel question behind a full-arm read. `channel` is the segment a board
    // spells at its producer's CANONICAL name (`base_color`, `geometry_normal`, `orm`) and never at an ingest
    // alias (`basecolor`, `albedo`), because the segment is a directory a scan predicates on and two spellings
    // of one channel split its tree into halves no board joins; a per-channel cold-tail sweep then prunes whole
    // directories exactly as the catalogue arm prunes per catalogue.
    public static readonly LandingArm MaterialsTexture = new("materials-texture", "store.materials.texture.land", "channel", ["set"]);
    // Receipt evidence lands under its capability domain, so a cold-tail scan prunes whole directories on the
    // same segment a metric name and a residence sort key carry — one vocabulary across all three planes.
    public static readonly LandingArm Receipt   = new("receipt", "store.receipt.land", "domain", ["at"]);
    // Billing generations partition by their accrual WINDOW, never by a receipt domain: the chargeback batch
    // carries its own dataset shape, so folding it under the receipt arm splits that arm's tree at `schema=`
    // and a cold-tail sweep loses the one readable segment a whole billing period prunes on. `Segment` is an
    // `Identifier`, which refuses a digit lead, so the month token carries its own leading letter.
    public static readonly LandingArm Cost      = new("cost", "store.cost.land", "month", ["kind"]);

    public StoreSlot Slot { get; }
    public Identifier Partition { get; }
    // The order the arm's generations are WRITTEN in, declared on the dataset shape because the row IS that shape:
    // the Parquet writer stamps it as sorting-column metadata and the scan side's row-grain skip reads it, so an
    // arm whose scans predicate on a content key sorts on that key and one whose scans walk time sorts on the
    // instant. A call-site literal here would let two generations of one arm claim different orders in one tree.
    public Seq<Identifier> Sorted { get; }
    private LandingArm(string key, string slot, string partition, Seq<string> sorted) : this(key) =>
        (Slot, Partition, Sorted) = (StoreSlot.Create(slot), Identifier.Create(partition), sorted.Map(Identifier.Create));
}

// ONE landed generation coordinate, and the only owner of a cold-tail directory spelling. Four segments carry
// four distinct facts and each earns its place:
//   `tenant=`     — the WHOLE `ResidenceTenancy.Prefix` mechanism. `Residence.Lake` renders its tenancy
//                   predicate against a `tenant` column no Lake dataset declares and `hive_partitioning`
//                   projects that column back from this segment alone, so a tree missing it answers every
//                   tenant-scoped scan with zero rows and raises nothing on any engine.
//   `<arm key>=`  — the arm's own partition noun at a READABLE value, so a domain, model, study, or catalogue
//                   scan prunes whole directories on the segment a metric name and a residence sort key carry.
//                   Keying it by a content hash prunes exactly as well and names nothing a board can spell.
//   `schema=`     — the producer's schema-identity content key, which is what makes an additive column a
//                   compatible generation rather than a split tree.
//   `generation=` — the generation content key the artifact index registers, so a retry re-lands clean.
// DuckDB resolves a hive key colliding with a body column IN FAVOUR OF THE DIRECTORY — shadowing the file's
// own value with no error and no duplicate column — so `Segment` derives from whichever projection wrote that
// body column and never spells a second time at a call site; a divergent pair silently rewrites evidence
// on read.
public readonly record struct LakeGeneration(
    LandingArm Arm, TenantContext Tenant, Identifier Segment, UInt128 SchemaKey, UInt128 GenerationKey) {
    public StorePath Path(StorePath root) =>
        StorePath.Create(string.Create(CultureInfo.InvariantCulture,
            $"{(string)root}/{(string)Residence.TenantColumn}={Tenant.Entry}/{(string)Arm.Partition}={(string)Segment}"
            + $"/schema={SchemaKey:x32}/generation={GenerationKey:x32}/data.parquet"));
}
```

| [INDEX] | [POLICY]             | [VALUE]                                      | [BINDING]                                                      |
| :-----: | :------------------- | :------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | BimOpenSchema egress | co-transactional `FlatTableProjection`       | read-your-writes, never daemon-lagged (`M4`)                   |
|  [02]   | column value         | smart-enum `.Key` / `GraphDelta` count       | `StatementMap.Map` writes a primitive, never the object        |
|  [03]   | BIM tables           | eleven suffixed columnar tables              | written in-corpus; the DEBUG-IL writer stays off the hot path  |
|  [04]   | Parquet codec        | `ParquetSharp.Arrow` ↔ `RecordBatch` codec   | distinct from the DuckDB SQL parquet path; meet at the file    |
|  [05]   | encrypted extract    | one `PmeCustody` on the write AND the read   | the factory outlives every reader it arms; never a split owner |
|  [06]   | lakehouse            | `PublishDelta` metadata-only commit          | `AddAction` registration; `TransactionVersion` = the AS-OF cut |
|  [07]   | path admission       | `StorePath` on both Parquet codec legs       | read/write/encryption consume ONE admitted path value          |
|  [08]   | lake scan            | `DatasetReader` + `HivePartitioning.Factory` | `Col`/`IFilter` pushdown; no DuckDB mount in the loop          |
|  [09]   | read tuning          | one `ScanTuning` across both Parquet legs    | footer bounds, range coalescing, batch grain; never one leg    |
|  [10]   | carrier decode       | one `CompressionCodecFactory` at ingest      | identity reads decompressed bytes; framing never enters keys   |
|  [11]   | landing spine        | `LandingArm` row per producer family         | schema handoff only; storage, slots, custody stay here         |
|  [12]   | generation directory | `LakeGeneration` tenant/segment/key spelling | hive key wins a body-column collision; segment derives once    |

## [05]-[ANALYTICS_RESIDENCE]

- Owner: `ColumnType`/`ColumnShape`/`ColumnRow`/`AnalyticsSchema` the producer handoff vocabulary crossing the `[WIRE]: AnalyticsSchema` seam that `Rasm.Element`, `Rasm.Materials`, and `Rasm.Compute` each hand their typed record-batch schema across, `ColumnType` carrying one physical token per residence dialect beside the Arrow field and the Substrait literal that token admits and `ColumnShape` generating the list, map, and dictionary-encoded containers over it, so a neutral producer token — scalar or composite — spells itself three ways in SQL, once in a record batch, and once in a plan with no consumer branch, `TimeSpine` the declared temporal category deciding who owns a dataset's clock; and `AnalyticsSchema` carrying the TEMPORAL SPINE — the time column every residence partitions on, the category naming its owner, and the optional measure a rollup folds; `Residence` the `[SmartEnum<string>]` residence family keyed by CAPABILITY — `Series` the temporal projection tier, `Fleet` the interactive wide-event tier, `Lake` the cold tail — each row answering the estate residence floor (`Fits`, `Admit`, `Tenancy`, `Lifetime` carrying both its extent and its ending owner, `Degrade`, and `Cap` stated permanently false) beside this plane's own extension: the projections it answers, its physical projection, its dialect tokens, its tenant and instant literals, and the provisioning statements its own engine runs; `ResidenceProjection` the closed projection vocabulary a residence declares rather than degrading silently; `ResidenceWindow` the half-open read window and `ResidenceScope` the one read frame every entry takes; `ResidenceReach` the transport union one read discriminates on; `ResidencePlan` the ONE Substrait `RelationVisitor` lowering a logical plan per dialect; `ResidenceDdl` the parameterized provisioning emitter; `ResidenceRead` the ONE query entry with `ResidenceReceipt` its non-generic read evidence; `ResidenceCell` the write counterpart of `ResidenceRow` and `ResidenceLanding` the ONE relational landing with `ResidenceIngestReceipt` its staged-count evidence; `SeriesKind`/`SeriesPoint`/`SeriesSelector`/`SeriesLane` the Series residence's hypertable roster, its landing arm, key-or-facet selection, and summary-backed reads; `WarehouseOpRow`/`WarehouseSchema` the Fleet residence's op-log row vocabulary both ends of the `Version/egress` seam read; `ReceiptFactRow`/`ReceiptResidence` the kernel receipt stream's wide-event plane, its numeric measure projection, its Arrow landing batch, its named scan plans, and its resident envelope read; `EngineFault` the one engine-neutral diagnostic pair every residence renders its own provider failure into; `ResidenceHealth` the family policy-health row and `SeriesJobHealth` the Timescale-only bgworker enrichment beside it; `ResidenceFault` the closed `FaultBand.Series` band.
- Cases: `Residence` is `Series` (relational hypertables under in-database policies), `Fleet` (MergeTree wide events under table TTL), `Lake` (hive Parquet generations under generation eviction); `ResidenceProjection` is `Point`/`Window`/`Quantile`/`Aggregate`/`Fraction` and every residence publishes the subset it answers, so a plan naming an unanswered projection refuses typed at the seam carrying that row's `Degrade` clause instead of rendering an empty tile; `ResidenceTenancy` is `SortKey` (the residence stores the tenant as its leading column) and `Prefix` (a hive directory holds it and the scan projects it back), so tenancy decides where the byte rests and never how a predicate compares it; `ResidenceReach` is `Relational(NpgsqlDataSource)` | `Fleet(ClickHouseClient)` | `Flight(FlightSqlClient)` | `Local(ColumnarSession)`; `SeriesSelector` is `Key` (the source artifact's own content key) and `Facets` (the ordered text values a board spells), so one predicate fragment serves a content-addressed read and a dashboard read alike; `SeriesKind` is `Assessment` (discipline-assessment series — energy, thermal, daylight — 1-day chunks, 1-hour bucket, 365-day retention, 7-day columnstore age), `Sensor` (BMS/operational streams — 1-day chunks, 15-minute bucket, 90-day retention, 2-day columnstore age), and `Telemetry` (the receipt-stream measure projection — 1-day chunks, 1-minute bucket, 90-day retention, 1-day columnstore age, `domain`/`slot`/`measure` facets naming the stream in text); `ColumnShape` is `Scalar` | `List` | `Map` | `Dictionary`, the last an ENCODING declaration the Fleet dialect wraps and the other two dialects leave bare; `TimeSpine` is `Event` (the producer stamps its own observation clock as a declared column) and `Landing` (the producer declares none and this custodian stamps the moment it admitted the batch); `ResidenceFault` is `IngestRefused | Unprovisioned | ReadRefused | Unanswerable | Unlowerable | Unwritable` (`8481`-`8486`), every refusal naming its residence beside one `EngineFault` pair rather than a column one backend fills.
- Entry: `public static Fin<AnalyticsSchema> Admit(string dataset, Seq<(string Name, string Type, bool Nullable)> columns, Seq<string> key, string spine, Option<string> time = default, Option<string> measure = default)` is the one seam gate turning a producer's wire schema into admitted identifiers and its category token into the `TimeSpine` row, refusing a dataset whose declared category and columns disagree, appending the landing column a `Landing` dataset hands over, and proving every key, time, and measure name against the roster before a statement composes; `public static Seq<string> ResidenceDdl.Provision(Residence residence, AnalyticsSchema schema, ResidencePolicy policy)` derives the WHOLE ordered idempotent statement set the reviewed-migration artifact carries, so no environment hand-spells a policy script and no exporter creates a table; `public static IO<Fin<ResidenceResult<T>>> ResidenceRead.Read<T>(ResidenceReach reach, Plan plan, ResidenceScope scope, ResidenceProjection projection, Func<ResidenceRow, Fin<T>> shape)` is the ONE query entry over every residence — the logical plan lowers once per dialect and executes on the reach the value's own shape discriminates; `public static IO<Fin<ResidenceIngestReceipt>> ResidenceLanding.Stage(NpgsqlDataSource store, AnalyticsSchema schema, Seq<Seq<ResidenceCell>> rows, ProjectionContext frame)` is the ONE relational landing — the column list, the tenancy lead, and every wire type derive from the same schema the DDL emitter provisions from, and its success branch alone calls `CompleteAsync`, and it stamps every custodian-owned column — the tenant leading and a `Landing` dataset's instant trailing — so a producer sends exactly the cells its category obliges — with `SeriesLane.Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame)` its hypertable-family arm projecting points into the kind's declared column order; `public static IO<Fin<ResidenceResult<ResidenceHealth>>> ResidenceRead.Health(ResidenceReach reach, Residence residence, AnalyticsSchema schema)` is the FAMILY policy probe every residence answers over the same four reach arms, measuring the resident time extent against the declared horizon; `SeriesLane.Weighted`/`Bucketed`/`Jobs` are the raw-chunk time-weighted read, the accessor read over the materialised summaries, and the Timescale-only bgworker run history naming WHICH policy stalled, the first two taking one `SeriesSelector` and one `ResidenceWindow`; `ReceiptResidence.Facts`/`Points`/`Batch`/`Scan`/`Resident` are the wide-event fold, the numeric measure projection feeding `SeriesKind.Telemetry`, the Arrow landing batch, the named plan a durable evidence read takes instead of assembling relations, and the resident envelope read the AppUi evidence source binds.
- Auto: the residence family is ONE row set answering the estate residence floor beside this plane's own extension, so adding a residence is a row carrying what it fits, the entry that admits into it, its tenancy mechanism, how long a resident row survives beside the owner that ends it, its honest projection subset, its dialect tokens, and the clause naming what it gives up, and adding a physical type is one `ColumnType` row answering every dialect column and a container is one `ColumnShape` case whose four composer columns each residence fills — a residence hardcoded below the family, a second query language, or a raw-SQL reader is the deleted form the `ResidencePlan` fold forecloses; landing stays each residence's declared owner — `ResidenceLanding.Stage`'s binary COPY for the Series tier with `SeriesLane.Ingest` its hypertable-family arm, the `Version/egress` sink for the Fleet tier, `#FLAT_TABLE_EGRESS`'s `LakeGeneration` for the Lake tier — so no second writer enters beside a reader and a producer-declared dataset lands through the same entry its own schema provisions; a cell arm and its column's declared row prove ARITY and TYPE together ahead of the copy against the SUPPLIED roster — `Payload` minus every column the custodian stamps — because a binary importer infers nothing from a column list and a mismatch found at row n discards the n-1 rows already staged, while an arity gate counting the custodian's own columns refuses every producer whose category forbids it to carry them; one batch reads the landing clock once, so a single COPY carries one admission instant; provisioning is DERIVED per residence from the schema's own spine — the Series arm splits SELECT functions (`create_hypertable`, `add_retention_policy`, `add_continuous_aggregate_policy`) from CALL procedures (`add_columnstore_policy`) so a mis-verbed row is unrepresentable, the Fleet arm emits `CREATE TABLE … ENGINE = MergeTree` with the tenant leading and the time column trailing `ORDER BY`, a `TTL … DELETE` from the row's own retention window, and one `bloom_filter` skip index per admitted text column outside the sort key, and the Lake arm creates no storage and emits exactly the VIEW that gives its hive tree the name every lowered plan addresses; a measure-free dataset provisions its hypertable, columnstore, and retention and emits no continuous aggregate, so a wide event never grows a fabricated `avg` over a column it never declared; grouping and segmenting are DIFFERENT lists derived from the same schema — the rollup groups the whole key so each stream keeps its own buckets while the columnstore segments the bounded text keys alone, because segmenting on a `KeyHex` content key mints one compressed batch per row and deletes the compression the columnstore exists for; refresh, retention, and compression run on each residence's OWN scheduler — TimescaleDB bgworkers, ClickHouse merges, generation eviction — and no telemetry worker or scheduler surface enters this branch; policy health reads as OUTCOME at the family and as self-report at the one tier publishing one — `ResidenceRead.Health` measures each residence's resident time extent against its declared horizon so a stalled expiry surfaces on every tier, and `SeriesLane.Jobs` adds the Timescale `timescaledb_information.jobs`+`job_stats` join naming which bgworker stalled; a family probe transcribing one engine's catalog measures one tier and reports a healthy silence for the other two; the rollup materialises toolkit SUMMARY state — `time_weight` beside `percentile_agg` — and the reader names its accessor, so the cheap tile and the expensive raw-chunk investigation answer ONE statistic where a materialised `avg` beside a `time_weight` read is two means wearing one caption; every read lands a `ResidenceResult` carrying the residence, the lowered text, and the rows scanned, the Fleet arm filling it from the `QueryStats` `X-ClickHouse-Summary` receipt and `Receipt` projecting it non-generically onto the read slot.
- Receipt: a provisioning derivation rides `store.columnar.residence.provision` carrying the residence and the row count; an ingest rides `store.columnar.residence.ingest` as one `ResidenceIngestReceipt` naming its dataset beside the staged count; a residence read rides `store.columnar.residence.read` as the non-generic `ResidenceReceipt` carrying the residence key, the lowered text, the scanned rows, and the elapsed figure, and the last two project onto the `#STORE_INSTRUMENTS` residence rows.
- Packages: Npgsql (`NpgsqlDataSource.OpenConnectionAsync`/`NpgsqlConnection.BeginBinaryImportAsync`/`NpgsqlBinaryImporter.StartRowAsync`/`WriteAsync`/`CompleteAsync`; `NpgsqlDbType`), timescaledb + timescaledb_toolkit (`create_hypertable`/`by_range`/`add_retention_policy`/`add_columnstore_policy`/`add_continuous_aggregate_policy`/`time_bucket`/`time_weight`/`average` — server-side SQL per `api-timescaledb`/`api-timescaledb-toolkit`), ClickHouse.Driver (`ClickHouseClient.CreateConnection`/`ClickHouseCommand.ExecuteReaderAsync`/`ExecuteNonQueryAsync`/`QueryStats`/`ClickHouseServerException`), FlowtideDotNet.Substrait (`Plan`/`Relation`/`RelationVisitor<TReturn,TState>`/`ReadRelation`/`FilterRelation`/`ProjectRelation`/`AggregateRelation`/`SortRelation`/`FetchRelation`/`RootRelation`/`NamedTable.Names`/`NamedTable.DotSeperated`/`NamedStruct.Names`/`DirectFieldReference`/`StructReferenceSegment.Field`/`ScalarFunction`/`AggregateFunction`/`SortField`/`SortDirection`/`Literals.NumericLiteral`/`Literals.StringLiteral`/`FunctionsComparison`/`FunctionsArithmetic`/`FunctionsAggregateGeneric`), Apache.Arrow.Flight.Sql (`Apache.Arrow.Flight.Sql.Client.FlightSqlClient.ExecuteAsync(string, Transaction)`/`DoGetAsync(FlightTicket)`, `Transaction.NoTransaction`), Apache.Arrow.Flight (`FlightInfo.Endpoints`/`TotalRecords`, `FlightEndpoint.Ticket`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new residence is one `Residence` row answering the estate floor beside every extension column this plane declares; a new physical type is one `ColumnType` row answering every dialect token, its Arrow field, its binary-COPY wire type, and the plan literal it admits, beside the one `ResidenceCell` arm that fills it; a new container is one `ColumnShape` case beside four composer columns per residence, never a flat row per element instantiation; a new producer dataset landing is one cell projection over its own `AnalyticsSchema` beside the `TimeSpine` its semantics name, never a second importer; a third clock owner is one `TimeSpine` row breaking the supplied-roster and stamp folds at compile time; a new series family is one `SeriesKind` row deriving its whole provisioning set; a new way to name a stream is one `SeriesSelector` case breaking both reads at compile time; a new transport is one `ResidenceReach` case breaking the read dispatch at compile time; a new pushdown is one `ResidencePlan` arm; zero new surface — a per-environment policy script, an exporter-created table, a second query language, a raw-SQL reader, a per-residence read entry, a per-selector read entry, an AppHost-scheduled refresh, a telemetry worker, or a cardinality ceiling is the deleted form because the family generates the space, the plan is one IR, and the engines own their own cadence.
- Boundary: a residence row is TEMPORAL by construction — every residence partitions, prunes, and expires on time — and WHICH clock a dataset dates by is its declared `TimeSpine`, never an inference off whether a `time` argument arrived: an event-time dataset names its observation column and stamps every cell, a landing-time dataset names none and this custodian owns the instant, and the seam refuses either category paired with the other's columns, because a producer stamping a landing clock defeats the category whose whole meaning is custodian admission while an event-time dataset silently re-dated to admission strands every board joining two datasets on one time axis; no producer ever learns a chunk interval, a TTL, or a partition expression; SCOPE and SHAPE part at the read — residence, schema, window, and frame ride one `ResidenceScope` value, so a Substrait plan carries filters, projections, and folds alone and an unbounded or cross-tenant residence scan has no shape that expresses it; a physical value whose spelling differs per engine — the 16-byte tenant key, an instant — renders through its residence's own literal column, because a quoted hex text compared against a `bytea` or a `FixedString(16)` matches nothing and raises nothing, and the whole point of the leading sort key is lost the moment that predicate silently fails; every residence answers the ONE lowered `FROM <table>` — the two relational tiers by owning the table and the cold tail by owning a view over its hive tree — so a reach that cannot name a relation is unrepresentable rather than a scan against a relation nobody created; NO analytics residence carries a cardinality cap and no row can grow one — a metrics store demands view caps because a TSDB indexes every series, while unbounded dimensionality IS the reason these residences exist, so a ceiling here deletes the capability and the `#STORE_INSTRUMENTS` `rasm.tenant` view cap governs the metrics plane alone; every residence is DERIVED and carries zero authority — the receipt stream and the identity-tier journal own truth, a residence drops at warm-up cost and rebuilds from evidence, and reading one as authority turns a dropped accelerator into billing loss; DDL for every relation this branch's producers fill is branch-owned at this custodian, and the collector's `clickhouseexporter` runs `create_schema: false` because a default exporter schema leaves every attribute in a `Map` outside the sort key and a single-tenant filter then scans granules holding every other tenant; the Series tier is a RELATIONAL residence beside `element_identity` and never an artifact-catalog class — the heavy source artifact stays the `Query/cache` `ArtifactKind.Assessment` content-keyed row under `RetentionClass.Cache` and the hypertable is its queryable temporal PROJECTION whose chunks `add_retention_policy` drops in-database (re-derivable by re-ingest — cost, never correctness), so `Version/retention#SWEEP_AND_GC` never deletes residence rows and a `RetentionClass` row for them is the rejected double-governor; the Fleet leg is READ-side only — `Version/egress`'s `ClickHouse` sink owns landing under `insert_deduplication_token` dedup and the two meet at `WarehouseSchema.Table`/`Columns`, the ONE typed row vocabulary a fleet question composes over, so writer and reader cannot drift while naming one table — and ClickHouse carries no transaction, so every fleet read is a convergence-consistent view whose staleness the egress cursor bounds; the `Flight` reach is the estate's one cross-runtime columnar query plane per the Tier-0 Flight SQL ruling, so a runtime needing new residence capability extends `ResidencePlan` rather than minting a sidecar transport; this custodian reads NO metrics store and owes no reach row for one — Prometheus, VictoriaMetrics, and Mimir stay outside the roster, and Tier-0 `[08]` `[EVIDENCE_RESIDENCE]` already routes durable signal evidence here through the branch analytics custodian while the metrics plane serves live alerting at its own store; the branch's metric-side obligation is therefore EMISSION, not query — `Store/observability#STORE_INSTRUMENTS` mounts the rows and the OTLP wire carries them — so the Tier-0 conformance table holds with zero C# metrics-store reader, and a `Residence` row named for a TSDB imports the cardinality cap this family deletes and forks the one lowered-plan currency onto a second query language; COLUMN TYPES are branch-local vocabulary and never a cross-language correspondence: no `tests/contracts/MANIFEST.md` entry names a column type, and a residence relation plants at the tier that installs the residence and whose writer fills it — this custodian for every relation its own producers land, the deploy plane for the collector-filled wide events — readers transcribing the planter's spelling rather than mirroring these keys, with a cross-branch correspondence earned by a manifest entry beside its conformance fixture; PRODUCERS are named at both ends — `Rasm.Element` hands its `element.*` catalogue datasets, `Rasm.Materials` its `materials.*` catalogue rows through the `catalogue`-partitioned arm and its per-channel texture-plane generations through the `channel`-partitioned `MaterialsTexture` arm, and `Rasm.Compute` its DOE and chargeback batches beside its geometry corpus through the LAKE lane, whose generation keys on the kernel `PackSchema.SchemaId` the `LandingArm.Geometry` row names by law — each crossing the `[WIRE]: AnalyticsSchema` seam as text a producer writes and this seam admits, so a producer declares its own dataset and learns no storage type, and the collector's own OTLP wide events land as `Map`/`List`/`Dictionary` columns the installing tier plants before the exporter's first write — an exporter creates no table on any residence.

```csharp signature
using Apache.Arrow;
using Apache.Arrow.Flight;
using Apache.Arrow.Flight.Sql;                    // Transaction.NoTransaction — the client itself homes one namespace deeper
using Apache.Arrow.Flight.Sql.Client;             // FlightSqlClient
using Apache.Arrow.Types;                         // the Arrow face of the one ColumnType row set
using ClickHouse.Driver;
using ClickHouse.Driver.ADO;
using FlowtideDotNet.Substrait;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.FunctionExtensions;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Type;
using NodaTime.Text;                              // InstantPattern — the one ISO text every residence stamp wraps
using Npgsql;
using NpgsqlTypes;
using System.Buffers.Binary;                      // BinaryPrimitives — the big-endian key pack and its inverse
using System.Collections.Frozen;
using System.Diagnostics;                         // Stopwatch — the elapsed figure a reach without server stats reports
using System.Globalization;
using System.IO.Hashing;                          // XxHash128 — the measure-path series identity
using System.Text;
using System.Text.Json;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// Producer handoff vocabulary — the receiving half of the `[WIRE]: AnalyticsSchema` seam every producer
// hands its typed record-batch schema across. The KEY roster mirrors the producer's own neutral tokens and the
// physical columns are custodian-only, so a residence spells a producer token without the producer ever naming
// a storage type; a producer minting a key absent here fails the mirror at the admission gate below.
// `Arrow` is the RECORD-BATCH face of the same row — the seam trades batches, so the vocabulary that spells
// three SQL dialects spells the batch field too and no landing hand-builds a schema beside the declaration.
// `Plan` renders a narrowing value as the Substrait literal the column's own type admits: a quoted string
// compared against an `Int64` column is a ClickHouse type error and a silently coerced Postgres one, and the
// two types Substrait carries NO literal for return `None` here, because a tenant key and an instant are read
// SCOPE the frame already owns rather than plan shape a filter could carry.
// This roster is BRANCH-LOCAL vocabulary, never a cross-language correspondence: Tier-0 `[03]` grants a shape
// cross-branch meaning only through a `tests/contracts/MANIFEST.md` entry, none names a column type, and a peer
// runtime planting a residence relation reaches this custodian's DDL rather than spelling its own token set.
// That manifest entry beside a conformance fixture earns a cross-branch correspondence; mirrored spellings
// with no entry behind them fork on the first row either side adds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ResidenceFault>]
public sealed partial class ColumnType {
    public static readonly ColumnType Utf8      = new("utf8", "text", "String", "VARCHAR", StringType.Default, NpgsqlDbType.Text, Text);
    public static readonly ColumnType Float32   = new("float32", "real", "Float32", "FLOAT", FloatType.Default, NpgsqlDbType.Real, Number);
    public static readonly ColumnType Float64   = new("float64", "double precision", "Float64", "DOUBLE", DoubleType.Default, NpgsqlDbType.Double, Number);
    public static readonly ColumnType Int32     = new("int32", "integer", "Int32", "INTEGER", Int32Type.Default, NpgsqlDbType.Integer, Number);
    public static readonly ColumnType Int64     = new("int64", "bigint", "Int64", "BIGINT", Int64Type.Default, NpgsqlDbType.Bigint, Number);
    // Unsigned rows widen on the Series dialect because PostgreSQL carries no unsigned integer: a `UInt8`
    // severity lands in `smallint`, a `UInt32` in `bigint`, and a `UInt64` in `numeric(20,0)` because `bigint`
    // is signed 64 and an OTLP counter past 2^63 wraps to a negative rather than refusing. The Fleet and Lake
    // dialects carry the exact width, so the widening is one row's honest column and never a lost value.
    public static readonly ColumnType UInt8     = new("uint8", "smallint", "UInt8", "UTINYINT", UInt8Type.Default, NpgsqlDbType.Smallint, Number);
    public static readonly ColumnType UInt32    = new("uint32", "bigint", "UInt32", "UINTEGER", UInt32Type.Default, NpgsqlDbType.Bigint, Number);
    public static readonly ColumnType UInt64    = new("uint64", "numeric(20,0)", "UInt64", "UBIGINT", UInt64Type.Default, NpgsqlDbType.Numeric, Number);
    public static readonly ColumnType Bool      = new("bool", "boolean", "Bool", "BOOLEAN", BooleanType.Default, NpgsqlDbType.Boolean, Flag);
    public static readonly ColumnType Date      = new("date32", "date", "Date32", "DATE", Date32Type.Default, NpgsqlDbType.Date, Unplanned);
    public static readonly ColumnType Timestamp = new("timestamp-ns", "timestamptz", "DateTime64(9)", "TIMESTAMP_NS", new TimestampType(TimeUnit.Nanosecond, "UTC"), NpgsqlDbType.TimestampTz, Unplanned);
    public static readonly ColumnType KeyHex    = new("fixed-hex128", "bytea", "FixedString(16)", "BLOB", new FixedSizeBinaryType(16), NpgsqlDbType.Bytea, Unplanned);

    public string Series { get; }
    public string Fleet { get; }
    public string Lake { get; }
    public IArrowType Arrow { get; }
    // Binary-COPY wire type: the Series tier lands through an importer that infers NOTHING from the column
    // list, so the physical type a row declared for its DDL is the same value its ingest binds — the row
    // answers the landing dialect exactly as it answers the three query dialects and the record batch.
    public NpgsqlDbType Wire { get; }
    public Func<string, Option<Expression>> Plan { get; }
    private ColumnType(string key, string series, string fleet, string lake, IArrowType arrow, NpgsqlDbType wire, Func<string, Option<Expression>> plan) : this(key) =>
        (Series, Fleet, Lake, Arrow, Wire, Plan) = (series, fleet, lake, arrow, wire, plan);

    // `NumericLiteral.Value` is `decimal`, so every numeric narrowing crosses one parse and a magnitude
    // past that range refuses rather than lowering a rounded operand no predicate would match.
    static Option<Expression> Text(string value) => Some<Expression>(new StringLiteral { Value = value });
    static Option<Expression> Number(string value) =>
        decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal magnitude)
            ? Some<Expression>(new NumericLiteral { Value = magnitude })
            : None;
    static Option<Expression> Flag(string value) =>
        bool.TryParse(value, out bool state) ? Some<Expression>(new BoolLiteral { Value = state }) : None;
    static Option<Expression> Unplanned(string _) => None;
}

// COMPOSITE shape over the scalar roster: a wide-event producer hands attribute MAPS, span-event and link
// ARRAYS, and dictionary-encoded low-cardinality text, so the vocabulary that spells three dialects generates
// those containers rather than enumerating one flat row per instantiation — `Map(Utf8, Utf8)` and
// `Map(Utf8, Float64)` are two values of one case, where a `map-string-string` scalar row would mint a roster
// entry per element pair and strand every pair nobody thought to name. Nesting is by construction, so a
// `List(Map(Utf8, Utf8))` resource-attribute run needs no new case.
// `Dictionary` is an ENCODING declaration, not a distinct logical type: the Fleet dialect wraps
// `LowCardinality`, the Lake dialect leaves the token bare because Parquet dictionary-encodes a column chunk
// on its own statistics, and the Series dialect leaves it bare because PostgreSQL has no inline encoding
// wrapper — one shape, three honest spellings, and a producer never learns which engine compresses how.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ColumnShape {
    private ColumnShape() { }
    public sealed record Scalar(ColumnType Type) : ColumnShape;
    public sealed record List(ColumnShape Element) : ColumnShape;
    public sealed record Map(ColumnType Key, ColumnShape Value) : ColumnShape;
    public sealed record Dictionary(ColumnType Element) : ColumnShape;

    // Scalar lift is IMPLICIT so a scalar column declares as its bare row and a composite spells its case —
    // generated conversion stays off because two cases take a `ColumnType` and emit an ambiguous pair, so
    // this file writes the one wanted direction.
    public static implicit operator ColumnShape(ColumnType type) => new Scalar(type);

    // Segment and skip-index eligibility: a columnstore segments BOUNDED TEXT and a bloom filter covers it, so
    // a dictionary-encoded text column is the paradigm case rather than an exception. A content key mints one
    // compressed batch per row and deletes the compression the columnstore exists for, and a container carries
    // no single value an index entry addresses, so both stay out by shape rather than by a name check.
    public bool Bounded => Switch(
        scalar:     static c => c.Type == ColumnType.Utf8,
        dictionary: static c => c.Element == ColumnType.Utf8,
        list:       static _ => false,
        map:        static _ => false);

    // Arrow face folds with the shape: `MapType(key, value)` builds the entries struct itself, and the
    // dictionary index is `Int32Type.Default` because that ctor THROWS `ArgumentException` on a non-integer
    // index type — the width is fixed here so no call site can reach that throw.
    public IArrowType Arrow => Switch(
        scalar:     static c => c.Type.Arrow,
        list:       static c => new ListType(c.Element.Arrow),
        map:        static c => (IArrowType)new MapType(c.Key.Arrow, c.Value.Arrow),
        dictionary: static c => new DictionaryType(Int32Type.Default, c.Element.Arrow, ordered: false));

    // Substrait literal rendering is SCALAR-only: a narrowing predicate compares one value, and a container
    // comparison carries no literal the plan admits, so a filter over a map or list column refuses at
    // lowering rather than rendering an operand the dialect coerces to something no row matches.
    public Func<string, Option<Expression>> Plan => Switch(
        scalar:     static c => c.Type.Plan,
        list:       static _ => Unplannable,
        map:        static _ => Unplannable,
        dictionary: static c => c.Element.Plan);

    // Binary-COPY wire type for the Series landing, FALLIBLE where the shape outruns what one `NpgsqlDbType`
    // value spells: `Array` is a flag OR'd onto its element, so a nested list has no second flag bit and a
    // map lands as `Jsonb` whose element typing the wire value cannot carry. The refusal fires at admission,
    // ahead of a copy whose mismatch would surface at row n and discard the n-1 rows already staged.
    public Fin<NpgsqlDbType> Wire => Switch(
        scalar:     static c => Fin.Succ(c.Type.Wire),
        dictionary: static c => Fin.Succ(c.Element.Wire),
        map:        static _ => Fin.Succ(NpgsqlDbType.Jsonb),
        list:       static c => c.Element is Scalar leaf
            ? Fin.Succ(NpgsqlDbType.Array | leaf.Type.Wire)
            : Fin.Fail<NpgsqlDbType>(new ResidenceFault.Unwritable(Residence.Series.Key, "nested-list")));

    static readonly Func<string, Option<Expression>> Unplannable = static _ => None;
}

// One admitted column; `Identifier` is the trust gate the raw producer name crosses exactly once.
public readonly record struct ColumnRow(Identifier Name, ColumnShape Type, bool Nullable);

// One dataset with its ordered key columns and its TEMPORAL SPINE; `Dataset` keeps the producer's dotted
// `<producer>.<source>` grammar as the wire value and `Table` is its admitted single-identifier projection, so a
// dotted wire name never reaches engine SQL unquoted and two producers cannot collide on one physical table.
// `Time` names the column every residence partitions, buckets, and expires on, and `Measure` names the numeric
// column a rollup folds — a wide-event dataset carries none and says so, which is exactly what keeps the Series
// arm from emitting a continuous aggregate over a column that dataset never declared. Both resolve against the
// column roster at the admission gate, so a residence arm reads a proven identifier and never a spine literal.
// Temporal CATEGORY, declared and never inferred. EVENT-TIME datasets date by when the world produced the
// fact, so each names its own observation column and its producer stamps every cell; LANDING-TIME datasets
// date by when this custodian admitted them, so each names none and hands the clock to the custodian. Category is a dataset's own semantics, so it travels on the declaration rather than
// being read off whether a `time` argument happened to arrive — an optional clock alone leaves an event-time
// dataset silently re-dated to admission, and a board joining two datasets on time then compares two clocks
// under one axis with nothing raising.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ResidenceFault>]
public sealed partial class TimeSpine {
    public static readonly TimeSpine Event   = new("event");
    public static readonly TimeSpine Landing = new("landing");
}

public sealed record AnalyticsSchema(string Dataset, Seq<Identifier> Key, Seq<ColumnRow> Columns, Identifier Time, TimeSpine Spine, Option<Identifier> Measure) {
    // ONE canonical relation spelling all three dialects agree on: PostgreSQL FOLDS an unquoted identifier to
    // lower case while ClickHouse folds nothing and DuckDB preserves, so a mixed-case dataset — `telemetry.` +
    // a `TelemetrySource` row spelled `Rasm.AppUi` — provisions one relation and the quoted plan then addresses
    // another that was never created. Lower-casing at the one projection is what makes DDL and read name one table.
    public Identifier Table => Identifier.Create(Dataset.Replace('.', '_').ToLowerInvariant());
    public Seq<ColumnRow> Sorted => Key.Bind(key => Columns.Filter(column => column.Name == key));
    public Seq<ColumnRow> Payload => Columns.Filter(column => !Key.Contains(column.Name));
    public bool Declares(Identifier column) => Columns.Exists(row => row.Name == column);

    // Arrow face of the SAME declaration, so the record-batch handoff this seam is named for derives from the
    // row set that spells the SQL dialects: a Lake landing, a Flight batch, and the DDL cannot disagree on
    // field order, nullability, or physical type, and a hand-built schema beside the dataset has nothing to be.
    public Schema Fields => new(Columns.Map(static column => new Field((string)column.Name, column.Type.Arrow, column.Nullable)), null);

    // Declaration order IS the Substrait field-reference ordinal and the reader's column index alike, so a
    // plan builder addresses a column by NAME and every consumer's ordinals move with one column insert.
    public int Ordinal(Identifier column) => Columns.Map(static row => row.Name).IndexOf(column);
}

// Projection vocabulary a residence ANSWERS: a residence answering fewer declares the subset on its row and the
// read refuses typed, so a tile degrades visibly rather than a second query path opening beside it.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResidenceProjection {
    public static readonly ResidenceProjection Point     = new("point");
    public static readonly ResidenceProjection Window    = new("window");
    public static readonly ResidenceProjection Quantile  = new("quantile");
    public static readonly ResidenceProjection Aggregate = new("aggregate");
    public static readonly ResidenceProjection Fraction  = new("fraction");
}

// Tenancy mechanism per residence: a sort-key column prunes granules before the filter applies, a partition
// prefix prunes whole directories. Both resolve the SAME `TenantId.Wire` text, so a metric series and a
// residence row join on one alphabet.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResidenceTenancy {
    public static readonly ResidenceTenancy SortKey = new("sort-key");
    public static readonly ResidenceTenancy Prefix  = new("prefix");
}

// Retention, rollup grain, partition chunk, and residence root a residence provisions under, supplied by the
// composing root rather than baked, so an operator tunes the evidence horizon without a corpus edit. `Retain`
// is an INDEPENDENT coordinate, never a multiple of a series or alert window — deriving evidence retention from
// an alert lookback silently shortens the audit trail the moment an operator retunes the alert. This duration is
// exactly the extent each relational row's `Lifetime` column names, and the two spellings stay apart because one
// is the tuned magnitude and the other is the sentence stating who spends it. `Root` names
// where a dataset's bytes rest — hive generation directory on the cold tail, table itself on a relational
// tier — so the Lake arm reads its scan target from policy and never from a literal path in a fence.
public readonly record struct ResidencePolicy(Duration Retain, Duration Grain, Duration Chunk, Duration Backfill, StorePath Root);

// ONE diagnostic pair every engine renders its own failure into: a PostgreSQL `SqlState`, a ClickHouse numeric
// `ErrorCode`, and a DuckDB `ErrorType` are three alphabets for one question, so each renders to TEXT at its
// own row and the fault family carries the pair rather than a column one backend fills and the rest leave
// empty. `Code` stays the engine's OWN token — an estate-normalized code would erase the value an operator
// searches the engine's own documentation with.
public readonly record struct EngineFault(string Code, string Detail);

// Residence rows: ONE family answering the same capability columns. Adding a residence is a row; hardcoding a
// residence below the family is the defect this shape forecloses. `Fits`, `Admit`, `Tenancy`, `Lifetime`,
// `Degrade`, and `Cap` are the estate residence floor every branch's family answers, so a reader crossing this
// family and a peer's reads different VALUES under one column set; every column below them is this plane's own
// extension, because provisioning and lowering are what only a custodian decides. `Cap` is STATED and
// permanently false rather than omitted — unbounded dimensionality IS the capability, and a declared `false`
// is what a later pass has to overwrite instead of a gap it can helpfully fill. `Literal` is the
// column every dialect must answer and none can share: the tenant is a 16-byte key whose PHYSICAL spelling
// differs per engine, so a residence renders its own literal off the one `TenantId.Wire` hex text and a
// quoted-text comparison against a `bytea` or a `FixedString(16)` — which matches nothing and raises nothing —
// is unrepresentable from the family. `Degrade` states the honest clause a row gives up rather than a boolean,
// because what a residence cannot do is EVIDENCE both a tile and a refusal read — `Unanswerable` then names
// that limitation in the row's own words instead of handing an operator two keys and no reason. `Lifetime`
// carries BOTH halves in one string — how long a resident row survives and which owner ends it — across the
// three engines running three expiry schedulers. `Admit` names the entry that puts rows in, wide enough for a bulk door, a sink, and a
// per-point lane alike, so no residence spells its own narrower verb and strands the other two entry kinds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Residence {
    public static readonly Residence Series = new("series", ResidenceTenancy.SortKey,
        "temporal projection tier: bounded-key streams a board reads at interactive latency off materialised summaries",
        "`ResidenceLanding.Stage` binary COPY beside `SeriesLane.Ingest`, both against relations this custodian provisioned",
        "the declared `ResidencePolicy.Retain` extent, ended in-database by the Timescale bgworker `add_retention_policy` arms",
        "single-node; admits no wide event — a payload column belongs to the Fleet tier",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Quantile, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Series,
        // PostgreSQL spells an array by suffix and has no inline dictionary wrapper, so the encoding token
        // passes through and TOAST owns the compression. A map lands as `jsonb`: no relational type carries a
        // typed key-value pair, `hstore` is text-to-text alone, and stating that here keeps the widening one
        // row's honest column rather than a silent coercion the reader discovers through a failing predicate.
        static element => $"{element}[]",
        static (_, _) => "jsonb",
        static element => element,
        static name => $"\"{name}\"",
        static entry => $"'\\x{entry}'::bytea",
        static iso => $"CAST('{iso}' AS timestamptz)",
        static (column, grain) => $"time_bucket(INTERVAL '{Interval(grain)}', {column})",
        static (column, quantile) => $"percentile_cont({quantile.ToString("0.####", CultureInfo.InvariantCulture)}) WITHIN GROUP (ORDER BY {column})",
        // Provider failure renders through a TYPE TEST, never a cast: a driver raising a socket, TLS, or
        // cancellation exception is not a `PostgresException`, and casting one at the fold throws straight out
        // of the `Fin` the rail exists to carry.
        static error => error is PostgresException wire
            ? new EngineFault(wire.SqlState, wire.MessageText)
            : new EngineFault("<provider>", error.Message),
        SeriesResidence.Statements);

    public static readonly Residence Fleet = new("fleet", ResidenceTenancy.SortKey,
        "interactive wide-event tier: any cardinality, the tenant leading the sort key so a single-tenant filter prunes granules",
        "the `Version/egress` ClickHouse sink under `insert_deduplication_token`, beside the collector's own OTLP wide events",
        "the declared `ResidencePolicy.Retain` extent, ended by the ClickHouse merge scheduler running the row's `TTL … DELETE`",
        "no transaction — every read is a convergence-consistent view the egress cursor bounds",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Quantile, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Fleet,
        // ClickHouse is the one dialect carrying all three containers natively, so the OTLP attribute map, the
        // span-event array, and the low-cardinality dimension each render exactly rather than widening.
        static element => $"Array({element})",
        static (key, value) => $"Map({key}, {value})",
        static element => $"LowCardinality({element})",
        static name => $"`{name}`",
        static entry => $"unhex('{entry}')",
        // ClickHouse's own CAST parses a space-separated datetime, never the ISO `T`/`Z` form, so this row
        // spells the best-effort parser rather than the shared cast its two siblings share.
        static iso => $"parseDateTime64BestEffort('{iso}', 9, 'UTC')",
        static (column, grain) => $"toStartOfInterval({column}, INTERVAL {(long)grain.TotalSeconds} SECOND)",
        static (column, quantile) => $"quantileTDigest({quantile.ToString("0.####", CultureInfo.InvariantCulture)})({column})",
        // `ClickHouseServerException.ErrorCode` is the inherited `DbException` int, and the driver parses it out
        // of the server's own text — an unparsable message yields `-1`, so the render carries whatever the
        // driver resolved rather than asserting a code it never read.
        static error => error is ClickHouseServerException server
            ? new EngineFault(server.ErrorCode.ToString(CultureInfo.InvariantCulture), server.Message)
            : new EngineFault("<provider>", error.Message),
        FleetResidence.Statements);

    // Cold-tail reads answer scans and shares, never an interactive quantile: a Parquet generation carries
    // per-row-group statistics, not a digest, so a quantile tile over the lake reads as a report or refuses.
    // Its tenant literal is QUOTED TEXT like every other predicate here — a hive key reads back as a `VARCHAR`
    // column under `hive_partitioning`, so the prefix mechanism decides where the byte rests, never how the
    // scan compares it, and the directory spelling belongs to `#FLAT_TABLE_EGRESS`'s `LakeGeneration` alone.
    public static readonly Residence Lake = new("lake", ResidenceTenancy.Prefix,
        "cold tail: cheapest per byte, batch scan over hive Parquet generations the object plane holds",
        "`#FLAT_TABLE_EGRESS`'s `LakeGeneration` writes the generations this row's VIEW names",
        "whatever the object plane leaves resident, ended by `Store/blobstore#BLOB_GC` generation eviction; this residence expires nothing itself",
        "no interactive latency and no digest — a quantile tile here reads as a report or refuses",
        Seq(ResidenceProjection.Point, ResidenceProjection.Window, ResidenceProjection.Aggregate, ResidenceProjection.Fraction),
        static column => column.Lake,
        // DuckDB carries list and map natively; the dictionary token passes through because Parquet
        // dictionary-encodes a column chunk off its own statistics and a DuckDB `ENUM` is a named catalog
        // type, so declaring one here would bind the scan to a value roster the producer never froze.
        static element => $"{element}[]",
        static (key, value) => $"MAP({key}, {value})",
        static element => element,
        static name => $"\"{name}\"",
        static entry => $"'{entry}'",
        static iso => $"CAST('{iso}' AS TIMESTAMP_NS)",
        static (column, grain) => $"time_bucket(INTERVAL '{Interval(grain)}', {column})",
        static (column, quantile) => $"approx_quantile({column}, {quantile.ToString("0.####", CultureInfo.InvariantCulture)})",
        // Cold-tail reads answer through the in-process engine, so this diagnostic is the DuckDB `ErrorType`
        // enum name — the same token `ColumnarFault` carries on the engine decade, spelled once here.
        static error => error is DuckDBException engine
            ? new EngineFault(engine.ErrorType.ToString(), engine.Message)
            : new EngineFault("<provider>", error.Message),
        LakeResidence.Statements);

    public ResidenceTenancy Tenancy { get; }
    public string Fits { get; }
    public string Admit { get; }
    public string Lifetime { get; }
    public string Degrade { get; }
    // Floor column stated rather than omitted, computed rather than passed, and INSTANCE rather than static: no row
    // can answer it differently, so a constructor argument would offer the choice this family exists to refuse, while
    // a type-level member strands the one floor column a fold walking `Items` cannot read off the row beside its five
    // siblings — which is the reader crossing a family and changing SHAPE for a single column.
    public bool Cap => false;
    public Seq<ResidenceProjection> Projections { get; }
    public Func<ColumnType, string> Physical { get; }
    public Func<string, string> ListOf { get; }
    public Func<string, string, string> MapOf { get; }
    public Func<string, string> DictOf { get; }
    public Func<Identifier, string> Quote { get; }
    public Func<string, string> Literal { get; }
    public Func<string, string> Stamp { get; }
    public Func<string, Duration, string> Bucket { get; }
    public Func<string, double, string> Quantile { get; }
    public Func<Exception, EngineFault> Diagnose { get; }
    public Func<AnalyticsSchema, ResidencePolicy, Seq<string>> Statements { get; }

    private Residence(string key, ResidenceTenancy tenancy, string fits, string admit, string lifetime, string degrade,
        Seq<ResidenceProjection> projections,
        Func<ColumnType, string> physical, Func<string, string> listOf, Func<string, string, string> mapOf,
        Func<string, string> dictOf, Func<Identifier, string> quote, Func<string, string> literal,
        Func<string, string> stamp, Func<string, Duration, string> bucket, Func<string, double, string> quantile,
        Func<Exception, EngineFault> diagnose,
        Func<AnalyticsSchema, ResidencePolicy, Seq<string>> statements) : this(key) =>
        (Tenancy, Fits, Admit, Lifetime, Degrade, Projections, Physical, ListOf, MapOf, DictOf, Quote, Literal, Stamp, Bucket, Quantile, Diagnose, Statements) =
        (tenancy, fits, admit, lifetime, degrade, projections, physical, listOf, mapOf, dictOf, quote, literal, stamp, bucket, quantile, diagnose, statements);

    // Both refusal shapes derive from the row, so a fold naming a residence never hand-spells the engine's
    // diagnostic and every call site carries the residence key it already holds. Each is named for the FAULT it
    // mints rather than the verb that raised it: `Fits`, `Admit`, `Lifetime`, and `Degrade` are the estate floor's
    // own column names held here as properties, and a member group sharing a name with a property on one type
    // does not compile — so a factory spelled `Admit` is unrepresentable while `Ingest` stays free for the
    // landing entry `SeriesLane` owns.
    public ResidenceFault ReadRefused(Exception error) => new ResidenceFault.ReadRefused(Key, Diagnose(error));
    public ResidenceFault IngestRefused(Exception error) => new ResidenceFault.IngestRefused(Key, Diagnose(error));

    // POLICY HEALTH every residence answers, derived from the row's OWN tokens: three engines run three
    // expiry schedulers — Timescale bgworkers, ClickHouse TTL merges, generation eviction — and each publishes
    // its self-report in a catalog only that engine has, so a probe reading one engine's catalog measures one
    // tier and reports a healthy silence for the other two. This probe measures the OUTCOME instead: the
    // resident time extent against the declared horizon, which every residence answers because every residence
    // partitions on time. `Identifier` already gated the relation and the column, so the text carries no
    // parameter and the same statement runs on every reach the family serves.
    public string Horizon(AnalyticsSchema schema) =>
        $"SELECT MIN({Quote(schema.Time)}), MAX({Quote(schema.Time)}), COUNT(*) FROM {Quote(schema.Table)}";

    public bool Answers(ResidenceProjection projection) => Projections.Contains(projection);

    // ONE recursive render every DDL arm and every lowered projection reads: the shape walks itself and the row
    // supplies four tokens, so a nested attribute map spells three dialects from one declaration and no arm
    // re-implements the walk. A `Map` key renders through `Physical` because a key is scalar by construction.
    public string Render(ColumnShape shape) => shape.Switch(
        state: this,
        scalar:     static (row, c) => row.Physical(c.Type),
        list:       static (row, c) => row.ListOf(row.Render(c.Element)),
        map:        static (row, c) => row.MapOf(row.Physical(c.Key), row.Render(c.Value)),
        dictionary: static (row, c) => row.DictOf(row.Physical(c.Element)));

    // ONE instant spelling per dialect off one ISO text: a Substrait plan carries no timestamp literal, so an
    // instant reaching engine SQL as a bare number would compare against a `timestamptz`, a `DateTime64(9)`,
    // and a `TIMESTAMP_NS` as three type errors. The window is scope, not shape, so it rides the read frame
    // and every residence renders its own moment.
    public string Moment(Instant at) => Stamp(InstantPattern.ExtendedIso.Format(at));

    // ONE tenancy predicate every mechanism resolves: a sort-key residence compares its leading stored column
    // and a prefix residence compares the hive key its scan projects back as a column, so both read as the same
    // equality against `TenantId.Wire` — the hex text is the single alphabet a metric series and a residence row
    // join on. Tenancy decides where the byte RESTS (`ResidenceDdl.Columns`), never how a scan compares it, so a
    // second predicate shape here would be a branch with one body.
    public string Partition(TenantContext tenant) => $"{Quote(TenantColumn)} = {Literal(tenant.Entry)}";

    public static readonly Identifier TenantColumn = Identifier.Create("tenant");

    // Postgres and DuckDB both read a bare INTERVAL literal; seconds is the one grain both accept without a
    // unit table, and the Fleet arm spells its own seconds form because ClickHouse takes no INTERVAL string.
    internal static string Interval(Duration grain) =>
        $"{(long)grain.TotalSeconds} seconds";
}
```

```csharp signature
// --- [ERRORS] -----------------------------------------------------------------------------
// One band closes the whole residence family: ingest, provisioning, read, the projection a residence does not
// answer, the relation or expression a dialect cannot express, and the column shape a landing cannot write.
// Every refusal names its RESIDENCE beside one engine-neutral diagnostic pair.
[Union]
public abstract partial record ResidenceFault : Expected, IValidationError<ResidenceFault> {
    private ResidenceFault() : base() { }
    public sealed record IngestRefused(string Residence, EngineFault Engine) : ResidenceFault;
    public sealed record Unprovisioned(string Lane) : ResidenceFault;
    public sealed record ReadRefused(string Residence, EngineFault Engine) : ResidenceFault;
    // Refusal carries the row's own `Degrade` clause, so an operator reads WHY the residence cannot answer
    // rather than two keys and a shrug.
    public sealed record Unanswerable(string Residence, string Projection, string Degrade) : ResidenceFault;
    public sealed record Unlowerable(string Residence, string Node) : ResidenceFault;
    public sealed record Unwritable(string Residence, string Shape) : ResidenceFault;

    public override int Code => FaultBand.Series + Switch(
        ingestRefused: static _ => 1,
        unprovisioned: static _ => 2,
        readRefused:   static _ => 3,
        unanswerable:  static _ => 4,
        unlowerable:   static _ => 5,
        unwritable:    static _ => 6);

    public override string Message => Switch(
        ingestRefused: static c => $"<residence-ingest:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>",
        unprovisioned: static c => $"<residence-unprovisioned:{c.Lane}>",
        readRefused:   static c => $"<residence-read:{c.Residence}:{c.Engine.Code}:{c.Engine.Detail}>",
        unanswerable:  static c => $"<residence-unanswerable:{c.Residence}:{c.Projection}:{c.Degrade}>",
        unlowerable:   static c => $"<residence-unlowerable:{c.Residence}:{c.Node}>",
        unwritable:    static c => $"<residence-unwritable:{c.Residence}:{c.Shape}>");

    public override string Category => Switch(
        ingestRefused: static _ => "Ingest",
        unprovisioned: static _ => "Provision",
        readRefused:   static _ => "Read",
        unanswerable:  static _ => "Projection",
        unlowerable:   static _ => "Lowering",
        unwritable:    static _ => "Shape");

    public static ResidenceFault Create(string message) => new Unprovisioned(message);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam gate: a producer hands dotted names and neutral type tokens as TEXT, and this is the one place that
// text becomes admitted identifiers and vocabulary rows. Every column admits before any statement composes,
// so a hostile producer name is a typed refusal at the seam rather than an interpolation site downstream.
public static class AnalyticsSeam {
    // Landing instant the custodian owns exactly as it owns the tenant column: every residence partitions,
    // prunes, and expires on time, so a residence row is temporal by construction. A snapshot producer hands
    // catalogue columns and learns no storage concern — this seam appends the landing axis — while a producer
    // that already carries its own instant names it and keeps one column. Neither ever spells a chunk
    // interval, a TTL, or a partition expression.
    public static readonly Identifier LandedColumn = Identifier.Create("landed_at");

    public static Fin<AnalyticsSchema> Admit(
        string dataset, Seq<(string Name, string Type, bool Nullable)> columns, Seq<string> key,
        string spine, Option<string> time = default, Option<string> measure = default) =>
        (columns.TraverseM(Column).As(), key.TraverseM(static name => Trusted(name)).As(),
            Category(spine), time.Traverse(Trusted).As(), measure.Traverse(Trusted).As())
            .Apply(static (rows, keys, category, at, value) =>
                (Rows: rows, Keys: keys, Spine: category, At: at, Value: value))
            .As()
            .Bind(parts => Spined(dataset, parts.Spine, parts.Rows, parts.Keys, parts.At, parts.Value))
            .Bind(static schema => Resolved(schema));

    // Category crosses as TEXT exactly as every column token does, because the two AEC producers this seam names
    // reference the kernel alone and sit BELOW this custodian — a typed parameter is unconstructable at both, and
    // no reference closes that gap without inverting the edge the store already owns in the other direction.
    static Fin<TimeSpine> Category(string token) =>
        TimeSpine.Validate(token, null, out TimeSpine? spine) is { } fault
            ? Fin.Fail<TimeSpine>(fault)
            : Fin.Succ(spine!);

    // Category and columns AGREE or the dataset never admits, which is the one refusal that keeps the two
    // clocks apart: a landing-time dataset naming its own instant hands the custodian a clock it does not own,
    // and an event-time dataset naming none is re-dated to admission by the very append that serves the other
    // category. The landing column APPENDS, so it is the tail of `Columns` and the provisioned order is
    // `tenant`, every supplied column, then the custodian's instant — the exact order `ResidenceLanding.Stage`
    // writes, so the COPY column list and the write loop cannot drift.
    static Fin<AnalyticsSchema> Spined(
        string dataset, TimeSpine spine, Seq<ColumnRow> rows, Seq<Identifier> keys,
        Option<Identifier> at, Option<Identifier> measure) {
        // This seam drops the custodian's own tenant column from BOTH producer rosters exactly here, because a
        // producer naming `tenant` describes the key the seam already stamps: every downstream derivation — DDL
        // column list, sort key, rollup grouping, columnstore segment list, and COPY roster — then reads one
        // roster carrying it once, where a per-site filter leaves whichever site nobody remembered emitting a
        // second column at a second physical type or a duplicate `orderby` entry the storage parameter rejects
        // outright.
        Seq<ColumnRow> supplied = rows.Filter(static column => column.Name != Residence.TenantColumn);
        Seq<Identifier> key = keys.Filter(static name => name != Residence.TenantColumn);
        return at.Match(
            Some: named => spine == TimeSpine.Event
                ? Fin.Succ(new AnalyticsSchema(dataset, key, supplied, named, spine, measure))
                : Fin.Fail<AnalyticsSchema>(new ResidenceFault.Unprovisioned($"<schema-spine:{dataset}:landing-names-clock>")),
            None: () => spine == TimeSpine.Landing
                ? Fin.Succ(new AnalyticsSchema(dataset, key,
                    supplied + Seq(new ColumnRow(LandedColumn, ColumnType.Timestamp, Nullable: false)), LandedColumn, spine, measure))
                : Fin.Fail<AnalyticsSchema>(new ResidenceFault.Unprovisioned($"<schema-spine:{dataset}:event-names-no-clock>")));
    }

    // Every declared identifier resolves against the roster BEFORE a statement composes: a key the columns
    // omit, a time column no residence can partition on, and a rollup measure no aggregate can fold each
    // refuse here rather than emitting DDL the engine rejects at parse time on a table that then half-exists.
    // Identity and CARDINALITY are both proven, because a membership test alone reads a repeated name as
    // present: a twice-declared column mints two DDL entries at one name and a twice-named key mints a
    // duplicate `orderby` entry TimescaleDB rejects outright, and each survives every downstream derivation.
    static Fin<AnalyticsSchema> Resolved(AnalyticsSchema schema) {
        Seq<Identifier> declared = schema.Key + Seq(schema.Time) + schema.Measure.ToSeq();
        return declared.ForAll(schema.Declares)
            && Unique(schema.Columns.Map(static column => column.Name)) && Unique(schema.Key)
            ? Fin.Succ(schema)
            : Fin.Fail<AnalyticsSchema>(new ResidenceFault.Unprovisioned($"<schema-columns:{schema.Dataset}>"));
    }

    static bool Unique(Seq<Identifier> names) => names.Distinct().Count == names.Count;

    static Fin<ColumnRow> Column((string Name, string Type, bool Nullable) row) =>
        (Trusted(row.Name), Admitted(row.Type)).Apply((name, type) => new ColumnRow(name, type, row.Nullable)).As();

    static Fin<Identifier> Trusted(string raw) =>
        Identifier.Validate(raw, null, out Identifier admitted) is { } fault ? Fin.Fail<Identifier>(fault) : Fin.Succ(admitted);

    // Producer type tokens carry the COMPOSITE grammar the wide-event seam needs — `list<utf8>`,
    // `map<utf8,float64>`, `dict<utf8>` — over the scalar roster, so an OTLP attribute map, a span-event run,
    // and a low-cardinality dimension all arrive as text a producer writes and become shape exactly here. A
    // map key is a scalar token by construction and a scalar key carries no comma, so the split is the FIRST
    // comma and needs no depth scan; the value recurses, which is what admits `map<utf8,list<utf8>>` whole.
    static Fin<ColumnShape> Admitted(string token) =>
        Wrapped(token, "list<") is { } element ? Admitted(element).Map(static shape => (ColumnShape)new ColumnShape.List(shape))
        : Wrapped(token, "dict<") is { } encoded ? Scalar(encoded).Map(static type => (ColumnShape)new ColumnShape.Dictionary(type))
        : Wrapped(token, "map<") is { } body ? Pair(body)
        : Scalar(token).Map(static type => (ColumnShape)type);

    static string? Wrapped(string token, string opener) =>
        token.StartsWith(opener, StringComparison.Ordinal) && token.EndsWith('>') ? token[opener.Length..^1] : null;

    static Fin<ColumnShape> Pair(string body) =>
        body.IndexOf(',', StringComparison.Ordinal) is int cut && cut > 0
            ? (Scalar(body[..cut]), Admitted(body[(cut + 1)..]))
                .Apply(static (key, value) => (ColumnShape)new ColumnShape.Map(key, value)).As()
            : Fin.Fail<ColumnShape>(new ResidenceFault.Unprovisioned($"<column-type:map<{body}>>"));

    static Fin<ColumnType> Scalar(string token) =>
        ColumnType.Validate(token, null, out ColumnType? type) is { } fault ? Fin.Fail<ColumnType>(fault) : Fin.Succ(type!);
}
```

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
// ONE Substrait lowering for every residence: the plan is the query currency and the residence row supplies the
// tokens, so a question written once renders three ways and no second query language enters. `Visit` is the
// guarded entry every recursion re-enters, so an unadmitted relation returns the typed refusal instead of
// reaching an unoverridden base arm and throwing out of the fold.
// Read SCOPE, distinct from read SHAPE: the tenant arrives on the frame and the window arrives here, so a
// Substrait plan carries filters, projections, and folds alone and never the two coordinates every residence
// prunes on — which is what makes an unbounded or cross-tenant residence scan unrepresentable rather than
// merely discouraged.
public readonly record struct ResidenceWindow(Instant From, Instant Until);

// ONE read scope every entry on this cluster takes: residence, schema, window, and frame always travel
// together, so they travel as one value rather than as four parameters each call site re-threads and each new
// coordinate widens at every signature. The lowering reads it as its visitor state and the read reads it as
// its scope, which is what keeps `Lower` and `Read` from drifting on which four things a scan is bounded by.
public sealed record ResidenceScope(Residence Residence, AnalyticsSchema Schema, ResidenceWindow Window, ProjectionContext Frame) {
    // Field references arrive as ORDINALS a foreign plan carries, so resolution is fallible by construction:
    // an ordinal past the roster refuses typed here, where an index into the column list throws straight out
    // of the `Fin` fold and turns a lowering the rail exists to explain into an unhandled exception.
    public Fin<string> Column(int ordinal) =>
        ordinal >= 0 && ordinal < Schema.Columns.Count
            ? Fin.Succ(Residence.Quote(Schema.Columns[ordinal].Name))
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(Residence.Key, $"<field-ordinal:{ordinal}>"));

    // Tenant predicate beside the half-open window, both in the residence's own literal dialect: the leading
    // sort-key column prunes to one tenant's granules and the trailing time column prunes the window, so the
    // two coordinates the physical layout exists to serve are the two the scan always carries.
    public string Scope =>
        $"{Residence.Partition(Frame.Tenant)} AND {Residence.Quote(Schema.Time)} >= {Residence.Moment(Window.From)}"
        + $" AND {Residence.Quote(Schema.Time)} < {Residence.Moment(Window.Until)}";
}

public sealed class ResidencePlan : RelationVisitor<Fin<string>, ResidenceScope> {
    // Comparison and arithmetic extension names are consts on the shipped catalogs, so a renamed upstream
    // function breaks the build rather than silently lowering to a spelling no backend resolves.
    static readonly FrozenDictionary<string, string> Operators =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsComparison.Equal] = "=", [FunctionsComparison.NotEqual] = "<>",
            [FunctionsComparison.GreaterThan] = ">", [FunctionsComparison.GreaterThanOrEqual] = ">=",
            [FunctionsComparison.LessThan] = "<", [FunctionsComparison.LessThanOrEqual] = "<=",
            [FunctionsArithmetic.Add] = "+", [FunctionsArithmetic.Subtract] = "-",
            [FunctionsArithmetic.Multiply] = "*", [FunctionsArithmetic.Divide] = "/",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    static readonly FrozenDictionary<string, string> Folds =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsArithmetic.Sum] = "sum", [FunctionsArithmetic.Min] = "min",
            [FunctionsArithmetic.Max] = "max", [FunctionsArithmetic.Average] = "avg",
            [FunctionsAggregateGeneric.Count] = "count",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Guarded entry: every arm recurses through `Visit`, never `Accept`, so the admitted-relation test runs
    // once per node and an unadmitted kind never reaches the base arm that throws.
    public override Fin<string> Visit(Relation relation, ResidenceScope state) =>
        relation is ReadRelation or FilterRelation or ProjectRelation or AggregateRelation or SortRelation or FetchRelation or TopNRelation or RootRelation
            ? base.Visit(relation, state)
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, relation.GetType().Name));

    // Reads are tenant-scoped and window-bounded: this arm takes both coordinates off the frame, so no filter
    // is ever the only thing separating tenants, no granule holding another tenant reads, and a plan missing
    // its window cannot fall through to a full-history scan.
    public override Fin<string> VisitReadRelation(ReadRelation readRelation, ResidenceScope state) =>
        Fin.Succ($"SELECT * FROM {state.Residence.Quote(state.Schema.Table)} WHERE {state.Scope}");

    public override Fin<string> VisitFilterRelation(FilterRelation filterRelation, ResidenceScope state) =>
        from inner in Visit(filterRelation.Input, state)
        from where in Predicate(filterRelation.Condition, state)
        select $"SELECT * FROM ({inner}) AS leg WHERE {where}";

    public override Fin<string> VisitProjectRelation(ProjectRelation projectRelation, ResidenceScope state) =>
        from inner in Visit(projectRelation.Input, state)
        from columns in toSeq(projectRelation.Expressions).TraverseM(expression => Predicate(expression, state)).As()
        select $"SELECT {string.Join(", ", columns)} FROM ({inner}) AS leg";

    // Grouping keys thread DOWN into the fold, so a windowed aggregate re-buckets at the caller's grain
    // rather than silently answering the residence's own storage grain.
    public override Fin<string> VisitAggregateRelation(AggregateRelation aggregateRelation, ResidenceScope state) =>
        from inner in Visit(aggregateRelation.Input, state)
        from keys in toSeq(aggregateRelation.Groupings ?? []).Bind(static grouping => toSeq(grouping.GroupingExpressions))
            .TraverseM(expression => Predicate(expression, state)).As()
        from folds in toSeq(aggregateRelation.Measures ?? []).TraverseM(measure => Fold(measure, state)).As()
        select keys.IsEmpty
            ? $"SELECT {string.Join(", ", folds)} FROM ({inner}) AS leg"
            : $"SELECT {string.Join(", ", keys + folds)} FROM ({inner}) AS leg GROUP BY {string.Join(", ", keys)}";

    public override Fin<string> VisitSortRelation(SortRelation sortRelation, ResidenceScope state) =>
        from inner in Visit(sortRelation.Input, state)
        from order in toSeq(sortRelation.Sorts).TraverseM(field => Ordered(field, state)).As()
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)}";

    public override Fin<string> VisitFetchRelation(FetchRelation fetchRelation, ResidenceScope state) =>
        from inner in Visit(fetchRelation.Input, state)
        from bound in Bounded(fetchRelation.Count, fetchRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg {bound}";

    public override Fin<string> VisitTopNRelation(TopNRelation topNRelation, ResidenceScope state) =>
        from inner in Visit(topNRelation.Input, state)
        from order in toSeq(topNRelation.Sorts).TraverseM(field => Ordered(field, state)).As()
        from bound in Bounded(topNRelation.Count, topNRelation.Offset, state)
        select $"SELECT * FROM ({inner}) AS leg ORDER BY {string.Join(", ", order)} {bound}";

    // ONE bound fragment both fetch-shaped arms render, so the row limit is proven once rather than
    // interpolated twice: a negative count or offset is a plan every dialect answers differently — PostgreSQL
    // and ClickHouse raise, DuckDB coerces — so it refuses typed at lowering rather than reaching an engine.
    static Fin<string> Bounded(int count, int offset, ResidenceScope state) =>
        count >= 0 && offset >= 0
            ? Fin.Succ($"LIMIT {count} OFFSET {offset}")
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, $"<fetch-bounds:{count}:{offset}>"));

    // Root relations name the output columns, so the caller's shape reader binds by ordinal against a projection the
    // plan declared rather than against whatever the innermost leg happened to emit.
    public override Fin<string> VisitRootRelation(RootRelation rootRelation, ResidenceScope state) =>
        from inner in Visit(rootRelation.Input, state)
        from names in toSeq(rootRelation.Names).TraverseM(name =>
            Identifier.Validate(name, null, out Identifier admitted) is { } fault
                ? Fin.Fail<string>(fault)
                : Fin.Succ(state.Residence.Quote(admitted))).As()
        select $"SELECT {string.Join(", ", names)} FROM ({inner}) AS root";

    // Expression fold: a field reference resolves through the admitted schema by ordinal, a literal renders
    // in its own invariant form, and a function resolves through the operator table — an unmapped extension
    // name is a typed refusal, never a spelled fallback the backend rejects at parse time.
    static Fin<string> Predicate(Expression expression, ResidenceScope state) => expression switch {
        DirectFieldReference { ReferenceSegment: StructReferenceSegment segment } =>
            state.Column(segment.Field),
        NumericLiteral literal => Fin.Succ(literal.Value.ToString(CultureInfo.InvariantCulture)),
        StringLiteral literal => Fin.Succ($"'{literal.Value.Replace("'", "''", StringComparison.Ordinal)}'"),
        BoolLiteral literal => Fin.Succ(literal.Value ? "TRUE" : "FALSE"),
        ScalarFunction call when Operators.TryGetValue(call.ExtensionName, out string? glyph) =>
            call.Arguments.Count == 2
                ? toSeq(call.Arguments).TraverseM(argument => Predicate(argument, state)).As()
                    .Map(parts => $"({string.Join($" {glyph} ", parts)})")
                : Unarity(call, 2, state),
        ScalarFunction call when Postfixes.TryGetValue(call.ExtensionName, out string? postfix) =>
            call.Arguments.Count == 1
                ? Predicate(call.Arguments[0], state).Map(part => $"({part} {postfix})")
                : Unarity(call, 1, state),
        _ => Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, expression.GetType().Name)),
    };

    // Null tests are POSTFIX rows on the same table discipline the infix glyphs ride, so the two shapes are
    // two rosters and one arm each rather than a per-function arm accreting down the switch.
    static readonly FrozenDictionary<string, string> Postfixes =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            [FunctionsComparison.IsNull] = "IS NULL", [FunctionsComparison.IsNotNull] = "IS NOT NULL",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Arity is proven BEFORE any argument lowers, because the malformed shapes are silent otherwise: a
    // one-argument comparison renders a bare operand the engine parses as a column, and a zero-argument null
    // test indexes past its own list and throws straight out of the `Fin` fold the lowering exists to carry.
    static Fin<string> Unarity(ScalarFunction call, int expected, ResidenceScope state) =>
        Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key,
            $"<arity:{call.ExtensionName}:{call.Arguments.Count}:{expected}>"));

    // Aggregate folds carry the residence's own quantile spelling, so a percentile tile renders three ways
    // off one measure and no page re-derives an engine's quantile function.
    static Fin<string> Fold(AggregateMeasure measure, ResidenceScope state) =>
        Folds.TryGetValue(measure.Measure.ExtensionName, out string? verb)
            ? toSeq(measure.Measure.Arguments).TraverseM(argument => Predicate(argument, state)).As()
                .Map(parts => $"{verb}({string.Join(", ", parts)})")
            : Fin.Fail<string>(new ResidenceFault.Unlowerable(state.Residence.Key, measure.Measure.ExtensionName));

    static Fin<string> Ordered(SortField field, ResidenceScope state) =>
        Predicate(field.Expression, state).Map(part => field.SortDirection switch {
            SortDirection.SortDirectionDescNullsFirst or SortDirection.SortDirectionDescNullsLast => $"{part} DESC",
            _ => $"{part} ASC",
        });

    // ONE plan builder every dataset shares: callers name the equality narrowings their question carries as
    // TEXT and take back the rooted plan, so relation assembly lives here once instead of at each consuming
    // page and a page that would otherwise hand-write SQL reaches a typed shape. Scope stays off this plan —
    // tenant and window both enter at the read — so one plan serves every residence and every window.
    // Assembly is FALLIBLE because both failure modes are silent otherwise: an undeclared column ordinals to
    // -1 and indexes out of the roster at lowering, and a value rendered as text against a numeric column
    // raises on one dialect and coerces on another.
    public static Fin<Plan> Scan(AnalyticsSchema schema, Seq<(Identifier Column, string Value)> matches) =>
        matches.TraverseM(match => Narrowed(schema, match)).As().Map(conditions => {
            List<string> names = [.. schema.Columns.Map(static column => (string)column.Name)];
            Relation scan = conditions.Fold(
                (Relation)new ReadRelation {
                    NamedTable = new NamedTable { Names = [(string)schema.Table] },
                    BaseSchema = new NamedStruct { Names = names },
                },
                static (input, condition) => new FilterRelation { Input = input, Condition = condition });
            return new Plan { Relations = [new RootRelation { Input = scan, Names = names }] };
        });

    // One narrowing is one admitted column ordinal beside the literal that column's own declared type renders,
    // so a temporal or key-typed match — the two Substrait carries no literal for — refuses here rather than
    // lowering an operand three engines each reject differently.
    static Fin<Expression> Narrowed(AnalyticsSchema schema, (Identifier Column, string Value) match) =>
        schema.Ordinal(match.Column) is var ordinal && ordinal < 0
            ? Fin.Fail<Expression>(new ResidenceFault.Unprovisioned($"<schema-column:{schema.Dataset}.{match.Column}>"))
            : schema.Columns[ordinal].Type.Plan(match.Value).Match(
                Some: literal => Fin.Succ<Expression>(new ScalarFunction {
                    ExtensionUri = FunctionsComparison.Uri,
                    ExtensionName = FunctionsComparison.Equal,
                    Arguments = [
                        new DirectFieldReference { ReferenceSegment = new StructReferenceSegment { Field = ordinal } },
                        literal,
                    ],
                }),
                None: () => Fin.Fail<Expression>(new ResidenceFault.Unlowerable(
                    schema.Dataset, $"<literal:{schema.Columns[ordinal].Type.Key}:{match.Column}>")));

    // One lowering entry: the root relation of the plan folds under the scope's own residence dialect, a plan
    // naming a projection that residence does not answer refuses carrying the row's `Degrade` clause, and an
    // empty or inverted window refuses ahead of both — a half-open window whose end precedes its start returns
    // zero rows on every engine, which a tile reads as a healthy quiet period rather than an unspellable scope.
    public static Fin<string> Lower(Plan plan, ResidenceScope scope, ResidenceProjection projection) =>
        scope.Window.Until <= scope.Window.From
            ? Fin.Fail<string>(new ResidenceFault.ReadRefused(scope.Residence.Key, new EngineFault("<read-window>", $"{scope.Window.From}..{scope.Window.Until}")))
            : !scope.Residence.Answers(projection)
                ? Fin.Fail<string>(new ResidenceFault.Unanswerable(scope.Residence.Key, projection.Key, scope.Residence.Degrade))
                : toSeq(plan.Relations).Last.Match(
                    Some: root => new ResidencePlan().Visit(root, scope),
                    None: () => Fin.Fail<string>(new ResidenceFault.Unlowerable(scope.Residence.Key, "<empty-plan>")));
}
```

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
// Parameterized provisioning: the WHOLE statement set derives from the residence row and the admitted schema,
// so no environment hand-spells a script and no exporter creates a table. Every step is idempotent, so the
// reviewed-migration artifact re-applies without a guard ladder.
public static class ResidenceDdl {
    public static Seq<string> Provision(Residence residence, AnalyticsSchema schema, ResidencePolicy policy) =>
        residence.Statements(schema, policy);

    // Column list is one projection both the Series and Fleet arms compose, so a nullability or type edit
    // lands once and the two dialects cannot drift on the same schema. A sort-key residence LEADS with this
    // custodian's own tenant column at the key type every tenancy predicate compares against, while a prefix
    // residence declares none, its hive directory being where that byte already rests. The admitted roster
    // carries no tenant column of its own — `AnalyticsSeam.Spined` dropped it — so the lead is unconditional.
    internal static string Columns(Residence residence, AnalyticsSchema schema) =>
        string.Join(", ", (residence.Tenancy.Switch(
                sortKey: () => Seq(new ColumnRow(Residence.TenantColumn, ColumnType.KeyHex, false)),
                prefix:  () => Seq<ColumnRow>())
            + schema.Columns)
            .Map(column => $"{residence.Quote(column.Name)} {residence.Render(column.Type)}{(column.Nullable ? string.Empty : " NOT NULL")}"));

    // Sort key is tenant-first and time-last by construction: the leading column prunes a single-tenant read
    // to its own granules and the trailing one prunes a window, so a schema whose key omits its time column
    // still orders on it rather than forcing every range scan through the whole partition. The leading column
    // reads the same tenancy the roster does, so a residence carrying no stored tenant column never sorts on
    // one it does not declare.
    internal static string Keys(Residence residence, AnalyticsSchema schema) =>
        string.Join(", ", (residence.Tenancy.Switch(sortKey: () => Seq(Residence.TenantColumn), prefix: () => Seq<Identifier>())
            + schema.Key
            + (schema.Key.Contains(schema.Time) ? Seq<Identifier>() : Seq(schema.Time))).Map(residence.Quote));
}

// Series residence: the relational hypertable tier. Provisioning splits SELECT functions from CALL procedures
// per the emission law, so a mis-verbed row is unrepresentable from the derivation, and the continuous
// aggregate is what makes a board tile a bucket read rather than a raw-chunk re-scan. Every statement reads
// its schema's OWN spine, so a measure-free wide-event dataset provisions hypertable, columnstore, and
// retention and emits no rollup — a fabricated `avg` over a column that dataset never declared is the form
// this derivation deletes.
public static class SeriesResidence {
    public static Seq<string> Statements(AnalyticsSchema schema, ResidencePolicy policy) {
        // Relation arguments are REGCLASS text and parse their own quoting, while a column argument and every
        // storage-parameter entry are attname TEXT compared verbatim — so the relation carries the quoted
        // spelling its own `CREATE` used and a column never does.
        string table = Residence.Series.Quote(schema.Table);
        string rollup = Residence.Series.Quote(Rollup(schema));
        string at = (string)schema.Time;
        // Grouping preserves IDENTITY and segmenting preserves COMPRESSION, and they are different lists: the
        // rollup groups the whole key so each stream keeps its own buckets, while the columnstore segments the
        // bounded text keys alone. Both lead with the tenant, so a keyless dataset —
        // a stream whose whole identity is its tenant and its instant — still emits a well-formed list.
        string grouping = Names(Seq(Residence.TenantColumn) + schema.Sorted.Map(static column => column.Name));
        string segments = Names(Seq(Residence.TenantColumn) + Bounded(schema).Map(static column => column.Name));
        // Time trails the order list exactly once: a dataset naming its instant IN the key would otherwise
        // repeat it, and a duplicate `orderby` entry is a storage-parameter the engine rejects outright.
        string ordering = Names(Unbounded(schema).Map(static column => column.Name).Filter(name => name != schema.Time) + Seq(schema.Time));
        string grain = Residence.Interval(policy.Grain);
        return Seq(
            $"CREATE TABLE IF NOT EXISTS {table} ({ResidenceDdl.Columns(Residence.Series, schema)})",
            $"SELECT create_hypertable('{table}', by_range('{at}', INTERVAL '{Residence.Interval(policy.Chunk)}'), if_not_exists => TRUE)",
            $"ALTER TABLE {table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = '{segments}', timescaledb.orderby = '{ordering}')",
            $"CALL add_columnstore_policy('{table}', after => INTERVAL '{Residence.Interval(policy.Chunk)}', if_not_exists => TRUE)",
            $"SELECT add_retention_policy('{table}', drop_after => INTERVAL '{Residence.Interval(policy.Retain)}', if_not_exists => TRUE)")
        + schema.Measure.ToSeq().Bind(value => Seq(
            $"CREATE MATERIALIZED VIEW IF NOT EXISTS {rollup} WITH (timescaledb.continuous) AS SELECT {grouping}, time_bucket(INTERVAL '{grain}', {at}) AS {Bucket}, time_weight('linear', {at}, {value}) AS {Weight}, percentile_agg({value}) AS {Sketch}, min({value}) AS {Low}, max({value}) AS {High}, count(*) AS {Samples} FROM {table} GROUP BY {grouping}, {Bucket} WITH NO DATA",
            $"ALTER MATERIALIZED VIEW {rollup} SET (timescaledb.enable_columnstore = true)",
            $"SELECT add_continuous_aggregate_policy('{rollup}', start_offset => INTERVAL '{Residence.Interval(policy.Backfill)}', end_offset => INTERVAL '{grain}', schedule_interval => INTERVAL '{grain}', if_not_exists => TRUE)"));
    }

    // Cardinality reads off the DECLARED type, never off a hand roster: a `Utf8` key is a bounded facet a
    // filter equals on, and every other key type is identity a segment list must not carry.
    static Seq<ColumnRow> Bounded(AnalyticsSchema schema) => schema.Sorted.Filter(static column => column.Type.Bounded);
    static Seq<ColumnRow> Unbounded(AnalyticsSchema schema) => schema.Sorted.Filter(static column => !column.Type.Bounded);
    static string Names(Seq<Identifier> columns) => string.Join(", ", columns.Map(static column => (string)column));

    // Rollup shape is this arm's own declaration — the aggregate's name, its bucket column, and the two
    // SUMMARY columns beside the three scalars — so a rollup column add moves the view and its reader together
    // and no read site spells a `_rollup` suffix or a fold alias twice. The summaries are what make the tile
    // read and the raw-chunk read ONE statistic: a materialised `avg` answers a different question than the
    // `time_weight` mean it accelerates, so the aggregate stores state and `Projection` names the accessor.
    public static Identifier Rollup(AnalyticsSchema schema) => Identifier.Create($"{(string)schema.Table}_rollup");
    public static readonly Identifier Bucket = Identifier.Create("bucket");
    public static readonly Identifier Weight = Identifier.Create("weight");
    public static readonly Identifier Sketch = Identifier.Create("sketch");
    public static readonly Identifier Low = Identifier.Create("low");
    public static readonly Identifier High = Identifier.Create("high");
    public static readonly Identifier Samples = Identifier.Create("samples");

    // Read-time accessor projection over the materialised state, in the ordinal order `SeriesBucket` binds: the
    // quantile is a read argument rather than a second view, which is exactly the two-stage discipline the
    // toolkit's aggregate/accessor split exists for.
    public static string Projection(double quantile) =>
        $"average({Weight}), approx_percentile({quantile.ToString("0.####", CultureInfo.InvariantCulture)}, {Sketch}), {Low}, {High}, {Samples}";
}

// Fleet residence: the interactive wide-event tier. Tenant leads `ORDER BY` so a single-tenant filter prunes
// granules BEFORE the predicate applies — a default exporter schema leaving attributes in an unsorted `Map`
// scans granules holding every other tenant — and one bloom skip index per admitted text column outside the
// sort key prunes attribute-key existence before any value comparison. Partition and TTL expressions read the
// schema's own time column, so a dataset spelling its instant `time` provisions exactly as one spelling `at`.
public static class FleetResidence {
    public static Seq<string> Statements(AnalyticsSchema schema, ResidencePolicy policy) {
        string table = Residence.Fleet.Quote(schema.Table);
        string at = Residence.Fleet.Quote(schema.Time);
        return Seq(
            $"CREATE TABLE IF NOT EXISTS {table} ({ResidenceDdl.Columns(Residence.Fleet, schema)}) " +
            $"ENGINE = MergeTree PARTITION BY toYYYYMM({at}) ORDER BY ({ResidenceDdl.Keys(Residence.Fleet, schema)}) " +
            $"TTL toDateTime({at}) + INTERVAL {(long)policy.Retain.TotalSeconds} SECOND DELETE SETTINGS index_granularity = 8192, ttl_only_drop_parts = 1")
            + schema.Payload
                .Filter(static column => column.Type.Bounded)
                .Map(column =>
                    $"ALTER TABLE {table} ADD INDEX IF NOT EXISTS bloom_{column.Name} {Residence.Fleet.Quote(column.Name)} TYPE bloom_filter(0.01) GRANULARITY 1");
    }
}

// Lake residence: the cold tail. The hive tree IS the schema — a generation directory keyed by the producer's
// content key carries its own Parquet footer, so this arm creates no storage and declares no column type. It
// emits exactly ONE statement, the VIEW that gives the tree the NAME the shared plan lowering addresses:
// without it a lowered `SELECT * FROM "<table>"` names nothing on a DuckDB lane and the whole cold-tail reach
// resolves against a relation that was never going to exist. `union_by_name` makes an additive column
// compatible by construction and `hive_partitioning` projects the tenant directory back as the column the one
// tenancy predicate compares, so retention stays generation eviction on the object plane and no `DROP` lands.
public static class LakeResidence {
    public static Seq<string> Statements(AnalyticsSchema schema, ResidencePolicy policy) => Seq(
        $"CREATE OR REPLACE VIEW {Residence.Lake.Quote(schema.Table)} AS SELECT * FROM read_parquet('{(string)policy.Root}/**/*.parquet', hive_partitioning = true, union_by_name = true)");
}
```

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
// Transport union: the READ discriminates on the reach VALUE's shape, never on a residence name or a mode
// flag, so a residence reachable two ways (an in-process lake scan and a cross-runtime Flight SQL hop) needs
// no second entry and a new transport breaks the dispatch at compile time.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceReach {
    private ResidenceReach() { }
    public sealed record Relational(NpgsqlDataSource Source) : ResidenceReach;
    public sealed record Fleet(ClickHouseClient Client) : ResidenceReach;
    public sealed record Flight(FlightSqlClient Client) : ResidenceReach;
    public sealed record Local(ColumnarSession Session) : ResidenceReach;
}

// ONE row surface every reach yields, so the caller writes one shape and the relational and Arrow legs are
// genuinely interchangeable — a `DbDataReader`-typed shape would force the Flight leg to fake a reader it
// cannot supply, and a per-reach shape delegate would fork the one entry this cluster exists to hold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceRow {
    private ResidenceRow() { }
    public sealed record Ado(System.Data.Common.DbDataReader Reader) : ResidenceRow;
    public sealed record Arrow(RecordBatch Batch, int Ordinal) : ResidenceRow;

    // Absence is a RAIL fact on both arms: an Arrow primitive reads nullable by construction and a relational
    // column reads `IsDBNull` ahead of its typed getter, so the two legs answer one shape and neither invents a
    // value. A shape needing a total column takes the refusal rather than an empty string, a zero, or a 1970
    // instant — the three sentinels a board renders indistinguishably from a measured reading.
    public Fin<string> Text(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<string>.None : Some(c.Reader.GetString(column)),
        arrow: c => Optional(((StringArray)c.Batch.Column(column))[c.Ordinal]))
        .ToFin(Missing(residence, column));

    public Fin<long> Whole(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<long>.None : Some(c.Reader.GetInt64(column)),
        arrow: c => Optional(((Int64Array)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(residence, column));

    public Fin<double> Real(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column) ? Option<double>.None : Some(c.Reader.GetDouble(column)),
        arrow: c => Optional(((DoubleArray)c.Batch.Column(column)).GetValue(c.Ordinal)))
        .ToFin(Missing(residence, column));

    public Fin<Instant> At(Residence residence, int column) => Switch(
        ado:   c => c.Reader.IsDBNull(column)
            ? Option<Instant>.None
            : Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(c.Reader.GetDateTime(column), DateTimeKind.Utc))),
        arrow: c => Optional(((TimestampArray)c.Batch.Column(column))[c.Ordinal]).Map(Instant.FromDateTimeOffset))
        .ToFin(Missing(residence, column));

    // One refusal spelling every reader shares, carrying the residence that answered and the ordinal that came
    // back empty, so a corrupt total column names itself instead of surfacing as a downstream parse failure.
    static ResidenceFault Missing(Residence residence, int column) =>
        new ResidenceFault.ReadRefused(residence.Key,
            new EngineFault("<null-column>", column.ToString(CultureInfo.InvariantCulture)));
}

// WRITE counterpart of the one row surface: an arm per `ColumnType` row, so the value a producer hands and the
// physical type its column declared are two facts one gate proves rather than two dispatches that disagree
// mid-copy. The importer infers NOTHING from a column list — a mismatch discovered at row n has already staged
// n-1 rows the importer then discards whole — so the proof runs before the copy opens.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResidenceCell {
    private ResidenceCell() { }
    public sealed record Text(string Value) : ResidenceCell;
    public sealed record Real(double Value) : ResidenceCell;
    public sealed record Whole(long Value) : ResidenceCell;
    public sealed record Flag(bool Value) : ResidenceCell;
    public sealed record Day(LocalDate Value) : ResidenceCell;
    public sealed record Moment(Instant Value) : ResidenceCell;
    public sealed record Key(UInt128 Value) : ResidenceCell;
    // Composite arms carry the wide-event shapes: `Items` a homogeneous run and `Tags` the attribute map every
    // OTLP record hands over. Both declare their ELEMENT type, so the arm proves against the column's declared
    // shape exactly as a scalar arm does and a heterogeneous bag has no cell to arrive in.
    public sealed record Items(ColumnType Element, Seq<string> Values) : ResidenceCell;
    public sealed record Tags(ColumnType Element, Seq<(string Key, string Value)> Pairs) : ResidenceCell;

    // Each arm NAMES the shape it fills, so conformance costs one comparison per column rather than a shape
    // match repeated at every write site, and a new `ColumnShape` case breaks this fold at compile time.
    public ColumnShape Column => Switch(
        text:   static _ => ColumnType.Utf8,
        real:   static _ => ColumnType.Float64,
        whole:  static _ => ColumnType.Int64,
        flag:   static _ => ColumnType.Bool,
        day:    static _ => ColumnType.Date,
        moment: static _ => ColumnType.Timestamp,
        key:    static _ => ColumnType.KeyHex,
        items:  static c => new ColumnShape.List(c.Element),
        tags:   static c => new ColumnShape.Map(ColumnType.Utf8, c.Element));

    // Composite arms carry their values as TEXT, so their element type is `Utf8` by construction: a
    // `list<float64>` column conforms on shape and then binds a string run under a numeric array wire type,
    // and a repeated attribute key raises out of the map serializer — both at row n with n-1 rows already
    // staged inside an open copy. Each is a WRITABILITY fact of the cell, so it proves at the conformance
    // gate beside arity and type, where the residence that cannot write the shape names it.
    public Fin<Unit> Canonical => Switch(
        text:   static _ => Fin.Succ(unit),
        real:   static _ => Fin.Succ(unit),
        whole:  static _ => Fin.Succ(unit),
        flag:   static _ => Fin.Succ(unit),
        day:    static _ => Fin.Succ(unit),
        moment: static _ => Fin.Succ(unit),
        key:    static _ => Fin.Succ(unit),
        items:  static c => c.Element == ColumnType.Utf8
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, $"<list-element:{c.Element.Key}>")),
        tags:   static c => c.Element != ColumnType.Utf8
            ? Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, $"<map-value:{c.Element.Key}>"))
            : c.Pairs.Map(static pair => pair.Key).Distinct().Count == c.Pairs.Count
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ResidenceFault.Unwritable(Residence.Series.Key, "<map-duplicate-key>")));

    // Big-endian 16-byte pack of a key scalar: the tenant reaches it through `TenantId.Value` and a series
    // through its own `UInt128`, so ONE encoder serves every `KeyHex` column on every landing and `SeriesLane`'s
    // read inverse decodes exactly what this staged.
    public static byte[] Packed(UInt128 key) {
        byte[] bytes = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, key);
        return bytes;
    }

    // Wire type arrives from the COLUMN's own row while the arm supplies its value at CLR type, so one write
    // body serves every dialect spelling and no landing re-spells a physical type beside its declaration.
    public Task Stage(NpgsqlBinaryImporter importer, NpgsqlDbType wire) => Switch(
        text:   cell => importer.WriteAsync(cell.Value, wire),
        real:   cell => importer.WriteAsync(cell.Value, wire),
        whole:  cell => importer.WriteAsync(cell.Value, wire),
        flag:   cell => importer.WriteAsync(cell.Value, wire),
        day:    cell => importer.WriteAsync(cell.Value, wire),
        moment: cell => importer.WriteAsync(cell.Value, wire),
        key:    cell => importer.WriteAsync(Packed(cell.Value), wire),
        // Series renders a list as its element's array and a map as `jsonb`, so the array arm writes the
        // value run under the flagged wire type and the map arm writes the ONE canonical JSON text — the
        // pairs serialize here rather than at a producer, so every landing spells one document shape, and the
        // key roster reached this loop already proven distinct so the document build is total.
        items:  cell => importer.WriteAsync(cell.Values.ToArray(), wire),
        tags:   cell => importer.WriteAsync(
            JsonSerializer.Serialize(cell.Pairs.ToDictionary(static pair => pair.Key, static pair => pair.Value)), wire));
}

// Every read lands its own receipt: the residence answered, the text that ran, and the scanned magnitude —
// so a slow tile names the residence and the lowered plan rather than a bare elapsed figure. `Receipt` is the
// NON-GENERIC projection the `store.columnar.residence.read` slot carries, so the rows never cross the receipt
// wire and a consumer arrow handing back bare values loses no diagnosis — the read's own evidence lands at its
// owner rather than dying at the boundary that only wanted the payload.
public readonly record struct ResidenceReceipt(string Residence, string Lowered, long Scanned, Duration Elapsed);

public readonly record struct ResidenceResult<T>(Residence Residence, string Lowered, Seq<T> Rows, long Scanned, Duration Elapsed) {
    public ResidenceReceipt Receipt => new(Residence.Key, Lowered, Scanned, Elapsed);
}

// FAMILY health row: the resident time extent and cardinality of one residence's relation, the one evidence
// every tier produces because every tier partitions on time. `Retained` reads the expiry scheduler's OUTCOME —
// residue older than the declared horizon means the policy stopped firing, whichever engine owns it — and
// `Lag` reads the refresh's, so a stalled rollup surfaces as measured staleness rather than as an empty tile a
// board renders indistinguishably from a quiet stream. An empty relation answers absence at both ends, because
// a zero instant reads as 1970 and would report every quiet stream as catastrophically stale.
public readonly record struct ResidenceHealth(string Residence, string Relation, Option<Instant> Oldest, Option<Instant> Newest, long Rows) {
    public bool Retained(ResidencePolicy policy, Instant now) => Oldest.Map(at => at >= now - policy.Retain).IfNone(true);
    public Duration Lag(Instant now) => Newest.Map(at => now - at).IfNone(Duration.Zero);
}

public static class ResidenceRead {
    // Policy health rides the SAME four reach arms every read takes, so the family gains its probe as one
    // statement and one row shape rather than a second transport ladder per residence.
    public static IO<Fin<ResidenceResult<ResidenceHealth>>> Health(ResidenceReach reach, Residence residence, AnalyticsSchema schema) {
        string probe = residence.Horizon(schema);
        // Row count gates the extent pair, because an empty relation answers absence at both ends and reads its
        // extent columns not at all — two readers instead refuse on the very row that legitimately carries none.
        Fin<ResidenceHealth> Shape(ResidenceRow row) =>
            row.Whole(residence, 2).Bind(rows => rows == 0
                ? Fin.Succ(new ResidenceHealth(residence.Key, (string)schema.Table, None, None, rows))
                : (row.At(residence, 0), row.At(residence, 1))
                    .Apply((oldest, newest) => new ResidenceHealth(
                        residence.Key, (string)schema.Table, Some(oldest), Some(newest), rows)).As());
        return reach.Switch(
            relational: leg => Relational(leg, residence, probe, Shape),
            fleet:      leg => Fleet(leg, residence, probe, Shape),
            flight:     leg => Flight(leg, residence, probe, Shape),
            local:      leg => Local(leg, residence, probe, Shape));
    }

    // ONE query entry across every residence: the logical plan lowers once through the residence's dialect
    // and the reach value alone decides the transport. A caller-supplied SQL string has no parameter to
    // arrive on, which is what makes writer/reader drift and ad-hoc tenant scans unrepresentable.
    public static IO<Fin<ResidenceResult<T>>> Read<T>(
        ResidenceReach reach, Plan plan, ResidenceScope scope, ResidenceProjection projection, Func<ResidenceRow, Fin<T>> shape) =>
        ResidencePlan.Lower(plan, scope, projection).Match(
            Succ: lowered => reach.Switch(
                relational: leg => Relational(leg, scope.Residence, lowered, shape),
                fleet:      leg => Fleet(leg, scope.Residence, lowered, shape),
                flight:     leg => Flight(leg, scope.Residence, lowered, shape),
                local:      leg => Local(leg, scope.Residence, lowered, shape)),
            Fail: error => IO.pure(Fin<ResidenceResult<T>>.Fail(error)));

    // `NpgsqlException` is the driver's whole failure surface and `PostgresException` its server-reported
    // sealed leaf, so the catch names the base: a socket drop, a TLS refusal, or a pool timeout is a read
    // refusal exactly as a SQLSTATE is, and the row's own `Diagnose` type-test renders each without a cast.
    static IO<Fin<ResidenceResult<T>>> Relational<T>(ResidenceReach.Relational leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape) =>
        IO.liftAsync(async () => {
            Stopwatch clock = Stopwatch.StartNew();
            await using NpgsqlCommand command = leg.Source.CreateCommand(lowered);
            try {
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                return (await Drain(reader, shape).ConfigureAwait(false))
                    .Map(rows => new ResidenceResult<T>(residence, lowered, rows, rows.Count, Duration.FromTimeSpan(clock.Elapsed)));
            }
            catch (NpgsqlException wire) { return Fin<ResidenceResult<T>>.Fail(residence.ReadRefused(wire)); }
        });

    // `QueryStats` is the ONLY honest scanned figure on this leg — the returned row count says nothing about the
    // granules a predicate pruned, which is the whole reason the tenant leads the sort key.
    static IO<Fin<ResidenceResult<T>>> Fleet<T>(ResidenceReach.Fleet leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape) =>
        IO.liftAsync(async () => {
            await using ClickHouseConnection lane = leg.Client.CreateConnection();
            await lane.OpenAsync().ConfigureAwait(false);
            // This leg rides the ADO mirror rather than the pooled client's own reader entry: `QueryStats`
            // is a post-execution property on the COMMAND and `DbConnection.CreateCommand` takes no text, so
            // each command binds its connection at construction and carries the lowered plan as its state.
            await using ClickHouseCommand command = new(lane) { CommandText = lowered };
            await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            Fin<Seq<T>> rows = await Drain(reader, shape).ConfigureAwait(false);
            QueryStats stats = command.QueryStats!;
            return rows.Map(held => new ResidenceResult<T>(residence, lowered, held, (long)stats.ReadRows, Duration.FromNanoseconds(stats.ElapsedNs)));
        }) | @catch<IO, Fin<ResidenceResult<T>>>(static e => e.Exception.Map(static x => x is ClickHouseServerException).IfNone(false),
            e => IO.pure(Fin<ResidenceResult<T>>.Fail(residence.ReadRefused(e.ToException()))));

    // Flight is the ONE cross-runtime columnar query plane, Flight SQL the dialect layered on it: the lowered text executes server-side, the
    // returned `FlightInfo` carries one endpoint per partition, and every endpoint's ticket streams Arrow
    // batches back on the same plane. Rows project through the schema's own ordinals, so a Flight consumer
    // and an in-process consumer read one column order.
    static IO<Fin<ResidenceResult<T>>> Flight<T>(ResidenceReach.Flight leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape) =>
        IO.liftAsync(async () => {
            Stopwatch clock = Stopwatch.StartNew();
            FlightInfo info = await leg.Client.ExecuteAsync(lowered, Transaction.NoTransaction).ConfigureAwait(false);
            Fin<Seq<T>> rows = Fin.Succ(Seq<T>());
            // One endpoint per server-side partition, each redeemed on the same plane — a single-endpoint
            // read and a partitioned read are one loop, so a residence that later partitions needs no arm.
            foreach (FlightEndpoint endpoint in info.Endpoints) {
                await foreach (RecordBatch batch in leg.Client.DoGetAsync(endpoint.Ticket).ConfigureAwait(false)) {
                    rows = rows.Bind(held => toSeq(Enumerable.Range(0, batch.Length))
                        .TraverseM(ordinal => shape(new ResidenceRow.Arrow(batch, ordinal))).As()
                        .Map(batched => held + batched));
                }
            }
            return rows.Map(held => new ResidenceResult<T>(residence, lowered, held, info.TotalRecords, Duration.FromTimeSpan(clock.Elapsed)));
        }) | @catch<IO, Fin<ResidenceResult<T>>>(static error => error.IsExceptional,
            error => IO.pure(Fin<ResidenceResult<T>>.Fail(new ResidenceFault.ReadRefused(residence.Key, new EngineFault("<flight>", error.Message)))));

    // Lake reads run in-process over the hive generation tree through the standing DuckDB anchor, so a
    // report-grade scan needs no Flight hop and the same lowered text serves both reaches.
    static IO<Fin<ResidenceResult<T>>> Local<T>(ResidenceReach.Local leg, Residence residence, string lowered, Func<ResidenceRow, Fin<T>> shape) =>
        IO.liftAsync(async () => {
            Stopwatch clock = Stopwatch.StartNew();
            await using DuckDBConnection lane = leg.Session.Lane();
            await using DuckDBCommand command = lane.CreateCommand();
            (command.CommandText, command.UseStreamingMode) = (lowered, true);
            await using DuckDBDataReader reader = (DuckDBDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
            return (await Drain(reader, shape).ConfigureAwait(false))
                .Map(rows => new ResidenceResult<T>(residence, lowered, rows, rows.Count, Duration.FromTimeSpan(clock.Elapsed)));
        }) | @catch<IO, Fin<ResidenceResult<T>>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<ResidenceResult<T>>.Fail(residence.ReadRefused(e.ToException()))));

    // One drain every ADO-shaped reach shares; the row wrapper is what lets the Arrow leg reuse the caller's
    // one shape without a second delegate or a fabricated reader.
    // `Drain` aborts on the FIRST refusing row and returns that fault, so a corrupt column stops the read where
    // it happened rather than yielding a partially fabricated result set the caller cannot tell apart.
    static async ValueTask<Fin<Seq<T>>> Drain<T>(System.Data.Common.DbDataReader reader, Func<ResidenceRow, Fin<T>> shape) {
        List<T> rows = [];
        ResidenceRow row = new ResidenceRow.Ado(reader);
        while (await reader.ReadAsync().ConfigureAwait(false)) {
            Fin<T> shaped = shape(row);
            if (shaped.Case is not T held) return shaped.Map(static _ => Seq<T>());
            rows.Add(held);
        }
        return Fin.Succ(toSeq(rows));
    }
}

// Staged-count receipt the `store.columnar.residence.ingest` slot carries: `Dataset` names the residence
// dataset a batch landed under, so the projection arm reads a DECLARED wire shape rather than a count with no
// subject — an ingest stream that stopped feeding is invisible when the receipt is a bare number.
public readonly record struct ResidenceIngestReceipt(string Dataset, long Staged, Instant At, CorrelationId Correlation);

// ONE landing across every relational residence dataset, the WRITE peer of `ResidenceRead`. The COPY column
// list, the tenancy lead, and each column's wire type all derive from the SAME `AnalyticsSchema` the DDL
// emitter provisions from, so a landed row and the table it lands in cannot drift on order, count, or physical
// type — and a producer-declared dataset is FED rather than provisioned, readable, and empty. This is the
// Series tier's declared landing owner: `SeriesLane.Ingest` is its hypertable-family arm and carries no second
// copy loop, while the Fleet tier lands through `Version/egress`'s sink and the Lake tier through
// `#FLAT_TABLE_EGRESS`'s generation, so each residence keeps exactly one writer. Binary COPY is the lane —
// `CompleteAsync` commits and disposal without it discards, so the retry unit is the whole batch and a
// refusal leaves nothing half-written.
public static class ResidenceLanding {
    // Payload roster IS the admitted roster, so the ingest column list and the CREATE column list are one
    // derivation off one declaration — the seam already dropped the custodian's own tenant column, so neither
    // side carries a second column at a second physical type the sort key would then split on.
    public static Seq<ColumnRow> Payload(AnalyticsSchema schema) => schema.Columns;

    // Roster a PRODUCER fills, which is `Payload` minus every column the custodian stamps. A landing-time
    // dataset's instant is the custodian's BY CATEGORY — that ownership is what the category means — so no
    // producer cell answers it and the arity gate proves against this narrower roster. Proving against
    // `Payload` demands a cell for the very column the category forbids a producer to carry, so every correct
    // landing-time producer arrives one cell short and refuses; the gate stays exact, it just stops counting
    // a column the contract never asked for.
    public static Seq<ColumnRow> Supplied(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Landing
            ? Payload(schema).Filter(column => column.Name != schema.Time)
            : Payload(schema);

    // Custodian-stamped trailing column, resolved AHEAD of the copy exactly as every supplied column's wire
    // is, so the write loop stays total. `Admit` APPENDS the landing column, so it is the tail of `Payload`
    // and `tenant` + supplied + landed reconstructs the provisioned order byte-for-byte.
    static Fin<Option<(Identifier Name, NpgsqlDbType Wire)>> Landed(AnalyticsSchema schema) =>
        schema.Spine == TimeSpine.Event
            ? Fin.Succ(Option<(Identifier, NpgsqlDbType)>.None)
            : schema.Columns.Find(column => column.Name == schema.Time).Match(
                Some: column => column.Type.Wire.Map(wire => Some((schema.Time, wire))),
                None: () => Fin.Fail<Option<(Identifier, NpgsqlDbType)>>(
                    new ResidenceFault.Unprovisioned($"<schema-spine:{schema.Dataset}>")));

    public static IO<Fin<ResidenceIngestReceipt>> Stage(
        NpgsqlDataSource store, AnalyticsSchema schema, Seq<Seq<ResidenceCell>> rows, ProjectionContext frame) =>
        (Conformed(schema, Supplied(schema), rows), Landed(schema))
            .Apply(static (bound, landed) => (Bound: bound, Landed: landed)).As()
            .Match(
            Succ: proved => IO.liftAsync(async () => {
                string columns = string.Join(", ",
                    (Seq(Residence.TenantColumn) + proved.Bound.Map(static entry => entry.Column.Name)
                        + proved.Landed.Map(static stamp => stamp.Name).ToSeq()).Map(Residence.Series.Quote));
                await using NpgsqlConnection lane = await store.OpenConnectionAsync().ConfigureAwait(false);
                // Scoped disposal releases the copy exactly once on every outcome — commit, typed refusal,
                // cancellation, or a conversion the gate could not foresee — and an uncompleted importer
                // discards its staged rows on the way out; a per-branch dispose repeats that release on the
                // paths it remembers and leaks the copy open on the ones it does not.
                await using NpgsqlBinaryImporter importer = await lane.BeginBinaryImportAsync(
                    $"COPY {Residence.Series.Quote(schema.Table)} ({columns}) FROM STDIN (FORMAT BINARY)").ConfigureAwait(false);
                try {
                    // Tenancy is the FRAME's, never a row column: the whole batch lands under the ingesting
                    // tenant and every read filters by it, so equal keys under distinct tenants never share rows.
                    byte[] tenant = ResidenceCell.Packed(frame.Tenant.TenantId.Value);
                    // Landing instants ride the frame on the SAME terms: one batch carries one admission
                    // moment, so a landing-time fact dates to when this custodian took it. Reading the clock
                    // once per batch rather than once per row keeps a single COPY internally consistent, and
                    // this stamp binds through the same `ResidenceCell` arm every supplied cell binds through,
                    // so a spine type change moves one declaration and no write re-spells a physical type.
                    Instant landedAt = frame.Now();
                    ResidenceCell stamp = new ResidenceCell.Moment(landedAt);
                    foreach (Seq<ResidenceCell> row in rows) {
                        await importer.StartRowAsync().ConfigureAwait(false);
                        await importer.WriteAsync(tenant, ColumnType.KeyHex.Wire).ConfigureAwait(false);
                        foreach ((ResidenceCell Cell, (ColumnRow Column, NpgsqlDbType Wire) Bound) pair in row.Zip(proved.Bound)) {
                            await pair.Cell.Stage(importer, pair.Bound.Wire).ConfigureAwait(false);
                        }
                        foreach ((Identifier Name, NpgsqlDbType Wire) landed in proved.Landed.ToSeq()) {
                            await stamp.Stage(importer, landed.Wire).ConfigureAwait(false);
                        }
                    }
                    ulong staged = await importer.CompleteAsync().ConfigureAwait(false);
                    // Receipt and rows carry the SAME instant, so evidence and residence agree on when this
                    // batch landed; a second clock read here dates the receipt after the rows it accounts for.
                    return Fin<ResidenceIngestReceipt>.Succ(new ResidenceIngestReceipt(schema.Dataset, (long)staged, landedAt, frame.Correlation));
                }
                // One catch filter spans the driver base and this seam's own conversion refusal: a SQLSTATE, a
                // socket drop mid-copy, and a value the wire type cannot spell are one ingest refusal, each
                // rendering through the row's own `Diagnose` without a cast. Cancellation stays a rail fact and
                // propagates — a cooperative cancel converted to a fault reads as a failed landing.
                catch (Exception wire) when (wire is NpgsqlException or InvalidCastException) {
                    return Fin<ResidenceIngestReceipt>.Fail(Residence.Series.IngestRefused(wire));
                }
            }),
            Fail: error => IO.pure(Fin<ResidenceIngestReceipt>.Fail(error)));

    // Conformance is ARITY, TYPE, CANONICITY, and WRITABILITY together, all ahead of the copy: a short row
    // shifts every later column silently, a cell whose arm disagrees with its column's declared shape binds a
    // value the server rejects mid-stream after the copy has already staged its predecessors, a composite
    // cell carrying a non-text element or a repeated map key raises out of the write loop itself, and a shape
    // no single `NpgsqlDbType` spells has nothing to bind at all. Resolving the wire type HERE is what keeps the copy
    // loop total — the fold hands each column its bound value and the loop carries no fallible lookup.
    // Arity proves against `Supplied`, never `Payload` — against what the producer's contract obliges it to
    // send — so a row one cell short of that contract still refuses exactly as before while a row exactly
    // matching it passes. Counting the custodian's own stamped columns here demands cells the category forbids
    // a producer to carry, and the gate then reads a correct producer as a defective one.
    static Fin<Seq<(ColumnRow Column, NpgsqlDbType Wire)>> Conformed(AnalyticsSchema schema, Seq<ColumnRow> supplied, Seq<Seq<ResidenceCell>> rows) =>
        rows.Exists(row => row.Count != supplied.Count)
            ? Fin.Fail<Seq<(ColumnRow, NpgsqlDbType)>>(new ResidenceFault.IngestRefused(Residence.Series.Key, new EngineFault("<row-arity>", schema.Dataset)))
            : rows.Bind(row => row.Zip(supplied))
                .Find(static pair => pair.Item1.Column != pair.Item2.Type)
                .Match(
                    Some: pair => Fin.Fail<Seq<(ColumnRow, NpgsqlDbType)>>(new ResidenceFault.IngestRefused(Residence.Series.Key,
                        new EngineFault("<cell-type>", $"{schema.Dataset}.{(string)pair.Item2.Name}"))),
                    None: () => rows.Bind(static row => row).TraverseM(static cell => cell.Canonical).As()
                        .Bind(_ => supplied.Traverse(static column => column.Type.Wire.Map(wire => (column, wire))).As()));
}
```

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// One row per Series family; the WHOLE hypertable provisioning set derives from these columns. `Facets` names the
// ordered text columns a family carries beyond the shared `(tenant, series_key, at, value)` spine, so the
// telemetry stream keys by domain, slot, and measure without a second table and Assessment/Sensor carry none.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesKind {
    // Spine and facet identifiers lead the rows: a static field initializer runs in DECLARATION order, so a row
    // reading a facet declared below it would capture an uninitialized `Identifier` and mount a nameless column.
    public static readonly Identifier SeriesColumn = Identifier.Create("series_key");
    public static readonly Identifier AtColumn = Identifier.Create("at");
    public static readonly Identifier ValueColumn = Identifier.Create("value");
    public static readonly Identifier DomainFacet = Identifier.Create("domain");
    public static readonly Identifier SlotFacet = Identifier.Create("slot");
    public static readonly Identifier MeasureFacet = Identifier.Create("measure");

    public static readonly SeriesKind Assessment = new("assessment", "assessment_series",
        Duration.FromHours(1), Duration.FromDays(365), Duration.FromDays(1), Duration.FromDays(3), Seq<Identifier>());
    public static readonly SeriesKind Sensor = new("sensor", "sensor_series",
        Duration.FromMinutes(15), Duration.FromDays(90), Duration.FromDays(1), Duration.FromDays(3), Seq<Identifier>());
    // Receipt-stream measures: the fan projects each numeric receipt field as one point under its domain,
    // slot, and measure path, so a board tile reads a one-minute continuous aggregate instead of scanning the
    // evidence plane, while the receipt itself stays the truth this projection derives from and never
    // replaces. The measure facet is what makes a tile expressible in TEXT — a series key is a content hash
    // no dashboard can spell, so the three facets together name the stream a query filters on.
    public static readonly SeriesKind Telemetry = new("telemetry", "telemetry_series",
        Duration.FromMinutes(1), Duration.FromDays(90), Duration.FromHours(6), Duration.FromDays(1),
        Seq(DomainFacet, SlotFacet, MeasureFacet));

    public string Table { get; }
    public Duration Bucket { get; }
    public Duration DropAfter { get; }
    public Duration Chunk { get; }
    public Duration Backfill { get; }
    public Seq<Identifier> Facets { get; }
    private SeriesKind(string key, string table, Duration bucket, Duration dropAfter, Duration chunk, Duration backfill, Seq<Identifier> facets) : this(key) =>
        (Table, Bucket, DropAfter, Chunk, Backfill, Facets) = (table, bucket, dropAfter, chunk, backfill, facets);

    // One projection into the residence family's own schema shape, so ONE provisioning emitter serves the
    // hypertable roster and every producer-handed dataset alike and no second DDL path exists. The spine
    // travels as schema columns rather than as statics the emitter reaches for, which is what lets a
    // producer-handed dataset with its own instant name provision through the identical arm.
    public AnalyticsSchema Schema => new(Table,
        Seq(SeriesColumn) + Facets,
        Seq(new ColumnRow(SeriesColumn, ColumnType.KeyHex, Nullable: false))
            + Facets.Map(static facet => new ColumnRow(facet, ColumnType.Utf8, Nullable: false))
            + Seq(new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false),
                  new ColumnRow(ValueColumn, ColumnType.Float64, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: Some(ValueColumn));

    // Relational tiers rest in their own table, so this residence root IS the table name and the Lake arm
    // alone reads the column as a hive generation directory.
    public ResidencePolicy Policy => new(DropAfter, Bucket, Chunk, Backfill, StorePath.Create(Table));
}

// ingest row: `Series` is the content-key identity the source artifact already carries (the assessment
// `(subgraph·route·policy)` key, a telemetry point's `(scope·instrument·dimensions)` key), `At` the sample
// instant, `Value` the measure, and `Facets` the ordered text values binding positionally against the kind's
// own `Facets` roster. Tenancy is NOT a point column — the whole COPY batch lands under the ingesting
// frame's tenant and every read filters by it, so equal series keys under distinct tenants never share rows.
public readonly record struct SeriesPoint(UInt128 Series, Instant At, double Value, Seq<string> Facets);

// Series selection discriminates on the VALUE's own shape: a caller holding the source artifact's content key
// names the key, and a board naming its stream names the facet values, which bind positionally against the
// kind's own roster exactly as an ingested point's do. Without the facet arm the whole telemetry projection is
// unreadable from a dashboard — a series key is a content hash no tile can spell — so the two arms are one
// predicate fragment and neither read entry forks by selector.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SeriesSelector {
    private SeriesSelector() { }
    public sealed record Key(UInt128 Series) : SeriesSelector;
    public sealed record Facets(Seq<string> Values) : SeriesSelector;
}
```

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SeriesLane {
    // Provisioning rides the one residence emitter over the kind's own schema projection, and the rows ride the
    // same reviewed-migration rail every `Store/provisioning#SERVER_EXTENSIONS` admission rides, gated on the
    // verdict holding the `timescaledb` lane (`Unprovisioned` when absent).
    public static Seq<string> Provision(SeriesKind kind) =>
        ResidenceDdl.Provision(Residence.Series, kind.Schema, kind.Policy);

    // Hypertable-family ARM of the one residence landing: this projects points into the kind's own declared
    // column order and `ResidenceLanding.Stage` owns the copy loop, the tenancy lead, and the wire types, so a
    // spine column add moves the schema projection alone and no second importer body exists to drift from it.
    // Facet arity refuses inside the shared conformance gate, which reads the same roster this projection walks.
    public static IO<Fin<ResidenceIngestReceipt>> Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame) =>
        ResidenceLanding.Stage(store, kind.Schema, points.Map(Cells), frame);

    // Declaration order IS the projection order — key, facets, instant, measure — read off `SeriesKind.Schema`
    // rather than re-spelled, so the DDL, the COPY roster, and this fold cannot disagree on position.
    static Seq<ResidenceCell> Cells(SeriesPoint point) =>
        Seq<ResidenceCell>(new ResidenceCell.Key(point.Series))
        + point.Facets.Map(static facet => (ResidenceCell)new ResidenceCell.Text(facet))
        + Seq<ResidenceCell>(new ResidenceCell.Moment(point.At), new ResidenceCell.Real(point.Value));

    // Scope fragment both reads share: relation and column names are closed-vocabulary row values composed into
    // this text while only VALUES bind as parameters, so a spine rename moves DDL, ingest column list, and both
    // reads together and tenant, selector, and window still cross as typed parameters. The selector's own arm
    // supplies its predicate, so a facet read and a key read differ by a fragment rather than by an entry.
    // Each selector conjunct carries its OWN leading `AND`, so a family declaring no facets composes a
    // well-formed predicate under a facet selector instead of the empty join that leaves a dangling operator —
    // and reads every stream the tenant and window hold, which is what a nameless family honestly selects.
    static string Scope(SeriesKind kind, Identifier axis, SeriesSelector selector) =>
        $"WHERE {Residence.Series.Quote(Residence.TenantColumn)} = @tenant"
        + selector.Switch(
            state: kind,
            key:    static (_, _) => $" AND {Residence.Series.Quote(SeriesKind.SeriesColumn)} = @series",
            facets: static (family, _) => string.Concat(family.Facets.Map(static (facet, index) => $" AND {Residence.Series.Quote(facet)} = @f{index}")))
        + $" AND {Residence.Series.Quote(axis)} >= @from AND {Residence.Series.Quote(axis)} < @until";

    // Selector parameters as ROWS, so the fragment and the binding read one arm and a facet added to a kind
    // moves both together; a facet arity the roster does not match refuses before the command opens rather than
    // binding a short row set the server then reports as a missing parameter.
    static Fin<Seq<(string Name, object Value)>> Binding(SeriesKind kind, SeriesSelector selector) =>
        selector.Switch(
            state: kind,
            key:    static (_, row) => Fin.Succ(Seq((Name: "series", Value: (object)ResidenceCell.Packed(row.Series)))),
            facets: static (family, row) => row.Values.Count == family.Facets.Count
                ? Fin.Succ(row.Values.Map(static (value, index) => (Name: $"f{index}", Value: (object)value)).Strict())
                : Fin.Fail<Seq<(string Name, object Value)>>(new ResidenceFault.ReadRefused(Residence.Series.Key, new EngineFault("<facet-arity>", family.Key))));

    // toolkit time-weighted read over RAW chunks, the grain below the rollup's bucket: each sample weighs by
    // its holding interval, the honest mean for irregular timesteps a naive avg over-counts. The fold groups by
    // series because `time_weight` summaries combine only across DISJOINT windows — folding two live streams
    // sharing a window is a mean no algebra defines — so a facet selection matching several streams answers one
    // weighted mean each. An empty window yields an empty seq: absence stays absence, where a collapsed `0d`
    // is a reading a board renders indistinguishably from a measured floor.
    public static IO<Fin<Seq<SeriesWeight>>> Weighted(NpgsqlDataSource store, SeriesKind kind, SeriesSelector selector, ResidenceWindow window, ProjectionContext frame) =>
        Rows(store, $"SELECT {Residence.Series.Quote(SeriesKind.SeriesColumn)}, average(time_weight('linear', {Residence.Series.Quote(SeriesKind.AtColumn)}, {Residence.Series.Quote(SeriesKind.ValueColumn)})) FROM {Residence.Series.Quote(kind.Schema.Table)} {Scope(kind, SeriesKind.AtColumn, selector)} GROUP BY {Residence.Series.Quote(SeriesKind.SeriesColumn)}",
            kind, selector, window, frame,
            static reader => new SeriesWeight(Identity(reader, 0), reader.GetDouble(1)));

    // Pre-bucketed read off the continuous aggregate the Series arm emitted: the view name, the bucket axis,
    // and the accessor projection all read that arm's own declaration, so the reader binds by an ordinal the
    // emitter owns rather than by a column list two sites spell. The accessors run over the SAME materialised
    // summaries the raw-chunk read folds live, so the cheap tile and the expensive investigation answer one
    // statistic — a materialised `avg` beside a `time_weight` read is two means wearing one caption.
    public static IO<Fin<Seq<SeriesBucket>>> Bucketed(NpgsqlDataSource store, SeriesKind kind, SeriesSelector selector, ResidenceWindow window, double quantile, ProjectionContext frame) =>
        Rows(store, $"SELECT {Residence.Series.Quote(SeriesResidence.Bucket)}, {Residence.Series.Quote(SeriesKind.SeriesColumn)}, {SeriesResidence.Projection(quantile)} FROM {Residence.Series.Quote(SeriesResidence.Rollup(kind.Schema))} {Scope(kind, SeriesResidence.Bucket, selector)} ORDER BY {Residence.Series.Quote(SeriesResidence.Bucket)}",
            kind, selector, window, frame,
            static reader => new SeriesBucket(reader.GetFieldValue<Instant>(0), Identity(reader, 1),
                reader.GetDouble(2), reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(5), reader.GetInt64(6)));

    // Timescale-ONLY enrichment beside the family probe, and named for what it is: `ResidenceRead.Health`
    // measures the expiry OUTCOME every residence answers, while this reads the bgworker run history only the
    // Timescale catalog publishes — a failed status, or a last_successful_finish older than twice the job's
    // schedule interval, names WHICH policy stalled where the outcome probe reports only that residue survived.
    // Sibling residences carry no counterpart and need none: each scheduler publishes no such catalog, which
    // is exactly why the family surface measures outcome rather than transcribing one engine's self-report.
    public static IO<Fin<Seq<SeriesJobHealth>>> Jobs(NpgsqlDataSource store, SeriesKind kind) =>
        IO.liftAsync(async () => {
            await using NpgsqlCommand command = store.CreateCommand(
                "SELECT j.hypertable_name, s.job_status, s.last_successful_finish, s.total_failures FROM timescaledb_information.jobs j JOIN timescaledb_information.job_stats s ON s.job_id = j.job_id WHERE j.hypertable_name = @table");
            _ = command.Parameters.AddWithValue("table", (string)kind.Schema.Table);
            try {
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                List<SeriesJobHealth> rows = [];
                while (await reader.ReadAsync().ConfigureAwait(false)) {
                    rows.Add(new SeriesJobHealth(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? Option<Instant>.None : Some(reader.GetFieldValue<Instant>(2)), reader.GetInt64(3)));
                }
                return Fin<Seq<SeriesJobHealth>>.Succ(toSeq(rows));
            }
            catch (NpgsqlException wire) { return Fin<Seq<SeriesJobHealth>>.Fail(Residence.Series.ReadRefused(wire)); }
        });

    // Selector binding refuses BEFORE the command opens, so an arity break never reaches the server; the three
    // scope parameters bind unconditionally and the selector's own rows fold on beside them.
    static IO<Fin<Seq<T>>> Rows<T>(NpgsqlDataSource store, string sql, SeriesKind kind, SeriesSelector selector, ResidenceWindow window, ProjectionContext frame, Func<NpgsqlDataReader, T> shape) =>
        Binding(kind, selector).Match(
            Succ: bound => IO.liftAsync(async () => {
                await using NpgsqlCommand command = store.CreateCommand(sql);
                _ = command.Parameters.AddWithValue("tenant", ResidenceCell.Packed(frame.Tenant.TenantId.Value));
                _ = command.Parameters.AddWithValue("from", window.From);
                _ = command.Parameters.AddWithValue("until", window.Until);
                bound.Iter(row => ignore(command.Parameters.AddWithValue(row.Name, row.Value)));
                try {
                    await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    List<T> rows = [];
                    while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
                    return Fin<Seq<T>>.Succ(toSeq(rows));
                }
                catch (NpgsqlException wire) { return Fin<Seq<T>>.Fail(Residence.Series.ReadRefused(wire)); }
            }),
            Fail: error => IO.pure(Fin<Seq<T>>.Fail(error)));

    // Read inverse of `ResidenceCell.Packed`: one decoder returns the identity a facet-selected read groups by,
    // beside the one encoder every `KeyHex` landing already shares.
    static UInt128 Identity(NpgsqlDataReader reader, int ordinal) =>
        BinaryPrimitives.ReadUInt128BigEndian(reader.GetFieldValue<byte[]>(ordinal));
}

// One raw-chunk weighted mean per matching series; `Series` is the identity a facet selection resolves to, so
// a caller reading a single stream reads one row and a caller reading a family reads each stream apart.
public readonly record struct SeriesWeight(UInt128 Series, double Weighted);

// One rollup row per (bucket, series): `Mean` is the time-weighted accessor over the materialised summary and
// `Tail` the sketch quantile the read named, so a tile's caption and the statistic behind it are one choice.
public readonly record struct SeriesBucket(Instant Bucket, UInt128 Series, double Mean, double Tail, double Low, double High, long Samples);

// One job_stats row per Timescale background job on the kind's hypertable — status, last successful finish,
// failure counter — the WHICH-policy-stalled detail the family `ResidenceHealth` outcome probe cannot name.
public readonly record struct SeriesJobHealth(string Hypertable, string Status, Option<Instant> LastSuccessfulFinish, long TotalFailures);

// ONE warehouse row vocabulary both ends of the Fleet seam read: the `Version/egress` sink lands EXACTLY these
// columns projected from `Egress.Envelope` (`id` the content key, `source`/`type`/`time` the envelope attributes,
// `partition_key`/`sequence` the partitioning extensions, `data` the redacted payload), and a fleet question
// composes over `WarehouseSchema.Shape`. The roster is one `AnalyticsSchema` value, so the sink's insert
// columns, the residence DDL, and the reader's ordinals derive from ONE declaration and cannot drift.
public sealed record WarehouseOpRow(string Id, string Source, string Type, Instant Time, string PartitionKey, long Sequence, ReadOnlyMemory<byte> Data);

public static class WarehouseSchema {
    // Op-log rows spell their instant `time`, not `at` — exactly why the residence spine travels as a schema
    // column: this dataset provisions its MergeTree partition, sort key, and TTL through the same arm every
    // `at`-spelled series table rides, with no dialect arm branching on a column name.
    public static readonly Identifier TimeColumn = Identifier.Create("time");

    public static readonly AnalyticsSchema Dataset = new("rasm.oplog",
        Seq(Identifier.Create("source"), Identifier.Create("type"), TimeColumn),
        Seq(new ColumnRow(Identifier.Create("id"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("source"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("type"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(TimeColumn, ColumnType.Timestamp, Nullable: false),
            new ColumnRow(Identifier.Create("partition_key"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("sequence"), ColumnType.Int64, Nullable: false),
            new ColumnRow(Identifier.Create("data"), ColumnType.Utf8, Nullable: false)),
        Time: TimeColumn, Spine: TimeSpine.Event, Measure: None);

    public static string Table => (string)Dataset.Table;
    public static string Columns => string.Join(", ", Dataset.Columns.Map(static column => (string)column.Name));

    // Ordinals read off the declaration above, so a column insert moves the reader and the DDL together and the
    // shape binds through the one row surface every reach yields. Every column declares `Nullable: false`, so the
    // seven reads compose as one applicative product and a residence answering an empty cell refuses naming it
    // rather than handing the fleet leg a row wearing a fabricated value.
    public static Fin<WarehouseOpRow> Shape(Residence residence, ResidenceRow row) =>
        (row.Text(residence, 0), row.Text(residence, 1), row.Text(residence, 2), row.At(residence, 3),
            row.Text(residence, 4), row.Whole(residence, 5), row.Text(residence, 6))
        .Apply(static (id, source, type, time, partition, sequence, data) =>
            new WarehouseOpRow(id, source, type, time, partition, sequence, Encoding.UTF8.GetBytes(data))).As();
}
```

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Receipt EVIDENCE plane: the kernel `ReceiptEnvelope` flattened to one wide-event row per emission. The
// producer-side pattern every sibling folder already spells — a typed row record beside its schema and one
// pure fold — transcribes here because this custodian receives the stream rather than minting it, and the
// payload crosses as its own JSON text so a residence scan never re-decodes a foreign package's shape.
// `Tenant` is the ROUTING key, not a stored column: each residence owns its tenant column at the one key type
// every tenancy predicate compares against, and every scan is tenant-scoped by frame, so a per-row tenant
// column duplicates that key at a second physical type. Multi-tenant folds split on this field before the
// write, and every read returns exactly the tenant it scoped to.
public readonly record struct ReceiptFactRow(
    string Package, string Kind, string Domain, string Correlation, string Tenant,
    Instant At, long Logical, long SkewNanos, string Payload);

public static class ReceiptResidence {
    // One dataset per CAPABILITY DOMAIN under the `telemetry.<domain>` grammar, so a residence query joins on the
    // same domain segment a metric name carries and a scan never crosses two subjects. The whole column
    // roster is one declaration the DDL emitter, the egress projection, and every reader's ordinals derive
    // from, so a column add lands once.
    // Wide events declare NO measure: an envelope carries a payload, not a scalar, so this dataset provisions
    // its hypertable, columnstore, and retention and emits no continuous aggregate — numeric rollup rides
    // whichever `SeriesKind.Telemetry` projection `Points` derives, and a fabricated `avg` over a JSON
    // payload column is the form this measure-free row forecloses.
    public static AnalyticsSchema Dataset(string domain) => new($"telemetry.{domain}",
        Seq(PackageColumn, KindColumn, AtColumn),
        Seq(new ColumnRow(PackageColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(KindColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(DomainColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(CorrelationColumn, ColumnType.Utf8, Nullable: false),
            new ColumnRow(AtColumn, ColumnType.Timestamp, Nullable: false),
            new ColumnRow(LogicalColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(SkewColumn, ColumnType.Int64, Nullable: false),
            new ColumnRow(PayloadColumn, ColumnType.Utf8, Nullable: false)),
        Time: AtColumn, Spine: TimeSpine.Event, Measure: None);

    // Total domain projection: a dotted slot carries its capability domain in the second segment exactly as the
    // `store.<domain>.<verb>` grammar declares, and a bare kind (a package whose receipt kinds are flat
    // case literals) falls to the emitting package id — so every envelope resolves a domain and none lands
    // under an empty partition key the residence would then scan whole.
    public static string Domain(ReceiptEnvelope envelope) =>
        envelope.Kind.Split('.') is [_, var domain, ..] && domain.Length > 0 ? domain : envelope.Package;

    // One pure fold — every column derives from the envelope the sink already sealed, so the residence
    // carries envelope-grade provenance and re-measures nothing.
    public static Seq<ReceiptFactRow> Facts(Seq<ReceiptEnvelope> envelopes) =>
        envelopes.Map(static envelope => new ReceiptFactRow(
            envelope.Package, envelope.Kind, Domain(envelope),
            envelope.Correlation.ToString(), envelope.Tenant.Entry,
            envelope.Physical, (long)envelope.Logical,
            (long)envelope.SkewBound.ToInt64Nanoseconds(),
            envelope.Payload.GetRawText()));

    // MEASURE PROJECTION: every numeric leaf of a receipt payload is one point on the `SeriesKind.Telemetry`
    // hypertable, keyed by the dotted path it sits at, so a producer adding a numeric field gains its series
    // with zero rows here and a board tile filters `(domain, slot, measure)` in TEXT rather than a content
    // hash no dashboard can spell. Arrays stay off this plane — a per-row collection is evidence the wide
    // event carries whole, never a scalar a time bucket averages — and the walk is depth-bounded so a nested
    // payload cannot fan unbounded series out of one envelope. The unit separator between the identity parts
    // keeps a dotted measure path from colliding with the package and kind segments ahead of it.
    const int MeasureDepth = 4;

    public static Seq<SeriesPoint> Points(Seq<ReceiptEnvelope> envelopes) =>
        envelopes.Bind(static envelope => Measures(envelope.Payload, string.Empty, MeasureDepth)
            .Map(measure => new SeriesPoint(
                Series: XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"{envelope.Package}{envelope.Kind}{measure.Path}")),
                At: envelope.Physical,
                Value: measure.Value,
                Facets: Seq(Domain(envelope), envelope.Kind, measure.Path))));

    static Seq<(string Path, double Value)> Measures(JsonElement node, string path, int depth) =>
        node.ValueKind switch {
            JsonValueKind.Number when path.Length > 0 => Seq((path, node.GetDouble())),
            JsonValueKind.Object when depth > 0 => toSeq(node.EnumerateObject()).Bind(property =>
                Measures(property.Value, path.Length == 0 ? property.Name : $"{path}.{property.Name}", depth - 1)),
            _ => Seq<(string, double)>(),
        };

    // COLD-TAIL handoff: `#FLAT_TABLE_EGRESS`'s `LandingArm.Receipt` generation lands Arrow batches, so the
    // custodian projects its own evidence rows into one batch off the dataset's own field list — the
    // record-batch half of the `[WIRE]: AnalyticsSchema` seam, derived from the one declaration rather than a
    // schema hand-built beside it. Column order IS the declaration order every reader's ordinals already bind.
    public static (Schema Fields, RecordBatch Batch) Batch(string domain, Seq<ReceiptEnvelope> envelopes) {
        Seq<ReceiptFactRow> rows = Facts(envelopes);
        Schema fields = Dataset(domain).Fields;
        return (fields, new RecordBatch(fields, Seq<IArrowArray>(
            Text(rows.Map(static row => row.Package)), Text(rows.Map(static row => row.Kind)),
            Text(rows.Map(static row => row.Domain)), Text(rows.Map(static row => row.Correlation)),
            Stamps(rows.Map(static row => row.At)), Whole(rows.Map(static row => row.Logical)),
            Whole(rows.Map(static row => row.SkewNanos)), Text(rows.Map(static row => row.Payload))), rows.Count));
    }

    static StringArray Text(Seq<string> values) => new StringArray.Builder().AppendRange(values).Build();
    static Int64Array Whole(Seq<long> values) => new Int64Array.Builder().AppendRange(values).Build();
    // Builders read the SAME `ColumnType` row the schema field does, so batch unit and zone can never disagree
    // with the declared field a landing then rejects at write.
    static TimestampArray Stamps(Seq<Instant> values) =>
        new TimestampArray.Builder((TimestampType)ColumnType.Timestamp.Arrow)
            .AppendRange(values.Map(static at => at.ToDateTimeOffset())).Build();

    // ONE named question over the evidence plane, composed on the family's own plan builder: consumers name
    // whichever correlation they reconstruct and take the plan, so no page assembles Substrait relations and
    // no page writes SQL. Scope — tenant and window — rides the read frame, so a correlation-free call is
    // exactly the whole-window scan a durable usage fold reads.
    public static Fin<Plan> Scan(string domain, Option<CorrelationId> correlation) =>
        ResidencePlan.Scan(Dataset(domain), correlation.Map(id => (CorrelationColumn, id.ToString())).ToSeq());

    // DURABLE counterpart to the in-process sink, and the producing half of the `[RECEIPT]: resident
    // ReceiptEnvelope` seam: the read hands back the SAME envelope values the live sink held, so
    // `Rasm.AppUi/Diagnostics/evidence#CORRELATION_JOIN`'s `EvidenceSource.Resident` arrow binds here and the
    // correlation join and the billing accrual each stay ONE fold over two sources — an incident reconstructs
    // after the process that emitted it is gone rather than through a second fold that would drift from the
    // first. The read's own scan magnitude rides `ResidenceResult.Receipt` at this custodian, so a consumer
    // arrow yielding bare envelopes surrenders no diagnosis.
    public static IO<Fin<ResidenceResult<ReceiptEnvelope>>> Resident(
        ResidenceReach reach, ResidenceScope scope, string domain, Option<CorrelationId> correlation) =>
        Scan(domain, correlation).Match(
            Succ: plan => ResidenceRead.Read(reach, plan, scope, ResidenceProjection.Point,
                row => Envelope(scope, row)),
            Fail: error => IO.pure(Fin<ResidenceResult<ReceiptEnvelope>>.Fail(error)));

    // Egress rides the ONE `COPY (SELECT) TO` rail: the domain partition key is the artifact class's own row
    // and the stamp is the caller's content address, so an evidence generation carries its identity in the
    // Parquet footer and a renamed file keeps it.
    public static IO<Fin<Unit>> Publish(ColumnarSession session, string domain, StorePath destination, UInt128 stamp) =>
        ArtifactEgress.Publish(session, ArtifactClass.TelemetryEvidence,
            new Projection(Dataset(domain).Table, Dataset(domain).Columns.Map(static column => column.Name)),
            destination, stamp);

    public static readonly Identifier PackageColumn = Identifier.Create("package");
    public static readonly Identifier KindColumn = Identifier.Create("kind");
    public static readonly Identifier DomainColumn = Identifier.Create("domain");
    public static readonly Identifier CorrelationColumn = Identifier.Create("correlation");
    public static readonly Identifier AtColumn = Identifier.Create("at");
    public static readonly Identifier LogicalColumn = Identifier.Create("logical");
    public static readonly Identifier SkewColumn = Identifier.Create("skew_nanos");
    public static readonly Identifier PayloadColumn = Identifier.Create("payload");

    // Reader inverse over the one row surface: ordinals read off `Dataset`'s own declaration through the root
    // relation's projected names, so a column insert moves schema, DDL, and this reader together and physical
    // residence column order never reaches a consumer. Tenant returns from the frame each read scoped with —
    // that being the only tenant a tenant-scoped scan can have returned.
    public static Fin<ReceiptFactRow> Shape(ResidenceScope scope, ResidenceRow row) =>
        (row.Text(scope.Residence, 0), row.Text(scope.Residence, 1), row.Text(scope.Residence, 2),
            row.Text(scope.Residence, 3), row.At(scope.Residence, 4), row.Whole(scope.Residence, 5),
            row.Whole(scope.Residence, 6), row.Text(scope.Residence, 7))
        .Apply((package, kind, domain, correlation, at, logical, skew, payload) =>
            new ReceiptFactRow(package, kind, domain, correlation, scope.Frame.Tenant.Entry, at, logical, skew, payload)).As();

    // ENVELOPE inverse over the flat row rather than over the ordinals a second time — the shape a consuming
    // fold declared against the live sink, so a durable read reaches those folds with no second decode, no
    // flat-row twin of the causal frame, and no ordinal roster that can drift from the one above it.
    // Exemption: the parse is a boundary capsule and `JsonElement` outlives no `JsonDocument`, so the payload
    // CLONES onto its own buffer — an element handed out past the parse scope reads returned pooled memory.
    public static Fin<ReceiptEnvelope> Envelope(ResidenceScope scope, ResidenceRow row) =>
        Shape(scope, row).Map(fact => {
            using JsonDocument payload = JsonDocument.Parse(fact.Payload);
            return new ReceiptEnvelope(
                CorrelationId.Create(Guid.Parse(fact.Correlation)), scope.Frame.Tenant, fact.Package, fact.Kind,
                payload.RootElement.Clone(), fact.At, (ulong)fact.Logical, Duration.FromNanoseconds(fact.SkewNanos));
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                       | [BINDING]                                                         |
| :-----: | :------------------ | :-------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | residence family    | one `Residence` row per capability tier       | a residence hardcoded below the family is the deleted form        |
|  [02]   | cardinality         | NO cap on any residence                       | unbounded dimensionality IS the capability; the metrics cap holds |
|  [03]   | authority           | residences are DERIVED, receipts are truth    | a dropped residence rebuilds at warm-up cost, never billing loss  |
|  [04]   | DDL custody         | branch-owned at this custodian                | `clickhouseexporter` runs `create_schema: false`                  |
|  [05]   | query currency      | one Substrait `Plan` per question             | one lowering, three dialects; no second query language            |
|  [06]   | tenancy             | sort-key column or hive prefix                | one `TenantId.Wire` text; never a filter-only separation          |
|  [07]   | series provisioning | derived from the `SeriesKind` row             | SELECT/CALL emission law; migration-carried, verdict-gated        |
|  [08]   | policy cadence      | each residence's own scheduler                | never AppHost-scheduled; `job_stats` is the Series proof row      |
|  [09]   | irregular timesteps | `average(time_weight('linear', …))`           | a naive `avg` over-counts dense bursts                            |
|  [10]   | residence retention | in-residence policy, TTL, or generation evict | projection of retained evidence; never a `RetentionClass` row     |
|  [11]   | fleet leg           | READ row + `QueryStats` scanned figure        | the egress sink owns landing; never a second SoR                  |
|  [12]   | lake reach          | Flight SQL cross-runtime, DuckDB in-process   | one plane per the Tier-0 ruling; never a sidecar transport        |
|  [13]   | series identity     | the source artifact's content key             | one origin with `ArtifactKind.Assessment`; never a second mint    |
|  [14]   | temporal spine      | `AnalyticsSchema.Time` + optional `Measure`   | proven at the seam; no arm reaches a spine literal                |
|  [15]   | read scope          | frame tenant + `ResidenceWindow`              | the plan carries shape; an unbounded scan is unrepresentable      |
|  [16]   | physical literals   | `Literal` tenant + `Stamp` instant per row    | a text comparison against `bytea`/`FixedString` matches nothing   |
|  [17]   | measure projection  | numeric leaves under a facet triple           | text facets name a stream; a hash names nothing                   |
|  [18]   | honest degradation  | one `Degrade` clause per row                  | `Unanswerable` reports it; never a boolean or a silent empty tile |
|  [19]   | lowered relation    | table for a tier, view for the hive tree      | every residence answers one `FROM`; never an uncreated relation   |
|  [20]   | relation spelling   | one lower-cased `Table` every dialect quotes  | PG folds an unquoted name; DDL and read address one relation      |
|  [21]   | narrowing literal   | the column's own declared `ColumnType.Plan`   | a text operand against a numeric column raises or coerces         |
|  [22]   | rollup statistic    | materialised summary + read-time accessor     | one statistic per caption; never `avg` accelerating `time_weight` |
|  [23]   | columnstore segment | bounded text keys; identity and time order    | a `KeyHex` segment mints one batch per row and deletes it         |
|  [24]   | batch handoff       | `AnalyticsSchema.Fields` off the one row set  | the seam trades batches; never a schema built beside the dataset  |
|  [25]   | relational landing  | one `ResidenceLanding.Stage` binary COPY      | `SeriesLane.Ingest` is its arm; never a second importer body      |
|  [26]   | cell conformance    | arity, type, canonicity, writability pre-copy | the importer infers nothing; a row-n refusal discards n-1 rows    |
|  [27]   | total column read   | `Fin` readers over both row arms              | absence is a refusal; never an empty string, a zero, or 1970      |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
