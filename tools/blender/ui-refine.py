# ruff: file-ignore[print]
"""Blender 5.2 LTS interface refinement over the saved files of `ui-layout.py`.

One header row, one device pixel lines, the Radix field and palette, Rhino's navigation rule, and a 100 px Timeline. Runs in GUI
mode with Blender quit first (window and screen data must exist):

    /Applications/Blender.app/Contents/MacOS/Blender --python tools/blender/ui-refine.py
    /Applications/Blender.app/Contents/MacOS/Blender --python tools/blender/ui-refine.py -- --theme

`--theme` re-applies the theme section alone, the Blender_Dark preset click wipes every theme override. The script saves nothing
and never quits: File > Defaults > Save Startup File and Preferences > Save Preferences follow the manual panel pass, and every
assertion prints to stdout with a final `UI_REFINE OK` or `UI_REFINE FAIL`.

Facts the values rest on (blender-v5.2-release, read 2026-09-23): wm_window.cc computes `dpi = auto_dpi * ui_scale * 72 / 96`
and `pixelsize = max(1, dpi // 64 + ui_line_width)`, so 144 x 0.9 gives 129 and THIN gives one device pixel; screen_ops.cc
`area_snap_calc_location` snaps a Dope Sheet area to header plus scrub height, so the Timeline resizes as an Outliner and is
switched back; view3d_navigate.cc `view3d_rotation_poll` fails under `lock_rotation` and a failed poll passes the event to the
next keymap item, which `rasm_navigation` relies on. Colours are Radix Colors 3.0.0 dark scales, hex over 255 rounded to four
places, the field #4D4D4D being the one non-Radix value; Blender stores theme channels as bytes, so the alphas are 128 and 60
over 255 rather than 0.5 and 0.235.
"""

from collections.abc import Callable, Iterator
from contextlib import AbstractContextManager
import sys
from typing import cast

import bpy

# --- [TYPES] ----------------------------------------------------------------------------

