# [COMPUTE_CODECS]

Rasm.Compute interchange lane: the compute-and-transport half of artifact interchange, owning the chunked error-bounded field/result codec over the simulation-field carrier, the FastCDC structural geometry-delta codec over meshes, B-reps, point clouds, and NURBS, the two-hop IFC-to-geometry tessellation bridge that crosses geometry evaluation to the IfcOpenShell companion and re-imports the GLB, the 3D-Tiles streamable-LOD octree partition over the imported geometry carrier with its `EXT_structural_metadata` semantic layer joining the IFC classification and the solver field values at the content-key, and the content-addressed artifact identity that folds the format key plus the deflection and tolerance policy into one `XxHash128` key. The page owns the `FieldCodec` and `DeltaCodec` codecs, the `TessellationRequest` companion bridge, the `TileSet` octree partition with its `MetadataProperty`/`PropertyTable`/`TileMetadata`/`FeatureBand` metadata family, and the `InterchangeIdentity` content-key — composing the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` identity precedent, the `solver#DISCRETIZATION_MESH` `FieldSpace` shape, the SharpGLTF raw glTF-extension write surface, the meshoptimizer LOD kernels, and the `Substrate.RemoteGrpc` companion hop as settled vocabulary. The IFC/glTF/STEP semantic object model and its format/codec/frame import-export surface are owned by `Rasm.Bim` and reached at the companion seam; this page is HOST-LOCAL and carries no TS_PROJECTION.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                       |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|   [1]   | TWO_HOP_TESSELLATION | IFC/AP242/native geometry crosses to the companion, never in-proc            |
|   [2]   | FIELD_RESULT_CODEC   | Chunked simulation-field layout; error-bounded lossy/lossless; zero-copy     |
|   [3]   | GEOMETRY_DELTA       | FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; progressive |
|   [4]   | TILE_PARTITION       | 3D-Tiles octree partition; streamable LOD over the content-keyed geometry    |
|   [5]   | CONTENT_ADDRESSING   | XxHash128 artifact identity folding deflection and tolerance into the key    |

## [2]-[TWO_HOP_TESSELLATION]

- Owner: `TessellationRequest` — the two-hop bridge that crosses IFC geometry evaluation to the IfcOpenShell companion (`IfcConvert` producing GLB) and re-imports the GLB through the Bim glTF import path; the request is host-local in posture and rides the existing remote-lane companion rpc, never a new transport; `ImportedGeometry` the decoded mesh-scene carrier the re-import lands and the tile partition reads; `InterchangePolicy` the deflection/tolerance/tile-partition policy folded into the content-key.
- Entry: `public static Fin<TessellationRequest> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; the companion round-trip rides the existing `remote#PROTO_VOCABULARY` `Solve`/artifact transport — the GLB result re-enters through the Bim glTF import rail as an `ImportedGeometry`.
- Auto: `Plan` reads the source format's companion-tessellation flag to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference to the Persistence artifact index rather than re-crossing the companion.
- Receipt: the `RemoteCall` receipt carries the companion transport, the IFC content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `TessellationRequest` folded into the content-key; zero new surface.
- Boundary: the two-hop rail is the single IFC-to-geometry path because the Bim IFC object model carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry`, never a NuGet pin, and it is reached only through the existing remote-lane companion rpc so this page mints no transport, no channel, and no second wire vocabulary — the host-local posture means an in-process Rhino host crosses to the companion process over the same UDS/InProcess leg `remote#TRANSPORT_AXIS` owns and a remote tessellation rides that same companion rpc; the GLB the companion returns re-enters the Bim glTF import rail so the decoded mesh is one `ImportedGeometry` shape, and the IFC semantic graph (owned by `Rasm.Bim`) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the content-key.

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
    public static Fin<TessellationRequest> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy) =>
        requiresCompanion
            ? Fin.Succ(new TessellationRequest(
                InterchangeIdentity.Key(formatKey, ifcBytes.Span, policy.Deflection, policy.Tolerance, policy.AngleTolerance), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, "glb"))
            : Fin.Fail<TessellationRequest>(new ComputeFault.ModelRejected($"<tessellation-not-required:{formatKey}>"));

    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}
