# [DOSSIER] toolpath-nesting — Rasm.Fabrication design survey

Lane: `toolpath-nesting`. Scope (10 pages, deep-read fully on disk): `Toolpath/{motion,skeleton,slicing,guard,kinematics}`, `Nesting/{nfp,stock,workholding}`, `Polygon/{clipper,import}`. Cross-page anchors verified for register spanning: `Process/owner.md`, `Posting/program.md`, `.api/api-cavaliercontours.md`, package-root `ARCHITECTURE.md`/`README.md`, `Rasm.Fabrication.csproj`. Member-verification posture: NO restored assembly (no `obj/bin`, no vendored DLLs on disk); `assay api query` would FAULT — phantom adjudication runs off the folder `.api` catalog (sanctioned fallback per brief `[E14]`). Stance: HOSTILE. Interiors are craft; the production system is naive until disk proves otherwise.

Verdict legend: HOLD = register anchor exact + defect real; DRIFT = defect real, anchor moved (corrected anchor given); REFUTED = disk falsifies the claim (line cited).

---

## [01] REGISTER RE-VERIFICATION (every assigned row, on disk)

### E2 — Unwired pipeline (arc-rail/wiring half)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `guard.md:3` "Cam.Solve consults Guard.Check per feed move" vs `motion.md:46-62` no call | guard.md:3; motion.md:46-62 | **HOLD** | `Cam.Solve` (motion.md:46-62) generates moves then hands to `RobotProgram.Solve` OR emits bare `Motion` — NO `Guard.Check` anywhere. guard.md:16 ALSO asserts "Cam.Solve consults Check"; motion never delivers. Cross-corpus `rg 'Guard\.Check'` → only motion.md:19 Growth-prose ("the `Guard.Check` per-move gate produces"), zero fold call sites. |
| `workholding.md` `Condition` called by no fold | workholding.md:78-87 | **HOLD** | `Workholding.Condition` (78-87) has zero callers corpus-wide. Worse: its body walks moves and returns `Fin.Succ(moves)` — it NEVER calls `Posting.Sequence` despite the prose claim at workholding.md:3,16 ("the surviving cut contours then ordered inner-before-outer through the `Posting/program` `Sequence` fold"). Prose-fiction on top of unwired. |
| (cross-lane corroboration) `magazine.Schedule` no consumer in `program.md:141-164` | program.md:141-164 | **HOLD** | `Posting.Post`/`Condition` (141-164) never reads `Magazine.Schedule`; `GCommand.ToolChange`(M6)/`LengthOffset`(G43) rows exist (program.md:61-62) but no fold emits them. `rg 'Magazine\.Schedule'` → zero. |

### E3 — Arc-rail fiction (split verdict)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `clipper.md:5,33` retirement claim vs `program.md:247-282` still refits | clipper.md:5,33; program.md:247-282 | **HOLD** | clipper.md:5/33 claim the `g3.BiArcFit2` refit is RETIRED (bulge → G2/G3 direct). program.md:268 still constructs `new BiArcFit2(new Vector2d(a.X,a.Y),…)` inside `FitRun`, walking `loop.At(i)` vertices and reading NO `Bulges`. The retirement is fiction. |
| `owner.md:42` `Loop(Arr<Point3d>, bool)` — no `Bulges`/3-arg ctor/`BulgeAt` the consumer calls | owner.md:42; clipper.md:257,271 | **HOLD** | owner.md:42 is exactly `public sealed record Loop(Arr<Point3d> Vertices, bool Closed)` — 2 params, no `Bulges` column, no `BulgeAt`, no 3-arg ctor (members: `At`,`Winding`,`AsCcw`,`Bound`,`Covers`). clipper.md:257 calls `ccw.BulgeAt(i)`; clipper.md:271 constructs `new Loop(toArr(verts), pline.IsClosed, toArr(bulges))` (3-arg). The widened `Loop` the arc rail requires was never landed by the atom owner. Both fences would fail to compile against the real `Loop`. |
| fence namespaces `lnContours`/`PlineOffset.ln<>` at `clipper.md:169-227` | clipper.md:169-227 | **REFUTED** | Disk clipper.md:169-172 imports `CavalierContours.Core/Polyline/Shape/Spatial`; calls `PlineOffset.ParallelOffset<Polyline<double>,double>` (213), `PlineBoolean.PolylineBoolean<Polyline<double>,double>` (219). NO `lnContours`, NO `PlineOffset.ln<>`. Matches `api-cavaliercontours.md:12` real namespace. The divergence the register flagged was already corrected; the sub-claim is stale. |
| hand-built lead arcs | program.md:215-223 | **HOLD** | `LeadArc` arc arm (219-222): `Point3d center = from + 0.5*(to-from); new GWord(GCommand.ArcCcw,…,Some(center.X-from.X),Some(center.Y-from.Y),…)` — manual center math, not routed through `ArcAlgebra`. |
| NEW phantom member (arc owner) | clipper.md:221 | **NEW-DEFECT** | `ArcBoolean` reads `result.Positive` (clipper.md:221). `api-cavaliercontours.md:47` states explicitly: "the positive list is `PosPlines`, never a `Positive` accessor." `BooleanResult<O,T>.Positive` is a phantom; the real member is `PosPlines : List<BooleanResultPline<O,T>>` and each carries `.Pline`. Fence would not compile. |

