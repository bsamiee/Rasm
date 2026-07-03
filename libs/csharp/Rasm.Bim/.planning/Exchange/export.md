# [BIM_EXPORT_RAIL]

The artifact-emit rail: one `BimExport.Export` TOTAL codec fold over an `ExportPayload` union — `Soup` the flat `ImportedGeometry` triangle carrier, `Scene` the per-element `ElementScene` (a content-keyed mesh pool plus placed `ElementInstance` rows) — dispatching GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, `.bim` through the `dotbim` shared-`Mesh`-pool/placed-`Element` instancing wire, FBX/Collada through the `AssimpNetter` `AssimpContext.ExportToBlob` scene serializer, OpenUSD through the `UniversalSceneDescription` `UsdStage` author, and the 3D-Tiles 1.1 `.subtree` availability bitstream through the `subtree` `SubtreeCreator`. The dispatch is the generated exhaustive `InterchangeCodec.Switch` mirroring `import#IMPORT_RAIL` — a new codec row BREAKS this call site at compile time; the `==` codec ladder with its silent `export-codec-miss` tail is the deleted form. `BimExport.Author` mints the per-element glTF scene as a `GlbScene` — one `NodeBuilder` per element NAMED by its seam GlobalId, one logical mesh per distinct content key (N repeats = N nodes over ONE mesh, `EXT_mesh_gpu_instancing` a policy threshold) — so the `GlobalId`→`Node` index `TileMetadata` and `AnimateSchedule` bind against is MINTED HERE, never caller-walked. The IFC STEP/XML/JSON leg does NOT re-author IFC here: it DELEGATES to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` (the ONE Bim-internal `ElementGraph`→`DatabaseIfc` re-author — the `PredefinedType` egress gate [C6], the 1:1 `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` [H9], and the material/classification/relationship re-author), this rail OWNING only the artifact seal (`ExportArtifact` + the Compute content key) and reading the serialization from the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column. The page composes the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`Element` (a consumer reads the baked `Element`, never a stored record), the `import#IMPORT_RAIL` `ImportedGeometry` carrier and `BimIo.ImportIfc` schema-sniffed re-decode, the `format#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key as settled vocabulary; the emitted `ExportArtifact` feeds the Compute content-addressing seam. The retired `BimModel`/`BimElement` carriers and the hand-rolled `IfcBuildingElementProxy` re-author (a lossy SECOND IFC-egress owner) are GONE. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[EXPORT_RAIL]: artifact emit — the `ExportPayload` `Soup`/`Scene` union through one TOTAL `InterchangeCodec.Switch` (GLB with Draco/meshopt encode, the per-element `GlbScene` author + `EXT_mesh_gpu_instancing`, the `dotbim` instancing wire, AssimpNetter FBX/Collada, `UsdStage`); the IFC STEP/XML/JSON leg DELEGATING to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, this rail owning only the `ExportArtifact` seal + the `InterchangeFormat.Serialization` column read.
- [02]-[TILE_METADATA]: per-tile `EXT_structural_metadata` schema/class/property-table over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, not a stored record), bound through `EXT_mesh_features` over the `Staged`-authored per-vertex `_FEATURE_ID_0` row stamps the `GlbScene.Rows` index names.
- [03]-[BIM_LOD]: the per-element LOD pyramid through `Meshopt.Simplify`/`SimplifySloppy`, the `Meshopt.BuildMeshlets` meshlet residency band, and the per-LOD content key the `Rasm.Compute#TILE_PARTITION` pyramid addresses.
- [04]-[SCHEDULE_ANIMATION]: the `AnimateSchedule` arm baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element glTF visibility/scale keyframe tracks through `ModelRoot.CreateAnimation` and the `KHR_node_visibility` channel over the `Author`-minted `GlbScene` `GlobalId`→`Node` index, so a 4D schedule exports as one animated GLB a web viewer scrubs.
- [05]-[ROUNDTRIP]: the `RoundTrip` lossless-verification matrix folding an `ElementGraph` emit→`BimIo.ImportIfc` schema-sniffed re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations, witnessing per-element fidelity by the seam content key joined on the 1:1 `ExternalId` and naming the divergent members through the `Generator.Equals` structured diff.
- [06]-[TILE_AVAILABILITY]: the `TileAvailability` 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream author over the `subtree` `SubtreeCreator`/`SubtreeCreator3D`/`Tile`/`Tile3D`/`MortonIndex` surface — the tileset-side complement the `SharpGLTF.Ext.3DTiles` per-tile content leg cannot reach, retiring the hand-rolled implicit-tiling bitstream.

## [02]-[EXPORT_RAIL]

- Owner: `BimExport` — the export fold over `InterchangeFormat`, one TOTAL generated `InterchangeCodec.Switch` over the `ExportPayload` union (`Soup(ImportedGeometry)` | `Scene(ElementScene)`), the IFC STEP/XML/JSON leg DELEGATING to the seam `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit` (this rail seals the bytes, the projector owns the re-author); `ElementScene` the per-element carrier — a `Map<UInt128, ImportedGeometry>` content-keyed mesh pool plus `ElementInstance` placement rows (GlobalId, name, classification code, mesh key, rigid `Matrix4x4` placement) — so repeated geometry travels ONCE; `GlbScene` the `Author`-minted `(ModelRoot, Map<string, Node>, Map<string, int>)` triple carrying the per-element node index AND the `GlobalId`→feature-row index downstream legs bind; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export(InterchangeFormat format, ExportPayload payload, InterchangePolicy policy, ClockPolicy clocks, Op key)` — ONE entry, the payload case discriminating flat-soup from per-element emit per MODAL_ARITY (a `bool perElement` knob beside the value is the rejected form); `BimExport.Author(ElementScene scene, InterchangePolicy policy, Op key)` minting the `GlbScene` the metadata/animation legs decorate before `Emit(GlbScene, InterchangeFormat, InterchangePolicy, ClockPolicy, Op)` seals it; `BimExport.ExportIfc(InterchangeFormat format, ElementGraph graph, SemanticProjector projector, InterchangePolicy policy, ClockPolicy clocks, Option<ElementGraph> prior, IIfcProfileStore profiles, Op key)` for the IFC serialization — the `graph` is the seam read snapshot, the `projector` the Bim-internal IFC-egress owner the app wires, the `prior` snapshot driving the diff-derived `OwnerHistory` `ChangeAction` [H9], the `profiles` store reconstituting a `ProfileSet`'s `IfcProfileDef` from preserved profile data; `Fin<T>` aborts on a write-capability miss (`Model/faults#FAULT_BAND` `BimFault.CodecReject`), a route miss the total `Switch` names (the IFC/geospatial/point-cloud arms fault with their owning rail in the message), or a captured serialization/predefined-gate fault the projector lowers (`BimFault.ModelRejected`/`BimFault.UnmappedClass`), each typed `BimFault` case (band 2600, `Expected`-derived) lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop.
- Auto: the `GlbBytes` fold switches on `InterchangePolicy.Compression` — the `KhrEncoder.None` arm routes `Soup` through the single-mesh `SceneOf` (`MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, `SceneBuilder.AddRigidMesh`, `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`) and `Scene` through `Author` (one `MeshBuilder` per distinct pool key, one GlobalId-named `NodeBuilder` per instance with `NodeBuilder.LocalMatrix` the rigid placement, `AddRigidMesh(mesh, node)` sharing ONE logical mesh across repeats, `SceneBuilderSchema2Settings.GpuMeshInstancingMinCount = policy.GpuInstancingMinCount` collapsing node fan-outs into `EXT_mesh_gpu_instancing` past the threshold), both writing through `ModelRoot.WriteGLB`; the `KhrEncoder.Draco` arm bypasses the GLB container and quantizes the payload's flattened soup into a `DracoMesh` (`PointAttribute.Wrap(AttributeType.Position, …)` per-attribute wrap, `DracoMesh.AddFace(int[])` per triangle), emitting the Draco byte stream through `Draco.Encode(mesh, DracoEncodeOptions)`; the `KhrEncoder.Meshopt` arm first runs the catalogued meshopt optimization pipeline — `GenerateVertexRemap` deduplicates the exploded triangle-soup into a unique-vertex set, `RemapVertexBuffer`/`RemapIndexBuffer` apply the remap, then `OptimizeVertexCache`/`OptimizeOverdraw`/`OptimizeVertexFetch` reorder for GPU cache, overdraw, and fetch locality — bounds the destination through `EncodeVertexBufferBound`/`EncodeIndexBufferBound`, and emits the meshopt bufferView payloads through the pinned-pointer `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the optimized indexed buffers; neither codec takes a glTF `ModelRoot` and neither arm writes the GLB container, so the compression leg replaces the GLB write rather than post-processing it (a per-element `Scene` payload flattens through `ElementScene.Soup` — the raw Draco/meshopt streams carry no scene graph, so the per-element structure rides the GLB arms only); the `.bim` arm pools distinct geometry by content key into `dotbim.Mesh` rows and each `ElementInstance` into a placed `dotbim.Element` (`Matrix4x4.Decompose` splitting the rigid placement onto `Vector`+`Rotation`, the GlobalId onto the validated `Guid`, the classification code onto `Type`, the name into the `Info` bag); the IFC leg selects no serialization writer here — `ExportIfc` reads the row's `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column (`Some` exactly on the GeometryGym rows) and hands the seam `ElementGraph` plus that serialization to `SemanticProjector.Emit`, which re-authors the whole graph (the per-`Object` `PredefinedType` egress gate + schema-span validation [C6][H8], the 1:1 `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` [H9], and the material/classification/relationship re-author) and returns the IFC text the rail UTF-8-encodes and seals — no `DatabaseIfc` is constructed or canonically placed on this page.
- Receipt: the `ModelEmit` receipt case carries the format key, codec key, emitted byte count, and the `ExportArtifact.ContentKey` the Compute addressing seam computes, symmetric to the import `ModelLoad` case; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, CommunityToolkit.HighPerformance, AssimpNetter, dotbim, UniversalSceneDescription, Rasm.Element, NodaTime, LanguageExt.Core
- Growth: a new managed export is one arm on the TOTAL `InterchangeCodec.Switch` — the compiler forces the arm the moment the codec row lands (the `dotBim` instancing arm joined the SharpGltf/SceneExchange/UsdStage arms this way), never a per-format exporter family and never a silent ladder tail; a new emit modality is one `ExportPayload` case every codec arm is compiler-forced to route; a new IFC serialization is the `InterchangeFormat.Serialization` column value on one GeometryGym row; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold; a new assimp export target is one scene-exchange row whose KEY is the `ExportToBlob` `exportFormatId` (`IsExportFormatSupported`-guarded).
- Boundary: the export fold extends the `BimExport` boundary capsule and its dispatch is the generated exhaustive `Switch` — every codec row declares its export route or its route-naming fault (`ExportIfc` for GeometryGym, `Semantics/geospatial#VECTOR_INGEST` `GeoVector.Write` for the geospatial rows, the companion bridge for native/IGES), so a new row can never fall into a stale miss tail; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers, buffer-merge, and the `GpuMeshInstancingMinCount` threshold so the emitted artifact is deterministic byte layout the Compute content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the per-element `Author` scene is the ONE `GlobalId`→`Node` index minter (nodes named by the seam `Object.ExternalId`, index read back from `ModelRoot.LogicalNodes`), the ONE feature-row stamp minter (`GlbScene.Rows` instance ordinals, stamped per-vertex as `_FEATURE_ID_0` at `Staged` time — the only point the vertex layout is open), AND the ONE color-tint minter (the `ElementInstance.Rgba` column lands as the primitive `BaseColor` through `MaterialBuilder.WithChannelParam(KnownChannel.BaseColor, Vector4)`, materials pooled per distinct Rgba, uniform-color repeats keeping their tint — the GLB arm erasing the color the dotbim arm round-trips was the deleted asymmetry) — a caller-walked scene graph, a second index mint, or a post-hoc attribute write beside it is the deleted form, and the `EXT_mesh_gpu_instancing` collapse is a POLICY threshold because a gpu-merged node loses its per-node visibility/metadata identity (the 4D/metadata pipeline runs `GpuInstancingMinCount: 0`, the streaming-tile pipeline raises it — the tension is a policy value, never a code fork); the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)` before any write so a material/light/texture channel serializes through its decompile-verified SharpGLTF schema type rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression` and `KHR_meshopt_compression` rows carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode through the static `Draco.Encode(DracoPointCloud, DracoEncodeOptions)` over a `DracoMesh` built from `PointAttribute.Wrap` attributes, and `Alimer.Bindings.MeshOptimizer` owns the meshopt encode through the static `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the raw vertex/index buffers, both quantizing to the `InterchangePolicy` bit budget, a glTF `ModelRoot` passed to either codec the rejected form because neither package owns a glTF model type; the `.bim` and USD arms cross a temp path because `dotbim.File.Save`/`UsdStage.Export` are path-bound (no stream overload) — the temp file is deleted in the same expression and never escapes the capsule; the IFC egress is NOT this rail's — `ExportIfc` delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, and a hand-rolled `IfcBuildingElementProxy` re-author (the deleted `IfcBytes` form) is a SECOND IFC-egress owner the seam forbids — the `GlobalId` round-trips 1:1 from the `Object.ExternalId` inside `Emit` (never a fresh GUID per export), making export idempotent under the Compute content-key; `ExportIfc` retains only the `CanExport` capability gate and the `Serialization` column read (a `None` column IS the non-IFC-row fault, the deleted `SerializationOf` ladder's job now row data), faulting at the boundary on a write-only row; the chunked-field and structural-delta codecs stay at `Rasm.Compute/Runtime/codecs` consumed at the seam.

```csharp signature
public sealed record InterchangePolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    ReleaseVersion IfcSchema,
    StepProtocol StepProtocol,
    bool MergeBuffers,
    bool StridedBuffers,
    KhrEncoder Compression,
    int QuantizationBits,
    int GpuInstancingMinCount,
    Seq<double> LodRatios,
    Seq<KhrExtension> Extensions) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.Ifc4X3Add2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Compression: KhrEncoder.None, QuantizationBits: 14, GpuInstancingMinCount: 0,
        LodRatios: Seq(0.5, 0.25, 0.1, 0.05), Extensions: Seq<KhrExtension>());
    public static readonly InterchangePolicy Web = Canonical with {
        Compression = KhrEncoder.Meshopt, QuantizationBits = 12, GpuInstancingMinCount = 16,
        Extensions = Seq(KhrExtension.MaterialsSpecular, KhrExtension.MaterialsIor, KhrExtension.MaterialsEmissiveStrength, KhrExtension.LightsPunctual, KhrExtension.TextureBasisu, KhrExtension.TextureTransform),
    };
    public static readonly InterchangePolicy Pbr = Canonical with {
        Extensions = Seq(KhrExtension.MaterialsClearcoat, KhrExtension.MaterialsTransmission, KhrExtension.MaterialsVolume, KhrExtension.MaterialsSheen,
            KhrExtension.MaterialsIridescence, KhrExtension.MaterialsAnisotropy, KhrExtension.MaterialsDispersion, KhrExtension.MaterialsSpecular, KhrExtension.MaterialsIor, KhrExtension.MaterialsEmissiveStrength),
    };
}

