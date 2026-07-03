# DOSSIER — corpus-a — Rasm.Fabrication/.planning (Nesting, Polygon, Posting) + governing docs

Scope read fully: `Nesting/{stock,nfp,workholding}.md`, `Polygon/{clipper,import}.md`, `Posting/{program,projection}.md` (7 pages, first-half of 5 alpha-sorted subfolders); `ARCHITECTURE.md`, `README.md`, `IDEAS.md`, `TASKLOG.md`; corroborating grep census over the full 17-page corpus (namespaces, host imports, package wiring), the `.csproj`, and `Directory.Packages.props`. Stance: hostile.

---

## [A] PER-PAGE VERDICTS

### Nesting/stock.md — 8/10 (landed law, strong)
The rectangular cutting-stock engine. One `StockNest.Pack` fold; `NestStrategy [Union]` collapses 5 `RectangleBinPack.CSharp` packers + heuristic sweep; typed `NestYield`/per-sheet `SheetYield` ledger; int-domain `[BOUNDARY_ADMISSION]`; `[EXPRESSION_SPINE]` exemption confined to the stateful `DriveStream`/`MassCut` kernels. Band ownership correct (`FabricationFault.Nest` for job faults, `GeometryFault.DegenerateInput` for degenerate sheet). Matches census point (1) exactly.
- **Coarse rotation policy** (line 135): `allowRotation = StrategyRotates(strategy) && parts.ForAll(p => p.AllowRotation)` — ONE grain-locked part disables the 90° flip for the WHOLE run, dropping rotatable parts that only fit rotated. Forced by the per-packer (not per-part) `MaxRectsBinPack(w,h,allowRotation)` constructor; document as an inherited packer limitation, not a free choice.
- **Belt-and-suspenders silent drop** (line 278): a part that fails `Insert` on a fresh empty sheet `continue`s (counted unplaced) though `FitsSheet` (line 136) already guaranteed fit — defensive but silent.
- Aggregate utilization (line 194) divides placed area by `usableArea × sheetCount` (treats every sheet full incl. the partial last) — defensible "fraction of purchased material" definition; state it.
- Charter as-is: correct. The single non-negotiable is the disjoint-concern seam to `nfp#NESTING` via `Nest.Honor` on `FabricationInput.Plan` — intact.

### Nesting/nfp.md — 7/10 (very capable, one real correctness defect)
2D true-shape NFP nesting: `Stock [Union]` (7 cases) with one `Contains`/`Area`/`Extent`/`Planar`/`Outline` total fold; NFP via `PolygonAlgebra.MinkowskiSum`, IFP via `MinkowskiDiff`; multi-sheet `Schedule`/`Consume` with `Remnant` lineage re-injection; `RectpackSharp` rect-fastpath gated to `Planar`; `Nest.Honor` consumes sibling `NestPlan`; `NestPolicy.Score` DRL delegate column.
- **CONTENT-IDENTITY COLLISION** (line 88): `Stock.Of()` for a non-remnant hashes `Area` ALONE — `XxHash128.HashToUInt128(MemoryMarshal.AsBytes<double>(new[] { Area }))`. `Sheet(100,50)`, `Sheet(50,100)`, `Sheet(2500,2)` all → identical identity. The `Remnant.Parent` lineage stamp (`Remnant.From` line 68 `Some(stock.Of())`) inherits the collision, corrupting the content-keyed remnant lineage the multi-sheet inventory relies on. `Remnant.Of` (line 59) correctly hashes full boundary vertices — `Stock.Of()` must too (discriminant tag + all dimension doubles).
- **NFP reference-point consistency unverified**: `NoFitPolygon.Of` (line 144) builds the locus around the origin (reflect-through-origin + Minkowski); placement feasibility (`BottomLeft` line 300) tests `Pair(pl.Id).Feasible(c.Point − Anchor(pl.Part))` using `Anchor` = min-Y-then-min-X vertex (line 331). Origin-referenced NFP vs Anchor-referenced test is a latent placement-correctness gap — verify the reference frames agree.
- **Fragile magic-scale heuristic** (line 137): `Heuristic = t.Ty * 1e6 + t.Tx` — lexicographic lowest-then-leftmost breaks for `Tx > 1e6` mm. Use a tuple comparison.
- `using Rhino.Geometry` (line 35); namespace `Rasm.Fabrication.Nesting` (folder-aligned, good), imports `Rasm.Fabrication.Geometry2D` (line 32) for the Polygon owner — see folder/namespace verdict.

