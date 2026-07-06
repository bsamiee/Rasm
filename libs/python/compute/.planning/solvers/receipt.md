# [PY_COMPUTE_RECEIPT]

The one method-discriminated solve receipt folded across every solver route, and the solver plane's shared verdict machinery: the `equinox.Enumeration` enum-verdict fold every gated route composes, the public `status_of` termination fold, and the ONE solver-axis graduation projection every solve owner feeds. `SolverReceipt` is a single `@tagged_union` whose `Literal` tag is the solve method (`direct`, `iterative`, `least_squares`, `eigen`), each case carrying a per-method tuple payload terminating in one `SolveStatus`, so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case rather than in flat shared fields. `SolveStatus` is the ONE termination `StrEnum` every backend folds into — the `lineax`/`optimistix`/`diffrax` `RESULTS` enums, the `scipy` `info`/`istop`/`success` codes, the `cvxpy` feasibility constants, and the residual-floor verdict — so a converged, event-terminated, max-steps, singular, or stagnated solve is a distinct first-class verdict carrying its own `converged` predicate, never one Boolean collapsing every non-success cause to `False`. The `_SLOTS` data row per method is the slot-name vocabulary; the `.status` read and the `.facts` projection are total `match self` folds over the four `@tagged_union` cases closed by `assert_never`, not a reflective `getattr(self, self.tag)` that escapes the exhaustive match, and not a parallel factory-plus-fact-dict. `SolverReceipt.contribute` narrows the runtime `ReceiptContributor` port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` `graduation/handoff.md#GRADUATION` `GraduationReceipt.contribute` returns, carrying the full per-method numeric evidence the C# graduation gate reads.

## [01]-[INDEX]

- [01]-[RECEIPT]: the unified method-discriminated solve receipt, the `_SLOTS` slot/output-projection table, the termination-status value-object vocabulary, the public `status_of` fold, the shared `verdict` enum fold, and the `graduate` solver-axis projection.

## [02]-[RECEIPT]

