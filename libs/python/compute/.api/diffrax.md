# [PY_COMPUTE_API_DIFFRAX]

`diffrax` supplies JAX-native ODE, SDE, and CDE solvers for the compute numeric-intent differential-equation rail. The package owner routes `NumericIntent` ODE, SDE, and event-driven cases through `diffeqsolve` with a chosen solver, step-size controller, and adjoint; it never re-implements a numeric integrator the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `diffrax`
- package: `diffrax`
- import: `diffrax`
- owner: `compute`
- rail: differential-equation
- installed: requires `jax`; marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel)
- capability: JAX-native ODE/SDE/CDE integration — adaptive and fixed-step Runge-Kutta solvers, stochastic solvers, event detection, adjoint-based differentiation, and dense output

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: abstract base protocol
- rail: differential-equation

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]           | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :----------------------- | :------------------------------------ |
|   [1]   | `AbstractSolver`                     | solver base              | root of all solver types              |
|   [2]   | `AbstractAdaptiveSolver`             | adaptive solver base     | solvers with error estimation         |
|   [3]   | `AbstractImplicitSolver`             | implicit solver base     | stiff-equation solvers                |
|   [4]   | `AbstractItoSolver`                  | Ito SDE base             | Ito-convention stochastic solvers     |
|   [5]   | `AbstractStratonovichSolver`         | Stratonovich SDE base    | Stratonovich-convention SDE solvers   |
|   [6]   | `AbstractTerm`                       | term base                | root of drift/diffusion term types    |
|   [7]   | `AbstractStepSizeController`         | controller base          | root of step-size controller types    |
|   [8]   | `AbstractAdaptiveStepSizeController` | adaptive controller base | adaptive step-size controllers        |
|   [9]   | `AbstractAdjoint`                    | adjoint base             | root of adjoint differentiation modes |
|  [10]   | `AbstractBrownianPath`               | path base                | root of Brownian path types           |
|  [11]   | `AbstractGlobalInterpolation`        | interpolation base       | root of global interpolation types    |
|  [12]   | `AbstractLocalInterpolation`         | interpolation base       | root of local interpolation types     |

[PUBLIC_TYPE_SCOPE]: concrete ODE solvers
- rail: differential-equation

| [INDEX] | [SYMBOL]        | [SOLVER_FAMILY] | [ORDER] |
| :-----: | :-------------- | :-------------- | :------ |
|   [1]   | `Euler`         | explicit ERK    | 1       |
|   [2]   | `Heun`          | explicit ERK    | 2       |
|   [3]   | `Midpoint`      | explicit ERK    | 2       |
|   [4]   | `Ralston`       | explicit ERK    | 2       |
|   [5]   | `Bosh3`         | explicit ERK    | 3       |
|   [6]   | `Tsit5`         | explicit ERK    | 5       |
|   [7]   | `Dopri5`        | explicit ERK    | 5       |
|   [8]   | `Dopri8`        | explicit ERK    | 8       |
|   [9]   | `ImplicitEuler` | implicit DIRK   | 1       |
|  [10]   | `KenCarp3`      | ESDIRK          | 3       |
|  [11]   | `KenCarp4`      | ESDIRK          | 4       |
|  [12]   | `KenCarp5`      | ESDIRK          | 5       |
|  [13]   | `Kvaerno3`      | SDIRK           | 3       |
|  [14]   | `Kvaerno4`      | SDIRK           | 4       |
|  [15]   | `Kvaerno5`      | SDIRK           | 5       |

[PUBLIC_TYPE_SCOPE]: SDE solvers and stochastic terms
- rail: differential-equation

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]          | [CAPABILITY]                             |
| :-----: | :-------------------------- | :---------------------- | :--------------------------------------- |
|   [1]   | `EulerHeun`                 | Ito SDE solver          | Euler-Heun weak order 1                  |
|   [2]   | `ItoMilstein`               | Ito SDE solver          | Milstein order 1 Ito                     |
|   [3]   | `StratonovichMilstein`      | Stratonovich SDE solver | Milstein order 1 Stratonovich            |
|   [4]   | `ReversibleHeun`            | reversible SDE solver   | reversible Heun for structure-preserving |
|   [5]   | `LeapfrogMidpoint`          | SDE solver              | leapfrog midpoint                        |
|   [6]   | `SemiImplicitEuler`         | semi-implicit solver    | semi-implicit Euler for stiff SDEs       |
|   [7]   | `ControlTerm`               | SDE term                | diffusion term with control path         |
|   [8]   | `WeaklyDiagonalControlTerm` | SDE term                | weakly diagonal diffusion term           |
|   [9]   | `MultiTerm`                 | term combiner           | combines multiple terms                  |
|  [10]   | `VirtualBrownianTree`       | Brownian path           | space-time-time Levy area Brownian tree  |
|  [11]   | `UnsafeBrownianPath`        | Brownian path           | unsafe fast Brownian path                |
|  [12]   | `BrownianIncrement`         | Brownian increment      | increment-only (no Levy area)            |

