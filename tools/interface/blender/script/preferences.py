# ty: ignore[invalid-argument-type, invalid-assignment, possibly-missing-attribute, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender's preferences, keyconfig preferences, auto-run exclusion, asset libraries, studio lights, and the user keymap edits of stock items."""

from collections.abc import Iterator
from functools import partial
from glob import escape
from itertools import chain
import os
from pathlib import Path
from types import ModuleType
from typing import Literal

import bpy

from interface.blender.rows import Launch, LOOK_DEVELOPMENT
from interface.blender.script.library import ASSETS
from interface.blender.script.rna import converge, Paint
from interface.blender.script.startup import EYE_HEIGHT, HEADLAMP
from interface.render import DESIGN_TOOLS, MATERIALS
from interface.report import digest, Error, Item, Row, subscript
from interface.roles import Guide, Typography
from interface.units import ANGLE_STEP, Length

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SETTINGS]
def declared(preferences: bpy.types.Preferences, launch: Launch, memory: int) -> dict[str, object]:
    """Declared preferences by path with memory limits in MB and the text editor executable the host found, online access before its handled flag."""
    caches, cycles = Path(bpy.app.cachedir), preferences.addons["cycles"].preferences
    device = next(kind for kind, _, _, value in cycles.get_device_types(None) if value == type(cycles).default_device())
    return {
        "view.ui_scale": 0.9,
        "view.ui_line_width": "THIN",
        "view.border_width": 1,
        "view.show_splash": False,
        "view.use_save_prompt": False,
        "view.use_reduce_motion": True,
        "view.smooth_view": 0,
        **dict.fromkeys(("view.show_developer_ui", "view.show_tooltips", "view.show_tooltips_python", "view.show_addons_enabled_only"), True),
        "view.show_navigate_ui": False,
        "view.header_align": "TOP",
        "view.mini_axis_type": "MINIMAL",
        "view.mini_axis_size": 25,
        "view.mini_axis_brightness": 10,
        "view.rotation_angle": ANGLE_STEP,
        "view.date_format": "ME_SLASH",
        "view.time_format": "H12",
        **dict.fromkeys(("view.show_statusbar_stats", "view.show_statusbar_memory"), True),
        **dict.fromkeys(("view.show_statusbar_version", "view.show_statusbar_scene_duration"), False),
        **dict.fromkeys(("view.show_playback_fps", "view.show_object_info", "view.show_view_name"), True),
        "view.render_display_type": "SCREEN",
        "view.filebrowser_display_type": "SCREEN",
        "view.preferences_display_type": "SCREEN",
        "view.text_hinting": "NONE",
        "view.pie_tap_timeout": 20,
        "view.view2d_grid_spacing_min": 35,
        "view.use_text_antialiasing": True,
        "view.use_text_render_subpixelaa": True,
        "view.gizmo_size": 65,
        "view.lookdev_sphere_size": 100,
        "view.asset_access": "ALL",
        "asset_libraries.use_online_essentials": False,
        "system.use_region_overlap": False,
        "system.show_panel_tabs_compact": True,
        "system.use_online_access": True,
        "extensions.use_online_access_handled": True,
        "system.scrollback": 32768,
        "system.memory_cache_limit": memory,
        "system.anisotropic_filter": "FILTER_16",
        "system.viewport_aa": "OFF",
        "system.use_overlay_smooth_wire": False,
        "system.use_edit_mode_smooth_wire": False,
        "system.audio_device": "CoreAudio",
        "inputs.drag_threshold_mouse": 3,
        "inputs.drag_threshold": 30,
        "inputs.mouse_double_click_time": 350,
        "inputs.use_mouse_emulate_3_button": False,
        "inputs.use_drag_immediately": True,
        "inputs.use_emulate_numpad": False,
        **dict.fromkeys(("inputs.use_multitouch_gestures", "inputs.use_mouse_continuous", "inputs.use_numeric_input_advanced"), True),
        "inputs.view_rotate_method": "TURNTABLE",
        "inputs.use_rotate_around_active": False,
        **dict.fromkeys(("inputs.use_auto_perspective", "inputs.use_mouse_depth_navigate", "inputs.use_zoom_to_mouse"), True),
        "inputs.view_zoom_method": "DOLLY",
        "inputs.view_zoom_axis": "VERTICAL",
        "inputs.invert_mouse_zoom": False,
        "inputs.invert_zoom_wheel": False,
        "inputs.walk_navigation.view_height": EYE_HEIGHT,
        "inputs.walk_navigation.walk_speed": 5 * Length.FEET,
        "inputs.walk_navigation.use_gravity": True,
        "inputs.walk_navigation.jump_height": 18 * Length.INCHES,
        "edit.object_align": "WORLD",
        "edit.use_enter_edit_mode": False,
        "edit.use_auto_keying": False,
        "edit.use_text_edit_auto_close": True,
        "edit.undo_steps": 256,
        "edit.undo_memory_limit": memory,
        "edit.keyframe_new_interpolation_type": "LINEAR",
        "edit.grease_pencil_default_color": Paint(Guide.CONSTRUCTION),
        "filepaths.font_directory": f"{DESIGN_TOOLS / 'fonts'}/",
        "filepaths.texture_directory": f"{MATERIALS}/",
        "filepaths.text_editor": launch.editor or "",
        "filepaths.text_editor_args": "" if launch.editor is None else "-g $filepath:$line:$column",
        "filepaths.use_scripts_auto_execute": True,
        "filepaths.save_version": 3,
        "filepaths.use_load_ui": False,
        "filepaths.use_auto_save_temporary_files": False,
        "filepaths.use_tabs_as_spaces": True,
        "filepaths.save_modified_images": "ALWAYS_SAVE",
        "filepaths.use_file_compression": False,
        "filepaths.temporary_directory": f"{caches / 'temp'}/",
        "filepaths.texture_cache_directory": f"{caches / 'texture-cache'}/",
        "filepaths.render_output_directory": f"{launch.renders}/",
        **{f"experimental.{prop.identifier}": False for prop in preferences.experimental.bl_rna.properties if prop.type == "BOOLEAN" and not prop.is_readonly},
        'addons["cycles"].preferences.compute_device_type': device,
        **{f"{subscript('extensions.repos', index)}.{name}": False for index, _ in enumerate(preferences.extensions.repos) for name in ("use_sync_on_startup", "use_cache")},
    }


