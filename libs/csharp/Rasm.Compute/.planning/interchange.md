# [COMPUTE_INTERCHANGE]

Rasm.Compute interchange lane: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to geometry or model graph) from export (artifact to foreign bytes) across the managed glTF/GLB read-write over SharpGLTF and the managed IFC STEP/XML/JSON read-write over GeometryGym, the in-process semantic IFC ingest through `DatabaseIfc.Project.Extract`, the two-hop IFC-to-geometry tessellation rail that crosses to the IfcOpenShell companion only when geometry evaluation is required, and the content-addressed artifact identity that folds deflection and tolerance into the key. The page owns the `InterchangeFormat` axis with its `CanImport`/`CanExport`/codec-owner/`TessellationRequiresCompanion` columns, the `InterchangeIo` import/export fold, the `IfcSemanticModel` graph projection, the `TessellationRequest` two-hop bridge, and the `InterchangeIdentity` content-key — composing the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` identity precedent, and the `Substrate.RemoteGrpc` companion hop as settled vocabulary; the page is HOST-LOCAL and carries no TS_PROJECTION.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                      |
| :-----: | :------------------ | :------------------------------------------------------------------------- |
|   [1]   | FORMAT_AXIS         | Format rows; import/export capability, codec owner, companion columns      |
|   [2]   | IMPORT_RAIL         | Foreign-bytes ingest: managed glTF/GLB mesh+scene, in-proc semantic IFC    |
|   [3]   | EXPORT_RAIL         | Artifact emit: mesh/scene to GLB, model to IFC STEP/XML/JSON               |
|   [4]   | TWO_HOP_TESSELLATION| IFC geometry evaluation crosses to the IfcOpenShell companion, never in-proc|
|   [5]   | CONTENT_ADDRESSING  | XxHash128 artifact identity folding deflection and tolerance into the key  |

## [2]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` interchange-format rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, and `TessellationRequiresCompanion` columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package that reads and writes the row.
- Cases: `InterchangeFormat` rows gltf · glb · ifc · ifc-xml · ifc-json; `InterchangeCodec` rows sharp-gltf (SharpGLTF managed glTF 2.0) · geometry-gym (GeometryGym managed IFC).
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or a wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC geometry request to the two-hop rail and a managed glTF mesh inline without an `if (ifc)` branch.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, BCL inbox
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, and companion column; a new managed codec package is one `InterchangeCodec` row; zero new surface.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form mirroring the no-`TensorService` law; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric read-write while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC rows because GeometryGym carries no tessellation kernel (the catalogue boundary fact) — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; media types are the IANA `model/gltf-binary`, `model/gltf+json`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, traced here once for the lane.

```csharp signature
public sealed class InterchangeKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class InterchangeCodec {
    public static readonly InterchangeCodec SharpGltf = new("sharp-gltf");
    public static readonly InterchangeCodec GeometryGym = new("geometry-gym");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc", ".step", ".stp"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true);

    private readonly Seq<string> extensions;

    public string MediaType { get; }
    public bool CanImport { get; }
    public bool CanExport { get; }
    public InterchangeCodec Codec { get; }
    public bool TessellationRequiresCompanion { get; }

    public Seq<string> Extensions => extensions;

    static readonly FrozenDictionary<string, InterchangeFormat> ByExtension =
        Items.SelectMany(static row => row.extensions.Map(ext => (ext, row))).ToFrozenDictionary(static pair => pair.ext, static pair => pair.row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByMediaType =
        Items.ToFrozenDictionary(static row => row.MediaType, static row => row, StringComparer.OrdinalIgnoreCase);

    public static Fin<InterchangeFormat> Detect(string pathOrMediaType) =>
        ByMediaType.TryGetValue(pathOrMediaType, out var byType) ? Fin.Succ(byType)
        : ByExtension.TryGetValue(Path.GetExtension(pathOrMediaType), out var byExt) ? Fin.Succ(byExt)
        : Fin.Fail<InterchangeFormat>(new ComputeFault.ModelRejected($"<interchange-format-miss:{pathOrMediaType}>"));
}
```

