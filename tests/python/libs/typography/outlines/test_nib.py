"""Broad-nib sweeps, extrusions and monoline strokes along straight, curved and closed skeletons."""

import math

from tests.python.support import assert_close
from typography.faces.glyphs import Outline, Point
from typography.outlines.nib import extruded, monoline, swept
from typography.outlines.paths import area, bounds, Box

# --- [CONSTANTS] ------------------------------------------------------------------------

SEGMENT: Outline = ((Point(x=0.0, y=0.0), Point(x=100.0, y=0.0)),)
ARC: Outline = ((Point(x=0.0, y=0.0), Point(x=100.0, y=100.0, on=False), Point(x=200.0, y=0.0)),)

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_swept_segment_is_parallelogram() -> None:
    """Horizontal segment under a nib 20 wide, upright and at 45 degrees."""
    assert_close(area(swept(SEGMENT, 20.0, 90.0)), 2000.0, rel_tol=1e-6)
    assert_close(area(swept(SEGMENT, 20.0, 45.0)), 2000.0 * math.sin(math.pi / 4), rel_tol=1e-6)
    assert_close(bounds(swept(SEGMENT, 20.0, 90.0)), Box(xmin=0.0, ymin=-10.0, xmax=100.0, ymax=10.0), abs_tol=1e-9)


def test_swept_arc_splits_at_nib_parallel_tangent() -> None:
    """Arch under a flat nib, its tangent turning through the nib at the apex, its ink the nib width times the vertical travel less the 10/3 both halves share over the apex."""
    ink = swept(ARC, 20.0, 0.0)
    box = bounds(ink)
    assert box is not None
    assert_close((box.xmin, box.xmax, box.ymax), (-10.0, 210.0, 50.0), abs_tol=1e-3)
    assert_close(area(ink), 20.0 * 100.0 - 10.0 / 3.0, rel_tol=1e-6)


def test_monoline_is_the_round_pen_stadium() -> None:
    """Segment stroked by a round pen 20 across, a rectangle with a half disc at each end."""
    assert_close(area(monoline(SEGMENT, 20.0)), 2000.0 + math.pi * 100.0, rel_tol=1e-3)


def test_swept_closed_skeleton_rings_its_path() -> None:
    """Square path swept closed by an upright nib 20 wide, a ring with one hole."""
    square = (Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0).contour,)
    ring = swept(square, 20.0, 90.0, closed=True)
    assert len(ring) == 2
    assert_close(bounds(ring), Box(xmin=0.0, ymin=-10.0, xmax=100.0, ymax=110.0), abs_tol=1e-9)


def test_extruded_square_is_its_minkowski_sum_with_the_vector() -> None:
    """Square extruded by (10, -10), its area grown by each edge pair the vector crosses."""
    shadow = extruded((Box(xmin=0.0, ymin=0.0, xmax=100.0, ymax=100.0).contour,), (10.0, -10.0))
    assert_close(area(shadow), 12000.0, rel_tol=1e-9)
    assert_close(bounds(shadow), Box(xmin=0.0, ymin=-10.0, xmax=110.0, ymax=100.0), abs_tol=1e-9)
