# [DOSSIER_CENSUS] — Rasm.Fabrication/.planning

Lane: census (READ-ONLY). Scope: whole `libs/csharp/Rasm.Fabrication/.planning/` tree (17 pages, 5 folders) + package governance (`ARCHITECTURE.md`/`README.md`/`Rasm.Fabrication.csproj`/`IDEAS.md`/`TASKLOG.md`/`.api/`) diffed against `libs/.planning/architecture.md` (strata) + the six upstream briefs. Anchors are `.planning/`-relative unless prefixed `ARCH:`(package ARCHITECTURE.md) / `README:` / `CSPROJ:` / `TASKLOG:` / `GBRIEF:`(RASM-CS-GEOMETRY-BRIEF.md) / `PBRIEF:`(RASM-CS-PERSISTENCE-BRIEF.md) / `CBRIEF:`(RASM-CS-COMPUTE-BRIEF.md) / `UIBRIEF:`(RASM-CS-APPUI-BRIEF.md).

## [00]-[TELOS_AS_IT_SHOULD_BE]

`Rasm.Fabrication` is the AEC-DOMAIN host-neutral production digital-fabrication owner over `{Rasm, Rasm.Element}`: the polymorphic `Fabrication.Run` closing 3D→program across full multi-axis CNC/CAM toolpath depth, production-breadth post-processing (controller families incl. conversational dialects, canned cycles, macro programming), production-grade 3D printing (FFF/DED + resin/powder over PicoGK implicit/lattice/support), 2D true-shape nesting + rectangular cutting-stock yield, and production specs (tolerance/finish/process-capability/setup-sheet/traveler) — **consuming** the kernel toolpath plane (corner-strategy offset, slice-stack, medial-with-clearance-radius, developables, manufacturing-tolerance booleans, DrawingProjection) at the seam per GBRIEF `[V3]`/`[V10]`, never re-implementing it, and sharpening the demand rows where production scope exceeds the kernel's `[V10]` floor. The current corpus is **world-class in its dense interiors and structurally naive as a production system**: five kernel-shaped author-kernels the geometry campaign is landing kernel-side still live here as owners; four admitted packages have zero page consumers; three safety/tooling/fixturing owners are built but unwired into the execution fold; the process-breadth axes (dialect, strategy, material-modality) are thin slices; and the 5-folder map hides the ~12-namespace true decomposition.

## [01]-[PER_PAGE_VERDICTS]

Verdict 1-10 (production-grade bar); defects with file:line; split/merge/move pressure; owner charter as it SHOULD be.

### Process/owner.md — 8/10
- Polymorphic `Fabrication` owner: `FabricationPolicy`[Union](HiddenLine/Cam/Nest) → one `Run` generated total `Switch` → `FabricationResult`[Union]. `FabricationInput` is a genuinely settled 10-field record absorbing every growth (process/machine axis, inventory, plan, dialect, keepouts, cell) as policy DATA — award-grade collapse (owner.md:68-78, 100-106).
- **DEFECT (contract gap):** `Loop` is declared 2-arg `Loop(Arr<Point3d> Vertices, bool Closed)` with no bulge (owner.md:42) and no `BulgeAt`/3-arg ctor, but `clipper.md` (the sibling that widens it) calls `ccw.BulgeAt(i)` and `new Loop(verts, closed, bulges)` (clipper.md:257,271) and its prose asserts "The canonical Loop is widened with a parallel `Arr<double> Bulges` column" (clipper.md:5). The atom OWNER never landed the widening its consumer depends on → compile-broken across the arc rail.
- **CHARTER:** unchanged, but `Loop` must carry the `Bulges` arc column (0 straight / tan(θ/4) per arc) as the one shape both PolygonAlgebra and ArcAlgebra read; `owner.md` owns it.

### Process/family.md — 8/10
- The one de-hardcode axis: `Process`/`Machine`/`RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect`/`CutStrategy` as `[SmartEnum<string>]` rows with constructor-bound behavior columns + `Admits` set-relations. APPROACH-correct (rows are seed DATA on closed families, generated `Switch`) — not a hardcoding-vs-generator defect.
- **DEFECT (coverage, mandate):** `PostDialect.Admits` models a binary `Rs274 ? modality != Additive : modality == Additive` (family.md:94-95) — cannot express conversational non-RS-274 controllers the production mandate names (Heidenhain Klartext, Mazak Mazatrol, Okuma OSP). The 8 dialect rows (linuxcnc/grbl/fanuc/haas/marlin/reprap/hypertherm/mazak) are a thin slice; Siemens 840D/Sinumerik, Heidenhain, Fagor, Centroid absent (family.md:80-96).
- **DEFECT (coverage, mandate):** `CutStrategy` is 8 essentially-2.5D rows (family.md:34-43) — no 3D-surface finishing (parallel/scallop/waterline/pencil/flowline), no 5-axis (swarf/flank/morph), no rest-machining/rest-rough. The production "multi-axis strategies" + "rest machining" + "tool-engagement" mandate is uncovered at the axis level; a strategy-dimensionality column (2.5D/3D/5-axis) is absent.
- **CHARTER:** the dialect axis gains a block-format/canned-cycle/arc-mode/conversational capability column (not a Comment/LineNumbers-only render); CutStrategy gains the 3D-surface and multi-axis rows + a dimensionality column feeding an axis-count admission.

### Process/faults.md — 9/10
- Landed law: `FabricationFault`[Union] 10 arms on `FaultBand.Fabrication = 2700`, offsets +1..+10, Nest at +10 (faults.md:50-61); composes kernel band-2400 `GeometryFault`; no comparer accessor; the 2501-2509→2700 reband rationale is correctly recorded (faults.md:3). This page is the reference — build ON it, never re-band.
- Clean. (All stale 25xx references live in OTHER files — see [02] R6.)

