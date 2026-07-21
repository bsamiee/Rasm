# [PY_COMPUTE_DIFFERENTIAL]

Differential-equations route of the one numeric solver. `DifferentialIntent` discriminates initial-value `Ode`, stochastic `Sde`, and controlled `Cde` integration over `diffrax.diffeqsolve` on the JAX/Equinox floor — adaptive step control, steady-state and root-finding events, dense interpolated output, adjoint-differentiable solves — folding the integration diagnostics into the one `SolverReceipt`. It is the trajectory-integration peer of the scalar-integral `solvers/quadrature.md#QUADRATURE` route: this owner integrates a vector field, that one a scalar integrand.

This solve is adjoint-differentiable, so a parametric study reads sensitivities through the integration; the batched-sweep path runs the initial-state sweep under `AdjointMode.FORWARD`, the regime `solvers/sensitivity.md#SENSITIVITY` differentiates through. Vector field, drift, diffusion, and control are JAX pytrees carried as one `FieldFn`/`Pytree`/`ControlPath` vocabulary, never weak `object`/`np.ndarray` slots. One frozen `SolveEngine` folds the gated `dfx`/`eqx`/`jnp`/`jtu`/`jr` handles behind one `gated()` import that runs `jax_enable_x64` — the discipline `solvers/nonlinear.md#NONLINEAR` and `solvers/sensitivity.md#SENSITIVITY` share, floating the `1e-8` tolerance and stiff/adjoint solves to float64. Resolved `SolverReceipt` (`solvers/receipt.md#RECEIPT`) rides the hub `evidence_run` weave's fenced contributor harvest, its status mapped through the receipt-owned shared `verdict` fold; the x64-gated family declares the `HOSTILE` trait (`x64` is process-global native state), isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing owner.

## [01]-[INDEX]

- [01]-[DIFFERENTIAL]: ODE/SDE/CDE integration over Diffrax — table-dispatched solver/Levy/term/path selection, adjoint-differentiable and batched-vmap solves, a pytree-total terminal residual, and a numpy explicit-Euler ODE floor on one `DifferentialIntent` owner.

## [02]-[DIFFERENTIAL]

