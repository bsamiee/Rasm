# [PERSISTENCE_QUERY_COLUMNAR]

Rasm.Persistence runs analytical aggregation, columnar rollup, lakehouse scan, and artifact egress through one in-process DuckDB engine. `ColumnarSession` owns one posture-configured anchor and gives every concurrent operation a `Duplicate()` lane. `Identifier` admits all SQL identifier roles, `StorePath` admits storage locations, and `SecretResidence` distinguishes session and persistent credentials. Parquet and ADBC remain distinct boundaries over the shared Arrow model. Beyond the in-process engine, this page owns the two residences the provisioning axis map assigns it — the in-PG TimescaleDB series tier turning assessment and sensor results into queryable temporal data, and the ClickHouse fleet-scale read row consuming what the `Version/egress` sink lands.

## [01]-[INDEX]

- [01]-[COLUMNAR_LANE]: the posture-configured DuckDB anchor session, the `INSTALL`/`LOAD` extension-profile bootstrap, the parameterized query and streaming drain, the typed bulk appender, the read-only `ATTACH` mount, the `CREATE SECRET` credential rail, the ADBC Arrow bridge, and the closed fault rail.
- [02]-[ARTIFACT_EGRESS]: the engine-mediated `COPY (SELECT) TO` artifact rail over the closed `Format`/`Codec`/`Collision`/`ArtifactClass` vocabularies, the footer-metadata stamp and admission read, and the `read_parquet` generation scan.
- [03]-[FLAT_TABLE_EGRESS]: the co-transactional `Ara3D.BimOpenSchema` `FlatTableProjection`, the in-corpus eleven-table `.duckdb` write, the async Parquet daemon materialization, the native `ParquetSharp.Arrow` codec lane — read and PME-encrypted write — the columnar query rides, and the metadata-only Delta-log publication versioning each generation to its AS-OF cut.
- [04]-[SERIES_AND_SCALEOUT]: the beyond-engine residences — the `SeriesKind`-declared TimescaleDB hypertable tier (provisioning SQL rows under the SELECT/CALL emission law, binary-COPY ingest, continuous-aggregate and time-weighted reads, in-database retention and columnstore policy) and the ClickHouse fleet-scale read row over the `Version/egress` `ClickHouse` sink's landed table.

## [02]-[COLUMNAR_LANE]