## [3]-[IMPORT_RAIL]

- Owner: `InterchangeIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF and the in-process semantic IFC ingest through GeometryGym; `ImportedGeometry` the decoded mesh-scene carrier; `IfcSemanticModel` the IFC model-graph projection.
- Entry: `public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF mesh path; `public static Fin<IfcSemanticModel> ImportIfc(ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the in-process IFC semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, projecting the SharpGLTF `ModelException` family and the GeometryGym parse fault onto `ComputeFault.ModelRejected` at the boundary so domain code never sees the package exception.
- Auto: glTF/GLB decode lands through `ModelRoot.ParseGLB`/`ModelRoot.Load` then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project` to `IfcProject`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), and type objects (`IfcTypeObject`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, Rasm (project), Rasm.Persistence (project), BCL inbox
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; zero new surface.
- Boundary: `InterchangeIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`GetTextureCoord`/`TriangleIndices` so vertex and index data project to `ImportedGeometry` spans owned by the `staging-and-streams#ALLOCATION_AXIS` `PooledMemory` row, never a managed `float[]` per primitive; the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and the catalogue boundary fact holds that GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `TWO_HOP_TESSELLATION` rail and never evaluates a BRep in-process; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on import so a malformed asset faults at parse rather than mid-decode; the string-tensor and host-geometry types stay inside the capsule and never enter lane signatures; an `IfcImporter`/`GltfImporter` service family and a managed IFC tessellator are the deleted forms.

```csharp signature
public sealed record ImportedGeometry(
    InterchangeFormat Format,
    ReadOnlyMemory<float> Vertices,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<long> Indices,
    int VertexCount,
    int TriangleCount,
    Instant At);

public sealed record IfcSemanticModel(
    ReleaseVersion Schema,
    ModelView View,
    Seq<IfcSemanticModel.SpatialNode> Spatial,
    Seq<IfcSemanticModel.ProductRow> Products,
    Seq<IfcSemanticModel.PropertyRow> Properties,
    Seq<IfcSemanticModel.QuantityRow> Quantities,
    Seq<IfcSemanticModel.MaterialRow> Materials,
    Seq<IfcSemanticModel.TypeRow> Types,
    double Tolerance,
    Instant At) {
    public sealed record SpatialNode(string GlobalId, string EntityType, string Name, string LongName, Seq<string> ContainedGlobalIds);
    public sealed record ProductRow(string GlobalId, string EntityType, string Name, string Tag, Option<string> TypeGlobalId);
    public sealed record PropertyRow(string OwnerGlobalId, string SetName, string PropertyName, string Value);
    public sealed record QuantityRow(string OwnerGlobalId, string SetName, string QuantityName, double Value, string Unit);
    public sealed record MaterialRow(string OwnerGlobalId, string MaterialName);
    public sealed record TypeRow(string GlobalId, string EntityType, string Name);
}

public static class InterchangeIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<import-unsupported:{format.Key}>"))
        : format.Codec == InterchangeCodec.SharpGltf ? Gltf(format, bytes, clocks.Now)
        : Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<import-needs-companion:{format.Key}>"));

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Try.lift(() => Semantic(Database(format, bytes), clocks.Now)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message))
            : Fin.Fail<IfcSemanticModel>(new ComputeFault.ModelRejected($"<ifc-codec-miss:{format.Key}>"));

    static Fin<ImportedGeometry> Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) =>
        Try.lift(() => {
            var settings = new ReadSettings { Validation = ValidationMode.Strict };
            var model = format == InterchangeFormat.Glb ? ModelRoot.ParseGLB(new ArraySegment<byte>(bytes.ToArray()), settings) : ModelRoot.ReadGLB(new MemoryStream(bytes.ToArray()), settings);
            return Decoded(format, model.LogicalMeshes.Decode(), at);
        }).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message));

    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes) =>
        format == InterchangeFormat.Ifc ? DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span))
        : format == InterchangeFormat.IfcJson ? JsonDatabase(bytes)
        : XmlDatabase(bytes);
}
```

## [4]-[EXPORT_RAIL]

- Owner: `InterchangeIo` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization, all fully managed with no companion; `ExportArtifact` the emitted-bytes carrier feeding the content-addressing cluster.
- Entry: `public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss or a codec fault projected onto `ComputeFault.ModelRejected`.
- Auto: GLB export assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
- Receipt: the `StreamSegment` receipt carries the format key, codec key, emitted byte count, and the content-key the addressing cluster computes; emission rides the sink port.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new managed export is one codec arm on the export fold; a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; zero new surface.
- Boundary: the export fold extends the `InterchangeIo` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; IFC serialization selects `FormatIfcSerialization.STEP`/`XML`/`JSON` by the row and `DatabaseIfc.WriteStream`/`ToString` are the only emit members — a hand-rolled STEP writer is the deleted form; the model graph re-authoring runs through `FactoryIfc` canonical axes, origins, and owner history so a round-tripped model carries stable GlobalIds through `ParserIfc.HashGlobalID` keyed on a stable entity key rather than a fresh GUID per export, making export idempotent under the content-key; a write to a row whose `CanExport` is false faults at the boundary; the emitted bytes never copy into a managed array beyond the one write window the `staging-and-streams#STREAM_POOL` contiguous route bounds.

```csharp signature
public sealed record InterchangePolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    ReleaseVersion IfcSchema,
    bool MergeBuffers,
    bool StridedBuffers) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, MergeBuffers: true, StridedBuffers: true);
}

public sealed record ExportArtifact(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

public static class InterchangeExport {
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<export-unsupported:{format.Key}>"))
        : format.Codec == InterchangeCodec.SharpGltf
            ? Try.lift(() => Sealed(format, GlbBytes(geometry, policy), policy, clocks.Now)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<export-codec-miss:{format.Key}>"));

    public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Try.lift(() => Sealed(format, IfcBytes(format, model, policy), policy, clocks.Now)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<ifc-export-codec-miss:{format.Key}>"));

    static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes, InterchangeIdentity.Key(format, bytes, policy), bytes.LongLength, at);
}
```

## [5]-[TWO_HOP_TESSELLATION]

- Owner: `TessellationRequest` — the two-hop bridge that crosses IFC geometry evaluation to the IfcOpenShell companion (`IfcConvert` producing GLB) and re-imports the GLB through the `IMPORT_RAIL` glTF path; the request is host-local in posture and rides the existing remote-lane companion rpc, never a new transport.
- Entry: `public static Fin<TessellationRequest> Plan(IfcSemanticModel model, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; the companion round-trip rides the existing `remote-lane#PROTO_VOCABULARY` `Solve`/artifact transport — the GLB result re-enters through `InterchangeIo.ImportGeometry(InterchangeFormat.Glb, ...)`.
- Auto: `Plan` reads `InterchangeFormat.TessellationRequiresCompanion` to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference to the Persistence artifact index rather than re-crossing the companion.
- Receipt: the `RemoteCall` receipt carries the companion transport, the IFC content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `TessellationRequest` folded into the content-key; zero new surface.
- Boundary: the two-hop rail is the single IFC-to-geometry path because GeometryGym carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package living in `libs/python/compute`, never a NuGet pin, and it is reached only through the existing remote-lane companion rpc so this page mints no transport, no channel, and no second wire vocabulary — the host-local posture means an in-process Rhino host crosses to the companion process over the same UDS/InProcess leg `remote-lane#TRANSPORT_AXIS` owns and a remote tessellation rides that same companion rpc; the GLB the companion returns re-enters the managed import rail so the decoded mesh is the same `ImportedGeometry` shape a native glTF import produces, and the IFC semantic graph (from the `IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the content-key; the companion-daemon protocol detail is the next-loop concern named in RESEARCH, the bridge fence here is transcription-complete on the request shape and the cache-by-content-key reuse.

```csharp signature
public sealed record TessellationRequest(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    InterchangeFormat Result) {
    public static Fin<TessellationRequest> Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy) =>
        source.TessellationRequiresCompanion
            ? Fin.Succ(new TessellationRequest(
                InterchangeIdentity.Key(source, ifcBytes.ToArray(), policy), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, InterchangeFormat.Glb))
            : Fin.Fail<TessellationRequest>(new ComputeFault.ModelRejected($"<tessellation-not-required:{source.Key}>"));

    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}
```

## [6]-[CONTENT_ADDRESSING]

- Owner: `InterchangeIdentity` — the content-key derivation folding the artifact bytes plus the deflection and tolerance policy into one `XxHash128` identity, mirroring the model-lane `ModelIdentity.Snapshot` precedent; the artifact lands content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` with no second cache.
- Entry: `public static UInt128 Key(InterchangeFormat format, ReadOnlySpan<byte> bytes, InterchangePolicy policy)` — pure value; identity derives from the bytes and the evaluation policy, never from a path or filename.
- Auto: the key seeds `XxHash128.HashToUInt128` over the artifact bytes with a seed mixing the format key, the deflection, the tolerance, and the angle tolerance so a re-tessellation at a different deflection keys distinctly and a re-import of identical bytes at identical settings keys identically — deflection and tolerance fold into the key, never a cross-setting hit; `Admit` projects the artifact onto `ArtifactIndexRow.Admit` under the interchange classification and retention columns so the blob lane stores and serves the addressed bytes.
- Receipt: the `Cache` receipt carries the content-key and the hit/miss/store outcome; a stored artifact rides the `ArtifactIndexRow` checksum and byte size into the receipt.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new evaluation parameter that changes the artifact is one column folded into the seed; zero new surface.
- Boundary: artifact identity is `XxHash128` over the canonical bytes — the suite hash law the `remote-lane#ARTIFACT_FRAMES` whole-artifact identity row and the model-lane `ModelIdentity` checksum already hold, never a second hashing pass and never a path-keyed identity; the deflection and tolerance fold into the seed so the geometry-evaluation settings partition the key and a coarse and a fine tessellation of the same IFC never collide — a cross-setting hit is the named defect; the addressed bytes land on the Persistence blob lane through `ArtifactIndexRow.Admit` keyed on the content-key, the single artifact owner, so the IFC semantic graph, the tessellated GLB, and a re-exported glTF are three content-keyed rows under one identity scheme the Persistence index owns — Compute owns the identity derivation and Persistence owns blob residence, neither re-declaring the other; a managed copy of the artifact bytes beside the blob lane is the rejected form.

```csharp signature
public static class InterchangeIdentity {
    public static UInt128 Key(InterchangeFormat format, ReadOnlySpan<byte> bytes, InterchangePolicy policy) =>
        XxHash128.HashToUInt128(bytes, Seed(format, policy));

    public static ArtifactIndexRow Admit(ExportArtifact artifact, DataClassification classification, string retentionClass) =>
        ArtifactIndexRow.Admit(ArtifactIndexRow.Interchange, $"{artifact.ContentKey:x32}:{artifact.Format.Key}", artifact.Bytes.ToArray(), classification, retentionClass, artifact.At);

    static long Seed(InterchangeFormat format, InterchangePolicy policy) =>
        unchecked((long)XxHash3.HashToUInt64(MemoryMarshal.AsBytes($"{format.Key}|{policy.Deflection:R}|{policy.Tolerance:R}|{policy.AngleTolerance:R}".AsSpan())));
}
```

## [7]-[RESEARCH]

- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the two-hop tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is the next-loop concern owned by `libs/python/compute`; the `TessellationRequest` shape and the content-key cache-reuse are transcription-complete, the companion wire detail rides the existing remote-lane companion rpc and lands when the Python branch authors its compute folder.
- [ARTIFACT_INDEX_ROW]: the `ArtifactIndexRow.Interchange` classification row on the Persistence artifact-blob index that carries the interchange artifact kind beside `EpContext` and `OnnxProfile` — the row exists on the Persistence cache-indexes owner and Compute consumes it as settled vocabulary; the exact kind-enum spelling confirms against the Persistence `ArtifactIndexRow` owner at cross-folder alignment.
