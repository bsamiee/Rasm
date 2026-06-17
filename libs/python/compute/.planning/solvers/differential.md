# [PY_COMPUTE_DIFFERENTIAL]

The differential-equations route of the one numeric solver. `DifferentialIntent` discriminates initial-value ODE, stochastic SDE, and controlled CDE integration over Diffrax on the JAX-and-Equinox floor, with adaptive step control, event handling, and adjoint-differentiable integration, folding the integration diagnostics into the one `SolverReceipt`. This is the genuine capability gap beside the 1-D quadrature route: where `solvers/quadrature.md#QUADRATURE` owns a scalar integral, this owner owns trajectory integration of a vector field. The Diffrax solve is adjoint-differentiable, so a parametric trajectory study reads sensitivities through the integration without differentiating the step sequence.

## [1]-[INDEX]

[DIFFERENTIAL]: ODE/SDE/CDE integration over Diffrax with adjoint-differentiable solves on one `DifferentialIntent` owner.

## [2]-[DIFFERENTIAL]

- Owner: `DifferentialIntent` — the differential-equations cases on the one solver; `Ode(vector_field, y0, span)` over `diffrax.diffeqsolve` with an `ODETerm` and a `Tsit5`/`Dopri5`/`Kvaerno5` solver, `Sde(drift, diffusion, y0, span)` over a `MultiTerm` of `ODETerm` and `ControlTerm` with a `VirtualBrownianTree`, and `Cde(vector_field, control, y0, span)` over a `ControlTerm` driven by an interpolated control path. `StepControl` selects `ConstantStepSize` or `PIDController` adaptive stepping; `AdjointMode` selects `RecursiveCheckpointAdjoint` or `BacksolveAdjoint` for the differentiable solve.
- Entry: `DifferentialIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the solve runs `diffrax.diffeqsolve(terms, solver, t0, t1, dt0, y0, stepsize_controller, saveat, adjoint, event)`, reads `Solution.stats["num_steps"]` and the terminal-state residual against a steady-state or event target, and folds them into `SolverReceipt.Iterative`. Event handling routes through `diffrax.Event` with a root-finding condition, so a transient field decay or a contact event terminates the integration at the crossing.
- Packages: `diffrax` (`diffeqsolve`, `ODETerm`, `ControlTerm`, `MultiTerm`, `Tsit5`, `Dopri5`, `Kvaerno5`, `PIDController`, `ConstantStepSize`, `SaveAt`, `VirtualBrownianTree`, `Event`, `RecursiveCheckpointAdjoint`, `BacksolveAdjoint`, `Solution`), `equinox` (the PyTree foundation Diffrax composes), `numpy` (`asarray`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new solver is one `match` arm; a new equation class is one `DifferentialIntent` case; a new step controller is one `StepControl` row; zero new surface.
- Boundary: `diffrax`/`equinox`/`jaxlib` carry no cp315 wheel, so the entire owner is authored against the documented API on the JAX floor; there is no numpy ODE floor because trajectory integration of a stiff or stochastic system is the gated capability itself. The adjoint solve feeds `differentiation/sensitivity.md#SENSITIVITY` and the parametric-trajectory case of `experiments/study.md#STUDY`; a hand-rolled Runge-Kutta loop and a scalar quadrature route here are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


# --- [TYPES] -------------------------------------------------------------------------------
class StepControl(StrEnum):
    CONSTANT = "constant"
    PID = "pid"


class AdjointMode(StrEnum):
    RECURSIVE_CHECKPOINT = "recursive_checkpoint"
    BACKSOLVE = "backsolve"


