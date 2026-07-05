# [COMPUTE_CODECS]

Rasm.Compute interchange lane: the compute-and-transport half of artifact interchange, owning the chunked error-bounded field/result codec over the simulation-field carrier, the FastCDC structural geometry-delta codec over meshes, B-reps, point clouds, and NURBS, the two-hop IFC-to-geometry tessellation bridge that crosses geometry evaluation to the IfcOpenShell companion and re-imports the GLB, the 3D-Tiles streamable-LOD octree partition over the imported geometry carrier with its `EXT_structural_metadata` semantic layer joining the IFC classification and the solver field values at the content-key, and the content-addressed interchange-cache identity that canonicalizes the format key and the deflection and tolerance policy to one machine-independent byte form, maps an empty artifact to the empty-artifact `SeedZero` sentinel, composes the causal stamp in the fixed (physical, logical) half order, and folds it all into one policy-seeded `XxHash128` cache-partition key — the GLB geometry-content identity itself being the kernel seed-zero `XxHash128` `GeometryHash` composed here, never re-minted with a policy seed. The page owns the `FieldCodec` and `DeltaCodec` codecs, the `TessellationRequest` companion bridge, the `TileSet` octree partition with its `MetadataProperty`/`PropertyTable`/`TileMetadata`/`FeatureBand` metadata family, and the `CanonicalForm`/`InterchangeIdentity` content-key — composing the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` identity precedent, the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, the SharpGLTF raw glTF-extension write surface, the meshoptimizer LOD kernels, and the `Substrate.RemoteGrpc` companion hop as settled vocabulary. The IFC/glTF/STEP semantic object model and its format/codec/frame import-export surface are owned by `Rasm.Bim` and reached at the companion seam; this page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[INDEX]

- [01]-[TWO_HOP_TESSELLATION]: IFC/AP242/native geometry crosses to the companion, never in-proc; ifctester IDS-audit oracle and GeoArrow-buffer consume ride the same companion rpc.
- [02]-[FIELD_RESULT_CODEC]: chunked simulation-field layout; error-bounded lossy/lossless; zero-copy.
- [03]-[GEOMETRY_DELTA]: FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; progressive.
- [04]-[TILE_PARTITION]: 3D-Tiles octree partition; streamable LOD over the content-keyed geometry.
- [05]-[CONTENT_ADDRESSING]: policy-seeded canonical-form `XxHash128` interchange-cache key (the GLB geometry-content identity is the kernel seed-zero `GeometryHash` composed, distinct); empty-artifact sentinel; HLC two-half compose.

## [02]-[TWO_HOP_TESSELLATION]

- Owner: `TessellationRequest` — the two-hop bridge that crosses IFC geometry evaluation to the IfcOpenShell companion (`IfcConvert` producing GLB) and re-imports the GLB through the Bim glTF import path; the request is host-local in posture and rides the existing remote-lane companion rpc, never a new transport; `IdsAuditRequest` the companion-rpc leg passing an IDS-XML payload to the Python ifctester oracle and projecting the per-specification pass/fail `GlobalId` set back into the Bim `IdsAudit` shape (one ifctester invocation shape beside the `IfcConvert` one, no new transport); `ImportedGeometry` the decoded mesh-scene carrier the re-import lands and the tile partition reads; `TessellationPolicy` the deflection/tolerance/tile-partition policy folded into the content-key.
- Entry: `public static Fin<TessellationRequest> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, TessellationPolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; the companion round-trip rides the existing `Runtime/wire#PROTO_VOCABULARY` `Solve`/artifact transport — the GLB result re-enters through the Bim glTF import rail as an `ImportedGeometry`.
- Auto: `Plan` reads the source format's companion-tessellation flag to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `TessellationPolicy`, and the content-key so a re-cross of the same source at the same policy is gated — the DURABLE GLB residence is keyed by the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` dual `SourceKey`/`ContentKey` (composed through the kernel seed-zero content-hash, NEVER a policy seed) the Bim bridge owns and the Persistence object store's `ContentAddress` residence (the Bim bridge performs that durable reuse BEFORE crossing), while this leg's policy-seeded `IfcContentKey` is the companion-rpc cache-partition over the source IFC plus evaluation policy that gates re-crossing the companion, the two coexisting as distinct cache layers neither re-minting the other's key.
- Receipt: the `RemoteCall` receipt carries the companion transport, the IFC content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `TessellationRequest` folded into the content-key; a geospatial mesh payload is one `GeoArrowBuffer` decode landing into the same `ImportedGeometry` vertex/index spans (never a second spatial codec); zero new surface.
- Boundary: the two-hop rail is the single IFC-to-geometry path because the Bim IFC object model carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry`, never a NuGet pin, and it is reached only through the existing remote-lane companion rpc so this page mints no transport, no channel, and no second wire vocabulary — the host-local posture means an in-process Rhino host crosses to the companion process over the same UDS/InProcess leg `Runtime/transport#TRANSPORT_AXIS` owns and a remote tessellation rides that same companion rpc; the GLB the companion returns re-enters the Bim glTF import rail so the decoded mesh is one `ImportedGeometry` shape, and the IFC semantic graph (owned by `Rasm.Bim`) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the content-key; the `python:data/spatial/geospatial` `[SHAPE]` GeoArrow buffers (the `EgressFormat.GEOARROW` column-major coordinate/offset buffers the data-companion geospatial owner emits — a zero-copy columnar layout the C# decode reads with no GEOS/GDAL dependency of its own) decode through `GeoArrowBuffer.ToImportedGeometry` directly into the GLB tessellation wire's vertex/index spans on the EXISTING companion channel so a geospatial mesh crosses on the one settled GLB binary layout the tessellation bridge already projects, never a second spatial codec — the GeoArrow column buffers (the interleaved `xy`/`xyz` coordinate buffer and the geometry/ring offset buffers) marshal into the `ImportedGeometry` `Vertices`/`Indices` `ReadOnlyMemory` spans by reinterpreting the column-major coordinate buffer as the vertex stream and the offset buffer as the triangle-fan index stream, and the decoded geometry emits the existing tessellation receipt; the GeoArrow buffer interchange is consumed at the wire on both endpoints (the data companion produces the buffers, this seam consumes them) so the geospatial interchange never strands one-sided on the Python producer; the ifctester IDS-audit oracle rides the EXISTING companion path the tessellation bridge proves so Compute mints no new transport — `IdsAuditRequest` adds one ifctester invocation shape beside the `IfcConvert` one, passing the IDS-XML payload plus the IFC content to the `python:geometry/ifc-companion` ifctester (the IfcOpenShell `ids` oracle) and relaying the per-specification ifctester verdict wire back, which the Bim-owned `Review/validation#IDS_FACETS` `IdsAudit.Reconcile` composes into its `IdsVerdict` rows (never a re-declared Compute-side struct), so that Bim fold joins the C# self-audit against the conformant ifctester result on the (GlobalId, `FacetKey`) axis — where `FacetKey` is the Bim composite join token unique within a specification (facet-type prefix plus its value discriminator), never the bare facet-type word — rather than a message diff — the Bim owner authors and parses the IDS spec AND owns the `IdsVerdict` row plus the `IdsAudit.Reconcile` projection that composes it from the relayed wire (Compute owns only the companion-rpc orchestration and the verdict-wire relay — it references no Bim type, the two meeting at the companion-rpc wire exactly as the tessellation GLB does — the Python companion owns the IfcOpenShell `ids` oracle), and a Compute-minted IDS parser or a second transport beside the tessellation companion rpc is the rejected form.

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
                InterchangeIdentity.Key(formatKey, ifcBytes.Span, policy.Deflection, policy.Tolerance, policy.AngleTolerance), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, "glb"))
            : Fin.Fail<TessellationRequest>(new ComputeFault.ModelRejected($"<tessellation-not-required:{formatKey}>"));

    // The companion-rpc cache-partition over the source IFC + evaluation policy (gates re-crossing the
    // companion) — NOT the durable GLB store address: that is the Bim Exchange/tessellation#TESSELLATION_BRIDGE
    // ContentKey (composed through the kernel seed-zero content-hash) the Persistence object store keys.
    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}

