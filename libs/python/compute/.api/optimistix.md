# [PY_COMPUTE_API_OPTIMISTIX]

`optimistix` owns JAX-native nonlinear solving — minimisation, least squares, root finding, and fixed-point iteration — behind four unified entry points where the solver instance, never the entry point, selects the algorithm. Every solve compiles under `jax.jit`, batches under `vmap`, and differentiates through `ImplicitAdjoint` (one `lineax` solve per backward) or `RecursiveCheckpointAdjoint`, so a converged solve nests inside a `diffrax` adjoint or an outer `optax` design loop without unrolling.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optimistix`
- package: `optimistix`
- import: `optimistix` (alias `optx`); submodule `optimistix.compat`
- owner: `compute`
- rail: differentiable nonlinear optimization
- capability: unified minimise/least-squares/root-find/fixed-point solving with composable descent and line-search/trust-region strategies, `BestSoFar*` wrappers, and implicit/recursive-checkpoint adjoints

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: minimizer solver types

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [SIGNATURE]                                                                         |
| :-----: | :---------------- | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BFGS`            | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`                      |
|  [02]   | `DFP`             | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`                      |
|  [03]   | `LBFGS`           | limited-memory BFGS | `(rtol, atol, norm=max_norm, use_inverse=True, history_length=10, verbose=False)`   |
|  [04]   | `GradientDescent` | first-order         | `(learning_rate, rtol, atol, norm=max_norm)`                                        |
|  [05]   | `NonlinearCG`     | nonlinear CG        | `(rtol, atol, norm=max_norm, method=polak_ribiere, search=BacktrackingArmijo(...))` |
|  [06]   | `NelderMead`      | derivative-free     | `(rtol, atol, norm=max_norm, rdelta=0.05, adelta=0.00025)`                          |
|  [07]   | `OptaxMinimiser`  | Optax wrapper       | `(optim, rtol, atol, norm=max_norm, verbose=False)`                                 |

