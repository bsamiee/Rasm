# [PY_COMPUTE_INTERVAL]

The one validated-numerics owner producing certified enclosures over a layered floor ladder. `IntervalNumerics` evaluates an interval extension over an input box, certifies that an enclosure contains a target, and refines an enclosure by bisection toward a width tolerance, all on one `IntervalOp` `@tagged_union` whose tag selects the operation and whose case payload parameterizes the input shape (a single box or a polynomial); the certified-output shape is the `Yield = Enclosure | tuple[Enclosure, ...]` union the dispatch fold returns and `IntervalReceipt.of` collapses total, so `run` is one uniform `RuntimeRail[IntervalReceipt]` egress over every op. `Interval` is the inclusion-monotone `Meta`-bounded value object owning the full relational algebra (`contains`/`overlaps`/`hull`/`meet`/`bisect`/`mid`/`rad`) `flint.arb` and `mpmath.iv` expose, so containment and hull are value-object methods rather than re-derived per call. `Certificate` replaces the prior lone `bool certified` flag with the bounded `Floor` ladder member that produced the enclosure plus its `rel_accuracy_bits` evidence, so a receipt names *which* floor certified an enclosure and *how tight* the ball is, not merely that it did. `Floor` is the closed Arb→mpmath→numpy ladder vocabulary the `_FLOOR_LADDER` data table folds — the resolution is a `Block.choose`/`try_head` first-available fold over the table, never the three stacked `try/except ImportError: pass` blocks that smuggled import-failure into domain control flow.

## [01]-[INDEX]

- [01]-[ENCLOSURE]: certified interval evaluation, containment certification, and width-driven bisection refinement on one `IntervalNumerics` owner driven by the `_FLOOR_LADDER` data table (each row a `(Floor, evaluator)` pair), the `flint.arb`/`mpmath.iv`/`numpy` floor ladder resolved by a first-available `Block.choose` fold, the `flint.good` adaptive-precision driver and `arb_poly.real_roots` certified root isolation stacked into the evaluate and refine rails, content identity keyed through the runtime `ContentIdentity` over the op-owned `IntervalOp.identity_buffer` fold (extension key, bounds, target, and yield-changing knobs — never the box alone), and the `@receipted` egress aspect.

## [02]-[ENCLOSURE]

