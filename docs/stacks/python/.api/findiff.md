# [PY_COMPUTE_API_FINDIFF]

`findiff` supplies finite-difference numerical derivatives and PDE solves on uniform and non-uniform grids of any dimension: a single `Diff(axis, grid, acc, periodic)` derivative operator that composes under an operator algebra (powers, sums, products, mixed partials, variable coefficients), raw finite-difference coefficient generation, vector-calculus operators (`Gradient`, `Divergence`, `Curl`, `Laplacian`), matrix representations of linear differential operators, compact (Padé) interior/boundary schemes, symbolic scheme generation, and boundary-value PDE solves with Dirichlet/Neumann/Robin conditions. It is the compute finite-difference floor for solvers and sensitivity: the autodiff owner falls back to `findiff` where automatic differentiation is unavailable, composing `Diff` and its `.matrix(shape)` operator representation rather than hand-rolling stencils.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `findiff`
- package: `findiff`
- module: `findiff`
- owner: `compute`
- rail: finite-difference derivatives and PDE solve
- namespace: `findiff` (all public symbols at top level via `__all__`)
- installed: `0.13.1` authored from ledger ([04]-sourced; `assay api` reflection blocked on the cp315 core because the transitive `scipy`/`numpy` dependencies have no matching cp315 wheels and source build requires the Forge scientific toolchain — OpenBLAS/CMake/`pkg-config`); license MIT; wheel `findiff-0.13.1-py3-none-any.whl` (`py3-none-any`, pure-Python)
- gate: `[GATED]` `; python_version<'3.15'` — `findiff` depends on `scipy` (structurally cp315-gated, no cp315 wheel) and `numpy`, so it carries the `; python_version<'3.15'` marker as a gated enrichment finite-difference floor for compute solvers/sensitivity, never a cp315-core dependency
- requires: `numpy`, `scipy`, `sympy`
- capability: single `Diff` derivative operator with operator algebra, raw FD coefficient generation, vector-calculus operators, sparse matrix representation of linear differential operators, compact (Padé) schemes, symbolic scheme generation, boundary-value PDE solve

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: derivative operator and algebra
- rail: finite-difference derivatives
- `Diff` is the canonical operator (from `findiff.interface`); all derivative construction routes through it. Operator algebra (`+`, `*`, `**`, scalar/field coefficients) is owned by the `Expression` base in `findiff.operators`, returning composed `Add` / `Mul` operators that are themselves applied or rendered to a matrix.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]        | [CAPABILITY]                                                                               |
| :-----: | :------------ | :------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Diff`        | derivative operator  | partial derivative along one axis; composes under `+`/`*`/`**` into mixed/higher operators |
|  [02]   | `Identity`    | identity operator    | identity term for building `a*Diff + b*Identity` linear operators                          |
|  [03]   | `Coefficient` | variable coefficient | position-dependent coefficient multiplying an operator (variable-coeff terms)              |
|  [04]   | `Coef`        | variable coefficient | short alias of `Coefficient` (`findiff.compatible`)                                        |
|  [05]   | `Id`          | identity operator    | short alias of `Identity` (`findiff.compatible`)                                           |

[PUBLIC_TYPE_SCOPE]: vector-calculus operators
- rail: finite-difference derivatives
- `Gradient`, `Divergence`, `Curl` construct with no positional grid args (spacing supplied at construction via keyword on the concrete classes); `Laplacian` takes grid spacing and accuracy directly.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :----------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `Gradient`   | vector operator | gradient of a scalar field (vector of partials)       |
|  [02]   | `Divergence` | vector operator | divergence of a vector field (scalar)                 |
|  [03]   | `Curl`       | vector operator | curl of a 3-D vector field (vector)                   |
|  [04]   | `Laplacian`  | vector operator | Laplacian `Laplacian(h, acc)`; sum of second partials |

[PUBLIC_TYPE_SCOPE]: PDE, boundary, and scheme types
- rail: PDE solve

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]         | [CAPABILITY]                                                                                                   |
| :-----: | :------------------- | :-------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `PDE`                | boundary-value PDE    | `PDE(lhs, rhs, bcs)`; assembles and solves a linear BVP                                                        |
|  [02]   | `BoundaryConditions` | BC container          | `BoundaryConditions(shape)`; indexed assignment of Dirichlet/Neumann/Robin rows                                |
|  [03]   | `CompactScheme`      | compact (Padé) scheme | `CompactScheme(deriv, left, right)`; implicit interior/boundary stencil; `from_accuracy(acc, deriv, num_left)` |

[PUBLIC_TYPE_SCOPE]: symbolic and diagnostic types
- rail: finite-difference derivatives

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [CAPABILITY]                                                       |
| :-----: | :-------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `SymbolicMesh`  | symbolic grid     | `SymbolicMesh(coord, equidistant)`; symbolic coordinate mesh       |
|  [02]   | `SymbolicDiff`  | symbolic operator | `SymbolicDiff(mesh, axis, degree)`; symbolic FD scheme over a mesh |
|  [03]   | `ErrorEstimate` | error diagnostic  | a-posteriori truncation-error estimate for an applied operator     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: derivative operator construction and application
- rail: finite-difference derivatives
- `Diff` is applied by calling it on an array (`d(f)`); `acc` defaults to 2 and may be raised globally via `set_accuracy`. All four trailing parameters are positional-or-keyword (no keyword-only `*` marker): positional order is `periodic` before `acc`, so positional binding is `Diff(axis, grid, periodic, acc, scheme, compact)`.
- `compact` is an odd-integer accuracy shortcut: it is mutually exclusive with `scheme` (passing both raises `ValueError`), must be an odd integer (even raises `ValueError`), and is converted internally to a `CompactScheme` via `CompactScheme.from_accuracy(acc, 1, compact)`.

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]        | [RAIL]                                                                                                                                                       |
| :-----: | :-------------------------------------------------------------------------- | :-------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Diff(axis=0, grid=None, periodic=False, acc=2, scheme=None, compact=None)` | construction          | partial derivative along `axis`; `grid` is uniform spacing or coord array; `compact` = odd-integer compact-scheme shortcut, mutually exclusive with `scheme` |
|  [02]   | `Diff(axis, grid) ** n`                                                     | algebra (power)       | n-th derivative along `axis` (e.g. `Diff(0, dx) ** 2`)                                                                                                       |
|  [03]   | `op_a + op_b` / `op_a * op_b`                                               | algebra (sum/product) | linear combination and composition (mixed partials, products)                                                                                                |
|  [04]   | `Coefficient(field) * Diff(axis, grid)`                                     | algebra (coeff)       | variable-coefficient term                                                                                                                                    |
|  [05]   | `operator(f)`                                                               | application           | apply the (composed) operator to array `f`, returning the derivative array                                                                                   |
|  [06]   | `operator.set_accuracy(acc)` / `operator.set_grid(grid)`                    | reconfiguration       | raise accuracy order / rebind grid on a built operator                                                                                                       |

