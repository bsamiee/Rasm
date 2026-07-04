# [DRAFT] — RASM-CS-FABRICATION — LENS: PACKAGE-MINING-FIRST

Complete folder/page-set blueprint for `libs/csharp/Rasm.Fabrication/.planning/`. The structure is DERIVED from what the admitted roster can actually do: each integration mandate (PicoGK, OcctNet.Wrapper, DSTV.Net, SharpVoronoiLib) and each ADD (OpenCAMLib, lib3mf) gets its named owner page BEFORE the partition settles, every shared-tier mine row gets its consuming pages, and every `[04]` obligation + probe finding is homed. The result reproduces the `[V1]` 13-folder floor EXACTLY (package demand and namespace truth converge), so no floor dilution and no unproven expansion. Counts: **13 folders · 33 pages · 24 package dispositions · V1-V10 + E1-E14 disposed · 23 cards + 3 reseeds**.

Verified-on-disk corrections this draft carries (the DECISION must absorb):
- **Float-lane re-group LANDED** (`PROPS:77,78,87` — CavalierContours/Clipper2/SharpVoronoiLib now under `Label="Fabrication"`; the api-manifests dossier's "unexecuted at :69-77" is stale by ~minutes). Fabrication OWNS the label from leg 1; R5 = VERIFY-only, no motion.
- **MathNet held at `6.0.0-beta2`** (`PROPS:62-63` comment: Compute `Integrate.OnCuboid` is 6.0-only). The brief's "5.0.0 downgrade ruling" is SUPERSEDED by the live manifest hold — `Spec/capability` composes MathNet at `6.0.0-beta2`, no Fabrication-forced version motion.
- **TWO `[V12]` reciprocal bindings REFUTED as landed law** (upstream-bindings §D-2): `PBRIEF [V12]` is `[V12_GOVERNANCE_RECONCILE]` (NOT an `ArtifactKind` egress index) and `CBRIEF [V12]` is `[V12_DISCIPLINE_COVERAGE]` (NOT the `WasteAreaMm2` rollup). Both are UN-LANDED forward demands. The blueprint mints egress keys through the confirmed `ContentHash.Of` entry and carries `ArtifactKind`/durable-rows + waste-rollup as Fabrication-AUTHORED demands + seam residuals, never composed upstream contracts.
- **Signature-lock against the geometry DECISION `FAB:NN` anchors** (upstream-bindings §B), not the drifted `GBRIEF` line pins (+14..+16). E14 "spelling-lock PENDING" is RESOLVED.

---

## [01] — V1 NAMESPACE HAZARD RULING (blocks folder=namespace ratification of `Process/`)

`owner.md:39` declares `namespace Rasm.Fabrication.Process`; `family.md:98` declares TYPE `Process` (the removal-physics `[SmartEnum<string>]`). The `[V1]` floor homes owner AND family both into `Process/` → both declare `Rasm.Fabrication.Process`, and any downstream `Rasm.Fabrication.*` file referencing the bare `Process` type hits a **CS0118** shadow (the enclosing-namespace member `Rasm.Fabrication.Process` outranks a `using`-imported type in C# simple-name lookup). Already latent: `program.md:28,30,142,143,150` reference bare `Process`.

**RULING: rename the two suffix-less axis types to the category-suffix convention the sibling axes already set** — `Process → ProcessKind`, `Machine → MachineKind` (aligning with `RemovalModality`/`KinematicClass`/`HoldingClass`/`CutStrategy`/`PostDialect`, all category-suffixed). No type named `Process` or `Machine` survives, so CS0118 is structurally impossible. This is principled regularization, NOT suffix-drift: `ProcessKind`/`MachineKind` ARE the more-precise canonical names for closed vocabularies, and the two collided types are the ONLY axes lacking a category suffix. `FabricationInput` FIELD names `Process`/`Machine` stay (property names never collide with namespaces), so `input.Process : ProcessKind` reads naturally. All four `Process/` pages ratify to `Rasm.Fabrication.Process` (owner atoms + entry, family axes, physics, faults) — folder=namespace 1:1 holds, the atoms/entry/rail being genuine Process-folder residents downstream planes reach via `using Rasm.Fabrication.Process;`. Ripples: `owner.md`, `family.md`, `physics.md`, `motion.md`, `program.md`, `workholding.md` mechanical type-rename (greenfield, no shim).

---

## [02] — FOLDER / PAGE-SET TABLE (33 rows · both action columns)

Columns: **PATH** | **SEMANTIC → ENGINE LOWERING** (`kind` new/rebuild/improve; `absorb {into,from}`; `deletePages`) | **OWNER CHARTER** | **NAMESPACE** | **ENTRY GROWTH** (only where the page grows the `Fabrication.Run` entry / `FabricationPolicy`/`FabricationResult` / `FabricationInput`) | **SEAM IN/OUT** (anchor per seam; full ledger in [05]) | **WAVE**.

### Process/ — the entry, axes, physics, rail (ns `Rasm.Fabrication.Process`, all 4 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Process/owner.md` | KEEP+grow → `improve` | The ONE entry. `FabricationPolicy`/`FabricationResult` unions + shared atoms; `owner#atoms` mints the widened `Loop(…,Arr<double> Bulges)`+`BulgeAt`, `ProjectionDir` (inversion fix), `ResidualStock`, per-setup `StockSnapshot`, the egress content-key seed; `owner#run` the terminal fold. | `FabricationPolicy += {Additive, Verify, Inspect}`; `FabricationResult += {Build, Removal, Inspection}`; `Cam.ToolRadius:double → CutterForm Form`; `FabricationInput += {Option<ResidualStock> Residual, Seq<StockSnapshot> Snapshots}`. Entry unchanged: `Fin<FabricationResult> Run(FabricationPolicy, FabricationInput)`. | OUT `owner#run →` {Documentation/projection, Toolpath/motion, Nesting/nfp, Additive/production, Verify/removal, Verify/probing}. `owner#atoms →` (upstream-only, NO compose edges). IN `← Rasm/Numerics/predicates`, `← Rasm/Domain/identity ContentHash.Of`. | 1 |
| `Process/family.md` | REBUILD → `rebuild` | The axes. `PostDialect` gains GRAMMAR-family columns (family/canned-cycle/macro-subprogram/WCS-roster/cutter-comp/arc-mode/block-cap/decimal-modal/code-override); `CutStrategy` gains dimensionality (2.5D\|3D-surface\|multi-axis) + axis-count admission; `KinematicClass` deepens to rotary topology; `RemovalModality` holds erosion. `Process→ProcessKind`, `Machine→MachineKind`. | — (axis is `FabricationInput` policy data; no new entrypoint). | OUT `→` {owner, physics, Tooling/cuttingdata, Toolpath/motion, Kinematics/machine, Posting/program, Posting/dialect, Tooling/magazine, Fixturing/workholding, Fixturing/setups}. | 1 |
| `Process/physics.md` | REBUILD → `rebuild` | Physics identity. ONE `Material` identity carrying `Map<RemovalModality, ModalityPhysics>`; `Budget` modality-dispatches; `RemovalBudget.Erosion/Resin/Powder` land; the `stainless`/`stainless-abrasive` fragmentation collapses; `Overrides=Empty` DELETES; `90.0` demotes behind the table; `CuttingData` MOVES OUT to `Tooling/cuttingdata`. | — | IN `← Rasm.Materials/Properties` (raw-double AEC-peer boundary, NOT a reference). OUT `→` Toolpath/motion (RemovalBudget), Tooling/cuttingdata (partial content move). | 1 |
| `Process/faults.md` | KEEP+payloads → `improve` | The 2700 rail. Every arm retypes `string Detail → typed payload` (`Gouge→point+tool`, `Collision→zone`, `InadmissiblePair→(RemovalModality,CutStrategy)`, `NoFit→part+rotations`); new-plane arms at `+11..`; 25xx purge closes. | `FabricationFault` grows arms per [04] fault-registry (below). | IN `← Rasm.Element/Projection FaultBand` (registry), `← Rasm.Geometry GeometryFault` (band-2400 DegenerateInput). | 1 |

### Tooling/ — ISO-13399 tool intelligence (ns `Rasm.Fabrication.Tooling`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Tooling/magazine.md` | MOVE → `new` @dest + `deletePages[Process/magazine.md]` `absorb{into:Tooling/magazine.md, from:Process/magazine.md}` | MTConnect `CuttingToolAsset` (`ToolAssembly`, `Schedule` life-split, `HolderEnvelope`). The typed `CutterForm` axis (flat/ball/bull/taper/drill/chamfer/thread-mill; dia/corner-radius/taper-angle/flute-length) PROJECTED from the ISO-13399 measurement set already carried. `Runout` reads the real measurement (not `0.01`/`0.005`). `GenerateHash → ContentHash.Of` reconcile. | — | IN `← MTConnect.NET-Common CuttingToolAsset`, `← Rasm.AppHost` (decoded tool-life telemetry, transport is livewire's). OUT `→` Toolpath/guard (HolderEnvelope), Fixturing/workholding (holder), **Posting/program (`Schedule` — the E2 fix, WIRED)**, Toolpath/surface + Verify/removal + guard + cuttingdata (`CutterForm`). | 1 |
| `Tooling/cuttingdata.md` | NEW → `new` (absorbs `Process/physics` CuttingData table, partial content migration — not a page delete) | Kienzle unit-cutting-force `kc` + chip-thickness exponent + per-op surface-speed + feed-per-tooth columns keyed by the `physics` `Material` identity; a CSV/QIF data-INGRESS arm (honest seed DATA behind ingress, never a formula pretending measured); `CutterForm`-reading effective-diameter speed derivation on ball/taper engagement. | — | IN `← Process/physics Material`, `← Tooling/magazine CutterForm`. OUT `→` Posting/program (`kc` deflection column; `0.0012` literal dies). | 1 |

### Geometry2D/ — the 2D substrate (ns `Rasm.Fabrication.Geometry2D`, 2 pages · SPLIT from Polygon/clipper)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Geometry2D/algebra.md` | SPLIT → `new` (`absorb{into:Geometry2D/algebra.md, from:Polygon/clipper.md}`) | Clipper2 line-space owner: `InflatePaths`/`DeltaCallback64` variable offset/Boolean clip/`MinkowskiSum`+`Diff`/open-path clip; the `ReuseableDataContainer64` scanbeam-reuse stacking law. | IN `← Clipper2Lib`. OUT `→` nfp, guard, motion, workholding, slicing, program (2D substrate). | 2 |
| `Geometry2D/arcs.md` | SPLIT → `new` (`absorb{into:Geometry2D/arcs.md, from:Polygon/clipper.md}`) | CavalierContours arc-space, the `[V10]a`-RULED Fabrication-stratum kerf lane over `ArcAlgebra`: pure kerf compensation ON TOP of the kernel corner-row/medial (never a second manifest). `PlineOffset.ParallelOffset<O,T>`, island-preserving `Shape<T>.FromPlines(loops).ParallelOffset`, `FindPointAtPathLength`, `StaticAABB2DIndex`. **`result.Positive` phantom → `result.PosPlines[].Pline`** (`clipper.md:221` fix per `api-cavaliercontours.md:47`). | IN `← CavalierContours.*`, `← Rasm/Meshing/offset` (kernel corner-row region offsets composed on top). OUT `→` program (kerf/lead/adaptive arc-native offsets). | 2 |

### Ingress/ — everything entering as geometry (ns `Rasm.Fabrication.Ingress`, 3 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Ingress/profile.md` | MOVE → `new` @dest + `deletePages[Polygon/import.md]` `absorb{into:Ingress/profile.md, from:Polygon/import.md}` | ACadSharp read-only DXF/DWG ingress; the dead `warnings` accumulator wires the strict-mode `NotificationType.Error` escalation (claim-deletion FORBIDDEN — loses ingress-degradation visibility); netDxf rejection frame re-writes (nothing to reject); one polymorphic `Admit` → kernel-admissible `Loop`. | IN `← Rasm.Bim/Exchange` (ACadSharp read codec, central pin; write is AppUi's). OUT `→` nfp (part library), program (profile program). | 2 |
| `Ingress/solid.md` | NEW → `new` | OcctNet.Wrapper B-rep manufacturing ingress: `ImportStep(AP203/214/242)`/`ImportIges`/`ImportStl → OcctShape → Triangulate → OcctMesh`; the tessellated `OcctMesh` crosses to kernel `MeshSpace` at the seam, dirty STL routed through the kernel `HealOp` rail; the `SCOPE_LIMIT` (single-shape, `libTKHLR`/`libTKXCAF` unbound) is a standing demand row re-verified each leg. | IN `← OcctNet.Wrapper` (native OCCT 7.9.3 dynamic-link). OUT `→ Rasm/Meshing/mesh` (MeshSpace admission), `→ Rasm/Processing/repair` (HealOp); feeds slicing/surface/projection as admitted kernel vocabulary. | 2 |
| `Ingress/steel.md` | NEW → `new` | DSTV.Net NC1 record tree READ (`ST` header + `BO`/`SI`/`SC`/`AK`/`IK`/`KA` blocks); Mapperly generated `DSTV-record → Loop` projection; the `KA` bend rows seed the sheet-metal IDEAS lane. | IN `← DSTV.Net`, `← Riok.Mapperly` (generated projection). OUT `→` nfp/program (steel profiles), `→ Posting/dialect` (the same record tree as the NC1 EMIT model). | 2 |

### Toolpath/ — subtractive CAM (ns `Rasm.Fabrication.Toolpath`, 5 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Toolpath/motion.md` | REBUILD → `rebuild` | Real `(RemovalModality, CutStrategy)` generator arms — every stub dies: `Turn` a true ZX radial sweep over a real revolved stock envelope (kernel `Analysis/measure Bounds` enclosing-cylinder fit), `helical` a distinct thread/ramp, `Peck` real canned-cycle geometry w/ dwell-retract, `SliceWalk` a real perimeter/infill/travel orderer over the real `SliceLayer`. Engagement bound reads the medial clearance field AND the physics chip-load budget on one policy row. **`Guard.Check` wired per committed feed move** (E2 fix). | Lowers `FabricationPolicy.Cam`. | IN `← family` (Modality/Admits), `← physics/cuttingdata` (chip-load), `← Toolpath/skeleton` (clearance field), `← Additive/slicing` (SliceLayer), `← Kinematics/cell` (RobotProgram), `← Toolpath/surface` (3D arms). OUT `→ Toolpath/guard` (Check per feed move), `→ Fixturing/workholding` (Condition). | 3 |
| `Toolpath/surface.md` | NEW → `new` | **OpenCAMLib** drop/push/waterline/adaptive-waterline cutter POSITIONING via a thin in-house `extern "C"` C-shim + `[LibraryImport]` source-generated P/Invoke (probe FEASIBLE; ~18-24 blittable fns over the `Operation` lifecycle; `CutterForm` maps 1:1 onto `Cyl/Ball/Bull/Cone/Composite`). Path LAYOUT composed from the kernel geodesics/extract isolines + flow streamlines + segment fields (a Fabrication-side on-mesh re-implementation is the `[V2]` defect). waterline/scallop/pencil/flowline/rest/3+2/swarf/thread-mill rows. | — (composed within `Cam`). | IN `← libocl` (NATIVE, RID osx-arm64/win-x64/linux-x64, ALC-firebreak/sidecar, content-keyed, golden-fixture gated), `← Rasm/Processing/{geodesics,extract,flow,segment}`, `← Tooling/magazine CutterForm`, `← Spec/tolerance` (Ra→scallop stepover). | 3 |
| `Toolpath/partition.md` | NEW → `new` | **SharpVoronoiLib** `VoronoiPlane` Fortune sites + Lloyd `Relax(iterations,strength,reTessellate)`: pocket-region decomposition seeds, `VoronoiSite.Centroid` spiral-pocket seeds, stipple/engrave/even-spacing strategy rows. Point-site only (polygon medial stays the kernel's). | — | IN `← SharpVoronoiLib` (`Aliases="Voronoi"`). OUT `→` motion (partition strategy rows). | 3 |
| `Toolpath/guard.md` | KEEP+wire → `improve` | Per-move design-time swept Minkowski envelope; distinct part-keep/stock-keep-out verdicts; `Lift` retract. Interior unchanged; the 3 out-seams declared; the stock keep-out reads the CURRENT `StockSnapshot`, never the raw blank. | `FabricationFault.Gouge/Collision` (typed payloads). | IN `← Toolpath/skeleton` (ClearanceAt/Clearance-radius), `← Fixturing/workholding` (ExclusionZone), `← Tooling/magazine` (HolderEnvelope), `← Rasm/Spatial/index` (BVH). | 3 |
| `Toolpath/skeleton.md` | REBUILD → `rebuild` | **Kernel-medial CONSUMER.** `Toolpath/Skeleton.cs` Wavefront/`Propagate`/`OffsetAt` dies UNCONDITIONALLY (`[V10]d`); reads `Offsetting.Apply(OffsetOp,Op?)→Fin<OffsetResult>` per-point clearance RADIUS + `Clearance(Point3d probe)`, `Skeletonize.Apply(SkeletonOp,Op?)→Fin<CurveSkeleton>`; keeps ONLY the trochoidal constant-engagement WALK over the kernel clearance field. `[03]-[RESEARCH]` tail + false author-forever seal purge. | — | IN `← Rasm/Meshing/offset` (clearance-radius family), `← Rasm/Meshing/skeleton` (CurveSkeleton). OUT `→` motion (WALK), guard (ClearanceAt). | 2 |

### Kinematics/ — motion topology (ns `Rasm.Fabrication.Kinematics`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Kinematics/cell.md` | MOVE → `new` @dest + `deletePages[Toolpath/kinematics.md]` `absorb{into:Kinematics/cell.md, from:Toolpath/kinematics.md}` | Robots cell held (per-manufacturer RAPID/KRL/URScript/VAL3/DRL posts, `Program` look-ahead, `extern alias R3` boundary); the two-`Rhino.Geometry` seam owned by generated `Riok.Mapperly` partials (hand-written copyists die); band-2500→2700; `[03]-[RESEARCH]` tail purge. | IN `← Robots → Rhino3dm (R3)`, `← Riok.Mapperly`. OUT `→` motion (RobotProgram.Solve). | 3 |
| `Kinematics/machine.md` | NEW → `new` | 5-axis machine-tool topology: `KinematicClass` rotary rows (table-table trunnion\|head-head\|head-table\|nutating) so `mill-5axis` stops binding `CartesianGantry`; the rotary-axis inverse + TCP/RTCP admission; the ONE shared jerk/accel motion-dynamics law posting `Lookahead` reads (typed policy shape, so the two look-ahead planners cannot drift); multi-axis orientation interpolation composes the kernel `MotionInterpolation` one-slerp owner (never a second slerp). | IN `← family KinematicClass`, `← Rasm/Parametric/projections MotionInterpolation`. OUT `→ Posting/program` (motion-dynamics policy rows), `→ Kinematics/cell` (Robots maps at R3 edge). | 3 |

### Additive/ — production 3DP (ns `Rasm.Fabrication.Additive`, 3 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Additive/slicing.md` | MOVE+REBUILD → `new` @dest + `deletePages[Toolpath/slicing.md]` `absorb{into:Additive/slicing.md, from:Toolpath/slicing.md}` | FFF/DED planar owner CONSUMING the kernel `Slicing.Apply(SliceOp,Op?)→Fin<SliceStack>` (4 layer policies + 5-channel SoA forest); `Section`/O(n²)`Chain` DELETE; gyroid/TPMS routes to `Additive/implicit` voxel lane (the `InfillPattern` false-collapse dies); adaptive/variable-by-slope layer height as kernel slice-stack policy rows (demand landed upstream `[V10]b`); gsSlicer prior-art enrichment. `[03]-[RESEARCH]` tail purge. | — | IN `← Rasm/Meshing/slice SliceStack`. OUT `→` motion (SliceLayer), production (layers). | 3 |
| `Additive/implicit.md` | NEW → `new` | **PicoGK** to depth: `IImplicit→Voxels` TPMS/gyroid/cellular conformal infill; `Lattice.AddBeam` overhang-conditioned supports; SLA/DLP/MSLA grayscale + `.cli` vector layer stacks (`oVectorize→CliIo`, `Vdb2Cli`); voxel offset/shell lightweighting. **Declares the net9+native ALC-firebreak/sidecar posture ONCE** + the content-keyed mesh↔`Voxels` wire (`Verify/removal` composes this seam, never a second posture). | `FabricationPolicy.Additive` sub-fold. | IN `← PicoGK` (net9/native, `SkiaSharp` on AppUI row), `← CommunityToolkit.HighPerformance` (Span2D grayscale layers). OUT `→` production (Lattice→beam-lattice), Verify/removal (Voxels ALC seam). | 3 |
| `Additive/production.md` | NEW → `new` | Build-orientation optimization (author-kernel over overhang/bottom-area/contour objective reading the kernel `Analysis/select Faces`/`Curves.Draft` per candidate orientation, never a hand-rolled normal classifier; Tweaker-3 REFERENCE); machine profiles (build volume/nozzle/vat/laser/kinematics as rows); **lib3mf** 3MF core/production/beam-lattice egress (vendored official binding + RID native asset; PicoGK `Lattice`→beam-lattice extension; golden-fixture gated; content-keyed; STL-implied hand-off dies). | Lowers `FabricationPolicy.Additive → FabricationResult.Build`. | IN `← lib3mf` (NATIVE vendored), `← Additive/implicit` (Lattice), `← Additive/slicing` (layers), `← Rasm/Analysis/select`. OUT `→` (3MF/`.cli`/FFF egress, content-keyed). | 3 |

### Nesting/ — layout + yield (ns `Rasm.Fabrication.Nesting`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Nesting/nfp.md` | KEEP+identity fix → `improve` | NFP true-shape engine held. `Stock.Of()` hashes discriminant + ALL dimensions through `ContentHash.Of` (area-only collision poisoning `Remnant.Parent` dies); `Ty*1e6+Tx → (Ty,Tx)` tuple comparison; the origin-vs-`Anchor` reference-frame incoherence PROVEN/fixed (candidates translated by placed `(pl.Tx,pl.Ty)`, single frame); the 3 raw `XxHash128` mints route the ONE `ContentHash.Of` entry; `RectpackSharp` fast-path re-homes to `RectangleBinPack.CSharp` `MaxRectsBinPack` heuristic-sweep. | IN `← Geometry2D/{algebra,arcs}`, `← RectangleBinPack.CSharp` (fast-path), `← Rasm/Domain/identity ContentHash.Of`, `← Rasm/Processing/flatten` (ChartAtlas). OUT `→ Rasm.Compute` (DRL score, injected delegate — BLOCKED boundary). | 3 |
| `Nesting/stock.md` | KEEP → `improve` | Rectangular yield engine (5 `RectangleBinPack.CSharp` packers) held — the scalar exemplar. `MaterialId` ledger row declared; `[05]-[RESEARCH]` tail purge; RBP central pin re-group from Materials label verified. | IN `← RectangleBinPack.CSharp`, `← Rasm.Element/MaterialId` (NOW declared, E11). OUT `→ Nesting/nfp` (NestPlan), `→ Rasm.Compute WasteAreaMm2` (FORWARD one-sided seam — CBRIEF WasteAreaMm2 REFUTED as landed). | 3 |

### Fixturing/ — keep-out + setup planning (ns `Rasm.Fabrication.Fixturing`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Fixturing/workholding.md` | MOVE+safety → `new` @dest + `deletePages[Nesting/workholding.md]` `absorb{into:Fixturing/workholding.md, from:Nesting/workholding.md}` | `Clears`/`Condition` use segment-vs-polygon intersection (the thin-diagonal-clamp miss dies); per-kind REAL footprint-shape geometry columns (two-jaw vise / revolved chuck jaw / full-bed vacuum), not a `MarginScale` scalar; `ForHolding` a TOTAL mapping (`Vise` reachable); **`Condition` WIRED into the Cam fold AND composing `Posting.Sequence`** (E2 double-fix). | `FabricationFault.Collision`. | IN `← family HoldingClass`, `← Geometry2D`. OUT `→ Toolpath/guard` (ExclusionZone), **`→ Posting/program` (Sequence — WIRED)**, `→ Toolpath/motion` (Condition). | 3 |
| `Fixturing/setups.md` | NEW → `new` (realizes `MULTI_FIXTURE_SCHEDULE`) | Multi-fixture scheduler: `QuikGraph` operation-precedence/datum-lineage graph (the NP-hard sequencing half homes here, never a second magazine rail); OWNS the setup→WCS assignment rows (setup k → the dialect's G54-G59.x/G54.1-Pn roster); op-N workholding admitted against the op-N-1 machined `StockSnapshot` (a clamp on an already-cut face is the plan-time defect the snapshot catches). | — | IN `← QuikGraph`, `← family Machine.AxisCount`, `← Verify/removal StockSnapshot` (input-carried, owner#atoms). OUT `→ Posting/dialect` (WCS rows), `← Verify/probing` (datum measurement), `← Spec/capability` (plan-time Cpk gate). | 3 |

### Posting/ — machine-code emission (ns `Rasm.Fabrication.Posting`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Posting/program.md` | REBUILD → `rebuild` | The AST grows structure with the grammar: `Seq<GWord>` gains canned-cycle nodes (typed R/Q/P slots), subprogram units (repeat counts), macro parameter slots — per-family lowering is node expansion, never string synthesis. **`CutProgram` gains its content key via `ContentHash.Of`** (`[V12]` egress). Consumes `Magazine.Schedule` (real `Tnn`/`M6`/`G43`) + `Workholding.Sequence`. Arc rail reads `Loop.Bulges`/`ArcAlgebra`; `g3.BiArcFit2` retained ONLY for genuinely line-sourced chains (kernel mesh sections) until the owned Bolton biarc fold lands. `Feedrate`/`Lookahead` math preserved, reading `Kinematics/machine`. | — | IN `← owner` (Move/Loop/Result), `← family PostDialect`, `← physics/cuttingdata` (RemovalBudget/`kc`), `← Geometry2D/arcs` (kerf/lead), `← Tooling/magazine Schedule`, `← Fixturing/workholding Sequence`, `← Kinematics/machine` (motion-dynamics). OUT `→ Rasm.Persistence` (content key — Fabrication-authored demand on the BLOCKED `[FABRICATION_PROGRAM_DURABLE_ROWS]` card). | 4 |
| `Posting/dialect.md` | NEW → `new` | The emission FOLDS the `family.md` grammar columns drive: `Emit` a generated total `Switch` over the dialect-family axis (the flat two-column render dies); canned-cycle expansion (single blocks where the family admits, expanded moves where not); macro/subprogram lowering (Fanuc macro-B `#`-vars/WHILE, M98/M99, Siemens R-params, Heidenhain Q-params); per-dialect code-override resolution (`Dwell`/`Pierce` both-G4 collapse to one command+flag); WCS render from `Fixturing/setups`; the **NC1 emit TARGET** (DSTV.Net record tree as emission model, mirroring `Ingress/steel` read); the `Parse` round-trip arm carrying the NIST RS274NGC modal-group state machine (motion/plane/distance/units/feed groups). Rows widen as seed DATA: Siemens 840D, Heidenhain TNC, Okuma OSP, Fagor, Centroid. | — | IN `← family` (grammar columns), `← Ingress/steel DSTV` (NC1 model), `← Fixturing/setups` (WCS), `← Verify/probing` (G31/G38 rows). OUT `→ program` (AST node lowering). | 4 |

### Verify/ — program-level truth (ns `Rasm.Fabrication.Verify`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | ENTRY GROWTH | SEAM IN/OUT | W |
|---|---|---|---|---|---|
| `Verify/removal.md` | NEW → `new` | **PicoGK** voxel material-removal verification: stock `Voxels` minus the accumulated swept-tool volume via `BoolSubtract` (a ball-nose sweep IS the capsule beam); typed gouge/uncut/overcut/air-cut receipts against the ACTUAL removed-stock state; the `ResidualStock` field the `[V5]` rest-machining rows consume; content-keyed per-SETUP `StockSnapshot` (never one terminal state). Composes the `Additive/implicit` DECLARED ALC seam, never a second posture. | `FabricationPolicy.Verify → FabricationResult.Removal`. | IN `← Additive/implicit` (Voxels ALC seam), `← owner#atoms` (ResidualStock/StockSnapshot). OUT `→ Fixturing/setups` (op-N-1 snapshot, input-carried), `→ Toolpath/guard` (current snapshot), `→ Toolpath/motion` (residual, input-carried — never a page edge). | 4 |
| `Verify/probing.md` | NEW → `new` (realizes `PROBING`) | Touch-probe cycle vocabulary (G31/G38 rows on the posting AST); work-offset/tool-length metrology; datum best-fit composes the kernel `Processing/register AlignKind` ICP dispatcher under one `AlignmentPolicy` (probed→nominal, never hand-rolled LS); measured-vs-nominal folds through the kernel `Analysis/measure ConformanceMetric` over `ResidualSample`; every receipt on the kernel validity fold; QIF (ISO 23952) MeasurementResults feed `Spec/capability`. Measured ingress via the SAME AppHost decoded-telemetry seam or typed manual-entry rows. | `FabricationPolicy.Inspect → FabricationResult.Inspection`. | IN `← Rasm/Processing/register ICP`, `← Rasm/Analysis/measure ConformanceMetric`, `← Rasm.AppHost` (decoded MTConnect, SECOND consumer), `← Riok.Mapperly`, `← NodaTime` (Instant). OUT `→ Posting/dialect` (G31/G38 rows), `→ Spec/capability` (QIF), `→ Fixturing/setups` (datum). | 4 |

### Spec/ — production specs (ns `Rasm.Fabrication.Spec`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Spec/tolerance.md` | NEW → `new` | Typed GD&T vocabulary — ISO 286 fits (grade/deviation tables as GENERATED rows, never enumerated), ISO 1101/ASME Y14.5 feature-control frames + datum systems, ISO 1302 surface texture (Ra/Rz + lay + process marks), MMC/LMC — QIF-aligned. **DRIVES process**: the Ra/Rz→constant-scallop-stepover derivation row `Toolpath/surface` consumes, the ISO-286 IT-grade→finishing-allowance row conditioning `[V5]` pass planning. UnitsNet overlay extends Force/Power/Temperature/Angle/Torque. (Frame DRAWING is the artifacts-plane's; this owner is the supplier.) | IN `← UnitsNet` (extended overlay). OUT `→ Toolpath/surface` (Ra→scallop, within-wave), `→ Toolpath/motion` (IT-grade→allowance), `→ Spec/capability` (grade), `→ Documentation/traveler` (spec rows). | 3 |
| `Spec/capability.md` | NEW → `new` | Process capability — Cp/Cpk/Pp/Ppk with the streaming-moment half on the kernel `Domain/stats Stat.Of`/`Distribution.Of`/`StatContext.Tolerance` (probing evidence arrives pre-folded), the distribution-fit + Monte-Carlo tolerance-stackup half over **MathNet.Numerics** (first Fabrication consumer, pin held `6.0.0-beta2`); SPC control-limit rows. The capability gate runs at PLAN time — a process row whose Cpk history cannot hold the demanded grade fails admission before material is cut. | IN `← Rasm/Domain/stats`, `← MathNet.Numerics`, `← Verify/probing` (measured ingress), `← NodaTime`. OUT `→ Fixturing/setups` (plan-time Cpk gate). | 4 |

### Documentation/ — shop documentation (ns `Rasm.Fabrication.Documentation`, 2 pages)

| PATH | SEMANTIC → ENGINE | OWNER CHARTER | SEAM IN/OUT | W |
|---|---|---|---|---|
| `Documentation/projection.md` | MOVE+REBUILD → `new` @dest + `deletePages[Posting/projection.md]` `absorb{into:Documentation/projection.md, from:Posting/projection.md}` | The BSP interior DIES for the kernel `View.Apply(ViewOp,Op?)→Fin<DrawingProjection>` {HiddenLine·Silhouette·Section·Creases} (`[V3]`); KEEP the `BooleanSolid` watertight arm (kernel `Arrangement.Apply`/`ToMesh` compose-seam); PRESERVE the `HiddenLineResult` receipt shape AppUi is insulated at; DROP the phantom `Predicate.Orient2D` import (0 fence calls). | — (composed within `HiddenLine` fold). | IN `← Rasm/Drawing/view DrawingProjection`, `← Rasm/Meshing/arrangement`, `← Rasm/Numerics/predicates`, `← Rasm/Spatial/index`. OUT `→ Rasm.AppUi/Render` (HiddenLineResult, receipt insulation). | 4 |
| `Documentation/traveler.md` | NEW → `new` | Setup sheets + shop travelers as content-keyed shop documents composing the projection views, magazine tool lists, workholding/setups plans, program facts, and spec rows — the typed document MODEL only (sheet/annotation RENDERING rides the artifacts-plane seam, `[06]`). `NodaTime.Instant` stamps. | IN `← Documentation/projection` (views), `← Tooling/magazine` (tool lists), `← Fixturing/{workholding,setups}` (plans), `← Posting/program` (facts), `← Spec/{tolerance,capability}` (rows), `← NodaTime`. | 4 |

---

## [03] — DELETE-WITH-ABSORB ROWS (engine `deletePages` + `absorb` pairs)

| deletePages (source) | absorb pairs {into, from} | Vacated folder |
|---|---|---|
| `Process/magazine.md` | {into: `Tooling/magazine.md`, from: `Process/magazine.md`} | — |
| `Polygon/clipper.md` | {into: `Geometry2D/algebra.md`, from: `Polygon/clipper.md`} · {into: `Geometry2D/arcs.md`, from: `Polygon/clipper.md`} | `Polygon/` (both leave) |
| `Polygon/import.md` | {into: `Ingress/profile.md`, from: `Polygon/import.md`} | `Polygon/` DELETED |
| `Toolpath/kinematics.md` | {into: `Kinematics/cell.md`, from: `Toolpath/kinematics.md`} | — |
| `Toolpath/slicing.md` | {into: `Additive/slicing.md`, from: `Toolpath/slicing.md`} | — |
| `Nesting/workholding.md` | {into: `Fixturing/workholding.md`, from: `Nesting/workholding.md`} | — |
| `Posting/projection.md` | {into: `Documentation/projection.md`, from: `Posting/projection.md`} | — |

7 delete rows · 8 absorb pairs. `Polygon/` is the only folder DELETED (both residents leave). Net: 5 incumbent folders (Process/Toolpath/Nesting/Posting minus Polygon) → 13.

---

## [04] — FAULT-REGISTRY TABLE (folder = fault-cluster; one 2700-century block per folder)

Landed 2701-2710 are the GRANDFATHERED core (archive DECISION fixed them — build ON, never re-band). The mechanical `folder=fault-cluster` check applies to the NEW `+11..` growth blocks, each folder a contiguous sub-block sized by the growth law. Landed cores are mapped to their producing folders (documented, not renumbered).

| Folder | Growth block | Landed core (2701-2710) mapped by PRODUCER | New growth arms |
|---|---|---|---|
| `Process/` | 2711 (reserve) | 2704 `OpenLoop`, 2705 `InadmissiblePair` | — (Process routes DegenerateInput/existing) |
| `Tooling/` | 2712-2713 | — | `ToolUnavailable`, `HolderInterference` |
| `Geometry2D/` | 2714 | 2703 `KerfCollision` | `OffsetCollapse` (arc-space degenerate) |
| `Ingress/` | 2715-2717 | — | `SolidImportUnbound`, `SteelBlockMalformed`, `ProfileDegraded` |
| `Toolpath/` | 2718-2720 | 2706 `Gouge`, 2707 `Collision` | `SurfaceSampling` (OCL), `PartitionDegenerate` (Voronoi), `EngagementExceeded` |
| `Kinematics/` | 2721-2722 | 2702 `Unreachable` | `RotarySingular` (RTCP), `MotionDynamicsExceeded` |
| `Additive/` | 2723-2725 | 2708 `NonManifoldSlice` | `VoxelOverflow`, `OrientationInfeasible`, `Lib3mfWrite` |
| `Nesting/` | 2726 (reserve) | 2701 `NoFit`, 2709 `StockOverflow`, 2710 `Nest` | — |
| `Fixturing/` | 2727-2729 | — | `ClampCollision`, `NoFeasibleSetup`, `DatumLineageBreak` |
| `Posting/` | 2730-2733 | — | `UnsupportedCycle`, `MacroExpansion`, `ParseModalGroup`, `Nc1Emit` |
| `Verify/` | 2734-2738 | — | `GougeVerified`, `Uncut`, `Overcut`, `AirCut`, `ProbeOvertravel` |
| `Spec/` | 2739-2740 | — | `UnattainableGrade`, `CapabilityGate` |
| `Documentation/` | 2741 | — | `TravelerIncomplete` |

All within the 2700 century (max 2741). Every new arm carries its typed payload (never bare `string Detail`). Escalation to a federation-wide renumber fires ONLY if `+11..` overflows 2799 (`[06]`) — it does not.

---

## [05] — SEAM LEDGER (both directions · two-node owner rule · acyclic)

`owner` evaluates as TWO nodes: `owner#atoms` (upstream-only source of `Loop`+`Bulges`, `ProjectionDir`, `ResidualStock`, `StockSnapshot`, `ArcCenter`, `Move`, `FabricationInput`/`Policy`/`Result` — NO compose edges) and `owner#run` (terminal, compose edges ONLY). Cross-plane RESULT data rides the entry vocabulary minted on `owner#atoms`, so the motion→removal→program→motion type cycle NEVER forms.

### Cross-package edges (corrected; every stale target re-pointed)

| # | Edge | Direction | Kind | Counterpart / correction |
|---|---|---|---|---|
| S1 | `Documentation/projection ← Rasm/Drawing/view` | in | PROJECTION | `View.Apply(ViewOp,Op?)→Fin<DrawingProjection>` {HiddenLine·Silhouette·Section·Creases}; DECISION row 27; supersedes the BSP (ARCH:46 "beside" clause dies). |
| S2 | `Documentation/projection → Rasm.AppUi/Render` | out | RECEIPT | `HiddenLineResult` PRESERVED; AppUi insulated at supersession (UIBRIEF `[V7]`(b) — NOT `[V6]`; ARCH:75-76 ⇄ this row). |
| S3 | `Documentation/projection ← Rasm/Meshing/arrangement` | in | WIRE | `Arrangement.Apply(ArrangementOp,Op?)→Fin<ArrangementResult>`+`BooleanOp`; DECISION row 9; re-points ARCH:44 `Processing/repair`. |
| S4 | `Documentation/projection ← Rasm/Numerics/predicates` | in | WIRE | `Predicate.Orient2D/Orient3D` (ARCH:42). |
| S5 | `Documentation/projection ← Rasm/Spatial/index` | in | SHAPE | SpatialIndex BVH occluder prune (ARCH:45). |
| S6 | `Additive/slicing → Rasm/Meshing/slice` | in* | WIRE | `Slicing.Apply(SliceOp,Op?)→Fin<SliceStack>`; DECISION row 17; REALIZES ARCH:48 "when realized"; layer-policy demand landed `[V10]b`. |
| S7 | `Toolpath/skeleton ← Rasm/Meshing/offset` + `Rasm/Meshing/skeleton` | in | WIRE | `Offsetting.Apply→Fin<OffsetResult>` clearance-RADIUS + `Clearance(Point3d)`; `Skeletonize.Apply→Fin<CurveSkeleton>`; DECISION rows 10,18; `[V10]d`; `Skeleton.cs` dies. |
| S8 | `Geometry2D/arcs ← Rasm/Meshing/offset` | in | WIRE | kernel corner-row `Offset(join×end)` composed ON TOP over CavalierContours; `[V10]a`; DECISION row 10 + V10 line 149. |
| S9 | `Toolpath/surface ← Rasm/Processing/{geodesics,extract,flow,segment}` | in | WIRE | heat distance / `ContourPolicy.MeshScalar` isolines / streamlines / direction fields for path LAYOUT; `[V5]`. |
| S10 | `Toolpath/surface → libocl (OpenCAMLib)` | out | NATIVE | C-shim + `[LibraryImport]`; RID osx-arm64/win-x64/linux-x64; ALC-firebreak/sidecar; content-keyed; golden-fixture gated (NEW admission). |
| S11 | `Ingress/solid → Rasm/Meshing/mesh` + `Rasm/Processing/repair` | out | WIRE | `OcctMesh` → kernel `MeshSpace` admission; dirty STL → kernel `HealOp`; `[03]`. |
| S12 | `Ingress/solid → OcctNet.Wrapper` | in | NATIVE | ImportStep/Iges/Stl → OcctShape → OcctMesh; native OCCT 7.9.3 dynamic-link; SCOPE_LIMIT demand row. |
| S13 | `Ingress/profile ← Rasm.Bim/Exchange` | in | SHAPE | ACadSharp read codec, central pin; netDxf reframed (write is AppUi's ACadSharp `DxfWriter`+`DwgWriter`); ARCH:52 re-point; ⇄ Bim Exchange/format. |
| S14 | `Ingress/steel → DSTV.Net` | in | WIRE | NC1 record tree read; `Riok.Mapperly` record→Loop projection (NEW consumer). |
| S15 | `Process/physics ← Rasm.Materials/Properties` | in | WIRE | Thermal/Density scalars as RAW DOUBLES at AEC-peer boundary (NOT a reference); reconcile `physics.md:3` anchor vs `ARCH:49`. |
| S16 | `Tooling/magazine ← Rasm.AppHost` (decoded MTConnect) | in | WIRE | mid-job tool-life reload; transport livewire's; APPHOST `[04]` MTConnect.NET row (FIRST consumer). |
| S17 | `Verify/probing ← Rasm.AppHost` (decoded MTConnect) | in | WIRE | measured-feature/work-offset observations; APPHOST `[04]` (SECOND consumer). |
| S18 | `Verify/probing ← Rasm/Processing/register` | in | WIRE | `AlignKind` ICP dispatcher under `AlignmentPolicy`; `[V7]`. |
| S19 | `Verify/probing ← Rasm/Analysis/measure` | in | WIRE | `ConformanceMetric` over `ResidualSample`; `[V7]`. |
| S20 | `Spec/capability ← Rasm/Domain/stats` | in | WIRE | `Stat.Of`/`Distribution.Of`/`StatContext.Tolerance` streaming moments; `[V8]`. |
| S21 | `Spec/capability → MathNet.Numerics` | in | WIRE | distribution-fit + Monte-Carlo stackup; first Fabrication consumer; pin held `6.0.0-beta2` (Compute `Integrate.OnCuboid` hold; "5.0.0 downgrade" SUPERSEDED). |
| S22 | `Nesting/stock ← Rasm.Element/MaterialId` | in | SHAPE | wired-undeclared → NOW declared (stock.md:40, E11). |
| S23 | `Nesting/stock → Rasm.Compute` (WasteAreaMm2) | out | PROJECTION | **FORWARD one-sided seam** — `CBRIEF [V12]` WasteAreaMm2 REFUTED as landed; un-named Compute-side; a Fabrication-authored demand, not a composed binding (ARCH:51 demoted). |
| S24 | `Nesting/nfp → Rasm.Compute` (DRL score) | out | POLICY | injected `Func<NoFitPolygon,PartTransform,double>` delegate (BLOCKED, strata-correct). |
| S25 | `Posting/program → Rasm.Persistence` (content key) | out | WIRE | `CutProgram` content key via `ContentHash.Of`; the `ArtifactKind`/durable-rows fold is a **Fabrication-AUTHORED demand + BLOCKED cross-package residual** on `[FABRICATION_PROGRAM_DURABLE_ROWS]` (PBRIEF `[V12]` ArtifactKind REFUTED as landed; 2701-2710 decode constraint recorded). NOT a composed upstream contract. |
| S26 | `Rasm/Drawing/pack` (toolpath PackKind) | in | ENCODING | `Encode.Apply(PackOp,Op?)→Fin<EncodedGeometry>` `PackKind.toolpath`; DECISION row 28; channel-set alignment is a ledger row, Compute decodes; never a Fabrication-side encoder. |
| S27 | `* → Rasm.Element/Projection` | out | CONTRACT | future `FabricationProjector:IElementProjection` (queued). |
| S28 | `* → Rasm` | in | SHAPE | Matrix/Point3d/Vector3d/MeshSpace. |

\*S6 direction "in" = Fabrication CONSUMES the kernel (arrow reads "composes").

### Intra-package web (declared; the executing fold exists)

The `[V3]` WIRED-PIPELINE realized: `Run → Cam → {Guard.Check per feed move, Workholding.Condition, Magazine.Schedule} → Post` (acyclic). Declared edges: `family →` {every axis consumer}; `Tooling/magazine →` {guard, workholding, **program (Schedule)**}; `Toolpath/skeleton →` {motion, guard}; `Fixturing/workholding →` {guard, **program (Sequence)**, motion}; `Toolpath/motion →` {guard (Check), workholding (Condition), skeleton, slicing, surface, cell}; `Fixturing/setups →` {dialect (WCS)}; `Verify/removal →` {setups, guard, motion — all via input-carried `owner#atoms` fields, never page edges}; `Spec/tolerance →` {surface (Ra→scallop, within-wave), motion, capability}; `Documentation/traveler →` {projection, magazine, workholding, setups, program, tolerance, capability}. The `ProjectionDir` inversion is fixed: it mints on `owner#atoms` (not `Documentation/projection`), so `owner#atoms → projection` becomes upstream-only and `owner#run → projection` terminal — no bidirectional cycle.

---

## [06] — ROSTER DELTA TABLE (24 dispositions · with .api obligations)

| # | Package | Action | Home page(s) | .api obligation | Manifest motion |
|---|---|---|---|---|---|
| 1 | `PicoGK` | KEEP (mandate) | `Additive/implicit` (lanes 1,2), `Verify/removal` (lane 3) | `api-picogk.md`: DROP the `geometry3Sharp DMesh3` permanent-owner coupling (`:5,113,154`) → owned biarc fold + kernel mesh vocab | held (`PROPS:84`) |
| 2 | `OcctNet.Wrapper` | KEEP (mandate) | `Ingress/solid` | `api-occtnet-wrapper.md`: re-point `:137 Rasm.Persistence/Schema` → `ContentHash.Of` egress; SCOPE_LIMIT demand row re-verified each leg | held (`:83`) |
| 3 | `DSTV.Net` | KEEP (mandate) | `Ingress/steel` (read), `Posting/dialect` (NC1 emit model) | `api-dstv-net.md`: purge band-2500 (`:3,126`)→2700; purge netDxf (`:132,143`) | held (`:79`) |
| 4 | `SharpVoronoiLib` | KEEP (mandate) | `Toolpath/partition` | `api-sharpvoronoilib.md`: slim dual-ownership prose (`:3,18`) → Fabrication-sole | held (`:87`; float-lane re-group **LANDED**) |
| 5 | `OpenCAMLib` | **ADD** (probe FEASIBLE) | `Toolpath/surface` | `api-opencamlib.md` **NEW**: `Operation` lifecycle, cutter-form ctors, the ~18-24-fn C-shim ABI, RID/libomp asset map, LGPL-2.1 dynamic-link note | ADD to Fabrication label; RID assets recorded; golden-fixture gated; `USE_OPENMP` build-policy row |
| 6 | `lib3mf` | **ADD** (BSD-2) | `Additive/production` | `api-lib3mf.md` **NEW**: 3MF core/production/beam-lattice writer, vendored-binding + RID native-asset mechanics, golden-fixture | ADD to Fabrication label; vendored binding (not on NuGet) |
| 7 | `CavalierContours` | KEEP (`[V10]a` stratum) | `Geometry2D/arcs` | `api-cavaliercontours.md`: `PosPlines` authority HOLDS (drives the `result.Positive`→`PosPlines[].Pline` fix) | float-lane re-group **LANDED** (`:77`) |
| 8 | `Clipper2` | KEEP (line-space) | `Geometry2D/algebra` | `api-clipper2.md`: clean | float-lane re-group **LANDED** (`:78`) |
| 9 | `RectangleBinPack.CSharp` | KEEP (the ONE rect engine) | `Nesting/stock` (5 packers), `Nesting/nfp` (MaxRects fast-path) | `api-rectanglebinpack-csharp.md`: subset-superset proof holds | re-group from Materials label (`:167`) → Fabrication PENDING |
| 10 | `RectpackSharp` | **REMOVE** (redundancy) | (nfp fast-path re-homes to RBP) | `api-rectpacksharp.md` **DELETE**; RAIL_LAW re-homes | REMOVE from Fabrication label (`:85`); `README:65` rewrite |
| 11 | `geometry3Sharp` | REPLACE-candidate | `Posting/program` (owned Bolton biarc fold for line-sourced chains only) | `api-geometry3sharp.md`: `:106` "dropped CavalierContours" self-contradiction dies | held (`:81`); leaves when the owned fold lands + Bim drops its mesh-importer (cross-package residual `Rasm.Bim.csproj:22`) |
| 12 | `MTConnect.NET-Common` | KEEP (model-only) | `Tooling/magazine`, `Verify/probing` | `api-mtconnect-net-common.md`: `GenerateHash → ContentHash.Of` reconcile (`:3,116`); re-point `:119 Persistence/Schema` | held (`:82`) |
| 13 | `Robots` | KEEP (cell) | `Kinematics/cell` | `api-robots.md`: band-2500→2700 (`:5,94`) | held (`:86`) |
| 14 | `ACadSharp` | KEEP (read-only) | `Ingress/profile` | `api-acadsharp.md`: `:120` stale write-leg → AppUi ACadSharp two-format leg | held (BIM label `:122`) |
| 15 | `netDxf` | VERIFY-REMOVE | (none — AppUi's `[V7]`) | — | VERIFY-REMOVE surviving orphan at this leg (`:359`) |
| 16 | `netDxf.netstandard` | VERIFY-REMOVE | (none — Compute FEALiTE2D floor) | — | stays a transitive floor if FEALiTE2D parity-kept (`:470`); not Fabrication residue |
| 17 | `MathNet.Numerics` | KEEP (shared, first Fab consumer) | `Spec/capability` | shared tier | **held `6.0.0-beta2`** (Compute `Integrate.OnCuboid`; "5.0.0 downgrade" SUPERSEDED) |
| 18 | `Riok.Mapperly` | KEEP (shared mine) | `Kinematics/cell`, `Tooling/magazine`, `Verify/probing`, `Ingress/steel` | `api-mapperly.md` (shared, VERIFY) | held (Foundational Core `:30`) |
| 19 | `System.Numerics.Tensors` | KEEP (shared mine) | `Toolpath/surface`+`motion`, `Nesting/nfp`, `Spec/capability` | `api-tensors.md` (shared) | held (`:42`) |
| 20 | `CommunityToolkit.HighPerformance` | KEEP (shared mine) | `Additive/implicit`, `Verify/removal`, `Toolpath/surface` | `api-highperformance.md` (shared) | held (`:38`) |
| 21 | `NodaTime` | KEEP (shared mine) | `Documentation/traveler`, `Verify/probing`, `Tooling/magazine`, `Spec/capability` | `api-nodatime.md` (shared) | held (`:39`) |
| 22 | `QuikGraph` | KEEP (shared mine) | `Fixturing/setups` | shared tier | held (`:41`) |
| 23 | `System.IO.Hashing` | KEEP (via `ContentHash.Of` ONLY) | all egress | `api-hashing.md`: SLIM to the `api-unitsnet.md` thin-overlay pattern; `:3,88` "no shared tier" FALSE; name `ContentHash.Of` as the ONE mint site | held (`:31`) |
| 24 | `UnitsNet` | KEEP (overlay extend) | cut-parameter boundary + `Spec/tolerance` | `api-unitsnet.md`: extend Force/Power/Temperature/Angle/Torque | held (`:43`) |

.api authoring: **2 NEW** (`api-opencamlib.md`, `api-lib3mf.md`), **1 DELETE** (`api-rectpacksharp.md`), **9 re-point/slim** (hashing, sharpvoronoilib, picogk, occtnet, mtconnect, dstv-net, robots, acadsharp, geometry3sharp). REFERENCE-only records unchanged (`[04]` list: gsGCode/gsSlicer/MillSimSharp/CAMotics/Prusa/Cura/libslic3r/FreeCAD/Tweaker-3/UVtools/PySLM/QIF/NIST/E3Studio).

---

## [07] — VERDICT DISPOSITION (V1-V10, all disposed)

| V | Disposition in this blueprint |
|---|---|
| **V1** FOLDER_PARTITION | 13 folders / 33 pages ratified folder=namespace=fault-cluster 1:1; the six ratifications execute (family/magazine leave `ProcessModel`, physics leaves `ProcessPhysics`, faults leaves root, import leaves `Geometry2D`→`Ingress`, projection leaves `Projection`→`Documentation`); the **CS0118 hazard RULED** (`Process→ProcessKind`, `Machine→MachineKind`; [01]); every folder ≥2 pages; `owner` two-node split keeps the graph acyclic. |
| **V2** KERNEL_PLANE_CONSUMPTION | (a) `skeleton` consumes `Offsetting.Apply`/`Skeletonize.Apply` clearance-radius, `Skeleton.cs` dies UNCONDITIONALLY (S7); (b) `slicing.Section` → `Slicing.Apply` (S6); (c) `projection` BSP dies for `View.Apply`, `BooleanSolid` arm kept, `HiddenLineResult` preserved (S1-S3); (d) offsets compose the `[V10]a` split — `ArcAlgebra`/CavalierContours kerf ON TOP of kernel corner-row (S8); (e) booleans/SDF kernel-consumed. All 5 `[RESEARCH]` tails purge (`rg '\[RESEARCH\]'`==0). |
| **V3** EXECUTION_WIRE | `Cam.Solve → Guard.Check` per feed move + `Workholding.Condition`; `Post ← Magazine.Schedule` (real Tnn/M6/G43); arc rail realizes (kerf/lead/adaptive read arc-native offsets; `Loop.Bulges` landed on `owner#atoms`); Fixturing hardens (segment-vs-polygon, total `ForHolding`, per-kind footprints, `Condition` composes `Posting.Sequence`); the work-offset chain wires (`Fixturing/setups` OWNS setup→WCS, posting renders it, probing reconciles). |
| **V4** POST_GRAMMAR | `family.PostDialect` gains GRAMMAR-family columns (wave-1 seed rows); `Posting/dialect` owns the emission folds (`Emit` generated total Switch, canned-cycle/macro/subprogram lowering, code-override, NC1 target, `Parse` NIST modal-group state machine); the two look-ahead planners read ONE `Kinematics/machine` motion-dynamics law; rows widen (840D/TNC/OSP/Fagor/Centroid). |
| **V5** CAM_DEPTH | `CutStrategy` widens with dimensionality + axis-count admission; `Toolpath/surface` ADMITS OpenCAMLib (probe FEASIBLE; path LAYOUT kernel-composed, cutter POSITIONING OCL); `Toolpath/partition` realizes SharpVoronoiLib; `Kinematics/machine` closes the 5-axis gap (rotary topology + TCP/RTCP); the `CutterForm` axis lands on `Tooling/magazine` (4 consumers); every stub arm becomes a real generator. |
| **V6** ADDITIVE_PRODUCTION | Additive is a FOLDER (slicing consumer / implicit PicoGK / production+lib3mf); `physics` mints Resin/Powder/DED budgets; gyroid routes to the voxel lane (InfillPattern false-collapse dies). |
| **V7** VERIFY_PLANE | `Verify/removal` (PicoGK `BoolSubtract` gouge/uncut/overcut/air-cut + ResidualStock + per-setup StockSnapshot); `Verify/probing` (G31/G38, kernel ICP + ConformanceMetric, AppHost decoded ingress, QIF→capability); `FabricationPolicy += {Verify, Inspect}`; the ALC boundary declares once on `Additive/implicit`. |
| **V8** SPEC_PLANE | `Spec/tolerance` (ISO 286/1101/1302 + ASME Y14.5, QIF-aligned, process-DRIVING derivation rows) + `Spec/capability` (Cp/Cpk over kernel Stat + MathNet fit/Monte-Carlo, plan-time Cpk gate); `Documentation/traveler` (typed document model); UnitsNet overlay extends. |
| **V9** PHYSICS_IDENTITY | ONE `Material` identity + `Map<RemovalModality,ModalityPhysics>`; `RemovalBudget.Erosion` lands; `CuttingData → Tooling/cuttingdata` Kienzle `kc`; `Overrides` deletes onto the ingress arm; `0.0012`/`90.0` demote behind the table. |
| **V10** GOVERNANCE_TRUTH | R6 25xx purge (ARCH:16, kinematics.md:3, TASKLOG:44, api-dstv-net/robots) → 2700; netDxf purge; ARCH:53,54 Schema → blobstore/cache (subject to the BLOCKED-card caveat, S25); ARCH:63 → `Fixturing/setups`; `[GUARD]-[QUEUED]` closes; api-geometry3sharp:106 dies; api-hashing slims; `nfp` Stock.Of/Ty*1e6+Tx/frame-consistency land. |

---

## [08] — EVIDENCE DISPOSITION (E1-E14, all disposed; corrections carried)

| E | Verdict | Disposition |
|---|---|---|
| **E1** dead admissions | HOLD | All four get NAMED owner pages FIRST (the lens's core discipline): PicoGK→implicit+removal, OcctNet→solid, DSTV→steel+dialect(NC1), SharpVoronoiLib→partition. Anchor-pairing DRIFT corrected (PicoGK `:20`, DSTV `:16`). |
| **E2** unwired pipeline | HOLD | The executing fold exists ([05] intra-web): Guard.Check/Workholding.Condition/Magazine.Schedule all consumed; ARCH:63 scheduler → `Fixturing/setups`. |
| **E3** arc-rail fiction | HOLD (+2 corrections) | `Loop.Bulges`+`BulgeAt`+3-arg ctor land on `owner#atoms` (wave 1); program reads them; `g3.BiArcFit2` line-sourced-only. **lnContours sub-claim REFUTED** (already `CavalierContours.*` on disk); the LIVE phantom is `result.Positive`→`PosPlines[].Pline` (S8/arcs). |
| **E4** post thinness | HOLD | V4 grammar families; `Emit` generated Switch; the dialect-invariant `Code` table dies. |
| **E5** stub generators | HOLD | Every arm a real generator (V5): Turn ZX sweep over kernel Bounds, helical distinct, Peck canned-cycle, SliceWalk real orderer. |
| **E6** physics identity | HOLD | V9 identity map + budgets; `CuttingData`→`Tooling/cuttingdata`. |
| **E7** HLR not CAD-grade | HOLD | BSP dies for kernel `View.Apply` (V2c); `HiddenLineResult` preserved; the PHANTOM `Predicate.Orient2D` import (0 fence calls, `projection.md:31`) DROPPED. |
| **E8** coverage silences | HOLD | surface/partition/verify/spec/traveler/machine/steel/solid pages; CutterForm axis; spec→plan derivation. |
| **E9** identity/logic | HOLD (+strengthened) | `Stock.Of` discriminant+dims via `ContentHash.Of`; `Ty*1e6+Tx`→tuple; the NFP frame incoherence (THREE frames, dossier-strengthened beyond "unverified") reconciled + proven; 3 raw XxHash128 mints → one entry. |
| **E10** stale governance | HOLD | All re-points in V10; ARCH:53,54 Schema carries the S25 BLOCKED-card caveat (PBRIEF ArtifactKind un-landed); `faults.md:33` "Element's 2500 band" STAYS (correct rationale). |
| **E11** ledger gaps | HOLD (+reciprocal correction) | `Nesting/stock ← Rasm.Element/MaterialId` declared (S22); intra-web declared; **ARCH:51 WasteAreaMm2 is one-sided FORWARD** (CBRIEF reciprocal REFUTED, S23). |
| **E12** redundancy/overlay | HOLD | RectpackSharp REMOVE (⊂ RBP); api-hashing slims (shared tier exists); `Runout` reads the real measurement. |
| **E13** page-craft | DRIFT (undercount) | **FIVE** `[RESEARCH]` tails purge (add `stock.md:317` + `program.md:379` to the brief's 3); false author-forever seals die. |
| **E14** upstream bindings | DRIFT+2 REFUTED | Signature-lock against DECISION `FAB:NN` (not the +14..+16 drifted GBRIEF pins); **PBRIEF `[V12]` ArtifactKind + CBRIEF `[V12]` WasteAreaMm2 REFUTED as landed** → forward demands (S23, S25); UIBRIEF `[V6]`→`[V7]`(b); the eight §B seam spellings locked. |

---

## [09] — [03] CAPABILITY-ESCALATION DELTA DISPOSITION (14 rows → target 9.5)

| Plane | Homed by |
|---|---|
| Process axes | `Process/{owner,family,faults}` (policy cases Additive/Verify/Inspect; grammar columns; dimensionality; rotary topology; typed payloads; `Loop.Bulges`; substrate rails). |
| Physics + machining data | `Process/physics` (identity map + budgets) + `Tooling/cuttingdata` (Kienzle `kc` + ingress arm). |
| Tooling | `Tooling/magazine` (wired Post; `CutterForm` projection; tool-life seam; `Runout` typed) + `cuttingdata`. |
| Geometry2D | `Geometry2D/{algebra,arcs}` two-owner split; `[V10]a` sited; arc rail consumed downstream. |
| Ingress | `Ingress/{profile,solid,steel}` — one polymorphic admit → kernel-admissible geometry at the seam. |
| Toolpath | `Toolpath/{motion,surface,partition,guard,skeleton}` — real arms; OCL positioning + kernel-composed layout; Voronoi; guard wired; kernel-medial consumer. |
| Kinematics | `Kinematics/{cell,machine}` — cell held; 5-axis topology + TCP/RTCP; one motion-dynamics law; kernel one-slerp. |
| Additive | `Additive/{slicing,implicit,production}` — slice-stack; PicoGK lane; orientation; lib3mf. |
| Nesting | `Nesting/{nfp,stock}` — identity fix; frame proof; seam re-anchors; MaterialId row. |
| Fixturing | `Fixturing/{workholding,setups}` — segment-exact; per-kind footprints; `Condition` wired; WCS-owning setups; op-N stock-snapshot admission. |
| Posting | `Posting/{program,dialect}` — grammar families + canned/macro/WCS/comp/arc-mode; NC1 target; `Parse` round-trip; content key. |
| Verify | `Verify/{removal,probing}` — voxel receipts; residual field; per-setup snapshots; probing + ICP + conformance. |
| Spec | `Spec/{tolerance,capability}` — typed vocab QIF-aligned; Cp/Cpk over kernel Stat + MathNet; process-driving derivation rows. |
| Documentation | `Documentation/{projection,traveler}` — kernel-`DrawingProjection` consumer + preserved receipt; setup-sheet/traveler owner. |

---

## [10] — CARD DISPOSITION (TASKLOG + IDEAS · 23 existing + 3 reseed)

### TASKLOG (16 cards)

| Card | Status | Disposition |
|---|---|---|
| `NFP_DRL_POLICY` | BLOCKED | STAYS BLOCKED (injected-delegate is the strata-correct shape; closes when Compute lands its side). |
| `CSG_WATERTIGHT_SILHOUETTE` | COMPLETE | STAYS; ripples to `Documentation/projection` (BooleanSolid arm kept under the kernel `View.Apply` consumer). |
| `TOOLPATH_STRATEGY_MODALITY_FACTOR` | COMPLETE | STAYS; `Toolpath/motion` rebuild deepens the `(RemovalModality,CutStrategy)` cross-product with real arms + dimensionality. |
| `POST_DIALECT_OVERRIDE_SEAM` | COMPLETE | STAYS; the override seam survives into `Posting/dialect`'s grammar-family resolution. |
| `TROCHOIDAL_ENGAGEMENT_LIMIT` | COMPLETE | STAYS; the trochoidal WALK is the ONLY skeleton survivor (kernel-medial consumer). |
| `NEST_MULTISHEET_SCHEDULE` | COMPLETE | STAYS. |
| `NEST_REMNANT_LINEAGE` | COMPLETE | STAYS; `Remnant.Of` re-frames onto `ContentHash.Of` (E9). |
| `RECTPACK_FASTPATH_ADMISSION` | COMPLETE | RE-OPENS as the RectpackSharp REMOVE ripple (fast-path re-homes to RBP `MaxRectsBinPack`); `README:65` rewrite. |
| `BIARC_ARC_EMISSION_ADMISSION` | COMPLETE | STAYS; g3 scoped to line-sourced chains only; owned Bolton fold is the geometry3Sharp exit. |
| `ACADSHARP_SPLINE_INSERT` | COMPLETE | STAYS; ripples to `Ingress/profile`. |
| `CLIPPER_INNER_FIT` | COMPLETE | STAYS; ripples to `Geometry2D/{algebra,arcs}`. |
| `FABRICATION_FAULT_BAND_DEEPENING` | COMPLETE | CARRIES stale 2505-2509 → the R6 25xx purge closes it (superseded by `FAULT_REBAND_2700`). |
| `TOOL_CUTTING_DATA_TABLE` | COMPLETE | RIPPLES: the `CuttingData` table moves to `Tooling/cuttingdata` + deepens to Kienzle `kc` (V9). |
| `CLIPPER_VARIABLE_KERF` | COMPLETE | STAYS; ripples to `Geometry2D/algebra`. |
| `FAULT_REBAND_2700` | COMPLETE | STAYS (confirms 2701-2710; the growth-block registry builds ON). |
| `STOCK_NEST_MOVE` | COMPLETE | STAYS; the `WasteAreaMm2` counterpart demoted to FORWARD one-sided (CBRIEF REFUTED); MaterialId ledger row lands. |

### IDEAS (7 cards)

| Card | Status | Disposition |
|---|---|---|
| `DRL_NEST_POLICY` | BLOCKED | STAYS BLOCKED. |
| `GUARD` | QUEUED | CLOSE **COMPLETE** (guard.md landed; the wire is the `[V3]` per-move fold). |
| `PROBING` | QUEUED | REALIZES into `Verify/probing` (`[V7]`); card closes on realization. |
| `MULTI_FIXTURE_SCHEDULE` | QUEUED | REALIZES into `Fixturing/setups` (`[V1]`); card closes on realization. |
| `CSG_SILHOUETTE` | COMPLETE | STAYS. |
| `MAGAZINE` | COMPLETE | RIPPLES: page MOVES to `Tooling/magazine`; the `G43`/`M6`/`Tnn` claim becomes REAL (Schedule wired into Post). |
| `FEEDRATE_LOOKAHEAD` | COMPLETE | RIPPLES: the jerk/accel law homes on `Kinematics/machine` as the ONE motion-dynamics policy the two planners read. |

### IDEAS reseed (3 named growth rows)

| New card | Growth axis |
|---|---|
| `SHEET_METAL_UNFOLD` | `Ingress/steel` (DSTV `KA` bend decode) + `Spec` (K-factor bend-allowance table over the kernel `develop.md` isometric unroll; never a second unroll engine). |
| `SWARF_5AXIS_SIMULTANEOUS` | `Toolpath/surface` (flank/swarf orientation over the kernel `segment` direction fields; 5-axis simultaneous refinement beyond the seed rows). |
| `CONVERSATIONAL_DIALECT_BREADTH` | `Posting/dialect` (Klartext/Mazatrol/OSP conversational families beyond the word-address seed rows). |

---

## [11] — LEG PARTITION (acyclic against [05]; arrow = composes, every edge within-wave or earlier)

- **Leg 1 — STRUCTURE + AXES + RAIL** (6 pages): `Process/{owner,family,physics,faults}`, `Tooling/{magazine,cuttingdata}`. The `[V1]` re-partition lands corpus-wide (moves/splits/namespaces + CS0118 rename + ARCH `[01]/[02]` + README router) with the FULL roster reconciliation ([06]: 2 ADD, 1 REMOVE, RBP re-group, netDxf VERIFY-REMOVE, `.api` stubs, overlay slimming); `owner#atoms` mints `Loop.Bulges`/`ProjectionDir`/`ResidualStock`/`StockSnapshot`/content-key seed; family/physics/faults rebuild; magazine wire-ready; cuttingdata absorbs the physics table.
- **Leg 2 — SUBSTRATE + INGRESS** (6 pages): `Geometry2D/{algebra,arcs}`, `Ingress/{profile,solid,steel}`, `Toolpath/skeleton` (kernel-medial consumer, STAGED EARLY — wave-3 motion reads its clearance field). slicing's slice-stack re-route stays wave 3.
- **Leg 3 — CAM + ADDITIVE + NESTING + FIXTURING** (14 pages): `Toolpath/{motion,surface,partition,guard}`, `Kinematics/{cell,machine}`, `Additive/{slicing,implicit,production}`, `Nesting/{nfp,stock}`, `Fixturing/{workholding,setups}`, `Spec/tolerance` (STAGED EARLY — surface's Ra→scallop reads it within-wave).
- **Leg 4 — EGRESS + TRUTH + DOCS** (7 pages): `Posting/{program,dialect}`, `Verify/{removal,probing}`, `Spec/capability`, `Documentation/{projection,traveler}`.

Acyclicity holds: skeleton(2)→motion(3); tolerance(3)→surface(3) within-wave; removal(4)/setups(3) exchange stock-state via input-carried `owner#atoms`(1) fields, never a wave-4→wave-3 back-edge; the `owner` two-node split breaks the atom↔plane inversion. The four `[05]` acceptance dry-runs compose entirely from rebuilt fences.

---

## [12] — SELF-REPORTED COUNTS

- **Folders: 13** (Process, Tooling, Geometry2D, Ingress, Toolpath, Kinematics, Additive, Nesting, Fixturing, Posting, Verify, Spec, Documentation). Polygon DELETED. Meets the `[V1]` floor exactly — no dilution, no unproven expansion.
- **Page rows: 33** — improve 5 (owner, faults, guard, nfp, stock) · rebuild 5 (family, physics, motion, skeleton, program) · new@dest MOVE 6 (magazine, profile, cell, slicing, workholding, projection) · split-new 2 (algebra, arcs) · new 15 (cuttingdata, solid, steel, surface, partition, machine, implicit, production, setups, dialect, removal, probing, tolerance, capability, traveler).
- **deletePages: 7 · absorb pairs: 8.**
- **Verdicts disposed: 10** (V1-V10). **Evidence disposed: 14** (E1-E14).
- **Package dispositions: 24** (2 ADD, 1 REMOVE, 1 REPLACE-candidate, 2 VERIFY-REMOVE, 4 mandate-KEEP, 6 other folder-KEEP, 8 shared-tier). .api: 2 NEW, 1 DELETE, 9 re-point/slim.
- **Cards disposed: 23** (16 TASKLOG + 7 IDEAS) **+ 3 reseeds.**
- **Fault registry: 13 folder blocks · landed 2701-2710 grandfathered · growth 2711-2741.**
