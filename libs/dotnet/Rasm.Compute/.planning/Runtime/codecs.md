# [COMPUTE_CODECS]

Rasm.Compute owns the compute-and-transport half of artifact interchange: the structural geometry delta that lays a changed mesh, B-rep, point cloud, or NURBS net down as touched chunks alone, the content-addressed identity every codec on the branch keys on, and the columnar projection that hands the Persistence lake custodian a declared, self-describing generation.

`DeltaCodec` owns the FastCDC-chunked structural diff and the shared `Quantization` bit-budget law both it and `Runtime/field` compose; `CanonicalForm`/`InterchangeIdentity` own the canonical byte form, the policy-seeded interchange cache key, the HLC compose, and the ONE object-plane address grammar; `ArrowBatch` owns the columnar dataset projection and the one `Landing` the custodian's `FlatTableEgress.Land` takes. The chunked-field codecs live at `Runtime/field`, the HDF5 session at `Runtime/archive`, and the companion hop and tile partition at `Runtime/tiles`.

GLB geometry-content identity composes the kernel seed-zero `XxHash128` `GeometryHash` here, never re-minted with a policy seed.

## [01]-[INDEX]

- [02]-[GEOMETRY_DELTA]: FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; the shared quantization kernel; progressive ordering as a row.
- [03]-[CONTENT_ADDRESSING]: policy-seeded canonical-form `XxHash128` interchange-cache key (the GLB geometry-content identity is the kernel seed-zero `GeometryHash` composed, distinct); the empty-artifact sentinel; the HLC two-half compose; the one object-plane address grammar.
- [04]-[ARROW_BATCH]: `Solver/sweep` `DoeDataset`, `Runtime/receipts` `ChargebackDataset`, and the `GeometryDataset` kernel-encode corpus project into self-describing Arrow batches under ONE `LakeDataset` family — the row-major pair folding through the Persistence declaration fold, the geometry corpus borrowing its arena verbatim, and one `Landing` handing the custodian everything `Land` takes.

## [02]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` the structural-diff target rows, each carrying its whole normalization law as a behavior column; `ChunkOrder` `[SmartEnum<string>]` the transmission-ordering rows carrying their own comparator, so the progressive posture is a value the delta records rather than a bool the codec re-branches on; `DeltaPolicy` the `[ComplexValueObject]` chunk policy whose factory admits once, so the two ends that used to re-prove it on every `Diff` and every `Apply` read an already-admitted value; `DeltaChunk`/`GeometryDelta` the content-addressed delta records; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission; `Quantization` the shared codec quantization law this lane and `Runtime/field` both compose.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control; `ChunkOrder` rows `Sequential` (recipe order) and `Progressive` (largest-first, so a transmission renders coarse coverage before fine detail).
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the ordered target chunk recipe (`TargetChunks`) with the new-chunk payload (`Added`, hashes absent from the base); `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` walks the recipe and reconstructs the NORMALIZED target exactly, pulling each chunk from the payload or the re-chunked base — `TargetHash` is taken over the normalized bytes, so the verify proves the reconstruction bit-for-bit and `GeometryDelta.GeometricError` states the residual that separates it from the caller's original target; `Fin<T>` aborts on float alignment, base or target hash mismatch, corrupt payload framing, and an unresolved recipe hash.
- Auto: `Diff` first runs each kind's row-owned `Normalize` — the float-column kinds (vertex/point) round every float to the finer of the bit-budget grid and `Tolerance` so a sub-tolerance perturbation hashes to one chunk, bounded-lossy within `Tolerance`; topology and B-rep-face streams pass verbatim; the `nurbs-control` parametric stream rounds its control-net coordinate block alone, knots and weights crossing verbatim — then runs FastCDC over the normalized bytes — a 256-entry SplitMix64 `Gear` table rolls the fingerprint, a STRICT mask below `AvgChunk` and a LOOSE mask above normalize the chunk-size distribution so an inserted vertex shifts only its local chunk; `TargetChunks` records the ordered hash recipe, `Added` the distinct new chunks laid out under the policy's own `ChunkOrder` row, and the delta's own `GeometricError` the quantization step every one of them was rounded to (zero on a kind that passed verbatim), so the residual is stated once rather than restated per chunk; the delta carries its own `DeltaPolicy` so `Apply` re-chunks the base identically and round-trips deterministically. Every content digest on this lane rides the kernel `ContentHash.Of` seed-zero entry, so the delta's base and target identities are the SAME key the Persistence blob lane and the `Rasm/Domain/identity` owner mint.
- Receipt: the `Cache` receipt carries the delta content key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable — `GeometryDelta.BaseBytes` and `DeltaBytes` are the columns that stamp reads, and `Ratio` is the derived figure a board spells; a progressive transmission stamps the `ChunkOrder` row's key.
- Packages: System.IO.Hashing, System.Numerics.Tensors, LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (project — `ContentHash.Of` the ONE seed-zero identity entry, `Deterministic` the splitmix64 owner, and the kernel reconciliation `EncodeForm.Parametric` canonical stream the `nurbs-control` payload rides), Rasm.Persistence (project), BCL inbox (`System.Numerics.BitOperations` mask sizing)
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row carrying its row-owned `Normalize` law; a new transmission posture is one `ChunkOrder` row carrying its comparator; a new chunk policy column is one field on `DeltaPolicy` the factory admits beside its siblings; zero new surface.
- Boundary: geometry delta is the structural diff the blob-level delta never owned — the Persistence blob delta diffs opaque bytes, this diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only touched chunks; the diff algebra pairs with the `Rasm.Persistence/Version/ledger#CHANGEFEED` closure-graph diff, Compute owning the structural chunking and the Persistence sync lane the content-key delta, neither re-deriving the other, and the generated `rasm.contracts.sync.SyncService` (`Rasm.Persistence/Version/ledger#SYNC_TRANSPORTS`) carries the delta's op-log frames between stores; the chunker is real FastCDC — a `Gear` rolling fingerprint with a STRICT-below / LOOSE-above-`AvgChunk` dual-mask tightening the size distribution so a local edit shifts only its own chunk, a fixed-block or single-mask shift-add chunker the rejected form; reconstruction is order-faithful and hash-verified — `TargetChunks` places a mid-stream insert at its true position, not the tail, and `Apply` re-chunks the base under the delta's OWN `DeltaPolicy`, never a hardcoded one — but LOSSLESS is a per-KIND property this codec never claims whole: a non-quantizable kind (`mesh-topology`, `brep-face`) passes `Normalize` verbatim, so its reconstruction IS the original target, while a quantizable kind hashes the NORMALIZED bytes, so `Apply` returns the target rounded to the delta's own grid and `GeometryDelta.GeometricError` carries that step — the finer of the bit grid and `Tolerance`, the residual law the `DeltaPolicy` row decides — as a bound the caller STATES rather than assumes; a delta advertised lossless across every kind is what turns a bounded-lossy round trip into a silent one, and a per-chunk restatement of the one step is the column that collapse deleted; the bounded-lossy `Normalize` never exceeds the geometry tolerance; the new-chunk set transmits progressively in `ChunkOrder` over whichever artifact seam carries it and content-key-dedups against the Persistence blob lane (never a second delta store); the geometry-kind discriminant scopes quantization, so a topology-only edit never quantizes and a position-only edit never re-transmits the topology column; the `nurbs-control` payload IS the kernel `Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE` `EncodeForm.Parametric` canonical counted stream — the one frozen parametric byte layout, read here the way Persistence reads the frozen mesh layout, so a Compute-local NURBS byte encoding is the deleted second layout — and its row's `Normalize` scopes the tolerance grid to the control-net coordinate block ALONE, knot and weight bytes crossing verbatim: the `Rasm/Parametric/nurbs#NURBS_ENGINE` `Nurbs.Of` admission law (normalized clamped knots, strictly positive weights) holds by CONSTRUCTION on every emitted delta, so a rounded net the owner faults is unrepresentable rather than guarded — a whole-stream float grid rounding knots and weights, driving a weight non-positive or de-normalizing a knot vector, is the rejected form — while a malformed counted layout refuses the typed `<delta-parametric-layout:…>` fault at normalization, and a post-quantization re-admission call re-validating what the scoped grid already preserves is the interior re-validation the admission law forecloses.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Each row carries its whole normalization law as a behavior column — the bytes the chunker hashes and the one
// grid step the delta reports — so the per-kind admission, grid scope, and verbatim passes live on the
// vocabulary, never re-derived in the codec body: a new kind is one row, zero codec edits.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeometryDeltaKind {
    public static readonly GeometryDeltaKind MeshVertex = new("mesh-vertex", DeltaCodec.NormalizeFloatColumns);
    public static readonly GeometryDeltaKind MeshTopology = new("mesh-topology", DeltaCodec.NormalizeVerbatim);
    public static readonly GeometryDeltaKind BrepFace = new("brep-face", DeltaCodec.NormalizeVerbatim);
    public static readonly GeometryDeltaKind PointCloudOctant = new("pointcloud-octant", DeltaCodec.NormalizeFloatColumns);
    public static readonly GeometryDeltaKind NurbsControl = new("nurbs-control", DeltaCodec.NormalizeParametricNet);

    [UseDelegateFromConstructor]
    internal partial Fin<(ReadOnlyMemory<byte> Bytes, double Step)> Normalize(ReadOnlyMemory<byte> bytes, DeltaPolicy policy);
}

