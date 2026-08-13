# [PY_COMPUTE_RECEIPT]

`SolverReceipt` is the one method-discriminated solve receipt folded across every solver route — a single `@tagged_union` whose `Literal` tag IS the solve method (`direct`, `iterative`, `least_squares`, `eigen`), each case carrying its own tuple payload — the numeric evidence, the optional jit-minted `EngineProfile` band, and one terminating `SolveStatus` — so the linear, nonlinear, quadrature, and differential routes emit one receipt and the discriminant lives in the case. `SolveStatus` is the one bounded termination vocabulary every backend folds into — the `lineax`/`optimistix`/`diffrax` `RESULTS` enums, the `scipy` `info`/`istop`/`success` codes, the `cvxpy` feasibility constants, the residual-floor verdict — so a converged, event-terminated, max-steps, singular, or stagnated solve is a distinct first-class verdict carrying its own `converged` predicate rather than one Boolean collapsing every non-success cause to `False`. Receipts carry the termination evidence the C# graduation gate reads and hold no benchmark authority, no substrate selection, and never the admit/reject verdict the `HandoffAxis` cases own.

Three exported folds stay stable across the solver plane: `status_of`, the one termination fold `mesh`, `field`, and `design` compose by name; `verdict`, the one `equinox.Enumeration` `RESULTS._name_to_item` inversion the gated routes compose, taking the caller's x64-gated `jax.numpy` handle and the `RESULTS` class as parameters so this owner imports neither `jax` nor `equinox`; and `graduate`, the one solver-axis graduation projection discriminating on its evidence shape — a fed `SolverReceipt` projects its own `ledger`, a prepared ledger passes through — so a crossing composes receipt, family ceiling row, key, and the caller's composition key in one call and the fold imports no downstream type. `scipy` `info`/`istop` codes fold in through the `solvers/linear#LINEAR` projections; `EVENT` is the terminal class `solvers/differential#DIFFERENTIAL` adds for a `diffrax.Event` crossing, and `INFEASIBLE`/`UNBOUNDED` are the feasibility verdicts `optimization/convex#CONVEX` folds the cvxpy constants into. Receipts graduate outward through `graduate` on the `solver` `HandoffAxis` case into the `graduation/handoff#GRADUATION` `GraduationReceipt`, and `contribute` is the `ReceiptContributor` the study spine harvests.

## [01]-[INDEX]

- [02]-[RECEIPT]: the one method-discriminated solve receipt and the `SolveStatus` termination vocabulary the solver plane folds into.

## [02]-[RECEIPT]

