# [COMPUTE_CODECS]

Rasm.Compute owns the compute-and-transport half of artifact interchange: the chunked error-bounded field/result codec over the simulation-field carrier, the FastCDC structural geometry-delta codec over meshes, B-reps, point clouds, and NURBS, the two-hop IFC-to-geometry tessellation bridge crossing to the IfcOpenShell companion, the 3D-Tiles streamable-LOD octree partition with its `EXT_structural_metadata` semantic layer joining IFC classification and solver field values at the content-key, and the content-addressed interchange-cache identity folding format key with each owner's complete output policy into one policy-seeded `XxHash128` cache-partition key. `Rasm.Bim` owns the IFC/glTF/STEP semantic object model and its import-export surface, reached at the companion seam; this lane is HOST-LOCAL and carries no TS_PROJECTION.

Owner types are `FieldCodec`/`DeltaCodec`, the `TessellationRequest` companion bridge, the `TileSet` octree with its `MetadataProperty`/`PropertyTable`/`TileMetadata`/`FeatureBand` family, and the `CanonicalForm`/`InterchangeIdentity` content-key. Composed as settled vocabulary: the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` precedent, the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, the SharpGLTF glTF-extension write surface, the meshoptimizer LOD kernels, and the `Substrate.RemoteGrpc` companion hop. GLB geometry-content identity is the kernel seed-zero `XxHash128` `GeometryHash` composed here, never re-minted with a policy seed.

## [01]-[INDEX]

- [01]-[TWO_HOP_TESSELLATION]: IFC/AP242/native geometry crosses to the companion, never in-proc; ifctester IDS-audit oracle and GeoArrow-buffer consume ride the same companion rpc.
- [02]-[FIELD_RESULT_CODEC]: chunked simulation-field layout; error-bounded lossy/lossless; zero-copy.
- [03]-[GEOMETRY_DELTA]: FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; progressive.
- [04]-[TILE_PARTITION]: 3D-Tiles octree partition; streamable LOD over the content-keyed geometry.
- [05]-[CONTENT_ADDRESSING]: policy-seeded canonical-form `XxHash128` interchange-cache key (the GLB geometry-content identity is the kernel seed-zero `GeometryHash` composed, distinct); empty-artifact sentinel; HLC two-half compose.
- [06]-[ARROW_BATCH]: `Solver/sweep` `DoeDataset` and `Runtime/receipts` `ChargebackDataset` project into a self-describing Arrow `RecordBatch` — per-axis/objective bulk-span `DoubleArray` columns, the `OnFront` `BooleanArray`, content-key/strategy/window as `Schema` metadata; the surrogate-training and billing lake egress the Persistence Flight-SQL landing redeems.

## [02]-[TWO_HOP_TESSELLATION]

- Owner: `TessellationRequest` — the two-hop bridge crossing IFC geometry evaluation to the IfcOpenShell companion (`IfcConvert` producing GLB) and re-importing the GLB through the Bim glTF path, host-local and riding the existing companion rpc, never a new transport; `IdsAuditRequest` the companion-rpc leg passing IDS-XML to the Python ifctester oracle and projecting the per-specification pass/fail `GlobalId` set into the Bim `IdsAudit` shape (one invocation beside `IfcConvert`); `ImportedGeometry` the decoded mesh-scene carrier the re-import lands and the tile partition reads; `TessellationPolicy` the deflection/tolerance/tile-partition policy folded into the content-key.
- Entry: `public static Fin<TessellationRequest> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, TessellationPolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; the companion round-trip rides the existing `Runtime/wire#PROTO_VOCABULARY` `Solve`/artifact transport, and the GLB re-enters the Bim glTF import rail as an `ImportedGeometry`.
- Auto: `Plan` gates the hop on the source format's companion-tessellation flag so a non-IFC format never crosses; the request carries the IFC bytes, deflection, tolerance, and content-key so a re-cross at the same policy is gated. Durable GLB residence is keyed by the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` dual `SourceKey`/`ContentKey` (kernel seed-zero content-hash, never a policy seed) with the Persistence object-store `ContentAddress`, the Bim bridge performing that durable reuse before crossing; this leg's policy-seeded `IfcContentKey` is the companion-rpc cache-partition over source IFC and evaluation policy that gates re-crossing — distinct cache layers, neither re-minting the other's key.
- Receipt: the `RemoteCall` receipt carries the companion transport, the IFC content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `TessellationRequest` folded into the content-key; a geospatial mesh payload is one `GeoArrowRequest` over the same companion operation and GLB result (never a second spatial codec); zero new surface.
- Boundary: two-hop rail is the single IFC-to-geometry path — the Bim IFC object model carries no tessellation kernel, so a managed IFC BRep evaluator is the deleted form; companion is the IfcOpenShell PyPI package in `libs/python/geometry`, never a NuGet pin, reached only over the existing `Runtime/transport#TRANSPORT_AXIS` UDS/InProcess companion rpc, so this page mints no transport, channel, or second wire vocabulary; a returned GLB re-enters the Bim glTF import rail as one `ImportedGeometry`, and the Bim IFC semantic graph and this hop's tessellated geometry are two projections of one content-keyed IFC artifact joined by the content-key; `python:data/spatial/geospatial` emits `EgressFormat.GEOARROW` as Arrow IPC bytes, and `GeoArrowRequest` carries that exact artifact to the companion for native geometry conversion before the existing GLB return — C# never invents a coordinate/offset ABI or hand-triangulates GeoArrow rings; `IdsAuditRequest` adds one ifctester invocation beside `IfcConvert` over the same companion rpc, passing IDS-XML with IFC content to the `python:geometry/ifc-companion` ifctester (`ids` oracle) and relaying the per-specification verdict wire back, which the Bim-owned `Review/validation#IDS_FACETS` `IdsAudit.Reconcile` composes into `IdsVerdict` rows and joins the C# self-audit against on the (GlobalId, `FacetKey`) axis — `FacetKey` the Bim composite join token unique within a specification (facet-type prefix and value discriminator), never the bare facet-type word — Compute referencing no Bim type and owning only the rpc orchestration and verdict relay, a Compute-minted IDS parser or a second transport the rejected form.

```csharp signature
public sealed record ImportedGeometry(
    string FormatKey,
    ReadOnlyMemory<float> Vertices,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<long> Indices,
    int VertexCount,
    int TriangleCount,
    Instant At);

public sealed record TessellationRequest(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    string ResultFormatKey) {
    public static Fin<TessellationRequest> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, TessellationPolicy policy) =>
        requiresCompanion
            ? Fin.Succ(new TessellationRequest(
                InterchangeIdentity.Key(formatKey, ifcBytes.Span, [policy.Deflection, policy.Tolerance, policy.AngleTolerance]), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, "glb"))
            : Fin.Fail<TessellationRequest>(new ComputeFault.ModelRejected($"<tessellation-not-required:{formatKey}>"));

    // Companion-rpc cache-partition over source IFC + evaluation policy — NOT the durable GLB store address
    // (the Bim TESSELLATION_BRIDGE ContentKey, kernel seed-zero, that the Persistence object store keys).
    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}

public sealed record GeoArrowRequest(UInt128 ContentKey, ReadOnlyMemory<byte> ArrowIpc, string ResultFormatKey) {
    public static Fin<GeoArrowRequest> Plan(ReadOnlyMemory<byte> arrowIpc) =>
        arrowIpc.IsEmpty
            ? Fin.Fail<GeoArrowRequest>(new ComputeFault.PayloadOverBounds("<geoarrow-ipc-empty>"))
            : Fin.Succ(new GeoArrowRequest(InterchangeIdentity.Key("geoarrow", arrowIpc.Span, []), arrowIpc, "glb"));

    public string ArtifactKey => $"{ContentKey:x32}:glb";
}

// ifctester cross-tool IDS-audit oracle over the settled TWO_HOP companion rpc: Compute orchestrates the
// companion invocation exactly as the tessellation hop, relaying the per-specification verdict wire back as the
// Bim-owned Review/validation#IDS_FACETS IdsVerdict(GlobalId, Specification, Spec, Requirement, Facet, Passed,
// Reason) row — composed by IdsAudit.Reconcile in Bim (the IDS authority), never re-declared here; Compute
// references no Bim type. Spec is the specification's zero-based document ordinal and Requirement the facet's
// ordinal within its spec, both from the shared document order (never the spec NAME, which IDS v1.0 leaves
// non-unique); Reconcile joins the self-audit against the oracle on the ordinal-qualified (GlobalId, Requirement,
// FacetKey) axis, oracle rows filtered by the Spec ordinal.
public sealed record IdsAuditRequest(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    ReadOnlyMemory<byte> IdsXml,
    string ResultFormatKey) {
    public static Fin<IdsAuditRequest> Plan(ReadOnlyMemory<byte> ifcBytes, ReadOnlyMemory<byte> idsXml, TessellationPolicy policy) =>
        idsXml.IsEmpty
            ? Fin.Fail<IdsAuditRequest>(new ComputeFault.ModelRejected("<ids-audit-empty-spec>"))
            : Fin.Succ(new IdsAuditRequest(
                InterchangeIdentity.Key("ids", ifcBytes.Span, [policy.Deflection, policy.Tolerance, policy.AngleTolerance]), ifcBytes, idsXml, "ids-verdict"));

    public string ArtifactKey => $"{IfcContentKey:x32}:ids";
}
```

## [03]-[FIELD_RESULT_CODEC]

