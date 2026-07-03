# [PERSISTENCE_QUERY_COLUMNAR]

Rasm.Persistence runs analytical aggregation, columnar rollup, lakehouse scan, and engine-mediated artifact egress through ONE in-process DuckDB engine made fully-featured by the SQL `INSTALL`/`LOAD` extension bootstrap (`api-duckdb`), fed by two egress shapes off the Marten substrate: a CO-TRANSACTIONAL `Ara3D.BimOpenSchema` `BimOpenSchemaProjection : FlatTableProjection` that writes the BIM columnar facts in the append transaction (never daemon-lagged, per `M4`), and ASYNC daemon projections (`ProjectionLifecycle.Async`) materializing the eleven-table `.duckdb`/Parquet generations the analytical lane scans, each carrying an explicit `StalenessWatermark`. The engine is NOT a bare per-query connection: `ColumnarSession` holds ONE refcounted posture-configured anchor `DuckDBConnection` per source path for its lifetime and fans in-process concurrency through `Duplicate()` lanes, because a streaming drain occupies its connection until disposed; `ColumnarProfile` is the `[SmartEnum<string>]` analytical-lane axis carrying BOTH the four declared posture knobs (`memory_limit`/`threads`/`temp_directory`/`preserve_insertion_order`) AND the ordered extension roster a lane requires, the loaded set landing as a profile receipt through `duckdb_extensions()`. The analytical lane is the `Query/lane#READ_ROUTING` async lane, so an interactive-correctness query never reads here without `WaitForNonStaleProjectionDataAsync`; strong-consistency reads go through the synchronous `Element/graph#GRAPH_PROJECTION` inline projection and the `Query/topology#GRAPH_TOPOLOGY` view. `Ara3D.BimOpenSchema.IO`'s `BimData.WriteDuckDB` writes the `.duckdb` the analytical query reads over the SAME pinned runtime, `ParquetSharp.Arrow.FileReader`/`FileWriter` is the native columnar-file codec the managed Arrow stack lacks, the `Apache.Arrow.Adbc` driver manager is the DuckDB→`IArrowArrayStream` zero-managed-copy bridge (the v1.5.3 DuckDB surface exposes no managed Arrow member), and DuckDB `ATTACH … TYPE postgres` joins the live Marten/Npgsql database analytically without an ETL hop. `GraphDelta`/`Header`/`ElementGraph`/`Node`/`NodeId` arrive settled from `Rasm.Element`; the `GraphEvent` Marten event-body family (`GraphCreated`/`GraphRevised`/`GraphRetired`), the inline `GraphProjection`, and the `ElementJson` serializer arrive from the Persistence siblings `Element/graph`/`Element/codec` (`GraphEvent` is the Persistence-owned event body wrapping the seam `GraphDelta`, not a `Rasm.Element` type); `StalenessWatermark`/`QueryLane` arrive from `Query/lane`; `ClockPolicy`, `ReceiptSinkPort`, `CorrelationId`, `DataClassification`, `TenantContext` arrive from AppHost; `ContentAddress` arrives from `Element/codec#CONTENT_ADDRESS`.

## [01]-[INDEX]

- [01]-[COLUMNAR_LANE]: the posture-configured DuckDB anchor session, the `INSTALL`/`LOAD` extension-profile bootstrap, the parameterized query and streaming drain, the typed bulk appender, the read-only `ATTACH` mount, the `CREATE SECRET` credential rail, the ADBC Arrow bridge, and the closed fault rail.
- [02]-[ARTIFACT_EGRESS]: the engine-mediated `COPY (SELECT) TO` artifact rail over the closed `Format`/`Codec`/`Collision`/`ArtifactClass` vocabularies, the footer-metadata stamp and admission read, and the `read_parquet` generation scan.
- [03]-[FLAT_TABLE_EGRESS]: the co-transactional `Ara3D.BimOpenSchema` `FlatTableProjection`, the eleven suffixed BIM tables, the async Parquet daemon materialization, and the native `ParquetSharp.Arrow` codec lane the columnar query reads.

## [02]-[COLUMNAR_LANE]

