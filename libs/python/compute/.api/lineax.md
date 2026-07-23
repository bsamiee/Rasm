# [PY_COMPUTE_API_LINEAX]

`lineax` owns JAX-native linear operators, tag-dispatched direct and iterative solvers, and lazy operator algebra for the differentiable-linear-algebra rail. Every solve compiles under `jax.jit`, batches under `vmap`, and differentiates through the implicit-function-theorem adjoint, so it nests inside an `optimistix` root-find or a `diffrax` implicit step without re-deriving its gradient.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lineax`
- package: `lineax`
- import: `lineax` (alias `lx`)
- owner: `compute`
- rail: differentiable linear algebra
- capability: JAX-native linear operators, direct and iterative solvers, structure-tagged auto-dispatch, operator arithmetic algebra, and autodiff-through-solve pipelines under JIT/`vmap`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: linear operator types

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                                                        |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `AbstractLinearOperator`    | operator base     | abstract base; `.mv`, `.as_matrix`, `.transpose`, `.in_structure`, `.out_structure` |
|  [02]   | `MatrixLinearOperator`      | dense operator    | operator from a JAX array matrix; `(matrix, tags=())`                               |
|  [03]   | `FunctionLinearOperator`    | function operator | operator from a linear callable `fn(v)` plus `input_structure`                      |
|  [04]   | `JacobianLinearOperator`    | Jacobian operator | JVP-backed `J = d(fn)/dx` at `x`; never materialises the Jacobian                   |
|  [05]   | `DiagonalLinearOperator`    | diagonal operator | operator from a diagonal vector                                                     |
|  [06]   | `IdentityLinearOperator`    | identity operator | identity over a given input structure                                               |
|  [07]   | `PyTreeLinearOperator`      | pytree operator   | operator wrapping a pytree-structured matrix with explicit `output_structure`       |
|  [08]   | `TridiagonalLinearOperator` | structured op     | `(diagonal, lower_diagonal, upper_diagonal)`; sizes `n, n-1, n-1`                   |
|  [09]   | `TaggedLinearOperator`      | tag wrapper       | re-tags an operator with explicit structure tags                                    |
|  [10]   | `TangentLinearOperator`     | tangent operator  | forward-mode tangent (JVP) of a linear operator                                     |
|  [11]   | `AddLinearOperator`         | arithmetic op     | sum of two operators (`a + b`)                                                      |
|  [12]   | `ComposedLinearOperator`    | arithmetic op     | composition/product (`a @ b`)                                                       |
|  [13]   | `MulLinearOperator`         | arithmetic op     | scalar-multiply (`c * a`)                                                           |
|  [14]   | `DivLinearOperator`         | arithmetic op     | scalar-divide (`a / c`)                                                             |
|  [15]   | `NegLinearOperator`         | arithmetic op     | negation (`-a`)                                                                     |

[PUBLIC_TYPE_SCOPE]: linear solver types deriving from `AbstractLinearSolver`, passed as `solver=` to `linear_solve`; `CG`/`GMRES`/`BiCGStab`/`LSMR` share `(rtol, atol, norm=…, max_steps=None)`, each row adding only its extra parameters

| [INDEX] | [SYMBOL]               | [ENTRY_FAMILY]   | [SIGNATURE]                                                                         |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `AbstractLinearSolver` | solver base      | abstract base; subclass for custom `init`/`compute`/`transpose`/`conj` solver state |
|  [02]   | `AutoLinearSolver`     | dispatch solver  | `(well_posed: bool \| None)`; `True`→direct-by-tag, `None`→QR, `False`→SVD          |
|  [03]   | `LU`                   | direct solver    | `()`; square well-posed                                                             |
|  [04]   | `QR`                   | direct solver    | `()`; rectangular well-posed / overdetermined                                       |
|  [05]   | `Cholesky`             | direct solver    | `()`; requires `positive_semidefinite_tag` or `symmetric_tag`                       |
|  [06]   | `SVD`                  | direct solver    | `(rcond: float \| None = None)`; least-squares / rank-deficient                     |
|  [07]   | `Diagonal`             | direct solver    | `(well_posed: bool = False, rcond: float \| None = None)`; requires `diagonal_tag`  |
|  [08]   | `Triangular`           | direct solver    | `()`; requires `lower_triangular_tag` or `upper_triangular_tag`                     |
|  [09]   | `Tridiagonal`          | direct solver    | `()`; Thomas algorithm; requires `tridiagonal_tag`                                  |
|  [10]   | `CG`                   | iterative solver | `stabilise_every=10`; symmetric positive-definite                                   |
|  [11]   | `GMRES`                | iterative solver | `restart=20, stagnation_iters=20`; general square                                   |
|  [12]   | `BiCGStab`             | iterative solver | general square non-symmetric                                                        |
|  [13]   | `LSMR`                 | iterative solver | `norm=two_norm, conlim=1e8`; least-squares / min-norm rectangular                   |
|  [14]   | `Normal`               | normal-equations | `(inner_solver: AbstractLinearSolver)`; solves `AᵀA x = Aᵀb` via the wrapped solver |

[PUBLIC_TYPE_SCOPE]: solution and result types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]    | [CAPABILITY]                                                                                          |
| :-----: | :--------- | :--------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Solution` | result carrier   | `equinox.Module` fields `value` (PyTree), `result` (`RESULTS` item), `stats` (dict), `state` (PyTree) |
|  [02]   | `RESULTS`  | termination enum | `equinox.Enumeration`; a member is an `EnumerationItem` carrying `_value`/`_enumeration` only         |

