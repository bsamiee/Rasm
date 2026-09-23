"""Blender 5.2 LTS interface, viewport, workspace, and scene defaults for an architect driving Blender through agents.

Record of the run of 2026-09-22 that shaped the saved `userpref.blend` and `startup.blend`. Do not run it again: it deletes and
duplicates workspaces, re-applies the Blender_Dark preset, which wipes the theme overrides `ui-refine.py` writes, and ends by
saving both files and quitting. `ui-refine.py` is the script that runs on the current files.

It ran in GUI mode (window and screen data must exist):

    /Applications/Blender.app/Contents/MacOS/Blender --python tools/blender/ui-layout.py -- --fps 24 --asset-dir ~/Documents/Blender/Assets/Projects

Arguments after `--` are read by this script alone. `--fps` accepts 24 or 30 (default 24). `--asset-dir` names the project asset
library directory (default ~/Documents/Blender/Assets/Projects).

Workspace operators act on the window's active workspace and several of them finish through deferred notifiers, so every
workspace step runs from a `bpy.app.timers` callback that switches the window, yields to the event loop, and continues once the
switch is visible (workspace_edit.cc workspace_context_get, workspace_delete_exec, workspace_new_exec, blender-v5.2-release, read
2026-09-22; bpy.app.timers, Blender Python API 5.2, read 2026-09-22).

Every attribute below was probed against the installed Blender 5.2.2 LTS build (hash d13f752e3b9c) in background mode on
2026-09-22; every value carries its source and date.
"""

import argparse
from collections.abc import Iterator
from contextlib import AbstractContextManager
from pathlib import Path
import sys
from typing import cast, Protocol

import bpy

# --- [TYPES] ----------------------------------------------------------------------------


class SpacebarPreferences(Protocol):
    """Preferences of the Blender keyconfig, whose `Prefs` class the keyconfig defines in Python."""

    spacebar_action: str


# --- [CONSTANTS] ------------------------------------------------------------------------

TICK = 0.1

# Tab row, front to back. Names absent from the file are skipped. "BIM" is Bonsai's own workspace, appended by
# bonsai/bim/handler.py load_post from bonsai/bim/data/workspace.blend when the add-on preference should_setup_workspace is on
# (installed Bonsai wheel, read 2026-09-22), so it is ordered here and never created here. Default workspace list and the
# Reorder to Front/Back, Duplicate, Delete controls: manual interface/window_system/workspaces.html (5.2, 2026-09-22).
WORKSPACE_ORDER = ("Layout", "Modeling", "Drafting", "BIM", "Geometry Nodes", "Shading", "UV Editing", "Rendering", "Compositing", "Animation", "Scripting")

# Sculpting and Texture Paint serve character and asset painting work this owner does not do; both stay available through the
# Add Workspace menu (General group) because that menu reads the embedded startup file (workspace_edit.cc
# workspace_system_file_read, workspace_add_menu, read 2026-09-22).
WORKSPACES_REMOVED = ("Sculpting", "Texture Paint")

# Drafting duplicates Modeling (3D Viewport, Outliner, Properties) and opens on a top orthographic view for CAD Sketcher,
# MeasureIt_ARCH, and Bonsai 2D drafting, which starts from a top view.
DRAFTING_NAME = "Drafting"
DRAFTING_SOURCE = "Modeling"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [ARGUMENTS]
def parse_arguments(argv: list[str]) -> argparse.Namespace:
    """Frame rate and asset directory from the arguments after `--`."""
    parser = argparse.ArgumentParser(prog="ui-layout.py", description=__doc__.splitlines()[0])
    parser.add_argument("--fps", type=int, choices=(24, 30), default=24)
    parser.add_argument("--asset-dir", default=str(Path.home() / "Documents" / "Blender" / "Assets" / "Projects"))
    return parser.parse_args(argv)


