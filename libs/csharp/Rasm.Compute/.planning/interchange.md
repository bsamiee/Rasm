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
- Cases: `InterchangeFormat` rows gltf · glb · ifc · ifc-xml · ifc-json · step-ap203 · step-ap214 · step-ap242 · iges · stl · 3mf · obj · ply · cgns · ensight · vtk · zarr · e57 · las · pts · rvt · nwc · dwg · ifc5, plus the verify-before-admit candidate rows usd · usdz · fbx · dae · hdf5 · netcdf · openvdb each carrying a `CataloguePending` marker naming the package that would admit it; `InterchangeCodec` rows sharp-gltf (SharpGLTF managed glTF 2.0) · geometry-gym (GeometryGym managed IFC/IFC5) · step-iso10303 (ISO 10303 AP203/AP214/AP242 solid-model exchange discriminated by `StepProtocol`) · mesh-text (STL/3MF/OBJ/PLY managed mesh decode) · point-cloud (E57/LAS/LAZ/PTS managed scan ingest) · field-chunk (CGNS/EnSight/VTK/Zarr chunked field layout) · native-companion (Revit/Navisworks/DWG native libraries through the companion process) · usd-stage · fbx-sdk · collada-xml · hdf5-field · netcdf-field · openvdb-volume (candidate codecs each `CataloguePending` until the admitting package decompile lands); `KhrExtension` rows naming the thirteen glTF KHR/EXT extensions the SharpGLTF export rides on the codec extension axis (Draco/Meshopt compression, mesh quantization, Basis texture, punctual lights, and the eight material extensions), each carrying its decompile-verified SharpGLTF schema-type owner and its extension registration key.
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or a wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the two-hop rail and a managed glTF/mesh/point-cloud/field decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change that maps glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical DDG-kernel Z-up right-handed frame so interior code never re-derives a per-importer flip.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, BCL inbox
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, companion column, and `StepProtocol` discriminant; a new managed codec package is one `InterchangeCodec` row; a new glTF KHR/EXT capability is one `KhrExtension` row on the codec extension axis carrying its SharpGLTF schema-type owner and registration key; a not-yet-decompiled format admits as a candidate `InterchangeFormat` row carrying a `CataloguePending` marker naming the package, promoted in place when the package decompile lands; zero new surface.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form mirroring the no-`TensorService` law; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric read-write while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC rows because GeometryGym carries no tessellation kernel (the catalogue boundary fact) — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; the `StepProtocol` column disambiguates the three ISO 10303 application protocols that share the `step-iso10303` codec and the `.step`/`.stp`/`.p21` physical-file extension set — AP203 (config-controlled 3D design, `CanExport=false` because the managed branch reads but never re-authors a config-controlled assembly), AP214 (automotive core, import-and-export), AP242 (managed model-based 3D engineering, the merged successor) — so a STEP file resolves to a single codec row and the protocol is a data column the reader switches on, never three sibling `Ap203Importer`/`Ap214Importer`/`Ap242Importer` types, and `StepProtocol.None` on every non-STEP row keeps the column total; the SharpGLTF KHR/EXT capability set is the `KhrExtension` axis where each glTF extension is one row carrying its decompile-verified schema-type owner (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `TextureKTX2` for KHR_texture_basisu, `PunctualLight` for KHR_lights_punctual) or a `CataloguePending` marker where SharpGLTF exposes the capability through a toolkit encode path rather than a named schema type (KHR_draco_mesh_compression, EXT_meshopt_compression, KHR_mesh_quantization), registered through `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read/write — a per-extension importer/exporter type is the deleted form; the candidate `usd`/`usdz`/`fbx`/`dae`/`hdf5`/`netcdf`/`openvdb` rows carry a `CataloguePending` marker naming the admitting package (`USD.NET`, `AssimpNet`/`Ufbx`, `Collada141`/`AssimpNet`, `HDF.PInvoke`, `Microsoft.Research.Science.Data`/NetCDF native, `OpenVdbSharp`) and no invented member spellings — the candidate row holds the media-type, extension set, and codec slot so the format is enumerable and detectable as `CanImport=false`/`CanExport=false` until the package decompile lands and promotes the row in place, never a phantom codec body; media types are the IANA `model/gltf-binary`, `model/gltf+json`, `application/step`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, plus the candidate `model/vnd.usd`, `model/vnd.usdz+zip`, `application/vnd.autodesk.fbx`, `model/vnd.collada+xml`, `application/x-hdf5`, `application/x-netcdf`, `application/vnd.openvdb` USD/FBX/COLLADA/HDF5/NetCDF/VDB media types traced once for the lane.

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
    public static readonly InterchangeCodec SharpGltf = new("sharp-gltf", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeometryGym = new("geometry-gym", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec StepIso10303 = new("step-iso10303", managed: true, companion: true, cataloguePackage: "<step-ap242-reader-pending>");
    public static readonly InterchangeCodec MeshText = new("mesh-text", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec PointCloud = new("point-cloud", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec FieldChunk = new("field-chunk", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", managed: true, companion: false, cataloguePackage: "USD.NET");
    public static readonly InterchangeCodec FbxSdk = new("fbx-sdk", managed: true, companion: false, cataloguePackage: "Ufbx");
    public static readonly InterchangeCodec ColladaXml = new("collada-xml", managed: true, companion: false, cataloguePackage: "Collada141");
    public static readonly InterchangeCodec Hdf5Field = new("hdf5-field", managed: true, companion: false, cataloguePackage: "HDF.PInvoke");
    public static readonly InterchangeCodec NetcdfField = new("netcdf-field", managed: true, companion: false, cataloguePackage: "Microsoft.Research.Science.Data");
    public static readonly InterchangeCodec OpenVdbVolume = new("openvdb-volume", managed: true, companion: false, cataloguePackage: "OpenVdbSharp");

    public bool Managed { get; }
    public bool Companion { get; }
    public Option<string> CataloguePackage { get; }

    public bool CataloguePending => CataloguePackage.IsSome;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class KhrExtension {
    public static readonly KhrExtension DracoMeshCompression = new("KHR_draco_mesh_compression", KhrSlot.Compression, registrar: Option<Action>.None, cataloguePackage: "<sharpgltf-draco-toolkit-encode-pending>");
    public static readonly KhrExtension MeshoptCompression = new("EXT_meshopt_compression", KhrSlot.Compression, registrar: Option<Action>.None, cataloguePackage: "<sharpgltf-meshopt-toolkit-encode-pending>");
    public static readonly KhrExtension MeshQuantization = new("KHR_mesh_quantization", KhrSlot.Geometry, registrar: Option<Action>.None, cataloguePackage: "<sharpgltf-quantization-toolkit-encode-pending>");
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, TextureKTX2>("KHR_texture_basisu"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, PunctualLight>("KHR_lights_punctual"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsSpecular = new("KHR_materials_specular", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialSpecular>("KHR_materials_specular"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsIor = new("KHR_materials_ior", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialIOR>("KHR_materials_ior"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsIridescence = new("KHR_materials_iridescence", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialIridescence>("KHR_materials_iridescence"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsSheen = new("KHR_materials_sheen", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialSheen>("KHR_materials_sheen"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsClearcoat = new("KHR_materials_clearcoat", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialClearCoat>("KHR_materials_clearcoat"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsTransmission = new("KHR_materials_transmission", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialTransmission>("KHR_materials_transmission"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsVolume = new("KHR_materials_volume", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialVolume>("KHR_materials_volume"), cataloguePackage: Option<string>.None);
    public static readonly KhrExtension MaterialsEmissiveStrength = new("KHR_materials_emissive_strength", KhrSlot.Material, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, MaterialEmissiveStrength>("KHR_materials_emissive_strength"), cataloguePackage: Option<string>.None);

    public KhrSlot Slot { get; }
    public Option<Action> Registrar { get; }
    public Option<string> CataloguePackage { get; }

    public bool CataloguePending => CataloguePackage.IsSome;

    public Fin<KhrExtension> Register() =>
        Registrar.Match(
            Some: register => Try.lift(() => { register(); return this; }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message)),
            None: () => Fin.Fail<KhrExtension>(new ComputeFault.ModelRejected($"<khr-catalogue-pending:{Key}>")));
}

public enum KhrSlot : byte { Compression = 0, Geometry = 1, Texture = 2, Scene = 3, Material = 4 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat StepAp203 = new("step-ap203", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.Ap203);
    public static readonly InterchangeFormat StepAp214 = new("step-ap214", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: true, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.Ap214);
    public static readonly InterchangeFormat StepAp242 = new("step-ap242", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: true, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.Ap242);
    public static readonly InterchangeFormat Iges = new("iges", mediaType: "model/iges", extensions: Seq(".igs", ".iges"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Stl = new("stl", mediaType: "model/stl", extensions: Seq(".stl"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat ThreeMf = new("3mf", mediaType: "model/3mf", extensions: Seq(".3mf"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Obj = new("obj", mediaType: "model/obj", extensions: Seq(".obj"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ply = new("ply", mediaType: "model/ply", extensions: Seq(".ply"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Cgns = new("cgns", mediaType: "application/cgns", extensions: Seq(".cgns"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat EnSight = new("ensight", mediaType: "application/ensight", extensions: Seq(".case", ".encas"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Vtk = new("vtk", mediaType: "application/vtk", extensions: Seq(".vtk", ".vtu", ".vtp"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Zarr = new("zarr", mediaType: "application/zarr", extensions: Seq(".zarr"), canImport: true, canExport: true, codec: InterchangeCodec.FieldChunk, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat E57 = new("e57", mediaType: "application/e57", extensions: Seq(".e57"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Las = new("las", mediaType: "application/vnd.las", extensions: Seq(".las", ".laz"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Pts = new("pts", mediaType: "application/vnd.pts", extensions: Seq(".pts"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), canImport: true, canExport: true, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), canImport: false, canExport: false, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), canImport: false, canExport: false, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Fbx = new("fbx", mediaType: "application/vnd.autodesk.fbx", extensions: Seq(".fbx"), canImport: false, canExport: false, codec: InterchangeCodec.FbxSdk, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Collada = new("dae", mediaType: "model/vnd.collada+xml", extensions: Seq(".dae"), canImport: false, canExport: false, codec: InterchangeCodec.ColladaXml, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Hdf5 = new("hdf5", mediaType: "application/x-hdf5", extensions: Seq(".h5", ".hdf5"), canImport: false, canExport: false, codec: InterchangeCodec.Hdf5Field, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat NetCdf = new("netcdf", mediaType: "application/x-netcdf", extensions: Seq(".nc", ".cdf"), canImport: false, canExport: false, codec: InterchangeCodec.NetcdfField, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat OpenVdb = new("openvdb", mediaType: "application/vnd.openvdb", extensions: Seq(".vdb"), canImport: false, canExport: false, codec: InterchangeCodec.OpenVdbVolume, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);

    private readonly Seq<string> extensions;

    public string MediaType { get; }
    public bool CanImport { get; }
    public bool CanExport { get; }
    public InterchangeCodec Codec { get; }
    public bool TessellationRequiresCompanion { get; }
    public UpAxis UpAxis { get; }
    public Handedness Handedness { get; }
    public StepProtocol StepProtocol { get; }

    public Seq<string> Extensions => extensions;

    public bool CataloguePending => Codec.CataloguePending && !CanImport && !CanExport;

    static readonly FrozenDictionary<string, InterchangeFormat> ByExtension =
        Items.SelectMany(static row => row.extensions.Map(ext => (ext, row)))
            .GroupBy(static pair => pair.ext, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static pair => (int)pair.row.StepProtocol).row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByMediaType =
        Items.GroupBy(static row => row.MediaType, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static row => (int)row.StepProtocol)!, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByKey =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.OrdinalIgnoreCase);

    public static Fin<InterchangeFormat> Detect(string pathOrMediaTypeOrKey) =>
        ByKey.TryGetValue(pathOrMediaTypeOrKey, out var byKey) ? Fin.Succ(byKey)
        : ByMediaType.TryGetValue(pathOrMediaTypeOrKey, out var byType) ? Fin.Succ(byType)
        : ByExtension.TryGetValue(Path.GetExtension(pathOrMediaTypeOrKey), out var byExt) ? Fin.Succ(byExt)
        : Fin.Fail<InterchangeFormat>(new ComputeFault.ModelRejected($"<interchange-format-miss:{pathOrMediaTypeOrKey}>"));
}

public enum UpAxis : byte { X = 0, Y = 1, Z = 2 }

public enum Handedness : byte { Right = 0, Left = 1 }

public enum StepProtocol : byte { None = 0, Ap203 = 203, Ap214 = 214, Ap242 = 242 }

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
}
```

