# [PY_COMPUTE_API_LINEAX]

`lineax` supplies JAX-native linear operators, direct and iterative solvers, structure tags, and operator algebra for the compute differentiable-linear-algebra rail; every solve is JIT-compatible, `vmap`/`grad`-transformable, and supports forward/reverse autodiff through the implicit-function-theorem adjoint, so a solve nests inside an `optimistix` root-find or a `diffrax` implicit step without re-deriving its own gradient.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lineax`
- package: `lineax`
- version: `0.1.1`
- license: Apache-2.0
- import: `lineax` (conventional alias `lx`)
- owner: `compute`
- rail: differentiable linear algebra
- asset: pure-Python; runtime deps `equinox>=0.11.10`, `jax>=0.10.0`, `jaxtyping>=0.2.24`
- capability: JAX-native linear operators, direct and iterative solvers, structure-tagged auto-dispatch, operator arithmetic algebra, and autodiff-through-solve pipelines under JIT/`vmap`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: linear operator types — every operator is an `equinox.Module` pytree; arithmetic returns a new operator node, never a materialised matrix
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :-------------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `AbstractLinearOperator`    | operator base     | abstract base; `.mv`, `.as_matrix`, `.transpose`, `.in_structure`, `.out_structure`, `+`/`@`/`*` algebra |
|  [02]   | `MatrixLinearOperator`      | dense operator    | operator from a JAX array matrix; `(matrix, tags=())`        |
|  [03]   | `FunctionLinearOperator`    | function operator | operator from a linear callable `fn(v)` plus `input_structure` |
|  [04]   | `JacobianLinearOperator`    | Jacobian operator | JVP-backed `J = d(fn)/dx` at `x`; never materialises the Jacobian |
|  [05]   | `DiagonalLinearOperator`    | diagonal operator | operator from a diagonal vector                              |
|  [06]   | `IdentityLinearOperator`    | identity operator | identity over a given input structure                        |
|  [07]   | `PyTreeLinearOperator`      | pytree operator   | operator wrapping a pytree-structured matrix with explicit `output_structure` |
|  [08]   | `TridiagonalLinearOperator` | structured op     | `(diagonal, lower_diagonal, upper_diagonal)`; sizes `n, n-1, n-1` |
|  [09]   | `TaggedLinearOperator`      | tag wrapper       | re-tags an operator with explicit structure tags             |
|  [10]   | `TangentLinearOperator`     | tangent operator  | forward-mode tangent (JVP) of a linear operator              |
|  [11]   | `AddLinearOperator`         | arithmetic op     | sum of two operators (`a + b`)                               |
|  [12]   | `ComposedLinearOperator`    | arithmetic op     | composition/product (`a @ b`)                                |
|  [13]   | `MulLinearOperator`         | arithmetic op     | scalar-multiply (`c * a`)                                    |
|  [14]   | `DivLinearOperator`         | arithmetic op     | scalar-divide (`a / c`)                                      |
|  [15]   | `NegLinearOperator`         | arithmetic op     | negation (`-a`)                                              |

[PUBLIC_TYPE_SCOPE]: linear solver types — all derive from `AbstractLinearSolver`; pass an instance as the `solver=` argument to `linear_solve`
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]                | [ENTRY_FAMILY]   | [SIGNATURE]                                                                                       |
| :-----: | :---------------------- | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `AbstractLinearSolver`  | solver base      | abstract base; subclass for custom `init`/`compute`/`transpose`/`conj` solver state              |
|  [02]   | `AutoLinearSolver`      | dispatch solver  | `(well_posed: bool \| None)`; `well_posed=True`→LU/Cholesky/Triangular by tag, `False`→QR, `None`→least-squares SVD |
|  [03]   | `LU`                    | direct solver    | `()`; square well-posed                                                                          |
|  [04]   | `QR`                    | direct solver    | `()`; rectangular well-posed / overdetermined                                                    |
|  [05]   | `Cholesky`              | direct solver    | `()`; requires `positive_semidefinite_tag` or `symmetric_tag`                                    |
|  [06]   | `SVD`                   | direct solver    | `(rcond: float \| None = None)`; least-squares / rank-deficient                                  |
|  [07]   | `Diagonal`              | direct solver    | `(well_posed: bool = False, rcond: float \| None = None)`; requires `diagonal_tag`               |
|  [08]   | `Triangular`            | direct solver    | `()`; requires `lower_triangular_tag` or `upper_triangular_tag`                                  |
|  [09]   | `Tridiagonal`           | direct solver    | `()`; Thomas algorithm; requires `tridiagonal_tag`                                               |
|  [10]   | `CG`                    | iterative solver | `(rtol, atol, norm=max_norm, stabilise_every=10, max_steps=None)`; symmetric positive-definite   |
|  [11]   | `GMRES`                 | iterative solver | `(rtol, atol, norm=max_norm, max_steps=None, restart=20, stagnation_iters=20)`; general square   |
|  [12]   | `BiCGStab`              | iterative solver | `(rtol, atol, norm=max_norm, max_steps=None)`; general square non-symmetric                      |
|  [13]   | `LSMR`                  | iterative solver | `(rtol, atol, norm=two_norm, max_steps=None, conlim=1e8)`; least-squares / min-norm rectangular  |
|  [14]   | `Normal`                | normal-equations | `(inner_solver: AbstractLinearSolver)`; solves `AᵀA x = Aᵀb` via the wrapped solver              |
|  [15]   | `NormalCG`              | DEPRECATED        | `(rtol, atol, **cg_kwargs)` → `Normal(CG(...))`; deprecated in favour of `lx.Normal(lx.CG(...))`, slated for removal |

[PUBLIC_TYPE_SCOPE]: solution and result types
- rail: differentiable linear algebra

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]  | [CAPABILITY]                                                                                          |
| :-----: | :--------- | :------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Solution` | result carrier | `equinox.Module` fields: `value` (PyTree), `result` (`RESULTS` item), `stats` (dict), `state` (PyTree) |
|  [02]   | `RESULTS`  | termination enum (`equinox.Enumeration`) | items `successful` (the zero code), `max_steps_reached`, `singular`, `breakdown`, `stagnation`, `conlim`, `nonfinite_input`; the same `equinox.Enumeration` base `optimistix`/`diffrax` `RESULTS` ride, so a member is an `equinox.EnumerationItem` exposing ONLY `_value` (the int code) and `_enumeration` (the class) — NO `.name`/`.value` member-name attribute, and `RESULTS[item]` returns the human MESSAGE, so the member-name key is recoverable only by inverting the class `_name_to_item` map; `RESULTS.promote(item)` is inheritance-widening (lifts a member from a parent `Enumeration` to a subclass, raising `ValueError` on a same-class member) NOT a vmap reduction, and `RESULTS.where(pred, a, b)` is the branchless `jnp.where` 2-way select — the worst-case batched-`vmap`/`filter_vmap` aggregation is `jnp.max` over `solution.result._value` (the sole-zero `successful` making `max == 0` iff every right-hand side converged) plus the `_name_to_item` inversion, never `promote` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve and invert operations
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                                                                                                | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `linear_solve(operator, vector, solver=AutoLinearSolver(well_posed=True), *, options=None, state=sentinel, throw=True)` → `Solution` | solve          | JIT/`vmap`/autodiff-compatible solve         |
|  [02]   | `invert(operator, solver=AutoLinearSolver(well_posed=True), *, options=None, throw=True)` → `FunctionLinearOperator`      | invert         | operator inverse as a matrix-free, autodiff-safe `FunctionLinearOperator` |

