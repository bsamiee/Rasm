# [FABRICATION_ARCHITECTURE]

The professional domain map of `Rasm.Fabrication` — the host-neutral AEC-DOMAIN portable-fabrication frontier over the `Rasm` kernel. HLR/hidden-line projection, CAM toolpath plus serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter over a shared Clipper2 polygon floor.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/            # the polymorphic Fabrication owner and the Process/Machine axis
│   ├── Owner.cs        # the polymorphic Fabrication owner with one Run generated Switch
│   ├── Family.cs       # the process/machine removal-modality discriminant every kernel reads
│   ├── Physics.cs      # the removal-physics table projecting the modality-discriminated RemovalBudget
│   └── Faults.cs       # the FabricationFault band-2500 composing the kernel band-2400
├── Polygon/            # the 2D polygon-algebra substrate over Clipper2 and the profile-ingress boundary
│   ├── Clipper.cs      # the 2D polygon algebra over Clipper2: offset, Boolean clip, Minkowski sum
│   └── Import.cs       # DXF/DWG profile ingress tessellated into Loop through ACadSharp
├── Toolpath/           # CAM motion plus the kinematic and additive author-kernels
│   ├── Motion.cs       # the (Process, ToolpathKind) CAM move family over Geometry2D offset
│   ├── Skeleton.cs     # the straight-skeleton/medial-axis trochoidal primitive
│   ├── Slicing.cs      # the FFF/DED planar-section-and-infill slicer
│   └── Kinematics.cs   # the DH/damped-least-squares serial-chain IK solver
├── Nesting/            # 2D true-shape nesting plus workholding keep-out
│   ├── Nfp.cs          # NFP-feasibility true-shape nesting with bottom-left/genetic placement
│   └── WorkHolding.cs  # the Workholder keep-out family conditioning toolpath and cut sequence
└── Posting/            # host-neutral downstream emission: cut-program and hidden-line projection
    ├── Program.cs      # the dialect-neutral G-code AST and the PostDialect cut-conditioning family
    └── Projection.cs   # the BSP front-to-back HLR projection for the AppUi Viewport2D
```

The `Process` owner and the `Polygon` substrate are read by every kernel, the `Posting` emitter is the downstream consumer, and `Process/Faults` carries the fabrication fault rail.

## [2]-[SEAMS]

```text seams
Posting             ←  csharp:Rasm/Geometry/Meshing      # IntersectResult / PlaneMesh section curve (wire)
Posting             →  csharp:Rasm/Geometry/Numerics     # Predicate.Orient2D/Orient3D exact verdict (wire)
Process/physics     ←  csharp:Rasm.Materials/Properties  # Thermal Conductivity / SpecificHeat / Density scalars (wire)
Process/physics     ←  csharp:Rasm.Compute/Symbolic      # UnitsNet quantity canonicalization to SI scalar (wire)
Posting             ←  csharp:Rasm/Geometry/Drawing      # DrawingProjection / HLR visible/hidden segments (projection)
Posting             ←  csharp:Rasm/Geometry/Processing   # ChartAtlas / UV island layout + DistortionReceipt (projection)
Posting/projection  →  csharp:Rasm.AppUi/Render          # HiddenLineResult Viewport2D edge sets (receipt)
*                   →  csharp:Rasm                       # Matrix / Point3d / Vector3d (shape)
Posting/projection  →  csharp:Rasm/Geometry/Spatial      # SpatialIndex BVH broad-phase (shape)
Render/drafting     →  csharp:Rasm.AppUi/Render          # HiddenLineSeam BSP visibility solver (boundary)
```

## [3]-[PLANNED_DEPTH]

The settled author-kernels — the `Polygon/clipper` polygon algebra, the `Toolpath/skeleton` wavefront-event resolution, the `Posting/program` cut-conditioning fold, the `Nesting/nfp` NFP feasibility, the `Toolpath/kinematics` DH/IK solver, and the `Posting/projection` BSP visibility — are correct for the milling instance but bake a single hidden process (subtractive CNC milling) into the feeds-and-speeds, the toolpaths, and the post words. The recovery is the `Process/family` axis pair: the `Process`/`Machine` discriminant (mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire and their machines) is the one fabrication axis the de-hardcode hangs on. From it, `Process/physics` widens to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive), `Toolpath/motion` widens to the `(Process, ToolpathKind)` move family (lathe turning, thermal contour, additive slice-layer beside the milling four), `Posting/program` widens to the `PostDialect` family over the dialect-neutral AST, `Nesting/nfp` widens its rectangle to the `Stock` union, and `Nesting/workholding` rides the workholding kind as a `HoldingClass`-keyed `WorkholderKind` footprint-shape column on the concrete `Clamp` — each a case/row/column on a settled owner, never a parallel pipeline.

The two new sub-domains close the real fabrication-input and additive frontiers the subtractive-biased folder lacks: `Toolpath/slicing` is the FFF/DED planar-section-and-infill author-kernel over the one Geometry2D substrate (no managed slicer exists, the author-kernel posture is the `STRAIGHT_SKELETON` precedent), and `Polygon/import` is the portable DXF/DWG profile-ingress boundary through the pure-managed `ACadSharp` reader (host-neutral, coexisting with Rhino-native I/O, no RID burden). The `CSG_SILHOUETTE` watertight-solid outline is held in `IDEAS.md` as a compose-the-kernel-arrangement task against the C# branch `ROBUST_ARRANGEMENT_SUBSTRATE` managed exact path (the `Rasm.Geometry/Meshing/arrangement#ARRANGEMENT` `Arrangement` `[Union]` owner — its DESIGN page is authored, so the compose-target anchor is settled), not a native-asset block; the per-facet `Posting/projection` kernel stays pure-managed until the realized C# arrangement owner lands.
