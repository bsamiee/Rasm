# [DRAFT] — RASM-CS-FABRICATION blueprint, lens = KERNEL-SEAM-FIRST

LENS: the structure is derived from the kernel-consumption map. Every page that reads a kernel plane earns its seam row and signature-lock FIRST, against the LANDED `RASM-CS-GEOMETRY-DECISION.md` member spellings (not the drifted brief line-pins); the folder partition then falls out of the consumption dependency, and the wave order is the topological sort of that DAG. The five owner→consumer re-framings ([V2]a-e) and the ruled kernel hinges ([V10]a kerf split, [V10]d medial-radius carriage) are carried verbatim and signature-locked, never re-opened.

THESIS: `Rasm.Fabrication` is a production digital-fabrication engine whose spine is what it CONSUMES from the kernel, not what it re-implements. Nine kernel members (`Offsetting.Apply`, `Skeletonize.Apply`, `Slicing.Apply`, `Intersection.Apply`, `Arrangement.Apply`, `View.Apply`, `Encode.Apply`, `SignedDistanceFromMesh`, `ContentHash.Of`) plus four on-mesh algorithm suites are the load-bearing seams; the 33-page/13-folder namespace-true partition is the minimal structure that homes every consumer to its fault-cluster and lands every seam acyclically across four waves. Structure first — dead admissions become pages, pipeline islands wire into one fold, the arc rail realizes, and the two REFUTED upstream `[V12]` reciprocals demote to Fabrication-authored forward demands minted through the one LANDED `ContentHash.Of` entry.

---

## [00]-[KERNEL-SEAM SIGNATURE-LOCK] — the spine (leg-1 re-verify against the DECISION)

Every consumed kernel member, its EXACT landed signature (source: `RASM-CS-GEOMETRY-DECISION.md` [02] page rows + [04] seam ledger rows 123-127 + [08] Fabrication gate + [05] V10 line 149; cross-verified against the landed page fences per `GBRIEF [LANDED_KERNEL_LAW]` — kernel is pre-realization `.planning`, no restored assembly, so member truth READS the DECISION rows). Fabrication pages signature-lock these at their leg-1 pass and NEVER re-derive the interior.

| # | Kernel member (locked signature) | Namespace · fault | Consuming Fabrication page(s) | Ruled disposition carried | Leg |
|---|---|---|---|---|---|
| K1 | `Fin<OffsetResult> Offsetting.Apply(OffsetOp, Op? key = null)` — cases `Skeleton · Offset(JoinType×EndType corner rows) · Medial · Minkowski · Weighted · Clearance(probe)`; per-point clearance **RADIUS** first-class result field + `Clearance(Point3d probe)` arbitrary-probe query | `Rasm.Geometry.Offsetting` · 2416-2419 (row 10) | `Geometry2D/arcs` (corner-row region offsets), `Toolpath/skeleton` (Medial + RADIUS), `Toolpath/motion` (engagement clearance) | **[V10]a kerf split** — kernel owns exact corner-row assembly + medial; Fabrication owns pure kerf compensation in arc space over CavalierContours ON TOP (`README:99`, one concern per stratum) | 2 |
| K2 | `Fin<CurveSkeleton> Skeletonize.Apply(SkeletonOp, Op? key = null)` — COMPOSES K1's clearance family (same RADIUS + `Clearance(Point3d probe)`, ONE result family across 2D/3D) | `Rasm.Geometry.Offsetting` · `SkeletonStalled 2417`/`CollapseStalled 2418` (row 18) | `Toolpath/skeleton` (3D MCF curve-skeleton) | **[V10]d + [V2]a** — `Toolpath/Skeleton.cs` DIES UNCONDITIONALLY for `Offsetting.Apply`; page keeps ONLY the trochoidal constant-engagement WALK over the kernel clearance field (`FAB:22`) | 2 |
| K3 | `Fin<SliceStack> Slicing.Apply(SliceOp, Op? key = null)` — ALL FOUR layer policies OPEN (uniform · adaptive · variable-by-slope · support-interface); oriented contours (outer CCW / holes CW); typed OPEN-chain rows; **5-channel SoA forest wire** (layers · contours · nesting parent/child index arrays · open chains · elevations) | `Rasm.Geometry.Intersection` · `SectionFault 2425` (row 17) | `Additive/slicing` | **[V2]b** — `Toolpath/slicing.Section` + O(n²) `Chain` DIE; `FAB:23/48` "when realized" promise REALIZED; adaptive/support-interface layer rows are kernel slice-stack policy seeds landed on this lane's named demand, never a Fabrication layer-height engine | 3 |
| K4 | `Fin<IntersectResult> Intersection.Apply(IntersectOp, Op? key = null)` — `IntersectOp.PlaneMesh` Section fold; oriented `Chain`; `IntersectResult` widened beyond loops-only | `Rasm.Geometry.Intersection` · 2424 (row 8) | `Additive/slicing` (section primitive under K3) | consumed under K3; `FAB:48` | 3 |
| K5 | `Fin<ArrangementResult> Arrangement.Apply(ArrangementOp, Op? key = null)` + `BooleanOp` smart enum — `MeshBoolean · PlanarOverlay · CellComplex`; Manifold tier-3 behind `ArrangementPolicy.ScaleCeiling = 1_000_000` | `Rasm.Geometry.Arrangement` · 2420+2423 (row 9) | `Documentation/projection` (watertight `BooleanSolid` arm via `Arrangement.Apply(MeshBoolean)`/`ToMesh`) | **[V2]e / FAB:44** — `BooleanOp` anchor re-points `csharp:Rasm/Processing/repair` → `Meshing/arrangement`; `projection`'s `Rasm.Geometry.Healing` import re-points | 4 |
| K6 | `Fin<DrawingProjection> View.Apply(ViewOp, Op? key = null)` — `HiddenLine · Silhouette · Section · Creases`; exact analytic Appel QI (zero sampling) | `Rasm.Geometry.Projection` · 2436-2439 (row 27) | `Documentation/projection` | **[V2]c / [V3] / FAB:33+46** — the BSP interior DIES; "beside the in-folder BSP solver" clause dies; page PRESERVES the `HiddenLineResult` receipt shape AppUi is insulated at (`UIBRIEF [V7]`(b), `:162`) | 4 |
| K7 | `Fin<EncodedGeometry> Encode.Apply(PackOp, Op? key = null)` — `PackKind` gains **`toolpath`** case row + active-channel set | `Rasm.Geometry.Encoding` · 2444-2447 (row 28) | `Posting/program` (committed motion stream residency) | **[V13]** — the channel-set alignment lands as ONE ledger row (Compute decodes residency, AppHost locks `EncodingKind.Toolpath`); NEVER a Fabrication-side encoder | 4 |
| K8 | `SignedDistanceFromMesh` (landed `fields.md` → `reconstruct.md` GWN → `index.md` BVH winding, row 3) — the ONE distance-field lane | `Rasm.Geometry` (`Analysis/fields`) | `Toolpath/surface` (target-proximity / gouge field) | **[V8]** — the kernel mints no second SDF owner; PicoGK stays Fabrication's VOXEL lane (`Additive/implicit`, `Verify/removal`), never a second kernel SDF owner | 3 |
| K9 | `ContentHash.Of` — seed-zero federation entry (`XxHash128.HashToUInt128` reached THROUGH it) | `Rasm` (`Domain/identity`) | ALL egress: `program`, `nfp`, `implicit`, `production`, `removal`, `traveler`, `magazine`, `stock`, `nfp.Remnant` | **[SEAM_AND_ENTRY_LAW]** — the ONE mint site; every raw `XxHash128`/`GenerateHash` is the second-hasher defect; content keys minted here, the `ArtifactKind` egress fold carried as a Fabrication-AUTHORED demand (D-2: PBRIEF `[V12]` ArtifactKind is REFUTED/BLOCKED) | 1 atoms, per-plane |

On-mesh algorithm suites (composed at the seam, `[V5]`; a Fabrication-side re-implementation is the `[V2]` defect class):

| # | Kernel suite | Consumer | Use |
|---|---|---|---|
| K10 | `Processing/geodesics.md` heat distance (via `fields.md` `Geodesic` case) | `Toolpath/surface` | geodesic-parallel / constant-stepover path LAYOUT |
| K11 | `Processing/extract.md` `ContourPolicy.MeshScalar` isolines | `Toolpath/surface` | pass extraction |
| K12 | `Processing/flow.md` streamline over `VectorField` | `Toolpath/surface` | flowline / morph rows |
| K13 | `Processing/segment.md` cross-field / stripe | `Toolpath/surface` | flank / swarf direction alignment |
| K14 | `Analysis/measure.md` `Bounds` enclosing-cylinder + `Meshing/delaunay.md` `LowerHull` (row 7) + `cloud.md` rail | `Toolpath/motion` | `Turn` revolved stock envelope + clearing-stock hull ([V11] two-tier) |
| K15 | `Meshing/mesh.md` `MeshSpace` admission + `Processing/repair.md` `HealOp` (predicate-gated) | `Ingress/solid` | `OcctMesh` crosses to kernel vocabulary; dirty STL healed at the seam |
| K16 | `Processing/register.md` `AlignKind` ICP dispatcher (one `AlignmentPolicy`) | `Verify/probing` | probe-to-nominal datum best-fit |
| K17 | `Analysis/measure.md` `ConformanceMetric` over `ResidualSample` | `Verify/probing`, `Spec/capability` | measured-vs-nominal verdicts, pre-folded evidence |
| K18 | `Domain/stats.md` `Stat.Of` (Welford) / `Distribution.Of` (quantiles) / `StatContext.Tolerance` | `Spec/capability` | streaming-moment half of Cp/Cpk/SPC (MathNet owns the fit/Monte-Carlo half) |
| K19 | `Parametric/projections.md` `MotionInterpolation` (one-slerp owner) | `Kinematics/machine` | multi-axis orientation interpolation — never a second slerp site |
| K20 | `Analysis/select.md` `Faces` / `Curves.Draft` axis-ranked face decomposition | `Additive/production` | build-orientation overhang/bottom-area objective — never a hand-rolled normal classifier |

