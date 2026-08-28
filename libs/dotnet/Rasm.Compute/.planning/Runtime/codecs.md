# [COMPUTE_CODECS]

Rasm.Compute owns the compute-and-transport half of artifact interchange: the structural geometry delta that lays a changed mesh, B-rep, point cloud, or NURBS net down as touched chunks alone, the content-addressed identity every codec on the branch keys on, and the columnar projection that hands the Persistence lake custodian a declared, self-describing generation.

`DeltaCodec` owns the FastCDC-chunked structural diff and the shared `Quantization` bit-budget law both it and `Runtime/field` compose; `CanonicalForm`/`InterchangeIdentity` own the canonical byte form, the policy-seeded interchange cache key, the HLC compose, and the ONE object-plane address grammar; `ArrowBatch` owns the columnar dataset projection and the one `Landing` the custodian's `FlatTableEgress.Land` takes. The chunked-field codecs live at `Runtime/field`, the HDF5 session at `Runtime/archive`, and the companion hop and tile partition at `Runtime/tiles`.

GLB geometry-content identity composes the kernel seed-zero `XxHash128` `GeometryHash` here, never re-minted with a policy seed.

## [01]-[INDEX]

- [02]-[GEOMETRY_DELTA]: FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; the shared quantization kernel; progressive ordering as a row.
- [03]-[CONTENT_ADDRESSING]: policy-seeded canonical-form `XxHash128` interchange-cache key (the GLB geometry-content identity is the kernel seed-zero `GeometryHash` composed, distinct); the empty-artifact sentinel; the HLC two-half compose; the one object-plane address grammar.
- [04]-[ARROW_BATCH]: `Solver/sweep` `DoeDataset`, `Runtime/ledger` `ChargebackDataset`, and the `GeometryDataset` kernel-encode corpus project into self-describing Arrow batches under ONE `LakeDataset` family — the row-major pair folding through the Persistence declaration fold, the geometry corpus borrowing its arena verbatim, and one `Landing` handing the custodian everything `Land` takes.

## [02]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` the structural-diff target rows, each carrying its whole normalization law as a behavior column; `ChunkOrder` `[SmartEnum<string>]` the transmission-ordering rows carrying their own comparator, so the progressive posture is a value the delta records rather than a bool the codec re-branches on; `DeltaPolicy` the `[ComplexValueObject]` chunk policy whose factory admits once, so the two ends that used to re-prove it on every `Diff` and every `Apply` read an already-admitted value; `DeltaChunk`/`GeometryDelta` the content-addressed delta records; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission; `Quantization` the shared codec quantization law this lane and `Runtime/field` both compose.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control; `ChunkOrder` rows `Sequential` (recipe order) and `Progressive` (largest-first, so a transmission renders coarse coverage before fine detail).
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the ordered target chunk recipe (`TargetChunks`) with the new-chunk payload (`Added`, hashes absent from the base); `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` walks the recipe and reconstructs the NORMALIZED target exactly, pulling each chunk from the payload or the re-chunked base — `TargetHash` is taken over the normalized bytes, so the verify proves the reconstruction bit-for-bit and `GeometryDelta.GeometricError` states the residual that separates it from the caller's original target; `Fin<T>` aborts on float alignment, base or target hash mismatch, corrupt payload framing, and an unresolved recipe hash.
- Auto: `Diff` first runs each kind's row-owned `Normalize` — the float-column kinds (vertex/point) round every float to the finer of the bit-budget grid and `Tolerance` so a sub-tolerance perturbation hashes to one chunk, bounded-lossy within `Tolerance`; topology and B-rep-face streams pass verbatim; the `nurbs-control` parametric stream rounds its control-net coordinate block alone, knots and weights crossing verbatim — then runs FastCDC over the normalized bytes — a 256-entry SplitMix64 `Gear` table rolls the fingerprint, a STRICT mask below `AvgChunk` and a LOOSE mask above normalize the chunk-size distribution so an inserted vertex shifts only its local chunk; `TargetChunks` records the ordered hash recipe, `Added` the distinct new chunks laid out under the policy's own `ChunkOrder` row, and the delta's own `GeometricError` the quantization step every one of them was rounded to (zero on a kind that passed verbatim), so the residual is stated once rather than restated per chunk; the delta carries its own `DeltaPolicy` so `Apply` re-chunks the base identically and round-trips deterministically. Every content digest on this lane rides the kernel `ContentHash.Of` seed-zero entry, so the delta's base and target identities are the SAME key the Persistence blob lane and the `Rasm/Domain/identity` owner mint.
- Output: `GeometryDelta` carries the delta content key, the changed-chunk count, `BaseBytes`, and `DeltaBytes`, and `Ratio` is the derived figure a board spells, so a structural diff's compression is auditable off the value; a progressive transmission carries the `ChunkOrder` row's key.
- Packages: System.IO.Hashing, System.Numerics.Tensors, LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (project — `ContentHash.Of` the ONE seed-zero identity entry, `Deterministic` the splitmix64 owner, and the kernel reconciliation `EncodeForm.Parametric` canonical stream the `nurbs-control` payload rides), Rasm.Persistence (project), BCL inbox (`System.Numerics.BitOperations` mask sizing)
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row carrying its row-owned `Normalize` law; a new transmission posture is one `ChunkOrder` row carrying its comparator; a new chunk policy column is one field on `DeltaPolicy` the factory admits beside its siblings; zero new surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ChunkOrder {
    public static readonly ChunkOrder Sequential = new("sequential", static added => added);
    public static readonly ChunkOrder Progressive = new("progressive", static added => toSeq(added.OrderByDescending(static chunk => chunk.ByteLength)));

    [UseDelegateFromConstructor]
    internal partial Seq<DeltaChunk> Layout(Seq<DeltaChunk> added);
}

