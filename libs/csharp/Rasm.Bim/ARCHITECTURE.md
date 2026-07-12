# [RASM_BIM_ARCHITECTURE]

The domain map of `Rasm.Bim` — the host-neutral AEC-DOMAIN BIM/IFC owner and the IFC arm of the `Rasm.Element` seam, depending UP on `{Rasm, Rasm.Element}` and projecting GeometryGym into the canonical `ElementGraph`. The `Model`, `Semantics`, `Planning`, `Exchange`, `Energy`, `Review`, and `Projection` sub-domains lower onto the one `BimFault` band. `Projection` hosts `SemanticProjector : IElementProjection` for GeometryGym `DatabaseIfc` ingress and IFC egress, while `IfcLegality : IGraphConstraint` owns IFC-semantic legality. The consumer-facing element is the seam `Bake(objectNode)` fold over the `ElementGraph`. Bim stays the SOLE GeometryGym/IFC owner and references no AEC peer; alignment with `Rasm.Materials`/`Rasm.Fabrication` travels through the shared seam graph and the content-keyed wire.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Bim/
├── Model/                 # Host-neutral BIM object model and analytical model
│   ├── Elements.cs        # Generated IfcClass taxonomy (offline IfcVocabularyEmitter over GeometryGym) + ReleaseMap
│   ├── Query.cs           # Set-algebraic ElementPredicate query folded over ElementSet
│   ├── Spatial.cs         # SpatialClass Rank vocabulary (IsRoot/IsContainer/CanContain) + SpatialStructure derived tree VIEW
│   ├── Zones.cs           # BimZone many-to-many zone/program overlay
│   ├── Systems.cs         # DistributionSystem derived MEP connectivity VIEW + directed SystemTrace + InterferenceCheck
│   ├── Structural.cs      # StructuralProjection reader lowering restraints/loads/groups onto neutral seam edge/bag payloads
│   └── Faults.cs          # Band-2600 BimFault closed [Union] entrypoint rail
├── Semantics/             # Element-bound semantic enrichment
│   ├── Properties.cs      # PropertyKey Pset/Qto TEMPLATE authority, bSDD DataType resolution, Property classifier, etc...
│   ├── Classification.cs  # bSDD-bound Classification axis with BsddResolution live dictionary
│   ├── Composition.cs     # MaterialProjection bidirectional GeometryGym↔seam material projector Project ingress / AuthorComposition+AuthorUsage egress
│   ├── Appearance.cs      # AppearanceProjection lowers IfcSurfaceStyle onto seam AppearanceSummary Node.Appearance, Materials-reconciled at content key
│   ├── Connection.cs      # ConnectionProjection reader lowering the GeometryGym realizing-element surface onto seam DetailSchema.Realization bags
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
│   ├── Reconstruct.cs     # Scan-to-BIM ReconstructionProjector over the dual-engine Themis.Las/laszip LAS/LAZ ingest front
│   └── Wire.cs            # Host-free IfcWire IFC interchange artifact the Python and TypeScript peers decode
├── Energy/                # Building-energy-model exchange
│   ├── Exchange.cs        # EnergyExchange.Apply over the closed EnergyOp [Union]
│   ├── Projector.cs       # EnergyProjector raises HBJSON/DFJSON/OSM/gbXML/IDF evidence
│   └── Derive.cs          # EnergyDerive BIM-to-BEM lower honeybee envelope + opaque/glazing library, dragonfly massing + EnergyTranslate OSM matrix
├── Review/                # Model-checking and coordination
│   ├── Validation.cs      # IDS owner folding six IdsFacet arms over the seam ElementGraph
│   ├── Issues.cs          # BCF issue exchange with .bcfzip codec and BcfApi REST projection
│   ├── Diff.cs            # GlobalId-plus-content-key ModelDiff federation change-set
│   ├── Coordination.cs    # CoordinationRule [Union] rule engine, ClashProposal fold, ImpactReport, BCF SignOff [SmartEnum]
│   └── Versioning.cs      # Content-addressed BimCommit DAG + three-way ElementChange Merge over the diff content-key
└── Projection/            # The IFC arm of the Rasm.Element seam
    ├── Semantic.cs        # SemanticProjector:IElementProjection ingress fold + IfcLegality:IGraphConstraint
    ├── Relations.cs       # IfcRelKind full IfcRel* roster + EdgeProjection.All neutral-edge lowering
    └── Egress.cs          # Emit IFC re-author: railed ReleaseRaise/Sniff, Instantiable + per-token AdmitPredefined gate
