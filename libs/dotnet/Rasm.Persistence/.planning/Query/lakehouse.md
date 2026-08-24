# [PERSISTENCE_QUERY_LAKEHOUSE]

Rasm.Persistence owns the COLD TAIL here: the co-transactional flat-table egress that keeps a live QTO read consistent with its own writes, the native Parquet codec both directions of an encrypted generation ride, the partitioned lake scan that serves history back with no engine mount in the loop, and the one landing port every producer family hands a batch to. Producers hand a declared dataset and a record batch; writers, hive residence, generation spelling, and index custody stay here.

Two producer crossings are named and distinct: `Rasm.Element` and `Rasm.Compute` hand their datasets across the `[WIRE]: AnalyticsSchema` seam, and `Rasm.Materials` hands its catalogue and per-channel texture generations across the `[WIRE]: MaterialsDataset` seam — two named wires over ONE admitted vocabulary `Query/residence#COLUMN_VOCABULARY` declares, so a landing arm names its producer without a second schema language entering.

## [01]-[INDEX]

- [02]-[FLAT_TABLE_EGRESS]: `BimOpenSchemaProjection` writes the columnar BIM facts in the append transaction, `FlatTableEgress` owns the in-corpus table write, the `ParquetSharp.Arrow` codec both custody arms share, the partitioned scan, the metadata-only Delta commit, and the one `Land` port; `PmeCustody` and `ScanTuning` are the two policy values both Parquet legs take; `LandingArm` and `LakeGeneration` are the producer roster and the one cold-tail directory spelling.
- [03]-[RESEARCH]: open verification debts and their routes.

## [02]-[FLAT_TABLE_EGRESS]

