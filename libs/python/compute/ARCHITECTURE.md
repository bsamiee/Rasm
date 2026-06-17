# [PY_COMPUTE_ARCHITECTURE]

`compute` is one offline-evidence surface: every concern is an axis owner with closed cases, one numeric-intent solver discriminates every route, and every useful experiment graduates through one receipt rail into the managed owner system. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the implementation source tree, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, the package boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The planned module layout IS the build order: each file is one transcription unit, array admission before the solver that consumes it, the quantity/study/model owners before the graduation receipt that moves their evidence outward. Each leaf is annotated with the owners it transcribes and the owning page#cluster.

```text codemap
compute/
├── arrays.py            # ArrayPayload, NamedAxis — array-solver#ARRAY
├── solver.py            # NumericIntent, SolverReceipt, Differentiation, IntervalNumerics, Signal, SymbolicDerivation — array-solver#SOLVER, #DIFFERENTIATION, #RIGOR, #SIGNAL
├── units_study.py       # UncertainQuantity, Study, RunHistory, ModelAsset — units-study#QUANTITY, #STUDY, #MODEL
├── graduation.py        # GraduationReceipt, HandoffAxis, StubCodegen — graduation#GRADUATION, #CODEGEN
└── inference.py         # Inference, InferenceReceipt — graduation#INFERENCE
```

`arrays.py` lands first: `ArrayPayload` admits dtype/shape/named-axes/finite-policy/layout/chunking/identity over numpy and composes the data-branch labelled-array shapes. `solver.py` follows with `NumericIntent` — one solver owner discriminating by route (dense-LA, sparse, eigen, nonlinear, integrate, interpolate, symbolic, FEM) with the numba and jax accelerator rows on the same owner — plus `SolverReceipt` (the one method-discriminated solve receipt), `Differentiation` (one VJP/sensitivity surface with a finite-difference floor), `IntervalNumerics` (validated/interval numerics), `Signal` (one DSP surface), and `SymbolicDerivation` (lambdify codegen). `units_study.py` holds `UncertainQuantity` (correlated uncertainty threaded through the pint unit algebra), `Study`/`RunHistory` (the study-lake and experiment-run spine with `BenchmarkData` as a measurement-mode discriminant), and `ModelAsset`. `graduation.py` lands after the evidence owners: `GraduationReceipt` and `HandoffAxis` move useful Python evidence outward and `StubCodegen` emits typed stubs from the decoded evidence-bundle shape. `inference.py` lands last as the Bayesian owner over a prior/likelihood/posterior axis.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual wheel-floor/deploy-asset/seam probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]         | [OWNER]              | [KIND]                                  | [CASES]                                                             | [PAGE#CLUSTER]              |  [STATE]  |
| :-----: | :------------------ | :------------------- | :-------------------------------------- | :----------------------------------------------------------------- | :------------------------- | :-------: |
|   [1]   | array admission     | `ArrayPayload`       | frozen owner + `NamedAxis`              | dtype/shape/named-axes/finite/layout/chunking                      | array-solver#ARRAY         |   SPIKE   |
|   [2]   | numeric intent      | `NumericIntent`      | tagged union                            | dense-LA/sparse/eigen/nonlinear/integrate/interpolate/symbolic/fem | array-solver#SOLVER        |   SPIKE   |
|   [3]   | solver receipt      | `SolverReceipt`      | tagged union                            | direct/iterative/least-squares/eigen folded over every route       | array-solver#SOLVER        |   SPIKE   |
|   [4]   | accelerator rows    | `NumericIntent`      | row on the owner                        | numba LLVM JIT, jax XLA jit/grad/vmap                              | array-solver#SOLVER        |   SPIKE   |
|   [5]   | differentiation     | `Differentiation`    | tagged union `DiffMode`                 | reverse-vjp (gated) / finite-difference (numpy floor)              | array-solver#DIFFERENTIATION |  SPIKE   |
|   [6]   | validated numerics  | `IntervalNumerics`   | frozen owner + `IntervalOp`             | evaluate/certify/refine — Arb ball (gated) / nextafter floor       | array-solver#RIGOR         |   SPIKE   |
|   [7]   | signal processing   | `Signal`             | tagged union `SignalOp`                 | filter/spectral/resample (gated)                                  | array-solver#SIGNAL        |   SPIKE   |
|   [8]   | symbolic derivation | `SymbolicDerivation` | static surface                          | lambdify codegen, graduation handoff                              | array-solver#SOLVER        | FINALIZED |
|   [9]   | quantity claim      | `UncertainQuantity`  | frozen owner + `PropagationMode`        | scalar/correlated/expression UFloat over pint Measurement          | units-study#QUANTITY       |   SPIKE   |
|  [10]   | study lake          | `Study`              | tagged-union method axis + `RunHistory` | LHS/factorial/Sobol/Halton · Morris/Sobol-indices · poly/GP        | units-study#STUDY          |   SPIKE   |
|  [11]   | model asset         | `ModelAsset`         | frozen owner + manifest                 | file identity/checksum/io-names/validation                        | units-study#MODEL          |   SPIKE   |
|  [12]   | graduation receipt  | `GraduationReceipt`  | tagged union + `HandoffAxis`            | solver/symbolic/model/array/unit/uncertainty/geometry             | graduation#GRADUATION      | FINALIZED |
|  [13]   | bayesian inference  | `Inference`          | static owner + `InferenceReceipt`       | prior/likelihood/sampler axis · r-hat/ess diagnostics             | graduation#INFERENCE       |   SPIKE   |
|  [14]   | stub codegen        | `StubCodegen`        | static owner + `ast` builder            | evidence-bundle shape · per-field-kind annotation fold            | graduation#CODEGEN         | FINALIZED |

One rail per entrypoint: the solver returns one `SolverReceipt`, the study returns through `RunHistory`, every useful experiment graduates through one `GraduationReceipt`. Classical statistics/ML is in-scope; generative/deep-learning is never in-scope — the differentiation and signal owners state that boundary. A new route or accelerator is a case or row, never a second solver owner.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PACKAGE]   | [MAY_REFERENCE_COMPUTE] | [COMPUTE_MAY_REFERENCE] | [BOUNDARY]                                       |
| :-----: | :---------- | :---------------------: | :---------------------: | :----------------------------------------------- |
|   [1]   | `runtime`   |           no            |           yes           | content key and receipt port consumed inward     |
|   [2]   | `data`      |           no            |           yes           | compute composes data array/dataset shapes       |
|   [3]   | `geometry`  |           no            |           yes           | geometry evidence graduates through `HandoffAxis` |
|   [4]   | `artifacts` |           no            |           no            | artifact production stays at `artifacts`          |

