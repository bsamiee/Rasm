# [BIM_INTERCHANGE]

The universal interchange codec: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to BIM semantic graph or geometry) from export (artifact to foreign bytes), the `InterchangeCodec`/`KhrExtension` axes, the per-importer `FrameNormalization` onto the canonical kernel frame, the `BimIo` import/export fold, the `IfcSemanticModel` graph projection, and the `TessellationRequest` companion bridge. The page composes the kernel `Rasm` geometry, the `csharp:Compute/interchange#CONTENT_ADDRESSING` `InterchangeIdentity` content key, the `csharp:Compute/interchange#TWO_HOP_TESSELLATION` companion tessellation rail, and the `csharp:Compute/remote#TRANSPORT_AXIS` transport as settled vocabulary; the page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[FORMAT_AXIS]: format/codec/extension rows; capability, companion, and frame-normalization columns.
- [3]-[IMPORT_RAIL]: foreign-bytes ingest — managed mesh decode and in-process semantic IFC/IFC5/STEP graph.
- [4]-[EXPORT_RAIL]: artifact emit — GLB mesh-and-scene with Draco/meshopt encode, IFC STEP/XML/JSON serialization.
- [5]-[TESSELLATION_BRIDGE]: IFC/AP242/native geometry crosses to the Compute companion rail, never in-process.

## [2]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, `TessellationRequiresCompanion`, and the `UpAxis`/`Handedness` ingest-frame columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package or companion that reads and writes the row; `UpAxis`/`Handedness` the per-importer local-frame enums; `FrameNormalization` the static reconciliation surface coercing every imported coordinate into the canonical kernel frame.
- Auto: `Detect` resolves a row from a file extension or media type through the frozen extension index so a path or wire media-type lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the companion bridge and a managed glTF/mesh decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change mapping glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical kernel Z-up right-handed frame.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, companion column, and `StepProtocol` discriminant; a new managed codec package is one `InterchangeCodec` row; a new glTF KHR/EXT capability is one `KhrExtension` row on the codec extension axis carrying its SharpGLTF schema-type owner and registration key; a not-yet-decompiled format admits as a candidate row carrying a `CataloguePending` marker naming the package, promoted in place when the catalogue lands.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC/STEP/native rows because GeometryGym carries no tessellation kernel — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; the `StepProtocol` column disambiguates the three ISO 10303 application protocols sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` extension set — AP203 (config-controlled 3D design, `CanExport=false` because the managed branch reads but never re-authors a config-controlled assembly), AP214 (automotive core, import-and-export), AP242 (model-based 3D engineering, the merged successor) — so a STEP file resolves to one codec row and the protocol is a data column the reader switches on, with `StepProtocol.None` on every non-STEP row keeping the column total; the `KhrExtension` axis carries each ratified glTF extension as one row owning its decompile-verified SharpGLTF schema type or its encoder owner — compression rows own `Openize.Drako` (`KHR_draco_mesh_compression`, encode and decode) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`, encode), registered through `ExtensionsFactory.RegisterExtension<TParent,TExt>(name)` before any read/write — a per-extension importer/exporter type is the deleted form; the candidate `fbx`/`dae` rows carry a `CataloguePending` marker naming the admitting package (`Ufbx`, `Collada141`) and no invented member spellings, holding the media-type, extension set, and codec slot so the format is enumerable and detectable as `CanImport=false`/`CanExport=false` until the catalogue promotes the row; the `usd`/`usdz` rows carry the OpenUSD scene-graph codec slot as a peer coexistence axis (Core Spec 1.0) — USD carries the scene, IFC carries the BIM semantics, so the USD codec is a scene-graph peer, never a BIM-semantic replacement; media types are the IANA `model/gltf-binary`, `model/gltf+json`, `application/step`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, plus the `model/vnd.usd`/`model/vnd.usdz+zip` USD and the candidate `application/vnd.autodesk.fbx`/`model/vnd.collada+xml` media types; the chunked simulation-field codec, the FastCDC structural geometry-delta codec, and the content-addressed artifact identity stay at `csharp:Compute/interchange` and are consumed at the seam, never re-minted here.

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

