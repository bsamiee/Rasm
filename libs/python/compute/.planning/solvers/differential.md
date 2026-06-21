# [PY_COMPUTE_DIFFERENTIAL]

The differential-equations route of the one numeric solver. `DifferentialIntent` discriminates initial-value `Ode`, stochastic `Sde`, and controlled `Cde` integration over `diffrax.diffeqsolve` on the JAX-and-Equinox floor with adaptive step control, steady-state and root-finding events, dense interpolated output, and adjoint-differentiable integration, folding the integration diagnostics into the one `SolverReceipt`. It is the trajectory-integration peer of the scalar-integral `solvers/quadrature.md#QUADRATURE` route: this owner integrates a vector field, that one a scalar integrand.

The solve is adjoint-differentiable, so a parametric study reads sensitivities through the integration rather than through the step sequence. The vector field, drift, diffusion, and control are JAX pytrees carried as one `FieldFn`/`Pytree`/`ControlPath` vocabulary rather than weak `object`/`np.ndarray` slots. The frozen `SolveEngine` value object folds the gated modules (`dfx`/`eqx`/`jnp`/`jtu`/`jr`) so the controller, event, save spec, adjoint, per-leaf lift, terminal read, residual norm, and Langevin term-pair are carrier methods reading one populated handle rather than six module imports and a `(jnp, jtu)` pair threaded through every helper — the `DiffEngine.gated()` discipline `solvers/sensitivity.md#SENSITIVITY` runs and the `SolveEngine.gated()` discipline `solvers/nonlinear.md#NONLINEAR` runs. `SolveEngine.gated()` imports once behind the band and runs `jax.config.update("jax_enable_x64", True)` so the `1e-8` tolerance and the stiff/adjoint solve hold at float64, `engine.lift` raises `y0` per-leaf through `jax.tree_util.tree_map(jax.numpy.asarray, y0)` and `engine.terminal` reads back through the same per-leaf `tree_map` — never a bare `numpy.asarray`/`jax.numpy.asarray` over the whole tree, which flattens a structured `(x, v)` Langevin or multi-leaf state and breaks a non-array leaf — `engine.tree_norm` contracts the one per-leaf sum-of-squares residual total over that pytree, and each field thunk wraps in `equinox.filter_jit` so static closure leaves skip XLA tracing. Setting `IntegratePolicy.batched` reads the leading axis of `y0` as a sweep of initial states and maps the whole solve through `equinox.filter_vmap` over that axis without leaving the compiled solve, under the `AdjointMode.FORWARD` forward-sensitivity adjoint; the receipt folds the per-row residual to its `jnp.max` worst component and the per-row `Solution.result` to the worst-case termination member, that path feeding the per-axis trajectory sensitivity `solvers/sensitivity.md#SENSITIVITY` differentiates through and `experiments/study.md#STUDY` reads as a DGSM Jacobian, never a Python loop over the stack inside this fence.