```

## [3]-[FIELD_RESULT_CODEC]

- Owner: `FieldCodecPolicy` the chunked-layout and error-bound policy record carrying the residual-predict column; `ResidualPredictor` the content-keyed model-lane chunk predictor; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `PointScan` the point-cloud carrier over E57/LAS/LAZ/PTS; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with error-bounded lossy, learned-residual-predicted, or exact lossless residence and a zero-copy solver↔store↔viz handoff; `InterchangeIo` the scientific-data ingest surface dispatching the chunked field decode and the point-scan ingest, the geometry and IFC import arms owned by `Rasm.Bim`.
- Entry: `public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks)` reads a chunked field through `FieldCodec.FieldDecode`; `public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` reads a point-cloud scan; `public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at)` reads a chunked field artifact into the integration-point/nodal field carrier; `public static Fin<ExportArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at)` emits the chunked layout with the policy error bound; `Fin<T>` aborts on a chunk-shape mismatch or an error bound the lossy quantizer cannot meet.
- Auto: the codec chunks the field by the policy chunk shape so a large solve result streams chunk-by-chunk through the `staging#STREAM_POOL` `GetReadOnlySequence` zero-copy read, never a flattened array; the lossy column quantizes each chunk to the policy bit budget and the residual stays below the relative error bound (a chunk whose quantization exceeds the bound falls back to lossless), the residual column predicts each grid-coordinate-indexed chunk from its grid neighbours through the `ResidualPredictor` model-lane ONNX field model and quantizes only the residual against the prediction (a smooth converged field stores far past the per-chunk quantizer at the same bound, and a chunk whose residual exceeds the bound falls through to the lossless deflate with no new failure mode), the lossless column deflates the raw bytes, and the zero-copy handoff wraps the chunk window with `UnsafeByteOperations.UnsafeWrap` so the solver field, the store blob, and the viz upload are one buffer; the chunk index keys each chunk by its grid coordinate so a viewport reads only the chunks its frustum intersects and the residual predictor sees true spatial neighbours.
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy or residual-predicted encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, Microsoft.IO.RecyclableMemoryStream, System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new chunked field format is one row on the `field-chunk` codec owned by the Bim format axis; a new point-scan format is one row on the `point-cloud` codec owned by the Bim format axis; a new error-bound policy is one column on `FieldCodecPolicy`; a learned predictor is one `ResidualPredictor` content-keyed ONNX session reused across chunks; zero new surface — a `ResidualCoder`/`NeuralFieldCompressor` sibling is collapsed onto the `FieldCodecPolicy.ResidualPredict` column and the one `ResidualEncode` fold.
- Boundary: the field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor solve field rides the `solver#DISCRETIZATION_MESH` `FieldSpace` shape, so the codec chunks by station and component, never a generic byte blob; the chunked layout composes the suite `XxHash128` chunk identity and the Persistence blob lane content-addressed, so a re-emitted identical chunk dedups and a re-read warms from the store — a second field store is the rejected form; the lossy quantizer's error bound is a typed policy column the receipt records, so an error-bounded compression never silently exceeds its bound; the zero-copy edge is the same `GetReadOnlySequence`/`UnsafeWrap` path the remote frame law owns, so a field chunk crosses solver→store→viz without a managed copy — a `ToArray` flatten on the field path is the named defect; the residual row is the learned-compression terminal — the `ResidualPredictor` is one model-lane `inference#INFERENCE_MODES` ONNX session content-keyed by the parametric-family digest and shared across every chunk so the codec composes the model lane it sits beside rather than minting a second inference path, the grid-coordinate chunk index is preserved (never reduced to content-defined byte chunking, which destroys the grid-coordinate locality the predictor depends on — the FastCDC `#GEOMETRY_DELTA` chunker is the rejected rewrite of this codec) so the predictor sees true spatial neighbours, only the bounded residual against the prediction stores and a chunk whose residual exceeds the bound falls back to the lossless deflate (no new failure mode), the predictor weights are one content-addressed ONNX artifact the Python offline-science companion fits and returns over the same offline-training seam the optimizer surrogate uses (never an in-proc fit), and the achieved residual stays auditable on the `Cache` receipt; the `PointScan` ingest carries the `point-cloud` codec discriminant the Bim format axis names and faults `point-catalogue-pending` until the E57/LAS/LAZ/PTS reader package is admitted; the geometry mesh decode and the IFC semantic ingest are the `Rasm.Bim` import rail, never re-derived here — an `ImportGeometry`/`ImportIfc` arm in this surface is the deleted form.

