# [DOSSIER_CENSUS] libs/python/compute/.planning

Lane: census (read-only). Scope: 26 design pages / 6 sub-folders / ~9187 LOC markdown + ARCHITECTURE/README/IDEAS/TASKLOG + 35 `.api` catalogs. Upstream briefs read FULL: RASM-RUNTIME-BRIEF, RASM-DATA-BRIEF, RASM-GEOMETRY-BRIEF. `compute` is the LAST py folder (runtime→data→geometry→compute), UNBRIEFED — this census is its pre-brief survey.

Headline: the corpus is **page-level world-class and campaign-level broken** — 26 pages at 8.5–9.5 internal craft, but the graduation spine is inverted against the finalized geometry brief, a TYPE_CHECKING import cycle hides in solvers, the dispatch rail is three-way bifurcated, every governance surface is blind to the internal graph and to five upstream ripples, and the two declared data seams have zero composing fence. No per-page cold pass fixes it. Graduation + graph + rail first.

---

## [01]-[PER_PAGE_CENSUS]

Verdict = internal craft 1-10. `⚠` = campaign-level structural issue overriding the craft score. Every page carries a `[03/04]-[RESEARCH]` tail (BANNED folder-wide by runtime[V11]/data[V11]/geometry[V10]) and spells `rasm.runtime.content_identity` where imported (runtime[V4] renames → `rasm.runtime.identity`) — stated once here, not repeated per row.

### solvers/ (8 pages)

