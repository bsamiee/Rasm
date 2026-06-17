# [PY_COMPUTE]

The host-free numeric and scientific companion of the monorepo: array admission, one route-discriminated solver, differentiation, validated numerics, signal processing, symbolics, unit-bearing uncertainty, design-of-experiments, model assets, and Bayesian inference, with the graduation rail that hands offline results across the wire into the C# managed owner system. This router indexes the design pages under `.planning/` and registers every external package the folder uses; `ARCHITECTURE.md` carries the domain map and boundaries, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[PAGES]

| [INDEX] | [PAGE]                                                          | [OWNS]                                                            |
| :-----: | :------------------------------------------------------------- | :--------------------------------------------------------------- |
|   [1]   | [arrays/payload](.planning/arrays/payload.md)                  | namespace-dispatched array admission over the Array API standard |
|   [2]   | [solvers/receipt](.planning/solvers/receipt.md)                | the method-discriminated solve receipt over every route          |
|   [3]   | [solvers/linear](.planning/solvers/linear.md)                  | dense/sparse/eigen over scipy and the Lineax operator tier       |
|   [4]   | [solvers/nonlinear](.planning/solvers/nonlinear.md)            | root/minimise/fixed-point/least-squares over Optimistix          |
|   [5]   | [solvers/quadrature](.planning/solvers/quadrature.md)          | 1-D quadrature, spline interpolation, the weak-form FEM fold      |
|   [6]   | [solvers/differential](.planning/solvers/differential.md)      | ODE/SDE/CDE integration over Diffrax                              |
|   [7]   | [differentiation/sensitivity](.planning/differentiation/sensitivity.md) | reverse-mode VJP and the implicit-adjoint solver loop   |
|   [8]   | [validated_numerics/enclosure](.planning/validated_numerics/enclosure.md) | the Arb/mpmath/numpy certified-enclosure floor ladder |
|   [9]   | [signal/dsp](.planning/signal/dsp.md)                          | IIR/FIR filter design, spectral estimation, resampling           |
|  [10]   | [symbolics/derivation](.planning/symbolics/derivation.md)      | sympy lambdify and codegen producing the handoff artifact         |
|  [11]   | [metrology/quantity](.planning/metrology/quantity.md)          | correlated uncertainty through the pint unit algebra              |
|  [12]   | [experiments/study](.planning/experiments/study.md)            | DOE sampling, SALib sensitivity, surrogate fitting                |
|  [13]   | [experiments/run_history](.planning/experiments/run_history.md) | run persistence, partial-cell resume, run comparison            |
|  [14]   | [models/asset](.planning/models/asset.md)                      | ONNX validation and the sklearn-to-ONNX export                    |
|  [15]   | [inference/bayesian](.planning/inference/bayesian.md)          | the sampler-backend axis and arviz convergence diagnostics        |
|  [16]   | [graduation/receipt](.planning/graduation/receipt.md)          | the handoff axis moving offline evidence outward                  |
|  [17]   | [graduation/stub_codegen](.planning/graduation/stub_codegen.md) | the ast-builder stub emitter decoding the C# evidence bundle     |

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions live in the one root manifest; centralization is absolute, and no per-folder manifest exists.

- Array admission and acceleration: `numpy`, `array-api-compat`, `array-api-extra`, `numba`, `jax`.
- Solvers and differential equations: `scipy`, `scikit-fem`, `lineax`, `optimistix`, `diffrax`, `equinox`.
- Spatial geometry and mesh interchange: `scipy` (`scipy.spatial`), `meshio`.
- Symbolics: `sympy`.
- Validated numerics: `python-flint`, `mpmath`.
- Metrology: `pint`, `uncertainties`.
- Experiments and sensitivity: `SALib`, `scikit-learn`.
- Model assets: `onnx`, `onnxruntime`, `skl2onnx`.
- Bayesian inference: `pymc`, `arviz`, `numpyro`, `nutpie`.
- Encoding and typing: `msgspec`, `expression`, `beartype`.