- Owner: `IntervalNumerics` — the ONE validated-arithmetic owner; `Interval` the inclusion-monotone `[lo, hi]` value object with its relational algebra, `Enclosure` the interval plus a `Certificate`, `Floor` the closed `Arb`/`Mpmath`/`Numpy` ladder vocabulary, `Certificate` the floor-and-`rel_accuracy_bits` evidence the receipt reads, and `IntervalOp` the `@tagged_union` discriminating `Evaluate(expr, box)` (interval extension over a box through the tightest available floor), `Certify(enclosure, target)` (does the enclosure provably contain a target `Interval` or scalar), `Refine(expr, enclosure, target, target_width, budget)` (`tailrec` bisection re-evaluating `expr` over each half toward a width tolerance, retaining the half whose certified extension brackets the target), and `Roots(poly, box)` (certified `arb_poly.real_roots` isolation as a tuple of enclosures). Every certified operation is one tag on this owner over one `_dispatch` fold, never a parallel rigorous-arithmetic surface and never a per-tag `_*_evaluate` body.
- Operation fold: `_dispatch` is the one total `match` over `IntervalOp` closed by `assert_never` — `Evaluate` resolves the ladder and runs the floor evaluator, `Certify` reseats the enclosure's `Certificate` to `Refuted` when `Interval.contains` fails (the certification narrows, never widens), `Refine` runs the `tailrec` `_bisect` loop, and `Roots` isolates certified real roots. `_dispatch(op) -> Yield` returns the honest `Yield = Enclosure | tuple[Enclosure, ...]` union its arms produce — `Evaluate`/`Certify`/`Refine` fold to one `Enclosure`, `Roots` to a `tuple[Enclosure, ...]` — and `IntervalReceipt.of` folds that union total over both shapes, so the output shape is the union the body proves rather than a phantom output type parameter on the carrier the arms cannot statically satisfy.
- Refine: `_bisect` is the `expression` `tailrec` trampoline — it bisects the interval through `Interval.bisect`, re-evaluates the carried `Expr` over each half through the resolved floor, retains the half whose certified extension brackets the target (or the tighter half when neither does), and returns `TailCall` until the width falls under `target_width` or the integer `budget` is exhausted, so refinement is a stack-safe fold toward a tolerance rather than the prior single parity-keyed `Interval(lo, mid) if budget % 2` step that discarded the budget, the target, and root bracketing. `Refine` carries the real extension rather than an identity placeholder, so the refined half stays certified by the floor that produced it instead of being re-rounded; a `Roots`-isolated enclosure feeds straight back as the refine target.
- Entry: `IntervalNumerics.run(op, *, precision)` is the one static-method entry on the `IntervalNumerics` owner, riding the hub `evidence_run` weave under the `compute.interval` scope row — the former page-local `compute.rigor` tracer literal disagreed with the owning module leaf and is the deleted form — so span, fault fence, and the fenced `@receipted(REDACTION)` receipt harvest are composed, never page-owned. `IntervalNumerics.graduates(receipt)` FEEDS the solver-axis graduation projection on `solvers/receipt` with the `(ledger, ceiling, key)` triple projected from its own `Certificate` evidence (refutation and width against the `_CEILING` family policy row), the one producer pattern the hub rules.
- Receipt: `IntervalReceipt.of(op_tag, yielded, key)` folds the dispatched `Yield` total over both output shapes — a single `Enclosure` reads its own width/floor/`accuracy_bits`, a `Roots` tuple folds to its widest (loosest-certified) member plus the `roots` count, an empty tuple a vacuous certified row — so one receipt carries every op without a per-tag carrier. `span_facts` projects the bounded native scalars the `is_recording()`-gated span batches through `set_attributes`; `contribute` returns the one-element `tuple[Receipt, ...]` the runtime `ReceiptContributor` port streams — `Receipt.of("compute.interval", ("emitted", op_tag, facts))` against the runtime two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return against the `Iterable[Receipt]` port — spreading `span_facts` plus the content-key hex so the OTLP attribute set and the receipt facts share one projection. The facts ride the width, the `Floor` tag, the `rel_accuracy_bits`, the `roots` count, and the content-key hex as native scalars the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without an `f""`/`str()` coerce. A `Floor.Arb` or `Floor.Mpmath` enclosure graduates as a proof `graduation/handoff.md#GRADUATION` admits on the `solver` axis; a `Floor.Numpy` band graduates only as advisory evidence, the floor distinction first-class on the `Certificate` rather than a bare boolean.
- Packages: `flint` (`arb`/`arb(mid, rad)`, `arb.mid`/`arb.rad`/`arb.rel_accuracy_bits` the ball endpoints and accuracy evidence, `arb.contains`/`arb.overlaps`/`arb.union`/`arb.intersection`/`arb.is_finite` the relational algebra the `Interval` value-object methods mirror, `arb_poly`/`arb_poly.real_roots` certified root isolation, `flint.good(func, prec=, maxprec=)` the seeded adaptive-precision driver and `flint.ctx.workprec` the block-local precision scope for the `Roots` isolation — python-flint Arb ball arithmetic), `mpmath` (`iv.mpf` the interval lift, `workprec` the block-local restore-safe precision scope for the `iv`-context floor — the arbitrary-precision interval-context floor), `numpy` (`linspace`/`array`/`min`/`max` the `Numpy`-floor grid hull, `finfo(float64).eps`/`tiny` the `_ULP`/`_TINY` outward directed-rounding pad over the hull endpoints, `ascontiguousarray`/`float64` the canonical content-identity buffer), `expression` (`tagged_union`/`tag`/`case` the `IntervalOp` ADT, `Block.of_seq`/`Block.choose`/`Block.try_head` the floor-ladder first-available fold, `tailrec`/`TailCall` the bisection trampoline, `Ok`/`Error` the `run`-arm and in-body `_keyed` re-raise `match`, `Option`/`Some`/`Nothing` the floor-resolution and root-bracket folds), `msgspec` (`Struct`, `Meta` the `Interval`/`Certificate` bounds), `expression.collections` (`Map` the `_FLOOR_LADDER` table rail), `beartype` (`@beartype(conf=FAULT_CONF)` the `_report` contract fence whose violation the `CLASSIFY` `api` row folds onto the rail), `numerics/array.md#PAYLOAD` (a box admitting from an `ArrayPayload` keys through the same `ContentIdentity.of` seed), hub (`EvidenceScope`/`evidence_run`/`GraduationReceipt` — the weave and the graduation carrier), `solvers/receipt.md#RECEIPT` (`graduate` the solver-axis projection this owner feeds), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF`, `ContentIdentity`/`ContentKey`/`CANONICAL_POLICY`, `Receipt` the contributor row)` egress aspect owning the `Signals.emit` fold rather than imported here).
- Growth: a new certified operation is one `IntervalOp` case plus one `_dispatch` arm; a new floor is one `Floor` member plus one `_FLOOR_LADDER` row plus one `Certificate` arm; a new relational op is one `Interval` method; a new evidence slot is one column on `IntervalReceipt`; zero new surface, never a parallel rigorous-arithmetic struct, never a per-floor `_*_evaluate` body, never a `try/except` ladder parallel to the `_FLOOR_LADDER` fold.

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
    # the closed certified-arithmetic ladder, tightest first; the enum value names the rung the
    # `Certificate` carries and the receipt reads. `Arb`/`Mpmath` certify, `Numpy` is the sound
    # but uncertified outward-rounding band — the floor distinction is first-class, never a bool.
    ARB = "arb"
    MPMATH = "mpmath"
    NUMPY = "numpy"

    @property
    def certifies(self) -> bool:
        return self is not Floor.NUMPY