### Nesting/workholding.md — 6/10 (mis-filed; safety keep-out under-samples)
`WorkholderKind [SmartEnum<string>]` (MarginScale + HoldingClass columns) collapses the workholding kind into ONE `Clamp` footprint-shape column — clean (no parallel `Workholder` enum). `ExclusionZone`/`Fixture`/`Workholding`.
- **SEGMENT KEEP-OUT UNDER-SAMPLED** (`Clears` lines 73-76; `Condition` line 82): tests only segment endpoints A/B + midpoint against zones. A feed move crossing a thin clamp diagonally (endpoints + midpoint all outside the keep-out) passes undetected — a safety-critical miss for the page's own charter ("a crash against a clamp is a planned exclusion rather than a runtime collision"). Needs segment-vs-polygon intersection, not 3-point containment.
- **MIS-FILED**: lives in `Nesting/` but declares `namespace Rasm.Fabrication.Fixturing` (line 34) and imports `Rasm.Fabrication.ProcessModel` + `Rasm.Fabrication.Process` (lines 26,29). It is a fixturing/toolpath-conditioning concern grouped with 2D layout because both "place parts on stock" — weak. Belongs in a `Fixturing/` growth folder (its own namespace already says so).
- `using Rhino.Geometry` (line 30).
- Charter as-is: the single-fixture keep-out is correct; the multi-setup scheduler ARCHITECTURE.md:63 attributes to it is NOT present (correctly QUEUED as `MULTI_FIXTURE_SCHEDULE`).

### Polygon/clipper.md — 8/10 (excellent craft; owns a dead owner)
Two justified owners on one page: `PolygonAlgebra` (line-space, Clipper2 — offset/variable-offset/clip/Minkowski-sum+diff/open-path/simplify/area/bounds) and `ArcAlgebra` (arc-space, CavalierContours — bulge-carrying offset/Boolean/island-pocket/arc-length). The line-vs-arc split is real (Clipper2 cannot express constant-radius arcs). Boundary maps clean; no `PathsD`/`Polyline<double>` escapes; `Predicate.Orient2D` kept as the sole winding authority across both.
- **ARCALGEBRA IS UNCONSUMED** — see verdict candidate VC4. `ArcAlgebra`/`Bulges`/`BulgeAt` appear ONLY in clipper.md (grep). The page CLAIMS (line 5) it "RETIRES the post-hoc Clipper2-offset-then-g3.BiArcFit2-refit on the Toolpath/motion/skeleton lead-arc and morphed-spiral rails and the Posting/program kerf/lead arc" and (line 33) "Posting/program reading each `Loop.Bulges` entry straight into a G2/G3 `ArcCenter` without a refit" — but `program.md` still refits via `g3.BiArcFit2` (lines 247-282) and reads no `Bulges`. The retirement is fiction until wired.
- **`Loop.Bulges` widening unratified upstream**: `new Loop(verts, closed, toArr(bulges))` + `BulgeAt(i)` (lines 257,271) require the canonical `Loop` (owned by `Process/owner`, corpus-b) to carry the parallel `Bulges` column and a 3-arg ctor — verify `owner.md` ratifies it; today it is defined AND consumed only here.
- **Folder/namespace mismatch**: folder `Polygon/`, namespace `Rasm.Fabrication.Geometry2D` (lines 50,182).
- `is var` expression-let with a dead `else` branch (lines 212, 228-230) — the false arm (`Seq<Loop>()`) is unreachable.
- `using Rhino.Geometry` (lines 46,178).

