"""Analytic geometry and stroke fitting over thin, holed, and disconnected ink."""

import math

from fontTools.misc.transform import Transform
from pathops import PathOp
import pytest

from typography.faces.glyphs import Outline, Point
from typography.outlines.measure import column_ink, counter, gap, inradius, moments, perimeter, restroked, stroke
from typography.outlines.nib import monoline
from typography.outlines.paths import area, Axis, Box, combined, transformed

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0, ymin=0, xmax=100, ymax=100).contour,)
BAR: Outline = (Box(xmin=0, ymin=0, xmax=100, ymax=20).contour,)
SEGMENT: Outline = ((Point(x=0, y=0), Point(x=200, y=0)),)
STADIUM = monoline(SEGMENT, 20)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_rectangle_and_stadium_measurements_follow_analytic_geometry() -> None:
    """Area and perimeter determine mean stroke for flat and round-ended bars."""
    assert perimeter(BAR) == pytest.approx(240, rel=1e-9, abs=0)
    assert stroke(BAR) == pytest.approx(4000 / 240, rel=1e-9, abs=0)
    assert stroke(STADIUM) == pytest.approx(2 * (4000 + math.pi * 100) / (400 + 20 * math.pi), rel=1e-3, abs=0)


def test_counter_and_inradius_distinguish_holes_from_filled_ink() -> None:
    """A stadium counter has its pen diameter while filled square ink has no counter."""
    slotted = combined((Box(xmin=-50, ymin=-50, xmax=250, ymax=50).contour,), STADIUM, PathOp.DIFFERENCE)
    assert counter(slotted) == pytest.approx(20, rel=1e-2, abs=0)
    assert counter(SQUARE) == 0
    assert inradius(SQUARE) == pytest.approx(50, rel=1e-9, abs=0)
    assert stroke(()) == counter(()) == inradius(()) == 0


def test_counter_measures_the_bounded_channel_around_an_ink_island() -> None:
    """The square channel's largest corner circle has diameter twenty times two minus the square root of two."""
    nested = (*SQUARE, tuple(reversed(Box(xmin=10, ymin=10, xmax=90, ymax=90).contour)), Box(xmin=20, ymin=20, xmax=80, ymax=80).contour)
    assert counter(nested) == pytest.approx(20 * (2 - math.sqrt(2)), rel=0, abs=0.5)


def test_moments_rotate_with_the_ink_about_its_center() -> None:
    """A bar keeps its center while its principal axis follows a thirty-degree rotation."""
    rotated = moments(transformed(BAR, Transform().translate(50, 10).rotate(math.pi / 6).translate(-50, -10)))
    assert rotated.centre == pytest.approx((50, 10), rel=1e-9, abs=1e-6)
    assert rotated.angle == pytest.approx(30, rel=1e-6, abs=0)


@pytest.mark.parametrize("dx,dy,expected", [(130, 0, 30), (130, 130, 30 * math.sqrt(2)), (100, 0, 0), (90, 0, -5)])
def test_signed_gaps_are_symmetric_and_translation_invariant(dx: float, dy: float, expected: float) -> None:
    """Disjoint gaps, touching ink, and intersection inradius retain the same geometry after translation."""
    other, shift = transformed(SQUARE, Transform().translate(dx, dy)), Transform().translate(-250, 300)
    assert gap(SQUARE, other) == pytest.approx(expected, rel=1e-9, abs=0)
    assert gap(other, SQUARE) == pytest.approx(expected, rel=1e-9, abs=0)
    assert gap(transformed(SQUARE, shift), transformed(other, shift)) == pytest.approx(expected, rel=1e-9, abs=0)


def test_axis_gaps_exclude_ink_that_never_faces() -> None:
    """A mark faces the base vertically while horizontal and empty separations are infinite."""
    mark = (Box(xmin=40, ymin=130, xmax=60, ymax=150).contour,)
    assert gap(SQUARE, mark, "y") == pytest.approx(30, rel=1e-9, abs=0)
    assert gap(SQUARE, mark, "x") == gap((), mark) == math.inf
    assert gap(SQUARE, transformed(mark, Transform().translate(0, -40)), "y") < 0


@pytest.mark.parametrize("axis,box", [("x", Box(xmin=0, ymin=0, xmax=200, ymax=100)), ("y", Box(xmin=0, ymin=0, xmax=100, ymax=200))])
def test_strip_coverage_conserves_ink(axis: Axis, box: Box) -> None:
    """Strip fractions cover half a box and reconstruct the measured ink area."""
    fractions = column_ink(SQUARE, box, 4, axis)
    assert fractions == pytest.approx((1, 1, 0, 0), rel=1e-9, abs=0)
    assert sum(fractions) * box.width * box.height / len(fractions) == pytest.approx(area(SQUARE), rel=1e-9, abs=0)


@pytest.mark.parametrize("ink,pen,relative", [(BAR, 10, 1e-3), (SQUARE, 30, 1e-3), ((Box(xmin=0, ymin=0, xmax=1000, ymax=1).contour,), 2, 1e-3), (STADIUM, 10, 1e-2), (STADIUM, 30, 1e-2)])
def test_stroke_fit_retains_ink_and_reduces_target_error(ink: Outline, pen: float, relative: float) -> None:
    """Offset fitting improves finite targets without erasing long thin ink on an empty search plateau."""
    fitted = restroked(ink, pen)
    assert area(fitted) > 0
    assert stroke(fitted) == pytest.approx(pen, rel=relative, abs=0)


def test_stroke_fit_handles_holes_and_competing_disconnected_extents() -> None:
    """Topology changes retain finite ink with smaller mean-stroke error than the original."""
    ring = (*SQUARE, tuple(reversed(Box(xmin=20, ymin=20, xmax=80, ymax=80).contour)))
    disconnected = (*BAR, Box(xmin=500, ymin=0, xmax=501, ymax=1000).contour)
    for ink in (ring, disconnected):
        target = stroke(ink) / 2
        fitted = restroked(ink, target)
        assert area(fitted) > 0
        assert abs(stroke(fitted) - target) < abs(stroke(ink) - target)
    assert restroked((), 20) == ()