@runtime_checkable
class Expr(Protocol):
    # the interval extension: one callable that is inclusion-monotone over whatever ball/interval type
    # the resolving floor lifts (a `flint.arb`, an `mpmath.iv.mpf`, or the value-object `Interval` the
    # `Numpy` floor passes), so the same closure evaluates on every rung. Replaces the phantom
    # `expr: object` the prior page carried — a symbolic derivation lowers a
    # `sympy.lambdify(..., modules='mpmath')`/Arb closure to this shape.
    def over(self, ball: object, /) -> object: ...

    # the extension's STABLE content identity — a lowered-spec digest for a symbolic/jit-minted
    # derivation, canonical float64 coefficient bytes for a polynomial — so the op identity fold
    # names the expression; an anonymous closure with no key cannot enter the identity rail.
    def key(self) -> bytes: ...


@runtime_checkable
class Poly(Expr, Protocol):
    # a polynomial extension additionally exposing its `arb`-domain coefficient vector so the `Roots`
    # op feeds `arb_poly.real_roots` certified isolation rather than a hand-rolled Taylor scan; its
    # `key()` is the canonical contiguous float64 render of exactly these coefficients.
    def coeffs(self) -> Sequence[float]: ...


# --- [CONSTANTS] ---------------------------------------------------------------------------

_NUMPY_GRID: Final = 17  # the uncertified `Numpy` floor's `linspace` sample count over the box hull
_ULP: Final[float] = float(np.finfo(np.float64).eps)  # one relative double ulp: the `np.finfo` outward-pad scale
_TINY: Final[float] = float(np.finfo(np.float64).tiny)  # the absolute outward floor near zero, so a [0, 0] hull still widens


# --- [MODELS] ------------------------------------------------------------------------------


class Interval(Struct, frozen=True, gc=False):
    # the inclusion-monotone value object owning the relational algebra `flint.arb`/`mpmath.iv`
    # expose, so containment/hull/bisect are methods rather than re-derived per call site.
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
        # one polymorphic containment over a scalar point OR a sub-interval — the `arb.contains`
        # semantic, discriminated by the target shape rather than a `contains`/`contains_interval` pair.
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
        # the certified `arb.intersection`: `Nothing` when the intervals are disjoint, so a meet
        # that would invert (`lo > hi`) is the structural absence rather than a malformed interval.
        return Some(Interval(lo, hi)) if (lo := max(self.lo, other.lo)) <= (hi := min(self.hi, other.hi)) else Nothing

    def bisect(self) -> tuple["Interval", "Interval"]:
        return Interval(self.lo, self.mid), Interval(self.mid, self.hi)


