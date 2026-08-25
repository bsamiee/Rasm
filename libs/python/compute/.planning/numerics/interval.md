# [PY_COMPUTE_INTERVAL]

One validated-numerics owner produces certified enclosures over a layered floor ladder: `IntervalNumerics` evaluates an interval extension over a box, certifies that an enclosure contains a target, refines an enclosure by bisection toward a width tolerance, and isolates certified polynomial roots through one `IntervalOp` dispatch. `run` returns the resulting `Yield` with its content key; each `Enclosure` retains the producer's `Certificate`.

`run` rides the hub `evidence_run` weave under the `EvidenceScope.INTERVAL` scope row and `graduates` feeds the solver-axis projection on `solvers/solve#SOLVE` with the `(ledger, ceiling, key)` triple projected from its own `Certificate`, both threading the caller's composition `ScopeKey` so an embedded composition's lifecycle and admission facts key to it. Identity is op-owned: `identity_source` hands the extension key, bounds, target, and every yield-changing knob to runtime `ContentIdentity` as N framed semantic fields, and a box admitting from a `numerics/array#PAYLOAD` payload keys through the same seed.

## [01]-[INDEX]

- [02]-[ENCLOSURE]: certified evaluate/certify/refine/roots on one `IntervalNumerics` owner over the Arb-to-mpmath-to-numpy floor ladder.

## [02]-[ENCLOSURE]

- Owner: `IntervalNumerics` folds every certified operation through one `IntervalOp` tag and returns the `Yield` its `_dispatch` arm produces.
- Cases: `Refine` carries the real extension so the refined half stays certified by the floor that produced it, never re-rounded through an identity placeholder, and a `Roots`-isolated enclosure feeds straight back as a refine target; `Certify` only narrows — a failed containment refutes the certificate and a refuted Arb enclosure stays first-class evidence of the failed claim.
- Law: the rung name and the importable module are two columns, not one — the Arb rung is reached through the `flint` module, so probing the rung's own spelling answers absent on every host and silently degrades every `Evaluate`/`Refine` to the mpmath rung. `FloorRow.module` is the sole probe target and the unconditional numpy floor earns its row by that same presence read, never by a per-rung short-circuit; the direct-`flint` root isolator is a distinct need and stays outside the fold. Certification holds by vacuous truth over an empty root set, which is sound on the span and worthless as outward proof — so `vacuous` is a governed ceiling bar beside `refuted` and `width`, and an isolation that enclosed nothing is a ceiling REJECTION rather than a crossing whose fabricated `{refuted: 0.0, width: 0.0}` ledger clears every other bar.
- Output: `run` preserves the produced enclosure or isolated-root tuple beside its content key, and graduation derives its ledger from the carried certificates.
- Growth: a new certified operation is one `IntervalOp` case, one `_dispatch` arm, and its `identity_source` arm; a new floor is one `Floor` member, one `_FLOOR_LADDER` row carrying its module column, and one `Certificate` arm; a new admission bar is one `_CEILING` row and one `attributes` slot the ledger already reads; a new relational op is one `Interval` method.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import Annotated, Final, Literal, Protocol, Self, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from expression import Nothing, Option, Some, TailCall, case, tag, tagged_union, tailrec
from expression.collections import Block, Map
from msgspec import Meta, Struct

from rasm.compute.graduation.handoff import EvidenceScope, Graduation, evidence_run
from rasm.compute.solvers.solve import graduate
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

lazy import flint
lazy import mpmath


# --- [TYPES] ----------------------------------------------------------------------------

type Width = Annotated[float, Meta(ge=0.0)]
type AccuracyBits = Annotated[int, Meta(ge=0)]
type Tag = Literal["evaluate", "certify", "refine", "roots"]
type Target = Interval | float
type Yield = Enclosure | tuple[Enclosure, ...]


class Floor(StrEnum):
    ARB = "arb"
    MPMATH = "mpmath"
    NUMPY = "numpy"

    @property
    def certifies(self) -> bool:
        return self is not Floor.NUMPY


@runtime_checkable
class Expr(Protocol):
    def over(self, ball: object, /) -> object: ...

    def key(self) -> bytes: ...


@runtime_checkable
class Poly(Expr, Protocol):
    def coeffs(self) -> Sequence[float]: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_NUMPY_GRID: Final = 17
_ULP: Final[float] = float(np.finfo(np.float64).eps)
_TINY: Final[float] = float(np.finfo(np.float64).tiny)


# --- [MODELS] ---------------------------------------------------------------------------


