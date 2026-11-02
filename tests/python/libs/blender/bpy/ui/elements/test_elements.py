"""Interface elements registered and released through the host: panels at each placement kind, menus of every body, headers, appends, gizmo groups and custom gizmo types, toolbar tools, asset shelves, and the preferences page."""

from typing import Final

from expression import downcast, Error, Ok, Some

from tests.python.libs.blender.bpy.conftest import observing, Steps
lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [CONSTANTS] ------------------------------------------------------------------------

_PANEL_MEMBERS: Final = ("bl_space_type", "bl_region_type", "bl_context", "bl_category", "bl_parent_id", "bl_owner_id")
_OWNER: Final = "bl_ext.user_default.fixture"
_PANELS: Final = (
    ("FIXTURE_PT_main", ("VIEW_3D", "UI", None, "Fixture", None, _OWNER)),
    ("FIXTURE_PT_main_detail", ("VIEW_3D", "UI", None, "Fixture", "FIXTURE_PT_main", _OWNER)),
    ("FIXTURE_PT_main_scene_pt_unit", ("PROPERTIES", "WINDOW", "scene", None, None, _OWNER)),
    ("FIXTURE_PT_main_scene_pt_unit_detail", ("PROPERTIES", "WINDOW", "scene", None, "FIXTURE_PT_main_scene_pt_unit", _OWNER)),
    ("FIXTURE_PT_nested", ("PROPERTIES", "WINDOW", None, None, "RENDER_PT_format", _OWNER)),
    ("FIXTURE_PT_regioned", ("IMAGE_EDITOR", "TOOLS", None, None, None, _OWNER)),
    ("FIXTURE_PT_popped", ("TOPBAR", "HEADER", None, None, None, _OWNER)),
)
_MENUS: Final = (
    ("FIXTURE_MT_main", ("Main", "Fixture commands", ())),
    ("FIXTURE_MT_searched", ("Searched", "Searchable commands", ("SEARCH_ON_KEY_PRESS",))),
    ("FIXTURE_MT_grouped", ("Grouped", "Grouped commands", ())),
    ("FIXTURE_MT_grouped_first", ("First", "First: Grouped commands", ())),
    ("FIXTURE_MT_grouped_second", ("Second", "Second: Grouped commands", ())),
    ("FIXTURE_MT_pie", ("Pie", "Radial commands", ())),
)
_ELEMENTS: Final = ("enabled", "elements")
_CLASSES: Final = ("released", "elements", "released_classes")

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    *((((*_ELEMENTS, "panels", name, "some", member), value) for name, values in _PANELS for member, value in zip(_PANEL_MEMBERS, values, strict=True))),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_main", "some", "bl_label"), "Main"),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_main", "some", "bl_options"), ("DEFAULT_CLOSED",)),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_main_detail", "some", "bl_options"), ()),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_regioned", "some", "bl_options"), ("DEFAULT_CLOSED", "HIDE_HEADER")),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_main", "some", "bl_ui_units_x"), 12),
    ((*_ELEMENTS, "panels", "FIXTURE_PT_popped", "some", "bl_ui_units_x"), 10),
    ((*_ELEMENTS, "iconed", "sidebar icon"), True),
    *(((*_ELEMENTS, "iconed", "before", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_category"), ("VIEW_3D", "UI", "Fixture"), strict=True)),
    ((*_ELEMENTS, "iconed", "fired"), Ok(None)),
    *(((*_ELEMENTS, "iconed", "after", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_category"), ("IMAGE_EDITOR", "UI", None), strict=True)),
    *(((*_ELEMENTS, "menus", name, "some", member), value) for name, values in _MENUS for member, value in zip(("bl_label", "bl_description", "bl_options"), values, strict=True)),
    ((*_ELEMENTS, "headed", "some", "bl_space_type"), "VIEW_3D"),
    ((*_ELEMENTS, "headed", "some", "bl_region_type"), "HEADER"),
    ((*_ELEMENTS, "appended"), {"before": (False, False, False), "after": (True, True, True)}),
    (("released", "elements", "released_elements", "appended"), (False, False, False)),
    *((("released", "elements", "refusals", case), "RuntimeError") for case in ("menu section", "header section", "pie value", "unknown filter")),
    *(((*_ELEMENTS, "gizmos", "group", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_label", "bl_options"), ("VIEW_3D", "WINDOW", "Main", ()), strict=True)),
    ((*_ELEMENTS, "gizmos", "persistent", "some", "bl_options"), ("PERSISTENT",)),
    ((*_ELEMENTS, "gizmos", "callbacks"), ("invoke_prepare", "setup_keymap")),
    ((*_ELEMENTS, "gizmos", "shape"), ("offset",)),
    ((*_ELEMENTS, "gizmos", "ensured"), Ok(None)),
    ((*_ELEMENTS, "gizmos", "refused"), Error(("REFUSED",))),
    *(((*_CLASSES, name), False) for name in ("FIXTURE_GGT_main", "FIXTURE_GT_mark", "FIXTURE_HT_main", "FIXTURE_AST_main")),
    *(((*_CLASSES, name), False) for name, _ in (*_PANELS, ("FIXTURE_PT_docked", ()), *_MENUS)),
    ((*_ELEMENTS, "tooled", "activation"), Ok(("wm.tool_set_by_id", (("name", "fixture.probe"), ("space_type", "VIEW_3D"), ("cycle", False), ("as_fallback", False))))),
    ((*_ELEMENTS, "tooled", "ran"), Ok(("FINISHED",))),
    ((*_ELEMENTS, "tooled", "active"), Some("fixture.probe")),
    ((*_ELEMENTS, "tooled", "brush"), Ok("ValueError")),
    ((*_ELEMENTS, "tooled", "keymaps", 0, 0), "3D View Tool: Object, Probe"),
    ((*_ELEMENTS, "tooled", "keymaps", 0, 1, "event", 0), "LEFTMOUSE"),
    ((*_ELEMENTS, "tooled", "keymaps", 0, 1, "properties"), {"amount": 6}),
    (("released", "elements", "released_elements", "tools"), ()),
    *(
        ((*_ELEMENTS, "shelved", "some", member), value)
        for member, value in zip(("bl_space_type", "filter_mesh", "bl_options"), ("VIEW_3D", True, ("DEFAULT_VISIBLE", "NO_ASSET_DRAG", "STORE_ENABLED_CATALOGS_IN_PREFERENCES")), strict=True)
    ),
    ((*_ELEMENTS, "preferred", "properties"), Some(("ask_confirmed", "settings"))),
    ((*_ELEMENTS, "preferred", "read"), Ok(3)),
    ((*_ELEMENTS, "preferred", "show"), True),
    ((*_ELEMENTS, "preferred", "purge"), True),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_elements(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected


def test_tools_join_the_toolbar_after_their_anchor(observed: Observations) -> None:
    """First tool follows named stock tool behind a separator and opens a flyout group the second tool joins, binding one tool keymap."""
    toolbar = downcast(tuple, observed[*_ELEMENTS, "tooled", "toolbar"])
    group = toolbar.index(("group", ("fixture.probe", "fixture.probe_second")))
    assert ("tool", "builtin.transform") in toolbar[:group]
    assert toolbar[group - 1] == ("separator", None)
    assert len(downcast(tuple, observed[*_ELEMENTS, "tooled", "keymaps"])) == 1
