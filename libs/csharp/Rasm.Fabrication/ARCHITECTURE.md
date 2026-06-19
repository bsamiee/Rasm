# [FABRICATION_ARCHITECTURE]

`Rasm.Fabrication` is the host-neutral portable-fabrication domain over the `Rasm` kernel: HLR/hidden-line projection, CAM toolpath plus serial-chain kinematics, and 2D true-shape nesting, with a portable cut-program emitter downstream and a shared 2D polygon-algebra floor over Clipper2. This page is the professional domain folder-map — the full sub-domain structure, each named by its real domain concept with a one-line charter, including planned sub-domains that hold no design page yet. A planned-but-empty sub-domain is a visible gap that fuels the ideas and tasks. Per-cluster owners and signature fences live on the `.planning/` pages; the cross-package dependency direction is stated once in the branch `ARCHITECTURE.md` and never restated here.

## [1]-[DOMAIN_MAP]

The sub-domain folders mirror the eventual source tree, one folder per professional domain concept. Each leaf design page is annotated with the owning `<sub-domain>/<page>#CLUSTER`; the `frontier` owner and the `geometry2d` substrate are read by every kernel, the `posting` emitter is the downstream consumer, and `faults` carries the fabrication fault rail.

```text codemap
Rasm.Fabrication/
├── frontier/                       # The polymorphic Fabrication owner: FrontierPolicy/FrontierResult unions, the shared Loop/Edge3/Move/PartTransform atoms, one Run generated total Switch
│   └── owner.md                    # frontier/owner#FABRICATION_OWNER
├── process/                        # The Process/Machine axis pair: removal-modality (subtractive/thermal/abrasive/additive/erosion), kinematic class, holding class, and post-dialect default — the one fabrication discriminant cut-parameter/motion/posting read
│   └── family.md                   # process/family#PROCESS_FAMILY
├── geometry2d/                     # The 2D polygon-algebra substrate over Clipper2 plus the portable profile-ingress boundary
│   ├── clipper.md                  # geometry2d/clipper#POLYGON_ALGEBRA       — offset/inflate, Boolean clip, Minkowski sum (NFP), open-path screen clip
│   └── profile-import.md           # geometry2d/profile-import#PROFILE_IMPORT  — DXF/DWG closed-polyline/arc entities tessellated into Loop through ACadSharp, host-neutral, coexisting with Rhino-native I/O
├── projection/                     # HLR: BSP front-to-back visibility ordering + Clipper2 open-path-Boolean screen clip; visible/hidden/silhouette edge sets for AppUi Viewport2D
│   └── hidden-line.md              # projection/hidden-line#PROJECTION_HIDDEN_LINE
├── toolpath/                       # CAM motion: the (Process, ToolpathKind) move family (mill contour/pocket/drill/trochoidal, lathe turn/face/groove/thread, thermal contour, additive slice-layer) over Geometry2D offset, plus the straight-skeleton primitive
│   ├── motion.md                   # toolpath/motion#CAM_MOTION
│   └── skeleton.md                 # toolpath/skeleton#STRAIGHT_SKELETON
├── additive/                       # FFF/DED slicing author-kernel: planar-section layer contours, perimeter shells (Geometry2D Offset), and hatch-clip infill (Geometry2D Clip) over the InfillPattern axis
│   └── slicing.md                  # additive/slicing#SLICING
├── kinematics/                     # Serial-chain kinematics: DH forward kinematics and the one damped-least-squares Jacobian-pseudoinverse IK solver through singularities
│   └── serial-chain.md             # kinematics/serial-chain#SERIAL_CHAIN
├── nesting/                        # 2D true-shape nesting: NFP feasibility (Minkowski via Geometry2D), bottom-left/genetic placement fold, plus the Stock union (sheet/plate/bar/tube/billet/filament/remnant) content-keyed feasibility set
│   └── nfp.md                      # nesting/nfp#NESTING
├── posting/                        # Host-neutral portable cut-program emission: the dialect-neutral G-code AST plus the PostDialect family (linuxcnc/grbl/fanuc/marlin/hypertherm), kerf-comp, lead-in/out, pierce, micro-tab/bridge, cut-sequencing over Geometry2D offset
│   └── program.md                  # posting/program#CUT_PROGRAM
├── fixturing/                      # Workholding keep-out: the Workholder family (clamp/vise/chuck/vacuum-table/magnet/sacrificial-bed) conditioning the toolpath and the cut sequence against fixture geometry over the Geometry2D offset/clip substrate
│   └── workholding.md              # fixturing/workholding#WORKHOLDING
├── process-physics/                # Removal-physics policy: the process × material × tool × operation table projecting to the modality-discriminated RemovalBudget (subtractive/thermal/abrasive/additive) the toolpath generators read
│   └── cut-parameter.md            # process-physics/cut-parameter#CUT_PARAMETER
└── faults/                         # FabricationFault band-2500 for fabrication-specific cases (NoFit, unreachable-IK, kerf-collision, open-loop) composing the kernel band-2400 GeometryFault
    └── faults.md                   # faults/faults#FAULT_BAND
```

## [2]-[PLANNED_DEPTH]

The settled author-kernels — the `geometry2d/clipper` polygon algebra, the `toolpath/skeleton` wavefront-event resolution, the `posting/program` cut-conditioning fold, the `nesting/nfp` NFP feasibility, the `kinematics/serial-chain` DH/IK solver, and the `projection/hidden-line` BSP visibility — are correct for the milling instance but bake a single hidden process (subtractive CNC milling) into the feeds-and-speeds, the toolpaths, and the post words. The recovery is the `process/family` axis pair: the `Process`/`Machine` discriminant (mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire and their machines) is the one fabrication axis the de-hardcode hangs on. From it, `process-physics/cut-parameter` widens to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive), `toolpath/motion` widens to the `(Process, ToolpathKind)` move family (lathe turning, thermal contour, additive slice-layer beside the milling four), `posting/program` widens to the `PostDialect` family over the dialect-neutral AST, `nesting/nfp` widens its rectangle to the `Stock` union, and `fixturing/workholding` rides the workholding kind as a `HoldingClass`-keyed `WorkholderKind` footprint-shape column on the concrete `Clamp` — each a case/row/column on a settled owner, never a parallel pipeline.

The two new sub-domains close the real fabrication-input and additive frontiers the subtractive-biased folder lacks: `additive/slicing` is the FFF/DED planar-section-and-infill author-kernel over the one Geometry2D substrate (no managed slicer exists, the author-kernel posture is the `STRAIGHT_SKELETON` precedent), and `geometry2d/profile-import` is the portable DXF/DWG profile-ingress boundary through the pure-managed `ACadSharp` reader (host-neutral, coexisting with Rhino-native I/O, no RID burden). The `CSG_SILHOUETTE` watertight-solid outline is held in `IDEAS.md` as a compose-the-kernel-arrangement task against the C# branch `ROBUST_ARRANGEMENT_SUBSTRATE` managed exact path (the `Rasm.Geometry/arrangement/arrangement#ARRANGEMENT` `Arrangement` `[Union]` owner — its DESIGN page is authored, so the compose-target anchor is settled), not a native-asset block; the per-facet `projection/hidden-line` kernel stays pure-managed until the realized C# arrangement owner lands.
