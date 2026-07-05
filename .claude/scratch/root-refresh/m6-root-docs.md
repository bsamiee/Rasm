# M6 — ROOT CAMPAIGN DURABLES: DOCUMENT-LAYER AUDIT

Scope: all 16 root `RASM-*.md` durables as DOCUMENTS — structure, signal, coherence, engine-contract fitness. Every claim line-anchored. Engine truth = `.claude/workflows/rebuild.js` (583 LOC): `{targets, brief, leg, waves, riders, acceptance}` entry, targetless `{brief, leg}` valid, one-run multi-leg `leg:[..]`, typed riders `{motion∈[manifest-drop,manifest-add,catalog-delete,counterpart-edit,verify], target, anchor(SYMBOL), page?, guardPage?, wave}`, traces `{name, needs:['<page>#<entry>']}`, listed order IS dependency order (never re-sorted), one serialized per-wave tail = sole central-file writer, fail-open fix-in-place fable acceptance close, NO reconcile stage, NO residual routing.

## [01]-[COUNTS_PER_DOC]

| Doc | Chaff | Stale-engine rows | Fragile counts | Dead/drifted refs |
|---|---|---|---|---|
| CS-GEOMETRY-BRIEF | 4 | 4 (EXECUTION table) | 9 | 0 hard |
| CS-GEOMETRY-DECISION | 5 (3 = disposition tables) | 5 (leg rows + acceptance) | 8 | 0 hard |
| CS-PERSISTENCE-BRIEF | 4 | 0 | ~11 | 6 geometry cites + 4 WRONG line anchors |
| CS-PERSISTENCE-DECISION | ~0 | 0 | ~8 | 5 geometry cites + 1 self-contradiction (`:324`) |
| CS-COMPUTE-BRIEF | 6 | 1 hard (`:13` impossible invocation) + 2 soft | 11 | 8 geometry-handle lines |
| CS-APPHOST-BRIEF | 6 | 0 hard + 3 soft residual-noun | 9 | 8 geometry-handle lines |
| CS-APPUI-BRIEF | 6 | 0 hard + 3 soft | 12 | 9 geometry-handle lines |
| CS-FABRICATION-BRIEF | 6 | 0 | 6 | ~17 geometry pins + 3 refuted upstream bindings |
| CS-FABRICATION-DECISION | 5 | 4 (`:117` reconcile phantom, `:143/:287/:289` advisory misuse) | 12 | ~11 geometry-DECISION line pins |
| GENERATION-SPEC | ~2 | 1 (`:179` future-gates a landed campaign) | ~4 | 3 kernel-gate cites |
| PY-ARTIFACTS-BRIEF | ~6 | 3 (`:12-14`, `:264`, `:270`) | ~16 | 2 upstream brief-handles |
| PY-ARTIFACTS-DECISION | ~9 | 9 (`:27,:82,:281,:287,:300,:469,:472,:478-499`) | ~14 | 0 |
| PY-RUNTIME-BRIEF | 3 | 4 (`:45,:202,:204,:206`) | ~11 | fwd-refs to artifacts |
| PY-DATA-BRIEF | 3 | 5 (`:46,:82,:217,:219,:221`) | ~12 | 2 |
| PY-GEOMETRY-BRIEF | 3 | 4 (`:47,:121,:199,:202,:204`) | ~14 | 2 |
| PY-COMPUTE-BRIEF | 2 | 4 (`:46,:117,:200,:202,:204`) | ~13 | 2 |

Cleanest pair: PERSISTENCE (engine-language fully current; its defects are wrong line anchors and one false count). Dirtiest: PY-ARTIFACTS-DECISION (a whole leg architected on a phantom reconcile stage) and FAB-DECISION (phantom reconcile pass + advisory-order excuse for real inversions).

## [02]-[CHAFF_NOISE_CENSUS]

Corpus-wide classes first, then per-doc uniques.