### E5 — Stub generators (E4-class stub-arm rows, `motion.md`)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `radial-sweep` AND `helical` both alias `Turn` | motion.md:71,73 | **HOLD** | `radialSweep: static s => Turn(...)` (71); `helical: static s => Turn(...)` (73). Two distinct strategy rows collapse to one per-vertex offset. |
| `Turn` = `v.Y - radius - k*stepOver` per-vertex offset (no ZX profile; `BarStock` prose-only) | motion.md:76-79 | **HOLD** | `Turn` (76-79): `.Map(v => new Move(new Point3d(v.X, Math.Max(0.0, v.Y - radius - k*stepOver), 0.0),…))`. A per-vertex Y push, not a lathe ZX/Z-vs-radius sweep. `BarStock` appears only in prose (motion.md:3,14,16, mermaid:156) — no `BarStock` shape exists (the real `Stock.BarStock` lives in `nfp.md:81`, never composed here). |
| `Plunge`/`Peck` 2-move centroid stubs | motion.md:81-84,105-108 | **HOLD** | `Plunge` (81-84): retract + plunge-to-centroid, 2 moves. `Peck` (105-108): retract + plunge-to-centroid, 2 moves. No peck-cycle geometry, no dwell/retract policy. |
| `SliceWalk` orders nothing | motion.md:86-87 | **HOLD** | `SliceWalk(Loop layerContour) => toSeq(layerContour.AsCcw().Vertices).Map(v => new Move(v,…))`. No perimeter/infill/travel ordering. DEEPER: `Generate` is called with `input.Profiles` loops (motion.md:54), so `layer-walk` never touches the `SliceLayer` (contours/shells/infill) that `slicing.Layers` produces — the motion→slicing seam is prose-only (motion.md:16), effectively unwired. |

### E8 — Coverage silences (lane portion)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| NC verification absent — guard is per-move design-time only | guard.md:59-67 | **HOLD** | `Guard.Sweep`/`Check` (58-79) emit per-move `Verdict` (Clear/Gouge/Collision/Clearance). No program-level voxel material-removal owner anywhere in the lane. `rg` waterline/scallop/pencil/rest/swarf across all 10 pages → zero hits (coverage silence confirmed). |
| `KinematicClass` 4 rows; `mill-5axis` binds gantry | (family.md:100,118 cross-lane) | **HOLD (adjacent)** | `kinematics.md:3` names `articulated-arm` / `cartesian-gantry` / `rotary-spindle` rows; the 5-axis rotary-topology deepening ([V5]) is unrealized in-lane — `RobotProgram` targets a tool-down `Plane(m.To, ToolFrame.XAxis, YAxis)` (kinematics.md:85), no rotary/TCP owner. |

