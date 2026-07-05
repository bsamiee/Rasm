# [M5_FAB_APPHOST_APPUI] — root-doc refresh mining (READ-ONLY census)

Scope: `RASM-CS-FABRICATION-BRIEF.md` (218), `RASM-CS-FABRICATION-DECISION.md` (309), `RASM-CS-APPHOST-BRIEF.md` (231), `RASM-CS-APPUI-BRIEF.md` (211), audited against the landed kernel (`libs/csharp/Rasm/.planning/`) and the corrected package `ARCHITECTURE.md` files (package-root, not `.planning/`).

## [00]-[LANDED_KERNEL_GROUND_TRUTH]

Kernel is ONE `Rasm` assembly, 9 folders = 9 namespaces (`dotnet_style_namespace_match_folder = true:error`, `Rasm/ARCHITECTURE.md:128-130`). NO `Geometry/` folder; `rg 'Rasm\.Geometry'` over the whole kernel package = ZERO. The geometry-pair era is gone; every `Rasm.Geometry.*` citation downstream is retired-pair drift.

| Concern | Real page | Real namespace |
|---|---|---|
| ContentHash.Of | `Domain/identity.md` | `Rasm.Domain` |
| PackOp/PackKind/EncodingKind/EncodingChannel/EncodedGeometry | `Drawing/pack.md` | `Rasm.Drawing` |
| DrawingProjection / View.Apply / HLR | `Drawing/view.md` | `Rasm.Drawing` |
| SliceStack / Slicing.Apply | `Meshing/slice.md` | `Rasm.Meshing` |
| IntersectOp.PlaneMesh | `Meshing/intersect.md` | `Rasm.Meshing` |
| Skeletonize.Apply / CurveSkeleton | `Meshing/skeleton.md` | `Rasm.Meshing` |
| Offsetting.Apply / Medial / Clearance | `Meshing/offset.md` | `Rasm.Meshing` |
| Arrangement.Apply / BooleanOp | `Meshing/arrangement.md` | `Rasm.Meshing` |
| ChartAtlas (unroll/atlas) | `Processing/flatten.md` | `Rasm.Processing` |
| HealOp (repair/heal) | `Processing/repair.md` | `Rasm.Processing` |
| MeshSpace admission | `Meshing/mesh.md` | `Rasm.Meshing` |
| geodesics/extract/flow/segment/register/intent | `Processing/*.md` | `Rasm.Processing` |
| Bounds/ConformanceMetric/select/query | `Analysis/*.md` | `Rasm.Analysis` |
| Stat.Of/Distribution.Of | `Domain/stats.md` | `Rasm.Domain` |
| AtomProjection/predicates | `Numerics/atoms.md`/`predicates.md` | `Rasm.Numerics` |
| **GeometryFault** (band 2400, incl. projection cluster 2436-2439) | `Numerics/faults.md` | `Rasm.Numerics` |
| MotionInterpolation | `Parametric/projections.md` | `Rasm.Parametric` |
| SpatialIndex/ToAcceleration/fields/cloud | `Spatial/*.md` | `Rasm.Spatial` |

Corrected package-ARCHITECTURE kernel-delegation rows (all slash-form under single `Rasm`, folder-true): Fab `ARCH:46` projection←`Rasm/Drawing/view`, `ARCH:48` slicing←`Rasm/Meshing/slice`, `ARCH:22 codemap` skeleton over kernel clearance family, `ARCH:55` nfp←`Rasm/Processing/flatten` ChartAtlas; AppHost `ARCH:67` Runtime←`Rasm/Drawing/pack`; AppUi `ARCH:72` Render←`Rasm/Drawing/view`, `ARCH:73` Render←`Rasm/Processing/flatten` ChartAtlas.

## [01]-[FABRICATION_DECISION] — 309 LOC

Verdict: the DECISION is FRESH — already reconciled against the landed geometry DECISION (V12 refutation `[00]`3, 5-tail census, UIBRIEF `[V7]` fix, probe FEASIBLE, folder-path seams, acyclicity proof, 4→3 leg merge). Defects are concentrated in the K-table namespace column and two missing/mis-pointed seams.