- Owner: `BimIo` — the import fold over `InterchangeFormat`, dispatching the managed glTF/GLB mesh-and-scene decode through SharpGLTF, the STL/3MF/OBJ/PLY mesh-text arm that faults until its reader package catalogues, the in-process semantic IFC/IFC5 ingest through GeometryGym over `DatabaseIfc`/`Extract<T>`, and the AP242/native-companion two-hop route; `ImportedGeometry` the decoded mesh-scene carrier, `IfcSemanticModel` the IFC model-graph projection.
- Entry: `BimIo.ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks)` for the managed glTF and mesh-text path; `BimIo.ImportIfc(...)` for the in-process IFC/IFC5 semantic graph — `Fin<T>` aborts on a codec reject or a capability miss, the foreign decode arity discriminating on the row's `InterchangeCodec` so a path lands one decode without a call-site type branch, projecting the package exception onto `BimFault.ModelRejected` at the boundary so domain code never sees the SharpGLTF `ModelException` or the GeometryGym parse fault.
- Auto: binary GLB decode lands through `ModelRoot.ParseGLB(ArraySegment<byte>)` and text `.gltf` decode through `ReadContext.ReadTextSchema2(Stream)` then `model.LogicalMeshes.Decode()` projecting `IMeshDecoder<Material>` primitives to `ImportedGeometry` vertex and index spans with zero intermediate file; the IFC semantic path constructs a `DatabaseIfc` over the bytes through `DatabaseIfc.ParseString`/`ReadXMLDoc`/`ReadJSON` by the row's format, narrows `db.Project`, and folds `db.Project.Extract<T>()` collecting spatial hierarchy (`IfcSpatialStructureElement`), products (`IfcProduct`), property sets (`IfcPropertySet`), quantities (`IfcElementQuantity`), materials (`IfcRelAssociatesMaterial`), classification associations (`IfcRelAssociatesClassification`), type objects (`IfcTypeObject`), and decomposition relationships (`IfcRelDecomposes`) into the `IfcSemanticModel` graph — never tessellated BRep.
- Receipt: the `ModelLoad` receipt case carries the format key, codec key, source byte count, and elapsed for a managed mesh import; an IFC semantic ingest stamps the schema version (`db.Release`), the model-view (`db.ModelView`), and the extracted-entity counts; emission rides the sink port at the composition edge.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, NodaTime, LanguageExt.Core, Rasm
- Growth: a new managed import is one codec arm on the import fold keyed by the `InterchangeFormat.Codec` row; a new extracted IFC entity family is one `Extract<T>` projection on `IfcSemanticModel`; a new STEP application protocol is one `InterchangeFormat` row carrying its `StepProtocol` discriminant — the `step-iso10303` codec reads the protocol column to select the entity-instance vocabulary version so a single STEP reader spans all three without a per-protocol codec.
- Boundary: `BimIo` is the page boundary capsule and its codec arms carry the language-owned statement forms the foreign package decode requires; glTF mesh decode rides the `MeshDecoder.Decode` runtime contract reading `IMeshPrimitiveDecoder.GetPosition`/`GetNormal`/`TriangleIndices` (an accessor-based contract returning per-vertex `Vector3`/index-tuple values, so the decode materializes one contiguous `ImportedGeometry` vertex/normal/index triple at the boundary — the accessor contract admits no zero-copy span into SharpGLTF's internal buffers, so the one boundary materialization, not a per-primitive `float[]` proliferation, is the allocation point); the IFC semantic graph is a model-data projection only — `BaseClassIfc.Extract<T>` collects reachable entities and GeometryGym carries no tessellation kernel, so a geometry request on an IFC row routes to the `TESSELLATION_BRIDGE` rail and never evaluates a BRep in-process; the `step-iso10303` STEP solid-model path reads the entity-instance graph at the protocol the `StepProtocol` column names (the AP203/AP214/AP242 EXPRESS vocabularies share the STEP physical-file token grammar) and routes its B-rep/NURBS evaluation through the same companion rail because managed STEP solid evaluation has no in-process kernel; `DatabaseIfc.Tolerance`/`ToleranceAngleRadians`/`ScaleSI` read the model precision the content-key folds; the SharpGLTF `ReadSettings.Validation` rides `ValidationMode.Strict` on import so a malformed asset faults at parse; a candidate `fbx`/`dae` import faults at the boundary with the `import-catalogue-pending` rail naming the admitting package because the format is enumerable and detectable but its codec body is unwritten until the catalogue lands; an `IfcImporter`/`GltfImporter` service family and a managed IFC tessellator are the deleted forms.

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
    Seq<IfcSemanticModel.ClassificationRow> Classifications,
    Seq<IfcSemanticModel.TypeRow> Types,
    Seq<AssemblyRel> Decomposition,
    double Tolerance,
    Instant At) {
    public sealed record SpatialNode(string GlobalId, string EntityType, string Name, string LongName, Seq<string> ContainedGlobalIds);
    public sealed record ProductRow(string GlobalId, string EntityType, string Name, string Tag, Option<string> TypeGlobalId);
    public sealed record PropertyRow(string OwnerGlobalId, string SetName, string PropertyName, string Value);
    public sealed record QuantityRow(string OwnerGlobalId, string SetName, string QuantityName, double Value, string Unit);
    public sealed record MaterialRow(string OwnerGlobalId, string MaterialName);
    public sealed record ClassificationRow(string OwnerGlobalId, string System, string Code, string DictionaryClassUri);
    public sealed record TypeRow(string GlobalId, string EntityType, string Name);
}

