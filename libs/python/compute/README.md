# [PY_COMPUTE]

The host-free numeric and scientific companion of the monorepo: array admission, one route-discriminated solver, differentiation, validated numerics, signal processing, symbolics, unit-bearing uncertainty, design-of-experiments, model assets, and Bayesian inference, with the graduation rail that hands offline results across the wire into the C# managed owner system. This router indexes the design pages under `.planning/` and registers every external package the folder uses; `ARCHITECTURE.md` carries the domain map and boundaries, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- arrays: [payload](.planning/arrays/payload.md)
- solvers: [receipt](.planning/solvers/receipt.md), [linear](.planning/solvers/linear.md), [nonlinear](.planning/solvers/nonlinear.md), [quadrature](.planning/solvers/quadrature.md), [differential](.planning/solvers/differential.md)
- differentiation: [sensitivity](.planning/differentiation/sensitivity.md)
- optimization (queued, see `TASKLOG.md`): `design`, `program`, `convex`
- validated_numerics: [enclosure](.planning/validated_numerics/enclosure.md)
- signal: [dsp](.planning/signal/dsp.md)
- symbolics: [derivation](.planning/symbolics/derivation.md)
- metrology: [quantity](.planning/metrology/quantity.md)
- experiments: [study](.planning/experiments/study.md), [run_history](.planning/experiments/run_history.md)
- spatial: [query](.planning/spatial/query.md)
- simframe: [mesh_field](.planning/simframe/mesh_field.md)
- models: [asset](.planning/models/asset.md)
- inference: [bayesian](.planning/inference/bayesian.md)
- graduation: [receipt](.planning/graduation/receipt.md), [stub_codegen](.planning/graduation/stub_codegen.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions live in the one root manifest; centralization is absolute, and no per-folder manifest exists.

- Array admission and acceleration: `array-api-compat`, `array-api-extra`, `numba`, `jax`, `sparse`.
- Solvers and differential equations: `scipy`, `scikit-fem`, `lineax`, `optimistix`, `diffrax`, `equinox`.
- Inverse-design optimization: `optimistix`, `equinox`, `jax`, `optax` (the first-order-descent axis row threaded through `optimistix.OptaxMinimiser`).
- Constrained and discrete optimization: `scipy` (`scipy.optimize` `linprog`/`milp`/`differential_evolution`/`minimize`/`linear_sum_assignment`).
- Convex optimization: `cvxpy`, `clarabel` (the conic backend producing the dual-certificate proof of optimality).
- Spatial geometry and mesh interchange: `scipy` (`scipy.spatial`), `meshio`.
- Signal processing: `scipy` (`scipy.signal`), `pywavelets`.
- Symbolics: `sympy`.
- Validated numerics: `python-flint`, `mpmath`.
- Metrology: `pint`, `uncertainties`.
- Experiments and sensitivity: `SALib`, `scikit-learn`, `dask`.
- Model assets: `onnx`, `onnxruntime`, `skl2onnx`.
- Bayesian inference: `pymc`, `arviz`, `numpyro`, `nutpie`, `blackjax`.

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes; canonical registry lives at `libs/python/.api/`.

- expression
- beartype
- msgspec
- numpy
