# [PY_COMPUTE_SOLVE]

`Solve[T]` is the one method-discriminated solve result every solver route returns — a single `@tagged_union` whose `Literal` tag IS the solve method (`direct`, `iterative`, `least_squares`, `eigen`), each case carrying the actual typed solution beside its numeric measures, optional jit-minted `EngineProfile` band, and one terminating `SolveStatus`. `SolveStatus` is the one bounded termination vocabulary every backend folds into — the `lineax`/`optimistix`/`diffrax` `RESULTS` enums, the `scipy` `info`/`istop`/`success` codes, the `cvxpy` feasibility constants, the residual-floor verdict — so a converged, event-terminated, max-steps, singular, or stagnated solve is a distinct first-class verdict carrying its own `converged` predicate rather than one Boolean collapsing every non-success cause to `False`. Every case carries the `ContentKey` its route minted, so the result names the computation it settles and the per-route `graduates` wrapper carries no key beside a value that already holds one. A `Solve` carries the termination facts the C# graduation gate reads and holds no benchmark authority, no substrate selection, and never the admit/reject verdict the `HandoffAxis` cases own.

Three exported folds stay stable across the solver plane: `status_of`, the one termination fold `mesh`, `field`, and `design` compose by name; `verdict`, the one `equinox.Enumeration` `RESULTS._name_to_item` inversion the gated routes compose, taking the caller's x64-gated `jax.numpy` handle and the `RESULTS` class as parameters so this owner imports neither `jax` nor `equinox`; and `graduate`, the one solver-axis graduation projection discriminating on its evidence shape — a fed `Solve` projects its own `ledger`, a prepared ledger passes through — so a crossing composes the value, family ceiling row, key, and the caller's composition key in one call and the fold imports no downstream type. `scipy` `info`/`istop` codes fold in through the `solvers/linear#LINEAR` projections; `EVENT` is the terminal class `solvers/differential#DIFFERENTIAL` adds for a `diffrax.Event` crossing, and `INFEASIBLE`/`UNBOUNDED` are the feasibility verdicts `optimization/convex#CONVEX` folds the cvxpy constants into. Solves graduate outward through `graduate` on the `solver` `HandoffAxis` case into the `graduation/handoff#GRADUATION` `Graduation`, and every factory stamps its `attributes` on the live span the route's `evidence_run` opened, so the trace carries the same facts the value does.

## [01]-[INDEX]

- [02]-[SOLVE]: the one method-discriminated solve result and the `SolveStatus` termination vocabulary the solver plane folds into.

## [02]-[SOLVE]