public sealed record ExportArtifact(
    InterchangeFormat Format,
    ReadOnlyMemory<byte> Bytes,
    UInt128 ContentKey,
    long ByteCount,
    Instant At);

// One placed element: the seam Object.ExternalId GlobalId, the baked name, the "ifc" Classification code,
// the content key selecting its pool mesh, the rigid placement, and the packed RGBA the dotbim Color column
// round-trips (0xRRGGBBAA, opaque white default). N repeats of one geometry are N rows over ONE pool entry —
// the instancing the flat soup erases.
public sealed record ElementInstance(string GlobalId, string Name, string Class, UInt128 MeshKey, Matrix4x4 Placement, uint Rgba = 0xFFFFFFFF);

// The per-element carrier: a content-keyed mesh pool (each entry ONE baked single-block ImportedGeometry —
// the canonical soup for one distinct geometry) plus the placement rows. Pooled re-describes the scene as the
// import carrier's Blocks/Instances overlay; Soup flattens it through the seam's ONE Bake fold for the arms
// that carry no scene graph (Draco/meshopt streams, single-mesh scene rows) — a second transform loop beside
// ImportedGeometry.Bake is the deleted re-derivation.
public sealed record ElementScene(Map<UInt128, ImportedGeometry> Pool, Seq<ElementInstance> Instances) {
    // Single-element degrade for a flat soup reaching a per-element wire: one pool entry keyed by the
    // vertex-bytes hash, one identity placement, the untyped-proxy classification.
    public static ElementScene Of(ImportedGeometry soup) {
        UInt128 key = XxHash128.HashToUInt128(MemoryMarshal.AsBytes(soup.Vertices.Span));
        return new ElementScene(Map((key, soup)), Seq1(new ElementInstance("soup", "soup", "IfcBuildingElementProxy", key, Matrix4x4.Identity)));
    }

    public ImportedGeometry Soup() => Pooled().Bake();

    // Each pool entry lands one MeshBlock, each ElementInstance one MeshInstance over its block ordinal — the
    // pooled ImportedGeometry carries the SAME sharing this scene does, so a consumer needing world-space
    // geometry calls the one Bake owner and a consumer preserving instancing reads the overlay.
    public ImportedGeometry Pooled() {
        var head = Pool.Values.HeadOrNone().IfNoneUnsafe(() => throw new InvalidDataException("<element-scene-empty>"));
        var keys = Pool.Keys.ToSeq();
        var ordinals = keys.Select(static (k, i) => (k, i)).ToMap();
        int vertexTotal = Pool.Values.Sum(static m => m.VertexCount);
        int indexTotal = Pool.Values.Sum(static m => m.Indices.Length);
        var (vertices, normals, indices) = (new float[vertexTotal * 3], new float[vertexTotal * 3], new long[indexTotal]);
        var blocks = new MeshBlock[keys.Count];
        var (vBase, iBase, slot) = (0, 0, 0);
        foreach (var key in keys) {
            var mesh = Pool[key];
            mesh.Vertices.Span.CopyTo(vertices.AsSpan(vBase * 3));
            mesh.Normals.Span.CopyTo(normals.AsSpan(vBase * 3));
            var t = mesh.Indices.Span;
            for (int s = 0; s < t.Length; s++) { indices[iBase + s] = t[s] + vBase; }
            blocks[slot] = new MeshBlock(vBase, mesh.VertexCount, iBase, t.Length);
            (vBase, iBase, slot) = (vBase + mesh.VertexCount, iBase + t.Length, slot + 1);
        }
        var placed = Instances.Map(i => new MeshInstance(ordinals[i.MeshKey], i.Placement));
        return new ImportedGeometry(head.Format, vertices, normals, indices, vertexTotal, indexTotal / 3, blocks.ToSeq(), placed, head.At);
    }
}

// The ONE emit-modality axis: flat soup or per-element scene — every codec arm is compiler-forced to route
// both cases; a bool/perElement knob beside the payload is the rejected form (MODAL_ARITY).
[Union]
public abstract partial record ExportPayload {
    public sealed record Soup(ImportedGeometry Geometry) : ExportPayload;
    public sealed record Scene(ElementScene Elements) : ExportPayload;

    public ImportedGeometry Flat() => Switch(soup: static s => s.Geometry, scene: static s => s.Elements.Soup());
}

// The Author-minted triple: the glTF model, the GlobalId->Node index, and the GlobalId->feature-row index —
// TileMetadata/AnimateSchedule bind against BOTH; the node index is READ BACK from ModelRoot.LogicalNodes by
// node name so it indexes the model actually emitted, and Rows carries the instance ordinal every uniquely-
// meshed vertex was stamped with at Staged time (the EXT_mesh_features property-table row).
public sealed record GlbScene(ModelRoot Model, Map<string, SharpGLTF.Schema2.Node> Nodes, Map<string, int> Rows);

