# [PY_COMPUTE_RECEIPT]

The one method-discriminated solve receipt folded across every solver route. `SolverReceipt` is a single `@tagged_union` whose `Literal` tag is the solve method (`direct`, `iterative`, `least_squares`, `eigen`), each case carrying a per-method tuple payload terminating in one `SolveStatus`, so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case rather than in flat shared fields. `SolveStatus` is the ONE termination `StrEnum` every backend folds into — the `lineax`/`optimistix`/`diffrax` `RESULTS` enums, the `scipy` `info`/`istop`/`success` codes, the `cvxpy` feasibility constants, and the numpy-floor residual verdict — so a converged, event-terminated, max-steps, singular, or stagnated solve is a distinct first-class verdict carrying its own `converged` predicate, never one Boolean collapsing every non-success cause to `False`. The case shapes, the `.status` read, and the `contribute` fact projection are one `_SLOTS` data row per method, not a parallel factory-plus-fact-dict; `SolverReceipt.contribute` narrows the runtime `ReceiptContributor` port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` `graduation/handoff.md#GRADUATION` `GraduationReceipt.contribute` returns, carrying the full per-method numeric evidence the C# graduation gate reads.

## [01]-[INDEX]

- [01]-[RECEIPT]: the unified method-discriminated solve receipt, the `_SLOTS` slot/output-projection table, the termination-status value-object vocabulary, and the status fold.

## [02]-[RECEIPT]

