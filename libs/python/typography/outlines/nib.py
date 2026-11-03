"""Broad-nib sweeps, extrusions and round-pen strokes along quadratic skeletons and outlines."""

lazy import math

lazy from fontTools.misc.bezierTools import splitQuadraticAtT
lazy from fontTools.misc.transform import Offset
lazy import pathops

lazy from typography.faces.glyphs import Outline, Point
lazy from typography.outlines.paths import Segment, segments, trace, traced, transformed, union

# --- [OPERATIONS] -----------------------------------------------------------------------


def _unfolded(segment: Segment, nib: tuple[float, float], /) -> tuple[Segment, ...]:
    """Splits a quadratic where its tangent runs parallel to the nib, the root of the tangent's linear cross product with it, so no swept piece folds."""
    match segment:
        case ((x0, y0), (x1, y1), (x2, y2)) if (bend := (x2 - 2 * x1 + x0) * nib[1] - (y2 - 2 * y1 + y0) * nib[0]) and 0.0 < (at := ((y1 - y0) * nib[0] - (x1 - x0) * nib[1]) / bend) < 1.0:
            return tuple(tuple(piece) for piece in splitQuadraticAtT(*segment, at))
        case _:
            return (segment,)


def _ribbons(outline: Outline, start: tuple[float, float], end: tuple[float, float], /, *, closed: bool) -> tuple[Outline, ...]:
    """Areas a straight nib from `start` to `end` sweeps along each segment, the segment moved to `end` and back reversed at `start`, one outline each so each fixes its own winding."""
    return tuple(
        (
            tuple(
                Point(x=x + dx, y=y + dy, on=len(piece) == 2 or index != 1)
                for (dx, dy), ordered in ((end, tuple(enumerate(piece))), (start, tuple(enumerate(piece))[::-1]))
                for index, (x, y) in ordered
            ),
        )
        for segment in segments(outline, closed=closed)
        for piece in _unfolded(segment, (end[0] - start[0], end[1] - start[1]))
    )


def swept(skeleton: Outline, width: float, angle: float, /, *, closed: bool = False) -> Outline:
    """Returns the ink of a broad nib `width` units wide held at `angle` degrees and drawn along each skeleton contour."""
    half = (math.cos(math.radians(angle)) * width / 2, math.sin(math.radians(angle)) * width / 2)
    return union(*_ribbons(skeleton, (-half[0], -half[1]), half, closed=closed))


def extruded(outline: Outline, vector: tuple[float, float], /) -> Outline:
    """Returns the ink an outline covers moving along a vector, every edge swept by it, the solid a shadow face casts."""
    return union(outline, transformed(outline, Offset(*vector)), *_ribbons(outline, (0.0, 0.0), vector, closed=True))


def monoline(skeleton: Outline, width: float, /, *, closed: bool = False) -> Outline:
    """Returns the ink of a round pen `width` units across drawn along each skeleton contour, its ends and joins the pen's own arcs."""
    path = pathops.Path()
    trace(path.getPen(), skeleton, closed=closed)
    path.stroke(width, pathops.LineCap.ROUND_CAP, pathops.LineJoin.ROUND_JOIN, 1.0)
    path.convertConicsToQuads()
    return union(traced(path.draw))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["extruded", "monoline", "swept"]
