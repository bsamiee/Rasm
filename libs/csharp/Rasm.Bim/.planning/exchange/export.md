# [BIM_EXPORT_RAIL]

The artifact-emit rail: one `BimExport` export fold dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, FBX/Collada/3MF through the `AssimpNetter` `AssimpContext.ExportToBlob` scene serializer, OpenUSD through the `UniversalSceneDescription` `UsdStage` author, and the 3D-Tiles 1.1 `.subtree` availability bitstream through the `subtree` `SubtreeCreator` — over the `format#FORMAT_AXIS` `InterchangeFormat` rows. The IFC STEP/XML/JSON leg does NOT re-author IFC here: it DELEGATES to the seam `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit` (the ONE Bim-internal `ElementGraph`→`DatabaseIfc` re-author — the `PredefinedType` egress gate [C6], the 1:1 `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` [H9], and the material/classification/relationship re-author), this rail OWNING only the artifact seal (`ExportArtifact` + the Compute content key) and the `InterchangeFormat`→`FormatIfcSerialization` row map. The page composes the seam `Graph/element#ELEMENT_GRAPH` `ElementGraph`/`Element` (a consumer reads the baked `Element`, never a stored record), the `import#IMPORT_RAIL` `ImportedGeometry` mesh-scene carrier, the `format#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key as settled vocabulary; the emitted `ExportArtifact` feeds the Compute content-addressing seam. The retired `BimModel`/`BimElement` carriers and the hand-rolled `IfcBuildingElementProxy` re-author (a lossy SECOND IFC-egress owner — proxy elements only, no real classes, predefined types, properties, quantities, relationships, owner-history, materials, or classifications) are GONE. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[EXPORT_RAIL]: artifact emit — GLB mesh-and-scene with Draco/meshopt encode; the IFC STEP/XML/JSON leg DELEGATING to the seam `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit`, this rail owning only the `ExportArtifact` seal + the `InterchangeFormat`→`FormatIfcSerialization` row map.
- [02]-[TILE_METADATA]: per-tile `EXT_structural_metadata` schema/class/property-table over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, not a stored record), bound to the GLB primitive through `EXT_mesh_features` feature IDs.
- [03]-[BIM_LOD]: the per-element LOD pyramid through `Meshopt.Simplify`/`SimplifySloppy`, the `Meshopt.BuildMeshlets` meshlet residency band, and the per-LOD content key the `Rasm.Compute#TILE_PARTITION` pyramid addresses.
- [04]-[SCHEDULE_ANIMATION]: the `AnimateSchedule` arm baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element glTF visibility/scale keyframe tracks through `ModelRoot.CreateAnimation` and the `KHR_node_visibility` channel over a caller-supplied element-`GlobalId`→glTF-`Node` index, so a 4D schedule exports as one animated GLB a web viewer scrubs.
- [05]-[ROUNDTRIP]: the `RoundTrip` lossless-verification matrix folding an `ElementGraph` emit→re-decode→`Project`→`Assemble` cycle across the IFC STEP/ifcXML/ifcJSON serializations, witnessing per-element fidelity by the seam content key joined on the 1:1 `ExternalId` and naming the divergent members through the `Generator.Equals` structured diff.
- [06]-[TILE_AVAILABILITY]: the `TileAvailability` 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream author over the `subtree` `SubtreeCreator`/`SubtreeCreator3D`/`Tile`/`Tile3D`/`MortonIndex` surface — the tileset-side complement the `SharpGLTF.Ext.3DTiles` per-tile content leg cannot reach, retiring the hand-rolled implicit-tiling bitstream.

## [02]-[EXPORT_RAIL]

