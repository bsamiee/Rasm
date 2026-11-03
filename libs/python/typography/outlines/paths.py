"""Pen conversion, booleans, pieces, extents, affine maps and stroke offsets over quadratic outlines through skia-pathops."""

from enum import StrEnum
from typing import Literal, Self
lazy from collections.abc import Callable
lazy from functools import partial
lazy from itertools import groupby, pairwise

import msgspec
lazy from fontTools.misc.arrayTools import insetRect, sectRect, unionRect
lazy from fontTools.pens.basePen import AbstractPen
lazy from fontTools.pens.boundsPen import BoundsPen
lazy from fontTools.pens.cu2quPen import Cu2QuPen
lazy from fontTools.pens.pointPen import AbstractPointPen, PointToSegmentPen, ReverseContourPointPen, SegmentToPointPen
lazy from fontTools.pens.recordingPen import RecordingPointPen
lazy from fontTools.pens.transformPen import TransformPointPen
lazy import pathops

lazy from typography.faces.glyphs import Contour, Outline, Point

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal["x", "y"]
type Matrix = tuple[float, float, float, float, float, float]
type Segment = tuple[tuple[float, float], ...]


class Operation(StrEnum):
    """Boolean operations, each value the `pathops.PathOp` member that applies it."""

    UNION = "UNION"
    DIFFERENCE = "DIFFERENCE"
    INTERSECTION = "INTERSECTION"
    XOR = "XOR"


# --- [CONSTANTS] ------------------------------------------------------------------------

_FILL = frozendict(fix_winding=True, keep_starting_points=False, clockwise=False)

# --- [MODELS] ---------------------------------------------------------------------------