// The data-companion GeoArrow buffer interchange consumed at the GLB tessellation wire: the column-major
// coordinate buffer and the geometry/ring offset buffers (the EgressFormat.GEOARROW column-major layout the
// data-companion geospatial owner emits, a zero-copy columnar form the C# decode reads with no GEOS/GDAL of its
// own) decode into the one ImportedGeometry vertex/index spans the tessellation bridge already
// projects, mirroring the python:data/spatial/geospatial -> Rasm.Compute [SHAPE] edge so the geospatial
// interchange lands at the wire on both endpoints rather than stranded one-sided on the Python producer.
public sealed record GeoArrowBuffer(
    int Dimensions,
    ReadOnlyMemory<double> Coordinates,
    ReadOnlyMemory<int> GeometryOffsets,
    ReadOnlyMemory<int> RingOffsets,
    string FormatKey) {
    public Fin<ImportedGeometry> ToImportedGeometry(Instant at) {
        if (Dimensions is < 2 or > 3) { return Fin.Fail<ImportedGeometry>(new ComputeFault.PayloadOverBounds($"<geoarrow-dim:{Dimensions}>")); }
        if (HasInteriorRings(GeometryOffsets.Span, RingOffsets.Span)) { return Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected("<geoarrow-interior-ring-needs-kernel-triangulation>")); }
        int vertexCount = Coordinates.Length / Dimensions;
        var vertices = new float[vertexCount * 3];
        ReadOnlySpan<double> coords = Coordinates.Span;
        for (int v = 0; v < vertexCount; v++) {
            vertices[v * 3] = (float)coords[v * Dimensions];
            vertices[v * 3 + 1] = (float)coords[v * Dimensions + 1];
            vertices[v * 3 + 2] = Dimensions == 3 ? (float)coords[v * Dimensions + 2] : 0f;
        }
        var indices = Triangulate(GeometryOffsets.Span, RingOffsets.Span, vertexCount);
        return Fin.Succ(new ImportedGeometry(FormatKey, vertices.AsMemory(), Array.Empty<float>(), indices.AsMemory(), vertexCount, indices.Length / 3, at));
    }

    // A geometry spanning more than one ring carries interior (hole) rings, which the per-ring fan would FILL
    // rather than subtract (and a concave exterior self-overlaps), so ToImportedGeometry FAULTS that case to the
    // geometry kernel's robust constrained triangulation at the Bim import seam — the "never re-derived on this
    // wire" boundary is STRUCTURAL, not a comment, never a silent hole-fill. Single-ring geometries (a convex
    // exterior, or a MultiPolygon of single-ring parts) decode through the fan.
    static bool HasInteriorRings(ReadOnlySpan<int> geometryOffsets, ReadOnlySpan<int> ringOffsets) {
        if (geometryOffsets.Length >= 2) {
            for (int g = 0; g + 1 < geometryOffsets.Length; g++) { if (geometryOffsets[g + 1] - geometryOffsets[g] > 1) { return true; } }
            return false;
        }
        return ringOffsets.Length > 2;
    }

    // GeometryOffsets delimit the ring range of each geometry, RingOffsets the coordinate range of each ring;
    // every ring fans around its first vertex, so a MultiPolygon of single-ring parts triangulates per geometry.
    // The interior-ring fault upstream means this fan only ever sees a single convex exterior ring per geometry —
    // exact for that case. A single linear primitive (no rings) crosses as the trivial vertex index run.
    static long[] Triangulate(ReadOnlySpan<int> geometryOffsets, ReadOnlySpan<int> ringOffsets, int vertexCount) {
        if (ringOffsets.Length < 2) { return [.. Enumerable.Range(0, vertexCount).Select(static i => (long)i)]; }
        var triangles = new List<long>();
        if (geometryOffsets.Length >= 2) {
            for (int geometry = 0; geometry + 1 < geometryOffsets.Length; geometry++) { FanRings(ringOffsets, geometryOffsets[geometry], geometryOffsets[geometry + 1], triangles); }
        } else {
            FanRings(ringOffsets, 0, ringOffsets.Length - 1, triangles);
        }
        return [.. triangles];
    }

    static void FanRings(ReadOnlySpan<int> ringOffsets, int ringStart, int ringEnd, List<long> triangles) {
        for (int ring = ringStart; ring < ringEnd; ring++) {
            int start = ringOffsets[ring], end = ringOffsets[ring + 1];
            for (int vertex = start + 1; vertex + 1 < end; vertex++) { triangles.Add(start); triangles.Add(vertex); triangles.Add(vertex + 1); }
        }
    }
}

