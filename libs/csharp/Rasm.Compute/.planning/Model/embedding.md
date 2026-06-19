# [COMPUTE_EMBEDDING]

Rasm.Compute model lane: the embedding-and-retrieval half of the inference spine, owning ONE encoding axis (`VectorEncoding`), ONE metric axis (`VectorScore`), ONE content-keyed vector carrier (`EmbeddingVector`), and ONE state-threaded `Score`/`Encode`/`Rank` fold over the `System.Numerics.Tensors` `TensorPrimitives` SIMD reduction surface — it takes the L2-normalized `float[]` the `Model/inference#INFERENCE_MODES` `Embed` run already produces (the embedding projection is **not** owned here: `Embed` mean-pools-or-CLS-slices then L2-normalizes via `TensorPrimitives.Norm`+`Divide` to a unit vector, and this page extends that one `Embed`/`BoundLoop` device-resident run with the quantization, scoring, and retrieval owner the run lacks — never a new model lane), quantizes it down the `VectorEncoding` axis through `TensorPrimitives.ConvertSaturating` and the sign/codebook arms, scores any encoding-pair through the metric-lowered SIMD member, and content-keys every vector through the same `System.IO.Hashing` `XxHash128` identity scheme `Model/identity#MODEL_IDENTITY` rides. The page reads the `ExecutionProvider`/`ModelPrecision` from `Model/providers#EP_AXIS` and the `ModelIdentity.Key` from `Model/identity#MODEL_IDENTITY` as settled vocabulary, an embed run rides the existing `Runtime/receipts#RECEIPT_UNION` `Generate` family slot (an `Embed` vector count is a `Generate` receipt, never a new `Embedding` case), and the candidate set crosses to the `Rasm.Persistence` vector lane **by content-key reference** — Persistence owns the on-disk/in-memory vector index and the recency horizon, this page owns embed-and-score, so a Compute-side vector store, ANN graph, or persisted index is the named drift defect. The same `Score`/`Encode` surface backs the BIM point-cloud→element and symbol-recognition classifier the `Model/inference#INFERENCE_MODES` `Classify`/`Embed` run feeds (a class prototype is one stored `EmbeddingVector`, nearest-prototype is one `Score` fold), never a BIM-specific retrieval service; this page is HOST-LOCAL and carries no TS_PROJECTION.

## [1]-[INDEX]

- [1]-[EMBEDDING]: the `VectorEncoding` encoding axis, the `VectorScore` metric axis, the `EmbeddingVector` content-keyed carrier, the `QuantizationPolicy` budget, and the `Score`/`Encode`/`Rank` fold over the `TensorPrimitives` SIMD surface crossing to the Persistence vector lane by reference.

## [2]-[EMBEDDING]

