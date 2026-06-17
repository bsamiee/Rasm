# [PY_COMPUTE_ARCHITECTURE]

The professional domain map of the `compute` package: the full sub-domain structure that mirrors the eventual source tree, each sub-domain named by its real numeric-science concept and carrying a one-line charter. Sub-domains with a design page are decision-complete; sub-domains listed without a page are visible gaps that fuel the ideas and tasks. The map is the atlas, not the mechanics — every fence, owner, and integration point lives on its `.planning/<sub-domain>/<page>.md` design page, and the open work lives in `TASKLOG.md`.

## [1]-[DOMAIN_MAP]

```text codemap
compute/
├── arrays/                     # backend-agnostic array admission over the Array API standard
│   └── payload.md              #   ArrayPayload — namespace-dispatched dtype/shape/named-axes/finite/identity admission
├── solvers/                    # one route-discriminated numeric solver folding one solve receipt
│   ├── receipt.md              #   SolverReceipt — the method-discriminated solve receipt over every route
│   ├── linear.md               #   LinearIntent — dense/sparse/eigen over scipy + the Lineax autodiff operator tier
│   ├── nonlinear.md            #   NonlinearIntent — root/minimise/fixed-point/least-squares over Optimistix + the numba row
│   ├── quadrature.md           #   QuadratureIntent — 1-D quadrature, spline interpolation, the weak-form FEM fold
│   └── differential.md         #   DifferentialIntent — ODE/SDE/CDE integration over Diffrax with adjoint-differentiable solves
├── differentiation/            # the one VJP/Jacobian/sensitivity owner over the JAX family
│   └── sensitivity.md          #   Differentiation — reverse-mode adjoint + implicit-adjoint solver loop + finite-difference floor
├── validated_numerics/         # certified-enclosure interval and ball arithmetic
│   └── enclosure.md            #   IntervalNumerics — Arb ball / mpmath interval / numpy outward-rounding floor ladder
├── signal/                     # classical digital signal processing
│   └── dsp.md                  #   Signal — IIR/FIR filter design, Welch spectral estimation, polyphase resampling
├── symbolics/                  # classical computer-algebra derivation and codegen
│   └── derivation.md           #   SymbolicDerivation — sympy lambdify/codegen producing the numpy/C handoff artifact
├── metrology/                  # unit-bearing uncertain quantities
│   └── quantity.md             #   UncertainQuantity — correlated first-order uncertainty through the pint unit algebra
├── experiments/                # design-of-experiments, sensitivity, surrogates, run history
│   ├── study.md                #   Study — DOE sampling, SALib sensitivity, surrogate fitting, the benchmark discriminant
│   └── run_history.md          #   RunHistory — content-key-keyed run persistence, partial-cell resume, run comparison
├── models/                     # classical ML model-asset validation and export
│   └── asset.md                #   ModelAsset — ONNX graph validation, io-binding, smoke inference, the sklearn-to-ONNX export
├── inference/                  # classical Bayesian inference over a prior/likelihood graph
│   └── bayesian.md             #   Inference — the sampler-backend axis (pymc/numpyro/nutpie), arviz rhat-and-ess diagnostics
├── graduation/                 # the Python-only graduation rail and the C# stub codegen
│   ├── receipt.md              #   GraduationReceipt — the handoff axis moving offline evidence into the managed owner system
│   └── stub_codegen.md         #   StubCodegen — the ast-builder stub emitter decoding the C# evidence-bundle shape
├── spatial/                    # PLANNED — array-native computational geometry (KD-tree/Voronoi/Delaunay/alpha-shape) over scipy.spatial
└── simframe/                   # PLANNED — simulation mesh-and-field interchange and weak-form assembly beside the FEM solver route
```

## [2]-[ORGANIZATION]

The sub-domains are independent numeric-science concerns that meet only through the one solve receipt, the one study spine, and the one graduation rail. `arrays` admits any backend array; `solvers` discriminates every numeric route and folds one `SolverReceipt`; `differentiation` reads the implicit-function-theorem adjoint through the autodifferentiable solvers; `validated_numerics`, `signal`, and `symbolics` are independent evidence producers; `metrology` threads uncertainty through the unit algebra; `experiments` orchestrates the study spine with SALib-owned sensitivity; `models` validates and exports classical estimators; `inference` owns gradient-MCMC posteriors; and `graduation` is the single rail every useful result crosses outward.

The `spatial` and `simframe` sub-domains are planned gaps: `spatial` houses array-native computational geometry over `scipy.spatial`, and `simframe` houses simulation mesh-and-field interchange and weak-form assembly beside the existing FEM solver route. Each is named by its domain concept, never a rail or axis file-naming scheme, so the gap is visible to the ideas and tasks before any page exists.

## [3]-[BOUNDARIES]

- `compute` is not a production compute runtime, benchmark authority, substrate selector, tensor-session owner, or product-receipt owner; it owns offline evidence that graduates through the one rail.
- Columnar and labelled-array interchange ownership stays in the `data` branch; `compute` composes the `xarray`/`dask` shapes as study inputs and never re-catalogues them.
- Geometry tessellation, registration, and topology stay in the `geometry` branch and graduate through the geometry `HandoffAxis` case; `compute` accepts the geometry evidence, never re-implements it.
- Classical statistics, validated numerics, surrogate and classification model assets, and gradient-MCMC inference are in-scope; generative and deep-learning model authoring is out of scope across every sub-domain.