- Owner: `DifferentialIntent` — `Ode`/`Sde`/`Cde` cases on the one solver over `diffrax.diffeqsolve`. Solver class, Levy-area level, term-shape, Brownian-path generator, and the step/adjoint/save/event policy are orthogonal table and policy selections; the single `match intent` in `_terms` binds the `(terms, residual)` pair and is the only equation-shaped branch, every other axis a data cell. A steady-state target, a contact crossing, a step-clamped stiff march, an order-1 fixed-step Milstein solve, a Langevin sampler, and a memory-checkpointed reverse-mode adjoint are policy rows, never branches.
- Cases: four orthogonal tables own selection. `_SOLVER` maps each `OdeSolver`/`SdeSolver` member to its diffrax class — total, no hardcoded `Tsit5()`, no dead solver. `_LEVY` keys each strong-order/Langevin solver to the Levy-area level its path must supply (`_LEVY_CLASS` resolves it into `levy_area=`) and doubles as the SDE adaptivity witness: an order-1 solver is absent from `_LEVY`, carries no error estimate, and so `_forced_pid` floors it to `ConstantStepSize` and arms no event — a mis-paired path and an adaptive controller on a fixed-step solver are both unrepresentable. `_LANGEVIN` membership selects the `UnderdampedLangevin*Term` pair over a `(x, v)` state keyed by `gamma`/`langevin_u`, where the plain SDE family builds `MultiTerm(ODETerm, ControlTerm)` — one cell, never a fourth case. `BrownianPath` selects the generator, floored to the reproducible `VirtualBrownianTree` under `AdjointMode.BACKSOLVE` — a backsolve adjoint reconstructs the path at backward time-points the forward-only `UnsafeBrownianPath` cannot supply, so `(UNSAFE, BACKSOLVE)` is unsatisfiable by construction, never a solve-time fault.
- Entry: `DifferentialIntent.solve(lane)` composes `lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self)` under `evidence_run`; isolation, band, and worker-death retry derive at the runtime `Kernel` crossing owner from the trait row. `_diffrax_receipt` runs one `diffeqsolve(..., throw=False)` — the load-bearing knob: the `True` default raises on any non-`successful` result, which the `boundary` `catch=Exception` then converts to a `BoundaryFault` that dead-codes the status fold, so `throw=False` keeps a `max_steps_reached`/`event_occurred`/`dt_min_reached` solve a first-class verdict. When jaxlib is absent the ODE case falls to `_euler_floor` (fixed-step explicit-Euler over numpy on the policy's `init_steps` grid, `result=None`, residual-vs-tolerance adjudicates); the SDE/CDE/Langevin cases hold no floor because the gated integrator IS the capability.
- Auto: `AdjointMode` selects the differentiable solve — checkpoint/backsolve/implicit/direct reverse modes for the few-outputs/many-parameters regime, `ForwardMode` for the many-outputs/few-parameters and batched-sweep regime. `IntegratePolicy.batched` reads `y0`'s leading axis as an initial-state sweep mapped through `filter_vmap` under `AdjointMode.FORWARD` inside the compiled solve; the receipt folds the per-row residual to its `jnp.max` worst and the per-row result to the worst-case termination, never a Python loop. `SaveKind.DENSE` backs a non-`None` `Solution.interpolation` the sensitivity route resamples through the adjoint while the receipt carries only the terminal verdict.
- Receipt: `SolverReceipt.Iterative` folds the worst residual, step count, `rtol`, and the mapped `RESULTS` member name as adjudicated status. Every residual is the one `engine.tree_norm` per-leaf sum-of-squares total over a structured terminal pytree — the ODE steady-state field `‖f(t1, y_T)‖`, the SDE/CDE/Langevin terminal state, the Langevin `(x, v)` pair — where a bare `jnp.linalg.norm` assumes a single array leaf and breaks a multi-leaf state. `SolveEngine.verdict` recovers the member name off the `Solution.result._value` code (an `EnumerationItem` carries no `.name`) through the receipt-owned shared fold; the batched path reduces per-row `_value` codes by `jnp.max`, not `RESULTS.promote` (inheritance-widening, not a vmap combine).
- Packages: `diffrax` (the `diffeqsolve` driver, the solver/term/path/adjoint/event families), `equinox` (`filter_jit` field thunks and per-row solve; `filter_vmap` batched sweep), `jax` (`jax_enable_x64` floats the gated solve to float64 so the `1e-8` `rtol`/`atol` clear float32 eps; `tree_util` per-leaf lift/terminal/norm over a structured pytree; `random.split` for the Brownian seed lineage), `numpy` (the explicit-Euler floor), `expression` (`tagged_union` union, `Map` table rail), `msgspec` (`Struct` policy), `jaxtyping`+`beartype` (`jaxtyped(typechecker=beartype(conf=FAULT_CONF))` shape/dtype fence on `_diffrax_receipt` — a bare `object` state on this JAX-gated route is the rejected form). Seams: `solvers/receipt.md#RECEIPT` owns `SolverReceipt` and the `verdict` fold; hub `evidence_run` owns span/fence/harvest; runtime owns the `LanePolicy`/`Kernel`/`KernelTrait` offload crossing.
- Growth: a deterministic solver is one `OdeSolver`+`_SOLVER` row; a stochastic solver adds one `SdeSolver`+`_SOLVER` row, a strong-order-1.5/Langevin solver one `_LEVY` row admitting it to the adaptive controller, and a Langevin solver one `_LANGEVIN` member — never a fourth equation case. A new equation class is one `DifferentialIntent` case and one `_terms` arm; a new step controller/adjoint/save/event/path is one enum member with its row or ternary; a new integration scalar is one `IntegratePolicy` field; a new termination class is one `_STATUS` row on the receipt owner; a new gated module is one `SolveEngine` field and one `gated()` import line.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.receipt import SolverReceipt, verdict
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait


# --- [TYPES] -------------------------------------------------------------------------------

# Field thunks are (t, y) -> dy/drift/diffusion; the CDE control is a sampled (ts, ys) pair
# backward_hermite lowers. The numpy floor narrows to np.ndarray at its jaxlib-free boundary.
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
    DIRECT = "direct"  # reverse-mode autodiff through the unrolled solve
    FORWARD = "forward"  # forward-mode; the few-parameters / batched-sweep sensitivity adjoint


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


# Levy-area level a strong-order-1.5 solver demands; the order-1 family carries none and falls to the
# BrownianIncrement default. SpaceTimeTimeLevyArea is a superset, so SlowRK's level also satisfies the
# space-time solvers.
type LevyLevel = Literal["space_time", "space_time_time"]


# --- [CONSTANTS] ---------------------------------------------------------------------------

# Band-independent Brownian-coupling axis: keys each SdeSolver member onto the LevyLevel the gated
# _LEVY_CLASS later resolves. Order-1 solvers are absent, keeping the BrownianIncrement default.
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

# Term-shape axis: membership selects the UnderdampedLangevin*Term pair over a (x, v) state; the
# drift thunk supplies grad_f, the diffusion thunk is unused (fixed by gamma/u), and y0 IS the (x, v) pair.
_LANGEVIN: frozenset[SdeSolver] = frozenset({SdeSolver.ALIGN, SdeSolver.SHOULD, SdeSolver.QUICSORT})

# family DEFAULT graduation ceiling; a caller's tighter row overrides at `graduate` on solvers/receipt.
_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", 1e-6)])


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
    init_steps: int = 1000  # grid count seeding dt0=(t1-t0)/init_steps, never a buried literal
    max_steps: int = 4096  # diffeqsolve step budget; raised past the default for stiff long horizons
    noise_dim: int | None = None  # Brownian width; None couples noise to y0's last dim for diagonal noise
    brownian: BrownianPath = BrownianPath.VIRTUAL  # UNSAFE is forward-only, floored to VIRTUAL under BACKSOLVE
    gamma: float = 1.0  # underdamped-Langevin friction; read only by the Langevin arm
    langevin_u: float = 1.0  # underdamped-Langevin inverse-mass (1/m); read only by the Langevin arm
    condition: Callable[..., Pytree] | None = None  # diffrax Event cond_fn(t, y, args, **kw); ROOT_FIND only
    root_finder: object | None = None  # an optimistix AbstractRootFinder deferred behind the worker lane
    batched: bool = False  # y0's leading axis is a sweep filter_vmapped through one solve under FORWARD
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

    async def solve(self, lane: LanePolicy) -> "RuntimeRail[SolverReceipt]":
        # one HOSTILE-trait Kernel carries the solve; isolation, band, and worker-death retry derive at
        # runtime Kernel crossing owner. This weave owns span, fence, and the fenced contributor harvest.
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            return await lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE), self)

        return await evidence_run(EvidenceScope.DIFFERENTIAL, f"solve.{self.tag}", dispatch, facts={"equation": self.tag})