[ENTRYPOINT_SCOPE]: matrix representation and spectral diagnostics
- rail: finite-difference derivatives
- defined on the `Expression` base; every `Diff` and composed operator carries these, so the AD-fallback owner composes the sparse matrix instead of looping stencils.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :----------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `operator.matrix(shape)` → `scipy.sparse` matrix | matrix rep     | sparse matrix of the linear differential operator over a grid `shape` |
|  [02]   | `operator.stencil(shape)`                        | stencil        | finite-difference stencil set for the operator                        |
|  [03]   | `operator.estimate_error(f, acc)`                | error estimate | a-posteriori truncation-error estimate                                |
|  [04]   | `operator.eigs(shape, k, which, sigma, bc, M)`   | spectrum       | eigenpairs of the operator (general)                                  |
|  [05]   | `operator.eigsh(shape, k, which, sigma, bc, M)`  | spectrum       | eigenpairs of the operator (symmetric/Hermitian)                      |

[ENTRYPOINT_SCOPE]: raw coefficient generation
- rail: finite-difference derivatives
- RETURN SHAPE: `coefficients(deriv, acc=...)` (no explicit `offsets`) returns a dict keyed by scheme position — `{'center': {...}, 'forward': {...}, 'backward': {...}}` — where each inner dict carries `'coefficients'` (the weight array) and `'offsets'` (the integer stencil offsets), plus an `'accuracy'` entry. The consumer reads `result['center']['coefficients']` and `result['center']['offsets']` for the interior stencil and the `'forward'`/`'backward'` keys for the one-sided boundary stencils. `coefficients(deriv, offsets=[...])` (explicit stencil) returns a SINGLE flat dict `{'coefficients': ..., 'offsets': ..., 'accuracy': ...}` with no center/forward/backward partition, because an explicit offset list already fixes the one stencil.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                                                              |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `coefficients(deriv, acc=None, offsets=None, symbolic=False)` | coefficients   | FD coefficients for derivative order `deriv` at target accuracy; returns center/forward/backward dict |
|  [02]   | `coefficients(deriv, offsets=[...], symbolic=False)`          | coefficients   | FD coefficients for an explicit offset stencil; returns one flat `{'coefficients','offsets','accuracy'}` dict |
|  [03]   | `coefficients(deriv, acc, symbolic=True)`                     | coefficients   | exact `sympy` rational coefficients (same dict layout, `Rational` weights)          |

[ENTRYPOINT_SCOPE]: PDE assembly and solve
- rail: PDE solve

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]   | [RAIL]                                                       |
| :-----: | :------------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `BoundaryConditions(shape)`      | BC container     | construct an empty BC set sized to the grid `shape`          |
|  [02]   | `bcs[index] = value`             | Dirichlet BC     | fix solution value at boundary index                         |
|  [03]   | `bcs[index] = (operator, value)` | Neumann/Robin BC | impose a derivative-operator condition at boundary index     |
|  [04]   | `PDE(lhs, rhs, bcs)`             | BVP assembly     | assemble linear BVP from operator `lhs`, source `rhs`, `bcs` |
|  [05]   | `PDE(...).solve(solver=None)`    | BVP solve        | solve the assembled boundary-value problem                   |