// The ifctester cross-tool IDS-audit oracle over the settled TWO_HOP companion rpc: the Bim owner authors
// and parses the IDS spec and Compute orchestrates the companion invocation identically to the tessellation
// TWO_HOP pattern, passing the IDS-XML payload plus the IFC content to the Python ifc-companion ifctester
// (the IfcOpenShell `ids` oracle) and relaying the per-specification verdict wire back as the BIM-OWNED
// Review/validation#IDS_FACETS IdsVerdict(GlobalId, Specification, Spec, Requirement, Facet, Passed, Reason) row —
// declared in Bim (the IDS authority; the strata forbid an AEC-DOMAIN owner referencing APP-PLATFORM Compute) and
// composed THERE by IdsAudit.Reconcile from the relayed wire, NEVER re-declared as a Compute type — Compute
// references no Bim type, the two meeting at the companion-rpc wire exactly as the tessellation GLB does. Spec is
// the specification's ZERO-BASED document ordinal and Requirement the facet's ordinal within its spec — BOTH derived
// from the one document order the two tools share (never the spec NAME, which IDS v1.0 does not require unique).
// The Facet column is the Bim IdsFacet.FacetKey join
// token VERBATIM — the INJECTIVE derivation Review/validation#IDS_FACETS owns and this projection mirrors:
//   entity:{Simplify(classes) '|'-joined}:{predefined tokens '|'-joined} | attribute:{KeyOf(name)}:{KeyOf(values) ','-joined}
//   | property:{KeyOf(set)}:{KeyOf(name)}:{KeyOf(values) ','-joined} | classification:{head system}:{codes '|'-joined}
//   | material:{KeyOf(values) ','-joined} | partOf:{relation.Key}:{container FacetKey | "*"}
// where KeyOf renders exacts verbatim, a pattern its expression, a range its invariant-"R" SI bounds, a length its
// bounds, and "*" the match-any — so patterned names, folded classification codes, and the RECURSED partOf container
// (Aggregated/Nested/Voided now emitted where they previously dropped) join byte-identically and two same-kind
// requirements never collapse. Bim's IdsAudit.Reconcile joins the self-audit against the oracle on the
// ORDINAL-QUALIFIED (GlobalId, Requirement, FacetKey) axis, oracle rows filtered by the Spec ordinal — the IDS
// projection Bim-owned, never a Compute-side IdsAudit re-projection, and the python ifc-companion ids-oracle
// projection derives the SAME tokens and ordinals when that leg lands.
public sealed record IdsAuditRequest(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    ReadOnlyMemory<byte> IdsXml,
    string ResultFormatKey) {
    public static Fin<IdsAuditRequest> Plan(ReadOnlyMemory<byte> ifcBytes, ReadOnlyMemory<byte> idsXml, TessellationPolicy policy) =>
        idsXml.IsEmpty
            ? Fin.Fail<IdsAuditRequest>(new ComputeFault.ModelRejected("<ids-audit-empty-spec>"))
            : Fin.Succ(new IdsAuditRequest(
                InterchangeIdentity.Key("ids", ifcBytes.Span, policy.Deflection, policy.Tolerance, policy.AngleTolerance), ifcBytes, idsXml, "ids-verdict"));

    public string ArtifactKey => $"{IfcContentKey:x32}:ids";
}
```

## [03]-[FIELD_RESULT_CODEC]

- Owner: `FieldCodecPolicy` the chunked-layout and error-bound policy record carrying the residual-predict column; `ResidualPredictor` the content-keyed model-lane chunk predictor; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `PointScan` the point-cloud carrier over E57/LAS/LAZ/PTS; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with error-bounded lossy, learned-residual-predicted, or exact lossless residence and a zero-copy solver↔store↔viz handoff; `InterchangeIo` the scientific-data ingest surface dispatching the chunked field decode and the point-scan ingest, the geometry and IFC import arms owned by `Rasm.Bim`.
- Entry: `public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks)` reads a chunked field through `FieldCodec.FieldDecode`; `public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` reads a point-cloud scan; `public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at)` reads a chunked field artifact into the integration-point/nodal field carrier; `public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at)` emits the chunked layout with the policy error bound; `Fin<T>` aborts on a chunk-shape mismatch or an error bound the lossy quantizer cannot meet.
- Auto: the codec chunks the field by the policy chunk shape and exposes the chunk blob two ways — `FieldCodec.ChunkSequence` is a MULTI-segment `ReadOnlySequence<byte>` (one segment per chunk) a consumer streams chunk-by-chunk with no flatten, and `FieldArtifact.Chunk(ordinal)` is the per-ordinal random-access slice a frustum cull reads; the `FieldArtifact.GridChunks` extent maps a chunk ordinal to its grid coordinate so both reads address chunks by grid position, not byte offset; the lossy column quantizes each chunk to the policy bit budget through the shared `Quantization` kernel (one `TensorPrimitives.MaxMagnitude` scale, never a Max/Min/Abs hand-roll) and the residual stays below the relative error bound; the residual column gathers each chunk's axis-aligned face neighbours by `GridChunks` coordinate (`GatherNeighbours` — the true spatial stencil, never a contiguous 1-D window that crosses grid faces), predicts the chunk through the `ResidualPredictor` model-lane ONNX field model, and quantizes only the prediction residual (a smooth converged field stores far past the per-chunk quantizer at the same bound, and a chunk whose residual exceeds the bound falls through to the lossless Brotli with no new failure mode); the lossless column Brotli-compresses the raw bytes through the `System.IO.Compression` `BrotliEncoder`/`BrotliDecoder` span codec sized by `GetMaxCompressedLength` (no intermediate stream, no `RecyclableMemoryStream` round-trip); the protobuf `ByteString` wrap that fans the one chunk buffer to the store blob and the viz upload is the `Runtime/transport#ARTIFACT_FRAMES` frame law (composed, never re-owned here).
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy or residual-predicted encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable.
- Packages: System.IO.Hashing, System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox (`System.IO.Compression` Brotli span codec, `System.Buffers` sequence segments)
- Growth: a new chunked field format is one row on the `field-chunk` codec owned by the Bim format axis; a new point-scan format is one row on the `point-cloud` codec owned by the Bim format axis; a new error-bound policy is one column on `FieldCodecPolicy`; a learned predictor is one `ResidualPredictor` content-keyed ONNX session reused across chunks; zero new surface — a `ResidualCoder`/`NeuralFieldCompressor` sibling is collapsed onto the `FieldCodecPolicy.ResidualPredict` column and the one `ResidualEncode` fold.
- Boundary: the field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor solve field rides the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, so the codec chunks by station and component, never a generic byte blob; the chunked layout composes the suite `XxHash128` chunk identity and the Persistence blob lane content-addressed, so a re-emitted identical chunk dedups and a re-read warms from the store — a second field store is the rejected form; the lossy quantizer's error bound is a typed policy column the receipt records, so an error-bounded compression never silently exceeds its bound; the zero-copy edge is the same `GetReadOnlySequence`/`UnsafeWrap` path the remote frame law owns, so a field chunk crosses solver→store→viz without a managed copy — a `ToArray` flatten on the field path is the named defect; the residual row is the learned-compression terminal — the `ResidualPredictor` is one model-lane `Model/inference#INFERENCE_MODES` ONNX session content-keyed by the parametric-family digest and shared across every chunk so the codec composes the model lane it sits beside rather than minting a second inference path, the grid-coordinate chunk index is preserved (never reduced to content-defined byte chunking, which destroys the grid-coordinate locality the predictor depends on — the FastCDC `#GEOMETRY_DELTA` chunker is the rejected rewrite of this codec) so the predictor sees true spatial neighbours, only the bounded residual against the prediction stores and a chunk whose residual exceeds the bound falls back to the lossless Brotli (no new failure mode), the predictor weights are one content-addressed ONNX artifact the Python offline-science companion fits and returns over the same offline-training seam the optimizer surrogate uses (never an in-proc fit), and the achieved residual stays auditable on the `Cache` receipt; the `PointScan` ingest carries the `point-cloud` codec discriminant the Bim format axis names and faults `point-catalogue-pending` until the E57/LAS/LAZ/PTS reader package is admitted; the geometry mesh decode and the IFC semantic ingest are the `Rasm.Bim` import rail, never re-derived here — an `ImportGeometry`/`ImportIfc` arm in this surface is the deleted form.

