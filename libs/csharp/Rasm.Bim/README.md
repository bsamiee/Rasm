# [RASM_BIM]

`Rasm.Bim` owns host-neutral openBIM semantics over the IFC vocabularies, model readers, 4D/5D delivery networks, IDS/BCF/clash/diff review, content-addressed versioning, energy-model exchange, and geospatial site context. Its bar is round-trip fidelity at coordination grade: a foreign model lowers onto the seam graph without semantic loss, re-emits as legal IFC through the per-token admission gate, and every review verdict lands as a typed receipt the issue board and review planes consume directly.

Every carrier sub-domain lowers its foreign format to seam-graph, content-key, or receipt currency.

## [01]-[ROUTER]

[ENERGY]:
- [01]-[DERIVE](.planning/Energy/derive.md): BIM-to-BEM lower — honeybee building envelope, dragonfly massing, and the OSM-centric translation matrix.
- [02]-[EXCHANGE](.planning/Energy/exchange.md): `EnergyExchange.Apply` folding raise, lower, and translate onto content-keyed document carriers.
- [03]-[PROJECTOR](.planning/Energy/projector.md): `EnergyProjector` raise landing every energy format in the shape the Compute runner reads.
- [04]-[RESULTS](.planning/Energy/results.md): `EnergyResults.Admit` landing a run's typed receipt as producer-authored result bags on the graph.

[EXCHANGE]:
- [05]-[EVENTS](.planning/Exchange/events.md): Announcement projection subscribing fired `BimFact` rows onto the kernel message-envelope owner.
- [06]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — one total codec switch over scene, IFC, COBie, and tile targets.
- [07]-[FORMAT](.planning/Exchange/format.md): Format-codec-extension table with per-importer frame normalization and sniffed row resolution.
- [08]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold landing every decode arm on the pooled imported-geometry carrier.
- [09]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): Scan-to-BIM folding segmented clouds into seam occurrences over the LAS/LAZ ingest front.
- [10]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [11]-[WIRE](.planning/Exchange/wire.md): Host-free content-keyed `IfcWire` interchange artifact the Python and TypeScript peers decode.

[MODEL]:
- [12]-[ELEMENTS](.planning/Model/elements.md): Generated `IfcClass` taxonomy with release map, domain partition, and predefined egress gate.
- [13]-[FAULTS](.planning/Model/faults.md): `BimFault` closed union lifting every rejection onto the seam fault band's typed rail.
- [14]-[OBSERVABILITY](.planning/Model/observability.md): `BimPoint` roster, `BimHooks` rail, `BimTelemetry` instruments, corpus bench claims.
- [15]-[QUERY](.planning/Model/query.md): Set-algebraic `ElementSet` query over a closed predicate union, `PredicateWire`, `StorePlan` push-down.
- [16]-[SPATIAL](.planning/Model/spatial.md): Spatial rank vocabulary, containment tree over seam compose edges, adjacency, and linear positioning.
- [17]-[STRUCTURAL](.planning/Model/structural.md): Structural-analysis reader lowering restraints, loads, and topology onto seam payloads.
- [18]-[SYSTEMS](.planning/Model/systems.md): Derived MEP connectivity — port flow edges, directed system trace, demand folds, interference check.
- [19]-[ZONES](.planning/Model/zones.md): Cross-cutting `BimZone` many-to-many overlay distinct from the single-parent containment tree.

