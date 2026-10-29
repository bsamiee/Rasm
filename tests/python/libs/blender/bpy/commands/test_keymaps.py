"""Add-on shortcuts: keymap items over placements in the host keyconfig-data form, their user counterparts, and keymap locations."""

import sys

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_bindings_add_items_with_their_values(observed: Observations) -> None:
    """Each binding adds an item pressing its event with its placement's values: a select role at the default keymap's own parameters, a pie pressing, and a Ctrl chord doubled as Cmd on macOS, each item with a user counterpart once the keyconfigs update."""
    counted = {"idname": "fixture.counted", "event": ("F", "PRESS", 0, 1, 0, 0), "properties": {"amount": 4}}
    doubled = ({**counted, "event": ("F", "PRESS", 0, 0, 0, 1)},) if sys.platform == "darwin" else ()
    items = (
        *doubled,
        counted,
        {"idname": "wm.context_toggle", "event": ("RIGHTMOUSE", "PRESS", 0, 0, 1, 0), "properties": {"data_path": "scene.use_gravity"}},
        {"idname": "wm.context_set_value", "event": ("G", "PRESS", 1, 0, 0, 0), "properties": {"data_path": "scene.frame_current", "value": "5"}},
        {"idname": "wm.call_menu_pie", "event": ("Q", "PRESS", 0, 0, 1, 0), "properties": {"name": "VIEW3D_MT_pivot_pie"}},
    )
    assert observed["commands.shortcuts"] == {"items": items, "counterparts": len(items)}


def test_keymap_names_locate_in_the_host_hierarchy(observed: Observations) -> None:
    """A stock keymap name locates its space and region, and an unknown name faults."""
    assert observed["commands.nested"] == {"view": Ok((("3D View", "VIEW_3D", "WINDOW"),)), "absent": Error(("ABSENT",))}


def test_release_removes_every_item(released: Observations) -> None:
    """No add-on item stays in the keymap after release."""
    assert released["commands.keys"] == ()
