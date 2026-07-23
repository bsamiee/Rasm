# [PY_COMPUTE_API_FINDIFF]

`findiff` owns arbitrary-order finite-difference coefficient generation; compute admits one member, `coefficients`, for the central-difference stencil weights the `Differentiation` finite-difference arm reads where automatic differentiation is unavailable. Every operator, sparse-matrix, vector-calculus, PDE, and scheme-generation surface stays declined — the AD owner is `jax`/`equinox`, field operators are the mesh and quadrature owners'.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `findiff`
- package: `findiff`
- module: `findiff` — the top-level `coefficients` function alone
- rail: finite-difference derivative coefficient floor

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: raw stencil-weight generation on a uniform grid, `acc` and `offsets` mutually exclusive

| [INDEX] | [SURFACE]                                 | [SHAPE] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------- | :------ | :-------------------------------------------------------- |
|  [01]   | `coefficients(deriv, acc)`                | static  | position-keyed `center`/`forward`/`backward` stencil dict |
|  [02]   | `coefficients(deriv, offsets)`            | static  | one flat single-stencil dict for an explicit offset list  |
|  [03]   | `coefficients(deriv, acc, symbolic=True)` | static  | exact `sympy.Rational` weights over the same dict layout  |

- `coefficients` inner shape: `acc` mode keys `{center, forward, backward}`, each `{coefficients, offsets, accuracy}` — interior under `center`, one-sided under `forward`/`backward`; `offsets` mode returns one flat `{coefficients, offsets, accuracy}`.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every consumed call is a pure Vandermonde solve — a derivative order and accuracy width in, position-keyed weight arrays out; `findiff` assembles no operator and reads no field data.

[STACKING]:
- `numpy`: `coefficients` and `offsets` return as ndarrays contracting as one dot over the per-axis stencil grid, the sole numeric path the floor walks.
- `sympy`: `symbolic=True` returns `Rational` weights and integer offsets, the exact-arithmetic path for a verified stencil table.
- `Differentiation` finite-difference arm: contracts the `center` stencil from `coefficients(deriv=1, acc=acc)` over an accuracy-width grid and records the realized order — the arbitrary-accuracy stencil `numpy.gradient`'s fixed second order cannot supply.

[LOCAL_ADMISSION]:
- `findiff` admits at `coefficients` alone; a live compute fence importing an operator, matrix, vector-calculus, PDE, or scheme surface under a named consumer reopens the wider surface.

[RAIL_LAW]:
- Package: `findiff`
- Owns: arbitrary-order finite-difference coefficient generation — `coefficients` returning position-keyed uniform-grid stencil weights, exact under `symbolic=True`
- Accept: `coefficients(deriv, acc=acc)` for the `center` interior and `forward`/`backward` boundary stencils inside the `Differentiation` finite-difference mode where AD is unavailable
- Reject: a hand-rolled stencil-weight table `coefficients` owns; the `Diff`/`Identity`/`Coefficient` operator algebra, `matrix(shape)`/`stencil`/`eigs`/`eigsh` sparse representation, `Gradient`/`Divergence`/`Curl`/`Laplacian` vector calculus, and `PDE`/`BoundaryConditions`/`CompactScheme`/`SymbolicDiff` solve surfaces — field operators are the mesh and quadrature owners', sparse-eigen routes to `scipy.sparse.linalg`