class Certificate(Struct, frozen=True, gc=False):
    # replaces the lone `bool certified`: the producing `Floor` plus its accuracy evidence. A
    # `refuted` flag flips only through `Certify` when containment fails — the certification narrows.
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
        # `Certify`: keep the certificate when the interval brackets the target, refute it otherwise —
        # a refuted Arb/mpmath enclosure stays first-class evidence of a failed containment claim.
        return self if self.interval.contains(target) else Enclosure(self.interval, self.certificate.refute())


class IntervalReceipt(Struct, frozen=True):
    # the `ReceiptContributor` `run` returns on its rail — the receipt IS the contributor and the
    # rail IS the boundary, the `analysis/transform.md#TRANSFORM`/`numerics/statistics.md#STATISTICS`
    # egress form. `of` folds the yield total over both output shapes: a single `Enclosure` reads its
    # own width/floor, a `Roots` `tuple` folds to its widest (loosest-certified) member plus `roots`.
    op: Tag
    floor: Floor
    width: Width
    accuracy_bits: AccuracyBits
    certified: bool
    roots: int
    content_key: ContentKey

    @staticmethod
    def of(op: Tag, yielded: Yield, key: ContentKey, /) -> "IntervalReceipt":
        # an `Enclosure` `Struct` is not a `Sequence`, so the single-enclosure ops fall to the capture
        # arm while the `Roots` tuple folds to its widest (loosest-certified) member; an empty isolation
        # is a vacuous certified row carrying `roots=0` rather than a missing receipt.
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
        # the bounded native scalars the `is_recording()`-gated span batches through `set_attributes`;
        # the `content_key` hex rides the receipt facts only, not the OTLP attribute set.
        return {"floor": self.floor.value, "width": self.width, "accuracy_bits": self.accuracy_bits, "certified": self.certified, "roots": self.roots}

    def contribute(self) -> Iterable[Receipt]:
        # the runtime two-argument `Receipt.of(owner, evidence)` contract over the `(Phase, subject,
        # facts)` triple; native scalars ride the `dict[str, object]` `EventDict` the `enc_hook=repr`
        # renderer serializes without a coerce — never the four-positional form or a pre-`f""` map.
        facts: dict[str, object] = {**self.span_facts, "content_key": self.content_key.project("hex")}
        return (Receipt.of("compute.interval", ("emitted", self.op, facts)),)


# --- [OPERATIONS] --------------------------------------------------------------------------


@tagged_union(frozen=True)
class IntervalOp:
    # the certified-operation request; the tag selects the op and the case payload parameterizes the
    # input shape. The certified-output shape is the `Yield` union the dispatch fold returns and
    # `IntervalReceipt.of` collapses total — `run` returns `RuntimeRail[IntervalReceipt]` for every op,
    # so no phantom output type rides the carrier (a free `R` the `_dispatch` arms cannot prove they
    # satisfy is the unsound form; the `assert_never`-closed `match` is the dense and sound owner).
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
        # carries the real extension so `_bisect` re-evaluates `expr` over each half — the refined
        # enclosure stays certified by the same floor, never re-rounded by an identity expression.
        return IntervalOp(refine=(expr, enclosure, target, target_width, budget))

    @staticmethod
    def Roots(poly: Poly, box: Interval, /) -> "IntervalOp":
        return IntervalOp(roots=(poly, box))

    def identity_buffer(self, precision: int) -> bytes:
        # the FULL op identity, owned by the op: tag, the extension's stable `Expr.key()` (a lowered-spec
        # digest or canonical coefficient bytes — never an opaque closure id), every interval/enclosure
        # bound, the target, and every knob that changes the yield (`target_width`, `budget`, and
        # `precision` on the floor-evaluating arms; the input `Certificate` on `Certify`, whose output
        # carries it through) — so two different computations over one box never share a `ContentKey`.
        # Length-prefixed parts keep the buffer unambiguous; a new op case is one arm here.
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


