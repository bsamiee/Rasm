# [RASM_BIM_ARCHITECTURE]

The domain map of `Rasm.Bim` — the host-neutral AEC-DOMAIN BIM object model and IFC/glTF/STEP exchange. The `Model`, `Semantics`, `Planning`, `Exchange`, and `Review` sub-domains, each composing the one `BimModel` and lowering onto the one `BimFault` band.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Bim/
├── Model/                 # Host-neutral BIM object model and analytical model
│   ├── Elements.cs        # IfcClass-discriminated BimElement record + BimModel.Project fold
│   ├── Query.cs           # Set-algebraic ElementPredicate query folded over ElementSet
│   ├── Structure.cs       # Spatial-structure tree + closed AssemblyRel decomposition
│   ├── Zones.cs           # BimZone many-to-many zone/program overlay
│   ├── Systems.cs         # DistributionSystem MEP connectivity graph with SystemTrace fold
│   ├── Structural.cs      # IFC structural-analysis AnalysisModel the Compute solver reads
│   └── Faults.cs          # Band-2600 BimFault closed [Union] every entrypoint lowers onto
├── Semantics/             # Element-bound semantic enrichment
│   ├── Properties.cs      # Typed PropertySet/QuantitySet with type-vs-occurrence inheritance
│   ├── Classification.cs  # bSDD-bound Classification axis with BsddResolution live dictionary
│   ├── Composition.cs     # BimMaterial construction-material composition [Union]
│   ├── Appearance.cs      # BimAppearance PBR record reconciled with Rasm.Materials at content-key seam
│   ├── Connection.cs      # ConnectionDetail realizing-element joint [Union] (Bolted/Welded/Bearing/Cast) over IfcRelConnectsWithRealizingElements
│   └── GeoReference.cs    # GeoReference IFC4.3 owner with ProjNET datum-to-datum reprojection
├── Planning/              # 4D/5D delivery network
│   ├── Schedule.cs        # ConstructionTask 4D activity schedule over IfcTaskTime intervals
│   └── Cost.cs            # CostItem 5D cost-and-resource estimate with CostSchedule.Rollup fold
├── Exchange/              # Universal interchange codec
│   ├── Format.cs          # Format/codec/extension axis plus FrameNormalization
│   ├── Import.cs          # BimIo foreign-bytes ingest fold
│   ├── Export.cs          # BimExport emit fold with per-tile EXT_structural_metadata
│   ├── Tessellation.cs    # TessellationRequest Compute companion bridge
│   ├── Reconstruct.cs     # Scan-to-BIM ReconstructionPrimitive [Union] fitting fold
│   └── Wire.cs            # Host-free BimWire projection the Python and TypeScript peers decode
└── Review/                # Model-checking and coordination
    ├── Validation.cs      # IDS v1.0 owner folding six IdsFacet arms over BimModel
    ├── Issues.cs          # BCF 3.0 issue exchange with .bcfzip codec and BcfApi REST projection
    ├── Diff.cs            # GlobalId-plus-content-key ModelDiff federation change-set
    ├── Coordination.cs    # CoordinationRule [Union] rule engine, ClashProposal fold, ImpactReport, BCF SignOff [SmartEnum]
    └── Versioning.cs      # Content-addressed BimCommit DAG + three-way ElementChange Merge over the diff content-key
