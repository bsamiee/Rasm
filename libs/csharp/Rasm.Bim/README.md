# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package owning the universal BIM object model and IFC/glTF/STEP/USD exchange and validation semantics. It owns the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec, the OpenUSD scene-graph codec (`UniversalSceneDescription`), the dedicated PLY codec (`Ply.Net`), the FBX/Collada/3MF scene exchange owner (`AssimpNetter`, retiring the BCL 3MF reader hand-roll), per-importer frame normalization, the `BimElement` vocabulary, the `ElementSet` query algebra, the bSDD-bound classification axis, and the host-neutral assembly tree. It owns the BIM-review surface: buildingSMART IDS v1.0 spec model and file-audit (`Xbim.InformationSpecifications` + `ids-lib`), the BCF 2.1/3.0 container-and-API codec (`Smino.Bcf.Toolkit`), and the 3D AABB BVH broad-phase backing clash/interference (`SwiftCollections.Lean`). It owns the georeferenced-BIM geospatial seam: the Simple-Features planar geometry algebra (`NetTopologySuite`), the shapefile/GeoPackage/GeoJSON vector codecs, the web-scale columnar GeoParquet codec (`GISBlox.IO.GeoParquet`) and the cloud-optimized streaming FlatGeobuf codec (`FlatGeobuf`), the KML/KMZ presentation codec (`SharpKml.Core`), the Mapbox Vector Tile authoring/encode/decode pair (`NetTopologySuite.IO.VectorTiles` + `NetTopologySuite.IO.VectorTiles.Mapbox`), the H3 hexagonal DGGS site-context keyer (`pocketken.H3`), the CityJSON urban-context codec (`bertt.CityJSON`), the GDAL/OGR raster+universal-vector ingest engine (`MaxRev.Gdal.Core`, osx-arm64 native), the 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream codec (`subtree`), and the ASPRS LAS/LAZ point-cloud decode front backing the scan-to-BIM ingest seam (`Themis.Las` uncompressed reader + `Unofficial.laszip.netstandard` LAZ decompression), with `ProjNET` remaining the CRS/datum reprojection owner. It owns the cost/schedule/planning surface: the first-class `Money`/`Currency`/`ExchangeRate` cost algebra (`NodaMoney`, meeting `UnitsNet` at the quantity x rate join), and the shared directed-graph algorithm owner (`QuikGraph`) collapsing the CPM topological-sort, MEP `SystemTrace` reachability, and commit-DAG common-ancestor walks. It owns the authoritative buildingSMART Pset_/Qto_ property-and-quantity template dataset (`Xbim.Properties`). It composes the kernel `Rasm` geometry, consumes the `Rasm.Compute` content-identity and companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/Model/faults.md): `BimFault` closed `[Union]` (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`) every Bim entrypoint lowers onto the `Fin<T>` rail through `.ToError()`.
- [02]-[ELEMENTS](.planning/Model/elements.md): `BimElement` record, the `IfcClass`/`IfcDomain` entity-class vocabulary, the `PredefinedType` sub-class discriminant, `BimType` type-occurrence factoring, `BimModel` collection, and the `Project` fold from the IFC semantic graph.
- [03]-[QUERY](.planning/Model/query.md): set-algebraic `ElementSet` query over a closed `ElementPredicate` union with `ByDomain`/`ByPredefinedType`/`ByZone` discrimination arms.
- [04]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD-bound standard-systems classification axis, local code-shape policy, `BsddResolution` live dictionary resolution, and the `IfcRelAssociatesClassification` round-trip.
- [05]-[COMPOSITION](.planning/Semantics/composition.md): host-neutral `BimMaterial` construction-material composition — `BimMaterialComposition` `[Union]` over the IFC layered/profiled/constituent material sets and the thickness-keyed `MaterialLayer`.
- [06]-[APPEARANCE](.planning/Semantics/appearance.md): `BimAppearance` host-neutral PBR record projected from `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`, reconciled with the `Rasm.Materials` OpenPBR owner at the content-key seam.
- [07]-[STRUCTURE](.planning/Model/structure.md): host-neutral spatial-structure tree and the closed `AssemblyRel` decomposition algebra.
- [08]-[ZONES](.planning/Model/zones.md): cross-cutting `BimZone` many-to-many grouping overlay over `IfcZone`/`IfcGroup`/`IfcSystem`, distinct from the single-parent containment tree, with the `ByZone` query arm.
- [09]-[PROPERTIES](.planning/Semantics/properties.md): typed `PropertySet`/`QuantitySet` keyed vocabulary, the bSDD-resolved `PropertyKey` template, the `QuantitySet.Derive` per-`IfcClass` base-quantity fold over the `GeometryHandle` with `UnitsNet` SI-base coercion, the type-vs-occurrence inheritance fold, and the `IfcRelDefinesByProperties` round-trip.
- [10]-[VALIDATION](.planning/Review/validation.md): IDS v1.0 `IdsSpecification`/`IdsFacet` owner folding the six facets onto the `ElementPredicate` algebra for structural selection and the `Xbim.InformationSpecifications` `ValueConstraint` engine for typed value match, the `Xids.LoadBuildingSmartIDS` spec parse, the `ids-lib` `Audit` IDS-file conformance receipt, and the `IdsLib.IfcSchema` schema authority.
- [11]-[ISSUES](.planning/Review/issues.md): BCF 2.1/3.0 `BcfTopic`/`BcfComment`/`BcfViewpoint` record family over the `Smino.Bcf.Toolkit` `Worker`/`BcfBuilder` `.bcfzip` codec, and the `BcfApi` REST projection.
- [12]-[DIFF](.planning/Review/diff.md): `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms joined by GlobalId plus content-key.
- [13]-[STRUCTURAL](.planning/Model/structural.md): host-neutral `AnalysisModel` structural-analysis graph projecting `IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember`/load/support, binding the physical `BimElement` by GlobalId for the Compute solver.
- [14]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` activity network carrying `IfcTaskTime` as a NodaTime `Interval`, the `SequenceRel` `[Union]` dependency lag as a `Period`, and the `ConstructionState.At(Instant)` element-set snapshot.
- [15]-[COST](.planning/Planning/cost.md): 5D `CostItem` cost-and-resource network joining an `IfcCostValue` rate to the `QuantitySet` quantity by GlobalId, the `ConstructionResource` `[Union]`, and the `CostSchedule.Rollup` fold.
- [16]-[SYSTEMS](.planning/Model/systems.md): `DistributionSystem` MEP connectivity graph with `DistributionSystemKind` `[SmartEnum]`, the `PortConnection` `[Union]` over `IfcRelConnectsPortToElement`/`IfcRelConnectsPorts`, and the `SystemTrace` graph fold.
- [17]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): scan-to-BIM `ReconstructionPrimitive` `[Union]` (plane/cylinder/torus/freeform) folding the kernel-registered segmented cloud into `BimElement` rows with an `ElementPredicate`-classified `IfcClass` and source-cloud lineage key, over the `Themis.Las` `LasIngest` ASPRS LAS decode front the kernel registration/segmentation consumes.
- [18]-[GEOREFERENCE](.planning/Semantics/georeference.md): host-neutral `GeoReference` record projected from `IfcMapConversion`/`IfcProjectedCRS`, the `FrameNormalization.Georeference` CRS overload, and the `GeoReference.Reproject` `ProjNET` datum-to-datum geodetic leg.
- [19]-[FORMAT](.planning/Exchange/format.md): `InterchangeFormat`/`InterchangeCodec`/`KhrExtension` format-codec-extension table, `FrameNormalization` per-importer frame coercion, and the `Detect` row resolution.
- [20]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold — managed glTF mesh decode (SharpGLTF), the `geometry3Sharp` OBJ/STL/OFF arm, the dedicated `Ply.Net` PLY decode, the `AssimpNetter` FBX/Collada/3MF scene decode, the `UsdStage` OpenUSD decode, the in-process semantic IFC/IFC5/STEP `IfcSemanticModel` graph, and the Speckle `Base` object-graph seam onto the canonical carriers (retiring the BCL `PlyReader`/`ThreeMfReader` hand-rolls).
- [21]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — GLB mesh-and-scene with Draco/meshopt encode, the `AssimpNetter` FBX/Collada/3MF and `UsdStage` OpenUSD scene emit, IFC STEP/XML/JSON serialization, per-tile `EXT_structural_metadata` `TileMetadata` author, the `subtree` 3D-Tiles `.subtree` availability bitstream, and the `InterchangePolicy`/`ExportArtifact` carriers.
- [22]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [23]-[WIRE](.planning/Exchange/wire.md): host-free `BimWire` JSON projection of the generated owners through the Thinktecture converters, the source-generated `BimWireContext`, and the content-keyed `BimModel` snapshot the Python and TypeScript peers decode.
- [24]-[CONNECTION](.planning/Semantics/connection.md): host-neutral `ConnectionDetail` realizing-element joint `[Union]` (`Bolted`/`Welded`/`Bearing`/`Cast`) over `IfcRelConnectsWithRealizingElements`, and the `csharp:Rasm.Materials/Connection` `ConnectionItem` axis IFC 4.3 wire (the `IfcReinforcingBar`/`IfcMechanicalFastener` author, the `MaterialAssignment` trichotomy, the sustainability Psets).
- [25]-[COORDINATION](.planning/Review/coordination.md): `CoordinationRule` `[Union]` rule engine over the `ElementPredicate` algebra, the `ClashProposal` fold over the `Model/systems#INTERFERENCE` evidence, the `ImpactReport` over two diffs, and the `SignOff` `[SmartEnum]` BCF state machine — the BCF issue-board DOMAIN owner the Persistence/AppUi relocations settle here.
- [26]-[GEOSPATIAL](.planning/Semantics/geospatial.md): host-neutral georeferenced-BIM site-context owner — the `GeoFeature` canonical row over the `NetTopologySuite` Simple-Features planar algebra, the `GeoModel` `STRtree` broad-phase, the `GeoVector`/`GeoRaster` universal ingest (managed shapefile/CityJSON/GeoPackage/GeoJSON plus the `MaxRev.Gdal.Core` OGR universal driver + GeoTIFF/COG/DEM raster), the OGR↔NTS WKB bridge, and the `GeoFeature.ToElement` site-context `BimElement` projection reprojected through the `GeoReference` `ProjNET` leg.
- [27]-[VERSIONING](.planning/Review/versioning.md): content-addressed model history — the `BimCommit` commit object whose identity IS the `Review/diff` `ElementFingerprint` set, the `BimBranch` ref, the `BimRepository` commit-DAG (`Commit`/`History`/`CommonAncestor`), and the `Version.Merge` three-way fold reconciling two revisions against their common ancestor into a merged graph plus a closed `MergeConflict` `[Union]` the `Review/coordination` `SignOff` resolves; the branching counterpart to the linear `Review/diff` `AuditTrail`, meeting the `Rasm.Persistence` Version owner at the content-key wire.

