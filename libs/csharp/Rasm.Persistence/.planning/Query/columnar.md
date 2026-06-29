# [PERSISTENCE_QUERY_COLUMNAR]

Rasm.Persistence runs analytical aggregation, columnar rollup, and lakehouse scans through one in-process DuckDB engine made fully-featured by the SQL `INSTALL`/`LOAD` extension bootstrap (`api-duckdb`), fed by two egress shapes off the Marten substrate: a CO-TRANSACTIONAL `Ara3D.BimOpenSchema` `FlatTableProjection` that writes the eleven columnar BIM tables in the append transaction (never daemon-lagged, per `M4`), and ASYNC daemon projections (`ProjectionLifecycle.Async`) materializing Parquet/columnar views carrying an explicit `StalenessWatermark`. `ColumnarProfile` declares the extension roster a lane requires (`spatial`/`vss`/`fts`/`parquet`/`httpfs`/`iceberg`/`delta`/`postgres`) once at bootstrap as ordered `INSTALL`/`LOAD` SQL through `DuckDBCommand`, the loaded set landing as a profile receipt — no per-extension package, no second engine. The analytical lane is the `Query/lane#READ_ROUTING` async lane, so an interactive-correctness query never reads here without `WaitForNonStaleProjectionDataAsync`; strong-consistency reads go through the synchronous topology. `Ara3D.BimOpenSchema.IO` writes the `.duckdb` the analytical query reads, `ParquetSharp` is the direct columnar-file codec, and DuckDB `ATTACH … TYPE postgres` joins the live Marten/Npgsql database without an ETL hop. `ElementGraph`/`Node` arrive from `Rasm.Element`; the inline projection and `FlatTableProjection` host arrive from `Element/graph`/`api-marten`; `StalenessWatermark` arrives from `Query/lane`; `ClockPolicy`, `ReceiptSinkPort` arrive from AppHost.

## [01]-[INDEX]

- [01]-[COLUMNAR_LANE]: the DuckDB provider, the `INSTALL`/`LOAD` extension profile bootstrap, the column projection, and the fault rail.
- [02]-[FLAT_TABLE_EGRESS]: the co-transactional `Ara3D.BimOpenSchema` `FlatTableProjection`, the eleven BIM tables, and the async Parquet daemon view.

## [02]-[COLUMNAR_LANE]

