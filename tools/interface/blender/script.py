# ty: ignore[invalid-argument-type, invalid-assignment, invalid-context-manager, not-iterable, not-subscriptable, unresolved-attribute]
# mypy: disable-error-code="arg-type, assignment, attr-defined, func-returns-value, index, union-attr"
# ruff: file-ignore[blind-except]
"""Blender side of the interface, run inside Blender's GUI by the host through `--python-expr`, which calls `run(report, package, port)`.

Every step writes onto the live data between two observations of every value a step may change, and the report rows name the changes, skips, and errors.
"""

from collections.abc import Iterator
from datetime import timedelta
from enum import auto, Enum, StrEnum
from functools import partial, reduce
import hashlib
from importlib import import_module
from math import ceil, floor, radians
from pathlib import Path
import re
import tomllib
import traceback
from types import ModuleType
from typing import Final, Literal
import zipfile

import addon_utils
import bpy
from layout import BOTTOM_STRIP, NODE_EDITOR, ORGANIZER_ROWS, RIGHT_COLUMN
from location import CRS, DAYLIGHT, ELEVATION, LATITUDE, LONGITUDE, MOMENT, NORTH, OFFSET
from mathutils import Color as Linear, Matrix, Vector
from radix import Rgb
from render import (
    CAUSTICS,
    DESIGN_TOOLS,
    DIFFUSE_BOUNCES,
    DPI,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    INDIRECT_CLAMP,
    MATERIALS,
    MAX_BOUNCES,
    NOISE_THRESHOLD,
    SAMPLES,
    stocked,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from theme import (
    Accent,
    Axis,
    blend,
    FACE_FILL_ALPHA,
    Field,
    FRAME_NODE_ALPHA,
    GRID_MAJOR_ALPHA,
    GRID_MINOR_ALPHA,
    Guide,
    Line,
    Selection,
    SELECTION_FILL_ALPHA,
    Status,
    Surface,
    Tag,
    Text,
    TEXT_WEIGHT,
    Typography,
)
from units import FOOT, INCH, Units

# --- [TYPES] ----------------------------------------------------------------------------


class Side(StrEnum):
    """Edge of an area a resize moves, as the report names it."""

    TOP = "top"
    LEFT = "left"
    RIGHT = "right"

    def extent(self, area: bpy.types.Area) -> int:
        """The area's size along the axis the edge moves."""
        return area.height if self is Side.TOP else area.width

    def grip(self, area: bpy.types.Area, pixel: int) -> tuple[int, int]:
        """Window position one pixel across the edge, midway along it, where a drag moves it."""
        match self:
            case Side.TOP:
                return (area.x + area.width // 2, area.y + area.height - 1 + pixel)
            case Side.LEFT:
                return (area.x - pixel, area.y + area.height // 2)
            case Side.RIGHT:
                return (area.x + area.width - 1 + pixel, area.y + area.height // 2)


class Addon(StrEnum):
    """Identity of every add-on a row, a workspace, or a step names, as `identity` returns it."""

    SVERCHOK = "Sverchok"
    CAD_SKETCHER = "CAD_Sketcher"
    EXTRA_MESH_OBJECTS = "extra_mesh_objects"
    BOOL_TOOL = "bool_tool"
    AMBIENTCG = "ambientcg_material_importer"
    BLENDER_GIS = "BlenderGIS"
    MCP = "mcp"
    MCP_FOR_BLENDER = "MCP for Blender"
    BONSAI = "bonsai"
    DIMENSIONS = "dimensions"
    CURVE_PROFILE_CREATOR = "curve_profile_creator"
    MEASUREIT_ARCH = "MeasureIt_ARCH"
    PIN_SOLVER = "pin_solver"
    SUN_POSITION = "sun_position"
    LADYBUG_TOOLS = "ladybug_tools"
    NODE_WRANGLER = "Node Wrangler"
    CAD_HELPER = "cad_helper"
    ARCH_GENERATOR = "arch_generator"
    SURFACEPSYCHO = "surfacepsycho"
    STRUCT_TOPO_OPT = "struct_topo_opt"
    BLENDIFF = "blendiff"
    STEP_IMPORTER = "step_importer"
    POINT_CLOUD_IO = "point_cloud_io"
    ATTRIO_CSV = "attrio_csv"
    POHLKE = "pohlke"
    STB_SECTION_TOOLBOX = "stb_section_toolbox"
    NODE_TO_PYTHON = "node_to_python"
    TREE_CLIPPER = "tree_clipper"
    FORMULA_TO_NODES = "formula_to_nodes"
    JUPYTER_BLENDER = "jupyter_blender"
    VI_SUITE = "VI-Suite"
    MEASUREIT = "measureit"
    STEPPER_REBORN = "stepper_reborn"
    DYNAMIC_SKY = "dynamic_sky"
    INDUSTRIAL_AOV_CONNECTOR = "Industrial_AOV_Connector"
    THEBETTERBAKER = "thebetterbaker"
    MACBLEND = "macblend"
    BAGAPIE = "Bagapie"
    PRINT3D_TOOLBOX = "print3d_toolbox"
    THREEMF_IO = "ThreeMF_io"
    EXPORT_PAPER_MODEL = "export_paper_model"
    MMGPY = "mmgpy"
    LATEX_TEXT_GENERATOR = "latex_text_generator"
    SCENETRACE = "scenetrace"
    RENDERCUE = "rendercue"
    LIGHT_IT_UP = "light_it_up"
    HOME_BUILDER_5 = "home_builder_5"
    HARDFLOW = "hardflow"
    BLENDGUARD = "blendguard"
    POSE_LIBRARY = "Pose Library"


class Store(Enum):
    """Where an add-on keeps a setting: its preferences, the scene, the scene's Bonsai solar group written as raw items, or the BlenderGIS georeference."""

    PREFERENCES = auto()
    SCENE = auto()
    SOLAR = auto()
    GEOREFERENCE = auto()

    def owner(self, scene: bpy.types.Scene, module: str) -> object:
        """The struct the store's paths start from."""
        match self:
            case Store.PREFERENCES:
                return bpy.context.preferences.addons[module].preferences
            case Store.SCENE:
                return scene
            case Store.SOLAR:
                return scene.BIMSolarProperties
            case Store.GEOREFERENCE:
                return import_module(f"{module}.geoscene").GeoScene(scene)

    def write(self, owner: object, path: str, value: object) -> None:
        """Set the dotted path under the owner, an item on the solar group, and a color role in the color space the property declares."""
        *parents, name = path.split(".")
        match self, value:
            case Store.SOLAR, _:
                owner[path] = value
            case _, Paint():
                value.apply(reduce(getattr, parents, owner), name)
            case _:
                setattr(reduce(getattr, parents, owner), name, value)


# --- [CONSTANTS] ------------------------------------------------------------------------


LEFT_COLUMN: Final = 395
TICK: Final = 0.1
RIGHT: Final = ("PROPERTIES", "OUTLINER")
NODE_ADDONS: Final = (Addon.SVERCHOK, Addon.NODE_WRANGLER, Addon.NODE_TO_PYTHON, Addon.TREE_CLIPPER, Addon.FORMULA_TO_NODES, Addon.AMBIENTCG)
PLAN_DISTANCE: Final = 100 * FOOT
EYE_HEIGHT: Final = 66 * INCH

# --- [MODELS] ---------------------------------------------------------------------------


class Frame(Enum):
    """One workspace by its tab name: its source and main column editors from the top, its Properties context, its 3D view's shading, solid color, and plan axis, and the add-ons shown only in the workspaces that name them."""

    left: tuple[str, ...]
    center: tuple[str, ...]
    context: str
    shading: str
    color: str
    axis: str | None
    addons: tuple[Addon, ...]

    Model = (
        (),
        ("VIEW_3D",),
        "OBJECT",
        "SOLID",
        "SINGLE",
        None,
        (
            Addon.BOOL_TOOL,
            Addon.CAD_HELPER,
            Addon.ARCH_GENERATOR,
            Addon.SURFACEPSYCHO,
            Addon.STRUCT_TOPO_OPT,
            Addon.PIN_SOLVER,
            Addon.BLENDIFF,
            Addon.STEP_IMPORTER,
            Addon.POINT_CLOUD_IO,
            Addon.ATTRIO_CSV,
            Addon.CURVE_PROFILE_CREATOR,
        ),
    )
    BIM = (("PROPERTIES",), ("VIEW_3D",), "SCENE", "SOLID", "MATERIAL", None, (Addon.BONSAI,))
    Nodes = (("SPREADSHEET",), ("VIEW_3D", "GeometryNodeTree", "TIMELINE"), "MODIFIER", "SOLID", "SINGLE", None, NODE_ADDONS)
    Drafting = ((), ("VIEW_3D",), "SCENE", "SOLID", "SINGLE", "TOP", (Addon.DIMENSIONS, Addon.MEASUREIT_ARCH, Addon.POHLKE, Addon.STB_SECTION_TOOLBOX, Addon.LADYBUG_TOOLS, Addon.BLENDER_GIS))
    Shading = (("FILES",), ("VIEW_3D", "ShaderNodeTree"), "MATERIAL", "MATERIAL", "SINGLE", None, NODE_ADDONS)
    Render = ((), ("IMAGE_EDITOR", "CompositorNodeTree", "TIMELINE"), "RENDER", "SOLID", "SINGLE", None, ())
    Script = (("CONSOLE",), ("VIEW_3D", "TEXT_EDITOR"), "OBJECT", "SOLID", "SINGLE", None, (Addon.JUPYTER_BLENDER, Addon.MCP_FOR_BLENDER))

    def __init__(self, left: tuple[str, ...], center: tuple[str, ...], context: str, shading: str, color: str, axis: str | None, addons: tuple[Addon, ...]) -> None:
        """Bind the member's tuple to its named fields."""
        self.left, self.center, self.context, self.shading, self.color, self.axis, self.addons = left, center, context, shading, color, axis, addons


class Paint:
    """A color role at an alpha, None keeping the alpha the property holds."""

    __slots__ = ("alpha", "rgb")

    def __init__(self, rgb: Rgb, alpha: float | None = 1.0) -> None:
        """Hold the role and its alpha."""
        self.rgb, self.alpha = rgb, alpha

    def apply(self, struct: bpy.types.bpy_struct, name: str) -> None:
        """Write the role where it differs, in the color space and channel count the property declares."""
        prop = struct.bl_rna.properties[name]
        held = tuple(round(channel, 4) for channel in getattr(struct, name))
        channels = linear(self.rgb) if prop.subtype == "COLOR" else unit(self.rgb)
        alpha = held[3:] if self.alpha is None else (round(round(self.alpha * 255) / 255, 4),)
        wanted = channels if prop.array_length == 3 else (*channels, *alpha)
        if held != wanted:
            setattr(struct, name, wanted)


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [CONTEXT]
def override(window: bpy.types.Window, area: bpy.types.Area | None = None, region: bpy.types.Region | None = None) -> bpy.types.ContextTempOverride:
    """Context override on the window and its screen."""
    return bpy.context.temp_override(window=window, screen=window.screen, area=area, region=region)


# --- [COLOR]
def unit(rgb: Rgb) -> tuple[float, ...]:
    """Float channels of a color role rounded to the byte Blender stores."""
    return tuple(round(channel / 255, 4) for channel in rgb)


def linear(rgb: Rgb) -> tuple[float, ...]:
    """A color role in scene linear, rounded as `unit` rounds."""
    return tuple(round(channel, 4) for channel in Linear(unit(rgb)).from_srgb_to_scene_linear())


# --- [OBSERVATION]
def snapshot(struct: bpy.types.bpy_struct, prefix: str) -> dict[str, object]:
    """Every writable property under the struct by dotted path, without collections, ID pointers, secrets, and values Blender changes on its own."""
    transient = {("Preferences", "is_dirty"), ("SpaceNodeEditor", "cursor_location"), ("SpaceConsole", "select_start"), ("SpaceConsole", "select_end")}
    rows: dict[str, object] = {}
    for prop in struct.bl_rna.properties:
        path = f"{prefix}{prop.identifier}"
        match prop:
            case bpy.types.PointerProperty():
                value = getattr(struct, prop.identifier)
                if prop.identifier != "rna_type" and isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
                    rows.update(snapshot(value, f"{path}."))
            case bpy.types.CollectionProperty():
                continue
            case _ if prop.is_readonly or prop.subtype == "PASSWORD" or (type(struct).__name__, prop.identifier) in transient:
                continue
            case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty() if prop.array_length:
                value = getattr(struct, prop.identifier)
                rows[path] = tuple(map(tuple, value)) if prop.array_dimensions[1] else tuple(value)
            case _:
                rows[path] = getattr(struct, prop.identifier)
    return rows


def colors(struct: bpy.types.bpy_struct, prefix: str) -> Iterator[tuple[bpy.types.bpy_struct, str, str]]:
    """Every color property under the theme struct with its dotted path, collection items indexed."""
    for prop in struct.bl_rna.properties:
        path = f"{prefix}{prop.identifier}"
        match prop:
            case bpy.types.PointerProperty() if prop.identifier != "rna_type":
                yield from colors(getattr(struct, prop.identifier), f"{path}.")
            case bpy.types.CollectionProperty():
                for index, item in enumerate(getattr(struct, prop.identifier)):
                    yield from colors(item, f"{path}[{index}].")
            case bpy.types.FloatProperty() if prop.subtype in {"COLOR", "COLOR_GAMMA"}:
                yield struct, prop.identifier, path


def settings(properties: bpy.types.OperatorProperties) -> dict[str, object]:
    """The operator properties a keymap item sets, by identifier."""
    return {prop.identifier: getattr(properties, prop.identifier) for prop in properties.bl_rna.properties if not prop.is_readonly and properties.is_property_set(prop.identifier)}


def observe(preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, scene: bpy.types.Scene, package: Path) -> dict[str, object]:
    """Every value a step may change, by path, the installed extension as the digest of its files."""
    world, theme, (_, folder, _) = scene.world.node_tree, preferences.themes[0], extension(preferences, package)
    return {
        f"extension/{folder.name}": hashlib.sha256(b"\0".join(part for path, content in sorted(installed_files(folder).items()) for part in (path.encode(), content))).hexdigest(),
        **snapshot(preferences, "preferences."),
        **snapshot(theme, "theme."),
        **{
            path: value
            for prop in theme.bl_rna.properties
            if isinstance(prop, bpy.types.CollectionProperty)
            for index, item in enumerate(getattr(theme, prop.identifier))
            for path, value in snapshot(item, f"theme.{prop.identifier}[{index}].").items()
        },
        **snapshot(preferences.ui_styles[0], "style."),
        **snapshot(keyconfigs.active.preferences, "keyconfig."),
        **{path: value for repo in preferences.extensions.repos for path, value in snapshot(repo, f"repos/{repo.module}.").items()},
        **{path: value for addon in preferences.addons if addon.preferences is not None for path, value in snapshot(addon.preferences, f"addons/{addon.module}.").items()},
        **{f"addons/cycles.devices/{device.id}": device.use for device in preferences.addons["cycles"].preferences.devices},
        **snapshot(scene, "scene."),
        **{f"scene[{key}]": str(value) for key, value in scene.items()},
        **{path: value for index, slot in enumerate(scene.transform_orientation_slots) for path, value in snapshot(slot, f"scene.transform_orientation_slots[{index}].").items()},
        **{path: value for block in (*bpy.data.lights, *bpy.data.cameras) for path, value in snapshot(block, f"{type(block).__name__}/{block.name}.").items()},
        **{f"Object/{target.name}.matrix_world": tuple(map(tuple, target.matrix_world)) for target in (scene.camera, light(scene))},
        **{path: value for node in world.nodes for path, value in snapshot(node, f"world/{node.name}.").items()},
        "world links": tuple(sorted((repr(link.from_socket), repr(link.to_socket)) for link in world.links)),
        **{
            path: value
            for workspace in bpy.data.workspaces
            for index, area in enumerate(workspace.screens[0].areas)
            for path, value in snapshot(area.spaces.active, f"{workspace.name}/{index}:{area.ui_type}.").items()
        },
        **{f"{workspace.name}/{index}:{area.ui_type}.show_menus": area.show_menus for workspace in bpy.data.workspaces for index, area in enumerate(workspace.screens[0].areas)},
        **{f"{workspace.name}.{name}": getattr(workspace, name) for workspace in bpy.data.workspaces for name in ("object_mode", "use_pin_scene", "use_filter_by_owner")},
        **{f"{workspace.name}/screen.show_statusbar": workspace.screens[0].show_statusbar for workspace in bpy.data.workspaces},
        **{f"{workspace.name}.owner_ids": tuple(sorted(owner.name for owner in workspace.owner_ids)) for workspace in bpy.data.workspaces},
        **{
            f"{workspace.name}/{index}:{area.ui_type}.bonsai_tab": workspace.screens[0].BIMAreaProperties[index].tab
            for workspace in (bpy.data.workspaces if Addon.BONSAI in enabled(preferences) else ())
            for index, area in enumerate(workspace.screens[0].areas)
            if area.type == "PROPERTIES"
        },
        **{f"{workspace.name}/tool/{tool.space_type}/{tool.mode}": tool.idname for workspace in bpy.data.workspaces for tool in workspace.tools},
        **{
            f"keymap/{keymap.name}/{index}": (item.idname, item.to_string(), item.value, item.direction, item.active, () if item.properties is None else tuple(settings(item.properties).items()))
            for keymap in keyconfigs.user.keymaps
            for index, item in enumerate(keymap.keymap_items)
        },
        "workspaces": tuple(workspace.name for workspace in bpy.data.workspaces),
        "addons": tuple(sorted(addon.module for addon in preferences.addons)),
        "autoexec_paths": tuple((path.path, path.use_glob) for path in preferences.autoexec_paths),
        "asset_libraries": tuple((library.name, library.path, library.import_method) for library in preferences.filepaths.asset_libraries),
        "blocks": tuple(sorted((type(block).__name__, block.name) for block in (*bpy.data.objects, *bpy.data.meshes, *bpy.data.materials, *bpy.data.texts))),
    }


# --- [PREFERENCES]
def apply_preferences(preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations) -> None:
    """Interface, fonts, viewport, asset, input, navigation, editing, file, system, developer, render device, keyconfig, and extension repository preferences."""
    view, system, inputs, edit, filepaths, experimental = preferences.view, preferences.system, preferences.inputs, preferences.edit, preferences.filepaths, preferences.experimental
    caches, downloads = Path(bpy.app.cachedir), f"{Path.home() / 'Downloads'}/"
    for directory in (caches / "temp", caches / "texture-cache", caches / "gis"):
        directory.mkdir(parents=True, exist_ok=True)
    view.ui_scale, view.ui_line_width, view.border_width = 0.9, "THIN", 1
    view.font_path_ui, view.font_path_ui_mono = str(Typography.INTERFACE.path), str(Typography.MONOSPACE.path)
    view.show_splash, view.use_save_prompt, view.use_reduce_motion, view.smooth_view = False, False, True, 0
    view.show_developer_ui = view.show_tooltips = view.show_tooltips_python = view.show_addons_enabled_only = True
    view.show_navigate_ui, view.header_align, view.mini_axis_type = False, "TOP", "GIZMO"
    view.date_format, view.time_format = "ME_SLASH", "H12"
    view.show_statusbar_stats, view.show_statusbar_memory, view.show_statusbar_version, view.show_statusbar_scene_duration = True, False, False, False
    view.show_playback_fps = view.show_object_info = view.show_view_name = True
    view.render_display_type, view.filebrowser_display_type, view.preferences_display_type = "NONE", "SCREEN", "SCREEN"
    view.text_hinting, view.pie_tap_timeout, view.view2d_grid_spacing_min = "NONE", 20, 35
    view.use_text_antialiasing = view.use_text_render_subpixelaa = True
    view.gizmo_size, view.gizmo_size_navigate_v3d, view.lookdev_sphere_size = 65, 70, 100
    system.use_region_overlap, system.show_panel_tabs_compact, system.use_online_access = False, True, True
    view.asset_access, preferences.asset_libraries.use_online_essentials = "ALL", True
    system.scrollback, system.memory_cache_limit = 32768, 16384
    system.anisotropic_filter, system.viewport_aa = "FILTER_16", "16"
    system.use_studio_light_edit, system.use_overlay_smooth_wire, system.use_edit_mode_smooth_wire = False, True, True
    inputs.drag_threshold_mouse, inputs.drag_threshold, inputs.mouse_double_click_time = 3, 30, 350
    inputs.use_mouse_emulate_3_button, inputs.use_drag_immediately = False, True
    inputs.use_emulate_numpad = inputs.use_multitouch_gestures = inputs.use_mouse_continuous = inputs.use_numeric_input_advanced = True
    inputs.view_rotate_method, inputs.use_rotate_around_active = "TURNTABLE", False
    inputs.use_auto_perspective = inputs.use_mouse_depth_navigate = inputs.use_zoom_to_mouse = True
    inputs.view_zoom_method, inputs.view_zoom_axis = "DOLLY", "VERTICAL"
    inputs.invert_mouse_zoom = inputs.invert_zoom_wheel = False
    inputs.walk_navigation.view_height = EYE_HEIGHT
    edit.object_align, edit.use_enter_edit_mode, edit.use_auto_keying, edit.use_text_edit_auto_close = "WORLD", False, False, True
    edit.undo_steps, edit.undo_memory_limit = 256, 16384
    edit.keyframe_new_interpolation_type, edit.collection_instance_empty_size = "LINEAR", FOOT
    Paint(Guide.CONSTRUCTION).apply(edit, "grease_pencil_default_color")
    filepaths.font_directory = f"{DESIGN_TOOLS / 'fonts'}/"
    filepaths.use_scripts_auto_execute, filepaths.save_version, filepaths.auto_save_time, filepaths.use_load_ui = True, 3, 1, False
    filepaths.use_auto_save_temporary_files, filepaths.use_tabs_as_spaces = True, True
    filepaths.save_modified_images, filepaths.use_file_compression = "ALWAYS_SAVE", False
    filepaths.temporary_directory, filepaths.texture_cache_directory = f"{caches / 'temp'}/", f"{caches / 'texture-cache'}/"
    filepaths.render_output_directory, filepaths.render_cache_directory = "//render/", ""
    preferences.extensions.use_online_access_handled = True
    experimental.use_undo_legacy = experimental.override_auto_resync = experimental.use_cycles_debug = experimental.use_asset_indexing = False
    experimental.write_legacy_blend_file_format = experimental.use_all_linked_data_direct = experimental.use_recompute_usercount_on_save_debug = False
    experimental.no_data_block_packing = False
    for path in [path for path in preferences.autoexec_paths if (path.path, path.use_glob) != (downloads, False)]:
        preferences.autoexec_paths.remove(path)
    if not preferences.autoexec_paths:
        preferences.autoexec_paths.new().path = downloads
    cycles = preferences.addons["cycles"].preferences
    cycles.compute_device_type = next(kind for kind, *_ in cycles.get_device_types(bpy.context) if kind != "NONE")
    cycles.refresh_devices()
    for device in (device for device in cycles.devices if device.type == cycles.compute_device_type):
        device.use = True
    for repo in preferences.extensions.repos:
        repo.use_sync_on_startup = False
    for font in (getattr(preferences.ui_styles[0], prop.identifier) for prop in preferences.ui_styles[0].bl_rna.properties):
        if isinstance(font, bpy.types.ThemeFontStyle):
            font.shadow, font.character_weight, font.points = 0, TEXT_WEIGHT, 11
    if keyconfigs.active.name != "Blender":
        bpy.utils.keyconfig_set(bpy.utils.preset_find("Blender", "keyconfig"))
    keymap = keyconfigs.active.preferences
    for name, value in (("select_mouse", "LEFT"), ("spacebar_action", "SEARCH"), ("use_pie_click_drag", True), ("v3d_alt_mmb_drag_action", "ABSOLUTE")):
        if getattr(keymap, name) != value:
            setattr(keymap, name, value)


# --- [ADDONS]
def identity(module: ModuleType) -> str:
    """Name an add-on row keys on: the manifest id of an extension, the registered name of an add-on outside the extension system."""
    return module.__name__.rpartition(".")[2] if module.__name__.startswith("bl_ext.") else module.bl_info["name"]


def installed() -> dict[str, str]:
    """Module name of every installed add-on by the name its rows key on."""
    return {identity(module): module.__name__ for module in addon_utils.modules()}


def enabled(preferences: bpy.types.Preferences) -> dict[str, str]:
    """Module name of every enabled add-on by the name its rows key on."""
    return {name: module for name, module in installed().items() if module in preferences.addons}


def addon_settings(scene: bpy.types.Scene, port: int, modules: dict[str, str]) -> tuple[tuple[Addon, Store, str, object], ...]:
    """Every add-on setting the run owns as its add-on, store, path, and value, in the order the add-ons' update chains need."""
    denominator, standard, quarter = str(round(INCH / Units.IMPERIAL.resolution)), OFFSET / timedelta(hours=1), INCH / 4
    declared = {
        "door.overall_width": 3 * FOOT,
        "door.overall_height": 7 * FOOT,
        "window.overall_width": 3 * FOOT,
        "window.overall_height": 4 * FOOT,
        "stair.width": 3 * FOOT + 8 * INCH,
        "stair.tread_run": 11 * INCH,
        "railing.height": 3 * FOOT + 6 * INCH,
    }
    groups = () if (bonsai := modules.get(Addon.BONSAI)) is None else Store.PREFERENCES.owner(scene, bonsai).bl_rna.properties["default_parameters"].fixed_type.properties
    lengths = {
        f"{group.identifier}.{prop.identifier}": prop
        for group in groups
        if isinstance(group, bpy.types.PointerProperty)
        for prop in group.fixed_type.properties
        if isinstance(prop, bpy.types.FloatProperty) and prop.unit == "LENGTH" and prop.identifier != "total_length_target"
    }
    return (
        *((Addon.SVERCHOK, Store.PREFERENCES, name, False) for name in ("log_to_buffer", "auto_apply_theme", "apply_theme_on_open")),
        (Addon.SVERCHOK, Store.PREFERENCES, "exception_color", Paint(Status.ERROR)),
        (Addon.SVERCHOK, Store.PREFERENCES, "no_data_color", Paint(Status.INFO)),
        *((Addon.SVERCHOK, Store.PREFERENCES, f"color_{kind}", Paint(Surface.BOX)) for kind in ("viz", "tex", "sce", "lay", "gen")),
        (Addon.CAD_SKETCHER, Store.PREFERENCES, "imperial_precision", denominator),
        (Addon.CAD_SKETCHER, Store.PREFERENCES, "show_whats_new", False),
        (Addon.CAD_SKETCHER, Store.PREFERENCES, "entity_scale", 1.0),
        *(
            (Addon.CAD_SKETCHER, Store.PREFERENCES, f"theme_settings.{path}", Paint(rgb))
            for path, rgb in (
                ("entity.default", Line.GEOMETRY),
                ("entity.highlight", Selection.HOVER),
                ("entity.selected", Selection.ITEM),
                ("entity.selected_highlight", Selection.HOVER),
                ("entity.inactive", Line.GRID),
                ("entity.inactive_selected", Selection.INACTIVE),
                ("entity.fixed", Line.LOCKED),
                ("constraint.default", Guide.CONSTRUCTION),
                ("constraint.highlight", Selection.HOVER),
                ("constraint.failed", Status.ERROR),
                ("constraint.failed_highlight", Text.ERROR),
                ("constraint.reference", Guide.HANDLE),
                ("constraint.reference_highlight", Selection.HOVER),
                ("constraint.text", Text.PRIMARY),
                ("constraint.text_highlight", Selection.HOVER),
            )
        ),
        (Addon.EXTRA_MESH_OBJECTS, Store.PREFERENCES, "show_gemstones", False),
        (Addon.EXTRA_MESH_OBJECTS, Store.PREFERENCES, "show_gears", False),
        (Addon.BOOL_TOOL, Store.PREFERENCES, "solver", "EXACT"),
        (Addon.AMBIENTCG, Store.PREFERENCES, "cache_dir", str(MATERIALS)),
        (Addon.BLENDER_GIS, Store.PREFERENCES, "logLevel", "INFO"),
        (Addon.BLENDER_GIS, Store.PREFERENCES, "overpassServer", "https://overpass.kumi.systems/api/interpreter"),
        (Addon.BLENDER_GIS, Store.PREFERENCES, "cacheFolder", str(Path(bpy.app.cachedir) / "gis")),
        (Addon.BLENDER_GIS, Store.PREFERENCES, "forceTexturedSolid", False),
        (Addon.MCP, Store.PREFERENCES, "port", port),
        (Addon.MCP, Store.PREFERENCES, "use_autostart", True),
        (Addon.MCP, Store.PREFERENCES, "autostart_delay", 0.0),
        (Addon.MCP, Store.PREFERENCES, "timer_interval_active", 0.05),
        (Addon.MCP, Store.PREFERENCES, "timer_interval_idle", 0.1),
        (Addon.MCP, Store.PREFERENCES, "timer_interval_idle_delay", 60.0),
        (Addon.MCP, Store.PREFERENCES, "use_log", False),
        (Addon.MCP_FOR_BLENDER, Store.PREFERENCES, "telemetry_consent", False),
        *((Addon.MCP_FOR_BLENDER, Store.SCENE, f"blendermcp_use_{service}", True) for service in ("polyhaven", "sketchfab", "polypizza", "hyper3d", "hunyuan3d")),
        (Addon.MCP_FOR_BLENDER, Store.SCENE, "blendermcp_hunyuan3d_mode", "OFFICIAL_API"),
        (Addon.MCP_FOR_BLENDER, Store.SCENE, "blendermcp_hunyuan3d_intl_pro", True),
        (Addon.MCP_FOR_BLENDER, Store.SCENE, "blendermcp_hunyuan3d_texture", True),
        (Addon.MCP_FOR_BLENDER, Store.SCENE, "blendermcp_sketchfab_api_key", ""),
        (Addon.MCP_FOR_BLENDER, Store.SCENE, "blendermcp_polypizza_api_key", ""),
        (Addon.BONSAI, Store.PREFERENCES, "activate_workspace", False),
        (Addon.BONSAI, Store.PREFERENCES, "should_setup_workspace", False),
        (Addon.BONSAI, Store.PREFERENCES, "should_use_snap", False),
        (Addon.BONSAI, Store.PREFERENCES, "should_setup_toolbar", False),
        (Addon.BONSAI, Store.PREFERENCES, "doc.imperial_precision", f"1/{denominator}"),
        (Addon.BONSAI, Store.PREFERENCES, "decorations_colour", Paint(Line.GEOMETRY)),
        (Addon.BONSAI, Store.PREFERENCES, "decorator_color_selected", Paint(Selection.ITEM)),
        (Addon.BONSAI, Store.PREFERENCES, "decorator_color_unselected", Paint(Line.GEOMETRY)),
        (Addon.BONSAI, Store.PREFERENCES, "decorator_color_special", Paint(Guide.CONSTRUCTION)),
        (Addon.BONSAI, Store.PREFERENCES, "decorator_color_error", Paint(Status.ERROR)),
        (Addon.BONSAI, Store.PREFERENCES, "decorator_color_background", Paint(Surface.SHADED)),
        (Addon.BONSAI, Store.SCENE, "BIMProperties.area_unit", "square foot"),
        (Addon.BONSAI, Store.SCENE, "BIMProperties.volume_unit", "cubic foot"),
        (Addon.BONSAI, Store.SCENE, "BIMProperties.section_plane_colour", Paint(Surface.SECTION)),
        *(
            (
                Addon.BONSAI,
                Store.PREFERENCES,
                f"default_parameters.{path}",
                declared.get(path, tuple(round(value / quarter) * quarter for value in prop.default_array) if prop.array_length else round(prop.default / quarter) * quarter),
            )
            for path, prop in lengths.items()
        ),
        (Addon.BONSAI, Store.SOLAR, "latitude", LATITUDE),
        (Addon.BONSAI, Store.SOLAR, "longitude", LONGITUDE),
        (Addon.BONSAI, Store.SOLAR, "timezone", str(MOMENT.tzinfo)),
        (Addon.BONSAI, Store.SOLAR, "true_north", radians(NORTH)),
        *((Addon.BONSAI, Store.SOLAR, field, getattr(MOMENT, field)) for field in ("year", "month", "day", "hour", "minute")),
        (Addon.DIMENSIONS, Store.SCENE, "dimensions_settings.imperial_denominator", denominator),
        (Addon.DIMENSIONS, Store.SCENE, "dimensions_settings.show_selected_object_overlay", False),
        (Addon.DIMENSIONS, Store.SCENE, "dimensions_settings.dimension_color", Paint(Line.GEOMETRY)),
        (Addon.DIMENSIONS, Store.SCENE, "dimensions_settings.selected_dimension_color", Paint(Selection.ITEM)),
        (Addon.DIMENSIONS, Store.SCENE, "dimensions_settings.guide_color", Paint(Guide.TRACKING)),
        *((Addon.DIMENSIONS, Store.SCENE, f"dimensions_settings.{kind}_line_width", 1.0) for kind in ("dimension", "guide")),
        (Addon.DIMENSIONS, Store.PREFERENCES, "default_offset_distance", FOOT),
        (Addon.DIMENSIONS, Store.PREFERENCES, "empty_display_size", 2 * INCH),
        (Addon.CURVE_PROFILE_CREATOR, Store.SCENE, "cpc_settings.imperial_fraction_denominator", denominator),
        (Addon.MEASUREIT_ARCH, Store.SCENE, "MeasureItArchProps.imperial_precision", denominator),
        (Addon.MEASUREIT_ARCH, Store.SCENE, "MeasureItArchProps.default_color", Paint(Line.GEOMETRY)),
        (Addon.PIN_SOLVER, Store.SCENE, "pinsolver_settings.text_color", Paint(Text.PRIMARY)),
        (Addon.PIN_SOLVER, Store.SCENE, "pinsolver_settings.text_outline_color", Paint(Line.GEOMETRY)),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.sun_object", light(scene)),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.sun_distance", 0.0),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.sky_texture", sky(scene).name),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.latitude", LATITUDE),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.longitude", LONGITUDE),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.UTC_zone", standard),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.use_daylight_savings", bool(DAYLIGHT)),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.north_offset", -radians(NORTH)),
        *((Addon.SUN_POSITION, Store.SCENE, f"sun_pos_properties.{field}", getattr(MOMENT, field)) for field in ("year", "month", "day")),
        (Addon.SUN_POSITION, Store.SCENE, "sun_pos_properties.time", MOMENT.hour + MOMENT.minute / 60),
        (Addon.LADYBUG_TOOLS, Store.SCENE, "ladybug.latitude", LATITUDE),
        (Addon.LADYBUG_TOOLS, Store.SCENE, "ladybug.longitude", LONGITUDE),
        (Addon.LADYBUG_TOOLS, Store.SCENE, "ladybug.time_zone", standard),
        (Addon.LADYBUG_TOOLS, Store.SCENE, "ladybug.elevation", ELEVATION),
        (Addon.LADYBUG_TOOLS, Store.SCENE, "ladybug.north", NORTH),
        (Addon.BLENDER_GIS, Store.GEOREFERENCE, "lon", LONGITUDE),
        (Addon.BLENDER_GIS, Store.GEOREFERENCE, "lat", LATITUDE),
        (Addon.BLENDER_GIS, Store.GEOREFERENCE, "crs", CRS),
    )


