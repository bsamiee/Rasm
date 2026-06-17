# [FABRICATION_TASKLOG]

The open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — plus bullets naming the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations.

## [1]-[OPEN]

[ACTIVE] GEOMETRY2D_CLIPPER_SUBSTRATE — from [CLIPPER2_GEOMETRY2D]:
- Build `geometry2d/clipper#POLYGON_ALGEBRA`: the one `PolygonAlgebra` owner over Clipper2 with `Offset`/`Clip`/`MinkowskiSum`/`ClipOpenPath`, the `ClipOp`/`OffsetEnds` axes, and the `Loop`/`Edge3` boundary map.
- Integrate Clipper2 (`Clipper2Lib` — the `Clipper` facade `BooleanOp`/`InflatePaths`/`MinkowskiSum` and the `ClipperD` engine `AddOpenSubject`/`AddClip`/`Execute(ClipType, FillRule, PolyTreeD, PathsD openPaths)` for the open-path clip; `ClipType`/`FillRule`/`JoinType`/`EndType`/`PathD`/`PathsD`/`PointD`/`PolyTreeD`); register it in the language manifest as the folder's first admission; compose the kernel `Predicate.Orient2D` for the fold-back winding verdict.
- Boundaries: the one Clipper2 owner; the `ToPath`/`FromPaths` boundary map is the only place `double` crosses into the Clipper2 `PathD`/`PathsD` domain, and no `PathsD`/`Path64`/`PolyTreeD` type escapes the owner into a sibling kernel signature; the decimal `Precision.Digits` count is the one robustness knob the facade and engine read.
- Key consideration: every fold-back re-imposes the `Predicate.Orient2D` winding sign, Clipper2's inferred orientation is never the domain verdict; constrained triangulation is NOT a Clipper2 public surface (its `Delaunay` is an internal demo type), so the owner carries no `Triangulate` arm and a 2D-meshing need is a separate library admission.

[QUEUED] PROJECTION_CLIP_ONTO_CLIPPER2 — from [CLIPPER2_GEOMETRY2D]:
- Refit `projection/hidden-line#PROJECTION_HIDDEN_LINE` `ClipEdge` to the `geometry2d/clipper#POLYGON_ALGEBRA` `ClipOpenPath` open-path Boolean, dropping the hand-rolled `SpanInside`/`Subtract`/`Complement` parameter-interval subtraction.
- Integrate Clipper2 open-path Boolean via `geometry2d/clipper`; keep the kernel `SpatialIndex` BVH broad-phase and the author-kernel BSP front-to-back ordering and silhouette extraction.
- Boundaries: the screen clip rides the one Geometry2D owner; the BSP visibility ordering and the `Predicate.Orient2D` silhouette view-dot stay author-kernel internal to `projection`.
- Key consideration: the depth gate keeps only strictly-nearer occluders; the screen-to-world unprojection maps each clipped endpoint back by parameter along the projected segment.

[QUEUED] TROCHOIDAL_STRAIGHT_SKELETON — from [TROCHOIDAL_SKELETON]:
- Build `toolpath/skeleton#STRAIGHT_SKELETON`: the `StraightSkeleton` wavefront-propagation `MedialAxis`/`OffsetAt` over `SkeletonEvent` (edge-collapse/reflex-split) and `Wavefront`; add the `trochoidal` `ToolpathKind` row and the `Cam.Trochoidal` adaptive-clearing generator in `toolpath/motion#CAM_MOTION`.
- Integrate Clipper2 via `geometry2d/clipper#POLYGON_ALGEBRA` for the convex-inset offset cross-check; compose the kernel `Predicate.Orient2D` for the bisector/side verdict; the wavefront-event resolution itself is the genuine forward author-kernel (no managed library).
- Boundaries: internal to `toolpath`; the convex-inset offset cross-checks Geometry2D `Offset` and the skeleton owns only the reflex-split and medial-axis construction Clipper2 does not expose.
- Key consideration: the `SKELETON_EVENTS` RESEARCH item names the earliest-event scheduling, edge-collapse merge, and reflex-split — ground against the Aichholzer-Aurenhammer construction with the convex offset agreement as the correctness witness.

