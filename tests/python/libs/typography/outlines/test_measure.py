"""Stroke width, counters, moments, signed gaps and column ink over rectangles, rings and stroked skeletons."""

import math

from fontTools.misc.transform import Transform

from tests.python.support import assert_close
from typography.faces.glyphs import Outline, Point
from typography.outlines.measure import column_ink, counter, gap, moments, perimeter, restroked, stroke
from typography.outlines.nib import monoline
from typography.outlines.paths import Box, combined, Operation, transformed

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0).contour,)
BAR: Outline = (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=20.0).contour,)
SEGMENT: Outline = ((Point(x=0.0, y=0.0), Point(x=200.0, y=0.0)),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_stroke_of_rectangle_is_twice_area_over_perimeter() -> None:
    """Bar of 100 by 20, mean width 2 * 2000 / 240."""
    assert_close(perimeter(BAR), 240.0)
    assert_close(stroke(BAR), 4000.0 / 240.0)
    assert stroke(()) == 0.0


def test_stroke_of_stroked_skeleton_nears_pen_width() -> None:
    """Segment of 200 stroked 20 wide with round ends, a stadium of known area and perimeter."""
    assert_close(stroke(monoline(SEGMENT, 20.0)), 2 * (4000.0 + math.pi * 100.0) / (400.0 + 20.0 * math.pi), rel_tol=1e-3)


def test_counter_reads_stadium_hole_width() -> None:
    """Slab with a stadium hole 20 wide, the counter its pen width, a square without holes 0."""
    slotted = combined((Box(xmin=-50.0, ymin=-50.0, xmax=250.0, ymax=50.0).contour,), monoline(SEGMENT, 20.0), Operation.DIFFERENCE)
    assert_close(counter(slotted), 20.0, rel_tol=1e-2)
    assert counter(SQUARE) == 0.0


def test_moments_read_centre_and_axis() -> None:
    """Bar turned 30 degrees about its centre, the centre kept and the principal axis at 30 degrees."""
    turned = moments(transformed(BAR, Transform().translate(50.0, 10.0).rotate(math.radians(30.0)).translate(-50.0, -10.0)))
    assert_close(turned.centre, (50.0, 10.0), abs_tol=1e-6)
    assert_close(turned.angle, 30.0, rel_tol=1e-6)


def test_gap_positive_for_separated_squares() -> None:
    """Squares 30 units apart sideways and diagonally."""
    assert_close(gap(SQUARE, transformed(SQUARE, Transform().translate(130.0, 0.0))), 30.0)
    assert_close(gap(SQUARE, transformed(SQUARE, Transform().translate(130.0, 130.0))), 30.0 * math.sqrt(2.0))


def test_gap_negative_for_overlapping_rectangles() -> None:
    """Squares overlapping in a 10 by 100 band, depth the inward offset of the stadium with its area and perimeter, near half the band."""
    assert_close(gap(SQUARE, transformed(SQUARE, Transform().translate(90.0, 0.0))), -(220.0 - math.sqrt(220.0**2 - 4000.0 * math.pi)) / (2 * math.pi))


def test_gap_along_each_axis() -> None:
    """Mark square above a base square, 30 apart vertically, never facing horizontally, overlapping once lowered."""
    mark = (Box(xmin=40.0, ymin=130.0, xmax=60.0, ymax=150.0).contour,)
    assert_close(gap(SQUARE, mark, "y"), 30.0)
    assert gap(SQUARE, mark, "x") == math.inf
    assert gap(SQUARE, transformed(mark, Transform().translate(0.0, -40.0)), "y") < 0.0


def test_column_ink_per_strip_along_each_axis() -> None:
    """Square across the left half of a box, two full and two empty columns, and the bottom half of a tall box, two full and two empty rows."""
    assert_close(column_ink(SQUARE, Box(xmin=0.0, ymin=0.0, xmax=200.0, ymax=100.0), 4), (1.0, 1.0, 0.0, 0.0))
    assert_close(column_ink(SQUARE, Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=200.0), 4, "y"), (1.0, 1.0, 0.0, 0.0))


def test_restroked_reaches_the_pen() -> None:
    """Stadium of pen 20 restroked to 30 and to 10, each mean stroke the pen it was given."""
    stadium = monoline(SEGMENT, 20.0)
    assert_close(tuple(stroke(restroked(stadium, pen)) for pen in (30.0, 10.0)), (30.0, 10.0), rel_tol=1e-2)
