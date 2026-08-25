# [PY_COMPUTE_DIFFERENTIAL]

Differential-equations route of the one numeric solver. `DifferentialIntent` discriminates initial-value `Ode`, stochastic `Sde`, and controlled `Cde` integration over `diffrax.diffeqsolve` on the JAX/Equinox floor — adaptive step control, steady-state and root-finding events, dense interpolated output, adjoint-differentiable solves — folding the integration diagnostics into the one `SolverReceipt`. It is the trajectory-integration peer of the scalar-integral `solvers/quadrature#QUADRATURE` route: this owner integrates a vector field, that one a scalar integrand.

This solve is adjoint-differentiable, so a parametric study reads sensitivities through the integration; the batched-sweep path runs the initial-state sweep under `AdjointMode.FORWARD`, the regime `solvers/sensitivity#SENSITIVITY` differentiates through. Vector field, drift, diffusion, and control are JAX pytrees carried as one `FieldFn`/`Pytree`/`ControlPath` vocabulary, never weak `object`/`np.ndarray` slots. One frozen `SolveEngine` folds the gated `dfx`/`eqx`/`jnp`/`jtu`/`jr` handles behind one `gated()` import that runs `jax_enable_x64` — the discipline `solvers/nonlinear#NONLINEAR` and `solvers/sensitivity#SENSITIVITY` share, floating the `1e-8` tolerance and stiff/adjoint solves to float64. Resolved `SolverReceipt` (`solvers/receipt#RECEIPT`) rides the hub `evidence_run` weave's fenced contributor harvest, its status mapped through the receipt-owned shared `verdict` fold; the x64-gated family declares the `HOSTILE` trait (`x64` is process-global native state), isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing owner.

## [01]-[INDEX]

- [02]-[DIFFERENTIAL]: ODE/SDE/CDE integration over Diffrax — table-dispatched solver/Levy/term/path selection, adjoint-differentiable and batched-vmap solves, a pytree-total terminal residual, and a numpy explicit-Euler ODE floor on one `DifferentialIntent` owner.

## [02]-[DIFFERENTIAL]

