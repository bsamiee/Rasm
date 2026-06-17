# [PY_COMPUTE_API_SCIPY]

`scipy` supplies the dense/sparse linear algebra, optimization, integration, interpolation, and signal/statistics surfaces for the compute numeric-intent solver rail. The package owner routes the `NumericIntent` dense-linear, sparse-solve, nonlinear-optimize, integrate, and interpolate cases onto scipy submodules; it never re-implements a numeric routine scipy owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scipy`
- package: `scipy`
- import: `scipy` (lint alias `sp`); submodules `scipy.linalg`, `scipy.sparse`, `scipy.sparse.linalg`, `scipy.optimize`, `scipy.integrate`, `scipy.interpolate`, `scipy.stats`, `scipy.signal`
- owner: `compute`
- rail: solvers
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: scientific solver suite — dense and sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics, signal processing

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: solver submodule owners
- rail: solvers

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]        | [CAPABILITY]                              |
| :-----: | :-------------------- | :-------------------- | :---------------------------------------- |
|   [1]   | `scipy.linalg`        | dense linear algebra  | factorizations, eigensolvers, dense solve |
|   [2]   | `scipy.sparse`        | sparse containers     | CSR/CSC/COO sparse matrix formats         |
|   [3]   | `scipy.sparse.linalg` | sparse solve          | iterative and direct sparse solvers       |
|   [4]   | `scipy.optimize`      | nonlinear optimize    | root-find, least-squares, minimization    |
|   [5]   | `scipy.integrate`     | numerical integration | quadrature and ODE integrators            |
|   [6]   | `scipy.interpolate`   | interpolation         | spline and grid interpolation             |
|   [7]   | `scipy.stats`         | statistics            | distributions and statistical tests       |
|   [8]   | `scipy.signal`        | signal processing     | filters and spectral transforms           |

[ENTRYPOINTS]:
- UN_REFLECTED: exact callable spellings and verified signatures (the `scipy.linalg`/`scipy.sparse.linalg`/`scipy.optimize`/`scipy.integrate`/`scipy.interpolate` routines) require a reflectable install to capture; submodule names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- import: submodule imports at boundary scope only; module-level import is banned by the manifest import policy.
- routing: `NumericIntent` dense-linear -> `scipy.linalg`; sparse-solve -> `scipy.sparse.linalg`; nonlinear-optimize -> `scipy.optimize`; integrate -> `scipy.integrate`; interpolate -> `scipy.interpolate`.
- evidence: each solve captures the route submodule, the tolerance inputs, and the convergence/residual as a study receipt.
- boundary: scipy results are offline study evidence; production substrate selection and benchmark claims stay in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `scipy`
- Owns: dense/sparse linear algebra, nonlinear optimization, numerical integration, interpolation, statistics, signal processing for the numeric-intent rail
- Accept: a `NumericIntent` case routed to a scipy submodule with captured tolerances and residuals
- Reject: hand-rolled numeric kernels scipy owns; wrapper-renames of solver callables; product benchmark claims