# the ladder is data, not control flow: each row binds a `Floor` to its evaluator built behind the
# floor's gated import, and `_resolve_floor` keeps the tightest row whose import resolves through one
# `Block.choose`/`try_head` first-available fold — never three stacked `try/except ImportError: pass`
# so the fold needs no catch-all; the order Arb -> Mpmath -> Numpy is the tightness order.
class FloorRow(Struct, frozen=True):
    floor: Floor
    evaluate: Callable[[Expr, Interval, int], Enclosure]


def _arb_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    import flint

    ball = flint.arb(box.mid, box.rad)
    # `flint.good(func, prec=, maxprec=)` seeds the starting precision and drives it up adaptively
    # until the ball is accurate to `ctx.dps`, capped at `maxprec` so a non-convergent extension halts
    # rather than looping — the bounded form of the hand-written `ctx.prec` retry the prior page lacked.
    # Seeding through `prec=` keeps the working precision local to the call rather than leaking a session
    # `ctx.prec` mutation across evaluators. `rel_accuracy_bits`/`mid`/`rad` are the certified evidence.
    result = flint.good(lambda: expr.over(ball), prec=precision, maxprec=8 * precision)
    interval = Interval.around(float(result.mid()), float(result.rad()))
    return Enclosure(interval, Certificate(Floor.ARB, int(result.rel_accuracy_bits())))


def _mpmath_evaluate(expr: Expr, box: Interval, precision: int) -> Enclosure:
    import mpmath

    # the block-local `mpmath.workprec(precision)` manager scopes the working bit-precision and restores
    # it on exit, so the `iv`-context evaluation never leaks a session `prec` mutation across evaluators —
    # the precision-scoping discipline the `Arb`/`Roots` rungs hold. `iv.mpf([lo, hi])` lifts the box to
    # the inclusion-monotone interval whose `.a`/`.b` endpoints read back as the certified band.
    with mpmath.workprec(precision):
        result = expr.over(mpmath.iv.mpf([box.lo, box.hi]))
    return Enclosure(Interval(float(result.a), float(result.b)), Certificate(Floor.MPMATH, precision))


def _numpy_evaluate(expr: Expr, box: Interval, _precision: int) -> Enclosure:
    # numpy carries no interval type, so the uncertified floor evaluates the scalar `expr.over` on a
    # `linspace` grid spanning the box, hulls the samples, then pads the hull outward by one relative
    # `np.finfo(float64).eps` ulp (floored at `tiny` so a degenerate hull still widens) — a directed-
    # rounding emulation through the catalogued `finfo` metadata, since the branch numpy surface
    # carries no `nextafter` directed-step. Sound for a monotone extension, a heuristic band otherwise
    # (never the prior midpoint-collapse + `box.rad` that assumed a 1-Lipschitz `expr` and under-
    # enclosed a nonlinear one). The grid width and the outward `_ULP` pad are the float floor's rigor.
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
    # the `tailrec` bisection toward a width tolerance: bisect the interval, evaluate the certified
    # extension over each half, keep the half whose extension brackets the target (else the tighter
    # half), and recurse until width < tolerance or the budget is exhausted — a stack-safe fold, never
    # the prior single `Interval(lo, mid) if budget % 2` step that discarded budget, target, and bracket.
    if enclosure.width <= target_width or budget <= 0:
        return enclosure
    left, right = enclosure.interval.bisect()
    lo_enc, hi_enc = floor.evaluate(expr, left, precision), floor.evaluate(expr, right, precision)
    keep = lo_enc if lo_enc.interval.contains(target) or lo_enc.width <= hi_enc.width else hi_enc
    return TailCall(keep, expr, target, target_width, budget - 1, floor, precision)


