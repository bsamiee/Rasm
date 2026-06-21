# [PY_COMPUTE_API_OPTIMISTIX]

`optimistix` supplies JAX-native nonlinear solvers for minimization, least squares, root finding, and fixed-point iteration as the solver core of the inverse-design loop. Four unified entry points (`minimise`/`least_squares`/`root_find`/`fixed_point`) take a solver instance plus an arbitrary-PyTree `y0`; the solver type — not the entry point — selects the algorithm. Every solve is JIT-compatible and differentiable through the `adjoint` protocol: `ImplicitAdjoint` backpropagates via the implicit function theorem (one `lineax` linear solve per backward pass), `RecursiveCheckpointAdjoint` through the iteration. It stacks across the JAX numerical rail: inner linear solves dispatch to `lineax` solvers (`linear_solver=`), the objective and `y0` are `equinox` PyTrees, an `optax` optimizer enters as `OptaxMinimiser`, and a differentiable solve composes inside a `diffrax` adjoint. It never re-implements a `lineax` linear solve or an `optax` first-order step the sibling owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optimistix`
- package: `optimistix`
- import: `optimistix` (alias `optx`); submodule `optimistix.compat`
- owner: `compute`
- rail: differentiable nonlinear optimization
- installed: license Apache-2.0; `Requires-Python ~=3.11` (floor 3.11); `optimistix` wheel is `py3-none-any` (pure Python); marker-gated `python_version<'3.15'` in the manifest because the transitive `jaxlib` backend ships no cp315 wheel — the gate is jaxlib, not optimistix — so `assay api resolve optimistix` is `unsupported` on the cp315 core (uninstalled, marker-gated); surface confirmed against the optimistix 0.1.x module API
- capability: JAX-native nonlinear minimization, least-squares, root-finding, and fixed-point iteration — four unified solve entry points, quasi-Newton/Gauss-Newton/Levenberg-Marquardt/trust-region/Newton/CG/Nelder-Mead/golden-section solvers, composable descent + line-search/trust-region search strategies, `BestSoFar*` solver wrappers, implicit/recursive-checkpoint adjoints, a `RESULTS` termination enum, and a scipy-compatible `minimize` shim

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: minimizer solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [KEY_SIGNATURE]                                                            |
| :-----: | :---------------- | :------------------ | :------------------------------------------------------------------------- |
|  [01]   | `BFGS`            | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`             |
|  [02]   | `DFP`             | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`             |
|  [03]   | `LBFGS`           | limited-memory BFGS | `(rtol, atol, norm=max_norm, use_inverse=True, history_length=10, verbose=False)` |
|  [04]   | `GradientDescent` | first-order         | `(learning_rate, rtol, atol, norm=max_norm)`                              |
|  [05]   | `NonlinearCG`     | nonlinear CG        | `(rtol, atol, norm=max_norm, method=polak_ribiere, search=BacktrackingArmijo(...))` |
|  [06]   | `NelderMead`      | derivative-free     | `(rtol, atol, norm=max_norm, rdelta=0.05, adelta=0.00025)`                 |
|  [07]   | `OptaxMinimiser`  | Optax wrapper       | `(optim, rtol, atol, norm=max_norm, verbose=False)` — wraps any `optax` optimizer as an `AbstractMinimiser` |

[PUBLIC_TYPE_SCOPE]: root-finder, fixed-point, and 1-D solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [KEY_SIGNATURE]                                                                          |
| :-----: | :-------------------- | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Newton`              | Newton method    | `(rtol, atol, norm=max_norm, kappa=0.01, linear_solver=AutoLinearSolver(well_posed=None), cauchy_termination=True)` |
|  [02]   | `Chord`               | chord method     | `(rtol, atol, norm=max_norm, kappa=0.01, linear_solver=AutoLinearSolver(well_posed=None), cauchy_termination=True)` |
|  [03]   | `Bisection`           | 1-D bisection    | `(rtol, atol, flip='detect', expand_if_necessary=False)`                                 |
|  [04]   | `GoldenSearch`        | 1-D minimization | `(rtol, atol)` — golden-section bracketing minimiser                                     |
|  [05]   | `FixedPointIteration` | Banach iteration | `(rtol, atol, norm=max_norm, damp=0.0)`                                                   |

