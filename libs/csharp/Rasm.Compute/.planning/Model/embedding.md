# [COMPUTE_EMBEDDING]

`Rasm.Compute`'s embedding lane is the retrieval half of the inference spine: it owns ONE encoding axis (`VectorEncoding`), ONE metric axis (`VectorScore`), ONE content-keyed carrier (`EmbeddingVector`), and ONE `Encode`/`Score`/`Rank` fold over the `System.Numerics.Tensors` `TensorPrimitives` SIMD surface. It consumes the L2-normalized `float[]` the `Model/inference#INFERENCE_MODES` `Embed` run produces — the mean-pool/CLS-slice + `TensorPrimitives.Norm`/`Divide` projection is `Embed`'s and is never re-implemented here — and projects that unit vector down the `VectorEncoding` axis: `float32` is the exact-rerank ground truth, `float16` is the `ConvertToHalf` 2×-smaller high-accuracy residence that widens back to near-`float32` fidelity, `int8-scalar` is the `ConvertSaturating` 4×-smaller index residence that still carries magnitude, `binary-hamming` is the 32×-smaller sign-packed coarse gate, and `product-quantized` is the codebook-coded residence for the largest corpora. Every artifact content-keys through `System.IO.Hashing` `XxHash128` over the source `ModelIdentity.Key`, the encoding key, the float dimension, the codebook identity, and the **encoded** component bytes, so a deterministic re-encode of an identical vector at an identical posture addresses the same content key the `Rasm.Persistence` vector index dedups against and a posture change can never alias two distinct artifacts onto one key.

The crossing to Persistence is content-key reference ONLY. Persistence owns the on-disk/in-memory vector index, the pgvector HNSW graph, the product-quantization codebook training, the amortized corpus scan, and the cross-process recency horizon; this page owns the encode/score/rank math and nothing that resembles a store. The `ProductCodebook` descriptor — subspace count, per-subspace centroids, code width, identity — is defined and trained by the Persistence vector lane (`Query/lane#VECTOR_CODEBOOK`) and arrives settled here through the `Rasm.Persistence` project reference, so PQ here is genuine nearest-centroid assignment and centroid reconstruction over a supplied codebook, never an in-Compute fit. The two-stage retrieval is honest: `binary-hamming` carries sign only, so its `HammingBitDistance` coarse gate returns content keys, Persistence resolves the survivors' fine (`int8-scalar`/`float32`) forms, and the exact rerank runs `Rank` over those fine forms — the magnitude a 1-bit encoding discards is never faked back from the ±1 decode.

The same `Encode`/`Score`/`Rank` surface backs the BIM point-cloud→element and symbol-recognition classifier the `Model/inference#INFERENCE_MODES` `Classify`/`Embed` run feeds: a labelled class prototype is one stored `EmbeddingVector`, nearest-prototype classification is one `Rank` over the prototype set, never a BIM-specific embedding or retrieval service. The page is HOST-LOCAL and carries no `TS_PROJECTION`.

## [01]-[INDEX]

- [01]-[EMBEDDING]: the `VectorEncoding` encoding axis, the `VectorScore` metric axis, the `EmbeddingVector` content-keyed carrier, the `EncodeBudget` sign/rerank knobs, and the `Encode`/`Score`/`Rank` fold over the `TensorPrimitives` SIMD surface crossing to the Persistence vector lane by content-key reference.

## [02]-[EMBEDDING]

