# [RASM_BIM]

`Rasm.Bim` is the host-neutral AEC-domain package and the SOLE GeometryGym/IFC owner — the IFC arm of the `Rasm.Element` seam, depending UP on the element seam and projecting GeometryGym into the canonical `ElementGraph` rather than re-storing a parallel element record. IFC-imported `IfcElementType` identity is reconciled through `IIfcTypeReconciler`. A resolver hit reuses a canonical Materials `Component` Type Object; a miss imports an ad-hoc Type Object with preserved IFC material/profile signatures and `PropertySource.Import`. BIM never forges catalogue identity from an IFC name/profile string alone. It owns the `SemanticProjector : IElementProjection` that lowers a GeometryGym `DatabaseIfc` into a seam `GraphDelta` (and the Bim-internal `Emit` IFC re-author), the `IfcLegality : IGraphConstraint` IFC-semantic legality validator, the IFC entity-class taxonomy + `PredefinedType` egress gate (the generated `IfcClass` roster/`IfcDomain`/seam `SchemaSpan`), the Pset/Qto template authority + `InheritanceMode` classifier, the bSDD-bound classification axis, and the host-neutral spatial-structure view over the seam graph. Enrichment precedence is source-ranked: Materials catalogue defaults (`Catalogue`) < IFC import (`Import`) < Bim-derived base quantities (`Derived`) < Rhino/user override (`User`). Type/occurrence inheritance is still controlled by `InheritanceMode`; source rank only resolves competing values at the same bag/name after inheritance is known. Round-trip preserves imported/ad-hoc IFC data unless a canonical resolver explicitly replaces it. It owns the IFC/glTF/STEP/USD exchange and validation semantics: the IFC semantic graph (in-process GeometryGym ingest, never tessellated BRep), the glTF/IFC/STEP import-export codec, the OpenUSD scene-graph codec (`UniversalSceneDescription`), the dedicated PLY codec (`Ply.Net`), the FBX/Collada/3MF scene exchange owner (`AssimpNetter`, retiring the BCL 3MF reader hand-roll), and per-importer frame normalization. It owns the building-energy-model exchange: the HBJSON/DFJSON authoring legs (`HoneybeeSchema`/`DragonflySchema`), the OSM/gbXML/IDF translator matrix (`NREL.OpenStudio.macOS-arm64`), and the `EnergyProjector : IElementProjection` raise/lower over the seam graph with content-keyed `EnergyArtifact` handoff — energy SIMULATION stays `Rasm.Compute`'s, aligned by the seam graph never coupled. The retired `BimElement`/`BimModel` element record and the stringly `PropertyBinding`/`QuantityBinding` triples are GONE — the consumer-facing element is the seam `Bake(objectNode)` fold over the `ElementGraph`. It owns the BIM-review surface: buildingSMART IDS v1.0 spec model and file-audit (`Xbim.InformationSpecifications` + `ids-lib`), the BCF 2.1/3.0 container-and-API codec (`Smino.Bcf.Toolkit`), and the 3D AABB BVH broad-phase backing clash/interference (`SwiftCollections.Lean`). It owns the georeferenced-BIM geospatial seam: the Simple-Features planar geometry algebra (`NetTopologySuite`), the shapefile/GeoPackage/GeoJSON vector codecs, the web-scale columnar GeoParquet codec (`GISBlox.IO.GeoParquet`) and the cloud-optimized streaming FlatGeobuf codec (`FlatGeobuf`), the KML/KMZ presentation codec (`SharpKml.Core`), the Mapbox Vector Tile authoring/encode/decode pair (`NetTopologySuite.IO.VectorTiles` + `NetTopologySuite.IO.VectorTiles.Mapbox`), the H3 hexagonal DGGS site-context keyer (`pocketken.H3`), the CityJSON urban-context codec (`bertt.CityJSON`), the GDAL/OGR raster+universal-vector ingest engine (`MaxRev.Gdal.Core`, osx-arm64 native), the 3D-Tiles 1.1 implicit-tiling `.subtree` availability-bitstream codec (`subtree`), and the ASPRS LAS/LAZ point-cloud decode front backing the scan-to-BIM ingest seam (`Themis.Las` uncompressed reader + `Unofficial.laszip.netstandard` LAZ decompression), with `ProjNET` remaining the CRS/datum reprojection owner. It owns the cost/schedule/planning surface: the first-class `Money`/`Currency`/`ExchangeRate` cost algebra (`NodaMoney`, meeting the seam `MeasureValue.Si` takeoff at the quantity x rate join), and the CPM topological-sort, MEP `SystemTrace` reachability, and commit-DAG common-ancestor graph walks it folds through the shared `QuikGraph` substrate (`libs/csharp/.api`). It owns the authoritative buildingSMART Pset_/Qto_ property-and-quantity template dataset (`Xbim.Properties`). It composes the kernel `Rasm` geometry, consumes the `Rasm.Compute` content-identity and companion tessellation rail at the seam, and meets `python:geometry/ifc-companion` ifcopenshell only at the wire. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/Model/faults.md): `BimFault` closed `[Union]` (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`), `Expected`-derived so the bare typed case lifts DIRECTLY onto the `Fin<T>`/`Validation<Error,T>` rail — band 2600 the `Code => FaultBand.Bim` read of the seam `FaultBand` registry, no `.ToError()` hop.
- [02]-[ELEMENTS](.planning/Model/elements.md): the GENERATED `IfcClass` `[SmartEnum<string>]` IFC entity-class taxonomy — the committed output of the offline `IfcVocabularyEmitter` reflection pass over GeometryGymIFC_Core (the full 474-entity roster, per-token `PredefinedRow` schema spans, `Instantiable` abstract-supertype rows, `ReleaseMap` the one GG-to-seam release table), the `IfcDomain` partition, the per-token `AdmitPredefined` egress gate over the seam-owned `PredefinedType`, and the `IfcRepresentation` `RepresentationContentHash` content-keyer (`BimElement`/`BimModel` RETIRED — the element is the seam `Bake` fold).
- [03]-[QUERY](.planning/Model/query.md): set-algebraic `ElementSet` query over a closed `ElementPredicate` union with `ByDomain`/`ByPredefinedType`/`ByZone` discrimination arms.
- [04]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD-bound standard-systems classification axis, local code-shape policy, `BsddResolution` live dictionary resolution, and the `IfcRelAssociatesClassification` round-trip.
- [05]-[COMPOSITION](.planning/Semantics/composition.md): the bidirectional GeometryGym↔seam material projector — `MaterialProjection.Project` lowering the `IfcMaterialSelect` surface (`IfcMaterialLayerSet`/`ProfileSet`/`ConstituentSet`/`IfcMaterial`) onto the `Rasm.Element` seam `Material` node `MaterialComposition`, and the `AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node back onto the IFC material-definition family + `IfcMaterialProperties` `Pset_Material*` + the `Associate`-edge `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [C7] (the seam-graph egress that replaces the retired Materials material wires).
- [06]-[APPEARANCE](.planning/Semantics/appearance.md): `AppearanceProjection` lowering `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading` onto the seam `AppearanceSummary` host-neutral PBR record carried as a `Node.Appearance`, reconciled with the `Rasm.Materials` OpenPBR owner at the content-key seam.
- [07]-[SPATIAL](.planning/Model/spatial.md): the `SpatialClass` spatial-interpretation vocabulary (`Rank`-derived `IsRoot`/`IsContainer`/`CanContain` the `IfcLegality` containment gate consumes) and the `SpatialStructure` derived spatial-tree VIEW over the seam's neutral `Compose` edges, traversed through the shared `QuikGraph` substrate (`BimAssembly`/`AssemblyRel`/`IfcSemanticModel` RETIRED).
- [08]-[ZONES](.planning/Model/zones.md): cross-cutting `BimZone` many-to-many grouping overlay over `IfcZone`/`IfcGroup`/`IfcSystem`, distinct from the single-parent containment tree, with the `ByZone` query arm.
- [09]-[PROPERTIES](.planning/Semantics/properties.md): the `PropertyKey` Pset/Qto template vocabulary, the bSDD-resolved template supplying each property's IFC `DataType`, the `PropertyInheritance` `InheritanceMode` classifier stamped on seam bag nodes at ingest, and the `QuantityDerivation` per-`IfcClass` base-quantity fold producing seam `QuantitySet` values (the typed `PropertyValue`/`MeasureValue` value half RETIRED to the seam).
- [10]-[VALIDATION](.planning/Review/validation.md): IDS v1.0 `IdsSpecification`/`IdsFacet` owner folding the six facets onto the `ElementPredicate` algebra for structural selection and the `Xbim.InformationSpecifications` `ValueConstraint` engine for typed value match, the `Xids.LoadBuildingSmartIDS` spec parse, the `ids-lib` `Audit` IDS-file conformance receipt, and the `IdsLib.IfcSchema` schema authority.
- [11]-[ISSUES](.planning/Review/issues.md): BCF 2.1/3.0 `BcfTopic`/`BcfComment`/`BcfViewpoint` record family over the `Smino.Bcf.Toolkit` `Worker`/`BcfBuilder` `.bcfzip` codec, and the `BcfApi` REST projection.
- [12]-[DIFF](.planning/Review/diff.md): `ModelDiff` change-set folding two seam `ElementGraph` snapshots into added/modified/removed/moved arms joined by GlobalId plus content-key, the `Modified` deltas typed by member path and `DeltaShape`.
- [13]-[STRUCTURAL](.planning/Model/structural.md): `StructuralProjection` the structural-analysis-domain READER the `SemanticProjector` composes — the ONE polymorphic `Attrs` bag over restraints (6-DOF fixity + SI springs), the full `IfcStructuralLoad` family (point/uniform/trapezoid via `IfcStructuralLoadConfiguration`), load groups/cases/results/analysis models, `AtStart`/`Station` topology discriminants, all `UnitScale`-coerced onto neutral seam edge/bag payloads the Compute frame solve reads (`AnalysisModel` store RETIRED).
- [14]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` activity network carrying `IfcTaskTime` as a NodaTime `Interval`, the `SequenceRel` `[Union]` dependency lag as a `Period`, and the `ConstructionState.At(Instant)` element-set snapshot.
- [15]-[COST](.planning/Planning/cost.md): 5D `CostItem` cost-and-resource network joining an `IfcCostValue` rate to the `QuantitySet` quantity by GlobalId, the `ConstructionResource` `[Union]`, and the `CostSchedule.Rollup` fold.
- [16]-[SYSTEMS](.planning/Model/systems.md): `DistributionSystem` the derived MEP connectivity VIEW over the seam's `NodeId`-keyed neutral `Connect`/`Generic` edges — `DistributionSystemKind` `[SmartEnum]`, the `DistributionPort`/`FlowEdge` port view (flow direction off the projector-synthesized port bag), the `(MembershipKey, TopologyKey)` content identity, the directed `SystemTrace` reachability fold, and the BVH-broad-phase `InterferenceCheck` (`PortConnection` `[Union]` RETIRED).
- [17]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): scan-to-BIM `ReconstructionProjector : IElementProjection` folding kernel-registered `SegmentedCloud` rows into a seam `GraphDelta` of `Node.Object` occurrences — the six-arm `ReconstructionPrimitive` `[Union]` (plane/sphere/cylinder/cone/torus/freeform), the frozen `ElementClassifier` table + `AsprsBias` ASPRS policy admitting every landing through `AdmitPredefined`, the kernel-`ContentHash` `ReconstructionLineage` key, and the dual-engine `LasIngest` LAS/LAZ decode front (`Themis.Las` uncompressed + `Unofficial.laszip.netstandard` compressed) the kernel registration/segmentation consumes.
- [18]-[GEOREFERENCE](.planning/Semantics/georeference.md): the `GeoReferenceProjector` lowering `IfcMapConversion`/`IfcMapConversionScaled`/`IfcProjectedCRS` into the seam `GeoReference` (full field set, per-axis scale, EPSG parse + fault-on-unresolvable), and the `GeoTransform.Reproject` `ProjNET` datum-to-datum leg over the seam value (the `GeoReference`/`ProjectedCrs` value-objects RETIRED to the seam).
- [19]-[FORMAT](.planning/Exchange/format.md): `InterchangeFormat`/`InterchangeCodec`/`KhrExtension` format-codec-extension table (the `Serialization` IFC column, the `dotbim`/KML/KMZ/MVT rows, capability columns naming their realizing arm), `FrameNormalization` per-importer frame coercion, and the `Detect` row resolution.
- [20]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold — managed glTF mesh decode (SharpGLTF, Draco/meshopt pre-decode), the `geometry3Sharp` OBJ/STL/OFF arm, the dedicated `Ply.Net` PLY decode, the `dotbim` `.bim` pooled-mesh decode (pool x placed `Element` fold, identity restored `Info["globalId"]` first / `Guid` second) plus its `DotbimProjector` semantic arm, the `AssimpNetter` FBX/Collada/3MF scene decode, the `ACadSharp` DWG/DXF mesh arm, the `UsdStage` OpenUSD decode, `ImportIfc` the ONE bytes→`DatabaseIfc` decode (schema-sniffed through the railed `Sniff`), the in-process `StepReader` Part-21 semantic leg, and the Speckle `Base` object-graph seam onto the pooled/instanced `ImportedGeometry` carrier (retiring the BCL `PlyReader`/`ThreeMfReader` hand-rolls).
- [21]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — one TOTAL codec `Switch` over the `ExportPayload` `Soup`/`Scene` union, the per-element `GlbScene` author (GlobalId-named nodes, mesh-pool instancing, `EXT_mesh_gpu_instancing` threshold), GLB with Draco/meshopt encode, the `dotbim` `.bim` instancing wire, the `AssimpNetter` FBX/Collada and `UsdStage` OpenUSD scene emit, IFC STEP/XML/JSON serialization, the graph-sourced `CobieEmit` COBie FM-handover XLSX (`Xbim.CobieExpress` authored `Instances.New` from the seam graph, `ExportToTable` seal), per-tile `EXT_structural_metadata` `TileMetadata` author, the `subtree` 3D-Tiles `.subtree` availability bitstream, and the `InterchangePolicy`/`ExportArtifact` carriers.
- [22]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [23]-[WIRE](.planning/Exchange/wire.md): the cross-runtime IFC interchange wire — one host-free `IfcWire` content-keyed artifact (`Seal` egress, `Admit` ingress, `Negotiate` serialization selection) carrying the seam `ElementGraph` re-authored to IFC bytes (STEP/ifcXML/ifcJSON) through `SemanticProjector.Emit`, stamped `ContentAddress.OfGraph`, plus the `WireParity` cross-runtime golden-corpus leg — the Python and TypeScript peers decode the same IFC serialization Bim emits.
- [24]-[CONNECTION](.planning/Semantics/connection.md): the `ConnectionProjection` realizing-element-detail reader the `Projection/semantic` `SemanticProjector` composes — lowering the whole GeometryGym realizing surface (`IfcMechanicalFastener`/`IfcFastener`/`IfcReinforcingBar`/`IfcReinforcingMesh`/`IfcTendon`/`IfcTendonAnchor`/`IfcBearing`) onto neutral `DetailSchema.Realization` seam `PropertySet` bags bound to realizing `Object` nodes by `Assign.PropertyDefinition` edges. Materials reaches the same shape through first-class `ComponentFamily` rows (`reinforcement`/`fastener`/`connector`/`joint`) in the single Component projector, not through a retired Connection axis.
- [25]-[COORDINATION](.planning/Review/coordination.md): `CoordinationRule` `[Union]` rule engine over the `ElementPredicate` algebra, the `ClashProposal` fold over the `Model/systems#INTERFERENCE` evidence, the `ImpactReport` over two diffs, and the `SignOff` `[SmartEnum]` BCF state machine — the BCF issue-board DOMAIN owner the Persistence/AppUi relocations settle here.
- [26]-[GEOSPATIAL](.planning/Semantics/geospatial.md): host-neutral georeferenced-BIM site-context PROJECTOR — the `GeoFeature` canonical row over the `NetTopologySuite` Simple-Features planar algebra, the `GeoModel` `STRtree` broad-phase, the `GeoVector`/`GeoRaster` universal ingest (managed shapefile/CityJSON/GeoPackage/GeoJSON plus the `MaxRev.Gdal.Core` OGR universal driver + GeoTIFF/COG/DEM raster), the OGR↔NTS WKB bridge, and the `GeoFeature.ToObject`/`GeoRaster.ToCoverage` site-context projection onto seam `Object`/`Coverage` nodes through a `GraphDelta`, reprojected through the seam `GeoReference` `ProjNET` leg.
- [27]-[VERSIONING](.planning/Review/versioning.md): content-addressed model history — the `BimCommit` commit object whose identity IS the `Review/diff` `ElementFingerprint` set, the `BimBranch` ref, the `BimRepository` commit-DAG (`Commit`/`History`/`CommonAncestor`), and the `Version.Merge` three-way fold reconciling two revisions against their common ancestor into a merged graph plus a closed `MergeConflict` `[Union]` the `Review/coordination` `SignOff` resolves; the branching counterpart to the linear `Review/diff` `AuditTrail`, meeting the `Rasm.Persistence` Version owner at the content-key wire.
- [28]-[SEMANTIC](.planning/Projection/semantic.md): `SemanticProjector : IElementProjection` lowering a GeometryGym `DatabaseIfc` into the seam `GraphDelta` (rooted `NodeId` mint + 1:1 IFC `GlobalId`, `OwnerHistory`/`StepHeader`, the railed `ReleaseLower` schema lowering, per-projection `UnitScale` SI coercion, synthesized port/structural entity-attribute bags), `IIfcTypeReconciler` type identity reconciliation, `IIfcProfileStore` imported profile preservation/egress, and `IfcLegality : IGraphConstraint` IFC-semantic legality (relationship rules + `CanContain` rank arm + the two roster vocabulary arms).
- [29]-[RELATIONS](.planning/Projection/relations.md): `IfcRelKind` the full `IfcRel*` roster `[SmartEnum<string>]` (axis + sub-kind + inverse-attribute names + `Author` egress mint) and `EdgeProjection.All` folding every relationship family — decomposition/containment/ordered-nest ordinals, void/fill, assignment, structural member/activity payloads via `StructuralProjection.Attrs(rel)`, space boundaries, material `Associate` usages — onto the seam's neutral 5-kind edge algebra plus `Generic`.
- [30]-[EGRESS](.planning/Projection/egress.md): the Bim-internal `SemanticProjector.Emit` IFC re-author — the railed `ReleaseRaise` (`ReleaseMap.Raise`) + `Sniff` (STEP/ifcXML/ifcJSON schema probe), the `Instantiable` + per-token `AdmitPredefined` egress gate [C6][H8], typed property/quantity raise, relationship/material/classification re-author, and the ordered-nest `NestOrdinal` re-emit.
- [31]-[ENERGY](.planning/Exchange/energy.md): the building-energy-model exchange owner — `EnergyExchange.Apply` over the closed `EnergyOp` `[Union]` (`Raise`/`Lower`/`Translate`), the `EnergyProjector : IElementProjection` raising HBJSON/DFJSON/OSM/gbXML/IDF onto the seam graph in the exact `IfcRelSpaceBoundary`/FootPrint/`MaterialComposition` shape the Compute energy runner reads, the `EnergyDerive` BIM-to-BEM lower (graph → honeybee envelope + library, graph → dragonfly massing), the `EnergyTranslate` OSM-centric translator matrix, and the content-keyed `EnergyArtifact` object-plane handoff — energy SIMULATION stays `Rasm.Compute`'s, the python energy plane meets ONLY at content-keyed document bytes.