- Owner: `SolverReceipt` — the ONE `@tagged_union` solve receipt over every route; the `Literal` tag is the solve method, read directly through `.tag` (the tag IS the method literal, never a thin `.method` re-exposure). Each method case carries its own tuple payload terminating in one `SolveStatus`, so `direct` carries `(residual, condition, status)`, `iterative` carries `(residual, iterations, tol, status)`, `least_squares` carries `(residual, rank, iterations, tol, status)`, and `eigen` carries `(spectral_residual, k, condition, status)`. The status is the LAST slot of every case by construction, so `.status` is one total `match self` whose four-tag or-pattern binds the trailing `(*_, SolveStatus() as status)` once across every case and closes on `assert_never` — sound because the match is over `self` (the closed union), never the reflective `getattr(self, self.tag)` whose `object` residual makes `assert_never` a lie; the discriminant lives in the case, never in a flat shared struct and never in a parallel vocabulary enum beside the tag.
- Slot table: `_SLOTS` is the ONE `Map[SolveMethod, tuple[str, ...]]` naming each method's payload field sequence (`direct -> (residual, condition, status)`, `iterative -> (residual, iterations, tol, status)`, and so through `least_squares`/`eigen`), the trailing `status` slot common to every row. This single table is the slot-name vocabulary the `.facts` projection reads: `.facts` is a total `match self` whose per-tag arm zips the matched row against that case's destructured payload through `zip(_SLOTS[tag], payload, strict=True)` to mint the `dict[str, SolveSlot]` named-fact map, so the projection is exhaustive (a fifth case is a type error at the `assert_never` tail, not a silent reflective miss) AND the receipt projects its FULL per-method numeric evidence rather than a hand-spelled three-key dict that discarded residual/condition/iterations/rank. A new method's evidence is one `_SLOTS` row beside one factory plus one `match` arm; the table and the case tuples cannot drift because `strict=True` raises on a length mismatch.
- Status vocabulary: `SolveStatus` is the ONE bounded termination `StrEnum` — `SUCCESS`, `EVENT`, `MAX_STEPS`, `SINGULAR`, `BREAKDOWN`, `STAGNATION`, `DIVERGENCE`, `NONFINITE`, `ILL_CONDITIONED`, `INFEASIBLE`, `UNBOUNDED`, `OTHER` — and a value object with behavior: its `converged` property tests membership in the `_CONVERGENT` `frozenset` (`SUCCESS` plus the diffrax event-crossing `EVENT`), so the convergent class is one named anchor folded once rather than a tuple re-spelled at every consumer, and the receipt's `converged` delegates to `status.converged` so the Boolean contract survives while the receipt carries *why* a solve did not converge. A backend that adjudicates termination (`lineax`/`optimistix`/`diffrax` `Solution.result`, all `RESULTS` members) maps into `SolveStatus` through the one `_STATUS` boundary table keyed on the documented `RESULTS` member-name strings; the scipy `info`/`istop` codes fold through the `solvers/linear.md#LINEAR` `_info_status`/`_ISTOP` projections into the same vocabulary, `scipy.OptimizeResult.success` maps to `SUCCESS`/`STAGNATION`, and the numpy floor with no adjudicator derives its verdict from the residual against tolerance.
- Verdict fold: `verdict(gated, results, outcome)` is the ONE shared enum-verdict fold the gated routes compose — it inverts the `equinox.Enumeration` `RESULTS._name_to_item` table into a code-to-name map, reduces a batched result to its worst code through the caller's x64-gated `jax.numpy` module handle (`gated.max`), and renders the member-name string `status_of` maps into `SolveStatus`. The fold takes the gated module handle AND the `RESULTS` class as parameters, so this owner imports neither `jax` nor `equinox` at module top — every solver page, JAX-gated or not, imports receipt, and the x64-gated carrier stays the folder's sole JAX-importing owner. `LinearEngine`, `NonlinearEngine`, `SolveEngine`, and the design result-name read are one-row compositions of this fold, never four hand-spelled `_name_to_item` inversions.
- Status fold: `status_of(adjudicated, residual, tol)` is the ONE public polymorphic verdict every factory folds, one total `match` over the `str | None` discriminant rather than an `is not None` if-chain — the `case str() as name` arm reads `_STATUS.try_find(name).default_value(SolveStatus.OTHER)` so an unmapped backend member degrades to `OTHER` rather than crashing, and the two `case None` arms are the residual floor: the guarded `case None if not isfinite(residual)` returns `NONFINITE` over the stdlib `math.isfinite` scalar primitive and the bare `case None` returns `SUCCESS` when `residual <= tol` else `STAGNATION` for a finite-but-stalled residual. One verdict path holds: the backend status is authority where it exists, the residual floor the fallback where it does not, never two parallel convergence notions. The trailing `case unreachable: assert_never(unreachable)` is the typed totality witness closing the `str | None` discriminant. The fold is PUBLIC — `mesh`, `field`, and `design` compose it by name, so the cross-module contract is honest rather than a `_status` private masquerade. The four method tolerances are one frozen `_TOL` table keyed by tag (`direct`/`least_squares` at `1e-6`, `iterative` carrying its caller-supplied `tol`, `eigen` at `1e-8`), so a new method tolerance is one row, never a fifth inlined comparison expression.
- Graduation projection: `graduate(owner, subject, key, ledger, ceiling)` is the ONE solver-axis graduation projection — a `graduates`-composing fold beside the tables it already owns, importing the hub downward and returning `RuntimeRail[GraduationReceipt]`. Every solve owner FEEDS it by calling with the `(ledger, ceiling, key)` triple projected from its own receipt — `linear`/`nonlinear`/`differential`/`quadrature` from `SolverReceipt.facts`, `design`/`program` from their `OutcomeReceipt`, `interval` from its `Certificate` — so the projection is parameterized over the triple and this owner imports neither `OutcomeReceipt` nor `Certificate` nor any downstream type. The family's DEFAULT ceiling is a policy row on each family's own policy carrier beside its route table; the caller's tighter row overrides.
- Cross-route members: `EVENT` is the genuinely-new terminal class `solvers/differential.md#DIFFERENTIAL` adds when a `diffrax.Event` crossing or `steady_state_event` stops the integration — a successful termination distinct from `SUCCESS` recording *that an event fired*, which `converged` admits. `INFEASIBLE`/`UNBOUNDED` are the feasibility verdicts `optimization/convex.md#CONVEX` folds the cvxpy `infeasible`/`unbounded` constants into through its own `_CONVEX_STATUS` table; they live on this shared vocabulary so the convex receipt's `status` reads the same enum rather than a parallel convex-only status type, the iterative and least-squares routes simply never emitting them.
- Entry: the four `@classmethod` factories `Direct`, `Iterative`, `LeastSquares`, and `Eigen` returning `Self` are the canonical constructors every solver route folds into, each terminating its payload through `status_of` — a route that holds a backend `RESULTS` member passes its name (the gated routes derive it through `verdict`), a numpy-floor route passes `None` and lets the residual floor adjudicate. The `Self` return binds the subtype rather than a `"SolverReceipt"` forward-reference string re-spelled four times. `.status` binds the trailing status slot through one total `match self` or-pattern, `.converged` delegates to the status value object, and `.facts` projects the named per-method evidence by zipping the `_SLOTS` row against each matched case payload. One mapping folds the scipy, lineax, optimistix, diffrax, and scikit-fem termination reasons into the same vocabulary.
- Receipt: `SolverReceipt.contribute` implements the runtime `ReceiptContributor` port structurally (the `@runtime_checkable` `Protocol` `_stream` admits by `isinstance`, never a declared base on the decorator-rewritten `@tagged_union`), narrowing the port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` return rather than a bare `Receipt`, so a multi-phase contributor stays representable on the one port. It mints through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("compute.solver", ("emitted", self.tag, facts))`, the `(Phase, subject, facts)` triple the runtime factory discriminates — so the method tag rides as the receipt's `subject`, and the `facts` map carries the derived `converged` flag plus the spread of `.facts`, so the line carries the residual, condition number, iteration count, rank, tolerance, and eigen count the graduation gate reads as numeric evidence rather than a method/status pair discarding the numbers. The facts ride as native `float`/`int`/`bool` and the `SolveStatus` `StrEnum` (a `str` subclass `msgspec` encodes by value) through the runtime `Signals` `msgspec` `json.Encoder(enc_hook=repr, order="deterministic")` rather than a `str()` coerce. A solve graduating outward routes through the `graduate` projection on the `solver` `HandoffAxis` case, the residual ledger the `GraduationReceipt.graduates` ceiling fold clears being the per-case evidence this receipt projects. Emission rides the hub `evidence_run` weave the measured solver kernel composes, so receipt production stays a decorator rail rather than an inline `Signals.emit` threaded through each route body.
- Packages: `expression` (`tagged_union`/`case`/`tag`, `Map` the three dispatch tables — the folder's one table rail), stdlib `enum.StrEnum`/`math.isfinite`/`types.ModuleType`, runtime (`Receipt`, `ContentKey`, `RuntimeRail`), hub (`GraduationReceipt`/`HandoffAxis` — the downward graduation import).
- Growth: a new convergence shape is one `SolverReceipt` case keyed by one tag literal plus one `_TOL` row plus one `_SLOTS` row, the slot table projecting its evidence with no `contribute` edit; a new backend termination reason is one `_STATUS` row mapping its `RESULTS` member name into the existing `SolveStatus` vocabulary, or one new `SolveStatus` member when a genuinely new termination class appears (the `EVENT` member being exactly that path realized for the diffrax event crossing); a new graduating solve family is one `graduate` call with its own triple — the status fold, the verdict fold, the fact projection, and the graduation projection are reused, never re-inlined; zero new surface, no per-solver receipt struct, no flat shared-field receipt, no parallel method-vocabulary enum, no Boolean-per-case convergence flag discarding the backend's termination reason.
- Boundary: no benchmark authority and no substrate selection on the receipt — it carries the termination evidence the C# graduation gate reads, never a runtime decision, and never the admit/reject verdict the `convex_program`/`solver` `HandoffAxis` cases own (`SolveStatus` is the vocabulary the gate reads, not the gate). The deleted forms are a per-solver receipt struct, a flat receipt with a stringly-typed method field, a `SolveMethod` enum duplicating the tag, a thin `.method` accessor aliasing `.tag`, a single per-case factory collapse hiding the method payload, a hand-spelled `_SLOTS`-parallel per-case fact dict discarding the numeric evidence the slot projection carries, a reflective `getattr(self, self.tag)`-based `.status`/`.facts` whose `object` residual escapes the exhaustive match and makes the `assert_never` tail a lie, a non-total `status_of`/`.status`/`.facts` match lacking the `assert_never` tail, a per-page `RESULTS._name_to_item` inversion where the one `verdict` fold is composed, a `jax`/`equinox` import at this owner's module top where the gated handle and `RESULTS` class arrive as parameters, a private `_status` cross-imported by three siblings where the public `status_of` is the honest contract, a per-owner inline `HandoffAxis(solver=...)` admission where the one `graduate` projection owns the crossing, a redundant `"method"` fact key re-spelling the method tag the receipt already carries as its `subject`, a `str(self.converged)`/`str(residual)` coerce where the runtime encoder carries native scalars, a four-positional `Receipt.of("emitted", owner, subject, facts)` call against the two-argument `of(owner, evidence)` contract, a `contribute` returning one bare `Receipt` against the `Iterable[Receipt]` port, and a `@staticmethod`-plus-`"SolverReceipt"`-forward-ref factory where the `@classmethod`-plus-`Self` form binds the subtype once.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from math import isfinite
from types import ModuleType
from typing import Final, Literal, Self, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt

# --- [TYPES] -------------------------------------------------------------------------------


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]
type SolveSlot = float | int | SolveStatus


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