public static partial class BimExport {
    // TOTAL codec dispatch (import#IMPORT_RAIL parity): a new InterchangeCodec row BREAKS this call site at
    // compile time; each non-emitting arm names its owning route — never a silent ladder tail.
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ExportPayload payload, InterchangePolicy policy, ClockPolicy clocks, Op key) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-unsupported:{format.Key}"))
        : format.Codec.Switch(
            sharpGltf:        () => GlbBytes(payload, policy, key).Map(bytes => Sealed(format, bytes, policy, clocks.Now)),
            dotBim:           () => Try.lift(() => Sealed(format, DotBimBytes(payload), policy, clocks.Now)).Run().MapFail(error => new BimFault.CodecReject(key, $"bim-export:{error.Message}")),
            sceneExchange:    () => Try.lift(() => Sealed(format, SceneBytes(format, payload), policy, clocks.Now)).Run().MapFail(error => new BimFault.CodecReject(key, $"scene-export:{error.Message}")),
            usdStage:         () => Try.lift(() => Sealed(format, UsdBytes(format, payload), policy, clocks.Now)).Run().MapFail(error => new BimFault.CodecReject(key, $"usd-export:{error.Message}")),
            geometryGym:      () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"ifc-export-route:use-ExportIfc:{format.Key}")),
            geospatialVector: () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"geo-export-route:GeoVector.Write:{format.Key}")),
            geospatialRaster: () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"raster-export-none:{format.Key}")),
            meshText:         () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-import-only:{format.Key}")),
            ply:              () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-import-only:{format.Key}")),
            pointCloud:       () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-import-only:{format.Key}")),
            acadSharp:        () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-host-native:{format.Key}")),
            stepIso10303:     () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-import-only:{format.Key}")),
            nativeCompanion:  () => Fin.Fail<ExportArtifact>(new BimFault.CapabilityMiss(key, $"export-needs-host:{format.Key}")),
            igesAnsi:         () => Fin.Fail<ExportArtifact>(new BimFault.CapabilityMiss(key, $"export-needs-host:{format.Key}")),
            saf:              () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-catalogue-pending:{format.Key}")),
            ifc5Pending:      () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-catalogue-pending:{format.Key}")));

    // The per-element scene author — the ONE GlobalId->Node index minter. One MeshBuilder per distinct pool
    // key, one GlobalId-named NodeBuilder per instance (LocalMatrix = the rigid placement), repeats sharing
    // ONE logical mesh; the GpuMeshInstancingMinCount threshold collapses node fan-outs into
    // EXT_mesh_gpu_instancing (policy 0 = never — a gpu-merged node loses per-node visibility/metadata
    // identity, so the 4D/metadata pipeline keeps 0 and the streaming-tile pipeline raises it).
    public static Fin<GlbScene> Author(ElementScene scene, InterchangePolicy policy, Op key) =>
        RegisterExtensions(policy, key).Bind(_ =>
            Try.lift(() => Staged(scene, policy)).Run().MapFail(error => new BimFault.CodecReject(key, $"scene-author:{error.Message}")));

    // Seals a decorated GlbScene (metadata attached, schedule animated) as the GLB artifact.
    public static Fin<ExportArtifact> Emit(GlbScene scene, InterchangeFormat format, InterchangePolicy policy, ClockPolicy clocks, Op key) =>
        Try.lift(() => Sealed(format, WriteGlb(scene.Model, policy), policy, clocks.Now)).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"gltf-export:{error.Message}"));

    // The feature-row stamp AND the color tint are authored HERE, at MeshBuilder time — the only point the
    // vertex layout and the primitive material are open: a pool mesh referenced by exactly ONE instance stamps
    // that instance's table row on every vertex and tints its primitive from ElementInstance.Rgba (BaseColor via
    // WithChannelParam — the wire color the dotbim arm round-trips no longer erases on the GLB arm); a SHARED
    // pool mesh stamps the null row because EXT_mesh_features lives on the (shared) primitive and cannot carry
    // per-node identity, yet KEEPS its tint when every repeat agrees on one Rgba (color is per-mesh, not
    // per-node) — mixed-color repeats fall to opaque white, and the GpuInstancingMinCount policy owns the
    // identity trade (merge repeats and re-bind per instance, or keep per-node visibility and accept null rows).
    static GlbScene Staged(ElementScene scene, InterchangePolicy policy) {
        var rows = scene.Instances.Select(static (instance, row) => (instance.GlobalId, row)).ToMap();
        int nullRow = scene.Instances.Count;
        var byMesh = scene.Instances
            .Select(static (instance, row) => (instance.MeshKey, Row: row, instance.Rgba))
            .GroupBy(static pair => pair.MeshKey)
            .ToDictionary(
                static g => g.Key,
                g => (Stamp: g.Count() == 1 ? g.First().Row : nullRow,
                      Rgba: g.Select(static pair => pair.Rgba).Distinct().Count() == 1 ? g.First().Rgba : 0xFFFFFFFFu));
        var materials = new Dictionary<uint, MaterialBuilder>();
        MaterialBuilder Tinted(uint rgba) => materials.TryGetValue(rgba, out var held) ? held
            : materials[rgba] = new MaterialBuilder($"rgba-{rgba:X8}").WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, new Vector4(rgba >> 24 & 0xFF, rgba >> 16 & 0xFF, rgba >> 8 & 0xFF, rgba & 0xFF) / 255f);
        var pool = scene.Pool.Map((key, mesh) => {
            var (row, rgba) = byMesh.GetValueOrDefault(key, (nullRow, 0xFFFFFFFFu));
            return MeshOf(mesh, Tinted(rgba), Some(row));
        });
        var builder = new SceneBuilder();
        scene.Instances.Iter(instance => {
            var node = new NodeBuilder(instance.GlobalId) { LocalMatrix = instance.Placement };
            builder.AddRigidMesh(pool[instance.MeshKey], node);
        });
        var model = builder.ToGltf2(new SceneBuilderSchema2Settings {
            UseStridedBuffers = policy.StridedBuffers,
            GpuMeshInstancingMinCount = policy.GpuInstancingMinCount <= 0 ? int.MaxValue : policy.GpuInstancingMinCount,
        });
        return new GlbScene(
            model,
            model.LogicalNodes.AsIterable().Filter(static n => n.Name is { Length: > 0 }).Map(static n => (n.Name, n)).ToMap(),
            rows);
    }

    // FBX/Collada emit through AssimpNetter — the `scene-exchange` codec; a Scene payload flattens (Assimp
    // per-element node authoring is the admission-gated growth). The row KEY is the exportFormatId
    // (`fbx`/`collada`), guarded against the live export matrix; glTF/GLB export stays on SharpGLTF so the
    // Draco/meshopt encode stacks on that path, not this one.
    static byte[] SceneBytes(InterchangeFormat format, ExportPayload payload) {
        var geometry = payload.Flat();
        var mesh = new Assimp.Mesh("bim") { MaterialIndex = 0 };
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        for (int v = 0; v < geometry.VertexCount; v++) {
            mesh.Vertices.Add(new Vector3(verts[v * 3], verts[v * 3 + 1], verts[v * 3 + 2]));
            mesh.Normals.Add(new Vector3(normals[v * 3], normals[v * 3 + 1], normals[v * 3 + 2]));
        }
        var indices = geometry.Indices.Span;
        for (int t = 0; t < geometry.TriangleCount; t++) {
            mesh.Faces.Add(new Assimp.Face([(int)indices[t * 3], (int)indices[t * 3 + 1], (int)indices[t * 3 + 2]]));
        }
        var scene = new Assimp.Scene { RootNode = new Assimp.Node("root") };
        scene.Materials.Add(new Assimp.Material { Name = "default" });
        scene.Meshes.Add(mesh);
        scene.RootNode.MeshIndices.Add(0);
        using var context = new AssimpContext();
        return context.IsExportFormatSupported(format.Key)
            ? context.ExportToBlob(scene, format.Key).Data
            : throw new NotSupportedException($"<scene-export-format:{format.Key}>");
    }

    // .bim emit through dotbim — the ONLY wire preserving instancing: distinct geometry pools ONCE as Mesh
    // rows, every ElementInstance a placed Element. File.Save is path-bound (`.bim`-enforced, no stream
    // overload), so the bytes cross a temp path exactly as UsdBytes does. Element.Guid demands RFC-4122 text,
    // and the seam GlobalId is 22-char IFC-compressed — the Guid is minted deterministically from
    // XxHash128(GlobalId) and the verbatim GlobalId rides Info["globalId"], so identity round-trips losslessly
    // and re-export is byte-stable. A non-rigid placement faults loud: the dotbim wire carries no scale.
    static byte[] DotBimBytes(ExportPayload payload) {
        var scene = payload.Switch(soup: static s => ElementScene.Of(s.Geometry), scene: static s => s.Elements);
        var ordinals = scene.Pool.Keys.Select(static (k, index) => (k, index)).ToMap();
        var meshes = scene.Pool.AsIterable().Map(pair => new dotbim.Mesh {
            MeshId = ordinals[pair.Key],
            Coordinates = [.. pair.Value.Vertices.ToArray().Select(static v => (double)v)],
            Indices = [.. pair.Value.Indices.ToArray().Select(static i => (int)i)],
        }).ToList();
        var elements = scene.Instances.Map(instance => {
            if (!Matrix4x4.Decompose(instance.Placement, out var scale, out var rotation, out var translation)
                || Math.Abs(scale.X - 1f) > 1e-4f || Math.Abs(scale.Y - 1f) > 1e-4f || Math.Abs(scale.Z - 1f) > 1e-4f) {
                throw new InvalidDataException($"<dotbim-nonrigid-placement:{instance.GlobalId}>");
            }
            return new dotbim.Element {
                MeshId = ordinals[instance.MeshKey],
                Vector = new dotbim.Vector { X = translation.X, Y = translation.Y, Z = translation.Z },
                Rotation = new dotbim.Rotation { Qx = rotation.X, Qy = rotation.Y, Qz = rotation.Z, Qw = rotation.W },
                Guid = new Guid(XxHash128.Hash(Encoding.UTF8.GetBytes(instance.GlobalId))).ToString(),
                Type = instance.Class,
                Color = new dotbim.Color {
                    R = (int)(instance.Rgba >> 24 & 0xFF), G = (int)(instance.Rgba >> 16 & 0xFF),
                    B = (int)(instance.Rgba >> 8 & 0xFF), A = (int)(instance.Rgba & 0xFF),
                },
                Info = new Dictionary<string, string> { ["globalId"] = instance.GlobalId, ["name"] = instance.Name },
            };
        }).ToList();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bim");
        try {
            new dotbim.File { SchemaVersion = "1.1.0", Meshes = meshes, Elements = elements, Info = new Dictionary<string, string>() }.Save(path);
            return File.ReadAllBytes(path);
        } finally { File.Delete(path); }
    }

    // USD emit through UniversalSceneDescription — the `usd-stage` codec. One UsdStage authors a UsdGeomMesh
    // prim (points VtVec3fArray, faceVertexCounts/Indices VtIntArray through the typed-array Set seam), exports
    // to the temp path, and reads the bytes; a Scene payload flattens (per-prim element authoring over
    // UsdGeomXformable.AddXformOp is the admission-gated growth); USD is a scene-graph peer, never re-deriving
    // the BIM semantics. The usdz row is import-only: the binding ships no .usdz packaging member.
    static byte[] UsdBytes(InterchangeFormat format, ExportPayload payload) {
        var geometry = payload.Flat();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{format.Extensions.Head.IfNone(".usd")}");
        try {
            using var stage = UsdStage.CreateNew(path);
            var mesh = UsdGeomMesh.Define(stage, new SdfPath("/Bim"));
            var verts = geometry.Vertices.Span;
            var points = new VtVec3fArray((uint)geometry.VertexCount);
            for (int v = 0; v < geometry.VertexCount; v++) { points[v] = new GfVec3f(verts[v * 3], verts[v * 3 + 1], verts[v * 3 + 2]); }
            mesh.GetPointsAttr().Set(new VtValue(points), UsdTimeCode.Default());
            var counts = new VtIntArray((uint)geometry.TriangleCount);
            var corners = new VtIntArray((uint)(geometry.TriangleCount * 3));
            var indices = geometry.Indices.Span;
            for (int t = 0; t < geometry.TriangleCount; t++) {
                counts[t] = 3;
                corners[t * 3] = (int)indices[t * 3]; corners[t * 3 + 1] = (int)indices[t * 3 + 1]; corners[t * 3 + 2] = (int)indices[t * 3 + 2];
            }
            mesh.GetFaceVertexCountsAttr().Set(new VtValue(counts), UsdTimeCode.Default());
            mesh.GetFaceVertexIndicesAttr().Set(new VtValue(corners), UsdTimeCode.Default());
            stage.Save();
            return File.ReadAllBytes(path);
        } finally { File.Delete(path); }
    }

    // IFC egress is the seam's Projection/egress#IFC_EGRESS SemanticProjector.Emit — the ONE Bim-internal
    // ElementGraph->DatabaseIfc re-author (the PredefinedType egress gate + schema span [C6][H8], the 1:1 GlobalId
    // round-trip [H6], the diff-derived OwnerHistory ChangeAction against `prior` [H9], the material/classification/
    // relationship re-author). This rail OWNS only the CanExport capability gate, the format#FORMAT_AXIS
    // InterchangeFormat.Serialization column read (None IS the non-IFC-row rejection — the retired SerializationOf
    // ladder as row data), and the ExportArtifact content-key seal; the hand-rolled IfcBuildingElementProxy re-author
    // is the DELETED form (a lossy second IFC-egress owner). The app wires the projector (the seam owns Assemble,
    // the app the wiring); `prior` drives the ChangeAction diff, and the profile store is the projector's
    // ingress-part capture-promoted field — Emit is the frozen 4-arg egress surface, never a re-passed parameter.
    public static Fin<ExportArtifact> ExportIfc(
        InterchangeFormat format, ElementGraph graph, SemanticProjector projector,
        InterchangePolicy policy, ClockPolicy clocks, Option<ElementGraph> prior, Op key) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-unsupported:{format.Key}"))
        : format.Serialization.Match(
            None: () => Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"ifc-export-codec-miss:{format.Key}")),
            Some: serialization => projector.Emit(graph, serialization, key, prior)
                .Map(text => Sealed(format, Encoding.UTF8.GetBytes(text), policy, clocks.Now)));

    // The SharpGLTF/Draco/meshopt encode funnels its native faults onto the rail (Draco.Encode raises DrakoException,
    // ToGltf2/WriteGLB raise SharpGLTF ModelException) so a malformed-geometry or compression fault lifts BARE as a typed
    // BimFault.CodecReject(key) — symmetric with the bim/scene/usd arms — never escaping the Fin<T> rail as an uncaught
    // exception; RegisterExtensions keeps its own registration rail ahead of the encode. The raw Draco/meshopt streams
    // carry no scene graph, so BOTH payload cases flatten there; the container arm routes Soup through the single-mesh
    // SceneOf and Scene through the per-element Staged author.
    static Fin<byte[]> GlbBytes(ExportPayload payload, InterchangePolicy policy, Op key) =>
        RegisterExtensions(policy, key).Bind(_ => Try.lift(() => policy.Compression switch {
            KhrEncoder.Draco => DracoBytes(payload.Flat(), policy),
            KhrEncoder.Meshopt => MeshoptBytes(payload.Flat(), policy),
            KhrEncoder.None => WriteGlb(payload.Switch(
                     soup:  s => SceneOf(s.Geometry, policy),
                     scene: s => Staged(s.Elements, policy).Model), policy),
            var unknown => throw new NotSupportedException($"<khr-encoder-unrouted:{unknown}>"),   // a new encoder faults LOUD, never a silent uncompressed container
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"gltf-export:{error.Message}")));

    static ModelRoot SceneOf(ImportedGeometry geometry, InterchangePolicy policy) {
        var scene = new SceneBuilder();
        scene.AddRigidMesh(MeshOf(geometry, new MaterialBuilder("default").WithMetallicRoughnessShader(), Option<int>.None), AffineTransform.Identity);
        return scene.ToGltf2(new SceneBuilderSchema2Settings { UseStridedBuffers = policy.StridedBuffers });
    }

    // The Toolkit custom-attribute fragment (api-sharpgltf IVertexCustom, the vertex-fragment seam): ONE float
    // custom attribute named "_FEATURE_ID_0" — the per-vertex EXT_mesh_features row stamp — on a zero-channel
    // IVertexMaterial face. <fragment members: the IVertexCustom/IVertexMaterial contract surface, spelled at
    // realization against SharpGLTF.Toolkit 1.0.6>
    readonly struct FeatureVertex(int row) : IVertexMaterial, IVertexCustom { /* _FEATURE_ID_0 = row */ }

    // The ONE triangle-soup MeshBuilder build the soup scene and the per-element pool share — the vertex layout
    // is an Option discriminant, never a sibling builder pair: Some(row) stamps every vertex's _FEATURE_ID_0
    // with the element's property-table row through FeatureVertex, None emits the bare position+normal layout
    // the flat-soup rows use.
    static IMeshBuilder<MaterialBuilder> MeshOf(ImportedGeometry geometry, MaterialBuilder material, Option<int> feature) =>
        feature.Match<IMeshBuilder<MaterialBuilder>>(
            Some: row => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, FeatureVertex, VertexEmpty>(geometry.Format.Key), geometry, material, _ => new FeatureVertex(row)),
            None: () => Filled(new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>(geometry.Format.Key), geometry, material, static _ => default));

    static MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> Filled<TvM>(
        MeshBuilder<MaterialBuilder, VertexPositionNormal, TvM, VertexEmpty> mesh, ImportedGeometry geometry, MaterialBuilder material, Func<int, TvM> slot)
        where TvM : struct, IVertexMaterial {
        var primitive = mesh.UsePrimitive(material);
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            primitive.AddTriangle(
                Vertex(verts, normals, (int)indices[tri * 3], slot),
                Vertex(verts, normals, (int)indices[tri * 3 + 1], slot),
                Vertex(verts, normals, (int)indices[tri * 3 + 2], slot));
        }
        return mesh;
    }

    static VertexBuilder<VertexPositionNormal, TvM, VertexEmpty> Vertex<TvM>(ReadOnlySpan<float> verts, ReadOnlySpan<float> normals, int index, Func<int, TvM> slot)
        where TvM : struct, IVertexMaterial {
        int v = index * 3;
        return new VertexBuilder<VertexPositionNormal, TvM, VertexEmpty>(
            new VertexPositionNormal(verts[v], verts[v + 1], verts[v + 2], normals[v], normals[v + 1], normals[v + 2]), slot(index));
    }

    static byte[] WriteGlb(ModelRoot model, InterchangePolicy policy) {
        if (policy.MergeBuffers) { model.MergeBuffers(); }
        return model.WriteGLB(new WriteSettings { MergeBuffers = policy.MergeBuffers }).ToArray();
    }

    // The two ToArray copies are BOUNDARY-FORCED exact-length arrays: PointAttribute.Wrap couples its point count
    // to array.Length, so a pool rent (pow2-oversized backing) would corrupt the attribute count — the pooled
    // staging law applies to the count-explicit meshopt kernel below, never to this length-coupled vendor seam.
    static byte[] DracoBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var mesh = new DracoMesh { NumPoints = geometry.VertexCount };
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Position, 3, geometry.Vertices.ToArray()));
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Normal, 3, geometry.Normals.ToArray()));
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            mesh.AddFace([(int)indices[tri * 3], (int)indices[tri * 3 + 1], (int)indices[tri * 3 + 2]]);
        }
        mesh.DeduplicateAttributeValues();   // the corner-expanded soup repeats each value 3-6x; collapse before encode per the catalogue law
        return Draco.Encode(mesh, new DracoEncodeOptions {
            PositionBits = policy.QuantizationBits, NormalBits = policy.QuantizationBits,
            CompressionLevel = DracoCompressionLevel.Optimal,
        });
    }

    // Pooled staging end to end: every transient buffer rents through SpanOwner<T> (the meshopt pinned-pointer
    // surface takes EXPLICIT counts, never array lengths, so pool-oversized rents are safe) and only the final
    // self-delimiting frame allocates — the eight per-export staging arrays were the LOH churn the admitted
    // CommunityToolkit.HighPerformance owner deletes.
    static unsafe byte[] MeshoptBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        using var soupOwner = SpanOwner<VertexPositionNormal>.Allocate(geometry.VertexCount);
        Span<VertexPositionNormal> soup = soupOwner.Span;
        for (int v = 0; v < geometry.VertexCount; v++) {
            int o = v * 3;
            soup[v] = new VertexPositionNormal(verts[o], verts[o + 1], verts[o + 2], normals[o], normals[o + 1], normals[o + 2]);
        }
        using var soupIndexOwner = SpanOwner<uint>.Allocate(geometry.TriangleCount * 3);
        Span<uint> soupIndices = soupIndexOwner.Span;
        for (int i = 0; i < soupIndices.Length; i++) { soupIndices[i] = (uint)geometry.Indices.Span[i]; }
        nuint vertSize = (nuint)Unsafe.SizeOf<VertexPositionNormal>();
        nuint soupCount = (nuint)geometry.VertexCount;
        nuint indexCount = (nuint)soupIndices.Length;
        using var remapOwner = SpanOwner<uint>.Allocate(geometry.VertexCount);
        Span<uint> remap = remapOwner.Span;
        nuint uniqueCount;
        fixed (uint* remapPtr = remap)
        fixed (uint* idxPtr = soupIndices)
        fixed (VertexPositionNormal* vSoup = soup) {
            uniqueCount = Meshopt.GenerateVertexRemap(remapPtr, idxPtr, indexCount, vSoup, soupCount, vertSize);
        }
        using var remappedOwner = SpanOwner<VertexPositionNormal>.Allocate((int)uniqueCount);
        using var interleavedOwner = SpanOwner<VertexPositionNormal>.Allocate((int)uniqueCount);
        using var indexOwner = SpanOwner<uint>.Allocate((int)indexCount);
        Span<VertexPositionNormal> remapped = remappedOwner.Span;
        Span<VertexPositionNormal> interleaved = interleavedOwner.Span;
        Span<uint> indices = indexOwner.Span;
        fixed (uint* remapPtr = remap)
        fixed (uint* idxSrc = soupIndices)
        fixed (uint* idxDst = indices)
        fixed (VertexPositionNormal* vSoup = soup)
        fixed (VertexPositionNormal* vRemap = remapped)
        fixed (VertexPositionNormal* vDstI = interleaved) {
            Meshopt.RemapVertexBuffer(vRemap, vSoup, soupCount, vertSize, remapPtr);
            Meshopt.RemapIndexBuffer(idxDst, idxSrc, indexCount, remapPtr);
            Meshopt.OptimizeVertexCache(idxDst, idxDst, indexCount, uniqueCount);
            Meshopt.OptimizeOverdraw(idxDst, idxDst, indexCount, (float*)vRemap, uniqueCount, vertSize, 1.05f);
            Meshopt.OptimizeVertexFetch(vDstI, idxDst, indexCount, vRemap, uniqueCount, vertSize);
        }
        using var vOwner = SpanOwner<byte>.Allocate((int)Meshopt.EncodeVertexBufferBound(uniqueCount, vertSize));
        using var iOwner = SpanOwner<byte>.Allocate((int)Meshopt.EncodeIndexBufferBound(indexCount, uniqueCount));
        Span<byte> vBuffer = vOwner.Span;
        Span<byte> iBuffer = iOwner.Span;
        nuint vLen, iLen;
        fixed (byte* vDst = vBuffer)
        fixed (byte* iDst = iBuffer)
        fixed (VertexPositionNormal* vSrc = interleaved)
        fixed (uint* iSrc = indices) {
            vLen = Meshopt.EncodeVertexBuffer(vDst, (nuint)vBuffer.Length, vSrc, uniqueCount, vertSize);
            iLen = Meshopt.EncodeIndexBuffer(iDst, (nuint)iBuffer.Length, iSrc, indexCount);
        }
        // Self-delimiting frame: (uniqueCount, indexCount, vLen, iLen) little-endian int32 header, then the two
        // encoded streams — DecodeVertexBuffer/DecodeIndexBuffer each need their element count and byte length,
        // so a frame omitting indexCount/iLen is undecodable (the deleted form).
        return [
            .. BitConverter.GetBytes((int)uniqueCount), .. BitConverter.GetBytes((int)indexCount),
            .. BitConverter.GetBytes((int)vLen), .. BitConverter.GetBytes((int)iLen),
            .. vBuffer[..(int)vLen], .. iBuffer[..(int)iLen],
        ];
    }

    static Fin<Unit> RegisterExtensions(InterchangePolicy policy, Op key) =>
        policy.Extensions.Traverse(khr => khr.Register(key)).As().Map(static _ => unit);

    // The artifact content key: the kernel seed-zero ContentHash over the seam CanonicalWriter fold of the format
    // key, the quality triple, and the emitted bytes — the one-hasher law every sibling key observes (reconstruct/
    // tessellation); minting through the Rasm.Compute InterchangeIdentity was the deleted downward strata reference.
    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes,
            ContentHash.Of(new CanonicalWriter(0.0)
                .String(format.Key).Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)
                .Raw(bytes).ToBytes().Span),
            bytes.LongLength, at);
}
```

## [03]-[TILE_METADATA]

- Owner: `TileMetadata` the per-tile `EXT_structural_metadata` author over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, never a stored record) — one embedded schema carrying the element's `Classification` code, `ExternalId` GlobalId, name, and (as growth) the baked property/quantity columns, one `PropertyTable` per-feature value store, and the `EXT_mesh_features` feature-ID binding tying each GLB primitive vertex span to its element row so the Cesium 3D Tiles web peer resolves per-element metadata at pick time.
- Entry: `TileMetadata.Attach(GlbScene scene, Seq<Element> elements, Op key)` authors the structural-metadata schema/class/property-table on the `Author`-minted GLB scene — the feature-ID VALUES are already in the model (the per-vertex `_FEATURE_ID_0` stamps `Staged` authored through the `FeatureVertex` fragment), so `Attach` only defines the schema and binds the table, never re-walking or re-stamping geometry; `Fin<T>` aborts on a registration fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the per-tile metadata emit composes through the `Rasm.Compute` interchange codec `TILE_PARTITION` at the seam and `Rasm.Bim` authors the canonical schema shape and the extension surface.
- Auto: `Attach` runs `Tiles3DExtensions.RegisterExtensions()` once, opens the schema through `scene.Model.UseStructuralMetadata().UseEmbeddedSchema(id)`, defines the `Element` class through `UseClassMetadata("Element")` with one `UseProperty(name).With<Type>(...)` per canonical column (`GlobalId` string off `Element.ExternalId`, `Class` enumeration over the `IfcClass` vocabulary through `UseEnumMetadata` off `Element.Classification.Code`, `Name` string, and as growth the baked-Pset property columns off `Element.Properties`), adds the per-feature `PropertyTable` through `AddPropertyTable(class, featureCount, name)` with ROWS ORDERED BY the `GlbScene.Rows` instance ordinals — the one row space the `Staged` vertex stamps already index — encoding each column through `UseProperty(key).SetValues<T>(...)` (element semantics joined by the seam GlobalId; an element-less row carries empty strings and the `Class` column's reserved `Unclassified` noData sentinel — a bare `IfNone(0)` silently claimed the first REAL `IfcClass` row), then binds ONE `new FeatureIDBuilder(featureCount, attributeOrTexture: 0, propertyTable, ...)` per DISTINCT logical mesh through `primitive.AddMeshFeatureIds(builder)` with `nullFeatureId = Rows.Count` so a shared-mesh repeat's null-row stamps resolve to "no feature" at pick rather than a wrong row.
- Receipt: the authored `EXT_structural_metadata` schema and `PropertyTable` are the per-tile semantic the web peer reads — the same seam `Element` vocabulary a consumer reads at the `Exchange/wire#WIRE_PROJECTION`, projected onto the binary tile metadata so a Cesium consumer resolves per-element BIM semantics at pick without a second metadata mint.
- Packages: SharpGLTF.Core, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new metadata column is one `UseProperty(name).With<Type>(...)` row on the embedded class fed from a baked `Element` field; a new feature-ID binding is one `FeatureIDBuilder` over the primitive; the `IfcClass` enumeration is one `UseEnumMetadata` row tracking the `IfcClass` vocabulary; never a hand-authored JSON metadata block and never a second per-tile metadata mint.
- Boundary: the per-tile metadata authors through the `SharpGLTF.Ext.3DTiles` `EXT_structural_metadata`/`EXT_mesh_features` surface — a hand-authored JSON `EXT_structural_metadata` block is the deleted form, the `StructuralMetadataClassProperty.With<Type>` selectors and the `PropertyTableProperty.SetValues<T>` binary encode own the schema and value storage; `Tiles3DExtensions.RegisterExtensions()` runs once before any author and the call is idempotent at the factory level; the per-feature semantic is the seam baked `Element` and a retired `BimElement` row crossing `Attach` is the deleted form (the element is the `Bake` fold over the `ElementGraph`, the `Classification` code resolved to the `IfcClass` enumeration, never a typed `IfcClass` on the row); the `IfcClass` column rides `UseEnumMetadata` so the closed BIM class vocabulary serializes by its enumeration rather than a free string; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` admits the extension surface and the canonical schema shape, never the tile pyramid; the `OneOf<int, Texture>` feature-ID attribute selector is a transitive `OneOf` dependency consumed only by `FeatureIDBuilder` and no Bim code references it directly; the per-tile `Element` semantic is the same vocabulary the wire projection carries, never a second metadata vocabulary.

```csharp signature
public static class TileMetadata {
    public static Fin<GlbScene> Attach(GlbScene scene, Seq<Element> elements, Op key) =>
        Try.lift(() => Author(scene, elements)).Run().MapFail(error => new BimFault.ModelRejected(key, $"tile-metadata:{error.Message}"));