- Owner: `FieldResidence` the closed exact/quantized/predicted residence `[Union]` whose per-case bits and bound ARE the arm's law — a lossless case selecting a lossy transform is unrepresentable; `FieldCodecPolicy` the chunked-layout record carrying the residence case and the compress column; `ResidualPredictor` the content-keyed model-lane chunk predictor; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `PointScan` the point-cloud carrier over E57/LAS/LAZ/PTS; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with error-bounded lossy, learned-residual-predicted, or exact lossless residence and a zero-copy solver↔store↔viz handoff; `InterchangeIo` the scientific-data ingest surface dispatching the chunked field decode and the point-scan ingest, the geometry and IFC import arms owned by `Rasm.Bim`.
- Entry: `public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock, Option<ResidualPredictor> predictor = default)` reads and reconstructs a self-describing chunked field through `FieldCodec.FieldDecode`; `public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock)` reads a point-cloud scan; `public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, Option<ResidualPredictor> predictor = default)` derives residence and compression from the header, decodes exact and quantized bodies directly, and reconstructs predicted bodies through their required model; `public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default)` emits the chunked layout under the residence case's own law; `Fin<T>` aborts on corrupt compression, a chunk-shape mismatch, a quantized bound the bit budget cannot meet, or a predicted residence handed no predictor.
- Auto: chunk blob exposes two views — `FieldCodec.ChunkSequence`, a multi-segment `ReadOnlySequence<byte>` (one segment per chunk) streamed with no flatten, and `FieldArtifact.Chunk(ordinal)`, the per-ordinal random-access slice a frustum cull reads — both addressing chunks by `FieldArtifact.GridChunks` grid position, not byte offset; the quantized residence codes each chunk to its case's bit budget through the shared `Quantization` kernel (`TensorPrimitives.MaxMagnitude` scale, never a Max/Min/Abs hand-roll) and gates its own bound; the predicted residence walks chunks CAUSALLY — the stencil gathers axis-aligned face neighbours by `GridChunks` coordinate from the RECONSTRUCTED buffer (`GatherNeighbours`, the true spatial stencil, never a 1-D window crossing grid faces and never source values the decoder cannot hold), predicts through the `ResidualPredictor` ONNX field model, quantizes only the prediction residual, and re-codes an over-bound chunk's residual exactly (step 0) so the case bound holds by construction and `Reconstruct` inverts the walk from stored residuals alone; lossless Brotli-compresses via the `System.IO.Compression` span codec sized by `GetMaxCompressedLength`, no intermediate stream; the `ByteString` wrap fanning one chunk buffer to store blob and viz upload is the `Runtime/transport#ARTIFACT_FRAMES` frame law, composed.
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy or residual-predicted encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable.
- Packages: System.IO.Hashing, System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox (`System.IO.Compression` Brotli span codec, `System.Buffers` sequence segments)
- Growth: a new chunked field format is one row on the `field-chunk` codec owned by the Bim format axis; a new point-scan format is one row on the `point-cloud` codec owned by the Bim format axis; a new residence law is one `FieldResidence` case whose arm the `FieldEncode` `Switch` demands at compile time; a learned predictor is one `ResidualPredictor` content-keyed ONNX session reused across chunks; zero new surface — a `ResidualCoder`/`NeuralFieldCompressor` sibling is collapsed onto the `FieldResidence.Predicted` case and the one `ResidualEncode`/`Reconstruct` pair.
- Boundary: field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor field rides the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, chunked by station and component, never a generic byte blob; the layout composes the suite `XxHash128` chunk identity content-addressed on the Persistence blob lane, so an identical chunk dedups and a re-read warms, a second field store the rejected form; the error bound is per-residence-case data the receipt records, never silently exceeded — the quantized arm faults an unmeetable bound and the predicted arm holds its bound by per-chunk exact fallback; the zero-copy edge is the remote frame law's `GetReadOnlySequence`/`UnsafeWrap` path, so a chunk crosses solver→store→viz with no managed copy, a `ToArray` flatten the named defect; the learned-compression terminal `ResidualPredictor` is one model-lane `Model/inference#INFERENCE_MODES` ONNX session content-keyed by the parametric-family digest and shared across chunks, composing the model lane rather than minting a second inference path, its grid-coordinate chunk index preserved (content-defined byte chunking destroys the grid locality the predictor needs — the FastCDC `#GEOMETRY_DELTA` chunker is the rejected rewrite), only the bounded residual stored, an over-bound chunk re-coded exact so the bound holds structurally, and the causal reconstructed-stencil walk making `Reconstruct` the codec's true inverse, the ONNX weights one content-addressed artifact the Python offline-science companion fits over the same offline-training seam the optimizer surrogate uses (never an in-proc fit), the achieved residual auditable on the `Cache` receipt; `PointScan` carries the `point-cloud` codec discriminant the Bim format axis names and faults `point-catalogue-pending` until an E57/LAS/LAZ/PTS reader is admitted; the geometry mesh decode and IFC semantic ingest are the `Rasm.Bim` import rail, never re-derived — an `ImportGeometry`/`ImportIfc` arm here the deleted form.

