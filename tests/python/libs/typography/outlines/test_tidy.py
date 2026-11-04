"""Cleanup removes narrow defects while retaining intended ink and coordinate groups."""

from pathops import PathOp
import pytest

from typography.faces.glyphs import Outline, Point
from typography.outlines.paths import area, bounds, Box, combined
from typography.outlines.tidy import despiked, snapped, tidied

# --- [CONSTANTS] ------------------------------------------------------------------------

SQUARE: Outline = (Box(xmin=0, ymin=0, xmax=100, ymax=100).contour,)
RETRACED: Outline = ((Point(x=0, y=0), Point(x=50, y=0), Point(x=50, y=-40), Point(x=50, y=0), Point(x=100, y=0), Point(x=100, y=100), Point(x=0, y=100)),)
NEEDLED: Outline = ((Point(x=0, y=0), Point(x=45, y=0), Point(x=50, y=-50), Point(x=55, y=0), Point(x=100, y=0), Point(x=100, y=100), Point(x=0, y=100)),)
BOWL: Outline = ((Point(x=0, y=0), Point(x=50, y=-20, on=False), Point(x=100, y=0), Point(x=100, y=100), Point(x=0, y=100)),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_retraced_spurs_and_thin_slivers_leave_the_intended_fill() -> None:
    """Cleanup restores square ink rather than merely retaining its bounding box."""
    sliver = Box(xmin=200, ymin=0, xmax=300, ymax=1).contour
    cleaned = tidied((*RETRACED, sliver), merge=1, corner=1, sliver=2)
    assert len(cleaned) == 1
    assert bounds(cleaned) == bounds(SQUARE)
    assert area(combined(cleaned, SQUARE, PathOp.XOR)) == 0
    assert area(combined(tidied(cleaned, merge=1, corner=1, sliver=2), cleaned, PathOp.XOR)) == 0


def test_near_points_and_collinear_vertices_merge_without_losing_corners() -> None:
    """Near and collinear bottom points collapse to four square corners."""
    crowded = ((Point(x=0, y=0), Point(x=0.4, y=0), Point(x=50, y=0), Point(x=100, y=0), Point(x=100, y=100), Point(x=0, y=100)),)
    cleaned = tidied(crowded, merge=1, corner=1, sliver=2)
    assert len(cleaned[0]) == 4
    assert area(combined(cleaned, SQUARE, PathOp.XOR)) == 0


def test_join_band_cuts_needles_and_preserves_round_overshoots() -> None:
    """A narrow needle is cut at the baseline while a broad curved bowl retains its overshoot."""
    cut, kept = despiked(NEEDLED, (0, 40)), bounds(despiked(BOWL, (0, 40)))
    assert area(combined(cut, SQUARE, PathOp.XOR)) == 0
    assert kept is not None
    assert kept.ymin == pytest.approx(-10, rel=1e-9, abs=1e-6)


def test_coordinate_snapping_preserves_groups_and_bounds_displacement() -> None:
    """Separated coordinate runs meet at their means and move no point past the tolerance."""
    skewed = ((Point(x=0, y=0), Point(x=100, y=0.4), Point(x=99.6, y=100), Point(x=0.4, y=99.8)),)
    aligned = snapped(skewed, 1)
    assert {(point.x, point.y) for point in aligned[0]} == {(0.2, 0.2), (99.8, 0.2), (99.8, 99.9), (0.2, 99.9)}
    assert all(abs(old.x - new.x) <= 1 and abs(old.y - new.y) <= 1 for old, new in zip(skewed[0], aligned[0], strict=True))
    assert snapped(aligned, 1) == aligned
