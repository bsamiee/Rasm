"""Set algebra, affine geometry, holes, offsets, and quadratic segments."""

import math

from fontTools.misc.transform import Transform
from fontTools.pens.areaPen import AreaPen
from hypothesis import given, strategies as st
from pathops import PathOp
import pytest

from typography.faces.glyphs import Outline, Point
from typography.outlines.paths import area, Axis, bounds, Box, combined, iou, offset, pieces, segments, strip, trace, transformed, union

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0, ymin=0, xmax=100, ymax=100).contour,)
RING: Outline = (*SQUARE, tuple(reversed(Box(xmin=20, ymin=20, xmax=80, ymax=80).contour)))
ROUND: Outline = (tuple(Point(x=x, y=y, on=False) for x, y in ((0, 0), (100, 0), (100, 100), (0, 100))),)

# --- [OPERATIONS] -----------------------------------------------------------------------


@given(width=st.integers(1, 1000), height=st.integers(1, 1000), dx=st.integers(-1000, 1000), dy=st.integers(-1000, 1000))
def test_boolean_areas_follow_rectangle_set_algebra(width: int, height: int, dx: int, dy: int) -> None:
    """Union, intersection, difference, and exclusive or conserve area across overlap and disjointness."""
    first = (Box(xmin=0, ymin=0, xmax=width, ymax=height).contour,)
    second = transformed(first, Transform().translate(dx, dy))
    whole, shared = width * height, max(width - abs(dx), 0) * max(height - abs(dy), 0)
    joined, intersection = union(first, second), combined(first, second, PathOp.INTERSECTION)
    assert area(joined) == pytest.approx(2 * whole - shared, rel=1e-9, abs=0)
    assert area(intersection) == pytest.approx(shared, rel=1e-9, abs=0)
    assert bounds(joined) == Box(xmin=min(0, dx), ymin=min(0, dy), xmax=max(width, dx + width), ymax=max(height, dy + height))
    if shared:
        assert len(joined) == 1
        assert bounds(intersection) == Box(xmin=max(0, dx), ymin=max(0, dy), xmax=min(width, dx + width), ymax=min(height, dy + height))
    else:
        assert intersection == ()
    assert area(combined(first, second, PathOp.DIFFERENCE)) == pytest.approx(whole - shared, rel=1e-9, abs=0)
    assert area(combined(first, second, PathOp.XOR)) == pytest.approx(2 * (whole - shared), rel=1e-9, abs=0)
    assert iou(first, second) == pytest.approx(shared / (2 * whole - shared), rel=1e-9, abs=0)


def test_holes_and_nested_islands_keep_area_and_connectivity() -> None:
    """An island inside a hole stays separate while opposite winding outer fills add."""
    island = Box(xmin=40, ymin=40, xmax=60, ymax=60).contour
    filled = (*RING, island)
    assert area(filled) == pytest.approx(6800, rel=1e-9, abs=0)
    assert sorted(map(len, pieces(filled))) == [1, 2]
    assert sorted(area(piece) for piece in pieces(filled)) == pytest.approx([400, 6400], rel=1e-9, abs=0)
    assert area(union(SQUARE, (tuple(reversed(Box(xmin=50, ymin=0, xmax=150, ymax=100).contour)),))) == pytest.approx(15000, rel=1e-9, abs=0)


@pytest.mark.parametrize(
    "matrix,extent",
    [
        (Transform().translate(10, -5), (10, -5, 110, 95)),
        (Transform().scale(2, 3), (0, 0, 200, 300)),
        (Transform().skew(0.5), (0, 0, 100 * (1 + math.tan(0.5)), 100)),
        (Transform(-1, 0, 0, 1, 50, 0), (-50, 0, 50, 100)),
    ],
)
def test_affine_maps_preserve_set_geometry_under_inverse(matrix: Transform, extent: tuple[float, float, float, float]) -> None:
    """Affine area follows its determinant and inverse maps recover holes and reflection winding."""
    mapped, determinant = transformed(RING, matrix), abs(matrix.xx * matrix.yy - matrix.xy * matrix.yx)
    before, after = AreaPen(), AreaPen()  # type: ignore[no-untyped-call]
    trace(before, RING)
    trace(after, mapped)
    box = bounds(mapped)
    assert box is not None
    assert (box.xmin, box.ymin, box.xmax, box.ymax) == pytest.approx(extent, rel=1e-9, abs=1e-9)
    assert after.value == pytest.approx(determinant * before.value, rel=1e-9, abs=0)
    assert area(mapped) == pytest.approx(determinant * area(RING), rel=1e-6, abs=0)
    assert area(combined(transformed(mapped, matrix.inverse()), RING, PathOp.XOR)) == pytest.approx(0, rel=0, abs=0.01)  # type: ignore[no-untyped-call]
    assert area(union(mapped, mapped)) == pytest.approx(area(mapped), rel=1e-9, abs=0)