```csharp signature
public sealed record FieldCodecPolicy(int[] ChunkShape, bool Lossy, int QuantizationBits, double RelativeErrorBound, bool Deflate, bool ResidualPredict) {
    public static readonly FieldCodecPolicy Lossless = new(ChunkShape: [64, 64, 64], Lossy: false, QuantizationBits: 0, RelativeErrorBound: 0.0, Deflate: true, ResidualPredict: false);
    public static readonly FieldCodecPolicy Bounded = new(ChunkShape: [64, 64, 64], Lossy: true, QuantizationBits: 12, RelativeErrorBound: 1e-3, Deflate: true, ResidualPredict: false);
    public static readonly FieldCodecPolicy Residual = Bounded with { ResidualPredict = true };
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
    public Fin<float[]> Predict(ReadOnlySpan<float> neighbours, int chunkElements) {
        float[] stencil = neighbours.ToArray();
        return Session.Infer(Options, Scope, RunOps.Bind(new RunInput.Managed<float>(InputName, stencil, [1, stencil.Length])), Seq(OutputName),
            results => {
                ReadOnlySpan<float> predicted = results.First().GetTensorDataAsSpan<float>();
                return predicted.Length >= chunkElements ? Fin.Succ(predicted[..chunkElements].ToArray()) : Fin.Fail<float[]>(new ComputeFault.ModelRejected($"<residual-predict-undersized:{predicted.Length}<{chunkElements}>"));
            });
    }
}

public sealed record FieldArtifact(
    string FormatKey,
    string Station,
    int Rank,
    int Components,
    long Count,
    int[] ChunkShape,
    int ChunkCount,
    ReadOnlyMemory<byte> Chunks,
    double MaxResidual,
    Instant At);

public sealed record PointScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    Option<ReadOnlyMemory<float>> Colors,
    Option<ReadOnlyMemory<float>> Intensity,
    long PointCount,
    Instant At);

public static class InterchangeIo {
    public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks) =>
        codecKey == "field-chunk"
            ? FieldCodec.FieldDecode(formatKey, bytes, policy, clocks.Now)
            : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{formatKey}>"));

    public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        codecKey != "point-cloud"
            ? Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{formatKey}>"))
            : Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-catalogue-pending:{formatKey}:e57-las-laz-pts-reader-unadmitted>"));
}

public static class FieldCodec {
    public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) =>
        Try.lift(() => Decode(formatKey, bytes, policy, at)).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    public static Fin<ExportArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default) {
        var encoded = policy.ResidualPredict && predictor.Case is ResidualPredictor net
            ? ResidualEncode(field, policy, net)
            : policy.Lossy ? Quantize(field, policy) : Raw(field, policy);
        var packed = Pack(encoded, policy);
        return encoded.MaxResidual <= policy.RelativeErrorBound || !policy.Lossy
            ? Fin.Succ(new ExportArtifact(formatKey, packed, InterchangeIdentity.Key(formatKey, packed, InterchangePolicy.Canonical.Deflection, InterchangePolicy.Canonical.Tolerance, InterchangePolicy.Canonical.AngleTolerance), packed.LongLength, at))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<field-error-bound:{encoded.MaxResidual:R}>{policy.RelativeErrorBound:R}"));
    }

    static FieldArtifact ResidualEncode(FieldArtifact field, FieldCodecPolicy policy, ResidualPredictor predictor) {
        var source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span);
        int chunkElements = policy.ChunkShape.Aggregate(1, static (acc, dim) => acc * dim) * field.Components;
        if (chunkElements <= 0 || source.Length < chunkElements) { return policy.Lossy ? Quantize(field, policy) : Raw(field, policy); }
        var residual = new float[source.Length];
        float scale = MathF.Max(MathF.Abs(TensorPrimitives.Max(source)), MathF.Abs(TensorPrimitives.Min(source)));
        float step = scale / ((1 << policy.QuantizationBits) - 1);
        double worst = 0.0;
        for (int chunk = 0; chunk * chunkElements < source.Length; chunk++) {
            int start = chunk * chunkElements, length = Math.Min(chunkElements, source.Length - start);
            ReadOnlySpan<float> stencil = source.Slice(Math.Max(0, start - predictor.NeighbourStencil), Math.Min(predictor.NeighbourStencil + length, source.Length - Math.Max(0, start - predictor.NeighbourStencil)));
            float[] prediction = predictor.Predict(stencil, length).IfFail(static () => Array.Empty<float>());
            for (int index = 0; index < length; index++) {
                float predicted = index < prediction.Length ? prediction[index] : 0f;
                float delta = source[start + index] - predicted;
                float coded = step == 0f ? delta : MathF.Round(delta / step) * step;
                residual[start + index] = coded;
                worst = Math.Max(worst, scale == 0f ? 0.0 : Math.Abs(delta - coded) / scale);
            }
        }
        return field with { Chunks = MemoryMarshal.AsBytes(residual.AsSpan()).ToArray(), MaxResidual = worst };
    }

    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) =>
        new(field.Chunks);

    static FieldArtifact Decode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) {
        var span = bytes.Span;
        var (station, rank, components, count) = (Encoding.ASCII.GetString(span[..16]).TrimEnd('\0'),
            BinaryPrimitives.ReadInt32LittleEndian(span[16..]), BinaryPrimitives.ReadInt32LittleEndian(span[20..]),
            BinaryPrimitives.ReadInt64LittleEndian(span[24..]));
        var payload = policy.Deflate ? Inflate(bytes[32..]) : bytes[32..];
        int chunkBytes = policy.ChunkShape.Aggregate(1, static (acc, dim) => acc * dim) * components * sizeof(float);
        int chunkCount = (payload.Length + chunkBytes - 1) / Math.Max(chunkBytes, 1);
        return new FieldArtifact(formatKey, station, rank, components, count, policy.ChunkShape, chunkCount, payload, 0.0, at);
    }

    static FieldArtifact Raw(FieldArtifact field, FieldCodecPolicy policy) =>
        field with { MaxResidual = 0.0 };

    static FieldArtifact Quantize(FieldArtifact field, FieldCodecPolicy policy) {
        var source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span);
        var quantized = new float[source.Length];
        float scale = MathF.Max(MathF.Abs(TensorPrimitives.Max(source)), MathF.Abs(TensorPrimitives.Min(source)));
        float step = scale / ((1 << policy.QuantizationBits) - 1);
        double residual = 0.0;
        for (int index = 0; index < source.Length; index++) {
            quantized[index] = step == 0f ? source[index] : MathF.Round(source[index] / step) * step;
            residual = Math.Max(residual, scale == 0f ? 0.0 : Math.Abs(source[index] - quantized[index]) / scale);
        }
        return field with { Chunks = MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), MaxResidual = residual };
    }

    static byte[] Pack(FieldArtifact field, FieldCodecPolicy policy) {
        var header = new byte[32];
        Encoding.ASCII.GetBytes(field.Station.PadRight(16, '\0')[..16]).CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), field.Rank);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(20), field.Components);
        BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(24), field.Count);
        var body = policy.Deflate ? Deflate(field.Chunks) : field.Chunks;
        return [.. header, .. body.Span];
    }

    static ReadOnlyMemory<byte> Deflate(ReadOnlyMemory<byte> data) {
        using var sink = new RecyclableMemoryStream(RecyclableMemoryStreamManager.Default);
        using (var brotli = new BrotliStream(sink, CompressionLevel.Optimal, leaveOpen: true)) { brotli.Write(data.Span); }
        return sink.GetReadOnlySequence().ToArray();
    }

    static ReadOnlyMemory<byte> Inflate(ReadOnlyMemory<byte> data) {
        using var source = new MemoryStream(data.ToArray());
        using var brotli = new BrotliStream(source, CompressionMode.Decompress);
        using var sink = new RecyclableMemoryStream(RecyclableMemoryStreamManager.Default);
        brotli.CopyTo(sink);
        return sink.GetReadOnlySequence().ToArray();
    }
}
```