Four orthogonal data tables own solver and path selection. `_SOLVER` folds the full catalogued diffrax family — explicit-RK `Tsit5`/`Dopri5`/`Dopri8`, ESDIRK/SDIRK stiff `KenCarp3`-`5`/`Kvaerno3`-`5`, order-1 stochastic `EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`LeapfrogMidpoint`/`SemiImplicitEuler`, strong-order-1.5 `SRA1`/`ShARK`/`GeneralShARK`/`SlowRK`/`SEA`/`SPaRK`, and the underdamped-Langevin `ALIGN`/`ShOULD`/`QUICSORT` — into the chosen integrator, so a new solver is one row, never a `match` arm. `_LEVY` keys each strong-order/Langevin member to the `SpaceTimeLevyArea`/`SpaceTimeTimeLevyArea` path level it demands and folds that level into the path's `levy_area=`; the order-1 family carries no row and keeps the `BrownianIncrement` default, so a mis-paired path is unrepresentable. `_LANGEVIN` membership selects the `MultiTerm(UnderdampedLangevinDriftTerm, UnderdampedLangevinDiffusionTerm)` term-pair keyed by the policy's `gamma`/`langevin_u` over a `(x, v)` position-velocity pytree state, where the plain SDE family builds `MultiTerm(ODETerm, ControlTerm)` — one data cell, never a per-solver arm. `_LEVY` membership is also the SDE adaptivity witness: an order-1 solver carries no error estimate and is fixed-step only, so the controller selection floors it to `ConstantStepSize` regardless of `StepKind`, while the order-1.5/Langevin family and every ODE/CDE solver admit the adaptive `PIDController`. The `BrownianPath` axis selects the reproducible reverse-mode-safe `VirtualBrownianTree` or the faster forward-only `UnsafeBrownianPath` path generator orthogonally to the solver. The non-stiff ODE route carries a reachable fixed-step explicit-Euler `numpy` floor so a cp315 run without the jaxlib wheel never returns `Error(Import)`, exactly as every sibling route floors.

## [01]-[INDEX]

- [01]-[DIFFERENTIAL]: ODE/SDE/CDE integration over Diffrax with adjoint-differentiable solves, table-dispatched solver/term/path/policy selection across the full deterministic, stochastic, and underdamped-Langevin solver families, a pytree-total terminal/steady-state residual, and a numpy explicit-Euler ODE floor on one `DifferentialIntent` owner.

## [02]-[DIFFERENTIAL]

- Owner: `DifferentialIntent` — the differential-equations cases on the one solver; `Ode(vector_field, y0, span)` over `diffrax.diffeqsolve` with an `ODETerm` and an `OdeSolver`-selected explicit-RK (`Tsit5`/`Dopri5`/`Dopri8`) or ESDIRK/SDIRK stiff (`KenCarp3`/`KenCarp4`/`KenCarp5`/`Kvaerno3`/`Kvaerno4`/`Kvaerno5`) integrator, `Sde(drift, diffusion, y0, span)` over an `SdeSolver`-selected stochastic integrator with a `BrownianPath`-selected path — the order-1 `EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`LeapfrogMidpoint`/`SemiImplicitEuler` family and the strong-order-1.5 `SRA1`/`ShARK`/`GeneralShARK`/`SlowRK`/`SEA`/`SPaRK` family over a `MultiTerm(ODETerm, ControlTerm)`, plus the underdamped-Langevin `ALIGN`/`ShOULD`/`QUICSORT` family over a `MultiTerm(UnderdampedLangevinDriftTerm, UnderdampedLangevinDiffusionTerm)` keyed by `gamma`/`langevin_u` on a `(x, v)` state, each family's required Levy-area level folded into the path by `_LEVY` — and `Cde(vector_field, control, y0, span)` over a `ControlTerm` driven by a `backward_hermite_coefficients`-prepared `CubicInterpolation` control path. The integration policy is one `IntegratePolicy` struct carried in every case rather than a free `solve` kwarg or a scatter of literals: `StepKind` selects `ConstantStepSize` or the adaptive `PIDController` (parameterised by the policy's `rtol`/`atol` and the optional `dtmin`/`dtmax` step clamps threaded straight into the controller), `AdjointMode` selects `RecursiveCheckpointAdjoint`/`BacksolveAdjoint`/`ImplicitAdjoint`/`DirectAdjoint`/`ForwardMode` for the differentiable solve — the reverse-mode checkpoint, continuous backsolve, implicit-function-theorem, and unrolled-reverse modes for the few-outputs/many-parameters reverse regime and `ForwardMode` for the forward-sensitivity many-outputs/few-parameters and batched-sweep regime `solvers/sensitivity.md#SENSITIVITY` differentiates through — `SaveKind` selects terminal-only or dense `SaveAt(t1=True, dense=True)` output, the dense mode backing a non-`None` `Solution.interpolation` on the differentiable `Solution` that `solvers/sensitivity.md#SENSITIVITY` resamples through the adjoint while the receipt carries only the terminal verdict, `EventKind` selects no event, the built-in `steady_state_event()`, or a root-finding `Event(condition, root_finder)` terminal crossing, `BrownianPath` selects the reproducible reverse-mode-safe `VirtualBrownianTree` or the faster forward-only `UnsafeBrownianPath`, `gamma`/`langevin_u` carry the underdamped-Langevin friction and inverse-mass the Langevin term-pair reads, `init_steps` seeds the initial `dt0=(t1 - t0) / init_steps` for both the `diffeqsolve` march and the numpy-floor grid rather than a buried `1000` literal, `noise_dim` sets the path width and falls to the last leaf's last dimension only for the diagonal-noise case where state and noise dimensions coincide, and `max_steps` lifts the `diffeqsolve` step budget a long-horizon stiff solve overruns rather than resting on the `4096` default. The `SolveEngine.adjoint`/`event`/`controller`/`save` carrier methods fold each policy member into its diffrax object off the `_ADJOINT`/`_EVENT` builder tables, and `controller` is the one `_forced_pid` gate that selects `PIDController`/`ConstantStepSize` from `StepKind`, the event arming, and the solver's adaptivity (`_LEVY` membership on the SDE arm) — so a steady-state target, a contact crossing, a step-clamped stiff march, an order-1 fixed-step Milstein solve, an underdamped-Langevin sampler, and a memory-checkpointed reverse-mode adjoint are policy rows and data cells, never branches.
- Solver dispatch: `_SOLVER(dfx)` is the ONE table mapping every `OdeSolver`/`SdeSolver` member onto its diffrax solver class — `Tsit5`, `Dopri5`, `Dopri8`, `KenCarp3`, `KenCarp4`, `KenCarp5`, `Kvaerno3`, `Kvaerno4`, `Kvaerno5` on the deterministic side; the order-1 `EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`LeapfrogMidpoint`/`SemiImplicitEuler` plus the strong-order-1.5 `SRA1`/`ShARK`/`GeneralShARK`/`SlowRK`/`SEA`/`SPaRK` and the underdamped-Langevin `ALIGN`/`ShOULD`/`QUICSORT` on the stochastic side. Three orthogonal SDE axes ride beside it. `_LEVY` is the band-independent Brownian-coupling axis: it keys each high-order/Langevin `SdeSolver` to the `LevyLevel` string its path must supply and `_LEVY_CLASS(dfx)` resolves that level to the `SpaceTimeLevyArea`/`SpaceTimeTimeLevyArea` class folded into the path's `levy_area=`, so the solver row and the path row stay two cells `_terms` reads rather than a `match` over solver families — a high-order solver fed a plain-increment path is unrepresentable, never a runtime guard. `_LANGEVIN` is the term-shape axis: membership selects the `SolveEngine.langevin_terms` `MultiTerm(UnderdampedLangevinDriftTerm, UnderdampedLangevinDiffusionTerm)` pair keyed by `policy.gamma`/`policy.langevin_u` on a `(x, v)` state where the plain family builds `MultiTerm(ODETerm, ControlTerm)`, so the underdamped-Langevin family is one membership cell, never a fourth equation case or a per-solver arm. `BrownianPath` is the path-generator axis: `VirtualBrownianTree(t0, t1, tol, shape, key, levy_area=)` for the reproducible reverse-mode-safe path and `UnsafeBrownianPath(shape, key, levy_area=)` for the faster forward-only path, selected by the `SolveEngine.reproducible_path` gate that floors `UNSAFE` to `VirtualBrownianTree` under `AdjointMode.BACKSOLVE` — a continuous backsolve adjoint reconstructs the path at backward time-points the forward-only `UnsafeBrownianPath` cannot supply, so the `(UNSAFE, BACKSOLVE)` pairing is unsatisfiable by construction rather than a solve-time backend fault, the same flooring discipline `_forced_pid` runs over the controller. Because the diffrax classes resolve only on the gated band the `dfx`-keyed tables build from the carrier's `dfx` at solve time while `_LEVY`/`_LANGEVIN` stay band-independent module constants — the same band-local discipline `solvers/nonlinear.md#NONLINEAR` runs for its Optimistix dispatch — so the dispatch is total over the vocabulary with no `NotImplemented` arm, no hardcoded `Tsit5()`, and no dead solver: each advertised solver is reachable through one row. A new diffrax solver class is one `_SOLVER` row plus, when it is high-order/Langevin, one `_LEVY` row and, when it is Langevin, one `_LANGEVIN` member; the integrate path never grows a `match` arm per solver.
- Term dispatch: the three equation cases vary only in how the `terms` argument is built, whether a Brownian path threads in, and how the residual contracts; the `t0`/`t1`/`dt0`/`y0`/`adjoint`/`stepsize_controller`/`saveat`/`event`/`max_steps`/`throw` arguments are shared engine reads. The one `match intent` arm in `_terms` reads the case and binds the `(terms, residual)` pair — an `ODETerm` with the steady-state residual `‖f(t1, y_T)‖` for the ODE case, the SDE case building the `BrownianPath`-selected path keyed by a split `jax.random` key then folding `_LANGEVIN` membership to either the `engine.langevin_terms` pair or the plain `MultiTerm(ODETerm, ControlTerm)`, and a `ControlTerm` over the hermite-prepared `CubicInterpolation` for the CDE case — co-locating each term construction with its `engine.tree_norm` residual contraction while the single `diffeqsolve` call site in `_diffrax_receipt` runs every case. Every residual is the one `engine.tree_norm` per-leaf sum-of-squares fold total over a structured terminal pytree (the ODE steady-state field, the SDE/CDE terminal state, the Langevin `(x, v)` pair), so the per-case arms collapse to three `(terms, residual)` rows sharing one norm rather than three single-array-assuming `jnp.linalg.norm` calls that a multi-leaf state breaks. `_terms` is the term-builder the call site reads, not a residual-splitting helper: it returns the `(terms, residual)` pair so the contraction stays bound to the terms it contracts.
- Entry: `DifferentialIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; `_diffrax_receipt` builds one `SolveEngine.gated()` carrier and runs one `engine.dfx.diffeqsolve(terms, solver, ..., max_steps=policy.max_steps, throw=False)` under `throw=False` — the load-bearing knob, since the `True` default raises on any non-`successful` result and the `boundary` `catch=Exception` would then convert a `max_steps_reached` solve into a `BoundaryFault` rail rather than a verdict, dead-coding the status fold — reads the terminal state through `engine.terminal` (the per-leaf `tree_map` total over a structured `y0` pytree), contracts the one `engine.tree_norm` per-leaf residual — the steady-state field `‖f(t1, y_T)‖` (ODE) or the terminal-state norm (SDE/CDE/Langevin) — reads `Solution.stats["num_steps"]`, maps the `RESULTS` member name (`SolveEngine.verdict` inverting `RESULTS._name_to_item` off the `Solution.result._value` code, since the `EnumerationItem` carries no `.name`) through the receipt's `_STATUS` table, and folds them into `SolverReceipt.Iterative` with the diffrax termination reason as the adjudicated status — so a `max_steps_reached`, an `event_occurred`, or a `dt_min_reached` solve is a distinct first-class verdict, never collapsed to a residual-only floor, exactly as `solvers/nonlinear.md#NONLINEAR` reads its Optimistix `RESULTS` verdict under the same `throw=False`. `SolveEngine._forced_pid` is the one hard gate over `controller` and `event`: `adaptive_capable` is true for every ODE/CDE solver and for the order-1.5/Langevin SDE family (`_LEVY` membership), false for the order-1 `SdeSolver` family that carries no error estimate, so `_forced_pid` is `adaptive_capable and (event armed or StepKind.PID)` and a non-`NONE` `EventKind` or a requested `StepKind.PID` selects the `PIDController` only on an adaptive-capable solver. An order-1 SDE solver floors to `ConstantStepSize` and — because a diffrax event needs adaptive stepping — arms no event rather than placing an adaptive controller on a solver that supplies no error estimate it consumes, so the (fixed-step + event) configuration is total: `controller` is fixed and `event` returns `None`, never a mis-stepped adaptive march. When the jaxlib wheel is absent the ODE case falls to `_euler_floor`: a fixed-step explicit-Euler march over `numpy` reusing the policy's `init_steps` grid count, returning the steady-state residual and step count with `result=None`, so the receipt's residual-against-tolerance floor adjudicates the verdict; the SDE/CDE/Langevin cases hold no numpy floor because a stochastic, rough-path, or underdamped-Langevin integrator is the gated capability itself. Emission rides the runtime `@receipted(_REDACTION)` aspect the measured `_dispatch` kernel wears, so the `SolverReceipt.contribute` stream emits on exit rather than an inline `Signals.emit` threaded through each receipt body — matching every sibling solver route.
- Packages: `diffrax` (`diffeqsolve`, `ODETerm`, `ControlTerm`, `MultiTerm`, `UnderdampedLangevinDriftTerm`, `UnderdampedLangevinDiffusionTerm`, `Tsit5`, `Dopri5`, `Dopri8`, `KenCarp3`, `KenCarp4`, `KenCarp5`, `Kvaerno3`, `Kvaerno4`, `Kvaerno5`, `EulerHeun`, `ItoMilstein`, `StratonovichMilstein`, `ReversibleHeun`, `LeapfrogMidpoint`, `SemiImplicitEuler`, `SRA1`, `ShARK`, `GeneralShARK`, `SlowRK`, `SEA`, `SPaRK`, `ALIGN`, `ShOULD`, `QUICSORT`, `SpaceTimeLevyArea`, `SpaceTimeTimeLevyArea`, `PIDController`, `ConstantStepSize`, `SaveAt`, `VirtualBrownianTree`, `UnsafeBrownianPath`, `CubicInterpolation`, `backward_hermite_coefficients`, `Event`, `steady_state_event`, `RecursiveCheckpointAdjoint`, `BacksolveAdjoint`, `ImplicitAdjoint`, `DirectAdjoint`, `ForwardMode`, `Solution`, `RESULTS`, `RESULTS._name_to_item` inverted through `SolveEngine.verdict` to recover the member name off the `Solution.result._value` code since an `EnumerationItem` carries no `.name`, the batched sweep reducing the per-row `_value` codes by `jnp.max` rather than `RESULTS.promote` which is inheritance-widening not a vmap combine), `equinox` (`filter_jit` — the field thunks and the per-row solve compile through the JAX-native transform; `filter_vmap` maps a batched `y0` stack over the leading axis for the forward-sensitivity sweep `solvers/sensitivity.md#SENSITIVITY` differentiates through under `AdjointMode.FORWARD`), `jax` (`config.update("jax_enable_x64", True)` floating the gated solve to float64 so the `1e-8` `rtol`/`atol` and the stiff/adjoint solves are reachable rather than silently clamped at float32 eps, `numpy.asarray`, `random.key`, `random.split` for the Brownian seed lineage, `tree_util.tree_map`/`tree_reduce`/`tree_leaves` for the `engine.lift` per-leaf `y0` lift, the `engine.terminal` read, and the `engine.tree_norm` residual total over a structured `(x, v)` Langevin / multi-leaf `y0` pytree), `numpy` (`asarray`, `linalg.norm`, `linspace` for the explicit-Euler floor), `expression` (`tag`, `case`, `tagged_union` for the `DifferentialIntent` union; `expression.collections.Map` for the empty `Redaction.classified` policy), `dataclasses` (`dataclass(frozen=True, slots=True)` for the `SolveEngine` carrier, `Self`-bound `gated()` matching the sibling routes), `beartype` (`FrozenDict` for the `_LEVY`/`_SOLVER`/`_LEVY_CLASS`/`_ADJOINT`/`_EVENT` tables), `msgspec` (`Struct` for the `IntegratePolicy` record), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`, the `Redaction` empty-`classified` policy and the `@receipted` aspect the `_dispatch` kernel wears, plus `Signals` whose `msgspec` encoder carries the receipt's native scalars).
- Growth: a deterministic solver is one `OdeSolver` member plus one `_SOLVER` row; a stochastic solver is one `SdeSolver` member plus one `_SOLVER` row and, when it is strong-order-1.5/Langevin, one `_LEVY` row carrying its Brownian-path level and admitting it to the adaptive controller; an underdamped-Langevin solver adds one `_LANGEVIN` member selecting the Langevin term-pair, never a fourth equation case; a new equation class is one `DifferentialIntent` case plus one arm in the term-building `match` (the bounded ode/sde/cde dispatch, never a per-solver arm); a new step controller, adjoint mode, save mode, event kind, or path generator is one `StepKind`/`AdjointMode`/`SaveKind`/`EventKind`/`BrownianPath` member plus one row or ternary in its fold; a new integration scalar (`dtmin`/`dtmax`/`init_steps`/`noise_dim`/`max_steps`/`gamma`/`langevin_u`) is one `IntegratePolicy` field threaded into the controller, the path, the Langevin term, or the solve; a multi-state study sets `IntegratePolicy.batched` and reuses the same `solve` through `filter_vmap`, never a Python loop over the start stack; a new termination class is one `_STATUS` row on the receipt owner; a new gated module is one `SolveEngine` field plus one `gated()` import line read off the carrier; zero new surface, zero new entrypoint, zero free `solve` knob.
- Boundary: `diffrax`/`equinox`/`jaxlib` carry no cp315 wheel, so the gated solve body is authored against the documented API on the JAX floor; the ODE case carries a reachable numpy explicit-Euler floor matching every sibling route, while the SDE/CDE/Langevin cases are gated-only because a stochastic, rough-path, or underdamped-Langevin integrator is the gated capability itself. The adjoint solve feeds `solvers/sensitivity.md#SENSITIVITY`, whose trajectory Jacobian `experiments/study.md#STUDY` reads for its DGSM screen; a hand-rolled adaptive Runge-Kutta loop, a scalar quadrature route here, a hardcoded `Tsit5()`, a bare `jnp.asarray(y0)`/`numpy.asarray(y0)` lift or a `jnp.asarray(solution.ys)[-1]` terminal read that flattens a structured `(x, v)` Langevin / multi-leaf pytree where the per-leaf `jax.tree_util.tree_map` lift and read belong, a single-array `jnp.linalg.norm` residual where the `engine.tree_norm` per-leaf fold is total over the terminal pytree, a gated solve left on the JAX default float32 where the `1e-8` tolerance is below float32 eps and the `jax.config.update("jax_enable_x64", True)` floor belongs, a loose `(dfx, eqx, jnp, jtu)` module quadruple threaded positionally through a free `_tree_norm`/`_langevin_terms` helper where `SolveEngine` reads them off the carrier and `gated()` floats the rail once, three parallel `diffeqsolve` bodies, a fourth equation case for the underdamped-Langevin family where one `_LANGEVIN` membership cell selects its term-pair on the SDE arm, a free `adjoint` kwarg on `solve`, a `NotImplemented` solver arm, a per-solver `match` arm, a `diffeqsolve` left at the `throw=True` default that raises a non-`successful` termination into the `boundary` rail instead of folding it to a verdict, a high-order `SRA1`/`ShARK`/`SEA`/`SPaRK`/`ALIGN`/`ShOULD`/`QUICSORT` (space-time) or `SlowRK` (space-time-time) solver fed a plain `BrownianIncrement` path where its `_LEVY`-required Levy-area level belongs, a `match` over solver families to pick the Levy level where one `_LEVY` row carries it, a hardcoded `VirtualBrownianTree` where the `BrownianPath` axis selects the forward-only `UnsafeBrownianPath`, a forward-only `UnsafeBrownianPath` left under a `BacksolveAdjoint` continuous-adjoint solve that reconstructs the path at backward time-points it cannot supply where `SolveEngine.reproducible_path` floors it to `VirtualBrownianTree`, an adaptive `PIDController` placed on an order-1 fixed-step `SdeSolver` that supplies no error estimate, a Brownian width hardcoded to `y0`'s last dimension where the `noise_dim` policy scalar carries the noise width, a `dt0=(t1 - t0) / 1000` literal where the `init_steps` policy scalar belongs, a buried `max_steps`/`dtmin`/`dtmax`/`gamma`/`langevin_u` literal where an `IntegratePolicy` scalar belongs, a Python loop over a batched initial-state stack where one `filter_vmap` under `IntegratePolicy.batched` maps the compiled solve over the leading axis, a batched solve discarding its per-row `Solution.result` to `result=None` where `SolveEngine.verdict` reduces the per-row `_value` codes by `jnp.max` to the worst-case verdict, a `RESULTS.promote(...)` batched combine (inheritance-widening that raises on a same-class member, NOT a vmap reduction) or a `solution.result.name` read off an `EnumerationItem` that carries no `.name` where `SolveEngine.verdict` inverts `RESULTS._name_to_item`, and an inline `Signals.emit` in the dispatch body where the `@receipted(_REDACTION)` aspect owns egress are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from functools import reduce
from typing import Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Redaction, receipted