[ENTRYPOINT_SCOPE]: compact schemes and symbolic generation
- rail: finite-difference derivatives

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                                      |
| :-----: | :-------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `CompactScheme(deriv, left, right)`                 | compact scheme | implicit (Padé) FD scheme; pass via `Diff(..., scheme=...)` |
|  [02]   | `CompactScheme.from_accuracy(acc, deriv, num_left)` | compact scheme | compact scheme targeting a given accuracy order             |
|  [03]   | `SymbolicMesh(coord, equidistant=True)`             | symbolic mesh  | symbolic coordinate mesh for scheme derivation              |
|  [04]   | `SymbolicDiff(mesh, axis, degree=1)`                | symbolic op    | symbolic FD scheme over a `SymbolicMesh`                    |

[ENTRYPOINT_SCOPE]: legacy tuple-argument constructor
- rail: finite-difference derivatives
- `FinDiff` is a deprecated factory function in `findiff.compatible`, retained for backward compatibility; it is NOT a class and is NOT an identity alias of `Diff` (`FinDiff is Diff` is false). It accepts the legacy tuple-argument form and builds `Diff`-based operators; new code constructs `Diff` directly.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :--------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `FinDiff(axis, spacing, count=1, acc=2)` | legacy factory | uniform-grid derivative via legacy tuple form (deprecated) |
|  [02]   | `FinDiff((axis, spacing, count), ...)`   | legacy factory | mixed/higher operator from multiple tuples (deprecated)    |

## [04]-[IMPLEMENTATION_LAW]

[FD_TOPOLOGY]:
- namespace: `findiff`; all public symbols live at top level through `__all__` — `Diff` (from `findiff.interface`) is the canonical operator and every derivative routes through it.
- operator algebra is the construction surface: `Diff(0, dx) ** 2` is a second derivative, `Diff(0, dx) * Diff(1, dy)` is a mixed partial, `a * Diff(0, dx) + b * Identity` is a general linear operator, and `Coefficient(field) * Diff(...)` is a variable-coefficient term; operators are applied by calling them on an array.
- matrix law: `operator.matrix(shape)` returns the `scipy.sparse` representation of the linear differential operator over a grid of shape `shape`; the AD-fallback path composes this matrix rather than re-deriving stencils.
- coefficient law: `coefficients(deriv, acc=...)` or `coefficients(deriv, offsets=...)` is the single raw-coefficient surface; `symbolic=True` returns exact `sympy` rationals.
- PDE law: a boundary-value problem is one `PDE(lhs, rhs, bcs)`, where `bcs` is a `BoundaryConditions(shape)` populated by indexed assignment — `bcs[idx] = value` for Dirichlet, `bcs[idx] = (operator, value)` for Neumann/Robin; `.solve()` returns the field.
- compact law: compact (Padé) interior/boundary schemes pass to `Diff` via the `scheme=` keyword as a `CompactScheme`; `CompactScheme.from_accuracy(...)` targets an accuracy order.

[LOCAL_ADMISSION]:
- `findiff` is pure-Python (`py3-none-any`) but is `[GATED]` `; python_version<'3.15'` because it depends on `scipy` (structurally cp315-gated, no cp315 wheel) and `numpy`; it is a gated enrichment finite-difference floor for compute solvers/sensitivity owned by the Forge scientific toolchain on the sub-3.15 band, never a cp315-core dependency.
- the compute autodiff owner admits `findiff` as the finite-difference fallback: where automatic differentiation is unavailable it composes `Diff` and `operator.matrix(shape)` for Jacobian/sensitivity assembly, never a hand-rolled stencil loop.
- `FinDiff`, `Coef`, `Id` (from `findiff.compatible`) are backward-compatibility shims; new owners construct `Diff`, `Coefficient`, and `Identity` directly and never introduce a parallel derivative type beside `Diff`.

[RAIL_LAW]:
- Package: `findiff`
- Owns: finite-difference derivative operators with operator algebra, raw FD coefficient generation, vector-calculus operators, sparse matrix representation of linear differential operators, compact (Padé) schemes, symbolic scheme generation, and boundary-value PDE solves
- Accept: `Diff(axis, grid, periodic=, acc=, scheme=, compact=)` as the single derivative owner; `**`/`+`/`*`/`Coefficient` operator algebra for higher, mixed, and variable-coefficient operators; `operator.matrix(shape)` for the sparse operator representation behind AD fallback; `coefficients(deriv, acc=|offsets=)` for raw coefficients; `PDE` + `BoundaryConditions` for BVPs
- Reject: hand-rolled finite-difference stencils or coefficient tables when `Diff`/`coefficients` own them; a parallel derivative type beside `Diff`; use of the deprecated `FinDiff` tuple factory in new code; a local sparse-operator assembly duplicating `operator.matrix(shape)`
