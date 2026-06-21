# [PY_COMPUTE_INTERVAL]

The one validated-numerics owner producing certified enclosures over a layered floor ladder. `IntervalNumerics` evaluates an interval extension over an input box, certifies that an enclosure contains a target, and refines an enclosure by bisection toward a width tolerance, all on one `IntervalOp` `@tagged_union` whose tag selects the operation and whose payload parameterizes both the input shape (a single box or a polynomial) and the certified-output shape. `Interval` is the inclusion-monotone `Meta`-bounded value object owning the full relational algebra (`contains`/`overlaps`/`hull`/`meet`/`bisect`/`mid`/`rad`) `flint.arb` and `mpmath.iv` expose, so containment and hull are value-object methods rather than re-derived per call. `Certificate` replaces the prior lone `bool certified` flag with the bounded `Floor` ladder member that produced the enclosure plus its `rel_accuracy_bits` evidence, so a receipt names *which* floor certified an enclosure and *how tight* the ball is, not merely that it did. `Floor` is the closed Arb→mpmath→numpy ladder vocabulary the `_FLOOR_LADDER` data table folds — the resolution is a `Block.choose`/`try_head` first-available fold over the table, never the three stacked `try/except ImportError: pass` blocks that smuggled import-failure into domain control flow.

## [01]-[INDEX]

- [01]-[ENCLOSURE]: certified interval evaluation, containment certification, and width-driven bisection refinement on one `IntervalNumerics` owner driven by the `_FLOOR_LADDER` data table (each row a `(Floor, evaluator)` pair), the `flint.arb`/`mpmath.iv`/`numpy` floor ladder resolved by a first-available `Block.choose` fold, the `flint.good` adaptive-precision driver and `arb_poly.real_roots` certified root isolation stacked into the evaluate and refine rails, content identity keyed through the runtime `ContentIdentity`, and the `@receipted` egress aspect.

## [02]-[ENCLOSURE]