// The transmission posture as a ROW carrying its own layout comparator, so the recorded delta states which order
// it was laid out in and a receipt reads a key instead of a bool. The `bool Progressive` this replaces selected
// between an `OrderByDescending` and the identity at one site and told a consumer nothing.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ChunkOrder {
    public static readonly ChunkOrder Sequential = new("sequential", static added => added);
    public static readonly ChunkOrder Progressive = new("progressive", static added => toSeq(added.OrderByDescending(static chunk => chunk.ByteLength)));

    [UseDelegateFromConstructor]
    internal partial Seq<DeltaChunk> Layout(Seq<DeltaChunk> added);
}

// --- [MODELS] -----------------------------------------------------------------------------
// Admitted ONCE. The hand `ValidPolicy` predicate this replaces ran at both ends of the round trip — the encode
// re-proving what it had just built, the decode re-proving what the delta carried — and reported one aggregate
// slug for five independent facts. The factory accumulates, so a policy with an inverted chunk band AND an
// out-of-range bit budget reports both.
[ComplexValueObject]
public sealed partial class DeltaPolicy {
    public static readonly DeltaPolicy Canonical = Create(
        minChunk: 2048, avgChunk: 8192, maxChunk: 65536, quantizationBits: 14, tolerance: 1e-5, order: ChunkOrder.Progressive);

    public int MinChunk { get; }
    public int AvgChunk { get; }
    public int MaxChunk { get; }
    public int QuantizationBits { get; }
    public double Tolerance { get; }
    public ChunkOrder Order { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minChunk, ref int avgChunk, ref int maxChunk,
        ref int quantizationBits, ref double tolerance, ref ChunkOrder order) {
        int low = minChunk, mid = avgChunk, high = maxChunk, bits = quantizationBits;
        double grid = tolerance;
        validationError = Seq(
                low > 0 ? None : Some($"<delta-min-chunk:{low}>"),
                low <= mid ? None : Some($"<delta-chunk-band:{low}>{mid}>"),
                mid <= high ? None : Some($"<delta-chunk-band:{mid}>{high}>"),
                bits is >= 0 and <= 24 ? None : Some($"<delta-bits:{bits}>"),
                double.IsFinite(grid) && grid > 0d ? None : Some($"<delta-tolerance:{grid:R}>"))
            .Somes()
            .Match(Empty: () => null, More: (head, tail) => new ValidationError(string.Join(';', head.Cons(tail))));
    }
}

public readonly record struct DeltaChunk(UInt128 Hash, int Ordinal, int Offset, int ByteLength);

// `TargetHash` is over the NORMALIZED target, so `Apply` reconstructs THAT exactly and `GeometricError` is the one
// quantization step separating it from the caller's original bytes — zero wherever the kind passed verbatim. The
// bound rides the delta rather than each `Added` chunk because every chunk was rounded to the same step: a
// per-chunk copy of one number is a value a partial transmission can contradict and a caller has to reduce.
//
// `Payload` is a `ReadOnlyMemory<byte>`, which record equality compares BY REFERENCE — two byte-identical deltas
// would read unequal, so a dedup keyed on the delta value re-transmits what it already holds. The generated
// comparer reads the sequence, and the two `Seq` columns compare in ORDER because the recipe's order IS content.
[Equatable]
public sealed partial record GeometryDelta(
    GeometryDeltaKind Kind,
    UInt128 BaseHash,
    UInt128 TargetHash,
    [property: OrderedEquality] Seq<UInt128> TargetChunks,
    [property: OrderedEquality] Seq<DeltaChunk> Added,
    [property: SequenceEquality] ReadOnlyMemory<byte> Payload,
    DeltaPolicy Policy,
    double GeometricError,
    long BaseBytes,
    long DeltaBytes) {
    // The compression figure the `Cache` receipt reads, derived rather than stored: a stored ratio is a third
    // number the two byte counts can contradict.
    public double Ratio => BaseBytes > 0L ? (double)DeltaBytes / BaseBytes : 0d;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Shared codec quantization law, composed by the `Runtime/field` field and residual quantizers and the
// normalization rows below: scale is one absolute-extremum SIMD reduction (never a Max/Min/Abs hand-roll), step
// the bit-budget grid, residual the relative rounding error a receipt records. One generic declaration serves
// every IEEE width — the float32 field and residual lanes and the float64 parametric net — through
// `TensorPrimitives`, so a per-width overload pair is the deleted form. `Code` discriminates on its INPUT SHAPE:
// the span arm is the vectorized fold a whole plane takes, the scalar arm the per-element step a causal residual
// walk needs, and neither is a mode flag beside the other.
public static class Quantization {
    public static (T Scale, T Step) Steps<T>(ReadOnlySpan<T> source, int bits) where T : IFloatingPointIeee754<T> {
        T scale = T.Abs(TensorPrimitives.MaxMagnitude(source));
        int levels = (1 << bits) - 1;
        return (scale, levels > 0 ? scale / T.CreateChecked(levels) : T.Zero);
    }

    public static T Code<T>(T value, T step) where T : IFloatingPointIeee754<T> =>
        step == T.Zero ? value : T.Round(value / step) * step;

    // Whole-plane coding: divide, round, multiply — three vectorized passes where the `Select(...).ToArray()` this
    // replaces ran one virtual call and one allocation per element over an entire field.
    public static void Code<T>(ReadOnlySpan<T> source, Span<T> destination, T step) where T : IFloatingPointIeee754<T> {
        if (step == T.Zero) { source.CopyTo(destination); return; }
        TensorPrimitives.Divide(source, step, destination);
        TensorPrimitives.Round(destination, destination);
        TensorPrimitives.Multiply(destination, step, destination);
    }

    public static double Residual(float value, float coded, float scale) => scale == 0f ? 0.0 : Math.Abs(value - coded) / scale;

    // The worst relative rounding error over a coded plane — the value an error-bounded encode gates on and the
    // receipt stamps, reduced vectorially rather than through a zipped per-element fold.
    public static double Worst<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> coded, T scale) where T : IFloatingPointIeee754<T> {
        if (scale == T.Zero) { return 0d; }
        T[] error = new T[source.Length];
        TensorPrimitives.Subtract(source, coded, error);
        TensorPrimitives.Abs<T>(error, error);
        return double.CreateChecked(T.Abs(TensorPrimitives.MaxMagnitude<T>(error)) / scale);
    }
}