def apply_addons(preferences: bpy.types.Preferences) -> None:
    """Disable every installed add-on the set leaves out, VI-Suite first, then enable Node Wrangler."""
    modules = enabled(preferences)
    for module in [
        modules[name]
        for name in (
            Addon.VI_SUITE,
            Addon.MEASUREIT,
            Addon.STEPPER_REBORN,
            Addon.DYNAMIC_SKY,
            Addon.INDUSTRIAL_AOV_CONNECTOR,
            Addon.THEBETTERBAKER,
            Addon.MACBLEND,
            Addon.BAGAPIE,
            Addon.PRINT3D_TOOLBOX,
            Addon.THREEMF_IO,
            Addon.EXPORT_PAPER_MODEL,
            Addon.MMGPY,
            Addon.LATEX_TEXT_GENERATOR,
            Addon.SCENETRACE,
            Addon.RENDERCUE,
            Addon.LIGHT_IT_UP,
            Addon.HOME_BUILDER_5,
            Addon.HARDFLOW,
            Addon.BLENDGUARD,
            Addon.POSE_LIBRARY,
        )
        if name in modules
    ]:
        bpy.ops.preferences.addon_disable(module=module)
    if Addon.NODE_WRANGLER not in modules:
        bpy.ops.preferences.addon_enable(module=installed()[Addon.NODE_WRANGLER])