- Owner: `BimExport` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and the IFC STEP/XML/JSON leg DELEGATING to the seam `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit` (this rail seals the bytes, the projector owns the re-author); `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks, Op key)` for the GLB/scene/USD geometry path; `BimExport.ExportIfc(InterchangeFormat format, ElementGraph graph, SemanticProjector projector, InterchangePolicy policy, ClockPolicy clocks, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles, Op key)` for the IFC serialization — the `graph` is the seam read snapshot, the `projector` the Bim-internal IFC-egress owner the app wires, the `prior` snapshot driving the diff-derived `OwnerHistory` `ChangeAction` [H9], the `profiles` resolver reconstituting a `ProfileSet`'s `IfcProfileDef` from the content store; `Fin<T>` aborts on a write-capability miss (`Model/faults#FAULT_BAND` `BimFault.CodecReject`), a non-GeometryGym codec, or a captured GeometryGym serialization/predefined-gate fault the projector lowers (`BimFault.ModelRejected`/`BimFault.UnmappedClass`), each typed `BimFault` case (band 2600, `Expected`-derived) lifting BARE onto the `Fin<T>` rail with no `.ToError()` hop.
- Auto: the `GlbBytes` fold switches on `InterchangePolicy.Compression` — the `KhrEncoder.None` arm assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; the `KhrEncoder.Draco` arm bypasses the GLB container and quantizes the geometry into a `DracoMesh` (`PointAttribute.Wrap(AttributeType.Position, …)` per-attribute wrap, `DracoMesh.AddFace(int[])` per triangle), emitting the Draco byte stream through `Draco.Encode(mesh, DracoEncodeOptions)`; the `KhrEncoder.Meshopt` arm first runs the catalogued meshopt optimization pipeline — `GenerateVertexRemap` deduplicates the exploded triangle-soup into a unique-vertex set, `RemapVertexBuffer`/`RemapIndexBuffer` apply the remap, then `OptimizeVertexCache`/`OptimizeOverdraw`/`OptimizeVertexFetch` reorder for GPU cache, overdraw, and fetch locality — bounds the destination through `EncodeVertexBufferBound`/`EncodeIndexBufferBound`, and emits the meshopt bufferView payloads through the pinned-pointer `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the optimized indexed buffers; neither codec takes a glTF `ModelRoot` and neither arm writes the GLB container, so the compression leg replaces the GLB write rather than post-processing it; the IFC leg selects no serialization writer here — `ExportIfc` maps the `ifc`/`ifc-xml`/`ifc-json` row to `FormatIfcSerialization.STEP`/`XML`/`JSON` through `SerializationOf` and hands the seam `ElementGraph` plus that serialization to `SemanticProjector.Emit`, which re-authors the whole graph (the per-`Object` `PredefinedType` egress gate + schema-span validation [C6][H8], the 1:1 `GlobalId` round-trip [H6], the diff-derived `OwnerHistory` [H9], and the material/classification/relationship re-author) and returns the IFC text the rail UTF-8-encodes and seals — no `DatabaseIfc` is constructed or canonically placed on this page.
- Receipt: the `ModelEmit` receipt case carries the format key, codec key, emitted byte count, and the `ExportArtifact.ContentKey` the Compute addressing seam computes, symmetric to the import `ModelLoad` case; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, AssimpNetter, UniversalSceneDescription, Rasm.Element, NodaTime, LanguageExt.Core
- Growth: a new managed export is one codec arm on the export fold (the `InterchangeCodec.SceneExchange` AssimpNetter arm and the `UsdStage` USD arm join the SharpGLTF/GeometryGym arms by `InterchangeCodec` discriminant, never a per-format exporter family); a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold; a new assimp export target is one `format.Key` `exportFormatId` the `ExportToBlob` matrix already covers.
- Boundary: the export fold extends the `BimExport` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the Compute content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)` before any write so a material/light/texture channel serializes through its decompile-verified SharpGLTF schema type rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression` and `KHR_meshopt_compression` rows carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode through the static `Draco.Encode(DracoPointCloud, DracoEncodeOptions)` over a `DracoMesh` built from `PointAttribute.Wrap` attributes, and `Alimer.Bindings.MeshOptimizer` owns the meshopt encode through the static `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the raw vertex/index buffers, both quantizing to the `InterchangePolicy` bit budget; both packages are the outside-Rhino EXPORT_RAIL concern, so the `GlbBytes` fold routes the compression leg through those packages and a managed in-Rhino-ALC compression encode is the rejected form, and a glTF `ModelRoot` passed to either codec is the rejected form because neither package owns a glTF model type; the IFC egress is NOT this rail's — `ExportIfc` delegates to `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit`, and a hand-rolled `IfcBuildingElementProxy` re-author (the deleted `IfcBytes` form: proxy elements only, dropping the real `IfcClass`, the `PredefinedType`, every property/quantity/relationship, the owner-history, and the materials/classifications) is a SECOND IFC-egress owner the seam forbids — the `GlobalId` round-trips 1:1 from the `Object.ExternalId` inside `Emit` (never a fresh GUID per export), making export idempotent under the Compute content-key; `ExportIfc` retains only the `CanExport`/`GeometryGym`-codec capability gate and the `SerializationOf` row map, faulting at the boundary on a write-only row or a non-GeometryGym codec; the chunked-field and structural-delta codecs stay at `Rasm.Compute/Runtime/codecs` consumed at the seam.

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
    Seq<KhrExtension> Extensions) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.Ifc4X3Add2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Compression: KhrEncoder.None, QuantizationBits: 14, Extensions: Seq<KhrExtension>());
    public static readonly InterchangePolicy Web = Canonical with {
        Compression = KhrEncoder.Meshopt, QuantizationBits = 12,
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

public static partial class BimExport {
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks, Op key) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-unsupported:{format.Key}"))
        : format.Codec == InterchangeCodec.SharpGltf
            ? GlbBytes(geometry, policy, key).Map(bytes => Sealed(format, bytes, policy, clocks.Now))
        : format.Codec == InterchangeCodec.SceneExchange
            ? Try.lift(() => Sealed(format, SceneBytes(format, geometry), policy, clocks.Now)).Run().MapFail(error => new BimFault.CodecReject(key, $"scene-export:{error.Message}"))
        : format.Codec == InterchangeCodec.UsdStage
            ? Try.lift(() => Sealed(format, UsdBytes(format, geometry), policy, clocks.Now)).Run().MapFail(error => new BimFault.CodecReject(key, $"usd-export:{error.Message}"))
            : Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-codec-miss:{format.Key}"));

    // FBX/Collada/3MF emit through AssimpNetter — the `scene-exchange` codec. The canonical triangle-soup
    // rebuilds into one Assimp.Scene (RootNode + Mesh + Material) and ExportToBlob serializes it at the row's
    // FormatId; glTF/GLB export stays on SharpGLTF so the Draco/meshopt encode stacks on that path, not this one.
    static byte[] SceneBytes(InterchangeFormat format, ImportedGeometry geometry) {
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
        return context.ExportToBlob(scene, format.Key).Data;
    }

    // USD emit through UniversalSceneDescription — the `usd-stage` codec. One UsdStage authors a UsdGeomMesh
    // prim (points VtVec3fArray, faceVertexCounts/Indices VtIntArray through the typed-array Set seam), exports
    // to the temp path, and reads the bytes; USD is a scene-graph peer, never re-deriving the BIM semantics.
    static byte[] UsdBytes(InterchangeFormat format, ImportedGeometry geometry) {
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

    // IFC egress is the seam's Projection/semantic#IFC_EGRESS SemanticProjector.Emit — the ONE Bim-internal
    // ElementGraph->DatabaseIfc re-author (the PredefinedType egress gate + schema span [C6][H8], the 1:1 GlobalId
    // round-trip [H6], the diff-derived OwnerHistory ChangeAction against `prior` [H9], the material/classification/
    // relationship re-author). This rail OWNS only the CanExport/GeometryGym capability gate, the
    // InterchangeFormat->FormatIfcSerialization row map (SerializationOf), and the ExportArtifact content-key seal;
    // the hand-rolled IfcBuildingElementProxy re-author is the DELETED form (a lossy second IFC-egress owner). The
    // app wires the projector (the seam owns Assemble, the app the wiring); `prior` drives the ChangeAction diff and
    // `profiles` reconstitutes a ProfileSet's IfcProfileDef from the content store.
    public static Fin<ExportArtifact> ExportIfc(
        InterchangeFormat format, ElementGraph graph, SemanticProjector projector,
        InterchangePolicy policy, ClockPolicy clocks, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles, Op key) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"export-unsupported:{format.Key}"))
        : format.Codec != InterchangeCodec.GeometryGym ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject(key, $"ifc-export-codec-miss:{format.Key}"))
        : projector.Emit(graph, SerializationOf(format), prior, profiles).Map(text => Sealed(format, Encoding.UTF8.GetBytes(text), policy, clocks.Now));

    // The InterchangeFormat -> FormatIfcSerialization row map (internal so the ROUNDTRIP re-decode shares it).
    internal static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    // The SharpGLTF/Draco/meshopt encode funnels its native faults onto the rail (Draco.Encode raises DrakoException,
    // ToGltf2/WriteGLB raise SharpGLTF ModelException) so a malformed-geometry or compression fault lifts BARE as a typed
    // BimFault.CodecReject(key) — symmetric with the scene-export/usd-export arms — never escaping the Fin<T> rail as an
    // uncaught exception; RegisterExtensions keeps its own ModelRejected registration rail ahead of the encode.
    static Fin<byte[]> GlbBytes(ImportedGeometry geometry, InterchangePolicy policy, Op key) =>
        RegisterExtensions(policy).Bind(_ => Try.lift(() => policy.Compression switch {
            KhrEncoder.Draco => DracoBytes(geometry, policy),
            KhrEncoder.Meshopt => MeshoptBytes(geometry, policy),
            _ => WriteGlb(SceneOf(geometry, policy), policy),
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"gltf-export:{error.Message}")));

    static ModelRoot SceneOf(ImportedGeometry geometry, InterchangePolicy policy) {
        var material = new MaterialBuilder("default").WithMetallicRoughnessShader();
        var mesh = new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>(geometry.Format.Key);
        var primitive = mesh.UsePrimitive(material);
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            primitive.AddTriangle(
                Vertex(verts, normals, (int)indices[tri * 3]),
                Vertex(verts, normals, (int)indices[tri * 3 + 1]),
                Vertex(verts, normals, (int)indices[tri * 3 + 2]));
        }
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, AffineTransform.Identity);
        return scene.ToGltf2(new SceneBuilderSchema2Settings { UseStridedBuffers = policy.StridedBuffers });
    }

    static VertexBuilder<VertexPositionNormal, VertexEmpty, VertexEmpty> Vertex(ReadOnlySpan<float> verts, ReadOnlySpan<float> normals, int index) {
        int v = index * 3;
        return new VertexPositionNormal(verts[v], verts[v + 1], verts[v + 2], normals[v], normals[v + 1], normals[v + 2]);
    }

    static byte[] WriteGlb(ModelRoot model, InterchangePolicy policy) {
        if (policy.MergeBuffers) { model.MergeBuffers(); }
        return model.WriteGLB(new WriteSettings { MergeBuffers = policy.MergeBuffers }).ToArray();
    }

    static byte[] DracoBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var mesh = new DracoMesh { NumPoints = geometry.VertexCount };
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Position, 3, geometry.Vertices.ToArray()));
        mesh.AddAttribute(PointAttribute.Wrap(AttributeType.Normal, 3, geometry.Normals.ToArray()));
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            mesh.AddFace([(int)indices[tri * 3], (int)indices[tri * 3 + 1], (int)indices[tri * 3 + 2]]);
        }
        return Draco.Encode(mesh, new DracoEncodeOptions {
            PositionBits = policy.QuantizationBits, NormalBits = policy.QuantizationBits,
            CompressionLevel = DracoCompressionLevel.Optimal,
        });
    }

    static unsafe byte[] MeshoptBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        var soup = new VertexPositionNormal[geometry.VertexCount];
        for (int v = 0; v < geometry.VertexCount; v++) {
            int o = v * 3;
            soup[v] = new VertexPositionNormal(verts[o], verts[o + 1], verts[o + 2], normals[o], normals[o + 1], normals[o + 2]);
        }
        var soupIndices = new uint[geometry.TriangleCount * 3];
        for (int i = 0; i < soupIndices.Length; i++) { soupIndices[i] = (uint)geometry.Indices.Span[i]; }
        nuint vertSize = (nuint)Unsafe.SizeOf<VertexPositionNormal>();
        nuint soupCount = (nuint)soup.Length;
        nuint indexCount = (nuint)soupIndices.Length;
        var remap = new uint[soup.Length];
        nuint uniqueCount;
        fixed (uint* remapPtr = remap)
        fixed (uint* idxPtr = soupIndices)
        fixed (VertexPositionNormal* vSoup = soup) {
            uniqueCount = Meshopt.GenerateVertexRemap(remapPtr, idxPtr, indexCount, vSoup, soupCount, vertSize);
        }
        var remapped = new VertexPositionNormal[(int)uniqueCount];
        var interleaved = new VertexPositionNormal[(int)uniqueCount];
        var indices = new uint[(int)indexCount];
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
        var vBuffer = new byte[(int)Meshopt.EncodeVertexBufferBound(uniqueCount, vertSize)];
        var iBuffer = new byte[(int)Meshopt.EncodeIndexBufferBound(indexCount, uniqueCount)];
        nuint vLen, iLen;
        fixed (byte* vDst = vBuffer)
        fixed (byte* iDst = iBuffer)
        fixed (VertexPositionNormal* vSrc = interleaved)
        fixed (uint* iSrc = indices) {
            vLen = Meshopt.EncodeVertexBuffer(vDst, (nuint)vBuffer.Length, vSrc, uniqueCount, vertSize);
            iLen = Meshopt.EncodeIndexBuffer(iDst, (nuint)iBuffer.Length, iSrc, indexCount);
        }
        return [.. BitConverter.GetBytes((int)uniqueCount), .. BitConverter.GetBytes((int)vLen), .. vBuffer.AsSpan(0, (int)vLen), .. iBuffer.AsSpan(0, (int)iLen)];
    }

    static Fin<Unit> RegisterExtensions(InterchangePolicy policy) =>
        policy.Extensions.Traverse(static khr => khr.Register()).Map(static _ => unit);

    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes, InterchangeIdentity.Key(format.Key, bytes, policy.Deflection, policy.Tolerance, policy.AngleTolerance), bytes.LongLength, at);
}
```

## [03]-[TILE_METADATA]

- Owner: `TileMetadata` the per-tile `EXT_structural_metadata` author over the seam `Graph/element#ELEMENT_GRAPH` `Element` semantic (the baked element, never a stored record) — one embedded schema carrying the element's `Classification` code, `ExternalId` GlobalId, name, and (as growth) the baked property/quantity columns, one `PropertyTable` per-feature value store, and the `EXT_mesh_features` feature-ID binding tying each GLB primitive vertex span to its element row so the Cesium 3D Tiles web peer resolves per-element metadata at pick time.
- Entry: `TileMetadata.Attach(ModelRoot tile, Seq<Element> elements, Op key)` authors the structural-metadata schema/class/property-table on the GLB and binds the feature IDs to the tile primitives — `Fin<T>` aborts on a registration fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the per-tile metadata emit composes through the `Rasm.Compute` interchange codec `TILE_PARTITION` at the seam and `Rasm.Bim` authors the canonical schema shape and the extension surface; the caller bakes the per-element scene and supplies the matching baked `Element` set, so the feature-ID order and the property-table row order align.
- Auto: `Attach` runs `Tiles3DExtensions.RegisterExtensions()` once, opens the schema through `tile.UseStructuralMetadata().UseEmbeddedSchema(id)`, defines the `Element` class through `UseClassMetadata("Element")` with one `UseProperty(name).With<Type>(...)` per canonical column (`GlobalId` string off `Element.ExternalId`, `Class` enumeration over the `IfcClass` vocabulary through `UseEnumMetadata` off `Element.Classification.Code`, `Name` string, and as growth the baked-Pset property columns off `Element.Properties`), adds the per-feature `PropertyTable` through `AddPropertyTable(class, featureCount, name)` encoding each element row through `UseProperty(key).SetValues<T>(...)`, and binds the GLB primitive feature IDs through `new FeatureIDBuilder(featureCount, attributeIndex, propertyTable, ...)` then `primitive.AddMeshFeatureIds(builder)` so the `EXT_mesh_features` feature-ID attribute indexes each vertex span to its `PropertyTable` row.
- Receipt: the authored `EXT_structural_metadata` schema and `PropertyTable` are the per-tile semantic the web peer reads — the same seam `Element` vocabulary a consumer reads at the `Exchange/wire#WIRE_PROJECTION`, projected onto the binary tile metadata so a Cesium consumer resolves per-element BIM semantics at pick without a second metadata mint.
- Packages: SharpGLTF.Core, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new metadata column is one `UseProperty(name).With<Type>(...)` row on the embedded class fed from a baked `Element` field; a new feature-ID binding is one `FeatureIDBuilder` over the primitive; the `IfcClass` enumeration is one `UseEnumMetadata` row tracking the `IfcClass` vocabulary; never a hand-authored JSON metadata block and never a second per-tile metadata mint.
- Boundary: the per-tile metadata authors through the `SharpGLTF.Ext.3DTiles` `EXT_structural_metadata`/`EXT_mesh_features` surface — a hand-authored JSON `EXT_structural_metadata` block is the deleted form, the `StructuralMetadataClassProperty.With<Type>` selectors and the `PropertyTableProperty.SetValues<T>` binary encode own the schema and value storage; `Tiles3DExtensions.RegisterExtensions()` runs once before any author and the call is idempotent at the factory level; the per-feature semantic is the seam baked `Element` and a retired `BimElement` row crossing `Attach` is the deleted form (the element is the `Bake` fold over the `ElementGraph`, the `Classification` code resolved to the `IfcClass` enumeration, never a typed `IfcClass` on the row); the `IfcClass` column rides `UseEnumMetadata` so the closed BIM class vocabulary serializes by its enumeration rather than a free string; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` admits the extension surface and the canonical schema shape, never the tile pyramid; the `OneOf<int, Texture>` feature-ID attribute selector is a transitive `OneOf` dependency consumed only by `FeatureIDBuilder` and no Bim code references it directly; the per-tile `Element` semantic is the same vocabulary the wire projection carries, never a second metadata vocabulary.

```csharp signature
public static class TileMetadata {
    public static Fin<ModelRoot> Attach(ModelRoot tile, Seq<Element> elements, Op key) =>
        Try.lift(() => Author(tile, elements)).Run().MapFail(error => new BimFault.ModelRejected(key, $"tile-metadata:{error.Message}"));