- Owner: `ColumnarExtension` the `[SmartEnum<string>]` extension vocabulary carrying its `INSTALL`/`LOAD` SQL and statically-linked flag; `ColumnarProfile` the `[SmartEnum<string>]` analytical-lane axis carrying its posture knobs AND its required extension roster; `ColumnarSession` the refcounted posture-configured anchor over one `DuckDBConnection` per source, owning the `Duplicate()` lane fan, the read-only `ATTACH` mount, the live-query progress probe, and disposal; `ColumnarFault` the closed DuckDB-boundary fault discriminated on `DuckDBErrorType`; `ColumnarLane` the static surface owning the connection open, the ordered extension bootstrap, the parameterized query, the streaming drain, the typed bulk-appender egress, and the ADBC Arrow bridge.
- Cases: `ColumnarProfile` is `Geometry` (`spatial`/`parquet`, memory-heavy spatial aggregation posture), `Search` (`vss`/`fts`, ANN/BM25 columnar posture), `Lakehouse` (`httpfs`/`iceberg`/`delta`/`postgres`, order-free remote-scan posture with `preserve_insertion_order` false), `Bim` (`parquet`/`json` over the BimOpenSchema `.duckdb`, read-your-writes BIM analytics posture); `ColumnarExtension` rows are `Spatial`, `Vss`, `Fts`, `Parquet`, `Json`, `Httpfs`, `Iceberg`, `Delta`, `Postgres`, `Sqlite`, `Excel`, `Avro`, `Icu`, `Aws`, `Azure` — a new extension being one row carrying its SQL, never a per-extension package; `ColumnarFault` is `QueryFailed`/`ExtensionGap`/`MountRefused`/`AppendRefused`/`EgressRefused`/`UnstampedArtifact`/`SecretRefused` (the `CREATE SECRET` credential failure, distinct from `MountRefused` — a secret-creation fault is a credential cause, never an attach-alias cause, so it carries the secret `Name` rather than mis-labelling itself a mount).
- Entry: `public static IO<ColumnarSession> Open(ColumnarProfile profile)` opens the posture-configured anchor and runs the profile's ordered `INSTALL`/`LOAD` bootstrap before any query, returning the held session whose loaded set is a receipt; `public static IO<Seq<T>> Query<T>(ColumnarSession session, FormattableString sql, Func<DuckDBDataReader, T> shape)` runs a parameterized streaming analytical query on a `Duplicate()` lane; `public static IO<long> Append<T, TMap>(ColumnarSession session, string table, Seq<T> rows) where TMap : DuckDBAppenderMap<T>, new()` bulk-appends through a typed `DuckDBMappedAppender<T, TMap>`; `public static IO<Fin<Unit>> Mount(ColumnarSession session, string alias, string store, ColumnarExtension typed)` read-only `ATTACH`es a foreign store under its typed engine; `public static IO<Fin<Unit>> Secret(ColumnarSession session, SecretScope scope, string name, Seq<(string Key, string Value)> config, bool persist)` emits the `CREATE OR REPLACE SECRET` the `httpfs`/lakehouse credential the posture requires (never an inline path key); `public static IO<QueryResult> ArrowStream(AdbcConnection adbc, string composed, Option<RecordBatch> bound = default)` bridges a DuckDB result to an `Apache.Arrow` `IArrowArrayStream` through the ADBC driver manager, binding any value-bearing parameters through the typed `AdbcStatement.Bind` seam.
- Auto: the session is ONE refcounted native handle per source path held for its lifetime and the last close evicts the buffer pool and attached catalogs (data-interchange `SESSION_POSTURE`), so a streaming drain rides a `Duplicate()` lane over the same handle rather than a fresh open; posture is the four `ColumnarProfile` knobs (`memory_limit` 80 percent of RAM, `threads` the host parallelism budget, `temp_directory` off the home directory, `max_temp_directory_size` the spill ceiling so an operator past `memory_limit` fails loud at the cap rather than silently filling the disk) plus the separate order-free-export `preserve_insertion_order` lever (false for the `Lakehouse` profile), all composed into the connection-string tail through the `DuckDBConnectionStringBuilder` indexer; the extension bootstrap is profile policy expressed as SQL — `Open` runs the ordered `INSTALL <ext>; LOAD <ext>;` pairs through `DuckDBCommand.ExecuteNonQuery` immediately after the connection opens, the statically-linked extensions (`parquet`/`json`/`icu`) needing no `INSTALL` and the on-demand extensions (`spatial`/`httpfs`/`vss`) downloading once to a deterministic `extension_directory` so a sealed run pre-warms the cache; the loaded set is queried back through `duckdb_extensions()` as a profile receipt; `Query` sets `UseStreamingMode` so a large result holds one vector chunk at a time rather than materializing the whole result in the native handle, and `GetQueryProgress().Percentage` negative projects to absence (`Option<double>`), never a fabricated zero; the columnar appender streams rows through a `DuckDBMappedAppender<T, TMap>` typed `AppendValue` so a `[ValueObject]`/`[SmartEnum]` owner crosses to a column through the `Element/codec#CODEC_AXIS` projection, never a hand-rolled column map — the mapped appender validating its column map at construction (a count or type mismatch throws before any row moves) and flushing on `Close()`; the analytical extract path bridges DuckDB → `Apache.Arrow` `RecordBatch` through the `Apache.Arrow.Adbc` driver manager (`AdbcStatement.SqlQuery`/`ExecuteQueryAsync` → `QueryResult.Stream`), NOT a managed DuckDB Arrow member, because the v1.5.3 surface exposes none.
- Receipt: a session open rides `store.columnar.open` carrying the loaded extension set and the posture; a query rides `store.columnar.query` carrying the `DuckDBQueryProgress` percentage; an append rides `store.columnar.append` carrying the row count; a mount rides `store.columnar.mount` carrying the alias.
- Packages: DuckDB.NET.Data.Full (`DuckDBConnection`/`DuckDBCommand`/`DuckDBConnectionStringBuilder`/`DuckDBMappedAppender<T,TMap>`/`DuckDBAppenderMap<T>`/`DuckDBDataReader`/`DuckDBQueryProgress`/`DuckDBErrorType`), Apache.Arrow, Apache.Arrow.Adbc (`AdbcDatabase`/`AdbcConnection`/`AdbcStatement`/`QueryResult`/`IArrowArrayStream`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new analytical profile is one `ColumnarProfile` row carrying its posture and roster; a new extension is one `ColumnarExtension` row carrying its `INSTALL`/`LOAD` SQL; a new credential kind is one `SecretScope` row carrying its `PROVIDER` token; a new fault cause is one `ColumnarFault` case; zero new surface — a per-extension NuGet package, a second analytical engine, an open-per-query connection, command interleaving on one handle, inline credentials in a path, or a provider-branded service family is the deleted form because DuckDB is fully-featured through the one centrally pinned runtime, the engine is a posture-configured anchor with `Duplicate()` concurrency, and the extension roster is profile policy expressed as SQL.
- Boundary: DuckDB is fully-featured through the SQL `INSTALL`/`LOAD` pattern over the one pinned `DuckDB.NET.Data.Full` runtime — no second engine, no per-extension package, the extension roster being profile policy expressed as SQL (`api-duckdb#EXTENSION_PATTERN`); the engine is ONE refcounted anchor connection per source held for its lifetime (data-interchange `SESSION_POSTURE`) and in-process concurrency is `Duplicate()` over the same handle, never command interleaving — a streaming drain occupies its connection until disposed; spill is silent by design (operators past `memory_limit` complete slowly, not fail) so the posture sizes for a dedicated machine and a loud-failure lane caps spill low; the columnar lane is the `Query/lane#READ_ROUTING` ASYNC analytical lane carrying a `StalenessWatermark`, so an interactive-correctness query (clash, void-resolution, live QTO) never reads here without `WaitForNonStaleProjectionDataAsync` and strong-consistency reads go through the synchronous inline projection / topology view (the DuckDB `ATTACH … TYPE postgres` lane joins the live database analytically but is never the consistency owner, `C2`); spatial→PG GiST and ANN→pgvector are the transactional index owners so DuckDB `spatial`/`vss` are the columnar aggregators only (`L2`), meeting NTS/GDAL at WKB through `ST_AsWKB`/`ST_GeomFromWKB`; credentials are `CREATE OR REPLACE SECRET` objects the typed `ColumnarLane.Secret` over the `SecretScope` axis emits (the config pairs `DuckDBParameter`-bound), never an inline key in a path or a `SET` — and `httpfs` is the transport prerequisite for every `s3://`/`http(s)://` path the secret only authorizes; foreign stores enter read-only through `ATTACH … (READ_ONLY)` (read-from-many, write-to-one by engine law) and a foreign-store pre-flight is a `GetSchema` metadata query against the alias before any data moves; `DuckDBException` lifts at the provider edge discriminated on `DuckDBErrorType` into `ColumnarFault`, never a raw ADO exception, the fault arms mapping the typed error class and never message substrings.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Data;
using System.Globalization;
using System.Linq;
using Apache.Arrow;
using Apache.Arrow.Adbc;
using DuckDB.NET.Data;
using DuckDB.NET.Data.Mapping;
using DuckDB.NET.Native;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — the alias wins over LanguageExt.Common.Expected for the bare name
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// The extension vocabulary — POLICY_VALUES whose `Bootstrap` SQL IS the entire extension API (api-duckdb#EXTENSION_PATTERN).
// `Linked` extensions are statically linked into the bundled native `duckdb` (no `INSTALL`, resolve offline); the rest
// download once on first `INSTALL` to a deterministic `extension_directory`. A new extension is ONE row; a per-extension
// NuGet/native package is the deleted form because the one pinned runtime carries the whole capability surface.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarExtension {
    public static readonly ColumnarExtension Spatial  = new("spatial", linked: false);
    public static readonly ColumnarExtension Vss      = new("vss", linked: false);
    public static readonly ColumnarExtension Fts      = new("fts", linked: false);
    public static readonly ColumnarExtension Parquet  = new("parquet", linked: true);
    public static readonly ColumnarExtension Json     = new("json", linked: true);
    public static readonly ColumnarExtension Icu      = new("icu", linked: true);
    public static readonly ColumnarExtension Httpfs   = new("httpfs", linked: false);
    public static readonly ColumnarExtension Iceberg  = new("iceberg", linked: false);
    public static readonly ColumnarExtension Delta    = new("delta", linked: false);
    public static readonly ColumnarExtension Postgres = new("postgres", linked: false);
    public static readonly ColumnarExtension Sqlite   = new("sqlite", linked: false);
    public static readonly ColumnarExtension Excel    = new("excel", linked: false);
    public static readonly ColumnarExtension Avro     = new("avro", linked: false);
    public static readonly ColumnarExtension Aws      = new("aws", linked: false);
    public static readonly ColumnarExtension Azure    = new("azure", linked: false);

    public bool Linked { get; }
    private ColumnarExtension(string key, bool linked) : this(key) => Linked = linked;

    // Statically-linked extensions `LOAD` only; on-demand extensions `INSTALL` then `LOAD`. The SQL text IS the API.
    public string Bootstrap => Linked ? $"LOAD {Key};" : $"INSTALL {Key}; LOAD {Key};";
}