[CLASS_A] — "`docs/stacks/<lang>/` governs unchanged and is never restated here": GEO-B:5, PERS-B:5(variant), COMP-B:5, APPHOST-B:5, APPUI-B:5, FAB-B:5(variant), RT:3, DT:3, GE:3, CO:3, PY-ART-B:5. Meta-narration about not-restating; the instruction chain already binds the doctrine. DELETE the clause everywhere (keep at most "`docs/stacks/<lang>/` governs").

[CLASS_B] — "LANDED (initial pass)" disk-state narration baked into durable law: RT:81-82, DT:88, GE:77. Point-in-time progress notes that stale on every disk change; the plan classifies kind from disk. DELETE; fold the one durable ruling each carries into its owning verdict as a ruled row.

[CLASS_C] — freshness/version tails in briefs (versions live in the manifest): COMP-B:58,195-198,202-204 ("feed-verified 0.2.0", "4.12.2 (2023…)", "5.0.0-beta-77"); APPHOST-B:204-206; APPUI-B:72 ("feed-latest 2023.11.10", "ARCHIVED since 2023-10"). REWRITE: admission by package name + license/RID gate only.

[CLASS_D] — `[PYPROJECT_RECONCILIATION]`/roster run-on walls (~40-50-line single paragraphs carrying 12-15 rulings): GE:67 (worst in corpus), DT:66, CO:65, RT:63, APPUI-B:72, APPHOST-B:132. REWRITE one-ruling-per-line or table.

[CLASS_E] — superlative verdict-framing: PERS-B:22,34,84 ("genuinely world-class" ×3). REWRITE to the named property ("exactly-once egress"), drop the grade word.

Per-doc uniques:
- GEO-B:86 ("the design pass may exceed them" — design pass already ran; DELETE), GEO-B:169 (restates own role; DELETE).
- GEO-D:136-196 — three parallel disposition tables (V/E/delta, ~60 lines) re-projecting `[02]`; on archive pure restatement. Collapse to one attestation line.
- GEO-D:247-249 duplicates GEO-B:208-210 rejection roster verbatim. One owner; DELETE the DECISION copy.
- FAB-D:3 — "Every V1-V10, E1-E14, 14 capability plane, and 24 roster package is disposed — no open item survives" — closure-frame + quad fragile count. REWRITE as pointers to `[06]-[09]`.
- PY-ART-D:5 — the all-count headline ("69 live pages … 42 producers … 23 receipt cases … 5 inversions … 4 cycles"). REWRITE as section pointers.
- APPHOST-B:39 "(767 LOC…)", APPUI-B:35 "~8.7k design LOC" — LOC-in-prose; drop.

[FRAGILE_COUNT_LAW] (rewrite shape for all ~150 sites): drop the numeral or convert to a checkable attestation against the owning table/disk (`rg`-zero gates, "the `[02]` table is the roster", "one row per page"). Exemplars: GEO-B:5 "the 18 settled pages"/"34 landed kernel pages (7/21/6)" (duplicated GEO-D:3 — dual-drift); FAB-D:27 "13 folders = 13 namespaces"; FAB-D:53 "33 rows"; FAB-D:112 kind tallies; PERS-B:60 "(30)" shared catalogs — ALREADY WRONG (disk = 31); CO:89 "117 occurrences, 53 … 15 pages"; GE:81/CO:54 "frozen 13-literal union" ×5 sites; PY-ART-B:159 "69 spellings … 44 pages"; APPUI-B:211 "25-decade headroom".

## [03]-[ENGINE_CONTRACT_AUDIT]

### Hard fictions (stages that do not exist)