```

Every sub-domain projects onto or reads the one seam `ElementGraph` (the `Projection/Semantic` `SemanticProjector` lowers GeometryGym into it; the seam `Bake(objectNode)` fold derives the consumer element) rather than a parallel `BimModel` surface, lowers rejection onto the `Model/Faults` `BimFault` band, and consumes the `Model/Query` `ElementPredicate` algebra and the `Semantics/Classification` axis as settled vocabulary.

## [02]-[SEAMS]

```text seams
Projection/semantic       →  csharp:Rasm.Element/Graph # [PROJECTION]: SemanticProjector: DatabaseIfc→GraphDelta via Assemble
Projection/semantic       →  csharp:Rasm.Element/Projection # [PORT]: IfcLegality:IGraphConstraint IFC legality after seam structural law [M3]
Projection/semantic       ←  csharp:Rasm.Element/Graph # [SHAPE]: Node/Relationship/GraphDelta/NodeId/ProjectionContext/Assemble
Model/elements            →  csharp:Rasm.Element/Graph # [PROJECTION]: classification plus PredefinedType on Object
Semantics/composition     ⇄  csharp:Rasm.Element/Composition # [PROJECTION]: material sets→MaterialComposition/ProfileRef
Semantics/composition     ←  csharp:Rasm.Element/Composition # [SHAPE]: Cost per-unit doubles + ISO-4217 into NodaMoney algebra at 5D join
Semantics/properties      →  csharp:Rasm.Element/Properties # [PROJECTION]: PropertyKey→PropertyValue/MeasureValue
Semantics/connection      ⇄  csharp:Rasm.Element/Properties # [SHAPE]: seam DetailSchema + PropertyName
Semantics/properties      ←  csharp:Rasm # [CONTENT_KEY]: GeometryMeasures→QuantityDerivation.Derive
Semantics/georeference    →  csharp:Rasm.Element/Geospatial # [PROJECTION]: GeoReferenceProjector→Header.GeoReference
Semantics/geospatial      →  csharp:Rasm.Element/Graph # [PROJECTION]: GeoFeature.ToObject/GeoRaster.ToCoverage→Object/Coverage via GraphDelta
Exchange/tessellation     ⇄  python:geometry/mesh                     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
*                         →  typescript:core/interchange/codec        # [WIRE]: BcfTopicWire / BcfViewpointWire
Review/issues             →  typescript:ui/viewer                     # [WIRE]: BcfTopicWire / BcfViewpointWire
Exchange/import           →  python:geometry/ifc                      # [PROJECTION]: IFC semantic graph ingest via GeometryGym
Exchange/wire             →  python:geometry/ifc # [WIRE]: IfcWire bytes
Model/elements            →  typescript:ui/viewer                     # [WIRE]: GlobalId element selection set
Review/validation         ←  python:geometry/ifc                      # [BOUNDARY]: IDS validation evidence via ifctester
Model                     →  csharp:Rasm.Persistence/Query/columnar # [PROJECTION]: Persistence FlatTableProjection reads Bim-typed nodes
Model/structural          →  csharp:Rasm.Compute/Analysis # [SHAPE]: StructuralReads Supports/Loads on Generic edges
Semantics/appearance      ⇄  csharp:Rasm.Element/Composition # [CONTENT_KEY]: AppearanceSummary ↔ Materials Appearance at content key, no direct ref
Model                     ←  csharp:Rasm.Materials/Component # [WIRE]: IIfcTypeReconciler Type Object identity
Semantics/properties      ←  csharp:Rasm.Materials/Projection # [SHAPE]: round-trips IDENTICAL DetailSchema realization bag at IFC ingress/Emit
Model                     →  python:geometry/mesh                     # [SHAPE]: IFC GLB tessellation reference for scan-deviation analysis
Exchange/wire             →  python:geometry/energy # [SHAPE]: IFC SPF source bytes for geometry-side BIM-to-BEM derivation modality
Semantics                 →  csharp:Rasm.Compute/Runtime              # [PROJECTION]: IFC/glTF semantic metadata layer
Semantics/classification  ←  csharp:Rasm.Compute/Runtime/transport     # [TRANSPORT]: BsddPort injected bSDD GET /api/Class/v1 BsddClassResponse
Semantics/geospatial      →  python:data/spatial/geospatial # [WIRE]: GeoFeature WKB Geometry.ToBinary decode via shapely NTS-equivalent planar peer
Semantics/geospatial      →  typescript:core/interchange/codec # [WIRE]: GeoFeature WKB decode
Semantics/geospatial      →  csharp:Rasm.AppUi/Charts/basemap # [SHAPE]: NTS Feature geometry as Mapsui basemap overlays beside Wgpu viewport
Semantics/geospatial      ←  csharp:Rasm.Persistence/Store # [TRANSPORT]: GDAL /vsimem open + OGR Arrow C-stream GeoParquet/FlatGeobuf ingest
Semantics/geospatial      ⇄  Semantics/georeference # [PROJECTION]: GeoFeature.Reproject ProjNET GEODETIC_TRANSFORM
Model                     →  csharp:Rasm.Compute/Runtime/codecs # [CONTENT_KEY]: IfcRepresentation.Keys [M2] off kernel seed-zero
Planning/cost             →  csharp:Rasm.AppUi/Charts # [RECEIPT]: CostSchedule EarnedValue/ChangeOrder report as Charts/dashboards projection
Exchange/tessellation     →  csharp:Rasm.Compute/Runtime/codecs # [TESSELLATION]: TessellationOutcome GLB
Exchange/tessellation     →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: TessellationOutcome ArtifactKey cache-hit lookup
Exchange/import           →  csharp:Rasm.Persistence/Query # [CONTENT_KEY]: Reimport prior seam ElementGraph snapshot content-key delta join
Exchange/import           ←  csharp:Rasm.Rhino/Exchange               # [BOUNDARY]: [^1]
Exchange/wire             →  csharp:Rasm.Persistence/Query # [CONTENT_KEY]: IfcWire ContentAddress.OfGraph
Review/diff               →  csharp:Rasm.Persistence/Query/federation # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log
Review/versioning         →  csharp:Rasm.Persistence/Version/commits  # [CONTENT_KEY]: BimCommit commit-DAG stored as CommitNode by the wire CommitKey
Exchange/wire             ←  csharp:Rasm.Persistence/Element/codec # [BOUNDARY]: snapshot/op-log wire = Persistence SnapshotCodec
Review/versioning         →  csharp:Rasm.Persistence/Version/commits  # [SHAPE]: BimCommit DAG common-ancestor merge substrate (CommitGraph.MergeBase)
Model                     →  csharp:Rasm.Persistence/Store/quality    # [SHAPE]: IFC validation rules into QualityRule rows
Review/validation         →  csharp:Rasm.Compute/Runtime/codecs       # [TRANSPORT]: IdsAudit ifctester oracle two-hop rpc, GlobalId-plus-facet diff
Exchange/format           →  csharp:Rasm.Fabrication/Ingress          # [SHAPE]: ACadSharp read; Bim projects meshes, Fabrication 2D profiles
Exchange/wire             →  typescript:core/interchange/codec # [WIRE]: IfcWire WireParity
Exchange/wire             →  typescript:ui/viewer                     # [WIRE]: BcfWire/DiffWire GlobalId anchor decode
coordination              ⇄  csharp:Rasm.Persistence/Version/ledger # [WIRE]: durable annotation + CDE rows through OpLogEntry/ReplayWindow op-log
schedule                  ⇄  csharp:Rasm.Persistence/Version/ledger # [WIRE]: P6/MS-Project + 4D construction changefeed rows through same op-log
coordination              →  csharp:Rasm.AppUi/Collab/issues          # [PORT]: BCF issue-board projection
Exchange/import           ⇄  csharp:Rasm.Persistence/Version/ledger # [TRANSPORT]: Speckle Base→ElementGraph diff rows over op-log owner
Exchange/tessellation     ⇄  csharp:Rasm.Compute # [SHAPE]: Bim EXT_structural_metadata/EXT_mesh_features glTF
Energy/projector          ⇄  csharp:Rasm.Compute/Analysis # [SHAPE]: OpenStudio SIMULATION Compute vs Bim EXCHANGE
Projection/semantic       →  csharp:Rasm.Compute/Analysis # [PROJECTION]: IfcRelSpaceBoundary/FootPrint/Qto/IsExternal shape EnergyGraphReads reads
Energy/exchange           →  csharp:Rasm.Persistence/Store/blobstore # [CONTENT_KEY]: EnergyArtifact content-keyed, write-blob-first
Energy/exchange           ⇄  python:geometry/energy # [WIRE]: HBJSON/DFJSON bytes, XxHash128 identity
Planning                  ⇄  csharp:Rasm.Compute/Analysis # [SHAPE]: schedule/4D MPXJ Bim vs material-cost takeoff Compute
Exchange                  →  csharp:Rasm.Persistence/Store/blobstore # [CONTENT_KEY]: imported IFC/BREP by IfcRepHash
```
- App-root RhinoDoc import projects to host-neutral mesh plus `GlobalId`; the app path selects the authoritative reader.

The `[CONTENT_KEY]` seam rows above are one canonical idiom, not per-page schemes: every page that joins the federation, solver, cache, or diff lane derives a typed `UInt128` key through the ONE kernel seed-zero hasher — `ContentHash.Of` over the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` `CanonicalWriter` fold — and the `csharp:Rasm.Compute/Runtime/codecs` `CONTENT_ADDRESSING` lane joins the same content space at the seam, never a second identity scheme and never a downward `InterchangeIdentity` reference from Bim.