    // The per-feature semantic is the seam baked `Element`: ExternalId GlobalId + the generic Classification code
    // resolved to the IfcClass enumeration + name (the Pset/Qto columns grow off the baked Element.Properties/Quantities),
    // so the tile carries the SAME vocabulary the wire projection does, never a second metadata mint nor a typed IfcClass row.
    static ModelRoot Author(ModelRoot tile, Seq<Element> elements) {
        Tiles3DExtensions.RegisterExtensions();
        var root = tile.UseStructuralMetadata();
        var schema = root.UseEmbeddedSchema("rasm-element");
        var classRows = IfcClass.Items.ToSeq();
        var classKinds = schema.UseEnumMetadata("IfcClass", [.. classRows.Map(static (row, i) => (row.Key, i))]);
        var element = schema.UseClassMetadata("Element");
        element.UseProperty("GlobalId").WithStringType(noData: null, defaultValue: null);
        element.UseProperty("Class").WithEnumeration(classKinds, noData: null);
        element.UseProperty("Name").WithStringType(noData: null, defaultValue: null);
        var table = root.AddPropertyTable(element, elements.Count, "elements");
        table.UseProperty("GlobalId").SetValues([.. elements.Map(static e => e.ExternalId.IfNone(""))]);
        table.UseProperty("Class").SetValues([.. elements.Map(e => classRows.FindIndex(r => r.Key == e.Classification.Code))]);
        table.UseProperty("Name").SetValues([.. elements.Map(static e => e.Name)]);
        tile.LogicalMeshes
            .SelectMany(static mesh => mesh.Primitives)
            .Iter((primitive, index) => primitive.AddMeshFeatureIds(
                new FeatureIDBuilder(elements.Count, attributeOrTexture: 0, propertyTable: table, channels: null, label: "elements", nullFeatureId: null)));
        return tile;
    }
}
```

## [04]-[BIM_LOD]

- Owner: `BimLod` the per-element LOD-pyramid leg ADDITIVE to the export rail — one progressive-detail chain per element derived through the catalogued `Meshopt.Simplify`/`SimplifySloppy` decimation keyed by target triangle ratio, plus the `MeshletResidency` band through `Meshopt.BuildMeshlets` for the WebGPU raster path; `LodLevel` the per-level record carrying the decimated index buffer, the target ratio, and the per-LOD content key the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses.
- Entry: `BimLod.Pyramid(ImportedGeometry geometry, InterchangePolicy policy, Op key)` derives the LOD chain over the policy's ratio schedule (each level a `Meshopt.Simplify` at decreasing target index count, falling back to `Meshopt.SimplifySloppy` when the error threshold cannot be met), and `BimLod.Meshlets(ImportedGeometry geometry, Op key)` clusters the residency band through `Meshopt.BuildMeshlets` (bounded by `Meshopt.BuildMeshletsBound`, optimized per meshlet through `Meshopt.OptimizeMeshlet`) — `Fin<T>` aborts on a degenerate decimation captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; each level seals its own `ExportArtifact.ContentKey` so the web peer streams each LOD by view distance, the `TileMetadata` per-tile semantic riding each level unchanged.
- Receipt: each `LodLevel` carries its target ratio, resulting triangle count, and per-LOD content key — the same `InterchangeIdentity` the full-resolution `ExportArtifact` seals, computed per level so the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses every detail level and the cross-libs `WEB_GEOMETRY_RESIDENCY_WIRE` splat/meshlet manifest the AppUi projection mints streams each LOD by view distance.
- Packages: Alimer.Bindings.MeshOptimizer, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new detail level is one content-keyed `LodLevel` row on the pyramid; a new meshlet bound is one `MeshletResidency` band over the residency set; the per-tile `TileMetadata` semantic rides each LOD unchanged; never a per-element full-resolution emit and never a second LOD or residency owner.
- Boundary: the LOD decimation is `Alimer.Bindings.MeshOptimizer`'s — `Meshopt.Simplify` (error-threshold decimation with `SimplificationOptions` flags) and `Meshopt.SimplifySloppy` (aggressive fallback) over the optimized indexed buffer own the LOD chain, and a hand-rolled edge-collapse decimator is the deleted form; the meshlet residency rides `Meshopt.BuildMeshlets` (allocated via `BuildMeshletsBound`, optimized per meshlet via `OptimizeMeshlet`) so the WebGPU raster path consumes the package-owned meshlet partition, never a hand-rolled cluster algorithm; the per-LOD content key meets `csharp:Rasm.Compute#TILE_PARTITION` at the seam — `Rasm.Bim` derives the per-element pyramid and seals each level's content key, the tile-pyramid partitioning and streaming stay at Compute consumed at the seam; the residency band feeds the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest the AppUi projection mints, never a second residency owner; the LOD leg composes the same `ImportedGeometry` triangle-soup the `EXPORT_RAIL` `SceneOf` reads, never a second geometry carrier.

