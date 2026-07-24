# [RASM_BIM]

`Rasm.Bim` owns host-neutral BIM semantics — the federation's openBIM authority over the IFC vocabularies, model readers, 4D/5D delivery networks, IDS/BCF/clash/diff review, content-addressed versioning, energy-model exchange, and geospatial site context. Its bar is round-trip fidelity at coordination grade: a foreign model lowers onto the seam graph without semantic loss, re-emits as legal IFC through the per-token admission gate, and every review verdict lands as a typed receipt the issue board and review planes consume directly.

It projects GeometryGym models into canonical `ElementGraph` deltas over the `Rasm.Element` seam, re-emits IFC through `SemanticProjector.Emit`, gates legality through `IfcLegality`, and mints the vocabularies every consumer composes. It references no AEC peer — exchange, review, geospatial, energy, cost, and schedule carriers lower foreign formats to seam graph, content-key, or receipt surfaces, simulation stays Compute-owned, and the Python IFC companion meets only at the wire.

## [01]-[ROUTER]

[ENERGY]:
- [01]-[DERIVE](.planning/Energy/derive.md): BIM-to-BEM lower — honeybee envelope, dragonfly massing, and the OSM-centric translation matrix.
- [02]-[EXCHANGE](.planning/Energy/exchange.md): `EnergyExchange.Apply` folding raise, lower, and translate onto content-keyed document carriers.
- [03]-[PROJECTOR](.planning/Energy/projector.md): `EnergyProjector` raise landing every energy format in the shape the Compute runner reads.

[EXCHANGE]:
- [04]-[EVENTS](.planning/Exchange/events.md): `BimEvent` model-mutation fact union and its CloudEvents envelope with W3C trace continuity.
- [05]-[EXPORT](.planning/Exchange/export.md): `BimExport` artifact emit — one total codec switch over scene, IFC, COBie, and tile targets.
- [06]-[FORMAT](.planning/Exchange/format.md): Format-codec-extension table with per-importer frame normalization and sniffed row resolution.
- [07]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold landing every decode arm on the pooled imported-geometry carrier.
- [08]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): Scan-to-BIM folding segmented clouds into seam occurrences over the LAS/LAZ ingest front.
- [09]-[TESSELLATION](.planning/Exchange/tessellation.md): `TessellationRequest` IFC/AP242/native geometry hop to the Compute companion rail.
- [10]-[WIRE](.planning/Exchange/wire.md): Host-free content-keyed `IfcWire` interchange artifact the Python and TypeScript peers decode.

[MODEL]:
- [11]-[ELEMENTS](.planning/Model/elements.md): Generated `IfcClass` taxonomy with release map, domain partition, and predefined egress gate.
- [12]-[FAULTS](.planning/Model/faults.md): `BimFault` closed union lifting every rejection onto the seam fault band's typed rail.
- [13]-[OBSERVABILITY](.planning/Model/observability.md): `BimHooks` rail, `BimTelemetry` receipt-projected instruments, corpus-gated bench claims.
- [14]-[QUERY](.planning/Model/query.md): Set-algebraic `ElementSet` query over a closed predicate union, `PredicateWire`, `StorePlan` push-down.
- [15]-[SPATIAL](.planning/Model/spatial.md): Spatial rank vocabulary, containment tree over seam compose edges, adjacency, and linear positioning.
- [16]-[STRUCTURAL](.planning/Model/structural.md): Structural-analysis reader lowering restraints, loads, and topology onto seam payloads.
- [17]-[SYSTEMS](.planning/Model/systems.md): Derived MEP connectivity — port flow edges, directed system trace, demand folds, interference check.
- [18]-[ZONES](.planning/Model/zones.md): Cross-cutting `BimZone` many-to-many overlay distinct from the single-parent containment tree.