## [3]-[IMPORT_RAIL]

- Owner: `InterchangeIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF (the one decode arm whose package is decompiled, fully authored), the STL/3MF/OBJ/PLY mesh-text and E57/LAS/LAZ/PTS point-scan arms that fault `mesh-text-catalogue-pending`/`point-catalogue-pending` until their reader packages decompile, the chunked CGNS/EnSight/VTK/Zarr field decode through `FieldCodec`, the in-process semantic IFC/IFC5 ingest through GeometryGym (fully authored over `DatabaseIfc`/`Extract<T>`), and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `PointScan` the point-cloud carrier, `FieldArtifact` the chunked simulation-field carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `public static Fin<PointScan> ImportPoints(...)` for the managed scan path; `public static Fin<FieldArtifact> ImportField(...)` for the chunked field path; `public static Fin<IfcSemanticModel> ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `ComputeFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` (the catalogued text-parse member — `ReadGLB` reads GLB binary only, never text glTF) then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project` to `IfcProject`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), and type objects (`IfcTypeObject`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, Rasm (project), Rasm.Persistence (project), BCL inbox
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the `step-iso10303` codec reads the protocol column to select the entity-instance vocabulary version (AP203 config-controlled, AP214 automotive core, AP242 model-based engineering) so a single STEP reader spans all three without a per-protocol codec; zero new surface.
- Boundary: `InterchangeIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple owned by the `staging-and-streams#ALLOCATION_AXIS` `PooledMemory` row at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the PooledMemory ownership point); the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and the catalogue boundary fact holds that GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `TWO_HOP_TESSELLATION` rail and never evaluates a BRep in-process; the `step-iso10303` STEP solid-model path reads the entity-instance graph at the protocol the `StepProtocol` column names (the AP203/AP214/AP242 EXPRESS vocabularies share the STEP physical-file token grammar) and routes its B-rep/NURBS evaluation through the same companion two-hop rail because managed STEP solid evaluation has no in-process kernel — the `step-ap242-reader-pending` catalogue marker names the unadmitted reader so the codec's semantic-read body grounds at the next admission gate while the row, codec, protocol, and frame columns are transcription-complete; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on import so a malformed asset faults at parse rather than mid-decode; a candidate `usd`/`usdz`/`fbx`/`dae`/`hdf5`/`netcdf`/`openvdb` import faults at the boundary with the `import-catalogue-pending` rail naming the admitting package because the format is enumerable and detectable but its codec body is unwritten until the package decompile lands — a phantom decode arm over an undecompiled package is the rejected form; the string-tensor and host-geometry types stay inside the capsule and never enter lane signatures; an `IfcImporter`/`GltfImporter` service family and a managed IFC tessellator are the deleted forms.

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
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("<unknown>")}>"))
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.MeshText ? Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<mesh-text-catalogue-pending:{format.Key}:stl-3mf-obj-ply-reader-unadmitted>"))
        : Fin.Fail<ImportedGeometry>(new ComputeFault.ModelRejected($"<import-needs-companion:{format.Key}>"));

    public static Fin<PointScan> ImportPoints(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec != InterchangeCodec.PointCloud ? Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-codec-miss:{format.Key}>"))
        : Fin.Fail<PointScan>(new ComputeFault.ModelRejected($"<point-catalogue-pending:{format.Key}:e57-las-laz-pts-reader-unadmitted>"));

    public static Fin<FieldArtifact> ImportField(InterchangeFormat format, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.FieldChunk
            ? FieldCodec.FieldDecode(format, bytes, policy, clocks.Now)
            : Fin.Fail<FieldArtifact>(new ComputeFault.ModelRejected($"<field-codec-miss:{format.Key}>"));

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(() => Semantic(Database(format, bytes), clocks.Now))
            : Fin.Fail<IfcSemanticModel>(new ComputeFault.ModelRejected($"<ifc-codec-miss:{format.Key}>"));

    static Fin<T> Boundary<T>(Func<T> decode) =>
        Try.lift(decode).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    static ImportedGeometry Gltf(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Instant at) {
        var settings = new ReadSettings { Validation = ValidationMode.Strict };
        var model = format == InterchangeFormat.Glb
            ? ModelRoot.ParseGLB(new ArraySegment<byte>(bytes.ToArray()), settings)
            : new ReadContext().ReadTextSchema2(new MemoryStream(bytes.ToArray()));
        return Decoded(format, model.LogicalMeshes.Decode(), at);
    }

    static ImportedGeometry Decoded(InterchangeFormat format, IReadOnlyList<IMeshDecoder<Material>> meshes, Instant at) {
        var triangles = meshes
            .SelectMany(static mesh => mesh.Primitives)
            .SelectMany(static prim => prim.TriangleIndices.Map(tri => (prim, tri)))
            .ToSeq();
        int vertexCount = triangles.Count * 3;
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        var indices = new long[vertexCount];
        int slot = 0;
        foreach (var (prim, (a, b, c)) in triangles) {
            foreach (int corner in stackalloc[] { a, b, c }) {
                var p = prim.GetPosition(corner);
                var n = prim.GetNormal(corner);
                int v = slot * 3;
                (vertices[v], vertices[v + 1], vertices[v + 2]) = (p.X, p.Y, p.Z);
                (normals[v], normals[v + 1], normals[v + 2]) = (n.X, n.Y, n.Z);
                indices[slot] = slot;
                slot++;
            }
        }
        return new ImportedGeometry(format, vertices, normals, indices, vertexCount, triangles.Count, at);
    }

    static ImportedGeometry Framed(InterchangeFormat format, ImportedGeometry geometry) {
        if (format.UpAxis == UpAxis.Z && format.Handedness == Handedness.Right) {
            return geometry;
        }
        var vertices = MemoryMarshal.AsMemory(geometry.Vertices);
        FrameNormalization.Canonicalize(format, vertices.Span, stride: 3);
        return geometry with { Vertices = vertices };
    }

    static IfcSemanticModel Semantic(DatabaseIfc db, Instant at) {
        var project = db.Project;
        return new IfcSemanticModel(
            db.Release, db.ModelView,
            project.Extract<IfcSpatialStructureElement>().AsIterable()
                .Map(static s => new IfcSemanticModel.SpatialNode(s.GlobalId, s.GetType().Name, s.Name ?? "", s.LongName ?? "",
                    s.Extract<IfcProduct>().AsIterable().Map(static p => p.GlobalId).ToSeq())).ToSeq(),
            project.Extract<IfcProduct>().AsIterable()
                .Map(static p => new IfcSemanticModel.ProductRow(p.GlobalId, p.GetType().Name, p.Name ?? "", (p as IfcElement)?.Tag ?? "",
                    Optional((p as IfcObject)?.IsTypedBy.FirstOrDefault()?.RelatingType?.GlobalId))).ToSeq(),
            project.Extract<IfcPropertySet>().AsIterable()
                .SelectMany(static ps => ps.HasProperties.Values.OfType<IfcPropertySingleValue>()
                    .Select(pv => new IfcSemanticModel.PropertyRow(ps.GlobalId, ps.Name ?? "", pv.Name ?? "", pv.NominalValue?.ValueString ?? ""))).ToSeq(),
            project.Extract<IfcElementQuantity>().AsIterable()
                .SelectMany(static eq => eq.Quantities.Values.OfType<IfcPhysicalSimpleQuantity>()
                    .Select(q => new IfcSemanticModel.QuantityRow(eq.GlobalId, eq.Name ?? "", q.Name ?? "", q.SimpleValue, q.Unit?.ToString() ?? ""))).ToSeq(),
            project.Extract<IfcRelAssociatesMaterial>().AsIterable()
                .Map(static r => new IfcSemanticModel.MaterialRow(r.GlobalId, (r.RelatingMaterial as IfcMaterial)?.Name ?? "")).ToSeq(),
            project.Extract<IfcTypeObject>().AsIterable()
                .Map(static t => new IfcSemanticModel.TypeRow(t.GlobalId, t.GetType().Name, t.Name ?? "")).ToSeq(),
            db.Tolerance, at);
    }

    static DatabaseIfc Database(InterchangeFormat format, ReadOnlyMemory<byte> bytes) =>
        format == InterchangeFormat.IfcJson ? JsonDatabase(bytes)
        : format == InterchangeFormat.IfcXml ? XmlDatabase(bytes)
        : DatabaseIfc.ParseString(Encoding.UTF8.GetString(bytes.Span));

    static DatabaseIfc JsonDatabase(ReadOnlyMemory<byte> bytes) {
        var db = new DatabaseIfc(false, ReleaseVersion.IFC4X3_ADD2);
        db.ReadJSON((JsonObject)JsonNode.Parse(bytes.Span)!);
        return db;
    }

    static DatabaseIfc XmlDatabase(ReadOnlyMemory<byte> bytes) {
        var doc = new XmlDocument();
        doc.LoadXml(Encoding.UTF8.GetString(bytes.Span));
        var db = new DatabaseIfc(false, ReleaseVersion.IFC4X3_ADD2);
        db.ReadXMLDoc(doc);
        return db;
    }
}
```

## [4]-[EXPORT_RAIL]

- Owner: `InterchangeIo` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization, all fully managed with no companion; `ExportArtifact` the emitted-bytes carrier feeding the content-addressing cluster.
- Entry: `public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss or a codec fault projected onto `ComputeFault.ModelRejected`.
- Auto: GLB export assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
- Receipt: the `StreamSegment` receipt carries the format key, codec key, emitted byte count, and the content-key the addressing cluster computes; emission rides the sink port.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new managed export is one codec arm on the export fold; a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; zero new surface.
- Boundary: the export fold extends the `InterchangeIo` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over the generic `ExtensionsFactory.RegisterExtension<ModelRoot, TExt>(name)` (the only catalogued overload — no runtime-`Type`-argument form exists) before any write so a `KHR_materials_clearcoat`/`KHR_materials_transmission`/`KHR_materials_volume`/`KHR_materials_specular`/`KHR_materials_ior`/`KHR_materials_iridescence`/`KHR_materials_sheen`/`KHR_materials_emissive_strength` material channel and a `KHR_lights_punctual` light and a `KHR_texture_basisu` KTX2 texture serialize through their decompile-verified SharpGLTF schema types (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `PunctualLight`, `TextureKTX2`) rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression`/`EXT_meshopt_compression`/`KHR_mesh_quantization` rows carry no named schema type because SharpGLTF exposes compression through a toolkit encode path the `MeshCompression` column selects, so their `Register` faults with the `khr-catalogue-pending` rail and the encode body grounds at the SharpGLTF toolkit catalogue — a phantom `DracoMeshCompression` schema-type reference is the rejected form; IFC serialization selects `FormatIfcSerialization.STEP`/`XML`/`JSON` by the row and `DatabaseIfc.WriteStream`/`ToString` are the only emit members — a hand-rolled STEP writer is the deleted form; the model graph re-authoring runs through `FactoryIfc` canonical axes, origins, and owner history so a round-tripped model carries stable GlobalIds through `ParserIfc.HashGlobalID` keyed on a stable entity key rather than a fresh GUID per export, making export idempotent under the content-key; a write to a row whose `CanExport` is false faults at the boundary; the emitted bytes never copy into a managed array beyond the one write window the `staging-and-streams#STREAM_POOL` contiguous route bounds.

```csharp signature
public sealed record InterchangePolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    ReleaseVersion IfcSchema,
    StepProtocol StepProtocol,
    bool MergeBuffers,
    bool StridedBuffers,
    MeshCompression Compression,
    int MeshoptQuantizationBits,
    Seq<KhrExtension> Extensions,
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Compression: MeshCompression.None, MeshoptQuantizationBits: 14, Extensions: Seq<KhrExtension>(), TileMaxDepth: 16,
        TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0);
    public static readonly InterchangePolicy Web = Canonical with {
        Compression = MeshCompression.Meshopt, MeshoptQuantizationBits = 12,
        Extensions = Seq(KhrExtension.MaterialsSpecular, KhrExtension.MaterialsIor, KhrExtension.MaterialsEmissiveStrength, KhrExtension.LightsPunctual, KhrExtension.TextureBasisu),
    };
    public static readonly InterchangePolicy Pbr = Canonical with {
        Extensions = Seq(KhrExtension.MaterialsClearcoat, KhrExtension.MaterialsTransmission, KhrExtension.MaterialsVolume,
            KhrExtension.MaterialsSheen, KhrExtension.MaterialsIridescence, KhrExtension.MaterialsSpecular, KhrExtension.MaterialsIor, KhrExtension.MaterialsEmissiveStrength),
    };
}

public enum MeshCompression : byte { None = 0, Draco = 1, Meshopt = 2 }

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Seq<TileNode> Children);

public sealed record TileSet(TileNode Root, double GeometricErrorRoot, int MaxDepth, int NodeCount, Instant At) {
    public static TileSet Build(ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) {
        var root = Partition(geometry, policy, depth: 0);
        return new TileSet(root, policy.TileGeometricErrorRoot, policy.TileMaxDepth, Count(root), clocks.Now);
    }

    static TileNode Partition(ImportedGeometry geometry, InterchangePolicy policy, int depth) {
        var bounds = Bounds(geometry);
        double error = policy.TileGeometricErrorRoot / Math.Pow(2, depth);
        var contentKey = InterchangeIdentity.Key(geometry.Format, MemoryMarshal.AsBytes(geometry.Vertices.Span), policy);
        return depth >= policy.TileMaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? new TileNode(depth, bounds, error, contentKey, Seq<TileNode>())
            : new TileNode(depth, bounds, error, contentKey,
                Split(geometry, bounds).Map(child => Partition(child, policy, depth + 1)));
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
            ? GlbBytes(geometry, policy).Map(bytes => Sealed(format, bytes, policy, clocks.Now))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<export-codec-miss:{format.Key}>"));

    public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Try.lift(() => Sealed(format, IfcBytes(format, model, policy), policy, clocks.Now)).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<ifc-export-codec-miss:{format.Key}>"));

    public static Fin<Seq<ExportArtifact>> ExportTiles(ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks) =>
        Tiled(TileSet.Build(geometry, policy, clocks), policy, clocks.Now).Traverse(static result => result);

    static FormatIfcSerialization SerializationOf(InterchangeFormat format) =>
        format == InterchangeFormat.IfcXml ? FormatIfcSerialization.XML
        : format == InterchangeFormat.IfcJson ? FormatIfcSerialization.JSON
        : FormatIfcSerialization.STEP;

    static Fin<byte[]> GlbBytes(ImportedGeometry geometry, InterchangePolicy policy) =>
        RegisterExtensions(policy).Bind(_ => policy.Compression switch {
            MeshCompression.Draco => Fin.Fail<byte[]>(new ComputeFault.ModelRejected("<draco-encode-catalogue-pending:sharpgltf-draco-toolkit-encode-unadmitted>")),
            MeshCompression.Meshopt => Fin.Fail<byte[]>(new ComputeFault.ModelRejected("<meshopt-encode-catalogue-pending:sharpgltf-meshopt-toolkit-encode-unadmitted>")),
            _ => Fin.Succ(WriteGlb(SceneOf(geometry), policy)),
        });

    static ModelRoot SceneOf(ImportedGeometry geometry) {
        var material = new MaterialBuilder("default").WithMetallicRoughnessShader();
        var mesh = new MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>(geometry.Format.Key);
        var primitive = mesh.UsePrimitive(material);
        var verts = geometry.Vertices.Span;
        var normals = geometry.Normals.Span;
        var indices = geometry.Indices.Span;
        for (int tri = 0; tri < geometry.TriangleCount; tri++) {
            var corners = (
                Vertex(verts, normals, (int)indices[tri * 3]),
                Vertex(verts, normals, (int)indices[tri * 3 + 1]),
                Vertex(verts, normals, (int)indices[tri * 3 + 2]));
            primitive.AddTriangle(corners.Item1, corners.Item2, corners.Item3);
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

    static byte[] IfcBytes(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy) {
        var db = new DatabaseIfc(true, policy.IfcSchema);
        var site = db.Project.UppermostSite();
        var storeys = toMap(model.Spatial
            .Map(node => (node.GlobalId, storey: new IfcBuildingStorey(site, node.Name) { GlobalId = ParserIfc.HashGlobalID(node.GlobalId), LongName = node.LongName })));
        model.Products.Iter(row => {
            var host = row.TypeGlobalId.Bind(id => storeys.Find(id).Map(static s => (IfcSpatialElement)s)).IfNone(() => site);
            var element = new IfcBuildingElementProxy(host, null, null) { GlobalId = ParserIfc.HashGlobalID(row.GlobalId), Name = row.Name, Tag = row.Tag };
            model.Materials.Filter(m => m.OwnerGlobalId == row.GlobalId).Iter(m => element.SetMaterial(new IfcMaterial(db, m.MaterialName)));
        });
        return Encoding.UTF8.GetBytes(db.ToString(SerializationOf(format)));
    }

    static Seq<Fin<ExportArtifact>> Tiled(TileSet tiles, InterchangePolicy policy, Instant at) =>
        Flatten(tiles.Root)
            .Filter(static node => node.Children.IsEmpty)
            .Map(static node => Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<tile-content-catalogue-pending:{node.ContentKey:x32}:b3dm-glb-tile-emit-unadmitted>")));

    static Seq<TileNode> Flatten(TileNode node) =>
        node.Cons(node.Children.Bind(Flatten));

    static readonly FrozenDictionary<MeshCompression, KhrExtension> CompressionExtension =
        new Dictionary<MeshCompression, KhrExtension> {
            [MeshCompression.Draco] = KhrExtension.DracoMeshCompression,
            [MeshCompression.Meshopt] = KhrExtension.MeshoptCompression,
        }.ToFrozenDictionary();

    static Fin<Unit> RegisterExtensions(InterchangePolicy policy) =>
        policy.Extensions
            .Append(CompressionExtension.TryGetValue(policy.Compression, out var compression) ? Seq1(compression) : Seq<KhrExtension>())
            .Filter(static khr => khr.Registrar.IsSome)
            .Map(static khr => khr.Register())
            .Traverse(static result => result)
            .Map(static _ => unit);

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
        Try.lift(() => Decode(format, bytes, policy, at)).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    public static Fin<ExportArtifact> FieldEncode(FieldArtifact field, InterchangeFormat format, FieldCodecPolicy policy, Instant at) {
        var encoded = policy.Lossy ? Quantize(field, policy) : Raw(field, policy);
        var packed = Pack(encoded, policy);
        return encoded.MaxResidual <= policy.RelativeErrorBound || !policy.Lossy
            ? Fin.Succ(new ExportArtifact(format, packed, InterchangeIdentity.Key(format, packed, InterchangePolicy.Canonical), packed.LongLength, at))
            : Fin.Fail<ExportArtifact>(new ComputeFault.ModelRejected($"<field-error-bound:{encoded.MaxResidual:R}>{policy.RelativeErrorBound:R}"));
    }

    public static ReadOnlySequence<byte> ChunkSequence(FieldArtifact field) =>
        new(field.Chunks);

    static FieldArtifact Decode(InterchangeFormat format, ReadOnlyMemory<byte> bytes, FieldCodecPolicy policy, Instant at) {
        var span = bytes.Span;
        var (station, rank, components, count) = (Encoding.ASCII.GetString(span[..16]).TrimEnd('\0'),
            BinaryPrimitives.ReadInt32LittleEndian(span[16..]), BinaryPrimitives.ReadInt32LittleEndian(span[20..]),
            BinaryPrimitives.ReadInt64LittleEndian(span[24..]));
        var payload = policy.Deflate ? Inflate(bytes[32..]) : bytes[32..];
        int chunkBytes = policy.ChunkShape.Aggregate(1, static (acc, dim) => acc * dim) * components * sizeof(float);
        int chunkCount = (payload.Length + chunkBytes - 1) / Math.Max(chunkBytes, 1);
        return new FieldArtifact(format, station, rank, components, count, policy.ChunkShape, chunkCount, payload, 0.0, at);
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

- [AP242_CODEC]: the ISO 10303 AP203/AP214/AP242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the three protocols ride one `step-iso10303` codec discriminated by the `StepProtocol` column (`Ap203`=203 config-controlled read-only, `Ap214`=214 automotive read-write, `Ap242`=242 model-based engineering read-write, `None`=0 on every non-STEP row), all routing geometry evaluation through the same companion two-hop rail GeometryGym IFC uses because managed STEP solid evaluation has no in-proc kernel, so the codec owns the semantic/topology read and the companion owns tessellation; the `.step`/`.stp`/`.p21` extension and `application/step` media type resolve to AP242 by `StepProtocol` rank with AP203/AP214 reachable through their explicit row keys, the `step-ap242-reader-pending` `CataloguePackage` marker names the unadmitted managed STEP reader, and the row, codec, protocol, and frame columns are transcription-complete with the body grounding at the cross-folder Python-companion alignment.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the companion process (the managed C# branch has no native loader); the E57/LAS/LAZ/PTS point-scan and STL/3MF/OBJ/PLY mesh-text decode are managed-in-intent but the reader packages are undecompiled, so `ImportPoints` and the `mesh-text` import arm fault `point-catalogue-pending`/`mesh-text-catalogue-pending` and their decode member spellings ground against the admitted point-cloud and mesh-text libraries at the next admission gate; only the rows, codecs, capability, and frame columns are transcription-complete here — the decode bodies are explicitly undone until the package decompile lands, and the native-companion legs land at the Python-companion cross-branch touchpoint.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse and the OpenCascade-grade BREP tessellation ride the GeometryGym IFC5 surface for the semantic graph and the companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection and the body grounds against the GeometryGym IFC5 member surface at alignment.
- [MESH_COMPRESSION]: the thirteen glTF KHR/EXT extension rows ride the `KhrExtension` `[SmartEnum<string>]` axis on the codec extension column, each carrying its decompile-verified SharpGLTF schema-type owner where SharpGLTF exposes a named Schema2 type — `KHR_materials_clearcoat`→`MaterialClearCoat`, `KHR_materials_transmission`→`MaterialTransmission`, `KHR_materials_volume`→`MaterialVolume`, `KHR_materials_specular`→`MaterialSpecular`, `KHR_materials_ior`→`MaterialIOR`, `KHR_materials_iridescence`→`MaterialIridescence`, `KHR_materials_sheen`→`MaterialSheen`, `KHR_materials_emissive_strength`→`MaterialEmissiveStrength`, `KHR_texture_basisu`→`TextureKTX2`, `KHR_lights_punctual`→`PunctualLight` — each registered through its `Registrar` closure over the generic `ExtensionsFactory.RegisterExtension<ModelRoot, TExt>(name)` (the sole catalogued overload; the `(Type, Type, string)` form is a phantom) before write; the `KHR_draco_mesh_compression`/`EXT_meshopt_compression`/`KHR_mesh_quantization` rows carry `Registrar=None` and a `CataloguePackage` marker because SharpGLTF encodes compression through a toolkit path with no named schema type, so the `MeshCompression` column selects the encode and the row's `Register` faults `khr-catalogue-pending` until the toolkit encode member spelling confirms; the 3D-Tiles tileset b3dm/glTF tile content schema and the `TileSet` octree partition and quantization-bit policy are transcription-complete and the Draco/Meshopt encode body grounds at the SharpGLTF toolkit catalogue.
- [CANDIDATE_FORMATS]: the verify-before-admit `usd`/`usdz`/`fbx`/`dae`/`hdf5`/`netcdf`/`openvdb` rows admit as candidate `InterchangeFormat` rows carrying media-type, extension set, frame columns, and a `CataloguePending` codec slot naming the admitting package (`USD.NET` for USD stage I/O, `Ufbx` for FBX, `Collada141` for COLLADA `.dae`, `HDF.PInvoke` for HDF5, `Microsoft.Research.Science.Data` for NetCDF, `OpenVdbSharp` for OpenVDB volumes) with `CanImport=false`/`CanExport=false` so the format is enumerable and `Detect`-able while a managed import/export faults `import-catalogue-pending`; each candidate codec (`usd-stage`, `fbx-sdk`, `collada-xml`, `hdf5-field`, `netcdf-field`, `openvdb-volume`) promotes in place — capability columns flip and the codec body grounds — when the named package decompile lands at the next admission gate, never an invented member spelling before the catalogue confirms.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the two-hop tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is the next-loop concern owned by `libs/python/geometry` (`geometry/.planning/ifc-companion.md`); the `TessellationRequest` shape and the content-key cache-reuse are transcription-complete, the companion wire detail rides the existing remote-lane companion rpc and lands when the Python branch authors its geometry folder.
- [ARTIFACT_INDEX_ROW]: the `ArtifactIndexRow.Interchange` classification row on the Persistence artifact-blob index that carries the interchange artifact kind beside `EpContext` and `OnnxProfile` — the row exists on the Persistence cache-indexes owner and Compute consumes it as settled vocabulary; the exact kind-enum spelling confirms against the Persistence `ArtifactIndexRow` owner at cross-folder alignment.