## [02]-[DOMAIN_PACKAGES]

The IFC/glTF/STEP/USD interchange, BIM-review, and geospatial domain packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[IFC_SEMANTIC]:
- `GeometryGymIFC_Core`

[STRUCTURAL_ANALYSIS_EXCHANGE]:
- `StructuralAnalysisFormat` — SAF/XLSX structural-analysis exchange over the seam structural payloads `Model/structural.md` defines (members, supports, load cases, combinations, result tables lowered from the neutral `Generic` edge/bag attrs onto `ExcelModel`), an `Exchange/format` candidate row until its import/export arms land — never a canonical model beside the seam graph.

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

[CLASSIFICATION_DICTIONARY]:
- bSDD `Dictionaries API` — the live buildingSMART Data Dictionary REST service (`https://api.bsdd.buildingsmart.org/`, MIT); a hand-thin read-only HTTP client over the Compute transport resolving standard classification systems and their class-to-property mappings, with no manifest pin and no managed assembly.

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
- `HoneybeeSchema` — the HBJSON object graph (`Model.FromJson`/`ToJson`, the energy library, the abridged-reference model) the `Exchange/energy` raise/lower composes to operator depth.
- `DragonflySchema` — the DFJSON urban-massing layer (`Building`/`Story`/`Room2D`) composing the honeybee vocabulary by identifier; the `Exchange/energy` massing arm.
- `OpenStudio` via `NREL.OpenStudio.macOS-arm64` — the OSM/IDF object store + the `VersionTranslator`/gbXML/EnergyPlus translator matrix backing the `Exchange/energy` OSM-family raise and `Translate` rows; the SIMULATION half of the one central pin is `Rasm.Compute/Analysis/energy`'s, aligned by the seam graph never coupled.

