"""Panels registered at each placement kind: an extension Sidebar tab, beside a stock panel, under a stock parent, in an editor region, as a popover, and docked where a signal moves them."""

from expression import Ok
import pytest

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "name,space,region,context,category,parent",
    [
        ("FIXTURE_PT_main", "VIEW_3D", "UI", None, "Fixture", None),
        ("FIXTURE_PT_main_detail", "VIEW_3D", "UI", None, "Fixture", "FIXTURE_PT_main"),
        ("FIXTURE_PT_main_scene_pt_unit", "PROPERTIES", "WINDOW", "scene", None, None),
        ("FIXTURE_PT_main_scene_pt_unit_detail", "PROPERTIES", "WINDOW", "scene", None, "FIXTURE_PT_main_scene_pt_unit"),
        ("FIXTURE_PT_nested", "PROPERTIES", "WINDOW", None, None, "RENDER_PT_format"),
        ("FIXTURE_PT_regioned", "IMAGE_EDITOR", "TOOLS", None, None, None),
        ("FIXTURE_PT_popped", "TOPBAR", "HEADER", None, None, None),
    ],
)
def test_placements_register_their_host_members(observed: Observations, name: str, space: str, region: str, context: str | None, category: str | None, parent: str | None) -> None:
    """Each placement writes its editor, region, tab context, Sidebar category, and parent, a mirror copying the stock panel it sits beside and children following their parent."""
    read = ("bl_space_type", "bl_region_type", "bl_context", "bl_category", "bl_parent_id")
    assert tuple(observed["surfaces.panels", name, "some", member] for member in read) == (space, region, context, category, parent)
    assert observed["surfaces.panels", name, "some", "bl_owner_id"] == "bl_ext.user_default.fixture"


def test_panels_take_their_section_texts_and_options(observed: Observations) -> None:
    """Panels label as their section, start closed unless the section opens, add declared options, and size as popovers by their units."""
    assert (observed["surfaces.panels", "FIXTURE_PT_main", "some", "bl_label"], observed["surfaces.panels", "FIXTURE_PT_main", "some", "bl_options"]) == ("Main", ("DEFAULT_CLOSED",))
    assert observed["surfaces.panels", "FIXTURE_PT_main_detail", "some", "bl_options"] == ()
    assert observed["surfaces.panels", "FIXTURE_PT_regioned", "some", "bl_options"] == ("DEFAULT_CLOSED", "HIDE_HEADER")
    assert (observed["surfaces.panels", "FIXTURE_PT_main", "some", "bl_ui_units_x"], observed["surfaces.panels", "FIXTURE_PT_popped", "some", "bl_ui_units_x"]) == (12, 10)


def test_sidebar_tab_carries_the_brand_glyph(observed: Observations) -> None:
    """A Sidebar panel takes the extension's brand glyph as its tab icon."""
    assert observed["surfaces.iconed", "sidebar icon"] is True


def test_docked_panel_moves_when_its_signal_fires(observed: Observations) -> None:
    """A docked panel registers where its placement reads, and re-registers where the placement reads after its signal fires."""
    read = ("bl_space_type", "bl_region_type", "bl_category")
    assert tuple(observed["surfaces.iconed", "before", "some", member] for member in read) == ("VIEW_3D", "UI", "Fixture")
    assert observed["surfaces.iconed", "fired"] == Ok(None)
    assert tuple(observed["surfaces.iconed", "after", "some", member] for member in read) == ("IMAGE_EDITOR", "UI", None)


def test_release_removes_every_panel(released: Observations) -> None:
    """No panel class of any placement stays registered after release."""
    classes = released.of(dict, "surfaces.classes")
    assert [name for name, held in classes.items() if name.startswith("FIXTURE_PT_") and held] == []