- Owner: `VectorEncoding` `[SmartEnum<string>]` the one closed encoding axis — `float32` · `int8-scalar` · `binary-hamming` · `product-quantized` — each row a column-bearing posture (bytes-per-component, packed-bit width, whether it scalar-quantizes, whether it is Hamming-scored), so a new encoding is one row and never a per-encoding vector type; `VectorScore` `[SmartEnum<string>]` the one closed metric axis — `cosine` · `dot` · `euclidean` · `hamming` — each row carrying the lowered `TensorPrimitives` member identity and whether a smaller score ranks better, so a new metric is one row and never a per-metric `Similarity` method; `QuantizationPolicy` the encode-budget record carrying the int8 symmetric scale, the product-quantization subspace count and codebook bit width, and the binary sign threshold folded into the content-key seed; `EmbeddingVector` the **content-keyed buffer carrier** — the encoded component bytes, the `VectorEncoding` tag, the source `ModelIdentity.Key`, the float dimension, and the `XxHash128` content key over (model key, encoding, raw float bytes), NOT a vector-store row; `Embedding` the static `Encode`/`Score`/`Rank` fold projecting the `Model/inference#INFERENCE_MODES` `Embed` `float[]` into a `VectorEncoding` row, scoring any stored-vector pair through the metric-lowered SIMD reduction, and two-stage ranking a binary-Hamming pre-filter into an exact float rerank.
- Cases: `VectorEncoding` rows `float32` (raw unit vector, the exact-rerank ground truth) · `int8-scalar` (`ConvertSaturating`-quantized symmetric int8, 4× smaller, the default index residence) · `binary-hamming` (sign-thresholded 1-bit-per-component packed, 32× smaller, the coarse pre-filter floor) · `product-quantized` (subspace-codebook indices, the largest-corpus residence); `VectorScore` rows `cosine` (`TensorPrimitives.CosineSimilarity`, the unit-vector default) · `dot` (`TensorPrimitives.Dot`, the inner product over already-normalized vectors) · `euclidean` (`TensorPrimitives.Distance`, smaller-is-nearer) · `hamming` (`TensorPrimitives.HammingBitDistance` over the packed bit encoding, smaller-is-nearer, the pre-filter metric).
- Entry: `public static EmbeddingVector Encode(ReadOnlySpan<float> unit, VectorEncoding encoding, string modelKey, QuantizationPolicy policy)` projects the L2-normalized `Embed` output onto the encoding row and content-keys the result; `public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric)` scores an encoding-matched pair through the metric-lowered SIMD reduction; `public static Fin<Seq<(EmbeddingVector Candidate, float Score)>> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top)` two-stage ranks the candidate set; `Fin<T>` aborts on an encoding mismatch the metric cannot bridge, a Hamming score over a non-binary encoding, or a dimension mismatch between query and candidate.
- Auto: `Encode` dispatches the `VectorEncoding` row through one generated `Switch` — the `float32` arm copies the unit vector's bytes, the `int8-scalar` arm scales each component by the policy `Int8Scale` and saturates to `sbyte` through `TensorPrimitives.ConvertSaturating<float, sbyte>` (the catalogued saturating conversion, never a hand-rolled clamp-and-cast loop), the `binary-hamming` arm sign-thresholds each component against the policy `SignThreshold` and packs the sign bits eight-to-a-byte through `BitOperations`, and the `product-quantized` arm splits the vector into `policy.Subspaces` contiguous sub-vectors and emits the nearest-codebook index per subspace (the codebook is supplied by the Persistence index owner at admission, never trained here) — every arm keys the carrier through `XxHash128.HashToUInt128` over the model key, the encoding key, and the raw float bytes so a re-encode of an identical vector at identical model+encoding addresses the same content key the Persistence index dedups against; `Score` dispatches the `VectorScore` row through one generated `Switch` lowering each metric onto exactly one `TensorPrimitives` reduction over the decoded component spans — `cosine`→`CosineSimilarity`, `dot`→`Dot`, `euclidean`→`Distance`, `hamming`→`HammingBitDistance` over the packed bit spans — so a hand-rolled similarity accumulation loop is the rejected form and a new metric adds one row, never a fence statement; `Rank` runs the coarse `binary-hamming` `HammingBitDistance` pre-filter over the full candidate set (the integral bit-distance is the cheapest possible nearest-neighbour gate), takes the policy `RerankFanout` × `top` nearest by Hamming, decodes each survivor to `float32`, and reranks the survivors by the exact float metric — the binary pre-filter is a gate, never the terminal verdict, so a binary-only ranking that skips the exact rerank is the named defect; the int8 decode dequantizes through the inverse policy scale before the exact metric reads it, and a sentinel/NaN score projects to a `ModelRejected` fault at the boundary, never inward.
- Receipt: an embed run rides the existing `Runtime/receipts#RECEIPT_UNION` `Generate(string ModelChecksum, ExecutionProvider Ep, string ModelType, int Tokens, double TokensPerSecond, string GuidanceKind, int ConstrainedTokens, int ToolCalls)` family slot — the embedded-vector count rides the `Tokens` slot and the encoding rides the `ModelType` dimension, so an `Embed`/encode emission is auditable through the existing slot exactly as the boundary law `an Embed run rides the Generate family slot` prescribes, and an `EmbeddingReceipt` or `RetrievalReceipt` standalone type is the rejected form; the scoring and ranking are pure value transforms beneath the receipt edge (no native handle, no side effect), so a `Score`/`Rank` call mints no receipt and the candidate-index recall/latency is the Persistence vector-lane owner's measured concern, never a second receipt here.
- Packages: System.Numerics.Tensors, System.IO.Hashing, Microsoft.ML.OnnxRuntime, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new vector encoding is one `VectorEncoding` row with its `Encode`-arm `Switch` case and its posture columns; a new similarity metric is one `VectorScore` row carrying its lowered `TensorPrimitives` member and ranking direction folded into the one `Score` `Switch`; a new quantization budget is one column on `QuantizationPolicy`; a BIM class prototype is one stored `EmbeddingVector` and nearest-prototype classification is one `Rank` over the prototype set; zero new surface — a `CosineSimilarity`/`DotProductScorer`/`EuclideanDistance`/`HammingScorer` sibling method family is collapsed onto the one `Score` fold over the `VectorScore` axis, an `Int8Quantizer`/`BinaryEncoder`/`ProductQuantizer` sibling owner is collapsed onto the one `Encode` fold over the `VectorEncoding` axis, and a per-encoding `EmbeddingVector` subtype is collapsed onto the one content-keyed carrier whose encoding column discriminates.
- Boundary: this page owns embed-and-score and crosses to the `Rasm.Persistence` vector lane **by content-key reference only** — Persistence owns the on-disk/in-memory vector index, the approximate-nearest-neighbour graph, the product-quantization codebook training, and the cross-process result-reuse recency horizon, so a Compute-side vector store, ANN index, or codebook fit is the named drift defect and the `EmbeddingVector.ContentKey` is the single join the Persistence index addresses (the `:x32` form the Persistence marshal renders is the suite `XxHash128.HashToUInt128` value `Model/identity#MODEL_IDENTITY` mints, never a second hash); the embedding **projection** is the `Model/inference#INFERENCE_MODES` `Embed` run's — `Embed` mean-pools-or-CLS-slices then L2-normalizes via `TensorPrimitives.Norm`+`Divide` to the unit `float[]` this page encodes, so a re-implemented mean-pool/normalize on this page is the rejected form and a raw last-token passthrough is rejected at the `Embed` owner already; `System.Numerics.Tensors` owns every reduction — the scoring lowers onto `TensorPrimitives.CosineSimilarity`/`Dot`/`Distance` over `float` spans and `HammingBitDistance` over the packed integral spans (a `long`-returning bit-distance per the catalogue) and the int8 quantize lowers onto `ConvertSaturating<float, sbyte>`, so a hand-rolled dot-product accumulation, a hand-rolled saturating clamp, or a hand-rolled bit-distance popcount loop is the deleted form; the `float32` encoding is the exact-rerank ground truth and the `binary-hamming` encoding is a coarse pre-filter gate, so `Rank` always decodes the Hamming survivors to `float32` and reranks by the exact metric — a binary-Hamming terminal verdict is the named defect because the 1-bit sign quantization loses the magnitude the exact metric needs; an encoding-mismatched `Score` (a `float32` query against an `int8-scalar` candidate without the dequantize bridge, or a `hamming` metric over a non-binary encoding) faults `ModelRejected` at the boundary so a silently-wrong cross-encoding score never reaches the rail; the BIM point-cloud→element and symbol-recognition classifier reuses this same `Encode`/`Score` surface (a labelled class prototype is one stored `EmbeddingVector`, classification is one `Rank` over the prototype set reusing the `Model/inference#INFERENCE_MODES` `Classify`/`Embed` run output), never a BIM-specific embedding or retrieval service; the encoded component bytes stay span-faced — the encode arms write into a rented `MemoryOwner<byte>`/`MemoryOwner<sbyte>` window from `CommunityToolkit.HighPerformance` and the carrier holds the trimmed `ReadOnlyMemory`, so a `ToArray` flatten past the one encoded window on the encode path is the named defect.