```csharp signature
public sealed record LodLevel(int Level, double TargetRatio, int TriangleCount, ReadOnlyMemory<uint> Indices, UInt128 ContentKey);

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
        return LodSchedule.Map((ratio, level) => Decimate(source, verts, vertexCount, vertexStride, scale, ratio, level, geometry.Format.Key, policy)).ToSeq();
    }

    static unsafe LodLevel Decimate(uint[] source, float[] verts, nuint vertexCount, nuint vertexStride, float scale, double ratio, int level, string formatKey, InterchangePolicy policy) {
        nuint sourceCount = (nuint)source.Length;
        nuint targetCount = (nuint)((long)source.Length * ratio);
        var destination = new uint[source.Length];
        nuint resultCount;
        float resultError;
        fixed (uint* dst = destination)
        fixed (uint* src = source)
        fixed (float* vPtr = verts) {
            resultCount = Meshopt.Simplify(dst, src, sourceCount, vPtr, vertexCount, vertexStride, targetCount, 0.01f * scale, SimplificationOptions.None, &resultError);
            if (resultCount > targetCount) {
                resultCount = Meshopt.SimplifySloppy(dst, src, sourceCount, vPtr, vertexCount, vertexStride, (byte*)null, targetCount, 0.05f * scale, &resultError);
            }
        }
        var indices = destination.AsSpan(0, (int)resultCount).ToArray();
        var bytes = MemoryMarshal.AsBytes(indices.AsSpan());
        return new LodLevel(level, ratio, (int)resultCount / 3, indices,
            InterchangeIdentity.Key($"{formatKey}:lod{level}", bytes, policy.Deflection, policy.Tolerance, policy.AngleTolerance));
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

    static readonly Seq<double> LodSchedule = Seq(0.5, 0.25, 0.1, 0.05);
}
```

