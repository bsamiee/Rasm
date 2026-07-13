# [PY_COMPUTE]

`libs/python/compute` owns the monorepo's host-free offline scientific evidence: independent solver, optimization, experiment, numeric, and analysis sub-domains converge through one solve receipt, one study spine, and one graduation rail that hands every result across the wire to the C# managed owner. It imports no host runtime and re-owns no peer interior — geometry, columnar data, and tensor sessions cross only as receipt data at the `HandoffAxis`.

## [01]-[ROUTER]

[SOLVERS]:
- [01]-[RECEIPT](.planning/solvers/receipt.md): `SolverReceipt` every solve route folds its method-discriminated evidence onto.
- [02]-[LINEAR](.planning/solvers/linear.md): `LinearIntent` route over dense, sparse, and eigen solves.
- [03]-[NONLINEAR](.planning/solvers/nonlinear.md): `NonlinearIntent` route over root, minimise, fixed-point, and least-squares solves.
- [04]-[QUADRATURE](.planning/solvers/quadrature.md): `QuadratureIntent` route over quadrature, interpolation, and the weak-form FEM fold.
- [05]-[DIFFERENTIAL](.planning/solvers/differential.md): `DifferentialIntent` route over adjoint-differentiable ODE, SDE, and CDE integration.
- [06]-[SENSITIVITY](.planning/solvers/sensitivity.md): `Differentiation` owner threading reverse-mode and implicit-adjoint sensitivity.
- [07]-[MESH](.planning/solvers/mesh.md): `MeshField` owner of mesh topology, per-node and per-cell fields, and weak-form assembly.
- [08]-[FIELD](.planning/solvers/field.md): `FieldQuery` readout interpolating, projecting, and resampling over a discrete field.

[OPTIMIZATION]:
- [09]-[DESIGN](.planning/optimization/design.md): `DesignProblem` driving differentiable design over the implicit-adjoint gradient.
- [10]-[PROGRAM](.planning/optimization/program.md): `ProgramIntent` over constrained, integer, global, and assignment programs.
- [11]-[CONVEX](.planning/optimization/convex.md): `ConvexProgram` returning a dual-certificate proof over disciplined-convex programs.

[EXPERIMENTS]:
- [12]-[STUDY](.planning/experiments/study.md): `Study` spine folding DOE sampling, sensitivity, surrogate fitting, and the benchmark discriminant.
- [13]-[HISTORY](.planning/experiments/history.md): `RunHistory` owner of content-keyed run persistence, partial resume, and comparison.
- [14]-[INFERENCE](.planning/experiments/inference.md): `Inference` owner of gradient-MCMC posteriors with convergence diagnostics.
- [15]-[MODEL](.planning/experiments/model.md): `ModelAsset` owner of classical-estimator validation, smoke inference, and ONNX export.

[NUMERICS]:
- [16]-[ARRAY](.planning/numerics/array.md): `ArrayPayload` admitting any backend array through namespace dispatch.
- [17]-[JIT](.planning/numerics/jit.md): `JitBackend` compiling the LLVM and XLA routes over one capture table.
- [18]-[INTERVAL](.planning/numerics/interval.md): `IntervalNumerics` certified-interval floor ladder.
- [19]-[QUANTITY](.planning/numerics/quantity.md): `UncertainQuantity` threading correlated uncertainty through unit algebra.
- [20]-[STATISTICS](.planning/numerics/statistics.md): `Statistics` owner of in-memory hypothesis tests and MLE distribution fit.

[ANALYSIS]:
- [21]-[SIGNAL](.planning/analysis/signal.md): `SignalOp` folding IIR/FIR filtering, spectral estimation, resample, and the wavelet case.
- [22]-[TRANSFORM](.planning/analysis/transform.md): `SpectralReadout` over in-memory DFT, trigonometric, and analytic-signal transforms.
- [23]-[SYMBOLIC](.planning/analysis/symbolic.md): `SymbolicDerivation` lowering symbols to a numpy or C handoff artifact.
- [24]-[SPATIAL](.planning/analysis/spatial.md): `SpatialQuery` folding neighbour, hull, Delaunay, Voronoi, and alpha-shape queries.

[GRADUATION]:
- [25]-[HANDOFF](.planning/graduation/handoff.md): `HandoffAxis` owning outward egress, geometry decode, and evidence weave.
- [26]-[CODEGEN](.planning/graduation/codegen.md): `StubCodegen` emitting the C# evidence-bundle stub under the drift gate.

## [02]-[DOMAIN_PACKAGES]

Scientific and solver libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`.

[ARRAY_AND_JIT]:
- `array-api-compat`
- `array-api-extra`
- `numba`
- `jax`
- `jaxtyping`
- `sparse`

[SOLVERS]:
- `scipy`
- `scikit-fem`
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
- `scikit-learn`
- `onnx`
- `onnxruntime`
- `skl2onnx`
- `pymc`
- `arviz`
- `numpyro`
- `nutpie`
- `blackjax`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the `libs/python/.planning/README.md` registry; the registry and its charters own the full contracts, and `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`

[NUMERIC_SUBSTRATE]:
- `numpy`
- `dask` (passive `array_namespace` backend only — the branch-tier catalog is the owner; compute imports no Dask runtime)
- `xarray`
- `narwhals` (the study DOE-frame admission arm, consumed through the published `data/tabular` contract surfaces)
- `meshio`

[OBSERVABILITY]:
- `opentelemetry-api` (the hub `evidence_run` weave imports `trace`/`Span`/`Status`/`StatusCode`; SDK binding stays at composition root)

[RESOURCES]:
- `universal-pathlib`