- Owner: `SolverReceipt` — the ONE `@tagged_union` solve receipt over every route; the `Literal` tag is the solve method, read directly through `.tag` (the tag IS the method literal, never a thin `.method` re-exposure). Each method case carries its own tuple payload terminating in one `SolveStatus`, so `direct` carries `(residual, condition, status)`, `iterative` carries `(residual, iterations, tol, status)`, `least_squares` carries `(residual, rank, iterations, tol, status)`, and `eigen` carries `(spectral_residual, k, condition, status)`. The status is the LAST slot of every case by construction, so `.status` reads it through one structural `case (*_, status)` pattern closed by `assert_never` rather than four shape-specific arms; the discriminant lives in the case, never in a flat shared struct and never in a parallel vocabulary enum beside the tag.
- Slot table: `_SLOTS` is the ONE `FrozenDict[SolveMethod, tuple[str, ...]]` naming each method's payload field sequence (`direct -> (residual, condition, status)`, `iterative -> (residual, iterations, tol, status)`, and so through `least_squares`/`eigen`), the trailing `status` slot common to every row. This single table is the output-projection owner: `.facts` zips it against the case payload through `zip(..., strict=True)` to mint the named-fact dict, so the receipt projects its FULL per-method numeric evidence rather than a hand-spelled three-key dict that discarded residual/condition/iterations/rank. A new method's evidence is one `_SLOTS` row beside one factory, never a per-case fact construction; the table and the case tuples cannot drift because `strict=True` raises on a length mismatch.
- Status vocabulary: `SolveStatus` is the ONE bounded termination `StrEnum` — `SUCCESS`, `EVENT`, `MAX_STEPS`, `SINGULAR`, `BREAKDOWN`, `STAGNATION`, `DIVERGENCE`, `NONFINITE`, `ILL_CONDITIONED`, `INFEASIBLE`, `UNBOUNDED`, `OTHER` — and a value object with behavior: its `converged` property tests membership in the `_CONVERGENT` `frozenset` (`SUCCESS` plus the diffrax event-crossing `EVENT`), so the convergent class is one named anchor folded once rather than a tuple re-spelled at every consumer, and the receipt's `converged` delegates to `status.converged` so the Boolean contract survives while the receipt carries *why* a solve did not converge. A backend that adjudicates termination (`lineax`/`optimistix`/`diffrax` `Solution.result`, all `RESULTS` members) maps into `SolveStatus` through the one `_STATUS` boundary table keyed on the documented `RESULTS` member-name strings; the scipy `info`/`istop` codes fold through the `solvers/linear.md#LINEAR` `_info_status`/`_ISTOP` projections into the same vocabulary, `scipy.OptimizeResult.success` maps to `SUCCESS`/`STAGNATION`, and the numpy floor with no adjudicator derives its verdict from the residual against tolerance.
- Cross-route members: `EVENT` is the genuinely-new terminal class `solvers/differential.md#DIFFERENTIAL` adds when a `diffrax.Event` crossing or `steady_state_event` stops the integration — a successful termination distinct from `SUCCESS` recording *that an event fired*, which `converged` admits. `INFEASIBLE`/`UNBOUNDED` are the feasibility verdicts `optimization/convex.md#CONVEX` folds the cvxpy `infeasible`/`unbounded` constants into through its own `_CONVEX_STATUS` table; they live on this shared vocabulary so the convex receipt's `status` reads the same enum rather than a parallel convex-only status type, the iterative and least-squares routes simply never emitting them.
- Status fold: `_status(adjudicated, residual, tol)` is the ONE polymorphic verdict every factory folds, one total `match` over the `str | None` discriminant rather than an `is not None` if-chain — the `case str() as name` arm reads `_STATUS.get(name, SolveStatus.OTHER)` so an unmapped backend member degrades to `OTHER` rather than crashing, and the two `case None` arms are the numpy floor: the guarded `case None if not np.isfinite(residual)` returns `NONFINITE` and the bare `case None` returns `SUCCESS` when `residual <= tol` else `STAGNATION` for a finite-but-stalled residual. The trailing `case unreachable: assert_never(unreachable)` is the typed totality witness closing the `str | None` discriminant, so the fold carries the same `assert_never` closure the `.status` slot read does rather than an open `match` a type checker reads as a possible `None` return. The four method tolerances are one frozen `_TOL` table keyed by tag (`direct`/`least_squares` at `1e-6`, `iterative` carrying its caller-supplied `tol`, `eigen` at `1e-8`), so a new method tolerance is one row, never a fifth inlined `np.isfinite`/comparison expression, and the LM least-squares and the iterative floor read the same parameterized verdict.
- Entry: the four `@classmethod` factories `Direct`, `Iterative`, `LeastSquares`, and `Eigen` returning `Self` are the canonical constructors every solver route folds into, each terminating its payload through `_status` — a route that holds a backend `RESULTS` member passes its name, a numpy-floor route passes `None` and lets the residual floor adjudicate. The `Self` return binds the subtype rather than a `"SolverReceipt"` forward-reference string re-spelled four times. `.status` reads the trailing status slot through one structural pattern, `.converged` delegates to the status value object, and `.facts` projects the named per-method evidence off `_SLOTS`. One mapping folds the scipy, lineax, optimistix, diffrax, and scikit-fem termination reasons into the same vocabulary.
- Receipt: `SolverReceipt.contribute` implements the runtime `ReceiptContributor` port structurally (the `@runtime_checkable` `Protocol` `_stream` admits by `isinstance`, never a declared base on the decorator-rewritten `@tagged_union`), narrowing the port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` return rather than a bare `Receipt`, so a multi-phase contributor stays representable on the one port. It mints through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("compute.solver", ("emitted", self.tag, facts))`, the `(Phase, subject, facts)` triple the runtime factory discriminates, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes — carrying the method tag, the derived `converged` flag, and the spread of `.facts`, so the line carries the residual, condition number, iteration count, rank, tolerance, and eigen count the graduation gate reads as numeric evidence rather than a method/status pair discarding the numbers. The facts ride as native `float`/`int`/`bool` through the runtime `Signals` `msgspec` `json.Encoder(enc_hook=repr, order="deterministic")` rather than a `str()` coerce. A solve graduating outward routes through `graduation/handoff.md#GRADUATION` on the `solver` `HandoffAxis` case, the residual ledger the `GraduationReceipt.graduates` ceiling fold clears being the per-case `residual` this receipt projects. Emission rides the runtime `@receipted` aspect the measured solver kernel wears, so receipt production stays a decorator rail rather than an inline `Signals.emit` threaded through each route body.
- Packages: `expression` (`tag`, `case`, `tagged_union`), `numpy` (`isfinite` floors the no-adjudicator verdict), stdlib `typing` (`Self` binding the factory return, `assert_never` closing the `.status` match), runtime (`Receipt` the lone import, the `ReceiptContributor` port satisfied structurally and the `@receipted` aspect plus `Signals` whose `msgspec` `Encoder(enc_hook=repr, order="deterministic")` carries the receipt's native scalars — referenced as the egress contract, not imported here, matching the sibling `Receipt`-only import). The `lineax`/`optimistix`/`diffrax` `RESULTS` member names this owner maps are supplied by the gated solver routes at the boundary, never imported here, so the receipt owner carries no gated-band dependency and resolves clean on cp315.
- Growth: a new convergence shape is one `SolverReceipt` case keyed by one tag literal plus one `_TOL` row plus one `_SLOTS` row, the slot table projecting its evidence with no `contribute` edit; a new backend termination reason is one `_STATUS` row mapping its `RESULTS` member name into the existing `SolveStatus` vocabulary, or one new `SolveStatus` member when a genuinely new termination class appears (the `EVENT` member being exactly that path realized for the diffrax event crossing); the status fold and the fact projection are reused, never re-inlined; zero new surface, no per-solver receipt struct, no flat shared-field receipt, no parallel method-vocabulary enum, no Boolean-per-case convergence flag discarding the backend's termination reason.
- Boundary: no benchmark authority and no substrate selection on the receipt — it carries the termination evidence the C# graduation gate reads, never a runtime decision, and never the admit/reject verdict the `convex-program`/`solver` `HandoffAxis` cases own (`SolveStatus` is the vocabulary the gate reads, not the gate). The deleted forms are a per-solver receipt struct, a flat receipt with a stringly-typed method field, a `SolveMethod` enum duplicating the tag, a thin `.method` accessor aliasing `.tag`, a single per-case factory collapse hiding the method payload, a hand-spelled `_SLOTS`-parallel per-case fact dict discarding the numeric evidence the slot projection carries, a non-total `_status`/`.status` match lacking the `assert_never` tail, a `str(self.converged)`/`str(residual)` coerce where the runtime encoder carries native scalars, a four-positional `Receipt.of("emitted", owner, subject, facts)` call against the two-argument `of(owner, evidence)` contract, a `contribute` returning one bare `Receipt` against the `Iterable[Receipt]` port the siblings yield a one-element tuple for, and a `@staticmethod`-plus-`"SolverReceipt"`-forward-ref factory where the `@classmethod`-plus-`Self` form binds the subtype once.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from enum import StrEnum
from typing import Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.runtime.receipts import Receipt

# --- [TYPES] -------------------------------------------------------------------------------


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]


