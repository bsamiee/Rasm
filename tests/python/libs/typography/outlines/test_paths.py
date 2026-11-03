"""Booleans, pieces, segments, extents, transforms and offsets over squares, rings and overlapping rectangles."""

import math

from fontTools.misc.transform import Transform

from tests.python.support import assert_close
from typography.faces.glyphs import Contour, Outline, Point
from typography.outlines.paths import area, bounds, Box, combined, iou, offset, Operation, pieces, segments, strip, transformed, union

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0).contour,)
HOLE: Contour = tuple(reversed(Box(xmin=20.0, ymin=20.0, xmax=80.0, ymax=80.0).contour))
RING: Outline = (SQUARE[0], HOLE)
WIDE: Outline = (Box(xmin=50.0, ymin=0.0, xmax=150.0, ymax=100.0).contour,)
ROUND: Outline = (tuple(Point(x=x, y=y, on=False) for x, y in ((0.0, 0.0), (100.0, 0.0), (100.0, 100.0), (0.0, 100.0))),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_union_of_overlapping_rectangles_merges() -> None:
    """Rectangles overlapping by half, one contour of the joined extent."""
    joined = union(SQUARE, WIDE)
    assert len(joined) == 1
    assert_close(area(joined), 15000.0)
    assert bounds(joined) == Box(xmin=0.0, ymin=0.0, xmax=150.0, ymax=100.0)


def test_union_adds_outlines_of_opposite_direction() -> None:
    """Rectangle drawn clockwise beside one drawn counter-clockwise, their union holding both."""
    assert_close(area(union(SQUARE, (tuple(reversed(WIDE[0])),))), 15000.0)


def test_combined_operations_split_overlap() -> None:
    """Overlapping rectangles, difference and intersection each one half, exclusive or both outer halves."""
    assert_close(area(combined(SQUARE, WIDE, Operation.DIFFERENCE)), 5000.0)
    assert bounds(combined(SQUARE, WIDE, Operation.INTERSECTION)) == Box(xmin=50.0, ymin=0.0, xmax=100.0, ymax=100.0)
    assert_close(area(combined(SQUARE, WIDE, Operation.XOR)), 10000.0)


def test_ring_area_and_pieces_keep_hole() -> None:
    """Square with a square hole, one piece of outer and hole, an island inside the hole its own piece."""
    assert_close(area(RING), 6400.0)
    assert [len(piece) for piece in pieces(RING)] == [2]
    assert sorted(len(piece) for piece in pieces((*RING, Box(xmin=40.0, ymin=40.0, xmax=60.0, ymax=60.0).contour))) == [1, 2]


def test_bounds_of_empty_outline() -> None:
    """Empty outline, no extent."""
    assert bounds(()) is None


def test_iou_of_half_overlap() -> None:
    """Rectangles sharing half their width, a third of the union shared."""
    assert_close(iou(SQUARE, WIDE), 1 / 3)
    assert iou(SQUARE, SQUARE) == 1.0
    assert iou((), ()) == 0.0


def test_transformed_moves_scales_shears_and_keeps_winding_under_reflection() -> None:
    """Square moved, scaled, sheared about its middle line and mirrored, the mirror adding to the original."""
    assert bounds(transformed(SQUARE, Transform().translate(10.0, -5.0))) == Box(xmin=10.0, ymin=-5.0, xmax=110.0, ymax=95.0)
    assert bounds(transformed(SQUARE, Transform().scale(2.0))) == Box(xmin=0.0, ymin=0.0, xmax=200.0, ymax=200.0)
    assert_close(bounds(transformed(SQUARE, Transform().translate(0, 50.0).skew(math.radians(45.0)).translate(0, -50.0))), Box(xmin=-50.0, ymin=0.0, xmax=150.0, ymax=100.0), abs_tol=1e-9)
    assert_close(area(union(SQUARE, transformed(SQUARE, (-1.0, 0.0, 0.0, 1.0, 50.0, 0.0)))), 15000.0)


def test_offset_grows_and_shrinks_with_mitred_corners() -> None:
    """Square grown and shrunk by ten units, corners kept square, shrunk past its half width to nothing, its ring the difference."""
    assert_close(area(offset(SQUARE, 10.0)), 14400.0, rel_tol=1e-4)
    assert_close(area(offset(SQUARE, -10.0)), 6400.0, rel_tol=1e-4)
    assert bounds(offset(SQUARE, 10.0)) == Box(xmin=-10.0, ymin=-10.0, xmax=110.0, ymax=110.0)
    assert not offset(SQUARE, -60.0)
    assert [len(piece) for piece in pieces(combined(SQUARE, offset(SQUARE, -10.0), Operation.DIFFERENCE))] == [2]


def test_offset_of_curved_contour_stays_quadratic() -> None:
    """Contour of four off-curve points, grown by five units into quadratic arcs around a larger area."""
    grown = offset(ROUND, 5.0)
    assert any(not point.on for contour in grown for point in contour)
    assert area(grown) > area(ROUND) + 5.0 * 300.0


def test_segments_make_implied_points_explicit() -> None:
    """Square as four lines with its closing line, four off-curve points as arcs through implied midpoints, an open run as one line."""
    assert len(segments(SQUARE)) == 4
    arcs = segments(ROUND)
    assert all(len(arc) == 3 for arc in arcs)
    assert {(50.0, 0.0), (100.0, 50.0), (50.0, 100.0), (0.0, 50.0)} <= {arc[0] for arc in arcs}
    assert segments(((Point(x=0.0, y=0.0), Point(x=10.0, y=0.0)),), closed=False) == (((0.0, 0.0), (10.0, 0.0)),)


def test_box_algebra_through_rect_tools() -> None:
    """Square and wide rectangle sharing half their width, a box 30 units off met once the square grows past 30, the square split in rows."""
    square, wide, far = (Box(xmin=xmin, ymin=0.0, xmax=xmin + 100.0, ymax=100.0) for xmin in (0.0, 50.0, 130.0))
    assert (square & wide, square | wide, square.grown(30.0) & far, square.grown(31.0) & far) == (
        Box(xmin=50.0, ymin=0.0, xmax=100.0, ymax=100.0),
        Box(xmin=0.0, ymin=0.0, xmax=150.0, ymax=100.0),
        None,
        Box(xmin=130.0, ymin=0.0, xmax=131.0, ymax=100.0),
    )
    assert square.strips(2, "y") == (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=50.0), Box(xmin=0.0, ymin=50.0, xmax=100.0, ymax=100.0))


def test_strip_keeps_ink_between_two_coordinates() -> None:
    """Ring cut to a vertical band through its hole and a horizontal band over it, the empty outline cut to nothing."""
    assert_close(area(strip(RING, 40.0, 60.0)), 800.0)
    assert bounds(strip(RING, 90.0, 200.0, "y")) == Box(xmin=0.0, ymin=90.0, xmax=100.0, ymax=100.0)
    assert not strip((), 0.0, 1.0)
