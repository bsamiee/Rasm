# [BIM_FORMAT_AXIS]

The interchange format vocabulary: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to BIM semantic graph or geometry) from export (artifact to foreign bytes), the `InterchangeCodec`/`KhrExtension` codec-and-extension axes, and the per-importer `FrameNormalization` coercing every imported coordinate onto the canonical kernel frame. The page composes the kernel `Rasm` geometry as settled vocabulary; the `import-rail#IMPORT_RAIL` ingest and `export-rail#EXPORT_RAIL` emit read these rows to dispatch a codec without a call-site branch. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[FORMAT_AXIS]: format/codec/extension rows; capability, companion, and frame-normalization columns.

## [2]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, `TessellationRequiresCompanion`, and the `UpAxis`/`Handedness` ingest-frame columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package or companion that reads and writes the row; `UpAxis`/`Handedness` the per-importer local-frame enums; `FrameNormalization` the static reconciliation surface coercing every imported coordinate into the canonical kernel frame.
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the companion bridge and a managed glTF/mesh decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change mapping glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical kernel Z-up right-handed frame.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, companion column, and `StepProtocol` discriminant; a new managed codec package is one `InterchangeCodec` row; a new glTF KHR/EXT capability is one `KhrExtension` row on the codec extension axis carrying its SharpGLTF schema-type owner and registration key; a not-yet-decompiled format admits as a candidate row carrying a `CataloguePending` marker naming the package, promoted in place when the catalogue lands.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC/STEP/native rows because GeometryGym carries no tessellation kernel — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; the `StepProtocol` column disambiguates the three ISO 10303 application protocols sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` extension set — AP203 (config-controlled 3D design, `CanExport=false` because the managed branch reads but never re-authors a config-controlled assembly), AP214 (automotive core, import-and-export), AP242 (model-based 3D engineering, the merged successor) — so a STEP file resolves to one codec row and the protocol is a data column the reader switches on, with `StepProtocol.None` on every non-STEP row keeping the column total; the `KhrExtension` axis carries each ratified glTF extension as one row owning its decompile-verified SharpGLTF schema type or its encoder owner — compression rows own `Openize.Drako` (`KHR_draco_mesh_compression`, encode and decode) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`, encode), registered through `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read/write — a per-extension importer/exporter type is the deleted form; the candidate `fbx`/`dae` rows carry a `CataloguePending` marker naming the admitting package (`Ufbx`, `Collada141`) and no invented member spellings, holding the media-type, extension set, and codec slot so the format is enumerable and detectable as `CanImport=false`/`CanExport=false` until the catalogue promotes the row; the `usd`/`usdz` rows carry the OpenUSD scene-graph codec slot as a peer coexistence axis (Core Spec 1.0) — USD carries the scene, IFC carries the BIM semantics, so the USD codec is a scene-graph peer, never a BIM-semantic replacement; media types are the IANA `model/gltf-binary`, `model/gltf+json`, `application/step`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, plus the `model/vnd.usd`/`model/vnd.usdz+zip` USD and the candidate `application/vnd.autodesk.fbx`/`model/vnd.collada+xml` media types; the chunked simulation-field codec, the FastCDC structural geometry-delta codec, and the content-addressed artifact identity stay at `Rasm.Compute/interchange/codecs` and are consumed at the seam, never re-minted here.

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
    public static readonly InterchangeCodec MeshText = new("mesh-text", managed: true, companion: false, cataloguePackage: "<stl-3mf-obj-ply-reader-pending>");
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", managed: true, companion: false, cataloguePackage: Option<string>.None);
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
    public static readonly KhrExtension DracoMeshCompression = new("KHR_draco_mesh_compression", KhrSlot.Compression, encoder: KhrEncoder.Draco, registrar: Option<Action>.None);
    public static readonly KhrExtension MeshoptCompression = new("KHR_meshopt_compression", KhrSlot.Compression, encoder: KhrEncoder.Meshopt, registrar: Option<Action>.None);
    public static readonly KhrExtension MeshQuantization = new("KHR_mesh_quantization", KhrSlot.Geometry, encoder: KhrEncoder.Quantization, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureTransform = new("KHR_texture_transform", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<TextureInfo, TextureTransform>("KHR_texture_transform"));
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, TextureKTX2>("KHR_texture_basisu"));
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<ModelRoot, PunctualLight>("KHR_lights_punctual"));
    public static readonly KhrExtension NodeVisibility = new("KHR_node_visibility", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension AnimationPointer = new("KHR_animation_pointer", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension GaussianSplatting = new("KHR_gaussian_splatting", KhrSlot.Geometry, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension XmpJsonLd = new("KHR_xmp_json_ld", KhrSlot.Metadata, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsVariants = new("KHR_materials_variants", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsUnlit = new("KHR_materials_unlit", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialUnlit>("KHR_materials_unlit"));
    public static readonly KhrExtension MaterialsSpecular = new("KHR_materials_specular", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialSpecular>("KHR_materials_specular"));
    public static readonly KhrExtension MaterialsIor = new("KHR_materials_ior", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialIOR>("KHR_materials_ior"));
    public static readonly KhrExtension MaterialsIridescence = new("KHR_materials_iridescence", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialIridescence>("KHR_materials_iridescence"));
    public static readonly KhrExtension MaterialsSheen = new("KHR_materials_sheen", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialSheen>("KHR_materials_sheen"));
    public static readonly KhrExtension MaterialsClearcoat = new("KHR_materials_clearcoat", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialClearCoat>("KHR_materials_clearcoat"));
    public static readonly KhrExtension MaterialsTransmission = new("KHR_materials_transmission", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialTransmission>("KHR_materials_transmission"));
    public static readonly KhrExtension MaterialsVolume = new("KHR_materials_volume", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialVolume>("KHR_materials_volume"));
    public static readonly KhrExtension MaterialsAnisotropy = new("KHR_materials_anisotropy", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialAnisotropy>("KHR_materials_anisotropy"));
    public static readonly KhrExtension MaterialsDispersion = new("KHR_materials_dispersion", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialDispersion>("KHR_materials_dispersion"));
    public static readonly KhrExtension MaterialsDiffuseTransmission = new("KHR_materials_diffuse_transmission", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialDiffuseTransmission>("KHR_materials_diffuse_transmission"));
    public static readonly KhrExtension MaterialsEmissiveStrength = new("KHR_materials_emissive_strength", KhrSlot.Material, encoder: KhrEncoder.None, registrar: () => ExtensionsFactory.RegisterExtension<Material, MaterialEmissiveStrength>("KHR_materials_emissive_strength"));

    public KhrSlot Slot { get; }
    public KhrEncoder Encoder { get; }
    public Option<Action> Registrar { get; }

    public Fin<KhrExtension> Register() =>
        Registrar.Match(
            Some: register => Try.lift(() => { register(); return this; }).Run().MapFail(static error => (Error)new BimFault.ModelRejected(error.Message)),
            None: () => Fin.Succ(this));
}

public enum KhrSlot : byte { Compression = 0, Geometry = 1, Texture = 2, Scene = 3, Material = 4, Metadata = 5 }

public enum KhrEncoder : byte { None = 0, Draco = 1, Meshopt = 2, Quantization = 3 }

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
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), canImport: true, canExport: true, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), canImport: true, canExport: true, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
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

    public bool IsCanonicalFrame => UpAxis == UpAxis.Z && Handedness == Handedness.Right;

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
    public static void Canonicalize(InterchangeFormat format, Span<float> vertices, int stride) {
        if (format.IsCanonicalFrame) {
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
            vertices[offset + 1] = format.Handedness == Handedness.Right ? uy : -uy;
            vertices[offset + 2] = uz;
        }
    }
}
```

