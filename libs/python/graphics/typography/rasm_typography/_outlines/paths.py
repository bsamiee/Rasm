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

lazy from rasm_typography._faces.glyphs import _Contour, _draw_points, _Outline, _Point, _traced, recorded

# --- [TYPES] ----------------------------------------------------------------------------

type _Axis = Literal["x", "y"]
type _Matrix = tuple[float, float, float, float, float, float]
type _Segment = tuple[tuple[float, float], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FILL = frozendict(fix_winding=True, keep_starting_points=False, clockwise=False)

# --- [MODELS] ---------------------------------------------------------------------------


class _Box(msgspec.Struct, frozen=True, kw_only=True):
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
    def contour(self) -> _Contour:
        """Counter-clockwise rectangle contour over the extent."""
        return (_Point(x=self.xmin, y=self.ymin), _Point(x=self.xmax, y=self.ymin), _Point(x=self.xmax, y=self.ymax), _Point(x=self.xmin, y=self.ymax))

    def __and__(self, other: _Box, /) -> Self | None:
        """Returns the shared extent of two boxes, `None` where they share no area."""
        match sectRect(msgspec.structs.astuple(self), msgspec.structs.astuple(other)):
            case (True, rect):
                return self.of(rect)
            case _:
                return None

    def __or__(self, other: _Box, /) -> Self:
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

    def band(self, low: float, high: float, /, axis: _Axis = "x") -> Self:
        """Returns the extent cut to the span from `low` to `high` along an axis."""
        return msgspec.structs.replace(self, xmin=low, xmax=high) if axis == "x" else msgspec.structs.replace(self, ymin=low, ymax=high)

    def strips(self, count: int, /, axis: _Axis = "x") -> tuple[Self, ...]:
        """Returns `count` equal strips laid across the extent along an axis, left to right or bottom to top."""
        low, high = (self.xmin, self.xmax) if axis == "x" else (self.ymin, self.ymax)
        return tuple(self.band(low + index * (high - low) / count, low + (index + 1) * (high - low) / count, axis) for index in range(count))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PENS]


def _trace(pen: AbstractPen, outline: _Outline, /, *, closed: bool = True) -> None:
    """Draws contours into a segment pen as given, an open contour starting at its first point."""
    _draw_points(PointToSegmentPen(pen), outline, closed=closed)


def _draw(pen: AbstractPen, /, *outlines: _Outline) -> None:
    """Draws each outline into a segment pen with winding fixed to counter-clockwise outer contours, overlapping outlines adding."""
    for outline in outlines:
        path = pathops.Path()
        _trace(path.getPen(), outline)
        pathops.simplify(path, **_FILL).draw(pen)


def _segments(outline: _Outline, /, *, closed: bool = True) -> tuple[_Segment, ...]:
    """Returns every line as two points and every quadratic as three, implied on-curve points explicit, closed contours read from their union with each closing line."""
    path = pathops.Path()
    _trace(path.getPen(), outline, closed=closed)
    return tuple(
        (before[-1], *step)
        for contour in (pathops.simplify(path, **_FILL) if closed else path).contours
        for steps in (tuple(tuple((float(x), float(y)) for x, y in points) for _, points in contour if points),)
        for before, step in pairwise((*steps, *(((steps[0][0],),) if closed and steps[-1][-1] != steps[0][0] else ())))
    )


# --- [BOOLEANS]


def _union(*outlines: _Outline) -> _Outline:
    """Returns the nonzero union of outlines, every overlap and retraced spur removed, each outline simplified then folded in pairwise, the form resolving slivers and near-coincident edges the native builder and a raw pair refuse."""

    def merged(held: pathops.Path, outline: _Outline, /) -> pathops.Path:  # type: ignore[no-any-unimported]
        path = pathops.Path()
        _trace(path.getPen(), outline)
        return pathops.op(held, pathops.simplify(path, **_FILL), pathops.PathOp.UNION, **_FILL)

    return _traced(reduce(merged, outlines, pathops.Path()).draw)


def _combined(a: _Outline, b: _Outline, operation: pathops.PathOp, /) -> _Outline:  # type: ignore[no-any-unimported]
    """Returns the native boolean operation of two nonzero-filled outlines."""
    one, two = pathops.Path(), pathops.Path()
    _trace(one.getPen(), a)
    _trace(two.getPen(), b)
    return _traced(pathops.op(one, two, operation, **_FILL).draw)