# Gated modules folded into one value object, behavior built ONCE per solve: every carrier method
# reads self.dfx/eqx/jnp/jtu/jr rather than re-importing or threading a loose handle quadruple.
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

        jax.config.update("jax_enable_x64", True)  # 1e-8 rtol/atol and stiff/adjoint solves assume float64
        return cls(dfx=dfx, eqx=eqx, jnp=jnp, jtu=jtu, jr=jr)

    # One residual norm, total over a structured y0 pytree where a bare jnp.linalg.norm assumes a
    # single array leaf: the per-leaf sum-of-squares fold reduces to the global L2 norm. Returns the
    # traced scalar — the caller coerces at the eager boundary, never inside filter_vmap where float()
    # on a Tracer raises.
    def tree_norm(self, tree: object) -> object:
        squared = self.jtu.tree_map(lambda leaf: self.jnp.sum(self.jnp.asarray(leaf) ** 2), tree)
        return self.jtu.tree_reduce(lambda a, b: a + b, squared, 0.0) ** 0.5

    def verdict(self, result: object) -> str:
        # receipt-owned shared enum-verdict fold over this carrier's handle and diffrax.RESULTS.
        return verdict(self.jnp, self.dfx.RESULTS, result)

    def lift(self, y0: Pytree) -> object:  # per-leaf; a bare jnp.asarray flattens a structured (x, v)/multi-leaf y0
        return self.jtu.tree_map(self.jnp.asarray, y0)

    def terminal(self, solution: object) -> object:  # last save on every leaf; total over the y0 pytree
        return self.jtu.tree_map(lambda leaf: self.jnp.asarray(leaf)[-1], solution.ys)

    def last_dim(self, y0: Pytree) -> int:  # diagonal-noise width: y0's last leaf's last dimension
        return int(self.jnp.asarray(self.jtu.tree_leaves(y0)[-1]).shape[-1])

    def adjoint(self, mode: AdjointMode) -> object:
        return _ADJOINT(self.dfx)[mode]()

    def save(self, policy: IntegratePolicy) -> object:
        return self.dfx.SaveAt(t1=True, dense=policy.save is SaveKind.DENSE)

    # A BACKSOLVE adjoint reconstructs the path at backward time-points UnsafeBrownianPath cannot supply,
    # so the path floors to VirtualBrownianTree under BACKSOLVE.
    def reproducible_path(self, policy: IntegratePolicy) -> bool:
        return policy.brownian is BrownianPath.VIRTUAL or policy.adjoint is AdjointMode.BACKSOLVE

    # Underdamped-Langevin (x, v) term-pair: gamma and u parameterise the drift/diffusion terms, the
    # drift thunk supplying the potential gradient.
    def langevin_terms(self, policy: IntegratePolicy, drift: FieldFn, brownian: object) -> object:
        grad = self.eqx.filter_jit(lambda t, y, _: drift(t, y))
        return self.dfx.MultiTerm(
            self.dfx.UnderdampedLangevinDriftTerm(gamma=policy.gamma, u=policy.langevin_u, grad_f=grad),
            self.dfx.UnderdampedLangevinDiffusionTerm(gamma=policy.gamma, u=policy.langevin_u, bm=brownian),
        )

    # adaptive_capable gates BOTH the event-forced and StepKind.PID paths: an order-1 SDE solver (absent
    # from _LEVY) carries no error estimate, floors to ConstantStepSize, and arms no event.
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


