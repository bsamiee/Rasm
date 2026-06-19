# [PY_COMPUTE_API_OPTIMISTIX]

`optimistix` supplies JAX-native nonlinear solvers for minimization, least squares, root finding, and fixed-point iteration; every solver is JIT-compatible, supports forward/reverse autodiff via the `adjoint` protocol, and composes with `lineax` for inner linear solves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optimistix`
- package: `optimistix`
- module: `optimistix`
- asset: runtime library (JAX extension)
- rail: differentiable nonlinear optimization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: minimizer solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [KEY_SIGNATURE]                                                    |
| :-----: | :---------------- | :------------------ | :----------------------------------------------------------------- |
|  [01]   | `BFGS`            | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`     |
|  [02]   | `DFP`             | quasi-Newton        | `(rtol, atol, norm=max_norm, use_inverse=True, verbose=False)`     |
|  [03]   | `LBFGS`           | limited-memory BFGS | `(rtol, atol, norm=max_norm, memory=10, verbose=False)`            |
|  [04]   | `GradientDescent` | first-order         | `(learning_rate, rtol, atol, norm=max_norm)`                       |
|  [05]   | `NonlinearCG`     | nonlinear CG        | `(rtol, atol, method=polak_ribiere, norm=max_norm, verbose=False)` |
|  [06]   | `NelderMead`      | derivative-free     | `(rtol, atol, norm=max_norm, rdelta=0.05, adelta=0.00025)`         |
|  [07]   | `OptaxMinimiser`  | Optax wrapper       | wraps any Optax optimizer as a minimiser                           |

[PUBLIC_TYPE_SCOPE]: root-finder and fixed-point solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [KEY_SIGNATURE]                                                                                          |
| :-----: | :-------------------- | :--------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Newton`              | Newton method    | `(rtol, atol, norm=max_norm, kappa=0.01, linear_solver=AutoLinearSolver(None), cauchy_termination=True)` |
|  [02]   | `Bisection`           | bisection        | `(rtol, atol, flip='detect', expand_if_necessary=False)`                                                 |
|  [03]   | `Chord`               | chord method     | `(rtol, atol, norm=max_norm, linear_solver=AutoLinearSolver(None))`                                      |
|  [04]   | `FixedPointIteration` | Banach iteration | `(rtol, atol, norm=max_norm, damp=0.0)`                                                                  |

[PUBLIC_TYPE_SCOPE]: least-squares solver types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]         | [KEY_SIGNATURE]                                                        |
| :-----: | :--------------------------- | :-------------------- | :--------------------------------------------------------------------- |
|  [01]   | `GaussNewton`                | Gauss-Newton          | `(rtol, atol, norm=max_norm, linear_solver=QR(), verbose=False)`       |
|  [02]   | `LevenbergMarquardt`         | Levenberg-Marquardt   | `(rtol, atol, norm=max_norm, linear_solver=QR(), verbose=False)`       |
|  [03]   | `IndirectLevenbergMarquardt` | iterative LM          | `(rtol, atol, norm=max_norm, linear_solver=GMRES(...), verbose=False)` |
|  [04]   | `DampedNewtonDescent`        | damped-Newton descent | `(rtol, atol, norm=max_norm, linear_solver=AutoLinearSolver(None))`    |
|  [05]   | `Dogleg`                     | dogleg trust-region   | `(rtol, atol, norm=max_norm, linear_solver=QR(), verbose=False)`       |

[PUBLIC_TYPE_SCOPE]: descent strategies and line searches
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :---------------------------- | :------------ | :------------------------------------ |
|  [01]   | `BacktrackingArmijo`          | line search   | Armijo backtracking line search       |
|  [02]   | `LearningRate`                | step size     | constant learning-rate step           |
|  [03]   | `ClassicalTrustRegion`        | trust region  | classical trust-region step           |
|  [04]   | `LinearTrustRegion`           | trust region  | linear trust-region step              |
|  [05]   | `NewtonDescent`               | descent step  | Newton-direction descent              |
|  [06]   | `NonlinearCGDescent`          | descent step  | nonlinear CG direction                |
|  [07]   | `DoglegDescent`               | descent step  | dogleg combined descent               |
|  [08]   | `SteepestDescent`             | descent step  | steepest-descent direction            |
|  [09]   | `IndirectDampedNewtonDescent` | descent step  | iterative-solve damped Newton descent |

[PUBLIC_TYPE_SCOPE]: adjoint and result types
- rail: differentiable nonlinear optimization

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                                                              |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Solution`                   | result carrier | fields: `value`, `result` (`RESULTS`), `aux`, `stats`, `state`                                                                                                            |
|  [02]   | `RESULTS`                    | result enum    | `successful`, `max_steps_reached`, `nonlinear_max_steps_reached`, `nonlinear_divergence`, `nonfinite`, `singular`, `breakdown`, `stagnation`, `conlim`, `nonfinite_input` |
|  [03]   | `ImplicitAdjoint`            | adjoint        | `(linear_solver=AutoLinearSolver(well_posed=None))`; implicit-function-theorem gradients                                                                                  |
|  [04]   | `RecursiveCheckpointAdjoint` | adjoint        | `(checkpoints=None)`; backpropagation through solver iterations                                                                                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve entry points
- rail: differentiable nonlinear optimization