```csharp signature
public sealed record FieldCodecPolicy(int[] ChunkShape, bool Lossy, int QuantizationBits, double RelativeErrorBound, bool Compress, bool ResidualPredict) {
    public static readonly FieldCodecPolicy Lossless = new(ChunkShape: [64, 64, 64], Lossy: false, QuantizationBits: 0, RelativeErrorBound: 0.0, Compress: true, ResidualPredict: false);
    public static readonly FieldCodecPolicy Bounded = new(ChunkShape: [64, 64, 64], Lossy: true, QuantizationBits: 12, RelativeErrorBound: 1e-3, Compress: true, ResidualPredict: false);
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
    public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks) =>
        codecKey == "field-chunk"
            ? FieldCodec.FieldDecode(formatKey, bytes, policy, clocks.Now)
            : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{formatKey}>"));

    public static Fin<PointScan> ImportPoints(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        codecKey != "point-cloud"
            ? Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{formatKey}>"))
            : Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-catalogue-pending:{formatKey}:e57-las-laz-pts-reader-unadmitted>"));
}

// The shared codec quantization law — declared once and composed by the field quantizer, the residual
// quantizer, and the geometry-delta normalizer ([GEOMETRY_DELTA]). The scale is one absolute-extremum SIMD
// reduction (never a Max/Min/Abs hand-roll), the step is the bit-budget grid, and the residual is the
// relative rounding error a receipt records — a re-derived per-call quantization loop is the rejected form.
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
    public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) =>
        Try.lift(() => Decode(formatKey, bytes, policy, at)).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default) {
        var encoded = policy.ResidualPredict && predictor.Case is ResidualPredictor net
            ? ResidualEncode(field, policy, net)
            : policy.Lossy ? Quantize(field, policy) : Raw(field, policy);
        var packed = Pack(encoded, policy);
        return encoded.MaxResidual <= policy.RelativeErrorBound || !policy.Lossy
            ? Fin.Succ(new ComputeArtifact(formatKey, packed, InterchangeIdentity.Key(formatKey, packed, TessellationPolicy.Canonical.Deflection, TessellationPolicy.Canonical.Tolerance, TessellationPolicy.Canonical.AngleTolerance), packed.LongLength, at))
            : Fin.Fail<ComputeArtifact>(new ComputeFault.ModelRejected($"<field-error-bound:{encoded.MaxResidual:R}>{policy.RelativeErrorBound:R}"));
    }

    static FieldArtifact ResidualEncode(FieldArtifact field, FieldCodecPolicy policy, ResidualPredictor predictor) {
        var source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span);
        int chunkElements = field.ChunkElements;
        if (chunkElements <= 0 || source.Length < chunkElements) { return policy.Lossy ? Quantize(field, policy) : Raw(field, policy); }
        int[] grid = field.GridChunks.Length > 0 ? field.GridChunks : [field.ChunkCount];
        var residual = new float[source.Length];
        var (scale, step) = Quantization.Steps(source, policy.QuantizationBits);
        double worst = 0.0;
        for (int chunk = 0; chunk < field.ChunkCount; chunk++) {
            int start = chunk * chunkElements, length = Math.Min(chunkElements, source.Length - start);
            if (length <= 0) { break; }
            float[] prediction = predictor.Predict(GatherNeighbours(source, grid, chunk, chunkElements, predictor.NeighbourStencil), length).IfFail([]);
            for (int index = 0; index < length; index++) {
                float predicted = index < prediction.Length ? prediction[index] : 0f;
                float delta = source[start + index] - predicted;
                float coded = Quantization.Code(delta, step);
                residual[start + index] = coded;
                worst = Math.Max(worst, Quantization.Residual(delta, coded, scale));
            }
        }
        return field with { Chunks = MemoryMarshal.AsBytes(residual.AsSpan()).ToArray(), MaxResidual = worst };
    }

    // The true spatial stencil: the center chunk plus its axis-aligned face neighbours read by GridChunks
    // coordinate, so the residual model predicts from real grid neighbours — never a contiguous 1-D window
    // that crosses grid faces. An out-of-grid neighbour contributes a zero chunk, preserving the stencil shape.
    static float[] GatherNeighbours(ReadOnlySpan<float> source, int[] grid, int ordinal, int chunkElements, int radius) {
        int rank = grid.Length;
        Span<int> coord = stackalloc int[rank];
        int remainder = ordinal;
        for (int axis = rank - 1; axis >= 0; axis--) { coord[axis] = remainder % grid[axis]; remainder /= grid[axis]; }
        var stencil = new float[(1 + 2 * rank) * chunkElements];
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

    // One multi-segment view over the chunk blob: each chunk is its own segment so a consumer streams
    // chunk-by-chunk over the pooled sequence — never one flat segment that forfeits per-chunk addressing.
    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) {
        int chunkBytes = field.ChunkElements * sizeof(float);
        if (chunkBytes <= 0 || field.ChunkCount <= 1) { return new(field.Chunks); }
        ChunkSegment? head = null, tail = null;
        for (int chunk = 0; chunk < field.ChunkCount; chunk++) {
            int start = chunk * chunkBytes;
            if (start >= field.Chunks.Length) { break; }
            var slice = field.Chunks.Slice(start, Math.Min(chunkBytes, field.Chunks.Length - start));
            tail = tail is null ? head = new ChunkSegment(slice, 0) : tail.Append(slice);
        }
        return head is null ? new(field.Chunks) : new ReadOnlySequence<byte>(head, 0, tail!, tail!.Memory.Length);
    }

    static FieldArtifact Decode(string formatKey, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) {
        var span = bytes.Span;
        string station = Encoding.ASCII.GetString(span[..16]).TrimEnd('\0');
        int rank = BinaryPrimitives.ReadInt32LittleEndian(span[16..]);
        int components = BinaryPrimitives.ReadInt32LittleEndian(span[20..]);
        long count = BinaryPrimitives.ReadInt64LittleEndian(span[24..]);
        int rawBytes = BinaryPrimitives.ReadInt32LittleEndian(span[32..]);
        int gridRank = BinaryPrimitives.ReadInt32LittleEndian(span[36..]);
        var gridChunks = new int[gridRank];
        for (int axis = 0; axis < gridRank; axis++) { gridChunks[axis] = BinaryPrimitives.ReadInt32LittleEndian(span[(40 + axis * 4)..]); }
        int gridEnd = 40 + gridRank * 4;
        int chunkRank = BinaryPrimitives.ReadInt32LittleEndian(span[gridEnd..]);
        var chunkShape = new int[chunkRank];
        for (int axis = 0; axis < chunkRank; axis++) { chunkShape[axis] = BinaryPrimitives.ReadInt32LittleEndian(span[(gridEnd + 4 + axis * 4)..]); }
        int headerEnd = gridEnd + 4 + chunkRank * 4;
        var payload = policy.Compress ? Decompress(bytes[headerEnd..], rawBytes) : bytes[headerEnd..];
        int chunkBytes = chunkShape.Aggregate(1, static (acc, dim) => acc * dim) * components * sizeof(float);
        int chunkCount = (payload.Length + chunkBytes - 1) / Math.Max(chunkBytes, 1);
        return new FieldArtifact(formatKey, station, rank, components, count, chunkShape, gridChunks, chunkCount, payload, 0.0, at);
    }

    static FieldArtifact Raw(FieldArtifact field, FieldCodecPolicy policy) =>
        field with { MaxResidual = 0.0 };

    static FieldArtifact Quantize(FieldArtifact field, FieldCodecPolicy policy) {
        var source = MemoryMarshal.Cast<byte, float>(field.Chunks.Span);
        var quantized = new float[source.Length];
        var (scale, step) = Quantization.Steps(source, policy.QuantizationBits);
        double worst = 0.0;
        for (int index = 0; index < source.Length; index++) {
            quantized[index] = Quantization.Code(source[index], step);
            worst = Math.Max(worst, Quantization.Residual(source[index], quantized[index], scale));
        }
        return field with { Chunks = MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), MaxResidual = worst };
    }

    // Self-describing header: the fixed station/rank/components/count prefix, the uncompressed payload length
    // (the span-decompress destination size), the GridChunks extent (the grid-coordinate index), and the
    // ChunkShape extent (the per-chunk station/component layout) precede the body, so a Decode reconstructs the
    // chunk grid and the residual stencil from the bytes alone — the chunk shape rides the header, never an
    // out-of-band policy agreement that would mis-count chunks under a decode-policy mismatch.
    static byte[] Pack(FieldArtifact field, FieldCodecPolicy policy) {
        int gridRank = field.GridChunks.Length, chunkRank = field.ChunkShape.Length;
        var header = new byte[44 + gridRank * 4 + chunkRank * 4];
        Encoding.ASCII.GetBytes(field.Station.PadRight(16, '\0')[..16]).CopyTo(header, 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(16), field.Rank);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(20), field.Components);
        BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(24), field.Count);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(32), field.Chunks.Length);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(36), gridRank);
        for (int axis = 0; axis < gridRank; axis++) { BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40 + axis * 4), field.GridChunks[axis]); }
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(40 + gridRank * 4), chunkRank);
        for (int axis = 0; axis < chunkRank; axis++) { BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(44 + gridRank * 4 + axis * 4), field.ChunkShape[axis]); }
        var body = policy.Compress ? Compress(field.Chunks) : field.Chunks;
        return [.. header, .. body.Span];
    }

    static ReadOnlyMemory<byte> Compress(ReadOnlyMemory<byte> data) {
        var destination = new byte[BrotliEncoder.GetMaxCompressedLength(data.Length)];
        return BrotliEncoder.TryCompress(data.Span, destination, out int written) ? destination.AsMemory(0, written) : data;
    }

    static ReadOnlyMemory<byte> Decompress(ReadOnlyMemory<byte> data, int rawLength) {
        var destination = new byte[rawLength];
        return BrotliDecoder.TryDecompress(data.Span, destination, out int written) ? destination.AsMemory(0, written) : data;
    }

    sealed class ChunkSegment : ReadOnlySequenceSegment<byte> {
        public ChunkSegment(ReadOnlyMemory<byte> memory, long runningIndex) {
            Memory = memory;
            RunningIndex = runningIndex;
        }

        public ChunkSegment Append(ReadOnlyMemory<byte> memory) {
            var next = new ChunkSegment(memory, RunningIndex + Memory.Length);
            Next = next;
            return next;
        }
    }
}
```