## [05]-[SCHEDULE_ANIMATION]

- Owner: `ScheduleAnimation` the 4D-emit leg ADDITIVE to the export rail — one glTF `Animation` baking the `Planning/schedule#SCHEDULE` `ScheduleNetwork` construction sequence into per-element keyframe tracks: each `ConstructionTask`'s scheduled `Interval` drives a per-element visibility track (the element is invisible before its task starts and visible from its task start) plus an optional scale track (the element grows from a zero-scale point to its full scale across its task window) so a viewer scrubs the construction sequence on the GLB timeline; `AnimationTrack` the per-element keyframe record carrying the element `GlobalId`, the appear-time and full-time seconds, and the glTF `Node` the element's mesh binds to.
- Entry: `BimExport.AnimateSchedule(ModelRoot model, ScheduleNetwork network, ScheduleAnimationPolicy policy, IReadOnlyDictionary<string, SharpGLTF.Schema2.Node> nodes, Op key)` bakes the schedule into the model's animation set — projecting each `ConstructionTask` `Interval` bound onto its glTF time-in-seconds through `policy.SecondsOf(Instant moment, Instant projectStart)` (the bound mapped to the timeline via the NodaTime `Duration` from the project start, scaled by `policy.SecondsPerDay`), resolving each assigned element's glTF `Node` through the CALLER-SUPPLIED per-element `GlobalId→Node` index (the element `GlobalId` is the seam `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`; the caller bakes the per-element scene, this leg never re-walks it), and authoring one `KHR_node_visibility` visibility channel (and the `policy.Grow` scale channel when set) per element through the SharpGLTF `Animation.CreateVisibilityChannel`/`CreateScaleChannel` keyframe surface — `Fin<T>` aborts on a SharpGLTF authoring fault captured at the boundary (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; the animation and the `Planning/schedule#SCHEDULE` `ConstructionState.At` snapshot share one `Interval`-to-`Instant` time axis so a scrub at glTF time `t` shows exactly the element set `ConstructionState.At` resolves at the inverse instant (the schedule owner is the `BimModel`→`ElementGraph` cross-file alignment point its rebuild settles).
- Auto: `AnimateSchedule` runs `ExtensionsFactory`-registered `KHR_node_visibility` once, creates one `Animation` through `model.CreateAnimation("construction-sequence")`, folds the network's tasks into per-element `AnimationTrack` rows (a task's `Interval.Start` is the element's appear time, `Interval.End` its full time, unioned over every task that assigns the element so a multi-task element appears at its earliest task), and authors each element's keyframe dictionary: the visibility track is `{ appearTime − ε: false, appearTime: true }` so the element pops in at its task start under the `STEP` interpolation the `bool` channel forces, and the optional scale track is `{ appearTime: Vector3.Zero, fullTime: Vector3.One }` under `LINEAR` interpolation so the element grows across its task window; `policy.SecondsOf(moment, projectStart)` projects an `Interval` bound onto the float seconds axis through `(moment − projectStart).TotalDays × SecondsPerDay`, the one time axis the `ConstructionState.At` snapshot reads, so the keyframe author and the snapshot never carry two clocks; the `model` returns with its `LogicalAnimations` populated so the downstream `WriteGlb` emits the animated GLB and the `TileMetadata` per-element semantic rides each frame unchanged.
- Receipt: the `Seq<AnimationTrack>` is the 4D-emit evidence — each row carries the element `GlobalId`, the appear/full seconds, and the bound `Node` logical index so the Cesium/three.js timeline scrub resolves the construction state at any timeline instant; the animated GLB the `WriteGlb` emits is the streamed 4D timeline a web viewer plays, the `Planning/schedule#SCHEDULE` `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keying the animation only on a re-sequenced plan.
- Packages: SharpGLTF.Core, SharpGLTF.Runtime, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new keyframe channel (a translation track lowering an element into place, a color track tinting the active task) is one `Animation.Create*Channel` arm on the same fold; a new interpolation mode rides the SharpGLTF `bool linear` channel knob; a new time-axis policy is one column on `ScheduleAnimationPolicy`; never a per-element `Animation` instance, never a hand-authored glTF animation JSON block, and never a second time axis beside the `ConstructionState.At` `Interval`.
- Boundary: the keyframe author rides the SharpGLTF `ModelRoot.CreateAnimation` + `Animation.CreateVisibilityChannel`/`CreateScaleChannel`/`CreateTranslationChannel` keyframe surface (`.api/api-sharpgltf` `ModelRoot.CreateAnimation` entrypoint 07, `Animation`/`AnimationChannel` rows 14-15, `AnimationInterpolationMode` row 07) — a hand-authored glTF `animations[]`/`samplers[]`/`channels[]` JSON block is the deleted form, the `Create*Channel(Node, IReadOnlyDictionary<float, T> keyframes, bool linear)` overloads own the sampler/channel/accessor emission and the keyframe dictionary is the float-seconds→value map; the `KHR_node_visibility` extension drives the per-element visibility keyframe so the `bool` track is the settled `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` row, registered once through the factory — a custom visibility-by-opacity hack is the deleted form; the SharpGLTF.Runtime pin (already csproj-referenced, exercised today only for `IMeshDecoder`/`SceneTemplate` decode) is the load-bearing surface this leg exploits, no new package and no new `InterchangeFormat` row; the animation time axis is the `Planning/schedule#SCHEDULE` `ConstructionTask.Interval` projected to seconds and a second clock on the export side is the named seam violation — the `ConstructionState.At` snapshot and the keyframe author read the one `Interval`-to-`Instant` axis; the per-element glTF `Node` resolves through the CALLER-SUPPLIED `GlobalId→Node` index (keyed by the seam `Object.ExternalId`) and a re-built scene graph in this leg is the deleted form — `SceneOf` builds one rigid mesh, not a per-element node index, so the caller owns the per-element scene; a 4D-emit fault lowers onto `Model/faults#FAULT_BAND` `BimFault` through the `Try.lift(...).Run().MapFail(...)` funnel.

```csharp signature
public sealed record ScheduleAnimationPolicy(double SecondsPerDay, bool Grow, double EpsilonSeconds) {
    public static readonly ScheduleAnimationPolicy Default = new(SecondsPerDay: 1.0, Grow: false, EpsilonSeconds: 0.001);
    public static readonly ScheduleAnimationPolicy Growing = Default with { Grow = true };

    public float SecondsOf(Instant moment, Instant projectStart) =>
        (float)((moment - projectStart).TotalDays * SecondsPerDay);
}

public sealed record AnimationTrack(string GlobalId, float AppearSeconds, float FullSeconds, int NodeIndex);

public static partial class BimExport {
    // The element GlobalId->glTF Node index is CALLER-SUPPLIED (the seam Object.ExternalId keys it): SceneOf builds
    // one rigid mesh, not a per-element node index, so the caller owns the per-element scene and this leg only binds
    // keyframes onto the supplied nodes — never a re-walked scene graph, never a retired BimModel.
    public static Fin<Seq<AnimationTrack>> AnimateSchedule(
        ModelRoot model, ScheduleNetwork network, ScheduleAnimationPolicy policy, IReadOnlyDictionary<string, SharpGLTF.Schema2.Node> nodes, Op key) =>
        Try.lift(() => Author(model, network, policy, nodes)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"schedule-animation:{error.Message}"));

    static Seq<AnimationTrack> Author(
        ModelRoot model, ScheduleNetwork network, ScheduleAnimationPolicy policy, IReadOnlyDictionary<string, SharpGLTF.Schema2.Node> nodes) {
        KhrExtension.NodeVisibility.Register();
        var projectStart = network.Tasks.Map(static t => t.Scheduled.Start).Min();
        var animation = model.CreateAnimation("construction-sequence");
        var windows = network.Assignments
            .Bind(a => network.Tasks.Find(t => t.GlobalId == a.TaskGlobalId)
                .Map(task => a.ElementGlobalIds.Map(id => (Id: id, task.Scheduled))).IfNone(Seq<(string, Interval)>()))
            .GroupBy(static row => row.Item1)
            .ToMap(static g => g.Key, static g => g.Map(static row => row.Item2));
        return windows
            .Choose(pair => Optional(nodes.TryGetValue(pair.Key, out var node) ? node : null)
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
- Entry: `RoundTrip.Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, ClockPolicy clocks)` runs the source graph through one IFC serialization and back — emitting through the `EXPORT_RAIL` `BimExport.ExportIfc` (which delegates to `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit`), re-decoding the IFC text into a `DatabaseIfc` through `Decode` (STEP `DatabaseIfc.ParseString`, ifcXML `ReadXMLDoc`, ifcJSON `ReadJSON`), re-projecting through a fresh `SemanticProjector(db)` and folding the delta onto a `Genesis(source.Header)` seed through the seam `Projection/projection#PROJECTION` `ProjectionAssembly.Assemble` (the `IfcLegality` constraint admitting the re-imported edges), then comparing the source and reimported graphs by baked-element member diff — `Fin<T>` aborts on a codec reject, a re-decode fault, or a predefined-gate reject in either leg (`Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected`/`UnmappedClass`) lifting BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), no `.ToError()` hop; `RoundTrip.Matrix(ElementGraph source, ProjectionContext ctx, ClockPolicy clocks)` lifts the verify over the IFC STEP/XML/JSON triad (`InterchangeFormat.Ifc`/`IfcXml`/`IfcJson`) onto the per-format `Map<string, RoundTripReport>` fidelity matrix so a single call witnesses which serialization preserves which field.
- Auto: `Verify` emits the source graph through the IFC serialization, re-decodes the bytes into a live `DatabaseIfc`, re-projects and assembles the reimported `ElementGraph`, and folds the source-vs-reimported comparison through the seam diff: each rooted `Object` node is baked into an `Element` keyed by its stable 1:1 `ExternalId` GlobalId (the `NodeId` is freshly minted each re-ingest [H6], so the join is the GlobalId, never the id), a source GlobalId whose reimported baked `Element` carries no divergent member is a lossless element, a divergence names the changed members through `Element.EqualityComparer.Default.Inequalities` (the `Generator.Equals` structured diff, EXCLUDING the freshly-minted `Id` and the `Parts` composition paths the child element rows already own) onto the lossy-field set, and a source GlobalId absent from the reimport is a dropped element; the report reads the lossless-element count, the per-element divergent-member set (a serialization that drops a property data type or a quantity unit surfaces the exact `Properties[..].DataType`/`Quantities[..].Unit` member path), and the dropped set, the geometry leg crossing the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion; `Matrix` runs the same `Verify` over each `InterchangeFormat` triad row, the per-format reports keyed by format so a per-format fidelity comparison reads one matrix.
- Receipt: the `RoundTripReport` per format is the codec-fidelity evidence — a per-format fidelity matrix proving which serialization preserves which field, an interchange-policy losslessness witness, and a codec regression oracle; the STEP report typically reads the highest match ratio (the canonical IFC physical file), the XML/JSON reports surfacing any serialization-specific field loss, and the divergent-member set the exact members a round-trip drops.
- Packages: GeometryGymIFC_Core, Rasm.Element, Generator.Equals, LanguageExt.Core, NodaTime, Rasm
- Growth: a new serialization format is one `InterchangeFormat` row the `Matrix` triad widens to; a new fidelity dimension (a placement-key match, a coverage round-trip) is one column on `RoundTripReport` over the same baked-element diff; a new comparison basis rides the existing `Generator.Equals` `Inequalities`; never a second element-comparison surface, never a per-format report record family, and never a parallel fidelity store.
- Boundary: the round-trip fold reuses the seam's `Generator.Equals` `Inequalities` member diff as the fidelity metric rather than minting a second element-comparison surface — a field-by-field string compare or a `Seq("content")` placeholder is the deleted form, the structured diff naming the EXACT divergent member path; the cycle composes the `EXPORT_RAIL` `ExportIfc` egress (itself delegating to `SemanticProjector.Emit`) and a fresh `SemanticProjector(db)` re-ingest folded through the seam `ProjectionAssembly.Assemble`, never the retired `BimModel.Project`/`IfcSemanticModel` lossy-row path and never a hand-rolled IFC re-author; the join is the stable 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], and a NodeId-keyed join is the deleted form; the geometry leg crosses the `tessellation#TESSELLATION_BRIDGE` companion so the matrix witnesses semantic-graph and property fidelity in-process while geometry fidelity rides the same companion, and the verification couples to no host geometry type; the `RoundTripReport` is partitioned by `InterchangeFormat` over the one baked-element diff and a per-format `StepReport`/`XmlReport`/`JsonReport` class family is the deleted form; a round-trip rejection lifts the typed `BimFault` case BARE onto the `Fin<T>` rail (band 2600, `Expected`-derived), with no `.ToError()` hop.

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

    // The lossless cycle over the SEAM graph: ExportIfc (-> SemanticProjector.Emit) seals the IFC text, Decode
    // re-builds the live DatabaseIfc, a fresh SemanticProjector(db) re-projects, and ProjectionAssembly.Assemble
    // folds the delta onto a Genesis(source.Header) seed under the IfcLegality constraint — then Compare witnesses
    // fidelity by the seam member diff. The egress projector's ctor db is unused by Emit (it builds its own target
    // from the graph header), so an empty DatabaseIfc seeds it; the harness resolves no ProfileSet IfcProfileDef.
    public static Fin<RoundTripReport> Verify(ElementGraph source, InterchangeFormat format, ProjectionContext ctx, ClockPolicy clocks) =>
        BimExport.ExportIfc(format, source, new SemanticProjector(new DatabaseIfc()), InterchangePolicy.Canonical, clocks, Option<ElementGraph>.None, static _ => Option<IfcProfileDef>.None, ctx.Key)
            .Bind(artifact => Decode(Encoding.UTF8.GetString(artifact.Bytes.Span), BimExport.SerializationOf(format), ctx.Key))
            .Bind(db => ProjectionAssembly.Assemble(
                Seq<IElementProjection>(new SemanticProjector(db)),
                Seq<IGraphConstraint>(new IfcLegality()),
                ElementGraph.Genesis(source.Header), ctx, ctx.Key))
            .Map(reimported => Compare(format.Key, source, reimported, ctx.Key));

    public static Fin<Map<string, RoundTripReport>> Matrix(ElementGraph source, ProjectionContext ctx, ClockPolicy clocks) =>
        IfcTriad.TraverseM(format => Verify(source, format, ctx, clocks).Map(report => (format.Key, report))).As()
            .Map(static rows => rows.ToMap());

    // Re-decode the emitted IFC text per serialization into a live DatabaseIfc the projector re-ingests: STEP through
    // the static DatabaseIfc.ParseString, ifcXML through ReadXMLDoc over an XmlDocument, ifcJSON through ReadJSON over
    // the parsed JsonObject — the same GeometryGym decode the import rail composes, captured at the boundary.
    static Fin<DatabaseIfc> Decode(string text, FormatIfcSerialization format, Op key) =>
        Try.lift(() => format switch {
            FormatIfcSerialization.XML  => XmlDecoded(text),
            FormatIfcSerialization.JSON => JsonDecoded(text),
            _                           => DatabaseIfc.ParseString(text),
        }).Run().MapFail(error => new BimFault.ModelRejected(key, $"roundtrip-reimport:{error.Message}"));

    static DatabaseIfc XmlDecoded(string text) {
        var db = new DatabaseIfc();
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(text);
        db.ReadXMLDoc(doc);
        return db;
    }

    static DatabaseIfc JsonDecoded(string text) {
        var db = new DatabaseIfc();
        db.ReadJSON((System.Text.Json.Nodes.JsonObject)System.Text.Json.Nodes.JsonNode.Parse(text)!);
        return db;
    }

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