- **receipt.md** (146) — **9**. The one `@tagged_union SolverReceipt` over 4 methods; `_SLOTS`/`_STATUS`/`_TOL` tables; total `match self`+`assert_never`; `SolveStatus` StrEnum-with-`converged`. Exemplary shared-receipt owner. Defects: `FrozenDict` rail (receipt.md:28,65,71,82); RESEARCH tail :179. Charter: unchanged — the termination-vocabulary + slot-projection owner every solver folds into.
- **quadrature.md** (477) — **9 ⚠**. Quad/interp/FEM routes; one `_QUAD`/`_INTERP` catalog per family; `QuadEngine.gated()` x64 contract; honest residual floors. **CYCLE**: quadrature.md:57-58 imports `AssembledSystem` from mesh under `if TYPE_CHECKING` — a guarded cycle-dodge (data-E2's named defect) since mesh.md:48 imports `ElementKind`/`FemForm` from quadrature at RUNTIME. `FrozenDict` rail. Charter: quad/interp/FEM-condense owner — but `ElementKind`/`FemForm` must re-home OFF this page (see V2).
- **mesh.md** (313) — **9 ⚠**. `MeshField`/`MeshExchange` assemble/read/write; meshio physical-group round-trip; `@railed` content-key fold. Runtime leg of the cycle (mesh.md:48 → quadrature). Owns `_CTOR`+`AssembledSystem`; content_identity :50; `FrozenDict`. Charter: mesh-and-field interchange + weak-form assembly.
- **linear.md** (473) — **9**. Dense/sparse/eigen + lineax operator tier; `_ISTOP`/`_TAG_NAMES`; `_sparse_receipt` reused by quadrature FEM. `FrozenDict`. Charter unchanged.
- **nonlinear.md** (380) — **8.5**. root/minimise/fixed-point/least-squares over optimistix; numpy central-diff floor. nonlinear.md:392 `raise ValueError(bracket)` — FENCED (converts on `solve` boundary), acceptable. `FrozenDict`. Charter unchanged.
- **differential.md** (422) — **9**. ODE/SDE/CDE over diffrax; `_LEVY`/`_SOLVER`/`_ADJOINT`/`_EVENT` deferred-resolver tables; adjoint modes. `FrozenDict`. Charter unchanged.
- **sensitivity.md** (421) — **9**. AD algebra (grad/jac/hess/jvp/vjp) + implicit-adjoint + findiff floor; `_PRODUCT`/`_SPEC`. `FrozenDict`. Charter unchanged. (findiff `.api` consumed here — verify vs README roster.)
- **field.md** (285) — **9**. FE/grid readout; imports mesh(`_CTOR`,`MeshField`)+quadrature(`ElementKind`)+receipt; interpax x64 resample; `basis.project` residual. content_identity :42; `FrozenDict`. Confirms the element-axis split across 3 files.

### optimization/ (3 pages)

- **design.md** (464) — **9**. Gradient inverse-design apex; `@functools.cache`-built `_objective()` shape table; Field/Mesh/param cases; reads implicit-adjoint gradient. Owns shared `OutcomeReceipt`. content_identity+IdentityPolicy :42; `FrozenDict`. Charter unchanged.
- **program.md** (308) — **8.5**. LP/MILP/global/assignment over scipy.optimize; imports design.`OutcomeReceipt`+receipt.`SolveStatus`. content_identity+IdentityPolicy :40; `FrozenDict`. Charter unchanged.
- **convex.md** (396) — **9.5**. DCP cone programs; distinct `ConvexReceipt` (KKT dual-certificate) — split from `OutcomeReceipt` is DELIBERATE + justified (fundamentally different evidence). `_CONE_ROWS`/`_CONE_KKT`/`_SENSE`; `ParamBind` DPP sweep. content_identity :37; `FrozenDict` (8 tables). Charter unchanged.

### experiments/ (4 pages)

- **study.md** (415) — **9 ⚠**. SALib 8-analyzer→one `SALIB_ROUTES` collapse; union-owned design/discrepancy/indices; scipy-faithful `AxisDist`; honest `Measured` speedup. **DATA SEAM UNWIRED**: generates design via `method.design(axes,seed)`, admits no `data/tabular` frame; no `rasm.data` import despite ARCH `study ← data/tabular`. `expression.Map` rail; inline `_TRACER` :357 + span :450. Charter unchanged.
- **history.md** (279) — **8.5**. Content-key-keyed run persistence/resume/compare; imports study. history.md:236 `raise KeyError(missing)` — FENCED. content_identity+IdentityPolicy; `Map`; inline span :315. Charter unchanged.
- **inference.md** (364) — **9.5**. Bayesian; `Distribution` read in prior+likelihood roles w/ GLM inverse-links; `canonical` encoder-native identity payload; `_RESIDUALS` dimension table; arviz-1.x field discipline (elpd/p/pareto_k). GRADUATES `uncertainty_law` :302. Consumes `xarray.DataTree` :45 (data co-consume). content_identity :40; inline span :337. Charter unchanged.
- **model.md** (368) — **9.5**. ONNX export/validate; `ExportSource`/`ValidationCheck`/`ValidationEvidence`; `PROBE_RANK`; io-name set-equality. GRADUATES `model_asset` :289. **UNDECLARED RUNTIME SEAM**: model.md:42 `from rasm.runtime.roots import ResourceRef` + `upath` :36 — absent from ARCH ledger AND README substrate. content_identity :39; inline span. Charter unchanged.

### numerics/ (5 pages)

- **array.md** (255) — **9.5 ⚠**. Two-axis `ArraySource`×`AdmitMode`; array-api-compat/extra stacking; lazy/eager fork; `FiniteGate` masked reduction; railed identity; `DenseBound`. **SEAM MISFILE**: ARCH `array ⇄ runtime/transport` — `ContentIdentity` lives in runtime `evidence/identity`, not `transport`; `⇄` wrong (array only consumes). **DATA SEAM PROSE-ONLY**: admits dask duck-typed (correct, no import) but no fence takes an `xarray` Dataset/DataArray. No `graduates()` despite handoff.md:205 `array_layout` claim. content_identity :38. Charter: keep; add explicit data-shape admission arm OR drop the prose claim.
- **interval.md** (352) — **9**. Arb-ball/mpmath/numpy outward-rounding ladder; `_FLOOR_LADDER`. content_identity+CANONICAL_POLICY :33; interval.md:398 `raise RuntimeError(fault)` FENCED; `FrozenDict`; inline span :421. Charter unchanged.
- **jit.md** (231) — **8.5**. numba LLVM + jax XLA on one route table; `_JIT_ROUTES`. content_identity :30; `FrozenDict` (3). Charter unchanged.
- **quantity.md** (447) — **9.5**. pint.Measurement + uncertainties; `Magnitude.reseat/join` fold collapse; `Umath` SmartEnum-with-arity; fenced `consistent` verdict (not tautological). GRADUATES `unit_law` :305. content_identity :51; quantity.md:468 `raise` FENCED. Charter unchanged.
- **statistics.md** (289) — **9**. scipy.stats hypothesis + MLE fit → one `StatReport`; `_STAT_ROUTES`/`_SIGNIFICANCE_CALLS`. content_identity+CANONICAL_POLICY :34; `FrozenDict`; inline span :228. Charter unchanged.

### analysis/ (4 pages)

- **signal.md** (308) — **9**. DSP + pywt multiresolution on one `Signal`; imports transform(`SpectralReadout`)+array. `Map`; content_identity :36. Charter unchanged.
- **transform.md** (290) — **9**. scipy.fft DFT/DCT/DST + Hankel + Hilbert; imports array. `Map`; content_identity :32. Charter unchanged.
- **symbolic.md** (697) — **9 ⚠**. Biggest page; `Block[SymbolicOp]` fold over `ExprForm`; sympy+python-flint; lambdify/codegen. `symbolic` HandoffAxis case DEAD (no `graduates()` call, no graduation import — handoff.md:202 claims it produces). symbolic.md:738/779/786/824 `raise` all FENCED. content_identity+IdentityPolicy :60; `Map`; inline span :?. Charter unchanged.
- **spatial.md** (316) — **9 ⚠**. scipy.spatial comp-geometry; `SpatialQuery`/`SpatialEvidence`; `NEIGHBOUR_FLOOR`; alpha-shape kernel; points-vs-cardinality split. **GEOMETRY SEAM BACKWARDS**: spatial.md:14 graduates `reconstructed-mesh`/`geometry`-case evidence OUTWARD to geometry scan (ARCH `spatial → geometry/scan`), but geometry is UPSTREAM (built first) and geometry[V2] mints `reconstructed-mesh` itself — compute can't feed already-built geometry, and both now claim to produce it. No `graduates()` import despite the prose. `Map`; content_identity :30. Charter: producer of point-set evidence; the geometry crossing must invert or die.

### graduation/ (2 pages)

- **handoff.md** (208) — **9 internal / 5 campaign ⚠⚠**. Internal machinery world-class: two-stage `boundary(_admit).bind(_clear).bind(boundary(_emit))` rail; finiteness refinements; one `_subject` fold; residual-over-ceiling collapse. But the DOMAIN CONTRACT is INVERTED vs geometry[V2]: **owns `GeometrySubject`** (handoff.md:61-64) which geometry now mints; **claims compute is the PRODUCER** geometry consumes (handoff.md:21,206) — geometry is the producer, compute the decode-hub; **stale literal set** (`registration-transform`/`reconstructed-mesh`/`topology-graph`/`network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra`/`scan-deviation`) missing geometry's differentiated `bim-compliance`/`bim-lifecycle`/`section-property`/`building-energy`/`thermal-comfort`; **5 of 8 axes dead** (only model_asset/unit_law/uncertainty_law have producers). content_identity :56; inline span :113. Charter: REBUILD as the multi-domain graduation HUB — decode geometry's minted `GeometryHandoff` carrier at the seam, own the compute-native axes, re-graduate to C# `Rasm.Compute`; own the shared evidence-weave + scope table (geometry[V5] analogue).
- **codegen.md** (314) — **9.5 ⚠**. Exceptional: `_fold(node,alg)` catamorphism run by 3 `FieldAlgebra` interpreters (`_NODE`/`_TYPES`/`_REFS`); divergence-free `ast.unparse` identity; topological `_ordered` w/ gray-set cycle guard; data-driven import preamble. Correctly realizes `codegen ← csharp:Rasm.Compute EvidenceBundle` decode seam. **HARDCODED STALE STRING**: codegen.md:188 `_BARE = frozenset({"builtins", "rasm.runtime.content_identity"})` breaks under the rename (ContentKey renders dotted, not bare). Plain `dict` rail (`_SCALAR`,`_DECODER`) — third rail form. inline span :258. Charter: keep; owns its own graduation-bundle wire shapes (offline json/msgpack, distinct from runtime gRPC `shapes`) — a compute brief should rule whether that shape belongs in a compute shapes owner.

---

## [02]-[SEAM_GRAPH_DIFF]

### Declared (ARCHITECTURE.md [02], 9 rows) vs real fence graph

```
ARCH-declared                                    reality
* ⇄ csharp:Rasm.Compute (HandoffAxis grad)       WIRED (handoff graduates outward) but ⇄/wildcard suspect — compute is the hub, direction is compute→C# only (→)
graduation/codegen ← csharp (EvidenceBundle)     WIRED — codegen.md decodes EvidenceBundle. CORRECT.
solvers/receipt → csharp (SolverReceipt verdict)  WIRED (SolverReceipt.contribute); the C# gate reads it. CORRECT.
graduation ← python:geometry/ifc (IDS/clash/BCF) DECLARED-UNWIRED — no fence decodes geometry ifc evidence; inverted vs geometry[V2] (geometry PRODUCES → compute DECODES, but no decode fence exists)
graduation/handoff ⇄ python:geometry/graph        MISTYPED ⇄ — geometry[V11] kills the ⇄ (no compute→geometry import; no geometry→compute import — crossing is wire-only). handoff has no geometry decode fence.
numerics/array ⇄ python:runtime/transport         MISFILED endpoint (ContentIdentity = runtime evidence/identity, NOT transport) + wrong ⇄ (array consumes only, →). WIRED to the wrong-named owner.
analysis/spatial → python:geometry/scan            BACKWARDS — compute (downstream) cannot feed geometry (upstream, built first); double-producer of reconstructed-mesh. Likely DEAD/invert.
experiments/study ← python:data/tabular            DECLARED-UNWIRED — study admits no data frame; no rasm.data import.
numerics/array ← python:data/tabular               DECLARED-UNWIRED (xarray path) — array admits dask duck-typed only; no xarray-Dataset admission fence.
```

### Wired-undeclared (real edges the ledger is blind to)

- **ALL ~24 intra-compute edges** (ARCH [02] declares ZERO). Union-find clusters: solvers `{receipt ← linear,nonlinear,differential,quadrature,mesh,field}`, `{quadrature ⇄ mesh}` (CYCLE), `{field → mesh,quadrature,receipt}`; optimization `{convex,design → receipt}`, `{program → design,receipt}`; experiments `{history → study}`, `{inference,model → graduation/handoff}`; numerics `{quantity → graduation/handoff}`; analysis `{signal → transform,array}`, `{spatial,transform → array}`.
- **model → runtime/roots** (`ResourceRef`, model.md:42) — undeclared runtime seam.
- **inference → xarray.DataTree** (:45), **model → upath.UPath** (:36) — undeclared substrate consumes (not in README [03]).

### Page-level import CYCLES

1. **`solvers/quadrature ⇄ solvers/mesh`** — mesh.md:48 `import ElementKind,FemForm` (RUNTIME) ↔ quadrature.md:58 `import AssembledSystem` (TYPE_CHECKING dodge). Runtime graph is technically acyclic ONLY because of the guard; the element-axis vocabulary is one concern fractured across two files. This is data-E2's exact anti-pattern.

### csharp seams the pages carry

- `graduation/codegen ← Rasm.Compute` **EvidenceBundle** (schema_version/owners[OwnerDescriptor]/bundle_key; `FieldScalar` i32/i64/f64/bool/string/key/bytes/decimal; `FieldNode` union) — decode-only, no C# interior import. SOUND.
- `graduation/handoff → Rasm.Compute` graduation-evidence wire — `GraduationReceipt` names the wire axis, never a C# owner row (handoff.md:9,31). SOUND stance, but the geometry-axis half is inverted (above).
- `solvers/receipt → Rasm.Compute` — `SolverReceipt` termination evidence the C# graduation gate reads (receipt.md:19). SOUND.
- FIT law observed corpus-wide: `ContentKey.hex` `{value:032x}:{fmt}` the C# `InterchangeIdentity.Key`/`RepresentationContentHash` reads; count-prefixed canonical-bytes (RASM-COMPONENT-PARADIGM-DECISION [AMENDMENTS]) consumed TRANSITIVELY through runtime's writer — compute authors no canonical-writer mirror. SOUND.

---

## [03]-[CROSS_CUTTING]

- **Dispatch-rail bifurcation (3-way, folder-correlated).** `expression.Map` in analysis/+experiments/ (6 pages: signal/spatial/symbolic/transform/history/study `Map.of_seq`); `beartype.FrozenDict` in solvers/+numerics/+optimization/ (13 pages, e.g. convex 8 tables, field 7, receipt 4); plain `dict` in codegen (`_SCALAR`/`_DECODER`). Doctrine `rails-and-effects.md:17` names `Map[K,V]` THE immutable-lookup owner; data[V2] ruled `expression.Map` canonical and deleted all frozendict forms. `beartype.FrozenDict` is REAL (verified `.venv/.../beartype/__init__.py:92`) — a `hint_overrides` config helper, so this is a consistency/doctrine defect, not a broken import. `Redaction.classified: Map` (runtime-owned) forces `Map` on every page's `_REDACTION`, so pages already import both — the split is gratuitous.
- **Duplicated span mechanism.** Exactly 8 pages mint a page-local `_TRACER = trace.get_tracer("compute.<x>")` + inline `start_as_current_span`: history:315, inference:308/337, model, study:357/450, codegen:167/258, handoff:89/113, interval:421, statistics:228. Exact defect geometry[V5] collapsed onto its `graduation.md` evidence-weave owner + one scope-vocabulary table. Compute has no evidence-weave owner; each page re-opens the span. (The other producers route egress through `@receipted` only — no span — a second inconsistency: span-bearing vs span-free evidence emission.)
- **Concern mixing — element axis.** `ElementKind`/`FemForm` (quadrature) + `_CTOR`/`AssembledSystem` (mesh) + read by field: one FEM element concern split across 3 files, forcing the quadrature⇄mesh cycle-dodge.
- **Dead typed carriers.** `HandoffAxis` has 8 cases; 5 (`solver`,`symbolic`,`array_layout`,`geometry`,`convex_program`) have NO producing fence. handoff.md:25,200-206 prose claims solvers/receipt+design+program+convex+symbolic+array+spatial all feed the rail — none import `graduation.handoff`. `[ARRAY_LAYOUT_GRADUATION]-[QUEUED]` and `[DATA_STUDY_INPUT]-[QUEUED]` acknowledge two of the gaps, but the prose states them as realized.
- **Prose-vs-fence (illusory capability).** (a) handoff geometry-producer prose vs no decode/produce fence; (b) study/array "data study input" prose vs no `rasm.data` admission; (c) array `array_layout` graduation prose vs no `graduates()`; (d) spatial "graduates reconstructed-mesh to geometry" vs no graduation import.
- **Hardcoding-vs-generator.** Mostly clean (tables are seed data everywhere). Two literals: codegen.md:188 `"rasm.runtime.content_identity"` module string (rename-fragile); the `content_identity` import spelling at 20 sites (mechanical, runtime[V4]).
- **Unmined upstream capability (see [04]).** No compute page composes runtime `lanes.offload` (interpreter/process/thread) — every worker-band solve (`QuadEngine.gated`, `SolveEngine`, skfem assemble, pymc sample, ONNX session) runs inline on the event loop; runtime[V5] pins the offload axis + `WORKER_BAND` as consumer exports. No page composes runtime `guarded`/`stamina` retry over flaky oracles (olca-ipc/onnxruntime/external NUTS). No page reads the reproduction `ParityReceipt`/`CorpusFixture` corpus (array_layout bit-identity is exactly its use — TASKLOG QUEUED).
- **Roster/README drift.** README [02] omits 3 consumed domain packages: `interpax` (quadrature/field), `quadax` (quadrature), `findiff` (sensitivity.md:477 `from findiff import coefficients` — LIVE, not dead). README [03] SUBSTRATE lists only `numpy` — omits consumed `xarray` (inference:45) + `universal-pathlib`/`upath` (model:36). `compute/.api/dask.md` must RELOCATE to branch tier (data-brief `DASK_CATALOG_REHOME` ripple). Central manifest `sparse` retag `[DATA]→[COMPUTE]` (data-brief ripple; compute is the live consumer).
- **Sound + preserve.** Every receipt owner's `_SLOTS`+`assert_never`+`Self`-factory discipline; the `@railed`/`@receipted`/`boundary` weave; `ContentIdentity.of` single-mint (no re-mint anywhere); the raise-inside-fence idiom (all raises FENCED); convex's justified distinct receipt; codegen's catamorphism; inference's arviz-1.x fidelity; quantity's fenced consistency verdict.

---

## [04]-[UPSTREAM_CONSUMPTION_LEDGER]

### Capabilities compute COULD compose instead of hand-rolling

- **runtime `execution/lanes.offload`** (isolation-modality: interpreter|process|thread; `WORKER_BAND`; `retry: RetryClass` kw) — runtime[V5] pins these as consumer exports (geometry named). Every compute worker-band solve should offload: jax x64 solves, skfem assemble, pymc/nutpie sampling, ONNX InferenceSession, SALib analyzers. TODAY: all inline. Named opportunity per solver/experiments page.
- **runtime `reliability/resilience.guarded`/`guarded_sync`** over the `POLICY` table — flaky external oracles: `olca-ipc` IPC (`_from_olca`-analogue is data, but compute has onnxruntime/external-NUTS), external NUTS engines, scipy solves that transiently fail. TODAY: none wrapped.
- **runtime `evidence/reproduction` `ParityReceipt`/`CorpusFixture`** — directly satisfies `[ARRAY_LAYOUT_GRADUATION]` (cross-backend bit-identity) and any cross-runtime graduation parity. runtime[V7] lands the corpus. TODAY: array.md hand-plans a comparison it never emits.
- **runtime `evidence/identity` `IdentitySource.lift` 3 modalities** (buffer/stream/merkle) — mesh.md already uses `stream`; array uses `whole`/buffer; a merkle key over multi-array payloads (StudyPayload, cohort) is available and unused.
- **runtime `transport/shapes`** (16 msgspec.Struct wire vocab + `FaultDetail` + `_PROTO_VOCABULARY`) — if compute ever serves over the gRPC wire (it does not today; graduation is offline). codegen's `EvidenceBundle` is a SEPARATE offline wire — correctly not registered in `shapes`. Named as a boundary a compute brief should rule.
- **data frames as study/array inputs** — data[V12] keeps `xarray` branch-tier BECAUSE compute co-consumes `DataTree`; `experiments/study ← data/tabular` + `numerics/array ← data/tabular` are the declared consume points. Realize the admission arms (study: accept a contract-gated DOE frame; array: admit an xarray Dataset extracting `.data`+coords into `NamedAxis`).

### Assumptions an upstream campaign INVALIDATES (migration pressure)

- **runtime[V4] identity rename** `content_identity → identity` — 20 import sites + codegen.md:188 literal. Mechanical, corpus-wide.
- **geometry[V2] graduation inversion** — geometry MINTS `GeometrySubject`+`GeometryHandoff`; compute's handoff.md OWNING `GeometrySubject` + claiming producer role is now WRONG. Compute must DECODE geometry's carrier. The differentiated union (bim-compliance/bim-lifecycle/section-property/building-energy/thermal-comfort) replaces compute's stale literals.
- **geometry[V11] ⇄ kill** — ARCH `graduation/handoff ⇄ geometry/graph` and the wildcard `* ⇄ csharp graduation` on the geometry side die; compute's ledger must flip its geometry rows to `←` (decode-only) and keep only the compute→C# graduation `→`.
- **geometry[V2/V11] `analysis/spatial → geometry/scan`** — geometry imports only runtime/data/itself; compute (downstream) cannot be consumed by upstream geometry. This seam is dead-as-import; if the alpha-shape boundary genuinely crosses, it is a wire/graduation artifact geometry consumes at RUN time, not build time — the ARCH row + spatial.md:14 prose must be reconceived or struck.
- **data[V12] `DASK_CATALOG_REHOME`** — `compute/.api/dask.md` (folder-tier, owner:compute) is superseded by branch-tier `libs/python/.api/dask.md`; the removal is an explicit ripple to the compute campaign.
- **data[V12] `sparse` retag** `[DATA]→[COMPUTE]` and `netcdf4` `[COMPUTE]→[DATA]` — central manifest tag truths compute inherits (sparse: compute is the live consumer; netcdf4: no compute consumer).
- **data brief restructures the seams `[DATA_STUDY_INPUT]` cites** — that card names data-side `[CONTRACT_GATE_FOLD]`/`[CUBED_LINALG_DEEPEN]`; the data rebuild re-homes `tabular/contract` (stays) and adds `tabular/materialize`/`gridded/store` settlements — the compute card's seam names + glyphs need re-alignment to the post-rebuild data surface.

### WATERFALL-RIPPLE EDITS APPLIED (this census → upstream briefs)

None required. Every compute demand is satisfiable by capabilities the finalized upstream briefs ALREADY pin as consumer exports: runtime[V5] `offload`/`WORKER_BAND`/`guarded`, runtime[V7] `ParityReceipt`/`IdentitySource.lift`, runtime[V4] `identity`, data[V12] `xarray` branch-tier + `sparse` retag + `dask` rehome, geometry[V2] `GeometryHandoff` carrier. The graduation-decode contract compute needs is exactly what geometry[V2] froze ("compute's `HandoffAxis` hub and `GraduationReceipt` admission fold DECODE this carrier"). No upstream owner extension is demanded by this folder.

---

## [05]-[VERDICT_CANDIDATES]

**VC1 — GRADUATION-SPINE REBUILD (campaign-defining).** handoff.md is internally 9/10 but its domain contract is inverted against finalized geometry[V2]: it OWNS `GeometrySubject` (handoff.md:61) geometry now mints; claims compute PRODUCES geometry subjects (handoff.md:21,206) when geometry produces and compute decodes; carries a stale literal set; and 5/8 `HandoffAxis` cases have no producer. Rebuild compute as the multi-domain graduation HUB: (a) DECODE geometry's minted `GeometryHandoff` carrier at the wire (new decode fence, no `rasm.geometry` import); (b) own the compute-native axes (`solver`/`convex_program`/`symbolic`/`array_layout`/`model_asset`/`unit_law`/`uncertainty_law`) and WIRE the 5 dead producers (solvers/receipt+design+program → `solver`; convex → `convex_program`; symbolic → `symbolic`; array → `array_layout`); (c) re-graduate outward to C# `Rasm.Compute`. Evidence: handoff.md:61,21,206,200-206; geometry-brief [V2]/[V11]; only 3 `graduation.handoff` importers (inference/model/quantity).

**VC2 — ELEMENT-AXIS RE-HOME (kill the cycle).** `solvers/quadrature ⇄ solvers/mesh` is a TYPE_CHECKING cycle-dodge (quadrature.md:58 guards the `AssembledSystem` back-edge; mesh.md:48 imports quadrature at runtime) rooted in the FEM element vocabulary fractured across two files. Re-home `ElementKind`/`FemForm`/`_CTOR`/`AssembledSystem` into ONE base element-axis owner both import strictly downward (candidate: a new `solvers/element.md` leaf, or consolidate onto `mesh` with `AssembledSystem` staying there and quadrature importing mesh downward only). Evidence: quadrature.md:57-58, mesh.md:48, field.md:39-41; data-brief[V1] re-homing precedent.

**VC3 — DISPATCH-RAIL UNIFICATION.** Three rails for one "immutable keyed table" concern: `expression.Map` (analysis/experiments, 6 pages), `beartype.FrozenDict` (solvers/numerics/optimization, 13 pages), plain `dict` (codegen). Ratify `expression.Map` corpus-wide per doctrine `rails-and-effects.md:17` + data-brief[V2] precedent; every `FrozenDict[...]`/`Final[dict[...]]` dispatch table → `Final[Map[...]]` `Map.of_seq`. `beartype.FrozenDict` is real but off-label (a `hint_overrides` helper). Evidence: per-page rail map — convex.md 8 FrozenDict tables, study.md `Map.of_seq`, codegen.md `_SCALAR: Final[dict]`.

**VC4 — RESEARCH-PURGE + ONE-EVIDENCE-WEAVE.** All 26 pages carry banned `[RESEARCH]` tails (runtime/data/geometry all purged these). ~10 pages inline a page-local `_TRACER`+`start_as_current_span`. Purge every tail (fold load-bearing member confirmations into `Packages` blocks / `.api`); mint a compute evidence-weave owner (graduation-sited, geometry[V5] analogue) composing `boundary`+span+`@receipted` with a one-table scope vocabulary, and route every producer's span through it. Evidence: RESEARCH headers at 26 pages; inline `_TRACER` study:357/inference:308/model/codegen:167/handoff:89/history:315/interval:421/statistics:228/symbolic.

**VC5 — GOVERNANCE LEDGER TRUTH.** ARCH [02] declares ZERO intra-compute edges (blind to ~24 edges + the quadrature⇄mesh cycle), misfiles `array → runtime` on `transport` (owner is `evidence/identity`), mistypes 3 geometry/csharp rows `⇄`, and omits `model → runtime/roots` + the xarray/upath substrate consumes. Rebuild the ledger to see the internal DAG + correct every cross-libs endpoint/direction (mirror runtime[V4]/data[V12]/geometry[V11]). Evidence: ARCHITECTURE.md:43-55; the 24-edge graph in [02] above.

**VC6 — DATA-SEAM REALIZATION-OR-HONESTY.** Both `experiments/study ← data/tabular` and `numerics/array ← data/tabular` are declared-unwired (study generates its own design; array admits only backend arrays, no xarray-Dataset arm). `[DATA_STUDY_INPUT]-[QUEUED]` acknowledges the gap but the page prose states it realized. Either land the admission arms (study: accept a contract-gated DOE frame + labelled array; array: admit `xarray.Dataset`/`DataArray` extracting `.data`+coords→`NamedAxis`) or strike the prose + mark the seam BLOCKED; re-align the card to the post-data-brief seam names. Evidence: study.md (no `rasm.data` import), array.md:5,13 (prose), TASKLOG `[DATA_STUDY_INPUT]`.

**VC7 — CONTENT_IDENTITY RENAME.** `rasm.runtime.content_identity → rasm.runtime.identity` at 20 import sites + the hardcoded module string codegen.md:188 (which silently breaks bare-vs-dotted rendering of `ContentKey`). Mechanical corpus-wide sweep, runtime[V4]. Evidence: 20 import lines; codegen.md:188 `_BARE`.

**VC8 — PACKAGE ROSTER + `.api` TRUTH.** README [02] omits 3 consumed domain packages `interpax`/`quadax`/`findiff` (all LIVE, all with `.api`); README [03] omits consumed substrate `xarray`/`universal-pathlib`; `compute/.api/dask.md` must relocate to branch tier (data `DASK_CATALOG_REHOME` ripple); central-manifest `sparse` retag `[DATA]→[COMPUTE]`, `netcdf4` `[COMPUTE]→[DATA]`. Evidence: quadrature/field interpax+quadax imports, sensitivity.md:477 findiff; inference `xarray.DataTree`:45; model `upath`:36; data-brief[V12].

**VC9 (secondary) — SPATIAL/GEOMETRY SEAM DISPOSITION.** `analysis/spatial → python:geometry/scan` is architecturally backwards (compute downstream cannot feed upstream-built geometry; both claim `reconstructed-mesh`). Rule: strike the import-seam, keep spatial's alpha-shape as compute-native evidence that graduates to C# only, and let geometry consume any boundary at the wire at RUN time (never a build seam). Evidence: spatial.md:3,14; geometry-brief[V2] internal-DAG law (runtime/data/itself only).
