# [DRAFT] — RASM-CS-FABRICATION — OUTPUT-FIRST LENS

Lens: **output-first**. The structure is derived BACKWARD from the four flagship egress artifacts. Every folder, page, entry case, and seam exists because a named terminal artifact demands it; a page no flagship (or ruled integration mandate) pulls into being is out of scope. Thesis: **Fabrication is an egress engine — every flagship terminates in a content-keyed machine-consumable artifact, so the organizing spine is the ArtifactKind discriminant + the ONE `ContentHash.Of` content-key fold homed on `owner#atoms`, and every plane exists to feed a terminal `FabricationResult` case that projects to one of those artifacts.** The 13-folder / 33-page floor is confirmed by the backward trace with zero orphan and zero missing home; it is not exceeded (no artifact demands a 14th folder — the egress concern COLLAPSES onto the entry vocabulary rather than fragmenting into a parallel egress folder).

Structure: **13 folders, 33 pages**. Deltas the output-first trace makes load-bearing beyond the other lenses:

1. **The egress spine collapses onto `owner#atoms`.** All five artifact families (word-address/conversational G-code, NC1 steel, `.cli` resin stacks, 3MF hand-offs, setup-sheet/traveler documents) are content-keyed through ONE fold. `ArtifactKind` is a discriminant row family on `owner#atoms`, co-located with the `FabricationResult` cases it keys — never an `Egress/` folder (that would force a compose edge from every plane and re-mint the E9 second-hasher defect). The upstream `ArtifactKind`/durable-row persistence index is a **Fabrication-AUTHORED demand + BLOCKED seam residual** (Persistence `[FABRICATION_PROGRAM_DURABLE_ROWS]`, PBRIEF:221), never a composed upstream binding — the "PBRIEF `[V12]` ArtifactKind" and "CBRIEF `[V12]` WasteAreaMm2 waterfall-landed" bindings the brief cites are REFUTED on disk (upstream-bindings dossier §D-2); this blueprint carries the corrected forward-demand disposition.
2. **Two terminal-egress policy cases beyond the brief's named six.** `FabricationPolicy` grows `{Additive, Verify, Inspect}` (brief-named) PLUS `{Post, Document}` (output-first-mandated): the five-target suite RE-POSTS one `Motion` to five dialects, and the traveler COMPOSES a completed job's result-set — both are re-invocable terminal transforms over a prior result, which under the one-entry law is a `Run` policy case carrying the prior result on `owner#atoms`, never a second public fold. Eight `FabricationPolicy` cases / eight `FabricationResult` cases.
3. **The neutral-AST / dialect-lowering split is forced by re-post.** One part -> five byte-DIVERSE programs means the cut-conditioned `CutProgram` AST is assembled ONCE (dialect-neutral, content-keyed, inside the Cam fold where `Guard`/`Workholding`/`Magazine` execute — resolving E2) and LOWERED N times to bytes (`Posting/dialect` generated grammar switch — resolving E4). `program`=AST+conditioning+`Parse`; `dialect`=per-family byte lowering. Byte-diversity across the five targets is the concrete E4 kill-test.
4. **The run-N -> run-N+1 truth loop is the centerpiece rail.** Flagship 2 closes `spec -> plan -> verify -> capability` as ONE rail: `Verify/removal` produces the residual-stock field + per-setup snapshot (minted on `owner#atoms`), `Toolpath/motion` rest-arms READ them input-carried (no `motion<->removal` page cycle), `Verify/probing` measured features feed `Spec/capability` Cp/Cpk, and the capability gate runs at PLAN time (a process whose Cpk cannot hold the grade fails admission before material is cut). This forces `ResidualStock`/`StockSnapshot`/`CapabilityVerdict` onto `owner#atoms`.
5. **`Documentation/traveler` is the widest fan-in node.** It composes projection views + magazine tool lists + fixturing plans/WCS + program facts + spec rows into ONE content-keyed document model — stress-testing that every upstream owner exposes a typed receipt. The receipt discipline is validated corpus-wide by the terminal composer, not asserted per-page.

---

## [01]-[BACKWARD_TRACE] — flagship artifact -> demanded structure

Each flagship is a chain of `Fabrication.Run` invocations over the 8-case policy; the terminal invocation projects a content-keyed artifact. Every page below is pulled into existence by at least one flagship terminal (or the binding SharpVoronoiLib integration mandate, noted).

**F1 — five-target posting suite** (word-address ×3 grammar-true + conversational + additive; `[05](b)`).
Artifacts: `fanuc.nc`, `haas.nc`, `grbl.nc` (word-address, grammar-true: canned cycles G81-89/G76 as single blocks, macro-B `#`-vars/WHILE, M98/M99 subprograms, G54-59 WCS, G41/42 comp, IJK/R arc mode — each DISTINCT), `klartext.h` (Heidenhain conversational), `marlin.gcode` (FFF additive) + `part.nc1` (DSTV steel) — six byte-diverse files, each content-keyed; a `Parse` round-trip on the word-address subset (`Parse ∘ Emit = id`).
Backward: `Run(Cam)` -> `Motion` carrying the neutral content-keyed `CutProgram` AST (canned-cycle/subprogram/macro nodes with typed R/Q/P slots) => `Toolpath/motion`, `Posting/program`. Then `Run(Post{motion, dialect})` ×5 => `Posting/dialect` (generated grammar-family switch over `PostDialect` capability columns) + `Process/family` (the `PostDialect` axis rows = seed DATA). NC1 target = a sibling emit on the `Post` fold over the `DSTV.Net` record tree => `Ingress/steel` (shared read/emit model). `Parse` arm (NIST modal-group state machine) => `Posting/program`. Content key => `Process/owner#atoms` -> `ContentHash.Of`. Fault arms (inexpressible-cycle/macro-unsupported/block-cap) => `Process/faults`.