# --- [TYPES] -------------------------------------------------------------------------------

# y0, every field value, and every solve value are arbitrary JAX pytrees diffrax tracks through
# the integration; the numpy floor narrows to np.ndarray at its jaxlib-free boundary. The field
# thunks are (t, y) -> dy/drift/diffusion; the CDE control is a sampled (ts, ys) pair the
# backward_hermite_coefficients prep lowers into a CubicInterpolation.
type Pytree = object
type FieldFn = Callable[[float, Pytree], Pytree]
type Span = tuple[float, float]
type ControlPath = tuple[np.ndarray, np.ndarray]


class StepKind(StrEnum):
    CONSTANT = "constant"
    PID = "pid"


class AdjointMode(StrEnum):
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"
    BACKSOLVE = "backsolve"
    IMPLICIT = "implicit"
    DIRECT = "direct"  # reverse-mode autodiff through the unrolled solve
    FORWARD = "forward"  # forward-mode autodiff through the solve; the few-parameters / batched-sweep sensitivity adjoint


class SaveKind(StrEnum):
    TERMINAL = "terminal"
    DENSE = "dense"


class EventKind(StrEnum):
    NONE = "none"
    STEADY_STATE = "steady_state"
    ROOT_FIND = "root_find"


class OdeSolver(StrEnum):
    TSIT5 = "tsit5"
    DOPRI5 = "dopri5"
    DOPRI8 = "dopri8"
    KENCARP3 = "kencarp3"
    KENCARP4 = "kencarp4"
    KENCARP5 = "kencarp5"
    KVAERNO3 = "kvaerno3"
    KVAERNO4 = "kvaerno4"
    KVAERNO5 = "kvaerno5"


