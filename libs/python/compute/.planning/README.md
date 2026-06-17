# [PY_COMPUTE_PLANNING]

`compute` owns offline scientific evidence that graduates into C# owner rows: array admission, one polymorphic numeric-intent solver/symbolic dispatch with accelerator rows, units + uncertainty claims, study + experiment-run orchestration, model-asset validation, and the C# graduation receipt with a geometry handoff case. It has zero consumers today; implementation is full-capability. It is not the production compute runtime — Python owns only offline evidence that graduates.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                          | [OWNS]                                                             | [STATE]   |
| :-----: | :------------------------------ | :----------------------------------------------------------------- | :-------- |
|   [1]   | [array-solver](array-solver.md) | array admission, the numeric-intent solver, symbolic, accelerators | finalized |
|   [2]   | [units-study](units-study.md)   | units/uncertainty claims, study + run-history, model assets        | finalized |
|   [3]   | [graduation](graduation.md)     | the C# graduation receipt with the geometry handoff case           | finalized |

## [2]-[CATALOGUE_PENDING]

- `numba`, `jax`, `onnx`, `onnxruntime`, `scikit-learn` carry a `python_version<'3.15'` marker (no cp315 wheel); the project `>=3.15` venv prunes them. Their fence members verify out-of-band on the marker floor (suite TASKLOG `PY_API_003`).
- `pymc`, `arviz` carry no explicit marker but are DEPLOY-ASSET-GATED on cp315 by their scientific transitive deps: `pymc`->`pytensor`->`numba`->`llvmlite` (no cp315 wheel, source build fails) and `arviz`->`arviz-stats`->`scipy` (no cp315 wheel, Fortran-toolchain source build). The `Inference` fence is a documented-API transcription verified-by-stability on the marker floor (suite TASKLOG `PY_INFER_001`), the same deploy-asset-gate posture as the accelerator rows.
- `numpy`, `scipy`, `pint`, `uncertainties` carry no cp315 wheel; `sympy` is cp315-installed and reflected. `xarray`/`dask` are composed from the data-branch catalogues, never re-catalogued.
- `scikit-fem` (FEM assemble/solve), `python-flint` (Arb validated/interval numerics), and `optimistix` (JAX implicit-adjoint VJP) are DEPLOY-ASSET-GATED on cp315 (scikit-fem -> scipy; python-flint -> FLINT/Arb C toolchain; optimistix -> jax/jaxlib, no cp315 wheel). The array-solver `#SOLVER`/`#DIFFERENTIATION`/`#RIGOR`/`#SIGNAL` fences are documented-API transcriptions verified-by-stability on the marker floor; each pairs with a numpy-floor branch (`_dense_receipt`, `_finite_difference_jacobian`, `_floor_enclosure`) that runs on cp315.

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis. `NumericIntent` is ONE route-discriminated solver owner subsuming the former standalone `SolverPlan`; the `numba`/`jax` accelerator rows ride the same owner, never parallel methods. `SolverReceipt` is the ONE method-discriminated solve receipt (direct/iterative/least-squares/eigen) folded across the scipy.sparse.linalg + scikit-fem paths — never a receipt-type per solver. `Differentiation` (one `DiffMode` VJP owner), `IntervalNumerics` (certified-enclosure validated numerics), and `Signal` (one `SignalOp` DSP owner) are deploy-asset-gated owners: each pairs a documented-API gated body (jax/optimistix, python-flint Arb, scipy.signal) with a numpy-floor branch that runs unconditionally on cp315, and each states the classical/generative boundary (no learned filters, no model training). `UncertainQuantity` threads correlated `uncertainties.UFloat` magnitudes THROUGH the pint unit algebra via the native `pint.Measurement` bridge — one owner discriminated by a `PropagationMode` axis, no parallel uncertain-type. `Study` is ONE study-lake owner over a `StudyMethod` axis (DOE sampling / sensitivity / surrogate as cases); `BenchmarkData` collapses into the study receipt with a measurement-mode discriminant and `RunHistory` rides the same study spine. `Inference` is ONE Bayesian owner over a prior/likelihood/sampler axis — a new distribution is a `Prior`/`Likelihood` case, never a parallel sampler; its diagnostics (r-hat/ess) are receipt fields, not a parallel diagnostic surface. `StubCodegen` is ONE generator folding the C# evidence-bundle field kinds by `match`, never a sibling emitter per kind. `[STATE]` carries `SPIKE` on the marker-floor probe and `LIVE` where the distribution reflects on the active env: `UncertainQuantity` is `LIVE` (pint/uncertainties installed and reflected) and `Study` is `SPIKE/LIVE` (numpy LHS/factorial/Morris/polynomial bodies live; the Sobol/Halton/Sobol-indices/GP rows route to deploy-gated scipy/sklearn). The `Inference` shape is settled but `pymc`/`arviz` are deploy-asset-gated on cp315 (their scientific transitive deps carry no cp315 wheel), so the row is `SPIKE` on the floor, FINALIZED on shape.