class SolveStatus(StrEnum):
    SUCCESS = "success"
    EVENT = "event"
    MAX_STEPS = "max_steps"
    SINGULAR = "singular"
    BREAKDOWN = "breakdown"
    STAGNATION = "stagnation"
    DIVERGENCE = "divergence"
    NONFINITE = "nonfinite"
    ILL_CONDITIONED = "ill_conditioned"
    INFEASIBLE = "infeasible"
    UNBOUNDED = "unbounded"
    OTHER = "other"

    @property
    def converged(self) -> bool:
        return self in _CONVERGENT


# --- [CONSTANTS] ---------------------------------------------------------------------------

# The two convergent terminal classes the `SolveStatus.converged` predicate folds: ordinary
# `SUCCESS` and the diffrax event-crossing `EVENT`. The membership lives once on the vocabulary,
# so every consumer reads the same value-object behavior rather than re-spelling the test.
_CONVERGENT: frozenset[SolveStatus] = frozenset({SolveStatus.SUCCESS, SolveStatus.EVENT})

# Per-method residual-floor tolerance, one row per `SolveMethod`; the `iterative` entry is the
# floor when a caller passes none, the live tolerance overriding it per solve.
_TOL: FrozenDict[SolveMethod, float] = FrozenDict(
    {"direct": 1e-6, "iterative": 1e-6, "least_squares": 1e-6, "eigen": 1e-8}
)

# Per-method payload field names, one tuple per tag, the trailing `status` slot common to every
# row. One owner over the case shapes: the factory packs by it, `.status` reads the last slot,
# and `contribute` projects each named slot to a fact, so a method's evidence is one row, never a
# per-case factory + a per-case fact dict.
_SLOTS: FrozenDict[SolveMethod, tuple[str, ...]] = FrozenDict(
    {
        "direct": ("residual", "condition", "status"),
        "iterative": ("residual", "iterations", "tol", "status"),
        "least_squares": ("residual", "rank", "iterations", "tol", "status"),
        "eigen": ("spectral_residual", "k", "condition", "status"),
    }
)

# Backend `RESULTS` member-name -> `SolveStatus`. Keys are the documented `lineax`/`optimistix`/
# `diffrax` enum member-name strings the gated routes pass; an unmapped member degrades to `OTHER`
# at the fold rather than crashing, so a future backend's new member reaches the receipt as a fact.
_STATUS: FrozenDict[str, SolveStatus] = FrozenDict(
    {
        "successful": SolveStatus.SUCCESS,
        "event_occurred": SolveStatus.EVENT,
        "max_steps_reached": SolveStatus.MAX_STEPS,
        "nonlinear_max_steps_reached": SolveStatus.MAX_STEPS,
        "max_steps_rejected": SolveStatus.MAX_STEPS,
        "dt_min_reached": SolveStatus.MAX_STEPS,
        "singular": SolveStatus.SINGULAR,
        "breakdown": SolveStatus.BREAKDOWN,
        "internal_error": SolveStatus.BREAKDOWN,
        "stagnation": SolveStatus.STAGNATION,
        "nonlinear_divergence": SolveStatus.DIVERGENCE,
        "nonfinite": SolveStatus.NONFINITE,
        "nonfinite_input": SolveStatus.NONFINITE,
        "conlim": SolveStatus.ILL_CONDITIONED,
    }
)


