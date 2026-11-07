"""Outline cleanup: near points merged, shallow joins smoothed, slivers dropped, needle spurs cut, and near coordinates snapped."""

lazy from collections.abc import Iterable
lazy import math
lazy from statistics import fmean

lazy from pathops import PathOp

lazy from rasm_typography._faces.glyphs import _Contour, _Outline, _Point
lazy from rasm_typography._outlines.measure import _stroke
lazy from rasm_typography._outlines.paths import _bounds, _Box, _combined, _pieces, _union

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [POINTS]


def _merged(contour: _Contour, merge: float, /) -> _Contour:
    """Drops each point within `merge` units of the last kept point, an on-curve point winning over an off-curve one."""
    held: list[_Point] = []
    for point in contour:
        if held and math.dist((held[-1].x, held[-1].y), (point.x, point.y)) < merge:
            held[-1] = held[-1] if held[-1].on or not point.on else point
        else:
            held.append(point)
    return tuple(held)


def _smoothed(contour: _Contour, corner: float, /) -> _Contour:
    """Drops each on-curve point between two lines or two arcs whose direction turns under `corner` degrees from the last kept point."""
    held: list[_Point] = []
    for point, after in zip(contour, (*contour[1:], contour[0]), strict=True):
        before = held[-1] if held else contour[-1]
        ax, ay, bx, by = point.x - before.x, point.y - before.y, after.x - point.x, after.y - point.y
        if not (point.on and before.on == after.on and abs(math.degrees(math.atan2(ax * by - ay * bx, ax * bx + ay * by))) < corner):
            held.append(point)
    return tuple(held)


def _tidied(outline: _Outline, /, *, merge: float, corner: float, sliver: float) -> _Outline:
    """Merges points under `merge` units apart, smooths joins turning under `corner` degrees, and drops pieces under `sliver` units of mean width."""
    kept = (piece for piece in _pieces(_union(tuple(_merged(contour, merge) for contour in outline))) if _stroke(piece) >= sliver)
    return tuple(smoothed for piece in kept for contour in piece if len(smoothed := _smoothed(contour, corner)) > 2)


# --- [CUTS]


def _despiked(outline: _Outline, band: tuple[float, float], /) -> _Outline:
    """Cuts at the band foot each outline piece under a join band `(low, high)` narrower than half the band, round overshoots kept."""
    low, high = band
    match _bounds(outline):
        case _Box(xmin=xmin, ymin=ymin, xmax=xmax) if ymin < low:
            under = _combined(outline, (_Box(xmin=xmin, ymin=ymin, xmax=xmax, ymax=low).contour,), PathOp.INTERSECTION)
            return _combined(outline, tuple(*piece for piece in _pieces(under) if (extent := _bounds(piece)) and extent.width < (high - low) / 2), PathOp.DIFFERENCE)
        case _:
            return _union(outline)


# --- [SNAPS]


def _levels(values: Iterable[float], tolerance: float, /) -> frozendict[float, float]:
    """Maps each value to the mean of its run, a run holding sorted values within `tolerance` of its first."""
    runs: list[list[float]] = []
    for value in sorted(set(values)):
        if runs and value - runs[-1][0] <= tolerance:
            runs[-1].append(value)
        else:
            runs.append([value])
    return frozendict({value: centre for run in runs for centre in (fmean(run),) for value in run})


def _snapped(outline: _Outline, tolerance: float, /) -> _Outline:
    """Moves coordinates within `tolerance` units of each other onto one shared value per axis, putting near-straight and aligned edges on one line."""
    points = tuple(*contour for contour in outline)
    xs, ys = _levels((point.x for point in points), tolerance), _levels((point.y for point in points), tolerance)
    return tuple(tuple(_Point(x=xs[point.x], y=ys[point.y], on=point.on) for point in contour) for contour in outline)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
