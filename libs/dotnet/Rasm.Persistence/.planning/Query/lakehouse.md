# [PERSISTENCE_QUERY_LAKEHOUSE]

Rasm.Persistence owns the COLD TAIL here: the co-transactional flat-table egress that keeps a live QTO read consistent with its own writes, the native Parquet codec both directions of an encrypted generation ride, the partitioned lake scan that serves history back with no engine mount in the loop, and the one landing port every producer family hands a batch to. Producers hand a declared dataset and a record batch; writers, hive layout, generation spelling, and index custody stay here.

Two producer crossings are named and distinct: `Rasm.Element` and `Rasm.Compute` hand their datasets across the `[WIRE]: AnalyticsSchema` boundary, and `Rasm.Materials` hands its catalogue and per-channel texture generations across the `[WIRE]: MaterialsDataset` boundary — two named wires over ONE admitted vocabulary `Query/backend#COLUMN_VOCABULARY` declares, so a landing arm names its producer without a second schema language entering.

## [01]-[INDEX]

- [02]-[FLAT_TABLE_EGRESS]: `BimOpenSchemaProjection` writes the columnar BIM facts in the append transaction, `FlatTableEgress` owns the in-corpus table write, the `ParquetSharp.Arrow` codec both custody arms share, the partitioned scan, the metadata-only Delta commit, and the one `Land` port; `PmeCustody` and `ScanTuning` are the two policy values both Parquet legs take; `LandingArm` and `LakeGeneration` are the producer roster and the one cold-tail directory spelling.
- [03]-[RESEARCH]: open verification debts and their routes.

## [02]-[FLAT_TABLE_EGRESS]