# `EVENT` counts as converged: a diffrax event crossing is a successful non-`SUCCESS` termination.
_CONVERGENT: frozenset[SolveStatus] = frozenset({SolveStatus.SUCCESS, SolveStatus.EVENT})

# The `iterative` row is the floor when a caller passes no `tol`; a live tolerance overrides it.
_TOL: Final[Map[SolveMethod, float]] = Map.of_seq([("direct", 1e-6), ("iterative", 1e-6), ("least_squares", 1e-6), ("eigen", 1e-8)])

# The `.facts` zip reads these rows against each case payload under `strict=True`, so a row/tuple
# length drift raises rather than silently truncating; `status` is the trailing slot of every row.
_SLOTS: Final[Map[SolveMethod, tuple[str, ...]]] = Map.of_seq([
    ("direct", ("residual", "condition", "status")),
    ("iterative", ("residual", "iterations", "tol", "status")),
    ("least_squares", ("residual", "rank", "iterations", "tol", "status")),
    ("eigen", ("spectral_residual", "k", "condition", "status")),
])

# Keys are the documented `lineax`/`optimistix`/`diffrax` `RESULTS` member-name strings the gated
# routes pass; an unmapped member degrades to `OTHER` at the fold, never a crash on a future member.
_STATUS: Final[Map[str, SolveStatus]] = Map.of_seq([
    ("successful", SolveStatus.SUCCESS),
    ("event_occurred", SolveStatus.EVENT),
    ("max_steps_reached", SolveStatus.MAX_STEPS),
    ("nonlinear_max_steps_reached", SolveStatus.MAX_STEPS),
    ("max_steps_rejected", SolveStatus.MAX_STEPS),
    ("dt_min_reached", SolveStatus.MAX_STEPS),
    ("singular", SolveStatus.SINGULAR),
    ("breakdown", SolveStatus.BREAKDOWN),
    ("internal_error", SolveStatus.BREAKDOWN),
    ("stagnation", SolveStatus.STAGNATION),
    ("nonlinear_divergence", SolveStatus.DIVERGENCE),
    ("nonfinite", SolveStatus.NONFINITE),
    ("nonfinite_input", SolveStatus.NONFINITE),
    ("conlim", SolveStatus.ILL_CONDITIONED),
])