def apply_addon_settings(preferences: bpy.types.Preferences, scene: bpy.types.Scene, port: int) -> Iterator[str]:
    """Write every setting of an enabled add-on and yield a skip row per add-on that is disabled or absent."""
    modules = enabled(preferences)
    rows = addon_settings(scene, port, modules)
    for name, store, path, value in rows:
        if name in modules:
            store.write(store.owner(scene, modules[name]), path, value)
    yield from dict.fromkeys(f"skip\t{name}" for name, *_ in rows if name not in modules)


# --- [THEME]
def shaded(rgb: Rgb, lift: int) -> Rgb:
    """The color less the shade Blender adds when it draws a grid line."""
    red, green, blue = (channel - lift for channel in rgb)
    return (red, green, blue)


def apply_theme(preferences: bpy.types.Preferences) -> None:
    """Every color leaf the role table names, the flat one-pixel chrome, and the fills a state draws, each written where it differs."""
    minor_lift, major_lift = 10, 20
    (minor, *_), (major, *_) = (blend(Line.GRID, Surface.CANVAS, alpha) for alpha in (GRID_MINOR_ALPHA, GRID_MAJOR_ALPHA))
    canvas, *_ = Surface.CANVAS
    image_alpha = (major - minor) / (major_lift - minor_lift)
    image = round(canvas + (minor - canvas) / image_alpha) - minor_lift
    text, *_ = Text.PRIMARY
    hover, *_ = Surface.HOVER
    menu = next(byte for byte in range(256) if int(0.8 * byte + 0.2 * text) == hover)
    list_red, list_green, list_blue = (max(0, 2 * selected - active) for selected, active in zip(Accent.ROW_SELECTED, Accent.ROW_ACTIVE, strict=True))
    widget_roles = (
        (r"wcol_(regular|tool|radio|text|option|toggle|num|numslider|menu)\.inner", Paint(Surface.FIELD)),
        (r"wcol_toolbar_item\.inner", Paint(Surface.PANEL)),
        (r"wcol_box\.inner", Paint(Surface.WELL)),
        (r"wcol_menu_item\.inner", Paint((menu, menu, menu), 0.0)),
        (r"wcol_(pulldown|scroll)\.inner", Paint(Surface.FRAME, 0.0)),
        (r"wcol_list_item\.inner", Paint(Surface.WELL, 0.0)),
        (r"wcol_\w+\.inner", Paint(Surface.FRAME)),
        (r"wcol_(text|num|numslider)\.inner_sel", Paint(Field.EDITING)),
        (r"wcol_curve\.inner_sel", Paint(Surface.FRAME)),
        (r"wcol_option\.inner_sel", Paint(Accent.CHECKBOX_CHECKED)),
        (r"wcol_box\.inner_sel", Paint(Surface.FIELD)),
        (r"wcol_scroll\.inner_sel", Paint(Line.LOCKED)),
        (r"wcol_progress\.inner_sel", Paint(Accent.INDICATOR)),
        (r"wcol_tab\.inner_sel", Paint(Accent.TAB_ACTIVE)),
        (r"wcol_(menu_back|pie_menu|tooltip|menu_item|list_item)\.inner_sel", Paint(Accent.ROW_ACTIVE)),
        (r"wcol_\w+\.inner_sel", Paint(Accent.CONTROL_PRESSED)),
        (r"wcol_regular\.item", Paint(Surface.FRAME, 128 / 255)),
        (r"wcol_tool\.item", Paint(Text.PRIMARY)),
        (r"wcol_toolbar_item\.item", Paint(Text.PRIMARY, 179 / 255)),
        (r"wcol_text\.item", Paint(Accent.TEXT_SELECTED)),
        (r"wcol_option\.item", Paint(Text.PRIMARY)),
        (r"wcol_(num|numslider|progress)\.item", Paint(Accent.INDICATOR)),
        (r"wcol_(menu|pulldown|menu_back|tooltip|menu_item)\.item", Paint(Text.SECONDARY)),
        (r"wcol_pie_menu\.item", Paint(Surface.HOVER)),
        (r"wcol_scroll\.item", Paint(Line.GRID)),
        (r"wcol_list_item\.item", Paint(Text.PRIMARY, 51 / 255)),
        (r"wcol_curve\.item", Paint(Line.GRID, 89 / 255)),
        (r"wcol_\w+\.item", Paint(Surface.FRAME)),
        (r"wcol_tab\.outline", Paint(Surface.FRAME)),
        (r"wcol_tab\.outline_sel", Paint(Accent.TAB_ACTIVE)),
        (r"wcol_list_item\.outline", Paint((list_red, list_green, list_blue), 0.0)),
        (r"wcol_(pulldown|menu_item)\.outline", Paint(Line.BORDER, 0.0)),
        (r"wcol_toolbar_item\.outline", Paint(Surface.PANEL)),
        (r"wcol_(regular|tool|radio|toggle|menu)\.outline", Paint(Surface.FIELD)),
        (r"wcol_\w+\.outline", Paint(Line.BORDER)),
        (r"wcol_(pulldown|menu_item)\.outline_sel", Paint(Accent.FOCUS, 0.0)),
        (r"wcol_(box|menu_back|tooltip|progress|scroll|list_item)\.outline_sel", Paint(Line.BORDER)),
        (r"wcol_curve\.outline_sel", Paint(Text.SECONDARY)),
        (r"wcol_(regular|tool|toolbar_item|radio|toggle|menu)\.outline_sel", Paint(Accent.CONTROL_PRESSED)),
        (r"wcol_pie_menu\.outline_sel", Paint(Accent.ROW_ACTIVE)),
        (r"wcol_\w+\.outline_sel", Paint(Accent.FOCUS)),
        (r"wcol_curve\.text(_sel)?", Paint(Text.PRIMARY, 0.0)),
        (r"wcol_(menu_back|tab)\.text", Paint(Text.SECONDARY, None)),
        (r"wcol_\w+\.text(_sel)?", Paint(Text.PRIMARY, None)),
        (r"wcol_state\.error", Paint(Status.ERROR)),
        (r"wcol_state\.warning", Paint(Status.WARNING)),
        (r"wcol_state\.info", Paint(Status.INFO)),
        (r"wcol_state\.success", Paint(Status.SUCCESS)),
        (r"wcol_state\.inner_anim(_sel)?", Paint(Field.ANIMATED)),
        (r"wcol_state\.inner_key(_sel)?", Paint(Field.KEYED)),
        (r"wcol_state\.inner_driven(_sel)?", Paint(Field.DRIVEN)),
        (r"wcol_state\.inner_overridden(_sel)?", Paint(Field.OVERRIDDEN)),
        (r"wcol_state\.inner_changed(_sel)?", Paint(Field.CHANGED)),
    )
    roles = (
        *((rf"user_interface\.{pattern}", paint) for pattern, paint in widget_roles),
        (r"(node_editor|image_editor|clip_editor)\.space\.back", Paint(Surface.CANVAS)),
        (r"(topbar|statusbar)\.space\.(back|header)", Paint(Surface.FRAME)),
        (r"(outliner|file_browser|spreadsheet|console|info|text_editor)\.space\.back", Paint(Surface.WELL)),
        (r"(?!view_3d)\w+\.space\.back", Paint(Surface.PANEL)),
        (r"\w+\.space\.header", Paint(Surface.PANEL)),
        (r"(graph_editor|dopesheet_editor|nla_editor|sequence_editor|clip_editor|statusbar)\.space\.text", Paint(Text.SECONDARY, None)),
        (r"statusbar\.space\.header_text", Paint(Text.SECONDARY, None)),
        (r"\w+\.space\.(title|text|text_hi|header_text|header_text_hi)", Paint(Text.PRIMARY, None)),
        (r"user_interface\.widget_emboss", Paint(Line.GEOMETRY, 0.0)),
        (r"user_interface\.link", Paint(Accent.LINK)),
        (r"user_interface\.panel_active", Paint(Accent.FOCUS)),
        (r"user_interface\.icon_autokey", Paint(Accent.CHECKBOX_CHECKED)),
        (r"user_interface\.editor_(border|outline|outline_active)", Paint(Surface.FRAME)),
        (r"user_interface\.(panel_title|panel_text)", Paint(Text.PRIMARY, None)),
        (r"user_interface\.panel_outline", Paint(Text.PRIMARY, 17 / 255)),
        (r"user_interface\.widget_text_cursor", Paint(Accent.ITEM_SELECTED)),
        (r"user_interface\.panel_header", Paint(Surface.BOX)),
        (r"user_interface\.panel_back", Paint(Surface.PANEL)),
        (r"user_interface\.panel_sub_back", Paint(Line.GEOMETRY, 31 / 255)),
        (r"user_interface\.transparent_checker_primary", Paint(Surface.BOX)),
        (r"user_interface\.transparent_checker_secondary", Paint(Surface.PANEL)),
        (r"user_interface\.axis_x", Paint(Axis.X)),
        (r"user_interface\.axis_y", Paint(Axis.Y)),
        (r"user_interface\.axis_z", Paint(Axis.Z)),
        (r"user_interface\.axis_w", Paint(Text.SECONDARY)),
        (r"user_interface\.gizmo_hi", Paint(Selection.HOVER)),
        (r"user_interface\.gizmo_(primary|a)", Paint(Selection.GIZMO)),
        (r"user_interface\.gizmo_(secondary|b)", Paint(Selection.GIZMO_SECONDARY)),
        (r"user_interface\.gizmo_view_align", Paint(Text.PRIMARY)),
        (r"user_interface\.icon_\w+", Paint(Text.ICON)),
        (r"regions\.(asset_shelf|sidebars)\.back", Paint(Surface.PANEL)),
        (r"regions\.channels\.back", Paint(Surface.WELL)),
        (r"regions\.(asset_shelf\.header_back|scrubbing\.back|sidebars\.tab_back)", Paint(Surface.FRAME)),
        (r"regions\.channels\.text", Paint(Text.PRIMARY)),
        (r"regions\.(channels\.text_selected|scrubbing\.time_marker_selected)", Paint(Accent.ITEM_SELECTED)),
        (r"regions\.scrubbing\.(text|time_marker)", Paint(Text.SECONDARY)),
        (r"common\.anim\.playhead", Paint(Accent.INDICATOR)),
        (r"(common\.anim\.preview_range|outliner\.edited_object)", Paint(Guide.CONSTRUCTION, 102 / 255)),
        (r"(common\.anim\.scene_strip_range|view_3d\.gp_wire_edit)", Paint(Line.GEOMETRY, 128 / 255)),
        (r"common\.anim\.channels", Paint(Surface.BOX)),
        (r"common\.anim\.(channels_sub|channel_group)", Paint(Surface.PANEL)),
        (r"common\.anim\.channel_group_active", Paint(Accent.ROW_ACTIVE)),
        (r"common\.anim\.channel", Paint(Surface.WELL)),
        (r"common\.anim\.channel_selected", Paint(Accent.ROW_SELECTED)),
        (r"common\.anim\.keyframe", Paint(Text.SECONDARY)),
        (r"common\.anim\.keyframe(_extreme|_breakdown|_jitter)?_selected", Paint(Selection.ITEM)),
        (r"common\.anim\.keyframe_extreme", Paint(Line.KEY_EXTREME)),
        (r"common\.anim\.keyframe_breakdown", Paint(Line.KEY_BREAKDOWN)),
        (r"common\.anim\.keyframe_jitter", Paint(Line.KEY_JITTER)),
        (r"common\.anim\.keyframe_moving_hold", Paint(Line.LOCKED)),
        (r"common\.anim\.keyframe_(moving_hold|generated)_selected", Paint(Selection.INACTIVE)),
        (r"common\.anim\.keyframe_generated", Paint(Line.GRID)),
        (r"common\.anim\.long_key", Paint(Text.PRIMARY, 31 / 255)),
        (r"common\.anim\.long_key_selected", Paint(Selection.ITEM, 153 / 255)),
        (r"common\.curves\.handle_sel_\w+", Paint(Selection.ITEM)),
        (r"common\.curves\.handle_vertex", Paint(Line.GEOMETRY)),
        (r"common\.curves\.handle_vertex_select", Paint(Selection.ITEM)),
        (r"common\.curves\.handle_\w+", Paint(Guide.HANDLE)),
        (r"view_3d\.space\.gradients\.(high_gradient|gradient)", Paint(Surface.CANVAS)),
        (r"view_3d\.grid", Paint(shaded((minor, minor, minor), minor_lift))),
        (r"view_3d\.grid_major", Paint(shaded(Line.GRID, major_lift), GRID_MAJOR_ALPHA)),
        (r"view_3d\.clipping_border_3d", Paint(Surface.SECTION)),
        (r"view_3d\.(wire|wire_edit|vertex|camera|empty|speaker|camera_path|camera_passepartout|vertex_unreferenced|gp_vertex|light)", Paint(Line.GEOMETRY)),
        (r"view_3d\.view_overlay", Paint(Guide.HANDLE)),
        (r"view_3d\.(gp_vertex_select|object_selected|vertex_select|edge_select|edge_mode_select|nurb_sel_uline|nurb_sel_vline|bone_pose)", Paint(Selection.ITEM)),
        (r"view_3d\.text_grease_pencil", Paint(Text.SECONDARY)),
        (r"view_3d\.(object_active|bone_pose_active)", Paint(Selection.ACTIVE)),
        (r"view_3d\.(face_select|face_mode_select)", Paint(Selection.ITEM, FACE_FILL_ALPHA)),
        (r"view_3d\.editmesh_active", Paint(Selection.ACTIVE, FACE_FILL_ALPHA)),
        (r"view_3d\.face", Paint(Text.PRIMARY, 2 / 255)),
        (r"view_3d\.face_back", Paint(Status.ERROR, 179 / 255)),
        (r"view_3d\.face_front", Paint(Line.GEOMETRY, 0.0)),
        (r"view_3d\.seam", Paint(Line.SEAM)),
        (r"view_3d\.before_current_frame", Paint(Line.BEFORE_FRAME)),
        (r"view_3d\.sharp", Paint(Line.SHARP)),
        (r"view_3d\.crease", Paint(Line.CREASE)),
        (r"view_3d\.bevel", Paint(Line.BEVEL)),
        (r"view_3d\.after_current_frame", Paint(Line.AFTER_FRAME)),
        (r"view_3d\.(freestyle|nurb_uline|nurb_vline)", Paint(Guide.HANDLE)),
        (r"view_3d\.extra_\w+", Paint(Text.PRIMARY)),
        (r"view_3d\.(normal|split_normal|vertex_normal|skin_root)", Paint(Guide.CONSTRUCTION)),
        (r"view_3d\.transform", Paint(Guide.TRACKING)),
        (r"view_3d\.face_retopology", Paint(Surface.SHADED, FACE_FILL_ALPHA)),
        (r"view_3d\.(bone_solid|bundle_solid)", Paint(Surface.SHADED)),
        (r"view_3d\.bone_locked_weight", Paint(Line.LOCKED, 128 / 255)),
        (r"(graph_editor|nla_editor|dopesheet_editor|sequence_editor)\.grid", Paint(Line.GRID_PANEL)),
        (r"node_editor\.grid", Paint((minor, minor, minor))),
        (r"image_editor\.grid", Paint((image, image, image), image_alpha)),
        (r"clip_editor\.grid", Paint(Line.GRID, GRID_MINOR_ALPHA)),
        (r"graph_editor\.vertex", Paint(Text.SECONDARY)),
        (r"graph_editor\.vertex_select", Paint(Selection.ITEM)),
        (r"graph_editor\.vertex_active", Paint(Selection.ACTIVE)),
        (r"(file_browser|outliner|info)\.(selected_file|selected_highlight|info_selected)", Paint(Accent.ROW_SELECTED)),
        (r"(file_browser|sequence_editor|outliner|spreadsheet)\.row_alternate", Paint(Text.PRIMARY, 0.0)),
        (r"nla_editor\.active_action", Paint(Accent.INDICATOR, 102 / 255)),
        (r"nla_editor\.active_action_unset", Paint(Line.LOCKED, 77 / 255)),
        (r"nla_editor\.(strips|meta_strips|sound_strips)", Paint(Surface.FIELD)),
        (r"nla_editor\.transition_strips", Paint(Surface.BOX)),
        (r"nla_editor\.(strips|meta_strips|sound_strips|transition_strips)_selected", Paint(Selection.BODY)),
        (r"nla_editor\.tweak", Paint(Guide.TENTATIVE)),
        (r"nla_editor\.tweak_duplicate", Paint(Status.WARNING)),
        (r"(nla_editor|dopesheet_editor|sequence_editor)\.keyframe_border(_selected)?", Paint(Line.GEOMETRY)),
        (r"dopesheet_editor\.summary", Paint(Surface.BOX)),
        (r"dopesheet_editor\.anim_interpolation_linear", Paint(Text.SECONDARY, 204 / 255)),
        (r"dopesheet_editor\.anim_interpolation_constant", Paint(Line.CREASE, 204 / 255)),
        (r"dopesheet_editor\.anim_interpolation_other", Paint(Line.LOCKED, 179 / 255)),
        (r"dopesheet_editor\.simulated_frames", Paint(Accent.INDICATOR)),
        (r"image_editor\.(vertex|wire_edit)", Paint(Line.GEOMETRY)),
        (r"image_editor\.(vertex_select|edge_select)", Paint(Selection.ITEM)),
        (r"image_editor\.face_select", Paint(Selection.ITEM, FACE_FILL_ALPHA)),
        (r"image_editor\.face_mode_select", Paint(Selection.ITEM, 0.0)),
        (r"image_editor\.face", Paint(Text.PRIMARY, 10 / 255)),
        (r"image_editor\.editmesh_active", Paint(Selection.ACTIVE, FACE_FILL_ALPHA)),
        (r"image_editor\.scope_back", Paint(Surface.PANEL)),
        (r"image_editor\.preview_stitch_(face|edge|vert)", Paint(Guide.TRACKING, FACE_FILL_ALPHA)),
        (r"image_editor\.preview_stitch_stitchable", Paint(Guide.TRACKING)),
        (r"image_editor\.preview_stitch_unstitchable", Paint(Status.ERROR)),
        (r"image_editor\.preview_stitch_active", Paint(Guide.TRACKING, SELECTION_FILL_ALPHA)),
        (r"image_editor\.uv_shadow", Paint(Line.LOCKED)),
        (r"(image_editor|sequence_editor|clip_editor)\.metadatabg", Paint(Surface.FRAME)),
        (r"(image_editor|sequence_editor|clip_editor)\.metadatatext", Paint(Text.PRIMARY)),
        (r"sequence_editor\.active_strip", Paint(Selection.ACTIVE)),
        (r"sequence_editor\.(selected_strip|text_strip_cursor)", Paint(Selection.ITEM)),
        (r"sequence_editor\.\w+_strip", Paint(Surface.FIELD)),
        (r"sequence_editor\.selected_text", Paint(Accent.TEXT_SELECTED)),
        (r"sequence_editor\.preview_back", Paint(Line.GEOMETRY)),
        (r"(properties|preferences|outliner)\.match", Paint(Accent.INDICATOR)),
        (r"text_editor\.line_numbers", Paint(Text.SECONDARY)),
        (r"text_editor\.line_numbers_background", Paint(Surface.FRAME)),
        (r"(text_editor\.selected_text|console\.select)", Paint(Accent.TEXT_SELECTED)),
        (r"(text_editor|console)\.cursor", Paint(Accent.ITEM_SELECTED)),
        (r"text_editor\.syntax_(reserved|preprocessor)", Paint(Text.KEYWORD)),
        (r"text_editor\.syntax_string", Paint(Text.STRING)),
        (r"text_editor\.syntax_numbers", Paint(Text.NUMBER)),
        (r"text_editor\.syntax_builtin", Paint(Text.BUILTIN)),
        (r"text_editor\.syntax_(special|symbols)", Paint(Text.PRIMARY)),
        (r"text_editor\.syntax_comment", Paint(Text.SECONDARY)),
        (r"node_editor\.(node_outline|wire|wire_inner)", Paint(Line.GEOMETRY)),
        (r"node_editor\.(node_selected|wire_select)", Paint(Selection.ITEM)),
        (r"node_editor\.node_active", Paint(Selection.ACTIVE)),
        (r"node_editor\.node_backdrop", Paint(Surface.PANEL)),
        (r"node_editor\.group_socket_node", Paint(Surface.FRAME)),
        (r"node_editor\.(frame_node|\w+_zone)", Paint(Surface.PANEL, FRAME_NODE_ALPHA)),
        (r"node_editor\.\w+_node", Paint(Surface.BOX)),
        (r"outliner\.active", Paint(Accent.ROW_ACTIVE)),
        (r"outliner\.selected_object", Paint(Accent.ITEM_SELECTED)),
        (r"outliner\.active_object", Paint(Accent.ITEM_ACTIVE)),
        (r"info\.info_warning_text", Paint(Text.ON_SOLID)),
        (r"info\.info_\w+_text", Paint(Text.PRIMARY)),
        (r"info\.info_(debug|property|operator)", Paint(Status.INFO)),
        (r"console\.line_output", Paint(Text.PRIMARY)),
        (r"console\.(line_input|line_info)", Paint(Text.SECONDARY)),
        (r"console\.line_error", Paint(Text.ERROR)),
        (r"clip_editor\.marker_outline", Paint(Line.GEOMETRY)),
        (r"clip_editor\.marker", Paint(Text.SECONDARY)),
        (r"clip_editor\.active_marker", Paint(Selection.ACTIVE)),
        (r"clip_editor\.selected_marker", Paint(Selection.ITEM)),
        (r"clip_editor\.disabled_marker", Paint(Line.GRID)),
        (r"clip_editor\.locked_marker", Paint(Line.LOCKED)),
        (r"clip_editor\.path_(keyframe_)?before", Paint(Line.BEFORE_FRAME)),
        (r"clip_editor\.path_(keyframe_)?after", Paint(Line.AFTER_FRAME)),
        *((rf"collection_color\[{index}\]\.color", Paint(tag.solid)) for index, tag in enumerate(Tag) if tag is not Tag.COLOR_09),
        *((rf"strip_color\[{index}\]\.color", Paint(tag.solid)) for index, tag in enumerate(Tag)),
        *(
            (rf"bone_color_sets\[{index}\]\.{field}", Paint(rgb))
            for index, tag in enumerate((*(tag for tag in Tag if tag is not Tag.COLOR_09), *((Tag.COLOR_09,) * 7)))
            for field, rgb in (("normal", tag.solid), ("select", tag.selected), ("active", tag.active))
        ),
        (r"bone_color_sets\[\d+\]\.(normal|select|active)", Paint(Line.GEOMETRY)),
    )
    theme = preferences.themes[0]
    rules = [(re.compile(pattern), paint) for pattern, paint in roles]
    for struct, name, path in colors(theme, ""):
        if (paint := next((paint for pattern, paint in rules if pattern.fullmatch(path)), None)) is not None:
            paint.apply(struct, name)
    ui, v3d = theme.user_interface, theme.view_3d
    widgets = [widget for prop in ui.bl_rna.properties if isinstance(widget := getattr(ui, prop.identifier), bpy.types.ThemeWidgetColors)]
    for struct, name, value in (
        (ui, "panel_roundness", 0.2),
        (ui, "menu_shadow_width", 0),
        (ui, "icon_saturation", 0.0),
        (ui, "icon_alpha", 1.0),
        (ui, "icon_border_intensity", 0.0),
        (ui, "transparent_checker_size", 10),
        (ui.wcol_state, "blend", 1.0),
        *((widget, "roundness", 1.0 if widget == ui.wcol_scroll else 0.2) for widget in widgets),
        *((widget, "show_shaded", False) for widget in widgets),
        (v3d, "edge_width", 1),
        (v3d, "outline_width", 1),
        (v3d, "vertex_size", 4),
        (v3d, "facedot_size", 3),
        (v3d, "gp_vertex_size", 3),
        (v3d, "object_origin_size", 4),
        (v3d, "grid_axis_brightness", 0.5),
        (theme.image_editor, "vertex_size", 4),
        (theme.image_editor, "facedot_size", 3),
        (theme.image_editor, "edge_width", 1),
        (theme.graph_editor, "vertex_size", 4),
        (theme.common.curves, "handle_vertex_size", 3),
        (theme.node_editor, "noodle_curving", 4),
        *(
            (struct, name, struct.bl_rna.properties[name].default)
            for struct, name in (
                (theme.dopesheet_editor, "keyframe_scale_factor"),
                (theme.node_editor, "grid_levels"),
                (theme.node_editor, "dash_alpha"),
                *((bone_set, "show_colored_constraints") for bone_set in theme.bone_color_sets),
            )
        ),
    ):
        if round(getattr(struct, name), 4) != value:
            setattr(struct, name, value)


