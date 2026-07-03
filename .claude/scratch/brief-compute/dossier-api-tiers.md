# [DOSSIER_API_TIERS] — libs/python/compute

Lane: api-tiers. Stance: hostile. Scope: BOTH catalog tiers COMPLETE — `libs/python/.api/*.md` (26 branch stubs) + `libs/python/compute/.api/*.md` (35 folder stubs), judged mined-vs-unmined against the 26 owning `.planning` pages (9187 LOC). Upstream law consumed: `RASM-RUNTIME-BRIEF.md`, `RASM-DATA-BRIEF.md`, `RASM-GEOMETRY-BRIEF.md` (all read full). Method: per-package fence-member surface (`- Packages:` lines) vs catalog surface; import-site counts (alias-corrected); pyproject tag/roster reconciliation.

Anchors are `.planning/`-relative unless prefixed. `pyproject.toml:` anchors repo root. `ARCH:`/`README:` anchor `libs/python/compute/{ARCHITECTURE,README}.md`.

---

## [00]-[HEADLINE]

The compute `.api` corpus is CRAFT-STRONG and DISCIPLINE-DRIFTED. The 35 folder catalogs are integration-shaped (every stub carries a `[STACKING]`/`[LOCAL_ADMISSION]` section, not a flat API dump), and the deep-science spine is exemplary: `sympy` (the deepest mine in the folder — calculus/rewrite/solve/linalg/numbertheory/codegen with a `python-flint` FLINT ground-domain acceleration tier), `scipy` (124 fence refs across 12 pages, each mining a distinct submodule), `cvxpy`/`clarabel`, `pint`/`uncertainties`, `optimistix`/`lineax`/`diffrax`/`equinox`/`optax`/`jax`, `array-api-compat`/`array-api-extra`/`sparse`, `pymc`/`arviz`, `SALib`, `onnx`/`onnxruntime`/`skl2onnx`/`scikit-learn`, `meshio`/`scikit-fem`/`pywavelets`/`numba`/`quadax`/`interpax` are all mined to fence depth. The rot is FIVE structural classes, all api-tiers-visible:

1. **Three over-authored sampler catalogs for string-only backends.** `numpyro.md`/`blackjax.md`/`nutpie.md` (~55KB, ~1300 catalog LOC) fully document sampler/kernel/adaptation/SMC/SGMCMC/VI surfaces for packages consumed ONLY as literal strings in `type NutsSampler = Literal["numpyro","blackjax","nutpie"]` (`inference.md:50`) routed through `pymc.sample(nuts_sampler=)` — `inference.md:22` states verbatim "installed only so PyMC's own dispatch resolves them, **never imported here**". `blackjax.md:60,73,83` catalog `smc`/`sgmcmc`/`vi` — the exact surface `TASKLOG [BLACKJAX_RAW_ALGEBRA]-[DROPPED]` seals out of charter.
2. **One over-catalogued near-dead admission.** `findiff.md` (17.2KB, 4 sections) documents the full Diff-operator algebra / vector-calculus / matrix-representation / Padé / BVP-PDE surface; `sensitivity.md:469,477` mines ONLY `coefficients(deriv=1, acc=acc)` for the FD floor. The catalog charter even claims "composing `Diff` and its `.matrix(shape)`" — a composition NO fence performs.
3. **One mislocated + untagged + over-catalogued substrate.** `compute/.api/dask.md` (owner:compute, folder-tier) is consumed only as a passive `array_namespace` backend in one TYPE_CHECKING union (`array.md:48,53`); `RASM-DATA-BRIEF.md [V12]` relocates it to branch-tier `libs/python/.api/dask.md` (both consumers named) as the `DASK_CATALOG_REHOME` ripple; `pyproject.toml:50` carries it UNTAGGED (every peer has a `# [TAG]:`).
4. **Roster + tag + owner-line drift.** README `[02]` omits three consumed admissions (`findiff`/`quadax`/`interpax`); README `[03]` lists two never-consumed substrate rails (`pydantic`/`anyio`) and omits one consumed (`universal-pathlib`); `pyproject.toml:118` tags `sparse [DATA]` against a live compute consumer (data `[V12]` retags `[COMPUTE]`); `array-api-compat.md`/`array-api-extra.md` carry NO `- owner:` line.
5. **Two upstream-rename time bombs + one paradigm inversion the catalogs encode.** 20 pages / 21 sites import `rasm.runtime.content_identity` (runtime `[V4]` renames it `identity` — the folder is unimportable the moment runtime lands); `handoff.md:61-64` MINTS `GeometrySubject` as a compute `Literal` that geometry `[V2]` re-homes to a geometry mint (compute must become the DECODER); `ARCH:50` mistypes the crossing `⇄`.

