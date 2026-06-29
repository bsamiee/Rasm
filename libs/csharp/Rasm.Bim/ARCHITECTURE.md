# [RASM_BIM_ARCHITECTURE]

The domain map of `Rasm.Bim` — the host-neutral AEC-DOMAIN BIM/IFC owner and the IFC arm of the `Rasm.Element` seam, depending UP on `{Rasm, Rasm.Element}` and projecting GeometryGym into the canonical `ElementGraph` rather than re-storing a parallel element record. The `Model`, `Semantics`, `Planning`, `Exchange`, `Review`, and `Projection` sub-domains, each lowering onto the one `BimFault` band; the `Projection` sub-domain hosts the one `SemanticProjector : IElementProjection` (GeometryGym `DatabaseIfc` → seam `GraphDelta` ingress; the Bim-internal `Emit` IFC re-author) and the `IfcLegality : IGraphConstraint` IFC-semantic legality. The retired `BimElement`/`BimModel` element record and the stringly `PropertyBinding`/`QuantityBinding` triples are GONE — the consumer-facing element is the seam `Bake(objectNode)` fold over the `ElementGraph`. Bim stays the SOLE GeometryGym/IFC owner and references no AEC peer; alignment with `Rasm.Materials`/`Rasm.Fabrication` travels through the shared seam graph and the content-keyed wire, never sibling coupling.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Bim/
├── Model/                 # Host-neutral BIM object model and analytical model
│   ├── Elements.cs        # IfcClass entity-class taxonomy
│   ├── Query.cs           # Set-algebraic ElementPredicate query folded over ElementSet
│   ├── Structure.cs       # Spatial-structure tree + closed AssemblyRel decomposition
│   ├── Zones.cs           # BimZone many-to-many zone/program overlay
│   ├── Systems.cs         # DistributionSystem MEP connectivity graph with SystemTrace fold
│   ├── Structural.cs      # IFC structural-analysis AnalysisModel the Compute solver reads
│   └── Faults.cs          # Band-2600 BimFault closed [Union] every entrypoint lowers onto
├── Semantics/             # Element-bound semantic enrichment
│   ├── Properties.cs      # PropertyKey Pset/Qto TEMPLATE authority, bSDD DataType resolution, Property classifier, etc...
│   ├── Classification.cs  # bSDD-bound Classification axis with BsddResolution live dictionary
│   ├── Composition.cs     # BimMaterial construction-material composition [Union]
│   ├── Appearance.cs      # BimAppearance PBR record reconciled with Rasm.Materials at content-key seam
│   ├── Connection.cs      # ConnectionDetail realizing-element joint [Union] (Bolted/Welded/Bearing/Cast) over IfcRelConnectsWithRealizingElements
│   ├── GeoReference.cs    # GeoReferenceProjector lowering IfcMapConversion/IfcProjectedCRS into the seam GeoReference
│   └── Geospatial.cs      # GeoFeature/GeoModel NTS Simple-Features algebra + GDAL/OGR universal vector+raster ingest, shapefile/CityJSON codecs, etc...
├── Planning/              # 4D/5D delivery network
│   ├── Schedule.cs        # ConstructionTask 4D activity schedule over IfcTaskTime intervals
│   └── Cost.cs            # CostItem 5D cost-and-resource estimate with CostSchedule.Rollup fold
├── Exchange/              # Universal interchange codec
│   ├── Format.cs          # Format/codec/extension axis (glTF/IFC/STEP/USD/scene-exchange/PLY/point-cloud/geospatial) plus FrameNormalization
│   ├── Import.cs          # BimIo foreign-bytes ingest fold — SharpGLTF/geometry3Sharp/Ply.Net/AssimpNetter/UsdStage decode arms
│   ├── Export.cs          # BimExport emit fold — GLB/AssimpNetter/UsdStage scene + IFC serialization + subtree .subtree availability bitstream
│   ├── Tessellation.cs    # TessellationRequest Compute companion bridge
│   ├── Reconstruct.cs     # Scan-to-BIM ReconstructionPrimitive [Union] fitting fold over the Themis.Las LAS ingest front
│   └── Wire.cs            # Host-free BimWire projection the Python and TypeScript peers decode
├── Review/                # Model-checking and coordination
│   ├── Validation.cs      # IDS v1.0 owner folding six IdsFacet arms over the seam ElementGraph
│   ├── Issues.cs          # BCF 3.0 issue exchange with .bcfzip codec and BcfApi REST projection
│   ├── Diff.cs            # GlobalId-plus-content-key ModelDiff federation change-set
│   ├── Coordination.cs    # CoordinationRule [Union] rule engine, ClashProposal fold, ImpactReport, BCF SignOff [SmartEnum]
│   └── Versioning.cs      # Content-addressed BimCommit DAG + three-way ElementChange Merge over the diff content-key
└── Projection/            # The IFC arm of the Rasm.Element seam
    └── Semantic.cs        # SemanticProjector:IElementProjection
