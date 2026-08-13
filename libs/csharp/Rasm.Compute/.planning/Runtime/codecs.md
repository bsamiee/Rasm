# [COMPUTE_CODECS]

Rasm.Compute owns the compute-and-transport half of artifact interchange: the codecs laying simulation fields and structural geometry deltas down as bytes, the companion hop turning IFC into geometry, the streamable octree partitioning that geometry beneath its semantic layer, and the content-addressed identity every one of them keys on. `Rasm.Bim` owns the IFC/glTF/STEP semantic object model and its import-export surface, reached at the companion seam; this lane is HOST-LOCAL and carries no TS_PROJECTION.

`FieldCodec`/`DeltaCodec`, the `TessellationJob` companion bridge, the `TileSet` octree with its `MetadataProperty`/`PropertyTable`/`TileMetadata`/`FeatureBand` family, and the `CanonicalForm`/`InterchangeIdentity` content-key own the lane, composing the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` precedent, the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, the kernel `EncodeForm.Parametric` stream, the SharpGLTF glTF-extension write surface, the meshoptimizer LOD kernels, and the `Substrate.RemoteGrpc` companion hop.

GLB geometry-content identity composes the kernel seed-zero `XxHash128` `GeometryHash` here, never re-minted with a policy seed.

## [01]-[INDEX]

- [02]-[TWO_HOP_TESSELLATION]: IFC/AP242/native geometry crosses to the companion, never in-proc; ifctester IDS-audit oracle and GeoArrow-buffer consume ride the same companion rpc.
- [03]-[FIELD_RESULT_CODEC]: chunked simulation-field layout; error-bounded lossy/lossless; HDF5 ingest and egress arms; zero-copy.
- [04]-[HDF_ARCHIVE]: one HDF5 container-session capsule owns source-cased opens, declared-selection reads, cursor-guarded chunked writes, and the composition filter seat.
- [05]-[GEOMETRY_DELTA]: FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; progressive.
- [06]-[TILE_PARTITION]: 3D-Tiles octree partition; streamable LOD over the content-keyed geometry.
- [07]-[CONTENT_ADDRESSING]: policy-seeded canonical-form `XxHash128` interchange-cache key (the GLB geometry-content identity is the kernel seed-zero `GeometryHash` composed, distinct); empty-artifact sentinel; HLC two-half compose.
- [08]-[ARROW_BATCH]: `Solver/sweep` `DoeDataset`, `Runtime/receipts` `ChargebackDataset`, and the `GeometryDataset` kernel-encode corpus project into self-describing Arrow batches — bulk-span columns for the row-major pair, arena-borrowed `FixedSizeList` channel columns for geometry, receipt facts as `Schema` metadata; the surrogate-training, billing, and geometry lake egress one `Landing` projection hands the Persistence custodian.

## [02]-[TWO_HOP_TESSELLATION]

- Owner: `TessellationJob` — the rpc job named APART from the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` `TessellationRequest` it composes under, one seam carrying two shapes so neither page re-mints the other's key — the two-hop bridge crossing IFC geometry evaluation to the IfcOpenShell companion (`python:geometry/mesh/daemon` `TessellationDaemon.tessellate`, evaluating in-process and serializing GLB to a file sink) and re-importing the GLB through the Bim glTF path, host-local and riding the existing companion rpc, never a new transport; `IdsAuditRequest` the companion-rpc leg passing IDS-XML to the Python ifctester oracle and projecting the per-specification pass/fail `GlobalId` set into the Bim `IdsAudit` shape (one invocation beside `IfcConvert`); the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` the decoded mesh-pool carrier the re-import lands and the tile partition reads (ONE shape at the seam — the Bim rail produces it, this lane consumes it, no Compute-local twin); `TessellationPolicy` the deflection/tolerance/tile-partition policy folded into the content-key.
- Entry: `public static Fin<TessellationJob> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, TessellationPolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; the round-trip rides the `Runtime/wire#PROTO_VOCABULARY` `Tessellate` rpc, whose `TessellationRequest`/`TessellationReceipt` pair this job transcribes onto while the GLB body returns over the ArtifactSyncService frame seam, and the GLB re-enters the Bim glTF import rail as an `ImportedGeometry`.
- Auto: `Plan` gates the hop on the source format's companion-tessellation flag so a non-IFC format never crosses; the request carries the IFC bytes, deflection, tolerance, and content-key so a re-cross at the same policy is gated. Durable GLB residence is keyed by the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` dual `SourceKey`/`ContentKey` (kernel seed-zero content-hash, never a policy seed) with the Persistence object-store `ContentAddress`, the Bim bridge performing that durable reuse before crossing; this leg's policy-seeded `IfcContentKey` is the companion-rpc cache-partition over source IFC and evaluation policy that gates re-crossing — distinct cache layers, neither re-minting the other's key.
- Receipt: the `RemoteCall` receipt carries the companion transport, the IFC content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.Element (project — the seam `ImportedGeometry` carrier), Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `TessellationJob` folded into the content-key; a geospatial mesh payload is one `GeoArrowRequest` over the same companion operation and GLB result (never a second spatial codec); zero new surface.
- Boundary: two-hop rail is the single IFC-to-geometry path — the Bim IFC object model carries no tessellation kernel, so a managed IFC BRep evaluator is the deleted form; companion is the IfcOpenShell PyPI package in `libs/python/geometry`, never a NuGet pin, reached only over the existing `Runtime/transport#TRANSPORT_AXIS` UDS/InProcess companion rpc, so this page mints no transport, channel, or second wire vocabulary; a returned GLB re-enters the Bim glTF import rail as one `ImportedGeometry`, and the Bim IFC semantic graph and this hop's tessellated geometry are two projections of one content-keyed IFC artifact joined by the content-key; `python:data/spatial/geospatial` emits `EgressFormat.GEOARROW` as Arrow IPC bytes, and `GeoArrowRequest` carries that exact artifact to the companion for native geometry conversion before the existing GLB return — C# never invents a coordinate/offset ABI or hand-triangulates GeoArrow rings; `IdsAuditRequest` adds one ifctester invocation beside `IfcConvert` over the same companion rpc, passing IDS-XML with IFC content to the `python:geometry/ifc-companion` ifctester (`ids` oracle) and relaying the per-specification verdict wire back, which the Bim-owned `Review/validation#IDS_FACETS` `IdsAudit.Reconcile` composes into `IdsVerdict` rows and joins the C# self-audit against on the (GlobalId, `FacetKey`) axis — `FacetKey` the Bim composite join token unique within a specification (facet-type prefix and value discriminator), never the bare facet-type word — Compute referencing no Bim type and owning only the rpc orchestration and verdict relay, a Compute-minted IDS parser or a second transport the rejected form.

```csharp signature
// ImportedGeometry is the seam Rasm.Element/Projection/projection#INTERCHANGE_CARRIER shape — one kernel
// EncodedGeometry arena whose descriptor set names every declared lane, filled at the ONE Bim decode (UV from
// TEXCOORD_0, colour from the vertex-colour accessor). This lane bakes, partitions, and re-mints it
// lane-generically while the residency meshlet arm encodes each lane as its own stream, so a hit on a streamed
// cluster resolves a REAL unwrap with no second decode of the same GLB bytes. Compute-local carrier twins and
// per-column reads of the arena are both the deleted form.
public sealed record TessellationJob(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    string ResultFormatKey) {
    public static Fin<TessellationJob> Plan(string formatKey, bool requiresCompanion, ReadOnlyMemory<byte> ifcBytes, TessellationPolicy policy) =>
        requiresCompanion
            ? Fin.Succ(new TessellationJob(
                InterchangeIdentity.Key(formatKey, ifcBytes.Span, [policy.Deflection, policy.Tolerance, policy.AngleTolerance]), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, "glb"))
            : Fin.Fail<TessellationJob>(new ComputeFault.ModelRejected($"<tessellation-not-required:{formatKey}>"));

    // Companion-rpc cache-partition over source IFC + evaluation policy — NOT the durable GLB store address
    // (the Bim TESSELLATION_BRIDGE ContentKey, kernel seed-zero, that the Persistence object store keys).
    // ONE seam, TWO shapes, TWO names: the Bim bridge owns the TessellationRequest (the AEC-domain crossing under
    // its cache-before-cross/store-before-return policy), this lane owns the TessellationJob (the rpc job planned
    // from source bytes and evaluation policy).
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

- Owner: `FieldResidence` the closed exact/quantized/predicted residence `[Union]` whose per-case bits and bound ARE the arm's law — a lossless case selecting a lossy transform is unrepresentable; `FieldCodecPolicy` the chunked-layout record carrying the residence case and the compress column; `ResidualPredictor` the content-keyed model-lane chunk predictor; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `PointScan` the point-cloud carrier over E57/LAS/LAZ/PTS; `WaveformWindow`/`WaveformCorpus` the frame/hop-declared multi-channel waveform interchange carrier over long SHM records and fitted reference banks — the `Stats/signal` corpus seam, which stores nothing; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with error-bounded lossy, learned-residual-predicted, or exact lossless residence and a zero-copy solver↔store↔viz handoff; `InterchangeIo` the scientific-data ingest surface dispatching the chunked field decode and the point-scan ingest onto the admitted `Rasm.Persistence` `Ingest/pointcloud#SCAN_SOURCE` reader, the geometry and IFC import arms owned by `Rasm.Bim`. Two containers carry a chunked field and both are this codec's: its OWN 64-byte-header layout is the residence-bearing encode target, and HDF5 over the `[04]-[HDF_ARCHIVE]` capsule is both the ingest target every h5py and netCDF-4 corpus already writes and the interop egress those toolchains read back.
- Entry: `public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock, Option<ResidualPredictor> predictor = default, string dataset = "/field")` reads and reconstructs a self-describing chunked field, dispatching the native layout onto `FieldCodec.FieldDecode` and the HDF5 container onto `public static Fin<FieldArtifact> FieldCodec.Hdf5Decode(string formatKey, HdfHandle handle, string dataset, Instant at, Selection? window = null)` over a `[04]-[HDF_ARCHIVE]` handle, the optional `window` the station-slab file selection a partial read declares; `public static Fin<ComputeArtifact> FieldCodec.Hdf5Encode(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at)` emits the same station×component chunk model as an HDF5 1.10 container over the archive's cursor-guarded writer, `Predicted` refusing typed; `public static (ulong[] FileDims, uint[] Chunks) FieldCodec.Grid(ReadOnlySpan<int> extent, int components, int targetChunkElements)` the ONE station-outermost container-grid derivation every producer and archive consumer composes; `public static IO<Fin<PointScan>> ImportPoints(string formatKey, string codecKey, ReadOnlySequence<byte> bytes, ScanSpec spec, ProjectionContext frame, Func<ScanFact, IO<Unit>> sink)` reads a point-cloud scan by composing the Persistence `Ingest/pointcloud#SCAN_SOURCE` owner — one `ScanOp.Ingest` landing the capture bytes, one `ScanOp.Window` over the regions that ingest yielded — and folds the batches into `PointScan`, `pts` alone keeping its unadmitted-reader refusal; `public static Fin<WaveformCorpus> ImportWaveforms(string formatKey, HdfHandle handle, string dataset, WaveformWindow window, Instant at)` reads a `[samples, channels]` waveform corpus over a `[04]-[HDF_ARCHIVE]` handle under the declared frame/hop selection, the `sample-rate` attribute mandatory; `public static Fin<FieldArtifact> FieldDecode(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, Option<ResidualPredictor> predictor = default)` derives residence and compression from the header, decodes exact and quantized bodies directly, and reconstructs predicted bodies through their required model; `public static Fin<ComputeArtifact> FieldEncode(FieldArtifact field, string formatKey, FieldCodecPolicy policy, Instant at, Option<ResidualPredictor> predictor = default)` emits the chunked layout under the residence case's own law; `Fin<T>` aborts on a payload shorter than the fixed header prefix, corrupt compression, a chunk-shape mismatch, a quantized bound the bit budget cannot meet, or a predicted residence handed no predictor.
- Auto: chunk blob exposes two views — `FieldCodec.ChunkSequence`, a multi-segment `ReadOnlySequence<byte>` (one segment per chunk) streamed with no flatten, and `FieldArtifact.Chunk(ordinal)`, the per-ordinal random-access slice a frustum cull reads — both addressing chunks by `FieldArtifact.GridChunks` grid position, not byte offset; the quantized residence codes each chunk to its case's bit budget through the shared `Quantization` kernel (`TensorPrimitives.MaxMagnitude` scale, never a Max/Min/Abs hand-roll) and gates its own bound; the predicted residence walks chunks CAUSALLY — the stencil gathers axis-aligned face neighbours by `GridChunks` coordinate from the RECONSTRUCTED buffer (`GatherNeighbours`, the true spatial stencil, never a 1-D window crossing grid faces and never source values the decoder cannot hold), predicts through the `ResidualPredictor` ONNX field model, quantizes only the prediction residual, and re-codes an over-bound chunk's residual exactly (step 0) so the case bound holds by construction and `Reconstruct` inverts the walk from stored residuals alone; lossless Brotli-compresses via the `System.IO.Compression` span codec sized by `GetMaxCompressedLength`, no intermediate stream; the `ByteString` wrap fanning one chunk buffer to store blob and viz upload is the `Runtime/transport#ARTIFACT_FRAMES` frame law, composed.
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy or residual-predicted encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable.
- Packages: PureHDF (`NativeDataset.Read<T>(H5DatasetAccess, Span<T>, Selection?, Selection?, ulong[]?)` chunk-slab reads under `PureHDF.Selections.HyperslabSelection` file selections, `IH5DataType`/`H5DataTypeClass` element gating, `IH5DataLayout.Chunks` grid seating, `H5Dataset<T>(ulong[] fileDims, uint[] chunks, …)` deferred chunked writes — every open, cache, filter, and writer mechanic the `[04]-[HDF_ARCHIVE]` capsule owns), PureHDF.Filters.Lzf and PureHDF.Filters.BZip2.SharpZipLib (the managed filter pair — the accelerated Blosc2/Bitshuffle/ISA-L filters publish no osx RID, so this pair IS the admitted filter set, registered once at `HdfArchive.Mount`), System.IO.Hashing, System.Numerics.Tensors, Microsoft.ML.OnnxRuntime, LanguageExt.Core, NodaTime, Rasm.Persistence (project — `Ingest/pointcloud#SCAN_SOURCE` `ScanSource.Run`/`ScanSpec`/`ScanOp`/`ScanYield`/`ScanBatch`/`ScanHeader`/`ScanRegion`/`ScanFact`, `Element/graph#STORE_RAIL` `ProjectionContext`), BCL inbox (`System.IO.Compression` Brotli span codec, `System.Buffers` sequence segments)
- Growth: a new chunked field format is one dispatch row on `InterchangeIo.ImportField` — the `field-chunk` and container vocabulary is THIS page's, the Bim format axis carrying its own exchange formats and never these (its `[ROW_PROMOTION]` carve names chunked-field codecs Compute-owned, consumed at the seam); a new container is one dispatch row beside the native and HDF5 arms carrying its own reader, never a second field surface; a new point-scan format is one `ScanFormat` row at the Persistence reader and one `point-cloud` codec row on the Bim format axis, never a decode arm here; a new residence law is one `FieldResidence` case whose arm the `FieldEncode` `Switch` demands at compile time; a new waveform-corpus source is one `FormatKey` value on `WaveformCorpus` and a new windowing posture one `WaveformWindow` column, never a second carrier or a signal-local reader; a learned predictor is one `ResidualPredictor` content-keyed ONNX session reused across chunks; zero new surface — a `ResidualCoder`/`NeuralFieldCompressor` sibling is collapsed onto the `FieldResidence.Predicted` case and the one `ResidualEncode`/`Reconstruct` pair.
- Boundary: `Decode` proves the fixed `HeaderBytes` prefix present before its first slice, because `FieldDecode` lifts a thrown message directly onto the fault detail — a truncated payload must land the typed `<field-header-short:…>` refusal, never a BCL slice-range message dressed as this codec's verdict, and the `Pack` writer sizes off the same const so one number states the prefix. Field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor field rides the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` shape, chunked by station and component, never a generic byte blob; HDF5 is that same chunk model under another container, which is why it is ADMITTED rather than adapted — one station×component chunk IS one HDF5 chunk and the shuffle-plus-deflate pipeline IS this codec's compression leg, so an h5py or netCDF-4 corpus reads directly with no format bridge; the ingest seats the dataspace as the field extent with the trailing axis the COMPONENT axis of ONE dataset — one dataset per component is the refuted sibling layout, forking the chunk address a consumer computes — the container's own chunk grid seating onto `ChunkShape`/`GridChunks` so `Chunk(ordinal)` and the residual stencil survive ingest, an unwindowed read walking the corpus chunk-by-chunk and a `HyperslabSelection` `window` reading exactly the station slab a frustum or station read declares, so a re-chunk on import and an unqualified whole-dataset call are both the deleted form; the reader is pure managed with no native asset and decodes little-endian alone, so a big-endian source corpus refuses TYPED at the archive open rather than transposing bytes behind the caller; HDF5 writes are create-only and chunk-aligned, so an in-place edit of an ingested container is unrepresentable; the native layout stays the residence-bearing format and `Hdf5Encode` is its interop egress — `Exact` and `Quantized` land with bits and bound as attributes (evidence a foreign reader may ignore), `Predicted` refuses because no container slot carries a residence law a reader must enforce; the layout composes the suite `XxHash128` chunk identity content-addressed on the Persistence blob lane, so an identical chunk dedups and a re-read warms, a second field store the rejected form; the error bound is per-residence-case data the receipt records, never silently exceeded — the quantized arm faults an unmeetable bound and the predicted arm holds its bound by per-chunk exact fallback; the zero-copy edge is the remote frame law's `GetReadOnlySequence`/`UnsafeWrap` path, so a chunk crosses solver→store→viz with no managed copy, a `ToArray` flatten the named defect; the learned-compression terminal `ResidualPredictor` is one model-lane `Model/inference#INFERENCE_MODES` ONNX session content-keyed by the parametric-family digest and shared across chunks, composing the model lane rather than minting a second inference path, its grid-coordinate chunk index preserved (content-defined byte chunking destroys the grid locality the predictor needs — the FastCDC `#GEOMETRY_DELTA` chunker is the rejected rewrite), only the bounded residual stored, an over-bound chunk re-coded exact so the bound holds structurally, and the causal reconstructed-stencil walk making `Reconstruct` the codec's true inverse, the ONNX weights one content-addressed artifact the Python offline-science companion fits over the same offline-training seam the optimizer surrogate uses (never an in-proc fit), the achieved residual auditable on the `Cache` receipt; `PointScan` carries the `point-cloud` codec discriminant the Bim format axis names and COMPOSES the admitted E57/LAS/LAZ reader — `Rasm.Persistence` `Ingest/pointcloud#SCAN_SOURCE` owns that decode and the durable residence beneath it, so this arm mints no codec and narrows the double-position batches into the float carrier at one boundary, ASCII `pts` alone faulting `point-catalogue-pending` for want of a reader; the geometry mesh decode and IFC semantic ingest are the `Rasm.Bim` import rail, never re-derived — an `ImportGeometry`/`ImportIfc` arm here the deleted form; signal corpora cross ONLY through `ImportWaveforms` — `Stats/signal` composes `WaveformCorpus` and stores nothing, the recorded estimator (Arrow) and monitor (receipt-stream) negatives scoping the arm to genuine corpora, and a signal-local `H5File` open is the second surface the one-owner ruling rejects.

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