    // The per-feature semantic is the seam baked `Element`: ExternalId GlobalId + the generic Classification code
    // resolved to the IfcClass enumeration + name (the Pset/Qto columns grow off the baked Element.Properties/
    // Quantities), so the tile carries the SAME vocabulary the wire projection does, never a second metadata mint.
    // Table rows are the Author-minted GlbScene.Rows instance ordinals — the SAME values Staged stamped into every
    // uniquely-meshed vertex's _FEATURE_ID_0 — so the feature-ID attribute and the property table index one row
    // space by construction; the element semantics join by GlobalId, a Rows entry with no baked element carries
    // empty columns (its stamp resolves, its semantics ride the wire). ONE FeatureIDBuilder binds per DISTINCT
    // logical mesh (the extension lives on the primitive — a per-node re-bind would duplicate featureId sets on a
    // shared primitive); a shared mesh's null-row stamps resolve to nullFeatureId = Rows.Count, so a non-merged
    // repeat picks as "no feature" rather than mislabeling — per-element identity on repeats is the
    // GpuInstancingMinCount + EXT_instance_features arm, never a silent wrong row.
    static GlbScene Author(GlbScene scene, Seq<Element> elements) {
        Tiles3DExtensions.RegisterExtensions();
        var byExternal = elements.Choose(static e => e.ExternalId.Map(ext => (ext, e))).ToMap();
        var slots = scene.Rows.AsIterable().OrderBy(static pair => pair.Value).Select(pair => byExternal.Find(pair.Key)).ToSeq();
        var root = scene.Model.UseStructuralMetadata();
        var schema = root.UseEmbeddedSchema("rasm-element");
        var classIndex = IfcClass.Items.Select(static (row, i) => (row.Key, i)).ToMap();
        // The enum table carries a reserved noData sentinel: an element-less row encodes -1 and picks as "no
        // class" — IfNone(0) silently claimed the FIRST real IfcClass row under noData: null, the deleted defect.
        var classKinds = schema.UseEnumMetadata("IfcClass", [("Unclassified", -1), .. classIndex.AsIterable().Map(static pair => (pair.Key, pair.Value))]);
        var elementClass = schema.UseClassMetadata("Element");
        elementClass.UseProperty("GlobalId").WithStringType(noData: null, defaultValue: null);
        elementClass.UseProperty("Class").WithEnumeration(classKinds, noData: "Unclassified");
        elementClass.UseProperty("Name").WithStringType(noData: null, defaultValue: null);
        var table = root.AddPropertyTable(elementClass, slots.Count, "elements");
        table.UseProperty("GlobalId").SetValues([.. slots.Map(static slot => slot.Bind(static e => e.ExternalId).IfNone(""))]);
        table.UseProperty("Class").SetValues([.. slots.Map(slot => slot.Bind(e => classIndex.Find(e.Classification.Code)).IfNone(-1))]);
        table.UseProperty("Name").SetValues([.. slots.Map(static slot => slot.Map(static e => e.Name).IfNone(""))]);
        var features = new FeatureIDBuilder(slots.Count, attributeOrTexture: 0, propertyTable: table, channels: null, label: "elements", nullFeatureId: slots.Count);
        scene.Nodes.Values.AsIterable().Choose(static n => Optional(n.Mesh)).Distinct()
            .Iter(mesh => mesh.Primitives.Iter(primitive => primitive.AddMeshFeatureIds(features)));
        return scene;
    }
}
```

## [04]-[BIM_LOD]

- Owner: `BimLod` the per-element LOD-pyramid leg ADDITIVE to the export rail — one progressive-detail chain per element derived through the catalogued `Meshopt.Simplify`/`SimplifySloppy` decimation keyed by target triangle ratio, plus the `MeshletResidency` band through `Meshopt.BuildMeshlets` for the WebGPU raster path; `LodLevel` the per-level record carrying the decimated index buffer, the target ratio, and the per-LOD content key the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses.
- Entry: `BimLod.Pyramid(ImportedGeometry geometry, InterchangePolicy policy, Op key)` derives the LOD chain over the policy's ratio schedule (each level a `Meshopt.Simplify` at decreasing target index count, falling back to `Meshopt.SimplifySloppy` when the error threshold cannot be met), and `BimLod.Meshlets(ImportedGeometry geometry, Op key)` clusters the residency band through `Meshopt.BuildMeshlets` (bounded by `Meshopt.BuildMeshletsBound`, optimized per meshlet through `Meshopt.OptimizeMeshlet`) — `Fin<T>` aborts on a degenerate decimation captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; each level seals its own `ExportArtifact.ContentKey` so the web peer streams each LOD by view distance, the `TileMetadata` per-tile semantic riding each level unchanged.
- Receipt: each `LodLevel` carries its target ratio, resulting triangle count, the world-space `WorldError` deviation (`Meshopt.Simplify`'s relative `result_error` × `SimplifyScale` — the solver evidence the typed-receipt law keeps, a discarded `out` error is the deleted form), and the per-LOD content key — the same `InterchangeIdentity` the full-resolution `ExportArtifact` seals, computed per level so the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses every detail level and the cross-libs `WEB_GEOMETRY_RESIDENCY_WIRE` splat/meshlet manifest the AppUi projection mints streams each LOD by view distance against a real per-level error bound.
- Packages: Alimer.Bindings.MeshOptimizer, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new detail level is one ratio on the `InterchangePolicy.LodRatios` policy column (the schedule is policy data, never a fence-local constant), each landing one content-keyed `LodLevel` row on the pyramid; a new meshlet bound is one `MeshletResidency` band over the residency set; the per-tile `TileMetadata` semantic rides each LOD unchanged; never a per-element full-resolution emit and never a second LOD or residency owner.
- Boundary: the LOD decimation is `Alimer.Bindings.MeshOptimizer`'s — `Meshopt.Simplify` (error-threshold decimation with `SimplificationOptions` flags) and `Meshopt.SimplifySloppy` (aggressive fallback) over the optimized indexed buffer own the LOD chain, and a hand-rolled edge-collapse decimator is the deleted form; the meshlet residency rides `Meshopt.BuildMeshlets` (allocated via `BuildMeshletsBound`, optimized per meshlet via `OptimizeMeshlet`) so the WebGPU raster path consumes the package-owned meshlet partition, never a hand-rolled cluster algorithm; the per-LOD content key meets `csharp:Rasm.Compute#TILE_PARTITION` at the seam — `Rasm.Bim` derives the per-element pyramid and seals each level's content key, the tile-pyramid partitioning and streaming stay at Compute consumed at the seam; the residency band feeds the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest the AppUi projection mints, never a second residency owner; the LOD leg composes the same `ImportedGeometry` triangle-soup the `EXPORT_RAIL` `SceneOf` reads, never a second geometry carrier.