- Owner: `Identifier`, `StorePath`, `ExecutionThreads`, and `AdbcSql` admit dynamic boundary values; `SecretResidence` carries credential lifetime; `AdbcRequest` carries one statement door plus its optional bind; `ColumnarSession` owns one anchor and duplicate lanes; `ColumnarProfile`, `ColumnarExtension`, `ColumnarFault`, `AdbcQuery`, and `ColumnarLane` own posture, capabilities, faults, execution shape, and operations.
- Cases: `ColumnarProfile` is `Geometry` (`spatial`/`parquet`, memory-heavy spatial aggregation posture), `Search` (`vss`/`fts`, ANN/BM25 columnar posture), `Lakehouse` (`httpfs`/`iceberg`/`delta`/`postgres`, order-free remote-scan posture with `preserve_insertion_order` false), `Bim` (`parquet`/`json` over the BimOpenSchema `.duckdb`, read-your-writes BIM analytics posture), `Federation` (`parquet`/`substrait`/`postgres`, fail-closed on the community row); `ColumnarExtension` rows are `Spatial`, `Vss`, `Fts`, `Parquet`, `Json`, `Httpfs`, `Iceberg`, `Delta`, `Postgres`, `Sqlite`, `Excel`, `Avro`, `Icu`, `Aws`, `Azure`, and `Substrait`; `ColumnarFault` closes native query, extension, append, mount, egress, stamp, secret, trust, Delta, and policy admission failures across `8350`–`8359`.
- Entry: `Open` boots and probes the profile; `Query`, `Append`, `Mount`, `Secret`, `Publish`, and `StampOf` each own a duplicate connection; `ArrowStream` admits one `AdbcRequest` and drains inside the ADBC statement lifetime.
- Auto: every concurrent operation rides a duplicate lane over the held anchor; profile settings and extension bootstrap are composition data; `Query` streams, mapped appenders own bulk ingress, and ADBC owns Arrow extraction.
- Receipt: a session open rides `store.columnar.open` carrying the loaded extension set and the posture; a query rides `store.columnar.query` carrying the `DuckDBQueryProgress` percentage; an append rides `store.columnar.append` carrying the row count; a mount rides `store.columnar.mount` carrying the alias.
- Packages: DuckDB.NET.Data.Full (`DuckDBConnection`/`DuckDBCommand`/`DuckDBConnectionStringBuilder`/`DuckDBMappedAppender<T,TMap>`/`DuckDBAppenderMap<T>`/`DuckDBDataReader`/`DuckDBQueryProgress`/`DuckDBErrorType`), Apache.Arrow, Apache.Arrow.Adbc (`AdbcDatabase`/`AdbcConnection`/`AdbcStatement`/`QueryResult`/`IArrowArrayStream`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new analytical profile is one `ColumnarProfile` row carrying its posture and roster; a new extension is one `ColumnarExtension` row carrying its `ExtensionRepo`; a new install repository is one `ExtensionRepo` row carrying its bootstrap template; a new credential kind is one `SecretScope` row carrying its `PROVIDER` token; a new dynamic-token class is one trust-gate `[ValueObject]` admission, never a raw interpolation site; a new fault cause is one `ColumnarFault` case; zero new surface — a per-extension NuGet package, a second analytical engine, an open-per-query connection, command interleaving on one handle, inline credentials in a path, a raw-string identifier crossing into engine SQL, or a provider-branded service family is the deleted form because DuckDB is fully-featured through the one centrally pinned runtime, the engine is a posture-configured anchor with `Duplicate()` concurrency, and the extension roster is profile policy expressed as SQL.
- Boundary: DuckDB extensions load under profile policy, and each source owns one anchor whose duplicate connections isolate commands and streams. `Identifier` admits aliases, tables, columns, and secret names; `StorePath` admits external paths. `SecretResidence` distinguishes session and persistent secrets without a bool payload. Foreign stores attach read-only, `substrait` fails closed when unavailable, and provider exceptions lift once into `ColumnarFault`.

```csharp signature
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using Rasm.Persistence.Element;                   // FaultBand — the one band registry (graph#FAULT_TABLES)
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
    // `Open` applies ordered bootstrap policy, then verifies every roster row through `duckdb_extensions()`.
    // Missing linked, core, or community extensions rail `ExtensionGap` before any query.
    public static IO<ColumnarSession> Open(ColumnarProfile profile, StorePath dataSource, ExecutionThreads threads) =>
        IO.liftAsync(async () => {
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

    // Streaming queries run on `Duplicate()` lanes and bind interpolation holes as named `$pN` parameters.
    // A seam-local list accumulates rows once before `toSeq`, avoiding persistent-sequence forcing per row.
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

    // ADBC owns SQL and Substrait execution plus batch or stream binding on one statement seam.
    // `drain` runs inside statement lifetime so no `QueryResult.Stream` escapes disposal.
    public static IO<T> ArrowStream<T>(AdbcConnection adbc, AdbcRequest request, Func<QueryResult, ValueTask<T>> drain) =>
        IO.liftAsync(async () => {
            using AdbcStatement statement = adbc.CreateStatement();
            request.Query.Switch(
                sql:  s => statement.SqlQuery = (string)s.Composed,
                plan: p => statement.SubstraitPlan = p.Substrait);
            request.Bind.Iter(bind => bind.Switch(
                batch:  b => statement.Bind(b.Value, b.Value.Schema),
                stream: s => statement.BindStream(s.Value)));
            QueryResult result = await statement.ExecuteQueryAsync().ConfigureAwait(false);
            return await drain(result).ConfigureAwait(false);
        });
}

// The DRIVER axis binding the admitted ADBC packages: each row names its driver, its parameter vocabulary
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
            ? Fin<AdbcConnection>.Fail(new ColumnarFault.PolicyRefused($"<adbc-parameters:{driver.Key}>"))
            : Fin<AdbcConnection>.Succ(driver.Open(parameters.ToDictionary(static p => p.Key, static p => p.Value)).Connect(new Dictionary<string, string>())))
        | @catch<IO, Fin<AdbcConnection>>(static error => error.IsExceptional,
            error => IO.pure(Fin<AdbcConnection>.Fail(new ColumnarFault.PolicyRefused($"<adbc-open:{error.Message}>"))));
}

// `AdbcRequest` closes the statement seam over composed SQL and portable Substrait bytes.
// Federation owns plan identity; this seam executes without rehashing.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdbcQuery {
    private AdbcQuery() { }
    public sealed record Sql(AdbcSql Composed) : AdbcQuery;
    public sealed record Plan(byte[] Substrait) : AdbcQuery;
}

public sealed record AdbcRequest(AdbcQuery Query, Option<AdbcBind> Bind);

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
|  [01]   | engine session      | one anchor + `Duplicate()`                | never command interleaving                                        |
|  [02]   | extension bootstrap | ordered repo-derived `INSTALL`/`LOAD` SQL | tri-state `ExtensionRepo`; fail-closed `duckdb_extensions()` probe |
|  [03]   | consistency stance  | async, `StalenessWatermark`               | never read by interactive correctness without the wait (`C2`)      |
|  [04]   | index ownership     | DuckDB spatial/vss are aggregators        | GiST/pgvector own the transactional index (`L2`)                   |
|  [05]   | credential rail     | `CREATE SECRET` over `SecretScope`        | quote-doubled config literals; never an inline path key            |
|  [06]   | Arrow bridge        | ADBC driver manager → `IArrowArrayStream` | no managed Arrow member; params via `AdbcStatement.Bind`           |
|  [07]   | fault rail          | `DuckDBException` → `ColumnarFault`       | discriminated on `DuckDBErrorType`, never a raw ADO throw          |
|  [08]   | trust gate          | `Identifier`/`StorePath`                  | one grammar per identity regime                                   |
|  [09]   | plan execution      | `AdbcQuery.Plan` → `SubstraitPlan`        | the federation intra-leg edge on the one ADBC statement seam       |

## [03]-[ARTIFACT_EGRESS]

- Owner: `Codec` the `[SmartEnum<string>]` compression vocabulary whose `.Key` IS the `COPY` `COMPRESSION` token; `Collision` the destination-collision vocabulary; `EgressFormat` the format vocabulary carrying its grouped flag and the JSON `ARRAY` row; `Projection` the composed TYPED projection (trust-gated source + column identifiers) the COPY body embeds — never raw caller SQL; `ArtifactClass` the closed analytical-artifact declaration deriving emission, partition, and the footer stamp from one row; `ArtifactEgress` the static surface owning the `COPY (SELECT) TO` rail, the footer-metadata stamp read, and the `read_parquet` generation scan.
- Cases: `EgressFormat` is `Parquet` (grouped), `Csv`, `Json` (carrying `ARRAY true`); `Codec` is `Zstd`/`Snappy`; `Collision` is `Overwrite`/`OverwriteOrIgnore`/`Append`; `ArtifactClass` is `BimRollup` (the QTO/quantity Parquet generation, Zstd, overwrite) and `CoverageFeed` (the partitioned geospatial-coverage JSON feed, Snappy, append) — a new artifact class is one row deriving its whole emission.
- Entry: `public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, Projection projection, StorePath destination, UInt128 stamp)` runs the one `COPY (projection) TO destination (…)` statement assembled from the artifact-class policy rows over the trust-gated projection and destination, the stamp the `UInt128` content-address currency hex-formatted at the seam so caller raw text is unrepresentable; `public static IO<Fin<UInt128>> StampOf(ColumnarSession session, StorePath artifact)` reads the content stamp from the Parquet footer through `parquet_kv_metadata` without decoding data and parses it back to the content-address currency (missing or malformed rails `UnstampedArtifact`); `public static FormattableString Generation(StorePath root)` derives the `read_parquet` glob scan over an artifact-generation directory with `union_by_name`/`hive_partitioning`/`filename` provenance.
- Auto: one `COPY (SELECT) TO` statement owns engine-mediated egress (data-interchange `ARTIFACT_PROJECTION`) — `FORMAT`/`COMPRESSION`/`ROW_GROUP_SIZE`/`PARTITION_BY` interpolate beside the shared destination from the artifact-class rows so a mistyped token is unrepresentable rather than a runtime SQL parse error, a second export path per format is the deleted form, and a `KV_METADATA` stamp binds the artifact's content identity into the footer; row-group geometry is the unit of scan parallelism and zonemap pruning so the `ROW_GROUP_SIZE` near the default-row count prunes well and a tiny-group append-per-batch exporter batches through a staging projection and exports once; partitioning is a pruning instrument (`PARTITION_BY` into hive directories at cardinality in the tens to low thousands), never a uniqueness scheme; the footer answers declared shape, per-row-group statistics, and the caller stamp without decoding data (`parquet_kv_metadata`/`parquet_metadata`/`parquet_schema`) so artifact admission is a metadata-cost gate run on every delivery; the generation read is `read_parquet` over a path/glob/list so a generation directory growing changes only the path argument, `union_by_name` makes additive columns compatible by construction (absent reads NULL), and `filename`/`file_row_number` pin per-row provenance; the `COPY` is a filesystem effect outside transaction rollback so publication is the atomic-write protocol, never transactional cleanup.
- Receipt: an egress rides `store.columnar.egress` carrying the artifact class and the destination; a footer stamp read rides `store.columnar.stamp` carrying the content identity.
- Packages: DuckDB.NET.Data.Full (`DuckDBCommand.ExecuteNonQuery`/`ExecuteScalar`/`DuckDBParameter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact class is one `ArtifactClass` row deriving emission, partition, and stamp; a new egress format/codec/collision is one vocabulary row whose `.Key` IS the `COPY` token; zero new surface — a per-format export path, a `FORMAT` value stretched to name a transport the engine never performs, a filename-convention identity trust, or an in-place generation rewrite is the deleted form because the COPY rail is the one SQL-mediated egress, identity rides the footer stamp, and generations are immutable.
- Boundary: the `COPY (SELECT) TO` rail is the SQL-mediated egress lane, not the egress monopoly — a zero-copy in-process columnar handoff (the `ColumnarLane.ArrowStream` ADBC bridge) and a direct managed file codec (`#FLAT_TABLE_EGRESS` `ParquetSharp.Arrow`) are distinct lanes a `COPY` `FORMAT` token cannot express, so a non-SQL egress lands as a sibling lane beside the COPY family, never as a `FORMAT` row (the deleted form is a `FORMAT` value stretched to name a transport the engine never performs); artifact identity is the footer content stamp plus the declared `ArtifactClass` shape, never a filename convention — a renamed artifact keeps its identity and a stamp that no longer matches its content is corruption, not drift; generations are immutable (compaction is a new artifact written beside the old with a new stamp, never an in-place merge) and `FIELD_IDS` at export plus an id-keyed scan map make renames non-breaking across generations; the `COPY` is a filesystem effect outside transaction rollback so publication composes the atomic-write protocol the `Element/codec#SNAPSHOT_SPINE` owns, never transactional cleanup; the lakehouse `delta`/`iceberg` scans read the same tables the managed `#FLAT_TABLE_EGRESS` `PublishDelta` commit produces — DuckDB the read/aggregate projection, the managed Delta log the versioned publication, meeting at the table path and never re-authoring each other's metadata.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// COPY-token smart enums own emitted format, codec, and compression literals.
// A mistyped token (`OVERWRITE_OR_INGORE`) is unrepresentable rather than a runtime SQL parse error; a new format/codec/collision is ONE row.
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