- Owner: `DifferentialIntent` — `Ode`/`Sde`/`Cde` cases on the one solver over `diffrax.diffeqsolve`. Solver class, Levy-area level, term-shape, Brownian-path generator, and the step/adjoint/save/event policy are orthogonal table and policy selections; the single `match intent` in `_terms` binds the `(terms, residual)` pair and is the only equation-shaped branch, every other axis a data cell. Policy rows carry every remaining variation — steady-state target, contact crossing, step-clamped stiff march, order-1 fixed-step Milstein solve, Langevin sampler, and memory-checkpointed reverse-mode adjoint alike — never branches.
- Cases: four orthogonal tables own selection. `_SOLVER` maps each `OdeSolver`/`SdeSolver` member to its diffrax class — total, no hardcoded `Tsit5()`, no dead solver; `IMPLICIT_EULER` is the order-1 A-B-L-stable DIRK floor beneath the Kvaerno/KenCarp SDIRK ladder, adaptive through its embedded order-2 Heun estimate, its Chord root-find riding the same optimistix/lineax implicit-step seam. `step_ts`/`jump_ts`/`rejected_revisit` wrap the PID controller in `ClipStepSizeController` — forced exact step times, known-discontinuity stepping, and the SDE rejected-step revisit that keeps noncommutative-noise Levy distributions honest; the wrap admits adaptive controllers alone, so the triple stays inert on the fixed-step path and a dt RANGE bound stays `dtmin`/`dtmax` on the PID row, never the clip wrap. `_LEVY` keys each strong-order/Langevin solver to the Levy-area level its path must supply (`_LEVY_CLASS` resolves it into `levy_area=`) and doubles as the SDE adaptivity witness: an order-1 solver is absent from `_LEVY`, carries no error estimate, and so `_forced_pid` floors it to `ConstantStepSize` and arms no event — a mis-paired path and an adaptive controller on a fixed-step solver are both unrepresentable. `_LANGEVIN` membership selects the `UnderdampedLangevin*Term` pair over a `(x, v)` state keyed by `gamma`/`langevin_u`, where the plain SDE family builds `MultiTerm(ODETerm, ControlTerm)` — one cell, never a fourth case. `BrownianPath` selects the generator, floored to the reproducible `VirtualBrownianTree` under `AdjointMode.BACKSOLVE` — a backsolve adjoint reconstructs the path at backward time-points the forward-only `UnsafeBrownianPath` cannot supply, so `(UNSAFE, BACKSOLVE)` is unsatisfiable by construction, never a solve-time fault.
- Entry: `DifferentialIntent.solve(lane)` composes `lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self)` under `evidence_run`; isolation, band, and worker-death retry derive at the runtime `Kernel` crossing owner from the trait row. `_diffrax_receipt` runs one `diffeqsolve(..., throw=False)` — the load-bearing knob: the `True` default raises on any non-`successful` result, which the weave's own fence then converts to a `BoundaryFault` that dead-codes the status fold, so `throw=False` keeps a `max_steps_reached`/`event_occurred`/`dt_min_reached` solve a first-class verdict. When jaxlib is absent the ODE case falls to `_euler_floor` (fixed-step explicit-Euler over numpy on the policy's `init_steps` grid, `result=None`, residual-vs-tolerance adjudicates); the SDE/CDE/Langevin cases hold no floor because the gated integrator IS the capability.
- Auto: `AdjointMode` selects the differentiable solve — checkpoint/backsolve/implicit/direct reverse modes for the few-outputs/many-parameters regime, `ForwardMode` for the many-outputs/few-parameters and batched-sweep regime. `IntegratePolicy.batched` reads `y0`'s leading axis as an initial-state sweep mapped through `filter_vmap` under `AdjointMode.FORWARD` inside the compiled solve; the receipt folds the per-row residual to its `jnp.max` worst and the per-row result to the worst-case termination, never a Python loop. `SaveKind.DENSE` backs a non-`None` `Solution.interpolation` the sensitivity route resamples through the adjoint while the receipt carries only the terminal verdict.
- Receipt: `SolverReceipt.Iterative` folds the worst residual, step count, `rtol`, and the mapped `RESULTS` member name as adjudicated status. Every residual is the one `engine.tree_norm` per-leaf sum-of-squares total over a structured terminal pytree — the ODE steady-state field `‖f(t1, y_T)‖`, the SDE/CDE/Langevin terminal state, the Langevin `(x, v)` pair — where a bare `jnp.linalg.norm` assumes a single array leaf and breaks a multi-leaf state. `SolveEngine.verdict` recovers the member name off the `Solution.result._value` code (an `EnumerationItem` carries no `.name`) through the receipt-owned shared fold; the batched path reduces per-row `_value` codes by `jnp.max`, not `RESULTS.promote` (inheritance-widening, not a vmap combine).
- Packages: `diffrax` (the `diffeqsolve` driver, the solver/term/path/adjoint/event families), `equinox` (`filter_jit` field thunks and per-row solve; `filter_vmap` batched sweep), `jax` (`jax_enable_x64` floats the gated solve to float64 so the `1e-8` `rtol`/`atol` clear float32 eps; `tree_util` per-leaf lift/terminal/norm over a structured pytree; `random.split` for the Brownian seed lineage), `numpy` (the explicit-Euler floor), `expression` (`tagged_union` union, `Map` table rail), `msgspec` (`Struct` policy), `jaxtyping`+`beartype` (`jaxtyped(typechecker=beartype(conf=FAULT_CONF))` shape/dtype fence on `_diffrax_receipt` — a bare `object` state on this JAX-gated route is the rejected form). Seams: `solvers/receipt#RECEIPT` owns `SolverReceipt` and the `verdict` fold; hub `evidence_run` owns span/fence/harvest; runtime owns the `LanePolicy`/`Kernel`/`KernelTrait` offload crossing.
- Growth: a deterministic solver is one `OdeSolver`+`_SOLVER` row; a stochastic solver adds one `SdeSolver`+`_SOLVER` row, a strong-order-1.5/Langevin solver one `_LEVY` row admitting it to the adaptive controller, and a Langevin solver one `_LANGEVIN` member — never a fourth equation case. Each new equation class lands one `DifferentialIntent` case and one `_terms` arm; a new step controller/adjoint/save/event/path is one enum member with its row or ternary; a new integration scalar is one `IntegratePolicy` field; a new termination class is one `_STATUS` row on the receipt owner; a new gated module is one `SolveEngine` field and one `gated()` import line.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from functools import reduce
from typing import Final, Literal, Self, assert_never

