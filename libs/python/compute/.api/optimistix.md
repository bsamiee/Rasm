# [PY_COMPUTE_API_OPTIMISTIX]

`optimistix` supplies JAX-native nonlinear solvers for minimization, least squares, root finding, and fixed-point iteration as the solver core of the inverse-design loop. Four unified entry points (`minimise`/`least_squares`/`root_find`/`fixed_point`) take a solver instance plus an arbitrary-PyTree `y0`; the solver type — not the entry point — selects the algorithm. Every solve is JIT-compatible and differentiable through the `adjoint` protocol: `ImplicitAdjoint` backpropagates via the implicit function theorem (one `lineax` linear solve per backward pass), `RecursiveCheckpointAdjoint` through the iteration. It stacks across the JAX numerical rail: inner linear solves dispatch to `lineax` solvers (`linear_solver=`), the objective and `y0` are `equinox` PyTrees, an `optax` optimizer enters as `OptaxMinimiser`, and a differentiable solve composes inside a `diffrax` adjoint. It never re-implements a `lineax` linear solve or an `optax` first-order step the sibling owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optimistix`
- package: `optimistix`
- import: `optimistix` (alias `optx`); submodule `optimistix.compat`
- owner: `compute`
- rail: differentiable nonlinear optimization
- capability: JAX-native nonlinear minimization, least-squares, root-finding, and fixed-point iteration — four unified solve entry points, quasi-Newton/Gauss-Newton/Levenberg-Marquardt/trust-region/Newton/CG/Nelder-Mead/golden-section solvers, composable descent + line-search/trust-region search strategies, `BestSoFar*` solver wrappers, implicit/recursive-checkpoint adjoints, a `RESULTS` termination enum, and a scipy-compatible `minimize` shim

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: minimizer solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [KEY_SIGNATURE]                                                                     |
| :-----: | :---------------- | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BFGS`            | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`                      |
|  [02]   | `DFP`             | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`                      |
|  [03]   | `LBFGS`           | limited-memory BFGS | `(rtol, atol, norm=max_norm, use_inverse=True, history_length=10, verbose=False)`   |
|  [04]   | `GradientDescent` | first-order         | `(learning_rate, rtol, atol, norm=max_norm)`                                        |
|  [05]   | `NonlinearCG`     | nonlinear CG        | `(rtol, atol, norm=max_norm, method=polak_ribiere, search=BacktrackingArmijo(...))` |
|  [06]   | `NelderMead`      | derivative-free     | `(rtol, atol, norm=max_norm, rdelta=0.05, adelta=0.00025)`                          |
|  [07]   | `OptaxMinimiser`  | Optax wrapper       | `(optim, rtol, atol, norm=max_norm, verbose=False)`                                 |