[ENTRYPOINT_SCOPE]: operator utilities
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :---------------------------------------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `linearise(operator)` → `AbstractLinearOperator`                                                | linearise        | freeze a `JacobianLinearOperator` to a fixed-point linear form |
|  [02]   | `materialise(operator)` → `AbstractLinearOperator`                                              | materialise      | force a dense `MatrixLinearOperator`; debug / small ops only    |
|  [03]   | `diagonal(operator)` → `Shaped[Array, "size"]`                                                  | diagonal extract | extract the main diagonal as a 1-D JAX array    |
|  [04]   | `tridiagonal(operator)` → `(diag, lower, upper)` arrays sized `n, n-1, n-1`                      | tridiagonal extract | extract the three diagonals (NOT a constructor) |
|  [05]   | `conj(operator)` → `AbstractLinearOperator`                                                     | conjugate        | complex-conjugate operator                      |

[ENTRYPOINT_SCOPE]: structure tag predicates, tag constants, and tag transforms — tags are `_HasRepr` sentinels passed as a `frozenset` in `tags=`, NOT booleans
- rail: differentiable linear algebra

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `is_symmetric(op)` / `symmetric_tag`                            | tag predicate  | symmetry check and tag constant                        |
|  [02]   | `is_diagonal(op)` / `diagonal_tag`                             | tag predicate  | diagonal structure                                     |
|  [03]   | `is_positive_semidefinite(op)` / `positive_semidefinite_tag`   | tag            | PSD structure (enables `Cholesky`)                     |
|  [04]   | `is_negative_semidefinite(op)` / `negative_semidefinite_tag`   | tag            | NSD structure                                          |
|  [05]   | `is_lower_triangular(op)` / `lower_triangular_tag`             | tag            | lower-triangular (enables `Triangular`)                |
|  [06]   | `is_upper_triangular(op)` / `upper_triangular_tag`             | tag            | upper-triangular (enables `Triangular`)                |
|  [07]   | `is_tridiagonal(op)` / `tridiagonal_tag`                       | tag            | tridiagonal (enables `Tridiagonal`)                    |
|  [08]   | `has_unit_diagonal(op)` / `unit_diagonal_tag`                  | tag            | unit-diagonal structure                                |
|  [09]   | `transpose_tags(tags: frozenset)` → `frozenset`                | tag transform  | maps a tag set to its transpose's tag set (takes TAGS, not an operator) |
|  [10]   | `transpose_tags_rules`                                          | tag registry   | the dispatch table `transpose_tags` consults; extend to teach new tags |