[RESULTS_ITEMS]: `successful` (zero code) `max_steps_reached` `singular` `breakdown` `stagnation` `conlim` `nonfinite_input` — the `Solution.result` termination vocabulary, read to a message via `RESULTS[item]`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve and invert operations — both default `solver=AutoLinearSolver(well_posed=True)`, `*, options=None, throw=True`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `linear_solve(operator, vector, *, state=sentinel)` → `Solution` | solve          | JIT/`vmap`/autodiff-compatible solve        |
|  [02]   | `invert(operator)` → `FunctionLinearOperator`                    | invert         | matrix-free, autodiff-safe inverse operator |

[ENTRYPOINT_SCOPE]: operator utilities

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]      | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `linearise(operator)` → `AbstractLinearOperator`   | linearise           | freeze a `JacobianLinearOperator` to a fixed-point form      |
|  [02]   | `materialise(operator)` → `AbstractLinearOperator` | materialise         | force a dense `MatrixLinearOperator`; debug / small ops only |
|  [03]   | `diagonal(operator)` → `Shaped[Array, "size"]`     | diagonal extract    | extract the main diagonal as a 1-D JAX array                 |
|  [04]   | `tridiagonal(operator)` → `(diag, lower, upper)`   | tridiagonal extract | diagonals `n, n-1, n-1`; extractor, not constructor          |
|  [05]   | `conj(operator)` → `AbstractLinearOperator`        | conjugate           | complex-conjugate operator                                   |