Substrate rails (kernel-owned mechanisms; `FabricationFault` stays the one Fabrication fault family — the substrate-mechanisms/own-faults split the robust core rides):

- K21 `Domain/rails.md` — entries thread the `Op` value key with `Eff<Env>` carriage; every Fabrication receipt registers into the `IValidityEvidence` + `ValidityClaim` fold reaching the `Domain/validation.md` `OpAcceptance` oracle with ZERO oracle edits (a hand-rolled per-receipt `IsValid` is the dual-paradigm defect).
- K22 `Domain/validation.md` — input guards compose the admission vocabulary + the one `OpAcceptance` oracle.
- K23 `Numerics/atoms.md` — typed output projection rides `AtomProjection`/`ProjectionRow`, never a `typeof(TOut)` switch.

---

## [00b]-[COLLISION RULING] — the CS0118 namespace/type hazard (DECISION must rule)

`owner.md:39` declares `namespace Rasm.Fabrication.Process`; `family.md:98` declares `public sealed partial class Process` (the `[SmartEnum<string>]`). The `[V1]` floor homes owner AND family both into `Process/` under namespace `Rasm.Fabrication.Process`, so the type resolves as `Rasm.Fabrication.Process.Process` — a latent `CS0118` ambiguity already live where `program.md` imports both the `Process` namespace and the `Process` type and references bare `Process`.

RULING: the `Process` `[SmartEnum<string>]` axis **RENAMES to `ProcessKind`** (the removal-process discriminant), matching the sibling axis-naming pattern (`Machine`/`RemovalModality`/`KinematicClass`/`HoldingClass`/`PostDialect`/`CutStrategy`). Folder=namespace `Process/` = `Rasm.Fabrication.Process` holds clean; every `input.Process`/`Resolve(…, Process)`/`(Process, Material, Tool, Operation)` reference re-spells to `ProcessKind`. Signature-lock item on `Process/family` (leg 1); a greenfield rename, no shim.

---

## [00c]-[UPSTREAM CORRECTIONS] — the two REFUTED `[V12]` reciprocals (D-2, load-bearing)

The brief treats two upstream bindings as landed law; disk falsifies both. The DECISION MUST carry the demotion or the egress plane composes a contract that does not exist.

1. **PBRIEF `[V12]` ArtifactKind egress index — REFUTED.** PBRIEF `[V12]` is `[V12_GOVERNANCE_RECONCILE]`; no `ArtifactKind`/`.cli`/`NC1`/`traveler`/`3MF`/`stock-state` vocabulary exists. The real home is the held-open BLOCKED card `PBRIEF:221 [FABRICATION_PROGRAM_DURABLE_ROWS]` ("2701-2710 decode constraint recorded"). DISPOSITION: Fabrication mints EVERY egress content key through the LANDED `ContentHash.Of` entry (K9) and carries the `ArtifactKind`/durable-row fold as a **Fabrication-AUTHORED demand + seam-ledger residual**, never a composed upstream contract. The `2701-2710` decode constraint is honored (receipts decode 2701-2710, never 25xx).
2. **CBRIEF `[V12]` WasteAreaMm2 rollup — REFUTED.** CBRIEF `[V12]` is `[V12_DISCIPLINE_COVERAGE]`; zero waste-rollup tokens. `ARCH:51` is a genuinely ONE-SIDED forward seam. DISPOSITION: `Nesting/stock → Rasm.Compute NestYield.WasteAreaMm2` is a Fabrication-authored FORWARD demand row, not a "waterfall landed" binding; the Compute counterpart stays un-named upstream until Compute lands its side.

Anchor discipline: signature-lock against the STABLE DECISION `FAB:NN` anchors (rows 123-127), NOT the drifted `GBRIEF` line-pins (systematic +14..+16). UIBRIEF HiddenLineSeam is `[V7]`(b), not `[V6]`. The `[RESEARCH]`-tail census is FIVE (`kinematics.md:122`, `skeleton.md:199`, `slicing.md:219`, `stock.md:317`, `program.md:379`), not three — `rg '\[RESEARCH\]'` under `.planning/` returns zero post-motion.

---

## [01]-[FOLDER / PAGE-SET] — 13 folders (= namespaces = fault-clusters, 1:1), 33 pages

Engine lowering per row: SPLIT → N `new` + one `delete{from,absorb→[…]}`; MERGE → one absorber `improve/rebuild` + `delete{from,absorb}`; MOVE → `new` at destination + `delete{from, absorb→destination}`; KEEP → `improve`; REBUILD → `rebuild`; NEW → `new`. Seam column = one anchor per seam, both directions (`←` consumes / `→` produces); `owner` evaluates as TWO nodes (`owner#atoms` upstream-only, `owner#run` terminal compose-only). Kernel seams cite the K# lock.

### `Process/` — ns `Rasm.Fabrication.Process` — charter: the entry, the axes, the physics, the rail

| Page | Action → lowering | Owner charter | Entry growth (Policy/Result case) | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Process/owner.md` | KEEP+grow → `improve` | The polymorphic `Fabrication.Run` fold over `FabricationInput`+`FabricationPolicy [Union]`; the atom vocabulary every plane reads | `FabricationPolicy` gains `Additive`/`Verify`/`Inspect` (join `HiddenLine`/`Cam`/`Nest`); `FabricationResult` gains `AdditiveResult`/`VerifyResult`/`InspectResult`; `owner#atoms` mints widened `Loop(Arr<Point3d>, bool, Arr<double> Bulges)`+3-arg ctor+`BulgeAt`, `ProjectionDir`, `ResidualStock`, per-setup `StockSnapshot`, `CutterForm` reference, the egress content-key seed | `owner#atoms →` every plane (atoms); `owner#run ←` {`projection`,`motion`,`nfp`,`slicing`,`removal`,`probing`} terminal; threads K21 `Op`/`Eff<Env>`, K9 content key | 1 |
| `Process/family.md` | REBUILD → `rebuild` | The de-hardcode axis pair; `[SmartEnum]` axes with constructor-bound behavior columns + generated total `Switch` | `Process`→**`ProcessKind`** rename (00b); `PostDialect` gains GRAMMAR-family capability columns (family/canned-cycle/macro-subprogram/WCS-roster/cutter-comp/arc-mode/block-cap/decimal-policy/per-`GCommand` code-override); `CutStrategy` gains a dimensionality column (2.5D\|3D-surface\|multi-axis) + axis-count admission; `KinematicClass` deepens to rotary-topology rows | `← faults` (GeometryFault); read BY {`owner`,`physics`,`motion`,`program`,`dialect`,`magazine`,`workholding`,`setups`,`cell`,`machine`} | 1 |
| `Process/physics.md` | REBUILD → `rebuild` | ONE `Material` identity carrying `Map<RemovalModality, ModalityPhysics>`; `Budget` modality-dispatches into the map | — (peer-boundary reader) | `← Rasm.Materials/Properties` (raw doubles, peer boundary); `→ cuttingdata` (Material identity key); `RemovalBudget` gains `Erosion`/`Resin`/`Powder`/`DED`; `stainless`+`stainless-abrasive` collapse; dead `Overrides` deletes | 1 |
| `Process/faults.md` | KEEP+payloads → `improve` | `FabricationFault [Union]` on `FaultBand.Fabrication = 2700`; ONE fault rail | — | `← Rasm.Element` (FaultBand registry, substrate); every arm retypes bare `string Detail` → typed payload (`Gouge`→point+tool, `Collision`→zone, `InadmissiblePair`→typed pair, `NoFit`→part+rotations); new arms per the fault registry [02] | 1 |

### `Tooling/` — ns `Rasm.Fabrication.Tooling` — charter: ISO-13399 tool intelligence

