# [RASM_BIM]

`Rasm.Bim` owns host-neutral BIM semantics over the `Rasm.Element` seam: it projects GeometryGym models into canonical `ElementGraph` deltas, re-emits IFC through `SemanticProjector.Emit`, gates legality through `IfcLegality`, and mints the IFC class, property, classification, and spatial vocabularies every consumer composes. It references no AEC peer — exchange, review, geospatial, energy, cost, and schedule carriers lower foreign formats to seam graph, content-key, or receipt surfaces, simulation stays Compute-owned, and the Python IFC companion meets only at the wire.

## [01]-[ROUTER]

[ENERGY]:
- [01]-[DERIVE](.planning/Energy/derive.md): BIM-to-BEM lower — honeybee envelope, dragonfly massing, and the OSM-centric translation matrix.
- [02]-[EXCHANGE](.planning/Energy/exchange.md): `EnergyExchange.Apply` folding raise, lower, and translate onto content-keyed document carriers.
- [03]-[PROJECTOR](.planning/Energy/projector.md): `EnergyProjector` raise landing every energy format in the shape the Compute runner reads.

[EXCHANGE]:
- [04]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — one total codec switch over scene, IFC, COBie, and tile targets.
- [05]-[FORMAT](.planning/Exchange/format.md): Format-codec-extension table with per-importer frame normalization and sniffed row resolution.
- [06]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold landing every decode arm on the pooled imported-geometry carrier.
- [07]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): Scan-to-BIM folding segmented clouds into seam occurrences over the LAS/LAZ ingest front.
- [08]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [09]-[WIRE](.planning/Exchange/wire.md): Host-free content-keyed `IfcWire` interchange artifact the Python and TypeScript peers decode.

[MODEL]:
- [10]-[ELEMENTS](.planning/Model/elements.md): Generated `IfcClass` taxonomy with release map, domain partition, and predefined egress gate.
- [11]-[FAULTS](.planning/Model/faults.md): `BimFault` closed union lifting every rejection onto the seam fault band's typed rail.
- [12]-[QUERY](.planning/Model/query.md): Set-algebraic `ElementSet` query over a closed predicate union discriminating domain, type, and zone.
- [13]-[SPATIAL](.planning/Model/spatial.md): Spatial rank vocabulary and the containment-tree view derived over the seam's compose edges.
- [14]-[STRUCTURAL](.planning/Model/structural.md): Structural-analysis reader lowering restraints, loads, and topology onto seam payloads.
- [15]-[SYSTEMS](.planning/Model/systems.md): Derived MEP connectivity view — port flow edges, directed system trace, and interference check.
- [16]-[ZONES](.planning/Model/zones.md): Cross-cutting `BimZone` many-to-many overlay distinct from the single-parent containment tree.