public static class DeltaCodec {
    public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy) =>
        kind.Normalize(baseBytes, policy).Bind(normalizedBase =>
        kind.Normalize(targetBytes, policy).Map(normalizedTarget => {
            HashSet<UInt128> baseSet = toHashSet(FastCdc(normalizedBase.Bytes.Span, policy).Map(static c => c.Hash));
            Seq<DeltaChunk> targetChunks = FastCdc(normalizedTarget.Bytes.Span, policy);
            Seq<DeltaChunk> added = toSeq(targetChunks.Filter(c => !baseSet.Contains(c.Hash)).DistinctBy(static c => c.Hash));
            Seq<DeltaChunk> ordered = policy.Order.Layout(added);
            return new GeometryDelta(kind,
                ContentHash.Of(baseBytes.Span), ContentHash.Of(normalizedTarget.Bytes.Span),
                targetChunks.Map(static c => c.Hash), ordered, Concatenate(ordered, normalizedTarget.Bytes), policy, normalizedTarget.Step,
                baseBytes.Length, ordered.Sum(static c => (long)c.ByteLength));
        }));

    public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        ContentHash.Of(baseBytes.Span) == delta.BaseHash
            ? Reconstruct(delta, baseBytes).Bind(target => ContentHash.Of(target.Span) == delta.TargetHash
                ? Fin.Succ(target)
                : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-target-mismatch:{delta.TargetHash:x32}>")))
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-base-mismatch:{delta.BaseHash:x32}>"));

    // --- [NORMALIZATION_ROWS]

    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeVerbatim(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        Fin.Succ((bytes, 0d));

    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeFloatColumns(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        if (bytes.IsEmpty || bytes.Length % sizeof(float) != 0) { return Fin.Fail<(ReadOnlyMemory<byte> Bytes, double Step)>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Alignment(bytes.Length, sizeof(float))))); }
        if (policy.QuantizationBits <= 0) { return Fin.Succ(((ReadOnlyMemory<byte>)bytes, 0d)); }
        float[] source = MemoryMarshal.Cast<byte, float>(bytes.Span).ToArray();
        float step = GridStep<float>(source, policy);
        float[] quantized = new float[source.Length];
        Quantization.Code<float>(source, quantized, step);
        return Fin.Succ(((ReadOnlyMemory<byte>)MemoryMarshal.Cast<float, byte>(quantized.AsSpan()).ToArray(), (double)step));
    }

    // nurbs-control payload is the kernel EncodeForm.Parametric canonical counted stream (little-endian: direction
    // count; per direction degree, knot count, knots; weight count, weights; control count, xyz doubles). The grid
    // touches the CONTROL-NET block alone — knot and weight bytes copy verbatim — so the Nurbs.Of gate (normalized
    // clamped knots, strictly positive weights) holds by construction on every emitted delta, and a malformed
    // counted layout lands the typed refusal instead of a BCL slice-range message wearing this codec's verdict.
    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        Op.Of(name: "delta.parametric-layout").Catch(() => Fin.Succ(ParametricNet(bytes, policy)));

    static (ReadOnlyMemory<byte> Bytes, double Step) ParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        ReadOnlySpan<byte> stream = bytes.Span;
        int directions = BinaryPrimitives.ReadInt32LittleEndian(stream);
        int cursor = sizeof(int);
        for (int direction = 0; direction < directions; direction++) {  // Exemption: a counted-stream cursor walk whose trip count the stream itself declares; the rail resumes at the Op.Catch boundary above
            int knots = BinaryPrimitives.ReadInt32LittleEndian(stream[(cursor + sizeof(int))..]);
            cursor += (sizeof(int) * 2) + (knots * sizeof(double));
        }
        int weights = BinaryPrimitives.ReadInt32LittleEndian(stream[cursor..]);
        cursor += sizeof(int) + (weights * sizeof(double));
        int controls = BinaryPrimitives.ReadInt32LittleEndian(stream[cursor..]);
        int netOffset = cursor + sizeof(int);
        if (directions < 1 || weights != controls || netOffset + (controls * 3 * sizeof(double)) != stream.Length) { throw new InvalidDataException($"<extent:{directions}:{weights}:{controls}:{stream.Length}>"); }
        if (policy.QuantizationBits <= 0) { return (bytes, 0d); }
        byte[] normalized = stream.ToArray();
        Span<double> net = MemoryMarshal.Cast<byte, double>(normalized.AsSpan(netOffset));
        double step = GridStep<double>(net, policy);
        Quantization.Code<double>(net, net, step);
        return (normalized, step);
    }

    static T GridStep<T>(ReadOnlySpan<T> source, DeltaPolicy policy) where T : IFloatingPointIeee754<T> {
        T bitStep = Quantization.Steps(source, policy.QuantizationBits).Step;
        T tolerance = T.CreateChecked(policy.Tolerance);
        return bitStep <= T.Zero ? tolerance : T.Min(bitStep, tolerance);
    }

    // --- [FAST_CDC]

    // Incremental build is Cons-then-reverse: repeated `Add` on a `Seq` re-walks its spine at every cut, which on
    // a corpus-scale artifact is the chunk count squared.
    static Seq<DeltaChunk> FastCdc(ReadOnlySpan<byte> data, DeltaPolicy policy) {
        Seq<DeltaChunk> reversed = Seq<DeltaChunk>();
        int start = 0, ordinal = 0;
        while (start < data.Length) {                                   // Exemption: a content-defined cut walk whose step size the data decides; a `Span` cannot cross a lambda seam
            int cut = ContentDefinedCut(data[start..], policy);
            reversed = reversed.Cons(new DeltaChunk(ContentHash.Of(data.Slice(start, cut)), ordinal++, start, cut));
            start += cut;
        }
        return reversed.Reverse();
    }

    // CARVE — branch RULINGS [02] `[NOT] a frozen wire constant whose VALUES define a format and re-cut stored
    // payloads`: these three splitmix64 constants are the same values the kernel `Rasm/Domain/identity`
    // `Deterministic` owner carries, and they are RE-SPELLED here DELIBERATELY. The Gear table is not a random
    // stream this lane draws from — it is the FastCDC format's own frozen substitution table. Re-keying it from
    // the kernel owner would re-cut every chunk boundary in every stored delta, so a base and target chunked
    // under two Gear tables share no hash and every persisted delta becomes unappliable. The kernel owner and
    // this table therefore move independently by law, and the carve is stated here rather than assumed.
    static readonly ulong[] Gear = BuildGear();

    static ulong[] BuildGear() {
        ulong[] gear = new ulong[256];
        ulong state = 0x9E3779B97F4A7C15UL;
        for (int index = 0; index < 256; index++) {
            state += 0x9E3779B97F4A7C15UL;
            ulong mix = (state ^ (state >> 30)) * 0xBF58476D1CE4E5B9UL;
            mix = (mix ^ (mix >> 27)) * 0x94D049BB133111EBUL;
            gear[index] = mix ^ (mix >> 31);
        }
        return gear;
    }

    static int ContentDefinedCut(ReadOnlySpan<byte> window, DeltaPolicy policy) {
        int max = Math.Min(window.Length, policy.MaxChunk);
        if (max <= policy.MinChunk) { return max; }
        int normal = Math.Min(policy.AvgChunk, max);
        int bits = BitOperations.Log2((uint)Math.Max(1, policy.AvgChunk));
        ulong maskStrict = (1UL << Math.Min(62, bits + 2)) - 1, maskLoose = (1UL << Math.Max(1, bits - 2)) - 1;
        ulong fingerprint = 0;
        int index = policy.MinChunk;
        for (; index < normal; index++) { fingerprint = (fingerprint << 1) + Gear[window[index]]; if ((fingerprint & maskStrict) == 0) { return index; } }
        for (; index < max; index++) { fingerprint = (fingerprint << 1) + Gear[window[index]]; if ((fingerprint & maskLoose) == 0) { return index; } }
        return max;
    }

    // --- [PAYLOAD_FRAMING]
    // Chunk header: the chunk hash as the kernel's sixteen big-endian wire bytes (`ContentHash.Wire`, the one
    // persisted spelling of a content key), then ordinal and byte length little-endian, then the body.
    private const int ChunkHeader = 16 + (sizeof(int) * 2);
    private static readonly Op SplitKey = Op.Of(name: "delta.payload-split");

    static ReadOnlyMemory<byte> Concatenate(Seq<DeltaChunk> added, ReadOnlyMemory<byte> targetBytes) {
        int total = added.Sum(static c => c.ByteLength + ChunkHeader);
        byte[] buffer = new byte[total];
        Span<byte> sink = buffer.AsSpan();
        int cursor = 0;
        foreach (DeltaChunk chunk in added) {                           // Exemption: a framed write into a pre-sized span; a `Span` cannot be captured by any lambda
            ContentHash.Wire(chunk.Hash).Span.CopyTo(sink[cursor..]);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 16)..], chunk.Ordinal);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 20)..], chunk.ByteLength);
            targetBytes.Span.Slice(chunk.Offset, chunk.ByteLength).CopyTo(sink[(cursor + ChunkHeader)..]);
            cursor += ChunkHeader + chunk.ByteLength;
        }
        return buffer.AsMemory(0, cursor);
    }

    static Fin<ReadOnlyMemory<byte>> Reconstruct(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        SplitPayload(delta.Payload)
            .Bind(addedByHash => delta.Kind.Normalize(baseBytes, delta.Policy).Bind(normalized => {
                ReadOnlyMemory<byte> normalizedBase = normalized.Bytes;
                HashMap<UInt128, ReadOnlyMemory<byte>> baseByHash = FastCdc(normalizedBase.Span, delta.Policy)
                    .Fold(HashMap<UInt128, ReadOnlyMemory<byte>>(), (map, chunk) =>
                        map.AddOrUpdate(chunk.Hash, normalizedBase.Slice(chunk.Offset, chunk.ByteLength)));
                return delta.TargetChunks
                    .Fold(Fin.Succ(Seq<ReadOnlyMemory<byte>>()), (rail, hash) => rail.Bind(pieces =>
                        (addedByHash.Find(hash) | baseByHash.Find(hash))
                            .Map(pieces.Add)
                            .ToFin(new ComputeFault.CacheCorrupt($"<delta-chunk-missing:{hash:x32}>"))))
                    .Map(pieces => {
                        byte[] target = new byte[pieces.Sum(static piece => piece.Length)];
                        int written = pieces.Fold(0, (cursor, piece) => { piece.Span.CopyTo(target.AsSpan(cursor)); return cursor + piece.Length; });
                        return (ReadOnlyMemory<byte>)target.AsMemory(0, written);
                    });
            }));

    // A framed read on the rail: every truncation and the hash admission refuse typed, so no exception stands in
    // for a verdict and the loop is the one platform-forced statement seam a cursor over a span demands.
    static Fin<HashMap<UInt128, ReadOnlyMemory<byte>>> SplitPayload(ReadOnlyMemory<byte> payload) {
        Fin<HashMap<UInt128, ReadOnlyMemory<byte>>> map = HashMap<UInt128, ReadOnlyMemory<byte>>();
        int cursor = 0;
        while (map.IsSucc && cursor < payload.Length) {                 // Exemption: a framed read whose step the frame header declares; the rail carries every refusal out
            if (payload.Length - cursor < ChunkHeader) { return Fin.Fail<HashMap<UInt128, ReadOnlyMemory<byte>>>(new ComputeFault.CacheCorrupt($"<delta-header-truncated:{cursor}:{payload.Length}>")); }
            int byteLength = BinaryPrimitives.ReadInt32LittleEndian(payload.Span[(cursor + 20)..]);
            if (byteLength < 0 || byteLength > payload.Length - cursor - ChunkHeader) { return Fin.Fail<HashMap<UInt128, ReadOnlyMemory<byte>>>(new ComputeFault.CacheCorrupt($"<delta-chunk-truncated:{cursor}:{byteLength}:{payload.Length}>")); }
            int at = cursor;
            map = map.Bind(held => ContentHash.Admit(payload.Span.Slice(at, 16), SplitKey)
                .Map(hash => held.AddOrUpdate(hash, payload.Slice(at + ChunkHeader, byteLength))));
            cursor += ChunkHeader + byteLength;
        }
        return map;
    }
}
```

## [03]-[CONTENT_ADDRESSING]

- Owner: `CanonicalForm` the tag-folding kernel every keyed format or codec tag passes before it enters a preimage; `InterchangeIdentity` the interchange CACHE-PARTITION key derivation folding the canonical tag, the complete ordered output-policy vector, and the source bytes into ONE kernel `ContentHash.Of` preimage (distinct from the kernel seed-zero `GeometryHash` the seam/Bim/Persistence/peers share — distinct by PREIMAGE, never by seed), mirroring the model-lane `ModelIdentity.Snapshot` precedent, with `Compose` sealing the content key and HLC two-half stamp into one frame key, `SeedZero` minting the absent-artifact identity, and `Address` owning the ONE object-plane address grammar every Compute artifact-id spells; `ComputeArtifact` the emitted-bytes carrier the field, tile, and Bim export rails feed, landing content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(...)` — pure value; the contiguous and pooled-sequence cases derive identity from the canonical tag, the policy vector, and the payload bytes, while the geometry case frames the kernel arena's own witness digest WHOLE — the `DigestRoot` ordinal ahead of `ContentHash`, the kernel `RoundTripWitness.Root` dedup law read into the preimage — beside its descriptor roster and the index column before one streaming fold; `public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical)` folds the content key with the causal stamp in the fixed (physical, logical) half order; `public static UInt128 SeedZero(string formatKey, ReadOnlySpan<double> policy)` is the absent-artifact identity; `public static UInt128 Schema(AnalyticsSchema declaration)` frames a declared column roster injectively into the schema identity a landed generation keys on; `public static string Address(UInt128 contentKey, string kind)` is the ONE `<content-key:x32>:<kind>` object-plane spelling; `ComputeArtifact.Of` is the one emit-carrier mint deriving the content key from bytes with its complete policy vector, discriminating on the payload's own shape — a contiguous `ReadOnlyMemory<byte>` or a pooled `ReadOnlySequence<byte>` whose key folds segment by segment before any contiguity is demanded — and `ByteCount` derives off the carried payload rather than travelling as a second stored column a caller contradicts.
- Auto: every keyed input rides the kernel `CanonicalWriter` — `CanonicalForm.Tag` lower-cases invariant culture and trims the format/codec tag so `"GLB"` and `" glb "` key one identity, the writer's `String` length-frames it, `Doubles` count-frames the policy vector and canonicalizes every NaN payload and `-0.0` before the bits leave, the presence byte (`Optional`) separates an absent artifact from a present-but-empty one, and the payload rides `Raw` as the whole-payload leaf the writer's exemption names — injective framing, so distinct `(formatKey, policy, bytes)` triples never share a preimage. Tessellation folds deflection, tolerance, angle tolerance, tile depth, root geometric error, and split threshold; field residence folds bits and bound. `Admit` projects onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns, addressing the row through the same `Address` grammar every companion request answers.
- Receipt: the `Cache` receipt carries the content key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt; an absent-keyed artifact stamps the `SeedZero` identity so an absent-versus-empty distinction is auditable.
- Packages: NodaTime, LanguageExt.Core, Rasm (project — the kernel `EncodedGeometry` arena the geometry key frames, `ContentHash.Of`/`Hex`, `CanonicalWriter.String`/`Doubles`/`Optional`/`Raw`/`Rows`/`Ordinal`/`I64`/`U128`), Rasm.Persistence (project — `ArtifactIndexRow.Admit`, `Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema` the schema key frames), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one canonical scalar in the policy vector; a new keyed-input kind is one `InterchangeIdentity.Key` arity whose preimage states its own fields; a new per-vertex lane is a kernel `EncodingChannel` row the geometry key absorbs through the descriptor roster with no edit here; zero new surface.
- Law: ONE alphabet. Every preimage on this page is `ContentHash.Of(state, (s, w) => ...)` over the kernel writer — no seeded accumulator, no local length-prefix writer, no `XxHash3` seed folded from a tag. NAMED LOSS: the policy-seeded `XxHash128(seed)` form and the `CanonicalForm.Write`/`Seed`/`Scalar` trio are retired, so a cache key minted before this change re-keys once. Witness: `Key(formatKey, bytes, policy)` folds the same three facts in the same owner order, the policy now a framed field stream instead of a computed seed — the form `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` names for a digest seed folded from two or more fields.
- Boundary: interchange-cache identity is the kernel seed-zero `XxHash128` over the canonical preimage — the suite hash law the `Runtime/channels#ARTIFACT_FRAMES` whole-artifact identity and the model-lane `ModelIdentity` checksum hold, never a second hashing pass and never a path-keyed identity; canonical-form normalization is the cross-machine reproducibility floor — case-folded trimmed tag, length-framed text, count-framed little-endian policy scalars, negative zero collapsed, every NaN payload mapped to one quiet NaN — so two semantically-equal source artifacts on osx-arm64, linux-x64, and win-x64 cache-key one identity (the `lang:python:runtime/evidence/identity#IDENTITY` `ContentIdentity` folds the same format/deflection/tolerance, the cross-runtime peer), a raw-string-interpolated seed (`$"{formatKey}|{deflection:R}|..."`) the rejected drift defect keying distinctly across cultures and float renderings; the SHARED geometry WIRE hash is a DISTINCT key — the GLB geometry-content identity the seam `Rasm.Element/Graph/element#NODE_MODEL` `RepresentationContentHash`, the Persistence `Store/blobstore#OBJECT_STORE` blob name, and the `lang:typescript:core/interchange/frame#GEOMETRY_PLANE` + `lang:typescript:data/object/store` `ObjectKey` peers reproduce is the kernel `GeometryHash` over the canonical bytes ALONE (`libs/contracts/manifest.json` `MESH_ADJACENCY_GOLDEN` the golden vector anchoring C#/Python/TypeScript byte-parity), composed here and never re-minted under a policy head — a policy-keyed GLB geometry-content hash the named cross-runtime defect, the two keys coexisting by design because their preimages differ; the absent-artifact `SeedZero` identity is the absent-versus-empty law — the presence byte false under the policy head, never the hash of an empty span under it, so a cache key never collides absent against present-but-empty; the HLC compose order seals the kernel `Rasm/Domain/frame#RECEIPT_PORT` `ReceiptSinkPort.Advance` stamp byte-identical — the content key as the writer's two little-endian `I64` halves, the physical half as the `Instant` Unix-tick `long`, the logical half as the monotone `ulong` bits, the layout `libs/contracts/manifest.json` `HLC_TWO_HALF` freezes across the three runtimes — so `Compose` re-derives no ordering the capsule already fixed, a logical-half-first composition the named defect folding a fresh op as stale; the OBJECT-PLANE ADDRESS is one grammar with one owner — `<content-key:x32>:<kind>` through `ContentHash.Hex`, minted here and composed by every Compute artifact-id, three hand interpolations having spelled it independently before this seat; the `Rasm.Bim` `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` value object carries the IDENTICAL grammar at a package this one holds no project reference to, so the correspondence is a stated law rather than a shared type and a Compute-side re-declaration of that value object is unreachable, not merely undesirable; the key takes a format-key string rather than the Bim `InterchangeFormat` owner so the content identity stays a Compute concern decoupled from the moved format axis; every output-affecting scalar folds in owner order, so deflection, tolerance, angle tolerance, tile depth, root error, or split-threshold movement partitions a tileset key and prevents cross-setting hits; addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` under the content-key string `Path`, so the IFC semantic graph (Bim), the tessellated GLB, the field artifact, and a re-exported glTF are rows under the ONE kernel seed-zero `XxHash128` residence identity the Persistence index re-derives (`ArtifactIndexRow.Admit` -> `ContentAddress.Of`) — Compute owning only the policy-headed cache-key derivation (the logical label), the kernel/seam the seed-zero residence identity, Persistence the blob residence, none re-declaring another; the export-rail field/tile/re-exported-glTF artifacts self-key (their `SourceKey` their own `ContentHash`, single-projection) while the tessellated GLB and its semantic graph share one cross-projection `sourceKey` — the kernel seed-zero `SourceKey` the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` mints from the generated source oneof coordinate, generated STEP protocol coordinate iff STEP, and raw source bytes, with no tolerance or language-local tag — so an in-process semantic ingest re-derives the same family identity and distinct STEP application protocols never alias. That pure key, not the policy-headed cache key, is the `Option<UInt128> sourceKey` `ArtifactIndexRow.Project` groups under; each row's blob residence remains the kernel seed-zero `ContentAddress.Of`, so no GLB self-key strands geometry from semantics and no managed artifact copy stands beside the blob lane.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// String-keyed compute-lane emit carrier: the format tag is a bare key here, so the content identity stays
// decoupled from the Bim format axis a codec-rowed carrier would bind it to. This is the ONE shape every encode
// on the branch returns — the field codec, the HDF5 interop egress, and the tileset manifest all land here, and
// `InterchangeIdentity.Admit` is the projection that carries one onto the Persistence blob lane.
public sealed record ComputeArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    Instant At) {
    public long ByteCount => Bytes.Length;