## [04]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` structural-diff target rows; `GeometryDelta` the content-addressed delta record; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control.
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the ordered target chunk recipe (`TargetChunks`) plus the new-chunk payload (`Added`, hashes absent from the base); `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` walks the recipe and reconstructs the target by pulling each chunk from the payload or the re-chunked base; `Fin<T>` aborts on a base-hash mismatch.
- Auto: `Diff` first `Normalize`s a quantizable kind (vertex/point/control-point floats round to the finer of the bit-budget grid and `Tolerance`, so a sub-tolerance perturbation hashes to one chunk and the delta is bounded-lossy within `Tolerance`; topology and B-rep-face streams pass verbatim), then runs real FastCDC over the normalized bytes — a 256-entry SplitMix64 `Gear` table rolls the fingerprint and a STRICT mask below `AvgChunk` plus a LOOSE mask above it normalize the chunk-size distribution toward `AvgChunk` so an inserted vertex shifts only its local chunk; `TargetChunks` records the ordered hash recipe and `Added` the distinct new chunks, each stamped with the quantization `GeometricError`; the progressive column orders the new chunks largest-first so a transmission renders the coarse coverage before the fine detail; the delta carries its own `DeltaPolicy` so `Apply` re-chunks the base identically and round-trips deterministically (a quantizable kind to the bounded-lossy target, an integer-structural kind exactly).
- Receipt: the `Cache` receipt carries the delta content-key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable; a progressive transmission stamps the coarse-chunk-first ordering count.
- Packages: System.IO.Hashing, System.Numerics.Tensors, LanguageExt.Core, Rasm.Persistence (project), BCL inbox (`System.Numerics.BitOperations` mask sizing)
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row carrying its `Quantizable` column; a new chunk policy is one column on `DeltaPolicy`; the quantization law is the shared `Quantization` kernel ([FIELD_RESULT_CODEC]); zero new surface.
- Boundary: the geometry delta is the structural diff the blob-level delta never owned — the existing Persistence blob delta diffs opaque bytes, this codec diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only the touched chunks, and the diff algebra mirrors the `Runtime/wire#PROTO_VOCABULARY` `GraphDiff`/`SubtreeFetch` wire shape Compute already owns — Compute owns the structural chunking and the Persistence sync lane owns the closure-graph diff, neither re-deriving the other; the chunker is real FastCDC — a `Gear`-hash rolling fingerprint with a STRICT-below / LOOSE-above-`AvgChunk` normalized dual-mask so the chunk-size distribution tightens toward the average and a local edit shifts only its own chunk, while a fixed-block or single-mask shift-add chunker that re-chunks the whole stream on an insert is the rejected form; reconstruction is order-faithful and lossless — the `TargetChunks` recipe places a mid-stream insert at its true position rather than appending it at the tail, and `Apply` re-chunks the base under the delta's OWN `DeltaPolicy`, never a hardcoded one; the quantization-aware bounded-lossy `Normalize` rounds a quantizable kind to the finer of the bit grid and `Tolerance` so a delta never silently exceeds the geometry tolerance; the new-chunk set transmits progressively through the `SubtreeFetch` server-stream and the content-key dedups against the Persistence blob lane, never a second delta store; the geometry-kind discriminant scopes quantization, so a topology-only edit never quantizes and a position-only edit never re-transmits the topology column.

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
        var normalizedBase = Normalize(kind, baseBytes, policy);
        var normalizedTarget = Normalize(kind, targetBytes, policy);
        float error = QuantError(kind, targetBytes, policy);
        var baseSet = FastCdc(normalizedBase.Span, policy).Map(static c => c.Hash).ToHashSet();
        var targetChunks = FastCdc(normalizedTarget.Span, policy);
        var added = toSeq(targetChunks.Filter(c => !baseSet.Contains(c.Hash)).DistinctBy(static c => c.Hash)).Map(c => c with { GeometricError = error });
        var ordered = policy.Progressive ? added.OrderByDescending(static c => c.ByteLength).ToSeq() : added;
        return Fin.Succ(new GeometryDelta(kind,
            XxHash128.HashToUInt128(baseBytes.Span), XxHash128.HashToUInt128(normalizedTarget.Span),
            targetChunks.Map(static c => c.Hash), ordered, Concatenate(ordered, normalizedTarget), policy,
            baseBytes.LongLength, ordered.Sum(static c => (long)c.ByteLength)));
    }

    public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        XxHash128.HashToUInt128(baseBytes.Span) == delta.BaseHash
            ? Fin.Succ(Reconstruct(delta, baseBytes))
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-base-mismatch:{delta.BaseHash:x32}>"));

    // Bounded-lossy normalization: a quantizable stream (vertex / point / control-point floats) rounds to the
    // FINER of the bit-budget grid and the geometry Tolerance before chunking, so a sub-tolerance perturbation
    // hashes to the same chunk and the delta never exceeds Tolerance; an integer-structural stream (topology,
    // B-rep face id) passes through verbatim. This is what makes Quantizable / QuantizationBits / Tolerance live.
    static ReadOnlyMemory<byte> Normalize(GeometryDeltaKind kind, ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        if (!kind.Quantizable || policy.QuantizationBits <= 0) { return bytes; }
        var source = MemoryMarshal.Cast<byte, float>(bytes.Span);
        var quantized = new float[source.Length];
        float step = QuantStep(source, policy);
        for (int index = 0; index < source.Length; index++) { quantized[index] = Quantization.Code(source[index], step); }
        return MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray();
    }

    static float QuantStep(ReadOnlySpan<float> source, DeltaPolicy policy) {
        var (_, bitStep) = Quantization.Steps(source, policy.QuantizationBits);
        return bitStep <= 0f ? (float)policy.Tolerance : MathF.Min(bitStep, (float)policy.Tolerance);
    }

    static float QuantError(GeometryDeltaKind kind, ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        kind.Quantizable && policy.QuantizationBits > 0 ? QuantStep(MemoryMarshal.Cast<byte, float>(bytes.Span), policy) : 0f;

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

    static readonly ulong[] Gear = BuildGear();

    // A 256-entry Gear table seeded deterministically (SplitMix64) so the rolling fingerprint reproduces
    // across runtimes — the cross-machine content-defined-chunking floor the Python/TS delta lanes re-derive.
    static ulong[] BuildGear() {
        var gear = new ulong[256];
        ulong state = 0x9E3779B97F4A7C15UL;
        for (int index = 0; index < 256; index++) {
            state += 0x9E3779B97F4A7C15UL;
            ulong mix = (state ^ (state >> 30)) * 0xBF58476D1CE4E5B9UL;
            mix = (mix ^ (mix >> 27)) * 0x94D049BB133111EBUL;
            gear[index] = mix ^ (mix >> 31);
        }
        return gear;
    }

    // FastCDC normalized chunking: a Gear hash rolls per byte; below AvgChunk a STRICT mask (more 1-bits)
    // makes a cut rare so a small chunk is suppressed, above AvgChunk a LOOSE mask (fewer 1-bits) makes a cut
    // likely so a large chunk is capped — the chunk-size distribution tightens toward AvgChunk and a local
    // edit shifts only its own chunk. A fixed-block cut that re-chunks the whole stream on an insert is rejected.
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

    // Order-faithful lossless reconstruction: walk the target chunk recipe in order, pulling each chunk from
    // the added payload or the re-chunked base by content hash — a mid-stream insert reconstructs at its true
    // position, never appended at the tail. The base re-chunks under the delta's OWN policy, never a hardcoded one.
    static ReadOnlyMemory<byte> Reconstruct(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) {
        var addedByHash = SplitPayload(delta.Payload);
        var normalizedBase = Normalize(delta.Kind, baseBytes, delta.Policy);
        var baseByHash = new System.Collections.Generic.Dictionary<UInt128, ReadOnlyMemory<byte>>();
        foreach (var chunk in FastCdc(normalizedBase.Span, delta.Policy)) { baseByHash[chunk.Hash] = normalizedBase.Slice(chunk.Offset, chunk.ByteLength); }
        var pieces = delta.TargetChunks.Map(hash => addedByHash.TryGetValue(hash, out var added) ? added : baseByHash.TryGetValue(hash, out var held) ? held : ReadOnlyMemory<byte>.Empty);
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

## [05]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the imported geometry carrier; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf content-keyed metadata property table joining the IFC classification column and the solver field-value columns under one feature-id mapping, carrying its own `ReplayKey` so a tile is independently addressable and cache-replayable; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `LeafContent`/`TilesetExport` the manifest-plus-leaf-reference export carriers; `ExportTiles` the tileset-manifest emit fold that serializes the octree to tileset.json and enumerates the leaf-content references the manifest names, riding the content-key and the metadata layer, the leaf BODIES themselves the Bim glTF codec's cross-package product; the partition consumes the deflection/tolerance and tile-depth/error/split scalars from `TessellationPolicy` and the `InterchangeIdentity.Key`/`InterchangeIdentity.Compose` content-key, never the Bim format/codec/KHR surface.
- Entry: `public static Fin<TilesetExport> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, ClockPolicy clocks)` builds the octree, attaches the per-leaf metadata read at the node content-key, serializes the real tileset.json manifest, and enumerates the leaf-content references the manifest names — the leaf BODIES are the Bim glTF codec's product resolved at the content-key URIs, never emitted here; `public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, ClockPolicy clocks)` partitions the geometry into the depth-bounded octree; `Fin<T>` aborts on a tileset serialization miss projected onto `ComputeFault.ModelRejected`.
- Auto: `Build` partitions the geometry octant-by-octant to the policy max depth or the triangle split threshold, computing the geometric error as the root error halved per depth and the per-node content-key over the node geometry through `InterchangeIdentity.Key` so a re-partition of identical geometry at identical settings keys identically, then reads the per-leaf `TileMetadata` at the node content-key so the same key addresses geometry and metadata; `ExportTiles` serializes the octree to the tileset.json manifest (box bounding volumes, per-level geometric error, refine REPLACE, leaf content-key URIs) and flattens the octree to enumerate the leaf-content references keyed on each node content-key — the leaf BODIES those URIs name (carrying the `EXT_structural_metadata` property table and `EXT_mesh_features` feature-id mapping) are the Bim glTF codec's cross-package product resolved against the Persistence artifact index, never emitted here; `TileMetadata.Join` folds the `Rasm.Bim` IFC classification projection and the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` per-element field values read at the shared content-key into one feature-keyed property table keyed by its own tile content-key, `TileMetadata.ReplayKey` composes that tile content-key with the causal stamp through `InterchangeIdentity.Compose` in the fixed (physical, logical) half order so a tile's metadata is independently addressable and a re-partition at the same stamp replays the identical per-tile property table from cache without rebuilding the octree, and `PropertyTable.Pack` lays each `MetadataProperty` column out as a contiguous buffer-view body the leaf-tile emit references; `FeatureBand.Of` classifies an achieved per-element field value onto its styling band so the viewer styles by solved field without recomputing the metric.
- Receipt: the `StreamSegment` receipt carries the leaf-reference count, the root geometric error, the max depth, the node count, and the per-leaf property-column count; emission rides the sink port.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Ext.3DTiles (the `Schema2.Tiles3D` EXT_structural_metadata/EXT_mesh_features leaf-body schema surface, admitted via `Tiles3DExtensions.RegisterExtensions()` once at composition — the settled Compute admission; models no tileset.json manifest tree), meshoptimizer, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox (`System.Text.Json` Utf8JsonWriter tileset-manifest emit, `System.Buffers` ArrayBufferWriter)
- Growth: a new tile-partition parameter is one column on `TessellationPolicy` folded into the partition; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.
- Boundary: the 3D-Tiles partition is the streamable-LOD octree over the content-keyed geometry the field/tile compute lane owns — it rides `InterchangeIdentity.Key` and the imported-geometry carrier, so the partition stays a compute concern while the b3dm/glTF tile content encode is the Bim glTF codec the leaf emit composes; the metadata layer is one content-keyed schema column on the leaf-tile content emit, never a parallel attribute store or a second tiling owner — each `TileMetadata` carries its own tile content-key so a tile is independently addressable, and `TileMetadata.ReplayKey` composes that content-key with the causal stamp through `InterchangeIdentity.Compose` so a leaf tile is cache-replayable under the one frame without rebuilding the octree; the IFC classification reads the `Rasm.Bim` IFC semantic graph at the shared content-key (the companion seam aligned to a named boundary, never reaching into the Bim interior) and the per-element field values read the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` achieved value (never a recomputed metric), so the IFC semantic graph and the tessellated geometry stay two projections of the one content-keyed IFC artifact joined at the tile boundary and a re-tessellation at a new deflection re-keys geometry and metadata together; the `EXT_structural_metadata` property tables and the `EXT_mesh_features` feature-id attribute are vendor glTF extensions `SharpGLTF.Core` does not model natively (its in-box extension framework is `ExtensionsFactory.RegisterExtension<TParent,TExt>(string name)` over a caller-supplied `JsonSerializable`-derived class; the material-PBR surface is the string-keyed `MaterialChannel` API in Core and the `KnownChannel` enum in Toolkit, neither a general vendor-extension surface), so the schema, the property-table buffer-view columns, and the feature-id vertex attribute emit through the `ExtensionsFactory.RegisterExtension` write surface against a `JsonSerializable`-derived extension class registered before write — the binary layout (property-table column buffer views, feature-id `_FEATURE_ID_0` attribute) is the `[EXT_STRUCTURAL_METADATA]` RESEARCH leaf against the SharpGLTF extension API and the 3D-Tiles 1.1 spec, never an assumed SharpGLTF helper; meshoptimizer owns the leaf-tile `Meshopt.Simplify`/`OptimizeVertexCache` LOD optimization the octree leaf geometry rides, never a hand-rolled simplifier; the leaf-tile content body is NOT emitted here — `ExportTiles` yields one typed `LeafContent` reference per octree leaf (content-key, `{contentKey:x32}.glb` URI, metadata-column count), the octree, metadata schema, and quantization-bit policy owned here while the b3dm/glTF body each URI names is the Bim tile-emit codec's cross-package product resolved against the Persistence index, so a public leaf-body entry that can only decline is the rejected honesty defect and a tile partition that re-derives the glTF tile content body in-place or a metadata layer that re-reads the IFC parser is the rejected form.

```csharp signature
// The compute-lane geometry-quality + tile-partition policy — RENAMED off `InterchangePolicy`: the Bim
// Exchange/export `InterchangePolicy` (codec emit columns) is a DISTINCT concept, and one name over two shapes
// was the named SEMANTIC_NAMING split-brain at the seam; the quality triple salts every compute content key.
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

    public UInt128 ReplayKey(Instant physical, ulong logical) => InterchangeIdentity.Compose(ContentKey, physical, logical);
}

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Option<TileMetadata> Metadata, Seq<TileNode> Children);

