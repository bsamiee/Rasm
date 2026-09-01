# [RASM_BIM]

`Rasm.Bim` owns host-neutral openBIM semantics over the IFC vocabularies, model readers, 4D/5D delivery networks, IDS/BCF/clash/diff review, content-addressed versioning, energy-model exchange, and geospatial site context. Its bar is round-trip fidelity at coordination grade: a foreign model lowers onto the element graph without semantic loss, re-emits as legal IFC through the per-token admission gate, and every review verdict is a typed value the issue board and review planes read directly.

Every carrier sub-domain lowers its foreign format to element-graph or content-key currency.

## [01]-[ROUTER]

[MODEL]:
- [01]-[ELEMENTS](.planning/Model/elements.md): Generated `IfcClass` taxonomy with release map, domain partition, and predefined egress gate.
- [02]-[EMITTER](.planning/Model/emitter.md): Offline vocabulary producer — EXPRESS-intersected reflection, hand overlays, regeneration runner.
- [03]-[QUERY](.planning/Model/query.md): Graph-bound `ElementQuery` over the shared predicate closure and native `StorePlan` push-down.
- [04]-[SPATIAL](.planning/Model/spatial.md): Spatial rank vocabulary, containment tree over contract edges, adjacency, and linear positioning.
- [05]-[ZONES](.planning/Model/zones.md): Cross-cutting `BimZone` many-to-many overlay distinct from the single-parent containment tree.
- [06]-[SYSTEMS](.planning/Model/systems.md): Derived MEP connectivity — port flow edges, directed system trace, demand folds, interference check.
- [07]-[STRUCTURAL](.planning/Model/structural.md): Structural-analysis reader lowering restraints, loads, and topology onto shared payloads.
- [08]-[EUROCODE](.planning/Model/eurocode.md): EN 1990 action classification, partial-factor stamp, and the elected combination roster.
- [09]-[FAULTS](.planning/Model/faults.md): `BimFault` closes terminal scope/reason refusals and cause-preserving boundary failures over band 2600.
- [10]-[OBSERVABILITY](.planning/Model/observability.md): `BimPoint` roster, `BimHooks` over the kernel hooks, `BimInstrument` rows, bench claims.

[SEMANTICS]:
- [11]-[PROPERTIES](.planning/Semantics/properties.md): Pset/Qto template authority, inheritance classifier, quantity derivation, conformance audit.
- [12]-[CLASSIFICATION](.planning/Semantics/classification.md): bSDD classification axis — live resolution, association round-trip, enrichment.
- [13]-[COMPOSITION](.planning/Semantics/composition.md): Bidirectional material projector between IFC material selects and shared composition.
- [14]-[APPEARANCE](.planning/Semantics/appearance.md): Surface-style lowering onto the shared PBR summary reconciled at the Materials content key.
- [15]-[CONNECTION](.planning/Semantics/connection.md): `ConnectionProjection` lowering realizing elements onto shared detail bags and edges.
- [16]-[GEOREFERENCE](.planning/Semantics/georeference.md): Map-conversion and CRS lowering onto shared `GeoReference` with federation preflight.
- [17]-[FEATURE](.planning/Semantics/feature.md): Host-neutral geospatial row — precision root, datum leg, classifier ladder, wire and GDAL boundary.
- [18]-[MODEL](.planning/Semantics/model.md): Indexed feature set — DE-9IM join, k-NN clash, linear referencing, overlays, DGGS cover, MVT pyramid.
- [19]-[VECTOR](.planning/Semantics/vector.md): Vector source table — managed codecs, remote range read, KML presentation, typed OGR fold.
- [20]-[RASTER](.planning/Semantics/raster.md): GDAL raster ingest — band schema, overview pyramid, coverage projection, DEM and contour derive legs.

[PLANNING]:
- [21]-[SCHEDULE](.planning/Planning/schedule.md): 4D `ConstructionTask` network with task-time intervals, lags, and phase-partitioned snapshots.
- [22]-[PROGRESS](.planning/Planning/progress.md): Scan-derived physical-progress verification joining reconstructed occurrences to the task network.
- [23]-[COST](.planning/Planning/cost.md): 5D `CostItem` resource network and 6D `CarbonEstimate` carbon rollup over the material-true takeoff.

