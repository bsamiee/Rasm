# ty: ignore[invalid-argument-type, possibly-missing-attribute, unresolved-attribute]
# mypy: disable-error-code="arg-type, no-any-return, union-attr"
"""Blender's declared preferences, keyconfig preferences, and user keymap edits."""

from collections.abc import Iterator, Mapping
from functools import partial
from itertools import takewhile
import os
from pathlib import Path
from types import MappingProxyType
from typing import Final, Literal

import bpy

from interface.blender.startup import EYE_HEIGHT
from interface.blender.theme import Paint
from interface.render import ASSETS, DESIGN_TOOLS, LOOK_DEVELOPMENT, MATERIALS, stocked
from interface.report import Kind, line, Row
from interface.roles import Guide, Typography
from interface.units import ANGLE_STEP, FOOT, INCH

# --- [CONSTANTS] ------------------------------------------------------------------------

KEYCONFIG: Final = MappingProxyType({"select_mouse": "LEFT", "spacebar_action": "SEARCH", "use_pie_click_drag": True, "v3d_alt_mmb_drag_action": "ABSOLUTE"})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [STRUCTURE]
def folders(label: str, owner: bpy.types.bpy_struct, declared: Mapping[str, object]) -> list[Row]:
    """Row creating each declared folder under the owner, a templated path up to its first template field."""

    def created(path: str, folder: Path) -> Row:
        return Row(label=f"{label}.{path}[folder]", read=folder.is_dir, write=lambda _target: folder.mkdir(parents=True), target=True)

    return [
        created(path, Path(*takewhile(lambda part: "{" not in part, Path(value).parts)))
        for path, value in declared.items()
        for parent, _, name in (path.rpartition("."),)
        if isinstance(prop := (owner.path_resolve(parent) if parent else owner).bl_rna.properties.get(name), bpy.types.StringProperty) and prop.subtype == "DIR_PATH" and value
    ]


def prepared(preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, declared: Mapping[str, object]) -> tuple[list[Row], tuple[str, ...]]:
    """Rows of the folders, auto-run path, keyconfig, asset libraries, and studio lights the preferences hold, with a skip line per absent source."""
    downloads, libraries, studio = f"{Path.home() / 'Downloads'}/", preferences.filepaths.asset_libraries, Path(bpy.utils.user_resource("DATAFILES", path="studiolights"))
    lights = [(source, studio / kind / source.name) for kind, source in (("studio", Path(__file__).with_name("Headlamp.sl")), ("world", LOOK_DEVELOPMENT))]
    local: tuple[tuple[str, str, Literal["APPEND", "PACK"]], ...] = (("Assets", str(ASSETS), "APPEND"),) if stocked(ASSETS) else ()
    remotes: tuple[tuple[str, str, Literal["APPEND", "PACK"]], ...] = (("CGMatter", "https://cgmatter.github.io/website/nodes/", "PACK"), ("ambientCG", "https://ambientcg.com/api/blender/", "APPEND"))

    def autoexec(_target: object) -> None:
        for path in list(preferences.autoexec_paths):
            preferences.autoexec_paths.remove(path)
        preferences.autoexec_paths.new().path = downloads

    def assets(_target: object) -> None:
        for held in list(libraries):
            libraries.remove(held)
        for name, directory, method in local:
            libraries.new(name=name, directory=directory).import_method = method
        for name, url, method in remotes:
            bpy.ops.preferences.asset_library_add(type="REMOTE", name=name, remote_url=url)
            libraries[-1].import_method = method

    def linked(target: Path) -> str | None:
        return str(target.readlink()) if target.is_symlink() else None

    def link(source: Path, target: Path, _value: object) -> None:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.unlink(missing_ok=True)
        target.symlink_to(source)
        preferences.studio_lights.refresh()

    rows = [
        *folders("preferences", preferences, declared),
        Row(label="preferences.autoexec_paths", read=lambda: tuple((path.path, path.use_glob) for path in preferences.autoexec_paths), write=autoexec, target=((downloads, False),)),
        Row(label="keyconfigs.active", read=lambda: keyconfigs.active.name, write=lambda target: bpy.utils.keyconfig_set(bpy.utils.preset_find(target, "keyconfig")), target="Blender"),
        Row(
            label="preferences.filepaths.asset_libraries",
            read=lambda: tuple((held.name, held.remote_url if held.use_remote_url else held.path, held.import_method) for held in libraries),
            write=assets,
            target=(*local, *remotes),
        ),
        *(Row(label=f"studiolights.{target.parent.name}.{target.name}", read=partial(linked, target), write=partial(link, source, target), target=str(source)) for source, target in lights),
    ]
    return rows, () if local else (line(Kind.SKIP, str(ASSETS)),)


