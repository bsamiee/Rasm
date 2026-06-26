# [BIM_FORMAT_AXIS]

The interchange format vocabulary: one `InterchangeFormat` `[SmartEnum<string>]` table discriminating import (foreign bytes to BIM semantic graph or geometry) from export (artifact to foreign bytes), the `InterchangeCodec`/`KhrExtension` codec-and-extension axes, and the per-importer `FrameNormalization` coercing every imported coordinate onto the canonical kernel frame. The page composes the kernel `Rasm` geometry as settled vocabulary; the `import#IMPORT_RAIL` ingest and `export#EXPORT_RAIL` emit read these rows to dispatch a codec without a call-site branch. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[FORMAT_AXIS]: format/codec/extension rows; capability, companion, and frame-normalization columns.

## [02]-[FORMAT_AXIS]

- Owner: `InterchangeKeyPolicy` ordinal accessor; `InterchangeFormat` `[SmartEnum<string>]` rows carrying media-type, extension set, `CanImport`/`CanExport` capability, codec-owner discriminant, `TessellationRequiresCompanion`, and the `UpAxis`/`Handedness` ingest-frame columns; `InterchangeCodec` codec-owner vocabulary discriminating the managed package or companion that reads and writes the row; `UpAxis`/`Handedness` the per-importer local-frame enums; `FrameNormalization` the static reconciliation surface coercing every imported coordinate into the canonical kernel frame.
- Auto: `Detect` resolves a row from a key, media type, file path, or a bare dotted extension through the frozen indices so a path, a wire media-type, or a `.glb`-style bare extension (which `Path.GetExtension` returns empty for) lands one row with zero call-site branching; `Companion` reads the `TessellationRequiresCompanion` column so the import fold routes an IFC/AP242/native geometry request to the companion bridge and a managed glTF/mesh decode inline without an `if (ifc)` branch; `FrameNormalization.Canonicalize` reads the row's `UpAxis`/`Handedness` columns and applies the one basis change mapping glTF Y-up right-handed, IFC/Rhino Z-up right-handed, and every other importer frame onto the canonical kernel Z-up right-handed frame.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, Themis.Las, UniversalSceneDescription, NetTopologySuite, NetTopologySuite.IO.Esri.Shapefile, bertt.CityJSON, MaxRev.Gdal.Core, ACadSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row carrying its media-type, extension set, capability columns, codec owner, companion column, and `StepProtocol` discriminant; a new managed codec package is one `InterchangeCodec` row; a new glTF KHR/EXT capability is one `KhrExtension` row on the codec extension axis carrying its SharpGLTF schema-type owner and registration key; a not-yet-decompiled format admits as a candidate row carrying a `CataloguePending` marker naming the package, promoted in place when the catalogue lands.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form; `CanImport` and `CanExport` are capability columns the import and export folds read so a write-only or read-only direction faults at the boundary rather than mid-codec, and every IFC row carries `CanImport=true`/`CanExport=true` because GeometryGym is symmetric while every glTF row is symmetric over SharpGLTF; the `TessellationRequiresCompanion` column is `true` exactly on the IFC/STEP/native rows because GeometryGym carries no tessellation kernel — a managed IFC geometry evaluation is the rejected form; the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns; the `StepProtocol` column disambiguates the three ISO 10303 application protocols sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` extension set — AP203 (config-controlled 3D design, `CanExport=false` because the managed branch reads but never re-authors a config-controlled assembly), AP214 (automotive core, import-and-export), AP242 (model-based 3D engineering, the merged successor) — so a STEP file resolves to one codec row and the protocol is a data column the reader switches on, with `StepProtocol.None` on every non-STEP row keeping the column total; the `step-iso10303` codec drops its `CataloguePackage` marker (`Managed=true`) because the in-process BCL-only `import#IMPORT_RAIL` `StepReader` Part-21 entity-instance reader is admitted for the semantic-graph leg, while it keeps `Companion=true` because the B-rep/NURBS geometry leg still routes to the Compute companion (no managed STEP solid evaluator is admitted) — the rejected NuGet readers stand (`IxMilia.Step`/`StepFileParser` are not on NuGet, `STPLoader` is net35/`AForge`-coupled, `DevelApp.StepParser` is a regex grammar engine not a STEP reader) and a native-coupled OpenCASCADE STEP reader in-process is the rejected pick, so `BimIo.ImportStep` lands the product structure in-process and the geometry hop crosses `tessellation#TESSELLATION_BRIDGE` exactly as the IFC geometry request does; IGES is NOT an ISO 10303 STEP protocol — the ANSI IGES section-based file grammar shares neither the STEP physical-file token grammar nor the GeometryGym entity surface — so the `iges` row carries the distinct `iges-ansi` companion codec rather than `step-iso10303`, routing its B-rep/NURBS evaluation through the same Compute companion rail the native-format rows use because no managed IGES reader is admitted (a `StepProtocol.None` IGES row on the `step-iso10303` codec would hand the STEP reader no protocol to switch on and a grammar it cannot parse — the deleted form); the `KhrExtension` axis carries each ratified glTF extension as one row owning its `KhrSlot`/`KhrEncoder` discriminant — the in-box material/texture/scene rows (`KHR_materials_*`, `KHR_texture_transform`, `KHR_texture_basisu`, `KHR_lights_punctual`, `KHR_animation_pointer`, `KHR_xmp_json_ld`) carry `Registrar=None` because SharpGLTF.Core serializes them in-box and they are authored/read only through the public `Material`/`MaterialChannel`/`Texture`/`Node`/`ModelRoot` surface, never named directly (the `KHR_materials_*` and texture extension classes are `internal` in the catalogued assembly, so an `ExtensionsFactory.RegisterExtension<Material, MaterialSpecular>(...)` over an internal type is the rejected form); the `Registrar` closure is reserved for a caller-supplied custom extension class implemented from `JsonSerializable`, never an in-box SharpGLTF type; the compression rows own `Openize.Drako` (`KHR_draco_mesh_compression`, encode and decode) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`, encode and decode) on the `KhrEncoder` discriminant because SharpGLTF.Core ships no compression encoder or decoder — the `export#EXPORT_RAIL` `GlbBytes` switch drives the encode half and the `import#IMPORT_RAIL` `Decompress` pre-decode branch drives the symmetric decode half through `Draco.Decode` and `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer`/`DecodeFilter*`, so both rows are bidirectional and a per-extension importer/exporter type is the deleted form; the `mesh-text` codec names `geometry3Sharp` (pure-managed netstandard2.0, zero-dependency, ALC-safe) as its reader package and grounds the `Stl`/`Obj`/`Off` rows' decode through the `StandardMeshReader`/`OBJFormatReader`/`STLFormatReader` surface — `geometry3Sharp` ships no PLY and no 3MF reader, so PLY and 3MF leave the `mesh-text` codec: `PLY` is now its own dedicated `ply-net` codec naming `Ply.Net` (the `PlyParser.Parse(stream, maxChunkSize)` header-plus-chunked-body decoder over the immutable `Header`/`Dataset`/`ElementData`/`PropertyData` record graph, every property value a typed `System.Array` reachable by name — the `Vertex` element's `x`/`y`/`z` `Float32` columns and the `Face` element's `vertex_indices` list column, ascii/`binary_little_endian`/`binary_big_endian` read off `Header.Format`), retiring the hand-rolled BCL `PlyReader`, and 3MF moves to the `scene-exchange` codec naming `AssimpNetter` (the native Open Asset Import Library wrapper) which retires the hand-rolled BCL `ThreeMfReader`; the `scene-exchange` codec owns the formats no other Bim codec covers — FBX (`.fbx`), Collada (`.dae`), and the standalone 3MF read leg — through one disposable `AssimpContext` whose `ImportFile*`/`ExportFile`/`ExportToBlob` matrix drives the `Scene`→`Node`→`Mesh`→`Material` model and whose `PostProcessSteps` (`Triangulate`/`JoinIdenticalVertices`/`GenerateSmoothNormals`/`CalculateTangentSpace`/`GenerateUVCoords`) is the post-import transform algebra, so the `Fbx`/`Collada`/`ThreeMf` rows are live managed import-and-export (`CanImport=true`/`CanExport=true`) and a per-format `StlImporter`/`PlyImporter`/`FbxImporter` family is the deleted form; the rejected reader picks stand (`lib3mf` native C++, `Aspose.3D` closed/commercial — `AssimpNetter` ships its own osx-arm64 `libassimp.dylib`, RID-coupled but admitted as the one scene-exchange owner); the `acad-sharp` codec names `ACadSharp` (pure-managed AnyCPU IL, osx-arm64-safe, already consumed by `csharp:Rasm.Fabrication` and `csharp:Rasm.AppUi`) as the in-process DWG+DXF reader and the `Dwg` row routes its `.dwg`/`.dxf` extension set to the managed `acad-sharp` codec (`CanImport=true`/`CanExport=true`, `TessellationRequiresCompanion=false`) rather than the `native-companion` two-hop — the `import#IMPORT_RAIL` `BimIo` ACadSharp decode arm folds the `LINE`/`LWPOLYLINE`/`CIRCLE`/`ARC`/`MESH`/`3DFACE`/`INSERT` entities onto the `ImportedGeometry` triangle-soup through the package-owned `Mesh`/`Face3D`/`PolygonalVertexes` surface, and the codec promotes by row data not an `if(dwg)` branch per the row-promotion discipline; `netDxf` (DXF-only) is NOT admitted to Bim because `ACadSharp` supersedes it (managed DWG AND DXF) — admitting a second managed CAD lib for one row is the rejected form, and the native-coupled `Aspose.CAD`/`Teigha`/`ODA` readers break the ALC firebreak; the `fbx`/`dae` rows are now live `scene-exchange` (`AssimpNetter`), retiring the former `Ufbx`/`Collada141` `CataloguePending` markers — `AssimpNetter` is the one admitted FBX+Collada owner, and admitting a second managed FBX/Collada lib beside it is the rejected form; the `las` row is the `point-cloud` codec naming `Themis.Las` (the managed ASPRS LAS reader/writer the `reconstruct#RECONSTRUCTION` scan-to-BIM ingest front decodes through) with the `Unofficial.laszip.netstandard` pure-managed LASzip decompression front composing in for the compressed `.laz` extension, so the codec reads both uncompressed `.las` and arithmetic-coded `.laz` over one ingest path; the `shp`/`gpkg`/`geojson`/`cityjson` rows are the `geospatial-vector` codec the `Semantics/geospatial#VECTOR_INGEST` owner decodes (managed `NetTopologySuite.IO.Esri.Shapefile`/`bertt.CityJSON` plus the `MaxRev.Gdal.Core` OGR universal driver path) and the `geotiff` row the `geospatial-raster` codec `Semantics/geospatial#RASTER_INGEST` reads through `MaxRev.Gdal.Core`; the `usd`/`usdz` rows carry the `usd-stage` OpenUSD scene-graph codec (`UniversalSceneDescription`, Core Spec 1.0) as a peer coexistence axis — USD carries the scene through `UsdStage` layer composition and the `UsdGeomMesh`/`UsdGeomXformable`/`UsdShadeMaterialBindingAPI` typed schemas, IFC carries the BIM semantics, so the USD codec is a scene-graph peer, never a BIM-semantic replacement; media types are the IANA `model/gltf-binary`, `model/gltf+json`, `application/step`, and the buildingSMART `application/x-step`/`application/ifc+xml`/`application/ifc+json` values, plus the `model/vnd.usd`/`model/vnd.usdz+zip` USD, the `application/vnd.autodesk.fbx`/`model/vnd.collada+xml` scene-exchange, the `application/vnd.las` point-cloud, and the `application/vnd.shp`/`application/geopackage+sqlite3`/`application/geo+json`/`application/city+json`/`image/tiff;application=geotiff` geospatial media types; a codec admit is one row promotion, never a new importer family: a candidate format promotes in place by flipping its `CanImport`/`CanExport` columns and dropping the `InterchangeCodec.CataloguePackage` marker (or naming the admitted package on the codec row), the `import#IMPORT_RAIL` and `export#EXPORT_RAIL` folds gain one `InterchangeCodec`-keyed arm grounded against the named package decompile with zero new `BimIo`/`BimExport` entrypoint, and the managed-versus-companion split is read from the `TessellationRequiresCompanion` column rather than an `if (ifc)`/`if (step)` call-site branch — a managed codec carries `TessellationRequiresCompanion=false` and grounds its decode inline, a companion codec carries `true` and routes the geometry hop to `tessellation#TESSELLATION_BRIDGE`; a parallel `GltfImporter`/`StlImporter`/`StepImporter` service family, a per-format codec entrypoint, and a second BimIo fold are the named deleted forms, and a `CataloguePending` row that an admit promotes never grows a new surface beside the existing fold; the chunked simulation-field codec, the FastCDC structural geometry-delta codec, and the content-addressed artifact identity stay at `Rasm.Compute/Runtime/codecs` and are consumed at the seam, never re-minted here.

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
    public static readonly InterchangeCodec StepIso10303 = new("step-iso10303", managed: true, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec MeshText = new("mesh-text", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Ply = new("ply-net", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec SceneExchange = new("scene-exchange", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec PointCloud = new("point-cloud", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialVector = new("geospatial-vector", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialRaster = new("geospatial-raster", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec AcadSharp = new("acad-sharp", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec IgesAnsi = new("iges-ansi", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", managed: true, companion: false, cataloguePackage: Option<string>.None);

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
    public static readonly KhrExtension TextureTransform = new("KHR_texture_transform", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension NodeVisibility = new("KHR_node_visibility", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension AnimationPointer = new("KHR_animation_pointer", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension GaussianSplatting = new("KHR_gaussian_splatting", KhrSlot.Geometry, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension XmpJsonLd = new("KHR_xmp_json_ld", KhrSlot.Metadata, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsVariants = new("KHR_materials_variants", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsUnlit = new("KHR_materials_unlit", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsSpecular = new("KHR_materials_specular", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsIor = new("KHR_materials_ior", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsIridescence = new("KHR_materials_iridescence", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsSheen = new("KHR_materials_sheen", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsClearcoat = new("KHR_materials_clearcoat", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsTransmission = new("KHR_materials_transmission", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsVolume = new("KHR_materials_volume", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsAnisotropy = new("KHR_materials_anisotropy", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsDispersion = new("KHR_materials_dispersion", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsDiffuseTransmission = new("KHR_materials_diffuse_transmission", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension MaterialsEmissiveStrength = new("KHR_materials_emissive_strength", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);

    public KhrSlot Slot { get; }
    public KhrEncoder Encoder { get; }
    public Option<Action> Registrar { get; }

    public Fin<KhrExtension> Register() =>
        Registrar.Match(
            Some: register => Try.lift(() => { register(); return this; }).Run().MapFail(static error => new BimFault.ModelRejected(error.Message).ToError()),
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
    public static readonly InterchangeFormat Iges = new("iges", mediaType: "model/iges", extensions: Seq(".igs", ".iges"), canImport: true, canExport: false, codec: InterchangeCodec.IgesAnsi, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Stl = new("stl", mediaType: "model/stl", extensions: Seq(".stl"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat ThreeMf = new("3mf", mediaType: "model/3mf", extensions: Seq(".3mf"), canImport: true, canExport: true, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Obj = new("obj", mediaType: "model/obj", extensions: Seq(".obj"), canImport: true, canExport: true, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ply = new("ply", mediaType: "model/ply", extensions: Seq(".ply"), canImport: true, canExport: false, codec: InterchangeCodec.Ply, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Las = new("las", mediaType: "application/vnd.las", extensions: Seq(".las", ".laz"), canImport: true, canExport: true, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Shapefile = new("shp", mediaType: "application/vnd.shp", extensions: Seq(".shp"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoPackage = new("gpkg", mediaType: "application/geopackage+sqlite3", extensions: Seq(".gpkg"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoJson = new("geojson", mediaType: "application/geo+json", extensions: Seq(".geojson"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat CityJson = new("cityjson", mediaType: "application/city+json", extensions: Seq(".city.json", ".cityjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoTiff = new("geotiff", mediaType: "image/tiff;application=geotiff", extensions: Seq(".tif", ".tiff"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialRaster, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), canImport: true, canExport: true, codec: InterchangeCodec.AcadSharp, tessellationRequiresCompanion: false, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, upAxis: UpAxis.Z, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), canImport: true, canExport: true, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), canImport: true, canExport: true, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Fbx = new("fbx", mediaType: "application/vnd.autodesk.fbx", extensions: Seq(".fbx"), canImport: true, canExport: true, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Collada = new("dae", mediaType: "model/vnd.collada+xml", extensions: Seq(".dae"), canImport: true, canExport: true, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, upAxis: UpAxis.Y, handedness: Handedness.Right, stepProtocol: StepProtocol.None);

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
        : ByExtension.TryGetValue(ExtensionOf(pathOrMediaTypeOrKey), out var byExt) ? Fin.Succ(byExt)
        : Fin.Fail<InterchangeFormat>(new BimFault.CodecReject($"interchange-format-miss:{pathOrMediaTypeOrKey}").ToError());

    static string ExtensionOf(string input) =>
        Path.GetExtension(input) is { Length: > 0 } ext ? ext
        : input.StartsWith('.') && !input.Contains('/') ? input
        : "";
}

public enum UpAxis : byte { X = 0, Y = 1, Z = 2 }

public enum Handedness : byte { Right = 0, Left = 1 }

public enum StepProtocol : byte { None = 0, Ap203 = 203, Ap214 = 214, Ap242 = 242 }

public static partial class FrameNormalization {
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

## [03]-[RESEARCH]

- [GLTF_EXTENSIONS]: the ratified Khronos KHR/EXT extension rows ride the `KhrExtension` axis on the codec extension column carrying only their `KhrSlot`/`KhrEncoder` discriminant, never a named SharpGLTF Schema2 extension type — the catalogued `SharpGLTF.Core` assembly serializes every `KHR_materials_*` and texture extension `internal`, authored and read only through the public `Material`/`MaterialChannel`/`Texture`/`Node`/`ModelRoot` surface and never named directly, with `TextureTransform`/`XmpPackets`/`PunctualLight` the only public extension types; every in-box material/texture/scene/metadata row therefore carries `Registrar=None` because SharpGLTF auto-registers the in-box set, and the `Registrar` closure is reserved for a caller-supplied custom extension class implemented from `JsonSerializable` (the only `ExtensionsFactory.RegisterExtension<TParent, TExt>(name)` path), so a `RegisterExtension<Material, MaterialSpecular>(...)` over an internal type is the rejected form; the `KHR_draco_mesh_compression`/`KHR_meshopt_compression`/`KHR_mesh_quantization` rows carry a `KhrEncoder` discriminant and route the encode through `Openize.Drako` (`Draco.Encode(DracoPointCloud, DracoEncodeOptions)` over a `DracoMesh` built from `PointAttribute.Wrap`) and `Alimer.Bindings.MeshOptimizer` (the `unsafe static extern Meshopt.EncodeVertexBuffer`/`EncodeIndexBuffer` pinned-pointer surface over the interleaved vertex/index buffers), and route the symmetric decode through `Draco.Decode(byte[])` and the `Meshopt.DecodeVertexBuffer`/`DecodeIndexBuffer` pinned-pointer surface followed by the `DecodeFilterOct`/`DecodeFilterQuat`/`DecodeFilterExp`/`DecodeFilterColor` octahedral-normal/quaternion/exponent/color filter inverses keyed on the `EXT_meshopt_compression` bufferView `mode`/`filter` discriminant, all verified against those package surfaces; the glTF-to-Draco attribute mapping and the meshopt bufferView framing into and out of the `EXT_meshopt_compression` extension block ground at the codec admission gate.
- [USD_COEXISTENCE]: the `usd-stage` codec composes the `UniversalSceneDescription` managed OpenUSD binding (`.api/api-usd`, the SWIG-generated `pxr` namespace, `osx-arm64` native verified) — `UsdStage.Open`/`CreateNew`/`Export`/`Flatten` the codec root, `DefinePrim`/`Traverse`/`GetReferences`/`GetVariantSets` the namespace and composition, the `UsdGeomMesh.Define`+`GetPointsAttr`/`GetFaceVertexIndicesAttr` mesh schema crossing the `VtVec3fArray`/`VtIntArray` typed-array seam to the kernel mesh vocabulary, `UsdGeomXformable.AddXformOp` the transform stack the `FrameNormalization` Y-up→Z-up row coerces, and `UsdShadeMaterialBindingAPI.Bind` the shade network mapping onto `Semantics/appearance#BIM_APPEARANCE` — USD carries the scene as a host-neutral peer coexistence axis and the GeometryGym IFC graph carries the BIM semantics, so deriving `BimElement`/`IfcClass` from USD prim type names is the named boundary violation; the `SWIGTYPE_p_*`/`*PINVOKE` interop types never enter canonical owners and a stage op with no matching RID native payload faults `BimFault.CapabilityMiss`.
- [SCENE_EXCHANGE_AND_GEOSPATIAL]: the `scene-exchange` codec composes `AssimpNetter` (`.api/api-assimpnetter`, the `AssimpContext`/`PostProcessSteps`/`Scene`/`Mesh`/`Material` surface, an `osx-arm64` `libassimp.dylib`) owning FBX+Collada import/export and the standalone 3MF read leg that retires the BCL `ThreeMfReader`; the `ply-net` codec composes `Ply.Net` (`.api/api-ply-net`, `PlyParser.Parse`/`ParseHeader` over the `Header`/`Dataset`/`PropertyData` record graph) retiring the BCL `PlyReader`; the `point-cloud` codec composes `Themis.Las` (`.api/api-themis-las`, `LasReader`/`LasPoint` with `MathNet.Numerics.LinearAlgebra.Vector<double>` positions) plus the `Unofficial.laszip.netstandard` LASzip decompression front for the compressed `.laz` extension, the `reconstruct#RECONSTRUCTION` ingest front driving both; the `geospatial-vector` codec composes `NetTopologySuite.IO.Esri.Shapefile`/`bertt.CityJSON`/`MaxRev.Gdal.Core` OGR and the `geospatial-raster` codec composes `MaxRev.Gdal.Core` raster — both decode through the `Semantics/geospatial#VECTOR_INGEST`/`#RASTER_INGEST` owner producing `GeoFeature` rows over the `NetTopologySuite` planar algebra, so a codec admit reaching this axis is one `InterchangeCodec` row promotion and never a parallel importer family.