[PLANNING]:
- [17]-[COST](.planning/Planning/cost.md): 5D `CostItem` cost-and-resource network joining rates to quantities with the rollup fold.
- [18]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` network carrying task-time intervals, lags, and instant snapshots.

[PROJECTION]:
- [19]-[EGRESS](.planning/Projection/egress.md): `SemanticProjector.Emit` IFC re-author with railed release raise and per-token admission gate.
- [20]-[RELATIONS](.planning/Projection/relations.md): `IfcRelKind` roster folding every relationship family onto the seam edge algebra.
- [21]-[SEMANTIC](.planning/Projection/semantic.md): `SemanticProjector` lowering GeometryGym into seam deltas beside the IFC-legality constraint.

[REVIEW]:
- [22]-[COORDINATION](.planning/Review/coordination.md): Clash rule engine, impact report, and sign-off machine owning the BCF issue board.
- [23]-[DIFF](.planning/Review/diff.md): `ModelDiff` folding two graph snapshots into typed added, modified, removed, and moved arms.
- [24]-[ISSUES](.planning/Review/issues.md): BCF topic, comment, and viewpoint family over the `.bcfzip` codec and the REST projection.
- [25]-[VALIDATION](.planning/Review/validation.md): IDS owner folding facets onto the predicate algebra with typed value-constraint match.
- [26]-[VERSIONING](.planning/Review/versioning.md): Content-addressed model history — the commit DAG and three-way merge with typed conflicts.

[SEMANTICS]:
- [27]-[APPEARANCE](.planning/Semantics/appearance.md): Surface-style lowering onto the seam PBR summary reconciled at the Materials content key.
- [28]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD-bound classification axis with live resolution and association round-trip.
- [29]-[COMPOSITION](.planning/Semantics/composition.md): Bidirectional material projector between IFC material selects and seam composition.
- [30]-[CONNECTION](.planning/Semantics/connection.md): `ConnectionProjection` lowering realizing elements onto seam detail bags and edges.
- [31]-[GEOREFERENCE](.planning/Semantics/georeference.md): Map-conversion and CRS lowering into the seam `GeoReference` with the reprojection leg.
- [32]-[GEOSPATIAL](.planning/Semantics/geospatial.md): Site-context projector — Simple-Features algebra and universal vector/raster ingest.
- [33]-[PROPERTIES](.planning/Semantics/properties.md): Pset/Qto template authority, inheritance classifier, and base-quantity derivation.

## [02]-[DOMAIN_PACKAGES]

BIM-domain libraries admitted by this folder; versions centralize in the C# manifest and corroborate against this folder's `.api/`.

[MODEL_INTERCHANGE]:
- `GeometryGymIFC_Core` — the sole IFC semantic model surface.
- `SharpGLTF.Core`
- `SharpGLTF.Toolkit`
- `SharpGLTF.Runtime`
- `SharpGLTF.Ext.3DTiles`
- `subtree` — the 3D-Tiles implicit-availability bitstream.
- `UniversalSceneDescription` — OpenUSD scene decode and emit.
- `ACadSharp` — DWG/DXF mesh read.
- `AssimpNetter` — FBX/Collada/3MF scene decode and emit.
- `geometry3Sharp` — the OBJ/STL/OFF text-mesh arm.
- `Ply.Net` — dedicated PLY decode.
- `dotbim` — lightweight `.bim` mesh-plus-metadata interchange.
- `Openize.Drako` — Draco mesh compression.
- `Alimer.Bindings.MeshOptimizer` — meshopt compression.
- `Themis.Las` — uncompressed LAS point-cloud decode.
- `Unofficial.laszip.netstandard` — compressed LAZ point-cloud decode.
- `StructuralAnalysisFormat` — SAF/XLSX exchange over seam structural payloads.
- `Speckle.Sdk` — the Speckle object-graph sync seam.
- `Speckle.Objects`
- `Xbim.CobieExpress` — the COBie FM-handover emit.
- `Xbim.IO.CobieExpress`
- `Xbim.CobieExpress.Exchanger`

[REVIEW]:
- `Xbim.InformationSpecifications` — the IDS value-constraint engine.
- `ids-lib` — IDS-file conformance audit and schema authority.
- `Smino.Bcf.Toolkit` — the BCF `.bcfzip` codec.
- `SwiftCollections.Lean` — the BVH broad phase behind the interference check.

[DOMAIN_VOCABULARY]:
- `Xbim.Properties` — Pset/Qto template definitions.
- `bSDD Dictionaries API` — live buildingSMART dictionary REST over the Compute transport; no manifest pin, no assembly.
- `BrickSchema.Net` — building-systems ontology.
- `VividOrange.Loads`
- `VividOrange.Cases`
- `VividOrange.Stages`
- `VividOrange.Countries`
- `VividOrange.IStandards` — verified-transitive through `VividOrange.Cases`; no direct manifest row.
- `NodaMoney` — the 5D cost-value money type.

[ENERGY_EXCHANGE]:
- `HoneybeeSchema` — the HBJSON object graph composed to operator depth.
- `DragonflySchema` — DFJSON massing composing honeybee by identifier.
- `NREL.OpenStudio.macOS-arm64` — OSM/IDF store and translators; simulation is Compute's half.

[GEOSPATIAL]:
- `NetTopologySuite.IO.Esri.Shapefile`
- `NetTopologySuite.IO.GeoPackage`
- `NetTopologySuite.IO.GeoJSON4STJ`
- `NetTopologySuite.IO.VectorTiles`
- `NetTopologySuite.IO.VectorTiles.Mapbox`
- `GISBlox.IO.GeoParquet`
- `FlatGeobuf`
- `SharpKml.Core`
- `pocketken.H3`
- `bertt.CityJSON`
- `MaxRev.Gdal.Core` — the OGR universal vector driver and GeoTIFF/COG/DEM raster ingest.
- `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- `ProjNET` — the datum-to-datum reprojection leg.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[SEAM_CONTRACTS]:
- `Rasm.Element` — the element seam Bim depends UP on: the canonical graph and the projection and constraint contracts.
- `Riok.Mapperly` — compile-time boundary transcription over the seam unions.
- `Generator.Equals` — structural equality and member diff behind the emit change derivation.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[NUMERIC_ALGORITHM]:
- `UnitsNet`
- `NetTopologySuite` — the OGC Simple-Features planar algebra behind the geospatial seam; the IO codecs stay folder additions.
- `QuikGraph` — the CPM sort, system-trace reachability, commit-DAG ancestor, and coordination closure walks.
- `CommunityToolkit.HighPerformance` — pooled buffer staging behind the mesh encoders.

[IDENTITY_TIME]:
- `System.IO.Hashing` — reached only through the kernel content-hash mint every content key seeds from.
- `NodaTime`

[TEST]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