# --- [SETTINGS]
def declared_preferences(preferences: bpy.types.Preferences, editor: str | None) -> dict[str, object]:
    """Declared preferences by path with the text editor bundle the host found, online access before its handled flag."""
    caches, cycles = Path(bpy.app.cachedir), preferences.addons["cycles"].preferences
    style = preferences.ui_styles[0]
    fonts = [prop.identifier for prop in style.bl_rna.properties if isinstance(getattr(style, prop.identifier), bpy.types.ThemeFontStyle)]
    device = next(kind for kind, _, _, value in cycles.get_device_types(bpy.context) if value == type(cycles).default_device())
    memory = os.sysconf("SC_PHYS_PAGES") * os.sysconf("SC_PAGE_SIZE") // 4 // 2**20
    return {
        "view.ui_scale": 0.9,
        "view.ui_line_width": "THIN",
        "view.border_width": 1,
        "view.font_path_ui": str(Typography.INTERFACE.path),
        "view.font_path_ui_mono": str(Typography.MONOSPACE.path),
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
        "system.use_studio_light_edit": False,
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
        "inputs.walk_navigation.walk_speed": 5 * FOOT,
        "inputs.walk_navigation.use_gravity": True,
        "inputs.walk_navigation.jump_height": 18 * INCH,
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
        **({"filepaths.text_editor": str(Path(editor, "Contents", "Resources", "app", "bin", "code")), "filepaths.text_editor_args": "-g $filepath:$line:$column"} if editor is not None else {}),
        "filepaths.use_scripts_auto_execute": True,
        "filepaths.save_version": 3,
        "filepaths.use_load_ui": False,
        "filepaths.use_auto_save_temporary_files": False,
        "filepaths.use_tabs_as_spaces": True,
        "filepaths.save_modified_images": "ALWAYS_SAVE",
        "filepaths.use_file_compression": False,
        "filepaths.temporary_directory": f"{caches / 'temp'}/",
        "filepaths.texture_cache_directory": f"{caches / 'texture-cache'}/",
        "filepaths.render_output_directory": f"{Path(__file__).resolve().parents[3] / '.artifacts' / 'blender' / 'renders'}/{{blend_name}}/",
        "filepaths.render_cache_directory": "",
        **dict.fromkeys(
            (
                f"experimental.{name}"
                for name in (
                    "use_undo_legacy",
                    "override_auto_resync",
                    "use_cycles_debug",
                    "use_asset_indexing",
                    "write_legacy_blend_file_format",
                    "use_all_linked_data_direct",
                    "use_recompute_usercount_on_save_debug",
                    "no_data_block_packing",
                    "show_asset_debug_info",
                    "use_viewport_debug",
                    "use_paint_debug",
                    "use_extensions_debug",
                )
            ),
            False,
        ),
        "experimental.use_remote_asset_libraries": True,
        'addons["cycles"].preferences.compute_device_type': device,
        **{f"extensions.repos[{index}].{name}": False for index, _ in enumerate(preferences.extensions.repos) for name in ("use_sync_on_startup", "use_cache")},
        **{f"ui_styles[0].{font}.{name}": value for font in fonts for name, value in (("shadow", 0), ("character_weight", 400), ("points", 10 if font == "tooltip" else 11))},
    }


def declared_devices(preferences: bpy.types.Preferences) -> dict[str, object]:
    """Cycles devices of the compute type in use by path, each switched on."""
    cycles = preferences.addons["cycles"].preferences
    return {f'addons["cycles"].preferences.devices[{index}].use': True for index, device in enumerate(cycles.devices) if device.type == cycles.compute_device_type}


def declared_keymaps(user: bpy.types.KeyConfig) -> Iterator[tuple[str, bpy.types.KeyMapItem, dict[str, object]]]:
    """Edited user keymap items by path with their declared values."""
    spreadsheet = user.keymaps["Spreadsheet Generic"]
    canvases = frozenset({"EMPTY", "VIEW_3D", "NODE_EDITOR", "IMAGE_EDITOR", "CLIP_EDITOR", "GRAPH_EDITOR", "DOPESHEET_EDITOR", "NLA_EDITOR", "SEQUENCE_EDITOR"})
    displaced = frozenset({("VIEW3D_MT_mpr_append", "3D View"), ("VIEW3D_PT_library_instance_menu", "Object Mode")})
    for keymap, index, item in ((each, index, item) for each in user.keymaps for index, item in enumerate(each.keymap_items)):
        path, properties, unmodified = f'keymaps["{keymap.name}"].keymap_items[{index}]', item.properties, not (item.any or item.shift or item.ctrl or item.alt or item.oskey)
        match item.idname, item.type:
            case "wm.call_menu" | "wm.call_panel", "RIGHTMOUSE" if keymap.space_type in canvases and (
                (unmodified and item.value in {"PRESS", "CLICK"}) or (item.value == "RELEASE" and properties.name == "NODEVIEW_MT_sv_rclick_menu")
            ):
                yield path, item, {"value": "CLICK"}
            case "wm.call_menu" | "wm.call_panel", _ if (properties.name, keymap.name) in displaced:
                yield path, item, {"active": False}
            case "bim.direct_profile_edit", "SPACE" if item.value == "PRESS":
                yield path, item, {"active": False}
            case "wm.context_toggle", "T" if keymap == spreadsheet and item.value == "PRESS" and properties.data_path in {"space_data.show_region_channels", "space_data.show_region_toolbar"}:
                yield path, item, {"properties.data_path": "space_data.show_region_toolbar", "active": True}
            case "node.find_node" | "improved_node_search.search", "F" if keymap.name == "Node Editor" and item.oskey and not (item.shift or item.ctrl or item.alt) and item.value == "PRESS":
                yield path, item, {"idname": "improved_node_search.search", "active": True}
            case "object.library_instance" | "object.edit_library_instance" | "object.library_instance_ungroup" | "object.mpr_modal_edit", _:
                yield path, item, {"active": False}
            case "object.select_lasso_xray" | "mesh.select_lasso_xray", "L":
                yield path, item, {"active": False}
            case "wm.call_menu_pie", "ACCENT_GRAVE" if properties.name in {"VIEW3D_MT_PIE_univ_obj", "VIEW3D_MT_PIE_univ_edit"}:
                yield path, item, {"active": False}
            case _:
                pass


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["KEYCONFIG", "declared_devices", "declared_keymaps", "declared_preferences", "folders", "prepared"]