### Process/physics.md — 6/10
- **DEFECT (APPROACH-naivety, strong):** `Material` carries a SINGLE `ModalityPhysics Physics` (physics.md:99), so one physical material fragments into modality-keyed rows: `mild-steel`(Subtractive) vs `mild-steel-thermal`(Thermal); `stainless` vs `stainless-abrasive` (physics.md:91,95,96). To budget a waterjet cut of stainless you need a DIFFERENT `Material` identity than to mill it. This VIOLATES the one-canonical-name-per-concept law (CLAUDE `[01]`) and CONTRADICTS the page's own prose (physics.md:3 "materials own real specific vocabularies without a flat wide record where every modality's columns sit nullable"). The fix chosen (split the material) is worse than the flat record it rejects. Correct form: `Material` carries a per-modality payload map (`Map<RemovalModality, ModalityPhysics>`), `Budget` reads the modality-matched payload — one `stainless` identity carrying subtractive SFM AND abrasive jet AND thermal kerf.
- **DEFECT (dead carrier):** `Overrides` frozen table is `FrozenDictionary.Empty` (physics.md:114-115) yet consulted first every call (physics.md:128) — a permanent no-op seed; either seed it or state the growth-seam honestly.
- **DEFECT (hardcode residue):** the "defeated" `90.0` constant survives as the no-cell fallback (physics.md:141) and re-appears inline (physics.md:91 `mild-steel` SurfaceSpeed 90.0); CuttingData is 6 seed rows (physics.md:119-124) — acceptable as seed, but the feeds/speeds gap is real (README:58 records "no free-full feeds/speeds dataset on NuGet").
- **CHARTER:** one `Material` identity per physical material carrying a per-modality physics map; `Budget` modality-dispatches into the map; the SFM/chip-load `CuttingData` and `Overrides` tables ride beside it (correct today).

### Process/magazine.md — 8/10
- Deep MTConnect.NET-Common integration: `ToolAssembly`[ComplexValueObject] composing `ICuttingToolAsset`/ISO-13399 `Measurements`/`ToolLife`/`CutterStatus`, `Schedule` minimal-swap fold with tool-life split, `HolderEnvelope` shared with guard/workholding, `CuttingToolAsset.GenerateHash` reconciled with `XxHash128` (magazine.md:15-20). Award-grade mining of an admitted package.
- **DEFECT (unwired seam):** magazine.md:18,22 asserts "the `Posting/program#CUT_PROGRAM` `Post` emits each `ToolChange` as the `G43`/`M6`/`Tnn` block" but `program.md`'s `Post` fold (program.md:141-164) never consumes a `Magazine.Schedule` output — the `ToolChange` GCommand rows exist (program.md:61-62) with no producer wired in. Built owner, orphaned from the pipeline.
- Namespace `Rasm.Fabrication.ProcessModel` (magazine.md:39), shared with family.md — see [03] folder verdict.
- **CHARTER:** correct; must be wired — `Fabrication.Run`→`Cam`/`Post` threads `Magazine.Schedule` so `Post` emits the real swap blocks.

### Polygon/clipper.md — 9/10
- Excellent dual-owner split: `PolygonAlgebra` (Clipper2 line-space: uniform+variable-delta offset via `ClipperOffset.DeltaCallback64`, Boolean, Minkowski sum/diff, open-path clip, `SimplifyMode` behavior-column) + `ArcAlgebra` (CavalierContours arc-space: bulge-native offset/Boolean/`Shape.ParallelOffset` islands, arc-length sampler, `StaticAABB2DIndex`). Correctly retires the post-hoc biarc refit for bulge paths; correctly excludes the buggy Clipper2 `Triangulate` (clipper.md:25); precision-bearing `Minkowski.Diff` (clipper.md:124-125). This is the folder's strongest substrate page.
- **DEFECT:** depends on `Loop.Bulges`/`BulgeAt`/3-arg ctor the atom owner (owner.md:42) never declared — see owner.md.
- **CROSS-CAMPAIGN MIGRATION (see [04]):** `ArcAlgebra`'s CavalierContours ownership is contested — GBRIEF `[V10]a`+`[PLACEMENT_LAW]` rule CavalierContours sites at ONE stratum (kernel offset owner OR Fabrication), "never CavalierContours in two manifests"; the geometry DECISION may re-home the arc-offset assembly kernel-side with Fabrication consuming the seam.
- **CHARTER:** the LINE+ARC 2D polygon-algebra substrate; if the geometry DECISION sites the exact-medial-composing offset kernel-side, `ArcAlgebra` re-scopes to pure-kerf arc compensation OR consumes the kernel `OffsetOp` arc seam.

### Polygon/import.md — 9/10
- Excellent ACadSharp read boundary: total `Admit` switch over `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Spline`/`Insert`, native `Spline.TryPolygonalVertexes` tessellation, `Insert.Explode()` recursive block flatten with baked transform, `Failsafe=true` notification-sink resilience, `Arc.CreateFromBulge` (import.md:14,16, 92-111). Every member ratified against `.api/api-acadsharp.md`.
- **DEFECT (stale, cross-campaign — census point 3):** import.md:20 justifies read-only via "`netDxf` (present in the central manifest as an `Rasm.AppUi` DXF-write dependency)" — but UIBRIEF `[V7]` REMOVES netDxf repo-wide and makes AppUi's WRITE leg ACadSharp two-format (DXF+DWG) (UIBRIEF:127,146,170). The netDxf-as-AppUi-write rationale is dead.
- **MOVE PRESSURE:** import.md is CAD profile INGRESS, not "polygon algebra" — its home under `Polygon/` is a concern-mix. Belongs in an `Ingress/` folder beside the unwired OcctNet.Wrapper (STEP/IGES) and DSTV.Net (NC1) ingress owners (grows the folder past one file).
- **CHARTER:** the one 2D DXF/DWG profile-ingress owner; re-home to `Ingress/`; drop the netDxf framing (netDxf is gone; AppUi write is ACadSharp; the read/write split is intra-ACadSharp).