- Owner: `BimOpenSchemaProjection` is the co-transactional Marten `FlatTableProjection` writing the columnar BIM fact table in the append transaction; `FlatTableEgress` is the static surface owning the async daemon materialization, the IN-CORPUS eleven-table `.duckdb` write over the one pinned runtime, the native `ParquetSharp.Arrow` codec lane both directions of an encrypted generation ride, and the one landing port; `PmeCustody` is the ONE encryption custody value the write and the read both take; `ScanTuning` is the ONE read policy both Parquet legs derive their reader and Arrow-decode properties from; `LandingArm` is the producer-family row set and `LakeGeneration` the one owner of a cold-tail directory spelling.
- Entry: `BimOpenSchemaProjection` registers inline columnar facts; `Materialize(IDocumentStore)` runs the async daemon view and returns the measured wait; `WriteFrames(ColumnarSession, BimData)` streams the `ToDataSet()` tables through the raw appender; `ReadParquetFrames`, `ScanDataset`, and `ReadIpcFrames` return owned `IAsyncEnumerable<RecordBatch>` drains over the one `Frames` bracket, both Parquet legs taking the same `Option<PmeCustody>` and the same `ScanTuning`; `WriteParquetFrames(Seq<RecordBatch>, StorePath, AnalyticsSchema, Seq<Identifier> sorted, Option<PmeCustody>, Seq<(string Key, string Value)> metadata)` stages then atomically publishes one generation; `PublishDelta` registers published files metadata-only; `Land(LakeGeneration, AnalyticsSchema, Seq<RecordBatch>, Seq<(string Key, string Value)>, StorePath, Option<PmeCustody>, Func<UInt128, StorePath, IO<Unit>>)` is the ONE landing port.
- Auto: the `ElementGraph → BimOpenSchema` egress is a CO-TRANSACTIONAL `FlatTableProjection` written in the same transaction as the events, never daemon-lagged, because a flat analytical view a live QTO reads must be read-your-writes consistent — the structural map maps the `GraphCreated`'s `Header.Schema.Key`/`Header.View.Key` and the `GraphRevised`'s `GraphDelta.NodeCount`/`EdgeCount` through the single-column primary key `FlatTableProjection` requires, since `StatementMap.Map` writes a primitive column and never a smart-enum object. Eleven suffixed BIM tables write IN-CORPUS: `frames.ToDataSet()` projects the fixed-ordinal `IDataSet` whose `Tables` order IS the DuckDB ordinal suffix, and `WriteFrames` folds each `IDataTable` through a `CREATE OR REPLACE TABLE` beside a raw `DuckDBAppender` stream on this lane's session. Every generation's field list, its physical types, its sorting-column ordinals, and its footer metadata derive from ONE `AnalyticsSchema`, so a hand-built `Schema` beside a declared dataset has nothing left to state.
- Receipt: a flat-table projection rides `store.columnar.flattable` carrying the change magnitude; a daemon materialization rides `store.columnar.materialize` carrying the watermark; a frame write rides `store.columnar.frames` carrying the table count; a Parquet read rides `store.columnar.parquet` carrying the record-batch count; a landing rides its `LandingArm.Slot`.
- Packages: Marten (`FlatTableProjection`/`StatementMap`/`SchemaNameSource`/`IDocumentStore`/`BuildProjectionDaemonAsync`/`WaitForNonStaleData`), Ara3D.BimOpenSchema (`BimData`/`ToDataSet` — DATA MODEL only post-absorption, `api-ara3d-bimopenschema#IMPLEMENTATION_LAW`), Ara3D.SDK (`IDataSet.Tables`/`IDataTable.Name`/`Rows`/`Columns`/`this[column,row]`/`IDataColumn.ColumnIndex`/`Descriptor`/`IDataDescriptor.Name`/`Type`), DuckDB.NET.Data.Full (`DuckDBAppender.CreateRow`/`IDuckDBAppenderRow.AppendValue`/`AppendNullValue`/`EndRow`/`Close`), ParquetSharp (`Arrow.FileReader`/`Arrow.FileWriter`/`WriterPropertiesBuilder`/`ReaderProperties.GetDefaultReaderProperties`/`SetFooterReadSize`/`SetThriftStringSizeLimit`/`SetThriftContainerSizeLimit`/`FileDecryptionProperties`/`Arrow.ArrowReaderProperties.GetDefault`/`BatchSize`/`UseThreads`/`PreBuffer`/`CacheOptions`; `ParquetSharp.Encryption` `CryptoFactory`/`KmsConnectionConfig`/`EncryptionConfiguration`/`DecryptionConfiguration`), DeltaLake.Net (`DeltaEngine`/`EngineOptions`/`TableOptions`/`AddAction`/`CommitOptions`/`CreateWriteTransactionAsync`/`GetLatestTransactionVersionAsync`/`DeltaLakeException`), ParquetSharp.Dataset (`DatasetReader`/`ToBatches`/`HivePartitioning.Factory`/`Col`/`FilterExtensions`), Apache.Arrow (`RecordBatch`/`Schema`/`IArrowArrayStream`/`ArrowStreamReader`), Apache.Arrow.Compression (`CompressionCodecFactory`), Rasm.Element (`GraphDelta`/`Header`), Rasm.Persistence (`Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ArrowLanding` — the declaration every generation derives from, `Element/graph#STREAM_GRAIN` `GraphEvent`), LanguageExt.Core, BCL inbox.
- Growth: a new flat-table column is one `map.Map` statement; a new analytical generation is one async daemon view; a new frame codec is the existing `ParquetSharp.Arrow` lane reading a new format; an encryption stance is one `PmeCustody` value both directions take; a new lakehouse publication is one `PublishDelta` commit over `AddAction` rows the codec write already computed; a new producer landing is one `LandingArm` row carrying its slot, hive key, and write order — schema handoff only, zero new storage code; a read-side retune is one `ScanTuning` value both legs take; zero new surface — a daemon-lagged BimOpenSchema egress, a hand-rolled columnar map, a second Parquet runtime, a hollow writer, a per-leg drain loop, or a `Schema` assembled beside a declared dataset is the deleted form.
- Law: reader ownership and lease release ride ONE bracket. Each reader opens its chain and hands back the stream beside the leases it acquired, and the drain releases them in REVERSE acquisition order on every outcome — drained, refused, or cancelled — because a lease released before the stream that reads through it faults in native memory where no managed catch can see it. `PmeCustody` factories OUTLIVE every reader they arm: ParquetSharp holds native references inside the decryption properties, which is why custody is a composition-held value and never a per-call construction. Both reader property types MUTATE IN PLACE and every ctor captures by reference, so each derivation mints its own pair — one shared instance propagates a later tune into a scan already streaming.
- Boundary: `FlatTableProjection` requires a single-column primary key and writes a primitive per `StatementMap.Map`, so a `ReleaseVersion`/`ModelView` smart-enum maps as its `.Key` and a `GraphDelta` as its counts, never as the object. Bim-lowered `StorePlan` values execute on this lane's `ColumnarSession` as DATA crossing the same seam, so the estate-scale element query runs where the data rests with no Persistence-side predicate vocabulary. Direct SQL consumers reference the `<Name>_<n>` projection-ordinal suffix that IS the real table identity, never a bare table name. `ParquetSharp.Arrow` owns the Parquet file codec — the native read/write the managed Arrow stack lacks — distinct from the DuckDB SQL `read_parquet`/`COPY` path, the three meeting at the file format and the `Apache.Arrow` model owned by `api-arrow`. `Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built at the HELD `1.0.1` pin, the feed-newest `.IO` having regressed to a Windows-only target that is restore-inadmissible here; the ruled escalation is EXECUTED — the consumed write surface is absorbed in-corpus, so those assemblies serve only the in-memory schema model and its `ToDataSet()` projection and never a hot IO loop, and the pin bump is never the fix. `PARTITION_BY` is a pruning instrument at cardinality in the tens to low thousands, never a uniqueness scheme.

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
using Marten.Events.Projections.Flattened;
using NodaTime;
using ParquetSharp;
using ParquetSharp.Arrow;
using ParquetSharp.Encryption;
using Rasm.Domain;                                // TenantContext — the tenancy prefix a lake generation rests under
using Rasm.Element.Graph;
using System.Globalization;                       // CultureInfo — the invariant generation-directory spelling
using System.Runtime.CompilerServices;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] -----------------------------------------------------------------------------
// Inline lifecycle preserves read-your-writes correctness for live QTO; the map writes primitive header keys and
// delta magnitudes under a single primary key because that is all `StatementMap.Map` carries.
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
    // Daemon materialization waits for non-stale state before heavy analytical scans and returns the MEASURED wait.
    // Inline projection remains the same-commit correctness owner.
    public static IO<Duration> Materialize(IDocumentStore store) =>
        IO.liftAsync(async () => await Op.Of().Catch(async _ => {
            await using IProjectionDaemon daemon = await store.BuildProjectionDaemonAsync().ConfigureAwait(false);
            await daemon.StartAllAsync().ConfigureAwait(false);
            return Fin<Duration>.Succ(await ReadRouter.AwaitNonStale(daemon, QueryLane.Columnar).RunAsync().ConfigureAwait(false));
        }).ConfigureAwait(false)).Bind(IO.liftFin);

    // `<Name>_<n>` IS the admitted table identity a direct SQL consumer references, so the ordinal the projection
    // emitted at travels into the relation name rather than into a mapping table beside it.
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
            static (cause, engine) => new ColumnarFault.AppendRefused("<bim-frames>", engine.ErrorType, cause))))
        .Bind(IO.liftFin);

    // BIM fact tables arrive as an EAV `IDataSet` carrying CLR values and no residence declaration, so this pair
    // holds the one place a CLR type decides a column — the declared `ColumnType` correspondence governs every
    // dataset that crosses a seam, and this corpus-absorbed projection crosses none.
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
    // read derives file decryption properties from the SAME factory, so a generation written encrypted is readable by
    // construction rather than by a second owner nobody wired.
    public sealed record PmeCustody(CryptoFactory Crypto, KmsConnectionConfig Kms, EncryptionConfiguration Encrypt, DecryptionConfiguration Decrypt);

    // ONE read policy both Parquet read legs take: the single-file read and its multi-file counterpart rest on the
    // SAME object-store residence, so a lane tuned on one and left at native defaults on the other coalesces ranges
    // under two regimes over identical bytes. Two thrift ceilings bound footer deserialization against whatever
    // length the FILE declares, so an untrusted generation refuses at a stated ceiling rather than allocating against
    // own header; `PreBuffer` with a sized `CacheOptions` IS the native range coalescing a request layer would
    // re-implement, while `BatchSize` sets the record-batch grain every downstream fold reads.
    public readonly record struct ScanTuning(
        long FooterReadSize, int ThriftStringLimit, int ThriftContainerLimit,
        long BatchSize, bool PreBuffer, bool Threaded, CacheOptions Cache) {
        public static readonly ScanTuning Residence = new(
            FooterReadSize: 256L * 1024, ThriftStringLimit: 100 * 1024 * 1024, ThriftContainerLimit: 100 * 1024 * 1024,
            BatchSize: 65_536L, PreBuffer: true, Threaded: true,
            Cache: new CacheOptions(hole_size_limit: 8L * 1024 * 1024, range_size_limit: 32L * 1024 * 1024, lazy: true, prefetch_limit: 8L));

        // `path` is `Some` on the single-file leg and `None` on a multi-file scan, and that is a capability boundary
        // rather than a convenience: external key material keys off the path each generation carries, which one
        // properties value cannot supply across a tree, so a scan binds INTERNAL key material alone and an
        // external-material generation reads single-file.
        public ReaderProperties Reader(Option<PmeCustody> custody, Option<StorePath> path) {
            ReaderProperties properties = ReaderProperties.GetDefaultReaderProperties();
            properties.SetFooterReadSize(FooterReadSize);
            properties.SetThriftStringSizeLimit(ThriftStringLimit);
            properties.SetThriftContainerSizeLimit(ThriftContainerLimit);
            custody.Iter(pme => properties.FileDecryptionProperties = pme.Crypto.GetFileDecryptionProperties(
                pme.Kms, pme.Decrypt, path.Match<string?>(Some: static held => (string)held, None: static () => null)));
            return properties;
        }

        // `CacheOptions` is a mutable struct behind a property, so it assigns WHOLE — a field write through the getter
        // mutates a copy the reader never sees.
        public ArrowReaderProperties Arrow() {
            ArrowReaderProperties properties = ArrowReaderProperties.GetDefault();
            (properties.BatchSize, properties.UseThreads, properties.PreBuffer, properties.CacheOptions) =
                (BatchSize, Threaded, PreBuffer, Cache);
            return properties;
        }
    }

    // --- [FRAME_DRAINS]
    // ONE drain for every reader: the OPEN and its leases arrive as one value, so a leg cannot acquire a lease the
    // release path never learned about. The iterator's `finally` IS the bracket a streaming shape admits — an
    // `IO.Bracket` collapses to a value and a record-batch drain must stay lazy to bound memory at the batch grain —
    // and it releases in REVERSE acquisition order on every outcome, drained, refused, or cancelled alike.
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

    // Decryption is the symmetric `Option` of the write's own custody value, so an encrypted generation and a plain
    // one read through one member. The file handle rides the lease set: `Arrow.FileReader` adopts the stream and
    // publishes no leave-open switch, so a handle left off the chain outlives the drain that opened it.
    public static IAsyncEnumerable<RecordBatch> ReadParquetFrames(StorePath parquetPath, Option<PmeCustody> custody,
        ScanTuning tuning, CancellationToken token = default) =>
        Frames(() => {
            ReaderProperties properties = tuning.Reader(custody, Some(parquetPath));
            FileStream handle = File.OpenRead((string)parquetPath);
            FileReader reader = new(handle, properties);
            return (reader.GetRecordBatchReader(), Seq<IDisposable>(properties, handle, reader));
        }, token);

    // Partitioned lake scan — the multi-file counterpart: the hive scheme infers from the `key=value` directory tree,
    // `Col`-rooted predicates and column projection push down to partition, row-group-statistics, and row grain,
    // and survivors stream back as one Arrow lane with no engine mount in the loop. `DatasetReader` implements no
    // disposal interface and exposes no `Dispose` member, so it carries no lease and the two property values are the
    // whole chain — an absent `using` here is the type's own shape, not a leak.
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

    // Compressed-carrier decode arm: sibling-minted Arrow IPC wires may arrive with transport-band `Lz4Frame`/`Zstd`
    // block compression, so every ingest reader passes the ONE codec factory — and every `ContentAddress` derivation
    // reads the DECOMPRESSED canonical bytes, so transport framing never enters identity. The carrier stays
    // caller-owned, which is why it is not on the lease chain.
    static readonly Apache.Arrow.Compression.CompressionCodecFactory IpcCodecs = new();

    public static IAsyncEnumerable<RecordBatch> ReadIpcFrames(Stream carrier, CancellationToken token = default) =>
        Frames(() => (new ArrowStreamReader(carrier, IpcCodecs), Seq<IDisposable>()), token);

    // --- [GENERATION_WRITE]
    // Declarations supply the file schema, its metadata, and every sorting-column ordinal, so a generation, the
    // relation a residence plants for the same dataset, and every reader's ordinals derive from one row set. A sort
    // column the dataset never declared refuses HERE: `Ordinal` answers `-1` for an unknown name and the native
    // builder reads that as a column index, stamping sorting metadata pointing at nothing.
    //
    // This leg states its own pushdown grain as "partition, row-group-statistics, and row", and only the page index
    // plus declared sorting columns make the row-grain skip real. The size-statistics level and the page index ARM
    // TOGETHER by the codec's own coupling — a `PageAndColumnChunk` level with the index disabled writes no
    // page-level statistics at all and degrades silently to column-chunk grain — so the two ride one fold.
    public static IO<Fin<long>> WriteParquetFrames(Seq<RecordBatch> batches, StorePath path, AnalyticsSchema declaration,
        Seq<Identifier> sorted, Option<PmeCustody> custody, Seq<(string Key, string Value)> metadata) =>
        Ordered(declaration, sorted).Match(
            Succ: order => IO.lift<Fin<long>>(() => {
                string published = (string)path;
                return Optional(Path.GetDirectoryName(published)).Match(
                    Some: directory => Publish(batches, published, directory, declaration.Fields(metadata), order, custody),
                    None: () => Fin.Fail<long>(new ColumnarFault.PolicyRefused("generation-directory", published)));
            }),
            Fail: error => IO.pure(Fin<long>.Fail(error)));

    // Sorting columns ACCUMULATE their refusals: a generation naming three undeclared sort columns reports all three,
    // because the write is the round trip and a producer cannot see the second bad name after the first.
    static Fin<WriterProperties.SortingColumn[]> Ordered(AnalyticsSchema declaration, Seq<Identifier> sorted) =>
        sorted.Traverse(column => declaration.Ordinal(column) is int at && at >= 0
            ? Success<Error, WriterProperties.SortingColumn>(
                new WriterProperties.SortingColumn { ColumnIndex = at, IsDescending = false, NullsFirst = false })
            : Fail<Error, WriterProperties.SortingColumn>(
                new ColumnarFault.PolicyRefused("generation-sort", $"{declaration.Dataset}.{(string)column}")))
            .As().Map(static columns => columns.ToArray()).ToFin();

    // Publication is the ATOMIC-WRITE protocol: the body writes to a hidden sibling and moves into place, so a
    // reader never observes a partial generation and a failed write leaves the destination untouched. The `COPY` and
    // this codec are both filesystem effects outside transaction rollback, which is why the stage-then-move IS the
    // cleanup rather than a transactional one.
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

    // ONE tuning fold both custody arms take, so an encrypted generation and a plain one carry identical read
    // geometry. `DefaultWriterProperties` is process-global ambient policy no per-file builder can scope, so every
    // knob this generation needs is stated on its own builder rather than inherited from a static another
    // composition may have set.
    static WriterPropertiesBuilder Tuned(WriterPropertiesBuilder builder, WriterProperties.SortingColumn[] order) =>
        builder.EnableStatistics()
            .EnableWritePageIndex()
            .SetSizeStatisticsLevel(SizeStatisticsLevel.PageAndColumnChunk)
            .SortingColumns(order);

    // App and transaction versions enforce exactly-once publication after the latest-version pre-check, so a replayed
    // commit of one generation resolves to the version already held rather than to a duplicate registration.
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
    // ONE landing discipline for every producer arm: the write rides the standing Parquet codec into the arm's hive
    // generation keyed by the producer's schema-identity content key, and custody registers the content-keyed
    // residence on the `Query/cache#ARTIFACT_BLOB_INDEX`. A producer reaches this port with a DECLARATION and its
    // batches — a row-major producer folds its typed rows through `Query/residence#COLUMN_VOCABULARY`'s
    // `ArrowLanding.Build` against that same declaration and hands the batch, while a producer whose bytes are already
    // contiguous wraps its arena and hands that, so neither re-declares field order and no builder copies bytes the
    // caller already laid out. Custody is the visibility gate: a custody failure unpublishes its generation before the
    // typed `UnstampedArtifact` fault returns, so `ScanDataset` never serves an unregistered generation while a
    // retry of that same generation key re-lands clean through the `CreateNew` stage; custody keeps its original
    // `Error` after compensation rather than reminting it as an artifact-shape refusal.
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

    // Custody-failure compensation: delete the published body and prune the emptied generation directory so the hive
    // tree never carries an index-less generation and the same generation key re-publishes without collision.
    static IO<Unit> Unpublish(StorePath published) =>
        IO.lift(() => Op.Of().Catch(() => {
            string path = (string)published;
            if (File.Exists(path)) { File.Delete(path); }
            string? generation = Path.GetDirectoryName(path);
            if (generation is not null && Directory.Exists(generation) && !Directory.EnumerateFileSystemEntries(generation).Any()) {
                Directory.Delete(generation);
            }
            return Fin<Unit>.Succ(unit);
        })).Bind(IO.liftFin);
}

