# [PY_COMPUTE_INTERVAL]

The one validated-numerics owner producing certified enclosures over a layered floor ladder: `IntervalNumerics` evaluates an interval extension over a box, certifies that an enclosure contains a target, refines an enclosure by bisection toward a width tolerance, and isolates certified polynomial roots, every operation one tag on one `IntervalOp` dispatch. The receipt names which `Floor` certified an enclosure and how tight the ball is — Arb and mpmath certify, the numpy grid is a sound-but-uncertified band — so rigor is first-class evidence, never a bare boolean.

`run` rides the hub `evidence_run` weave under the `compute.interval` scope row, and `graduates` feeds the solver-axis projection on `solvers/receipt.md#RECEIPT` with the `(ledger, ceiling, key)` triple projected from its own `Certificate`. Identity is op-owned: `identity_buffer` folds the extension key, bounds, target, and every yield-changing knob through runtime `ContentIdentity`, and a box admitting from a `numerics/array.md#PAYLOAD` payload keys through the same seed.

## [01]-[INDEX]

- [01]-[ENCLOSURE]: certified evaluate/certify/refine/roots on one `IntervalNumerics` owner over the Arb-to-mpmath-to-numpy floor ladder.

## [02]-[ENCLOSURE]

- Owner: `IntervalNumerics` — every certified operation is one `IntervalOp` tag over one `_dispatch` fold, never a parallel rigorous-arithmetic surface or a per-tag evaluator family; `_dispatch` returns the honest `Yield` union its arms produce, and `IntervalReceipt.of` folds that union total, so no phantom output type parameter rides the carrier.
- Cases: `Refine` carries the real extension so the refined half stays certified by the floor that produced it, never re-rounded through an identity placeholder, and a `Roots`-isolated enclosure feeds straight back as a refine target; `Certify` only narrows — a failed containment refutes the certificate and a refuted Arb enclosure stays first-class evidence of the failed claim.
- Receipt: a `Roots` tuple reports its widest (loosest-certified) member beside the root count, and an empty isolation is a vacuous certified row rather than a missing receipt; `span_facts` and the receipt facts share one projection so the OTLP attribute set never forks from the receipt.
- Growth: a new certified operation is one `IntervalOp` case plus one `_dispatch` arm plus its `identity_buffer` arm; a new floor is one `Floor` member plus one `_FLOOR_LADDER` row plus one `Certificate` arm; a new relational op is one `Interval` method.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import StrEnum
from typing import Annotated, Final, Literal, Protocol, Self, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Some, TailCall, case, tag, tagged_union, tailrec
from expression.collections import Block, Map
from msgspec import Meta, Struct

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, evidence_run
from rasm.compute.solvers.receipt import graduate
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------

type Width = Annotated[float, Meta(ge=0.0)]
type AccuracyBits = Annotated[int, Meta(ge=0)]
type Tag = Literal["evaluate", "certify", "refine", "roots"]  # the op discriminant the receipt reads
type Target = Interval | float  # a containment/bracket target: a scalar point or a sub-interval
type Yield = Enclosure | tuple[Enclosure, ...]  # Evaluate/Certify/Refine -> one, Roots -> a tuple


class Floor(StrEnum):
    # the closed certified-arithmetic ladder, tightest first; the enum value names the rung the `Certificate` carries.
    ARB = "arb"
    MPMATH = "mpmath"
    NUMPY = "numpy"

    @property
    def certifies(self) -> bool:
        return self is not Floor.NUMPY


@runtime_checkable
class Expr(Protocol):
    # inclusion-monotone over whatever ball/interval type the resolving floor lifts, so one closure evaluates on every rung; a
    # symbolic derivation lowers a `sympy.lambdify(..., modules='mpmath')`/Arb closure to this shape.
    def over(self, ball: object, /) -> object: ...

    # the extension's STABLE identity (lowered-spec digest or canonical coefficient bytes) — an anonymous
    # closure with no key cannot enter the identity rail.
    def key(self) -> bytes: ...


@runtime_checkable
class Poly(Expr, Protocol):
    # exposes the `arb`-domain coefficient vector so `Roots` feeds `arb_poly.real_roots`; `key()` is the canonical float64 render of these.
    def coeffs(self) -> Sequence[float]: ...


# --- [CONSTANTS] ---------------------------------------------------------------------------

_NUMPY_GRID: Final = 17  # the uncertified `Numpy` floor's `linspace` sample count over the box hull
_ULP: Final[float] = float(np.finfo(np.float64).eps)  # one relative double ulp: the `np.finfo` outward-pad scale
_TINY: Final[float] = float(np.finfo(np.float64).tiny)  # the absolute outward floor near zero, so a [0, 0] hull still widens


# --- [MODELS] ------------------------------------------------------------------------------