### Toolpath/motion.md — 5/10
- The `(RemovalModality, CutStrategy)` cross-product dispatch skeleton is correct and the factoring that retires the flat `ToolpathKind` is genuinely good (motion.md:64-74); `Contour`/`Pocket` route PolygonAlgebra offset, `Adaptive` reads the skeleton clearance field.
- **DEFECT (illusory depth, strong):** 5 of 8 generator arms are placeholder-grade beneath the complete dispatch: `Turn` is a naive `v.Y - radius - k*stepOver` Y-offset (motion.md:76-79) — NOT a lathe radial Z-vs-radius sweep the prose claims "over the `BarStock` revolved envelope" (motion.md:14,16); `helical` ALIASES `Turn` (motion.md:71,73 both `Turn`) so the distinction is unrealized; `Plunge`/`Peck` are 2-move centroid stubs with no real peck/dwell cycle (motion.md:81-84,105-108); `SliceWalk` maps layer vertices to moves with no infill walk (motion.md:86-87). `BarStock` is a phantom — named in prose, absent from the fence.
- **DEFECT (unwired seam):** `guard.md:3,16` asserts `Cam.Solve` consults `Guard.Check` "before committing each feed move" — but `Cam.Solve` (motion.md:46-62) never calls `Guard.Check`; the collision floor is not in the motion loop. Same for `Workholding.Condition`.
- **CROSS-CAMPAIGN MIGRATION:** `Adaptive` reads `StraightSkeleton.MedialAxis`/`ClearanceAt` (motion.md:111,130) which migrates to the kernel medial-with-clearance-radius seam per GBRIEF `[V10]c`; `Contour`/`Pocket` offset migrates to the kernel corner-strategy `OffsetOp` per `[V10]a` if kernel-sited; 5-axis simultaneous milling is growth-only (motion.md:19).
- **CHARTER:** the production CAM-motion owner consuming the kernel offset/medial plane; every strategy arm a real operation (lathe radial sweep in ZX, distinct helical thread, real peck/dwell canned-cycle geometry, real infill walk); guard + workholding + magazine wired into the fold.

### Toolpath/kinematics.md — 7/10
- Strong `Robots`(visose) cell owner: `Program` look-ahead compile, `RobotConfigurations` branch, `Program.Errors`→`Unreachable` fold, the one `extern alias R3` RhinoCommon↔Rhino3dm pose boundary, `Collision` correctly firewalled (throws on the Rhino3dm build) (kinematics.md:9,20,125). Verified-member discipline is exemplary.
- **DEFECT (stale 25xx — census point 4, R6):** kinematics.md:3 "folding the planner's reach/limit/singularity diagnostics into the typed **band-2500** `FabricationFault`" — faults.md landed 2700. Persisted-boundary hazard per PBRIEF `[V12]`.
- **DEFECT (page-craft):** a `[03]-[RESEARCH]` tail (kinematics.md:122-125) carries research-origin framing the durable-doc law forbids — inconsistent with the 8 non-author-kernel pages that carry none.
- **DEFECT (coverage):** shortest Toolpath page (103 LOC); owns articulated-arm IK only — 5-axis simultaneous GANTRY milling (tilt/rotary head, not a robot cell) has no owner here or in motion.
- **CHARTER:** the robot-cell + multi-axis-machine kinematics owner; its own folder (`Kinematics/`) grown past one file by the 5-axis-gantry demand; research tail folded into design prose.

### Toolpath/skeleton.md — 8/10 (as author-kernel) / MIGRATION CASUALTY
- Genuinely-new author-kernel: wavefront-propagation straight skeleton (`SkeletonEvent`[Union] Edge/Split, unit-speed bisector march, `1/sin(θ/2)` scale) with the `ClearanceAt` inscribed-radius field the trochoidal engagement + guard reach read (skeleton.md:16,202). Correct exact-predicate floor.
- **CROSS-CAMPAIGN MIGRATION (strong, census point 2):** GBRIEF `[V10]c-e` lands medial-with-clearance-radius KERNEL-side and rules "Fabrication's `Toolpath/Skeleton.cs` dies for `Offsetting.Apply`" IF the kernel medial carries the radius — and skeleton.md's `ClearanceAt` IS exactly the "wavefront clearance-radius field" the kernel brief names (GBRIEF:105, ARCH:26 counterpart). Settled end-state: skeleton.md becomes a consumer of the kernel medial seam.
- **DEFECT (page-craft):** `[03]-[RESEARCH]` tail (skeleton.md:199-202).
- **CHARTER:** post-migration, the consumer of the kernel medial-with-clearance-radius seam; the demand-sharpening owner where production CAM needs 3D medial (multi-axis rest machining) the kernel `[V10]d` floor doesn't yet carry.