[PUBLIC_TYPE_SCOPE]: root-finder, fixed-point, and 1-D solver types
- rail: differentiable nonlinear optimization
- shared: `Newton` and `Chord` take `(rtol, atol, norm=max_norm, kappa=0.01, linear_solver=AutoLinearSolver(well_posed=None), cauchy_termination=True)`.
- bracket: `Bisection` and `GoldenSearch` carry no constructor bracket and require the entry `options=dict(lower=, upper=)`, and `GoldenSearch` ignores `y0`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [KEY_SIGNATURE]                                                                         |
| :-----: | :-------------------- | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Newton`              | Newton method    | re-linearizes each step; shared signature above                                         |
|  [02]   | `Chord`               | chord method     | holds the initial Jacobian; shared signature above                                      |
|  [03]   | `Bisection`           | 1-D bisection    | `(rtol, atol, flip='detect', expand_if_necessary=False)`; needs entry bracket `options` |
|  [04]   | `GoldenSearch`        | 1-D minimization | `(rtol, atol)`; needs entry bracket `options`, ignores `y0`                             |
|  [05]   | `FixedPointIteration` | Banach iteration | `(rtol, atol, norm=max_norm, damp=0.0)`                                                 |

[PUBLIC_TYPE_SCOPE]: least-squares solver types
- rail: differentiable nonlinear optimization
- shared: every solver takes `(rtol, atol, norm=max_norm, linear_solver=..., verbose=False)`; the row carries its `linear_solver=` default and extras.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [KEY_SIGNATURE]                                            |
| :-----: | :--------------------------- | :------------------ | :--------------------------------------------------------- |
|  [01]   | `GaussNewton`                | Gauss-Newton        | `linear_solver=AutoLinearSolver(well_posed=None)`          |
|  [02]   | `LevenbergMarquardt`         | Levenberg-Marquardt | `linear_solver=QR()`, classical trust region               |
|  [03]   | `IndirectLevenbergMarquardt` | iterative LM        | iterative-solve trust region; hyperparameters in note [03] |
|  [04]   | `Dogleg`                     | dogleg trust-region | `linear_solver=AutoLinearSolver(well_posed=None)`          |

- [03]-[INDIRECTLEVENBERGMARQUARDT]: `lambda_0=1.0`, `linear_solver=AutoLinearSolver(well_posed=False)`, `root_finder=Newton(rtol=0.01, atol=0.01)`.

[PUBLIC_TYPE_SCOPE]: `BestSoFar*` solver wrappers
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [CAPABILITY]                                                        |
| :-----: | :------------------------------ | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `BestSoFarMinimiser(solver)`    | minimiser wrapper   | returns the best (lowest-objective) iterate seen, not the final one |
|  [02]   | `BestSoFarLeastSquares(solver)` | LS wrapper          | best iterate by residual-sum-of-squares                             |
|  [03]   | `BestSoFarRootFinder(solver)`   | root wrapper        | best iterate by residual norm                                       |
|  [04]   | `BestSoFarFixedPoint(solver)`   | fixed-point wrapper | best iterate by fixed-point residual                                |

[PUBLIC_TYPE_SCOPE]: descent strategies, searches, and the tag union
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `BacktrackingArmijo`          | line search   | `(decrease_factor=0.5, slope=0.1, ...)` Armijo backtracking               |
|  [02]   | `LearningRate`                | search        | constant learning-rate step                                               |
|  [03]   | `ClassicalTrustRegion`        | trust region  | classical trust-region acceptance                                         |
|  [04]   | `LinearTrustRegion`           | trust region  | linear-model trust-region acceptance                                      |
|  [05]   | `NewtonDescent`               | descent step  | Newton direction for a search                                             |
|  [06]   | `SteepestDescent`             | descent step  | steepest-descent direction                                                |
|  [07]   | `NonlinearCGDescent`          | descent step  | nonlinear-CG direction                                                    |
|  [08]   | `DoglegDescent`               | descent step  | dogleg direction                                                          |
|  [09]   | `DampedNewtonDescent`         | descent step  | LM-style damped Newton, `linear_solver=AutoLinearSolver(well_posed=None)` |
|  [10]   | `IndirectDampedNewtonDescent` | descent step  | iterative-solve damped Newton                                             |
|  [11]   | `FunctionInfo`                | tag union     | descent-input tags in note [11]                                           |

- [11]-[FUNCTIONINFO]: descent-input tags `Eval`/`EvalGrad`/`EvalGradHessian`/`EvalGradHessianInv`/`Residual`/`ResidualJac`.

[PUBLIC_TYPE_SCOPE]: abstract solver and strategy bases
- rail: differentiable nonlinear optimization
- Subclass an abstract base to author a custom solver, descent, search, or adjoint.

| [INDEX] | [BASE_ROLE]    | [BASES]                                                                                             |
| :-----: | :------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | problem-class  | `AbstractMinimiser`, `AbstractLeastSquaresSolver`, `AbstractRootFinder`, `AbstractFixedPointSolver` |
|  [02]   | quasi-Newton   | `AbstractQuasiNewton`, `AbstractBFGS`, `AbstractDFP`, `AbstractLBFGS`                               |
|  [03]   | GN / GD        | `AbstractGaussNewton`, `AbstractGradientDescent`                                                    |
|  [04]   | strategy hooks | `AbstractDescent`, `AbstractSearch`, `AbstractAdjoint`, `AbstractIterativeSolver`                   |

[PUBLIC_TYPE_SCOPE]: adjoint and result types
- rail: differentiable nonlinear optimization
- `Solution` is generic over `Y`/`Aux`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [CAPABILITY]                                                                   |
| :-----: | :--------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `Solution`                   | result carrier   | `value: Y`, `result: RESULTS`, `aux: Aux`, `stats: dict[str, PyTree]`, `state` |
|  [02]   | `RESULTS`                    | termination enum | member vocabulary in the fence below; `successful` is the zero code            |
|  [03]   | `ImplicitAdjoint`            | adjoint          | `(linear_solver=AutoLinearSolver(well_posed=None))`, implicit-function-theorem |
|  [04]   | `RecursiveCheckpointAdjoint` | adjoint          | `(checkpoints=None)`, backprop through iterations with checkpointing           |

```python signature
# optimistix.RESULTS members (equinox.Enumeration); successful is the zero code
RESULTS.successful, RESULTS.max_steps_reached, RESULTS.nonlinear_max_steps_reached,
RESULTS.nonlinear_divergence, RESULTS.singular, RESULTS.breakdown, RESULTS.stagnation,
RESULTS.nonfinite, RESULTS.conlim, RESULTS.nonfinite_input
```

- [02]-[RESULTS]: each member is an `equinox.EnumerationItem` exposing only `_value` (int code) and `_enumeration` — no `.name`/`.value`; `RESULTS[item]` returns the human message and the member-name key inverts the class `_name_to_item` map.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified solve entry points
- rail: differentiable nonlinear optimization
- shared: every entry point has `(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` -> `Solution[Y, Aux]`; only `fn`'s shape differs.

| [INDEX] | [SURFACE]       | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :-------------- | :------------- | :---------------------------------- |
|  [01]   | `minimise`      | minimise       | scalar `fn(y, args) -> scalar`      |
|  [02]   | `least_squares` | least squares  | residual `fn(y, args) -> residuals` |
|  [03]   | `root_find`     | root find      | `fn(y, args) -> 0`                  |
|  [04]   | `fixed_point`   | fixed point    | `fn(y, args) -> y`                  |

[ENTRYPOINT_SCOPE]: norm functions, CG β-coefficients, and the scipy-compat shim
- rail: differentiable nonlinear optimization
- shared: the norms `max_norm`/`rms_norm`/`two_norm` take `(PyTree[Array]) → Shaped[Array, '']`; the β-coefficients take `(grad, grad_prev, y_diff) → scalar`.

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]   | [RAIL]                                                                       |
| :-----: | :--------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `max_norm`             | termination norm | Chebyshev (L∞) norm                                                          |
|  [02]   | `rms_norm`             | termination norm | RMS norm                                                                     |
|  [03]   | `two_norm`             | termination norm | Euclidean (L2) norm                                                          |
|  [04]   | `polak_ribiere`        | CG β-coefficient | Polak-Ribiere, feeds `NonlinearCG(method=)`                                  |
|  [05]   | `fletcher_reeves`      | CG β-coefficient | Fletcher-Reeves                                                              |
|  [06]   | `hestenes_stiefel`     | CG β-coefficient | Hestenes-Stiefel                                                             |
|  [07]   | `dai_yuan`             | CG β-coefficient | Dai-Yuan                                                                     |
|  [08]   | `compat.minimize(...)` | scipy shim       | scipy `minimize`-compatible over `BFGS`/`NelderMead`; signature in note [08] |

- [08]-[compat.minimize]: `compat.minimize(fun, x0, args=(), *, method, tol=None, options=None)` → `compat.OptimizeResults`.

## [04]-[IMPLEMENTATION_LAW]

[OPTIMIZATION_TOPOLOGY]:
- namespace: `optimistix`; all solver types, search/descent strategies, abstract bases, and the four solve functions at top level; `compat` is the scipy-compatible submodule.
- four unified solve functions cover every nonlinear problem class; the solver instance determines the algorithm, not the entry point.
- `fn` signature: `minimise` is `(y, args) -> scalar` (or `(scalar, aux)` when `has_aux=True`); `least_squares`/`root_find` are `(y, args) -> residuals`; `fixed_point` is `(y, args) -> y`. `y0` and `args` are arbitrary JAX PyTrees and the solver tracks the structure of `y` throughout iteration.
- `Solution.result` carries a `RESULTS` enum value; `throw=True` (default) raises on any non-`successful` value, otherwise the caller inspects `result` (`RESULTS.successful` is the zero code). A batched `vmap`/`filter_vmap` sweep aggregates the per-start codes by `jnp.max` over `solution.result._value` (the sole-zero `successful` makes `max == 0` iff every start converged) and recovers the worst-case member name by inverting the class `_name_to_item` map; `RESULTS.promote` is inheritance-widening (raising `ValueError` on a same-class member) and `RESULTS.where` is the branchless `jnp.where` select, so neither is the batch combine.
- `max_steps` is a static integer; changing it recompiles under JIT. `Solution.stats` carries step/evaluation counts; `Solution.aux` carries caller auxiliary data when `has_aux=True`, else `None`.
- adjoint: `ImplicitAdjoint` computes gradients via the implicit function theorem (one `lineax` solve per backward, for a well-posed converged solve); `RecursiveCheckpointAdjoint` backpropagates through the iterations when the implicit form is unavailable.

[INTEGRATION_LAW]:
- linear-solve stacking: Newton/Chord/GaussNewton/LM/Dogleg/DampedNewtonDescent take `linear_solver=` from `lineax` (`AutoLinearSolver`, `QR`, `NormalCG`, `GMRES`, `CG`); the inner linear system is a `lineax` solve, not a hand-rolled factorization, and `IndirectLevenbergMarquardt` further threads a `root_finder=Newton(...)` for the trust-region radius.
- optax stacking: `OptaxMinimiser(optim, rtol, atol)` lifts any `optax` `GradientTransformation` into an `AbstractMinimiser`, so a custom `optax.chain` runs inside `minimise` with the full `Solution`/`RESULTS`/adjoint machinery instead of a manual training loop.
- bracket options: the `options=None` slot on every entry is per-solve runtime state forwarded to the solver, distinct from the constructor hyperparameters. The two 1-D bracketing solvers REQUIRE it: `Bisection` (`root_find`) and `GoldenSearch` (`minimise`) both take `options=dict(lower=, upper=)` bracketing the root/minimum, e.g. `root_find(..., options=dict(lower=0, upper=1))`; `GoldenSearch` further IGNORES `y0` (the golden-ratio split is the sole region specifier), so the bracket is its only region argument. Every other solver runs with `options=None`, so the bracketing pair is the lone `options` consumer in this surface and a `Bisection`/`GoldenSearch` solve passing no `options` raises inside the solver.
- carrier: `y0`, `args`, and `fn`'s output are `equinox` PyTrees, so a parametrized `equinox.Module` is optimized directly; the objective uses `equinox.filter_jit`/`filter_value_and_grad`, and static config leaves are partitioned out before the solve.
- differentiable composition: a converged `optimistix` solve under `ImplicitAdjoint` is itself differentiable, so it nests inside a `diffrax.diffeqsolve` adjoint or an outer `optax` design loop — the inverse-design gradient flows through the inner solve without unrolling it.
- solver upcasting: `least_squares` accepts an `AbstractMinimiser` (residual norm becomes the objective); `root_find`/`fixed_point` accept least-squares or minimiser solvers when the problem class permits; a `BestSoFar*` wrapper guards against a final non-monotone iterate.

[LOCAL_ADMISSION]:
- solver instances are constructed outside JIT with their hyperparameters and are not recompiled per call; the line-search/descent/trust-region strategy is composed into the solver, not re-derived per step.
- inner linear solves route to `lineax`; re-implementing a Krylov or QR solve locally is rejected.
- a first-order `optax` step enters through `OptaxMinimiser`; constructing a parallel hand-written descent loop beside the unified solve is rejected.

[RAIL_LAW]:
- Package: `optimistix`
- Owns: JAX-native nonlinear minimization, least-squares, root-finding, and fixed-point iteration with composable descent/search strategies and differentiable implicit/recursive-checkpoint adjoints
- Accept: `minimise`/`least_squares`/`root_find`/`fixed_point` as the canonical solve entry points; `lineax` solvers as `linear_solver=`; an `optax` optimizer via `OptaxMinimiser`; an `equinox` PyTree as `y0`/`args`; a `Solution` with a `RESULTS` receipt
- Reject: `scipy.optimize` (use `compat.minimize` only at a non-JAX boundary) when JAX autodiff, JIT, or PyTree inputs are required; a hand-rolled inner linear solve or first-order loop the siblings own
