# DOSSIER — corpus-a — libs/python/compute/.planning (FIRST-HALF subfolders: analysis, experiments, graduation) + root governance

Scope read in full: `analysis/{transform,signal,spatial,symbolic}.md`, `experiments/{study,model,inference,history}.md`, `graduation/{handoff,codegen}.md`; `compute/ARCHITECTURE.md`, `README.md`, `TASKLOG.md`, `IDEAS.md`. FIT authorities read in full: `libs/.planning/architecture.md`, `RASM-RUNTIME-BRIEF.md`, `RASM-DATA-BRIEF.md`, `RASM-GEOMETRY-BRIEF.md`, `RASM-COMPONENT-PARADIGM-DECISION.md` `[AMENDMENTS]`.

Corpus verdict (headline): PAGE-LEVEL EXCELLENT, SEAM-LEVEL BROKEN AT THE GRADUATION HUB, and carries the SAME systemic prose/governance rot the three upstream briefs each ruled fatal (RESEARCH appendix folder-wide, an identity-import rename that breaks the instant runtime lands, an unstated import-DAG). Craft floor across the ten pages is ~9/10: universal tagged-union evidence collapse (`TransformEvidence`/`SignalEvidence`/`SpatialEvidence`/`Outcome`/`ValidationEvidence`/`PosteriorSummary`), callable route tables (`FOURIER_ROUTES`/`WAVELET_ROUTES`/`SALIB_ROUTES`/`_CODE_PRINTER`/`NEIGHBOUR_FLOOR`/`_KERNELS`/`_RESIDUALS`), one-owner dispatch, `@cache` deferred imports, numpy-floor discipline, clean runtime FIT (faults/receipts/identity/roots consumed by symbol, nothing re-minted). The rot is NOT per-page shallowness; it is (1) the graduation hub sitting on the WRONG SIDE of the now-finalized geometry `[V2]` inversion, (2) a content-identity-policy rail bifurcation that is a latent cross-page correctness defect, (3) the corpus-wide grammar/rename/governance debt the upstream campaigns already purged. compute is the LAST py folder and unbriefed; every upstream ruling it consumes has landed as finalized law and it has drifted from all three.

---

## [1] PER-PAGE

### graduation/handoff.md — VERDICT 6/10 (superb craft, fundamentally mis-sided seam)
Owner: the Python-branch graduation admission rail. `HandoffAxis` `@tagged_union` (solver/symbolic/model_asset/array_layout/unit_law/uncertainty_law/geometry/convex_program); `GraduationReceipt.graduates` two-stage `boundary(_admit).bind(_clear).bind(boundary(_emit))` rail; `_subject` one-or-pattern fold. Craft is 9.5 — the two-stage clearance fence vs the siblings' single-fence `_emit(_fit(...))`, the finiteness `Is` refinement, the negated-floor admission, the four-clause collapse onto one `_clear` are all exemplary.
Defects:
- FIT DIRECTION INVERSION (fatal): geometry `[V2]` (finalized) mints `GeometrySubject` IN GEOMETRY now and rules "compute's `HandoffAxis` hub and `GraduationReceipt` admission fold DECODE this carrier at the seam and re-shape nothing without a geometry ripple." handoff.md:61-64 OWNS `GeometrySubject` (8 literals) — must become geometry-owned + decoded. handoff.md:21 "`scan-deviation` is the PRODUCER literal compute emits ... the geometry `scan/deviation.md#DEVIATION` owner consumes" is BACKWARDS: geometry's `scan/deviation` COMPUTES deviation and PRODUCES it; compute is the hub that decodes it. handoff.md:206 systematically frames geometry owners (`graph/algebra`, `graph/features`, `graph/nonmanifold`, `scan/*`) as CONSUMERS of "the literals ... their geometry-branch owners consume" — inverted; those owners PRODUCE. The compute `ARCHITECTURE.md:49` seam `graduation ← python:geometry/ifc` (receive) and geometry-brief `[V2]`/`[06]` are the truth.
- STALE SUBJECT UNION: post-`[V2]` the geometry subject union DIFFERENTIATES — `numerical-primitive` stays, and `bim-compliance`/`bim-lifecycle`/`section-property` (`[V2]`) plus `building-energy`/`thermal-comfort` (`[V1]` energy plane) JOIN. handoff.md:62-64's 8-member literal lacks all five and cannot decode a geometry `GeometryHandoff` carrying them.
- content_identity import (handoff.md:56) — runtime `[V4]` rename.
- `[04]-[RESEARCH]` appendix (handoff.md:208) — banned; index/body numbering drift (index `[03]-[RESEARCH]` line 15 vs body `[04]` line 208; index `[01]` maps to body `[02]`).
Charter as it SHOULD be: the ONE Python-branch graduation HUB. Two structurally-distinct roles on one owner: (a) EGRESS of compute-OWN evidence (solver/symbolic/model_asset/array_layout/unit_law/uncertainty_law/convex_program) outward to C#; (b) DECODE of geometry-minted `GeometryHandoff` carriers (frozen contract inherited from geometry `graduation.md`) re-emitted on the unified wire — `HandoffAxis.geometry` carries the DIFFERENTIATED geometry-minted subject vocabulary as decoded data, not compute-minted. The lone genuine compute→geometry PROJECTION (`analysis/spatial` reconstructed-mesh alpha-shape boundary → geometry/scan, `ARCHITECTURE.md:52`) is the exception preserved as an outbound emit on the geometry case.

