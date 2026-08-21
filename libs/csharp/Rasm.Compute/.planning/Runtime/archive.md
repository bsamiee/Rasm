# [COMPUTE_ARCHIVE]

Rasm.Compute owns ONE HDF5 container-session capsule for the whole branch: every open, declared-selection read, deferred chunked write, and filter registration in the Compute closure crosses it. The capsule is job machinery and never a store — artifacts it emits land content-addressed through `ArtifactIndexRow.Admit` on the Persistence blob lane, and the chunk-grid derivation every producer and consumer keys on seats here beside the writer that walks it.

`HdfArchive`/`HdfHandle`/`HdfWriter` own the session, `HdfArchivePolicy` with its `DeflateGrade`/`FilterStage` row vocabulary owns the composition filter seat, and `ChunkGrid` owns the `(extent, components) → grid → ordinal → hyperslab` correspondence in both directions. `Runtime/field`, `Solver/contract`, `Solver/route`, `Solver/field`, `Solver/uncertainty`, `Solver/clash`, `Model/identity`, `Model/sessions`, `Symbolic/lowering`, `Tensor/factor`, `Tensor/sampling`, `Tensor/quadrature`, `Tensor/dispatch`, `Tensor/blas`, `Analysis/daylight`, `Analysis/capacity`, and `Runtime/scheduling` compose this page and none opens an `H5File` of its own.

## [01]-[INDEX]

- [02]-[HDF_ARCHIVE]: the session capsule — source-cased opens, the bracketed session, the process-static filter seat, and the concrete `NativeDataset`/`NativeGroup` resolve on the rail.
- [03]-[CHUNK_CURSOR]: the chunk-grid owner, the ONE declared-write session capsule every archive artifact class composes, and the per-slot monotone cursor that makes an out-of-order chunk unrepresentable rather than thrown.

## [02]-[HDF_ARCHIVE]

