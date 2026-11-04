"""Nib sweeps and extrusions obey analytic area and composition laws."""

import math

from pathops import PathOp
import pytest

from typography.faces.glyphs import Outline, Point
from typography.outlines.nib import extruded, monoline, swept
from typography.outlines.paths import area, bounds, Box, combined, pieces

# --- [CONSTANTS] ------------------------------------------------------------------------

SEGMENT: Outline = ((Point(x=0, y=0), Point(x=100, y=0)),)
ARC: Outline = ((Point(x=0, y=0), Point(x=100, y=100, on=False), Point(x=200, y=0)),)
SQUARE: Outline = (Box(xmin=0, ymin=0, xmax=100, ymax=100).contour,)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("angle", [0, 45, 90])
def test_straight_nib_sweeps_have_parallelogram_area(angle: float) -> None:
    """Sweep area equals segment length times the nib's perpendicular projection."""
    ink = swept(SEGMENT, 20, angle)
    assert area(ink) == pytest.approx(100 * 20 * abs(math.sin(math.radians(angle))), rel=1e-6, abs=0)
    if angle == 90:
        box = bounds(ink)
        assert box is not None
        assert (box.xmin, box.ymin, box.xmax, box.ymax) == pytest.approx((0, -10, 100, 10), rel=1e-9, abs=1e-9)


def test_curved_sweep_splits_at_a_parallel_tangent() -> None:
    """A flat nib crossing an arch apex removes overlap between its folded halves."""
    ink = swept(ARC, 20, 0)
    box = bounds(ink)
    assert box is not None
    assert (box.xmin, box.xmax, box.ymax) == pytest.approx((-10, 210, 50), rel=1e-9, abs=1e-3)
    assert area(ink) == pytest.approx(20 * 100 - 10 / 3, rel=1e-6, abs=0)


def test_round_caps_and_closed_nib_sweeps_keep_their_true_topology() -> None:
    """Round caps add one disc; an upright nib leaves two bars while a diagonal nib closes a ring."""
    assert area(monoline(SEGMENT, 20)) == pytest.approx(100 * 20 + math.pi * 10**2, rel=1e-3, abs=0)
    bars = swept(SQUARE, 20, 90, closed=True)
    separated = pieces(bars)
    assert tuple(len(piece) for piece in separated) == (1, 1)
    assert sorted(area(piece) for piece in separated) == pytest.approx((2000, 2000), rel=1e-9, abs=0)
    box = bounds(bars)
    assert box is not None
    assert (box.xmin, box.ymin, box.xmax, box.ymax) == pytest.approx((0, -10, 100, 110), rel=1e-9, abs=1e-9)
    assert tuple(len(piece) for piece in pieces(swept(SQUARE, 20, 45, closed=True))) == (2,)


@pytest.mark.parametrize("vector", [(10, -10), (-30, 20), (0, 0), (150, 200)])
def test_extrusion_obeys_minkowski_area_and_collinear_composition(vector: tuple[float, float]) -> None:
    """Square extrusion grows by crossed edge projections and repeated vectors compose."""
    shadow = extruded(SQUARE, vector)
    assert area(shadow) == pytest.approx(100**2 + 100 * sum(map(abs, vector)), rel=1e-9, abs=0)
    assert bounds(shadow) == Box(xmin=min(0, vector[0]), ymin=min(0, vector[1]), xmax=100 + max(0, vector[0]), ymax=100 + max(0, vector[1]))
    twice = extruded(shadow, vector)
    direct = extruded(SQUARE, (2 * vector[0], 2 * vector[1]))
    assert area(combined(twice, direct, PathOp.XOR)) == 0