```csharp signature
public sealed record LodLevel(int Level, double TargetRatio, int TriangleCount, double WorldError, ReadOnlyMemory<uint> Indices, UInt128 ContentKey);

public static class BimLod {
    public static Fin<Seq<LodLevel>> Pyramid(ImportedGeometry geometry, InterchangePolicy policy, Op key) =>
        Try.lift(() => Levels(geometry, policy)).Run().MapFail(error => new BimFault.ModelRejected(key, $"lod-decimate:{error.Message}"));

    static unsafe Seq<LodLevel> Levels(ImportedGeometry geometry, InterchangePolicy policy) {
        var source = new uint[geometry.Indices.Length];
        for (int i = 0; i < source.Length; i++) { source[i] = (uint)geometry.Indices.Span[i]; }
        var verts = geometry.Vertices.ToArray();
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(3 * sizeof(float));
        float scale;
        fixed (float* vPtr = verts) { scale = Meshopt.SimplifyScale(vPtr, vertexCount, vertexStride); }
        return policy.LodRatios.Select((ratio, level) => Decimate(source, verts, vertexCount, vertexStride, scale, ratio, level, geometry.Format.Key, policy)).ToSeq();
    }

    // target_error is RELATIVE to mesh extents under SimplificationOptions.None (0.01 = 1% deformation);
    // SimplifyScale is the relative->world conversion factor for the RECEIPT, never a budget multiplier —
    // `0.01f * scale` passed a world-sized budget where a fraction belongs, so every level hit its count under
    // unbounded deformation (the deleted defect). The solver's resultError lands on the receipt as WorldError = error x scale,
    // the per-level deviation the streaming consumer selects LODs by.
    static unsafe LodLevel Decimate(uint[] source, float[] verts, nuint vertexCount, nuint vertexStride, float scale, double ratio, int level, string formatKey, InterchangePolicy policy) {
        nuint sourceCount = (nuint)source.Length;
        nuint targetCount = (nuint)((long)source.Length * ratio);
        var destination = new uint[source.Length];
        nuint resultCount;
        float resultError;
        fixed (uint* dst = destination)
        fixed (uint* src = source)
        fixed (float* vPtr = verts) {
            resultCount = Meshopt.Simplify(dst, src, sourceCount, vPtr, vertexCount, vertexStride, targetCount, 0.01f, SimplificationOptions.None, &resultError);
            if (resultCount > targetCount) {
                resultCount = Meshopt.SimplifySloppy(dst, src, sourceCount, vPtr, vertexCount, vertexStride, (byte*)null, targetCount, 0.05f, &resultError);
            }
        }
        var indices = destination.AsSpan(0, (int)resultCount).ToArray();
        var bytes = MemoryMarshal.AsBytes(indices.AsSpan());
        return new LodLevel(level, ratio, (int)resultCount / 3, resultError * scale, indices,
            ContentHash.Of(new CanonicalWriter(0.0)
                .String($"{formatKey}:lod{level}").Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)
                .Raw(bytes).ToBytes().Span));
    }

    public static unsafe Fin<Seq<Meshlet>> Meshlets(ImportedGeometry geometry, Op key) =>
        Try.lift(() => Cluster(geometry)).Run().MapFail(error => new BimFault.ModelRejected(key, $"meshlet-build:{error.Message}"));

    static unsafe Seq<Meshlet> Cluster(ImportedGeometry geometry) {
        var indices = new uint[geometry.Indices.Length];
        for (int i = 0; i < indices.Length; i++) { indices[i] = (uint)geometry.Indices.Span[i]; }
        var verts = geometry.Vertices.ToArray();
        nuint indexCount = (nuint)indices.Length;
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(3 * sizeof(float));
        const nuint maxVertices = 64, maxTriangles = 124;
        nuint bound = Meshopt.BuildMeshletsBound(indexCount, maxVertices, maxTriangles);
        var meshlets = new Meshlet[(int)bound];
        var meshletVertices = new uint[(int)bound * (int)maxVertices];
        var meshletTriangles = new byte[(int)bound * (int)maxTriangles * 3];
        nuint count;
        fixed (Meshlet* mPtr = meshlets)
        fixed (uint* mvPtr = meshletVertices)
        fixed (byte* mtPtr = meshletTriangles)
        fixed (uint* iPtr = indices)
        fixed (float* vPtr = verts) {
            count = Meshopt.BuildMeshlets(mPtr, mvPtr, mtPtr, iPtr, indexCount, vPtr, vertexCount, vertexStride, maxVertices, maxTriangles, 0.0f);
            for (nuint m = 0; m < count; m++) { Meshopt.OptimizeMeshlet(&mvPtr[mPtr[m].vertex_offset], &mtPtr[mPtr[m].triangle_offset], mPtr[m].triangle_count, mPtr[m].vertex_count); }
        }
        return meshlets.AsSpan(0, (int)count).ToArray().ToSeq();
    }
}
```

## [05]-[SCHEDULE_ANIMATION]