# --- [PREFERENCES]
def apply_interface_preferences(view: bpy.types.PreferencesView, system: bpy.types.PreferencesSystem) -> None:
    """Interface > Display, Editors, Status Bar, and Accessibility (manual editors/preferences/interface.html, 5.2, 2026-09-22)."""
    # Resolution Scale is relative to the DPI Blender detects, so 1.0 keeps the Retina panel's native 2x scaling and gives
    # screenshots one fixed pixel geometry.
    view.ui_scale = 1.0
    # Line Width "Default" (identifier AUTO) follows the detected DPI.
    view.ui_line_width = "AUTO"
    view.show_splash = False
    # Developer Extras: Operator Search, Copy Python Command, Edit Source, icon names in tooltips (5.2 release notes Tooltips).
    view.show_developer_ui = True
    # User and Python tooltips: an agent reading a screenshot sees the property path under the tooltip.
    view.show_tooltips = True
    view.show_tooltips_python = True
    # Region Overlap off keeps the toolbar and sidebar beside the viewport, so a screenshot never hides geometry under a panel.
    system.use_region_overlap = False
    # Navigation Controls off, the axis gizmo stays through mini_axis_type below.
    view.show_navigate_ui = False
    # Header Position Top for every new editor, so layouts built by agents are uniform.
    view.header_align = "TOP"
    # Status Bar: Scene Statistics, System Memory, Blender Version on; Scene Duration off because the owner does not animate.
    # show_statusbar_vram is read-only on this build: unified memory reports no separate video memory (probed 2026-09-22).
    view.show_statusbar_stats = True
    view.show_statusbar_memory = True
    view.show_statusbar_version = True
    view.show_statusbar_scene_duration = False
    # Reduce Motion avoids interface animations, so a screenshot never lands mid-transition.
    view.use_reduce_motion = True


def apply_viewport_preferences(view: bpy.types.PreferencesView) -> None:
    """Viewport > Display (manual editors/preferences/viewport.html, 5.2, 2026-09-22)."""
    # Object Info and View Name on so every screenshot names the active object and the view.
    view.show_object_info = True
    view.show_view_name = True
    # Playback FPS off, a run-dependent number would make viewport captures differ between runs.
    view.show_playback_fps = False
    # Interactive Navigation keeps clickable axes for a trackpad user while the navigation buttons are hidden above.
    view.mini_axis_type = "GIZMO"
    # Smooth View 0 removes the animation so a view change is complete in the next frame.
    view.smooth_view = 0


def apply_input_preferences(inputs: bpy.types.PreferencesInput) -> None:
    """Input > Mouse, Keyboard, and Touchpad (manual editors/preferences/input.html, 5.2, 2026-09-22)."""
    # Emulate 3 Button Mouse stays off: a LinearMouse-driven mouse is present and multi-touch gestures cover the trackpad,
    # emulation would remap Cmd-click. The modifier is set so enabling it later uses Cmd.
    inputs.use_mouse_emulate_3_button = False
    inputs.mouse_emulate_3_button_modifier = "OSKEY"
    # Emulate Numpad for the laptop keyboard.
    inputs.use_emulate_numpad = True
    # Multi-touch Gestures on: gestures navigate instead of scroll-wheel emulation. Scroll Direction is Wayland-only.
    inputs.use_multitouch_gestures = True
    # Continuous Grab on for relative devices such as the trackpad.
    inputs.use_mouse_continuous = True


def apply_navigation_preferences(inputs: bpy.types.PreferencesInput) -> None:
    """Navigation (manual editors/preferences/navigation.html, 5.2, 2026-09-22)."""
    # Turntable keeps the horizon horizontal, the right frame for buildings.
    inputs.view_rotate_method = "TURNTABLE"
    # Orbit Around Selection off: inconvenient for large objects such as terrain, which describes BIM and geospatial ground.
    inputs.use_rotate_around_active = False
    # Auto Perspective: orthographic on axis views, perspective when orbiting, which is drafting behaviour.
    inputs.use_auto_perspective = True
    # Auto Depth with Zoom to Mouse Position supplies the orbit pivot and avoids panning while zooming.
    inputs.use_mouse_depth_navigate = True
    inputs.use_zoom_to_mouse = True
    # Dolly zoom has a constant speed; vertical axis, no inversion.
    inputs.view_zoom_method = "DOLLY"
    inputs.view_zoom_axis = "VERTICAL"
    inputs.invert_mouse_zoom = False
    inputs.invert_zoom_wheel = False
    # NDOF (3D mouse) for architectural review: Object mode, Lock Horizon, automatic orbit center on the visible model.
    inputs.ndof_navigation_mode = "OBJECT"
    inputs.ndof_lock_horizon = True
    inputs.ndof_orbit_center_auto = True
    inputs.ndof_orbit_center_selected = False


def apply_keymap_preferences(window_manager: bpy.types.WindowManager) -> None:
    """Keymap > Preferences > Spacebar Action: Search opens Menu Search on Space, F3 stays (manual editors/preferences/keymap.html, 5.2)."""
    keyconfig = cast("bpy.types.KeyConfig", window_manager.keyconfigs.active)
    cast("SpacebarPreferences", keyconfig.preferences).spacebar_action = "SEARCH"


