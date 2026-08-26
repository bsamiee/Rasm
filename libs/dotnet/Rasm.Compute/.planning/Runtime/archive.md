# [COMPUTE_ARCHIVE]

Rasm.Compute owns ONE HDF5 container-session capsule for the whole branch: every open, declared-selection read, deferred chunked write, and filter registration in the Compute closure crosses it. The capsule is job machinery and never a store — artifacts it emits land content-addressed through `ArtifactIndexRow.Admit` on the Persistence blob lane, and the chunk-grid derivation every producer and consumer keys on seats here beside the writer that walks it.

`HdfArchive`/`HdfHandle`/`HdfWriter` own the session, `HdfArchivePolicy` with its `DeflateGrade`/`FilterStage` row vocabulary owns the composition filter seat, and `ChunkGrid` owns the `(extent, components) → grid → ordinal → hyperslab` correspondence in both directions. `Runtime/field`, `Solver/contract`, `Solver/route`, `Solver/field`, `Solver/uncertainty`, `Solver/clash`, `Model/identity`, `Model/sessions`, `Symbolic/lowering`, `Tensor/factor`, `Tensor/sampling`, `Tensor/quadrature`, `Tensor/dispatch`, `Tensor/blas`, `Analysis/daylight`, `Analysis/capacity`, and `Runtime/scheduling` compose this page and none opens an `H5File` of its own.

## [01]-[INDEX]

- [02]-[HDF_ARCHIVE]: the session capsule — source-cased opens, the bracketed session, the process-static filter seat, and the concrete `NativeDataset`/`NativeGroup` resolve on the result.
- [03]-[CHUNK_CURSOR]: the chunk-grid owner, the declared-slot session capsule for chunk-streamed artifacts, and the per-slot monotone cursor that makes an out-of-order chunk unrepresentable rather than thrown.

## [02]-[HDF_ARCHIVE]

