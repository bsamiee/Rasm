# [PY_COMPUTE_PLANNING]

`compute` owns offline scientific evidence that graduates into C# owner rows: array admission, one polymorphic numeric-intent solver/symbolic dispatch with accelerator rows, units + uncertainty claims, study + experiment-run orchestration, model-asset validation, and the C# graduation receipt with a geometry handoff case. It has zero consumers today; implementation is full-capability. It is not the production compute runtime — Python owns only offline evidence that graduates.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                          | [OWNS]                                                        | [STATE]   |
| :-----: | :------------------------------ | :----------------------------------------------------------- | :-------- |
|   [1]   | [array-solver](array-solver.md) | array admission, the numeric-intent solver, symbolic, accelerators | finalized |
|   [2]   | [units-study](units-study.md)   | units/uncertainty claims, study + run-history, model assets  | finalized   |
|   [3]   | [graduation](graduation.md)     | the C# graduation receipt with the geometry handoff case     | finalized   |

## [2]-[CATALOGUE_PENDING]

- `numba`, `jax`, `onnx`, `onnxruntime`, `scikit-learn` carry a `python_version<'3.15'` marker (no cp315 wheel); the project `>=3.15` venv prunes them. Their fence members verify out-of-band on the marker floor (suite TASKLOG `PY_API_003`).
- `numpy`, `scipy`, `pint`, `uncertainties` carry no cp315 wheel; `sympy` is cp315-installed and reflected. `xarray`/`dask` are composed from the data-branch catalogues, never re-catalogued.

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis. `NumericIntent` is ONE route-discriminated solver owner subsuming the former standalone `SolverPlan`; the `numba`/`jax` accelerator rows ride the same owner, never parallel methods. `BenchmarkData` collapses into the study receipt with a measurement-mode discriminant; `RunHistory` rides the same study spine. `[STATE]` carries `SPIKE` on the marker-floor probe.

| [INDEX] | [AXIS/CONCERN]      | [OWNER]              | [KIND]                  | [CASES]                                        |  [STATE]  |
| :-----: | :------------------ | :------------------ | :---------------------- | :--------------------------------------------- | :-------: |
|   [1]   | Array admission     | `ArrayPayload`      | frozen owner + `NamedAxis` | dtype/shape/named-axes/finite/layout/chunking |   SPIKE   |
|   [2]   | Numeric intent      | `NumericIntent`     | tagged union            | dense-LA/sparse/nonlinear/integrate/interpolate/symbolic |   SPIKE   |
|   [3]   | Symbolic derivation | `SymbolicDerivation`| static surface          | lambdify codegen, C# handoff                   | FINALIZED |
|   [4]   | Accelerator rows    | `NumericIntent`     | row on the owner        | numba LLVM JIT, jax XLA jit/grad/vmap          |   SPIKE   |
|   [5]   | Quantity claim      | `QuantityClaim`     | value object pair       | unit-expr/conversion-path + nominal/stddev     |   SPIKE   |
|   [6]   | Study plan          | `StudyPlan`         | frozen owner + `RunHistory` | param-axis/sample-grid/route/measurement-mode |   SPIKE   |
|   [7]   | Model asset         | `ModelAsset`        | frozen owner + manifest | file identity/checksum/io-names/validation     |   SPIKE   |
|   [8]   | Graduation receipt  | `GraduationReceipt` | tagged union + `HandoffAxis` | solver/symbolic/model/array/unit/uncertainty/geometry | FINALIZED |

## [4]-[BUILD_ORDER]

| [INDEX] | [FILE]              | [TRANSCRIBES]                          | [GATE]         |
| :-----: | :------------------ | :------------------------------------- | :------------- |
|   [1]   | `arrays.py`         | array-solver#ARRAY                     | static + floor |
|   [2]   | `solver.py`         | array-solver#SOLVER                    | static + floor |
|   [3]   | `units_study.py`    | units-study#QUANTITY, #STUDY, #MODEL   | static + floor |
|   [4]   | `graduation.py`     | graduation#GRADUATION                  | static         |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]                       | [EVIDENCE]                                          |
| :----: | :--------------------------- | :------------------------------------------------- |
|  [G1]  | `uv lock --check`            | compute pins resolve against the root manifest     |
|  [G2]  | `.api` catalogue             | every fence member resolves to an `.api` row       |
|  [G3]  | wheel floor                  | cp315/marker-floor wheels install before re-reflect |

## [6]-[PROHIBITIONS]

- [NEVER] mint a C# `ComputeReceipt`, benchmark authority, substrate selection, or production tensor session; compute graduates offline evidence only.
- [NEVER] add a second solver owner beside `NumericIntent`; a new route or accelerator is a case or row.
- [NEVER] re-catalogue `xarray`/`dask`; compute composes the data-branch catalogues as study-input shapes.
- [NEVER] split `BenchmarkData` into a parallel benchmark owner; it is a measurement-mode discriminant on the study receipt.
- [NEVER] add a parallel experiment-tracker beside `RunHistory`; it rides the study spine.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                      | [PAGE]        | [CATALOGUE]              | [STATUS]          |
| :-----: | :----------------------------- | :------------ | :----------------------- | :---------------- |
|   [1]   | numpy                          | array-solver  | api-numpy.md             | catalogue-pending |
|   [2]   | scipy, sympy                   | array-solver  | api-scipy.md, api-sympy.md | catalogue-pending |
|   [3]   | numba, jax                     | array-solver  | api-numba.md, api-jax.md | catalogue-pending |
|   [4]   | pint, uncertainties            | units-study   | api-pint.md, api-uncertainties.md | catalogue-pending |
|   [5]   | onnx, onnxruntime, scikit-learn | units-study   | api-onnx.md ...          | catalogue-pending |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. Every `SPIKE` owner finalizes once its distribution installs on the marker floor and `assay api query` fills the catalogue; the `sympy` lambdify-to-C# codegen handoff and the geometry `HandoffAxis` case are proven against the C# owner-row contract. The bar: any offline scientific study a flagship app runs — a solver comparison, a symbolic derivation graduated to a C# kernel, a model-asset round-trip validated before graduation — is buildable from these pages alone, graduating through the one receipt rail.