| Page | Action → lowering | Owner charter | Axis growth | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Tooling/magazine.md` | MOVE (`ProcessModel`→`Tooling`) → `new`@`Tooling/magazine` + `delete{from:Process/magazine, absorb→Tooling/magazine}` | `MTConnect.NET-Common` `CuttingToolAsset` carousel/turret/manual slot map, `ToolAssembly` holder geometry, minimal-swap `Schedule`; mints the typed `CutterForm` axis (flat/ball/bull/taper/drill/chamfer/thread-mill rows, diameter/corner-radius/taper-angle/flute-length columns PROJECTED from the ISO-13399 measurement set) | — | `→ guard`(HolderEnvelope), `→ workholding`(holder), `→ program/dialect`(`Schedule`→`M6`/`G43`/`Tnn`, NOW WIRED per V3), `→ cuttingdata`(CutterForm effective-diameter); `← Rasm.AppHost`(decoded tool-life telemetry, APPHOST[04]); `Runout` reads the real `ShankDiameterMeasurement`; `GenerateHash` reconciles to K9 | 1 |
| `Tooling/cuttingdata.md` | NEW → `new` (absorb `Process/physics:117` `CuttingData`) | The machinability model: Kienzle unit-cutting-force `kc` + chip-thickness exponent + per-operation surface-speed + feed-per-tooth columns keyed by the `physics` `Material` identity; a CSV/QIF data-INGRESS arm (honest seed DATA, never a formula pretending measured) | — | `← physics`(Material identity); read BY {`program`(deflection `kc`, the `0.0012` literal dies), `surface`(effective-diameter speed), `motion`(chip-load)}; `CutterForm` effective-diameter | 1 |

### `Geometry2D/` — ns `Rasm.Fabrication.Geometry2D` — charter: the 2D substrate (two-owner line/arc split)

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Geometry2D/algebra.md` | SPLIT from `clipper` → `new`@`Geometry2D/algebra` | `Clipper2` line-space owner: offset/`DeltaCallback64` variable offset/Boolean/Minkowski/open-path clip; `ReuseableDataContainer64` scanbeam-reuse | (line-space, no kernel seam) | read BY {`motion`,`nfp`,`guard`,`slicing`,`workholding`,`program`,`partition`} | 2 |
| `Geometry2D/arcs.md` | SPLIT from `clipper` → `new`@`Geometry2D/arcs` + `delete{from:Polygon/clipper, absorb→[algebra,arcs]}` | `CavalierContours` arc-space (bulge) owner composing the **K1 [V10]a split**: kernel emits corner-row region offsets (`Offsetting.Apply`), Fabrication composes pure kerf compensation in arc space ON TOP; `ArcBoolean` reads `result.PosPlines[].Pline` (the `result.Positive` phantom dies) | **K1** `Offsetting.Apply` | `← owner#atoms`(`Loop.Bulges`); read BY {`motion`(lead/kerf/adaptive), `program`(arc-native emission)}; `lnContours` divergence already REFUTED on disk | 2 |