// Frame/hop IS the hyperslab law: the frame length maps onto the block extent and the hop onto the stride, so a
// non-overlapping walk (hop >= frame) reads as ONE strided selection and an overlapping walk (hop < frame) reads
// per-frame slabs, because an HDF5 hyperslab admits no stride below its block. Both forms are DECLARED
// selections, so the unqualified whole-record read of a screening-scale SHM corpus stays unspellable — a
// whole-record consumer (a fitted filter bank, a reference spectrum) declares frame == samples.
public sealed record WaveformWindow(int Frame, int Hop);

// Multi-channel waveform interchange carrier: long SHM records and python-fitted reference banks cross as ONE
// container class on the interchange surface — `Stats/signal` composes it and stores nothing, closing the
// estimator (Arrow) and monitor (receipt-stream) storage declines without re-opening them. Frames are frame-major
// `[FrameCount, Frame, Channels]` row-major float, the exact walk a Welch/PSD or filter-bank fold consumes.
public sealed record WaveformCorpus(
    string FormatKey,
    string Station,
    int Channels,
    long Samples,
    double SampleRate,
    int Frame,
    int Hop,
    int FrameCount,
    ReadOnlyMemory<float> Frames,
    Instant At);

public static class InterchangeIo {
    // Container dispatch, never a format test: the codec's own layout and HDF5 are two carriers of ONE chunk
    // model, so the key picks a reader and the `FieldArtifact` that lands is the same shape either way. The
    // predicted residence reaches only the native arm because only the native header names a residence at all —
    // an HDF5 corpus already applied whatever transform its producer chose, so an ingested field lands `Exact`
    // rather than publishing a bound this codec never enforced. The payload overload keeps the bytes-in seam a
    // companion or store lane hands over; a parallel or store-resident corpus opens its own `HdfSource.Path`/
    // `Mapped` handle and calls the handle arm directly.
    public static Fin<FieldArtifact> ImportField(string formatKey, string codecKey, ReadOnlyMemory<byte> bytes, IClock clock, Option<ResidualPredictor> predictor = default, string dataset = "/field", Selection? window = null) =>
        codecKey switch {
            "field-chunk" => FieldCodec.FieldDecode(formatKey, bytes, clock.GetCurrentInstant(), predictor),
            "hdf5" => HdfArchive.Open(new HdfSource.Payload(bytes), HdfArchivePolicy.Interchange)
                .Bind(handle => { using (handle) { return FieldCodec.Hdf5Decode(formatKey, handle, dataset, clock.GetCurrentInstant(), window); } }),
            _ => Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{codecKey}:{formatKey}>")),
        };