## [3]-[RESEARCH]

- [GLTF_EXTENSIONS]: the ratified Khronos KHR/EXT extension rows ride the `KhrExtension` axis on the codec extension column, each material/texture/scene/metadata row carrying its decompile-verified SharpGLTF Schema2 type owner where SharpGLTF exposes one (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `MaterialAnisotropy`, `MaterialDispersion`, `MaterialDiffuseTransmission`, `MaterialUnlit`, `TextureKTX2`, `TextureTransform`, `PunctualLight`) registered through its `Registrar` closure over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)`; the `KHR_node_visibility`/`KHR_animation_pointer`/`KHR_gaussian_splatting`/`KHR_xmp_json_ld`/`KHR_materials_variants` rows carry `Registrar=None` until their SharpGLTF schema-type or codec spelling confirms; the `KHR_draco_mesh_compression`/`KHR_meshopt_compression`/`KHR_mesh_quantization` rows carry a `KhrEncoder` discriminant and route the encode through `Openize.Drako` (`Draco.Encode(DracoPointCloud, DracoEncodeOptions)` over a `DracoMesh` built from `PointAttribute.Wrap`) and `Alimer.Bindings.MeshOptimizer` (`Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` over the raw vertex/index buffers), both verified against those package surfaces; the glTF-to-Draco attribute mapping and the meshopt bufferView framing into the `EXT_meshopt_compression` extension block ground at the codec admission gate.
- [USD_COEXISTENCE]: the OpenUSD stage I/O for the `usd`/`usdz` rows rides the OpenUSD Core Spec 1.0 scene-graph as a host-neutral peer coexistence axis — USD carries the scene graph, IFC carries the BIM semantics, so the USD codec is a scene-graph peer, never a BIM-semantic replacement; the `usd-stage` codec stage-read/stage-write member spellings ground against the admitted OpenUSD managed surface at the next alignment.
- [CANDIDATE_FORMATS]: the `fbx`/`dae` rows admit as candidate `InterchangeFormat` rows carrying media-type, extension set, frame columns, and a `CataloguePending` codec slot naming the admitting package (`Ufbx` for FBX, `Collada141` for COLLADA `.dae`) with `CanImport=false`/`CanExport=false` so the format is enumerable and `Detect`-able while a managed import/export faults `import-catalogue-pending`; each candidate codec promotes in place — capability columns flip and the codec body grounds — when the named package catalogue lands, never an invented member spelling before the catalogue confirms.