- Owner: `HdfArchive` — the branch's ONE HDF5 container-session capsule, so the process-static filter registry seats once, the parallel-entry law rides a handle column a fan-out entry consumes, and every resolve leaves on the `Fin` result; `HdfSource` the closed payload/path/mapped source `[Union]` whose case IS the concurrency law; `HdfHandle` the job-scoped open (one `NativeFile` per job, disposed at the job boundary); `DeflateGrade` the four-value compress vocabulary; `FilterStage` the pipeline-rank row set carrying each stage's own `H5Filter` mint; `HdfArchivePolicy` the `[ComplexValueObject]` filter-and-cache policy whose factory refuses a stage set the compress path cannot serve.
- Cases: `HdfSource.Payload` (bytes in hand — zero-copy stream view, single-reader by the shared `Stream.Position`), `HdfSource.Path` (file-handle driver, `ThreadLocal` position, parallel-fan-out safe), `HdfSource.Mapped` (memory-mapped accessor, parallel-fan-out safe); `DeflateGrade.Default|Store|Fast|Dense` — the ONLY levels the compress path serves (`-1`, `0`, `1`, `9`); every other integer passes dataset construction and faults at the first chunk compress, which is why the grade is a row and never an `int` knob; `FilterStage.Shuffle|Deflate|Fletcher32` — filter ids `2`/`1`/`3` at pipeline ranks `0`/`1`/`2`, the rank column carrying the ordering law a positional collection expression used to leave in a comment.
- Law: reads DECLARE their selection — the read entry takes a `Selection` and a caller-owned destination span, so an unqualified whole-dataset read of a screening-scale corpus is unspellable; the per-read chunk cache sizes from policy (`SimpleReadingChunkCache`, default 521 slots / 1 MiB) because a working set past the cache re-decompresses every miss, so slab-scale readers pass a slab-sized policy; write staging is unbounded by the library (`SimpleWritingChunkCache` holds every touched chunk until flush), so chunk-aligned index-order writing IS the memory bound, enforced by the writer cursor, never by a smaller cache.
- Entry: `HdfArchive.Mount()` once at composition (the `Tiles3DExtensions.RegisterExtensions` precedent) registering `LzfFilter` and `BZip2SharpZipLibFilter` — the COMPLETE managed filter extension on the branch RID, the accelerated Blosc2/Bitshuffle/ISA-L natives publishing no osx payload; `HdfArchive.Session<A>(HdfSource, HdfArchivePolicy, Func<HdfHandle, IO<Fin<A>>>)` the BRACKETED read scope every bounded consumer takes, acquiring, using, and releasing on every outcome arm; `HdfArchive.Open(HdfSource, HdfArchivePolicy)` the job-scoped mint for the consumer whose handle outlives one expression and whose job boundary owns the dispose; `HdfArchive.Fan(HdfSource, HdfArchivePolicy, int workers)` the parallel-fan-out entry that GATES on `HdfSource.Parallel` and refuses a `Payload` source typed; `HdfHandle.Dataset(string)` resolving the CONCRETE `NativeDataset` on `Fin` (the `Span<T>` and `H5DatasetAccess` read overloads live there alone, never the `IH5Dataset` face) and `HdfHandle.Group(string)` resolving the `NativeGroup` whose attribute roster metadata reads walk; `HdfArchive.Begin(H5File, Stream, HdfArchivePolicy)` opening the deferred-write session over the composition's pooled sink.
- Output: consumers hold `HdfHandle`/`HdfWriter`, never a `NativeFile` — driver, chunk cache, and global-heap map all hang off the file object, so a long-lived open across jobs is the rejected form.
- Packages: PureHDF (`H5File.OpenRead`/`Open(Stream, bool, H5ReadOptions?)`/`Open(MemoryMappedViewAccessor, H5ReadOptions?)`, `NativeFile`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, Selection?, Selection?, ulong[]?)`, `H5DatasetAccess`, `SimpleReadingChunkCache`, `IH5Group.LinkExists`, `H5File.BeginWrite(Stream, H5WriteOptions?)`, `H5NativeWriter.Write<T>(H5Dataset<T>, T, Selection?, Selection?)`, `H5DatasetCreation`, `H5Filter.Register`, `DeflateFilter`/`ShuffleFilter`/`Fletcher32Filter`, `HyperslabSelection`), PureHDF.Filters.Lzf (`LzfFilter`), PureHDF.Filters.BZip2.SharpZipLib (`BZip2SharpZipLibFilter`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`System.IO.MemoryMappedFiles`, `System.Runtime.InteropServices.MemoryMarshal`, `System.Collections.Frozen`)
- Growth: a new archive artifact class (solver time-history, modal basis, ensemble store, sparse-operator exchange, checkpoint, response corpus) is a CONSUMER declaring its own slots and attributes to the `[03]-[CHUNK_CURSOR]` `ArchiveSession` capsule, or composing `Session`/`Open` on the read side — zero rows here; a new filter is one `H5Filter.Register` row inside `Mount` beside one `FilterStage` row carrying its id, rank, and mint; a new source modality is one `HdfSource` case whose `Parallel` column states its law and whose value the `Fan` entry reads.
- Boundary: `Op.Catch` admits every PureHDF throw as its original `Error`; typed archive refusals arise only from explicit shape, policy, and capacity decisions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record HdfSource {
    private HdfSource() { }

    public sealed record Payload(ReadOnlyMemory<byte> Bytes) : HdfSource;
    public sealed record Path(string File) : HdfSource;
    public sealed record Mapped(MemoryMappedViewAccessor View) : HdfSource;

    public bool Parallel => Switch(
        payload: static _ => false,
        path: static _ => true,
        mapped: static _ => true);
}

[SmartEnum<int>]
public sealed partial class DeflateGrade {
    public static readonly DeflateGrade Default = new(-1);
    public static readonly DeflateGrade Store = new(0);
    public static readonly DeflateGrade Fast = new(1);
    public static readonly DeflateGrade Dense = new(9);
}

[SmartEnum<int>]
public sealed partial class FilterStage {
    public static readonly FilterStage Deflate    = new(1, rank: 1, mint: static grade => new H5Filter(DeflateFilter.Id, new() { [DeflateFilter.COMPRESSION_LEVEL] = grade.Key }));
    public static readonly FilterStage Shuffle    = new(2, rank: 0, mint: static _ => new H5Filter(ShuffleFilter.Id));
    public static readonly FilterStage Fletcher32 = new(3, rank: 2, mint: static _ => new H5Filter(Fletcher32Filter.Id));

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial H5Filter Mint(DeflateGrade grade);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class HdfArchivePolicy {
    public static readonly HdfArchivePolicy Interchange = Create(
        DeflateGrade.Fast, [FilterStage.Shuffle, FilterStage.Deflate], readCacheSlots: 521, readCacheBytes: 1UL << 20);

    public DeflateGrade Deflate { get; }
    public FrozenSet<FilterStage> Stages { get; }
    public int ReadCacheSlots { get; }
    public ulong ReadCacheBytes { get; }

    public H5DatasetCreation Creation() =>
        new(Filters: [.. Stages.OrderBy(static stage => stage.Rank).Select(stage => stage.Mint(Deflate))]);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref DeflateGrade deflate, ref FrozenSet<FilterStage> stages,
        ref int readCacheSlots, ref ulong readCacheBytes) =>
        validationError = Seq(
                stages.Contains(FilterStage.Deflate) ? None : Some("<hdf5-policy-stages:deflate-absent>"),
                readCacheSlots > 0 ? None : Some($"<hdf5-policy-cache-slots:{readCacheSlots}>"),
                readCacheBytes > 0UL ? None : Some("<hdf5-policy-cache-bytes:0>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));

    public static HdfArchivePolicy Create(DeflateGrade deflate, Seq<FilterStage> stages, int readCacheSlots, ulong readCacheBytes) =>
        Create(deflate, stages.ToFrozenSet(), readCacheSlots, readCacheBytes);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class HdfHandle : IDisposable {
    internal HdfHandle(NativeFile file, bool parallel, H5DatasetAccess access) { File = file; Parallel = parallel; Access = access; }

    internal NativeFile File { get; }
    public bool Parallel { get; }
    public H5DatasetAccess Access { get; }

    public bool Exists(string path) => File.LinkExists(path);

    public Fin<NativeDataset> Dataset(string path) =>
        File.LinkExists(path)
            ? File.Dataset(path) as NativeDataset is { } dataset
                ? Fin.Succ(dataset)
                : Fin.Fail<NativeDataset>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Unsupported(ComputeCapability.Dataset)))
            : Fin.Fail<NativeDataset>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)));

    public Fin<NativeGroup> Group(string path) =>
        File.LinkExists(path)
            ? File.Group(path) as NativeGroup is { } group
                ? Fin.Succ(group)
                : Fin.Fail<NativeGroup>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Unsupported(ComputeCapability.Group)))
            : Fin.Fail<NativeGroup>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)));

    public void Dispose() => File.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HdfArchive {
    static readonly object MountGate = new();
    static bool _mounted;

    public static void Mount() {
        lock (MountGate) {
            if (_mounted) { return; }
            H5Filter.Register(new LzfFilter());
            H5Filter.Register(new BZip2SharpZipLibFilter());
            _mounted = true;
        }
    }

    public static IO<Fin<A>> Session<A>(HdfSource source, HdfArchivePolicy policy, Func<HdfHandle, IO<Fin<A>>> read) =>
        IO.lift<Fin<HdfHandle>>(() => Open(source, policy)).Bind(opened => opened.Match(
            Succ: handle => IO.lift(() => handle).Bracket(read, static handle => IO.lift(() => { handle.Dispose(); return unit; })),
            Fail: error => IO.pure(Fin<A>.Fail(error))));

    public static Fin<HdfHandle> Open(HdfSource source, HdfArchivePolicy policy) =>
        Op.Of(name: "hdf5.open").Catch(() => {
            H5DatasetAccess access = new() { ChunkCache = new SimpleReadingChunkCache(policy.ReadCacheSlots, policy.ReadCacheBytes) };
            return Fin.Succ(source.Switch(
                state: access,
                payload: static (a, payload) => new HdfHandle(H5File.Open(View(payload.Bytes), leaveOpen: false), parallel: false, a),
                path: static (a, path) => new HdfHandle(H5File.OpenRead(path.File), parallel: true, a),
                mapped: static (a, mapped) => new HdfHandle(H5File.Open(mapped.View), parallel: true, a)));
        });

    public static Fin<Seq<HdfHandle>> Fan(HdfSource source, HdfArchivePolicy policy, int workers) =>
        !source.Parallel
            ? Fin.Fail<Seq<HdfHandle>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Type(source.GetType()))))
            : workers < 1
            ? Fin.Fail<Seq<HdfHandle>>(new ComputeFault.PayloadOverBounds($"<hdf5-fan-workers:{workers}>"))
            : OpenFan(source, policy, workers);

    static Fin<Seq<HdfHandle>> OpenFan(HdfSource source, HdfArchivePolicy policy, int workers) {
        List<HdfHandle> handles = new(workers);
        Error? failure = null;
        for (int worker = 0; worker < workers; worker++) {
            bool opened = Open(source, policy).Match(
                Succ: handle => { handles.Add(handle); return true; },
                Fail: error => { failure = error; return false; });
            if (!opened) {
                handles.ForEach(static handle => handle.Dispose());
                return Fin.Fail<Seq<HdfHandle>>(failure!);
            }
        }
        return Fin.Succ(toSeq(handles));
    }

    static MemoryStream View(ReadOnlyMemory<byte> bytes) =>
        MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new MemoryStream(bytes.ToArray(), writable: false);

    public static HdfWriter Begin(H5File graph, Stream sink, HdfArchivePolicy policy) =>
        new(graph.BeginWrite(sink, new H5WriteOptions()));
}
```