| [INDEX] | [AXIS/CONCERN]      | [OWNER]              | [KIND]                                  | [CASES]                                                              |  [STATE]   |
| :-----: | :------------------ | :------------------- | :-------------------------------------- | :------------------------------------------------------------------- | :--------: |
|   [1]   | Array admission     | `ArrayPayload`       | frozen owner + `NamedAxis`              | dtype/shape/named-axes/finite/layout/chunking                        |   SPIKE    |
|   [2]   | Numeric intent      | `NumericIntent`      | tagged union                            | dense-LA/sparse/eigen/nonlinear/integrate/interpolate/symbolic/fem   |   SPIKE    |
|  [2a]   | Solver receipt      | `SolverReceipt`      | tagged union                            | direct/iterative/least-squares/eigen — folded over every route       |   SPIKE    |
|  [2b]   | Differentiation     | `Differentiation`    | tagged union `DiffMode`                 | reverse-vjp (jax/optimistix gated) / finite-difference (numpy floor) |   SPIKE    |
|  [2c]   | Validated numerics  | `IntervalNumerics`   | frozen owner + `IntervalOp`             | evaluate/certify/refine — Arb ball (flint gated) / nextafter floor   |   SPIKE    |
|  [2d]   | Signal processing   | `Signal`             | tagged union `SignalOp`                 | filter/spectral/resample — scipy.signal gated                        |   SPIKE    |
|   [3]   | Symbolic derivation | `SymbolicDerivation` | static surface                          | lambdify codegen, C# handoff                                         | FINALIZED  |
|   [4]   | Accelerator rows    | `NumericIntent`      | row on the owner                        | numba LLVM JIT, jax XLA jit/grad/vmap                                |   SPIKE    |
|   [5]   | Quantity claim      | `UncertainQuantity`  | frozen owner + `PropagationMode`        | scalar/correlated/expression UFloat over pint Measurement            |    LIVE    |
|   [6]   | Study lake          | `Study`              | tagged-union method axis + `RunHistory` | LHS/factorial/Sobol/Halton · Morris/Sobol-indices · poly/GP          | SPIKE/LIVE |
|   [7]   | Model asset         | `ModelAsset`         | frozen owner + manifest                 | file identity/checksum/io-names/validation                           |   SPIKE    |
|   [8]   | Graduation receipt  | `GraduationReceipt`  | tagged union + `HandoffAxis`            | solver/symbolic/model/array/unit/uncertainty/geometry                | FINALIZED  |
|   [9]   | Bayesian inference  | `Inference`          | static owner + `InferenceReceipt`       | prior/likelihood/sampler axis · r-hat/ess diagnostics                |   SPIKE    |
|  [10]   | Stub codegen        | `StubCodegen`        | static owner + `ast` builder            | C# evidence-bundle shape · per-field-kind annotation fold            | FINALIZED  |

## [4]-[BUILD_ORDER]