# --- [WORKSPACES]
def activate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Switch the window to the workspace and yield until the window manager applied it and refreshed its screen."""
    window.workspace = bpy.data.workspaces[name]
    while window.workspace.name != name:
        yield TICK
    yield TICK


def duplicate(window: bpy.types.Window, name: str) -> Iterator[float]:
    """Duplicate Model, yield until the window shows the copy, and name it."""
    yield from activate(window, Frame.Model.name)
    before = set(bpy.data.workspaces)
    with override(window):
        bpy.ops.workspace.duplicate()
    copy = next(workspace for workspace in bpy.data.workspaces if workspace not in before)
    while window.workspace != copy:
        yield TICK
    copy.name = name


def shape_workspaces(window: bpy.types.Window) -> Iterator[float]:
    """Object Mode on every workspace, the stock workspaces renamed, each missing frame a copy of Model, and every workspace outside the frames removed."""
    for workspace in bpy.data.workspaces:
        workspace.object_mode = "OBJECT"
    for old, new in (("Layout", "Model"), ("Geometry Nodes", "Nodes"), ("Compositing", "Render"), ("Scripting", "Script")):
        if old in bpy.data.workspaces:
            bpy.data.workspaces[old].name = new
    for name in [frame.name for frame in Frame if frame.name not in bpy.data.workspaces]:
        yield from duplicate(window, name)
    for name in [workspace.name for workspace in bpy.data.workspaces if workspace.name not in Frame.__members__]:
        yield from activate(window, name)
        with override(window):
            bpy.ops.workspace.delete()
        while name in bpy.data.workspaces:
            yield TICK


# --- [LAYOUT]
def scaled(size: int) -> int:
    """Device pixels of a size in logical pixels at the interface scale."""
    return round(size * bpy.context.preferences.system.ui_scale)


def points(size: int) -> int:
    """Device pixels of a size in macOS points."""
    preferences = bpy.context.preferences
    return size * round(preferences.system.ui_scale / preferences.view.ui_scale)


def columns(screen: bpy.types.Screen) -> tuple[tuple[str, ...], ...]:
    """The editor types of the screen by column from the left, each column from the top."""
    return tuple(tuple(area.ui_type for area in sorted((area for area in screen.areas if area.x == x), key=lambda area: -area.y)) for x in sorted({area.x for area in screen.areas}))


def framed(frame: Frame) -> tuple[tuple[str, ...], ...]:
    """The columns the frame declares: its source column, its main editors, and the Properties editor above the Outliner."""
    return tuple(column for column in (frame.left, frame.center, RIGHT) if column)


def fixed(kind: str) -> int | None:
    """Device pixels of a fixed row's height, the Outliner from its header and shared row count, None for a row that takes what the fixed rows leave."""
    system = bpy.context.preferences.system
    widget = floor(18 * system.ui_scale + 0.5) + 2 * int(system.pixel_size)
    match kind:
        case "TIMELINE":
            return points(BOTTOM_STRIP)
        case "OUTLINER":
            return widget + int(6 * system.ui_scale) + 2 + ORGANIZER_ROWS * widget
        case "GeometryNodeTree" | "ShaderNodeTree" | "CompositorNodeTree" | "TEXT_EDITOR":
            return points(NODE_EDITOR)
        case _:
            return None


