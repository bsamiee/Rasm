# [PY_COMPUTE_API_DIFFRAX]

`diffrax` supplies JAX-native ODE, SDE, and CDE solvers for the compute numeric-intent differential-equation rail. The package owner routes `NumericIntent` ODE, SDE, and event-driven cases through `diffeqsolve` with a chosen solver, step-size controller, and adjoint; it never re-implements a numeric integrator the package owns. Implicit/stiff solvers compose a `lineax`/`optimistix` root-find inside the step, and the whole solve is JIT-/grad-/vmap-transformable as an `equinox` pytree.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `diffrax`
- package: `diffrax`
- import: `diffrax`
- owner: `compute`
- rail: differential-equation
- license: Apache-2.0
- installed: requires `jax`; marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel) — companion-band alongside the sibling JAX stack (`jax`/`equinox`/`lineax`/`optimistix`/`optax`/`interpax`/`quadax`).
- capability: JAX-native ODE/SDE/CDE integration — adaptive and fixed-step Runge-Kutta solvers, Ito/Stratonovich and high-order Levy-area SDE solvers, Brownian path generators with selectable Levy-area level, continuous and discrete event detection, four adjoint differentiation modes, and dense/sub-save output

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: abstract base protocol
- rail: differential-equation

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]           | [CAPABILITY]                          |
| :-----: | :----------------------------------- | :----------------------- | :------------------------------------ |
|  [01]   | `AbstractSolver`                     | solver base              | root of all solver types              |
|  [02]   | `AbstractAdaptiveSolver`             | adaptive solver base     | solvers with error estimation         |
|  [03]   | `AbstractImplicitSolver`             | implicit solver base     | stiff-equation solvers (carry a root_finder) |
|  [04]   | `AbstractItoSolver`                  | Ito SDE base             | Ito-convention stochastic solvers     |
|  [05]   | `AbstractStratonovichSolver`         | Stratonovich SDE base    | Stratonovich-convention SDE solvers   |
|  [06]   | `AbstractTerm`                       | term base                | root of drift/diffusion term types    |
|  [07]   | `AbstractStepSizeController`         | controller base          | root of step-size controller types    |
|  [08]   | `AbstractAdaptiveStepSizeController` | adaptive controller base | adaptive step-size controllers        |
|  [09]   | `AbstractAdjoint`                    | adjoint base             | root of adjoint differentiation modes |
|  [10]   | `AbstractPath` / `AbstractBrownianPath` | path base             | root of path / Brownian path types    |
|  [11]   | `AbstractGlobalInterpolation`        | interpolation base       | root of global interpolation types    |
|  [12]   | `AbstractLocalInterpolation`         | interpolation base       | root of local interpolation types     |
|  [13]   | `AbstractBrownianIncrement` / `AbstractSpaceTimeLevyArea` / `AbstractSpaceTimeTimeLevyArea` | Levy-area base | root of Brownian increment / Levy-area structures |

[PUBLIC_TYPE_SCOPE]: concrete ODE solvers
- rail: differential-equation

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
- rail: differential-equation