### Polygon/import.md — 8/10 (strong ratified boundary; one dead carrier)
`ProfileImport` over ACadSharp: one `Admit` total switch (7 entity arms), resilient `Failsafe=true` read, `Insert.Explode()` recursion (package bakes OCS→WCS), `Spline.TryPolygonalVertexes` native tessellator, all members ratified against `.api-acadsharp.md`. Foreign CAD types firewalled at the seam.
- **DEAD WARNINGS ACCUMULATOR / SELF-CONTRADICTION** (line 54): `Seq<NotificationEventArgs> warnings = Empty` is populated by the sink `(_, e) => warnings = warnings.Add(e)` (line 55) then NEVER read — `Fold` doesn't receive it, `Read` returns without inspecting it. The prose (line 5, "folds its `NotificationType.Error` count into the admission") and the boundary rule (line 20, "discarding that structured warning stream is the deleted form") are both contradicted by the code, which discards it. Either wire a strict-mode escalation on `NotificationType.Error` or drop the claim.
- `using Rhino.Geometry` (line 32); namespace `Rasm.Fabrication.Geometry2D` (line 36) — folder/namespace mismatch.
- Charter as-is: correct read-only ingress; the write-leg rejection prose (line 20) is generic ("Rasm.AppUi/Render drafting concern") — good, unlike README.md:64 which names the stale netDxf.

### Posting/program.md — 8/10 (rich emitter; magic force coeff, split conditioning)
`CutProgram` dialect-neutral G-code AST; `GCommand [SmartEnum]` (21 rows); `PostDialect` COMPOSED from `Process/family` (no second dialect enum); full conditioning fold — `Kerf`/`Simplify`/`Compensate`/`Biarc`/`Lead`/`Pierce`/`Tab`/`Lookahead`/`Sequence`; dialect-override `Resolve` with `Admits` gate. Dimensional jerk-vs-accel discipline load-bearing and correct.
- **MAGIC EMPIRICAL COEFFICIENT** (line 183): `double radialForce = 0.0012 * policy.RemovalRate` — a bare uncited constant driving the whole deflection-compensation model. The cantilever stiffness (line 98-101) is rigorously parameterized; the FORCE is not. Should read a specific-cutting-pressure (Kc) column — `physics.md` owns the `(Material,Tool,Operation)` cutting-data table, the natural home.
- **MOTION-ARM CONDITIONING ASYMMETRY** (line 147 vs 148): the `Placement` arm runs the full `Sequence`→`Condition` (kerf/simplify/compensate/biarc/lead/pierce/tab/lookahead); the `Motion` arm runs ONLY `Lookahead(m.Moves.Map(ToBlock))` — no kerf/lead/pierce/tab/sequence. A milling/plasma toolpath arriving as `Motion` gets no lead-in/out or micro-tabs. Either the split is principled (cutting-outlines vs machined-toolpaths) and must be stated, or the conditioning belongs on both arms.
- **DSTV.Net unwired**: README [36] admits `DSTV.Net` for "DSTV/NC1 steel fabrication exchange … beside the neutral G-code AST" — program.md has no NC1/DSTV `Emit` sibling. Integration mandate (VC3).
- Duplicate G-codes across rows: `Dwell`/`Pierce` both "G4" (46,54); `Feed`/`Extrude` both "G1" (43,58) — semantically defensible, note it.
- Namespace `Rasm.Fabrication.Posting`; imports BOTH `Rasm.Fabrication.Process` + `Rasm.Fabrication.ProcessModel` (lines 28,30); `using Rhino.Geometry` (line 33).