## [04]-[INTEGRATION_TOPOLOGY]

[STACKS_WITH]: the differentiable-linear-algebra rail composes lineax as the inner linear-solve under the JAX numerical owners; a single dense rail threads operator → solve → enclosing nonlinear/ODE solve under one JIT boundary.

| [INDEX] | [SIBLING]      | [SEAM]                                                                                                            |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `equinox`      | every operator/`Solution`/`RESULTS` is an `eqx.Module`/`eqx.Enumeration`; filter operators with `eqx.partition`, branch on `result` with `eqx.error_if`, store solver state in module fields |
|  [02]   | `jax`          | `linear_solve` is `jax.jit`/`jax.vmap`/`jax.grad`-transformable; the implicit-function-theorem adjoint means reverse-mode through a solve never traces the solver loop — wrap a batched Newton step's linear solve in `vmap` directly |
|  [03]   | `optimistix`   | a Gauss-Newton/Levenberg-Marquardt root-find/minimise hands lineax its inner linear solve via the shared `AbstractLinearSolver` protocol; pass `lx.GMRES(...)` or `lx.AutoLinearSolver(...)` as the `linear_solver=` of an optimistix solver |
|  [04]   | `diffrax`      | implicit/IMEX `diffrax` integrators solve the stage equations with a lineax `linear_solver=`; a stiff ODE step reuses the same operator-tag dispatch as a standalone solve |
|  [05]   | `scikit-fem`   | assemble a sparse FEM system into a `FunctionLinearOperator` (matrix-free `mv` over the assembled bilinear form) tagged `symmetric_tag, positive_semidefinite_tag`, then `linear_solve(..., CG(rtol, atol))` for a differentiable PDE solve without densifying |
|  [06]   | `findiff`      | a finite-difference stencil operator wraps into `FunctionLinearOperator`/`TridiagonalLinearOperator` and inherits tag-driven solver selection |