def split(window: bpy.types.Window, area: bpy.types.Area, direction: Literal["HORIZONTAL", "VERTICAL"], factor: float) -> frozenset[int]:
    """Split the area at the factor's share from its left or bottom edge and return the areas held before."""
    before = frozenset(area.as_pointer() for area in window.screen.areas)
    with override(window, area):
        bpy.ops.screen.area_split(direction=direction, factor=factor)
    return before


def parts(window: bpy.types.Window, area: bpy.types.Area, before: frozenset[int], direction: Literal["HORIZONTAL", "VERTICAL"]) -> tuple[bpy.types.Area, bpy.types.Area]:
    """The two parts of a split area once the screen holds them, left then right or top then bottom."""
    first, second = sorted((area, next(area for area in window.screen.areas if area.as_pointer() not in before)), key=lambda part: part.x if direction == "VERTICAL" else -part.y)
    return first, second


def stack(window: bpy.types.Window, column: bpy.types.Area, rows: tuple[str, ...]) -> Iterator[float]:
    """Split the column into its rows from the top, the fixed rows at their heights and the others sharing what remains."""
    heights = [fixed(kind) for kind in rows]
    share = (column.height - sum(height for height in heights if height is not None)) // heights.count(None)
    sizes = [share if height is None else height for height in heights]
    area = column
    for index, kind in enumerate(rows[:-1]):
        before = split(window, area, "HORIZONTAL", sum(sizes[index + 1 :]) / area.height)
        yield TICK
        top, area = parts(window, area, before, "HORIZONTAL")
        top.ui_type = kind
    area.ui_type = rows[-1]