1. FAB-D:117 — "the W3 arm-fill edits land as recorded leg residuals closed by the engine's reconcile pass". NO reconcile pass. Self-contradicts FAB-D:287,297 and FAB-B:9,203. CORRECTED: each leg-3 plane agent fills its own `owner#run` arm in-pass; `owner.md` shared-file edits serialize through the leg-3 per-wave tail (the sole-writer rule); no second `owner.md` rebuild target.
2. PY-ART-D:300 — "LEG-1B corpus-wide reconcile (whole-repository write authority): the emit() rewire on ALL 42 producers". Structurally impossible: leg 1b owns 7 pages; the other producers land `emit()` in their OWN legs from `[01]-[ENTRY_CONTRACT]`. Propagates: `:27` ("in the leg-1b reconcile"), `:82`, `:281`, `:287` ("gated on 1a residual-clean" → "gated on 1a landed"), `:469`, `:472`; echoed PY-ART-B:264.
3. PY quad — "this leg's reconcile" noun at RT:202,204 / DT:82,217,219 / GE:121,199,202 / CO:117,200,202, with DT:217/GE:199/CO:200 escalating to "applied to every page, not only this leg's" — engine-illegal cross-leg mutation (a leg builds only its pages; the tail writes only index docs + manifests). CORRECTED: renames land per-page in-pass as each owning page rebuilds; the acceptance agent's symbol sweep fixes stragglers in place.
4. PY quad — "read-only acceptance pass … rejects any back-edge" at RT:45, DT:46, GE:47, CO:46. Acceptance is fail-open and FIXES in place. CORRECTED: "the fail-open acceptance agent re-derives the import graph and fixes any back-edge in place."
5. Residual-forward-transfer clause — "residuals naming a later leg's pages transfer forward explicitly in the leg return, never silently" at RT:206, DT:221, GE:204, CO:204, PY-ART-B:270, GEO-B:221 (variant), FAB-D echoes. No leg-return residual channel exists. DELETE the clause; the adjacent in-pass-repair sentence is correct and stands alone.

### Invocation-form defects (every EXECUTION table)

- COMP-B:13 — `{targets: "libs/csharp/Rasm.Compute", brief}` with PRODUCES "One leg landed per run": ENGINE-IMPOSSIBLE (no `leg` param = all brief pages expand = all 4 legs one run). CORRECTED: `{brief, leg: N}` or `{brief, leg:[1,2,3,4]}`.
- APPHOST-B:13-16, APPUI-B:13-16 — `targets: [<[05] leg-N page set>]` placeholders hand-copy the `[05]` partition (drift channel). CORRECTED: `{brief, leg: N}`, targets unset.
- GEO-B:11-18, FAB-B:14-16, PERS-B:14-16 — `targets: <DECISION leg-N targets>` prose placeholders, no `leg`/`riders`/`acceptance`, one-run form never mentioned despite strictly ordered legs. CORRECTED: `{brief: "<DECISION>", leg: N}` per row, plus a stated `leg:[1..N]` one-run option.
- RT:11, GE:11, CO:11 — `{targets: "<folder>", brief}` with "one launch per leg" prose (same impossibility as COMP). DT:11 `{brief, leg: N}` is the ONLY engine-correct EXECUTION row in the corpus — unify all 16 docs on DT's shape.
- PY-ART-D:478-499 — four invocation blocks: no `leg`, no `riders`, no `acceptance`, redundant target arrays that will drift from `[03]`. CORRECTED: one-run `{brief, leg:["1a","1b","2","3"]}` + typed riders + typed traces.

### Riders and acceptance never typed

No doc emits typed rider rows or traces; all live as prose the plan sonnet must lower, and the receipt-forcing/tail-audit machinery is forfeited:
- GEO-D:257-260 — riders as prose with LINE-NUMBER anchors (`COMPUTE:121`, `Persistence:55`, `FAB:44`, `mesh.md:647`) — contract: anchor is a SYMBOL. Corrected exemplars: `{motion: counterpart-edit, target: Rasm.Compute/ARCHITECTURE.md, anchor: ClashDecode, wave: 1}`; GShark leg-4: `{motion: manifest-drop, target: Directory.Packages.props, anchor: GShark, wave: 4}` + `{motion: catalog-delete, target: libs/csharp/Rasm/.api/api-gshark.md, guardPage: Parametric/nurbs.md, wave: 4}`.
- FAB-D:293-295 rightmost column — same defect (`{catalog-delete, api-rectpacksharp.md, guardPage: Nesting/nfp.md, wave:1}` etc. owed as rows; `PROPS:359`/`csproj:16` line pins illegal as anchors).
- PERS-D:282,299 — catalog/manifest motions prose-only; recommend explicit `catalog-delete`/`manifest-drop`/`manifest-add` rows with symbol anchors.
- PY-ART-D:412 + `[04]`/`[09]` — pvlib admit, pyexiv2/iptcinfo3/xmp drops, zlib-ng re-entry all owed as typed riders (shapes given in the audit).
- Acceptance: GEO-B:221/GEO-D:231 dry-runs, FAB-D:299-309 F1-F4, PERS-D:315 (a)-(e), PY-ART-D:519-521 flagships — all prose narratives; each owes `{name, needs:[…]}` rows (worked traces exist for geometry: `{name:"generation-spineref", needs:['Spatial/reconciliation.md#Encode','Parametric/curve.md#Stations','Parametric/subdivide.md#Subdivision.Apply','Parametric/panelize.md#Panelization.Apply','Parametric/patternmap.md#Patterning.Apply']}` etc.; for fabrication: `{name:"F1-posting", needs:['Toolpath/motion.md#Cam.Solve','Posting/program.md#Post.Parse','Posting/dialect.md#Dialect.Emit','Process/owner.md#ArtifactKind']}`).