def _roots(poly: Poly, box: Interval, precision: int) -> tuple[Enclosure, ...]:
    import flint

    # `arb_poly.real_roots` returns certified root-isolating balls — each `(mid, rad)` lifts to one
    # `Floor.Arb` enclosure, never a hand-rolled Taylor sign-change scan. The membership test is
    # `box.overlaps` over the certified ball interval, not a midpoint-only `contains`, so a root whose
    # enclosure straddles the box boundary is retained rather than dropped when its midpoint falls just
    # outside. `ctx.workprec` is the block-scoped precision the isolation needs (`real_roots` is not
    # `flint.good`-driven), restored on exit so no session `ctx.prec` mutation leaks across evaluators.
    with flint.ctx.workprec(precision):
        isolated = flint.arb_poly([flint.arb(c) for c in poly.coeffs()]).real_roots()
    enclosures = (Enclosure(Interval.around(float(r.mid()), float(r.rad())), Certificate(Floor.ARB, int(r.rel_accuracy_bits()))) for r in isolated)
    return tuple(enc for enc in enclosures if box.overlaps(enc.interval))


def _dispatch(op: IntervalOp, precision: int) -> Yield:
    # the one total `match` over the op tag closed by `assert_never`, returning the honest `Yield`
    # union the arms produce — `Roots` the only `tuple[Enclosure, ...]` arm, the rest one `Enclosure`.
    # `IntervalReceipt.of` folds that union total, so no phantom output type rides the dispatch.
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
    # the op OWNS its identity: `identity_buffer` folds the extension key, every bound, the target,
    # and the yield-changing knobs, so the key names the computation, never merely the box.
    # `ContentIdentity.of` defaults `CANONICAL_POLICY` and the `view="value"` projection that returns
    # `RuntimeRail[ContentKey]`, so a repeated op at identical precision keys by reference.
    return ContentIdentity.of(f"interval.{op.tag}", op.identity_buffer(precision))


@beartype(conf=FAULT_CONF)
def _report(op: IntervalOp, precision: int) -> IntervalReceipt:
    # the one `@beartype(conf=FAULT_CONF)`-fenced body returning a PLAIN `IntervalReceipt`: a `flint`/`mpmath`
    # raise, the gated `ImportError`, or a contract violation folds onto the rail through the enclosing
    # `boundary` fence rather than escaping. The railed `_keyed` digest is matched HERE inside the already-
    # fenced body — an `Error` re-raises onto the `boundary` whose `_convert` re-folds it (a `BoundaryFault`
    # is no exception) — so the impure floor solve and the pure content-key fold ride the one fence and the
    # entry mints exactly one `RuntimeRail[IntervalReceipt]`, never a double rail flattened through `.bind`.
    yielded = _dispatch(op, precision)
    match _keyed(op, precision):
        case Ok(key):
            return IntervalReceipt.of(op.tag, yielded, key)
        case Error(fault):
            raise RuntimeError(fault)


# --- [ENTRY] -------------------------------------------------------------------------------

# the interval family's DEFAULT graduation ceiling — a certified enclosure admits zero refutation
# and a finite width bound; the governed policy row per the hub ceiling law, caller-overridable.
_CEILING: Final[Map[str, float]] = Map.of_seq([("refuted", 0.0), ("width", 1e-6)])


class IntervalNumerics:
    # the one validated-arithmetic owner; `run` is the single entry every certified op enters through,
    # riding the hub `evidence_run` weave — the span opens from the `compute.interval` scope row (the
    # former `compute.rigor` literal disagreed with the owning module leaf, the deleted form), the
    # fence converts a flint/mpmath raise, and the fenced `@receipted(REDACTION)` harvest streams
    # `IntervalReceipt.contribute` — never a page-local tracer, never an inline `Signals.emit`.
    @staticmethod
    def run(op: IntervalOp, *, precision: int = 128) -> RuntimeRail[IntervalReceipt]:
        return evidence_run(EvidenceScope.INTERVAL, f"interval.{op.tag}", lambda: _report(op, precision))

    @staticmethod
    def graduates(receipt: IntervalReceipt, subject: str = "interval-certificate") -> "RuntimeRail[GraduationReceipt]":
        # interval proof evidence FEEDS the solver-axis projection on `solvers/receipt` with the
        # `(ledger, ceiling, key)` triple projected from its own Certificate — the one producer
        # pattern; the family ceiling row governs and a caller's tighter row overrides at the hub.
        ledger = {"refuted": 0.0 if receipt.certified else 1.0, "width": float(receipt.width)}
        return graduate("compute.interval", subject, receipt.content_key, ledger, dict(_CEILING.items()))
```
