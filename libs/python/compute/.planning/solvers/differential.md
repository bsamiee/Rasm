# [PY_COMPUTE_DIFFERENTIAL]

The differential-equations route of the one numeric solver. `DifferentialIntent` discriminates initial-value ODE, stochastic SDE, and controlled CDE integration over Diffrax on the JAX-and-Equinox floor, with adaptive step control, steady-state and root-finding events, dense interpolated output, and adjoint-differentiable integration, folding the integration diagnostics into the one `SolverReceipt`. This is the genuine capability gap beside the 1-D quadrature route: where `solvers/quadrature.md#QUADRATURE` owns a scalar integral, this owner owns trajectory integration of a vector field. The Diffrax solve is adjoint-differentiable, so a parametric trajectory study reads sensitivities through the integration without differentiating the step sequence. The vector field, drift, diffusion, and control are JAX pytrees: the gated body lifts `y0` and reads back the terminal state with `jax.numpy.asarray` — never `numpy.asarray`, which flattens a pytree and breaks a non-array leaf — and wraps each field thunk in `equinox.filter_jit` so the static closure leaves skip XLA tracing; a batched `y0` stack vectorises through `equinox.filter_vmap` over the leading axis without leaving the compiled solve, owned by the parametric-trajectory case of `experiments/study.md#STUDY` rather than implemented in this fence. The solver is never hardcoded: each case carries a member of the bounded `OdeSolver`/`SdeSolver` vocabulary and one frozen `_SOLVER` table folds the full catalogued diffrax solver family into the chosen integrator, so adding the eighth-order `Dopri8`, the `KenCarp3`/`KenCarp4`/`KenCarp5` ESDIRK stiff solvers, or the `ItoMilstein` stochastic solver is one row, never a new `match` arm or a new entrypoint. The non-stiff ODE route carries a reachable fixed-step explicit-Euler numpy floor so a cp315 run without the jaxlib wheel never returns `Error(Import)` for the initial-value case, exactly as every sibling solver route floors.

## [01]-[INDEX]

- [01]-[DIFFERENTIAL]: ODE/SDE/CDE integration over Diffrax with adjoint-differentiable solves, table-dispatched solver/term/policy selection, and a numpy explicit-Euler ODE floor on one `DifferentialIntent` owner.

## [02]-[DIFFERENTIAL]