[EXCHANGE]:
- [24]-[FORMAT](.planning/Exchange/format.md): Format-codec-extension table with per-importer frame normalization and sniffed row resolution.
- [25]-[IMPORT](.planning/Exchange/import.md): `BimIo` foreign-bytes ingest fold landing every decode arm on the pooled imported-geometry carrier.
- [26]-[EXPORT](.planning/Exchange/export.md): Artifact emit path over glTF, 3D-Tiles, COBie and SAF targets, with the round-trip fidelity witness.
- [27]-[TESSELLATION](.planning/Exchange/tessellation.md): Typed canonical-IFC hop to the Compute companion path.
- [28]-[RECONSTRUCT](.planning/Exchange/reconstruct.md): Scan-to-BIM folding segmented clouds into shared occurrences over the LAS/LAZ ingest front.
- [29]-[SAF](.planning/Exchange/saf.md): SAF XLSX interchange codec — workbook I/O, correspondence spine, graph lowering, and import authoring.
- [30]-[WIRE](.planning/Exchange/wire.md): Host-free content-keyed `IfcWire` interchange artifact the Python and TypeScript peers decode.
- [31]-[EVENTS](.planning/Exchange/events.md): Announcement projection subscribing fired `BimFact` rows onto the kernel message-envelope owner.

[ENERGY]:
- [32]-[EXCHANGE](.planning/Energy/exchange.md): `EnergyExchange.Apply` folding raise, lower, and translate onto content-keyed document carriers.
- [33]-[PROJECTOR](.planning/Energy/projector.md): `EnergyProjector` raise landing every energy format in the shape the Compute runner reads.
- [34]-[DERIVE](.planning/Energy/derive.md): BIM-to-BEM lowering — honeybee envelope, dragonfly massing, and the OSM translation matrix.
- [35]-[RESULTS](.planning/Energy/results.md): `EnergyResults.Admit` landing a Compute run's typed results as result bags on the graph.

[REVIEW]:
- [36]-[VALIDATION](.planning/Review/validation.md): Three-tier model-QA verdict — shared audit beneath template baseline and IDS facet folds.
- [37]-[ISSUES](.planning/Review/issues.md): BCF topic, comment, and viewpoint family over the `.bcfzip` codec and the BCF-API request projection.
- [38]-[DIFF](.planning/Review/diff.md): `ModelDiff` folding two graph snapshots into typed added, modified, removed, and moved arms.
- [39]-[COORDINATION](.planning/Review/coordination.md): Clash rule engine, impact report, and sign-off machine owning the BCF issue board.
- [40]-[VERSIONING](.planning/Review/versioning.md): Content-addressed model history — commit DAG and three-way merge with typed conflicts.

[PROJECTION]:
- [41]-[SEMANTIC](.planning/Projection/semantic.md): `SemanticProjector` GeometryGym-to-contract lowering under `IfcLegality`.
- [42]-[FOREIGN](.planning/Projection/foreign.md): Foreign-object-graph shared arm beside the projector-polymorphic reingest reconcile.
- [43]-[FIDELITY](.planning/Projection/fidelity.md): Bounded-drop vocabulary, the `FidelityLog` monoid, and the carrier every lowering returns.
- [44]-[WIREFORM](.planning/Projection/wireform.md): Serialization and container axes, the published release matrix, the pre-construction sniff.
- [45]-[VALUE](.planning/Projection/value.md): IFC unit-declaration ingress and the `IfcProperty`/quantity value narrowing onto the shared cases.
- [46]-[RAISE](.planning/Projection/raise.md): Egress value raise — the derived measure and quantity mint tables under one two-rung election.
- [47]-[RELATIONS](.planning/Projection/relations.md): `IfcRelKind` roster folding every relationship family onto the boundary edge algebra.
- [48]-[EGRESS](.planning/Projection/egress.md): `SemanticProjector.Emit` IFC re-author — release raise, per-token admission gate, scoped emit.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

[MODEL_INTERCHANGE]:
- `GeometryGymIFC_Core` — Sole IFC semantic-model surface.
- `subtree` — 3D-Tiles implicit-availability bitstream.
- `UniversalSceneDescription` — OpenUSD scene decode and emit.
- `AssimpNetter` — FBX/Collada/3MF scene decode and emit.
- `Ply.Net` — Dedicated PLY decode.
- `dotbim` — Lightweight `.bim` mesh-and-metadata interchange.
- `Openize.Drako` — Draco mesh compression.
- `Themis.Las` — Uncompressed LAS point-cloud decode.
- `StructuralAnalysisFormat` — SAF/XLSX exchange over shared structural payloads.
- `Xbim.CobieExpress` — COBie FM-handover emit.
- `Xbim.IO.CobieExpress`
- `Xbim.CobieExpress.Exchanger`

