"""Surfaces registered and released through the host: panels at each placement kind, menus of every body, headers, appends, gizmo groups and custom gizmo types, toolbar tools, asset shelves, and the preferences page."""

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
_SURFACES: Final = ("enabled", "surfaces")
_CLASSES: Final = ("released", "surfaces", "released_classes")

CASES: Final[tuple[tuple[Steps, object], ...]] = (
    *((((*_SURFACES, "panels", name, "some", member), value) for name, values in _PANELS for member, value in zip(_PANEL_MEMBERS, values, strict=True))),
    ((*_SURFACES, "panels", "FIXTURE_PT_main", "some", "bl_label"), "Main"),
    ((*_SURFACES, "panels", "FIXTURE_PT_main", "some", "bl_options"), ("DEFAULT_CLOSED",)),
    ((*_SURFACES, "panels", "FIXTURE_PT_main_detail", "some", "bl_options"), ()),
    ((*_SURFACES, "panels", "FIXTURE_PT_regioned", "some", "bl_options"), ("DEFAULT_CLOSED", "HIDE_HEADER")),
    ((*_SURFACES, "panels", "FIXTURE_PT_main", "some", "bl_ui_units_x"), 12),
    ((*_SURFACES, "panels", "FIXTURE_PT_popped", "some", "bl_ui_units_x"), 10),
    ((*_SURFACES, "iconed", "sidebar icon"), True),
    *(((*_SURFACES, "iconed", "before", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_category"), ("VIEW_3D", "UI", "Fixture"), strict=True)),
    ((*_SURFACES, "iconed", "fired"), Ok(None)),
    *(((*_SURFACES, "iconed", "after", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_category"), ("IMAGE_EDITOR", "UI", None), strict=True)),
    *(((*_SURFACES, "menus", name, "some", member), value) for name, values in _MENUS for member, value in zip(("bl_label", "bl_description", "bl_options"), values, strict=True)),
    ((*_SURFACES, "headed", "some", "bl_space_type"), "VIEW_3D"),
    ((*_SURFACES, "headed", "some", "bl_region_type"), "HEADER"),
    ((*_SURFACES, "appended"), {"before": (False, False, False), "now": (True, True, True)}),
    (("released", "surfaces", "released_surfaces", "appended"), (False, False, False)),
    *((("released", "surfaces", "refusals", case), "RuntimeError") for case in ("menu section", "header section", "pie value", "unknown filter")),
    *(((*_SURFACES, "gizmos", "group", "some", member), value) for member, value in zip(("bl_space_type", "bl_region_type", "bl_label", "bl_options"), ("VIEW_3D", "WINDOW", "Main", ()), strict=True)),
    ((*_SURFACES, "gizmos", "persistent", "some", "bl_options"), ("PERSISTENT",)),
    ((*_SURFACES, "gizmos", "callbacks"), ("invoke_prepare", "setup_keymap")),
    ((*_SURFACES, "gizmos", "shape"), ("offset",)),
    ((*_SURFACES, "gizmos", "ensured"), Ok(None)),
    ((*_SURFACES, "gizmos", "refused"), Error(("REFUSED",))),
    *(((*_CLASSES, name), False) for name in ("FIXTURE_GGT_main", "FIXTURE_GT_mark", "FIXTURE_HT_main", "FIXTURE_AST_main")),
    *(((*_CLASSES, name), False) for name, _ in (*_PANELS, ("FIXTURE_PT_docked", ()), *_MENUS)),
    ((*_SURFACES, "tooled", "activation"), Ok(("wm.tool_set_by_id", (("name", "fixture.probe"), ("space_type", "VIEW_3D"))))),
    ((*_SURFACES, "tooled", "ran"), Ok(("FINISHED",))),
    ((*_SURFACES, "tooled", "active"), Some("fixture.probe")),
    ((*_SURFACES, "tooled", "brush"), Ok("ValueError")),
    ((*_SURFACES, "tooled", "keymaps", 0, 0), "3D View Tool: Object, Probe"),
    ((*_SURFACES, "tooled", "keymaps", 0, 1, "event", 0), "LEFTMOUSE"),
    ((*_SURFACES, "tooled", "keymaps", 0, 1, "properties"), {"amount": 6}),
    (("released", "surfaces", "released_surfaces", "tools"), ()),
    *(
        ((*_SURFACES, "shelved", "some", member), value)
        for member, value in zip(("bl_space_type", "filter_mesh", "bl_options"), ("VIEW_3D", True, ("DEFAULT_VISIBLE", "NO_ASSET_DRAG", "STORE_ENABLED_CATALOGS_IN_PREFERENCES")), strict=True)
    ),
    ((*_SURFACES, "preferred", "properties"), Some(("ask_confirmed", "hidden", "level"))),
    ((*_SURFACES, "preferred", "read"), Ok(3)),
    ((*_SURFACES, "preferred", "value"), Ok(3)),
    ((*_SURFACES, "preferred", "show"), True),
    ((*_SURFACES, "preferred", "purge"), True),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@observing(CASES)
def test_surfaces(observed: Observations, path: Steps, expected: object) -> None:
    """Each observation equals its case."""
    assert observed[path] == expected


def test_tools_join_the_toolbar_after_their_anchor(observed: Observations) -> None:
    """The first tool lands after the named stock tool behind a separator, opening a flyout group the second tool joins, and it binds one tool keymap."""
    toolbar = downcast(tuple, observed[*_SURFACES, "tooled", "toolbar"])
    group = toolbar.index(("group", ("fixture.probe", "fixture.probe_second")))
    assert ("tool", "builtin.transform") in toolbar[:group]
    assert toolbar[group - 1] == ("separator", None)
    assert len(downcast(tuple, observed[*_SURFACES, "tooled", "keymaps"])) == 1