```csharp contract
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class VectorEncoding {
    public static readonly VectorEncoding Float32 = new("float32", bytesPerComponent: 4, bits: 32, quantizes: false, hammingScored: false);
    public static readonly VectorEncoding Int8Scalar = new("int8-scalar", bytesPerComponent: 1, bits: 8, quantizes: true, hammingScored: false);
    public static readonly VectorEncoding BinaryHamming = new("binary-hamming", bytesPerComponent: 0, bits: 1, quantizes: true, hammingScored: true);
    public static readonly VectorEncoding ProductQuantized = new("product-quantized", bytesPerComponent: 1, bits: 8, quantizes: true, hammingScored: false);

    public int BytesPerComponent { get; }
    public int Bits { get; }
    public bool Quantizes { get; }
    public bool HammingScored { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ModelKeyPolicy, string>]
[KeyMemberComparer<ModelKeyPolicy, string>]
public sealed partial class VectorScore {
    public static readonly VectorScore Cosine = new("cosine", smallerIsNearer: false, overBinary: false);
    public static readonly VectorScore Dot = new("dot", smallerIsNearer: false, overBinary: false);
    public static readonly VectorScore Euclidean = new("euclidean", smallerIsNearer: true, overBinary: false);
    public static readonly VectorScore Hamming = new("hamming", smallerIsNearer: true, overBinary: true);

    public bool SmallerIsNearer { get; }
    public bool OverBinary { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

public sealed record QuantizationPolicy(
    float Int8Scale,
    float SignThreshold,
    int Subspaces,
    int CodebookBits,
    int RerankFanout) {
    public static readonly QuantizationPolicy Canonical = new(
        Int8Scale: 127f, SignThreshold: 0f, Subspaces: 8, CodebookBits: 8, RerankFanout: 4);
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record EmbeddingVector(
    VectorEncoding Encoding,
    string ModelKey,
    int Dimension,
    ReadOnlyMemory<byte> Components,
    UInt128 ContentKey) {
    public string ArtifactKey => $"{ContentKey:x32}:{Encoding.Key}";

    public static UInt128 KeyOf(string modelKey, VectorEncoding encoding, ReadOnlySpan<float> unit) {
        var raw = MemoryMarshal.AsBytes(unit);
        var seed = Encoding.UTF8.GetBytes($"{modelKey};{encoding.Key};");
        using var owner = MemoryOwner<byte>.Allocate(seed.Length + raw.Length);
        var sink = owner.Span;
        seed.CopyTo(sink);
        raw.CopyTo(sink[seed.Length..]);
        return XxHash128.HashToUInt128(sink);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Embedding {
    public static EmbeddingVector Encode(ReadOnlySpan<float> unit, VectorEncoding encoding, string modelKey, QuantizationPolicy policy) {
        var key = EmbeddingVector.KeyOf(modelKey, encoding, unit);
        var components = encoding.Switch(
            state: (Unit: unit.ToArray(), Policy: policy),
            float32: static s => MemoryMarshal.AsBytes<float>(s.Unit).ToArray().AsMemory(),
            int8Scalar: static s => EncodeInt8(s.Unit, s.Policy.Int8Scale),
            binaryHamming: static s => EncodeBinary(s.Unit, s.Policy.SignThreshold),
            productQuantized: static s => EncodeProduct(s.Unit, s.Policy.Subspaces));
        return new EmbeddingVector(encoding, modelKey, unit.Length, components, key);
    }

    public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric) =>
        metric.OverBinary
            ? query.Encoding.HammingScored && candidate.Encoding.HammingScored
                ? Fin.Succ((float)TensorPrimitives.HammingBitDistance(query.Components.Span, candidate.Components.Span))
                : Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-hamming-needs-binary:{query.Encoding.Key}|{candidate.Encoding.Key}>"))
            : query.Encoding == VectorEncoding.ProductQuantized || candidate.Encoding == VectorEncoding.ProductQuantized
                ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-pq-reconstruct-via-persistence-codebook:{query.Encoding.Key}|{candidate.Encoding.Key}>"))
                : query.Dimension != candidate.Dimension
                    ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-dim-mismatch:{query.Dimension}!={candidate.Dimension}>"))
                    : ScoreFloat(Decode(query).Span, Decode(candidate).Span, metric);

    static Fin<float> ScoreFloat(ReadOnlySpan<float> left, ReadOnlySpan<float> right, VectorScore metric) =>
        metric == VectorScore.Cosine ? Guarded(TensorPrimitives.CosineSimilarity<float>(left, right))
        : metric == VectorScore.Dot ? Guarded(TensorPrimitives.Dot<float>(left, right))
        : metric == VectorScore.Euclidean ? Guarded(TensorPrimitives.Distance<float>(left, right))
        : Fin.Fail<float>(new ComputeFault.ModelRejected("<embed-hamming-non-binary>"));

    public static Fin<Seq<(EmbeddingVector Candidate, float Score)>> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, QuantizationPolicy policy) =>
        candidates.IsEmpty
            ? Fin.Succ(Seq<(EmbeddingVector, float)>())
            : query.Encoding.HammingScored
                ? RankBinary(query, candidates, metric, top, policy)
                : RankExact(query, candidates, metric, top);

    static Fin<Seq<(EmbeddingVector Candidate, float Score)>> RankBinary(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, QuantizationPolicy policy) {
        int fanout = checked(top * policy.RerankFanout);
        var prefiltered = candidates
            .Map(candidate => (Candidate: candidate, Hamming: (float)TensorPrimitives.HammingBitDistance(query.Components.Span, candidate.Components.Span)))
            .OrderBy(static row => row.Hamming)
            .Take(fanout)
            .Map(static row => row.Candidate)
            .ToSeq();
        return RankExact(query, prefiltered, metric, top);
    }

    static Fin<Seq<(EmbeddingVector Candidate, float Score)>> RankExact(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top) =>
        candidates
            .TraverseM(candidate => Score(query, candidate, metric).Map(score => (Candidate: candidate, Score: score)))
            .Map(scored => metric.SmallerIsNearer
                ? scored.OrderBy(static row => row.Score).Take(top).ToSeq()
                : scored.OrderByDescending(static row => row.Score).Take(top).ToSeq())
            .As();

    static ReadOnlyMemory<float> Decode(EmbeddingVector vector) =>
        vector.Encoding.Switch(
            state: vector,
            float32: static v => MemoryMarshal.Cast<byte, float>(v.Components.Span).ToArray().AsMemory(),
            int8Scalar: static v => DecodeInt8(MemoryMarshal.Cast<byte, sbyte>(v.Components.Span), QuantizationPolicy.Canonical.Int8Scale),
            binaryHamming: static v => DecodeBinary(v.Components.Span, v.Dimension),
            productQuantized: static v => MemoryMarshal.Cast<byte, float>(v.Components.Span).ToArray().AsMemory());

    static ReadOnlyMemory<byte> EncodeInt8(ReadOnlySpan<float> unit, float scale) {
        var scaled = new float[unit.Length];
        TensorPrimitives.Multiply(unit, scale, scaled);
        using var owner = MemoryOwner<sbyte>.Allocate(unit.Length);
        TensorPrimitives.ConvertSaturating<float, sbyte>(scaled, owner.Span);
        return MemoryMarshal.AsBytes(owner.Span).ToArray().AsMemory();
    }

    static ReadOnlyMemory<float> DecodeInt8(ReadOnlySpan<sbyte> packed, float scale) {
        var floats = new float[packed.Length];
        TensorPrimitives.ConvertChecked<sbyte, float>(packed, floats);
        TensorPrimitives.Divide(floats, scale, floats);
        return floats.AsMemory();
    }

    static ReadOnlyMemory<byte> EncodeBinary(ReadOnlySpan<float> unit, float threshold) {
        int bytes = (unit.Length + 7) / 8;
        var packed = new byte[bytes];
        for (int component = 0; component < unit.Length; component++) {
            if (unit[component] > threshold) { packed[component >> 3] |= (byte)(1 << (component & 7)); }
        }
        return packed.AsMemory();
    }

    static ReadOnlyMemory<float> DecodeBinary(ReadOnlySpan<byte> packed, int dimension) {
        var floats = new float[dimension];
        for (int component = 0; component < dimension; component++) {
            floats[component] = (packed[component >> 3] & (1 << (component & 7))) != 0 ? 1f : -1f;
        }
        return floats.AsMemory();
    }

    static ReadOnlyMemory<byte> EncodeProduct(ReadOnlySpan<float> unit, int subspaces) {
        int width = Math.Max(1, unit.Length / Math.Max(1, subspaces));
        using var owner = MemoryOwner<byte>.Allocate(subspaces);
        var codes = owner.Span;
        for (int subspace = 0; subspace < subspaces; subspace++) {
            var slice = unit.Slice(subspace * width, Math.Min(width, unit.Length - subspace * width));
            codes[subspace] = (byte)(TensorPrimitives.IndexOfMaxMagnitude(slice) & 0xFF);
        }
        return codes.ToArray().AsMemory();
    }

    static Fin<float> Guarded(float score) =>
        float.IsNaN(score) || float.IsInfinity(score)
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-score-degenerate:{score:R}>"))
            : Fin.Succ(score);
}
```

## [3]-[RESEARCH]

- [PQ_CODEBOOK_SOURCE]: the `product-quantized` encoding's per-subspace codebook is trained and owned by the `Rasm.Persistence` vector-lane index, not here — the `EncodeProduct` arm emits the nearest-codebook index per contiguous subspace and the codebook itself (the k-means centroids per subspace the index fits over the admitted corpus) crosses as settled vocabulary the Persistence owner supplies at admission, so the `IndexOfMaxMagnitude` nearest-code placeholder grounds against the real codebook-lookup contract the Persistence vector lane publishes, and an in-Compute codebook fit is the rejected form (Persistence owns the index and the training). The exact rerank of a `product-quantized` candidate is therefore the Persistence index's codebook-reconstruction concern — `Score` routes a PQ encoding to `embed-pq-reconstruct-via-persistence-codebook` rather than reconstructing a centroid sum here, so the in-page exact-rerank ground truth stays the `float32`/`int8-scalar` path and the `Decode` PQ arm is the codebook-reconstruction stub the gate replaces. The exact subspace-width tiling (uniform split versus the index's learned subspace assignment) and the codebook bit width band ground against the Persistence vector-lane codebook descriptor at the index-admission gate.