- Owner: `Solve[T]` — the one parametric `@tagged_union` over every route; `.tag` IS the method literal, never a thin `.method` re-exposure, and `.value` retains the producer's concrete solution type. `status` is the LAST payload slot of every case by construction, so `.status` is one total `match self` binding the trailing `(*_, SolveStatus() as status)` across the four cases and closing on `assert_never` — sound because the match is over `self`, the closed union, never a reflective `getattr(self, self.tag)` whose `object` residual makes the `assert_never` tail a lie. `_SLOTS` is the one `Map[SolveMethod, tuple[str, ...]]` fact vocabulary; `.facts` skips the typed value and zips each row against the remaining payload under `strict=True`. `_LEDGER` is its graduation counterpart — the per-method DECLARED residual set the outward `ledger` narrows to — so evidence a ceiling cannot bar never reaches the admission fold as a pseudo-residual. Every case mounts the jit-minted `EngineProfile` as its optional `profile` slot before `status`, so a solve accelerated through a compiled kernel carries the engine's own measurements beside its numbers and a slow solve explains itself from the value, never from an external profiler attach.
- Cases: `SolveStatus` is the one bounded termination `StrEnum` and a value object — `converged` tests membership in the `_CONVERGENT` `frozenset` (`SUCCESS` and the diffrax `EVENT`), folded once rather than re-spelled at every consumer, and the value's `converged` delegates to it so the Boolean contract survives while the value carries *why* a solve did not converge. A backend that adjudicates termination maps in through the one `_STATUS` boundary table keyed on the documented `RESULTS` member-name strings; a numpy floor with no adjudicator derives its verdict from the residual against tolerance.
- Law: `condition` is `float | None` on the `direct` and `eigen` cases and defaults absent on their factories, because only a dense route holds the singular spectrum a condition number reads — a sparse factorization, a `SuperLU` back-substitution, an ARPACK stall, and a lineax operator solve each measure no conditioning at all. The unmeasured slot leaves the `ledger` rather than floating, so the hub's key-coverage gate refuses a ceiling naming a quantity the route never took; a `float("nan")` in the slot is the deleted form — it enters the ledger as a value, then breaches the hub's own finiteness refinement on every sparse crossing.
- Entry: the four `@classmethod` factories `Direct`/`Iterative`/`LeastSquares`/`Eigen` return `Self` — binding the subtype, not a forward-ref re-spelled four times — and terminate their payload through `status_of`, a route holding a backend `RESULTS` member passing its name (gated routes derive it through `verdict`), a numpy-floor route passing `None` to let the residual floor adjudicate. `status_of` is one total `match` over the `str | None` discriminant: `case str()` degrades an unmapped member to `OTHER` rather than crashing, the guarded `case None` returns `NONFINITE`, the bare `case None` returns `SUCCESS`/`STAGNATION` off the residual-vs-tolerance floor, and the trailing `assert_never` witnesses totality — backend status where it exists, the residual floor where it does not, never two parallel convergence notions. Method tolerances live in one frozen `_TOL` table keyed by tag.
- Output: every factory closes through `_noted`, which writes the value's `attributes` onto the current span — method, key, `converged`, every numeric and vocabulary slot, and the `profile.`-namespaced engine band — so the route's `evidence_run` span carries the termination facts without a second emit surface, and a solve minted outside any span writes onto the no-op span and costs nothing. The profile band stays off the graduation `ledger`, so a profile extent can never masquerade as a residual a ceiling clears.
- Packages: `expression` (`tagged_union`/`case`/`tag`, and `Map` for every dispatch table), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attributes`), stdlib `enum.StrEnum`/`math.isfinite`/`types.ModuleType`, runtime (`ContentKey`, `RuntimeRail`, and the `ScopeKey`/`DEFAULT_SCOPE` composition key `graduate` forwards), the downward hub graduation import (`Graduation`/`HandoffAxis`), and the `numerics/jit` `EngineProfile` band import.
- Growth: a new convergence shape is one `Solve` case, one `_TOL` row, one `_SLOTS` row leading with `key`, and one `_LEDGER` row, its attributes projecting with no `_noted` edit; a new graded quantity is one `_LEDGER` member, a new call-evidence slot one `_SLOTS` entry the ledger never sees; a new backend termination reason is one `_STATUS` row into the existing vocabulary, or one new `SolveStatus` member when a genuinely new termination class appears (`EVENT` being that path realized); a profiled solve is the same factory call carrying its `profile` row, and a new profile statistic lands on the jit `EngineProfile` with zero edits here; a new graduating solve family is one `graduate` call with its value, family ceiling row, and composition key — the status, verdict, fact, ledger, and graduation folds reused, never re-inlined.
- Boundary: `SolveStatus` is the vocabulary the C# graduation gate reads, not the gate itself; the admit/reject verdict belongs to the `convex_program`/`solver` `HandoffAxis` cases, and the graduation crossing to the one `graduate` projection rather than a per-owner inline `HandoffAxis(solver=...)`. Family DEFAULT ceilings are policy rows on each family's own carrier beside its route table; the caller's tighter row overrides. Composition custody is the caller's — `graduate` forwards the key it is handed and defaults `DEFAULT_SCOPE`, so the root call shape stays scope-free and the registry partition is the hub owner's mechanic, never re-derived here.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from math import isfinite
from types import ModuleType
from typing import Final, Literal, Self, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from opentelemetry import trace

from rasm.compute.graduation.handoff import Graduation, HandoffAxis
from rasm.compute.numerics.jit import EngineProfile
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------


type SolveMethod = Literal["direct", "iterative", "least_squares", "eigen"]
type SolveSlot = float | int | ContentKey | EngineProfile | Provider | None | SolveStatus
type Attributes = dict[str, str | bool | int | float]


class Provider(StrEnum):
    GATED = "gated"
    FLOOR = "floor"


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_CONVERGENT: frozenset[SolveStatus] = frozenset({SolveStatus.SUCCESS, SolveStatus.EVENT})

_TOL: Final[Map[SolveMethod, float]] = Map.of_seq([("direct", 1e-6), ("iterative", 1e-6), ("least_squares", 1e-6), ("eigen", 1e-8)])

_SLOTS: Final[Map[SolveMethod, tuple[str, ...]]] = Map.of_seq([
    ("direct", ("key", "residual", "condition", "profile", "status")),
    ("iterative", ("key", "residual", "iterations", "tol", "provider", "profile", "status")),
    ("least_squares", ("key", "residual", "rank", "iterations", "tol", "provider", "profile", "status")),
    ("eigen", ("key", "spectral_residual", "k", "condition", "profile", "status")),
])

_LEDGER: Final[Map[SolveMethod, frozenset[str]]] = Map.of_seq([
    ("direct", frozenset({"residual", "condition"})),
    ("iterative", frozenset({"residual"})),
    ("least_squares", frozenset({"residual"})),
    ("eigen", frozenset({"spectral_residual", "condition"})),
])

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


# --- [OPERATIONS] -----------------------------------------------------------------------


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
    names: dict[int, str] = {int(item._value): name for name, item in results._name_to_item.items()}
    return names[int(gated.max(outcome._value))]


def graduate[T](
    owner: str,
    subject: str,
    key: ContentKey,
    evidence: "Solve[T] | dict[str, float]",
    ceiling: dict[str, float],
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[Graduation]:
    ledger = evidence.ledger if isinstance(evidence, Solve) else evidence
    return Graduation.graduates(owner, HandoffAxis(solver=subject), key, ledger, ceiling, composition=composition)


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Solve[T]:
    tag: SolveMethod = tag()
    direct: tuple[T, ContentKey, float, float | None, EngineProfile | None, SolveStatus] = case()
    iterative: tuple[T, ContentKey, float, int, float, Provider, EngineProfile | None, SolveStatus] = case()
    least_squares: tuple[T, ContentKey, float, int, int, float, Provider, EngineProfile | None, SolveStatus] = case()
    eigen: tuple[T, ContentKey, float, int, float | None, EngineProfile | None, SolveStatus] = case()

    @classmethod
    def Direct(
        cls, value: T, key: ContentKey, residual: float, condition: float | None = None, result: str | None = None,
        profile: EngineProfile | None = None,
    ) -> Self:
        return cls(direct=(value, key, residual, condition, profile, status_of(result, residual, _TOL["direct"])))._noted()

    @classmethod
    def Iterative(
        cls, value: T, key: ContentKey, residual: float, iterations: int, provider: Provider, tol: float = _TOL["iterative"],
        result: str | None = None, profile: EngineProfile | None = None,
    ) -> Self:
        return cls(iterative=(value, key, residual, iterations, tol, provider, profile, status_of(result, residual, tol)))._noted()

    @classmethod
    def LeastSquares(
        cls,
        value: T,
        key: ContentKey,
        residual: float,
        rank: int,
        iterations: int,
        provider: Provider,
        tol: float = _TOL["least_squares"],
        result: str | None = None,
        profile: EngineProfile | None = None,
    ) -> Self:
        return cls(least_squares=(value, key, residual, rank, iterations, tol, provider, profile, status_of(result, residual, tol)))._noted()

    @classmethod
    def Eigen(
        cls, value: T, key: ContentKey, spectral_residual: float, k: int, condition: float | None = None, result: str | None = None,
        profile: EngineProfile | None = None,
    ) -> Self:
        return cls(eigen=(value, key, spectral_residual, k, condition, profile, status_of(result, spectral_residual, _TOL["eigen"])))._noted()

    @property
    def status(self) -> SolveStatus:
        match self:
            case (
                Solve(tag="direct", direct=(*_, SolveStatus() as status))
                | Solve(tag="iterative", iterative=(*_, SolveStatus() as status))
                | Solve(tag="least_squares", least_squares=(*_, SolveStatus() as status))
                | Solve(tag="eigen", eigen=(*_, SolveStatus() as status))
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
                Solve(tag="direct", direct=(_, *payload))
                | Solve(tag="iterative", iterative=(_, *payload))
                | Solve(tag="least_squares", least_squares=(_, *payload))
                | Solve(tag="eigen", eigen=(_, *payload))
            ):
                return dict(zip(_SLOTS[self.tag], payload, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def value(self) -> T:
        match self:
            case (
                Solve(tag="direct", direct=(value, *_))
                | Solve(tag="iterative", iterative=(value, *_))
                | Solve(tag="least_squares", least_squares=(value, *_))
                | Solve(tag="eigen", eigen=(value, *_))
            ):
                return value
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def profile(self) -> EngineProfile | None:
        slotted = self.facts["profile"]
        return slotted if isinstance(slotted, EngineProfile) else None

    @property
    def content_key(self) -> ContentKey:
        return self.facts["key"]

    @property
    def ledger(self) -> dict[str, float]:
        return {name: float(value) for name, value in self.facts.items() if name in _LEDGER[self.tag] and isinstance(value, (int, float))}

    @property
    def attributes(self) -> Attributes:
        banded = self.profile
        return {
            "method": self.tag,
            "key": self.content_key.hex,
            "converged": self.converged,
            **{name: value for name, value in self.facts.items() if isinstance(value, str | bool | int | float)},
            **(banded.facts("profile.") if banded is not None else {}),
        }

    def _noted(self) -> Self:
        trace.get_current_span().set_attributes(self.attributes)
        return self
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