### Toolpath/slicing.md — 6/10 / MIGRATION CASUALTY + INTEGRATION MANDATE
- Planar FFF/DED author-kernel: plane-vs-triangle `Section` over exact `Orient3D`, `Chain` contour assembly, inward `Offset` shells, `InfillPattern`[SmartEnum] rectilinear/concentric/honeycomb/grid over one `ClipOpenPath` (slicing.md:16,144-150). Clean dispatch shape.
- **DEFECT (integration mandate, strong):** does NOT consume the admitted `PicoGK` (csproj:20, README `[IMPLICIT_VOXEL]`) — the OSS implicit/SDF/voxel owner for exactly the mandate's resin/powder + lattice/TPMS-infill + support scaffolds; slicing.md:19 records gyroid/support as HAND-ROLLED growth, and its prose concedes "the resin/powder path the planar-only FFF `Section` never reaches" (README:55). PicoGK has ZERO page consumers folder-wide.
- **DEFECT (coverage, mandate):** no build-orientation optimization, no support generation, no lattice/TPMS infill (4 basic hand-rolled patterns), no machine profiles — the production-3DP mandate lanes are absent.
- **CROSS-CAMPAIGN MIGRATION:** `Section` migrates to the kernel slice-stack owner per GBRIEF `[V10]b` (slicing.md:221 already promises the re-route; oriented CCW/CW contours the arc-offset demands land kernel-side).
- **MOVE PRESSURE:** namespace `Rasm.Fabrication.Additive` (slicing.md:35) inside a `Toolpath/` folder — additive is its own production lane, not a toolpath sibling.
- **CHARTER:** an `Additive/` folder: `slicing` consuming the kernel slice-stack; a `lattice`/`support`/`resin` owner over PicoGK (implicit infill, overhang-conditioned supports, SLA/DLP/MSLA `GetVoxelSlice`/`CliIo` grayscale stacks); an `orientation` owner.

### Toolpath/guard.md — 8/10
- Strong swept tool-plus-holder safety floor: `SweptVolume` (Minkowski disc-along-segment + holder ring), `Verdict`[Union] Clear/Gouge/Collision/Clearance, SpatialIndex BVH broad-phase, `ClearanceAt` reach, collision-aware safe-Z `Lift` (guard.md:13-16). Part-keep vs stock-keep-out held distinct so the cause is typed.
- **DEFECT (unwired seam, strong):** guard.md:3 asserts "a closed `Verdict` `Cam.Solve` consults before committing each feed move" — but motion.md's `Cam.Solve` never invokes `Guard.Check` (motion.md:46-62). The mandatory safety floor is a realized island.
- **CROSS-CAMPAIGN MIGRATION:** `Sweep` composes the same Minkowski owner that migrates to the kernel offset per `[V10]a`; `ClearanceAt` migrates to the kernel medial seam per `[V10]c`.
- **CHARTER:** correct; must be wired into the motion per-move fold.

### Nesting/nfp.md — 9/10
- Dense true-shape nesting: NFP via Minkowski sum, IFP via `MinkowskiDiff` dual, `Stock`[Union] 7 cases with one `Contains`/`Area`/`Planar` fold, multi-sheet spill scheduler, kerf-inflated Boolean-difference `Remnant` lineage with `XxHash128` content-key + `Parent`, `RectpackSharp` AABB fast-path, GA, `PairKey` precompute memo, injected DRL `Score` delegate correctly avoiding the AEC→app-platform Compute edge, `Nest.Honor` consuming the sibling `NestPlan` (nfp.md:16,206-217). Award-grade.
- **DEFECT (seam target stale — census point 4):** the `Nesting/nfp → Rasm.Persistence/Schema` durable-row seam (ARCH:53) targets the DELETED Persistence `Schema/` folder; re-points to blobstore content rows + cache artifact index per PBRIEF `[V12]` `[FABRICATION_PROGRAM_DURABLE_ROWS]`. `Placement` is not content-keyed (only `Remnant` is).
- **CHARTER:** correct; the Persistence seam re-anchors to the post-Persistence-campaign owners.

### Nesting/stock.md — 9/10
- Excellent cutting-stock yield engine: `NestStrategy`[Union] collapsing the five `RectangleBinPack` packers + heuristic-sweep best-of, per-sheet `SheetYield` ledger, `[EXPRESSION_SPINE]` kernel exemption honestly scoped, int-domain admission boundary, `Nest.Honor` seam (stock.md:50-67,233-240). The CLEANEST host-neutral page — no `Rhino.Geometry` import, scalar `NestPlacement` (stock.md:99).
- **DEFECT (wired-undeclared seam):** stock.md:40,83,99 imports `Rasm.Element` `MaterialId` (`CutPart.Material`, `NestPlacement.Material`) — a real cross-package seam ABSENT from `ARCH:[02]-[SEAMS]` (the only Element row is the future `* → Element/Projection`). Both-directions ledger gap.
- **DEFECT (forward seam):** the `Nesting/stock → Rasm.Compute` `NestYield.WasteAreaMm2` rollup (ARCH:50, stock.md:3) has no Compute-side named row (census point 5).
- **CHARTER:** correct; `ARCH:[02]` gains the `Nesting/stock ← Rasm.Element/MaterialId` row and the Compute counterpart is named.

### Nesting/workholding.md — 7/10
- Good fixture keep-out model: `Clamp`/`ExclusionZone`/`Fixture`, `WorkholderKind`[SmartEnum] footprint-shape column keyed off `Machine.HoldingClass`, exclusion offset over the one Clipper owner, `Loop.Covers` exact containment, composing the posting `Sequence` (workholding.md:13-16).
- **DEFECT (illusory depth):** prose claims kind-specific footprint SHAPES (two-jaw vise, revolved chuck, full-bed vacuum — workholding.md:3,14) but the fence differs kinds only by a `MarginScale` scalar (workholding.md:39-44); the distinct footprint shapes are unmodeled — a prose-vs-fence gap.
- **DEFECT (logic):** `WorkholderKind.ForHolding` returns the FIRST `Holding`-match (workholding.md:49-50); `Clamp` and `Vise` both bind `HoldingClass.Mechanical` (workholding.md:39-40), so `ForHolding(Mechanical)` always returns `Clamp` — `Vise` is unreachable via the mapping.
- **DEFECT (unwired seam):** `Condition` composes posting `Sequence` and the prose says it conditions the toolpath — but no page's fold calls `Workholding.Condition`; the multi-fixture-setup scheduler is IDEAS-queued (`[MULTI_FIXTURE_SCHEDULE]-[QUEUED]`), single-fixture today.
- **MOVE PRESSURE:** namespace `Rasm.Fabrication.Fixturing` (workholding.md:34) under a `Nesting/` folder — fixturing ≠ nesting; the ARCH codemap even bundles them by "layout" prose (ARCH:27).
- **CHARTER:** a `Fixturing/` folder (workholding + the queued multi-fixture-setup scheduler); real per-kind footprint geometry; wired into the motion/posting conditioning.