- Owner: `VectorEncoding` `[SmartEnum<string>]` the one closed encoding axis — `float32` · `float16` · `int8-scalar` · `binary-hamming` · `product-quantized` — each row carrying the dense `BytesPerComponent` width and the `HammingScored` discriminant and folding both into one `ByteLength(dimension, subspaces)` layout formula, so a new encoding is one row and never a per-encoding vector type; `VectorScore` `[SmartEnum<string>]` the one closed metric axis — `cosine` · `dot` · `euclidean` · `hamming` — each row carrying `SmallerIsNearer` (the ranking direction) and `OverBinary` (the packed-bit-versus-float-reduction discriminant), so a new metric is one row and never a per-metric `Similarity` method; `EncodeBudget` the encode budget — the binary `SignThreshold` and the `RerankFanout` the cross-boundary coarse gate sizes its fetch by; `EmbeddingVector` the **content-keyed buffer carrier** — the encoded component bytes, the `VectorEncoding` tag, the source `ModelIdentity.Key`, the float dimension, and the `XxHash128` content key, with an `Admit` boundary that re-checks `ByteLength` and the content-key echo on the rehydrate path, NOT a vector-store row; `Embedding` the static `Encode`/`Score`/`Rank` fold projecting the `Embed` `float[]` onto a `VectorEncoding` row, scoring any pair through the metric-lowered SIMD reduction, and top-K ranking a bounded candidate set.
- Cases: `VectorEncoding` rows `float32` (raw unit vector, the exact-rerank ground truth, `BytesPerComponent: 4`) · `float16` (`ConvertToHalf`-narrowed half-precision, 2× smaller, the high-accuracy compressed residence that widens with near-`float32` fidelity, `BytesPerComponent: 2`) · `int8-scalar` (`ConvertSaturating`-quantized symmetric int8, 4× smaller, the default index residence that survives dequantization, `BytesPerComponent: 1`) · `binary-hamming` (sign-thresholded 1-bit-per-component packed, 32× smaller, the coarse pre-filter floor, `HammingScored`) · `product-quantized` (one code byte per subspace over the Persistence-trained codebook, the largest-corpus residence); `VectorScore` rows `cosine` (`TensorPrimitives.CosineSimilarity`, the unit-vector default, larger-is-nearer) · `dot` (`TensorPrimitives.Dot`, the inner product over already-normalized vectors, larger-is-nearer) · `euclidean` (`TensorPrimitives.Distance`, smaller-is-nearer) · `hamming` (`TensorPrimitives.HammingBitDistance` over the packed bit encoding, smaller-is-nearer, `OverBinary`, the coarse-gate metric).
- Entry: `public static Fin<EmbeddingVector> Encode(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, EncodeBudget policy, Option<ProductCodebook> codebook = default)` projects the L2-normalized `Embed` output onto the encoding row and content-keys the encoded result, faulting when the `product-quantized` row is requested without its codebook or against a codebook whose dimension disagrees; `public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric, Option<ProductCodebook> codebook = default)` scores a pair asymmetrically through the metric-lowered SIMD reduction; `public static Fin<Seq<(EmbeddingVector Candidate, float Score)>> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, Option<ProductCodebook> codebook = default)` bounded-top-K ranks a candidate set; `Fin<T>` aborts on a Hamming metric over a non-binary encoding, a packed-width mismatch, a dimension mismatch, a `product-quantized` operand without its codebook or against a dimension-disagreeing codebook, or a degenerate (`NaN`/`Inf`) score.
- Auto: `Encode` dispatches the `VectorEncoding` row through one generated total `Switch` threaded on the `ReadOnlyMemory<float>` source as state so the unit vector reaches every arm with zero managed copy (a `ToArray` lift into the dispatch state is the deleted form) — the `float32` arm reinterprets the unit bytes, the `float16` arm narrows through `TensorPrimitives.ConvertToHalf` into a `Half`-reinterpreted byte buffer (a catalogued vectorized narrow, no hand-rolled half conversion), the `int8-scalar` arm scales by the protocol `Int8Scale` into a pooled `SpanOwner<float>` scratch and saturates to `sbyte` through `TensorPrimitives.ConvertSaturating<float, sbyte>` (the catalogued saturating conversion, never a hand-rolled clamp-and-cast loop), the `binary-hamming` arm sign-thresholds and bit-packs eight components to a byte, and the `product-quantized` arm assigns each contiguous sub-vector to its nearest codebook centroid by `TensorPrimitives.Distance` over the Persistence-supplied `ProductCodebook` (genuine k-means assignment, never the magnitude-extremum index a `IndexOfMaxMagnitude` placeholder would emit) — then content-keys over the **encoded** bytes plus the float dimension and the codebook identity so the quantization posture is baked into the address; `Score` routes the `OverBinary` metric to `HammingBitDistance` over the packed integral spans and every other metric to an EXHAUSTIVE identity dispatch lowering onto exactly one `TensorPrimitives` reduction — `cosine`→`CosineSimilarity`, `dot`→`Dot`, `euclidean`→`Distance` — over both operands decoded to `float`, with an explicit unlowered-metric fault arm rather than a silent final else that would score `euclidean` for an unrecognized metric (the metric→reduction lowering is an identity dispatch, not a stored `Func<ReadOnlySpan<float>,…>` row and not the generated `Switch`, because a ref-struct span argument is neither a valid generic type argument nor a capturable closure field — so compile-time exhaustiveness is forfeited and the loud runtime fault is the guard), so a hand-rolled similarity accumulation is the rejected form and a new non-binary metric adds one `VectorScore` row AND one `ScoreFloat` arm the fault forces; `Rank` decodes-and-scores every candidate, short-circuiting the whole rank on the first scoring fault, then selects the top-K through a bounded `PriorityQueue` heap (O(n log k), never a full `OrderBy` sort), keyed by the row's `SmallerIsNearer` direction; the `int8-scalar` decode dequantizes through the inverse protocol scale before any exact metric reads it, the `product-quantized` decode reconstructs the float vector by concatenating the chosen centroids, and a sentinel/`NaN`/`Inf` score projects to a `ModelRejected` fault at the boundary, never inward.
- Receipt: an `Encode` emission rides the existing `Runtime/receipts#RECEIPT_UNION` `Generate(string ModelChecksum, ExecutionProvider Ep, string ModelType, int Tokens, double TokensPerSecond, GuidanceKind GuidanceKind, int ConstrainedTokens, int ToolCalls)` family slot — the encoding rides the `ModelType` dimension and the embedded dimension the `Tokens` slot — so an embed/encode emission is auditable through the existing slot exactly as the boundary law `an Embed run rides the Generate family slot` prescribes, and an `EmbeddingReceipt` or `RetrievalReceipt` standalone type is the rejected form; `Score` and `Rank` are pure value transforms beneath the receipt edge (no native handle, no side effect) so they mint no receipt, and the candidate-index recall/latency is the Persistence vector-lane owner's measured concern.
- Packages: System.Numerics.Tensors, System.IO.Hashing, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new vector encoding is one `VectorEncoding` row with its `Encode`/`DecodeFloat`-arm `Switch` cases and its `BytesPerComponent`/`ByteLength` posture; a new similarity metric is one `VectorScore` row carrying its ranking direction and over-binary discriminant plus its one `ScoreFloat` lowering arm; a new encode knob is one column on `EncodeBudget`; a BIM class prototype is one stored `EmbeddingVector` and nearest-prototype classification is one `Rank` over the prototype set; zero new surface — a `CosineSimilarity`/`DotProductScorer`/`EuclideanDistance`/`HammingScorer` sibling method family is collapsed onto the one `Score` fold over the `VectorScore` axis, an `Int8Quantizer`/`BinaryEncoder`/`ProductQuantizer` sibling owner is collapsed onto the one `Encode` fold over the `VectorEncoding` axis, and a per-encoding `EmbeddingVector` subtype is collapsed onto the one content-keyed carrier whose encoding column discriminates.
- Boundary: this page owns embed-and-score and crosses to the `Rasm.Persistence` vector lane **by content-key reference only** — Persistence owns the on-disk/in-memory vector index, the pgvector HNSW graph, the product-quantization codebook training, the amortized asymmetric-distance corpus scan, and the cross-process result-reuse recency horizon, so a Compute-side vector store, ANN index, codebook fit, or recency horizon is the named drift defect and the `EmbeddingVector.ContentKey` is the single join the Persistence index addresses (the `:x32` form the Persistence marshal renders is the suite `XxHash128` value, never a second hash); the embedding **projection** is the `Model/inference#INFERENCE_MODES` `Embed` run's — `Embed` mean-pools-or-CLS-slices then L2-normalizes via `TensorPrimitives.Norm`+`Divide` to the unit `float[]` this page encodes — so a re-implemented mean-pool/normalize here is the rejected form and a raw last-token passthrough is rejected at the `Embed` owner already; `System.Numerics.Tensors` owns every reduction and conversion — scoring lowers onto `CosineSimilarity`/`Dot`/`Distance` over `float` spans and `HammingBitDistance` (a `long`-returning bit-distance) over the packed integral spans, the float16 narrow lowers onto `ConvertToHalf` and the widen onto `ConvertToSingle`, the int8 quantize lowers onto `ConvertSaturating<float, sbyte>` and dequantize onto `ConvertChecked<sbyte, float>`, and PQ assignment lowers onto `Distance` — so a hand-rolled dot accumulation, a half conversion, a saturating clamp, a bit-distance popcount, or a centroid-distance loop is the deleted form, and only the sign-pack bit-twiddle and the nearest-centroid argmin remain as the two named span-kernel exemptions because no single `TensorPrimitives` member packs one bit per element or argmin-assigns a codebook; the two-stage retrieval is honest — `binary-hamming` is a sign-only encoding whose `Score` against a non-Hamming metric decodes to a ±1 vector that recovers no magnitude (so a float rerank of a decoded binary vector is the coarse gate's own information, not a precision gain), the genuine rerank is `Rank` over the survivors' fine forms that Persistence resolves from the content keys the coarse `Rank` returns, sized `top × policy.RerankFanout`, so a binary-Hamming terminal verdict that skips the fine rerank and an in-page float rerank of the ±1 decode are both the named defects; the `float16`/`int8-scalar` dense rows keep magnitude, so the asymmetric path scores the full-precision `float32` query against the widened-half or dequantized-int8 candidate and a cross-encoding `Score` (a `float32` query against a `float16`/`int8-scalar` candidate) is a SUPPORTED bridge, never a fault — only a Hamming metric over a non-binary encoding, a packed-width mismatch, a dimension mismatch, a `product-quantized` operand without its codebook, or a degenerate score faults `ModelRejected` at the boundary so a silently-wrong score never reaches the rail; the `ProductCodebook` descriptor (subspaces, per-subspace centroids, code width, identity) arrives settled from the Persistence vector-lane index owner — this page does nearest-centroid encode and centroid-reconstruction decode over it but never trains it, and the amortized asymmetric-distance scan over the full corpus is the Persistence index's concern while the bounded rerank here reconstructs-and-scores; the content key folds the model key, the encoding, the float dimension, the codebook identity, and the **encoded** bytes so the quantization posture and the codebook are part of the address (a key over the raw `float` bytes that ignores the posture, or over the packed bytes alone without the dimension the non-invertible `binary-hamming` length cannot recover, is the deleted form that would alias two distinct artifacts onto one key), and `EmbeddingVector.Admit` re-validates `ByteLength` and the content-key echo on the rehydrate-from-Persistence path so a corrupt or mistyped buffer faults at admission rather than scoring wrong; the encoded component bytes the carrier holds are an OWNED array — the transient int8 scaling scratch is a pooled `SpanOwner<float>` disposed in-arm, but a pooled `MemoryOwner` rent can never back the immutable carrier because the pool reuses the buffer (a use-after-free, the deleted form).

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorEncoding {
    public static readonly VectorEncoding Float32 = new("float32", bytesPerComponent: 4, hammingScored: false);
    public static readonly VectorEncoding Float16 = new("float16", bytesPerComponent: 2, hammingScored: false);
    public static readonly VectorEncoding Int8Scalar = new("int8-scalar", bytesPerComponent: 1, hammingScored: false);
    public static readonly VectorEncoding BinaryHamming = new("binary-hamming", bytesPerComponent: 0, hammingScored: true);
    public static readonly VectorEncoding ProductQuantized = new("product-quantized", bytesPerComponent: 1, hammingScored: false);

    public int BytesPerComponent { get; }
    public bool HammingScored { get; }

    // Stored component-buffer length: dense rows are dimension x bytes-per-component, the bit-packed row is
    // ceil(dimension / 8), the product-quantized row is one code byte per subspace.
    public int ByteLength(int dimension, int subspaces) =>
        this == BinaryHamming ? (dimension + 7) / 8
        : this == ProductQuantized ? subspaces
        : dimension * BytesPerComponent;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorScore {
    public static readonly VectorScore Cosine = new("cosine", smallerIsNearer: false, overBinary: false);
    public static readonly VectorScore Dot = new("dot", smallerIsNearer: false, overBinary: false);
    public static readonly VectorScore Euclidean = new("euclidean", smallerIsNearer: true, overBinary: false);
    public static readonly VectorScore Hamming = new("hamming", smallerIsNearer: true, overBinary: true);

    public bool SmallerIsNearer { get; }
    public bool OverBinary { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

public sealed record EncodeBudget(float SignThreshold, int RerankFanout) {
    public static readonly EncodeBudget Canonical = new(SignThreshold: 0f, RerankFanout: 4);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record EmbeddingVector(
    VectorEncoding Encoding,
    string ModelKey,
    int Dimension,
    ReadOnlyMemory<byte> Components,
    UInt128 ContentKey) {
    public string ArtifactKey => $"{ContentKey:x32}:{Encoding.Key}";

    // The content key binds EVERY identity input Admit re-checks — model key, encoding, float dimension,
    // codebook identity, encoded bytes. Dimension is load-bearing for the bit-packed `binary-hamming` row whose
    // ceil(dimension / 8) byte length is non-invertible (dims 8k-7..8k all pack to k bytes), so a key over the
    // packed bytes alone would alias two distinct-dimension sign vectors onto one address.
    public static UInt128 KeyOf(string modelKey, VectorEncoding encoding, int dimension, ReadOnlySpan<byte> components, UInt128 codebookId = default) {
        var hash = new XxHash128();
        hash.Append(MemoryMarshal.AsBytes(modelKey.AsSpan()));
        hash.Append(MemoryMarshal.AsBytes(encoding.Key.AsSpan()));
        Span<byte> tag = stackalloc byte[20];
        BinaryPrimitives.WriteUInt128LittleEndian(tag, codebookId);
        BinaryPrimitives.WriteInt32LittleEndian(tag[16..], dimension);
        hash.Append(tag);
        hash.Append(components);
        return hash.GetCurrentHashAsUInt128();
    }

    public static Fin<EmbeddingVector> Admit(VectorEncoding encoding, string modelKey, int dimension, int subspaces, ReadOnlyMemory<byte> components, UInt128 codebookId, UInt128 expectedKey) =>
        components.Length != encoding.ByteLength(dimension, subspaces)
            ? Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-byte-length:{components.Length}!={encoding.ByteLength(dimension, subspaces)}>"))
            : KeyOf(modelKey, encoding, dimension, components.Span, encoding == VectorEncoding.ProductQuantized ? codebookId : UInt128.Zero) is var key && key != expectedKey
                ? Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-key-echo:{key:x32}!={expectedKey:x32}>"))
                : Fin.Succ(new EmbeddingVector(encoding, modelKey, dimension, components, key));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Embedding {
    // int8 symmetric scale is a protocol constant, not a stored knob: an L2-normalized vector's components lie in
    // [-1, 1], so 127 maps the full range onto the sbyte grid and dividing by 127 recovers magnitude on decode.
    const float Int8Scale = 127f;

    public static Fin<EmbeddingVector> Encode(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, EncodeBudget policy, Option<ProductCodebook> codebook = default) =>
        encoding != VectorEncoding.ProductQuantized
            ? Fin.Succ(Mint(unit, encoding, modelKey, policy, codebook))
            : codebook.Case is ProductCodebook book
                ? book.Dimension != unit.Length
                    ? Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-pq-dimension:{unit.Length}!={book.Dimension}>"))
                    : Fin.Succ(Mint(unit, encoding, modelKey, policy, codebook))
                : Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-pq-needs-codebook:{modelKey}>"));

    public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric, Option<ProductCodebook> codebook = default) =>
        metric.OverBinary
            ? Hammed(query, candidate)
            : query.Dimension != candidate.Dimension
                ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-dimension:{query.Dimension}!={candidate.Dimension}>"))
                : PqFault(query, candidate, codebook) is ComputeFault fault
                    ? Fin.Fail<float>(fault)
                    : ScoreFloat(DecodeFloat(query, codebook).Span, DecodeFloat(candidate, codebook).Span, metric);

    public static Fin<Seq<(EmbeddingVector Candidate, float Score)>> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, Option<ProductCodebook> codebook = default) =>
        candidates
            .TraverseM(candidate => Score(query, candidate, metric, codebook).Map(score => (Candidate: candidate, Score: score)))
            .Map(scored => TopK(scored, top, metric.SmallerIsNearer))
            .As();

    static EmbeddingVector Mint(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, EncodeBudget policy, Option<ProductCodebook> codebook) {
        var components = encoding.Switch(
            state: (Unit: unit, Policy: policy, Codebook: codebook),
            float32: static s => MemoryMarshal.AsBytes(s.Unit.Span).ToArray(),
            float16: static s => EncodeHalf(s.Unit.Span),
            int8Scalar: static s => EncodeInt8(s.Unit.Span),
            binaryHamming: static s => EncodeBinary(s.Unit.Span, s.Policy.SignThreshold),
            productQuantized: static s => s.Codebook.Case is ProductCodebook book ? EncodeProduct(s.Unit.Span, book) : []);
        var codebookId = encoding == VectorEncoding.ProductQuantized
            ? codebook.Match(Some: static book => book.Id, None: static () => UInt128.Zero)
            : UInt128.Zero;
        return new EmbeddingVector(encoding, modelKey, unit.Length, components, EmbeddingVector.KeyOf(modelKey, encoding, unit.Length, components, codebookId));
    }

    static Fin<float> Hammed(EmbeddingVector query, EmbeddingVector candidate) =>
        !(query.Encoding.HammingScored && candidate.Encoding.HammingScored)
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-hamming-needs-binary:{query.Encoding.Key}|{candidate.Encoding.Key}>"))
            : query.Components.Length != candidate.Components.Length
                ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-hamming-width:{query.Components.Length}!={candidate.Components.Length}>"))
                : Fin.Succ((float)TensorPrimitives.HammingBitDistance(query.Components.Span, candidate.Components.Span));

    // The metric->reduction lowering is an EXHAUSTIVE identity dispatch with an explicit unlowered-metric fault
    // arm, never a silent final else that scores euclidean for an unrecognized metric: a `ReadOnlySpan<float>`
    // cannot thread through the generated `Switch` state nor a closure (a ref struct is neither a valid generic
    // type argument nor a capturable lambda field), so compile-time exhaustiveness is forfeited and the loud
    // runtime fault is the guard a new non-binary `VectorScore` row trips until its `ScoreFloat` arm lands.
    static Fin<float> ScoreFloat(ReadOnlySpan<float> left, ReadOnlySpan<float> right, VectorScore metric) =>
        metric == VectorScore.Cosine ? Guarded(TensorPrimitives.CosineSimilarity(left, right))
        : metric == VectorScore.Dot ? Guarded(TensorPrimitives.Dot(left, right))
        : metric == VectorScore.Euclidean ? Guarded(TensorPrimitives.Distance(left, right))
        : Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-metric-unlowered:{metric.Key}>"));

    // PQ readiness guard: a product-quantized operand decodes through the codebook, so the codebook must be present
    // and dimension-matched before any reduction reads the reconstructed spans; null is the no-fault verdict.
    static ComputeFault? PqFault(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        query.Encoding != VectorEncoding.ProductQuantized && candidate.Encoding != VectorEncoding.ProductQuantized
            ? null
            : codebook.Case is ProductCodebook book
                ? book.Dimension != query.Dimension
                    ? new ComputeFault.ModelRejected($"<embed-pq-dimension:{query.Dimension}!={book.Dimension}>")
                    : null
                : new ComputeFault.ModelRejected($"<embed-pq-needs-codebook:{query.ArtifactKey}|{candidate.ArtifactKey}>");

    static ReadOnlyMemory<float> DecodeFloat(EmbeddingVector vector, Option<ProductCodebook> codebook) =>
        vector.Encoding.Switch(
            state: (Vector: vector, Codebook: codebook),
            float32: static s => MemoryMarshal.Cast<byte, float>(s.Vector.Components.Span).ToArray(),
            float16: static s => DecodeHalf(s.Vector.Components.Span),
            int8Scalar: static s => DecodeInt8(MemoryMarshal.Cast<byte, sbyte>(s.Vector.Components.Span)),
            binaryHamming: static s => DecodeBinary(s.Vector.Components.Span, s.Vector.Dimension),
            productQuantized: static s => s.Codebook.Case is ProductCodebook book ? Reconstruct(s.Vector.Components.Span, book) : []);

    static byte[] EncodeInt8(ReadOnlySpan<float> unit) {
        using var scaled = SpanOwner<float>.Allocate(unit.Length);
        TensorPrimitives.Multiply(unit, Int8Scale, scaled.Span);
        var components = new byte[unit.Length];
        TensorPrimitives.ConvertSaturating<float, sbyte>(scaled.Span, MemoryMarshal.Cast<byte, sbyte>(components));
        return components;
    }

    static float[] DecodeInt8(ReadOnlySpan<sbyte> codes) {
        var unit = new float[codes.Length];
        TensorPrimitives.ConvertChecked<sbyte, float>(codes, unit);
        TensorPrimitives.Divide(unit, Int8Scale, unit);
        return unit;
    }

    static byte[] EncodeHalf(ReadOnlySpan<float> unit) {
        var components = new byte[unit.Length * 2];
        TensorPrimitives.ConvertToHalf(unit, MemoryMarshal.Cast<byte, Half>(components));
        return components;
    }

    static float[] DecodeHalf(ReadOnlySpan<byte> components) {
        var unit = new float[components.Length / 2];
        TensorPrimitives.ConvertToSingle(MemoryMarshal.Cast<byte, Half>(components), unit);
        return unit;
    }

    static byte[] EncodeBinary(ReadOnlySpan<float> unit, float threshold) {
        var packed = new byte[(unit.Length + 7) / 8];
        for (int component = 0; component < unit.Length; component++) {
            if (unit[component] > threshold) { packed[component >> 3] |= (byte)(1 << (component & 7)); }
        }
        return packed;
    }

    static float[] DecodeBinary(ReadOnlySpan<byte> packed, int dimension) {
        var unit = new float[dimension];
        for (int component = 0; component < dimension; component++) {
            unit[component] = (packed[component >> 3] & (1 << (component & 7))) != 0 ? 1f : -1f;
        }
        return unit;
    }

    static byte[] EncodeProduct(ReadOnlySpan<float> unit, ProductCodebook codebook) {
        var codes = new byte[codebook.Subspaces];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            var part = unit.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            (float Nearest, int Code) best = (float.PositiveInfinity, 0);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                float distance = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
                if (distance < best.Nearest) { best = (distance, code); }
            }
            codes[subspace] = (byte)best.Code;
        }
        return codes;
    }

    static float[] Reconstruct(ReadOnlySpan<byte> codes, ProductCodebook codebook) {
        var unit = new float[codebook.Dimension];
        for (int subspace = 0; subspace < codes.Length; subspace++) {
            codebook.Centroid(subspace, codes[subspace]).CopyTo(unit.AsSpan(subspace * codebook.SubspaceDim, codebook.SubspaceDim));
        }
        return unit;
    }

    static Seq<(EmbeddingVector Candidate, float Score)> TopK(Seq<(EmbeddingVector Candidate, float Score)> scored, int top, bool smallerIsNearer) {
        if (top <= 0 || scored.IsEmpty) { return Seq<(EmbeddingVector Candidate, float Score)>(); }
        var heap = new PriorityQueue<(EmbeddingVector Candidate, float Score), float>(top);
        foreach (var row in scored) {
            float rank = smallerIsNearer ? -row.Score : row.Score;
            if (heap.Count < top) { heap.Enqueue(row, rank); }
            else if (heap.TryPeek(out _, out float worst) && rank > worst) { heap.EnqueueDequeue(row, rank); }
        }
        int kept = heap.Count;
        var ordered = new (EmbeddingVector Candidate, float Score)[kept];
        for (int slot = kept - 1; slot >= 0; slot--) { ordered[slot] = heap.Dequeue(); }
        return toSeq(ordered);
    }

    static Fin<float> Guarded(float score) =>
        float.IsNaN(score) || float.IsInfinity(score)
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-score-degenerate:{score:R}>"))
            : Fin.Succ(score);
}
```

## [03]-[RESEARCH]

- [PQ_CODEBOOK_CONTRACT]: the `product-quantized` encode and decode consume a `ProductCodebook` (`Subspaces`, `SubspaceDim`, `CodesPerSubspace`, the `Centroids` buffer, a content `Id`, the `Dimension => Subspaces * SubspaceDim` and `Centroid(subspace, code)` accessors) defined, trained, and owned by the `Rasm.Persistence` vector lane (`Query/lane#VECTOR_CODEBOOK`) — this page assigns each sub-vector to its nearest centroid by `TensorPrimitives.Distance` and reconstructs a candidate by concatenating its chosen centroids, but never fits the codebook (the k-means centroids per subspace the Persistence index trains over the admitted corpus cross as settled vocabulary). The amortized asymmetric-distance-computation scan that precomputes one query→centroid table and sums per-subspace lookups over the whole corpus is the Persistence index's, because it owns the HNSW/IVF traversal and the recency horizon; the bounded rerank here reconstructs-and-scores the Persistence-supplied candidate set. The exact subspace tiling and code width band against the Persistence vector-lane codebook descriptor at the index-admission gate, and the content key folds the codebook `Id` so a re-trained codebook re-keys every `product-quantized` artifact that depends on it.