`compute` consumes runtime `ContentIdentity` and `ReceiptContributor`, composes data dataset/array shapes as study inputs without re-cataloguing them, and accepts geometry evidence through the graduation `HandoffAxis` geometry case. Graduation moves offline evidence into the managed owner system; the cross-boundary graduation seam and the symbolic/model handoff consequence ride the Tier-0 `region-map/seam-splits.md`.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named compute cluster, consequences land at the consumer. Intra-Python seams ride `pkg/page#CLUSTER`; cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as a Tier-0 seam, never restated here.

| [INDEX] | [SEAM]              | [MECHANICS_AT]                    | [CONSEQUENCE_AT]                                                          |
| :-----: | :------------------ | :-------------------------------- | :----------------------------------------------------------------------- |
|   [1]   | content identity    | runtime/content-identity#IDENTITY | model-asset and study artifacts key by one `ContentIdentity`             |
|   [2]   | graduation receipt  | graduation#GRADUATION             | runtime/observability#RECEIPT wires `GraduationReceipt` through `ReceiptContributor` |
|   [3]   | array shape input   | array-solver#ARRAY                | data/columnar-query#DATASET dataset/array shapes compose as study inputs |
|   [4]   | geometry handoff    | graduation#GRADUATION             | geometry/scan-processing#REGISTRATION and geometry/geometry-algebra#ALGEBRA evidence graduates through the `HandoffAxis` geometry case |
|   [5]   | inference diagnostics | graduation#INFERENCE             | the posterior convergence diagnostics gate the uncertainty-law handoff at graduation#GRADUATION |

## [5]-[BOUNDARIES]

- `compute` is not a production compute runtime, benchmark authority, substrate selector, tensor-session owner, or product-receipt owner; it owns offline evidence that graduates.
- Columnar and labelled-array interchange ownership stays in `data` (compute composes the shape); geometry tessellation/registration/topology stays in `geometry` and graduates through the geometry `HandoffAxis` case.
- Statement carve-outs are named per fence: the route-dispatch acceptor on `NumericIntent`, the study-method fold on `Study`, the evidence-bundle decode in `StubCodegen`, and the graduation acceptor on `GraduationReceipt` are the boundary capsules; every other member stays expression-shaped.
- `Inference` is bounded at gradient-MCMC over an explicit prior/likelihood graph; deep-generative, variational, and neural-posterior estimation never enter it.
- Successful Python experiments graduate as managed owner rows, not permanent product execution surfaces; the cross-boundary graduation seam rides the Tier-0 ledger.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER mint a managed `ComputeReceipt`, benchmark authority, substrate selection, or production tensor session; compute graduates offline evidence only.
- NEVER add a second solver owner beside `NumericIntent`; a new route or accelerator is a case or row.
- NEVER re-catalogue `xarray`/`dask`; compute composes the data-branch catalogues as study-input shapes.
- NEVER split `BenchmarkData` into a parallel benchmark owner; it is a measurement-mode discriminant on the study receipt.
- NEVER add a parallel experiment-tracker beside `RunHistory`; it rides the study spine.
- NEVER add a second Bayesian owner beside `Inference`; a new distribution is a `Prior`/`Likelihood` case, a new step method is a `SamplerKind` case, and the diagnostics are `InferenceReceipt` fields.
- NEVER admit deep-generative / variational / neural-posterior estimation into `Inference`; it is bounded at gradient-MCMC over an explicit prior/likelihood graph.
- NEVER re-mint the graduation-evidence bundle shape in `StubCodegen`; it decodes the sealed bundle once at the offline seam and emits stubs, importing nothing from a managed interior; a new managed field kind is one `FieldKind` literal and one match arm.