class SdeSolver(StrEnum):
    EULER_HEUN = "euler_heun"
    ITO_MILSTEIN = "ito_milstein"
    STRATONOVICH_MILSTEIN = "stratonovich_milstein"
    REVERSIBLE_HEUN = "reversible_heun"
    LEAPFROG_MIDPOINT = "leapfrog_midpoint"
    SEMI_IMPLICIT_EULER = "semi_implicit_euler"
    SRA1 = "sra1"
    SHARK = "shark"
    GENERAL_SHARK = "general_shark"
    SLOW_RK = "slow_rk"
    SEA = "sea"
    SPARK = "spark"
    ALIGN = "align"  # underdamped Langevin; pairs with the UnderdampedLangevin*Term position-velocity term
    SHOULD = "should"
    QUICSORT = "quicsort"


class BrownianPath(StrEnum):
    VIRTUAL = "virtual"  # VirtualBrownianTree — reproducible, reverse-mode-safe, BacksolveAdjoint-admitting
    UNSAFE = "unsafe"  # UnsafeBrownianPath — faster forward-only path, no reverse-mode reconstruction


# Levy-area level a strong-order-1.5 SDE solver demands on its VirtualBrownianTree; the order-1
# family carries no entry and falls to the BrownianIncrement default. SpaceTimeTimeLevyArea is a
# superset of SpaceTimeLevyArea, so SlowRK's level also satisfies the space-time solvers.
type LevyLevel = Literal["space_time", "space_time_time"]


# --- [CONSTANTS] ---------------------------------------------------------------------------

# The Brownian-coupling axis, band-independent because it keys SdeSolver members onto a LevyLevel
# string the gated _LEVY_CLASS later resolves: SRA1/ShARK/GeneralShARK/SEA/SPaRK/ALIGN/ShOULD/QUICSORT
# demand SpaceTimeLevyArea and SlowRK SpaceTimeTimeLevyArea, the order-1 family carrying no row and
# keeping the BrownianIncrement default. Membership doubles as the SDE adaptivity witness, so a
# mis-paired path is unrepresentable and an order-1 solver floors to ConstantStepSize.
_LEVY: FrozenDict[SdeSolver, LevyLevel] = FrozenDict(
    {
        SdeSolver.SRA1: "space_time",
        SdeSolver.SHARK: "space_time",
        SdeSolver.GENERAL_SHARK: "space_time",
        SdeSolver.SEA: "space_time",
        SdeSolver.SPARK: "space_time",
        SdeSolver.ALIGN: "space_time",
        SdeSolver.SHOULD: "space_time",
        SdeSolver.QUICSORT: "space_time",
        SdeSolver.SLOW_RK: "space_time_time",
    }
)

# The term-shape axis: membership selects the MultiTerm(UnderdampedLangevinDriftTerm,
# UnderdampedLangevinDiffusionTerm) pair keyed by gamma/u over a (x, v) state where the plain SDE
# family builds MultiTerm(ODETerm, ControlTerm) — one membership cell, never a per-solver arm. The
# drift thunk supplies the potential gradient grad_f; the diffusion thunk is unused because the
# fluctuation-dissipation diffusion is fixed by gamma/u, and y0 IS the (x, v) position-velocity pair.
_LANGEVIN: frozenset[SdeSolver] = frozenset({SdeSolver.ALIGN, SdeSolver.SHOULD, SdeSolver.QUICSORT})

# Field-redaction policy the `@receipted` aspect binds; the integration facts carry no secret, so the
# classification `Map` is empty and every fact reaches the line natively, exactly as the sibling solver
# routes bind it — one policy object the aspect threads, never a per-call construction.
_REDACTION: Redaction = Redaction(classified=Map.empty())


# --- [MODELS] ------------------------------------------------------------------------------

class IntegratePolicy(Struct, frozen=True):
    step: StepKind = StepKind.PID
    adjoint: AdjointMode = AdjointMode.RECURSIVE_CHECKPOINT
    save: SaveKind = SaveKind.TERMINAL
    event: EventKind = EventKind.NONE
    rtol: float = 1e-8
    atol: float = 1e-8
    dtmin: float | None = None  # PIDController lower step clamp; None lets the controller pick freely
    dtmax: float | None = None  # PIDController upper step clamp for a stiff long-horizon march
    init_steps: int = 1000  # ConstantStepSize grid count seeding dt0=(t1-t0)/init_steps, never a buried literal
    max_steps: int = 4096  # diffeqsolve step budget; raised past the default for stiff long horizons
    noise_dim: int | None = None  # Brownian path width; None couples noise to y0's last dim for diagonal noise
    brownian: BrownianPath = BrownianPath.VIRTUAL  # path generator; UNSAFE is forward-only, floored to VIRTUAL under BACKSOLVE
    gamma: float = 1.0  # underdamped-Langevin friction coefficient; read only by the ALIGN/ShOULD/QUICSORT arm
    langevin_u: float = 1.0  # underdamped-Langevin inverse-mass (1/m); read only by the Langevin arm
    condition: Callable[..., Pytree] | None = None  # diffrax Event cond_fn(t, y, args, **kw); ROOT_FIND only
    root_finder: object | None = None  # an optimistix AbstractRootFinder deferred behind the gated band
    batched: bool = False  # y0 carries a leading sweep axis filter_vmapped through one compiled solve under AdjointMode.FORWARD
    seed: int = 0


