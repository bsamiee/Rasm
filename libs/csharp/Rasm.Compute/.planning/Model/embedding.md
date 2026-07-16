# [COMPUTE_EMBEDDING]

`Rasm.Compute`'s embedding lane is the retrieval half of the inference spine: it owns one encoding axis (`VectorEncoding`), one metric axis (`VectorScore`), one content-keyed carrier (`EmbeddingVector`), and one `Encode`/`Score`/`Rank` fold over the `System.Numerics.Tensors` `TensorPrimitives` SIMD surface. It projects the L2-normalized unit `float[]` the `Model/inference#INFERENCE_MODES` `Embed` run produces down the `VectorEncoding` axis — `float32` exact-rerank ground truth, `float16` the `ConvertToHalf` 2× residence widening to near-`float32`, `int8-scalar` the `ConvertSaturating` 4× index residence, `binary-hamming` the 32× sign-packed coarse gate, `product-quantized` the codebook-coded residence for the largest corpora — and scores and ranks what production already minted. RETRIEVAL only: vector PRODUCTION is the AppHost-governed `IEmbeddingGenerator` lane where Compute supplies the inner provider and never the builder, and no vector store, ANN index, codebook fit, or recency horizon lives here.

Every artifact content-keys through `System.IO.Hashing` `XxHash128` over the source `ModelIdentity.Key`, the encoding key, the float dimension, the codebook identity, and the encoded component bytes, so a deterministic re-encode addresses the same key the `Rasm.Persistence` vector index dedups against and a posture change never aliases two artifacts onto one key. Crossing to Persistence is content-key reference only: Persistence owns the on-disk/in-memory index, the pgvector HNSW graph, the product-quantization codebook training, the amortized corpus scan, and the cross-process recency horizon, and the `ProductCodebook` descriptor arrives settled from the Persistence vector lane (`Query/retrieval#VECTOR_CODEBOOK`) so PQ here is nearest-centroid assignment and centroid reconstruction over a supplied codebook, never an in-Compute fit. Two-stage retrieval stays honest — `binary-hamming` carries sign only, so its `HammingBitDistance` coarse gate returns content keys, Persistence resolves the survivors' fine forms, and the exact rerank runs `Rank` over those. Same `Encode`/`Score`/`Rank` surface backs the BIM point-cloud→element and symbol-recognition classifier the `Classify`/`Embed` run feeds — a class prototype is one stored `EmbeddingVector`, nearest-prototype classification one `Rank`, never a BIM-specific retrieval service. Page is HOST-LOCAL and carries no `TS_PROJECTION`.

## [01]-[INDEX]

- [01]-[EMBEDDING]: the `VectorEncoding` encoding axis, the `VectorScore` metric axis, the `EmbeddingVector` content-keyed carrier, the `EncodeBudget` sign/rerank knobs, and the `Encode`/`Score`/`Rank` fold over the `TensorPrimitives` SIMD surface crossing to the Persistence vector lane by content-key reference.

## [02]-[EMBEDDING]