    public string ArtifactKey => InterchangeIdentity.Address(ContentKey, FormatKey);

    public static ComputeArtifact Of(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, ReadOnlyMemory<double> policy = default) =>
        new(formatKey, bytes, InterchangeIdentity.Key(formatKey, bytes, policy), at);

    // Segmented mint for a pooled emit: the key folds the multi-segment sequence segment by segment, so a producer
    // whose key already resides never materializes a byte, and the miss path pays ONE exact-extent copy where a
    // growable writer paid a doubling ladder through the large-object heap. Arity is the input's own shape —
    // contiguous or segmented — never a mode flag beside the value.
    public static ComputeArtifact Of(string formatKey, ReadOnlySequence<byte> bytes, Instant at, ReadOnlyMemory<double> policy = default) {
        byte[] owned = new byte[checked((int)bytes.Length)];
        bytes.CopyTo(owned);
        return new(formatKey, owned, InterchangeIdentity.Key(formatKey, bytes, policy), at);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The ONE normalization this page still owns: the tag. Scalars canonicalize inside the kernel writer (`Doubles`
// collapses -0.0 and every NaN payload), so no local scalar normalization or byte layout survives beside it.
public static class CanonicalForm {
    public static string Tag(string raw) => raw.Trim().ToLowerInvariant();
}

public static class InterchangeIdentity {
    // The ONE object-plane address grammar on this branch: thirty-two lower-case hex characters, a colon, and the
    // canonical kind tag. Every Compute artifact-id spells it through here. The `Rasm.Bim`
    // `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` value object gates the IDENTICAL grammar at a package this
    // one holds no project reference to, so the two are one address SPACE reached through two owners by strata,
    // not by choice — a Compute-side copy of that value object cannot compile and a divergent spelling here
    // strands every address the Bim admission then refuses.
    public static string Address(UInt128 contentKey, string kind) =>
        string.Create(CultureInfo.InvariantCulture, $"{ContentHash.Hex(contentKey)}:{CanonicalForm.Tag(kind)}");

    // ONE preimage head for every payload arity: the length-framed tag, the count-framed policy vector, then the
    // presence byte. `MODAL_ARITY` admits the three entrypoints — a contiguous span, a pooled sequence, and a
    // descriptor-tiled arena are genuinely different payload shapes — and the head is spelled once rather than
    // at each arity.
    static CanonicalWriter Head(CanonicalWriter w, string formatKey, ReadOnlySpan<double> policy, bool present) =>
        w.String(CanonicalForm.Tag(formatKey)).Doubles(policy).Bool(present);

    public static UInt128 Key(string formatKey, ReadOnlyMemory<byte> bytes, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, bytes, policy), static (s, w) => Head(w, s.formatKey, s.policy.Span, present: true).Raw(s.bytes.Span));

    // Incremental sibling for pooled multi-segment payloads (a chunked field blob, a reassembled frame sequence):
    // the segments are ONE payload leaf written in order, so no segment boundary enters the preimage and a
    // re-segmented artifact keys identically.
    public static UInt128 Key(string formatKey, ReadOnlySequence<byte> bytes, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, bytes, policy), static (s, w) => {
            Head(w, s.formatKey, s.policy.Span, present: true);
            foreach (ReadOnlyMemory<byte> segment in s.bytes) { w.Raw(segment.Span); }   // Exemption: a span cannot be captured by any lambda; the segments are one leaf
        });

    // Channel-generic geometry identity over the ONE kernel arena, replacing the retired vertices/indices/normals
    // triple that silently EXCLUDED every lane it did not name — UV and colour among them — so a roster growth
    // moved no key and two leaves differing only in their UV unwrap collided. Three framed components seal it:
    // the witness composite WHOLE — the DigestRoot ordinal ahead of the ContentHash, per the kernel
    // RoundTripWitness.Root law that a dedup or lake-identity consumer reads the root beside the digest, so a
    // source-rooted Apply witness and a payload-rooted Of witness share no preimage even where their bytes
    // coincide, never a digest-only fold leaning on preimage-domain disjointness — the descriptor roster (WHICH
    // channels at WHICH storage width and element count produced them) as count-framed rows, and Indices (the one
    // non-channel column) as one length-framed leaf. New EncodingChannel rows therefore re-key by construction
    // and are named nowhere here.
    public static UInt128 Key(string formatKey, EncodedGeometry lanes, ReadOnlyMemory<byte> indices, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, lanes, indices, policy), static (s, w) =>
            Head(w, s.formatKey, s.policy.Span, present: !(s.lanes.Descriptors.IsEmpty && s.indices.IsEmpty))
                .Ordinal(s.lanes.Witness.Root.Key)
                .U128(s.lanes.Witness.ContentHash.Value)
                .Rows(s.lanes.Descriptors, static (d, x) => x.String(d.Channel.Key).String(d.Dtype.Key).I64(d.Count))
                .Ordinal(s.indices.Length)
                .Raw(s.indices.Span));