```csharp signature
// Residence is a CLOSED case family, never a bool triple whose combinations the encoder re-derives: each case
// carries exactly the law its arm enforces, so a "lossless" policy selecting a lossy transform is unrepresentable.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldResidence {
    private FieldResidence() { }

    public sealed record Exact : FieldResidence;
    public sealed record Quantized(int Bits, double Bound) : FieldResidence;
    public sealed record Predicted(int Bits, double Bound) : FieldResidence;

    public int QuantizationBits => Switch(
        exact: static _ => 0,
        quantized: static q => q.Bits,
        predicted: static p => p.Bits);

    public double ErrorBound => Switch(
        exact: static _ => 0.0,
        quantized: static q => q.Bound,
        predicted: static p => p.Bound);
}

// ChunkShape is a GATE, never an inherited decoration: a pinned shape admits only an artifact laid out exactly
// so (re-chunking is not this codec's operation), an empty shape inherits the artifact layout, and disagreement
// is the one typed disposition — a policy column the encode silently ignores is the deleted form.
public sealed record FieldCodecPolicy(int[] ChunkShape, FieldResidence Residence, bool Compress) {
    public static readonly FieldCodecPolicy Lossless = new(ChunkShape: [], new FieldResidence.Exact(), Compress: true);
    public static readonly FieldCodecPolicy Bounded = new(ChunkShape: [], new FieldResidence.Quantized(Bits: 12, Bound: 1e-3), Compress: true);
    public static readonly FieldCodecPolicy Residual = new(ChunkShape: [], new FieldResidence.Predicted(Bits: 12, Bound: 1e-3), Compress: true);
}

public sealed record ResidualPredictor(
    UInt128 FamilyDigest,
    ModelIdentity Model,
    string InputName,
    string OutputName,
    int NeighbourStencil,
    InferenceSession Session,
    RunOptions Options,
    CancelScope Scope) {
    public Fin<float[]> Predict(float[] stencil, int chunkElements) =>
        RunOps.Bind(new RunInput.Managed<float>(InputName, stencil, [1, stencil.Length])).Bind(inputs =>
            Session.Infer(Options, Scope, inputs, Seq(OutputName),
                results => {
                    ReadOnlySpan<float> predicted = results.First().GetTensorDataAsSpan<float>();
                    return predicted.Length >= chunkElements ? Fin.Succ(predicted[..chunkElements].ToArray()) : Fin.Fail<float[]>(new ComputeFault.ModelRejected($"<residual-predict-undersized:{predicted.Length}<{chunkElements}>"));
                }));
}

public sealed record FieldArtifact(
    string FormatKey,
    string Station,
    int Rank,
    int Components,
    long Count,
    int[] ChunkShape,
    int[] GridChunks,
    int ChunkCount,
    ReadOnlyMemory<byte> Chunks,
    double MaxResidual,
    Instant At) {
    public int ChunkElements => ChunkShape.Aggregate(1, static (acc, dim) => acc * dim) * Components;

    // Random-access read of one chunk's float bytes by its grid ordinal — the frustum-cull seam: a viewport
    // maps its frustum onto GridChunks coordinates, those onto ordinals, and reads only the intersected slices.
    public ReadOnlyMemory<byte> Chunk(int ordinal) {
        int chunkBytes = ChunkElements * sizeof(float);
        int start = ordinal * chunkBytes;
        return (uint)start >= (uint)Chunks.Length ? ReadOnlyMemory<byte>.Empty : Chunks.Slice(start, Math.Min(chunkBytes, Chunks.Length - start));
    }
}

public sealed record PointScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    Option<ReadOnlyMemory<float>> Colors,
    Option<ReadOnlyMemory<float>> Intensity,
    long PointCount,
    Instant At);

public static class InterchangeIo {
    public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock, Option<ResidualPredictor> predictor = default) =>
        codecKey == "field-chunk"
            ? FieldCodec.FieldDecode(formatKey, bytes, clock.GetCurrentInstant(), predictor)
            : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{formatKey}>"));

    public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock) =>
        codecKey != "point-cloud"
            ? Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{formatKey}>"))
            : Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-catalogue-pending:{formatKey}:e57-las-laz-pts-reader-unadmitted>"));
}

// Shared codec quantization law, composed by the field quantizer, the residual quantizer, and the
// [GEOMETRY_DELTA] normalizer: scale is one absolute-extremum SIMD reduction (never a Max/Min/Abs hand-roll),
// step the bit-budget grid, residual the relative rounding error a receipt records.
public static class Quantization {
    public static (float Scale, float Step) Steps(ReadOnlySpan<float> source, int bits) {
        float scale = MathF.Abs(TensorPrimitives.MaxMagnitude(source));
        int levels = (1 << bits) - 1;
        return (scale, levels > 0 ? scale / levels : 0f);
    }

    public static float Code(float value, float step) => step == 0f ? value : MathF.Round(value / step) * step;

    public static double Residual(float value, float coded, float scale) => scale == 0f ? 0.0 : Math.Abs(value - coded) / scale;
}

public static class FieldCodec {
    public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, Option<ResidualPredictor> predictor = default) =>
        Try.lift(() => Decode(formatKey, bytes, at)).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message))
            .Bind(decoded => decoded.Residence.Switch(
                state: (Decoded: decoded.Artifact, Predictor: predictor),
                exact: static (state, _) => Fin.Succ(state.Decoded),
                quantized: static (state, _) => Fin.Succ(state.Decoded),
                predicted: static (state, _) => state.Predictor
                    .ToFin(new ComputeFault.ModelRejected("<residual-needs-predictor>"))
                    .Bind(net => Reconstruct(state.Decoded, net))));

    public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default) =>
        Admit(field, policy).Bind(admitted => admitted.Policy.Residence.Switch(
            state: (admitted.Field, Predictor: predictor),
            exact: static (s, _) => Fin.Succ(s.Field with { MaxResidual = 0.0 }),
            quantized: static (s, q) => Fin.Succ(Quantize(s.Field, q.Bits)).Bind(coded =>
                coded.MaxResidual <= q.Bound
                    ? Fin.Succ(coded)
                    : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-error-bound:{coded.MaxResidual:R}>{q.Bound:R}"))),
            predicted: static (s, p) => s.Predictor
                .ToFin(new ComputeFault.ModelRejected("<residual-needs-predictor>"))
                .Bind(net => ResidualEncode(s.Field, p, net)))
        .Map(encoded => Packed(encoded, formatKey, admitted.Policy, at)));

    static Fin<(FieldArtifact Field, FieldCodecPolicy Policy)> Admit(FieldArtifact field, FieldCodecPolicy policy) {
        long chunkElements = field.ChunkShape.Aggregate(1L, static (product, extent) => extent > 0 && product <= int.MaxValue / extent ? product * extent : long.MaxValue);
        chunkElements = field.Components > 0 && chunkElements <= int.MaxValue / field.Components ? chunkElements * field.Components : long.MaxValue;
        long gridCount = field.GridChunks.Aggregate(1L, static (product, extent) => extent > 0 && product <= int.MaxValue / extent ? product * extent : long.MaxValue);
        bool residence = policy.Residence.Switch(
            exact: static _ => true,
            quantized: static row => row.Bits is >= 1 and <= 24 && double.IsFinite(row.Bound) && row.Bound > 0d,
            predicted: static row => row.Bits is >= 1 and <= 24 && double.IsFinite(row.Bound) && row.Bound > 0d);
        bool shape = field.Components > 0 && field.Count >= 0L && field.ChunkShape.Length > 0 && field.ChunkShape.All(static extent => extent > 0)
            && field.GridChunks.Length == field.ChunkShape.Length && field.GridChunks.All(static extent => extent > 0)
            && chunkElements is > 0 and <= int.MaxValue && gridCount == field.ChunkCount && field.Chunks.Length % sizeof(float) == 0;
        // A pinned policy shape GOVERNS: the artifact's layout must equal it exactly; an empty policy shape inherits.
        bool layout = policy.ChunkShape.Length == 0 || policy.ChunkShape.AsSpan().SequenceEqual(field.ChunkShape);
        return residence && shape && layout
            ? Fin.Succ((field, policy))
            : Fin.Fail<(FieldArtifact, FieldCodecPolicy)>(new ComputeFault.ModelRejected(layout
                ? $"<field-codec-shape:{field.Components}:{field.ChunkCount}:{chunkElements}:{gridCount}:{policy.Residence}>"
                : $"<field-codec-chunk-shape:[{string.Join(',', policy.ChunkShape)}]!=[{string.Join(',', field.ChunkShape)}]>"));
    }

    static ComputeArtifact Packed(FieldArtifact encoded, string formatKey, FieldCodecPolicy policy, Instant at) {
        ReadOnlyMemory<byte> packed = Pack(encoded, policy);
        return ComputeArtifact.Of(formatKey, packed, at, [policy.Residence.QuantizationBits, policy.Residence.ErrorBound]);
    }

    static Fin<FieldArtifact> ResidualEncode(FieldArtifact field, FieldResidence.Predicted residence, ResidualPredictor net) {
        float[] source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span).ToArray();
        int chunkElements = field.ChunkElements;
        int[] grid = field.GridChunks.Length > 0 ? field.GridChunks : [field.ChunkCount];
        (float scale, float step) = Quantization.Steps(source, residence.Bits);
        Fin<(float[] Residual, float[] Reconstructed, double Worst)> initial = Fin.Succ((new float[source.Length], new float[source.Length], 0d));
        return Range(0, field.ChunkCount)
            .Fold(initial, (rail, chunk) => rail.Bind(state => EncodeChunk(source, state, grid, chunk, chunkElements, net, residence, scale, step)))
            .Map(state => field with { Chunks = MemoryMarshal.AsBytes(state.Residual.AsSpan()).ToArray(), MaxResidual = state.Worst });
    }

    static Fin<(float[] Residual, float[] Reconstructed, double Worst)> EncodeChunk(
        float[] source,
        (float[] Residual, float[] Reconstructed, double Worst) state,
        int[] grid,
        int chunk,
        int chunkElements,
        ResidualPredictor net,
        FieldResidence.Predicted residence,
        float scale,
        float step) {
        int start = chunk * chunkElements;
        int length = Math.Min(chunkElements, source.Length - start);
        return length <= 0
            ? Fin.Succ(state)
            : net.Predict(GatherNeighbours(state.Reconstructed, grid, chunk, chunkElements, net.NeighbourStencil), length)
                .Map(prediction => {
                    double bounded = CodeChunk(source, prediction, state.Residual, state.Reconstructed, start, length, step, scale);
                    double achieved = bounded > residence.Bound
                        ? CodeChunk(source, prediction, state.Residual, state.Reconstructed, start, length, 0f, scale)
                        : bounded;
                    return (state.Residual, state.Reconstructed, Math.Max(state.Worst, achieved));
                });
    }

    static double CodeChunk(float[] source, float[] prediction, float[] residual, float[] reconstructed, int start, int length, float step, float scale) =>
        Range(0, length).Fold(0d, (worst, index) => {
            float predicted = index < prediction.Length ? prediction[index] : 0f;
            float coded = Quantization.Code(source[start + index] - predicted, step);
            residual[start + index] = coded;
            reconstructed[start + index] = predicted + coded;
            return Math.Max(worst, Quantization.Residual(source[start + index], predicted + coded, scale));
        });

    public static Fin<FieldArtifact> Reconstruct(FieldArtifact residualField, ResidualPredictor net) {
        float[] stored = MemoryMarshal.Cast<byte, float>(residualField.Chunks.Span).ToArray();
        int chunkElements = residualField.ChunkElements;
        if (chunkElements <= 0) { return Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected("<reconstruct-empty-chunk-shape>")); }
        int[] grid = residualField.GridChunks.Length > 0 ? residualField.GridChunks : [residualField.ChunkCount];
        Fin<float[]> reconstructed = Range(0, residualField.ChunkCount)
            .Fold(Fin.Succ(new float[stored.Length]), (rail, chunk) => rail.Bind(values => ReconstructChunk(stored, values, grid, chunk, chunkElements, net)));
        return reconstructed.Map(values => residualField with { Chunks = MemoryMarshal.AsBytes(values.AsSpan()).ToArray() });
    }

    static Fin<float[]> ReconstructChunk(float[] stored, float[] reconstructed, int[] grid, int chunk, int chunkElements, ResidualPredictor net) {
        int start = chunk * chunkElements;
        int length = Math.Min(chunkElements, stored.Length - start);
        return length <= 0
            ? Fin.Succ(reconstructed)
            : net.Predict(GatherNeighbours(reconstructed, grid, chunk, chunkElements, net.NeighbourStencil), length)
                .Map(prediction => Range(0, length).Fold(reconstructed, (values, index) => {
                    values[start + index] = (index < prediction.Length ? prediction[index] : 0f) + stored[start + index];
                    return values;
                }));
    }

    // Grid-coordinate face neighbours prevent 1-D boundary leakage; missing neighbours contribute a zero chunk.
    static float[] GatherNeighbours(ReadOnlySpan<float> source, int[] grid, int ordinal, int chunkElements, int radius) {
        int rank = grid.Length;
        Span<int> coord = stackalloc int[rank];
        int remainder = ordinal;
        for (int axis = rank - 1; axis >= 0; axis--) { coord[axis] = remainder % grid[axis]; remainder /= grid[axis]; }
        float[] stencil = new float[(1 + 2 * rank) * chunkElements];
        CopyChunk(source, ordinal, chunkElements, stencil, 0);
        int slot = 1;
        for (int axis = 0; axis < rank; axis++) {
            int minus = coord[axis] - radius, plus = coord[axis] + radius;
            if (minus >= 0) { CopyChunk(source, OrdinalAt(grid, coord, axis, minus), chunkElements, stencil, slot * chunkElements); }
            slot++;
            if (plus < grid[axis]) { CopyChunk(source, OrdinalAt(grid, coord, axis, plus), chunkElements, stencil, slot * chunkElements); }
            slot++;
        }
        return stencil;
    }

    static int OrdinalAt(int[] grid, ReadOnlySpan<int> coord, int axis, int value) {
        int ordinal = 0;
        for (int a = 0; a < grid.Length; a++) { ordinal = ordinal * grid[a] + (a == axis ? value : coord[a]); }
        return ordinal;
    }

    static void CopyChunk(ReadOnlySpan<float> source, int ordinal, int chunkElements, float[] destination, int offset) {
        int start = ordinal * chunkElements;
        if ((uint)start >= (uint)source.Length) { return; }
        int length = Math.Min(chunkElements, source.Length - start);
        source.Slice(start, length).CopyTo(destination.AsSpan(offset, length));
    }

    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) {
        int chunkBytes = field.ChunkElements * sizeof(float);
        if (chunkBytes <= 0 || field.ChunkCount <= 1) { return new(field.Chunks); }
        ChunkSegment? head = null, tail = null;
        for (int chunk = 0; chunk < field.ChunkCount; chunk++) {
            int start = chunk * chunkBytes;
            if (start >= field.Chunks.Length) { break; }
            ReadOnlyMemory<byte> slice = field.Chunks.Slice(start, Math.Min(chunkBytes, field.Chunks.Length - start));
            tail = tail is null ? head = new ChunkSegment(slice, 0) : tail.Append(slice);
        }
        return head is null ? new(field.Chunks) : new ReadOnlySequence<byte>(head, 0, tail!, tail!.Memory.Length);
    }

    static (FieldArtifact Artifact, FieldResidence Residence) Decode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at) {
        ReadOnlySpan<byte> span = bytes.Span;
        string station = Encoding.ASCII.GetString(span[..16]).TrimEnd('\0');
        int rank = BinaryPrimitives.ReadInt32LittleEndian(span[16..]);
        int components = BinaryPrimitives.ReadInt32LittleEndian(span[20..]);
        long count = BinaryPrimitives.ReadInt64LittleEndian(span[24..]);
        int rawBytes = BinaryPrimitives.ReadInt32LittleEndian(span[32..]);
        if (rank <= 0 || components <= 0 || count < 0L || rawBytes < 0) { throw new InvalidDataException($"<field-prefix:{rank}:{components}:{count}:{rawBytes}>"); }
        int residenceCode = BinaryPrimitives.ReadInt32LittleEndian(span[36..]);
        int bits = BinaryPrimitives.ReadInt32LittleEndian(span[40..]);
        double bound = BinaryPrimitives.ReadDoubleLittleEndian(span[44..]);
        bool compressed = span[52] switch { 0 => false, 1 => true, _ => throw new InvalidDataException($"<field-compression-flag:{span[52]}>") };
        FieldResidence residence = residenceCode switch {
            0 when bits == 0 && bound == 0d => new FieldResidence.Exact(),
            1 when bits is >= 1 and <= 24 && double.IsFinite(bound) && bound > 0d => new FieldResidence.Quantized(bits, bound),
            2 when bits is >= 1 and <= 24 && double.IsFinite(bound) && bound > 0d => new FieldResidence.Predicted(bits, bound),
            _ => throw new InvalidDataException($"<field-residence:{residenceCode}:{bits}:{bound:R}>")
        };
        int gridRank = BinaryPrimitives.ReadInt32LittleEndian(span[56..]);
        if (gridRank is < 1 or > 32 || 60 + gridRank * 4 > span.Length - sizeof(int)) { throw new InvalidDataException($"<field-grid-rank:{gridRank}:{span.Length}>"); }
        int[] gridChunks = new int[gridRank];
        for (int axis = 0; axis < gridRank; axis++) { gridChunks[axis] = BinaryPrimitives.ReadInt32LittleEndian(span[(60 + axis * 4)..]); }
        int gridEnd = 60 + gridRank * 4;
        int chunkRank = BinaryPrimitives.ReadInt32LittleEndian(span[gridEnd..]);
        if (chunkRank != gridRank || chunkRank is < 1 or > 32 || gridEnd + sizeof(int) + chunkRank * 4 > span.Length) { throw new InvalidDataException($"<field-chunk-rank:{chunkRank}:{gridRank}:{span.Length}>"); }
        int[] chunkShape = new int[chunkRank];
        for (int axis = 0; axis < chunkRank; axis++) { chunkShape[axis] = BinaryPrimitives.ReadInt32LittleEndian(span[(gridEnd + 4 + axis * 4)..]); }
        int headerEnd = gridEnd + 4 + chunkRank * 4;
        ReadOnlyMemory<byte> payload = compressed ? Decompress(bytes[headerEnd..], rawBytes) : bytes[headerEnd..];
        if (!compressed && payload.Length != rawBytes) { throw new InvalidDataException($"<field-raw-body:{payload.Length}:{rawBytes}>"); }
        long chunkElements = chunkShape.Aggregate(1L, static (product, extent) => extent > 0 && product <= int.MaxValue / extent ? product * extent : long.MaxValue);
        chunkElements = components > 0 && chunkElements <= int.MaxValue / components ? chunkElements * components : long.MaxValue;
        if (chunkElements is <= 0 or > int.MaxValue / sizeof(float) || payload.Length % sizeof(float) != 0 || gridChunks.Any(static extent => extent <= 0)) { throw new InvalidDataException($"<field-shape:{components}:{chunkElements}:{payload.Length}>"); }
        int chunkBytes = (int)chunkElements * sizeof(float);
        int chunkCount = (payload.Length + chunkBytes - 1) / chunkBytes;
        long gridCount = gridChunks.Aggregate(1L, static (product, extent) => product <= int.MaxValue / extent ? product * extent : long.MaxValue);
        if (gridCount != chunkCount) { throw new InvalidDataException($"<field-grid-count:{gridCount}:{chunkCount}>"); }
        return (new FieldArtifact(formatKey, station, rank, components, count, chunkShape, gridChunks, chunkCount, payload, 0.0, at), residence);
    }

    static FieldArtifact Quantize(FieldArtifact field, int bits) {
        float[] source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span).ToArray();
        (float scale, float step) = Quantization.Steps(source, bits);
        float[] quantized = source.Map(value => Quantization.Code(value, step)).ToArray();
        double worst = source.Zip(quantized).Fold(0d, (value, pair) => Math.Max(value, Quantization.Residual(pair.First, pair.Second, scale)));
        return field with { Chunks = MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), MaxResidual = worst };
    }

    // Self-describing header: the fixed station/rank/components/count prefix, uncompressed payload length,
    // GridChunks extent (grid-coordinate index), and ChunkShape extent precede the body, so Decode rebuilds the
    // chunk grid and residual stencil from bytes alone — never an out-of-band policy agreement that mis-counts chunks.
    static byte[] Pack(FieldArtifact field, FieldCodecPolicy policy) {
        int gridRank = field.GridChunks.Length, chunkRank = field.ChunkShape.Length;
        byte[] header = new byte[64 + gridRank * 4 + chunkRank * 4];
        Encoding.ASCII.GetBytes(field.Station.PadRight(16, '\0')[..16]).CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), field.Rank);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(20), field.Components);
        BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(24), field.Count);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(32), field.Chunks.Length);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(36), policy.Residence.Switch(exact: static _ => 0, quantized: static _ => 1, predicted: static _ => 2));
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40), policy.Residence.QuantizationBits);
        BinaryPrimitives.WriteDoubleLittleEndian(header.AsSpan(44), policy.Residence.ErrorBound);
        header[52] = policy.Compress ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(56), gridRank);
        for (int axis = 0; axis < gridRank; axis++) { BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(60 + axis * 4), field.GridChunks[axis]); }
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(60 + gridRank * 4), chunkRank);
        for (int axis = 0; axis < chunkRank; axis++) { BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(64 + gridRank * 4 + axis * 4), field.ChunkShape[axis]); }
        ReadOnlyMemory<byte> body = policy.Compress ? Compress(field.Chunks) : field.Chunks;
        return [.. header, .. body.Span];
    }

    static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data) {
        byte[] destination = new byte[BrotliEncoder.GetMaxCompressedLength(data.Length)];
        return BrotliEncoder.TryCompress(data.Span, destination, out int written)
            ? destination.AsMemory(0, written)
            : throw new InvalidDataException("<field-compress-failed>");
    }

    static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data, int rawLength) {
        byte[] destination = new byte[rawLength];
        return BrotliDecoder.TryDecompress(data.Span, destination, out int written) && written == rawLength
            ? destination
            : throw new InvalidDataException($"<field-decompress-corrupt:{written}:{rawLength}>");
    }

    sealed class ChunkSegment : ReadOnlySequenceSegment<byte> {
        public ChunkSegment(ReadOnlyMemory<byte> memory, long runningIndex) {
            Memory = memory;
            RunningIndex = runningIndex;
        }

        public ChunkSegment Append(ReadOnlyMemory<byte> memory) {
            ChunkSegment next = new ChunkSegment(memory, RunningIndex + Memory.Length);
            Next = next;
            return next;
        }
    }
}

```

