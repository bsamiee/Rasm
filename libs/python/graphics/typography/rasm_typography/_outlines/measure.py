"""Stroke width, perimeter, counter width, moments, signed gap, strip coverage and stroke-targeted offsets over quadratic outlines."""

# mypy: disable-error-code="no-untyped-call"

lazy from itertools import pairwise
lazy import math

import msgspec
lazy from fontTools.misc.bezierTools import segmentPointAtT
lazy from fontTools.pens.perimeterPen import PerimeterPen
lazy from fontTools.pens.statisticsPen import StatisticsPen
lazy from pathops import Path, PathOp
lazy from scipy.optimize import minimize_scalar
lazy from shapely import build_area, linestrings, maximum_inscribed_circle, MultiLineString, multilinestrings, MultiPolygon, Polygon

lazy from rasm_typography._faces.glyphs import _Outline
lazy from rasm_typography._outlines.paths import _area, _Axis, _bounds, _Box, _combined, _draw, _offset, _pieces, _segments, _transformed, _union

# --- [MODELS] ---------------------------------------------------------------------------


class _Moments(msgspec.Struct, frozen=True, kw_only=True):
    """Outline center of mass and principal axis direction in degrees within [-90, 90]."""

    centre: tuple[float, float]
    angle: float


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WIDTHS]


def perimeter(outline: _Outline, /) -> float:
    """Returns the edge length of the outline's union."""
    pen = PerimeterPen()
    _draw(pen, outline)
    return float(pen.value)


def _stroke(outline: _Outline, /) -> float:
    """Returns the mean stroke width as twice the area over the perimeter, 0 for the empty outline."""
    path, pen = Path(), PerimeterPen()
    _draw(path.getPen(), outline)
    path.draw(pen)
    return 2 * path.area / pen.value if pen.value else 0.0  # ty: ignore[unsound-return-statement]


def _restroked(outline: _Outline, pen: float, /) -> _Outline:
    """Returns a geometric offset locally fitting the requested mean stroke, an empty outline kept empty.

    Fits squared stroke error between the outline's erosion radius and outward growth by the target pen. Topology changes can leave the closest local fit short of the target.
    """
    match _bounds(outline):
        case None:
            return ()
        case _:
            fit = minimize_scalar(lambda distance: (_stroke(_offset(outline, distance)) - pen) ** 2, bounds=(-_inradius(outline), pen))
            return _offset(outline, float(fit.x))


def _counters(outline: _Outline, /) -> tuple[_Outline, ...]:
    """Returns bounded empty regions, retaining filled islands and nested holes with their original curves."""
    holes = _union(*((hole,) for _, *holes in _pieces(outline) for hole in holes))
    return _pieces(_combined(holes, outline, PathOp.DIFFERENCE))


def _counter(outline: _Outline, /) -> float:
    """Returns the smallest counter's largest inscribed diameter, 0 for an outline without holes."""
    return min((2 * _inradius(void) for void in _counters(outline)), default=0.0)


def _moments(outline: _Outline, /) -> _Moments:
    """Returns the center of mass and principal axis of the outline's filled area from its first and second moments."""
    pen = StatisticsPen()
    _draw(pen, outline)
    return _Moments(centre=(float(pen.meanX), float(pen.meanY)), angle=math.degrees(0.5 * math.atan2(2 * pen.covariance, pen.varianceX - pen.varianceY)))


# --- [DISTANCES]


def _flattened(outline: _Outline, /) -> tuple[tuple[tuple[float, float], ...], tuple[tuple[tuple[float, float], tuple[float, float]], ...]]:
    """Vertices and edges of the union flattened within a quarter unit, each quadratic cut into as many steps as the square root of its second difference length."""
    runs = tuple(
        tuple(segmentPointAtT(segment, step / steps) for step in range(steps + 1))
        for segment in _segments(outline)
        for steps in (max(1, math.ceil(math.sqrt(math.hypot(*(a - 2 * b + c for a, b, c in zip(*segment, strict=True)))))) if len(segment) == 3 else 1,)
    )
    return (tuple(*run for run in runs), tuple(*pairwise(run) for run in runs))  # ty: ignore[unsound-return-statement]


def _strung(edges: tuple[tuple[tuple[float, float], tuple[float, float]], ...], /) -> MultiLineString:
    """Edges as one geometry through the array constructors, an empty one for none."""
    return multilinestrings(linestrings(edges)) if edges else MultiLineString()  # type: ignore[return-value] # ty: ignore[invalid-return-type]


def _inradius(outline: _Outline, /) -> float:
    """Returns the largest inscribed circle's radius, 0 for edges enclosing no area, retaining holes and islands."""
    match build_area(_strung(_flattened(outline)[1])):
        case Polygon() | MultiPolygon() as filled:
            return float(maximum_inscribed_circle(filled, tolerance=0.25).length)
        case _:
            return 0.0


def _apart(point: tuple[float, float], start: tuple[float, float], end: tuple[float, float], /) -> float:
    """Vertical separation where a segment spans a point's abscissa, infinite elsewhere."""
    (px, py), (sx, sy), (dx, dy) = point, start, (end[0] - start[0], end[1] - start[1])
    return abs(py - sy - (px - sx) * dy / dx) if dx and min(sx, end[0]) <= px <= max(sx, end[0]) else math.inf


def _gap(a: _Outline, b: _Outline, /, axis: _Axis | None = None) -> float:
    """Returns the signed distance between two outlines, nearest or along an axis, negated as the overlap depth, infinite where they never face.

    Args:
        a: First outline.
        b: Second outline.
        axis: Axis the distance runs along, `None` for the nearest separation in any direction.

    Returns:
        The separation for disjoint outlines, else the negative radius of the largest circle inside their intersection.
    """
    match _combined(a, b, PathOp.INTERSECTION):
        case ():
            (points, lines), (others, edges) = (_flattened(_transformed(outline, (0.0, 1.0, 1.0, 0.0, 0.0, 0.0)) if axis == "x" else outline) for outline in (a, b))
            return (
                (float(_strung(lines).distance(_strung(edges))) if lines and edges else math.inf)
                if axis is None
                else min((_apart(vertex, *edge) for vertices, spans in ((points, edges), (others, lines)) for vertex in vertices for edge in spans), default=math.inf)
            )
        case overlaps:
            return -_inradius(overlaps)


# --- [COVERAGE]


def _column_coverage(outline: _Outline, box: _Box, columns: int, /, axis: _Axis = "x") -> tuple[float, ...]:
    """Returns the filled fraction of each of `columns` equal strips laid across a box along an axis, left to right or bottom to top."""
    return tuple(_area(_combined(outline, (strip.contour,), PathOp.INTERSECTION)) / (strip.width * strip.height) for strip in box.strips(columns, axis))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["perimeter"]