## [4]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` structural-diff target rows; `GeometryDelta` the content-addressed delta record; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control.
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the changed-chunk set; `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` reconstructs the target from the base plus the delta; `Fin<T>` aborts on a base-hash mismatch.
- Auto: `Diff` runs FastCDC content-defined chunking (a rolling hash splits each artifact at content boundaries so an inserted vertex shifts only the local chunks, never the whole stream) over the columnar layout the geometry kind declares — mesh vertices in a position column, topology in an index column, B-rep faces by face id, point-cloud points by octant cell, NURBS by control-point grid — then diffs the chunk hash sets and emits the added/removed chunk ids; the quantization-aware column quantizes a vertex/control-point chunk to the policy bit budget so the delta is bounded-lossy within a tolerance, and the progressive column orders the changed chunks coarse-to-fine so a transmission renders a coarse target first and refines; the delta keys on the base and target closure hashes so it round-trips deterministically.
- Receipt: the `Cache` receipt carries the delta content-key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable; a progressive transmission stamps the coarse-chunk-first ordering count.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, System.Numerics.Tensors, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row with its columnar-layout column; a new chunk policy is one column on `DeltaPolicy`; zero new surface.
- Boundary: the geometry delta is the structural diff the blob-level delta never owned — the existing Persistence blob delta diffs opaque bytes, this codec diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only the touched chunks, and the diff algebra mirrors the `remote#PROTO_VOCABULARY` `GraphDiff`/`SubtreeFetch` wire shape Compute already owns — Compute owns the structural chunking and the Persistence sync lane owns the closure-graph diff, neither re-deriving the other; FastCDC content-defined chunking is the standard rolling-hash boundary so a local edit shifts local chunks only, and a fixed-block chunker that re-chunks the whole stream on an insert is the rejected form; the quantization-aware bounded-lossy column carries its tolerance so a delta never silently exceeds the geometry tolerance; the changed-chunk set transmits progressively through the `SubtreeFetch` server-stream and the content-key dedups against the Persistence blob lane, never a second delta store; the columnar layout is the geometry-kind column, so a position-only edit never re-transmits the topology column.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
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
    Seq<DeltaChunk> Added,
    Seq<UInt128> Removed,
    ReadOnlyMemory<byte> Payload,
    long BaseBytes,
    long DeltaBytes);