class Interval(Struct, frozen=True, gc=False):
    lo: float
    hi: float

    @property
    def width(self) -> Width:
        return self.hi - self.lo

    @property
    def mid(self) -> float:
        return 0.5 * (self.lo + self.hi)

    @property
    def rad(self) -> float:
        return 0.5 * self.width

    @staticmethod
    def around(mid: float, rad: float, /) -> "Interval":
        return Interval(mid - rad, mid + rad)

    @staticmethod
    def point(value: float, /) -> "Interval":
        return Interval(value, value)

    def contains(self, target: Target, /) -> bool:
        match target:
            case Interval(lo=lo, hi=hi):
                return self.lo <= lo and hi <= self.hi
            case point:
                return self.lo <= point <= self.hi

    def overlaps(self, other: "Interval", /) -> bool:
        return self.lo <= other.hi and other.lo <= self.hi

    def hull(self, other: "Interval", /) -> "Interval":
        return Interval(min(self.lo, other.lo), max(self.hi, other.hi))

    def meet(self, other: "Interval", /) -> Option["Interval"]:
        return Some(Interval(lo, hi)) if (lo := max(self.lo, other.lo)) <= (hi := min(self.hi, other.hi)) else Nothing

    def bisect(self) -> tuple["Interval", "Interval"]:
        return Interval(self.lo, self.mid), Interval(self.mid, self.hi)


class Certificate(Struct, frozen=True, gc=False):
    floor: Floor
    accuracy_bits: Option[AccuracyBits] = Nothing
    refuted: bool = False

    @property
    def certified(self) -> bool:
        return self.floor.certifies and not self.refuted

    def refute(self) -> Self:
        return Certificate(self.floor, self.accuracy_bits, refuted=True)


class Enclosure(Struct, frozen=True, gc=False):
    interval: Interval
    certificate: Certificate

    @property
    def width(self) -> Width:
        return self.interval.width

    def recertify(self, target: Target, /) -> "Enclosure":
        return self if self.interval.contains(target) else Enclosure(self.interval, self.certificate.refute())


# --- [OPERATIONS] -----------------------------------------------------------------------


@tagged_union(frozen=True)
class IntervalOp:
    tag: Tag = tag()
    evaluate: tuple[Expr, Interval] = case()
    certify: tuple[Enclosure, Target] = case()
    refine: tuple[Expr, Enclosure, Target, Width, int] = case()
    roots: tuple[Poly, Interval] = case()

    @staticmethod
    def Evaluate(expr: Expr, box: Interval, /) -> "IntervalOp":
        return IntervalOp(evaluate=(expr, box))

    @staticmethod
    def Certify(enclosure: Enclosure, target: Target, /) -> "IntervalOp":
        return IntervalOp(certify=(enclosure, target))

    @staticmethod
    def Refine(expr: Expr, enclosure: Enclosure, target: Target, target_width: Width, budget: int = 64, /) -> "IntervalOp":
        return IntervalOp(refine=(expr, enclosure, target, target_width, budget))

    @staticmethod
    def Roots(poly: Poly, box: Interval, /) -> "IntervalOp":
        return IntervalOp(roots=(poly, box))

    def identity_source(self, precision: int) -> IdentitySource:
        def _bounds(interval: Interval) -> bytes:
            return np.ascontiguousarray([interval.lo, interval.hi], dtype=np.float64).tobytes()

        def _aim(target: Target) -> bytes:
            return _bounds(target) if isinstance(target, Interval) else np.float64(target).tobytes()

        parts: tuple[bytes, ...]
        match self:
            case IntervalOp(tag="evaluate", evaluate=(expr, box)):
                parts = (expr.key(), _bounds(box), precision.to_bytes(8, "big"))
            case IntervalOp(tag="certify", certify=(enclosure, target)):
                cert = enclosure.certificate
                parts = (
                    _bounds(enclosure.interval),
                    cert.floor.value.encode(),
                    cert.accuracy_bits.map(lambda bits: bits.to_bytes(8, "big")).default_value(b"unmeasured"),
                    bytes([cert.refuted]),
                    _aim(target),
                )
            case IntervalOp(tag="refine", refine=(expr, enclosure, target, target_width, budget)):
                parts = (
                    expr.key(),
                    _bounds(enclosure.interval),
                    _aim(target),
                    np.float64(target_width).tobytes(),
                    budget.to_bytes(8, "big"),
                    precision.to_bytes(8, "big"),
                )
            case IntervalOp(tag="roots", roots=(poly, box)):
                parts = (poly.key(), _bounds(box), precision.to_bytes(8, "big"))
            case _ as unreachable:
                assert_never(unreachable)
        return IdentitySource(parts=(self.tag.encode(), *parts))


# --- [TABLES] ---------------------------------------------------------------------------


class FloorRow(Struct, frozen=True):
    floor: Floor
    evaluate: Callable[[Expr, Interval, int], Enclosure]
    module: str


def _arb_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    ball = flint.arb(box.mid, box.rad)
    result = flint.good(lambda: expr.over(ball), prec=precision, maxprec=8 * precision)
    interval = Interval.around(float(result.mid()), float(result.rad()))
    return Enclosure(interval, Certificate(Floor.ARB, Some(int(result.rel_accuracy_bits()))))