# diffrax resolves only on the worker lane, so these tables build from the carrier's `dfx` at solve time,
# not module import. Each is total over its vocabulary with no NotImplemented arm.
def _SOLVER(dfx: object) -> Map[OdeSolver | SdeSolver, Callable[[], object]]:
    return Map.of_seq([
        (OdeSolver.TSIT5, dfx.Tsit5),
        (OdeSolver.DOPRI5, dfx.Dopri5),
        (OdeSolver.DOPRI8, dfx.Dopri8),
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


# --- [OPERATIONS] --------------------------------------------------------------------------


# One measured kernel — module-level and import-resolvable, so it crosses the process lane as spec
# data plus operands; the weave's `@receipted(REDACTION)` harvest streams the receipt.
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


@jaxtyped(typechecker=beartype(conf=FAULT_CONF))
def _diffrax_receipt(
    intent: DifferentialIntent, solver: OdeSolver | SdeSolver, y0: Pytree, t0: float, t1: float, policy: IntegratePolicy
) -> SolverReceipt:
    # jaxtyping contract rails a rank/dtype breach on `y0` at the boundary, never a mid-solve XLA
    # shape error, through the one shared beartype fence.
    engine = SolveEngine.gated()  # imports the gated modules once behind the band
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

    if policy.batched:  # y0's leading axis is a sweep; one compiled filter_vmap solve, never a Python loop
        solutions = engine.eqx.filter_vmap(engine.eqx.filter_jit(run), in_axes=0)(engine.lift(y0))
        per_row = engine.eqx.filter_vmap(lambda s: residual(engine.terminal(s)), in_axes=0)(solutions)
        worst = float(
            engine.jnp.max(engine.jnp.asarray(per_row))
        )  # worst per-row residual, each itself a tree_norm over that row's terminal pytree
        steps = int(engine.jnp.max(engine.jnp.asarray(solutions.stats["num_steps"])))
        return SolverReceipt.Iterative(worst, steps, policy.rtol, engine.verdict(solutions.result))
    solution = run(
        engine.lift(y0)
    )  # per-leaf lift; a bare jnp.asarray(y0) flattens a structured (x, v)/multi-leaf pytree
    return SolverReceipt.Iterative(
        float(residual(engine.terminal(solution))), int(solution.stats["num_steps"]), policy.rtol, engine.verdict(solution.result)
    )


# Three equation cases vary only in the (terms, residual) pair; controller, adjoint, save, and event
# are shared engine reads on the one diffeqsolve call site. Each residual is engine.tree_norm.
def _terms(
    e: "SolveEngine", intent: DifferentialIntent, solver: OdeSolver | SdeSolver, y0: Pytree, t0: float, t1: float, policy: IntegratePolicy
) -> tuple[object, Callable[[object], float]]:
    dfx, eqx = e.dfx, e.eqx
    match intent:
        case DifferentialIntent(tag="ode", ode=(field, _, _, _, _)):
            compiled = eqx.filter_jit(lambda t, y, _: field(t, y))
            return dfx.ODETerm(compiled), lambda yt: e.tree_norm(compiled(t1, yt, None))
        case DifferentialIntent(tag="sde", sde=(drift, diffusion, _, _, sde_solver, _)):
            # sde_solver narrows the union to SdeSolver, so _LEVY.get types cleanly with no ignore. The
            # path generator floors UNSAFE to VIRTUAL under BACKSOLVE; UnsafeBrownianPath takes no (t0, t1, tol).
            levy = _LEVY.try_find(sde_solver).to_optional()
            width = policy.noise_dim if policy.noise_dim is not None else e.last_dim(y0)
            levy_kw = {"levy_area": _LEVY_CLASS(dfx)[levy]} if levy is not None else {}
            key = e.jr.split(e.jr.key(policy.seed))[0]
            brownian = (
                dfx.VirtualBrownianTree(t0, t1, tol=policy.atol, shape=(width,), key=key, **levy_kw)
                if e.reproducible_path(policy)
                else dfx.UnsafeBrownianPath(shape=(width,), key=key, **levy_kw)
            )
            # _LANGEVIN membership selects the (x, v) drift/diffusion pair; the plain family builds
            # MultiTerm(ODETerm, ControlTerm). One cell, never a per-solver arm.
            terms = (
                e.langevin_terms(policy, drift, brownian)
                if sde_solver in _LANGEVIN
                else dfx.MultiTerm(
                    dfx.ODETerm(eqx.filter_jit(lambda t, y, _: drift(t, y))),
                    dfx.ControlTerm(eqx.filter_jit(lambda t, y, _: diffusion(t, y)), brownian),
                )
            )
            return terms, e.tree_norm  # SDE/CDE residual is the terminal-state norm; the ODE arm composes f(t1, .)
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

[LANGEVIN]-[BLOCKED]: the exact `UnderdampedLangevinDriftTerm`/`UnderdampedLangevinDiffusionTerm` constructor kwargs (`gamma`/`u`/`grad_f`/`bm`); verify against `compute/.api/diffrax.md` at the gated reflection pass.

(none)
