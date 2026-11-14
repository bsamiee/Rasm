"""Stroke width, perimeter, counter width, moments, signed gap, strip coverage and stroke-targeted offsets over quadratic outlines."""

lazy import math

import msgspec
lazy from fontTools.pens.perimeterPen import PerimeterPen
lazy from fontTools.pens.statisticsPen import StatisticsPen
lazy import numpy as np
lazy import numpy.typing as npt
lazy from pathops import Path, PathOp
lazy from scipy.optimize import minimize_scalar
lazy from shapely import build_area, linestrings, maximum_inscribed_circle, MultiLineString, multilinestrings, MultiPolygon, node, Polygon

lazy from rasm_typography._faces.glyphs import Point
lazy from rasm_typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class _Moments(msgspec.Struct, frozen=True, kw_only=True):
    """Outline center of mass, principal axis direction in degrees within [-90, 90], and elongation, the spread along that axis over the spread across it."""

    centre: tuple[float, float]
    angle: float
    elongation: float


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
    """Returns the center of mass, principal axis, and elongation of the outline's filled area from its first and second moments, an elongation infinite for a filled line."""
    pen = StatisticsPen()
    paths.draw(pen, outline)
    middle, spread = (pen.varianceX + pen.varianceY) / 2, math.hypot((pen.varianceX - pen.varianceY) / 2, pen.covariance)
    return _Moments(
        centre=(float(pen.meanX), float(pen.meanY)),
        angle=math.degrees(0.5 * math.atan2(2 * pen.covariance, pen.varianceX - pen.varianceY)),
        elongation=math.sqrt((middle + spread) / (middle - spread)) if middle > spread else math.inf,
    )


# --- [DISTANCES]


def _flattened(outline: tuple[tuple[Point, ...], ...], /) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.intp]]:
    """Points of the outline's lines and quadratics flattened within a quarter unit with the segment each lies on, a quadratic cut into as many steps as the square root of its second difference length, a line held as a quadratic with its middle as control."""
    held = np.array(
        tuple(segment if len(segment) == 3 else (segment[0], ((segment[0][0] + segment[1][0]) / 2, (segment[0][1] + segment[1][1]) / 2), segment[1]) for segment in paths.segments(outline)),
        dtype=np.float64,
    ).reshape(-1, 3, 2)
    steps = np.maximum(1, np.ceil(np.sqrt(np.hypot(*(held[:, 0] - 2 * held[:, 1] + held[:, 2]).T)))).astype(np.intp)
    runs = np.repeat(np.arange(len(held)), steps + 1)
    t = ((np.arange(len(runs)) - np.repeat(np.cumsum(steps + 1) - steps - 1, steps + 1)) / np.repeat(steps, steps + 1))[:, None]
    start, control, end = held[runs].transpose(1, 0, 2)
    return ((1 - t) ** 2 * start + 2 * t * (1 - t) * control + t**2 * end, runs)


def _strung(points: npt.NDArray[np.float64], runs: npt.NDArray[np.intp], /) -> MultiLineString:
    """Flattened segments as one geometry through the array constructors, an empty one for none, `TypeError` for a geometry the constructors return in place of an array or a multi line."""
    match linestrings(points, indices=runs) if len(points) else None:
        case None:
            return MultiLineString()
        case np.ndarray() as lines if isinstance(strung := multilinestrings(lines), MultiLineString):
            return strung
        case other:
            raise TypeError(other)


def inradius(outline: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns the largest inscribed circle's radius, 0 for edges enclosing no area, retaining holes and islands, the union's edges noded where flattening crosses them."""
    match build_area(node(_strung(*_flattened(paths.union(outline))))):
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
        The separation for disjoint outlines, the boolean intersection run only where their flattened extents meet within the flattening tolerance, else the negative radius of the largest circle inside their intersection.
    """
    (points, runs), (others, sides) = (_flattened(paths.transformed(outline, (0.0, 1.0, 1.0, 0.0, 0.0, 0.0)) if axis == "x" else outline) for outline in (a, b))
    facing = len(points) > 0 and len(others) > 0
    match paths.combined(a, b, PathOp.INTERSECTION) if facing and (points.min(axis=0) <= others.max(axis=0) + 0.25).all() and (others.min(axis=0) <= points.max(axis=0) + 0.25).all() else ():
        case () if not facing:
            return math.inf
        case () if axis is None:
            return float(_strung(points, runs).distance(_strung(others, sides)))
        case ():
            return min(
                (
                    float(np.min(np.abs(vertices[:, None, 1] - start[None, :, 1] - (vertices[:, None, 0] - start[None, :, 0]) * slope[None, :]), initial=math.inf, where=inside))
                    for vertices, spans, ids in ((points, others, sides), (others, points, runs))
                    for start, end in ((spans[:-1][ids[1:] == ids[:-1]], spans[1:][ids[1:] == ids[:-1]]),)
                    for slope in (np.divide(end[:, 1] - start[:, 1], end[:, 0] - start[:, 0], out=np.zeros(len(start)), where=end[:, 0] != start[:, 0]),)
                    for inside in (
                        (end[None, :, 0] != start[None, :, 0])
                        & (np.minimum(start[None, :, 0], end[None, :, 0]) <= vertices[:, None, 0])
                        & (vertices[:, None, 0] <= np.maximum(start[None, :, 0], end[None, :, 0])),
                    )
                ),
                default=math.inf,
            )
        case overlaps:
            return -inradius(overlaps)


# --- [COVERAGE]


def column_coverage(outline: tuple[tuple[Point, ...], ...], box: paths.Box, columns: int, /, axis: paths.Axis = "x") -> tuple[float, ...]:
    """Returns the filled fraction of each of `columns` equal strips laid across a box along an axis, left to right or bottom to top."""
    return tuple(paths.area(paths.combined(outline, (strip.contour,), PathOp.INTERSECTION)) / (strip.width * strip.height) for strip in box.strips(columns, axis))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["column_coverage", "counters", "gap", "inradius", "moments", "perimeter", "restroked", "stroke"]