def _mpmath_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    with mpmath.workprec(precision):
        result = expr.over(mpmath.iv.mpf([box.lo, box.hi]))
    return Enclosure(Interval(float(result.a), float(result.b)), Certificate(Floor.MPMATH, Some(precision)))


def _numpy_evaluate(expr: Expr, box: Interval, _precision: int) -> Enclosure:
    samples = np.array([float(expr.over(float(x))) for x in np.linspace(box.lo, box.hi, _NUMPY_GRID)], dtype=np.float64)
    lo, hi = float(samples.min()), float(samples.max())
    interval = Interval(lo - max(_ULP * abs(lo), _TINY), hi + max(_ULP * abs(hi), _TINY))
    return Enclosure(interval, Certificate(Floor.NUMPY))


_FLOOR_LADDER: Map[Floor, FloorRow] = Map.of_seq([
    (Floor.ARB, FloorRow(Floor.ARB, _arb_evaluate, "flint")),
    (Floor.MPMATH, FloorRow(Floor.MPMATH, _mpmath_evaluate, "mpmath")),
    (Floor.NUMPY, FloorRow(Floor.NUMPY, _numpy_evaluate, "numpy")),
])


def _importable(row: FloorRow) -> bool:
    return find_spec(row.module) is not None


def _resolve_floor() -> FloorRow:
    rows = Block.of_seq(_FLOOR_LADDER.values())
    return rows.choose(lambda row: Some(row) if _importable(row) else Nothing).try_head().default_value(_FLOOR_LADDER[Floor.NUMPY])


# --- [ENCLOSURE_FOLD] -------------------------------------------------------------------


@tailrec
def _bisect(enclosure: Enclosure, expr: Expr, target: Target, target_width: Width, budget: int, floor: FloorRow, precision: int) -> Enclosure:
    if enclosure.width <= target_width or budget <= 0:
        return enclosure
    left, right = enclosure.interval.bisect()
    lo_enc, hi_enc = floor.evaluate(expr, left, precision), floor.evaluate(expr, right, precision)
    keep = lo_enc if lo_enc.interval.contains(target) or lo_enc.width <= hi_enc.width else hi_enc
    return TailCall(keep, expr, target, target_width, budget - 1, floor, precision)


def _roots(poly: Poly, box: Interval, precision: int) -> tuple[Enclosure, ...]:
    with flint.ctx.workprec(precision):
        isolated = flint.arb_poly([flint.arb(c) for c in poly.coeffs()]).real_roots()
    enclosures = (Enclosure(Interval.around(float(r.mid()), float(r.rad())), Certificate(Floor.ARB, Some(int(r.rel_accuracy_bits())))) for r in isolated)
    return tuple(enc for enc in enclosures if box.overlaps(enc.interval))


def _dispatch(op: IntervalOp, precision: int) -> Yield:
    match op:
        case IntervalOp(tag="evaluate", evaluate=(expr, box)):
            return _resolve_floor().evaluate(expr, box, precision)
        case IntervalOp(tag="certify", certify=(enclosure, target)):
            return enclosure.recertify(target)
        case IntervalOp(tag="refine", refine=(expr, enclosure, target, target_width, budget)):
            return _bisect(enclosure, expr, target, target_width, budget, _resolve_floor(), precision)
        case IntervalOp(tag="roots", roots=(poly, box)):
            return _roots(poly, box, precision)
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] ----------------------------------------------------------------------------

_CEILING: Final[Map[str, float]] = Map.of_seq([("refuted", 0.0), ("width", 1e-6), ("vacuous", 0.0)])


class IntervalNumerics:
    @staticmethod
    @beartype(conf=FAULT_CONF)
    def run(op: IntervalOp, *, precision: int = 128, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[tuple[Yield, ContentKey]]:
        def result() -> RuntimeRail[tuple[Yield, ContentKey]]:
            yielded = _dispatch(op, precision)
            return ContentIdentity.of(f"interval.{op.tag}", op.identity_source(precision)).map(lambda key: (yielded, key))

        facts = {"op": op.tag, "precision": precision}
        return evidence_run(EvidenceScope.INTERVAL, f"interval.{op.tag}", result, facts=facts, composition=composition)

    @staticmethod
    def graduates(
        result: tuple[Yield, ContentKey], subject: str = "interval-certificate", *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Graduation]":
        yielded, key = result
        enclosures = yielded if isinstance(yielded, tuple) else (yielded,)
        ledger = {
            "refuted": float(any(enclosure.certificate.refuted for enclosure in enclosures)),
            "width": max((enclosure.width for enclosure in enclosures), default=float("inf")),
            "vacuous": float(not enclosures),
        }
        return graduate(EvidenceScope.INTERVAL.value, subject, key, ledger, dict(_CEILING.items()), composition=composition)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