The high-order SDE solvers (`SPaRK`/`GeneralShARK`/`ShARK`/`SlowRK`/`SEA`/`SRA1`/`ALIGN`/`QUICSORT`/`ShOULD`) require a Brownian path that supplies the matching Levy-area level (e.g. `SpaceTimeLevyArea` or `SpaceTimeTimeLevyArea`); construct `VirtualBrownianTree(..., levy_area=SpaceTimeLevyArea)` to feed them.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]              | [CAPABILITY]                                   |
| :-----: | :-------------------------------- | :-------------------------- | :--------------------------------------------- |
|  [01]   | `EulerHeun`                       | Stratonovich SDE solver     | Euler-Heun, no Levy area                       |
|  [02]   | `ItoMilstein`                     | Ito SDE solver              | Milstein order 1 Ito                           |
|  [03]   | `StratonovichMilstein`            | Stratonovich SDE solver     | Milstein order 1 Stratonovich                  |
|  [04]   | `ReversibleHeun`                  | reversible SDE solver       | reversible Heun for structure-preserving       |
|  [05]   | `LeapfrogMidpoint`                | SDE solver                  | leapfrog midpoint                              |
|  [06]   | `SemiImplicitEuler`               | semi-implicit solver        | semi-implicit Euler for separable/stiff SDEs   |
|  [07]   | `SRA1`                            | additive-noise SDE solver   | strong order 1.5, space-time Levy area         |
|  [08]   | `ShARK` / `GeneralShARK`          | high-order SDE solver       | strong order 1.5 (additive / general noise), space-time Levy area |
|  [09]   | `SlowRK`                          | commutative-noise SDE solver| strong order 1.5, space-time-time Levy area    |
|  [10]   | `SPaRK`                           | general SDE solver          | strong order 1.5 general Runge-Kutta           |
|  [11]   | `SEA`                             | additive-noise SDE solver   | shifted Euler, weak order, space-time Levy area |
|  [12]   | `ALIGN` / `ShOULD` / `QUICSORT`   | underdamped Langevin solver | structure-preserving Langevin SDE integrators  |
|  [13]   | `ControlTerm`                     | SDE term                    | diffusion term driven by a control path        |
|  [14]   | `WeaklyDiagonalControlTerm`       | SDE term                    | weakly diagonal diffusion term                 |
|  [15]   | `MultiTerm`                       | term combiner               | combines drift + diffusion terms into one term |
|  [16]   | `UnderdampedLangevinDriftTerm` / `UnderdampedLangevinDiffusionTerm` | Langevin term | drift/diffusion terms for the Langevin solvers |
|  [17]   | `VirtualBrownianTree`             | Brownian path               | reproducible Brownian tree with selectable Levy area |
|  [18]   | `UnsafeBrownianPath`              | Brownian path               | fast non-reproducible path (forward solve only) |
|  [19]   | `BrownianIncrement` / `SpaceTimeLevyArea` / `SpaceTimeTimeLevyArea` | Levy-area level | increment-only / space-time / space-time-time Levy area |

[PUBLIC_TYPE_SCOPE]: step-size controllers and adjoints
- rail: differential-equation

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                             |
| :-----: | :--------------------------- | :------------------ | :--------------------------------------- |
|  [01]   | `ConstantStepSize`           | fixed controller    | constant step size                       |
|  [02]   | `PIDController`              | adaptive controller | PID-based adaptive step size             |
|  [03]   | `ClipStepSizeController`     | wrapping controller | clips a wrapped controller's steps to a range |
|  [04]   | `StepTo`                     | fixed controller    | step to specified times                  |
|  [05]   | `RecursiveCheckpointAdjoint` | adjoint             | memory-efficient recursive checkpointing (default) |
|  [06]   | `DirectAdjoint`              | adjoint             | direct JAX autodiff through the solve    |
|  [07]   | `BacksolveAdjoint`           | adjoint             | continuous adjoint (backsolve)           |
|  [08]   | `ImplicitAdjoint`            | adjoint             | implicit-function-theorem adjoint (steady-state) |
|  [09]   | `ForwardMode`                | adjoint             | forward-mode autodiff through the solve  |

[PUBLIC_TYPE_SCOPE]: solution, save, term, and event types
- rail: differential-equation