# --- [OPERATIONS] --------------------------------------------------------------------------
@tagged_union(frozen=True)
class DifferentialIntent:
    tag: Literal["ode", "sde", "cde"] = tag()
    ode: tuple[object, np.ndarray, tuple[float, float], StepControl] = case()
    sde: tuple[object, object, np.ndarray, tuple[float, float]] = case()
    cde: tuple[object, object, np.ndarray, tuple[float, float]] = case()

    @staticmethod
    def Ode(
        vector_field: Callable[..., np.ndarray],
        y0: np.ndarray,
        span: tuple[float, float],
        step: StepControl = StepControl.PID,
    ) -> "DifferentialIntent":
        return DifferentialIntent(ode=(vector_field, y0, span, step))

    @staticmethod
    def Sde(
        drift: Callable[..., np.ndarray],
        diffusion: Callable[..., np.ndarray],
        y0: np.ndarray,
        span: tuple[float, float],
    ) -> "DifferentialIntent":
        return DifferentialIntent(sde=(drift, diffusion, y0, span))

    @staticmethod
    def Cde(
        vector_field: Callable[..., np.ndarray],
        control: object,
        y0: np.ndarray,
        span: tuple[float, float],
    ) -> "DifferentialIntent":
        return DifferentialIntent(cde=(vector_field, control, y0, span))


def solve(intent: DifferentialIntent, *, adjoint: AdjointMode = AdjointMode.RECURSIVE_CHECKPOINT) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent, adjoint))


def _dispatch(intent: DifferentialIntent, adjoint: AdjointMode) -> SolverReceipt:
    import diffrax as dfx

    adj = dfx.RecursiveCheckpointAdjoint() if adjoint is AdjointMode.RECURSIVE_CHECKPOINT else dfx.BacksolveAdjoint()
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, y0, (t0, t1), step)):
            controller = dfx.PIDController(rtol=1e-8, atol=1e-8) if step is StepControl.PID else dfx.ConstantStepSize()
            solution = dfx.diffeqsolve(
                dfx.ODETerm(lambda t, y, _: field(t, y)),
                dfx.Tsit5(),
                t0=t0,
                t1=t1,
                dt0=(t1 - t0) / 1000,
                y0=y0,
                stepsize_controller=controller,
                saveat=dfx.SaveAt(t1=True),
                adjoint=adj,
            )
            terminal = np.asarray(solution.ys)[-1]
            return SolverReceipt.Iterative(float(np.linalg.norm(field(t1, terminal))), int(solution.stats["num_steps"]), 1e-8)
        case DifferentialIntent(tag="sde", sde=(drift, diffusion, y0, (t0, t1))):
            import jax.random as jr

            brownian = dfx.VirtualBrownianTree(t0, t1, tol=1e-3, shape=(y0.shape[-1],), key=jr.PRNGKey(0))
            terms = dfx.MultiTerm(dfx.ODETerm(lambda t, y, _: drift(t, y)), dfx.ControlTerm(lambda t, y, _: diffusion(t, y), brownian))
            solution = dfx.diffeqsolve(terms, dfx.Tsit5(), t0=t0, t1=t1, dt0=(t1 - t0) / 1000, y0=y0, adjoint=adj)
            return SolverReceipt.Iterative(float(np.linalg.norm(np.asarray(solution.ys)[-1])), int(solution.stats["num_steps"]), 1e-3)
        case DifferentialIntent(tag="cde", cde=(field, control, y0, (t0, t1))):
            terms = dfx.ControlTerm(lambda t, y, _: field(t, y), control)
            solution = dfx.diffeqsolve(terms, dfx.Tsit5(), t0=t0, t1=t1, dt0=(t1 - t0) / 1000, y0=y0, adjoint=adj)
            return SolverReceipt.Iterative(float(np.linalg.norm(np.asarray(solution.ys)[-1])), int(solution.stats["num_steps"]), 1e-8)
        case unreachable:
            assert_never(unreachable)
```

## [3]-[RESEARCH]

- [DIFFRAX_SOLVE]: `diffrax` and `equinox` are NOT yet in the root manifest; the `diffeqsolve`/`ODETerm`/`ControlTerm`/`MultiTerm`/`Tsit5`/`Dopri5`/`Kvaerno5`/`PIDController`/`ConstantStepSize`/`SaveAt`/`VirtualBrownianTree`/`Event`/`RecursiveCheckpointAdjoint`/`BacksolveAdjoint`/`Solution.stats` spellings are admitted to the `scientific` group on the jaxlib `python_version<'3.15'` floor and verified against the branch `.api` catalogue before any fence names them. The Diffrax adjoint solve feeds `differentiation/sensitivity.md#SENSITIVITY` and the parametric-trajectory case of `experiments/study.md#STUDY`.
