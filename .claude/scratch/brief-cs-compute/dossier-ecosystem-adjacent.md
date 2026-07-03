# DOSSIER — Rasm.Compute — lane: ecosystem-adjacent

Stance: hostile-adjacent. The corpus interior is genuinely world-class (25-case `ComputeReceipt` union; 11-physics FEM `SolveLane`; 11-kernel `Optimizer`; full Wiener-Askey UQ; 16-row `Estimator`; 8-design-code structural runner US+Euro; ISO/EN closed-form thermal/acoustic/fire; EnergyPlus + EN 15978/EC3). Interior naivety is minimal and is the census of the corpus-half lanes, not this one. This lane hunts the concerns the target SHOULD own that nobody has named — the ADJACENT/expansion axes and what the corpus is silent on — with the same evidence bar and license gate. Every candidate below is verified real, maintained, .NET-current, and license-admissible; the table is SEED DATA for the brief's escalation and roster rows, not a build order.

License gate applied: OSS-usable + free-for-OSS-commercial admitted; pay-tiered/seat-licensed/proprietary REJECTED. Native engines with clean C# bindings/P-Invoke admissible where the managed ecosystem lacks the concern.

---

## [0] METHOD + VERIFICATION LEDGER

NuGet flat-container + GitHub license/activity probes (2026-07-02):

| Package / repo | Version | License | Last push | Admitted centrally? | Verdict |
| -------------- | ------- | ------- | --------- | ------------------- | ------- |
| `Microsoft.Z3` | 4.12.2 stable (upstream nightly 2026-06, LIA/LRA/NRA/NIA) | MIT | 2026-06-18 | NO | ADMIT — SMT rule/compliance |
| `wo80/csparse-interop` (ARPACK/MKL/SuiteSparse/SuperLU bindings) | not-on-NuGet (source) | BSD-3 | 2025-11-30 | NO | ADMIT — sparse eigensolver + native direct; SAME AUTHOR as admitted `CSparse` |
| `Microsoft.ML.Probabilistic` + `.Compiler` (Infer.NET) | 0.4.2504.701 | MIT | 2025-12-08 | NO | ADMIT — Bayesian inference/calibration |
| `govert/RobustGeometry.NET` (Shewchuk exact predicates) | not-on-NuGet (single-file vendor) | MIT (predicate core public-domain) | 2014 (stable spec) | NO | ADMIT — robust orient3d/insphere |
| `QuikGraph` | 2.5.0 | MS-PL | 2024-08-20 | YES (`libs/csharp/.api/api-quikgraph.md`) — NOT consumed by Compute | CONSUME — egress/circulation |
| `NetTopologySuite` / `Clipper2` | 2.6.0 / 2.0.0 | BSD-3 / BSL-1.0 | 2026-04 | YES (central manifest) — NOT consumed by Compute | CONSUME — planar/isovist |
| `wo80/Triangle.NET` | not-on-NuGet | permissive (confirm) | 2026-06-10 | NO | OPTIONAL — 2D quality mesher (boundary path only); same author as `CSparse` |
| `MichaCo/CDT.NET` | 1.0.0 | MPL-2.0 | 2026-02-26 | NO | OPTIONAL — 2D constrained Delaunay, robust |
| `SolarAngles` (ThomasSchuetz) | 1.0.1 | MIT | 2020 (thin) | NO | SEED — solar angles; NREL SPA public-domain hand-rollable |
| `Epanet.Net` (GPL) | — | GPL | — | — | REJECT — copyleft; use public-domain EPA toolkit or compose sparse solver |
| `Numerics.NET` (Exoanalytics) sparse eigen | — | commercial | — | — | REJECT — seat-licensed |