[STRUCTURAL_TAXONOMY]:
- `VividOrange.Loads`
- `VividOrange.Cases`
- `VividOrange.Stages`
- `VividOrange.Countries`

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

[COST_MONEY]:
- `NodaMoney`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`.

[ELEMENT_SEAM]:
- `Rasm.Element` — the lowest-AEC element seam this folder depends UP on and implements: the `ElementGraph`/`Node`/`Relationship`/`GraphDelta` canonical graph, the `IElementProjection`/`IGraphConstraint` contracts the `SemanticProjector`/`IfcLegality` implement, and the seam-owned typed value family (`PropertyValue`/`MeasureValue`/`Dimension`, `Classification`, `PredefinedType`, `GeoReference`/`ProjectedCrs`, `OwnerHistory`, `StepHeader`, `InheritanceMode`, `RepresentationContentHash`).

[SEAM_GENERATORS]:
- `Riok.Mapperly` — compile-time `ElementGraph`↔DTO/proto boundary transcription (`[MapDerivedType]` over the `Node`/`Relationship` `[Union]`), shared substrate in `libs/csharp/.api`.
- `Generator.Equals` — compile-time node/edge structural equality + member-level `Inequalities` diff (the `Emit` `ChangeAction` derivation), shared substrate in `libs/csharp/.api`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `System.IO.Hashing`

[BUFFER_POOLING]:
- `CommunityToolkit.HighPerformance` — `ArrayPoolBufferWriter<T>`/`MemoryOwner<T>` pooled staging behind the Draco/meshopt encode kernels and the import `MeshSoup` accumulation, shared substrate in `libs/csharp/.api`.

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[GRAPH_ALGORITHM]:
- `QuikGraph` — the pure-managed graph containers + `AlgorithmExtensions` facade the Bim CPM topological-sort (`Planning/schedule`), MEP `SystemTrace` reachability (`Model/systems`), commit-DAG common-ancestor (`Review/versioning`), and coordination impact/`Reachable`-rule transitive-closure (`Review/coordination`) walks fold a transient graph through, shared substrate in `libs/csharp/.api` (the seam `ElementGraph` topology view and the Persistence `Query/topology` lane co-consume it).

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
