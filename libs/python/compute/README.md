# [PY_COMPUTE]

`compute` is the terminal evidence plane of the Python branch: host-free scientific computation converging on the one graduation hub through which every Python-branch crossing to the C# managed owner clears. Its bar is the composed hub, not the solver roster: evidence lands under receipts, admits against governed ceilings, keys over canonical bytes, and emits self-describing so the C# graduation gate decodes without re-derivation; heavy solves run off the event loop, and a study resumes from history under provably equal keys.

It imports no host runtime and re-owns no peer interior — geometry, columnar data, and tensor sessions cross only as receipt data at the `HandoffAxis`. Every producer streams runtime receipts through the hub evidence weave, solve routes fold the one solve receipt, and each graduating axis owner clears the one admission rail.

## [01]-[ROUTER]

[SOLVERS]:
- [01]-[RECEIPT](.planning/solvers/receipt.md): `SolverReceipt` every solve route folds its method-discriminated evidence onto.
- [02]-[LINEAR](.planning/solvers/linear.md): `LinearIntent` route over dense, sparse, and eigen solves.
- [03]-[NONLINEAR](.planning/solvers/nonlinear.md): `NonlinearIntent` route over root, minimise, fixed-point, and least-squares solves.
- [04]-[QUADRATURE](.planning/solvers/quadrature.md): `QuadratureIntent` route over quadrature, interpolation, and the weak-form FEM fold.
- [05]-[DIFFERENTIAL](.planning/solvers/differential.md): `DifferentialIntent` route over adjoint-differentiable ODE, SDE, and CDE integration.
- [06]-[SENSITIVITY](.planning/solvers/sensitivity.md): `Differentiation` owner threading reverse-mode and implicit-adjoint sensitivity.
- [07]-[MESH](.planning/solvers/mesh.md): `MeshField` topology-and-field owner beside `MeshExchange` assembling, reading, and writing the mesh.
- [08]-[FIELD](.planning/solvers/field.md): `FieldQuery` readout interpolating, projecting, and resampling over a discrete field.

[OPTIMIZATION]:
- [09]-[DESIGN](.planning/optimization/design.md): `DesignProblem` driving differentiable design over the implicit-adjoint gradient.
- [10]-[PROGRAM](.planning/optimization/program.md): `ProgramIntent` over constrained, integer, global, and assignment programs.
- [11]-[CONVEX](.planning/optimization/convex.md): `ConvexProgram` returning a dual-certificate proof over disciplined-convex programs.

[EXPERIMENTS]:
- [12]-[STUDY](.planning/experiments/study.md): `Study` folding DOE sampling, SALib sensitivity, surrogate fitting, and the benchmark discriminant.
- [13]-[HISTORY](.planning/experiments/history.md): `RunHistory` owner of content-keyed run persistence, partial resume, and comparison.
- [14]-[INFERENCE](.planning/experiments/inference.md): `Inference` owner of gradient-MCMC posteriors with convergence diagnostics.
- [15]-[MODEL](.planning/experiments/model.md): `ModelAsset` owner of classical-estimator validation, smoke inference, and ONNX export.

[NUMERICS]:
- [16]-[ARRAY](.planning/numerics/array.md): `ArrayPayload` admitting any backend array through namespace dispatch.
- [17]-[JIT](.planning/numerics/jit.md): `JitBackend` compiling the numba LLVM, ufunc, C-ABI, and XLA routes over one capture table.
- [18]-[INTERVAL](.planning/numerics/interval.md): `IntervalNumerics` certified-interval floor ladder.
- [19]-[QUANTITY](.planning/numerics/quantity.md): `UncertainQuantity` threading correlated uncertainty through unit algebra.
- [20]-[STATISTICS](.planning/numerics/statistics.md): `TestIntent` routing in-memory hypothesis tests and MLE distribution fit.

[ANALYSIS]:
- [21]-[SIGNAL](.planning/analysis/signal.md): `SignalOp` folding IIR/FIR filtering, spectral estimation, resample, and the wavelet case.
- [22]-[TRANSFORM](.planning/analysis/transform.md): `TransformOp` over in-memory DFT, trigonometric, Hankel, and analytic-signal transforms.
- [23]-[SYMBOLIC](.planning/analysis/symbolic.md): `SymbolicDerivation` lowering symbols to a numpy or C handoff artifact.
- [24]-[SPATIAL](.planning/analysis/spatial.md): `SpatialQuery` folding neighbour, hull, Delaunay, Voronoi, and alpha-shape queries.

[GRADUATION]:
- [25]-[HANDOFF](.planning/graduation/handoff.md): `HandoffAxis` owning outward egress, geometry decode, and evidence weave.
- [26]-[CODEGEN](.planning/graduation/codegen.md): `StubCodegen` decoding the C# evidence bundle into typed stubs and schema under the drift gate.

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
- `dask` — passive array-backend row only; its catalog homes folder-local, and compute imports no Dask runtime.
- `xarray`
- `narwhals` — admits the study DOE frame, consumed through the published `data/tabular` contract surfaces.
- `meshio`

[OBSERVABILITY]:
- `rasm.runtime.receipts` — `measured` weave the hub `evidence_run` binding composes; compute imports no OTel symbol, and SDK binding stays at the composition root.

[RESOURCES]:
- `universal-pathlib`