    // The DECLARED column roster as one injective preimage — count-framed rows of length-framed name and type —
    // so an additive column lands a compatible generation under its own key while a reordered or retyped column
    // lands a distinct tree the reader's positional ordinals never mis-bind. The `string.Join('|', ...)` digest
    // this replaces let a column named `a|b` spell the same preimage as the pair `a`, `b`, and a separator inside
    // a rendered Arrow type id collided two schemas onto one hive directory. Field metadata stays out of the
    // digest: a receipt fact rides `Schema.Metadata` and never re-keys the tree.
    public static UInt128 Schema(AnalyticsSchema declaration) =>
        ContentHash.Of(declaration, static (d, w) => w.String(d.Dataset)
            .Rows(d.Columns, static (column, x) => x.String((string)column.Name).String(column.Type.ToString())));

    // The absent-artifact identity: the same head every present payload carries, with the presence byte false,
    // so absent and present-but-empty key apart under one policy and no sentinel constant stands in for absence.
    public static UInt128 SeedZero(string formatKey, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, policy), static (s, w) => Head(w, s.formatKey, s.policy.Span, present: false));

    // The content key's two little-endian halves, the physical tick `long`, the logical `ulong` bits — the
    // `HLC_TWO_HALF` layout, written through the kernel members that already spell each width.
    public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical) =>
        ContentHash.Of((contentKey, physical, logical), static (s, w) =>
            w.U128(s.contentKey).I64(s.physical.ToUnixTimeTicks()).I64(unchecked((long)s.logical)));

    public static ArtifactIndexRow Admit(ComputeArtifact artifact, DataClassification classification, Option<UInt128> sourceKey) =>
        ArtifactIndexRow.Admit(ArtifactKind.Interchange, artifact.ArtifactKey, artifact.Bytes.Span, classification, artifact.At, sourceKey);
}
```

## [04]-[ARROW_BATCH]

- Owner: `LakeDataset` — the ONE lake-bound producer family, its three cases (`Doe`, `Geometry`, `Chargeback`) sharing one identity regime, one landing port, one metadata contract, and one consumer, so the arm row, the declared schema, the content key, the readable segment, and the required metadata are COLUMNS on the family rather than three parallel builders and three overloads that agreed only by inspection; `ArrowBatch` the columnar-construction surface projecting each case into self-describing `Apache.Arrow` batches; `GeometryDataset` the lake-bound corpus pairing one `PackKind` with its model segment and encoded instances, deriving both its schema identity and its generation key from content; `LakeLanding` the exact quadruple `FlatTableEgress.Land` takes. Core `Apache.Arrow` is the sole reference: the IPC writer, the LZ4/Zstd `CompressionCodecFactory`, the ADBC query surface, and the Flight-SQL transport are the Persistence egress rails, absent from the Compute closure.
- Cases: `Doe` the `Solver/sweep` design-of-experiments corpus (`LandingArm.Doe`, `study=` segment, `run` sort column); `Geometry` the kernel-encode corpus (`LandingArm.Geometry`, `model=` segment, `node` sort column, keying its schema off the kernel's own `PackSchema.SchemaId`); `Chargeback` the `Runtime/receipts` billing corpus (`LandingArm.Cost`, `month=` segment, `kind` sort column).
- Entry: `public static Fin<LakeLanding> Landing(LakeDataset dataset, TenantContext tenant, Option<MemoryAllocator> allocator)` is the ONE lake-landing projection — a total `Switch` sealing the case's batches, naming its `LandingArm` row and the readable segment that arm's hive key spells, and deriving the generation coordinate the custodian writes under; `public static Fin<Seq<RecordBatch>> Batches(LakeDataset dataset, Option<MemoryAllocator> allocator)` is the projection half a caller redeeming batches alone takes.
- Auto: the row-major cases fold through the Persistence `Query/residence#COLUMN_VOCABULARY` `ArrowLanding.Build<TRow>` — the declaration supplies the field list, its order, and every column's own builder, and the conformance proof ACCUMULATES across columns, so a producer handing one batch learns every offending column at once; the pivot from rows to columns happens inside that fold, so no producer arm re-spells a strided gather. The geometry arm gathers NOTHING — the kernel already tiled each channel contiguously at its own descriptor offset, so an `ArrowBuffer` borrows that slice and a `FixedSizeListType` of the channel's arity states the interleave already in the bytes, leaving the two identity columns as the only material a landing allocates; that arm binds pre-built columns through the metadata-bearing `RecordBatch` constructor against the SAME `AnalyticsSchema` declaration, so both paths derive one field order from one declaration. Metadata is REQUIRED at every arm and defaulted nowhere: `Schema.Builder` and `RecordBatch.Builder` expose no metadata seat, so a batch reaching the custodian with none states no content key, no window, and no strategy, and the arm's own `Metadata` column is what the fold carries.
- Receipt: none new — each batch is a projection of a standing dataset shape, and the landed generation's evidence rides the custodian's own `LandingArm.Slot`, never a second Compute row; the geometry corpus carries the kernel `RoundTripWitness` per instance, so quantization evidence is already proved upstream and no landing re-measures it.
- Packages: Apache.Arrow, NodaTime (`InstantPattern.ExtendedIso` the metadata instant, `LocalDatePattern.CreateWithInvariantCulture` over `Instant.InUtc().Date` the billing-month segment), Thinktecture.Runtime.Extensions (`DoeDesign`/`Substrate` `.Key`), System.IO.Hashing, Rasm.Persistence (project — `Query/lakehouse#FLAT_TABLE_EGRESS` `LandingArm`/`LakeGeneration`/`FlatTableEgress.Land`, `Query/residence#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnRow`/`ColumnShape`/`ColumnType`/`ColumnCell`/`TimeSpine`/`ArrowLanding.Build`/`Identifier`), Rasm (project — `TenantContext`, `ContentHash.Of`, `CanonicalWriter.U128`/`Rows`/`Ordinal`, and the `Rasm.Drawing` encode wire `EncodedGeometry`/`PackKind`/`PackSchema`/`EncodingChannel`/`ChannelDtype`), LanguageExt.Core, CommunityToolkit.HighPerformance (`MemoryOwner<double>` the strided scratch), BCL inbox (`CultureInfo.InvariantCulture`)
- Growth: a new dataset producer is one `LakeDataset` case answering the family's five columns — arm, declaration, content key, segment, metadata — and the `Landing` `Switch` then breaks LOUDLY until it does, where the three-overload form this collapse deletes admitted a fourth producer as two new public surfaces the custodian's own `LandingArm` roster never learned about; a new column is one `ColumnRow` on the declaration and one `ColumnCell` in the row fold; a new kernel `EncodingChannel` or `PackKind` reaches the geometry columns with ZERO edit here because the kind's own declared active set generates the schema; the `MemoryAllocator` injects per lane so a staging-bounded arena charges the batch buffers against the lane budget.
- Boundary: Compute BUILDS the columnar table; the Persistence lakehouse OWNS everything that CARRIES it — `ArrowStreamWriter`/`ArrowFileWriter` IPC, the `Apache.Arrow.Compression` LZ4/Zstd codec, the ADBC query surface, and the `FlightClient`/`FlightSqlClient` — so Compute holds one core `Apache.Arrow` reference, references none of the four egress packages, and opens no Flight listener; the DECLARATION is the contract, so this page hands `Land` an `AnalyticsSchema` and never a hand-built `Schema` beside a batch that agrees with it only by inspection, and each arm's declared columns include the SORT COLUMN its `LandingArm` row names — a generation whose declaration omits that column refuses at `Ordered` before a byte is written; a bare `DateTime` where the NodaTime instant crosses, the shared `MemoryAllocator.Default` where a lane arena is available, a per-element `Append(T)` loop where a span append exists, and a hand-rolled columnar byte layout `RecordBatch` already owns are the rejected forms; the geometry arm adds three of its own — a per-component scalar fan-out of an arity-3 channel, which re-keys the tree on every arity edit and reinstates the strided copy the kernel's tiling deletes; a half or unorm lane widened to float at the wrap, which re-spells values the round-trip witness certified at their stored width; and a schema key re-digested off the Arrow field list, which keys the hive tree on a projection the kernel never published while `PackSchema.SchemaId` is the identity the custodian's geometry row names by law; the sealed `RecordBatch` stops at the Compute edge — `Landing` hands the custodian a `LakeGeneration` coordinate, its declaration, its batches, and its metadata, and `FlatTableEgress.Land` writes them, so byte framing exists only where the `topology` axis puts the custodian in another process and the composition root frames it there through the Persistence IPC writer, never here.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// GeometryDataset carries one lake-bound corpus: one PackKind, one model segment, and the encoded instances
// sharing that kind's declared channel set. It homes HERE and not at a producing page because it has no life
// outside this landing — a corpus assembled only to cross the columnar seam is the landing owner's noun, where
// DoeDataset and ChargebackDataset each answer a question of their own before any batch exists. Schema identity
// and generation identity both DERIVE, so neither is forgeable at a call site and a retry re-lands the same bytes
// under the same key; the encode instant is deliberately absent, since a wall-clock stamp would re-key an
// unchanged corpus.
public sealed record GeometryDataset(PackKind Kind, string Model, Seq<EncodedGeometry> Instances) {
    public PackSchema Schema => PackSchema.Of(Kind);