def apply_editing_preferences(edit: bpy.types.PreferencesEdit) -> None:
    """Editing > Objects > New Objects (manual editors/preferences/editing.html, 5.2, 2026-09-22)."""
    # Objects an agent adds land world-aligned regardless of the current view, and stay in Object Mode for the next step.
    edit.object_align = "WORLD"
    edit.use_enter_edit_mode = False


def apply_theme(window: bpy.types.Window) -> None:
    """Blender_Dark preset applied explicitly so the saved preferences carry a known theme (manual editors/preferences/themes.html, 5.2).

    The preset click wipes every theme override, which is one reason this script never runs again.
    """
    preset_dir = next(path for path in bpy.utils.preset_paths("interface_theme") if (Path(path) / "Blender_Dark.xml").exists())
    with override(window):
        bpy.ops.script.execute_preset(filepath=str(Path(preset_dir) / "Blender_Dark.xml"), menu_idname="USERPREF_MT_interface_theme_presets")


def apply_asset_library(filepaths: bpy.types.PreferencesFilePaths, asset_dir: str) -> None:
    """Asset library "Projects" on the directory with import method Append, so a project block stays editable without a library link."""
    Path(asset_dir).mkdir(parents=True, exist_ok=True)
    if (name := "Projects") not in filepaths.asset_libraries:
        library = filepaths.asset_libraries.new(name=name, directory=asset_dir)
        library.import_method = "APPEND"


# --- [VIEWPORT]
def apply_shading(shading: bpy.types.View3DShading) -> None:
    """Viewport Shading: Solid, studio light, material colours, cavity both at 1.0, outline on, shadows off (manual editors/3dview/display/shading.html, 5.2)."""
    shading.type = "SOLID"
    shading.light = "STUDIO"
    shading.color_type = "MATERIAL"
    shading.show_cavity = True
    shading.cavity_type = "BOTH"
    shading.cavity_ridge_factor = 1.0
    shading.cavity_valley_factor = 1.0
    shading.curvature_ridge_factor = 1.0
    shading.curvature_valley_factor = 1.0
    shading.show_object_outline = True
    shading.show_shadows = False
    shading.show_specular_highlight = True
    shading.show_xray = False
    shading.background_type = "THEME"


def apply_overlay(overlay: bpy.types.View3DOverlay) -> None:
    """Viewport Overlays: statistics, text info, floor, ortho grid, three axes, no reference spheres, outline selected (manual editors/3dview/display/overlays.html, 5.2)."""
    overlay.show_stats = True
    overlay.show_text = True
    overlay.show_floor = True
    overlay.show_ortho_grid = True
    overlay.show_axis_x = True
    overlay.show_axis_y = True
    overlay.show_axis_z = True
    overlay.show_look_dev = False
    overlay.show_outline_selected = True


def apply_view3d(space: bpy.types.SpaceView3D) -> None:
    """Sidebar View panel: 35 mm lens, clip 0.05 m to 10000 m for a 10 km site at joinery scale (manual editors/3dview/sidebar.html, 5.2)."""
    space.lens = 35.0
    space.clip_start = 0.05
    space.clip_end = 10000.0
    apply_shading(space.shading)
    apply_overlay(space.overlay)


def view3d_spaces() -> list[bpy.types.SpaceView3D]:
    """Every 3D Viewport space in every screen of the startup file, so each workspace shares the same defaults."""
    return [space for screen in bpy.data.screens for area in screen.areas for space in area.spaces if isinstance(space, bpy.types.SpaceView3D)]


def apply_drafting_view(space: bpy.types.SpaceView3D) -> None:
    """Top Orthographic at 30 m of view distance, which frames a house plan at 1:1 metres."""
    view = cast("bpy.types.RegionView3D", space.region_3d)
    view.view_perspective = "ORTHO"
    view.view_rotation = (1.0, 0.0, 0.0, 0.0)
    view.view_distance = 30.0


# --- [SCENE]
def apply_scene(scene: bpy.types.Scene, fps: int) -> None:
    """Imperial display over metre storage, frame rate, Khronos PBR Neutral view transform, Workbench shading matching the viewport."""
    scene.unit_settings.system = "IMPERIAL"
    scene.unit_settings.scale_length = 1.0
    # The unit enums are dynamic, the stubs type them as their DEFAULT member alone.
    scene.unit_settings.length_unit = "FEET"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene.unit_settings.use_separate = True
    scene.unit_settings.mass_unit = "POUNDS"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene.unit_settings.temperature_unit = "FAHRENHEIT"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene.render.fps = fps
    scene.render.fps_base = 1.0
    # PBR Neutral keeps material colours as assigned, so a screenshot shows the colour an agent set; AgX skews bright hues. The
    # view transform enum is dynamic too, the stubs type it as NONE alone.
    cast("bpy.types.ColorManagedViewSettings", scene.view_settings).view_transform = "Khronos PBR Neutral"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    apply_shading(cast("bpy.types.View3DShading", cast("bpy.types.SceneDisplay", scene.display).shading))


