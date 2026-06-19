# [BIM_EXPORT_RAIL]

The artifact-emit rail: one `BimExport` export fold over the `format-axis#FORMAT_AXIS` `InterchangeFormat` rows, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization. The page composes the `import-rail#IMPORT_RAIL` `ImportedGeometry`/`IfcSemanticModel` carriers, the `format-axis#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key as settled vocabulary; the emitted `ExportArtifact` feeds the Compute content-addressing seam. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[EXPORT_RAIL]: artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization.
- [3]-[TILE_METADATA]: per-tile `EXT_structural_metadata` schema/class/property-table over the `BimElement` semantic, bound to the GLB primitive through `EXT_mesh_features` feature IDs.
- [4]-[BIM_LOD]: the per-element LOD pyramid through `Meshopt.Simplify`/`SimplifySloppy`, the `Meshopt.BuildMeshlets` meshlet residency band, and the per-LOD content key the `Rasm.Compute#TILE_PARTITION` pyramid addresses.

## [2]-[EXPORT_RAIL]

- Owner: `BimExport` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `BimExport.ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss (`faults#FAULT_BAND` `BimFault.CodecReject`) or a captured GeometryGym serialization fault (`BimFault.ModelRejected`), each lowered with `.ToError()`.
- Auto: the `GlbBytes` fold switches on `InterchangePolicy.Compression` — the `KhrEncoder.None` arm assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; the `KhrEncoder.Draco` arm bypasses the GLB container and quantizes the geometry into a `DracoMesh` (`PointAttribute.Wrap(AttributeType.Position, …)` per-attribute wrap, `DracoMesh.AddFace(int[])` per triangle), emitting the Draco byte stream through `Draco.Encode(mesh, DracoEncodeOptions)`; the `KhrEncoder.Meshopt` arm first runs the catalogued meshopt optimization pipeline — `GenerateVertexRemap` deduplicates the exploded triangle-soup into a unique-vertex set, `RemapVertexBuffer`/`RemapIndexBuffer` apply the remap, then `OptimizeVertexCache`/`OptimizeOverdraw`/`OptimizeVertexFetch` reorder for GPU cache, overdraw, and fetch locality — bounds the destination through `EncodeVertexBufferBound`/`EncodeIndexBufferBound`, and emits the meshopt bufferView payloads through the pinned-pointer `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the optimized indexed buffers; neither codec takes a glTF `ModelRoot` and neither arm writes the GLB container, so the compression leg replaces the GLB write rather than post-processing it; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
- Receipt: the `ModelEmit` receipt case carries the format key, codec key, emitted byte count, and the `ExportArtifact.ContentKey` the Compute addressing seam computes, symmetric to the import `ModelLoad` case; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, NodaTime, LanguageExt.Core
- Growth: a new managed export is one codec arm on the export fold; a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold.
- Boundary: the export fold extends the `BimExport` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the Compute content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)` before any write so a material/light/texture channel serializes through its decompile-verified SharpGLTF schema type rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression` and `KHR_meshopt_compression` rows carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode through the static `Draco.Encode(DracoPointCloud, DracoEncodeOptions)` over a `DracoMesh` built from `PointAttribute.Wrap` attributes, and `Alimer.Bindings.MeshOptimizer` owns the meshopt encode through the static `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the raw vertex/index buffers, both quantizing to the `InterchangePolicy` bit budget; both packages are the outside-Rhino EXPORT_RAIL concern, so the `GlbBytes` fold routes the compression leg through those packages and a managed in-Rhino-ALC compression encode is the rejected form, and a glTF `ModelRoot` passed to either codec is the rejected form because neither package owns a glTF model type; IFC serialization selects `FormatIfcSerialization.STEP`/`XML`/`JSON` by the row and `DatabaseIfc.WriteStream`/`ToString` are the only emit members — a hand-rolled STEP writer is the deleted form; the model graph re-authoring runs through `FactoryIfc` canonical axes, origins, and owner history so a round-tripped model carries stable GlobalIds through `ParserIfc.HashGlobalID` keyed on a stable entity key rather than a fresh GUID per export, making export idempotent under the Compute content-key; a write to a row whose `CanExport` is false faults at the boundary; the chunked-field and structural-delta codecs stay at `Rasm.Compute/interchange/codecs` consumed at the seam.

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
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
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