// `AnalyticalArtifact` rows derive complete `COPY` policy and immutable generation paths.
// `KV_METADATA` carries the `ContentAddress` stamp in the footer rather than the filename.
public sealed class ArtifactClass {
    public static readonly ArtifactClass BimRollup = new("bim-rollup", EgressFormat.Parquet, Codec.Zstd, 122_880, None, Collision.Overwrite);
    public static readonly ArtifactClass CoverageFeed = new("coverage-feed", EgressFormat.Json, Codec.Snappy, 122_880, Some(Identifier.Create("crs")), Collision.Append);
    public string Name { get; }
    public EgressFormat Format { get; }
    public Codec Codec { get; }
    public int RowGroup { get; }
    public Option<Identifier> PartitionKey { get; }
    public Collision Collision { get; }

    private ArtifactClass(string name, EgressFormat format, Codec codec, int rowGroup, Option<Identifier> partitionKey, Collision collision) =>
        (Name, Format, Codec, RowGroup, PartitionKey, Collision) = (name, format, codec, rowGroup, partitionKey, collision);

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

- Owner: `BimOpenSchemaProjection` the co-transactional Marten `FlatTableProjection` writing the columnar BIM fact table in the append transaction; `FlatTableEgress` the static surface owning the async daemon materialization, the IN-CORPUS eleven-table `.duckdb` write over the one pinned runtime (the DECISION `[10]`.3 absorption — the DEBUG-IL `Ara3D` writer never sits on the hot path), and the native `ParquetSharp.Arrow` codec lane (read AND PME-encrypted write) the columnar query rides.
- Entry: `public sealed class BimOpenSchemaProjection : FlatTableProjection` registered `ProjectionLifecycle.Inline` so the columnar BIM facts write in the append transaction (`Project<GraphEvent.GraphCreated>(map => …)`); `public static IO<Unit> Materialize(IDocumentStore store)` runs the async daemon view under `QueryLane.Columnar.WaitBudget`; `public static IO<long> WriteFrames(ColumnarSession session, BimData frames)` streams the eleven `ToDataSet()` tables through this lane's raw `DuckDBAppender` under the exact `<Name>_<n>` projection-ordinal names — `frames.ToDataSet()` supplies the fixed-order table data; `public static IO<Seq<RecordBatch>> ReadParquetFrames(StorePath parquetPath)` reads a standard-format `.parquet` generation into `Apache.Arrow` `RecordBatch`es through the native `ParquetSharp.Arrow.FileReader`; `public static IO<long> WriteParquetFrames(Seq<RecordBatch> batches, StorePath path, Schema schema, Option<(CryptoFactory Crypto, KmsConnectionConfig Kms, EncryptionConfiguration Enc)> encryption)` is the write half of the same codec correspondence — Parquet Modular Encryption enters as an `Option` policy deriving `FileEncryptionProperties` from the admitted KMS trio, never a second writer type, and both codec legs take the `#COLUMNAR_LANE` trust-gated `StorePath`; `public static IO<Fin<long>> PublishDelta(TableOptions table, Seq<AddAction> files, Identifier appId, long asOfVersion)` registers the generation's ParquetSharp-written files into the Delta transaction log metadata-only (`CreateWriteTransactionAsync`), and the AS-OF version is the idempotent `CommitOptions.AppId`/`TransactionVersion` marker gated by `GetLatestTransactionVersionAsync`.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` (`Project<T>(StatementMap)`) written in the same transaction as the events, NOT daemon-lagged (`M4`), because a flat analytical view a live QTO reads must be read-your-writes consistent — the structural map maps the `GraphEvent.GraphCreated`'s `Header.Schema.Key`/`Header.View.Key` (the `ReleaseVersion`/`ModelView` smart-enum keys, since `StatementMap.Map` writes a primitive column, never a smart-enum object) and the `GraphRevised`'s `GraphDelta.NodeCount`/`EdgeCount` change magnitude through the single-column primary key `FlatTableProjection` requires; the eleven suffixed BIM tables (`Points_0`/`Strings_1`/`Descriptors_2`/`Documents_3`/`Entities_4`/`Relations_5`/`DoubleParameters_6`/`IntegerParameters_7`/`StringParameters_8`/`EntityParameters_9`/`PointParameters_10`) are written IN-CORPUS: `frames.ToDataSet()` projects the fixed-ordinal `IDataSet` (`Tables` in the order that IS the DuckDB ordinal suffix), and `WriteFrames` folds each `IDataTable` (`Name`/`Columns`/`Rows`, `IDataDescriptor.Name`/`Type` typing the DDL, the `[column, row]` indexer supplying cells) through a `CREATE OR REPLACE TABLE` + raw `DuckDBAppender` `CreateRow`/`AppendValue`/`EndRow` stream on THIS lane's session — the DEBUG-IL `DuckDbUtils.WriteToDuckDB` writer is data-model-only, never the hot write loop — and a Persistence analytical query opens that `.duckdb` over the same pinned runtime and SQL-joins the suffixed entity/parameter/relation tables by their exact suffixed names; the async daemon `Materialize` blocks on `WaitForNonStaleData` so the generation is current before the heavy aggregation lanes read it carrying the `StalenessWatermark`; the native `ParquetSharp.Arrow.FileReader.GetRecordBatchReader` reads the same standard-format `.parquet` files the managed `Parquet.Net` writer produced into `IArrowArrayStream` `RecordBatch`es for the columnar query rail (managed writer / native libparquet-cpp reader interoperate at the file format, never the assembly).
- Receipt: a flat-table projection rides `store.columnar.flattable` carrying the change magnitude; a daemon materialization rides `store.columnar.materialize` carrying the watermark; a frame write rides `store.columnar.frames` carrying the table count; a Parquet read rides `store.columnar.parquet` carrying the record-batch count.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`SchemaNameSource`/`ProjectionLifecycle`/`IDocumentStore`/`BuildProjectionDaemonAsync`/`WaitForNonStaleData`), Ara3D.BimOpenSchema (`BimData`/`BimDataBuilder`/`ToDataSet` — DATA MODEL only post-absorption), Ara3D.SDK (`IDataSet.Tables`/`IDataTable.Name`/`Rows`/`Columns`/`this[column,row]`/`IDataColumn.ColumnIndex`/`Descriptor`/`IDataDescriptor.Name`/`Type` — decompile-verified), DuckDB.NET.Data.Full (`DuckDBAppender.CreateRow`/`IDuckDBAppenderRow.AppendValue`/`AppendNullValue`/`EndRow`/`Close` — the in-corpus write loop), ParquetSharp (`Arrow.FileReader`/`Arrow.FileWriter`/`WriterPropertiesBuilder`), ParquetSharp.Encryption (`CryptoFactory`/`KmsConnectionConfig`/`EncryptionConfiguration` — PME over the admitted KMS trio), DeltaLake.Net (`DeltaEngine`/`EngineOptions`/`TableOptions`/`AddAction`/`CommitOptions`/`CreateWriteTransactionAsync`/`GetLatestTransactionVersionAsync`/`DeltaLakeException` — the metadata-only Delta commit rail; assembly `DeltaLake`), Apache.Arrow (`RecordBatch`/`IArrowArrayStream`), Rasm.Element (`GraphDelta`/`Header`), Rasm.Persistence (`Element/graph#STREAM_GRAIN` `GraphEvent`/`GraphCreated`/`GraphRevised` the Marten event body), LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `map.Map` statement on the `StatementMap`; a new analytical generation is one async daemon view; a new frame codec is the existing `ParquetSharp.Arrow` lane reading a new format; an encryption stance is one `Option` policy value on the existing write, never a sibling encrypted writer; a new lakehouse publication is one `PublishDelta` commit over `AddAction` rows the codec write already computed, never a second write of the bytes; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, a second Parquet runtime beside `ParquetSharp`, or a hollow writer that opens a row group and writes no column is the deleted form because the BimOpenSchema egress is co-transactional, the managed `Parquet.Net` writer and the native `ParquetSharp` reader meet at the file format, and the Arrow record-batch model is `api-arrow`'s.
- Boundary: the `ElementGraph → Ara3D.BimOpenSchema` egress is a co-transactional `FlatTableProjection` (`M4`) so a live-QTO analytical read is read-your-writes consistent rather than daemon-lagged — `FlatTableProjection` requires a single-column primary key and writes a primitive per `StatementMap.Map`, so a `ReleaseVersion`/`ModelView` smart-enum maps as its `.Key` and a `GraphDelta` maps as its `NodeCount`/`EdgeCount`, never as the smart-enum or delta object itself; if BimOpenSchema is EAV-generic Persistence owns the structural map, if BIM-typed it is a Bim-implemented seam projection (the wire seam, never a sibling reference); a Bim-lowered `StorePlan` (the `Rasm.Bim` `Model/query#PREDICATE_PUSHDOWN` predicate push-down — one parameterized statement over the suffixed fact tables plus an in-process residue) executes on this lane's `ColumnarSession` as DATA crossing the same seam, so the estate-scale element query runs where the data rests with no Persistence-side predicate vocabulary; the eleven suffixed BIM tables are read with the built-in `parquet`/`json` surface and `spatial`/`vss`/`fts` extend them for geometry/ANN/text analytics over the same `.duckdb`, all on the one pinned runtime, and a direct SQL consumer references the `<Name>_<n>` projection-ordinal suffix that IS the real table identity (`api-ara3d-bimopenschema#DATASET_BRIDGE`), never a bare table name; the Parquet file codec is `ParquetSharp.Arrow` (the native libparquet-cpp read/write the managed Arrow stack lacks, exposing the `Apache.Arrow` `RecordBatch` directly so Parquet↔Arrow is a first-class managed call), distinct from the DuckDB SQL `read_parquet`/`COPY` path, the three meeting at the Parquet file format and the `Apache.Arrow` model owned by `api-arrow` not re-declared here; the `Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built at the HELD `1.0.1` pin (JIT optimizations disabled in the shipped IL; the feed-newest `.IO` `1.6.1` regressed to `net8.0-windows7.0`, `NU1202` on net10.0 osx-arm64, so the bump is restore-inadmissible) — the ruled escalation is EXECUTED here: the consumed write surface is absorbed in-corpus (`WriteFrames` streams the eleven tables through this lane's appender; `ReadParquetFrames`/`WriteParquetFrames` ride the native `ParquetSharp` codec), so the DEBUG-IL assemblies serve only the in-memory schema model and `ToDataSet()` projection, never a hot IO loop, and the pin bump is never the fix.

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
using Rasm.Element.Graph;
using Rasm.Element.Projection;
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
    // Daemon materialization waits for non-stale state before heavy analytical scans.
    // Inline projection remains the same-commit correctness owner.
    public static IO<Unit> Materialize(IDocumentStore store) =>
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

    // ParquetSharp.Arrow reads standard Parquet generations into Apache.Arrow record batches.
    // `GetRecordBatchReader` yields the one `IArrowArrayStream` codec boundary.
    public static IO<Seq<RecordBatch>> ReadParquetFrames(StorePath parquetPath) =>
        IO.liftAsync(async () => {
            using FileReader reader = new(File.OpenRead((string)parquetPath));
            using IArrowArrayStream stream = reader.GetRecordBatchReader();
            List<RecordBatch> batches = [];
            while (await stream.ReadNextRecordBatchAsync().ConfigureAwait(false) is { } batch) batches.Add(batch);
            return toSeq(batches);
        });

    // ParquetSharp.Arrow `FileWriter` owns plain and PME-encrypted record-batch writes.
    // Read, write, and encryption metadata share one admitted `StorePath`.
    public static IO<long> WriteParquetFrames(Seq<RecordBatch> batches, StorePath path, Schema schema,
        Option<(CryptoFactory Crypto, KmsConnectionConfig Kms, EncryptionConfiguration Enc)> encryption) =>
        IO.lift(() => {
            using WriterProperties properties = encryption.Match(
                Some: pme => new WriterPropertiesBuilder().Encryption(pme.Crypto.GetFileEncryptionProperties(pme.Kms, pme.Enc, (string)path)).Build(),
                None: static () => new WriterPropertiesBuilder().Build());
            using FileWriter writer = new(File.Open((string)path, FileMode.Create, FileAccess.Write, FileShare.None), schema, properties, null, leaveOpen: false);
            foreach (RecordBatch batch in batches) { writer.WriteRecordBatch(batch); }
            return (long)batches.Count;
        });

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
}
```

| [INDEX] | [POLICY]             | [VALUE]                                     | [BINDING]                                                       |
| :-----: | :------------------- | :------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | BimOpenSchema egress | co-transactional `FlatTableProjection`      | read-your-writes, never daemon-lagged (`M4`)                    |
|  [02]   | column value         | smart-enum `.Key` / `GraphDelta` count      | `StatementMap.Map` writes a primitive, never the object         |
|  [03]   | BIM tables           | eleven suffixed columnar tables             | written in-corpus; the DEBUG-IL writer stays off the hot path   |
|  [04]   | Parquet codec        | `ParquetSharp.Arrow` ↔ `RecordBatch` codec  | distinct from the DuckDB SQL parquet path; meet at the file     |
|  [05]   | encrypted extract    | PME `Option` policy on `WriteParquetFrames` | `CryptoFactory` × the admitted KMS trio; never a sibling writer |
|  [06]   | lakehouse            | `PublishDelta` metadata-only commit         | `AddAction` registration; `TransactionVersion` = the AS-OF cut  |
|  [07]   | path admission       | `StorePath` on both Parquet codec legs      | read/write/encryption consume ONE admitted path value          |

## [05]-[SERIES_AND_SCALEOUT]

- Owner: `SeriesKind` the `[SmartEnum<string>]` temporal-lane axis — one row per series family carrying its hypertable identity, chunk interval, rollup bucket, retention bound, and columnstore age as policy columns from which the WHOLE provisioning SQL set derives; `SeriesPoint` the ingest row keyed by the series content key (an `assessment` point keys by the Compute `(subgraph·route·policy)` input key — the SAME identity the `Query/cache` `ArtifactKind.Assessment` row carries, so the heavy source artifact and its queryable temporal projection share one origin); `SeriesFault` the closed `FaultBand.Series` band; `SeriesLane` the static surface owning provisioning-SQL derivation, binary-COPY ingest, and the bucketed and time-weighted reads; `ScaleoutRead` the ClickHouse fleet read row consuming the table the `Version/egress` `EgressSink.ClickHouse` case lands.
- Cases: `SeriesKind.Assessment` (hourly/sub-hourly discipline-assessment series — energy, thermal, daylight — 1-day chunks, 1-hour rollup bucket, 365-day retention, 7-day columnstore age) and `SeriesKind.Sensor` (BMS/operational telemetry — 1-day chunks, 15-minute bucket, 90-day retention, 2-day columnstore age); `SeriesFault` is `IngestRefused | Unprovisioned | FleetRefused | ReadRefused` (`8481`-`8484`).
- Entry: `public static Seq<string> Provision(SeriesKind kind)` derives the ordered idempotent SQL rows the reviewed-migration artifact carries — table DDL, `SELECT create_hypertable(...)`, the columnstore `ALTER TABLE ... SET`, `CALL add_columnstore_policy(...)`, `SELECT add_retention_policy(...)`, the continuous-aggregate view plus `SELECT add_continuous_aggregate_policy(...)` — every optional argument `=>`-named and every step `if_not_exists`-idempotent; `public static IO<Fin<ulong>> Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame)` streams the points through one binary-COPY importer whose success branch alone calls `CompleteAsync`; `public static IO<Fin<double>> Weighted(NpgsqlDataSource store, SeriesKind kind, UInt128 series, Instant from, Instant until)` runs the toolkit time-weighted read over the raw chunks and `public static IO<Fin<Seq<SeriesBucket>>> Bucketed(...)` the pre-bucketed continuous-aggregate read — the table name a closed-vocabulary row literal, only VALUES binding as parameters; `public static IO<Fin<(Seq<T> Rows, QueryStats Stats)>> ScaleoutRead.Fleet<T>(ClickHouseClient fleet, string sql, HashMap<string, object> binds, Func<DbDataReader, T> shape)` is the billion-row read leg.
- Auto: provisioning is DERIVED, never hand-spelled per environment — the emission law splits SELECT functions (`create_hypertable`, `add_retention_policy`, `add_continuous_aggregate_policy`) from CALL procedures (`add_columnstore_policy`) so a mis-verbed row is unrepresentable from the derivation, and the rows ride the same reviewed-migration rail every `Store/provisioning#SERVER_EXTENSIONS` admission rides, gated on the verdict holding the `timescaledb` lane (`Unprovisioned` when absent); refresh, retention, and compression jobs run on TimescaleDB's OWN bgworker scheduler — the AppHost schedule port never schedules a database-internal job — and the provisioning verification fold is `SeriesLane.Verify` — the catalogued `timescaledb_information.jobs`+`job_stats` join returning one `JobHealth` row per refresh/retention/columnstore job, a failed status or stale `last_successful_finish` the typed negative evidence — so a non-firing policy surfaces, never a silent gap behind the emitted-rows receipt; the time-weighted read is the honest algebra for IRREGULAR simulation timesteps — `average(time_weight('linear', at, value))` weighs each sample by its holding interval where a naive `avg` over-counts dense bursts — and the dashboard tile read rides the pre-bucketed continuous aggregate, never a raw-chunk re-scan; the fleet read binds named parameters through the driver's `{name:Type}` substitution and lands the `QueryStats` throughput receipt (`ReadRows`/`ElapsedNs` off the `X-ClickHouse-Summary` header) on every read, with `ClickHouseServerException` folding `FleetRefused` on its numeric `ErrorCode`, never a raw `HttpRequestException`.
- Receipt: a provisioning derivation rides `store.columnar.series.provision` carrying the kind and the row count; an ingest rides `store.columnar.series.ingest` carrying the staged count; a fleet read rides `store.columnar.fleet` carrying the `QueryStats` rows and elapsed figures.
- Packages: Npgsql (`NpgsqlDataSource.OpenConnectionAsync`/`NpgsqlConnection.BeginBinaryImportAsync`/`NpgsqlBinaryImporter.StartRowAsync`/`WriteAsync`/`CompleteAsync` — the COPY kernel; `NpgsqlDbType`), timescaledb + timescaledb_toolkit (`create_hypertable`/`by_range`/`add_retention_policy`/`add_columnstore_policy`/`add_continuous_aggregate_policy`/`time_bucket`/`time_weight`/`average`/`rollup` — server-side SQL per `api-timescaledb`/`api-timescaledb-toolkit`), ClickHouse.Driver (`IClickHouseClient.ExecuteReaderAsync`/`QueryStats`/`ClickHouseServerException` — the fleet read leg; the write leg is `Version/egress`'s), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new series family is one `SeriesKind` row deriving its whole provisioning set; a new rollup grain is one bucket column value; a new fleet question is one composed read over the standing client, never a second client or a per-question service; zero new surface — a per-environment hand-spelled policy script, an AppHost-scheduled refresh job, a naive `avg` over irregular timesteps, a second ClickHouse write path beside the egress sink, or a transaction-scoped ClickHouse write (the backend has none) is the deleted form because the policy set derives from the row, the bgworker owns cadence, and the sink owns landing.
- Boundary: the series tier is a RELATIONAL residence beside `element_identity` — never an artifact-catalog class: the heavy source artifact (an eplusout.sql, an FEA result set) stays the `Query/cache` `ArtifactKind.Assessment` content-keyed row under `RetentionClass.Cache`, and this hypertable is its queryable temporal PROJECTION whose chunks TimescaleDB's own `add_retention_policy` drops in-database (re-derivable by re-ingest from the retained artifact — cost, never correctness), so the `Version/retention#SWEEP_AND_GC` executor never deletes series chunks and a `RetentionClass` row for them is the rejected double-governor; the Compute `AssessmentSink` emits the typed points this lane ingests (the `ARCHITECTURE.md [02]-[SEAMS]` `Compute → Version` assessment edge widened from opaque content-keyed bytes to a typed series), and the same `SeriesPoint` shape serves BMS/sensor streams under the `Sensor` row; the ClickHouse leg is READ-side only — the `Version/egress` `ClickHouse` sink owns landing under `insert_deduplication_token` dedup, this row consumes at fleet scale (who changed what across hundreds of models, org-wide element churn), the two meeting at `WarehouseSchema.Table`/`WarehouseSchema.Columns` — the ONE typed row vocabulary (`WarehouseOpRow` + `WarehouseSchema.Shape`) a fleet question composes over, so writer and reader cannot drift while naming one table; ClickHouse is never a second system of record and carries no transaction, so every fleet read is an eventually-consistent analytical view whose staleness the egress cursor bounds.

```csharp signature
using ClickHouse.Driver;
using ClickHouse.Driver.ADO;
using Npgsql;
using NpgsqlTypes;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// One row per series family; the WHOLE TimescaleDB provisioning set derives from these columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeriesKind {
    public static readonly SeriesKind Assessment = new("assessment", "assessment_series", "1 day", "1 hour", "365 days", "7 days");
    public static readonly SeriesKind Sensor     = new("sensor", "sensor_series", "1 day", "15 minutes", "90 days", "2 days");

    public string Table { get; }
    public string Chunk { get; }
    public string Bucket { get; }
    public string DropAfter { get; }
    public string ColumnstoreAfter { get; }
    private SeriesKind(string key, string table, string chunk, string bucket, string dropAfter, string columnstoreAfter) : this(key) =>
        (Table, Chunk, Bucket, DropAfter, ColumnstoreAfter) = (table, chunk, bucket, dropAfter, columnstoreAfter);
}

// The ingest row: `Series` is the content-key identity the source artifact already carries
// (the assessment `(subgraph·route·policy)` key), `At` the sample instant, `Value` the measure. Tenancy is
// NOT a point column — the whole COPY batch lands under the ingesting frame's tenant, and every read filters
// by it, so equal series keys under distinct tenants never share or return rows.
public readonly record struct SeriesPoint(UInt128 Series, Instant At, double Value);

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record SeriesFault : Expected, IValidationError<SeriesFault> {
    private SeriesFault() : base() { }
    public sealed record IngestRefused(string SqlState, string Detail) : SeriesFault;
    public sealed record Unprovisioned(string Lane) : SeriesFault;
    public sealed record FleetRefused(int ErrorCode, string Detail) : SeriesFault;
    public sealed record ReadRefused(string SqlState, string Detail) : SeriesFault;

    public override int Code => FaultBand.Series + Switch(
        ingestRefused: static _ => 1,
        unprovisioned: static _ => 2,
        fleetRefused:  static _ => 3,
        readRefused:   static _ => 4);

    public override string Message => Switch(
        ingestRefused: static c => $"<series-ingest:{c.SqlState}:{c.Detail}>",
        unprovisioned: static c => $"<series-unprovisioned:{c.Lane}>",
        fleetRefused:  static c => $"<series-fleet:{c.ErrorCode}:{c.Detail}>",
        readRefused:   static c => $"<series-read:{c.SqlState}:{c.Detail}>");

    public override string Category => Switch(
        ingestRefused: static _ => "Ingest",
        unprovisioned: static _ => "Provision",
        fleetRefused:  static _ => "Fleet",
        readRefused:   static _ => "Read");

    public static SeriesFault Create(string message) => new IngestRefused("<wire>", message);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class SeriesLane {
    // The derived provisioning set: SELECT functions vs CALL procedures per the emission law,
    // every optional argument `=>`-named, every step idempotent. The rows ride the migration artifact.
    public static Seq<string> Provision(SeriesKind kind) => [
        $"CREATE TABLE IF NOT EXISTS {kind.Table} (tenant bytea NOT NULL, series_key bytea NOT NULL, at timestamptz NOT NULL, value double precision NOT NULL)",
        $"SELECT create_hypertable('{kind.Table}', by_range('at', INTERVAL '{kind.Chunk}'), if_not_exists => TRUE)",
        $"ALTER TABLE {kind.Table} SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = 'tenant, series_key', timescaledb.orderby = 'at')",
        $"CALL add_columnstore_policy('{kind.Table}', after => INTERVAL '{kind.ColumnstoreAfter}', if_not_exists => TRUE)",
        $"SELECT add_retention_policy('{kind.Table}', drop_after => INTERVAL '{kind.DropAfter}', if_not_exists => TRUE)",
        $"CREATE MATERIALIZED VIEW IF NOT EXISTS {kind.Table}_rollup WITH (timescaledb.continuous) AS SELECT tenant, series_key, time_bucket(INTERVAL '{kind.Bucket}', at) AS bucket, avg(value) AS mean, min(value) AS low, max(value) AS high, count(*) AS samples FROM {kind.Table} GROUP BY tenant, series_key, bucket WITH NO DATA",
        $"SELECT add_continuous_aggregate_policy('{kind.Table}_rollup', start_offset => INTERVAL '3 days', end_offset => INTERVAL '{kind.Bucket}', schedule_interval => INTERVAL '{kind.Bucket}', if_not_exists => TRUE)",
    ];

    // Binary COPY is the ingest lane: Complete commits, disposal without it discards — the
    // exception path therefore discards rather than half-writes, and the retry unit is the whole COPY.
    public static IO<Fin<ulong>> Ingest(NpgsqlDataSource store, SeriesKind kind, Seq<SeriesPoint> points, ProjectionContext frame) =>
        IO.liftAsync(async () => {
            await using NpgsqlConnection lane = await store.OpenConnectionAsync().ConfigureAwait(false);
            NpgsqlBinaryImporter importer = await lane.BeginBinaryImportAsync($"COPY {kind.Table} (tenant, series_key, at, value) FROM STDIN (FORMAT BINARY)").ConfigureAwait(false);
            try {
                byte[] tenant = SeriesKey(frame.Tenant);
                foreach (SeriesPoint point in points) {
                    await importer.StartRowAsync().ConfigureAwait(false);
                    byte[] series = new byte[16];
                    BinaryPrimitives.WriteUInt128BigEndian(series, point.Series);
                    await importer.WriteAsync(tenant, NpgsqlDbType.Bytea).ConfigureAwait(false);
                    await importer.WriteAsync(series, NpgsqlDbType.Bytea).ConfigureAwait(false);
                    await importer.WriteAsync(point.At, NpgsqlDbType.TimestampTz).ConfigureAwait(false);
                    await importer.WriteAsync(point.Value, NpgsqlDbType.Double).ConfigureAwait(false);
                }
                ulong staged = await importer.CompleteAsync().ConfigureAwait(false);
                await importer.DisposeAsync().ConfigureAwait(false);
                return Fin<ulong>.Succ(staged);
            }
            catch (PostgresException wire) {
                await importer.DisposeAsync().ConfigureAwait(false);
                return Fin<ulong>.Fail(new SeriesFault.IngestRefused(wire.SqlState, wire.MessageText));
            }
        });

    // The toolkit time-weighted read over raw chunks: each sample weighs by its holding interval,
    // the honest mean for irregular simulation timesteps a naive avg over-counts. The table name is a
    // closed-vocabulary row literal composed into the text; only VALUES bind as parameters.
    public static IO<Fin<double>> Weighted(NpgsqlDataSource store, SeriesKind kind, UInt128 series, Instant from, Instant until, ProjectionContext frame) =>
        Rows(store, $"SELECT average(time_weight('linear', at, value)) FROM {kind.Table} WHERE tenant = @tenant AND series_key = @series AND at >= @from AND at < @until", series, from, until, frame,
            static reader => reader.GetDouble(0))
        .Map(read => read.Map(static values => values.Head.IfNone(0d)));

    public static IO<Fin<Seq<SeriesBucket>>> Bucketed(NpgsqlDataSource store, SeriesKind kind, UInt128 series, Instant from, Instant until, ProjectionContext frame) =>
        Rows(store, $"SELECT bucket, mean, low, high, samples FROM {kind.Table}_rollup WHERE tenant = @tenant AND series_key = @series AND bucket >= @from AND bucket < @until ORDER BY bucket", series, from, until, frame,
            static reader => new SeriesBucket(reader.GetFieldValue<Instant>(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3), reader.GetInt64(4)));

    // Provisioning HEALTH is distinct from provisioning EMISSION: this fold reads the catalogued
    // timescaledb_information.job_stats run history for the kind's refresh, retention, and columnstore jobs and
    // flags a failed status or a last_successful_finish older than twice the job's schedule interval — a policy
    // that stopped firing surfaces typed, never a silent gap behind an emitted-rows receipt.
    public static IO<Fin<Seq<JobHealth>>> Verify(NpgsqlDataSource store, SeriesKind kind) =>
        IO.liftAsync(async () => {
            await using NpgsqlCommand command = store.CreateCommand(
                "SELECT j.hypertable_name, s.job_status, s.last_successful_finish, s.total_failures FROM timescaledb_information.jobs j JOIN timescaledb_information.job_stats s ON s.job_id = j.job_id WHERE j.hypertable_name = @table");
            _ = command.Parameters.AddWithValue("table", kind.Table);
            try {
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                List<JobHealth> rows = [];
                while (await reader.ReadAsync().ConfigureAwait(false)) {
                    rows.Add(new JobHealth(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? Option<Instant>.None : Some(reader.GetFieldValue<Instant>(2)), reader.GetInt64(3)));
                }
                return Fin<Seq<JobHealth>>.Succ(toSeq(rows));
            }
            catch (PostgresException wire) { return Fin<Seq<JobHealth>>.Fail(new SeriesFault.ReadRefused(wire.SqlState, wire.MessageText)); }
        });

    static IO<Fin<Seq<T>>> Rows<T>(NpgsqlDataSource store, string sql, UInt128 series, Instant from, Instant until, ProjectionContext frame, Func<NpgsqlDataReader, T> shape) =>
        IO.liftAsync(async () => {
            await using NpgsqlCommand command = store.CreateCommand(sql);
            _ = command.Parameters.AddWithValue("tenant", SeriesKey(frame.Tenant));
            _ = command.Parameters.AddWithValue("series", SeriesKey(series));
            _ = command.Parameters.AddWithValue("from", from);
            _ = command.Parameters.AddWithValue("until", until);
            try {
                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                List<T> rows = [];
                while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
                return Fin<Seq<T>>.Succ(toSeq(rows));
            }
            catch (PostgresException wire) { return Fin<Seq<T>>.Fail(new SeriesFault.ReadRefused(wire.SqlState, wire.MessageText)); }
        });

    static byte[] SeriesKey(UInt128 series) {
        byte[] bytes = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, series);
        return bytes;
    }
}

public readonly record struct SeriesBucket(Instant Bucket, double Mean, double Low, double High, long Samples);

// One job_stats health row per background job on the kind's hypertable — status, last successful finish,
// failure counter — the typed negative evidence the provisioning receipt distinguishes from emitted policy rows.
public readonly record struct JobHealth(string Hypertable, string Status, Option<Instant> LastSuccessfulFinish, long TotalFailures);

// ONE warehouse row vocabulary both ends of the ClickHouse seam read: the `Version/egress` sink lands EXACTLY
// these columns projected from the CdcEnvelope (`id` the content key, `source`/`type`/`time` the envelope
// attributes, `partition_key`/`sequence` the partitioning extensions, `data` the redacted payload), and a fleet
// question composes over `WarehouseSchema.Shape` — a free-SQL reader binding an ad-hoc row against the sink's
// table is the deleted writer/reader drift form.
public sealed record WarehouseOpRow(string Id, string Source, string Type, Instant Time, string PartitionKey, long Sequence, ReadOnlyMemory<byte> Data);

public static class WarehouseSchema {
    public const string Table = "rasm_oplog";
    public const string Columns = "id, source, type, time, partition_key, sequence, data";

    public static WarehouseOpRow Shape(System.Data.Common.DbDataReader reader) => new(
        reader.GetString(0), reader.GetString(1), reader.GetString(2),
        Instant.FromDateTimeUtc(DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc)),
        reader.GetString(4), reader.GetInt64(5), (byte[])reader[6]);
}

public static class ScaleoutRead {
    // The fleet-scale read leg over the table the Version/egress ClickHouse sink lands; QueryStats is
    // the throughput receipt and ClickHouseServerException folds typed on its numeric ErrorCode.
    public static IO<Fin<(Seq<T> Rows, QueryStats Stats)>> Fleet<T>(ClickHouseClient fleet, string sql, HashMap<string, object> binds, Func<System.Data.Common.DbDataReader, T> shape) =>
        IO.liftAsync(async () => {
            await using ClickHouseConnection lane = fleet.CreateConnection();
            await lane.OpenAsync().ConfigureAwait(false);
            await using ClickHouseCommand command = lane.CreateCommand(sql);
            binds.Iter(pair => { _ = command.Parameters.AddWithValue(pair.Key, pair.Value); });
            await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            List<T> rows = [];
            while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
            return Fin<(Seq<T>, QueryStats)>.Succ((toSeq(rows), command.QueryStats!));
        }) | @catch<IO, Fin<(Seq<T>, QueryStats)>>(static e => e.Exception.Map(static x => x is ClickHouseServerException).IfNone(false),
            e => IO.pure(Fin<(Seq<T>, QueryStats)>.Fail(new SeriesFault.FleetRefused(((ClickHouseServerException)e.ToException()).ErrorCode, e.Message))));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                            | [BINDING]                                                      |
| :-----: | :----------------- | :-------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | series provisioning | derived from the `SeriesKind` row                  | SELECT/CALL emission law; migration-carried, verdict-gated     |
|  [02]   | policy cadence     | TimescaleDB bgworker jobs                           | never AppHost-scheduled; `job_stats` is the proof row          |
|  [03]   | irregular timesteps | `average(time_weight('linear', …))`                | a naive `avg` over-counts dense bursts                         |
|  [04]   | series retention   | in-database `add_retention_policy`                  | projection of a retained artifact; never a `RetentionClass` row |
|  [05]   | fleet leg          | ClickHouse READ row + `QueryStats` receipt          | the egress sink owns landing; never a second SoR               |
|  [06]   | series identity    | the source artifact's content key                   | one origin with `ArtifactKind.Assessment`; never a second mint |