### E9 — Identity/logic defects (`nfp.md`, `workholding.md`)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `Stock.Of()` hashes `Area` alone (equal-area collide; poisons `Remnant.Parent`) | nfp.md:88 | **HOLD** | nfp.md:88: `this is FromRemnant fr ? fr.Remnant.Identity : XxHash128.HashToUInt128(MemoryMarshal.AsBytes<double>(new[] { Area }))`. Discriminant + all dims ignored: `Sheet(2,6)` vs `Sheet(3,4)` (Area 12), `Plate(w,h,d1)` vs `Plate(w,h,d2)` (Area ignores depth) all collide. `Remnant.From` (nfp.md:68) stamps `Some(stock.Of())` as `Parent`, so lineage is poisoned. Exemplar to fold onto: nfp.md:59 full-vertex `Remnant.Of`. |
| `Ty*1e6+Tx` heuristic | nfp.md:137 | **HOLD** | nfp.md:137: `static double Heuristic(NoFitPolygon nfp, PartTransform t) => t.Ty*1e6+t.Tx;`. Magic composite-sort constant; should be tuple `(Ty,Tx)` comparison. |
| origin-referenced NFP vs `Anchor`-referenced test (register: "unverified") | nfp.md:144 vs :300,331 | **HOLD (real incoherence)** | `NoFitPolygon.Of` (144-147) = `MinkowskiSum(fixed, Reflect(orbiting))` where `Reflect` (172) is about `Point3d.Origin` — NFP is in the orbiting-coordinate-origin frame. `BottomLeft` feasibility (300): `Pair(pl.Id).Feasible(c.X - Anchor(pl.Part).X, c.Y - Anchor(pl.Part).Y)` where `Anchor` (331) is the bottom-left vertex — a DIFFERENT reference. Compounding: candidate points come from raw `Pair(pl.Id).Boundary.Vertices` (295) WITHOUT translating by the placed part's actual `(pl.Tx,pl.Ty)`, while `stock.Contains(part, c.X, c.Y)` (298) uses raw `c` with NO anchor offset. Three inconsistent frames mixed; the NFP feasibility is geometrically incoherent, not merely unverified. |
| 3-point keep-out sampling (thin diagonal clamp crossing undetected) | workholding.md:73-83 | **HOLD** | `Clears` (73-76) samples `segment.A`, `segment.B`, `mid` — 3 points. `Condition` (78-87) samples `m.To` + midpoint — 2 points. A thin diagonal clamp crossing between sample points is undetected; needs segment-vs-polygon intersection. |
| `ForHolding` first-match (`Vise` unreachable) | workholding.md:49-50 | **HOLD (anchor → 49-51)** | `ForHolding` (49-51): `toSeq(Items).Find(k => k.Holding == holding).IfNone(Clamp)`. `Clamp` (39) and `Vise` (40) BOTH map `HoldingClass.Mechanical`; `Find` returns first → `Clamp` always wins, `Vise` unreachable. Should be a total mapping. Method body spans 49-51 (register said 49-50). |
| `MarginScale` scalar vs kind-shape prose | workholding.md:39-44 | **HOLD** | Rows (39-44) carry only `marginScale:` (1.0/1.0/1.5/0.5/0.25/0.0) + `HoldingClass`. `Zone` (67-71) does a uniform `Offset(footprint, margin*MarginScale)` identical across kinds. The prose (workholding.md:3,14) claims kind-specific SHAPES — "vacuum table inflates to a full-bed keep-out", "chuck a revolved jaw envelope", "vise a two-jaw paired footprint" — none are real geometry columns. Growth claim (workholding.md:19 "one `WorkholderKind` row carrying its footprint-shape behavior column") is aspirational: no such column exists. |

### E10 — Stale governance (lane portion)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| band-2500 | kinematics.md:3 | **HOLD** | kinematics.md:3: "into the typed band-2500 `FabricationFault`." Sole band-2500 in the lane (swept all 10 pages). Should be 2700. |
| netDxf rationale | import.md:20 | **HOLD** | import.md:20: "`netDxf` (present in the central manifest as an `Rasm.AppUi` DXF-write dependency) is the rejected second DXF reader". README:64 mirrors the rejection. Brief re-frames: netDxf removed repo-wide → the rejection frame is stale (nothing to reject). |
| ARCH band-2500 | ARCH:16 | **HOLD** | ARCH:16: "FabricationFault band-2500 composing kernel band-2400". |
| ARCH nfp→Persistence/Schema (deleted folder) | ARCH:54-equiv | **HOLD** | ARCH seam row: `Nesting/nfp → csharp:Rasm.Persistence/Schema [WIRE]: Placement / Remnant XxHash128 content-keyed durable row`. `Rasm.Persistence/Schema` is the deleted folder (brief); re-points to blobstore/cache. nfp is my page. (`Posting/program → Persistence/Schema` sibling row also stale.) |
| ARCH netDxf write-leg | ARCH:52 | **HOLD** | Full ARCH:52 line carries "(netDxf in AppUi owns the distinct WRITE leg)" in the `Polygon/import ← Rasm.Bim/Exchange` parenthetical — stale netDxf reference. |