[CONTENT_KEY_IDIOM]:
- `Model/structural` carries NO parallel model key — the analytical member resolves one-hop by its `Representations.Axis` content key; restraint/load payloads ride the neutral `Generic` edges.
- `Model/systems` derives `(MembershipKey, TopologyKey)` — the ordered member-id hash plus the sorted flow-edge topology hash, the trace memoization key.
- `Planning/schedule` derives `(GeometryKey, ScheduleKey)` — the element geometry plus its 4D task-time hash, the schedule catalog read.
- `Planning/cost` derives `(QuantityKey, ResourceKey)` — the quantity-set hash plus the per-value resource-rate hash, the cost-rollup join.
- `Review/diff` derives `(ContentKey, PlacementKey)` — the element fingerprint plus its placement hash, the federation dedup and three-way merge anchor.
- `Exchange/tessellation`, `Exchange/export`, `Exchange/import`, and `Energy/exchange` mint the artifact `ContentKey` through the same kernel `ContentHash` + `CanonicalWriter` fold, and `Exchange/wire` stamps the serialization-independent seam `ContentAddress.OfGraph` (the `Energy/exchange` `EnergyArtifact.Graph` pedigree reuses it) — so the tessellation cache, the emitted artifact, the reimport delta, the energy-model document, and the wire identity all address one content space.