def frame_screen(window: bpy.types.Window, frame: Frame) -> Iterator[float]:
    """Rebuild the workspace's screen from the largest area when its columns differ from its frame."""
    if columns(window.screen) == framed(frame):
        return
    keep = max(window.screen.areas, key=lambda area: area.width * area.height)
    for area in [area for area in window.screen.areas if area.as_pointer() != keep.as_pointer()]:
        with override(window, area):
            bpy.ops.screen.area_close()
        yield TICK
    before = split(window, keep, "VERTICAL", 1 - points(RIGHT_COLUMN) / keep.width)
    yield TICK
    main, right = parts(window, keep, before, "VERTICAL")
    yield from stack(window, right, RIGHT)
    if frame.left:
        before = split(window, main, "VERTICAL", scaled(LEFT_COLUMN) / main.width)
        yield TICK
        left, main = parts(window, main, before, "VERTICAL")
        yield from stack(window, left, frame.left)
    yield from stack(window, main, frame.center)


def shape_spaces(workspace: bpy.types.WorkSpace, frame: Frame, modules: dict[str, str]) -> None:
    """The workspace's own settings, the enabled add-ons it shows, its Object and Edit Mesh tool, each Properties area's Bonsai tab, and every area's space settings on screen."""
    placed, screen = {name for each in Frame for name in each.addons}, workspace.screens[0]
    if {owner.name for owner in workspace.owner_ids} != (owners := {module for name, module in modules.items() if name not in placed or name in frame.addons}):
        workspace.owner_ids.clear()
        for module in sorted(owners):
            workspace.owner_ids.new(module)
    workspace.use_pin_scene, workspace.use_filter_by_owner = False, True
    if Addon.BONSAI in modules:
        tabs, left = screen.BIMAreaProperties, min(area.x for area in screen.areas)
        for index, area in enumerate(screen.areas):
            if area.type == "PROPERTIES" and tabs[index].tab != (tab := "PROJECT" if area.x == left else "BLENDER"):
                tabs[index].tab = tab
    screen.show_statusbar = True
    for mode in ("OBJECT", "EDIT_MESH"):
        workspace.tools.from_space_view3d_mode(mode, create=True).idname = "builtin.select_box"
    for area in screen.areas:
        area.show_menus = True
        match area.spaces.active:
            case bpy.types.SpaceView3D() as space:
                shading, overlay = space.shading, space.overlay
                space.lens, space.clip_start, space.clip_end = 100.0, INCH, 20_000 * FOOT
                space.show_region_tool_header, space.show_region_toolbar, space.show_region_ui = False, True, False
                space.show_region_header = True
                space.use_local_collections = frame.axis is not None
                space.show_gizmo_empty_image = space.show_gizmo_empty_force_field = space.show_gizmo_light_size = True
                space.show_gizmo_light_look_at = space.show_gizmo_camera_lens = space.show_gizmo_camera_dof_distance = True
                space.show_gizmo_object_translate = space.show_gizmo_object_rotate = space.show_gizmo_object_scale = True
                space.show_reconstruction = False
                shading.type, shading.light, shading.studio_light = "SOLID", "MATCAP", "check_reflection_horizontal.exr"
                shading.light, shading.studio_light = "STUDIO", next(studio for studio in bpy.context.preferences.studio_lights if studio.type == "STUDIO").name
                shading.type, shading.background_type = frame.shading, "THEME"
                shading.color_type, shading.wireframe_color_type, shading.show_cavity = frame.color, "THEME", False
                Paint(Surface.SHADED).apply(shading, "single_color")
                Paint(Line.GEOMETRY).apply(shading, "object_outline_color")
                Paint(Surface.CANVAS).apply(shading, "background_color")
                Paint(Line.GRID).apply(overlay, "gpencil_grid_color")
                shading.show_object_outline = shading.use_scene_world_render = shading.use_scene_lights_render = True
                shading.use_world_space_lighting = shading.show_specular_highlight = shading.show_shadows = shading.show_xray = shading.show_backface_culling = False
                shading.studiolight_background_alpha, shading.studiolight_background_blur, shading.xray_alpha_wireframe = 0.0, 0.5, 0.0
                overlay.show_text = overlay.show_floor = overlay.show_ortho_grid = overlay.show_outline_selected = overlay.show_wireframes = True
                overlay.show_axis_x = overlay.show_axis_y = True
                overlay.show_extras = overlay.show_cursor = overlay.show_object_origins = overlay.show_bones = overlay.show_motion_paths = overlay.show_annotation = True
                overlay.show_edge_seams = overlay.show_edge_sharp = overlay.show_edge_crease = overlay.show_edge_bevel_weight = True
                overlay.show_freestyle_edge_marks = overlay.show_freestyle_face_marks = overlay.show_viewer_attribute = True
                overlay.show_camera_guides = overlay.show_camera_passepartout = True
                overlay.show_axis_z = overlay.show_look_dev = overlay.show_relationship_lines = overlay.show_face_orientation = overlay.show_stats = False
                overlay.show_object_origins_all = overlay.show_fade_inactive = overlay.show_face_center = overlay.show_statvis = overlay.show_light_colors = False
                overlay.show_extra_edge_length = overlay.show_extra_edge_angle = overlay.show_extra_face_angle = overlay.show_extra_face_area = overlay.show_extra_indices = False
                overlay.wireframe_threshold, overlay.wireframe_opacity, overlay.normals_length, overlay.xray_alpha_bone = 0.0, 1.0, 0.1, 0.0
                overlay.gpencil_grid_opacity, overlay.gpencil_fade_layer = GRID_MINOR_ALPHA, overlay.bl_rna.properties["gpencil_fade_layer"].default
            case bpy.types.SpaceImageEditor() as space:
                space.image = next(image for image in bpy.data.images if image.type == "RENDER_RESULT")
                space.show_region_tool_header = space.show_region_toolbar = space.show_region_ui = space.use_image_pin = False
                space.show_region_header = space.show_gizmo = space.show_annotation = space.overlay.show_overlays = True
                space.ui_mode, space.display_channels = "VIEW", "COLOR_ALPHA"
            case bpy.types.SpaceNodeEditor() as space:
                space.show_region_toolbar = space.show_region_ui = space.pin = space.overlay.show_timing = space.overlay.show_wire_color = False
                space.show_region_header = space.show_annotation = space.overlay.show_overlays = True
                space.overlay.show_reroute_auto_labels = space.overlay.show_context_path = space.overlay.show_named_attributes = space.overlay.show_previews = True
                match space.tree_type:
                    case "GeometryNodeTree":
                        space.node_tree_sub_type = "MODIFIER"
                    case "CompositorNodeTree":
                        space.node_tree_sub_type, space.show_region_asset_shelf, space.show_backdrop = "SCENE", False, False
                    case "ShaderNodeTree":
                        space.shader_type = "OBJECT"
                    case _:
                        pass
            case bpy.types.SpaceDopeSheetEditor() as space:
                space.show_region_channels = space.show_seconds = space.show_locked_time = space.dopesheet.show_only_errors = False
                space.show_region_header = space.show_markers = space.show_cache = space.cache_simulation_nodes = True
                space.cache_softbody = space.cache_particles = space.cache_cloth = space.cache_smoke = space.cache_dynamicpaint = space.cache_rigidbody = True
            case bpy.types.SpaceSpreadsheet() as space:
                space.show_region_header, space.show_region_toolbar, space.show_region_footer, space.show_region_ui = True, True, False, False
                space.show_internal_attributes, space.use_filter, space.show_only_selected = False, True, False
            case bpy.types.SpaceFileBrowser() as space:
                space.show_region_header, space.show_region_toolbar, space.show_region_ui = False, False, True
                params = space.params
                params.display_type = "LIST_VERTICAL"
                params.show_details_datetime = params.show_details_size = params.show_hidden = params.use_sort_invert = False
                params.sort_method, params.recursion_level, params.use_filter = "FILE_SORT_ALPHA", "NONE", True
                params.use_filter_folder = params.use_filter_image = True
            case bpy.types.SpaceConsole() as space:
                space.font_size = 14
            case bpy.types.SpaceTextEditor() as space:
                space.show_region_footer = space.show_region_ui = space.show_word_wrap = space.show_line_highlight = space.show_margin = False
                space.use_live_edit = space.use_match_case = space.use_find_all = False
                space.show_region_header = space.show_line_numbers = space.show_syntax_highlight = space.use_find_wrap = True
                space.font_size, space.tab_width, space.margin_column = 12, 4, 80
            case bpy.types.SpaceProperties() as space:
                space.context, space.show_region_header, space.use_pin_id, space.outliner_sync = frame.context, True, False, "AUTO"
                space.show_properties_particles = space.show_properties_physics = space.show_properties_effects = False
                space.show_properties_strip = space.show_properties_strip_modifier = False
            case bpy.types.SpaceOutliner() as space:
                space.display_mode, space.filter_text, space.filter_state = "VIEW_LAYER", "", "ALL"
                space.use_filter_complete = space.use_filter_case_sensitive = space.filter_invert = space.use_filter_view_layers = False
                space.show_restrict_column_holdout = space.show_restrict_column_indirect_only = space.use_filter_object_content = False
                space.use_sort_alpha = space.use_sync_select = space.show_mode_column = space.use_filter_children = space.scroll_to_active = True
                space.show_restrict_column_enable = space.show_restrict_column_select = space.show_restrict_column_hide = True
                space.show_restrict_column_viewport = space.show_restrict_column_render = True
                space.use_filter_collection = space.use_filter_object = True
                space.use_filter_object_mesh = space.use_filter_object_armature = space.use_filter_object_empty = True
                space.use_filter_object_light = space.use_filter_object_camera = space.use_filter_object_grease_pencil = space.use_filter_object_others = True
            case _:
                pass


