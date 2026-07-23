# [PY_COMPUTE_API_DIFFRAX]

`diffrax` owns JAX-native ODE, SDE, and CDE integration for the compute differential-equation rail: one `diffeqsolve` routes every `NumericIntent` case on a chosen solver, step-size controller, and adjoint. Implicit and stiff solvers fold a `lineax`/`optimistix` root-find inside each step, and the whole solve stays JIT-, grad-, and vmap-transformable as an `equinox` pytree.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `diffrax`
- package: `diffrax`
- import: `diffrax`
- owner: `compute`
- rail: differential-equation
- capability: JAX-native ODE/SDE/CDE integration — adaptive and fixed-step Runge-Kutta solvers, Ito/Stratonovich and high-order Levy-area SDE solvers, Brownian path generators with selectable Levy-area level, continuous and discrete event detection, four adjoint differentiation modes, and dense/sub-save output

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: abstract base protocol

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]           | [CAPABILITY]                                 |
| :-----: | :-------------------------------------- | :----------------------- | :------------------------------------------- |
|  [01]   | `AbstractSolver`                        | solver base              | root of all solver types                     |
|  [02]   | `AbstractAdaptiveSolver`                | adaptive solver base     | solvers with error estimation                |
|  [03]   | `AbstractImplicitSolver`                | implicit solver base     | stiff-equation solvers (carry a root_finder) |
|  [04]   | `AbstractItoSolver`                     | Ito SDE base             | Ito-convention stochastic solvers            |
|  [05]   | `AbstractStratonovichSolver`            | Stratonovich SDE base    | Stratonovich-convention SDE solvers          |
|  [06]   | `AbstractTerm`                          | term base                | root of drift/diffusion term types           |
|  [07]   | `AbstractStepSizeController`            | controller base          | root of step-size controller types           |
|  [08]   | `AbstractAdaptiveStepSizeController`    | adaptive controller base | adaptive step-size controllers               |
|  [09]   | `AbstractAdjoint`                       | adjoint base             | root of adjoint differentiation modes        |
|  [10]   | `AbstractPath` / `AbstractBrownianPath` | path base                | root of path / Brownian path types           |
|  [11]   | `AbstractGlobalInterpolation`           | interpolation base       | root of global interpolation types           |
|  [12]   | `AbstractLocalInterpolation`            | interpolation base       | root of local interpolation types            |
|  [13]   | `AbstractBrownianIncrement`             | Levy-area base           | root of Brownian increment structures        |
|  [14]   | `AbstractSpaceTimeLevyArea`             | Levy-area base           | root of space-time Levy-area structures      |
|  [15]   | `AbstractSpaceTimeTimeLevyArea`         | Levy-area base           | root of space-time-time Levy-area structures |

[PUBLIC_TYPE_SCOPE]: concrete ODE solvers

| [INDEX] | [SYMBOL]        | [SOLVER_FAMILY] | [ORDER] |
| :-----: | :-------------- | :-------------- | :------ |
|  [01]   | `Euler`         | explicit ERK    | 1       |
|  [02]   | `Heun`          | explicit ERK    | 2       |
|  [03]   | `Midpoint`      | explicit ERK    | 2       |
|  [04]   | `Ralston`       | explicit ERK    | 2       |
|  [05]   | `Bosh3`         | explicit ERK    | 3       |
|  [06]   | `Tsit5`         | explicit ERK    | 5       |
|  [07]   | `Dopri5`        | explicit ERK    | 5       |
|  [08]   | `Dopri8`        | explicit ERK    | 8       |
|  [09]   | `ImplicitEuler` | implicit DIRK   | 1       |
|  [10]   | `KenCarp3`      | ESDIRK          | 3       |
|  [11]   | `KenCarp4`      | ESDIRK          | 4       |
|  [12]   | `KenCarp5`      | ESDIRK          | 5       |
|  [13]   | `Kvaerno3`      | SDIRK           | 3       |
|  [14]   | `Kvaerno4`      | SDIRK           | 4       |
|  [15]   | `Kvaerno5`      | SDIRK           | 5       |
|  [16]   | `Sil3`          | additive IMEX   | 3       |