## [03]-[CHUNK_CURSOR]

- Owner: `ChunkGrid` — the ONE station-outermost container-grid correspondence, forward and inverse under one owner: the `(extent, components, budget) → (FileDims, Chunks)` derivation, the grid-ordinal decomposition, and the chunk-aligned `HyperslabSelection` that ordinal names; `ArchiveSession` the declared-slot capsule for an artifact whose datasets stream chunkwise, folding the slot mint, graph build, root-or-group attribute stamp, writer open, completion gate, and release into one act; `ArchiveSlot<T>` the typed dataset declaration, `ArchiveAttributes` the object coordinate for a typed attribute set, and `ArchiveAttribute` the closed value vocabulary for that capsule; `HdfWriter` the `BeginWrite` session wrapper also held directly by a mixed inline/deferred graph or job-lived history; `ChunkCursor<T>` the typed view over the session-owned monotone state for one slot.
- Entry: `ArchiveSession.Open(Stream, HdfArchivePolicy, Seq<IArchiveSlot>, Seq<(string, ArchiveAttribute)>, Seq<ArchiveAttributes> = default)` mints the declared-write capsule and `ArchiveSession.Write` is its bracketed form, requiring every declared slot complete before a successful result and releasing on every outcome arm; the first attribute sequence stamps the file root and each `ArchiveAttributes` stamps the exact group path it carries; `session.Cursor(ArchiveSlot<T>)` is the one write door and every handle for one slot shares the session-owned ordinal; `session.Seal()` proves all declared slots complete; `ChunkGrid.Derive(ReadOnlySpan<int> extent, int components, int targetChunkElements)` is the derivation every producer and archive consumer composes — station axis chunks at 1, the component axis rides whole as the trailing extent, interior axes halve largest-first until the slab meets the element budget; `ChunkGrid.Seat(ReadOnlySpan<ulong> fileDims, ReadOnlySpan<uint> chunks)` seats a container's OWN declared chunk grid without re-deriving it; `grid.Selection(int ordinal)` is the edge-clipped hyperslab that ordinal addresses, `grid.Pack(source, ordinal)` gathers that hyperslab from a row-major whole payload, and `grid.LogicalSlice(int ordinal, int elementBytes, int payloadBytes)` projects the same ordinal into an already chunk-packed payload, never an HDF5 file offset; `ChunkCursor<T>.Write(T[] chunk)` advances one exact chunk, and `WriteAll(T[])` gathers and drains a whole row-major payload.
- Auto: the cursor OWNS the ordinal, so an out-of-order or repeated chunk is unrepresentable rather than refused — the only ordinal the cursor accepts is the one it already holds, so the parameter reconstructed nothing and every call site drops a hand-maintained counter with it. The cursor gates each chunk's exact edge-clipped element count before provider IO; `WriteAll` gathers a row-major whole payload through the grid rather than slicing it as if every multidimensional hyperslab were contiguous.
- Law: a chunked write's evidence rides the composing consumer's own `Streamed` value (bytes and segments), never a second row here.
- Packages: PureHDF (`H5Dataset<T>`, `H5NativeWriter.Write<T>`, `HyperslabSelection(int, ulong[], ulong[])` and the four-argument stride/count form), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new archive artifact class DECLARES its slots and attributes and takes its cursors off the one session — zero rows here; a new attribute shape is one `ArchiveAttribute` case; a new grid-shaping law is one column on `ChunkGrid` the derivation reads.
- Boundary: `Chunks can only be written once.` throws from the library's v4 index MID-ENCODE, after the producing work is already spent — the cursor refuses at admission instead, and with the ordinal no longer a parameter the refusal is a state no caller can construct; `ChunkGrid` is the SINGLE owner of the ordinal correspondence, so derivation, edge-clipped write selection, and packed-payload projection cannot fork; `LogicalSlice` never claims an encoded-file byte range because HDF5 filter output and chunk-index placement are provider-owned; the grid is a value with no container attached, so a producer derives one before any file exists and an ingest seats the container's own without a second concept entering.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ChunkGrid {
    public ReadOnlyMemory<int> Grid { get; }
    public ReadOnlyMemory<uint> Chunk { get; }
    public ReadOnlyMemory<ulong> FileDims { get; }

    public int Rank => Chunk.Length;
    public int Count => Grid.Span.ToArray().Aggregate(1, static (acc, axis) => acc * axis);
    public int ChunkElements => Chunk.Span.ToArray().Aggregate(1, static (acc, axis) => acc * (int)axis);

    public HyperslabSelection Selection(int ordinal) => Selection(ordinal, ReadOnlySpan<ulong>.Empty);

    public HyperslabSelection Selection(int ordinal, ReadOnlySpan<ulong> origin) {
        if (!origin.IsEmpty && origin.Length != Rank) { throw new ArgumentException("selection origin rank", nameof(origin)); }
        (ulong[] local, ulong[] blocks) = Bounds(ordinal);
        ulong[] starts = [.. local];
        for (int axis = 0; axis < Rank; axis++) { starts[axis] += origin.IsEmpty ? 0UL : origin[axis]; }
        return new HyperslabSelection(Rank, starts, blocks);
    }

    (ulong[] Starts, ulong[] Blocks) Bounds(int ordinal) {
        if ((uint)ordinal >= (uint)Count) { throw new ArgumentOutOfRangeException(nameof(ordinal)); }
        ulong[] starts = new ulong[Rank];
        ReadOnlySpan<int> grid = Grid.Span;
        ReadOnlySpan<uint> chunk = Chunk.Span;
        int remainder = ordinal;
        for (int axis = grid.Length - 1; axis >= 0; axis--) {
            starts[axis] = (ulong)(remainder % grid[axis]) * chunk[axis];
            remainder /= grid[axis];
        }
        ulong[] blocks = new ulong[Rank];
        ReadOnlySpan<ulong> dims = FileDims.Span;
        for (int axis = 0; axis < Rank; axis++) { blocks[axis] = Math.Min(chunk[axis], dims[axis] - starts[axis]); }
        return (starts, blocks);
    }

    public int SelectionElements(int ordinal) => checked((int)Selection(ordinal).TotalElementCount);

    public T[] Pack<T>(ReadOnlySpan<T> source, int ordinal) {
        ReadOnlySpan<ulong> dims = FileDims.Span;
        long total = dims.ToArray().Aggregate(1L, static (acc, axis) => acc * (long)axis);
        if (source.Length != total) { throw new ArgumentException("chunk source extent", nameof(source)); }
        (ulong[] starts, ulong[] blocks) = Bounds(ordinal);
        int count = checked((int)blocks.Aggregate(1UL, static (acc, axis) => acc * axis));
        T[] packed = new T[count];
        ulong[] strides = new ulong[Rank];
        ulong stride = 1UL;
        for (int axis = Rank - 1; axis >= 0; axis--) { strides[axis] = stride; stride *= dims[axis]; }
        for (int index = 0; index < count; index++) {
            int remainder = index;
            ulong sourceIndex = 0UL;
            for (int axis = Rank - 1; axis >= 0; axis--) {
                ulong coordinate = (ulong)(remainder % (int)blocks[axis]);
                remainder /= (int)blocks[axis];
                sourceIndex += (starts[axis] + coordinate) * strides[axis];
            }
            packed[index] = source[checked((int)sourceIndex)];
        }
        return packed;
    }

    public Range LogicalElements(int ordinal, int payloadElements) {
        if ((uint)ordinal >= (uint)Count || payloadElements < 1) { return new Range(0, 0); }
        long start = 0L;
        for (int prior = 0; prior < ordinal; prior++) { start += SelectionElements(prior); }
        return start >= payloadElements || start < 0L
            ? new Range(0, 0)
            : new Range((int)start, (int)Math.Min(start + SelectionElements(ordinal), payloadElements));
    }

    public Range LogicalSlice(int ordinal, int elementBytes, int payloadBytes) {
        if (elementBytes < 1 || payloadBytes < 1 || payloadBytes % elementBytes != 0) { return new Range(0, 0); }
        Range elements = LogicalElements(ordinal, payloadBytes / elementBytes);
        (int offset, int length) = elements.GetOffsetAndLength(payloadBytes / elementBytes);
        return new Range(checked(offset * elementBytes), checked((offset + length) * elementBytes));
    }

    public static Validation<Error, ChunkGrid> Derive(ReadOnlySpan<int> extent, int components, int targetChunkElements) {
        if (extent.Length < 1 || components < 1 || targetChunkElements < components) {
            return Fail<Error, ChunkGrid>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Counts(extent.Length, components, targetChunkElements))));
        }
        ulong[] dims = [.. extent.ToArray().Select(static axis => (ulong)axis), (ulong)components];
        uint[] chunks = [1U, .. extent[1..].ToArray().Select(static axis => (uint)axis), (uint)components];
        long slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        while (slab > targetChunkElements) {
            int widest = 1;
            for (int axis = 2; axis < chunks.Length - 1; axis++) { widest = chunks[axis] > chunks[widest] ? axis : widest; }
            if (chunks[widest] <= 1) { break; }
            chunks[widest] = (chunks[widest] + 1) / 2;
            slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        }
        return Seat(dims, chunks);
    }

    public static Validation<Error, ChunkGrid> Seat(ReadOnlySpan<ulong> fileDims, ReadOnlySpan<uint> chunks) {
        int[] grid = [.. fileDims.ToArray().Zip(chunks.ToArray(), static (whole, chunk) => chunk == 0U ? 0 : (int)(((long)whole + chunk - 1) / chunk))];
        return Create(grid, chunks.ToArray(), fileDims.ToArray());
    }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ReadOnlyMemory<int> grid, ref ReadOnlyMemory<uint> chunk, ref ReadOnlyMemory<ulong> fileDims) {
        int[] g = grid.ToArray();
        uint[] c = chunk.ToArray();
        ulong[] d = fileDims.ToArray();
        validationError = Seq(
                g.Length == c.Length && c.Length == d.Length ? None : Some($"<chunk-grid-rank:{g.Length}:{c.Length}:{d.Length}>"),
                g.Length > 0 ? None : Some("<chunk-grid-empty>"),
                g.All(static axis => axis > 0) ? None : Some($"<chunk-grid-extent:[{string.Join(',', g)}]>"),
                c.All(static axis => axis > 0U) ? None : Some($"<chunk-grid-chunk:[{string.Join(',', c)}]>"),
                c.Aggregate(1L, static (acc, axis) => acc * axis) is > 0L and <= int.MaxValue ? None : Some("<chunk-grid-slab-overflow>"),
                g.Aggregate(1L, static (acc, axis) => acc * axis) is > 0L and <= int.MaxValue ? None : Some("<chunk-grid-count-overflow>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));
    }
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record ArchiveAttribute {
    private ArchiveAttribute() { }

    public sealed record Text(string Value) : ArchiveAttribute;
    public sealed record Real(double Value) : ArchiveAttribute;
    public sealed record Whole(long Value) : ArchiveAttribute;
    public sealed record WholeVector(ReadOnlyMemory<long> Value) : ArchiveAttribute;
    public sealed record Flag(bool Value) : ArchiveAttribute;

    internal object Boxed => Switch<object>(
        text: static t => t.Value, real: static r => r.Value, whole: static w => w.Value,
        wholeVector: static w => w.Value.ToArray(), flag: static f => f.Value);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ArchiveSlot<T>(string Path, ChunkGrid Grid) : IArchiveSlot where T : unmanaged {
    object IArchiveSlot.Seat(HdfArchivePolicy policy) {
        return new H5Dataset<T[]>(Grid.FileDims.ToArray(), Grid.Chunk.ToArray(), datasetCreation: policy.Creation());
    }
}

public interface IArchiveSlot {
    string Path { get; }
    ChunkGrid Grid { get; }
    internal object Seat(HdfArchivePolicy policy);
}

public sealed record ArchiveAttributes(string Path, Seq<(string Key, ArchiveAttribute Value)> Values);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ArchiveSession : IDisposable {
    readonly HdfWriter _writer;
    readonly Dictionary<IArchiveSlot, object> _datasets;
    readonly Dictionary<IArchiveSlot, ChunkState> _states;
    bool _released;

    ArchiveSession(HdfWriter writer, Dictionary<IArchiveSlot, object> datasets, Dictionary<IArchiveSlot, ChunkState> states) =>
        (_writer, _datasets, _states) = (writer, datasets, states);

    public static Fin<ArchiveSession> Open(
        Stream sink, HdfArchivePolicy policy, Seq<IArchiveSlot> slots, Seq<(string Key, ArchiveAttribute Value)> attributes,
        Seq<ArchiveAttributes> grouped = default) =>
        slots.IsEmpty
            ? Fin.Fail<ArchiveSession>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
            : slots.Map(static slot => slot.Path).Distinct().Count != slots.Count
            ? Fin.Fail<ArchiveSession>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(slots.Map(static slot => slot.Path).Distinct().Count, slots.Count))))
            : Op.Of(name: "hdf5.session-open").Catch(() => {
                H5File graph = new();
                Dictionary<IArchiveSlot, object> datasets = new(ReferenceEqualityComparer.Instance);
                Dictionary<IArchiveSlot, ChunkState> states = new(ReferenceEqualityComparer.Instance);
                slots.Iter(slot => { Seat(graph, slot, policy, datasets); states.Add(slot, new ChunkState(slot.Grid)); });
                attributes.Iter(pair => graph.Attributes[pair.Key] = pair.Value.Boxed);
                grouped.Iter(set => set.Values.Iter(pair => Group(graph, set.Path).Attributes[pair.Key] = pair.Value.Boxed));
                return Fin.Succ(new ArchiveSession(new HdfWriter(graph.BeginWrite(sink, new H5WriteOptions())), datasets, states));
            });

    static void Seat(H5File graph, IArchiveSlot slot, HdfArchivePolicy policy, Dictionary<IArchiveSlot, object> datasets) {
        string[] cells = slot.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (cells.Length == 0) { throw new InvalidOperationException("archive slot path is empty"); }
        H5Group parent = Group(graph, string.Join('/', cells.Take(cells.Length - 1)));
        object dataset = slot.Seat(policy);
        parent[cells[^1]] = dataset;
        datasets.Add(slot, dataset);
    }

    static H5Group Group(H5File graph, string path) {
        H5Group parent = graph;
        foreach (string cell in path.Split('/', StringSplitOptions.RemoveEmptyEntries)) {
            if (parent.TryGetValue(cell, out object? found)) {
                parent = found as H5Group ?? throw new InvalidOperationException($"archive path is not a group: {path}");
            } else {
                H5Group child = new();
                parent[cell] = child;
                parent = child;
            }
        }
        return parent;
    }

    public Fin<ChunkCursor<T>> Cursor<T>(ArchiveSlot<T> slot) where T : unmanaged =>
        !_released && _datasets.TryGetValue(slot, out object? seated) && seated is H5Dataset<T[]> dataset
            && _states.TryGetValue(slot, out ChunkState? state)
            ? Fin.Succ(_writer.Open(dataset, state))
            : Fin.Fail<ChunkCursor<T>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Initialized, new ContractEvidence.Key(slot.Path))));

    public Fin<Unit> Seal() {
        int incomplete = _states.Values.Count(static state => !state.Complete);
        return !_released && incomplete == 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime,
                new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(_states.Count - incomplete, _states.Count))));
    }

    public static IO<Fin<A>> Write<A>(
        Stream sink, HdfArchivePolicy policy, Seq<IArchiveSlot> slots,
        Seq<(string Key, ArchiveAttribute Value)> attributes, Func<ArchiveSession, IO<Fin<A>>> emit,
        Seq<ArchiveAttributes> grouped = default) =>
        IO.pure(Open(sink, policy, slots, attributes, grouped)).Bind(opened => opened.Match(
            Succ: session => IO.lift(() => session).Bracket(
                s => emit(s).Map(result => result.Bind(value => s.Seal().Map(_ => value))),
                static s => IO.lift(() => { s.Release(); return unit; })),
            Fail: error => IO.pure(Fin<A>.Fail(error))));

    internal void Release() {
        if (_released) { return; }
        _released = true;
        _writer.Dispose();
    }

    public void Dispose() {
        int incomplete = _states.Values.Count(static state => !state.Complete);
        Release();
        if (incomplete != 0) { throw new InvalidOperationException($"archive session incomplete: {_states.Count - incomplete}/{_states.Count} slots"); }
    }
}