- Owner: `ColumnarProfile` the `[SmartEnum<string>]` analytical-lane axis carrying its required extension roster; `ColumnarExtension` the `[SmartEnum<string>]` extension vocabulary carrying its `INSTALL`/`LOAD` SQL and statically-linked flag; `ColumnarFault` the closed DuckDB-boundary fault discriminated on `DuckDBErrorType`; `ColumnarLane` the static surface owning the connection open, the ordered extension bootstrap, the bulk appender egress, and the columnar query.
- Cases: `ColumnarProfile` is `Geometry` (spatial), `Search` (vss + fts), `Lakehouse` (iceberg + delta + httpfs + postgres), `Bim` (parquet + json over the BimOpenSchema `.duckdb`); `ColumnarExtension` rows are `spatial`, `vss`, `fts`, `parquet`, `json`, `httpfs`, `iceberg`, `delta`, `postgres`, `excel` — a new extension being one row carrying its SQL, never a per-extension package.
- Entry: `public static IO<DuckDBConnection> Open(ColumnarProfile profile, string dataSource)` opens the ADO connection and runs the profile's ordered `INSTALL`/`LOAD` bootstrap before any query; `public static IO<Seq<T>> Query<T>(DuckDBConnection connection, FormattableString sql, Func<DuckDBDataReader, T> shape)` runs a parameterized analytical query; `public static IO<long> Append<T, TMap>(DuckDBConnection connection, string table, Seq<T> rows)` bulk-appends through a `DuckDBMappedAppender<T, TMap>`.
- Auto: the extension bootstrap is profile policy expressed as SQL — `Open` runs the ordered `INSTALL <ext>; LOAD <ext>;` pairs through `DuckDBCommand.ExecuteNonQuery` immediately after the connection opens, the statically-linked extensions (`parquet`/`json`/`icu`) needing no `INSTALL` and the on-demand extensions (`spatial`/`httpfs`/`vss`) downloading once to a deterministic `extension_directory` so a sealed run pre-warms the cache; the loaded set is queried back through `duckdb_extensions()` as a profile receipt; the columnar appender streams rows through `DuckDBMappedAppender` typed `AppendValue` so a `[ValueObject]`/`[SmartEnum]` owner crosses to a column through the `Element/codec#CODEC_AXIS` projection, never a hand-rolled column map; the analytical extract path bridges DuckDB result → Arrow record batch through the `Apache.Arrow.Adbc` driver manager, not a managed DuckDB Arrow member (the v1.5.3 surface exposes none).
- Receipt: a lane open rides `store.columnar.open` carrying the loaded extension set; a query rides `store.columnar.query` carrying the `DuckDBQueryProgress` percentage; an append rides `store.columnar.append` carrying the row count.
- Packages: DuckDB.NET.Data.Full (`DuckDBConnection`/`DuckDBCommand`/`DuckDBMappedAppender`/`DuckDBQueryProgress`/`DuckDBErrorType`), Apache.Arrow, Apache.Arrow.Adbc, ParquetSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new analytical profile is one `ColumnarProfile` row carrying its roster; a new extension is one `ColumnarExtension` row carrying its `INSTALL`/`LOAD` SQL; zero new surface — a per-extension NuGet package, a second analytical engine, inline credentials in a path, or a provider-branded service family is the deleted form because DuckDB is fully-featured through the one centrally pinned runtime and the extension roster is profile policy.
- Boundary: DuckDB is fully-featured through the SQL `INSTALL`/`LOAD` pattern over the one pinned `DuckDB.NET.Data.Full` runtime — no second engine, no per-extension package, the extension roster being profile policy expressed as SQL (`api-duckdb#EXTENSION_PATTERN`); the columnar lane is the `Query/lane#READ_ROUTING` ASYNC analytical lane carrying a `StalenessWatermark`, so an interactive-correctness query never reads here without `WaitForNonStaleProjectionDataAsync` and strong-consistency reads go through the synchronous topology (the DuckDB `ATTACH … TYPE postgres` lane joins the live database analytically but is never the consistency owner, `C2`); spatial→PG GiST and ANN→pgvector are the transactional index owners so DuckDB `spatial`/`vss` are the columnar aggregators only (`L2`), meeting NTS/GDAL at WKB through `ST_AsWKB`/`ST_GeomFromWKB`; credentials are `CREATE SECRET` objects never inline keys; `DuckDBException` lifts at the provider edge discriminated on `DuckDBErrorType` into `ColumnarFault`, never a raw ADO exception.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarExtension {
    public static readonly ColumnarExtension Spatial = new("spatial", linked: false);
    public static readonly ColumnarExtension Vss = new("vss", linked: false);
    public static readonly ColumnarExtension Fts = new("fts", linked: false);
    public static readonly ColumnarExtension Parquet = new("parquet", linked: true);
    public static readonly ColumnarExtension Json = new("json", linked: true);
    public static readonly ColumnarExtension Httpfs = new("httpfs", linked: false);
    public static readonly ColumnarExtension Iceberg = new("iceberg", linked: false);
    public static readonly ColumnarExtension Delta = new("delta", linked: false);
    public static readonly ColumnarExtension Postgres = new("postgres", linked: false);
    public static readonly ColumnarExtension Excel = new("excel", linked: false);
    public bool Linked { get; }
    private ColumnarExtension(string key, bool linked) : this(key) => Linked = linked;
    public string Bootstrap => Linked ? $"LOAD {Key};" : $"INSTALL {Key}; LOAD {Key};";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnarProfile {
    public static readonly ColumnarProfile Geometry = new("geometry", [ColumnarExtension.Spatial, ColumnarExtension.Parquet]);
    public static readonly ColumnarProfile Search = new("search", [ColumnarExtension.Vss, ColumnarExtension.Fts]);
    public static readonly ColumnarProfile Lakehouse = new("lakehouse", [ColumnarExtension.Httpfs, ColumnarExtension.Iceberg, ColumnarExtension.Delta, ColumnarExtension.Postgres]);
    public static readonly ColumnarProfile Bim = new("bim", [ColumnarExtension.Parquet, ColumnarExtension.Json]);
    public Seq<ColumnarExtension> Roster { get; }
    private ColumnarProfile(string key, Seq<ColumnarExtension> roster) : this(key) => Roster = roster;
}

[Union]
public abstract partial record ColumnarFault : Expected, IValidationError<ColumnarFault> {
    private ColumnarFault(string detail, int code) : base(detail, code, None) { }
    public static ColumnarFault Create(string message) => new QueryFailed(message, default);
    public sealed record QueryFailed(string Detail, DuckDBErrorType Kind) : ColumnarFault($"<columnar-query:{Detail}>", 8350);
    public sealed record ExtensionGap(string Extension) : ColumnarFault($"<columnar-extension:{Extension}>", 8351);
}

public static class ColumnarLane {
    public static IO<DuckDBConnection> Open(ColumnarProfile profile, string dataSource) =>
        IO.liftAsync(async () => {
            var connection = new DuckDBConnection(new DuckDBConnectionStringBuilder { DataSource = dataSource }.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            foreach (var extension in profile.Roster) { command.CommandText = extension.Bootstrap; await command.ExecuteNonQueryAsync().ConfigureAwait(false); }
            return connection;
        });

    public static IO<Seq<T>> Query<T>(DuckDBConnection connection, FormattableString sql, Func<DuckDBDataReader, T> shape) =>
        IO.liftAsync(async () => {
            await using var command = connection.CreateCommand();
            command.CommandText = sql.Format;
            for (var i = 0; i < sql.ArgumentCount; i++) command.Parameters.Add(new DuckDBParameter(sql.GetArgument(i)));
            await using var reader = (DuckDBDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
            var rows = Seq<T>();
            while (await reader.ReadAsync().ConfigureAwait(false)) rows = rows.Add(shape(reader));
            return rows;
        }) | @catch<IO, Seq<T>>(static e => e.Exception.Map(static x => x is DuckDBException).IfNone(false),
            e => IO.fail<Seq<T>>(new ColumnarFault.QueryFailed(e.Message, ((DuckDBException)e.ToException()).ErrorType)));

    public static IO<long> Append<T, TMap>(DuckDBConnection connection, string table, Seq<T> rows) where TMap : DuckDBAppenderMap<T>, new() =>
        IO.liftAsync(async () => {
            var appender = connection.CreateAppender<T, TMap>(table);
            await appender.AppendRecordsAsync(rows).ConfigureAwait(false);
            appender.Close();
            return (long)rows.Count;
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | extension bootstrap | ordered `INSTALL`/`LOAD` SQL           | profile policy; no per-extension package, no second engine |
|  [02]   | consistency stance  | async, `StalenessWatermark`            | never read by interactive correctness without the wait    |
|  [03]   | index ownership     | DuckDB spatial/vss are aggregators     | GiST/pgvector own the transactional index (`L2`)          |
|  [04]   | fault rail          | `DuckDBException` → `ColumnarFault`     | discriminated on `DuckDBErrorType`, never a raw ADO throw  |

## [03]-[FLAT_TABLE_EGRESS]

- Owner: `BimOpenSchemaProjection` the co-transactional Marten `FlatTableProjection` writing the columnar BIM tables; `ColumnarView` the async daemon Parquet view; `FlatTableEgress` the static surface owning the eleven-table BimOpenSchema egress and the Parquet daemon materialization.
- Entry: `public sealed class BimOpenSchemaProjection : FlatTableProjection` registered `ProjectionLifecycle.Inline` so the eleven columnar tables write in the append transaction; `public static IO<Unit> Materialize(IDocumentStore store, ColumnarProfile profile, string parquetRoot)` runs the async daemon view producing the Parquet/`.duckdb` the analytical lane reads.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` (`Project<T>(StatementMap)`) written in the same transaction as the events, NOT daemon-lagged (`M4`), because a flat-table analytical view a live QTO reads must be read-your-writes consistent; the structural (EAV-generic) map is Persistence-owned here and a BIM-typed map is a Bim-implemented seam projection; `Ara3D.BimOpenSchema.IO`'s `WriteDuckDB`/`DuckDbUtils.WriteToDuckDB` bulk-appends the eleven suffixed BIM tables (`Entities_4`/`DoubleParameters_6`/…) through a `DuckDBAppender`, and a Persistence analytical query opens that `.duckdb` over the same pinned runtime and SQL-joins the suffixed tables; the async Parquet daemon view is for the heavy aggregation lanes carrying the `StalenessWatermark`.
- Receipt: a flat-table projection rides `store.columnar.flattable` carrying the row count; a Parquet materialization rides `store.columnar.parquet` carrying the file count and the watermark.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`ProjectionLifecycle`), Ara3D.BimOpenSchema, Ara3D.BimOpenSchema.IO, ParquetSharp, DuckDB.NET.Data.Full, LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `Project<T>` statement map; a new analytical view is one async daemon projection; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, or a second columnar file codec beside ParquetSharp is the deleted form because the BimOpenSchema egress is co-transactional and ParquetSharp owns the direct file codec.
- Boundary: the `ElementGraph → Ara3D.BimOpenSchema` egress is a co-transactional `FlatTableProjection` (`M4`) so a live-QTO analytical read is read-your-writes consistent rather than daemon-lagged; if BimOpenSchema is EAV-generic Persistence owns the structural map, if BIM-typed it is a Bim-implemented seam projection (the wire seam, never a sibling reference); the eleven suffixed BIM tables are read with the built-in `parquet`/`json` surface and `spatial`/`vss`/`fts` extend them for geometry/ANN/text analytics over the same `.duckdb`, all on the one pinned runtime; the Parquet file codec is `ParquetSharp` (the native libparquet-cpp read/write the managed Arrow stack lacks), distinct from the DuckDB SQL parquet path, the two meeting at the Parquet file; the lakehouse `delta`/`iceberg` scans read the same tables the managed `DeltaLake.Net` writer produces, DuckDB the read/aggregate projection and the managed writer the system of record, meeting at the table path.

```csharp signature
public sealed class BimOpenSchemaProjection : FlatTableProjection {
    public BimOpenSchemaProjection() : base("bim_entities", SchemaNameSource.DocumentSchema) {
        Project<GraphEvent.GraphCreated>(map => {
            map.Map(static e => e.Header.Schema).NotNull();
            map.Map(static e => e.Header.View);
        });
        Project<GraphEvent.GraphRevised>(map => map.Map(static e => e.Delta.AddedNodes.Count + e.Delta.RevisedNodes.Count));
    }
}

public static class FlatTableEgress {
    public static IO<Unit> Materialize(IDocumentStore store, ColumnarProfile profile, string parquetRoot) =>
        IO.liftAsync(async () => {
            await using var daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            await daemon.WaitForNonStaleData(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            return unit;
        });

    public static IO<long> WriteParquet<T>(string path, Seq<T> rows, Func<T, object[]> project, params (string Name, Type Type)[] columns) =>
        IO.lift(() => {
            using var file = new ParquetSharp.ParquetFileWriter(path, columns.Map(static c => (ParquetSharp.Column)new ParquetSharp.Column<object>(c.Name)).ToArray());
            using var group = file.AppendRowGroup();
            return (long)rows.Count;
        });
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | BimOpenSchema egress| co-transactional `FlatTableProjection` | read-your-writes, never daemon-lagged (`M4`)              |
|  [02]   | BIM tables          | eleven suffixed columnar tables        | read over the one pinned runtime; `spatial`/`vss` extend  |
|  [03]   | Parquet codec       | `ParquetSharp`                         | distinct from the DuckDB SQL parquet path                 |
|  [04]   | lakehouse           | `delta`/`iceberg` scan                 | DuckDB reads, `DeltaLake.Net` writes; meet at the table   |
