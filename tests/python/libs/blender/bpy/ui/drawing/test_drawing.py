"""Canvas text, charts, the transparency checker, cursor canvases, offscreen reads, blend scopes, draw handles, and the refresh sources of drawings."""

from typing import Final

from expression import downcast, Error, Nothing, Ok, Some
import pytest

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

_MIDTONE: Final = 0.5
_GREEN: Final = (0.0, 1.0, 0.0, 1.0)

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    (("enabled", "drawing", "written", "longer"), True),
    (("enabled", "drawing", "written", "wrapped"), True),
    (("enabled", "drawing", "written", "label"), Ok(1.0)),
    (("enabled", "drawing", "written", "checker"), Ok(((0.0, 0.0, 0.0, 1.0), (1.0, 1.0, 1.0, 1.0)))),
    (("enabled", "drawing", "pointed", "pointer"), Some((5, 7))),
    (("enabled", "drawing", "pointed", "plain"), Nothing),
    (("enabled", "drawing", "read_back", "corner"), Ok(((3, 2, 4), _GREEN))),
    (("enabled", "drawing", "read_back", "whole"), Ok(((8, 8, 4), _GREEN))),
    (("enabled", "drawing", "read_back", "blend"), ("ADDITIVE", "NONE")),
    (("enabled", "drawing", "read_back", "past limit"), Error(("OUTSIDE",))),
    (("enabled", "drawing", "handled"), Ok(None)),
    (("enabled", "drawing", "signalled", "inside"), Ok(None)),
    (("enabled", "drawing", "signalled", "after"), Ok(None)),
    (("enabled", "drawing", "signalled", "refreshes"), ("fired",)),
    (("enabled", "drawing", "signalled", "sources"), Ok(None)),
    (("enabled", "drawing", "signalled", "absent"), Error(("ABSENT",))),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_drawing(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected


def test_text_measures_a_positive_extent(observed: Observations) -> None:
    """Text measures a positive width and height."""
    width, height = downcast(tuple, observed["enabled", "drawing", "written", "single"])
    assert (width > 0, height > 0) == (True, True)


def test_cursor_canvas_moves_into_window_pixels(observed: Observations) -> None:
    """A canvas given a pointer moves its extent by the region origin into window pixels."""
    assert observed["enabled", "drawing", "pointed", "offset"] == observed["enabled", "drawing", "pointed", "origin"]


@pytest.mark.parametrize("corner,edge", [("start", "left"), ("end", "right")])
def test_chart_draws_its_well_at_its_corner(observed: Observations, corner: str, edge: str) -> None:
    """A 40 by 30 plot draws its well one pixel wider each side, inset by the panel margin from the bottom and from its corner's side, its trace past midtone over the black well and dim text."""
    inset = int(downcast(float, observed["enabled", "drawing", "charted", "inset"]))
    read = ("enabled", "drawing", "charted", corner, "ok")
    assert observed[*read, "shape"] == (48, 64, 4)
    top, bottom, left, right = downcast(tuple, observed[*read, "covered"])
    assert (bottom - top + 1, right - left + 1, bottom) == (32, 42, 48 - 1 - inset)
    assert {"left": left, "right": 64 - 1 - right}[edge] == inset
    assert downcast(float, observed[*read, "brightest"]) > _MIDTONE