type Value = str | int | float | bool | tuple[float, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

TICK = 0.1
ATTEMPTS = 3
ADDON = "bl_ext.user_default.rasm_navigation"
UI_SCALE = 0.9
DPI = 129
PIXEL_SIZE = 1.0
TIMELINE_HEIGHT = 100
TIMELINE_WORKSPACES = ("Layout", "Geometry Nodes", "Rendering", "Compositing")
STATS_WORKSPACE = "Drafting"
TOOL_HEADER_WORKSPACE = "BIM"
EXTRA_PROPERTIES_WORKSPACE = "BIM"
PROPERTIES_TABS = ("show_properties_particles", "show_properties_physics", "show_properties_effects", "show_properties_strip", "show_properties_strip_modifier")
CONTEXT_MENUS = ("wm.call_menu", "wm.call_panel")
MODE_KEYMAPS = (
    "Object Mode",
    "Mesh",
    "Curve",
    "Curves",
    "Armature",
    "Metaball",
    "Lattice",
    "Particle",
    "Font",
    "Pose",
    "Sculpt",
    "Image Paint",
    "Vertex Paint",
    "Weight Paint",
    "Grease Pencil Draw Mode",
    "Grease Pencil Edit Mode",
    "Grease Pencil Sculpt Mode",
    "Grease Pencil Weight Paint",
    "Grease Pencil Vertex Paint",
)
VIEW_WRAPPERS = (("view3d.view_persportho", "rasm.view_persportho"), ("view3d.view_axis", "rasm.view_axis"), ("view3d.view_camera", "rasm.view_camera"))
NAVIGATION_ORDER = (("view3d.rotate", "TRACKPADPAN", "ANY"), ("view3d.rotate", "RIGHTMOUSE", "CLICK_DRAG"), ("view3d.move", "RIGHTMOUSE", "CLICK_DRAG"), ("view3d.move", "TRACKPADPAN", "ANY"))
AXES = ("BACK", "BOTTOM", "FRONT", "LEFT", "RIGHT", "TOP")

FIELD = (0.302, 0.302, 0.302)
GRID = (0.3765, 0.3765, 0.3765, 0.502)
TEXT = (0.9333, 0.9333, 0.9333)
CYAN_11 = (0.298, 0.8, 0.902)
CYAN_12 = (0.7137, 0.9255, 0.9686)
PLUM_10 = (0.7137, 0.3451, 0.7686)
PLUM_11 = (0.9059, 0.5882, 0.9529)
RED_9 = (0.898, 0.2824, 0.302)
GRASS_9 = (0.2745, 0.6549, 0.3451)
BLUE_9 = (0.0, 0.5647, 1.0)
ORANGE_11 = (1.0, 0.6275, 0.3412)

THEME: tuple[tuple[str, Value], ...] = (
    ("view_3d.space.gradients.background_type", "SINGLE_COLOR"),
    ("view_3d.space.gradients.high_gradient", FIELD),
    ("view_3d.space.text", TEXT),
    ("view_3d.grid", GRID),
    ("view_3d.grid_major", GRID),
    ("view_3d.object_selected", CYAN_11),
    ("view_3d.object_active", CYAN_12),
    ("view_3d.edge_select", CYAN_11),
    ("view_3d.vertex_select", CYAN_12),
    ("view_3d.edge_mode_select", CYAN_12),
    ("view_3d.face_mode_select", (*CYAN_12, 0.2)),
    ("view_3d.face_select", (*CYAN_11, 0.2)),
    ("view_3d.gp_vertex_select", CYAN_11),
    ("view_3d.transform", PLUM_11),
    ("view_3d.sharp", ORANGE_11),
    ("view_3d.edge_width", 1),
    ("view_3d.outline_width", 1),
    ("view_3d.vertex_size", 4),
    ("user_interface.gizmo_primary", PLUM_11),
    ("user_interface.gizmo_secondary", PLUM_10),
    ("user_interface.axis_x", RED_9),
    ("user_interface.axis_y", GRASS_9),
    ("user_interface.axis_z", BLUE_9),
    ("outliner.selected_object", CYAN_11),
    ("outliner.active_object", CYAN_12),
    ("node_editor.node_selected", CYAN_11),
    ("sequence_editor.selected_strip", CYAN_11),
    ("nla_editor.strips_selected", CYAN_11),
    ("clip_editor.selected_marker", CYAN_11),
    ("image_editor.edge_select", CYAN_11),
    ("image_editor.vertex_select", CYAN_12),
    ("image_editor.face_select", (*CYAN_11, 0.2353)),
    ("graph_editor.vertex_select", CYAN_11),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def check(label: str, *, ok: bool, detail: object) -> bool:
    """Print one assertion, flushed so a terminal sees it as it happens, and return whether it held."""
    print(f"{'ok  ' if ok else 'FAIL'} {label}: {detail}", flush=True)
    return ok


def resolve(root: object, path: str) -> tuple[object, str]:
    """Owner and attribute name of a dotted path."""
    *parents, name = path.split(".")
    owner = root
    for parent in parents:
        owner = getattr(owner, parent)
    return owner, name


def read(owner: object, name: str) -> Value:
    """Attribute value with colour components rounded to the four places the constants carry."""
    value = getattr(owner, name)
    if isinstance(value, str | int | float):
        return value
    return tuple(round(component, 4) for component in value)


def settle(label: str, owner: object, path: str, *, value: Value) -> bool:
    """Write the attribute and check the read-back."""
    parent, name = resolve(owner, path)
    setattr(parent, name, value)
    stored = read(parent, name)
    return check(f"{label} {path}", ok=stored == value, detail=stored)


# --- [PREFERENCES]
def apply_preferences(preferences: bpy.types.Preferences, held: list[bool]) -> Iterator[float]:
    """Scale 0.9 with thin lines for one device pixel, one pixel borders, compact sidebar tabs, then read the DPI Blender derived."""
    preferences.view.ui_scale = UI_SCALE
    preferences.view.ui_line_width = "THIN"
    preferences.view.border_width = 1
    preferences.system.show_panel_tabs_compact = True
    yield TICK
    held.extend((
        check("system.dpi", ok=preferences.system.dpi == DPI, detail=preferences.system.dpi),
        check("system.pixel_size", ok=preferences.system.pixel_size == PIXEL_SIZE, detail=preferences.system.pixel_size),
        check("system.show_panel_tabs_compact", ok=preferences.system.show_panel_tabs_compact, detail=preferences.system.show_panel_tabs_compact),
    ))


def apply_theme(theme: bpy.types.Theme, held: list[bool]) -> None:
    """Write every palette attribute on the theme and read each one back."""
    held.extend(settle("theme", theme, path, value=value) for path, value in THEME)


# --- [AREAS]
def refine_space(workspace: str, space: bpy.types.Space, held: list[bool]) -> None:
    """One header row per editor, statistics in Drafting alone, the Properties tabs this owner uses, and the Outliner select column."""
    if isinstance(space, bpy.types.SpaceView3D):
        held.extend((settle(workspace, space, "show_region_tool_header", value=workspace == TOOL_HEADER_WORKSPACE), settle(workspace, space, "overlay.show_stats", value=workspace == STATS_WORKSPACE)))
        if workspace == STATS_WORKSPACE:
            held.extend((settle(workspace, space, "overlay.show_wireframes", value=True), settle(workspace, space, "overlay.wireframe_threshold", value=0.5)))
    if isinstance(space, bpy.types.SpaceImageEditor):
        held.append(settle(workspace, space, "show_region_tool_header", value=False))
    if isinstance(space, bpy.types.SpaceProperties):
        held.extend(settle(workspace, space, tab, value=workspace == EXTRA_PROPERTIES_WORKSPACE) for tab in PROPERTIES_TABS)
    if isinstance(space, bpy.types.SpaceOutliner):
        held.append(settle(workspace, space, "show_restrict_column_select", value=True))


def apply_areas(workspaces: "bpy.types.bpy_prop_collection[bpy.types.WorkSpace]", held: list[bool]) -> None:
    """Refine the active space of every area of every workspace screen."""
    for workspace in workspaces:
        for area in workspace.screens[0].areas:
            refine_space(workspace.name, cast("bpy.types.Space", area.spaces.active), held)


def activate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Switch the window to the workspace and yield until the window manager applied it on an event-loop pass."""
    window.workspace = bpy.data.workspaces[name]
    while window.workspace.name != name:
        yield TICK


def move_edge(window: bpy.types.Window, x: int, y: int, delta: int) -> bool:
    """Run `area_move` on the edge at the position when it polls, which needs no region under the cursor."""
    with cast("AbstractContextManager[None]", bpy.context.temp_override(window=window, screen=window.screen)):
        if polled := cast("Callable[[], bool]", bpy.ops.screen.area_move.poll)():
            bpy.ops.screen.area_move(x=x, y=y, delta=delta)
    return polled


def resize_timeline(window: bpy.types.Window, name: str, held: list[bool]) -> Iterator[float]:
    """Move the Timeline's top edge to 100 px while the area is an Outliner, which carries no scrub-height snap, then switch it back."""
    # The cursor is warped onto the edge and two event-loop passes clear the active region before each attempt, and the first
    # workspace after startup takes a second attempt.
    yield from activate(window, name)
    screen = window.screen
    area = next(area for area in screen.areas if area.ui_type == "TIMELINE")
    above = min((other for other in screen.areas if other.y > area.y and other.x < area.x + area.width and area.x < other.x + other.width), key=lambda other: other.y)
    edge_x = area.x + area.width // 2
    edge_y = above.y
    area.ui_type = "OUTLINER"
    yield TICK
    try:
        attempts = 0
        while area.height != TIMELINE_HEIGHT and attempts < ATTEMPTS:
            attempts += 1
            window.cursor_warp(edge_x, edge_y)
            yield TICK
            yield TICK
            if not move_edge(window, edge_x, edge_y, TIMELINE_HEIGHT - area.height):
                print(f"area_move poll false in {name} at {edge_x} {edge_y}, attempt {attempts}", flush=True)
            yield TICK
    finally:
        area.ui_type = "TIMELINE"
    yield TICK
    held.append(check(f"{name} timeline", ok=area.ui_type == "TIMELINE" and area.height == TIMELINE_HEIGHT, detail=(area.ui_type, area.height)))


# --- [NAVIGATION]
def enable_navigation(preferences: bpy.types.Preferences, held: list[bool]) -> None:
    """Enable the extension that locks rotation on every non-perspective view and supplies the view wrappers."""
    bpy.ops.preferences.addon_enable(module=ADDON)
    held.append(check(f"addon {ADDON}", ok=ADDON in preferences.addons, detail=[addon.module for addon in preferences.addons if addon.module == ADDON]))


def modifiers(item: bpy.types.KeyMapItem) -> tuple[int, int, int, int]:
    """Modifier state of a keymap item, zeros for none."""
    return (item.shift, item.ctrl, item.alt, item.oskey)


def properties(item: bpy.types.KeyMapItem) -> bpy.types.OperatorProperties:
    """Operator properties of a keymap item."""
    return cast("bpy.types.OperatorProperties", item.properties)


def repoint(item: bpy.types.KeyMapItem, idname: str) -> None:
    """Point the item at another operator and restore the properties it carried, which the idname write resets."""
    before = properties(item)
    kept = {prop.identifier: getattr(before, prop.identifier) for prop in before.bl_rna.properties if prop.identifier != "rna_type" and before.is_property_set(prop.identifier)}
    item.idname = idname
    after = properties(item)
    for prop, value in kept.items():
        setattr(after, prop, value)


def apply_keymap(keyconfig: bpy.types.KeyConfig, held: list[bool]) -> None:
    """Context menus on a still right click, right drag and two-finger swipe orbit or pan by the lock, numpad views through the wrappers."""
    clicks = 0
    for name in MODE_KEYMAPS:
        for item in keyconfig.keymaps[name].keymap_items:
            if item.idname in CONTEXT_MENUS and item.type == "RIGHTMOUSE" and item.value == "PRESS" and not item.any and modifiers(item) == (0, 0, 0, 0):
                item.value = "CLICK"
            clicks += item.idname in CONTEXT_MENUS and item.type == "RIGHTMOUSE" and item.value == "CLICK"

    view = keyconfig.keymaps["3D View"]
    view.keymap_items.new("view3d.rotate", "RIGHTMOUSE", "CLICK_DRAG")
    view.keymap_items.new("view3d.move", "RIGHTMOUSE", "CLICK_DRAG")
    view.keymap_items.new("view3d.move", "TRACKPADPAN", "ANY")
    wrappers = dict(VIEW_WRAPPERS)
    for item in view.keymap_items:
        if item.idname in wrappers:
            repoint(item, wrappers[item.idname])

    order = [
        (item.idname, item.type, item.value)
        for item in view.keymap_items
        if item.type in {"RIGHTMOUSE", "TRACKPADPAN"} and item.idname in {"view3d.rotate", "view3d.move"} and modifiers(item) == (0, 0, 0, 0)
    ]
    stock = [item.idname for item in view.keymap_items if item.idname in wrappers]
    wrapped = sorted({item.idname for item in view.keymap_items if item.idname in wrappers.values()})
    axes = sorted({str(properties(item).type) for item in view.keymap_items if item.idname == "rasm.view_axis"})
    orbit = [(item.type, item.value, item.ctrl, item.shift) for item in view.keymap_items if item.idname == "rasm.orbit"]
    held.extend((
        check("Ctrl+Shift+RMB orbit item merged", ok=orbit == [("RIGHTMOUSE", "PRESS", 1, 1)], detail=orbit),
        check("context menu CLICK items", ok=clicks == len(MODE_KEYMAPS), detail=clicks),
        check("3D View navigation items in order", ok=order == list(NAVIGATION_ORDER), detail=order),
        check("stock numpad view items repointed", ok=stock == [], detail=stock),
        check("wrapper items present", ok=wrapped == sorted(wrappers.values()), detail=wrapped),
        check("view axis properties kept", ok=axes == list(AXES), detail=axes),
    ))


# --- [COMPOSITION] ----------------------------------------------------------------------


def steps(window: bpy.types.Window, *, theme_only: bool) -> Iterator[float]:
    """Every step in order, the theme alone under `--theme`."""
    preferences = cast("bpy.types.Preferences", bpy.context.preferences)
    manager = cast("bpy.types.WindowManager", bpy.context.window_manager)
    held: list[bool] = []
    if theme_only:
        apply_theme(preferences.themes[0], held)
    else:
        yield from apply_preferences(preferences, held)
        apply_theme(preferences.themes[0], held)
        apply_areas(bpy.data.workspaces, held)
        for name in TIMELINE_WORKSPACES:
            yield from resize_timeline(window, name, held)
        yield from activate(window, "Layout")
        enable_navigation(preferences, held)
        # One event-loop pass merges the extension's keymap item into the user keymap before the user keymap is edited: the
        # diff of an edited user keymap is taken against default plus add-on items, and an item the user keymap lacked at that
        # moment is recorded as removed (wm_keymap.cc wm_keymap_diff_update, blender-v5.2-release).
        yield TICK
        apply_keymap(cast("bpy.types.KeyConfig", manager.keyconfigs.user), held)
    print(f"UI_REFINE {'OK' if all(held) else 'FAIL'}", flush=True)


def run() -> None:
    """Register the step generator as a timer on the first window."""
    arguments = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    window = cast("bpy.types.WindowManager", bpy.context.window_manager).windows[0]
    generator = steps(window, theme_only="--theme" in arguments)

    def tick() -> float | None:
        try:
            return next(generator)
        except StopIteration:
            return None
        except Exception:
            print("UI_REFINE FAIL raised", flush=True)
            raise

    bpy.app.timers.register(tick, first_interval=0.5)


run()