[PUBLIC_TYPE_SCOPE]: SDE solvers and stochastic terms

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]               | [CAPABILITY]                                          |
| :-----: | :--------------------------------- | :--------------------------- | :---------------------------------------------------- |
|  [01]   | `EulerHeun`                        | Stratonovich SDE solver      | Euler-Heun, no Levy area                              |
|  [02]   | `ItoMilstein`                      | Ito SDE solver               | Milstein order 1 Ito                                  |
|  [03]   | `StratonovichMilstein`             | Stratonovich SDE solver      | Milstein order 1 Stratonovich                         |
|  [04]   | `ReversibleHeun`                   | reversible SDE solver        | reversible Heun for structure-preserving              |
|  [05]   | `LeapfrogMidpoint`                 | SDE solver                   | leapfrog midpoint                                     |
|  [06]   | `SemiImplicitEuler`                | semi-implicit solver         | semi-implicit Euler for separable/stiff SDEs          |
|  [07]   | `SRA1`                             | additive-noise SDE solver    | strong order 1.5, space-time Levy area                |
|  [08]   | `ShARK`                            | high-order SDE solver        | strong order 1.5 additive noise, space-time Levy area |
|  [09]   | `GeneralShARK`                     | high-order SDE solver        | strong order 1.5 general noise, space-time Levy area  |
|  [10]   | `SlowRK`                           | commutative-noise SDE solver | strong order 1.5, space-time-time Levy area           |
|  [11]   | `SPaRK`                            | general SDE solver           | strong order 1.5 general Runge-Kutta                  |
|  [12]   | `SEA`                              | additive-noise SDE solver    | shifted Euler, weak order, space-time Levy area       |
|  [13]   | `ALIGN`                            | underdamped Langevin solver  | structure-preserving Langevin SDE integrator          |
|  [14]   | `ShOULD`                           | underdamped Langevin solver  | structure-preserving Langevin SDE integrator          |
|  [15]   | `QUICSORT`                         | underdamped Langevin solver  | structure-preserving Langevin SDE integrator          |
|  [16]   | `ControlTerm`                      | SDE term                     | diffusion term driven by a control path               |
|  [17]   | `WeaklyDiagonalControlTerm`        | SDE term                     | weakly diagonal diffusion term                        |
|  [18]   | `MultiTerm`                        | term combiner                | combines drift + diffusion terms into one term        |
|  [19]   | `UnderdampedLangevinDriftTerm`     | Langevin term                | drift term for the Langevin solvers                   |
|  [20]   | `UnderdampedLangevinDiffusionTerm` | Langevin term                | diffusion term for the Langevin solvers               |
|  [21]   | `VirtualBrownianTree`              | Brownian path                | reproducible Brownian tree with selectable Levy area  |
|  [22]   | `UnsafeBrownianPath`               | Brownian path                | fast non-reproducible path (forward solve only)       |
|  [23]   | `BrownianIncrement`                | Levy-area level              | increment-only Levy area                              |
|  [24]   | `SpaceTimeLevyArea`                | Levy-area level              | space-time Levy area                                  |
|  [25]   | `SpaceTimeTimeLevyArea`            | Levy-area level              | space-time-time Levy area                             |

[PUBLIC_TYPE_SCOPE]: step-size controllers and adjoints

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                                       |
| :-----: | :--------------------------- | :------------------ | :------------------------------------------------- |
|  [01]   | `ConstantStepSize`           | fixed controller    | constant step size                                 |
|  [02]   | `PIDController`              | adaptive controller | PID-based adaptive step size                       |
|  [03]   | `ClipStepSizeController`     | wrapping controller | clips a wrapped controller's steps to a range      |
|  [04]   | `StepTo`                     | fixed controller    | step to specified times                            |
|  [05]   | `RecursiveCheckpointAdjoint` | adjoint             | memory-efficient recursive checkpointing (default) |
|  [06]   | `DirectAdjoint`              | adjoint             | direct JAX autodiff through the solve              |
|  [07]   | `BacksolveAdjoint`           | adjoint             | continuous adjoint (backsolve)                     |
|  [08]   | `ImplicitAdjoint`            | adjoint             | implicit-function-theorem adjoint (steady-state)   |
|  [09]   | `ForwardMode`                | adjoint             | forward-mode autodiff through the solve            |