[PUBLIC_TYPE_SCOPE]: root-finder, fixed-point, and 1-D solver types
- shared: `Newton` and `Chord` take `(rtol, atol, norm=max_norm, kappa=0.01, linear_solver=AutoLinearSolver(well_posed=None), cauchy_termination=True)`.
- bracket: `Bisection` and `GoldenSearch` carry no constructor bracket and require the entry `options=dict(lower=, upper=)`; `GoldenSearch` ignores `y0`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [SIGNATURE]                                                                             |
| :-----: | :-------------------- | :--------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Newton`              | Newton method    | re-linearizes each step; shared signature above                                         |
|  [02]   | `Chord`               | chord method     | holds the initial Jacobian; shared signature above                                      |
|  [03]   | `Bisection`           | 1-D bisection    | `(rtol, atol, flip='detect', expand_if_necessary=False)`; needs entry bracket `options` |
|  [04]   | `GoldenSearch`        | 1-D minimization | `(rtol, atol)`; needs entry bracket `options`, ignores `y0`                             |
|  [05]   | `FixedPointIteration` | Banach iteration | `(rtol, atol, norm=max_norm, damp=0.0)`                                                 |

[PUBLIC_TYPE_SCOPE]: least-squares solver types
- shared: every solver takes `(rtol, atol, norm=max_norm, linear_solver=..., verbose=False)`; each row carries its `linear_solver=` default and extras.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [SIGNATURE]                                       |
| :-----: | :--------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `GaussNewton`                | Gauss-Newton        | `linear_solver=AutoLinearSolver(well_posed=None)` |
|  [02]   | `LevenbergMarquardt`         | Levenberg-Marquardt | `linear_solver=QR()`, classical trust region      |
|  [03]   | `IndirectLevenbergMarquardt` | iterative LM        | iterative-solve trust region; note [03]           |
|  [04]   | `Dogleg`                     | dogleg trust-region | `linear_solver=AutoLinearSolver(well_posed=None)` |

- [03]-[INDIRECTLEVENBERGMARQUARDT]: `lambda_0=1.0`, `linear_solver=AutoLinearSolver(well_posed=False)`, `root_finder=Newton(rtol=0.01, atol=0.01)`.

[PUBLIC_TYPE_SCOPE]: `BestSoFar*` solver wrappers

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [CAPABILITY]                                                        |
| :-----: | :------------------------------ | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `BestSoFarMinimiser(solver)`    | minimiser wrapper   | returns the best (lowest-objective) iterate seen, not the final one |
|  [02]   | `BestSoFarLeastSquares(solver)` | LS wrapper          | best iterate by residual-sum-of-squares                             |
|  [03]   | `BestSoFarRootFinder(solver)`   | root wrapper        | best iterate by residual norm                                       |
|  [04]   | `BestSoFarFixedPoint(solver)`   | fixed-point wrapper | best iterate by fixed-point residual                                |

[PUBLIC_TYPE_SCOPE]: descent strategies, searches, and the tag union

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
- Subclass an abstract base to author a custom solver, descent, search, or adjoint.

| [INDEX] | [BASE_ROLE]    | [BASES]                                                                                             |
| :-----: | :------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | problem-class  | `AbstractMinimiser`, `AbstractLeastSquaresSolver`, `AbstractRootFinder`, `AbstractFixedPointSolver` |
|  [02]   | quasi-Newton   | `AbstractQuasiNewton`, `AbstractBFGS`, `AbstractDFP`, `AbstractLBFGS`                               |
|  [03]   | GN / GD        | `AbstractGaussNewton`, `AbstractGradientDescent`                                                    |
|  [04]   | strategy hooks | `AbstractDescent`, `AbstractSearch`, `AbstractAdjoint`, `AbstractIterativeSolver`                   |

[PUBLIC_TYPE_SCOPE]: adjoint and result types
- `Solution` is generic over `Y`/`Aux`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [CAPABILITY]                                                                   |
| :-----: | :--------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `Solution`                   | result carrier   | `value: Y`, `result: RESULTS`, `aux: Aux`, `stats: dict[str, PyTree]`, `state` |
|  [02]   | `RESULTS`                    | termination enum | `equinox.Enumeration`; member roster below, `successful` the zero code         |
|  [03]   | `ImplicitAdjoint`            | adjoint          | `(linear_solver=AutoLinearSolver(well_posed=None))`, implicit-function-theorem |
|  [04]   | `RecursiveCheckpointAdjoint` | adjoint          | `(checkpoints=None)`, backprop through iterations with checkpointing           |

[RESULTS_ITEMS]: `successful` (zero code) `max_steps_reached` `nonlinear_max_steps_reached` `nonlinear_divergence` `singular` `breakdown` `stagnation` `nonfinite` `conlim` `nonfinite_input` — the termination vocabulary a `Solution.result` carries, read to a message via `RESULTS[item]`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified solve entry points
- shared: every entry has `(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` -> `Solution[Y, Aux]`; only `fn`'s shape differs.

| [INDEX] | [SURFACE]       | [SHAPE]       | [CAPABILITY]                        |
| :-----: | :-------------- | :------------ | :---------------------------------- |
|  [01]   | `minimise`      | minimise      | scalar `fn(y, args) -> scalar`      |
|  [02]   | `least_squares` | least squares | residual `fn(y, args) -> residuals` |
|  [03]   | `root_find`     | root find     | `fn(y, args) -> 0`                  |
|  [04]   | `fixed_point`   | fixed point   | `fn(y, args) -> y`                  |

[ENTRYPOINT_SCOPE]: norm functions, CG β-coefficients, and the scipy-compat shim
- shared: the norms `max_norm`/`rms_norm`/`two_norm` take `(PyTree[Array]) -> Shaped[Array, '']`; the β-coefficients take `(grad, grad_prev, y_diff) -> scalar`.

| [INDEX] | [SURFACE]              | [SHAPE]          | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `max_norm`             | termination norm | Chebyshev (L∞) norm                                             |
|  [02]   | `rms_norm`             | termination norm | RMS norm                                                        |
|  [03]   | `two_norm`             | termination norm | Euclidean (L2) norm                                             |
|  [04]   | `polak_ribiere`        | CG β-coefficient | Polak-Ribiere, feeds `NonlinearCG(method=)`                     |
|  [05]   | `fletcher_reeves`      | CG β-coefficient | Fletcher-Reeves                                                 |
|  [06]   | `hestenes_stiefel`     | CG β-coefficient | Hestenes-Stiefel                                                |
|  [07]   | `dai_yuan`             | CG β-coefficient | Dai-Yuan                                                        |
|  [08]   | `compat.minimize(...)` | scipy shim       | scipy `minimize`-compatible over `BFGS`/`NelderMead`; note [08] |

- [08]-[compat.minimize]: `compat.minimize(fun, x0, args=(), *, method, tol=None, options=None)` -> `compat.OptimizeResults`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `fn` shape per entry: `minimise` is `(y, args) -> scalar` (or `(scalar, aux)` under `has_aux=True`), `least_squares`/`root_find` are `(y, args) -> residuals`, `fixed_point` is `(y, args) -> y`; `y0`/`args` are arbitrary JAX PyTrees the solver tracks through iteration.
- `Solution.result` carries a `RESULTS` value; `throw=True` raises on any non-`successful` code (the zero code), else the caller inspects `result`; a batched `vmap`/`filter_vmap` sweep aggregates per-start codes by `jnp.max` over `result._value`, so `max == 0` iff every start converged.
- `max_steps` is static — changing it recompiles under JIT; `Solution.stats` carries step/evaluation counts and `Solution.aux` the caller data under `has_aux=True`, else `None`.
- adjoint: `ImplicitAdjoint` differentiates via the implicit-function theorem (one `lineax` solve per backward on a well-posed converged solve); `RecursiveCheckpointAdjoint` backpropagates through the iterations when the implicit form is unavailable.
- bracketing: `Bisection` (`root_find`) and `GoldenSearch` (`minimise`) take `options=dict(lower=, upper=)` as their sole region, so passing no `options` raises inside the solver; every other solver runs `options=None`, and `GoldenSearch` further ignores `y0`.
- solver upcasting: `least_squares` accepts an `AbstractMinimiser` (residual norm becomes the objective), `root_find`/`fixed_point` accept least-squares or minimiser solvers where the class permits, and a `BestSoFar*` wrapper guards a non-monotone final iterate.

[STACKING]:
- `lineax`(`.api/lineax.md`): `Newton`/`Chord`/`GaussNewton`/LM/`Dogleg`/`DampedNewtonDescent` take a `lineax` solver as `linear_solver=` (`AutoLinearSolver`, `QR`, `NormalCG`, `GMRES`, `CG`) through the `AbstractLinearSolver` protocol, so the inner system is a `lineax` solve; `IndirectLevenbergMarquardt` further threads `root_finder=Newton(...)` for the trust-region radius.
- `optax`(`.api/optax.md`): `OptaxMinimiser(optim, rtol, atol)` lifts any `optax` `GradientTransformation` into an `AbstractMinimiser`, so an `optax.chain` runs inside `minimise` with the full `Solution`/`RESULTS`/adjoint machinery.
- `equinox`(`.api/equinox.md`): `y0`/`args`/`fn` output are `eqx.Module` PyTrees, so a parametrized module optimises directly under `eqx.filter_jit`/`filter_value_and_grad` with static leaves partitioned out; `RESULTS` is an `eqx.Enumeration` whose `EnumerationItem` mechanics — `_value`/`_enumeration` access, `RESULTS[item]` message, `_name_to_item` name recovery, `jnp.max` verdict reduction — are owned there.
- `diffrax`(`.api/diffrax.md`): a converged `ImplicitAdjoint` solve is itself differentiable, so it nests inside a `diffrax.diffeqsolve` adjoint and the inverse-design gradient flows through the inner solve without unrolling it.

[LOCAL_ADMISSION]:
- construct solver instances outside JIT with their hyperparameters, so no per-call recompile fires; the line-search/descent/trust-region strategy composes into the solver.
- inner linear solves route to `lineax`; a hand-rolled Krylov or QR solve is rejected.
- a first-order `optax` step enters through `OptaxMinimiser`; a parallel hand-written descent loop beside the unified solve is rejected.

[RAIL_LAW]:
- Package: `optimistix`
- Owns: JAX-native nonlinear minimization, least-squares, root-finding, and fixed-point iteration with composable descent/search strategies and differentiable implicit/recursive-checkpoint adjoints
- Accept: `minimise`/`least_squares`/`root_find`/`fixed_point` as the solve entries, a `lineax` solver as `linear_solver=`, an `optax` optimizer via `OptaxMinimiser`, an `equinox` PyTree as `y0`/`args`, a `Solution` carrying a `RESULTS` receipt
- Reject: `scipy.optimize` where JAX autodiff, JIT, or PyTree inputs are required (`compat.minimize` bridges only at a non-JAX boundary); a hand-rolled inner linear solve or first-order loop the siblings own