internal sealed class ChunkState(ChunkGrid grid) {
    public ChunkGrid Grid { get; } = grid;
    public int Next { get; private set; }
    public bool Complete => Next == Grid.Count;
    public void Advance() => Next++;
}

public sealed class HdfWriter : IDisposable {
    readonly H5NativeWriter _writer;

    internal HdfWriter(H5NativeWriter writer) => _writer = writer;

    public ChunkCursor<T> Open<T>(H5Dataset<T[]> slot, ChunkGrid grid) where T : unmanaged => new(this, slot, new ChunkState(grid));

    internal ChunkCursor<T> Open<T>(H5Dataset<T[]> slot, ChunkState state) where T : unmanaged => new(this, slot, state);

    internal Fin<Unit> Write<T>(H5Dataset<T[]> slot, T[] chunk, HyperslabSelection file) where T : unmanaged =>
        Op.Of(name: "hdf5.chunk-write").Catch(() => { _writer.Write(slot, chunk, fileSelection: file); return Fin.Succ(unit); });

    public void Dispose() => _writer.Dispose();
}

public sealed class ChunkCursor<T> where T : unmanaged {
    readonly HdfWriter _writer;
    readonly H5Dataset<T[]> _slot;
    readonly ChunkState _state;

    internal ChunkCursor(HdfWriter writer, H5Dataset<T[]> slot, ChunkState state) => (_writer, _slot, _state) = (writer, slot, state);

    public int Next => _state.Next;
    public bool Complete => _state.Complete;

    public Fin<Unit> Write(T[] chunk) =>
        _state.Next >= _state.Grid.Count
            ? Fin.Fail<Unit>(new ComputeFault.PayloadOverBounds($"<hdf5-chunk-count:{_state.Next}:{_state.Grid.Count}>"))
            : chunk.Length != _state.Grid.SelectionElements(_state.Next)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(chunk.Length, _state.Grid.SelectionElements(_state.Next)))))
            : _writer.Write(_slot, chunk, _state.Grid.Selection(_state.Next)).Map(_ => { _state.Advance(); return unit; });

    public Fin<Unit> WriteAll(T[] source) =>
        source.LongLength != _state.Grid.FileDims.Span.ToArray().Aggregate(1L, static (acc, axis) => acc * (long)axis)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(source.LongLength, _state.Grid.FileDims.Span.ToArray().Aggregate(1L, static (acc, axis) => acc * (long)axis)))))
            : Range(_state.Next, _state.Grid.Count - _state.Next).Fold(Fin.Succ(unit), (result, ordinal) =>
                result.Bind(_ => Write(_state.Grid.Pack(source, ordinal))));
}
```