- Owner: `SolverReceipt` — the one `@tagged_union` over every route; `.tag` IS the method literal, never a thin `.method` re-exposure. `status` is the LAST payload slot of every case by construction, so `.status` is one total `match self` binding the trailing `(*_, SolveStatus() as status)` across the four cases and closing on `assert_never` — sound because the match is over `self`, the closed union, never a reflective `getattr(self, self.tag)` whose `object` residual makes the `assert_never` tail a lie. `_SLOTS` is the one `Map[SolveMethod, tuple[str, ...]]` slot-name vocabulary; `.facts` zips each case's row against its destructured payload under `strict=True` to mint the full per-method `dict[str, SolveSlot]`, never a hand-spelled dict discarding residual/condition/iterations/rank. `_LEDGER` is its graduation counterpart — the per-method DECLARED residual set the outward `ledger` narrows to — so evidence a ceiling cannot bar never reaches the admission fold as a pseudo-residual. Every case mounts the jit-minted `EngineProfile` as its optional `profile` slot before `status`, so a solve accelerated through a compiled kernel carries the engine's own measurements beside its numbers and a slow solve explains itself from the receipt, never from an external profiler attach.
- Cases: `SolveStatus` is the one bounded termination `StrEnum` and a value object — `converged` tests membership in the `_CONVERGENT` `frozenset` (`SUCCESS` and the diffrax `EVENT`), folded once rather than re-spelled at every consumer, and the receipt's `converged` delegates to it so the Boolean contract survives while the receipt carries *why* a solve did not converge. A backend that adjudicates termination maps in through the one `_STATUS` boundary table keyed on the documented `RESULTS` member-name strings; a numpy floor with no adjudicator derives its verdict from the residual against tolerance.
- Law: `condition` is `float | None` on the `direct` and `eigen` cases and defaults absent on their factories, because only a dense route holds the singular spectrum a condition number reads — a sparse factorization, a `SuperLU` back-substitution, an ARPACK stall, and a lineax operator solve each measure no conditioning at all. The unmeasured slot leaves the `ledger` rather than floating, so the hub's key-coverage gate refuses a ceiling naming a quantity the route never took; a `float("nan")` in the slot is the deleted form — it enters the ledger as a value, then breaches the hub's own finiteness refinement on every sparse crossing.
- Entry: the four `@classmethod` factories `Direct`/`Iterative`/`LeastSquares`/`Eigen` return `Self` — binding the subtype, not a forward-ref re-spelled four times — and terminate their payload through `status_of`, a route holding a backend `RESULTS` member passing its name (gated routes derive it through `verdict`), a numpy-floor route passing `None` to let the residual floor adjudicate. `status_of` is one total `match` over the `str | None` discriminant: `case str()` degrades an unmapped member to `OTHER` rather than crashing, the guarded `case None` returns `NONFINITE`, the bare `case None` returns `SUCCESS`/`STAGNATION` off the residual-vs-tolerance floor, and the trailing `assert_never` witnesses totality — backend status where it exists, the residual floor where it does not, never two parallel convergence notions. Method tolerances live in one frozen `_TOL` table keyed by tag.
- Receipt: `contribute` narrows the runtime `ReceiptContributor` port's `Iterable[Receipt]` to a concrete one-element tuple, so a multi-phase contributor stays representable on the port; the method tag rides as the receipt `subject`, and the `facts` map carries the derived `converged` flag and the full `.facts` spread — residual, condition, iterations, rank, tolerance, eigen count — as the numeric evidence the graduation gate reads, never a method/status pair discarding the numbers nor a `"method"` key re-spelling the subject; the profile band spreads `profile.`-namespaced beside the numeric slots and stays off the graduation `ledger`, so a profile extent can never masquerade as a residual a ceiling clears.
- Packages: `expression` (`tagged_union`/`case`/`tag`, and `Map` for every dispatch table), stdlib `enum.StrEnum`/`math.isfinite`/`types.ModuleType`, runtime (`Receipt`, `ContentKey`, `RuntimeRail`, and the `ScopeKey`/`DEFAULT_SCOPE` composition key `graduate` forwards), the downward hub graduation import (`GraduationReceipt`/`HandoffAxis`), and the `numerics/jit` `EngineProfile` band import.
- Growth: a new convergence shape is one `SolverReceipt` case, one `_TOL` row, one `_SLOTS` row, and one `_LEDGER` row, its evidence projecting with no `contribute` edit; a new graded quantity is one `_LEDGER` member, a new call-evidence slot one `_SLOTS` entry the ledger never sees; a new backend termination reason is one `_STATUS` row into the existing vocabulary, or one new `SolveStatus` member when a genuinely new termination class appears (`EVENT` being that path realized); a profiled solve is the same factory call carrying its `profile` row, and a new profile statistic lands on the jit `EngineProfile` with zero edits here; a new graduating solve family is one `graduate` call with its receipt, family ceiling row, and composition key — the status, verdict, fact, ledger, and graduation folds reused, never re-inlined.
- Boundary: `SolveStatus` is the vocabulary the C# graduation gate reads, not the gate itself; the admit/reject verdict belongs to the `convex_program`/`solver` `HandoffAxis` cases, and the graduation crossing to the one `graduate` projection rather than a per-owner inline `HandoffAxis(solver=...)`. Family DEFAULT ceilings are policy rows on each family's own carrier beside its route table; the caller's tighter row overrides. Composition custody is the caller's — `graduate` forwards the key it is handed and defaults `DEFAULT_SCOPE`, so the root call shape stays scope-free and the registry partition is the hub owner's mechanic, never re-derived here.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from math import isfinite
from types import ModuleType
from typing import Final, Literal, Self, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis
from rasm.compute.numerics.jit import EngineProfile
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

# --- [TYPES] -------------------------------------------------------------------------------


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]
# `None` is the UNMEASURED slot, not a zero: a sparse factorization exposes no condition number, so the slot
# spells absence and the ledger drops it rather than publishing a forged value a ceiling then clears.
type SolveSlot = float | int | EngineProfile | None | SolveStatus


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

# `EVENT` is converged: a diffrax event crossing is a successful termination.
_CONVERGENT: frozenset[SolveStatus] = frozenset({SolveStatus.SUCCESS, SolveStatus.EVENT})

# Floor when a caller passes no `tol`; a live tolerance overrides.
_TOL: Final[Map[SolveMethod, float]] = Map.of_seq([("direct", 1e-6), ("iterative", 1e-6), ("least_squares", 1e-6), ("eigen", 1e-8)])

# `strict=True` raises on a length drift, never truncates; `profile` is every row's penultimate slot and `status` its trailing slot.
_SLOTS: Final[Map[SolveMethod, tuple[str, ...]]] = Map.of_seq([
    ("direct", ("residual", "condition", "profile", "status")),
    ("iterative", ("residual", "iterations", "tol", "profile", "status")),
    ("least_squares", ("residual", "rank", "iterations", "tol", "profile", "status")),
    ("eigen", ("spectral_residual", "k", "condition", "profile", "status")),
])