### E11 — Ledger gaps (lane portion)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `Rasm.Element/MaterialId` wired-undeclared | stock.md:40,83,99 | **HOLD** | stock.md:40 `using Rasm.Element;`; `CutPart` (83) + `NestPlacement` (99) carry `MaterialId Material`. csproj confirms `<ProjectReference Include="../Rasm.Element/Rasm.Element.csproj" />`. ARCH `[02]` seam ledger has NO itemized `Nesting/stock ← Rasm.Element/MaterialId` row (only the wildcard `* → Rasm.Element/Projection`). Edge legal (Rasm.Element ∈ the `{Rasm, Rasm.Element}` substrate) but undeclared. |
| intra-package web undeclared | ARCH `[02]` | **HOLD** | ARCH `[02]` declares only CROSS-package edges + ONE intra row (`Nesting/stock → Nesting/nfp [PLAN]`). Absent: guard→skeleton (`ClearanceAt`), guard→workholding (`ExclusionZone`), workholding→posting (`Sequence`), motion→{skeleton,slicing,kinematics}, magazine→guard/workholding (`ToolAssembly` holder). |

### E13 — Page-craft (`[RESEARCH]` tails + false author-forever seals)
| Claim | Anchor | Verdict | Disk evidence |
|---|---|---|---|
| `[03]-[RESEARCH]` tails | kinematics.md:122-125; skeleton.md:199-202; slicing.md:219-222 | **HOLD** | All three present verbatim. |
| additional RESEARCH tail (register undercount) | stock.md:317 | **DRIFT (+1)** | stock.md:317 `## [05]-[RESEARCH]`. Register named only 3; the brief `[V2]` acceptance is `rg '\[RESEARCH\]'` → zero, so this 4th instance must also purge. |
| "no managed library exists" seal now false | skeleton.md:3; slicing.md:3,221 | **HOLD** | skeleton.md:3 "no managed straight-skeleton library exists on NuGet… the author-kernel posture is correct and forward" — false per `[V2]a` (kernel medial-with-clearance-radius lands; `StraightSkeleton` dies unconditionally for `Offsetting.Apply`). slicing.md:3,221 "no managed slicer exists… in-folder author-kernel" — false per `[V2]b` (ARCH:47 already promises "Section re-routes to the kernel `IntersectOp.PlaneMesh` when realized"; gate fired). |

### E14 — Upstream bindings (lane portion)
| Claim | Verdict | Disk evidence |
|---|---|---|
| CavalierContours sited at Fabrication stratum (`[V10]a`) | **HOLD** | clipper.md `ArcAlgebra` composes the admitted `CavalierContours` (csproj pin + `.api` present). One arc owner; kernel owns exact medial/corner-row assembly per brief. Fence member surface matches `.api` EXCEPT phantom `result.Positive` (see E3-NEW). |
| skeleton/slicing kernel-consumer reframe pending | **HOLD** | Both remain full author-kernels on disk (skeleton wavefront 74-196; slicing `Section`/`Chain` 91-135). `[V2]a/b` reframes unrealized. |
| nesting identity through kernel `ContentHash.Of` | **DRIFT** | nfp mints raw `XxHash128.HashToUInt128` at 3 sites (nfp.md:59 `Remnant.Of`, :88 `Stock.Of`, :169 `PairKey`) — the second-hasher defect; `[SEAM_AND_ENTRY_LAW]` wants the one `ContentHash.Of` federation entry. |

---

## [02] PER-PAGE DOSSIER