# --- [WORKSPACES]
def override(window: bpy.types.Window) -> AbstractContextManager[None]:
    """Context override on the window, which the stubs type without the context manager protocol."""
    return cast("AbstractContextManager[None]", bpy.context.temp_override(window=window))


def activate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Switch the window to the workspace and yield until the window manager applied it on an event-loop pass."""
    window.workspace = bpy.data.workspaces[name]
    while window.workspace.name != name:
        yield TICK


def delete_workspace(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Delete the workspace through its deferred notifier (workspace_edit.cc workspace_delete_exec)."""
    yield from activate(window, name)
    with override(window):
        bpy.ops.workspace.delete()
    while name in bpy.data.workspaces:
        yield TICK


def duplicate_workspace(window: bpy.types.Window, source: str, name: str) -> Iterator[float]:
    """Duplicate the source workspace, which activates the copy, and rename it."""
    yield from activate(window, source)
    before = {workspace.name for workspace in bpy.data.workspaces}
    with override(window):
        bpy.ops.workspace.duplicate()
    created = next(workspace for workspace in bpy.data.workspaces if workspace.name not in before)
    created.name = name
    yield from activate(window, name)


def resize_scripting(window: bpy.types.Window) -> Iterator[float]:
    """Swap the Scripting screen's tall 3D Viewport with its small Info editor, so agent logs get the tall area."""
    yield from activate(window, "Scripting")
    areas = window.screen.areas
    viewport = next(area for area in areas if area.type == "VIEW_3D")
    info = next(area for area in areas if area.type == "INFO")
    with override(window):
        viewport.type = "INFO"
        info.type = "VIEW_3D"
    yield TICK


def reorder(window: bpy.types.Window, names: tuple[str, ...]) -> Iterator[float]:
    """Activate each name from back to front and move it to the front, which yields the tab row in order."""
    for name in reversed(names):
        yield from activate(window, name)
        with override(window):
            bpy.ops.workspace.reorder_to_front()
        yield TICK


# --- [COMPOSITION] ----------------------------------------------------------------------


def steps(window: bpy.types.Window, arguments: argparse.Namespace) -> Iterator[float]:
    """Every step in order, ending with the saves and the quit."""
    preferences = cast("bpy.types.Preferences", bpy.context.preferences)
    apply_interface_preferences(preferences.view, preferences.system)
    apply_viewport_preferences(preferences.view)
    apply_input_preferences(preferences.inputs)
    apply_navigation_preferences(preferences.inputs)
    apply_editing_preferences(preferences.edit)
    apply_keymap_preferences(cast("bpy.types.WindowManager", bpy.context.window_manager))
    apply_asset_library(preferences.filepaths, arguments.asset_dir)
    apply_theme(window)
    yield TICK

    for name in WORKSPACES_REMOVED:
        if name in bpy.data.workspaces:
            yield from delete_workspace(window, name)

    if DRAFTING_NAME not in bpy.data.workspaces:
        yield from duplicate_workspace(window, DRAFTING_SOURCE, DRAFTING_NAME)

    yield from resize_scripting(window)

    for space in view3d_spaces():
        apply_view3d(space)
    for area in bpy.data.workspaces[DRAFTING_NAME].screens[0].areas:
        for drafting in area.spaces:
            if isinstance(drafting, bpy.types.SpaceView3D):
                apply_drafting_view(drafting)

    apply_scene(cast("bpy.types.Scene", bpy.context.scene), arguments.fps)

    yield from reorder(window, tuple(name for name in WORKSPACE_ORDER if name in bpy.data.workspaces))
    yield from activate(window, "Layout")

    with override(window):
        bpy.ops.wm.save_userpref()
        bpy.ops.wm.save_homefile()
        bpy.ops.wm.quit_blender()


def run() -> None:
    """Register the step generator as a timer on the first window."""
    arguments = parse_arguments(sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else [])
    window = cast("bpy.types.WindowManager", bpy.context.window_manager).windows[0]
    generator = steps(window, arguments)

    def tick() -> float | None:
        try:
            return next(generator)
        except StopIteration:
            return None

    bpy.app.timers.register(tick, first_interval=0.5)


run()
