# [COMPUTE_EMBEDDING]

`Rasm.Compute`'s embedding lane is the retrieval half of the inference spine: it owns one encoding axis (`VectorEncoding`), one metric axis (`VectorScore`), one content-keyed carrier (`EmbeddingVector`), and one `Encode`/`Score`/`Rank` fold over the `System.Numerics.Tensors` `TensorPrimitives` SIMD surface. It projects the L2-normalized unit `float[]` the `Model/inference#INFERENCE_MODES` `Embed` run produces down the `VectorEncoding` axis — `float32` exact-rerank ground truth, `float16` the `ConvertToHalf` 2× residence widening to near-`float32`, `int8-scalar` the `ConvertSaturating` 4× index residence, `binary-hamming` the 32× sign-packed coarse gate, `product-quantized` the codebook-coded residence for the largest corpora — and scores and ranks what production already minted. RETRIEVAL only: vector PRODUCTION is the AppHost-governed `IEmbeddingGenerator` lane where Compute supplies the inner provider and never the builder, and no vector store, ANN index, codebook fit, or recency horizon lives here.

Every artifact content-keys through `Rasm.Domain.ContentHash.Of` over a length-framed UTF-8 preimage containing the source `ModelIdentity.Key`, encoding key, float dimension, codebook identity, and encoded component bytes, so a deterministic re-encode addresses the same key the `Rasm.Persistence` vector index deduplicates against and a posture change never aliases two artifacts. Crossing to Persistence is content-key reference only: Persistence owns the on-disk/in-memory index, the pgvector HNSW graph, product-quantization training, amortized corpus scan, and cross-process recency horizon. `ProductCodebook` arrives settled from `Query/retrieval#VECTOR_CODEBOOK`, so PQ here is nearest-centroid assignment and centroid reconstruction over a supplied identity-matching codebook. `binary-hamming` carries sign only; its `HammingBitDistance` coarse gate returns content keys, Persistence resolves survivor fine forms, and `Rank` performs the exact rerank. Same `Encode`/`Score`/`Rank` surface backs point-cloud and symbol classifiers without a BIM-specific retrieval service. Page is host-local and carries no `TS_PROJECTION`.

## [01]-[INDEX]

- [01]-[EMBEDDING]: the `VectorEncoding` encoding axis, the behavior-bearing `VectorScore` metric axis, the `EmbeddingVector` content-keyed carrier, the `VectorPolicy` sign/rerank values, and the `Encode`/`Score`/`Rank` fold over the `TensorPrimitives` SIMD surface crossing to the Persistence vector lane by content-key reference.

## [02]-[EMBEDDING]