**F2 — verified two-setup multi-tool machining job** (spec -> plan -> guard/workholding/magazine wired -> post -> voxel verify -> capability; `[05](d)`).
Artifacts: one two-setup multi-tool program (per-setup G54-59 WCS blocks), voxel gouge/uncut/overcut/air-cut receipts, the op-2 clamp admitted against the op-1 stock snapshot, probe-reconciled work offsets, rest-machining consuming the residual field, a Cp/Cpk row gating the next plan, one setup sheet + one traveler.
Backward: `Spec/tolerance` (the part's GD&T/fits/finish; Ra->scallop + IT-grade->allowance derivation) + `Spec/capability` (plan-time Cpk gate) -> `Nesting/stock`+kernel `[V11]` hull (blank envelope) + `Fixturing/setups` (operation->setup partition, setup-k->WCS assignment rows) + `Fixturing/workholding` (per-kind footprint keep-out, `Condition` wired) + `Tooling/magazine` (schedule wired into the Cam fold) + `Tooling/cuttingdata` (Kienzle `kc` feeds/speeds + deflection) + `Toolpath/{motion (2.5D+rest), surface (3D finish via OCL), guard (per-move), skeleton (medial clearance)}` + `Kinematics/{cell, machine}` (3+2/5-axis topology + TCP) -> `Run(Verify{motion, stock})` => `Verify/removal` (PicoGK `BoolSubtract`, gouge/uncut receipts, residual field, per-setup snapshot) + `Run(Inspect{cycles})` => `Verify/probing` (touch-probe, work-offset metrology, ICP datum best-fit, ConformanceMetric) -> `Spec/capability` (Cp/Cpk) -> `Run(Post)` (per-setup WCS from `setups`) -> `Run(Document)` => `Documentation/{projection, traveler}`. Owner grows `Verify`/`Inspect`/`Post`/`Document` cases + `ResidualStock`/`StockSnapshot`/`CapabilityVerdict` on `owner#atoms`.

**F3 — resin/powder + FFF additive pair** (implicit lane + slice stack + 3MF; `[05](c)`).
Artifacts: a STEP solid in; an oriented part scored; lattice supports + TPMS infill; an FFF `.gcode` program AND a resin `.cli` layer stack AND a 3MF hand-off — three content-keyed egress.
Backward: `Ingress/solid` (OcctNet `ImportStep`->`OcctShape`->`OcctMesh` -> kernel `MeshSpace` admission, dirty STL through kernel `HealOp`) -> `Additive/production` (build-orientation over the overhang/bottom-area/contour objective reading kernel `Analysis/select` `Faces`/`Curves.Draft`; machine profiles; 3MF egress via lib3mf beam-lattice+production writer) + `Additive/implicit` (PicoGK `IImplicit->Voxels` TPMS/gyroid conformal infill, `Lattice` beam supports, SLA/DLP/MSLA grayscale + `.cli` stack via `oVectorize->CliIo`; net9/native ALC-firebreak) + `Additive/slicing` (FFF/DED planar consuming the kernel `Slicing.Apply` slice-stack; perimeter/infill/travel order) -> `Run(Post{additive dialect})` => `Posting/{program, dialect}` (Marlin). `Process/physics` mints `RemovalBudget.{Resin, Powder, DED}`. Content keys => `owner#atoms` -> `ContentHash.Of`.

**F4 — issued setup-sheet/traveler package** (`[05](d)` document half).
Artifact: one content-keyed setup sheet + one shop traveler (typed document MODEL; rendering rides the `[06]` artifacts-plane seam — Fabrication owns the model only).
Backward: `Run(Document{results})` => `Documentation/traveler` composing `Documentation/projection` (kernel `View.Apply` HiddenLine views, preserved `HiddenLineResult` receipt) + `Tooling/magazine` (tool list) + `Fixturing/{workholding, setups}` (fixture plan + WCS) + `Posting/program` (program facts) + `Spec/{tolerance, capability}` (spec rows).

**Structure the four traces demand (13 folders):** `Process` (owner/family/physics/faults), `Tooling` (magazine/cuttingdata), `Geometry2D` (algebra/arcs — the 2D substrate every conditioning fold rides), `Ingress` (profile/solid/steel), `Toolpath` (motion/surface/partition/guard/skeleton), `Kinematics` (cell/machine), `Additive` (slicing/implicit/production), `Nesting` (nfp/stock), `Fixturing` (workholding/setups), `Posting` (program/dialect), `Verify` (removal/probing), `Spec` (tolerance/capability), `Documentation` (projection/traveler). `Toolpath/partition` (SharpVoronoiLib) is pulled by the engrave/stipple/pen-plot artifact class (a real production egress on the same `PostedProgram` case) and is the binding `[V1]` integration realization; `Geometry2D/{algebra,arcs}` is pulled by every conditioning artifact (kerf/lead lives in arc space). No trace lands outside these 13; none demands a 14th.

---

## [02]-[PAGE_SET] — 33 rows, engine-native actions

Action = semantic (KEEP/REBUILD/SPLIT/MERGE/MOVE/DELETE/NEW) + rebuild-engine lowering (`kind` new|rebuild|improve; `del{p}` = deletePages row; `absorb{into,from}`). MOVE -> `new` at destination + `del`+`absorb` at source. SPLIT -> N×`new` + one `del` + N `absorb`. Namespace = folder (1:1 ratified). Entry column names the `FabricationPolicy`/`FabricationResult` case a page grows. Seams: `<-` in, `->` out (one anchor/seam). Wave per `[05]`.

### Process/ — the entry, the axes, the physics, the rail (namespace `Rasm.Fabrication.Process`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Process/owner.md` | KEEP+grow (`improve`) | `owner#atoms`: mint `Loop`+`Bulges`(0/tan(θ/4),3-arg ctor,`BulgeAt`), `Edge3`/`Move`/`PartTransform`, `ProjectionDir`, `ResidualStock`, per-setup `StockSnapshot`, `ArtifactKind` discriminant + content-key seed, `CapabilityVerdict`; `FabricationInput`/`FabricationPolicy`/`FabricationResult` grow to **8 cases** {HiddenLine,Cam,Nest,Additive,Verify,Inspect,**Post**,**Document**} / {HiddenLineResult,Motion,Placement,AdditiveResult,VerifyResult,InspectResult,**PostedProgram**,**TravelerDocument**}. `owner#run`: terminal `Fabrication.Run` fold, `Op`-threaded `Eff<Env>`, every receipt registers on the kernel `IValidityEvidence`/`ValidityClaim` fold reaching `OpAcceptance` with zero oracle edits; output projection rides `AtomProjection`/`ProjectionRow` | `owner#atoms <- Rasm/Domain/identity ContentHash.Of`; `<- Rasm/Domain/validation` admission vocab; `<- Rasm/Numerics/atoms AtomProjection` | `owner#run -> {every plane}.Solve`; `owner#atoms -> {every plane}` (atoms) | 1 |
| `Process/family.md` | REBUILD (`rebuild`) | axis widening; **rename `[SmartEnum]` `Process`->`ProcessKind`** (resolves the `namespace Process`/type `Process` CS0118 collision); `PostDialect` grows GRAMMAR-family capability columns (family {WordAddress\|Conversational\|Additive}, canned-cycle grammar, macro/subprogram convention, WCS roster, cutter-comp admission, arc mode, block cap, decimal/modal policy, per-`GCommand` code-override); `CutStrategy` gains dimensionality column (2.5D\|3D-surface\|multi-axis); `KinematicClass` deepens to rotary-topology rows; `Process`-modality gains additive rows | `<- Process/owner` atoms | `-> physics, magazine, cuttingdata, motion, surface, dialect, cell, machine, workholding` (behavior columns) | 1 |
| `Process/physics.md` | REBUILD (`rebuild`) | identity map: ONE `Material` identity carrying `Map<RemovalModality,ModalityPhysics>`; `Budget` modality-dispatches; `RemovalBudget.{Erosion,Resin,Powder}` land (+DED distinct from FFF); `stainless`/`stainless-abrasive` fragments collapse; dead `Overrides` DELETES; `90.0` fallback demotes behind the table; `CuttingData` table MOVES OUT to `Tooling/cuttingdata` | `<- Rasm.Materials/Properties` raw-double peer boundary; `<- UnitsNet` | `-> motion, surface, cuttingdata, dialect` (RemovalBudget) | 1 |
| `Process/faults.md` | KEEP+payloads (`rebuild`) | namespace root->`Process`; retype every arm's bare `string Detail` to typed payload (`Gouge`->point+tool, `Collision`->zone, `InadmissiblePair`->typed pair, `NoFit`->part+rotations); 2701-2710 band FROZEN (build ON); new `+11..` arms per the fault registry | `<- Rasm.Element` FaultBand registry (2500 note STAYS) | `-> {every plane}` fault rail | 1 |

### Tooling/ — ISO-13399 tool intelligence (namespace `Rasm.Fabrication.Tooling`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Tooling/magazine.md` | MOVE+wire (`new` @Tooling; `del{Process/magazine.md}` `absorb{into:Tooling/magazine.md,from:Process/magazine.md}`) | namespace `ProcessModel`->`Tooling`; preserve MTConnect `CuttingToolAsset` mining + life-split `Schedule`; project typed `CutterForm` axis (flat/ball/bull/taper/drill/chamfer/thread-mill w/ diameter/corner-radius/taper-angle/flute-length) from the ISO-13399 measurement set; `Runout` reads the real measurement (not literal `0.01`); `GenerateHash` reconciles onto `ContentHash.Of`; `Schedule` feeds the **Cam fold** (neutral M6/G43/Tnn tool-change blocks) | `<- MTConnect.NET-Common`; `<- Riok.Mapperly` (decoded-telemetry mappers); `<- family` CutterForm host | `-> cuttingdata, surface, removal, guard` (CutterForm); `-> motion` (Schedule, in Cam fold); `-> traveler` (tool list); `<- Rasm.AppHost` decoded tool-life telemetry (livewire seam) | 1 |
| `Tooling/cuttingdata.md` | NEW (`new`) | Kienzle machinability model: unit-cutting-force `kc` + chip-thickness exponent + per-op surface-speed + feed-per-tooth columns keyed by the `physics` `Material` identity; CSV/QIF data-INGRESS arm (honest seed DATA, no formula-as-measured); effective-diameter speed derivation on ball/taper; the `0.0012` force literal and `90.0` fallback die here | `<- physics` Material identity; `<- family` CutterForm; `<- FreeCAD machinability (REFERENCE schema)` | `-> motion, surface` (feeds/speeds+deflection) | 1 |

### Geometry2D/ — the 2D substrate (namespace `Rasm.Fabrication.Geometry2D`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Geometry2D/algebra.md` | SPLIT from clipper (`new`; `del{Polygon/clipper.md}` `absorb{into:Geometry2D/algebra.md,from:Polygon/clipper.md}`) | Clipper2 LINE-space owner: offset/`InflatePaths`, `DeltaCallback64` variable offset, Boolean clip, Minkowski sum/diff, open-path clip, `ReuseableDataContainer64` scanbeam reuse; reads the widened `Loop` | `<- Clipper2`; `<- owner#atoms Loop` | `-> nfp, guard, motion, workholding, arcs` | 2 |
| `Geometry2D/arcs.md` | SPLIT from clipper (`new`; `absorb{into:Geometry2D/arcs.md,from:Polygon/clipper.md}`) | CavalierContours ARC-space owner (**[V10]a RULED: kernel owns the exact corner-row `JoinType`×`EndType` offset assembly + medial via `Offsetting.Apply`; Fabrication owns pure kerf compensation in arc space ON TOP over CavalierContours — one concern per stratum, never two manifests**); `PlineOffset.ParallelOffset<O,T>`, island-preserving `Shape<T>.FromPlines`, `FindPointAtPathLength`, `StaticAABB2DIndex`; **fix `result.Positive` phantom -> `result.PosPlines[].Pline`** (assay-verify leg 1); reads `Loop.BulgeAt`; kerf/lead/adaptive rails wire through here over kernel-emitted region offsets | `<- CavalierContours`; `<- Rasm/Meshing/offset Offsetting.Apply` (kernel corner-row offsets, signature-locked); `<- owner#atoms Loop.Bulges` | `-> motion, program` (arc-native offsets); `-> dialect` (arc mode) | 2 |

### Ingress/ — everything entering as geometry (namespace `Rasm.Fabrication.Ingress`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Ingress/profile.md` | MOVE (`new` @Ingress; `del{Polygon/import.md}` `absorb{into:Ingress/profile.md,from:Polygon/import.md}`) | namespace `Geometry2D`->`Ingress`; ACadSharp read-only DXF/DWG boundary (`Failsafe` rail, Spline/Arc/Circle/Insert samplers, `Insert.Explode`); dead `warnings` accumulator wires the `NotificationType.Error` strict escalation; netDxf rejection frame DELETES (nothing to reject); emits kernel-admissible `Loop` | `<- ACadSharp`; `<- Rasm.Bim/Exchange` read seam; `<- Riok.Mapperly` (DXF-entity->Loop) | `-> nfp, program` (2D profiles) | 2 |
| `Ingress/solid.md` | NEW (`new`) | OcctNet `ImportStep`(AP203/214/242)/`ImportIges`/`ImportStl`->`OcctShape`->`Triangulate`->`OcctMesh`; the tessellated mesh CROSSES to kernel vocabulary at the seam (`MeshSpace` admission; dirty STL through kernel `HealOp`); SCOPE_LIMIT (single-shape; `libTKHLR`/`libTKXCAF` unbound) recorded as a standing demand row; boundary-maps at the `OcctShape`/`OcctMesh` seam | `<- OcctNet.Wrapper`; `<- Rasm/Meshing/mesh MeshSpace`; `<- Rasm/Processing/repair HealOp` | `-> slicing, surface, projection` (admitted mesh) | 2 |
| `Ingress/steel.md` | NEW (`new`) | DSTV.Net NC1 record tree READ (`ST` header + `BO`/`SI`/`SC`/`AK`/`IK`/`KA` blocks); the `KA` bend rows seed the sheet-metal IDEAS lane; emits `Loop`+hole/marking rows; the SAME record tree is the emission model `Posting/dialect` renders (shared, never duplicated) | `<- DSTV.Net`; `<- Riok.Mapperly` (DSTV-record->Loop) | `-> nfp, program` (steel profiles); `<-> dialect` (NC1 record model, bidirectional) | 2 |

### Toolpath/ — subtractive CAM (namespace `Rasm.Fabrication.Toolpath`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Toolpath/motion.md` | REBUILD (`rebuild`) | `Cam.Solve` grows `FabricationPolicy.Cam`->`Motion`; real `(RemovalModality,CutStrategy)` generators — `Turn` true ZX sweep over the revolved stock envelope (kernel `Analysis/measure Bounds` enclosing-cylinder), `helical` distinct thread/ramp (alias dies), `Peck` real canned-cycle geometry, `SliceWalk` a perimeter/infill/travel orderer over the real `SliceLayer`; REST-MACHINING arm reads the input-carried `ResidualStock` (`[V7]`, never a motion->removal edge); `Guard.Check` WIRED per committed feed move; engagement bound reads the medial clearance field AND the physics chip-load budget; `Placement` runs kerf/lead/pierce/tab/sequence conditioning, `Motion` carries CAM-conditioned tool-center moves (split stated — a kerf pass over `Motion` is the forbidden double-compensation); assembles the neutral content-keyed `CutProgram` AST | `<- family` CutStrategy/Admits; `<- physics`+`cuttingdata` budgets; `<- algebra`+`arcs` offsets; `<- skeleton` clearance; `<- guard` per-move; `<- surface` 3D passes; `<- cell` RobotProgram; `<- owner#atoms ResidualStock` | `-> program` (neutral CutProgram); `-> Rasm/Drawing/pack PackKind.toolpath` (committed motion residency, ledger row) | 3 |
| `Toolpath/surface.md` | NEW (`new`) | 3D finishing: waterline/Z-level, constant-scallop, pencil/corner-trace, flowline/morph rows; **ADMIT OpenCAMLib** (probe FEASIBLE) as the drop/push/waterline cutter-POSITIONING engine via in-house `extern "C"` C-shim + `[LibraryImport]` P/Invoke, ALC-firebreak/sidecar, RID assets osx-arm64/win-x64/linux-x64, golden-fixture gated, content-keyed wire; path LAYOUT composes the kernel on-mesh machinery (geodesic-parallel <- `Processing/geodesics` via `fields Geodesic`, isolines <- `extract ContourPolicy.MeshScalar`, flowline <- `flow` streamlines, direction fields <- `segment`); Ra->scallop stepover reads `Spec/tolerance`; **author-kernel drop-cutter over kernel `[V8]` SDF is the RECORDED FALLBACK, stands only on admission abandonment** | `<- OpenCAMLib` (shim); `<- Rasm/Processing/{geodesics,extract,flow,segment}`; `<- Tooling/cuttingdata`+`magazine CutterForm`; `<- Spec/tolerance` | `-> motion` (3D passes) | 3 |
| `Toolpath/partition.md` | NEW (`new`) | SharpVoronoiLib `[V1]` mandate realization: `VoronoiPlane` Fortune sites + Lloyd `Relax(iterations,strength,reTessellate)`, `VoronoiSite.Centroid` seeds; region-decomposition pocketing, stipple/engrave/even-spacing/pen-plot path strategy rows (point-site only — polygon medial stays the kernel's) | `<- SharpVoronoiLib`; `<- algebra` | `-> motion` (partition strategies) | 3 |
| `Toolpath/guard.md` | KEEP+wire (`improve`) | interior unchanged (swept Minkowski envelope, part-keep/stock-keep-out `Verdict` distinct); DECLARE the three out-seams; consumed per-move by the Cam fold; stock keep-out reads the CURRENT per-setup snapshot not the raw blank; stays the design-time floor DISTINCT from `Verify/removal` | `<- algebra` Minkowski; `<- Rasm/Spatial/index BVH`; `<- skeleton ClearanceAt`; `<- magazine HolderEnvelope`; `<- workholding ExclusionZone`; `<- owner#atoms StockSnapshot` | `-> motion` (per-move verdict) | 3 |
| `Toolpath/skeleton.md` | REBUILD (`rebuild`) | **[V10]d RULED: `Toolpath/Skeleton.cs` dies UNCONDITIONALLY for `Offsetting.Apply`; the kernel medial carries the per-point clearance RADIUS + `Clearance(Point3d probe)` arbitrary-probe query as first-class result fields (a radius-less medial is a kernel `[V10]` defect repaired upstream, never license to keep the author-kernel)**; keep ONLY the trochoidal constant-engagement WALK over the kernel clearance field; delete `Wavefront`/`Propagate`/`OffsetAt`/`SkeletonEvent`; purge the `[RESEARCH]` tail + false "no managed library" seal; signature-lock `Skeletonize.Apply(SkeletonOp,Op?)->Fin<CurveSkeleton>` composing offset.md's clearance family | `<- Rasm/Meshing/skeleton Skeletonize.Apply`; `<- Rasm/Meshing/offset Offsetting.Apply` (clearance radius) | `-> motion, guard` (clearance field) | 2 |

### Kinematics/ — motion topology (namespace `Rasm.Fabrication.Kinematics`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Kinematics/cell.md` | MOVE (`new` @Kinematics; `del{Toolpath/kinematics.md}` `absorb{into:Kinematics/cell.md,from:Toolpath/kinematics.md}`) | namespace `Kinematics` (folder `Toolpath`->`Kinematics`), page `kinematics`->`cell`; Robots serial-chain solver held (`extern alias R3` boundary exemplar, `Riok.Mapperly` generated R3 seam mappers); band-2500->2700; purge `[RESEARCH]` tail; its jerk/accel planner NAMES the shared motion-dynamics law | `<- Robots` (->Rhino3dm R3); `<- Riok.Mapperly`; `<- machine` motion-dynamics law | `-> motion` (RobotProgram.Solve) | 3 |
| `Kinematics/machine.md` | NEW (`new`) | 5-axis machine-tool topology closing the gap `family` `mill-5axis`->`CartesianGantry` binds: `KinematicClass` rotary-topology rows (table-table trunnion\|head-head\|head-table\|nutating); rotary-axis inverse + TCP/RTCP admission; the shared jerk/accel motion-dynamics policy shape posting `Lookahead` reads; orientation interpolation composes the kernel `Parametric/projections MotionInterpolation` one-slerp owner (never a second slerp site) | `<- family` KinematicClass; `<- Rasm/Parametric/projections MotionInterpolation` | `-> cell, motion, dialect` (dynamics law + rotary topology) | 3 |

### Additive/ — production 3DP (namespace `Rasm.Fabrication.Additive`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Additive/slicing.md` | MOVE+re-route (`new` @Additive; `del{Toolpath/slicing.md}` `absorb{into:Additive/slicing.md,from:Toolpath/slicing.md}`) | namespace `Additive` (folder `Toolpath`->`Additive`); **`[V2]b` RULED: `Section`/O(n²)`Chain` DIE, re-route to the kernel `Slicing.Apply(SliceOp,Op?)->Fin<SliceStack>` (4 layer policies + 5-channel SoA forest)**; keep FFF/DED `InfillPattern` + shell composition over kernel-emitted oriented contours; gyroid/TPMS row CORRECTS off the 2D-hatch collapse -> routes to `implicit` voxel lane; adaptive/support-interface layer height = kernel slice-stack policy rows (Fabrication-named demand, not a Fabrication engine); purge `[RESEARCH]` tail + false seal | `<- Rasm/Meshing/slice Slicing.Apply`; `<- Ingress/solid` mesh; `<- algebra` | `-> program` (FFF layer program); `-> production` (slice stack) | 3 |
| `Additive/implicit.md` | NEW (`new`) | PicoGK voxel lane: `IImplicit->Voxels` TPMS/gyroid/cellular conformal infill (corrects the `slicing.md:19` false 2D-hatch collapse), `Lattice.AddBeam` supports + overhang-conditioned scaffolds, SLA/DLP/MSLA grayscale + `.cli` vector layer stacks (`oVectorize->CliIo`,`Vdb2Cli`), voxel offset/shell lightweighting; **declares the ONE net9/native ALC-firebreak/sidecar posture + the content-keyed mesh<->`Voxels` wire** `Verify/removal` composes | `<- PicoGK`; `<- Rasm/Meshing/mesh MeshSpace` (wire) | `-> production` (implicit geometry); `-> removal` (declared voxel seam); `.cli` egress -> `owner#atoms ArtifactKind.CliStack` | 3 |
| `Additive/production.md` | NEW (`new`) | grows `FabricationPolicy.Additive`->`AdditiveResult`; build-orientation optimization (overhang/bottom-area/contour objective reading kernel `Analysis/select Faces`/`Curves.Draft` per candidate, never a hand-rolled normal classifier); machine profiles (build volume, nozzle/vat/laser, kinematics class as rows); **ADMIT lib3mf** (BSD-2, vendored official C# binding + RID native asset, golden-fixture gated) 3MF core+production+beam-lattice writer (PicoGK `Lattice`->beam-lattice extension; STL-implied hand-off dies) | `<- lib3mf`; `<- Rasm/Analysis/select`; `<- implicit`+`slicing` | 3MF egress -> `owner#atoms ArtifactKind.ThreeMf`; `-> traveler` | 3 |

### Nesting/ — layout + yield (namespace `Rasm.Fabrication.Nesting`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Nesting/nfp.md` | KEEP+identity fix (`rebuild`) | `Nest.Solve` grows `FabricationPolicy.Nest`->`Placement`; `Stock.Of` hashes discriminant+ALL dimensions through `ContentHash.Of` (area-only collision poisoning `Remnant.Parent` dies); `Heuristic` `Ty*1e6+Tx`->`(Ty,Tx)` tuple; NFP reference-frame incoherence PROVEN/fixed (candidates translated by placed `(pl.Tx,pl.Ty)`, single frame) on the golden fixture; all 3 raw `XxHash128` mints route `ContentHash.Of`; RectpackSharp fast-path RE-HOMES to a `MaxRectsBinPack` heuristic sweep (RectpackSharp REMOVE) | `<- algebra` Minkowski; `<- ContentHash.Of`; `<- stock NestPlan`; `<- RectangleBinPack.CSharp`; `<- Rasm/Processing/flatten ChartAtlas` | `-> program` (nested profiles); `-> Rasm.Persistence` durable rows (**BLOCKED demand**, see ledger) | 3 |
| `Nesting/stock.md` | KEEP (`improve`) | scalar exemplar unchanged; ADD the `<- Rasm.Element/MaterialId` ledger row (wired-undeclared); `[RESEARCH]` tail purges; `RectangleBinPack.CSharp` pin re-group from vacated Materials label verified; blank-envelope feeds `[V11]` hull consumers | `<- RectangleBinPack.CSharp`; `<- Rasm.Element/MaterialId`; `<- Rasm.Vectors PositiveMagnitude` | `-> nfp` NestPlan; `-> Rasm.Compute WasteAreaMm2` (**one-sided forward seam**, see ledger) | 3 |

### Fixturing/ — keep-out + setup planning (namespace `Rasm.Fabrication.Fixturing`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Fixturing/workholding.md` | MOVE+safety fix (`new` @Fixturing; `del{Nesting/workholding.md}` `absorb{into:Fixturing/workholding.md,from:Nesting/workholding.md}`) | namespace `Fixturing` (folder `Nesting`->`Fixturing`); `Clears`/`Condition` use segment-vs-polygon intersection (thin diagonal clamp crossing now caught); per-kind REAL footprint-shape geometry columns (two-jaw vise, revolved chuck jaw, full-bed vacuum) not a `MarginScale` scalar; `ForHolding` a TOTAL mapping (`Vise` reachable); `Condition` WIRED into the Cam conditioning fold, actually composing `Posting.Sequence` | `<- algebra` Offset/Clip; `<- family HoldingClass`; `<- owner#atoms Loop.Covers` | `-> guard ExclusionZone`; `-> motion` (Condition, in Cam fold); `-> program Sequence`; `-> setups` footprint; `-> traveler` | 3 |
| `Fixturing/setups.md` | NEW (`new`) | realizes `[MULTI_FIXTURE_SCHEDULE]`; `Setup [ComplexValueObject]` (Fixture, WCS datum, ReachableOps); `Schedule` partitions ops across reorientation setups by reach-and-clearance feasibility, orders to minimize re-fixturing (op-precedence = the NP-hard half over `QuikGraph`); OWNS setup-k->WCS assignment rows (G54-59.x/G54.1-Pn per dialect); admits op-N workholding against the op-N-1 `StockSnapshot`; per-setup datum-shift Posting RENDERS (never a posting-side default) | `<- QuikGraph`; `<- workholding` footprint; `<- machine` reach; `<- owner#atoms StockSnapshot` | `-> program` (per-setup WCS rows); `-> guard` (per-setup input); `-> probing` (datum); `-> traveler` | 3 |

### Posting/ — machine-code emission (namespace `Rasm.Fabrication.Posting`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Posting/program.md` | REBUILD (`rebuild`) | dialect-NEUTRAL cut-conditioning author-kernel (Kerf/Simplify/Compensate/Feedrate/Lookahead/Sequence held; `Lookahead` reads the shared `machine` motion-dynamics law); AST grows structure — canned-cycle nodes (typed R/Q/P slots), subprogram units (repeat counts), macro parameter slots — so per-family lowering is node EXPANSION not string synthesis; the arc rail reads `arcs` arc-native offsets + `Loop.Bulges` (`new BiArcFit2` retained ONLY for line-sourced kernel mesh-section chains until the owned Bolton biarc fold lands); `Parse` round-trip arm (NIST modal-group state machine: motion/plane/distance/units/feed groups); `CutProgram` gains its content key (mints on `owner#atoms`->`ContentHash.Of`); `0.0012` force literal dies; purge `[RESEARCH]` tail | `<- arcs`+`algebra`; `<- owner Motion/CutProgram`; `<- geometry3Sharp` (line-sourced biarc, REPLACE-candidate); `<- Rasm/Numerics/predicates Orient2D` (REAL) | `-> dialect` (neutral AST); `-> traveler` (program facts); `-> Rasm.Persistence` durable rows (**BLOCKED demand**) | 4 |
| `Posting/dialect.md` | NEW (`new`) | grows `FabricationPolicy.Post{Motion,PostDialect}`->`PostedProgram`; `Emit` = generated TOTAL switch over the `PostDialect` family axis (data in `family`, lowering here); word-address grammar (G81-89/G76 canned as single blocks where admitted, macro-B/R/Q, M98/M99, G54-59, G41/42, IJK/R) + conversational (Klartext/Mazatrol/OSP) + additive (Marlin/RepRap); NC1 emit TARGET over the shared `DSTV.Net` record tree; per-dialect `GCommand` code overrides (`Dwell`/`Pierce` "G4" collapse to one command+flag); rows widen as seed DATA (Siemens 840D/Heidenhain TNC/Okuma OSP/Fagor/Centroid); byte-DIVERSE across families is the acceptance test | `<- family` PostDialect columns; `<- owner Motion`; `<- setups` WCS; `<- steel` DSTV record tree; `<- arcs` arc mode; `<- machine` dynamics | `PostedProgram`+NC1 egress -> `owner#atoms ArtifactKind.{GProgram,Nc1}` | 4 |

### Verify/ — program-level truth (namespace `Rasm.Fabrication.Verify`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Verify/removal.md` | NEW (`new`) | grows `FabricationPolicy.Verify{Motion,stock}`->`VerifyResult`; voxel material-removal — stock `Voxels` minus accumulated swept-tool volume via PicoGK `BoolSubtract` (a ball-nose sweep IS the capsule beam); typed gouge/uncut/overcut/air-cut receipts against the ACTUAL removed state; produces the `ResidualStock` field the `[V5]` rest arm reads + content-keyed per-SETUP `StockSnapshot` (op-N `setups`/`guard` read the CURRENT snapshot); composes `implicit`'s declared voxel posture (never a second) | `<- PicoGK` (via implicit seam); `<- owner Motion`+`StockSnapshot`; `<- magazine CutterForm` swept | `VerifyResult`+ResidualStock+snapshot -> `owner#atoms`; `-> capability` (uncut evidence) | 4 |
| `Verify/probing.md` | NEW (`new`) | realizes `[PROBING]`; grows `FabricationPolicy.Inspect`->`InspectResult`; touch-probe cycle vocabulary (G31/G38 rows on the posting AST), work-offset/tool-length metrology; datum best-fit composes the kernel `Processing/register AlignKind` ICP under one `AlignmentPolicy` (probed->nominal, never a hand-rolled fit); measured-vs-nominal folds through kernel `Analysis/measure ConformanceMetric` over `ResidualSample`; QIF (ISO 23952) MeasurementResults feed `capability`; measured ingress via the AppHost decoded-telemetry seam OR typed manual rows | `<- Rasm/Processing/register ICP`; `<- Rasm/Analysis/measure ConformanceMetric`; `<- Rasm.AppHost` MTConnect decoded; `<- setups` datum; `<- NodaTime` stamps | `-> capability` (measured features); `-> setups` (work-offset reconcile); `-> traveler` | 4 |

### Spec/ — production specs (namespace `Rasm.Fabrication.Spec`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Spec/tolerance.md` | NEW (`new`) | typed GD&T vocabulary: ISO 286 fits (grade/deviation tables as GENERATED rows), ISO 1101/ASME Y14.5 feature-control frames + datum systems, ISO 1302 surface texture (Ra/Rz+lay+marks), MMC/LMC; QIF-aligned; DRIVES process — Ra/Rz+corner-radius->constant-scallop stepover (a `surface` derivation row), ISO 286 IT-grade->finishing-pass allowance (conditions `[V5]` pass planning); Fabrication owns the VOCABULARY (the py drawing plane draws FROM supplied values); UnitsNet overlay extends Force/Power/Temperature/Angle/Torque | `<- QIF/ISO 23952 schemas`; `<- UnitsNet` | `-> surface` (Ra->scallop); `-> motion` (IT->allowance); `-> traveler`; `-> py drawing plane` (supplier, `[06]` seam) | 3 |
| `Spec/capability.md` | NEW (`new`) | Cp/Cpk/Pp/Ppk on the kernel `Domain/stats Stat.Of` Welford + `Distribution.Of` quantiles + `StatContext.Tolerance` (probing evidence arrives pre-folded); distribution-fit + Monte-Carlo tolerance-stackup over MathNet.Numerics (first Fabrication consumer); SPC control-limit rows; the capability GATE runs at PLAN time — a process whose Cpk history cannot hold the demanded grade fails admission before material is cut (closes spec->plan->verify->capability) | `<- Rasm/Domain/stats`; `<- MathNet.Numerics`; `<- probing` measured; `<- removal` uncut | `CapabilityVerdict -> owner#atoms` (plan-admission gate); `-> traveler` | 4 |

### Documentation/ — shop documentation (namespace `Rasm.Fabrication.Documentation`)

| Page | Action (lowering) | Entry / case growth | In-seams `<-` | Out-seams `->` | W |
|---|---|---|---|---|---|
| `Documentation/projection.md` | MOVE+rebuild (`new` @Documentation; `del{Posting/projection.md}` `absorb{into:Documentation/projection.md,from:Posting/projection.md}`) | namespace `Projection`->`Documentation`; **`[V3]` RULED: the BSP interior DIES for the kernel `View.Apply(ViewOp,Op?)->Fin<DrawingProjection>` {HiddenLine,Silhouette,Section,Creases}**; KEEP the `BooleanSolid` watertight arm (kernel `Arrangement.Apply`/`ToMesh` compose-seam); **PRESERVE the `HiddenLineResult` receipt shape AppUi is insulated at** (UIBRIEF `[V7]`(b)); drop the phantom `Predicate.Orient2D` import (raw-double signs it never called); grows `FabricationPolicy.HiddenLine`->`HiddenLineResult` | `<- Rasm/Drawing/view View.Apply` (signature-locked); `<- Rasm/Meshing/arrangement`; `<- Ingress/solid` | `-> Rasm.AppUi/Render HiddenLineResult` (receipt, supersession insulated); `-> traveler` (views) | 4 |
| `Documentation/traveler.md` | NEW (`new`) | grows `FabricationPolicy.Document`->`TravelerDocument`; setup sheets + shop travelers as content-keyed shop documents (typed MODEL only; sheet/annotation RENDERING rides the `[06]` artifacts-plane seam); COMPOSES projection views + magazine tool lists + workholding/setups plans+WCS + program facts + spec rows; the widest fan-in node | `<- projection, magazine, workholding, setups, program, tolerance, capability`; `<- NodaTime` stamps | `TravelerDocument` egress -> `owner#atoms ArtifactKind.Traveler` | 4 |

**Delete-with-absorb summary (7 deletePages / 8 absorb pairs):** `del{Process/magazine.md}`->`{Tooling/magazine}`; `del{Polygon/clipper.md}`->`{Geometry2D/algebra}`+`{Geometry2D/arcs}`; `del{Polygon/import.md}`->`{Ingress/profile}`; `del{Posting/projection.md}`->`{Documentation/projection}`; `del{Toolpath/slicing.md}`->`{Additive/slicing}`; `del{Toolpath/kinematics.md}`->`{Kinematics/cell}`; `del{Nesting/workholding.md}`->`{Fixturing/workholding}`. Folders emptied and removed: `Polygon/` (both pages leave), `Process/`+`Toolpath/`+`Nesting/`+`Posting/` shed pages but survive. In-place: `owner`(improve), `family`/`physics`/`faults`/`motion`/`skeleton`/`nfp`/`program`/`projection`-rebuild-then-move (rebuild), `guard`/`stock`(improve). Physical `owner.md` stays ONE file split into two ledger NODES (`owner#atoms`/`owner#run`) — not a physical split (the two-node ledger passes).

---

## [03]-[NAMESPACE_RATIFICATION] — folder = namespace = fault-cluster, 1:1

13 folders, 13 namespaces, mechanically 1:1. The realized-namespace scheme wins clean (Fabrication carries NO external freeze). Every schism re-homes in the same MOVE; `ARCHITECTURE.md [01]`/`[02]` + README router rewrite in the same motion.

| Folder | Namespace | Ratification (schism resolved) |
|---|---|---|
| `Process/` | `Rasm.Fabrication.Process` | `family`+`physics`+`faults` re-home: `ProcessModel`/`ProcessPhysics`/root -> `Process`. **Collision fix: `[SmartEnum]` type `Process`->`ProcessKind`** so `family`'s type no longer resolves as `Rasm.Fabrication.Process.Process` (the latent CS0118 `program.md` already risks) |
| `Tooling/` | `Rasm.Fabrication.Tooling` | `magazine` leaves `ProcessModel` -> `Tooling`; `cuttingdata` NEW |
| `Geometry2D/` | `Rasm.Fabrication.Geometry2D` | `clipper` SPLITS (`Polygon` folder, `Geometry2D` namespace) -> `algebra`+`arcs` |
| `Ingress/` | `Rasm.Fabrication.Ingress` | `import` leaves `Geometry2D` -> `Ingress`; `solid`/`steel` NEW |
| `Toolpath/` | `Rasm.Fabrication.Toolpath` | `motion`/`guard`/`skeleton` already folder-true; `surface`/`partition` NEW |
| `Kinematics/` | `Rasm.Fabrication.Kinematics` | `kinematics` leaves `Toolpath` -> `Kinematics/cell`; `machine` NEW |
| `Additive/` | `Rasm.Fabrication.Additive` | `slicing` leaves `Toolpath` -> `Additive`; `implicit`/`production` NEW |
| `Nesting/` | `Rasm.Fabrication.Nesting` | `nfp`/`stock` already folder-true |
| `Fixturing/` | `Rasm.Fabrication.Fixturing` | `workholding` leaves `Nesting` -> `Fixturing`; `setups` NEW |
| `Posting/` | `Rasm.Fabrication.Posting` | `program` already folder-true; `dialect` NEW |
| `Verify/` | `Rasm.Fabrication.Verify` | `removal`/`probing` NEW |
| `Spec/` | `Rasm.Fabrication.Spec` | `tolerance`/`capability` NEW |
| `Documentation/` | `Rasm.Fabrication.Documentation` | `projection` leaves `Projection` -> `Documentation`; `traveler` NEW |

Six schism ratifications the brief `[V1]` names all execute in-motion (family/magazine ex-`ProcessModel`, physics ex-`ProcessPhysics`, faults ex-root, import ex-`Geometry2D`, projection ex-`Projection`), plus the three the toolpath-nesting dossier confirmed (kinematics ex-`Toolpath`, slicing ex-`Toolpath`, workholding ex-`Nesting`, clipper `Polygon`->`Geometry2D`).

---

## [04]-[FAULT_REGISTRY] — one 2700-century offset block per folder

The `FabricationFault` rail stays ONE `[Union]` in `Process/faults.md` on `FaultBand.Fabrication = 2700`; "folder = fault-cluster" is the offset-block ALLOCATION, mechanically checkable. Landed 2701-2710 are FROZEN (archive law — build ON, never re-band); their frozen codes map to their now-home cluster. Growth `+11..` sized by the 5x law; 2701-2740 used, 2741-2799 reserve (fits the century, no federation escalation).

| Cluster (folder) | Offsets | Arms |
|---|---|---|
| Process (owner/family/physics/faults) | 2701-2704 (frozen) | the original four process/admission arms + `InadmissiblePair 2705` (frozen) |
| Toolpath (motion/surface/partition/guard/skeleton) | 2706-2707 (frozen) + 2718-2719 | `Gouge 2706`, `Collision 2707` (frozen, guard cluster); `DropCutterSampling 2718`, `EmptyPartition 2719` |
| Additive (slicing/implicit/production) | 2708 (frozen) + 2723-2726 | `NonManifoldSlice 2708` (frozen); `UnsupportedImplicit 2723`, `OrientationInfeasible 2724`, `SliceStackFault 2725`, `Unsupported3mfExtension 2726` |
| Nesting (nfp/stock) | 2709-2710 (frozen) | `StockOverflow 2709`, `Nest 2710` (frozen) |
| Ingress (profile/solid/steel) | 2711-2714 | `UnreadableSource`, `OcctUnboundFeature`, `MalformedNc1`, `UnhealableMesh` |
| Geometry2D (algebra/arcs) | 2715 | `KerfToleranceFault` |
| Tooling (magazine/cuttingdata) | 2716-2717 | `NoToolForOp`, `CuttingDataMissing` |
| Kinematics (cell/machine) | 2720-2722 | `RotaryLimit`, `TcpUnreachable`, `DynamicsViolation` |
| Fixturing (workholding/setups) | 2727-2729 | `NoFeasibleSetup`, `ClampOnMachinedFace`, `OverSetupCap` |
| Posting (program/dialect) | 2730-2732 | `InexpressibleCycle`, `MacroUnsupported`, `BlockCapExceeded` |
| Verify (removal/probing) | 2733-2736 | `ProbeOvertravel`, `UncutExceeds`, `OvercutGouge`, `AirCutWaste` |
| Spec (tolerance/capability) | 2737-2739 | `CapabilityGateFail`, `ToleranceInexpressible`, `StackupExceeds` |
| Documentation (projection/traveler) | 2740 | `MissingComposeSource` |

Every arm carries its typed payload (never bare `string Detail`): `Gouge`->point+tool, `Collision`->zone, `RotaryLimit`->axis+angle, `NoFeasibleSetup`->op+tried-fixtures, `CapabilityGateFail`->process+demanded-grade+held-Cpk, `ProbeOvertravel`->cycle+limit. R6 25xx purge closes corpus-wide: `ARCH:16`, `kinematics.md:3`, `TASKLOG:44`, `api-dstv-net.md:3,126`, `api-robots.md:5,94` -> 2700; `faults.md:33` "Element's 2500 band" note STAYS (Element genuinely owns 2500).

---

## [05]-[SEAM_LEDGER] — corrected, both directions

`owner` evaluates as TWO nodes: `owner#atoms` (upstream-only declarations — `Loop`+`Bulges`, `ProjectionDir`, `ResidualStock`, `StockSnapshot`, `ArtifactKind`, content-key seed) and `owner#run` (terminal fold, compose edges only). Cross-plane RESULT data rides the entry vocabulary (`ResidualStock`/`StockSnapshot`/`CapabilityVerdict` mint on `owner#atoms`), so the `motion->removal->program->motion` and `motion<->removal` cycles never form. Graph acyclic; leg order (1->4) is a valid topological sort.

### Cross-package edges (both directions, corrected)

| Edge | Dir | Status / disposition |
|---|---|---|
| `* -> Rasm.Element/Projection` | out | `[CONTRACT]` future `FabricationProjector:IElementProjection` (queued, one row) — HOLD |
| `Documentation/projection <- Rasm/Drawing/view` `View.Apply` | in | `[V3]` kernel `DrawingProjection` consumer; **drop "beside the in-folder BSP solver"** — the BSP dies. Signature-locked `View.Apply(ViewOp,Op?)->Fin<DrawingProjection>` |
| `Documentation/projection <- Rasm/Meshing/arrangement` | in | `Arrangement.Apply`/`ToMesh` watertight `BooleanSolid` arm — HOLD |
| `Documentation/projection <- Rasm/Meshing/arrangement` `BooleanOp` | in | **re-point `ARCH:44` from `Processing/repair` -> `Meshing/arrangement`** (kernel `[V5]` re-home) |
| `Documentation/projection <- Rasm/Spatial/index` | in | BVH occluder prune — HOLD |
| `Documentation/projection -> Rasm.AppUi/Render` | out | `HiddenLineResult` receipt; supersession insulated AT the receipt (UIBRIEF `[V7]`(b), corrected from the brief's `[V6]`) — HOLD, seam declared BOTH sides |
| `Additive/slicing -> Rasm/Meshing/intersect` `Slicing.Apply` | in | **`ARCH:48` gate FIRED** — `Section` re-routes to the kernel slice-stack; drop "when realized" |
| `Toolpath/skeleton <- Rasm/Meshing/{skeleton,offset}` | in | NEW — `Skeletonize.Apply`+`Offsetting.Apply` clearance-radius family ([V10]d) |
| `Geometry2D/arcs <- Rasm/Meshing/offset` `Offsetting.Apply` | in | NEW — kernel corner-row `JoinType`×`EndType` offset assembly ([V10]a); Fabrication kerf ON TOP |
| `Toolpath/surface <- Rasm/Processing/{geodesics,extract,flow,segment}` | in | NEW — on-mesh path LAYOUT (OCL owns positioning) |
| `Toolpath/surface <- OpenCAMLib` (C-shim) | in | NEW — drop/push/waterline sampling; fallback = kernel `[V8]` SDF |
| `Toolpath/motion -> Rasm/Drawing/pack` `PackKind.toolpath` | out | NEW — committed motion residency channel-set alignment (`GBRIEF [V13]`); ledger row, NOT a Fabrication encoder |
| `Additive/implicit <-> Rasm/Meshing/mesh` `MeshSpace` | both | NEW — content-keyed mesh<->`Voxels` wire (ONE ALC-firebreak posture) |
| `Ingress/solid <- Rasm/Meshing/mesh`+`Processing/repair` | in | NEW — `MeshSpace` admission + `HealOp` dirty-STL heal |
| `Verify/probing <- Rasm/Processing/register` ICP | in | NEW — `AlignKind` datum best-fit |
| `Verify/probing <- Rasm/Analysis/measure` `ConformanceMetric` | in | NEW — measured-vs-nominal |
| `Verify/probing <- Rasm.AppHost` MTConnect decoded | in | NEW — measured-feature ingress (`APPHOST [04]` names probing the 2nd Fabrication consumer) — HOLD |
| `Tooling/magazine <- Rasm.AppHost` MTConnect decoded | in | tool-life reload (`APPHOST [04]` 1st consumer) — HOLD |
| `Spec/capability <- Rasm/Domain/stats` | in | NEW — `Stat.Of`/`Distribution.Of`/`StatContext.Tolerance` streaming moments |
| `Spec/capability <- MathNet.Numerics` | in | NEW — distribution-fit + Monte-Carlo stackup |
| `Spec/tolerance -> py drawing plane` | out | NEW — GD&T VALUE supplier (`[06]` artifacts seam; Fabrication owns vocab, py draws) |
| `Process/physics <- Rasm.Materials/Properties` | in | raw-double AEC-peer boundary — HOLD (reconcile `ARCH:49` label vs `physics.md:3` anchor drift) |
| `Nesting/stock <- Rasm.Element/MaterialId` | in | **ADD** wired-undeclared row (`stock.md:40,83,99`; csproj ProjectReference) |
| `Nesting/stock -> Rasm.Compute` `WasteAreaMm2` | out | **ONE-SIDED forward seam** — CBRIEF `[V12]` is NOT the WasteAreaMm2 waterfall (REFUTED, upstream dossier §D-2); Compute side un-landed; record as forward demand, not "waterfall landed" |
| `Nesting/nfp -> Rasm.Persistence` durable rows | out | **re-point `ARCH:54` from `Rasm.Persistence/Schema` (DELETED)** -> BLOCKED cross-package DEMAND on Persistence `[FABRICATION_PROGRAM_DURABLE_ROWS]` (PBRIEF:221); NOT a composed `ArtifactKind` binding (REFUTED §D-2); mint key via `ContentHash.Of` |
| `Posting/program -> Rasm.Persistence` durable rows | out | **re-point `ARCH:53`** same as above; `CutProgram` content key minted via `ContentHash.Of`, `ArtifactKind` fold = Fabrication-authored demand + seam residual |
| `Ingress/profile <- Rasm.Bim/Exchange` | in | ACadSharp read seam; **re-point `ARCH:52` netDxf write-leg parenthetical -> AppUi ACadSharp `DxfWriter`+`DwgWriter` two-format leg** (netDxf removed repo-wide) |
| `Nesting/nfp <- Rasm/Processing/flatten` `ChartAtlas` | in | unrolled UV islands — HOLD |
| `Toolpath/guard <- Rasm/Spatial/index` | in | BVH keep-out prune — HOLD |
| `* -> Rasm` `Matrix`/`Point3d`/`Vector3d` | out | kernel vocabulary — HOLD |

### Intra-package web (declare all — E11 gap closes)

`owner#run -> {projection,motion,nfp,slicing,production,removal,probing,dialect,traveler}.Solve` (terminal). `owner#atoms -> {every page}` (atoms). `family -> {physics,magazine,cuttingdata,motion,surface,dialect,cell,machine,workholding}`. `magazine -> {guard,workholding}` (HolderEnvelope) + `-> motion` (Schedule in Cam fold) + `-> traveler`. `guard <- skeleton` (ClearanceAt) + `<- workholding` (ExclusionZone) + `-> motion` (per-move). `workholding -> {motion(Condition),program(Sequence),setups,guard,traveler}`. `setups -> {program(WCS),guard,probing,traveler}`. `motion -> {skeleton,surface,partition,cell}` (composes) + `-> program`. `removal -> {owner#atoms(residual/snapshot),capability}`. `probing -> {capability,setups,traveler}`. `program -> {dialect,traveler}`. `dialect <- steel` (NC1). `capability -> {owner#atoms(gate),traveler}`. `tolerance -> {surface,motion,traveler}`.

---

## [06]-[ROSTER_DELTA] — 24 dispositions, .api obligations

Central ownership `Directory.Packages.props` (hand-edited, label-grouped) + `Rasm.Fabrication.csproj`; 14 folder-tier `.api` + shared `libs/csharp/.api/`. INTEGRATION-FIRST: the four zero-consumer admissions are MANDATES realized as pages, never removal candidates.

| # | Package | Disposition | .api obligation |
|---|---|---|---|
| 1 | `OpenCAMLib` | **ADD** (probe FEASIBLE) LGPL-2.1, C-shim `extern "C"`+`[LibraryImport]` P/Invoke; RID osx-arm64/win-x64/linux-x64 (shipped SHARED `libocl` archives OR from-source per-RID via forge-scientific-env); libomp carriage (osx bundle rpaths `libomp.dylib`; linux `libgomp`; win `vcomp`; or `USE_OPENMP=OFF` build-policy row); golden-fixture gated (drop-cutter/waterline per cutter form); content-keyed wire. Home `Toolpath/surface` | NEW `api-opencamlib.md`: `Operation` lifecycle, cutter-form ctors (Cyl/Ball/Bull/Cone/Composite -> `CutterForm`), shim ABI (~18-24 fns, blittable), RID/libomp map, LGPL dynamic-link note |
| 2 | `lib3mf` | **ADD** BSD-2, vendored official C# binding + RID native asset; golden-fixture gated; 3MF core+production+beam-lattice writer (PicoGK `Lattice`->beam-lattice); STL-implied hand-off dies. Home `Additive/production` | NEW `api-lib3mf.md`: writer surface, beam-lattice mapping, RID asset, vendored-binding mechanics |
| 3 | `RectpackSharp` | **REMOVE** — strict subset of RectangleBinPack.CSharp (`PackingHints.FindBest` reproduces as one `MaxRectsBinPack` heuristic sweep); sole consumer (nfp fast-path) re-homes; `README:65` rationale rewrites | `api-rectpacksharp.md` content re-homes to the nfp MaxRects fast-path over RBP |
| 4 | `netDxf` | **VERIFY-REMOVE** (`PROPS:357`) — leaves with AppUi `[V7]` motion; this campaign removes only a surviving orphan | remove; `import.md:20`/`README:33,64`/`ARCH:52` netDxf frames re-write to the ACadSharp two-format leg |
| 5 | `netDxf.netstandard` | **VERIFY-REMOVE** (`PROPS:468` — brief's `:472` is DRIFT) — leaves with Compute `[V8]` FEALiTE2D.Plotting; surviving under a parity-kept FEALiTE2D stays a transitive floor, never Fabrication residue | none (transitive) |
| 6 | `geometry3Sharp` | **REPLACE-candidate** — abandoned (feed 2019-03-07); arc rail retires the refit for arc-sourced paths; owned Bolton biarc fold lands on `program` for line-sourced kernel mesh sections; leaves when both land (`Rasm.Bim.csproj:22`+`CSPROJ:17` remaining consumers, central pin leaves with Bim's mesh-text-importer drop — cross-package residual, recorded not executed) | `api-geometry3sharp.md:106` "dropped CavalierContours/one-Clipper2" self-contradiction DIES; `api-picogk.md:5,113,154` geometry3Sharp-permanent-owner coupling reframes to the owned biarc + kernel mesh vocab |
| 7 | `CavalierContours` | **KEEP** at the ONE `[V10]a`-ruled Fabrication stratum; re-group `PROPS:70` Kernel-Geometry->Fabrication label (verify landed; execute if geometry pass hasn't run) | `api-cavaliercontours.md` HOLD; **the `clipper.md:221 result.Positive` phantom -> `result.PosPlines[].Pline`** is the one leg-1 `assay api` member check |
| 8 | `Clipper2` | **KEEP** line-space owner; re-group `PROPS:71`->Fabrication label | `api-clipper2.md` clean — HOLD |
| 9 | `SharpVoronoiLib` | **KEEP** — `[V1]` `Toolpath/partition` mandate realization; re-group `PROPS:75`->Fabrication label; `Aliases="Voronoi"` extern alias | `api-sharpvoronoilib.md:3,18` **dual-ownership prose SLIMS to Fabrication-sole** (kernel csproj has no reference) |
| 10 | `RectangleBinPack.CSharp` | **KEEP** the ONE rectangle engine post-removal; re-group `PROPS:165` vacated-Materials->Fabrication label | `api-rectanglebinpack-csharp.md` HOLD |
| 11 | `PicoGK` | **KEEP+WIRE** — 3 lanes: `Additive/implicit` (TPMS/gyroid/`.cli`), `Additive/implicit` (Lattice supports), `Verify/removal` (`BoolSubtract` NC-verify); ALC-firebreak/sidecar | `api-picogk.md` reframe geometry3Sharp coupling (see #6); ALC posture HOLD |
| 12 | `OcctNet.Wrapper` | **KEEP+WIRE** — `Ingress/solid` STEP/IGES/STL->`OcctMesh`; SCOPE_LIMIT (single-shape, `libTKHLR`/`libTKXCAF` unbound) a standing demand row | `api-occtnet-wrapper.md:137` `Rasm.Persistence/Schema` re-point -> content-keyed artifact index (BLOCKED demand) |
| 13 | `DSTV.Net` | **KEEP+WIRE** — `Ingress/steel` READ + `Posting/dialect` NC1 emit (shared record tree); `KA` bend rows seed sheet-metal IDEAS | `api-dstv-net.md:3,126` band-2500->2700; `:132,143` netDxf purge |
| 14 | `MTConnect.NET-Common` | **KEEP** model-only; `GenerateHash` reconciles onto `ContentHash.Of` (never a 2nd mint); transport half is AppHost livewire's | `api-mtconnect-net-common.md:3,116` `GenerateHash`/`ContentHash.Of` reconcile note; `:119` `Persistence/Schema` re-point |
| 15 | `ACadSharp` | **KEEP** read-only ingress; strict `NotificationType.Error` escalation wires the dead `warnings` accumulator; write stays AppUi's | `api-acadsharp.md:120` stale Rhino-write-leg attribution -> AppUi ACadSharp leg |
| 16 | `Robots` | **KEEP** cell owner; `extern alias R3`; names the shared motion-dynamics law with posting | `api-robots.md:5,94` band-2500->2700 |
| 17 | `System.IO.Hashing` | **KEEP** reached THROUGH `ContentHash.Of` (never a direct 2nd call site) | `api-hashing.md:3,88` **false "no shared tier" law DIES**; slims to a thin nesting-identity overlay (the `api-unitsnet.md` pattern) naming `ContentHash.Of` as the one mint |
| 18 | `UnitsNet` | **KEEP** cut-parameter boundary; overlay EXTENDS Force/Power/Temperature/Angle/Torque | `api-unitsnet.md` overlay extension |
| 19 | `MathNet.Numerics` | **MINE** (first Fabrication consumer) — `Spec/capability` distribution-fit + Monte-Carlo; central pin rides the Geometry `[ROSTER_RECONCILIATION]` 5.0.0 downgrade (currently beta2, NOT landed) | shared-tier catalog (verify) |
| 20 | `Riok.Mapperly` | **MINE** — generated boundary mappers: R3 Robots seam, MTConnect decoded rows, DSTV-record->`Loop`, DXF-entity->`Loop` (hand-written copyists die) | shared-tier (verify) |
| 21 | `System.Numerics.Tensors` | **MINE** — `TensorPrimitives` SIMD-lowers hot sampling folds (engagement fields, drop-cutter/scallop grids where the OCL probe fails over, NFP sweep, capability batch stats) | shared-tier (verify) |
| 22 | `CommunityToolkit.HighPerformance` | **MINE** — `Span2D`/`Memory2D` carries 2D grids (grayscale layers, uncut/overcut maps, engagement rasters); `HashCode<T>.Combine` rejection holds | shared-tier (verify) |
| 23 | `NodaTime` | **MINE** — `Instant` stamps traveler/probing/tool-life/SPC receipts | shared-tier (verify) |
| 24 | `QuikGraph` | **MINE** — `Fixturing/setups` operation-precedence/datum-lineage graph (the NP-hard sequencing half; magazine eviction greedy stays optimal for a fixed sequence) | shared-tier (verify) |

Float-lane re-group (#7,8,9) + Materials re-group (#10) = the geometry wave-1 manifest motion (`GBRIEF:210`), verified landed at leg 1, executed here only if the geometry pass has not run. REFERENCE records (never vendored): gsGCode (Parse prior art), gsSlicer (FFF enrichment), MillSimSharp (voxel-verify corroboration), Tweaker-3 (orientation), FreeCAD machinability (`kc` schema) + FreeCAD_SheetMetal (K-factor), PySLM (Powder hatch), QIF/ISO 23952 (Spec authority), NIST RS274NGC v3 (dialect grammar), UVtools (resin format breadth). Probe correction: the upstream Python binding is Boost.Python, not pybind11 (immaterial to shim feasibility).

---

## [07]-[VERDICT_DISPOSITION] — V1-V10

| V | Disposition (output-first) |
|---|---|
| V1 folder partition | 13 folders / 33 pages ratified (`[02]`/`[03]`); the backward trace lands every flagship on this decomposition with zero orphan; floor CONFIRMED, not exceeded. Two-node `owner` passes the acyclicity gate |
| V2 kernel consumption | (a) skeleton -> `Skeletonize.Apply` clearance family, `Skeleton.cs` dies unconditionally (WALK kept); (b) slicing -> `Slicing.Apply` (`Section`/`Chain` die); (c) projection -> `View.Apply` (BSP dies, `BooleanSolid`+`HiddenLineResult` kept); (d) arcs = kerf ON TOP of kernel corner-row `Offsetting.Apply`; (e) booleans/SDF kernel-consumed. All 5 `[RESEARCH]` tails purge (`rg '\[RESEARCH\]'`==0: kinematics/skeleton/slicing + **program.md:379**+**stock.md:317**) |
| V3 execution wire | `Run->Cam->{Guard.Check per move, Workholding.Condition, Magazine.Schedule}->neutral CutProgram` executes; arc rail realizes (kerf/lead/adaptive read `arcs`); `Loop.Bulges` ratified on `owner#atoms`; Fixturing safety (segment-exact keep-out, per-kind footprints, total `ForHolding`); work-offset chain: `setups` OWNS WCS rows, `dialect` renders, `probing` reconciles |
| V4 post grammar | `program`(AST+conditioning+`Parse`) / `dialect`(generated grammar-family lowering) split forced by re-post; grammar columns on `family`; canned/macro/subprogram AST nodes; NC1 emit target; byte-diversity across 5 targets = the acceptance test; NIST RS274NGC v3 the grammar authority |
| V5 CAM depth | `(RemovalModality,CutStrategy)` + dimensionality column; real generators (Turn ZX sweep, helical distinct, Peck cycle, SliceWalk orderer); waterline/scallop/pencil/flowline/REST/3+2/swarf/thread-mill/drill rows; **OpenCAMLib ADMITTED** (probe FEASIBLE) on `surface` for positioning + kernel geodesics/extract/flow/segment for layout; `partition` Voronoi; `machine` 5-axis topology+TCP; `CutterForm` axis (4 consumers) |
| V6 additive production | `Additive/` folder: `slicing` (kernel slice-stack consumer), `implicit` (PicoGK TPMS/lattice/`.cli`), `production` (orientation+profiles+lib3mf 3MF); `physics` `Resin`/`Powder`/DED budgets; gyroid 2D-hatch collapse corrected to the voxel lane |
| V7 verify plane | `Verify/removal` (voxel gouge/uncut/overcut/air-cut + residual + per-setup snapshot) + `Verify/probing` (`[PROBING]` realized: G31/G38 cycles, ICP datum, ConformanceMetric, AppHost-decoded ingress); `FabricationPolicy.Verify`/`Inspect` cases; the run-N->run-N+1 loop is input-carried |
| V8 spec plane | `Spec/tolerance` (ISO 286/1101/1302+ASME Y14.5, QIF-aligned, DRIVES process) + `Spec/capability` (Cp/Cpk over kernel `Stat`/`Distribution` + MathNet, plan-time gate); `Documentation/traveler` composes; UnitsNet overlay extends |
| V9 physics identity | ONE `Material` identity + `Map<RemovalModality,ModalityPhysics>`; `Erosion`/`Resin`/`Powder` budgets; `CuttingData`->`Tooling/cuttingdata` Kienzle `kc`; dead `Overrides` deletes; `0.0012`/`90.0` die |
| V10 governance truth | R6 25xx purge (all 5 sites); netDxf purge (VERIFY-REMOVE per owner); `ARCH:63` scheduler -> `Fixturing/setups`; `[GUARD]-[QUEUED]` closes COMPLETE; `api-geometry3sharp:106`/`api-hashing:3,88`/`api-sharpvoronoilib:3,18` self-contradictions die; `nfp` `Stock.Of` identity + `Ty*1e6+Tx`->tuple + NFP frame proven |

---

## [08]-[EVIDENCE_DISPOSITION] — E1-E14

| E | Disposition |
|---|---|
| E1 dead admissions | RESOLVED — all four wired: PicoGK (`implicit`+`removal`), OcctNet (`solid`), DSTV (`steel`+`dialect` NC1), SharpVoronoiLib (`partition`); README charters realized on the named pages |
| E2 unwired pipeline | RESOLVED — `Guard.Check`/`Workholding.Condition`/`Magazine.Schedule` all EXECUTE in the Cam fold; `ARCH:63` scheduler realized as `Fixturing/setups` |
| E3 arc-rail fiction | RESOLVED — `Loop.Bulges`/`BulgeAt`/3-arg ctor land on `owner#atoms`; `clipper.md:221 result.Positive`->`PosPlines[].Pline` (assay leg 1); the `lnContours` sub-claim was already REFUTED on disk (no change); arc rail wired downstream; `g3.BiArcFit2` line-sourced-only |
| E4 post thinness | RESOLVED — grammar-family `dialect` generator; canned/macro/subprogram/WCS/comp/arc-mode nodes; `Dwell`/`Pierce` collapse to command+flag |
| E5 stub generators | RESOLVED — real `(RemovalModality,CutStrategy)` arms (Turn/helical/Peck/SliceWalk); `BarStock` -> real revolved envelope via kernel `Bounds` |
| E6 physics identity | RESOLVED — per-modality map; Erosion/Resin/Powder; `Overrides` deletes; `90.0`/`0.0012` die |
| E7 HLR not CAD-grade | RESOLVED — BSP dies for kernel `View.Apply`; `BooleanSolid`+`HiddenLineResult` kept; phantom `Orient2D` import dropped |
| E8 coverage silences | RESOLVED — waterline/scallop/rest/NC-verify/specs/G-code-read/additive-supports/5-axis all homed; `CutterForm` axis + `Cam.ToolRadius`->form-row |
| E9 identity/logic | RESOLVED — `Stock.Of` full-dimension hash via `ContentHash.Of`; `Ty*1e6+Tx`->tuple; NFP frame proven; workholding segment-exact + total `ForHolding` + per-kind footprints |
| E10 stale governance | RESOLVED — 25xx/netDxf/Persistence-Schema/scheduler all re-point; `PROPS:357`/`:468` (brief `:472` DRIFT) VERIFY-REMOVE |
| E11 ledger gaps | RESOLVED — `stock<-MaterialId` added; full intra-web declared; `physics.md:3` vs `ARCH:49` reconciled; `ARCH:51` recorded as one-sided forward seam (NOT waterfall-landed — REFUTED) |
| E12 redundancy/overlay | RESOLVED — RectpackSharp REMOVE; `api-hashing` slims; `README:65` rewrites; `Runout` reads the real measurement |
| E13 page-craft | RESOLVED — 5 `[RESEARCH]` tails purge (anchor set corrected +2: program.md:379, stock.md:317); false "no managed library" seals die |
| E14 upstream bindings | CORRECTED — signature-lock against the LANDED geometry DECISION `FAB:NN` anchors (not the drifted +14..+16 GBRIEF line pins); `spelling-lock RESOLVED`; **PBRIEF `[V12]`/CBRIEF `[V12]` bindings REFUTED** -> demoted to forward/BLOCKED demands; UIBRIEF `[V6]`->`[V7]`(b) |

---

## [09]-[ESCALATION_DELTA] — [03] targets

Every `[03]` capability-escalation delta is homed on a page above: Process axes 8->9.5 (`owner`/`family`/`faults`); Physics+data 6->9 (`physics`+`cuttingdata`); Tooling 8->9.5 (`magazine`+`cuttingdata`); Geometry2D 8->9.5 (`algebra`/`arcs`); Ingress 5->9.5 (`profile`/`solid`/`steel`); Toolpath 5->9.5 (`motion`/`surface`/`partition`/`guard`/`skeleton`); Kinematics 7->9.5 (`cell`/`machine`); Additive 4->9.5 (`slicing`/`implicit`/`production`); Nesting 8.5->9.5 (`nfp`/`stock`); Fixturing 6->9.5 (`workholding`/`setups`); Posting 6->9.5 (`program`/`dialect`); Verify —->9.5 (`removal`/`probing`); Spec —->9.5 (`tolerance`/`capability`); Documentation 6->9.5 (`projection`/`traveler`). No delta is unhomed.

---

## [10]-[CARD_DISPOSITION] — TASKLOG + IDEAS (23 cards)

### TASKLOG (16)

| Card | Disposition |
|---|---|
| `[NFP_DRL_POLICY]`-BLOCKED | STAYS BLOCKED — injected-delegate column is the strata-correct shape; closes when Compute lands its side |
| `[CSG_WATERTIGHT_SILHOUETTE]`-COMPLETE | PARTIAL supersede — `BooleanSolid` watertight arm KEPT; BSP interior dies for kernel `View.Apply` |
| `[TOOLPATH_STRATEGY_MODALITY_FACTOR]`-COMPLETE | EXTENDED — cross-product stays; arms become real generators + dimensionality column |
| `[POST_DIALECT_OVERRIDE_SEAM]`-COMPLETE | HELD — the override-resolution fold survives into `dialect` |
| `[TROCHOIDAL_ENGAGEMENT_LIMIT]`-COMPLETE | PARTIAL — trochoidal WALK KEPT; `skeleton` re-homes onto the kernel medial |
| `[NEST_MULTISHEET_SCHEDULE]`-COMPLETE | HELD — `Nest.Solve` multi-sheet fold survives |
| `[NEST_REMNANT_LINEAGE]`-COMPLETE | HELD + fix — `Remnant` lineage kept; the poisoning `Stock.Of` area-only hash fixed (E9) |
| `[RECTPACK_FASTPATH_ADMISSION]`-COMPLETE | SUPERSEDED — RectpackSharp REMOVE; fast-path re-homes to `MaxRectsBinPack` |
| `[BIARC_ARC_EMISSION_ADMISSION]`-COMPLETE | PARTIAL supersede — refit retires for arc-sourced; line-sourced kept until owned Bolton fold lands |
| `[ACADSHARP_SPLINE_INSERT]`-COMPLETE | HELD — moves with `import`->`Ingress/profile` |
| `[CLIPPER_INNER_FIT]`-COMPLETE | HELD — MinkowskiDiff IFP survives into `algebra`/`nfp` |
| `[FABRICATION_FAULT_BAND_DEEPENING]`-COMPLETE | STALE-PURGE — the `TASKLOG:44` 2505-2509 codes in this COMPLETE card re-point to 2700 (R6) |
| `[TOOL_CUTTING_DATA_TABLE]`-COMPLETE | ABSORBED — `CuttingData` moves to `Tooling/cuttingdata`, deepens to Kienzle `kc`; `90.0` dies |
| `[CLIPPER_VARIABLE_KERF]`-COMPLETE | HELD — `DeltaCallback64` variable offset survives into `algebra` |
| `[FAULT_REBAND_2700]`-COMPLETE | HELD — the 2700 band is the frozen foundation; typed payloads + new arms build ON |
| `[STOCK_NEST_MOVE]`-COMPLETE | HELD — `stock` engine kept; `MaterialId` ledger row added; WasteAreaMm2 recorded as one-sided forward seam |

### IDEAS (7)

| Card | Disposition |
|---|---|
| `[DRL_NEST_POLICY]`-BLOCKED | STAYS BLOCKED (pairs `[NFP_DRL_POLICY]`) |
| `[GUARD]`-QUEUED | CLOSES COMPLETE — `guard` landed; `IDEAS:27`/`ARCH:63`/README close in-motion |
| `[PROBING]`-QUEUED | REALIZES -> `Verify/probing` (`[V7]`) |
| `[MULTI_FIXTURE_SCHEDULE]`-QUEUED | REALIZES -> `Fixturing/setups` (`[V1]`) |
| `[CSG_SILHOUETTE]`-COMPLETE | HELD — the kernel-arrangement compose target |
| `[MAGAZINE]`-COMPLETE | HELD + supersede — `G43`/`M6`/`Tnn` claim MADE REAL (wired in Cam fold, was E2 fiction) |
| `[FEEDRATE_LOOKAHEAD]`-COMPLETE | HELD — jerk-limited planner survives; reads the shared `machine` dynamics law |

**New IDEAS seeds (growth rows, not build-leg pages):** sheet-metal bend/unfold (DSTV `KA` decode lands with `Ingress/steel`; K-factor unfold over kernel `develop.md` isometric unroll, `Spec`-owned bend-allowance table); 5-axis simultaneous swarf/flank refinement; conversational-dialect breadth beyond the seed rows; the `FabricationProjector:IElementProjection` row stays queued.

---

## [11]-[DRY_RUN_CLOSURE] — the four acceptances compose from rebuilt fences

- **(a) toolpath dry-run** (`GBRIEF:226(b)` Fabrication side): corner-strategy offsets (`arcs`<-`Offsetting.Apply`), oriented slice stack (`slicing`<-`Slicing.Apply`), medial clearance-radius read (`skeleton`<-`Skeletonize.Apply`), distance-field read (`surface` fallback<-`[V8]`), stock-envelope hull (`stock`<-`[V11]`) — ZERO Fabrication-side geometry re-implementation.
- **(b) five-target posting**: `Run(Cam)`->neutral content-keyed `CutProgram`; `Run(Post{motion,dialect})` ×5 (fanuc/haas/grbl/klartext/marlin) byte-DIVERSE, canned cycles as single blocks where admitted + NC1; `Parse` round-trip on the word-address subset; `CutProgram` content key minted.
- **(c) additive**: STEP solid in (`Ingress/solid`); orientation scored (`production`); lattice supports + TPMS infill (`implicit`); FFF program (`slicing`->`Post`) AND resin `.cli` stack (`implicit`) AND 3MF (`production`) out, all three content-keyed.
- **(d) production-truth**: two-setup multi-tool program voxel-verified (`removal` gouge/uncut); op-2 clamp admitted against the op-1 snapshot (`setups`<-`StockSnapshot`); work offsets setups-assigned (`setups`->`dialect`) + probe-reconciled (`probing`); rest-machining reads the residual field (`motion`); probing feeds a Cp/Cpk row gating the next plan (`capability`->`owner#atoms`); one setup sheet + traveler (`traveler`).

A dry-run reaching a missing owner reopens the owning verdict as a residual; the 33-page set closes all four with no gap.

---

## [12]-[COUNTS]

- Lens: **output-first**
- Folders: **13** (Process, Tooling, Geometry2D, Ingress, Toolpath, Kinematics, Additive, Nesting, Fixturing, Posting, Verify, Spec, Documentation)
- Page rows: **33** (Process 4, Tooling 2, Geometry2D 2, Ingress 3, Toolpath 5, Kinematics 2, Additive 3, Nesting 2, Fixturing 2, Posting 2, Verify 2, Spec 2, Documentation 2)
- Actions: `improve` 3 (owner, guard, stock), `rebuild` 7 (family, physics, faults, motion, skeleton, nfp, program), `new` 23 (6 moved-destinations: magazine/profile/cell/slicing/workholding/projection + 2 split: algebra/arcs + 15 net-new); `del` 7 / `absorb` 8
- Verdicts disposed: **10** (V1-V10)
- Evidence disposed: **14** (E1-E14)
- Cards disposed: **23** (TASKLOG 16 + IDEAS 7); + 4 new IDEAS growth seeds
- Packages disposed: **24** (2 ADD, 1 REMOVE, 2 VERIFY-REMOVE, 1 REPLACE-candidate, 4 re-group, 8 KEEP/WIRE-or-reconcile, 6 shared-tier MINE)
- Namespace ratifications: 13 folders = 13 namespaces (1:1); the `Process`/`ProcessKind` collision fix
- Kernel seam members signature-locked: 8 (`Offsetting.Apply`, `Skeletonize.Apply`, `Slicing.Apply`, `Intersection.Apply`, `Arrangement.Apply`+`BooleanOp`, `View.Apply`, `Encode.Apply` `PackKind.toolpath`, `ContentHash.Of`)
