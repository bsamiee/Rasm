"""Pen conversion, booleans, pieces, extents, affine maps and stroke offsets over quadratic outlines through skia-pathops."""

from typing import Literal, Self
lazy from functools import partial
lazy from itertools import pairwise

import msgspec
lazy from fontTools.misc.arrayTools import insetRect, offsetRect, sectRect, unionRect
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.pens.basePen import AbstractPen, decomposeQuadraticSegment
lazy from fontTools.pens.boundsPen import BoundsPen
lazy from fontTools.pens.pointPen import PointToSegmentPen
lazy from fontTools.pens.roundingPen import RoundingPen
lazy import pathops

lazy from rasm_typography._faces.glyphs import draw_points, Point, traced

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal["x", "y"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FILL = frozendict(fix_winding=True, keep_starting_points=False, clockwise=False)
_GRID = 64

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
    def contour(self) -> tuple[Point, ...]:
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


def trace(pen: AbstractPen, outline: tuple[tuple[Point, ...], ...], /, *, closed: bool = True) -> None:
    """Draws contours into a segment pen on a 1/64-unit grid, an open contour starting at its first point, edges nearer than the grid coinciding, which skia-pathops booleans resolve where near-coincident ones return wrong fills."""
    draw_points(PointToSegmentPen(RoundingPen(pen, roundFunc=lambda value: round(value * _GRID) / _GRID)), outline, closed=closed)


def draw(pen: AbstractPen, /, *outlines: tuple[tuple[Point, ...], ...]) -> None:
    """Draws each outline into a segment pen with winding fixed to counter-clockwise outer contours, overlapping outlines adding."""
    for outline in outlines:
        path = pathops.Path()
        trace(path.getPen(), outline)
        pathops.simplify(path, **_FILL).draw(pen)


def segments(outline: tuple[tuple[Point, ...], ...], /, *, closed: bool = True) -> tuple[tuple[tuple[float, float], ...], ...]:
    """Returns every line as two points and every quadratic as three as the contours are drawn, implied on-curve points explicit, a closed contour ending at its start, one of off-curve points alone starting between its last and first."""
    return tuple(
        (start, *step)
        for contour in outline
        if contour
        for first, implied in ((next((index for index, point in enumerate(contour) if point.on), None), Point(x=(contour[-1].x + contour[0].x) / 2, y=(contour[-1].y + contour[0].y) / 2)),)
        for ring in ((*contour[first:], *contour[:first], *(contour[first : first + 1] if closed else ())) if first is not None else (implied, *contour, implied),)
        for low, high in pairwise(index for index, point in enumerate(ring) if point.on)
        for steps in (
            (((ring[high].x, ring[high].y),),)
            if high == low + 1
            else tuple(((float(cx), float(cy)), (float(ex), float(ey))) for (cx, cy), (ex, ey) in decomposeQuadraticSegment(tuple((point.x, point.y) for point in ring[low + 1 : high + 1]))),
        )
        for start, step in zip(((ring[low].x, ring[low].y), *(pair[-1] for pair in steps[:-1])), steps, strict=True)
    )


# --- [BOOLEANS]


def union(*outlines: tuple[tuple[Point, ...], ...]) -> tuple[tuple[Point, ...], ...]:
    """Returns the nonzero union of outlines, every overlap and retraced spur removed, each outline simplified then folded in pairwise, the form resolving slivers and near-coincident edges the native builder and a raw pair refuse."""
    held = pathops.Path()
    for outline in outlines:
        path = pathops.Path()
        trace(path.getPen(), outline)
        held = pathops.op(held, pathops.simplify(path, **_FILL), pathops.PathOp.UNION, **_FILL)
    return traced(held.draw)


def combined(a: tuple[tuple[Point, ...], ...], b: tuple[tuple[Point, ...], ...], operation: int, /) -> tuple[tuple[Point, ...], ...]:
    """Returns the native boolean operation of two nonzero-filled outlines."""
    one, two = pathops.Path(), pathops.Path()
    trace(one.getPen(), a)
    trace(two.getPen(), b)
    return traced(pathops.op(one, two, operation, **_FILL).draw)


def pieces(outline: tuple[tuple[Point, ...], ...], /) -> tuple[tuple[tuple[Point, ...], ...], ...]:
    """Splits the union of an outline into connected fills in drawing order, each outer contour with the holes whose smallest enclosing outer it is, an outer enclosing no area no fill."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    contours = tuple(path.contours)
    outers = sorted((contour for contour in contours if not contour.clockwise), key=lambda contour: contour.area)
    owners = tuple((hole, next((outer for outer in outers if outer.contains(hole.firstPoints[0])), None)) for hole in contours if hole.clockwise)
    return tuple(
        piece for outer in contours if not outer.clockwise for piece in (traced(partial(pathops.union, (outer, *(hole for hole, owner in owners if owner is outer)))),) if piece
    )


def groups(outline: tuple[tuple[Point, ...], ...], /) -> tuple[tuple[tuple[Point, ...], ...], ...]:
    """Pieces of an outline, each lying inside another's outer contour joined to the smallest such piece, a contour drawing's core with the ring around it, smallest first, the boolean run only where one piece's extent holds the other's."""
    held = sorted(((area(piece), piece, box) for piece in pieces(outline) if (box := bounds(piece))), key=lambda row: row[0])
    owners = tuple(
        next(
            (
                other
                for other, (_, outer, frame) in enumerate(held[index + 1 :], index + 1)
                if (box & frame) == box and not combined(piece, (max(outer, key=lambda contour: area((contour,))),), pathops.PathOp.DIFFERENCE)
            ),
            index,
        )
        for index, (_, piece, box) in enumerate(held)
    )
    roots: dict[int, int] = {}
    for index in reversed(range(len(held))):
        roots[index] = index if owners[index] == index else roots[owners[index]]
    return tuple(union(*(held[member][1] for member, root in roots.items() if root == index)) for index in sorted(set(roots.values())))


def strip(outline: tuple[tuple[Point, ...], ...], low: float, high: float, /, axis: Axis = "x") -> tuple[tuple[Point, ...], ...]:
    """Returns the outline between two coordinates along an axis, the band reaching one unit past the outline across the other."""
    match bounds(outline):
        case Box() as box:
            return combined(outline, (box.grown(1.0).band(low, high, axis).contour,), pathops.PathOp.INTERSECTION)
        case _:
            return ()


# --- [EXTENTS]


def area(outline: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns the filled area of the outline's union."""
    path = pathops.Path()
    draw(path.getPen(), outline)
    return float(path.area)


def bounds(outline: tuple[tuple[Point, ...], ...], /) -> Box | None:
    """Returns the curve extent of an outline, `None` for an empty outline."""
    pen = BoundsPen(None)
    trace(pen, outline)
    return Box.of(pen.bounds) if pen.bounds else None


def iou(a: tuple[tuple[Point, ...], ...], b: tuple[tuple[Point, ...], ...], /) -> float:
    """Returns intersection over union of two outlines' filled areas, 0 when both are empty."""
    one, two = pathops.Path(), pathops.Path()
    trace(one.getPen(), a)
    trace(two.getPen(), b)
    joined = float(pathops.op(one, two, pathops.PathOp.UNION, **_FILL).area)
    return float(pathops.op(one, two, pathops.PathOp.INTERSECTION, **_FILL).area) / joined if joined else 0.0


# --- [SHAPES]


def transformed(outline: tuple[tuple[Point, ...], ...], matrix: tuple[float, float, float, float, float, float], /) -> tuple[tuple[Point, ...], ...]:
    """Maps every point through an affine matrix `(xx, xy, yx, yy, dx, dy)`, a `fontTools.misc.transform.Transform` included, contours reversed under a reflection to keep their winding."""
    mapping = Transform(*matrix)
    return tuple(
        tuple(Point(x=x, y=y, on=point.on) for (x, y), point in zip(mapping.transformPoints(tuple((point.x, point.y) for point in ordered)), ordered, strict=True))
        for contour in outline
        for ordered in (contour[::-1] if mapping.xx * mapping.yy - mapping.xy * mapping.yx < 0 else contour,)
    )


def offset(outline: tuple[tuple[Point, ...], ...], distance: float, /) -> tuple[tuple[Point, ...], ...]:
    """Grows the outline's filled area by `distance` units, or shrinks it for a negative distance, at mitered corners with miter limit 4, body and rim on the trace grid, so edges meeting at a half stem coincide exactly and near-coincident ones never reach the native boolean."""
    if not distance:
        return union(outline)
    body = pathops.Path()
    draw(body.getPen(), outline)
    stroked, rim = pathops.Path(body), pathops.Path()
    stroked.stroke(2 * abs(distance), pathops.LineCap.BUTT_CAP, pathops.LineJoin.MITER_JOIN, 4.0)
    stroked.convertConicsToQuads()
    stroked.draw(RoundingPen(rim.getPen(), roundFunc=lambda value: round(value * _GRID) / _GRID))
    return traced(pathops.op(body, rim, pathops.PathOp.UNION if distance > 0 else pathops.PathOp.DIFFERENCE, **_FILL).draw)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Box", "area", "bounds", "combined", "draw", "groups", "iou", "offset", "pieces", "segments", "strip", "trace", "transformed", "union"]