## [02]-[DOMAIN_PACKAGES]

The IFC/glTF/STEP/USD interchange, BIM-review, and geospatial domain packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[IFC_SEMANTIC]:
- `GeometryGymIFC_Core`

[STRUCTURAL_ANALYSIS_EXCHANGE]:
- `StructuralAnalysisFormat` — SAF/XLSX structural-analysis exchange for the host-neutral `AnalysisModel` graph, binding structural members, supports, load cases, combinations, and result tables without making SAF the canonical model.

[LIGHTWEIGHT_BIM_EXCHANGE]:
- `dotbim` — lightweight `.bim` mesh+metadata interchange for preview, portable issue payloads, and low-friction external model exchange beside IFC/glTF/USD.

[GLTF_CODEC]:
- `SharpGLTF.Core`
- `SharpGLTF.Toolkit`
- `SharpGLTF.Runtime`
- `SharpGLTF.Ext.3DTiles`

[TILESET_IMPLICIT]:
- `subtree`

[USD_SCENE]:
- `UniversalSceneDescription`

[CAD_INTERCHANGE]:
- `ACadSharp`

[MESH_COMPRESSION]:
- `Openize.Drako`
- `Alimer.Bindings.MeshOptimizer`

[MESH_TEXT]:
- `geometry3Sharp`
- `Ply.Net`