- Owner: `DifferentialIntent` — the differential-equations cases on the one solver; `Ode(vector_field, y0, span)` over `diffrax.diffeqsolve` with an `ODETerm` and an `OdeSolver`-selected explicit-RK (`Tsit5`/`Dopri5`/`Dopri8`) or ESDIRK/SDIRK stiff (`KenCarp3`/`KenCarp4`/`KenCarp5`/`Kvaerno3`/`Kvaerno4`/`Kvaerno5`) integrator, `Sde(drift, diffusion, y0, span)` over a `MultiTerm` of `ODETerm` and `ControlTerm` with a `VirtualBrownianTree` and an `SdeSolver`-selected stochastic integrator (`EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`SemiImplicitEuler`), and `Cde(vector_field, control, y0, span)` over a `ControlTerm` driven by a `backward_hermite_coefficients`-prepared `CubicInterpolation` control path. The integration policy is one `IntegratePolicy` struct carried in every case rather than a free `solve` kwarg or a scatter of literals: `StepKind` selects `ConstantStepSize` or the adaptive `PIDController` (with the `rtol`/`atol`/`dtmin`/`dtmax` it parameterises), `AdjointMode` selects `RecursiveCheckpointAdjoint`/`BacksolveAdjoint`/`ImplicitAdjoint`/`DirectAdjoint` for the differentiable solve, `SaveKind` selects terminal-only or dense `SaveAt(dense=True)` output backed by `Solution.interpolation`, and `EventKind` selects no event, the built-in `steady_state_event()`, or a root-finding `Event(condition, root_finder)` terminal crossing. An `_ADJOINT` table and an `_EVENT` builder table fold each policy member into its diffrax object and the controller is the one `StepKind`-plus-event ternary that selects `PIDController`/`ConstantStepSize`, so a steady-state target, a contact crossing, and a memory-checkpointed reverse-mode adjoint are policy rows, never branches.
- Solver dispatch: `_SOLVER` is the ONE `FrozenDict` mapping every `OdeSolver`/`SdeSolver` member onto its diffrax solver class — `Tsit5`, `Dopri5`, `Dopri8`, `KenCarp3`, `KenCarp4`, `KenCarp5`, `Kvaerno3`, `Kvaerno4`, `Kvaerno5` on the deterministic side and `EulerHeun`, `ItoMilstein`, `StratonovichMilstein`, `ReversibleHeun`, `SemiImplicitEuler` on the stochastic side. Because the diffrax classes resolve only on the gated band the table is built inside the gated body — the same band-local `FrozenDict` discipline `solvers/nonlinear.md#NONLINEAR` runs for its Optimistix `_SOLVER`/`_ADJOINT` tables — so the dispatch is total over the vocabulary with no `NotImplemented` arm, no hardcoded `Tsit5()`, and no dead solver: each advertised solver is reachable through one row. A new diffrax solver class is one enum member plus one `_SOLVER` row; the integrate path never grows a `match` arm per solver.
- Term dispatch: the three equation cases vary only in how the `terms` argument is built and whether a Brownian path threads in; the `t0`/`t1`/`dt0`/`y0`/`adjoint`/`stepsize_controller`/`saveat`/`event` arguments are shared. `_terms(intent)` is the one builder that reads the case and returns the `(terms, solver)` pair — an `ODETerm` for the ODE case, a `MultiTerm(ODETerm, ControlTerm(VirtualBrownianTree))` for the SDE case keyed by a split `jax.random` key, and a `ControlTerm` over the hermite-prepared `CubicInterpolation` for the CDE case — so the single `diffeqsolve` call site runs every case and the per-case match arms collapse from three full solve bodies to three term rows.
- Entry: `DifferentialIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the gated body runs one `diffrax.diffeqsolve(terms, solver, t0, t1, dt0, y0, stepsize_controller, saveat, adjoint, event)`, reads the terminal state through `jax.numpy.asarray(solution.ys)[-1]`, contracts the steady-state residual `‖f(t1, y_T)‖` (ODE) or the terminal-state norm (SDE/CDE) on the JAX floor, reads `Solution.stats["num_steps"]`, maps the `Solution.result.name` `RESULTS` member name through the receipt's `_STATUS` table, and folds them into `SolverReceipt.Iterative` with the diffrax termination reason as the adjudicated status — so a `max_steps_reached`, an `event_occurred`, or a `dt_min_reached` solve is a distinct first-class verdict, never collapsed to a residual-only floor. Events require adaptive stepping, so a non-`NONE` `EventKind` forces the `PIDController` regardless of the policy's `StepKind`. When the jaxlib wheel is absent the ODE case falls to `_euler_floor`: a fixed-step explicit-Euler march over `numpy` returning the steady-state residual and step count with `result=None`, so the receipt's residual-against-tolerance floor adjudicates the verdict; the SDE/CDE cases hold no numpy floor because a stochastic or rough-path integrator is the gated capability itself.
- Packages: `diffrax` (`diffeqsolve`, `ODETerm`, `ControlTerm`, `MultiTerm`, `Tsit5`, `Dopri5`, `Dopri8`, `KenCarp3`, `KenCarp4`, `KenCarp5`, `Kvaerno3`, `Kvaerno4`, `Kvaerno5`, `EulerHeun`, `ItoMilstein`, `StratonovichMilstein`, `ReversibleHeun`, `SemiImplicitEuler`, `PIDController`, `ConstantStepSize`, `SaveAt`, `VirtualBrownianTree`, `CubicInterpolation`, `backward_hermite_coefficients`, `Event`, `steady_state_event`, `RecursiveCheckpointAdjoint`, `BacksolveAdjoint`, `ImplicitAdjoint`, `DirectAdjoint`, `Solution`, `RESULTS`), `equinox` (`filter_jit` — the field thunks compile through the JAX-native transform; the batched-`y0` `filter_vmap` is the parametric-trajectory case owned by `experiments/study.md#STUDY`, not this fence), `jax` (`numpy.asarray`, `numpy.linalg.norm`, `random.key`, `random.split` for the Brownian seed lineage), `numpy` (`asarray`, `linalg.norm`, `linspace` for the explicit-Euler floor), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new solver is one `OdeSolver`/`SdeSolver` member plus one `_SOLVER` row; a new equation class is one `DifferentialIntent` case plus one `_terms` row; a new step controller, adjoint mode, save mode, or event kind is one `StepKind`/`AdjointMode`/`SaveKind`/`EventKind` member plus one row in its fold table; a new termination class is one `_STATUS` row on the receipt owner; zero new surface, zero new entrypoint, zero free `solve` knob.
- Boundary: `diffrax`/`equinox`/`jaxlib` carry no cp315 wheel, so the gated solve body is authored against the documented API on the JAX floor; the ODE case carries a reachable numpy explicit-Euler floor matching every sibling route, while the SDE/CDE cases are gated-only because a stochastic or rough-path integrator is the gated capability itself. The adjoint solve feeds `solvers/sensitivity.md#SENSITIVITY` and the parametric-trajectory case of `experiments/study.md#STUDY`; a hand-rolled adaptive Runge-Kutta loop, a scalar quadrature route here, a hardcoded `Tsit5()`, a `numpy.asarray` over a JAX pytree, three parallel `diffeqsolve` bodies, a free `adjoint` kwarg on `solve`, a `NotImplemented` solver arm, and a per-solver `match` arm are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


class StepKind(StrEnum):
    CONSTANT = "constant"
    PID = "pid"


class AdjointMode(StrEnum):
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"
    BACKSOLVE = "backsolve"
    IMPLICIT = "implicit"
    DIRECT = "direct"


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
    SEMI_IMPLICIT_EULER = "semi_implicit_euler"


class IntegratePolicy(Struct, frozen=True):
    step: StepKind = StepKind.PID
    adjoint: AdjointMode = AdjointMode.RECURSIVE_CHECKPOINT
    save: SaveKind = SaveKind.TERMINAL
    event: EventKind = EventKind.NONE
    rtol: float = 1e-8
    atol: float = 1e-8
    condition: Callable[..., object] | None = None
    root_finder: object | None = None
    seed: int = 0


@tagged_union(frozen=True)
class DifferentialIntent:
    tag: Literal["ode", "sde", "cde"] = tag()
    ode: tuple[object, np.ndarray, tuple[float, float], OdeSolver, IntegratePolicy] = case()
    sde: tuple[object, object, np.ndarray, tuple[float, float], SdeSolver, IntegratePolicy] = case()
    cde: tuple[object, object, np.ndarray, tuple[float, float], OdeSolver, IntegratePolicy] = case()

    @staticmethod
    def Ode(
        vector_field: Callable[..., object],
        y0: np.ndarray,
        span: tuple[float, float],
        solver: OdeSolver = OdeSolver.TSIT5,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(ode=(vector_field, y0, span, solver, policy))

    @staticmethod
    def Sde(
        drift: Callable[..., object],
        diffusion: Callable[..., object],
        y0: np.ndarray,
        span: tuple[float, float],
        solver: SdeSolver = SdeSolver.EULER_HEUN,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(sde=(drift, diffusion, y0, span, solver, policy))

    @staticmethod
    def Cde(
        vector_field: Callable[..., object],
        control: tuple[np.ndarray, np.ndarray],
        y0: np.ndarray,
        span: tuple[float, float],
        solver: OdeSolver = OdeSolver.TSIT5,
        policy: IntegratePolicy = IntegratePolicy(),
    ) -> "DifferentialIntent":
        return DifferentialIntent(cde=(vector_field, control, y0, span, solver, policy))

    def solve(self) -> "RuntimeRail[SolverReceipt]":
        return boundary(f"solve.{self.tag}", lambda: _dispatch(self))


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
        case unreachable:
            assert_never(unreachable)


def _diffrax_receipt(
    intent: DifferentialIntent,
    solver: OdeSolver | SdeSolver,
    y0: np.ndarray,
    t0: float,
    t1: float,
    policy: IntegratePolicy,
) -> SolverReceipt:
    import diffrax as dfx
    import equinox as eqx
    import jax.numpy as jnp
    import jax.random as jr

    _SOLVER: FrozenDict[OdeSolver | SdeSolver, Callable[[], object]] = FrozenDict(
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
            SdeSolver.SEMI_IMPLICIT_EULER: dfx.SemiImplicitEuler,
        }
    )
    _ADJOINT: FrozenDict[AdjointMode, Callable[[], object]] = FrozenDict(
        {
            AdjointMode.RECURSIVE_CHECKPOINT: dfx.RecursiveCheckpointAdjoint,
            AdjointMode.BACKSOLVE: dfx.BacksolveAdjoint,
            AdjointMode.IMPLICIT: dfx.ImplicitAdjoint,
            AdjointMode.DIRECT: dfx.DirectAdjoint,
        }
    )
    _EVENT: FrozenDict[EventKind, Callable[[], object | None]] = FrozenDict(
        {
            EventKind.NONE: lambda: None,
            EventKind.STEADY_STATE: lambda: dfx.Event(dfx.steady_state_event()),
            EventKind.ROOT_FIND: lambda: dfx.Event(policy.condition, policy.root_finder),
        }
    )
    forced_pid = policy.step is StepKind.PID or policy.event is not EventKind.NONE
    controller = dfx.PIDController(rtol=policy.rtol, atol=policy.atol) if forced_pid else dfx.ConstantStepSize()
    event = _EVENT[policy.event]()
    save = dfx.SaveAt(t1=True) if policy.save is SaveKind.TERMINAL else dfx.SaveAt(t1=True, dense=True)

    match intent:
        case DifferentialIntent(tag="ode", ode=(field, _, _, _, _)):
            compiled = eqx.filter_jit(lambda t, y, _: field(t, y))
            terms, residual = dfx.ODETerm(compiled), lambda yt: float(jnp.linalg.norm(compiled(t1, yt, None)))
        case DifferentialIntent(tag="sde", sde=(drift, diffusion, _, _, _, _)):
            brownian = dfx.VirtualBrownianTree(t0, t1, tol=policy.atol, shape=(int(np.asarray(y0).shape[-1]),), key=jr.split(jr.key(policy.seed))[0])
            drift_c, diff_c = eqx.filter_jit(lambda t, y, _: drift(t, y)), eqx.filter_jit(lambda t, y, _: diffusion(t, y))
            terms, residual = dfx.MultiTerm(dfx.ODETerm(drift_c), dfx.ControlTerm(diff_c, brownian)), lambda yt: float(jnp.linalg.norm(yt))
        case DifferentialIntent(tag="cde", cde=(field, (ts, ys), _, _, _, _)):
            control = dfx.CubicInterpolation(ts, dfx.backward_hermite_coefficients(ts, ys))
            field_c = eqx.filter_jit(lambda t, y, _: field(t, y))
            terms, residual = dfx.ControlTerm(field_c, control), lambda yt: float(jnp.linalg.norm(yt))
        case unreachable:
            assert_never(unreachable)

    solution = dfx.diffeqsolve(
        terms,
        _SOLVER[solver](),
        t0=t0,
        t1=t1,
        dt0=(t1 - t0) / 1000,
        y0=jnp.asarray(y0),
        stepsize_controller=controller,
        saveat=save,
        adjoint=_ADJOINT[policy.adjoint](),
        event=event,
    )
    terminal = jnp.asarray(solution.ys)[-1]
    return SolverReceipt.Iterative(residual(terminal), int(solution.stats["num_steps"]), policy.rtol, solution.result.name)


def _euler_floor(field: Callable[..., object], y0: np.ndarray, t0: float, t1: float, policy: IntegratePolicy) -> SolverReceipt:
    grid = np.linspace(t0, t1, 1001)
    terminal = reduce(lambda y, lo: y + (grid[1] - grid[0]) * np.asarray(field(float(lo), y)), grid[:-1], y0)
    return SolverReceipt.Iterative(float(np.linalg.norm(np.asarray(field(t1, terminal)))), int(grid.size - 1), policy.rtol, None)
```

## [03]-[RESEARCH]

- [DIFFRAX_SOLVE]: `diffrax` and `equinox` resolve on the gated `python_version<'3.15'` band riding the jaxlib floor; the `diffeqsolve`/`ODETerm`/`ControlTerm`/`MultiTerm`/`Tsit5`/`Dopri5`/`Dopri8`/`KenCarp3`/`KenCarp4`/`KenCarp5`/`Kvaerno3`/`Kvaerno4`/`Kvaerno5`/`EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`SemiImplicitEuler`/`PIDController`/`ConstantStepSize`/`SaveAt`/`VirtualBrownianTree`/`CubicInterpolation`/`backward_hermite_coefficients`/`Event`/`steady_state_event`/`RecursiveCheckpointAdjoint`/`BacksolveAdjoint`/`ImplicitAdjoint`/`DirectAdjoint`/`Solution.stats`/`Solution.result`/`RESULTS` spellings verify against `compute/.api/diffrax.md` under a uv-sync reflection pass on that band. `equinox.filter_jit`/`filter_vmap` and `jax.numpy.asarray`/`jax.numpy.linalg.norm`/`jax.random.key`/`jax.random.split` verify against `compute/.api/equinox.md` and `compute/.api/jax.md`. The Diffrax adjoint solve feeds `solvers/sensitivity.md#SENSITIVITY` and the parametric-trajectory case of `experiments/study.md#STUDY`.
- [JAX_PYTREE]: `y0` and every solve value are JAX pytrees, so the gated body lifts the initial state with `jax.numpy.asarray`, contracts terminal-state and steady-state residuals with `jax.numpy.linalg.norm`, and reads back the terminal state with `jax.numpy.asarray(solution.ys)[-1]` — never `numpy.asarray`, which flattens a pytree and breaks a non-array leaf. Each field thunk (vector field, drift, diffusion) is wrapped in `equinox.filter_jit` so the static (non-array) closure leaves skip XLA tracing while array leaves compile, exactly the discipline `solvers/nonlinear.md#NONLINEAR` runs for the Optimistix residual probe. A batched `y0` stack maps through `equinox.filter_vmap` over the leading axis in the parametric-trajectory case of `experiments/study.md#STUDY`, returning one stacked solve whose per-row residual the receipt folds to its max component, and the `jax.random` Brownian seed threads as a `random.key`/`random.split` lineage rather than a reused key.
- [SOLVER_DISPATCH]: the `_diffrax_receipt` solver table folds the catalogued solver family — the explicit-RK `Tsit5`/`Dopri5`/`Dopri8` (orders 5/5/8), the ESDIRK `KenCarp3`/`KenCarp4`/`KenCarp5` and SDIRK `Kvaerno3`/`Kvaerno4`/`Kvaerno5` stiff solvers, and the stochastic `EulerHeun`/`ItoMilstein`/`StratonovichMilstein`/`ReversibleHeun`/`SemiImplicitEuler` — each reachable through one row keyed by an `OdeSolver`/`SdeSolver` member, so no advertised solver is a dead `match` arm and the integrate path stays total over the vocabulary. The table is the one band-local `FrozenDict` built inside the gated body, matching the `solvers/nonlinear.md#NONLINEAR` Optimistix dispatch discipline. The stiff ESDIRK/SDIRK solvers compose with the `PIDController` adaptive controller for the stiff-equation regime; the Ito/Stratonovich Milstein solvers carry their convention into the `ControlTerm`/`VirtualBrownianTree` stochastic floor; `ReversibleHeun` preserves time-reversal structure for the `BacksolveAdjoint` continuous-adjoint gradient.
- [TERM_AND_POLICY_FOLD]: the three equation cases vary only in the `terms` argument and the residual contraction; the `t0`/`t1`/`dt0`/`y0`/`adjoint`/`stepsize_controller`/`saveat`/`event` arguments are shared, so one `diffeqsolve` call site runs every case. The `IntegratePolicy` struct carries the full integration policy as rows — `StepKind`/`AdjointMode`/`SaveKind`/`EventKind` plus the `rtol`/`atol`/`seed` it parameterises — folded into the diffrax controller, adjoint, save spec, and event through one table each, so the `adjoint` mode is a policy row rather than a free `solve` kwarg, the dense-output `SaveAt(dense=True)` backing `Solution.interpolation` is a `SaveKind` row, and a new policy axis is one struct field plus one table row. The CDE control path is a `(ts, ys)` sample pair prepared through `backward_hermite_coefficients` into a `CubicInterpolation` driving the `ControlTerm`, never a pre-built opaque control object.
- [EVENT_TERMINATION]: `EventKind` selects no event, the built-in `diffrax.steady_state_event()` for a transient-decay-to-equilibrium target, or a root-finding `diffrax.Event(condition, root_finder)` for a contact or threshold crossing; events require adaptive stepping, so a non-`NONE` `EventKind` forces the `PIDController` regardless of the policy's `StepKind`. The `Solution.result.name` member name maps through the receipt's `_STATUS` table — `event_occurred`, `steady_state_reached`, `max_steps_reached`, and `dt_min_reached` reach `SolverReceipt` as distinct `SolveStatus` verdicts rather than a residual-only floor; an unmapped diffrax `RESULTS` member degrades to `SolveStatus.OTHER`. The receipt owner's `_STATUS` table gains the `diffrax`-specific member names as one row each when the diffrax `RESULTS` reflection confirms the exact member-name spellings.
- [EULER_FLOOR]: the non-stiff ODE case carries a reachable fixed-step explicit-Euler march over `numpy` (`linspace` grid, immutable per-step accumulation) returning the steady-state residual `‖f(t1, y_T)‖` and the step count with `result=None`, so the `solvers/receipt.md#RECEIPT` residual-against-tolerance floor adjudicates the verdict exactly as the `solvers/quadrature.md#QUADRATURE` trapezoid and `solvers/nonlinear.md#NONLINEAR` central-difference floors do; a cp315 run without the jaxlib wheel never returns `Error(Import)` for the initial-value case. The SDE/CDE cases hold no numpy floor because a stochastic Milstein or rough-path integrator is the gated capability itself, not a value a fixed-step march approximates.