- Owner: `HdfArchive` — the branch's ONE HDF5 container-session capsule, so the process-static filter registry seats once, the parallel-entry law rides a handle column a fan-out entry consumes, and every resolve leaves on the `Fin` rail; `HdfSource` the closed payload/path/mapped source `[Union]` whose case IS the concurrency law; `HdfHandle` the job-scoped open (one `NativeFile` per job, disposed at the job boundary); `DeflateGrade` the four-value compress vocabulary; `FilterStage` the pipeline-rank row set carrying each stage's own `H5Filter` mint; `HdfArchivePolicy` the `[ComplexValueObject]` filter-and-cache policy whose factory refuses a stage set the compress path cannot serve.
- Cases: `HdfSource.Payload` (bytes in hand — zero-copy stream view, single-reader by the shared `Stream.Position`), `HdfSource.Path` (file-handle driver, `ThreadLocal` position, parallel-fan-out safe), `HdfSource.Mapped` (memory-mapped accessor, parallel-fan-out safe); `DeflateGrade.Default|Store|Fast|Dense` — the ONLY levels the compress path serves (`-1`, `0`, `1`, `9`); every other integer passes dataset construction and faults at the first chunk compress, which is why the grade is a row and never an `int` knob; `FilterStage.Shuffle|Deflate|Fletcher32` — filter ids `2`/`1`/`3` at pipeline ranks `0`/`1`/`2`, the rank column carrying the ordering law a positional collection expression used to leave in a comment.
- Law: reads DECLARE their selection — the read entry takes a `Selection` and a caller-owned destination span, so an unqualified whole-dataset read of a screening-scale corpus is unspellable; the per-read chunk cache sizes from policy (`SimpleReadingChunkCache`, default 521 slots / 1 MiB) because a working set past the cache re-decompresses every miss, so slab-scale readers pass a slab-sized policy; write staging is unbounded by the library (`SimpleWritingChunkCache` holds every touched chunk until flush), so chunk-aligned index-order writing IS the memory bound, enforced by the writer cursor, never by a smaller cache.
- Entry: `HdfArchive.Mount()` once at composition (the `Tiles3DExtensions.RegisterExtensions` precedent) registering `LzfFilter` and `BZip2SharpZipLibFilter` — the COMPLETE managed filter extension on the branch RID, the accelerated Blosc2/Bitshuffle/ISA-L natives publishing no osx payload; `HdfArchive.Session<A>(HdfSource, HdfArchivePolicy, Func<HdfHandle, IO<Fin<A>>>)` the BRACKETED read scope every bounded consumer takes, acquiring, using, and releasing on every outcome arm; `HdfArchive.Open(HdfSource, HdfArchivePolicy)` the job-scoped mint for the consumer whose handle outlives one expression and whose job boundary owns the dispose; `HdfArchive.Fan(HdfSource, HdfArchivePolicy, int workers)` the parallel-fan-out entry that GATES on `HdfSource.Parallel` and refuses a `Payload` source typed; `HdfHandle.Dataset(string)` resolving the CONCRETE `NativeDataset` on `Fin` (the `Span<T>` and `H5DatasetAccess` read overloads live there alone, never the `IH5Dataset` face) and `HdfHandle.Group(string)` resolving the `NativeGroup` whose attribute roster metadata reads walk; `HdfArchive.Begin(H5File, Stream, HdfArchivePolicy)` opening the deferred-write session over the composition's pooled sink.
- Output: consumers hold `HdfHandle`/`HdfWriter`, never a `NativeFile` — driver, chunk cache, and global-heap map all hang off the file object, so a long-lived open across jobs is the rejected form.
- Packages: PureHDF (`H5File.OpenRead`/`Open(Stream, bool, H5ReadOptions?)`/`Open(MemoryMappedViewAccessor, H5ReadOptions?)`, `NativeFile`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, Selection?, Selection?, ulong[]?)`, `H5DatasetAccess`, `SimpleReadingChunkCache`, `IH5Group.LinkExists`, `H5File.BeginWrite(Stream, H5WriteOptions?)`, `H5NativeWriter.Write<T>(H5Dataset<T>, T, Selection?, Selection?)`, `H5DatasetCreation`, `H5Filter.Register`, `DeflateFilter`/`ShuffleFilter`/`Fletcher32Filter`, `HyperslabSelection`), PureHDF.Filters.Lzf (`LzfFilter`), PureHDF.Filters.BZip2.SharpZipLib (`BZip2SharpZipLibFilter`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`System.IO.MemoryMappedFiles`, `System.Runtime.InteropServices.MemoryMarshal`, `System.Collections.Frozen`)
- Growth: a new archive artifact class (solver time-history, modal basis, ensemble store, sparse-operator exchange, checkpoint, response corpus) is a CONSUMER declaring its own slots and attributes to the `[03]-[CHUNK_CURSOR]` `ArchiveSession` capsule, or composing `Session`/`Open` on the read side — zero rows here; a new filter is one `H5Filter.Register` row inside `Mount` beside one `FilterStage` row carrying its id, rank, and mint; a new source modality is one `HdfSource` case whose `Parallel` column states its law and whose value the `Fan` entry reads.
- Boundary: `Op.Catch` admits every PureHDF throw as its original `Error`; typed archive refusals arise only from explicit shape, policy, and capacity decisions.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The source case IS the concurrency law, and `Fan` is the entry that READS it: a fan-out gates on `Parallel`
// instead of a caller remembering which driver keeps its read position in a ThreadLocal.
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

// The compress path maps ONLY these four levels onto CompressionLevel and throws on the rest AFTER dataset
// construction — the grade row turns that mid-encode fault into an unspellable state.
[SmartEnum<int>]
public sealed partial class DeflateGrade {
    public static readonly DeflateGrade Default = new(-1);
    public static readonly DeflateGrade Store = new(0);
    public static readonly DeflateGrade Fast = new(1);
    public static readonly DeflateGrade Dense = new(9);
}

// Pipeline ORDER is a row column, never a positional collection expression whose law lives in a comment: Shuffle
// id 2 ahead of Deflate id 1 is the h5py `compression='gzip', shuffle=True` pipeline both directions read, and
// Fletcher32 id 3 tails when the corpus wants end-to-end detection (`SkipEdc` the read-side bypass). The key IS
// the registered HDF5 filter id, so a stage and its wire identifier cannot fork, and the mint column carries the
// one `H5Filter` construction each id takes — the Deflate arm the only one reading the grade.
[SmartEnum<int>]
public sealed partial class FilterStage {
    public static readonly FilterStage Deflate    = new(1, rank: 1, mint: static grade => new H5Filter(DeflateFilter.Id, new() { [DeflateFilter.COMPRESSION_LEVEL] = grade.Key }));
    public static readonly FilterStage Shuffle    = new(2, rank: 0, mint: static _ => new H5Filter(ShuffleFilter.Id));
    public static readonly FilterStage Fletcher32 = new(3, rank: 2, mint: static _ => new H5Filter(Fletcher32Filter.Id));

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial H5Filter Mint(DeflateGrade grade);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Admission-bearing policy: the factory ACCUMULATES, so a policy naming an empty stage set AND a zero cache
// reports both. Deflate is required because the read side decodes what the branch writes and a stage set without
// it publishes a compression posture the DeflateGrade column then states about nothing.
[ComplexValueObject]
public sealed partial class HdfArchivePolicy {
    public static readonly HdfArchivePolicy Interchange = Create(
        DeflateGrade.Fast, [FilterStage.Shuffle, FilterStage.Deflate], readCacheSlots: 521, readCacheBytes: 1UL << 20);

    public DeflateGrade Deflate { get; }
    public FrozenSet<FilterStage> Stages { get; }
    public int ReadCacheSlots { get; }
    public ulong ReadCacheBytes { get; }

    // Rank-ordered fold: the pipeline the container records is the stage set sorted by its own declared rank, so a
    // new stage lands at its rank and no caller re-spells the order.
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

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class HdfHandle : IDisposable {
    internal HdfHandle(NativeFile file, bool parallel, H5DatasetAccess access) { File = file; Parallel = parallel; Access = access; }

    internal NativeFile File { get; }
    public bool Parallel { get; }
    public H5DatasetAccess Access { get; }

    // Concrete resolve on the RAIL: the Span/H5DatasetAccess overloads and the real chunk grid live on
    // NativeDataset alone, and `LinkExists` is what separates "no such link" from "a link of another kind" in the
    // slug — which is the whole reason the guard-then-resolve two-call protocol needed a second public member.
    public Fin<NativeDataset> Dataset(string path) =>
        File.LinkExists(path)
            ? File.Dataset(path) as NativeDataset is { } dataset
                ? Fin.Succ(dataset)
                : Fin.Fail<NativeDataset>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Unsupported(ComputeCapability.Dataset)))
            : Fin.Fail<NativeDataset>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)));

    // Group resolve stays on the handle for the same reason: attribute rosters read off the resolved object, so
    // no consumer touches the NativeFile to reach its metadata.
    public Fin<NativeGroup> Group(string path) =>
        File.LinkExists(path)
            ? File.Group(path) as NativeGroup is { } group
                ? Fin.Succ(group)
                : Fin.Fail<NativeGroup>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Unsupported(ComputeCapability.Group)))
            : Fin.Fail<NativeGroup>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)));

    public void Dispose() => File.Dispose();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class HdfArchive {
    static int _mounted;

    // Token-gated one-shot: the registry is a process-static ConcurrentDictionary, so a per-read register writes
    // shared state on the hot path and a second Mount is a no-op, never a re-seed.
    public static void Mount() {
        if (Interlocked.Exchange(ref _mounted, 1) == 1) { return; }
        H5Filter.Register(new LzfFilter());
        H5Filter.Register(new BZip2SharpZipLibFilter());
    }

    // The BRACKETED scope every bounded read takes: acquisition, use, and release ride one `IO.Bracket`, so the
    // handle closes on the fault arm exactly as on the success arm. `Open` survives beside it for the consumer
    // whose handle outlives one expression — the discriminant is WHO OWNS THE BOUNDARY, the scope here and the
    // job there, and a `using` inside a rail lambda is the release-bound-to-the-success-arm form both delete.
    public static IO<Fin<A>> Session<A>(HdfSource source, HdfArchivePolicy policy, Func<HdfHandle, IO<Fin<A>>> read) =>
        IO.pure(Open(source, policy)).Bind(opened => opened.Match(
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

    // The entry the `Parallel` column exists for: a worker fan opens one handle PER WORKER and the source case
    // decides whether that is lawful. A `Payload` source shares one `Stream.Position` across every reader, so the
    // fan refuses it here rather than corrupting a concurrent read at the driver.
    public static Fin<Seq<HdfHandle>> Fan(HdfSource source, HdfArchivePolicy policy, int workers) =>
        !source.Parallel
            ? Fin.Fail<Seq<HdfHandle>>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Type(source.GetType()))))
            : workers < 1
            ? Fin.Fail<Seq<HdfHandle>>(new ComputeFault.PayloadOverBounds($"<hdf5-fan-workers:{workers}>"))
            : toSeq(Range(0, workers)).Traverse(_ => Open(source, policy).ToValidation<Error>()).As()
                .ToFin().MapFail(static error => error);

    // Zero-copy view over an array-backed payload; a non-array payload takes its one staging copy HERE, typed,
    // never an unreceipted ToArray at a call site.
    static MemoryStream View(ReadOnlyMemory<byte> bytes) =>
        MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new MemoryStream(bytes.ToArray(), writable: false);

    public static HdfWriter Begin(H5File graph, Stream sink, HdfArchivePolicy policy) =>
        new(graph.BeginWrite(sink, new H5WriteOptions()));
}
```