### Dependency-order violations

- FAB-D:143,287,289 — "Intra-leg order is ADVISORY … NOT a strict leaf-before-consumer sort" is FALSE against the contract (listed order IS dependency order; batches chain behind predecessors) and excuses three real inversions in the leg-3 array (FAB-D:295): `motion`(pos 12) before `surface`(13) though motion→surface; `production`(11) before `slicing`(14); `probing`(18) before `program`(19). CORRECTED: delete the advisory framing; re-sort so `surface`/`slicing`/`program` precede their consumers.
- PY-ART-B:12-14 — 3-leg EXECUTION rows predate the DECISION's 4-leg (1a/1b/2/3) ruling.

### Clean rows (attested, no fix)

PERS-B + PERS-D: zero stale-engine phrases; in-pass + tail model verbatim correct (PERS-B:206, PERS-D:295). COMP-B:210/APPHOST-B:213/APPUI-B:191 "listed order IS dependency order" — compliant. FAB kind/absorb/deletePages mechanics (FAB-D:55,112,113,114) — contract-exact, including delete-travels-on-absorb.

## [04]-[WATERFALL_COHERENCE]

### Retiring geometry pair — the cite fan and its replacement anchors

Successor docs cite the pair via 4 handle forms, all of which dangle on archive: (1) filename cites, (2) `GBRIEF:NNN` line pins, (3) "geometry DECISION line NNN" pins — used at FAB-D:23,151,152,155,156 while FAB-D:147 FORBIDS exactly that form, (4) bare `[V#]` handles. Full replacement map (`libs/csharp/Rasm/.planning/`-relative; disk-verified):