@tagged_union(frozen=True)
class DifferentialIntent:
    tag: Literal["ode", "sde", "cde"] = tag()
    ode: tuple[FieldFn, Pytree, Span, OdeSolver, IntegratePolicy] = case()
    sde: tuple[FieldFn, FieldFn, Pytree, Span, SdeSolver, IntegratePolicy] = case()
    cde: tuple[FieldFn, ControlPath, Pytree, Span, OdeSolver, IntegratePolicy] = case()

    @staticmethod
    def Ode(
        vector_field: FieldFn,
        y0: Pytree,
        span: Span,
        solver: OdeSolver = OdeSolver.TSIT5,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(ode=(vector_field, y0, span, solver, policy))

    @staticmethod
    def Sde(
        drift: FieldFn,
        diffusion: FieldFn,
        y0: Pytree,
        span: Span,
        solver: SdeSolver = SdeSolver.EULER_HEUN,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(sde=(drift, diffusion, y0, span, solver, policy))

    @staticmethod
    def Cde(
        vector_field: FieldFn,
        control: ControlPath,
        y0: Pytree,
        span: Span,
        solver: OdeSolver = OdeSolver.TSIT5,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(cde=(vector_field, control, y0, span, solver, policy))

    def solve(self) -> "RuntimeRail[SolverReceipt]":
        return boundary(f"solve.{self.tag}", lambda: _dispatch(self))


# The gated modules folded into one value object with behavior built ONCE per solve: `controller`,
# `event`, `save`, `adjoint`, `lift`, `terminal`, `tree_norm`, `last_dim`, and `langevin_terms` read
# `self.dfx`/`self.eqx`/`self.jnp`/`self.jtu`/`self.jr` off the carrier rather than each helper
# re-importing or threading a loose (dfx, eqx, jnp, jtu) handle quadruple. `gated()` imports once behind
# the band and floats the rail to float64 — the DiffEngine.gated()/SolveEngine.gated() discipline
# `solvers/sensitivity.md#SENSITIVITY` and `solvers/nonlinear.md#NONLINEAR` run, so the JAX flow
# (build -> solve -> read) is one rail and the float64 promotion fires once.
@dataclass(frozen=True, slots=True)
class SolveEngine:
    dfx: object
    eqx: object
    jnp: object
    jtu: object
    jr: object

    @classmethod
    def gated(cls) -> Self:
        import diffrax as dfx
        import equinox as eqx
        import jax
        import jax.numpy as jnp
        import jax.random as jr
        import jax.tree_util as jtu

        jax.config.update("jax_enable_x64", True)  # 1e-8 rtol/atol and stiff/adjoint solves assume float64; JAX defaults to float32
        return cls(dfx=dfx, eqx=eqx, jnp=jnp, jtu=jtu, jr=jr)

    # The one terminal/steady-state residual norm, total over a structured y0 pytree (the Langevin
    # (x, v) pair, a multi-leaf state) where a bare jnp.linalg.norm assumes a single array leaf: the
    # per-leaf sum-of-squares fold reduces to the global L2 norm, collapsing to plain norm on one array.
    # Returns the traced jnp scalar — the caller coerces to float at the eager boundary, never inside
    # filter_vmap where float() on a Tracer raises; the batched path folds the stacked scalars first.
    def tree_norm(self, tree: object) -> object:
        squared = self.jtu.tree_map(lambda leaf: self.jnp.sum(self.jnp.asarray(leaf) ** 2), tree)
        return self.jtu.tree_reduce(lambda a, b: a + b, squared, 0.0) ** 0.5

    def verdict(self, result: object, *, batched: bool) -> str:
        # the `RESULTS` member NAME the receipt's `_STATUS` table keys on, recovered by inverting the class
        # `_name_to_item` code->item map — the diffrax `RESULTS` is an `equinox.Enumeration` whose item carries
        # ONLY `_value`/`_enumeration` (no `.name`, `RESULTS[item]` yields the human MESSAGE), the same
        # `optimization/design.md#DESIGN` `_result_names` inversion. The batched sweep reduces the worst-case
        # verdict by `jnp.max` over the per-row `_value` codes (the zero `successful` makes `max == 0` iff every
        # start converged); `RESULTS.promote` is NOT a batch combine — it widens a member from a parent
        # `Enumeration` to a subclass and raises on a same-class member — so the reduction is the code max,
        # never `promote(result)`, and the single path inverts the one `int(result._value)`.
        code = int(self.jnp.max(result._value)) if batched else int(result._value)
        names = {int(item._value): name for name, item in self.dfx.RESULTS._name_to_item.items()}
        return names.get(code, "")

    def lift(self, y0: Pytree) -> object:  # per-leaf, total over a structured (x, v) / multi-leaf y0; a bare jnp.asarray flattens the pytree
        return self.jtu.tree_map(self.jnp.asarray, y0)

    def terminal(self, solution: object) -> object:  # last save on every leaf; total over the y0 pytree
        return self.jtu.tree_map(lambda leaf: self.jnp.asarray(leaf)[-1], solution.ys)

    def last_dim(self, y0: Pytree) -> int:  # diagonal-noise width: y0's last leaf's last dimension
        return int(self.jnp.asarray(self.jtu.tree_leaves(y0)[-1]).shape[-1])

    def adjoint(self, mode: AdjointMode) -> object:
        return _ADJOINT(self.dfx)[mode]()

    def save(self, policy: IntegratePolicy) -> object:
        return self.dfx.SaveAt(t1=True, dense=policy.save is SaveKind.DENSE)

    # A BACKSOLVE continuous adjoint integrates backward and reconstructs the Brownian path at backward
    # time-points; UnsafeBrownianPath is forward-only and non-reproducible, so the reverse pass starves.
    # The path floors to the reproducible VirtualBrownianTree under BACKSOLVE regardless of BrownianPath,
    # so the (UNSAFE, BACKSOLVE) pairing is unsatisfiable by construction, never a solve-time backend fault.
    def reproducible_path(self, policy: IntegratePolicy) -> bool:
        return policy.brownian is BrownianPath.VIRTUAL or policy.adjoint is AdjointMode.BACKSOLVE

    # The underdamped-Langevin (x, v) term-pair: friction gamma and inverse-mass u parameterise the
    # UnderdampedLangevinDriftTerm/DiffusionTerm the ALIGN/ShOULD/QUICSORT solvers consume, the drift
    # thunk supplying the potential gradient; the exact constructor binding resolves at the gated
    # reflection pass against compute/.api/diffrax.md. One method isolates it from the SDE arm.
    def langevin_terms(self, policy: IntegratePolicy, drift: FieldFn, brownian: object) -> object:
        grad = self.eqx.filter_jit(lambda t, y, _: drift(t, y))
        return self.dfx.MultiTerm(
            self.dfx.UnderdampedLangevinDriftTerm(gamma=policy.gamma, u=policy.langevin_u, grad_f=grad),
            self.dfx.UnderdampedLangevinDiffusionTerm(gamma=policy.gamma, u=policy.langevin_u, bm=brownian),
        )

    # adaptive_capable is the hard gate over BOTH the event-forced and StepKind.PID paths: an order-1
    # SDE solver (absent from _LEVY) carries no error estimate, so it is fixed-step only and a diffrax
    # event armed on it cannot force a PIDController the solver starves — the configuration floors to
    # ConstantStepSize and the event never arms, never an adaptive controller on a starved solver.
    # ODE/CDE explicit-RK and ESDIRK/SDIRK solvers are always adaptive-capable.
    def _forced_pid(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> bool:
        adaptive_capable = tag != "sde" or solver in _LEVY
        return adaptive_capable and (policy.event is not EventKind.NONE or policy.step is StepKind.PID)

    def controller(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> object:
        if self._forced_pid(tag, solver, policy):
            return self.dfx.PIDController(rtol=policy.rtol, atol=policy.atol, dtmin=policy.dtmin, dtmax=policy.dtmax)
        return self.dfx.ConstantStepSize()

    def event(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> object | None:
        return _EVENT(self.dfx, policy)[policy.event]() if self._forced_pid(tag, solver, policy) else None


# --- [TABLES] ------------------------------------------------------------------------------

# diffrax resolves only on the gated band, so these tables are built from the carrier's `dfx` at solve
# time rather than at module import. Each is total over its vocabulary with no NotImplemented arm: a new
# solver is one _SOLVER row, a new adjoint mode one _ADJOINT row, a new event kind one _EVENT row.
def _SOLVER(dfx: object) -> FrozenDict[OdeSolver | SdeSolver, Callable[[], object]]:
    return FrozenDict(
        {
            OdeSolver.TSIT5: dfx.Tsit5,
            OdeSolver.DOPRI5: dfx.Dopri5,
            OdeSolver.DOPRI8: dfx.Dopri8,
            OdeSolver.KENCARP3: dfx.KenCarp3,
            OdeSolver.KENCARP4: dfx.KenCarp4,
            OdeSolver.KENCARP5: dfx.KenCarp5,
            OdeSolver.KVAERNO3: dfx.Kvaerno3,
            OdeSolver.KVAERNO4: dfx.Kvaerno4,
            OdeSolver.KVAERNO5: dfx.Kvaerno5,
            SdeSolver.EULER_HEUN: dfx.EulerHeun,
            SdeSolver.ITO_MILSTEIN: dfx.ItoMilstein,
            SdeSolver.STRATONOVICH_MILSTEIN: dfx.StratonovichMilstein,
            SdeSolver.REVERSIBLE_HEUN: dfx.ReversibleHeun,
            SdeSolver.LEAPFROG_MIDPOINT: dfx.LeapfrogMidpoint,
            SdeSolver.SEMI_IMPLICIT_EULER: dfx.SemiImplicitEuler,
            SdeSolver.SRA1: dfx.SRA1,
            SdeSolver.SHARK: dfx.ShARK,
            SdeSolver.GENERAL_SHARK: dfx.GeneralShARK,
            SdeSolver.SLOW_RK: dfx.SlowRK,
            SdeSolver.SEA: dfx.SEA,
            SdeSolver.SPARK: dfx.SPaRK,
            SdeSolver.ALIGN: dfx.ALIGN,
            SdeSolver.SHOULD: dfx.ShOULD,
            SdeSolver.QUICSORT: dfx.QUICSORT,
        }
    )


def _LEVY_CLASS(dfx: object) -> FrozenDict[LevyLevel, object]:
    return FrozenDict({"space_time": dfx.SpaceTimeLevyArea, "space_time_time": dfx.SpaceTimeTimeLevyArea})


def _ADJOINT(dfx: object) -> FrozenDict[AdjointMode, Callable[[], object]]:
    return FrozenDict(
        {
            AdjointMode.RECURSIVE_CHECKPOINT: dfx.RecursiveCheckpointAdjoint,
            AdjointMode.BACKSOLVE: dfx.BacksolveAdjoint,
            AdjointMode.IMPLICIT: dfx.ImplicitAdjoint,
            AdjointMode.DIRECT: dfx.DirectAdjoint,
            AdjointMode.FORWARD: dfx.ForwardMode,
        }
    )


def _EVENT(dfx: object, policy: IntegratePolicy) -> FrozenDict[EventKind, Callable[[], object | None]]:
    return FrozenDict(
        {
            EventKind.NONE: lambda: None,
            EventKind.STEADY_STATE: lambda: dfx.Event(dfx.steady_state_event()),
            EventKind.ROOT_FIND: lambda: dfx.Event(policy.condition, policy.root_finder),
        }
    )


# --- [OPERATIONS] --------------------------------------------------------------------------

# `@receipted(_REDACTION)` wraps the measured dispatch and emits its `SolverReceipt.contribute` stream
# on exit, so receipt egress is the decorator rail every sibling solver route wears rather than an
# inline `Signals.emit` threaded through each receipt body.
@receipted(_REDACTION)
def _dispatch(intent: DifferentialIntent) -> SolverReceipt:
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, y0, (t0, t1), solver, policy)):
            try:
                return _diffrax_receipt(intent, solver, y0, t0, t1, policy)
            except ImportError:
                return _euler_floor(field, np.asarray(y0), t0, t1, policy)
        case (
            DifferentialIntent(tag="sde", sde=(_, _, y0, (t0, t1), solver, policy))
            | DifferentialIntent(tag="cde", cde=(_, _, y0, (t0, t1), solver, policy))
        ):
            return _diffrax_receipt(intent, solver, y0, t0, t1, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _diffrax_receipt(
    intent: DifferentialIntent,
    solver: OdeSolver | SdeSolver,
    y0: Pytree,
    t0: float,
    t1: float,
    policy: IntegratePolicy,
) -> SolverReceipt:
    engine = SolveEngine.gated()  # imports the gated modules once and floats the rail to float64
    terms, residual = _terms(engine, intent, solver, y0, t0, t1, policy)
    cls, controller = _SOLVER(engine.dfx)[solver](), engine.controller(intent.tag, solver, policy)
    saveat, adjoint, event = engine.save(policy), engine.adjoint(policy.adjoint), engine.event(intent.tag, solver, policy)

    def run(start: object) -> object:
        return engine.dfx.diffeqsolve(
            terms,
            cls,
            t0=t0,
            t1=t1,
            dt0=(t1 - t0) / policy.init_steps,
            y0=start,
            stepsize_controller=controller,
            saveat=saveat,
            adjoint=adjoint,
            event=event,
            max_steps=policy.max_steps,
            throw=False,  # return the non-successful RESULTS for the receipt fold; the True default raises
        )

    if policy.batched:  # leading axis of y0 is an initial-state sweep; one compiled filter_vmap solve, never a Python loop
        solutions = engine.eqx.filter_vmap(engine.eqx.filter_jit(run), in_axes=0)(engine.lift(y0))
        per_row = engine.eqx.filter_vmap(lambda s: residual(engine.terminal(s)), in_axes=0)(solutions)
        worst = float(engine.jnp.max(engine.jnp.asarray(per_row)))  # worst per-row residual; each is itself a tree_norm over that row's terminal pytree
        steps = int(engine.jnp.max(engine.jnp.asarray(solutions.stats["num_steps"])))
        return SolverReceipt.Iterative(worst, steps, policy.rtol, engine.verdict(solutions.result, batched=True))
    solution = run(engine.lift(y0))  # per-leaf lift, total over a structured (x, v) Langevin / multi-leaf y0; a bare jnp.asarray(y0) flattens the pytree
    return SolverReceipt.Iterative(float(residual(engine.terminal(solution))), int(solution.stats["num_steps"]), policy.rtol, engine.verdict(solution.result, batched=False))


# The three equation cases vary only in the (terms, residual) pair; the SDE arm folds the BrownianPath
# and _LANGEVIN data cells into its term-pair while the controller, adjoint, save spec, and event are
# shared engine reads on the one diffeqsolve call site. Each residual is engine.tree_norm, total over
# the terminal pytree (the ODE steady-state field, the SDE/CDE/Langevin terminal state).
def _terms(
    e: "SolveEngine", intent: DifferentialIntent, solver: OdeSolver | SdeSolver, y0: Pytree, t0: float, t1: float, policy: IntegratePolicy
) -> tuple[object, Callable[[object], float]]:
    dfx, eqx = e.dfx, e.eqx
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, _, _, _, _)):
            compiled = eqx.filter_jit(lambda t, y, _: field(t, y))
            return dfx.ODETerm(compiled), lambda yt: e.tree_norm(compiled(t1, yt, None))
        case DifferentialIntent(tag="sde", sde=(drift, diffusion, _, _, sde_solver, _)):
            # sde_solver narrows the union to SdeSolver, so _LEVY.get types cleanly with no ignore.
            # A strong-order solver demands its Levy-area level on the path; the order-1 family keeps the
            # BrownianIncrement default. Noise width is the policy's noise_dim, falling to y0's last dim
            # only for diagonal noise where the noise and state dimensions coincide. The path GENERATOR
            # is the orthogonal BrownianPath axis: VirtualBrownianTree is reproducible and reverse-mode
            # safe, UnsafeBrownianPath is the faster forward-only path (no (t0, t1, tol) args). A
            # reverse-continuous adjoint reconstructs the path at backward time-points, which the
            # forward-only UnsafeBrownianPath cannot supply, so engine.reproducible_path floors UNSAFE to
            # VIRTUAL under AdjointMode.BACKSOLVE — the (UNSAFE, BACKSOLVE) pairing is unsatisfiable by
            # construction, never a solve-time backend fault, the same flooring _forced_pid runs.
            levy = _LEVY.get(sde_solver)
            width = policy.noise_dim if policy.noise_dim is not None else e.last_dim(y0)
            levy_kw = {"levy_area": _LEVY_CLASS(dfx)[levy]} if levy is not None else {}
            key = e.jr.split(e.jr.key(policy.seed))[0]
            brownian = (
                dfx.VirtualBrownianTree(t0, t1, tol=policy.atol, shape=(width,), key=key, **levy_kw)
                if e.reproducible_path(policy)
                else dfx.UnsafeBrownianPath(shape=(width,), key=key, **levy_kw)
            )
            # The underdamped-Langevin family integrates a (x, v) state over the Langevin drift/diffusion
            # term-pair keyed by gamma/u; the plain family builds MultiTerm(ODETerm, ControlTerm). One
            # _LANGEVIN-membership cell, never a per-solver arm, and the residual is the terminal norm either way.
            terms = (
                e.langevin_terms(policy, drift, brownian)
                if sde_solver in _LANGEVIN
                else dfx.MultiTerm(
                    dfx.ODETerm(eqx.filter_jit(lambda t, y, _: drift(t, y))),
                    dfx.ControlTerm(eqx.filter_jit(lambda t, y, _: diffusion(t, y)), brownian),
                )
            )
            return terms, e.tree_norm  # SDE/CDE residual is the terminal-state norm; the ODE arm composes f(t1, .) first
        case DifferentialIntent(tag="cde", cde=(field, (ts, ys), _, _, _, _)):
            control = dfx.CubicInterpolation(ts, dfx.backward_hermite_coefficients(ts, ys))
            return dfx.ControlTerm(eqx.filter_jit(lambda t, y, _: field(t, y)), control), e.tree_norm
        case _ as unreachable:
            assert_never(unreachable)


def _euler_floor(field: FieldFn, y0: np.ndarray, t0: float, t1: float, policy: IntegratePolicy) -> SolverReceipt:
    grid = np.linspace(t0, t1, policy.init_steps + 1)
    terminal = reduce(lambda y, lo: y + (grid[1] - grid[0]) * np.asarray(field(float(lo), y)), grid[:-1], y0)
    return SolverReceipt.Iterative(float(np.linalg.norm(np.asarray(field(t1, terminal)))), int(grid.size - 1), policy.rtol, None)
```

## [03]-[RESEARCH]

- [DIFFRAX_SOLVE]: `diffrax` and `equinox` resolve on the gated `python_version<'3.15'` band riding the jaxlib floor; the `diffeqsolve`/`ODETerm`/`ControlTerm`/`MultiTerm`/`UnderdampedLangevinDriftTerm`/`UnderdampedLangevinDiffusionTerm`/`Tsit5`/`Dopri5`/`Dopri8`/`KenCarp3`/`KenCarp4`/`KenCarp5`/`Kvaerno3`/`Kvaerno4`/`Kvaerno5`/`EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`LeapfrogMidpoint`/`SemiImplicitEuler`/`SRA1`/`ShARK`/`GeneralShARK`/`SlowRK`/`SEA`/`SPaRK`/`ALIGN`/`ShOULD`/`QUICSORT`/`SpaceTimeLevyArea`/`SpaceTimeTimeLevyArea`/`PIDController`/`ConstantStepSize`/`SaveAt`/`VirtualBrownianTree`/`UnsafeBrownianPath`/`CubicInterpolation`/`backward_hermite_coefficients`/`Event`/`steady_state_event`/`RecursiveCheckpointAdjoint`/`BacksolveAdjoint`/`ImplicitAdjoint`/`DirectAdjoint`/`ForwardMode`/`Solution.stats`/`Solution.result`/`RESULTS` spellings verify against `compute/.api/diffrax.md` under a uv-sync reflection pass on that band. The `diffeqsolve` signature `(terms, solver, t0, t1, dt0, y0, *, saveat, stepsize_controller, adjoint, event, max_steps=4096, throw=True, ...)` carries `throw` and `max_steps` as keyword arguments; the solve passes `throw=False` so a non-`successful` `Solution.result` returns for the status fold rather than raising into the `boundary` `catch=Exception` rail, and `max_steps=policy.max_steps` lifts the step budget. `PIDController(rtol, atol, *, dtmin, dtmax, ...)` accepts the optional step clamps the policy threads, and `UnsafeBrownianPath(shape, key, levy_area=)` is the forward-only Brownian path the `BrownianPath.UNSAFE` arm selects beside `VirtualBrownianTree(t0, t1, tol, shape, key, levy_area=)`. The catalogue documents `UnsafeBrownianPath` as forward-solve-only and `BacksolveAdjoint` as the continuous reverse-mode adjoint that reconstructs the path at backward time-points, so the `(UNSAFE, BACKSOLVE)` pairing is unsatisfiable — the forward-only path supplies no backward sample; `SolveEngine.reproducible_path` floors `UNSAFE` to `VirtualBrownianTree` under `AdjointMode.BACKSOLVE` so the configuration is total by construction rather than a solve-time backend fault, the same structural floor the `_LEVY` Levy/path coupling and the `_forced_pid` controller/adaptivity coupling run. `equinox.filter_jit`/`filter_vmap` and `jax.numpy.asarray`/`jax.random.key`/`jax.random.split`/`jax.tree_util.tree_map`/`tree_reduce`/`tree_leaves` verify against `compute/.api/equinox.md` and `compute/.api/jax.md`. JAX arrays default to 32-bit, so `SolveEngine.gated()` runs `jax.config.update("jax_enable_x64", True)` before the solve: a `1e-8` `rtol`/`atol` is below float32 eps (`~1.2e-7`) and a stiff ESDIRK/SDIRK or `BacksolveAdjoint` continuous-adjoint solve assumes double precision, so without the x64 flag the tolerance is unsatisfiable and a downcast `y0` loses the precision the receipt's residual floor adjudicates against — the same x64 contract every sibling JAX solver route (`solvers/nonlinear.md#NONLINEAR`, `solvers/sensitivity.md#SENSITIVITY`) carries on the gated band. The Diffrax adjoint solve feeds `solvers/sensitivity.md#SENSITIVITY`, whose per-axis trajectory Jacobian `experiments/study.md#STUDY` reads for its DGSM derivative screen; the `AdjointMode.FORWARD` `ForwardMode` mode is the forward-sensitivity adjoint for the batched-sweep many-outputs/few-parameters regime, the four reverse modes covering the few-outputs/many-parameters case. The `IntegratePolicy.batched` sweep maps the solve through `equinox.filter_vmap(..., in_axes=0)` and reduces the batched `Solution.result` through `SolveEngine.verdict` — `jnp.max` over the per-row `_value` codes (the zero `successful` making `max == 0` iff every start converged) plus the `RESULTS._name_to_item` name inversion; the diffrax `RESULTS` is the same `equinox.Enumeration` base whose `RESULTS.promote` is inheritance-widening (raising on a same-class member, NOT a vmap combine) and `RESULTS.where` is the branchless select, so the worst-case verdict is the code max the sibling `solvers/nonlinear.md#NONLINEAR` and `optimization/design.md#DESIGN` batched paths run, never `promote`.
- [JAX_PYTREE]: `y0` and every solve value are JAX pytrees carried as the typed `Pytree`/`FieldFn`/`ControlPath` vocabulary — the underdamped-Langevin state is the `(x, v)` position-velocity pair, the canonical multi-leaf case — so `SolveEngine.gated()` first floats the rail to float64 with `jax.config.update("jax_enable_x64", True)` (the `1e-8` `rtol`/`atol` is below float32 eps, and JAX downcasts a float64 `y0` to float32 without it), `engine.lift` raises the initial state per-leaf with `jax.tree_util.tree_map(jnp.asarray, y0)`, `engine.tree_norm` contracts terminal-state and steady-state residuals with the one per-leaf sum-of-squares fold (`tree_map` over the leaves then `tree_reduce` to the global L2 norm) returning the traced `jnp` scalar the caller coerces to `float` only at the eager receipt boundary — never inside the `filter_vmap` batched fence, where `float()` on a `Tracer` raises and the stacked per-row scalars fold through `jnp.max` first — and `engine.terminal` reads back the terminal state with `jax.tree_util.tree_map(lambda leaf: jnp.asarray(leaf)[-1], solution.ys)` — every read per-leaf and total over a structured pytree, where a bare `jnp.asarray(y0)` lift, a `jnp.asarray(solution.ys)[-1]` read, or a `jnp.linalg.norm(yt)` residual assumes a single array leaf and flattens the `(x, v)` state, and never `numpy.asarray`, which breaks a non-array leaf outright. Each field thunk (vector field, drift, diffusion) is wrapped in `equinox.filter_jit` so the static (non-array) closure leaves skip XLA tracing while array leaves compile, exactly the discipline `solvers/nonlinear.md#NONLINEAR` runs for the Optimistix residual probe. When `IntegratePolicy.batched` is set the leading axis of `y0` is a sweep of initial states and the whole solve maps through `equinox.filter_vmap(filter_jit(run), in_axes=0)` as one compiled stacked solve, a second `filter_vmap` contracting the per-row terminal residual; the receipt folds the per-row residual scalar to its `jnp.max` worst component and reduces the per-row `Solution.result` through `SolveEngine.verdict` — `jnp.max` over the per-row `_value` codes plus the `RESULTS._name_to_item` name inversion, NEVER `RESULTS.promote` (inheritance-widening that raises on a same-class member, not a vmap combine) — to the single worst-case termination member, so the sweep carries its true aggregate verdict rather than a `result=None` residual-floor fiction, while the single-state path inverts the one `int(solution.result._value)` (the `EnumerationItem` carries no `.name`); the stacked trajectory Jacobian `solvers/sensitivity.md#SENSITIVITY` differentiates for the `experiments/study.md#STUDY` DGSM screen, and the `jax.random` Brownian seed threads as a `random.key`/`random.split` lineage rather than a reused key. One compiled solve over the whole sweep, never a Python loop over starts.
- [SOLVER_DISPATCH]: the `_diffrax_receipt` `_SOLVER` table folds the catalogued solver family — the explicit-RK `Tsit5`/`Dopri5`/`Dopri8` (orders 5/5/8), the ESDIRK `KenCarp3`/`KenCarp4`/`KenCarp5` and SDIRK `Kvaerno3`/`Kvaerno4`/`Kvaerno5` stiff solvers, the order-1 stochastic `EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`LeapfrogMidpoint`/`SemiImplicitEuler`, the strong-order-1.5 stochastic `SRA1`/`ShARK`/`GeneralShARK`/`SlowRK`/`SEA`/`SPaRK`, and the underdamped-Langevin `ALIGN`/`ShOULD`/`QUICSORT` — each reachable through one row keyed by an `OdeSolver`/`SdeSolver` member, so no advertised solver is a dead `match` arm and the integrate path stays total over the vocabulary. Three band-local SDE tables ride beside it. `_LEVY` is the Brownian-coupling axis the diffrax catalogue makes mandatory: `SRA1`/`ShARK`/`GeneralShARK`/`SEA`/`SPaRK`/`ALIGN`/`ShOULD`/`QUICSORT` demand a `SpaceTimeLevyArea` path and `SlowRK` a `SpaceTimeTimeLevyArea` path, so the SDE arm reads the solver's required `LevyLevel` from `_LEVY` and folds the `_LEVY_CLASS`-resolved Levy class into the selected path's `levy_area=` — the order-1 family carries no `_LEVY` row and keeps the `BrownianIncrement` default — keeping the high-order solver and its path consistent by construction rather than by a runtime check. `_LANGEVIN` is the term-shape axis: its members route through the one `_langevin_terms` builder minting the `MultiTerm(UnderdampedLangevinDriftTerm, UnderdampedLangevinDiffusionTerm)` pair keyed by `policy.gamma`/`policy.langevin_u` over a `(x, v)` state, the plain family the `MultiTerm(ODETerm, ControlTerm)` pair, so the Langevin family is one membership cell rather than a fourth equation case. The catalogue confirms the `UnderdampedLangevin*Term` types and their Langevin-solver role and the Langevin solvers' Levy-area path requirement; the exact term-constructor argument binding resolves at the gated reflection pass against `compute/.api/diffrax.md`, isolated inside `_langevin_terms` rather than spread across the SDE arm. `BrownianPath` is the path-generator axis: `VirtualBrownianTree` (reproducible, reverse-mode-safe) or `UnsafeBrownianPath` (faster forward-only) by one ternary, the Levy fold shared across both. `_LEVY` membership doubles as the SDE adaptivity witness: an order-1 solver carries no error estimate, so its absence from `_LEVY` floors `SolveEngine.controller` to `ConstantStepSize` while the order-1.5/Langevin members admit the adaptive `PIDController`. The `dfx`-keyed tables (`_SOLVER`/`_LEVY_CLASS`/`_ADJOINT`/`_EVENT`) build from the carrier's `dfx` at solve time while `_LEVY`/`_LANGEVIN` are band-independent module constants, matching the `solvers/nonlinear.md#NONLINEAR` Optimistix dispatch discipline. The stiff ESDIRK/SDIRK solvers compose with the `PIDController` adaptive controller for the stiff-equation regime; the Ito/Stratonovich Milstein solvers carry their convention into the `ControlTerm` stochastic floor; the high-order space-time-Levy solvers deliver strong order 1.5 on that floor; the underdamped-Langevin family samples a kinetic `(x, v)` system through its drift/diffusion term-pair; `ReversibleHeun` preserves time-reversal structure for the `BacksolveAdjoint` continuous-adjoint gradient.
- [TERM_AND_POLICY_FOLD]: the three equation cases vary only in the `terms` argument and the residual contraction (the SDE arm folding the `BrownianPath` and `_LANGEVIN` data cells into its term-pair); the `t0`/`t1`/`dt0`/`y0`/`adjoint`/`stepsize_controller`/`saveat`/`event`/`max_steps`/`throw` arguments are shared, so one `diffeqsolve` call site runs every case. The `IntegratePolicy` struct carries the full integration policy as rows — the `StepKind`/`AdjointMode`/`SaveKind`/`EventKind`/`BrownianPath` axes folded into the diffrax controller, adjoint, save spec, event, and path generator through one table or ternary each, plus the `rtol`/`atol`/`dtmin`/`dtmax` scalars threaded into the `PIDController`, the `gamma`/`langevin_u` scalars threaded into the underdamped-Langevin term-pair, the `init_steps` count seeding `dt0` for both the gated solve and the numpy floor, the `noise_dim` width sizing the selected Brownian path, the `max_steps` budget threaded into `diffeqsolve`, and the `seed` threading the Brownian key — so the `adjoint` mode is a policy row rather than a free `solve` kwarg, the dense-output `SaveAt(t1=True, dense=True)` backing the `Solution.interpolation` a downstream consumer resamples through the differentiable solve is a `SaveKind` row, the path generator is a `BrownianPath` ternary, the Langevin physical parameters and the step clamps, initial step, noise width, and step budget are policy scalars rather than buried literals, and a new policy axis is one struct field plus one table row, ternary, or threaded scalar. `SolveEngine.controller` is selected by one `_forced_pid` gate reading `StepKind`, the event arming, and the solver's adaptivity — the order-1 SDE family (absent from `_LEVY`) carries no error estimate and floors to `ConstantStepSize`, so an adaptive controller never lands on a solver that cannot feed it. The CDE control path is a `(ts, ys)` sample pair prepared through `backward_hermite_coefficients` into a `CubicInterpolation` driving the `ControlTerm`, never a pre-built opaque control object.
- [EVENT_TERMINATION]: `EventKind` selects no event, the built-in `diffrax.steady_state_event()` for a transient-decay-to-equilibrium target, or a root-finding `diffrax.Event(condition, root_finder)` for a contact or threshold crossing; both fire through the one `event` argument, so a steady-state arrival and a contact crossing both terminate with the `RESULTS.event_occurred` member rather than two distinct codes, and the `EventKind` carries which event was armed. Events require adaptive stepping, so a non-`NONE` `EventKind` selects the `PIDController` only on an adaptive-capable solver; on a fixed-step-only order-1 `SdeSolver` the `SolveEngine._forced_pid` gate floors `controller` to `ConstantStepSize` and `event` returns `None`, so an adaptive controller never lands on a solver that supplies no error estimate. The `RESULTS` member name (`SolveEngine.verdict` inverting `RESULTS._name_to_item` off the `Solution.result._value` code, since the `EnumerationItem` carries no `.name`) maps through the receipt's `_STATUS` table — the catalogued diffrax `RESULTS` members are `successful`/`max_steps_reached`/`dt_min_reached`/`event_occurred`/`max_steps_rejected`/`internal_error`, so an `event_occurred`, a `dt_min_reached`, and a `max_steps_reached` solve reach `SolverReceipt` as distinct `SolveStatus` verdicts rather than a residual-only floor; an unmapped member degrades to `SolveStatus.OTHER`. The `solvers/receipt.md#RECEIPT` `_STATUS` table already carries every diffrax spelling this route emits — `event_occurred`→`SolveStatus.EVENT` (the convergent terminal-event class), `dt_min_reached`/`max_steps_rejected`→`MAX_STEPS`, `internal_error`→`BREAKDOWN` — and the `EVENT` member already rides the receipt's `_CONVERGENT` set, so this route adds no cross-owner row.
- [EULER_FLOOR]: the non-stiff ODE case carries a reachable fixed-step explicit-Euler march over `numpy` (`linspace` grid, immutable per-step accumulation) returning the steady-state residual `‖f(t1, y_T)‖` and the step count with `result=None`, so the `solvers/receipt.md#RECEIPT` residual-against-tolerance floor adjudicates the verdict exactly as the `solvers/quadrature.md#QUADRATURE` trapezoid and `solvers/nonlinear.md#NONLINEAR` central-difference floors do; a cp315 run without the jaxlib wheel never returns `Error(Import)` for the initial-value case. The floor is the single-state fallback: the `IntegratePolicy.batched` forward-sensitivity sweep is an inherently gated-JAX capability over `filter_vmap`, so a batched run on the floor reduces to the unbatched march over the stacked `numpy` array its `field` broadcasts, never a fabricated per-row Jacobian. The SDE/CDE/Langevin cases hold no numpy floor because a stochastic Milstein, rough-path, or underdamped-Langevin integrator is the gated capability itself, not a value a fixed-step march approximates.