## [03]-[CHUNK_CURSOR]

- Owner: `ChunkGrid` — the ONE station-outermost container-grid correspondence, forward and inverse under one owner: the `(extent, components, budget) → (FileDims, Chunks)` derivation, the grid-ordinal decomposition, and the chunk-aligned `HyperslabSelection` that ordinal names; `ArchiveSession` the ONE declared-write capsule every archive artifact class composes, folding the slot mint, the graph build, the attribute stamp, the writer open, and the release into one act; `ArchiveSlot<T>` the typed dataset declaration and `ArchiveAttribute` the closed attribute vocabulary a container stamps; `HdfWriter` the `BeginWrite` session wrapper the capsule holds; `ChunkCursor<T>` the per-slot typed write cursor the session hands back, holding the monotone ordinal the caller used to carry.
- Entry: `ArchiveSession.Open(Stream, HdfArchivePolicy, Seq<IArchiveSlot>, Seq<(string, ArchiveAttribute)>)` mints the declared-write capsule and `ArchiveSession.Write` is its bracketed form, releasing on every outcome arm; `session.Cursor(ArchiveSlot<T>)` is the one write door; `ChunkGrid.Derive(ReadOnlySpan<int> extent, int components, int targetChunkElements)` is the derivation every producer and archive consumer composes — station axis chunks at 1, the component axis rides whole as the trailing extent, interior axes halve largest-first until the slab meets the element budget; `ChunkGrid.Seat(ReadOnlySpan<ulong> fileDims, ReadOnlySpan<uint> chunks)` seats a container's OWN declared chunk grid without re-deriving it; `grid.Selection(int ordinal)` is the chunk-aligned hyperslab that ordinal addresses and `grid.Slice(int ordinal, int elementBytes, int payloadBytes)` the byte extent a random-access read takes, empty past the payload bound; `HdfWriter.Open<T>(H5Dataset<T[]> slot, ChunkGrid grid)` mints the per-slot cursor and `ChunkCursor<T>.Write(T[] chunk)` advances it, returning `Fin<Unit>`.
- Auto: the cursor OWNS the ordinal, so an out-of-order or repeated chunk is unrepresentable rather than refused — the only ordinal the cursor accepts is the one it already holds, so the parameter reconstructed nothing and every call site drops a hand-maintained counter with it. The cursor's own `Count` bound closes the write: a chunk past the grid's declared count faults typed rather than reaching the library's dataspace.
- Receipt: none new — a chunked write's evidence rides the composing consumer's own receipt (`StreamSegment` bytes and segments), never a second row here.
- Packages: PureHDF (`H5Dataset<T>`, `H5NativeWriter.Write<T>`, `HyperslabSelection(int, ulong[], ulong[])` and the four-argument stride/count form), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new archive artifact class DECLARES its slots and attributes and takes its cursors off the one session — zero rows here; a new attribute shape is one `ArchiveAttribute` case; a new grid-shaping law is one column on `ChunkGrid` the derivation reads.
- Boundary: `Chunks can only be written once.` throws from the library's v4 index MID-ENCODE, after the producing work is already spent — the cursor refuses at admission instead, and with the ordinal no longer a parameter the refusal is a state no caller can construct; `ChunkGrid` is the SINGLE owner of the ordinal↔hyperslab correspondence, so the forward derivation, the write selection, and the random-access byte slice cannot fork — the three hand spellings this collapse replaces agreed only by inspection and each carried its own row-major decomposition; the grid is a value with no container attached, so a producer derives one before any file exists and an ingest seats the container's own without a second concept entering.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// ONE chunk-grid owner: derivation, ordinal decomposition, write selection, and byte slice are four projections
// of one correspondence, so no consumer re-spells a row-major decompose. The factory ACCUMULATES — a grid with a
// rank mismatch AND a non-positive extent reports both, because a caller cannot see the second defect after the
// first refusal. `Grid` is chunks-per-axis, `Chunk` the chunk extent per axis with the COMPONENT axis trailing,
// and `FileDims` their product — the station×component layout the native header and the container share.
[ComplexValueObject]
public sealed partial class ChunkGrid {
    public ReadOnlyMemory<int> Grid { get; }
    public ReadOnlyMemory<uint> Chunk { get; }
    public ReadOnlyMemory<ulong> FileDims { get; }