```

Every sub-domain projects onto or reads the one seam `ElementGraph` (the `Projection/Semantic` `SemanticProjector` lowers GeometryGym into it; the seam `Bake(objectNode)` fold derives the consumer element) rather than a parallel `BimModel` surface, lowers rejection onto the `Model/Faults` `BimFault` band, and consumes the `Model/Query` `ElementPredicate` algebra and the `Semantics/Classification` axis as settled vocabulary.

## [02]-[SEAMS]

```text seams
Projection/semantic       →  csharp:Rasm.Element/Graph                 # [PROJECTION]: SemanticProjector:IElementProjection lowers DatabaseIfc → GraphDelta the Assemble fold merges; the Bim-internal Emit re-authors ElementGraph → IFC bytes, never a seam member
Projection/semantic       →  csharp:Rasm.Element/Projection            # [CONSTRAINT]: IfcLegality:IGraphConstraint IFC-semantic legality (containment-spatial / Voids element→opening / type-no-aggregate-occurrence) composed after the seam structural law [M3]
Projection/semantic       ←  csharp:Rasm.Element/Graph                 # [CONTRACT]: Node/Relationship/GraphDelta/NodeId/ProjectionContext/Assemble — GeometryGym captured internally, never crossing the IElementProjection signature
Model/elements            →  csharp:Rasm.Element/Graph                 # [PROJECTION]: generic Classification("ifc",code) + seam PredefinedType token stamped on the Object node (IfcClass is the IFC validation authority, never a seam field) [C6]
Semantics/composition     →  csharp:Rasm.Element/Composition           # [PROJECTION]: MaterialProjection lands seam Material nodes (layer/profile/constituent set) + the C7 Associate material-usage edge at IFC ingest
Semantics/properties      →  csharp:Rasm.Element/Properties            # [CONTRACT]: PropertyKey template threads the seam PropertyValue/MeasureValue; PropertyInheritance stamps InheritanceMode on seam bag nodes [H1] (the value half seam-owned, not re-authored)
Semantics/properties      ←  csharp:Rasm                              # [CONTENT_KEY]: kernel GeometryMeasures(Option<double> Length/Area/Volume) value-object resolved from the Object RepresentationContentHash geometry and injected into QuantityDerivation.Derive — Bim consumes the measure (never re-tessellates); the kernel/Compute supply it, the same kernel owner the Dimension value-object rides
Semantics/georeference    →  csharp:Rasm.Element/Geospatial            # [PROJECTION]: GeoReferenceProjector lands Header.GeoReference [M1]; GeoTransform.Reproject the ProjNET leg over the seam value
Semantics/geospatial      →  csharp:Rasm.Element/Graph                 # [PROJECTION]: GeoFeature.ToObject/GeoRaster.ToCoverage land seam Object/Coverage nodes through a GraphDelta
Exchange/tessellation     ⇄  python:geometry/mesh                     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
*                         →  typescript:interchange                   # [WIRE]: BcfTopicWire / BcfViewpointWire
Review/issues             →  typescript:ui/overlay                    # [WIRE]: BcfTopicWire / BcfViewpointWire
Exchange/import           →  python:geometry/ifc                      # [PROJECTION]: IFC semantic graph ingest via GeometryGym
Exchange/wire             →  python:geometry/ifc                      # [PROJECTION]: BimWire model vocabulary
Model/elements            →  typescript:ui/overlay                    # [SHAPE]: GlobalId element selection set
Review/validation         ←  python:geometry/ifc                      # [BOUNDARY]: IDS validation evidence via ifctester
Model                     →  csharp:Rasm.Persistence/Query/columnar   # [PROJECTION]: Persistence owns the co-transactional BimOpenSchemaProjection:FlatTableProjection (Marten) and the EAV-generic structural map; the BIM-typed columns it materializes read the Bim-stamped typed seam nodes (Classification/PredefinedType/PropertyValue/MeasureValue from Projection/semantic) — Bim owns the typing as the wire seam, never a Marten FlatTableProjection in Bim, never a sibling reference [M4]
Model/structural          →  csharp:Rasm.Compute/Solver               # [CONTENT_KEY]: AnalysisModel (GeometryKey, PropertyKey) content-key
Semantics/appearance      ⇄  csharp:Rasm.Element/Composition          # [CONTENT_KEY]: BimAppearance reconciled with the Materials-projected seam Appearance node at the content key, never a direct Rasm.Materials reference
Model/query               →  csharp:Rasm.AppUi/Render                 # [PORT]: ElementSet query algebra via capability descriptor
Model                     ←  csharp:Rasm.Materials/Connection         # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener (connection-item egress; the material binding rides the MaterialProjector via the seam, NOT an AEC-peer reference)
Model                     ←  csharp:Rasm.Materials/Connection/joint   # [WIRE]: weld/stud IfcMechanicalFastener + IfcRelConnectsWithRealizingElements
Semantics                 →  csharp:Rasm.Compute/Runtime              # [PROJECTION]: IFC/glTF semantic metadata layer
Semantics/classification  ←  csharp:Rasm.Compute/Runtime/channels     # [TRANSPORT]: BsddPort injected bSDD GET /api/Class/v1 BsddClassResponse
Semantics/geospatial      →  python:geometry/ifc                      # [WIRE]: GeoFeature WKB Geometry.ToBinary decode via shapely (NTS-equivalent planar peer)
Semantics/geospatial      →  typescript:interchange                   # [WIRE]: GeoFeature WKB decode via turf (NTS-equivalent planar peer)
Semantics/geospatial      →  csharp:Rasm.AppUi/Charts                 # [SHAPE]: GeoFeature/GeoModel NetTopologySuite Feature geometry drawn as Mapsui.Avalonia12 basemap overlays beside the Wgpu viewport — AppUi's Mapsui transitively binds this Bim-owned NTS 2.6.0 pin and consumes the Feature shape, never referencing NTS directly
Semantics/geospatial      ←  csharp:Rasm.Persistence/Store            # [TRANSPORT]: GDAL /vsimem fsspec dataset open + OGR Arrow C-stream GeoParquet/FlatGeobuf columnar ingest
Semantics/geospatial      ⇄  Semantics/georeference                   # [PROJECTION]: GeoFeature.Reproject composes the ProjNET GEODETIC_TRANSFORM leg (OSR escalation for exotic datum-grids)
Model                     →  csharp:Rasm.Compute/Runtime/codecs       # [CONTENT_KEY]
Model/structural          →  csharp:Rasm.AppUi/Charts                 # [RECEIPT]: CriticalPath/EarnedValue schedule-and-cost report rendered as a Charts/dashboards projection over the Bim-owned schedule network (AppUi has no Schedule page; the 4D/5D report is a Charts projection — a dedicated Schedule projection page is a plan-cs-folders decision)
Planning/schedule         →  csharp:Rasm.AppUi/Charts                 # [RECEIPT]: ScheduleNetwork CPM/calendar/4D report rendered as a Charts/dashboards projection over the Bim-owned schedule network
Planning/cost             →  csharp:Rasm.AppUi/Charts                 # [RECEIPT]: CostSchedule EarnedValue/ChangeOrder report rendered as a Charts/dashboards projection over the Bim-owned cost network
Exchange/tessellation     →  csharp:Rasm.Compute/Runtime/codecs       # [TESSELLATION]: TessellationOutcome two-hop GLB, TessellationOrigin by ArtifactKey (dual SourceKey/ContentKey)
Exchange/tessellation     →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: TessellationOutcome ArtifactKey cache-hit lookup
Exchange/import           →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: Reimport prior seam ElementGraph snapshot content-key delta join
Exchange/import           ←  csharp:Rasm.Rhino/Exchange               # [BOUNDARY]: [^1]
Exchange/wire             →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: BimWire snapshot content-key ArtifactIndexRow join
Review/diff               →  csharp:Rasm.Persistence/Query/federation # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log
Review/versioning         →  csharp:Rasm.Persistence/Version/commits  # [CONTENT_KEY]: BimCommit content-addressed commit-DAG durably stored as CommitNode by the wire CommitKey
Exchange/wire             →  csharp:Rasm.Persistence/Sync             # [TRANSPORT]: OpLogWire ElementChange op-stream CRDT convergence
Review/versioning         →  csharp:Rasm.Persistence/Version/commits  # [SHAPE]: BimCommit DAG common-ancestor merge substrate (CommitGraph.MergeBase)
Model                     →  csharp:Rasm.Persistence/Store/quality    # [SHAPE]: IFC validation rules into QualityRule rows
Review/validation         →  csharp:Rasm.Compute/Runtime/codecs       # [TRANSPORT]: IdsAudit ifctester oracle two-hop rpc, GlobalId-plus-facet diff
Review/validation         →  python:geometry/ifc-companion            # [BOUNDARY]: ifctester IDS-XML conformance oracle verdict
Exchange/format           →  csharp:Rasm.Fabrication/Polygon           # [SHAPE]: ACadSharp managed DWG/DXF DxfDocument/CadDocument read codec — Bim owns the host-neutral CAD-interchange read surface, Fabrication consumes it for 2D profile ingress over the same central pin (mirror of the Fabrication-side Polygon/import ← Rasm.Bim/Exchange row)
Exchange/wire             →  typescript:interchange                   # [WIRE]: BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity
Exchange/wire             →  typescript:ui/overlay                    # [WIRE]: BcfWire/DiffWire GlobalId anchor decode
coordination              ⇄  csharp:Rasm.Persistence/Sync/annotation  # [WIRE]: durable annotation + CDE op-log
schedule                  ⇄  csharp:Rasm.Persistence/Sync/schedule    # [WIRE]: P6/MS-Project + 4D construction domain
coordination              →  csharp:Rasm.AppUi/Editing/issues         # [PORT]: BCF issue-board projection
Exchange/import           ⇄  csharp:Rasm.Persistence/Sync             # [TRANSPORT]: Speckle Base object-graph -> seam ElementGraph import over the Persistence-owned Speckle.Sdk SyncTransport.SpeckleLikeDiff
Exchange/tessellation     ⇄  csharp:Rasm.Compute                      # [SHAPE]: SharpGLTF/meshopt leg split — Bim authors per-tile EXT_structural_metadata/EXT_mesh_features glTF encode, Compute composes residency/transport meshopt-encode at interchange/codecs#TILE_PARTITION
Exchange                  ⇄  csharp:Rasm.Compute/Analysis             # [SHAPE]: OpenStudio energy SIMULATION (Compute) distinct from the IFC↔OSM semantic exchange (Bim), aligned by the seam graph never coupled
Planning                  ⇄  csharp:Rasm.Compute/Analysis             # [SHAPE]: construction schedule/4D MPXJ (Bim) vs embodied material-cost takeoff (Compute), aligned by the seam graph never coupled
Exchange                  →  csharp:Rasm.Persistence/Store/blobstore  # [CONTENT_KEY]: imported IFC/BREP geometry by IfcRepHash; IfcConvert GLB content-keyed wire, write-blob-first
```
- [^1] App-root RhinoDoc import projected to host-neutral mesh + GlobalId; Rhino owns the Rhino-side production + projection adapter, Bim owns the wire payload — the two reader engines (Rhino FileIO vs the managed PlyReader/ThreeMfReader/StepReader + SharpGLTF/geometry3Sharp arms) can disagree on the same OBJ/STL/PLY/3MF/glTF/STEP bytes, so the app path declares which reader is authoritative

The `[CONTENT_KEY]` seam rows above are one canonical idiom, not per-page schemes: every page that joins the federation, solver, cache, or diff lane derives a typed `UInt128` key pair through `XxHash128.HashToUInt128` and joins the `csharp:Rasm.Compute/Runtime/codecs` `CONTENT_ADDRESSING` `InterchangeIdentity`, never a second identity scheme.

[CONTENT_KEY_IDIOM]:
- `Model/structural` derives `(GeometryKey, PropertyKey)` — the analytical member's geometry plus its section/material property hash, the solver join surface.
- `Model/systems` derives `(GeometryKey, TopologyKey)` — the distribution element's geometry plus its port-connectivity topology hash, the trace memoization key.
- `Planning/schedule` derives `(GeometryKey, ScheduleKey)` — the element geometry plus its 4D task-time hash, the schedule catalog read.
- `Planning/cost` derives `(QuantityKey, ResourceKey)` — the quantity-set hash plus the resource-rate hash, the cost-rollup join.
- `Review/diff` derives `(ContentKey, PlacementKey)` — the element fingerprint plus its placement hash, the federation dedup and three-way merge anchor.
- `Exchange/tessellation`, `Exchange/wire`, and `Exchange/import` derive the artifact `ContentKey` through the same `InterchangeIdentity.Key` so the tessellation cache, the wire snapshot, and the reimport delta all address one content space.

A second identity scheme, a per-page hash function, or a `Guid`-keyed federation join is the named cross-folder drift defect: a page deriving a new content key inherits this idiom from the map, mints the typed `UInt128` pair through `XxHash128.HashToUInt128`, and joins the one `InterchangeIdentity` owner.

[HOST_BOUNDARY_EDGE]: the `Exchange/import ← csharp:Rasm.Rhino/Exchange` row is a single-sided HOST-BOUNDARY edge, not an interior dependency — `Rasm.Rhino/Exchange` is strata-locked, every format dispatching through RhinoCommon `Rhino.FileIO.*`/`RhinoDoc` with zero host-neutral parts, so it references only the kernel `Rasm` and is composed exclusively at the app root. `Rasm.Bim` never names `Rasm.Rhino`; the edge resolves only where the app root binds the live Rhino host, projecting a `RhinoDoc` import to a host-neutral mesh plus `GlobalId` the `Exchange/import` fold admits as a wire payload. Bim owns the wire payload; Rhino owns the Rhino-side production and the projection adapter. Because the two reader engines — Rhino FileIO and the managed `PlyReader`/`ThreeMfReader`/`StepReader` plus the SharpGLTF/`geometry3Sharp` arms — can decode the same OBJ/STL/PLY/3MF/glTF/STEP bytes to divergent meshes, the app root declares per path which reader is authoritative; the two coexist (Rhino-native capture and host-neutral managed ingest) and neither is gutted to feed the other.