public static class BimIo {
    public static Fin<ImportedGeometry> ImportGeometry(InterchangeFormat format, ReadOnlyMemory<byte> bytes, ClockPolicy clocks) =>
        !format.CanImport ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-unsupported:{format.Key}>"))
        : format.CataloguePending ? Fin.Fail<ImportedGeometry>(new BimFault.ModelRejected($"<import-catalogue-pending:{format.Key}:{format.Codec.CataloguePackage.IfNone("<unknown>")}>"))
        : format.Codec == InterchangeCodec.SharpGltf ? Boundary(() => Framed(format, Gltf(format, bytes, clocks.Now)))
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
            project.Extract<IfcRelAssociatesClassification>().AsIterable()
                .SelectMany(static r => r.RelatedObjects.Select(o => (o.GlobalId, reference: r.RelatingClassification as IfcClassificationReference)))
                .Where(static pair => pair.reference is not null)
                .Select(static pair => new IfcSemanticModel.ClassificationRow(
                    pair.GlobalId,
                    (pair.reference!.ReferencedSource as IfcClassification)?.Name ?? "",
                    pair.reference.Identification ?? "",
                    pair.reference.Location ?? "")).ToSeq(),
            project.Extract<IfcTypeObject>().AsIterable()
                .Map(static t => new IfcSemanticModel.TypeRow(t.GlobalId, t.GetType().Name, t.Name ?? "")).ToSeq(),
            project.Extract<IfcRelAggregates>().AsIterable()
                .Map(static r => (AssemblyRel)new AssemblyRel.Aggregates(r.RelatingObject.GlobalId, r.RelatedObjects.Select(static o => o.GlobalId).ToSeq())).ToSeq(),
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

- Owner: `BimExport` — the export fold over `InterchangeFormat`, dispatching mesh-and-scene to GLB through the SharpGLTF `SceneBuilder`/`MeshBuilder` path with Draco/meshopt encode, and a model graph to IFC STEP/XML/JSON through GeometryGym `DatabaseIfc` serialization; `ExportArtifact` the emitted-bytes carrier feeding the Compute content-addressing seam.
- Entry: `BimExport.Export(InterchangeFormat format, ImportedGeometry geometry, InterchangePolicy policy, ClockPolicy clocks)` for the GLB geometry path; `BimExport.ExportIfc(InterchangeFormat format, IfcSemanticModel model, InterchangePolicy policy, ClockPolicy clocks)` for the IFC model serialization — `Fin<T>` aborts on a write-capability miss or a codec fault projected onto `BimFault.ModelRejected`.
- Auto: GLB export assembles a `SceneBuilder` from `MeshBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty>` primitives through `PrimitiveBuilder.AddTriangle`, attaches through `SceneBuilder.AddRigidMesh`, converts through `SceneBuilder.ToGltf2(SceneBuilderSchema2Settings)`, applies the `KhrEncoder`-selected compression encode, and writes through `ModelRoot.WriteGLB` to bytes; IFC export selects the format through `DatabaseIfc.ToString(FormatIfcSerialization)` mapping the `ifc`/`ifc-xml`/`ifc-json` row to `STEP`/`XML`/`JSON`, with the model graph re-authored into a `DatabaseIfc` at the row's `ReleaseVersion` through the `FactoryIfc` canonical placements.
- Receipt: the `StreamSegment` receipt carries the format key, codec key, emitted byte count, and the content-key the Compute addressing seam computes; emission rides the sink port.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, NodaTime, LanguageExt.Core
- Growth: a new managed export is one codec arm on the export fold; a new IFC serialization format is one `InterchangeFormat` row mapping to a `FormatIfcSerialization` value; a new glTF KHR/EXT capability the exporter attaches is one `KhrExtension` row registered through `KhrExtension.Register` before write; a new compression encoder is one `KhrEncoder` arm on the `GlbBytes` fold.
- Boundary: the export fold extends the `BimExport` boundary capsule; GLB emission rides `SceneBuilderSchema2Settings` for strided buffers and buffer-merge so the emitted artifact is deterministic byte layout the Compute content-key addresses, and `ModelRoot.MergeBuffers` consolidates logical buffers before write so the same geometry always emits the same bytes; the `KhrExtension` rows the policy carries register through the per-row `Registrar` closure each closing over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)` before any write so a material/light/texture channel serializes through its decompile-verified SharpGLTF schema type rather than a hand-authored JSON extension block; the `KHR_draco_mesh_compression` and `KHR_meshopt_compression` rows carry a `KhrEncoder` discriminant rather than a SharpGLTF schema type because SharpGLTF ships no compression encoder — `Openize.Drako` owns the Draco encode and `Alimer.Bindings.MeshOptimizer` owns the meshopt encode, both net10-only and the Compute EXPORT_RAIL outside-Rhino concern, so the `GlbBytes` fold routes the compression leg through those packages and a managed in-Rhino-ALC compression encode is the rejected form; IFC serialization selects `FormatIfcSerialization.STEP`/`XML`/`JSON` by the row and `DatabaseIfc.WriteStream`/`ToString` are the only emit members — a hand-rolled STEP writer is the deleted form; the model graph re-authoring runs through `FactoryIfc` canonical axes, origins, and owner history so a round-tripped model carries stable GlobalIds through `ParserIfc.HashGlobalID` keyed on a stable entity key rather than a fresh GUID per export, making export idempotent under the Compute content-key; a write to a row whose `CanExport` is false faults at the boundary; the chunked-field and structural-delta codecs stay at `csharp:Compute/interchange` consumed at the seam.

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
    int MeshoptQuantizationBits,
    Seq<KhrExtension> Extensions) {
    public static readonly InterchangePolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        IfcSchema: ReleaseVersion.IFC4X3_ADD2, StepProtocol: StepProtocol.Ap242, MergeBuffers: true, StridedBuffers: true,
        Compression: KhrEncoder.None, MeshoptQuantizationBits: 14, Extensions: Seq<KhrExtension>());
    public static readonly InterchangePolicy Web = Canonical with {
        Compression = KhrEncoder.Meshopt, MeshoptQuantizationBits = 12,
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
        RegisterExtensions(policy).Map(_ => WriteGlb(SceneOf(geometry), policy));

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
        var encoded = policy.Compression switch {
            KhrEncoder.Draco => DracoEncoder.Encode(model, policy.MeshoptQuantizationBits),
            KhrEncoder.Meshopt => MeshOptimizer.EncodeGltf(model, policy.MeshoptQuantizationBits),
            _ => model,
        };
        return encoded.WriteGLB(new WriteSettings { MergeBuffers = policy.MergeBuffers }).ToArray();
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
            .Filter(static khr => khr.Registrar.IsSome)
            .Map(static khr => khr.Register())
            .Traverse(static result => result)
            .Map(static _ => unit);

    static ExportArtifact Sealed(InterchangeFormat format, byte[] bytes, InterchangePolicy policy, Instant at) =>
        new(format, bytes, InterchangeIdentity.Key(format.Key, bytes, policy.Deflection, policy.Tolerance, policy.AngleTolerance), bytes.LongLength, at);
}
```