### Posting/program.md — 9/10
- Excellent dialect-neutral G-code AST: `GCommand`[SmartEnum] 21 rows, `GWord` word-addressed block, biarc refit over `geometry3Sharp` `g3.BiArcFit2`, jerk-limited S-curve `Lookahead` author-kernel, curvature/edge-adaptive `Feedrate`, cantilever-beam deflection `Compensate`, crash-safe `Sequence`, kerf/lead/tab conditioning, `PostDialect` override resolution (program.md:13-20,247-322). Dense, correct dimensional discipline.
- **DEFECT (integration mandate, strong):** DSTV.Net (csproj:16, README `[STEEL_FABRICATION_EXCHANGE]`) is admitted with ZERO page consumers — README:36 intends it "beside the neutral G-code AST" but program.md never emits DSTV/NC1 saw/drill/punch programs.
- **DEFECT (coverage, mandate):** `CutProgram.Emit` varies dialects only by `Dialect.LineNumbers`/`Comment` (program.md:122-136) — a Fanuc post and a Haas post emit near-identically; NO canned cycles (G81/G83/G84 emitted as expanded moves, not single blocks), NO macro programming (Fanuc macro B / variables / subprograms), NO conversational output. The "dialect breadth + canned cycles + macro programming" mandate is unrealized at the emit layer; `PostDialect` needs a block-format/canned-cycle/arc-mode capability column.
- **DEFECT (seam gap):** the `Posting/program → Persistence` "CutProgram AST content-addressed durable-row" seam (ARCH:52) has no content-key producer — program.md "computes no hash" (program.md:3), so nothing content-addresses the `CutProgram` (contrast nfp's `Remnant.Of`). Stale target `Schema/` (deleted) → blobstore/cache per PBRIEF `[V12]`.
- **DEFECT (unwired seam):** magazine `ToolChange`/`G43`/`M6` not consumed (see magazine.md).
- **MOVE PRESSURE:** shares `Posting/` with projection.md — two unrelated downstream concerns (cut-program emission vs HLR drafting).
- **CHARTER:** the production cut-program owner: G-code AST + a real per-dialect emit (canned cycles, macro, conversational, block format) + the DSTV/NC1 steel lane as a sibling emit target on one `Program.Post` fold; `CutProgram` content-keyed via `XxHash128` feeding the re-anchored Persistence blobstore seam.

### Posting/projection.md — 8/10 (as author-kernel) / MIGRATION CASUALTY
- Strong BSP HLR: front-to-back `BspNode` visibility, silhouette via exact incident-facet view-dot, `ClipOpenPath` screen clip, `BooleanSolid` watertight arm composing the kernel `Arrangement.Apply`/`ToMesh` (projection.md:16,113-117). Correctly composes SpatialIndex/Numerics/Arrangement.
- **CROSS-CAMPAIGN MIGRATION (strong, census point 3-adjacent):** GBRIEF `[V3]` rebuilds the kernel `Drawing/view` DrawingProjection and rules "Fabrication's `Posting/Projection.cs` BSP HLR dies … the kernel `DrawingProjection` supersedes BOTH, Fabrication consumes" — and UIBRIEF:93,193 confirms AppUi consumes the kernel projection at the declared seam. The BSP author-kernel here is the migration casualty; projection.md becomes a consumer of the kernel DrawingProjection.
- **DEFECT (assumption invalidated):** `using Rasm.Geometry.Healing` for `BooleanOp` (projection.md:30,40) — GBRIEF `[V5]` re-homes `BooleanOp` from Processing/Healing to `arrangement`/the shared floor; the seam anchor (ARCH:44 `← Rasm/Geometry/Healing`) re-points.
- **MOVE PRESSURE:** namespace `Rasm.Fabrication.Projection` (projection.md:37) mis-filed under `Posting/` — HLR drafting is not cut-program emission; belongs in a `Drafting/` folder.
- **CHARTER:** post-migration, the consumer of the kernel DrawingProjection producing world-space edge sets for AppUi `Viewport2D`; the `BooleanSolid` watertight arm the one Fabrication-side compose against the kernel arrangement seam.

## [02]-[CROSS_CUTTING]

### R6 stale-25xx census (MANDATE point 4 — close it) — THREE genuine references
- `ARCH:16` codemap: "`Faults.cs # FabricationFault band-2500 composing kernel band-2400`" — faults.md landed 2700.
- `kinematics.md:3`: "the typed **band-2500** `FabricationFault`".
- `TASKLOG:44` `[FABRICATION_FAULT_BAND_DEEPENING]-[COMPLETE]`: "InadmissiblePair **2505**/Gouge **2506**/Collision **2507**/NonManif…" — the superseded band inside a [COMPLETE] card, contradicting `TASKLOG:47` `[FAULT_REBAND_2700]`.
- (`faults.md:3` "2501-2509 collision" and `TASKLOG:47` "2701-2709" are CORRECT reband rationale, not stale; `TASKLOG:47` is stale by one — missing Nest +10 = 2710.) PBRIEF `[V12]` makes this a persisted-boundary correctness hazard, not cosmetic: every persisted Fabrication receipt must decode 2701-2710, never 25xx.

### Unwired seams (declared/claimed but not composed)
- **Intra-package pipeline islands (strong):** three "professional floor" owners are realized but NOT wired into the `Run`→`Cam`→`Post` fold: `Guard.Check` (guard.md:3 claims Cam.Solve consults it; motion.md:46-62 doesn't), `Magazine.Schedule`/`ToolChange` (magazine.md:18 claims Post emits it; program.md:141-164 doesn't), `Workholding.Condition` (workholding.md claims it conditions the toolpath; no fold calls it). The safety/tooling/fixturing capability the folder proudly added does not execute.
- **Persistence seams (stale targets):** `Posting/program → Rasm.Persistence/Schema` (ARCH:52) + `Nesting/nfp → Rasm.Persistence/Schema` (ARCH:53) both target the DELETED `Schema/` folder; re-point to blobstore content rows + cache artifact index (PBRIEF `[V12]`); program.md mints no content key for `CutProgram`.
- **Compute forward seam:** `Nesting/stock → Rasm.Compute` `WasteAreaMm2` (ARCH:50) has no Compute-side row (census point 5).
- **Declared-unwired page:** `Toolpath/probing → Rasm.Fabrication/Posting` (ARCH:55) + `Probing.cs` codemap node (ARCH:26) reference a page that doesn't exist (`[PROBING]-[QUEUED]` IDEAS:34).

### Wired-undeclared seams (ledger gaps)
- `Nesting/stock ← Rasm.Element/MaterialId` (stock.md:40) — real cross-package edge absent from ARCH:[02].
- Intra-package web under-declared: magazine→guard/workholding (HolderEnvelope), guard→workholding (ExclusionZone), guard→skeleton (ClearanceAt), motion→skeleton/slicing/kinematics, workholding→program (Sequence), magazine→program (G43/M6) — ARCH:[02] formalizes only ~2 intra seams (stock→nfp, probing→posting) while the codemap prose describes the rest.
- Anchor drift: `physics.md:3` reads `Rasm.Materials/physical-properties#MATERIAL_PROPERTY` vs ARCH:48 `Rasm.Materials/Properties`.

### Page-level cycles
- None confirmed. The magazine↔guard, workholding→program, motion→guard relations are single-direction (no page imports back). Note: guard/magazine/workholding→motion/program are the INTENDED (unwired) consumption direction; wiring them keeps the fold acyclic (owner→cam→{guard,magazine,workholding}→posting).

### Unmined admitted capability (integration mandates — INTEGRATE before ever REMOVE)
Four admitted packages, zero page consumers, each `.api`-catalogued + README-charter'd + csproj-pinned:
- **PicoGK** (csproj:20, README `[IMPLICIT_VOXEL]`, `.api/api-picogk.md`) — implicit/SDF/voxel for additive: TPMS/gyroid/lattice infill, overhang supports, SLA/DLP/MSLA grayscale slices. Exactly the mandate's resin/powder + lattice lane. Realize as an `Additive/` lattice+support+resin owner.
- **OcctNet.Wrapper** (csproj:19, README `[SOLID_INGRESS]`, `.api/api-occtnet-wrapper.md`) — STEP/IGES B-rep ingress → `OcctShape`/`OcctMesh`. The first managed 3D-solid manufacturing-geometry ingress; no page consumes it. Realize as an `Ingress/` solid-CAD owner (single-shape import; TKHLR unbound so HLR stays the projection owner).
- **DSTV.Net** (csproj:16, README `[STEEL_FABRICATION_EXCHANGE]`, `.api/api-dstv-net.md`) — DSTV/NC1 steel profile-cut/saw/drill/punch exchange. Realize as a `Posting/` sibling emit target beside the G-code AST.
- **SharpVoronoiLib** (csproj:24 alias Voronoi, README `[VORONOI_TESSELLATION]`, `.api/api-sharpvoronoilib.md`) — Fortune's 2D Voronoi + Lloyd relaxation for toolpath region partitioning, spiral-pocket seeds, stipple/engrave/pen-plot paths. Realize as a `Toolpath/` region-decomposition arm (point-site only; the polygon medial stays skeleton/kernel).

### Duplication / concern-mixing / hardcoding-vs-generator
- **Concern-mix folders:** `Posting/` conflates cut-program emission (program) + HLR drafting (projection); `Nesting/` conflates part-layout (nfp/stock) + fixture keep-out (workholding); `Toolpath/` conflates subtractive motion (motion/skeleton/guard) + robot kinematics + additive slicing; `Polygon/` conflates 2D algebra (clipper) + CAD ingress (import).
- **Hardcoding-vs-generator:** family.md is the model (rows = seed DATA, generated Switch) and is APPROACH-correct; physics.md `Material` modality-split is the one genuine APPROACH-naivety (parallel modality-keyed material instances where a per-modality payload map belongs). motion.md's 5 placeholder generators are coverage/depth defects, not hardcoding.
- **Dead carriers:** physics.md `Overrides` = `FrozenDictionary.Empty` (physics.md:114); `BarStock` phantom in motion.md prose; the `90.0` fallback constant (physics.md:141).

### Prose-vs-fence splits
- clipper.md prose "Loop widened with Bulges" vs owner.md:42 2-arg Loop (fence).
- workholding.md prose "kind-specific footprint shapes" vs fence MarginScale-only.
- motion.md prose "over the BarStock revolved envelope" vs fence Y-offset `Turn`.
- guard.md/magazine.md prose "Cam.Solve consults" / "Post emits" vs fences that don't wire it.

### Page-craft consistency
- `[03]-[RESEARCH]` tails on the three author-kernel pages (kinematics:122-125, skeleton:199-202, slicing:219-222) — research-origin framing the durable-doc law forbids; the other 14 pages carry none. Fold into design prose (esp. as skeleton/slicing/projection migrate to consumers).

### Host-neutrality note
- Every page except stock.md imports `Rhino.Geometry` (RhinoCommon) — consistent with the kernel being RhinoCommon-typed (kinematics.md:9,33 confirms RhinoCommon=kernel vocabulary, distinct from Rhino3dm=Robots). "Host-neutral" (ARCH:3) means not-Rhino-plugin-bound, not host-free (architecture.md:44). NOT a defect, but the folder carries TWO Rhino geometry stacks (RhinoCommon kernel + Rhino3dm via Robots) — a real weight/identity surface the extern-alias boundary manages correctly. stock.md is the exemplar of a genuinely scalar host-neutral page.

### Folder-architecture verdict (MANDATE overhaul — the graded axis)
The 5-folder/17-page map (Process/Polygon/Toolpath/Nesting/Posting) maps to ~12 namespaces (`Process`, root, `ProcessModel`, `ProcessPhysics`, `Geometry2D`, `Toolpath`, `Kinematics`, `Additive`, `Nesting`, `Fixturing`, `Posting`, `Projection`) — the namespaces already NAME the true decomposition the folders obscure (the same folder≠namespace schism GBRIEF `[V1]` ruled kernel-side, where the realized-namespace scheme WINS). The production-scope overhaul (no one-file-one-folder combos; every folder a growth axis) re-partitions to a namespace-true structure the mandate's new lanes populate:
- `Process/` (owner, faults, family, physics, magazine) — the process/machine/tooling core.
- `Geometry2D/` (clipper + a second 2D owner) — substrate; avoid one-file by pairing.
- `Ingress/` (import + occt STEP/IGES + dstv NC1) — CAD/solid/steel ingress; grows past one file via the two unwired ingress packages.
- `Toolpath/` (motion, skeleton→consumer, guard, + rest-machining, + multi-axis, + voronoi region-decomp) — subtractive CAM.
- `Additive/` (slicing→consumer, + picogk lattice/support/resin, + orientation, + machine-profiles) — production 3DP.
- `Kinematics/` (kinematics + 5-axis-gantry) — motion topology.
- `Nesting/` (nfp, stock) — layout.
- `Fixturing/` (workholding + multi-fixture-setup scheduler) — keep-out.
- `Posting/` (program + dialect/canned-cycle/macro pages + dstv emit) — cut-program emission.
- `Drafting/` (projection→consumer) — HLR/visualization.
- `Spec/` (NEW — tolerances, surface-finish, process-capability, setup-sheets, shop-travelers) — the production-spec mandate lane with no home today.
Thin single-owner folders (Geometry2D, Kinematics, Drafting) reach ≥2 pages exactly through the production-scope demand + the migration-consumer re-framing; the mandate's scope IS the growth that makes the partition legal.

## [03]-[VERDICT_CANDIDATES]

Campaign-defining structural rulings, strongest first, each with evidence.

1. **KERNEL-PLANE CONSUMPTION RE-FRAME (the largest structural pressure).** Five kernel-shaped author-kernels — `skeleton` (medial-with-clearance-radius), `slicing.Section` (slice-stack), `projection` (BSP HLR→DrawingProjection), `motion` offset (corner-strategy), and clipper `ArcAlgebra` (arc-offset) — are the concerns GBRIEF `[V3]`/`[V10]a-e` land KERNEL-side with Fabrication the named demanding consumer (GBRIEF:77,105; ARCH:22,26,32,45,46 counterparts; UIBRIEF:93,193). Ruling: re-frame these from OWNERS to CONSUMERS at the kernel seam, and sharpen the production demand rows the kernel `[V10]` floor doesn't yet carry (3D medial for multi-axis rest machining; conformal/non-planar + variable-by-slope + support-interface slicing; production-tolerance booleans). Waterfall-ripple edits into GBRIEF `[V10]` recording those sharpened demands with Fabrication named. This is the campaign's spine — most of the folder's "author-kernel depth" is migrating.

2. **FOLDER-ARCHITECTURE OVERHAUL to the namespace-true partition.** The 5-folder map hides a ~12-namespace decomposition and mis-files import (ingress under Polygon), slicing (Additive under Toolpath), kinematics (Kinematics under Toolpath), workholding (Fixturing under Nesting), projection (Projection under Posting). Ruling: re-partition to `Process/ · Geometry2D/ · Ingress/ · Toolpath/ · Additive/ · Kinematics/ · Nesting/ · Fixturing/ · Posting/ · Drafting/ · Spec/`, every folder a ≥2-page growth axis the production scope + integration mandates populate; update ARCH `[01]`/`[02]` + README router in the same motion. Evidence: the namespace grep (all 17 pages), the four concern-mix folders.

3. **FOUR INTEGRATION MANDATES (INTEGRATE, never REMOVE — zero consumers never lowers the bar).** PicoGK (additive lattice/support/resin), OcctNet.Wrapper (STEP/IGES solid ingress), DSTV.Net (NC1 steel emit), SharpVoronoiLib (toolpath region-decomposition) are admitted + `.api`-catalogued + charter'd with zero page consumers. Ruling: each lands as a named row/arm/owner on the re-partitioned folders (Additive/, Ingress/, Posting/, Toolpath/). These four ARE the second pages that make the thin folders legal. Evidence: package-consumer grep (all return empty), csproj:16,19,20,24, README `[IMPLICIT_VOXEL]`/`[SOLID_INGRESS]`/`[STEEL_FABRICATION_EXCHANGE]`/`[VORONOI_TESSELLATION]`.

4. **THE UNWIRED SAFETY/TOOLING/FIXTURING PIPELINE.** guard, magazine, workholding are realized owners that the `Run`→`Cam`→`Post` fold never invokes despite each page asserting it does (guard.md:3, magazine.md:18, workholding.md prose vs motion.md:46-62, program.md:141-164). Ruling: wire `Guard.Check` into the per-move motion fold, `Magazine.Schedule`→`Post` G43/M6 emission, and `Workholding.Condition` into the toolpath conditioning — the professional-CAM floor must EXECUTE, not just exist. Acyclic (owner→cam→{guard,magazine,workholding}→posting).

5. **PROCESS-BREADTH THIN SLICES (the production mandate at the axis layer).** Three axes are seed-thin against production scope: `PostDialect` (8 rows, RS-274/Marlin binary — no conversational Klartext/Mazatrol/OSP, no canned-cycle/macro/block-format capability; family.md:80-96, program.md:122-136), `CutStrategy` (8 essentially-2.5D rows — no 3D-surface/5-axis/rest-machining; family.md:34-43), and the posting emit (dialect varies only by Comment/LineNumbers). Ruling: widen the dialect axis with a block-format/canned-cycle/arc-mode/conversational capability column and the strategy axis with 3D-surface + multi-axis + rest rows + a dimensionality column; realize canned-cycle and macro emission on `Program.Post`. These are the CNC/CAM depth + post-processor breadth mandate.

6. **PHYSICS MATERIAL MODALITY-SPLIT (the one genuine APPROACH-naivety).** `Material` carries a single `ModalityPhysics` (physics.md:99), fragmenting one physical material into modality-keyed rows (`stainless`/`stainless-abrasive`; physics.md:91-97) against the one-concept law and the page's own anti-flat-record prose (physics.md:3). Ruling: `Material` carries a per-modality payload map (`Map<RemovalModality, ModalityPhysics>`); `Budget` modality-dispatches into it; one `stainless` identity carries every modality's physics. Also fix the `Overrides = Empty` dead carrier and the `90.0` fallback residue.

7. **THE R6 STALE-25xx CLOSURE + PERSISTENCE SEAM RE-ANCHOR.** Three genuine 25xx references survive the 2700 reband (`ARCH:16`, `kinematics.md:3`, `TASKLOG:44`) — a persisted-boundary correctness hazard per PBRIEF `[V12]` (durable Fabrication receipts must decode 2701-2710, never 25xx). Same motion: re-point the two `→ Rasm.Persistence/Schema` seams (ARCH:52,53) to blobstore content rows + cache artifact index (Schema/ is deleted), content-key the `CutProgram` via `XxHash128` (program.md mints none), and add the wired-undeclared `Nesting/stock ← Rasm.Element/MaterialId` ledger row. Cross-validated by PBRIEF `[FABRICATION_PROGRAM_DURABLE_ROWS]` re-anchor.

8. **NETDXF PURGE + ACADSHARP READ/WRITE SPLIT (census point 3, cross-validated).** Fabrication carries four stale netDxf references (README:33, README:64 `[REJECTED]`, import.md:20, ARCH:51) justifying its read-only ACadSharp scope via "netDxf owns the AppUi write leg" — but UIBRIEF `[V7]` removes netDxf repo-wide and makes AppUi's write leg ACadSharp two-format DXF+DWG (UIBRIEF:127,146,170; packages.lock has no netDxf). Ruling: purge the netDxf framing; the read (Bim/Fabrication) / write (AppUi) split is intra-ACadSharp over the one central pin; the `[REJECTED]` netDxf entry is reframed (netDxf no longer exists to reject).

9. **PRODUCTION-SPEC LANE (uncovered mandate scope).** Tolerances, surface finish, process capability, setup sheets, and shop travelers — named production scope — have no owner anywhere in the corpus. Ruling: a NEW `Spec/` folder (tolerance/finish/process-capability receipts riding the typed-receipt discipline; setup-sheet/traveler as content-keyed shop documents). Machine-tooling ecosystem intelligence (the MTConnect asset model magazine already mines) extends here. Evidence: grep of the whole tree returns no tolerance/finish/capability/traveler owner.

10. **CAVALIERCONTOURS OWNERSHIP RESOLUTION (cross-campaign coordination).** CavalierContours is admitted in Fabrication (csproj:14, clipper.md ArcAlgebra) AND flagged an orphaned kernel-manifest pin the geometry campaign PROMOTES to the `[V10]a` toolpath-offset owner "at whichever stratum that verdict sites … never CavalierContours in two manifests" (GBRIEF:183, `[PLACEMENT_LAW]`). Ruling: the Fabrication campaign aligns to the geometry DECISION's `[V10]a` siting — keep `ArcAlgebra` if CavalierContours sites Fabrication-side (pure kerf arc-compensation), or re-scope it to consume the kernel `OffsetOp` arc seam if the exact-medial-composing offset sites kernel-side. Same coordination for Clipper2 (GBRIEF:184 RE-SCOPEs it from the kernel manifest group to Fabrication's — already Fabrication-pinned, a manifest-grouping alignment).