    public int Rank => Chunk.Length;
    public int Count => Grid.Span.ToArray().Aggregate(1, static (acc, axis) => acc * axis);
    public int ChunkElements => Chunk.Span.ToArray().Aggregate(1, static (acc, axis) => acc * (int)axis);

    // Grid ordinal -> chunk-aligned hyperslab: coordinates decompose row-major over the grid, starts land on
    // chunk boundaries, blocks are one chunk — the only write shape the write-once law admits.
    public HyperslabSelection Selection(int ordinal) {
        ulong[] starts = new ulong[Rank];
        ReadOnlySpan<int> grid = Grid.Span;
        ReadOnlySpan<uint> chunk = Chunk.Span;
        int remainder = ordinal;
        for (int axis = grid.Length - 1; axis >= 0; axis--) {
            starts[axis] = (ulong)(remainder % grid[axis]) * chunk[axis];
            remainder /= grid[axis];
        }
        ulong[] blocks = new ulong[Rank];
        for (int axis = 0; axis < Rank; axis++) { blocks[axis] = chunk[axis]; }
        return new HyperslabSelection(Rank, starts, blocks);
    }

    // The SAME ordinal read as a flat byte extent — the frustum-cull seam a viewport takes after mapping its
    // frustum onto grid coordinates. Out-of-range answers an empty range rather than a fault, because a cull that
    // walks past the last chunk is a bound, never a corruption.
    public Range Slice(int ordinal, int elementBytes, int payloadBytes) {
        long start = (long)ordinal * ChunkElements * elementBytes;
        return start >= payloadBytes || start < 0L
            ? new Range(0, 0)
            : new Range((int)start, (int)Math.Min(start + ((long)ChunkElements * elementBytes), payloadBytes));
    }

