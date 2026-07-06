# [PY_COMPUTE]

`libs/python/compute` is the host-free numeric and scientific library of the monorepo. It owns array admission, JIT kernel acceleration, one route-discriminated solver surface, differentiation, validated numerics, signal processing, symbolics, unit-bearing uncertainty, design-of-experiments, model assets, and Bayesian inference. The graduation rail hands offline results across the wire into the C# managed owner system. `ARCHITECTURE.md` carries the domain map and boundaries, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[RECEIPT](.planning/solvers/receipt.md)
- [02]-[LINEAR](.planning/solvers/linear.md)
- [03]-[NONLINEAR](.planning/solvers/nonlinear.md)
- [04]-[QUADRATURE](.planning/solvers/quadrature.md)
- [05]-[DIFFERENTIAL](.planning/solvers/differential.md)
- [06]-[SENSITIVITY](.planning/solvers/sensitivity.md)
- [07]-[MESH](.planning/solvers/mesh.md)
- [08]-[FIELD](.planning/solvers/field.md)
- [09]-[DESIGN](.planning/optimization/design.md)
- [10]-[PROGRAM](.planning/optimization/program.md)
- [11]-[CONVEX](.planning/optimization/convex.md)
- [12]-[STUDY](.planning/experiments/study.md)
- [13]-[HISTORY](.planning/experiments/history.md)
- [14]-[INFERENCE](.planning/experiments/inference.md)
- [15]-[MODEL](.planning/experiments/model.md)
- [16]-[ARRAY](.planning/numerics/array.md)
- [17]-[JIT](.planning/numerics/jit.md)
- [18]-[INTERVAL](.planning/numerics/interval.md)
- [19]-[QUANTITY](.planning/numerics/quantity.md)
- [20]-[STATISTICS](.planning/numerics/statistics.md)
- [21]-[SIGNAL](.planning/analysis/signal.md)
- [22]-[TRANSFORM](.planning/analysis/transform.md)
- [23]-[SYMBOLIC](.planning/analysis/symbolic.md)
- [24]-[SPATIAL](.planning/analysis/spatial.md)
- [25]-[HANDOFF](.planning/graduation/handoff.md)
- [26]-[CODEGEN](.planning/graduation/codegen.md)

## [02]-[DOMAIN_PACKAGES]

Every scientific and solver library this folder uses, planned or implemented. Versions are centralized in the one root manifest; corroborating API evidence lives in the adjacent `.api/` folder.

[ARRAY_ADMISSION]:
- `array-api-compat`
- `array-api-extra`
- `numba`
- `jax`
- `jaxtyping`
- `sparse`

[SOLVERS_DIFFERENTIAL]:
- `scipy`
- `scikit-fem`
- `lineax`
- `optimistix`
- `diffrax`
- `equinox`
- `findiff`
- `quadax`
- `interpax`

[INVERSE_DESIGN]:
- `optimistix`
- `equinox`
- `jax`
- `optax`

[CONSTRAINED_DISCRETE_OPTIMIZATION]:
- `scipy`

[CONVEX_OPTIMIZATION]:
- `cvxpy`
- `clarabel`

[SPATIAL_MESH]:
- `scipy`
- `meshio`

[SIGNAL_PROCESSING]:
- `scipy`
- `pywavelets`

[SYMBOLICS]:
- `sympy`

[VALIDATED_NUMERICS]:
- `python-flint`
- `mpmath`

[METROLOGY]:
- `pint`
- `uncertainties`

[EXPERIMENTS_SENSITIVITY]:
- `SALib`
- `scikit-learn`

[MODEL_ASSETS]:
- `onnx`
- `onnxruntime`
- `skl2onnx`

[BAYESIAN_INFERENCE]:
- `pymc`
- `arviz`
- `numpyro`
- `nutpie`
- `blackjax`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting Python substrate libraries this folder consumes; canonical registry and charters live in `libs/python/.planning/README.md` and the adjacent `libs/python/.api/` folder.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`

[NUMERIC_SUBSTRATE]:
- `numpy`
- `dask` (passive `array_namespace` backend only — the branch-tier catalog is the owner; compute imports no Dask runtime)

[FIELD_DATA]:
- `xarray`
- `narwhals` (the study DOE-frame admission arm, consumed through the published `data/tabular` contract surfaces)

[OBSERVABILITY]:
- `opentelemetry-api` (the hub `evidence_run` weave imports `trace`/`Span`/`Status`/`StatusCode`; SDK binding stays at composition root)

[RESOURCES]:
- `universal-pathlib`