- Owner: `ScheduleAnimation` the 4D-emit leg ADDITIVE to the export rail — one glTF `Animation` baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element keyframe tracks: each `ConstructionTask`'s scheduled `Interval` drives a per-element visibility track (the element is invisible before its task starts and visible from its task start) plus an optional scale track (the element grows from a zero-scale point to its full scale across its task window) so a viewer scrubs the construction sequence on the GLB timeline; `AnimationTrack` the per-element keyframe record carrying the element `GlobalId`, the appear-time and full-time seconds, and the glTF `Node` the element's mesh binds to.
- Entry: `BimExport.AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy, Op key)` bakes the schedule into the scene model's animation set — projecting each `ConstructionTask` `Interval` bound onto its glTF time-in-seconds through `policy.SecondsOf(Instant moment, Instant projectStart)` (the bound mapped to the timeline via the NodaTime `Duration` from the project start, scaled by `policy.SecondsPerDay`), resolving each assigned element's glTF `Node` through the `Author`-minted `GlbScene` `GlobalId→Node` index (the element `GlobalId` is the seam `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`; `Author` names each node by it, so the 4D leg binds the scene actually emitted — the retired caller-supplied index parameter is GONE), and authoring one `KHR_node_visibility` visibility channel (and the `policy.Grow` scale channel when set) per element through the SharpGLTF `Animation.CreateVisibilityChannel`/`CreateScaleChannel` keyframe surface — `Fin<T>` aborts on a SharpGLTF authoring fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the animation and the `Planning/schedule#SCHEDULE` `ConstructionState.At` snapshot share one `Interval`-to-`Instant` time axis so a scrub at glTF time `t` shows exactly the element set `ConstructionState.At` resolves at the inverse instant (the schedule owner is the `BimModel`→`ElementGraph` cross-file alignment point its rebuild settles).
- Auto: `AnimateSchedule` registers `KHR_node_visibility` on its `Fin` rail, creates one `Animation` through `scene.Model.CreateAnimation("construction-sequence")`, folds the network's tasks into per-element `AnimationTrack` rows (a task's `Interval.Start` is the element's appear time, `Interval.End` its full time, unioned over every task that assigns the element so a multi-task element appears at its earliest task), and authors each element's keyframe dictionary: the visibility track is `{ appearTime − ε: false, appearTime: true }` so the element pops in at its task start under the `STEP` interpolation the `bool` channel forces, and the optional scale track is `{ appearTime: Vector3.Zero, fullTime: Vector3.One }` under `LINEAR` interpolation so the element grows across its task window; `policy.SecondsOf(moment, projectStart)` projects an `Interval` bound onto the float seconds axis through `(moment − projectStart).TotalDays × SecondsPerDay`, the one time axis the `ConstructionState.At` snapshot reads, so the keyframe author and the snapshot never carry two clocks; the scene returns with its `LogicalAnimations` populated so the downstream `Emit` seals the animated GLB and the `TileMetadata` per-element semantic rides each frame unchanged.
- Receipt: the `Seq<AnimationTrack>` is the 4D-emit evidence — each row carries the element `GlobalId`, the appear/full seconds, and the bound `Node` logical index so the Cesium/three.js timeline scrub resolves the construction state at any timeline instant; the animated GLB the `WriteGlb` emits is the streamed 4D timeline a web viewer plays, the `Planning/schedule#SCHEDULE` `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keying the animation only on a re-sequenced plan.
- Packages: SharpGLTF.Core, SharpGLTF.Runtime, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new keyframe channel (a translation track lowering an element into place, a color track tinting the active task) is one `Animation.Create*Channel` arm on the same fold; a new interpolation mode rides the SharpGLTF `bool linear` channel knob; a new time-axis policy is one column on `ScheduleAnimationPolicy`; never a per-element `Animation` instance, never a hand-authored glTF animation JSON block, and never a second time axis beside the `ConstructionState.At` `Interval`.
- Boundary: the keyframe author rides the SharpGLTF `ModelRoot.CreateAnimation` + `Animation.CreateVisibilityChannel`/`CreateScaleChannel`/`CreateTranslationChannel` keyframe surface (`.api/api-sharpgltf` `ModelRoot.CreateAnimation` entrypoint 07, `Animation`/`AnimationChannel` rows 14-15, `AnimationInterpolationMode` row 07) — a hand-authored glTF `animations[]`/`samplers[]`/`channels[]` JSON block is the deleted form, the `Create*Channel(Node, IReadOnlyDictionary<float, T> keyframes, bool linear)` overloads own the sampler/channel/accessor emission and the keyframe dictionary is the float-seconds→value map; the `KHR_node_visibility` extension drives the per-element visibility keyframe so the `bool` track is the settled `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` row, registered once through the factory — a custom visibility-by-opacity hack is the deleted form; the SharpGLTF.Runtime pin (already csproj-referenced, exercised today only for `IMeshDecoder`/`SceneTemplate` decode) is the load-bearing surface this leg exploits, no new package and no new `InterchangeFormat` row; the animation time axis is the `Planning/schedule#SCHEDULE` `ConstructionTask.Interval` projected to seconds and a second clock on the export side is the named seam violation — the `ConstructionState.At` snapshot and the keyframe author read the one `Interval`-to-`Instant` axis; the per-element glTF `Node` resolves through the `Author`-minted `GlbScene` index (nodes NAMED by the seam `Object.ExternalId`) — a caller-supplied index parameter, a re-walked scene graph, or a second index mint in this leg is the deleted form; a 4D-emit fault lowers onto `Model/faults#FAULT_BAND` `BimFault` through the `Try.lift(...).Run().MapFail(...)` funnel.

```csharp signature
public sealed record ScheduleAnimationPolicy(double SecondsPerDay, bool Grow, double EpsilonSeconds) {
    public static readonly ScheduleAnimationPolicy Default = new(SecondsPerDay: 1.0, Grow: false, EpsilonSeconds: 0.001);
    public static readonly ScheduleAnimationPolicy Growing = Default with { Grow = true };

    public float SecondsOf(Instant moment, Instant projectStart) =>
        (float)((moment - projectStart).TotalDays * SecondsPerDay);
}

public sealed record AnimationTrack(string GlobalId, float AppearSeconds, float FullSeconds, int NodeIndex);

public static partial class BimExport {
    // The element GlobalId->glTF Node index is the Author-minted GlbScene (nodes named by the seam
    // Object.ExternalId): this leg binds keyframes onto the scene actually emitted — never a caller-supplied
    // index, never a re-walked scene graph, never a retired BimModel.
    public static Fin<Seq<AnimationTrack>> AnimateSchedule(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy, Op key) =>
        KhrExtension.NodeVisibility.Register(key).Bind(_ =>
            Try.lift(() => Tracks(scene, network, policy)).Run()
                .MapFail(error => new BimFault.ModelRejected(key, $"schedule-animation:{error.Message}")));

    static Seq<AnimationTrack> Tracks(GlbScene scene, ScheduleNetwork network, ScheduleAnimationPolicy policy) {
        var projectStart = network.Tasks.Map(static t => t.Scheduled.Start).Min();
        var animation = scene.Model.CreateAnimation("construction-sequence");
        // Tasks index ONCE by GlobalId — the per-assignment Tasks.Find linear scan was O(assignments·tasks).
        var taskWindow = network.Tasks.Fold(Map<string, Interval>(), static (held, task) => held.TryAdd(task.GlobalId, task.Scheduled));
        var windows = network.Assignments
            .Bind(a => taskWindow.Find(a.TaskGlobalId)
                .Map(window => a.ElementGlobalIds.Map(id => (Id: id, Window: window))).IfNone(Seq<(string, Interval)>()))
            .GroupBy(static row => row.Item1)
            .Select(static g => (g.Key, g.Select(static row => row.Item2).ToSeq()))
            .ToMap();
        return windows
            .Choose(pair => scene.Nodes.Find(pair.Key)
                .Map(node => Track(animation, node, pair.Key, pair.Value, projectStart, policy)))
            .ToSeq();
    }

    static AnimationTrack Track(Animation animation, SharpGLTF.Schema2.Node node, string globalId, Seq<Interval> windows, Instant projectStart, ScheduleAnimationPolicy policy) {
        float appear = windows.Map(w => policy.SecondsOf(w.Start, projectStart)).Min();
        float full = windows.Map(w => policy.SecondsOf(w.End, projectStart)).Max();
        animation.CreateVisibilityChannel(node, new Dictionary<float, bool> {
            [Math.Max(0f, appear - (float)policy.EpsilonSeconds)] = false,
            [appear] = true,
        });
        if (policy.Grow) {
            animation.CreateScaleChannel(node, new Dictionary<float, Vector3> {
                [appear] = Vector3.Zero,
                [Math.Max(full, appear + (float)policy.EpsilonSeconds)] = Vector3.One,
            });
        }
        return new AnimationTrack(globalId, appear, full, node.LogicalIndex);
    }
}
```

## [06]-[ROUNDTRIP]