// --- [MODELS] --------------------------------------------------------------------------
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
    public double Ratio => BaseBytes > 0L ? (double)DeltaBytes / BaseBytes : 0d;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Quantization {
    public static (T Scale, T Step) Steps<T>(ReadOnlySpan<T> source, int bits) where T : IFloatingPointIeee754<T> {
        T scale = T.Abs(TensorPrimitives.MaxMagnitude(source));
        int levels = (1 << bits) - 1;
        return (scale, levels > 0 ? scale / T.CreateChecked(levels) : T.Zero);
    }

    public static T Code<T>(T value, T step) where T : IFloatingPointIeee754<T> =>
        step == T.Zero ? value : T.Round(value / step) * step;

    public static void Code<T>(ReadOnlySpan<T> source, Span<T> destination, T step) where T : IFloatingPointIeee754<T> {
        if (step == T.Zero) { source.CopyTo(destination); return; }
        TensorPrimitives.Divide(source, step, destination);
        TensorPrimitives.Round(destination, destination);
        TensorPrimitives.Multiply(destination, step, destination);
    }

    public static double Residual(float value, float coded, float scale) => scale == 0f ? 0.0 : Math.Abs(value - coded) / scale;

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

    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        Try.lift(() => ParametricNet(bytes, policy)).Run().Bind(static inner => inner);

    static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> ParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        ReadOnlySpan<byte> stream = bytes.Span;
        int directions = BinaryPrimitives.ReadInt32LittleEndian(stream);
        int cursor = sizeof(int);
        for (int direction = 0; direction < directions; direction++) {
            int knots = BinaryPrimitives.ReadInt32LittleEndian(stream[(cursor + sizeof(int))..]);
            cursor += (sizeof(int) * 2) + (knots * sizeof(double));
        }
        int weights = BinaryPrimitives.ReadInt32LittleEndian(stream[cursor..]);
        cursor += sizeof(int) + (weights * sizeof(double));
        int controls = BinaryPrimitives.ReadInt32LittleEndian(stream[cursor..]);
        int netOffset = cursor + sizeof(int);
        if (directions < 1 || weights != controls || netOffset + (controls * 3 * sizeof(double)) != stream.Length) {
            return Fin.Fail<(ReadOnlyMemory<byte>, double)>(new ComputeFault.Violation(ComputeArea.Runtime,
                new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(stream.Length, netOffset + (controls * 3 * sizeof(double))))));
        }
        if (policy.QuantizationBits <= 0) { return Fin.Succ(((ReadOnlyMemory<byte>)bytes, 0d)); }
        byte[] normalized = stream.ToArray();
        Span<double> net = MemoryMarshal.Cast<byte, double>(normalized.AsSpan(netOffset));
        double step = GridStep<double>(net, policy);
        Quantization.Code<double>(net, net, step);
        return Fin.Succ(((ReadOnlyMemory<byte>)normalized, step));
    }

    static T GridStep<T>(ReadOnlySpan<T> source, DeltaPolicy policy) where T : IFloatingPointIeee754<T> {
        T bitStep = Quantization.Steps(source, policy.QuantizationBits).Step;
        T tolerance = T.CreateChecked(policy.Tolerance);
        return bitStep <= T.Zero ? tolerance : T.Min(bitStep, tolerance);
    }

    // --- [FAST_CDC]

    static Seq<DeltaChunk> FastCdc(ReadOnlySpan<byte> data, DeltaPolicy policy) {
        Seq<DeltaChunk> reversed = Seq<DeltaChunk>();
        int start = 0, ordinal = 0;
        while (start < data.Length) {
            int cut = ContentDefinedCut(data[start..], policy);
            reversed = reversed.Cons(new DeltaChunk(ContentHash.Of(data.Slice(start, cut)), ordinal++, start, cut));
            start += cut;
        }
        return reversed.Reverse();
    }

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
    private const int ChunkHeader = 16 + (sizeof(int) * 2);

    static ReadOnlyMemory<byte> Concatenate(Seq<DeltaChunk> added, ReadOnlyMemory<byte> targetBytes) {
        int total = added.Sum(static c => c.ByteLength + ChunkHeader);
        byte[] buffer = new byte[total];
        Span<byte> sink = buffer.AsSpan();
        int cursor = 0;
        foreach (DeltaChunk chunk in added) {
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
                    .Fold(Fin.Succ(Seq<ReadOnlyMemory<byte>>()), (acc, hash) => acc.Bind(pieces =>
                        (addedByHash.Find(hash) | baseByHash.Find(hash))
                            .Map(pieces.Add)
                            .ToFin(new ComputeFault.CacheCorrupt($"<delta-chunk-missing:{hash:x32}>"))))
                    .Map(pieces => {
                        byte[] target = new byte[pieces.Sum(static piece => piece.Length)];
                        int written = pieces.Fold(0, (cursor, piece) => { piece.Span.CopyTo(target.AsSpan(cursor)); return cursor + piece.Length; });
                        return (ReadOnlyMemory<byte>)target.AsMemory(0, written);
                    });
            }));

    static Fin<HashMap<UInt128, ReadOnlyMemory<byte>>> SplitPayload(ReadOnlyMemory<byte> payload) {
        Fin<HashMap<UInt128, ReadOnlyMemory<byte>>> map = HashMap<UInt128, ReadOnlyMemory<byte>>();
        int cursor = 0;
        while (map.IsSucc && cursor < payload.Length) {
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

- Owner: `CanonicalForm` the tag-folding kernel every keyed format or codec tag passes before it enters a preimage; `InterchangeIdentity` the interchange CACHE-PARTITION key derivation folding the canonical tag, the complete ordered output-policy vector, and the source bytes into ONE kernel `ContentHash.Of` preimage (distinct from the kernel seed-zero `GeometryHash` the contract/Bim/Persistence/peers share — distinct by PREIMAGE, never by seed), mirroring the model-lane `ModelIdentity.Snapshot` precedent, with `Compose` sealing the content key and HLC two-half stamp into one frame key, `SeedZero` minting the absent-artifact identity, and `Address` owning the ONE object-plane address grammar every Compute artifact-id spells; `ComputeArtifact` the emitted-bytes carrier the field, tile, and Bim export pipelines feed, landing content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(...)` — pure value; the contiguous and pooled-sequence cases derive identity from the canonical tag, the policy vector, and the payload bytes, while the geometry case frames the kernel arena's own witness digest WHOLE — the `DigestRoot` ordinal ahead of `ContentHash`, the kernel `RoundTripWitness.Root` dedup law read into the preimage — beside its descriptor roster and the index column before one streaming fold; `public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical)` folds the content key with the causal stamp in the fixed (physical, logical) half order; `public static UInt128 SeedZero(string formatKey, ReadOnlySpan<double> policy)` is the absent-artifact identity; `public static UInt128 Schema(AnalyticsSchema declaration)` frames a declared column roster injectively into the schema identity a landed generation keys on; `public static string Address(UInt128 contentKey, string kind)` is the ONE `<content-key:x32>:<kind>` object-plane spelling; `ComputeArtifact.Of` is the one emit-carrier mint deriving the content key from bytes with its complete policy vector, discriminating on the payload's own shape — a contiguous `ReadOnlyMemory<byte>` or a pooled `ReadOnlySequence<byte>` whose key folds segment by segment before any contiguity is demanded — and `ByteCount` derives off the carried payload rather than travelling as a second stored column a caller contradicts.
- Auto: every keyed input rides the kernel `CanonicalWriter` — `CanonicalForm.Tag` lower-cases invariant culture and trims the format/codec tag so `"GLB"` and `" glb "` key one identity, the writer's `String` length-frames it, `Doubles` count-frames the policy vector and canonicalizes every NaN payload and `-0.0` before the bits leave, the presence byte (`Optional`) separates an absent artifact from a present-but-empty one, and the payload rides `Raw` as the whole-payload leaf the writer's exemption names — injective framing, so distinct `(formatKey, policy, bytes)` triples never share a preimage. Tessellation folds deflection, tolerance, angle tolerance, tile depth, root geometric error, and split threshold; field storage folds bits and bound. `Admit` projects onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns, addressing the row through the same `Address` grammar every companion request answers.
- Law: a stored artifact carries the `ArtifactIndexRow` checksum and byte size; an absent-keyed artifact stamps the `SeedZero` identity so an absent-versus-empty distinction is auditable.
- Packages: NodaTime, LanguageExt.Core, Rasm (project — the kernel `EncodedGeometry` arena the geometry key frames, `ContentHash.Of`/`Hex`, `CanonicalWriter.String`/`Doubles`/`Optional`/`Raw`/`Rows`/`Ordinal`/`I64`/`U128`), Rasm.Persistence (project — `ArtifactIndexRow.Admit`, `Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema` the schema key frames), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one canonical scalar in the policy vector; a new keyed-input kind is one `InterchangeIdentity.Key` arity whose preimage states its own fields; a new per-vertex lane is a kernel `EncodingChannel` row the geometry key absorbs through the descriptor roster with no edit here; zero new surface.
- Law: ONE alphabet. Every preimage on this page is `ContentHash.Of(state, (s, w) => ...)` over the kernel writer — no seeded accumulator, no local length-prefix writer, no `XxHash3` seed folded from a tag. NAMED LOSS: the policy-seeded `XxHash128(seed)` form and the `CanonicalForm.Write`/`Seed`/`Scalar` trio are retired, so a cache key minted before this change re-keys once. Witness: `Key(formatKey, bytes, policy)` folds the same three facts in the same owner order, the policy now a framed field stream instead of a computed seed.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    Instant At) {
    public long ByteCount => Bytes.Length;

    public string ArtifactKey => InterchangeIdentity.Address(ContentKey, FormatKey);

    public static ComputeArtifact Of(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, ReadOnlyMemory<double> policy = default) =>
        new(formatKey, bytes, InterchangeIdentity.Key(formatKey, bytes, policy), at);

    public static ComputeArtifact Of(string formatKey, ReadOnlySequence<byte> bytes, Instant at, ReadOnlyMemory<double> policy = default) {
        byte[] owned = new byte[checked((int)bytes.Length)];
        bytes.CopyTo(owned);
        return new(formatKey, owned, InterchangeIdentity.Key(formatKey, bytes, policy), at);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanonicalForm {
    public static string Tag(string raw) => raw.Trim().ToLowerInvariant();
}

public static class InterchangeIdentity {
    public static string Address(UInt128 contentKey, string kind) =>
        string.Create(CultureInfo.InvariantCulture, $"{ContentHash.Hex(contentKey)}:{CanonicalForm.Tag(kind)}");

    static CanonicalWriter Head(CanonicalWriter w, string formatKey, ReadOnlySpan<double> policy, bool present) =>
        w.String(CanonicalForm.Tag(formatKey)).Doubles(policy).Bool(present);

    public static UInt128 Key(string formatKey, ReadOnlyMemory<byte> bytes, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, bytes, policy), static (s, w) => Head(w, s.formatKey, s.policy.Span, present: true).Raw(s.bytes.Span));

    public static UInt128 Key(string formatKey, ReadOnlySequence<byte> bytes, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, bytes, policy), static (s, w) => {
            Head(w, s.formatKey, s.policy.Span, present: true);
            foreach (ReadOnlyMemory<byte> segment in s.bytes) { w.Raw(segment.Span); }
        });

    public static UInt128 Key(string formatKey, EncodedGeometry lanes, ReadOnlyMemory<byte> indices, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, lanes, indices, policy), static (s, w) =>
            Head(w, s.formatKey, s.policy.Span, present: !(s.lanes.Descriptors.IsEmpty && s.indices.IsEmpty))
                .Ordinal(s.lanes.Witness.Root.Key)
                .U128((UInt128)s.lanes.Witness.ContentHash)
                .I64(s.lanes.Count)
                .Rows(s.lanes.Descriptors, static (d, x) => x.String(d.Channel.Key).Ordinal(d.Channel.Dtype.Key))
                .Ordinal(s.indices.Length)
                .Raw(s.indices.Span));

    public static UInt128 Schema(AnalyticsSchema declaration) =>
        ContentHash.Of(declaration, static (d, w) => w.String(d.Dataset)
            .Rows(d.Columns, static (column, x) => x.String((string)column.Name).String(column.Type.ToString())));

    public static UInt128 SeedZero(string formatKey, ReadOnlyMemory<double> policy) =>
        ContentHash.Of((formatKey, policy), static (s, w) => Head(w, s.formatKey, s.policy.Span, present: false));

    public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical) =>
        ContentHash.Of((contentKey, physical, logical), static (s, w) =>
            w.U128(s.contentKey).I64(s.physical.ToUnixTimeTicks()).I64(unchecked((long)s.logical)));

    public static ArtifactIndexRow Admit(ComputeArtifact artifact, DataClassification classification, Option<UInt128> sourceKey) =>
        ArtifactIndexRow.Admit(ArtifactKind.Interchange, artifact.ArtifactKey, artifact.Bytes.Span, classification, artifact.At, sourceKey);
}
```

## [04]-[ARROW_BATCH]

- Owner: `LakeDataset` — the ONE lake-bound producer family, its three cases (`Doe`, `Geometry`, `Chargeback`) sharing one identity regime, one landing port, one metadata contract, and one consumer, so the arm row, the declared schema, the content key, the readable segment, and the required metadata are COLUMNS on the family rather than three parallel builders and three overloads that agreed only by inspection; `ArrowBatch` the columnar-construction surface projecting each case into self-describing `Apache.Arrow` batches; `GeometryDataset` the lake-bound corpus pairing one `PackKind` with its model segment and encoded instances, deriving both its schema identity and its generation key from content; `LakeLanding` the exact quadruple `FlatTableEgress.Land` takes. Core `Apache.Arrow` is the sole reference: the IPC writer, the LZ4/Zstd `CompressionCodecFactory`, the ADBC query surface, and the Flight-SQL transport are the Persistence egress pipelines, absent from the Compute closure.
- Cases: `Doe` the `Solver/sweep` design-of-experiments corpus (`LandingArm.Doe`, `study=` segment, `run` sort column); `Geometry` the kernel-encode corpus (`LandingArm.Geometry`, `model=` segment, `node` sort column, keying its schema off the kernel's own `PackSchema.SchemaId`); `Chargeback` the `Runtime/ledger` billing corpus (`LandingArm.Cost`, `month=` segment, `kind` sort column).
- Entry: `public static Fin<LakeLanding> Landing(LakeDataset dataset, TenantContext tenant, Option<MemoryAllocator> allocator)` is the ONE lake-landing projection — a total `Switch` sealing the case's batches, naming its `LandingArm` row and the readable segment that arm's hive key spells, and deriving the generation coordinate the custodian writes under; `public static Fin<Seq<RecordBatch>> Batches(LakeDataset dataset, Option<MemoryAllocator> allocator)` is the projection half a caller redeeming batches alone takes.
- Auto: the row-major cases fold through the Persistence `Query/backend#COLUMN_VOCABULARY` `ArrowLanding.Build<TRow>` — the declaration supplies the field list, its order, and every column's own builder, and the conformance proof ACCUMULATES across columns, so a producer handing one batch learns every offending column at once; the pivot from rows to columns happens inside that fold, so no producer arm re-spells a strided gather. The geometry arm gathers NOTHING — the kernel already tiled each channel contiguously at its own descriptor offset, so an `ArrowBuffer` borrows that slice and a `FixedSizeListType` of the channel's arity states the interleave already in the bytes, leaving the two identity columns as the only material a landing allocates; that arm binds pre-built columns through the metadata-bearing `RecordBatch` constructor against the SAME `AnalyticsSchema` declaration, so both paths derive one field order from one declaration. Metadata is REQUIRED at every arm and defaulted nowhere: `Schema.Builder` and `RecordBatch.Builder` expose no metadata seat, so a batch reaching the custodian with none states no content key, no window, and no strategy, and the arm's own `Metadata` column is what the fold carries.
- Law: each batch is a projection of a standing dataset shape, and the landed generation's evidence rides the custodian's own `LandingArm.Slot`, never a second Compute row; the geometry corpus carries the kernel `RoundTripWitness` per instance, so quantization evidence is already proved upstream and no landing re-measures it.
- Packages: Apache.Arrow, NodaTime (`InstantPattern.ExtendedIso` the metadata instant, `LocalDatePattern.CreateWithInvariantCulture` over `Instant.InUtc().Date` the billing-month segment), Thinktecture.Runtime.Extensions (`DoeDesign`/`Substrate` `.Key`), System.IO.Hashing, Rasm.Persistence (project — `Query/lakehouse#FLAT_TABLE_EGRESS` `LandingArm`/`LakeGeneration`/`FlatTableEgress.Land`, `Query/backend#COLUMN_VOCABULARY` `AnalyticsSchema`/`ColumnRow`/`ColumnShape`/`ColumnType`/`ColumnCell`/`TimeSpine`/`ArrowLanding.Build`/`Identifier`), Rasm (project — `TenantContext`, `ContentHash.Of`, `CanonicalWriter.U128`/`Rows`/`Ordinal`, and the `Rasm.Drawing` encode wire `EncodedGeometry`/`PackKind`/`PackSchema`/`EncodingChannel`/`ChannelDtype`), LanguageExt.Core, CommunityToolkit.HighPerformance (`MemoryOwner<double>` the strided scratch), BCL inbox (`CultureInfo.InvariantCulture`)
- Growth: a new dataset producer is one `LakeDataset` case answering the family's five columns — arm, declaration, content key, segment, metadata — and the `Landing` `Switch` then breaks LOUDLY until it does, where the three-overload form this collapse deletes admitted a fourth producer as two new public surfaces the custodian's own `LandingArm` roster never learned about; a new column is one `ColumnRow` on the declaration and one `ColumnCell` in the row fold; a new kernel `EncodingChannel` or `PackKind` reaches the geometry columns with ZERO edit here because the kind's own declared active set generates the schema; the `MemoryAllocator` injects per lane so a staging-bounded arena charges the batch buffers against the lane budget.
- Boundary: Compute BUILDS the columnar table; the Persistence lakehouse OWNS everything that CARRIES it — `ArrowStreamWriter`/`ArrowFileWriter` IPC, the `Apache.Arrow.Compression` LZ4/Zstd codec, the ADBC query surface, and the `FlightClient`/`FlightSqlClient` — so Compute holds one core `Apache.Arrow` reference, references none of the four egress packages, and opens no Flight listener; the DECLARATION is the contract, so this page hands `Land` an `AnalyticsSchema` and never a hand-built `Schema` beside a batch that agrees with it only by inspection, and each arm's declared columns include the SORT COLUMN its `LandingArm` row names — a generation whose declaration omits that column refuses at `Ordered` before a byte is written; a bare `DateTime` where the NodaTime instant crosses, the shared `MemoryAllocator.Default` where a lane arena is available, a per-element `Append(T)` loop where a span append exists, and a hand-rolled columnar byte layout `RecordBatch` already owns are the rejected forms; the geometry arm adds three of its own — a per-component scalar fan-out of an arity-3 channel, which re-keys the tree on every arity edit and reinstates the strided copy the kernel's tiling deletes; a half or unorm lane widened to float at the wrap, which re-spells values the round-trip witness certified at their stored width; and a schema key re-digested off the Arrow field list, which keys the hive tree on a projection the kernel never published while `PackSchema.SchemaId` is the identity the custodian's geometry row names by law; the sealed `RecordBatch` stops at the Compute edge — `Landing` hands the custodian a `LakeGeneration` coordinate, its declaration, its batches, and its metadata, and `FlatTableEgress.Land` writes them, so byte framing exists only where the `topology` axis puts the custodian in another process and the composition root frames it there through the Persistence IPC writer, never here.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeometryDataset(PackKind Kind, string Model, Seq<EncodedGeometry> Instances) {
    public PackSchema Schema => PackSchema.Of(Kind);

    public UInt128 ContentKey =>
        ContentHash.Of(this, static (d, w) => w.U128(d.Schema.SchemaId)
            .Rows(d.Instances, static (instance, x) => x.Ordinal(instance.Witness.Root.Key).U128((UInt128)instance.Witness.ContentHash)));
}

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

    public UInt128 SchemaKeyOf(AnalyticsSchema declaration) => Switch(
        state: declaration,
        doe: static (d, _) => InterchangeIdentity.Schema(d),
        geometry: static (_, g) => g.Dataset.Schema.SchemaId,
        chargeback: static (d, _) => InterchangeIdentity.Schema(d));

    public Validation<Error, Identifier> Segment => Switch(
        doe: static d => Admitted(d.Dataset.Strategy.Key),
        geometry: static g => Admitted(g.Dataset.Model),
        chargeback: static c => Admitted(MonthSegment.Format(c.Dataset.WindowStart.InUtc().Date)));

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

    internal static Validation<Error, Identifier> Admitted(string raw) =>
        FactoryBridge.Accept<Identifier>(raw.Replace('-', '_')).ToValidation();
}