    // ONE station-outermost derivation serves the native layout, the HDF5 encode, and every archive consumer —
    // `Solver/discretization` `FieldSpace` composes it downward, so two chunk grids never fork one concept.
    public static Validation<Error, ChunkGrid> Derive(ReadOnlySpan<int> extent, int components, int targetChunkElements) {
        if (extent.Length < 1 || components < 1 || targetChunkElements < 1) {
            return Fail<Error, ChunkGrid>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Counts(extent.Length, components, targetChunkElements))));
        }
        ulong[] dims = [.. extent.ToArray().Select(static axis => (ulong)axis), (ulong)components];
        uint[] chunks = [1U, .. extent[1..].ToArray().Select(static axis => (uint)axis), (uint)components];
        long slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        while (slab > targetChunkElements) {                            // Exemption: a shrink loop with no fixed trip count; the rail resumes at Create
            int widest = 1;
            for (int axis = 2; axis < chunks.Length - 1; axis++) { widest = chunks[axis] > chunks[widest] ? axis : widest; }
            if (chunks[widest] <= 1) { break; }
            chunks[widest] = (chunks[widest] + 1) / 2;
            slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        }
        return Seat(dims, chunks);
    }

    // Seat a container's OWN declared grid: an ingested corpus states its chunking, so re-deriving one would
    // publish a layout the file never carries and strand `Selection`/`Slice` against the real chunk addresses.
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