def shape_regions(window: bpy.types.Window) -> Iterator[float]:
    """Show each Properties editor's tab column and clear the console history on screen."""
    for area in window.screen.areas:
        match area.type, {region.type: region for region in area.regions}:
            case "PROPERTIES", {"NAVIGATION_BAR": bar} if bar.width <= 1:
                with override(window, area):
                    bpy.ops.screen.region_toggle(region_type="NAVIGATION_BAR")
                yield TICK
            case "CONSOLE", {"WINDOW": region}:
                with override(window, area, region):
                    bpy.ops.console.clear(scrollback=False, history=True)
            case _:
                pass


def shape_view(window: bpy.types.Window, frame: Frame, sidebar: str | None) -> Iterator[float]:
    """Turn every 3D view to the perspective isometric home view, or a plan frame's view to its axis at plan distance with its hidden sidebar's tab on the category."""
    for area in [area for area in window.screen.areas if area.type == "VIEW_3D"]:
        space = area.spaces.active
        view = space.region_3d
        view.lock_rotation = False
        if frame.axis is None:
            view.view_perspective = "PERSP"
            view.view_rotation, view.view_location, view.view_distance = Vector((1.0, -1.0, 1.0)).to_track_quat("Z", "Y"), (0.0, 0.0, 0.0), 60 * FOOT
            continue
        with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
            bpy.ops.view3d.view_axis(type=frame.axis)
        view.view_distance = PLAN_DISTANCE
        if sidebar is not None:
            space.show_region_ui = True
            region = next(region for region in area.regions if region.type == "UI")
            while region.active_panel_category == "UNSUPPORTED":
                yield TICK
            region.active_panel_category = sidebar
            space.show_region_ui = False
            yield TICK


def edges(workspace: bpy.types.WorkSpace) -> Iterator[tuple[bpy.types.Area, Side, int]]:
    """Every area edge the frame sizes with its target in device pixels, fixed rows from the bottom up."""
    areas, frame = workspace.screens[0].areas, Frame[workspace.name]
    right, left = max(area.x + area.width for area in areas), min(area.x for area in areas)
    yield next(area for area in areas if area.type == "PROPERTIES" and area.x + area.width == right), Side.LEFT, points(RIGHT_COLUMN)
    if frame.left:
        yield next(area for area in areas if area.x == left), Side.RIGHT, scaled(LEFT_COLUMN)
    yield from ((area, Side.TOP, height) for area in sorted(areas, key=lambda area: area.y) if (height := fixed(area.ui_type)) is not None)


def resize(window: bpy.types.Window, area: bpy.types.Area, side: Side, target: int, pixel: int) -> Iterator[float]:
    """Move one edge of the area so its size along that axis is `target`, a Dope Sheet area resizing as an Outliner."""
    if (size := side.extent(area)) == target:
        return
    x, y = side.grip(area, pixel)
    kind = area.ui_type
    if area.type == "DOPESHEET_EDITOR":
        area.ui_type = "OUTLINER"
        yield TICK
    window.event_simulate(type="MOUSEMOVE", value="NOTHING", x=x, y=y)
    yield TICK
    yield TICK
    with override(window):
        bpy.ops.screen.area_move(x=x, y=y, delta=size - target if side is Side.LEFT else target - size)
    yield TICK
    area.ui_type = kind
    yield TICK


def stored(width: int, scale: float) -> int | None:
    """The logical width a drag stored for a region drawn `width` device pixels wide, None for a region on its editor's default."""
    held = ceil(width / scale - 0.5)
    return held if int(scale * (held + 0.5)) == width else None


def stroke(window: bpy.types.Window, region: bpy.types.Region, travel: int) -> Iterator[float]:
    """Drag the region's inner edge `travel` device pixels outward from three logical pixels inside it, past the edge zone of a region beside it."""
    outward, inset = 1 if region.alignment == "LEFT" else -1, int(3 * bpy.context.preferences.system.ui_scale)
    x, y = region.x + region.width - 1 - inset if outward == 1 else region.x + inset, region.y + region.height // 2
    for kind, value, at in (
        ("MOUSEMOVE", "NOTHING", x),
        ("MOUSEMOVE", "NOTHING", x),
        ("LEFTMOUSE", "PRESS", x),
        ("MOUSEMOVE", "NOTHING", x + outward * travel),
        ("LEFTMOUSE", "RELEASE", x + outward * travel),
    ):
        window.event_simulate(type=kind, value=value, x=at, y=y)
        yield TICK
        yield TICK


def drag(window: bpy.types.Window, area: bpy.types.Area, region: bpy.types.Region, target: int) -> Iterator[float]:
    """Reset the region to zoom 1, where Blender's size snap reads exact units, then drag its edge until its stored width is `target` logical pixels."""
    scale = bpy.context.preferences.system.ui_scale
    with override(window, area, region):
        bpy.ops.view2d.reset()
    yield TICK
    if stored(region.width, scale) is None:
        yield from stroke(window, region, ceil(target * scale) - region.width)
    if (held := stored(region.width, scale)) is not None and held != target:
        yield from stroke(window, region, (1 if target > held else -1) * ceil(abs(target - held) * scale))


def toggle(window: bpy.types.Window, area: bpy.types.Area, kind: str) -> Iterator[float]:
    """Show a hidden region of the area or hide a shown one, and yield until the screen drew the change."""
    with override(window, area):
        bpy.ops.screen.region_toggle(region_type=kind)
    yield TICK
    yield TICK


def size_regions(window: bpy.types.Window) -> Iterator[float | str]:
    """Size every region of the role table on screen to its role's width and yield an error row for each that ends off it."""
    scale, toolbar, sidebar = bpy.context.preferences.system.ui_scale, 56, 215
    roles = (
        ("VIEW_3D", "TOOLS", toolbar),
        ("VIEW_3D", "UI", sidebar),
        ("NODE_EDITOR", "TOOLS", toolbar),
        ("NODE_EDITOR", "UI", sidebar),
        ("IMAGE_EDITOR", "TOOLS", toolbar),
        ("IMAGE_EDITOR", "UI", sidebar),
        ("TEXT_EDITOR", "UI", sidebar),
        ("SPREADSHEET", "TOOLS", 160),
        ("SPREADSHEET", "UI", sidebar),
        ("FILE_BROWSER", "TOOLS", 200),
        ("DOPESHEET_EDITOR", "CHANNELS", 200),
    )
    for area in window.screen.areas:
        for kind, target in [(kind, size) for editor, kind, size in roles if editor == area.type]:
            region = next(region for region in area.regions if region.type == kind)
            if hidden := min(region.width, region.height) <= 1:
                yield from toggle(window, area, kind)
            if min(region.width, region.height) > 1:
                yield from drag(window, area, region, target)
            if region.width != (wanted := int(scale * (target + 0.5))):
                yield f"error\t{window.workspace.name} {area.ui_type} {kind} is {region.width} and needs {wanted}"
            if hidden:
                yield from toggle(window, area, kind)


def shape_layout(window: bpy.types.Window, pixel: int, modules: dict[str, str]) -> Iterator[float | str]:
    """Shape every workspace in frame order while it is on screen, from the enabled add-ons by the name their rows key on."""
    sidebar = "Ladybug" if Addon.LADYBUG_TOOLS in modules else None
    for frame in Frame:
        yield from activate(window, frame.name)
        with override(window):
            bpy.ops.workspace.reorder_to_back()
        yield from frame_screen(window, frame)
        shape_spaces(window.workspace, frame, modules)
        yield from shape_regions(window)
        for area, side, target in edges(window.workspace):
            yield from resize(window, area, side, target, pixel)
        yield from size_regions(window)
        yield from shape_view(window, frame, sidebar)


def unsized() -> Iterator[str]:
    """Every workspace whose columns differ from its frame and every sized edge whose area is off its target, read off every screen."""
    for workspace in bpy.data.workspaces:
        if (held := columns(workspace.screens[0])) != (wanted := framed(Frame[workspace.name])):
            yield f"{workspace.name} columns are {held} and need {wanted}"
    yield from (
        f"{workspace.name} {area.ui_type} {side} is {size} and needs {target}" for workspace in bpy.data.workspaces for area, side, target in edges(workspace) if (size := side.extent(area)) != target
    )