## [04]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` structural-diff target rows; `GeometryDelta` the content-addressed delta record; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control.
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the ordered target chunk recipe (`TargetChunks`) with the new-chunk payload (`Added`, hashes absent from the base); `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` walks the recipe and reconstructs the target by pulling each chunk from the payload or the re-chunked base; `Fin<T>` aborts on invalid chunk policy or float alignment, base or target hash mismatch, corrupt payload framing, and an unresolved recipe hash.
- Auto: `Diff` first `Normalize`s a quantizable kind (vertex/point/control-point floats round to the finer of the bit-budget grid and `Tolerance` so a sub-tolerance perturbation hashes to one chunk, bounded-lossy within `Tolerance`; topology and B-rep-face streams pass verbatim), then runs FastCDC over the normalized bytes — a 256-entry SplitMix64 `Gear` table rolls the fingerprint, a STRICT mask below `AvgChunk` and a LOOSE mask above normalize the chunk-size distribution so an inserted vertex shifts only its local chunk; `TargetChunks` records the ordered hash recipe and `Added` the distinct new chunks stamped with the quantization `GeometricError`; the progressive column orders new chunks largest-first so a transmission renders coarse coverage before fine detail; the delta carries its own `DeltaPolicy` so `Apply` re-chunks the base identically and round-trips deterministically.
- Receipt: the `Cache` receipt carries the delta content-key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable; a progressive transmission stamps the coarse-chunk-first ordering count.
- Packages: System.IO.Hashing, System.Numerics.Tensors, LanguageExt.Core, Rasm.Persistence (project), BCL inbox (`System.Numerics.BitOperations` mask sizing)
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row carrying its `Quantizable` column; a new chunk policy is one column on `DeltaPolicy`; the quantization law is the shared `Quantization` kernel ([FIELD_RESULT_CODEC]); zero new surface.
- Boundary: geometry delta is the structural diff the blob-level delta never owned — the Persistence blob delta diffs opaque bytes, this diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only touched chunks; the diff algebra mirrors the `Runtime/wire#PROTO_VOCABULARY` `GraphDiff`/`SubtreeFetch` wire shape, Compute owning the structural chunking and the Persistence sync lane the closure-graph diff, neither re-deriving the other; the chunker is real FastCDC — a `Gear` rolling fingerprint with a STRICT-below / LOOSE-above-`AvgChunk` dual-mask tightening the size distribution so a local edit shifts only its own chunk, a fixed-block or single-mask shift-add chunker the rejected form; reconstruction is order-faithful and lossless — `TargetChunks` places a mid-stream insert at its true position, not the tail, and `Apply` re-chunks the base under the delta's OWN `DeltaPolicy`, never a hardcoded one; the bounded-lossy `Normalize` rounds a quantizable kind to the finer of the bit grid and `Tolerance` so a delta never exceeds the geometry tolerance; the new-chunk set transmits progressively through the `SubtreeFetch` server-stream and content-key-dedups against the Persistence blob lane (never a second delta store); the geometry-kind discriminant scopes quantization, so a topology-only edit never quantizes and a position-only edit never re-transmits the topology column.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeometryDeltaKind {
    public static readonly GeometryDeltaKind MeshVertex = new("mesh-vertex", quantizable: true);
    public static readonly GeometryDeltaKind MeshTopology = new("mesh-topology", quantizable: false);
    public static readonly GeometryDeltaKind BrepFace = new("brep-face", quantizable: false);
    public static readonly GeometryDeltaKind PointCloudOctant = new("pointcloud-octant", quantizable: true);
    public static readonly GeometryDeltaKind NurbsControl = new("nurbs-control", quantizable: true);

    public bool Quantizable { get; }
}

public sealed record DeltaPolicy(int MinChunk, int AvgChunk, int MaxChunk, int QuantizationBits, double Tolerance, bool Progressive) {
    public static readonly DeltaPolicy Canonical = new(MinChunk: 2048, AvgChunk: 8192, MaxChunk: 65536, QuantizationBits: 14, Tolerance: 1e-5, Progressive: true);
}

public readonly record struct DeltaChunk(UInt128 Hash, int Ordinal, int Offset, int ByteLength, double GeometricError);

public sealed record GeometryDelta(
    GeometryDeltaKind Kind,
    UInt128 BaseHash,
    UInt128 TargetHash,
    Seq<UInt128> TargetChunks,
    Seq<DeltaChunk> Added,
    ReadOnlyMemory<byte> Payload,
    DeltaPolicy Policy,
    long BaseBytes,
    long DeltaBytes);