public static class DeltaCodec {
    public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy) {
        var baseChunks = FastCdc(baseBytes.Span, policy);
        var targetChunks = FastCdc(targetBytes.Span, policy);
        var baseSet = baseChunks.Map(static c => c.Hash).ToHashSet();
        var added = targetChunks.Filter(c => !baseSet.Contains(c.Hash));
        var targetSet = targetChunks.Map(static c => c.Hash).ToHashSet();
        var removed = baseChunks.Map(static c => c.Hash).Filter(h => !targetSet.Contains(h));
        var ordered = policy.Progressive ? added.OrderByDescending(static c => c.GeometricError).ToSeq() : added;
        return Fin.Succ(new GeometryDelta(kind, XxHash128.HashToUInt128(baseBytes.Span), XxHash128.HashToUInt128(targetBytes.Span),
            ordered, removed, Concatenate(ordered, targetBytes), baseBytes.LongLength, ordered.Sum(static c => (long)c.ByteLength)));
    }

    public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        XxHash128.HashToUInt128(baseBytes.Span) == delta.BaseHash
            ? Fin.Succ(Reconstruct(delta, baseBytes))
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-base-mismatch:{delta.BaseHash:x32}>"));

    static Seq<DeltaChunk> FastCdc(ReadOnlySpan<byte> data, DeltaPolicy policy) {
        var chunks = Seq<DeltaChunk>();
        int start = 0, ordinal = 0;
        while (start < data.Length) {
            int cut = ContentDefinedCut(data[start..], policy);
            var slice = data.Slice(start, cut);
            chunks = chunks.Add(new DeltaChunk(XxHash128.HashToUInt128(slice), ordinal++, start, cut, 0.0));
            start += cut;
        }
        return chunks;
    }

    static int ContentDefinedCut(ReadOnlySpan<byte> window, DeltaPolicy policy) {
        ulong fingerprint = 0;
        int normal = policy.AvgChunk;
        for (int index = policy.MinChunk; index < Math.Min(window.Length, policy.MaxChunk); index++) {
            fingerprint = (fingerprint << 1) + window[index];
            if (index >= normal && (fingerprint & ((1UL << 13) - 1)) == 0) { return index; }
        }
        return Math.Min(window.Length, policy.MaxChunk);
    }

    static ReadOnlyMemory<byte> Concatenate(Seq<DeltaChunk> added, ReadOnlyMemory<byte> targetBytes) {
        int total = added.Sum(static c => c.ByteLength + sizeof(int) * 2 + 16);
        var buffer = new byte[total];
        var sink = buffer.AsSpan();
        int cursor = 0;
        foreach (var chunk in added) {
            MemoryMarshal.Write(sink[cursor..], in Unsafe.AsRef(in chunk.Hash));
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 16)..], chunk.Ordinal);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 20)..], chunk.ByteLength);
            targetBytes.Span.Slice(chunk.Offset, chunk.ByteLength).CopyTo(sink[(cursor + 24)..]);
            cursor += 24 + chunk.ByteLength;
        }
        return buffer.AsMemory(0, cursor);
    }

    static ReadOnlyMemory<byte> Reconstruct(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) {
        var addedByHash = SplitPayload(delta.Payload);
        var removedSet = delta.Removed.ToHashSet();
        var baseChunks = FastCdc(baseBytes.Span, DeltaPolicy.Canonical)
            .Filter(c => !removedSet.Contains(c.Hash))
            .Map(c => baseBytes.Slice(c.Offset, c.ByteLength));
        var addedChunks = delta.Added.OrderBy(static c => c.Ordinal).Map(c => addedByHash[c.Hash]);
        var pieces = baseChunks.Append(addedChunks).ToSeq();
        var target = new byte[pieces.Sum(static p => p.Length)];
        int cursor = 0;
        foreach (var piece in pieces) { piece.Span.CopyTo(target.AsSpan(cursor)); cursor += piece.Length; }
        return target;
    }

    static System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>> SplitPayload(ReadOnlyMemory<byte> payload) {
        var map = new System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>>();
        int cursor = 0;
        while (cursor < payload.Length) {
            var hash = MemoryMarshal.Read<UInt128>(payload.Span[cursor..]);
            int byteLength = BinaryPrimitives.ReadInt32LittleEndian(payload.Span[(cursor + 20)..]);
            map[hash] = payload.Slice(cursor + 24, byteLength);
            cursor += 24 + byteLength;
        }
        return map;
    }
}
```

## [5]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the imported geometry carrier; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf metadata layer joining the IFC classification column and the solver field-value columns under one feature-id mapping; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `ExportTiles` the leaf-tile emit fold riding the content-key, the metadata layer, and the field/tile compute lane; the partition consumes the deflection/tolerance and tile-depth/error/split scalars from `InterchangePolicy` and the `InterchangeIdentity.Key` content-key, never the Bim format/codec/KHR surface.
- Entry: `public static Fin<Seq<ExportArtifact>> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, InterchangePolicy policy, ClockPolicy clocks)` builds the octree, attaches the per-leaf metadata read at the node content-key, and emits the leaf tiles; `public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, InterchangePolicy policy, ClockPolicy clocks)` partitions the geometry into the depth-bounded octree; `Fin<T>` aborts on a tile-content emit miss projected onto `ComputeFault.ModelRejected`.
- Auto: `Build` partitions the geometry octant-by-octant to the policy max depth or the triangle split threshold, computing the geometric error as the root error halved per depth and the per-node content-key over the node geometry through `InterchangeIdentity.Key` so a re-partition of identical geometry at identical settings keys identically, then reads the per-leaf `TileMetadata` at the node content-key so the same key addresses geometry and metadata; `ExportTiles` flattens the octree, filters to the leaves, and emits each leaf tile content keyed on its node content-key with the attached `EXT_structural_metadata` property table and `EXT_mesh_features` feature-id mapping; `TileMetadata.Join` folds the `Rasm.Bim` IFC classification projection and the `solver#DISCRETIZATION_MESH` `FieldSpace` per-element field values read at the shared content-key into one feature-keyed property table, and `PropertyTable.Pack` lays each `MetadataProperty` column out as a contiguous buffer-view body the leaf-tile emit references; `FeatureBand.Of` classifies an achieved per-element field value onto its styling band so the viewer styles by solved field without recomputing the metric.
- Receipt: the `StreamSegment` receipt carries the leaf-tile count, the root geometric error, the max depth, the node count, and the per-leaf property-column count; emission rides the sink port.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, meshoptimizer, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new tile-partition parameter is one column on `InterchangePolicy` folded into the partition; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.
- Boundary: the 3D-Tiles partition is the streamable-LOD octree over the content-keyed geometry the field/tile compute lane owns — it rides `InterchangeIdentity.Key` and the imported-geometry carrier, so the partition stays a compute concern while the b3dm/glTF tile content encode is the Bim glTF codec the leaf emit composes; the metadata layer is one schema column on the leaf-tile content emit, never a parallel attribute store or a second tiling owner — the IFC classification reads the `Rasm.Bim` IFC semantic graph at the shared content-key (the companion seam aligned to a named boundary, never reaching into the Bim interior) and the per-element field values read the `solver#DISCRETIZATION_MESH` `FieldSpace` achieved value (never a recomputed metric), so the IFC semantic graph and the tessellated geometry stay two projections of the one content-keyed IFC artifact joined at the tile boundary and a re-tessellation at a new deflection re-keys geometry and metadata together; the `EXT_structural_metadata` property tables and the `EXT_mesh_features` feature-id attribute are vendor glTF extensions SharpGLTF does not model natively (the in-box `KnownChannel` extension surface and `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` carry only the material-PBR and registered-extension framework), so the schema, the property-table buffer-view columns, and the feature-id vertex attribute emit through the raw glTF-extension write surface against a `JsonSerializable`-derived extension class registered before write — the binary layout (property-table column buffer views, feature-id `_FEATURE_ID_0` attribute) is the `[EXT_STRUCTURAL_METADATA]` RESEARCH leaf against the SharpGLTF extension API and the 3D-Tiles 1.1 spec, never an assumed SharpGLTF helper; meshoptimizer owns the leaf-tile `Meshopt.Simplify`/`OptimizeVertexCache` LOD optimization the octree leaf geometry rides, never a hand-rolled simplifier; the leaf-tile content body emit faults `tile-content-catalogue-pending` until the Bim tile-emit codec is admitted, the row, octree, metadata schema, and quantization-bit policy owned here; a tile partition that re-derives the glTF tile content body in-place or a metadata layer that re-reads the IFC parser is the rejected form.