- Owner: `RoundTrip` the lossless verification matrix folding a seam `ElementGraph` emit→re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations into a typed `RoundTripReport` that witnesses per-element AND per-property field fidelity by the seam's structured member diff joined on the 1:1 `ExternalId` GlobalId, so the codec proves losslessness rather than asserting it; `RoundTripReport` the receipt partitioned by `InterchangeFormat` carrying the lossless-element count, the dropped-element set, and the per-element divergent-member set.
- Entry: `RoundTrip.Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, ClockPolicy clocks, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` runs the source graph through one IFC serialization and back — emitting through the `EXPORT_RAIL` `BimExport.ExportIfc` (which delegates to `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`), re-decoding the artifact bytes through the `import#IMPORT_RAIL` `BimIo.ImportIfc` (the ONE `DatabaseIfc` decode owner — its `SemanticProjector.Sniff` schema sniff constructs the ifcXML/ifcJSON database at the EMITTED `ReleaseVersion`, so the reimport lands at the schema the export wrote, never the GeometryGym default [H8]; a page-local `new DatabaseIfc()` re-decode is the deleted form), re-projecting through a fresh `SemanticProjector(db, reconciler, profiles)` and folding the delta onto a `Genesis(source.Header)` seed through the seam `Projection/projection#PROJECTION` `ProjectionAssembly.Assemble` (the `IfcLegality` constraint admitting the re-imported edges), then comparing the source and reimported graphs by baked-element member diff — `Fin<T>` aborts on a codec reject, a re-decode fault, or a predefined-gate reject in either leg (`Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected`/`UnmappedClass`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; `RoundTrip.Matrix(ElementGraph source, ProjectionContext ctx, ClockPolicy clocks, IIfcTypeReconciler reconciler, IIfcProfileStore profiles)` lifts the verify over the IFC STEP/XML/JSON triad (`InterchangeFormat.Ifc`/`IfcXml`/`IfcJson`) onto the per-format `Map<string, RoundTripReport>` fidelity matrix so a single call witnesses which serialization preserves which field.
- Auto: `Verify` emits the source graph through the IFC serialization, re-decodes the bytes into a live `DatabaseIfc` through `BimIo.ImportIfc`, re-projects and assembles the reimported `ElementGraph`, and folds the source-vs-reimported comparison through the seam diff: each rooted `Object` node is baked into an `Element` keyed by its stable 1:1 `ExternalId` GlobalId (the `NodeId` is freshly minted each re-ingest [H6], so the join is the GlobalId, never the id), a source GlobalId whose reimported baked `Element` carries no divergent member is a lossless element, a divergence names the changed members through `Element.EqualityComparer.Default.Inequalities` (the `Generator.Equals` structured diff, EXCLUDING the freshly-minted `Id` and the `Parts` composition paths the child element rows already own) onto the lossy-field set, and a source GlobalId absent from the reimport is a dropped element; the report reads the lossless-element count, the per-element divergent-member set (a serialization that drops a property data type or a quantity unit surfaces the exact `Properties[..].DataType`/`Quantities[..].Unit` member path), and the dropped set, the geometry leg crossing the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion; `Matrix` runs the same `Verify` over each `InterchangeFormat` triad row, the per-format reports keyed by format so a per-format fidelity comparison reads one matrix.
- Receipt: the `RoundTripReport` per format is the codec-fidelity evidence — a per-format fidelity matrix proving which serialization preserves which field, an interchange-policy losslessness witness, and a codec regression oracle; the STEP report typically reads the highest match ratio (the canonical IFC physical file), the XML/JSON reports surfacing any serialization-specific field loss, and the divergent-member set the exact members a round-trip drops.
- Packages: GeometryGymIFC_Core, Rasm.Element, Generator.Equals, LanguageExt.Core, NodaTime, Rasm
- Growth: a new serialization format is one `InterchangeFormat` row the `Matrix` triad widens to; a new fidelity dimension (a placement-key match, a coverage round-trip) is one column on `RoundTripReport` over the same baked-element diff; a new comparison basis rides the existing `Generator.Equals` `Inequalities`; never a second element-comparison surface, never a per-format report record family, and never a parallel fidelity store.
- Boundary: the round-trip fold reuses the seam's `Generator.Equals` `Inequalities` member diff as the fidelity metric rather than minting a second element-comparison surface — a field-by-field string compare or a `Seq("content")` placeholder is the deleted form, the structured diff naming the EXACT divergent member path; the cycle composes the `EXPORT_RAIL` `ExportIfc` egress (itself delegating to `SemanticProjector.Emit`) and the `import#IMPORT_RAIL` `BimIo.ImportIfc` re-decode (the schema-sniffed `DatabaseIfc` owner — a THIRD page-local decode copy beside import/wire was the deleted form, and its missing sniff mis-reported the fidelity matrix at the wrong schema) folded through the seam `ProjectionAssembly.Assemble`, never the retired `BimModel.Project`/`IfcSemanticModel` lossy-row path and never a hand-rolled IFC re-author; the join is the stable 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], and a NodeId-keyed join is the deleted form; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion, and the verification couples to no host geometry type; the `RoundTripReport` is partitioned by `InterchangeFormat` over the one baked-element diff and a per-format `StepReport`/`XmlReport`/`JsonReport` class family is the deleted form; a round-trip rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), with no `.ToError()` hop.

```csharp signature
public sealed record RoundTripReport(
    string Format,
    int SourceCount,
    int LosslessCount,
    Seq<string> DroppedGlobalIds,
    Map<string, Seq<string>> LossyFields) {
    public double MatchRatio => SourceCount == 0 ? 1.0 : (double)LosslessCount / SourceCount;
    public bool Lossless => LosslessCount == SourceCount && DroppedGlobalIds.IsEmpty;
}

public static class RoundTrip {
    static readonly Seq<InterchangeFormat> IfcTriad = Seq(InterchangeFormat.Ifc, InterchangeFormat.IfcXml, InterchangeFormat.IfcJson);

    // The lossless cycle over the SEAM graph: ExportIfc (-> SemanticProjector.Emit) seals the IFC bytes,
    // BimIo.ImportIfc re-builds the live DatabaseIfc (the import rail's ONE decode owner — Sniff-schema'd, so the
    // ifcXML/ifcJSON reimport constructs at the EMITTED ReleaseVersion [H8], where the deleted page-local
    // new DatabaseIfc() copy silently reimported at the GG default and mis-reported the matrix), a fresh
    // SemanticProjector(db, reconciler, profiles) re-projects, and ProjectionAssembly.Assemble folds the delta onto a
    // Genesis(source.Header) seed under the IfcLegality constraint, yielding (Graph, Delta) — the round-trip keeps
    // .Graph and Compare witnesses fidelity by the seam member diff. The egress projector's ctor db is unused by Emit
    // (it builds its own target from the graph header), so an empty DatabaseIfc seeds it.
    public static Fin<RoundTripReport> Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, ClockPolicy clocks, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        BimExport.ExportIfc(format, source, new SemanticProjector(new DatabaseIfc(), reconciler, profiles), InterchangePolicy.Canonical, clocks, Option<ElementGraph>.None, ctx.Key)
            .Bind(artifact => BimIo.ImportIfc(format, artifact.Bytes, ctx.Key))
            .Bind(db => ProjectionAssembly.Assemble(
                Seq<IElementProjection>(new SemanticProjector(db, reconciler, profiles)),
                Seq<IGraphConstraint>(new IfcLegality()),
                ElementGraph.Genesis(source.Header), ctx))
            .Map(static r => r.Graph)
            .Map(reimported => Compare(format.Key, source, reimported, ctx.Key));

    public static Fin<Map<string, RoundTripReport>> Matrix(ElementGraph source, ProjectionContext ctx, ClockPolicy clocks, IIfcTypeReconciler reconciler, IIfcProfileStore profiles) =>
        IfcTriad.TraverseM(format => Verify(source, format, ctx, clocks, reconciler, profiles).Map(report => (format.Key, report))).As()
            .Map(static rows => rows.ToMap());

    static RoundTripReport Compare(string formatKey, ElementGraph source, ElementGraph reimported, Op key) {
        var sourceElements = ElementsByExternal(source, key);
        var reimportedElements = ElementsByExternal(reimported, key);
        var dropped = sourceElements.Keys.Filter(id => !reimportedElements.ContainsKey(id)).ToSeq();
        var lossy = sourceElements
            .Choose(pair => reimportedElements.Find(pair.Key)
                .Map(other => (Id: pair.Key, Fields: Divergence(pair.Value, other)))
                .Filter(static row => row.Fields.IsEmpty == false))
            .ToMap(static row => row.Id, static row => row.Fields);
        return new RoundTripReport(formatKey, sourceElements.Count, sourceElements.Count - dropped.Count - lossy.Count, dropped, lossy);
    }

    // Bake every rooted Object element keyed by its stable 1:1 ExternalId GlobalId — the NodeId is freshly minted each
    // re-ingest [H6], so the join is the GlobalId, never the id; the baked Element folds in the Pset/Qto/material bags,
    // so the roundtrip witnesses FULL element fidelity (class/predefined/representations PLUS properties/quantities/materials).
    static Map<string, Element> ElementsByExternal(ElementGraph graph, Op key) =>
        graph.ObjectNodes
            .Choose(o => o.ExternalId.Bind(ext => graph.Bake(o.Id, key).ToOption().Map(element => (ext, element))))
            .ToMap();

    // The Generator.Equals member-level structured diff names the divergent members (Properties[..].FireRating,
    // Materials[0].Composition.Layers[2].Thickness) — EXCLUDING the freshly-minted Id paths (the rooted NodeId differs
    // each re-ingest but is not a fidelity loss) and the Parts composition paths (each child element owns its own row,
    // a dropped child surfacing in `dropped`), so a serialization that drops a property data type or a quantity unit
    // surfaces the EXACT member, never a "content" placeholder; lossless iff the filtered diff is empty.
    static Seq<string> Divergence(Element source, Element reimported) =>
        Element.EqualityComparer.Default.Inequalities(source, reimported)
            .ToSeq()
            .Filter(static i => i.Path.Segments switch {
                [{ Kind: MemberPathSegmentKind.Property, Value: "Parts" }, ..] => false,
                [.., { Kind: MemberPathSegmentKind.Property, Value: "Id" }]    => false,
                _                                                              => true,
            })
            .Map(static i => i.Path.ToString());
}
```

## [07]-[TILE_AVAILABILITY]

- Owner: `TileAvailability` the 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream author over the `subtree` package — the tileset AVAILABILITY structure (the Morton-ordered tile/content/child-subtree bitstreams telling a 3D-Tiles client which implicit nodes exist) the `SharpGLTF.Ext.3DTiles` `[3]-[TILE_METADATA]` per-tile CONTENT author cannot reach, the two meeting at the shared Morton tile index; `TileNode` the scheme-neutral per-tile authoring coordinate — `Lod` the subdivision level (mapped onto the quadtree `subtree.Tile.Z` level field or the octree `subtree.Tile3D.Level`), `X`/`Y` the in-level position, `Z` the octree vertical axis (unused under the `Quadtree` scheme, where `subtree.Tile` carries no spatial third axis), plus the `Available`/`ContentUri`/`GeometricError` columns the `subtree.Tile` node carries — retiring the hand-rolled implicit-tiling bitstream.
- Entry: `TileAvailability.Author(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme, Op key)` folds the tile list into the `.subtree` binary, the `scheme` discriminant selecting the authoring root — `SubtreeCreator.GenerateSubtreefile(List<Tile>)` for `Quadtree` (each `TileNode` projected through `TileOf` onto `subtree.Tile(z: node.Lod, x, y, available)` so the LOD lands in the `Tile.Z` level field the Morton author folds on, carrying its `ContentUri`/`GeometricError`) and `SubtreeCreator3D.GenerateSubtreefile(List<Tile3D>)` for `Octree` (each projected through `TileOf3D` onto `subtree.Tile3D(level: node.Lod, x, y, z: node.Z)` so the octree gains its third spatial axis) — `Fin<T>` aborts on a degenerate tile list captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; `TileAvailability.AuthorMany(Seq<TileNode> tiles, ImplicitSubdivisionScheme scheme, Op key)` lifts to the matching `GenerateSubtreefiles` (the `Dictionary<Tile, byte[]>`/`Dictionary<Tile3D, byte[]>` multi-subtree overflow form) when the tileset exceeds one subtree's level budget, keying each binary by its root tile's `(Level, X, Y, Z)` coordinate (the library builds each root through `new Tile(level, x, y)`/`new Tile3D(level, x, y, z)`, so the root key reads the level-and-position identity, never the auxiliary `Tile.Lod` the author leaves zero).
- Auto: `Author` maps each `TileNode` onto the `subtree.Tile` quadtree node through `TileOf` (or the `subtree.Tile3D` octree node through `TileOf3D` under the `Octree` scheme), runs `SubtreeCreator.GenerateSubtreefile`/`SubtreeCreator3D.GenerateSubtreefile` to author the binary availability bitstream, returning the raw `.subtree` bitstream the caller seals through the `EXPORT_RAIL` `Sealed` content-key — `MortonIndex.GetMortonIndices`/`GetMortonIndices3D` buckets each tile's availability by its level (`Tile.Z` for the quadtree, `Tile3D.Level` for the octree) and sets the `BitArray2D`/`BitArray3D` cell at its `X`/`Y`(`/Z`) position, so the tile and content availability order identically and a tile is "available with content" exactly when both bitstreams set the same Morton position, the same index the `[3]-[TILE_METADATA]` SharpGLTF tile content keys off; a multi-subtree tileset re-bases child coordinates through `SubtreeCreator.GetRelativeTile`/`GetSubtreeTiles` (the `SubtreeCreator3D` twin for the octree) so the child-subtree availability pointers resolve.
- Receipt: the `.subtree` binary is the tileset availability evidence the 3D-Tiles client reads to stream only the existing implicit nodes; the subdivision scheme, level count, and available-tile count are the receipt facts the export records, and each binary content-addresses through the same `InterchangeIdentity` the GLB tile content seals so the tileset and its content share one content space.
- Packages: subtree, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new subdivision scheme is the `ImplicitSubdivisionScheme.Quadtree`/`Octree` discriminant the `SubtreeCreator`/`SubtreeCreator3D` pair already carries; a new availability column is one field on `TileNode` the `subtree.Tile` node exposes; a multi-subtree overflow is the existing `GenerateSubtreefiles` form; never a hand-rolled Morton/bitstream codec and never a second availability authoring path beside `subtree`.
- Boundary: the `.subtree` availability authoring is `subtree`'s — `SubtreeCreator`/`SubtreeCreator3D` `GenerateSubtreefile`/`GenerateSubtreefiles`, the `Tile`/`Tile3D` authoring nodes, the `MortonOrder.Encode2D`/`Encode3D` z-order index, and `SubtreeReader.ReadSubtree` for the round-trip receipt assertion own the bitstream, and a hand-rolled implicit-tiling bitstream is the retired form; the content/availability split is the law — `SharpGLTF.Ext.3DTiles` (`[3]-[TILE_METADATA]`) authors the per-tile glTF CONTENT and `EXT_structural_metadata`, `subtree` authors the tileset AVAILABILITY indexing which implicit nodes exist, the two meeting at the shared Morton tile index and never duplicating the availability logic; the tileset.json root hierarchy and the per-tile bounding-volume geometry stay outside this owner (the `subtree` package carries no tileset.json and no geometry), and the tile-pyramid partitioning/streaming stay at `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` authors the availability binary and the content glTF, never the pyramid.

```csharp signature
public sealed record TileNode(int Lod, int X, int Y, bool Available, string ContentUri, double GeometricError, int Z = 0);

public static class TileAvailability {
    public static Fin<byte[]> Author(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme, Op key) =>
        Try.lift(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefile(tiles.Map(TileOf3D).ToList())
                : subtree.SubtreeCreator.GenerateSubtreefile(tiles.Map(TileOf).ToList())).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"subtree-author:{error.Message}"));

    public static Fin<Map<(int Level, int X, int Y, int Z), byte[]>> AuthorMany(Seq<TileNode> tiles, subtree.ImplicitSubdivisionScheme scheme, Op key) =>
        Try.lift<Map<(int Level, int X, int Y, int Z), byte[]>>(() => scheme == subtree.ImplicitSubdivisionScheme.Octree
                ? subtree.SubtreeCreator3D.GenerateSubtreefiles(tiles.Map(TileOf3D).ToList())
                    .ToMap(static pair => (Level: pair.Key.Level, X: pair.Key.X, Y: pair.Key.Y, Z: pair.Key.Z), static pair => pair.Value)
                : subtree.SubtreeCreator.GenerateSubtreefiles(tiles.Map(TileOf).ToList())
                    .ToMap(static pair => (Level: pair.Key.Z, X: pair.Key.X, Y: pair.Key.Y, Z: 0), static pair => pair.Value)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"subtree-author-many:{error.Message}"));

    // node.Lod -> Tile.Z is the quadtree subdivision level MortonIndex folds availability on; X/Y the in-level cell.
    static subtree.Tile TileOf(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Available) { ContentUri = node.ContentUri, GeometricError = node.GeometricError };

    // node.Lod -> Tile3D.Level the octree level; node.Z -> Tile3D.Z the third spatial axis; Available is set post-ctor.
    static subtree.Tile3D TileOf3D(TileNode node) =>
        new(node.Lod, node.X, node.Y, node.Z) { Available = node.Available };
}
```

## [08]-[RESEARCH]

- [IFC_EGRESS_DELEGATION]: the IFC STEP/XML/JSON egress is NOT re-authored on this rail — `ExportIfc` delegates to the `Projection/egress#IFC_EGRESS` `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Op key, Option<ElementGraph> prior, IIfcProfileStore profiles) → Fin<string>` Bim-internal re-author (the seam `ElementGraph` is the egress source of truth: the per-`Object` `PredefinedType` egress gate + schema-span validation [C6][H8], the 1:1 `GlobalId` round-trip from `Object.ExternalId` [H6], the diff-derived `OwnerHistory` `ChangeAction` against `prior` [H9], and the `ReauthorMaterials`/`ReauthorClassifications`/`ReauthorRelationships` re-author), this rail retaining only the `CanExport` capability gate, the `format#FORMAT_AXIS` `InterchangeFormat.Serialization` column read (the `Option<FormatIfcSerialization>` row value — `Some` exactly on the GeometryGym rows, verified against the GeometryGym 25.7.30 `FormatIfcSerialization` enum + `DatabaseIfc.ToString(FormatIfcSerialization)`; the duplicated `SerializationOf` ladders this page and `wire#WIRE_PROJECTION` carried are the deleted form), and the `ExportArtifact` content-key seal; the deleted `IfcBytes`/`IfcBuildingElementProxy` hand-roll (proxy elements only — no real `IfcClass`, no `PredefinedType`, no properties/quantities/relationships, no owner-history, no materials/classifications) was a lossy SECOND IFC-egress owner the seam forbids, and the `SemanticProjector` instance ignoring its ctor `db` in `Emit` (it builds its own target from `graph.Header`) is the one cross-file friction this delegation surfaces.
- [ROUNDTRIP_CYCLE]: the `RoundTrip.Verify` cycle re-decodes the emitted IFC bytes through the `import#IMPORT_RAIL` `BimIo.ImportIfc(InterchangeFormat, ReadOnlyMemory<byte>, Op) → Fin<DatabaseIfc>` — the ONE decode owner whose `SemanticProjector.Sniff` constructs the ifcXML/ifcJSON `DatabaseIfc` at the sniffed `ReleaseVersion` (the deleted page-local `ParseString`/`ReadXMLDoc`/`ReadJSON` copy constructed `new DatabaseIfc()` at the GG default, reimporting at the WRONG schema and mis-reporting the matrix [H8]) — re-projects through a fresh `SemanticProjector(db, reconciler, profiles)`, and folds the delta onto a `Graph/element#ELEMENT_GRAPH` `Genesis(source.Header)` seed through the seam `Projection/projection#PROJECTION` `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx)` under the `IfcLegality : IGraphConstraint` constraint, taking `.Graph` of the returned `(ElementGraph, GraphDelta)`; the fidelity metric is the baked-`Element` `Generator.Equals` structured diff `Element.EqualityComparer.Default.Inequalities(source, reimported)` → `Inequality.Path` `MemberPath` member paths (verified against `libs/csharp/.api/api-generator-equals` — `Inequality.Path`, `MemberPath.Segments`, `MemberPathSegment.Kind`/`Value`, `MemberPathSegmentKind.Property`), joined on the 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], the `Id`/`Parts` paths filtered so each row reports the element's OWN divergent members and a dropped child surfaces via `dropped` — the seam member diff aligns with the kernel content key by sharing the ONE canonical member set, never re-implementing it nor the stale `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff.Between` over the retired `BimModel`.
- [ELEMENT_SCENE_SURFACE]: the per-element `Author` members are decompile-verified against SharpGLTF.Toolkit 1.0.6 — `NodeBuilder` (ctor-named, `LocalMatrix` get/set), `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, NodeBuilder)`, `SceneBuilderSchema2Settings.GpuMeshInstancingMinCount` (the `ToGltf2` threshold merging same-mesh node fan-outs into `EXT_mesh_gpu_instancing`; the policy `0` maps to `int.MaxValue` = never, because a gpu-merged node LOSES its per-node identity — `KHR_node_visibility` keyframes and `EXT_mesh_features` bindings target NODES, so the 4D/metadata pipeline holds `GpuInstancingMinCount: 0` and the streaming-tile pipeline raises it; per-INSTANCE metadata over a merged node is the `EXT_instance_features` `Tiles3DExtensions.AddInstanceFeatureIds` growth arm) — and `ModelRoot.LogicalNodes` (SharpGLTF.Core 1.0.6) is the read-back the `GlbScene.Nodes` index folds by node name; glTF-level mesh sharing (N nodes referencing ONE logical mesh) is the instancing floor every `Scene` emit gets regardless of the threshold, retiring the N-copied-meshes soup emit for repeated geometry; the per-element tint rides `MaterialBuilder.WithChannelParam(KnownChannel.BaseColor, Vector4)` (catalogue-verified channel-parameter overload) with materials pooled per distinct `Rgba`, so the `ElementInstance.Rgba` column the dotbim wire round-trips also survives the GLB arm.
- [DOTBIM_EMIT]: the `.bim` arm members are catalogue-verified (`.api/api-dotbim`, dotbim 1.2.0) — `File` (`SchemaVersion`/`Meshes`/`Elements`/`Info`, instance `Save(path, format)` + static `Read(path)`, both `.bim`-extension-enforced and path-bound so the byte arm crosses a temp path), `Mesh` (`MeshId` pool key, flat `Coordinates`/`Indices`), `Element` (`MeshId` reference, `Vector` translation + quaternion `Rotation`, setter-validated `Guid`, `Type`, `Color`, `Info` bag) — the placement decomposes through the BCL `Matrix4x4.Decompose` and a non-rigid placement faults loud (the wire carries no scale); `Element.Guid` demands RFC-4122 text while the seam GlobalId is 22-char IFC-compressed, so the Guid mints deterministically from `XxHash128.Hash` over the GlobalId UTF-8 (System.IO.Hashing, csproj-admitted) and the verbatim GlobalId rides `Info["globalId"]` — identity round-trips losslessly and re-export is byte-stable under the Compute content key; the import counterpart resolves `Element.MeshId` against the pool and restores identity from `Info["globalId"]` first, `Guid` second; the setter-validated `Element.Color` writes off the `ElementInstance.Rgba` packed column (opaque-white default) and the import projector re-binds it as one `Pset_Dotbim` `color` row, so the wire's whole-object color survives the round-trip (the per-face `FaceColors` override stream is the one remaining growth column).
- [SCHEDULE_ANIMATION_SURFACE]: the SharpGLTF keyframe-authoring members the `AnimateSchedule` fold composes are decompile-verified — `ModelRoot.CreateAnimation(string name)` returns the new `Animation` added to `LogicalAnimations`, and the `Animation` carries the public keyframe-channel authoring surface `CreateVisibilityChannel(Node, IReadOnlyDictionary<float, bool> keyframes)` (the `KHR_node_visibility` per-node visibility track, `STEP`-interpolated by construction), `CreateScaleChannel(Node, IReadOnlyDictionary<float, Vector3> keyframes, bool linear)`, `CreateTranslationChannel(Node, IReadOnlyDictionary<float, Vector3> keyframes, bool linear)`, and `CreateRotationChannel(Node, IReadOnlyDictionary<float, Quaternion> keyframes, bool linear)` — so the keyframe dictionary is the float-seconds→value map the schedule projection builds and the `bool linear` knob selects the `AnimationInterpolationMode.LINEAR`/`STEP`; the `KHR_node_visibility` extension is the `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` `KhrSlot.Scene` row registered on the rail before the author; the per-element glTF `Node` binding is the `Author`-minted `GlbScene` index (nodes named by the seam `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`, read back from `ModelRoot.LogicalNodes`), and the SharpGLTF.Runtime pin the leg exploits is the already-referenced package no new admission needs.
- [ANIMATION_TIME_AXIS]: the animation time axis is the `Planning/schedule#SCHEDULE` `ConstructionTask.Scheduled` `Interval` projected to glTF seconds through the NodaTime `Duration` from the project start — `policy.SecondsOf(moment, projectStart)` reads `(moment − projectStart).TotalDays × SecondsPerDay` so the keyframe author and the `Planning/schedule#SCHEDULE` `ConstructionState.At(Instant)` snapshot read the one `Interval`-to-`Instant` axis (a scrub at glTF time `t` shows exactly `ConstructionState.At` at the inverse instant), the NodaTime `Duration.TotalDays`/`Interval.Start`/`Interval.End` surface owning the time arithmetic; the `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keys the animation only on a re-sequenced plan so the animated GLB content-addresses through the same schedule key the report reads.
- [LOD_MESHLET_SURFACE]: the `Alimer.Bindings.MeshOptimizer` LOD/meshlet members the `BimLod` fold composes — `Meshopt.Simplify` (entrypoint 1, error-threshold decimation), `Meshopt.SimplifySloppy` (entrypoint 4, aggressive fallback), `Meshopt.SimplifyScale` (entrypoint 7, world-space error normalization), `Meshopt.BuildMeshlets` (meshlet entrypoint 1, cone-culling cluster), `Meshopt.BuildMeshletsBound` (entrypoint 3, buffer bound), `Meshopt.OptimizeMeshlet` (entrypoint 6, per-meshlet cache reorder), and the `Meshlet` struct (`vertex_offset`/`triangle_offset`/`vertex_count`/`triangle_count`) — are decompile-verified in the `.api/api-alimer-meshoptimizer` catalogue; the pinned-pointer simplify/meshlet call convention (the `nuint` count/stride arguments and the `SimplificationOptions` flag set) mirrors the settled `EXPORT_RAIL` `MeshoptBytes` pinned-pointer encode so the LOD leg reuses the package's native interop convention rather than a second binding — the pointer-overload `Meshopt.Simplify(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, target_index_count, target_error, SimplificationOptions, result_error)` is ten arguments, and the pointer-overload `Meshopt.SimplifySloppy(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, vertex_lock, target_index_count, target_error, result_error)` carries the `Byte* vertex_lock` per-vertex lock mask between the stride and the target count (passed `(byte*)null` for the no-lock decimation), distinct from the seven-argument `Span<uint>` overload — and the per-LOD `InterchangeIdentity.Key` content key per detail level meets `csharp:Rasm.Compute#TILE_PARTITION` at the codec admission gate; `target_error` is RELATIVE to mesh extents under `SimplificationOptions.None` (`meshopt_SimplifyErrorAbsolute` is the absolute-units flag) and `SimplifyScale` is the relative→world conversion the `LodLevel.WorldError` receipt column applies to the solver's `result_error` — a scale-multiplied budget passed as `target_error` was the deleted defect.
- [CONTENT_IDENTITY_CONSUME]: the `ExportArtifact.ContentKey` and per-`LodLevel` content keys mint through the kernel seed-zero `ContentHash.Of` over the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` fold (`String(formatKey)`, the `Deflection`/`Tolerance`/`AngleTolerance` quality triple as `Double` columns, `Raw(bytes)`) — the SAME one-hasher law the `reconstruct#RECONSTRUCTION` and `tessellation#TESSELLATION_BRIDGE` keys observe, so a Bim artifact key never depends DOWN on `Rasm.Compute` (the retired `InterchangeIdentity.Key` mint was the named strata inversion); the Compute lane derives its own keys at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and the artifact lands content-addressed on the Persistence blob lane at the seam — Bim mints no second identity scheme and no second blob owner.
- [TILE_FEATURE_BINDING]: the `SharpGLTF.Ext.3DTiles` per-tile metadata author members the `TileMetadata.Attach` fold composes — `Tiles3DExtensions.RegisterExtensions`/`UseStructuralMetadata`, `EXTStructuralMetadataRoot.UseEmbeddedSchema`/`AddPropertyTable`, `StructuralMetadataSchema.UseClassMetadata`/`UseEnumMetadata`, `StructuralMetadataClass.UseProperty`, `StructuralMetadataClassProperty.WithStringType`/`WithEnumeration`, `PropertyTable.UseProperty`, `PropertyTableProperty.SetValues<T>`, `new FeatureIDBuilder(...)`, and `MeshPrimitive.AddMeshFeatureIds` — are decompile-verified in the `Rasm.Bim/.api/api-sharpgltf-3dtiles` catalogue; the per-vertex `_FEATURE_ID_0` the `attributeOrTexture: 0` slot names is AUTHORED, not assumed — `Staged` stamps it through the `FeatureVertex` custom fragment (`IVertexCustom`, the catalogued Toolkit vertex-fragment seam; the fragment's `IVertexCustom`/`IVertexMaterial` member spellings confirm against SharpGLTF.Toolkit 1.0.6 at realization) with the `GlbScene.Rows` instance ordinal on every uniquely-meshed vertex and the null row on shared meshes, so the stamp, the table row, and the `nullFeatureId` sentinel index ONE row space; a gpu-instanced merged node re-binds through `Tiles3DExtensions.AddInstanceFeatureIds` (`EXT_instance_features`) as the growth arm — its per-instance `_FEATURE_ID_0` instance-attribute authoring is the admission gate that arm carries; the tile-pyramid partitioning rides `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam.
- [TILE_AVAILABILITY_SURFACE]: the `subtree` authoring members the `TileAvailability` fold composes are decompile-verified — `subtree.Tile` ctors `(int z, int x, int y)` / `(int z, int x, int y, bool available)` set the `Z`/`X`/`Y` coordinates where `Z` is the quadtree subdivision level (`MortonIndex.GetMortonIndices` buckets availability by `tile.Z`, `Tile.GetChildren` emits `Z+1` children, `Tile.Parent` climbs to `Z-1`) and the `Lod` property is an auxiliary the quadtree author never reads, so `TileOf` maps `node.Lod` onto the ctor `z` slot (the level), never a dropped coordinate; `subtree.Tile3D` ctor `(int level, int x, int y, int z)` sets a distinct `Level` subdivision axis plus a true third spatial `Z` (`MortonIndex.GetMortonIndices3D` buckets by `t.Level` and sets the `BitArray3D` cell at `X`/`Y`/`Z`, `Tile3D.Parent` halving all three), so `TileOf3D` supplies `node.Z` for the octree vertical axis; `SubtreeCreator`/`SubtreeCreator3D` `GenerateSubtreefile`/`GenerateSubtreefiles` author the binary, the multi-subtree `GenerateSubtreefiles` keying its `Dictionary<Tile[3D], byte[]>` by roots the library builds through `new Tile(level, x, y)`/`new Tile3D(level, x, y, z)` whose `Lod` stays zero — so `AuthorMany` keys by the root `(Level, X, Y, Z)` coordinate, never `Lod`; `ImplicitSubdivisionScheme` is the `{ Quadtree, Octree }` enum the `Author`/`AuthorMany` fold switches on, the `Tile`/`Tile3D`, `SubtreeCreator`/`SubtreeCreator3D`, and `MortonIndex` members verified in the `.api/api-subtree` catalogue.
