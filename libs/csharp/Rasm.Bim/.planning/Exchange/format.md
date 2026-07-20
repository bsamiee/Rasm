# [BIM_FORMAT_AXIS]

`InterchangeFormat` owns the interchange-format vocabulary: one `[SmartEnum<string>]` table discriminating import (foreign bytes to a BIM semantic graph or geometry) from export (an artifact to foreign bytes), joined by the `InterchangeCodec`/`KhrExtension` codec-and-extension axes and the per-importer `FrameNormalization` coercing every imported coordinate onto the canonical kernel frame. Format selection is HOST-LOCAL row data.

`import#IMPORT_RAIL` ingest and `export#EXPORT_RAIL` emit read these rows to dispatch a codec without a call-site branch. Kernel `Rasm` geometry composes as settled vocabulary.

## [01]-[INDEX]

- [01]-[FORMAT_AXIS]: format/codec/extension rows; capability, companion, and `Frame` basis-change columns.

## [02]-[FORMAT_AXIS]

- Owner: `InterchangeFormat` the format vocabulary keyed by media-type, extension, and key; `InterchangeCodec` the codec-owner vocabulary discriminating the managed package or companion reading and writing a row; `KhrExtension` the glTF extension axis on its `KhrSlot`/`KhrEncoder` discriminant; `BasisChange` the per-importer signed-permutation basis carrying positions and normals alike onto the canonical kernel frame; `FrameNormalization` the static surface coercing every imported coordinate onto that frame.
- Auto: `Detect` resolves one row from a key, media type, path, bare dotted extension, or compound suffix with zero call-site branching; `Companion` folds the format flag and the codec flag into the one predicate the import fold reads; `FrameNormalization.Canonicalize` applies a row's `BasisChange` to a position or a normal buffer alike, and `FlipsWinding` reports the mirror case (negative determinant) driving the import fold's triangle-order reversal rather than the kernel negating one component unrewound.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, dotbim, Themis.Las, UniversalSceneDescription, NetTopologySuite, NetTopologySuite.IO.Esri.Shapefile, NetTopologySuite.IO.VectorTiles, NetTopologySuite.IO.VectorTiles.Mapbox, SharpKml.Core, bertt.CityJSON, FlatGeobuf, GISBlox.IO.GeoParquet, MaxRev.Gdal.Core, ACadSharp, HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row (media-type, extensions, capability columns, codec owner, `TessellationRequiresCompanion`, `Frame`, `StepProtocol`); a new IFC serialization GeometryGym emits is the `Serialization` value on one GeometryGym-codec row, never a call-site format ladder; a new managed codec is one `InterchangeCodec` row; a new glTF capability is one `KhrExtension` row on its `KhrSlot`/`KhrEncoder` discriminant, its `Registrar` closure set only for a caller-supplied non-in-box `JsonSerializable`; a new ingest frame is one `BasisChange` static row, never a per-axis branch; a not-yet-decompiled format admits as a `CataloguePending` candidate row naming its package (`ifc5`/`Ifc5Pending` the standing example), promoted in place when the catalogue lands.

Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form. `CanImport`/`CanExport` fault a write-only or read-only direction at the boundary rather than mid-codec, and every `true` column names its realizing arm: mesh/scene emit through the `export#EXPORT_RAIL` `BimExport.Export` codec `Switch`, IFC emit through `ExportIfc`→`Projection/egress#IFC_EGRESS` `SemanticProjector.Emit`, geospatial-vector read/write through the `Semantics/geospatial#VECTOR_INGEST` `GeoVector.Read`/`GeoVector.Write` fold, raster read through `#RASTER_INGEST`. A capability flag with no realizing arm is the rejected PHANTOM — a flag flips WITH its arm, never before, each row carrying its own phantom reason on its sub-bullet below, and IFC and glTF rows stay bidirectional because GeometryGym and SharpGLTF are symmetric over their serializations. `TessellationRequiresCompanion` is `true` exactly on the IFC/STEP/native rows because GeometryGym carries no tessellation kernel — a managed IFC geometry evaluation is the rejected form — and the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns.
- [STEP_PROTOCOLS]: `StepProtocol` disambiguates AP203 (config-controlled 3D design), AP214 (automotive core), and AP242 (model-based 3D engineering, the merged successor) sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` set, `StepProtocol.None` on every non-STEP row keeping the column total. All three carry `CanExport=false` — no managed STEP writer is admitted, the in-process BCL-only `StepReader` being an import-only Part-21 entity-instance reader and a re-author the rejected form (`IxMilia.Step`/`StepFileParser` absent from NuGet, `STPLoader` net35/`AForge`-coupled, `DevelApp.StepParser` a regex grammar engine, a native OpenCASCADE reader in-process breaking the firebreak). `step-iso10303` drops its `CataloguePackage` marker (`Managed=true`) for the admitted in-process semantic-graph leg yet keeps `Companion=true` because the B-rep/NURBS geometry leg routes to the Compute companion; `BimIo.ImportStep` lands product structure in-process and the geometry hop crosses `tessellation#TESSELLATION_BRIDGE` as the IFC request does.
- [IGES_DISTINCT]: IGES is not an ISO 10303 protocol — its ANSI section-based grammar shares neither the STEP physical-file token grammar nor the GeometryGym entity surface, so the `iges` row carries the distinct `iges-ansi` companion codec and routes its B-rep/NURBS evaluation through the Compute companion (no managed IGES reader admitted); an `iges` row on `step-iso10303` hands `StepReader` a `StepProtocol.None` and a grammar it cannot parse, the deleted form.
- [GLTF_EXTENSION_ROWS]: `KhrExtension` carries each glTF extension SharpGLTF.Core serializes in-box as one row on its `KhrSlot`/`KhrEncoder` discriminant. In-box geometry/material/texture/scene/metadata rows carry `Registrar=None` because SharpGLTF.Core auto-registers them and authors them only through the public `Material`/`MaterialChannel`/`Texture`/`Node`/`MeshGpuInstancing`/`ModelRoot` surface — the `KHR_materials_*` and texture classes are `internal` (only `TextureTransform`/`XmpPackets`/`PunctualLight`/`MeshGpuInstancing` public), so an `ExtensionsFactory.RegisterExtension<Material, MaterialSpecular>(...)` over an internal type is the rejected form, and a `Registrar=None` row for an absent extension (`KHR_gaussian_splatting`/`KHR_materials_variants`/`KHR_mesh_quantization`) is the rejected phantom. `Registrar` is reserved for a caller-supplied custom extension from `JsonSerializable`, never an in-box type.
- [COMPRESSION_ROWS]: `Openize.Drako` (`KHR_draco_mesh_compression`) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`) own encode and decode on the `KhrEncoder` discriminant because SharpGLTF.Core ships no compression codec — the `export#EXPORT_RAIL` `GlbBytes` switch drives the encode half and the `import#IMPORT_RAIL` `Decompress` pre-decode branch the symmetric decode half, so both rows are bidirectional and a per-extension importer/exporter type is the deleted form.
- [MESH_TEXT_ROWS]: `geometry3Sharp` (pure-managed netstandard2.0, ALC-safe) grounds the `Stl`/`Obj`/`Off` decode, import-only (`CanExport=false`) because the mesh egress is the GLB rail, not `geometry3Sharp`'s writer; `geometry3Sharp` ships no PLY or 3MF reader, so `PLY` is the dedicated `ply-net` codec naming `Ply.Net` (`PlyParser.Parse` over the immutable `Header`/`Dataset`/`PropertyData` graph) retiring the BCL `PlyReader`, and 3MF moves to `scene-exchange`.
- [SCENE_EXCHANGE_ROWS]: `AssimpNetter` (shipping its own osx-arm64 `libassimp.dylib`, RID-coupled but the one admitted owner) covers the formats no other Bim codec owns — FBX, Collada, and the standalone 3MF read leg — through one disposable `AssimpContext` and its `PostProcessSteps` transform algebra; `Fbx`/`Collada` are import-and-export (`CanImport=true`/`CanExport=true`) and `ThreeMf` import-only because the Assimp 3MF leg reads but does not write, faulting the export fold at the boundary. A scene-exchange row's KEY doubles as the AssimpNetter `exportFormatId` (`fbx`/`collada`/`3mf`), pinning the Collada row key to `collada` — a `dae` key handed to `ExportToBlob` misses the export matrix, the `.dae` extension alone owning `Detect` — and the export arm guards through `IsExportFormatSupported`. `lib3mf` (native C++) and `Aspose.3D` (closed) are rejected, a second managed FBX/Collada lib beside `AssimpNetter` the rejected form retiring the former `Ufbx`/`Collada141` candidates; a per-format `StlImporter`/`PlyImporter`/`FbxImporter` family is the deleted form.
- [ACAD_CODEC]: `ACadSharp` (pure-managed AnyCPU IL, osx-arm64-safe) is the in-process DWG+DXF reader, so the `Dwg` row routes `.dwg`/`.dxf` to the managed `acad-sharp` codec (`CanImport=true`/`CanExport=false` — read-only ingress, host-bound DWG/DXF write staying Rhino-native, `TessellationRequiresCompanion=false`) rather than the `native-companion` two-hop; the `import#IMPORT_RAIL` `BimIo` decode arm folds the mesh-bearing `MESH`/`3DFACE`/`POLYFACE_MESH`/`POLYGON_MESH`/`INSERT` entities onto the `ImportedGeometry` triangle soup (the `LINE`/`LWPOLYLINE`/`CIRCLE`/`ARC` 2D profiles being `csharp:Rasm.Fabrication`'s `Loop` concern, never this arm), promoting by row data not an `if(dwg)` branch. `netDxf` is not admitted because `ACadSharp` supersedes it over both formats, a second managed CAD lib the rejected form, and the native-coupled `Aspose.CAD`/`Teigha`/`ODA` readers break the ALC firebreak.
- [DOTBIM_CODEC]: `dotbim` (pure-managed netstandard2.0, STJ wire, zero native payload) is the only admitted codec whose WIRE expresses instancing — `File` owns a shared `Mesh` pool with placed `Element` instances referencing a pool `MeshId` by a rigid `Vector`+quaternion `Rotation`, a validated `Guid`, a `Type`, an RGBA `Color`, and a `string`→`string` `Info` bag, so N repeated objects serialize as N placements over ONE mesh. Bidirectional: import resolves each `Element.MeshId` against the pool and bakes the placement transform; export pools distinct geometry by content key, decomposes onto `Vector`/`Rotation`, stamps the seam GlobalId onto `Element.Guid` and the classification onto `Type`, and round-trips IFC tags through `Info`. `File.Read`/`File.Save` are PATH-BOUND (`.bim`-enforced, no stream overload), so the byte arms cross a temp path as the `usd-stage` codec does, and the typed setters validate at assignment (`Color` 0..255, `MeshId >= 0`, malformed `Guid`) so an invalid model faults at build before `Save`.
- [POINT_CLOUD_AND_GEOSPATIAL]: `Themis.Las` is the `point-cloud` codec — the ASPRS LAS reader the `reconstruct#RECONSTRUCTION` scan-to-BIM front decodes through, `Unofficial.laszip.netstandard` composing in for the compressed `.laz` so one ingest path reads `.las` and `.laz`. That row stays import-only because no rail composes the catalogued `LasWriter` yet, a point-cloud egress being `reconstruct#RECONSTRUCTION` growth over its point carrier, never a mesh-rail LAS dump. `shp`/`gpkg`/`geojson`/`cityjson`/`fgb`/`geoparquet`/`kml`/`kmz`/`mvt` are the `geospatial-vector` codec the `Semantics/geospatial#VECTOR_INGEST` owner decodes and `GeoVector.Write` re-emits over `NetTopologySuite.IO.Esri.Shapefile`/`bertt.CityJSON`/`FlatGeobuf`/`GISBlox.IO.GeoParquet`, the `MaxRev.Gdal.Core` OGR path with `SharpKml.Core` the admitted managed `.kml`/`.kmz` upgrade, and `NetTopologySuite.IO.VectorTiles.Mapbox` over `.mvt`/`.pbf`; `cityjson` is import-only because the planar `GeoFeature` egress cannot re-emit the 3D city hierarchy while `fgb`/`geoparquet` round-trip. `MaxRev.Gdal.Core` raster is the `geospatial-raster` codec `Semantics/geospatial#RASTER_INGEST` reads, import-only until a raster egress arm composes the GDAL `CreateCopy` write.
- [USD_PEER]: `UsdStage` layer composition carries the USD scene through the `UsdGeomMesh`/`UsdGeomXformable`/`UsdShadeMaterialBindingAPI` schemas while GeometryGym carries the BIM semantics, so the `usd-stage` codec is a scene-graph peer, never a BIM-semantic replacement — deriving `BimElement`/`IfcClass` from USD prim type names is the named boundary violation, the `SWIGTYPE_p_*`/`*PINVOKE` interop types never entering canonical owners and a stage op with no matching RID native payload faulting `BimFault.CapabilityMiss`. `usdz` carries `CanExport=false` because the `UniversalSceneDescription` binding ships `UsdStage.Export` over `.usd`/`.usda`/`.usdc` and no `.usdz` packaging member, so a `CreateNew("*.usdz")` author is the rejected phantom while `.usdz` READS through the package layer. `Frame` stays the Y-up default a metadata-less stage falls to, USD's `upAxis` being per-stage metadata (`UsdGeom.UsdGeomGetStageUpAxis`), so the import `Usd` arm selects the basis per stage and a Z-up CAD/BIM export lands canonical with no row edit.
- [ENERGY_MODEL_ROWS]: `HoneybeeSchema` (HBJSON), `DragonflySchema` (DFJSON), and `NREL.OpenStudio.macOS-arm64` (OSM/gbXML/IDF) are the `energy-model` codec whose realizing arms live on the `Energy/` folder — `CanImport` is the `Energy/projector#ENERGY_PROJECTOR` raise, the hbjson/dfjson `CanExport` the `Energy/derive#MODEL_DERIVE` BIM-to-BEM lower, and the osm/gbxml/idf emit rides `Energy/derive#TRANSLATE_MATRIX` over an OSM-family SOURCE, never the graph, so those rows carry `CanExport=false` per the realizing-arm law. These rows never enter the `BimIo` mesh/scene fold (a format row's arm may live on any rail), `TessellationRequiresCompanion=false` because an energy model carries no BRep, and the frames are Identity.
- [ROW_PROMOTION]: a codec admit is one row promotion — a candidate flips its `CanImport`/`CanExport` and drops the `CataloguePackage` marker, the `import#IMPORT_RAIL`/`export#EXPORT_RAIL` folds gain one `InterchangeCodec`-keyed arm grounded against the named package with zero new `BimIo`/`BimExport` entrypoint, and the managed-versus-companion split reads from `TessellationRequiresCompanion` (managed grounds its decode inline, companion routes the geometry hop to `tessellation#TESSELLATION_BRIDGE`), never an `if(ifc)`/`if(step)` branch. A parallel `GltfImporter`/`StlImporter`/`StepImporter` family, a per-format codec entrypoint, and a second `BimIo` fold are the deleted forms. Chunked simulation-field, FastCDC geometry-delta, and content-addressed artifact codecs stay at `Rasm.Compute/Runtime/codecs`, consumed at the seam, never re-minted here.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class InterchangeCodec {
    public static readonly InterchangeCodec SharpGltf = new("sharp-gltf", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeometryGym = new("geometry-gym", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec StepIso10303 = new("step-iso10303", managed: true, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec MeshText = new("mesh-text", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Ply = new("ply-net", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec DotBim = new("dotbim", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec SceneExchange = new("scene-exchange", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec PointCloud = new("point-cloud", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialVector = new("geospatial-vector", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialRaster = new("geospatial-raster", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec AcadSharp = new("acad-sharp", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec IgesAnsi = new("iges-ansi", managed: false, companion: true, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Saf = new("saf-xlsx", managed: true, companion: false, cataloguePackage: "StructuralAnalysisFormat admitted; the ExcelModel<->seam-payload arms land as the row promotion");
    public static readonly InterchangeCodec CobieXlsx = new("cobie-xlsx", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec EnergyModel = new("energy-model", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Ifc5Pending = new("ifc5-ecs", managed: false, companion: false, cataloguePackage: "no admitted IFC5/IFCX toolkit; GeometryGym is IFC2x3-IFC4.x only");

    public bool Managed { get; }
    public bool Companion { get; }
    public Option<string> CataloguePackage { get; }

    public bool CataloguePending => CataloguePackage.IsSome;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KhrExtension {
    public static readonly KhrExtension DracoMeshCompression = new("KHR_draco_mesh_compression", KhrSlot.Compression, encoder: KhrEncoder.Draco, registrar: Option<Action>.None);
    public static readonly KhrExtension MeshoptCompression = new("KHR_meshopt_compression", KhrSlot.Compression, encoder: KhrEncoder.Meshopt, registrar: Option<Action>.None);
    public static readonly KhrExtension MeshGpuInstancing = new("EXT_mesh_gpu_instancing", KhrSlot.Geometry, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureTransform = new("KHR_texture_transform", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureWebp = new("EXT_texture_webp", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension TextureDds = new("MSFT_texture_dds", KhrSlot.Texture, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension NodeVisibility = new("KHR_node_visibility", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension AnimationPointer = new("KHR_animation_pointer", KhrSlot.Scene, encoder: KhrEncoder.None, registrar: Option<Action>.None);
    public static readonly KhrExtension XmpJsonLd = new("KHR_xmp_json_ld", KhrSlot.Metadata, encoder: KhrEncoder.None, registrar: Option<Action>.None);
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
    public static readonly KhrExtension MaterialsPbrSpecularGlossiness = new("KHR_materials_pbrSpecularGlossiness", KhrSlot.Material, encoder: KhrEncoder.None, registrar: Option<Action>.None);

    public KhrSlot Slot { get; }
    public KhrEncoder Encoder { get; }
    public Option<Action> Registrar { get; }

    public Fin<KhrExtension> Register(Op key) =>
        Registrar.Match(
            Some: register => Try.lift(() => { register(); return this; }).Run().MapFail(error => new BimFault.ModelRejected(key, error.Message)),
            None: () => Fin.Succ(this));
}

public enum KhrSlot : byte { Compression = 0, Geometry = 1, Texture = 2, Scene = 3, Material = 4, Metadata = 5 }

public enum KhrEncoder : byte { None = 0, Draco = 1, Meshopt = 2 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: FormatIfcSerialization.STEP);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: FormatIfcSerialization.XML);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: FormatIfcSerialization.JSON);
    public static readonly InterchangeFormat StepAp203 = new("step-ap203", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap203);
    public static readonly InterchangeFormat StepAp214 = new("step-ap214", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap214);
    public static readonly InterchangeFormat StepAp242 = new("step-ap242", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap242);
    public static readonly InterchangeFormat Iges = new("iges", mediaType: "model/iges", extensions: Seq(".igs", ".iges"), canImport: true, canExport: false, codec: InterchangeCodec.IgesAnsi, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Stl = new("stl", mediaType: "model/stl", extensions: Seq(".stl"), canImport: true, canExport: false, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat ThreeMf = new("3mf", mediaType: "model/3mf", extensions: Seq(".3mf"), canImport: true, canExport: false, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Obj = new("obj", mediaType: "model/obj", extensions: Seq(".obj"), canImport: true, canExport: false, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Off = new("off", mediaType: "model/off", extensions: Seq(".off"), canImport: true, canExport: false, codec: InterchangeCodec.MeshText, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ply = new("ply", mediaType: "model/ply", extensions: Seq(".ply"), canImport: true, canExport: false, codec: InterchangeCodec.Ply, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat DotBim = new("bim", mediaType: "application/vnd.dotbim+json", extensions: Seq(".bim"), canImport: true, canExport: true, codec: InterchangeCodec.DotBim, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Las = new("las", mediaType: "application/vnd.las", extensions: Seq(".las", ".laz"), canImport: true, canExport: false, codec: InterchangeCodec.PointCloud, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Shapefile = new("shp", mediaType: "application/vnd.shp", extensions: Seq(".shp"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoPackage = new("gpkg", mediaType: "application/geopackage+sqlite3", extensions: Seq(".gpkg"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoJson = new("geojson", mediaType: "application/geo+json", extensions: Seq(".geojson"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat CityJson = new("cityjson", mediaType: "application/city+json", extensions: Seq(".city.json", ".cityjson"), canImport: true, canExport: false, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat FlatGeobuf = new("fgb", mediaType: "application/vnd.flatgeobuf", extensions: Seq(".fgb"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoParquet = new("geoparquet", mediaType: "application/vnd.apache.parquet", extensions: Seq(".parquet"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Kml = new("kml", mediaType: "application/vnd.google-earth.kml+xml", extensions: Seq(".kml"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Kmz = new("kmz", mediaType: "application/vnd.google-earth.kmz", extensions: Seq(".kmz"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Mvt = new("mvt", mediaType: "application/vnd.mapbox-vector-tile", extensions: Seq(".mvt", ".pbf"), canImport: true, canExport: true, codec: InterchangeCodec.GeospatialVector, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoTiff = new("geotiff", mediaType: "image/tiff;application=geotiff", extensions: Seq(".tif", ".tiff"), canImport: true, canExport: false, codec: InterchangeCodec.GeospatialRaster, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), canImport: true, canExport: false, codec: InterchangeCodec.NativeCompanion, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), canImport: true, canExport: false, codec: InterchangeCodec.AcadSharp, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), canImport: false, canExport: false, codec: InterchangeCodec.Ifc5Pending, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    // SAF structural-analysis XLSX: the admitted StructuralAnalysisFormat package's home row — capability columns
    // flip WITH the ExcelModel<->seam-payload arms (Model/structural#STRUCTURAL_PROJECTION payloads), never before.
    public static readonly InterchangeFormat Saf = new("saf", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".saf.xlsx"), canImport: false, canExport: false, codec: InterchangeCodec.Saf, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    // COBie FM-handover XLSX — WRITE-ONLY, GRAPH-SOURCED: the export#COBIE_EMIT CobieEmit.Export author (the
    // ExportPayload codec Switch routes it there; a COBie spreadsheet is never a geometry import source).
    public static readonly InterchangeFormat Cobie = new("cobie", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".cobie.xlsx"), canImport: false, canExport: true, codec: InterchangeCodec.CobieXlsx, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    // Energy-model rows (the Energy/ folder the realizing owner): HBJSON/DFJSON raise+lower, OSM/gbXML/IDF raise-only
    // against the graph — their emit rides the energy Translate matrix over an OSM-family SOURCE, so CanExport stays
    // false until the graph-egress arm lands (the flag flips WITH the arm). Ladybug/OSM/gbXML are Z-up: Identity frame.
    public static readonly InterchangeFormat Hbjson = new("hbjson", mediaType: "application/vnd.ladybug.hbjson+json", extensions: Seq(".hbjson"), canImport: true, canExport: true, codec: InterchangeCodec.EnergyModel, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dfjson = new("dfjson", mediaType: "application/vnd.ladybug.dfjson+json", extensions: Seq(".dfjson"), canImport: true, canExport: true, codec: InterchangeCodec.EnergyModel, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Osm = new("osm", mediaType: "application/vnd.openstudio.osm", extensions: Seq(".osm"), canImport: true, canExport: false, codec: InterchangeCodec.EnergyModel, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GbXml = new("gbxml", mediaType: "application/vnd.gbxml+xml", extensions: Seq(".gbxml"), canImport: true, canExport: false, codec: InterchangeCodec.EnergyModel, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Idf = new("idf", mediaType: "application/vnd.energyplus.idf", extensions: Seq(".idf"), canImport: true, canExport: false, codec: InterchangeCodec.EnergyModel, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), canImport: true, canExport: true, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), canImport: true, canExport: false, codec: InterchangeCodec.UsdStage, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Fbx = new("fbx", mediaType: "application/vnd.autodesk.fbx", extensions: Seq(".fbx"), canImport: true, canExport: true, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Collada = new("collada", mediaType: "model/vnd.collada+xml", extensions: Seq(".dae"), canImport: true, canExport: true, codec: InterchangeCodec.SceneExchange, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);

    private readonly Seq<string> extensions;

    public string MediaType { get; }
    public bool CanImport { get; }
    public bool CanExport { get; }
    public InterchangeCodec Codec { get; }
    public bool TessellationRequiresCompanion { get; }
    public BasisChange Frame { get; }
    public StepProtocol StepProtocol { get; }

    // Some exactly on the GeometryGym rows; the ctor's nullable `FormatIfcSerialization? serialization = null`
    // arg lifts once to Option, so non-IFC rows omit the column. ExportIfc and the wire Seal read THIS row
    // value — a call-site InterchangeFormat==/ternary serialization ladder is the deleted form.
    public Option<FormatIfcSerialization> Serialization { get; }

    public Seq<string> Extensions => extensions;

    public bool CataloguePending => Codec.CataloguePending && !CanImport && !CanExport;

    // Geometry leg crosses the companion bridge when EITHER the format declares it (IFC/STEP/IGES/native)
    // OR the codec's own geometry read is companion-bound; the import fold reads this one predicate, not two.
    public bool Companion => TessellationRequiresCompanion || Codec.Companion;

    public bool IsCanonicalFrame => Frame.IsIdentity;

    static readonly FrozenDictionary<string, InterchangeFormat> ByExtension =
        Items.SelectMany(static row => row.extensions.Map(ext => (ext, row)))
            .GroupBy(static pair => pair.ext, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static pair => (int)pair.row.StepProtocol).row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByMediaType =
        Items.GroupBy(static row => row.MediaType, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static row => (int)row.StepProtocol)!, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByKey =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.OrdinalIgnoreCase);

    public static Fin<InterchangeFormat> Detect(string pathOrMediaTypeOrKey, Op key) =>
        ByKey.TryGetValue(pathOrMediaTypeOrKey, out var byKey) ? Fin.Succ(byKey)
        : ByMediaType.TryGetValue(pathOrMediaTypeOrKey, out var byType) ? Fin.Succ(byType)
        : ByExtension.TryGetValue(ExtensionOf(pathOrMediaTypeOrKey), out var byExt) ? Fin.Succ(byExt)
        : CompoundSuffix(pathOrMediaTypeOrKey).Match(
            Some: Fin.Succ,
            None: () => Fin.Fail<InterchangeFormat>(new BimFault.CodecReject(key, $"interchange-format-miss:{pathOrMediaTypeOrKey}")));

    static string ExtensionOf(string input) =>
        Path.GetExtension(input) is { Length: > 0 } ext ? ext
        : input.StartsWith('.') && !input.Contains('/') ? input
        : "";

    // A compound extension (e.g. ".city.json") that Path.GetExtension cannot return as one token: the longest
    // registered extension the lowercased path ends with wins, so a ".city.json" path resolves where the bare
    // ".json" leg misses. Media-type and key inputs (which carry '/' or no leading dot) never reach this fold.
    static Option<InterchangeFormat> CompoundSuffix(string input) =>
        input.Contains('/')
            ? Option<InterchangeFormat>.None
            : ByExtension
                .Where(pair => input.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(static pair => pair.Key.Length)
                .Select(static pair => pair.Value)
                .ToSeq().Head;
}

// Basis change onto the canonical kernel frame (Z-up, right-handed) is a signed axis permutation: each
// canonical component names its signed source axis (+-1->source X, +-2->source Y, +-3->source Z). A signed
// permutation is orthogonal, so the one map carries positions AND unit normals; the determinant sign decides
// triangle-winding reversal. A new ingest frame is one static row, never a per-axis branch in the kernel; the
// retired UpAxis/Handedness enum pair (whose Left/X-up values no row exercised and whose handedness flip negated
// one axis without reversing winding) collapses into this data row per DERIVED_LOGIC.
public readonly record struct BasisChange(sbyte CanonicalX, sbyte CanonicalY, sbyte CanonicalZ) {
    public static readonly BasisChange Identity = new(1, 2, 3);
    public static readonly BasisChange YUpToCanonical = new(1, -3, 2);

    public bool IsIdentity => this == Identity;
    public bool FlipsWinding => Determinant < 0;

    public (float X, float Y, float Z) Apply(float x, float y, float z) {
        ReadOnlySpan<float> v = [x, y, z];
        return (Source(v, CanonicalX), Source(v, CanonicalY), Source(v, CanonicalZ));

        static float Source(ReadOnlySpan<float> axes, sbyte signedAxis) =>
            signedAxis < 0 ? -axes[-signedAxis - 1] : axes[signedAxis - 1];
    }

    int Determinant {
        get {
            ReadOnlySpan<sbyte> axes = [CanonicalX, CanonicalY, CanonicalZ];
            int parity = 1, signs = 1;
            for (int i = 0; i < axes.Length; i++) {
                signs *= Math.Sign((int)axes[i]);
                for (int j = i + 1; j < axes.Length; j++) {
                    if (Math.Abs((int)axes[i]) > Math.Abs((int)axes[j])) { parity = -parity; }
                }
            }
            return parity * signs;
        }
    }
}

public enum StepProtocol : byte { None = 0, Ap203 = 203, Ap214 = 214, Ap242 = 242 }

public static partial class FrameNormalization {
    // Span kernel (the platform-forced exemption): applies the row's basis change to the leading vec3 of each
    // stride-spaced element in place — a position buffer and a normal buffer are each canonicalized by a SEPARATE
    // call over their own strided view (the one orthogonal signed permutation carries both), so a fully interleaved
    // pos+normal buffer in one call coerces only the positions; FlipsWinding tells the import fold to reverse
    // triangle order on a mirror (det < 0), never silently rewriting the index buffer here.
    public static void Canonicalize(InterchangeFormat format, Span<float> components, int stride) {
        var basis = format.Frame;
        if (basis.IsIdentity) {
            return;
        }
        for (int offset = 0; offset + 2 < components.Length; offset += stride) {
            (components[offset], components[offset + 1], components[offset + 2]) =
                basis.Apply(components[offset], components[offset + 1], components[offset + 2]);
        }
    }

    public static bool FlipsWinding(InterchangeFormat format) => format.Frame.FlipsWinding;
}
```

## [03]-[RESEARCH]

(none)