[PLANNING]:
- [20]-[COST](.planning/Planning/cost.md): 5D `CostItem` resource network and 6D `CarbonEstimate` carbon rollup over the material-true takeoff.
- [21]-[PROGRESS](.planning/Planning/progress.md): Scan-derived physical-progress verification joining reconstructed occurrences to the task network.
- [22]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` network with task-time intervals, lags, and phase-partitioned snapshots.

[PROJECTION]:
- [23]-[EGRESS](.planning/Projection/egress.md): `SemanticProjector.Emit` IFC re-author — release raise, per-token admission gate, scoped emit.
- [24]-[RELATIONS](.planning/Projection/relations.md): `IfcRelKind` roster folding every relationship family onto the seam edge algebra.
- [25]-[SEMANTIC](.planning/Projection/semantic.md): `SemanticProjector` GeometryGym-to-seam lowering under `IfcLegality`, fidelity-drop ledger.

[REVIEW]:
- [26]-[COORDINATION](.planning/Review/coordination.md): Clash rule engine, impact report, and sign-off machine owning the BCF issue board.
- [27]-[DIFF](.planning/Review/diff.md): `ModelDiff` folding two graph snapshots into typed added, modified, removed, and moved arms.
- [28]-[ISSUES](.planning/Review/issues.md): BCF topic, comment, and viewpoint family over the `.bcfzip` codec and the BCF-API request projection.
- [29]-[VALIDATION](.planning/Review/validation.md): Two-tier model-QA — template-audit baseline beneath the authored IDS facet fold.
- [30]-[VERSIONING](.planning/Review/versioning.md): Content-addressed model history — commit DAG and three-way merge with typed conflicts.

[SEMANTICS]:
- [31]-[APPEARANCE](.planning/Semantics/appearance.md): Surface-style lowering onto the seam PBR summary reconciled at the Materials content key.
- [32]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD classification axis — live resolution, association round-trip, enrichment.
- [33]-[COMPOSITION](.planning/Semantics/composition.md): Bidirectional material projector between IFC material selects and seam composition.
- [34]-[CONNECTION](.planning/Semantics/connection.md): `ConnectionProjection` lowering realizing elements onto seam detail bags and edges.
- [35]-[GEOREFERENCE](.planning/Semantics/georeference.md): Map-conversion and CRS lowering onto seam `GeoReference` with federation preflight.
- [36]-[GEOSPATIAL](.planning/Semantics/geospatial.md): Site-context projector — Simple-Features algebra and universal vector/raster ingest.
- [37]-[PROPERTIES](.planning/Semantics/properties.md): Pset/Qto template authority, inheritance classifier, quantity derivation, conformance audit.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[MODEL_INTERCHANGE]:
- `GeometryGymIFC_Core` — sole IFC semantic-model surface.
- `subtree` — 3D-Tiles implicit-availability bitstream.
- `UniversalSceneDescription` — OpenUSD scene decode and emit.
- `AssimpNetter` — FBX/Collada/3MF scene decode and emit.
- `Ply.Net` — dedicated PLY decode.
- `dotbim` — lightweight `.bim` mesh-and-metadata interchange.
- `Openize.Drako` — Draco mesh compression.
- `Themis.Las` — uncompressed LAS point-cloud decode.
- `StructuralAnalysisFormat` — SAF/XLSX exchange over seam structural payloads.
- `Xbim.CobieExpress` — COBie FM-handover emit.
- `Xbim.IO.CobieExpress`
- `Xbim.CobieExpress.Exchanger`

[REVIEW]:
- `Xbim.InformationSpecifications` — IDS value-constraint engine.
- `ids-lib` — IDS-file conformance audit and schema authority.
- `Smino.Bcf.Toolkit` — BCF `.bcfzip` codec.
- `SwiftCollections.Lean` — BVH broad phase behind the interference check.

[DOMAIN_VOCABULARY]:
- `Xbim.Properties` — Pset/Qto template definitions.
- `bSDD Dictionaries API` — live buildingSMART dictionary REST over the Compute transport; no manifest pin, no assembly.
- `BrickSchema.Net` — building-systems ontology.
- `VividOrange.Loads`
- `VividOrange.Cases`
- `VividOrange.Stages`
- `VividOrange.Countries`
- `VividOrange.IStandards` — transitive through `VividOrange.Cases`; no direct manifest row.
- `NodaMoney` — 5D cost-value money type.

[ENERGY_EXCHANGE]:
- `HoneybeeSchema` — HBJSON object graph composed to operator depth.
- `DragonflySchema` — DFJSON massing composing honeybee by identifier.

[GEOSPATIAL]:
- `NetTopologySuite.IO.Esri.Shapefile`
- `NetTopologySuite.IO.VectorTiles`
- `NetTopologySuite.IO.VectorTiles.Mapbox`
- `GISBlox.IO.GeoParquet`
- `FlatGeobuf`
- `SharpKml.Core`
- `bertt.CityJSON`
- `MaxRev.Gdal.Core` — OGR universal vector driver and GeoTIFF/COG/DEM raster ingest.
- `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- `ProjNET` — datum-to-datum reprojection leg.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`
- `NodaTime`
- `System.IO.Hashing` — reached only through the kernel content-hash mint every content key seeds from.
- `UnitsNet`
- `CommunityToolkit.HighPerformance` — pooled buffer staging behind the mesh encoders.
- `QuikGraph` — CPM sort, system-trace reachability, commit-DAG ancestor, and coordination closure walks.

[EXCHANGE_SUBSTRATE]:
- `ACadSharp` — DWG/DXF mesh-read leg into `ImportedGeometry`; Fabrication holds the profile read, AppUi the drafting write.
- `SharpGLTF.Core` — glTF schema I/O behind the `Exchange/export` emit and the import decode leg.
- `SharpGLTF.Toolkit` — builds the vertex-fragment, mesh, scene, and material heads feeding `ToGltf2`.
- `SharpGLTF.Runtime` — scene templatization and per-instance decode over an imported `ModelRoot`.
- `SharpGLTF.Ext.3DTiles` — authors the 3D Tiles overlay at `Exchange/export#TILE_METADATA`.
- `Speckle.Sdk` — receive-side `Base` graph: `Flatten` traversal, display values, metre conversion.
- `Speckle.Objects` — display-mesh geometry and the `DataObject` host-object family the import seam folds.
- `Unofficial.laszip.netstandard` — compressed-LAZ decode leg of the dual-engine `Exchange/reconstruct` ingest front.
- `NREL.OpenStudio.macOS-arm64` — drives the OSM/IDF exchange leg: robust load, save, version upgrade, and the gbXML/SDD semantic bridges.

[MESH_PROCESSING]:
- `Alimer.Bindings.MeshOptimizer` — meshopt compression behind the mesh encoders.
- `geometry3Sharp` — OBJ/STL/OFF text-mesh decode arm of the `MeshText` interchange codec.

[PLANAR_GEOSPATIAL]:
- `NetTopologySuite` — OGC Simple-Features planar algebra behind the geospatial seam.
- `NetTopologySuite.IO.GeoJSON4STJ` — carries the STJ GeoJSON codec leg of the geospatial seam for site context and web projection.
- `NetTopologySuite.IO.GeoPackage` — carries the GeoPackage geometry-BLOB leg for site and context ingest.
- `pocketken.H3` — keys the `Semantics/geospatial#GEOSPATIAL_SEAM` DGGS arm, the coarse `ulong` bucket beside the `STRtree`.

[WIRE_SEAM]:
- `Riok.Mapperly` — compile-time boundary transcription over the seam unions.
- `Generator.Equals` — structural equality and member diff behind the emit change derivation.
- `CloudNative.CloudEvents` — message-envelope type the announcement projection mints through `Rasm/Domain/event`; transport bindings stay app-tier.
- `System.Text.Json` — generated wire contexts behind the exchange message envelopes, review records, and the GeoJSON seam.
