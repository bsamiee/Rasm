# [COMPUTE_EMBEDDING]

`Rasm.Compute`'s embedding lane is the retrieval half of the inference spine: it owns one encoding axis (`VectorEncoding`), one metric axis (`VectorScore`), one content-keyed carrier (`EmbeddingVector`), and one `Encode`/`Score`/`Rank` fold over the `System.Numerics.Tensors` `TensorPrimitives` SIMD surface. It projects the L2-normalized unit `float[]` the `Model/run#RUN_MODES` `Embed` run produces down the `VectorEncoding` axis. RETRIEVAL only: vector PRODUCTION is the AppHost-governed `IEmbeddingGenerator` lane where Compute supplies the inner provider and never the builder.

Every artifact content-keys through `Rasm.Domain.ContentHash.Of` over a length-framed UTF-8 preimage folding model, encoding, dimension, codebook, and component bytes, so a deterministic re-encode addresses the same key the `Rasm.Persistence` vector index deduplicates against. Crossing to Persistence is content-key reference only, `ProductCodebook` arrives settled from `Query/retrieval#VECTOR_CODEBOOK`, and the one `Encode`/`Score`/`Rank` surface backs point-cloud and symbol classifiers without a BIM-specific retrieval service. Page is host-local and carries no `TS_PROJECTION`.

## [01]-[INDEX]

- [02]-[EMBEDDING]: `Encode`, `Score`, and `Rank` fold an `EmbeddingVector` over the `VectorEncoding` and `VectorScore` axes under `VectorPolicy` sign/rerank values over the `TensorPrimitives` SIMD surface crossing to the Persistence vector lane by content-key reference.

## [02]-[EMBEDDING]