- Owner: `VectorEncoding` `[SmartEnum<string>]` the one closed encoding axis, each row carrying `BytesPerComponent` and the `HammingScored` discriminant folded into one `ByteLength(dimension, subspaces)` formula, so a new encoding is one row, never a per-encoding vector type; `VectorScore` `[SmartEnum<string>]` the one closed metric axis, each row carrying `SmallerIsNearer` (ranking direction) and `OverBinary` (packed-bit-versus-float discriminant), so a new metric is one row, never a per-metric `Similarity` method; `EncodeBudget` the binary `SignThreshold` and the `RerankFanout` the coarse gate sizes its fetch by; `EmbeddingVector` the content-keyed buffer carrier — encoded bytes, encoding tag, source `ModelIdentity.Key`, float dimension, `XxHash128` key — whose `Admit` boundary re-checks `ByteLength` and the content-key echo on rehydrate, NOT a vector-store row; `Embedding` the static `Encode`/`Score`/`Rank` fold.
- Cases: `VectorEncoding` rows `float32` (raw unit vector, exact-rerank ground truth) · `float16` (`ConvertToHalf`-narrowed, 2× smaller, high-accuracy residence widening to near-`float32`) · `int8-scalar` (`ConvertSaturating`-quantized symmetric int8, 4× smaller, default index residence) · `binary-hamming` (sign-thresholded 1-bit-per-component packed, 32× smaller, coarse pre-filter floor) · `product-quantized` (one code byte per subspace over the Persistence-trained codebook, largest-corpus residence); `VectorScore` rows `cosine` (unit-vector default, larger-is-nearer) · `dot` (inner product over normalized vectors, larger-is-nearer) · `euclidean` (smaller-is-nearer) · `hamming` (over the packed bit encoding, smaller-is-nearer, the coarse-gate metric).
- Entry: `Encode` projects the `Embed` unit output onto the encoding row and content-keys the result; `Score` scores a pair asymmetrically through the metric-lowered SIMD reduction; `Rank` bounded-top-K ranks a candidate set. `Fin<T>` aborts on a Hamming metric over a non-binary encoding, a packed-width mismatch, a dimension mismatch, a `product-quantized` operand without or against a dimension-disagreeing codebook, or a degenerate (`NaN`/`Inf`) score.
- Auto: `Encode` dispatches the `VectorEncoding` row through one generated total `Switch` threaded on the `ReadOnlyMemory<float>` source as state so the unit vector reaches every arm with zero managed copy — a `ToArray` lift into the state is the deleted form; the `int8-scalar` arm scales into a pooled `SpanOwner<float>` scratch and saturates through `TensorPrimitives.ConvertSaturating<float, sbyte>` (never a hand-rolled clamp-and-cast loop), and the `product-quantized` arm assigns each sub-vector to its nearest codebook centroid by `TensorPrimitives.Distance` (genuine k-means assignment, never the magnitude-extremum index an `IndexOfMaxMagnitude` placeholder emits); the address then folds the encoded bytes plus dimension and codebook identity, baking the quantization posture in. `Score` routes the `OverBinary` metric to `HammingBitDistance` over the packed spans and every other metric to the exhaustive identity dispatch lowering onto one `TensorPrimitives` reduction, so a new non-binary metric adds one `VectorScore` row AND one `ScoreFloat` arm the fault forces. `Rank` decodes-and-scores every candidate, short-circuiting on the first fault, then selects top-K through a bounded `PriorityQueue` heap (O(n log k), never a full `OrderBy` sort) keyed by `SmallerIsNearer`; the `int8-scalar` decode dequantizes and the `product-quantized` decode reconstructs by concatenating centroids before any exact metric reads, and a degenerate score projects to `ModelRejected` at the boundary, never inward.
- Receipt: an `Encode` emission rides the existing `Runtime/receipts#RECEIPT_UNION` `Generate` family slot — encoding on the `ModelType` dimension, embedded dimension on the `Tokens` slot — per the boundary law that an Embed run rides the Generate slot, so a standalone `EmbeddingReceipt`/`RetrievalReceipt` type is the rejected form; `Score` and `Rank` are pure value transforms beneath the receipt edge and mint none, and candidate-index recall/latency is the Persistence vector-lane owner's measured concern.
- Packages: System.Numerics.Tensors, System.IO.Hashing, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new vector encoding is one `VectorEncoding` row with its `Encode`/`DecodeFloat` `Switch` arm and its `BytesPerComponent`/`ByteLength` posture; a new metric is one `VectorScore` row with its direction and over-binary discriminant plus one `ScoreFloat` lowering arm; a new encode knob is one column on `EncodeBudget`; a BIM class prototype is one stored `EmbeddingVector` and nearest-prototype classification one `Rank`; zero new surface — a `CosineSimilarity`/`DotProductScorer`/`HammingScorer` method family collapses onto the one `Score` fold, an `Int8Quantizer`/`BinaryEncoder`/`ProductQuantizer` owner onto the one `Encode` fold, and a per-encoding `EmbeddingVector` subtype onto the one carrier whose encoding column discriminates.
- Boundary: this page owns embed-and-score and crosses to the `Rasm.Persistence` vector lane by content-key reference only — Persistence owns the index, the pgvector HNSW graph, the codebook training, the amortized asymmetric-distance scan, and the recency horizon, so a Compute-side vector store, ANN index, codebook fit, or horizon is the named drift defect and `EmbeddingVector.ContentKey` is the single join the index addresses (the `:x32` form the Persistence marshal renders is the suite `XxHash128` value, never a second hash). Embedding projection is the `Model/inference#INFERENCE_MODES` `Embed` run's — it mean-pools-or-CLS-slices then L2-normalizes via `TensorPrimitives.Norm`+`Divide` — so a re-implemented mean-pool/normalize here is the rejected form and a raw last-token passthrough is rejected at `Embed` already. `System.Numerics.Tensors` owns every reduction and conversion — scoring lowers onto `CosineSimilarity`/`Dot`/`Distance` over `float` spans and `HammingBitDistance` over the packed integral spans, float16 onto `ConvertToHalf`/`ConvertToSingle`, int8 onto `ConvertSaturating<float, sbyte>`/`ConvertChecked<sbyte, float>`, PQ assignment onto `Distance` — so a hand-rolled dot, half conversion, saturating clamp, bit-distance popcount, or centroid-distance loop is the deleted form, and only the sign-pack bit-twiddle and the nearest-centroid argmin remain as the two named span-kernel exemptions because no single `TensorPrimitives` member packs one bit per element or argmin-assigns a codebook. Two-stage retrieval is honest — `binary-hamming` is sign-only, so a `Score` against a non-Hamming metric decodes to a ±1 vector recovering no magnitude (a float rerank of that decode is the coarse gate's own information, not a precision gain), and the genuine rerank is `Rank` over the survivors' fine forms Persistence resolves from the coarse `Rank`'s returned keys, sized `top × policy.RerankFanout`; a binary-Hamming terminal verdict skipping the fine rerank and an in-page float rerank of the ±1 decode are both named defects. Dense `float16`/`int8-scalar` rows keep magnitude, so the asymmetric path scores the full-precision `float32` query against the widened-half or dequantized-int8 candidate and a cross-encoding `Score` is a supported bridge, never a fault — only a Hamming metric over a non-binary encoding, a packed-width mismatch, a dimension mismatch, a `product-quantized` operand without its codebook, or a degenerate score faults `ModelRejected` at the boundary so a silently-wrong score never reaches the rail. `ProductCodebook` (subspaces, per-subspace centroids, code width, identity) arrives settled from the Persistence vector-lane owner — this page does nearest-centroid encode and centroid-reconstruction decode over it but never trains it, and the amortized asymmetric-distance corpus scan is the index's concern while the bounded rerank here reconstructs-and-scores. Content key folds the model key, encoding, float dimension, codebook identity, and encoded bytes so the quantization posture and codebook are part of the address, so a re-trained codebook re-keys every `product-quantized` artifact, and `EmbeddingVector.Admit` re-validates `ByteLength` and the content-key echo on the rehydrate path so a corrupt or mistyped buffer faults at admission rather than scoring wrong. Encoded component bytes the carrier holds are an owned array — the transient int8 scaling scratch is a pooled `SpanOwner<float>` disposed in-arm, but a pooled `MemoryOwner` rent can never back the immutable carrier because the pool reuses the buffer (a use-after-free, the deleted form).

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

    // Content key binds every identity input Admit re-checks: model key, encoding, float dimension, codebook id, encoded bytes.
    // Dimension is load-bearing — the `binary-hamming` ceil(dimension / 8) length is non-invertible (dims 8k-7..8k pack to k bytes),
    // so keying packed bytes alone aliases distinct-dimension sign vectors onto one address.
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
    // int8 symmetric scale is a protocol constant: an L2-normalized vector lies in [-1, 1], so 127 maps the range onto the sbyte grid and dividing by 127 recovers magnitude on decode.
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

    // Metric->reduction lowering is an exhaustive identity dispatch with an explicit unlowered-metric fault arm, never a silent final else:
    // a `ReadOnlySpan<float>` threads through neither the generated `Switch` state nor a closure (a ref struct is not a valid generic type
    // argument nor a capturable field), so compile-time exhaustiveness is forfeited and the runtime fault guards a new `VectorScore` row until its `ScoreFloat` arm lands.
    static Fin<float> ScoreFloat(ReadOnlySpan<float> left, ReadOnlySpan<float> right, VectorScore metric) =>
        metric == VectorScore.Cosine ? Guarded(TensorPrimitives.CosineSimilarity(left, right))
        : metric == VectorScore.Dot ? Guarded(TensorPrimitives.Dot(left, right))
        : metric == VectorScore.Euclidean ? Guarded(TensorPrimitives.Distance(left, right))
        : Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-metric-unlowered:{metric.Key}>"));

    // PQ readiness guard: a product-quantized operand needs its codebook present and dimension-matched before any reduction reads the reconstructed spans; null is the no-fault verdict.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
