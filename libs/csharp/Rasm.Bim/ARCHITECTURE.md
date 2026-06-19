# [RASM_BIM_ARCHITECTURE]

The professional domain map of `Rasm.Bim` — the host-neutral AEC-DOMAIN BIM object model and IFC/glTF/STEP exchange. Five sub-domains (`Model`, `Semantics`, `Planning`, `Exchange`, `Review`), each composing the one `BimModel` and lowering onto the one `BimFault` band.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm.Bim/
├── Model/                 # host-neutral BIM object model and analytical model
│   ├── Elements.cs        # IfcClass-discriminated BimElement record + the BimModel.Project fold
│   ├── Query.cs           # set-algebraic ElementPredicate query folded over the ElementSet
│   ├── Structure.cs       # the spatial-structure tree + closed AssemblyRel decomposition
│   ├── Zones.cs           # the BimZone many-to-many zone/program overlay
│   ├── Systems.cs         # the DistributionSystem MEP connectivity graph with SystemTrace fold
│   ├── Structural.cs      # the IFC structural-analysis AnalysisModel the Compute solver reads
│   └── Faults.cs          # the band-2600 BimFault closed [Union] every entrypoint lowers onto
├── Semantics/             # element-bound semantic enrichment
│   ├── Properties.cs      # typed PropertySet/QuantitySet with type-vs-occurrence inheritance
│   ├── Classification.cs  # the bSDD-bound Classification axis with BsddResolution live dictionary
│   ├── Composition.cs     # the BimMaterial construction-material composition [Union]
│   ├── Appearance.cs      # the BimAppearance PBR record reconciled with Rasm.Materials at the content-key seam
│   └── GeoReference.cs    # the GeoReference IFC4.3 owner with ProjNET datum-to-datum reprojection
├── Planning/              # the 4D/5D delivery network
│   ├── Schedule.cs        # the ConstructionTask 4D activity schedule over IfcTaskTime intervals
│   └── Cost.cs            # the CostItem 5D cost-and-resource estimate with CostSchedule.Rollup fold
├── Exchange/              # universal interchange codec
│   ├── Format.cs          # the format/codec/extension axis plus FrameNormalization
│   ├── Import.cs          # the BimIo foreign-bytes ingest fold
│   ├── Export.cs          # the BimExport emit fold with per-tile EXT_structural_metadata
│   ├── Tessellation.cs    # the TessellationRequest Compute companion bridge
│   ├── Reconstruct.cs     # the scan-to-BIM ReconstructionPrimitive [Union] fitting fold
│   └── Wire.cs            # the host-free BimWire projection the Python and TypeScript peers decode
└── Review/                # model-checking and coordination
    ├── Validation.cs      # the IDS v1.0 owner folding six IdsFacet arms over BimModel
    ├── Issues.cs          # the BCF 3.0 issue exchange with .bcfzip codec and BcfApi REST projection
    └── Diff.cs            # the GlobalId-plus-content-key ModelDiff federation change-set
```

Every sub-domain composes the one `BimModel` the `Exchange` import rail produces rather than a parallel model surface, lowers rejection onto the `Model/Faults` `BimFault` band, and consumes the `Model/Query` `ElementPredicate` algebra and the `Semantics/Classification` axis as settled vocabulary.

## [2]-[SEAMS]

```text seams
Exchange/tessellation  ⇄  python:geometry/mesh                # GLB tessellation rail / TessellationRequest (tessellation)
*                      →  typescript:interchange              # BcfTopicWire / BcfViewpointWire (wire)
Review/issues          →  typescript:ui/overlay               # BcfTopicWire / BcfViewpointWire (wire)
Exchange/import        →  python:geometry/ifc                 # IFC semantic graph ingest via GeometryGym (projection)
Exchange/wire          →  python:geometry/ifc                 # BimWire model vocabulary (projection)
Model/elements         →  typescript:ui/overlay               # GlobalId element selection set (shape)
Review/validation      ←  python:geometry/ifc                 # IDS validation evidence via ifctester (boundary)
Model                  ←  csharp:Rasm.Persistence/Query       # ArtifactIndexRow IfcSemantic content-addressed model graph (content-key)
Model/structural       →  csharp:Rasm.Compute/Solver          # AnalysisModel (GeometryKey, PropertyKey) content-key (content-key)
Semantics/*            ←  csharp:Rasm.Materials               # BimAppearance (content-key)
Model/query            →  csharp:Rasm.AppUi/Render            # ElementSet query algebra via capability descriptor (port)
Model                  ←  csharp:Rasm.Materials/Connection    # ConnectionItem IFC wire IfcReinforcingBar/IfcMechanicalFastener (wire)
Semantics              ←  csharp:Rasm.Materials/Construction  # MaterialAssignment IFC trichotomy LayerSet/ProfileSet/ConstituentSet (projection)
Semantics              →  csharp:Rasm.Compute/Runtime         # IFC/glTF semantic metadata layer (projection)
```