Corpus interior confirmed via full read of: `ARCHITECTURE.md`, `README.md`, `Rasm.Compute.csproj`, `IDEAS.md`, `TASKLOG.md`; deep-read `Solver/{optimizer,discretization,contract,clash,sweep,uncertainty}`, `Analysis/{structural,physics,assessment,energy,lifecycle,aggregator}`, `Model/{embedding,inference,generative}`, `Stats/{estimator,signal}`, `Tensor/{blas,factor}`, `Symbolic/expression`, `Runtime/receipts`; charters of the remainder.

---

## [1] PER-PAGE VERDICTS (ecosystem-adjacent lens)

Score = internal quality (1-10). The adjacency note is the concern the owner SHOULD grow into. Where internal quality is 8+, the page needs no interior rework from THIS lane — the value is the growth axis.

### Tensor/
- `vocabulary.md` (332) — 9. `Tensor<T>`+196-row op family over `System.Numerics.Tensors`. No adjacency; substrate is correct.
- `layout.md` (145) — 9. Reshape union. No adjacency.
- `dispatch.md` (958) — 9. Arity kernel tables + forward/reverse AD tape + JacobianColoring. AD is hand-rolled but deliberate (geometry tape). No package adjacency; excellent.
- `residency.md` (209) — 9. OrtValue C-data lattice + geometry-to-tensor. No adjacency.
- `memory.md` (279) — 9. Recyclable zero-copy pool. No adjacency.
- `blas.md` (466) — 9. RID-keyed provider + ATen native leg + LM. **ADJACENT:** dense-only spectral; the native-direct scale-out (MKL PARDISO / SuiteSparse) is the `csparse-interop` opportunity, aligned with the sparse-eigen gap (VC2).
- `factor.md` (572) — 9. CSR sparse direct/iterative + kernel lowering. **ADJACENT:** `csparse-interop` (same `wo80` author) adds ARPACK sparse eigensolver + native MKL/SuiteSparse/SuperLU as one `FactorKind`/`Modal` row — VC2.
- `quadrature.md` (366) — 9. Accuracy-routed adaptive quadrature. No adjacency.
- `sampling.md` (438) — 9. Owned Sobol/Halton + RBF scatter. No adjacency; correctly owned.

### Symbolic/
- `expression.md` (173) — 9. Closed CAS over MathNet.Symbolics, content-keyed. **ADJACENT:** the CAS is the natural lowering target for a Z3 rule-check owner (VC3) — a rule predicate is a `SymbolicExpr` lowered to a Z3 assertion, not a new parser.
- `dimensional.md` (204) — 9. ℚ⁷ SI proof. No adjacency.
- `lowering.md` (195) — 9. Content-keyed compiled cache + analytic Jacobian. No adjacency.
- `units.md` (185) — 8. UnitsNet boundary. No adjacency.

