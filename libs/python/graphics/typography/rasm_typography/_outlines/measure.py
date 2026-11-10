"""Stroke width, perimeter, counter width, moments, signed gap, strip coverage and stroke-targeted offsets over quadratic outlines."""

lazy from itertools import pairwise
lazy import math

import msgspec
lazy from fontTools.misc.bezierTools import segmentPointAtT
lazy from fontTools.pens.perimeterPen import PerimeterPen
lazy from fontTools.pens.statisticsPen import StatisticsPen
lazy from pathops import Path, PathOp
lazy from scipy.optimize import minimize_scalar
lazy from shapely import build_area, linestrings, maximum_inscribed_circle, MultiLineString, multilinestrings, MultiPolygon, Polygon

lazy from rasm_typography._faces.glyphs import Point
lazy from rasm_typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class _Moments(msgspec.Struct, frozen=True, kw_only=True):
    """Outline center of mass and principal axis direction in degrees within [-90, 90]."""

    centre: tuple[float, float]
    angle: float


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WIDTHS]


def perimeter(outline: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns the edge length of the outline's union."""
    pen = PerimeterPen()
    paths.draw(pen, outline)
    return float(pen.value)


def stroke(outline: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns the mean stroke width as twice the area over the perimeter, 0 for the empty outline."""
    path, pen = Path(), PerimeterPen()
    paths.draw(path.getPen(), outline)
    path.draw(pen)
    return 2 * float(path.area) / float(pen.value) if pen.value else 0.0


def restroked(outline: tuple[tuple[Point, ...], ...], pen: float, /) -> tuple[tuple[Point, ...], ...]:
    """Returns a geometric offset locally fitting the requested mean stroke, an empty outline kept empty.

    Fits squared stroke error between the outline's erosion radius and outward growth by the target pen. Topology changes can leave the closest local fit short of the target.
    """
    match paths.bounds(outline):
        case None:
            return ()
        case _:
            fit = minimize_scalar(lambda distance: (stroke(paths.offset(outline, distance)) - pen) ** 2, bounds=(-inradius(outline), pen))
            return paths.offset(outline, float(fit.x))


def counters(outline: tuple[tuple[Point, ...], ...], /) -> tuple[tuple[tuple[Point, ...], ...], ...]:
    """Returns bounded empty regions, retaining filled islands and nested holes with their original curves."""
    holes = paths.union(*((hole,) for _, *holes in paths.pieces(outline) for hole in holes))
    return paths.pieces(paths.combined(holes, outline, PathOp.DIFFERENCE))


def moments(outline: tuple[tuple[Point, ...], ...], /) -> _Moments:
    """Returns the center of mass and principal axis of the outline's filled area from its first and second moments."""
    pen = StatisticsPen()
    paths.draw(pen, outline)
    return _Moments(centre=(float(pen.meanX), float(pen.meanY)), angle=math.degrees(0.5 * math.atan2(2 * pen.covariance, pen.varianceX - pen.varianceY)))


# --- [DISTANCES]


def _flattened(outline: tuple[tuple[Point, ...], ...], /) -> tuple[tuple[tuple[float, float], ...], tuple[tuple[tuple[float, float], tuple[float, float]], ...]]:
    """Vertices and edges of the union flattened within a quarter unit, each quadratic cut into as many steps as the square root of its second difference length."""
    runs = tuple(
        tuple((float(x), float(y)) for step in range(steps + 1) for x, y in (segmentPointAtT(segment, step / steps),))
        for segment in paths.segments(outline)
        for steps in (max(1, math.ceil(math.sqrt(math.hypot(*(a - 2 * b + c for a, b, c in zip(*segment, strict=True)))))) if len(segment) == 3 else 1,)
    )
    return (tuple(*run for run in runs), tuple(*pairwise(run) for run in runs))


def _strung(edges: tuple[tuple[tuple[float, float], tuple[float, float]], ...], /) -> MultiLineString:
    """Edges as one geometry through the array constructors, an empty one for none, `TypeError` for an array the constructors return in its place."""
    match multilinestrings(linestrings(edges)) if edges else MultiLineString():
        case MultiLineString() as strung:
            return strung
        case other:
            raise TypeError(other)


def inradius(outline: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns the largest inscribed circle's radius, 0 for edges enclosing no area, retaining holes and islands."""
    match build_area(_strung(_flattened(outline)[1])):
        case Polygon() | MultiPolygon() as filled:
            return float(maximum_inscribed_circle(filled, tolerance=0.25).length)
        case _:
            return 0.0


def gap(a: tuple[tuple[Point, ...], ...], b: tuple[tuple[Point, ...], ...], /, axis: paths.Axis | None = None) -> float:
    """Returns the signed distance between two outlines, nearest or along an axis, negated as the overlap depth, infinite where they never face.

    Args:
        a: First outline.
        b: Second outline.
        axis: Axis the distance runs along, `None` for the nearest separation in any direction.

    Returns:
        The separation for disjoint outlines, else the negative radius of the largest circle inside their intersection.
    """
    match paths.combined(a, b, PathOp.INTERSECTION):
        case ():
            (points, lines), (others, edges) = (_flattened(paths.transformed(outline, (0.0, 1.0, 1.0, 0.0, 0.0, 0.0)) if axis == "x" else outline) for outline in (a, b))
            return (
                (float(_strung(lines).distance(_strung(edges))) if lines and edges else math.inf)
                if axis is None
                else min(
                    (
                        abs(py - sy - (px - sx) * (ey - sy) / (ex - sx))
                        for vertices, spans in ((points, edges), (others, lines))
                        for px, py in vertices
                        for (sx, sy), (ex, ey) in spans
                        if ex != sx and min(sx, ex) <= px <= max(sx, ex)
                    ),
                    default=math.inf,
                )
            )
        case overlaps:
            return -inradius(overlaps)


# --- [COVERAGE]


def column_coverage(outline: tuple[tuple[Point, ...], ...], box: paths.Box, columns: int, /, axis: paths.Axis = "x") -> tuple[float, ...]:
    """Returns the filled fraction of each of `columns` equal strips laid across a box along an axis, left to right or bottom to top."""
    return tuple(paths.area(paths.combined(outline, (strip.contour,), PathOp.INTERSECTION)) / (strip.width * strip.height) for strip in box.strips(columns, axis))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["column_coverage", "counters", "gap", "inradius", "moments", "perimeter", "restroked", "stroke"]
