# [PY_COMPUTE_ENCLOSURE]

The one validated-numerics owner producing certified enclosures. `IntervalNumerics` evaluates an expression over an input box, certifies that an enclosure contains a target, and refines an interval by bisection, with three layered floors: python-flint Arb ball arithmetic for a certified midpoint-radius enclosure, an mpmath arbitrary-precision interval floor that runs on cp315 where the Arb wheel is unavailable, and a numpy outward-rounding band as the last resort. `Interval` is the inclusion-monotone value object and `Enclosure` carries the interval plus a `certified` flag and a width. The mpmath floor is the modern always-on tight floor that replaces a coarse double-precision band.

## [1]-[INDEX]

[ENCLOSURE]: interval/ball arithmetic, certified enclosures, and the Arb/mpmath/numpy floor ladder on one `IntervalNumerics` owner.

## [2]-[ENCLOSURE]

- Owner: `IntervalNumerics` — the ONE validated-arithmetic owner; `Interval` is the inclusion-monotone `[lo, hi]` value object, `Enclosure` carries the interval plus a `certified` flag, and `IntervalOp` discriminates `Evaluate(expr, box)` (interval extension over an input box), `Certify(enclosure, target)` (does the enclosure provably contain the target), and `Refine(interval, bisections)` (interval bisection to a width tolerance). Every certified operation is a row on this owner, never a parallel rigorous-arithmetic surface.
- Floor ladder: `_certified_evaluate` resolves the tightest available floor. With the Arb wheel present it lifts each input to a `flint.arb(mid, rad)` ball, evaluates through Arb ball arithmetic at the requested precision, and reads `arb.mid()`/`arb.rad()` back as a certified enclosure. Without Arb but with mpmath it lifts to an `mpmath.mpi` interval at `mpmath.mp.prec`, evaluates through the inclusion-monotone interval arithmetic, and reads the interval endpoints back as a certified enclosure on cp315. Without either it falls to the numpy floor, evaluating at the box midpoint and rounding the bounds outward through `np.nextafter` for a sound but uncertified band. The `evaluate` op always has a reachable floor and never returns `Error(Import)`.
- Entry: `IntervalNumerics.evaluate` returns `RuntimeRail[Enclosure]`; the `certify` op tightens the certified flag by the containment test, and the `refine` op bisects the interval toward the target width.
- Receipt: `Enclosure.contribute` emits one `Receipt.of("emitted", ...)` row carrying the width and the `certified` flag; a certified Arb or mpmath enclosure graduates as a proof the C# gate admits, an uncertified numpy band graduates only as advisory evidence.
- Packages: `flint` (`arb`, `arb.mid`, `arb.rad`, `ctx.prec` — python-flint Arb ball arithmetic), `mpmath` (`mpi`, `mp.prec`, `mpf` — arbitrary-precision interval floor), `numpy` (`nextafter`, `spacing`, `finfo`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new certified operation is one `IntervalOp` case; a new rounding policy is one branch in the floor ladder; zero new surface.
- Boundary: classical validated numerics only — interval arithmetic, certified enclosures, and bisection refinement are in-scope. `flint` carries no cp315 wheel, so the Arb branch is authored against the documented API; `mpmath` is pure-Python and cp315-clean, so the mpmath interval floor runs unconditionally on cp315 and gives a tight certified enclosure where Arb is unavailable; the numpy `nextafter` band is the last-resort uncertified floor.

```python signature
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class Interval(Struct, frozen=True):
    lo: float
    hi: float

    @property
    def width(self) -> float:
        return self.hi - self.lo

    def contains(self, target: float) -> bool:
        return self.lo <= target <= self.hi

    @staticmethod
    def around(mid: float, rad: float) -> "Interval":
        return Interval(mid - rad, mid + rad)


class Enclosure(Struct, frozen=True):
    interval: Interval
    certified: bool

    def contribute(self) -> Receipt:
        facts = {"width": f"{self.interval.width:.3e}", "certified": str(self.certified)}
        return Receipt.of("emitted", "compute.rigor", "enclosure", facts)


@tagged_union(frozen=True)
class IntervalOp:
    tag: Literal["evaluate", "certify", "refine"] = tag()
    evaluate: tuple[object, Interval] = case()
    certify: tuple[Enclosure, float] = case()
    refine: tuple[Interval, int] = case()

    @staticmethod
    def Evaluate(expr: object, box: Interval) -> "IntervalOp":
        return IntervalOp(evaluate=(expr, box))

    @staticmethod
    def Certify(enclosure: Enclosure, target: float) -> "IntervalOp":
        return IntervalOp(certify=(enclosure, target))

    @staticmethod
    def Refine(interval: Interval, bisections: int) -> "IntervalOp":
        return IntervalOp(refine=(interval, bisections))


def evaluate(op: IntervalOp, *, precision: int = 128) -> "RuntimeRail[Enclosure]":
    return boundary(f"rigor.{op.tag}", lambda: _evaluate(op, precision))


def _evaluate(op: IntervalOp, precision: int) -> Enclosure:
    match op:
        case IntervalOp(tag="evaluate", evaluate=(expr, box)):
            return _certified_evaluate(expr, box, precision)
        case IntervalOp(tag="certify", certify=(enclosure, target)):
            return Enclosure(enclosure.interval, enclosure.certified and enclosure.interval.contains(target))
        case IntervalOp(tag="refine", refine=(interval, budget)):
            mid = 0.5 * (interval.lo + interval.hi)
            half = Interval(interval.lo, mid) if budget % 2 else Interval(mid, interval.hi)
            return Enclosure(half, False)
        case unreachable:
            assert_never(unreachable)


def _certified_evaluate(expr: object, box: Interval, precision: int) -> Enclosure:
    mid_in, rad_in = 0.5 * (box.lo + box.hi), 0.5 * box.width
    try:
        import flint

        flint.ctx.prec = precision
        result = expr(flint.arb(mid_in, rad_in))
        return Enclosure(Interval.around(float(result.mid()), float(result.rad())), certified=True)
    except ImportError:
        pass
    try:
        import mpmath

        mpmath.mp.prec = precision
        result = expr(mpmath.mpi(box.lo, box.hi))
        return Enclosure(Interval(float(result.a), float(result.b)), certified=True)
    except ImportError:
        out = float(expr(mid_in))
        return _floor_enclosure(out, rad_in + float(np.spacing(abs(out))))


def _floor_enclosure(mid: float, rad: float) -> Enclosure:
    lo = float(np.nextafter(mid - rad, -np.inf))
    hi = float(np.nextafter(mid + rad, np.inf))
    return Enclosure(Interval(lo, hi), certified=False)
```

## [3]-[RESEARCH]

- [FLINT_ARB]: `flint` (python-flint) carries no cp315 wheel; the `flint.arb`/`arb.mid`/`arb.rad`/`flint.ctx.prec` ball-arithmetic spellings are authored against the documented Arb API and verify against the `.api` catalogue once the python-flint wheel resolves.
- [MPMATH_INTERVAL]: `mpmath` resolves on the cp315 core (pure-Python, cp315-clean). The `mpmath.mpi`/`mpmath.mp.prec`/`mpf` interval spellings verify against the `.api` catalogue under a uv-sync reflection pass. The mpmath interval floor is the always-on tight certified floor on cp315 beneath the gated Arb path.