`Event` + `steady_state_event` are the modern event surface; `DiscreteTerminatingEvent` and `SteadyStateEvent` are deprecated and retained only for backward compatibility (static type checkers warn on use).

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE] | [CAPABILITY]                                   |
| :-----: | :------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `Solution`                 | result carrier | `t0`, `t1`, `ts`, `ys`, `interpolation`, `stats`, `result`, `solver_state`, `controller_state`, `made_jump`, `event_mask` |
|  [02]   | `SaveAt`                   | save spec      | selects which times and fields to save         |
|  [03]   | `SubSaveAt`                | save spec      | per-subsolve save specification within `SaveAt` |
|  [04]   | `ODETerm`                  | ODE term       | wraps a vector field callable `f(t, y, args)`  |
|  [05]   | `Event`                    | event spec     | root-find continuous event(s) with optional `root_finder` |
|  [06]   | `RESULTS`                  | result enum (`equinox.Enumeration`) | members `successful`, `max_steps_reached`, `dt_min_reached`, `event_occurred`, `max_steps_rejected`, `internal_error`; the same `equinox.Enumeration` base `optimistix`/`lineax` `RESULTS` ride, so `RESULTS.promote(item)` (inheritance-widening, `ValueError` on a same-class member) and `RESULTS.where(pred, a, b)` (branchless `jnp.where` select) are inherited base members, and a member is an `EnumerationItem` exposing only `_value` (int code) and `_enumeration` (no `.name`); a batched `filter_vmap` sweep aggregates the worst-case verdict by `jnp.max` over `solution.result._value` plus the `_name_to_item` name inversion, never `promote` |
|  [07]   | `DenseInterpolation`       | interpolation  | dense output interpolation                     |
|  [08]   | `CubicInterpolation`       | interpolation  | cubic Hermite interpolation (CDE control path) |
|  [09]   | `LinearInterpolation`      | interpolation  | piecewise-linear interpolation (CDE control)   |
|  [10]   | `DiscreteTerminatingEvent` / `SteadyStateEvent` | event spec (deprecated) | superseded by `Event` + `steady_state_event` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: primary solve entrypoint
- rail: differential-equation

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY]   | [RAIL]                                   |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `diffeqsolve(terms, solver, t0, t1, dt0, y0, args=None, *, saveat=SaveAt(t1=True), stepsize_controller=ConstantStepSize(), adjoint=RecursiveCheckpointAdjoint(), event=None, max_steps=4096, throw=True, progress_meter=NoProgressMeter(), solver_state=None, controller_state=None, made_jump=None)` -> `Solution` | ODE/SDE/CDE solve | primary integration entrypoint           |
|  [02]   | `ODETerm(vector_field)`                                                                                               | term constructor | wraps `f(t, y, args) -> dy`              |
|  [03]   | `ControlTerm(vector_field, control)` / `MultiTerm(*terms)`                                                            | term constructor | diffusion term over a control path; combine drift+diffusion |
|  [04]   | `SaveAt(*, t0=False, t1=False, ts=None, steps=False, dense=False, fn=..., subs=None)`                                 | save spec        | configures output storage / dense output |
|  [05]   | `PIDController(rtol, atol, pcoeff=0, icoeff=1, dcoeff=0, *, dtmin=None, dtmax=None, norm=rms_norm, jump_ts=None, step_ts=None)` | controller init | PID adaptive step-size configuration |
|  [06]   | `VirtualBrownianTree(t0, t1, tol, shape, key, levy_area=BrownianIncrement)`                                           | path constructor | reproducible Brownian path; `levy_area=SpaceTimeLevyArea`/`SpaceTimeTimeLevyArea` for high-order SDE solvers |
|  [07]   | `UnsafeBrownianPath(shape, key, levy_area=BrownianIncrement)`                                                         | path constructor | fast forward-only Brownian path          |
|  [08]   | `Event(cond_fn, root_finder=None)`                                                                                    | event constructor| continuous root-find event(s); `cond_fn(t, y, args, **kw)` |
|  [09]   | `RecursiveCheckpointAdjoint(checkpoints=None)` / `BacksolveAdjoint(**kwargs)`                                         | adjoint init     | memory-checkpointed / continuous reverse-mode adjoint |

[ENTRYPOINT_SCOPE]: interpolation and utility
- rail: differential-equation

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]  | [RAIL]                                 |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------- |
|  [01]   | `linear_interpolation(ts, ys, *, replace_nans_at_start=None, fill_forward_nans_at_end=False)` | interpolation | piecewise-linear path interpolation (CDE) |
|  [02]   | `rectilinear_interpolation(ts, ys, ...)`                   | interpolation   | rectilinear path interpolation         |
|  [03]   | `backward_hermite_coefficients(ts, ys, ...)`               | CDE preparation | precomputes `CubicInterpolation` control coefficients |
|  [04]   | `adjoint_rms_seminorm(x)`                                  | norm helper     | RMS seminorm for `BacksolveAdjoint` tolerance |
|  [05]   | `diffeqsolve(...).result` interrogation via `RESULTS`      | result check    | compare `Solution.result` against `RESULTS` members |
|  [06]   | `steady_state_event(rtol=None, atol=None, norm=rms_norm)`  | event factory   | builds an `Event` cond_fn detecting steady state |