| [INDEX] | [SURFACE]                                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `minimise(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution`      | minimise       | scalar fn minimization        |
|  [02]   | `least_squares(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution` | least squares  | residual vector least-squares |
|  [03]   | `root_find(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution`     | root find      | fn(y)=0 solve                 |
|  [04]   | `fixed_point(fn, solver, y0, args=None, options=None, *, has_aux=False, max_steps=256, adjoint=ImplicitAdjoint(), throw=True, tags=frozenset())` → `Solution`   | fixed point    | fn(y)=y solve                 |

[ENTRYPOINT_SCOPE]: norm functions
- rail: differentiable nonlinear optimization

| [INDEX] | [SURFACE]              | [SIGNATURE]                              | [RAIL]                |
| :-----: | :--------------------- | :--------------------------------------- | :-------------------- |
|  [01]   | `max_norm(x)` → scalar | `(x: PyTree[Array]) → Shaped[Array, '']` | Chebyshev (L∞) norm   |
|  [02]   | `rms_norm(x)` → scalar | `(x: PyTree[Array]) → Shaped[Array, '']` | root-mean-square norm |
|  [03]   | `two_norm(x)` → scalar | `(x: PyTree[Array]) → Shaped[Array, '']` | Euclidean (L2) norm   |

[ENTRYPOINT_SCOPE]: nonlinear-CG β-coefficient functions
- rail: differentiable nonlinear optimization
- family: β coefficient

| [INDEX] | [SURFACE]          | [RAIL]                               |
| :-----: | :----------------- | :----------------------------------- |
|  [01]   | `polak_ribiere`    | Polak-Ribière nonlinear-CG update    |
|  [02]   | `fletcher_reeves`  | Fletcher-Reeves nonlinear-CG update  |
|  [03]   | `hestenes_stiefel` | Hestenes-Stiefel nonlinear-CG update |
|  [04]   | `dai_yuan`         | Dai-Yuan nonlinear-CG update         |

## [04]-[IMPLEMENTATION_LAW]

[OPTIMIZATION_TOPOLOGY]:
- namespace: `optimistix`; all public types and functions at top level
- four unified solve functions cover all nonlinear problem classes; solver type determines the algorithm, not the entry point
- `fn` signature for `minimise` is `(y, args) -> scalar` or `(y, args) -> (scalar, aux)` when `has_aux=True`
- `fn` signature for `least_squares` and `root_find` is `(y, args) -> residuals` or `(y, args) -> (residuals, aux)`
- `fn` signature for `fixed_point` is `(y, args) -> y` or `(y, args) -> (y, aux)`
- `y0` and `args` are arbitrary JAX pytrees; the solver tracks the pytree structure of `y` throughout iteration
- `Solution.result` carries a `RESULTS` enum value; `throw=True` (default) raises on any non-`successful` value
- `ImplicitAdjoint` computes gradients via the implicit function theorem (one linear solve per backward pass); use for well-posed problems
- `RecursiveCheckpointAdjoint` backpropagates through solver iterations with checkpointing; use when the implicit form is not available

[LOCAL_ADMISSION]:
- Solver instances are constructed outside JIT with their hyperparameters; they are not recompiled per call.
- `max_steps` is a static integer; changing it triggers recompilation under JIT.
- `least_squares` accepts both `AbstractLeastSquaresSolver` and `AbstractMinimiser` as `solver`; the latter wraps the residual norm as the objective.
- `root_find` and `fixed_point` also accept upcast solver types (least-squares or minimiser) when the problem class permits it.
- `Solution.aux` carries caller-defined auxiliary data when `has_aux=True`; it is `None` otherwise.

[RAIL_LAW]:
- Package: `optimistix`
- Owns: JAX-native nonlinear minimization, least-squares, root-finding, and fixed-point iteration with differentiable adjoints
- Accept: `minimise`, `least_squares`, `root_find`, `fixed_point` as the canonical solve entry points; `lineax` solvers as `linear_solver=` in Newton/LM variants
- Reject: `scipy.optimize` when JAX autodiff, JIT, or pytree inputs are required