class Interval(Struct, frozen=True, gc=False):
    # owns the relational algebra `flint.arb`/`mpmath.iv` expose, so containment/hull/bisect are methods, never re-derived per call site.
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
        # one polymorphic containment discriminated by target shape — never a `contains`/`contains_interval` pair.
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
        # `Nothing` on disjoint intervals — an inverted `lo > hi` meet is structural absence, never a malformed interval.
        return Some(Interval(lo, hi)) if (lo := max(self.lo, other.lo)) <= (hi := min(self.hi, other.hi)) else Nothing

    def bisect(self) -> tuple["Interval", "Interval"]:
        return Interval(self.lo, self.mid), Interval(self.mid, self.hi)


class Certificate(Struct, frozen=True, gc=False):
    floor: Floor
    accuracy_bits: AccuracyBits = 0
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


class IntervalReceipt(Struct, frozen=True):
    op: Tag
    floor: Floor
    width: Width
    accuracy_bits: AccuracyBits
    certified: bool
    roots: int
    content_key: ContentKey

    @staticmethod
    def of(op: Tag, yielded: Yield, key: ContentKey, /) -> "IntervalReceipt":
        # an `Enclosure` `Struct` is not a `Sequence`, so the single-enclosure ops fall to the capture arm, never matched as `[*roots]`.
        match yielded:
            case [] | ():
                return IntervalReceipt(op, Floor.ARB, 0.0, 0, certified=True, roots=0, content_key=key)
            case [*roots]:
                widest = max(roots, key=lambda e: e.width)
                cert = widest.certificate
                return IntervalReceipt(op, cert.floor, widest.width, cert.accuracy_bits, cert.certified, len(roots), key)
            case enclosure:
                cert = enclosure.certificate
                return IntervalReceipt(op, cert.floor, enclosure.width, cert.accuracy_bits, cert.certified, 1, key)

    @property
    def span_facts(self) -> dict[str, object]:
        # the `content_key` hex rides the receipt facts only, not the OTLP attribute set.
        return {"floor": self.floor.value, "width": self.width, "accuracy_bits": self.accuracy_bits, "certified": self.certified, "roots": self.roots}

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {**self.span_facts, "content_key": self.content_key.project("hex")}
        return (Receipt.of("compute.interval", ("emitted", self.op, facts)),)


# --- [OPERATIONS] --------------------------------------------------------------------------


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

    def identity_buffer(self, precision: int) -> bytes:
        # tag, stable `Expr.key()`, every bound, the target, and every yield-changing knob fold into the buffer, so two different
        # computations over one box never share a `ContentKey`; length-prefixed parts keep the buffer unambiguous.
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
                parts = (_bounds(enclosure.interval), cert.floor.value.encode(), cert.accuracy_bits.to_bytes(8, "big"), bytes([cert.refuted]), _aim(target))
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
        return b"".join(len(part).to_bytes(8, "big") + part for part in (self.tag.encode(), *parts))


# --- [TABLES] ------------------------------------------------------------------------------


# the ladder is data, not control flow: each row binds a `Floor` to its evaluator behind the gated import, and `_resolve_floor`
# keeps the tightest importable row through one first-available fold — never stacked `try/except ImportError: pass` blocks.
class FloorRow(Struct, frozen=True):
    floor: Floor
    evaluate: Callable[[Expr, Interval, int], Enclosure]


def _arb_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    import flint

    ball = flint.arb(box.mid, box.rad)
    # `flint.good` drives precision up adaptively, capped at `maxprec` so a non-convergent extension halts rather than looping;
    # seeding through `prec=` keeps working precision call-local instead of leaking a session `ctx.prec` mutation across evaluators.
    result = flint.good(lambda: expr.over(ball), prec=precision, maxprec=8 * precision)
    interval = Interval.around(float(result.mid()), float(result.rad()))
    return Enclosure(interval, Certificate(Floor.ARB, int(result.rel_accuracy_bits())))


def _mpmath_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    import mpmath

    # `workprec` restores on exit so the `iv`-context evaluation never leaks a session `prec` mutation; `iv.mpf([lo, hi])` lifts the
    # box to the inclusion-monotone interval whose `.a`/`.b` endpoints read back as the certified band.
    with mpmath.workprec(precision):
        result = expr.over(mpmath.iv.mpf([box.lo, box.hi]))
    return Enclosure(Interval(float(result.a), float(result.b)), Certificate(Floor.MPMATH, precision))