### Model/
- `identity.md` (130) / `sessions.md` (177) / `providers.md` (141) / `extension.md` (66) — 8-9. ONNX identity/session/EP/custom-op. No adjacency; deliberate ONNX-only.
- `inference.md` (234) — 9. OrtValue-only run fold + Classify/ClashScore/Embed reuse. No adjacency; the BIM classifier reuse is exemplary.
- `embedding.md` (231) — 9. VectorEncoding/Score/Rank; PQ over Persistence-trained codebook; ANN index correctly delegated to Persistence. No Compute-side adjacency (HNSW is Persistence's, by contract).
- `generative.md` (354) — 9. ORT-GenAI streaming + tool-call + LoRA. No adjacency; M.E.AI governed at AppHost.

### Solver/
- `discretization.md` (896) — 7. 12-element isoparametric + real Bowyer-Watson/octree/sweep/inflation. **DEFECT (robustness):** in-sphere/orient predicates are FLOAT (`MeshSpace.Encloses` det `< 1e-9f` ~L388; `DelaunayCore.Triangulate` `Topology.InSphere/Orient` float ~L585-617; Boundary L18 names them "this page's named kernel exemption"). Float predicates fail on coplanar/axis-aligned/cocircular building geometry → non-manifold/inverted cells. **ADJACENT:** `RobustGeometry.NET` (MIT, Shewchuk adaptive-precision) is the surgical fix — VC4.
- `contract.md` (1093) — 9. 11-physics `Bᵀ·D·B` FEM. **DEFECT (self-flagged capability ceiling):** L13/L104 admit `dense-evd` is "the only eigensolver the admitted MathNet/CSparse stack provides, never a phantom sparse LOBPCG/Lanczos" — but `fea-modal`/`fea-buckling` on real buildings are 10⁴-10⁶ DOF where dense O(n³) EVD is infeasible. **ADJACENT:** `csparse-interop` ARPACK shift-invert Lanczos — VC2. **ADJACENT:** `cfd-incompressible` (L32) and `daylight-radiosity` (L35) are single FEM rows, not disciplines — see VC5.
- `optimizer.md` (1128) — 10. 11 genuine kernels incl. topology-SIMP + Bayesian-GP + CMA-ES. **ADJACENT:** gradient-adjoint is hand-rolled trust-region/Armijo; a managed interior-point NLP (no permissive .NET IPOPT exists) stays hand-rolled — no action. CP-SAT owns optimize; Z3 owns VERIFY (VC3), an orthogonal concern.
- `sweep.md` (313) — 9. DOE + joint space-filling + tornado. No adjacency.
- `clash.md` (344) — 9. BVH narrow-phase + Kalman/CUSUM twin. **ADJACENT (reuse):** the `AccelerationStructure` BVH + two-direction Möller-Trumbore is a ready ray-cast engine for daylight shadow-rays and geometric room-acoustics (image-source/ray-trace) — VC5, VC8. Twin's scalar Kalman is the seed the Bayesian owner (VC6) generalizes.
- `uncertainty.md` (696) — 10. FORM/SORM/PCE/Sobol/Morris/subset, copula. **DEFECT (coverage, not approach):** forward-only. INVERSE (calibration/Bayesian updating) has no owner — VC6.

### Stats/
- `estimator.md` (869) — 10. 16-row Fit/Predict, three honest solution mechanisms. **ADJACENT:** classical only (training is Python by contract). Bayesian inference (VC6) is a distinct engine, not an `EstimatorKind` row.
- `signal.md` (578) — 9. FFT/STFT/PSD/wavelet + FIR/IIR. No adjacency; feeds estimator/twin correctly.

### Runtime/
- `admission.md` (234) / `scheduling.md` (394) / `progress.md` (180) / `receipts.md` (314) / `channels.md` (643) / `codecs.md` (730) / `payload.md` (302) — 9. Admit-to-receipt plane. No adjacency; adding a discipline (VC1/VC5) is one `AssessmentRoute` + one `ComputeReceipt.Assessment` case (already the 25th, `partial`) — the plane is built for it.

### Analysis/
- `assessment.md` (335) — 9 (interior), 6 (coverage). The Discipline→runner→route→`Node.Assessment` spine is a textbook open/closed generator. **DEFECT (coverage):** L14 disciplines = structural/thermal/acoustic/fire/energy/environmental/cost — NO circulation/egress (life-safety, code-mandated) and NO daylight. The roster is a SEED of 7, not a ceiling — VC1, VC5, VC7.
- `structural.md` (698) — 8. Frame-only (BFE 3D + FEALiTE 2D) + 8 design codes. **ADJACENT (coverage):** no shell/plate runner (slab/wall/diaphragm), no seismic response-spectrum (though `StructuralCase.Seismic` exists and `Solver/contract` has `fea-modal`), no footfall/vibration serviceability `LimitState`, no connection/foundation design. Growth = new `LimitState` rows + a modal-response-spectrum composition once VC2 lands sparse modal. Scope-mandate residuals (2)(3) (cold-formed capacity overload; EC2 6.2.3 shear) are corpus-half census, noted not owned here.
- `physics.md` (698) — 9. Closed-form thermal (6946/13788 Glaser + 10077 window) / acoustic (12354) / fire (1993/1992-1-2). **ADJACENT (coverage):** daylight absent; hygrothermal transient (EN 15026) flagged as growth (L17); geometric room-acoustics (RT60/STI) reusing clash BVH is unnamed.
- `energy.md` (430) — 9. EnergyPlus whole-building via parameterized toolchain. No adjacency; comprehensive.
- `lifecycle.md` (430) — 9. EN 15978 + EC3/openEPD + cost. **ADJACENT (minor):** operational carbon (energy owns), circularity/material-passport/biogenic — future `LciaMethod`/fold rows, not packages.
- `aggregator.md` (306) — 9. Multi-ply ISO 6946/12354/13786/13788 + 10077 window. No adjacency.

---

## [2] CROSS-CUTTING

**Duplication / concern-mixing / dead carriers:** none found from this lane — the corpus is aggressively de-duplicated (one `evaluate` oracle across optimizer/sweep/uncertainty; one receipt union; one `AssemblyAggregator` relocated from Materials; contract-uniform solver lanes). The corpus-half lanes own any interior census.

**Hardcoding-vs-generator:** the Analysis Discipline roster (assessment.md:14) and the `PhysicsKind` roster (contract.md) READ as generators (SmartEnum rows + total Switch that breaks at compile time), but their DISCIPLINE/PHYSICS coverage is an enumerated seed, not the domain. This is the correct axis to attack: a world-class AEC compute platform's disciplines are circulation, daylight, MEP-flow, comfort — none named. This is COVERAGE naivety (thin slice of the owned domain), not APPROACH naivety; the approach (row + runner + route) is exemplary.

**Unwired seams / unmined admitted capability (highest-value):**
- `QuikGraph` (MS-PL, `api-quikgraph.md`) is admitted substrate with Dijkstra/A*/Bellman-Ford/**max-flow**/min-cut/SCC/MST/LCA, consumed by Element (ElementGraph topology), Persistence (topology lane), Bim (CPM/MEP-trace/version-graph) — but NOT Compute. Compute is the measured-analysis owner; graph-theoretic ANALYSIS (egress distance, egress capacity via max-flow, betweenness/integration for wayfinding) is Compute's to own, unwired. — VC1.
- `NetTopologySuite` (2.6.0) + `Clipper2` (2.0.0) are centrally pinned but unconsumed by Compute; the planar side of egress/daylight/site-analysis (isovist visibility polygons, corridor medial axis, occupant-area, setback/FAR envelope) has no Compute owner. — VC1/VC3/VC5.
- `Solver/clash` BVH + Möller-Trumbore is a general ray-cast engine used only for clash; daylight shadow-rays and geometric room-acoustics could reuse it — unmined. — VC5/VC8.
- Central manifest already pins `MathNet.Numerics.Providers.MKL` (blocked native row, `[NATIVE-BLAS]`); `csparse-interop` would light the SAME native tier (MKL/SuiteSparse) AND add the missing ARPACK sparse-eigen the managed stack cannot reach. — VC2.

**Folder-architecture verdict:** SOUND, no rot. `.planning/` mirrors the six-folder eventual source tree; each folder is a genuine higher-order domain (Tensor/Symbolic/Model/Solver/Stats/Runtime/Analysis); no loose one-file-one-folder combos; no flat sprawl. The growth of every adjacency below lands INSIDE the existing structure: a discipline = one `Analysis/<name>.md` + one `Discipline` row + one `AssessmentRoute` + the already-declared `ComputeReceipt.Assessment` case; a numeric enabler = one row on the existing `Tensor/factor` or `Tensor/blas` owner. No new top-level folder is warranted by any finding. The folder architecture is BUILT for this expansion — the finding is that the discipline/physics/package rosters are seeds, and the campaign should treat "which disciplines does an AEC compute platform own" as the primary escalation axis. — VC7.

**Interior naivety on this lane:** exactly one — the float geometric predicates in `discretization.md` (VC4). Everything else scoring <9 is COVERAGE (missing discipline), not a code defect.

---

## [3] VERDICT CANDIDATES (campaign-defining, most-severe first)

**VC1 — CIRCULATION/EGRESS is the missing life-safety Analysis discipline; QuikGraph is admitted and unconsumed.**
Evidence: `assessment.md:14` disciplines = structural/thermal/acoustic/fire/energy/environmental/cost — no circulation/egress. `api-quikgraph.md` documents QuikGraph (MS-PL, admitted) max-flow/min-cut/Dijkstra/A*/SCC consumed by Element/Persistence/Bim but NOT Compute. Egress travel-distance, egress-width/exit-capacity (max-flow/min-cut), dead-end/common-path-of-travel, occupant-load distribution are IBC Ch.10 / EN code-MANDATED — a first-class compliance discipline absent entirely.
Ruling: add `Analysis/circulation.md` — a `Discipline.Circulation` runner reading the space-adjacency subgraph of the concrete `ElementGraph` (spaces + door/opening connections), computing shortest-path egress distance (Dijkstra/A*), egress capacity (max-flow = required-vs-provided exit width), and occupant-load-weighted flow, one `AssessmentRoute` per code (IBC-1017/1005, EN), writing `Node.Assessment` exactly as the other runners. Planar side (isovist/corridor medial-axis/occupant-area) via admitted `NetTopologySuite`/`Clipper2`. ZERO new package. Largest single domain-coverage hole; lowest friction.

**VC2 — Sparse eigensolver gap is self-flagged and caps structural modal/seismic/buckling; csparse-interop (same author as admitted CSparse) is the fix.**
Evidence: `contract.md:13,104` — `dense-evd` is "the only eigensolver the admitted MathNet/CSparse stack provides, never a phantom sparse LOBPCG/Lanczos." Building `fea-modal`/`fea-buckling` is inherently 10⁴-10⁶ DOF; dense O(n³) EVD is infeasible past ~few-thousand DOF, so seismic modal-response-spectrum and linear-buckling on real models are unreachable. `wo80/csparse-interop` (BSD-3, active 2025-11) binds ARPACK (shift-invert Lanczos — the industry-standard sparse generalized eigensolver, the `Kx=λMx` shifted-block-Lanczos the buckling/modal literature uses) plus native MKL/SuiteSparse/SuperLU direct solvers — and is authored by `wo80`, the SAME author as the admitted `CSparse 4.4.0`.
Ruling: admit `csparse-interop` (source-vendored, per-RID native ARPACK/OpenBLAS/SuiteSparse) as the sparse-eigen + native-direct owner; add a sparse `Modal` row to `Tensor/factor`/`Solver/contract` so `fea-modal`/`fea-buckling` scale and structural seismic response-spectrum becomes reachable. Simultaneously lights the blocked `[NATIVE-BLAS]` MKL tier for large sparse direct. Single biggest capability ceiling on the structural discipline.

**VC3 — Rule/code-compliance (SMT) is an unnamed capability class; Microsoft.Z3 (MIT) is categorical-best, orthogonal to CP-SAT.**
Evidence: Compute owns OR-Tools CP-SAT (`optimizer.md`, OPTIMIZE) and relays ifctester/IDS (Bim, schema-conformance). But VERIFYING a design satisfies a logical+arithmetic rule set — min ceiling height, egress width, ADA/accessibility clearance, setback, zoning envelope, spatial-program adjacency — parametric/geometric compliance over reals+integers — has no owner. `Microsoft.Z3` (MIT, macOS-arm64 natives shipped, NRA/NIA nonlinear real+integer SMT, upstream nightly-active 2026-06) is the categorical-best.
Ruling: admit `Microsoft.Z3`; a `RuleCheck` owner lowers a typed rule set (naturally a `SymbolicExpr`, `Symbolic/expression`) to Z3 assertions and returns SAT/UNSAT + model/unsat-core as an `AssessmentResult` (unsat-core = the exact violated rules). CP-SAT optimizes; Z3 verifies + explains — orthogonal. Frames the generative platform's "is this design legal/feasible" question that nothing currently answers. Caveat: stable NuGet 4.12.2 (2023) lags the very-active upstream; pin a fresh build or the community repackage.

**VC4 — discretization.md float geometric predicates are a robustness defect on exactly the geometry buildings produce; RobustGeometry.NET (MIT) is the surgical fix.**
Evidence: `discretization.md` hand-rolls Bowyer-Watson with FLOAT predicates — `MeshSpace.Encloses` Möller-Trumbore `Math.Abs(det) < 1e-9f` (~L388), `DelaunayCore.Triangulate` `Topology.InSphere`/`Orient` in `Vector3`/`float` (~L585-617), Boundary L18 self-names them "this page's named kernel exemption." Float orientation/in-sphere tests give wrong signs on near-degenerate configs — coplanar faces, axis-aligned edges, cocircular points — which are the COMMON case in building geometry, producing non-manifold/inverted meshes the FEM solve then trusts.
Ruling: admit `govert/RobustGeometry.NET` (MIT; Shewchuk adaptive-precision orient2d/orient3d/incircle/insphere; predicate core placed in public domain by Shewchuk; single-file vendor; the algorithm is a STABLE spec so 2014 staleness is moot) as the exact-predicate substrate; keep the hand-rolled Bowyer-Watson topology. Note: the 3D volumetric-meshing ecosystem is copyleft (Gmsh GPL / Netgen LGPL / TetGen AGPL / CGAL GPL) — this JUSTIFIES the hand-rolled mesher but NOT the float predicates; the fix is surgical, no copyleft adoption. (`wo80/Triangle.NET` and MPL `CDT.NET` are optional permissive 2D quality-mesher upgrades for the 2D boundary/element path only.)

**VC5 — Daylight/solar is one thin FEM row, not a discipline; solar-geometry is hand-rollable, CBDM stays Python.**
Evidence: `contract.md:35` `daylight-radiosity` is ONE `PhysicsKind` row on the general FEM contract; `physics.md` building-physics runner omits daylight entirely; there is no solar-position, sky-model, or shortwave-radiation owner. Daylighting (climate-based daylight modeling sDA/ASE, solar gain, shading, glare) is a first-class AEC discipline.
Ruling: name a `Discipline.Daylight` runner. The primitive — NREL SPA solar position (public-domain STABLE spec; C# ports exist; `SolarAngles` MIT is a seed), Perez all-weather sky, cumulative radiation — is hand-rollable in-folder; shadow/visibility ray-casts REUSE the `clash.md` BVH + Möller-Trumbore. Full Radiance-class CBDM/glare stays the Python companion (Ladybug/Honeybee/frads/LBNL) decoded over the existing `ONE_GRADUATION_EVIDENCE` seam — mirroring the corpus's own Python-owns-training, C#-owns-inference law. Coverage gap with a hand-rollable primitive + a Python-companion tail.

**VC6 — UQ is forward-only; inverse/Bayesian calibration has no owner; Infer.NET (MIT, active) is categorical-best.**
Evidence: `uncertainty.md` owns forward UQ + reliability (FORM/SORM/PCE/Sobol/Morris/subset) — but the INVERSE problem (calibrating an energy or structural model against measured/metered/monitoring data, Bayesian parameter updating, sensor fusion) is unowned; `clash.md`'s digital twin uses a hand-rolled scalar Kalman only. `Microsoft.ML.Probabilistic` (Infer.NET) 0.4.2504.701 (MIT, active 2025-12; EP/VMP/Gibbs message-passing) is the categorical-best managed Bayesian engine.
Ruling: admit Infer.NET as the model-calibration/probabilistic-inference owner — energy-model calibration to metered data, structural-model updating to monitoring, twin state-estimation beyond scalar Kalman. Pairs with the digital-twin and the `ARCHITECTURE.md:116` Compute→Persistence/Store/quality anomaly seam. Caveat: heavyweight (runtime model compiler `.Compiler`); scope to calibration/inference, not as an `EstimatorKind` row (`estimator.md` is classical fit).

**VC7 — Folder architecture is sound and BUILT for this; the discipline/physics/package rosters are the growth axis, not the ceiling.**
Evidence: `.planning/` mirrors the six eventual source folders; each is a genuine higher-order domain; no one-file folders, no flat sprawl; the `Analysis` spine (Discipline row → runner page → `AssessmentRoute` → `ComputeReceipt.Assessment` `partial`, `assessment.md`) is a textbook open/closed generator; `PhysicsKind`/`OptimizerKind`/`EstimatorKind`/`UncertaintyMethod` are all row-generators. Every adjacency above (VC1/VC3/VC5, and secondary VC8) lands as ONE `Analysis/<name>.md` + ONE row + ONE route; every numeric enabler (VC2/VC4) lands as ONE row on an existing `Tensor` owner.
Ruling: NO new top-level folder is warranted by any finding; no folder rot. The campaign should treat "which disciplines an AEC compute platform owns" as the primary escalation axis and add discipline pages under `Analysis/`, package rows to the roster, and numeric-enabler rows to `Tensor/` — never restructure. This is a graded PASS on folder architecture with the explicit ruling that coverage, not structure, is where the naivety lives.

**VC8 — Secondary reuse adjacencies: MEP network-flow discipline and geometric room-acoustics both compose existing substrate, no new package.**
Evidence: (a) MEP hydraulic/thermal network sizing (duct/pipe/hydronic pressure-flow, Hardy-Cross/global-gradient) is a graph-Laplacian solve the admitted sparse solver (`Tensor/factor`) + admitted QuikGraph already enable; the EPA EPANET toolkit is public-domain (the `Epanet.Net` NuGet is GPL — REJECTED — but the underlying toolkit and the Todini global-gradient algorithm are freely implementable). (b) Geometric room acoustics (RT60/STI via image-source/ray-tracing) reuses the `clash.md` `AccelerationStructure` BVH + Möller-Trumbore already built, complementing the closed-form ISO 12354 in `physics.md`.
Ruling: name both as runner-level compositions of existing substrate (a `Discipline.Network`/`Discipline.Acoustic`-geometric growth), not new packages — evidence the corpus's numeric core already over-provides for its named disciplines, which is precisely why the discipline roster (VC1/VC5/VC7) is the campaign's true frontier.

---

## [4] SCOPE-MANDATE ACKNOWLEDGEMENT (not this lane's to own)

The accumulated census (COMPUTE_ASSESSMENT_RUNNERS failure-lifecycle rebuild; Materials-leg cold-formed capacity-overload + EC2 6.2.3 shear; Component-paradigm re-point; Persistence-seam counterpart rows; python:data GLB layout) is corpus-half / cross-package census, verified present in `assessment.md`/`structural.md`/`ARCHITECTURE.md` but owned by the sibling survey lanes. This lane's only intersection: the assessment-runner Failed-lifecycle work (scope item 1) is where the Bayesian-anomaly owner (VC6) and a retry/re-solve health signal would eventually compose — noted, not claimed.

WATERFALL RIPPLE (upstream briefs): no edit demanded. `RASM-CS-GEOMETRY-BRIEF.md` and `RASM-CS-PERSISTENCE-BRIEF.md` foundations are consumed as boundary contracts; none of VC1-VC8 demands a capability those briefs lack (QuikGraph/NTS are monorepo substrate; the sparse-eigen/predicate/SMT/Bayesian packages are Compute-interior roster additions; egress/daylight disciplines read the Element `ElementGraph` the geometry/persistence briefs already settle). The vector-index delegation to Persistence (`embedding.md`) is honored, not extended.