class Box(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned ink extent in font units, the rectangle `fontTools.misc.arrayTools` reads in field order."""

    xmin: float
    ymin: float
    xmax: float
    ymax: float

    @classmethod
    def of(cls, rect: tuple[float, float, float, float], /) -> Self:
        """Returns the extent of an `(xmin, ymin, xmax, ymax)` rectangle."""
        xmin, ymin, xmax, ymax = map(float, rect)
        return cls(xmin=xmin, ymin=ymin, xmax=xmax, ymax=ymax)

    @property
    def width(self) -> float:
        """Horizontal extent."""
        return self.xmax - self.xmin

    @property
    def height(self) -> float:
        """Vertical extent."""
        return self.ymax - self.ymin

    @property
    def middle(self) -> tuple[float, float]:
        """Center point."""
        return ((self.xmin + self.xmax) / 2, (self.ymin + self.ymax) / 2)

    @property
    def contour(self) -> Contour:
        """Counter-clockwise rectangle contour over the extent."""
        return (Point(x=self.xmin, y=self.ymin), Point(x=self.xmax, y=self.ymin), Point(x=self.xmax, y=self.ymax), Point(x=self.xmin, y=self.ymax))

    def __and__(self, other: Box, /) -> Self | None:
        """Returns the shared extent of two boxes, `None` where they share no area."""
        match sectRect(msgspec.structs.astuple(self), msgspec.structs.astuple(other)):
            case (True, rect):
                return self.of(rect)
            case _:
                return None

    def __or__(self, other: Box, /) -> Self:
        """Returns the extent holding both boxes."""
        return self.of(unionRect(msgspec.structs.astuple(self), msgspec.structs.astuple(other)))

    def grown(self, distance: float, /) -> Self:
        """Returns the extent moved out by a distance on every side, in for a negative one."""
        return self.of(insetRect(msgspec.structs.astuple(self), -distance, -distance))

    def band(self, low: float, high: float, /, axis: Axis = "x") -> Self:
        """Returns the extent cut to the span from `low` to `high` along an axis."""
        return msgspec.structs.replace(self, xmin=low, xmax=high) if axis == "x" else msgspec.structs.replace(self, ymin=low, ymax=high)

    def strips(self, count: int, /, axis: Axis = "x") -> tuple[Self, ...]:
        """Returns `count` equal strips laid across the extent along an axis, left to right or bottom to top."""
        low, high = (self.xmin, self.xmax) if axis == "x" else (self.ymin, self.ymax)
        return tuple(self.band(low + index * (high - low) / count, low + (index + 1) * (high - low) / count, axis) for index in range(count))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PENS]


def _fed(points: AbstractPointPen, outline: Outline, /, *, closed: bool) -> None:
    """Sends contours to a point pen, each on-curve point typed by the point before it, an open contour starting with a move."""
    for contour in outline:
        points.beginPath()
        for index, (before, point) in enumerate(zip((contour[-1], *contour[:-1]), contour, strict=True)):
            points.addPoint((point.x, point.y), None if not point.on else "move" if not closed and not index else "line" if before.on else "qcurve")
        points.endPath()


def _recorded(recording: RecordingPointPen, /) -> Outline:
    """Contours a recording point pen holds."""
    return tuple(
        tuple(Point(x=float(x), y=float(y), on=kind is not None) for _, ((x, y), kind, *_), _ in group)
        for adding, group in groupby(recording.value, key=lambda entry: entry[0] == "addPoint")
        if adding
    )


def trace(pen: AbstractPen, outline: Outline, /, *, closed: bool = True) -> None:
    """Draws contours into a segment pen as given, an open contour starting at its first point."""
    _fed(PointToSegmentPen(pen), outline, closed=closed)


def draw(pen: AbstractPen, /, *outlines: Outline) -> None:
    """Draws each outline into a segment pen with winding fixed to counter-clockwise outer contours, overlapping outlines adding."""
    for outline in outlines:
        path = pathops.Path()
        trace(path.getPen(), outline)
        pathops.simplify(path, **_FILL).draw(pen)


def traced(drawing: Callable[[AbstractPen], object], /) -> Outline:
    """Reads what a drawing callable sends to a segment pen as quadratic contours, cubic segments converted through cu2qu within one unit."""
    recording = RecordingPointPen()
    drawing(Cu2QuPen(SegmentToPointPen(recording), max_err=1.0, all_quadratic=True))
    return _recorded(recording)


def segments(outline: Outline, /, *, closed: bool = True) -> tuple[Segment, ...]:
    """Returns every line as two points and every quadratic as three, implied on-curve points explicit, closed contours read from their union with each closing line."""
    path = pathops.Path()
    trace(path.getPen(), outline, closed=closed)
    return tuple(
        (before[-1], *step)
        for contour in (pathops.simplify(path, **_FILL) if closed else path).contours
        for steps in (tuple(points for _, points in contour if points),)
        for before, step in pairwise((*steps, *(((steps[0][0],),) if closed and steps[-1][-1] != steps[0][0] else ())))
    )


# --- [BOOLEANS]


def union(*outlines: Outline) -> Outline:
    """Returns the nonzero union of outlines, every overlap and retraced spur removed."""
    path = pathops.Path()
    draw(path.getPen(), *outlines)
    return traced(pathops.simplify(path, **_FILL).draw)


def combined(a: Outline, b: Outline, operation: Operation, /) -> Outline:
    """Returns the boolean operation of `a` with `b`, each outline's winding fixed first."""
    one, two = pathops.Path(), pathops.Path()
    draw(one.getPen(), a)
    draw(two.getPen(), b)
    return traced(pathops.op(one, two, pathops.PathOp[operation], **_FILL).draw)


def pieces(outline: Outline, /) -> tuple[Outline, ...]:
    """Splits the union of an outline into connected fills in drawing order, each outer contour with the holes whose smallest enclosing outer it is."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    contours = tuple(path.contours)
    outers = sorted((contour for contour in contours if not contour.clockwise), key=lambda contour: contour.area)
    owners = tuple((hole, next((outer for outer in outers if outer.contains(hole.firstPoints[0])), None)) for hole in contours if hole.clockwise)
    return tuple(traced(partial(pathops.union, (outer, *(hole for hole, owner in owners if owner is outer)))) for outer in contours if not outer.clockwise)


def strip(outline: Outline, low: float, high: float, /, axis: Axis = "x") -> Outline:
    """Returns the ink between two coordinates along an axis, the band reaching one unit past the ink across the other."""
    match bounds(outline):
        case Box() as box:
            return combined(outline, (box.grown(1.0).band(low, high, axis).contour,), Operation.INTERSECTION)
        case _:
            return ()


# --- [EXTENTS]


def area(outline: Outline, /) -> float:
    """Returns the ink area of the outline's union."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    return float(path.area)


def bounds(outline: Outline, /) -> Box | None:
    """Returns the curve extent of an outline, `None` for an empty outline."""
    pen = BoundsPen(None)
    trace(pen, outline)
    return Box.of(pen.bounds) if pen.bounds else None


def iou(a: Outline, b: Outline, /) -> float:
    """Returns intersection over union of two outlines' ink, 0 when both are empty."""
    joined = area(union(a, b))
    return area(combined(a, b, Operation.INTERSECTION)) / joined if joined else 0.0


# --- [SHAPES]


def transformed(outline: Outline, matrix: Matrix, /) -> Outline:
    """Maps every point through an affine matrix `(xx, xy, yx, yy, dx, dy)`, a `fontTools.misc.transform.Transform` included, contours reversed under a reflection to keep their winding."""
    recording = RecordingPointPen()
    xx, xy, yx, yy, _, _ = matrix
    _fed(TransformPointPen(ReverseContourPointPen(recording) if xx * yy - xy * yx < 0 else recording, matrix), outline, closed=True)
    return _recorded(recording)


def offset(outline: Outline, distance: float, /) -> Outline:
    """Grows the outline's ink by `distance` units, or shrinks it for a negative distance, at mitered corners with miter limit 4."""
    if not distance:
        return union(outline)
    body, rim = pathops.Path(), pathops.Path()
    draw(body.getPen(), outline)
    draw(rim.getPen(), outline)
    rim.stroke(2 * abs(distance), pathops.LineCap.BUTT_CAP, pathops.LineJoin.MITER_JOIN, 4.0)
    rim.convertConicsToQuads()
    return traced(pathops.op(body, pathops.simplify(rim, **_FILL), pathops.PathOp.UNION if distance > 0 else pathops.PathOp.DIFFERENCE, **_FILL).draw)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Box", "Matrix", "Operation", "Segment", "area", "bounds", "combined", "draw", "iou", "offset", "pieces", "segments", "strip", "trace", "traced", "transformed", "union"]
