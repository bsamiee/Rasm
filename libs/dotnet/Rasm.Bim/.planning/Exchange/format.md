# [BIM_FORMAT_AXIS]

`InterchangeFormat` owns the interchange-format vocabulary: one `[SmartEnum<string>]` table whose rows carry a `CapabilitySet<InterchangeCapability>` discriminating import (foreign bytes to a BIM semantic graph or geometry) from export (an artifact to foreign bytes), joined by the `InterchangeCodec`/`KhrExtension` codec-and-extension axes over that same vocabulary and the per-importer `FrameNormalization` coercing every imported coordinate onto the canonical kernel frame. Format selection is HOST-LOCAL row data.

`import#IMPORT_PIPELINE` ingest and `export#EXPORT_PIPELINE` emit read these rows to dispatch a codec without a call-site branch. Kernel `Rasm` geometry composes as settled vocabulary.

## [01]-[INDEX]

- [02]-[FORMAT_AXIS]: format/codec/extension rows over one capability vocabulary; the corner law, detect-precedence, companion, and `Frame` basis-change columns.

## [02]-[FORMAT_AXIS]

- Owner: `InterchangeCapability` the `[SmartEnum<string>]` realizing kernel `ICapability<InterchangeCapability>` — the ONE combinable vocabulary all three interchange carriers hold; `InterchangeFormat` the format vocabulary keyed by media-type, extension, and key, its `DetectRank` column the declared precedence among rows sharing one of those keys and its `RoundTrippable` derivation the both-directions predicate the wire narrows on; `InterchangeCodec` the codec-owner vocabulary whose set discriminates the managed package, the companion, and the catalogue-pending state reading and writing a row; `KhrExtension` the glTF extension axis on its `KhrSlot`/`KhrEncoder` discriminants and its write capability; `InterchangeCorner` the three named corner rosters and the three `CapabilityLaw<InterchangeCapability>` values built from them; `BasisChange` the per-importer signed-permutation basis carrying positions and normals alike onto the canonical kernel frame; `FrameNormalization` the static surface coercing every imported coordinate onto that frame.
- Auto: `Detect` resolves one row from a key, media type, path, bare dotted extension, or compound suffix with zero call-site branching, a contended media type or extension resolving on the `DetectRank` column both frozen indexes read; `Admitted` proves the row's corner against `InterchangeCorner.FormatLaw` then demands the asked capability through the kernel `Require` door, so the refusal ALWAYS carries the `Missing` complement and the catalogue-pending, companion-bound, and plain one-way arms each raise their own `Detail` row read off the row's own set; `Companion` folds the format capability and the codec capability into the one predicate the import fold reads; `FrameNormalization.Canonicalize` applies a row's `BasisChange` to a position or a normal buffer alike.
- Packages: SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Runtime, GeometryGymIFC_Core, Openize.Drako, Alimer.Bindings.MeshOptimizer, geometry3Sharp, Ply.Net, AssimpNetter, dotbim, Themis.Las, UniversalSceneDescription, NetTopologySuite, NetTopologySuite.IO.Esri.Shapefile, NetTopologySuite.IO.VectorTiles, NetTopologySuite.IO.VectorTiles.Mapbox, SharpKml.Core, bertt.CityJSON, FlatGeobuf, GISBlox.IO.GeoParquet, MaxRev.Gdal.Core, ACadSharp, HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new interchange format is one `InterchangeFormat` row (media-type, extensions, one named `InterchangeCorner` set, codec owner, `Frame`, `StepProtocol`, and `DetectRank` only where it contends for a key another row already holds); a new legal corner is one `InterchangeCorner` row the law admits, so a capability combination no row holds today is legislated once rather than discovered at a call site; a new IFC wire form GeometryGym emits is one `Projection/wireform#IFC_WIRE_FORM` row on the owning AXIS — a serialization carrying its own probe, seal, and admit delegates, or a container naming what it wraps — and the crossing generates the form the `Serialization` column names, never a call-site format ladder and never a container column beside it; a new managed codec is one `InterchangeCodec` row; a new glTF capability is one `KhrExtension` row on its `KhrSlot`/`KhrEncoder` discriminants, an import-only extension entering on the empty corner rather than staying off the roster; a new ingest frame is one `BasisChange` static row, never a per-axis branch, and a MIRRORING frame (negative determinant) lands its winding-reversal arm in the same row rather than a standing predicate no row exercises; a not-yet-decompiled format admits as a pending candidate row naming its package, promoted in place when the catalogue lands.
- Boundary: format selection is row data resolved through `Detect`, never a call-site extension switch. `Admitted(format, demanded)` proves the row's corner and demands through the kernel `Require` door once, so no entrypoint re-spells the pending/capability order. A capability flips only with a realizing arm: canonical IFC SPF, STEP, and IGES have real generated companion inputs; IFC XML, IFC JSON, and IFC ZIP remain managed round-trip encodings rather than aliasing onto the SPF source arm; RVT and NWC remain recognized identities under `PendingCompanion` with an `Unadmitted` codec until a native service exists. `RoundTrippable` remains the derived two-direction proof, and every managed read/write arm stays attached to its codec owner rather than a parallel importer family.
- [STEP_PROTOCOLS]: `StepProtocol` disambiguates AP203 (config-controlled 3D design), AP214 (automotive core), and AP242 (model-based 3D engineering, the merged successor) sharing the `step-iso10303` codec and the `.step`/`.stp`/`.p21` set, `StepProtocol.None` on every non-STEP row keeping the column total. All three hold `Import` alone beside `CompanionGeometry` — no managed STEP writer is admitted, the in-process BCL-only `StepReader` an import-only Part-21 entity-instance reader. `step-iso10303` holds `ManagedDecode` for the admitted in-process semantic-graph leg yet keeps `CompanionDecode` because the B-rep/NURBS geometry leg routes to the Compute companion; `BimIo.ImportStep` lands product structure in-process and the geometry hop crosses `tessellation#TESSELLATION_BRIDGE` as the IFC request does.
- [IFC_ZIP_CONTAINER]: `ifc-zip` carries the SAME STEP serialization the `ifc` row does, inside the zip container the `Projection/wireform#IFC_WIRE_FORM` `IfcWireForm.StepZip` form names — the serialization and the container are that owner's two AXES and the form is their admitted crossing, so the container is never a second column on this table; `.ifczip` is the buildingSMART container extension and `application/x-ifczip` its registered media type, so `Detect` resolves the row by either. BOTH managed directions stand because the wire form carries BOTH delegates: the `Admit` half is what lets the ingest reach the entry bytes before the schema sniff, so the row round-trips. The generated companion source arm accepts canonical IFC SPF bytes, not a ZIP container; direct companion geometry therefore stays off this row rather than silently aliasing the container to SPF.
- [IGES_DISTINCT]: IGES is not an ISO 10303 protocol — its ANSI section-based grammar shares neither the STEP physical-file token grammar nor the GeometryGym entity surface, so the `iges` row carries the distinct `iges-ansi` companion codec and routes its B-rep/NURBS evaluation through the Compute companion (no managed IGES reader admitted); an `iges` row on `step-iso10303` hands `StepReader` a `StepProtocol.None` and a grammar it cannot parse, the deleted form.
- [GLTF_EXTENSION_ROWS]: `KhrExtension` carries each glTF extension SharpGLTF.Core serializes in-box as one row on its `KhrSlot`/`KhrEncoder`/`Writable` discriminants. Registration is NOT a row concern: SharpGLTF.Core's process-global `ExtensionsFactory` already carries the in-box KHR/EXT set and authors it only through the public `Material`/`MaterialChannel`/`Texture`/`Node`/`MeshGpuInstancing`/`ModelRoot` surface, and the `KHR_materials_*` and texture classes are `internal` (only `TextureTransform`/`XmpPackets`/`PunctualLight`/`MeshGpuInstancing` public), so registration over an internal type is unreachable; the write path is the `Writables` NARROWING alone, and a row for an extension the in-box factory does not ship is the rejected phantom.
- [EXTENSION_CAPABILITY]: `WriteExtension` declares each row.s WRITE capability, so a row this branch reads and never authors is DECLARED read vocabulary, the `export#EXPORT_PIPELINE` registration union narrows through `Writables`, and no rostered-but-unfillable row reads as support. `KhrSlot.Scene` and `KhrSlot.Metadata` rows hold the empty corner — this path authors no scene object, light, animation channel, or XMP packet; a `KhrSlot.Material` row empties wherever the shared summary drops its magnitude.
- [TEXTURE_SLOT_STATUS]: every `KhrSlot.Texture` row holds `WriteExtension` against a named realizing arm at `export#EXPORT_PIPELINE`. `KHR_texture_transform` realizes through the Toolkit `TextureBuilder.WithTransform` frame a `ChannelImage` carries; `KHR_texture_basisu`, `EXT_texture_webp`, and `MSFT_texture_dds` realize through the CONTAINER of the already-sealed bytes the composing edge binds — `TextureBuilder.PrimaryImage` reads PNG, JPG, DDS, WEBP, and KTX2, so the row is the extension the chosen container obliges rather than a codec this branch owns, and `Rasm.Bim` encodes no image on either path. `FallbackImage` admits PNG and JPG alone, so a consumer lacking one of the three container extensions reads the core-format fallback rather than an unresolvable texture — the fallback is the row's honest degradation, never a second material.
- [MATERIAL_SLOT_STATUS]: each `KhrSlot.Material` row.s corner IS the statement of whether an arm fills it: `KHR_materials_transmission` realizes off the shared `AppearanceSummary.Transmissive` bit the `export#EXPORT_PIPELINE` `MaterialFinish.Author` writes; `KHR_materials_specular`, `_clearcoat`, `_sheen`, `_iridescence`, `_anisotropy`, and `_diffuse_transmission` realize through the `GltfChannel` rows a bound map resolves onto — a coat, sheen, iridescence, anisotropy, or subsurface map obliges its own extension at `ChannelImage.Obliges`, so the row and its arm land together. `KHR_materials_volume`, `_dispersion`, `_ior`, and `_emissive_strength` hold the empty corner: the shared summary is the one factor source and it drops the volume thickness, the dispersion factor, and the refraction magnitude, while the emissive binding writes unit strength — each row takes `WriteExtension` the moment a finish column carries its magnitude. `KHR_materials_unlit` and `KHR_materials_pbrSpecularGlossiness` stay IMPORT vocabulary alone — the exporter selects `WithMetallicRoughnessShader` on every material.
- [COMPRESSION_ROWS]: `Openize.Drako` (`KHR_draco_mesh_compression`) and `Alimer.Bindings.MeshOptimizer` (`KHR_meshopt_compression`) own encode and decode on the `KhrEncoder` discriminant because SharpGLTF.Core ships no compression codec — the `export#EXPORT_PIPELINE` `GlbBytes` switch drives the encode half and the `import#IMPORT_PIPELINE` `Decompress` pre-decode branch the symmetric decode half, so both rows are bidirectional and a per-extension importer/exporter type is the deleted form.
- [MESH_TEXT_ROWS]: `geometry3Sharp` (pure-managed, ALC-safe) grounds the `Stl`/`Obj`/`Off` decode, import-only because the mesh egress is the GLB path, not `geometry3Sharp`'s writer; `geometry3Sharp` ships no PLY or 3MF reader, so `PLY` is the dedicated `ply-net` codec naming `Ply.Net` (`PlyParser.Parse` over the immutable `Header`/`Dataset`/`PropertyData` graph) retiring the BCL `PlyReader`, and 3MF moves to `scene-exchange`.
- [SCENE_EXCHANGE_ROWS]: `AssimpNetter` (shipping its own osx-arm64 `libassimp.dylib`, RID-coupled but the one admitted owner) covers the formats no other Bim codec owns — FBX, Collada, and the standalone 3MF read leg — through one disposable `AssimpContext` and its `PostProcessSteps` transform algebra; `Fbx`/`Collada` hold both directions and `ThreeMf` import-only because the Assimp 3MF leg reads but does not write, faulting the export fold at the boundary. Each scene-exchange row carries its KEY as the AssimpNetter `exportFormatId` (`fbx`/`collada`/`3mf`), pinning the Collada row key to `collada` — a `dae` key handed to `ExportToBlob` misses the export matrix, the `.dae` extension alone owning `Detect` — and the export arm guards through `IsExportFormatSupported`.
- [ACAD_CODEC]: `ACadSharp` (pure-managed AnyCPU IL, osx-arm64-safe) is the in-process DWG+DXF reader, so the `Dwg` row routes `.dwg`/`.dxf` to the managed `acad-sharp` codec (`Import` alone — read-only ingress, host-bound DWG/DXF write staying Rhino-native, no `CompanionGeometry`) rather than the `native-companion` two-hop; the `import#IMPORT_PIPELINE` `BimIo` decode arm folds the mesh-bearing `MESH`/`3DFACE`/`POLYFACE_MESH`/`POLYGON_MESH`/`INSERT` entities onto the `ImportedGeometry` triangle soup (the `LINE`/`LWPOLYLINE`/`CIRCLE`/`ARC` 2D profiles being `Rasm.Fabrication`'s `Loop` concern, never this arm), promoting by row data. That fold carries GEOMETRY alone: every DWG layer name, ACI colour index, and plot pen weight the entity declares is a BOUNDED shed the decode arm TELLS — `DecodeReason.PresentationDropped` degrades once per distinct source layer on the exchange-degrade hook point (the `import#IMPORT_PIPELINE` idiom every Fin-result-returning decode shed rides; the `FidelityDrop` ledger is the WriterT round-trip carrier and a ledger row with no writer thread would be a producerless law), the VERBATIM layer name the subject — the wire declares no layer standard, so the kernel `Rasm/Drawing/sheet#LAYER` `LayerName.Parse(standard, text)` composes at a re-authoring consumer that ELECTS a standard, never at the shed. A re-authoring consumer reads a counted named loss instead of inferring silence.
- [DOTBIM_CODEC]: `dotbim` (pure-managed, STJ wire, zero native payload) is the only admitted codec whose WIRE expresses instancing — `File` owns a shared `Mesh` pool with placed `Element` instances referencing a pool `MeshId` by a rigid `Vector`+quaternion `Rotation`, a validated `Guid`, a `Type`, an RGBA `Color`, and a `string`→`string` `Info` bag, so N repeated objects serialize as N placements over ONE mesh. Bidirectional: import resolves each `Element.MeshId` against the pool and bakes the placement transform; export pools distinct geometry by content key, decomposes onto `Vector`/`Rotation`, stamps the shared GlobalId onto `Element.Guid` and the classification onto `Type`, and round-trips IFC tags through `Info`. `File.Read`/`File.Save` are PATH-BOUND (`.bim`-enforced, no stream overload), so the byte arms cross a temp path as the `usd-stage` codec does, and the typed setters validate at assignment (`Color` 0..255, `MeshId >= 0`, malformed `Guid`) so an invalid model faults at build before `Save`.
- [POINT_CLOUD_AND_GEOSPATIAL]: `Themis.Las` is the `point-cloud` codec — the ASPRS LAS reader the `reconstruct#RECONSTRUCTION` scan-to-BIM front decodes through, `Unofficial.laszip.netstandard` composing in for the compressed `.laz` so one ingest path reads `.las` and `.laz`; the row stays import-only because no path composes the catalogued `LasWriter` yet, a point-cloud egress being `reconstruct#RECONSTRUCTION` growth over its point carrier, never a mesh-path LAS dump. `shp`/`gpkg`/`geojson`/`cityjson`/`fgb`/`geoparquet`/`kml`/`kmz`/`mvt` are the `geospatial-vector` codec the `Semantics/vector#VECTOR_FOLD` owner decodes and `GeoVector.Write` re-emits over `NetTopologySuite.IO.Esri.Shapefile`/`bertt.CityJSON`/`FlatGeobuf`/`GISBlox.IO.GeoParquet`, the `MaxRev.Gdal.Core` OGR path with `SharpKml.Core` the admitted managed `.kml`/`.kmz` upgrade, and `NetTopologySuite.IO.VectorTiles.Mapbox` over `.mvt`/`.pbf`; `cityjson` is import-only because the planar `GeoFeature` egress cannot re-emit the 3D city hierarchy while `fgb`/`geoparquet` round-trip. `MaxRev.Gdal.Core` raster is the `geospatial-raster` codec `Semantics/raster#RASTER_INGEST` reads, import-only until a raster egress arm composes the GDAL `CreateCopy` write.
- [USD_PEER]: `UsdStage` layer composition carries the USD scene through the `UsdGeomMesh`/`UsdGeomXformable`/`UsdShadeMaterialBindingAPI` schemas while GeometryGym carries the BIM semantics, so the `usd-stage` codec is a scene-graph peer, never a BIM-semantic replacement — deriving `BimElement`/`IfcClass` from USD prim type names is the named boundary violation, the `SWIGTYPE_p_*`/`*PINVOKE` interop types never entering canonical owners and a stage op with no matching RID native payload faulting `BimFault.Refused` with `BimReason.Capability`. `usdz` withholds `Export` because the `UniversalSceneDescription` binding ships `UsdStage.Export` over `.usd`/`.usda`/`.usdc` and no `.usdz` packaging member, so a `CreateNew("*.usdz")` author is the rejected phantom while `.usdz` READS through the package layer. `Frame` stays the Y-up default a metadata-less stage falls to, USD's `upAxis` being per-stage metadata (`UsdGeom.UsdGeomGetStageUpAxis`), so the import `Usd` arm selects the basis per stage and a Z-up CAD/BIM export lands canonical with no row edit.
- [ENERGY_MODEL_ROWS]: `HoneybeeSchema` (HBJSON), `DragonflySchema` (DFJSON), and `NREL.OpenStudio.macOS-arm64` (OSM/gbXML/IDF) are the `energy-model` codec whose realizing arms live on the `Energy/` folder — `Import` is the `Energy/projector#ENERGY_PROJECTOR` raise, the hbjson/dfjson `Export` the `Energy/derive#MODEL_DERIVE` BIM-to-BEM lower, and the osm/gbxml/idf emit rides `Energy/derive#TRANSLATE_MATRIX` over an OSM-family SOURCE, never the graph, so those rows withhold `Export` per the realizing-arm law. These rows never enter the `BimIo` mesh/scene fold (a format row.s arm may live on any path), no row holds `CompanionGeometry` because an energy model carries no BRep, and the frames are Identity.
- [CATALOGUE_PENDING]: `CataloguePackage` is a bare package identifier. `ifc5-ecs` awaits a toolkit no registry ships; RVT and NWC await a real native companion service and generated source arms. All three retain recognized format identities under a pending corner, never claim a live import, and are excluded from the wire projection until their executable owner lands.
- [DETECT_PRECEDENCE]: `DetectRank` is the DECLARED precedence `Detect` resolves a contended key on, defaulting `0` so an uncontended row omits it. AP242 — the merged successor — outranks AP214 and AP203 on the shared `.step` set, and the round-trippable SAF row outranks the write-only COBie row on the shared `xlsx` media type (a bare media-type detect must resolve a row that can read; the `.saf.xlsx`/`.cobie.xlsx` compound suffixes stay uncontended). Ranking on the `StepProtocol` enum VALUE read a schema token as a precedence scale, so both `xlsx` rows tied and `MaxBy` seated them by accident.
- [ROW_PROMOTION]: one codec admit promotes one row — a candidate trades the pending corner for a direction-bearing one and drops the `CataloguePackage` marker, the `import#IMPORT_PIPELINE`/`export#EXPORT_PIPELINE` folds gain one `InterchangeCodec`-keyed arm grounded against the named package with zero new `BimIo`/`BimExport` entrypoint, and the managed-versus-companion split reads the `Companion` predicate (managed grounds its decode inline, companion routes the geometry hop to `tessellation#TESSELLATION_BRIDGE`), never an `if(ifc)`/`if(step)` branch. Chunked simulation-field, FastCDC geometry-delta, and content-addressed artifact codecs stay at `Rasm.Compute/Runtime/codecs`, consumed at the boundary, never re-minted here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Bim.Model;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InterchangeCapability : ICapability<InterchangeCapability> {
    public static readonly InterchangeCapability Import = new("import");
    public static readonly InterchangeCapability Export = new("export");
    public static readonly InterchangeCapability CompanionGeometry = new("companion-geometry");
    public static readonly InterchangeCapability ManagedDecode = new("managed-decode");
    public static readonly InterchangeCapability CompanionDecode = new("companion-decode");
    public static readonly InterchangeCapability CataloguePending = new("catalogue-pending");
    public static readonly InterchangeCapability WriteExtension = new("write-extension");
}

public static class InterchangeCorner {
    static CapabilitySet<InterchangeCapability> Of(params ReadOnlySpan<InterchangeCapability> held) =>
        CapabilitySet<InterchangeCapability>.Of(held);

    public static readonly CapabilitySet<InterchangeCapability> Read = Of(InterchangeCapability.Import);
    public static readonly CapabilitySet<InterchangeCapability> Write = Of(InterchangeCapability.Export);
    public static readonly CapabilitySet<InterchangeCapability> RoundTrip = Of(InterchangeCapability.Import, InterchangeCapability.Export);
    public static readonly CapabilitySet<InterchangeCapability> ReadCompanion = Of(InterchangeCapability.Import, InterchangeCapability.CompanionGeometry);
    public static readonly CapabilitySet<InterchangeCapability> WriteCompanion = Of(InterchangeCapability.Export, InterchangeCapability.CompanionGeometry);
    public static readonly CapabilitySet<InterchangeCapability> RoundTripCompanion = Of(InterchangeCapability.Import, InterchangeCapability.Export, InterchangeCapability.CompanionGeometry);
    public static readonly CapabilitySet<InterchangeCapability> Pending = Of(InterchangeCapability.CataloguePending);
    public static readonly CapabilitySet<InterchangeCapability> PendingCompanion = Of(InterchangeCapability.CataloguePending, InterchangeCapability.CompanionGeometry);

    public static readonly CapabilitySet<InterchangeCapability> Inline = Of(InterchangeCapability.ManagedDecode);
    public static readonly CapabilitySet<InterchangeCapability> InlineThenCompanion = Of(InterchangeCapability.ManagedDecode, InterchangeCapability.CompanionDecode);
    public static readonly CapabilitySet<InterchangeCapability> CompanionOnly = Of(InterchangeCapability.CompanionDecode);
    public static readonly CapabilitySet<InterchangeCapability> Unadmitted = Of(InterchangeCapability.CataloguePending);

    public static readonly CapabilitySet<InterchangeCapability> Authored = Of(InterchangeCapability.WriteExtension);
    public static readonly CapabilitySet<InterchangeCapability> Imported = CapabilitySet<InterchangeCapability>.None;

    public static readonly CapabilityLaw<InterchangeCapability> FormatLaw = new(Seq(
        Read, Write, RoundTrip, ReadCompanion, WriteCompanion, RoundTripCompanion, Pending, PendingCompanion));

    public static readonly CapabilityLaw<InterchangeCapability> CodecLaw = new(Seq(
        Inline, InlineThenCompanion, CompanionOnly, Unadmitted));

    public static readonly CapabilityLaw<InterchangeCapability> ExtensionLaw = new(Seq(Authored, Imported));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class InterchangeCodec {
    public static readonly InterchangeCodec SharpGltf = new("sharp-gltf", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeometryGym = new("geometry-gym", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec StepIso10303 = new("step-iso10303", capabilities: InterchangeCorner.InlineThenCompanion, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec MeshText = new("mesh-text", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Ply = new("ply-net", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec DotBim = new("dotbim", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec SceneExchange = new("scene-exchange", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec PointCloud = new("point-cloud", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialVector = new("geospatial-vector", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec GeospatialRaster = new("geospatial-raster", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec NativeCompanion = new("native-companion", capabilities: InterchangeCorner.Unadmitted, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec AcadSharp = new("acad-sharp", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec IgesAnsi = new("iges-ansi", capabilities: InterchangeCorner.CompanionOnly, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec UsdStage = new("usd-stage", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Saf = new("saf-xlsx", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec CobieXlsx = new("cobie-xlsx", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec EnergyModel = new("energy-model", capabilities: InterchangeCorner.Inline, cataloguePackage: Option<string>.None);
    public static readonly InterchangeCodec Ifc5Pending = new("ifc5-ecs", capabilities: InterchangeCorner.Unadmitted, cataloguePackage: Option<string>.None);

    public CapabilitySet<InterchangeCapability> Capabilities { get; }

    public Option<string> CataloguePackage { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KhrExtension {
    public static readonly KhrExtension DracoMeshCompression = new("KHR_draco_mesh_compression", KhrSlot.Compression, encoder: KhrEncoder.Draco, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MeshoptCompression = new("KHR_meshopt_compression", KhrSlot.Compression, encoder: KhrEncoder.Meshopt, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MeshGpuInstancing = new("EXT_mesh_gpu_instancing", KhrSlot.Geometry, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension TextureTransform = new("KHR_texture_transform", KhrSlot.Texture, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension TextureBasisu = new("KHR_texture_basisu", KhrSlot.Texture, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension TextureWebp = new("EXT_texture_webp", KhrSlot.Texture, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension TextureDds = new("MSFT_texture_dds", KhrSlot.Texture, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension LightsPunctual = new("KHR_lights_punctual", KhrSlot.Scene, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension NodeVisibility = new("KHR_node_visibility", KhrSlot.Scene, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension AnimationPointer = new("KHR_animation_pointer", KhrSlot.Scene, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension XmpJsonLd = new("KHR_xmp_json_ld", KhrSlot.Metadata, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsUnlit = new("KHR_materials_unlit", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsSpecular = new("KHR_materials_specular", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsIor = new("KHR_materials_ior", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsIridescence = new("KHR_materials_iridescence", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsSheen = new("KHR_materials_sheen", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsClearcoat = new("KHR_materials_clearcoat", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsTransmission = new("KHR_materials_transmission", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsVolume = new("KHR_materials_volume", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsAnisotropy = new("KHR_materials_anisotropy", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsDispersion = new("KHR_materials_dispersion", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsDiffuseTransmission = new("KHR_materials_diffuse_transmission", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Authored);
    public static readonly KhrExtension MaterialsEmissiveStrength = new("KHR_materials_emissive_strength", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);
    public static readonly KhrExtension MaterialsPbrSpecularGlossiness = new("KHR_materials_pbrSpecularGlossiness", KhrSlot.Material, encoder: KhrEncoder.None, capabilities: InterchangeCorner.Imported);

    public KhrSlot Slot { get; }
    public KhrEncoder Encoder { get; }

    public CapabilitySet<InterchangeCapability> Capabilities { get; }

    public static Seq<KhrExtension> Writables => WritableRows.Value;

    static readonly Lazy<Seq<KhrExtension>> WritableRows = new(static () =>
        toSeq(Items.Where(static row => row.Capabilities.Admits(InterchangeCapability.WriteExtension))));
}

public enum KhrSlot : byte { Compression = 0, Geometry = 1, Texture = 2, Scene = 3, Material = 4, Metadata = 5 }

public enum KhrEncoder : byte { None = 0, Draco = 1, Meshopt = 2 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class InterchangeFormat {
    public static readonly InterchangeFormat Gltf = new("gltf", mediaType: "model/gltf+json", extensions: Seq(".gltf"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.SharpGltf, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Glb = new("glb", mediaType: "model/gltf-binary", extensions: Seq(".glb"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.SharpGltf, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc = new("ifc", mediaType: "application/x-step", extensions: Seq(".ifc"), capabilities: InterchangeCorner.RoundTripCompanion, codec: InterchangeCodec.GeometryGym, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Step);
    public static readonly InterchangeFormat IfcXml = new("ifc-xml", mediaType: "application/ifc+xml", extensions: Seq(".ifcxml"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeometryGym, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Xml);
    public static readonly InterchangeFormat IfcJson = new("ifc-json", mediaType: "application/ifc+json", extensions: Seq(".ifcjson"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeometryGym, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.Json);
    public static readonly InterchangeFormat IfcZip = new("ifc-zip", mediaType: "application/x-ifczip", extensions: Seq(".ifczip"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeometryGym, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, serialization: IfcWireForm.StepZip);
    public static readonly InterchangeFormat StepAp203 = new("step-ap203", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), capabilities: InterchangeCorner.ReadCompanion, codec: InterchangeCodec.StepIso10303, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap203, detectRank: 1);
    public static readonly InterchangeFormat StepAp214 = new("step-ap214", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), capabilities: InterchangeCorner.ReadCompanion, codec: InterchangeCodec.StepIso10303, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap214, detectRank: 2);
    public static readonly InterchangeFormat StepAp242 = new("step-ap242", mediaType: "application/step", extensions: Seq(".step", ".stp", ".p21"), capabilities: InterchangeCorner.ReadCompanion, codec: InterchangeCodec.StepIso10303, frame: BasisChange.Identity, stepProtocol: StepProtocol.Ap242, detectRank: 3);
    public static readonly InterchangeFormat Iges = new("iges", mediaType: "model/iges", extensions: Seq(".igs", ".iges"), capabilities: InterchangeCorner.ReadCompanion, codec: InterchangeCodec.IgesAnsi, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Stl = new("stl", mediaType: "model/stl", extensions: Seq(".stl"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.MeshText, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat ThreeMf = new("3mf", mediaType: "model/3mf", extensions: Seq(".3mf"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.SceneExchange, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Obj = new("obj", mediaType: "model/obj", extensions: Seq(".obj"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.MeshText, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Off = new("off", mediaType: "model/off", extensions: Seq(".off"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.MeshText, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ply = new("ply", mediaType: "model/ply", extensions: Seq(".ply"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.Ply, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat DotBim = new("bim", mediaType: "application/vnd.dotbim+json", extensions: Seq(".bim"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.DotBim, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Las = new("las", mediaType: "application/vnd.las", extensions: Seq(".las", ".laz"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.PointCloud, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Shapefile = new("shp", mediaType: "application/vnd.shp", extensions: Seq(".shp"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoPackage = new("gpkg", mediaType: "application/geopackage+sqlite3", extensions: Seq(".gpkg"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoJson = new("geojson", mediaType: "application/geo+json", extensions: Seq(".geojson"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat CityJson = new("cityjson", mediaType: "application/city+json", extensions: Seq(".city.json", ".cityjson"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat FlatGeobuf = new("fgb", mediaType: "application/vnd.flatgeobuf", extensions: Seq(".fgb"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoParquet = new("geoparquet", mediaType: "application/vnd.apache.parquet", extensions: Seq(".parquet"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Kml = new("kml", mediaType: "application/vnd.google-earth.kml+xml", extensions: Seq(".kml"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Kmz = new("kmz", mediaType: "application/vnd.google-earth.kmz", extensions: Seq(".kmz"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Mvt = new("mvt", mediaType: "application/vnd.mapbox-vector-tile", extensions: Seq(".mvt", ".pbf"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.GeospatialVector, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GeoTiff = new("geotiff", mediaType: "image/tiff;application=geotiff", extensions: Seq(".tif", ".tiff"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.GeospatialRaster, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Rvt = new("rvt", mediaType: "application/vnd.autodesk.rvt", extensions: Seq(".rvt"), capabilities: InterchangeCorner.PendingCompanion, codec: InterchangeCodec.NativeCompanion, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Nwc = new("nwc", mediaType: "application/vnd.autodesk.nwc", extensions: Seq(".nwc", ".nwd"), capabilities: InterchangeCorner.PendingCompanion, codec: InterchangeCodec.NativeCompanion, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dwg = new("dwg", mediaType: "application/vnd.autodesk.dwg", extensions: Seq(".dwg", ".dxf"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.AcadSharp, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Ifc5 = new("ifc5", mediaType: "application/ifc5+json", extensions: Seq(".ifcx", ".ifc5"), capabilities: InterchangeCorner.PendingCompanion, codec: InterchangeCodec.Ifc5Pending, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Saf = new("saf", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".saf.xlsx"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.Saf, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, detectRank: 2);
    public static readonly InterchangeFormat Cobie = new("cobie", mediaType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensions: Seq(".cobie.xlsx"), capabilities: InterchangeCorner.Write, codec: InterchangeCodec.CobieXlsx, frame: BasisChange.Identity, stepProtocol: StepProtocol.None, detectRank: 1);
    public static readonly InterchangeFormat Hbjson = new("hbjson", mediaType: "application/vnd.ladybug.hbjson+json", extensions: Seq(".hbjson"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.EnergyModel, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Dfjson = new("dfjson", mediaType: "application/vnd.ladybug.dfjson+json", extensions: Seq(".dfjson"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.EnergyModel, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Osm = new("osm", mediaType: "application/vnd.openstudio.osm", extensions: Seq(".osm"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.EnergyModel, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat GbXml = new("gbxml", mediaType: "application/vnd.gbxml+xml", extensions: Seq(".gbxml"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.EnergyModel, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Idf = new("idf", mediaType: "application/vnd.energyplus.idf", extensions: Seq(".idf"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.EnergyModel, frame: BasisChange.Identity, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usd = new("usd", mediaType: "model/vnd.usd", extensions: Seq(".usd", ".usda", ".usdc"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.UsdStage, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Usdz = new("usdz", mediaType: "model/vnd.usdz+zip", extensions: Seq(".usdz"), capabilities: InterchangeCorner.Read, codec: InterchangeCodec.UsdStage, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Fbx = new("fbx", mediaType: "application/vnd.autodesk.fbx", extensions: Seq(".fbx"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.SceneExchange, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);
    public static readonly InterchangeFormat Collada = new("collada", mediaType: "model/vnd.collada+xml", extensions: Seq(".dae"), capabilities: InterchangeCorner.RoundTrip, codec: InterchangeCodec.SceneExchange, frame: BasisChange.YUpToCanonical, stepProtocol: StepProtocol.None);

    private readonly Seq<string> extensions;

    public string MediaType { get; }
    public CapabilitySet<InterchangeCapability> Capabilities { get; }
    public InterchangeCodec Codec { get; }
    public BasisChange Frame { get; }
    public StepProtocol StepProtocol { get; }

    public Option<IfcWireForm> Serialization { get; }

    public Seq<string> Extensions => extensions;

    public bool CataloguePending => Capabilities.Admits(InterchangeCapability.CataloguePending);

    public bool Companion =>
        Capabilities.Admits(InterchangeCapability.CompanionGeometry)
        || Codec.Capabilities.Admits(InterchangeCapability.CompanionDecode);

    public bool RoundTrippable => Capabilities.AdmitsAll(InterchangeCorner.RoundTrip);

    public static Fin<InterchangeFormat> Admitted(InterchangeFormat format, InterchangeCapability demanded) =>
        from corner in InterchangeCorner.FormatLaw.Admit(format.Capabilities)
        from _ in corner.Require(
            CapabilitySet<InterchangeCapability>.Of(demanded),
            missing => format.Refuse(missing))
        select format;

    Error Refuse(CapabilitySet<InterchangeCapability> missing) {
        bool importing = missing == InterchangeCorner.Read;
        BimScope scope = importing ? BimScope.Import : missing == InterchangeCorner.Write ? BimScope.Export : BimScope.Format;
        if (CataloguePending) {
            return new BimFault.Refused(scope, BimReason.Codec,
                string.Join(':', new object?[] { importing ? "import-catalogue-pending" : "export-catalogue-pending", Key, Codec.CataloguePackage.IfNone("") }));
        }
        if (Companion) {
            return new BimFault.Refused(scope, BimReason.Capability,
                string.Join(':', new object?[] { importing ? "import-needs-companion" : "export-needs-host", Key }));
        }
        return new BimFault.Refused(BimScope.Format, BimReason.Codec,
            string.Join(':', new object?[] { "direction-unsupported", missing.Wire, Key }));
    }

    public bool IsCanonicalFrame => Frame.IsIdentity;

    public int DetectRank { get; }

    static readonly FrozenDictionary<string, InterchangeFormat> ByExtension =
        Items.SelectMany(static row => row.extensions.Map(ext => (ext, row)))
            .GroupBy(static pair => pair.ext, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static pair => pair.row.DetectRank).row, StringComparer.OrdinalIgnoreCase);

    static readonly FrozenDictionary<string, InterchangeFormat> ByMediaType =
        Items.GroupBy(static row => row.MediaType, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static group => group.Key, static group => group.MaxBy(static row => row.DetectRank)!, StringComparer.OrdinalIgnoreCase);

    public static Fin<InterchangeFormat> Detect(string pathOrMediaTypeOrKey) =>
        TryGet(pathOrMediaTypeOrKey, out InterchangeFormat? byKey) && byKey is { } keyed ? Fin.Succ(keyed)
        : ByMediaType.TryGetValue(pathOrMediaTypeOrKey, out var byType) ? Fin.Succ(byType)
        : ByExtension.TryGetValue(ExtensionOf(pathOrMediaTypeOrKey), out var byExt) ? Fin.Succ(byExt)
        : CompoundSuffix(pathOrMediaTypeOrKey).ToFin(
            new BimFault.Refused(BimScope.Format, BimReason.Codec, string.Join(':', new object?[] { "interchange-format-miss", pathOrMediaTypeOrKey })));

    static string ExtensionOf(string input) =>
        Path.GetExtension(input) is { Length: > 0 } ext ? ext
        : input.StartsWith('.') && !input.Contains('/') ? input
        : "";

    static Option<InterchangeFormat> CompoundSuffix(string input) =>
        input.Contains('/')
            ? Option<InterchangeFormat>.None
            : ByExtension.Aggregate(
                (Length: 0, Format: Option<InterchangeFormat>.None),
                (best, pair) => pair.Key.Length > best.Length && input.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase)
                    ? (pair.Key.Length, Some(pair.Value))
                    : best).Format;
}

public readonly record struct BasisChange(sbyte CanonicalX, sbyte CanonicalY, sbyte CanonicalZ) {
    public static readonly BasisChange Identity = new(1, 2, 3);
    public static readonly BasisChange YUpToCanonical = new(1, -3, 2);

    public bool IsIdentity => this == Identity;

    public (float X, float Y, float Z) Apply(float x, float y, float z) {
        ReadOnlySpan<float> v = [x, y, z];
        return (Source(v, CanonicalX), Source(v, CanonicalY), Source(v, CanonicalZ));

        static float Source(ReadOnlySpan<float> axes, sbyte signedAxis) =>
            signedAxis < 0 ? -axes[-signedAxis - 1] : axes[signedAxis - 1];
    }
}

public enum StepProtocol : byte { None = 0, Ap203 = 203, Ap214 = 214, Ap242 = 242 }

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameNormalization {
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
}
```

## [03]-[RESEARCH]

(none)