public readonly record struct LakeLanding(
    LakeGeneration Generation, AnalyticsSchema Declaration, Seq<RecordBatch> Batches, Seq<(string Key, string Value)> Metadata);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArrowBatch {
    public static Fin<LakeLanding> Landing(LakeDataset dataset, TenantContext tenant, Option<MemoryAllocator> allocator) =>
        (dataset.Segment, Declaration(dataset))
            .Apply(static (segment, declaration) => (Segment: segment, Declaration: declaration))
            .As().ToFin()
            .Bind(admitted => Batches(dataset, allocator).Map(batches => new LakeLanding(
                new LakeGeneration(dataset.Arm, tenant, admitted.Segment, dataset.SchemaKeyOf(admitted.Declaration), dataset.ContentKey),
                admitted.Declaration, batches, dataset.Metadata)));

    public static Fin<Seq<RecordBatch>> Batches(LakeDataset dataset, Option<MemoryAllocator> allocator) =>
        Declaration(dataset).ToFin().Bind(declaration => dataset.Switch(
            state: (Declaration: declaration, Metadata: dataset.Metadata, Allocator: allocator),
            doe: static (s, d) => Doe(s.Declaration, d.Dataset, s.Metadata, s.Allocator).Map(static batch => Seq(batch)),
            geometry: static (s, g) => Geometry(s.Declaration, g.Dataset, s.Metadata, s.Allocator),
            chargeback: static (s, c) => Chargeback(s.Declaration, c.Dataset, s.Metadata, s.Allocator).Map(static batch => Seq(batch))));

    // --- [DECLARATIONS]
    internal static Validation<Error, AnalyticsSchema> Declaration(LakeDataset dataset) => dataset.Switch(
        doe: static d => DoeDeclaration(d.Dataset),
        geometry: static g => GeometryDeclaration(g.Dataset),
        chargeback: static _ => Success<Error, AnalyticsSchema>(ChargebackDeclaration));

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

    static Fin<Seq<RecordBatch>> Geometry(AnalyticsSchema declaration, GeometryDataset dataset, Seq<(string Key, string Value)> metadata, Option<MemoryAllocator> allocator) {
        Schema wire = declaration.Fields(metadata);
        return dataset.Instances
            .Traverse(instance => dataset.Schema.Describes(instance).ToValidation<Error>())
            .As().ToFin()
            .Bind(_ => dataset.Instances.Traverse(instance => Batch(wire, dataset.Kind.Channels, instance, allocator)).As());
    }

    static Fin<RecordBatch> Batch(Schema wire, Seq<EncodingChannel> channels, EncodedGeometry instance, Option<MemoryAllocator> allocator) {
        string node = string.Create(CultureInfo.InvariantCulture, $"{instance.Witness.Root.Key}:{(UInt128)instance.Witness.ContentHash:x32}");
        MemoryAllocator? arena = allocator.IfNoneUnsafe(() => null!);
        return channels.Traverse(channel => Wrap(channel, instance)).As().Map(mapped => new RecordBatch(
            wire,
            mapped + Seq<IArrowArray>(
                new StringArray.Builder().Reserve(instance.Count)
                    .AppendRange(Enumerable.Repeat(node, instance.Count)).Build(arena),
                new Int32Array.Builder().Reserve(instance.Count)
                    .Append(Enumerable.Range(0, instance.Count).ToArray()).Build(arena)),
            instance.Count));
    }

    static Fin<IArrowArray> Wrap(EncodingChannel channel, EncodedGeometry instance) =>
        Lane(channel.Dtype).Map(lane => new ArrowBuffer(instance.Channel(channel)) is var buffer && channel.Arity == 1
            ? lane.Borrow(buffer, instance.Count)
            : new FixedSizeListArray(new FixedSizeListType(lane.Column.Arrow, channel.Arity), instance.Count,
                lane.Borrow(buffer, checked(instance.Count * channel.Arity)), ArrowBuffer.Empty));

    static Validation<Error, ArenaLane> Lane(ChannelDtype dtype) => dtype.Switch(
        float32:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float32, static length => new FloatArray(ArrowBuffer.Empty, length, 0, 0))),
        float64:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float64, static length => new DoubleArray(ArrowBuffer.Empty, length, 0, 0))),
        unorm8:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt8, static length => new UInt8Array(ArrowBuffer.Empty, length, 0, 0))),
        uInt8:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt8, static length => new UInt8Array(ArrowBuffer.Empty, length, 0, 0))),
        int32:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Int32, static length => new Int32Array(ArrowBuffer.Empty, length, 0, 0))),
        uInt32:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt32, static length => new UInt32Array(ArrowBuffer.Empty, length, 0, 0))),
        int64:    static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Int64, static length => new Int64Array(ArrowBuffer.Empty, length, 0, 0))),
        uInt64:   static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.UInt64, static length => new UInt64Array(ArrowBuffer.Empty, length, 0, 0))),
        float16:  static _ => Success<Error, ArenaLane>(new ArenaLane(ColumnType.Float16, static length => new HalfFloatArray(ArrowBuffer.Empty, length, 0, 0))),
        unorm16:  static row => Unlanded(row),
        int8:     static row => Unlanded(row), int16: static row => Unlanded(row), uInt16: static row => Unlanded(row),
        cInt16:   static row => Unlanded(row), cInt32: static row => Unlanded(row),
        cFloat16: static row => Unlanded(row), cFloat32: static row => Unlanded(row), cFloat64: static row => Unlanded(row));

    static Validation<Error, ArenaLane> Unlanded(ChannelDtype dtype) =>
        Fail<Error, ArenaLane>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.Key(dtype.Key))));

    static Fin<RecordBatch> Chargeback(AnalyticsSchema declaration, ChargebackDataset dataset, Seq<(string Key, string Value)> metadata, Option<MemoryAllocator> allocator) =>
        dataset.Rows.Bind(row => Units(row).Map(unit => (Row: row, Unit: unit)))
            .Map(pair => Seq<ColumnCell>(
                new ColumnCell.Text(pair.Row.Tenant.Slug),
                new ColumnCell.Text(pair.Row.RouteKey),
                new ColumnCell.Text(pair.Unit.Kind),
                new ColumnCell.Real(pair.Unit.Value),
                new ColumnCell.Whole(pair.Row.Facts),
                new ColumnCell.Moment(dataset.WindowEnd)))
            .Bind(rows => ArrowLanding.Build(declaration, rows, static cells => cells, metadata, allocator.IfNoneUnsafe(() => null!)));

    static Seq<(string Kind, double Value)> Units(ChargebackRow row) => Seq(
        ("elapsed", row.Vector.ElapsedUnits),
        ("token", row.Vector.TokenUnits),
        ("byte", row.Vector.ByteUnits),
        ("remote", row.Vector.RemoteUnits));
}

public readonly record struct ArenaLane(ColumnType Column, Func<ArrowBuffer, int, IArrowArray> Borrow);
```
