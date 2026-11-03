"""Stroke width, perimeter, counter width, moments, signed gap, strip coverage and stroke-targeted offsets over quadratic outlines."""

lazy from itertools import pairwise
lazy import math

import msgspec
lazy from fontTools.misc.bezierTools import segmentPointAtT
lazy from fontTools.pens.perimeterPen import PerimeterPen
lazy from fontTools.pens.statisticsPen import StatisticsPen

lazy from typography.faces.glyphs import Outline
lazy from typography.outlines.paths import area, Axis, Box, combined, draw, offset, Operation, pieces, segments, transformed

# --- [MODELS] ---------------------------------------------------------------------------


class Moments(msgspec.Struct, frozen=True, kw_only=True):
    """Ink center of mass and principal axis direction in degrees within (-90, 90]."""

    centre: tuple[float, float]
    angle: float


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WIDTHS]


def perimeter(outline: Outline, /) -> float:
    """Returns the edge length of the outline's union."""
    pen = PerimeterPen()
    draw(pen, outline)
    return float(pen.value)


def stroke(outline: Outline, /) -> float:
    """Returns the mean stroke width as twice the area over the perimeter, 0 for the empty outline."""
    edge = perimeter(outline)
    return 2 * area(outline) / edge if edge else 0.0


def _reach(outline: Outline, pen: float, /) -> float:
    """Offset where mean stroke `2A/P` reaches a pen width, the root of Steiner's `2(A + P d + pi d^2) = pen (P + 2 pi d)` nearest zero, exact for discs and stroked segments, 0 for an empty outline."""
    held, edge = area(outline), perimeter(outline)
    linear, constant = 2 * edge - 2 * math.pi * pen, 2 * held - pen * edge
    return (-linear + math.sqrt(max(linear * linear - 8 * math.pi * constant, 0.0))) / (4 * math.pi) if edge else 0.0


def restroked(outline: Outline, pen: float, /) -> Outline:
    """Returns the outline offset until its mean stroke reaches a pen width."""
    return offset(outline, _reach(outline, pen))


def counter(outline: Outline, /) -> float:
    """Returns the narrowest counter width as twice the inward offset at which each hole's mean stroke reaches zero, 0 for an outline without holes."""
    return min((-2 * _reach((hole,), 0.0) for _, *holes in pieces(outline) for hole in holes), default=0.0)


def moments(outline: Outline, /) -> Moments:
    """Returns the center of mass and principal axis of the outline's ink from its first and second moments."""
    pen = StatisticsPen()
    draw(pen, outline)
    return Moments(centre=(float(pen.meanX), float(pen.meanY)), angle=math.degrees(0.5 * math.atan2(2 * pen.covariance, pen.varianceX - pen.varianceY)))


# --- [DISTANCES]


def _flattened(outline: Outline, /) -> tuple[tuple[tuple[float, float], ...], tuple[tuple[tuple[float, float], tuple[float, float]], ...]]:
    """Vertices and edges of the union flattened within a quarter unit, each quadratic cut into as many steps as the square root of its second difference length."""
    runs = tuple(
        tuple(segmentPointAtT(segment, step / steps) for step in range(steps + 1))
        for segment in segments(outline)
        for steps in (max(1, math.ceil(math.sqrt(math.hypot(*(a - 2 * b + c for a, b, c in zip(*segment, strict=True)))))) if len(segment) == 3 else 1,)
    )
    return (tuple(*run for run in runs), tuple(*pairwise(run) for run in runs))


def _apart(axis: Axis | None, point: tuple[float, float], start: tuple[float, float], end: tuple[float, float], /) -> float:
    """Distance from a point to a segment, nearest in any direction, or vertical under an axis where the segment spans the point's abscissa, else infinite, callers swapping an `x` axis onto `y` first."""
    (px, py), (sx, sy), (dx, dy) = point, start, (end[0] - start[0], end[1] - start[1])
    match axis:
        case None:
            along = min(max(((px - sx) * dx + (py - sy) * dy) / (dx * dx + dy * dy), 0.0), 1.0) if dx or dy else 0.0
            return math.dist(point, (sx + along * dx, sy + along * dy))
        case _ if dx and min(sx, end[0]) <= px <= max(sx, end[0]):
            return abs(py - sy - (px - sx) * dy / dx)
        case _:
            return math.inf


def gap(a: Outline, b: Outline, /, axis: Axis | None = None) -> float:
    """Returns the signed distance between two outlines, nearest or along an axis, negated as the overlap depth, infinite where they never face.

    Args:
        a: First outline.
        b: Second outline.
        axis: Axis the distance runs along, `None` for the nearest separation in any direction.

    Returns:
        The separation for disjoint outlines, else the offset at which the deepest intersection piece's mean stroke reaches zero.
    """
    match pieces(combined(a, b, Operation.INTERSECTION)):
        case ():
            (points, lines), (others, edges) = (_flattened(transformed(outline, (0.0, 1.0, 1.0, 0.0, 0.0, 0.0)) if axis == "x" else outline) for outline in (a, b))
            return min((_apart(axis, vertex, *edge) for vertices, spans in ((points, edges), (others, lines)) for vertex in vertices for edge in spans), default=math.inf)
        case overlaps:
            return min(_reach(piece, 0.0) for piece in overlaps)


# --- [COVERAGE]


def column_ink(outline: Outline, box: Box, columns: int, /, axis: Axis = "x") -> tuple[float, ...]:
    """Returns the inked fraction of each of `columns` equal strips laid across a box along an axis, left to right or bottom to top."""
    return tuple(area(combined(outline, (strip.contour,), Operation.INTERSECTION)) / (strip.width * strip.height) for strip in box.strips(columns, axis))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Moments", "column_ink", "counter", "gap", "moments", "perimeter", "restroked", "stroke"]