import numpy as np
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from jaxtyping import Array, Float, PyTree, jaxtyped
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, evidence_run
from rasm.compute.solvers.receipt import Provider, SolverReceipt, graduate, verdict
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait


# --- [TYPES] ----------------------------------------------------------------------------

type Pytree = PyTree[Float[Array, "..."]]
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
    DIRECT = "direct"
    FORWARD = "forward"


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
    IMPLICIT_EULER = "implicit_euler"
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
    ALIGN = "align"
    SHOULD = "should"
    QUICSORT = "quicsort"


class BrownianPath(StrEnum):
    VIRTUAL = "virtual"
    UNSAFE = "unsafe"


type LevyLevel = Literal["space_time", "space_time_time"]


# --- [CONSTANTS] ------------------------------------------------------------------------

_LEVY: Map[SdeSolver, LevyLevel] = Map.of_seq([
    (SdeSolver.SRA1, "space_time"),
    (SdeSolver.SHARK, "space_time"),
    (SdeSolver.GENERAL_SHARK, "space_time"),
    (SdeSolver.SEA, "space_time"),
    (SdeSolver.SPARK, "space_time"),
    (SdeSolver.ALIGN, "space_time"),
    (SdeSolver.SHOULD, "space_time"),
    (SdeSolver.QUICSORT, "space_time"),
    (SdeSolver.SLOW_RK, "space_time_time"),
])

_LANGEVIN: frozenset[SdeSolver] = frozenset({SdeSolver.ALIGN, SdeSolver.SHOULD, SdeSolver.QUICSORT})

_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-6)])


# --- [MODELS] ---------------------------------------------------------------------------


class IntegratePolicy(Struct, frozen=True):
    step: StepKind = StepKind.PID
    adjoint: AdjointMode = AdjointMode.RECURSIVE_CHECKPOINT
    save: SaveKind = SaveKind.TERMINAL
    event: EventKind = EventKind.NONE
    rtol: float = 1e-8
    atol: float = 1e-8
    dtmin: float | None = None
    dtmax: float | None = None
    step_ts: tuple[float, ...] | None = None
    jump_ts: tuple[float, ...] | None = None
    rejected_revisit: int | None = None
    init_steps: int = 1000
    max_steps: int = 4096
    noise_dim: int | None = None
    brownian: BrownianPath = BrownianPath.VIRTUAL
    gamma: float = 1.0
    langevin_u: float = 1.0
    condition: Callable[..., Pytree] | None = None
    root_finder: object | None = None
    batched: bool = False
    seed: int = 0