| Retiring handle | Landed disk anchor |
|---|---|
| `[V1]` partition | `Rasm/ARCHITECTURE.md#[03]` |
| `[V2]` parametric tier / canonical-bytes | `Spatial/reconciliation.md#[02]-[RECONCILIATION_BRIDGE]` (`EncodeForm.Parametric`) + `Parametric/nurbs.md#[IDENTITY_AND_CONSUMERS]` (`NurbsForm.ToEncodeForm`); pages `Parametric/{curve,surface,subdivide,develop,panelize,patternmap}.md` |
| `[V3]` view engine | `Drawing/view.md#View.Apply` (`DrawingProjection`) + `Rasm/ARCHITECTURE.md:91,95` |
| `[V4]` crossing/implicit | `Meshing/intersect.md#Intersection.Apply`; `Meshing/arrangement.md#Constraint`; `Numerics/predicates.md#Predicate` |
| `[V5]` booleans | `Meshing/arrangement.md#Arrangement.Apply` (`BooleanOp`, `ScaleCeiling`); `Numerics/faults.md#NativeAssetMissing` |
| `[V6]` substrate | `Meshing/edit.md#MeshEdit` |
| `[V7]` one-LM | `Solving/solver.md#Lm.Minimize` |
| `[V8]` index/distance-field lane | `Spatial/index.md#Spatial.Apply`; lane = `Spatial/fields.md#SignedDistanceFromMesh` → `Meshing/reconstruct.md` GWN → index BVH; `GeometryHash` = `Spatial/reconciliation.md#[01]` + `Rasm/ARCHITECTURE.md:90` |
| `[V10]`a-d | `Meshing/offset.md#Offsetting.Apply`/`#Clearance`/`#Medial`; `Meshing/slice.md#Slicing.Apply`; `Meshing/skeleton.md#Skeletonize.Apply`; `Meshing/delaunay.md#VoronoiDual` |
| `[V11]` hull/remesh | `Meshing/delaunay.md#LowerHull` + `Spatial/cloud.md`; `Processing/remesh.md` |
| `[V12]` fault band | `Numerics/faults.md#GeometryFault` (2400-2449) |
| `[V13]` pack residency | `Drawing/pack.md#PackKind` + `Rasm/ARCHITECTURE.md:92` |
| `[V14]` ledger duties | `Rasm/ARCHITECTURE.md:120,121` rows + `Numerics/matrix.md` |
| `[LANDED_KERNEL_LAW]` / hasher | `Domain/identity.md#ContentHash` (`#[02]-[CONTENT_KEY]`) + `Rasm/ARCHITECTURE.md:87-89` |
| FAB K1-K23 table (FAB-D:147-173) | full disk map delivered: K1 `Meshing/offset.md#Offsetting.Apply` · K2 `Meshing/skeleton.md#Skeletonize.Apply` · K3 `Meshing/slice.md#Slicing.Apply` · K4 `Meshing/intersect.md#Intersection.Apply` · K5 `Meshing/arrangement.md#Arrangement.Apply` · K6 `Drawing/view.md#View.Apply` · K7 `Drawing/pack.md#Encode.Apply` · K8 `Spatial/fields.md#SignedDistanceFromMesh` · K9 `Domain/identity.md#ContentHash` · K10-K13 `Processing/{geodesics,extract#ExtractionDomain,flow#FlowKernel.Trace,segment}.md` · K14 `Analysis/measure.md#Bounds`+`Meshing/delaunay.md#LowerHull` · K15 `Meshing/mesh.md#MeshSpace`+`Processing/repair.md#HealOp` · K16 `Processing/register.md#AlignKind` · K17 `Analysis/measure.md#ConformanceMetric` · K18 `Domain/stats.md#Stat.Of` · K19 `Parametric/projections.md#MotionInterpolation` · K20 `Analysis/select.md#Faces` · K21-K23 `Domain/rails.md`/`Domain/validation.md`/`Numerics/atoms.md#AtomProjection` |

Per-doc cite sites to re-point: PERS-B:5,108,120,137,211 + PERS-D:3,92,173,329,344; COMP-B:5,23,104,124,132,150,160,223; APPHOST-B:5,55,56,68,132,148,226; APPUI-B:5,29,66,104,112,137,205; FAB-B:5,145,190,205 + FAB-D:23,147,151-156; GENERATION-SPEC:3,179,226.

### GENERATION-SPEC gate inversion

SPEC:179 — "a Vectors/Geometry rebuild campaign PRECEDES Generation stand-up" gates on a campaign that has ALREADY LANDED to disk. Reframe as verify-against-landed-disk per operation: stations/frames → `Parametric/curve.md`+`locate.md`; isolines/geodesics/offsets → `Parametric/surface.md`+`Processing/geodesics.md`+`Meshing/offset.md`; tessellation/subdivision → `Meshing/delaunay.md`+`Parametric/subdivide.md`; developable/panelize → `Parametric/develop.md`+`panelize.md`; pattern-map → `Parametric/patternmap.md`; predicates → `Numerics/predicates.md`. SPEC:226 owner cell same fix. Inbound: GEO-B:26,94,151 + GEO-D:200 cite the spec BY LINE NUMBER (`RASM-GENERATION-SPEC.md:179,226,164,138,173`) — line pins into a live spec; convert to section anchors (`#[06]-[EXPOSURE_CONTRACT]` etc.).

