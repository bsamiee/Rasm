"""Located editors, editor queries, arrangements applied one host pass per step and reverted, region overlap and visible bounds by the host's own rules, region visibility, alignment flips, quad view, View2D coordinates, and owned workspaces."""

from typing import Final

from expression import downcast, Error, Nothing, Ok, Option, Some
import pytest

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

_ABSENT: Final = Error(("ABSENT",))

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    (("enabled", "screens", "located", "current"), _ABSENT),
    (("enabled", "screens", "located", "view"), Ok("VIEW_3D")),
    (("enabled", "screens", "located", "image"), _ABSENT),
    (("enabled", "screens", "located", "main"), Ok("WINDOW")),
    (("enabled", "screens", "located", "preview"), _ABSENT),
    (("enabled", "screens", "located", "within"), Ok(Some("HEADER"))),
    (("enabled", "screens", "retyped"), {"before": "TIMELINE", "applied": "FCURVES", "settled": Ok(None), "reverted": Ok(None), "after": "TIMELINE"}),
    (("enabled", "screens", "split", "reverts"), Ok(1)),
    (("enabled", "screens", "shared"), "ValueError"),
    *(
        (("enabled", "screens", "overlapping", state, pair), held)
        for pair, on, off in (
            ("VIEW_3D UI", True, False),
            ("VIEW_3D TOOLS", True, False),
            ("VIEW_3D HUD", True, True),
            ("NODE_EDITOR UI", True, False),
            ("IMAGE_EDITOR TOOL_HEADER", True, False),
            ("PROPERTIES UI", False, False),
            ("SEQUENCE_EDITOR UI", False, False),
        )
        for state, held in (("on", on), ("off", off))
    ),
    (("enabled", "screens", "shown"), {"states": (False, True, False), "prior": Ok(True), "restored": Ok(True), "main": _ABSENT, "channels": _ABSENT}),
    (("enabled", "screens", "flips"), {"same": Ok(None), "across": Ok("RIGHT"), "back": Ok("LEFT"), "crossing": Error(("OUTSIDE",)), "unlocated": _ABSENT}),
    (("enabled", "screens", "quads"), {"shown": Ok(4), "hidden": Ok(0)}),
    (("enabled", "screens", "converted", "outside"), Nothing),
    (("enabled", "screens", "converted", "unlocated"), Nothing),
    (("enabled", "screens", "workspaced", "made"), Ok(("Fixture Layout", "OBJECT", True, True, ("bl_ext.user_default.fixture",)))),
    (("enabled", "screens", "workspaced", "again"), Ok(True)),
    (("enabled", "screens", "workspaced", "pending"), Ok(True)),
    (("enabled", "screens", "workspaced", "reverted"), Ok(None)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_screens(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected


def test_areas_list_screen_then_global_areas_largest_query_first(observed: Observations) -> None:
    """Every screen area lists before the global top bar and status bar, and matching editors list from the largest area down."""
    located = ("enabled", "screens", "located")
    assert observed[*located, "areas"] == (*downcast(tuple, observed[*located, "screen"]), "TOPBAR", "STATUSBAR")
    sizes = downcast(tuple, observed[*located, "sizes"])
    assert sizes == tuple(sorted(sizes, reverse=True))


def test_split_adds_the_editor_at_its_edge(observed: Observations) -> None:
    """A right split adds a Text Editor right of the source area at its share."""
    width, before = downcast(tuple, observed["enabled", "screens", "split", "before"])
    _, after = downcast(tuple, observed["enabled", "screens", "split", "after"])
    _, text_x = downcast(tuple, observed["enabled", "screens", "split", "x"])
    text_width = downcast(int, observed["enabled", "screens", "split", "text"])
    assert after == (*before, "TEXT_EDITOR")
    assert (abs(text_width - 0.25 * width) <= 1, text_x + text_width) == (True, width)


def test_visible_bounds_exclude_overlapping_siblings(observed: Observations) -> None:
    """The main region's visible bounds start past the overlapping toolbar and below the overlapping headers, and cover the whole region without overlap."""
    x, y, width, height = downcast(tuple, observed["enabled", "screens", "bounded", "overlapping"])
    _, _, region_width, region_height = downcast(tuple, observed["enabled", "screens", "bounded", "region"])
    _, _, toolbar_width, _ = downcast(tuple, observed["enabled", "screens", "bounded", "toolbar"])
    assert observed["enabled", "screens", "bounded", "alone"] == (0, 0, region_width, region_height)
    assert (x, y, x + width) == (toolbar_width - 1, 0, region_width)
    assert 0 < height < region_height


@pytest.mark.parametrize("axis", [0, 1])
def test_coordinates_convert_through_view2d(observed: Observations, axis: int) -> None:
    """Region pixels convert to editor coordinates and back within the host's integer rounding."""
    point = downcast(Option, observed["enabled", "screens", "converted", "back"]).value
    assert abs(point[axis] - (40, 20)[axis]) <= 1