### Toolpath/motion.md — `Rasm.Fabrication.Toolpath`
- Namespace: `Toolpath` == folder ✓.
- Consumers: Clipper2 (via `PolygonAlgebra`), `Predicate.Orient2D`; composes `family` (CutStrategy/Admits), `skeleton` (MedialAxis/ClearanceAt), `kinematics` (RobotProgram.Solve), `slicing` (SliceLayer — imports `Rasm.Fabrication.Additive`). Zero dead-package hits.
- Seams OUT: motion→skeleton (Adaptive reads `StraightSkeleton.MedialAxis`+`ClearanceAt`, 110-137), motion→kinematics (58-61), motion→Geometry2D (Contour/Pocket 89-103), motion→slicing (PROSE-only; `layer-walk` walks `input.Profiles`, never `SliceLayer`).
- Naivety — APPROACH: 5 of 8 strategy arms are stubs (`radial-sweep`≡`helical`≡`Turn` per-vertex offset; `Plunge`/`Peck` 2-move centroid; `SliceWalk` unordered). `BarStock` revolved-envelope is a prose phantom. COVERAGE: no waterline/scallop/pencil/rest/3D-surface/swarf; adaptive engagement bound reads clearance but not the physics chip-load budget.
- Unwired: `Guard.Check` never called (guard framed as Growth, motion.md:19). `layer-walk` ignores slicing output.
- Charter-as-it-should-be: real `(RemovalModality, CutStrategy)` generators — `Turn` a true ZX radial sweep over a real revolved stock envelope (kernel `Bounds` enclosing-cylinder fit), `helical` a distinct thread/ramp, `Peck` canned-cycle geometry w/ dwell-retract, `SliceWalk` a perimeter/infill/travel orderer over the real `SliceLayer`; 3D-surface family composing kernel geodesics/isolines + OpenCAMLib cutter positioning; `Adaptive` re-pointed to the kernel medial-with-clearance-radius; `Guard.Check` wired per committed feed move in the Cam fold; the `Option<ArcCenter>` chord identity fed to the arc-native posting rail (not biarc-refit).

### Toolpath/skeleton.md — `Rasm.Fabrication.Toolpath`
- Namespace: `Toolpath` == folder ✓.
- Consumers: Clipper2 (convex-inset cross-check via `PolygonAlgebra`), `Predicate.Orient2D`. Zero dead-package hits.
- Seams OUT: skeleton→Geometry2D (`OffsetAt`, 82-83). Consumed BY: motion (Adaptive), guard (`ClearanceAt`).
- Naivety — APPROACH: entire page is an author-kernel wavefront (`Wavefront`/`SkeletonEvent`/`Propagate`, 48-196) that `[V2]a` rules DEAD for the kernel medial-with-clearance-radius; only the trochoidal constant-engagement WALK survives. `[03]-[RESEARCH]` tail (199-202) + false "no managed library / correct and forward" seal (skeleton.md:3).
- Charter-as-it-should-be: kernel-medial CONSUMER — read `MedialAxis`, `Clearance`, `ClearanceAt` (per-point radius + arbitrary-probe clearance) from the kernel `[V10]c-e` result; keep only the trochoidal engagement WALK strategy over the kernel clearance field; delete `Wavefront`/`Propagate`/`OffsetAt`; purge the RESEARCH tail.

### Toolpath/slicing.md — declares `Rasm.Fabrication.Additive` (folder `Toolpath`)
- Namespace SCHISM: folder `Toolpath` ≠ namespace `Additive`. Confirms `[V1]` re-home slicing→`Additive/`.
- Consumers: Clipper2 (`Offset`/`ClipOpenPath`), `Predicate.Orient3D`+`Orient2D`, native `Mesh` via `DuplicateNative`. Zero dead-package hits.
- Seams OUT: slicing→Geometry2D; forward-consume slicing→`Rasm/Meshing/intersect` (ARCH declared "when realized"). Consumed BY: motion `layer-walk` (prose-only — not actually wired).
- Naivety — APPROACH: `Section`/`Cut`/`Crossing` author-kernel + O(n²) `Chain` endpoint-adjacency walk (114-135, nested while + `remaining.RemoveAt(0)`) that `[V2]b` rules DEAD for the kernel slice-stack. gyroid mis-modeled: slicing.md:19 claims gyroid/TPMS is addable as a 2D-hatch `InfillPattern` row — false collapse (`[V6]`; TPMS is a 3D voxel structure, PicoGK lane, not a 2D hatch). `[03]-[RESEARCH]` tail (219-222) + false author-forever seal (3,221).
- Charter-as-it-should-be: kernel slice-stack CONSUMER (`[V10]b` oriented contours + typed open chains); delete `Section`/`Chain`; keep FFF/DED `InfillPattern` + shell composition over the kernel-emitted contours; route gyroid/TPMS to the PicoGK voxel lane (not an InfillPattern row); variable/adaptive layer height as kernel slice-stack policy rows; page moves to `Additive/`.