- Owner: `BimOpenSchemaProjection` is the co-transactional Marten `FlatTableProjection` writing the columnar BIM fact table in the append transaction; `FlatTableEgress` is the static surface owning the async daemon materialization, the IN-CORPUS eleven-table `.duckdb` write over the one pinned runtime, the native `ParquetSharp.Arrow` codec lane both directions of an encrypted generation ride, and the one landing port; `PmeCustody` is the ONE encryption custody value the write and the read both take; `ScanTuning` is the ONE read policy both Parquet legs derive their reader and Arrow-decode properties from; `LandingArm` is the producer-family row set and `LakeGeneration` the one owner of a cold-tail directory spelling.
- Entry: `BimOpenSchemaProjection` registers inline columnar facts; `Materialize(IDocumentStore)` runs the async daemon view and returns the measured wait; `WriteFrames(ColumnarSession, BimData)` streams the `ToDataSet()` tables through the raw appender; `ReadParquetFrames`, `ScanDataset`, and `ReadIpcFrames` return owned `IAsyncEnumerable<RecordBatch>` drains over the one `Frames` bracket, both Parquet legs taking the same `Option<PmeCustody>` and the same `ScanTuning`; `WriteParquetFrames(Seq<RecordBatch>, StorePath, AnalyticsSchema, Seq<Identifier> sorted, Option<PmeCustody>, Seq<(string Key, string Value)> metadata)` stages then atomically publishes one generation; `PublishDelta` registers published files metadata-only; `Land(LakeGeneration, AnalyticsSchema, Seq<RecordBatch>, Seq<(string Key, string Value)>, StorePath, Option<PmeCustody>, Func<UInt128, StorePath, IO<Unit>>)` is the ONE landing port.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` written in the same transaction as the events, never daemon-lagged, because a flat analytical view a live QTO reads must be read-your-writes consistent — the structural map maps the `GraphCreated`'s `Header.Schema.Key`/`Header.View.Key` and the `GraphRevised`'s `GraphDelta.NodeCount`/`EdgeCount` through the single-column primary key `FlatTableProjection` requires, since `StatementMap.Map` writes a primitive column and never a smart-enum object. Eleven suffixed BIM tables write IN-CORPUS: `frames.ToDataSet()` projects the fixed-ordinal `IDataSet` whose `Tables` order IS the DuckDB ordinal suffix, and `WriteFrames` folds each `IDataTable` through a `CREATE OR REPLACE TABLE` beside a raw `DuckDBAppender` stream on this lane's session. Every generation's field list, its physical types, its sorting-column ordinals, and its footer metadata derive from ONE `AnalyticsSchema`, so a hand-built `Schema` beside a declared dataset has nothing left to state.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`SchemaNameSource`/`IDocumentStore`/`BuildProjectionDaemonAsync`/`WaitForNonStaleData`), Ara3D.BimOpenSchema (`BimData`/`ToDataSet` — DATA MODEL only post-absorption, `api-ara3d-bimopenschema#IMPLEMENTATION_LAW`), Ara3D.SDK (`IDataSet.Tables`/`IDataTable.Name`/`Rows`/`Columns`/`this[column,row]`/`IDataColumn.ColumnIndex`/`Descriptor`/`IDataDescriptor.Name`/`Type`), DuckDB.NET.Data.Full (`DuckDBAppender.CreateRow`/`IDuckDBAppenderRow.AppendValue`/`AppendNullValue`/`EndRow`/`Close`), ParquetSharp (`Arrow.FileReader`/`Arrow.FileWriter`/`WriterPropertiesBuilder`/`ReaderProperties.GetDefaultReaderProperties`/`SetFooterReadSize`/`SetThriftStringSizeLimit`/`SetThriftContainerSizeLimit`/`FileDecryptionProperties`/`Arrow.ArrowReaderProperties.GetDefault`/`BatchSize`/`UseThreads`/`PreBuffer`/`CacheOptions`; `ParquetSharp.Encryption` `CryptoFactory`/`KmsConnectionConfig`/`EncryptionConfiguration`/`DecryptionConfiguration`), DeltaLake.Net (`DeltaEngine`/`EngineOptions`/`TableOptions`/`AddAction`/`CommitOptions`/`CreateWriteTransactionAsync`/`GetLatestTransactionVersionAsync`/`DeltaLakeException`), ParquetSharp.Dataset (`DatasetReader`/`ToBatches`/`HivePartitioning.Factory`/`Col`/`FilterExtensions`), Apache.Arrow (`RecordBatch`/`Schema`/`IArrowArrayStream`/`ArrowStreamReader`), Apache.Arrow.Compression (`CompressionCodecFactory`), Rasm.Element (`GraphDelta`/`Header`), Rasm.Persistence (`Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ArrowLanding` — the declaration every generation derives from, `Element/graph#STREAM_GRAIN` `GraphEvent`), LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `map.Map` statement; a new analytical generation is one async daemon view; a new frame codec is the existing `ParquetSharp.Arrow` lane reading a new format; an encryption stance is one `PmeCustody` value both directions take; a new lakehouse publication is one `PublishDelta` commit over `AddAction` rows the codec write already computed; a new producer landing is one `LandingArm` row carrying its hive key and write order — schema handoff only, zero new storage code; a read-side retune is one `ScanTuning` value both legs take; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, a second Parquet runtime, a hollow writer, a per-leg drain loop, or a `Schema` assembled beside a declared dataset is the deleted form.
- Law: reader ownership and lease release ride ONE bracket. Each reader opens its chain and hands back the stream beside the leases it acquired, and the drain releases them in REVERSE acquisition order on every outcome — drained, refused, or cancelled — because a lease released before the stream that reads through it faults in native memory where no managed catch can see it. `PmeCustody` factories OUTLIVE every reader they arm: ParquetSharp holds native references inside the decryption properties, which is why custody is a composition-held value and never a per-call construction. Both reader property types MUTATE IN PLACE and every ctor captures by reference, so each derivation mints its own pair — one shared instance propagates a later tune into a scan already streaming.
- Boundary: `FlatTableProjection` requires a single-column primary key and writes a primitive per `StatementMap.Map`, so a `ReleaseVersion`/`ModelView` smart-enum maps as its `.Key` and a `GraphDelta` as its counts, never as the object. Bim-lowered `StorePlan` values execute on this lane's `ColumnarSession` as DATA crossing the same boundary, so the dataset-scale element query runs where the data rests with no Persistence-side predicate vocabulary. Direct SQL consumers reference the `<Name>_<n>` projection-ordinal suffix that IS the real table identity, never a bare table name. `ParquetSharp.Arrow` owns the Parquet file codec — the native read/write the managed Arrow stack lacks — distinct from the DuckDB SQL `read_parquet`/`COPY` path, the three meeting at the file format and the `Apache.Arrow` model owned by `api-arrow`. `Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built at the HELD `1.0.1` pin, the feed-newest `.IO` having regressed to a Windows-only target that is restore-inadmissible here; the ruled escalation is EXECUTED — the consumed write surface is absorbed in-corpus, so those assemblies serve only the in-memory schema model and its `ToDataSet()` projection and never a hot IO loop, and the pin bump is never the fix. `PARTITION_BY` is a pruning instrument at cardinality in the tens to low thousands, never a uniqueness scheme.