- Owner: `VectorEncoding` `[SmartEnum<string>]` is the one closed encoding axis, each row carrying the `HammingScored` discriminant beside its OWN overflow-safe `ByteLength`, `Encode`, and `Decode` columns — the storage size, the projection down, and the projection back are one row's arithmetic, never two dispatch tables over one roster. `VectorScore` `[SmartEnum<string>]` is the one closed metric axis, each row carrying ranking direction, coarse-gate posture, and its complete score delegate. `VectorPolicy` owns binary threshold and rerank fanout. `EmbeddingVector` is the content-keyed buffer carrier — owned encoded bytes, encoding tag, source `ModelIdentity.Key`, float dimension, retained codebook identity, and `ContentHash` key — whose `Admit` boundary snapshots bytes before re-checking byte length, canonical padding, PQ identity, and content-key echo. `RankedCandidates` carries one ranking beside the content-keyed rows it dropped. `VectorOps` owns the static `Encode`/`Score`/`Rank` fold and the `Retrieve` two-stage crossing into the Persistence vector index; `EmbedRefusal` names contract-refusal values and carries no row key. The owner is `VectorOps`, never `Embedding` — that simple name is `Microsoft.Extensions.AI.Embedding`'s inside a package that admits that library.
- Cases: `VectorEncoding` rows `float32` (raw unit vector, exact-rerank ground truth) · `float16` (`ConvertToHalf`-narrowed, 2× smaller, high-accuracy storage widening to near-`float32`) · `int8-scalar` (`ConvertSaturating`-quantized symmetric int8, 4× smaller, default index storage) · `binary-hamming` (sign-thresholded 1-bit-per-component packed, 32× smaller, coarse pre-filter floor) · `product-quantized` (one code byte per subspace over the Persistence-trained codebook, largest-corpus storage); `VectorScore` rows `cosine` (unit-vector default, larger-is-nearer) · `dot` (inner product over normalized vectors, larger-is-nearer) · `euclidean` (smaller-is-nearer) · `l1` (Manhattan, smaller-is-nearer) · `hamming` (over the packed bit encoding, smaller-is-nearer, the coarse-gate metric) · `jaccard` (over the packed bit encoding, smaller-is-nearer) — full parity with the six-metric Persistence `VectorMetric` axis, so every index-supported distance has a Compute rerank arm.
- Entry: `Encode` projects the `Embed` unit output onto the encoding row and content-keys the result; `Score` applies the selected metric row to one admitted pair; `Rank` bounded-top-K ranks a candidate set, widens a coarse binary gate to `top × VectorPolicy.RerankFanout`, and returns the ranking beside its dropped rows; `Retrieve(query, coarse, gate, fine, top, policy, VectorIndex, codebook)` is the TWO-STAGE fold — the coarse rank returns content keys, `VectorIndex.Resolve` reads the survivors' fine storage by those keys, `EmbeddingVector.Admit` re-proves each against the carrier law and the key echo, and the fine metric re-ranks what the storage handed back, every leg's refusals riding one `Dropped` roster. `Fin<T>` aborts on an oversized encoding, a nonpositive or overflowing rank fanout, a cross-model pair, a binary metric over a non-packed encoding, a packed-width or dimension mismatch, a `product-quantized` operand without its codebook or against a dimension- or identity-disagreeing codebook, or a degenerate (`NaN`/`Inf`) score.
- Auto: `Encode` admits the unit vector and the codebook layout as INDEPENDENT accumulating facts, then reads the encoding row's own `Encode` column with the source span passed straight through — a `ToArray` lift is the deleted form; `float32` and `float16` canonicalize to little-endian bytes through one reinterpret and a conditional bulk `ReverseEndianness`, the `int8-scalar` arm scales into a pooled `SpanOwner<float>` scratch and saturates through `TensorPrimitives.ConvertSaturating<float, sbyte>` (never a hand-rolled clamp-and-cast loop), and the `product-quantized` row assigns each sub-vector to its nearest codebook centroid by `TensorPrimitives.Distance` into one scratch and `TensorPrimitives.IndexOfMin` over it (genuine k-means assignment whose tie rule is the primitive's, shared with the training-time partition) or REFUSES when no codebook arrived; the address then folds the encoded bytes with dimension and codebook identity, baking the quantization posture in. `Score` delegates the complete pair operation to the metric row — packed lowering uses `HammingBitDistance` or the `BitwiseAnd`/`BitwiseOr` + `PopCount` intersection-over-union fold; dense lowering uses `CosineSimilarity`/`Dot`/`Distance` or the `Subtract`+`SumOfMagnitudes` scratch fold — so a new metric is one behavior-complete `VectorScore` row. `Rank` scores every candidate in ONE pass that keeps both halves — the sound rows and the refusals keyed by content key — and selects over the sound set through the kernel `Ranked.Top` bounded order statistic (O(n log k), content-key-stable ties, never a full `OrderBy` sort or a hand heap); the `int8-scalar` decode dequantizes and the `product-quantized` decode reconstructs by concatenating centroids before any exact metric reads, and a degenerate score projects through its named `EmbedRefusal` at the boundary, never inward.
- Result: `Encode` returns the encoded tensor directly; `Score` and `Rank` are pure value transforms, and candidate-index recall and latency remain the Persistence vector-lane owner's measured concern.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Generator.Equals (`[Equatable(Explicit)]`+`[DefaultEquality]` — the one-member content-key equality replacing the hand-written five-member block), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`/`CanonicalWriter`), Rasm.Persistence (project — `ProductCodebook`, `VectorIndex`, `VectorRow`, `VectorFine`), BCL inbox
- Growth: a new vector encoding is one `VectorEncoding` row carrying its own `ByteLength`, `Encode`, and `Decode` columns — no dispatch arm and no static body moves; a new metric is one behavior-complete `VectorScore` row; a new encode-or-rerank value is one column on `VectorPolicy`; a BIM class prototype is one stored `EmbeddingVector` and nearest-prototype classification one `Rank`; zero new surface — a `CosineSimilarity`/`DotProductScorer`/`HammingScorer` method family collapses onto the one `Score` fold, an `Int8Quantizer`/`BinaryEncoder`/`ProductQuantizer` owner onto the one `Encode` fold, and a per-encoding `EmbeddingVector` subtype onto the one carrier whose encoding column discriminates.
- Boundary: this page owns embed-and-score and crosses to the `Rasm.Persistence` vector lane by content-key reference only — Persistence owns the index, the pgvector HNSW graph, the codebook training, the amortized asymmetric-distance scan, and the recency horizon, so a Compute-side vector store, ANN index, codebook fit, or horizon is the named drift defect and `EmbeddingVector.ContentKey` is the single join the index addresses (the `:x32` form the Persistence marshal renders is the suite `XxHash128` value, never a second hash). Embedding projection is the `Model/run#RUN_MODES` `Embed` run's — it applies the selected pooling row then L2-normalizes via `TensorPrimitives.Norm`+`Divide` — so a re-implemented pool/normalize here is the rejected form. `System.Numerics.Tensors` owns every reduction and conversion — scoring lowers onto `CosineSimilarity`/`Dot`/`Distance` over `float` spans, `Subtract`+`SumOfMagnitudes` for the `l1` scratch fold, `HammingBitDistance` and the `BitwiseAnd`/`BitwiseOr`+`PopCount` Jaccard fold over the packed integral spans, float16 onto `ConvertToHalf`/`ConvertToSingle`, int8 onto `ConvertSaturating<float, sbyte>`/`ConvertChecked<sbyte, float>`, PQ assignment onto `Distance` — so a hand-rolled dot, half conversion, saturating clamp, bit-distance popcount, or centroid-distance loop is the deleted form, and only the sign-PACK bit gather remains a span-kernel exemption, because no primitive owns a sign-bit gather into a byte plane — the byte framing lowers onto one reinterpret plus a conditional `ReverseEndianness`, the sign COMPARE onto `GreaterThan`, and nearest-centroid assignment onto `IndexOfMin`, whose tie rule must agree bit for bit with the training-time partition at both ends of the boundary. Two-stage retrieval is honest — `binary-hamming` is sign-only, so a `Score` against a dense metric is structurally unavailable, and the genuine rerank is a second `Rank` over the survivors' fine forms Persistence resolves from the coarse `Rank`'s returned keys, sized `top × policy.RerankFanout`; a binary-Hamming terminal verdict skipping the fine rerank and an in-page float rerank of a ±1 decode are both named defects. Dense `float16`/`int8-scalar` rows keep magnitude, so the asymmetric path scores the full-precision `float32` query against the widened-half or dequantized-int8 candidate and a cross-encoding `Score` is a supported bridge, never a fault — only a binary metric over a non-packed encoding, a packed-width or dimension mismatch, a `product-quantized` operand without its codebook or against a codebook whose `Id` disagrees with the carrier's retained `CodebookId`, or a degenerate score faults through its named `EmbedRefusal` at the boundary so a silently-wrong score never reaches the result. Those faults REFUSE ONE ROW inside `Rank` rather than the ranking: the sound candidates rank and every refusal rides `RankedCandidates.Dropped` keyed by its own content key, because a corpus page carrying one stale-codebook or cross-model row is a repair list, not an empty result, and a first-fault abort discards every candidate after the offender including the ones the caller was ranking for. `ProductCodebook` (subspaces, per-subspace centroids, code width, identity) arrives settled from the Persistence vector-lane owner — this page does nearest-centroid encode and centroid-reconstruction decode over it but never trains it, and the amortized asymmetric-distance corpus scan is the index's concern while the bounded rerank here reconstructs-and-scores. Content key folds the model key, encoding, float dimension, codebook identity, and canonical encoded bytes so the quantization posture and codebook are part of the address, a re-trained codebook re-keys every `product-quantized` artifact, and the carrier RETAINS `CodebookId` as a field so PQ scoring proves its reconstruction codebook by identity — a same-dimension codebook with different centroids reconstructs plausible garbage, and an identity carried only inside an opaque hash cannot be re-checked at score time; `EmbeddingVector.Admit` snapshots the supplied bytes before re-validating `ByteLength` and the content-key echo so a mutable caller cannot change a rehydrated carrier after admission, and it is the ONE rehydrator the `Retrieve` fold's fine-form leg crosses — an `int8-scalar` storage whose scale or zero point disagrees with this lane's protocol constant refuses by name rather than decoding to a different vector under one content key. Encoded component bytes the carrier holds are an owned array — the transient int8 scaling scratch is a pooled `SpanOwner<float>` disposed in-arm, but a pooled `MemoryOwner` rent can never back the immutable carrier because the pool reuses the buffer (a use-after-free, the deleted form).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public static class EmbedRefusal {
    public static readonly ContractRefusal UnitRejected = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ByteLengthRejected = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal CarrierRejected = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal KeyEchoFailed = new(ComputeArea.Model, ComputeContract.Consistent);
    public static readonly ContractRefusal ModelMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal DimensionMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal MagnitudeMissing = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal PackingMissing = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal PackedWidthMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal CodebookMissing = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal CodebookLayoutMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal CodebookIdentityMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal CodeRangeExceeded = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ScoreDegenerate = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal FanoutRejected = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal EncodingUnadmitted = new(ComputeArea.Model, ComputeContract.Valid);

}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorEncoding {
    public static readonly VectorEncoding Float32 = new(
        "float32", hammingScored: false,
        static (dimension, _) => (long)dimension * sizeof(float),
        static (unit, _, _) => Fin.Succ(VectorOps.Framed(MemoryMarshal.Cast<float, uint>(unit))),
        static (components, _, _) => Fin.Succ(VectorOps.Singles(components)));

    public static readonly VectorEncoding Float16 = new(
        "float16", hammingScored: false,
        static (dimension, _) => (long)dimension * sizeof(ushort),
        static (unit, _, _) => Fin.Succ(VectorOps.EncodeHalf(unit)),
        static (components, _, _) => Fin.Succ(VectorOps.DecodeHalf(components)));

    public static readonly VectorEncoding Int8Scalar = new(
        "int8-scalar", hammingScored: false,
        static (dimension, _) => (long)dimension * sizeof(sbyte),
        static (unit, _, _) => Fin.Succ(VectorOps.EncodeInt8(unit)),
        static (components, _, _) => Fin.Succ(VectorOps.DecodeInt8(MemoryMarshal.Cast<byte, sbyte>(components))));

    public static readonly VectorEncoding BinaryHamming = new(
        "binary-hamming", hammingScored: true,
        static (dimension, _) => ((long)dimension + 7L) / 8L,
        static (unit, policy, _) => Fin.Succ(VectorOps.EncodeBinary(unit, policy.SignThreshold)),
        static (components, dimension, _) => Fin.Succ(VectorOps.DecodeBinary(components, dimension)));

    public static readonly VectorEncoding ProductQuantized = new(
        "product-quantized", hammingScored: false,
        static (_, codebook) => codebook.Match(Some: static book => (long)book.Subspaces, None: static () => 0L),
        static (unit, _, codebook) => codebook.Match(
            Some: book => Fin.Succ(VectorOps.EncodeProduct(unit, book)),
            None: static () => Fin.Fail<byte[]>(EmbedRefusal.CodebookMissing.Fault())),
        static (components, _, codebook) => codebook.Match(
            Some: book => Fin.Succ(VectorOps.Reconstruct(components, book)),
            None: static () => Fin.Fail<float[]>(EmbedRefusal.CodebookMissing.Fault())));

    public bool HammingScored { get; }

    [UseDelegateFromConstructor]
    public partial long ByteLength(int dimension, Option<ProductCodebook> codebook);

    [UseDelegateFromConstructor]
    public partial Fin<byte[]> Encode(ReadOnlySpan<float> unit, VectorPolicy policy, Option<ProductCodebook> codebook);

    [UseDelegateFromConstructor]
    public partial Fin<float[]> Decode(ReadOnlySpan<byte> components, int dimension, Option<ProductCodebook> codebook);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VectorScore {
    public static readonly VectorScore Cosine = new("cosine", smallerIsNearer: false, coarse: false,
        static (query, candidate, codebook) => VectorOps.Dense(query, candidate, codebook,
            static (left, right) => VectorOps.Guarded(TensorPrimitives.CosineSimilarity(left, right))));

    public static readonly VectorScore Dot = new("dot", smallerIsNearer: false, coarse: false,
        static (query, candidate, codebook) => VectorOps.Dense(query, candidate, codebook,
            static (left, right) => VectorOps.Guarded(TensorPrimitives.Dot(left, right))));

    public static readonly VectorScore Euclidean = new("euclidean", smallerIsNearer: true, coarse: false,
        static (query, candidate, codebook) => VectorOps.Dense(query, candidate, codebook,
            static (left, right) => VectorOps.Guarded(TensorPrimitives.Distance(left, right))));

    public static readonly VectorScore L1 = new("l1", smallerIsNearer: true, coarse: false,
        static (query, candidate, codebook) => VectorOps.Dense(query, candidate, codebook, VectorOps.Manhattan));

    public static readonly VectorScore Hamming = new("hamming", smallerIsNearer: true, coarse: true,
        static (query, candidate, _) => VectorOps.Packed(query, candidate,
            static (left, right) => Fin.Succ((float)TensorPrimitives.HammingBitDistance(left, right))));

    public static readonly VectorScore Jaccard = new("jaccard", smallerIsNearer: true, coarse: true,
        static (query, candidate, _) => VectorOps.Packed(query, candidate, VectorOps.JaccardBits));

    public bool SmallerIsNearer { get; }

    public ExtremumDirection Direction => SmallerIsNearer ? ExtremumDirection.Minimum : ExtremumDirection.Maximum;
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

public sealed record RankedCandidates(
    Seq<(EmbeddingVector Candidate, float Score)> Ranked,
    Seq<(UInt128 ContentKey, Error Fault)> Dropped);

[Equatable(Explicit = true)]
public sealed partial class EmbeddingVector {
    private EmbeddingVector(VectorEncoding encoding, string modelKey, int dimension, Option<UInt128> codebookId, ReadOnlyMemory<byte> components, UInt128 contentKey) =>
        (Encoding, ModelKey, Dimension, CodebookId, Components, ContentKey) = (encoding, modelKey, dimension, codebookId, components, contentKey);

    public VectorEncoding Encoding { get; }
    public string ModelKey { get; }
    public int Dimension { get; }

    public Option<UInt128> CodebookId { get; }
    public ReadOnlyMemory<byte> Components { get; }
    [DefaultEquality]
    public UInt128 ContentKey { get; }

    public string ArtifactKey => $"{ContentKey:x32}:{Encoding.Key}";

    public static UInt128 KeyOf(string modelKey, VectorEncoding encoding, int dimension, ReadOnlySpan<byte> components, Option<UInt128> codebookId) =>
        ContentHash.Of(
            (Model: modelKey, Encoding: encoding, Dimension: dimension, Components: components.ToArray(), Codebook: codebookId),
            static (state, writer) => writer
                .String(state.Model)
                .String(state.Encoding.Key)
                .Ordinal(state.Dimension)
                .Optional(state.Codebook, static (identity, framed) => framed.U128(identity))
                .Ordinal(state.Components.Length)
                .Raw(state.Components));

    public static Fin<EmbeddingVector> Admit(VectorEncoding encoding, string modelKey, int dimension, ReadOnlyMemory<byte> components, Option<ProductCodebook> codebook, UInt128 expectedKey) {
        byte[] owned = components.ToArray();
        Option<UInt128> retained = encoding == VectorEncoding.ProductQuantized ? codebook.Map(static book => book.Id) : None;
        return (guard(
                    !string.IsNullOrWhiteSpace(modelKey) && dimension > 0,
                    (Error)EmbedRefusal.CarrierRejected.Fault()),
                guard(
                    encoding != VectorEncoding.ProductQuantized || retained.IsSome,
                    (Error)EmbedRefusal.CodebookMissing.Fault()),
                guard(
                    owned.Length == encoding.ByteLength(dimension, codebook),
                    (Error)EmbedRefusal.CarrierRejected.Fault()),
                guard(
                    encoding != VectorEncoding.BinaryHamming || CanonicalPadding(dimension, owned),
                    (Error)EmbedRefusal.CarrierRejected.Fault()))
            .Apply(static (_, _, _, _) => unit).As().ToFin()
            .Bind(_ => KeyOf(modelKey, encoding, dimension, owned, retained) is UInt128 key && key == expectedKey
                ? Fin.Succ(new EmbeddingVector(encoding, modelKey, dimension, retained, owned, key))
                : Fin.Fail<EmbeddingVector>(EmbedRefusal.KeyEchoFailed.Fault()));
    }

    internal static EmbeddingVector Owned(VectorEncoding encoding, string modelKey, int dimension, Option<UInt128> codebookId, byte[] components) =>
        new(encoding, modelKey, dimension, codebookId, components, KeyOf(modelKey, encoding, dimension, components, codebookId));

    static bool CanonicalPadding(int dimension, ReadOnlySpan<byte> components) =>
        dimension % 8 is 0 || (components[^1] & ~((1 << (dimension % 8)) - 1)) is 0;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class VectorOps {
    const float Int8Scale = 127f;

    internal delegate Fin<float> SpanScore<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right) where T : unmanaged;

    public static Fin<EmbeddingVector> Encode(ReadOnlyMemory<float> unit, VectorEncoding encoding, string modelKey, VectorPolicy policy, Option<ProductCodebook> codebook = default) =>
        (guard(
             !string.IsNullOrWhiteSpace(modelKey) && !unit.IsEmpty
             && TensorPrimitives.IsFiniteAll(unit.Span)
             && MathF.Abs(TensorPrimitives.Norm<float>(unit.Span) - 1f) <= 1e-4f,
             (Error)EmbedRefusal.UnitRejected.Fault()),
         codebook.Match(
             Some: book => encoding != VectorEncoding.ProductQuantized ? Pure(unit) : Laid(book, unit.Length),
             None: static () => Pure(unit)),
         guard(
             encoding.ByteLength(unit.Length, codebook) is > 0L and <= Array.MaxLength,
             (Error)EmbedRefusal.ByteLengthRejected.Fault()))
        .Apply(static (_, _, _) => unit).As().ToFin()
        .Bind(admitted => encoding.Encode(admitted.Span, policy, codebook))
        .Map(components => EmbeddingVector.Owned(
            encoding,
            modelKey,
            unit.Length,
            encoding == VectorEncoding.ProductQuantized ? codebook.Map(static book => book.Id) : None,
            components));

    public static Fin<float> Score(EmbeddingVector query, EmbeddingVector candidate, VectorScore metric, Option<ProductCodebook> codebook = default) =>
        (guard(
             StringComparer.Ordinal.Equals(query.ModelKey, candidate.ModelKey),
             (Error)EmbedRefusal.ModelMismatched.Fault()),
         guard(
             query.Dimension == candidate.Dimension,
             (Error)EmbedRefusal.DimensionMismatched.Fault()))
        .Apply(static (_, _) => unit).As().ToFin()
        .Bind(_ => metric.Apply(query, candidate, codebook));

    public static Fin<RankedCandidates> Rank(EmbeddingVector query, Seq<EmbeddingVector> candidates, VectorScore metric, int top, VectorPolicy policy, Option<ProductCodebook> codebook = default) {
        long selected = metric.Coarse ? (long)top * policy.RerankFanout : top;
        return guard(
                top > 0 && selected <= int.MaxValue,
                (Error)EmbedRefusal.FanoutRejected.Fault())
            .ToFin()
            .Map(_ => candidates.Fold(
                (Sound: Seq<(EmbeddingVector Candidate, float Score)>(), Dropped: Seq<(UInt128 ContentKey, Error Fault)>()),
                (held, candidate) => Score(query, candidate, metric, codebook).Match(
                    Succ: score => (held.Sound.Add((candidate, score)), held.Dropped),
                    Fail: fault => (held.Sound, held.Dropped.Add((candidate.ContentKey, fault))))))
            .Map(split => new RankedCandidates(Selected(split.Sound, (int)selected, metric.Direction), split.Dropped));
    }

    public static IO<Fin<RankedCandidates>> Retrieve(
        EmbeddingVector query, Seq<EmbeddingVector> coarse, VectorScore gate, VectorScore fine,
        int top, VectorPolicy policy, VectorIndex index, Option<ProductCodebook> codebook = default) =>
        Rank(query, coarse, gate, top, policy, codebook).Match(
            Succ: gated => index.Resolve(gated.Ranked.Map(static row => row.Candidate.ContentKey))
                .Map(rows => Rehydrated(query, toSeq(rows))
                    .Bind(survivors => Rank(query, survivors.Sound, fine, top, policy, codebook)
                        .Map(ranked => ranked with { Dropped = gated.Dropped + survivors.Dropped + ranked.Dropped }))),
            Fail: static fault => IO.pure(Fin.Fail<RankedCandidates>(fault)));

    static Fin<(Seq<EmbeddingVector> Sound, Seq<(UInt128 ContentKey, Error Fault)> Dropped)> Rehydrated(EmbeddingVector query, Seq<VectorRow> rows) =>
        Fin.Succ(rows.Fold(
            (Sound: Seq<EmbeddingVector>(), Dropped: Seq<(UInt128 ContentKey, Error Fault)>()),
            (held, row) => Admitted(query, row).Match(
                Succ: carrier => (held.Sound.Add(carrier), held.Dropped),
                Fail: fault => (held.Sound, held.Dropped.Add((row.ContentKey, fault))))));

    static Fin<EmbeddingVector> Admitted(EmbeddingVector query, VectorRow row) => row.Fine.Switch(
        state: (Query: query, Row: row),
        float32: static (at, fine) => EmbeddingVector.Admit(
            VectorEncoding.Float32, at.Query.ModelKey, fine.Values.Length,
            Framed(MemoryMarshal.Cast<float, uint>(fine.Values.Span)), None, at.Row.ContentKey),
        int8Scalar: static (at, fine) => fine.ZeroPoint is 0 && MathF.Abs(fine.Scale.Value - (1f / Int8Scale)) <= float.Epsilon
            ? EmbeddingVector.Admit(
                VectorEncoding.Int8Scalar, at.Query.ModelKey, fine.Values.Length,
                MemoryMarshal.AsBytes(fine.Values.Span).ToArray(), None, at.Row.ContentKey)
            : Fin.Fail<EmbeddingVector>(EmbedRefusal.EncodingUnadmitted.Fault()));

    static Validation<Error, ReadOnlyMemory<float>> Pure(ReadOnlyMemory<float> unit) => Success<Error, ReadOnlyMemory<float>>(unit);

    static Validation<Error, ReadOnlyMemory<float>> Laid(ProductCodebook book, int dimension) =>
        guard(
            (long)book.Subspaces * book.SubspaceDim == dimension,
            (Error)EmbedRefusal.CodebookLayoutMismatched.Fault())
        .Map(static _ => ReadOnlyMemory<float>.Empty).As();

    internal static Fin<float> Dense(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook, SpanScore<float> reduce) =>
        (guard(
             !query.Encoding.HammingScored && !candidate.Encoding.HammingScored,
             (Error)EmbedRefusal.MagnitudeMissing.Fault()),
         PqSound(query, candidate, codebook))
        .Apply(static (_, _) => unit).As().ToFin()
        .Bind(_ => query.Encoding.Decode(query.Components.Span, query.Dimension, codebook))
        .Bind(left => candidate.Encoding.Decode(candidate.Components.Span, candidate.Dimension, codebook)
            .Bind(right => reduce(left, right)));

    internal static Fin<float> Packed(EmbeddingVector query, EmbeddingVector candidate, SpanScore<byte> reduce) =>
        (guard(
             query.Encoding.HammingScored && candidate.Encoding.HammingScored,
             (Error)EmbedRefusal.PackingMissing.Fault()),
         guard(
             query.Dimension == candidate.Dimension && query.Components.Length == candidate.Components.Length,
             (Error)EmbedRefusal.PackedWidthMismatched.Fault()))
        .Apply(static (_, _) => unit).As().ToFin()
        .Bind(_ => reduce(query.Components.Span, candidate.Components.Span));

    internal static Fin<float> JaccardBits(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) {
        using SpanOwner<byte> scratch = SpanOwner<byte>.Allocate(left.Length);
        TensorPrimitives.BitwiseAnd(left, right, scratch.Span);
        long shared = TensorPrimitives.PopCount<byte>(scratch.Span);
        TensorPrimitives.BitwiseOr(left, right, scratch.Span);
        long union = TensorPrimitives.PopCount<byte>(scratch.Span);
        return union == 0L ? Fin.Succ(0f) : Guarded(1f - ((float)shared / union));
    }

    internal static Fin<float> Manhattan(ReadOnlySpan<float> left, ReadOnlySpan<float> right) {
        using SpanOwner<float> scratch = SpanOwner<float>.Allocate(left.Length);
        TensorPrimitives.Subtract(left, right, scratch.Span);
        return Guarded(TensorPrimitives.SumOfMagnitudes<float>(scratch.Span));
    }

    static Validation<Error, Unit> PqSound(EmbeddingVector query, EmbeddingVector candidate, Option<ProductCodebook> codebook) =>
        query.Encoding != VectorEncoding.ProductQuantized && candidate.Encoding != VectorEncoding.ProductQuantized
            ? Success<Error, Unit>(unit)
            : codebook.Match(
                Some: book => (Laid(book, query.Dimension).Map(static _ => unit).As(), Coded(query, book), Coded(candidate, book))
                    .Apply(static (_, _, _) => unit).As(),
                None: () => guard(
                    false,
                    (Error)EmbedRefusal.CodebookMissing.Fault()).As());

    static Validation<Error, Unit> Coded(EmbeddingVector vector, ProductCodebook book) =>
        vector.Encoding != VectorEncoding.ProductQuantized
            ? Success<Error, Unit>(unit)
            : (guard(
                   vector.CodebookId == Some(book.Id),
                   (Error)EmbedRefusal.CodebookIdentityMismatched.Fault()),
               guard(
                   vector.Components.Length == book.Subspaces
                   && (book.CodesPerSubspace >= 256 || TensorPrimitives.MaxNumber(vector.Components.Span) < book.CodesPerSubspace),
                   (Error)EmbedRefusal.CodeRangeExceeded.Fault()))
              .Apply(static (_, _) => unit).As();

    internal static byte[] EncodeInt8(ReadOnlySpan<float> unit) {
        using SpanOwner<float> scaled = SpanOwner<float>.Allocate(unit.Length);
        TensorPrimitives.Multiply(unit, Int8Scale, scaled.Span);
        byte[] components = new byte[unit.Length];
        TensorPrimitives.ConvertSaturating<float, sbyte>(scaled.Span, MemoryMarshal.Cast<byte, sbyte>(components));
        return components;
    }

    internal static float[] DecodeInt8(ReadOnlySpan<sbyte> codes) {
        float[] unit = new float[codes.Length];
        TensorPrimitives.ConvertChecked<sbyte, float>(codes, unit);
        TensorPrimitives.Divide(unit, Int8Scale, unit);
        return unit;
    }

    internal static byte[] Framed(ReadOnlySpan<uint> words) {
        byte[] framed = new byte[words.Length * sizeof(uint)];
        Span<uint> destination = MemoryMarshal.Cast<byte, uint>(framed);
        if (BitConverter.IsLittleEndian) { words.CopyTo(destination); } else { BinaryPrimitives.ReverseEndianness(words, destination); }
        return framed;
    }

    internal static byte[] Framed(ReadOnlySpan<ushort> words) {
        byte[] framed = new byte[words.Length * sizeof(ushort)];
        Span<ushort> destination = MemoryMarshal.Cast<byte, ushort>(framed);
        if (BitConverter.IsLittleEndian) { words.CopyTo(destination); } else { BinaryPrimitives.ReverseEndianness(words, destination); }
        return framed;
    }

    internal static float[] Singles(ReadOnlySpan<byte> components) {
        float[] unit = new float[components.Length / sizeof(float)];
        ReadOnlySpan<uint> source = MemoryMarshal.Cast<byte, uint>(components);
        Span<uint> destination = MemoryMarshal.Cast<float, uint>(unit);
        if (BitConverter.IsLittleEndian) { source.CopyTo(destination); } else { BinaryPrimitives.ReverseEndianness(source, destination); }
        return unit;
    }

    internal static byte[] EncodeHalf(ReadOnlySpan<float> unit) {
        Half[] narrowed = new Half[unit.Length];
        TensorPrimitives.ConvertToHalf(unit, narrowed);
        return Framed(MemoryMarshal.Cast<Half, ushort>(narrowed));
    }

    internal static float[] DecodeHalf(ReadOnlySpan<byte> components) {
        ushort[] bits = new ushort[components.Length / sizeof(ushort)];
        ReadOnlySpan<ushort> source = MemoryMarshal.Cast<byte, ushort>(components);
        if (BitConverter.IsLittleEndian) { source.CopyTo(bits); } else { BinaryPrimitives.ReverseEndianness(source, bits); }
        float[] unit = new float[bits.Length];
        TensorPrimitives.ConvertToSingle(MemoryMarshal.Cast<ushort, Half>(bits), unit);
        return unit;
    }

    internal static byte[] EncodeBinary(ReadOnlySpan<float> unit, float threshold) {
        using SpanOwner<float> thresholds = SpanOwner<float>.Allocate(unit.Length);
        thresholds.Span.Fill(threshold);
        using SpanOwner<float> above = SpanOwner<float>.Allocate(unit.Length);
        TensorPrimitives.GreaterThan(unit, thresholds.Span, above.Span);
        byte[] packed = new byte[(unit.Length + 7) / 8];
        for (int component = 0; component < unit.Length; component++) {
            if (above.Span[component] != 0f) { packed[component >> 3] |= (byte)(1 << (component & 7)); }
        }
        return packed;
    }

    internal static float[] DecodeBinary(ReadOnlySpan<byte> packed, int dimension) {
        float[] unit = new float[dimension];
        for (int component = 0; component < dimension; component++) {
            unit[component] = (packed[component >> 3] & (1 << (component & 7))) != 0 ? 1f : -1f;
        }
        return unit;
    }

    internal static byte[] EncodeProduct(ReadOnlySpan<float> unit, ProductCodebook codebook) {
        byte[] codes = new byte[codebook.Subspaces];
        using SpanOwner<float> distances = SpanOwner<float>.Allocate(codebook.CodesPerSubspace);
        for (int subspace = 0; subspace < codebook.Subspaces; subspace++) {
            ReadOnlySpan<float> part = unit.Slice(subspace * codebook.SubspaceDim, codebook.SubspaceDim);
            for (int code = 0; code < codebook.CodesPerSubspace; code++) {
                distances.Span[code] = TensorPrimitives.Distance(part, codebook.Centroid(subspace, code));
            }
            codes[subspace] = (byte)TensorPrimitives.IndexOfMin<float>(distances.Span);
        }
        return codes;
    }

    internal static float[] Reconstruct(ReadOnlySpan<byte> codes, ProductCodebook codebook) {
        float[] unit = new float[codebook.Dimension];
        for (int subspace = 0; subspace < codes.Length; subspace++) {
            codebook.Centroid(subspace, codes[subspace]).CopyTo(unit.AsSpan(subspace * codebook.SubspaceDim, codebook.SubspaceDim));
        }
        return unit;
    }

    static Seq<(EmbeddingVector Candidate, float Score)> Selected(Seq<(EmbeddingVector Candidate, float Score)> scored, int top, ExtremumDirection direction) =>
        Ranked.Top(scored, top,
            row => (row.Score, direction == ExtremumDirection.Minimum ? row.Candidate.ContentKey : UInt128.MaxValue - row.Candidate.ContentKey),
            direction);

    internal static Fin<float> Guarded(float score) =>
        float.IsNaN(score) || float.IsInfinity(score)
            ? Fin.Fail<float>(EmbedRefusal.ScoreDegenerate.Fault())
            : Fin.Succ(score);
}
```