public static class BimExport {
    public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) =>
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.CodecReject($"export-unsupported:{format.Key}").ToError())
        : format.Codec == InterchangeCodec.SharpGltf
            ? GlbBytes(geometry, policy).Map(bytes => Sealed(format, bytes, policy, clocks.Now))
            : Fin.Fail<ExportArtifact>(new BimFault.CodecReject($"export-codec-miss:{format.Key}").ToError());

    public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Try.lift(() => Sealed(format, IfcBytes(format, model, policy), policy, clocks.Now)).Run().MapFail(static error => new BimFault.ModelRejected(error.Message).ToError())
            : Fin.Fail<ExportArtifact>(new BimFault.CodecReject($"ifc-export-codec-miss:{format.Key}").ToError());

    static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    static Fin<byte[]> GlbBytes(ImportedGeometry geometry, InterchangePolicy policy) =>
        RegisterExtensions(policy).Map(_ => policy.Compression switch {
            KhrEncoder.Draco => DracoBytes(geometry, policy),
            KhrEncoder.Meshopt => MeshoptBytes(geometry, policy),
            _ => WriteGlb(SceneOf(geometry), policy),
        });

    static ModelRoot SceneOf(ImportedGeometry geometry) {
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
        return scene.ToGltf2(new SceneBuilderSchema2Settings { UseStridedBuffers = true });
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

    static byte[] IfcBytes(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy) {
        var db = new DatabaseIfc(true, policy.IfcSchema);
        var site = db.Project.UppermostSite();
        var storeys = toMap(model.Spatial
            .Map(node => (node.GlobalId, storey: new IfcBuildingStorey(site, node.Name, 0.0) { GlobalId = ParserIfc.HashGlobalID(node.GlobalId), LongName = node.LongName })));
        var container = toMap(model.Spatial.SelectMany(node => node.ContainedGlobalIds.Map(id => (id, node.GlobalId))));
        model.Products.Iter(row => {
            var host = container.Find(row.GlobalId).Bind(storeys.Find).Map(static s => (IfcSpatialElement)s).IfNone(() => site);
            var element = new IfcBuildingElementProxy(host, null, null) { GlobalId = ParserIfc.HashGlobalID(row.GlobalId), Name = row.Name, Tag = row.Tag };
            model.Materials.Filter(m => m.OwnerGlobalId == row.GlobalId).Iter(m => element.SetMaterial(new IfcMaterial(db, m.MaterialName)));
        });
        return Encoding.UTF8.GetBytes(db.ToString(SerializationOf(format)));
    }

    static Fin<Unit> RegisterExtensions(InterchangePolicy policy) =>
        policy.Extensions.Traverse(static khr => khr.Register()).Map(static _ => unit);

    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes, InterchangeIdentity.Key(format.Key, bytes, policy.Deflection, policy.Tolerance, policy.AngleTolerance), bytes.LongLength, at);
}
```

## [3]-[TILE_METADATA]

- Owner: `TileMetadata` the per-tile `EXT_structural_metadata` author over the `model/elements#ELEMENT_MODEL` `BimElement` semantic — one embedded schema carrying the canonical BIM class, GlobalId, name, and the property/quantity/classification columns, one `PropertyTable` per-feature value store, and the `EXT_mesh_features` feature-ID binding tying each GLB primitive vertex span to its element row so the Cesium 3D Tiles web peer resolves per-element metadata at pick time.
- Entry: `TileMetadata.Attach(ModelRoot tile, Seq<BimElement> elements)` authors the structural-metadata schema/class/property-table on the GLB the `EXPORT_RAIL` `SceneOf` builds and binds the feature IDs to the tile primitives — `Fin<T>` aborts on a registration fault captured at the boundary (`faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`; the per-tile metadata emit composes through the `Rasm.Compute` interchange codec `TILE_PARTITION` at the seam and `Rasm.Bim` authors the canonical schema shape and the extension surface.
- Auto: `Attach` runs `Tiles3DExtensions.RegisterExtensions()` once, opens the schema through `tile.UseStructuralMetadata().UseEmbeddedSchema(id)`, defines the `BimElement` class through `UseClassMetadata("BimElement")` with one `UseProperty(name).With<Type>(...)` per canonical column (`GlobalId` string, `Class` enumeration over the `IfcClass` vocabulary through `UseEnumMetadata`, `Name` string, and the per-Pset property columns), adds the per-feature `PropertyTable` through `AddPropertyTable(class, featureCount, name)` encoding each element row through `UseProperty(key).SetValues<T>(...)`, and binds the GLB primitive feature IDs through `new FeatureIDBuilder(featureCount, attributeIndex, propertyTable, ...)` then `primitive.AddMeshFeatureIds(builder)` so the `EXT_mesh_features` feature-ID attribute indexes each vertex span to its `PropertyTable` row.
- Receipt: the authored `EXT_structural_metadata` schema and `PropertyTable` are the per-tile semantic the web peer reads — the same `BimElement` vocabulary the `exchange/wire#WIRE_PROJECTION` JSON carries, projected onto the binary tile metadata so a Cesium consumer resolves per-element BIM semantics at pick without a second metadata mint.
- Packages: SharpGLTF.Core, SharpGLTF.Ext.3DTiles, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new metadata column is one `UseProperty(name).With<Type>(...)` row on the embedded class; a new feature-ID binding is one `FeatureIDBuilder` over the primitive; the `IfcClass` enumeration is one `UseEnumMetadata` row tracking the `IfcClass` vocabulary; never a hand-authored JSON metadata block and never a second per-tile metadata mint.
- Boundary: the per-tile metadata authors through the `SharpGLTF.Ext.3DTiles` `EXT_structural_metadata`/`EXT_mesh_features` surface — a hand-authored JSON `EXT_structural_metadata` block is the deleted form, the `StructuralMetadataClassProperty.With<Type>` selectors and the `PropertyTableProperty.SetValues<T>` binary encode own the schema and value storage; `Tiles3DExtensions.RegisterExtensions()` runs once before any author and the call is idempotent at the factory level; the `IfcClass` column rides `UseEnumMetadata` so the closed BIM class vocabulary serializes by its enumeration rather than a free string; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/interchange/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` admits the extension surface and the canonical schema shape, never the tile pyramid; the `OneOf<int, Texture>` feature-ID attribute selector is a transitive `OneOf` dependency consumed only by `FeatureIDBuilder` and no Bim code references it directly; the per-tile `BimElement` semantic is the same vocabulary the `exchange/wire#WIRE_PROJECTION` carries, never a second metadata vocabulary.

```csharp signature
public static class TileMetadata {
    public static Fin<ModelRoot> Attach(ModelRoot tile, Seq<BimElement> elements) =>
        Try.lift(() => Author(tile, elements)).Run().MapFail(static error => new BimFault.ModelRejected($"tile-metadata:{error.Message}").ToError());

    static ModelRoot Author(ModelRoot tile, Seq<BimElement> elements) {
        Tiles3DExtensions.RegisterExtensions();
        var root = tile.UseStructuralMetadata();
        var schema = root.UseEmbeddedSchema("rasm-bim");
        var classKinds = schema.UseEnumMetadata("IfcClass", [.. IfcClass.Items.Map(static (row, i) => (row.Key, i))]);
        var element = schema.UseClassMetadata("BimElement");
        element.UseProperty("GlobalId").WithStringType(noData: null, defaultValue: null);
        element.UseProperty("Class").WithEnumeration(classKinds, noData: null);
        element.UseProperty("Name").WithStringType(noData: null, defaultValue: null);
        var table = root.AddPropertyTable(element, elements.Count, "elements");
        table.UseProperty("GlobalId").SetValues([.. elements.Map(static e => e.GlobalId)]);
        table.UseProperty("Class").SetValues([.. elements.Map(static e => IfcClass.Items.ToSeq().FindIndex(r => r == e.Class))]);
        table.UseProperty("Name").SetValues([.. elements.Map(static e => e.Name)]);
        tile.LogicalMeshes
            .SelectMany(static mesh => mesh.Primitives)
            .Iter((primitive, index) => primitive.AddMeshFeatureIds(
                new FeatureIDBuilder(elements.Count, attributeOrTexture: 0, propertyTable: table, channels: null, label: "elements", nullFeatureId: null)));
        return tile;
    }
}
```

## [4]-[BIM_LOD]

- Owner: `BimLod` the per-element LOD-pyramid leg ADDITIVE to the export rail — one progressive-detail chain per element derived through the catalogued `Meshopt.Simplify`/`SimplifySloppy` decimation keyed by target triangle ratio, plus the `MeshletResidency` band through `Meshopt.BuildMeshlets` for the WebGPU raster path; `LodLevel` the per-level record carrying the decimated index buffer, the target ratio, and the per-LOD content key the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses.
- Entry: `BimLod.Pyramid(ImportedGeometry geometry, InterchangePolicy policy)` derives the LOD chain over the policy's ratio schedule (each level a `Meshopt.Simplify` at decreasing target index count, falling back to `Meshopt.SimplifySloppy` when the error threshold cannot be met), and `BimLod.Meshlets(ImportedGeometry geometry)` clusters the residency band through `Meshopt.BuildMeshlets` (bounded by `Meshopt.BuildMeshletsBound`, optimized per meshlet through `Meshopt.OptimizeMeshlet`) — `Fin<T>` aborts on a degenerate decimation captured at the boundary (`faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`; each level seals its own `ExportArtifact.ContentKey` so the web peer streams each LOD by view distance, the `TileMetadata` per-tile semantic riding each level unchanged.
- Receipt: each `LodLevel` carries its target ratio, resulting triangle count, and per-LOD content key — the same `InterchangeIdentity` the full-resolution `ExportArtifact` seals, computed per level so the `csharp:Rasm.Compute#TILE_PARTITION` pyramid content-addresses every detail level and the cross-libs `WEB_GEOMETRY_RESIDENCY_WIRE` splat/meshlet manifest the AppUi projection mints streams each LOD by view distance.
- Packages: Alimer.Bindings.MeshOptimizer, SharpGLTF.Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new detail level is one content-keyed `LodLevel` row on the pyramid; a new meshlet bound is one `MeshletResidency` band over the residency set; the per-tile `TileMetadata` semantic rides each LOD unchanged; never a per-element full-resolution emit and never a second LOD or residency owner.
- Boundary: the LOD decimation is `Alimer.Bindings.MeshOptimizer`'s — `Meshopt.Simplify` (error-threshold decimation with `SimplificationOptions` flags) and `Meshopt.SimplifySloppy` (aggressive fallback) over the optimized indexed buffer own the LOD chain, and a hand-rolled edge-collapse decimator is the deleted form; the meshlet residency rides `Meshopt.BuildMeshlets` (allocated via `BuildMeshletsBound`, optimized per meshlet via `OptimizeMeshlet`) so the WebGPU raster path consumes the package-owned meshlet partition, never a hand-rolled cluster algorithm; the per-LOD content key meets `csharp:Rasm.Compute#TILE_PARTITION` at the seam — `Rasm.Bim` derives the per-element pyramid and seals each level's content key, the tile-pyramid partitioning and streaming stay at Compute consumed at the seam; the residency band feeds the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest the AppUi projection mints, never a second residency owner; the LOD leg composes the same `ImportedGeometry` triangle-soup the `EXPORT_RAIL` `SceneOf` reads, never a second geometry carrier.

```csharp signature
public sealed record LodLevel(int Level, double TargetRatio, int TriangleCount, ReadOnlyMemory<uint> Indices, UInt128 ContentKey);

public static class BimLod {
    public static Fin<Seq<LodLevel>> Pyramid(ImportedGeometry geometry, InterchangePolicy policy) =>
        Try.lift(() => Levels(geometry, policy)).Run().MapFail(static error => new BimFault.ModelRejected($"lod-decimate:{error.Message}").ToError());

    static unsafe Seq<LodLevel> Levels(ImportedGeometry geometry, InterchangePolicy policy) {
        var source = new uint[geometry.Indices.Length];
        for (int i = 0; i < source.Length; i++) { source[i] = (uint)geometry.Indices.Span[i]; }
        var verts = geometry.Vertices.ToArray();
        nuint vertexCount = (nuint)geometry.VertexCount;
        nuint vertexStride = (nuint)(3 * sizeof(float));
        float scale;
        fixed (float* vPtr = verts) { scale = Meshopt.SimplifyScale(vPtr, vertexCount, vertexStride); }
        return LodSchedule.Map((ratio, level) => Decimate(source, verts, vertexCount, vertexStride, scale, ratio, level, geometry.Format.Key)).ToSeq();
    }

    static unsafe LodLevel Decimate(uint[] source, float[] verts, nuint vertexCount, nuint vertexStride, float scale, double ratio, int level, string formatKey) {
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
            InterchangeIdentity.Key($"{formatKey}:lod{level}", bytes, ratio, 1e-6, 1e-4));
    }

    public static unsafe Fin<Seq<Meshlet>> Meshlets(ImportedGeometry geometry) =>
        Try.lift(() => Cluster(geometry)).Run().MapFail(static error => new BimFault.ModelRejected($"meshlet-build:{error.Message}").ToError());

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

## [5]-[RESEARCH]

- [LOD_MESHLET_SURFACE]: the `Alimer.Bindings.MeshOptimizer` LOD/meshlet members the `BimLod` fold composes — `Meshopt.Simplify` (entrypoint 1, error-threshold decimation), `Meshopt.SimplifySloppy` (entrypoint 4, aggressive fallback), `Meshopt.SimplifyScale` (entrypoint 7, world-space error normalization), `Meshopt.BuildMeshlets` (meshlet entrypoint 1, cone-culling cluster), `Meshopt.BuildMeshletsBound` (entrypoint 3, buffer bound), `Meshopt.OptimizeMeshlet` (entrypoint 6, per-meshlet cache reorder), and the `Meshlet` struct (`vertex_offset`/`triangle_offset`/`vertex_count`/`triangle_count`) — are decompile-verified in the `.api/api-alimer-meshoptimizer` catalogue; the pinned-pointer simplify/meshlet call convention (the `nuint` count/stride arguments and the `SimplificationOptions` flag set) mirrors the settled `EXPORT_RAIL` `MeshoptBytes` pinned-pointer encode so the LOD leg reuses the package's native interop convention rather than a second binding — the pointer-overload `Meshopt.Simplify(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, target_index_count, target_error, SimplificationOptions, result_error)` is ten arguments, and the pointer-overload `Meshopt.SimplifySloppy(destination, indices, index_count, vertex_positions, vertex_count, vertex_positions_stride, vertex_lock, target_index_count, target_error, result_error)` carries the `Byte* vertex_lock` per-vertex lock mask between the stride and the target count (passed `(byte*)null` for the no-lock decimation), distinct from the seven-argument `Span<uint>` overload — and the per-LOD `InterchangeIdentity.Key` content key per detail level meets `csharp:Rasm.Compute#TILE_PARTITION` at the codec admission gate.
- [CONTENT_IDENTITY_CONSUME]: the `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation is owned at `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` and consumed here for the `ExportArtifact.ContentKey` and the per-`LodLevel` content-key slots; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the artifact lands content-addressed on the Persistence blob lane through the Compute `InterchangeIdentity.Admit` path — Bim mints no second identity scheme and no second blob owner.
- [TILE_FEATURE_BINDING]: the `SharpGLTF.Ext.3DTiles` per-tile metadata author members the `TileMetadata.Attach` fold composes — `Tiles3DExtensions.RegisterExtensions`/`UseStructuralMetadata`, `EXTStructuralMetadataRoot.UseEmbeddedSchema`/`AddPropertyTable`, `StructuralMetadataSchema.UseClassMetadata`/`UseEnumMetadata`, `StructuralMetadataClass.UseProperty`, `StructuralMetadataClassProperty.WithStringType`/`WithEnumeration`, `PropertyTable.UseProperty`, `PropertyTableProperty.SetValues<T>`, `new FeatureIDBuilder(...)`, and `MeshPrimitive.AddMeshFeatureIds` — are decompile-verified in the `Rasm.Bim/.api/api-sharpgltf-3dtiles` catalogue; the per-vertex feature-ID attribute index that binds each primitive vertex span to its element `PropertyTable` row (the `EXT_mesh_features` attribute accessor the `SceneOf` mesh build emits) grounds at the codec admission gate where the GLB primitive vertex-to-element mapping is settled against the `EXPORT_RAIL` triangle-soup layout, and the tile-pyramid partitioning rides `Rasm.Compute/interchange/codecs#TILE_PARTITION` consumed at the seam.