```csharp
using Apache.Arrow;
using Ara3D.BimOpenSchema;
using Ara3D.DataTable;
using DeltaLake.Errors;
using DeltaLake.Table;
using DuckDB.NET.Data;
using JasperFx.Events.Daemon;
using LanguageExt;
using Marten;
using Marten.Events.Projections.Flattened;
using NodaTime;
using ParquetSharp;
using ParquetSharp.Arrow;
using ParquetSharp.Encryption;
using Rasm.Domain;
using Rasm.Element.Graph;
using System.Globalization;
using System.Runtime.CompilerServices;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] --------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FlatTableEgress {
    public static IO<Duration> Materialize(IDocumentStore store) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            await using IProjectionDaemon daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            return Fin<Duration>.Succ(await ReadRouter.AwaitNonStale(daemon, QueryLane.Columnar).RunAsync().ConfigureAwait(false));
        }).ConfigureAwait(false)).Bind(IO.lift);

    public static IO<long> WriteFrames(ColumnarSession session, BimData frames) =>
        IO.lift(() => Op.Of().Catch(() => {
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
            return Fin<long>.Succ(written);
        }).MapFail(error => ColumnarFault.Lift(error,
            static (cause, engine) => new ColumnarFault.AppendRefused("<bim-frames>", engine.ErrorType, cause))));

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

    public sealed record PmeCustody(CryptoFactory Crypto, KmsConnectionConfig Kms, EncryptionConfiguration Encrypt, DecryptionConfiguration Decrypt);

    public readonly record struct ScanTuning(
        long FooterReadSize, int ThriftStringLimit, int ThriftContainerLimit,
        long BatchSize, bool PreBuffer, bool Threaded, CacheOptions Cache) {
        public static readonly ScanTuning Lake = new(
            FooterReadSize: 256L * 1024, ThriftStringLimit: 100 * 1024 * 1024, ThriftContainerLimit: 100 * 1024 * 1024,
            BatchSize: 65_536L, PreBuffer: true, Threaded: true,
            Cache: new CacheOptions(hole_size_limit: 8L * 1024 * 1024, range_size_limit: 32L * 1024 * 1024, lazy: true, prefetch_limit: 8L));

        public ReaderProperties Reader(Option<PmeCustody> custody, Option<StorePath> path) {
            ReaderProperties properties = ReaderProperties.GetDefaultReaderProperties();
            properties.SetFooterReadSize(FooterReadSize);
            properties.SetThriftStringSizeLimit(ThriftStringLimit);
            properties.SetThriftContainerSizeLimit(ThriftContainerLimit);
            custody.Iter(pme => properties.FileDecryptionProperties = pme.Crypto.GetFileDecryptionProperties(
                pme.Kms, pme.Decrypt, path.Match<string?>(Some: static held => (string)held, None: static () => null)));
            return properties;
        }

        public ArrowReaderProperties Arrow() {
            ArrowReaderProperties properties = ArrowReaderProperties.GetDefault();
            (properties.BatchSize, properties.UseThreads, properties.PreBuffer, properties.CacheOptions) =
                (BatchSize, Threaded, PreBuffer, Cache);
            return properties;
        }
    }

    // --- [FRAME_DRAINS]
    static async IAsyncEnumerable<RecordBatch> Frames(Func<(IArrowArrayStream Stream, Seq<IDisposable> Leases)> open,
        [EnumeratorCancellation] CancellationToken token = default) {
        (IArrowArrayStream stream, Seq<IDisposable> leases) = open();
        try {
            while (await stream.ReadNextRecordBatchAsync(token).ConfigureAwait(false) is { } batch) { yield return batch; }
        }
        finally {
            stream.Dispose();
            leases.Rev().Iter(static lease => lease.Dispose());
        }
    }

    public static IAsyncEnumerable<RecordBatch> ReadParquetFrames(StorePath parquetPath, Option<PmeCustody> custody,
        ScanTuning tuning, CancellationToken token = default) =>
        Frames(() => {
            ReaderProperties properties = tuning.Reader(custody, Some(parquetPath));
            FileStream handle = File.OpenRead((string)parquetPath);
            FileReader reader = new(handle, properties);
            return (reader.GetRecordBatchReader(), Seq<IDisposable>(properties, handle, reader));
        }, token);

    public static IAsyncEnumerable<RecordBatch> ScanDataset(StorePath root, Option<ParquetSharp.Dataset.Filter.IFilter> filter,
        Seq<Identifier> columns, Option<PmeCustody> custody, ScanTuning tuning, CancellationToken token = default) =>
        Frames(() => {
            ReaderProperties properties = tuning.Reader(custody, Option<StorePath>.None);
            ArrowReaderProperties arrow = tuning.Arrow();
            ParquetSharp.Dataset.DatasetReader dataset = new((string)root,
                new ParquetSharp.Dataset.Partitioning.HivePartitioning.Factory(),
                schema: null, readerProperties: properties, arrowReaderProperties: arrow);
            return (dataset.ToBatches(
                    filter.Match<ParquetSharp.Dataset.Filter.IFilter?>(Some: static held => held, None: static () => null),
                    columns.IsEmpty ? null : [.. columns.Map(static column => (string)column)]),
                Seq<IDisposable>(properties, arrow));
        }, token);

    static readonly Apache.Arrow.Compression.CompressionCodecFactory IpcCodecs = new();

    public static IAsyncEnumerable<RecordBatch> ReadIpcFrames(Stream carrier, CancellationToken token = default) =>
        Frames(() => (new ArrowStreamReader(carrier, IpcCodecs), Seq<IDisposable>()), token);

    // --- [GENERATION_WRITE]
    public static IO<Fin<long>> WriteParquetFrames(Seq<RecordBatch> batches, StorePath path, AnalyticsSchema declaration,
        Seq<Identifier> sorted, Option<PmeCustody> custody, Seq<(string Key, string Value)> metadata) =>
        Ordered(declaration, sorted).Match(
            Succ: order => IO.lift<Fin<long>>(() => {
                string published = (string)path;
                return Optional(Path.GetDirectoryName(published))
                    .ToFin(new ColumnarFault.PolicyRefused("generation-directory", published))
                    .Bind(directory => Publish(batches, published, directory, declaration.Fields(metadata), order, custody));
            }),
            Fail: error => IO.pure(Fin<long>.Fail(error)));

    static Fin<WriterProperties.SortingColumn[]> Ordered(AnalyticsSchema declaration, Seq<Identifier> sorted) =>
        sorted.Traverse(column => declaration.Ordinal(column) is int at && at >= 0
            ? Success<Error, WriterProperties.SortingColumn>(
                new WriterProperties.SortingColumn { ColumnIndex = at, IsDescending = false, NullsFirst = false })
            : Fail<Error, WriterProperties.SortingColumn>(
                new ColumnarFault.PolicyRefused("generation-sort", $"{declaration.Dataset}.{(string)column}")))
            .As().Map(static columns => columns.ToArray()).ToFin();

    static Fin<long> Publish(Seq<RecordBatch> batches, string published, string directory, Schema fields,
        WriterProperties.SortingColumn[] order, Option<PmeCustody> custody) =>
        Op.Of().Catch(() => {
            Directory.CreateDirectory(directory);
            string staging = Path.Combine(directory, $".{Path.GetFileName(published)}.{Guid.CreateVersion7():N}.tmp");
            using WriterProperties properties = custody.Match(
                Some: pme => Tuned(new WriterPropertiesBuilder().Encryption(pme.Crypto.GetFileEncryptionProperties(pme.Kms, pme.Encrypt, published)), order).Build(),
                None: () => Tuned(new WriterPropertiesBuilder(), order).Build());
            try {
                using (FileWriter writer = new(File.Open(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None), fields, properties, null, leaveOpen: false)) {
                    foreach (RecordBatch batch in batches) { writer.WriteRecordBatch(batch); }
                }
                File.Move(staging, published, overwrite: false);
                return Fin.Succ((long)batches.Count);
            }
            finally {
                if (File.Exists(staging)) { File.Delete(staging); }
            }
        });

    static WriterPropertiesBuilder Tuned(WriterPropertiesBuilder builder, WriterProperties.SortingColumn[] order) =>
        builder.EnableStatistics()
            .EnableWritePageIndex()
            .SetSizeStatisticsLevel(SizeStatisticsLevel.PageAndColumnChunk)
            .SortingColumns(order);

    public static IO<Fin<long>> PublishDelta(TableOptions table, Seq<AddAction> files, Identifier appId, long asOfVersion) =>
        IO.liftAsync(async () => (await Op.Of().Catch(async _ => {
            using DeltaEngine engine = new(EngineOptions.Default);
            using DeltaTable delta = await engine.LoadTableAsync(table, CancellationToken.None).ConfigureAwait(false);
            long? held = await delta.GetLatestTransactionVersionAsync((string)appId, CancellationToken.None).ConfigureAwait(false);
            if (held is { } committed && committed >= asOfVersion) { return Fin.Succ(committed); }
            await delta.CreateWriteTransactionAsync([.. files], new CommitOptions { AppId = (string)appId, TransactionVersion = asOfVersion }, CancellationToken.None).ConfigureAwait(false);
            return Fin.Succ(asOfVersion);
        }).ConfigureAwait(false)).MapFail(static error => error.Exception.Case is DeltaLakeException
            ? new ColumnarFault.DeltaRefused("<flat-table-generation>", error)
            : error));

    // --- [LANDING_PORT]
    public static IO<Fin<long>> Land(LakeGeneration generation, AnalyticsSchema declaration, Seq<RecordBatch> batches,
        Seq<(string Key, string Value)> metadata, StorePath root, Option<PmeCustody> pme,
        Func<UInt128, StorePath, IO<Unit>> custody) {
        StorePath published = generation.Path(root);
        return WriteParquetFrames(batches, published, declaration, generation.Arm.Sorted, pme, metadata)
            .Bind(written => written.Match(
                Succ: count => (custody(generation.GenerationKey, published).Map(_ => Fin.Succ(count))
                    | @catch<IO, Fin<long>>(static _ => true,
                        error => Unpublish(published).Map(_ => Fin.Fail<long>(error)))).As(),
                Fail: error => IO.pure(Fin<long>.Fail(error))));
    }

    static IO<Unit> Unpublish(StorePath published) =>
        IO.lift(() => Op.Of().Catch(() => {
            string path = (string)published;
            if (File.Exists(path)) { File.Delete(path); }
            string? generation = Path.GetDirectoryName(path);
            if (generation is not null && Directory.Exists(generation) && !Directory.EnumerateFileSystemEntries(generation).Any()) {
                Directory.Delete(generation);
            }
            return Fin<Unit>.Succ(unit);
        }));
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LandingArm {
    public static readonly LandingArm Geometry  = new("geometry", "model", ["node"]);
    public static readonly LandingArm Doe       = new("doe", "study", ["run"]);
    public static readonly LandingArm Tabulate  = new("tabulate", "model", ["entity"]);
    public static readonly LandingArm Materials = new("materials", "catalogue", ["material"]);
    public static readonly LandingArm MaterialsTexture = new("materials-texture", "channel", ["set"]);
    public static readonly LandingArm Cost      = new("cost", "month", ["kind"]);

    public Identifier Partition { get; }
    public Seq<Identifier> Sorted { get; }

    private LandingArm(string key, string partition, Seq<string> sorted) : this(key) =>
        (Partition, Sorted) = (Identifier.Create(partition), sorted.Map(Identifier.Create));
}

public readonly record struct LakeGeneration(
    LandingArm Arm, TenantContext Tenant, Identifier Segment, UInt128 SchemaKey, UInt128 GenerationKey) {
    public StorePath Path(StorePath root) =>
        StorePath.Create(string.Create(CultureInfo.InvariantCulture,
            $"{(string)root}/{(string)Backend.TenantColumn}={Tenant.Entry}/{(string)Arm.Partition}={(string)Segment}"
            + $"/schema={SchemaKey:x32}/generation={GenerationKey:x32}/data.parquet"));
}
```