// --- [TABLES] -----------------------------------------------------------------------------
// Landing spine rows: each producer family hands a declared dataset and a typed record batch, and this custodian owns
// writers, residence, slots, index custody, and batch-metadata preservation — a NEW producer is one row, zero new
// storage code. `Partition` names the hive KEY the arm's generation directories carry and the landed value fills it.
// One producer PACKAGE may hold more than one arm: the arm is the DATASET SHAPE, so two datasets whose generations
// prune on different segments are two arms.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LandingArm {
    // Geometry wires key by the kernel `ContentHash` schema-identity law; every other arm keys by its own suite
    // content address.
    public static readonly LandingArm Geometry  = new("geometry", "store.geometry.land", "model", ["node"]);
    public static readonly LandingArm Doe       = new("doe", "store.doe.land", "study", ["run"]);
    public static readonly LandingArm Tabulate  = new("tabulate", "store.tabulate.land", "model", ["entity"]);
    // Both Materials arms name the `[WIRE]: MaterialsDataset` crossing, one wire over the same admitted column
    // vocabulary the `[WIRE]: AnalyticsSchema` producers cross on.
    public static readonly LandingArm Materials = new("materials", "store.materials.land", "catalogue", ["material"]);
    // Texture-plane analytics land on their OWN arm partitioned by `channel`, never folded under the catalogue arm: a
    // catalogue generation is one row per material while a texture generation is one row per CHANNEL of a set, so
    // sharing the arm splits the catalogue tree at a segment a catalogue scan cannot prune on. `channel` carries the
    // producer's CANONICAL name (`base_color`, `geometry_normal`, `orm`) and never an ingest alias (`basecolor`,
    // `albedo`), because two spellings of one channel split its tree into halves no board joins.
    public static readonly LandingArm MaterialsTexture = new("materials-texture", "store.materials.texture.land", "channel", ["set"]);
    // Receipt evidence lands under its capability domain, so a cold-tail scan prunes whole directories on the same
    // segment a metric name and a residence sort key carry — one vocabulary across all three planes.
    public static readonly LandingArm Receipt   = new("receipt", "store.receipt.land", "domain", ["at"]);
    // Billing generations partition by their accrual WINDOW, never by a receipt domain: folding them under the
    // receipt arm splits that arm's tree at `schema=` and a cold-tail sweep loses the one readable segment a whole
    // billing period prunes on. `Segment` is an `Identifier`, which refuses a digit lead, so the month token carries
    // its own leading letter.
    public static readonly LandingArm Cost      = new("cost", "store.cost.land", "month", ["kind"]);

    public StoreSlot Slot { get; }
    public Identifier Partition { get; }
    // `Sorted` fixes the order the arm's generations are WRITTEN in, declared on the dataset shape because the row IS
    // that shape: the Parquet writer stamps it as sorting-column metadata and the scan side's row-grain skip reads
    // it, so every generation of one arm claims one order across the whole tree.
    public Seq<Identifier> Sorted { get; }

    private LandingArm(string key, string slot, string partition, Seq<string> sorted) : this(key) =>
        (Slot, Partition, Sorted) = (StoreSlot.Create(slot), Identifier.Create(partition), sorted.Map(Identifier.Create));
}

