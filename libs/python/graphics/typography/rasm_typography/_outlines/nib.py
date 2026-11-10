"""Broad-nib sweeps, extrusions and round-pen strokes along quadratic skeletons and outlines."""

lazy from math import remainder

lazy from fontTools.misc.bezierTools import splitQuadraticAtT
lazy from fontTools.misc.transform import Offset
lazy import numpy as np
lazy import pathops
lazy from scipy.special import cosdg, sindg

lazy from rasm_typography._faces.glyphs import Point, traced
lazy from rasm_typography._outlines import paths

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ribbons(outline: tuple[tuple[Point, ...], ...], start: tuple[float, float], end: tuple[float, float], /, *, closed: bool) -> tuple[tuple[tuple[Point, ...], ...], ...]:
    """Areas a straight nib from `start` to `end` sweeps along each segment, moved to `end` and back reversed at `start`, one outline per piece to fix its own winding, a quadratic split where its tangent runs parallel to the nib so no swept piece folds."""
    nib = (end[0] - start[0], end[1] - start[1])
    pieces: list[tuple[tuple[float, float], ...]] = []
    for segment in paths.segments(outline, closed=closed):
        match segment:
            case ((x0, y0), (x1, y1), (x2, y2)) if (bend := (x2 - 2 * x1 + x0) * nib[1] - (y2 - 2 * y1 + y0) * nib[0]) and 0.0 < (at := ((y1 - y0) * nib[0] - (x1 - x0) * nib[1]) / bend) < 1.0:
                pieces.extend(tuple((float(x), float(y)) for x, y in piece) for piece in splitQuadraticAtT(*segment, at))
            case _:
                pieces.append(segment)
    return tuple(
        (
            tuple(
                Point(x=x + dx, y=y + dy, on=len(piece) == 2 or index != 1)
                for (dx, dy), ordered in ((end, tuple(enumerate(piece))), (start, tuple(enumerate(piece))[::-1]))
                for index, (x, y) in ordered
            ),
        )
        for piece in pieces
    )


def swept(skeleton: tuple[tuple[Point, ...], ...], width: float, angle: float, /, *, closed: bool = False) -> tuple[tuple[Point, ...], ...]:
    """Returns the outline of a broad nib `width` units wide held at `angle` degrees and drawn along each skeleton contour."""
    degrees = np.float64(remainder(angle, 360.0))
    half = (cosdg(degrees) * width / 2, sindg(degrees) * width / 2)
    return paths.union(*_ribbons(skeleton, (-half[0], -half[1]), half, closed=closed))


def extruded(outline: tuple[tuple[Point, ...], ...], vector: tuple[float, float], /) -> tuple[tuple[Point, ...], ...]:
    """Returns the area an outline sweeps moving along a vector, every edge swept by it, the solid a shadow face casts."""
    return paths.union(outline, paths.transformed(outline, Offset(*vector)), *_ribbons(outline, (0.0, 0.0), vector, closed=True))


def monoline(skeleton: tuple[tuple[Point, ...], ...], width: float, /, *, closed: bool = False) -> tuple[tuple[Point, ...], ...]:
    """Returns the outline of a round pen `width` units across drawn along each skeleton contour, its ends and joins the pen's own arcs."""
    path = pathops.Path()
    paths.trace(path.getPen(), skeleton, closed=closed)
    path.stroke(width, pathops.LineCap.ROUND_CAP, pathops.LineJoin.ROUND_JOIN, 1.0)
    path.convertConicsToQuads()
    return paths.union(traced(path.draw))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["extruded", "monoline", "swept"]