[PLANNING]:
- [19]-[COST](.planning/Planning/cost.md): 5D `CostItem` resource network and 6D `CarbonEstimate` carbon rollup over the material-true takeoff.
- [20]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` network with task-time intervals, lags, and phase-partitioned snapshots.

[PROJECTION]:
- [21]-[EGRESS](.planning/Projection/egress.md): `SemanticProjector.Emit` IFC re-author — release raise, per-token admission gate, scoped emit.
- [22]-[RELATIONS](.planning/Projection/relations.md): `IfcRelKind` roster folding every relationship family onto the seam edge algebra.
- [23]-[SEMANTIC](.planning/Projection/semantic.md): `SemanticProjector` GeometryGym-to-seam lowering under `IfcLegality`, fidelity-drop ledger.

[REVIEW]:
- [24]-[COORDINATION](.planning/Review/coordination.md): Clash rule engine, impact report, and sign-off machine owning the BCF issue board.
- [25]-[DIFF](.planning/Review/diff.md): `ModelDiff` folding two graph snapshots into typed added, modified, removed, and moved arms.
- [26]-[ISSUES](.planning/Review/issues.md): BCF topic, comment, and viewpoint family over the `.bcfzip` codec and the BCF-API request projection.
- [27]-[VALIDATION](.planning/Review/validation.md): Two-tier model-QA — template-audit baseline beneath the authored IDS facet fold.
- [28]-[VERSIONING](.planning/Review/versioning.md): Content-addressed model history — commit DAG and three-way merge with typed conflicts.

[SEMANTICS]:
- [29]-[APPEARANCE](.planning/Semantics/appearance.md): Surface-style lowering onto the seam PBR summary reconciled at the Materials content key.
- [30]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD classification axis — live resolution, association round-trip, enrichment.
- [31]-[COMPOSITION](.planning/Semantics/composition.md): Bidirectional material projector between IFC material selects and seam composition.
- [32]-[CONNECTION](.planning/Semantics/connection.md): `ConnectionProjection` lowering realizing elements onto seam detail bags and edges.
- [33]-[GEOREFERENCE](.planning/Semantics/georeference.md): Map-conversion and CRS lowering onto seam `GeoReference` with federation preflight.
- [34]-[GEOSPATIAL](.planning/Semantics/geospatial.md): Site-context projector — Simple-Features algebra and universal vector/raster ingest.
- [35]-[PROPERTIES](.planning/Semantics/properties.md): Pset/Qto template authority, inheritance classifier, quantity derivation, conformance audit.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[MODEL_INTERCHANGE]:
- `GeometryGymIFC_Core` — sole IFC semantic-model surface.
- `SharpGLTF.Core`
- `SharpGLTF.Toolkit`
- `SharpGLTF.Runtime`
- `SharpGLTF.Ext.3DTiles`
- `subtree` — 3D-Tiles implicit-availability bitstream.
- `UniversalSceneDescription` — OpenUSD scene decode and emit.
- `ACadSharp` — DWG/DXF mesh read.
- `AssimpNetter` — FBX/Collada/3MF scene decode and emit.
- `geometry3Sharp` — OBJ/STL/OFF text-mesh arm.
- `Ply.Net` — dedicated PLY decode.
- `dotbim` — lightweight `.bim` mesh-and-metadata interchange.
- `Openize.Drako` — Draco mesh compression.
- `Alimer.Bindings.MeshOptimizer` — meshopt compression.
- `Themis.Las` — uncompressed LAS point-cloud decode.
- `Unofficial.laszip.netstandard` — compressed LAZ point-cloud decode.
- `StructuralAnalysisFormat` — SAF/XLSX exchange over seam structural payloads.
- `Speckle.Sdk` — Speckle object-graph sync seam.
- `Speckle.Objects`
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
- `NREL.OpenStudio.macOS-arm64` — OSM/IDF store and translators; simulation is Compute's half.

[EVENT_ENVELOPE]:
- `CloudNative.CloudEvents` — CloudEvents 1.0 envelope and typed attribute model.
- `CloudNative.CloudEvents.SystemTextJson` — structured-mode JSON event formatter; transport bindings stay app-tier.

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
- `MaxRev.Gdal.Core` — OGR universal vector driver and GeoTIFF/COG/DEM raster ingest.
- `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- `ProjNET` — datum-to-datum reprojection leg.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[SEAM_CONTRACTS]:
- `Rasm.Element` — element seam Bim depends UP on: canonical graph with projection and constraint contracts.
- `Riok.Mapperly` — compile-time boundary transcription over the seam unions.
- `Generator.Equals` — structural equality and member diff behind the emit change derivation.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `JetBrains.Annotations`

[NUMERIC_ALGORITHM]:
- `UnitsNet`
- `NetTopologySuite` — OGC Simple-Features planar algebra behind the geospatial seam; IO codecs stay folder additions.
- `QuikGraph` — CPM sort, system-trace reachability, commit-DAG ancestor, and coordination closure walks.
- `CommunityToolkit.HighPerformance` — pooled buffer staging behind the mesh encoders.

[IDENTITY_TIME]:
- `System.IO.Hashing` — reached only through the kernel content-hash mint every content key seeds from.
- `NodaTime`

[TEST]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