    // Preimage seats the schema identity ahead of every instance record in landed order: order IS content because
    // its row ordinal joins a scan back to the encode, and the schema is identity-bearing because it decides which
    // columns each generation carries. Each instance record is the witness composite WHOLE — the DigestRoot
    // ordinal ahead of Witness.ContentHash — so a source-rooted Apply mint and a payload-rooted Of mint of ONE
    // geometry key distinct generations structurally, the preimage reading Root beside the digest exactly as the
    // kernel RoundTripWitness.Root dedup law demands, never a digest-only fold leaning on preimage-domain
    // disjointness. The kernel writer streams the fold, so no spine is materialized at any corpus size and the
    // count-framed `Rows` states the instance count the fixed-width concatenation once left implicit.
    public UInt128 ContentKey =>
        ContentHash.Of(this, static (d, w) => w.U128(d.Schema.SchemaId)
            .Rows(d.Instances, static (instance, x) => x.Ordinal(instance.Witness.Root.Key).U128(instance.Witness.ContentHash.Value)));
}

// ONE lake-bound producer family. Three builders and three `Landing` overloads shared a return rail, an allocator
// parameter, a metadata-bearing tail, and ONE consumer, and named a discriminant nowhere — so a fourth producer
// cost two new public surfaces and the custodian's `LandingArm` roster learned about it only if someone
// remembered. Here the arm, the declaration, the content key, the segment, and the metadata are COLUMNS: a new
// row on `LandingArm` breaks the `Switch` below at compile time, which is the correspondence topology binds.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LakeDataset {
    private LakeDataset() { }

    public sealed record Doe(DoeDataset Dataset) : LakeDataset;
    public sealed record Geometry(GeometryDataset Dataset) : LakeDataset;
    public sealed record Chargeback(ChargebackDataset Dataset) : LakeDataset;

    public LandingArm Arm => Switch(
        doe: static _ => LandingArm.Doe,
        geometry: static _ => LandingArm.Geometry,
        chargeback: static _ => LandingArm.Cost);

    public UInt128 ContentKey => Switch(
        doe: static d => d.Dataset.ContentKey,
        geometry: static g => g.Dataset.ContentKey,
        chargeback: static c => c.Dataset.ContentKey);

    // Geometry is the one arm whose SCHEMA KEY is not derived from its declaration: the kernel already mints a
    // content-keyed schema identity over its own kind and field roster, and the custodian's geometry landing row
    // names that law by name. Re-digesting the projection here keys the hive tree on a spelling the kernel never
    // published, splitting the tree on projection detail two encoders agreeing on geometry already share.
    public UInt128 SchemaKeyOf(AnalyticsSchema declaration) => Switch(
        state: declaration,
        doe: static (d, _) => InterchangeIdentity.Schema(d),
        geometry: static (_, g) => g.Dataset.Schema.SchemaId,
        chargeback: static (d, _) => InterchangeIdentity.Schema(d));

    // The READABLE hive segment the arm's partition noun carries. `Identifier` admits ASCII letters, digits, and
    // underscore under a NON-DIGIT lead, so every segment normalizes through one projection — and it rides the
    // RAIL, because `Identifier.Create` THROWS and three `Fin`-typed public entries used to let that throw escape
    // past them. The month token carries its own leading letter for exactly that reason.
    public Validation<Error, Identifier> Segment => Switch(
        doe: static d => Admitted(d.Dataset.Strategy.Key),
        geometry: static g => Admitted(g.Dataset.Model),
        chargeback: static c => Admitted(MonthSegment.Format(c.Dataset.WindowStart.InUtc().Date)));

    // REQUIRED at every arm, defaulted nowhere: the metadata seat is the only place a batch states its content
    // key, its strategy, its window, or its shape, and a fold that defaults it drops all four silently.
    public Seq<(string Key, string Value)> Metadata => Switch(
        doe: static d => Seq(
            ("content_key", Hex(d.Dataset.ContentKey)),
            ("strategy", d.Dataset.Strategy.Key),
            ("at", InstantPattern.ExtendedIso.Format(d.Dataset.At)),
            ("points", d.Dataset.Points.ToString(CultureInfo.InvariantCulture))),
        geometry: static g => Seq(
            ("content_key", Hex(g.Dataset.ContentKey)),
            ("schema_id", g.Dataset.Schema.Tag),
            ("kind", g.Dataset.Kind.Key),
            ("instances", g.Dataset.Instances.Count.ToString(CultureInfo.InvariantCulture))),
        chargeback: static c => Seq(
            ("content_key", Hex(c.Dataset.ContentKey)),
            ("window_start", InstantPattern.ExtendedIso.Format(c.Dataset.WindowStart)),
            ("window_end", InstantPattern.ExtendedIso.Format(c.Dataset.WindowEnd))));

    static string Hex(UInt128 key) => string.Create(CultureInfo.InvariantCulture, $"{key:x32}");

    static readonly LocalDatePattern MonthSegment = LocalDatePattern.CreateWithInvariantCulture("'m'uuuu'_'MM");

    // The ONE `Identifier` admission for every RUNTIME value on this lane: a hive segment and a producer-supplied
    // column name cross the same gate, so the throwing `Identifier.Create` reaches no value a caller can shape.
    // A frozen literal declaration below still mints through `Create` at type initialization — the same form the
    // Persistence `Query/federation` `KeyProjection` exemplar takes — because a literal that cannot admit is a
    // build-time defect, not a runtime refusal a rail could carry anywhere.
    internal static Validation<Error, Identifier> Admitted(string raw) =>
        Identifier.Validate(raw.Replace('-', '_'), null, out Identifier? admitted) is { } error
            ? Fail<Error, Identifier>(error)
            : Success<Error, Identifier>(admitted!);
}