- **D1 — `Rasm.Geometry.*` namespace drift ×22** (dead retired-pair namespace). Hits: K1/K2 `Rasm.Geometry.Offsetting` (151,152), K3/K4 `Rasm.Geometry.Intersection` (153,154), K5 `Rasm.Geometry.Arrangement`+`Rasm.Geometry.Healing` (155), K6 `Rasm.Geometry.Projection` (156), K7 `Rasm.Geometry.Encoding` (157), K8-K20 bare `Rasm.Geometry` (158,160-167,169,170), `[00]` ruling refs (49,62,64), fault-registry Documentation `Rasm.Geometry.Projection ProjectionFault` (137). Re-point per `[00]` table: K1→`Rasm.Meshing`(offset.md), **K2→`Rasm.Meshing`(skeleton.md) [double drift: cited Offsetting, real page skeleton.md]**, K3→`Rasm.Meshing`(slice.md), K4→`Rasm.Meshing`(intersect.md), K5→`Rasm.Meshing`(arrangement.md)+`Rasm.Processing`(repair.md for Healing), K6→`Rasm.Drawing`(view.md), K7→`Rasm.Drawing`(pack.md), K8-K20→the specific real namespace per page.
- **D2 — GeometryFault namespace** `Process/family.md`/`faults.md` rows (62,64) cite `Rasm.Geometry GeometryFault` → real `Rasm.Numerics GeometryFault` (`Numerics/faults.md`, band 2400).
- **D3 — ProjectionFault PHANTOM** (137, K6 149-156). No type `ProjectionFault` exists; projection failures are the 2436-2439 cluster of `Rasm.Numerics.GeometryFault`, raised by `Rasm.Drawing` (view.md). "kernel `Rasm.Geometry.Projection ProjectionFault`" is doubly wrong (namespace + type).
- **D4 — SliceStack page mis-point.** K3 (153) + seam row 182 point `Slicing.Apply`/`SliceStack` at `Meshing/intersect`; real owner is `Meshing/slice.md` (ARCH `:48` already corrected to `Rasm/Meshing/slice`). intersect.md owns only `IntersectOp.PlaneMesh` (K4), composed UNDER slice.
- **D5 — MISSING ChartAtlas→nfp inbound seam.** Corrected Fab `ARCH:55` landed `Nesting/nfp ← Rasm/Processing/flatten` ([PROJECTION]: ChartAtlas unrolled UV islands + DistortionReceipt as true-shape part input). The DECISION nfp row 18 + seam-ledger rows 186-191 OMIT it entirely (nfp lists only →Persistence blocked, ←Compute DRL). New kernel-delegation the DECISION predates. NOTE: nesting engine correctly stays in-folder (NFP + 5 RectangleBinPack packers) — ChartAtlas is an INPUT projection, not the nesting owner; "nesting→ChartAtlas" is an inbound part-feed seam, not an engine delegation.
- **D6 — engine executability.** `[10]` rows 293-295 invoke `Workflow(rebuild.js, {targets:[…], brief})` — no `leg` selector (redesigned engine's targetless `{brief, leg}` form), riders are PROSE bullets in a table cell with line-number anchors (`PROPS:359`, `csproj:16`, `motion.md:71`), acceptance is prose F1-F4 `[11]`. Engine RIDER schema requires `{motion∈(manifest-drop|manifest-add|catalog-delete|counterpart-edit|verify), target, anchor(SYMBOL — never line number), wave, page?, guardPage?}`; TRACE requires `{name, needs:['<page>#<entry>'|'<seam-anchor>']}`. MODE.brief transcribes prose→typed, so executable, but the docs predate the typed interface and their line-number anchors violate the SYMBOL rule (DECISION itself concedes `±2` drift, `:199`).
- **D7 — chaff.** `[07]`/`[08]`/`[09]` (232-283) are three parallel disposition ledgers (V1-V10, E1-E14, 14 planes) re-projecting decisions the `[02]` page-set rows already encode — ~52 LOC of traceability narration that could collapse to one V/E/plane→rows cross-ref. `[11]` F1-F4 (299-309) partly echo the brief `[05]` (b)-(d) dry-runs. `[09]` grade columns ("8→9.5") are soft.

SOUND (preserve): partition `[01]`, page-set `[02]` schema, fault registry `[04]`, roster `[06]`, acyclicity/leg-merge `[10]`, folder-path seam citations (all folder-true bar D4).

## [02]-[FABRICATION_BRIEF] — 218 LOC

Superseded on several claims by its own DECISION (predates the kernel landing + the design pass).

- **B1 — `Rasm.Geometry.Healing`** (92) → `Rasm.Processing` (`Processing/repair.md` HealOp). Only Rasm.Geometry.* hit in the brief.
- **B2 — refuted reciprocal bindings** (DECISION `[00]`3 corrects): `PBRIEF [V12]` = "ArtifactKind egress rows / 2701-2710 decode" (5, 53, 145 E14) is FALSE — PBRIEF `[V12]` is governance-reconcile; the real binding is the held-open `[ARTIFACT_CONTENT_KEY_FEDERATION]` blocker. `CBRIEF [V12]` = "WasteAreaMm2 rollup counterpart" (5) is FALSE — CBRIEF `[V12]` is discipline-coverage; the waste rollup is a one-sided forward demand (`ARCH:51`).
- **B3 — stale counts/refs** the DECISION corrects: `[RESEARCH]`-tail census "three" (92 `[V2]`, 144 E13) → real 5 (adds `stock:317`,`program:379`); `UIBRIEF [V6]` (138 E7, 145 E14) → real `[V7]`.
- **B4 — missing ChartAtlas→nfp seam** (same as D5) — `[V1]` folder table (77) + `[05]` never name the kernel unrolled-part input.
- **B5 — engine executability** — EXECUTION table 14-16 `{targets:<DECISION leg-N targets>, brief}`, no leg-selector/typed riders (same class as D6).
- **B6 — chaff** — TELOS (33) ~350-word single sentence; `[03]` grade table (153-168) soft Now/Target columns.

SOUND: verdict `[00]`, integration-first roster law, seam/entry law, generator law — the DECISION builds faithfully on them.

## [03]-[APPHOST_BRIEF] — 231 LOC

Kernel-truth ALIGNED. Zero `Rasm.Geometry.*`. Kernel citations folder-true (`Domain/identity.md` ContentHash.Of, `Drawing/pack.md` PackOp/PackKind/EncodingChannel).

- **H1 — EncodingKind↔PackKind lock CORRECT.** `[SEAM_AND_RAIL_LAW]` (56) + `[V7]` (92) + `[V17]`/solver leg (218) anchor the Field/Toolpath encoding rows to kernel `PackKind` on `Drawing/pack.md` ("a field row packing geodesic/weight, a toolpath row packing position/weight"; solver signature-locks `EncodingKind.Toolpath`). Matches "verified law now." Correctly flags the `ARCH:67` pack-wire consumer miswire (`Runtime`→`Sandbox`, V17) — the current ARCHITECTURE L67 indeed still targets `Runtime`. Note: AppHost ARCHITECTURE has no PackKind/EncodingKind MODEL yet (only the L67 EncodedGeometry/PackOp wire) — the brief BUILDS it; correct for a rebuild brief, not drift.
- **H2 — engine executability** — EXECUTION 13-16 + `[05]` legs use `{targets:[<page set>], brief}`, prose riders (`[V…]`/`[04]` bullets, line-number anchors), prose (a)-(e) acceptance. Same class as D6; MODE.brief transcribes.
- **H3 — leg optimization (latent).** 4 legs dependency-sound (registry+manifest floor first is mandatory). Leg 3 (Agent+Wire) and Leg 4 (Sandbox+Observability) have no Agent/Wire→Sandbox/Observability data dependency beyond leg-1 registry + leg-2 ports — declarable as parallel waves; sequential is safe, so a soft optimization only.
- **H4 — chaff** — TELOS (35) ~400-word sentence; `[03]` grade table (167-178) soft; E-register (142-159) partly restates the V-verdicts.

SOUND: fault-registry V1, port-decode V2, wire-tier V7, kernel-identity V18 — all kernel-truth-consistent; `[V18]` correctly adds the `Rasm` ProjectReference.

## [04]-[APPUI_BRIEF] — 211 LOC

Kernel-truth ALIGNED. Zero `Rasm.Geometry.*`. Kernel citations folder-true (`Domain/identity.md`, `Drawing/view.md` DrawingProjection, `Processing/intent.md` VectorIntent.Pose, `Analysis/query.md`, `Parametric/projections.md` MotionInterpolation, `Spatial/index.md`).

- **U1 — DrawingProjection insulation CORRECT.** `[V7]`(b) keeps AppUi's producer on the Fabrication `HiddenLineResult` receipt so the kernel supersession is invisible; retires the stale declared-unwired `ARCH:72` Geometry `DrawingProjection` row two-sided. Matches landed truth.
- **U2 — ChartAtlas retire premise questionable federation-wide.** `[V7]`/`[V9]` rule `ARCH:73 ChartAtlas` RETIRE ("no page demands a UV atlas today"). AppUi's OWN texture-UV use is genuinely dead — but kernel `Processing/flatten` ChartAtlas now has a LIVE consumer: Fabrication `nfp` (Fab `ARCH:55`). The two-sided kernel `flatten→AppUi` mirror retire is handled, but the kernel ChartAtlas OWNER does not retire; the brief's federation-wide "no consumer" framing is stale (the demand moved to Fabrication). Low severity — AppUi's retire is still correct.
- **U3 — stale BSP-solver label unflagged.** AppUi `ARCH:75` still labels the Fabrication seam "BSP visibility solver"; Fabrication `ARCH:46` retired the in-folder BSP for kernel `DrawingProjection`. Brief E6/`[V7]` treats `ARCH:75-76` as two-sided-correct without noting the stale wording — a small missed re-point in the `[V9]` ledger sweep.
- **U4 — engine executability** — EXECUTION 13-16 + `[05]` legs `{targets:[<page set>], brief}`, prose riders + prose (a)-(e) acceptance. Same class as D6.
- **U5 — chaff** — TELOS (35) ~450-word single sentence; `[V6]` (100) is one ~600-word mega-paragraph chaining ~20 fence repairs (extreme density, borderline parse); `[03]` grade table (152-165) soft.

SOUND: fault-registry V1 (6xxx), collab-recharter V2, folder-repartition V3, residency-consumption V5, seam/rail law — all kernel-truth-consistent.

## [05]-[CROSS_CUTTING]

- **X1 — engine-interface drift (all 4 docs).** Every `rebuild` invocation is `{targets, brief}`; none use the redesigned engine's `{brief, leg}` leg-selector, typed `riders {motion,target,anchor(SYMBOL),wave,page?,guardPage?}`, or `acceptance {name,needs}` traces. Riders/acceptance are prose with line-number anchors (violating rider `anchor: SYMBOL — never a line number`). Engine `MODE.brief` transcribes prose→typed at Plan time, so the legs ARE executable — the drift is interface modernization, not a blocker.
- **X2 — `Rasm.Geometry.*` isolated to Fabrication.** DECISION ×22, BRIEF ×1; AppHost/AppUi ZERO. AppHost/AppUi already use folder-path slash citations matching the corrected ARCHITECTUREs.
- **X3 — ChartAtlas dual state.** Kernel `Processing/flatten` ChartAtlas: retired at AppUi (texture-UV, dead), live at Fabrication nfp (unrolled-part input, seam missing from BRIEF+DECISION). The kernel `flatten→AppUi` mirror should re-scope toward Fabrication.
- **X4 — README/ARCHITECTURE staleness (expected, planned).** Fab README L11 + AppUi `ARCH:75` still describe the pre-kernel in-folder BSP; both are scheduled for the leg-1 router/codemap rewrite, so not doc defects per se — but AppUi's brief does not explicitly name the `ARCH:75` wording re-point (U3).

## [06]-[COUNTS]

| Doc | LOC | Rasm.Geometry.* | Anchored defects | Class |
|---|---|---|---|---|
| FABRICATION-DECISION | 309 | 22 | D1-D7 (7) | fresh; K-table namespace drift + 1 missing seam + 1 mis-pointed page |
| FABRICATION-BRIEF | 218 | 1 | B1-B6 (6) | predates DECISION; ≥4 claims DECISION-refuted |
| APPHOST-BRIEF | 231 | 0 | H1-H4 (4, H1 confirms-correct) | kernel-aligned; interface + chaff only |
| APPUI-BRIEF | 211 | 0 | U1-U5 (5, U1 confirms-correct) | kernel-aligned; 1 stale-label miss + interface + chaff |

Cross-cutting: X1 (engine interface, all 4), X2-X4.