```

Every sub-domain composes the one `BimModel` the `Exchange` import rail produces rather than a parallel model surface, lowers rejection onto the `Model/Faults` `BimFault` band, and consumes the `Model/Query` `ElementPredicate` algebra and the `Semantics/Classification` axis as settled vocabulary.

## [02]-[SEAMS]

```text seams
Exchange/tessellation     ⇄  python:geometry/mesh                     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
*                         →  typescript:interchange                   # [WIRE]: BcfTopicWire / BcfViewpointWire
Review/issues             →  typescript:ui/overlay                    # [WIRE]: BcfTopicWire / BcfViewpointWire
Exchange/import           →  python:geometry/ifc                      # [PROJECTION]: IFC semantic graph ingest via GeometryGym
Exchange/wire             →  python:geometry/ifc                      # [PROJECTION]: BimWire model vocabulary
Model/elements            →  typescript:ui/overlay                    # [SHAPE]: GlobalId element selection set
Review/validation         ←  python:geometry/ifc                      # [BOUNDARY]: IDS validation evidence via ifctester
Model                     ←  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: ArtifactIndexRow IfcSemantic content-addressed model graph
Model/structural          →  csharp:Rasm.Compute/Solver               # [CONTENT_KEY]: AnalysisModel (GeometryKey, PropertyKey) content-key
Semantics/*               ←  csharp:Rasm.Materials                    # [CONTENT_KEY]: BimAppearance
Model/query               →  csharp:Rasm.AppUi/Render                 # [PORT]: ElementSet query algebra via capability descriptor
Model                     ←  csharp:Rasm.Materials/Connection         # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener
Semantics                 ←  csharp:Rasm.Materials/Construction       # [PROJECTION]: MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet
Semantics                 →  csharp:Rasm.Compute/Runtime              # [PROJECTION]: IFC/glTF semantic metadata layer
Semantics/connection      ←  csharp:Rasm.Materials/Connection         # [WIRE]: ConnectionItem realizing-element IfcMechanicalFastener...
Semantics/classification  ←  csharp:Rasm.Compute/Runtime/channels     # [TRANSPORT]: BsddPort injected bSDD GET /api/Class/v1 BsddClassResponse
Model                     →  csharp:Rasm.Compute/Runtime/codecs       # [CONTENT_KEY]
Model/structural          →  csharp:Rasm.AppUi/Schedule               # [RECEIPT]: CriticalPath/EarnedValue schedule-and-cost report
Planning/schedule         →  csharp:Rasm.AppUi/Schedule               # [RECEIPT]: ScheduleNetwork CPM/calendar/4D report
Planning/cost             →  csharp:Rasm.AppUi/Schedule               # [RECEIPT]: CostSchedule EarnedValue/ChangeOrder report
Exchange/tessellation     →  csharp:Rasm.Compute/Runtime/codecs       # [TESSELLATION]: TessellationOutcome two-hop GLB, CacheHit by ArtifactKey
Exchange/tessellation     →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: TessellationOutcome ArtifactKey cache-hit lookup
Exchange/import           →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: Reimport prior-BimModel content-key delta join
Exchange/import           ←  csharp:Rasm.Rhino/Exchange               # [BOUNDARY]: [^1]
Exchange/wire             →  csharp:Rasm.Persistence/Query            # [CONTENT_KEY]: BimWire snapshot content-key ArtifactIndexRow join
Review/diff               →  csharp:Rasm.Persistence/Query/federation # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log
Review/versioning         →  csharp:Rasm.Persistence/Query/federation # [CONTENT_KEY]: BimCommit content-addressed commit-DAG
Exchange/wire             →  csharp:Rasm.Persistence/Sync             # [TRANSPORT]: OpLogWire ElementChange op-stream CRDT convergence
Review/versioning         →  csharp:Rasm.Persistence/Sync             # [SHAPE]: BimCommit DAG common-ancestor merge substrate
Model                     →  csharp:Rasm.Persistence/Store/quality    # [SHAPE]: IFC validation rules into QualityRule rows
Review/validation         →  csharp:Rasm.Compute/Runtime/codecs       # [TRANSPORT]: IdsAudit ifctester oracle two-hop rpc, GlobalId-plus-facet diff
Review/validation         →  python:geometry/ifc-companion            # [BOUNDARY]: ifctester IDS-XML conformance oracle verdict
Exchange/format           ⇄  csharp:Rasm.Fabrication                  # [SHAPE]: ACadSharp managed DWG/DXF DxfDocument/CadDocument codec
Exchange/wire             →  typescript:interchange                   # [WIRE]: BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity
Exchange/wire             →  typescript:ui/overlay                    # [WIRE]: BcfWire/DiffWire GlobalId anchor decode
coordination              ⇄  csharp:Rasm.Persistence/Sync/annotation  # [WIRE]: durable annotation + CDE op-log
schedule                  ⇄  csharp:Rasm.Persistence/Sync/schedule    # [WIRE]: durable schedule store
coordination              →  csharp:Rasm.AppUi/Editing/issues         # [PORT]: BCF issue-board projection
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