// The exact quadruple `Query/lakehouse#FLAT_TABLE_EGRESS` `Land` takes, minus the custodian's own root, custody,
// and encryption stance. The three-tuple this replaces carried a hand-built `Schema` the custodian could not
// re-derive and dropped the metadata entirely.
public readonly record struct LakeLanding(
    LakeGeneration Generation, AnalyticsSchema Declaration, Seq<RecordBatch> Batches, Seq<(string Key, string Value)> Metadata);

// --- [OPERATIONS] -------------------------------------------------------------------------
// One Arrow construction owner, one producer family. Compute BUILDS the columnar table; the Persistence
// lakehouse OWNS everything that carries it (IPC writer, LZ4/Zstd codec, ADBC, Flight-SQL). Every builder takes
// an `Option<MemoryAllocator>` — the `MemoryAllocator? = null` default this replaces LICENSED the shared
// `MemoryAllocator.Default` at six call sites, which the Arrow catalog rejects wherever a lane arena exists.
public static class ArrowBatch {
    // ONE lake-landing projection over every producer, so no `LandDoe`/`LandCost` verb family arises: each arm
    // seals its batches, names its `LandingArm` row and readable segment, and derives the generation coordinate
    // the custodian writes under. Compute owns the batch shape and this coordinate ALONE — `Land` holds writers,
    // residence, slots, index custody, and batch-metadata preservation, so a Compute-side Parquet write,
    // generation directory, artifact-index stamp, or Flight dial to push bytes forks lake custody the branch
    // settled on one custodian. Tenancy arrives as the frame's own `TenantContext` because the hive tree's
    // `tenant=` segment is what makes a tenant-scoped scan prune rather than answer zero rows.
    public static Fin<LakeLanding> Landing(LakeDataset dataset, TenantContext tenant, Option<MemoryAllocator> allocator) =>
        (dataset.Segment, Declaration(dataset))
            .Apply(static (segment, declaration) => (Segment: segment, Declaration: declaration))
            .As().ToFin()
            .Bind(admitted => Batches(dataset, allocator).Map(batches => new LakeLanding(
                new LakeGeneration(dataset.Arm, tenant, admitted.Segment, dataset.SchemaKeyOf(admitted.Declaration), dataset.ContentKey),
                admitted.Declaration, batches, dataset.Metadata)));

    // Row-major producers fold through the ONE `ArrowLanding.Build` declaration fold; the geometry corpus is
    // already contiguous at its own descriptor offsets, so it wraps its arena and binds pre-built columns. That
    // split is the lakehouse port's own law, and it is the reason the geometry arm never spells a `ColumnCell`:
    // a cell carrier is a ROW value, and folding a tiled channel through one would reinstate exactly the strided
    // copy the kernel's tiling exists to delete.
    public static Fin<Seq<RecordBatch>> Batches(LakeDataset dataset, Option<MemoryAllocator> allocator) =>
        Declaration(dataset).ToFin().Bind(declaration => dataset.Switch(
            state: (Declaration: declaration, Metadata: dataset.Metadata, Allocator: allocator),
            doe: static (s, d) => Doe(s.Declaration, d.Dataset, s.Metadata, s.Allocator).Map(static batch => Seq(batch)),
            geometry: static (s, g) => Geometry(s.Declaration, g.Dataset, s.Metadata, s.Allocator),
            chargeback: static (s, c) => Chargeback(s.Declaration, c.Dataset, s.Metadata, s.Allocator).Map(static batch => Seq(batch))));

    // --- [DECLARATIONS]
    // Every arm's declaration carries the SORT COLUMN its `LandingArm` row names — `run`, `node`, `kind` — because
    // `WriteParquetFrames` proves the arm's sorting columns against the declaration and refuses the generation
    // before a byte lands. The three batches this collapse replaces declared none of the three.
    internal static Validation<Error, AnalyticsSchema> Declaration(LakeDataset dataset) => dataset.Switch(
        doe: static d => DoeDeclaration(d.Dataset),
        geometry: static g => GeometryDeclaration(g.Dataset),
        chargeback: static _ => Success<Error, AnalyticsSchema>(ChargebackDeclaration));