### Posting/projection.md — 7/10 (strong kernel; not CAD-grade as claimed)
HLR: BSP front-to-back visibility, silhouette extraction, `SpatialIndex` BVH broad-phase, Clipper2 open-path screen clip, kernel-arrangement watertight-solid seam (`Arrangement.Apply`/`ToMesh` — `CSG_SILHOUETTE` closed). Composes kernel seams cleanly (no in-folder CSG, no local BVH).
- **BSP WITHOUT POLYGON SPLITTING** (lines 74-77): `SideOf` classifies each facet wholesale by its centroid's signed plane distance — straddling facets are NOT split. Interpenetrating/large facets order wrong, undermining the charter claim (line 3) "a concave self-occluding solid resolves correctly."
- **PER-EDGE SINGLE AVERAGE DEPTH** (line 150): `edgeDepth = (sa.Z+sb.Z)/2`; occluder test `facets[fi].Depth < edgeDepth` (line 153). A long edge partly in front / partly behind an occluder is classified by its midpoint depth → depth-crossing edges mis-split into visible/hidden.
- **ALL MESH EDGES, UNDEDUPED, NO CREASE FILTER** (line 102 `silhouette.Concat(MeshEdges(facets))`; `MeshEdges` line 145): every triangulation edge of every facet is clipped and emitted (shared edges twice — no `GroupBy` dedup like `Silhouette` has). Output is a triangle-soup wireframe, not a CAD hidden-line drawing (which wants boundary + silhouette + sharp-crease edges only).
- **Magic eye distance** (line 100): `Point3d.Origin - 1e6 * View.Forward` — breaks for models near/over 1e6 units; ortho approximation, parameterize or document.
- **MIS-GROUPED**: HLR drafting (namespace `Rasm.Fabrication.Projection`, feeds AppUi `Viewport2D`) shares folder `Posting/` with the G-code emitter — two unrelated downstream concerns. Belongs in its own `Projection/`.

---

## [B] GOVERNING DOCS

### ARCHITECTURE.md — needs rebuild (stale carriers + coarse map)
- **Line 16**: `Faults.cs # FabricationFault band-2500 composing kernel band-2400` — STALE. Landed law is `FaultBand.Fabrication = 2700` (README:20, TASKLOG `FAULT_REBAND_2700`, faults.md `FaultBand.Fabrication + 1..10` → 2701-2710). 2500 is the exact Element band the reband VACATED — doubly wrong.
- **Line 51**: seam prose "netDxf in AppUi owns the distinct WRITE leg" — STALE per census point (3) ("netDxf removed repo-wide; WRITE leg is the ACadSharp two-format DXF+DWG leg AppUi owns"). Re-point to ACadSharp two-format.
- **Line 63**: presents the `Nesting/workholding` multi-fixture-setup scheduler as an owned floor ("partitions a job's operations across reorientation setups…") — it is QUEUED (`MULTI_FIXTURE_SCHEDULE`), NOT in workholding.md.
- The `[01]-[DOMAIN_MAP]` 5-folder grouping is the coarse partition that fights the namespace domain model (see VC1).

### README.md — mostly current; charter/consumer drift
- Band 2700 correct (line 20). Router lists 17 pages.
- **Line 33 / Line 64**: netDxf cited as live `Rasm.AppUi` DXF-write dependency — STALE (census point 3). `Directory.Packages.props` STILL pins `netDxf` 2023.11.10 (line 359) AND `netDxf.netstandard` 3.0.1 (line 472) — the removal has not propagated to the manifest either.
- DOMAIN_PACKAGES charters are rich but 4 (DSTV.Net, OcctNet.Wrapper, PicoGK, SharpVoronoiLib) name consumers no page realizes (VC3).

### IDEAS.md — one stale card
- **`[GUARD]-[QUEUED]` (line 27)**: describes exactly what the realized `guard.md` (116 LOC) now owns (SweptVolume, Verdict union, Guard.Sweep/Check/Lift, GuardPolicy). Page realized, card still open-queued, no closed GUARD task exists → align-cards defect; should be CLOSED-COMPLETE.
- OPEN pool otherwise coherent: `PROBING` (no page, genuinely queued — no `Toolpath/probing.md`), `MULTI_FIXTURE_SCHEDULE` (queued, matches workholding gap), `DRL_NEST_POLICY` (BLOCKED, strata-correct — no Fabrication→Compute edge).

### TASKLOG.md — coherent
One OPEN (`NFP_DRL_POLICY` BLOCKED); 15 CLOSED with accurate dispositions matching pages. `FABRICATION_FAULT_BAND_DEEPENING` (line 44) retains pre-reband 2505-2509 codes, superseded by `FAULT_REBAND_2700` (line 47) — historical closed-card residue, acceptable but the lingering 25xx strings could confuse a reader.