// The analytical-lane axis — one row carries BOTH the engine posture (the four data-interchange#SESSION_POSTURE knobs
// sized for a dedicated machine) AND the ordered extension roster, so a lane is one declaration the `Open` composes into
// the connection-string tail and the bootstrap SQL. `preserve_insertion_order` false is the memory lever for order-free
// export pipelines (Lakehouse); a strong-consistency BIM lane keeps insertion order. A new profile is ONE row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarProfile {
    public static readonly ColumnarProfile Geometry  = new("geometry", "80%", "geometry.tmp", "90%", preserveOrder: true,  [ColumnarExtension.Spatial, ColumnarExtension.Parquet]);
    public static readonly ColumnarProfile Search    = new("search", "80%", "search.tmp", "90%", preserveOrder: true,  [ColumnarExtension.Vss, ColumnarExtension.Fts]);
    public static readonly ColumnarProfile Lakehouse = new("lakehouse", "80%", "lakehouse.tmp", "90%", preserveOrder: false, [ColumnarExtension.Httpfs, ColumnarExtension.Iceberg, ColumnarExtension.Delta, ColumnarExtension.Postgres]);
    public static readonly ColumnarProfile Bim       = new("bim", "80%", "bim.tmp", "90%", preserveOrder: true,  [ColumnarExtension.Parquet, ColumnarExtension.Json, ColumnarExtension.Spatial]);

    public string MemoryCap { get; }
    public string SpillRoot { get; }
    public string SpillCap { get; }
    public bool PreserveOrder { get; }
    public Seq<ColumnarExtension> Roster { get; }
    private ColumnarProfile(string key, string memoryCap, string spillRoot, string spillCap, bool preserveOrder, Seq<ColumnarExtension> roster) : this(key) =>
        (MemoryCap, SpillRoot, SpillCap, PreserveOrder, Roster) = (memoryCap, spillRoot, spillCap, preserveOrder, roster);

    // The posture composed into the connection-string tail through the builder indexer (data-interchange#SESSION_POSTURE):
    // the FOUR sized-for-a-dedicated-machine knobs — `threads` consumed from the host parallelism budget, `memory_limit`,
    // `temp_directory`, and `max_temp_directory_size` (the spill ceiling; absent it, silent spill fills the disk to the
    // SESSION_POSTURE loud-failure-cap discipline). `preserve_insertion_order` is the SEPARATE order-free-export memory
    // lever (false for `Lakehouse`), not a posture knob — a bare per-query open with no posture is the deleted form.
    public string ConnectionString(string dataSource, int threads) {
        var rows = new DuckDBConnectionStringBuilder { DataSource = dataSource };
        (rows["threads"], rows["memory_limit"], rows["temp_directory"], rows["max_temp_directory_size"], rows["preserve_insertion_order"]) =
            (threads, MemoryCap, SpillRoot, SpillCap, PreserveOrder);
        return rows.ConnectionString;
    }
}

// The credential-kind axis the `CREATE SECRET` rail dials (api-duckdb#SECRET_AND_ATTACH): one row per `TYPE`, each carrying
// the `PROVIDER` token (`s3` chains the credential chain, `azure`/`postgres` carry config) and the `PersistInto` attached-store
// alias a persisted secret writes into. A new credential kind is one row; an inline path key is the deleted form `Secret` exists
// to forbid. `httpfs` resolves the TRANSPORT, this resolves the CREDENTIAL — the two compose, neither stands alone.
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