# --- [OPERATIONS] ----------------------------------------------------------------------------


# `case None` is the no-adjudicator floor; the trailing arm makes `assert_never` a typed totality
# witness over `str | None`, never an open match a checker reads as a possible `None` return.
# PUBLIC: `mesh`, `field`, and `design` compose this fold by name — an honest cross-module contract.
def status_of(adjudicated: str | None, residual: float, tol: float) -> SolveStatus:
    match adjudicated:
        case str() as name:
            return _STATUS.try_find(name).default_value(SolveStatus.OTHER)
        case None if not isfinite(residual):
            return SolveStatus.NONFINITE
        case None:
            return SolveStatus.SUCCESS if residual <= tol else SolveStatus.STAGNATION
        case _ as unreachable:
            assert_never(unreachable)


def verdict(gated: ModuleType, results: type, outcome: object) -> str:
    # the ONE shared enum-verdict fold: invert the `equinox.Enumeration` `RESULTS._name_to_item`
    # table once, reduce a batched result to its worst code through the caller's x64-gated
    # `jax.numpy` handle, and render the member name `status_of` maps — this owner imports neither
    # `jax` nor `equinox`; the gated carrier stays the folder's sole JAX-importing owner.
    names: dict[int, str] = {int(item._value): name for name, item in results._name_to_item.items()}
    return names[int(gated.max(outcome._value))]