- Owner: `VectorEncoding` `[SmartEnum<string>]` is the one closed encoding axis, each row carrying `BytesPerComponent` and the `HammingScored` discriminant folded into one overflow-safe `ByteLength(dimension, subspaces)` formula. `VectorScore` `[SmartEnum<string>]` is the one closed metric axis, each row carrying ranking direction, coarse-gate posture, and its complete score delegate. `VectorPolicy` owns binary threshold and rerank fanout. `EmbeddingVector` is the content-keyed buffer carrier — owned encoded bytes, encoding tag, source `ModelIdentity.Key`, float dimension, retained codebook identity, and `ContentHash` key — whose `Admit` boundary snapshots bytes before re-checking byte length, canonical padding, PQ identity, and content-key echo. `Embedding` owns the static `Encode`/`Score`/`Rank` fold.
- Cases: `VectorEncoding` rows `float32` (raw unit vector, exact-rerank ground truth) · `float16` (`ConvertToHalf`-narrowed, 2× smaller, high-accuracy residence widening to near-`float32`) · `int8-scalar` (`ConvertSaturating`-quantized symmetric int8, 4× smaller, default index residence) · `binary-hamming` (sign-thresholded 1-bit-per-component packed, 32× smaller, coarse pre-filter floor) · `product-quantized` (one code byte per subspace over the Persistence-trained codebook, largest-corpus residence); `VectorScore` rows `cosine` (unit-vector default, larger-is-nearer) · `dot` (inner product over normalized vectors, larger-is-nearer) · `euclidean` (smaller-is-nearer) · `l1` (Manhattan, smaller-is-nearer) · `hamming` (over the packed bit encoding, smaller-is-nearer, the coarse-gate metric) · `jaccard` (over the packed bit encoding, smaller-is-nearer) — full parity with the six-metric Persistence `VectorMetric` axis, so every index-supported distance has a Compute rerank arm.
- Entry: `Encode` projects the `Embed` unit output onto the encoding row and content-keys the result; `Score` applies the selected metric row to one admitted pair; `Rank` bounded-top-K ranks a candidate set and widens a coarse binary gate to `top × VectorPolicy.RerankFanout`. `Fin<T>` aborts on an oversized encoding, a cross-model pair, a binary metric over a non-packed encoding, a packed-width or dimension mismatch, a `product-quantized` operand without its codebook or against a dimension- or identity-disagreeing codebook, or a degenerate (`NaN`/`Inf`) score.
- Auto: `Encode` dispatches the `VectorEncoding` row through one generated total `Switch` threaded on the `ReadOnlyMemory<float>` source as state so the unit vector reaches every arm with zero managed copy — a `ToArray` lift into the state is the deleted form; `float32` and `float16` canonicalize each component to little-endian bytes, the `int8-scalar` arm scales into a pooled `SpanOwner<float>` scratch and saturates through `TensorPrimitives.ConvertSaturating<float, sbyte>` (never a hand-rolled clamp-and-cast loop), and the `product-quantized` arm assigns each sub-vector to its nearest codebook centroid by `TensorPrimitives.Distance` (genuine k-means assignment, never the magnitude-extremum index an `IndexOfMaxMagnitude` placeholder emits); the address then folds the encoded bytes plus dimension and codebook identity, baking the quantization posture in. `Score` delegates the complete pair operation to the metric row — packed lowering uses `HammingBitDistance` or the `BitwiseAnd`/`BitwiseOr` + `PopCount` intersection-over-union fold; dense lowering uses `CosineSimilarity`/`Dot`/`Distance` or the `Subtract`+`SumOfMagnitudes` scratch fold — so a new metric is one behavior-complete `VectorScore` row. `Rank` decodes-and-scores every candidate, short-circuiting on the first fault, then selects through a content-key-stable bounded `PriorityQueue` heap (O(n log k), never a full `OrderBy` sort); the `int8-scalar` decode dequantizes and the `product-quantized` decode reconstructs by concatenating centroids before any exact metric reads, and a degenerate score projects to `ModelRejected` at the boundary, never inward.
- Receipt: an `Encode` emission mints the `Runtime/receipts#RECEIPT_UNION` `Embedding` case — model checksum, encoding row key, embedded dimension, and encoded byte length, each field carrying its own meaning (a `Generate`-slot overload smuggling encoding into `ModelType` and dimension into `Tokens` is the deleted form; a receipt consumer must distinguish an embedding artifact from a token run); `Score` and `Rank` are pure value transforms beneath the receipt edge and mint none, and candidate-index recall/latency is the Persistence vector-lane owner's measured concern.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), Rasm.Persistence (project), BCL inbox
- Growth: a new vector encoding is one `VectorEncoding` row with its `Encode`/`DecodeFloat` `Switch` arm and its `BytesPerComponent`/`ByteLength` posture; a new metric is one behavior-complete `VectorScore` row; a new encode-or-rerank value is one column on `VectorPolicy`; a BIM class prototype is one stored `EmbeddingVector` and nearest-prototype classification one `Rank`; zero new surface — a `CosineSimilarity`/`DotProductScorer`/`HammingScorer` method family collapses onto the one `Score` fold, an `Int8Quantizer`/`BinaryEncoder`/`ProductQuantizer` owner onto the one `Encode` fold, and a per-encoding `EmbeddingVector` subtype onto the one carrier whose encoding column discriminates.
- Boundary: this page owns embed-and-score and crosses to the `Rasm.Persistence` vector lane by content-key reference only — Persistence owns the index, the pgvector HNSW graph, the codebook training, the amortized asymmetric-distance scan, and the recency horizon, so a Compute-side vector store, ANN index, codebook fit, or horizon is the named drift defect and `EmbeddingVector.ContentKey` is the single join the index addresses (the `:x32` form the Persistence marshal renders is the suite `XxHash128` value, never a second hash). Embedding projection is the `Model/inference#INFERENCE_MODES` `Embed` run's — it applies the selected pooling row then L2-normalizes via `TensorPrimitives.Norm`+`Divide` — so a re-implemented pool/normalize here is the rejected form. `System.Numerics.Tensors` owns every reduction and conversion — scoring lowers onto `CosineSimilarity`/`Dot`/`Distance` over `float` spans, `Subtract`+`SumOfMagnitudes` for the `l1` scratch fold, `HammingBitDistance` and the `BitwiseAnd`/`BitwiseOr`+`PopCount` Jaccard fold over the packed integral spans, float16 onto `ConvertToHalf`/`ConvertToSingle`, int8 onto `ConvertSaturating<float, sbyte>`/`ConvertChecked<sbyte, float>`, PQ assignment onto `Distance` — so a hand-rolled dot, half conversion, saturating clamp, bit-distance popcount, or centroid-distance loop is the deleted form, and only canonical scalar byte framing, sign-pack bit-twiddle, and nearest-centroid argmin remain span-kernel exemptions because no single `TensorPrimitives` member owns those operations. Two-stage retrieval is honest — `binary-hamming` is sign-only, so a `Score` against a dense metric is structurally unavailable, and the genuine rerank is a second `Rank` over the survivors' fine forms Persistence resolves from the coarse `Rank`'s returned keys, sized `top × policy.RerankFanout`; a binary-Hamming terminal verdict skipping the fine rerank and an in-page float rerank of a ±1 decode are both named defects. Dense `float16`/`int8-scalar` rows keep magnitude, so the asymmetric path scores the full-precision `float32` query against the widened-half or dequantized-int8 candidate and a cross-encoding `Score` is a supported bridge, never a fault — only a binary metric over a non-packed encoding, a packed-width or dimension mismatch, a `product-quantized` operand without its codebook or against a codebook whose `Id` disagrees with the carrier's retained `CodebookId`, or a degenerate score faults `ModelRejected` at the boundary so a silently-wrong score never reaches the rail. `ProductCodebook` (subspaces, per-subspace centroids, code width, identity) arrives settled from the Persistence vector-lane owner — this page does nearest-centroid encode and centroid-reconstruction decode over it but never trains it, and the amortized asymmetric-distance corpus scan is the index's concern while the bounded rerank here reconstructs-and-scores. Content key folds the model key, encoding, float dimension, codebook identity, and canonical encoded bytes so the quantization posture and codebook are part of the address, a re-trained codebook re-keys every `product-quantized` artifact, and the carrier RETAINS `CodebookId` as a field so PQ scoring proves its reconstruction codebook by identity — a same-dimension codebook with different centroids reconstructs plausible garbage, and an identity carried only inside an opaque hash cannot be re-checked at score time; `EmbeddingVector.Admit` snapshots the supplied bytes before re-validating `ByteLength` and the content-key echo so a mutable caller cannot change a rehydrated carrier after admission. Encoded component bytes the carrier holds are an owned array — the transient int8 scaling scratch is a pooled `SpanOwner<float>` disposed in-arm, but a pooled `MemoryOwner` rent can never back the immutable carrier because the pool reuses the buffer (a use-after-free, the deleted form).

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

    public long ByteLength(int dimension, int subspaces) =>
        this == BinaryHamming ? ((long)dimension + 7L) / 8L
        : this == ProductQuantized ? subspaces
        : (long)dimension * BytesPerComponent;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorScore {
    public static readonly VectorScore Cosine = new("cosine", false, false, Embedding.Cosine);
    public static readonly VectorScore Dot = new("dot", false, false, Embedding.Dot);
    public static readonly VectorScore Euclidean = new("euclidean", true, false, Embedding.Euclidean);
    public static readonly VectorScore L1 = new("l1", true, false, Embedding.L1);
    public static readonly VectorScore Hamming = new("hamming", true, true, Embedding.Hamming);
    public static readonly VectorScore Jaccard = new("jaccard", true, true, Embedding.Jaccard);

    public bool SmallerIsNearer { get; }
    public bool Coarse { get; }

    [UseDelegateFromConstructor]
    public partial Fin<float> Apply(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook);
}

