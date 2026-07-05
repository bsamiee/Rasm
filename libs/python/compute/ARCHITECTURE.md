# [PY_COMPUTE_ARCHITECTURE]

The domain map of `compute` — the host-free offline scientific-evidence package that graduates through one rail. Independent numeric-science sub-domains (`solvers`, `optimization`, `experiments`, `numerics`, `analysis`, `graduation`) meeting only through the one solve receipt, the one study spine, and the one graduation rail.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
compute/
├── solvers/                   # Unified solve (4 routes + receipt) plus autodiff sensitivity, weak-form mesh assembly, and DiscreteField/grid readout
│   ├── receipt.py             # SolverReceipt — method-discriminated solve receipt over every route
│   ├── linear.py              # LinearIntent — dense/sparse/eigen over scipy + Lineax autodiff operator tier
│   ├── nonlinear.py           # NonlinearIntent — root/minimise/fixed-point/least-squares over Optimistix + numba row
│   ├── quadrature.py          # QuadratureIntent — 1-D quadrature, spline interpolation, weak-form FEM fold
│   ├── differential.py        # DifferentialIntent — ODE/SDE/CDE integration over Diffrax with adjoint-differentiable solves
│   ├── sensitivity.py         # Differentiation — reverse-mode adjoint + implicit-adjoint solver loop + finite-difference floor
│   ├── mesh.py                # MeshField — mesh topology, per-node/per-cell fields, skfem assemble fold, meshio interchange
│   └── field.py               # FieldQuery — interpolate/project/resample readout over skfem DiscreteField + interpax grid folding FieldReceipt
├── optimization/              # By problem structure — differentiable design, constrained/discrete programs, certified convex programs
│   ├── design.py              # DesignProblem — Optimistix minimise/least-squares over an Equinox-parameterized objective reading implicit-adjoint gradient
│   ├── program.py             # ProgramIntent — linear/integer/global/constrained/assignment programs over scipy.optimize folding one OptimizeResult receipt
│   └── convex.py              # ConvexProgram — cvxpy disciplined-convex programs over a conic backend folding dual-certificate proof of optimality
├── experiments/               # Design-of-experiments study, content-keyed run history, Bayesian inference, classical ML model assets
│   ├── study.py               # Study — DOE sampling, SALib sensitivity, surrogate fitting, benchmark discriminant
│   ├── history.py             # RunHistory — content-key-keyed run persistence, partial-cell resume, run comparison
│   ├── inference.py           # Inference — sampler-backend axis (pymc/numpyro/nutpie), arviz rhat-and-ess diagnostics
│   └── model.py               # ModelAsset — ONNX graph validation, io-binding, smoke inference, sklearn-to-ONNX export
├── numerics/                  # Numeric substrate: Array-API admission, JIT acceleration, certified intervals, unit-bearing quantities, in-memory statistics
│   ├── array.py               # ArrayPayload — namespace-dispatched dtype/shape/named-axes/finite/identity admission
│   ├── jit.py                 # JitBackend — numba LLVM / jax XLA compile routes over one _JIT_ROUTES capture table folding JitEvidence lowered-IR
│   ├── interval.py            # IntervalNumerics — Arb ball / mpmath interval / numpy outward-rounding floor ladder
│   ├── quantity.py            # UncertainQuantity — correlated first-order uncertainty through pint unit algebra
│   └── statistics.py          # Statistics — in-memory scipy.stats hypothesis tests + MLE distribution fit folding one StatReport
├── analysis/                  # Classical-math producers: digital signal processing, computer-algebra derivation, computational geometry
│   ├── signal.py              # IIR/FIR filter design, Welch spectral estimation, polyphase resampling, pywt wavelet/scalogram/packet multiresolution fold
│   ├── transform.py           # scipy.fft DFT/real-DFT/cosine-sine transforms + scipy.signal.hilbert analytic-signal envelope on one frequency-domain owner
│   ├── symbolic.py            # SymbolicDerivation — sympy lambdify/codegen producing numpy/C handoff artifact
│   └── spatial.py             # SpatialQuery — KD-tree neighbour/radius, hull, Delaunay, Voronoi, alpha-shape boundary fold
└── graduation/                # Python-only graduation rail and C# stub codegen
    ├── handoff.py             # GraduationReceipt — handoff axis moving offline evidence into managed owner system
    └── codegen.py             # StubCodegen — ast-builder stub emitter decoding C# evidence-bundle shape