```csharp signature
public sealed record InterchangePolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        TileMaxDepth: 16, TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class FeatureBand {
    public static readonly FeatureBand Nominal = new("nominal", upperFraction: 0.5);
    public static readonly FeatureBand Elevated = new("elevated", upperFraction: 0.8);
    public static readonly FeatureBand Critical = new("critical", upperFraction: 1.0);

    public double UpperFraction { get; }

    public static FeatureBand Of(double value, double minimum, double maximum) {
        double span = Math.Max(1e-12, maximum - minimum);
        double fraction = Math.Clamp((value - minimum) / span, 0.0, 1.0);
        return Items.Filter(row => fraction <= row.UpperFraction).HeadOrNone().IfNone(Critical);
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
        var views = Seq<(string, int, int, string)>();
        int cursor = 0;
        var segments = new List<byte[]>();
        foreach (var column in Columns) {
            byte[] bytes = column.ColumnBytes.ToArray();
            views = views.Add((column.PropertyName, cursor, bytes.Length, column.ComponentType));
            segments.Add(bytes);
            cursor += bytes.Length;
        }
        var buffer = new byte[cursor];
        int slot = 0;
        foreach (var segment in segments) { segment.CopyTo(buffer.AsSpan(slot)); slot += segment.Length; }
        return (buffer, views);
    }
}

public sealed record TileMetadata(UInt128 ContentKey, PropertyTable Table, ReadOnlyMemory<int> FeatureIds) {
    public static TileMetadata Join(UInt128 contentKey, string ifcClass, Seq<string> classification, ReadOnlyMemory<float> fieldValues, string fieldUnit, double minimum, double maximum, ReadOnlyMemory<int> featureIds) {
        var bands = toSeq(fieldValues.ToArray().Select(value => FeatureBand.Of(value, minimum, maximum).Key));
        var columns = Seq<MetadataProperty>(
            new MetadataProperty.Classification("ifc-class", classification),
            new MetadataProperty.Scalar("field-value", fieldUnit, fieldValues),
            new MetadataProperty.Banded("field-band", fieldValues, bands));
        return new TileMetadata(contentKey, new PropertyTable(ifcClass, classification.Count, columns), featureIds);
    }
}

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Option<TileMetadata> Metadata, Seq<TileNode> Children);

public sealed record TileSet(TileNode Root, double GeometricErrorRoot, int MaxDepth, int NodeCount, Instant At) {
    public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, InterchangePolicy policy, ClockPolicy clocks) {
        var root = Partition(geometry, metadata, policy, depth: 0);
        return new TileSet(root, policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(root), clocks.Now);
    }

    static TileNode Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, InterchangePolicy policy, int depth) {
        var bounds = Bounds(geometry);
        double error = policy.TileGeometricErrorRoot / Math.Pow(2, depth);
        var contentKey = InterchangeIdentity.Key(geometry.FormatKey, MemoryMarshal.AsBytes(geometry.Vertices.Span), policy.Deflection, policy.Tolerance, policy.AngleTolerance);
        return depth >= policy.TileMaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? new TileNode(depth, bounds, error, contentKey, metadata(contentKey), Seq<TileNode>())
            : new TileNode(depth, bounds, error, contentKey, None,
                Split(geometry, bounds).Map(child => Partition(child, metadata, policy, depth + 1)));
    }

    static int Count(TileNode node) => 1 + node.Children.Sum(Count);

    static float[] Bounds(ImportedGeometry geometry) {
        var verts = geometry.Vertices.Span;
        (float minX, float minY, float minZ) = (float.MaxValue, float.MaxValue, float.MaxValue);
        (float maxX, float maxY, float maxZ) = (float.MinValue, float.MinValue, float.MinValue);
        for (int offset = 0; offset + 2 < verts.Length; offset += 3) {
            (minX, minY, minZ) = (Math.Min(minX, verts[offset]), Math.Min(minY, verts[offset + 1]), Math.Min(minZ, verts[offset + 2]));
            (maxX, maxY, maxZ) = (Math.Max(maxX, verts[offset]), Math.Max(maxY, verts[offset + 1]), Math.Max(maxZ, verts[offset + 2]));
        }
        return [(minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2, (maxX - minX) / 2, 0, 0, 0, (maxY - minY) / 2, 0, 0, 0, (maxZ - minZ) / 2];
    }

    static Seq<ImportedGeometry> Split(ImportedGeometry geometry, float[] bounds) {
        (float cx, float cy, float cz) = (bounds[0], bounds[1], bounds[2]);
        return Range(0, geometry.TriangleCount)
            .GroupBy(tri => Octant(geometry.Vertices.Span, tri, cx, cy, cz))
            .Map(group => Tessellate(geometry, group.ToSeq()))
            .ToSeq();
    }

    static int Octant(ReadOnlySpan<float> verts, int triangle, float cx, float cy, float cz) {
        int v = triangle * 9;
        return (verts[v] >= cx ? 1 : 0) | (verts[v + 1] >= cy ? 2 : 0) | (verts[v + 2] >= cz ? 4 : 0);
    }

    static ImportedGeometry Tessellate(ImportedGeometry geometry, Seq<int> triangles) {
        var srcV = geometry.Vertices.Span;
        var srcN = geometry.Normals.Span;
        var vertices = new float[triangles.Count * 9];
        var normals = new float[triangles.Count * 9];
        var indices = new long[triangles.Count * 3];
        int slot = 0;
        foreach (int tri in triangles) {
            srcV.Slice(tri * 9, 9).CopyTo(vertices.AsSpan(slot * 9));
            srcN.Slice(tri * 9, 9).CopyTo(normals.AsSpan(slot * 9));
            (indices[slot * 3], indices[slot * 3 + 1], indices[slot * 3 + 2]) = (slot * 3, slot * 3 + 1, slot * 3 + 2);
            slot++;
        }
        return geometry with { Vertices = vertices, Normals = normals, Indices = indices, VertexCount = triangles.Count * 3, TriangleCount = triangles.Count };
    }
}

public static class TilePartition {
    public static Fin<Seq<ExportArtifact>> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, InterchangePolicy policy, ClockPolicy clocks) =>
        Tiled(TileSet.Build(geometry, metadata, policy, clocks)).Traverse(static result => result);

    static Seq<Fin<ExportArtifact>> Tiled(TileSet tiles) =>
        Flatten(tiles.Root)
            .Filter(static node => node.Children.IsEmpty)
            .Map(static node => {
                int columns = node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0);
                return Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<tile-content-catalogue-pending:{node.ContentKey:x32}:ext-structural-metadata-cols={columns}:b3dm-glb-tile-emit-unadmitted>"));
            });

    static Seq<TileNode> Flatten(TileNode node) =>
        node.Cons(node.Children.Bind(Flatten));
}
```

