"""Region overlap and visible bounds by the host's own rules, region visibility as revertable editor state, alignment flips, quad view, and View2D coordinates."""

from expression import Error, Nothing, Ok, Option
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "pair,on,off",
    [
        ("VIEW_3D UI", True, False),
        ("VIEW_3D TOOLS", True, False),
        ("VIEW_3D HUD", True, True),
        ("NODE_EDITOR UI", True, False),
        ("IMAGE_EDITOR TOOL_HEADER", True, False),
        ("PROPERTIES UI", False, False),
        ("SEQUENCE_EDITOR UI", False, False),
    ],
)
def test_regions_overlap_by_editor_and_preference(observed: Observations, pair: str, *, on: bool, off: bool) -> None:
    """Sidebars and toolbars of overlapping editors overlap while region overlap is on, the HUD always, and other editors' regions never."""
    assert (observed["screens.overlapping", "on", pair], observed["screens.overlapping", "off", pair]) == (on, off)


def test_visible_bounds_exclude_overlapping_siblings(observed: Observations) -> None:
    """The main region's visible bounds start past the overlapping toolbar and below the overlapping headers, and cover the whole region without overlap."""
    x, y, width, height = observed.of(tuple, ("screens.bounded", "overlapping"))
    _, _, region_width, region_height = observed.of(tuple, ("screens.bounded", "region"))
    _, _, toolbar_width, _ = observed.of(tuple, ("screens.bounded", "toolbar"))
    assert observed["screens.bounded", "alone"] == (0, 0, region_width, region_height)
    assert (x, y, x + width) == (toolbar_width - 1, 0, region_width)
    assert 0 < height < region_height


def test_region_visibility_applies_and_reverts(observed: Observations) -> None:
    """Showing the Sidebar writes its member and answers the state it replaced, which restores it, and region types no member toggles or the editor lacks fault."""
    assert observed["screens.shown"] == {"states": (False, True, False), "prior": Ok(True), "restored": Ok(True), "main": Error(("ABSENT",)), "channels": Error(("ABSENT",))}


def test_regions_flip_across_their_axis(observed: Observations) -> None:
    """A toolbar flips to the other side and back, its own side changes nothing, a header refuses a side across its axis, and an unlocated region faults."""
    assert observed["screens.flips"] == {"same": Ok(None), "across": Ok("RIGHT"), "back": Ok("LEFT"), "crossing": Error(("OUTSIDE",)), "unlocated": Error(("ABSENT",))}


def test_quad_view_toggles_to_the_asked_state(observed: Observations) -> None:
    """Quad view shows its four regions and hides them again."""
    assert observed["screens.quads"] == {"shown": Ok(4), "hidden": Ok(0)}


def test_coordinates_convert_through_view2d(observed: Observations) -> None:
    """Region pixels convert to editor coordinates and back within the host's integer rounding, a point outside the region and an unlocated region read nothing."""
    column, row = observed.of(Option, ("screens.converted", "back")).value
    assert max(abs(column - 40), abs(row - 20)) <= 1
    assert (observed["screens.converted", "outside"], observed["screens.converted", "unlocated"]) == (Nothing, Nothing)