### graduation/codegen.md — VERDICT 8/10
Owner: `StubCodegen` — decodes the C# graduation-evidence `EvidenceBundle`, folds one `_fold(node, alg)` catamorphism run by THREE `FieldAlgebra` interpreters (`_NODE` ast.expr, `_TYPES` scalar types, `_REFS` nested edges), emits msgspec stubs + JSON Schema. Craft is 9.5 — the "annotation IS `ast.unparse(_fold(field,_NODE))`" divergence-free identity, the deleted eager `_HINT` interpreter, `_ordered` DAG topo-sort with a `visiting` gray-set cycle guard are masterful.
Defects:
- EMPTY `[03]-[RESEARCH]` header (codegen.md:392, file ends at 393) — the runtime `[V11]` "two EMPTY headers" defect class exactly; index (line 7) lists only `[01]-[STUB_CODEGEN]` while body is `[02]`/`[03]` (numbering drift).
- content_identity: codegen.md:38 import + codegen.md:188 `_BARE` frozenset literal `"rasm.runtime.content_identity"` — the rename must sweep the _BARE data row too (it drives the emitted stub's import preamble, so a stale spelling ships in generated code).
- FIT SEAM (wire ownership): `EvidenceBundle`/`OwnerDescriptor`/`FieldNode` are declared LOCALLY and decoded raw via `msgspec.json/msgpack` (codegen.md:171-174) with NO proto binding, NO descriptor drift gate, NO runtime `transport/shapes` registry row — yet runtime `[V2]` established `shapes` as the single wire-vocabulary owner with a `_PROTO_VOCABULARY` binding + `descriptor_pool` drift gate for every C#-minted shape. `ARCHITECTURE.md:47` declares `graduation/codegen ← csharp:Rasm.Compute [WIRE]: EvidenceBundle graduation-evidence wire` (C#-minted). Decide: is `EvidenceBundle` a runtime-shapes registry row (C#-minted, drift-gated) or legitimately compute-local? A C#-minted wire decoded with no drift gate is the runtime-E2 phantom-vocabulary risk at compute scale.
Charter: correct as a decode-only projector; reconcile the wire vocabulary against runtime `shapes` ownership + count-prefix canonical-bytes law (transitive, decode-only).

### analysis/transform.md — VERDICT 9/10
Owner: `Transform` — the ONE frequency-domain owner (`TransformOp`: Fourier/Trigonometric/Analytic/Hankel over `scipy.fft`+`hilbert`). Owns `SpectralReadout` (PEAK/CENTROID/BANDWIDTH/FLATNESS, linear-amplitude contract). `FOURIER_ROUTES` `(forward,inverse,freqs)` triple table; `TransformEvidence` union with `facts()`. Array-API `xp` dispatch + numpy-floor analytic arm.
Defects: `[03]-[RESEARCH]` (344); content_identity (32); the linear-amplitude contract restated 4x (owner prose :3, fold docstring :80-87, RESEARCH :348, :350 — runtime `[V11]` "invariant once" defect); `FiniteGate.REJECT`-already-excludes-non-finite restated ~4x (:12,:234,:269,:348,:350).
Charter: correct and dense; the SpectralReadout owner rightly serves transform+signal.

### analysis/signal.md — VERDICT 9/10
Owner: `Signal` — stationary `scipy.signal` (filter/welch/spectrogram/peaks/resample) + `pywt` multiresolution (decompose/mra/scalogram/packet) on one owner. Reuses `SpectralReadout` from transform (signal.md:34 import). `WAVELET_ROUTES` `(forward,inverse,max_level)` triple; `ThresholdMode.shrink` enum-owned denoise callable; `SignalEvidence` union held whole on the receipt.
Defects: `[03]-[RESEARCH]` (369); content_identity (36); linear-amplitude/PSD-power square-root contract restated (:7,:16,:253,:371) — same invariant-once class.
Charter: correct; the stationary+multiresolution one-owner collapse is a strong instance of the density mandate.

### analysis/spatial.md — VERDICT 9/10
Owner: `SpatialQuery` over `scipy.spatial` (Neighbours/Radius/Pairs/Distances/Hull/Triangulate/Tessellate/AlphaShape/Align). `SpatialEvidence` union (Proximity/Complex/Boundary/Alignment); `NEIGHBOUR_FLOOR` degrading numpy table for the two KD-tree routes; `points`(identity buffer, concatenated)/`cardinality`(reference count) split; `_alpha_shape` local kernel with `combinations` facet enumeration; `_interior_point` Chebyshev-centre via `linprog`.
Defects: `[03]-[RESEARCH]` (401); content_identity (30).
FIT nuance (feeds V-A): spatial.md:14,206 — this is the ONE genuine compute→geometry projection (`reconstructed-mesh` alpha-shape boundary graduates on the `geometry` case to geometry/scan, matching `ARCHITECTURE.md:52`). Confirms the geometry axis is genuinely BIDIRECTIONAL after the `[V2]` inversion (compute-spatial outbound producer + geometry inbound decode), so the handoff reshape must preserve this outbound emit, not delete it.
Charter: correct; explicitly does not re-own geometry-branch mesh.

### analysis/symbolic.md — VERDICT 9/10 (largest page, 835 LOC; masterwork)
Owner: `SymbolicDerivation` — one `Block[SymbolicOp]` left-fold over an `ExprForm` to a typed `SymbolicReceipt`. `SymbolicOp` 10-row vocabulary (calculus/rewrite/substitute/refine + solve/linalg/number/evaluate/lower/codegen); `GroundDomain` FLINT accelerator axis lowering to `fmpq_poly`/`fmpq_mat`/`fmpz` with `_FLINT_MATRIX_ROUTES` subset gating; `Outcome` terminal union; `_CALCULUS`/`_CODE_PRINTER` tables; `Precision` HEURISTIC/CERTIFIED (`flint.good` arb-ball with `rad()` bound).
Defects:
- content_identity (60); `[04]-[RESEARCH]` (827).
- POLICY BIFURCATION (feeds V-B): symbolic.md:767 `ContentIdentity.of("symbolic", SymbolicPayload.of(...), IdentityPolicy())` — 3-arg fresh `IdentityPolicy()`, the exact form study.md:476 / inference.md:21 call "rejected by the runtime content owner." symbolic.md:833 asserts the `of(fmt, source, policy, *, view=)` contract.
- Lower dead-carrier (tracked): symbolic.md:438 `_lower(...)` MATERIALIZES a numpy/jax/native callable, discards it (returns `Outcome.Callable(backend, arity)` metadata only), "the live `fn` rides the cross-file lowering seam" — no fence consumes it. Tracked by IDEAS `SYMBOLIC_CODEGEN_BRIDGE` (QUEUED, numba.cfunc bridge), so acceptable-open, but the "cross-file lowering seam" is currently unwired.
- srepr/repr canonical-bytes: `_form_spelling` (symbolic.md:790-802) uses `repr()` on a `MatrixForm` tuple and `srepr()` on an Expr as the SymbolicPayload.form STRING. Deterministic + round-trippable (distinct from the data-brief-banned `repr(dict)`), and the canonical BYTES come from msgspec encoding the struct via `ContentIdentity`, so acceptable — but verify against the runtime canonical-bytes law (data `[V3]`: "no repr()/str()/hash-of-object byte source").
Charter: correct; the FLINT ground-domain row-level accelerator selection is the folder's densest single surface.

### experiments/study.md — VERDICT 9/10
Owner: `Study` — DOE sampling + SALib sensitivity + surrogate fitting on one owner; `StudyMethod` union OWNS `design`/`discrepancy`/`indices` folds (no detached free fns); `SALIB_ROUTES` `SalibRoute` table collapses 8 analyzers to one sampler + one analyzer body; `MeasurementMode`/`Measured` honest speedup discriminant (`Nothing` unless a real batch lane); `AxisDist` per-axis marginal vocabulary with exact `scipy.stats.ppf` rescale.
Defects:
- content_identity (35); `[03]-[RESEARCH]` (488); hardcoded `get_tracer("compute.study")` (357).
- POLICY BIFURCATION (feeds V-B): study.md:477 `ContentIdentity.of("study", design.tobytes())` — 2-arg, and study.md:475-476 EXPLICITLY: "the `CANONICAL_POLICY` default keys the canonical path — never a fresh `IdentityPolicy()` allocation the runtime content owner rejects." Directly contradicts symbolic/history/design/program/field/mesh.
- receipt drops content_key: study.md:422-430 `contribute` facts OMIT the design `content_key` (present only in `span_facts` :415 via `.hex`), while every analysis sibling spreads `content_key.project("hex")` into receipt facts — a study receipt is not reuse-keyable by a downstream ledger.
- DATA seam declared-unwired (expected): `ARCHITECTURE.md:53` `experiments/study ← python:data/tabular [SHAPE]: DOE dataset` and TASKLOG `DATA_STUDY_INPUT` (QUEUED) name a data/tabular study-input; study.md's fence takes a bare `Objective`+`axes`, not a data frame. Consistent with the card being QUEUED; note the seam is not yet in any fence.
Charter: correct; the benchmark concern rightly absorbed as a live `MeasurementMode` discriminant, not a parallel owner.

### experiments/model.md — VERDICT 9/10
Owner: `ModelAsset` — sklearn→ONNX export (`ExportSource` union) + validation (`ValidationCheck` input union → `ValidationEvidence` output union, the input/output-parameterized-as-two-shapes discipline) + graduation on `model_asset`. `PROBE_RANK` verb→output-index rank; `_load_and_run` one element-dtype-keyed feed construction; `ContentIdentity.of` checksum matched off the rail; consumes runtime `ResourceRef` (model.md:42) + `UPath`.
Defects: content_identity (39); hardcoded `get_tracer("compute.model")` (66). NOTE: model.md is the SOLE corpus-a page WITHOUT a `[RESEARCH]` appendix — the grammar exemplar.
FIT (clean): graduates compute-OWN `model_asset` evidence outward (model.md:287-293) — correct hub egress; imports `GraduationReceipt`/`HandoffAxis` from handoff (model.md:38).
Charter: correct and at the density bar; the ValidationCheck/ValidationEvidence split is a reference instance of input/output parameterization.

### experiments/inference.md — VERDICT 9/10
Owner: `Inference` — one Bayesian owner (prior/likelihood/posterior graph); `Distribution` union with `declare` (dual prior/likelihood role + GLM inverse-links) AND `canonical` encoder-native projection; `SamplerBackend` axis (pymc_native/external_nuts with sorted-pair options); `_RESIDUALS` dimension table folding `measured`/`ceiling`; `PosteriorSummary` per-variable row; arviz-1.x field fidelity (`r_hat`/`ci_bound`/`ELPDData.p`/`psense_summary`).
Defects: content_identity (40); `[03]-[RESEARCH]` (427); hardcoded `get_tracer("compute.inference")` (308).
- POLICY BIFURCATION (feeds V-B): inference.md:21 "resting on the `of` `CANONICAL_POLICY` default rather than a fresh `IdentityPolicy()` allocation ... the same default-policy threading `numerics/jit.md#JIT` holds" — the CANONICAL_POLICY camp, explicitly rejecting the other.
FIT (clean, NAMED cross-consumer): inference.md:45 `from xarray import DataTree` — the DATA brief `[V12]` PINS xarray branch-tier specifically because "compute co-consumes DataTree, ...compute/.planning/experiments/inference.md:45." Verified consumer seam. inference correctly bounds at PyMC's `nuts_sampler` dispatch, never re-drives blackjax (matches TASKLOG `BLACKJAX_RAW_ALGEBRA`-DROPPED). Graduates compute-OWN `uncertainty_law` outward (inference.md:302).
Charter: correct; the `canonical` encoder-native projection (avoiding the no-`enc_hook` `_ENCODER` rejecting a raw `@tagged_union`) is a strong FIT-aware identity design.

### experiments/history.md — VERDICT 8/10
Owner: `RunHistory` — multi-run persistence/resume/comparison on the study spine, distinct from the single-run study receipt. `ResumePlan` union (Complete/Partial/Fresh) with full-design-recompute on Partial (SALib indices undefined over a tail slice); `CrossStat` StrEnum OWNING its `_rank`/`_correlation`/`_tau` kernels + `_KERNELS` table (Spearman/footrule/Kendall/Pearson); `_traced[E: Traceable]` shared rail; `Map[ContentKey, np.ndarray]` response cache.
Defects:
- content_identity (35); `[03]-[RESEARCH]` (338); hardcoded `get_tracer("compute.history")` (217).
- POLICY BIFURCATION + LATENT CORRECTNESS DEFECT (feeds V-B, the smoking gun): history.md:293 `ContentIdentity.of("study", design.tobytes(), IdentityPolicy())` — the SAME fmt+source study.md:477 keys WITHOUT the policy. The resume cache (keyed by the design `ContentKey`) hits only if history's key EQUALS study's key. history.md:341 ASSERTS "the explicit `IdentityPolicy()` argument here keys identically to the study owner's design key at a fixed format and policy"; study.md:476 asserts the opposite ("`IdentityPolicy()` ... the runtime content owner rejects"). If `IdentityPolicy() != CANONICAL_POLICY`, EVERY resume misses the cache and silently falls to `Fresh` — a real latent defect masked by an unproven assumption.
Charter: correct; composes study's `graded`/`StudyMethod` folds without re-declaring design algebra — but the resume-key correctness is contingent on V-B's resolution.

---

## [2] CROSS-CUTTING

### Duplication / repeated mechanism
- `_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())` declared identically on all 6 self-emitting pages (handoff:88, codegen:77, study:355, model:67, inference:309, history:218) — candidate one shared `_NO_REDACTION` constant; each is a per-owner empty policy (defensible, but 6 identical literals).
- Linear-amplitude `SpectralReadout` contract restated 4x within transform.md and mirrored in signal.md — invariant-once (runtime `[V11]`) at prose altitude.
- The span-then-fence-then-receipt weave prose ("the `Ok` arm sets Status(OK); the Error arm needs no body because `_convert` already recorded") is hand-restated in every self-emitting owner (study:15, model:17, inference:19, history:15, handoff:7, codegen:14). The MECHANISM is correctly shared (runtime aspects), but the PROSE is duplicated ~6x; a single owner-block statement per page suffices.

### Concern mixing / owner boundaries — none egregious
Pages are cleanly one-owner. `SpectralReadout` (transform-owned, signal-consumed) is a correct 2-consumer shared axis. The only cross-owner tension is the graduation hub conflating (a) compute-own egress and (b) geometry-inbound decode under one undifferentiated `geometry` case (V-A).

### Hardcoding vs generator
- Tracer scope literals: 8 hardcoded `get_tracer("compute.<x>")` (history/inference/model/study/codegen/handoff/interval/statistics) — INCONSISTENT (`compute.rigor` for interval, not `compute.interval`). Exactly the runtime-E9/geometry-E14 per-page tracer-literal defect the runtime `[V8]` scope-vocabulary owner and geometry `[V5]` scope table fixed. compute has NO scope vocabulary; a compute-local seed table (or composing runtime's `faults` scope owner) is the generator form.
- Owner-name receipt literals (`Receipt.of("compute.transform", ...)` etc.) — per-page string; acceptable owner self-naming, but note it pairs with the tracer literal for a single owner-scope datum.

### Dead / latent carriers
- symbolic `_lower` callable discarded (tracked by IDEAS `SYMBOLIC_CODEGEN_BRIDGE`).
- study receipt omits `content_key` from `contribute` facts (reuse-unkeyable).

### Unwired / mis-declared seams
- GRADUATION DIRECTION (V-A): handoff frames geometry owners as consumers; they are producers.
- DATA study-input (`ARCHITECTURE.md:53`, TASKLOG `DATA_STUDY_INPUT` QUEUED) — declared, not yet in study.md's fence (expected; card open).
- EvidenceBundle wire — decoded with no proto/drift-gate vs runtime `shapes` law (V-G).

### Unmined capability + governance drift (catalog anchors)
- README `[02]-[DOMAIN_PACKAGES]` OMITS `quadax`/`interpax`/`findiff` — all three carry `.api` catalogs (`compute/.api/quadax.md`,`interpax.md`,`findiff.md`) and are referenced by TASKLOG (`INTERPAX_QUADAX_USAGE`-DROPPED confirms quadax/interpax landed on `solvers/quadrature.md`). README roster is stale vs the .api tier.
- `numerics/jit.md` (231 LOC) and `solvers/field.md` (285 LOC) EXIST on disk but are ABSENT from BOTH `ARCHITECTURE.md` `[01]` codemap (numerics lists array/interval/quantity/statistics; solvers lists mesh not field) AND the README router (no JIT/FIELD rows). jit is referenced by IDEAS `SYMBOLIC_CODEGEN_BRIDGE` and inference.md:21; field by TASKLOG `INTERPAX_QUADAX`-DROPPED. Two orphan pages, unmapped + unrouted (corpus-b bodies, but a compute-governance defect).
- No IMPORT_DAG_LAW: unlike runtime/data/geometry (all three briefs mint an `[IMPORT_DAG_LAW]` + enforceable seam ledger + terminal back-edge reconcile), compute `ARCHITECTURE.md` has a `[02]-[SEAMS]` block but NO stated intra-compute topological order. Intra-compute edges observed: `analysis/signal → analysis/transform` (SpectralReadout), `analysis/{transform,signal,spatial}/symbolic → numerics/array`, `experiments/{model,inference} → graduation/handoff`, `experiments/history → experiments/study`. These are acyclic today but the ledger sees none of them.

### Package migration pressures (upstream ripples landing IN the compute campaign)
- `compute/.api/dask.md` slated for REMOVAL: data `[V12]` relocates dask to branch-tier `libs/python/.api/dask.md` (both consumers named), "the superseded `compute/.api/dask.md` removal traveling as a Ripple to the compute campaign" (data `[06]` `DASK_CATALOG_REHOME` → Compute).
- `sparse` retag `[DATA]`→`[COMPUTE]` (data `[00]`: "`sparse` `[DATA]`-tagged with zero data consumers and a live compute consumer `compute/.planning/numerics/array.md` — retag `[COMPUTE]`").
- `netcdf4` retags `[COMPUTE]`→`[DATA]` (data `[00]`; zero compute consumers) — compute drops any implied claim.
- xarray STAYS branch-tier with compute named (inference.md:45 DataTree) — no compute action beyond honoring the seam.

### FIT — what compute inherits (all finalized upstream)
- IDENTITY: `rasm.runtime.identity` (renamed from `content_identity`, runtime `[V4]`); `ContentIdentity.of` over canonical bytes; `IdentityPolicy` GENERIC seed carrier (runtime `[V7]` — domain knobs ride a consumer-owned policy folded into seed bytes, NEVER IdentityPolicy fields). compute's use is clean on the generic-policy law (no domain knobs on IdentityPolicy anywhere); the OPEN question is CANONICAL_POLICY existence + the 2-arg-vs-3-arg convention (V-B).
- FAULTS/RECEIPTS: `boundary`/`RuntimeRail`/`BoundaryFault`/`FAULT_CONF` (runtime `faults`); `Receipt.of(owner, evidence)` 2-arg / `ReceiptContributor`/`Redaction`/`@receipted`/`Signals.emit` (runtime `receipts`). compute consumes all by symbol, re-mints nothing — a maximal clean consumer (matching the praise data/geometry FIT earned).
- ROOTS: model.md `ResourceRef` (runtime `roots`).
- WIRE/COUNT-PREFIX: `RASM-COMPONENT-PARADIGM-DECISION.md` `[AMENDMENTS]` item 1 (count-prefixed canonical bytes) is consumed TRANSITIVELY through runtime's writer; the amendment's "Python + TypeScript wire decoders" row confirms mirrors are unauthored (`PY_WIRE_ALIGNMENT` QUEUED) and no live fork exists — compute never authors a canonical writer (symbolic/inference correctly defer canonical encode to `ContentIdentity`). The `[AMENDMENTS]` `Rasm.Compute` ripple row is C# `Rasm.Compute` (the managed owner compute GRADUATES INTO), NOT this py folder — distinct.
- GRADUATION: geometry `[V2]` frozen `GeometryHandoff` contract + differentiated subject union is the single most load-bearing inheritance (V-A). geometry `[06]` confirms: "the multi-domain `HandoffAxis` hub and `GraduationReceipt` admission fold stay compute's to build — it inherits `graduation.md`'s frozen contract with geometry the named demanding consumer."

---

## [3] VERDICT CANDIDATES (campaign-defining, evidence-anchored)

### V-A — GRADUATION HUB INVERSION (FIT, top priority)
Geometry `[V2]` (finalized) inverts graduation ownership: `GeometrySubject` is geometry-minted; compute's `HandoffAxis`/`GraduationReceipt` DECODE geometry's frozen `GeometryHandoff` carrier. handoff.md is on the wrong side: it OWNS `GeometrySubject` (61-64), and 21/206 frame geometry owners as consumers of compute-produced literals (backwards — geometry produces). The subject union is STALE (missing `bim-compliance`/`bim-lifecycle`/`section-property`/`building-energy`/`thermal-comfort` from geometry `[V2]`/`[V1]`). Reshape handoff into a true HUB: EGRESS compute-own axes + DECODE geometry-minted carriers; preserve the ONE real compute→geometry outbound projection (`analysis/spatial` reconstructed-mesh, `ARCHITECTURE.md:52`). Ripples: `ARCHITECTURE.md:49-50` seam typing (`graduation/handoff ⇄ geometry/graph` should be `←` receive, matching geometry `[V11]`'s `⇄`-dies ruling); every geometry-subject recital in handoff's `[03]-[CROSS_OWNER]` (200-206) rewrites to decode-side vocabulary. This is a structural rebuild, not a cold-pass edit.

### V-B — CONTENT-IDENTITY POLICY RAIL BIFURCATION (correctness)
Two camps call `ContentIdentity.of` incompatibly. CANONICAL-POLICY/2-arg camp: study.md:477, model.md:381, inference.md:21, interval.md:33+statistics.md:34 (import `CANONICAL_POLICY`), jit (cited by inference:21). IDENTITYPOLICY()/3-arg camp: symbolic.md:767, history.md:293, design.md:42, program.md:40, field.md:42, mesh.md:50 (construct `IdentityPolicy()`). study.md:476 + inference.md:21 EXPLICITLY declare a fresh `IdentityPolicy()` "rejected by the runtime content owner." SMOKING GUN: history.md:293 and study.md:477 key the IDENTICAL `("study", design.tobytes())` differently; the resume cache correctness DEPENDS on equal keys; history.md:341 asserts they match, study.md:476 asserts they can't — a latent silent-cache-miss defect. Plus `CANONICAL_POLICY` is NOT named in the runtime brief's identity surface (ContentIdentity/ContentKey/IdentitySource/IdentityPolicy/derived + reproduction's SeedReproduction/CorpusFixture/ParityReceipt) — potential phantom runtime export. RULING NEEDED: one canonical `of` convention, reconciled against the real runtime `rasm.runtime.identity` API; if `CANONICAL_POLICY` is not a runtime export, the entire 2-arg camp rests on a phantom; if it is, symbolic/history/design/program/field/mesh either re-key on every run or must drop the explicit `IdentityPolicy()`. Waterfall pressure on RUNTIME brief: name `CANONICAL_POLICY` (or its absence) explicitly on the identity surface, with compute the demanding consumer.

### V-C — [RESEARCH] APPENDIX PURGE (grammar, largest clean reduction)
25 `[RESEARCH]` headers across 24 of 26 pages (corpus-a: all but model.md; codegen.md:392 EMPTY). The exact appendix runtime `[V11]`/data `[V11]`/geometry `[V10]` each banned folder-wide. Non-empty tails are load-bearing `.api` member/arity confirmations → purge-and-fold into the owning `Packages` block or the `.api` catalog; "confirmed against the live distribution"/version narration → delete; empty headers → delete. Compute's tail count (25) exceeds runtime (10)/data (16)/geometry (18) — the biggest grammar debt of the py track. Lands per-leg, never a separate pass.

### V-D — content_identity → identity RENAME (FIT mechanical)
21 sites across all 20 pages import `rasm.runtime.content_identity`; runtime `[V4]` renamed the module to `rasm.runtime.identity`. Includes the codegen.md:188 `_BARE` DATA row (ships in generated stub imports). Mechanical corpus-wide sweep exactly as data `[V3]` (22 sites) and geometry `[V12]` (12 sites) executed. Acceptance: `rg 'rasm.runtime.content_identity' libs/python/compute/.planning` returns zero.

### V-E — TRACER SCOPE VOCABULARY (hardcode → generator)
8+ hardcoded `get_tracer("compute.<x>")` literals, inconsistently spelled (`compute.rigor` ≠ `compute.interval`). No compute scope vocabulary. Runtime `[V8]` established a `faults`-owned instrumentation-scope table ("each consumer mints its handle from its row"); geometry `[V5]` collapsed 7 page-local tracer literals onto its graduation spine's scope table. compute mints one seed scope table (compute-local, or compose runtime's `faults` scope owner) so a new owner's scope is one row.

### V-F — GOVERNANCE TRUTH (codemap + router + import-DAG + roster)
compute governance never got the runtime/data/geometry treatment. Land: (a) an `[IMPORT_DAG_LAW]` topological order over the real intra-compute edges with an enforceable seam ledger (currently blind to every intra-compute import); (b) codemap + router truth for the orphan pages `numerics/jit.md` and `solvers/field.md` (both exist, both unmapped + unrouted); (c) README `[02]-[DOMAIN_PACKAGES]` roster fix — add `quadax`/`interpax`/`findiff` (have `.api`, referenced by TASKLOG). Mirrors the runtime `[V4]`/data `[V12]`/geometry `[V11]` governance closeouts.

### V-G — EvidenceBundle WIRE OWNERSHIP (FIT seam)
codegen decodes a C#-minted `EvidenceBundle` (`ARCHITECTURE.md:47`) via raw msgspec with no proto binding, no descriptor drift gate, no runtime `shapes` registry row — against runtime `[V2]`'s single-wire-vocabulary-owner + `descriptor_pool` drift-gate law. Decide: a runtime `transport/shapes` registry row (C#-minted, drift-gated, compute-imported) vs a justified compute-local decode. If the former, waterfall a demanded row into the RUNTIME brief `[V2]` with compute `graduation/codegen` the named consumer.

### V-H — PACKAGE-TIER RIPPLES (land in-campaign)
`compute/.api/dask.md` removal + dask branch-tier relocation (data `DASK_CATALOG_REHOME` ripple → Compute); `sparse` `[DATA]`→`[COMPUTE]` retag lands its compute-owner truth; `netcdf4` `[COMPUTE]`→`[DATA]` retag drops any compute claim; xarray branch-tier honored (inference.md:45 named). These are pre-ruled by the data brief and must execute in the compute campaign's governance leg.

---
NOTE (corpus-b handoff): the CANONICAL_POLICY/IdentityPolicy() split, content_identity rename, RESEARCH purge, and tracer-scope hardcode all span into corpus-b (numerics: array/interval/jit/quantity/statistics; optimization: convex/design/program; solvers: all 8). V-B/V-C/V-D/V-E are FOLDER-WIDE rulings; corpus-b will corroborate. jit.md + field.md orphan-mapping (V-F) are corpus-b bodies.