### Toolpath/guard.md — `Rasm.Fabrication.Toolpath`
- Namespace: `Toolpath` == folder ✓.
- Consumers: Clipper2 (`MinkowskiSum`/`Offset`/`Clip`), kernel `SpatialIndex` (BVH broad-phase), `skeleton.ClearanceAt`, `magazine.ToolAssembly` (holder — passed as `Option<Loop> Holder`). Zero dead-package hits. Imports `Rasm.Fabrication.Fixturing` (ExclusionZone).
- Seams OUT: guard→skeleton (`ClearanceAt`, 76), guard→workholding (`ExclusionZone`/`Fixture.Zones`, 3/41/93), guard→Geometry2D, guard→Spatial. All UNDECLARED in ARCH.
- Naivety: shape-correct (register 8.5), the mandatory per-move safety floor. Sole defect class is UNWIRED — `Cam.Solve` never consults `Check`. `Lift` (81-84) is 3 unconditional rapids (up/over/down), no per-keep-out routing beyond the clearance plane.
- Charter-as-it-should-be: unchanged interior; WIRE `Check` per committed feed move inside the Cam fold (`Gouge`/`Collision`→typed fault, `Clearance`→substitute lift, `Clear`→commit). Guard stays the per-move DESIGN-time floor distinct from the NEW `Verify/removal` voxel material-removal owner (`[V7]`). Declare the 3 out-seams.

### Toolpath/kinematics.md — declares `Rasm.Fabrication.Kinematics` (folder `Toolpath`)
- Namespace SCHISM: folder `Toolpath` ≠ namespace `Kinematics`. Confirms `[V1]` re-home kinematics→`Kinematics/`.
- Consumers: `Robots` (visose) + `Rhino3dm` via `extern alias R3`. Zero dead-package hits. The `extern alias R3` boundary exemplar (register 9).
- Seams OUT: consumed BY motion (RobotProgram.Solve). Boundary-maps RhinoCommon↔Rhino3dm at `ToR3`/`FromR3` only.
- Naivety: sound (register 9). Defects: band-2500 (kinematics.md:3, stale→2700), `[03]-[RESEARCH]` tail (122-125). COVERAGE gap (`[V5]`): 5-axis rotary-topology/TCP owner absent — `Targets` seats a fixed tool-down frame (85), no rotary-axis inverse/RTCP; the shared jerk/accel motion-dynamics law with posting `Lookahead` is un-homed.
- Charter-as-it-should-be: page moves to `Kinematics/`; band-2500→2700; purge RESEARCH tail (fold member confirmations into `Packages`/`.api`); ADD a 5-axis machine-tool topology + TCP/RTCP owner beside the Robots cell; home the one motion-dynamics law posting reads.

### Nesting/nfp.md — `Rasm.Fabrication.Nesting`
- Namespace: `Nesting` == folder ✓.
- Consumers: `RectpackSharp` (rect-fastpath), Clipper2 (`MinkowskiSum`/`MinkowskiDiff`/`Clip`/`Offset`), `System.IO.Hashing.XxHash128` (direct), `Predicate.Orient2D`. Zero dead-package hits.
- Seams OUT: nfp→Geometry2D, nfp→`Rasm.Persistence/Schema` (ARCH, stale). Consumes `stock.NestPlan` via `FabricationInput.Plan` (`Nest.Honor`, 206-217).
- Naivety: strong nesting engine (register 9 minus E9). Defects: `Stock.Of` area-only hash (88), `Ty*1e6+Tx` (137), NFP reference-frame incoherence (144 vs 300/331 — real, see E9), raw `XxHash128` at 3 sites vs the one `ContentHash.Of` federation entry.
- Charter-as-it-should-be: `Stock.Of` hashes discriminant + all dimensions through kernel `ContentHash.Of`; `Heuristic` becomes a `(Ty,Tx)` tuple comparison; the NFP feasibility frame reconciled (translate candidates by the placed `(pl.Tx,pl.Ty)`, single reference — origin OR anchor, consistently) and proven on the golden fixture; all identity mints route the one `ContentHash.Of` entry; `RectpackSharp` re-homes to `RectangleBinPack.CSharp`'s `MaxRectsBinPack` heuristic-sweep per `[04]` (RectpackSharp REMOVE); Persistence seam re-points to blobstore/cache.