| [INDEX] | [FILE]           | [TRANSCRIBES]                        | [GATE]         |
| :-----: | :--------------- | :----------------------------------- | :------------- |
|   [1]   | `arrays.py`      | array-solver#ARRAY                   | static + floor |
|   [2]   | `solver.py`      | array-solver#SOLVER                  | static + floor |
|   [3]   | `units_study.py` | units-study#QUANTITY, #STUDY, #MODEL | static + floor |
|   [4]   | `graduation.py`  | graduation#GRADUATION, #CODEGEN      | static         |
|   [5]   | `inference.py`   | graduation#INFERENCE                 | static + floor |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]            | [EVIDENCE]                                          |
| :----: | :---------------- | :-------------------------------------------------- |
|  [G1]  | `uv lock --check` | compute pins resolve against the root manifest      |
|  [G2]  | `.api` catalogue  | every fence member resolves to an `.api` row        |
|  [G3]  | wheel floor       | cp315/marker-floor wheels install before re-reflect |

## [6]-[PROHIBITIONS]

- [NEVER] mint a C# `ComputeReceipt`, benchmark authority, substrate selection, or production tensor session; compute graduates offline evidence only.
- [NEVER] add a second solver owner beside `NumericIntent`; a new route or accelerator is a case or row.
- [NEVER] re-catalogue `xarray`/`dask`; compute composes the data-branch catalogues as study-input shapes.
- [NEVER] split `BenchmarkData` into a parallel benchmark owner; it is a measurement-mode discriminant on the study receipt.
- [NEVER] add a parallel experiment-tracker beside `RunHistory`; it rides the study spine.
- [NEVER] add a second Bayesian owner beside `Inference`; a new distribution is a `Prior`/`Likelihood` case, a new step method is a `SamplerKind` case, and the diagnostics are `InferenceReceipt` fields, never a parallel sampler or diagnostic surface.
- [NEVER] admit deep-generative / variational / neural-posterior estimation into `Inference`; the charter amendment bounds it at gradient-MCMC over an explicit prior/likelihood graph.
- [NEVER] re-mint the C# graduation-evidence bundle shape in `StubCodegen`; it decodes the sealed bundle once at the offline seam and emits stubs, importing nothing from a C# interior; a new C# field kind is one `FieldKind` literal and one match arm.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                            | [PAGE]       | [CATALOGUE]                                               | [STATUS]           |
| :-----: | :----------------------------------- | :----------- | :-------------------------------------------------------- | :----------------- |
|   [1]   | numpy                                | array-solver | api-numpy.md                                              | catalogue-pending  |
|   [2]   | scipy, sympy                         | array-solver | api-scipy.md, api-sympy.md                                | catalogue-pending  |
|   [3]   | numba, jax                           | array-solver | api-numba.md, api-jax.md                                  | catalogue-pending  |
|  [3a]   | scikit-fem, python-flint, optimistix | array-solver | api-scikit-fem.md, api-python-flint.md, api-optimistix.md | deploy-asset-gated |
|   [4]   | pint, uncertainties                  | units-study  | api-pint.md, api-uncertainties.md                         | catalogue-pending  |
|   [5]   | onnx, onnxruntime, scikit-learn      | units-study  | api-onnx.md ...                                           | catalogue-pending  |
|   [6]   | pymc, arviz                          | graduation   | api-pymc.md, api-arviz.md                                 | catalogue-pending  |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. Every `SPIKE` owner finalizes once its distribution installs on the marker floor and `assay api query` fills the catalogue; the `sympy` lambdify-to-C# codegen handoff and the geometry `HandoffAxis` case are proven against the C# owner-row contract; the `Inference` `pymc`/`arviz` member spellings re-reflect once `llvmlite`/`scipy` resolve a cp315 wheel and `arviz.rhat`/`arviz.ess` confirm the diagnostic projection; `StubCodegen` proves the `EvidenceBundle` decode against a real C# graduation-evidence sample at the seam. The bar: any offline scientific study a flagship app runs — a solver comparison, a symbolic derivation graduated to a C# kernel, a Bayesian posterior whose convergence diagnostics gate the uncertainty-law handoff, a model-asset round-trip validated before graduation — is buildable from these pages alone, graduating through the one receipt rail.