| [INDEX] | [POLICY]             | [VALUE]                                      | [BINDING]                                                       |
| :-----: | :------------------- | :------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | BimOpenSchema egress | co-transactional `FlatTableProjection`       | read-your-writes, never daemon-lagged                           |
|  [02]   | column value         | smart-enum `.Key` / `GraphDelta` count       | `StatementMap.Map` writes a primitive, never the object         |
|  [03]   | BIM tables           | eleven suffixed columnar tables              | written in-corpus; the DEBUG-IL writer stays off the hot path   |
|  [04]   | Parquet codec        | `ParquetSharp.Arrow` ↔ `RecordBatch` codec   | distinct from the DuckDB SQL parquet path; meet at the file     |
|  [05]   | encrypted extract    | one `PmeCustody` on the write AND the read   | the factory outlives every reader it arms; never a split owner  |
|  [06]   | lakehouse            | `PublishDelta` metadata-only commit          | `AddAction` registration; `TransactionVersion` = the AS-OF cut  |
|  [07]   | frame drain          | one `Frames` open-and-lease bracket          | reverse release on every outcome; never a per-leg loop          |
|  [08]   | lake scan            | `DatasetReader` + `HivePartitioning.Factory` | `Col`/`IFilter` pushdown; no engine mount in the loop           |
|  [09]   | read tuning          | one `ScanTuning` across both Parquet legs    | footer bounds, range coalescing, batch grain; never one leg     |
|  [10]   | carrier decode       | one `CompressionCodecFactory` at ingest      | identity reads decompressed bytes; framing never enters keys    |
|  [11]   | generation schema    | one `AnalyticsSchema` per landed generation  | fields, metadata, and sort ordinals derive from one declaration |
|  [12]   | landing spine        | `LandingArm` row per producer dataset shape  | schema handoff only; storage and custody stay here              |
|  [13]   | generation directory | `LakeGeneration` tenant/segment/key spelling | hive key wins a body-column collision; segment derives once     |

## [03]-[RESEARCH]

(none)
