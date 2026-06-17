# [BIM_INTERCHANGE]

Rasm.Bim universal exchange semantics: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to BIM semantic graph or geometry) from export (artifact to foreign bytes) across the managed glTF/GLB read-write over SharpGLTF, the managed STL/3MF/OBJ/PLY mesh decode, the managed IFC/IFC5 STEP/XML/JSON read-write over GeometryGym with in-process semantic ingest, the ISO 10303 AP242 and native Revit/Navisworks/DWG CAD-STEP semantics routed through the companion two-hop rail, the per-importer frame/handedness/up-axis reconciliation onto the canonical kernel frame at ingest, and the `IfcSemanticModel` graph projection. The page owns the `InterchangeFormat`/`InterchangeCodec` axes with their capability, companion, and frame columns, the `FrameNormalization` ingest-frame surface, the `BimIo` import/export fold, the `IfcSemanticModel` graph projection, and the `TessellationRequest` companion bridge — composing the `Rasm` kernel geometry, the `csharp:Compute/interchange#CONTENT_ADDRESSING` `InterchangeIdentity` content key, the `csharp:Compute/interchange#TWO_HOP_TESSELLATION` companion tessellation rail, and the `csharp:Compute/remote-lane#TRANSPORT_AXIS` companion transport as settled vocabulary; the page is HOST-LOCAL and carries no TS_PROJECTION.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                                       |
| :-----: | :------------------- | :--------------------------------------------------------------------------- |
|   [1]   | FORMAT_AXIS          | Format/codec rows; capability, companion, frame-normalization columns        |
|   [2]   | IMPORT_RAIL          | Foreign-bytes ingest: mesh, in-proc semantic IFC/IFC5/STEP graph             |
|   [3]   | EXPORT_RAIL          | Artifact emit: GLB mesh-and-scene, IFC STEP/XML/JSON model serialization     |
|   [4]   | TESSELLATION_BRIDGE  | IFC/AP242/native geometry crosses to the Compute companion rail, never in-proc |