## [6]-[CONTENT_ADDRESSING]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeIdentity` — the content-key derivation folding the artifact bytes plus the deflection and tolerance policy into one `XxHash128` identity, mirroring the model-lane `ModelIdentity.Snapshot` precedent; `ExportArtifact` the emitted-bytes carrier the field, tile, and Bim export rails feed; the artifact lands content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` — pure value; identity derives from the bytes and the evaluation policy, never from a path or filename.
- Auto: the key seeds `XxHash128.HashToUInt128` over the artifact bytes with a seed mixing the format key, the deflection, the tolerance, and the angle tolerance so a re-tessellation at a different deflection keys distinctly and a re-import of identical bytes at identical settings keys identically — deflection and tolerance fold into the key, never a cross-setting hit; `Admit` projects the artifact onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns so the blob lane stores and serves the addressed bytes.
- Receipt: the `Cache` receipt carries the content-key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one column folded into the seed; zero new surface.
- Boundary: artifact identity is `XxHash128` over the canonical bytes — the suite hash law the `remote#ARTIFACT_FRAMES` whole-artifact identity row and the model-lane `ModelIdentity` checksum already hold, never a second hashing pass and never a path-keyed identity; the key takes a format-key string rather than the Bim `InterchangeFormat` owner so the content identity stays a Compute concern decoupled from the moved format axis; the deflection and tolerance fold into the seed so the geometry-evaluation settings partition the key and a coarse and a fine tessellation of the same IFC never collide — a cross-setting hit is the named defect; the addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` keyed on the content-key, the single artifact owner, so the IFC semantic graph (Bim), the tessellated GLB, the field artifact, and a re-exported glTF are content-keyed rows under one identity scheme the Persistence index owns — Compute owns the identity derivation and Persistence owns blob residence, neither re-declaring the other; a managed copy of the artifact bytes beside the blob lane is the rejected form.

```csharp signature
public sealed class InterchangeKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