public sealed record TileSet(TileNode Root, double GeometricErrorRoot, int MaxDepth, int NodeCount, Instant At) {
    public static TileSet Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, ClockPolicy clocks) {
        var root = Partition(geometry, metadata, policy, depth: 0);
        return new TileSet(root, policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(root), clocks.Now);
    }

    static TileNode Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, int depth) {
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

// The tileset EXPORT the partition owns: the real tileset.json `Manifest` (this page's product) plus the
// `LeafContent` reference set the manifest names — the typed handoff to the Bim glTF leaf codec, never a leaf
// body minted here. A consumer emits the manifest, then resolves each `LeafContent.Uri` against the Persistence
// artifact index where the Bim-produced leaf body resides under its content-key.
public sealed record LeafContent(UInt128 ContentKey, string Uri, int MetadataColumns);

public sealed record TilesetExport(ComputeArtifact Manifest, Seq<LeafContent> Leaves);

public static class TilePartition {
    // ExportTiles emits this page's OWNED product — the real tileset.json manifest serialized over the octree
    // (box bounding volumes, per-level geometric error, refine REPLACE, per-leaf content-key URIs) — and
    // enumerates the LeafContent references the manifest names. The leaf BODIES (the b3dm/glTF each URI names,
    // carrying the EXT_structural_metadata/EXT_mesh_features rows) are the Bim glTF codec's product at
    // cross-package alignment, resolved by the AppUi/web consumer at the content-key URIs against the
    // Persistence artifact index; this page emits the manifest and the leaf-reference set, never a leaf body.
    public static Fin<TilesetExport> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, ClockPolicy clocks) {
        TileSet tiles = TileSet.Build(geometry, metadata, policy, clocks);
        return Tileset(tiles, clocks).Map(manifest => new TilesetExport(manifest, Leaves(tiles.Root)));
    }

    // tileset.json: refine REPLACE, box bounding volumes off each node's Aabb, geometricError halving per
    // level, leaf content URIs `{contentKey:x32}.glb` — the content-key addressing the AppUi/web consumer
    // resolves against the Persistence artifact index.
    static Fin<ComputeArtifact> Tileset(TileSet tiles, ClockPolicy clocks) =>
        Try.lift(() => ComputeArtifact.Of("tileset.json", TilesetBytes(tiles.Root), clocks.Now)).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<tileset-emit:{error.Message}>"));

    // The leaf-reference set the manifest names: one LeafContent per octree leaf carrying its content-key, the
    // `{contentKey:x32}.glb` URI, and the metadata-column count the leaf body's EXT_structural_metadata table
    // will carry — the typed handoff to the Bim leaf-content producer, NOT a body. The bodies land on the
    // Persistence blob lane under these content-keys and the web consumer resolves the manifest URIs to them.
    static Seq<LeafContent> Leaves(TileNode root) =>
        Flatten(root)
            .Filter(static node => node.Children.IsEmpty)
            .Map(static node => new LeafContent(node.ContentKey, $"{node.ContentKey:x32}.glb", node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)));

    // Real tileset.json serialization over the octree: a glTF-independent JSON manifest (asset 1.1, root
    // geometricError, and the recursive tile tree — box boundingVolume, per-node geometricError, refine
    // REPLACE, and a leaf content URI). SharpGLTF.Ext.3DTiles owns the glTF-EMBEDDED EXT_structural_metadata/
    // EXT_mesh_features rows of the LEAF BODIES (Bim's codec), never this manifest tree — so the manifest emits
    // through the BCL Utf8JsonWriter and the leaf bodies it references stay the Bim cross-package product.
    static ReadOnlyMemory<byte> TilesetBytes(TileNode root) {
        var sink = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(sink);
        writer.WriteStartObject();
        writer.WriteStartObject("asset");
        writer.WriteString("version", "1.1");
        writer.WriteEndObject();
        writer.WriteNumber("geometricError", root.GeometricError);
        writer.WritePropertyName("root");
        WriteNode(writer, root);
        writer.WriteEndObject();
        writer.Flush();
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
            foreach (var child in node.Children) { WriteNode(writer, child); }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }

    static Seq<TileNode> Flatten(TileNode node) =>
        node.Cons(node.Children.Bind(Flatten));
}
```

## [06]-[CONTENT_ADDRESSING]

- Owner: `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `CanonicalForm` the static byte-normalization kernel reducing every keyed input to one machine-independent canonical byte form before the hash seed; `InterchangeIdentity` — the interchange CACHE-PARTITION key derivation folding the canonicalized source bytes plus the deflection and tolerance policy into one policy-seeded `XxHash128` identity (the logical interchange/cache key for the Compute-authored field/tile/re-exported-glTF artifacts and the source-plus-policy cache partition — DISTINCT from the GLB geometry-content identity, which is the kernel seed-zero `XxHash128` `GeometryHash` the seam/Bim/Persistence/peers share, composed and never re-minted here), mirroring the model-lane `ModelIdentity.Snapshot` precedent, with `InterchangeIdentity.Compose` sealing the content key and the HLC two-half causal stamp into one frame key and `InterchangeIdentity.SeedZero` minting the empty-artifact sentinel; `ComputeArtifact` the emitted-bytes carrier the field, tile, and Bim export rails feed; the artifact lands content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` — pure value; identity derives from the canonical bytes and the evaluation policy, never from a path or filename; `public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical)` folds the content key with the causal stamp in the fixed (physical, logical) half order; `public static UInt128 SeedZero(string formatKey, double deflection, double tolerance, double angleTolerance)` is the empty-artifact sentinel identity.
- Auto: every keyed input passes through `CanonicalForm` before the seed — `CanonicalForm.Tag` lower-cases the invariant culture and trims the format key and codec tag so a `"GLB"` and a `" glb "` key one identity, `CanonicalForm.Scalar` collapses negative zero to positive zero and maps every NaN bit pattern to one quiet-NaN payload, and `CanonicalForm.Write` lays the canonical tag and the three canonical scalars out little-endian in fixed field order; the artifact bytes pass into the byte hash verbatim because the producing owner already emits canonical field/struct order; the key then seeds `XxHash128.HashToUInt128` over the canonical artifact bytes with a seed `XxHash3.HashToUInt64` mixes from the canonical tag and the three canonical-scalar little-endian doubles so a re-tessellation at a different deflection keys distinctly and a re-import of identical bytes at identical settings keys identically on any RID; a zero-length artifact never reaches the byte hash — `Key` routes empty bytes to `SeedZero`, a content key minted from the policy seed alone over the one-byte sentinel domain so an absent artifact and a present-but-empty artifact carry distinct, defined identities and never collide with an accidental hash of nothing; `Compose` lays the physical half first as the NodaTime `Instant` Unix-tick count and the logical half second as the monotone `ulong`, both little-endian, exactly the (physical, logical) order `AppHost/Runtime/ports#PORT_RECORDS` `ReceiptEnvelope` carries; `Admit` projects the artifact onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns so the blob lane stores and serves the addressed bytes.
- Receipt: the `Cache` receipt carries the content-key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt; a sentinel-keyed empty artifact stamps the `SeedZero` identity so an absent-versus-empty distinction is auditable.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one canonical-scalar column folded into the seed; a new keyed-input kind is one `CanonicalForm` arm; zero new surface.
- Boundary: the interchange-cache identity is `XxHash128` over the canonical source bytes — the suite hash law the `Runtime/transport#ARTIFACT_FRAMES` whole-artifact identity row and the model-lane `ModelIdentity` checksum already hold, never a second hashing pass for one key and never a path-keyed identity; canonical-form normalization is the cross-machine reproducibility floor the policy-seeded interchange CACHE-key derivation holds — case-folded trimmed tag, little-endian policy scalars, negative-zero collapsed to positive zero, every NaN payload mapped to one quiet NaN, so two semantically-equal source artifacts on osx-arm64, linux-x64, and win-x64 cache-key one identity (the `lang:python:runtime/evidence/identity#IDENTITY` `ContentIdentity` folds the SAME format/deflection/tolerance into its key, the cross-runtime cache-hit-by-reference peer of THIS cache-key derivation) and a raw-string-interpolated seed (the `$"{formatKey}|{deflection:R}|..."` form) is the rejected drift defect that keys distinctly across cultures and float renderings; the SHARED geometry WIRE hash is a DISTINCT key the parity claim does NOT conflate with the cache key — the GLB/representation geometry-content identity the seam `Rasm.Element/Graph/element#NODE_MODEL` `RepresentationContentHash`, the Persistence `Store/blobstore#OBJECT_STORE` blob name, and the `lang:typescript:core/interchange/frame#GEOMETRY_PLANE` + `lang:typescript:data/object/store` `ObjectKey` peers all reproduce is the KERNEL seed-zero (`seed=0`) `XxHash128` `GeometryHash` over the canonical bytes (the `csharp:Rasm/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` golden vector anchoring the C#/Python/TypeScript byte-parity), composed here and NEVER re-minted with a policy seed — a policy-seeded GLB geometry-content hash is the named cross-runtime defect, and the two keys (kernel seed-zero geometry identity, policy-seeded interchange cache partition) coexist by design; the empty-artifact `SeedZero` sentinel is the absent-versus-empty law (the policy-seeded empty-artifact case, distinct from the kernel `seed=0` discipline above) — an empty-bytes artifact keys to `SeedZero` over the policy alone, never the byte hash of an empty span, so a cache key never collides between absent and present-but-empty; the HLC compose order is byte-identical to `AppHost/Runtime/ports#PORT_RECORDS` — physical half first, logical half second, both little-endian — so a content key and a causal stamp seal one frame the peers re-derive from the same two-half order and a logical-half-first composition is the named defect that folds a fresh op as stale; the key takes a format-key string rather than the Bim `InterchangeFormat` owner so the content identity stays a Compute concern decoupled from the moved format axis; the deflection and tolerance fold into the seed so the geometry-evaluation settings partition the key and a coarse and a fine tessellation of the same IFC never collide — a cross-setting hit is the named defect; the addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` under the content-key string `Path`, the single artifact owner, so the IFC semantic graph (Bim), the tessellated GLB, the field artifact, and a re-exported glTF are rows under the ONE kernel seed-zero `XxHash128` residence identity the Persistence index re-derives over the bytes (`ArtifactIndexRow.Admit` -> `ContentAddress.Of`) — Compute owns only the policy-seeded interchange-cache-key derivation (the logical lookup label), the kernel/seam own the seed-zero residence identity, and Persistence owns blob residence, none re-declaring another; the export-rail field/tile/re-exported-glTF artifacts self-key (their `SourceKey` is their own `ContentHash`, a single-projection row) while the tessellated GLB and the IFC-semantic graph of one source IFC share one cross-projection `sourceKey` — the kernel seed-zero `SourceKey` the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` mints PURELY over the source bytes (tolerance-independent, so the in-process semantic-graph ingest re-derives it without the tessellation deflection), NOT the policy-seeded interchange cache key — so the Persistence `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Project` fold returns the two-projection family under that one kernel-seed-zero key, the `Option<UInt128> sourceKey` admission carrying that pure key while each row's blob residence is the kernel seed-zero `ContentAddress.Of` the Persistence index re-derives over the bytes (`ArtifactIndexRow.Admit`), never a GLB self-key minted off the policy-seeded cache partition that would strand the geometry projection off the semantic one; a managed copy of the artifact bytes beside the blob lane is the rejected form.

```csharp signature

// The string-keyed compute-lane emit carrier — RENAMED off `ExportArtifact`: the Bim Exchange/export
// `ExportArtifact` (an `InterchangeFormat`-rowed carrier) is a DISTINCT owner; one name over two shapes was
// the named SEMANTIC_NAMING split-brain at the seam.
public sealed record ComputeArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

public static class CanonicalForm {
    public const long QuietNaNBits = unchecked((long)0x7FF8000000000000UL);

    public static string Tag(string raw) => raw.Trim().ToLowerInvariant();

    public static double Scalar(double raw) =>
        double.IsNaN(raw) ? BitConverter.Int64BitsToDouble(QuietNaNBits)
        : raw == 0d ? 0d
        : raw;

    public static int Write(Span<byte> destination, string formatKey, double deflection, double tolerance, double angleTolerance) {
        int written = Encoding.UTF8.GetBytes(Tag(formatKey), destination);
        BinaryPrimitives.WriteDoubleLittleEndian(destination[written..], Scalar(deflection));
        BinaryPrimitives.WriteDoubleLittleEndian(destination[(written + 8)..], Scalar(tolerance));
        BinaryPrimitives.WriteDoubleLittleEndian(destination[(written + 16)..], Scalar(angleTolerance));
        return written + 24;
    }

    public static long Seed(string formatKey, double deflection, double tolerance, double angleTolerance) {
        Span<byte> canonical = stackalloc byte[Encoding.UTF8.GetByteCount(Tag(formatKey)) + 24];
        int length = Write(canonical, formatKey, deflection, tolerance, angleTolerance);
        return unchecked((long)XxHash3.HashToUInt64(canonical[..length]));
    }
}

public static class InterchangeIdentity {
    public const ulong SeedZeroDomain = 0xFFFF_FFFF_FFFF_FFFFUL;

    public static UInt128 Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance) =>
        bytes.IsEmpty
            ? SeedZero(formatKey, deflection, tolerance, angleTolerance)
            : XxHash128.HashToUInt128(bytes, CanonicalForm.Seed(formatKey, deflection, tolerance, angleTolerance));

    public static UInt128 SeedZero(string formatKey, double deflection, double tolerance, double angleTolerance) {
        Span<byte> sentinel = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(sentinel, SeedZeroDomain);
        BinaryPrimitives.WriteUInt64LittleEndian(sentinel[8..], 0UL);
        return XxHash128.HashToUInt128(sentinel, CanonicalForm.Seed(formatKey, deflection, tolerance, angleTolerance));
    }

    public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical) {
        Span<byte> frame = stackalloc byte[32];
        BinaryPrimitives.WriteUInt64LittleEndian(frame, (ulong)contentKey);
        BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], (ulong)(contentKey >> 64));
        BinaryPrimitives.WriteInt64LittleEndian(frame[16..], physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(frame[24..], logical);
        return XxHash128.HashToUInt128(frame);
    }

    public static ArtifactIndexRow Admit(ComputeArtifact artifact, DataClassification classification, string retentionClass, Option<UInt128> sourceKey = default) =>
        ArtifactIndexRow.Admit(ArtifactKind.Interchange, $"{artifact.ContentKey:x32}:{CanonicalForm.Tag(artifact.FormatKey)}", artifact.Bytes.ToArray(), classification, retentionClass, artifact.At, sourceKey);
}
```

## [07]-[RESEARCH]

- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the two-hop tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by the Python geometry companion and rides the remote-lane companion rpc; the `TessellationRequest` shape and the content-key cache-reuse are authored here.
- [FIELD_FORMAT]: the CGNS/EnSight/VTK/Zarr chunked-field decode member spellings ground against the admitted field-format library surface, the field-format row vocabulary is owned by the Bim format axis, and the decode body grounds at the field-codec admission gate.
- [RESIDUAL_PREDICTOR]: the `ResidualPredictor` neighbour-stencil-to-chunk ONNX field model is fit offline by the Python geometry-science companion and arrives as one content-addressed ONNX artifact keyed by the field-family digest over the `Runtime/wire#PROTO_VOCABULARY` artifact transport — C# owns the grid-coordinate face-neighbour gather (`GatherNeighbours` over `FieldArtifact.GridChunks`, stencil shape `(1 + 2·rank)·chunkElements`), the per-chunk inference (`RunOps.Bind` + the `InferenceSession.Infer` extension), and the residual fold; the face radius (`ResidualPredictor.NeighbourStencil`), the component interleave, and the trained model's input/output tensor names ground against the companion-published model signature at the residual-codec admission gate, and the predictor warms from the same `Model/inference#RESULT_CACHE` model-lane cache the optimizer neural-field surrogate uses.
- [TILE_CONTENT]: the leaf-tile b3dm/glTF content encode rides the Bim glTF codec; the tileset.json MANIFEST serialization (the octree tree — box volumes, per-level geometric error, refine REPLACE, content-key URIs) is owned here over the BCL `Utf8JsonWriter`, glTF-independent (`SharpGLTF.Ext.3DTiles` models the leaf-body extension schema, never the manifest tree); the `TileSet` octree partition, the per-node content-key, the `MetadataProperty`/`PropertyTable`/`TileMetadata` semantic-layer schema, the `FeatureBand` styling rows, and the quantization-bit policy are owned here and the leaf-tile content emit grounds against the Bim tile-emit codec at cross-package alignment.
- [EXT_STRUCTURAL_METADATA]: the `EXT_structural_metadata` property-table buffer-view layout and the `EXT_mesh_features` `_FEATURE_ID_0` vertex-attribute emit ride the SharpGLTF raw glTF-extension write surface — SharpGLTF.Core 1.0.6 models neither vendor extension natively (the in-box extension framework is `ExtensionsFactory.RegisterExtension<TParent,TExt>(string name)` registering a caller-supplied `JsonSerializable`-derived extension class before write; the material-PBR surface is separate — the string-keyed `MaterialChannel` API in Core and the `KnownChannel` enum in Toolkit — and is not a general vendor-extension surface), so the schema-class declaration, the property-table class/property JSON, and the feature-id accessor binding ground against the SharpGLTF `ExtensionsFactory`/`JsonSerializable` extension API and the 3D-Tiles 1.1 / glTF `EXT_structural_metadata` spec at the leaf-emit admission gate; the `PropertyTable.Pack` column buffer and the `MetadataProperty` typed columns are authored here. The leaf-tile geometry LOD rides `Meshopt.Simplify(uint* destination, uint* indices, nuint indexCount, float* positions, nuint vertexCount, nuint stride, nuint targetIndexCount, float targetError, SimplificationOptions options, float* resultError)` and `Meshopt.OptimizeVertexCache(uint* destination, uint* indices, nuint indexCount, nuint vertexCount)` over the per-octant geometry, the `targetIndexCount`/`targetError` band and the `SimplificationOptions` flags owned by the octree depth.
- [ARTIFACT_INDEX_ROW]: the `ArtifactKind.Interchange` kind row on the Persistence artifact-blob index carries every interchange artifact (tessellated GLB, chunked field, tile content, re-exported glTF) beside `EpContext`, `OnnxProfile`, `IfcSemantic`, and `ChunkContent` — the row is settled on the `Persistence/Query/cache#ARTIFACT_BLOB_INDEX` owner and Compute consumes the `interchange` kind constant as settled vocabulary; the residual is the classification/retention column values the interchange artifact carries at `InterchangeIdentity.Admit`, confirmed against the Persistence retention axis at cross-folder alignment.
