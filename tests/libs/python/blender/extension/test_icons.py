"""Text color a packaged icon takes from the widget it draws in under the Blender Light theme: menu and pie items by the menu drawing them, other buttons by their own widget, unembossed buttons and labels by their region, selected ones in their selected text."""

import bpy
from expression import NOTHING, Option, Some
import pytest

from rasm.extension.ui import Icons, Widget

# --- [CONSTANTS] ------------------------------------------------------------------------

_TOOL: Widget = lambda ui: ui.wcol_tool
_MENU: Widget = lambda ui: ui.wcol_menu
_TOOLBAR: Widget = lambda ui: ui.wcol_toolbar_item
_TOGGLE: Widget = lambda ui: ui.wcol_toggle

# --- [OPERATIONS] -----------------------------------------------------------------------


def _region(space: str, kind: str) -> tuple[bpy.types.Area, bpy.types.Region]:
    """Startup file's first editor of the space and its region of the kind."""
    area = next(area for screen in bpy.data.screens for area in screen.areas if area.type == space)
    return area, next(region for region in area.regions if region.type == kind)


@pytest.mark.parametrize(
    "emboss,space,kind,widget,selected,gray",
    [
        pytest.param("PULLDOWN_MENU", "VIEW_3D", "WINDOW", Some(_TOOL), False, 0x1A, id="menu-item"),
        pytest.param("PULLDOWN_MENU", "VIEW_3D", "WINDOW", Some(_TOOL), True, 0xFF, id="menu-item-selected"),
        pytest.param("PIE_MENU", "VIEW_3D", "WINDOW", Some(_TOOL), False, 0xD9, id="pie-item"),
        pytest.param("NORMAL", "VIEW_3D", "UI", Some(_MENU), False, 0xD9, id="dropdown"),
        pytest.param("NORMAL", "VIEW_3D", "TOOLS", Some(_TOOLBAR), False, 0xE6, id="toolbar-item"),
        pytest.param("NORMAL", "VIEW_3D", "UI", Some(_TOOL), False, 0x00, id="operator"),
        pytest.param("NORMAL", "VIEW_3D", "UI", Some(_TOGGLE), True, 0xFF, id="toggle-on"),
        pytest.param("NORMAL", "VIEW_3D", "UI", NOTHING, False, 0x1A, id="sidebar-label"),
        pytest.param("NONE", "IMAGE_EDITOR", "WINDOW", Some(_MENU), False, 0x00, id="unembossed"),
        pytest.param("NONE", "IMAGE_EDITOR", "WINDOW", Some(_MENU), True, 0xFF, id="unembossed-selected"),
    ],
)
def test_an_icon_takes_the_text_color_of_the_widget_it_draws_in(light: bpy.types.ThemeUserInterface, *, emboss: str, space: str, kind: str, widget: Option[Widget], selected: bool, gray: int) -> None:
    """Each button's tint is the Blender Light preset's text, or selected text, of the widget Blender draws that button as."""
    assert light.wcol_menu.text[0] == pytest.approx(0xD9 / 0xFF)
    _, color = Icons.tint(emboss, *_region(space, kind), widget, selected=selected)
    assert color == pytest.approx((gray / 0xFF, gray / 0xFF, gray / 0xFF, 1.0), abs=1e-6)


def test_buttons_drawing_as_one_widget_state_share_a_place(light: bpy.types.ThemeUserInterface) -> None:
    """Menu items share one place whatever their own widget, while a dropdown of the same color and a selected menu item hold their own."""
    assert light.wcol_pie_menu.text[0] == pytest.approx(light.wcol_menu.text[0])
    window, sidebar = _region("VIEW_3D", "WINDOW"), _region("VIEW_3D", "UI")
    item, _ = Icons.tint("PULLDOWN_MENU", *window, Some(_TOOL), selected=False)
    other, _ = Icons.tint("PULLDOWN_MENU", *window, Some(_MENU), selected=False)
    selected, _ = Icons.tint("PULLDOWN_MENU", *window, Some(_TOOL), selected=True)
    pie, _ = Icons.tint("PIE_MENU", *window, Some(_TOOL), selected=False)
    dropdown, _ = Icons.tint("NORMAL", *sidebar, Some(_MENU), selected=False)
    assert item == other
    assert len({item, selected, pie, dropdown}) == 4