// --- [TYPES] ------------------------------------------------------------------------------
// The attribute vocabulary a declared container carries, CLOSED. `H5File.Attributes[name] = value` takes `object`,
// so an untyped bag let a producer stamp a value the library then boxes and a foreign reader cannot type; these
// four are what every archive artifact class on the branch actually writes.
[Union]
public abstract partial record ArchiveAttribute {
    private ArchiveAttribute() { }

    public sealed record Text(string Value) : ArchiveAttribute;
    public sealed record Real(double Value) : ArchiveAttribute;
    public sealed record Whole(long Value) : ArchiveAttribute;
    public sealed record Flag(bool Value) : ArchiveAttribute;

    internal object Boxed => Switch<object>(
        text: static t => t.Value, real: static r => r.Value, whole: static w => w.Value, flag: static f => f.Value);
}

// --- [MODELS] -----------------------------------------------------------------------------
// One DECLARED dataset in a session: its path, its element type, and the chunk grid it writes under. The slot
// stays TYPED so the cursor a composer takes is typed, while `Seat` erases exactly as far as the graph needs —
// the `H5File` indexer takes the dataset object and nothing more.
public sealed record ArchiveSlot<T>(string Path, ChunkGrid Grid) : IArchiveSlot where T : unmanaged {
    public Option<H5Dataset<T[]>> Dataset { get; private set; }

    // Seating is the ONE erasure point and it is one hop wide: the `H5File` indexer takes the dataset object and
    // nothing more, so the type survives on this slot for the cursor the composer then opens.
    object IArchiveSlot.Seat(HdfArchivePolicy policy) {
        H5Dataset<T[]> seated = new(Grid.FileDims.ToArray(), Grid.Chunk.ToArray(), datasetCreation: policy.Creation());
        Dataset = Some(seated);
        return seated;
    }
}

public interface IArchiveSlot {
    string Path { get; }
    ChunkGrid Grid { get; }
    internal object Seat(HdfArchivePolicy policy);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// THE deferred-write capsule every archive artifact class composes. Four producers — this branch's solve archive,
// the mesh container, the clash twin seal, and the ensemble seal — each re-spelled the SAME five steps: mint one
// `H5Dataset<T[]>` per slot off the policy's filter pipeline, build the `H5File` graph, stamp its attributes,
// `Begin` the writer, and dispose it under a `using`. Four copies of one shape means a filter-pipeline change,
// an attribute-typing rule, or a release-on-fault repair had four places to miss. Here the declaration is a
// VALUE, the session is the handle a composer holds, and the cursor it hands back is the only write door.
public sealed class ArchiveSession : IDisposable {
    readonly HdfWriter _writer;
    readonly HdfArchivePolicy _policy;

    ArchiveSession(HdfWriter writer, HdfArchivePolicy policy) => (_writer, _policy) = (writer, policy);