- [IFC_EGRESS_DELEGATION]: the IFC STEP/XML/JSON egress is NOT re-authored on this rail — `ExportIfc` delegates to the `Projection/semantic#IFC_EGRESS` `SemanticProjector.Emit(ElementGraph graph, FormatIfcSerialization format, Option<ElementGraph> prior, Func<ProfileRef, Option<IfcProfileDef>> profiles) → Fin<string>` Bim-internal re-author (the seam `ElementGraph` is the egress source of truth: the per-`Object` `PredefinedType` egress gate + schema-span validation [C6][H8], the 1:1 `GlobalId` round-trip from `Object.ExternalId` [H6], the diff-derived `OwnerHistory` `ChangeAction` against `prior` [H9], and the `ReauthorMaterials`/`ReauthorClassifications`/`ReauthorRelationships` re-author), this rail retaining only the `CanExport`/`GeometryGym` capability gate, the `InterchangeFormat`→`FormatIfcSerialization.STEP`/`XML`/`JSON` row map (`SerializationOf`, verified against the GeometryGym 25.7.30 `FormatIfcSerialization` enum + `DatabaseIfc.ToString(FormatIfcSerialization)`), and the `ExportArtifact` content-key seal; the deleted `IfcBytes`/`IfcBuildingElementProxy` hand-roll (proxy elements only — no real `IfcClass`, no `PredefinedType`, no properties/quantities/relationships, no owner-history, no materials/classifications) was a lossy SECOND IFC-egress owner the seam forbids, and the `SemanticProjector` instance ignoring its ctor `db` in `Emit` (it builds its own target from `graph.Header`) is the one cross-file friction this delegation surfaces.
- [ROUNDTRIP_CYCLE]: the `RoundTrip.Verify` cycle re-decodes the emitted IFC text into a live `DatabaseIfc` through the decompile-verified GeometryGym 25.7.30 members `DatabaseIfc.ParseString(string)` (static, STEP), the instance `DatabaseIfc.ReadXMLDoc(XmlDocument)` (ifcXML), and the instance `DatabaseIfc.ReadJSON(JsonObject)` (ifcJSON), re-projects through a fresh `SemanticProjector(db)`, and folds the delta onto a `Graph/element#ELEMENT_GRAPH` `Genesis(source.Header)` seed through the seam `Projection/projection#PROJECTION` `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx, key)` under the `IfcLegality : IGraphConstraint` constraint; the fidelity metric is the baked-`Element` `Generator.Equals` structured diff `Element.EqualityComparer.Default.Inequalities(source, reimported)` → `Inequality.Path` `MemberPath` member paths (verified against `libs/csharp/.api/api-generator-equals` — `Inequality.Path`, `MemberPath.Segments`, `MemberPathSegment.Kind`/`Value`, `MemberPathSegmentKind.Property`), joined on the 1:1 `ExternalId` GlobalId because the rooted `NodeId` is freshly minted each ingest [H6], the `Id`/`Parts` paths filtered so each row reports the element's OWN divergent members and a dropped child surfaces via `dropped` — the seam member diff aligns with the kernel content key by sharing the ONE canonical member set, never re-implementing it nor the stale `Review/diff#MODEL_DIFF` `ElementFingerprint`/`ModelDiff.Between` over the retired `BimModel`.
- [SCHEDULE_ANIMATION_SURFACE]: the SharpGLTF keyframe-authoring members the `AnimateSchedule` fold composes are decompile-verified — `ModelRoot.CreateAnimation(string name)` returns the new `Animation` added to `LogicalAnimations`, and the `Animation` carries the public keyframe-channel authoring surface `CreateVisibilityChannel(Node, IReadOnlyDictionary<float, bool> keyframes)` (the `KHR_node_visibility` per-node visibility track, `STEP`-interpolated by construction), `CreateScaleChannel(Node, IReadOnlyDictionary<float, Vector3> keyframes, bool linear)`, `CreateTranslationChannel(Node, IReadOnlyDictionary<float, Vector3> keyframes, bool linear)`, and `CreateRotationChannel(Node, IReadOnlyDictionary<float, Quaternion> keyframes, bool linear)` — so the keyframe dictionary is the float-seconds→value map the schedule projection builds and the `bool linear` knob selects the `AnimationInterpolationMode.LINEAR`/`STEP`; the `KHR_node_visibility` extension is the `format#FORMAT_AXIS` `KhrExtension.NodeVisibility` `KhrSlot.Scene` row registered once before the author; the per-element glTF `Node` binding is the CALLER-SUPPLIED `GlobalId→Node` index (the element GlobalId is the seam `Graph/element#ELEMENT_GRAPH` `Object.ExternalId`; `SceneOf` builds ONE rigid mesh, not a per-element index, so the caller owns the per-element scene and the animation never re-walks it), and the SharpGLTF.Runtime pin the leg exploits is the already-referenced package no new admission needs.
- [ANIMATION_TIME_AXIS]: the animation time axis is the `Planning/schedule#SCHEDULE` `ConstructionTask.Scheduled` `Interval` projected to glTF seconds through the NodaTime `Duration` from the project start — `policy.SecondsOf(moment, projectStart)` reads `(moment − projectStart).TotalDays × SecondsPerDay` so the keyframe author and the `Planning/schedule#SCHEDULE` `ConstructionState.At(Instant)` snapshot read the one `Interval`-to-`Instant` axis (a scrub at glTF time `t` shows exactly `ConstructionState.At` at the inverse instant), the NodaTime `Duration.TotalDays`/`Interval.Start`/`Interval.End` surface owning the time arithmetic; the `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` re-keys the animation only on a re-sequenced plan so the animated GLB content-addresses through the same schedule key the report reads.