[REVIEW_ENGINES]:
- `Xbim.InformationSpecifications` — IDS value-constraint engine.
- `ids-lib` — IDS-file conformance audit and schema authority.
- `Smino.Bcf.Toolkit` — BCF `.bcfzip` codec.
- `SwiftCollections.Lean` — BVH broad phase behind the interference check.

[DOMAIN_VOCABULARY]:
- `Xbim.Properties` — Pset/Qto template definitions.
- `bSDD Dictionaries API` — Live buildingSMART dictionary REST over the Compute transport; no manifest pin, no assembly.
- `BrickSchema.Net` — Building-systems ontology.
- `VividOrange.Loads`
- `VividOrange.Cases`
- `VividOrange.Stages`
- `VividOrange.Countries` — National-context axis: eurocode annex bridge, schedule stage-governance nation, SAF design code.
- `VividOrange.IStandards` — Transitive through `VividOrange.Cases`; no direct manifest row.
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
- `ProjNET` — Datum-to-datum reprojection leg.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/dotnet/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `Riok.Mapperly` — Compile-time boundary transcription over the shared unions.
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `NodaTime`
- `System.IO.Hashing` — Reached only through the kernel content-hash mint every content key seeds from.
- `UnitsNet`
- `CommunityToolkit.HighPerformance` — Memory-backed stream projection at binary glTF admission and pooled buffer staging behind mesh encoders.
- `QuikGraph` — CPM sort, system-trace reachability, commit-DAG ancestor, and coordination closure walks.

[GEOMETRY_INTERCHANGE]:
- `ACadSharp` — DWG/DXF mesh-read leg into `ImportedGeometry`.
- `SharpGLTF.Core` — Schema I/O behind the `Exchange/export` emit and the import decode leg.
- `SharpGLTF.Toolkit` — Vertex, mesh, scene, and material heads feeding `ToGltf2`.
- `SharpGLTF.Runtime` — Per-instance decode over the imported `ModelRoot` at the templating leg.
- `SharpGLTF.Ext.3DTiles` — Authors the 3D Tiles overlay at `Exchange/export#TILE_METADATA`.
- `Speckle.Sdk` — Receive-side `Base` graph: `Flatten` traversal, display values, metre conversion.
- `Speckle.Objects` — Display-mesh geometry and the `DataObject` host-object family the import contract folds.
- `Unofficial.laszip.netstandard` — Compressed-LAZ decode leg of the dual-engine `Exchange/reconstruct` ingest front.

[ENERGY_SIMULATION]:
- `NREL.OpenStudio.macOS-arm64` — Drives the OSM/IDF exchange leg: robust load, save, version upgrade, and the gbXML/SDD semantic bridges.

[MESH_PROCESSING]:
- `Alimer.Bindings.MeshOptimizer` — meshopt compression behind the mesh encoders.
- `geometry3Sharp` — OBJ/STL/OFF text-mesh decode arm of the `MeshText` interchange codec.

[NUMERIC_SUBSTRATE]:
- `MathNet.Numerics` — Carries LAS point positions as `Vector<double>`, filled by the reconstruct decode and read unwrapped at registration.

[PLANAR_GEOSPATIAL]:
- `NetTopologySuite` — OGC Simple-Features planar algebra behind the geospatial boundary.
- `NetTopologySuite.IO.GeoJSON4STJ` — Carries the STJ GeoJSON codec leg of the geospatial contract for site context and web projection.
- `NetTopologySuite.IO.GeoPackage` — Carries the GeoPackage geometry-BLOB leg for site and context ingest.
- `pocketken.H3` — Keys the `Semantics/model#GEO_MODEL` DGGS arm, the coarse `ulong` bucket beside the `STRtree`.

[EVENT_TRANSPORT]:
- `Celly.Protovalidate` — Evaluates the generated event extension rules at BIM announcement mint and admission.
- `CloudNative.CloudEvents` — Message-envelope type the announcement projection mints through `Rasm/Domain/event`; bindings stay app-tier.
- `Google.Protobuf` — Generated descriptor, repeated-field, optional-field, timestamp, and wire runtime used by those projections.

[RUNTIME_INBOX]:
- `System.Text.Json` — Generated wire contexts behind the exchange message envelopes, review records, and the GeoJSON boundary.