---

## [C] CROSS-CUTTING

### C1. Folder↔namespace incoherence (systemic)
| Folder | Pages → namespace |
|--------|-------------------|
| `Nesting/` | nfp,stock → `Nesting`; workholding → **`Fixturing`** |
| `Polygon/` | clipper,import → **`Geometry2D`** |
| `Posting/` | program → `Posting`; projection → **`Projection`** |
| `Process/` | owner → `Process`; faults → **`Rasm.Fabrication`** (root); family,magazine → **`ProcessModel`**; physics → **`ProcessPhysics`** |
| `Toolpath/` | guard,motion,skeleton → `Toolpath`; kinematics → **`Kinematics`**; slicing → **`Additive`** |

5 folders host ~12 namespaces. The namespaces ARE the true seams (Process/ owns owner+model+physics+faults; Toolpath/ owns subtractive+kinematics+additive; Posting/ owns emit+HLR; Nesting/ owns nest+fixturing). The folder map is arbitrary. NAMING_SCHEMA [03] expects folder=namespace PascalCase alignment.

### C2. Host-neutral charter violated
13/17 pages `using Rhino.Geometry` directly (all except stock.md, faults.md). ARCHITECTURE.md:3 claims "host-neutral portable-fabrication owner"; ARCHITECTURE.md:56 declares geometry SHAPE comes from `csharp:Rasm`. stock.md proves the kernel-sourced alternative (`using Rasm.Vectors`). projection.md:120 `model.DuplicateNative()` then raw RhinoCommon `mesh.Faces/Vertices/FaceNormals` — reaches through the kernel `MeshSpace` to the native `Mesh`. Contradiction: resolve by routing geometry atoms through the `Rasm` kernel seam or dropping the host-neutral claim.

### C3. Unwired admitted packages (integration mandates)
| Package | Central pin | Page consumers | Named natural home |
|---------|-------------|----------------|--------------------|
| `SharpVoronoiLib` | 1.2.0 | ZERO (grep) | motion/skeleton CAM tessellation, spiral-pocket seeds (README [52]) |
| `PicoGK` | 2.2.0 | ZERO | slicing.md resin/powder + lattice supports (README [55]) |
| `OcctNet.Wrapper` | 0.1.1 | ZERO — NO domain-map node | NEW solid-ingress page beside import.md (STEP/IGES, README [61]) |
| `DSTV.Net` | 1.3.0 | ZERO | program.md NC1 `Emit` sibling (README [36]) |
All carry `.api/` catalogs + rich README charters. Per brief: each is an integration MANDATE, not a removal candidate. NOTE: `OcctNet.Wrapper 0.1.1` is a pre-1.0 single-maintainer wrapper — flag for the maintained/.NET-current gate.

### C4. Prose-vs-fence / dead carriers
- `ArcAlgebra` retirement claim (clipper.md:5,33) vs program.md still `g3.BiArcFit2`-refitting (VC4).
- import.md `warnings` accumulator captured then discarded, self-contradicting its own boundary rule (import.md:5,20,54).
- `Loop.Bulges` column defined+consumed only in clipper.md; upstream `owner.md` ratification unverified.

### C5. Hardcoding-vs-generator
- program.md:183 `0.0012 * RemovalRate` radial-force magic (→ physics cutting-data column).
- projection.md:100 `1e6` eye distance.
- nfp.md:137 `Ty*1e6+Tx` heuristic scale.
- All defensible individually; collectively the empirical constants want parameterization onto their owning policy/data surfaces.

### C6. Scope-expansion gaps (census mandate vs corpus)
Corpus is subtractive-CNC-biased. Against the PRODUCTION-GRADE mandate: additive is one thin FFF/DED page (`slicing.md`) with PicoGK (resin/powder/lattice) unwired; no post-processor dialect breadth beyond the 8-row `PostDialect` (no canned cycles / macro programming surface); no NC-verification / setup-sheet / shop-traveler / process-capability owner; DSTV steel-fab handoff unwired; OCCT solid ingress has no page. These are additions the folder-architecture overhaul must home as growth axes, not tack-ons.