Cross-cutting the api tiers: **8 pages claim "worker lane" execution in prose; ZERO fences compose `rasm.runtime.lanes.offload`** (`offload(` count = 0 corpus-wide) — the folder's whole CPU-bound science spine (JAX x64 solves, scipy sparse factorizations, `pymc` sampling, `cvxpy` conic solves) runs synchronously on the event loop, an illusory-concurrency defect identical to geometry E5, and an unnamed demanding-consumer pressure on runtime `[V5]`.

---

## [01]-[TIER_INVENTORY]

### [COMPUTE_TIER] — 35 folder catalogs

| Stub | owner/rail | Mined verdict | Consuming fence(s) | Defect |
|---|---|---|---|---|
| `scipy.md` | compute/solvers | 9 | linear/nonlinear/quadrature/program/statistics/signal/transform/spatial | missing `lobpcg`/`OPinv`/`ArpackNoConvergence`/`svds` shift-invert surface `IDEAS [SPARSE_EIGENVALUE_ROUTE]` needs (self-flagged RESEARCH) |
| `sympy.md` | compute/symbolic | 10 | symbolic.md (deepest mine) | none — exemplary |
| `python-flint.md` | compute/exact-arithmetic | 9 | interval.md, symbolic.md (FLINT ground-domain) | none |
| `mpmath.md` | compute/exact-arithmetic | 9 | interval.md, symbolic.md | none |
| `lineax.md` | compute/diff-linalg | 9 | linear.md, nonlinear.md | none |
| `optimistix.md` | compute/diff-nonlinear-opt | 9 | nonlinear/design/differential + linear | none |
| `diffrax.md` | compute/differential-eq | 9 | differential.md (full solver/adjoint/Levy family) | none |
| `equinox.md` | compute/model | 9 | linear/nonlinear/differential/design/sensitivity | none |
| `optax.md` | compute/first-order-opt | 8 | nonlinear.md, design.md | none |
| `jax.md` | compute/accelerator | 9 | 10 pages (sensitivity/jit/differential/nonlinear/linear/design/…) | none |
| `numba.md` | compute/accelerator | 8 | jit.md ONLY (7 refs) | none — narrow but correct owner |
| `cvxpy.md` | compute/convex | 9 | convex.md | none |
| `clarabel.md` | compute/convex-backend | 8 | convex.md | none |
| `scikit-fem.md` | compute/FEM | 8 | field.md, mesh.md, quadrature.md | none |
| `meshio.md` | compute/mesh-exchange | 8 | mesh.md (read/write/CellBlock/cell_*_dict/sets round-trips) | none |
| `pywavelets.md` | compute/signal | 8 | signal.md (wavedec/swt/cwt/WaveletPacket) | none |
| `pint.md` | compute/units | 9 | quantity.md | none |
| `uncertainties.md` | compute/uncertainty | 9 | quantity.md (ufloat/correlated_values/unumpy/ulinalg) | none |
| `SALib`→`salib.md` | compute/sensitivity | 9 | study.md (full sample+analyze family) | none |
| `scikit-learn.md` | compute/model | 8 | study.md, model.md (import token `sklearn`) | none |
| `onnx.md` | compute/model | 8 | model.md | none |
| `onnxruntime.md` | compute/model | 8 | model.md | none |
| `skl2onnx.md` | compute/model-asset | 8 | model.md | none |
| `arviz.md` | compute/Bayesian-study | 9 | inference.md (summary/hdi/loo/psense) | none — arviz-1.x renames correct |
| `pymc.md` | compute/Bayesian-study | 9 | inference.md | none |
| `sparse.md` | compute/array | 9 | array.md (COO/from_numpy/asformat/asnumpy/maybe_densify) | `pyproject.toml:118` still tags `[DATA]` — data `[V12]` retags `[COMPUTE]` |
| `array-api-compat.md` | (compute)/array-api | 8 | array.md, transform.md | **NO `- owner:` line** |
| `array-api-extra.md` | (compute)/array-api | 8 | array.md, transform.md | **NO `- owner:` line** |
| `interpax.md` | compute/interpolation | 8 | quadrature.md (CubicSpline/Pchip/Akima/interp1d + .derivative/.integrate/.roots) | README `[02]` roster-ABSENT |
| `quadax.md` | compute/quadrature | 8 | quadrature.md (adaptive_quadrature/GaussKronrod/romberg/tanhsinh/sampled) | README `[02]` roster-ABSENT |
| `findiff.md` | compute/finite-diff | **3** | sensitivity.md — `coefficients` ONLY | over-catalog (full Diff/vector-calc/matrix/PDE unmined); charter claims `Diff`/`.matrix` never used; README `[02]` roster-ABSENT |
| `numpyro.md` | compute/Bayesian-study | **3** | inference.md — STRING `"numpyro"` + `chain_method` kwarg | full model/effect-handler/MCMC/SVI/dist catalog for a never-imported package |
| `blackjax.md` | compute/Bayesian-study | **2** | inference.md — STRING `"blackjax"` only | full mcmc/smc/sgmcmc/vi catalog; smc/sgmcmc/vi are `TASKLOG`-dropped forms |
| `nutpie.md` | compute/Bayesian-study | **3** | inference.md — STRING `"nutpie"` + `backend` kwarg | over-catalog (`compile_pymc_model`/`compile_stan_model`/`benchmark_logp` unmined) |
| `dask.md` | compute/studies | **2** | array.md — passive `array_namespace` backend only | MISLOCATED (data `[V12]` → branch tier `DASK_CATALOG_REHOME`); over-catalog; `pyproject.toml:50` UNTAGGED |

