# [PY_COMPUTE_API_LINEAX]

`lineax` supplies JAX-native linear operators, direct and iterative solvers, structure tags, and operator algebra for the compute differentiable-linear-algebra rail; every solve is JIT-compatible and supports forward/reverse autodiff through the `adjoint` protocol.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lineax`
- package: `lineax`
- import: `lineax`
- owner: `compute`
- rail: differentiable linear algebra
- capability: JAX-native linear operators, direct and iterative solvers, structure-tagged dispatch, and differentiable solve pipelines with JIT and autodiff support

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: linear operator types
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------------- |
|   [1]   | `AbstractLinearOperator`    | operator base     | abstract base for all linear operators          |
|   [2]   | `MatrixLinearOperator`      | dense operator    | operator from a JAX array matrix                |
|   [3]   | `FunctionLinearOperator`    | function operator | operator defined by a linear callable `fn(v)`   |
|   [4]   | `JacobianLinearOperator`    | Jacobian operator | operator from `jax.jacobian` of a function      |
|   [5]   | `DiagonalLinearOperator`    | diagonal operator | operator from a diagonal vector                 |
|   [6]   | `IdentityLinearOperator`    | identity operator | identity of given shape                         |
|   [7]   | `PyTreeLinearOperator`      | pytree operator   | operator wrapping a pytree-structured matrix    |
|   [8]   | `TaggedLinearOperator`      | tagged operator   | operator with explicit structure tags           |
|   [9]   | `TangentLinearOperator`     | tangent operator  | forward-mode tangent of a linear operator       |
|  [10]   | `AddLinearOperator`         | arithmetic op     | sum of two linear operators                     |
|  [11]   | `ComposedLinearOperator`    | arithmetic op     | composition (product) of two operators          |
|  [12]   | `MulLinearOperator`         | arithmetic op     | scalar-multiply of a linear operator            |
|  [13]   | `DivLinearOperator`         | arithmetic op     | scalar-divide of a linear operator              |
|  [14]   | `NegLinearOperator`         | arithmetic op     | negation of a linear operator                   |
|  [15]   | `TridiagonalLinearOperator` | structured op     | tridiagonal operator from three diagonal arrays |

[PUBLIC_TYPE_SCOPE]: linear solver types
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]           | [ENTRY_FAMILY]   | [SIGNATURE]                                                                                    |
| :-----: | :----------------- | :--------------- | :--------------------------------------------------------------------------------------------- |
|   [1]   | `AutoLinearSolver` | dispatch solver  | `(well_posed: bool \| None)`                                                                   |
|   [2]   | `LU`               | direct solver    | `()`                                                                                           |
|   [3]   | `QR`               | direct solver    | `()`                                                                                           |
|   [4]   | `Cholesky`         | direct solver    | `()`; requires positive-definite operator                                                      |
|   [5]   | `SVD`              | direct solver    | `(rcond: float \| None = None)`; least-squares capable                                         |
|   [6]   | `Triangular`       | direct solver    | `()`; requires triangular tag                                                                  |
|   [7]   | `CG`               | iterative solver | `(rtol, atol, norm=max_norm, stabilise_every=10, max_steps=None)`; symmetric positive-definite |
|   [8]   | `GMRES`            | iterative solver | `(rtol, atol, norm=max_norm, max_steps=None, restart=20, stagnation_iters=20)`                 |
|   [9]   | `BiCGStab`         | iterative solver | `(rtol, atol, norm=max_norm, max_steps=None)`                                                  |
|  [10]   | `LSMR`             | iterative solver | `(rtol, atol, norm=two_norm, max_steps=None, conlim=1e8)`; least-squares/min-norm              |
|  [11]   | `Normal`           | normal-equations | wraps another solver to solve via normal equations                                             |
|  [12]   | `NormalCG`         | iterative solver | normal-equations CG shorthand                                                                  |

[PUBLIC_TYPE_SCOPE]: solution and result types
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                                                                          |
| :-----: | :--------- | :------------- | :---------------------------------------------------------------------------------------------------- |
|   [1]   | `Solution` | result carrier | fields: `value`, `result` (`RESULTS`), `stats`, `state`                                               |
|   [2]   | `RESULTS`  | result enum    | `successful`, `max_steps_reached`, `singular`, `breakdown`, `stagnation`, `conlim`, `nonfinite_input` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve and invert operations
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `linear_solve(operator, vector, solver=AutoLinearSolver(True), *, options=None, state=..., throw=True)` → `Solution` | solve          | JIT-compatible solve                         |
|   [2]   | `invert(operator, solver=AutoLinearSolver(True), *, options=None, throw=True)` → `FunctionLinearOperator`            | invert         | operator inverse as `FunctionLinearOperator` |

[ENTRYPOINT_SCOPE]: operator utilities
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]   | [RAIL]                                   |
| :-----: | :------------------------------------------------- | :--------------- | :--------------------------------------- |
|   [1]   | `linearise(operator)` → `AbstractLinearOperator`   | linearise        | freeze operator into a linear form       |
|   [2]   | `materialise(operator)` → `AbstractLinearOperator` | materialise      | dense-matrix realization of any operator |
|   [3]   | `diagonal(operator)` → `Array`                     | diagonal extract | extract diagonal as shaped JAX array     |
|   [4]   | `conj(operator)` → `AbstractLinearOperator`        | conjugate        | complex-conjugate of operator            |

[ENTRYPOINT_SCOPE]: structure tag predicates and constructors
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `is_symmetric(operator)` / `symmetric_tag`                     | tag predicate  | symmetry check and tag constant       |
|   [2]   | `is_diagonal(operator)` / `diagonal_tag`                       | tag predicate  | diagonal structure                    |
|   [3]   | `is_positive_semidefinite(op)` / `positive_semidefinite_tag`   | tag            | PSD structure                         |
|   [4]   | `is_negative_semidefinite(op)` / `negative_semidefinite_tag`   | tag            | NSD structure                         |
|   [5]   | `is_lower_triangular(op)` / `lower_triangular_tag`             | tag            | lower-triangular structure            |
|   [6]   | `is_upper_triangular(op)` / `upper_triangular_tag`             | tag            | upper-triangular structure            |
|   [7]   | `is_tridiagonal(operator)` / `tridiagonal_tag`                 | tag            | tridiagonal structure                 |
|   [8]   | `has_unit_diagonal(operator)` / `unit_diagonal_tag`            | tag            | unit-diagonal structure               |
|   [9]   | `tridiagonal(lower, mid, upper)` → `TridiagonalLinearOperator` | construction   | tridiagonal from three arrays         |
|  [10]   | `transpose_tags(operator)` → `AbstractLinearOperator`          | transpose      | transposed operator with swapped tags |

## [4]-[IMPLEMENTATION_LAW]

[LINEAR_ALGEBRA_TOPOLOGY]:
- namespace: `lineax`; all public types and functions at top level
- `linear_solve` is the unified entry point; `AutoLinearSolver(well_posed=True)` dispatches to LU for square well-posed, SVD for least-squares
- `Solution.result` carries a `RESULTS` enum value; `throw=True` raises on non-`successful` by default
- structure tags are not Python booleans; they are `_HasRepr` sentinel objects used as `tags=` frozenset members
- `materialise` constructs a dense `jax.Array` matrix from any operator; use only for debugging or small operators

[LOCAL_ADMISSION]:
- Operators are constructed once per problem and passed to `linear_solve`; never materialize inside a JIT-traced function unless the operator is static.
- `well_posed=None` in `AutoLinearSolver` uses the operator tags to choose the solver automatically; prefer over explicit solver selection when tags are set.
- `Solution.stats` carries iteration count and residual norms for iterative solvers; inspect after `throw=False` calls.
- `invert` returns a `FunctionLinearOperator` that is not stored as a dense matrix; it is JIT-safe and supports autodiff.

[RAIL_LAW]:
- Package: `lineax`
- Owns: JAX-native linear operator abstraction, direct and iterative linear solvers, differentiable solve pipelines
- Accept: `AbstractLinearOperator` subclasses as the canonical operator surface; `linear_solve` as the unified solve entry point
- Reject: `jnp.linalg.solve` or `scipy.sparse.linalg` when differentiable or structure-tagged solves are required
