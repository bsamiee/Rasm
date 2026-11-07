"""Pen conversion, booleans, pieces, extents, affine maps and stroke offsets over quadratic outlines through skia-pathops."""

# mypy: disable-error-code="no-untyped-call"

from typing import Literal, Self
lazy from functools import partial, reduce
lazy from itertools import pairwise

import msgspec
lazy from fontTools.misc.arrayTools import insetRect, offsetRect, sectRect, unionRect
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.pens.basePen import AbstractPen
lazy from fontTools.pens.boundsPen import BoundsPen
lazy from fontTools.pens.pointPen import PointToSegmentPen, ReverseContourPointPen
lazy from fontTools.pens.recordingPen import RecordingPointPen
lazy from fontTools.pens.roundingPen import RoundingPen
lazy from fontTools.pens.transformPen import TransformPointPen
lazy import pathops

lazy from typography.faces.glyphs import Contour, draw_points, Outline, Point, recorded, traced

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal["x", "y"]
type Matrix = tuple[float, float, float, float, float, float]
type Segment = tuple[tuple[float, float], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FILL = frozendict(fix_winding=True, keep_starting_points=False, clockwise=False)

# --- [MODELS] ---------------------------------------------------------------------------


class Box(msgspec.Struct, frozen=True, kw_only=True):
    """Axis-aligned outline extent in font units, the rectangle `fontTools.misc.arrayTools` reads in field order."""

    xmin: float
    ymin: float
    xmax: float
    ymax: float

    @classmethod
    def of(cls, rect: tuple[float, float, float, float], /) -> Self:
        """Returns the extent of an `(xmin, ymin, xmax, ymax)` rectangle."""
        xmin, ymin, xmax, ymax = rect
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

    def shifted(self, dx: float, dy: float, /) -> Self:
        """Returns the extent translated by a vector."""
        return self.of(offsetRect(msgspec.structs.astuple(self), dx, dy))

    def centered(self, advance: float, /) -> Transform:
        """Translation placing the horizontal outline center at the center of an advance."""
        return Offset(advance / 2 - self.middle[0], 0)

    def band(self, low: float, high: float, /, axis: Axis = "x") -> Self:
        """Returns the extent cut to the span from `low` to `high` along an axis."""
        return msgspec.structs.replace(self, xmin=low, xmax=high) if axis == "x" else msgspec.structs.replace(self, ymin=low, ymax=high)

    def strips(self, count: int, /, axis: Axis = "x") -> tuple[Self, ...]:
        """Returns `count` equal strips laid across the extent along an axis, left to right or bottom to top."""
        low, high = (self.xmin, self.xmax) if axis == "x" else (self.ymin, self.ymax)
        return tuple(self.band(low + index * (high - low) / count, low + (index + 1) * (high - low) / count, axis) for index in range(count))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PENS]


def trace(pen: AbstractPen, outline: Outline, /, *, closed: bool = True) -> None:
    """Draws contours into a segment pen as given, an open contour starting at its first point."""
    draw_points(PointToSegmentPen(pen), outline, closed=closed)


def draw(pen: AbstractPen, /, *outlines: Outline) -> None:
    """Draws each outline into a segment pen with winding fixed to counter-clockwise outer contours, overlapping outlines adding."""
    for outline in outlines:
        path = pathops.Path()
        trace(path.getPen(), outline)
        pathops.simplify(path, **_FILL).draw(pen)


def segments(outline: Outline, /, *, closed: bool = True) -> tuple[Segment, ...]:
    """Returns every line as two points and every quadratic as three, implied on-curve points explicit, closed contours read from their union with each closing line."""
    path = pathops.Path()
    trace(path.getPen(), outline, closed=closed)
    return tuple(  # ty: ignore[unsound-return-statement]
        (before[-1], *step)
        for contour in (pathops.simplify(path, **_FILL) if closed else path).contours
        for steps in (tuple(points for _, points in contour if points),)
        for before, step in pairwise((*steps, *(((steps[0][0],),) if closed and steps[-1][-1] != steps[0][0] else ())))
    )


# --- [BOOLEANS]


def union(*outlines: Outline) -> Outline:
    """Returns the nonzero union of outlines, every overlap and retraced spur removed, each outline simplified then folded in pairwise, the form resolving slivers and near-coincident edges the native builder and a raw pair refuse."""

    def merged(held: pathops.Path, outline: Outline, /) -> pathops.Path:  # type: ignore[no-any-unimported]
        path = pathops.Path()
        trace(path.getPen(), outline)
        return pathops.op(held, pathops.simplify(path, **_FILL), pathops.PathOp.UNION, **_FILL)

    return traced(reduce(merged, outlines, pathops.Path()).draw)