- [LOD_MESHLET_SURFACE]: the `Alimer.Bindings.MeshOptimizer` LOD/meshlet members the `BimLod` fold composes — `Meshopt.Simplify` (entrypoint 1, error-threshold decimation), `Meshopt.SimplifySloppy` (entrypoint 4, aggressive fallback), `Meshopt.SimplifyScale` (entrypoint 7, world-space error normalization), `Meshopt.BuildMeshlets` (meshlet entrypoint 1, cone-culling cluster), `Meshopt.BuildMeshletsBound` (entrypoint 3, buffer bound), `Meshopt.OptimizeMeshlet` (entrypoint 6, per-meshlet cache reorder), and the `Meshlet` struct (`vertex_offset`/`triangle_offset`/`vertex_count`/`triangle_count`) — are decompile-verified in the `.api/api-alimer-meshoptimizer` catalogue; the pinned-pointer simplify/meshlet call convention (the `nuint` count/stride arguments and the `SimplificationOptions` flag set) mirrors the settled `EXPORT_RAIL` `MeshoptBytes` pinned-pointer encode so the LOD leg reuses the package's native interop convention rather than a second binding — the pointer-overload `Meshopt.Simplify(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, target_index_count, target_error, SimplificationOptions, result_error)` is ten arguments, and the pointer-overload `Meshopt.SimplifySloppy(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, vertex_lock, target_index_count, target_error, result_error)` carries the `Byte* vertex_lock` per-vertex lock mask between the stride and the target count (passed `(byte*)null` for the no-lock decimation), distinct from the seven-argument `Span<uint>` overload — and the per-LOD `InterchangeIdentity.Key` content key per detail level meets `csharp:Rasm.Compute#TILE_PARTITION` at the codec admission gate.
- [CONTENT_IDENTITY_CONSUME]: the `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation is owned at `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and consumed here for the `ExportArtifact.ContentKey` and the per-`LodLevel` content-key slots; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the artifact lands content-addressed on the Persistence blob lane through the Compute `InterchangeIdentity.Admit` path — Bim mints no second identity scheme and no second blob owner.
- [TILE_FEATURE_BINDING]: the `SharpGLTF.Ext.3DTiles` per-tile metadata author members the `TileMetadata.Attach` fold composes — `Tiles3DExtensions.RegisterExtensions`/`UseStructuralMetadata`, `EXTStructuralMetadataRoot.UseEmbeddedSchema`/`AddPropertyTable`, `StructuralMetadataSchema.UseClassMetadata`/`UseEnumMetadata`, `StructuralMetadataClass.UseProperty`, `StructuralMetadataClassProperty.WithStringType`/`WithEnumeration`, `PropertyTable.UseProperty`, `PropertyTableProperty.SetValues<T>`, `new FeatureIDBuilder(...)`, and `MeshPrimitive.AddMeshFeatureIds` — are decompile-verified in the `Rasm.Bim/.api/api-sharpgltf-3dtiles` catalogue; the per-vertex feature-ID attribute index that binds each primitive vertex span to its element `PropertyTable` row (the `EXT_mesh_features` attribute accessor the `SceneOf` mesh build emits) grounds at the codec admission gate where the GLB primitive vertex-to-element mapping is settled against the `EXPORT_RAIL` triangle-soup layout, and the tile-pyramid partitioning rides `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam.
- [TILE_AVAILABILITY_SURFACE]: the `subtree` authoring members the `TileAvailability` fold composes are decompile-verified — `subtree.Tile` ctors `(int z, int x, int y)` / `(int z, int x, int y, bool available)` set the `Z`/`X`/`Y` coordinates where `Z` is the quadtree subdivision level (`MortonIndex.GetMortonIndices` buckets availability by `tile.Z`, `Tile.GetChildren` emits `Z+1` children, `Tile.Parent` climbs to `Z-1`) and the `Lod` property is an auxiliary the quadtree author never reads, so `TileOf` maps `node.Lod` onto the ctor `z` slot (the level), never a dropped coordinate; `subtree.Tile3D` ctor `(int level, int x, int y, int z)` sets a distinct `Level` subdivision axis plus a true third spatial `Z` (`MortonIndex.GetMortonIndices3D` buckets by `t.Level` and sets the `BitArray3D` cell at `X`/`Y`/`Z`, `Tile3D.Parent` halving all three), so `TileOf3D` supplies `node.Z` for the octree vertical axis; `SubtreeCreator`/`SubtreeCreator3D` `GenerateSubtreefile`/`GenerateSubtreefiles` author the binary, the multi-subtree `GenerateSubtreefiles` keying its `Dictionary<Tile[3D], byte[]>` by roots the library builds through `new Tile(level, x, y)`/`new Tile3D(level, x, y, z)` whose `Lod` stays zero — so `AuthorMany` keys by the root `(Level, X, Y, Z)` coordinate, never `Lod`; `ImplicitSubdivisionScheme` is the `{ Quadtree, Octree }` enum the `Author`/`AuthorMany` fold switches on, the `Tile`/`Tile3D`, `SubtreeCreator`/`SubtreeCreator3D`, and `MortonIndex` members verified in the `.api/api-subtree` catalogue.
