# [PY_COMPUTE_API_FINDIFF]

`findiff` is admitted as the finite-difference COEFFICIENT floor and nothing more: the compute autodiff owner mines exactly one member — `coefficients(deriv, acc)` — for the central-difference stencil weights it uses where automatic differentiation is unavailable. The `Diff` operator algebra, `.matrix(shape)` sparse-operator representation, vector-calculus operators, boundary-value PDE solve, compact/symbolic scheme generation, and eigen-spectrum surfaces are NOT consumed by any compute fence and are a sealed decline. The one live consumer is `solvers/sensitivity.md` (`Differentiation`, the finite-difference-mode arm): it contracts the `center` stencil from `coefficients(deriv=1, acc=acc)` over an accuracy-width grid — the arbitrary-accuracy stencil `numpy.gradient` (fixed 2nd-order) cannot supply — and never composes a `Diff` operator or assembles a `findiff` matrix.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `findiff`
- package: `findiff`
- import: `findiff` — the `coefficients` top-level function only
- owner: `compute`
- rail: finite-difference derivatives (coefficient floor)
- installed: `0.13.1`
- requires: `numpy`, `scipy`, `sympy`
- consumer: `solvers/sensitivity.md` — `Differentiation` finite-difference mode, `coefficients(deriv=1, acc=acc)` `center` stencil
- capability: arbitrary-order finite-difference coefficient generation for a derivative order at a target accuracy width; exact `sympy` rationals under `symbolic=True`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: raw coefficient generation
- rail: finite-difference derivatives
- RETURN SHAPE: `coefficients(deriv, acc=...)` (no explicit `offsets`) returns a dict keyed by scheme position — `{'center': {...}, 'forward': {...}, 'backward': {...}}` — where each inner dict carries `'coefficients'` (the weight array), `'offsets'` (integer stencil offsets), and `'accuracy'`. The consumer reads `result['center']['coefficients']` and `result['center']['offsets']` for the interior stencil, `'forward'`/`'backward'` for the one-sided boundary stencils. `coefficients(deriv, offsets=[...])` (explicit stencil) returns a SINGLE flat `{'coefficients', 'offsets', 'accuracy'}` dict with no center/forward/backward partition, because an explicit offset list already fixes the one stencil.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | `coefficients(deriv, acc=None, offsets=None, symbolic=False)` | center/forward/backward stencil dict at target accuracy  |
|  [02]   | `coefficients(deriv, offsets=[...], symbolic=False)`          | one flat single-stencil dict for an explicit offset list |
|  [03]   | `coefficients(deriv, acc, symbolic=True)`                     | exact `sympy` `Rational` weights (same dict layout)      |

## [03]-[DECLINE]

[SEALED_DECLINE]: every findiff surface beyond `coefficients` is CLOSED — no compute fence performs the composition, so authoring it is a sealed decline.
- Operator algebra (`Diff`/`Identity`/`Coefficient`, `**`/`+`/`*`, `Coef`/`Id`/`FinDiff` compat shims): the AD owner is `jax`/`equinox` grad; the finite-difference arm reads raw stencil weights, it never builds or applies a `Diff` operator.
- Sparse operator representation (`operator.matrix(shape)`, `.stencil(shape)`, `.estimate_error`, `.eigs`/`.eigsh`): unused — no fence assembles a findiff differential operator matrix; sparse-eigen demand routes to `scipy.sparse.linalg` (see `scipy.md`).
- Vector calculus (`Gradient`/`Divergence`/`Curl`/`Laplacian`): unused — field operators are the mesh/quadrature FEM owners', not findiff's.
- Boundary-value PDE (`PDE`/`BoundaryConditions`/`.solve`) and scheme generation (`CompactScheme`/`SymbolicMesh`/`SymbolicDiff`): out of scope — compute solves no BVP through findiff.
- Reopening requires a live compute fence importing a findiff operator surface under a named consumer, never catalog preference.

## [04]-[RAIL_LAW]

[RAIL_LAW]:
- Package: `findiff`
- Owns (as admitted): arbitrary-order finite-difference coefficient generation — `coefficients(deriv, acc=|offsets=)` returning the position-keyed stencil weights, exact under `symbolic=True`
- Accept: `coefficients(deriv, acc=acc)` for the `center` interior stencil (and `forward`/`backward` boundary stencils) inside the `Differentiation` finite-difference mode where AD is unavailable
- Reject: a hand-rolled stencil-weight table when `coefficients` owns it; any use of the `Diff` operator algebra, `.matrix(shape)`, vector-calculus, PDE, compact, or symbolic surfaces (sealed decline); the deprecated `FinDiff` tuple factory