### [BRANCH_TIER] — 26 shared stubs (compute-relevant subset)

| Stub | owner | Compute consume | Verdict |
|---|---|---|---|
| `expression.md` | (branch) | every page (ADT/Result/Block/Map spine) | mined — correct |
| `msgspec.md` | (branch) | every page (Struct/Meta) | mined — correct |
| `beartype.md` | (branch) | 20 pages (`@beartype(conf=FAULT_CONF)`, vale.Is, FrozenDict, door.is_bearable) | mined — correct |
| `numpy.md` | (branch) | every page | mined — correct |
| `xarray.md` | data | inference.md `DataTree` (`inference.md:45`) | co-consumer; owner line says only `owner: data` — data `[V12]` adds compute |
| `universal-pathlib.md` | runtime | model.md `UPath` (`model.md` Packages) | consumed but README `[03]` omits it |
| `pydantic.md` | (branch) | **ZERO compute fences** | README `[03]` over-declares |
| `opentelemetry-api.md` | (branch) | handoff/inference/interval/statistics/study/model (spans) | mined — correct |
| others (grpcio*/protobuf/adbc/arro3/daft/networkx/numcodecs/zlib-ng/psutil/pydantic-settings/stamina/structlog/trio/opentelemetry-sdk/-exporter/xxhash) | runtime/data | none by compute | out-of-folder; not compute's judgment |