### Disk-verified WRONG anchors (persistence pair, live today)

- PERS-B:42 — hasher law anchored `Rasm/ARCHITECTURE.md:76-81` = the `Analysis/` directory tree, NOT the seam rows. Real: `:87-89`.
- PERS-B:53 — "no second hasher at the codec" cited `:77` (= `Query.cs` tree line). Real: `:88`.
- PERS-D:92,173,344 — GeometryHash counterpart re-point targets `:79` (= `Inspect.cs`). Real: `:90`.
- PERS-B:28,112 — flagship RPO defect anchors `recovery.md:181`/`:172`; disk = `:180`/`:171` (self-caught at PERS-D:346 but left uncorrected in the BRIEF).
- Triangle-4 corollary: COMP-B:51 cites the same hasher law at `Rasm/ARCHITECTURE.md:78`, APPHOST-B:55,136 + APPUI-B:54,65 at `:76-81` — three anchor spellings for one law, at least two wrong. Canonical: `Domain/identity.md#ContentHash` + `ARCH:87-89`.

### Upstream re-litigation / refuted bindings

- FAB-B:5,142,145 assert `PBRIEF [V12]` = ArtifactKind egress index and `CBRIEF [V12]` = `NestYield.WasteAreaMm2` rollup — BOTH disk-refuted by FAB-D:17 (real: `[V12_GOVERNANCE_RECONCILE]`/`[V12_DISCIPLINE_COVERAGE]`; real binding = `[ARTIFACT_CONTENT_KEY_FEDERATION]` blocker + one-sided forward demand). BRIEF uncorrected — a build agent reading it inherits two wrong upstream contracts.
- FAB-B:92,144 "three `[RESEARCH]` tails" vs FAB-D:17,237,263 disk-verified FIVE. FAB-B:145 `UIBRIEF [V6]` vs FAB-D:107,264 corrected `[V7]`.
- laspy reversal: DT:66 finalizes `[GEOMETRY_DATA]`; GE:67,143 overrides to `[DATA]` (zero geometry imports) — downstream re-opens an upstream finalized tag. Fix at source: DT tags `laspy [DATA]`.
- dask double-author: DT:120 "it relocates to `libs/python/.api/dask.md`" vs CO:125,65,191 "this campaign executes" — data RULES, compute EXECUTES; rewrite DT:120.
- sparse/rustworkx: CO:65,188,146 present data-finalized retags as compute motions; under one-writer + waterfall compute owes `verify` riders (netcdf4 at CO:65 is the correct pattern).
- PY-ART-B:59 (`RASM-PY-DATA-BRIEF [00]`) and :152 (`RASM-PY-GEOMETRY-BRIEF [V1]/[V15]`) — `[V#]` handles treat finalized upstream as open briefs; resolve to landed `.planning` disk anchors.

### Drift triangles (shared law spelled divergently)

1. Rebuild-invocation form — 5 spellings across 16 docs; only DT:11 correct (map in `[03]`).
2. Compute fault-band extent — COMP-B:22,88 "2200-2220" vs APPHOST-B:68,148 "2200" vs APPUI-B:80 "2200-2299". Mirror-row law demands one spelling: 2200-2220.
3. "No DECISION pass" — COMP-B:9 "no DECISION pass" vs APPHOST-B:9/APPUI-B:9 "single-phase" (which misreads against their own 4-row tables). Unify on "no DECISION pass".
4. One-hasher law anchor — 3 spellings (above).
5. Strata-law anchor — COMP-B:40 `architecture.md:35,54` vs APPHOST-B:42 `:18-21,38` vs APPUI-B:54 `:19,40`; cite the section symbol, not per-doc lines.
6. Tessellation field floor — RT:75 defines; GE:59,85 re-specifies identically. Runtime owns; geometry cites "the runtime-`[V2]` field floor" without re-enumeration.
7. "Frozen 13-literal union" — GE:81,213 + CO:54,73,133,206: five sites pin the count; one source of truth (geometry), compute cites by name.
8. Per-leg closeout paragraph — near-verbatim ×10 docs, all carrying the dead residual-forward clause; fix the template once, propagate.
9. Kernel census "34 landed (7/21/6)" — GEO-B:5 + GEO-D:3 dual-drift; single-source as attestation.