A second identity scheme, a per-page hash function, or a `Guid`-keyed federation join is the named cross-folder drift defect: a page deriving a new content key inherits this idiom from the map and mints through the one kernel seed-zero `XxHash128` owner.

[HOST_BOUNDARY_EDGE]: the `Exchange/import ← csharp:Rasm.Rhino/Exchange` row is a single-sided HOST-BOUNDARY edge, not an interior dependency — `Rasm.Rhino/Exchange` is strata-locked, every format dispatching through RhinoCommon `Rhino.FileIO.*`/`RhinoDoc` with zero host-neutral parts, so it references only the kernel `Rasm` and is composed exclusively at the app root. `Rasm.Bim` never names `Rasm.Rhino`; the edge resolves only where the app root binds the live Rhino host, projecting a `RhinoDoc` import to a host-neutral mesh plus `GlobalId` the `Exchange/import` fold admits as a wire payload. Bim owns the wire payload; Rhino owns the Rhino-side production and the projection adapter. Because the two reader engines — Rhino FileIO and the managed `PlyReader`/`ThreeMfReader`/`StepReader` plus the SharpGLTF/`geometry3Sharp` arms — can decode the same OBJ/STL/PLY/3MF/glTF/STEP bytes to divergent meshes, the app root declares per path which reader is authoritative; the two coexist (Rhino-native capture and host-neutral managed ingest) and neither is gutted to feed the other.