    // Point-scan decode COMPOSES the admitted reader rather than re-deriving one: `Rasm.Persistence`
    // `Ingest/pointcloud#SCAN_SOURCE` owns the E57/LAS/LAZ decode and the durable residence, so capture bytes
    // land once under `ArtifactKind.Scan` and the windowed read streams batches back out of that blob — the
    // ingest yield's own region cells ARE the window, so a full read costs no second census. Only ASCII `pts`
    // has no admitted reader and keeps its refusal.
    public static IO<Fin<PointScan>> ImportPoints(
        string formatKey, string codecKey, ReadOnlySequence<byte> bytes, ScanSpec spec,
        ProjectionContext frame, Func<ScanFact, IO<Unit>> sink) =>
        (codecKey, formatKey) switch {
            (not "point-cloud", _) => IO.pure(Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{formatKey}>"))),
            (_, "pts") => IO.pure(Fin.Fail<PointScan>(new ComputeFault.ModelRejected("<point-catalogue-pending:pts:ascii-pts-reader-unadmitted>"))),
            _ => Scanned(spec, bytes, frame, sink),
        };

    // Waveform-corpus ingest arm over the [HDF_ARCHIVE] handle — the signal lane's one storage seam: no
    // signal-local `H5File` open exists, the handle arrives from the one archive owner, one `NativeFile` per job.
    // The dataset is `[samples, channels]` little-endian float with a `sample-rate` attribute; the element gate
    // matches `Hdf5Decode` (f32 direct, f64 narrowed once — the library never width-converts).
    public static Fin<WaveformCorpus> ImportWaveforms(string formatKey, HdfHandle handle, string dataset, WaveformWindow window, Instant at) =>
        Try.lift(() => {
            NativeDataset source = handle.Dataset(dataset);
            ulong[] extent = source.Space.Dimensions;
            if (extent.Length != 2 || extent.Any(static axis => axis is 0UL or > int.MaxValue)) { throw new InvalidDataException($"<hdf5-waveform-space:[{string.Join(',', extent)}]>"); }
            long samples = (long)extent[0];
            int channels = (int)extent[1];
            if (window.Frame <= 0 || window.Hop <= 0 || window.Frame > samples) { throw new InvalidDataException($"<hdf5-waveform-window:{window.Frame}:{window.Hop}:{samples}>"); }
            if (!source.AttributeExists("sample-rate")) { throw new InvalidDataException($"<hdf5-waveform-rate:{dataset}>"); }
            double rate = source.Attribute("sample-rate").Read<double>();
            int frames = (int)((samples - window.Frame) / window.Hop) + 1;
            float[] values = source.Type.Class switch {
                H5DataTypeClass.FloatingPoint when source.Type.Size == 4 => Windowed<float>(source, handle.Access, window, frames, channels),
                H5DataTypeClass.FloatingPoint when source.Type.Size == 8 => FieldCodec.Narrowed(Windowed<double>(source, handle.Access, window, frames, channels)),
                _ => throw new InvalidDataException($"<hdf5-element:{source.Type.Class}:{source.Type.Size}>"),
            };
            return new WaveformCorpus(formatKey, dataset, channels, samples, rate, window.Frame, window.Hop, frames, values, at);
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    // hop >= frame is ONE strided hyperslab; hop < frame walks per-frame slabs — a stride below its block is the
    // selection form HDF5 refuses, so the overlap case pays one read per frame by construction.
    static T[] Windowed<T>(NativeDataset source, H5DatasetAccess access, WaveformWindow window, int frames, int channels) where T : unmanaged {
        long frameValues = (long)window.Frame * channels;
        T[] destination = new T[checked((int)(frames * frameValues))];
        if (window.Hop >= window.Frame) {
            HyperslabSelection strided = new(rank: 2,
                starts: [0UL, 0UL],
                strides: [(ulong)window.Hop, (ulong)channels],
                counts: [(ulong)frames, 1UL],
                blocks: [(ulong)window.Frame, (ulong)channels]);
            source.Read<T>(access, destination.AsSpan(), fileSelection: strided);
            return destination;
        }
        for (int frame = 0; frame < frames; frame++) {
            HyperslabSelection slab = new(rank: 2,
                starts: [(ulong)((long)frame * window.Hop), 0UL],
                blocks: [(ulong)window.Frame, (ulong)channels]);
            source.Read<T>(access, destination.AsSpan(checked((int)(frame * frameValues)), checked((int)frameValues)), fileSelection: slab);
        }
        return destination;
    }

    static IO<Fin<PointScan>> Scanned(ScanSpec spec, ReadOnlySequence<byte> bytes, ProjectionContext frame, Func<ScanFact, IO<Unit>> sink) =>
        from landed in ScanSource.Run(new ScanOp.Ingest(spec, bytes), frame, sink)
        from read in landed.Match(
            Succ: y => y is ScanYield.Landed done
                ? ScanSource.Run(new ScanOp.Window(spec, done.Scan, done.Regions.Map(static region => region.Cell)), frame, sink)
                    .Map(points => Folded(spec, done.Header, points))
                : IO.pure(Fin.Fail<PointScan>(new ComputeFault.ModelRejected("<point-ingest-yield>"))),
            Fail: static faults => IO.pure(Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-ingest:{faults.Head.Message}>"))))
        select read;

    // Batch lanes carry double positions with an optional 16-bit colour triple; `PointScan` is the float compute
    // carrier, so the fold narrows ONCE at this boundary. Intensity stays absent because the residence currency
    // carries no intensity lane, and a zero-filled one would read as measured.
    static Fin<PointScan> Folded(ScanSpec spec, ScanHeader header, Validation<ScanFault, ScanYield> points) => points.Match(
        Succ: y => y is ScanYield.Points batches
            ? Fin<PointScan>.Succ(Carried(spec, header, batches.Batches))
            : Fin<PointScan>.Fail(new ComputeFault.ModelRejected("<point-window-yield>")),
        Fail: static faults => Fin<PointScan>.Fail(new ComputeFault.ModelRejected($"<point-window:{faults.Head.Message}>")));

    static PointScan Carried(ScanSpec spec, ScanHeader header, Seq<ScanBatch> batches) {
        int points = batches.Sum(static batch => batch.Count);
        bool coloured = batches.ForAll(static batch => batch.Colors.IsSome);
        float[] positions = new float[points * 3];
        float[] colors = coloured ? new float[points * 3] : [];
        int cursor = 0;
        foreach (ScanBatch batch in batches) {
            ReadOnlySpan<double> xyz = batch.Positions.Span;
            for (int lane = 0; lane < batch.Count * 3; lane++) { positions[cursor + lane] = (float)xyz[lane]; }
            // 16-bit channels normalize to the unit float lane the compute carrier reads; an unnormalized
            // widen would put colour on a different scale than every other float channel here.
            batch.Colors.Iter(rgb => {
                ReadOnlySpan<ushort> channels = rgb.Span;
                for (int lane = 0; lane < batch.Count * 3; lane++) { colors[cursor + lane] = channels[lane] / (float)ushort.MaxValue; }
            });
            cursor += batch.Count * 3;
        }
        return new PointScan(spec.Format.Key, positions, coloured ? Some((ReadOnlyMemory<float>)colors) : None, None, header.Points, header.At);
    }
}

// Shared codec quantization law, composed by the field quantizer, the residual quantizer, and the
// [GEOMETRY_DELTA] normalization rows: scale is one absolute-extremum SIMD reduction (never a Max/Min/Abs
// hand-roll), step the bit-budget grid, residual the relative rounding error a receipt records. One generic
// declaration serves every IEEE width — the float32 field and residual lanes and the float64 parametric net —
// through `TensorPrimitives.MaxMagnitude<T>`, so a per-width overload pair is the deleted form.
public static class Quantization {
    public static (T Scale, T Step) Steps<T>(ReadOnlySpan<T> source, int bits) where T : IFloatingPointIeee754<T> {
        T scale = T.Abs(TensorPrimitives.MaxMagnitude(source));
        int levels = (1 << bits) - 1;
        return (scale, levels > 0 ? scale / T.CreateChecked(levels) : T.Zero);
    }

    public static T Code<T>(T value, T step) where T : IFloatingPointIeee754<T> =>
        step == T.Zero ? value : T.Round(value / step) * step;

    public static double Residual(float value, float coded, float scale) => scale == 0f ? 0.0 : Math.Abs(value - coded) / scale;
}

public static class FieldCodec {
    // The fixed header prefix every layout carries ahead of its variable grid and chunk extents. `Decode` gates on
    // it BEFORE the first slice: `FieldDecode` maps a thrown message straight onto the fault detail, so a payload
    // shorter than the prefix would otherwise publish a slice-range message as this codec's typed refusal —
    // unslugged, unmatched by any recovery, and naming a BCL argument instead of a truncated artifact.
    public const int HeaderBytes = 64;

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

    // HDF5 ingest arm over the [HDF_ARCHIVE] handle. The dataspace IS the field extent with the trailing axis
    // the component axis — the same station×component layout the native header spells — and the container's OWN
    // chunk grid seats onto ChunkShape/GridChunks, so `Chunk(ordinal)`, `ChunkSequence`, and the residual stencil
    // stay live for an ingested corpus instead of collapsing onto one giant chunk. `window` narrows the file
    // selection to the station slab a frustum or station read asks for; absent, the read still walks the corpus
    // chunk-by-chunk into the destination rather than one unqualified whole-dataset call. Contiguous corpora
    // (Layout.Class without a chunk grid) land as one station-outermost derived grid. FloatingPoint size 4 reads
    // direct; size 8 reads double and narrows ONCE here — probe-proven: the library never width-converts, a
    // float32 destination over a float64 dataset (and the reverse) refuses `Unable to decode values types of
    // different type size.`, so the double read IS the only f64 path; every other element class refuses typed —
    // the reader is little-endian only, so a big-endian corpus faults at the archive open, never at the float cast.
    public static Fin<FieldArtifact> Hdf5Decode(string formatKey, HdfHandle handle, string dataset, Instant at, Selection? window = null) =>
        Try.lift(() => {
            NativeDataset source = handle.Dataset(dataset);
            ulong[] extent = source.Space.Dimensions;
            if (extent.Length < 2 || extent.Any(static axis => axis is 0UL or > int.MaxValue)) { throw new InvalidDataException($"<hdf5-dataspace:[{string.Join(',', extent)}]>"); }
            int[] shape = [.. extent[..^1].Select(static axis => (int)axis)];
            int components = (int)extent[^1];
            ulong[] grid = source.Layout.Chunks is { Length: > 0 } chunked ? chunked : Grid([.. shape], components, ChunkElementTarget).FileDims;
            int[] chunkShape = [.. grid[..^1].Select(static axis => (int)axis)];
            int[] gridChunks = [.. shape.Zip(chunkShape, static (whole, chunk) => (whole + chunk - 1) / chunk)];
            float[] values = source.Type.Class switch {
                H5DataTypeClass.FloatingPoint when source.Type.Size == 4 => Slabbed<float>(source, handle.Access, window, extent),
                H5DataTypeClass.FloatingPoint when source.Type.Size == 8 => Narrowed(Slabbed<double>(source, handle.Access, window, extent)),
                _ => throw new InvalidDataException($"<hdf5-element:{source.Type.Class}:{source.Type.Size}>"),
            };
            return new FieldArtifact(
                formatKey, dataset, shape.Length, components, values.LongLength / components,
                chunkShape, gridChunks, gridChunks.Aggregate(1, static (acc, dim) => acc * dim),
                MemoryMarshal.AsBytes(values.AsSpan()).ToArray(), 0.0, at);
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    // One chunk-aligned slab read per grid ordinal in index order under the handle's bounded per-read cache —
    // never one unqualified whole-dataset call; an explicit window reads exactly its selection.
    static T[] Slabbed<T>(NativeDataset source, H5DatasetAccess access, Selection? window, ulong[] extent) where T : unmanaged {
        if (window is not null) {
            T[] narrow = new T[checked((int)window.TotalElementCount)];
            source.Read<T>(access, narrow.AsSpan(), fileSelection: window);
            return narrow;
        }
        long total = extent.Aggregate(1L, static (acc, axis) => acc * (long)axis);
        T[] whole = new T[checked((int)total)];
        int stations = (int)extent[0];
        long slab = total / stations;
        for (int station = 0; station < stations; station++) {
            HyperslabSelection file = new(rank: extent.Length,
                starts: [(ulong)station, .. extent[1..].Select(static _ => 0UL)],
                blocks: [1UL, .. extent[1..]]);
            source.Read<T>(access, whole.AsSpan(checked((int)(station * slab)), checked((int)slab)), fileSelection: file);
        }
        return whole;
    }

    internal static float[] Narrowed(double[] wide) {
        float[] narrow = new float[wide.Length];
        for (int lane = 0; lane < wide.Length; lane++) { narrow[lane] = (float)wide[lane]; }
        return narrow;
    }

    // Default chunk budget for derived grids; a slab above it splits its largest interior axis until it fits.
    public const int ChunkElementTarget = 1 << 18;

    // ONE station-outermost container-grid derivation serves the native layout, the HDF5 encode, and every archive
    // consumer — `Solver/discretization` `FieldSpace` composes it downward, so two chunk grids never fork one
    // concept. Station axis chunks at 1, the component axis rides whole as the trailing extent, and interior axes
    // halve largest-first until the slab meets the element budget.
    public static (ulong[] FileDims, uint[] Chunks) Grid(ReadOnlySpan<int> extent, int components, int targetChunkElements) {
        ulong[] dims = [.. extent.ToArray().Select(static axis => (ulong)axis), (ulong)components];
        uint[] chunks = [1U, .. extent[1..].ToArray().Select(static axis => (uint)axis), (uint)components];
        long slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        while (slab > targetChunkElements) {
            int widest = 1;
            for (int axis = 2; axis < chunks.Length - 1; axis++) { widest = chunks[axis] > chunks[widest] ? axis : widest; }
            if (chunks[widest] <= 1) { break; }
            chunks[widest] = (chunks[widest] + 1) / 2;
            slab = chunks.Aggregate(1L, static (acc, axis) => acc * axis);
        }
        return (dims, chunks);
    }

    // HDF5 egress arm: the interop counterpart of `Hdf5Decode`, emitting the SAME station×component chunk model
    // as an HDF5 1.10 container h5py, ParaView, and HDFView open with no bridge (Shuffle id 2 + Deflate id 1 reads
    // back as compression='gzip', shuffle=True). The native 64-byte layout stays the residence-bearing format:
    // `Predicted` refuses typed because no container slot carries a residence law, and a Quantized encode lands its
    // ALREADY-CODED values with bits and bound as attributes — evidence, never a law a foreign reader must enforce.
    // Chunk writes walk grid ordinals in index order through the archive writer, whose cursor refuses out-of-order
    // ahead of the library's mid-encode chunk-once fault.
    public static Fin<ComputeArtifact> Hdf5Encode(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at) =>
        policy.Residence.Switch(
            state: (field, policy, archive, sink, at),
            exact: static (s, _) => Emit(s.field, s.policy, s.archive, s.sink, s.at),
            quantized: static (s, _) => Emit(s.field, s.policy, s.archive, s.sink, s.at),
            predicted: static (s, _) => Fin.Fail<ComputeArtifact>(new ComputeFault.ModelRejected("<hdf5-residence:predicted>")));

    static Fin<ComputeArtifact> Emit(FieldArtifact field, FieldCodecPolicy policy, HdfArchivePolicy archive, Stream sink, Instant at) =>
        Try.lift(() => {
            ulong[] dims = [.. field.GridChunks.Zip(field.ChunkShape, static (grid, chunk) => (ulong)((long)grid * chunk)), (ulong)field.Components];
            uint[] chunks = [.. field.ChunkShape.Select(static axis => (uint)axis), (uint)field.Components];
            H5Dataset<float[]> slot = new(dims, chunks, datasetCreation: archive.Creation());
            H5File graph = new() { [field.Station.Length > 0 ? field.Station : "field"] = slot };
            graph.Attributes["format-key"] = field.FormatKey;
            graph.Attributes["residence"] = policy.Residence.QuantizationBits == 0 ? "exact" : "quantized";
            graph.Attributes["bits"] = policy.Residence.QuantizationBits;
            graph.Attributes["bound"] = policy.Residence.ErrorBound;
            graph.Attributes["max-residual"] = field.MaxResidual;
            using HdfWriter writer = HdfArchive.Begin(graph, sink, archive);
            for (int ordinal = 0; ordinal < field.ChunkCount; ordinal++) {
                writer.WriteChunk(slot, MemoryMarshal.Cast<byte, float>(field.Chunk(ordinal).Span).ToArray(), ordinal, field.GridChunks, chunks);
            }
            return Unit.Default;
        }).Run()
        .MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message))
        .Map(_ => ComputeArtifact.Of("hdf5", ReadBack(sink), at, [policy.Residence.QuantizationBits, policy.Residence.ErrorBound]));

    // The sink is the composition's pooled stream (the ARTIFACT_FRAMES grant); the emitted container re-reads off
    // it as one contiguous view for the content-key mint, never a second buffer.
    static ReadOnlyMemory<byte> ReadBack(Stream sink) {
        sink.Position = 0;
        byte[] emitted = new byte[checked((int)sink.Length)];
        sink.ReadExactly(emitted);
        return emitted;
    }

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
        // Pinned policy shape GOVERNS: artifact layout equals it exactly, while an empty policy shape inherits.
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
        if (span.Length < HeaderBytes) { throw new InvalidDataException($"<field-header-short:{span.Length}>"); }
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
        float[] quantized = source.Select(value => Quantization.Code(value, step)).ToArray();
        double worst = toSeq(source.Zip(quantized)).Fold(0d, (value, pair) => Math.Max(value, Quantization.Residual(pair.First, pair.Second, scale)));
        return field with { Chunks = MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), MaxResidual = worst };
    }

    // Self-describing header: the fixed station/rank/components/count prefix, uncompressed payload length,
    // GridChunks extent (grid-coordinate index), and ChunkShape extent precede the body, so Decode rebuilds the
    // chunk grid and residual stencil from bytes alone — never an out-of-band policy agreement that mis-counts chunks.
    static byte[] Pack(FieldArtifact field, FieldCodecPolicy policy) {
        int gridRank = field.GridChunks.Length, chunkRank = field.ChunkShape.Length;
        byte[] header = new byte[HeaderBytes + gridRank * 4 + chunkRank * 4];
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
        for (int axis = 0; axis < chunkRank; axis++) { BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(HeaderBytes + gridRank * 4 + axis * 4), field.ChunkShape[axis]); }
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

## [04]-[HDF_ARCHIVE]

- Owner: `HdfArchive` — the branch's ONE HDF5 container-session capsule: every open, selection read, deferred chunked write, and filter registration in the Compute closure crosses it, so the process-static filter registry seats once, the parallel-entry law rides a handle column instead of caller memory, and the chunk-once ordering refuses at admission ahead of the library's mid-encode fault; `HdfSource` the closed payload/path/mapped source `[Union]` whose case IS the concurrency law; `HdfHandle` the job-scoped open (one `NativeFile` per job, disposed at the job boundary); `HdfArchivePolicy` the filter-and-cache policy whose `DeflateGrade` row vocabulary makes the compress path's four-value law unrepresentable to violate; `HdfWriter` the `BeginWrite` session wrapper carrying the per-dataset monotone chunk cursor.
- Cases: `HdfSource.Payload` (bytes in hand — zero-copy stream view, single-reader by the shared `Stream.Position`), `HdfSource.Path` (file-handle driver, `ThreadLocal` position, parallel-fan-out safe), `HdfSource.Mapped` (memory-mapped accessor, parallel-fan-out safe); `DeflateGrade.Default|Store|Fast|Dense` — the ONLY levels the compress path serves (`-1`, `0`, `1`, `9`); every other integer passes dataset construction and faults at the first chunk compress, which is why the grade is a row and never an `int` knob.
- Law: reads DECLARE their selection — the read entry takes a `Selection` and a caller-owned destination span, so an unqualified whole-dataset read of a screening-scale corpus is unspellable; the per-read chunk cache sizes from policy (`SimpleReadingChunkCache`, default 521 slots / 1 MiB) because a working set past the cache re-decompresses every miss, so slab-scale readers pass a slab-sized policy; write staging is unbounded by the library (`SimpleWritingChunkCache` holds every touched chunk until flush), so chunk-aligned index-order writing IS the memory bound, enforced by the writer cursor, never by a smaller cache.
- Entry: `HdfArchive.Mount()` once at composition (the `Tiles3DExtensions.RegisterExtensions` precedent) registering `LzfFilter` and `BZip2SharpZipLibFilter` — the COMPLETE managed filter extension on the branch RID, the accelerated Blosc2/Bitshuffle/ISA-L natives publishing no osx payload; `HdfArchive.Open(HdfSource, HdfArchivePolicy)` minting the job handle; `HdfHandle.Dataset(string)` resolving the CONCRETE `NativeDataset` (the `Span<T>` and `H5DatasetAccess` read overloads live there alone, never the `IH5Dataset` face) and `HdfHandle.Group(string)` resolving the `NativeGroup` whose attribute roster metadata reads walk; `HdfArchive.Begin(H5File, Stream, HdfArchivePolicy)` opening the deferred-write session over the composition's pooled sink.
- Output: consumers hold `HdfHandle`/`HdfWriter`, never a `NativeFile` — driver, chunk cache, and global-heap map all hang off the file object, so a long-lived open across jobs is the rejected form.
- Packages: PureHDF (`H5File.OpenRead`/`Open(Stream, bool, H5ReadOptions?)`/`Open(MemoryMappedViewAccessor, H5ReadOptions?)`, `NativeFile`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, Selection?, Selection?, ulong[]?)`, `H5DatasetAccess`, `SimpleReadingChunkCache`, `H5File.BeginWrite(Stream, H5WriteOptions?)`, `H5NativeWriter.Write<T>(H5Dataset<T>, T, Selection?, Selection?)`, `H5DatasetCreation`, `H5Filter.Register`, `DeflateFilter`/`ShuffleFilter`/`Fletcher32Filter`), PureHDF.Filters.Lzf (`LzfFilter`), PureHDF.Filters.BZip2.SharpZipLib (`BZip2SharpZipLibFilter`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`System.IO.MemoryMappedFiles`, `System.Runtime.InteropServices.MemoryMarshal`)
- Growth: a new archive artifact class (solver time-history, modal basis, ensemble store, sparse-operator exchange, checkpoint, response corpus) is a CONSUMER composing `Open`/`Begin` with its own datasets and attributes — zero rows here; a new filter is one `H5Filter.Register` row inside `Mount`; a new source modality is one `HdfSource` case whose `Parallel` column states its law.
- Boundary: create-only is the container's law — `Write` and `BeginWrite` truncate, no append, no in-place edit, no re-open-for-write exists, so an accumulating series segments at its producer's own cadence edge (one session per segment) and a growth story re-encodes whole; `H5Constants.Unlimited` in a deferred dataset's `fileDims` faults the `BeginWrite` encode itself — probe-proven, the dataspace sizing overflows before any chunk lands — so deferred writes declare fixed extents and unlimited never rescues an append design; big-endian corpora refuse at open (`<hdf5-byte-order:…>`) — the assembly ships a converter no read path calls, so re-encode upstream, never here; SZIP (id 4) is absent, N-Bit (id 5) throws both directions, Scale-Offset (id 6) decompresses only — a corpus on any of the three refuses TYPED by filter id, no managed substitute exists; every PureHDF fault leaving this capsule wears the `<hdf5-…>` slug grammar, a raw library message dressed as a Compute verdict the named defect; the capsule is job machinery, never a store — artifacts it emits land content-addressed through `ArtifactIndexRow.Admit` on the Persistence blob lane, and a Compute-side file catalog, multi-file scan, or open-handle registry beside that index is the drift defect the custodian ruling names.

```csharp signature
// The source case IS the concurrency law: a fan-out gates on `Parallel` instead of a caller remembering which
// driver keeps its read position in a ThreadLocal.
[Union]
public abstract partial record HdfSource {
    private HdfSource() { }

    public sealed record Payload(ReadOnlyMemory<byte> Bytes) : HdfSource;
    public sealed record Path(string File) : HdfSource;
    public sealed record Mapped(MemoryMappedViewAccessor View) : HdfSource;

    public bool Parallel => Switch(
        payload: static _ => false,
        path: static _ => true,
        mapped: static _ => true);
}

// The compress path maps ONLY these four levels onto CompressionLevel and throws on the rest AFTER dataset
// construction — the grade row turns that mid-encode fault into an unspellable state.
[SmartEnum<int>]
public sealed partial class DeflateGrade {
    public static readonly DeflateGrade Default = new(-1);
    public static readonly DeflateGrade Store = new(0);
    public static readonly DeflateGrade Fast = new(1);
    public static readonly DeflateGrade Dense = new(9);
}

public sealed record HdfArchivePolicy(DeflateGrade Deflate, bool Shuffle, bool Checksum, int ReadCacheSlots, ulong ReadCacheBytes) {
    public static readonly HdfArchivePolicy Interchange = new(DeflateGrade.Fast, Shuffle: true, Checksum: false, ReadCacheSlots: 521, ReadCacheBytes: 1UL << 20);

    // Shuffle id 2 ahead of Deflate id 1 is the h5py `compression='gzip', shuffle=True` pipeline; Fletcher32
    // id 3 appends only when the corpus wants end-to-end detection, `SkipEdc` being the read-side bypass.
    public H5DatasetCreation Creation() => new(Filters: [
        .. Shuffle ? new H5Filter[] { new(ShuffleFilter.Id) } : [],
        new(DeflateFilter.Id, new() { [DeflateFilter.COMPRESSION_LEVEL] = Deflate.Key }),
        .. Checksum ? new H5Filter[] { new(Fletcher32Filter.Id) } : []]);
}

public sealed class HdfHandle : IDisposable {
    internal HdfHandle(NativeFile file, bool parallel, H5DatasetAccess access) { File = file; Parallel = parallel; Access = access; }

    internal NativeFile File { get; }
    public bool Parallel { get; }
    public H5DatasetAccess Access { get; }

    // Concrete resolve: the Span/H5DatasetAccess overloads and the real chunk grid live on NativeDataset alone.
    public NativeDataset Dataset(string path) => File.Dataset(path) as NativeDataset
        ?? throw new InvalidDataException($"<hdf5-dataset:{path}>");

    // Group resolve stays on the handle for the same reason: attribute rosters read off the resolved object, so
    // no consumer touches the NativeFile to reach its metadata.
    public NativeGroup Group(string path) => File.Group(path) as NativeGroup
        ?? throw new InvalidDataException($"<hdf5-group:{path}>");

    public bool Exists(string path) => File.LinkExists(path);

    public void Dispose() => File.Dispose();
}

public static class HdfArchive {
    static int _mounted;

    // Token-gated one-shot: the registry is a process-static ConcurrentDictionary, so a per-read register writes
    // shared state on the hot path and a second Mount is a no-op, never a re-seed.
    public static void Mount() {
        if (Interlocked.Exchange(ref _mounted, 1) == 1) { return; }
        H5Filter.Register(new LzfFilter());
        H5Filter.Register(new BZip2SharpZipLibFilter());
    }

    public static Fin<HdfHandle> Open(HdfSource source, HdfArchivePolicy policy) =>
        Try.lift(() => {
            H5DatasetAccess access = new() { ChunkCache = new SimpleReadingChunkCache(policy.ReadCacheSlots, policy.ReadCacheBytes) };
            return source.Switch(
                state: access,
                payload: static (a, payload) => new HdfHandle(H5File.Open(View(payload.Bytes), leaveOpen: false), parallel: false, a),
                path: static (a, path) => new HdfHandle(H5File.OpenRead(path.File), parallel: true, a),
                mapped: static (a, mapped) => new HdfHandle(H5File.Open(mapped.View), parallel: true, a));
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<hdf5-open:{error.Message}>"));

    // Zero-copy view over an array-backed payload; a non-array payload takes its one staging copy HERE, typed,
    // never an unreceipted ToArray at a call site.
    static MemoryStream View(ReadOnlyMemory<byte> bytes) =>
        MemoryMarshal.TryGetArray(bytes, out ArraySegment<byte> segment)
            ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new MemoryStream(bytes.ToArray(), writable: false);

    public static HdfWriter Begin(H5File graph, Stream sink, HdfArchivePolicy policy) =>
        new(graph.BeginWrite(sink, new H5WriteOptions()));
}

// Deferred-write session with the per-dataset monotone chunk cursor: `Chunks can only be written once.` throws
// from the library's v4 index MID-ENCODE, after the producing work is already spent — the cursor refuses the
// out-of-order or repeated ordinal at admission instead.
public sealed class HdfWriter : IDisposable {
    readonly H5NativeWriter _writer;
    readonly Dictionary<object, int> _cursors = [];

    internal HdfWriter(H5NativeWriter writer) => _writer = writer;

    public void WriteChunk<T>(H5Dataset<T[]> slot, T[] chunk, int ordinal, ReadOnlySpan<int> grid, ReadOnlySpan<uint> chunkShape) where T : unmanaged {
        int expected = _cursors.TryGetValue(slot, out int cursor) ? cursor : 0;
        if (ordinal != expected) { throw new InvalidDataException($"<hdf5-chunk-order:{ordinal}:{expected}>"); }
        _cursors[slot] = expected + 1;
        _writer.Write(slot, chunk, fileSelection: FileSelection(ordinal, grid, chunkShape));
    }

    // Grid ordinal -> chunk-aligned hyperslab: coordinates decompose row-major over the grid, starts land on
    // chunk boundaries, blocks are one chunk — the only write shape the write-once law admits.
    static HyperslabSelection FileSelection(int ordinal, ReadOnlySpan<int> grid, ReadOnlySpan<uint> chunkShape) {
        int rank = chunkShape.Length;
        ulong[] starts = new ulong[rank];
        int remainder = ordinal;
        for (int axis = grid.Length - 1; axis >= 0; axis--) {
            starts[axis] = (ulong)(remainder % grid[axis]) * chunkShape[axis];
            remainder /= grid[axis];
        }
        ulong[] blocks = new ulong[rank];
        for (int axis = 0; axis < rank; axis++) { blocks[axis] = chunkShape[axis]; }
        return new HyperslabSelection(rank, starts, blocks);
    }

    public void Dispose() => _writer.Dispose();
}
```

## [05]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` structural-diff target rows; `GeometryDelta` the content-addressed delta record; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control.
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the ordered target chunk recipe (`TargetChunks`) with the new-chunk payload (`Added`, hashes absent from the base); `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` walks the recipe and reconstructs the NORMALIZED target exactly, pulling each chunk from the payload or the re-chunked base — `TargetHash` is taken over the normalized bytes, so the verify proves the reconstruction bit-for-bit and `GeometryDelta.GeometricError` states the residual that separates it from the caller's original target; `Fin<T>` aborts on invalid chunk policy or float alignment, base or target hash mismatch, corrupt payload framing, and an unresolved recipe hash.
- Auto: `Diff` first runs each kind's row-owned `Normalize` — the float-column kinds (vertex/point) round every float to the finer of the bit-budget grid and `Tolerance` so a sub-tolerance perturbation hashes to one chunk, bounded-lossy within `Tolerance`; topology and B-rep-face streams pass verbatim; the `nurbs-control` parametric stream rounds its control-net coordinate block alone, knots and weights crossing verbatim — then runs FastCDC over the normalized bytes — a 256-entry SplitMix64 `Gear` table rolls the fingerprint, a STRICT mask below `AvgChunk` and a LOOSE mask above normalize the chunk-size distribution so an inserted vertex shifts only its local chunk; `TargetChunks` records the ordered hash recipe, `Added` the distinct new chunks, and the delta's own `GeometricError` the quantization step every one of them was rounded to (zero on a kind that passed verbatim), so the residual is stated once rather than restated per chunk; the progressive column orders new chunks largest-first so a transmission renders coarse coverage before fine detail; the delta carries its own `DeltaPolicy` so `Apply` re-chunks the base identically and round-trips deterministically.
- Receipt: the `Cache` receipt carries the delta content-key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable; a progressive transmission stamps the coarse-chunk-first ordering count.
- Packages: System.IO.Hashing, System.Numerics.Tensors, LanguageExt.Core, Rasm (project — the kernel reconciliation `EncodeForm.Parametric` canonical stream the `nurbs-control` payload rides), Rasm.Persistence (project), BCL inbox (`System.Numerics.BitOperations` mask sizing)
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row carrying its row-owned `Normalize` law; a new chunk policy is one column on `DeltaPolicy`; the quantization law is the shared `Quantization` kernel ([FIELD_RESULT_CODEC]); zero new surface.
- Boundary: geometry delta is the structural diff the blob-level delta never owned — the Persistence blob delta diffs opaque bytes, this diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only touched chunks; the diff algebra mirrors the `Runtime/wire#PROTO_VOCABULARY` `GraphDiff`/`SubtreeFetch` wire shape, Compute owning the structural chunking and the Persistence sync lane the closure-graph diff, neither re-deriving the other; the chunker is real FastCDC — a `Gear` rolling fingerprint with a STRICT-below / LOOSE-above-`AvgChunk` dual-mask tightening the size distribution so a local edit shifts only its own chunk, a fixed-block or single-mask shift-add chunker the rejected form; reconstruction is order-faithful and hash-verified — `TargetChunks` places a mid-stream insert at its true position, not the tail, and `Apply` re-chunks the base under the delta's OWN `DeltaPolicy`, never a hardcoded one — but LOSSLESS is a per-KIND property this codec never claims whole: a non-quantizable kind (`mesh-topology`, `brep-face`) passes `Normalize` verbatim, so its reconstruction IS the original target, while a quantizable kind hashes the NORMALIZED bytes, so `Apply` returns the target rounded to the delta's own grid and `GeometryDelta.GeometricError` carries that step — the finer of the bit grid and `Tolerance`, the residual law the `DeltaPolicy` row decides — as a bound the caller STATES rather than assumes; a delta advertised lossless across every kind is what turns a bounded-lossy round trip into a silent one, and a per-chunk restatement of the one step is the column that collapse deleted; the bounded-lossy `Normalize` never exceeds the geometry tolerance; the new-chunk set transmits progressively through the `SubtreeFetch` server-stream and content-key-dedups against the Persistence blob lane (never a second delta store); the geometry-kind discriminant scopes quantization, so a topology-only edit never quantizes and a position-only edit never re-transmits the topology column; the `nurbs-control` payload IS the kernel `Rasm/Spatial/reconciliation#RECONCILIATION_BRIDGE` `EncodeForm.Parametric` canonical counted stream — the one frozen parametric byte layout, read here the way Persistence reads the frozen mesh layout, so a Compute-local NURBS byte encoding is the deleted second layout — and its row's `Normalize` scopes the tolerance grid to the control-net coordinate block ALONE, knot and weight bytes crossing verbatim: the `Rasm/Parametric/nurbs#NURBS_ENGINE` `Nurbs.Of` admission law (normalized clamped knots, strictly positive weights) holds by CONSTRUCTION on every emitted delta, so a rounded net the owner faults is unrepresentable rather than guarded — a whole-stream float grid rounding knots and weights, driving a weight non-positive or de-normalizing a knot vector, is the rejected form — while a malformed counted layout refuses the typed `<delta-parametric-layout:…>` fault at normalization, and a post-quantization re-admission call re-validating what the scoped grid already preserves is the interior re-validation the admission law forecloses.

```csharp signature
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

public sealed record DeltaPolicy(int MinChunk, int AvgChunk, int MaxChunk, int QuantizationBits, double Tolerance, bool Progressive) {
    public static readonly DeltaPolicy Canonical = new(MinChunk: 2048, AvgChunk: 8192, MaxChunk: 65536, QuantizationBits: 14, Tolerance: 1e-5, Progressive: true);
}

public readonly record struct DeltaChunk(UInt128 Hash, int Ordinal, int Offset, int ByteLength);

// `TargetHash` is over the NORMALIZED target, so `Apply` reconstructs THAT exactly and `GeometricError` is the one
// quantization step separating it from the caller's original bytes — zero wherever the kind passed verbatim. The
// bound rides the delta rather than each `Added` chunk because every chunk was rounded to the same step: a
// per-chunk copy of one number is a value a partial transmission can contradict and a caller has to reduce.
public sealed record GeometryDelta(
    GeometryDeltaKind Kind,
    UInt128 BaseHash,
    UInt128 TargetHash,
    Seq<UInt128> TargetChunks,
    Seq<DeltaChunk> Added,
    ReadOnlyMemory<byte> Payload,
    DeltaPolicy Policy,
    double GeometricError,
    long BaseBytes,
    long DeltaBytes);

public static class DeltaCodec {
    public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy) =>
        !ValidPolicy(policy)
            ? Fin.Fail<GeometryDelta>(new ComputeFault.ModelRejected($"<delta-policy:{kind.Key}:{policy.MinChunk}:{policy.AvgChunk}:{policy.MaxChunk}:{policy.QuantizationBits}:{policy.Tolerance:R}>"))
            : kind.Normalize(baseBytes, policy).Bind(normalizedBase =>
              kind.Normalize(targetBytes, policy).Map(normalizedTarget => {
                  HashSet<UInt128> baseSet = FastCdc(normalizedBase.Bytes.Span, policy).Map(static c => c.Hash).ToHashSet();
                  Seq<DeltaChunk> targetChunks = FastCdc(normalizedTarget.Bytes.Span, policy);
                  Seq<DeltaChunk> added = toSeq(targetChunks.Filter(c => !baseSet.Contains(c.Hash)).DistinctBy(static c => c.Hash));
                  Seq<DeltaChunk> ordered = policy.Progressive ? toSeq(added.OrderByDescending(static c => c.ByteLength)) : added;
                  return new GeometryDelta(kind,
                      XxHash128.HashToUInt128(baseBytes.Span), XxHash128.HashToUInt128(normalizedTarget.Bytes.Span),
                      targetChunks.Map(static c => c.Hash), ordered, Concatenate(ordered, normalizedTarget.Bytes), policy, normalizedTarget.Step,
                      baseBytes.Length, ordered.Sum(static c => (long)c.ByteLength));
              }));

    public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes) =>
        !ValidPolicy(delta.Policy)
            ? Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-policy:{delta.Kind.Key}>"))
            : XxHash128.HashToUInt128(baseBytes.Span) == delta.BaseHash
            ? Reconstruct(delta, baseBytes).Bind(target => XxHash128.HashToUInt128(target.Span) == delta.TargetHash
                ? Fin.Succ(target)
                : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-target-mismatch:{delta.TargetHash:x32}>")))
            : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.CacheCorrupt($"<delta-base-mismatch:{delta.BaseHash:x32}>"));

    static bool ValidPolicy(DeltaPolicy policy) =>
        policy.MinChunk > 0 && policy.MinChunk <= policy.AvgChunk && policy.AvgChunk <= policy.MaxChunk
            && policy.QuantizationBits is >= 0 and <= 24 && double.IsFinite(policy.Tolerance) && policy.Tolerance > 0d;

    // --- [NORMALIZATION_ROWS]

    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeVerbatim(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        Fin.Succ((bytes, 0d));

    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeFloatColumns(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
        if (bytes.IsEmpty || bytes.Length % sizeof(float) != 0) { return Fin.Fail<(ReadOnlyMemory<byte> Bytes, double Step)>(new ComputeFault.ModelRejected($"<delta-float-alignment:{bytes.Length}>")); }
        if (policy.QuantizationBits <= 0) { return Fin.Succ(((ReadOnlyMemory<byte>)bytes, 0d)); }
        float[] source = MemoryMarshal.Cast<byte, float>(bytes.Span).ToArray();
        float step = GridStep<float>(source, policy);
        float[] quantized = source.Select(value => Quantization.Code(value, step)).ToArray();
        return Fin.Succ(((ReadOnlyMemory<byte>)MemoryMarshal.AsBytes(quantized.AsSpan()).ToArray(), (double)step));
    }

    // nurbs-control payload is the kernel EncodeForm.Parametric canonical counted stream (little-endian: direction
    // count; per direction degree, knot count, knots; weight count, weights; control count, xyz doubles). The grid
    // touches the CONTROL-NET block alone — knot and weight bytes copy verbatim — so the Nurbs.Of gate (normalized
    // clamped knots, strictly positive weights) holds by construction on every emitted delta, and a malformed
    // counted layout lands the typed refusal instead of a BCL slice-range message wearing this codec's verdict.
    internal static Fin<(ReadOnlyMemory<byte> Bytes, double Step)> NormalizeParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) =>
        Try.lift(() => ParametricNet(bytes, policy)).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<delta-parametric-layout:{error.Message}>"));

    static (ReadOnlyMemory<byte> Bytes, double Step) ParametricNet(ReadOnlyMemory<byte> bytes, DeltaPolicy policy) {
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
        if (directions < 1 || weights != controls || netOffset + (controls * 3 * sizeof(double)) != stream.Length) { throw new InvalidDataException($"<extent:{directions}:{weights}:{controls}:{stream.Length}>"); }
        if (policy.QuantizationBits <= 0) { return (bytes, 0d); }
        byte[] normalized = stream.ToArray();
        Span<double> net = MemoryMarshal.Cast<byte, double>(normalized.AsSpan(netOffset));
        double step = GridStep<double>(net, policy);
        foreach (ref double coordinate in net) { coordinate = Quantization.Code(coordinate, step); }
        return (normalized, step);
    }

    static T GridStep<T>(ReadOnlySpan<T> source, DeltaPolicy policy) where T : IFloatingPointIeee754<T> {
        T bitStep = Quantization.Steps(source, policy.QuantizationBits).Step;
        T tolerance = T.CreateChecked(policy.Tolerance);
        return bitStep <= T.Zero ? tolerance : T.Min(bitStep, tolerance);
    }

    static Seq<DeltaChunk> FastCdc(ReadOnlySpan<byte> data, DeltaPolicy policy) {
        Seq<DeltaChunk> chunks = Seq<DeltaChunk>();
        int start = 0, ordinal = 0;
        while (start < data.Length) {
            int cut = ContentDefinedCut(data[start..], policy);
            ReadOnlySpan<byte> slice = data.Slice(start, cut);
            chunks = chunks.Add(new DeltaChunk(XxHash128.HashToUInt128(slice), ordinal++, start, cut));
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
            .Bind(addedByHash => delta.Kind.Normalize(baseBytes, delta.Policy).Bind(normalized => {
                ReadOnlyMemory<byte> normalizedBase = normalized.Bytes;
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
            }));

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

## [06]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` — one kernel `EncodedGeometry` arena read by descriptor, baked once at `Build`, leaves gathered lane-generically and re-minted as single-block identity pools; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf content-keyed metadata property table joining the IFC classification column and the solver field-value columns under one feature-id mapping, carrying its own `ReplayKey` so a tile is independently addressable and cache-replayable; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `LeafContent`/`TilesetExport` the manifest-plus-leaf-reference export carriers; `ExportTiles` the tileset-manifest emit fold that serializes the octree to tileset.json and enumerates the leaf-content references the manifest names, riding the content-key and the metadata layer, the leaf BODIES themselves the Bim glTF codec's cross-package product; the partition consumes the deflection/tolerance and tile-depth/error/split scalars from `TessellationPolicy` and the `InterchangeIdentity.Key`/`InterchangeIdentity.Compose` content-key, never the Bim format/codec/KHR surface.
- Entry: `public static Fin<TilesetExport> ExportTiles(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock, Op? key = null)` admits a census-consistent geometry, builds the octree, attaches the per-leaf metadata read at the node content-key, serializes the real tileset.json manifest, and enumerates the leaf-content references the manifest names — the leaf BODIES are the Bim glTF codec's product resolved at the content-key URIs, never emitted here; `public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock, Op? key = null)` bakes once and partitions the geometry into the depth-bounded octree; `Fin<T>` aborts on a census disagreement (`PayloadOverBounds` — a vertex-less mesh otherwise emits a manifest of float sentinel bounds), on a bake or per-leaf arena re-mint the kernel refuses, and on a tileset serialization miss projected onto `ComputeFault.ModelRejected`.
- Auto: `Build` bakes the pool through the seam's ONE `Bake(Op)` flatten, then partitions octant-by-octant to the policy max depth or triangle split threshold, geometric error the root error halved per depth, per-node content-key via the channel-generic `InterchangeIdentity.Key` over the arena so a re-partition of identical geometry keys identically AND a lane-roster growth re-keys, then reads the per-leaf `TileMetadata` at that content-key so one key addresses geometry and metadata. Every geometric read resolves BY DESCRIPTOR through the one `Lane` unpack — bounds and octant assignment stride on `EncodingChannel.Position.Arity`, and the per-leaf `Tessellate` gathers each declared lane by its own arity and re-mints the whole set through the kernel `Encode.Of` raw-lane entry, so a sliced leaf carries every lane its parent declared (UV and colour included) and a new channel row costs this partition nothing. `ExportTiles` serializes the tileset.json manifest (box bounding volumes, per-level geometric error, refine REPLACE, leaf content-key URIs) through the composition's one pooled stream — each node committing past the writer's own `BytesPending` watermark so the writer stays one commit wide and the manifest rides chained blocks with no migration — keys it off the pooled `ReadOnlySequence<byte>` through the incremental identity so a resident manifest materializes nothing, and flattens the octree to enumerate leaf-content references — the leaf BODIES (carrying `EXT_structural_metadata`/`EXT_mesh_features`) are the Bim glTF cross-package product against the Persistence index, never emitted here. `TileMetadata.Join` folds the `Rasm.Bim` IFC classification and the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` per-element field values read at the shared content-key into one feature-keyed property table under its own tile content-key; `TileMetadata.ReplayKey` composes that key with the causal stamp through `InterchangeIdentity.Compose` in (physical, logical) order so a tile's metadata replays from cache without rebuilding the octree; `PropertyTable.Pack` lays each `MetadataProperty` column as a contiguous buffer-view body; `FeatureBand.Of` classifies an achieved field value onto its styling band.
- Receipt: the `StreamSegment` receipt carries the leaf-reference count, the root geometric error, the max depth, the node count, and the per-leaf property-column count; emission rides the sink port.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Ext.3DTiles (the `Schema2.Tiles3D` EXT_structural_metadata/EXT_mesh_features leaf-body schema surface, admitted via `Tiles3DExtensions.RegisterExtensions()` once at composition — the settled Compute admission; models no tileset.json manifest tree), meshoptimizer, Microsoft.IO.RecyclableMemoryStream (the `RecyclableMemoryStream` the `Tensor/memory#STREAM_POOL` capsule grants — never a manager constructed here), LanguageExt.Core, NodaTime, Rasm (project — the kernel `EncodedGeometry` arena with `Encode.Of`, `Channel`, and `Descriptors`, the `EncodingChannel` lane roster, `ChannelDtype.Unpack`, `CorrelationId`, and `Op` the admission channel), Rasm.Element (project — the seam `ImportedGeometry`/`MeshBlock`/`MeshInstance` carrier), Rasm.Persistence (project), BCL inbox (`System.Text.Json` `Utf8JsonWriter` over the pooled stream with its `BytesPending` commit read, `System.Buffers` `ReadOnlySequence<byte>`)
- Growth: a new tile-partition parameter is one column on `TessellationPolicy` folded into the partition; a new per-vertex attribute is one kernel `EncodingChannel` row the partition reads, slices, and re-mints with ZERO edit here, because every geometric body addresses lanes by descriptor and none names a channel; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.
- Boundary: 3D-Tiles partition is the streamable-LOD octree over content-keyed geometry the compute lane owns — riding `InterchangeIdentity.Key` and the imported-geometry carrier — while the b3dm/glTF tile content encode is the Bim glTF codec the leaf emit composes; every geometry read here is CHANNEL-GENERIC over the seam carrier's one kernel arena — the descriptor set decides which lanes exist and at what storage width, and `Lane` is the single unpack; the metadata layer is one content-keyed schema column on the leaf-tile emit, never a parallel attribute store or second tiling owner, each `TileMetadata` carrying its own tile content-key (independently addressable) and `ReplayKey` composing it with the causal stamp so a leaf tile is cache-replayable without rebuilding the octree; the IFC classification reads the `Rasm.Bim` IFC semantic graph at the shared content-key (companion seam, never reaching into the Bim interior) and the per-element field values read the `Solver/discretization#DISCRETIZATION_MESH` `FieldSpace` achieved value (never a recomputed metric), so the IFC graph and the tessellated geometry stay two projections of one content-keyed IFC artifact joined at the tile boundary, a re-tessellation at a new deflection re-keying both together; `EXT_structural_metadata` property tables and the `EXT_mesh_features` feature-id ride the admitted `SharpGLTF.Ext.3DTiles` package, whose `Tiles3DExtensions.RegisterExtensions()` seats the types on Core's `ExtensionsFactory` and whose `UseStructuralMetadata`/`AddMeshFeatureIds` surface owns the property-table buffer-view layout through `PropertyTableProperty.SetValues<T>` — a hand-authored `JsonSerializable` extension class over the raw registration is the form `Rasm.Bim` `Exchange/export` already deletes, and Core's name-only `RegisterExtension<TParent,TExt>(string)` overload is `[Obsolete]` in favour of the factory-taking one (the material-PBR surface being the separate string-keyed `MaterialChannel` API in Core and `KnownChannel` enum in Toolkit); meshoptimizer owns the leaf-tile `Meshopt.Simplify`/`OptimizeVertexCache` LOD, never a hand-rolled simplifier; the manifest emit rides the `Tensor/memory#STREAM_POOL` capsule the composition already owns — a growable `ArrayBufferWriter` reaches a policy-depth manifest by doubling through the large-object heap, `GetBuffer` is the contiguity cliff, and `ToArray` the migration copy the pool's own posture bans, so all three are the deleted forms and a manager constructed at this boundary is the second-pool defect that owner forecloses; the leaf-tile content body is NOT emitted here — `ExportTiles` yields one typed `LeafContent` per leaf (content-key, `{contentKey:x32}.glb` URI, metadata-column count), the octree, metadata schema, and quantization-bit policy owned here while the b3dm/glTF body each URI names is the Bim tile-emit cross-package product against the Persistence index, a public leaf-body entry that can only decline the rejected honesty defect and a partition that re-derives the glTF body in-place or a metadata layer that re-reads the IFC parser the rejected form.

```csharp signature
// Compute-lane geometry-quality + tile-partition policy: every output-affecting quality and partition column
// salts the owning compute content key.
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
    // Bake FIRST: the octant walk addresses world-space triangles, so an instanced carrier flattens through the
    // one seam Bake owner and a non-instanced carrier passes through unchanged (its pool IS its scene). Bake
    // re-mints an arena, so it rails — the whole partition rides that rail rather than swallowing a mint refusal.
    public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock, Op? key = null) {
        Op k = key.OrDefault();
        return geometry.Bake(k)
            .Bind(baked => Partition(baked, metadata, policy, depth: 0, k))
            .Map(root => new TileSet(root, policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(root), clock.GetCurrentInstant()));
    }

    static Fin<TileNode> Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, int depth, Op key) {
        float[] positions = Lane(geometry.Lanes, EncodingChannel.Position);
        float[] bounds = Bounds(positions);
        double error = policy.TileGeometricErrorRoot / Math.Pow(2, depth);
        UInt128 contentKey = InterchangeIdentity.Key(
            geometry.FormatKey,
            geometry.Lanes,
            MemoryMarshal.AsBytes(geometry.Indices.AsSpan()), [
            policy.Deflection,
            policy.Tolerance,
            policy.AngleTolerance,
            policy.TileMaxDepth,
            policy.TileGeometricErrorRoot,
            policy.TileSplitThreshold,
        ]);
        return depth >= policy.TileMaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? Fin.Succ(new TileNode(depth, bounds, error, contentKey, metadata(contentKey), Seq<TileNode>()))
            : Split(geometry, positions, bounds, key)
                .Bind(leaves => leaves.TraverseM(leaf => Partition(leaf, metadata, policy, depth + 1, key)).As())
                .Map(children => new TileNode(depth, bounds, error, contentKey, None, children));
    }

    static int Count(TileNode node) => 1 + node.Children.Sum(Count);

    // ONE descriptor-addressed lane reader serves the whole partition: the descriptor names the dtype, so Unpack
    // lifts a unorm8 colour and a float32 position through the same call and no arm re-spells a storage width.
    // Absent channels answer the empty array — a MISSING DESCRIPTOR, never a zero-filled buffer a consumer
    // length-probes — which is what keeps every reader below channel-generic and every roster growth free.
    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(d => d.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

    // Strides on the Position row's OWN declared arity, so a channel-roster edit never leaves a literal 3 behind.
    static float[] Bounds(ReadOnlySpan<float> positions) {
        int arity = EncodingChannel.Position.Arity;
        (float minX, float minY, float minZ) = (float.MaxValue, float.MaxValue, float.MaxValue);
        (float maxX, float maxY, float maxZ) = (float.MinValue, float.MinValue, float.MinValue);
        for (int offset = 0; offset + arity - 1 < positions.Length; offset += arity) {
            (minX, minY, minZ) = (Math.Min(minX, positions[offset]), Math.Min(minY, positions[offset + 1]), Math.Min(minZ, positions[offset + 2]));
            (maxX, maxY, maxZ) = (Math.Max(maxX, positions[offset]), Math.Max(maxY, positions[offset + 1]), Math.Max(maxZ, positions[offset + 2]));
        }
        return [(minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2, (maxX - minX) / 2, 0, 0, 0, (maxY - minY) / 2, 0, 0, 0, (maxZ - minZ) / 2];
    }

    // Triangle corners resolve through ImportedGeometry.Indices, so indexed shared-vertex meshes partition
    // real corners; triangle-soup ordinal addressing mis-partitions non-identity index buffers.
    static Fin<Seq<ImportedGeometry>> Split(ImportedGeometry geometry, float[] positions, float[] bounds, Op key) {
        (float cx, float cy, float cz) = (bounds[0], bounds[1], bounds[2]);
        return toSeq(Range(0, geometry.TriangleCount)
                .GroupBy(tri => Octant(positions, geometry.Indices.AsSpan(), tri, cx, cy, cz))
                .Select(static group => toSeq(group)))
            .TraverseM(group => Tessellate(geometry, group, key)).As();
    }

    static int Octant(ReadOnlySpan<float> positions, ReadOnlySpan<long> indices, int triangle, float cx, float cy, float cz) {
        int v = (int)indices[triangle * 3] * EncodingChannel.Position.Arity;
        return (positions[v] >= cx ? 1 : 0) | (positions[v + 1] >= cy ? 2 : 0) | (positions[v + 2] >= cz ? 4 : 0);
    }

    // Channel-generic corner gather: every DECLARED lane unpacks once, gathers by its OWN arity, and the whole
    // set re-mints through the one kernel raw-lane entry — so a new EncodingChannel row reaches a sliced leaf
    // with zero edit here, where the retired per-column body grew one `if (!srcT.IsEmpty)` rung and one `with`
    // slot per attribute. Descriptor presence IS the absence test; a buffer length probe is the deleted form.
    static Fin<ImportedGeometry> Tessellate(ImportedGeometry geometry, Seq<int> triangles, Op key) {
        ReadOnlySpan<long> srcI = geometry.Indices.AsSpan();
        int vertices = triangles.Count * 3;
        (EncodingChannel Channel, float[] Source, float[] Gathered)[] lanes = [.. geometry.Lanes.Descriptors.Map(d =>
            (d.Channel, Lane(geometry.Lanes, d.Channel), new float[vertices * d.Channel.Arity]))];
        long[] indices = new long[vertices];
        int slot = 0;
        foreach (int tri in triangles) {                                   // Exemption: a measured gather over pre-sized arenas; the rail resumes at the Encode.Of mint
            for (int corner = 0; corner < 3; corner++) {
                int source = (int)srcI[tri * 3 + corner];
                foreach (var lane in lanes) {
                    int arity = lane.Channel.Arity;
                    lane.Source.AsSpan(source * arity, arity).CopyTo(lane.Gathered.AsSpan((slot * 3 + corner) * arity, arity));
                }
            }
            (indices[slot * 3], indices[slot * 3 + 1], indices[slot * 3 + 2]) = (slot * 3, slot * 3 + 1, slot * 3 + 2);
            slot++;
        }
        // Sliced leaves re-describe their pool honestly — one block spanning the leaf, one identity instance,
        // holding the baked-carrier invariant every downstream flat read (payload arms, bounds, splits) reads.
        return Encode.Of(vertices, toSeq(lanes).Map(static lane => (lane.Channel, lane.Gathered)), key)
            .Map(arena => geometry with {
                Lanes = arena, Indices = indices,
                VertexCount = vertices, TriangleCount = triangles.Count,
                Blocks = Seq(new MeshBlock(0, vertices, 0, vertices)),
                Instances = Seq(new MeshInstance(0, Matrix4x4.Identity)),
            });
    }
}

// Tileset EXPORT the partition owns: the real tileset.json Manifest (this page's product) and the LeafContent
// reference set it names — the typed handoff to the Bim glTF leaf codec, resolved against the Persistence index, never a body here.
public sealed record LeafContent(UInt128 ContentKey, string Uri, int MetadataColumns);

public sealed record TilesetExport(ComputeArtifact Manifest, Seq<LeafContent> Leaves);

public static class TilePartition {
    // Writer commit bound: an octree at the policy depth reaches six figures of nodes, so the emit commits each
    // completed node once the writer's own pending count crosses this width. The writer's internal buffer then
    // stays one commit wide regardless of depth while the pooled stream carries the manifest in chained blocks —
    // an unflushed writer buffers the WHOLE manifest, which is the growth this bound exists to delete.
    const int CommitWatermark = 64 * 1024;

    // Emits this page's OWNED product — the tileset.json manifest over the octree — and the LeafContent references
    // it names; the leaf BODIES (b3dm/glTF carrying EXT_structural_metadata/EXT_mesh_features) are the Bim cross-package product, never here.
    // Arena claim sets already prove descriptor tiling, per-lane recovery inside dtype tolerance, and payload
    // extent, so this gate re-validates none of it. What stays is the CENSUS the two shapes disagree on — the
    // carrier's vertex count against the arena's own element count, the index column against the triangle
    // count, and every index inside the vertex range.
    public static Fin<TilesetExport> ExportTiles(
        StreamPool pool, CorrelationId correlation, ImportedGeometry geometry,
        Func<UInt128, Option<TileMetadata>> metadata, TessellationPolicy policy, IClock clock, Op? key = null) =>
        geometry.VertexCount <= 0 || geometry.TriangleCount <= 0
            || geometry.Lanes.Count != geometry.VertexCount
            || geometry.Indices.Length < geometry.TriangleCount * 3
            || IndexOutOfRange(geometry)
            ? Fin.Fail<TilesetExport>(new ComputeFault.PayloadOverBounds($"<tileset-geometry:{geometry.VertexCount}:{geometry.TriangleCount}:{geometry.Lanes.Count}:{geometry.Indices.Length}>"))
            : TileSet.Build(geometry, metadata, policy, clock, key)
                .Bind(tiles => Tileset(pool, correlation, tiles, policy, clock)
                    .Map(manifest => new TilesetExport(manifest, Leaves(tiles.Root))));

    static bool IndexOutOfRange(ImportedGeometry geometry) {
        ReadOnlySpan<long> indices = geometry.Indices.AsSpan()[..(geometry.TriangleCount * 3)];
        foreach (long index in indices) { if (index < 0 || index >= geometry.VertexCount) { return true; } }
        return false;
    }

    // tileset.json: refine REPLACE, box bounding volumes off each node's Aabb, geometricError halving per level,
    // leaf content URIs {contentKey:x32}.glb the AppUi/web consumer resolves against the Persistence index. The
    // emit STREAMS through the composition's one `Tensor/memory#STREAM_POOL` capsule rather than an
    // ArrayBufferWriter: a manifest at the policy depth is tens of megabytes, and a growable writer reaches it by
    // doubling through the large-object heap while the pool holds chained blocks and never migrates. `GetBuffer`
    // (the contiguity cliff) and `ToArray` (the migration copy the pool's own ThrowExceptionOnToArray posture
    // bans) never appear — the sequence itself keys the artifact, and the carrier's own mint owns the one copy.
    static Fin<ComputeArtifact> Tileset(StreamPool pool, CorrelationId correlation, TileSet tiles, TessellationPolicy policy, IClock clock) =>
        pool.Get(correlation, new StreamGrant.Open())
            .Bind(staged => Try.lift(() => Manifested(staged, tiles.Root, policy, clock)).Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<tileset-emit:{error.Message}>")));

    // Exemption: the writer-and-stream disposal bracket is the platform-forced statement seam this codec boundary
    // owns; the rail resumes on the returned carrier.
    static ComputeArtifact Manifested(RecyclableMemoryStream staged, TileNode root, TessellationPolicy policy, IClock clock) {
        using (staged) {
            using (Utf8JsonWriter writer = new(staged)) {
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
            return ComputeArtifact.Of("tileset.json", staged.GetReadOnlySequence(), clock.GetCurrentInstant(), [
                policy.Deflection,
                policy.Tolerance,
                policy.AngleTolerance,
                policy.TileMaxDepth,
                policy.TileGeometricErrorRoot,
                policy.TileSplitThreshold,
            ]);
        }
    }

    // One LeafContent per octree leaf — content-key, {contentKey:x32}.glb URI, metadata-column count — the typed
    // handoff to the Bim leaf-content producer, whose bodies land on the Persistence blob lane under these keys.
    static Seq<LeafContent> Leaves(TileNode root) =>
        Flatten(root)
            .Filter(static node => node.Children.IsEmpty)
            .Map(static node => new LeafContent(node.ContentKey, $"{node.ContentKey:x32}.glb", node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)));

    // Real tileset.json serialization over the octree: a glTF-independent JSON manifest (asset 1.1, root
    // geometricError, recursive tile tree — box boundingVolume, per-node geometricError, refine REPLACE, leaf
    // content URI). SharpGLTF.Ext.3DTiles owns the glTF-embedded EXT_structural_metadata/EXT_mesh_features of the
    // leaf BODIES (Bim's codec), never this manifest tree, so it emits through the BCL Utf8JsonWriter. Each node
    // commits past the watermark, so a deep subtree never accumulates in the writer ahead of the pooled stream.
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
        if (writer.BytesPending >= CommitWatermark) { writer.Flush(); }
    }

    static Seq<TileNode> Flatten(TileNode node) =>
        node.Cons(node.Children.Bind(Flatten));
}
```

## [07]-[CONTENT_ADDRESSING]

- Owner: `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `CanonicalForm` the static byte-normalization kernel reducing every keyed input to one machine-independent canonical byte form before the hash seed; `InterchangeIdentity` the interchange CACHE-PARTITION key derivation folding canonicalized source bytes with the complete ordered output-policy vector into one policy-seeded `XxHash128` identity (distinct from the kernel seed-zero `GeometryHash` the seam/Bim/Persistence/peers share), mirroring the model-lane `ModelIdentity.Snapshot` precedent, with `Compose` sealing the content key and HLC two-half stamp into one frame key and `SeedZero` minting the empty-artifact sentinel; `ComputeArtifact` the emitted-bytes carrier the field, tile, and Bim export rails feed, landing content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(...)` — pure value; the contiguous and pooled-sequence cases derive identity from canonical bytes and the complete ordered policy vector, while the geometry case frames the kernel arena's own witness digest WHOLE — the `DigestRoot` key byte ahead of `ContentHash`, the kernel `RoundTripWitness.Root` dedup law read into the preimage — beside its descriptor roster and the index column by ordinal and byte length before one incremental hash; `public static UInt128 Compose(UInt128 contentKey, Instant physical, ulong logical)` folds the content key with the causal stamp in the fixed (physical, logical) half order; `public static UInt128 SeedZero(string formatKey, ReadOnlySpan<double> policy)` is the empty-artifact sentinel identity; `ComputeArtifact.Of` is the one emit-carrier mint deriving the content key from bytes with its complete policy vector, discriminating on the payload's own shape — a contiguous `ReadOnlyMemory<byte>` or a pooled `ReadOnlySequence<byte>` whose key folds incrementally before any contiguity is demanded — and `ByteCount` derives off the carried payload rather than travelling as a second stored column a caller contradicts.
- Auto: every keyed input passes through `CanonicalForm` before the seed — `CanonicalForm.Tag` lower-cases invariant culture and trims the format/codec tag so `"GLB"` and `" glb "` key one identity, `CanonicalForm.Scalar` collapses negative zero to positive zero and maps every NaN pattern to one quiet-NaN payload, and `CanonicalForm.Write` lays the length-prefixed tag, the policy scalar count, and every ordered policy scalar little-endian — injective framing, so distinct `(formatKey, policy)` tuples never share a canonical byte vector; artifact bytes pass into the byte hash verbatim. `Key` seeds `XxHash128.HashToUInt128` with the `XxHash3.HashToUInt64` of that canonical vector, so tessellation folds deflection, tolerance, angle tolerance, tile depth, root geometric error, and split threshold while field residence folds bits and bound. Zero-length artifact bytes route to `SeedZero` over the same policy vector, so absent and present-but-empty stay distinct. `Admit` projects onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns.
- Receipt: the `Cache` receipt carries the content-key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt; a sentinel-keyed empty artifact stamps the `SeedZero` identity so an absent-versus-empty distinction is auditable.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm (project — the kernel `EncodedGeometry` arena the geometry key frames), Rasm.Persistence (project), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one canonical-scalar column folded into the seed; a new keyed-input kind is one `CanonicalForm` arm; a new per-vertex lane is a kernel `EncodingChannel` row the geometry key absorbs through the descriptor roster with no edit here; zero new surface.
- Boundary: interchange-cache identity is `XxHash128` over the canonical source bytes — the suite hash law the `Runtime/transport#ARTIFACT_FRAMES` whole-artifact identity and the model-lane `ModelIdentity` checksum hold, never a second hashing pass and never a path-keyed identity; canonical-form normalization is the cross-machine reproducibility floor — case-folded trimmed tag, little-endian policy scalars, negative-zero collapsed to positive zero, every NaN payload mapped to one quiet NaN — so two semantically-equal source artifacts on osx-arm64, linux-x64, and win-x64 cache-key one identity (the `lang:python:runtime/evidence/identity#IDENTITY` `ContentIdentity` folds the same format/deflection/tolerance, the cross-runtime peer), a raw-string-interpolated seed (`$"{formatKey}|{deflection:R}|..."`) the rejected drift defect keying distinctly across cultures and float renderings; the SHARED geometry WIRE hash is a DISTINCT key — the GLB geometry-content identity the seam `Rasm.Element/Graph/element#NODE_MODEL` `RepresentationContentHash`, the Persistence `Store/blobstore#OBJECT_STORE` blob name, and the `lang:typescript:core/interchange/frame#GEOMETRY_PLANE` + `lang:typescript:data/object/store` `ObjectKey` peers reproduce is the KERNEL seed-zero (`seed=0`) `XxHash128` `GeometryHash` over the canonical bytes (`tests/contracts/MANIFEST.md` `MESH_ADJACENCY_GOLDEN` the golden vector anchoring C#/Python/TypeScript byte-parity), composed here and never re-minted with a policy seed — a policy-seeded GLB geometry-content hash the named cross-runtime defect, the two keys coexisting by design; the empty-artifact `SeedZero` sentinel is the absent-versus-empty law (policy-seeded empty case, distinct from the kernel `seed=0`) — empty bytes key to `SeedZero` over the policy alone, never the byte hash of an empty span, so a cache key never collides absent against present-but-empty; the HLC compose order seals the kernel `Rasm/Domain/telemetry#CAUSAL_FRAME` `ReceiptSinkPort.Advance` stamp byte-identical — physical half first as the `Instant` Unix-tick `long`, logical half second as the monotone `ulong`, both little-endian, the layout `tests/contracts/MANIFEST.md` `HLC_TWO_HALF` freezes across the three runtimes — so `Compose` re-derives no ordering the capsule already fixed, a logical-half-first composition the named defect folding a fresh op as stale; the key takes a format-key string rather than the Bim `InterchangeFormat` owner so the content identity stays a Compute concern decoupled from the moved format axis; every output-affecting scalar folds in owner order, so deflection, tolerance, angle tolerance, tile depth, root error, or split-threshold movement partitions a tileset key and prevents cross-setting hits; addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` under the content-key string `Path`, so the IFC semantic graph (Bim), the tessellated GLB, the field artifact, and a re-exported glTF are rows under the ONE kernel seed-zero `XxHash128` residence identity the Persistence index re-derives (`ArtifactIndexRow.Admit` -> `ContentAddress.Of`) — Compute owning only the policy-seeded cache-key derivation (the logical label), the kernel/seam the seed-zero residence identity, Persistence the blob residence, none re-declaring another; the export-rail field/tile/re-exported-glTF artifacts self-key (their `SourceKey` their own `ContentHash`, single-projection) while the tessellated GLB and the IFC-semantic graph of one source IFC share one cross-projection `sourceKey` — the kernel seed-zero `SourceKey` the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` mints purely over the source bytes (tolerance-independent, so the in-process semantic-graph ingest re-derives it without the deflection), NOT the policy-seeded cache key — so the Persistence `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow.Project` returns the two-projection family under that kernel-seed-zero key, the `Option<UInt128> sourceKey` admission carrying the pure key and each row's blob residence the kernel seed-zero `ContentAddress.Of` (`ArtifactIndexRow.Admit`), never a GLB self-key off the policy-seeded partition stranding the geometry projection off the semantic one; a managed copy of the artifact bytes beside the blob lane is the rejected form.

```csharp signature
// String-keyed compute-lane emit carrier: the format tag is a bare key here, so the content identity stays
// decoupled from the Bim format axis a codec-rowed carrier would bind it to.
public sealed record ComputeArtifact(
    string FormatKey,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    Instant At) {
    public long ByteCount => Bytes.Length;

    public static ComputeArtifact Of(string formatKey, ReadOnlyMemory<byte> bytes, Instant at, ReadOnlySpan<double> policy = default) =>
        new(formatKey, bytes, InterchangeIdentity.Key(formatKey, bytes.Span, policy), at);

    // Segmented mint for a pooled emit: the key folds the multi-segment sequence INCREMENTALLY, so a producer
    // whose key already resides never materializes a byte, and the miss path pays ONE exact-extent copy where a
    // growable writer paid a doubling ladder through the large-object heap. Arity is the input's own shape —
    // contiguous or segmented — never a mode flag beside the value.
    public static ComputeArtifact Of(string formatKey, ReadOnlySequence<byte> bytes, Instant at, ReadOnlySpan<double> policy = default) {
        byte[] owned = new byte[checked((int)bytes.Length)];
        bytes.CopyTo(owned);
        return new(formatKey, owned, InterchangeIdentity.Key(formatKey, bytes, policy), at);
    }
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

    // Channel-generic geometry identity over the ONE kernel arena, replacing the retired vertices/indices/normals
    // triple that silently EXCLUDED every lane it did not name — UV and colour among them — so a roster growth
    // moved no key and two leaves differing only in their UV unwrap collided. Three ordinal-framed components
    // seal it: the witness composite WHOLE — one DigestRoot key byte ahead of ContentHash, per the kernel
    // RoundTripWitness.Root law that a dedup or lake-identity consumer reads the root beside the digest, so a
    // source-rooted Apply witness and a payload-rooted Of witness share no preimage even where their bytes
    // coincide, never a digest-only fold leaning on preimage-domain disjointness — descriptor roster (WHICH
    // channels at WHICH storage width and element count produced them), and Indices (the one non-channel column).
    // New EncodingChannel rows therefore re-key by construction and are named nowhere here.
    public static UInt128 Key(string formatKey, EncodedGeometry lanes, ReadOnlySpan<byte> indices, ReadOnlySpan<double> policy) {
        if (lanes.Descriptors.IsEmpty && indices.IsEmpty) { return SeedZero(formatKey, policy); }
        Span<byte> digest = stackalloc byte[17];
        digest[0] = (byte)lanes.Witness.Root.Key;
        BinaryPrimitives.WriteUInt128BigEndian(digest[1..], lanes.Witness.ContentHash.Value);
        XxHash128 hasher = new(CanonicalForm.Seed(formatKey, policy));
        AppendComponent(hasher, 0, digest);
        AppendComponent(hasher, 1, Encoding.UTF8.GetBytes(Roster(lanes)));
        AppendComponent(hasher, 2, indices);
        return hasher.GetCurrentHashAsUInt128();
    }

    // Descriptor roster in the arena's own tiling order — channel key, storage dtype row, element count per lane —
    // so two arenas whose payload bytes coincide under different lane declarations never key alike.
    static string Roster(EncodedGeometry lanes) =>
        string.Join(';', lanes.Descriptors.Map(static d => string.Create(
            CultureInfo.InvariantCulture, $"{d.Channel.Key}:{d.Dtype.Key}:{d.Count}")));

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
        BinaryPrimitives.WriteUInt64LittleEndian(frame, ContentHash.Half(contentKey, 0));
        BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], ContentHash.Half(contentKey, 1));
        BinaryPrimitives.WriteInt64LittleEndian(frame[16..], physical.ToUnixTimeTicks());
        BinaryPrimitives.WriteUInt64LittleEndian(frame[24..], logical);
        return XxHash128.HashToUInt128(frame);
    }

    public static ArtifactIndexRow Admit(ComputeArtifact artifact, DataClassification classification, Option<UInt128> sourceKey) =>
        ArtifactIndexRow.Admit(ArtifactKind.Interchange, $"{artifact.ContentKey:x32}:{CanonicalForm.Tag(artifact.FormatKey)}", artifact.Bytes.Span, classification, artifact.At, sourceKey);
}
```

## [08]-[ARROW_BATCH]

- Owner: `ArrowBatch` — the one columnar-construction owner projecting the `Solver/sweep` `DoeDataset`, the `Runtime/receipts` `ChargebackDataset`, and the `GeometryDataset` kernel-encode corpus into self-describing `Apache.Arrow` batches; `Doe`/`Chargeback`/`Geometry` the three producers, `Strided`/`Doubles` the row-major column folds and `Lanes`/`ArenaLane` the quantization-row wrap the geometry arm borrows arena memory through. `GeometryDataset` — the lake-bound corpus pairing one `PackKind` with its model segment and encoded instances, deriving both its schema identity and its generation key from content. Core `Apache.Arrow` is the sole reference: the IPC writer, the LZ4/Zstd `CompressionCodecFactory`, the ADBC query surface, and the Flight-SQL transport are the Persistence `api-arrow` overlay's egress rails, absent from the Compute closure.
- Entry: `public static Fin<(LakeGeneration Generation, Schema Schema, Seq<RecordBatch> Batches)> Landing(…)` overloads on the dataset shape and hands the custodian its `FlatTableEgress.Land` triple; `public static Fin<RecordBatch> Doe(DoeDataset dataset, MemoryAllocator? allocator = null)` admits checked row-major dimensions and a non-empty, ordinal-unique field vocabulary excluding `on_front`, then projects the `Coordinates`/`Responses` blocks into one `DoubleArray` column per axis and per objective and the `OnFront` mask into one `BooleanArray` column, `ContentKey`/`Strategy`/`At`/`points` riding `Schema.Builder.Metadata`; `public static Fin<RecordBatch> Chargeback(ChargebackDataset dataset, MemoryAllocator? allocator = null)` folds the tenant-partitioned rows into `tenant`/`route` `StringArray`, four `CostVector` `DoubleArray`, and a `facts` `Int64Array` column, window bounds and content-key as metadata; `public static Fin<(Schema Schema, Seq<RecordBatch> Batches)> Geometry(GeometryDataset dataset, MemoryAllocator? allocator = null)` admits a non-empty corpus whose every quantization row has a lane and whose every instance the kernel's own `PackSchema.Describes` accepts, then emits ONE batch per instance whose channel columns wrap the arena verbatim beside the `source`/`ordinal` join pair, `content_key`/`schema_id`/`kind`/`instances` riding `Schema.Builder.Metadata`.
- Auto: each column bulk-appends one span — `OnFront` drives `BooleanArray.Builder.Append(onFront.Span)`, which copies the span once into the allocator-owned BooleanArray buffer, and each axis/objective column drives one `DoubleArray.Builder.Append(ReadOnlySpan<double>)` after a single row-major→columnar strided gather pre-sized by `Reserve(points)`, never a per-element `Append(T)` loop; the `Schema` field order and the batch column order are the one append sequence so the reader recovers columns positionally, and `ContentKey` rides `Schema` metadata so a batch whose metadata omits the content key is the drift defect; the geometry arm gathers NOTHING — the kernel tiled each channel contiguously at its own descriptor offset, so an `ArrowBuffer` borrows that slice and a `FixedSizeListType` of the channel's arity states the interleave already in the bytes, leaving the two identity columns as the only material a landing allocates.
- Receipt: none new — each batch is a projection of a standing dataset shape, and the landed generation's evidence rides the custodian's own `store.doe.land`/`store.cost.land`/`store.geometry.land` slots, never a second Compute row; the geometry corpus carries the kernel `RoundTripWitness` per instance, so quantization evidence is already proved upstream and no landing re-measures it.
- Packages: Apache.Arrow, NodaTime (`InstantPattern.ExtendedIso` the metadata instant, `LocalDatePattern.CreateWithInvariantCulture` over `Instant.InUtc().Date` the billing-month segment), Thinktecture.Runtime.Extensions (`DoeDesign`/`Substrate` `.Key`), System.IO.Hashing (`XxHash128` the schema-identity digest), Rasm.Persistence (project — `LandingArm`/`LakeGeneration`/`Identifier`), Rasm (project — `TenantContext`, `ContentHash.Of`, and the `Rasm.Drawing` encode wire `EncodedGeometry`/`PackKind`/`PackSchema`/`EncodingChannel`/`ChannelDtype`), LanguageExt.Core, CommunityToolkit.HighPerformance (`SpanOwner<byte>` the generation-preimage rent), BCL inbox (`CultureInfo.InvariantCulture`, `FrozenDictionary`, `BinaryPrimitives.WriteUInt128BigEndian`)
- Growth: a new dataset producer is one `ArrowBatch` method reusing the shared column folds beside one `Landing` overload naming its arm row, never a per-dataset columnar encoder; a per-row-instant lake producer (the receipt-journal egress) adds one `TimestampArray` column under `TimestampType.Default`, the NodaTime clock seam the metadata instant already shares; a new column is one `Field` and its bulk-span fold; a new kernel `ChannelDtype` is one `Lanes` row carrying its Arrow type and arena wrap, and a new `EncodingChannel` or `PackKind` reaches the geometry columns with ZERO edit here because the kind's own declared active set generates the schema; the `MemoryAllocator` injects per lane so a staging-bounded arena charges the batch buffers against the lane budget rather than the shared `MemoryAllocator.Default` fallback.
- Boundary: Compute BUILDS the columnar table; the Persistence `api-arrow` overlay OWNS everything that CARRIES it — `ArrowStreamWriter`/`ArrowFileWriter` IPC, the `Apache.Arrow.Compression.CompressionCodecFactory` LZ4/Zstd codec, the ADBC query surface, and the `FlightClient`/`FlightSqlClient` — so Compute holds one core `Apache.Arrow` reference, references none of the four egress packages, and opens no Flight listener; the row-major→columnar transpose is the one unavoidable gather (a `Reserve`+`Append(span)` per column, never a per-element builder loop); a bare `DateTime` where the NodaTime instant crosses, the shared `MemoryAllocator.Default` where a lane arena is available, a schema field order diverging from the column order, or a hand-rolled columnar byte layout `RecordBatch` already owns are the rejected forms; the geometry arm adds three of its own — a per-component scalar fan-out of an arity-3 channel, which re-keys the tree on every arity edit and reinstates the strided copy the kernel's tiling deletes; a half or unorm lane widened to float at the wrap, which re-spells values the round-trip witness certified at their stored width; and a schema key re-digested off the Arrow field list, which keys the hive tree on a projection the kernel never published while `PackSchema.SchemaId` is the identity the custodian's geometry row names by law; the sealed `RecordBatch` stops at the Compute edge — `Landing` hands the custodian a `LakeGeneration` coordinate and `FlatTableEgress.Land` writes it, so byte framing exists only where the `topology` axis puts the custodian in another process and the composition root frames it there through the Persistence overlay's IPC writer, never here, and the opaque `GeoArrowRequest.ArrowIpc` relay bytes are never decoded or re-encoded.

```csharp signature
// GeometryDataset carries one lake-bound corpus: one PackKind, one model segment, and the encoded instances
// sharing that kind's declared channel set. It homes HERE and not at a producing page because it has no life
// outside this landing — a corpus assembled only to cross the columnar seam is the landing owner's noun, where
// DoeDataset and ChargebackDataset each answer a question of their own before any batch exists. Schema identity and generation
// identity both DERIVE, so neither is forgeable at a call site and a retry re-lands the same bytes under the same
// key; the encode instant is deliberately absent, since a wall-clock stamp would re-key an unchanged corpus.
public sealed record GeometryDataset(PackKind Kind, string Model, Seq<EncodedGeometry> Instances) {
    public PackSchema Schema => PackSchema.Of(Kind);

    // Preimage seats the schema identity ahead of every instance record in landed order: order IS content because
    // its row ordinal joins a scan back to the encode, and the schema is identity-bearing because it decides which
    // columns each generation carries. Each instance record is the witness composite WHOLE — one DigestRoot key
    // byte ahead of Witness.ContentHash — so a source-rooted Apply mint and a payload-rooted Of mint of ONE
    // geometry key distinct generations structurally, the preimage reading Root beside the digest exactly as the
    // kernel RoundTripWitness.Root dedup law demands, never a digest-only fold leaning on preimage-domain
    // disjointness. Seventeen-byte fixed-width records concatenate injectively, so the spine needs
    // no length framing; big-endian is the estate's one persisted spelling where `MemoryMarshal` writes host order.
    // Corpus size is unbounded, so the spine rents rather than `stackalloc`s, and the write loop is a fold no rail
    // combinator can carry — a `Span` cannot be captured by any lambda, which is the named statement seam here.
    public UInt128 ContentKey {
        get {
            using SpanOwner<byte> rent = SpanOwner<byte>.Allocate(checked(16 + (Instances.Count * 17)));
            Span<byte> spine = rent.Span;
            BinaryPrimitives.WriteUInt128BigEndian(spine, Schema.SchemaId);
            int offset = 16;
            foreach (EncodedGeometry instance in Instances) {
                spine[offset] = (byte)instance.Witness.Root.Key;
                BinaryPrimitives.WriteUInt128BigEndian(spine[(offset + 1)..], instance.Witness.ContentHash.Value);
                offset += 17;
            }
            return ContentHash.Of(spine);   // every byte overwritten, so the pooled rent needs no clear
        }
    }
}

// One physical lane per kernel quantization row: the Arrow type the column declares beside the wrap that borrows
// an arena slice at that width. The wrap is a delegate column rather than a generic `new()` bound because the
// builder families are foreign sealed types with no shared constructible contract, and a constructed-generic
// factory over them lowers to the activator form this stack rejects. No span enters or leaves the delegate — the
// buffer is `ReadOnlyMemory`-backed, so nothing stack-only crosses a lambda seam.
sealed record ArenaLane(IArrowType Type, Func<ArrowBuffer, int, IArrowArray> Wrap);

// One Arrow construction owner, three dataset producers (Solver/sweep DoeDataset, Runtime/receipts
// ChargebackDataset, and the GeometryDataset above)
// — never a per-dataset bespoke columnar encoder. Compute BUILDS the columnar table; the Persistence api-arrow
// overlay OWNS everything that carries it (IPC writer, LZ4/Zstd codec, ADBC, Flight-SQL). Every builder takes a
// per-lane MemoryAllocator; a null allocator falls back to the process-global MemoryAllocator.Default, so a
// staging-bounded lane charges its own arena.
public static class ArrowBatch {
    // Quantization rows map to Arrow at their STORED width, so a landed generation carries exactly the bits the
    // kernel's round-trip witness certified. A new kernel ChannelDtype row is one entry here and the geometry
    // admission gate refuses the whole corpus by schema tag until it lands.
    static readonly FrozenDictionary<ChannelDtype, ArenaLane> Lanes =
        new Dictionary<ChannelDtype, ArenaLane> {
            [ChannelDtype.Float32] = new(FloatType.Default, static (buffer, length) => new FloatArray(buffer, ArrowBuffer.Empty, length, 0, 0)),
            [ChannelDtype.Float16] = new(HalfFloatType.Default, static (buffer, length) => new HalfFloatArray(buffer, ArrowBuffer.Empty, length, 0, 0)),
            [ChannelDtype.Unorm8] = new(UInt8Type.Default, static (buffer, length) => new UInt8Array(buffer, ArrowBuffer.Empty, length, 0, 0)),
        }.ToFrozenDictionary();

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
        Seq<(Field Field, IArrowArray Array)> columns = axisCols + objectiveCols + Seq(frontCol);
        Schema schema = columns.Fold(new Schema.Builder(), static (builder, column) => builder.Field(column.Field))
            .Metadata("content_key", $"{dataset.ContentKey:x32}")
            .Metadata("strategy", dataset.Strategy.Key)
            .Metadata("at", InstantPattern.ExtendedIso.Format(dataset.At))
            .Metadata("points", rows.ToString(CultureInfo.InvariantCulture))
            .Build();
        return Fin.Succ(new RecordBatch(schema, columns.Map(static column => column.Array), rows));
    }

    // Geometry egress is the ZERO-GATHER producer: the kernel already tiled each channel contiguously at its own
    // descriptor offset, so an ArrowBuffer wraps that slice verbatim and a FixedSizeList of the channel's arity
    // states the interleave the arena already carries. A per-component scalar fan-out would re-key the tree on
    // every arity edit AND force the strided copy the kernel's tiling exists to delete. Row grain is the packed
    // ELEMENT — vertex, cell, or path station — so a batch is one instance and `source`/`ordinal` carry the join
    // back to the encode. Both identity columns are wide in memory and near-free on disk (dictionary and RLE
    // encoding), which is the deliberate trade for channel columns that cost no copy at all.
    public static Fin<(Schema Schema, Seq<RecordBatch> Batches)> Geometry(
        GeometryDataset dataset, MemoryAllocator? allocator = null) {
        Seq<EncodingChannel> channels = dataset.Kind.Channels;
        PackSchema schema = dataset.Schema;
        // Admission runs WHOLE before the first buffer is wrapped, so a refusal frees nothing and the build below
        // is total — a rail faulting mid-construction abandons every column already allocated and `Fin` carries no
        // release arm. `Describes` is the KERNEL's own oracle over declaration versus packed instance, so this
        // producer proves conformance with the encoder's law instead of re-deriving a second descriptor check.
        if (dataset.Instances.IsEmpty) { return Fin.Fail<(Schema, Seq<RecordBatch>)>(ComputeFault.Create("<arrow-geometry-empty>")); }
        if (!channels.ForAll(static channel => Lanes.ContainsKey(channel.Dtype))) {
            return Fin.Fail<(Schema, Seq<RecordBatch>)>(ComputeFault.Create($"<arrow-geometry-dtype:{schema.Tag}>"));
        }
        return dataset.Instances
            .TraverseM(instance => schema.Describes(instance).MapFail(_ => (Error)ComputeFault.Create(
                $"<arrow-geometry-schema:{instance.Witness.ContentHash.Value:x32}>")))
            .As()
            .Map(_ => {
                Schema wire = channels.Fold(new Schema.Builder(), static (builder, channel) => builder.Field(Column(channel)))
                    .Field(new Field("source", StringType.Default, nullable: false))
                    .Field(new Field("ordinal", Int32Type.Default, nullable: false))
                    .Metadata("content_key", $"{dataset.ContentKey:x32}")
                    .Metadata("schema_id", schema.Tag)
                    .Metadata("kind", dataset.Kind.Key)
                    .Metadata("instances", dataset.Instances.Count.ToString(CultureInfo.InvariantCulture))
                    .Build();
                return (wire, dataset.Instances.Map(instance => Batch(wire, channels, instance, allocator)));
            });
    }

    // One instance, one batch: every channel array borrows the arena slice the kernel already owns, so the only
    // material this fold allocates is the two identity columns. Column order IS the kind's declared active-set
    // order, so the schema and the array sequence share one declaration and cannot drift apart.
    static RecordBatch Batch(Schema wire, Seq<EncodingChannel> channels, EncodedGeometry instance, MemoryAllocator? allocator) {
        // Root-qualified join token: the witness composite crosses WHOLE, so a scan joining back to the encode
        // never merges a source-rooted and a payload-rooted instance whose digest bytes coincide.
        string source = $"{instance.Witness.Root.Key}:{instance.Witness.ContentHash.Value:x32}";
        Seq<IArrowArray> columns = channels.Map(channel => Lane(channel, instance)) + Seq<IArrowArray>(
            new StringArray.Builder().Reserve(instance.Count)
                .AppendRange(Enumerable.Repeat(source, instance.Count)).Build(allocator),
            new Int32Array.Builder().Reserve(instance.Count)
                .Append(Enumerable.Range(0, instance.Count).ToArray()).Build(allocator));
        return new RecordBatch(wire, columns, instance.Count);
    }

    // Every channel slice rides the kernel's own `Channel` reader rather than a re-slice of `Payload`: this arena
    // is MIXED dtype — a mesh patch tiles float32 positions beside float16 curvature — so one width reinterpreted
    // across the whole payload reads its neighbours as garbage. Each wrap keeps the quantized bits its round-trip
    // witness measured, since widening a half or a unorm lane here re-spells values that tolerance proof already
    // certified at their stored width.
    static IArrowArray Lane(EncodingChannel channel, EncodedGeometry instance) {
        ArenaLane lane = Lanes[channel.Dtype];
        ArrowBuffer buffer = new(instance.Channel(channel));
        return channel.Arity == 1
            ? lane.Wrap(buffer, instance.Count)
            : new FixedSizeListArray(new FixedSizeListType(lane.Type, channel.Arity), instance.Count,
                lane.Wrap(buffer, checked(instance.Count * channel.Arity)), ArrowBuffer.Empty);
    }

    // Hyphenated channel keys normalize the same way a hive segment does, so a scan spells one identifier bare.
    static Field Column(EncodingChannel channel) =>
        new(channel.Key.Replace('-', '_'),
            channel.Arity == 1 ? Lanes[channel.Dtype].Type : new FixedSizeListType(Lanes[channel.Dtype].Type, channel.Arity),
            nullable: false);

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

    // ONE lake-landing projection over both producers, overloaded on the dataset shape so no `LandDoe`/
    // `LandCost` verb family arises: each fold seals its batch, names its LandingArm row and the readable
    // segment that arm's hive key spells, and derives the generation coordinate the custodian writes under.
    // Compute owns the batch shape and this coordinate ALONE — `Rasm.Persistence` `Query/columnar
    // #FLAT_TABLE_EGRESS` `Land` holds writers, residence, slots, index custody, and batch-metadata
    // preservation, so a Compute-side Parquet write, generation directory, artifact-index stamp, or Flight
    // dial to push bytes forks lake custody the branch settled on one custodian. Tenancy arrives as the
    // frame's own TenantContext because the hive tree's `tenant=` segment is what makes a tenant-scoped scan
    // prune rather than answer zero rows.
    public static Fin<(LakeGeneration Generation, Schema Schema, Seq<RecordBatch> Batches)> Landing(
        DoeDataset dataset, TenantContext tenant, MemoryAllocator? allocator = null) =>
        Doe(dataset, allocator).Map(batch => (
            new LakeGeneration(LandingArm.Doe, tenant, Segment(dataset.Strategy.Key), SchemaKey(batch.Schema), dataset.ContentKey),
            batch.Schema, Seq(batch)));

    // Geometry is the one arm whose SCHEMA KEY is not derived from the Arrow field list: the kernel already mints
    // a content-keyed schema identity over its own kind and field roster, and the custodian's geometry landing row
    // names that law by name. Re-digesting the Arrow projection here keys the hive tree on a spelling the kernel
    // never published, splitting the tree on projection detail two encoders agreeing on geometry already share.
    public static Fin<(LakeGeneration Generation, Schema Schema, Seq<RecordBatch> Batches)> Landing(
        GeometryDataset dataset, TenantContext tenant, MemoryAllocator? allocator = null) =>
        Geometry(dataset, allocator).Map(built => (
            new LakeGeneration(LandingArm.Geometry, tenant, Segment(dataset.Model),
                dataset.Schema.SchemaId, dataset.ContentKey),
            built.Schema, built.Batches));

    public static Fin<(LakeGeneration Generation, Schema Schema, Seq<RecordBatch> Batches)> Landing(
        ChargebackDataset dataset, TenantContext tenant, MemoryAllocator? allocator = null) =>
        Chargeback(dataset, allocator).Map(batch => (
            new LakeGeneration(
                LandingArm.Cost, tenant, Segment(MonthSegment.Format(dataset.WindowStart.InUtc().Date)),
                SchemaKey(batch.Schema), dataset.ContentKey),
            batch.Schema, Seq(batch)));

    // `Identifier` admits ASCII letters, digits, and underscore under a NON-DIGIT lead, so every hive segment
    // normalizes through one projection: a hyphenated smart-enum key or a bare ISO date reaches `Create` as a
    // refusal and kills the landing at a directory name rather than at a schema. The month token carries its
    // own leading letter for exactly that reason and stays the readable value a board spells.
    static readonly LocalDatePattern MonthSegment = LocalDatePattern.CreateWithInvariantCulture("'m'uuuu'_'MM");

    static Identifier Segment(string value) => Identifier.Create(value.Replace('-', '_'));

    // Schema identity is the ORDERED field vocabulary — each field's name beside its Arrow TypeId in append
    // order — so an additive column lands a compatible generation under its own schema key while a reordered
    // or retyped column lands a distinct tree the reader's positional ordinals never mis-bind. Field metadata
    // stays out of the digest: a receipt fact rides Schema.Metadata and never re-keys the tree.
    static UInt128 SchemaKey(Schema schema) =>
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(string.Join(
            '|', schema.FieldsList.Select(static field => $"{field.Name}:{field.DataType.TypeId}"))));

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

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