    // Declaration THEN open, in one act: slots seat against the policy's own `Creation()`, attributes stamp
    // through the closed vocabulary, and the writer opens over the composition's sink. A refusal here has
    // allocated no chunk, so there is nothing to release that the caller must remember.
    public static Fin<ArchiveSession> Open(
        Stream sink, HdfArchivePolicy policy, Seq<IArchiveSlot> slots, Seq<(string Key, ArchiveAttribute Value)> attributes) =>
        slots.IsEmpty
            ? Fin.Fail<ArchiveSession>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
            : slots.Map(static slot => slot.Path).Distinct().Count != slots.Count
            ? Fin.Fail<ArchiveSession>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(slots.Map(static slot => slot.Path).Distinct().Count, slots.Count))))
            : Op.Of(name: "hdf5.session-open").Catch(() => {
                H5File graph = new();
                slots.Iter(slot => graph[slot.Path] = slot.Seat(policy));
                attributes.Iter(pair => graph.Attributes[pair.Key] = pair.Value.Boxed);
                return Fin.Succ(new ArchiveSession(HdfArchive.Begin(graph, sink, policy), policy));
            });

    // The one write door: a typed cursor over a slot this session actually declared. A slot from another session
    // has no seated dataset, so the refusal is a state the type system reaches rather than a mid-encode fault.
    public Fin<ChunkCursor<T>> Cursor<T>(ArchiveSlot<T> slot) where T : unmanaged =>
        slot.Dataset
            .Map(dataset => _writer.Open(dataset, slot.Grid))
            .ToFin(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Initialized, new ContractEvidence.Key(slot.Path))));

    // The bracketed form, for a composer whose whole emit fits one expression — release binds to EVERY outcome
    // arm, where a `using` inside a rail lambda binds it to the success arm alone.
    public static IO<Fin<A>> Write<A>(
        Stream sink, HdfArchivePolicy policy, Seq<IArchiveSlot> slots,
        Seq<(string Key, ArchiveAttribute Value)> attributes, Func<ArchiveSession, IO<Fin<A>>> emit) =>
        IO.pure(Open(sink, policy, slots, attributes)).Bind(opened => opened.Match(
            Succ: session => IO.lift(() => session).Bracket(emit, static s => IO.lift(() => { s.Dispose(); return unit; })),
            Fail: error => IO.pure(Fin<A>.Fail(error))));

    public void Dispose() => _writer.Dispose();
}

// Deferred-write session. The per-slot cursor lives on `ChunkCursor<T>` rather than an `object`-keyed dictionary
// here: erasing the slot's own type to key a map is what forced the caller to carry the ordinal back in.
public sealed class HdfWriter : IDisposable {
    readonly H5NativeWriter _writer;

    internal HdfWriter(H5NativeWriter writer) => _writer = writer;

    public ChunkCursor<T> Open<T>(H5Dataset<T[]> slot, ChunkGrid grid) where T : unmanaged => new(this, slot, grid);

    internal Fin<Unit> Write<T>(H5Dataset<T[]> slot, T[] chunk, HyperslabSelection file) where T : unmanaged =>
        Op.Of(name: "hdf5.chunk-write").Catch(() => { _writer.Write(slot, chunk, fileSelection: file); return Fin.Succ(unit); });

    public void Dispose() => _writer.Dispose();
}

// The write-once law made STRUCTURAL: the cursor holds the only ordinal the slot will accept, so an out-of-order
// or repeated chunk is a value no caller can spell rather than a refusal the caller must handle. `Chunks can only
// be written once.` throws from the library's v4 index MID-ENCODE, after the producing work is already spent —
// the count bound below is what keeps that fault unreachable.
public sealed class ChunkCursor<T> where T : unmanaged {
    readonly HdfWriter _writer;
    readonly H5Dataset<T[]> _slot;
    readonly ChunkGrid _grid;
    int _next;

    internal ChunkCursor(HdfWriter writer, H5Dataset<T[]> slot, ChunkGrid grid) => (_writer, _slot, _grid) = (writer, slot, grid);

    public int Next => _next;
    public bool Complete => _next == _grid.Count;

    public Fin<Unit> Write(T[] chunk) =>
        _next >= _grid.Count
            ? Fin.Fail<Unit>(new ComputeFault.PayloadOverBounds($"<hdf5-chunk-count:{_next}:{_grid.Count}>"))
            : _writer.Write(_slot, chunk, _grid.Selection(_next)).Map(_ => { _next++; return unit; });
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