[OPERATOR_ALGEBRA]:
- Operators are pytrees; `a + b`, `a @ b`, `c * a`, `-a` build `AddLinearOperator`/`ComposedLinearOperator`/`MulLinearOperator`/`NegLinearOperator` nodes lazily — no matrix is formed until a solver's `.mv` runs.
- Tag a freshly built operator with `lx.TaggedLinearOperator(op, frozenset({lx.symmetric_tag, lx.positive_semidefinite_tag}))` so `AutoLinearSolver(well_posed=True)` dispatches `Cholesky`; mis-tagging is silently trusted, so tag only proven structure.
- `linearise(JacobianLinearOperator(...))` freezes a nonlinear residual's Jacobian at the current iterate into a reusable linear operator for an outer Newton loop.

[SOLVE_DISPATCH]:
- `AutoLinearSolver(well_posed=True)` reads operator tags to pick `Cholesky` (PSD) → `Triangular` (triangular) → `Tridiagonal` (tridiagonal) → `Diagonal` (diagonal) → `LU` (square) before falling through; `well_posed=False` routes overdetermined to `QR`; `well_posed=None` routes rank-deficient/least-squares to `SVD`.
- `Solution.result` is an `equinox.Enumeration` item: branch with `solution.result == lx.RESULTS.successful`, and read the human message via `lx.RESULTS[solution.result]`. Under `throw=True` (default) a non-successful result raises at the JAX boundary; pass `throw=False` to inspect `result`/`stats` inside a traced study rail. The `EnumerationItem` carries no `.name`, so the member-name key (the receipt-status vocabulary) is recovered by inverting the class `_name_to_item` map off `int(solution.result._value)`. A batched `vmap`/`filter_vmap` solve over a right-hand-side stack aggregates the per-column verdict by `jnp.max` over `solution.result._value` (the sole-zero `successful` makes `max == 0` iff every column converged) plus that `_name_to_item` inversion — the identical worst-case reduction the `optimistix`/`diffrax` `RESULTS` (the same `equinox.Enumeration` base) run; `RESULTS.promote` is inheritance-widening (raising `ValueError` on a same-class member) and `RESULTS.where` is the branchless `jnp.where` select, so neither is the batch combine.
- `Solution.stats` carries iterative-solver telemetry (iteration count, residual norms) and is empty `{}` for direct solvers; capture it as solve-receipt evidence after a `throw=False` call.

[LOCAL_ADMISSION]:
- Build the operator once per problem outside the hot loop; pass it to `linear_solve` inside JIT. Never call `materialise` in a traced path unless the operator is static and small.
- Reach for lineax (not `jnp.linalg.solve` or `scipy.sparse.linalg`) whenever the solve must be differentiated, batched with `vmap`, tag-dispatched, or nested inside an `optimistix`/`diffrax` solver.
- `invert` returns a matrix-free `FunctionLinearOperator`; store and reapply it rather than re-solving when the same operator hits many right-hand sides under autodiff.

[RAIL_LAW]:
- Package: `lineax`
- Owns: JAX-native linear operator abstraction, tag-dispatched direct/iterative linear solvers, and differentiable/batched solve pipelines
- Accept: `AbstractLinearOperator` subclasses as the canonical operator surface; `AbstractLinearSolver` instances as the solver surface; `linear_solve` as the unified entry point
- Reject: `jnp.linalg.solve` or `scipy.sparse.linalg` when differentiable, batched, or structure-tagged solves are required; the deprecated `NormalCG` in new code (use `Normal(CG(...))`)