def combined(a: Outline, b: Outline, operation: pathops.PathOp, /) -> Outline:  # type: ignore[no-any-unimported]
    """Returns the native boolean operation of two nonzero-filled outlines."""
    one, two = pathops.Path(), pathops.Path()
    trace(one.getPen(), a)
    trace(two.getPen(), b)
    return traced(pathops.op(one, two, operation, **_FILL).draw)


def meets(a: Outline, b: Outline, /) -> bool:
    """Tests whether two outlines overlap, each simplified first, the form resolving near-coincident edges a raw pair refuses."""
    one, two = pathops.Path(), pathops.Path()
    draw(one.getPen(), a)
    draw(two.getPen(), b)
    return bool(traced(pathops.op(one, two, pathops.PathOp.INTERSECTION, **_FILL).draw))


def pieces(outline: Outline, /) -> tuple[Outline, ...]:
    """Splits the union of an outline into connected fills in drawing order, each outer contour with the holes whose smallest enclosing outer it is."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    contours = tuple(path.contours)
    outers = sorted((contour for contour in contours if not contour.clockwise), key=lambda contour: contour.area)
    owners = tuple((hole, next((outer for outer in outers if outer.contains(hole.firstPoints[0])), None)) for hole in contours if hole.clockwise)
    return tuple(traced(partial(pathops.union, (outer, *(hole for hole, owner in owners if owner is outer)))) for outer in contours if not outer.clockwise)


def strip(outline: Outline, low: float, high: float, /, axis: Axis = "x") -> Outline:
    """Returns the outline between two coordinates along an axis, the band reaching one unit past the outline across the other."""
    match bounds(outline):
        case Box() as box:
            return combined(outline, (box.grown(1.0).band(low, high, axis).contour,), pathops.PathOp.INTERSECTION)
        case _:
            return ()


# --- [EXTENTS]


def area(outline: Outline, /) -> float:
    """Returns the filled area of the outline's union."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    return path.area  # type: ignore[no-any-return]  # ty: ignore[unsound-return-statement]


def bounds(outline: Outline, /) -> Box | None:
    """Returns the curve extent of an outline, `None` for an empty outline."""
    pen = BoundsPen(None)
    trace(pen, outline)
    return Box.of(pen.bounds) if pen.bounds else None


def iou(a: Outline, b: Outline, /) -> float:
    """Returns intersection over union of two outlines' filled areas, 0 when both are empty."""
    one, two = pathops.Path(), pathops.Path()
    trace(one.getPen(), a)
    trace(two.getPen(), b)
    joined = pathops.op(one, two, pathops.PathOp.UNION, **_FILL).area
    return pathops.op(one, two, pathops.PathOp.INTERSECTION, **_FILL).area / joined if joined else 0.0  # ty: ignore[unsound-return-statement]


# --- [SHAPES]


def transformed(outline: Outline, matrix: Matrix, /) -> Outline:
    """Maps every point through an affine matrix `(xx, xy, yx, yy, dx, dy)`, a `fontTools.misc.transform.Transform` included, contours reversed under a reflection to keep their winding."""
    recording = RecordingPointPen()
    xx, xy, yx, yy, _, _ = matrix
    draw_points(TransformPointPen(ReverseContourPointPen(recording) if xx * yy - xy * yx < 0 else recording, matrix), outline, closed=True)
    return recorded(recording)


def offset(outline: Outline, distance: float, /) -> Outline:
    """Grows the outline's filled area by `distance` units, or shrinks it for a negative distance, at mitered corners with miter limit 4, the rim snapped so edges meeting at a half stem coincide exactly."""
    if not distance:
        return union(outline)
    body = pathops.Path()
    draw(body.getPen(), outline)
    stroked, rim = pathops.Path(body), pathops.Path()
    snap = 1024
    stroked.stroke(2 * abs(distance), pathops.LineCap.BUTT_CAP, pathops.LineJoin.MITER_JOIN, 4.0)
    stroked.convertConicsToQuads()
    stroked.draw(RoundingPen(rim.getPen(), roundFunc=lambda value: round(value * snap) / snap))
    return traced(pathops.op(body, rim, pathops.PathOp.UNION if distance > 0 else pathops.PathOp.DIFFERENCE, **_FILL).draw)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Box", "Matrix", "Segment", "area", "bounds", "combined", "draw", "iou", "meets", "offset", "pieces", "segments", "strip", "trace", "transformed", "union"]
