# [FABRICATION_ARCHITECTURE]

The domain map of `Rasm.Fabrication` — the host-neutral AEC-DOMAIN portable-fabrication owner over the `Rasm` kernel. HLR/hidden-line projection, CAM toolpath plus serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter over a shared Clipper2 polygon floor.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/            # Polymorphic Fabrication owner and Process/Machine axis
│   ├── Owner.cs        # Polymorphic Fabrication owner with one Run generated Switch
│   ├── Family.cs       # Process/machine removal-modality discriminant every kernel reads
│   ├── Physics.cs      # Removal-physics table projecting modality-discriminated RemovalBudget
│   └── Faults.cs       # FabricationFault band-2500 composing kernel band-2400
├── Polygon/            # 2D polygon-algebra substrate over Clipper2 and profile-ingress boundary
│   ├── Clipper.cs      # 2D polygon algebra over Clipper2: offset, Boolean clip, Minkowski sum
│   └── Import.cs       # DXF/DWG profile ingress tessellated into Loop through ACadSharp
├── Toolpath/           # CAM motion plus kinematic and additive author-kernels
│   ├── Motion.cs       # (Process, ToolpathKind) CAM move family over Geometry2D offset
│   ├── Skeleton.cs     # Straight-skeleton/medial-axis trochoidal primitive
│   ├── Slicing.cs      # FFF/DED planar-section-and-infill slicer
│   └── Kinematics.cs   # DH/damped-least-squares serial-chain IK solver
├── Nesting/            # 2D true-shape nesting plus workholding keep-out
│   ├── Nfp.cs          # NFP-feasibility true-shape nesting with bottom-left/genetic placement
│   └── WorkHolding.cs  # Workholder keep-out family conditioning toolpath and cut sequence
└── Posting/            # Host-neutral downstream emission: cut-program and hidden-line projection
    ├── Program.cs      # Dialect-neutral G-code AST and PostDialect cut-conditioning family
    └── Projection.cs   # BSP front-to-back HLR projection for AppUi Viewport2D
```

The `Process` owner and the `Polygon` substrate are read by every kernel, the `Posting` emitter is the downstream consumer, and `Process/Faults` carries the fabrication fault rail.

## [2]-[SEAMS]

```text seams
Posting             ←  csharp:Rasm/Geometry/Meshing      # [WIRE]: IntersectResult / PlaneMesh section curve
Posting             →  csharp:Rasm/Geometry/Numerics     # [WIRE]: Predicate.Orient2D/Orient3D exact verdict
Process/physics     ←  csharp:Rasm.Materials/Properties  # [WIRE]: Thermal Conductivity / SpecificHeat / Density scalars
Process/physics     ←  csharp:Rasm.Compute/Symbolic      # [WIRE]: UnitsNet quantity canonicalization to SI scalar
Posting             ←  csharp:Rasm/Geometry/Drawing      # [PROJECTION]: DrawingProjection / HLR visible/hidden segments
Posting             ←  csharp:Rasm/Geometry/Processing   # [PROJECTION]: ChartAtlas / UV island layout + DistortionReceipt
Posting/projection  →  csharp:Rasm.AppUi/Render          # [RECEIPT]: HiddenLineResult Viewport2D edge sets
*                   →  csharp:Rasm                       # [SHAPE]: Matrix / Point3d / Vector3d
Posting/projection  →  csharp:Rasm/Geometry/Spatial      # [SHAPE]: SpatialIndex BVH broad-phase
Render/drafting     →  csharp:Rasm.AppUi/Render          # [BOUNDARY]: HiddenLineSeam BSP visibility solver
```

## [3]-[PLANNED_DEPTH]

The settled author-kernels — the `Polygon/clipper` polygon algebra, the `Toolpath/skeleton` wavefront-event resolution, the `Posting/program` cut-conditioning fold, the `Nesting/nfp` NFP feasibility, the `Toolpath/kinematics` DH/IK solver, and the `Posting/projection` BSP visibility — are correct for the milling instance but bake a single hidden process (subtractive CNC milling) into the feeds-and-speeds, the toolpaths, and the post words. The recovery is the `Process/family` axis pair: the `Process`/`Machine` discriminant (mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire and their machines) is the one fabrication axis the de-hardcode hangs on. From it, `Process/physics` widens to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive), `Toolpath/motion` widens to the `(Process, ToolpathKind)` move family (lathe turning, thermal contour, additive slice-layer beside the milling four), `Posting/program` widens to the `PostDialect` family over the dialect-neutral AST, `Nesting/nfp` widens its rectangle to the `Stock` union, and `Nesting/workholding` rides the workholding kind as a `HoldingClass`-keyed `WorkholderKind` footprint-shape column on the concrete `Clamp` — each a case/row/column on a settled owner, never a parallel pipeline.

The two new sub-domains close the real fabrication-input and additive gaps the subtractive-biased folder lacks: `Toolpath/slicing` is the FFF/DED planar-section-and-infill author-kernel over the one Geometry2D substrate (no managed slicer exists, the author-kernel posture is the `STRAIGHT_SKELETON` precedent), and `Polygon/import` is the portable DXF/DWG profile-ingress boundary through the pure-managed `ACadSharp` reader (host-neutral, coexisting with Rhino-native I/O, no RID burden). The `CSG_SILHOUETTE` watertight-solid outline is held in `IDEAS.md` as a compose-the-kernel-arrangement task against the C# branch `ROBUST_ARRANGEMENT_SUBSTRATE` managed exact path (the `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT` `Arrangement` `[Union]` owner — its DESIGN page is authored, so the compose-target anchor is settled), not a native-asset block; the per-facet `Posting/projection` kernel stays pure-managed until the realized C# arrangement owner lands.