### Nesting/stock.md — `Rasm.Fabrication.Nesting`
- Namespace: `Nesting` == folder ✓.
- Consumers: `RectangleBinPack.CSharp` (5 packers + heuristic sweep), `Rasm.Element.MaterialId`, `Rasm.Vectors.PositiveMagnitude`. Zero dead-package hits.
- Seams OUT: stock→nfp (`NestPlan` via `Nest.Honor`, DECLARED ARCH), stock→`Rasm.Element/MaterialId` (UNDECLARED — E11), stock→`Rasm.Compute` (`WasteAreaMm2` rollup, declared).
- Naivety: sound scalar exemplar (register 9; brief telos "the scalar exemplar"). Defects: `MaterialId` ledger row missing (E11); `[05]-[RESEARCH]` tail (317, additional beyond register).
- Charter-as-it-should-be: unchanged engine; ADD the `Nesting/stock ← Rasm.Element/MaterialId` ledger row; the `[05]-[RESEARCH]` section body folds into `Packages`/`.api` (RESEARCH-tail purge is corpus-wide law). Note: `RectangleBinPack.CSharp` central pin re-groups from the vacated Materials label (`[04]`) — verify at leg 1.

### Nesting/workholding.md — declares `Rasm.Fabrication.Fixturing` (folder `Nesting`)
- Namespace SCHISM: folder `Nesting` ≠ namespace `Fixturing`. Confirms `[V1]` re-home workholding→`Fixturing/`.
- Consumers: Clipper2 (`Offset`/`Clip`), `Loop.Covers`, `family.HoldingClass`. Zero dead-package hits. Imports `Rasm.Fabrication.Posting`.
- Seams OUT: workholding→posting (`Sequence` — imported but NEVER called), workholding→Geometry2D, workholding→family (HoldingClass). Consumed BY guard (`ExclusionZone`).
- Naivety — APPROACH: `MarginScale` scalar collapses all 6 kinds to a uniform offset (no footprint-shape column despite prose). `ForHolding` first-match makes `Vise` unreachable. 3-point keep-out sampling misses thin diagonal crossings. UNWIRED: `Condition` (78) has no caller AND never invokes `Posting.Sequence` despite workholding.md:16 prose.
- Charter-as-it-should-be: page moves to `Fixturing/`; `Clears`/`Condition` use segment-vs-polygon intersection; per-kind real footprint-shape geometry columns (two-jaw vise, revolved chuck jaw, full-bed vacuum) not a `MarginScale` scalar; `ForHolding` a total mapping; `Condition` wired into the Cam conditioning fold and actually composing `Posting.Sequence`; a NEW `Fixturing/setups` sibling (multi-fixture scheduler, WCS assignment, op-N-vs-op-N-1 stock-snapshot admission) joins it (no one-file folder).

### Polygon/clipper.md — declares `Rasm.Fabrication.Geometry2D` (folder `Polygon`)
- Namespace SCHISM: folder `Polygon` ≠ namespace `Geometry2D` (BOTH fences, 50 and 182). Confirms `[V1]` split `Polygon/`→`Geometry2D/{algebra,arcs}`.
- Consumers: Clipper2 (`Clipper2Lib` — full line-space surface), `CavalierContours` (arc-space), `Predicate.Orient2D`. Zero dead-package hits.
- Seams OUT: consumed BY motion/nfp/guard/slicing/workholding/program (the 2D substrate) + skeleton. Reads `Process/owner.Loop` (widened form NOT landed).
- Naivety: craft (register 9), two-owner line/arc split. Defects: reads `Loop.Bulges`/`BulgeAt`/3-arg ctor the atom owner never landed (owner.md:42 — E3); phantom `result.Positive` (221, real member `PosPlines` per `.api:47` — E3-NEW). The retirement claim (5,33) is unbacked while `program.md` still refits.
- Charter-as-it-should-be: SPLIT into `Geometry2D/algebra` (Clipper2 line-space) + `Geometry2D/arcs` (CavalierContours arc-space) per `[V1]`; the atom owner lands the widened `Loop(Arr<Point3d>, bool, Arr<double> Bulges)` + `BulgeAt` FIRST (leg 1); `ArcBoolean` reads `result.PosPlines[].Pline` (not `result.Positive`); the downstream kerf/lead/adaptive rails wire through `ArcAlgebra` so the retirement is real; `g3.BiArcFit2` survives only for genuinely line-sourced chains.