[PUBLIC_TYPE_SCOPE]: least-squares solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]         | [KEY_SIGNATURE]                                                                          |
| :-----: | :--------------------------- | :-------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `GaussNewton`                | Gauss-Newton          | `(rtol, atol, norm=max_norm, linear_solver=AutoLinearSolver(well_posed=None), verbose=False)` |
|  [02]   | `LevenbergMarquardt`         | Levenberg-Marquardt   | `(rtol, atol, norm=max_norm, linear_solver=QR(), verbose=False)` (classical trust region) |
|  [03]   | `IndirectLevenbergMarquardt` | iterative LM          | `(rtol, atol, norm=max_norm, lambda_0=1.0, linear_solver=AutoLinearSolver(well_posed=False), root_finder=Newton(rtol=0.01, atol=0.01), verbose=False)` |
|  [04]   | `Dogleg`                     | dogleg trust-region   | `(rtol, atol, norm=max_norm, linear_solver=AutoLinearSolver(well_posed=None), verbose=False)` |

[PUBLIC_TYPE_SCOPE]: `BestSoFar*` solver wrappers
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                                            |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `BestSoFarMinimiser(solver)`   | minimiser wrapper | returns the best (lowest-objective) iterate seen, not the final one |
|  [02]   | `BestSoFarLeastSquares(solver)` | LS wrapper      | best iterate by residual-sum-of-squares                              |
|  [03]   | `BestSoFarRootFinder(solver)`  | root wrapper    | best iterate by residual norm                                        |
|  [04]   | `BestSoFarFixedPoint(solver)`  | fixed-point wrapper | best iterate by fixed-point residual                             |

[PUBLIC_TYPE_SCOPE]: descent strategies, searches, and abstract bases
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :----------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `BacktrackingArmijo`           | line search   | `(decrease_factor=0.5, slope=0.1, ...)` Armijo backtracking         |
|  [02]   | `LearningRate`                 | search        | constant learning-rate step                                        |
|  [03]   | `ClassicalTrustRegion` / `LinearTrustRegion` | trust region | classical / linear-model trust-region acceptance      |
|  [04]   | `NewtonDescent` / `SteepestDescent` / `NonlinearCGDescent` / `DoglegDescent` | descent step | direction generators composed with a search |
|  [05]   | `DampedNewtonDescent(linear_solver=AutoLinearSolver(well_posed=None))` / `IndirectDampedNewtonDescent` | descent step | LM-style damped Newton (direct / iterative-solve) |
|  [06]   | `FunctionInfo`                 | tag union     | descent-input tags `Eval`/`EvalGrad`/`EvalGradHessian`/`EvalGradHessianInv`/`Residual`/`ResidualJac` |
|  [07]   | `AbstractMinimiser` / `AbstractLeastSquaresSolver` / `AbstractRootFinder` / `AbstractFixedPointSolver` | solver base | the four problem-class solver bases (subclass to author a custom solver) |
|  [08]   | `AbstractQuasiNewton` / `AbstractBFGS` / `AbstractDFP` / `AbstractLBFGS` / `AbstractGaussNewton` / `AbstractGradientDescent` | solver base | quasi-Newton/GN/GD specializations |
|  [09]   | `AbstractDescent` / `AbstractSearch` / `AbstractAdjoint` / `AbstractIterativeSolver` | strategy base | descent/search/adjoint/iterative-solver extension points |