// --- [CONSTANTS] -----------------------------------------------------------------------

[ComplexValueObject]
public sealed partial class VectorPolicy {
    public float SignThreshold { get; }

    public int RerankFanout { get; }

    public static readonly VectorPolicy Canonical = Create(signThreshold: 0f, rerankFanout: 4);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float signThreshold, ref int rerankFanout) =>
        validationError = float.IsFinite(signThreshold) && signThreshold is >= -1f and <= 1f && rerankFanout > 0
            ? null
            : new ValidationError(message: $"<vector-policy:{signThreshold}:{rerankFanout}>");
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed class EmbeddingVector : IEquatable<EmbeddingVector> {
    private EmbeddingVector(VectorEncoding encoding, string modelKey, int dimension, UInt128 codebookId, ReadOnlyMemory<byte> components, UInt128 contentKey) =>
        (Encoding, ModelKey, Dimension, CodebookId, Components, ContentKey) = (encoding, modelKey, dimension, codebookId, components, contentKey);

    public VectorEncoding Encoding { get; }
    public string ModelKey { get; }
    public int Dimension { get; }
    public UInt128 CodebookId { get; }
    public ReadOnlyMemory<byte> Components { get; }
    public UInt128 ContentKey { get; }

    public string ArtifactKey => $"{ContentKey:x32}:{Encoding.Key}";

    // Dimension stays in the content preimage because `binary-hamming` `ceil(dimension / 8)` maps multiple dimensions to one byte length.
    public static UInt128 KeyOf(string modelKey, VectorEncoding encoding, int dimension, ReadOnlySpan<byte> components, UInt128 codebookId = default) {
        ArrayBufferWriter<byte> preimage = new();
        Frame(preimage, modelKey);
        Frame(preimage, encoding.Key);
        Span<byte> scalar = preimage.GetSpan(20);
        BinaryPrimitives.WriteUInt128LittleEndian(scalar, codebookId);
        BinaryPrimitives.WriteInt32LittleEndian(scalar[16..], dimension);
        preimage.Advance(20);
        preimage.Write(components);
        return ContentHash.Of(preimage.WrittenSpan);
    }

    public static Fin<EmbeddingVector> Admit(VectorEncoding encoding, string modelKey, int dimension, int subspaces, ReadOnlyMemory<byte> components, UInt128 codebookId, UInt128 expectedKey) {
        UInt128 retained = encoding == VectorEncoding.ProductQuantized ? codebookId : UInt128.Zero;
        byte[] owned = components.ToArray();
        if (string.IsNullOrWhiteSpace(modelKey) || dimension <= 0
            || encoding == VectorEncoding.ProductQuantized && (subspaces <= 0 || retained == UInt128.Zero)
            || owned.Length != encoding.ByteLength(dimension, Math.Max(1, subspaces))
            || encoding == VectorEncoding.BinaryHamming && !CanonicalPadding(dimension, owned)) {
            return Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-carrier:{encoding.Key}:{dimension}:{owned.Length}>"));
        }
        UInt128 key = KeyOf(modelKey, encoding, dimension, owned, retained);
        return key == expectedKey
            ? Fin.Succ(new EmbeddingVector(encoding, modelKey, dimension, retained, owned, key))
            : Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-key-echo:{key:x32}!={expectedKey:x32}>"));
    }

    internal static EmbeddingVector Owned(VectorEncoding encoding, string modelKey, int dimension, UInt128 codebookId, byte[] components) =>
        new(encoding, modelKey, dimension, codebookId, components, KeyOf(modelKey, encoding, dimension, components, codebookId));

    public bool Equals(EmbeddingVector? other) => other is not null && ContentKey == other.ContentKey;
    public override bool Equals(object? other) => other is EmbeddingVector vector && Equals(vector);
    public override int GetHashCode() => ContentKey.GetHashCode();
    public static bool operator ==(EmbeddingVector? left, EmbeddingVector? right) => object.Equals(left, right);
    public static bool operator !=(EmbeddingVector? left, EmbeddingVector? right) => !object.Equals(left, right);

    // `System.Text.Encoding` is spelled fully: the instance property `Encoding` shadows the type's simple name inside this class.
    static void Frame(ArrayBufferWriter<byte> preimage, string value) {
        int bytes = System.Text.Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), bytes);
        preimage.Advance(4);
        preimage.Advance(System.Text.Encoding.UTF8.GetBytes(value, preimage.GetSpan(bytes)));
    }

    static bool CanonicalPadding(int dimension, ReadOnlySpan<byte> components) =>
        dimension % 8 is 0 || (components[^1] & ~((1 << (dimension % 8)) - 1)) is 0;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Embedding {
    // int8 symmetric scale is a protocol constant: an L2-normalized vector lies in [-1, 1], so 127 maps the range onto the sbyte grid and dividing by 127 recovers magnitude on decode.
    const float Int8Scale = 127f;

    delegate Fin<float> DenseScore(ReadOnlySpan<float> left, ReadOnlySpan<float> right);
    delegate Fin<float> PackedScore(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right);

    public static Fin<EmbeddingVector> Encode(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, VectorPolicy policy, Option<ProductCodebook> codebook = default) {
        if (string.IsNullOrWhiteSpace(modelKey) || unit.IsEmpty || !TensorPrimitives.IsFiniteAll(unit.Span) || MathF.Abs(TensorPrimitives.Norm<float>(unit.Span) - 1f) > 1e-4f) {
            return Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-unit:{unit.Length}>"));
        }
        if (encoding == VectorEncoding.ProductQuantized && codebook.Case is not ProductCodebook) {
            return Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-pq-needs-codebook:{modelKey}>"));
        }
        if (codebook.Case is ProductCodebook book && encoding == VectorEncoding.ProductQuantized && CodebookFault(book, unit.Length) is ComputeFault fault) {
            return Fin.Fail<EmbeddingVector>(fault);
        }
        int subspaces = codebook.Match(Some: static book => book.Subspaces, None: static () => 1);
        long bytes = encoding.ByteLength(unit.Length, subspaces);
        return bytes is <= 0L or > Array.MaxLength
            ? Fin.Fail<EmbeddingVector>(new ComputeFault.ModelRejected($"<embed-byte-length:{encoding.Key}:{unit.Length}:{bytes}>"))
            : Fin.Succ(Mint(unit, encoding, modelKey, policy, codebook));
    }

    public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric, Option<ProductCodebook> codebook = default) =>
        !StringComparer.Ordinal.Equals(query.ModelKey, candidate.ModelKey)
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-model:{query.ModelKey}!={candidate.ModelKey}>"))
            : query.Dimension != candidate.Dimension
                ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-dimension:{query.Dimension}!={candidate.Dimension}>"))
                : metric.Apply(query, candidate, codebook);

    public static Fin<Seq<(EmbeddingVector Candidate, float Score)>> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, VectorPolicy policy, Option<ProductCodebook> codebook = default) {
        long selected = metric.Coarse ? (long)top * policy.RerankFanout : top;
        return top <= 0 || selected > int.MaxValue
            ? Fin.Fail<Seq<(EmbeddingVector Candidate, float Score)>>(new ComputeFault.ModelRejected($"<embed-rank-top:{top}>"))
            : candidates
                .TraverseM(candidate => Score(query, candidate, metric, codebook).Map(score => (Candidate: candidate, Score: score)))
                .Map(scored => TopK(scored, (int)selected, metric.SmallerIsNearer))
                .As();
    }

    static EmbeddingVector Mint(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, VectorPolicy policy, Option<ProductCodebook> codebook) {
        byte[] components = encoding.Switch(
            state: (Unit: unit, Policy: policy, Codebook: codebook),
            float32: static s => EncodeFloat(s.Unit.Span),
            float16: static s => EncodeHalf(s.Unit.Span),
            int8Scalar: static s => EncodeInt8(s.Unit.Span),
            binaryHamming: static s => EncodeBinary(s.Unit.Span, s.Policy.SignThreshold),
            productQuantized: static s => s.Codebook.Case is ProductCodebook book ? EncodeProduct(s.Unit.Span, book) : []);
        UInt128 codebookId = encoding == VectorEncoding.ProductQuantized
            ? codebook.Match(Some: static book => book.Id, None: static () => UInt128.Zero)
            : UInt128.Zero;
        return EmbeddingVector.Owned(encoding, modelKey, unit.Length, codebookId, components);
    }

    internal static Fin<float> Cosine(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        Dense(query, candidate, codebook, static (left, right) => Guarded(TensorPrimitives.CosineSimilarity(left, right)));

    internal static Fin<float> Dot(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        Dense(query, candidate, codebook, static (left, right) => Guarded(TensorPrimitives.Dot(left, right)));

    internal static Fin<float> Euclidean(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        Dense(query, candidate, codebook, static (left, right) => Guarded(TensorPrimitives.Distance(left, right)));

    internal static Fin<float> L1(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        Dense(query, candidate, codebook, Manhattan);

    internal static Fin<float> Hamming(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> _) =>
        Packed(query, candidate, static (left, right) => Fin.Succ((float)TensorPrimitives.HammingBitDistance(left, right)));

    internal static Fin<float> Jaccard(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> _) =>
        Packed(query, candidate, JaccardBits);

    static Fin<float> Dense(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook, DenseScore reduce) =>
        query.Encoding.HammingScored || candidate.Encoding.HammingScored
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-dense-needs-magnitude:{query.Encoding.Key}|{candidate.Encoding.Key}>"))
            : PqFault(query, candidate, codebook) is ComputeFault fault
                ? Fin.Fail<float>(fault)
                : reduce(DecodeFloat(query, codebook).Span, DecodeFloat(candidate, codebook).Span);

    static Fin<float> Packed(EmbeddingVector query, EmbeddingVector candidate, PackedScore reduce) =>
        !(query.Encoding.HammingScored && candidate.Encoding.HammingScored)
            ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-binary-needs-packed:{query.Encoding.Key}|{candidate.Encoding.Key}>"))
            : query.Dimension != candidate.Dimension || query.Components.Length != candidate.Components.Length
                ? Fin.Fail<float>(new ComputeFault.ModelRejected($"<embed-binary-width:{query.Dimension}!={candidate.Dimension}>"))
                : reduce(query.Components.Span, candidate.Components.Span);

    static Fin<float> JaccardBits(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) {
        using SpanOwner<byte> scratch = SpanOwner<byte>.Allocate(left.Length);
        TensorPrimitives.BitwiseAnd(left, right, scratch.Span);
        long shared = TensorPrimitives.PopCount<byte>(scratch.Span);
        TensorPrimitives.BitwiseOr(left, right, scratch.Span);
        long union = TensorPrimitives.PopCount<byte>(scratch.Span);
        return union == 0L ? Fin.Succ(0f) : Guarded(1f - ((float)shared / union));
    }

    static Fin<float> Manhattan(ReadOnlySpan<float> left, ReadOnlySpan<float> right) {
        using SpanOwner<float> scratch = SpanOwner<float>.Allocate(left.Length);
        TensorPrimitives.Subtract(left, right, scratch.Span);
        return Guarded(TensorPrimitives.SumOfMagnitudes<float>(scratch.Span));
    }

    // PQ readiness guard: a product-quantized operand needs its OWN codebook present, dimension-matched, and identity-matched before any reduction reads the reconstructed spans; null is the no-fault verdict.
    static ComputeFault? PqFault(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        query.Encoding != VectorEncoding.ProductQuantized && candidate.Encoding != VectorEncoding.ProductQuantized
            ? null
            : codebook.Case is ProductCodebook book
                ? CodebookFault(book, query.Dimension) ?? Mismatched(query, book) ?? Mismatched(candidate, book)
                : new ComputeFault.ModelRejected($"<embed-pq-needs-codebook:{query.ArtifactKey}|{candidate.ArtifactKey}>");

    // `ProductCodebook.Of` is the sole factory: layout positivity, centroid length, finite content, and `Id` echo are admitted invariants — only the operand-dimension join re-checks, so a per-score O(centroids) re-hash never rides the rank loop.
    static ComputeFault? CodebookFault(ProductCodebook book, int dimension) =>
        (long)book.Subspaces * book.SubspaceDim != dimension
            ? new ComputeFault.ModelRejected($"<embed-pq-layout:{dimension}:{book.Subspaces}:{book.SubspaceDim}:{book.CodesPerSubspace}>")
            : null;

    static ComputeFault? Mismatched(EmbeddingVector vector, ProductCodebook book) =>
        vector.Encoding == VectorEncoding.ProductQuantized && vector.CodebookId != book.Id
            ? new ComputeFault.ModelRejected($"<embed-pq-codebook:{vector.CodebookId:x32}!={book.Id:x32}>")
            : vector.Encoding == VectorEncoding.ProductQuantized && (vector.Components.Length != book.Subspaces
                || book.CodesPerSubspace < 256 && TensorPrimitives.MaxNumber(vector.Components.Span) >= book.CodesPerSubspace)
                ? new ComputeFault.ModelRejected($"<embed-pq-codes:{vector.Components.Length}:{book.Subspaces}:{book.CodesPerSubspace}>")
            : null;

    static ReadOnlyMemory<float> DecodeFloat(EmbeddingVector vector, Option<ProductCodebook> codebook) =>
        vector.Encoding.Switch(
            state: (Vector: vector, Codebook: codebook),
            float32: static s => DecodeFloat32(s.Vector.Components.Span),
            float16: static s => DecodeHalf(s.Vector.Components.Span),
            int8Scalar: static s => DecodeInt8(MemoryMarshal.Cast<byte, sbyte>(s.Vector.Components.Span)),
            binaryHamming: static s => DecodeBinary(s.Vector.Components.Span, s.Vector.Dimension),
            productQuantized: static s => s.Codebook.Case is ProductCodebook book ? Reconstruct(s.Vector.Components.Span, book) : []);

    static byte[] EncodeInt8(ReadOnlySpan<float> unit) {
        using SpanOwner<float> scaled = SpanOwner<float>.Allocate(unit.Length);
        TensorPrimitives.Multiply(unit, Int8Scale, scaled.Span);
        byte[] components = new byte[unit.Length];
        TensorPrimitives.ConvertSaturating<float, sbyte>(scaled.Span, MemoryMarshal.Cast<byte, sbyte>(components));
        return components;
    }

    static float[] DecodeInt8(ReadOnlySpan<sbyte> codes) {
        float[] unit = new float[codes.Length];
        TensorPrimitives.ConvertChecked<sbyte, float>(codes, unit);
        TensorPrimitives.Divide(unit, Int8Scale, unit);
        return unit;
    }

    static byte[] EncodeHalf(ReadOnlySpan<float> unit) {
        Half[] narrowed = new Half[unit.Length];
        TensorPrimitives.ConvertToHalf(unit, narrowed);
        byte[] components = new byte[unit.Length * sizeof(ushort)];
        for (int index = 0; index < narrowed.Length; index++) { BinaryPrimitives.WriteUInt16LittleEndian(components.AsSpan(index * sizeof(ushort)), BitConverter.HalfToUInt16Bits(narrowed[index])); }
        return components;
    }

    static float[] DecodeHalf(ReadOnlySpan<byte> components) {
        Half[] narrowed = new Half[components.Length / sizeof(ushort)];
        for (int index = 0; index < narrowed.Length; index++) { narrowed[index] = BitConverter.UInt16BitsToHalf(BinaryPrimitives.ReadUInt16LittleEndian(components[(index * sizeof(ushort))..])); }
        float[] unit = new float[narrowed.Length];
        TensorPrimitives.ConvertToSingle(narrowed, unit);
        return unit;
    }

    static byte[] EncodeFloat(ReadOnlySpan<float> unit) {
        byte[] components = new byte[unit.Length * sizeof(float)];
        for (int index = 0; index < unit.Length; index++) { BinaryPrimitives.WriteSingleLittleEndian(components.AsSpan(index * sizeof(float)), unit[index]); }
        return components;
    }

    static float[] DecodeFloat32(ReadOnlySpan<byte> components) {
        float[] unit = new float[components.Length / sizeof(float)];
        for (int index = 0; index < unit.Length; index++) { unit[index] = BinaryPrimitives.ReadSingleLittleEndian(components[(index * sizeof(float))..]); }
        return unit;
    }

    static byte[] EncodeBinary(ReadOnlySpan<float> unit, float threshold) {
        byte[] packed = new byte[(unit.Length + 7) / 8];
        for (int component = 0; component < unit.Length; component++) {
            if (unit[component] > threshold) { packed[component >> 3] |= (byte)(1 << (component & 7)); }
        }
        return packed;
    }

    static float[] DecodeBinary(ReadOnlySpan<byte> packed, int dimension) {
        float[] unit = new float[dimension];
        for (int component = 0; component < dimension; component++) {
            unit[component] = (packed[component >> 3] & (1 << (component & 7))) != 0 ? 1f : -1f;
        }
        return unit;
    }

    static byte[] EncodeProduct(ReadOnlySpan<float> unit, ProductCodebook codebook) {
        byte[] codes = new byte[codebook.Subspaces];
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            ReadOnlySpan<float> part = unit.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
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
        float[] unit = new float[codebook.Dimension];
        for (int subspace = 0; subspace < codes.Length; subspace++) {
            codebook.Centroid(subspace, codes[subspace]).CopyTo(unit.AsSpan(subspace * codebook.SubspaceDim, codebook.SubspaceDim));
        }
        return unit;
    }

    static Seq<(EmbeddingVector Candidate, float Score)> TopK(Seq<(EmbeddingVector Candidate, float Score)> scored, int top, bool smallerIsNearer) {
        if (top <= 0 || scored.IsEmpty) { return Seq<(EmbeddingVector Candidate, float Score)>(); }
        PriorityQueue<(EmbeddingVector Candidate, float Score), (float Rank, UInt128 ReverseKey)> heap = new(top);
        foreach ((EmbeddingVector Candidate, float Score) row in scored) {
            float rank = smallerIsNearer ? -row.Score : row.Score;
            (float Rank, UInt128 ReverseKey) priority = (rank, UInt128.MaxValue - row.Candidate.ContentKey);
            if (heap.Count < top) { heap.Enqueue(row, priority); }
            else if (heap.TryPeek(out _, out (float Rank, UInt128 ReverseKey) worst) && priority.CompareTo(worst) > 0) { heap.EnqueueDequeue(row, priority); }
        }
        int kept = heap.Count;
        (EmbeddingVector Candidate, float Score)[] ordered = new (EmbeddingVector Candidate, float Score)[kept];
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