---

## [02]-[PER_PAGE_VERDICTS]

Verdicts are api-tiers-lensed (package-consumption quality + structural issues caught during the survey), not a full ADT re-grade.

- **solvers/receipt.md** (146 LOC) — 9. Pure receipt owner, no external package. `SolverReceipt`/`SolveStatus` is the one convergence carrier every solver route folds. `content_identity` import present. Charter: the method-discriminated solve receipt; correct.
- **solvers/linear.md** (473) — 9. Deep scipy (full Krylov family + factorization + eigen/svds) + lineax (tag-dispatched operators) + equinox filter_vmap + jax x64. Defect: prose "worker lane" (`linear.md:11`) with no `lane.offload` fence. `content_identity` import (`:44` region). Charter correct.
- **solvers/nonlinear.md** (380) — 9. Deep optimistix (17 solvers over `_SOLVER` profile table) + lineax inner + optax lift + jax pytree. Same worker-lane prose gap. Charter correct.
- **solvers/quadrature.md** (477) — 8. Deep quadax + interpax + scipy.integrate/interpolate + skfem condense/solve. quadax/interpax README-roster-absent. Worker-lane prose gap. Charter correct.
- **solvers/differential.md** (422) — 9. Deep diffrax (ODE/SDE/CDE solver+adjoint+Levy-area families) + equinox + jax. Worker-lane prose gap. Charter correct.
- **solvers/sensitivity.md** (421) — 7. Deep jax AD (value_and_grad/jacfwd/jacrev/hessian/jvp/vjp) but the `findiff` floor mines ONLY `coefficients` (`:469,477`) against a full findiff catalog. `content_identity` import. Charter should name the findiff consume as "raw central-difference `coefficients` stencil ONLY", not Diff/.matrix.
- **solvers/mesh.md** (313) — 8. skfem assemble + meshio round-trip well-mined. `content_identity` import. **Codemap-present** (`ARCH:18` mesh.py). Charter correct.
- **solvers/field.md** (285) — 7. Deep skfem (Basis/Element* families). **ABSENT from ARCHITECTURE codemap** — `ARCH:11-18` solvers list omits `field.py` though the page + `TASKLOG [INTERPAX_QUADAX_USAGE]` reference `solvers/field.md`. `content_identity` import. Charter exists but ungoverned.
- **optimization/convex.md** (396) — 9. Deep cvxpy (Variable/Parameter/cone constraints/dual_value/is_dcp) + clarabel dual-certificate. Composes `graduation.handoff` (convex_program axis) + `numerics/array` + `content_identity`. Charter correct.
- **optimization/design.md** (464) — 9. optimistix/equinox/jax/optax over an Equinox-parameterized objective reading the implicit-adjoint gradient. Worker-lane prose gap. Charter correct.
- **optimization/program.md** (308) — 8. scipy.optimize (linprog/milp/differential_evolution/global family) folding one OptimizeResult. `content_identity` import. Charter correct.
- **numerics/array.md** (255) — 9. Exemplary array-api-compat/extra/sparse mine; the substrate admission hub. `content_identity` import (`:38`). Admits `dask` as a passive backend (`:48,53`) — the sole dask consume. Charter correct; boundary "never re-catalogue xarray/dask" is honored.
- **numerics/interval.md** (352) — 9. Deep python-flint (arb ball arithmetic + arb_poly.real_roots + flint.good) + mpmath iv floor ladder. `content_identity` import. Charter correct.
- **numerics/quantity.md** (447) — 9. Deep pint (registry/Quantity/Measurement/dimensionality) + uncertainties (correlated_values/unumpy/ulinalg). Composes `graduation.handoff` unit_law. Charter correct.
- **numerics/statistics.md** (289) — 8. Deep scipy.stats (hypothesis tests + frozen dist fit family) + numpy.random. `content_identity` import. Charter correct; correctly defers grouped/labelled aggregation to data.
- **numerics/jit.md** (231) — 8. Deep numba (njit/vectorize/guvectorize/CPUDispatcher evidence) + jax (jit/make_jaxpr/tree_util). **ABSENT from ARCHITECTURE codemap** — `ARCH:28-32` numerics list omits `jit.py` though `IDEAS [SYMBOLIC_CODEGEN_BRIDGE]`/`[SPARSE_EIGENVALUE_ROUTE]` anchor `numerics/jit.md#JIT`. `content_identity` import (`:30`). Charter exists but ungoverned.
- **analysis/signal.md** (308) — 8. scipy.signal (butter/sosfiltfilt/welch/spectrogram/resample_poly) + pywt (wavedec/swt/cwt/WaveletPacket) multiresolution. `content_identity` import. Worker-lane prose gap. Charter correct.
- **analysis/transform.md** (290) — 8. scipy.fft (fft/rfft/hfft/dct/dst/fht) + array-api namespace dispatch (numpy/jax/dask spine). Charter correct.
- **analysis/symbolic.md** (697) — 10. The corpus's deepest package mine: sympy across calculus/rewrite/substitute/refine/solve/linalg/numbertheory/evaluate/lower/codegen + python-flint FLINT ground-domain (fmpq_poly/fmpq_mat/fmpz) + mpmath/arb certified evaluate. `content_identity` import. Charter exemplary.
- **analysis/spatial.md** (316) — 8. scipy.spatial (cKDTree/ConvexHull/Delaunay/Voronoi/alpha-shape). Composes `graduation.handoff` geometry case (`reconstructed-mesh` alpha-shape boundary → geometry scan). `content_identity` import. Charter correct.
- **experiments/study.md** (415) — 8. Deep SALib (full sample+analyze family) + scipy.stats.qmc + scikit-learn surrogates + numpy.polynomial. **`dask` NOT in Packages line** though README `[EXPERIMENTS_SENSITIVITY]` + `ARCH:24` claim it — study does not consume dask. Charter correct.
- **experiments/history.md** (279) — 8. numpy-only (content-keyed run persistence). `content_identity` import. Charter correct.
- **experiments/inference.md** (364) — 8 core / defect at seam. Deep pymc + arviz (correct 1.x renames) + xarray DataTree. `numpyro`/`blackjax`/`nutpie` STRING-only (`:22,50`). `content_identity` import (`:40`). Charter honest ("never imported here") — the CATALOGS lie, not the page.
- **experiments/model.md** (368) — 8. Deep onnx + onnxruntime + skl2onnx + scikit-learn + universal-pathlib UPath. Composes `graduation.handoff` model_asset + `content_identity` + `runtime.roots` ResourceRef. Charter correct.
- **graduation/handoff.md** (177) — 5. THE inversion. MINTS `GeometrySubject` as a compute `Literal` (`:61-64`) that geometry `[V2]` re-homes to a geometry mint; the 8-literal set is stale against geometry's differentiated union (`bim-compliance`/`bim-lifecycle`/`section-property`/`building-energy`/`thermal-comfort`). `content_identity` import (`:56`). Charter SHOULD be: the multi-domain `HandoffAxis` hub + `GraduationReceipt` admission fold that DECODES the geometry-minted `GeometryHandoff` carrier, owning the residual-over-ceiling `_admit` and the non-geometry axes (solver/symbolic/model_asset/array_layout/unit_law/uncertainty_law/convex_program); the `geometry` case carries the decoded subject, never re-declares the vocabulary.
- **graduation/codegen.md** (314) — 8. stdlib `ast` builder + msgspec. `content_identity` import. Charter correct (C# evidence-bundle stub emitter).

---

## [03]-[CROSS_CUTTING]

**Duplication / parallel rails.** None at the ADT level in the fences read (linear/nonlinear/array/jit/inference each collapse to one polymorphic owner). The duplication is at the CATALOG level: three sampler catalogs (numpyro/blackjax/nutpie) redundantly document the same "JAX/native NUTS backend" surface PyMC already dispatches; the correct shape is ONE contract note per engine (the `nuts_sampler` string + its `nuts_sampler_kwargs` keys), the kernel algebra a DECLINE.

**Concern mixing.** `pyproject.toml` tags cross-cut owners: `sparse [DATA]` (`:118`) but compute-consumed; `netcdf4 [COMPUTE]` (`:62`) but zero compute consumers (no `netcdf4.md` compute stub; data `[V12]` retags `[DATA]`); `dask` (`:50`) untagged. The `[COMPUTE]`/`[DATA]` boundary is drifted in the manifest.

**Hardcoding-vs-generator.** The catalogs are generator-shaped (route tables, `[STACKING]` sections). No roster-as-code violation found in the api tier. The one hardcode class is in fences (`array.md` DenseBound default `(1000,0.25)` is correctly parameterized; interval `_ULP`/`_TINY` correctly `finfo`-derived) — clean.

**Dead / thin carriers (catalog-level).** `findiff.md` (only `coefficients` live), `blackjax.md`/`numpyro.md`/`nutpie.md` (string-only), `dask.md` (passive backend). Combined ~72KB of catalog for ~5 live members.

**Unwired seams.** (a) 8 pages assert "worker lane" execution; ZERO compose `rasm.runtime.lanes.offload` (`offload(`=0). (b) `ARCH:50` `graduation/handoff ⇄ python:geometry/graph` mistyped bidirectional — geometry `[V2]` makes it decode-only `←`. (c) `ARCH:51` `numerics/array ⇄ python:runtime/transport [WIRE]: ContentIdentity` mis-homes ContentIdentity to `runtime/transport`; it lives in runtime `evidence/identity` (runtime `[V4]`/`[V7]`).

**Unmined capability (catalog anchors).** `blackjax.md:60/73/83` (smc/sgmcmc/vi — TASKLOG-dropped); `numpyro.md` SVI/effect-handler/distribution surface; `nutpie.md` compile_pymc_model/compile_stan_model/benchmark_logp; `findiff.md` Diff-algebra/Gradient/Divergence/Curl/Laplacian/.matrix/Padé/BVP-PDE; `dask.md` distributed-dataframe surface. `scipy.md` INVERSE gap: MISSING `lobpcg`/`OPinv`/`ArpackNoConvergence`/`svds` shift-invert the QUEUED `IDEAS [SPARSE_EIGENVALUE_ROUTE]` names as catalogue-unverified.

**Owner-line / roster drift.** `array-api-compat.md`/`array-api-extra.md` missing `- owner:`; README `[02]` missing `findiff`/`quadax`/`interpax`; README `[03]` lists unused `pydantic`/`anyio`, omits consumed `universal-pathlib`; `xarray.md` owner line lacks compute co-consumer.

---

## [04]-[MIGRATION_PRESSURES] (upstream briefs change surfaces this folder assumes)

1. **`content_identity` rename (runtime `[V4]`).** 20 files / 21 sites import `from rasm.runtime.content_identity import …`. Runtime renames the module `rasm.runtime.identity`. The whole folder is unimportable the moment runtime lands. Mechanical corpus-wide sweep (parallels geometry E4 12-site, data E5 22-site). ContentIdentity additionally SPLITS (runtime `[V7]`): `IdentitySource.lift`/`ParityReceipt`/reproduction move to `evidence/reproduction`; `array.md`'s `ARRAY_LAYOUT_GRADUATION` cross-backend bit-identity proof (TASKLOG) must ride the runtime `reproduction` `ParityReceipt`, not `evidence/identity`.
2. **Graduation inversion (geometry `[V2]`).** Geometry now MINTS `GeometrySubject`/`GeometryHandoff`; geometry's 11 `from rasm.compute.graduation.handoff` imports DIE. Compute's `handoff.md` must FLIP from vocabulary-owner to DECODER: stop declaring `type GeometrySubject = Literal[…]` (`:61-64`), decode the geometry-minted carrier at the seam, and adopt geometry's differentiated union (`numerical-primitive` splits into `bim-compliance`/`bim-lifecycle`/`section-property`; adds `building-energy`/`thermal-comfort`). The frozen contract geometry pins: `graduates(owner, HandoffAxis(geometry=subject), key, measured, ceiling)` + residual-over-ceiling `_admit`. `ARCH:50` `⇄`→`←`. NB compute still PRODUCES `scan-deviation`/`reconstructed-mesh(non-mesh boundary)` from `analysis/spatial` — that producer half stays, but over geometry's vocabulary.
3. **`dask.md` relocation (data `[V12]` `DASK_CATALOG_REHOME`).** Strike `compute/.api/dask.md`; author branch-tier `libs/python/.api/dask.md` with data + compute on the owner line. Travels to compute as a Ripple per `RASM-DATA-BRIEF.md [06]`.
4. **`sparse` retag (data `[V12]`).** `pyproject.toml:118` `sparse [DATA]` → `[COMPUTE]` (live consumer `array.md`). Catalog already correctly compute-tier.
5. **`xarray` co-consumer (data `[V12]`).** `xarray.md` stays branch-tier; owner line gains compute (`inference.md:45` DataTree). Data-owned edit; compute is the named co-consumer.
6. **Worker-lane offload demand (runtime `[V5]`).** Runtime's `offload` isolation-modality axis (interpreter|process|thread) exists and names geometry+data as demanding consumers. Compute is an UNNAMED demanding consumer: JAX x64 (`jax.config.update` is process-global — concurrent in-process JAX solves corrupt the flag), heavy scipy factorizations, `pymc` sampling, and `cvxpy` conic solves are exactly the process/interpreter-isolated CPU-bound workload. The 8 "worker lane" prose claims demand it. Waterfall pressure: runtime `[V5]` should name compute beside geometry/data on the offload/THREAD-band exports (recorded here, not edited — lane is read-only).

---

## [05]-[DOMAIN_GAPS] (roster-addition candidates — named, not researched)

- **Sparse-eigen shift-invert surface.** `scipy.md` lacks `scipy.sparse.linalg.lobpcg`/`OPinv`/`ArpackNoConvergence` and full `svds` — the QUEUED `IDEAS [SPARSE_EIGENVALUE_ROUTE]` names them catalogue-unverified. Gap is in the EXISTING scipy catalog (add rows), not a new package.
- **No genuine roster hole found elsewhere.** The geometry brief's categorical-best supersession sweep discipline applied here returns ZERO strikes-for-replacement among the deep-mined science packages — the roster is complete for its charter (validated numerics, convex, DOE, Bayesian, ONNX assets, symbolic, signal, FEM). The `MCERP_SOERP_REDTEAM` dropped card already closed the one candidate gap (second-order error propagation) in favor of the `uncertainties` first-order owner. `findiff` is the one admission whose value is marginal (single-member) — the gap question is whether the FD floor warrants a full package at all vs `numpy.gradient`, but `sensitivity.md` uses `findiff.coefficients` for arbitrary-accuracy central stencils numpy lacks, so it stays (narrowed catalog, not struck).

---

## [06]-[VERDICT_CANDIDATES] (campaign-defining structural rulings, evidence-anchored)

1. **REDUCE the three sampler catalogs to string-backend contract stubs.** `numpyro.md`/`blackjax.md`/`nutpie.md` (~1300 catalog LOC) collapse to the `nuts_sampler` name + per-engine `nuts_sampler_kwargs` keys (`nutpie backend='jax'/'numba'`, `numpyro chain_method`) with the kernel/SMC/SGMCMC/VI surface a stated DECLINE. Evidence: `inference.md:22` ("never imported here"), `inference.md:50` (`Literal[…]`), `blackjax.md:60,73,83` (dropped forms), `TASKLOG [BLACKJAX_RAW_ALGEBRA]-[DROPPED]`.
2. **STRIKE + RELOCATE `dask.md`; TAG the manifest.** Strike `compute/.api/dask.md`; the catalog lands branch-tier per data `[V12] DASK_CATALOG_REHOME`; `pyproject.toml:50` gains a `# [COMPUTE]:` tag (or moves to the data tag if the branch relocation carries it). Evidence: `array.md:48,53` (passive backend only), `study.md` Packages (no dask), `RASM-DATA-BRIEF.md [V12]/[06]`, `pyproject.toml:50` untagged.
3. **NARROW `findiff.md` to the `coefficients` floor.** Reduce the catalog to the raw central-difference `coefficients(deriv, acc)` surface; DECLINE the Diff-algebra/vector-calculus/matrix/PDE surface; correct the charter (it claims `Diff`/`.matrix` composition the fence never performs); add `findiff` to README `[02]`. Evidence: `sensitivity.md:469,477`, `findiff.md` charter line, README `[02]` absence.
4. **FLIP `handoff.md` from vocabulary-owner to decoder (graduation inversion).** Stop minting `GeometrySubject` (`:61-64`); decode the geometry-`[V2]`-minted carrier; adopt the differentiated union; retype `ARCH:50` `⇄`→`←`. The compute-owned axes (solver/symbolic/model_asset/array_layout/unit_law/uncertainty_law/convex_program) and the residual-over-ceiling `_admit` stay. Evidence: `handoff.md:20-21,61-64,206`, `RASM-GEOMETRY-BRIEF.md [V2]` + E3, `ARCH:50`.
5. **SWEEP the `content_identity`→`identity` rename + reproduction split corpus-wide.** 20 files / 21 sites; additionally rebind the `ARRAY_LAYOUT_GRADUATION` parity proof onto runtime `evidence/reproduction ParityReceipt`. Evidence: 21 import sites, `RASM-RUNTIME-BRIEF.md [V4]/[V7]`, `TASKLOG [ARRAY_LAYOUT_GRADUATION]`.
6. **WIRE the worker lane or delete the prose (illusory concurrency).** Either compute's heavy solver/study/optimization entries compose `rasm.runtime.lanes.offload` (with the JAX process/interpreter isolation the x64 global flag demands), OR the 8 "worker lane" prose claims delete. This is the folder's largest capability-vs-prose gap. Evidence: `offload(`=0 corpus-wide, 8 "worker lane" pages, runtime `[V5]` offload axis exists. Pairs with the runtime `[V5]` waterfall (name compute a demanding consumer).
7. **GOVERN the two codemap-orphan owners.** `solvers/field.py` and `numerics/jit.py` exist as design pages (285 + 231 LOC) referenced by TASKLOG/IDEAS but are ABSENT from `ARCHITECTURE.md [01]` codemap (`ARCH:11-18`, `:28-32`). Add both nodes; `ARCH:[02]` gains their seams. Evidence: `ARCH` codemap vs disk, `IDEAS [SYMBOLIC_CODEGEN_BRIDGE]`/`TASKLOG [INTERPAX_QUADAX_USAGE]`.
8. **RECONCILE roster/owner-line drift.** README `[02]` add `findiff`/`quadax`/`interpax`; README `[03]` strike unused `pydantic`/`anyio`, add `universal-pathlib`; `array-api-compat.md`/`array-api-extra.md` gain `- owner: compute`; `xarray.md` owner line gains compute. Evidence: Packages-line vs README diff, `pydantic`/`anyio` files=[], `array-api-*.md` missing owner line.