---

## [D] VERDICT CANDIDATES (campaign-defining, evidence-first)

1. **FOLDER↔NAMESPACE OVERHAUL** — the 5-folder map hosts ~12 namespaces incoherently (C1 table). Re-partition to folder=namespace 1:1 growth structure; the namespaces reveal the real seams (Process/ProcessModel/ProcessPhysics; Toolpath/Kinematics/Additive; Posting/Projection; Nesting/Fixturing; Polygon→Geometry2D). THE structural ruling.

2. **HOST-NEUTRAL CHARTER vs Rhino.Geometry** — 13/17 pages import `Rhino.Geometry` directly despite ARCHITECTURE.md:3 "host-neutral"; stock.md shows the `Rasm.Vectors` kernel-sourced alternative and ARCHITECTURE.md:56 declares the seam. Rule: route atoms through the `Rasm` kernel or retract the host-neutral claim.

3. **FOUR UNWIRED ADMITTED PACKAGES = INTEGRATION MANDATES** — DSTV.Net (→program NC1 Emit), OcctNet.Wrapper (→new solid-ingress page), PicoGK (→slicing resin/powder+lattice), SharpVoronoiLib (→CAM tessellation). Centrally pinned + `.api` + charter, zero consumers (C3). Realize as rows/arms; none is a removal candidate.

4. **ARCALGEBRA IS A DEAD OWNER + FALSE RETIREMENT** — clipper.md defines the CavalierContours arc-space owner and claims it retires the biarc refit on motion/skeleton/program (clipper.md:5,33), but program.md still refits via g3.BiArcFit2 (247-282) and no page reads `ArcAlgebra`/`Bulges` (grep). Either wire arc-native lead/kerf/adaptive rails (realizing the claim + the `Loop.Bulges` widening) or retract.

5. **PROJECTION HLR IS NOT CAD-GRADE** — projection.md claims concave self-occlusion resolves + supersedes AppUi painter sort, but centroid-classified BSP without facet splitting (74-77), per-edge average depth (150), and all-mesh-edges-undeduped-no-crease (102,145) contradict it. Ruling: polygon-splitting BSP + per-fragment depth + feature/crease-edge filter, or downgrade the charter.

6. **STALE GOVERNING DOCS vs LANDED LAW** — ARCHITECTURE.md:16 band-2500 (→2700); ARCHITECTURE.md:51 + README.md:33,64 netDxf write-leg (census point 3 removes it, yet Directory.Packages.props still pins netDxf 2023.11.10 + netDxf.netstandard 3.0.1); ARCHITECTURE.md:63 workholding multi-setup scheduler presented as owned (QUEUED); IDEAS.md:27 `[GUARD]-[QUEUED]` realized-but-open. Waterfall: re-point the netDxf rejection to the ACadSharp two-format write leg, and flag the manifest netDxf residue.

7. **nfp.md Stock.Of() CONTENT-IDENTITY COLLISION** — line 88 hashes `Area` alone; distinct equal-area stocks collide, poisoning the `Remnant.Parent` lineage (line 68) the multi-sheet inventory depends on. Hash the discriminant + all dimensions (mirror `Remnant.Of`'s full-vertex hash).

8. **workholding.md MIS-FILED + UNSAFE KEEP-OUT** — filed in Nesting/ (namespace Fixturing) though a fixturing concern; `Clears`/`Condition` sample only endpoints+midpoint (73-83), missing thin-clamp crossings — a safety gap for its own "planned-exclusion-not-runtime-collision" charter. Move to Fixturing/; replace 3-point sampling with segment-vs-polygon intersection.

9. **PROGRAM CONDITIONING ASYMMETRY + HARDCODED FORCE** — the Motion arm (program.md:147) bypasses kerf/lead/pierce/tab/sequence (Placement-only, 148); and `Compensate` rides a magic `0.0012` cutting-force coefficient (183) that belongs as a specific-cutting-pressure column on physics.md's cutting-data table. Justify the split or unify; parameterize the coefficient.