public static class DeltaCodec {
    public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy) {
        if (!Valid(kind, policy, baseBytes, targetBytes)) { return Fin.Fail<GeometryDelta>(new ComputeFault.ModelRejected($"<delta-policy:{kind.Key}:{policy.MinChunk}:{policy.AvgChunk}:{policy.MaxChunk}:{policy.QuantizationBits}:{policy.Tolerance:R}>")); }
        ReadOnlyMemory<byte> normalizedBase = Normalize(kind, baseBytes, policy);
        ReadOnlyMemory<byte> normalizedTarget = Normalize(kind, targetBytes, policy);
        float error = QuantError(kind, targetBytes, policy);
        HashSet<UInt128> baseSet = FastCdc(normalizedBase.Span, policy).Map(static c => c.Hash).ToHashSet();
        Seq<DeltaChunk> targetChunks = FastCdc(normalizedTarget.Span, policy);
        Seq<DeltaChunk> added = toSeq(targetChunks.Filter(c => !baseSet.Contains(c.Hash)).DistinctBy(static c => c.Hash)).Map(c => c with { GeometricError = error });
        Seq<DeltaChunk> ordered = policy.Progressive ? added.OrderByDescending(static c => c.ByteLength).ToSeq() : added;
        return Fin.Succ(new GeometryDelta(kind,
            XxHash128.HashToUInt128(baseBytes.Span), XxHash128.HashToUInt128(normalizedTarget.Span),
            targetChunks.Map(static c => c.Hash), ordered, Concatenate(ordered, normalizedTarget), policy,
            baseBytes.Length, ordered.Sum(static c => (long)c.ByteLength)));
    }

    public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        !Valid(delta.Kind, delta.Policy, baseBytes)
            ? Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-policy:{delta.Kind.Key}>"))
            : XxHash128.HashToUInt128(baseBytes.Span) == delta.BaseHash
            ? Reconstruct(delta, baseBytes).Bind(target => XxHash128.HashToUInt128(target.Span) == delta.TargetHash
                ? Fin.Succ(target)
                : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-target-mismatch:{delta.TargetHash:x32}>")))
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-base-mismatch:{delta.BaseHash:x32}>"));

    static bool Valid(GeometryDeltaKind kind, DeltaPolicy policy, params ReadOnlySpan<ReadOnlyMemory<byte>> payloads) =>
        policy.MinChunk > 0 && policy.MinChunk <= policy.AvgChunk && policy.AvgChunk <= policy.MaxChunk
            && policy.QuantizationBits is >= 0 and <= 24 && double.IsFinite(policy.Tolerance) && policy.Tolerance > 0d
            && (!kind.Quantizable || payloads.ToArray().All(static payload => !payload.IsEmpty && payload.Length % sizeof(float) == 0));

    static ReadOnlyMemory<byte> Normalize(GeometryDeltaKind kind, ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        if (!kind.Quantizable || policy.QuantizationBits <= 0) { return bytes; }
        float[] source = MemoryMarshal.Cast<byte, float>(bytes.Span).ToArray();
        float step = QuantStep(source, policy);
        float[] quantized = source.Map(value => Quantization.Code(value, step)).ToArray();
        return MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray();
    }

    static float QuantStep(ReadOnlySpan<float> source, DeltaPolicy policy) {
        float bitStep = Quantization.Steps(source, policy.QuantizationBits).Step;
        return bitStep <= 0f ? (float)policy.Tolerance : MathF.Min(bitStep, (float)policy.Tolerance);
    }

    static float QuantError(GeometryDeltaKind kind, ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        kind.Quantizable && policy.QuantizationBits > 0 ? QuantStep(MemoryMarshal.Cast<byte, float>(bytes.Span), policy) : 0f;

    static Seq<DeltaChunk> FastCdc(ReadOnlySpan<byte> data, DeltaPolicy policy) {
        Seq<DeltaChunk> chunks = Seq<DeltaChunk>();
        int start = 0, ordinal = 0;
        while (start < data.Length) {
            int cut = ContentDefinedCut(data[start..], policy);
            ReadOnlySpan<byte> slice = data.Slice(start, cut);
            chunks = chunks.Add(new DeltaChunk(XxHash128.HashToUInt128(slice), ordinal++, start, cut, 0.0));
            start += cut;
        }
        return chunks;
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

    static ReadOnlyMemory<byte> Concatenate(Seq<DeltaChunk> added, ReadOnlyMemory<byte> targetBytes) {
        int total = added.Sum(static c => c.ByteLength + sizeof(int) * 2 + 16);
        byte[] buffer = new byte[total];
        Span<byte> sink = buffer.AsSpan();
        int cursor = 0;
        foreach (DeltaChunk chunk in added) {
            BinaryPrimitives.WriteUInt128LittleEndian(sink[cursor..], chunk.Hash);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 16)..], chunk.Ordinal);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 20)..], chunk.ByteLength);
            targetBytes.Span.Slice(chunk.Offset, chunk.ByteLength).CopyTo(sink[(cursor + 24)..]);
            cursor += 24 + chunk.ByteLength;
        }
        return buffer.AsMemory(0, cursor);
    }

    static Fin<ReadOnlyMemory<byte>> Reconstruct(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        Try.lift(() => SplitPayload(delta.Payload)).Run()
            .MapFail(static error => (Error)new ComputeFault.CacheCorrupt($"<delta-payload:{error.Message}>"))
            .Bind(addedByHash => {
                ReadOnlyMemory<byte> normalizedBase = Normalize(delta.Kind, baseBytes, delta.Policy);
                System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>> baseByHash = new();
                foreach (DeltaChunk chunk in FastCdc(normalizedBase.Span, delta.Policy)) { baseByHash[chunk.Hash] = normalizedBase.Slice(chunk.Offset, chunk.ByteLength); }
                return delta.TargetChunks
                    .Fold(Fin.Succ(Seq<ReadOnlyMemory<byte>>()), (rail, hash) => rail.Bind(pieces =>
                        addedByHash.TryGetValue(hash, out ReadOnlyMemory<byte> added)
                            ? Fin.Succ(pieces.Add(added))
                            : baseByHash.TryGetValue(hash, out ReadOnlyMemory<byte> held)
                                ? Fin.Succ(pieces.Add(held))
                                : Fin.Fail<Seq<ReadOnlyMemory<byte>>>(new ComputeFault.CacheCorrupt($"<delta-chunk-missing:{hash:x32}>"))))
                    .Map(pieces => {
                        byte[] target = new byte[pieces.Sum(static piece => piece.Length)];
                        int written = pieces.Fold(0, (cursor, piece) => { piece.Span.CopyTo(target.AsSpan(cursor)); return cursor + piece.Length; });
                        return (ReadOnlyMemory<byte>)target.AsMemory(0, written);
                    });
            });

    static System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>> SplitPayload(ReadOnlyMemory<byte> payload) {
        System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>> map = new();
        int cursor = 0;
        while (cursor < payload.Length) {
            if (payload.Length - cursor < 24) { throw new InvalidDataException($"<delta-header-truncated:{cursor}:{payload.Length}>"); }
            UInt128 hash = BinaryPrimitives.ReadUInt128LittleEndian(payload.Span[cursor..]);
            int byteLength = BinaryPrimitives.ReadInt32LittleEndian(payload.Span[(cursor + 20)..]);
            if (byteLength < 0 || byteLength > payload.Length - cursor - 24) { throw new InvalidDataException($"<delta-chunk-truncated:{cursor}:{byteLength}:{payload.Length}>"); }
            map[hash] = payload.Slice(cursor + 24, byteLength);
            cursor += 24 + byteLength;
        }
        return map;
    }
}
```

## [05]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the imported geometry carrier; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf content-keyed metadata property table joining the IFC classification column and the solver field-value columns under one feature-id mapping, carrying its own `ReplayKey` so a tile is independently addressable and cache-replayable; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `LeafContent`/`TilesetExport` the manifest-plus-leaf-reference export carriers; `ExportTiles` the tileset-manifest emit fold that serializes the octree to tileset.json and enumerates the leaf-content references the manifest names, riding the content-key and the metadata layer, the leaf BODIES themselves the Bim glTF codec's cross-package product; the partition consumes the deflection/tolerance and tile-depth/error/split scalars from `TessellationPolicy` and the `InterchangeIdentity.Key`/`InterchangeIdentity.Compose` content-key, never the Bim format/codec/KHR surface.
- Entry: `public static Fin<TilesetExport> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock)` admits a non-empty geometry, builds the octree, attaches the per-leaf metadata read at the node content-key, serializes the real tileset.json manifest, and enumerates the leaf-content references the manifest names — the leaf BODIES are the Bim glTF codec's product resolved at the content-key URIs, never emitted here; `public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock)` partitions the geometry into the depth-bounded octree; `Fin<T>` aborts on an empty or under-shaped geometry (`PayloadOverBounds` — a vertex-less mesh otherwise emits a manifest of float sentinel bounds) and on a tileset serialization miss projected onto `ComputeFault.ModelRejected`.
- Auto: `Build` partitions octant-by-octant to the policy max depth or triangle split threshold, geometric error the root error halved per depth, per-node content-key via `InterchangeIdentity.Key` so a re-partition of identical geometry keys identically, then reads the per-leaf `TileMetadata` at that content-key so one key addresses geometry and metadata. `ExportTiles` serializes the tileset.json manifest (box bounding volumes, per-level geometric error, refine REPLACE, leaf content-key URIs) and flattens the octree to enumerate leaf-content references — the leaf BODIES (carrying `EXT_structural_metadata`/`EXT_mesh_features`) are the Bim glTF cross-package product against the Persistence index, never emitted here. `TileMetadata.Join` folds the `Rasm.Bim` IFC classification and the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` per-element field values read at the shared content-key into one feature-keyed property table under its own tile content-key; `TileMetadata.ReplayKey` composes that key with the causal stamp through `InterchangeIdentity.Compose` in (physical, logical) order so a tile's metadata replays from cache without rebuilding the octree; `PropertyTable.Pack` lays each `MetadataProperty` column as a contiguous buffer-view body; `FeatureBand.Of` classifies an achieved field value onto its styling band.
- Receipt: the `StreamSegment` receipt carries the leaf-reference count, the root geometric error, the max depth, the node count, and the per-leaf property-column count; emission rides the sink port.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Ext.3DTiles (the `Schema2.Tiles3D` EXT_structural_metadata/EXT_mesh_features leaf-body schema surface, admitted via `Tiles3DExtensions.RegisterExtensions()` once at composition — the settled Compute admission; models no tileset.json manifest tree), meshoptimizer, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox (`System.Text.Json` Utf8JsonWriter tileset-manifest emit, `System.Buffers` ArrayBufferWriter)
- Growth: a new tile-partition parameter is one column on `TessellationPolicy` folded into the partition; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.
- Boundary: 3D-Tiles partition is the streamable-LOD octree over content-keyed geometry the compute lane owns — riding `InterchangeIdentity.Key` and the imported-geometry carrier — while the b3dm/glTF tile content encode is the Bim glTF codec the leaf emit composes; the metadata layer is one content-keyed schema column on the leaf-tile emit, never a parallel attribute store or second tiling owner, each `TileMetadata` carrying its own tile content-key (independently addressable) and `ReplayKey` composing it with the causal stamp so a leaf tile is cache-replayable without rebuilding the octree; the IFC classification reads the `Rasm.Bim` IFC semantic graph at the shared content-key (companion seam, never reaching into the Bim interior) and the per-element field values read the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` achieved value (never a recomputed metric), so the IFC graph and the tessellated geometry stay two projections of one content-keyed IFC artifact joined at the tile boundary, a re-tessellation at a new deflection re-keying both together; `EXT_structural_metadata` property tables and the `EXT_mesh_features` feature-id are vendor glTF extensions SharpGLTF.Core does not model natively — its in-box framework is `ExtensionsFactory.RegisterExtension<TParent,TExt>(string name)` over a caller-supplied `JsonSerializable`-derived class (the material-PBR surface being the separate string-keyed `MaterialChannel` API in Core and `KnownChannel` enum in Toolkit) — so the schema, property-table buffer-view columns, and feature-id vertex attribute emit through that write surface against a `JsonSerializable`-derived class registered before write, the binary layout the `[EXT_STRUCTURAL_METADATA]` RESEARCH leaf against the SharpGLTF API and the 3D-Tiles 1.1 spec (never an assumed helper); meshoptimizer owns the leaf-tile `Meshopt.Simplify`/`OptimizeVertexCache` LOD, never a hand-rolled simplifier; the leaf-tile content body is NOT emitted here — `ExportTiles` yields one typed `LeafContent` per leaf (content-key, `{contentKey:x32}.glb` URI, metadata-column count), the octree, metadata schema, and quantization-bit policy owned here while the b3dm/glTF body each URI names is the Bim tile-emit cross-package product against the Persistence index, a public leaf-body entry that can only decline the rejected honesty defect and a partition that re-derives the glTF body in-place or a metadata layer that re-reads the IFC parser the rejected form.

```csharp signature
// Compute-lane geometry-quality + tile-partition policy, RENAMED off `InterchangePolicy` — the Bim
// Exchange/export `InterchangePolicy` (codec emit columns) is a DISTINCT owner; every output-affecting quality
// and partition column salts the owning compute content key.
public sealed record TessellationPolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold) {
    public static readonly TessellationPolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        TileMaxDepth: 16, TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class FeatureBand {
    public static readonly FeatureBand Nominal = new("nominal", upperFraction: 0.5);
    public static readonly FeatureBand Elevated = new("elevated", upperFraction: 0.8);
    public static readonly FeatureBand Critical = new("critical", upperFraction: 1.0);

    public double UpperFraction { get; }

    public static FeatureBand Of(double value, double minimum, double maximum) {
        double span = Math.Max(1e-12, maximum - minimum);
        double fraction = Math.Clamp((value - minimum) / span, 0.0, 1.0);
        return Items.Find(row => fraction <= row.UpperFraction).IfNone(Critical);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MetadataProperty {
    private MetadataProperty() { }

    public sealed record Classification(string Name, Seq<string> Values) : MetadataProperty;
    public sealed record Scalar(string Name, string Unit, ReadOnlyMemory<float> Values) : MetadataProperty;
    public sealed record Banded(string Name, ReadOnlyMemory<float> Values, Seq<string> Bands) : MetadataProperty;

    public string PropertyName =>
        Switch(classification: static c => c.Name, scalar: static s => s.Name, banded: static b => b.Name);

    public int Count =>
        Switch(classification: static c => c.Values.Count, scalar: static s => s.Values.Length, banded: static b => b.Values.Length);

    public string ComponentType =>
        Switch(classification: static _ => "STRING", scalar: static _ => "FLOAT32", banded: static _ => "FLOAT32");

    public ReadOnlyMemory<byte> ColumnBytes =>
        Switch(
            classification: static c => (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(string.Join('\0', c.Values)),
            scalar: static s => MemoryMarshal.AsBytes(s.Values.Span).ToArray(),
            banded: static b => MemoryMarshal.AsBytes(b.Values.Span).ToArray());
}

public sealed record PropertyTable(string Class, int FeatureCount, Seq<MetadataProperty> Columns) {
    public (ReadOnlyMemory<byte> Buffer, Seq<(string Name, int Offset, int ByteLength, string ComponentType)> Views) Pack() {
        Seq<(string, int, int, string)> views = Seq<(string, int, int, string)>();
        int cursor = 0;
        List<byte[]> segments = [];
        foreach (MetadataProperty column in Columns) {
            byte[] bytes = column.ColumnBytes.ToArray();
            views = views.Add((column.PropertyName, cursor, bytes.Length, column.ComponentType));
            segments.Add(bytes);
            cursor += bytes.Length;
        }
        byte[] buffer = new byte[cursor];
        int slot = 0;
        foreach (byte[] segment in segments) { segment.CopyTo(buffer.AsSpan(slot)); slot += segment.Length; }
        return (buffer, views);
    }
}

public sealed record TileMetadata(UInt128 ContentKey, PropertyTable Table, ReadOnlyMemory<int> FeatureIds) {
    public static TileMetadata Join(UInt128 contentKey, string ifcClass, Seq<string> classification, ReadOnlyMemory<float> fieldValues, string fieldUnit, double minimum, double maximum, ReadOnlyMemory<int> featureIds) {
        Seq<string> bands = toSeq(fieldValues.ToArray().Select(value => FeatureBand.Of(value, minimum, maximum).Key));
        Seq<MetadataProperty> columns = Seq<MetadataProperty>(
            new MetadataProperty.Classification("ifc-class", classification),
            new MetadataProperty.Scalar("field-value", fieldUnit, fieldValues),
            new MetadataProperty.Banded("field-band", fieldValues, bands));
        return new TileMetadata(contentKey, new PropertyTable(ifcClass, classification.Count, columns), featureIds);
    }

    public UInt128 ReplayKey(Instant physical, ulong logical) => InterchangeIdentity.Compose(ContentKey, physical, logical);
}

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Option<TileMetadata> Metadata, Seq<TileNode> Children);

public sealed record TileSet(TileNode Root, double GeometricErrorRoot, int MaxDepth, int NodeCount, Instant At) {
    public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock) {
        TileNode root = Partition(geometry, metadata, policy, depth: 0);
        return new TileSet(root, policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(root), clock.GetCurrentInstant());
    }

    static TileNode Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, int depth) {
        float[] bounds = Bounds(geometry);
        double error = policy.TileGeometricErrorRoot / Math.Pow(2, depth);
        UInt128 contentKey = InterchangeIdentity.Key(
            geometry.FormatKey,
            MemoryMarshal.AsBytes(geometry.Vertices.Span),
            MemoryMarshal.AsBytes(geometry.Indices.Span),
            MemoryMarshal.AsBytes(geometry.Normals.Span), [
            policy.Deflection,
            policy.Tolerance,
            policy.AngleTolerance,
            policy.TileMaxDepth,
            policy.TileGeometricErrorRoot,
            policy.TileSplitThreshold,
        ]);
        return depth >= policy.TileMaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? new TileNode(depth, bounds, error, contentKey, metadata(contentKey), Seq<TileNode>())
            : new TileNode(depth, bounds, error, contentKey, None,
                Split(geometry, bounds).Map(child => Partition(child, metadata, policy, depth + 1)));
    }

    static int Count(TileNode node) => 1 + node.Children.Sum(Count);

    static float[] Bounds(ImportedGeometry geometry) {
        ReadOnlySpan<float> verts = geometry.Vertices.Span;
        (float minX, float minY, float minZ) = (float.MaxValue, float.MaxValue, float.MaxValue);
        (float maxX, float maxY, float maxZ) = (float.MinValue, float.MinValue, float.MinValue);
        for (int offset = 0; offset + 2 < verts.Length; offset += 3) {
            (minX, minY, minZ) = (Math.Min(minX, verts[offset]), Math.Min(minY, verts[offset + 1]), Math.Min(minZ, verts[offset + 2]));
            (maxX, maxY, maxZ) = (Math.Max(maxX, verts[offset]), Math.Max(maxY, verts[offset + 1]), Math.Max(maxZ, verts[offset + 2]));
        }
        return [(minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2, (maxX - minX) / 2, 0, 0, 0, (maxY - minY) / 2, 0, 0, 0, (maxZ - minZ) / 2];
    }

    // Triangle vertices resolve through ImportedGeometry.Indices, so indexed shared-vertex meshes partition
    // real corners; triangle-soup ordinal addressing mis-partitions non-identity index buffers.
    static Seq<ImportedGeometry> Split(ImportedGeometry geometry, float[] bounds) {
        (float cx, float cy, float cz) = (bounds[0], bounds[1], bounds[2]);
        return Range(0, geometry.TriangleCount)
            .GroupBy(tri => Octant(geometry.Vertices.Span, geometry.Indices.Span, tri, cx, cy, cz))
            .Map(group => Tessellate(geometry, group.ToSeq()))
            .ToSeq();
    }

    static int Octant(ReadOnlySpan<float> verts, ReadOnlySpan<long> indices, int triangle, float cx, float cy, float cz) {
        int v = (int)indices[triangle * 3] * 3;
        return (verts[v] >= cx ? 1 : 0) | (verts[v + 1] >= cy ? 2 : 0) | (verts[v + 2] >= cz ? 4 : 0);
    }

    static ImportedGeometry Tessellate(ImportedGeometry geometry, Seq<int> triangles) {
        ReadOnlySpan<float> srcV = geometry.Vertices.Span;
        ReadOnlySpan<float> srcN = geometry.Normals.Span;
        ReadOnlySpan<long> srcI = geometry.Indices.Span;
        float[] vertices = new float[triangles.Count * 9];
        float[] normals = new float[triangles.Count * 9];
        long[] indices = new long[triangles.Count * 3];
        int slot = 0;
        foreach (int tri in triangles) {
            for (int corner = 0; corner < 3; corner++) {
                int vertex = (int)srcI[tri * 3 + corner] * 3;
                srcV.Slice(vertex, 3).CopyTo(vertices.AsSpan(slot * 9 + corner * 3));
                srcN.Slice(vertex, 3).CopyTo(normals.AsSpan(slot * 9 + corner * 3));
            }
            (indices[slot * 3], indices[slot * 3 + 1], indices[slot * 3 + 2]) = (slot * 3, slot * 3 + 1, slot * 3 + 2);
            slot++;
        }
        return geometry with { Vertices = vertices, Normals = normals, Indices = indices, VertexCount = triangles.Count * 3, TriangleCount = triangles.Count };
    }
}

// Tileset EXPORT the partition owns: the real tileset.json Manifest (this page's product) and the LeafContent
// reference set it names — the typed handoff to the Bim glTF leaf codec, resolved against the Persistence index, never a body here.
public sealed record LeafContent(UInt128 ContentKey, string Uri, int MetadataColumns);

public sealed record TilesetExport(ComputeArtifact Manifest, Seq<LeafContent> Leaves);

public static class TilePartition {
    // Emits this page's OWNED product — the tileset.json manifest over the octree — and the LeafContent references
    // it names; the leaf BODIES (b3dm/glTF carrying EXT_structural_metadata/EXT_mesh_features) are the Bim cross-package product, never here.
    public static Fin<TilesetExport> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock) =>
        geometry.VertexCount <= 0 || geometry.TriangleCount <= 0
            || geometry.Vertices.Length < geometry.VertexCount * 3
            || geometry.Normals.Length < geometry.VertexCount * 3
            || geometry.Indices.Length < geometry.TriangleCount * 3
            || IndexOutOfRange(geometry)
            ? Fin.Fail<TilesetExport>(new ComputeFault.PayloadOverBounds($"<tileset-geometry:{geometry.VertexCount}:{geometry.TriangleCount}:{geometry.Vertices.Length}:{geometry.Indices.Length}>"))
            : Fin.Succ(TileSet.Build(geometry, metadata, policy, clock))
                .Bind(tiles => Tileset(tiles, policy, clock).Map(manifest => new TilesetExport(manifest, Leaves(tiles.Root))));

    static bool IndexOutOfRange(ImportedGeometry geometry) {
        ReadOnlySpan<long> indices = geometry.Indices.Span[..(geometry.TriangleCount * 3)];
        foreach (long index in indices) { if (index < 0 || index >= geometry.VertexCount) { return true; } }
        return false;
    }

    // tileset.json: refine REPLACE, box bounding volumes off each node's Aabb, geometricError halving per level,
    // leaf content URIs {contentKey:x32}.glb the AppUi/web consumer resolves against the Persistence index.
    static Fin<ComputeArtifact> Tileset(TileSet tiles, TessellationPolicy policy, IClock clock) =>
        Try.lift(() => ComputeArtifact.Of("tileset.json", TilesetBytes(tiles.Root), clock.GetCurrentInstant(), [
            policy.Deflection,
            policy.Tolerance,
            policy.AngleTolerance,
            policy.TileMaxDepth,
            policy.TileGeometricErrorRoot,
            policy.TileSplitThreshold,
        ])).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<tileset-emit:{error.Message}>"));

    // One LeafContent per octree leaf — content-key, {contentKey:x32}.glb URI, metadata-column count — the typed
    // handoff to the Bim leaf-content producer, whose bodies land on the Persistence blob lane under these keys.
    static Seq<LeafContent> Leaves(TileNode root) =>
        Flatten(root)
            .Filter(static node => node.Children.IsEmpty)
            .Map(static node => new LeafContent(node.ContentKey, $"{node.ContentKey:x32}.glb", node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)));