# Per-method DECLARED residual set — the graduation-ledger domain, narrower than `_SLOTS` by construction: a
# tolerance, an iteration tally, a rank, and an eigen count are call evidence, never quantities a ceiling can
# bar, so projecting the whole slot row publishes them as pseudo-residuals a caller's tighter row then grades.
_LEDGER: Final[Map[SolveMethod, frozenset[str]]] = Map.of_seq([
    ("direct", frozenset({"residual", "condition"})),
    ("iterative", frozenset({"residual"})),
    ("least_squares", frozenset({"residual"})),
    ("eigen", frozenset({"spectral_residual", "condition"})),
])

# Documented `RESULTS` member-name keys; an unmapped member degrades to `OTHER`, never crashes.
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


# `case None` is the no-adjudicator floor; the trailing arm is the `assert_never` totality witness over `str | None`.
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
    # invert `_name_to_item`, reduce to the worst code through the gated `jax.numpy` handle, render the mapped name.
    names: dict[int, str] = {int(item._value): name for name, item in results._name_to_item.items()}
    return names[int(gated.max(outcome._value))]


def graduate(
    owner: str,
    subject: str,
    key: ContentKey,
    evidence: "SolverReceipt | dict[str, float]",
    ceiling: dict[str, float],
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[GraduationReceipt]:
    # evidence shape IS the modality: a receipt projects its own `ledger`, a prepared ledger passes through — no downstream import.
    # `composition` is the caller's custody key threaded straight onto the hub: the solver axis is the widest crossing in the
    # package, so an embedded second composition whose solve legs graduate through this ONE projection would otherwise fire every
    # admission and refusal into the root scope and register hook points that never receive a fact.
    ledger = evidence.ledger if isinstance(evidence, SolverReceipt) else evidence
    return GraduationReceipt.graduates(owner, HandoffAxis(solver=subject), key, ledger, ceiling, composition=composition)


# --- [MODELS] ------------------------------------------------------------------------------


@tagged_union(frozen=True)
class SolverReceipt:
    tag: SolveMethod = tag()
    direct: tuple[float, float | None, EngineProfile | None, SolveStatus] = case()
    iterative: tuple[float, int, float, EngineProfile | None, SolveStatus] = case()
    least_squares: tuple[float, int, int, float, EngineProfile | None, SolveStatus] = case()
    eigen: tuple[float, int, float | None, EngineProfile | None, SolveStatus] = case()

    @classmethod
    def Direct(cls, residual: float, condition: float | None = None, result: str | None = None, profile: EngineProfile | None = None) -> Self:
        # `condition` DEFAULTS absent: only the dense route holds a singular spectrum, so a sparse or operator solve
        # constructs without the slot rather than passing a sentinel the ledger must then learn to disbelieve.
        return cls(direct=(residual, condition, profile, status_of(result, residual, _TOL["direct"])))

    @classmethod
    def Iterative(
        cls, residual: float, iterations: int, tol: float = _TOL["iterative"], result: str | None = None, profile: EngineProfile | None = None
    ) -> Self:
        return cls(iterative=(residual, iterations, tol, profile, status_of(result, residual, tol)))

    @classmethod
    def LeastSquares(
        cls,
        residual: float,
        rank: int,
        iterations: int,
        tol: float = _TOL["least_squares"],
        result: str | None = None,
        profile: EngineProfile | None = None,
    ) -> Self:
        return cls(least_squares=(residual, rank, iterations, tol, profile, status_of(result, residual, tol)))

    @classmethod
    def Eigen(
        cls, spectral_residual: float, k: int, condition: float | None = None, result: str | None = None, profile: EngineProfile | None = None
    ) -> Self:
        return cls(eigen=(spectral_residual, k, condition, profile, status_of(result, spectral_residual, _TOL["eigen"])))

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

    @property
    def profile(self) -> EngineProfile | None:
        slotted = self.facts["profile"]
        return slotted if isinstance(slotted, EngineProfile) else None

    @property
    def ledger(self) -> dict[str, float]:
        # graduation-ledger projection over the method's DECLARED residual set alone, in `_SLOTS` order: the status
        # verdict, the profile band, and the tolerance/iteration/rank/count evidence all stay off it, and an UNMEASURED
        # slot drops rather than floating — the hub's `measured.keys() >= ceiling.keys()` gate then refuses a caller
        # whose ceiling bars a quantity this route never measured, where a forged value would silently clear it.
        return {name: float(value) for name, value in self.facts.items() if name in _LEDGER[self.tag] and isinstance(value, (int, float))}

    def contribute(self) -> Iterable[Receipt]:
        # profile band spreads `profile.`-namespaced beside the numeric slots, so a ledger metric can never shadow it.
        banded = self.profile
        facts: dict[str, object] = {
            "converged": self.converged,
            **{name: value for name, value in self.facts.items() if name != "profile"},
            **(banded.facts("profile.") if banded is not None else {}),
        }
        return (Receipt.of(EvidenceScope.RECEIPT.value, ("emitted", self.tag, facts)),)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