def listings(preferences: bpy.types.Preferences) -> tuple[Row, ...]:
    """Rows of auto-run exclusion, keyconfig every launch loads, and asset libraries, local and remote, each with its import method."""
    excluded, libraries = preferences.autoexec_paths, preferences.filepaths.asset_libraries

    def exclude(target: tuple[tuple[str, bool], ...]) -> None:
        for held in tuple(excluded):
            excluded.remove(held)
        for path, glob in target:
            added = excluded.new()
            added.path, added.use_glob = path, glob

    def activate(target: str) -> None:
        bpy.utils.keyconfig_set(bpy.utils.preset_find(target, "keyconfig"))

    def listed(target: tuple[tuple[str, bool, str, Literal["APPEND", "PACK"]], ...]) -> None:
        for held in tuple(libraries):
            libraries.remove(held)
        for name, remote, location, method in target:
            match remote:
                case True:
                    bpy.ops.preferences.asset_library_add(type="REMOTE", name=name, remote_url=location)
                    libraries[-1].import_method = method
                case False:
                    libraries.new(name=name, directory=location).import_method = method

    return (
        Row(label="preferences.autoexec_paths", read=lambda: tuple((held.path, held.use_glob) for held in excluded), write=exclude, target=((f"{Path.home() / 'Downloads'}/", False),)),
        Row(label="preferences.keymap.active_keyconfig", read=lambda: preferences.keymap.active_keyconfig, write=activate, target="Blender"),
        Row(
            label="preferences.filepaths.asset_libraries",
            read=lambda: tuple((held.name, held.use_remote_url, held.remote_url if held.use_remote_url else held.path, held.import_method) for held in libraries),
            write=listed,
            target=(
                ("Assets", False, str(ASSETS), "APPEND"),
                ("CGMatter", True, "https://cgmatter.github.io/website/nodes/", "PACK"),
                ("ambientCG", True, "https://ambientcg.com/api/blender/", "APPEND"),
            ),
        ),
    )


def lights(preferences: bpy.types.Preferences) -> tuple[Row, ...]:
    """Rows of headlamp studio light and look-development HDRI, each a copy `studiolight_install` makes in the user studio light folder, compared by content."""

    def held(kind: str, name: str) -> Path | None:
        return next((Path(light.path) for light in preferences.studio_lights if light.is_user_defined and light.type == kind and light.name == name), None)

    def install(kind: str, source: Path) -> None:
        bpy.ops.preferences.studiolight_install(directory=f"{source.parent}/", files=[{"name": source.name}], type=kind)
        preferences.studio_lights.refresh()

    return tuple(
        Row(
            label=subscript("preferences.studio_lights", source.name),
            read=partial(held, kind, source.name),
            write=partial(install, kind),
            target=source,
            plain=lambda path: digest(path.read_bytes()) if isinstance(path, Path) else None,
        )
        for kind, source in (("STUDIO", HEADLAMP), ("WORLD", LOOK_DEVELOPMENT))
    )


