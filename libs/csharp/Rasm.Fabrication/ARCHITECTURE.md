# [FABRICATION_ARCHITECTURE]

The domain map of `Rasm.Fabrication` — the host-neutral AEC-DOMAIN portable-fabrication owner over `{Rasm, Rasm.Element}` (the kernel plus the shared lowest-AEC element seam). HLR/hidden-line projection, CAM toolpath plus serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter over a shared Clipper2 polygon floor. It depends up on the element seam as a future third `IElementProjection` (one registration row) and references no AEC peer — alignment travels through the seam contracts and the content-keyed wire, never sibling coupling.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/            # Polymorphic Fabrication owner and Process/Machine axis
│   ├── Owner.cs        # Polymorphic Fabrication owner with one Run generated Switch
│   ├── Family.cs       # Process/machine removal-modality discriminant; PostDialect override resolves independently of the Process-bound default
│   ├── Physics.cs      # Removal-physics table projecting modality-discriminated RemovalBudget plus the (Material, Tool, Operation) cutting-data table
│   ├── Magazine.cs     # Tool-magazine carousel/turret/manual slot map, ToolAssembly holder geometry, and minimal-swap tool-change schedule
│   └── Faults.cs       # FabricationFault band-2700 composing kernel band-2400
├── Polygon/            # 2D polygon-algebra substrate over Clipper2 and profile-ingress boundary
│   ├── Clipper.cs      # 2D polygon algebra over Clipper2: offset, Boolean clip, Minkowski sum/diff, variable-delta offset
│   └── Import.cs       # DXF/DWG profile ingress tessellated into Loop through ACadSharp
├── Toolpath/           # CAM motion plus kinematic, safety, metrology, and additive author-kernels
│   ├── Motion.cs       # CutStrategy × RemovalModality CAM move family over Geometry2D offset
│   ├── Skeleton.cs     # Trochoidal fold over the kernel clearance family: Offsetting.Apply Medial + Skeletonize.Apply CurveSkeleton; medial solver dies
│   ├── Slicing.cs      # FFF/DED infill + deposition planner over the kernel Slicing.Apply SliceStack wire; the in-folder planar-section author-kernel dies
│   ├── Kinematics.cs   # Robots-cell serial-chain solver (FK/IK, reach/limit/singularity validation, cell-dialect post)
│   ├── Guard.cs        # Swept tool-plus-holder collision/gouge guard and collision-aware lift retract
│   └── Probing.cs      # QUEUED (IDEAS): touch-probe cycles, work-offset/tool-length metrology, measured-feature receipt; anchors in Faults/Magazine/Posting
├── Nesting/            # 2D true-shape nesting, the rectangular cutting-stock yield engine, plus multi-setup workholding keep-out
│   ├── Nfp.cs          # NFP-feasibility true-shape nesting over a Seq<Stock> inventory with bottom-left/genetic placement
│   ├── Stock.cs        # StockNest.Pack fold over the NestStrategy [Union] collapsing five RectangleBinPack packers into NestPlan/NestYield engine
│   └── WorkHolding.cs  # Workholder keep-out family plus multi-fixture-setup scheduler conditioning toolpath and cut sequence
└── Posting/            # Host-neutral downstream emission: cut-program and hidden-line projection
    ├── Program.cs      # Dialect-neutral G-code AST and PostDialect cut-conditioning family
    └── Projection.cs   # Drafting emission over the kernel DrawingProjection exact analytic HLR for AppUi Viewport2D; the in-folder BSP solver dies
