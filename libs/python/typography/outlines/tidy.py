"""Outline repair: near points merged, shallow joins smoothed, slivers dropped, needle spurs cut and near coordinates snapped."""

lazy from collections.abc import Iterable
lazy from functools import reduce
lazy import math
lazy from statistics import fmean

lazy from typography.faces.glyphs import Contour, Outline, Point
lazy from typography.outlines.measure import stroke
lazy from typography.outlines.paths import bounds, Box, combined, Operation, pieces, union

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [POINTS]


def _merged(contour: Contour, merge: float, /) -> Contour:
    """Drops each point within `merge` units of the last kept point, an on-curve point winning over an off-curve one."""

    def kept(held: Contour, point: Point) -> Contour:
        match held:
            case (*rest, last) if math.dist((last.x, last.y), (point.x, point.y)) < merge:
                return (*rest, last if last.on or not point.on else point)
            case _:
                return (*held, point)

    initial: Contour = ()
    return reduce(kept, contour, initial)


def _smoothed(contour: Contour, corner: float, /) -> Contour:
    """Drops each on-curve point between two lines or two arcs whose direction turns under `corner` degrees from the last kept point."""

    def kept(held: Contour, pair: tuple[Point, Point]) -> Contour:
        point, after = pair
        before = held[-1] if held else contour[-1]
        ax, ay, bx, by = point.x - before.x, point.y - before.y, after.x - point.x, after.y - point.y
        return held if point.on and before.on == after.on and abs(math.degrees(math.atan2(ax * by - ay * bx, ax * bx + ay * by))) < corner else (*held, point)

    initial: Contour = ()
    return reduce(kept, zip(contour, (*contour[1:], contour[0]), strict=True), initial)


def tidied(outline: Outline, /, *, merge: float, corner: float, sliver: float) -> Outline:
    """Merges points under `merge` units apart, smooths joins turning under `corner` degrees, and drops pieces under `sliver` units of mean width."""
    kept = (piece for piece in pieces(union(tuple(_merged(contour, merge) for contour in outline))) if stroke(piece) >= sliver)
    return tuple(smoothed for piece in kept for contour in piece if len(smoothed := _smoothed(contour, corner)) > 2)


# --- [CUTS]


def despiked(outline: Outline, band: tuple[float, float], /) -> Outline:
    """Cuts at the band foot each piece of ink under a join band `(low, high)` narrower than half the band, round overshoots kept."""
    low, high = band
    match bounds(outline):
        case Box(xmin=xmin, ymin=ymin, xmax=xmax) if ymin < low:
            under = combined(outline, (Box(xmin=xmin, ymin=ymin, xmax=xmax, ymax=low).contour,), Operation.INTERSECTION)
            return combined(outline, tuple(*piece for piece in pieces(under) if (extent := bounds(piece)) and extent.width < (high - low) / 2), Operation.DIFFERENCE)
        case _:
            return union(outline)


# --- [SNAPS]


def _levels(values: Iterable[float], tolerance: float, /) -> frozendict[float, float]:
    """Maps each value to the mean of its run, a run holding sorted values within `tolerance` of its first."""
    initial: tuple[tuple[float, ...], ...] = ()
    runs = reduce(lambda held, value: (*held[:-1], (*held[-1], value)) if held and value - held[-1][0] <= tolerance else (*held, (value,)), sorted(set(values)), initial)
    return frozendict({value: fmean(run) for run in runs for value in run})


def snapped(outline: Outline, tolerance: float, /) -> Outline:
    """Moves coordinates within `tolerance` units of each other onto one shared value per axis, so near-straight edges and aligned edges meet one line."""
    points = tuple(*contour for contour in outline)
    xs, ys = _levels((point.x for point in points), tolerance), _levels((point.y for point in points), tolerance)
    return tuple(tuple(Point(x=xs[point.x], y=ys[point.y], on=point.on) for point in contour) for contour in outline)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["despiked", "snapped", "tidied"]