### Polygon/import.md — declares `Rasm.Fabrication.Geometry2D` (folder `Polygon`)
- Namespace SCHISM: folder `Polygon` ≠ namespace `Geometry2D`. `[V1]` re-home import→`Ingress/profile`.
- Consumers: `ACadSharp` (DxfReader/DwgReader + Spline/Arc/Circle/Insert samplers), `Predicate.Orient2D`. Zero dead-package hits. netDxf named only to reject (20).
- Seams OUT: consumed BY nfp (part library), program (profile program). Import↔`Rasm.Bim/Exchange` (ARCH:52, over the shared ACadSharp pin).
- Naivety: craft (register 9), the one DXF/DWG ingress boundary. Defect: netDxf rejection rationale stale (20; netDxf removed repo-wide, replaced by AppUi ACadSharp `DxfWriter`+`DwgWriter`).
- Charter-as-it-should-be: page moves to `Ingress/profile` (joins NEW `Ingress/solid` OcctNet STEP/IGES/STL + `Ingress/steel` DSTV NC1); netDxf rejection frame re-writes (nothing to reject); read-only INGRESS posture preserved; one polymorphic admit discriminating on source kind, emitting kernel-admissible geometry at the seam.

---

## [03] CROSS-CUTTING FINDINGS

1. **NAMESPACE SCHISM (folder≠namespace) is 5/10 in-lane — the strongest `[V1]` evidence.** slicing→`Additive`, kinematics→`Kinematics`, workholding→`Fixturing`, clipper+import→`Geometry2D`. The realized namespaces already NAME the decomposition the folders obscure; the ratified partition should home each page to its namespace-true folder (slicing→Additive, kinematics→Kinematics, workholding→Fixturing, clipper→Geometry2D split, import→Ingress).

2. **UNWIRED PROFESSIONAL FLOOR (E2) confirmed corpus-wide.** Zero real call sites for `Guard.Check`, `Workholding.Condition`, `Magazine.Schedule` (only Growth-prose mentions). Compounding: `layer-walk` never consumes `SliceLayer`; `workholding.Condition` never calls `Posting.Sequence` despite its own prose. The pipeline is a set of islands; the single executing fold (`Run → Cam → {Guard, Workholding, Magazine} → Post`) does not exist.

3. **ARC RAIL split (E3): core defect HOLDS, one sub-claim REFUTED, one NEW phantom.** The unlanded widened `Loop` (owner.md:42 vs clipper.md:257/271) and the still-refitting `program.md:268` HOLD. The `lnContours`/`PlineOffset.ln<>` fence spelling is REFUTED — already corrected to `CavalierContours.*`/`ParallelOffset`. NEW: `clipper.md:221 result.Positive` is a phantom (`.api:47` → real member `PosPlines`), an uncompilable fence the register never flagged.

4. **KERNEL-CONSUMER REFRAMES (E13/`[V2]`) unrealized.** skeleton (whole wavefront author-kernel) and slicing (Section/O(n²) Chain) remain author-kernels; ARCH:47 already promises the slicing re-route "when realized" (gate fired). Both false "no managed library" seals persist.

5. **IDENTITY DISCIPLINE (E9/E14) — three raw `XxHash128` mints + an area-only collision + a coherence bug.** nfp mints `XxHash128` directly at :59/:88/:169 (second-hasher defect); `Stock.Of` (88) collides equal-area stocks and poisons `Remnant.Parent`; the NFP feasibility mixes origin/anchor/untranslated-placement frames (144 vs 300/331) — a real geometric incoherence, not merely "unverified".

6. **SEAM LEDGER (E11) is cross-package-only + one intra row.** ARCH `[02]` itemizes cross-package edges + `stock→nfp` but omits the entire toolpath/guard/workholding/motion intra web AND the wired `stock→Rasm.Element/MaterialId` edge. Two ARCH rows point at the deleted `Rasm.Persistence/Schema` (nfp + program). Band-2500 at ARCH:16 + kinematics.md:3. Stale netDxf at ARCH:52 + import.md:20 + README:64.

7. **RESEARCH-tail count is 4, not 3.** kinematics:122, skeleton:199, slicing:219 (register) + stock:317 (register undercount). All must purge for `rg '\[RESEARCH\]'`→zero.

8. **DEAD-ADMISSION (E1) confirmed for the lane.** Word-boundary grep of PicoGK/OcctNet/OcctShape/DSTV/SharpVoronoiLib/VoronoiPlane/Voxels across all 10 pages → ZERO consumers. The mandate lanes those four packages seed (implicit/voxel, solid ingress, steel NC1, Voronoi partition) are absent from toolpath-nesting; they land as NEW pages under the `[V1]` partition (`Additive/implicit`, `Ingress/solid`, `Ingress/steel`, `Toolpath/partition`).