## [2]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` interchange-format rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, `TessellationRequiresCompanion`, and the `UpAxis`/`Handedness` ingest-frame columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package or companion that reads and writes the row; `UpAxis`/`Handedness` the per-importer local-frame enums; `FrameNormalization` the static reconciliation surface coercing every imported coordinate into the canonical kernel frame.
- Cases: `InterchangeFormat` rows gltf · glb · ifc · ifc-xml · ifc-json · step-ap203 · step-ap214 · step-ap242 · iges · stl · 3mf · obj · ply · rvt · nwc · dwg · ifc5, plus the verify-before-admit candidate rows usd · usdz · fbx · dae each carrying a `CataloguePending` marker naming the admitting package; `InterchangeCodec` rows sharp-gltf (SharpGLTF managed glTF 2.0) · geometry-gym (GeometryGym managed IFC/IFC5) · step-iso10303 (ISO 10303 AP203/AP214/AP242 solid-model exchange discriminated by `StepProtocol`) · mesh-text (STL/3MF/OBJ/PLY managed mesh decode) · native-companion (Revit/Navisworks/DWG native libraries through the companion process) · usd-stage · fbx-sdk · collada-xml (candidate codecs each `CataloguePending` until the admitting package decompile lands); `KhrExtension` rows naming the thirteen glTF KHR/EXT extensions the SharpGLTF export rides on the codec extension axis (Draco/Meshopt compression, mesh quantization, Basis texture, punctual lights, and the eight material extensions), each carrying its decompile-verified SharpGLTF schema-type owner and its extension registration key.
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or a wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the companion bridge and a managed glTF/mesh decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change that maps glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical kernel Z-up right-handed frame so interior code never re-derives a per-importer flip.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project), BCL inbox
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, companion column, and `StepProtocol` discriminant; a new managed codec package is one `InterchangeCodec` row; a new glTF KHR/EXT capability is one `KhrExtension` row on the codec extension axis carrying its SharpGLTF schema-type owner and registration key; a not-yet-decompiled format admits as a candidate `InterchangeFormat` row carrying a `CataloguePending` marker naming the package, promoted in place when the package decompile lands; zero new surface.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric read-write while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC rows because GeometryGym carries no tessellation kernel (the catalogue boundary fact) — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; the `StepProtocol` column disambiguates the three ISO 10303 application protocols that share the `step-iso10303` codec and the `.step`/`.stp`/`.p21` physical-file extension set — AP203 (config-controlled 3D design, `CanExport=false` because the managed branch reads but never re-authors a config-controlled assembly), AP214 (automotive core, import-and-export), AP242 (managed model-based 3D engineering, the merged successor) — so a STEP file resolves to a single codec row and the protocol is a data column the reader switches on, never three sibling `Ap203Importer`/`Ap214Importer`/`Ap242Importer` types, and `StepProtocol.None` on every non-STEP row keeps the column total; the SharpGLTF KHR/EXT capability set is the `KhrExtension` axis where each glTF extension is one row carrying its decompile-verified schema-type owner (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `TextureKTX2` for KHR_texture_basisu, `PunctualLight` for KHR_lights_punctual) or a `CataloguePending` marker where SharpGLTF exposes the capability through a toolkit encode path rather than a named schema type (KHR_draco_mesh_compression, EXT_meshopt_compression, KHR_mesh_quantization), registered through `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read/write — a per-extension importer/exporter type is the deleted form; the candidate `usd`/`usdz`/`fbx`/`dae` rows carry a `CataloguePending` marker naming the admitting package (`USD.NET`, `Ufbx`, `Collada141`) and no invented member spellings — the candidate row holds the media-type, extension set, and codec slot so the format is enumerable and detectable as `CanImport=false`/`CanExport=false` until the package decompile lands and promotes the row in place, never a phantom codec body; media types are the IANA `model/gltf-binary`, `model/gltf+json`, `application/step`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, plus the candidate `model/vnd.usd`, `model/vnd.usdz+zip`, `application/vnd.autodesk.fbx`, `model/vnd.collada+xml` USD/FBX/COLLADA media types traced once for the lane; the chunked simulation-field codec, the FastCDC structural geometry-delta codec, and the content-addressed artifact identity stay at `csharp:Compute/interchange` and are consumed at the seam, never re-minted here.

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
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", managed: true, companion: false, cataloguePackage: "USD.NET");
    public static readonly InterchangeCodec FbxSdk = new("fbx-sdk", managed: true, companion: false, cataloguePackage: "Ufbx");
    public static readonly InterchangeCodec ColladaXml = new("collada-xml", managed: true, companion: false, cataloguePackage: "Collada141");

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
            Some: register => Try.lift(() => { register(); return this; }).Run().MapFail(static error => (Error)new BimFault.ModelRejected(error.Message)),
            None: () => Fin.Fail<KhrExtension>(new BimFault.ModelRejected($"<khr-catalogue-pending:{Key}>")));
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
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), canImport: true, canExport: true, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), canImport: false, canExport: false, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), canImport: false, canExport: false, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Fbx = new("fbx", mediaType: "application/vnd.autodesk.fbx", extensions: Seq(".fbx"), canImport: false, canExport: false, codec: InterchangeCodec.FbxSdk, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Collada = new("dae", mediaType: "model/vnd.collada+xml", extensions: Seq(".dae"), canImport: false, canExport: false, codec: InterchangeCodec.ColladaXml, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);

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
        : Fin.Fail<InterchangeFormat>(new BimFault.ModelRejected($"<interchange-format-miss:{pathOrMediaTypeOrKey}>"));
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

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF (the one decode arm whose package is decompiled, fully authored), the STL/3MF/OBJ/PLY mesh-text arm that faults `mesh-text-catalogue-pending` until its reader package decompiles, the in-process semantic IFC/IFC5 ingest through GeometryGym (fully authored over `DatabaseIfc`/`Extract<T>`), and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `public static Fin<IfcSemanticModel> ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `BimFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` (the catalogued text-parse member — `ReadGLB` reads GLB binary only, never text glTF) then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project` to `IfcProject`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), and type objects (`IfcTypeObject`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, Rasm (project), BCL inbox
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the `step-iso10303` codec reads the protocol column to select the entity-instance vocabulary version (AP203 config-controlled, AP214 automotive core, AP242 model-based engineering) so a single STEP reader spans all three without a per-protocol codec; zero new surface.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and the catalogue boundary fact holds that GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP solid-model path reads the entity-instance graph at the protocol the `StepProtocol` column names (the AP203/AP214/AP242 EXPRESS vocabularies share the STEP physical-file token grammar) and routes its B-rep/NURBS evaluation through the same companion rail because managed STEP solid evaluation has no in-process kernel — the `step-ap242-reader-pending` catalogue marker names the unadmitted reader so the codec's semantic-read body grounds at the next admission gate while the row, codec, protocol, and frame columns are transcription-complete; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on import so a malformed asset faults at parse rather than mid-decode; a candidate `usd`/`usdz`/`fbx`/`dae` import faults at the boundary with the `import-catalogue-pending` rail naming the admitting package because the format is enumerable and detectable but its codec body is unwritten until the package decompile lands — a phantom decode arm over an undecompiled package is the rejected form; an `IfcImporter`/`GltfImporter` service family and a managed IFC tessellator are the deleted forms.

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

public static class BimIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-unsupported:{format.Key}>"))
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("<unknown>")}>"))
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
        : format.Codec == InterchangeCodec.MeshText ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<mesh-text-catalogue-pending:{format.Key}:stl-3mf-obj-ply-reader-unadmitted>"))
        : Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-needs-companion:{format.Key}>"));

    public static Fin<IfcSemanticModel> ImportIfc(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        format.Codec == InterchangeCodec.GeometryGym
            ? Boundary(() => Semantic(Database(format, bytes), clocks.Now))
            : Fin.Fail<IfcSemanticModel>(new BimFault.ModelRejected($"<ifc-codec-miss:{format.Key}>"));

    static Fin<T> Boundary<T>(Func<T> decode) =>
        Try.lift(decode).Run().MapFail(static error => (Error)new BimFault.ModelRejected(error.Message));

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

- Owner: `BimIo` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization, all fully managed with no companion; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `public static Fin<ExportArtifact> Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `public static Fin<ExportArtifact> ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss or a codec fault projected onto `BimFault.ModelRejected`.
- Auto: GLB export assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, and writes through `ModelRoot.WriteGLB` to bytes; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
- Receipt: the `StreamSegment` receipt carries the format key, codec key, emitted byte count, and the content-key the Compute addressing seam computes; emission rides the sink port.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new managed export is one codec arm on the export fold; a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; zero new surface.
- Boundary: the export fold extends the `BimIo` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the Compute content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over the generic `ExtensionsFactory.RegisterExtension<ModelRoot, TExt>(name)` (the only catalogued overload — no runtime-`Type`-argument form exists) before any write so a `KHR_materials_clearcoat`/`KHR_materials_transmission`/`KHR_materials_volume`/`KHR_materials_specular`/`KHR_materials_ior`/`KHR_materials_iridescence`/`KHR_materials_sheen`/`KHR_materials_emissive_strength` material channel and a `KHR_lights_punctual` light and a `KHR_texture_basisu` KTX2 texture serialize through their decompile-verified SharpGLTF schema types (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `PunctualLight`, `TextureKTX2`) rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression`/`EXT_meshopt_compression`/`KHR_mesh_quantization` rows carry no named schema type because SharpGLTF exposes compression through a toolkit encode path the `MeshCompression` column selects, so their `Register` faults with the `khr-catalogue-pending` rail and the encode body grounds at the SharpGLTF toolkit catalogue — a phantom `DracoMeshCompression` schema-type reference is the rejected form; IFC serialization selects `FormatIfcSerialization.STEP`/`XML`/`JSON` by the row and `DatabaseIfc.WriteStream`/`ToString` are the only emit members — a hand-rolled STEP writer is the deleted form; the model graph re-authoring runs through `FactoryIfc` canonical axes, origins, and owner history so a round-tripped model carries stable GlobalIds through `ParserIfc.HashGlobalID` keyed on a stable entity key rather than a fresh GUID per export, making export idempotent under the Compute content-key; a write to a row whose `CanExport` is false faults at the boundary; the 3D-Tiles tiling and Draco/Meshopt encode bodies ground at the SharpGLTF toolkit catalogue named in RESEARCH, and the chunked-field and structural-delta codecs stay at `csharp:Compute/interchange` consumed at the seam.

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
    Seq<KhrExtension> Extensions) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Compression: MeshCompression.None, MeshoptQuantizationBits: 14, Extensions: Seq<KhrExtension>());
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
        RegisterExtensions(policy).Bind(_ => policy.Compression switch {
            MeshCompression.Draco => Fin.Fail<byte[]>(new BimFault.ModelRejected("<draco-encode-catalogue-pending:sharpgltf-draco-toolkit-encode-unadmitted>")),
            MeshCompression.Meshopt => Fin.Fail<byte[]>(new BimFault.ModelRejected("<meshopt-encode-catalogue-pending:sharpgltf-meshopt-toolkit-encode-unadmitted>")),
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

    static Fin<Unit> RegisterExtensions(InterchangePolicy policy) =>
        policy.Extensions
            .Append(CompressionExtension.TryGetValue(policy.Compression, out var compression) ? Seq1(compression) : Seq<KhrExtension>())
            .Filter(static khr => khr.Registrar.IsSome)
            .Map(static khr => khr.Register())
            .Traverse(static result => result)
            .Map(static _ => unit);

    static readonly FrozenDictionary<MeshCompression, KhrExtension> CompressionExtension =
        new Dictionary<MeshCompression, KhrExtension> {
            [MeshCompression.Draco] = KhrExtension.DracoMeshCompression,
            [MeshCompression.Meshopt] = KhrExtension.MeshoptCompression,
        }.ToFrozenDictionary();

    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes, InterchangeIdentity.Key(format.Key, bytes, policy.Deflection, policy.Tolerance, policy.AngleTolerance), bytes.LongLength, at);
}
```

## [5]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` — the Bim-side request shape that crosses IFC/AP242/native geometry evaluation to the `csharp:Compute/interchange#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-imports the GLB through the `IMPORT_RAIL` glTF path; the request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself.
- Entry: `public static Fin<TessellationRequest> Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; Compute issues the request over `csharp:Compute/remote-lane#TRANSPORT_AXIS` and the GLB result re-enters through `BimIo.ImportGeometry(InterchangeFormat.Glb, ...)`.
- Auto: `Plan` reads `InterchangeFormat.TessellationRequiresCompanion` to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the Compute content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference to the Persistence artifact index rather than re-crossing the companion.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, BCL inbox
- Growth: a new evaluation parameter is one column on `TessellationRequest` folded into the Compute content-key; zero new surface and never a new transport.
- Boundary: the companion bridge is the single IFC-to-geometry path because GeometryGym carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the tessellation REQUEST orchestration (issuing `TessellationRequest` over the companion rpc, re-importing the GLB, and the content-key cache reuse) is owned at `csharp:Compute/interchange#TWO_HOP_TESSELLATION` and Bim builds only the request shape and consumes the re-imported GLB through `IMPORT_RAIL`; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry` (`python:geometry/ifc-companion`), never a NuGet pin, reached only through Compute's existing companion rpc so this page mints no transport, no channel, and no second wire vocabulary; the IFC semantic graph (from the `IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the Compute content-key; the AP242 CAD-STEP bridge rides the same companion shape over `FORMAT_AXIS`.

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
                InterchangeIdentity.Key(source.Key, ifcBytes.ToArray(), policy.Deflection, policy.Tolerance, policy.AngleTolerance), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, InterchangeFormat.Glb))
            : Fin.Fail<TessellationRequest>(new BimFault.ModelRejected($"<tessellation-not-required:{source.Key}>"));

    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}