- Owner: `IntervalNumerics` — the ONE validated-arithmetic owner; `Interval` the inclusion-monotone `[lo, hi]` value object with its relational algebra, `Enclosure` the interval plus a `Certificate`, `Floor` the closed `Arb`/`Mpmath`/`Numpy` ladder vocabulary, `Certificate` the floor-and-`rel_accuracy_bits` evidence the receipt reads, and `IntervalOp` the `@tagged_union` discriminating `Evaluate(expr, box)` (interval extension over a box through the tightest available floor), `Certify(enclosure, target)` (does the enclosure provably contain a target `Interval` or scalar), `Refine(expr, enclosure, target, target_width, budget)` (`tailrec` bisection re-evaluating `expr` over each half toward a width tolerance, retaining the half whose certified extension brackets the target), and `Roots(poly, box)` (certified `arb_poly.real_roots` isolation as a tuple of enclosures). Every certified operation is one tag on this owner over one `_dispatch` fold, never a parallel rigorous-arithmetic surface and never a per-tag `_*_evaluate` body.
- Floor ladder: `_FLOOR_LADDER` is the one ordered `Block[FloorRow]` table — each `FloorRow` carries the `Floor` member and the bound `(Expr, Interval, int) -> Enclosure` evaluator closing over the floor's package, built behind the floor's own gated import. `_resolve_floor` is the first-available fold (`Block.choose` lifting each row whose import resolves into `Some(row)`, `try_head` reading the tightest, no catch-all because the `Numpy` floor's import never fails on cp315), so the ladder is data-driven dispatch rather than three stacked `try/except ImportError: pass` blocks that erased the import failure into silent fall-through. The `Arb` evaluator lifts the box endpoints to a `flint.arb(mid, rad)` ball, drives `flint.good(lambda: expr.over(ball), prec=precision, maxprec=...)` so the seeded working precision rises adaptively until the ball is accurate rather than a fixed `ctx.prec` retry — seeding through `prec=` keeps precision local rather than leaking a session `ctx.prec` mutation — and reads `arb.mid`/`arb.rad`/`arb.rel_accuracy_bits` back as a `Floor.Arb` `Certificate`. `Roots`, not `flint.good`-driven, scopes its isolation under the block-local `flint.ctx.workprec(precision)` manager. The `Mpmath` evaluator lifts to an `mpmath.iv.mpf([lo, hi])` interval under the interval context's own `mpmath.iv.prec` bit-precision (distinct from the scalar `mp.prec`), evaluates `expr.over(interval)` through the inclusion-monotone `iv` context, and reads the endpoints back as a `Floor.Mpmath` `Certificate`. The `Numpy` evaluator evaluates the scalar `expr.over` on an `np.linspace` grid of `_NUMPY_GRID` points across the box, hulls the samples, and rounds outward through `np.nextafter` — numpy carries no interval type, so the floor brackets through a grid rather than the prior midpoint-collapse that assumed a 1-Lipschitz `expr` and under-enclosed a nonlinear one; the band is sound for a monotone extension and a heuristic enclosure otherwise. `Evaluate` always resolves a floor and never returns `Error(import_)`.
- Operation fold: `_dispatch` is the one total `match` over `IntervalOp` closed by `assert_never` — `Evaluate` resolves the ladder and runs the floor evaluator, `Certify` reseats the enclosure's `Certificate` to `Refuted` when `Interval.contains` fails (the certification narrows, never widens), `Refine` runs the `tailrec` `_bisect` loop, and `Roots` isolates certified real roots. `IntervalOp[R]` parameterizes the certified-output shape on a phantom `R`: `Evaluate`/`Certify`/`Refine` fold to one `Enclosure`, `Roots` folds to a `tuple[Enclosure, ...]`, so `_dispatch[R](op: IntervalOp[R]) -> R` narrows the dispatched value on the op the caller constructed rather than a widened `Yield` union every arm re-matches, while `run[R: Yield]` folds that narrowed value through `IntervalReceipt.of` so the egress stays one uniform `RuntimeRail[IntervalReceipt]`.
- Refine: `_bisect` is the `expression` `tailrec` trampoline — it bisects the interval through `Interval.bisect`, re-evaluates the carried `Expr` over each half through the resolved floor, retains the half whose certified extension brackets the target (or the tighter half when neither does), and returns `TailCall` until the width falls under `target_width` or the integer `budget` is exhausted, so refinement is a stack-safe fold toward a tolerance rather than the prior single parity-keyed `Interval(lo, mid) if budget % 2` step that discarded the budget, the target, and root bracketing. `Refine` carries the real extension rather than an identity placeholder, so the refined half stays certified by the floor that produced it instead of being re-rounded; a `Roots`-isolated enclosure feeds straight back as the refine target.
- Entry: `IntervalNumerics.run(op, *, precision)` enters one `boundary(f"rigor.{op.tag}", ...)` and `.bind`-flattens the railed `_keyed`, so the floor-evaluation fence and the `RuntimeRail[ContentKey]` digest rail join on one `RuntimeRail[IntervalReceipt]` — the sibling `transform.md#TRANSFORM`/`statistics.md#STATISTICS` egress shape where the entry returns the railed receipt, the receipt the `ReceiptContributor` and the rail the boundary, never the bare `RuntimeRail[Enclosure | tuple[Enclosure, ...]]` the value-typed prior form returned and dropped the receipt. The box bytes key identity through the one runtime `ContentIdentity.of` over the canonical `(lo, hi)` buffer so a repeated evaluation at identical box and precision is a cache hit by reference, the railed `ContentKey` threaded through `Result.map` INTO `IntervalReceipt.of(op.tag, yielded, key)` rather than discarded by a `.map(lambda _key: result)` that dropped both the receipt and the digest fault. `run[R: Yield]` narrows the dispatched value on the op's phantom `R`, and `IntervalReceipt.of` folds that yield total over the single-`Enclosure` ops and the `Roots` tuple, so one receipt carries every op.
- Receipt: `IntervalReceipt.of(op_tag, yielded, key)` folds the dispatched `Yield` total over both output shapes — a single `Enclosure` reads its own width/floor/`accuracy_bits`, a `Roots` tuple folds to its widest (loosest-certified) member plus the `roots` count, an empty tuple a vacuous certified row — so one receipt carries every op without a per-tag carrier. `contribute` returns the one-element `tuple[Receipt, ...]` the runtime `ReceiptContributor` port streams — `Receipt.of("compute.rigor", ("emitted", op_tag, facts))` against the runtime two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return against the `Iterable[Receipt]` port. The facts ride the width, the `Floor` tag, the `rel_accuracy_bits`, the `roots` count, and the content-key hex as native scalars the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without an `f""`/`str()` coerce. A `Floor.Arb` or `Floor.Mpmath` enclosure graduates as a proof `graduation/handoff.md#GRADUATION` admits on the `solver` axis; a `Floor.Numpy` band graduates only as advisory evidence, the floor distinction first-class on the `Certificate` rather than a bare boolean.
- Packages: `flint` (`arb`/`arb(mid, rad)`, `arb.mid`/`arb.rad`/`arb.rel_accuracy_bits` the ball endpoints and accuracy evidence, `arb.contains`/`arb.overlaps`/`arb.union`/`arb.intersection`/`arb.is_finite` the relational algebra the `Interval` value-object methods mirror, `arb_poly`/`arb_poly.real_roots` certified root isolation, `flint.good(func, prec=, maxprec=)` the seeded adaptive-precision driver and `flint.ctx.workprec` the block-local precision scope for the `Roots` isolation — python-flint Arb ball arithmetic), `mpmath` (`iv.mpf` the interval lift, `iv.prec` the interval-context bit-precision — the arbitrary-precision interval-context floor), `numpy` (`linspace`/`array`/`min`/`max` the `Numpy`-floor grid hull, `nextafter` the outward-rounding band over the hull endpoints, `ascontiguousarray`/`float64` the canonical content-identity buffer), `expression` (`tagged_union`/`tag`/`case` the `IntervalOp` ADT, `Block.of_seq`/`Block.choose`/`Block.try_head` the floor-ladder first-available fold, `tailrec`/`TailCall` the bisection trampoline, `Result.bind`/`Result.map` the rail join, `Option`/`Some`/`Nothing` the floor-resolution and root-bracket folds), `msgspec` (`Struct`, `Meta` the `Interval`/`Certificate` bounds), `beartype` (`FrozenDict` the `_FLOOR_LADDER` keyed view), `numerics/array.md#PAYLOAD` (a box admitting from an `ArrayPayload` keys through the same `ContentIdentity.of` seed), runtime (`RuntimeRail`/`boundary` from `runtime/faults`, `ContentIdentity`/`ContentKey`/`IdentityPolicy` from `runtime/content_identity`, `Receipt`/`ReceiptContributor` from `runtime/receipts`).
- Growth: a new certified operation is one `IntervalOp` case plus one `_dispatch` arm; a new floor is one `Floor` member plus one `_FLOOR_LADDER` row plus one `Certificate` arm; a new relational op is one `Interval` method; a new evidence slot is one column on `IntervalReceipt`; zero new surface, never a parallel rigorous-arithmetic struct, never a per-floor `_*_evaluate` body, never a `try/except` ladder parallel to the `_FLOOR_LADDER` fold.
- Boundary: classical validated numerics only — interval arithmetic, certified enclosures, containment certification, bisection refinement, and certified polynomial-root isolation are in-scope. Columnar/gridded interval aggregation defers to the `data` branch; the JIT loop-kernel wrap stays on `numerics/jit.md#JIT`; the symbolic derivation that *produces* an `Expr` stays on `sympy` and graduates the lowered closure here, never re-deriving the algebra. `flint` carries no cp315 wheel, so the `Arb` floor is authored against the documented API and is the tightest ladder rung where the wheel resolves; `mpmath` is pure-Python and cp315-clean, so the `Mpmath` `iv`-context floor is the always-on tight certified rung beneath the gated `Arb` rung; the `Numpy` `nextafter` band is the last-resort uncertified rung, so this owner — unlike the no-floor `statistics.md#STATISTICS`/`program.md#PROGRAM` — always resolves a reachable floor. The deleted forms: a four-positional `Receipt.of` and a pre-`f""`-formatted `dict[str, str]` facts map where the two-argument factory carries native scalars; a bare `bool certified` flag where the `Certificate` names the producing `Floor` and `rel_accuracy_bits`; a phantom `expr: object` where the `Expr` `Protocol` types the extension; a `try/except ImportError: pass` ladder where the `_FLOOR_LADDER` `Block.choose` fold resolves the tightest rung; a parity-keyed single-step bisection where `_bisect` `tailrec`s toward a width tolerance with root bracketing; a hand-rolled precision-retry loop and Taylor root scan where `flint.good`/`arb_poly.real_roots` adapt and certify; a midpoint-collapse `Numpy` floor that under-encloses a nonlinear `expr` where the grid hull rounds outward; a `run` discarding the `IntervalReceipt` through `.map(lambda _key: result)` where the entry threads the key into `IntervalReceipt.of`; and an inline `Signals.emit` where `@receipted` owns egress.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import Annotated, Literal, Protocol, Self, assert_never, runtime_checkable

import numpy as np
from beartype import FrozenDict
from expression import Nothing, Option, Some, TailCall, case, tag, tagged_union, tailrec
from expression.collections import Block
from msgspec import Meta, Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
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


@runtime_checkable
class Poly(Expr, Protocol):
    # a polynomial extension additionally exposing its `arb`-domain coefficient vector so the `Roots`
    # op feeds `arb_poly.real_roots` certified isolation rather than a hand-rolled Taylor scan.
    def coeffs(self) -> Sequence[float]: ...


# --- [CONSTANTS] ---------------------------------------------------------------------------

_NUMPY_GRID = 17  # the uncertified `Numpy` floor's `linspace` sample count over the box hull


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
    # rail IS the boundary, the `transform.md#TRANSFORM`/`statistics.md#STATISTICS` egress form. `of`
    # folds the yield total over both output shapes: a single `Enclosure` reads its own width/floor,
    # a `Roots` `tuple` folds to its widest (loosest-certified) member plus the `roots` count.
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

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime two-argument `Receipt.of(owner, evidence)` contract over the `(Phase, subject,
        # facts)` triple; native scalars ride the `dict[str, object]` `EventDict` the `enc_hook=repr`
        # renderer serializes without a coerce — never the four-positional form or a pre-`f""` map.
        facts: dict[str, object] = {
            "floor": self.floor.value,
            "width": self.width,
            "accuracy_bits": self.accuracy_bits,
            "certified": self.certified,
            "roots": self.roots,
            "content_key": self.content_key.project("hex"),
        }
        return (Receipt.of("compute.rigor", ("emitted", self.op, facts)),)


# --- [OPERATIONS] --------------------------------------------------------------------------

@tagged_union(frozen=True)
class IntervalOp[R]:
    # `R` is the phantom certified-output type — `Enclosure` for the single-enclosure ops, a
    # `tuple[Enclosure, ...]` for `Roots` — so `_dispatch[R](op: IntervalOp[R]) -> R` narrows the
    # dispatched value on the op the caller constructed before `run` folds it into one `IntervalReceipt`.
    tag: Tag = tag()
    evaluate: tuple[Expr, Interval] = case()
    certify: tuple[Enclosure, Target] = case()
    refine: tuple[Expr, Enclosure, Target, Width, int] = case()
    roots: tuple[Poly, Interval] = case()

    @staticmethod
    def Evaluate(expr: Expr, box: Interval, /) -> "IntervalOp[Enclosure]":
        return IntervalOp(evaluate=(expr, box))

    @staticmethod
    def Certify(enclosure: Enclosure, target: Target, /) -> "IntervalOp[Enclosure]":
        return IntervalOp(certify=(enclosure, target))

    @staticmethod
    def Refine(expr: Expr, enclosure: Enclosure, target: Target, target_width: Width, budget: int = 64, /) -> "IntervalOp[Enclosure]":
        # carries the real extension so `_bisect` re-evaluates `expr` over each half — the refined
        # enclosure stays certified by the same floor, never re-rounded by an identity expression.
        return IntervalOp(refine=(expr, enclosure, target, target_width, budget))

    @staticmethod
    def Roots(poly: Poly, box: Interval, /) -> "IntervalOp[tuple[Enclosure, ...]]":
        return IntervalOp(roots=(poly, box))

    @property
    def box(self) -> Interval:
        # the interval each op keys identity over, projected total over the tag — the `Certify`/`Refine`
        # ops key over the enclosure's own interval, closed by `assert_never`.
        match self:
            case IntervalOp(tag="evaluate", evaluate=(_, box)) | IntervalOp(tag="roots", roots=(_, box)):
                return box
            case IntervalOp(tag="certify", certify=(enclosure, _)) | IntervalOp(tag="refine", refine=(_, enclosure, _, _, _)):
                return enclosure.interval
            case unreachable:
                assert_never(unreachable)


# --- [TABLES] ------------------------------------------------------------------------------

# the ladder is data, not control flow: each row binds a `Floor` to its evaluator built behind the
# floor's gated import, and `_resolve_floor` keeps the tightest row whose import resolves through one
# `Block.choose`/`try_head` first-available fold — never three stacked `try/except ImportError: pass`
# blocks that erase the import failure into silent fall-through. The `Numpy` row never fails on cp315,
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

    # the `iv` interval context carries its OWN bit-precision, distinct from the scalar `mp` context —
    # setting `mp.prec` would not configure the interval evaluation, so the precision binds on `iv.prec`.
    mpmath.iv.prec = precision
    result = expr.over(mpmath.iv.mpf([box.lo, box.hi]))
    return Enclosure(Interval(float(result.a), float(result.b)), Certificate(Floor.MPMATH, precision))


def _numpy_evaluate(expr: Expr, box: Interval, _precision: int) -> Enclosure:
    # numpy carries no interval type, so the uncertified floor evaluates the scalar `expr.over` on a
    # `linspace` grid spanning the box and hulls the samples, then rounds the hull outward through
    # `np.nextafter` — sound for a monotone extension, a heuristic band otherwise (never the prior
    # midpoint-collapse + `box.rad` that assumed a 1-Lipschitz `expr` and under-enclosed a nonlinear
    # one). The grid width and the outward `nextafter` are the only rigor the float floor admits.
    samples = np.array([float(expr.over(float(x))) for x in np.linspace(box.lo, box.hi, _NUMPY_GRID)], dtype=np.float64)
    interval = Interval(float(np.nextafter(samples.min(), -np.inf)), float(np.nextafter(samples.max(), np.inf)))
    return Enclosure(interval, Certificate(Floor.NUMPY))


_FLOOR_LADDER: FrozenDict[Floor, FloorRow] = FrozenDict({
    Floor.ARB: FloorRow(Floor.ARB, _arb_evaluate),
    Floor.MPMATH: FloorRow(Floor.MPMATH, _mpmath_evaluate),
    Floor.NUMPY: FloorRow(Floor.NUMPY, _numpy_evaluate),
})


def _importable(floor: Floor) -> bool:
    from importlib.util import find_spec

    # `numpy` always resolves on cp315 (the reachable floor); `flint`/`mpmath` resolve only where the
    # wheel is present, so the ladder fold reads availability without importing the heavy extension.
    return floor is Floor.NUMPY or find_spec(floor.value) is not None


def _resolve_floor() -> FloorRow:
    rows = Block.of_seq(_FLOOR_LADDER.values())
    return rows.choose(lambda row: Some(row) if _importable(row.floor) else Nothing).try_head().default_value(
        _FLOOR_LADDER[Floor.NUMPY]
    )


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
    enclosures = (
        Enclosure(Interval.around(float(r.mid()), float(r.rad())), Certificate(Floor.ARB, int(r.rel_accuracy_bits())))
        for r in isolated
    )
    return tuple(enc for enc in enclosures if box.overlaps(enc.interval))


def _dispatch[R](op: IntervalOp[R], precision: int) -> R:
    # the one total fold over the op tag, the return narrowing on the phantom `R` each factory pins;
    # `Roots` is the only `tuple[Enclosure, ...]` arm, the rest fold one `Enclosure`.
    match op:
        case IntervalOp(tag="evaluate", evaluate=(expr, box)):
            return _resolve_floor().evaluate(expr, box, precision)
        case IntervalOp(tag="certify", certify=(enclosure, target)):
            return enclosure.recertify(target)
        case IntervalOp(tag="refine", refine=(expr, enclosure, target, target_width, budget)):
            return _bisect(enclosure, expr, target, target_width, budget, _resolve_floor(), precision)
        case IntervalOp(tag="roots", roots=(poly, box)):
            return _roots(poly, box, precision)
        case unreachable:
            assert_never(unreachable)


def _keyed[R](op: IntervalOp[R]) -> RuntimeRail[ContentKey]:
    # the phantom `R` flows through unread so the box-buffer key stays variance-safe over the op the
    # caller built, never a widening `IntervalOp[object]` cast; `ContentIdentity.of` defaults the
    # canonical `IdentityPolicy` and the `view="value"` projection that returns `RuntimeRail[ContentKey]`.
    buffer = np.ascontiguousarray([op.box.lo, op.box.hi], dtype=np.float64).tobytes()
    return ContentIdentity.of(f"rigor.{op.tag}", buffer, IdentityPolicy())


def run[R: Yield](op: IntervalOp[R], *, precision: int = 128) -> RuntimeRail[IntervalReceipt]:
    # one entry over the op's phantom yield `R: Yield`: the floor-evaluation fence and the railed
    # `ContentIdentity.of` digest join on one rail through `.bind`/`.map`, the digest threaded INTO
    # `IntervalReceipt.of` rather than discarded — the sibling `transform.md`/`statistics.md` shape
    # where the entry returns `RuntimeRail[<Receipt>]`, the receipt the `ReceiptContributor` and the
    # rail the boundary. `IntervalReceipt.of` folds the yield total over `Enclosure` and the `Roots`
    # tuple, so the `R` phantom narrows the dispatched value while one receipt carries every op. The
    # study spine harvests this `Ok`-arm contributor through `@receipted`, never an inline emit here.
    return boundary(f"rigor.{op.tag}", lambda: _dispatch(op, precision)).bind(
        lambda yielded: _keyed(op).map(lambda key: IntervalReceipt.of(op.tag, yielded, key))
    )
```

## [03]-[RESEARCH]

- [FLOOR_LADDER]: the prior three stacked `try/except ImportError: pass` blocks were exception control flow inside domain logic — an import failure smuggled into silent fall-through. The ladder is now the `_FLOOR_LADDER` `FrozenDict[Floor, FloorRow]` table whose `_resolve_floor` keeps the tightest available rung through one `Block.choose`/`try_head` first-available `Option` fold over `importlib.util.find_spec` availability, the `Numpy` rung the reachable cp315 floor that needs no catch-all (`default_value(_FLOOR_LADDER[Floor.NUMPY])` anchors totality). The `Arb`/`Mpmath` evaluators import behind the row body so the heavy `flint`/`mpmath` extensions load only on the rung the fold selects, and the `Floor` member rides the `Certificate` so the receipt names which rung certified an enclosure — the deliberate floor asymmetry: unlike the no-floor `statistics.md#STATISTICS`/`optimization/program.md#PROGRAM` owners that return `Error(import_)`, this owner always resolves the `Numpy` band.
- [FLINT_ARB]: `flint` (python-flint) carries no cp315 wheel; the `flint.arb(mid, rad)`/`arb.mid`/`arb.rad`/`arb.rel_accuracy_bits`/`arb.contains`/`flint.good`/`flint.ctx.workprec`/`arb_poly.real_roots` spellings are catalogued in `compute/.api/python-flint.md` (ball-arithmetic public types [02], `arb` operations [03] rows [01]/[12], context-and-precision [03] `workprec` row [07], adaptive-eval [03] `flint.good(func, prec=, maxprec=)`, polynomial `real_roots`) and verify against the present catalogue. `flint.good(func, prec=, maxprec=)` is the adaptive-precision driver the prior fixed-`ctx.prec` path lacked — it seeds the start precision through `prec=` (no session `ctx.prec` mutation to leak) and re-runs the extension at escalating precision until the ball is accurate to `ctx.dps`, the `maxprec=8 * precision` cap halting a non-convergent extension rather than looping unbounded, replacing a hand-written retry loop; the `Roots` isolation, not `flint.good`-driven, scopes precision under the block-local `flint.ctx.workprec(precision)` manager that restores the prior precision on exit; `arb_poly.real_roots` is the certified root-isolation the prior page had no operation for, now the `Roots` case feeding `_bisect`-seeded refinement onto a certified bracket rather than a midpoint guess, with the box-membership test `box.overlaps(enc.interval)` over the certified ball rather than a midpoint-only `contains` that would drop a root straddling the boundary. The Arb rung is the tightest ladder rung where the wheel resolves.
- [MPMATH_INTERVAL]: `mpmath` resolves on the cp315 core (pure-Python, cp315-clean). The prior page used `mpmath.mpi` (the bare interval type) but never the `iv` *context* — the catalogue (`compute/.api/mpmath.md` MPMATH_TOPOLOGY [04], LOCAL_ADMISSION interval-arithmetic row) documents `iv` as the interval-arithmetic context whose elementary/special functions are bound methods, so the extension evaluates through `mpmath.iv.mpf([lo, hi])` under the `iv` context where every special function rigorously bounds absolute error, rather than the scalar `mpi` constructor alone. The `Mpmath` rung is the always-on tight certified floor beneath the gated `Arb` rung.
- [VALUE_OBJECT_ALGEBRA]: `Interval` collapses the relational operations `flint.arb` (`contains`/`overlaps`/`union`/`intersection`, python-flint `arb` ops [03][12]) and `mpmath.iv` expose into value-object methods — `contains` polymorphic over a scalar point OR a sub-`Interval` target (one method, not a `contains`/`contains_interval` pair), `hull` the certified `arb.union`, `meet` the `arb.intersection` returning `Option[Interval]` so a disjoint meet is the structural `Nothing` rather than an inverted interval, and `bisect` the refinement primitive. `Width`/`AccuracyBits` are `Meta`-bounded (`ge=0.0`/`ge=0`) so a negative width or accuracy is a decode-time validation fault, and the leaf value objects opt `gc=False` (container-free) per the branch leaf rule.
- [CERTIFICATE]: the prior lone `bool certified` flag could not distinguish a tight Arb ball from a loose numpy band or name the failure of a `Certify`. `Certificate` carries the producing `Floor` plus `rel_accuracy_bits` and a `refuted` flag that flips only through `recertify` when `Interval.contains` fails — so `certified` is the derived `floor.certifies and not refuted` predicate, an Arb/mpmath enclosure that fails a containment claim stays first-class refuted evidence, and the receipt's graduation eligibility reads the floor (`Arb`/`Mpmath` graduate as proof, `Numpy` as advisory) rather than a boolean collapsing every cause.
- [OUTPUT_PARAMETERIZATION]: `IntervalOp[R]` parameterizes the certified-output shape as well as the input — `Evaluate`/`Certify`/`Refine` pin `R = Enclosure`, `Roots` pins `R = tuple[Enclosure, ...]` — carried statically by the PEP 695 phantom type parameter so `_dispatch[R](op: IntervalOp[R]) -> R` narrows the dispatched value on the op the caller built rather than re-matching the `Yield = Enclosure | tuple[Enclosure, ...]` union, denser than the `@overload`-per-disposition shape `runtime/reliability/faults#FAULT` `traversed` holds (whose `Disposition` is a runtime value, not a phantom on the carrier) because the type rides the op itself. The PEP 695 bound `run[R: Yield]` keeps the entry's surface uniform — every op returns `RuntimeRail[IntervalReceipt]` — while the phantom narrows the value the fence dispatches and `IntervalReceipt.of` folds that yield total over both shapes through one structural `match`, so the output parameterization lives on the carrier and the egress stays one rail. The `Refine` op `tailrec`s through `_bisect` toward a `Width` tolerance, re-evaluating the carried `Expr` over each half and retaining the root-bracketing one, a stack-safe fold replacing the prior parity-keyed single step.
- [CONTENT_KEY]: `_keyed` derives the railed `RuntimeRail[ContentKey]` over the canonical contiguous `(lo, hi)` `float64` box buffer through `ContentIdentity.of("rigor.<tag>", ...)` under the runtime `IdentityPolicy`, whose `view="value"` default returns `RuntimeRail[ContentKey]`; `run` threads that rail through `Result.map` so a digest fault propagates on the one rail and the fence joins it under `.bind` — the sibling `optimization/program.md#PROGRAM` and `numerics/array.md#PAYLOAD` key algebra, where a box admitting through `numerics/array.md#PAYLOAD` keys under the same seed and a repeated evaluation at identical box and precision is a cache hit by reference, fixing the prior page's complete absence of content identity.
- [RECEIPT]: `run` returns `RuntimeRail[IntervalReceipt]`, threading the railed `ContentKey` INTO `IntervalReceipt.of(op.tag, yielded, key)` through `.map` — fixing the prior `.map(lambda _key: result)` that discarded BOTH the receipt model and the digest, returning the bare value-typed `RuntimeRail[Enclosure | tuple[...]]` and leaving `IntervalReceipt` a dead model the entry never minted. `IntervalReceipt.of` folds the dispatched `Yield` total over both output shapes through one structural `match`: a single `Enclosure` reads its own width/floor/`accuracy_bits`, a `Roots` tuple folds to its widest (loosest-certified) member plus the `roots` count, an empty tuple a vacuous certified row. `contribute` mints through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("compute.rigor", ("emitted", op_tag, facts))`, the `(Phase, subject, facts)` triple the runtime factory discriminates — returning the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port declares, never the four-positional `Receipt.of("emitted", "compute.rigor", "enclosure", facts)` the prior page carried (the exact deleted form the runtime owner and every dense sibling reject) and never a single-`Receipt` return. The facts ride the `Floor` tag, width, `rel_accuracy_bits`, certified flag, `roots` count, and content-key hex as native `str`/`float`/`int`/`bool` through the runtime `Signals` `Encoder(enc_hook=repr, order="deterministic")` rather than the prior `f"{width:.3e}"`/`str(certified)` pre-coerce. The resolved `IntervalReceipt` is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm — the same convention `analysis/transform.md#TRANSFORM`/`analysis/signal.md#DSP` hold, the receipt the contributor and the rail the boundary form — never an inline `Signals.emit` threaded through this body.