[ENTRYPOINT_SCOPE]: structure tag predicates, constants, and transforms — tags are `_HasRepr` sentinels passed as a `frozenset` in `tags=`

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `is_symmetric(op)` / `symmetric_tag`                         | tag predicate  | symmetry check and tag constant                      |
|  [02]   | `is_diagonal(op)` / `diagonal_tag`                           | tag predicate  | diagonal structure                                   |
|  [03]   | `is_positive_semidefinite(op)` / `positive_semidefinite_tag` | tag            | PSD structure (enables `Cholesky`)                   |
|  [04]   | `is_negative_semidefinite(op)` / `negative_semidefinite_tag` | tag            | NSD structure                                        |
|  [05]   | `is_lower_triangular(op)` / `lower_triangular_tag`           | tag            | lower-triangular (enables `Triangular`)              |
|  [06]   | `is_upper_triangular(op)` / `upper_triangular_tag`           | tag            | upper-triangular (enables `Triangular`)              |
|  [07]   | `is_tridiagonal(op)` / `tridiagonal_tag`                     | tag            | tridiagonal (enables `Tridiagonal`)                  |
|  [08]   | `has_unit_diagonal(op)` / `unit_diagonal_tag`                | tag            | unit-diagonal structure                              |
|  [09]   | `transpose_tags(tags: frozenset)` → `frozenset`              | tag transform  | tag-set → transpose tag-set (TAGS in, not operator)  |
|  [10]   | `transpose_tags_rules`                                       | tag registry   | dispatch `transpose_tags` reads; extend for new tags |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Operators are `equinox.Module` pytrees; `a + b`, `a @ b`, `c * a`, `-a` build lazy `AddLinearOperator`/`ComposedLinearOperator`/`MulLinearOperator`/`NegLinearOperator` nodes, forming no matrix until a solver's `.mv` runs.
- `AutoLinearSolver(well_posed=True)` reads operator tags in order `Cholesky`(PSD) → `Triangular` → `Tridiagonal` → `Diagonal` → `LU`; `well_posed=None` routes a rectangular full-rank operator to `QR`, `well_posed=False` a rank-deficient least-squares operator to `SVD`.
- `TaggedLinearOperator(op, frozenset({symmetric_tag, positive_semidefinite_tag}))` declares proven structure so `AutoLinearSolver` dispatches `Cholesky`; a mistag is trusted silently, so only proven structure is tagged.
- `linearise(JacobianLinearOperator(...))` freezes a residual's Jacobian at the current iterate into a reusable operator for an outer Newton loop.
- `linear_solve(..., throw=True)` raises a non-`successful` `RESULTS` at the JAX boundary; `throw=False` returns `result` and `stats` for a traced study, `stats` carrying iterative telemetry and `{}` for direct solvers.

[STACKING]:
- `equinox`(`.api/equinox.md`): every operator/`Solution`/`RESULTS` is an `eqx.Module`/`eqx.Enumeration`, `EnumerationItem` mechanics (`_name_to_item` recovery, batched `jnp.max` verdict reduction) owned there; filter operators with `eqx.partition` and guard `result` with `eqx.error_if`.
- `jax`(`.api/jax.md`): `linear_solve` is `jit`/`vmap`/`grad`-transformable, and the implicit-function-theorem adjoint keeps reverse-mode from tracing the solver loop, so a batched Newton step's linear solve wraps in `vmap` directly.
- `optimistix`(`.api/optimistix.md`): a Gauss-Newton or Levenberg-Marquardt solver takes `lx.GMRES(...)` or `lx.AutoLinearSolver(...)` as its `linear_solver=` through the shared `AbstractLinearSolver` protocol.
- `diffrax`(`.api/diffrax.md`): implicit and IMEX integrators solve their stage equations through a lineax `linear_solver=`, reusing the same operator-tag dispatch as a standalone solve.
- `scikit-fem`(`.api/scikit-fem.md`): an assembled FEM system folds into a `FunctionLinearOperator` tagged `symmetric_tag, positive_semidefinite_tag`, then `linear_solve(..., CG(rtol, atol))` differentiably solves the PDE without densifying.
- `findiff`(`.api/findiff.md`): a finite-difference stencil wraps into `FunctionLinearOperator`/`TridiagonalLinearOperator` and inherits tag-driven solver selection.

[LOCAL_ADMISSION]:
- Build the operator once per problem outside the hot loop and pass it into `linear_solve` under JIT; `materialise` runs only on a static, small operator.
- `invert` returns a matrix-free `FunctionLinearOperator`; store and reapply it across many right-hand sides under autodiff instead of re-solving.

[RAIL_LAW]:
- Package: `lineax`
- Owns: JAX-native linear-operator abstraction, tag-dispatched direct and iterative solvers, and differentiable batched solve pipelines
- Accept: `AbstractLinearOperator` subclasses as the operator surface, `AbstractLinearSolver` instances as the solver surface, `linear_solve` as the unified entry
- Reject: `jnp.linalg.solve` or `scipy.sparse.linalg` where a differentiable, batched, or tag-dispatched solve is required