[PUBLIC_TYPE_SCOPE]: adjoint and result types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                      |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Solution`                   | result carrier | fields `value: Y`, `result: RESULTS`, `aux: Aux`, `stats: dict[str, PyTree]`, `state` (generic over `Y`/`Aux`)   |
|  [02]   | `RESULTS`                    | termination enum (equinox enumeration) | `successful`, `max_steps_reached`, `nonlinear_max_steps_reached`, `nonlinear_divergence`, `singular`, `breakdown`, `stagnation`, `nonfinite`, `conlim`, `nonfinite_input`; `RESULTS.promote`/`RESULTS.where` combine codes; carries a human-readable message per case |
|  [03]   | `ImplicitAdjoint`            | adjoint        | `(linear_solver=AutoLinearSolver(well_posed=None))` — implicit-function-theorem gradients (one `lineax` solve per backward) |
|  [04]   | `RecursiveCheckpointAdjoint` | adjoint        | `(checkpoints=None)` — backpropagation through the solver iterations with checkpointing                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: unified solve entry points
- rail: differentiable nonlinear optimization

| [INDEX] | [SURFACE]                                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `minimise(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution[Y, Aux]` | minimise       | scalar `fn(y, args) -> scalar` |
|  [02]   | `least_squares(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution[Y, Aux]` | least squares | residual `fn(y, args) -> residuals` |
|  [03]   | `root_find(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution[Y, Aux]` | root find      | `fn(y, args) -> 0`             |
|  [04]   | `fixed_point(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution[Y, Aux]` | fixed point    | `fn(y, args) -> y`             |

[ENTRYPOINT_SCOPE]: norm functions, CG β-coefficients, and the scipy-compat shim
- rail: differentiable nonlinear optimization

| [INDEX] | [SURFACE]              | [SIGNATURE / CAPABILITY]                          | [RAIL]                |
| :-----: | :--------------------- | :------------------------------------------------ | :-------------------- |
|  [01]   | `max_norm(x)` / `rms_norm(x)` / `two_norm(x)` | `(PyTree[Array]) → Shaped[Array, '']` | Chebyshev (L∞) / RMS / Euclidean (L2) termination norm |
|  [02]   | `polak_ribiere` / `fletcher_reeves` / `hestenes_stiefel` / `dai_yuan` | `(grad, grad_prev, y_diff) → scalar` | nonlinear-CG β-coefficient (the `NonlinearCG(method=)`) |
|  [03]   | `compat.minimize(fun, x0, args=(), *, method, tol=None, options=None)` → `compat.OptimizeResults` | scipy-`optimize.minimize`-compatible signature over `BFGS`/`NelderMead` |

## [04]-[IMPLEMENTATION_LAW]

[OPTIMIZATION_TOPOLOGY]:
- namespace: `optimistix`; all solver types, search/descent strategies, abstract bases, and the four solve functions at top level; `compat` is the scipy-compatible submodule.
- four unified solve functions cover every nonlinear problem class; the solver instance determines the algorithm, not the entry point.
- `fn` signature: `minimise` is `(y, args) -> scalar` (or `(scalar, aux)` when `has_aux=True`); `least_squares`/`root_find` are `(y, args) -> residuals`; `fixed_point` is `(y, args) -> y`. `y0` and `args` are arbitrary JAX PyTrees and the solver tracks the structure of `y` throughout iteration.
- `Solution.result` carries a `RESULTS` enum value; `throw=True` (default) raises on any non-`successful` value, otherwise the caller inspects `result` (`RESULTS.successful` is the zero code, `RESULTS.where`/`.promote` combine codes across a `vmap`).
- `max_steps` is a static integer; changing it recompiles under JIT. `Solution.stats` carries step/evaluation counts; `Solution.aux` carries caller auxiliary data when `has_aux=True`, else `None`.
- adjoint: `ImplicitAdjoint` computes gradients via the implicit function theorem (one `lineax` solve per backward, for a well-posed converged solve); `RecursiveCheckpointAdjoint` backpropagates through the iterations when the implicit form is unavailable.

[INTEGRATION_LAW]:
- linear-solve stacking: Newton/Chord/GaussNewton/LM/Dogleg/DampedNewtonDescent take `linear_solver=` from `lineax` (`AutoLinearSolver`, `QR`, `NormalCG`, `GMRES`, `CG`); the inner linear system is a `lineax` solve, not a hand-rolled factorization, and `IndirectLevenbergMarquardt` further threads a `root_finder=Newton(...)` for the trust-region radius.
- optax stacking: `OptaxMinimiser(optim, rtol, atol)` lifts any `optax` `GradientTransformation` into an `AbstractMinimiser`, so a custom `optax.chain` runs inside `minimise` with the full `Solution`/`RESULTS`/adjoint machinery instead of a manual training loop.
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