// ONE landed generation coordinate, and the only owner of a cold-tail directory spelling. Four segments carry four
// distinct facts and each earns its place:
//   `tenant=`     — the WHOLE `ResidenceTenancy.Prefix` mechanism. `Residence.Lake` renders its tenancy predicate
//                   against a `tenant` column no Lake dataset declares and `hive_partitioning` projects that column
//                   back from this segment alone, so a tree missing it answers every tenant-scoped scan with zero
//                   rows and raises nothing on any engine.
//   `<arm key>=`  — the arm's own partition noun at a READABLE value, so a domain, model, study, or catalogue scan
//                   prunes whole directories on the segment a metric name and a residence sort key carry. Keying it
//                   by a content hash prunes exactly as well and names nothing a board can spell.
//   `schema=`     — the producer's schema-identity content key, which is what makes an additive column a compatible
//                   generation rather than a split tree.
//   `generation=` — the generation content key the artifact index registers, so a retry re-lands clean.
// DuckDB resolves a hive key colliding with a body column IN FAVOUR OF THE DIRECTORY — shadowing the file's own value
// with no error and no duplicate column — so `Segment` derives from whichever projection wrote that body column and
// never spells a second time at a call site; a divergent pair silently rewrites evidence on read.
public readonly record struct LakeGeneration(
    LandingArm Arm, TenantContext Tenant, Identifier Segment, UInt128 SchemaKey, UInt128 GenerationKey) {
    public StorePath Path(StorePath root) =>
        StorePath.Create(string.Create(CultureInfo.InvariantCulture,
            $"{(string)root}/{(string)Residence.TenantColumn}={Tenant.Entry}/{(string)Arm.Partition}={(string)Segment}"
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
|  [12]   | landing spine        | `LandingArm` row per producer dataset shape  | schema handoff only; storage, slots, custody stay here          |
|  [13]   | generation directory | `LakeGeneration` tenant/segment/key spelling | hive key wins a body-column collision; segment derives once     |

## [03]-[RESEARCH]

(none)