```

## [02]-[SEAMS]

```text seams
*                   ⇄  csharp:Rasm.Compute       # [GRADUATION]: HandoffAxis graduation evidence
graduation/codegen  ←  csharp:Rasm.Compute       # [WIRE]: EvidenceBundle graduation-evidence wire
solvers/receipt     →  csharp:Rasm.Compute       # [PROJECTION]: SolverReceipt convergence verdict
graduation          ←  python:geometry/ifc       # [GRADUATION]: geometry HandoffAxis case IDS/clash/BCF
graduation/handoff  ⇄  python:geometry/graph     # [GRADUATION]: HandoffAxis geometry case
numerics/array      ⇄  python:runtime/transport  # [WIRE]: ContentIdentity array backend dispatch
numerics/quantity   ⇄  csharp:Rasm.Compute       # [WIRE]: QuantityFamily SI canonicalization over the wire to host-free peers
experiments/study   ←  python:data/tabular       # [SHAPE]: DOE dataset / labelled-array study input
analysis            ←  python:artifacts/media    # [SHAPE]: SignalOp spectral/filter/resample + analytic-signal centroid/envelope substitutes
```

## [03]-[ORGANIZATION]

The sub-domains are independent numeric-science concerns that meet only through the one solve receipt, the one study spine, and the one graduation rail. `numerics/array` admits any backend array; `solvers` discriminates every numeric route and folds one `SolverReceipt`; `solvers/sensitivity` reads the implicit-function-theorem adjoint through the autodifferentiable solvers; `optimization` is the offline-optimization sub-domain spanning three sibling owners discriminated by problem structure — `design` drives an Equinox-parameterized objective to a stationary point through Optimistix over the autodifferentiable solves and the implicit-adjoint gradient, `program` solves the constrained, global, and discrete math programs the gradient loop cannot reach over `scipy.optimize`, and `convex` returns a dual-certificate proof of global optimality for disciplined-convex programs over a conic backend, each composing the solver/sensitivity/assembly owners rather than re-owning them; `numerics/interval`, `analysis/signal`, `analysis/transform`, and `analysis/symbolic` are independent evidence producers, `analysis/signal` carrying both stationary-spectrum and multiresolution-wavelet cases on one owner beside `analysis/transform` owning the in-memory frequency-domain DFT/trigonometric/analytic-signal transforms; `numerics/quantity` threads uncertainty through the unit algebra; `numerics/statistics` folds the `scipy.stats` hypothesis tests and the maximum-likelihood distribution fit into one in-memory `StatReport`, deferring all columnar and gridded statistical aggregation to the `data` branch gridded/field owner; `experiments` orchestrates the study spine with SALib-owned sensitivity; `experiments/model` validates and exports classical estimators; `experiments/inference` owns gradient-MCMC posteriors; and `graduation` is the single rail every useful result crosses outward.

The `analysis/spatial` and `solvers/mesh` owners close the former gaps: `analysis/spatial` houses array-native computational geometry over `scipy.spatial` — `SpatialQuery` discriminates KD-tree neighbour and radius search, convex hull, Delaunay, Voronoi, and the alpha-shape boundary fold, each query keyed by content identity. `solvers/mesh` houses simulation mesh-and-field interchange and weak-form assembly beside the FEM solver route — `MeshField` carries the mesh topology and per-node/per-cell field arrays, lowers a weak form to the sparse stiffness/load pair the quadrature FEM route and a Diffrax field problem consume, and `MeshExchange` rounds the mesh-and-field through the meshio format registry. Each is named by its domain concept, never a rail or axis file-naming scheme.

## [04]-[BOUNDARIES]

- `compute` is not a production compute runtime, benchmark authority, substrate selector, tensor-session owner, or product-receipt owner; it owns offline evidence that graduates through the one rail.
- Columnar and labelled-array interchange ownership stays in the `data` branch; `compute` composes the `xarray`/`dask` shapes as study inputs and never re-catalogues them. Columnar and gridded statistical aggregation — grouped reductions, rolling windows, per-cell/per-band summaries over a labelled or gridded array — is the `data` branch gridded/field owner; `numerics/statistics` operates on an in-memory sample array only and never re-owns a grouped-reduction or labelled-array aggregation.
- Geometry tessellation, registration, and topology stay in the `geometry` branch and graduate through the geometry `HandoffAxis` case; `compute` accepts the geometry evidence, never re-implements it.
- Classical statistics, validated numerics, surrogate and classification model assets, and gradient-MCMC inference are in-scope; generative and deep-learning model authoring is out of scope across every sub-domain.