## [04]-[IMPLEMENTATION_LAW]

[DIFFEQ_TOPOLOGY]:
- namespace: `diffrax`; all public types and functions at top level.
- one `diffeqsolve(terms, solver, t0, t1, dt0, y0, ...)` owns every ODE/SDE/CDE case; the equation kind is the `terms` shape (a single `ODETerm`, a `MultiTerm(ODETerm, ControlTerm)` for SDE/CDE) and the convention is the solver type (`AbstractItoSolver`/`AbstractStratonovichSolver`), never a parallel solve entrypoint.
- the solver, `stepsize_controller`, `adjoint`, `event`, and `saveat` are independent rows on the one `diffeqsolve`: adaptive vs fixed step is a controller row, the differentiation mode is an adjoint row, output selection is a `SaveAt` row, and termination/event detection is an `event` row — never a parallel solve variant per axis.
- `Solution.result` carries a `RESULTS` enum value; `throw=True` raises on a non-`successful` result, `throw=False` returns it for inspection alongside `Solution.stats` (step counts, accepted/rejected).
- SDE convention is load-bearing: an `AbstractItoSolver` integrates the Ito form and a `AbstractStratonovichSolver` the Stratonovich form; high-order SDE solvers demand a Brownian path whose `levy_area` matches (`SpaceTimeLevyArea` for `ShARK`/`SRA1`/`SEA`, `SpaceTimeTimeLevyArea` for `SlowRK`), and the Langevin solvers (`ALIGN`/`ShOULD`/`QUICSORT`) pair with `UnderdampedLangevin*` terms.

[INTEGRATION_STACK]:
- jax/equinox seam: a `diffeqsolve` call is a pure JAX function over pytree state, so it composes inside `eqx.filter_jit`/`eqx.filter_grad`/`eqx.filter_vmap` (`equinox.md`) — a batched parameter sweep is one `filter_vmap(diffeqsolve, ...)` and a gradient-through-solve is `filter_grad` over the chosen `adjoint`, never a Python loop.
- lineax/optimistix seam: an `AbstractImplicitSolver` (`ImplicitEuler`/`Kvaerno*`/`KenCarp*`) runs an `optimistix` root-find each step whose linear sub-solve is a `lineax` operator solve (`lineax.md`); `ImplicitAdjoint` likewise threads an `optimistix`/`lineax` implicit-function solve, so the stiff step reuses the sibling solver stack rather than a hand-rolled Newton iteration.
- interpax/quadax seam: dense output (`SaveAt(dense=True)` -> `Solution.interpolation`) yields a continuous trajectory the `interpax`/`quadax` (`interpax.md`/`quadax.md`) rails resample or integrate; a CDE control path is built with `backward_hermite_coefficients` + `CubicInterpolation` and passed as the `ControlTerm` control.
- adjoint receipt: `RecursiveCheckpointAdjoint` (default) bounds reverse-mode memory; the captured `Solution.stats` (`num_steps`/`num_accepted_steps`/`num_rejected_steps`) plus `Solution.result` form the solve receipt the graduation owner hands across the wire.

[RAIL_LAW]:
- Package: `diffrax`
- Owns: JAX-native ODE/SDE/CDE integration, adaptive step control, Ito/Stratonovich and high-order Levy-area SDE solvers, continuous/discrete event detection, and four adjoint differentiation modes
- Accept: a `NumericIntent` differential-equation case dispatched through `diffeqsolve` with an explicit solver, controller, and adjoint; a Brownian path whose `levy_area` matches the SDE solver; `RecursiveCheckpointAdjoint` for memory-constrained gradient studies; a `Solution` receipt capturing `stats` and `result`
- Reject: hand-rolled Runge-Kutta or Euler integrators; a hand-rolled Newton iteration where the implicit solver's `optimistix`/`lineax` root-find applies; the deprecated `DiscreteTerminatingEvent`/`SteadyStateEvent` where `Event`/`steady_state_event` is the modern surface; diffrax in any product runtime path; adjoint claims without a captured `Solution.stats` receipt
