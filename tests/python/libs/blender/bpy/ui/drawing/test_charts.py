"""Charts drawn offscreen in the scopes' well at a corner of the canvas."""

from typing import Final

import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

_MIDTONE: Final = 0.5

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("corner,edge", [("start", "left"), ("end", "right")])
def test_chart_draws_its_well_at_its_corner(observed: Observations, corner: str, edge: str) -> None:
    """A 40 by 30 plot draws its well one pixel wider each side, inset by the panel margin from the bottom and from its corner's side, its trace past midtone over the black well and dim text."""
    inset = int(observed.of(float, ("drawing.charted", "inset")))
    assert observed["drawing.charted", corner, "ok", "shape"] == (48, 64, 4)
    top, bottom, left, right = observed.of(tuple, ("drawing.charted", corner, "ok", "covered"))
    assert (bottom - top + 1, right - left + 1, bottom) == (32, 42, 48 - 1 - inset)
    assert {"left": left, "right": 64 - 1 - right}[edge] == inset
    assert observed.of(float, ("drawing.charted", corner, "ok", "brightest")) > _MIDTONE