    // Real tileset.json serialization over the octree: a glTF-independent JSON manifest (asset 1.1, root
    // geometricError, recursive tile tree — box boundingVolume, per-node geometricError, refine REPLACE, leaf
    // content URI). SharpGLTF.Ext.3DTiles owns the glTF-embedded EXT_structural_metadata/EXT_mesh_features of the
    // leaf BODIES (Bim's codec), never this manifest tree, so it emits through the BCL Utf8JsonWriter.
    static ReadOnlyMemory<byte> TilesetBytes(TileNode root) {
        ArrayBufferWriter<byte> sink = new();
        using (Utf8JsonWriter writer = new(sink)) {
            writer.WriteStartObject();
            writer.WriteStartObject("asset");
            writer.WriteString("version", "1.1");
            writer.WriteEndObject();
            writer.WriteNumber("geometricError", root.GeometricError);
            writer.WritePropertyName("root");
            WriteNode(writer, root);
            writer.WriteEndObject();
            writer.Flush();
        }
        return sink.WrittenMemory;
    }

    static void WriteNode(Utf8JsonWriter writer, TileNode node) {
        writer.WriteStartObject();
        writer.WriteStartObject("boundingVolume");
        writer.WriteStartArray("box");
        foreach (float component in node.BoundingVolume) { writer.WriteNumberValue(component); }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteNumber("geometricError", node.GeometricError);
        writer.WriteString("refine", "REPLACE");
        if (node.Children.IsEmpty) {
            writer.WriteStartObject("content");
            writer.WriteString("uri", $"{node.ContentKey:x32}.glb");
            writer.WriteEndObject();
        } else {
            writer.WriteStartArray("children");
            foreach (TileNode child in node.Children) { WriteNode(writer, child); }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    static Seq<TileNode> Flatten(TileNode node) =>
        node.Cons(node.Children.Bind(Flatten));
}
```

## [06]-[CONTENT_ADDRESSING]

- Owner: `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `CanonicalForm` the static byte-normalization kernel reducing every keyed input to one machine-independent canonical byte form before the hash seed; `InterchangeIdentity` the interchange CACHE-PARTITION key derivation folding canonicalized source bytes with the complete ordered output-policy vector into one policy-seeded `XxHash128` identity (distinct from the kernel seed-zero `GeometryHash` the seam/Bim/Persistence/peers share), mirroring the model-lane `ModelIdentity.Snapshot` precedent, with `Compose` sealing the content key and HLC two-half stamp into one frame key and `SeedZero` minting the empty-artifact sentinel; `ComputeArtifact` the emitted-bytes carrier the field, tile, and Bim export rails feed, landing content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(...)` — pure value; the contiguous and pooled-sequence cases derive identity from canonical bytes and the complete ordered policy vector, while the geometry case frames vertices, indices, and normals by ordinal and byte length before one incremental hash; `public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical)` folds the content key with the causal stamp in the fixed (physical, logical) half order; `public static UInt128 SeedZero(string formatKey, ReadOnlySpan<double> policy)` is the empty-artifact sentinel identity; `ComputeArtifact.Of` is the one emit-carrier mint deriving the content key from bytes with its complete policy vector.
- Auto: every keyed input passes through `CanonicalForm` before the seed — `CanonicalForm.Tag` lower-cases invariant culture and trims the format/codec tag so `"GLB"` and `" glb "` key one identity, `CanonicalForm.Scalar` collapses negative zero to positive zero and maps every NaN pattern to one quiet-NaN payload, and `CanonicalForm.Write` lays the length-prefixed tag, the policy scalar count, and every ordered policy scalar little-endian — injective framing, so distinct `(formatKey, policy)` tuples never share a canonical byte vector; artifact bytes pass into the byte hash verbatim. `Key` seeds `XxHash128.HashToUInt128` with the `XxHash3.HashToUInt64` of that canonical vector, so tessellation folds deflection, tolerance, angle tolerance, tile depth, root geometric error, and split threshold while field residence folds bits and bound. A zero-length artifact routes to `SeedZero` over the same policy vector so absent and present-but-empty remain distinct. `Compose` lays the physical half first and logical half second, both little-endian, matching `AppHost/Runtime/ports#PORT_RECORDS`; `Admit` projects onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns.
- Receipt: the `Cache` receipt carries the content-key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt; a sentinel-keyed empty artifact stamps the `SeedZero` identity so an absent-versus-empty distinction is auditable.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one canonical-scalar column folded into the seed; a new keyed-input kind is one `CanonicalForm` arm; zero new surface.
- Boundary: interchange-cache identity is `XxHash128` over the canonical source bytes — the suite hash law the `Runtime/transport#ARTIFACT_FRAMES` whole-artifact identity and the model-lane `ModelIdentity` checksum hold, never a second hashing pass and never a path-keyed identity; canonical-form normalization is the cross-machine reproducibility floor — case-folded trimmed tag, little-endian policy scalars, negative-zero collapsed to positive zero, every NaN payload mapped to one quiet NaN — so two semantically-equal source artifacts on osx-arm64, linux-x64, and win-x64 cache-key one identity (the `lang:python:runtime/evidence/identity#IDENTITY` `ContentIdentity` folds the same format/deflection/tolerance, the cross-runtime peer), a raw-string-interpolated seed (`$"{formatKey}|{deflection:R}|..."`) the rejected drift defect keying distinctly across cultures and float renderings; the SHARED geometry WIRE hash is a DISTINCT key — the GLB geometry-content identity the seam `Rasm.Element/Graph/element#NODE_MODEL` `RepresentationContentHash`, the Persistence `Store/blobstore#OBJECT_STORE` blob name, and the `lang:typescript:core/interchange/frame#GEOMETRY_PLANE` + `lang:typescript:data/object/store` `ObjectKey` peers reproduce is the KERNEL seed-zero (`seed=0`) `XxHash128` `GeometryHash` over the canonical bytes (`csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` the golden vector anchoring C#/Python/TypeScript byte-parity), composed here and never re-minted with a policy seed — a policy-seeded GLB geometry-content hash the named cross-runtime defect, the two keys coexisting by design; the empty-artifact `SeedZero` sentinel is the absent-versus-empty law (policy-seeded empty case, distinct from the kernel `seed=0`) — empty bytes key to `SeedZero` over the policy alone, never the byte hash of an empty span, so a cache key never collides absent against present-but-empty; the HLC compose order is byte-identical to `AppHost/Runtime/ports#PORT_RECORDS` (physical half first, logical second, both little-endian), a logical-half-first composition the named defect folding a fresh op as stale; the key takes a format-key string rather than the Bim `InterchangeFormat` owner so the content identity stays a Compute concern decoupled from the moved format axis; every output-affecting scalar folds in owner order, so deflection, tolerance, angle tolerance, tile depth, root error, or split-threshold movement partitions a tileset key and prevents cross-setting hits; addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` under the content-key string `Path`, so the IFC semantic graph (Bim), the tessellated GLB, the field artifact, and a re-exported glTF are rows under the ONE kernel seed-zero `XxHash128` residence identity the Persistence index re-derives (`ArtifactIndexRow.Admit` -> `ContentAddress.Of`) — Compute owning only the policy-seeded cache-key derivation (the logical label), the kernel/seam the seed-zero residence identity, Persistence the blob residence, none re-declaring another; the export-rail field/tile/re-exported-glTF artifacts self-key (their `SourceKey` their own `ContentHash`, single-projection) while the tessellated GLB and the IFC-semantic graph of one source IFC share one cross-projection `sourceKey` — the kernel seed-zero `SourceKey` the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` mints purely over the source bytes (tolerance-independent, so the in-process semantic-graph ingest re-derives it without the deflection), NOT the policy-seeded cache key — so the Persistence `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Project` returns the two-projection family under that kernel-seed-zero key, the `Option<UInt128> sourceKey` admission carrying the pure key and each row's blob residence the kernel seed-zero `ContentAddress.Of` (`ArtifactIndexRow.Admit`), never a GLB self-key off the policy-seeded partition stranding the geometry projection off the semantic one; a managed copy of the artifact bytes beside the blob lane is the rejected form.

```csharp signature

// String-keyed compute-lane emit carrier, RENAMED off `ExportArtifact` — the Bim Exchange/export
// `ExportArtifact` (an `InterchangeFormat`-rowed carrier) is a DISTINCT owner.
public sealed record ComputeArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At) {
    public static ComputeArtifact Of(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, ReadOnlySpan<double> policy = default) =>
        new(formatKey, bytes, InterchangeIdentity.Key(formatKey, bytes.Span, policy), bytes.Length, at);
}

// Ordered policy vectors are axis-neutral: each keyed lane supplies every output-affecting scalar in owner order,
// and CanonicalForm supplies the one scalar normalization and byte layout. Framing is injective: the tag byte
// length and the policy scalar count prefix their payloads little-endian, so no (formatKey, policy) pair can
// spell another pair's canonical bytes — a bare tag-then-scalars concatenation lets an 8-byte tag suffix
// masquerade as a policy scalar and collide two distinct identities into one seed.
public static class CanonicalForm {
    public const long QuietNaNBits = unchecked((long)0x7FF8000000000000UL);

    public static string Tag(string raw) => raw.Trim().ToLowerInvariant();

    public static double Scalar(double raw) =>
        double.IsNaN(raw) ? BitConverter.Int64BitsToDouble(QuietNaNBits)
        : raw == 0d ? 0d
        : raw;

    public static int Write(Span<byte> destination, string formatKey, ReadOnlySpan<double> policy) {
        string tag = Tag(formatKey);
        int tagBytes = Encoding.UTF8.GetByteCount(tag);
        BinaryPrimitives.WriteInt32LittleEndian(destination, tagBytes);
        int written = sizeof(int) + Encoding.UTF8.GetBytes(tag, destination[sizeof(int)..]);
        BinaryPrimitives.WriteInt32LittleEndian(destination[written..], policy.Length);
        written += sizeof(int);
        foreach (double scalar in policy) {
            BinaryPrimitives.WriteDoubleLittleEndian(destination[written..], Scalar(scalar));
            written += sizeof(double);
        }
        return written;
    }

    public static long Seed(string formatKey, ReadOnlySpan<double> policy) {
        Span<byte> canonical = stackalloc byte[sizeof(int) * 2 + Encoding.UTF8.GetByteCount(Tag(formatKey)) + policy.Length * sizeof(double)];
        int length = Write(canonical, formatKey, policy);
        return unchecked((long)XxHash3.HashToUInt64(canonical[..length]));
    }
}

public static class InterchangeIdentity {
    public const ulong SeedZeroDomain = 0xFFFF_FFFF_FFFF_FFFFUL;

    public static UInt128 Key(string formatKey, ReadOnlySpan<byte> bytes, ReadOnlySpan<double> policy) =>
        bytes.IsEmpty
            ? SeedZero(formatKey, policy)
            : XxHash128.HashToUInt128(bytes, CanonicalForm.Seed(formatKey, policy));

    // Incremental sibling for pooled multi-segment payloads (a chunked field blob, a reassembled frame sequence):
    // Seeded incremental hashing avoids flattening a multi-segment artifact.
    public static UInt128 Key(string formatKey, ReadOnlySequence<byte> bytes, ReadOnlySpan<double> policy) {
        if (bytes.IsEmpty) { return SeedZero(formatKey, policy); }
        XxHash128 hasher = new(CanonicalForm.Seed(formatKey, policy));
        foreach (ReadOnlyMemory<byte> segment in bytes) { hasher.Append(segment.Span); }
        return hasher.GetCurrentHashAsUInt128();
    }

    public static UInt128 Key(
        string formatKey,
        ReadOnlySpan<byte> vertices,
        ReadOnlySpan<byte> indices,
        ReadOnlySpan<byte> normals,
        ReadOnlySpan<double> policy) {
        if (vertices.IsEmpty && indices.IsEmpty && normals.IsEmpty) { return SeedZero(formatKey, policy); }
        XxHash128 hasher = new(CanonicalForm.Seed(formatKey, policy));
        AppendComponent(hasher, 0, vertices);
        AppendComponent(hasher, 1, indices);
        AppendComponent(hasher, 2, normals);
        return hasher.GetCurrentHashAsUInt128();
    }

    static void AppendComponent(XxHash128 hasher, byte ordinal, ReadOnlySpan<byte> bytes) {
        Span<byte> header = stackalloc byte[sizeof(byte) + sizeof(int)];
        header[0] = ordinal;
        BinaryPrimitives.WriteInt32LittleEndian(header[sizeof(byte)..], bytes.Length);
        hasher.Append(header);
        hasher.Append(bytes);
    }

    public static UInt128 SeedZero(string formatKey, ReadOnlySpan<double> policy) {
        Span<byte> sentinel = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(sentinel, SeedZeroDomain);
        BinaryPrimitives.WriteUInt64LittleEndian(sentinel[8..], 0UL);
        return XxHash128.HashToUInt128(sentinel, CanonicalForm.Seed(formatKey, policy));
    }

    public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical) {
        Span<byte> frame = stackalloc byte[32];
        BinaryPrimitives.WriteUInt64LittleEndian(frame, (ulong)contentKey);
        BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], (ulong)(contentKey >> 64));
        BinaryPrimitives.WriteInt64LittleEndian(frame[16..], physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(frame[24..], logical);
        return XxHash128.HashToUInt128(frame);
    }

    public static ArtifactIndexRow Admit(ComputeArtifact artifact, DataClassification classification, Option<UInt128> sourceKey) =>
        ArtifactIndexRow.Admit(ArtifactKind.Interchange, $"{artifact.ContentKey:x32}:{CanonicalForm.Tag(artifact.FormatKey)}", artifact.Bytes.Span, classification, artifact.At, sourceKey);
}
```

## [07]-[ARROW_BATCH]

- Owner: `ArrowBatch` — the one columnar-construction owner projecting the `Solver/sweep` `DoeDataset` and the `Runtime/receipts` `ChargebackDataset` into a self-describing `Apache.Arrow` `RecordBatch`; `Doe`/`Chargeback` the two producers, `Strided`/`Doubles` the shared column folds. Core `Apache.Arrow` is the sole reference: the IPC writer, the LZ4/Zstd `CompressionCodecFactory`, the ADBC query surface, and the Flight-SQL transport are the Persistence `api-arrow` overlay's egress rails, absent from the Compute closure.
- Entry: `public static Fin<RecordBatch> Doe(DoeDataset dataset, MemoryAllocator? allocator = null)` admits checked row-major dimensions and a non-empty, ordinal-unique field vocabulary excluding `on_front`, then projects the `Coordinates`/`Responses` blocks into one `DoubleArray` column per axis and per objective and the `OnFront` mask into one `BooleanArray` column, `ContentKey`/`Strategy`/`At`/`points` riding `Schema.Builder.Metadata`; `public static Fin<RecordBatch> Chargeback(ChargebackDataset dataset, MemoryAllocator? allocator = null)` folds the tenant-partitioned rows into `tenant`/`route` `StringArray`, four `CostVector` `DoubleArray`, and a `facts` `Int64Array` column, window bounds and content-key as metadata.
- Auto: each column bulk-appends one span — `OnFront` drives `BooleanArray.Builder.Append(onFront.Span)`, which copies the span once into the allocator-owned BooleanArray buffer, and each axis/objective column drives one `DoubleArray.Builder.Append(ReadOnlySpan<double>)` after a single row-major→columnar strided gather pre-sized by `Reserve(points)`, never a per-element `Append(T)` loop; the `Schema` field order and the batch column order are the one append sequence so the reader recovers columns positionally, and `ContentKey` rides `Schema` metadata so a batch whose metadata omits the content key is the drift defect.
- Receipt: none new — the batch is a projection of the standing `DoeDataset`/`ChargebackDataset` shapes; the sealed `RecordBatch` crosses to Persistence over the existing `Runtime/transport` wire plane and the Persistence `Query/columnar` `Land(LandingArm.Doe, …)` port redeems it.
- Packages: Apache.Arrow, NodaTime (`InstantPattern.ExtendedIso` the metadata instant), Thinktecture.Runtime.Extensions (`DoeDesign`/`Substrate` `.Key`), LanguageExt.Core, BCL inbox (`CultureInfo.InvariantCulture`)
- Growth: a new dataset producer is one `ArrowBatch` method reusing the shared column folds, never a per-dataset columnar encoder; a per-row-instant lake producer (the receipt-journal egress) adds one `TimestampArray` column under `TimestampType.Default`, the NodaTime clock seam the metadata instant already shares; a new column is one `Field` and its bulk-span fold; the `MemoryAllocator` injects per lane so a staging-bounded arena charges the batch buffers against the lane budget rather than the shared `MemoryAllocator.Default` fallback.
- Boundary: Compute BUILDS the columnar table; the Persistence `api-arrow` overlay OWNS everything that CARRIES it — `ArrowStreamWriter`/`ArrowFileWriter` IPC, the `Apache.Arrow.Compression.CompressionCodecFactory` LZ4/Zstd codec, the ADBC query surface, and the `FlightClient`/`FlightSqlClient` — so Compute holds one core `Apache.Arrow` reference, references none of the four egress packages, and opens no Flight listener; the row-major→columnar transpose is the one unavoidable gather (a `Reserve`+`Append(span)` per column, never a per-element builder loop); a bare `DateTime` where the NodaTime instant crosses, the shared `MemoryAllocator.Default` where a lane arena is available, a schema field order diverging from the column order, or a hand-rolled columnar byte layout `RecordBatch` already owns are the rejected forms; the sealed `RecordBatch` stops at the Compute edge — `[08]-[RESEARCH]` `FLIGHT_SQL_PUSH` (the Persistence `Query/columnar` landing over `Apache.Arrow.Flight.Sql`) is the reciprocal obligation, and the opaque `GeoArrowRequest.ArrowIpc` relay bytes are never decoded or re-encoded here.

```csharp signature

// One Arrow construction owner, two dataset producers (Solver/sweep DoeDataset, Runtime/receipts ChargebackDataset)
// — never a per-dataset bespoke columnar encoder. Compute BUILDS the columnar table; the Persistence api-arrow
// overlay OWNS everything that carries it (IPC writer, LZ4/Zstd codec, ADBC, Flight-SQL). Every builder takes a
// per-lane MemoryAllocator; a null allocator falls back to the process-global MemoryAllocator.Default, so a
// staging-bounded lane charges its own arena.
public static class ArrowBatch {
    // Surrogate-training egress: the DoeDataset row-major Coordinates/Responses blocks project to one DoubleArray
    // column PER axis and PER objective (the tabular training shape), the OnFront mask to one allocator-owned BooleanArray
    // column, and ContentKey/Strategy/At/shape ride Schema metadata so the batch is self-describing across the wire.
    public static Fin<RecordBatch> Doe(DoeDataset dataset, MemoryAllocator? allocator = null) {
        int rows = dataset.Points, d = dataset.Axes.Count, m = dataset.Objectives.Count;
        long coordinateCount = checked((long)rows * d);
        long responseCount = checked((long)rows * m);
        Seq<string> labels = dataset.Axes + dataset.Objectives;
        bool shape = rows > 0
            && d > 0
            && m > 0
            && coordinateCount <= int.MaxValue
            && responseCount <= int.MaxValue
            && dataset.Coordinates.Length == coordinateCount
            && dataset.Responses.Length == responseCount
            && dataset.OnFront.Length == rows;
        bool fields = labels.ForAll(static label => !string.IsNullOrWhiteSpace(label))
            && labels.ToArray().Distinct(StringComparer.Ordinal).Count() == labels.Count
            && !labels.Exists(static label => string.Equals(label, "on_front", StringComparison.Ordinal));
        if (!shape) {
            return Fin.Fail<RecordBatch>(ComputeFault.Create("<arrow-doe-shape>"));
        }
        if (!fields) { return Fin.Fail<RecordBatch>(ComputeFault.Create("<arrow-doe-fields>")); }
        Seq<(Field Field, IArrowArray Array)> axisCols = toSeq(Enumerable.Range(0, d)).Map(lane =>
            (new Field(dataset.Axes[lane], DoubleType.Default, false), (IArrowArray)Strided(dataset.Coordinates, d, lane, rows, allocator)));
        Seq<(Field Field, IArrowArray Array)> objectiveCols = toSeq(Enumerable.Range(0, m)).Map(lane =>
            (new Field(dataset.Objectives[lane], DoubleType.Default, false), (IArrowArray)Strided(dataset.Responses, m, lane, rows, allocator)));
        (Field Field, IArrowArray Array) frontCol =
            (new Field("on_front", BooleanType.Default, false), new BooleanArray.Builder().Append(dataset.OnFront.Span).Build(allocator));
        Seq<(Field Field, IArrowArray Array)> columns = axisCols + objectiveCols + Seq1(frontCol);
        Schema schema = columns.Fold(new Schema.Builder(), static (builder, column) => builder.Field(column.Field))
            .Metadata("content_key", $"{dataset.ContentKey:x32}")
            .Metadata("strategy", dataset.Strategy.Key)
            .Metadata("at", InstantPattern.ExtendedIso.Format(dataset.At))
            .Metadata("points", rows.ToString(CultureInfo.InvariantCulture))
            .Build();
        return Fin.Succ(new RecordBatch(schema, columns.Map(static column => column.Array), rows));
    }

    // Billing egress folds the same construction surface: one row per (tenant, route), the CostVector lanes as
    // DoubleArray columns, facts as an Int64Array, tenant/route as StringArray, window+content-key as metadata.
    public static Fin<RecordBatch> Chargeback(ChargebackDataset dataset, MemoryAllocator? allocator = null) {
        int rows = dataset.Rows.Count;
        Seq<(Field Field, IArrowArray Array)> columns = Seq<(Field Field, IArrowArray Array)>(
            (new Field("tenant", StringType.Default, false),
                new StringArray.Builder().AppendRange(dataset.Rows.Map(static row => row.Tenant.Slug)).Build(allocator)),
            (new Field("route", StringType.Default, true),
                new StringArray.Builder().AppendRange(dataset.Rows.Map(static row => row.Route.Match(
                    Some: route => route.Key,
                    None: () => (string)null!))).Build(allocator)),
            (new Field("elapsed_units", DoubleType.Default, false),
                Doubles(dataset.Rows.Map(static row => row.Vector.ElapsedUnits), allocator)),
            (new Field("token_units", DoubleType.Default, false),
                Doubles(dataset.Rows.Map(static row => row.Vector.TokenUnits), allocator)),
            (new Field("byte_units", DoubleType.Default, false),
                Doubles(dataset.Rows.Map(static row => row.Vector.ByteUnits), allocator)),
            (new Field("remote_units", DoubleType.Default, false),
                Doubles(dataset.Rows.Map(static row => row.Vector.RemoteUnits), allocator)),
            (new Field("facts", Int64Type.Default, false),
                new Int64Array.Builder().Reserve(rows).Append(dataset.Rows.Map(static row => row.Facts).ToArray()).Build(allocator)));
        Schema schema = columns.Fold(new Schema.Builder(), static (builder, column) => builder.Field(column.Field))
            .Metadata("content_key", $"{dataset.ContentKey:x32}")
            .Metadata("window_start", InstantPattern.ExtendedIso.Format(dataset.WindowStart))
            .Metadata("window_end", InstantPattern.ExtendedIso.Format(dataset.WindowEnd))
            .Build();
        return Fin.Succ(new RecordBatch(schema, columns.Map(static column => column.Array), rows));
    }

    // Row-major → columnar transpose: ONE bulk Append(ReadOnlySpan<double>) per column after a strided gather,
    // never a per-element Append(T) loop; Reserve pre-sizes the buffer to the known row count before the span append.
    static DoubleArray Strided(ReadOnlyMemory<double> block, int stride, int lane, int rows, MemoryAllocator? allocator) {
        Span<double> gather = rows <= 512 ? stackalloc double[rows] : new double[rows];
        ReadOnlySpan<double> source = block.Span;
        for (int row = 0; row < rows; row++) { gather[row] = source[row * stride + lane]; }
        return new DoubleArray.Builder().Reserve(rows).Append(gather).Build(allocator);
    }

    static DoubleArray Doubles(Seq<double> values, MemoryAllocator? allocator) =>
        new DoubleArray.Builder().Reserve(values.Count).Append(values.ToArray()).Build(allocator);
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [COMPANION_PROTOCOL]-[OPEN]: what is the IfcOpenShell companion-daemon request/response protocol for the two-hop hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract; the `python:geometry/ifc-companion` over the remote-lane companion rpc.
- [FIELD_FORMAT]-[OPEN]: what are the CGNS/EnSight/VTK/Zarr chunked-field decode member spellings; the admitted field-format library surface (row vocabulary owned by the Bim format axis), verified at the field-codec admission gate.
- [RESIDUAL_PREDICTOR]-[OPEN]: what are the trained field model's input/output tensor names, the face radius (`ResidualPredictor.NeighbourStencil`), and the component interleave; the companion-published model signature (the model fit offline by the `python:geometry` science companion, arriving as one content-addressed ONNX artifact over `Runtime/wire#PROTO_VOCABULARY` and warmed from the `Model/inference#RESULT_CACHE`), at the residual-codec admission gate.
- [TILE_CONTENT]-[OPEN]: what is the Bim tile-emit codec entry the leaf-tile b3dm/glTF content encode grounds against; the `Rasm.Bim` glTF/tile codec surface at cross-package alignment.
- [EXT_STRUCTURAL_METADATA]-[OPEN]: what is the `EXT_structural_metadata` property-table buffer-view binary layout and the `EXT_mesh_features` `_FEATURE_ID_0` vertex-attribute emit against the SharpGLTF `ExtensionsFactory`/`JsonSerializable` write surface; the SharpGLTF extension API (`Rasm.Compute/.api` catalogue) and the 3D-Tiles 1.1 / glTF `EXT_structural_metadata` spec at the leaf-emit admission gate.
- [ARTIFACT_INDEX_ROW]-[OPEN]: what classification value the interchange artifact carries at `InterchangeIdentity.Admit`; `ArtifactKind.Interchange` owns retention and `sourceKey` is explicit at the Persistence seam.
- [FLIGHT_SQL_PUSH]-[BLOCKED]: Persistence `Query/columnar` `Land(LandingArm.Doe, …)` already redeems the content-keyed DOE batch server-side over `Apache.Arrow.Flight.Sql`, and Compute crosses the sealed `RecordBatch` over the existing `Runtime/transport` wire plane (no Compute Flight listener) — unresolved is the exact transport framing of the batch bytes and a reciprocal `LandingArm` for the `ChargebackDataset` billing batch; bind the framing, add the billing landing arm.