[QUEUED] KINEMATICS_SPLIT — from [TROCHOIDAL_SKELETON] (the CAM rebuild that lands `toolpath/skeleton` is the seam at which kinematics separates into its own domain owner):
- Build `kinematics/serial-chain#SERIAL_CHAIN` as its own owner: `DhJoint` DH forward kinematics over the `Rasm`/Vectors `Matrix` rail and the one damped-least-squares `Ik.Solve` Jacobian-pseudoinverse fold, split out of the old CAM page.
- Integrate `Rasm`/Vectors `Matrix`/`Dimension` (no `operator *`; `Multiply → Fin<Matrix>`, `Identity(Dimension)`); LanguageExt `Fin`.
- Boundaries: internal to `kinematics`; driven by the `toolpath/motion#CAM_MOTION` `Cam` fold which warm-starts each IK solve and reads the `IkPolicy.ReachStrict` flag to decide the reach contract; never a CAM-local kinematics re-mint and never a reach verdict inside the solver.
- Key consideration: `Ik.Solve` stays total — the damping λ keeps the `JJᵀ + λ²I` system SPD through singularities and a non-converged target stamps the residual and a false reached flag, never a thrown divergence; the `Unreachable` fault is the caller's (`Cam.Solve`) policy verdict, not the solver's.

[QUEUED] PORTABLE_CUT_PROGRAM — from [PORTABLE_POSTING]:
- Build `posting/program#CUT_PROGRAM`: the `CutProgram` RS-274/ISO-6983 `GWord` AST over the `GCommand`/`LeadStyle` axes, the `Posting.Post` fold from `Motion`/`Placement`, and the `Kerf`/`Lead`/`Tabs`/`Sequence` cut-conditioning.
- Integrate Clipper2 via `geometry2d/clipper#POLYGON_ALGEBRA` for kerf and lead offset; compose the `frontier/owner#FABRICATION_OWNER` `Move`/`Loop`/`PartTransform` atoms; route `FabricationFault.KerfCollision`.
- Boundaries: the one portable cut-program owner; the AST is dialect-neutral and `Emit` discriminates by dialect; the portable program coexists with the Rhino-native file I/O at the data contract.
- Key consideration: the `CUT_CONDITIONING` RESEARCH item names the kerf half-width offset, the lead-in/out arc, the micro-tab gaps, and the crash-safe inner-before-outer sequence — each over the settled Geometry2D substrate.

[BLOCKED] NFP_DRL_POLICY — from [DRL_NEST_POLICY]:
- Add a DRL-guided placement `NestPolicy` column in `nesting/nfp#NESTING` ranking placements over the existing NFP primitive; route the NFP construction through `geometry2d/clipper#POLYGON_ALGEBRA` `MinkowskiSum` first (the phase-1 dependency).
- Integrate Microsoft.ML.OnnxRuntime composed only via the `Rasm.Compute/models/sessions#INFERENCE_MODES` OrtValue run-mode fold, never referenced directly; the NFP and bottom-left/genetic folds stay unchanged.
- Boundaries: the DRL policy is one column on `NestPolicy` internal to `nesting`; the inference aligns to the `Rasm.Compute/models/sessions#INFERENCE_MODES` seam at the OrtValue contract and never mints a second inference surface or reaches into the Compute session interior.
- Key consideration: blocked on the `Rasm.Compute/models/sessions#INFERENCE_MODES` seam and a trained placement model; phase-1 ships bottom-left/genetic with the Geometry2D-routed NFP.

[BLOCKED] CSG_WATERTIGHT_SILHOUETTE — from [CSG_SILHOUETTE]:
- A watertight-solid silhouette arm on `projection/hidden-line#PROJECTION_HIDDEN_LINE` producing the exact outline of a boolean-combined solid rather than the per-facet silhouette.
- Integrate a stable managed CSG library (none today) or a per-RID native CSG asset under a deploy-asset gate.
- Boundaries: gated forward so the per-facet HLR kernel stays pure-managed; no in-folder CSG author-kernel.
- Key consideration: blocked on a branch-level CSG-library admission decision outside this folder's write-scope — no pure-managed CSG kernel trusted for robustness exists, and the native/GPL alternatives carry license plus per-RID deploy burden; until that admission lands the per-facet HLR kernel stays the only silhouette owner.

## [2]-[CLOSED]

[COMPLETE] FAULT_BAND_OWNERSHIP — resolved the band-ownership question:
- Resolved that `faults/faults#FAULT_BAND` `FabricationFault` (band 2500) mints ONLY the fabrication-specific cases (`NoFit`, `Unreachable`, `KerfCollision`, `OpenLoop`) and composes the kernel band-2400 `Rasm/Geometry` `GeometryFault` for the one shared failure the kernel union declares (`DegenerateInput`); the kernel union carries no `OpenLoop`, so a non-closed boundary is a fabrication contract that mints `FabricationFault.OpenLoop`, never a synthesized kernel case.
- A parallel band re-casing the shared geometry failure, or a `GeometryFault.OpenLoop` the kernel never declares, is the rejected form; `FabricationFault` mirrors the kernel `GeometryFault` shape (`Code`/`Message`/`ToError`) so both lower onto the one `Fin<FrontierResult>` rail through `.ToError()`, and `FabricationKeyPolicy` is the one ordinal comparer accessor the smart-enum owners reference.