```

## [6]-[RESEARCH]

- [AP242_CODEC]: the ISO 10303 AP203/AP214/AP242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the three protocols ride one `step-iso10303` codec discriminated by the `StepProtocol` column (`Ap203`=203 config-controlled read-only, `Ap214`=214 automotive read-write, `Ap242`=242 model-based engineering read-write, `None`=0 on every non-STEP row), all routing geometry evaluation through the same Compute companion rail GeometryGym IFC uses because managed STEP solid evaluation has no in-proc kernel, so the codec owns the semantic/topology read and the companion owns tessellation; the `.step`/`.stp`/`.p21` extension and `application/step` media type resolve to AP242 by `StepProtocol` rank with AP203/AP214 reachable through their explicit row keys, the `step-ap242-reader-pending` `CataloguePackage` marker names the unadmitted managed STEP reader, and the row, codec, protocol, and frame columns are transcription-complete with the body grounding at the cross-folder Python-companion alignment.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader); the STL/3MF/OBJ/PLY mesh-text decode is managed-in-intent but the reader packages are undecompiled, so the `mesh-text` import arm faults `mesh-text-catalogue-pending` and its decode member spellings ground against the admitted mesh-text libraries at the next admission gate; only the rows, codecs, capability, and frame columns are transcription-complete here — the decode bodies are explicitly undone until the package decompile lands, and the native-companion legs land at the Python-companion cross-branch touchpoint.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the semantic graph and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection and the body grounds against the GeometryGym IFC5 member surface at alignment.
- [MESH_COMPRESSION]: the thirteen glTF KHR/EXT extension rows ride the `KhrExtension` `[SmartEnum<string>]` axis on the codec extension column, each carrying its decompile-verified SharpGLTF schema-type owner where SharpGLTF exposes a named Schema2 type — `KHR_materials_clearcoat`→`MaterialClearCoat`, `KHR_materials_transmission`→`MaterialTransmission`, `KHR_materials_volume`→`MaterialVolume`, `KHR_materials_specular`→`MaterialSpecular`, `KHR_materials_ior`→`MaterialIOR`, `KHR_materials_iridescence`→`MaterialIridescence`, `KHR_materials_sheen`→`MaterialSheen`, `KHR_materials_emissive_strength`→`MaterialEmissiveStrength`, `KHR_texture_basisu`→`TextureKTX2`, `KHR_lights_punctual`→`PunctualLight` — each registered through its `Registrar` closure over the generic `ExtensionsFactory.RegisterExtension<ModelRoot, TExt>(name)` (the sole catalogued overload; the `(Type, Type, string)` form is a phantom) before write; the `KHR_draco_mesh_compression`/`EXT_meshopt_compression`/`KHR_mesh_quantization` rows carry `Registrar=None` and a `CataloguePackage` marker because SharpGLTF encodes compression through a toolkit path with no named schema type, so the `MeshCompression` column selects the encode and the row's `Register` faults `khr-catalogue-pending` until the toolkit encode member spelling confirms; the 3D-Tiles tileset b3dm/glTF tile content schema and the Draco/Meshopt encode body ground at the SharpGLTF toolkit catalogue.
- [CANDIDATE_FORMATS]: the verify-before-admit `usd`/`usdz`/`fbx`/`dae` rows admit as candidate `InterchangeFormat` rows carrying media-type, extension set, frame columns, and a `CataloguePending` codec slot naming the admitting package (`USD.NET` for USD stage I/O, `Ufbx` for FBX, `Collada141` for COLLADA `.dae`) with `CanImport=false`/`CanExport=false` so the format is enumerable and `Detect`-able while a managed import/export faults `import-catalogue-pending`; each candidate codec (`usd-stage`, `fbx-sdk`, `collada-xml`) promotes in place — capability columns flip and the codec body grounds — when the named package decompile lands at the next admission gate, never an invented member spelling before the catalogue confirms.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated by `csharp:Compute/interchange#TWO_HOP_TESSELLATION`; the Bim `TessellationRequest` shape is transcription-complete, the companion wire detail rides Compute's existing companion rpc and the orchestration lands when the Compute and Python branches author the hop.
- [CONTENT_IDENTITY_CONSUME]: the `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation is owned at `csharp:Compute/interchange#CONTENT_ADDRESSING` and consumed here for the `ExportArtifact.ContentKey` and `TessellationRequest.IfcContentKey` slots; the exact public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the artifact lands content-addressed on the Persistence blob lane through the Compute `InterchangeIdentity.Admit` path — Bim mints no second identity scheme and no second blob owner.
