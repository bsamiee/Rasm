# [COMPUTE_INTERCHANGE]

Rasm.Compute interchange lane: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to geometry, point scan, simulation field, or model graph) from export (artifact to foreign bytes) across the managed glTF/GLB read-write over SharpGLTF with Draco/Meshopt compression and 3D-Tiles tiling, the managed STL/3MF/OBJ/PLY mesh decode, the managed E57/LAS/LAZ/PTS point-scan and CGNS/EnSight/VTK/Zarr chunked-field decode, the managed IFC/IFC5 STEP/XML/JSON read-write over GeometryGym with in-process semantic ingest, the ISO 10303 AP242 and native Revit/Navisworks/DWG bridges routed through the companion two-hop rail, the per-importer frame/handedness/up-axis reconciliation onto the canonical DDG-kernel frame at ingest, the chunked error-bounded field/result codec, the FastCDC structural geometry-delta codec, and the content-addressed artifact identity that folds deflection and tolerance into the key. The page owns the `InterchangeFormat`/`InterchangeCodec` axes with their capability, companion, and frame columns, the `InterchangeIo` import/export fold, the `FrameNormalization` ingest-frame surface, the `IfcSemanticModel` graph projection, the `FieldCodec` and `DeltaCodec` codecs, the `TessellationRequest` two-hop bridge, and the `InterchangeIdentity` content-key — composing the suite `XxHash128` hash law, the `ArtifactIndexRow` blob owner, the model-lane `ModelIdentity` identity precedent, the `solver-and-optimization#DISCRETIZATION_MESH` `FieldSpace` shape, and the `Substrate.RemoteGrpc` companion hop as settled vocabulary; the page is HOST-LOCAL and carries no TS_PROJECTION.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                       |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|   [1]   | FORMAT_AXIS          | Format/codec rows; capability, companion, frame-normalization columns        |
|   [2]   | IMPORT_RAIL          | Foreign-bytes ingest: mesh, point-scan, field, in-proc semantic IFC/IFC5     |
|   [3]   | EXPORT_RAIL          | Artifact emit: GLB + Draco/Meshopt + 3D-Tiles, IFC STEP/XML/JSON             |
|   [4]   | TWO_HOP_TESSELLATION | IFC/AP242/native geometry crosses to the companion, never in-proc            |
|   [5]   | FIELD_RESULT_CODEC   | Chunked simulation-field layout; error-bounded lossy/lossless; zero-copy     |
|   [6]   | GEOMETRY_DELTA       | FastCDC chunking; structural mesh/B-rep/point-cloud/NURBS delta; progressive |
|   [7]   | CONTENT_ADDRESSING   | XxHash128 artifact identity folding deflection and tolerance into the key    |