## [5]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` — the Bim-side request shape crossing IFC/AP242/native geometry evaluation to the `csharp:Compute/interchange#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `IMPORT_RAIL` glTF path; the request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself.
- Entry: `TessellationRequest.Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; Compute issues the request over `csharp:Compute/remote#TRANSPORT_AXIS` and the GLB result re-enters through `BimIo.ImportGeometry(InterchangeFormat.Glb, ...)`.
- Auto: `Plan` reads `InterchangeFormat.TessellationRequiresCompanion` to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the Compute content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference to the Persistence artifact index rather than re-crossing the companion.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing
- Growth: a new evaluation parameter is one column on `TessellationRequest` folded into the Compute content-key; never a new transport.
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

- [AP242_CODEC]: the ISO 10303 AP203/AP214/AP242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the three protocols ride one `step-iso10303` codec discriminated by the `StepProtocol` column, all routing geometry evaluation through the same Compute companion rail GeometryGym IFC uses because managed STEP solid evaluation has no in-process kernel; the `step-ap242-reader-pending` `CataloguePackage` marker names the unadmitted managed STEP reader and the row, codec, protocol, and frame columns are settled with the semantic-read body grounding at the cross-folder Python-companion alignment.
- [NATIVE_FORMAT_BRIDGES]: the Revit `.rvt`, Navisworks `.nwc`/`.nwd`, and DWG/DXF native readers ride the `native-companion` codec through the Compute companion process (the managed C# branch has no native loader); the STL/3MF/OBJ/PLY mesh-text decode is managed-in-intent but the reader packages are uncatalogued, so the `mesh-text` import arm faults until its decode member spellings ground against the admitted mesh-text libraries.
- [IFC5_ECS]: the IFC5 ECS-JSON (`.ifcx`) component-graph parse rides the GeometryGym IFC5 surface for the semantic graph and the Compute companion for native-grade tessellation; the `ifc5` row mirrors the IFC4x3 ingest with the ECS-component projection. IFC4.3 ADD2 is the production baseline (`ReleaseVersion.IFC4X3_ADD2`); IFC5 is the componentized/granular `.ifcx` architecture in active public development, so the `ifc5` row is a forward-looking GeometryGym IFC5-surface row grounding against the GeometryGym IFC5 member surface at alignment.
- [GLTF_EXTENSIONS]: the ratified Khronos KHR/EXT extension rows ride the `KhrExtension` axis on the codec extension column, each material/texture/scene/metadata row carrying its decompile-verified SharpGLTF Schema2 type owner where SharpGLTF exposes one (`MaterialClearCoat`, `MaterialTransmission`, `MaterialVolume`, `MaterialSpecular`, `MaterialIOR`, `MaterialIridescence`, `MaterialSheen`, `MaterialEmissiveStrength`, `MaterialAnisotropy`, `MaterialDispersion`, `MaterialDiffuseTransmission`, `MaterialUnlit`, `TextureKTX2`, `TextureTransform`, `PunctualLight`) registered through its `Registrar` closure over `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)`; the `KHR_node_visibility`/`KHR_animation_pointer`/`KHR_gaussian_splatting`/`KHR_xmp_json_ld`/`KHR_materials_variants` rows carry `Registrar=None` until their SharpGLTF schema-type or codec spelling confirms; the `KHR_draco_mesh_compression`/`KHR_meshopt_compression`/`KHR_mesh_quantization` rows carry a `KhrEncoder` discriminant and route the encode through `Openize.Drako` (`DracoEncoder.Encode`) and `Alimer.Bindings.MeshOptimizer` (`MeshOptimizer.EncodeGltf`) whose member spellings confirm against those package surfaces.
- [USD_COEXISTENCE]: the OpenUSD stage I/O for the `usd`/`usdz` rows rides the OpenUSD Core Spec 1.0 scene-graph as a host-neutral peer coexistence axis — USD carries the scene graph, IFC carries the BIM semantics, so the USD codec is a scene-graph peer, never a BIM-semantic replacement; the `usd-stage` codec stage-read/stage-write member spellings ground against the admitted OpenUSD managed surface at the next alignment.
- [CANDIDATE_FORMATS]: the `fbx`/`dae` rows admit as candidate `InterchangeFormat` rows carrying media-type, extension set, frame columns, and a `CataloguePending` codec slot naming the admitting package (`Ufbx` for FBX, `Collada141` for COLLADA `.dae`) with `CanImport=false`/`CanExport=false` so the format is enumerable and `Detect`-able while a managed import/export faults `import-catalogue-pending`; each candidate codec promotes in place — capability columns flip and the codec body grounds — when the named package catalogue lands, never an invented member spelling before the catalogue confirms.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated by `csharp:Compute/interchange#TWO_HOP_TESSELLATION`; the Bim `TessellationRequest` shape is settled and the companion wire detail rides Compute's existing companion rpc.
- [CONTENT_IDENTITY_CONSUME]: the `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation is owned at `csharp:Compute/interchange#CONTENT_ADDRESSING` and consumed here for the `ExportArtifact.ContentKey` and `TessellationRequest.IfcContentKey` slots; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the artifact lands content-addressed on the Persistence blob lane through the Compute `InterchangeIdentity.Admit` path — Bim mints no second identity scheme and no second blob owner.