def graduate(owner: str, subject: str, key: ContentKey, ledger: dict[str, float], ceiling: dict[str, float]) -> RuntimeRail[GraduationReceipt]:
    # the ONE solver-axis graduation projection: every solve owner calls it with the triple
    # projected from its own receipt — parameterized, so this owner imports no downstream type.
    return GraduationReceipt.graduates(owner, HandoffAxis(solver=subject), key, ledger, ceiling)


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
        return cls(direct=(residual, condition, status_of(result, residual, _TOL["direct"])))

    @classmethod
    def Iterative(cls, residual: float, iterations: int, tol: float = _TOL["iterative"], result: str | None = None) -> Self:
        return cls(iterative=(residual, iterations, tol, status_of(result, residual, tol)))

    @classmethod
    def LeastSquares(cls, residual: float, rank: int, iterations: int, tol: float = _TOL["least_squares"], result: str | None = None) -> Self:
        return cls(least_squares=(residual, rank, iterations, tol, status_of(result, residual, tol)))

    @classmethod
    def Eigen(cls, spectral_residual: float, k: int, condition: float, result: str | None = None) -> Self:
        return cls(eigen=(spectral_residual, k, condition, status_of(result, spectral_residual, _TOL["eigen"])))

    @property
    def status(self) -> SolveStatus:
        match self:
            case (
                SolverReceipt(tag="direct", direct=(*_, SolveStatus() as status))
                | SolverReceipt(tag="iterative", iterative=(*_, SolveStatus() as status))
                | SolverReceipt(tag="least_squares", least_squares=(*_, SolveStatus() as status))
                | SolverReceipt(tag="eigen", eigen=(*_, SolveStatus() as status))
            ):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status.converged

    @property
    def facts(self) -> dict[str, SolveSlot]:
        match self:
            case (
                SolverReceipt(tag="direct", direct=payload)
                | SolverReceipt(tag="iterative", iterative=payload)
                | SolverReceipt(tag="least_squares", least_squares=payload)
                | SolverReceipt(tag="eigen", eigen=payload)
            ):
                return dict(zip(_SLOTS[self.tag], payload, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {"converged": self.converged, **self.facts}
        return (Receipt.of("compute.solver", ("emitted", self.tag, facts)),)
```