    // Surrogate-training egress: the row-major Coordinates/Responses blocks declare one `Float64` column PER axis
    // and PER objective (the tabular training shape), the front mask one `Bool` column, and `run` the point
    // ordinal the arm sorts and a scan joins back on. The axis vocabulary is DATA, so the declaration derives
    // from it and the field names cannot drift from the columns the fold then emits.
    static Validation<Error, AnalyticsSchema> DoeDeclaration(DoeDataset dataset) {
        Seq<string> labels = dataset.Axes + dataset.Objectives;
        return dataset.Points > 0 && dataset.Axes.Count > 0 && dataset.Objectives.Count > 0
            && labels.ForAll(static label => !string.IsNullOrWhiteSpace(label))
            && labels.Distinct().Count == labels.Count
            && !labels.Exists(static label => label is "run" or "on_front")
                ? labels.Traverse(label => LakeDataset.Admitted(label).Map(name => new ColumnRow(name, ColumnType.Float64, Nullable: false))).As()
                    .Bind(columns => (LakeDataset.Admitted("run"), LakeDataset.Admitted("on_front"))
                        .Apply((run, front) => new AnalyticsSchema(
                            Dataset: "compute.doe",
                            Key: Seq(run),
                            Columns: new ColumnRow(run, ColumnType.Int64, Nullable: false)
                                .Cons(columns).Add(new ColumnRow(front, ColumnType.Bool, Nullable: false)),
                            Time: run, Spine: TimeSpine.Landing, Measure: None))
                        .As())
                : Fail<Error, AnalyticsSchema>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Counts(dataset.Axes.Count, dataset.Objectives.Count, dataset.Points))));
    }

    // Geometry declares one column per DECLARED channel at its own arity and stored width beside the `node` join
    // token and the row ordinal. `FixedList` is the landed shape for a declared-arity run, which is exactly the
    // interleave the kernel's tiling already carries, so the declaration and the arena wrap state one fact.
    static Validation<Error, AnalyticsSchema> GeometryDeclaration(GeometryDataset dataset) =>
        dataset.Instances.IsEmpty
            ? Fail<Error, AnalyticsSchema>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
            : (dataset.Kind.Channels.Traverse(channel => (LakeDataset.Admitted(channel.Key), Lane(channel.Dtype))
                        .Apply((name, lane) => new ColumnRow(name, channel.Arity == 1 ? lane.Column : new ColumnShape.FixedList(lane.Column, channel.Arity), Nullable: false))
                        .As()).As(),
                    LakeDataset.Admitted("node"), LakeDataset.Admitted("ordinal"))
                .Apply((channels, node, ordinal) => new AnalyticsSchema(
                    Dataset: "compute.geometry",
                    Key: Seq(node, ordinal),
                    Columns: channels.Add(new ColumnRow(node, ColumnType.Utf8, Nullable: false))
                        .Add(new ColumnRow(ordinal, ColumnType.Int32, Nullable: false)),
                    Time: node, Spine: TimeSpine.Landing, Measure: None))
                .As();

    // Billing lands LONG on `kind` — the cost-unit axis the `LandingArm.Cost` row sorts by — rather than four
    // WIDE unit columns. NAMED LOSS: a reader taking one row per (tenant, route) now reads four, and the wide
    // shape's implicit "these four lanes and no others" becomes a value the `kind` column carries. That is what
    // the arm's declared sort column demands, and the wide form would have refused at `Ordered` with no `kind`
    // column to point at.
    internal static readonly AnalyticsSchema ChargebackDeclaration = new(
        Dataset: "compute.chargeback",
        Key: Seq(Identifier.Create("tenant"), Identifier.Create("kind")),
        Columns: Seq(
            new ColumnRow(Identifier.Create("tenant"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("route"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("kind"), ColumnType.Utf8, Nullable: false),
            new ColumnRow(Identifier.Create("units"), ColumnType.Float64, Nullable: false),
            new ColumnRow(Identifier.Create("facts"), ColumnType.Int64, Nullable: false),
            new ColumnRow(Identifier.Create("at"), ColumnType.Timestamp, Nullable: false)),
        Time: Identifier.Create("at"), Spine: TimeSpine.Landing, Measure: Some(Identifier.Create("units")));

    // --- [PRODUCERS]

    // One row per design point, its cells in DECLARATION order. The strided gather this replaces ran per column;
    // the declaration fold pivots once, so the scratch here is one row-wide rent rather than one plane per axis.
    static Fin<RecordBatch> Doe(AnalyticsSchema declaration, DoeDataset dataset, Seq<(string Key, string Value)> metadata, Option<MemoryAllocator> allocator) {
        int axes = dataset.Axes.Count, objectives = dataset.Objectives.Count;
        return checked((long)dataset.Points * axes) == dataset.Coordinates.Length
            && checked((long)dataset.Points * objectives) == dataset.Responses.Length
            && dataset.OnFront.Length == dataset.Points
                ? ArrowLanding.Build(declaration, toSeq(Range(0, dataset.Points)), row => Cells(dataset, row, axes, objectives),
                    metadata, allocator.IfNoneUnsafe(() => null!))
                : Fin.Fail<RecordBatch>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Consistent, new ContractEvidence.Counts(dataset.Coordinates.Length, dataset.Responses.Length, dataset.OnFront.Length))));
    }

    static Seq<ColumnCell> Cells(DoeDataset dataset, int row, int axes, int objectives) =>
        new ColumnCell.Whole(row).Cons(
            toSeq(Range(0, axes).Select(lane => (ColumnCell)new ColumnCell.Real(dataset.Coordinates.Span[(row * axes) + lane])))
            + toSeq(Range(0, objectives).Select(lane => (ColumnCell)new ColumnCell.Real(dataset.Responses.Span[(row * objectives) + lane])))
            + Seq<ColumnCell>(new ColumnCell.Flag(dataset.OnFront.Span[row])));

    // Geometry egress is the ZERO-GATHER producer: the kernel already tiled each channel contiguously at its own
    // descriptor offset, so an ArrowBuffer wraps that slice verbatim and a FixedSizeList of the channel's arity
    // states the interleave the arena already carries. A per-component scalar fan-out would re-key the tree on
    // every arity edit AND force the strided copy the kernel's tiling exists to delete. Row grain is the packed
    // ELEMENT — vertex, cell, or path station — so a batch is one instance and `node`/`ordinal` carry the join
    // back to the encode. Both identity columns are wide in memory and near-free on disk (dictionary and RLE
    // encoding), which is the deliberate trade for channel columns that cost no copy at all.
    //
    // Admission runs WHOLE before the first buffer is wrapped, so a refusal frees nothing and the build below is
    // total — a rail faulting mid-construction abandons every column already allocated and `Fin` carries no
    // release arm. `Describes` is the KERNEL's own oracle over declaration versus packed instance, and it
    // ACCUMULATES here: a corpus admission wants every offending instance named, where the abort-on-first walk
    // this replaces reported one and hid the rest.
    static Fin<Seq<RecordBatch>> Geometry(AnalyticsSchema declaration, GeometryDataset dataset, Seq<(string Key, string Value)> metadata, Option<MemoryAllocator> allocator) {
        Schema wire = declaration.Fields(metadata);
        return dataset.Instances
            .Traverse(instance => dataset.Schema.Describes(instance).ToValidation<Error>())
            .As().ToFin()
            .Map(_ => dataset.Instances.Map(instance => Batch(wire, dataset.Kind.Channels, instance, allocator)));
    }

    // One instance, one batch: every channel array borrows the arena slice the kernel already owns, so the only
    // material this fold allocates is the two identity columns. Column order IS the declaration order, so the
    // schema and the array sequence share one declaration and cannot drift apart.
    static RecordBatch Batch(Schema wire, Seq<EncodingChannel> channels, EncodedGeometry instance, Option<MemoryAllocator> allocator) {
        // Root-qualified join token: the witness composite crosses WHOLE, so a scan joining back to the encode
        // never merges a source-rooted and a payload-rooted instance whose digest bytes coincide.
        string node = string.Create(CultureInfo.InvariantCulture, $"{instance.Witness.Root.Key}:{instance.Witness.ContentHash.Value:x32}");
        MemoryAllocator? arena = allocator.IfNoneUnsafe(() => null!);
        Seq<IArrowArray> columns = channels.Map(channel => Wrap(channel, instance)) + Seq<IArrowArray>(
            // Dictionary encoding is the INTENT for a column holding one repeated value per instance, and the
            // landed `ColumnShape.Dictionary` states it; until that shape reaches this arm the run appends once
            // per element, which is the one place this producer copies material it did not have to.
            new StringArray.Builder().Reserve(instance.Count)
                .AppendRange(Enumerable.Repeat(node, instance.Count)).Build(arena),
            new Int32Array.Builder().Reserve(instance.Count)
                .Append(Enumerable.Range(0, instance.Count).ToArray()).Build(arena));
        return new RecordBatch(wire, columns, instance.Count);
    }

    // Every channel slice rides the kernel's own `Channel` reader rather than a re-slice of `Payload`: this arena
    // is MIXED dtype — a mesh patch tiles float32 positions beside float16 curvature — so one width reinterpreted
    // across the whole payload reads its neighbours as garbage. Each wrap keeps the quantized bits its round-trip
    // witness measured, since widening a half or a unorm lane here re-spells values that tolerance proof already
    // certified at their stored width.
    static IArrowArray Wrap(EncodingChannel channel, EncodedGeometry instance) {
        ArenaLane lane = Lane(channel.Dtype).IfFail(_ => throw new UnreachableException());
        ArrowBuffer buffer = new(instance.Channel(channel));
        return channel.Arity == 1
            ? lane.Borrow(buffer, instance.Count)
            : new FixedSizeListArray(new FixedSizeListType(lane.Column.Arrow, channel.Arity), instance.Count,
                lane.Borrow(buffer, checked(instance.Count * channel.Arity)), ArrowBuffer.Empty);
    }

    // ONE kernel-dtype correspondence, TOTAL. The `FrozenDictionary<ChannelDtype, ArenaLane>` this replaces was a
    // hand-kept MIRROR of the kernel roster: a new `ChannelDtype` row landed upstream, passed every census here,
    // and refused the whole corpus at runtime by schema tag. A total `Switch` breaks at COMPILE time instead, and
    // each arm answers with the landed `Query/residence#COLUMN_VOCABULARY` physical row rather than a locally
    // declared Arrow type, so the declaration a generation carries and the buffer it borrows state one fact.
    // The refusing arms are the widths that vocabulary carries no row for — a corpus on one of them refuses at
    // the DECLARATION, where the mirror refused after the schema was already built.
    static Validation<Error, ArenaLane> Lane(ChannelDtype dtype) => dtype.Switch(
        float32:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float32, static (buffer, length) => new FloatArray(buffer, ArrowBuffer.Empty, length, 0, 0))),
        float64:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float64, static (buffer, length) => new DoubleArray(buffer, ArrowBuffer.Empty, length, 0, 0))),
        unorm8:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt8, static (buffer, length) => new UInt8Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        uInt8:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt8, static (buffer, length) => new UInt8Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        int32:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Int32, static (buffer, length) => new Int32Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        uInt32:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt32, static (buffer, length) => new UInt32Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        int64:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Int64, static (buffer, length) => new Int64Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        uInt64:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt64, static (buffer, length) => new UInt64Array(buffer, ArrowBuffer.Empty, length, 0, 0))),
        float16:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float16, static (buffer, length) => new HalfFloatArray(buffer, ArrowBuffer.Empty, length, 0, 0))),
        unorm16:  static row => Unlanded(row),
        int8:     static row => Unlanded(row), int16: static row => Unlanded(row), uInt16: static row => Unlanded(row),
        cInt16:   static row => Unlanded(row), cInt32: static row => Unlanded(row),
        cFloat16: static row => Unlanded(row), cFloat32: static row => Unlanded(row), cFloat64: static row => Unlanded(row));

    static Validation<Error, ArenaLane> Unlanded(ChannelDtype dtype) =>
        Fail<Error, ArenaLane>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Key(dtype.Key))));

    // Billing egress folds the same declaration surface: one row per (tenant, route, cost-unit kind), the unit
    // magnitude as the measure column, facts beside it, and the window as metadata. CONSTRAINT: `route` declares
    // NON-NULL by DOMAIN — a process-scoped row carries the ledger owner's declared `RouteKey` sentinel category,
    // so the column is total and the Persistence `ColumnCell.Absent` arm (which exists for genuinely nullable
    // columns) is deliberately NOT this seam's absence regime; one seam, one regime.
    static Fin<RecordBatch> Chargeback(AnalyticsSchema declaration, ChargebackDataset dataset, Seq<(string Key, string Value)> metadata, Option<MemoryAllocator> allocator) =>
        dataset.Rows.Bind(row => Units(row).Map(unit => (Row: row, Unit: unit)))
            // A process-scoped row lands under the ledger owner's DECLARED `RouteKey` sentinel key — the
            // route column is total by the owner's own fold, so the former route-absent refusal arm is dead.
            .Map(pair => Seq<ColumnCell>(
                new ColumnCell.Text(pair.Row.Tenant.Slug),
                new ColumnCell.Text(pair.Row.RouteKey),
                new ColumnCell.Text(pair.Unit.Kind),
                new ColumnCell.Real(pair.Unit.Value),
                new ColumnCell.Whole(pair.Row.Facts),
                new ColumnCell.Moment(dataset.WindowEnd)))
            .Bind(rows => ArrowLanding.Build(declaration, rows, static cells => cells, metadata, allocator.IfNoneUnsafe(() => null!)));

    // The four metering lanes as (kind, value) rows — one place names the vocabulary, so a lane added at the
    // ledger owner lands here as one row rather than a fifth column every reader re-positions on.
    static Seq<(string Kind, double Value)> Units(ChargebackRow row) => Seq(
        ("elapsed", row.Vector.ElapsedUnits),
        ("token", row.Vector.TokenUnits),
        ("byte", row.Vector.ByteUnits),
        ("remote", row.Vector.RemoteUnits));
}

// One physical lane per kernel quantization row: the landed `ColumnType` the column DECLARES beside the borrow
// that wraps an arena slice at that width. The borrow is a delegate column rather than a generic `new()` bound
// because the builder families are foreign sealed types with no shared constructible contract, and a
// constructed-generic factory over them lowers to the activator form this stack rejects. No span enters or leaves
// the delegate — the buffer is `ReadOnlyMemory`-backed, so nothing stack-only crosses a lambda seam.
public readonly record struct ArenaLane(ColumnType Column, Func<ArrowBuffer, int, IArrowArray> Borrow);
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