# the one verdict fold: a backend `RESULTS` member name maps through `_STATUS` (unmapped -> OTHER);
# the no-adjudicator numpy floor reads the residual against the per-method tolerance, the `case None`
# arm closing the `str | None` discriminant so the trailing `assert_never` is a typed totality witness.
def _status(adjudicated: str | None, residual: float, tol: float) -> SolveStatus:
    match adjudicated:
        case str() as name:
            return _STATUS.get(name, SolveStatus.OTHER)
        case None if not np.isfinite(residual):
            return SolveStatus.NONFINITE
        case None:
            return SolveStatus.SUCCESS if residual <= tol else SolveStatus.STAGNATION
        case unreachable:
            assert_never(unreachable)


# --- [MODELS] ------------------------------------------------------------------------------

@tagged_union(frozen=True)
class SolverReceipt:
    tag: SolveMethod = tag()
    direct: tuple[float, float, SolveStatus] = case()
    iterative: tuple[float, int, float, SolveStatus] = case()
    least_squares: tuple[float, int, int, float, SolveStatus] = case()
    eigen: tuple[float, int, float, SolveStatus] = case()

    @classmethod
    def Direct(cls, residual: float, condition: float, result: str | None = None) -> Self:
        return cls(direct=(residual, condition, _status(result, residual, _TOL["direct"])))

    @classmethod
    def Iterative(cls, residual: float, iterations: int, tol: float = _TOL["iterative"], result: str | None = None) -> Self:
        return cls(iterative=(residual, iterations, tol, _status(result, residual, tol)))

    @classmethod
    def LeastSquares(
        cls, residual: float, rank: int, iterations: int, tol: float = _TOL["least_squares"], result: str | None = None
    ) -> Self:
        return cls(least_squares=(residual, rank, iterations, tol, _status(result, residual, tol)))

    @classmethod
    def Eigen(cls, spectral_residual: float, k: int, condition: float, result: str | None = None) -> Self:
        return cls(eigen=(spectral_residual, k, condition, _status(result, spectral_residual, _TOL["eigen"])))

    @property
    def status(self) -> SolveStatus:
        match getattr(self, self.tag):
            case (*_, SolveStatus() as status):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status.converged

    @property
    def facts(self) -> dict[str, float | int | SolveStatus]:
        return dict(zip(_SLOTS[self.tag], getattr(self, self.tag), strict=True))

    def contribute(self) -> tuple[Receipt, ...]:
        facts: dict[str, object] = {"method": self.tag, "converged": self.converged, **self.facts}
        return (Receipt.of("compute.solver", ("emitted", self.tag, facts)),)
```

## [03]-[RESEARCH]

- [STATUS_VOCABULARY]: the `_STATUS` keys are exactly the documented `RESULTS` member names every gated backend carries — `lineax` (`successful`, `max_steps_reached`, `singular`, `breakdown`, `stagnation`, `conlim`, `nonfinite_input`), `optimistix` (that set plus `nonlinear_max_steps_reached`, `nonlinear_divergence`, `nonfinite`), `diffrax` (`successful`, `max_steps_reached`, `dt_min_reached`, `event_occurred`, `max_steps_rejected`, `internal_error`). The diffrax `event_occurred` maps to `EVENT`, `dt_min_reached`/`max_steps_rejected` to `MAX_STEPS`, `internal_error` to `BREAKDOWN`. The gated routes pass only the member-name string, never an imported `RESULTS` type, keeping this owner cp315-clean while the gated routes stay the only band-dependent code; an unmapped member degrades to `OTHER`, so a `RESULTS` member added by a future release reaches the receipt as a recorded fact rather than a crash.
- [VERDICT_PATH]: the numpy-floor routes (`solvers/linear.md#LINEAR` dense, `solvers/nonlinear.md#NONLINEAR` central-difference, `solvers/quadrature.md#QUADRATURE` trapezoid/interp, `solvers/differential.md#DIFFERENTIAL` explicit-Euler) hold no backend adjudicator, so they pass `result=None` and the residual-against-tolerance floor adjudicates `SUCCESS`/`STAGNATION`/`NONFINITE`; the gated routes pass the backend `Solution.result` member name and bypass the floor. One verdict path holds: the backend status is authority where it exists, the residual floor the fallback where it does not, never two parallel convergence notions.