[PUBLIC_TYPE_SCOPE]: step-size controllers and adjoints
- rail: differential-equation

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                             |
| :-----: | :--------------------------- | :------------------ | :--------------------------------------- |
|   [1]   | `ConstantStepSize`           | fixed controller    | constant step size                       |
|   [2]   | `PIDController`              | adaptive controller | PID-based adaptive step size             |
|   [3]   | `ClipStepSizeController`     | adaptive controller | clipped adaptive controller              |
|   [4]   | `StepTo`                     | fixed controller    | step to specified times                  |
|   [5]   | `RecursiveCheckpointAdjoint` | adjoint             | memory-efficient recursive checkpointing |
|   [6]   | `DirectAdjoint`              | adjoint             | direct JAX autodiff through the solve    |
|   [7]   | `BacksolveAdjoint`           | adjoint             | continuous adjoint (backsolve)           |
|   [8]   | `ImplicitAdjoint`            | adjoint             | implicit-function theorem adjoint        |

[PUBLIC_TYPE_SCOPE]: solution and event types
- rail: differential-equation

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE] | [CAPABILITY]                                   |
| :-----: | :------------------------- | :------------- | :--------------------------------------------- |
|   [1]   | `Solution`                 | result carrier | `ts`, `ys`, `result`, `stats`, `interpolation` |
|   [2]   | `SaveAt`                   | save spec      | selects which times and fields to save         |
|   [3]   | `SubSaveAt`                | save spec      | sub-solve save specification                   |
|   [4]   | `ODETerm`                  | ODE term       | wraps a vector field callable                  |
|   [5]   | `Event`                    | event spec     | continuous event with condition                |
|   [6]   | `DiscreteTerminatingEvent` | event spec     | discrete terminating event                     |
|   [7]   | `SteadyStateEvent`         | event spec     | built-in steady-state event                    |
|   [8]   | `RESULTS`                  | result enum    | solve result status enum                       |
|   [9]   | `DenseInterpolation`       | interpolation  | dense output interpolation                     |
|  [10]   | `CubicInterpolation`       | interpolation  | cubic Hermite interpolation                    |
|  [11]   | `LinearInterpolation`      | interpolation  | piecewise-linear interpolation                 |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: primary solve entrypoint
- rail: differential-equation

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY]   | [RAIL]                                   |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------- |
|   [1]   | `diffeqsolve(terms, solver, t0, t1, dt0, y0, args, *, saveat, stepsize_controller, adjoint, event, max_steps, throw)` | ODE/SDE solve    | primary integration entrypoint           |
|   [2]   | `ODETerm(vector_field)`                                                                                               | term constructor | wraps `f(t, y, args) -> dy`              |
|   [3]   | `SaveAt(*, t0, t1, ts, steps, dense)`                                                                                 | save spec        | configures output storage                |
|   [4]   | `PIDController(rtol, atol, norm, pcoeff, icoeff, dcoeff, dtmin, dtmax)`                                               | controller init  | PID adaptive step-size configuration     |
|   [5]   | `RecursiveCheckpointAdjoint(checkpoints)`                                                                             | adjoint init     | memory-checkpointed reverse-mode adjoint |

[ENTRYPOINT_SCOPE]: interpolation and utility
- rail: differential-equation

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]  | [RAIL]                                 |
| :-----: | :------------------------------------------ | :-------------- | :------------------------------------- |
|   [1]   | `linear_interpolation(ts, ys)`              | interpolation   | piecewise-linear path interpolation    |
|   [2]   | `rectilinear_interpolation(ts, ys)`         | interpolation   | rectilinear path interpolation         |
|   [3]   | `backward_hermite_coefficients(ts, ys)`     | CDE preparation | precomputes CDE control coefficients   |
|   [4]   | `adjoint_rms_seminorm(x)`                   | norm helper     | RMS seminorm for adjoint tolerance     |
|   [5]   | `is_okay(result)` / `is_successful(result)` | result check    | interrogates `RESULTS` enum value      |
|   [6]   | `steady_state_event()`                      | event factory   | constructs `SteadyStateEvent` instance |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `diffrax`
- Owns: JAX-native ODE/SDE/CDE integration, adaptive step control, event detection, and adjoint-based differentiation
- Accept: a `NumericIntent` differential-equation case dispatched through `diffeqsolve` with an explicit solver, `RecursiveCheckpointAdjoint` for memory-constrained gradient studies, and a `Solution` receipt capturing solver stats and result status
- Reject: hand-rolled Runge-Kutta or Euler integrators; diffrax in any product runtime path; adjoint claims without a captured `Solution.stats` receipt
