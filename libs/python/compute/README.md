# [PY_COMPUTE]

`compute` is the Python branch's terminal evidence plane: host-free scientific computation whose evidence lands under receipts, admits against governed ceilings, keys over canonical bytes, and emits self-describing so a consumer decodes without re-derivation. It imports no host runtime and re-owns no peer interior, and a study resumes from history under provably equal keys.

## [01]-[ROUTER]

[SOLVERS]:
- [01]-[RECEIPT](.planning/solvers/receipt.md): `SolverReceipt` every solve route folds its method-discriminated evidence onto.
- [02]-[LINEAR](.planning/solvers/linear.md): `LinearIntent` route over dense, sparse, and eigen solves beside the C#-paired exchange containers.
- [03]-[NONLINEAR](.planning/solvers/nonlinear.md): `NonlinearIntent` routing root, minimise, fixed-point, and least-squares solves over `Provider`.
- [04]-[QUADRATURE](.planning/solvers/quadrature.md): `QuadratureIntent` route over quadrature, interpolation, and the weak-form FEM fold.
- [05]-[DIFFERENTIAL](.planning/solvers/differential.md): `DifferentialIntent` route over adjoint-differentiable ODE, SDE, and CDE integration.
- [06]-[SENSITIVITY](.planning/solvers/sensitivity.md): `Differentiation` one autodiff owner over the full derivative algebra and the FD floor.
- [07]-[MESH](.planning/solvers/mesh.md): `MeshField` topology-and-field owner beside the `MeshExchange` generate, assemble, read, and write legs.
- [08]-[FIELD](.planning/solvers/field.md): `FieldQuery` readout interpolating, projecting, and resampling over a discrete field.

[OPTIMIZATION]:
- [09]-[DESIGN](.planning/optimization/design.md): `DesignProblem` driving differentiable design over the implicit-adjoint gradient.
- [10]-[PROGRAM](.planning/optimization/program.md): `ProgramIntent` over linear, integer, global, constrained-smooth, and assignment programs.
- [11]-[CONVEX](.planning/optimization/convex.md): `ConvexProgram` returning a dual-certificate proof over disciplined-convex programs.

[EXPERIMENTS]:
- [12]-[STUDY](.planning/experiments/study.md): `Study` folding DOE sampling, SALib sensitivity, surrogate fitting, and the benchmark discriminant.
- [13]-[HISTORY](.planning/experiments/history.md): `RunHistory` owner of content-keyed run persistence, partial resume, and comparison.
- [14]-[INFERENCE](.planning/experiments/inference.md): `Inference` owner of gradient-MCMC posteriors with chain-mixing diagnostics.
- [15]-[MODEL](.planning/experiments/model.md): `ModelAsset` owner of estimator validation, smoke inference, ONNX export, and the envelope copy.

[NUMERICS]:
- [16]-[ARRAY](.planning/numerics/array.md): `ArrayPayload` admitting any backend array through namespace dispatch.
- [17]-[JIT](.planning/numerics/jit.md): `JitBackend` compiling the numba LLVM, ufunc, C-ABI, and XLA routes over one capture table.
- [18]-[INTERVAL](.planning/numerics/interval.md): `IntervalNumerics` certifying, refining, and root-isolating enclosures over the floor ladder.
- [19]-[QUANTITY](.planning/numerics/quantity.md): `UncertainQuantity` threading correlated uncertainty through unit algebra.
- [20]-[STATISTICS](.planning/numerics/statistics.md): `TestIntent` routing in-memory hypothesis tests and MLE distribution fit.

[ANALYSIS]:
- [21]-[SIGNAL](.planning/analysis/signal.md): `SignalOp` folding IIR/FIR filtering, spectral estimation, resample, and the wavelet case.
- [22]-[TRANSFORM](.planning/analysis/transform.md): `TransformOp` over in-memory DFT, trigonometric, Hankel, and analytic-signal transforms.
- [23]-[SYMBOLIC](.planning/analysis/symbolic.md): `SymbolicDerivation` left-folding symbolic ops to a numpy, jax, or native handoff artifact.
- [24]-[SPATIAL](.planning/analysis/spatial.md): `SpatialQuery` over Qhull tessellation, KD-tree proximity, distances, alignment, alpha shapes.

[GRADUATION]:
- [25]-[HANDOFF](.planning/graduation/handoff.md): `HandoffAxis` owning outward egress, geometry decode, and the `ComputeLeg` roster.
- [26]-[CODEGEN](.planning/graduation/codegen.md): `StubCodegen` decoding the C# evidence bundle into typed stubs and schema under the drift gate.
- [27]-[OBSERVABILITY](.planning/graduation/observability.md): `ComputePoint` hook roster over the stage rail and the `ResourceUsage` ledger.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; admission rows ride the workspace manifests as bare names, `uv.lock` fixes every version, and this folder's `.api/` corroborates.

[ARRAY_AND_JIT]:
- `array-api-compat`
- `array-api-extra`
- `numba`
- `jax`
- `jaxtyping`
- `sparse`
- `dask` — Passive `array_namespace` backend; compute imports no Dask runtime.

[SOLVERS]:
- `scipy`
- `scikit-fem`
- `gmsh`
- `lineax`
- `optimistix`
- `diffrax`
- `equinox`
- `findiff`
- `quadax`
- `interpax`

[OPTIMIZATION]:
- `optax`
- `cvxpy`
- `clarabel`
- `scs`
- `highspy`

[CERTIFIED_NUMERICS]:
- `python-flint`
- `mpmath`
- `pint`
- `uncertainties`

[SIGNAL_SYMBOLIC]:
- `pywavelets`
- `sympy`

[EXPERIMENTS]:
- `SALib`
- `pyDOE3` — Classical design-of-experiments matrix generation.
- `scikit-learn`
- `onnx`
- `onnxruntime`
- `skl2onnx`
- `pymc`
- `arviz`
- `numpyro`
- `nutpie`
- `blackjax`

[INTERCHANGE]:
- `h5py` — HDF5 exchange containers for .NET-peer sparse operators, drift envelopes, and waveform corpora.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`

[OBSERVABILITY]:
- `opentelemetry-api` — Hub-only trace surface folding the geometry producer context as a span `Link`.

[NUMERIC_SUBSTRATE]:
- `numpy`
- `xarray`

[TRANSPORT]:
- `universal-pathlib`

[MESH_INTERCHANGE]:
- `meshio`

[WIRE_CODEGEN]:
- `protobuf` — Google message runtime beneath the ONNX model IR; `experiments/model` reads its `DecodeError` on torn model bytes.