def stock_edits(user: bpy.types.KeyConfig) -> Iterator[tuple[str, bpy.types.KeyMapItem, bool, dict[str, object]]]:
    """Stock user keymap items by label with declared values: context menus on click, Spreadsheet toolbar on T, Cmd+F node search, and Bonsai's Space profile edit off."""
    canvases = frozenset({"EMPTY", "VIEW_3D", "NODE_EDITOR", "IMAGE_EDITOR", "CLIP_EDITOR", "GRAPH_EDITOR", "DOPESHEET_EDITOR", "NLA_EDITOR", "SEQUENCE_EDITOR"})
    for keymap in user.keymaps:
        for index, item in enumerate(keymap.keymap_items):
            label, modified = subscript(f"{subscript('keyconfigs.user.keymaps', keymap.name)}.keymap_items", index), item.any or item.shift or item.ctrl or item.alt or item.oskey
            match item.idname, item.type, item.value:
                case "wm.call_menu" | "wm.call_panel", "RIGHTMOUSE", "PRESS" | "CLICK" if keymap.space_type in canvases and not modified:
                    yield label, item, True, {"value": "CLICK"}
                case "wm.context_toggle", "T", "PRESS" if keymap.name == "Spreadsheet Generic" and item.properties.data_path in {"space_data.show_region_channels", "space_data.show_region_toolbar"}:
                    yield label, item, True, {"properties.data_path": "space_data.show_region_toolbar"}
                case "node.find_node" | "improved_node_search.search", "F", "PRESS" if keymap.name == "Node Editor" and item.oskey and not (item.shift or item.ctrl or item.alt):
                    yield label, item, True, {"idname": "improved_node_search.search"}
                case "bim.direct_profile_edit", "SPACE", "PRESS":
                    yield label, item, False, {}
                case _:
                    pass


# --- [STEPS]
def converged_preferences(unit_system: ModuleType, preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, launch: Launch) -> Iterator[Item]:
    """Converge interface font faces, each the one file of its name in the user font folder, then preferences, auto-run exclusion, keyconfig, asset libraries, studio lights, keyconfig preferences, and Cycles devices of the declared compute type."""
    folder, cycles = Path.home() / "Library" / "Fonts", preferences.addons["cycles"].preferences
    for member, face in (("font_path_ui", Typography.INTERFACE), ("font_path_ui_mono", Typography.MONOSPACE)):
        match tuple(folder.rglob(escape(face.file))):
            case [path]:
                yield from converge(unit_system, "preferences.view", preferences.view, {member: str(path)})
            case found:
                yield Error(f"preferences.view.{member} finds {len(found)} files {tuple(map(str, found))} named {face.file} under {folder} where one is declared")
    yield from converge(unit_system, "preferences", preferences, declared(preferences, launch, os.sysconf("SC_PHYS_PAGES") * os.sysconf("SC_PAGE_SIZE") // 4 // 2**20))
    yield from listings(preferences)
    yield from lights(preferences)
    keyconfig = {"select_mouse": "LEFT", "spacebar_action": "SEARCH", "use_pie_click_drag": True, "v3d_alt_mmb_drag_action": "ABSOLUTE"}
    yield from converge(unit_system, "keyconfigs.active.preferences", keyconfigs.active.preferences, keyconfig)
    cycles.refresh_devices()
    devices = {f"{subscript('devices', index)}.use": True for index, device in enumerate(cycles.devices) if device.type == cycles.compute_device_type}
    yield from converge(unit_system, f"{subscript('preferences.addons', 'cycles')}.preferences", cycles, devices)


def bound_keymaps(unit_system: ModuleType, keyconfigs: bpy.types.KeyConfigurations) -> Iterator[Row]:
    """Rows of stock user keymap edits, each write followed by its item's active flag so an operator-property edit enters the user keymap diff."""

    def recorded(item: bpy.types.KeyMapItem, struct: "bpy.types.bpy_struct[object]", name: str, value: object, *, active: bool) -> None:
        setattr(struct, name, value)
        item.active = active

    return chain.from_iterable(converge(unit_system, label, item, {**edits, "active": active}, partial(recorded, item, active=active)) for label, item, active, edits in stock_edits(keyconfigs.user))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bound_keymaps", "converged_preferences"]