```

The `Process` owner and the `Polygon` substrate are read by every kernel, the `Posting` emitter is the downstream consumer, and `Process/Faults` carries the fabrication fault rail.

## [02]-[SEAMS]

```text seams
*                   →  csharp:Rasm.Element/Projection    # [PROJECTION]: FabricationProjector:IElementProjection lowers onto the seam ElementGraph [§4A]
Posting/projection  ←  csharp:Rasm/Numerics/predicates   # [WIRE]: Predicate.Orient2D/Orient3D exact silhouette/winding verdict
Posting/projection  ←  csharp:Rasm/Meshing/arrangement   # [WIRE]: Arrangement Apply/ToMesh kept-cell watertight outline, BooleanSolid arm
Posting/projection  ←  csharp:Rasm/Meshing/arrangement   # [WIRE]: BooleanOp union/difference/intersection the BooleanSolid carries
Posting/projection  ←  csharp:Rasm/Spatial/index         # [SHAPE]: SpatialIndex BVH occluder broad-phase prune
Posting/projection  ←  csharp:Rasm/Drawing/view          # [PROJECTION]: DrawingProjection exact QI visible/hidden segments — the ONE visibility solve
Posting/projection  →  csharp:Rasm.AppUi/Render          # [RECEIPT]: HiddenLineResult Viewport2D edge sets; supersedes the AppUi painter sort
Posting/projection  →  csharp:Rasm.AppUi/Render/drafting # [BOUNDARY]: HiddenLineSeam over the kernel DrawingProjection analytic HLR
Toolpath/slicing    ←  csharp:Rasm/Meshing/slice         # [WIRE]: Slicing.Apply SliceStack five-channel wire; in-folder planar section dies
Toolpath/skeleton   ⇄  csharp:Rasm/Meshing/offset        # [SHAPE]: ONE 2D/3D clearance family — Offsetting.Apply Medial + Clearance(probe) radius
Toolpath/skeleton   ←  csharp:Rasm/Meshing/skeleton      # [WIRE]: CurveSkeleton node/arc/radius SoA + Clearance(probe) — 3D half of clearance family
Process/physics     ←  csharp:Rasm.Materials/Properties  # [WIRE]: Conductivity/SpecificHeat/Density as raw doubles — AEC-peer boundary, NOT reference
Polygon/import      ←  csharp:Rasm.Bim/Exchange          # [SHAPE]: ACadSharp DWG/DXF read codec — Bim owns read surface, Fab consumes 2D ingress
Posting/program     →  csharp:Rasm.Persistence/Schema    # [WIRE]: CutProgram AST content-addressed durable-row projection
Nesting/nfp         →  csharp:Rasm.Persistence/Schema    # [WIRE]: Placement / Remnant XxHash128 content-keyed durable row
Nesting/nfp         ←  csharp:Rasm/Processing/flatten    # [PROJECTION]: ChartAtlas unrolled UV islands + DistortionReceipt as true-shape part input
Nesting/stock       →  csharp:Rasm.Compute               # [PROJECTION]: NestYield.WasteAreaMm2 feeds Compute AggregateCost/Environmental rollup
Toolpath/guard      ←  csharp:Rasm/Spatial/index         # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
*                   →  csharp:Rasm                       # [SHAPE]: Matrix / Point3d / Vector3d
```

## [03]-[PLANNED_DEPTH]

The settled author-kernels — the `Polygon/clipper` polygon algebra, the `Toolpath/skeleton` trochoidal fold over the kernel clearance family, the `Posting/program` cut-conditioning fold, the `Nesting/nfp` NFP feasibility, the `Toolpath/kinematics` `Robots`-cell serial-chain solver, and the `Posting/projection` kernel-`DrawingProjection` drafting emission — are correct for the milling instance but bake a single hidden process (subtractive CNC milling) into the feeds-and-speeds, the toolpaths, and the post words. The recovery is the `Process/family` axis pair: the `Process`/`Machine` discriminant (mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire and their machines) is the one fabrication axis the de-hardcode hangs on. From it, `Process/physics` widens to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive) plus the `(Material, Tool, Operation)` cutting-data table, `Toolpath/motion` factors to the `(RemovalModality, CutStrategy)` move family superseding the flat `(Process, ToolpathKind)` enum — a strategy lands once and the modality selects its envelope, the turning/thermal/additive rows collapsing onto the `boundary-pass`/`pocket-clear`/`radial-sweep`/`layer-walk` strategies a modality envelopes, with an `Admits` relation routing an inadmissible pair to `FabricationFault`, `Posting/program` widens to the `PostDialect` family over the dialect-neutral AST with the independent dialect-override seam resolving against the `Process`-bound default, `Nesting/nfp` widens its rectangle to the `Seq<Stock>` multi-sheet inventory, and `Nesting/workholding` rides the workholding kind as a `HoldingClass`-keyed `WorkholderKind` footprint-shape column on the concrete `Clamp` plus the multi-fixture-setup scheduler — each a case/row/column on a settled owner, never a parallel pipeline.

The domain-depth recovery adds professional-CAM sub-domains as new owners beside the settled kernels: `Toolpath/guard` is the swept tool-plus-holder collision/gouge guard with the collision-aware lift retract every feed move passes through, `Process/magazine` deepens the flat `Tool` enum into the physical carousel/turret/manual magazine with per-slot `ToolAssembly` holder geometry and the minimal-swap tool-change schedule, and the `Nesting/workholding` multi-fixture-setup scheduler partitions a job's operations across reorientation setups tracking the per-setup datum lineage — the safety, tool-management, and multi-setup floors the subtractive-biased folder lacked. `Toolpath/probing` (the in-process touch-probe metrology owner — G38 cycle vocabulary, work-offset/tool-length measurement, the measured-feature receipt closing the loop to corrected coordinates) is the queued sub-domain `IDEAS.md` carries: the `Process/faults`, `Process/magazine`, and `Posting/program` pages anchor its seams (the `ProbeCycle` G38/G10 rows, the measured `WorkOffset`) so the metrology floor lands as one design page against the existing anchors, never a parallel pipeline.

The two new sub-domains close the real fabrication-input and additive gaps the subtractive-biased folder lacks: `Toolpath/slicing` is the FFF/DED infill/deposition planner over the kernel `Slicing.Apply` slice stack and the one Geometry2D substrate (no managed slicer exists; the planar section is the kernel's), and `Polygon/import` is the portable DXF/DWG profile-ingress boundary through the pure-managed `ACadSharp` reader (host-neutral, coexisting with Rhino-native I/O, no RID burden). The `CSG_SILHOUETTE` watertight-solid outline is held in `IDEAS.md` as a compose-the-kernel-arrangement task against the C# branch `ROBUST_ARRANGEMENT_SUBSTRATE` managed exact path (the `Rasm/Meshing/arrangement#ARRANGEMENT` `Arrangement` `[Union]` owner — its DESIGN page is authored, so the compose-target anchor is settled), not a native-asset block; the per-facet `Posting/projection` kernel stays pure-managed until the realized C# arrangement owner lands.
