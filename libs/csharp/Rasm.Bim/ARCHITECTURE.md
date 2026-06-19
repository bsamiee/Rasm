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
    └── Diff.cs            # GlobalId-plus-content-key ModelDiff federation change-set
```

Every sub-domain composes the one `BimModel` the `Exchange` import rail produces rather than a parallel model surface, lowers rejection onto the `Model/Faults` `BimFault` band, and consumes the `Model/Query` `ElementPredicate` algebra and the `Semantics/Classification` axis as settled vocabulary.

## [02]-[SEAMS]

```text seams
Exchange/tessellation  ⇄  python:geometry/mesh                # [TESSELLATION]: GLB tessellation rail / TessellationRequest
*                      →  typescript:interchange              # [WIRE]: BcfTopicWire / BcfViewpointWire
Review/issues          →  typescript:ui/overlay               # [WIRE]: BcfTopicWire / BcfViewpointWire
Exchange/import        →  python:geometry/ifc                 # [PROJECTION]: IFC semantic graph ingest via GeometryGym
Exchange/wire          →  python:geometry/ifc                 # [PROJECTION]: BimWire model vocabulary
Model/elements         →  typescript:ui/overlay               # [SHAPE]: GlobalId element selection set
Review/validation      ←  python:geometry/ifc                 # [BOUNDARY]: IDS validation evidence via ifctester
Model                  ←  csharp:Rasm.Persistence/Query       # [CONTENT_KEY]: ArtifactIndexRow IfcSemantic content-addressed model graph
Model/structural       →  csharp:Rasm.Compute/Solver          # [CONTENT_KEY]: AnalysisModel (GeometryKey, PropertyKey) content-key
Semantics/*            ←  csharp:Rasm.Materials               # [CONTENT_KEY]: BimAppearance
Model/query            →  csharp:Rasm.AppUi/Render            # [PORT]: ElementSet query algebra via capability descriptor
Model                  ←  csharp:Rasm.Materials/Connection    # [WIRE]: ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener
Semantics              ←  csharp:Rasm.Materials/Construction  # [PROJECTION]: MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet
Semantics              →  csharp:Rasm.Compute/Runtime         # [PROJECTION]: IFC/glTF semantic metadata layer
```