public sealed record ExportArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

public static class InterchangeIdentity {
    public static UInt128 Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance) =>
        XxHash128.HashToUInt128(bytes, Seed(formatKey, deflection, tolerance, angleTolerance));

    public static ArtifactIndexRow Admit(ExportArtifact artifact, DataClassification classification, string retentionClass) =>
        ArtifactIndexRow.Admit(ArtifactIndexRow.Interchange, $"{artifact.ContentKey:x32}:{artifact.FormatKey}", artifact.Bytes.ToArray(), classification, retentionClass, artifact.At);

    static long Seed(string formatKey, double deflection, double tolerance, double angleTolerance) =>
        unchecked((long)XxHash3.HashToUInt64(MemoryMarshal.AsBytes($"{formatKey}|{deflection:R}|{tolerance:R}|{angleTolerance:R}".AsSpan())));
}
```

## [7]-[RESEARCH]

- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the two-hop tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by the Python geometry companion and rides the remote-lane companion rpc; the `TessellationRequest` shape and the content-key cache-reuse are authored here.
- [FIELD_FORMAT]: the CGNS/EnSight/VTK/Zarr chunked-field decode member spellings ground against the admitted field-format library surface, the field-format row vocabulary is owned by the Bim format axis, and the decode body grounds at the field-codec admission gate.
- [RESIDUAL_PREDICTOR]: the `ResidualPredictor` neighbour-stencil-to-chunk ONNX field model is fit offline by the Python geometry-science companion and arrives as one content-addressed ONNX artifact keyed by the field-family digest over the `remote#PROTO_VOCABULARY` artifact transport — C# owns only the per-chunk `RunOps.Infer` inference and the residual fold; the stencil shape (neighbour count, component interleave) and the trained model's input/output tensor names ground against the companion-published model signature at the residual-codec admission gate, and the predictor warms from the same `inference#RESULT_CACHE` model-lane cache the optimizer neural-field surrogate uses.
- [TILE_CONTENT]: the 3D-Tiles tileset b3dm/glTF tile content schema and the leaf-tile content encode ride the Bim glTF codec; the `TileSet` octree partition, the per-node content-key, the `MetadataProperty`/`PropertyTable`/`TileMetadata` semantic-layer schema, the `FeatureBand` styling rows, and the quantization-bit policy are owned here and the leaf-tile content emit grounds against the Bim tile-emit codec at cross-package alignment.
- [EXT_STRUCTURAL_METADATA]: the `EXT_structural_metadata` property-table buffer-view layout and the `EXT_mesh_features` `_FEATURE_ID_0` vertex-attribute emit ride the SharpGLTF raw glTF-extension write surface — SharpGLTF.Core 1.0.6 models neither vendor extension natively (the in-box extension surface is the `KnownChannel` material-PBR projection and `ExtensionsFactory.RegisterExtension<TParent,TExt>(string name)` for a caller-supplied `JsonSerializable`-derived extension class registered before write), so the schema-class declaration, the property-table class/property JSON, and the feature-id accessor binding ground against the SharpGLTF `ExtensionsFactory`/`JsonSerializable` extension API and the 3D-Tiles 1.1 / glTF `EXT_structural_metadata` spec at the leaf-emit admission gate; the `PropertyTable.Pack` column buffer and the `MetadataProperty` typed columns are authored here. The leaf-tile geometry LOD rides `Meshopt.Simplify(destination, indices, index_count, positions, vertex_count, stride, target, error, options, out error_out)` and `Meshopt.OptimizeVertexCache(destination, indices, index_count, vertex_count)` over the per-octant geometry, the simplification target and `SimplificationOptions` band owned by the octree depth.
- [ARTIFACT_INDEX_ROW]: the `ArtifactIndexRow.Interchange` kind row on the Persistence artifact-blob index carries every interchange artifact (tessellated GLB, chunked field, tile content, re-exported glTF) beside `EpContext`, `OnnxProfile`, `IfcSemantic`, and `ChunkContent` — the row is settled on the `Persistence/cache/indexes#ARTIFACT_BLOB_INDEX` owner and Compute consumes the `interchange` kind constant as settled vocabulary; the residual is the classification/retention column values the interchange artifact carries at `InterchangeIdentity.Admit`, confirmed against the Persistence retention axis at cross-folder alignment.
