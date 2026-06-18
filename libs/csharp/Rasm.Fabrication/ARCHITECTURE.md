# [FABRICATION_ARCHITECTURE]

`Rasm.Fabrication` is the host-neutral portable-fabrication domain over the `Rasm` kernel: HLR/hidden-line projection, CAM toolpath plus serial-chain kinematics, and 2D true-shape nesting, with a portable cut-program emitter downstream and a shared 2D polygon-algebra floor over Clipper2. This page is the professional domain folder-map — the full sub-domain structure, each named by its real domain concept with a one-line charter, including planned sub-domains that hold no design page yet. A planned-but-empty sub-domain is a visible gap that fuels the ideas and tasks. Per-cluster owners and signature fences live on the `.planning/` pages; the cross-package dependency direction is stated once in the branch `ARCHITECTURE.md` and never restated here.

## [1]-[DOMAIN_MAP]

The sub-domain folders mirror the eventual source tree, one folder per professional domain concept. Each leaf design page is annotated with the owning `<sub-domain>/<page>#CLUSTER`; the `frontier` owner and the `geometry2d` substrate are read by every kernel, the `posting` emitter is the downstream consumer, and `faults` carries the fabrication fault rail.

```text codemap
Rasm.Fabrication/
├── frontier/                       # The polymorphic Fabrication owner: FrontierPolicy/FrontierResult unions, the shared Loop/Edge3/Move/PartTransform atoms, one Run generated total Switch
│   └── owner.md                    # frontier/owner#FABRICATION_OWNER
├── geometry2d/                     # The one 2D polygon-algebra substrate over Clipper2: offset/inflate, Boolean clip, Minkowski sum (NFP), open-path screen clip
│   └── clipper.md                  # geometry2d/clipper#POLYGON_ALGEBRA
├── projection/                     # HLR: BSP front-to-back visibility ordering + Clipper2 open-path-Boolean screen clip; visible/hidden/silhouette edge sets for AppUi Viewport2D
│   └── hidden-line.md              # projection/hidden-line#PROJECTION_HIDDEN_LINE
├── toolpath/                       # CAM motion: contour/pocket/drill/trochoidal toolpaths over Geometry2D offset, plus the straight-skeleton/medial-axis primitive driving adaptive clearing
│   ├── motion.md                   # toolpath/motion#CAM_MOTION
│   └── skeleton.md                 # toolpath/skeleton#STRAIGHT_SKELETON
├── kinematics/                     # Serial-chain kinematics: DH forward kinematics and the one damped-least-squares Jacobian-pseudoinverse IK solver through singularities
│   └── serial-chain.md             # kinematics/serial-chain#SERIAL_CHAIN
├── nesting/                        # 2D true-shape nesting: NFP feasibility (Minkowski via Geometry2D) plus bottom-left/genetic placement fold
│   └── nfp.md                      # nesting/nfp#NESTING
├── posting/                        # Host-neutral portable cut-program emission: RS-274/ISO-6983 G-code model plus kerf-comp, lead-in/out, micro-tab/bridge, cut-sequencing over Geometry2D offset
│   └── program.md                  # posting/program#CUT_PROGRAM
└── faults/                         # FabricationFault band-2500 for fabrication-specific cases (NoFit, unreachable-IK, kerf-collision, open-loop) composing the kernel band-2400 GeometryFault
    └── faults.md                   # faults/faults#FAULT_BAND
```

## [2]-[PLANNED_DEPTH]

Every sub-domain carries a design page; the depth gaps are forward arms on the existing owners rather than missing folders. The `toolpath/skeleton` wavefront-event resolution and the `posting/program` cut-conditioning fold carry RESEARCH items that name their author-kernel depth; the `geometry2d/clipper` irregular/non-convex NFP arm, the `toolpath/motion` 5-axis tilt column, and the `nesting/nfp` DRL-guided placement column are growth arms on settled owners. The `CSG_SILHOUETTE` watertight-solid outline is a forward native-asset-gated concept held in `IDEAS.md`, outside the in-folder author-kernel scope, so the per-facet `projection/hidden-line` kernel stays pure-managed.