### `Ingress/` — ns `Rasm.Fabrication.Ingress` — charter: everything entering as geometry

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Ingress/profile.md` | MOVE (`Geometry2D`→`Ingress`) → `new`@`Ingress/profile` + `delete{from:Polygon/import, absorb→Ingress/profile}` | `ACadSharp` read-only DXF/DWG profile ingress (`Failsafe` rail, bulge/NURBS samplers, `Insert.Explode`); the dead `warnings` accumulator wires the strict-mode `NotificationType.Error` escalation | — | `← Rasm.Bim/Exchange`(shared ACadSharp pin); read BY {`nfp`(part library), `program`(profile program)}; netDxf rejection frame dies (nothing to reject) | 2 |
| `Ingress/solid.md` | NEW → `new` | `OcctNet.Wrapper` `ImportStep`(AP203/214/242)/`ImportIges`/`ImportStl` → `OcctShape` → `Triangulate` → `OcctMesh`; the SCOPE_LIMIT (libTKHLR/libTKXCAF unbound, single-shape) a standing demand row | **K15** `MeshSpace`+`HealOp` | `OcctMesh` crosses to kernel `MeshSpace`; dirty STL → kernel `HealOp`; read BY {`slicing`,`surface`,`projection`} as admitted kernel vocabulary | 2 |
| `Ingress/steel.md` | NEW → `new` | `DSTV.Net` NC1 record tree READ (`ST` header + `BO`/`SI`/`SC`/`AK`/`IK`/`KA` blocks); `Riok.Mapperly` DSTV-record→`Loop` projection | — | read BY `nfp`; `KA` bend rows SEED the sheet-metal IDEAS lane; the emission MODEL mirrors on `Posting/dialect` (NC1 target) | 2 |

### `Toolpath/` — ns `Rasm.Fabrication.Toolpath` — charter: subtractive CAM

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Toolpath/skeleton.md` | REBUILD → `rebuild` | Kernel-medial CONSUMER: keeps ONLY the trochoidal constant-engagement WALK over the kernel clearance field; `Wavefront`/`Propagate`/`OffsetAt` DELETE | **K1+K2** — `Skeleton.cs` dies unconditionally for `Offsetting.Apply`; reads per-point RADIUS + `Clearance(Point3d probe)` | `← Offsetting.Apply`/`Skeletonize.Apply`(clearance family); read BY {`motion`(engagement bound), `guard`(`ClearanceAt`)}; RESEARCH tail purges; false "no managed library" seal dies | 2 |
| `Toolpath/motion.md` | REBUILD → `rebuild` | Real `(RemovalModality, CutStrategy)` generator arms; `Turn` a true ZX radial sweep over a revolved stock envelope; `helical`/`Peck`/`SliceWalk` distinct real generators; adaptive engagement reads the medial clearance AND the physics chip-load on one policy row | **K1** (engagement clearance), **K14** (stock-envelope hull `Bounds`+`LowerHull`) | `← skeleton`(clearance), `← surface`(3D family), `← slicing`(real `SliceLayer`), `← cell`(RobotProgram), `← Geometry2D`, `← guard`(`Check` per committed feed move, NOW WIRED), `← family`/`physics`/`cuttingdata`; the `Placement`/`Motion` arm conditioning split states on the arm | 3 |
| `Toolpath/surface.md` | NEW → `new` | 3D finishing: path LAYOUT composes kernel on-mesh machinery, cutter POSITIONING owned by **OpenCAMLib** (drop/push/waterline height sampling); waterline/Z-level, constant-scallop, pencil/corner-trace, flowline/morph, 3+2, swarf/flank rows | **K8** SDF (gouge/proximity), **K10-K13** geodesics/extract/flow/segment | `← OpenCAMLib`(C-shim+P/Invoke, cutter positioning); `← tolerance`(Ra→scallop stepover); `← CutterForm`; a Fabrication-side on-mesh distance/isoline/trace re-implementation is the [V2] defect | 3 |
| `Toolpath/partition.md` | NEW → `new` | `SharpVoronoiLib` `VoronoiPlane` Fortune sites + Lloyd relaxation: pocket-region decomposition seeds, stipple/engrave/even-spacing strategy rows | — | `← Geometry2D`; point-site only (polygon medial stays the kernel's); realizes the `[V1]` `SharpVoronoiLib` mandate | 3 |
| `Toolpath/guard.md` | KEEP+wire → `improve` | The per-move DESIGN-time safety floor (swept Minkowski envelope, distinct part-keep/stock-keep-out verdicts, collision-aware lift) | — | `← skeleton`(`ClearanceAt`), `← workholding`(`ExclusionZone`), `← magazine`(`HolderEnvelope`), `← Rasm/Spatial/index`(BVH), `← Geometry2D`; consumed BY `motion` (`Check` per committed feed move — declares the 3 out-seams) | 3 |

### `Kinematics/` — ns `Rasm.Fabrication.Kinematics` — charter: motion topology

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Kinematics/cell.md` | MOVE (`Kinematics` ns→folder) → `new`@`Kinematics/cell` + `delete{from:Toolpath/kinematics, absorb→Kinematics/cell}` | `Robots` (visose) serial-chain cell: per-manufacturer RAPID/KRL/URScript/VAL3/DRL posts, `Program` look-ahead, `extern alias R3` boundary; articulated-arm IK stays here | — | `← Robots`→`Rhino3dm`(R3); `Riok.Mapperly` boundary mappers own the two-`Rhino.Geometry` seam; band-2500→2700; RESEARCH tail purges; read BY `motion` | 3 |
| `Kinematics/machine.md` | NEW → `new` | 5-axis machine-tool topology: `KinematicClass` rotary rows (table-table trunnion\|head-head\|head-table\|nutating); rotary-axis inverse, TCP/RTCP admission; HOMES the ONE jerk/accel motion-dynamics law posting + Robots both read as a typed policy shape | **K19** `MotionInterpolation` (one-slerp) | multi-axis orientation composes the kernel one-slerp owner (never a second slerp site); read BY {`program`(Lookahead), `cell`(R3-mapped), `setups`(reach)} | 3 |

### `Additive/` — ns `Rasm.Fabrication.Additive` — charter: production 3DP

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Additive/slicing.md` | MOVE+re-route (`Additive` ns→folder) → `new`@`Additive/slicing` + `delete{from:Toolpath/slicing, absorb→Additive/slicing}` | FFF/DED planar owner: keeps `InfillPattern` + shell composition OVER the kernel-emitted contours; `Section`/O(n²)`Chain` DELETE; gyroid/TPMS route to `implicit` (the InfillPattern-row false-collapse dies) | **K3+K4** `Slicing.Apply`/`Intersection.Apply` | `← Slicing.Apply`(SliceStack, 4 layer policies); adaptive/support-interface layer rows are kernel policy seeds landed on this named demand; read BY `motion`(real `SliceLayer`); false author-forever seal dies | 3 |
| `Additive/implicit.md` | NEW → `new` | `PicoGK` voxel lane: `IImplicit → Voxels` TPMS/gyroid/cellular conformal infill, `Lattice` beam supports, overhang-conditioned scaffolds, SLA/DLP/MSLA grayscale + `.cli` vector stacks (`oVectorize→CliIo`), voxel offset/shell; declares the ALC-firebreak/sidecar posture + content-keyed mesh↔`Voxels` wire ONCE | — (owns the voxel SDF, NOT the kernel K8) | net9+native rides ALC-firebreak; `→ removal`(declared voxel seam), `→ production`(Lattice→beam-lattice); `.cli`/grayscale content-keyed via K9 | 3 |
| `Additive/production.md` | NEW → `new` | Build-orientation optimization (author-kernel on overhang/bottom-area/contour objective), machine profiles (build volume/nozzle/vat/laser/kinematics-class rows), 3MF egress via **lib3mf** (core+production+beam-lattice) | **K20** `Faces`/`Curves.Draft` | orientation objective reads kernel axis-ranked face decomposition per candidate (never a hand-rolled normal classifier); PicoGK `Lattice`→lib3mf beam-lattice direct; STL-implied hand-off dies; content-keyed K9 | 3 |

### `Nesting/` — ns `Rasm.Fabrication.Nesting` — charter: layout + yield

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Nesting/nfp.md` | KEEP+identity fix → `improve` | NFP-feasibility true-shape nesting over `Seq<Stock>`; `Remnant` lineage; rect fast-path re-homes to `RectangleBinPack.CSharp` `MaxRectsBinPack` heuristic-sweep (RectpackSharp REMOVE) | **K9** `ContentHash.Of` | `Stock.Of` hashes discriminant+ALL dims through K9 (area-only collision dies, `Remnant.Parent` un-poisoned); `Heuristic` becomes a `(Ty,Tx)` tuple; NFP feasibility frame reconciled (translate candidates by placed `(pl.Tx,pl.Ty)`, single reference) + golden-fixture proven; `← stock`(NestPlan), `← Geometry2D`; 3 raw `XxHash128` mints → K9 | 3 |
| `Nesting/stock.md` | KEEP → `improve` | The scalar exemplar: `StockNest.Pack` yield engine over `RectangleBinPack.CSharp`; `NestPlan`/`NestYield` | — | `→ nfp`(NestPlan), `← Rasm.Element/MaterialId`(NOW DECLARED ledger row), `→ Rasm.Compute`(WasteAreaMm2 — FORWARD demand, one-sided per D-2); RESEARCH tail purges | 3 |

### `Fixturing/` — ns `Rasm.Fabrication.Fixturing` — charter: keep-out + setup planning

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Fixturing/workholding.md` | MOVE+safety fix (`Fixturing` ns→folder) → `new`@`Fixturing/workholding` + `delete{from:Nesting/workholding, absorb→Fixturing/workholding}` | Per-kind real footprint-shape geometry columns (two-jaw vise, revolved chuck jaw, full-bed vacuum) replacing the `MarginScale` scalar; `Clears`/`Condition` use segment-vs-polygon intersection; `ForHolding` a TOTAL mapping (`Vise` reachable) | — | `→ posting`(`Sequence`, NOW WIRED via `Condition`), `← family`(HoldingClass), `← Geometry2D`, `← magazine`; consumed BY `guard`(`ExclusionZone`) | 3 |
| `Fixturing/setups.md` | NEW → `new` | Multi-fixture scheduler: `Setup [ComplexValueObject]`, operation-to-setup assignment, setup ordering; OWNS the setup→WCS assignment rows (setup k → G54-G59.x/G54.1-Pn); `QuikGraph` operation-precedence/datum-lineage graph (the NP-hard sequencing half) | — | `→ posting`(WCS rows posting RENDERS, never a posting-side default), `← Verify/removal`(op-N vs op-N-1 `StockSnapshot`), `← guard`(per-setup input), `← probing`(datum), `← machine`(reach); realizes `[MULTI_FIXTURE_SCHEDULE]` | 3 |

### `Posting/` — ns `Rasm.Fabrication.Posting` — charter: machine-code emission

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Posting/program.md` | REBUILD → `rebuild` | The cut-conditioning author-kernel + the structured AST: `GWord` grows canned-cycle/subprogram/macro nodes with typed R/Q/P slots; `CutProgram` gains its content key; the arc rail realizes (reads `Loop.Bulges`/arc-native offsets, `g3.BiArcFit2` retained ONLY for line-sourced kernel mesh-section chains) | **K7** `Encode.Apply`(PackKind.toolpath), **K9** content key | `← family`(PostDialect), `← dialect`(lowering folds), `← physics`/`cuttingdata`(RemovalBudget, `kc`), `← Geometry2D/arcs`, `← magazine`(`Schedule`, NOW WIRED), `← workholding`(`Sequence`), `← setups`(WCS), `→ Rasm.Persistence`(content-keyed durable row — BLOCKED egress per 00c); stub-arm `GCommand` rows become real emissions | 4 |
| `Posting/dialect.md` | NEW → `new` | The emission folds the PostDialect grammar columns drive: `Emit` a generated total `Switch` over the family axis; canned-cycle expansion (G81-G89/G76), macro/subprogram lowering (Fanuc macro-B/M98-M99/Siemens R/Heidenhain Q), code-override resolution; the NC1 steel emit TARGET (`DSTV.Net` record tree); the `Parse` round-trip arm (NIST RS274NGC modal-group state machine, `Parse ∘ Emit = id` on the admitted subset) | — | `← family`(PostDialect columns — READS, never a second axis), `← Ingress/steel`(NC1 model); rows widen as seed DATA (840D/TNC/OSP/Fagor/Centroid); NO admissible post-processor library exists — author-kernel AST validated | 4 |

### `Verify/` — ns `Rasm.Fabrication.Verify` — charter: program-level truth

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Verify/removal.md` | NEW → `new` | Voxel material-removal verification: stock `Voxels` minus accumulated swept-tool volume via `PicoGK` `BoolSubtract`; typed gouge/uncut/overcut/air-cut receipts against the ACTUAL removed-stock state; emits content-keyed per-SETUP intermediate stock snapshots (never one terminal state) | **K9** content key | `← implicit`(declared voxel/ALC seam, never a second posture), `← owner#atoms`(`ResidualStock`/`StockSnapshot`); `→ setups`(op-N snapshot), `→ motion`(residual field the [V5] rest rows consume, input-carried run N→N+1); `guard` stays the distinct design-time floor | 4 |
| `Verify/probing.md` | NEW → `new` | Touch-probe cycle vocabulary (G31/G38 rows on the posting AST), work-offset/tool-length metrology, measured-feature receipts; realizes the `[PROBING]` card | **K16** ICP, **K17** ConformanceMetric, **K18** stats | datum best-fit composes `register.md` `AlignKind` ICP (never a hand-rolled fit); verdicts fold through `ConformanceMetric` over `ResidualSample`; `← Rasm.AppHost`(decoded measured-feature/work-offset, APPHOST[04] second consumer) or typed manual-entry; `→ setups`(datum), `→ Spec/capability`(QIF MeasurementResults) | 4 |

### `Spec/` — ns `Rasm.Fabrication.Spec` — charter: production specs

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Spec/tolerance.md` | NEW → `new` | Typed GD&T vocabulary: ISO 286 fits (grade/deviation tables as GENERATED rows), ISO 1101/ASME Y14.5 feature-control frames + datum systems, ISO 1302 surface texture, MMC/LMC modifiers; QIF-aligned; owns the VOCABULARY the artifacts drawing plane draws from | — | `→ surface`(Ra/Rz→constant-scallop stepover/stepdown), `→ motion/[V5]`(ISO 286 IT grade→finishing allowance); the spec DRIVES process | 3 |
| `Spec/capability.md` | NEW → `new` | Process capability: Cp/Cpk/Pp/Ppk (streaming-moment half on **K18** kernel stats owners), distribution-fit + Monte-Carlo tolerance-stackup half over `MathNet.Numerics` (first Fabrication consumer), SPC control-limit rows; the capability gate runs at PLAN time (a process row whose Cpk history cannot hold the demanded grade FAILS admission before material is cut) | **K17+K18** | `← probing`(measured ingress, pre-folded), `← tolerance`(grade); `NodaTime` `Instant` stamps SPC receipts; a re-derived Welford/quantile beside K18 is the dual-paradigm defect | 4 |

### `Documentation/` — ns `Rasm.Fabrication.Documentation` — charter: shop documentation

| Page | Action → lowering | Owner charter | Kernel seam / lock | Seams (in/out) | Wave |
|---|---|---|---|---|---|
| `Documentation/projection.md` | MOVE+rebuild (`Projection` ns→`Documentation`) → `new`@`Documentation/projection` + `delete{from:Posting/projection, absorb→Documentation/projection}` | Kernel-`DrawingProjection` CONSUMER: the BSP interior DIES; keeps the `BooleanSolid` watertight arm; PRESERVES the `HiddenLineResult` receipt AppUi is insulated at; drops the phantom `Predicate.Orient2D` import (raw-double signs were the committed defect its own prose forbade) | **K6** `View.Apply`, **K5** `Arrangement.Apply` | `← View.Apply`(DrawingProjection), `← Arrangement.Apply(MeshBoolean)/ToMesh`(FAB:44 re-point), `→ Rasm.AppUi/Render`(`HiddenLineResult`, supersession insulated at the receipt, FAB:47) | 4 |
| `Documentation/traveler.md` | NEW → `new` | Setup sheets + shop travelers as content-keyed shop documents (the typed document MODEL only; sheet/annotation RENDERING rides the artifacts-plane seam) composing projection views, magazine tool lists, workholding/setup plans, program facts, spec rows | **K9** content key | `← projection`(views), `← magazine`(tools), `← workholding`/`setups`(fixtures), `← program`(facts), `← tolerance`/`capability`(specs); `NodaTime` `Instant` stamps | 4 |

Structural gates satisfied: 13 folders, each ≥2 pages (no one-file folder); folder=namespace=fault-cluster 1:1; six namespace ratifications (`family`/`magazine` leave `ProcessModel`, `physics` leaves `ProcessPhysics`, `faults` leaves root, `import`→`profile` leaves `Geometry2D`, `projection` leaves `Projection`) execute in their MOVE; the 13-folder recommended floor held exactly (no disk proof warranted a 14th — the kernel-consumption map homes cleanly onto the namespace-true partition, and diluting is forbidden). `ARCHITECTURE.md [01]/[02]` + the `README.md` router rewrite in the same corpus-wide leg-1 motion.

---

## [02]-[FAULT REGISTRY] — one 2700-century offset block per folder (mechanically checkable)

LANDED 2701-2710 preserved verbatim (build ON, never re-band); every arm retypes bare `string Detail`→typed payload; new arms take contiguous per-folder blocks +11.. sized by the growth law; ONE `FabricationFault [Union]` on `FaultBand.Fabrication = 2700`.

| Folder (fault-cluster) | Offsets | Arms (typed payload) |
|---|---|---|
| `Process/` (landed base) | 2701-2705 | the original four (2701-2704, profile/input/geometry/admission) + `InadmissiblePair` 2705 (typed `(ProcessKind, RemovalModality)` pair) |
| `Toolpath/` (landed) | 2706-2707 | `Gouge` 2706 (point+tool), `Collision` 2707 (zone) |
| `Additive/` (landed) | 2708 | `NonManifoldSlice` 2708 (slice elevation+edge) |
| `Nesting/` (landed) | 2709-2710 | `StockOverflow` 2709 (unplaced part set), `Nest` 2710 (malformed job) |
| `Tooling/` (new) | 2711-2713 | `ToolLifeExpired`(asset+remaining), `MagazineOverflow`(slot demand), `RunoutExceeded`(measured+limit) |
| `Geometry2D/` (new) | 2714-2715 | `KerfOverlap`(offset region), `ArcFitDivergence`(chord+tolerance) |
| `Ingress/` (new) | 2716-2718 | `SolidImportFailed`(format+shape id), `UnboundTopology`(scope-limit), `DstvParseError`(block+line) |
| `Toolpath/` (new) | 2719-2721 | `EngagementExceeded`(bound+actual), `DropCutterStalled`(cutter+region), `VoronoiDegenerate`(site set) |
| `Kinematics/` (new) | 2722-2723 | `RtcpUnreachable`(pose), `RotaryLimitExceeded`(axis+angle) |
| `Additive/` (new) | 2724-2726 | `SupportGenerationFailed`(overhang region), `OrientationInfeasible`(objective), `VoxelResolutionExceeded`(bbox+cap) |
| `Fixturing/` (new) | 2727-2729 | `NoFeasibleSetup`(op set), `ClampCollision`(fixture zone), `DatumLineageBroken`(setup chain) |
| `Posting/` (new) | 2730-2732 | `UnsupportedDialectFeature`(dialect+feature), `MacroExpansionFault`(node), `ParseRoundTripMismatch`(word delta) |
| `Verify/` (new) | 2733-2736 | `GougeVerified`(voxel region+tool), `UncutRemaining`(residual voxels), `OvercutDetected`(region), `ProbeOvertravel`(cycle+limit) |
| `Spec/` (new) | 2737-2739 | `ToleranceUnsatisfiable`(frame), `CapabilityGateFailed`(Cpk+grade), `StackupExceeded`(chain+bound) |
| `Documentation/` (new) | 2740-2741 | `TravelerIncomplete`(missing section), `ViewProjectionFault`(view op) |

Headroom 2742-2799. Acceptance: `rg 'band-2500|25[0-9][0-9]'` returns zero corpus + `.api`-wide post-motion; each folder's new arms sit in its contiguous block.

---

## [03]-[SEAM LEDGER] — both directions, corrected, with counterpart obligations + stale re-points

Arrow = composes (`←` consumes / `→` produces). Every cross-package edge is a ledger row; every declared edge composes in a fence; the intra web is declared. `ARCHITECTURE.md [02]` rewrites to this in the leg landing each seam.

### (A) Kernel-consumed (INBOUND, Fabrication ← Rasm; the spine — K1-K20 above)
| Fabrication node | Kernel seam | Direction | Lock / disposition |
|---|---|---|---|
| `Geometry2D/arcs` | `Rasm/Meshing/offset` `Offsetting.Apply` | ← | K1 [V10]a — kerf ON TOP of kernel corner-row offsets |
| `Toolpath/skeleton` | `Rasm/Meshing/offset`+`Rasm/Meshing/skeleton` clearance family | ← | K1+K2, FAB:22 — `Skeleton.cs` dies |
| `Toolpath/motion` | `Rasm/Meshing/offset` (engagement) + `Rasm/Analysis/measure`+`Rasm/Meshing/delaunay`+`cloud` (hull) | ← | K1+K14 [V11] |
| `Toolpath/surface` | `Rasm/Analysis/fields` SDF + `Processing/{geodesics,extract,flow,segment}` | ← | K8+K10-K13 [V8]/[V5] |
| `Additive/slicing` | `Rasm/Meshing/slice`+`Rasm/Meshing/intersect` | ← | K3+K4, FAB:23/48 |
| `Documentation/projection` | `Rasm/Drawing/view` + `Rasm/Meshing/arrangement` | ← | K6+K5, FAB:33/44/46 |
| `Posting/program` | `Rasm/Drawing/pack` `Encode.Apply` PackKind.toolpath | → (residency) | K7 [V13] — channel-set alignment as ledger row, never a Fabrication encoder |
| `Ingress/solid` | `Rasm/Meshing/mesh`+`Rasm/Processing/repair` | ← | K15 [03] |
| `Verify/probing` | `Rasm/Processing/register`+`Rasm/Analysis/measure` | ← | K16+K17 [V7] |
| `Spec/capability` | `Rasm/Domain/stats` | ← | K18 [V8] |
| `Kinematics/machine` | `Rasm/Parametric/projections` `MotionInterpolation` | ← | K19 [V5] |
| `Additive/production` | `Rasm/Analysis/select` `Faces`/`Curves.Draft` | ← | K20 [V6] |
| `*` | `Rasm/Numerics/predicates` `Orient2D/Orient3D` | ← | WIRE (existing) |
| `*` | `Rasm` `Matrix/Point3d/Vector3d` | ← | SHAPE (existing) |

### (B) Substrate rails (INBOUND, kernel-owned mechanisms; K21-K23)
`Process/owner` threads `Rasm/Domain/rails` (`Op`/`Eff<Env>`); `*` register into `Rasm/Domain/validation` (`IValidityEvidence`→`OpAcceptance`, zero oracle edits); `*` mint identity through `Rasm/Domain/identity` `ContentHash.Of` (K9, the ONE site); typed output rides `Rasm/Numerics/atoms` `AtomProjection`/`ProjectionRow`.

### (C) Peer boundaries (raw-double / delegate — NOT references, strata law)
| Edge | Shape | Note |
|---|---|---|
| `Process/physics ← Rasm.Materials/Properties` | raw doubles (Thermal/SpecificHeat/Density) | ARCH:49 anchor reconcile (`Rasm.Materials/Properties` vs `physics.md:3` `physical-properties#MATERIAL_PROPERTY` — canonicalize to `Properties`) |
| `Nesting/nfp ← [injected Func<NoFitPolygon,PartTransform,double>]` | delegate | the DRL score boundary (BLOCKED card `[NFP_DRL_POLICY]`) |
| `Nesting/stock ← Rasm.Element/MaterialId` | substrate peer | **NEW ledger row** (E11 wired-undeclared, `stock.md:40,83,99`) |

### (D) Downstream receipts (OUTBOUND, app-platform consumes)
| Edge | Receipt | Note |
|---|---|---|
| `Documentation/projection → Rasm.AppUi/Render` | `HiddenLineResult` | FAB:47; supersession insulated AT the receipt (UIBRIEF [V7](b), `:162`) |
| `Nesting/stock → Rasm.Compute` | `NestYield.WasteAreaMm2` | **FORWARD demand, one-sided** (D-2 CBRIEF [V12] REFUTED — Compute counterpart un-named upstream) |
| `Process/magazine ← Rasm.AppHost` | decoded tool-life telemetry | APPHOST[04] MTConnect.NET; mid-job reload |
| `Verify/probing ← Rasm.AppHost` | decoded measured-feature/work-offset | APPHOST[04] second Fabrication consumer |

### (E) Persistence egress (OUTBOUND, content-keyed — BLOCKED cross-package dependency per 00c)
`Posting/program`, `Nesting/nfp` (Placement/Remnant), `Additive/implicit`+`production` (`.cli`/3MF), `Verify/removal` (per-setup snapshots), `Documentation/traveler` all mint content keys through K9 `ContentHash.Of` and carry the `ArtifactKind`/durable-row fold as a **Fabrication-authored demand on the BLOCKED Persistence card `[FABRICATION_PROGRAM_DURABLE_ROWS]`** (2701-2710 decode constraint honored). NOT a composed upstream contract; recorded as a seam-ledger residual.

### (F) Intra-package web (all declared — E11 closure)
`family →` {owner, physics, motion, program, dialect, magazine, workholding, setups, cell, machine} (axes/columns); `physics → cuttingdata`; `magazine →` {guard(HolderEnvelope), workholding(holder), program/dialect(Schedule), cuttingdata(CutterForm)}; `Geometry2D/algebra →` {motion, nfp, guard, slicing, workholding, program, partition}; `Geometry2D/arcs →` {motion, program}; `skeleton →` {motion(engagement), guard(ClearanceAt)}; `surface →` motion; `guard ←` motion(per-move); `workholding → posting`(Sequence); `setups →` {posting(WCS), guard, probing, removal}; `nfp ← stock`(NestPlan); `removal →` {setups(snapshot), motion(residual, input-carried)}; `probing →` {posting(G31/G38 rows), setups(datum), capability(QIF)}; `tolerance →` {surface, motion, capability}; `traveler ←` {projection, magazine, workholding, setups, program, tolerance, capability}; `owner#run ←` {projection, motion, nfp, slicing, removal, probing}.

### (G) Stale re-points (close in the leg changing the fact)
| Anchor | Was | Re-point | Leg |
|---|---|---|---|
| `ARCH:16`, `kinematics.md:3`, `TASKLOG:44`, `api-dstv-net.md:3,126`, `api-robots.md:5,94` | band-2500 | 2700 / 2701-2710 | 1 |
| `ARCH:52`, `README:33,64`, `import.md:20`, `api-dstv-net.md:132,143`, `api-acadsharp.md:120` | netDxf write-leg / Rhino write | AppUi ACadSharp `DxfWriter`+`DwgWriter` two-format leg ([V7]); netDxf rejection frame dies | 1-2 |
| `ARCH:53` (`program→`), `ARCH:54` (`nfp→`), `api-mtconnect-net-common.md:119`, `api-occtnet-wrapper.md:137` | `Rasm.Persistence/Schema` (DELETED) | content-keyed artifact index via K9 (BLOCKED egress, 00c) | 1 / 4 |
| `ARCH:44` | `Rasm/Processing/repair` `BooleanOp` | `Rasm/Meshing/arrangement` (K5, FAB:44) | 2/4 |
| `ARCH:46` | `Rasm/Drawing/view` "beside the in-folder BSP solver" | the "beside" clause DIES (kernel supersedes, K6) | 4 |
| `ARCH:63` | multi-fixture-scheduler as OWNED | `Fixturing/setups` realization | 1/3 |
| `api-geometry3sharp.md:106` | "dropped CavalierContours / one-Clipper2 law" | self-contradiction DIES (CavalierContours IS admitted) | 1 |
| `api-hashing.md:3,88` | "no shared C# tier" | slim to thin overlay deferring to shared `libs/csharp/.api/api-hashing.md`, name K9 `ContentHash.Of` | 1 |
| `api-sharpvoronoilib.md:3,18` | dual kernel+Fabrication ownership | Fabrication-sole (kernel csproj has no ref) | 1 |
| `api-picogk.md:5,113,154` | geometry3Sharp DMesh3 "SOLE owner" + wire meeting point | owned biarc fold + kernel mesh vocabulary | 1 |
| `csproj:3` Description | "CAM toolpath motion with DH/IK kinematics" | rewrite (Robots cell superseded DH/IK) | 1 |

---

## [04]-[ROSTER DELTA] — adds / removes / re-groups with `.api` obligations

Central ownership `Directory.Packages.props` (hand-edited, label-grouped) + `Rasm.Fabrication.csproj`; folder-tier `.api` under `libs/csharp/Rasm.Fabrication/.api/`, shared substrate under `libs/csharp/.api/`. INTEGRATION-FIRST: the four zero-consumer admissions are MANDATES realized as pages, never removal candidates. License gate: OSS + free-for-OSS-commercial admissible; LGPL native engines admissible as dynamic-link P/Invoke under the golden-fixture gate.

### ADD (2)
| Package | License | Siting / mechanics | `.api` obligation |
|---|---|---|---|
| **OpenCAMLib** | LGPL-2.1 (four-source confirmed) | `Toolpath/surface` drop/push/waterline/adaptive-waterline cutter POSITIONING via a thin in-house `extern "C"` C-shim + source-generated `[LibraryImport]` P/Invoke (upstream ships C++-mangled-ABI SHARED archives only — direct P/Invoke impossible, C-shim MANDATORY, the ruled path). Probe VERDICT **FEASIBLE**: all three RIDs ship (`macos-cxx-arm64`/`windows-cxx-x64`/`linux-cxx-x86_64` `libocl`, release 2023.01.11), one `ocl::Operation` base unifies the four engines, the 5 `MillingCutter` forms map 1:1 onto the `CutterForm` axis, dep closure is header-only Boost + OpenMP (zero submodules). NO NuGet pin (native asset RIDs recorded at admission; shipped SHARED archives OR from-source per RID via `forge-scientific-env`, Z3 precedent). **A1** per-RID asset; **A2** libomp carriage (osx bundle rpaths `libomp.dylib`; linux `libgomp`; win `vcomp`; or `USE_OPENMP=OFF` build-policy row); **A3** golden-fixture gate (drop-cutter/waterline per cutter form); content-keyed at wire. Author-kernel drop-cutter over K8 SDF is the RECORDED fallback, standing ONLY on admission abandonment (strictly heavier: analytic per-form contact vs grid-bound, waterline degrades to marching-squares, calendar-hostage to the unlanded SDF lane). | NEW `api-opencamlib.md` (Operation lifecycle, cutter-form ctors, ~18-24 extern-C fn ABI, RID/libomp map, LGPL dynamic-link note; Boost.Python binding is the member-by-member blueprint — brief's "pybind11" is imprecise, canonical is Boost.Python, immaterial) |
| **lib3mf** | BSD-2 | `Additive/production` 3MF core+production+beam-lattice writer via the vendored OFFICIAL C# binding + RID-keyed native asset (NOT on NuGet — feed-verified; `IxMilia.ThreeMf` dominated: unpublished, core-spec-only, no beam-lattice). PicoGK `Lattice` output maps directly to the beam-lattice extension; golden-fixture gated; content-keyed. | NEW `api-lib3mf.md` (writer surface, production/beam-lattice extensions, RID asset map, BSD-2 note) |

### REMOVE (4)
| Package | Proof | Consumer re-home / `.api` |
|---|---|---|
| **RectpackSharp** (`PROPS:85`) | proven redundancy: strict subset of `RectangleBinPack.CSharp` (`PackingHints.FindBest` sweep reproduces as one `MaxRectsBinPack.Insert` heuristic-sweep fold) | `nfp` fast-path re-homes to RBP `MaxRectsBinPack`; `README:65` rejection rewrites; `api-rectpacksharp.md` content re-homes to the nfp fast-path |
| **netDxf** (`PROPS:359`) | VERIFY-REMOVE: leaves with the AppUi `[V7]` motion; this campaign removes only a surviving orphan | — |
| **netDxf.netstandard** (`PROPS:470`) | VERIFY-REMOVE: leaves with the Compute `[V8]` FEALiTE2D.Plotting retirement (its sole lock parent); a surviving parity-kept-FEALiTE2D transitive floor stays, never Fabrication residue | — |
| **geometry3Sharp** (`PROPS:81`) | REPLACE-candidate: abandoned (feed 1.0.324, 2019-03-07). Arc-native rail retires the refit for arc-sourced paths; line-sourced kernel mesh-section chains land an owned Bolton biarc fold on `Posting/program`. Fabrication reference (`CSPROJ:17`) leaves when BOTH land; stays ONLY if the owned fold cannot meet refit tolerance on golden fixtures (recorded). Central pin leaves with Bim's own mesh-text-importer drop (`Rasm.Bim.csproj:22` the co-consumer) — a cross-package residual RECORDED, never executed here | `api-geometry3sharp.md:106` self-contradiction dies |

### RE-GROUP (verify / execute)
| Motion | State on disk | Action |
|---|---|---|
| Float-lane (`CavalierContours`/`Clipper2`/`SharpVoronoiLib`) → `Label="Fabrication"` | **LANDED** (`PROPS:77,78,87` under Fabrication label — geometry DECISION [09] executed pre-leg) | VERIFY at leg 1 |
| `geometry3Sharp` → Fabrication label | LANDED (`PROPS:81`, comment "Bim co-consumes; disposition rides Bim/Fabrication campaigns") | VERIFY; disposition = the REPLACE above |
| `RectangleBinPack.CSharp` → Fabrication label | **PENDING** (still `Label="Materials"` `PROPS:167`) | RE-GROUP from the vacated Materials label at leg 1 |

### VERIFY (no motion)
- **MathNet.Numerics** stays `6.0.0-beta2` (`PROPS:63`): the geometry DECISION [09] RULED the 5.0.0 downgrade RESOLVED as HOLD-beta2 (`Integrate.OnCuboid` is 6.0-only, no 5.0 substitute) — SUPERSEDING the brief's "5.0.0 downgrade ruling". Fabrication is a `[V8]` co-consumer; no motion; overlay catalogs stay truthful against beta2.

### KEEP-realize (the four dead admissions → pages) + KEEP-obligation
| Package | Realized page(s) | `.api` obligation |
|---|---|---|
| `PicoGK` | `Additive/implicit` (voxel/TPMS/lattice/support/resin `.cli`) + `Verify/removal` (`BoolSubtract` NC-verify) | `api-picogk.md` drops the geometry3Sharp-sole-owner coupling |
| `OcctNet.Wrapper` | `Ingress/solid` (STEP/IGES/STL→OcctMesh) | `:137` Schema→artifact index; SCOPE_LIMIT stays a demand row |
| `DSTV.Net` | `Ingress/steel` (NC1 read) + `Posting/dialect` (NC1 emit target) | `:3,126` band→2700; `:132,143` netDxf purge |
| `SharpVoronoiLib` | `Toolpath/partition` | `:3,18` dual-ownership→Fabrication-sole |
| `Clipper2`/`CavalierContours`/`Robots`/`MTConnect.NET-Common`/`ACadSharp`/`RectangleBinPack.CSharp`/`System.IO.Hashing`/`UnitsNet` | held (Geometry2D/cell/magazine/profile/nesting) | `System.IO.Hashing` reached ONLY via K9; `MTConnect` `GenerateHash`↔K9; `UnitsNet` overlay extends Force/Power/Temperature/Angle/Torque; `ACadSharp:120` Rhino-write→AppUi leg |

### SHARED-TIER mine (verify present, compose — no admission; the substrate `api-languageext.md` VERIFIES, never re-authored)
`Riok.Mapperly` (`PROPS:30`, R3/MTConnect/DSTV boundary mappers) · `System.Numerics.Tensors` (`:42`, SIMD hot sampling folds) · `CommunityToolkit.HighPerformance` (`:38`, `Span2D`/`Memory2D` grids) · `NodaTime` (`:39`, `Instant` receipt stamps) · `QuikGraph` (`:41`, `Fixturing/setups` precedence/lineage graph) · `MathNet.Numerics` (`:63`, `[V8]` fit/Monte-Carlo, first Fabrication consumer). `LanguageExt`/`Thinktecture`/`JetBrains.Annotations` substrate verified present, no Fabrication-scoped delta.

Post-campaign folder-tier `.api` count: 14 − 1 (RectpackSharp re-home) + 2 (opencamlib, lib3mf) = **15**.

---

## [05]-[VERDICT DISPOSITION] — V1-V10 (every verdict resolved)

| V | Resolution |
|---|---|
| **V1** FOLDER_PARTITION | 13 folders / 33 pages, folder=namespace=fault-cluster 1:1; six ratifications in their MOVE; the CS0118 `Process` type collision RULED (`ProcessKind` rename, 00b); `owner` evaluated as two nodes (`owner#atoms`/`owner#run`) — `ProjectionDir`+`ResidualStock`+`StockSnapshot` mint on `owner#atoms`, breaking the owner↔projection and motion↔removal cycles; graph acyclic (leg order is its topo-sort) |
| **V2** KERNEL_PLANE_CONSUMPTION | five reframes carried verbatim ([V2]a skeleton→K1/K2 medial, [V2]b slicing→K3/K4 slice-stack, [V2]c projection→K6 DrawingProjection + K5 watertight, [V2]d arcs→K1 [V10]a kerf split, [V2]e booleans/SDF→K5/K8 kernel-consumed); [00] signature-lock table; five `[RESEARCH]` tails purge |
| **V3** EXECUTION_WIRE | `Run → Cam → {Guard.Check per committed feed move, Workholding.Condition, Magazine.Schedule} → Post` EXECUTES (acyclic); arc rail realizes (`Loop.Bulges` on `owner#atoms`, `BulgeAt`, 3-arg ctor; `result.Positive`→`PosPlines`); Fixturing safety hardens (segment-vs-polygon, total `ForHolding`, per-kind footprint columns); work-offset chain wires `setups`→`posting`→`probing`; the `Placement`/`Motion` arm conditioning split states on the arm (no double-kerf) |
| **V4** POST_GRAMMAR | `PostDialect` grammar-family columns on `Process/family` (data), emission folds on `Posting/dialect` (lowering); `Emit` a generated total `Switch`; AST grows canned-cycle/subprogram/macro nodes; NC1 emit target; `Parse` round-trip (NIST modal-group); Robots + posting share ONE motion-dynamics law on `Kinematics/machine` |
| **V5** CAM_DEPTH | `CutStrategy` dimensionality column + axis-count admission; waterline/scallop/pencil/rest/3+2/swarf/thread-mill/drill rows; path LAYOUT from K10-K13, cutter POSITIONING from OpenCAMLib (probe FEASIBLE — ADMIT ruled); stub arms real generators (`Turn` ZX sweep over K14 hull, `helical`/`Peck`/`SliceWalk` distinct); `Kinematics/machine` 5-axis topology + TCP/RTCP; `CutterForm` axis on `Tooling` (four consumers); engagement reads medial clearance + chip-load on one row |
| **V6** ADDITIVE_PRODUCTION | `Additive/` folder (slicing K3 consumer, implicit PicoGK, production orientation/profiles/3MF+lib3mf); `RemovalBudget` Resin/Powder/DED cases; gyroid→voxel lane (InfillPattern false-collapse dies); adaptive layer-height as kernel slice-stack policy seeds |
| **V7** VERIFY_PLANE | `Verify/removal` (PicoGK `BoolSubtract` voxel truth, gouge/uncut/overcut/air-cut, residual field, per-setup content-keyed snapshots) + `Verify/probing` (G31/G38 cycles, K16 ICP datum, K17 conformance, AppHost-decoded ingress); `ResidualStock`/`StockSnapshot` on `owner#atoms`; `FabricationPolicy` Verify/Inspect cases |
| **V8** SPEC_PLANE | `Spec/tolerance` (ISO 286/1101/1302/ASME Y14.5, QIF-aligned) + `Spec/capability` (Cp/Cpk over K18 stats + MathNet fit/Monte-Carlo, SPC, plan-time gate); spec DRIVES process (Ra→scallop, IT→allowance, Cpk gate); `Documentation/traveler`; UnitsNet overlay extends |
| **V9** PHYSICS_IDENTITY | `Material` ONE identity + `Map<RemovalModality, ModalityPhysics>`; `Budget` modality-dispatches; `Erosion`/`Resin`/`Powder` budgets; `CuttingData`→`Tooling/cuttingdata` Kienzle `kc` (the `0.0012` literal dies); dead `Overrides` deletes onto the data-ingress arm; `90.0` demotes behind the table |
| **V10** GOVERNANCE_TRUTH | 25xx census→2700 (ARCH/kinematics/TASKLOG/api tier); netDxf purge; `ARCH:63` re-scope; `[GUARD]-[QUEUED]` COMPLETE; `api-geometry3sharp:106` dies; `api-hashing` slims; `nfp` `Stock.Of` K9 identity + `(Ty,Tx)` tuple + NFP frame consistency proven |

---

## [06]-[EVIDENCE DISPOSITION] — E1-E14 (with survey corrections)

| E | Disposition |
|---|---|
| **E1** dead admissions | four integration MANDATES → pages (PicoGK→implicit+removal, OcctNet→solid, DSTV→steel+dialect, SharpVoronoiLib→partition); anchor-pairing DRIFT noted (PicoGK `:20`, DSTV `:16`) |
| **E2** unwired pipeline | V3 executing fold; `ARCH:63` queued-scheduler→`setups` |
| **E3** arc-rail fiction | `Loop.Bulges` landed (`owner#atoms`); rail realizes; `lnContours` sub-claim REFUTED on disk (already `CavalierContours.*`); NEW phantom `result.Positive`→`PosPlines[].Pline` |
| **E4** post thinness | V4 grammar |
| **E5** stub generators | V5 real generators |
| **E6** physics identity | V9 |
| **E7** HLR not CAD-grade | V2c kernel DrawingProjection; `HiddenLineResult` preserved; phantom `Predicate.Orient2D` import dropped (survey addition — raw-double signs were the committed defect) |
| **E8** coverage silences | V5/V6/V7/V8 planes |
| **E9** identity/logic | V10 `Stock.Of`, NFP three-frame incoherence (survey: stronger than "unverified" — origin vs Anchor vs untranslated-placement), `ForHolding` total, segment-exact keep-out, footprint columns |
| **E10** stale governance | V10 + survey-added `.api` Schema anchors (`api-mtconnect:119`, `api-occtnet:137`) |
| **E11** ledger gaps | full seam ledger [03] incl. `MaterialId` row; the two REFUTED reciprocals demote to forward demands (00c) |
| **E12** redundancy/overlay | RectpackSharp REMOVE; `api-hashing` slim; `Runout` real measurement |
| **E13** page-craft | FIVE `[RESEARCH]` tails purge (not three: +`stock.md:317`, `program.md:379`); false author-forever seals corrected |
| **E14** upstream bindings | signature-lock against STABLE DECISION `FAB:NN` (not drifted `GBRIEF` +14..+16); the two `[V12]` REFUTED→forward demands; UIBRIEF `[V6]`→`[V7]`(b) |

---

## [07]-[ESCALATION DELTA] — [03] plane targets (each carried by its folder page-set)

Process axes 8→9.5 (`owner`/`family`/`faults` + K21-K23 substrate rails) · Physics+machining 6→9 (`physics` map + `Tooling/cuttingdata` Kienzle) · Tooling 8→9.5 (`magazine` wired + `CutterForm` + AppHost seam) · Geometry2D 8→9.5 (two-owner split + K1 [V10]a) · Ingress 5→9.5 (`profile`+`solid` K15+`steel`) · Toolpath 5→9.5 (real arms + `surface` OpenCAMLib/K8-K13 + `partition` + `guard` wired + `skeleton` K1/K2) · Kinematics 7→9.5 (`cell` held + `machine` 5-axis/K19) · Additive 4→9.5 (`slicing` K3 + `implicit` + `production` lib3mf) · Nesting 8.5→9.5 (`nfp` K9 identity + frame proof + `MaterialId`) · Fixturing 6→9.5 (`workholding` safety + `setups`) · Posting 6→9.5 (`program` AST+K7+key + `dialect` grammar/NC1/Parse) · Verify —→9.5 (`removal`+`probing`) · Spec —→9.5 (`tolerance`+`capability` K17/K18) · Documentation 6→9.5 (`projection` K6 + `traveler`). Every target grade is the acceptance bar the folder page-set reaches.

---

## [08]-[CARD DISPOSITION] — full TASKLOG + IDEAS

### IDEAS.md
| Card | Was | Disposition |
|---|---|---|
| `[DRL_NEST_POLICY]` | BLOCKED | STAYS BLOCKED — `NestPolicy.Score` injected-delegate is the strata-correct shape; closes when Compute lands its side |
| `[GUARD]` | QUEUED | CLOSE COMPLETE — `guard.md` landed; the unwired state is the V3 execution defect, not an unbuilt page (moves to CLOSED) |
| `[PROBING]` | QUEUED | REALIZE into `Verify/probing` ([V7]); close as realized-into-plane |
| `[MULTI_FIXTURE_SCHEDULE]` | QUEUED | REALIZE into `Fixturing/setups` ([V1]); close as realized-into-plane |
| `[CSG_SILHOUETTE]` | COMPLETE | keep — the `BooleanSolid` watertight arm (K5) survives [V2]c |
| `[MAGAZINE]` | COMPLETE | keep the model; the "G43/M6/Tnn emitted by program.md" clause is the E2 fiction — the wire realizes in V3 (leg 4) |
| `[FEEDRATE_LOOKAHEAD]` | COMPLETE | keep — the jerk/accel math is sound; now shares the ONE motion-dynamics law with Robots ([V4]) |
| RESEED | — | new growth: sheet-metal bend/unfold (DSTV `KA` + K-factor over OcctNet faces, `Ingress/`+`Spec/`; geometric floor = kernel `develop.md` isometric unroll, the K-factor table the Fabrication overlay); 5-axis simultaneous swarf refinement; conversational-dialect breadth beyond the seed rows |

### TASKLOG.md
| Card | Was | Disposition |
|---|---|---|
| `[NFP_DRL_POLICY]` | BLOCKED | STAYS BLOCKED (Ripple of `[DRL_NEST_POLICY]`) |
| `[CSG_WATERTIGHT_SILHOUETTE]` | COMPLETE | keep (K5 arm preserved) |
| `[TOOLPATH_STRATEGY_MODALITY_FACTOR]` | COMPLETE | keep — the `(RemovalModality,CutStrategy)` factor is the base V5 widens |
| `[POST_DIALECT_OVERRIDE_SEAM]` | COMPLETE | keep — the override seam is the base V4 grammar widens |
| `[TROCHOIDAL_ENGAGEMENT_LIMIT]` | COMPLETE | keep — the trochoidal WALK is the ONLY skeleton arm surviving [V2]a; engagement now reads K1 clearance |
| `[NEST_MULTISHEET_SCHEDULE]` | COMPLETE | keep |
| `[NEST_REMNANT_LINEAGE]` | COMPLETE | keep — but `Remnant.Of`/`Stock.Of` re-mint through K9 (V10) |
| `[RECTPACK_FASTPATH_ADMISSION]` | COMPLETE | SUPERSEDED — RectpackSharp REMOVE; the fast-path re-homes to RBP `MaxRectsBinPack` heuristic-sweep (re-dispose in the leg-1 roster motion) |
| `[BIARC_ARC_EMISSION_ADMISSION]` | COMPLETE | SUPERSEDED — geometry3Sharp REPLACE-candidate; refit retires for arc-sourced paths, an owned Bolton biarc fold owns line-sourced chains |
| `[ACADSHARP_SPLINE_INSERT]` | COMPLETE | keep (moves with `import`→`Ingress/profile`) |
| `[CLIPPER_INNER_FIT]` | COMPLETE | keep (moves with the `Geometry2D` split) |
| `[FABRICATION_FAULT_BAND_DEEPENING]` | COMPLETE | keep; the in-card `2505-2509` codes re-point to 2700 (R6, the COMPLETE-card stale-25xx) |
| `[TOOL_CUTTING_DATA_TABLE]` | COMPLETE | SUPERSEDED — `CuttingData` moves to `Tooling/cuttingdata` + deepens to Kienzle `kc` (V9) |
| `[CLIPPER_VARIABLE_KERF]` | COMPLETE | keep (moves with `Geometry2D/algebra`) |
| `[FAULT_REBAND_2700]` | COMPLETE | keep — the landed 2700 band the registry [02] builds ON |
| `[STOCK_NEST_MOVE]` | COMPLETE | keep — the `NestYield.WasteAreaMm2` Ripple re-dispose as a FORWARD demand (D-2, one-sided) |

Cards disposed: 7 IDEAS + 16 TASKLOG = 23.

---

## [09]-[BUILD-LEG PARTITION] — 4 legs, acyclic (kernel-seam topo-sort)

The DAG's topological sort: atoms before their consumers, kernel-seam consumers before the CAM/egress arms that read their output. Every edge is within-wave or earlier.

1. **STRUCTURE + AXES + RAIL** — the `[V1]` re-partition corpus-wide (moves/splits/namespaces, `ARCHITECTURE.md [01]/[02]`, README router) + FULL roster reconciliation ([04] adds/removes/re-groups, netDxf purge, `.api` stubs + slims, MathNet verify, the `ProcessKind` rename). `Process/` rebuilds (`owner` entry case SHAPES + `Loop.Bulges` + `[V7]` `ResidualStock`/`StockSnapshot` on `owner#atoms`; `family` axis widening; `physics` identity map + budgets; `faults` payloads + new arms + 25xx closure). `Tooling/` lands (`magazine` wire-ready; `cuttingdata` Kienzle). Pages 1-6.
2. **SUBSTRATE + INGRESS** — `Geometry2D/` (two-owner split composing K1 [V10]a, `lnContours`/`PosPlines` correction, arc rail exported), `Ingress/` (`profile` move + netDxf purge, `solid` K15 admission, `steel`), and `Toolpath/skeleton` rebuilds HERE (K1/K2 kernel-medial consumer, cross-folder staged early — wave-3 `motion` reads its clearance field). Pages 7-11, 16.
3. **CAM + ADDITIVE + NESTING + FIXTURING** — `Toolpath/` (`motion` real arms + engagement + `guard` wired; `surface` K8-K13 + OpenCAMLib; `partition`), `Kinematics/` (`cell`, `machine` K19), `Additive/` (`slicing` K3, `implicit`, `production` lib3mf), `Nesting/` (`nfp` K9 identity, `stock` seam re-anchors + `MaterialId`), `Fixturing/` (`workholding` safety, `setups`), and `Spec/tolerance` rebuilds HERE (cross-folder staged early — `surface` Ra→scallop reads its rows within-wave). Pages 12-15, 17-25, 30.
4. **EGRESS + TRUTH + DOCS** — `Posting/` (`program` AST + K7 + content key + magazine/workholding/setups consumption; `dialect` grammar + canned/macro + NC1 + `Parse`), `Verify/` (`removal`, `probing`), `Spec/capability`, `Documentation/` (`projection` K6 + `traveler`). Pages 26-29, 31, 32-33.

Per-leg closeout: hard residuals resolve before the next leg; `README`/`ARCHITECTURE`/`TASKLOG`/`IDEAS` close in the leg changing their facts; `.api` stubs land wave 1, deepened by the leg first composing the package; cross-package counterparts are 1-hop ripples in the leg return (the geometry DECISION's FAB:22/23/33/44/46/47/48 counterpart edits are VERIFIED in place, never re-edited). The four `[05]` acceptance dry-runs (toolpath / five-target posting / additive / production-truth) compose entirely from rebuilt fences with ZERO Fabrication-side geometry re-implementation.

Acyclicity proof: the kernel seams (A) are all INBOUND (Fabrication ← Rasm); the kernel references Fabrication only through the DECISION's counterpart ledger rows (already landed). `owner#atoms` (leg 1, upstream-only) → planes → `owner#run` (terminal, compose-only). The motion↔removal type cycle is broken by minting `ResidualStock` on `owner#atoms` and carrying it INPUT-side (run N verify → run N+1 rest). The owner↔projection cycle is broken by minting `ProjectionDir` on `owner#atoms`. Graph is a DAG; the four legs are its topo-sort.