## [2]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` interchange-format rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, `TessellationRequiresCompanion`, and the `UpAxis`/`Handedness` ingest-frame columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package or companion that reads and writes the row; `UpAxis`/`Handedness` the per-importer local-frame enums; `FrameNormalization` the static reconciliation surface coercing every imported coordinate into the canonical DDG-kernel frame.
- Cases: `InterchangeFormat` rows gltf · glb · ifc · ifc-xml · ifc-json · step-ap242 · iges · stl · 3mf · obj · ply · cgns · ensight · vtk · zarr · e57 · las · pts · rvt · nwc · dwg · ifc5; `InterchangeCodec` rows sharp-gltf (SharpGLTF managed glTF 2.0) · geometry-gym (GeometryGym managed IFC/IFC5) · step-ap242 (ISO 10303 AP203/214/242 solid-model exchange) · mesh-text (STL/3MF/OBJ/PLY managed mesh decode) · point-cloud (E57/LAS/LAZ/PTS managed scan ingest) · field-chunk (CGNS/EnSight/VTK/Zarr chunked field layout) · native-companion (Revit/Navisworks/DWG native libraries through the companion process).
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or a wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the two-hop rail and a managed glTF/mesh/point-cloud/field decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change that maps glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical DDG-kernel Z-up right-handed frame so interior code never re-derives a per-importer flip.
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
    public static readonly InterchangeCodec SharpGltf = new("sharp-gltf", managed: true, companion: false);
    public static readonly InterchangeCodec GeometryGym = new("geometry-gym", managed: true, companion: false);
    public static readonly InterchangeCodec StepAp242 = new("step-ap242", managed: true, companion: false);
    public static readonly InterchangeCodec MeshText = new("mesh-text", managed: true, companion: false);
    public static readonly InterchangeCodec PointCloud = new("point-cloud", managed: true, companion: false);
    public static readonly InterchangeCodec FieldChunk = new("field-chunk", managed: true, companion: false);
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true);

    public bool Managed { get; }
    public bool Companion { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat StepAp242 = new("step-ap242", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: true, codec: InterchangeCodec.StepAp242, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Iges = new("iges", mediaType: "model/iges", extensions: Seq(".igs", ".iges"), canImport: true, canExport: false, codec: InterchangeCodec.StepAp242, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Stl = new("stl", mediaType: "model/stl", extensions: Seq(".stl"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat ThreeMf = new("3mf", mediaType: "model/3mf", extensions: Seq(".3mf"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right);
    public static readonly InterchangeFormat Obj = new("obj", mediaType: "model/obj", extensions: Seq(".obj"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right);
    public static readonly InterchangeFormat Ply = new("ply", mediaType: "model/ply", extensions: Seq(".ply"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Cgns = new("cgns", mediaType: "application/cgns", extensions: Seq(".cgns"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat EnSight = new("ensight", mediaType: "application/ensight", extensions: Seq(".case", ".encas"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Vtk = new("vtk", mediaType: "application/vtk", extensions: Seq(".vtk", ".vtu", ".vtp"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Zarr = new("zarr", mediaType: "application/zarr", extensions: Seq(".zarr"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat E57 = new("e57", mediaType: "application/e57", extensions: Seq(".e57"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Las = new("las", mediaType: "application/vnd.las", extensions: Seq(".las", ".laz"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Pts = new("pts", mediaType: "application/vnd.pts", extensions: Seq(".pts"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), canImport: true, canExport: true, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right);

    private readonly Seq<string> extensions;

    public string MediaType { get; }
    public bool CanImport { get; }
    public bool CanExport { get; }
    public InterchangeCodec Codec { get; }
    public bool TessellationRequiresCompanion { get; }
    public UpAxis UpAxis { get; }
    public Handedness Handedness { get; }

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

public enum UpAxis : byte { X = 0, Y = 1, Z = 2 }

public enum Handedness : byte { Right = 0, Left = 1 }

public static class FrameNormalization {
    static readonly UpAxis CanonicalUp = UpAxis.Z;
    static readonly Handedness CanonicalHand = Handedness.Right;

    public static void Canonicalize(InterchangeFormat format, Span<float> vertices, int stride) {
        if (format.UpAxis == CanonicalUp && format.Handedness == CanonicalHand) {
            return;
        }
        for (int offset = 0; offset + 2 < vertices.Length; offset += stride) {
            (float x, float y, float z) = (vertices[offset], vertices[offset + 1], vertices[offset + 2]);
            (float ux, float uy, float uz) = format.UpAxis switch {
                UpAxis.Y => (x, -z, y),
                UpAxis.X => (z, y, -x),
                _ => (x, y, z),
            };
            vertices[offset] = ux;
            vertices[offset + 1] = format.Handedness == CanonicalHand ? uy : -uy;
            vertices[offset + 2] = uz;
        }
    }

    public static (float ScaleX, float ScaleY, float ScaleZ) Basis(InterchangeFormat format) =>
        format.UpAxis == CanonicalUp && format.Handedness == CanonicalHand ? (1f, 1f, 1f)
        : format.Handedness == CanonicalHand ? (1f, 1f, 1f) : (1f, -1f, 1f);
}
```

## [3]-[IMPORT_RAIL]

- Owner: `InterchangeIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the managed STL/3MF/OBJ/PLY mesh decode, the managed E57/LAS/LAZ/PTS point-scan decode, the chunked CGNS/EnSight/VTK/Zarr field decode, the in-process semantic IFC/IFC5 ingest through GeometryGym, and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `PointScan` the point-cloud carrier, `FieldArtifact` the chunked simulation-field carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `public static Fin<PointScan> ImportPoints(...)` for the managed scan path; `public static Fin<FieldArtifact> ImportField(...)` for the chunked field path; `public static Fin<IfcSemanticModel> ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `ComputeFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
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

public sealed record PointScan(
    InterchangeFormat Format,
    ReadOnlyMemory<float> Positions,
    Option<ReadOnlyMemory<float>> Colors,
    Option<ReadOnlyMemory<float>> Intensity,
    long PointCount,
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
        : format.Codec == InterchangeCodec.SharpGltf ? Gltf(format, bytes, clocks.Now).Map(g => Framed(format, g))
        : format.Codec == InterchangeCodec.MeshText ? MeshDecode(format, bytes, clocks.Now).Map(g => Framed(format, g))
        : Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<import-needs-companion:{format.Key}>"));

    public static Fin<PointScan> ImportPoints(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.PointCloud
            ? Try.lift(() => ScanDecode(format, bytes, clocks.Now)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message))
            : Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{format.Key}>"));

    public static Fin<FieldArtifact> ImportField(InterchangeFormat format, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.FieldChunk
            ? FieldCodec.FieldDecode(format, bytes, policy, clocks.Now)
            : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{format.Key}>"));

    static ImportedGeometry Framed(InterchangeFormat format, ImportedGeometry geometry) {
        float[] vertices = geometry.Vertices.ToArray();
        FrameNormalization.Canonicalize(format, vertices, stride: 3);
        return geometry with { Vertices = vertices };
    }

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
    bool StridedBuffers,
    MeshCompression Compression,
    int MeshoptQuantizationBits,
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, MergeBuffers: true, StridedBuffers: true,
        Compression: MeshCompression.None, MeshoptQuantizationBits: 14, TileMaxDepth: 16,
        TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0);
    public static readonly InterchangePolicy Web = Canonical with { Compression = MeshCompression.Meshopt, MeshoptQuantizationBits: 12 };
}

public enum MeshCompression : byte { None = 0, Draco = 1, Meshopt = 2 }

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Seq<TileNode> Children);

public sealed record TileSet(TileNode Root, double GeometricErrorRoot, int MaxDepth, int NodeCount, Instant At) {
    public static TileSet Build(ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) =>
        new(Partition(geometry, policy, depth: 0), policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(Partition(geometry, policy, 0)), clocks.Now);

    static TileNode Partition(ImportedGeometry geometry, InterchangePolicy policy, int depth) =>
        depth >= policy.MaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? new TileNode(depth, Bounds(geometry), policy.TileGeometricErrorRoot / Math.Pow(2, depth), Octant(geometry, depth), Seq<TileNode>())
            : new TileNode(depth, Bounds(geometry), policy.TileGeometricErrorRoot / Math.Pow(2, depth), Octant(geometry, depth),
                toSeq(Split(geometry)).Map(child => Partition(child, policy, depth + 1)));

    static int Count(TileNode node) => 1 + node.Children.Sum(Count);
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

    public static Fin<Seq<ExportArtifact>> ExportTiles(ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) =>
        Try.lift(() => Tiled(TileSet.Build(geometry, policy, clocks), policy, clocks.Now)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message));

    static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    static byte[] GlbBytes(ImportedGeometry geometry, InterchangePolicy policy) =>
        policy.Compression switch {
            MeshCompression.Draco => DracoEncode(SceneOf(geometry), policy),
            MeshCompression.Meshopt => MeshoptEncode(SceneOf(geometry), policy),
            _ => WriteGlb(SceneOf(geometry), policy),
        };

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
- Boundary: the two-hop rail is the single IFC-to-geometry path because GeometryGym carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry` (`geometry/.planning/ifc-companion.md`), never a NuGet pin, and it is reached only through the existing remote-lane companion rpc so this page mints no transport, no channel, and no second wire vocabulary — the host-local posture means an in-process Rhino host crosses to the companion process over the same UDS/InProcess leg `remote-lane#TRANSPORT_AXIS` owns and a remote tessellation rides that same companion rpc; the GLB the companion returns re-enters the managed import rail so the decoded mesh is the same `ImportedGeometry` shape a native glTF import produces, and the IFC semantic graph (from the `IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the content-key; the companion-daemon protocol detail is the next-loop concern named in RESEARCH, the bridge fence here is transcription-complete on the request shape and the cache-by-content-key reuse.

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

## [6]-[FIELD_RESULT_CODEC]

- Owner: `FieldCodecPolicy` the chunked-layout and error-bound policy record; `FieldArtifact` the chunked simulation-field carrier over CGNS/EnSight/VTK/Zarr; `FieldCodec` the static encode/decode surface projecting a `FieldSpace`-shaped result into a Zarr/VTK-class chunked layout with error-bounded lossy or exact lossless residence and a zero-copy solver↔store↔viz handoff.
- Entry: `public static Fin<FieldArtifact> FieldDecode(InterchangeFormat format, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at)` reads a chunked field artifact into the integration-point/nodal field carrier; `public static Fin<ExportArtifact> FieldEncode(FieldArtifact field, InterchangeFormat format, FieldCodecPolicy policy, Instant at)` emits the chunked layout with the policy error bound; `Fin<T>` aborts on a chunk-shape mismatch or an error bound the lossy quantizer cannot meet.
- Auto: the codec chunks the field by the policy chunk shape so a large solve result streams chunk-by-chunk through the `staging-and-streams#STREAM_POOL` `GetReadOnlySequence` zero-copy read, never a flattened array; the lossy column quantizes each chunk to the policy bit budget and the residual stays below the relative error bound (a chunk whose quantization exceeds the bound falls back to lossless), the lossless column deflates the raw bytes, and the zero-copy handoff wraps the chunk window with `UnsafeByteOperations.UnsafeWrap` so the solver field, the store blob, and the viz upload are one buffer; the chunk index keys each chunk by its grid coordinate so a viewport reads only the chunks its frustum intersects.
- Receipt: the `StreamSegment` receipt carries the field artifact id, the chunk count, and the emitted bytes; a lossy encode stamps the achieved max-residual against the bound on the `Cache` receipt so an error-bounded compression is auditable.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, Microsoft.IO.RecyclableMemoryStream, System.Numerics.Tensors, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new chunked field format is one `InterchangeFormat` row on the `field-chunk` codec; a new error-bound policy is one column on `FieldCodecPolicy`; zero new surface.
- Boundary: the field codec is the result-specific layout the generic blob/snapshot codecs never owned — a scalar/vector/tensor solve field rides the `solver-and-optimization#DISCRETIZATION_MESH` `FieldSpace` shape, so the codec chunks by station and component, never a generic byte blob; the chunked layout composes the suite `XxHash128` chunk identity and the Persistence blob lane content-addressed, so a re-emitted identical chunk dedups and a re-read warms from the store — a second field store is the rejected form; the lossy quantizer's error bound is a typed policy column the receipt records, so an error-bounded compression never silently exceeds its bound; the zero-copy edge is the same `GetReadOnlySequence`/`UnsafeWrap` path the remote frame law owns, so a field chunk crosses solver→store→viz without a managed copy — a `ToArray` flatten on the field path is the named defect.

```csharp signature
public sealed record FieldCodecPolicy(int[] ChunkShape, bool Lossy, int QuantizationBits, double RelativeErrorBound, bool Deflate) {
    public static readonly FieldCodecPolicy Lossless = new(ChunkShape: [64, 64, 64], Lossy: false, QuantizationBits: 0, RelativeErrorBound: 0.0, Deflate: true);
    public static readonly FieldCodecPolicy Bounded = new(ChunkShape: [64, 64, 64], Lossy: true, QuantizationBits: 12, RelativeErrorBound: 1e-3, Deflate: true);
}

public sealed record FieldArtifact(
    InterchangeFormat Format,
    string Station,
    int Rank,
    int Components,
    long Count,
    int[] ChunkShape,
    int ChunkCount,
    ReadOnlyMemory<byte> Chunks,
    double MaxResidual,
    Instant At);

public static class FieldCodec {
    public static Fin<FieldArtifact> FieldDecode(InterchangeFormat format, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) =>
        Try.lift(() => Decode(format, bytes, policy, at)).Run().MapFail(static error => new ComputeFault.ModelRejected(error.Message));

    public static Fin<ExportArtifact> FieldEncode(FieldArtifact field, InterchangeFormat format, FieldCodecPolicy policy, Instant at) {
        var encoded = policy.Lossy ? Quantize(field, policy) : Raw(field, policy);
        return encoded.MaxResidual <= policy.RelativeErrorBound || !policy.Lossy
            ? Fin.Succ(new ExportArtifact(format, Pack(encoded, policy), InterchangeIdentity.Key(format, Pack(encoded, policy), InterchangePolicy.Canonical), encoded.Chunks.LongLength, at))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<field-error-bound:{encoded.MaxResidual:R}>{policy.RelativeErrorBound:R}"));
    }

    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) =>
        new(field.Chunks);
}
```

## [7]-[GEOMETRY_DELTA]

- Owner: `GeometryDeltaKind` `[SmartEnum<string>]` structural-diff target rows; `GeometryDelta` the content-addressed delta record; `DeltaCodec` the static FastCDC-chunked structural-diff surface over meshes, B-reps, point clouds, and NURBS with quantization-aware bounded-lossy chunks, columnar layout, and progressive transmission.
- Cases: `GeometryDeltaKind` rows mesh-vertex · mesh-topology · brep-face · pointcloud-octant · nurbs-control.
- Entry: `public static Fin<GeometryDelta> Diff(GeometryDeltaKind kind, ReadOnlyMemory<byte> baseBytes, ReadOnlyMemory<byte> targetBytes, DeltaPolicy policy)` content-defined-chunks both artifacts and emits the changed-chunk set; `public static Fin<ReadOnlyMemory<byte>> Apply(GeometryDelta delta, ReadOnlyMemory<byte> baseBytes)` reconstructs the target from the base plus the delta; `Fin<T>` aborts on a base-hash mismatch.
- Auto: `Diff` runs FastCDC content-defined chunking (a rolling hash splits each artifact at content boundaries so an inserted vertex shifts only the local chunks, never the whole stream) over the columnar layout the geometry kind declares — mesh vertices in a position column, topology in an index column, B-rep faces by face id, point-cloud points by octant cell, NURBS by control-point grid — then diffs the chunk hash sets and emits the added/removed chunk ids; the quantization-aware column quantizes a vertex/control-point chunk to the policy bit budget so the delta is bounded-lossy within a tolerance, and the progressive column orders the changed chunks coarse-to-fine so a transmission renders a coarse target first and refines; the delta keys on the base and target closure hashes so it round-trips deterministically.
- Receipt: the `Cache` receipt carries the delta content-key, the changed-chunk count, the base byte count, and the delta byte count so a structural diff's compression ratio is auditable; a progressive transmission stamps the coarse-chunk-first ordering count.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, System.Numerics.Tensors, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new diffable geometry kind is one `GeometryDeltaKind` row with its columnar-layout column; a new chunk policy is one column on `DeltaPolicy`; zero new surface.
- Boundary: the geometry delta is the structural diff the blob-level delta never owned — the existing Persistence blob delta diffs opaque bytes, this codec diffs by geometry structure so an edit-resilient mesh/B-rep/point-cloud/NURBS change transmits only the touched chunks, and the diff algebra mirrors the `remote-lane#PROTO_VOCABULARY` `GraphDiff`/`SubtreeFetch` wire shape Compute already owns — Compute owns the structural chunking and the Persistence sync lane owns the closure-graph diff, neither re-deriving the other; FastCDC content-defined chunking is the standard rolling-hash boundary so a local edit shifts local chunks only, and a fixed-block chunker that re-chunks the whole stream on an insert is the rejected form; the quantization-aware bounded-lossy column carries its tolerance so a delta never silently exceeds the geometry tolerance; the changed-chunk set transmits progressively through the `SubtreeFetch` server-stream and the content-key dedups against the Persistence blob lane, never a second delta store; the columnar layout is the geometry-kind column, so a position-only edit never re-transmits the topology column.

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

public readonly record struct DeltaChunk(UInt128 Hash, int Ordinal, int ByteLength, double GeometricError);

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
            chunks = chunks.Add(new DeltaChunk(XxHash128.HashToUInt128(slice), ordinal++, cut, 0.0));
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
}
```

## [8]-[CONTENT_ADDRESSING]

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

## [9]-[RESEARCH]

- [AP242_CODEC]: the ISO 10303 AP203/214/242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the AP242 row routes its geometry evaluation through the same companion two-hop rail GeometryGym IFC uses because managed STEP solid evaluation has no in-proc kernel, so the `step-ap242` codec owns the semantic/topology read and the companion owns tessellation; the row, codec, and frame columns are transcription-complete and the body grounds at the cross-folder Python-companion alignment.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the companion process (the managed C# branch has no native loader); the E57/LAS/LAZ/PTS point-scan decode and the CGNS/EnSight/VTK/Zarr field decode are managed and their decode member spellings confirm against the admitted point-cloud and field-format libraries at the next admission gate — the rows, codecs, capability, and frame columns are transcription-complete, the native-companion legs land at the Python-companion cross-branch touchpoint.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse and the OpenCascade-grade BREP tessellation ride the GeometryGym IFC5 surface for the semantic graph and the companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection and the body grounds against the GeometryGym IFC5 member surface at alignment.
- [MESH_COMPRESSION]: the Draco and Meshopt glTF compression member spellings (`KHR_draco_mesh_compression` / `EXT_meshopt_compression` encode through the SharpGLTF toolkit extension surface) and the 3D-Tiles tileset b3dm/glTF tile content schema confirm against the SharpGLTF extension surface — the `MeshCompression` column, the `TileSet` octree partition, and the quantization-bit policy are transcription-complete and the encode body grounds at the SharpGLTF toolkit catalogue.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the two-hop tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is the next-loop concern owned by `libs/python/geometry` (`geometry/.planning/ifc-companion.md`); the `TessellationRequest` shape and the content-key cache-reuse are transcription-complete, the companion wire detail rides the existing remote-lane companion rpc and lands when the Python branch authors its geometry folder.
- [ARTIFACT_INDEX_ROW]: the `ArtifactIndexRow.Interchange` classification row on the Persistence artifact-blob index that carries the interchange artifact kind beside `EpContext` and `OnnxProfile` — the row exists on the Persistence cache-indexes owner and Compute consumes it as settled vocabulary; the exact kind-enum spelling confirms against the Persistence `ArtifactIndexRow` owner at cross-folder alignment.