@pytest.mark.parametrize("distance", [-60, -10, 0, 10])
def test_miter_offsets_have_analytic_square_extents(distance: int) -> None:
    """Square offsets keep square corners through growth, erosion, and disappearance."""
    grown = offset(SQUARE, distance)
    assert area(grown) == pytest.approx(max(100 + 2 * distance, 0) ** 2, rel=1e-4, abs=0)
    assert bounds(grown) == (Box(xmin=-distance, ymin=-distance, xmax=100 + distance, ymax=100 + distance) if distance > -50 else None)
    if -50 < distance < 0:
        assert tuple(map(len, pieces(combined(SQUARE, grown, PathOp.DIFFERENCE)))) == (2,)


def test_offset_resolves_edges_meeting_at_half_a_stem() -> None:
    """Erosion by half a slanted stem leaves rim edges within the native tolerance of each other and still answers."""
    corners = ((567, 962), (699, 600), (320, 600), (538, 0), (357, 0), (-145, 1380), (415, 1380), (481, 1199), (102, 1199), (188, 962))
    assert area(offset((tuple(Point(x=x, y=y) for x, y in corners),), -90.49928373070757)) > 0


def test_union_resolves_slivers_the_native_builder_refuses() -> None:
    """Two thin slivers sharing one abscissa union into both of their inks."""
    spike = tuple(
        Point(x=x, y=y, on=on)
        for x, y, on in (
            (1470.2557373046875, 309.4224548339844, True),
            (1470.2557373046875, 111.79133605957031, True),
            (1480.237060546875, 131.7540740966797, False),
            (1480.237060546875, 151.71681213378906, True),
            (1470.2557373046875, 157.7056427001953, False),
        )
    )
    sill = tuple(
        Point(x=x, y=y, on=on)
        for x, y, on in ((1277.615234375, 0.0, True), (1470.2557373046875, 0.0, True), (1304.56494140625, 1.9962738752365112, True), (1291.5892333984375, 1.9962738752365112, False))
    )
    assert area(union((spike,), (sill,))) == pytest.approx(area((spike,)) + area((sill,)), rel=1e-6, abs=0)


def test_union_resolves_edges_within_the_native_tolerance() -> None:
    """A curve starting a thousandth of a unit below a shared edge unions with the bar over that edge."""
    curved = (Point(x=2048, y=-0.0007300344877876341), Point(x=2048.998779296875, y=0, on=False), Point(x=2050, y=0), Point(x=2096, y=0), Point(x=2096, y=265), Point(x=2048, y=265))
    bar = (Point(x=2048, y=0), Point(x=2218.666748046875, y=0), Point(x=2218.666748046875, y=265), Point(x=2048, y=265))
    assert area(union((curved,), (bar,))) == pytest.approx(area((bar,)), rel=1e-4, abs=0)


def test_curved_offsets_and_segments_retain_quadratic_geometry() -> None:
    """Implied points become explicit quadratic segments while open runs retain their endpoints."""
    grown, arcs = offset(ROUND, 5), segments(ROUND)
    assert any(not point.on for contour in grown for point in contour)
    assert area(grown) > area(ROUND) + 5 * 300
    assert len(segments(SQUARE)) == 4
    assert all(len(arc) == 3 for arc in arcs)
    assert {arc[0] for arc in arcs} == {(50, 0), (100, 50), (50, 100), (0, 50)}
    assert segments(((Point(x=0, y=0), Point(x=10, y=0)),), closed=False) == (((0, 0), (10, 0)),)


@pytest.mark.parametrize("axis,edge", [("x", Box(xmin=90, ymin=0, xmax=100, ymax=100)), ("y", Box(xmin=0, ymin=90, xmax=100, ymax=100))])
def test_strips_partition_ink_and_box_area(axis: Axis, edge: Box) -> None:
    """Equal strips partition a box and their clipped ring inks conserve area."""
    box = Box(xmin=0, ymin=0, xmax=100, ymax=100)
    strips = box.strips(7, axis)
    assert sum(part.width * part.height for part in strips) == pytest.approx(box.width * box.height, rel=1e-9, abs=0)
    assert sum(area(combined(RING, (part.contour,), PathOp.INTERSECTION)) for part in strips) == pytest.approx(area(RING), rel=1e-6, abs=0)
    assert area(strip(RING, 40, 60, axis)) == pytest.approx(800, rel=1e-9, abs=0)
    assert bounds(strip(RING, 90, 200, axis)) == edge
    assert box.strips(2, "y") == (Box(xmin=0, ymin=0, xmax=100, ymax=50), Box(xmin=0, ymin=50, xmax=100, ymax=100))


def test_empty_geometry_has_no_extent_or_overlap() -> None:
    """Empty operands retain empty-set behavior across geometry operations."""
    assert bounds(()) is None
    assert union((), ()) == strip((), 0, 1) == ()
    assert iou((), ()) == 0
    assert iou(SQUARE, SQUARE) == 1


def test_box_algebra_distinguishes_contact_from_area_overlap() -> None:
    """Box union and intersection retain their extents, while touching grown edges have no shared area."""
    square, wide, far = (Box(xmin=xmin, ymin=0, xmax=xmin + 100, ymax=100) for xmin in (0, 50, 130))
    assert (square & wide, square | wide, square.grown(30) & far, square.grown(31) & far) == (
        Box(xmin=50, ymin=0, xmax=100, ymax=100),
        Box(xmin=0, ymin=0, xmax=150, ymax=100),
        None,
        Box(xmin=130, ymin=0, xmax=131, ymax=100),
    )