@tagged_union(frozen=True)
class DifferentialIntent:
    tag: Literal["ode", "sde", "cde"] = tag()
    ode: tuple[FieldFn, Pytree, Span, OdeSolver, IntegratePolicy] = case()
    sde: tuple[FieldFn, FieldFn, Pytree, Span, SdeSolver, IntegratePolicy] = case()
    cde: tuple[FieldFn, ControlPath, Pytree, Span, OdeSolver, IntegratePolicy] = case()

    @staticmethod
    def Ode(
        vector_field: FieldFn, y0: Pytree, span: Span, solver: OdeSolver = OdeSolver.TSIT5, policy: IntegratePolicy = IntegratePolicy()
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

    async def solve(self, lane: LanePolicy, key: ContentKey, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[SolverReceipt]":
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            return await lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self, key)

        return await evidence_run(EvidenceScope.DIFFERENTIAL, f"solve.{self.tag}", dispatch, facts={"equation": self.tag}, composition=composition)

    def graduates(
        self, receipt: SolverReceipt, ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[GraduationReceipt]":
        return graduate(
            EvidenceScope.DIFFERENTIAL.value, f"solve.{self.tag}", receipt.content_key, receipt, ceiling or dict(_CEILING.items()),
            composition=composition,
        )


@dataclass(frozen=True, slots=True)
class SolveEngine:
    dfx: object
    eqx: object
    jnp: object
    jtu: object
    jr: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)

        import diffrax as dfx
        import equinox as eqx
        import jax.numpy as jnp
        import jax.random as jr
        import jax.tree_util as jtu

        return cls(dfx=dfx, eqx=eqx, jnp=jnp, jtu=jtu, jr=jr)

    def tree_norm(self, tree: object) -> object:
        squared = self.jtu.tree_map(lambda leaf: self.jnp.sum(self.jnp.asarray(leaf) ** 2), tree)
        return self.jtu.tree_reduce(lambda a, b: a + b, squared, 0.0) ** 0.5

    def verdict(self, result: object) -> str:
        return verdict(self.jnp, self.dfx.RESULTS, result)

    def lift(self, y0: Pytree) -> object:
        return self.jtu.tree_map(self.jnp.asarray, y0)

    def terminal(self, solution: object) -> object:
        return self.jtu.tree_map(lambda leaf: self.jnp.asarray(leaf)[-1], solution.ys)

    def last_dim(self, y0: Pytree) -> int:
        return int(self.jnp.asarray(self.jtu.tree_leaves(y0)[-1]).shape[-1])

    def adjoint(self, mode: AdjointMode) -> object:
        return _ADJOINT(self.dfx)[mode]()

    def save(self, policy: IntegratePolicy) -> object:
        return self.dfx.SaveAt(t1=True, dense=policy.save is SaveKind.DENSE)

    def reproducible_path(self, policy: IntegratePolicy) -> bool:
        return policy.brownian is BrownianPath.VIRTUAL or policy.adjoint is AdjointMode.BACKSOLVE

    def langevin_terms(self, policy: IntegratePolicy, drift: FieldFn, brownian: object) -> object:
        grad = self.eqx.filter_jit(lambda t, y, _: drift(t, y))
        return self.dfx.MultiTerm(
            self.dfx.UnderdampedLangevinDriftTerm(gamma=policy.gamma, u=policy.langevin_u, grad_f=grad),
            self.dfx.UnderdampedLangevinDiffusionTerm(gamma=policy.gamma, u=policy.langevin_u, bm=brownian),
        )

    def _forced_pid(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> bool:
        adaptive_capable = tag != "sde" or solver in _LEVY
        return adaptive_capable and (policy.event is not EventKind.NONE or policy.step is StepKind.PID)

    def controller(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> object:
        if self._forced_pid(tag, solver, policy):
            pid = self.dfx.PIDController(rtol=policy.rtol, atol=policy.atol, dtmin=policy.dtmin, dtmax=policy.dtmax)
            if policy.step_ts is None and policy.jump_ts is None and policy.rejected_revisit is None:
                return pid
            return self.dfx.ClipStepSizeController(
                pid, step_ts=policy.step_ts, jump_ts=policy.jump_ts, store_rejected_steps=policy.rejected_revisit
            )
        return self.dfx.ConstantStepSize()

    def event(self, tag: str, solver: OdeSolver | SdeSolver, policy: IntegratePolicy) -> object | None:
        return _EVENT(self.dfx, policy)[policy.event]() if self._forced_pid(tag, solver, policy) else None


# --- [TABLES] ---------------------------------------------------------------------------


def _SOLVER(dfx: object) -> Map[OdeSolver | SdeSolver, Callable[[], object]]:
    return Map.of_seq([
        (OdeSolver.TSIT5, dfx.Tsit5),
        (OdeSolver.DOPRI5, dfx.Dopri5),
        (OdeSolver.DOPRI8, dfx.Dopri8),
        (OdeSolver.IMPLICIT_EULER, dfx.ImplicitEuler),
        (OdeSolver.KENCARP3, dfx.KenCarp3),
        (OdeSolver.KENCARP4, dfx.KenCarp4),
        (OdeSolver.KENCARP5, dfx.KenCarp5),
        (OdeSolver.KVAERNO3, dfx.Kvaerno3),
        (OdeSolver.KVAERNO4, dfx.Kvaerno4),
        (OdeSolver.KVAERNO5, dfx.Kvaerno5),
        (SdeSolver.EULER_HEUN, dfx.EulerHeun),
        (SdeSolver.ITO_MILSTEIN, dfx.ItoMilstein),
        (SdeSolver.STRATONOVICH_MILSTEIN, dfx.StratonovichMilstein),
        (SdeSolver.REVERSIBLE_HEUN, dfx.ReversibleHeun),
        (SdeSolver.LEAPFROG_MIDPOINT, dfx.LeapfrogMidpoint),
        (SdeSolver.SEMI_IMPLICIT_EULER, dfx.SemiImplicitEuler),
        (SdeSolver.SRA1, dfx.SRA1),
        (SdeSolver.SHARK, dfx.ShARK),
        (SdeSolver.GENERAL_SHARK, dfx.GeneralShARK),
        (SdeSolver.SLOW_RK, dfx.SlowRK),
        (SdeSolver.SEA, dfx.SEA),
        (SdeSolver.SPARK, dfx.SPaRK),
        (SdeSolver.ALIGN, dfx.ALIGN),
        (SdeSolver.SHOULD, dfx.ShOULD),
        (SdeSolver.QUICSORT, dfx.QUICSORT),
    ])


def _LEVY_CLASS(dfx: object) -> Map[LevyLevel, object]:
    return Map.of_seq([("space_time", dfx.SpaceTimeLevyArea), ("space_time_time", dfx.SpaceTimeTimeLevyArea)])


def _ADJOINT(dfx: object) -> Map[AdjointMode, Callable[[], object]]:
    return Map.of_seq([
        (AdjointMode.RECURSIVE_CHECKPOINT, dfx.RecursiveCheckpointAdjoint),
        (AdjointMode.BACKSOLVE, dfx.BacksolveAdjoint),
        (AdjointMode.IMPLICIT, dfx.ImplicitAdjoint),
        (AdjointMode.DIRECT, dfx.DirectAdjoint),
        (AdjointMode.FORWARD, dfx.ForwardMode),
    ])


def _EVENT(dfx: object, policy: IntegratePolicy) -> Map[EventKind, Callable[[], object | None]]:
    return Map.of_seq([
        (EventKind.NONE, lambda: None),
        (EventKind.STEADY_STATE, lambda: dfx.Event(dfx.steady_state_event())),
        (EventKind.ROOT_FIND, lambda: dfx.Event(policy.condition, policy.root_finder)),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dispatch(intent: DifferentialIntent, key: ContentKey) -> SolverReceipt:
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, y0, (t0, t1), solver, policy)):
            try:
                return _diffrax_receipt(key, intent, solver, y0, t0, t1, policy)
            except ImportError:
                return _euler_floor(key, field, np.asarray(y0), t0, t1, policy)
        case (
            DifferentialIntent(tag="sde", sde=(_, _, y0, (t0, t1), solver, policy))
            | DifferentialIntent(tag="cde", cde=(_, _, y0, (t0, t1), solver, policy))
        ):
            return _diffrax_receipt(key, intent, solver, y0, t0, t1, policy)
        case _ as unreachable:
            assert_never(unreachable)


@jaxtyped(typechecker=beartype(conf=FAULT_CONF))
def _diffrax_receipt(
    key: ContentKey, intent: DifferentialIntent, solver: OdeSolver | SdeSolver, y0: Pytree, t0: float, t1: float, policy: IntegratePolicy
) -> SolverReceipt:
    engine = SolveEngine.gated()
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
            throw=False,
        )

    if policy.batched:
        solutions = engine.eqx.filter_vmap(engine.eqx.filter_jit(run), in_axes=0)(engine.lift(y0))
        per_row = engine.eqx.filter_vmap(lambda s: residual(engine.terminal(s)), in_axes=0)(solutions)
        worst = float(
            engine.jnp.max(engine.jnp.asarray(per_row))
        )
        steps = int(engine.jnp.max(engine.jnp.asarray(solutions.stats["num_steps"])))
        return SolverReceipt.Iterative(key, worst, steps, Provider.GATED, policy.rtol, engine.verdict(solutions.result))
    solution = run(
        engine.lift(y0)
    )
    return SolverReceipt.Iterative(
        key, float(residual(engine.terminal(solution))), int(solution.stats["num_steps"]), Provider.GATED, policy.rtol,
        engine.verdict(solution.result),
    )


def _terms(
    e: "SolveEngine", intent: DifferentialIntent, solver: OdeSolver | SdeSolver, y0: Pytree, t0: float, t1: float, policy: IntegratePolicy
) -> tuple[object, Callable[[object], float]]:
    dfx, eqx = e.dfx, e.eqx
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, _, _, _, _)):
            compiled = eqx.filter_jit(lambda t, y, _: field(t, y))
            return dfx.ODETerm(compiled), lambda yt: e.tree_norm(compiled(t1, yt, None))
        case DifferentialIntent(tag="sde", sde=(drift, diffusion, _, _, sde_solver, _)):
            levy = _LEVY.try_find(sde_solver).to_optional()
            width = policy.noise_dim if policy.noise_dim is not None else e.last_dim(y0)
            levy_kw = {"levy_area": _LEVY_CLASS(dfx)[levy]} if levy is not None else {}
            key = e.jr.split(e.jr.key(policy.seed))[0]
            brownian = (
                dfx.VirtualBrownianTree(t0, t1, tol=policy.atol, shape=(width,), key=key, **levy_kw)
                if e.reproducible_path(policy)
                else dfx.UnsafeBrownianPath(shape=(width,), key=key, **levy_kw)
            )
            terms = (
                e.langevin_terms(policy, drift, brownian)
                if sde_solver in _LANGEVIN
                else dfx.MultiTerm(
                    dfx.ODETerm(eqx.filter_jit(lambda t, y, _: drift(t, y))),
                    dfx.ControlTerm(eqx.filter_jit(lambda t, y, _: diffusion(t, y)), brownian),
                )
            )
            return terms, e.tree_norm
        case DifferentialIntent(tag="cde", cde=(field, (ts, ys), _, _, _, _)):
            control = dfx.CubicInterpolation(ts, dfx.backward_hermite_coefficients(ts, ys))
            return dfx.ControlTerm(eqx.filter_jit(lambda t, y, _: field(t, y)), control), e.tree_norm
        case _ as unreachable:
            assert_never(unreachable)


def _euler_floor(key: ContentKey, field: FieldFn, y0: np.ndarray, t0: float, t1: float, policy: IntegratePolicy) -> SolverReceipt:
    grid = np.linspace(t0, t1, policy.init_steps + 1)
    terminal = reduce(lambda y, lo: y + (grid[1] - grid[0]) * np.asarray(field(float(lo), y)), grid[:-1], y0)
    return SolverReceipt.Iterative(key, float(np.linalg.norm(np.asarray(field(t1, terminal)))), int(grid.size - 1), Provider.FLOOR, policy.rtol, None)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