[PUBLIC_TYPE_SCOPE]: solution, save, term, and event types

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE] | [CAPABILITY]                                                                                             |
| :-----: | :---------- | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Solution`  | result carrier | solve result carrier; `stats`+`result` receipt, `interpolation` dense output (fields below)              |
|  [02]   | `SaveAt`    | save spec      | selects which times and fields to save                                                                   |
|  [03]   | `SubSaveAt` | save spec      | per-subsolve save specification within `SaveAt`                                                          |
|  [04]   | `ODETerm`   | ODE term       | wraps a vector field callable `f(t, y, args)`                                                            |
|  [05]   | `Event`     | event spec     | root-find continuous event(s) with optional `root_finder`                                                |
|  [06]   | `RESULTS`   | result enum    | `successful`/`max_steps_reached`/`dt_min_reached`/`event_occurred`/`max_steps_rejected`/`internal_error` |

- `Solution` fields: `t0` `t1` `ts` `ys` `interpolation` (dense, when `SaveAt(dense=True)`) `stats` (`num_steps`/`num_accepted_steps`/`num_rejected_steps`) `result` `solver_state` `controller_state` `made_jump` `event_mask`

[PUBLIC_TYPE_SCOPE]: path interpolation

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE] | [CAPABILITY]                                   |
| :-----: | :-------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `DenseInterpolation`  | interpolation  | dense output interpolation                     |
|  [02]   | `CubicInterpolation`  | interpolation  | cubic Hermite interpolation (CDE control path) |
|  [03]   | `LinearInterpolation` | interpolation  | piecewise-linear interpolation (CDE control)   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: solve, term, path, event, and adjoint entrypoints
- call: `diffeqsolve(terms, solver, t0, t1, dt0, y0, args=None, *, saveat=SaveAt(t1=True), stepsize_controller=ConstantStepSize(), adjoint=RecursiveCheckpointAdjoint(), event=None, max_steps=4096, throw=True, progress_meter=NoProgressMeter(), solver_state=None, controller_state=None, made_jump=None) -> Solution`
- call: `ODETerm(vector_field)`, `ControlTerm(vector_field, control)`, `MultiTerm(*terms)` build the term pytree; `SaveAt(*, t0=False, t1=False, ts=None, steps=False, dense=False, fn=..., subs=None)` and `PIDController(rtol, atol, pcoeff=0, icoeff=1, dcoeff=0, *, dtmin=None, dtmax=None, norm=rms_norm, jump_ts=None, step_ts=None)` configure output and adaptive stepping
- call: `VirtualBrownianTree(t0, t1, tol, shape, key, levy_area=BrownianIncrement)` / `UnsafeBrownianPath(shape, key, levy_area=BrownianIncrement)` build Brownian paths; `levy_area=SpaceTimeLevyArea`/`SpaceTimeTimeLevyArea` feeds the high-order SDE solvers
- call: `Event(cond_fn, root_finder=None)` with `cond_fn(t, y, args, **kw)`; `RecursiveCheckpointAdjoint(checkpoints=None)` / `BacksolveAdjoint(**kwargs)` select the adjoint

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]    | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------ | :---------------- | :---------------------------------------------------------- |
|  [01]   | `diffeqsolve`                                     | ODE/SDE/CDE solve | primary integration entrypoint                              |
|  [02]   | `ODETerm`                                         | term constructor  | wraps `f(t, y, args) -> dy`                                 |
|  [03]   | `ControlTerm` / `MultiTerm`                       | term constructor  | diffusion term over a control path; combine drift+diffusion |
|  [04]   | `SaveAt`                                          | save spec         | configures output storage / dense output                    |
|  [05]   | `PIDController`                                   | controller init   | PID adaptive step-size configuration                        |
|  [06]   | `VirtualBrownianTree`                             | path constructor  | reproducible Brownian path                                  |
|  [07]   | `UnsafeBrownianPath`                              | path constructor  | fast forward-only Brownian path                             |
|  [08]   | `Event`                                           | event constructor | continuous root-find event(s)                               |
|  [09]   | `RecursiveCheckpointAdjoint` / `BacksolveAdjoint` | adjoint init      | memory-checkpointed / continuous reverse-mode adjoint       |

[ENTRYPOINT_SCOPE]: interpolation and utility
- call: `linear_interpolation(ts, ys, *, replace_nans_at_start=None, fill_forward_nans_at_end=False)`, `rectilinear_interpolation(ts, ys, ...)`, and `backward_hermite_coefficients(ts, ys, ...)` build CDE control paths; `steady_state_event(rtol=None, atol=None, norm=rms_norm)` builds an `Event` cond_fn; `adjoint_rms_seminorm(x)` is the `BacksolveAdjoint` tolerance seminorm

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :-------------- | :---------------------------------------------------- |
|  [01]   | `linear_interpolation`          | interpolation   | piecewise-linear path interpolation (CDE)             |
|  [02]   | `rectilinear_interpolation`     | interpolation   | rectilinear path interpolation                        |
|  [03]   | `backward_hermite_coefficients` | CDE preparation | precomputes `CubicInterpolation` control coefficients |
|  [04]   | `adjoint_rms_seminorm`          | norm helper     | RMS seminorm for `BacksolveAdjoint` tolerance         |
|  [05]   | `Solution.result` via `RESULTS` | result check    | compare `Solution.result` against `RESULTS` members   |
|  [06]   | `steady_state_event`            | event factory   | builds an `Event` cond_fn detecting steady state      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `diffrax` — every public type and function at top level.
- one `diffeqsolve` owns every ODE/SDE/CDE case: the `terms` shape sets the equation kind (a single `ODETerm`, a `MultiTerm(ODETerm, ControlTerm)` for SDE/CDE) and the solver type sets the convention (`AbstractItoSolver`/`AbstractStratonovichSolver`); the `solver`, `stepsize_controller`, `adjoint`, `event`, and `saveat` ride as independent rows on that one call, never a parallel solve variant per axis.
- `Solution.result` carries a `RESULTS` value; `throw=True` raises on a non-`successful` result, `throw=False` returns it beside `Solution.stats` (accepted/rejected step counts) for inspection.
- SDE convention is load-bearing: an `AbstractItoSolver` integrates the Ito form, an `AbstractStratonovichSolver` the Stratonovich form; a high-order SDE solver demands a Brownian path whose `levy_area` matches (`SpaceTimeLevyArea` for `ShARK`/`SRA1`/`SEA`, `SpaceTimeTimeLevyArea` for `SlowRK`), and the Langevin solvers (`ALIGN`/`ShOULD`/`QUICSORT`) pair with `UnderdampedLangevin*` terms.

[STACKING]:
- `equinox`(`.api/equinox.md`): a `diffeqsolve` call is a pure JAX function over `equinox.Module` pytree state, composing inside `filter_jit`/`filter_grad`/`filter_vmap` — a batched parameter sweep is one `filter_vmap(diffeqsolve, ...)` and a gradient-through-solve is `filter_grad` over the chosen `adjoint`; `RESULTS` inherits the `equinox.Enumeration` termination base.
- `lineax`(`.api/lineax.md`) / `optimistix`(`.api/optimistix.md`): an `AbstractImplicitSolver` (`ImplicitEuler`/`Kvaerno*`/`KenCarp*`) runs an `optimistix` root-find each step whose linear sub-solve is a `lineax` operator solve, and `ImplicitAdjoint` threads the same `optimistix`/`lineax` implicit-function solve — the stiff step reuses the sibling solver stack.
- `interpax`(`.api/interpax.md`) / `quadax`(`.api/quadax.md`): dense output (`SaveAt(dense=True)` -> `Solution.interpolation`) yields a continuous trajectory the `interpax`/`quadax` rails resample or integrate; a CDE control path builds with `backward_hermite_coefficients` + `CubicInterpolation` passed as the `ControlTerm` control.
- within-lib: the `DifferentialIntent` route folds the `Solution` receipt (`stats` step counts + `result` verdict) onto `SolverReceipt`, and `RecursiveCheckpointAdjoint` bounds reverse-mode memory for the graduation gradient study.

[LOCAL_ADMISSION]:
- a `NumericIntent` differential-equation case dispatches through `diffeqsolve` with an explicit solver, controller, and adjoint; an SDE Brownian path carries the `levy_area` its solver demands, and `diffrax` stays a compute-plane solver.

[RAIL_LAW]:
- Package: `diffrax`
- Owns: JAX-native ODE/SDE/CDE integration, adaptive step control, Ito/Stratonovich and high-order Levy-area SDE solvers, continuous/discrete event detection, and four adjoint differentiation modes
- Accept: a `NumericIntent` differential-equation case dispatched through `diffeqsolve` with an explicit solver, controller, and adjoint; a Brownian path whose `levy_area` matches the SDE solver; `RecursiveCheckpointAdjoint` for memory-constrained gradient studies; a `Solution` receipt capturing `stats` and `result`
- Reject: hand-rolled Runge-Kutta or Euler integrators; a hand-rolled Newton iteration where the implicit solver's `optimistix`/`lineax` root-find applies; `diffrax` on any product runtime path; adjoint claims without a captured `Solution.stats` receipt