[SCENE_EXCHANGE]:
- `AssimpNetter`

[POINT_CLOUD]:
- `Themis.Las`
- `Unofficial.laszip.netstandard`

[IDS_VALIDATION]:
- `Xbim.InformationSpecifications`
- `ids-lib`

[PROPERTY_TEMPLATES]:
- `Xbim.Properties`

[COBIE_EXCHANGE]:
- `Xbim.CobieExpress`
- `Xbim.IO.CobieExpress`
- `Xbim.CobieExpress.Exchanger`

[BCF_ISSUES]:
- `Smino.Bcf.Toolkit`

[CLASH_SPATIAL]:
- `SwiftCollections.Lean`

[BUILDING_SYSTEMS_ONTOLOGY]:
- `BrickSchema.Net`

[ENERGY_MODEL_EXCHANGE]:
- `HoneybeeSchema`
- `DragonflySchema`
- `OpenStudio` via `NREL.OpenStudio.macOS-arm64`

[STRUCTURAL_TAXONOMY]:
- `VividOrange.Loads`
- `VividOrange.Cases`
- `VividOrange.Stages`
- `VividOrange.Countries`

[GRAPH_ALGORITHM]:
- `QuikGraph`

[GEOSPATIAL]:
- `NetTopologySuite`
- `NetTopologySuite.IO.Esri.Shapefile`
- `NetTopologySuite.IO.GeoPackage`
- `NetTopologySuite.IO.GeoJSON4STJ`
- `GISBlox.IO.GeoParquet`
- `FlatGeobuf`
- `SharpKml.Core`
- `NetTopologySuite.IO.VectorTiles`
- `NetTopologySuite.IO.VectorTiles.Mapbox`
- `pocketken.H3`
- `bertt.CityJSON`
- `MaxRev.Gdal.Core`
- `MaxRev.Gdal.MacosRuntime.Minimal.arm64`

[SPECKLE_SYNC]:
- `Speckle.Sdk`
- `Speckle.Objects`

[GEODETIC]:
- `ProjNET`

[QUANTITY_UNIT]:
- `UnitsNet`

[COST_MONEY]:
- `NodaMoney`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; full registry and version ownership live in `libs/csharp/.planning/README.md`, with decompile evidence in the folder `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `SharpFuzz`
- `Verify.XunitV3`
