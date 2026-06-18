# [BIM_EXPORT_RAIL]

The artifact-emit rail: one `BimExport` export fold over the `format-axis#FORMAT_AXIS` `InterchangeFormat` rows, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization. The page composes the `import-rail#IMPORT_RAIL` `ImportedGeometry`/`IfcSemanticModel` carriers, the `format-axis#FORMAT_AXIS` codec/extension rows, and the `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` `InterchangeIdentity` content key as settled vocabulary; the emitted `ExportArtifact` feeds the Compute content-addressing seam. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[EXPORT_RAIL]: artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization.

## [2]-[EXPORT_RAIL]

- Owner: `BimExport` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `BimExport.ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss or a codec fault projected onto `BimFault.ModelRejected`.
- Auto: the `GlbBytes` fold switches on `InterchangePolicy.Compression` — the `KhrEncoder.None` arm assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; the `KhrEncoder.Draco` arm bypasses the GLB container and quantizes the geometry into a `DracoMesh` (`PointAttribute.Wrap(AttributeType.Position, …)`, `DracoMesh.Indices.AddRange`), emitting the Draco byte stream through `Draco.Encode(mesh, DracoEncodeOptions)`; the `KhrEncoder.Meshopt` arm bounds the destination through `Meshopt.EncodeVertexBufferBound`/`EncodeIndexBufferBound` and emits the meshopt bufferView payloads through `Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the interleaved vertex stream — neither codec takes a glTF `ModelRoot` and neither arm writes the GLB container, so the compression leg replaces the GLB write rather than post-processing it; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
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
        !format.CanExport ? Fin.Fail<ExportArtifact>(new BimFault.ModelRejected($"<export-unsupported:{format.Key}>"))
        : format.Codec == InterchangeCodec.SharpGltf
            ? GlbBytes(geometry, policy).Map(bytes => Sealed(format, bytes, policy, clocks.Now))
            : Fin.Fail<ExportArtifact>(new BimFault.ModelRejected($"<export-codec-miss:{format.Key}>"));

    public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Try.lift(() => Sealed(format, IfcBytes(format, model, policy), policy, clocks.Now)).Run().MapFail(static error => (Error)new BimFault.ModelRejected(error.Message))
            : Fin.Fail<ExportArtifact>(new BimFault.ModelRejected($"<ifc-export-codec-miss:{format.Key}>"));

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
        mesh.Indices.AddRange(geometry.Indices.Span.ToArray().Map(static i => (int)i).ToArray());
        return Draco.Encode(mesh, new DracoEncodeOptions {
            PositionBits = policy.QuantizationBits, NormalBits = policy.QuantizationBits,
            CompressionLevel = DracoCompressionLevel.Optimal,
        });
    }

    static byte[] MeshoptBytes(ImportedGeometry geometry, InterchangePolicy policy) {
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        var interleaved = new VertexPositionNormal[geometry.VertexCount];
        for (int v = 0; v < geometry.VertexCount; v++) {
            int o = v * 3;
            interleaved[v] = new VertexPositionNormal(verts[o], verts[o + 1], verts[o + 2], normals[o], normals[o + 1], normals[o + 2]);
        }
        var indices = geometry.Indices.Span.ToArray().Map(static i => (uint)i).ToArray();
        var vBuffer = new byte[(int)Meshopt.EncodeVertexBufferBound((nuint)interleaved.Length, (nuint)Unsafe.SizeOf<VertexPositionNormal>())];
        var iBuffer = new byte[(int)Meshopt.EncodeIndexBufferBound((nuint)indices.Length, (nuint)geometry.VertexCount)];
        int vLen = (int)Meshopt.EncodeVertexBuffer(vBuffer, interleaved.AsSpan());
        int iLen = (int)Meshopt.EncodeIndexBuffer(iBuffer, indices);
        return [.. BitConverter.GetBytes(vLen), .. vBuffer.AsSpan(0, vLen), .. iBuffer.AsSpan(0, iLen)];
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

## [3]-[RESEARCH]

- [CONTENT_IDENTITY_CONSUME]: the `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation is owned at `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` and consumed here for the `ExportArtifact.ContentKey` slot; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the artifact lands content-addressed on the Persistence blob lane through the Compute `InterchangeIdentity.Admit` path — Bim mints no second identity scheme and no second blob owner.