# --- [SCENE]
def light(scene: bpy.types.Scene) -> bpy.types.Object:
    """The scene's one light object, the sun."""
    return next(target for target in scene.objects if target.type == "LIGHT")


def sky(scene: bpy.types.Scene) -> bpy.types.ShaderNodeTexSky:
    """The world's sky texture feeding its background, created on first use."""
    tree = scene.world.node_tree
    background = next(node for node in tree.nodes if node.type == "BACKGROUND")
    texture: bpy.types.ShaderNodeTexSky = next((node for node in tree.nodes if node.type == "TEX_SKY"), None) or tree.nodes.new("ShaderNodeTexSky")
    if not (color := background.inputs["Color"]).is_linked:
        tree.links.new(texture.outputs["Color"], color)
    return texture


def apply_scene(scene: bpy.types.Scene, preferences: bpy.types.Preferences) -> None:
    """Units, color management, render frame and pixel density, render engines with Cycles sampling, denoising, light paths, and clamps, Workbench surfaces, snapping and transform tools, and the sky of the scene."""
    units, render, tools = scene.unit_settings, scene.render, scene.tool_settings
    display, cycles, eevee = scene.display, scene.cycles, scene.eevee
    units.system, units.scale_length, units.use_separate = Units.IMPERIAL.name, 1.0, True
    units.length_unit, units.temperature_unit, units.time_unit, units.system_rotation = Units.IMPERIAL.length, Units.IMPERIAL.temperature, "SECONDS", "DEGREES"
    render.fps, render.fps_base, render.use_persistent_data, render.filepath = 24, 1.0, True, preferences.filepaths.render_output_directory
    render.engine, render.use_lock_interface, render.anisotropic_filter = "CYCLES", True, "FILTER_16"
    render.resolution_x, render.resolution_y, render.resolution_percentage = *FRAME_SIZE, 100
    render.ppm_factor, render.ppm_base = DPI, INCH
    scene.display_settings.display_device = "sRGB"
    view = scene.view_settings
    view.view_transform, view.look, view.exposure = "AgX", "None", -5.3
    display.render_aa = display.viewport_aa = "16"
    workbench = display.shading
    workbench.color_type, workbench.show_specular_highlight, workbench.use_world_space_lighting = "SINGLE", False, False
    Paint(Surface.SHADED).apply(workbench, "single_color")
    Paint(Line.GEOMETRY).apply(workbench, "object_outline_color")
    workbench.show_cavity, workbench.show_object_outline = False, True
    cycles.device, cycles.samples, cycles.preview_samples = "GPU", SAMPLES, 256
    cycles.use_adaptive_sampling, cycles.adaptive_threshold = True, NOISE_THRESHOLD
    cycles.use_denoising, cycles.denoiser, cycles.denoising_use_gpu, cycles.use_preview_denoising = True, "OPENIMAGEDENOISE", True, True
    cycles.max_bounces, cycles.diffuse_bounces, cycles.glossy_bounces = MAX_BOUNCES, DIFFUSE_BOUNCES, GLOSSY_BOUNCES
    cycles.transmission_bounces, cycles.volume_bounces, cycles.transparent_max_bounces = TRANSMISSION_BOUNCES, VOLUME_BOUNCES, TRANSPARENT_BOUNCES
    cycles.sample_clamp_direct, cycles.sample_clamp_indirect, cycles.blur_glossy = 0.0, INDIRECT_CLAMP / 2**view.exposure, FILTER_GLOSSY
    cycles.caustics_reflective = cycles.caustics_refractive = CAUSTICS
    eevee.use_raytracing, eevee.taa_render_samples = True, 128
    tools.use_snap, tools.snap_target, tools.snap_angle_increment_3d = True, "CLOSEST", radians(15)
    tools.snap_elements = {"VERTEX", "EDGE_MIDPOINT", "EDGE_PERPENDICULAR", "FACE_MIDPOINT"}
    tools.use_snap_translate = tools.use_snap_rotate = tools.use_snap_backface_culling = tools.use_snap_self = tools.use_snap_edit = tools.use_snap_nonedit = True
    tools.use_snap_scale = tools.use_snap_node = tools.use_snap_selectable = tools.use_snap_align_rotation = tools.use_snap_peel_object = False
    tools.use_proportional_edit = tools.use_mesh_automerge = False
    tools.transform_pivot_point, scene.transform_orientation_slots[0].type = "MEDIAN_POINT", "GLOBAL"
    texture = sky(scene)
    texture.sky_type, texture.sun_disc, texture.altitude = "MULTIPLE_SCATTERING", False, ELEVATION
    next(node for node in scene.world.node_tree.nodes if node.type == "BACKGROUND").inputs["Strength"].default_value = 1.0


def apply_data(scene: bpy.types.Scene) -> None:
    """The startup data: the light a sun lamp at the sky's 137 W/m² irradiance, the camera at eye height a plan distance south facing north, and no stock cube, material, or Sverchok log."""
    lamp, camera = light(scene), scene.camera
    lamp.data.type = "SUN"
    sun = lamp.data
    sun.energy, sun.angle = 137.0, sun.bl_rna.properties["angle"].default
    north = Matrix.Rotation(radians(NORTH), 3, "Z") @ Vector((0.0, 1.0, 0.0))
    camera.location, camera.rotation_euler = Vector((0.0, 0.0, EYE_HEIGHT)) - PLAN_DISTANCE * north, north.to_track_quat("-Z", "Y").to_euler()
    camera.data.clip_end = 3000 * FOOT
    stock = (bpy.data.objects.get("Cube"), bpy.data.meshes.get("Cube"), bpy.data.materials.get("Material"), bpy.data.texts.get("sverchok.log"))
    bpy.data.batch_remove([block for block in stock if block is not None])


# --- [EXTENSION]
def extension(preferences: bpy.types.Preferences, package: Path) -> tuple[str, Path, dict[str, bytes]]:
    """The user repository's module, the folder it holds the package's extension in under the manifest id, and the package's files by relative path."""
    with zipfile.ZipFile(package) as archive:
        files = {info.filename: archive.read(info) for info in archive.infolist() if not info.is_dir()}
    repository = next(repo for repo in preferences.extensions.repos if repo.module == "user_default")
    return repository.module, Path(repository.directory) / tomllib.loads(files["blender_manifest.toml"].decode())["id"], files


def installed_files(folder: Path) -> dict[str, bytes]:
    """The installed extension's files by relative path, bytecode caches left out."""
    return {relative.as_posix(): path.read_bytes() for path in folder.rglob("*") if path.is_file() and "__pycache__" not in (relative := path.relative_to(folder)).parts}


def install_extension(preferences: bpy.types.Preferences, package: Path) -> None:
    """Install the package into the user repository when its files differ from the installed ones, and enable the extension."""
    repository, folder, files = extension(preferences, package)
    if installed_files(folder) != files:
        bpy.ops.extensions.package_install_files(filepath=str(package), repo=repository, enable_on_install=True)
    if (module := f"bl_ext.{repository}.{folder.name}") not in preferences.addons:
        bpy.ops.preferences.addon_enable(module=module)


# --- [KEYMAP]
def apply_keymap(user: bpy.types.KeyConfig) -> None:
    """Context menus of the 3D Viewport modes and the node editor on a still right click, the extension's Z pie on a backtick drag, and the spreadsheet's T on its Data Set region."""
    spreadsheet = user.keymaps["Spreadsheet Generic"]
    for owner, item in ((keymap, item) for keymap in user.keymaps for item in keymap.keymap_items):
        properties, unmodified = item.properties, not (item.any or item.shift or item.ctrl or item.alt or item.oskey)
        match item.idname, item.type, item.value:
            case "wm.call_menu" | "wm.call_panel", "RIGHTMOUSE", "PRESS" if unmodified and properties.name.startswith(("VIEW3D_", "NODE_")):
                item.value = "CLICK"
            case "wm.call_menu_pie", "ACCENT_GRAVE", "CLICK_DRAG" if unmodified and properties.name == "VIEW3D_MT_view_pie":
                properties.name, item.active = "INTERFACE_MT_z", True
            case "wm.context_toggle", "T", "PRESS" if owner == spreadsheet and properties.data_path == "space_data.show_region_channels":
                properties.data_path, item.active = "space_data.show_region_toolbar", True


# --- [FILES]
def save(window: bpy.types.Window) -> None:
    """Save the preferences and startup files from Model's viewport."""
    area = max((area for area in window.screen.areas if area.type == "VIEW_3D"), key=lambda area: area.width * area.height)
    bpy.context.preferences.use_preferences_save = True
    with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
        bpy.ops.wm.save_userpref()
        bpy.ops.wm.save_homefile()


def close() -> None:
    """Save a titled main file holding unsaved edits and discard an untitled one."""
    if bpy.data.is_dirty and bpy.data.filepath:
        bpy.ops.wm.save_mainfile()
    elif bpy.data.is_dirty:
        bpy.ops.wm.read_homefile()


# --- [COMPOSITION] ----------------------------------------------------------------------


def interface(window: bpy.types.Window, preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, scene: bpy.types.Scene, package: Path, port: int) -> Iterator[float | str]:
    """Every step in order with an active subject object until the save, yielding ticks and report rows."""
    preferences.use_preferences_save = False
    apply_preferences(preferences, keyconfigs)
    if not stocked(MATERIALS):
        yield f"skip\t{MATERIALS}"
    apply_addons(preferences)
    install_extension(preferences, package)
    yield TICK
    apply_theme(preferences)
    yield from shape_workspaces(window)
    mesh = bpy.data.meshes.new("Subject")
    subject = bpy.data.objects.new("Subject", mesh)
    scene.collection.objects.link(subject)
    bpy.context.view_layer.objects.active = subject
    try:
        yield from shape_layout(window, int(preferences.system.pixel_size), enabled(preferences))
        apply_scene(scene, preferences)
        apply_data(scene)
        yield from apply_addon_settings(preferences, scene, port)
        apply_keymap(keyconfigs.user)
        yield from activate(window, Frame.Model.name)
    finally:
        bpy.data.batch_remove((subject, mesh))
    save(window)


def applied(
    window: bpy.types.Window, preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, scene: bpy.types.Scene, package: Path, port: int
) -> Iterator[float | tuple[str, ...]]:
    """Every step between two observations, yielding ticks and then the report rows."""
    before, rows = observe(preferences, keyconfigs, scene, package), list[str]()
    for step in interface(window, preferences, keyconfigs, scene, package, port):
        match step:
            case str():
                rows.append(step)
            case _:
                yield step
    yield TICK
    yield TICK
    after = observe(preferences, keyconfigs, scene, package)
    yield (*rows, *(f"error\t{edge}" for edge in unsized()), *(f"change\t{path}\t{old!r}\t{value!r}" for path, value in after.items() if (old := before.get(path)) != value))


def steps(window: bpy.types.Window, report: Path, package: Path, port: int) -> Iterator[float]:
    """The applied steps, the report or a raised step's traceback written, the document step, and the quit."""
    rows: tuple[str, ...] = ()
    try:
        for step in applied(window, bpy.context.preferences, bpy.context.window_manager.keyconfigs, bpy.context.scene, package, port):
            match step:
                case tuple():
                    rows = step
                case _:
                    yield step
    except Exception:
        rows = tuple(f"error\t{line}" for line in traceback.format_exc().splitlines())
    part = report.with_suffix(".part")
    part.write_text("\n".join((f"app\t{bpy.app.version_string}\t{bpy.utils.user_resource('CONFIG')}", *rows)) + "\n", encoding="utf-8")
    part.replace(report)
    close()
    yield TICK
    with override(bpy.context.window_manager.windows[0]):
        bpy.ops.wm.quit_blender()


def run(report: Path, package: Path, port: int) -> None:
    """Register the steps as a timer on the first window that survives the document step's file read."""
    generator = steps(bpy.context.window_manager.windows[0], report, package, port)
    bpy.app.timers.register(partial(next, generator, None), first_interval=0.5, persistent=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["close", "run"]