// --- [ERRORS] -----------------------------------------------------------------------------
// The closed Persistence-query columnar band (835x): a [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND` `BimFault`
// (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)` ctor (no
// `Category` to override) is the deleted form. A case lifts BARE onto the Fin rail through the Expected derivation (no
// `.ToError()` hop), no `[GenerateUnionOps]` (the kernel union-ops generator is strictly opt-in — the band wants no
// per-case `SelfOp`), the `[Union]`-generated `Switch`/`Map` untouched; band membership is a per-case `Code => 835x` override, sitting beside the cypher `GraphFault` (836x) and
// `TopologyFault` (837x). Every native arm carries the `DuckDBErrorType` the boundary discriminated on so the fault names
// the typed native error class, never a message substring, and `Category` projects the telemetry label.
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

    public override int Code => Switch(
        queryFailed:       static _ => 8350,
        extensionGap:      static _ => 8351,
        appendRefused:     static _ => 8352,
        mountRefused:      static _ => 8353,
        egressRefused:     static _ => 8354,
        unstampedArtifact: static _ => 8355,
        secretRefused:     static _ => 8356);

    public override string Message => Switch(
        queryFailed:       static c => $"<columnar-query:{c.Detail}>",
        extensionGap:      static c => $"<columnar-extension:{c.Extension}>",
        appendRefused:     static c => $"<columnar-append:{c.Table}>",
        mountRefused:      static c => $"<columnar-mount:{c.Alias}>",
        egressRefused:     static c => $"<columnar-egress:{c.Destination}>",
        unstampedArtifact: static c => $"<columnar-unstamped:{c.Path}>",
        secretRefused:     static c => $"<columnar-secret:{c.Name}>");

    public override string Category => Switch(
        queryFailed:       static _ => "Query",
        extensionGap:      static _ => "Extension",
        appendRefused:     static _ => "Append",
        mountRefused:      static _ => "Mount",
        egressRefused:     static _ => "Egress",
        unstampedArtifact: static _ => "Unstamped",
        secretRefused:     static _ => "Secret");

    public static ColumnarFault Create(string message) => new QueryFailed(message, default);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The refcounted posture-configured anchor (data-interchange#SESSION_POSTURE): ONE native handle per source path held for
// its lifetime, in-process concurrency through `Duplicate()` lanes (a streaming drain occupies its connection until
// disposed), foreign stores mounted read-only, the live-query progress projected through `Option<double>` so a negative
// percentage is absence not a fabricated zero. Opened through `ColumnarLane.Open` which runs the extension bootstrap; the
// ctor is private so a session is never constructed without its profile posture and loaded roster.
public sealed class ColumnarSession : IDisposable {
    readonly DuckDBConnection anchor;
    public ColumnarProfile Profile { get; }
    public Seq<string> Loaded { get; }

    internal ColumnarSession(DuckDBConnection anchor, ColumnarProfile profile, Seq<string> loaded) =>
        (this.anchor, Profile, Loaded) = (anchor, profile, loaded);

    public DuckDBConnection Lane() => anchor.Duplicate();
    public DuckDBCommand Command() => anchor.CreateCommand();
    public Option<double> Progress() => anchor.GetQueryProgress() is { Percentage: >= 0 and var alive } ? Some(alive) : None;
    public void Dispose() => anchor.Dispose();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ColumnarLane {
    // Open the posture-configured anchor and run the ordered INSTALL/LOAD bootstrap before any query, then read the loaded
    // set back through `duckdb_extensions()` as the profile receipt. The bootstrap is profile policy expressed as SQL — a
    // per-extension package or an implicit autoload-as-contract is the deleted form. Exemption: the open + bootstrap loop is
    // the platform-forced ADO statement seam.
    public static IO<ColumnarSession> Open(ColumnarProfile profile, string dataSource, int threads) =>
        IO.liftAsync(async () => {
            var anchor = new DuckDBConnection(profile.ConnectionString(dataSource, threads));
            await anchor.OpenAsync().ConfigureAwait(false);
            await using (var bootstrap = anchor.CreateCommand()) {
                foreach (var extension in profile.Roster) { bootstrap.CommandText = extension.Bootstrap; await bootstrap.ExecuteNonQueryAsync().ConfigureAwait(false); }
            }
            await using var probe = anchor.CreateCommand();
            probe.CommandText = "SELECT extension_name FROM duckdb_extensions() WHERE loaded";
            await using var reader = (DuckDBDataReader)await probe.ExecuteReaderAsync().ConfigureAwait(false);
            var loaded = Seq<string>();
            while (await reader.ReadAsync().ConfigureAwait(false)) loaded = loaded.Add(reader.GetString(0));
            return new ColumnarSession(anchor, profile, loaded);
        });

    // A parameterized streaming analytical query on a Duplicate() lane: `UseStreamingMode` holds one vector chunk at a time
    // (data-interchange#CHUNK_ALGEBRA) rather than materializing the whole result in the native handle. The interpolation
    // holes bind through DuckDB NAMED parameters: each `{i}` hole rewrites to the `$pi` placeholder DuckDB's parser
    // recognizes (a raw `FormattableString.Format` would leave the literal `{0}` template DuckDB cannot bind — the named
    // parameterization defect), and `GetArgument(i)` rides a `DuckDBParameter("pi", value)`. Exemption: the chunk drain
    // fills a seam-local list frozen once by `toSeq`; per-row `Seq.Add` forcing is the rejected O(n²) form.
    public static IO<Seq<T>> Query<T>(ColumnarSession session, FormattableString sql, Func<DuckDBDataReader, T> shape) =>
        IO.liftAsync(async () => {
            var lane = session.Lane();
            await using (lane.ConfigureAwait(false)) {
                await using var command = lane.CreateCommand();
                var placeholders = Enumerable.Range(0, sql.ArgumentCount).Select(static i => (object)$"$p{i}").ToArray();
                (command.CommandText, command.UseStreamingMode) = (string.Format(CultureInfo.InvariantCulture, sql.Format, placeholders), true);
                for (var i = 0; i < sql.ArgumentCount; i++) command.Parameters.Add(new DuckDBParameter($"p{i}", sql.GetArgument(i)));
                await using var reader = (DuckDBDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
                var rows = new List<T>();
                while (await reader.ReadAsync().ConfigureAwait(false)) rows.Add(shape(reader));
                return toSeq(rows);
            }
        }) | @catch<IO, Seq<T>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<Seq<T>>(new ColumnarFault.QueryFailed(e.Message, ((DuckDBException)e.ToException()).ErrorType)));

    // Typed bulk ingest through the mapped appender — the `DuckDBAppenderMap<T>` projects the owner's columns in declaration
    // order (validated at construction: a count or type mismatch throws before any row moves), `AppendRecords` streams the
    // batch, `Close()` flushes. A row-at-a-time INSERT loop is the deleted form (data-interchange#CHUNK_ALGEBRA).
    public static IO<long> Append<T, TMap>(ColumnarSession session, string table, Seq<T> rows) where TMap : DuckDBAppenderMap<T>, new() =>
        IO.lift(() => {
            using var lane = session.Lane();
            var appender = lane.CreateAppender<T, TMap>(table);
            appender.AppendRecords(rows);
            appender.Close();
            return (long)rows.Count;
        }) | @catch<IO, long>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<long>(new ColumnarFault.AppendRefused(table, ((DuckDBException)e.ToException()).ErrorType)));

    // Read-only foreign-store mount (data-interchange `read-from-many, write-to-one`): a live PG/SQLite/lakehouse store
    // enters through `ATTACH … (READ_ONLY)` so a federated query joins it without an ETL hop; the analytical join is never
    // the consistency owner (C2). `DETACH` evicts it. A foreign-store pre-flight is a metadata query against the alias. The
    // `httpfs`/`s3` path resolves its credential through `Secret` FIRST (an inline key in the path is the deleted form).
    public static IO<Fin<Unit>> Mount(ColumnarSession session, string alias, string store, ColumnarExtension typed) =>
        IO.liftAsync(async () => {
            await using var command = session.Command();
            command.CommandText = $"ATTACH IF NOT EXISTS '{store}' AS {alias} (TYPE {typed.Key}, READ_ONLY)";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.MountRefused(alias, ((DuckDBException)e.ToException()).ErrorType))));

    // The credential rail the `Lakehouse`/`httpfs` posture REQUIRES (api-duckdb#SECRET_AND_ATTACH): an `s3`/`azure`/`postgres`
    // credential enters as a `CREATE OR REPLACE SECRET` object whose config rows interpolate from the typed `SecretScope`
    // vocabulary — NEVER an inline `KEY_ID`/`SECRET` in a `read_parquet('s3://…')` path or a `SET`, the named deleted form
    // the boundary forbids. `Persist` writes the secret INTO the attached store (survives reconnect) versus an in-memory
    // profile-scoped secret. The config pairs bind through `DuckDBParameter` so a key never lands as a concatenated literal.
    public static IO<Fin<Unit>> Secret(ColumnarSession session, SecretScope scope, string name, Seq<(string Key, string Value)> config, bool persist) =>
        IO.liftAsync(async () => {
            await using var command = session.Command();
            var into = persist ? $" IN {scope.PersistInto}" : string.Empty;
            var rows = toSeq(config.AsEnumerable().Select((pair, i) => { command.Parameters.Add(new DuckDBParameter($"v{i}", pair.Value)); return $"{pair.Key} ${{v{i}}}"; }));
            command.CommandText = $"CREATE OR REPLACE SECRET {name}{into} (TYPE {scope.Key}, PROVIDER {scope.Provider}, {string.Join(", ", rows)})";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.SecretRefused(name, ((DuckDBException)e.ToException()).ErrorType))));

    // The DuckDB → Apache.Arrow zero-managed-copy bridge: the v1.5.3 DuckDB managed surface exposes NO Arrow member
    // (api-duckdb#ARROW_BOUNDARY), so the columnar extract dispatches through the `Apache.Arrow.Adbc` driver manager
    // (`AdbcStatement.SqlQuery` → `ExecuteQueryAsync`) and reads back one `IArrowArrayStream` on `QueryResult.Stream` — the
    // one egress boundary the IPC, ADBC, and Flight surfaces all meet at (api-arrow#LOCAL_ADMISSION). The ADBC lane carries
    // NO `DuckDBParameter`, so a value-bearing query binds an `Apache.Arrow.RecordBatch` through the typed `AdbcStatement.Bind`
    // seam (api-arrow `Bind`/`BindStream`) and the `composed` SQL holds the bind placeholders — passing a `FormattableString`
    // whose `.Format` is the raw `{0}` template (dropping every argument) is the named parameterization defect this deletes.
    public static IO<QueryResult> ArrowStream(AdbcConnection adbc, string composed, Option<Apache.Arrow.RecordBatch> bound = default) =>
        IO.liftAsync(() => {
            using var statement = adbc.CreateStatement();
            statement.SqlQuery = composed;
            bound.Iter(batch => statement.Bind(batch, batch.Schema));
            return statement.ExecuteQueryAsync().AsTask();
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | engine session      | one refcounted posture anchor + `Duplicate()` | never open-per-query, never command interleaving (`SESSION_POSTURE`) |
|  [02]   | extension bootstrap | ordered `INSTALL`/`LOAD` SQL           | profile policy; no per-extension package, no second engine |
|  [03]   | consistency stance  | async, `StalenessWatermark`            | never read by interactive correctness without the wait (`C2`) |
|  [04]   | index ownership     | DuckDB spatial/vss are aggregators     | GiST/pgvector own the transactional index (`L2`)          |
|  [05]   | credential rail     | `CREATE SECRET` over `SecretScope`     | `DuckDBParameter`-bound config; never an inline path key  |
|  [06]   | Arrow bridge        | ADBC driver manager → `IArrowArrayStream` | no managed DuckDB Arrow member; params via `AdbcStatement.Bind` |
|  [07]   | fault rail          | `DuckDBException` → `ColumnarFault`     | discriminated on `DuckDBErrorType`, never a raw ADO throw  |

## [03]-[ARTIFACT_EGRESS]

- Owner: `Codec` the `[SmartEnum<string>]` compression vocabulary whose `.Key` IS the `COPY` `COMPRESSION` token; `Collision` the destination-collision vocabulary; `EgressFormat` the format vocabulary carrying its grouped flag and the JSON `ARRAY` row; `ArtifactClass` the closed analytical-artifact declaration deriving emission, partition, and the footer stamp from one row; `ArtifactEgress` the static surface owning the `COPY (SELECT) TO` rail, the footer-metadata stamp read, and the `read_parquet` generation scan.
- Cases: `EgressFormat` is `Parquet` (grouped), `Csv`, `Json` (carrying `ARRAY true`); `Codec` is `Zstd`/`Snappy`; `Collision` is `Overwrite`/`OverwriteOrIgnore`/`Append`; `ArtifactClass` is `BimRollup` (the QTO/quantity Parquet generation, Zstd, overwrite) and `CoverageFeed` (the partitioned geospatial-coverage JSON feed, Snappy, append) — a new artifact class is one row deriving its whole emission.
- Entry: `public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, string projection, string destination, string stamp)` runs the one `COPY (projection) TO destination (…)` statement assembled from the artifact-class policy rows; `public static IO<Fin<string>> StampOf(ColumnarSession session, string artifact)` reads the content stamp from the Parquet footer through `parquet_kv_metadata` without decoding data; `public static FormattableString Generation(string root)` derives the `read_parquet` glob scan over an artifact-generation directory with `union_by_name`/`hive_partitioning`/`filename` provenance.
- Auto: one `COPY (SELECT) TO` statement owns engine-mediated egress (data-interchange `ARTIFACT_PROJECTION`) — `FORMAT`/`COMPRESSION`/`ROW_GROUP_SIZE`/`PARTITION_BY` interpolate beside the shared destination from the artifact-class rows so a mistyped token is unrepresentable rather than a runtime SQL parse error, a second export path per format is the deleted form, and a `KV_METADATA` stamp binds the artifact's content identity into the footer; row-group geometry is the unit of scan parallelism and zonemap pruning so the `ROW_GROUP_SIZE` near the default-row count prunes well and a tiny-group append-per-batch exporter batches through a staging projection and exports once; partitioning is a pruning instrument (`PARTITION_BY` into hive directories at cardinality in the tens to low thousands), never a uniqueness scheme; the footer answers declared shape, per-row-group statistics, and the caller stamp without decoding data (`parquet_kv_metadata`/`parquet_metadata`/`parquet_schema`) so artifact admission is a metadata-cost gate run on every delivery; the generation read is `read_parquet` over a path/glob/list so a generation directory growing changes only the path argument, `union_by_name` makes additive columns compatible by construction (absent reads NULL), and `filename`/`file_row_number` pin per-row provenance; the `COPY` is a filesystem effect outside transaction rollback so publication is the atomic-write protocol, never transactional cleanup.
- Receipt: an egress rides `store.columnar.egress` carrying the artifact class and the destination; a footer stamp read rides `store.columnar.stamp` carrying the content identity.
- Packages: DuckDB.NET.Data.Full (`DuckDBCommand.ExecuteNonQuery`/`ExecuteScalar`/`DuckDBParameter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new artifact class is one `ArtifactClass` row deriving emission, partition, and stamp; a new egress format/codec/collision is one vocabulary row whose `.Key` IS the `COPY` token; zero new surface — a per-format export path, a `FORMAT` value stretched to name a transport the engine never performs, a filename-convention identity trust, or an in-place generation rewrite is the deleted form because the COPY rail is the one SQL-mediated egress, identity rides the footer stamp, and generations are immutable.
- Boundary: the `COPY (SELECT) TO` rail is the SQL-mediated egress lane, not the egress monopoly — a zero-copy in-process columnar handoff (the `ColumnarLane.ArrowStream` ADBC bridge) and a direct managed file codec (`#FLAT_TABLE_EGRESS` `ParquetSharp.Arrow`) are distinct lanes a `COPY` `FORMAT` token cannot express, so a non-SQL egress lands as a sibling lane beside the COPY family, never as a `FORMAT` row (the deleted form is a `FORMAT` value stretched to name a transport the engine never performs); artifact identity is the footer content stamp plus the declared `ArtifactClass` shape, never a filename convention — a renamed artifact keeps its identity and a stamp that no longer matches its content is corruption, not drift; generations are immutable (compaction is a new artifact written beside the old with a new stamp, never an in-place merge) and `FIELD_IDS` at export plus an id-keyed scan map make renames non-breaking across generations; the `COPY` is a filesystem effect outside transaction rollback so publication composes the atomic-write protocol the `Element/codec#SNAPSHOT_SPINE` owns, never transactional cleanup; the lakehouse `delta`/`iceberg` scans read the same tables the managed `api-deltalake` writer produces — DuckDB the read/aggregate projection, the managed writer the system of record, meeting at the table path and never re-authoring each other's metadata.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The COPY-token vocabularies (data-interchange#ARTIFACT_PROJECTION): each `.Key` IS the literal COPY token so a mistyped
// `OVERWRITE_OR_INGORE` is unrepresentable rather than a runtime SQL parse error. A new format/codec/collision is ONE row.
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
// The closed analytical-artifact declaration: ONE row derives the whole `COPY` emission (format, codec, row-group geometry,
// partition key, collision) plus the footer stamp, so a column added to the projection updates emission and admission in one
// diff. `Egress` assembles the single COPY statement; `PARTITION_BY` is a pruning instrument (hive directories), never a
// uniqueness scheme; `KV_METADATA` stamps the content identity into the footer so identity rides the file, not its name.
public sealed record ArtifactClass(string Name, EgressFormat Format, Codec Codec, int RowGroup, Option<string> PartitionKey, Collision Collision) {
    public static readonly ArtifactClass BimRollup = new("bim-rollup", EgressFormat.Parquet, Codec.Zstd, 122_880, None, Collision.Overwrite);
    public static readonly ArtifactClass CoverageFeed = new("coverage-feed", EgressFormat.Json, Codec.Snappy, 122_880, Some("crs"), Collision.Append);

    public string Egress(string projection, string destination, string stamp) =>
        $"COPY ({projection}) TO '{destination}' ({string.Join(", ",
            Seq(Some($"FORMAT {Format.Key}"), Some($"COMPRESSION {Codec.Key}"),
                Format.Grouped ? Some($"ROW_GROUP_SIZE {RowGroup}") : Option<string>.None, Format.ArrayRow,
                PartitionKey.Map(static key => $"PARTITION_BY ({key})"), Some(Collision.Key),
                Some($"KV_METADATA {{ stamp: '{stamp}' }}")).Somes())})";
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ArtifactEgress {
    // The one engine-mediated egress: a single COPY statement assembled from the artifact-class policy rows. A second export
    // path per format is the deleted form. Exemption: the command body is the platform-forced ADO statement seam.
    public static IO<Fin<Unit>> Publish(ColumnarSession session, ArtifactClass artifact, string projection, string destination, string stamp) =>
        IO.liftAsync(async () => {
            await using var command = session.Command();
            command.CommandText = artifact.Egress(projection, destination, stamp);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return Fin<Unit>.Succ(unit);
        }) | @catch<IO, Fin<Unit>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.pure(Fin<Unit>.Fail(new ColumnarFault.EgressRefused(destination, ((DuckDBException)e.ToException()).ErrorType))));

    // Artifact admission is a footer-metadata gate (data-interchange#EGRESS_ROWS): the content stamp reads from the Parquet
    // footer without decoding data, so a renamed artifact keeps its identity and a missing stamp rails UnstampedArtifact.
    public static IO<Fin<string>> StampOf(ColumnarSession session, string artifact) =>
        IO.liftAsync(async () => {
            await using var command = session.Command();
            command.CommandText = "SELECT decode(value) FROM parquet_kv_metadata(@path) WHERE decode(key) = 'stamp'";
            command.Parameters.Add(new DuckDBParameter("path", artifact));
            return Optional(await command.ExecuteScalarAsync().ConfigureAwait(false)).Map(static held => (string)held);
        }).Map(stamp => stamp.Match(Some: Fin<string>.Succ, None: () => Fin<string>.Fail(new ColumnarFault.UnstampedArtifact(artifact))));

    // The generation scan (data-interchange#GENERATION_READS): `read_parquet` over a glob with `union_by_name` (additive
    // columns read NULL), `hive_partitioning` (directory keys lift to columns), and `filename`/`file_row_number` per-row
    // provenance — so a generation directory growing changes only the path argument and an in-place rewrite is the deleted form.
    public static FormattableString Generation(string root) =>
        $"SELECT *, filename, file_row_number FROM read_parquet('{root}/**/*.parquet', union_by_name = true, hive_partitioning = true, filename = true, file_row_number = true)";
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | engine egress       | one `COPY (SELECT) TO` rail            | `.Key` IS the COPY token; a second export path is deleted  |
|  [02]   | artifact identity   | footer `KV_METADATA` stamp             | rides the file, never a filename convention                |
|  [03]   | partitioning        | `PARTITION_BY` hive directories        | a pruning instrument, never a uniqueness scheme            |
|  [04]   | generation read     | `read_parquet` glob + `union_by_name`  | generations immutable; additive columns compatible by construction |
|  [05]   | non-SQL egress      | sibling ADBC / `ParquetSharp.Arrow` lane | never a `FORMAT` token stretched to name a transport      |

## [04]-[FLAT_TABLE_EGRESS]

- Owner: `BimOpenSchemaProjection` the co-transactional Marten `FlatTableProjection` writing the columnar BIM fact table in the append transaction; `FlatTableEgress` the static surface owning the async daemon materialization of the eleven-table `Ara3D.BimOpenSchema` `.duckdb`/Parquet generation and the native `ParquetSharp.Arrow` codec lane the columnar query reads.
- Entry: `public sealed class BimOpenSchemaProjection : FlatTableProjection` registered `ProjectionLifecycle.Inline` so the columnar BIM facts write in the append transaction (`Project<GraphEvent.GraphCreated>(map => …)`); `public static IO<Unit> Materialize(IDocumentStore store, Duration timeout)` runs the async daemon view producing the BimOpenSchema generation the analytical lane reads; `public static IO<long> WriteFrames(BimData frames, string path)` writes the eleven-table `.duckdb` through `BimData.WriteDuckDB`; `public static IO<Seq<RecordBatch>> ReadParquetFrames(string parquetPath)` reads a standard-format `.parquet` generation into `Apache.Arrow` `RecordBatch`es through the native `ParquetSharp.Arrow.FileReader`.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` (`Project<T>(StatementMap)`) written in the same transaction as the events, NOT daemon-lagged (`M4`), because a flat analytical view a live QTO reads must be read-your-writes consistent — the structural map maps the `GraphEvent.GraphCreated`'s `Header.Schema.Key`/`Header.View.Key` (the `ReleaseVersion`/`ModelView` smart-enum keys, since `StatementMap.Map` writes a primitive column, never a smart-enum object) and the `GraphRevised`'s `GraphDelta.NodeCount`/`EdgeCount` change magnitude through the single-column primary key `FlatTableProjection` requires; `Ara3D.BimOpenSchema.IO`'s `BimData.WriteDuckDB` bulk-appends the eleven suffixed BIM tables (`Points_0`/`Strings_1`/`Descriptors_2`/`Documents_3`/`Entities_4`/`Relations_5`/`DoubleParameters_6`/`IntegerParameters_7`/`StringParameters_8`/`EntityParameters_9`/`PointParameters_10`) in the fixed `ToDataSet()` projection ordinal, and a Persistence analytical query opens that `.duckdb` over the same pinned runtime and SQL-joins the suffixed entity/parameter/relation tables by their exact suffixed names; the async daemon `Materialize` blocks on `WaitForNonStaleData` so the generation is current before the heavy aggregation lanes read it carrying the `StalenessWatermark`; the native `ParquetSharp.Arrow.FileReader.GetRecordBatchReader` reads the same standard-format `.parquet` files the managed `Parquet.Net` writer produced into `IArrowArrayStream` `RecordBatch`es for the columnar query rail (managed writer / native libparquet-cpp reader interoperate at the file format, never the assembly).
- Receipt: a flat-table projection rides `store.columnar.flattable` carrying the change magnitude; a daemon materialization rides `store.columnar.materialize` carrying the watermark; a frame write rides `store.columnar.frames` carrying the table count; a Parquet read rides `store.columnar.parquet` carrying the record-batch count.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`SchemaNameSource`/`ProjectionLifecycle`/`IDocumentStore`/`BuildProjectionDaemonAsync`/`WaitForNonStaleData`), Ara3D.BimOpenSchema (`BimData`/`BimDataBuilder`/`ToDataSet`), Ara3D.BimOpenSchema.IO (`BimDataSerializer.WriteDuckDB`/`DuckDbUtils.WriteToDuckDB`), ParquetSharp (`Arrow.FileReader`/`Arrow.FileWriter`), Apache.Arrow (`RecordBatch`/`IArrowArrayStream`), DuckDB.NET.Data.Full, Rasm.Element (`GraphDelta`/`Header`), Rasm.Persistence (`Element/graph#STREAM_GRAIN` `GraphEvent`/`GraphCreated`/`GraphRevised` the Marten event body), LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `map.Map` statement on the `StatementMap`; a new analytical generation is one async daemon view; a new frame codec is the existing `ParquetSharp.Arrow` lane reading a new format; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, a second Parquet runtime beside `ParquetSharp`, or a hollow writer that opens a row group and writes no column is the deleted form because the BimOpenSchema egress is co-transactional, the managed `Parquet.Net` writer and the native `ParquetSharp` reader meet at the file format, and the Arrow record-batch model is `api-arrow`'s.
- Boundary: the `ElementGraph → Ara3D.BimOpenSchema` egress is a co-transactional `FlatTableProjection` (`M4`) so a live-QTO analytical read is read-your-writes consistent rather than daemon-lagged — `FlatTableProjection` requires a single-column primary key and writes a primitive per `StatementMap.Map`, so a `ReleaseVersion`/`ModelView` smart-enum maps as its `.Key` and a `GraphDelta` maps as its `NodeCount`/`EdgeCount`, never as the smart-enum or delta object itself; if BimOpenSchema is EAV-generic Persistence owns the structural map, if BIM-typed it is a Bim-implemented seam projection (the wire seam, never a sibling reference); the eleven suffixed BIM tables are read with the built-in `parquet`/`json` surface and `spatial`/`vss`/`fts` extend them for geometry/ANN/text analytics over the same `.duckdb`, all on the one pinned runtime, and a direct SQL consumer references the `<Name>_<n>` projection-ordinal suffix that IS the real table identity (`api-ara3d-bimopenschema#DATASET_BRIDGE`), never a bare table name; the Parquet file codec is `ParquetSharp.Arrow` (the native libparquet-cpp read/write the managed Arrow stack lacks, exposing the `Apache.Arrow` `RecordBatch` directly so Parquet↔Arrow is a first-class managed call), distinct from the DuckDB SQL `read_parquet`/`COPY` path, the three meeting at the Parquet file format and the `Apache.Arrow` model owned by `api-arrow` not re-declared here; the `Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built (JIT optimizations disabled in the shipped IL), so a hot ingest/codec loop pays the un-optimized penalty and a production-hot path measures rather than assuming.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Apache.Arrow;
using Ara3D.BimOpenSchema;
using Ara3D.BimOpenSchema.IO;
using LanguageExt;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Events.Projections.Flattened;
using NodaTime;
using ParquetSharp.Arrow;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] -----------------------------------------------------------------------------
// The co-transactional BIM columnar egress (M4). `FlatTableProjection` requires a SINGLE-column primary key and writes a
// PRIMITIVE per `StatementMap.Map`, so the model header maps as its smart-enum `.Key` strings and a steady-state delta as its
// `NodeCount`/`EdgeCount` magnitude — a smart-enum or `GraphDelta` object is NOT a legal column value. Registered
// `ProjectionLifecycle.Inline` (via `Snapshot`/`Add(..., Inline)`) so the facts write in the append transaction and a live
// QTO read is read-your-writes consistent, never daemon-lagged. `SchemaNameSource.DocumentSchema` keeps it in the document schema.
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
    // The async daemon materialization of the eleven-table generation: start the daemon, block on WaitForNonStaleData so the
    // generation is CURRENT before the heavy aggregation lanes read it (the StalenessWatermark consumers read the lag), then
    // the analytical lane scans it. The async daemon is the heavy-aggregation lane; the inline FlatTableProjection above is the
    // read-your-writes lane — two altitudes never conflated (C2). Exemption: the daemon lifecycle is the platform-forced seam.
    public static IO<Unit> Materialize(IDocumentStore store, Duration timeout) =>
        IO.liftAsync(async () => {
            await using var daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            await daemon.WaitForNonStaleData(timeout.ToTimeSpan()).ConfigureAwait(false);
            return unit;
        });

    // The eleven-table `.duckdb` write through Ara3D.BimOpenSchema.IO: `BimData.WriteDuckDB` projects the SoA columns to the
    // generic IDataSet and bulk-appends each table as `<Name>_<n>` (the fixed ToDataSet ordinal). A Persistence analytical
    // query opens this `.duckdb` over the ONE pinned DuckDB runtime and SQL-joins the suffixed tables. No second engine.
    public static IO<long> WriteFrames(BimData frames, string path) =>
        IO.lift(() => { frames.WriteDuckDB(path); return (long)frames.Entities.Count; });

    // The native ParquetSharp.Arrow read of a standard-format `.parquet` generation into Apache.Arrow RecordBatches — the
    // managed Parquet.Net writer (BimOpenSchema.IO `WriteToParquetZip`) and this native libparquet-cpp reader interoperate at
    // the FILE FORMAT, never the assembly. `GetRecordBatchReader` yields one `IArrowArrayStream` (the one egress boundary
    // api-arrow owns); a hand-rolled per-cell read loop or a managed re-implementation of the codec is the deleted form.
    public static IO<Seq<RecordBatch>> ReadParquetFrames(string parquetPath) =>
        IO.liftAsync(async () => {
            using var reader = new FileReader(File.OpenRead(parquetPath));
            using var stream = reader.GetRecordBatchReader();
            var batches = new List<RecordBatch>();
            while (await stream.ReadNextRecordBatchAsync().ConfigureAwait(false) is { } batch) batches.Add(batch);
            return toSeq(batches);
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | BimOpenSchema egress| co-transactional `FlatTableProjection` | read-your-writes, never daemon-lagged (`M4`)              |
|  [02]   | column value        | smart-enum `.Key` / `GraphDelta` count | `StatementMap.Map` writes a primitive, never the object   |
|  [03]   | BIM tables          | eleven suffixed columnar tables        | read over the one pinned runtime by `<Name>_<n>` ordinal  |
|  [04]   | Parquet codec       | native `ParquetSharp.Arrow` ↔ `RecordBatch` | distinct from the DuckDB SQL parquet path; meet at the file |
|  [05]   | lakehouse           | `delta`/`iceberg` scan                 | DuckDB reads, `DeltaLake.Net` writes; meet at the table   |