def _numpy_evaluate(expr: Expr, box: Interval, _precision: int) -> Enclosure:
    # numpy carries no interval type: sample the box on a grid, hull, pad outward by one relative `finfo` ulp floored at `tiny` so a
    # degenerate hull still widens — a directed-rounding emulation, since the branch numpy surface carries no `nextafter` step.
    # Sound for a monotone extension, a heuristic band otherwise.
    samples = np.array([float(expr.over(float(x))) for x in np.linspace(box.lo, box.hi, _NUMPY_GRID)], dtype=np.float64)
    lo, hi = float(samples.min()), float(samples.max())
    interval = Interval(lo - max(_ULP * abs(lo), _TINY), hi + max(_ULP * abs(hi), _TINY))
    return Enclosure(interval, Certificate(Floor.NUMPY))


_FLOOR_LADDER: Map[Floor, FloorRow] = Map.of_seq([
    (Floor.ARB, FloorRow(Floor.ARB, _arb_evaluate)),
    (Floor.MPMATH, FloorRow(Floor.MPMATH, _mpmath_evaluate)),
    (Floor.NUMPY, FloorRow(Floor.NUMPY, _numpy_evaluate)),
])


def _importable(floor: Floor) -> bool:
    from importlib.util import find_spec

    # package is present, so the ladder fold reads availability without importing the heavy extension.
    return floor is Floor.NUMPY or find_spec(floor.value) is not None


def _resolve_floor() -> FloorRow:
    rows = Block.of_seq(_FLOOR_LADDER.values())
    return rows.choose(lambda row: Some(row) if _importable(row.floor) else Nothing).try_head().default_value(_FLOOR_LADDER[Floor.NUMPY])


# --- [ENCLOSURE_FOLD] ----------------------------------------------------------------------


@tailrec
def _bisect(enclosure: Enclosure, expr: Expr, target: Target, target_width: Width, budget: int, floor: FloorRow, precision: int) -> Enclosure:
    # keep the half whose certified extension brackets the target (else the tighter half); stack-safe under the `tailrec` trampoline.
    if enclosure.width <= target_width or budget <= 0:
        return enclosure
    left, right = enclosure.interval.bisect()
    lo_enc, hi_enc = floor.evaluate(expr, left, precision), floor.evaluate(expr, right, precision)
    keep = lo_enc if lo_enc.interval.contains(target) or lo_enc.width <= hi_enc.width else hi_enc
    return TailCall(keep, expr, target, target_width, budget - 1, floor, precision)


def _roots(poly: Poly, box: Interval, precision: int) -> tuple[Enclosure, ...]:
    import flint

    # membership is `box.overlaps` over the certified ball interval, not a midpoint-only `contains`, so a root straddling the box
    # boundary is retained; `ctx.workprec` block-scopes the precision (`real_roots` is not `flint.good`-driven) and restores on exit.
    with flint.ctx.workprec(precision):
        isolated = flint.arb_poly([flint.arb(c) for c in poly.coeffs()]).real_roots()
    enclosures = (Enclosure(Interval.around(float(r.mid()), float(r.rad())), Certificate(Floor.ARB, int(r.rel_accuracy_bits()))) for r in isolated)
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


def _keyed(op: IntervalOp, precision: int) -> RuntimeRail[ContentKey]:
    # the key names the computation, never merely the box — a repeated op at identical precision keys by reference.
    return ContentIdentity.of(f"interval.{op.tag}", op.identity_buffer(precision))


@beartype(conf=FAULT_CONF)
def _report(op: IntervalOp, precision: int) -> IntervalReceipt:
    # the railed `_keyed` digest is matched HERE inside the already-fenced body — an `Error` re-raises onto the enclosing boundary,
    # which re-folds it — so the impure floor solve and the pure key fold ride ONE fence and the entry mints exactly one rail.
    yielded = _dispatch(op, precision)
    match _keyed(op, precision):
        case Ok(key):
            return IntervalReceipt.of(op.tag, yielded, key)
        case Error(fault):
            raise RuntimeError(fault)


# --- [ENTRY] -------------------------------------------------------------------------------

# the interval family's default graduation ceiling; a certified enclosure admits zero refutation and a finite width bound.
_CEILING: Final[Map[str, float]] = Map.of_seq([("refuted", 0.0), ("width", 1e-6)])


class IntervalNumerics:
    @staticmethod
    def run(op: IntervalOp, *, precision: int = 128) -> RuntimeRail[IntervalReceipt]:
        return evidence_run(EvidenceScope.INTERVAL, f"interval.{op.tag}", lambda: _report(op, precision))

    @staticmethod
    def graduates(receipt: IntervalReceipt, subject: str = "interval-certificate") -> "RuntimeRail[GraduationReceipt]":
        # the family ceiling row governs; a caller's tighter row overrides at the hub.
        ledger = {"refuted": 0.0 if receipt.certified else 1.0, "width": float(receipt.width)}
        return graduate("compute.interval", subject, receipt.content_key, ledger, dict(_CEILING.items()))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