def _meets(a: _Outline, b: _Outline, /) -> bool:
    """Tests whether two outlines overlap, each simplified first, the form resolving near-coincident edges a raw pair refuses."""
    one, two = pathops.Path(), pathops.Path()
    _draw(one.getPen(), a)
    _draw(two.getPen(), b)
    return bool(_traced(pathops.op(one, two, pathops.PathOp.INTERSECTION, **_FILL).draw))


def _pieces(outline: _Outline, /) -> tuple[_Outline, ...]:
    """Splits the union of an outline into connected fills in drawing order, each outer contour with the holes whose smallest enclosing outer it is."""
    path = pathops.Path()
    _draw(path.getPen(), outline)
    contours = tuple(path.contours)
    outers = sorted((contour for contour in contours if not contour.clockwise), key=lambda contour: contour.area)
    owners = tuple((hole, next((outer for outer in outers if outer.contains(hole.firstPoints[0])), None)) for hole in contours if hole.clockwise)
    return tuple(_traced(partial(pathops.union, (outer, *(hole for hole, owner in owners if owner is outer)))) for outer in contours if not outer.clockwise)


def _strip(outline: _Outline, low: float, high: float, /, axis: _Axis = "x") -> _Outline:
    """Returns the outline between two coordinates along an axis, the band reaching one unit past the outline across the other."""
    match _bounds(outline):
        case _Box() as box:
            return _combined(outline, (box.grown(1.0).band(low, high, axis).contour,), pathops.PathOp.INTERSECTION)
        case _:
            return ()


# --- [EXTENTS]


def _area(outline: _Outline, /) -> float:
    """Returns the filled area of the outline's union."""
    path = pathops.Path()
    _draw(path.getPen(), outline)
    return float(path.area)


def _bounds(outline: _Outline, /) -> _Box | None:
    """Returns the curve extent of an outline, `None` for an empty outline."""
    pen = BoundsPen(None)
    _trace(pen, outline)
    return _Box.of(pen.bounds) if pen.bounds else None


def _iou(a: _Outline, b: _Outline, /) -> float:
    """Returns intersection over union of two outlines' filled areas, 0 when both are empty."""
    one, two = pathops.Path(), pathops.Path()
    _trace(one.getPen(), a)
    _trace(two.getPen(), b)
    joined = float(pathops.op(one, two, pathops.PathOp.UNION, **_FILL).area)
    return float(pathops.op(one, two, pathops.PathOp.INTERSECTION, **_FILL).area) / joined if joined else 0.0


# --- [SHAPES]


def _transformed(outline: _Outline, matrix: _Matrix, /) -> _Outline:
    """Maps every point through an affine matrix `(xx, xy, yx, yy, dx, dy)`, a `fontTools.misc.transform.Transform` included, contours reversed under a reflection to keep their winding."""
    recording = RecordingPointPen()
    xx, xy, yx, yy, _, _ = matrix
    _draw_points(TransformPointPen(ReverseContourPointPen(recording) if xx * yy - xy * yx < 0 else recording, matrix), outline, closed=True)
    return recorded(recording)


def _offset(outline: _Outline, distance: float, /) -> _Outline:
    """Grows the outline's filled area by `distance` units, or shrinks it for a negative distance, at mitered corners with miter limit 4, the rim snapped so edges meeting at a half stem coincide exactly."""
    if not distance:
        return _union(outline)
    body = pathops.Path()
    _draw(body.getPen(), outline)
    stroked, rim = pathops.Path(body), pathops.Path()
    snap = 1024
    stroked.stroke(2 * abs(distance), pathops.LineCap.BUTT_CAP, pathops.LineJoin.MITER_JOIN, 4.0)
    stroked.convertConicsToQuads()
    stroked.draw(RoundingPen(rim.getPen(), roundFunc=lambda value: round(value * snap) / snap))
    return _traced(pathops.op(body, rim, pathops.PathOp.UNION if distance > 0 else pathops.PathOp.DIFFERENCE, **_FILL).draw)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
