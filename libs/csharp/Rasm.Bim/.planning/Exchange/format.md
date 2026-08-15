# [BIM_FORMAT_AXIS]

`InterchangeFormat` owns the interchange-format vocabulary: one `[SmartEnum<string>]` table discriminating import (foreign bytes to a BIM semantic graph or geometry) from export (an artifact to foreign bytes), joined by the `InterchangeCodec`/`KhrExtension` codec-and-extension axes and the per-importer `FrameNormalization` coercing every imported coordinate onto the canonical kernel frame. Format selection is HOST-LOCAL row data.

`import#IMPORT_RAIL` ingest and `export#EXPORT_RAIL` emit read these rows to dispatch a codec without a call-site branch. Kernel `Rasm` geometry composes as settled vocabulary.

## [01]-[INDEX]

- [02]-[FORMAT_AXIS]: format/codec/extension rows; capability, write-capability, detect-precedence, companion, and `Frame` basis-change columns.

## [02]-[FORMAT_AXIS]

- Owner: `InterchangeFormat` the format vocabulary keyed by media-type, extension, and key, its `DetectRank` column the declared precedence among rows sharing one of those keys and its `RoundTrippable` derivation the both-directions predicate the wire narrows on; `InterchangeDirection` the two-value axis the ONE `Admitted` capability gate keys on; `InterchangeCodec` the codec-owner vocabulary discriminating the managed package or companion reading and writing a row; `KhrExtension` the glTF extension axis on its `KhrSlot`/`KhrEncoder`/`Writable` discriminants; `BasisChange` the per-importer signed-permutation basis carrying positions and normals alike onto the canonical kernel frame; `FrameNormalization` the static surface coercing every imported coordinate onto that frame.
- Auto: `Detect` resolves one row from a key, media type, path, bare dotted extension, or compound suffix with zero call-site branching, a contended media type or extension resolving on the `DetectRank` column both frozen indexes read; `Admitted` answers the whole capability question for one direction — the catalogue-pending state, the companion-bound degradation, and the plain read-only or write-only refusal, each its own `Model/faults#DETAIL_ROSTER` row read off the row's own columns; `Companion` folds the format flag and the codec flag into the one predicate the import fold reads; `FrameNormalization.Canonicalize` applies a row's `BasisChange` to a position or a normal buffer alike, and `FlipsWinding` reports the mirror case (negative determinant) driving the import fold's triangle-order reversal rather than the kernel negating one component unrewound.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, dotbim, Themis.Las, UniversalSceneDescription, NetTopologySuite, NetTopologySuite.IO.Esri.Shapefile, NetTopologySuite.IO.VectorTiles, NetTopologySuite.IO.VectorTiles.Mapbox, SharpKml.Core, bertt.CityJSON, FlatGeobuf, GISBlox.IO.GeoParquet, MaxRev.Gdal.Core, ACadSharp, HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row (media-type, extensions, capability columns, codec owner, `TessellationRequiresCompanion`, `Frame`, `StepProtocol`, and `DetectRank` only where it contends for a key another row already holds); a new IFC wire form GeometryGym emits — a serialization, a container over an existing one, or both — is one `Projection/egress#IFC_EGRESS` `IfcWireForm` row named as the `Serialization` value on one GeometryGym-codec row, carrying its own `Seal` and `Admit` delegates, never a call-site format ladder and never a container column beside it; a new managed codec is one `InterchangeCodec` row; a new glTF capability is one `KhrExtension` row on its `KhrSlot`/`KhrEncoder`/`Writable` discriminants, an import-only extension entering `Writable=false` rather than staying off the roster; a new ingest frame is one `BasisChange` static row, never a per-axis branch; a not-yet-decompiled format admits as a `CataloguePending` candidate row naming its package, promoted in place when the catalogue lands.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch — a parallel `GltfImporter`/`IfcImporter`/`GltfExporter` family is the deleted form. Capability is ONE gate: `Admitted(format, direction, key)` reads the catalogue-pending state, the companion binding, and the direction column off the row, so `BimIo.ImportGeometry`/`ImportIfc`/`ImportStep` and `BimExport.Export`/`ExportIfc` each compose it once and no entrypoint re-spells a pending-then-capability ladder whose order decided which message an operator read. Each capability column — a format row's `CanImport`/`CanExport`, a `KhrExtension` row's `Writable` — flips WITH its realizing arm and never before, so a `true` with no arm is the rejected PHANTOM and a `false` is the row's declared one-way vocabulary, each row carrying its own reason on its sub-bullet below; `RoundTrippable` is the derived both-directions predicate the `wire#WIRE_PROJECTION` seal and negotiation narrow on, so a row that reads but cannot write is unreachable to a wire that must round-trip. Every `true` names its realizing arm: mesh/scene emit through the `export#EXPORT_RAIL` `BimExport.Export` codec `Switch`, IFC emit and admission through the `Projection/egress#IFC_EGRESS` `IfcWireForm` row's own `Seal`/`Admit` pair, geospatial-vector read/write through the `Semantics/geospatial#VECTOR_INGEST` `GeoVector.Read`/`GeoVector.Write` fold, raster read through `#RASTER_INGEST`, SAF structural read through the `Model/structural#STRUCTURAL_PROJECTION` `Saf`+`Author` GeometryGym-authoring leg the one `SemanticProjector` ingests and SAF write through its `Workbook`+`Saf` lowering. `TessellationRequiresCompanion` is `true` exactly on the IFC/STEP/native rows because GeometryGym carries no tessellation kernel — a managed IFC geometry evaluation is the rejected form — and the codec owner is the `InterchangeCodec` discriminant, not a delegate field, because the codec capsules carry no runtime state the row owns.
- [STEP_PROTOCOLS]: `StepProtocol` disambiguates AP203 (config-controlled 3D design), AP214 (automotive core), and AP242 (model-based 3D engineering, the merged successor) sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` set, `StepProtocol.None` on every non-STEP row keeping the column total. All three carry `CanExport=false` — no managed STEP writer is admitted, the in-process BCL-only `StepReader` an import-only Part-21 entity-instance reader. `step-iso10303` drops its `CataloguePackage` marker (`Managed=true`) for the admitted in-process semantic-graph leg yet keeps `Companion=true` because the B-rep/NURBS geometry leg routes to the Compute companion; `BimIo.ImportStep` lands product structure in-process and the geometry hop crosses `tessellation#TESSELLATION_BRIDGE` as the IFC request does.
- [IFC_ZIP_CONTAINER]: `ifc-zip` carries the SAME STEP serialization the `ifc` row does, inside the zip container the `Projection/egress#IFC_EGRESS` `IfcWireForm.StepZip` row's own `Seal` writes and its own `Admit` unzips — one `ZipArchive` over the stream holding one `<stem>.ifc` entry — so the container is that row and never a second column on this table; `.ifczip` is the buildingSMART container extension and `application/x-ifczip` its registered media type, so `Detect` resolves the row by either. BOTH capability flags stand because the wire form carries BOTH delegates: the `Admit` half is what lets the ingest reach the entry bytes before the schema sniff, so the row round-trips.
- [IGES_DISTINCT]: IGES is not an ISO 10303 protocol — its ANSI section-based grammar shares neither the STEP physical-file token grammar nor the GeometryGym entity surface, so the `iges` row carries the distinct `iges-ansi` companion codec and routes its B-rep/NURBS evaluation through the Compute companion (no managed IGES reader admitted); an `iges` row on `step-iso10303` hands `StepReader` a `StepProtocol.None` and a grammar it cannot parse, the deleted form.
- [GLTF_EXTENSION_ROWS]: `KhrExtension` carries each glTF extension SharpGLTF.Core serializes in-box as one row on its `KhrSlot`/`KhrEncoder`/`Writable` discriminants. Registration is NOT a row concern: SharpGLTF.Core's process-global `ExtensionsFactory` already carries the in-box KHR/EXT set and authors it only through the public `Material`/`MaterialChannel`/`Texture`/`Node`/`MeshGpuInstancing`/`ModelRoot` surface, and the `KHR_materials_*` and texture classes are `internal` (only `TextureTransform`/`XmpPackets`/`PunctualLight`/`MeshGpuInstancing` public), so registration over an internal type is unreachable; the write path is the `Writables` NARROWING alone, and a row for an extension the in-box factory does not ship is the rejected phantom.
- [EXTENSION_CAPABILITY]: `Writable` declares each row's WRITE capability, so a row this branch reads and never authors is DECLARED read vocabulary, the `export#EXPORT_RAIL` registration union narrows through `Writables`, and no rostered-but-unfillable row reads as support. `KhrSlot.Scene` and `KhrSlot.Metadata` rows sign `false` — this rail authors no scene object, light, animation channel, or XMP packet; a `KhrSlot.Material` row signs `false` wherever the seam summary drops its magnitude.
- [TEXTURE_SLOT_STATUS]: every `KhrSlot.Texture` row signs `Writable=true` against a named realizing arm at `export#EXPORT_RAIL`. `KHR_texture_transform` realizes through the Toolkit `TextureBuilder.WithTransform` frame a `ChannelImage` carries; `KHR_texture_basisu`, `EXT_texture_webp`, and `MSFT_texture_dds` realize through the CONTAINER of the already-sealed bytes the composing edge binds — `TextureBuilder.PrimaryImage` reads PNG, JPG, DDS, WEBP, and KTX2, so the row is the extension the chosen container obliges rather than a codec this branch owns, and `Rasm.Bim` encodes no image on either rail. `FallbackImage` admits PNG and JPG alone, so a consumer lacking one of the three container extensions reads the core-format fallback rather than an unresolvable texture — the fallback is the row's honest degradation, never a second material.
- [MATERIAL_SLOT_STATUS]: each `KhrSlot.Material` row's `Writable` column IS the statement of whether an arm fills it: `KHR_materials_transmission` realizes off the seam `AppearanceSummary.Transmissive` bit the `export#EXPORT_RAIL` `MaterialFinish.Author` writes; `KHR_materials_specular`, `_clearcoat`, `_sheen`, `_iridescence`, `_anisotropy`, and `_diffuse_transmission` realize through the `GltfChannel` rows a bound map resolves onto — a coat, sheen, iridescence, anisotropy, or subsurface map obliges its own extension at `ChannelImage.Obliges`, so the row and its arm land together. `KHR_materials_volume`, `_dispersion`, `_ior`, and `_emissive_strength` sign `Writable=false`: the seam summary is the one factor source and it drops the volume thickness, the dispersion factor, and the refraction magnitude, while the emissive binding writes unit strength — each row returns to `true` the moment a finish column carries its magnitude. `KHR_materials_unlit` and `KHR_materials_pbrSpecularGlossiness` stay IMPORT vocabulary alone — the exporter selects `WithMetallicRoughnessShader` on every material.
- [COMPRESSION_ROWS]: `Openize.Drako` (`KHR_draco_mesh_compression`) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`) own encode and decode on the `KhrEncoder` discriminant because SharpGLTF.Core ships no compression codec — the `export#EXPORT_RAIL` `GlbBytes` switch drives the encode half and the `import#IMPORT_RAIL` `Decompress` pre-decode branch the symmetric decode half, so both rows are bidirectional and a per-extension importer/exporter type is the deleted form.
- [MESH_TEXT_ROWS]: `geometry3Sharp` (pure-managed, ALC-safe) grounds the `Stl`/`Obj`/`Off` decode, import-only (`CanExport=false`) because the mesh egress is the GLB rail, not `geometry3Sharp`'s writer; `geometry3Sharp` ships no PLY or 3MF reader, so `PLY` is the dedicated `ply-net` codec naming `Ply.Net` (`PlyParser.Parse` over the immutable `Header`/`Dataset`/`PropertyData` graph) retiring the BCL `PlyReader`, and 3MF moves to `scene-exchange`.
- [SCENE_EXCHANGE_ROWS]: `AssimpNetter` (shipping its own osx-arm64 `libassimp.dylib`, RID-coupled but the one admitted owner) covers the formats no other Bim codec owns — FBX, Collada, and the standalone 3MF read leg — through one disposable `AssimpContext` and its `PostProcessSteps` transform algebra; `Fbx`/`Collada` are import-and-export (`CanImport=true`/`CanExport=true`) and `ThreeMf` import-only because the Assimp 3MF leg reads but does not write, faulting the export fold at the boundary. Each scene-exchange row carries its KEY as the AssimpNetter `exportFormatId` (`fbx`/`collada`/`3mf`), pinning the Collada row key to `collada` — a `dae` key handed to `ExportToBlob` misses the export matrix, the `.dae` extension alone owning `Detect` — and the export arm guards through `IsExportFormatSupported`.
- [ACAD_CODEC]: `ACadSharp` (pure-managed AnyCPU IL, osx-arm64-safe) is the in-process DWG+DXF reader, so the `Dwg` row routes `.dwg`/`.dxf` to the managed `acad-sharp` codec (`CanImport=true`/`CanExport=false` — read-only ingress, host-bound DWG/DXF write staying Rhino-native, `TessellationRequiresCompanion=false`) rather than the `native-companion` two-hop; the `import#IMPORT_RAIL` `BimIo` decode arm folds the mesh-bearing `MESH`/`3DFACE`/`POLYFACE_MESH`/`POLYGON_MESH`/`INSERT` entities onto the `ImportedGeometry` triangle soup (the `LINE`/`LWPOLYLINE`/`CIRCLE`/`ARC` 2D profiles being `Rasm.Fabrication`'s `Loop` concern, never this arm), promoting by row data.
- [DOTBIM_CODEC]: `dotbim` (pure-managed, STJ wire, zero native payload) is the only admitted codec whose WIRE expresses instancing — `File` owns a shared `Mesh` pool with placed `Element` instances referencing a pool `MeshId` by a rigid `Vector`+quaternion `Rotation`, a validated `Guid`, a `Type`, an RGBA `Color`, and a `string`→`string` `Info` bag, so N repeated objects serialize as N placements over ONE mesh. Bidirectional: import resolves each `Element.MeshId` against the pool and bakes the placement transform; export pools distinct geometry by content key, decomposes onto `Vector`/`Rotation`, stamps the seam GlobalId onto `Element.Guid` and the classification onto `Type`, and round-trips IFC tags through `Info`. `File.Read`/`File.Save` are PATH-BOUND (`.bim`-enforced, no stream overload), so the byte arms cross a temp path as the `usd-stage` codec does, and the typed setters validate at assignment (`Color` 0..255, `MeshId >= 0`, malformed `Guid`) so an invalid model faults at build before `Save`.
- [POINT_CLOUD_AND_GEOSPATIAL]: `Themis.Las` is the `point-cloud` codec — the ASPRS LAS reader the `reconstruct#RECONSTRUCTION` scan-to-BIM front decodes through, `Unofficial.laszip.netstandard` composing in for the compressed `.laz` so one ingest path reads `.las` and `.laz`; the row stays import-only because no rail composes the catalogued `LasWriter` yet, a point-cloud egress being `reconstruct#RECONSTRUCTION` growth over its point carrier, never a mesh-rail LAS dump. `shp`/`gpkg`/`geojson`/`cityjson`/`fgb`/`geoparquet`/`kml`/`kmz`/`mvt` are the `geospatial-vector` codec the `Semantics/geospatial#VECTOR_INGEST` owner decodes and `GeoVector.Write` re-emits over `NetTopologySuite.IO.Esri.Shapefile`/`bertt.CityJSON`/`FlatGeobuf`/`GISBlox.IO.GeoParquet`, the `MaxRev.Gdal.Core` OGR path with `SharpKml.Core` the admitted managed `.kml`/`.kmz` upgrade, and `NetTopologySuite.IO.VectorTiles.Mapbox` over `.mvt`/`.pbf`; `cityjson` is import-only because the planar `GeoFeature` egress cannot re-emit the 3D city hierarchy while `fgb`/`geoparquet` round-trip. `MaxRev.Gdal.Core` raster is the `geospatial-raster` codec `Semantics/geospatial#RASTER_INGEST` reads, import-only until a raster egress arm composes the GDAL `CreateCopy` write.
- [USD_PEER]: `UsdStage` layer composition carries the USD scene through the `UsdGeomMesh`/`UsdGeomXformable`/`UsdShadeMaterialBindingAPI` schemas while GeometryGym carries the BIM semantics, so the `usd-stage` codec is a scene-graph peer, never a BIM-semantic replacement — deriving `BimElement`/`IfcClass` from USD prim type names is the named boundary violation, the `SWIGTYPE_p_*`/`*PINVOKE` interop types never entering canonical owners and a stage op with no matching RID native payload faulting `BimFault.CapabilityMiss`. `usdz` carries `CanExport=false` because the `UniversalSceneDescription` binding ships `UsdStage.Export` over `.usd`/`.usda`/`.usdc` and no `.usdz` packaging member, so a `CreateNew("*.usdz")` author is the rejected phantom while `.usdz` READS through the package layer. `Frame` stays the Y-up default a metadata-less stage falls to, USD's `upAxis` being per-stage metadata (`UsdGeom.UsdGeomGetStageUpAxis`), so the import `Usd` arm selects the basis per stage and a Z-up CAD/BIM export lands canonical with no row edit.
- [ENERGY_MODEL_ROWS]: `HoneybeeSchema` (HBJSON), `DragonflySchema` (DFJSON), and `NREL.OpenStudio.macOS-arm64` (OSM/gbXML/IDF) are the `energy-model` codec whose realizing arms live on the `Energy/` folder — `CanImport` is the `Energy/projector#ENERGY_PROJECTOR` raise, the hbjson/dfjson `CanExport` the `Energy/derive#MODEL_DERIVE` BIM-to-BEM lower, and the osm/gbxml/idf emit rides `Energy/derive#TRANSLATE_MATRIX` over an OSM-family SOURCE, never the graph, so those rows carry `CanExport=false` per the realizing-arm law. These rows never enter the `BimIo` mesh/scene fold (a format row's arm may live on any rail), `TessellationRequiresCompanion=false` because an energy model carries no BRep, and the frames are Identity.
- [CATALOGUE_PENDING]: `CataloguePackage` is a bare package IDENTIFIER, so the `import#IMPORT_RAIL` pending fault derives from that id and a design note seated there reaches every caller as an unkeyable message body. `ifc5-ecs` awaits a toolkit no registry ships, so its row signs both capability flags false and the wire filter excludes it.
- [DETECT_PRECEDENCE]: `DetectRank` is the DECLARED precedence `Detect` resolves a contended key on, defaulting `0` so an uncontended row omits it. AP242 — the merged successor — outranks AP214 and AP203 on the shared `.step` set, and the round-trippable SAF row outranks the write-only COBie row on the shared `xlsx` media type (a bare media-type detect must resolve a row that can read; the `.saf.xlsx`/`.cobie.xlsx` compound suffixes stay uncontended). Ranking on the `StepProtocol` enum VALUE read a schema token as a precedence scale, so both `xlsx` rows tied and `MaxBy` seated them by accident.
- [ROW_PROMOTION]: one codec admit promotes one row — a candidate flips its `CanImport`/`CanExport` and drops the `CataloguePackage` marker, the `import#IMPORT_RAIL`/`export#EXPORT_RAIL` folds gain one `InterchangeCodec`-keyed arm grounded against the named package with zero new `BimIo`/`BimExport` entrypoint, and the managed-versus-companion split reads from `TessellationRequiresCompanion` (managed grounds its decode inline, companion routes the geometry hop to `tessellation#TESSELLATION_BRIDGE`), never an `if(ifc)`/`if(step)` branch. Chunked simulation-field, FastCDC geometry-delta, and content-addressed artifact codecs stay at `Rasm.Compute/Runtime/codecs`, consumed at the seam, never re-minted here.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using Rasm.Bim.Model;                          // BimFault + the Detail roster every refusal here raises through
using Rasm.Bim.Projection;                     // IfcWireForm — the egress row the Serialization column carries
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
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
    public static readonly InterchangeCodec Saf = new("saf-xlsx", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec CobieXlsx = new("cobie-xlsx", managed: true, companion: false, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec EnergyModel = new("energy-model", managed: true, companion: false, cataloguePackage: Option<string>.None);
    // GeometryGymIFC_Core carries zero `ifcx`/`ifc5` members and `ReleaseVersion` tops at `IFC4X4_DRAFT`, so NO
    // admitted toolkit carries an `.ifcx` ECS admission and the row is pending with no candidate package to name; the
    // gate re-probes as freshness work on a release shipping IFC5 members, never as standing research debt.
    public static readonly InterchangeCodec Ifc5Pending = new("ifc5-ecs", managed: false, companion: false, pending: true, cataloguePackage: Option<string>.None);

    public bool Managed { get; }
    public bool Companion { get; }

    // Pending is its OWN column — a row can await arms for an admitted distribution (package named) or await a
    // toolkit no registry ships (ifc5-ecs, package absent), so deriving pending from package presence conflates
    // the two states and silently disarms the packageless gate. The ctor defaults it false, so only the candidate
    // rows spell it.
    public bool Pending { get; }

    // Candidate package identifier alone — the token the pending fault message names so an operator reads WHICH
    // distribution a row awaits. Design rationale rides the [CATALOGUE_PENDING] card bullet, never this column: a
    // sentence seated here reaches every caller as a fault-message body no consumer can key on.
    public Option<string> CataloguePackage { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KhrExtension {
    // Writable=true names a row an export arm FILLS; Writable=false declares a row this branch READS and never
    // authors, so the roster stays the honest import vocabulary rather than an implied write capability.
    public static readonly KhrExtension DracoMeshCompression = new("KHR_draco_mesh_compression", KhrSlot.Compression, encoder: KhrEncoder.Draco, writable: true);
    public static readonly KhrExtension MeshoptCompression = new("KHR_meshopt_compression", KhrSlot.Compression, encoder: KhrEncoder.Meshopt, writable: true);
    public static readonly KhrExtension MeshGpuInstancing = new("EXT_mesh_gpu_instancing", KhrSlot.Geometry, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension TextureTransform = new("KHR_texture_transform", KhrSlot.Texture, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension TextureWebp = new("EXT_texture_webp", KhrSlot.Texture, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension TextureDds = new("MSFT_texture_dds", KhrSlot.Texture, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension NodeVisibility = new("KHR_node_visibility", KhrSlot.Scene, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension AnimationPointer = new("KHR_animation_pointer", KhrSlot.Scene, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension XmpJsonLd = new("KHR_xmp_json_ld", KhrSlot.Metadata, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsUnlit = new("KHR_materials_unlit", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsSpecular = new("KHR_materials_specular", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsIor = new("KHR_materials_ior", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsIridescence = new("KHR_materials_iridescence", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsSheen = new("KHR_materials_sheen", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsClearcoat = new("KHR_materials_clearcoat", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsTransmission = new("KHR_materials_transmission", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsVolume = new("KHR_materials_volume", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsAnisotropy = new("KHR_materials_anisotropy", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsDispersion = new("KHR_materials_dispersion", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsDiffuseTransmission = new("KHR_materials_diffuse_transmission", KhrSlot.Material, encoder: KhrEncoder.None, writable: true);
    public static readonly KhrExtension MaterialsEmissiveStrength = new("KHR_materials_emissive_strength", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);
    public static readonly KhrExtension MaterialsPbrSpecularGlossiness = new("KHR_materials_pbrSpecularGlossiness", KhrSlot.Material, encoder: KhrEncoder.None, writable: false);

    public KhrSlot Slot { get; }
    public KhrEncoder Encoder { get; }

    // Declared WRITE capability per row — the one column the export#EXPORT_RAIL registration union filters on, so a
    // read-only row can never reach a written extension block. Each row flips WITH the arm that fills it, exactly as
    // a format row's capability flag does, so a preset naming a false row governs nothing and reads as nothing.
    public bool Writable { get; }

    // Rows an export arm fills: a caller's InterchangePolicy roster and a payload's own obliged rows both narrow
    // through this set, so registration never carries a row no producer can serialize.
    public static Seq<KhrExtension> Writables => toSeq(Items.Where(static row => row.Writable));
}

public enum KhrSlot : byte { Compression = 0, Geometry = 1, Texture = 2, Scene = 3, Material = 4, Metadata = 5 }

public enum KhrEncoder : byte { None = 0, Draco = 1, Meshopt = 2 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), canImport: true, canExport: true, codec: InterchangeCodec.SharpGltf, tessellationRequiresCompanion: false, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Step);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Xml);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Json);
    // Zipped STEP repeats the `ifc` row's serialization under a container the Projection/egress#IFC_EGRESS
    // IfcWireForm row owns on BOTH delegates — Seal writes the entry, Admit unzips it ahead of the schema sniff —
    // so no container column joins this table and both capability flags stand against real arms.
    public static readonly InterchangeFormat IfcZip = new("ifc-zip", mediaType: "application/x-ifczip", extensions: Seq(".ifczip"), canImport: true, canExport: true, codec: InterchangeCodec.GeometryGym, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.StepZip);
    // AP203, AP214, and AP242 share one media type and one extension set, so each spells the DetectRank that seats
    // it: AP242 is the merged successor a bare `.step` resolves to, AP203 and AP214 reachable by their own keys.
    public static readonly InterchangeFormat StepAp203 = new("step-ap203", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap203, detectRank: 1);
    public static readonly InterchangeFormat StepAp214 = new("step-ap214", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap214, detectRank: 2);
    public static readonly InterchangeFormat StepAp242 = new("step-ap242", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), canImport: true, canExport: false, codec: InterchangeCodec.StepIso10303, tessellationRequiresCompanion: true, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap242, detectRank: 3);
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
    // SAF structural-analysis XLSX, both directions armed: import realizes through Model/structural#STRUCTURAL_PROJECTION
    // StructuralProjection.Saf(SafOp.Import) + the Author(db, host, model, key) overload authoring GeometryGym
    // structural entities the ONE SemanticProjector ingests; export through Workbook(graph, resolve, key) +
    // Saf(SafOp.Export). Round-trippable, so it outranks the write-only COBie row on the shared media type.
    public static readonly InterchangeFormat Saf = new("saf", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".saf.xlsx"), canImport: true, canExport: true, codec: InterchangeCodec.Saf, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, detectRank: 2);
    // COBie FM-handover XLSX — WRITE-ONLY, GRAPH-SOURCED: the export#COBIE_EMIT CobieEmit.Export author (the
    // ExportPayload codec Switch routes it there; a COBie spreadsheet is never a geometry import source). This row
    // shares its office-spreadsheet media type with `saf`; its rank keeps it ahead of every rankless spreadsheet
    // row while the round-trippable SAF row resolves a bare media-type detect.
    public static readonly InterchangeFormat Cobie = new("cobie", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".cobie.xlsx"), canImport: false, canExport: true, codec: InterchangeCodec.CobieXlsx, tessellationRequiresCompanion: false, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, detectRank: 1);
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

    // Some exactly on the GeometryGym rows; the ctor's nullable `IfcWireForm? serialization = null` arg lifts once
    // to Option, so non-IFC rows omit the column. The value is the `Projection/egress#IFC_EGRESS` `IfcWireForm` row —
    // serialization AND container as one owned value, so the `.ifczip` row differs from `.ifc` by its wire form
    // alone and no container column joins this table. ExportIfc and the wire Seal read THIS row value — a call-site
    // `InterchangeFormat==`/ternary serialization ladder is the deleted form.
    public Option<IfcWireForm> Serialization { get; }

    public Seq<string> Extensions => extensions;

    // Row-level pending state reads its codec's own column AND both capability flags in one hop. A same-named
    // forwarding property on InterchangeCodec restated `Pending` under a second name, so a reader tracing the flag
    // landed on an alias before reaching the column that decides.
    public bool CataloguePending => Codec.Pending && !CanImport && !CanExport;

    // Geometry leg crosses the companion bridge when EITHER the format declares it (IFC/STEP/IGES/native)
    // OR the codec's own geometry read is companion-bound; the import fold reads this one predicate, not two.
    public bool Companion => TessellationRequiresCompanion || Codec.Companion;

    // Both directions on one row. The wire#WIRE_PROJECTION seal and negotiation narrow on THIS rather than on
    // CanExport alone, because a wire that must re-admit what it emitted is answering a two-way question and a
    // one-way row silently satisfied the one-way test.
    public bool RoundTrippable => CanExport && CanImport;

    // Direction-keyed capability read: the column a direction asks about, so the ONE gate below never spells a
    // per-direction branch and a third direction is impossible rather than untested.
    public bool Admits(InterchangeDirection direction) =>
        direction == InterchangeDirection.Import ? CanImport : CanExport;

    // THE capability gate every entrypoint composes — BimIo.ImportGeometry/ImportIfc/ImportStep and
    // BimExport.Export/ExportIfc alike. Ordering is the whole point: a catalogue-pending row implies both flags
    // false, so the pending row must answer FIRST or the plain refusal shadows the richer package-naming message —
    // exactly the ordering each entrypoint used to re-spell and could re-order independently. A companion-bound
    // codec refused in a direction it cannot serve lowers CapabilityMiss (the rail cannot reach an evaluator),
    // where a managed one lowers CodecReject (the row declares one-way vocabulary), so the arm follows the ROW
    // rather than the call site.
    public static Fin<InterchangeFormat> Admitted(InterchangeFormat format, InterchangeDirection direction, Op key) =>
        format.CataloguePending
            ? Fin.Fail<InterchangeFormat>(Pending(direction).At(key, format.Key, format.Codec.CataloguePackage.IfNone("")))
        : format.Admits(direction) ? Fin.Succ(format)
        : format.Companion
            ? Fin.Fail<InterchangeFormat>(Companionless(direction).At(key, format.Key))
            : Fin.Fail<InterchangeFormat>(Detail.DirectionUnsupported.At(key, direction.ToString(), format.Key));

    static Detail Pending(InterchangeDirection direction) =>
        direction == InterchangeDirection.Import ? Detail.ImportCataloguePending : Detail.ExportCataloguePending;

    static Detail Companionless(InterchangeDirection direction) =>
        direction == InterchangeDirection.Import ? Detail.ImportNeedsCompanion : Detail.ExportNeedsHost;

    public bool IsCanonicalFrame => Frame.IsIdentity;

    // Declared precedence among rows sharing a media type or an extension, the ctor's `int detectRank = 0` arg so an
    // uncontended row omits the column. BOTH resolvers below read this ONE column: ranking on the StepProtocol enum
    // VALUE read a schema-vocabulary token as a precedence scale, so the two spreadsheet rows both signing None tied
    // and resolved by roster accident. Rank is a DOMAIN statement per row — see [DETECT_PRECEDENCE].
    public int DetectRank { get; }

    static readonly FrozenDictionary<string, InterchangeFormat> ByExtension =
        Items.SelectMany(static row => row.extensions.Map(ext => (ext, row)))
            .GroupBy(static pair => pair.ext, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static pair => pair.row.DetectRank).row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByMediaType =
        Items.GroupBy(static row => row.MediaType, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static row => row.DetectRank)!, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByKey =
        Items.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.OrdinalIgnoreCase);

    public static Fin<InterchangeFormat> Detect(string pathOrMediaTypeOrKey, Op key) =>
        ByKey.TryGetValue(pathOrMediaTypeOrKey, out var byKey) ? Fin.Succ(byKey)
        : ByMediaType.TryGetValue(pathOrMediaTypeOrKey, out var byType) ? Fin.Succ(byType)
        : ByExtension.TryGetValue(ExtensionOf(pathOrMediaTypeOrKey), out var byExt) ? Fin.Succ(byExt)
        : CompoundSuffix(pathOrMediaTypeOrKey).Match(
            Some: Fin.Succ,
            None: () => Fin.Fail<InterchangeFormat>(Detail.InterchangeFormatMiss.At(key, pathOrMediaTypeOrKey)));

    static string ExtensionOf(string input) =>
        Path.GetExtension(input) is { Length: > 0 } ext ? ext
        : input.StartsWith('.') && !input.Contains('/') ? input
        : "";

    // Longest registered extension the lowercased path ends with wins, so a compound extension (e.g. ".city.json")
    // that Path.GetExtension cannot return as one token resolves where the bare ".json" leg misses. Media-type and
    // key inputs (which carry '/' or no leading dot) never reach this fold.
    static Option<InterchangeFormat> CompoundSuffix(string input) =>
        input.Contains('/')
            ? Option<InterchangeFormat>.None
            : ByExtension.Aggregate(
                (Length: 0, Format: Option<InterchangeFormat>.None),
                (best, pair) => pair.Key.Length > best.Length && input.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase)
                    ? (pair.Key.Length, Some(pair.Value))
                    : best).Format;
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

// InterchangeDirection names the two directions a capability gate asks about; a bool parameter reads identically
// at both call sites and lets a caller pass the wrong polarity with nothing to name the mistake.
public enum InterchangeDirection : byte { Import = 0, Export = 1 }

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FrameNormalization {
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
