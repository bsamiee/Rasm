"""Spike, sliver, and near-point cleanup, needle cuts under a join band, and coordinate snapping."""

from tests.python.support import assert_close
from typography.faces.glyphs import Outline, Point
from typography.outlines.paths import area, bounds, Box
from typography.outlines.tidy import despiked, snapped, tidied

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0).contour,)
RETRACED: Outline = ((Point(x=0.0, y=0.0), Point(x=50.0, y=0.0), Point(x=50.0, y=-40.0), Point(x=50.0, y=0.0), Point(x=100.0, y=0.0), Point(x=100.0, y=100.0), Point(x=0.0, y=100.0)),)
NEEDLED: Outline = ((Point(x=0.0, y=0.0), Point(x=45.0, y=0.0), Point(x=50.0, y=-50.0), Point(x=55.0, y=0.0), Point(x=100.0, y=0.0), Point(x=100.0, y=100.0), Point(x=0.0, y=100.0)),)
BOWL: Outline = ((Point(x=0.0, y=0.0), Point(x=50.0, y=-20.0, on=False), Point(x=100.0, y=0.0), Point(x=100.0, y=100.0), Point(x=0.0, y=100.0)),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_tidied_removes_retraced_spike() -> None:
    """Square whose bottom edge runs out and back along one line, the spur gone."""
    assert bounds(tidied(RETRACED, merge=1.0, corner=1.0, sliver=2.0)) == Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0)


def test_tidied_drops_slivers_and_merges_points() -> None:
    """Square beside a one-unit sliver, its edges holding near and collinear points, back to four corners."""
    crowded = ((Point(x=0.0, y=0.0), Point(x=0.4, y=0.0), Point(x=50.0, y=0.0), Point(x=100.0, y=0.0), Point(x=100.0, y=100.0), Point(x=0.0, y=100.0)),)
    sliver = (Box(xmin=200.0, ymin=0.0, xmax=300.0, ymax=1.0).contour,)
    tidy = tidied((*crowded, *sliver), merge=1.0, corner=1.0, sliver=2.0)
    assert len(tidy) == 1
    assert len(tidy[0]) == 4
    assert_close(area(tidy), 10000.0)


def test_despiked_cuts_needle_and_keeps_overshoot() -> None:
    """Needle 10 wide under a band from 0 to 40 cut at the band foot, a bowl 100 wide kept."""
    assert bounds(despiked(NEEDLED, (0.0, 40.0))) == Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0)
    kept = bounds(despiked(BOWL, (0.0, 40.0)))
    assert kept is not None
    assert_close(kept.ymin, -10.0, abs_tol=1e-6)


def test_snapped_moves_near_coordinates_onto_one_grid_line() -> None:
    """Quadrilateral with corners within half a unit of a square, each axis snapped to two shared lines."""
    skewed = ((Point(x=0.0, y=0.0), Point(x=100.0, y=0.4), Point(x=99.6, y=100.0), Point(x=0.4, y=99.8)),)
    assert {(point.x, point.y) for point in snapped(skewed, 1.0)[0]} == {(0.2, 0.2), (99.8, 0.2), (99.8, 99.9), (0.2, 99.9)}