## [04]-[WATERFALL_RIPPLE_CANDIDATES] (record only; census is read-only)

Upstream owner-extension edits THIS target's production scope demands, framed as consumer pressure with Fabrication named — for the author stage to apply, NOT edited here:
- **GBRIEF `[V10]`:** add sharpened demand rows — 3D medial-with-clearance-radius for multi-axis rest machining (beyond `[V10]d`'s MCF curve-skeleton); conformal/non-planar + variable-by-slope + support-interface slice-stack policy rows (beyond `[V10]b`'s uniform/adaptive); manufacturing-tolerance boolean depth. Fabrication the demanding consumer.
- **CBRIEF (Compute):** a named `AggregateEnvironmental`/`AggregateCost` row reading `NestYield.WasteAreaMm2` through the seam Material node's Environmental/Cost cases (ARCH:50, stock.md:3) — the "recorded next-campaign Compute counterpart" (census point 5).
- **PBRIEF `[V12]`:** already records `[FABRICATION_PROGRAM_DURABLE_ROWS]` re-anchor (Schema→blobstore/cache) + the 2701-2710 decode constraint — verify the two Fabrication→Persistence seam targets re-point on both sides.
- Package waterfall: where PicoGK reaches full power with a companion decoder (SLA/DLP grayscale layer-stack wire) or OcctNet.Wrapper with the mesh/GLB layout Compute owns (SCOPE MANDATE point 5: "Rasm.Compute owns the layout law"), record the enablement both directions.