### Internal pair drift

- GEO pair: no contradictions; 4 recorded legal supersedes (Drawing rename GEO-B:90 vs GEO-D:21; 12-vs-13 clusters B:134 vs D:23,42; V4 re-scope B:102 vs D:17; quad-remesh B:130 vs D:97,150). DECISION wins each.
- PERS-D:324 — "19 own bands" contradicts its own 21-row `[03]` registry and PERS-D:11,130 "21". Hard false count; fix to 21.
- PY-ART pair: inversions 4 (B:41,268) vs 5 (D:5,84,94); Layer fan six (B:146,268) vs NINE (D:92); cycles 3 (B:55) vs 4 (D:84,99); carrier shapes 6 (B:22,53) vs 11 (D:31); `ColorReceipt` "collapses into" (B:54) vs DELETED + new `Color` case (D:45,291). DECISION wins; BRIEF headline counts must not survive as law.
- FAB pair: `[RESEARCH]` tails 3 vs 5, `[V12]` bindings, `UIBRIEF [V6]→[V7]` (above); FAB-D:147 forbids the line-pin form FAB-D:151-156 uses.

## [05]-[LEG_PHASE_STRUCTURE]

- GEO: 4-wave partition sound; acyclicity proof real (D:253). Missed one-run `leg:[1,2,3,4]` (the corpus's single biggest structural under-use). Wave 4 = 12 pages with THREE independent lanes (Meshing-new / Parametric / Drawing, B:219) never named as lanes — a successor could serialize them.
- PERS: honest 3-leg partition on real boundaries; leg 3 heavy (14 pages) but correctly one wave-partitioned lane; acyclicity proof complete (D:303-311). Missed one-run only.
- COMP/APPHOST/APPUI: 4-leg partitions all honest with real dependency evidence (registry-first, extraction-before-donor-rebuild, sealed `[FAULT_TABLES]`); APPUI strongest (explicit zero-forward-dependency notes, B:196). Defect is purely invocation-layer: leg structure lives in `[05]` prose with no launch selector; one-run unused.
- FAB: not spam — the opposite: leg 3 is a 21-page mega-leg whose 3 dependency inversions are excused by the false "advisory" clause; the `owner.md` two-touch mechanism is undefined under the one-writer rule (the reconcile phantom fills the hole). Re-sort + route arm-fills through the leg-3 tail.
- PY-ART: 1a/1b is mini-leg spam — the 1a→1b dependency is a within-leg wave barrier, not a leg boundary; the only justification (":287 gated on 1a residual-clean") is itself stale reconcile-reasoning. Collapse to 3 legs or run `leg:["1a","1b","2","3"]` one-run. Leg 3 "cold verify-only" premise rests on the false 1b-corpus-rewire (it is a normal build leg).
- PY-RT/DT/CO: clean topological partitions, no spam; all miss the one-run form; RT/GE/CO invocations cannot even do per-leg (no `leg` param).
- PY-GE: missed parallelism — energy chain (leg 4) depends only on `graduation` (leg 1), independent of the leg-3 wire plane (GE:44); serializing 3→4 has no dependency basis. Two independent waves or `leg:[3,4]`.
- Fable utilization: correct everywhere — no brief hard-codes agent models (the engine owns the split); the real gap is upstream: untyped riders/traces force the plan sonnet to re-derive rows the DECISIONs should emit.

## [06]-[CONTEXT_POISON_TOP_10]

1. `RASM-PY-ARTIFACTS-DECISION.md:300` — "LEG-1B corpus-wide reconcile (whole-repository write authority): the emit() rewire on ALL 42 producers." A whole leg architected on a phantom stage; poisons `:27,:82,:281,:287,:469,:472` and all four invocation blocks. FIX: each producer lands `emit()` in its own leg's build pass from `[01]`; 1b rewires only its 7 pages; the per-wave tail owns central files.
2. `RASM-CS-FABRICATION-DECISION.md:117` — "closed by the engine's reconcile pass" for the `owner.md` W3 arm-fills — the two-touch mechanism of a 21-page leg delegated to a stage that does not exist. FIX: plane agents fill arms in-pass; shared-file edits serialize through the leg-3 tail.
3. `RASM-PY-DATA-BRIEF.md:217` / `RASM-PY-GEOMETRY-BRIEF.md:199` / `RASM-PY-COMPUTE-BRIEF.md:200` (+ `RASM-PY-RUNTIME-BRIEF.md:202`) — "This leg's reconcile carries the sweeps … applied to every page, not only this leg's." Engine-illegal cross-leg mutation, replicated 4×. FIX: renames land per-page in-pass at each owning page's build; acceptance sweep fixes stragglers.
4. `RASM-CS-COMPUTE-BRIEF.md:13` — `{targets: folder, brief}` + "One leg landed per run": engine-impossible; an operator following it lands all 4 legs unknowingly. Same class: RT:11, GE:11, CO:11, APPHOST/APPUI `<[05] leg-N page set>`, GEO/FAB/PERS `<DECISION leg-N targets>`. FIX: unify on `{brief, leg: N}` / `{brief, leg:[..]}` (DT:11 is the corpus's one correct row).
5. `RASM-CS-FABRICATION-DECISION.md:289` — "Intra-leg order is ADVISORY … motion before its `surface` arms … harmless." False against the batch-chaining contract and masks 3 real inversions (`:295`). FIX: delete; re-sort `surface`/`slicing`/`program` before their consumers.
6. `RASM-GENERATION-SPEC.md:179` — "a Vectors/Geometry rebuild campaign PRECEDES Generation stand-up": gates on a campaign already landed; tells every future agent the kernel does not exist. FIX: verify-against-disk per operation over `Parametric/{curve,locate,surface,subdivide,develop,panelize,patternmap}.md`, `Meshing/{offset,delaunay}.md`, `Processing/geodesics.md`, `Numerics/predicates.md`.
7. `RASM-CS-FABRICATION-DECISION.md:151-156` — K-row dispositions pinned to "geometry DECISION line 149/123/125/222/266" while `:147` forbids exactly that form; all dangle on archive. FIX: re-anchor K1-K23 to the disk `#Symbol` table in `[04]` above. (Class representative for every `GBRIEF:NNN`/filename cite across PERS/COMP/APPHOST/APPUI/FAB.)
8. `RASM-CS-PERSISTENCE-BRIEF.md:42,53` (+ DECISION:92,173,344) — the load-bearing one-hasher law anchored to the kernel `Analysis/` directory tree (`ARCH:76-81`/`:77`/`:79`); real rows `:87-89`/`:88`/`:90`. Disk-verified wrong TODAY; an agent verifying the law reads `Query.cs`/`Inspect.cs` lines. FIX: `:87-89`, and prefer `Domain/identity.md#ContentHash`.
9. `RASM-PY-RUNTIME-BRIEF.md:45` / `DATA:46` / `GEOMETRY:47` / `COMPUTE:46` — "read-only acceptance pass … rejects any back-edge": contradicts the fail-open fix-in-place close; teaches agents acceptance cannot edit. FIX: "the fail-open acceptance agent re-derives the graph and fixes back-edges in place."
10. Residual-forward-transfer clause (×8: RT:206, DT:221, GE:204, CO:204, PY-ART-B:270, GEO-B:221, + FAB/GEO-D echoes) — "residuals naming a later leg's pages transfer forward explicitly in the leg return." No residual channel exists; invites agents to defer instead of repairing in-pass. FIX: delete the clause everywhere; the adjacent in-pass-repair sentence stands alone.

Honorable mentions (fix in the same sweep): PERS-D:324 "19 own bands" (real: 21); PY-ART-D:519-521 acceptance "renders for visual review" (category error — planning pages render nothing; acceptance = trace resolution over fences); FAB-B:5 two disk-refuted upstream `[V12]` bindings; DT:66 vs GE:67 laspy tag reversal; PY-ART-D:5 all-count headline.
